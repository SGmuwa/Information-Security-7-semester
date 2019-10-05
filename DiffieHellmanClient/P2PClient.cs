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
using System.IO;

namespace DiffieHellmanClient
{
    public class P2PClient : IDisposable, IEnumerable<TcpClient>, IEnumerable
    {
        private readonly HashSet<TcpClient> Clients = new HashSet<TcpClient>();
        private readonly TcpListener TcpListener;

        private readonly System.Timers.Timer timer = new System.Timers.Timer(20);
        private TimeSpan timeout = TimeSpan.FromSeconds(20);

        public TimeSpan Timeout
        {
            get => timeout; set
            {
                if (timeout > TimeSpan.Zero)
                    timeout = value;
                else
                    throw new ArgumentException("Вы не можете ждать ноль или менее времени!");
            }
        }
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
        public event Action<P2PClient, TcpClient> OnDisconnect;

        public readonly object TokenToIO = new object();

        /// <summary>
        /// Создать точку приёма пакетов.
        /// </summary>
        /// <param name="port">Порт, который будет прослушиваться и из которого будут идти пакеты.</param>
        public P2PClient(ushort port, string nameServer = default)
        {
            TcpListener = new TcpListener(IPAddress.Any, port);
            TcpListener.Start();
            timer.Elapsed += TimerListner;
            Task.Run(AcceptConnections);
            timer.Start();
            if (nameServer == default)
                nameServer = GetHashCode().ToString();
            this.nameServer = nameServer;
        }

        public bool IsLive => timer.Enabled;

        public TcpClient AddConnection(IPEndPoint toConnect)
        {
            TcpClient client = new TcpClient(TcpListener.Server.AddressFamily);
            client.Connect(toConnect);
            Clients.Add(client);
            OnConnection?.Invoke(this, client);
            return client;
        }

        /// <summary>
        /// Отправляет массив данных клиенту.
        /// </summary>
        /// <param name="client">Пользователь, которому надо отправить пакет.</param>
        /// <param name="toWrite">Данные, которые надо отправить.</param>
        public void Write(TcpClient client, Memory<byte> toWrite)
        {
            using CancellationTokenSource CancelTokenSrc = new CancellationTokenSource(Timeout);
            using var str = new MemoryStream();
            str.Write(BitConverter.GetBytes(toWrite.Length), 0, sizeof(int));
            str.Write(toWrite.Span);
            Console.WriteLine($"{this}, Записываю пакет: {toWrite.Length} байт, {string.Join(" ", toWrite.ToArray())}");
            lock (TokenToIO)
                client.GetStream().WriteAsync(str.ToArray(), CancelTokenSrc.Token).AsTask().Wait();
            //Console.WriteLine($"{this}, Отправлено.");
        }

        private object sync1 = new object(), sync2 = new object();

        /// <summary>
        /// Забирает один пакет с клиента.
        /// </summary>
        /// <param name="client">Клиент, от которого надо получить пакет.</param>
        /// <returns>Пакет данных от клиента.</returns>
        public Memory<byte> Read(TcpClient client)
        {
            using CancellationTokenSource CancelTokenSrc = new CancellationTokenSource(Timeout);
            Memory<byte> buffer = new byte[sizeof(int)];
            //Console.WriteLine($"{this}, Читаю размер пакета (4 байта)...");
            lock (TokenToIO)
            {
                client.GetStream().ReadAsync(buffer, CancelTokenSrc.Token).AsTask().Wait();
                int Length = BitConverter.ToInt32(buffer.Span);
                //Console.WriteLine($"{this}, Прочитал length: {Length}");
                buffer = new byte[Length];
                //Console.WriteLine($"{this}, Читаю пакет {Length} байт...");
                client.GetStream().ReadAsync(buffer, CancelTokenSrc.Token).AsTask().Wait();
            }
            Console.WriteLine($"{this}, Прочитал: {string.Join(" ", buffer.ToArray())}");
            return buffer;
        }

        public IEnumerator<TcpClient> GetEnumerator() => Clients.GetEnumerator();

        public void Dispose()
        {
            TcpListener.Stop();
            timer.Dispose();
            foreach (TcpClient c in Clients)
                c.Dispose();
        }

        private void RemoveOffline()
        {
            HashSet<TcpClient> toRemove = new HashSet<TcpClient>(from client in Clients where !client.Connected select client);
            foreach (TcpClient client in toRemove)
            {
                Clients.Remove(client);
                OnDisconnect?.Invoke(this, client);
                client.Dispose();
            }
        }

        private void AcceptConnections()
        {
            while (timer.Enabled)
                lock (TokenToIO)
                {
                    if (TcpListener.Pending())
                    {
                        TcpClient @new = TcpListener.AcceptTcpClient();
                        Clients.Add(@new);
                        /*try { */
                        OnConnection?.Invoke(this, @new); /*}
                    catch (Exception e) { Console.WriteLine(e.Message); } */
                    }
                    else
                        Thread.Sleep(50);
                }
        }


        private void TimerListner(object sender, ElapsedEventArgs args)
        {
            RemoveOffline();
            foreach (TcpClient client in from c in Clients where c.Available > 0 select c)
            {
                Memory<byte> msg = Read(client);
                try
                {
                    OnMessageSend?.Invoke(this, client, msg);
                }
                catch (Exception e)
                {
                    OnMessageSend?.Invoke(this, client, Encoding.UTF8.GetBytes(e.Message));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => Clients.GetEnumerator();

        private readonly string nameServer;

        public override string ToString()
        {
            return $"P2PClient, {nameServer}";
        }
    }
}
