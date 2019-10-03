using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;

namespace DiffieHellmanClient
{
    class Businesslogic : IDisposable
    {
        private P2PClient Server = null;

        private readonly Stack<dynamic> messages = new Stack<dynamic>();
        /// <summary>
        /// Происходит при получении сообщения от кого-либо.
        /// </summary>
        public event Action<P2PClient, TcpClient, dynamic> OnMessageSend;

        public Businesslogic() { }

        public void InitServer(P2PClient thisServer)
        {
            Server?.Dispose();
            Server = thisServer;
            Server.OnMessageSend += p_OnMessageSend;
        }

        private void p_OnMessageSend(P2PClient sender, TcpClient client, Memory<byte> msg)
        {
            dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msg.Span));
            messages.Push(new { TimeGet = DateTime.Now, Client = client, Json = json });
            OnMessageSend?.Invoke(sender, client, messages.Peek());
        }

        public void SendAll(string message)
        {
            foreach(TcpClient client in Server)
                client.GetStream().Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(
                    new { Type = "msg", Message = message })));
        }

        public void Run() { }

        public IEnumerable<dynamic> GetAllMessages() => from m in messages select m;

        public void AddConection(IPEndPoint toConnect) => Server.AddConnection(toConnect);

        public void Dispose()
        {
            Server?.Dispose();
            Server = null;
        }
    }
}
