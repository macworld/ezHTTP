using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using FileManager;
using HttpParser;
using SocketManager;
using System.Net.Sockets;
namespace ezHttpConsoleVer
{
    class Program
    {
        static void Main(string[] args)
        {
            //SocketManagerExample();
            HttpParserExample();
        }

        static void SocketManagerExample()
        {

            SocketServer socketServer = new SocketServer();
            socketServer.Init();
            if (socketServer.Start() == false)
                Console.WriteLine("Start failed");
            socketServer.OnDataReceived += new SocketServer.ConnetionChangedEventHandler(OnReceivedData);
            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }
        static void OnReceivedData(object sender, AsyncUserToken token, byte[] data)
        {

            data = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, data);
            string tempString = Encoding.UTF8.GetString(data, 0, data.Length);

            Console.WriteLine(tempString);
            Console.WriteLine("Do anything you want");
        }

        static void HttpParserExample()
        {

            SocketServer socketServer = new SocketServer();
            socketServer.Init();
            FileBuffer FB = FileBuffer.GetInstance();
            FB.Run();
            if (socketServer.Start() == false)
                Console.WriteLine("Start failed");
            socketServer.OnDataReceived += new SocketServer.ConnetionChangedEventHandler(OnReceivedHttpReq);
            Console.WriteLine("Press any key to STOP the server process....");
            Console.ReadKey();
            socketServer.Stop();
            FB.Stop();
            Console.WriteLine("Press any key to RESTART the server process....");
            Console.ReadKey();
            FB.Run();
            socketServer.Start();
            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
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
