using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace xbg
{
    public partial class Form1 : Form
    {
        // .ini file config
        Boolean use_color = true;
        Color color = Color.Black;
        String file_name;
        // local variables
        String exeName;
        String iniFile;

        public Form1()
        {
            InitializeComponent();
            exeName = Assembly.GetExecutingAssembly().GetName().Name;
            iniFile = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\" + exeName + ".ini";
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(exeName, "use_color", "true", RetVal, 255, iniFile);
            use_color = Boolean.Parse(RetVal.ToString());
            GetPrivateProfileString(exeName, "color", "0", RetVal, 255, iniFile);
            color = Color.FromArgb(int.Parse(RetVal.ToString()));
            GetPrivateProfileString(exeName, "file_name", "0", RetVal, 255, iniFile);
            file_name = RetVal.ToString();
            re_Draw();
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string name, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // Color button
        private void button1_Click(object sender, EventArgs e)
        {
            if(colorDialog1.ShowDialog() == DialogResult.OK)
            {
                color = colorDialog1.Color;
                use_color = true;
                Invalidate();
                re_Draw();
                WritePrivateProfileString(exeName, "use_color", use_color.ToString(), iniFile);
                WritePrivateProfileString(exeName, "color", color.ToArgb().ToString(), iniFile);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file_name = openFileDialog1.FileName;
                use_color = false;
                Invalidate();
                re_Draw();
                WritePrivateProfileString(exeName, "use_color", use_color.ToString(), iniFile);
                WritePrivateProfileString(exeName, "file_name", file_name, iniFile);
            }
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            re_Draw();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            re_Draw(e.Graphics, new Rectangle(new Point(0,0), this.Size));
        }

        private void re_Draw()
        {
            IntPtr workerW = getWorkerW();
            W32.RECT rect;
            W32.GetWindowRect(workerW, out rect);
            IntPtr dc = W32.GetDCEx(workerW, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            if (dc != IntPtr.Zero)
            {
                using (Graphics g = Graphics.FromHdc(dc))
                {
                    re_Draw(g, rect);
                }
                W32.ReleaseDC(workerW, dc);
            }
        }

        private void re_Draw(Graphics g, Rectangle r)
        {
            if (use_color)
            {
                g.FillRectangle(new SolidBrush(color), r);
            }
            else
            {
                g.DrawImage(Image.FromFile(file_name), r);
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
    }
}