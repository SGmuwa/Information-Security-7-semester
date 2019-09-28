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
            {
                FileInfo toEncrypt = GetFile("Что надо зашифровать? ");
                FileInfo toSave = GetFile("Куда сохранить результат? ", true);
                using (StreamWriter w = new StreamWriter(toSave.FullName))
                {
                    using (StreamReader r = new StreamReader(toEncrypt.FullName))
                    {
                        await Encryption.EncryptAsync(w, r, GetKey("Ключ Цезаря: "));
                    }
                }
                FileInfo toSample = GetFile("Файл для сбора анализа частот: ");
                FileInfo toSaveDecrypt = GetFile("Куда постараться расшифровать?", true);
                using (StreamReader dec = new StreamReader(toSave.FullName))
                {
                    using (StreamWriter sav = new StreamWriter(toSaveDecrypt.FullName))
                    {
                        using (StreamReader sam = new StreamReader(toSample.FullName))
                        {
                            await new CharacterFrequencyAnalyzer(sam).DecryptAsync(sav, dec);
                        }
                    }
                }
            }
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
                    f = new FileInfo(Console.ReadLine().Trim('"'));
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

        static byte GetKey(string message)
        {
            do
            {
                Console.Write(message);
                if (byte.TryParse(Console.ReadLine(), out byte key))
                    return key;
                else
                    Console.WriteLine($"Ошибка, попробуйте ещё раз от {byte.MinValue} до {byte.MaxValue}.");
            } while (true);
        }
    }
}
