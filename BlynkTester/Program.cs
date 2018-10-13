using BlynkMinimalLibrary;
using System;
using System.Threading.Tasks;
using PowerArgs;

namespace BlynkTester
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    public class Options
    {
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
            var url = options.Server; // Blynk server address
            var authorization = options.Authorization; // Authorization token
            using (var client = new Client(url, authorization))
            {
                client.Connect();
                var tcs = new TaskCompletionSource<bool>();
                client.OnAuthorized += v => tcs.SetResult(v);
                client.Login();
                var authorized = tcs.Task.Result;
                if (authorized)
                {
                    Console.WriteLine("Hardware client is authorized with given token");

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
                    var closeTcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                    Console.CancelKeyPress += (o, e) =>
                    {
                      closeTcs.SetResult(true);
                    };
                    Console.WriteLine("Test server is active. Press CTRL+C to stop.");
                    closeTcs.Task.Wait();
                    Console.WriteLine("Stopping Test server.");
                }
                else
                {
                    Console.WriteLine("Cannot authorize client with given token.");
                }
                client.Disconnect();
            }
        }
    }
}
