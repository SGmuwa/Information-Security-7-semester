using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.IO;
using Newtonsoft.Json;

namespace DiffieHellmanClient
{
    class ClientSocket : IDisposable
    {
        private readonly TcpClient client;
        private StreamReader sr;
        private StreamWriter sw;
        private readonly System.Timers.Timer timer = new System.Timers.Timer(20);
        public event Action<dynamic> GetMessage;
        private readonly IPEndPoint to;

        public bool IsLive => timer.Enabled;

        public ClientSocket(ushort port, IPEndPoint to)
        {
            
            client = new TcpClient(new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], port));
            timer.Elapsed += TimerListner;
            this.to = to;
            Connect();
            timer.Start();
        }

        private void Connect()
        {
            while (!client.Connected)
                try
                {
                    client.Connect(to);
                }
                catch (Exception e) { Console.Out.WriteLineAsync(e.Message); }
            sw = new StreamWriter(client.GetStream()) { AutoFlush = true };
            sr = new StreamReader(client.GetStream());
            Console.Out.WriteLineAsync(client.Client.LocalEndPoint.ToString());
        }


        private void TimerListner(object sender, ElapsedEventArgs args)
        {
            Connect();
            if (client.Available > 0)
                GetMessage(JsonConvert.DeserializeObject(sr.ReadToEnd()));
        }

        public void Send(dynamic toSend) => 
            sw.Write(JsonConvert.SerializeObject(toSend));

        public void Dispose()
        {
            client.Dispose();
            timer.Dispose();
            sr.Dispose();
            sw.Dispose();
        }
    }
}
