using BlynkMinimalLibrary;
using System;
using System.Threading.Tasks;

namespace BlynkTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "tcp://192.168.1.210:8080"; // Local Blynk server 
            var authorization = "d634a89dbdcf4c8a8f6e5fa9df96fed2"; // Authorization token
            using (var client = new Client(url, authorization))
            {
                client.Connect();
                var tcs = new TaskCompletionSource<bool>();
                client.OnAuthorized += v => tcs.SetResult(v);
                client.Login();
                var authorized = tcs.Task.Result;
                if (authorized)
                {
                    Console.WriteLine($"Hardware client is authorized with given token");

                    client.OnVirtualWritePin += (id, value) =>
                    {
                        Console.WriteLine($"Write Pin {id} has value {value}");
                    };
                    var counter = 0;
                    client.OnVirtualReadPin += id =>
                    {
                        Console.WriteLine($"Read Pin {id}");
                        client.WriteVirtualPin(id, counter++);
                    };

                }
                else
                {
                    Console.WriteLine($"Cannot authorize client with given token.");
                }
                Console.ReadKey();
                client.Disconnect();
            }
        }
    }
}