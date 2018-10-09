using BlynkMinimalLibrary;
using System;
using System.Threading.Tasks;

namespace BlynkRepeater
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort port = 8000; // Udp server listen to port
            var url = "tcp://192.168.1.210:8080"; // Local Blynk server
            var authorization = "d634a89dbdcf4c8a8f6e5fa9df96fed2"; // Authorization token
            using (var udpServer = new UdpServer(port))
            using (var client = new Client(url, authorization))
            {
                udpServer.Start();
                client.Connect();
                var tcs = new TaskCompletionSource<bool>();
                client.OnAuthorized += v => tcs.SetResult(v);
                client.Login();
                var authorized = tcs.Task.Result;
                if (authorized)
                {
                    Console.WriteLine("Hardware client is authorized with given token");
                    Console.WriteLine($"Listen to port {port}");
                    udpServer.OnVirtualPin += (id, value) =>
                    {
                        client.WriteVirtualPin(id, value);
                        Console.WriteLine($"Virtual pin {id} with value {value}");
                    };
                }
                else
                {
                    Console.WriteLine("Cannot authorize client with given token.");
                }
                Console.ReadKey();
                client.Disconnect();
                udpServer.Stop();
            }
        }
    }
}
