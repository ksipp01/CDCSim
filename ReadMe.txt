ASCOM Camera Simulaotr with Cartes du Ceil Screen Capture ond realtime Focus Simulation 

Based on The Original Version Published by Bob Denny, Chris Rowland and Matthias Busch.  

--------------------------------------------------------------------------------------------------------------
Per the Original Simulator framework : 
Install in the usual way.

The Setup dialog specifies the camera properties in the usual way.

The CCD temperature regulation will adjust the power to keep the temperature close to that specified.

The simulation is generated as follows:

An image can be specified. I've only tried a jpg image but any normal image type can beloaded. It expects a colour image.
The image is loaded into an image data array with the camera type and bayer offsets applied:
For monochrome the brightness is used.
For RGGB, CYMG, CMYG2 and LRGB the appropriate colours and bayer offsets are used.
This hasn't been fully tested.
All image values are scaled to 0 to 255.

At the end of an exposure the image array is extracted from the image data using the start, num and bin values, and multiplied by the exposure time in seconds.

If noise is specified then an offset of 3 plus dark current calculated from the exposure time and ccd temperature, assuming 1 ADU at 0 deg C and halving or doubling for every 5 deg difference is added and the value this gives used to get a poisson distributed value, for values over 50 a normal distribution is used.
The result is clipped to the Max ADU value and put into the image array.

If the shutter exists and is closed then the image data is not added but the noise and dark current is.

It passes the current conform, except for an error that's something to do with Conform.

The V2 properties are only available late bound, I'm not sure about the others, I had to change the the ClassInterfaceType from None to AutoDispatch to get the late bound properties.

I'd be interested to hear if this is useful.

Chris Rowland
---------------------------------------------------------------------------------------------------------------
Additional Features: 


Real-time Focus Simulation: 
This Camera Driver Simulator MUST (for now) connect to an ASCOM focuser and will apply image blurring based on focuser position.  

To Use:  You must press the Properties button when connecting to the driver, then the Focus chooser will display, select your ASCOM Focuser. 
The camera setup dialog with then display.  Define the Focus Point position and the Step Size.  The Step size is the amount of focus position change that will cause blurring. For example, with a setting of 100, each 100 step change in focus position will result in an incremental increased blurring)  The image will only blur to 10 times the step size, further focuser movement will not change the image.     

Cartes du Ciel (CDC) Screen Capture and telescope tracking for plate solve simulation.    
CDC A free planetarium program (https://www.ap-i.net/skychart/en/start) that can connect to an ASCOM Mount and follow the mount.  This can be used for testing automation software plate solving.  
This works best with dual displays but can be done on one.  
Set up as follows: 
Open CDC, Connect to ASCOM Mount.  Use Setup -> Display -> Finder rectangle to define a FOV that matches your Imaging chip.  Adjust rotation so the rectangle is square in the FOV.  Set the CDC FOV (Setup -> set FOV) so the CCD rectangle is completely visible in the center of the entire FOV and takes up about 50% of the entire FOV, make Sure CDC is maximized(full screen) AND on Primary display, for now.  If only using 1 display, remember about where the bounds of the CCD rectangle are, you will have imaging application and setup windows over part of CDC, that's ok) Make sure "Screen Capture" is checked. Press the Set button then Left click and drag the mouse over the CCD FOV rectangle, then double click to set.  Click Ok to close setup, then OK to close chooser.  The CDC window can now be moved to secondary display now but needs to be maximized to preserve the FOV selection.  
If CDC capture is not wanted, but focus simulation is, Push Select Image and navigiate to a desired image.  Adjust the CCD Height and Width in the Setup menu as needed.  (The Screen Capture check box will automatically uncheck when selecting a specific image).  

 
(4-30-2019)  

     
