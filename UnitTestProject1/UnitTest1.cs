using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caesar_s_code;
using System.IO;
using System;
using System.Text;
using System.Linq;

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

        private MemoryStream CreateStream(string data, Encoding encoding)
        {
            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, encoding);
            writer.Write(data);
            writer.Flush();
            output.Position = 0;
            return output;
        }

        [DataTestMethod]
        [DataRow("рсс", new byte[] { 240, 188, 192, 209, 177, 209, 178, 209, 178 }, Enc.UTF8, (byte)1)]
        public void TestMethod1(string input, byte[] expect, Enc enc, byte key)
        {
            Encoding encoding = GetEncoding(enc);
            string encr = GetEncryptWithTests(input, expect, encoding, key);
            Assert.AreEqual(input, GetEncryptWithTests(encr, encoding.GetBytes(input), encoding, (byte)-key));
        }

        public string GetEncryptWithTests(string input, byte[] expect, Encoding encoding, byte key)
        {
            Console.WriteLine(string.Join(", ", from info in Encoding.GetEncodings() select info.Name));
            using (MemoryStream ToEncrypt = CreateStream(input, encoding))
                using (MemoryStream Expect = new MemoryStream(expect))
                    using (MemoryStream OutputEncrypt = new MemoryStream())
                    {

                        Encryption.Encrypt(OutputEncrypt, ToEncrypt, key);

                        Console.WriteLine("OutputEncrypt: " + string.Join(", ", OutputEncrypt.ToArray()));
                        Console.WriteLine("Expect: " + string.Join(", ", Expect.ToArray()));
                        CollectionAssert.AreEquivalent(Expect.ToArray(), OutputEncrypt.ToArray());
                        Expect.Position = 0; OutputEncrypt.Position = 0;
                        string ExpectStr = new StreamReader(Expect, encoding).ReadToEnd();
                        string OutputEncryptStr = new StreamReader(OutputEncrypt, encoding).ReadToEnd();
                        Assert.AreEqual(ExpectStr, OutputEncryptStr);
                        Assert.AreEqual(ExpectStr.Length, encoding.GetString(expect).Length);
                        Assert.AreEqual(OutputEncryptStr.Length, encoding.GetString(expect).Length);
                        Console.Out.WriteLineAsync($"{ExpectStr} : {OutputEncryptStr}");
                        return OutputEncryptStr;
                    }
        }
    }
}
