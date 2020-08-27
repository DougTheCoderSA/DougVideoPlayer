using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DougVIdeoPlayer;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace DougVideoPlayer
{
    public partial class Form1 : Form
    {
        //private IKeyboardMouseEvents m_GlobalHook;
        public LibVLC _libVLC;
        public MediaPlayer _mp;
        public int StoredVolume = 40;
        private string VideoPath = "";
        private string[] args;
        private Rectangle posTopLeft, posTopRight, posBottomLeft, posBottomRight;
        private Rectangle _screenRectangle;
        private Rectangle _windowCoordinatesRectangle;
        private int mouseX, mouseY;
        private bool CursorIsInWindow = false;
        private bool FullScreenEnabled = false;
        private Timer timerMouseLocation, timerSavePlaylist;
        private double StoredOpacity;
        private bool EndReached = false;
        private bool DodgeMouseCursor = true;
        private PlayList playList;
        private PlayListItem currentPlayListItem, nextPlayListItem;
        private string fileNamePlaylist = "playlist.json";
        private string fileNameFinished = "finished.json";
        private bool SkippedToPosition = false;
        private long SavedPosition = 0;

        public Form1(string[] pargs)
        {
            args = pargs;

            if (!DesignMode)
            {
                Core.Initialize();
            }

            InitializeComponent();
            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            _mp.EndReached += _mp_EndReached;

            video.MediaPlayer = _mp;
            updateEndReachedDelegate = UpdateEndReached;
        }

        private delegate void UpdateEndReachedDelegate();

        private UpdateEndReachedDelegate updateEndReachedDelegate = null;

        private void UpdateEndReached()
        {
            EndReached = true;
            currentPlayListItem.Finished = true;
            File.AppendAllText(AppDataPath(fileNameFinished), currentPlayListItem.FilePath + Environment.NewLine);
        }

        private void _mp_EndReached(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(updateEndReachedDelegate);
            }
            else
            {
                UpdateEndReached();
            }
        }

        private void PlayNext()
        {
            if (currentPlayListItem == null)
            {
                nextPlayListItem = playList.GetCurrentItem();
            }
            else
            {
                nextPlayListItem = playList.GetNextItem();
            }
            if (nextPlayListItem != null)
            {
                currentPlayListItem = nextPlayListItem;
                string FilePath = currentPlayListItem.FilePath;
                if (!string.IsNullOrEmpty(FilePath))
                {
                    PlayFile(FilePath, currentPlayListItem.PlaybackPosition);
                }
            }
            else
            {
                EndReached = false;
            }
        }

        private void PlayPrevious()
        {
            if (currentPlayListItem == null)
            {
                nextPlayListItem = playList.GetCurrentItem();
            }
            else
            {
                nextPlayListItem = playList.GetPreviousItem();
            }
            if (nextPlayListItem != null)
            {
                currentPlayListItem = nextPlayListItem;
                string FilePath = currentPlayListItem.FilePath;
                if (!string.IsNullOrEmpty(FilePath))
                {
                    PlayFile(FilePath, currentPlayListItem.PlaybackPosition);
                }
            }
            else
            {
                EndReached = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Subscribe();
            Text = "Doug's Video Player - Hold Shift to stop me moving around";
            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();

            playList = new PlayList();

            if (args.Length > 0 && File.Exists(args[0]))
            {
                Text = Path.GetFileNameWithoutExtension(args[0]);
                VideoPath = $"file://{args[0].Replace("#", "%23")}";
            }
            else
            {
                LoadPlaylistState();
                PlayNext();
            }

            timerMouseLocation = new Timer();
            timerMouseLocation.Interval = 50;
            timerMouseLocation.Tick += TimerMouseLocationTick;
            timerMouseLocation.Enabled = true;

            timerSavePlaylist = new Timer();
            timerSavePlaylist.Interval = 1000;
            timerSavePlaylist.Tick += TimerSavePlaylist_Tick;
            timerSavePlaylist.Enabled = true;
        }

        private void TimerSavePlaylist_Tick(object sender, EventArgs e)
        {
            SaveCurrentPlaylistState();
        }

        private void SaveCurrentPlaylistState()
        {
            if (_mp.Media != null && _mp.IsPlaying)
            {
                currentPlayListItem.PlaybackPosition = _mp.Time;
            }
            if (playList != null && playList.Count > 0)
            {
                string json = JsonConvert.SerializeObject(playList.GetItems(), Formatting.Indented);
                File.WriteAllText(AppDataPath(fileNamePlaylist), json);
            }
        }

        private void LoadPlaylistState()
        {
            if (File.Exists(AppDataPath(fileNamePlaylist)))
            {
                playList.Clear();
                string json = File.ReadAllText(AppDataPath(fileNamePlaylist));
                if (!string.IsNullOrEmpty(json))
                {
                    playList.AddItems(JsonConvert.DeserializeObject<List<PlayListItem>>(json));
                }
            }
        }

        private string AppDataPath(string FileName)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                FileName);
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        private void TimerMouseLocationTick(object sender, EventArgs e)
        {
            timerMouseLocation.Enabled = false;

            Point cursorPos;
            GetCursorPos(out cursorPos);

            mouseX = cursorPos.X;
            mouseY = cursorPos.Y;

            if (WindowState != FormWindowState.Maximized &&
                _windowCoordinatesRectangle.Width < (_screenRectangle.Width / 2) &&
                !(_windowCoordinatesRectangle.Height >= (_screenRectangle.Height / 1.5)))
            {
                if (mouseX >= Left && mouseX <= Bounds.Right && mouseY >= Top && mouseY <= Bounds.Bottom)
                {
                    if ((ModifierKeys & Keys.Shift) == Keys.None && !CursorIsInWindow && DodgeMouseCursor)
                    {
                        MoveOutOfTheWay();
                    }
                    else
                    {
                        menuStrip1.Show();
                        CursorIsInWindow = true;
                    }
                }
                else
                {
                    CursorIsInWindow = false;
                    menuStrip1.Hide();
                }
            }

            if (SavedPosition != 0 && !SkippedToPosition)
            {
                SkippedToPosition = true;
                _mp.Time = SavedPosition;
                SavedPosition = 0;
            }

            // Check if the end of the video was reached, and play the next item in the playlist if so
            if (EndReached)
            {
                PlayNext();
            }

            timerMouseLocation.Enabled = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            CalculateScreenPositions();
            Bounds = posBottomRight;
            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();

            _mp.Volume = 40;
            if (!string.IsNullOrEmpty(VideoPath))
            {
                menuStrip1.Hide();
                _mp.Play(new Media(_libVLC, new Uri(VideoPath)));
            }
        }

        private Rectangle screenBoundsRectangle()
        {
            return Screen.FromControl(this).Bounds;
        }

        private Rectangle windowCoordinatesRectangle()
        {
            return RectangleToScreen(Bounds);
        }
        
        /// <summary>
        /// Set up the four rectangles that define the positions in all the corners of the current screen where we want our window to jump to
        /// </summary>
        private void CalculateScreenPositions()
        {
            int MarginTop = 25, MarginBottom = 35, MarginLeft = 25, MarginRight = 25;

            posTopLeft = new Rectangle(new Point(MarginLeft, MarginTop), new Size(Width, Height));
            posTopRight = new Rectangle(new Point(_screenRectangle.Width - Width - MarginRight, MarginTop), new Size(Width, Height));
            posBottomLeft = new Rectangle(new Point(MarginLeft, _screenRectangle.Height - Height - MarginBottom), new Size(Width, Height));
            posBottomRight = new Rectangle(new Point(_screenRectangle.Width - Width - MarginRight, _screenRectangle.Height - Height - MarginBottom), new Size(Width, Height));
        }

        /// <summary>
        /// Calculate the distance between 2 points, given the x and y coordinates of both points.
        /// </summary>
        /// <returns></returns>
        private double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

        public struct RectangleAndDistance
        {
            public Rectangle rectangle;
            public double distance;
        }

        public Rectangle GetNearestScreenPosRectangle()
        {
            List<RectangleAndDistance> rectangleAndDistances = new List<RectangleAndDistance>();
            rectangleAndDistances.Add(new RectangleAndDistance { rectangle = posTopLeft, distance = DistanceFromPos(posTopLeft) });
            rectangleAndDistances.Add(new RectangleAndDistance { rectangle = posTopRight, distance = DistanceFromPos(posTopRight) });
            rectangleAndDistances.Add(new RectangleAndDistance { rectangle = posBottomLeft, distance = DistanceFromPos(posBottomLeft) });
            rectangleAndDistances.Add(new RectangleAndDistance { rectangle = posBottomRight, distance = DistanceFromPos(posBottomRight) });

            rectangleAndDistances = rectangleAndDistances.OrderBy(x => x.distance).ToList();
            return rectangleAndDistances.First().rectangle;
        }

        public double DistanceFromPos(Rectangle posRectangle)
        {
            return GetDistance(_windowCoordinatesRectangle.Left, _windowCoordinatesRectangle.Top, posRectangle.Left, posRectangle.Top);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();
            CalculateScreenPositions();
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
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
                    StoredVolume = _mp.Volume;
                    _mp.Volume = 0;
                }
                else
                {
                    _mp.Volume = StoredVolume;
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

            // Increace opacity
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Unsubscribe();
            timerMouseLocation.Enabled = false;
            timerMouseLocation.Dispose();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.Multiselect = true;
            DialogResult dialogResult = fileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                if (fileDialog.FileNames.Length > 0)
                {
                    for (int i = 0; i < fileDialog.FileNames.Length; i++)
                    {
                        playList.AddToEnd(fileDialog.FileNames[i]);
                    }

                    if (_mp.Media == null)
                    {
                        PlayNext();
                    }
                }
            }
        }

        private void PlayFile(string FilePath, long PlaybackPosition = 0)
        {
            Text = Path.GetFileNameWithoutExtension(FilePath);
            VideoPath = $"file://{FilePath.Replace("#", "%23")}";
            EndReached = false;
            SavedPosition = PlaybackPosition;
            SkippedToPosition = false;

            Media media = new Media(_libVLC, new Uri(VideoPath));
            _mp.Play(media);
            _mp.Volume = 40;
        }

        private void clearQueueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the playlist?", "Confirm Playlist Clear", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _mp.Stop();
                playList.Clear();
                nextPlayListItem = null;
                currentPlayListItem = null;
                File.WriteAllText(AppDataPath(fileNamePlaylist), "");
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

        private void dodgeTheCursorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DodgeMouseCursor)
            {
                DodgeMouseCursor = false;
                dodgeTheCursorToolStripMenuItem.CheckState = CheckState.Unchecked;
            }
            else
            {
                DodgeMouseCursor = true;
                dodgeTheCursorToolStripMenuItem.CheckState = CheckState.Checked;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();
        }

        private void MoveOutOfTheWay()
        {
            Rectangle nearestRectangle = GetNearestScreenPosRectangle();
            if (nearestRectangle.Equals(posBottomRight))
            {
                Bounds = posTopRight;
            }

            if (nearestRectangle.Equals(posTopRight))
            {
                Bounds = posTopLeft;
            }

            if (nearestRectangle.Equals(posTopLeft))
            {
                Bounds = posBottomLeft;
            }

            if (nearestRectangle.Equals(posBottomLeft))
            {
                Bounds = posBottomRight;
            }

            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();
        }

        public void ToggleFullScreenMode()
        {
            if (FullScreenEnabled)
            {
                Opacity = StoredOpacity;
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                FullScreenEnabled = false;
            }
            else
            {
                StoredOpacity = Opacity;
                Opacity = 1.0;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                FullScreenEnabled = true;
            }
        }
        
    }
}
