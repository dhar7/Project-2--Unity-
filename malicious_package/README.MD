Encoding
=========================================================================================================

First create a Unity Project

Then create an empty gameobject and attach the Encode.cs file to it

In the inspector of the empty gameobject, in the variable space, paste the contents of ServerO.cs.

Run the game

box_embedded.jgp will be generated



Creating Malicious Package
=========================================================================================================

Export Packages combined with box_embedded.jgp + Launcher.cs, name it for example Malicious_Package.package

Import Malicious_Package.package

Imoported Malicious_Package.package will be automatically extracted into box_embedded.jgp and Launcher.cs

The successful import will generate ServerO.cs and then attach ServerO.cs to the Main Camera of the project.

Boom!


Important Tip
=========================================================================================================


Tune int R = 19520 in Launcher.cs; based on the length of the Steganographed file ServerO.cs. Here, based on an empirical basis, it is 19520.




