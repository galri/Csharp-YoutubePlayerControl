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
####"xaml" properties.
properties that are meant to be set at startup, as in the design view. These properties have no effect the rest of the lifehood of the controll.
- UpstartStartVideoID
- ShowControls: False is also known as chromeless player. 

####normal properties and functions
The controls "iseditble" must be true for the normal properties to work, there is also an event that fires when iseditble becomes true, called "BecomeEditble".
- VideoID: What movie to play. [how to find id](https://www.youtube.com/watch?v=EKyirtVHsK0) 
- Quality: The quality that videos shall be played in.  If quality choosen is higher then available, the player will set the highest for the video.
- Volume. From 0 (muted) to 100. 
- PlayerStatus: What state the player is in. 
  1. unstarted
  2. ended
  3. playing
  4. paused
  5. buffering
  6. video cued
- Duration: How long the video is in seconds.
- Progress: How long video has progressed in seconds.
- availableQualitys: lists all the qualitys the video can use.
- AutoPlay: 

####Events
Events for each status change:
  1. VideoUnstarted
  2. VideoEnded
  3. VideoPlaying
  4. VideoPaused
  5. VideoBuffering
  6. VideoCued
  
Events for errors:
  1. VideoUnEmbeddableError
  2. VideoNotFoundError

And a Event for when normal properties can be used:
  1. BecomeEditble
