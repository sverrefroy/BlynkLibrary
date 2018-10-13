using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BlynkRepeater
{
    public class UdpServer : IDisposable
    {
        private Task listener;
        private UdpClient udpClient;
        public UdpServer(ushort port)
        {
            this.udpClient = new UdpClient(port);
        }
        public Action<int, double> OnVirtualPin { get; set; }
        public void Start()
        {
            this.listener = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        var receiveBytes = this.udpClient.Receive(ref remoteIpEndPoint);
                        string receivedData = Encoding.UTF8.GetString(receiveBytes);
                        var split = receivedData.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            var id = int.Parse(split[0]);
                            var value = double.Parse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                            this.OnVirtualPin?.Invoke(id, value);
                        }
                        catch(Exception)
                        {
                            Console.WriteLine($"Endpoint sent {remoteIpEndPoint.Address} sent bad data {receivedData}");
                        }
                    }
                    catch(Exception)
                    {
                        break;
                    }
                }

            });
        }
        public void Stop()
        {
            this.udpClient.Close();
            this.listener.Wait();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                this.udpClient.Dispose();
                this.listener?.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
         ~UdpServer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
           Dispose(false);
         }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
             GC.SuppressFinalize(this);
        }
        #endregion

    }
}
