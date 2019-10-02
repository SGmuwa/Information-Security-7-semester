using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DiffieHellmanClient
{
    class P2PClient : IDisposable
    {
        private readonly List<P2PClient> Clients = new List<P2PClient>();
        private TcpListener TcpListener;

        private readonly Task taskConnect;
        private StreamReader sr;
        private StreamWriter sw;
        private readonly System.Timers.Timer timer = new System.Timers.Timer(20);
        public event Action<dynamic> GetMessage;

        private readonly IPEndPoint to;
        private readonly ushort port;

        /// <summary>
        /// Создать точку приёма пакетов.
        /// </summary>
        /// <param name="port">Порт, который будет прослушиваться и из которого будут идти пакеты.</param>
        public P2PClient(ushort port)
        {
            timer.Elapsed += TimerListner;
            this.port = port;
            taskConnect = Task.Run(ConnectAll);
            timer.Start();
        }

        private P2PClient(IPEndPoint to)
        {
            this.to = to;
        }

        public bool IsLive => timer.Enabled;

        internal void AddConnection(IPEndPoint toConnect)
        {
            throw new NotImplementedException();
        }

        private void ConnectAll()
        {
            while (timer.Enabled)
            {
                HashSet<Socket> toRemove = new HashSet<Socket>(from Socket client in Clients where !client.Connected select client);
                foreach(Socket client in toRemove)
                {
                    Clients.Remove(client);
                    client.Dispose();
                }
                Clients.Add(server.Accept());
            }
        }


        private void TimerListner(object sender, ElapsedEventArgs args)
        {

            if (remoteClient.Available > 0)
                GetMessage(JsonConvert.DeserializeObject(sr.ReadToEnd()));
        }

        public void Send(dynamic toSend) => 
            sw.Write(JsonConvert.SerializeObject(toSend));

        public void Dispose()
        {
            remoteClient.Dispose();
            timer.Dispose();
            sr.Dispose();
            sw.Dispose();
        }
    }
}
