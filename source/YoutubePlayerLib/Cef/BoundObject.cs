using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlayerLib.Cef
{
    class BoundObject
    {
        public event EventHandler PlayerLoadingDone;

        public void PlayerLoaded()
        {
            if(PlayerLoadingDone != null)
            {
                PlayerLoadingDone(this, new EventArgs());
            }
        }
    }
}
