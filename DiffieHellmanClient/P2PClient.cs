using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;
using System.Text;
using System.IO;

namespace DiffieHellmanClient
{
    public class P2PClient : IDisposable, IEnumerable<ulong>, IEnumerable
    {
        private readonly ConcurrentDictionary<ulong, TcpClient> Clients = new ConcurrentDictionary<ulong, TcpClient>();
        private readonly TcpListener TcpListener;
        /// <summary>
        /// Название сервера. Используется для Debug.
        /// </summary>
        private readonly string nameServer;
        /// <summary>
        /// Таймер для удаления отключенных пользователей,
        /// подключения новых пользователей,
        /// чтения новых сообщений.
        /// </summary>
        private readonly System.Timers.Timer timerRemoverConnecterReader = new System.Timers.Timer(20);

        /// <summary>
        /// Происходит для отображения всех сообщений.
        /// </summary>
        public event Action<string> OnDebugMessage;

        private TimeSpan timeout = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Отвечает за период ожидания пакета от пользователя.
        /// </summary>
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
        public event Action<P2PClient, ulong, Memory<byte>> OnMessageSend;
        /// <summary>
        /// Происходит при новом подключении.
        /// Вернуть нужно уникальный идентификатор.
        /// </summary>
        public event Action<P2PClient, ulong> OnConnect;
        /// <summary>
        /// Происходит при обрыве подключения.
        /// </summary>
        public event Action<P2PClient, ulong> OnDisconnect;

        /// <summary>
        /// Создать точку приёма пакетов.
        /// </summary>
        /// <param name="port">Порт, который будет прослушиваться и из которого будут идти пакеты.</param>
        public P2PClient(ushort port, string nameServer = default)
        {
            TcpListener = new TcpListener(IPAddress.Any, port);
            TcpListener.Start();
            timerRemoverConnecterReader.Elapsed += TimerListner;
            timerRemoverConnecterReader.Start();
            if (nameServer == default)
                nameServer = GetHashCode().ToString();
            this.nameServer = nameServer;
        }

        public bool IsLive => timerRemoverConnecterReader.Enabled;

        public ulong AddConnection(IPEndPoint toConnect)
        {
            TcpClient client = new TcpClient(TcpListener.Server.AddressFamily);
            client.Connect(toConnect);
            ulong id = AddUser(client);
            OnConnect?.Invoke(this, id);
            return id;
        }

        public EndPoint GetEndPoint(ulong id)
        {
            if (!Clients.TryGetValue(id, out TcpClient client))
                throw new Exception("Пользователь не найден.");
            return client.Client.RemoteEndPoint;
        }

        /// <summary>
        /// Отправляет массив данных клиенту.
        /// </summary>
        /// <param name="id">Пользователь, которому надо отправить пакет.</param>
        /// <param name="toWrite">Данные, которые надо отправить.</param>
        public void Write(ulong id, Memory<byte> toWrite)
        {
            if (!Clients.TryGetValue(id, out TcpClient client))
                throw new Exception("Пользователь не найден.");
            using CancellationTokenSource CancelTokenSrc = new CancellationTokenSource(Timeout);
            using var str = new MemoryStream();
            str.Write(BitConverter.GetBytes(toWrite.Length), 0, sizeof(int));
            str.Write(toWrite.Span);
            OnDebugMessage?.Invoke($"{this}, Записываю пакет: {toWrite.Length} байт, {string.Join(" ", toWrite.ToArray())}");
            client.GetStream().WriteAsync(str.ToArray(), CancelTokenSrc.Token).AsTask().Wait();
        }

        /// <summary>
        /// Забирает один пакет с клиента.
        /// </summary>
        /// <param name="client">Клиент, от которого надо получить пакет.</param>
        /// <returns>Пакет данных от клиента.</returns>
        /// <exception cref="OperationCanceledException">Не удалось дождаться ответа от пользователя.</exception>
        private Memory<byte> Read(TcpClient client)
        {
            using CancellationTokenSource CancelTokenSrc = new CancellationTokenSource(Timeout);
            Memory<byte> buffer = new byte[sizeof(int)];
            client.GetStream().ReadAsync(buffer, CancelTokenSrc.Token).AsTask().Wait();
            int Length = BitConverter.ToInt32(buffer.Span);
            buffer = new byte[Length];
            client.GetStream().ReadAsync(buffer, CancelTokenSrc.Token).AsTask().Wait();
            OnDebugMessage?.Invoke($"{this}, Прочитал: {string.Join(" ", buffer.ToArray())}");
            return buffer;
        }

        public IEnumerator<ulong> GetEnumerator() => (from u in Clients select u.Key).GetEnumerator();

        public void Dispose()
        {
            TcpListener.Stop();
            timerRemoverConnecterReader.Dispose();
            foreach (var pair in Clients)
                pair.Value.Dispose();
        }

        public void Disconnect(ulong id)
        {
            if (Clients.TryRemove(id, out TcpClient client))
            {
                OnDisconnect?.Invoke(this, id);
                client.Dispose();
            }
        }

        private void RemoveOffline()
        {
            IEnumerable<ulong> toRemove = from pair in Clients where !pair.Value.Connected select pair.Key;
            foreach (ulong id in toRemove)
                Disconnect(id);
        }

        private void AcceptConnections()
        {
            if (TcpListener.Pending())
            {
                TcpClient @new = TcpListener.AcceptTcpClient();
                ulong id = AddUser(@new);
                /*try { */
                OnConnect?.Invoke(this, id);
                /*}
                catch (Exception e) { Console.WriteLine(e.Message); } */
            }
        }
        

        private void ReadAll()
        {
            foreach (KeyValuePair<ulong, TcpClient> pair in from pair in Clients where pair.Value.Available > 0 select pair)
            {
                Memory<byte> msg;
                try
                {
                    msg = Read(pair.Value);
                }
                catch
                {
                    Disconnect(pair.Key);
                    break;
                }
                OnMessageSend?.Invoke(this, pair.Key, msg);
            }
        }


        private void TimerListner(object sender, ElapsedEventArgs args)
        {
            AcceptConnections();
            RemoveOffline();
            ReadAll();
        }

        private readonly Random ran = new Random();

        private int NextRandom()
        { lock(ran) { return ran.Next(int.MinValue, int.MaxValue); } }

        private ulong AddUser(TcpClient toAdd)
        {
            ulong id;
            do { id = ((ulong)NextRandom() << 32) | (uint)NextRandom(); }
            while (!Clients.TryAdd(id, toAdd));
            return id;
        }

        IEnumerator IEnumerable.GetEnumerator() => Clients.GetEnumerator();

        public override string ToString()
        {
            return $"P2PClient, {nameServer}";
        }
    }
}
