// See https://aka.ms/new-console-template for more information
using MyServerBase;
using System.Text;

var config = new Config
{
    IP = "127.0.0.1",
    Port = 3000,
    Backlog = 100,
    BufferSize = 2048,
    HeartBeatInterval = 30,
    HeartBeatTimeout = 120,
    LogLevel = 1,
};

// Echo server
var myServer = new Server();
myServer.OnConnectFunc = OnConnect;
myServer.OnDisconnectFunc = OnDisconnect;
myServer.OnReceiveFunc = OnReceive;

myServer.Start(config);

void OnConnect(long sessionId)
{
    Console.WriteLine($"New session is connected: {sessionId}");
}

void OnDisconnect(long sessionId, string reason)
{
    Console.WriteLine($"Session is disconnected: {sessionId}, reason: {reason}");
}

void OnReceive(long sessionId, byte[] data)
{
    var content = Encoding.UTF8.GetString(data);
    Console.WriteLine($"Received from session: {sessionId}, string content: {content}");

    myServer.SendSession(sessionId, data);
}

