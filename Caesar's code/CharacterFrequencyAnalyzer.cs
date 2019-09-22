using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Caesar_s_code
{
    class CharacterFrequencyAnalyzer
    {
        private SortedList<BigInteger, byte> TopSample;
        public CharacterFrequencyAnalyzer(FileStream sample)
        {
            Task<SortedList<BigInteger, byte>> t = AnalyzeAsync(sample);
            t.Wait();
            TopSample = t.Result;
        }

        private async Task<SortedList<BigInteger, byte>> AnalyzeAsync(FileStream sample)
        {
            ConcurrentDictionary<byte, BigInteger> map = new ConcurrentDictionary<byte, BigInteger>();
            Memory<byte> buffer = new Memory<byte>(new byte[1024 * 1024 * 256]);
            int i = 0;
            while ((i = await sample.ReadAsync(buffer)) > 0)
                Parallel.For(0, i, (int j) =>
                    map[buffer.Span[j]] = map.ContainsKey(buffer.Span[j]) ? map[buffer.Span[j]] + 1 : 1);
            BigInteger maxRepeat = SearchMaxReapeat(map.Values);
            await Console.Out.WriteLineAsync("repeat: " + maxRepeat);
            Parallel.ForEach(map, pair =>
            {
                Console.Out.WriteLineAsync($"{pair.Value : 0000000}:{pair.Key : 000}");
            });
            if (maxRepeat > 1)
            {
                Parallel.ForEach(map.Keys, key =>
                {
                    map[key] *= maxRepeat;
                });
            }
            SortedList<BigInteger, byte> output = new SortedList<BigInteger, byte>(map.Count);
            foreach(KeyValuePair<byte, BigInteger> pair in map)
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

        public void Decrypt(FileStream output, FileStream input)
        {
            Task.WaitAll(DecryptAsync(output, input));
        }

        public async Task DecryptAsync(FileStream output, FileStream input)
        {
            if (input.CanSeek == false)
                throw new Exception("Need Seek input.");
            SortedList<BigInteger, byte> currentSample = await AnalyzeAsync(input);
            input.Position = 0;
            try
            {
                byte[] buffer = new byte[1024 * 1024 * 256];
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

        private byte GetDecryptChar(byte v, SortedList<BigInteger, byte> sample)
        {
            return TopSample.Values[(int)((double)sample.IndexOfValue(v) / sample.Count * TopSample.Count)];
        }
    }
}
