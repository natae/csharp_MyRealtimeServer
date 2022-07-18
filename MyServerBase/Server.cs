using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace MyServerBase
{
    public delegate void OnConnect(long sessionId);
    public delegate void OnDisconnect(long sessionId, string reason);
    public delegate void OnReceive(long sessionId, byte[] data);
    
    public class Server
    {
        public OnConnect OnConnectFunc;
        public OnDisconnect OnDisconnectFunc;
        public OnReceive OnReceiveFunc;

        private Config _config;

        private bool _isListening;
        private Socket _listenerSocket;
        private ManualResetEvent _acceptEvent = new ManualResetEvent(false);

        private object _sessionLock = new object();
        private long _lastSessionId;
        private List<Session> _freeSessionList = new List<Session>();
        private Dictionary<long, Session> _sessionMap = new Dictionary<long, Session>();

        private Timer _heartBeatTimer;

        public void Start(Config config)
        {   
            if (config == null)
            {
                Console.WriteLine("Config is null");
                return;
            }

            _config = config;

            Logger.Init((LogLevel) _config.LogLevel);

            InitListening();

            InitHeartBeatCheck();

            WaitForExit();
        }

        public void Stop()
        {
            _isListening = false;
            _listenerSocket.Close();

            foreach (var session in _sessionMap.Values)
            {
                session.ClientSocket.Close();
            }
            _sessionMap.Clear();

            if (_heartBeatTimer != null)
            {
                _heartBeatTimer.Dispose();
            }

            _acceptEvent.Set();
        }

        private void InitListening()
        {
            IPAddress? ipAddress;
            if (string.IsNullOrEmpty(_config.IP))
            {
                var ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }
            else
            {
                ipAddress = IPAddress.Parse(_config.IP);
            }

            if (ipAddress == null)
            {
                Logger.LogError("IPAddress is null");
                return;
            }

            var ipEndPoint = new IPEndPoint(ipAddress, _config.Port);

            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _listenerSocket.Bind(ipEndPoint);
                _listenerSocket.Listen(_config.Backlog);
                _isListening = true;

                var listenerThread = new Thread(RunListening);
                listenerThread.Start();

                Logger.LogInformation($"Server started with {ipAddress}:{_config.Port}");
            }
            catch (Exception e)
            {
                Logger.LogError($"ListenerSocket Error: {e}");
            }
        }

        private void RunListening()
        {
            while (_isListening)
            {
                _acceptEvent.Reset();
                _listenerSocket.BeginAccept(AcceptCallback, _listenerSocket);

                _acceptEvent.WaitOne();
            }
        }

        private void InitHeartBeatCheck()
        {
            if (_config.HeartBeatInterval == 0 || _config.HeartBeatTimeout == 0)
            {
                Logger.LogError($"HeartBeat configs are null");
                return;
            }
            _heartBeatTimer = new Timer(OnCheckHeartBeat);
            _heartBeatTimer.Change(0, _config.HeartBeatInterval * 1000);
        }

        private void OnCheckHeartBeat(object? state)
        {
            var now = Util.Now;
            var timeout = _config.HeartBeatTimeout * 1000;

            lock (_sessionLock)
            {
                foreach (var session in _sessionMap.Values)
                {
                    if (now - session.HeartBeatTime > timeout)
                    {
                        Logger.LogInformation($"Start disconnecting session by timeout: {session.SessionId}");
                        DisconnectSession(session.SessionId, "HeartBeat timeout");
                    }
                }
            }
        }

        private void WaitForExit()
        {
            Logger.LogInformation($"Enter any key to exit");
            Console.ReadLine();

            Stop();
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _acceptEvent.Set();

            var listenerSocket = (Socket)ar.AsyncState;
            var clientSocket = listenerSocket.EndAccept(ar);

            try
            {
                Session session = GetNewSession();
                session.ClientSocket = clientSocket;
                session.HeartBeatTime = Util.Now;

                _sessionMap[session.SessionId] = session;

                clientSocket.BeginReceive(session.Buffer, 0, _config.BufferSize, 0, new AsyncCallback(ReadCallback), session);

                if (OnConnectFunc != null)
                {
                    OnConnectFunc(session.SessionId);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"AcceptCallback Error: {e}");
                DisconnectSocket(clientSocket);
            }
            
        }
        private void ReadCallback(IAsyncResult ar)
        {
            var session = (Session)ar.AsyncState;
            if (session == null)
            {
                Logger.LogWarning($"Session is null");
                return;
            }

            try
            {
                if (session.ClientSocket == null || !session.ClientSocket.Connected)
                {
                    throw new Exception($"ClientSocket is null or not connected: {session.SessionId}");
                }

                var bytesRead = session.ClientSocket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    session.HeartBeatTime = Util.Now;
                    if (OnReceiveFunc != null)
                    {
                        var data = session.Buffer.Take(bytesRead).ToArray();
                        OnReceiveFunc(session.SessionId, data);
                    }
                    else
                    {
                        Logger.LogWarning($"OnReceiveFunc is null");
                    }

                    session.ClientSocket.BeginReceive(session.Buffer, 0, _config.BufferSize, 0, new AsyncCallback(ReadCallback), session);

                    // TODO: TotalSize > BufferSize 경우 처리 필요
                }
                else
                {
                    throw new Exception("Read 0 bytes");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"ReadCallback Exception: {e}");
                DisconnectSession(session.SessionId, e.Message);
            }
        }
        /// <summary>
        /// 세션 끊기
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="reason"></param>
        public void DisconnectSession(long sessionId, string reason)
        {
            lock (_sessionLock)
            {
                if (_sessionMap.TryGetValue(sessionId, out var session))
                {
                    Logger.LogInformation($"Disconnect Session: {session.SessionId}");
                    DisconnectSocket(session.ClientSocket);
                    
                    ReturnSession(session);
                    _sessionMap.Remove(sessionId);

                    if (OnDisconnectFunc != null)
                    {
                        OnDisconnectFunc(session.SessionId, reason);
                    }
                }
                else
                {
                    Logger.LogWarning($"Disconnect Session not exist: {sessionId}");
                }
            }
        }

        private void DisconnectSocket(Socket clientSocket)
        {
            try 
            {
                if (clientSocket != null)
                {
                    if (clientSocket.Connected)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Disconnect(true);
                        clientSocket.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"DisconnectSession Exception: {e}");
            }
        }

        /// <summary>
        /// 메시지 전송
        /// </summary>
        /// <param name="SessionId"></param>
        /// <param name="data"></param>
        public void SendSession(long SessionId, byte[] data)
        {
            if (_sessionMap.TryGetValue(SessionId, out var session))
            {
                SendSocket(session.ClientSocket, data);
            }
            else
            {
                Logger.LogWarning($"Send Session not exist: {SessionId}");
            }
        }

        private void SendSocket(Socket clientSocket, byte[] data)
        {
            if (clientSocket == null)
            {
                Logger.LogError($"SendSocket clientSocket is null");
                return;
            }

            try
            {
                if (!clientSocket.Connected)
                {
                    throw new Exception("clientSocket not connected");
                }

                clientSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), clientSocket);
            }
            catch (Exception e)
            {
                Logger.LogError($"SendSocket Exception: {e}");
                DisconnectSocket(clientSocket);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var clientSocket = (Socket)ar.AsyncState;
            if (clientSocket == null)
            {
                Logger.LogError($"ClientSocket is null");
                return;
            }

            try
            {
                var bytesSent = clientSocket.EndSend(ar);
                Logger.LogDebug($"Sent {bytesSent} bytes to client");
            }
            catch (Exception e)
            {
                Logger.LogError($"SendCallback Exception: {e}");
                DisconnectSocket(clientSocket);
            }
        }

        // 세션을 새로 만들거나 세션풀에서 로드 
        private Session GetNewSession()
        {
            Session session;
            lock (_sessionLock)
            {
                if (_freeSessionList.Count > 0)
                {
                    session = _freeSessionList[0];
                    _freeSessionList.RemoveAt(0);
                    session.SessionId = GenerateSessionId();
                }
                else
                {
                    session = new Session
                    {
                        SessionId = GenerateSessionId(),
                        Buffer = new byte[_config.BufferSize],
                    };
                }
            }
            
            return session;
        }

        // 세션을 초기화하고 세션풀에 추가
        private void ReturnSession(Session session)
        {
            lock(_sessionLock)
            {
                session.SessionId = 0;
                session.ClientSocket = null;
                for (var i = 0; i < session.Buffer.Length; i++)
                {
                    session.Buffer[i] = 0;
                }

                _freeSessionList.Add(session);
            }   
        }

        private long GenerateSessionId()
        {
            Interlocked.Increment(ref _lastSessionId);
            return _lastSessionId;
        }
    }
}
