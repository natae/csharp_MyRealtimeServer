using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyServerBase
{
    public class Session
    {
        public long SessionId;

        public byte[] Buffer;

        public Socket ClientSocket;

        public long HeartBeatTime;
    }
}
