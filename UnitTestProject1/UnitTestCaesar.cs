using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caesar_s_code;
using System;
using static Caesar_s_code.LettersSupportProvider.TypeLettersSupport;
using static Caesar_s_code.LettersSupportProvider;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTestCaesar
    {
        [DataTestMethod]
        [DataRow("абб", "бвв", 1)]
        [DataRow("бвв", "абб", -1)]
        public void Encrypt(string input, string expect, int key)
        {
            Assert.AreEqual(expect, Encryption.Encrypt(input, key));
        }

        [DataTestMethod]
        [DataRow("абб", 1, "абб")]
        [DataRow("бвв", -1, "бвв")]
        [DataRow("абб", 3, "абб")]
        [DataRow("абб", 3, "ааббб")]
        [DataRow("ябб", 1, "аабббя")]
        public void Decrypt(string expect, int key, string sample)
        {
            CharacterFrequencyAnalyzer an = new CharacterFrequencyAnalyzer(sample);
            Assert.AreEqual(expect, an.Decrypt(Encryption.Encrypt(expect, key)));
        }

        [DataTestMethod]
        [DataRow("Vojna_i_mir__Tom_1_2_3_4", 21314, "Vojna_i_mir__Tom_1_2_3_4", 0.995, (int)get_all)]
        [DataRow("Vojna_i_mir__first_chapter", 413, "Vojna_i_mir__Tom_1_2_3_4", 0.28, (int)get_all)]
        [DataRow("Vojna_i_mir__first_chapter", -124, "Vojna_i_mir__Tom_1_2_3_4", 0.45, (int)russian)]
        [DataRow("Vojna_i_mir__first_chapter", 32, "Vojna_i_mir__first_chapter", 0.993, (int)get_all)]
        public void DecryptFromResources(string inputName, int key, string sampleName, double accuracy, dynamic sup)
        {
            LettersSetSettings((TypeLettersSupport)sup);
            string expect = Properties.Resources.ResourceManager.GetString(inputName);
            string sample = Properties.Resources.ResourceManager.GetString(sampleName);
            CharacterFrequencyAnalyzer an = new CharacterFrequencyAnalyzer(sample);
            string dec = an.Decrypt(Encryption.Encrypt(expect, key));
            int errors = 0; int all = 0;
            for (int i = 0; i < dec.Length || i < expect.Length; i++)
            {
                if (i < dec.Length || i < expect.Length)
                {
                    if (LettersSupport.Contains(expect[i]))
                    {
                        all++;
                        if (dec[i] != expect[i])
                            errors++;
                    }
                }
                else
                {
                    Console.Write($"{i}:{(i < dec.Length ? dec[i] : expect[i])}");
                }
            }
            Console.WriteLine();
            double accuracyCurrent = 1 - (double)errors/all;
            Console.WriteLine("accuracy: " + accuracyCurrent);
            Console.WriteLine("dec: " + dec);
            Assert.IsTrue(accuracyCurrent > accuracy);
        }
    }
}
