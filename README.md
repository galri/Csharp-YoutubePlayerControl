Csharp-YoutubePlayerControl
===========================

A bridge between Youtube javascript Iframe api and C#. 

Contains a wpf usercontrol that shows a youtube video with support for embedded events and functions. The control support almost all the events and functions the javascript embedded does (https://developers.google.com/youtube/iframe_api_reference).

##"What videos can be watched?
All videos that are not private, embeddable and not copyright withold from showing on other sites than youtube. 

##"Under the hood.
A usercontrols that wraps a [CefSharp](https://github.com/cefsharp/CefSharp). The webcontrol contains a youtube embedded player which the usercontrol communicates with. 

##How to use
###How to import into to my project?
download source compile and reference dll's.

NB! as noted in the iframe api, trying to change Tte quality of a video, may be ignored by the iframeplayer.

###How to use the controls.
See example project. 