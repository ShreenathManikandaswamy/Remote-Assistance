YOUR REMOTE ASSISTANCE
----------------------
Thanks for downloading this package.

This packages contains the full project of the Your Remote Assistance example.

This is not an Augmented Reality project. Both ARCore and Vuforia don't offer any option 
to send the video feed to a RTMP server, so in this project we are just using the video stream 
without any image processing related to augmented reality operation.

It will be a matter of time that any AR SDK will make it easy to send the video stream 
to an RTMP so you will have the expected remote assistance the way it's meant to be.

As a key points, this software allows to organize a session where multiple customers
running Android devices can video stream at the same time and multiple experts can
be in the same session allowing easy options for them to switch between the different 
video feed of the multiple clients connected.

You can find out more of our projects in:
http://www.yourvrexperience.com

CREDITS
--------
This application uses Open Source components. You can find the source code of their open source projects 
along with license information below. We acknowledge and are grateful to these developers for 
their contributions to open source.

Project: rtmp-rtsp-stream-client-java https://github.com/pedroSG94/rtmp-rtsp-stream-client-java
License (Apache License 2.0) https://github.com/pedroSG94/rtmp-rtsp-stream-client-java/blob/master/LICENSE.txt	
	
TUTORIAL
--------

	Your will find the video tutorial in:
	
		https://youtu.be/9BMib8im8BI

 0. SET UP A RTMP STREAM SERVER[OPTIONAL]:
 
	Before starting with this tutorial you should get a RTMP server online at your service.
	Our platform of choice has been DigitalOcean. There are easy tutorials to set up your
	RTMP streaming service there like the one provided next:
    
    https://obsproject.com/forum/resources/how-to-set-up-your-own-private-rtmp-server-using-nginx.50/

	Anyway, to complete this tutorial is it possible to do it without having a RTMP stream server,
	the only thing that will happen is that instead of having a video feed you will see 
	only a black screen.
	
	If you want to activate the streaming just use the preprocessor constant ENABLE_STREAMING.

 1. INSTALLATION OF FREE SDKs:
 
	The first step to do in the tutorial is to download these free plugins:
	
		**Volumetric Lines**:
		
			https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160
		
			This asset will allow you to paint nice colored lines that will allow
		the participants of the assistance session to send the right indications.
	
		**Facebook SDK**:
	
			https://developers.facebook.com/docs/unity/downloads
		
			This SDK will allow you to organize remote assistance sessions with your Facebook friends.	
			This step is also optional and you ignore the import of this SDK by removing 
		the preprocessor constant ENABLE_FACEBOOK.

  2. INSTALLATION OF PAID SDK:
  
	To be able to stream you will need a video player capable of playing a RTMP stream.
	In this case we are using Universal Media Player. This step is optional if you are
	not planning to stream.
	
	If you want to activate the player just use the preprocessor constant ENABLE_STREAMING.

  3. BUILDING AND RUNNING CLIENT:

	3.1. Now, import YourRemoteAssistance.unitypackage.

	3.2. Make sure that both scenes are in the building settings.
	
	3.3. In the case that you are running a RTMP server and planning to stream enable the
		preprocessor constant ENABLE_STREAMING and in the file GameConfiguration.cs
		set the constant URL_RTMP_SERVER_ASSISTANCE to your own IP address.

	3.3. Next we are going to run the client. 
  
		Just to ease the testing, you can do it without the need to make a build for Android, you can just run in the Editor and you are good to go. 
		Just remember that you will get a black screen in the streaming window.
	  
		Make a build for Android and run the app in the device.
		
	3.4. Then you can create a new local service there and select that you are going to be the user who needs assistance.
		
		Specify your phone number so the expert who is going to assist you can call you back.
		
		It will run using UNET, so give it some seconds until you see that the server has been initialized.
		
	3.5. If you are operating with streaming enabled then in some seconds you will get the video feed that
		your device is sending to your RTMP stream server. 
		
		What you are watching right now it's not the direct
		camera access but the RTMP stream, so there is going to be a delay that will depend on the quality of
		your RTMP server and Internet connection.

  5. Now we can run the instance of Unity that will be the expert. 
  
	We join the local service and we select that we are going to join as the expert who is going to provide service.
	
	Then you will see that a new item has appeared in the list of connection. You have to give it a couple of seconds
	to synchronize so you can select it and see the video stream of the client.
  
  6. So, now both client and server can start drawing lines to help each other for the remote assistance.
  
	6.1. You can delete the lines you have created with the delete button.
	
	6.2. Remember that this is not augmented reality, so if you move your device the lines everbody involved in the
		session won't make sense.
	
	6.3. This asset will be improved in the future when any of the existing Augmented Reality SDK for Unity provides
	of an easy way to send the video feed to a RTMP server.
   
  7. The most important key feature of this software is that you can create as many clients and experts as you
	want for the session, so you can get together multiple points of view.
   
  8. Finally, in this tutorial you can see about what you have to do to run the whole thing using sockets.
	Nothing changes aside the service organization that is transparent to the developer.
  
		https://youtu.be/DC_N353dLV8
		
		
   

		
		