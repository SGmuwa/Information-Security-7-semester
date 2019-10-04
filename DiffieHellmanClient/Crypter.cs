using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using System.Threading.Tasks;
using Prime_number_generator;

namespace DiffieHellmanClient
{
    public class Crypter : ICrypter
    {
        private readonly P2PClient server;

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
            Task<BigInteger> a_task = Task.Run(() => Generator.GenerateRandomPrime(256));
            do {
                arrangement = (byte)ran.Next(0, 255); // Договорённость.
                buffer = new Memory<byte>(new byte[2]);
                client.GetStream().Write(new byte[] { arrangement });
                if (client.GetStream().Read(buffer.Span) > 1)
                    throw new Exception("Не совпадает протокол. " + string.Join(", ", buffer));
            } while (arrangement == buffer.Span[0]);
            BigInteger p;
            BigInteger g;
            if (arrangement > buffer.Span[0])
            {
                p = Generator.GenerateRandomPrime(1024);
                g = AntiderivativeRootModulo(p);
            }
            else
            {
                buffer = new Memory<byte>(new byte[1024 / 8]);
                if(client.GetStream().Read(buffer.Span) != 1024 / 8)
                    throw new Exception("Не совпадает протокол. " + string.Join(", ", buffer));
                p = new BigInteger(buffer.ToArray());
                int size = client.GetStream().Read(buffer.Span);
                g = new BigInteger(buffer.Span.Slice(0, size).ToArray());
            }
            a_task.Wait();
            a = a_task.Result;
            BigInteger A = BigInteger.ModPow(g, a, p);
            byte[] A_array = A.ToByteArray();
            client.GetStream().Write(A_array, 0, A_array.Length);
            byte[] B_array = new byte[A_array.Length * 2];
            Span<byte> B_span = new Span<byte>(B_array).Slice(0, client.GetStream().Read(B_array));
            BigInteger B = new BigInteger(B_span.ToArray());
            BigInteger K = BigInteger.ModPow(B, a, p);
            users[client] = new Memory<byte>(K.ToByteArray());
        }

        public Memory<byte> Decrypt(TcpClient client, Memory<byte> msg)
        {
            if (!users.ContainsKey(client))
                throw new Exception("Connection not safe.");
            throw new NotImplementedException();
        }

        public Memory<byte> Encrypt(TcpClient client, Memory<byte> msg)
        {
            throw new NotImplementedException();
        }

        public bool IsConnectionSafe(TcpClient client)
            => users.ContainsKey(client);

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
                    ok &= Powmod(res, phi / fact[i], p) != 1;
                if (ok) return res;
            }
            return -1;

            static BigInteger Powmod(BigInteger a, BigInteger b, BigInteger p)
            {
                BigInteger res = 1;
                while (b != 0)
                    if ((b & 1) != 0)
                    {
                        res = (int)(res * a % p);
                        --b;
                    }
                    else
                    {
                        a = (int)(a * 1L * a % p);
                        b >>= 1;
                    }
                return res;
            }
        }
    }
}