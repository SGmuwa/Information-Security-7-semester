using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiffieHellmanClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Net.Sockets;

namespace UnitTestProject1
{
    [TestClass]
    class DiffieHellmanClient
    {
        [TestMethod]
        public void Test1()
        {
            using Businesslogic Program1 = new Businesslogic();
            Program1.InitServer(new P2PClient(1000));
            using Businesslogic Program2 = new Businesslogic();
            Program2.InitServer(new P2PClient(1001));
            TcpClient From1To2 = Program1.AddConection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1000));
            dynamic toSend = new { Type = "msg", Message = "Hello" };
            Program1.Send(From1To2, toSend);
            PackageInfo[] messages = Program2.GetAllMessages().ToArray();
            Assert.AreEqual(1, messages.Length);
            Assert.AreEqual(toSend, messages[0]);
        }
    }
}
