using System;
using System.IO;
using FileManager;
using SocketManager;
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
            Logger log = new Logger("AppLogger");
            if (!Directory.Exists(FileManager.Properties.FileManagerSettings.Default.ServerDirectory))
            {
                log.Error("Start failed: Server Directory doesn't exist!");
                return false;
            }
            socketServer = new SocketServer();
            socketServer.Init();
            if (socketServer.Start() == false)
            {
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
