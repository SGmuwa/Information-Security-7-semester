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
            Task<SortedList<BigInteger, char>> t = AnalyzeAsync(sample);
            t.Wait();
            TopSample = t.Result;
        }

        private async Task<SortedList<BigInteger, char>> AnalyzeAsync(StreamReader sample)
        {
            Dictionary<char, BigInteger> map = new Dictionary<char, BigInteger>();
            Memory<char> buffer = new Memory<char>(new char[1024 * 1024 * 256]);
            int i = 0;
            while ((i = await sample.ReadAsync(buffer)) > 0)
                Parallel.For(i, 0, (int j) =>
                    map[buffer.Span[j]] = map.ContainsKey(buffer.Span[j]) ? map[buffer.Span[j]] + 1 : 1);
            SortedList<BigInteger, char> output = new SortedList<BigInteger, char>(map.Count);
            foreach(KeyValuePair<char, BigInteger> pair in map)
            {
                output.Add(pair.Value, pair.Key);
            }
            return output;
        }

        public void Decrypt(StreamWriter output, StreamReader input)
        {
            Task.WaitAll(DecryptAsync(output, input));
        }

        public async Task DecryptAsync(StreamWriter output, StreamReader input)
        {
            if (input.BaseStream.CanSeek == false)
                throw new Exception("Need Seek input.");
            SortedList<BigInteger, char> currentSample = await AnalyzeAsync(input);
            input.BaseStream.Position = 0;
            try
            {
                Memory<char> buffer = new Memory<char>(new char[1]);
                while (await input.ReadAsync(buffer) > 0)
                {
                    await output.WriteAsync(GetDecryptChar(buffer.Span[0], currentSample));
                    await Console.Out.WriteAsync('.');
                }
            }
            catch (Exception e) { await Console.Out.WriteLineAsync(e.Message); }
        }

        private char GetDecryptChar(char v, SortedList<BigInteger, char> sample)
        {
            return TopSample[(BigInteger)((double)sample.IndexOfValue(v) / sample.Count * TopSample.Count)];
        }
    }
}
