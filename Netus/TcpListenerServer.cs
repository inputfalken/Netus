using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Netus {
    internal class TcpListenerServer {
        private static TcpListener _listener;

        public static void StartAsynchronus() {
            _listener = new TcpListener(IPAddress.Any, 23000);
            _listener.Start();
            Console.WriteLine("Listening...");
            StartAccept();
        }

        private static void StartAccept() {
            _listener.BeginAcceptTcpClient(HandleAsyncConnection, _listener);
        }

        private static void HandleAsyncConnection(IAsyncResult res) {
            StartAccept(); //listen for new connections again
            var client = _listener.EndAcceptTcpClient(res);

            var buffer = Encoding.ASCII.GetBytes("hello");
            var networkStream = client.GetStream();
            networkStream.Write(buffer, 0, buffer.Length);
        }

        public static void StartSynchronus() {
            const int port = 23000;
            var server = new TcpListener(IPAddress.Any, port);

            try {
                // Set the TcpListener on port 13000.


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
                        var data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        var msg = System.Text.Encoding.ASCII.GetBytes($"Server Responds: The lenght of the word {data} is {data.Length}");

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