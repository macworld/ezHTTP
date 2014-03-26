using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using HttpParser;

namespace SocketManager
{
    public class AsyncUserToken
    {
        Socket m_socket;
        HttpProtocolParser m_parser;

        public AsyncUserToken(Socket socket, HttpProtocolParser httpParser)
        {
            m_socket = socket;
            m_parser = httpParser;
        }

        public AsyncUserToken()
        {

        }

        public Socket Socket
        {
            get { return m_socket; }
            set { m_socket = value; }
        }

        public HttpProtocolParser HttpParser
        {
            get { return m_parser; }
            set { m_parser = value; }
        }
    }
}
