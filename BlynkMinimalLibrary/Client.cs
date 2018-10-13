using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BlynkMinimalLibrary
{
    public class Client : IDisposable
    {
        private string url;
        private string authentication;
        private TcpClient client;
        private ushort messageIndex = 0;
        private Task listener;
        private NetworkStream stream;
        private Timer pingTimer;

        public Action<int, double> OnVirtualWritePin { get; set; }
        public Action<int> OnVirtualReadPin { get; set; }
        public Action<bool> OnAuthorized { get; set; }
        public Client(string url, string authorization)
        {
            this.url = url;
            this.authentication = authorization;
            this.client = new TcpClient();
            this.pingTimer = new Timer(5000);
            this.pingTimer.Elapsed += (o, e) => this.SendPing();
        }
        public void Send(Command command, params string[] values)
        {
            this.messageIndex++;
            if (this.messageIndex == ushort.MaxValue)
            {
                this.messageIndex = 1;
            }

            var valueBytesLength = 0;
            byte[] valueBytes = null;
            if (values != null)
            {
                var message = string.Join("\0", values);
                valueBytes = UTF8Encoding.UTF8.GetBytes(message);
                valueBytesLength = valueBytes.Length;
            }
            var all = new byte[1 + 2 + 2 + valueBytesLength];
            all[0] = (byte)command;
            var messageBytes = BitConverter.GetBytes(this.messageIndex);
            Array.Reverse(messageBytes);
            var valueLengthBytes = BitConverter.GetBytes((ushort)valueBytesLength);
            Array.Reverse(valueLengthBytes);
            Array.Copy(messageBytes, 0, all, 1, 2);
            Array.Copy(valueLengthBytes, 0, all, 3, 2);
            if (valueBytes != null)
                Array.Copy(valueBytes, 0, all, 5, valueBytesLength);

            if (this.client.Connected && this.stream.CanWrite)
            {
                this.stream.Write(all, 0, all.Length);
            }
        }

        public void WriteVirtualPin(int id, double value)
        {
            this.Send(Command.HARDWARE, "vw", id.ToString(), value.ToString(CultureInfo.InvariantCulture));
        }
        public void SendPing()
        {
            this.Send(Command.PING, null);
        }
        public void Connect()
        {
            var uri = new Uri(this.url);
            this.client.Connect(uri.Host, uri.Port);
            this.stream = this.client.GetStream();
            if (this.stream.CanRead)
            {
                this.listener = Task.Run(() =>
                {
                    var bytes = new byte[this.client.ReceiveBufferSize];
                    var offset = 0;

                    while (this.client.Connected)
                    {
                        // Reads NetworkStream into a byte buffer.
                        int read = 0;
                        try
                        {
                            read = this.stream.Read(bytes, offset, bytes.Length - offset);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                        offset += read;

                        if (read == 0)
                            break;
                        while (offset >= 5)
                        {
                            var command = (Command)bytes[0];
                            var messageBytes = new byte[2];
                            Array.Copy(bytes, 1, messageBytes, 0, 2);
                            Array.Reverse(messageBytes);
                            var messageIndex = BitConverter.ToUInt16(messageBytes, 0);
                            if (command == Command.RESPONSE)
                            {
                                var responseCodeBytes = new byte[2];
                                Array.Copy(bytes, 3, responseCodeBytes, 0, 2);
                                Array.Reverse(responseCodeBytes);
                                var responseCode = (Response)BitConverter.ToUInt16(responseCodeBytes, 0);
                                if (messageIndex == 1)
                                {
                                    if (responseCode == Response.OK || responseCode == Response.USER_ALREADY_REGISTERED)
                                    {
                                        this.pingTimer.Start();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    this.OnAuthorized?.Invoke(responseCode == Response.OK || responseCode == Response.USER_ALREADY_REGISTERED);
                                }
                                offset -= 5;
                                Array.Copy(bytes, 5, bytes, 0, offset); // Move back
                            }
                            else if (command == Command.HARDWARE)
                            {
                                var messageLengthBytes = new byte[2];
                                Array.Copy(bytes, 3, messageLengthBytes, 0, 2);
                                Array.Reverse(messageLengthBytes);
                                var messageLength = BitConverter.ToUInt16(messageLengthBytes, 0);
                                if (messageLength <= offset)
                                {
                                    var message = new byte[messageLength];
                                    Array.Copy(bytes, 5, message, 0, messageLength);
                                    offset -= 5 + messageLength;
                                    Array.Copy(bytes, 5 + messageLength, bytes, 0, offset); // Move back

                                    var messageString = UTF8Encoding.UTF8.GetString(message);
                                    var split = messageString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
                                    var pinType = split[0];
                                    var pinId = int.Parse(split[1]);
                                    if (pinType == "vw")
                                    {
                                        var pinValue = double.Parse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture);
                                        this.OnVirtualWritePin?.Invoke(pinId, pinValue);
                                    }
                                    else if (pinType == "vr")
                                    {
                                        this.OnVirtualReadPin?.Invoke(pinId);
                                    }

                                }
                            }
                            else
                            {
                                Console.WriteLine($"Not supported command {command}");
                            }
                        }

                    }
                });
            }
        }

        public void Disconnect()
        {
            this.pingTimer.Stop();
            this.client.Close();
        }
        public void Login()
        {
            this.Send(Command.LOGIN, this.authentication);
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
                this.Disconnect();
                this.stream.Dispose();
                this.listener.Wait();
                this.listener.Dispose();
                this.client.Dispose();
                this.pingTimer.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Client()
        {
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
