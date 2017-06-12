The BlynkLibrary is a simplified Blynk client which supports the most basic parts of Blynk. The BlynkLibrary is a UWA DLL coded in C# and has been tested on Windows 10 (x64) and on Windows 10 IoT Core (ARM, Raspberry Pi 2). The project may be built on Visual Studio 2017 Community or similar.

The BlynkLibrary supports the following parts of Blynk:
  - Connecting and maintaining a connection to the Blynk server.
  - Writing of virtual pins from app/server to Blynk client.
  - Writing of digital pins from app/server to Blynk client.
  - Transmission of virtual pins from Blynk client to app/server.
  - Transmission of digital pins from Blynk client to app/server.
  - Read request for virtual pins from Blynk client to app/server. (Thanks to Chernega)
  - Setting widget properties from Blynk client to app/server. (Thanks to Chernega)
  - Sending virtual and digital pins to a different device through a bridge.

To use this together with the Blynk app you may not use the "Read" setting, but use "Push" and rely on the application using the BlynkLibrary, to push the values.

To include it in your project you need add the following code:

        void myInitMethod()
        {
             Int32 port = 8442;
             string server = "blynk-cloud.com";

             var AUTH = "ADD YOUR TOKEN HERE";

             Blynk myBlynk = new Blynk( AUTH, server, port );

             myBlynk.VirtualPinReceived += MyBlynk_VirtualPinReceived;
             myBlynk.DigitalPinReceived += MyBlynk_DigitalPinReceived;

             bool connected = myBlynk.Connect();
        }

        private void MyBlynk_DigitalPinReceived( Blynk b, DigitalPinEventArgs e )
        {
            // Handle your received digital pin here
        }

        private void MyBlynk_VirtualPinReceived( Blynk b, VirtualPinEventArgs e )
        {
            // Handle your received virtual pin here
        }
  
  In addition the Blynk client offers the following API:
  
        /// <summary>
        /// This is the Blynk constructor.
        /// </summary>
        /// <param name="authentication">The Blynk authentication token.</param>
        /// <param name="server">Sever connection string url.</param>
        /// <param name="port">The server port to connect to.</param>
        public Blynk( string authentication, string server, int port );

        /// <summary>
        /// This method will try to establish a connection to the Blynk server.
        /// </summary>
        /// <returns>'True' if a connection is established, 'False' if not.</returns>
        public bool Connect()
        
        /// <summary>
        /// This is the virtual pin sender.
        /// </summary>
        /// <param name="vp">The virtual pin to send.</param>
        public void SendVirtualPin( VirtualPin vp );
        
        /// <summary>
        /// This is the digital pin sender.
        /// </summary>
        /// <param name="dp">The digital pin to send.</param>
        public void SendDigitalPin( DigitalPin dp )
        
        /// <summary>
        /// This is the widget set property sender
        /// </summary>
        /// <param name="vp">The virtual pin with properties to send.</param>
        public void SetProperty( VirtualPin vp )
        
        /// <summary>
        /// Reads a virtual pin.
        /// </summary>
        /// <param name="vp">The virtual pin to read.</param>
        public void ReadVirtualPin( VirtualPin vp )
        

