using System.Net;
using System.Net.Sockets;

namespace Protocol
{
    public class Progarm
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("1. Normal mode\n2. Thread pooling");
            int mode = int.Parse(Console.ReadLine() ?? "0");

        Retry:
            switch (mode)
            {
                default: goto Retry;

                case 1:
                    NormalModeTester tester = new NormalModeTester();
                    tester.Run();

                    break;
                case 2:


                    break;
            }
        }
    }
}
