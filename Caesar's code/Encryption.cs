using System;
using System.IO;
using System.Threading.Tasks;
using static Caesar_s_code.LettersSupportProvider;

namespace Caesar_s_code
{
    public class Encryption
    {
        public static string Encrypt(string input, int key)
        {
            using (StreamWriter inputStreamFirst = new StreamWriter(new MemoryStream()))
            {
                inputStreamFirst.Write(input);
                inputStreamFirst.Flush();
                inputStreamFirst.BaseStream.Position = 0;
                using (StreamWriter outputStreamFirst = new StreamWriter(new MemoryStream()))
                {
                    using (StreamReader inputStreamSecond = new StreamReader(inputStreamFirst.BaseStream))
                    {
                        Encrypt(outputStreamFirst, inputStreamSecond, key);
                        outputStreamFirst.Flush();
                        outputStreamFirst.BaseStream.Position = 0;
                        using (StreamReader outputStreamSecond = new StreamReader(outputStreamFirst.BaseStream))
                        {
                            return outputStreamSecond.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static void Encrypt(StreamWriter output, StreamReader input, int key)
            => EncryptAsync(output, input, key).Wait();

        public static async Task EncryptAsync(StreamWriter output, StreamReader input, int key)
        {
            try
            {
                char[] buffer = new char[1024 * 1024 * 256];
                int i = 0;
                while ((i = await input.ReadAsync(buffer)) > 0)
                {
                    Parallel.For(0, i, (j) =>
                    {
                        buffer[j] = EncryptChar(buffer[j], key);
                    });
                    await output.WriteAsync(buffer, 0, i);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private static char EncryptChar(char input, int key)
        {
            int i = LettersSupport.IndexOf(input);
            if (i == -1) return input;
            i += key;
            while (i < 0) i += LettersSupport.Count;
            while (i >= LettersSupport.Count) i -= LettersSupport.Count;
            return LettersSupport[i];
        }
    }
}
