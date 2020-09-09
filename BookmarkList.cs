using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DougVideoPlayer
{
    public class BookmarkList
    {
        private List<Bookmark> _list;
        public int Count => _list.Count;

        public BookmarkList()
        {
            _list = new List<Bookmark>();
        }

        public void Clear()
        {
            _list.Clear();
        }

        public List<Bookmark> GetItems()
        {
            if (_list.Count == 0)
            {
                return new List<Bookmark>();
            }
            else
            {
                return _list.GetRange(0, Count);
            }
        }

        public void AddOrUpdate(string FilePath, long ViewingPosition, long FileSize = 0)
        {
            string FileName = Path.GetFileName(FilePath).ToLower();
            if (FileSize == 0)
            {
                FileInfo fi = new FileInfo(FilePath);
                FileSize = fi.Length;
            }

            // If file is already in the list, update the viewing position, else add it.
            Bookmark bookmark;
            bookmark = _list.Find(b => b.FileName == FileName && b.FileSize == FileSize);

            if (bookmark != null)
            {
                bookmark.ViewingPosition = ViewingPosition;
            }
            else
            {
                bookmark = new Bookmark {FileName = FileName, FileSize = FileSize, ViewingPosition = ViewingPosition};
                _list.Add(bookmark);
            }
        }
    }
}
