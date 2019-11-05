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
            UpdatePQED();
            this.server = server ?? throw new ArgumentNullException();
        }

        private readonly P2PClient server;
        /// <summary>
        /// Сколько ждать ответа от собеседника.
        /// </summary>
        public TimeSpan TimeoutConnection { get; set; } = TimeSpan.FromMinutes(4);

        private const int COUNT_BITS = 16;

        /// <summary>
        /// Таблица ключей. Соответствие идентификатора пользователя с классом...
        /// ... Tuple (Наш открытый и секретный, Чужой открытый ключ).
        /// </summary>
        private readonly Dictionary<ulong, (PSKey, PSKey)> users
            = new Dictionary<ulong, (PSKey, PSKey)>(); 

        public void AddUser(ulong client)
        {
            client = 1;
            using SimpleWriterReader wr = new SimpleWriterReader(server, TimeoutConnection);
            PSKey local = default;
            PQED preLoad = TakePQED();
            local.N = preLoad.P * preLoad.Q;
            local.E = preLoad.E;
            local.D = preLoad.D;
            //wr.Write(client, local.E.ToByteArray());
            //wr.Write(client, local.N.ToByteArray());
            PSKey remote = local;
            //remote.E = new BigInteger(wr.Read(client).ToArray(), isUnsigned: true);
            //remote.N = new BigInteger(wr.Read(client).ToArray(), isUnsigned: true);
            users.Add(client, (local, remote));
            // return;
            Memory<byte> start = new byte[]{3};
            Memory<byte> encrypted = Encrypt(1, start);
            Memory<byte> decrypted = Decrypt(1, encrypted);
            string decryptedText = System.Text.Encoding.UTF8.GetString(decrypted.Span);
            Console.WriteLine();
        }

        private BigInteger GetE(BigInteger ph)
        {
            BigInteger output;
            do
            {
                output = GenerateRandomPrime(COUNT_BITS / 4, isNeedSleep: true);
            } while(ph % output == 0);
            return output;
        }

        private BigInteger SearchD(BigInteger e, BigInteger ph)
        { // http://altaev-aa.narod.ru/security/Rsa.html http://altaev-aa.narod.ru/security/images/im7.png
            BigInteger Numerator = 1;
            BigInteger Denominator = e;
            BigInteger r = ph%e;
            do
            {
                Numerator += r;
            } while(Numerator % Denominator != 0);
            return Numerator / Denominator;
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
                    #warning
                    // if(c > localRemote.Item1.N)
                    //     throw new Exception($"Message too big! c >= N! ({c} >= {localRemote.Item1.N})");
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
                    i += 1;
                    if(i > msg.Length)
                        i = msg.Length;
                    BigInteger m = new BigInteger(msg.Span.Slice(oldI, i - oldI), isUnsigned: true);
                    #warning
                    // if(m > localRemote.Item2.N)
                    //     throw new Exception($"Message too big! m >= N! ({m} >= {localRemote.Item2.N})");
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
        /// Коллекция, в которой хранятся готовые подготовленные числа.
        /// </summary>
        private readonly BlockingCollection<PQED> Prepared = new BlockingCollection<PQED>();

        private void UpdatePQED()
        {
            Task.Run(() =>
            {
                PQED insert;
                while(Prepared.Count < 2)
                {
                    insert.P = 3;//GenerateRandomPrime(COUNT_BITS, isNeedSleep: true);
                    insert.Q = 11;//GenerateRandomPrime(COUNT_BITS, isNeedSleep: true);
                    BigInteger ph = (insert.P - 1) * (insert.Q - 1);
                    insert.E = GetE(ph);
                    insert.D = SearchD(insert.E, ph);
                    BigInteger N = insert.P * insert.Q;
                    if(BigInteger.ModPow(
                        BigInteger.ModPow(3, insert.E, N),
                        insert.D,
                        N) != 3)
                        Console.WriteLine();
                    Prepared.Add(insert);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Получает готовое простое число.
        /// </summary>
        private PQED TakePQED()
        {
            UpdatePQED();
            return Prepared.Take();
        }

        private struct PQED
        {
            public BigInteger P, Q, E, D;
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
