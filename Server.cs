using System.Net.Sockets;
using System.Net;
using System.Text;

namespace TCP
{
    public class Server
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter the test code number...\n" +
                "0: Only one ping-pong with date time\n" +
                "1: Infinite server that send server message to client\n" +
                "2: Infinite server that converts messages from clients to uppercase\n" +
                "3: Multi-chatting server and client");

            switch (int.Parse(Console.ReadLine()!))
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
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 50000);
            TcpListener listener = new TcpListener(ipEndPoint);

            try
            {
                listener.Start();

                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await using NetworkStream stream = handler.GetStream();

                string message = $"{DateTime.Now}";
                byte[] dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes);

                Console.WriteLine($"Sent message: \"{message}\"");
            }
            finally
            {
                listener.Stop();
            }
        }

        public static async void RunInfinity1()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 50000);
            TcpListener listener = new TcpListener(ipEndPoint);


            try
            {
                listener.Start();

                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await using NetworkStream stream = handler.GetStream();

                string message = $"{DateTime.Now}";
                byte[] dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes);

                while (true)
                {
                    message = Console.ReadLine() ?? "";

                    byte[] bytes = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(bytes);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        public static async void RunInfinity2()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 50000);
            TcpListener listener = new TcpListener(ipEndPoint);

            try
            {
                listener.Start();

                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await using NetworkStream stream = handler.GetStream();

                while (true)
                {
                    byte[] buffer = new byte[1_024];
                    int received = await stream.ReadAsync(buffer);

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Message received: \"{message}\"");

                    message = message.ToUpper();

                    buffer = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(buffer);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        public static async void RunManyClient()
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 50000);
            TcpListener listener = new TcpListener(ipEndPoint);
            List<NetworkStream> streams = new List<NetworkStream>();

            try
            {
                listener.Start();
                Inner(listener, streams, 0);
                await Task.Delay(-1);
            }
            finally
            {
                listener.Stop();

            }
        }

        private static async void Inner(TcpListener listener, List<NetworkStream> streams, int i)
        {
            TcpClient handler = await listener.AcceptTcpClientAsync();
            streams.Add(handler.GetStream());

            Inner(listener, streams, i + 1);
            bool first = true;
            string name = "User" + new Random(DateTime.Now.Millisecond).Next(0, 1_000_000);

            while (true)
            {
                byte[] buffer = new byte[1_024];
                int received = await streams[i].ReadAsync(buffer);

                if (first)
                {
                    first = false;

                    name = Encoding.UTF8.GetString(buffer, 0, received);
                    buffer = Encoding.UTF8.GetBytes("Join the " + name);

                    Console.WriteLine($"Join: \"{name}\"");

                    foreach (NetworkStream stream in streams)
                    {
                        await stream.WriteAsync(buffer);
                    }
                }
                else
                {
                    string message = "Say " + name + ": " + Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Message received: \"{message}\"");

                    buffer = Encoding.UTF8.GetBytes(message);

                    foreach (NetworkStream stream in streams)
                    {
                        await stream.WriteAsync(buffer);
                    }
                }
            }
        }
    }
}
