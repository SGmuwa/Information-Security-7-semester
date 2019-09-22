using System;
using System.IO;
using System.Threading.Tasks;

namespace Caesar_s_code
{
    class Encryption
    {
        public static async Task EncryptAsync(StreamWriter output, StreamReader input, short key)
        {
            await Task.Run(() => Encrypt(output, input, key));
        }

        public static void Encrypt(StreamWriter output, StreamReader input, short key)
        {
            try
            {
                Span<char> buffer = new Span<char>(new char[1]);
                while(input.Read(buffer) > 0)
                {
                    output.Write((char)(buffer[0] + key));
                    Console.Out.WriteAsync('.');
                }
            }
            catch(Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
