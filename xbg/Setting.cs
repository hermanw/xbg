using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace xbg
{
    public partial class Setting : Form
    {
        // .ini file config
        public int type;
        public Color color;
        public String file_name;
        public String bing_date;

        // local variables
        String exeName;
        String exePath;
        String iniFile;
        public String bing_file_name;


        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string name, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public Setting()
        {
            InitializeComponent();
            exeName = Assembly.GetExecutingAssembly().GetName().Name;
            exePath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + "\\";
            iniFile = exePath + exeName + ".ini";
            bing_file_name = exePath + "bing.jpg";

            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(exeName, "type", "1", RetVal, 255, iniFile);
            type = int.Parse(RetVal.ToString());
            switch (type)
            {
                case 1: radioButton1.Checked = true; break;
                case 2: radioButton2.Checked = true; break;
                case 3: radioButton3.Checked = true; break;
                default:
                    break;
            }
            GetPrivateProfileString(exeName, "color", "-16777216", RetVal, 255, iniFile);
            color = Color.FromArgb(int.Parse(RetVal.ToString()));
            pictureBox1.BackColor = color;
            GetPrivateProfileString(exeName, "file_name", "file name", RetVal, 255, iniFile);
            file_name = RetVal.ToString();
            textBox1.Text = file_name;
            GetPrivateProfileString(exeName, "bing_date", "0", RetVal, 255, iniFile);
            bing_date = RetVal.ToString();
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                color = colorDialog1.Color;
                pictureBox1.BackColor = color;
                WritePrivateProfileString(exeName, "color", color.ToArgb().ToString(), iniFile);
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file_name = openFileDialog1.FileName;
                textBox1.Text = file_name;
                WritePrivateProfileString(exeName, "file_name", file_name, iniFile);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                type = 1;
            }
            if (radioButton2.Checked)
            {
                type = 2;
            }
            if (radioButton3.Checked)
            {
                type = 3;
            }
            WritePrivateProfileString(exeName, "type", type.ToString(), iniFile);
        }

        public class Images
        {
            public String url { get; set; }
        };
        public class BingImages
        {
            public Images[] images { get; set; }
        }

        public async Task checkBing()
        {
            var today = DateTime.Now.ToShortDateString();
            if (today.CompareTo(bing_date) != 0)
            {
                try
                {
                    HttpClient httpClient = new HttpClient();
                    var bingImages = await httpClient.GetFromJsonAsync<BingImages>("https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&uhd=1&uhdwidth=3840&uhdheight=2160&ensearch=1");
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile("https://www.bing.com/" + bingImages.images[0].url, bing_file_name);
                    bing_date = today;
                    WritePrivateProfileString(exeName, "bing_date", bing_date, iniFile);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
