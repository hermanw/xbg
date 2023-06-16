using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace xbg
{
    public partial class MainForm : Form
    {
        Setting setting;

        public MainForm()
        {
            InitializeComponent();
            setting = new Setting();
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            re_Draw();
        }

        private IntPtr getWorkerW()
        {
            IntPtr progman = W32.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;

            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            W32.SendMessageTimeout(progman,
                                   0x052C,
                                   new IntPtr(0),
                                   IntPtr.Zero,
                                   W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                                   1000,
                                   out result);

            IntPtr workerw = IntPtr.Zero;
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    workerw = W32.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);

            return workerw;
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            await re_Draw();
            timer1.Interval = 120*1000;
        }

        private async void Form1_Paint(object sender, PaintEventArgs e)
        {
            await re_Draw(e.Graphics, new Rectangle(new Point(0,0), this.Size));
        }

        private async Task re_Draw()
        {
            IntPtr workerW = getWorkerW();
            W32.RECT rect;
            W32.GetWindowRect(workerW, out rect);
            IntPtr dc = W32.GetDCEx(workerW, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            if (dc != IntPtr.Zero)
            {
                using (Graphics g = Graphics.FromHdc(dc))
                {
                    foreach(var screen in Screen.AllScreens)
                    {
                        var r = screen.Bounds;
                        if (screen.Primary)
                        {
                            r.Offset(r.X - rect.X, r.Y - rect.Y);
                        }
                        else
                        {
                            int scaledWidth = rect.Width - Screen.PrimaryScreen.Bounds.Width;
                            int realWidth = r.Width;
                            r.X = r.X * scaledWidth / realWidth;
                            r.Y = r.Y * scaledWidth / realWidth;
                            r.Width = scaledWidth;
                            r.Height = r.Height * scaledWidth / realWidth;
                            r.X -= rect.X;
                            r.Y -= rect.Y;
                        }
                        await re_Draw(g, r);
                    }
                }
                W32.ReleaseDC(workerW, dc);
            }
        }

        private async Task re_Draw(Graphics g, Rectangle r)
        {
            switch (setting.type)
            {
                case 1:
                    g.FillRectangle(new SolidBrush(setting.color), r);
                    break;
                case 2:
                    try
                    {
                        g.DrawImage(Image.FromFile(setting.file_name), r);
                    }
                    catch (Exception)
                    {
                    }
                    break;
                case 3:
                    await setting.checkBing();
                    try
                    {
                        g.DrawImage(Image.FromFile(setting.bing_file_name), r);
                    }
                    catch (Exception)
                    {
                    }
                    break;
                default:
                    break;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
            else
            {
                Invalidate();
            }

        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            setting.ShowDialog();
            await re_Draw();
            Invalidate();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
            timer1.Stop();
            timer1.Interval = 8000;
            timer1.Start();
        }
    }
}