using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netus {
    internal class Program {
        public static void Main(string[] args) {
            new Thread(TcpListenerServer.StartAsynchronus).Start();
            TcpListenerServer.ClientConnects += () => Console.WriteLine("Client Connected");
            TcpListenerServer.ClientMessage += Console.WriteLine;
        }
    }
}