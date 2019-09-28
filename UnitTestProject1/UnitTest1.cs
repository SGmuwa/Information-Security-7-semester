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

        private StreamReader CreateStream(string data)
        {
            MemoryStream output = new MemoryStream();
            StreamWriter writer;
            writer = new StreamWriter(output);
            writer.Write(data);
            writer.Flush();
            output.Position = 0;
            return new StreamReader(output);
        }

        [DataTestMethod]
        [DataRow("рсс", "стт", 1)]
        [DataRow("стт", "рсс", -1)]
        public void Encrypt(string input, string expect, int key)
        {
            Assert.AreEqual(expect, Encryption.Encrypt(input, key));
        }

        [DataTestMethod]
        [DataRow("рсс", "рсс", 1)]
        [DataRow("стт", "стт", -1, "стт")]
        public void Decrypt(string input, string expect, int key, string sample)
        {
            CharacterFrequencyAnalyzer an = new CharacterFrequencyAnalyzer(sample);
            Assert.AreEqual(expect, an.Decrypt(Encryption.Encrypt(input, key)));
        }
    }
}
