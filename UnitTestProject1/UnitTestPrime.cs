using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Prime_number_generator;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestPrime
    {
        [TestMethod]
        public void GeneratePrimesSieveEratosthenesTest()
        {
            List<ushort> toBe = new List<ushort>() { 2, 3, 5, 7, 11, 13 };
            List<ushort> notToBe = new List<ushort>() { 0, 1, 4, 6, 8, 9, 10, 12 };
            CollectionAssert.IsSubsetOf(toBe, Generator.primes);
            foreach (ushort n in notToBe)
                CollectionAssert.DoesNotContain(Generator.primes, n);
        }
    }
}
