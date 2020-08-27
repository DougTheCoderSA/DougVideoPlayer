using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DougVIdeoPlayer
{
    public class PlayListItem
    {
        public string FilePath { get; set; }

        public long PlaybackPosition { get; set; } = 0;

        public bool Finished { get; set; } = false;

        public bool CurrentlyPlaying { get; set; } = false;
    }
}
