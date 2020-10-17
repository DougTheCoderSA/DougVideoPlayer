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
        public int Count => _list.Count;
        private List<Bookmark> _list;
        public BookmarkList()
        {
            _list = new List<Bookmark>();
        }

        public void AddOrUpdate(string FilePath, long ViewingPosition, long FileSize = 0)
        {
            // If file is already in the list, update the viewing position, else add it.
            Bookmark bookmark = GetBookmark(FilePath, FileSize);

            if (bookmark != null)
            {
                bookmark.ViewingPosition = ViewingPosition;
            }
            else
            {
                bookmark = new Bookmark { FileName = Path.GetFileName(FilePath).ToLower(), FileSize = FileSize, ViewingPosition = ViewingPosition };
                _list.Add(bookmark);
            }
        }

        public void Remove(string FilePath, long FileSize = 0)
        {
            Bookmark bookmark = GetBookmark(FilePath, FileSize);
            if (bookmark != null)
            {
                _list.Remove(bookmark);
            }
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

        public Bookmark GetBookmark(string FilePath, long FileSize = 0)
        {
            string FileName = Path.GetFileName(FilePath).ToLower();
            if (FileSize == 0)
            {
                FileInfo fi = new FileInfo(FilePath);
                FileSize = fi.Length;
            }

            return _list.Find(b => b.FileName == FileName && b.FileSize == FileSize);
        }
    }
}
