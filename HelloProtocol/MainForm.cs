using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HelloTCP
{
    public partial class MainForm : Form
    {
        private Button tcp_start_button;
        private Button udp_start_button;
        private DataGridView grid;
     
        public MainForm()
        {
            InitializeComponent();

            this.tcp_start_button = new Button()
            {
                Parent = this,
                Visible = true,
                Location = new Point(5, 5),
                Size = new Size((this.ClientSize.Width - 5) / 2, 50),
                Text = "TCP",
            };
            this.tcp_start_button.Click += TCPTester;

            this.udp_start_button = new Button()
            {
                Parent = this,
                Visible = true,
                Location = new Point(10 + this.tcp_start_button.Width, 5),
                Size = new Size(this.ClientSize.Width / 2 - 15, 50),
                Text = "UDP",
            };
            this.udp_start_button.Click += UDPTester;


            this.grid = new DataGridView()
            {
                Parent = this,
                Visible = true,
                Location = new Point(5, 60),
                Size = new Size(this.ClientSize.Width - 10, this.ClientSize.Height - 65),
            };
            this.grid.Columns.Add("Protocol", "Protocol");
            this.grid.Columns.Add("Start", "Start");
            this.grid.Columns.Add("Connect", "Connect");
            this.grid.Columns.Add("Push", "Push");
            this.grid.Columns.Add("Finish", "Finish");
            this.grid.Columns.Add("Loss", "Loss");
            this.grid.Columns.Add("Total", "Total");
        }

        private void UDPTester(object? sender, EventArgs e)
        {
            DateTime[] time = new DateTime[4];
            int i = 0;

            time[i++] = DateTime.Now;

            IPEndPoint ip = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 45678);

            byte[] buffer = Encoding.UTF8.GetBytes("Hello, world!");
            int count = 0;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ip);
            
            time[i++] = DateTime.Now;

            object locker = new object();
            bool send_while = true;

            Thread send = new Thread(() =>
            {
                while (true)
                {
                    socket.SendTo(buffer, ip);
                    count++;

                    lock (locker)
                    {
                        if (send_while == false)
                            break;
                    }
                }
            });

            Thread recv = new Thread(() =>
            {
                for (int idx = 0; idx < 10; idx++)
                {
                    EndPoint broad = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 45678);
                    socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.Broadcast, ref broad);
                }
                lock (locker) { send_while = false; }
            });


            send.Start();
            recv.Start();

            recv.Join();

            time[i++] = DateTime.Now;

            socket.Close();

            time[i++] = DateTime.Now;

            this.grid.Rows.Add("UDP",
                               $"{time[0].ToString("HH:mm:ss")}",
                               $"{(time[1].Ticks - time[0].Ticks) / 1000.0} ms",
                               $"{(time[2].Ticks - time[1].Ticks) / 1000.0} ms",
                               $"{(time[3].Ticks - time[2].Ticks) / 1000.0} ms",
                               $"{((count - 10)  / (double)count * 100).ToString("00.000")}%",
                               $"{(time[3].Ticks - time[0].Ticks) / 1000.0} ms");
        }

        private async void TCPTester(object? sender, EventArgs e)
        {
            DateTime[] time = new DateTime[4];
            int i = 0;

            time[i++] = DateTime.Now;

            IPEndPoint ip = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 45678);
            TcpListener listener = new TcpListener(ip);

            TcpClient client = new TcpClient();

            listener.Start();
            await client.ConnectAsync(ip);

            NetworkStream stream = client.GetStream();
            byte[] buffer = Encoding.UTF8.GetBytes("Hello, world!");

            time[i++] = DateTime.Now;

            Thread send = new Thread(() =>
            {
                for (int idx = 0; idx < 10; idx++)
                {
                    stream.WriteAsync(buffer);
                }
            });
            
            Thread recv = new Thread(() =>
            {
                for (int idx = 0; idx < 10; idx++)
                {
                    stream.ReadAsync(buffer);
                }
            });

            recv.Start();
            send.Start();

            send.Join();
            recv.Join();

            time[i++] = DateTime.Now;

            client.Close();
            listener.Stop();

            time[i++] = DateTime.Now;

            this.grid.Rows.Add("TCP",
                               $"{time[0].ToString("HH:mm:ss")}",
                               $"{(time[1].Ticks - time[0].Ticks) / 1000.0} ms",
                               $"{(time[2].Ticks - time[1].Ticks) / 1000.0} ms",
                               $"{(time[3].Ticks - time[2].Ticks) / 1000.0} ms",
                               "0%",
                               $"{(time[3].Ticks - time[0].Ticks) / 1000.0} ms");    
        }
    }
}