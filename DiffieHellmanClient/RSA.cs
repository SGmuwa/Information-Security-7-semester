using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using static Prime_number_generator.Generator;

namespace DiffieHellmanClient
{
    class RSA : ICrypter
    {
        public RSA(P2PClient server)
        {
            UpdatePrime();
            this.server = server ?? throw new ArgumentNullException();
        }

        private readonly P2PClient server;
        /// <summary>
        /// Сколько ждать ответа от собеседника.
        /// </summary>
        public TimeSpan TimeoutConnection { get; set; } = TimeSpan.FromMinutes(4);

        private const int COUNT_BITS = 80;

        /// <summary>
        /// Таблица ключей. Соответствие идентификатора пользователя с классом...
        /// ... Tuple (Наш открытый и секретный, Чужой открытый ключ).
        /// </summary>
        private readonly Dictionary<ulong, (PSKey, PSKey)> users
            = new Dictionary<ulong, (PSKey, PSKey)>(); 

        public void AddUser(ulong client)
        {
            using SimpleWriterReader wr = new SimpleWriterReader(server, TimeoutConnection);
            PSKey local = default;
            BigInteger p = TakePrime();
            BigInteger q = TakePrime();
            local.N = p * q;
            BigInteger ph = (p - 1) * (q - 1);
            local.E = GetE(ph);
            local.D = GreatestCommonDivisor(local.E, ph); // https://neerc.ifmo.ru/wiki/index.php?title=RSA Число d называется секретной экспонентой. Обычно, оно вычисляется при помощи расширенного алгоритма Евклида.
            server.Write(client, local.E.ToByteArray());
            server.Write(client, local.N.ToByteArray());
            PSKey remote = default;
            remote.E = new BigInteger(wr.Read(client).ToArray(), isUnsigned: true);
            remote.N = new BigInteger(wr.Read(client).ToArray(), isUnsigned: true);
            users.Add(client, (local, remote));
        }

        private BigInteger GetE(BigInteger ph)
        {
            BigInteger output;
            do
            {
                output = TakePrime();
            } while(ph % output == 0);
            return output;
        }

        private BigInteger GreatestCommonDivisor(BigInteger a, BigInteger b)
        {
            while (a != 0 && b != 0)
                if (a > b)
                    a %= b;
                else
                    b %= a;
            return a == 0 ? b : a;
        }

        public Memory<byte> Decrypt(ulong client, Memory<byte> msg)
        {
            if(users.TryGetValue(client, out var localRemote))
            {
                List<byte> output = new List<byte>(msg.Length);
                int i = 0;
                do
                {
                    int size = BitConverter.ToInt32(msg.Span.Slice(i, 4)); i += 4;
                    BigInteger c = new BigInteger(msg.Span.Slice(i, size), isUnsigned: true);
                    if(c > localRemote.Item1.N)
                        throw new Exception($"Message too big! c >= N! ({c} >= {localRemote.Item1.N})");
                    output.AddRange(BigInteger.ModPow(c, localRemote.Item1.D, localRemote.Item1.N).ToByteArray());
                    i += size;
                } while(i < msg.Length);
                return output.ToArray();
            }
            else throw new Exception("Connection not safe!");
        }

        public Memory<byte> Encrypt(ulong client, Memory<byte> msg)
        {
            if(users.TryGetValue(client, out var localRemote))
            {
                List<byte> output = new List<byte>(msg.Length);
                int oldI;
                int i = 0;
                do
                {
                    oldI = i;
                    i += localRemote.Item2.N.GetByteCount() - 1;
                    if(i > msg.Length)
                        i = msg.Length;
                    BigInteger m = new BigInteger(msg.Span.Slice(oldI, i - oldI), isUnsigned: true);
                    if(m > localRemote.Item2.N)
                        throw new Exception($"Message too big! m >= N! ({m} >= {localRemote.Item2.N})");
                    byte[] result = BigInteger.ModPow(m, localRemote.Item2.E, localRemote.Item2.N).ToByteArray();
                    output.AddRange(BitConverter.GetBytes(result.Length));
                    output.AddRange(result);
                } while(i < msg.Length);
                return output.ToArray();
            }
            else throw new Exception("Connection not safe!");
        }

		public bool IsConnectionSafe(ulong client, Memory<byte> message)
            => users.ContainsKey(client);

        /// <summary>
        /// Коллекция, в которой хранятся простые числа.
        /// </summary>
        private readonly BlockingCollection<BigInteger> Prepared = new BlockingCollection<BigInteger>();

        private void UpdatePrime()
        {
            Task.Run(() =>
            {
                BigInteger insert;
                while(Prepared.Count < 9)
                {
                    insert = GenerateRandomPrime(COUNT_BITS, isNeedSleep: Prepared.Count != 0);
                    Prepared.Add(insert);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Получает готовое простое число.
        /// </summary>
        private BigInteger TakePrime()
        {
            UpdatePrime();
            return Prepared.Take();
        }

        private struct PSKey
        {
            public BigInteger E { get; set; }
            public BigInteger N { get; set; }

            public BigInteger D { get; set;}

            public BigInteger PublicKey => E + (N << E.GetByteCount());
            public BigInteger SecretKey => D + (N << D.GetByteCount());

            public PSKey(BigInteger e, BigInteger n, BigInteger d)
            {
                E = e;
                N = n;
                D = d;
            }
        }
    }
}
