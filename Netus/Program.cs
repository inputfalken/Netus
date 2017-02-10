using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Netus {
    internal class Program {
        public static void Main(string[] args) {
            new Thread(TcpListenerServer.Listen).Start();
            TcpListenerServer.ClientConnects +=  Console.WriteLine;
            TcpListenerServer.ClientMessage += Console.WriteLine;
            TcpListenerServer.ClientDisconects += Console.WriteLine;
        }
    }
}