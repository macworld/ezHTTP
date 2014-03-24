using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using SocketManager;
using System.Net.Sockets;
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
            if(server.Start(localEndPoint) == false)
                Console.WriteLine("Start failed");
            server.OnDataReceived += new Server.ConnetionChangedEventHandler(OnReceivedData);
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

    }
}
