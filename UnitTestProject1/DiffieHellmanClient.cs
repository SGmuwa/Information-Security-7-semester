using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiffieHellmanClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class DiffieHellmanClient
    {
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromHours(30));

        [TestMethod]
        public void Test1()
        {
            using Businesslogic Program1 = new Businesslogic();
            Program1.InitServer(new P2PClient(1000, "Источник"));
            using Businesslogic Program2 = new Businesslogic();
            Program2.InitServer(new P2PClient(1001, "Приёмщик"));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ulong From1To2 = Program1.AddConection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1001));
            var toSend = new { Type = "msg", Message = new string('a', 128) };
            Program1.Send(From1To2, toSend);
            Program2.OnMessageSend += Program2_OnMessageSend;
            try
            {
                Task.Run(() => { while (!tokenSource.IsCancellationRequested) Thread.Sleep(5); }).Wait(tokenSource.Token);
            }
            catch { }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            return;

            void Program2_OnMessageSend(Businesslogic arg1, ulong arg2, dynamic arg3)
            {
                PackageInfo[] messages = Program2.GetAllMessages().ToArray();
                Assert.AreEqual(1, messages.Length);
                Console.WriteLine(JsonConvert.SerializeObject(toSend));
                Assert.AreEqual(JsonConvert.SerializeObject(toSend), JsonConvert.SerializeObject(messages[0].Json));
                tokenSource.Cancel();
            }
        }
    }
}
