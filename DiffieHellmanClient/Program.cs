using System;
using System.Net;

namespace DiffieHellmanClient
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort @from;
            IPEndPoint to;
            Console.Out.WriteLineAsync("Удалённый ip and port: ");
            while (!IPEndPoint.TryParse(Console.ReadLine(), out to)) ;
            Console.Out.WriteLineAsync("Ваш порт: ");
            while (!ushort.TryParse(Console.ReadLine(), out @from)) ;
            using ClientSocket mySocket = new ClientSocket(@from, to);
            new Businesslogic(mySocket);
            while (mySocket.IsLive) System.Threading.Thread.Sleep(500);
        }
    }
}
