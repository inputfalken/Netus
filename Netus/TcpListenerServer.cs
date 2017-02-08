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


        public static void StartSynchronus() {
            const int port = 23000;
            var server = new TcpListener(IPAddress.Any, port);

            try {
                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                var bytes = new byte[256];

                // Enter the listening loop.
                while (true) {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    var client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    // Get a stream object for reading and writing
                    var stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0) {
                        // Translate data bytes to a ASCII string.
                        var data = ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        var msg = ASCII.GetBytes($"Server Responds: The lenght of the word {data} is {data.Length}");

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e) {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally {
                // Stop listening for new clients.
                server.Stop();
            }
            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}