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
            string path = sample.Name;
            sample.Flush();
            sample.Dispose();
            SortedList<BigInteger, byte> result = null;
            await Task.Run(() =>
            {
                byte[] bytes = File.ReadAllBytes(sample.Name);
                sample = new FileStream(path, FileMode.Open);
                var groups = bytes
                .GroupBy(by => by)
                .Select(n => new KeyValuePair<byte, BigInteger>(n.Key, n.Count()))
                .GroupBy(by => by.Value)
                .Select(n => new KeyValuePair<BigInteger, byte>(n.Key, n.First().Key))
                .OrderBy(n => n.Value)
                .ToDictionary(t => t.Key, t => t.Value);
                result = new SortedList<BigInteger, byte>(groups);
            });
            return result;
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
