namespace DougVideoPlayer
{
    public class PlayListItem
    {
        public bool CurrentlyPlaying { get; set; }
        public string FilePath { get; set; }

        public bool Finished { get; set; } = false;
        public long PlaybackPosition { get; set; }
    }
}
