# BigOwl
The motorized owl powered by Windows IoT and Raspberry Pi

I built an owl sculpture for a brewery in Peoria, Illinois called Bearded Owl Brewing, and designed moving parts to be controlled by stepper motos and servos.  Using this as an excuse to learn some Raspberry Pi / Windows IoT UWP programming in C#, I set out to create a controller system that would connect to Azure, Bluetooth, and whatever.  

This project has a few phases.
1) Just make some stuff move. - DONE
2) Make a standalone Touchscreen app with some simple scheduling
3) Web-enable it with a simple HTTP web server with some buttons, etc.
4) Connect it to Azure IoT Hub.  Maybe.  Who knows.
5) Connect to Facebook API for actions when people "Like" the bar's page.
6) Add LEB lights on the back of the wings, control with PWM controller
7) Profit?

Disclaimer - The source code is a patchwork of clever original ideas and stuff I Googled/Pasted.  I think the most blatant copy/pastes are annoted with attribution, but I might have missed a spot (or just haven't deleted the sample yet).  It's apparent that I really did not know what I was doing when I started.

The bar's website is here:
https://www.facebook.com/beardedowlbrewing
