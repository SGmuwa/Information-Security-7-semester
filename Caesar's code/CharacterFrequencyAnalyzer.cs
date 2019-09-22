using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Caesar_s_code
{
    class CharacterFrequencyAnalyzer
    {
        private SortedList<BigInteger, char> TopSample;
        public CharacterFrequencyAnalyzer(StreamReader sample)
        {
            TopSample = Analyze(sample);
        }

        private SortedList<BigInteger, char> Analyze(StreamReader sample)
        {
            Span<char> a = new Span<char>(new char[1024 * 4]);
            Dictionary<char, BigInteger> map = new Dictionary<char, BigInteger>();
            int i = 0;
            while ((i = sample.Read(a)) > 0)
                for (i = i - 1; i != -1; i--)
                    map[a[i]] = map.ContainsKey(a[i]) ? map[a[i]] + 1 : 1;
            SortedList<BigInteger, char> output = new SortedList<BigInteger, char>(map.Count);
            foreach(KeyValuePair<char, BigInteger> pair in map)
            {
                output.Add(pair.Value, pair.Key);
            }
            return output;
        }

        public void DecryptDisponse(StreamWriter output, StreamReader input)
        {
            new Task(() =>
            {
                Decrypt(output, input);
                Console.Out.WriteAsync("d");
                output.Dispose();
                input.Dispose();
            }).Start();
        }

        public void Decrypt(StreamWriter output, StreamReader input)
        {
            if (input.BaseStream.CanSeek == false)
                throw new Exception("Need Seek input.");
            SortedList<BigInteger, char> currentSample = Analyze(input);
            input.BaseStream.Position = 0;
            try
            {
                Span<char> buffer = new Span<char>(new char[1]);
                while (input.Read(buffer) > 0)
                {
                    output.Write(GetDecryptChar(buffer[0], currentSample));
                    Console.Out.WriteAsync('.');
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private char GetDecryptChar(char v, SortedList<BigInteger, char> sample)
        {
            return TopSample[(BigInteger)((double)sample.IndexOfValue(v) / sample.Count * TopSample.Count)];
        }
    }
}
