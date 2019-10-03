using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.Text;

namespace DiffieHellmanClient
{
    class P2PClient : IDisposable, IEnumerable<TcpClient>, IEnumerable
    {
        private readonly HashSet<TcpClient> Clients = new HashSet<TcpClient>();
        private readonly TcpListener TcpListener;

        private readonly System.Timers.Timer timer = new System.Timers.Timer(20);
        /// <summary>
        /// Происходит при получении сообщения от кого-либо.
        /// </summary>
        public event Action<P2PClient, TcpClient, Memory<byte>> OnMessageSend;
        /// <summary>
        /// Происходит при новом подключении.
        /// Вернуть нужно уникальный идентификатор.
        /// </summary>
        public event Action<P2PClient, TcpClient> OnConnection;
        /// <summary>
        /// Происходит при обрыве подключения.
        /// </summary>
        public event Action<P2PClient, TcpClient> OnDisconnection;

        private readonly ushort port;

        /// <summary>
        /// Создать точку приёма пакетов.
        /// </summary>
        /// <param name="port">Порт, который будет прослушиваться и из которого будут идти пакеты.</param>
        public P2PClient(ushort port)
        {
            TcpListener = new TcpListener(IPAddress.Any, port);
            TcpListener.Start();
            timer.Elapsed += TimerListner;
            Task.Run(AcceptConnections);
            this.port = port;
            timer.Start();
        }

        public bool IsLive => timer.Enabled;

        public void AddConnection(IPEndPoint toConnect)
        {
            TcpClient client = new TcpClient(TcpListener.Server.AddressFamily);
            client.Connect(toConnect);
            Clients.Add(client);
            OnConnection?.Invoke(this, client);
        }

        private void RemoveOffline()
        {
            HashSet<TcpClient> toRemove = new HashSet<TcpClient>(from client in Clients where !client.Connected select client);
            foreach (TcpClient client in toRemove)
            {
                Clients.Remove(client);
                OnDisconnection?.Invoke(this, client);
                client.Dispose();
            }
        }

        private void AcceptConnections()
        {
            while (timer.Enabled)
                if (TcpListener.Pending())
                    Clients.Add(TcpListener.AcceptTcpClient());
                else
                    Thread.Sleep(50);
        }


        private void TimerListner(object sender, ElapsedEventArgs args)
        {
            RemoveOffline();
            foreach (TcpClient client in from c in Clients where c.Available > 0 select c)
            {
                Memory<byte> msg = new Memory<byte>(new byte[client.Available]);
                client.GetStream().Read(msg.Span);
                try
                {
                    OnMessageSend?.Invoke(this, client, msg);
                }
                catch(Exception e)
                {
                    OnMessageSend?.Invoke(this, client, Encoding.UTF8.GetBytes(e.Message));
                }
            }
        }


        public void Dispose()
        {
            TcpListener.Stop();
            timer.Dispose();
            foreach (TcpClient c in Clients)
                c.Dispose();
        }

        IEnumerator IEnumerable.GetEnumerator() => Clients.GetEnumerator();

        public IEnumerator<TcpClient> GetEnumerator() => Clients.GetEnumerator();
    }
}
