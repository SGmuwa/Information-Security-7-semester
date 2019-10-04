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
        private ICrypter crypter;

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
            crypter = new Crypter(Server);
            Server.OnMessageSend += p_OnMessageSend;
            Server.OnConnection += p_OnConnection;
        }

        private void p_OnConnection(P2PClient server, TcpClient client)
        {
            crypter.AddUser(client);
        }

        private void p_OnMessageSend(P2PClient sender, TcpClient client, Memory<byte> msg)
        {
            if (crypter.IsConnectionSafe(client))
            {
                msg = crypter.Decrypt(client, msg);
                dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msg.Span));
                messages.Push(new { TimeGet = DateTime.Now, Client = client, Json = json });
                OnMessageSend?.Invoke(sender, client, messages.Peek());
            }
        }

        public void Send(TcpClient client, dynamic msg)
        {
            Memory<byte> info = new Memory<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
            info = crypter.Encrypt(client, info);
            client.GetStream().Write(info.Span);
        }

        public void SendAll(dynamic msg)
        {
            foreach (TcpClient client in Server)
                Send(client, msg);
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
