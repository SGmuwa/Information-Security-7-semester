using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Prime_number_generator;

namespace DiffieHellmanClient
{
    public class Crypter : ICrypter
    {
        private readonly P2PClient server;

        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(30);

        private const ushort COUNT_PRIME_BITS = 50; // Рекомендуется 1024

        public Crypter(P2PClient server)
        {
            this.server = server;
            server.OnDisconnect += OnClientDisconnect;
        }

        ~Crypter()
        {
            if (server != null)
                server.OnDisconnect -= OnClientDisconnect;
        }

        /// <summary>
        /// Генератор случайных чисел.
        /// </summary>
        private Random ran = new Random();

        /// <summary>
        /// База данных пользователей и их ключей шифрования.
        /// </summary>
        private readonly ConcurrentDictionary<TcpClient, Memory<byte>> users = new ConcurrentDictionary<TcpClient, Memory<byte>>();

        public void AddUser(TcpClient client)
        {
            // Договариваемся, кто генерирует p и g.
            byte arrangement;
            Memory<byte> buffer;
            BigInteger a;
            Task<BigInteger> a_task = Task.Run(() => Generator.GenerateRandomPrime(COUNT_PRIME_BITS / 4));
            do {
                arrangement = (byte)ran.Next(byte.MinValue, byte.MaxValue); // Договорённость.
                server.Write(client, new Memory<byte>(new byte[] { arrangement }));
                buffer = server.Read(client);
                if (buffer.Length > 1)
                    throw new Exception("Не совпадает протокол. " + string.Join(", ", buffer));
            } while (arrangement == buffer.Span[0]);
            BigInteger p;
            BigInteger g;
            if (arrangement > buffer.Span[0])
            {
                p = Generator.GenerateRandomPrime(COUNT_PRIME_BITS);
                server.Write(client, p.ToByteArray());
                g = AntiderivativeRootModulo(p);
                server.Write(client, g.ToByteArray());
            }
            else
            {
                p = new BigInteger(server.Read(client).ToArray());
                g = new BigInteger(server.Read(client).ToArray());
            }
            a_task.Wait();
            a = a_task.Result;
            BigInteger A = BigInteger.ModPow(g, a, p);
            server.Write(client, A.ToByteArray());
            BigInteger B = new BigInteger(server.Read(client).ToArray());
            BigInteger K = BigInteger.ModPow(B, a, p);
            users[client] = new Memory<byte>(K.ToByteArray());
        }

        public Memory<byte> Decrypt(TcpClient client, Memory<byte> msg)
        {
            if (!users.ContainsKey(client))
                throw new Exception("Connection not safe.");
            IEnumerator<byte> key = InfinityRepeat(users[client]).GetEnumerator();
            Memory<byte> output = new Memory<byte>((byte[])msg.ToArray().Clone());
            foreach(ref byte b in output.Span)
            {
                key.MoveNext();
                b ^= key.Current;
            }
            return output;
        }

        public Memory<byte> Encrypt(TcpClient client, Memory<byte> msg)
            => Decrypt(client, msg);

        public bool IsConnectionSafe(TcpClient client)
            => users.ContainsKey(client);

        private static IEnumerable<T> InfinityRepeat<T>(Memory<T> toRepeat)
        {
            if (toRepeat.Length == 0)
                throw new Exception("Нельзя создать бесконечность из пустоты.");
            while (true)
                for (int i = 0; i < toRepeat.Length; i++)
                    yield return toRepeat.Span[i];
        }

        private void OnClientDisconnect(P2PClient server, TcpClient client)
            => users.TryRemove(client, out _);


        /// <summary>
        /// Ищет первообразный корень по модулю.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <see cref="http://e-maxx.ru/algo/export_primitive_root"/>
        private static BigInteger AntiderivativeRootModulo(BigInteger p)
        {
            List<BigInteger> fact = new List<BigInteger>();
            BigInteger phi = p - 1, n = phi;
            for (BigInteger i = 2; i * i <= n; ++i)
                if (n % i == 0)
                {
                    fact.Add(i);
                    while (n % i == 0)
                        n /= i;
                }
            if (n > 1)
                fact.Add(n);

            for (BigInteger res = 2; res <= p; ++res)
            {
                bool ok = true;
                for (int i = 0; i < fact.Count && ok; i++)
                    ok &= BigInteger.ModPow(res, phi / fact[i], p) != 1;
                if (ok) return res;
            }
            return -1;
        }
    }
}