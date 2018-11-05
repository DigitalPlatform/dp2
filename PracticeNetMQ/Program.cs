using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading;

namespace PracticeNetMQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("PracticeNetMQ");

            // 询问按照 Server 还是 Client 方式启动
            Console.WriteLine("选择运行方式:(s/c)");
            string type = Console.ReadLine();
            if (type == "s")
            {
                using (var server = new ResponseSocket())
                {
                    server.Bind("tcp://*:5555");

                    while (true)
                    {
                        var message = server.ReceiveFrameString();

                        Console.WriteLine("Received {0}", message);

                        // processing the request
                        Thread.Sleep(100);

                        Console.WriteLine("Sending World");
                        server.SendFrame("World");
                    }
                }
            }
            else if (type == "c")
            {
                using (var client = new RequestSocket())
                {
                    client.Connect("tcp://localhost:5555");

                    for (int i = 0; i < 10; i++)
    {
                        Console.WriteLine("Sending Hello");
                        client.SendFrame("Hello");

                        var message = client.ReceiveFrameString();
                        Console.WriteLine("Received {0}", message);
                    }
                }
            }
            else
            {
                Console.WriteLine("错误的运行方式:" + type);
            }
        }
    }
}
