using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using static Caesar_s_code.LettersSupportProvider;

namespace Caesar_s_code
{
    public class CharacterFrequencyAnalyzer
    {
        private SortedList<BigInteger, char> TopSample;

        public CharacterFrequencyAnalyzer(string sample)
            : this(sample.ConvertToStream()) { }

        public CharacterFrequencyAnalyzer(StreamReader sample)
        {
            Task<SortedList<BigInteger, char>> t = AnalyzeAsync(sample);
            t.Wait();
            TopSample = t.Result;
        }

        private async Task<SortedList<BigInteger, char>> AnalyzeAsync(StreamReader sample)
        {
            ConcurrentDictionary<char, BigInteger> map = new ConcurrentDictionary<char, BigInteger>();
            Memory<char> buffer = new Memory<char>(new char[1024 * 1024 * 256]);
            int i = 0;
            while ((i = await sample.ReadAsync(buffer)) > 0)
                Parallel.For(0, i, (int j) => {
                    if (LettersSupport.Contains(buffer.Span[j]))
                        map[buffer.Span[j]] = map.ContainsKey(buffer.Span[j]) ? map[buffer.Span[j]] + 1 : 1;
                    });
            BigInteger maxRepeat = SearchMaxReapeat(map.Values);
            if (maxRepeat > 1)
            {
                Parallel.ForEach(map.Keys, key =>
                {
                    map[key] *= maxRepeat;
                });
            }
            SortedList<BigInteger, char> output = new SortedList<BigInteger, char>(map.Count);
            foreach(KeyValuePair<char, BigInteger> pair in map)
            {
                BigInteger key = pair.Value;
                while (output.ContainsKey(key))
                {
                    key++;
                }
                output.Add(key, pair.Key);
            }
            return output;
        }

        /// <summary>
        /// Ищет повторяющиеся элементы. Возвращает максимальное количество повторов одного и того же элемента.
        /// </summary>
        /// <param name="values">Список, в котором надо искать повторяющиеся элементы.</param>
        /// <returns>Максимальное количество повторов одного и того же элемента</returns>
        private BigInteger SearchMaxReapeat<T>(ICollection<T> values)
        {
            Dictionary<T, BigInteger> d = new Dictionary<T, BigInteger>(values.Count);
            foreach(T e in values)
            {
                d[e] = d.ContainsKey(e) ? d[e] + 1 : 1;
            }
            return d.Values.Max();
        }

        public string Decrypt(string input)
        {
            StreamWriter ms = new StreamWriter(new MemoryStream());
            Task.WaitAll(DecryptAsync(ms, input.ConvertToStream()));
            return ms.ConvertStringAndClose();
        }

        public void Decrypt(StreamWriter output, StreamReader input)
        {
            Task.WaitAll(DecryptAsync(output, input));
        }

        public async Task DecryptAsync(StreamWriter output, StreamReader input)
        {
            if (input.BaseStream.CanSeek == false)
                throw new IOException("Need Seek input.");
            SortedList<BigInteger, char> currentSample = await AnalyzeAsync(input);
            input.BaseStream.Position = 0;
            input.DiscardBufferedData();
            try
            {
                char[] buffer = new char[1024 * 1024 * 256];
                int i = 0;
                while ((i = await input.ReadAsync(buffer)) > 0)
                {
                    Parallel.For(0, i, (j) =>
                    {
                        buffer[j] = GetDecryptChar(buffer[j], currentSample);
                    });
                    await output.WriteAsync(buffer, 0, i);
                    await Console.Out.WriteAsync('.');
                }
            }
            catch (Exception e) { await Console.Out.WriteLineAsync(e.Message); }
        }

        private char GetDecryptChar(char v, SortedList<BigInteger, char> sample)
        {
            if (LettersSupport.Contains(v))
                return TopSample.Values[(int)((double)sample.IndexOfValue(v) / sample.Count * TopSample.Count)];
            return v;
        }
    }
}
