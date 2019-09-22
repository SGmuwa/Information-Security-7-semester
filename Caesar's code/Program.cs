using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Caesar_s_code
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args).Wait();
            Console.Out.WriteLineAsync("Нажмите на кнопку для завершения работы...");
            Console.ReadKey(true);
        }

        static async Task Run(string[] args)
        {
            //try
            //{
                using (FileStream w = GetFile("Куда сохранить результат? ", true).Open(FileMode.Create))
                {
                    using (FileStream r = GetFile("Что надо зашифровать? ").Open(FileMode.Open))
                    {
                        await Encryption.EncryptAsync(w, r, GetKey("Ключ Цезаря: "));
                    }
                    using (FileStream d = GetFile("Куда постараться расшифровать?").Open(FileMode.Create))
                    {
                        using (FileStream s = GetFile("Файл для сбора анализа частот: ").Open(FileMode.Open))
                        {
                            await new CharacterFrequencyAnalyzer(s).DecryptAsync(d, w);
                        }
                    }
                }
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }


        static FileInfo GetFile(string message, bool isNeedWrite = true)
        {
            FileInfo f = null;
            do
            {
                Console.Write(message);
                try
                {
                    f = new FileInfo(Console.ReadLine());
                    if (!f.Exists)
                    {
                        f.Create().Dispose();
                        f = new FileInfo(f.FullName);
                    }
                }
                catch(Exception e) { Console.WriteLine(e.Message); continue; }
                if (f.IsReadOnly && isNeedWrite) { Console.WriteLine("Нет доступа к файлу на запись."); continue; }
                break;
            } while (true);
            return f;
        }

        static short GetKey(string message)
        {
            do
            {
                Console.Write(message);
                if (short.TryParse(Console.ReadLine(), out short key))
                    return key;
                else
                    Console.WriteLine($"Ошибка, попробуйте ещё раз от {short.MinValue} до {short.MaxValue}.");
            } while (true);
        }
    }
}
