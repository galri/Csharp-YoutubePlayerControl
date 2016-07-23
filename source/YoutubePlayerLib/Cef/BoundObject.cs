using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubePlayerLib.Cef
{
    class BoundObject
    {
        public event EventHandler PlayerLoadingDone;
        public event EventHandler PlayerQualityChanged;
        public event EventHandler<YoutubePlayerState> PlayerPlayingChanged;

        public void PlayerLoaded()
        {
            if(PlayerLoadingDone != null)
            {
                PlayerLoadingDone(this, new EventArgs());
            }
        }

        public void qualityChanged()
        {
            PlayerQualityChanged?.Invoke(this, new EventArgs());
        }

        public void PlayingChanged(int state)
        {
            PlayerPlayingChanged?.Invoke(this, state.ParseToYoutubeState());
        }
    }
}
