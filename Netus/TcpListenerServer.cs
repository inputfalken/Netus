using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Functional.Maybe;
using static System.Text.Encoding;

namespace Netus {
    internal class TcpListenerServer {
        private static readonly Dictionary<string, TcpClient> UserNameToClient = new Dictionary<string, TcpClient>();

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
            ClientConnects?.Invoke("Client connected");
            await welcomeMessageSent;
            var userName = await RegisterUserAsync(client);
            var writeMessageAsync = WriteMessageAsync(clientStream, $"You have been sucessfully registered with the name: {userName}");
            var messageClientsExcept = MessageClientsExceptAsync(userName, $"{userName} has joined the chat");
            await writeMessageAsync;
            await messageClientsExcept;
            await Task.Run(() => ChatSession(userName));
        }

        private static Maybe<string> Command(string command, string userName) {
            switch (command) {
                case "-members":
                    return UserNameToClient.Select(pair => pair.Key).Aggregate((s, s1) => $"{s}\n{s1}").ToMaybe();
                case "-whoami":
                    return userName.ToMaybe();
                default:
                    return Maybe<string>.Nothing;
            }
        }

        private static async Task MessageClientsExceptAsync(string userName, string message) {
            var clientsMessaged = UserNameToClient
                .Where(pair => !pair.Key.Equals(userName))
                .Select(pair => pair.Value.GetStream())
                .Select(stream => WriteMessageAsync(stream, message));
            await Task.WhenAll(clientsMessaged);
        }

        private static async Task ChatSession(string userName) {
            var networkStream = UserNameToClient[userName].GetStream();
            var streamReader = new StreamReader(networkStream);
            var message = (await streamReader.ReadLineAsync()).ToMaybe();
            while (message.HasValue) {
                var command = message.SelectMany(s => Command(s, userName));
                if (command.HasValue) {
                    await WriteMessageAsync(networkStream, command.Value);
                }
                else {
                    var messageWithUsername = $"{userName}: {message}";
                    ClientMessage?.Invoke(messageWithUsername);
                    await MessageClientsExceptAsync(userName, messageWithUsername);
                }
                message = (await streamReader.ReadLineAsync()).ToMaybe();
            }

            UserNameToClient.Remove(userName);
            var disconectMessage = $"Client: {userName} disconnected";
            await Task.WhenAll(UserNameToClient.Select(pair => WriteMessageAsync(pair.Value.GetStream(), disconectMessage)));
            ClientDisconects?.Invoke(disconectMessage);
        }

        public static event Action<string> ClientMessage;
        public static event Action<string> ClientConnects;
        public static event Action<string> ClientDisconects;

        private static async Task<string> RegisterUserAsync(TcpClient client) {
            var streamReader = new StreamReader(client.GetStream());
            var userName = await streamReader.ReadLineAsync();
            UserNameToClient.Add(userName, client);
            return userName;
        }


        private static async Task WriteMessageAsync(Stream stream, string message) {
            var buffer = ASCII.GetBytes(message + Environment.NewLine);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}