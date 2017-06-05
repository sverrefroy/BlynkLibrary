The BlynkLibrary is a simplified Blynk client which supports the most basic parts of Blynk. The BlynkLibrary is a UWA DLL coded in C# and has been tested on Windows 10 (x64) and on Windows 10 IoT Core (ARM, Raspberry Pi 2).

The BlynkLibrary supports the following parts of Blynk:
  - Connecting and maintaining a connection to the Blynk server.
  - Writing of virtual pins.
  - Writing of digital pins.
  - Transmission of virtual pins.
  - Transmission of digital pins.

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
