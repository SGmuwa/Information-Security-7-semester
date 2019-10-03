using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using Prime_number_generator;

namespace DiffieHellmanClient
{
    public class Crypter : ICrypter
    {
        private Random ran = new Random();

        private ConcurrentDictionary<TcpClient, dynamic> users = new ConcurrentDictionary<TcpClient, dynamic>();

        public void AddUser(TcpClient client)
        {
            BigInteger a = Generator.GenerateRandomPrime(256);
            BigInteger p = Generator.GenerateRandomPrime(1024);
            BigInteger g = AntiderivativeRootModulo(p);
            client.GetStream().Write();
        }

        public Memory<byte> Decrypt(TcpClient client, Memory<byte> msg)
        {
            throw new NotImplementedException();
        }

        public Memory<byte> Encrypt(TcpClient client, Memory<byte> msg)
        {
            throw new NotImplementedException();
        }

        public bool IsConnectionSafe(TcpClient client)
        {
            throw new NotImplementedException();
        }

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