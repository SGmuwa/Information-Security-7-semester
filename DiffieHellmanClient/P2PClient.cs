using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;

namespace DiffieHellmanClient
{
    class P2PClient : IDisposable
    {
        private readonly HashSet<P2PClient> Clients = new HashSet<P2PClient>();
        private TcpListener TcpListener;
        /// <summary>
        /// Используется для удалённых клиентов.
        /// </summary>
        private TcpClient remoteClient;

        private readonly Task taskRemover;
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
            TcpListener.Start();
            timer.Elapsed += TimerListner;
            this.port = port;
            taskRemover = Task.Run(RemoveOffline);
            timer.Start();
        }

        /// <summary>
        /// Создать виртуальный (удалённый) P2P клиент.
        /// </summary>
        private P2PClient(TcpClient remoteClient) { this.remoteClient = remoteClient; }

        public bool IsLive => timer.Enabled;

        internal void AddConnection(IPEndPoint toConnect)
        {
            throw new NotImplementedException();
        }

        private void RemoveOffline()
        {
            HashSet<P2PClient> toRemove = new HashSet<P2PClient>(from client in Clients where !client.IsLive select client);
            foreach (P2PClient client in toRemove)
            {
                Clients.Remove(client);
                client.Dispose();
            }
        }

        private async void AcceptConnections(TcpListener TcpListener)
        {
            Clients.Add(new P2PClient(await TcpListener.AcceptTcpClientAsync());
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
