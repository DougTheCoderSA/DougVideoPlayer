using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DougVideoPlayer
{
    public class Bookmark
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public long ViewingPosition { get; set; } = 0;
    }
}
