using System.Net.Sockets;
using System.Net;
using System.Text.Unicode;
using System.Text;

namespace Protocol
{
    public class NormalModeTester
    {
        public void Run()
        {
            Thread server = new Thread(() =>
            {
                new BaseServer().Start();
            });
            Thread client = new Thread(() =>
            {
                new BaseClient().Connect();
            });

            server.Start();
            client.Start();

            server.Join();
        }
    }

    public class BaseServer
    {
        public TcpListener Listener { get; set; } = new TcpListener(IPAddress.Parse("127.0.0.1"), 55556);
        public List<TcpClient> Clients { get; set; } = new List<TcpClient>();
        public List<NetworkStream> NetworkStreams { get; set; } = new List<NetworkStream>();

        private bool listen_mode = false;
        private bool run_mode = false;

        public void Start()
        {
            this.listen_mode = true;
            this.run_mode = true;

            new Thread(Listening).Start();
            new Thread(Messaging).Start();

            while (true)
            {
                Console.WriteLine("Exit? [y/n]");

                if (Console.ReadLine()!.ToLower().Equals("y"))
                {
                    this.listen_mode = false;
                    this.run_mode = false;

                    break;
                }
                else
                {
                    continue;
                }
            }
        }

        private async void Listening()
        {
            this.Listener.Start();

            while (this.listen_mode)
            {
                TcpClient client = await this.Listener.AcceptTcpClientAsync();

                this.Clients.Add(client);
                this.NetworkStreams.Add(client.GetStream());
            }

            this.Listener.Stop();
        }

        private async void Messaging()
        {
            while (this.run_mode) 
            {
                for (int i = 0; i < this.NetworkStreams.Count; i++)
                {
                    if (this.Clients[i].Connected) 
                    {
                        NetworkStream stream = this.NetworkStreams[i];

                        byte[] buffer = new byte[1024];

                        if (this.Clients[i].Connected)
                        {
                            await stream.ReadAsync(buffer);
                            buffer = Encoding.UTF8.GetBytes(
                                Encoding.UTF8.GetString(buffer) + DateTime.Now.ToString(" - HH:mm:ss.FFF\0"));

                            await stream.WriteAsync(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        this.Clients.RemoveAt(i);
                        this.NetworkStreams.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class BaseClient
    {
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }

        public BaseClient()
        {
            this.Client = new TcpClient();
        }

        public async void Connect()
        {
            await this.Client.ConnectAsync(IPAddress.Parse("127.0.0.1"), 55556);
            this.Stream = this.Client.GetStream();

            for (int i = 0; i < 10; i++)
            {
                byte[] buffer = Encoding.UTF8.GetBytes("Hello Network");

                await this.Stream.WriteAsync(buffer, 0, buffer.Length);
                await this.Stream.ReadAsync(buffer);

                string result = Encoding.UTF8.GetString(buffer);

                Console.WriteLine(result);
            }

            this.Client.Close();
        }
    }
}