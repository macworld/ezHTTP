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
            SocketManagerExample();
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

    }
}
