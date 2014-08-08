using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace YoutubePlayerLib
{
    //TODO: error handling. from javascript on properties.
    //TODO: list upp properties that require idEditble.

    //things that may be supportet.
    //TODO: playback rate
    //TODO: playlist.
    //TODO: Chromeless to be supported after initiaten
    /// <summary>
    /// Interaction logic for YoutubePlayer.xaml
    /// 
    /// Youtube recomends the window to be a minimum of 480px width and 270 in height.
    /// Before any communication be done that involves youtube player, make sure that Is.Editble == true. else it will be ignored. can also use the IsEditble event.
    /// This dosen't count properties, methods  or events inherited from usercontrol. 
    /// </summary>
    /// <remarks>
    /// open control in toolbox: http://msdn.microsoft.com/en-us/library/bb514703%28v=vs.90%29.aspx
    /// See http://wiki.awesomium.net/general-use/initialization-sequence.html#wpf-webcontrol for web events firing and when to communicate with the view.
    /// see https://developers.google.com/youtube/js_api_reference for javascript communication.
    /// 
    /// </remarks>
    public partial class YoutubePlayer : UserControl
    {
        /// <summary>
        /// Wether changes, that effects the webview inside the userconbtrol, wil have an effect
        /// </summary>
        public bool IsEditble { get { return web.IsLive; } }
        #region htmlsource 
        /// <summary>
        /// The source for webview
        /// </summary>
        private string htmlSource = @"<html>
            <head>
            <meta charset='UTF-8' />
            <style type='text/css'>
            body {
                overflow:hidden;
            }
            #player{height:95%; width: 100%;}
            </style>
            </head>
            <body>
            <div id='player'></div>
	            <script type='text/javascript' src='http://www.youtube.com/player_api'></script>
	            <script type='text/javascript'>
                    //holds on to player object
                    var player;

                    //create youtubeplayer
                    function onYouTubePlayerAPIReady() {
                        player = new YT.Player('player', {
                          height: '100px',
                          width: '100px',	
                          videoId: jsobject['upstartVideoId'],
		                  frameborder: '1',
		                  playerVars: { 'autoplay': 0, 'controls': jsobject['control'] },
                          events: { 
                            'onReady': onPlayerReady,
                            'onStateChange': onPlayerStateChange,
			                'onError': onPlayerError
                          }
                        });
                    }

                    // autoplay video
                    function onPlayerReady(event) {
		                jsobject.playerReady();
                    }

                    // when video ends
                    function onPlayerStateChange(event) { 
		                jsobject.stateChanged(event.data);
                    }
	
	                function onPlayerError(errorCode){
		                jsobject.error(errorCode.data);
	                }
	
	                function cueVideo(id, quality){
		                //ready's the next movie.
		                player.cueVideoById(id,0,quality);
		                //hides splash just in case.
		                hideSplash();
	                }
	
	                //change video
	                function changePlayerVideo(id,quality){
		                player.loadVideoById(id,0,quality);
		                hideSplash();
	                }
	
	                function changePlayerQuality(quality){
		                player.setPlaybackQuality(quality);
	                }
	
	                function getPlayerUrl(){
		                return player.getVideoUrl();
	                }
	
	                function showSplash(){
		                document.getElementById('splash').style.zIndex = '+10';
	                }
	
	                function hideSplash(){
		                document.getElementById('splash').style.zIndex = '-10';
	                }

                    function test(){
                        alert(jsobject['control']);
                    }
	            </script>
            </body></html>";
        #endregion
        /// <summary>
        /// Need to remember the qualurt since youtube player dont, it will set back to default every time video changes.
        /// </summary>
        private YoutubeQuality myPrefQuality = YoutubeQuality.@default;
        /// <summary>
        /// Gets and sets the preferd qualtity to play in. Default value will let the player try to determined the beat quality based on screen space and internett speed.
        /// 
        /// To get the quality the player is using, see the propertie <seealso cref="CurrentPlayingQuality"/>
        /// 
        /// If quality choosen is higher then available, the player will set the highest for the video .
        /// 
        /// what Youtube has to say about quality:
        /// Your client should not automatically switch to use the highest (or lowest) quality video or to any unknown format name. YouTube could expand the list of quality levels to include formats that may not be appropriate in your player context. Similarly, YouTube could remove quality options that would be detrimental to the user experience. By ensuring that your client only switches to known, available formats, you can ensure that your client's performance will not be affected by either the introduction of new quality levels or the removal of quality levels that are not appropriate for your player context.
        /// </summary>
        public YoutubeQuality Quality
        {
            get { return myPrefQuality; }
            set
            {
                if (web.IsLive)
                {
                    myPrefQuality = value;
                    web.ExecuteJavascript("changePlayerQuality('" + myPrefQuality + "')");
                }
            }
        }
        /// <summary>
        /// Gets the quality the player are using.
        /// </summary>
        public YoutubeQuality CurrentPlayingQuality
        {
            get
            {
                if (web.IsLive)
                {
                    JSValue qs = web.ExecuteJavascriptWithResult("player.getPlaybackQuality()");
                    if (!qs.IsUndefined && qs.IsString)
                    {
                        return toYoutubeQuality((string)qs);
                    }
                }
                return YoutubeQuality.unknownvalue;
            }
        }
        private string myUpstartVideoID;
        /// <summary>
        /// sets what video to load first. if value is not sett at startup. a splash image will be shown.
        /// </summary>
        public string UpstartVideoID { 
            set 
            {
                myUpstartVideoID = value;
            }
        }
        /// <summary>
        /// Set and gets the ID of video playing. 
        /// 
        /// get will return a empty string a few seconds after a song is changed, this happends while the javascript player is loading the video.
        /// </summary>
        public string VideoID
        {
            get
            {
                if (web.IsLive)
                {
                    JSValue url = web.ExecuteJavascriptWithResult("getPlayerUrl();");
                    if (!url.IsUndefined && url.IsString)
                    {
                        Uri idpath = new Uri((string)url, UriKind.Absolute);
                        System.Collections.Specialized.NameValueCollection querys = idpath.ParseQueryString();
                        string id = querys["v"];
                        return id;
                    }
                }
                return "";

            }
            set
            { 
                if (web.IsLive)
                {
                    if (AutoPlay)
                    {
                        web.ExecuteJavascript("changePlayerVideo('" + value + "','" + Quality + "')");
                    }
                    else
                    {
                        web.ExecuteJavascript("cueVideo('" + value + "','" + Quality + "')");
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets the Volume of player, values ranging from 0 to 100.
        /// 
        /// Get will return -1 if the controller is not ready yet or if something wrong happens. (IsEditble == false).
        /// set accepts value ranging from 0 up to 100. Another value is ignored.
        /// </summary>
        public int Volume
        {
            get
            {
                if (web.IsLive)
                {
                    JSValue volume = web.ExecuteJavascriptWithResult("player.getVolume()");
                    if (!volume.IsUndefined && volume.IsInteger)
                    {
                        return (int)volume;
                    }
                }
                //web view is not running yet.
                return -1;

            }
            set
            {
                if (web.IsLive && value < 101 && value > -1)
                {
                    web.ExecuteJavascript("player.setVolume(" + value + ");");
                }
            }
        }
        /// <summary>
        /// Get the state of the youtube player.
        /// 
        /// returns unkownvalue if IsEditble is false, can't get value from player or gets an unknown value from player.
        /// </summary>
        public YoutubePlayerState playerStatus
        {
            get
            {
                if (web.IsLive)
                {
                    //throw new InvalidOperationException("The web view is not live, IsEditble must be true before this propertie can be run");
                    JSValue state = web.ExecuteJavascriptWithResult("player.getPlayerState();");
                    if (!state.IsUndefined && state.IsInteger)
                    {
                        //throw new ValueUnavailableException("Value not recived from the player, player may not fully loaded after changing video."); 
                        ////should get a value from the switch. unless youtube updates its api.
                        switch ((int)state)
                        {
                            case -1:
                                return YoutubePlayerState.unstarted;
                            case 0:
                                return YoutubePlayerState.ended;
                            case 1:
                                return YoutubePlayerState.playing;
                            case 2:
                                return YoutubePlayerState.paused;
                            case 3:
                                return YoutubePlayerState.buffering;
                            case 5:
                                return YoutubePlayerState.videoCued;
                        }
                    }
                }
                return YoutubePlayerState.unknownvalue;
            }

        }
        /// <summary>
        /// Returns the length in seconds of the currently playing video. Note that getDuration() will return 0 until the video's metadata is loaded, which normally happens just after the video starts playing.
        /// </summary>
        public Double Duration
        {
            get
            {
                if (web.IsLive)
                {
                    JSValue volume = web.ExecuteJavascriptWithResult("player.getDuration()");
                    if (!volume.IsUndefined && volume.IsNumber)
                    {
                        return (int)volume;
                    }
                }
                return -1;
            }
        }
        /// <summary>
        /// Returns how long the video has progress from the start in seconds.
        /// </summary>
        public int Progress
        {
            get
            {
                if (web.IsLive)
                {
                    //throw new InvalidOperationException("The web view is not live, IsEditble must be true before this propertie can be run");
                    JSValue progress = web.ExecuteJavascriptWithResult("player.getCurrentTime();");
                    if (!progress.IsUndefined && progress.IsNumber)
                    {
                        return (int)progress;
                    }
                }
                return -1;
            }
        }
        /// <summary>
        /// This function returns the set of quality formats in which the current video is available. You could use this function to determine whether the video is available in a higher quality than the user is viewing, and your player could display a button or other element to let the user adjust the quality.
        /// </summary>
        public List<YoutubeQuality> availableQualitys
        {
            get
            {
                if (web.IsLive)
                {
                    JSValue qs = web.ExecuteJavascriptWithResult("player.getAvailableQualityLevels()");
                    if (!qs.IsUndefined && qs.IsArray)
                    {
                        List<YoutubeQuality> qsl = new List<YoutubeQuality>();
                        foreach (JSValue quality in (Array)qs)
                        {
                            qsl.Add(toYoutubeQuality((string)quality));
                        }
                        return qsl;
                    }
                }
                return new List<YoutubeQuality>();
            }
        }
        /// <summary>
        /// player.getPlaybackRate():Number
        /// player.setPlaybackRate(suggestedRate:Number):Void
        /// </summary>
        private bool myAutoPlay = false;
        /// <summary>
        /// </summary>
        public bool AutoPlay
        {
            get
            {
                return myAutoPlay;
            }
            set
            {
                myAutoPlay = value;
            }
        }
        private bool myShowControls = true;
        /// <summary>
        /// Works only at startup.
        /// also known as Chrome or CHromeless.
        /// </summary>
        public bool ShowControls
        {
            set
            {
                myShowControls = value;
            }
        }

        private JSObject jsobject;
        
        public YoutubePlayer()
        {
            InitializeComponent();
        }

        public void test(){
            web.ExecuteJavascript("test();");
        }

        /// <summary>
        /// Play the current video.
        /// </summary>
        public void play()
        {
            if (web.IsLive)
            {
                hideSplash();
                web.ExecuteJavascript("player.playVideo();");
            }
        }

        /// <summary>
        /// Pause the current video.
        /// </summary>
        public void pause()
        {
            if (web.IsLive)
            {
                web.ExecuteJavascript("player.pauseVideo();");
            }
        }

        /// <summary>
        /// Hide the player and show the splash image.
        /// </summary>
        private void showSplash()
        {
            web.ExecuteJavascript("showSplash();");
        }

        /// <summary>
        /// Hide splash image and show the player.
        /// </summary>
        private void hideSplash()
        {
            web.ExecuteJavascript("hideSplash();");
        }

        #region javascriptevents
        /// <summary>
        /// https://developers.google.com/youtube/js_api_reference#Playback_status 
        /// Used by the browser only.
        /// fires of the right wpf event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void youtubeStateChanged(object sender, JavascriptMethodEventArgs args)
        {
            if (!args.Arguments[0].IsUndefined && args.Arguments[0].IsInteger)
            {
                //MessageBox.Show(args.Arguments[0]);
                switch ((int)args.Arguments[0])
                {
                    case -1:
                        //unstarted (first loads a video,)
                        raiseVideoUnstartedEvent();
                        break;
                    case 0:
                        //ended
                        raiseVideoOverEvent();
                        break;
                    case 1:
                        //playing
                        raiseVideoStartedEvent();
                        break;
                    case 2:
                        //paused
                        raiseVideoPausedEvent();
                        break;
                    case 3:
                        //buffering
                        raiseVideoBufferingEvent();
                        break;
                    case 5:
                        //cued (ready to play)
                        raiseVideoCuedEvent();
                        break;
                }
            }
        }

        /// <summary>
        /// https://developers.google.com/youtube/js_api_reference#Events onError part.
        /// used by the web browser only.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void youtubeError(object sender, JavascriptMethodEventArgs args)
        {
            switch ((int)args.Arguments[0])
            {
                case 2:
                    //The request contains an invalid parameter value.
                    //MessageBox.Show("ulovlig verdi sendt over");
                    raiseVideoInvalidValueError();
                    break;
                case 100:
                    //The video requested was not found. This error occurs when a video has been removed (for any reason) or has been marked as private.
                    //MessageBox.Show("Video er privat, eller har blitt fjernet");
                    RaiseVideoNotFoundErrorEvent();
                    break;
                case 101:
                case 150:
                    // The owner of the requested video does not allow it to be played in embedded players.
                    //MessageBox.Show("Ikke lovlig å vise video embedded");
                    RaiseVideoUnEmbeddableErrorEvent();
                    break;
            }
        }

        private void youtubeReady(object sender, JavascriptMethodEventArgs args)
        {
            //if startid value exist, set it up, else show splash image.
            if (myUpstartVideoID != null)
            {
                //no video to start with, show splash.
                VideoID = myUpstartVideoID;
            }
            //player values that are not upstart can no be editet.
            raiseIsEditbleEvent();
        }


        #endregion

        #region WPFevents

        private void web_NativeViewInitialized(object sender, WebViewEventArgs e)
        {
            //set up connection to javascript and pass over values.
            jsobject = web.CreateGlobalJavascriptObject("jsobject");
            jsobject.Bind("stateChanged", false, youtubeStateChanged);
            jsobject.Bind("error", false, youtubeError);
            jsobject.Bind("playerReady", false, youtubeReady);
            
            jsobject["control"] = myShowControls ? 1 : 0;
            jsobject["upstartVideoId"] = myUpstartVideoID == null ? "": myUpstartVideoID;

            //sets source.
            web.LoadHTML(htmlSource);
        }

        private void web_InitializeView(object sender, WebViewEventArgs e)
        {
            //web.Source = new Uri("/source.html", UriKind.Relative);
            
        }

        #endregion        
        
        #region inner class and etc

        /// <summary>
        /// A helper function used to convert string values recived from the webclient into Youtubequality enum values.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static YoutubeQuality toYoutubeQuality(string value)
        {
            switch (value)
            {
                case "default":
                    return YoutubeQuality.@default;
                case "tiny":
                    return YoutubeQuality.tiny;
                case "small":
                    return YoutubeQuality.small;
                case "medium":
                    return YoutubeQuality.medium;
                case "large":
                    return YoutubeQuality.large;
                case "highres":
                    return YoutubeQuality.highres;
                case "hd720":
                    return YoutubeQuality.hd720;
                case "hd1080":
                    return YoutubeQuality.hd1080;
                default:
                    return YoutubeQuality.unknownvalue;
            }
        }
        #endregion

        #region created events

        #region state

        #region video unstarted
        public static readonly RoutedEvent VideoUnstartedEvent = EventManager.RegisterRoutedEvent("VideoUnstarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoUnstarted
        {
            add { AddHandler(VideoUnstartedEvent, value); }
            remove { RemoveHandler(VideoUnstartedEvent, value); }
        }
        void raiseVideoUnstartedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoUnstartedEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region video over
        public static readonly RoutedEvent VideoOverEvent = EventManager.RegisterRoutedEvent("VideoOver", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoOver
        {
            add { AddHandler(VideoOverEvent, value); }
            remove { RemoveHandler(VideoOverEvent, value); }
        }
        void raiseVideoOverEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoOverEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region started playing
        public static readonly RoutedEvent VideoStartedEvent = EventManager.RegisterRoutedEvent("VideoStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoStarted
        {
            add { AddHandler(VideoStartedEvent, value); }
            remove { RemoveHandler(VideoStartedEvent, value); }
        }
        void raiseVideoStartedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoStartedEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region video paused
        public static readonly RoutedEvent VideoPausedEvent = EventManager.RegisterRoutedEvent("VideoPaused", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoPaused
        {
            add { AddHandler(VideoPausedEvent, value); }
            remove { RemoveHandler(VideoPausedEvent, value); }
        }
        void raiseVideoPausedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoPausedEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region video buffering
        public static readonly RoutedEvent VideoBufferingEvent = EventManager.RegisterRoutedEvent("VideoBuffering", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoBuffering
        {
            add { AddHandler(VideoBufferingEvent, value); }
            remove { RemoveHandler(VideoBufferingEvent, value); }
        }
        void raiseVideoBufferingEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoBufferingEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region video cued
        public static readonly RoutedEvent VideoCuedEvent = EventManager.RegisterRoutedEvent("VideoCued", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoCued
        {
            add { AddHandler(VideoCuedEvent, value); }
            remove { RemoveHandler(VideoCuedEvent, value); }
        }
        void raiseVideoCuedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoCuedEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #endregion

        #region Error

        #region Invalid Value (Nr:2)
        public static readonly RoutedEvent VideoInvalidValueErrorEvent = EventManager.RegisterRoutedEvent("VideoInvalidValueError", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoInvalidValueError
        {
            add { AddHandler(VideoInvalidValueErrorEvent, value); }
            remove { RemoveHandler(VideoInvalidValueErrorEvent, value); }
        }
        void raiseVideoInvalidValueError()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoInvalidValueErrorEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #region Video not found (removed or private) (Nr:100)

        public static readonly RoutedEvent VideoNotFoundErrorEvent = EventManager.RegisterRoutedEvent("VideoNotFoundError", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoNotFoundError
        {
            add { AddHandler(VideoNotFoundErrorEvent, value); }
            remove { RemoveHandler(VideoNotFoundErrorEvent, value); }
        }
        void RaiseVideoNotFoundErrorEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoNotFoundErrorEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        #region UnEmbeddable (by uploader, not possible to show video) (Nr:101 and Nr:150)

        public static readonly RoutedEvent VideoUnEmbeddableErrorEvent = EventManager.RegisterRoutedEvent("VideoUnEmbeddableError", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler VideoUnEmbeddableError
        {
            add { AddHandler(VideoUnEmbeddableErrorEvent, value); }
            remove { RemoveHandler(VideoUnEmbeddableErrorEvent, value); }
        }
        void RaiseVideoUnEmbeddableErrorEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.VideoUnEmbeddableErrorEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        #endregion

        #region can edit.
        public static readonly RoutedEvent BecomeEditbleEvent = EventManager.RegisterRoutedEvent("BecomeEditble", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(YoutubePlayer));
        public event RoutedEventHandler BecomeEditble
        {
            add { AddHandler(BecomeEditbleEvent, value); }
            remove { RemoveHandler(BecomeEditbleEvent, value); }
        }
        void raiseIsEditbleEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(YoutubePlayer.BecomeEditbleEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion

        #endregion
    }
}
