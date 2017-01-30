using System;
using System.Collections.Generic;
using System.Threading;

namespace Netus {
    internal class Program {

        public static void Main(string[] args) {
            new Thread(Server.StartListening).Start();
            Console.WriteLine("Out");
        }
    }
}