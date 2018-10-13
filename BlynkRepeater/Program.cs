using BlynkMinimalLibrary;
using System;
using System.Threading.Tasks;
using PowerArgs;

namespace BlynkRepeater
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class Options
    {
        [ArgShortcut("p")]
        [ArgShortcut("--port")]
        [ArgDefaultValue(8000)]
        [ArgDescription("Listening port for the Udp server.")]
        public ushort Port { get; set; }

        [ArgShortcut("s")]
        [ArgShortcut("--server")]
        [ArgDefaultValue("tcp://127.0.0.1:8080")]
        [ArgDescription("The url to the Blynk server.")]
        public string Server { get; set; }

        [ArgShortcut("a")]
        [ArgShortcut("--authorization")]
        [ArgDefaultValue("****")]
        [ArgDescription("The device authorization token used to identitify the repeater.")]
        public string Authorization { get; set; }

    }


    class Program
    {
        static void Main(string[] args)
        {
            Options options = null;
            try 
            {
               options = Args.Parse<Options>(args);
            }
            catch (ArgException e)
            {
              Console.WriteLine(string.Format("Problems with the command line options: {0}", e.Message));
              Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<Options>());
              return;
            }            
            ushort port = options.Port; // Udp server listen to port
            var url = options.Server; // Blynk server address
            var authorization = options.Authorization; // Authorization token
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
//                        Console.WriteLine($"Virtual pin {id} with value {value}");
                    };
                    var closeTcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                    Console.CancelKeyPress += (o, e) =>
                    {
                      closeTcs.SetResult(true);
                    };
                    Console.WriteLine("Repeater is active. Press CTRL+C to stop.");
                    closeTcs.Task.Wait();
                    Console.WriteLine("Stopping repeater.");   
                }
                else
                {
                    Console.WriteLine("Cannot authorize client with given token.");
                }
                client.Disconnect();
                udpServer.Stop();
            }
        }
    }
}
