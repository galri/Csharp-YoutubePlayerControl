namespace YoutubePlayerLib
{
    public enum YoutubePlayerState
    {
        unstarted,
        ended,
        playing,
        paused,
        buffering,
        videoCued,
        unknownvalue
    }


    public static class YoutubeStateExtensions
    {
        public static YoutubePlayerState ParseToYoutubeState(this int state)
        {
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
                default:
                    return YoutubePlayerState.unknownvalue;
            }
        }
    }
}
