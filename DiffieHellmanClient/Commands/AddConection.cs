using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace DiffieHellmanClient.Commands
{
    class AddConection : AbstractCommand
    {
        private readonly CommandsProvider provider;

        public AddConection(CommandsProvider provider)
            : base(nameof(AddConection))
        {
            this.provider = provider;
        }
        public override string Info => 
            "Добавляет новый сервер в список серверов, к которым следует подключиться.";

        protected override void Action(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Требуется аргумент: сервер с портом в формате \"ip:port\" без кавычек.");
                return;
            }
            if (IPEndPoint.TryParse(args[0], out IPEndPoint result))
            {
                provider.mySystem.AddConection(result);
                return;
            }
            else
            {
                Console.WriteLine("Неудалось считать сервер с портом.");
                return;
            }
        }
    }
}
