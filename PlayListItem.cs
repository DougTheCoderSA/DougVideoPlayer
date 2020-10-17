using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DougVideoPlayer
{
    public class PlayListItem
    {
        private string _filePath;
        private string _urlFinal;
        public bool CurrentlyPlaying { get; set; }

        public string FilePath { get; set; }

        public bool Finished { get; set; } = false;
        /// <summary>
        /// The MRL is the universal locator used by VLC to find and play media from a variety of sources.
        /// </summary>
        public string MediaResourceLocator { get; set; }

        public long PlaybackPosition { get; set; }

        public string Type { get; set; } = "File";

        public string UrlFinal { get; set; }

        public string UrlInitial { get; set; }

        private string MrlForFile(string FilePath)
        {
            return $"file://{FilePath.Replace("#", "%23")}";
        }
    }
}
