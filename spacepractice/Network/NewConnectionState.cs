using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Network
{


    public class NewConnectionState
    {
        public NewConnectionState(Action<SocketState> call, TcpListener l)
        {
            this.callMe = call;
            this.listener = l;
        }

        //added:
        public Action<SocketState> callMe { get;}
        public TcpListener listener { get;}

        //what were these supposed to do?:
        //if you see this in the future replace with properties like above
        //public Action<SocketState> callMe =>
        //    this.callMe;

        //public TcpListener listener =>
        //    this.listener;
    }
}

