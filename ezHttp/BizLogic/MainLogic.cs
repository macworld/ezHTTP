using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using FileManager;
using HttpParser;
using SocketManager;
using System.Net.Sockets;
using CommonLib;
namespace wpfAppTest
{
    class MainLogic
    {
        static SocketServer socketServer = new SocketServer();
        static FileBuffer fileBuffer = FileBuffer.GetInstance();
        internal static void StartService()
        {
            socketServer.Init();
            if (socketServer.Start() == false)
            {
                Logger log=new Logger("AppLogger");
                log.Fatal("Filed to start service");
            }
                
            socketServer.OnDataReceived += new SocketServer.ConnetionChangedEventHandler(OnReceivedHttpReq);
            fileBuffer.Run();
        }

        internal static void RestartService()
        {
            socketServer.Start();
            fileBuffer.Run();
        }

        internal static void StopService()
        {
            socketServer.Stop();
            fileBuffer.Stop();
        }
        static void OnReceivedData(object sender, AsyncUserToken token, byte[] data)
        {

            data = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, data);
            string tempString = Encoding.UTF8.GetString(data, 0, data.Length);

            Console.WriteLine(tempString);
            Console.WriteLine("Do anything you want");
        }
        static void OnReceivedHttpReq(object sender, AsyncUserToken token, byte[] data)
        {


            data = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, data);
            int statusCode = 0;
            token.HttpParser.SetRawData(data);
            if (token.HttpParser.IsHttpRequest())
            {
                Console.WriteLine("\t\tresource url: " + token.HttpParser.GetResourceUrl());
                Byte[] sendData = FileManager.FileBuffer.GetInstance().readFile(token.HttpParser.GetResourceUrl(), ref statusCode);
                token.HttpParser.SetStatusCode(statusCode);
                token.Socket.Send(token.HttpParser.GetWrappedResponse(sendData));
            }

        }
    }
}
