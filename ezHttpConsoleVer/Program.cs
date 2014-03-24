using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using SocketManager;
using System.Net.Sockets;
using FileManager;
using HttpParser;

namespace ezHttpConsoleVer
{
    class Program
    {
        static void Main(string[] args)
        {
            int numConnections;
            int receiveSize;
            IPEndPoint localEndPoint;
            int port;


            numConnections = 1024;
            receiveSize = 64;
            port = 8080;
            localEndPoint = new IPEndPoint(IPAddress.Any, port);
            // Start the server listening for incoming connection requests
            Server server = new Server(numConnections, receiveSize);
            server.Init();
            server.Start(localEndPoint);
            server.OnDataReceived += new Server.ConnetionChangedEventHandler(OnReceivedData);
            Console.WriteLine("Press any key to terminate the server process....");
            Console.ReadKey();
        }

        static void OnReceivedData(object sender, Socket e, byte[] data)
        {

            data = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, data);
            string tempString = Encoding.UTF8.GetString(data, 0, data.Length);
            int statusCode = 0;
            HttpProtocolParser httpParser = new HttpProtocolParser(data);
            if (httpParser.IsHttpRequest())
            {
                Console.WriteLine("\t\tresource url: " + httpParser.GetResourceUrl());
                Byte[] sendData = FileManager.FileBuffer.GetInstance().readFile(httpParser.GetResourceUrl(), ref statusCode);
                httpParser.SetStatusCode(statusCode);
                e.Send(httpParser.GetWrappedResponse(sendData));
            }
            Console.WriteLine(tempString);
            Console.WriteLine("Do anything you want");
        }

    }
}
