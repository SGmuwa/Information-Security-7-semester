using System;
using System.IO;
using System.Threading.Tasks;

namespace Caesar_s_code
{
    class Encryption
    {
        public static void Encrypt(FileStream output, FileStream input, short key)
            => EncryptAsync(output, input, key).Wait();

        public static async Task EncryptAsync(FileStream output, FileStream input, short key)
        {
            try
            {
                byte[] buffer = new byte[1024 * 1024 * 256];
                int i = 0;
                while((i = await input.ReadAsync(buffer)) > 0)
                {
                    Parallel.For(0, i, (j) =>
                    {
                        buffer[j] = (byte)(buffer[j] + key);
                    });
                    await output.WriteAsync(buffer, 0, i);
                    await Console.Out.WriteAsync('.');
                }
            }
            catch(Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
