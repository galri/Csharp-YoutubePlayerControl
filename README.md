Csharp-YoutubePlayerControl
===========================

A bridge between Youtube javascript embedded and C#. 

Contains a wpf usercontrol that shows a youtube video with support for embedded events and functions. The control support almost all the events and functions the javascript embedded does (https://developers.google.com/youtube/js_api_reference).

##"What videos can be watched?
All videos that are not private and embeddable. 

##"Under the hood.
A usercontrols that wraps a [Awesomiums WebControl](http://www.awesomium.com). The webcontrol contains a youtube embedded player which the usercontrol communicates with. 

##How to use
###How to import into to my project?
1. Download library files.
2. Follow the instructions here: http://msdn.microsoft.com/en-us/library/bb514703%28v=vs.90%29.aspx , the file that need to be selected is named "YoutubePlayerLib.dll".


###How to use the controls.
iseditble

####"xaml" properties.
properties that are meant to be set at startup. These have no effect the rest of the lifehood of the controll.
- UpstartStartVideoID
- ShowControls: Also known as chromeless. 
####normal properties and functions
- VideoID: What movie to play. [how to find id](https://www.youtube.com/watch?v=EKyirtVHsK0) 
- Quality: The quality that videos shall be played in.  If quality choosen is higher then available, the player will set the highest for the video.
- Volume. From 0 (muted) to 100. 
- PlayerStatus
- Duration
- Progress
- availableQualitys
- AutoPlay
