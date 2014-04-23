using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using FileManager;
using HttpParser;
using SocketManager;
using System.Net.Sockets;
using CommonLib;
namespace ezHttp
{
    class MainLogic
    {
        public static SocketServer socketServer = new SocketServer();
        public static FileBuffer fileBuffer = FileBuffer.GetInstance();

        internal static bool Started
        {
            get { return socketServer.Strated; }
        }

        internal static bool StartService()
        {
            socketServer = new SocketServer();
            socketServer.Init();
            if (socketServer.Start() == false)
            {
                Logger log=new Logger("AppLogger");
                return false;
            }
                
            socketServer.OnDataReceived += new SocketServer.ConnetionChangedEventHandler(OnReceivedHttpReq);
            fileBuffer.Run();
            return true;
        }

        internal static void StopService()
        {
            socketServer.Stop();
            fileBuffer.Stop();
        }
        static void OnReceivedHttpReq(object sender, AsyncUserToken token, byte[] data)
        {

            int statusCode = 0;
            token.HttpParser.SetRawData(data);
            if (token.HttpParser.IsHttpRequest())
            {
                Byte[] sendData = FileManager.FileBuffer.GetInstance().readFile(token.HttpParser.GetResourceUrl(), ref statusCode);
                token.HttpParser.SetStatusCode(statusCode);
                token.Socket.Send(token.HttpParser.GetWrappedResponse(sendData));
            }

        }
    }
}
