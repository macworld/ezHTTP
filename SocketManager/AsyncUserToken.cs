using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace SocketManager
{
    public class AsyncUserToken
    {
        Socket m_socket;

        public AsyncUserToken(Socket socket)
        {
            m_socket = socket;
        }

        public AsyncUserToken() : this(null) { }

        public Socket Socket
        {
            get { return m_socket; }
            set { m_socket = value; }
        }
    }
}
