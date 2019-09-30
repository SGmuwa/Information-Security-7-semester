﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Prime_number_generator
{
    public static class Generator
    {
        public static readonly ReadOnlyCollection<ushort> primes = new ReadOnlyCollection<ushort>(GeneratePrimesSieveEratosthenes<ushort>(2000));

        private enum FlagType
        {
            Unknow,
            Prime,
            NotPrime
        }

        /// <summary>
        /// Генерация простых чисел с помощью решето Эратосфена.
        /// </summary>
        /// <param name="maximum">Граница поиска. Ожидается целое число.</param>
        /// <returns>Список простых чисел до max.</returns>
        public static List<T> GeneratePrimesSieveEratosthenes<T>(T maximum) where T : struct, IComparable
        {
            dynamic max = maximum;
            if (max < 2)
                throw new ArgumentException("max должен быть равен как минимум двойке.");
            
            Dictionary<T, FlagType> output = new Dictionary<T, FlagType>();
            dynamic current = (T)((dynamic)default(T) + 1);
            while(true)
            {
                current++;
                if (current > max)
                    break;
                if (!output.ContainsKey(current))
                    output[current] = FlagType.Unknow;
                if (output[current] == FlagType.Unknow)
                {
                    output[current] = FlagType.Prime;
                    for (dynamic i = (T)(current + current); i <= max && i >= current; i = (T)(i + current))
                    {
                        output[i] = FlagType.NotPrime;
                    }
                }
                else if(output[current] == FlagType.NotPrime)
                    output.Remove(current);
            }
            return new List<T>(from n in output where n.Value == FlagType.Prime select n.Key);
        }

        private static Random ran = new Random();

        private static BigInteger GenerateRandomBits(ulong countBits)
        {
            byte[] output = new byte[countBits / 8 + countBits % 8 == 0 ? 0 : 1];
            ran.NextBytes(output);
            output[^1] >>= (byte)(countBits % 8);
            return new BigInteger(output);
        }

        public static BigInteger Pow(BigInteger a, BigInteger b)
        {
            BigInteger result = BigInteger.One;
            do
            {
                int toPow = (int)(b & int.MaxValue);
                b >>= sizeof(int) - 1;
                if(b.Sign < 0)
                {
                    toPow = -toPow;
                    b = -b;
                }
                result *= BigInteger.Pow(a, toPow);
            } while (b > 1);
            return result;
        }

        private static BigInteger GenerateRandom(BigInteger min, BigInteger max)
        {
            if (min > max)
                throw new ArgumentException();
            BigInteger interval = max - min;
            return (GenerateRandomBits((ulong)interval.ToByteArray().Length * 8) % interval) + min;
        }


        public static BigInteger GenerateRandomPrime(int countBits)
        {
            if (countBits < 2)
                throw new ArgumentException();
            BigInteger output;
            do
            {
                output = GenerateRandomBits((uint)countBits);
                output |= 1;
                output |= BigInteger.One << (countBits - 1);
            } while (IsPrime(output));
            return output;
        }

        private static bool IsPrime(BigInteger output)
        {
            bool result = true; // true - простое. Иначе - false.
            Parallel.ForEach(primes, (n) =>
            {
                if (output % n == 0)
                    result = true;
            });
            if (!result) return false;
            Parallel.For(1, 5, (i) => {
                if (!MillerRabinPrimalityTest(output))
                    result = false;
            });
            return result;
        }

        private static bool MillerRabinPrimalityTest(BigInteger p)
        {
            int b = (int)GetCountDivByTwo(p - 1);
            int m = (int)((p - 1) / BigInteger.Pow(2, b));
            BigInteger a = GenerateRandom(0, p - 1);
            BigInteger j = 0;
            BigInteger z = BigInteger.Pow(a, m) % p;
            if (z == 1 || z == p - 1)
                return true;
            five:
            if (j > 0 && z == 1)
                return false; // Непростое.
            j++;
            if (j < b && z < p - 1)
            {
                z = z * z % p;
                goto five;
            }
            else if (z == p - 1)
                return true;
            if (j == b && z != p - 1)
                return false;
            return true;
        }

        private static uint GetCountDivByTwo(BigInteger bigInteger)
        {
            if (bigInteger == 0)
                return 0;
            uint output = 0;
            while ((bigInteger & 0xFFFFFFFFFFFFFFFF) == 0)
            {
                output += 64;
                bigInteger >>= 64;
            }
            while ((bigInteger & 0xFF) == 0)
            {
                output += 8;
                bigInteger >>= 8;
            }
            while ((bigInteger & 0b1) == 0)
            {
                output++;
                bigInteger >>= 1;
            }
            return output;
        }
    }
}
