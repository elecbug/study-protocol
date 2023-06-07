using System.Net.Sockets;
using System.Net;
using System.Text;
using System;

namespace TCP
{
    public class Client
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter the test code number...\n" +
                "0: Only one ping-pong with date time\n" +
                "1: Infinite server that send server message to client\n" +
                "2: Infinite server that converts messages from clients to uppercase\n" +
                "3: Multi-chatting server and client");

            switch(int.Parse(Console.ReadLine()!))
            {
                case 0: RunPingPong(); break;
                case 1: RunInfinity1(); break;
                case 2: RunInfinity2(); break;
                case 3: RunManyClient(); break;
            }

            while (true) ;
        }

        public static async void RunPingPong()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 50000);

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(ipEndPoint);
            await using NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1_024];
            int received = await stream.ReadAsync(buffer);

            string message = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Message received: \"{message}\"");
        }

        public static async void RunInfinity1()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 50000);

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(ipEndPoint);
            await using NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[1_024];
                int received = await stream.ReadAsync(buffer);

                string message = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine($"Message received: \"{message}\"");
            }
        }

        public static async void RunInfinity2()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 50000);

            using TcpClient client = new TcpClient();
            await client.ConnectAsync(ipEndPoint);
            await using NetworkStream stream = client.GetStream();

            while (true)
            {
                string message = Console.ReadLine() ?? "";

                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer);

                int received = await stream.ReadAsync(buffer);
                message = Encoding.UTF8.GetString(buffer, 0, received);

                Console.WriteLine($"Service Uppercase: \"{message}\"");

            }
        }

        public static async void RunManyClient()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 50000);

            TcpClient client = new TcpClient();
            await client.ConnectAsync(ipEndPoint);
            NetworkStream stream = client.GetStream();

            Console.Write("Enter the name: ");
            byte[] buffer = Encoding.UTF8.GetBytes(Console.ReadLine() ?? "");
            await stream.WriteAsync(buffer);
            
            new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    byte[] buffer = new byte[1_024];

                    int received = await stream.ReadAsync(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, received);

                    Console.WriteLine(message);
                }
            })).Start();

            new Thread(new ThreadStart(async () =>
            {
                while (true)
                {
                    string message = Console.ReadLine() ?? "";

                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(buffer);
                }
            })).Start();
        }
    }
}