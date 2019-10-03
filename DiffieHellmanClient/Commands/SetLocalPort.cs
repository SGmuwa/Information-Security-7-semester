using System;
using System.Net;

namespace DiffieHellmanClient.Commands
{
    internal class SetLocalPort : AbstractCommand
    {
        private readonly CommandsProvider provider;

        public SetLocalPort(CommandsProvider provider)
            : base(nameof(SetLocalPort))
        {
            this.provider = provider;
        }

        public override string Info => 
            "Устанавливает локальный порт клиента.";

        protected override void Action(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine($"Требуется как минимум 1 аргумент с целым значением от {IPEndPoint.MinPort} до {IPEndPoint.MaxPort}.");
                return;
            }
            if(ushort.TryParse(args[0], out ushort result))
            {
                if (IPEndPoint.MinPort <= result && result <= IPEndPoint.MaxPort)
                {
                    provider.mySystem.Dispose();
                    provider.mySystem.InitServer(new P2PClient(result));
                    return;
                }
                else
                {
                    Console.WriteLine($"{result} - вне диапазона. Диапазон: от {IPEndPoint.MinPort} до {IPEndPoint.MaxPort}.");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"\"{args[0]}\" - не удалось считать как порт.");
                return;
            }
        }
    }
}