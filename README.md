This is a small library together with some simple apps used together with the Blynk server. The library is based on BlynkLibrary by
Sverre Frøystein together with standard TCP client in .NET core. The client will only use virtual pins.

In order to test this library the following requirement must be met:

1. Install Blynk application on a Iphone or Android device.

2. Install and run https://github.com/blynkkk/blynk-server. The Blynk server is a java-based application which has to be accessible to 
   to the device above.

3. Connect the device application to the Blynk server and create a new project. Add a some button with a virtual pin number.

4. Copy the authorization token for the project to the BlynkTester Program file (find the authorization varible). Ensure that the url
   is correct and start the BlynkTester application.

5. If everything is ok, then if the button is pushed the client will notify us with a message.


With this client it is possible to use the push or pull technology to read sensors or control any kind of software or hardware.


The BlynkRepeater application is just a UDP server together with a Blynk client. The server listens at a port and if a message of type

id value

is sent (for example, 15 33.2), then the virtual pin id will push the value to a correspoding configured device application. 




