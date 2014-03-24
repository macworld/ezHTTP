using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
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
        static void OnReceivedData(object sender, Socket e, byte[] data)
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
            if (socketServer.Start() == false)
                Console.WriteLine("Start failed");
            socketServer.OnDataReceived += new SocketServer.ConnetionChangedEventHandler(OnReceivedHttpReq);
            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }
        static void OnReceivedHttpReq(object sender, Socket e, byte[] data)
        {


            data = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, data);
            int statusCode = 0;
            HttpProtocolParser httpParser = new HttpProtocolParser(data);
            if (httpParser.IsHttpRequest())
            {
                Console.WriteLine("\t\tresource url: " + httpParser.GetResourceUrl());
                Byte[] sendData = FileManager.FileBuffer.GetInstance().readFile(httpParser.GetResourceUrl(), ref statusCode);
                httpParser.SetStatusCode(statusCode);
                e.Send(httpParser.GetWrappedResponse(sendData));
            }

        }
    }
}
