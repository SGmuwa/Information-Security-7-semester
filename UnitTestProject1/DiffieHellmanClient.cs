using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiffieHellmanClient;
using System;
using System.Linq;
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
            P2PClient server1 = new P2PClient("Источник");
            Program1.InitServer(server1);
            using Businesslogic Program2 = new Businesslogic();
            P2PClient server2 = new P2PClient("Приёмщик");
            Program2.InitServer(server2);
            Stopwatch sw = new Stopwatch();
            Program1.OnDebugMessage += (a, b) => Console.WriteLine($"{Program1.ToString()}: [{sw.Elapsed}] {b}");
            Program2.OnDebugMessage += (a, b) => Console.WriteLine($"{Program2.ToString()}: [{sw.Elapsed}] {b}");
            sw.Start();
            ulong From1To2 = Program1.AddConnection(server2.LocalEndPoint);
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
