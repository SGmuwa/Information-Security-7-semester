using System;
using System.Net.Sockets;

namespace DiffieHellmanClient.Commands
{
    internal class ReadAllMessages : AbstractCommand
    {
        private readonly CommandsProvider provider;

        public ReadAllMessages(CommandsProvider provider)
            : base(nameof(ReadAllMessages))
        {
            this.provider = provider;
        }

        public override string Info =>
            "Включает вещание всех сообщений.";

        protected override void Action(string[] args)
        {
            foreach (PackageInfo m in provider.mySystem.GetAllMessages())
                Console.WriteLine(MsgToString(m));
            Console.WriteLine("Для выхода нажмите на любую клавишу...");
            try
            {
                provider.mySystem.OnMessageSend += OnMessageSend;
                Console.ReadKey(true);
            }
            finally
            {
                provider.mySystem.OnMessageSend -= OnMessageSend;
            }
        }

        private void OnMessageSend(Businesslogic server, ulong client, dynamic message)
            => Console.WriteLine(MsgToString(message));

        private string MsgToString(PackageInfo msg)
        {
            if (msg.Json.Type == "msg")
                return $"at {msg.Time} from {msg.UserId} ({provider.mySystem.GetEndPoint(msg.UserId)}) text: {msg.Json.Message}";
            else
                return "";
        }
    }
}