using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YoutubePlayerLib
{
    interface IYoutubeController
    {
        YoutubeQuality CurrentQuality { get; set; }

        string VideoId { get; set; }

        int Volume { get; set; }

        bool AutoPlay { get; set; }

        ICommand StartCommand { get; }

        ICommand StopCommand { get; }

        ICommand PauseCommand { get; }
    }
}
