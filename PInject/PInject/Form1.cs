using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PApplier
{
    public partial class Form1 : Form
    {
        string pubgPath, resPath;
        public Form1()
        {
            InitializeComponent();
            pubgPath = textBox1.Text + @"\TslGame\Content\Paks";
            resPath = textBox2.Text;
            File.WriteAllText(Application.StartupPath + "\\mnt.txt", "SELECT VDISK FILE = \"" + Application.StartupPath + "\\res.vhd" + "\"\r\nATTACH VDISK", System.Text.Encoding.ASCII);
            File.WriteAllText(Application.StartupPath + "\\unmnt.txt", "SELECT VDISK FILE = \"" + Application.StartupPath + "\\res.vhd" + "\"\r\nDETACH VDISK", System.Text.Encoding.ASCII);
        }
        async Task WaitnSec(int n)
        {
            await Task.Delay(n, new CancellationTokenSource().Token);
        }
        private void AttachVHD()
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.FileName = "DISKPART.EXE";
            pi.Arguments = "/S \"" + Application.StartupPath + "\\mnt.txt" + "\"";
            Process.Start(pi);
        }
        private void DetachVHD()
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.FileName = "DISKPART.EXE";
            pi.Arguments = "/S \"" + Application.StartupPath + "\\unmnt.txt" + "\"";
            Process.Start(pi);
        }
        private void CreateSL(string dst, string src)
        {
            if (File.Exists(dst)) File.Delete(dst);
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.FileName = "cmd";
            pi.Arguments = "/c \"mklink \"" + dst + "\" \"" + src + "\"\"";
            Process.Start(pi);
        }
        private void PreInjectPak(string pakName)
        {
            String org = pubgPath + @"\" + pakName,
                mid = Application.CommonAppDataPath + @"\" + pakName;
            CreateSL(org, mid);
        }
        private void InjectPak(string pakName)
        {
            String mid = Application.CommonAppDataPath + @"\" + pakName,
                src = resPath + @"\" + pakName;
            CreateSL(mid, src);
        }
        private void EjectPak(string pakName)
        {
            String mid = Application.CommonAppDataPath + @"\" + pakName,
                src = Application.StartupPath + @"\org." + pakName.Split('.')[1];
            CreateSL(mid, src);
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            AttachVHD();
            foreach (FileInfo eachPak in new DirectoryInfo(resPath).GetFiles("*.pak"))
            {
                PreInjectPak(eachPak.Name);
                EjectPak(eachPak.Name);
            }
            if (chkUsePList.Checked)
            {
                PreInjectPak("pakList.json");
                EjectPak("pakList.json");
            }
            tmrCheckPak.Start();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            pubgPath = textBox1.Text + @"\TslGame\Content\Paks";
        }

        private async void TmrCheckPUBG_Tick(object sender, EventArgs e)
        {
            Text = "Pak 적용 대기중";
            if (Process.GetProcessesByName("TslGame").Length >= 2)
            {
                tmrCheckPak.Stop();
                await WaitnSec(int.Parse(textBox3.Text));
                if (chkUsePList.Checked)
                    InjectPak("pakList.json");
                foreach (FileInfo eachPak in new DirectoryInfo(resPath).GetFiles("*.pak"))
                    InjectPak(eachPak.Name);
                Text = "적용완료";
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            tmrCheckPak.Stop();
            foreach (FileInfo eachPak in new DirectoryInfo(resPath).GetFiles("*.pak"))
                EjectPak(eachPak.Name);
            if (chkUsePList.Checked)
                EjectPak("pakList.json");
            DetachVHD();
            Text = "우회 완료";
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            resPath = textBox2.Text;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            textBox3.Text = (int.Parse(textBox3.Text) + 100).ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (FileInfo eachPak in new DirectoryInfo(resPath).GetFiles("*.pak"))
                EjectPak(eachPak.Name);
            EjectPak("pakList.json");
            DetachVHD();
        }
    }
}
