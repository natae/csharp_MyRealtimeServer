using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyServerBase;

namespace SampleClient
{
    internal class Client
    {
        const int BUFFER_SIZE = 2048;

        private byte[] _buffer = new byte[BUFFER_SIZE];

        private ManualResetEvent _connectDone = new ManualResetEvent(false);        
        private ManualResetEvent _sendDone = new ManualResetEvent(false);

        private Socket _clientSocket;

        public void Start(string ip, int port)
        {
            var ipAddress = IPAddress.Parse(ip);
            if (ipAddress == null)
            {
                Console.WriteLine("Fail to parse IP");
                return;
            }

            var remoteEndPoint = new IPEndPoint(ipAddress, port);

            _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.BeginConnect(remoteEndPoint, new AsyncCallback(ConnectCallback), null);

            _connectDone.WaitOne();


            _clientSocket.BeginReceive(_buffer, 0, BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndConnect(ar);

                Console.WriteLine($"Socket connected to {_clientSocket.RemoteEndPoint}");

                _connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ConnectCallback Error: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var bytesRead = _clientSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    var stringContent = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
                    Console.WriteLine($"Recevied bytes: {bytesRead}, string content: {stringContent}");
                }

                _clientSocket.BeginReceive(_buffer, 0, BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());                
            }
        }

        public void Send(string data)
        {
            var byteData = Encoding.UTF8.GetBytes(data);

            _clientSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), null);

            _sendDone.WaitOne();
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var bytesSent = _clientSocket.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to server");

                _sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
