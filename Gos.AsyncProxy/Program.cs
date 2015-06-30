using System;
using System.Net;
using Mono.Options;

namespace GOS.AsyncProxy
{
    class Program
    {
        static void Main(string[] args)
        {
IPAddress ip = null;
int port = 0;

var p = new OptionSet()
    .Add("a|address=", v => ip = IPAddress.Parse(v))
    .Add("p|port=", v => port = int.Parse(v));

p.Parse(args);

            var proxy = new Proxy
            {
                ListenToIp = ip,
                ListenOnPort = port
            };
            proxy.Start();

            Console.WriteLine("Proxy is started ans listening on {0}:{1}", proxy.ListenToIp, proxy.ListenOnPort);
            Console.WriteLine("Press 'Q' to stop the proxy");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        proxy.Stop();
                        Console.WriteLine("Proxy is stopped");
                        break;
                    }
                }
            }

        }
    }
}
