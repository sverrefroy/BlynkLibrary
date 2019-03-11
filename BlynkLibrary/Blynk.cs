////////////////////////////////////////////////////////////////////////////
//
//  This file is part of BlynkLibrary
//
//  Copyright (c) 2017, Sverre Frøystein
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of 
//  this software and associated documentation files (the "Software"), to deal in 
//  the Software without restriction, including without limitation the rights to use, 
//  copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
//  Software, and to permit persons to whom the Software is furnished to do so, 
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all 
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//  SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;


namespace BlynkLibrary
{
    /// <summary>
    /// This is the main Blynk client class. It handles the connection to Blynk and all communication to and from.
    /// </summary>
    public class Blynk
    {
        #region Public definitions
        public static Dictionary<WidgetProperty, string> WProperty = new Dictionary<WidgetProperty, string>()
        {
            { WidgetProperty.Color,     "color"},
            { WidgetProperty.Label,     "label"},
            { WidgetProperty.Max,       "max"},
            { WidgetProperty.Min,       "min"},
            { WidgetProperty.OnLabel,   "onLabel"},
            { WidgetProperty.OffLabel,  "offLabel"},
            { WidgetProperty.IsEnabled, "isEnabled"},
            { WidgetProperty.IsOnPlay,  "isOnPlay"}
        };

        /// <summary>
        /// This handler is triggered when a virtual pin is received from Blynk.
        /// </summary>
        public event VirtualPinReceivedHandler VirtualPinReceived;

        /// <summary>
        /// This id the virtual pin received event handler definition.
        /// </summary>
        /// <param name="b">Reference to the Blynk instance sending the event.</param>
        /// <param name="e">The event arguments for the virtual pin.</param>
        public delegate void VirtualPinReceivedHandler( Blynk b, VirtualPinEventArgs e );

        /// <summary>
        /// This handler is triggered when a digital pin is received from Blynk.
        /// </summary>
        public event DigitalPinReceivedHandler DigitalPinReceived;

        /// <summary>
        /// This id the digital pin received event handler definition.
        /// </summary>
        /// <param name="b">Reference to the Blynk instance sending the event.</param>
        /// <param name="e">The event arguments for the digital pin.</param>
        public delegate void DigitalPinReceivedHandler( Blynk b, DigitalPinEventArgs e );

        /// <summary>
        /// This property returns the connected state of the Blynk client.
        /// </summary>
        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        #endregion

        #region Private definitions
        private bool connected = false;
        private const int pingInterval = 5; // 5 seconds ping keep alive interval

        private TcpClient tcpClient = null;

        private string Authentication;
        private string Server;
        private int    Port;
        private Stream tcpStream;
        private int    txMessageId;

        private static ThreadPoolTimer blynkTimer;

        byte[] rcBuffer = new byte[ 100 ];

        #endregion

        #region Public methods

        /// <summary>
        /// This is the Blynk constructor.
        /// </summary>
        /// <param name="authentication">The Blynk authentication token.</param>
        /// <param name="server">Sever connection string url.</param>
        /// <param name="port">The server port to connect to.</param>
        public Blynk( string authentication, string server, int port )
        {
            Authentication = authentication;
            Server = server;
            Port = port;
        }

        /// <summary>
        /// This method will try to establish a connection to the Blynk server.
        /// </summary>
        /// <returns>'True' if a connection is established, 'False' if not.</returns>
        public bool Connect()
        {
            bool result = false;
            int connectTimeoutMilliseconds = 1000;

            try
            {
                tcpClient = new TcpClient();

                tcpClient.NoDelay = true;

                var connectionTask = tcpClient.ConnectAsync( Server, Port ).ContinueWith( task =>
                {
                    return task.IsFaulted ? null : tcpClient;
                }, TaskContinuationOptions.ExecuteSynchronously );

                var timeoutTask = Task.Delay( connectTimeoutMilliseconds ).ContinueWith<TcpClient>( task => null, TaskContinuationOptions.ExecuteSynchronously );
                var resultTask = Task.WhenAny( connectionTask, timeoutTask ).Unwrap();

                resultTask.Wait();
                var resultTcpClient = resultTask.Result;

                if ( resultTcpClient != null )
                {
                    tcpStream = tcpClient.GetStream();

                    txMessageId = 1;

                    List<byte> txMessage = new List<byte>() { 0x02 };

                    txMessage.Add( ( byte )( txMessageId >> 8 ) );
                    txMessage.Add( ( byte )( txMessageId ) );
                    txMessage.Add( ( byte )( Authentication.Length >> 8 ) );
                    txMessage.Add( ( byte )( Authentication.Length ) );

                    foreach ( char c in Authentication )
                    {
                        txMessage.Add( ( byte )c );
                    }

                    tcpStream.Write( txMessage.ToArray(), 0, txMessage.Count );

                    readTcpStream();

                    connected = true;

                    Task.Run( new Action( blynkReceiver ) );

                    result = true;
                }
                else
                {
                    // Not connected
                }
            }
            catch ( Exception )
            {
            }

            // Create a timer to handle the ping/reconnection period
            TimerElapsedHandler blynkTimerElapsedTimer = new TimerElapsedHandler( timer_Tick );

            blynkTimer = ThreadPoolTimer.CreatePeriodicTimer( timer_Tick, new TimeSpan( 0, 0, 5 ) );

            return result;
        }

        /// <summary>
        /// This method will disconnect from the Blynk server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                blynkTimer.Cancel();

                connected = false;

                tcpStream.Dispose();
                tcpClient.Dispose();
            }
            catch ( Exception )
            {
            }
        }

        /// <summary>
        /// This is the ping sender.
        /// </summary>
        public void SendPing()
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { ( byte )Command.PING };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );

                txMessage.Add( ( byte )( 0 ) );
                txMessage.Add( ( byte )( 0 ) );

                WriteToTcpStream( txMessage );
            }
        }

        /// <summary>
        /// This is the virtual pin sender.
        /// </summary>
        /// <param name="vp">The virtual pin to send.</param>
        public void SendVirtualPin( VirtualPin vp )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { (byte)Command.HARDWARE };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );
                PrepareVirtualWrite( vp, txMessage );

                WriteToTcpStream( txMessage );
            }
        }

        public static void PrepareVirtualWrite( VirtualPin vp, List<byte> txMessage )
        {
            txMessage.Add( ( byte )'v' );
            txMessage.Add( ( byte )'w' );
            txMessage.Add( 0x00 );

            txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( vp.Pin.ToString() ) );

            txMessage.Add( 0x00 );

            foreach ( object o in vp.Value )
            {
                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( o.ToString().Replace( ',', '.' ) ) );
                txMessage.Add( 0x00 );
            }

            txMessage.RemoveAt( txMessage.Count - 1 );

            int msgLength = txMessage.Count - 3;

            txMessage.Insert( 3, ( byte )( ( msgLength ) >> 8 ) );
            txMessage.Insert( 4, ( byte )( ( msgLength ) ) );
        }

        /// <summary>
        /// This is the digital pin sender.
        /// </summary>
        /// <param name="dp">The digital pin to send.</param>
        public void SendDigitalPin( DigitalPin dp )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { ( byte )Command.HARDWARE };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );

                string pin = dp.Pin.ToString();

                int msgLength = pin.Length + 5;

                txMessage.Add( ( byte )( ( msgLength ) >> 8 ) );
                txMessage.Add( ( byte )( ( msgLength ) ) );

                txMessage.Add( ( byte )'d' );
                txMessage.Add( ( byte )'w' );
                txMessage.Add( 0x00 );

                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( pin.ToString() ) );

                txMessage.Add( 0x00 );

                if ( dp.Value )
                {
                    txMessage.Add( ( byte )'1' );
                }
                else
                {
                    txMessage.Add( ( byte )'0' );
                }

                WriteToTcpStream( txMessage );
            }
        }

        /// <summary>
        /// This is the widget set property sender
        /// </summary>
        /// <param name="vp">The virtual pin with properties to send.</param>
        public void SetProperty( VirtualPin vp )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage  = new List<byte>();
                int        startCount = 0;

                foreach ( Tuple<object, object> p in vp.Property )
                {
                    startCount = txMessage.Count;

                    txMessage.Add( (byte)Command.SET_WIDGET_PROPERTY );
                    txMessage.Add( ( byte )( txMessageId >> 8 ) );
                    txMessage.Add( ( byte )( txMessageId ) );

                    txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( vp.Pin.ToString() ) );
                    txMessage.Add( 0x00 );
                    txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( p.Item1.ToString() ) );
                    txMessage.Add( 0x00 );
                    txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( p.Item2.ToString() ) );

                    int msgLength = ( txMessage.Count - startCount ) - 3;

                    txMessage.Insert( startCount + 3, ( byte )( ( msgLength ) >> 8 ) );
                    txMessage.Insert( startCount + 4, ( byte )( ( msgLength ) ) );
                }

                WriteToTcpStream( txMessage );
            }
        }

        /// <summary>
        /// Reads a virtual pin.
        /// </summary>
        /// <param name="vp">The virtual pin to read.</param>
        public void ReadVirtualPin( VirtualPin vp )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { ( byte )Command.HARDWARE_SYNC };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );
                txMessage.Add( ( byte )'v' );
                txMessage.Add( ( byte )'r' );
                txMessage.Add( 0x00 );

                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( vp.Pin.ToString() ) );

                int msgLength = txMessage.Count - 3;

                txMessage.Insert( 3, ( byte )( ( msgLength ) >> 8 ) );
                txMessage.Insert( 4, ( byte )( ( msgLength ) ) );

                WriteToTcpStream( txMessage );
            }
        }

        internal void BridgeVirtualWrite( int b, VirtualPin vp )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { ( byte )Command.BRIDGE };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );
                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( b.ToString() ) );
                txMessage.Add( ( byte )( 0x00 ) );

                PrepareVirtualWrite( vp, txMessage );

                WriteToTcpStream( txMessage );
            }
        }
        internal void BridgeDigitalWrite( int b, DigitalPin dp )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { ( byte )Command.BRIDGE };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );
                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( b.ToString() ) );
                txMessage.Add( ( byte )( 0x00 ) );
                txMessage.Add( ( byte )( 'd' ) );
                txMessage.Add( ( byte )( 'w' ) );
                txMessage.Add( ( byte )( 0x00 ) );
                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( dp.Pin.ToString() ) );
                txMessage.Add( ( byte )( 0x00 ) );
                txMessage.Add( (byte)(dp.Value ? '1' : '0' ) );

                int msgLength = txMessage.Count - 3;

                txMessage.Insert( 3, ( byte )( ( msgLength ) >> 8 ) );
                txMessage.Insert( 4, ( byte )( ( msgLength ) ) );

                WriteToTcpStream( txMessage );
            }
        }

        internal void BridgeSetAuthToken( int p, string auth )
        {
            if ( Connected )
            {
                txMessageId++;
                List<byte> txMessage = new List<byte>() { ( byte )Command.BRIDGE };

                txMessage.Add( ( byte )( txMessageId >> 8 ) );
                txMessage.Add( ( byte )( txMessageId ) );
                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( p.ToString() ) );
                txMessage.Add( ( byte )( 0x00 ) );
                txMessage.Add( ( byte )( 'i' ) );
                txMessage.Add( ( byte )( 0x00 ) );
                txMessage.AddRange( ASCIIEncoding.ASCII.GetBytes( auth ) );

                int msgLength = txMessage.Count - 3;

                txMessage.Insert( 3, ( byte )( ( msgLength ) >> 8 ) );
                txMessage.Insert( 4, ( byte )( ( msgLength ) ) );

                WriteToTcpStream( txMessage );
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Tis is the timer tick event handler for the tcp keep alive process which
        /// sends a ping at a regular basis.
        /// </summary>
        /// <param name="timer">The timer which triggered this event</param>
        private void timer_Tick( ThreadPoolTimer timer )
        {
            if ( Connected )
            {
                SendPing();
            }
            else
            {
                timer.Cancel();

                tcpStream.Dispose();
                tcpClient.Dispose();

                Connect();
            }
        }

        /// <summary>
        /// This is the tcp receiver background tread which reads incomming tcp data.
        /// </summary>
        private void blynkReceiver()
        {
            while ( Connected )
            {
                readTcpStream();
            }
        }

        /// <summary>
        /// This is the tcp reader which performs a synchronos read from tcp, splits it into Blynk messages
        /// and sends the messages to the decoder.
        /// </summary>
        private void readTcpStream()
        {
            UInt16 rcMessageId = 0;

            int count = tcpStream.Read( rcBuffer, 0, 100 );

            if ( count > 0 )
            {
                List<byte> rcMessage = new List<Byte>( rcBuffer );

                rcMessage = rcMessage.GetRange( 0, count );

                while ( rcMessage.Count >= 5 )
                {
                    byte[] lengthBytes = rcMessage.GetRange( 3, 2 ).ToArray();
                    lengthBytes = lengthBytes.Reverse().ToArray();

                    UInt16 messageLength = 0;

                    if ( rcMessage[ 0 ] != ( byte )Command.RESPONSE )
                    {
                        messageLength = BitConverter.ToUInt16( lengthBytes, 0 );
                    }

                    if ( rcMessage.Count >= 5 + messageLength )
                    {
                        decodeMessage( rcMessage.GetRange( 0, 5 + messageLength ) );

                        rcMessage.RemoveRange( 0, 5 + messageLength );
                    }
                    else
                    {
                        rcMessage.Clear();
                    }
                }

                if ( rcMessageId != 0 )
                {
                    SendResponse( rcMessageId );
                }
            }
        }

        /// <summary>
        /// This is the Blynk message decoder.
        /// </summary>
        /// <param name="rcMessage">The Blynk message to decode</param>
        private UInt16 decodeMessage( List<byte> rcMessage )
        {
            UInt16 messageLength = ( UInt16 )( rcMessage.Count - 5 );
            Command cmd = ( Command )rcMessage[ 0 ];
            byte[] receivedIdBytes = rcMessage.GetRange( 1, 2 ).ToArray();

            receivedIdBytes = receivedIdBytes.Reverse().ToArray();

            UInt16 rcMessageId = BitConverter.ToUInt16( receivedIdBytes, 0 );

            switch ( cmd )
            {
                case Command.RESPONSE:
                    break;

                case Command.BRIDGE:
                case Command.HARDWARE:
                    var elements = System.Text.Encoding.ASCII.GetString( rcMessage.GetRange( 5, messageLength ).ToArray() ).Split( ( char )0x00 );

                    if ( elements[ 0 ] == "vw" )
                    {
                        var e = new VirtualPinEventArgs();

                        e.Data.Pin = byte.Parse( elements[ 1 ] );
                        e.Data.Value.Clear();

                        for ( int i = 2; i <= elements.Length - 1; i++ )
                        {
                            int value;
                            if ( int.TryParse( elements[ i ], out value ) )
                            {
                                e.Data.Value.Add( value );
                            }
                            else
                            {
                                e.Data.Value.Add( elements[ i ] );
                            }
                        }

                        VirtualPinReceived( this, e );
                    }

                    if ( elements[ 0 ] == "dw" )
                    {
                        var e = new DigitalPinEventArgs();

                        e.Data.Pin = byte.Parse( elements[ 1 ] );
                        e.Data.Value = int.Parse( elements[ 2 ] ) == 1;

                        DigitalPinReceived( this, e );
                    }

                    break;

                default:
                    break;
            }

            return rcMessageId;
        }

        /// <summary>
        /// This is the message response sender.
        /// </summary>
        /// <param name="mId">The Id of the message responding to.</param>
        private void SendResponse( int mId )
        {
            if ( Connected )
            {
                List<byte> txMessage = new List<byte>() { ( byte )Command.RESPONSE };

                txMessage.Add( ( byte )( mId >> 8 ) );
                txMessage.Add( ( byte )( mId ) );

                txMessage.Add( ( byte )( 0 ) );
                txMessage.Add( ( byte )( Response.OK ) );

                WriteToTcpStream( txMessage );
            }
        }

        /// <summary>
        /// This is the tcp writer.
        /// </summary>
        /// <param name="txMessage">The Blynk message to send to tcp.</param>
        private void WriteToTcpStream( List<byte> txMessage )
        {
            try
            {
                tcpStream.Write( txMessage.ToArray(), 0, txMessage.Count );

            }
            catch ( IOException )
            {
                connected = false;
            }
        }

        #endregion


    }
}
