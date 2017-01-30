using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Netus {
    internal static class Server {
        private static readonly Socket Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int BufferSize = 1024;
        private static readonly AutoResetEvent AutoResetEvent = new AutoResetEvent(false);


        public static void StartListening() {
            Socket.Bind(new IPEndPoint(IPAddress.Any, 23000));
            Socket.Listen(100);
            while (true) {
                Socket.BeginAccept(ClientConnects, Socket);
                AutoResetEvent.WaitOne();
            }
        }

        private static void ClientConnects(IAsyncResult ar) {
            AutoResetEvent.Set();
            Console.WriteLine("An client has connected");
            var listenter = (Socket) ar.AsyncState;
            var handler = listenter.EndAccept(ar);
            var state = new State(handler);
            handler.BeginReceive(state.Buffer, 0, BufferSize, 0, ReadClient, state);
        }

        private static void ReadClient(IAsyncResult ar) {
            var res = (State) ar.AsyncState;
            var result = res.Socket.EndReceive(ar);
            Console.WriteLine(Encoding.ASCII.GetString(res.Buffer, 0, result));
        }

        private class State {
            public Socket Socket { get; }
            public StringBuilder StringBuilder { get; }
            public byte[] Buffer { get; }

            public State(Socket socket) {
                Socket = socket;
                StringBuilder = new StringBuilder();
                Buffer = new byte[BufferSize];
            }
        }
    }
}