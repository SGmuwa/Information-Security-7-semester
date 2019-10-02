using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace DiffieHellmanClient
{
    class Businesslogic : IDisposable
    {
        private P2PClient Server = null;

        private readonly Queue<dynamic> messages = new Queue<dynamic>();

        public Businesslogic() { }

        private void Socket_GetMessage(dynamic letter) => messages.Enqueue(letter);

        public void InitServer(P2PClient thisServer)
        {
            Server?.Dispose();
            Server = thisServer;
        }

        public void SendAll(string message)
        {
            foreach(P2PClient client in Server.Clients)
            {
                client.Send(new { message });
            }
        }

        public void Run() { }

        public IEnumerable<dynamic> GetAllMessages() => from m in messages select m;

        public void Dispose() => Server?.Dispose();

        public void AddConection(IPEndPoint toConnect) => Server.AddConnection(toConnect);
    }
}
