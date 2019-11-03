﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Prime_number_generator;

namespace DiffieHellmanClient
{
    public class Crypter : ICrypter
    {
        private readonly P2PClient server;

        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(2);

        private const ushort COUNT_PRIME_BITS = 64; // Рекомендуется 1024

        public Crypter(P2PClient server)
        {
            UpdatePG();
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
        private readonly ConcurrentDictionary<ulong, Memory<byte>> users = new ConcurrentDictionary<ulong, Memory<byte>>();

        /// <summary>
        /// База сообщений для каждого пользователя.
        /// </summary>
        private readonly ConcurrentDictionary<ulong, BlockingCollection<Memory<byte>>> Messages = new ConcurrentDictionary<ulong, BlockingCollection<Memory<byte>>>();

        /// <summary>
        /// Вычисления, которые сделаны заранее.
        /// </summary>
        private readonly BlockingCollection<PG> Prepared = new BlockingCollection<PG>();

        private readonly Stopwatch sw = new Stopwatch();

        public void AddUser(ulong client)
        {
            // Договариваемся, кто генерирует p и g.
            byte arrangement;
            Memory<byte> buffer;
            BigInteger a;
            Task<BigInteger> a_task = Task.Run(() => Generator.GenerateRandomPrime(COUNT_PRIME_BITS / 4));
            do {
                arrangement = (byte)ran.Next(byte.MinValue, byte.MaxValue); // Договорённость.
                server.Write(client, new Memory<byte>(new byte[] { arrangement }));
                buffer = Read(client);
                if (buffer.Length > 1)
                    throw new Exception("Не совпадает протокол. " + string.Join(", ", buffer));
            } while (arrangement == buffer.Span[0]);
            BigInteger p;
            BigInteger g = -2;
            if (arrangement > buffer.Span[0])
            {
                sw.Start();
                PG pg = TakePrepared();
                sw.Stop();
                server.DebugInfo($"GenerateRandomPrime and AntiderivativeRootModulo: {sw.Elapsed}.");
                sw.Reset();
                p = pg.P;
                g = pg.G;
                server.Write(client, p.ToByteArray());
                server.Write(client, g.ToByteArray());
            }
            else
            {
                p = new BigInteger(Read(client).ToArray());
                g = new BigInteger(Read(client).ToArray());
            }
            a_task.Wait();
            a = a_task.Result;
            BigInteger A = BigInteger.ModPow(g, a, p);
            server.Write(client, A.ToByteArray());
            BigInteger B = new BigInteger(Read(client).ToArray());
            BigInteger K = BigInteger.ModPow(B, a, p);
            users[client] = new Memory<byte>(K.ToByteArray());
        }

        public Memory<byte> Decrypt(ulong client, Memory<byte> msg)
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

        public Memory<byte> Encrypt(ulong client, Memory<byte> msg)
            => Decrypt(client, msg);

        public bool IsConnectionSafe(ulong client, Memory<byte> message)
        {
            if (users.ContainsKey(client))
                return true;
            BlockingCollection<Memory<byte>> messagesUser = Messages.GetOrAdd(client, _ => new BlockingCollection<Memory<byte>>());
            messagesUser.Add(message);
            return false;
        }

        private Memory<byte> Read(ulong client)
        {
            using CancellationTokenSource tokenSource = new CancellationTokenSource(timeout);
            BlockingCollection<Memory<byte>> messages = Messages.GetOrAdd(client, _ => new BlockingCollection<Memory<byte>>());
            return messages.Take(tokenSource.Token);
        }

        private static IEnumerable<T> InfinityRepeat<T>(Memory<T> toRepeat)
        {
            if (toRepeat.Length == 0)
                throw new Exception("Нельзя создать бесконечность из пустоты.");
            while (true)
                for (int i = 0; i < toRepeat.Length; i++)
                    yield return toRepeat.Span[i];
        }

        private void OnClientDisconnect(P2PClient server, ulong client)
        {
            users.TryRemove(client, out _);
            Messages.TryRemove(client, out _);
        }

        private PG TakePrepared()
        {
            UpdatePG();
            return Prepared.Take();
        }

        private void UpdatePG()
        {
            Task.Run(() =>
            {
                PG insert;
                while(Prepared.Count < 3)
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource(timeout / 8);
                    try
                    {
                        insert.P = Generator.GenerateRandomPrime(COUNT_PRIME_BITS, tokenSource.Token, Prepared.Count != 0);
                        insert.G = AntiderivativeRootModulo(insert.P, tokenSource.Token, Prepared.Count != 0);
                    }
                    catch
                    {
                        tokenSource.Dispose();
                        continue;
                    }
                    tokenSource.Dispose();
                    Prepared.Add(insert);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Ищет первообразный корень по модулю.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <see cref="http://e-maxx.ru/algo/export_primitive_root"/>
        private static BigInteger AntiderivativeRootModulo(BigInteger p, CancellationToken token = default, bool isNeedSleep = false)
        {
            List<BigInteger> fact = new List<BigInteger>(COUNT_PRIME_BITS / 4);
            BigInteger phi = p - 1, n = phi, nsqrt = n.Sqrt();
            Parallel.For(0, Environment.ProcessorCount, new ParallelOptions() { CancellationToken = token }, _ =>
            {
                for (BigInteger i = 2 + _; i <= nsqrt; i += Environment.ProcessorCount)
                {
                    if (n % i == 0)
                    {
                        lock (fact)
                        {
                            fact.Add(i);
                            do n /= i; while (n % i == 0);
                        }
                        nsqrt = n.Sqrt();
                    }
                    if (isNeedSleep)
                        Thread.Sleep(0);
                    token.ThrowIfCancellationRequested();
                }
            });
            if (n > 1)
                fact.Add(n);

            for (BigInteger res = 2; res <= p; ++res)
            {
                bool ok = true;
                for (int i = 0; i < fact.Count && ok; i++)
                    ok &= BigInteger.ModPow(res, phi / fact[i], p) != 1;
                if (ok) return res;
                if (isNeedSleep)
                    Thread.Sleep(1);
            }
            return -1;
        }

        private struct PG
        {
            public BigInteger P;
            public BigInteger G;

            public PG(BigInteger P, BigInteger G)
            {
                this.P = P;
                this.G = G;
            }
        }

    }
}