using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Netus {
    internal class Program {
        public static void Main(string[] args) {
            Task.Run(TcpListenerServer.StartAsync);
            TcpListenerServer.ClientConnects += Console.WriteLine;
            TcpListenerServer.ClientMessage += Console.WriteLine;
            TcpListenerServer.ClientDisconects += Console.WriteLine;
            Console.WriteLine("Press Enter to kill the server");
            Console.ReadLine();
        }
    }
}