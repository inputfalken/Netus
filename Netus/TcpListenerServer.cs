using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Text.Encoding;

namespace Netus {
    internal class TcpListenerServer {
        private static readonly AutoResetEvent AutoResetEvent = new AutoResetEvent(false);

        private static readonly Dictionary<string, TcpClient> UserNameToClient = new Dictionary<string, TcpClient>();

        public static void StartAsynchronus() {
            var listener = new TcpListener(IPAddress.Any, 23000);
            listener.Start();
            while (true) {
                HandleClientAsync(listener.AcceptTcpClientAsync());
                AutoResetEvent.WaitOne();
            }
        }

        private static async void HandleClientAsync(Task<TcpClient> clientTask) {
            AutoResetEvent.Set();
            var client = await clientTask;
            Console.WriteLine("Client connected.");
            var clientStream = client.GetStream();
            await GreetUser(clientStream); // After user is greeted do the following.
            var userName = await RegisterUser(client);
            await WriteMessageAsync(clientStream, $"You have been sucessfully registered with the name: {userName}");
        }

        private static async Task<string> RegisterUser(TcpClient client) {
            var streamReader = new StreamReader(client.GetStream());
            var userName = await streamReader.ReadLineAsync();
            UserNameToClient.Add(userName, client);
            return userName;
        }

        private static async Task GreetUser(Stream stream) {
            var buffer = ASCII.GetBytes("Welcome please enter your name\n");
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        private static async Task WriteMessageAsync(Stream stream, string message) {
            var buffer = ASCII.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}