﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class DiffieHellmanClient
    {
        [TestMethod]
        public void Test1()
        {
            using Businesslogic Program1 = new Businesslogic();
            Program1.InitServer(new P2PClient(1000, "Источник"));
            using Businesslogic Program2 = new Businesslogic();
            Program2.InitServer(new P2PClient(1001, "Приёмщик"));
            TcpClient From1To2 = Program1.AddConection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1001));
            dynamic toSend = new { Type = "msg", Message = new string('a', 128) };
            Program1.Send(From1To2, toSend);
            System.Threading.Thread.Sleep(10);
            PackageInfo[] messages = Program2.GetAllMessages().ToArray();
            Assert.AreEqual(1, messages.Length);
            Assert.AreEqual(toSend, messages[0]);
        }
    }
}