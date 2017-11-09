using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    public static class Networking
    {
        public const int DEFAULT_PORT = 0x2af8;

        public static void AcceptNewClient(IAsyncResult ar)
        {
            Console.WriteLine("A new Client has contacted the Server.");
            NewConnectionState asyncState = (NewConnectionState)ar.AsyncState;
            Socket socket = null;
            try
            {
                socket = asyncState.listener.EndAcceptSocket(ar);
                socket.NoDelay = true;
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1.ToString());
                asyncState.listener.BeginAcceptSocket(new AsyncCallback(Networking.AcceptNewClient), asyncState);
                return;
            }
            SocketState state2 = new SocketState
            {
                call_me = asyncState.callMe,
                workSocket = socket
            };
            state2.call_me(state2);
            asyncState.listener.BeginAcceptSocket(new AsyncCallback(Networking.AcceptNewClient), asyncState);
        }

        private static void AwaitInitialDataFromServer(SocketState state)
        {
            try
            {
                state.workSocket.BeginReceive(state.buffer, 0, 0x1000, SocketFlags.None, new AsyncCallback(Networking.ReceiveCallback), state);
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1.ToString());
            }
        }

        private static void ConnectedToServer(IAsyncResult ar)
        {
            SocketState asyncState = (SocketState)ar.AsyncState;
            try
            {
                asyncState.workSocket.EndConnect(ar);
                asyncState.workSocket.NoDelay = true;
                asyncState.call_me(asyncState);
                AwaitInitialDataFromServer(asyncState);
            }
            catch (Exception exception)
            {
                asyncState.error_message = exception.ToString();
                asyncState.error_occured = true;
                asyncState.call_me(asyncState);
            }
        }

        public static Socket ConnectToServer(Action<SocketState> call_me, string host_name)
        {
            try
            {
                IPAddress none = IPAddress.None;
                try
                {
                    bool flag = false;
                    foreach (IPAddress address2 in Dns.GetHostEntry(host_name).AddressList)
                    {
                        if (address2.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            flag = true;
                            none = address2;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    none = IPAddress.Parse(host_name);
                }
                Socket socket = new Socket(none.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                socket.NoDelay = true;
                SocketState state = new SocketState
                {
                    call_me = call_me,
                    workSocket = socket
                };
                if (!state.workSocket.BeginConnect(none, 0x2af8, new AsyncCallback(Networking.ConnectedToServer), state).AsyncWaitHandle.WaitOne(0xbb8, true))
                {
                    socket.Close();
                    return socket;
                }
                return state.workSocket;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                SocketState asyncState = (SocketState)ar.AsyncState;
                int count = asyncState.workSocket.EndReceive(ar);
                if (count > 0)
                {
                    asyncState.sb.Append(Encoding.UTF8.GetString(asyncState.buffer, 0, count));
                    try
                    {
                        asyncState.call_me(asyncState);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("error is: " + exception);
                    }
                }
            }
            catch (Exception exception2)
            {
                Console.WriteLine("EXCEPTION: " + exception2.ToString());
            }
        }

        public static void RequestMoreData(SocketState state)
        {
            state.workSocket.BeginReceive(state.buffer, 0, 0x1000, SocketFlags.None, new AsyncCallback(Networking.ReceiveCallback), state);
        }

        public static bool Send(Socket socket, string data)
        {
            try
            {
                int offset = 0;
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                socket.BeginSend(bytes, offset, bytes.Length, SocketFlags.None, new AsyncCallback(Networking.SendCallback), socket);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception)
                {
                }
                return false;
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                ((Socket)ar.AsyncState).EndSend(ar);
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1.ToString());
            }
        }

        public static void ServerAwaitingClientLoop(Action<SocketState> call_this)
        {
            Console.WriteLine("Server is up. Awaiting first client");
            TcpListener l = new TcpListener(IPAddress.Any, 0x2af8);
            try
            {
                l.Start();
                NewConnectionState state = new NewConnectionState(call_this, l);
                l.BeginAcceptSocket(new AsyncCallback(Networking.AcceptNewClient), state);
            }
            catch (Exception exception1)
            {
                Console.WriteLine(exception1.ToString());
            }
        }
    }
}

