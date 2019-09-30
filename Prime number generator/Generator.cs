using System;
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
        private static IList<T> GeneratePrimesSieveEratosthenes<T>(T maximum) where T : struct, IComparable
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
            }
            return new List<T>(from n in output where n.Value == FlagType.Prime select n.Key);
        }

        private static Random ran = new Random();

        private static BigInteger GenerateRandomBits(ulong countBits)
        {
            byte[] output = new byte[countBits / 8 + countBits % 8 == 0 ? 0 : 1];
            ran.NextBytes(output);
            output[^1] >>= (byte)(countBits % 8);
            output[0] |= 1;
            output[^1] |= (byte)(1 << (byte)(countBits % 8));
            return new BigInteger(output);
        }

        public static BigInteger GenerateRandomPrime(ulong countBits)
        {
            BigInteger output;
            do
            {
                output = GenerateRandomBits(countBits);
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

        private static bool MillerRabinPrimalityTest(BigInteger toTest)
        {
            throw new NotImplementedException();
        }

        private static BigInteger GetCountDivByTwo(BigInteger bigInteger)
        {
            if (bigInteger == 0)
                return 0;
            BigInteger output = 0;
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
