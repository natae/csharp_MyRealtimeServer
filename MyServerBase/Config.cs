using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServerBase
{
    public class Config
    {
        public string IP = "127.0.0.1";

        public int Port = 3000;

        public int Backlog = 100;

        public int BufferSize = 1024;

        /// <summary>
        /// seconds
        /// </summary>
        public int HeartBeatTimeout = 10;

        /// <summary>
        /// seconds
        /// </summary>
        public int HeartBeatInterval = 5;
    }
}
