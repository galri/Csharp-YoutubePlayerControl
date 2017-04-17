using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YoutubePlayerLib.Cef
{
    /// <summary>
    /// Interaction logic for CefYoutubeController.xaml
    /// 
    /// A wrapper around CefSharp webbrowser. the browser contains the youtube iframe
    /// api for communicating with youtube player.
    /// </summary>
    public partial class CefYoutubeController : UserControl, IYoutubeController
    {
        private static readonly bool DebuggingSubProcess = Debugger.IsAttached;
        static CefYoutubeController()
        {
            var settings = new CefSettings();

            settings.RegisterScheme(new CefCustomScheme
            {
                SchemeName = CefSharpSchemeHandlerFactory.SchemeName,
                SchemeHandlerFactory = new CefSharpSchemeHandlerFactory()
            });

            //if (!CefSharp.Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: !DebuggingSubProcess))
            //{
            //    throw new Exception("Unable to Initialize Cef");
            //}

            // The changes for Initialize, but not sure working perfectly.
            if (!CefSharp.Cef.Initialize(settings))
            {
                throw new Exception("Unable to Initialize Cef");
            }
        }

        private const string startVideoParam = "start";
        private const string stopVideoParam = "stop";
        private const string pausetVideoParam = "pause";

        /// <summary>
        /// Setting CurrentQuality may take som time, and the IFrame coomponent may ignore the call.
        /// </summary>
        public YoutubeQuality CurrentQuality
        {
            get { return (YoutubeQuality)GetValue(CurrentQualityProperty); }
            set { SetValue(CurrentQualityProperty, value); }
        }

        public static readonly DependencyProperty CurrentQualityProperty =
            DependencyProperty.Register("CurrentQuality", typeof(YoutubeQuality), typeof(CefYoutubeController), new PropertyMetadata(YoutubeQuality.@default, CurrentQualityChanged));

        private static void CurrentQualityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var controller = (CefYoutubeController)d;
            controller.TryToSetQuality((YoutubeQuality)e.NewValue);
        }

        public string VideoId
        {
            get { return (string)GetValue(VideoIdProperty); }
            set { SetValue(VideoIdProperty, value); }
        }

        public static readonly DependencyProperty VideoIdProperty =
            DependencyProperty.Register("VideoId", typeof(string), typeof(CefYoutubeController), new PropertyMetadata("", VideoIdChanged));

        private static void VideoIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var playerController = (CefYoutubeController)d;
            playerController.SetVideoId((string)e.NewValue);
        }

        public int Volume
        {
            get { return (int)GetValue(VolumeProperty); }
            set { SetValue(VolumeProperty, value); }
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(int), typeof(CefYoutubeController), new PropertyMetadata(100, VolumeCanged));

        //Need to change here and not in property because sometimes the propertie set might not be called in bindings.
        private static void VolumeCanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var playerControll = (CefYoutubeController)d;
            playerControll.SetVolume((int)e.NewValue);
        }

        /// <summary>
        /// Defaults true
        /// </summary>
        public bool AutoPlay
        {
            get { return (bool)GetValue(AutoPlayProperty); }
            set { SetValue(AutoPlayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register("AutoPlay", typeof(bool), typeof(CefYoutubeController), new PropertyMetadata(true, AutoPlayChanged));

        private static void AutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var playerControler = (CefYoutubeController)d;
            playerControler.SetAutoPlay((bool)e.NewValue);
        }

        public static readonly DependencyProperty PlayerStateProperty = DependencyProperty.Register(
            "PlayerState", typeof(YoutubePlayerState), typeof(CefYoutubeController), 
            new PropertyMetadata(YoutubePlayerState.unknownvalue, PlayerStateChanged));

        private static void PlayerStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var p = (CefYoutubeController) d;
            var newValue = (YoutubePlayerState) e.NewValue;

            switch (newValue)
            {
                case YoutubePlayerState.unstarted:
                    break;
                case YoutubePlayerState.ended:
                    p.Stop();
                    break;
                case YoutubePlayerState.playing:
                    p.Start();
                    break;
                case YoutubePlayerState.paused:
                    p.Pause();
                    break;
                case YoutubePlayerState.buffering:
                    break;
                case YoutubePlayerState.videoCued:
                    break;
                case YoutubePlayerState.unknownvalue:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// State of player.
        /// note that setting state value to anything other than 
        /// Ended(stop), playing(play) or paused(pause) will be ignored.
        /// </summary>
        public YoutubePlayerState PlayerState
        {
            get { return (YoutubePlayerState) GetValue(PlayerStateProperty); }
            set { SetValue(PlayerStateProperty, value); }
        }

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand PauseCommand { get; }

        #region startup

        private bool _browserLoaded = false;
        private bool _iframePlayerLoaded = false;
        private bool _startupSettingsRun = false;

        public CefYoutubeController()
        {
            InitializeComponent();
            WebBrowser.Address = @"custom://cefsharp/CefPlayer.html";
            //set startup value for Player
            WebBrowser.LoadingStateChanged += CheckkIfLoadingDone;
            var bound = new BoundObject();
            bound.PlayerLoadingDone += JavascriptReady;
            bound.PlayerPlayingChanged += BoundOnPlayerPlayingChanged;
            WebBrowser.RegisterJsObject("bound", bound);

            StartCommand = new Command(Start);
            StopCommand = new Command(() => WebBrowser.ExecuteScriptAsync("setPlayerState", stopVideoParam));
            PauseCommand = new Command(() => WebBrowser.ExecuteScriptAsync("setPlayerState", pausetVideoParam));
        }

        private void Pause()
        {
            WebBrowser.ExecuteScriptAsync("setPlayerState", pausetVideoParam);
        }

        private void BoundOnPlayerPlayingChanged(object sender, YoutubePlayerState e)
        {
            Dispatcher.Invoke(() => PlayerState = e);
        }

        private void Stop()
        {
            WebBrowser.ExecuteScriptAsync("setPlayerState", stopVideoParam);
        }

        private void CheckkIfLoadingDone(object sender, LoadingStateChangedEventArgs e)
        {
            if (WebBrowser != null)
            {
                WebBrowser.LoadingStateChanged -= CheckkIfLoadingDone;
                _browserLoaded = true;
                this.Dispatcher.Invoke(() => { CheckForStartupSettings(); });
            }
        }

        /// <summary>
        /// meant to be called when everything is ready on the html side.
        /// </summary>
        private void JavascriptReady(object sender, EventArgs e)
        {
            _iframePlayerLoaded = true;
            Dispatcher.Invoke(() => { CheckForStartupSettings(); });
        }

        /// <summary>
        /// checks if everything is up to set default settings on iframe player.
        /// </summary>
        private void CheckForStartupSettings()
        {
            if (_iframePlayerLoaded && _browserLoaded && !_startupSettingsRun)
            {
                _startupSettingsRun = true;
                SetAutoPlay(AutoPlay);
                SetVideoId(VideoId);
                SetVolume(Volume);
            }
            else if (_iframePlayerLoaded && _browserLoaded && _startupSettingsRun)
            {
                Console.WriteLine(string.Format("Trying to call CheckForStartupSettings after already being called once!"));
            }
        }

        //private void SetStartupId(object sender, LoadingStateChangedEventArgs e)
        //{
        //    if (WebBrowser != null)
        //    {
        //        WebBrowser.LoadingStateChanged -= SetStartupId;
        //        this.Dispatcher.Invoke(() => {
        //            WebBrowser.ExecuteScriptAsync("var startUpId = " + VideoId);
        //        });
        //    }
        //}

        #endregion

        private bool IsloadingDone()
        {
            return _startupSettingsRun;
        }

        private void Start()
        {
            if (IsloadingDone())
                WebBrowser.ExecuteScriptAsync("setPlayerState", startVideoParam);
        }

        private void SetVolume(int volume)
        {
            if (IsloadingDone())
                WebBrowser.ExecuteScriptAsync("setVolume", volume);
        }

        bool haventRun = true;

        private void SetVideoId(string videoId)
        {
            if (haventRun)
            {
                //if(WebBrowser.WebBrowser == null)
                //{
                //WebBrowser.LoadingStateChanged += SetStartupId;
                //}
                //else
                //{
                //    WebBrowser.ExecuteScriptAsync("var startUpId = " + VideoId);
                //}
                haventRun = false;
            }
            if (IsloadingDone())
            {
                WebBrowser.ExecuteScriptAsync("setVideoId", videoId);
            }
        }

        private void SetAutoPlay(bool autoPlay)
        {
            if (IsloadingDone())
            {
                var script = string.Format("autoPlay = {0};", autoPlay.ToString().ToLower());
                WebBrowser.ExecuteScriptAsync(script);
            }
        }

        private void TryToSetQuality(YoutubeQuality quality)
        {
            if (IsloadingDone())
                WebBrowser.ExecuteScriptAsync("setQuality", quality.ToString());
        }
    }
}
