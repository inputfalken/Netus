using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using static System.Text.Encoding;

namespace Netus {
    internal class TcpListenerServer {
        private static readonly Dictionary<TcpClient, string> ClientToUserName = new Dictionary<TcpClient, string>();

        public static void Listen() {
            var listener = new TcpListener(IPAddress.Any, 23000);
            listener.Start();
            while (true) {
                HandleClient(listener.AcceptTcpClient());
            }
        }

        private static async void HandleClient(TcpClient client) {
            var clientStream = client.GetStream();
            var welcomeMessageSent = WriteMessageAsync(clientStream, "Welcome please enter your name");
            ClientConnects?.Invoke();
            await welcomeMessageSent;
            var userName = await RegisterUserAsync(client);
            var writeMessageAsync = WriteMessageAsync(clientStream, $"You have been sucessfully registered with the name: {userName}");
            var messageClientsExcept = MessageClientsExceptAsync(client, $"{userName} has joined the chat");
            await writeMessageAsync;
            await messageClientsExcept;
            await Task.Run(() => ChatSession(client));
        }

        private static Maybe<string> Command(string command, TcpClient client) {
            switch (command) {
                case "-members":
                    return ClientToUserName.Select(pair => pair.Value).Aggregate((s, s1) => $"{s}\n{s1}").ToMaybe();
                case "-whoami":
                    return ClientToUserName[client].ToMaybe();
                default:
                    return Maybe<string>.Nothing;
            }
        }

        private static async Task MessageClientsExceptAsync(TcpClient client, string message) {
            var userName = ClientToUserName[client];
            var clientsMessaged = ClientToUserName
                .Where(pair => !pair.Value.Equals(userName))
                .Select(pair => pair.Key.GetStream())
                .Select(stream => WriteMessageAsync(stream, message));
            await Task.WhenAll(clientsMessaged);
        }

        private static async Task ChatSession(TcpClient client) {
            var networkStream = client.GetStream();
            var userName = ClientToUserName[client];
            var buffer = new byte[1024];
            while (true) {
                if (await networkStream.ReadAsync(buffer, 0, buffer.Length) > 0) {
                    var res = ASCII.GetString(buffer);
                    var command = Command(res, client);
                    if (command.HasValue) {
                        await WriteMessageAsync(networkStream, command.Value);
                    }
                    else {
                        var message = $"{userName}: {res}";
                        ClientMessage?.Invoke(message);
                        await MessageClientsExceptAsync(client, message);
                    }
                }
                else {
                    ClientToUserName.Remove(client);
                    var disconectMessage = $"Client: {userName} disconnected";
                    foreach (var keyValuePair in ClientToUserName)
                        await WriteMessageAsync(keyValuePair.Key.GetStream(), disconectMessage);
                    ClientDisconects?.Invoke(disconectMessage);
                    break;
                }
            }
        }

        public static event Action<string> ClientMessage;
        public static event Action ClientConnects;
        public static event Action<string> ClientDisconects;

        private static async Task<string> RegisterUserAsync(TcpClient client) {
            var streamReader = new StreamReader(client.GetStream());
            var userName = await streamReader.ReadLineAsync();
            ClientToUserName.Add(client, userName);
            return userName;
        }


        private static async Task WriteMessageAsync(Stream stream, string message) {
            var buffer = ASCII.GetBytes(message + Environment.NewLine);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}