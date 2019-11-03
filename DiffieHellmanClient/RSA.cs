using System;
using System.Collections.Generic;
using System.Numerics;
using static Prime_number_generator.Generator;

namespace DiffieHellmanClient
{
    class RSA : ICrypter
    {
        public RSA(P2PClient server)
        {
            this.server = server ?? throw new ArgumentNullException();
        }

        private readonly P2PClient server;

        private const int COUNT_BITS = 50;

        private readonly Dictionary<ulong, PSKeys> users = new Dictionary<ulong, PSKeys>(); 

        public void AddUser(ulong client)
        {
            BigInteger p = GenerateRandomPrime(COUNT_BITS);
            BigInteger q = GenerateRandomPrime(COUNT_BITS);
            BigInteger n = p * q;
            BigInteger ph = (p - 1) * (q - 1);
            BigInteger e = GetE(ph);
            BigInteger d = BigInteger.ModPow(e, -1, ph);
            BigInteger publicKey = n + e << (n.GetByteCount() * 8);
            server.Write(client, publicKey.ToByteArray());
            BigInteger secretKey = new BigInteger(Read(client));

        }

		private byte[] Read(ulong client)
		{
			throw new NotImplementedException();
		}

		private BigInteger GetE(BigInteger ph)
        {
            BigInteger output;
            do
            {
                output = GenerateRandomBits(COUNT_BITS / 2) | 1;
                output |= BigInteger.One << (COUNT_BITS / 2 - 1);
            } while (!IsCoprime(output, ph));
            return output;
        }

        public static bool IsCoprime(BigInteger a, BigInteger b)
            => a == b
            ? a == 1
            : a > b 
                ? IsCoprime(a - b, b)
                : IsCoprime(b - a, a);

        public Memory<byte> Decrypt(ulong client, Memory<byte> msg)
        {
            throw new NotImplementedException();
        }

        public Memory<byte> Encrypt(ulong client, Memory<byte> msg)
        {
            throw new NotImplementedException();
        }

        public bool IsConnectionSafe(ulong client, Memory<byte> message)
        {
            throw new NotImplementedException();
        }

        private struct PSKeys
        {
            public BigInteger e, n, d;

            public PSKeys(BigInteger e, BigInteger n, BigInteger d)
            {
                this.e = e;
                this.n = n;
                this.d = d;
            }
        }
    }
}
