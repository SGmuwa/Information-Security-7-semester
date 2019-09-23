using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caesar_s_code;
using System.IO;
using System;
using System.Text;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public enum Enc
        {
            ASCII,
            UTF8
        }

        private Encoding GetEncoding(Enc enc)
        {
            switch (enc)
            {
                case Enc.ASCII:
                    return Encoding.ASCII;
                case Enc.UTF8:
                    return Encoding.UTF8;
                default:
                    throw new NotSupportedException();
            }
        }

        [DataTestMethod]
        [DataRow("рсс", "стт", Enc.ASCII)]
        public void TestMethod1(string input, string expect, Enc enc)
        {
            Encoding encoding = GetEncoding(enc);
            MemoryStream ToEncrypt = new MemoryStream();
            new StreamWriter(ToEncrypt, encoding).WriteAsync(input);
            MemoryStream Expect = new MemoryStream();
            new StreamWriter(Expect, encoding).WriteAsync(expect);
            MemoryStream OutputEncrypt = new MemoryStream();
            Encryption.Encrypt(OutputEncrypt, ToEncrypt, 1);
            CollectionAssert.AreEquivalent(Expect.ToArray(), OutputEncrypt.ToArray());
            Expect.Position = 0; OutputEncrypt.Position = 0;
            string ExpectStr = new StreamReader(Expect, encoding).ReadToEnd();
            string OutputEncryptStr = new StreamReader(OutputEncrypt, encoding).ReadToEnd();
            Assert.AreEqual(ExpectStr, OutputEncryptStr);
            Assert.AreEqual(ExpectStr.Length, expect.Length);
            Assert.AreEqual(OutputEncryptStr.Length, expect.Length);
            Console.Out.WriteLineAsync($"{ExpectStr} : {OutputEncryptStr}");
        }
    }
}
