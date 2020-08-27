using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DougVIdeoPlayer
{
    public class PlayList
    {
        private List<PlayListItem> _list;
        private int _currentlyPlayingIndex = -1;

        public PlayList()
        {
            _list = new List<PlayListItem>();
        }

        public void AddToEnd(string FilePath)
        {
            _list.Add(new PlayListItem {FilePath = FilePath});
            if (_currentlyPlayingIndex == -1)
            {
                _currentlyPlayingIndex = 0;
                _list[_currentlyPlayingIndex].CurrentlyPlaying = true;
            }
        }

        public PlayListItem GetCurrentItem()
        {
            if (_currentlyPlayingIndex != -1)
            {
                return _list[_currentlyPlayingIndex];
            }
            else
            {
                return null;
            }
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

                return _list[_currentlyPlayingIndex];
            }
            else
            {
                return null;
            }
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

                return _list[_currentlyPlayingIndex];
            }
            else
            {
                return null;
            }
        }

        public int Count {
            get => _list.Count;
        }

        public void Clear()
        {
            _list.Clear();
            _currentlyPlayingIndex = -1;
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

        public List<PlayListItem> GetItems()
        {
            return _list.GetRange(0, Count);
        }
    }
}
