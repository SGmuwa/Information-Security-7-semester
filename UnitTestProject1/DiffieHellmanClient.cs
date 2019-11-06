using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiffieHellmanClient;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class DiffieHellmanClient
    {
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));

        [TestMethod]
        public void Test1()
        {
            using BusinessLogic Program1 = new BusinessLogic();
            P2PClient server1 = new P2PClient("Источник");
            Program1.InitServer(server1);
            using BusinessLogic Program2 = new BusinessLogic();
            P2PClient server2 = new P2PClient("Приёмщик");
            Program2.InitServer(server2);
            Stopwatch sw = new Stopwatch();
            Program1.OnDebugMessage += (a, b) => Console.WriteLine($"{Program1.ToString()}: [{sw.Elapsed}] {b}");
            Program2.OnDebugMessage += (a, b) => Console.WriteLine($"{Program2.ToString()}: [{sw.Elapsed}] {b}");
            sw.Start();
            ulong From1To2 = Program1.AddConnection(server2.LocalEndPoint);
            var toSend = new { Type = "msg", Message = new string('g', 128) + new string('я', 128) };
            Program2.OnMessageSend += Program2_OnMessageSend;
            Program1.Send(From1To2, toSend);
            bool good = false;
            while(!tokenSource.IsCancellationRequested)
                Thread.Sleep(1);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Assert.IsTrue(good);
            return;

            void Program2_OnMessageSend(BusinessLogic arg1, ulong arg2, dynamic arg3)
            {
                PackageInfo[] messages = Program2.GetAllMessages().ToArray();
                Assert.AreEqual(1, messages.Length);
                Console.WriteLine(JsonConvert.SerializeObject(toSend));
                Assert.AreEqual(JsonConvert.SerializeObject(toSend), JsonConvert.SerializeObject(messages[0].Json));
                good = true;
                tokenSource.Cancel();
            }
        }
    }
}
