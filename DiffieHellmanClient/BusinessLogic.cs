using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace DiffieHellmanClient
{
    public class BusinessLogic : IDisposable
    {
        private P2PClient Server = null;
        private ICrypter crypter;

        private readonly Stack<PackageInfo> messages = new Stack<PackageInfo>();
        /// <summary>
        /// Происходит при получении сообщения от кого-либо.
        /// </summary>
        public event Action<BusinessLogic, ulong, dynamic> OnMessageSend;
        /// <summary>
        /// Вызывается при отключении пользователя.
        /// </summary>
        public event Action<BusinessLogic, ulong> OnUserDisconnect;
        /// <summary>
        /// Вызывается при подключении пользователя.
        /// </summary>
        public event Action<BusinessLogic, ulong> OnUserConnect;
        /// <summary>
        /// Происходит при попытке <see cref="P2PClient"/> сообщить подробности об процессе.
        /// </summary>
        public event Action<BusinessLogic, string> OnDebugMessage;

        public BusinessLogic() { }

        public void InitServer(P2PClient thisServer)
        {
            Server?.Dispose();
            Server = thisServer;
            crypter = new Crypter(Server);
            Server.OnDebugMessage += str => OnDebugMessage?.Invoke(this, str);
            Server.OnMessageSend += p_OnMessageSend;
            Server.OnConnect += p_OnConnection;
            Server.OnDisconnect += (_, u) => OnUserDisconnect?.Invoke(this, u);
        }

        private void p_OnConnection(P2PClient server, ulong userId)
        {
            try
            {
                crypter.AddUser(userId);
                OnUserConnect?.Invoke(this, userId);
            }
            catch(System.OperationCanceledException)
            {
                server.Disconnect(userId);
            }
        }

        private void p_OnMessageSend(P2PClient sender, ulong userId, Memory<byte> msg)
        {
            if (crypter.IsConnectionSafe(userId, msg))
            {
                msg = crypter.Decrypt(userId, msg);
                dynamic json = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(msg.Span));
                messages.Push(new PackageInfo(json, userId));
                OnMessageSend?.Invoke(this, userId, messages.Peek());
            }
        }

        public EndPoint GetEndPoint(ulong userId)
            => Server.GetEndPoint(userId);

        public void Send(ulong userId, dynamic msg)
        {
            Memory<byte> info = new Memory<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg)));
            int tryCount = 4000;
            while(!crypter.IsConnectionSafe(userId, info) && tryCount-- > 0)
                Thread.Sleep(1);
            info = crypter.Encrypt(userId, info);
            Server.Write(userId, info);

        }

        public void SendAll(dynamic msg)
        {
            foreach (ulong userId in Server)
                Send(userId, msg);
        }

        public void Run() { }

        public IEnumerable<PackageInfo> GetAllMessages() => from m in messages select m;

        public ulong AddConnection(IPEndPoint toConnect) => Server.AddConnection(toConnect);

        public override string ToString()
            => $"{nameof(BusinessLogic)}: countMessages = {messages.Count}, server = {Server?.ToString()}";

        public void Dispose()
        {
            Server?.Dispose();
            Server = null;
        }
    }
}
