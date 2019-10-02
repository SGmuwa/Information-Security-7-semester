using System;
using System.Collections.Generic;
using System.Text;

namespace DiffieHellmanClient
{
    class Businesslogic
    {
        private readonly ClientSocket socket;

        public Businesslogic(ClientSocket socket)
        {
            this.socket = socket;
            this.socket.GetMessage += Socket_GetMessage;
            this.socket.Send(new { message = "Hello" });
        }

        private void Socket_GetMessage(dynamic obj)
        {
            Console.WriteLine(obj.message);
        }
    }
}
