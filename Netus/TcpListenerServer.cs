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

        private static readonly Dictionary<TcpClient, string> UserNameToClient = new Dictionary<TcpClient, string>();

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
            ClientConnects?.Invoke();
            var clientStream = client.GetStream();
            await WriteMessageAsync(clientStream, "Welcome please enter your name\n");
            var userName = await RegisterUser(client);
            await WriteMessageAsync(clientStream, $"You have been sucessfully registered with the name: {userName}\n");
            await clientStream.FlushAsync();
            var streamReader = new StreamReader(clientStream);
            while (true) {
                var readLineAsync = await streamReader.ReadLineAsync();
                var message = $"{userName}: {readLineAsync}\n";
                Console.WriteLine(message);
                var messageClients = UserNameToClient
                    .Where(pair => !pair.Value.Equals(userName))
                    .Select(pair => pair.Key.GetStream())
                    .Select(stream => WriteMessageAsync(stream, message));
                await Task.WhenAll(messageClients);
            }
        }

        public static event Action ClientConnects;

        private static async Task<string> RegisterUser(TcpClient client) {
            var streamReader = new StreamReader(client.GetStream());
            var userName = await streamReader.ReadLineAsync();
            UserNameToClient.Add(client, userName);
            return userName;
        }


        private static async Task WriteMessageAsync(Stream stream, string message) {
            var buffer = ASCII.GetBytes(message);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}