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
            foreach (var m in provider.mySystem.GetAllMessages())
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

        private void OnMessageSend(P2PClient server, TcpClient client, dynamic message)
            => Console.WriteLine(MsgToString(message));

        private string MsgToString(dynamic msg)
        {
            if (msg.Json.Type == "msg")
                return $"at {msg.TimeGet} from {((TcpClient)msg.Client).Client.RemoteEndPoint} text: {msg.Json.Message}";
            else
                return "";
        }
    }
}