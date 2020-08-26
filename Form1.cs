using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LibVLCSharp.Shared;

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
        private Timer timer;

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

            video.MediaPlayer = _mp;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Subscribe();
            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();

            if (args.Length > 0 && File.Exists(args[0]))
            {
                Text = Path.GetFileNameWithoutExtension(args[0]);
                VideoPath = $"file://{args[0].Replace("#", "%23")}";
            }

            timer = new Timer();
            timer.Interval = 50;
            timer.Tick += Timer_Tick;
            timer.Enabled = true;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false;

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
                    if ((ModifierKeys & Keys.Shift) == Keys.None && !CursorIsInWindow)
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

            timer.Enabled = true;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            CalculateScreenPositions();
            Bounds = posBottomRight;
            _screenRectangle = screenBoundsRectangle();
            _windowCoordinatesRectangle = windowCoordinatesRectangle();

            _mp.Volume = 60;
            if (!string.IsNullOrEmpty(VideoPath))
            {
                menuStrip1.Hide();
                _mp.Play(new Media(_libVLC, new Uri(VideoPath)));
            }
        }

        public void Subscribe()
        {
            //m_GlobalHook = Hook.GlobalEvents();

            //m_GlobalHook.MouseMove += M_GlobalHook_MouseMove;
        }

        public void Unsubscribe()
        {
            //m_GlobalHook.Dispose();
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
            timer.Enabled = false;
            timer.Dispose();
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

        private void M_GlobalHook_MouseMove(object sender, MouseEventArgs e)
        {
            //mouseX = e.X;
            //mouseY = e.Y;

            //if (WindowState == FormWindowState.Maximized || _windowCoordinatesRectangle.Width >= (_screenRectangle.Width / 2)
            //                                             || _windowCoordinatesRectangle.Height >= (_screenRectangle.Height / 1.5))
            //{
            //    return;
            //}

            //if (mouseX >= Left && mouseX <= Bounds.Right && mouseY >= Top && mouseY <= Bounds.Bottom)
            //{
            //    if ((ModifierKeys & Keys.Shift) == Keys.None && !CursorIsInWindow)
            //    {
            //        MoveOutOfTheWay();
            //    }
            //    else
            //    {
            //        menuStrip1.Show();
            //        CursorIsInWindow = true;
            //    }
            //}
            //else
            //{
            //    CursorIsInWindow = false;
            //    menuStrip1.Hide();
            //}
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
                FormBorderStyle = FormBorderStyle.Sizable;
                WindowState = FormWindowState.Normal;
                FullScreenEnabled = false;
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                FullScreenEnabled = true;
            }
        }
        
    }
}
