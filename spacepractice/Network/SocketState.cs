using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Text;

namespace Network
{
    public class SocketState
    {
        public byte[] buffer = new byte[0x1000];
        public const int BufferSize = 0x1000;
        public Action<SocketState> call_me;
        public string error_message = "";
        public bool error_occured;
        public StringBuilder sb = new StringBuilder();
        public long uid = -1L;
        public Socket workSocket;
    }
}
