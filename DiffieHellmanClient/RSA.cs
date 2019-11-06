﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using static Prime_number_generator.Generator;

namespace DiffieHellmanClient
{
    class RSA : ICrypter
    {
        public RSA(P2PClient server, ushort countBits = 2048)
        {
            this.CountBits = countBits;
            UpdatePQED();
            this.server = server ?? throw new ArgumentNullException();
        }

        private readonly P2PClient server;
        /// <summary>
        /// Сколько ждать ответа от собеседника.
        /// </summary>
        public TimeSpan TimeoutConnection { get; set; } = TimeSpan.FromMinutes(4);

        private readonly ushort CountBits;

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
            PQED preLoad = TakePQED();
            local.N = preLoad.P * preLoad.Q;
            local.E = preLoad.E;
            local.D = preLoad.D;
            wr.Write(client, local.E.ToByteArray());
            wr.Write(client, local.N.ToByteArray());
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
                output = GenerateRandomPrime(20, isNeedSleep: true);
            } while(ph % output == 0);
            return output;
        }

        private BigInteger SearchD(BigInteger e, BigInteger ph)
        { // http://altaev-aa.narod.ru/security/Rsa.html http://altaev-aa.narod.ru/security/images/im7.png
            BigInteger output = BigInteger.Zero;
            Parallel.For(0, Environment.ProcessorCount, (i) =>
            {
                BigInteger Denominator = e;
                BigInteger r = ph * Environment.ProcessorCount;
                BigInteger Numerator = 1 + i * r;
                do
                {
                    Numerator += r;
                    Thread.Sleep(0);
                } while(Numerator % Denominator != 0 && output.IsZero);
                if(output.IsZero)
                    output = Numerator / Denominator;
            });
            return output;
        }

        public Memory<byte> Decrypt(ulong client, Memory<byte> msg)
        {
            if(users.TryGetValue(client, out var localRemote))
            {
                List<byte> output = new List<byte>(msg.Length);
                Int32 i = 0;
                do
                {
                    Int32 size = BitConverter.ToInt32(msg.Span.Slice(i, 4));
                    i += 4;
                    BigInteger c = new BigInteger(msg.Span.Slice(i, size), isUnsigned: true);
                    if(c > localRemote.Item1.N)
                        throw new Exception($"Message too big! c >= N! ({c} >= {localRemote.Item1.N})");
                    Memory<byte> toAdd = BigInteger.ModPow(c, localRemote.Item1.D, localRemote.Item1.N).ToByteArray();
                    output.AddRange(toAdd.Span.Slice(1, toAdd.Span[^1] == 0 ? toAdd.Length - 2 : toAdd.Length - 1).ToArray());
                    i += size;
                } while(i < msg.Length);
                return output.ToArray();
            }
            else throw new Exception("Connection not safe!");
        }

        private Random ran = new Random();

        public Memory<byte> Encrypt(ulong client, Memory<byte> msg)
        {
            if(users.TryGetValue(client, out var localRemote))
            {
                if(localRemote.Item2.N.GetByteCount() <= 2)
                    throw new Exception("Слишком маленькое N.");
                List<byte> output = new List<byte>(msg.Length);
                Int32 oldI;
                Int32 i = 0;
                do
                {
                    oldI = i;
                    i += localRemote.Item2.N.GetByteCount() - 2;
                    if(i > msg.Length)
                        i = msg.Length;
                    BigInteger m = new BigInteger(msg.Span.Slice(oldI, i - oldI), isUnsigned: true);
                    if(m > localRemote.Item2.N)
                        throw new Exception($"Message too big! m >= N! ({m} >= {localRemote.Item2.N})");
                    m = (m << 8) | ran.Next(byte.MinValue, byte.MaxValue);
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
        private readonly object sync = new object();

        private void UpdatePQED()
        {
            Task.Run(() =>
            {
                PQED insert = default;
                lock(sync)
                {
                    while(Prepared.Count < 2)
                    {
                        Parallel.Invoke(
                            () => insert.P = GenerateRandomPrime(CountBits, isNeedSleep: true),
                            () => insert.Q = GenerateRandomPrime(CountBits, isNeedSleep: true)
                        );
                        BigInteger ph = (insert.P - 1) * (insert.Q - 1);
                        insert.E = GetE(ph);
                        insert.D = SearchD(insert.E, ph);
                        Prepared.Add(insert);
                    }
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
