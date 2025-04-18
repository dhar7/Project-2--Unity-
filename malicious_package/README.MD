Encoding
=========================================================================================================

First create a Unity Project

Then create an empty gameobject and attach the Encode.cs file to it

In the inspector of the empty gameobject, in the variable space, paste the contents of ServerObfuscated.cs.

Run the game

box_embedded.jpg will be generated



Creating a Malicious Package
=========================================================================================================

Export Packages combined with box_embedded.jpg + LauncherObfuscated.cs, name it for example Malicious_Package.package

Import Malicious_Package.package

Imoported Malicious_Package.package will be automatically extracted into box_embedded.jgp and Launcher.cs

The successful import will generate ServerObfuscated.cs and then attach ServerObfuscated.cs to the Main Camera of the project.

Boom!


Setting Up Malicious Server


Windows Powershell ==> .\ngrok.exe http --domain=crab-loved-hen.ngrok-free.app 3000
cd path_to_malicious_server.js ==> node malicious_server.js


Important Tip
=========================================================================================================

Tune int R = 19520 in LauncherObfuscated.cs; based on the length of the Steganographed file ServerObfuscated.cs. Here, based on an empirical basis, it is 19520.





