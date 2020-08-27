using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DougVideoPlayer;
using LibVLCSharp.Shared;
using Newtonsoft.Json;

namespace DougVideoPlayer
{
    public partial class Form1 : Form
    {
        private readonly string[] _args;

        private readonly string _fileNameFinished = "finished.json";

        private readonly string _fileNamePlaylist = "playlist.json";

        private readonly LibVLC _libVlc;
        private readonly MediaPlayer _mp;
        private readonly UpdateEndReachedDelegate _updateEndReachedDelegate;
        private PlayListItem _currentPlayListItem, _nextPlayListItem;
        private bool _cursorIsInWindow;
        private bool _dodgeMouseCursor = true;
        private bool _endReached;
        private bool _fullScreenEnabled;
        private int _mouseX, _mouseY;
        private PlayList _playList;
        private Rectangle _posTopLeft, _posTopRight, _posBottomLeft, _posBottomRight;
        private long _savedPosition;
        private Rectangle _screenRectangle;
        private bool _skippedToPosition;
        private double _storedOpacity;
        private int _storedVolume = 40;
        private Timer _timerMouseLocation, _timerSavePlaylist;
        private string _videoPath = "";
        private Rectangle _windowCoordinatesRectangle;
        private delegate void UpdateEndReachedDelegate();

        public Form1(string[] pargs)
        {
            _args = pargs;

            if (!DesignMode)
            {
                Core.Initialize();
            }

            InitializeComponent();
            _libVlc = new LibVLC();
            _mp = new MediaPlayer(_libVlc);
            _mp.EndReached += _mp_EndReached;

            video.MediaPlayer = _mp;
            _updateEndReachedDelegate = UpdateEndReached;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        private void _mp_EndReached(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(_updateEndReachedDelegate);
            }
            else
            {
                UpdateEndReached();
            }
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TopMost)
            {
                TopMost = false;
                alwaysOnTopToolStripMenuItem.CheckState = CheckState.Unchecked;
            }
            else
            {
                TopMost = true;
                alwaysOnTopToolStripMenuItem.CheckState = CheckState.Checked;
            }
        }

        private string AppDataPath(string fileName)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                fileName);
        }

        /// <summary>
        /// Set up the four rectangles that define the positions in all the corners of the current screen where we want our window to jump to
        /// </summary>
        private void CalculateScreenPositions()
        {
            int marginTop = 25, marginBottom = 35, marginLeft = 25, marginRight = 25;

            _posTopLeft = new Rectangle(new Point(marginLeft, marginTop), new Size(Width, Height));
            _posTopRight = new Rectangle(new Point(_screenRectangle.Width - Width - marginRight, marginTop), new Size(Width, Height));
            _posBottomLeft = new Rectangle(new Point(marginLeft, _screenRectangle.Height - Height - marginBottom), new Size(Width, Height));
            _posBottomRight = new Rectangle(new Point(_screenRectangle.Width - Width - marginRight, _screenRectangle.Height - Height - marginBottom), new Size(Width, Height));
        }

        private void clearQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the playlist?", "Confirm Playlist Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _mp.Stop();
                _playList.Clear();
                _nextPlayListItem = null;
                _currentPlayListItem = null;
                File.WriteAllText(AppDataPath(_fileNamePlaylist), "");
            }
        }

        private double DistanceFromPos(Rectangle posRectangle)
        {
            return GetDistance(_windowCoordinatesRectangle.Left, _windowCoordinatesRectangle.Top, posRectangle.Left, posRectangle.Top);
        }

        private void dodgeTheCursorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_dodgeMouseCursor)
            {
                _dodgeMouseCursor = false;
                dodgeTheCursorToolStripMenuItem.CheckState = CheckState.Unchecked;
            }
            else
            {
                _dodgeMouseCursor = true;
                dodgeTheCursorToolStripMenuItem.CheckState = CheckState.Checked;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timerMouseLocation.Enabled = false;
            _timerMouseLocation.Dispose();
            _timerSavePlaylist.Enabled = false;
            _timerSavePlaylist.Dispose();

            _playList.RemoveFinishedItems();
            SaveCurrentPlaylistState();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Pause & Resume
            if (e.KeyChar == ' ')
            {
                if (_mp.IsPlaying)
                {
                    _mp.Pause();
                }
                else
                {
                    _mp.Play();
                }
            }

            // Mute
            if (e.KeyChar == 'm')
            {
                if (_mp.Volume != 0)
                {
                    _storedVolume = _mp.Volume;
                    _mp.Volume = 0;
                }
                else
                {
                    _mp.Volume = _storedVolume;
                }
            }

            // Volume up
            if (e.KeyChar == '+')
            {
                _mp.Volume += 10;
            }

            // Volume down
            if (e.KeyChar == '-')
            {
                _mp.Volume -= 10;
            }

            // Back 10 seconds
            if (e.KeyChar == '5')
            {
                JumpMilliseconds(-10000);
            }

            // Forward 10 seconds
            if (e.KeyChar == '6')
            {
                JumpMilliseconds(10000);
            }

            // Back 30 seconds
            if (e.KeyChar == '4')
            {
                JumpMilliseconds(-30000);
            }

            // Forward 30 seconds
            if (e.KeyChar == '7')
            {
                JumpMilliseconds(30000);
            }

            // Back 5 minutes
            if (e.KeyChar == '3')
            {
                JumpMilliseconds(-300000);
            }

            // Forward 5 minutes
            if (e.KeyChar == '8')
            {
                JumpMilliseconds(300000);
            }

            // Back 15 minutes
            if (e.KeyChar == '2')
            {
                JumpMilliseconds(-900000);
            }

            // Forward 15 minutes
            if (e.KeyChar == '9')
            {
                JumpMilliseconds(900000);
            }

            // Back 1 hour
            if (e.KeyChar == '1')
            {
                JumpMilliseconds(-3600000);
            }

            // Forward 1 hour
            if (e.KeyChar == '0')
            {
                JumpMilliseconds(3600000);
            }

            // Full screen toggle
            if (e.KeyChar == 'f')
            {
                ToggleFullScreenMode();

            }

            // Increase transparency
            if (e.KeyChar == '[')
            {
                if (Opacity >= 0.1)
                {
                    Opacity -= 0.1;
                }
            }

            // Increase opacity
            if (e.KeyChar == ']')
            {
                if (Opacity <= 0.9)
                {
                    Opacity += 0.1;
                }
            }

            // Next item in playlist
            if (e.KeyChar == '.')
            {
                PlayNext();
            }

            // Previous item in playlist
            if (e.KeyChar == ',')
            {
                PlayPrevious();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Subscribe();
            Text = "Doug's Video Player - Hold Shift to stop me moving around";
            _screenRectangle = ScreenBoundsRectangle();
            _windowCoordinatesRectangle = WindowCoordinatesRectangle();

            _playList = new PlayList();

            LoadPlaylistState();
            if (_args.Length > 0 && File.Exists(_args[0]))
            {
                _playList.AddToBeginning(_args[0]);
            }

            PlayNext();

            _timerMouseLocation = new Timer { Interval = 50 };
            _timerMouseLocation.Tick += TimerMouseLocationTick;
            _timerMouseLocation.Enabled = true;

            _timerSavePlaylist = new Timer { Interval = 1000 };
            _timerSavePlaylist.Tick += TimerSavePlaylist_Tick;
            _timerSavePlaylist.Enabled = true;
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            _screenRectangle = ScreenBoundsRectangle();
            _windowCoordinatesRectangle = WindowCoordinatesRectangle();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            _screenRectangle = ScreenBoundsRectangle();
            _windowCoordinatesRectangle = WindowCoordinatesRectangle();
            CalculateScreenPositions();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            CalculateScreenPositions();
            Bounds = _posBottomRight;
            _screenRectangle = ScreenBoundsRectangle();
            _windowCoordinatesRectangle = WindowCoordinatesRectangle();

            _mp.Volume = 40;
        }

        /// <summary>
        /// Calculate the distance between 2 points, given the x and y coordinates of both points.
        /// </summary>
        /// <returns></returns>
        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        private Rectangle GetNearestScreenPosRectangle()
        {
            List<RectangleAndDistance> rectangleAndDistances = new List<RectangleAndDistance>
            {
                new RectangleAndDistance {Rectangle = _posTopLeft, Distance = DistanceFromPos(_posTopLeft)},
                new RectangleAndDistance {Rectangle = _posTopRight, Distance = DistanceFromPos(_posTopRight)},
                new RectangleAndDistance {Rectangle = _posBottomLeft, Distance = DistanceFromPos(_posBottomLeft)},
                new RectangleAndDistance {Rectangle = _posBottomRight, Distance = DistanceFromPos(_posBottomRight)}
            };

            rectangleAndDistances = rectangleAndDistances.OrderBy(x => x.Distance).ToList();
            return rectangleAndDistances.First().Rectangle;
        }

        private void JumpMilliseconds(int milliseconds)
        {
            long targetTime = _mp.Time + milliseconds;
            if (targetTime > _mp.Length)
            {
                if (_mp.Length > 20000)
                {
                    targetTime = _mp.Length - 10000;
                }
                else
                {
                    targetTime = _mp.Length - 1;
                }
            }

            if (targetTime < 1)
            {
                targetTime = 1;
            }

            _mp.Time = targetTime;
        }

        private void LoadPlaylistState()
        {
            if (File.Exists(AppDataPath(_fileNamePlaylist)))
            {
                _playList.Clear();
                string json = File.ReadAllText(AppDataPath(_fileNamePlaylist));
                if (!string.IsNullOrEmpty(json))
                {
                    _playList.AddItems(JsonConvert.DeserializeObject<List<PlayListItem>>(json));
                }
            }
        }

        private void MoveOutOfTheWay()
        {
            Rectangle nearestRectangle = GetNearestScreenPosRectangle();
            if (nearestRectangle.Equals(_posBottomRight))
            {
                Bounds = _posTopRight;
            }

            if (nearestRectangle.Equals(_posTopRight))
            {
                Bounds = _posTopLeft;
            }

            if (nearestRectangle.Equals(_posTopLeft))
            {
                Bounds = _posBottomLeft;
            }

            if (nearestRectangle.Equals(_posBottomLeft))
            {
                Bounds = _posBottomRight;
            }

            _screenRectangle = ScreenBoundsRectangle();
            _windowCoordinatesRectangle = WindowCoordinatesRectangle();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog { CheckFileExists = true, Multiselect = true };
            DialogResult dialogResult = fileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                if (fileDialog.FileNames.Length > 0)
                {
                    foreach (string fileName in fileDialog.FileNames)
                    {
                        _playList.AddToEnd(fileName);
                    }

                    if (_mp.Media == null || !_mp.IsPlaying)
                    {
                        PlayNext();
                    }
                }
            }
        }

        private void PlayFile(string filePath, long playbackPosition = 0)
        {
            Text = Path.GetFileNameWithoutExtension(filePath);
            _videoPath = $"file://{filePath.Replace("#", "%23")}";
            _endReached = false;
            _savedPosition = playbackPosition;
            _skippedToPosition = false;

            Media media = new Media(_libVlc, new Uri(_videoPath));
            _mp.Play(media);
            _mp.Volume = 40;
        }

        private void PlayNext()
        {
            UpdatePlayPosition();
            _nextPlayListItem = _currentPlayListItem == null ? _playList.GetCurrentItem() : _playList.GetNextItem();
            if (_nextPlayListItem != null)
            {
                _currentPlayListItem = _nextPlayListItem;
                string filePath = _currentPlayListItem.FilePath;
                if (!string.IsNullOrEmpty(filePath))
                {
                    PlayFile(filePath, _currentPlayListItem.PlaybackPosition);
                }
            }
            else
            {
                _endReached = false;
                if (_mp.Media == null)
                {
                    Text = "Doug's Video Player - Hold Shift to stop me moving around";
                }
            }
        }

        private void PlayPrevious()
        {
            UpdatePlayPosition();
            _nextPlayListItem = _currentPlayListItem == null ? _playList.GetCurrentItem() : _playList.GetPreviousItem();
            if (_nextPlayListItem != null)
            {
                _currentPlayListItem = _nextPlayListItem;
                string filePath = _currentPlayListItem.FilePath;
                if (!string.IsNullOrEmpty(filePath))
                {
                    PlayFile(filePath, _currentPlayListItem.PlaybackPosition);
                }
            }
            else
            {
                _endReached = false;
            }
        }

        private void SaveCurrentPlaylistState()
        {
            UpdatePlayPosition();
            if (_playList != null && _playList.Count > 0)
            {
                string json = JsonConvert.SerializeObject(_playList.GetItems(), Formatting.Indented);
                File.WriteAllText(AppDataPath(_fileNamePlaylist), json);
            }
        }

        private Rectangle ScreenBoundsRectangle()
        {
            return Screen.FromControl(this).Bounds;
        }

        private void TimerMouseLocationTick(object sender, EventArgs e)
        {
            _timerMouseLocation.Enabled = false;

            GetCursorPos(out Point cursorPos);

            _mouseX = cursorPos.X;
            _mouseY = cursorPos.Y;

            if (WindowState != FormWindowState.Maximized &&
                _windowCoordinatesRectangle.Width < (_screenRectangle.Width / 2) &&
                !(_windowCoordinatesRectangle.Height >= (_screenRectangle.Height / 1.5)))
            {
                if (_mouseX >= Left && _mouseX <= Bounds.Right && _mouseY >= Top && _mouseY <= Bounds.Bottom)
                {
                    if ((ModifierKeys & Keys.Shift) == Keys.None && !_cursorIsInWindow && _dodgeMouseCursor)
                    {
                        MoveOutOfTheWay();
                    }
                    else
                    {
                        menuStrip1.Show();
                        _cursorIsInWindow = true;
                    }
                }
                else
                {
                    _cursorIsInWindow = false;
                    menuStrip1.Hide();
                }
            }

            if (_savedPosition != 0 && !_skippedToPosition)
            {
                _skippedToPosition = true;
                _mp.Time = _savedPosition;
                _savedPosition = 0;
            }

            // Check if the end of the video was reached, and play the next item in the playlist if so
            if (_endReached)
            {
                PlayNext();
            }

            _timerMouseLocation.Enabled = true;
        }

        private void TimerSavePlaylist_Tick(object sender, EventArgs e)
        {
            _timerSavePlaylist.Enabled = false;

            SaveCurrentPlaylistState();

            _timerSavePlaylist.Enabled = true;
        }

        private void ToggleFullScreenMode()
        {
            if (_fullScreenEnabled)
            {
                Opacity = _storedOpacity;
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                _fullScreenEnabled = false;
            }
            else
            {
                _storedOpacity = Opacity;
                Opacity = 1.0;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                _fullScreenEnabled = true;
            }
        }

        private void UpdateEndReached()
        {
            _endReached = true;
            _currentPlayListItem.Finished = true;
            File.AppendAllText(AppDataPath(_fileNameFinished), _currentPlayListItem.FilePath + Environment.NewLine);
        }

        private void UpdatePlayPosition()
        {
            if (_currentPlayListItem == null)
            {
                return;
            }

            if (_mp.Media != null && _mp.IsPlaying)
            {
                _currentPlayListItem.PlaybackPosition = _mp.Time;
            }
        }

        private Rectangle WindowCoordinatesRectangle()
        {
            return RectangleToScreen(Bounds);
        }

        private struct RectangleAndDistance
        {
            public double Distance;
            public Rectangle Rectangle;
        }
    }
}
