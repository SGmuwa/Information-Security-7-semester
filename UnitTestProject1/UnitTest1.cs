using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caesar_s_code;
using System.IO;
using System;
using System.Text;
using System.Linq;
using static Caesar_s_code.LettersSupportProvider.TypeLettersSupport;
using static Caesar_s_code.LettersSupportProvider;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [DataTestMethod]
        [DataRow("рсс", "стт", 1)]
        [DataRow("стт", "рсс", -1)]
        public void Encrypt(string input, string expect, int key)
        {
            Assert.AreEqual(expect, Encryption.Encrypt(input, key));
        }

        [DataTestMethod]
        [DataRow("рсс", 1, "рсс")]
        [DataRow("стт", -1, "стт")]
        [DataRow("рсс", 3, "рсс")]
        [DataRow("рсс", 3, "ррссс")]
        [DataRow("џсс", 1, "ррсссџ")]
        public void Decrypt(string expect, int key, string sample)
        {
            CharacterFrequencyAnalyzer an = new CharacterFrequencyAnalyzer(sample);
            Assert.AreEqual(expect, an.Decrypt(Encryption.Encrypt(expect, key)));
        }

        [DataTestMethod]
        [DataRow("Vojna_i_mir__Tom_1_2_3_4", 21314, "Vojna_i_mir__Tom_1_2_3_4", 0.99, get_all)]
        [DataRow("Vojna_i_mir__first_chapter", 413, "Vojna_i_mir__Tom_1_2_3_4", 0.29, get_all)]
        [DataRow("Vojna_i_mir__first_chapter", -124, "Vojna_i_mir__Tom_1_2_3_4", 0.45, russian)]
        public void DecryptFromResurses(string inputName, int key, string sampleName, double accuracy, TypeLettersSupport sup)
        {
            LettersSetSettings(sup);
            string expect = Properties.Resources.ResourceManager.GetString(inputName);
            string sample = Properties.Resources.ResourceManager.GetString(sampleName);
            CharacterFrequencyAnalyzer an = new CharacterFrequencyAnalyzer(sample);
            string dec = an.Decrypt(Encryption.Encrypt(expect, key));
            int errors = 0; int all = 0;
            for (int i = 0; i < dec.Length || i < expect.Length; i++)
            {
                if (i < dec.Length || i < expect.Length)
                {
                    if (LettersSupportProvider.LettersSupport.Contains(expect[i]))
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
