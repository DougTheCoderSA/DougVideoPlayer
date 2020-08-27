using System.Collections.Generic;
using System.Linq;

namespace DougVideoPlayer
{
    public class PlayList
    {
        public int Count => _list.Count;
        private List<PlayListItem> _list;
        private int _currentlyPlayingIndex = -1;

        public PlayList()
        {
            _list = new List<PlayListItem>();
        }

        public void AddItems(List<PlayListItem> items)
        {
            _list.AddRange(items);
            if (_list.Count > 0 && _currentlyPlayingIndex == -1)
            {
                PlayListItem item = _list.FirstOrDefault(i => i.CurrentlyPlaying);
                if (item != null)
                {
                    _currentlyPlayingIndex = _list.IndexOf(item);
                }
                else
                {
                    _currentlyPlayingIndex = 0;
                    _list[_currentlyPlayingIndex].CurrentlyPlaying = true;
                }
            }
        }

        public void AddToBeginning(string FilePath)
        {
            _list.Insert(0, new PlayListItem { FilePath = FilePath });
            SetCurrentlyPlaying();
        }

        public void AddToEnd(string FilePath)
        {
            _list.Add(new PlayListItem {FilePath = FilePath});
            SetCurrentlyPlaying();
        }

        private void SetCurrentlyPlaying()
        {
            if (_currentlyPlayingIndex == -1 && _list.Count > 0)
            {
                _currentlyPlayingIndex = 0;
                _list[_currentlyPlayingIndex].CurrentlyPlaying = true;
            }
        }

        public void Clear()
        {
            _list.Clear();
            _currentlyPlayingIndex = -1;
        }

        public PlayListItem GetCurrentItem()
        {
            if (_currentlyPlayingIndex != -1)
            {
                foreach (PlayListItem playListItem in _list)
                {
                    playListItem.CurrentlyPlaying = false;
                }
                PlayListItem item = _list[_currentlyPlayingIndex];
                item.CurrentlyPlaying = true;
                return item;
            }
            else
            {
                return null;
            }
        }

        public List<PlayListItem> GetItems()
        {
            return _list.GetRange(0, Count);
        }

        public PlayListItem GetNextItem()
        {
            if (_currentlyPlayingIndex != -1)
            {
                if (_currentlyPlayingIndex + 1 < _list.Count)
                {
                    PlayListItem item = _list[_currentlyPlayingIndex];
                    item.CurrentlyPlaying = false;
                    _currentlyPlayingIndex++;
                    item = _list[_currentlyPlayingIndex];
                    item.CurrentlyPlaying = true;
                    return _list[_currentlyPlayingIndex];
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public PlayListItem GetPreviousItem()
        {
            if (_currentlyPlayingIndex != -1)
            {
                if (_currentlyPlayingIndex - 1 >= 0)
                {
                    PlayListItem item = _list[_currentlyPlayingIndex];
                    item.CurrentlyPlaying = false;
                    _currentlyPlayingIndex--;
                    item = _list[_currentlyPlayingIndex];
                    item.CurrentlyPlaying = true;
                    return _list[_currentlyPlayingIndex];
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public void RemoveFinishedItems()
        {
            _list = _list.FindAll(i => !i.Finished).ToList();
            _currentlyPlayingIndex = _list.Count > 0 ? 0 : -1;
        }
    }
}
