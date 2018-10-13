This is a small library together with some simple apps used together with the Blynk server. The library is based on BlynkLibrary by
Sverre Frøystein together with standard TCP client in .NET core. The client will only use virtual pins.

In order to test this library the following requirement must be met:

1. Install Blynk application on a Iphone or Android device.

2. Install and run https://github.com/blynkkk/blynk-server. The Blynk server is a java-based application which has to be accessible to 
   to the device above.

3. Connect the device application to the Blynk server and create a new project. Add a some button with a virtual pin number.

4. Copy the authorization token for the project. Use dotnet commands to publish BlynkTester application:
   1. Go to the BlynkTester folder.
   2. Run "dotnet build"
   3. Run "dotnet publish -r [rid]". Here [rid] is specified target platform (for example , win10-x64 (for Windows 10 x64) or linux-arm (for Raspberry pi)).
   4. Go to the publish folder and run "./BlynkTester -a [token]". Here [token] is the authorization token given by the project above.
   

5. Start the device inside the smart phone application. If everything is ok, then if the button is pushed the client will notify us with a message.


With this client example it is possible to use the push or pull technology to read sensors or control any kind of software or hardware.



The BlynkRepeater application is just a UDP server together with a Blynk client. The server listens at a port and if a message of type

id value

is sent (for example, 15 33.2), then the virtual pin id will push the value to a correspoding configured device application. 

1. Create project in smartphone application and write down the authorization token. Create a "Value Display" configured to use virtual pin 15 and of Push type. 

2. Start BlynkRepeater with 
   1. Go to the BlynkRepeater folder.
   2. Run "dotnet build"
   3. Run "dotnet publish -r [rid]". Here [rid] is specified target platform (for example , win10-x64 (for Windows 10 x64) or linux-arm (for Raspberry pi)).
   4. Go to the publish folder and run "./BlynkRepeater -a [token] -s [server] -p [port]". Here [token] is the authorization token given by a project. [server] is the Blynk server application tcp-url. 
      [port] is the listening port for the udp server. 


3. Use Netcat command: 

nc 127.0.0.1 8000 -u

to send message messages like (manually send by pressing enter):

15 1000
15 101.33


4. Start the device inside the smart phone application. If everything is ok, then the values will show up in the "Value Display" widget.
