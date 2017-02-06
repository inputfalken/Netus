using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Netus {
    internal class Program {
        public static void Main(string[] args) {
            new Thread(Server.StartListening).Start();
            Server.MessageRecieved += OnServerMessageRecieved;
        }

        private static void OnServerMessageRecieved(string message) {
            Console.WriteLine(message);
        }

        private static void WebClient() {
            using (var wb = new WebClient()) {
                var downloadString = wb.DownloadString(new Uri("http://en.wikipedia.org/"));
                Console.WriteLine("Webclient result \n");
                Console.WriteLine(downloadString.Length);
            }
        }
    }
}