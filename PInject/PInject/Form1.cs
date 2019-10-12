using Microsoft.VisualBasic.FileIO;
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
            File.WriteAllText(Application.StartupPath + "\\tmp\\mnt.txt", "SELECT VDISK FILE = \"" + Application.StartupPath + "\\res.vhd" + "\"\r\nATTACH VDISK\r\nEXIT", System.Text.Encoding.ASCII);
            File.WriteAllText(Application.StartupPath + "\\tmp\\unmnt.txt", "SELECT VDISK FILE = \"" + Application.StartupPath + "\\res.vhd" + "\"\r\nDETACH VDISK\r\nEXIT", System.Text.Encoding.ASCII);
        }
        async Task WaitnSec(int n)
        {
            await Task.Delay(n, new CancellationTokenSource().Token);
        }
        private string GetVHDVolumeLabel()
        {
            DriveInfo[] drive = DriveInfo.GetDrives();
            for (int i = 0; i < drive.Length; i++)
            {
                if (drive[i].VolumeLabel == "PUBG")
                {
                    return drive[i].RootDirectory.ToString();
                }
            }
            return "E:\\";
        }
        private void WaitForAttach()
        {
            while (true)
            {
                DriveInfo[] drive = DriveInfo.GetDrives();
                for (int i = 0; i < drive.Length; i++)
                    if (drive[i].VolumeLabel == "PUBG") return;
            }
        }
        private void AttachVHD()
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.FileName = "DISKPART.EXE";
            pi.Arguments = "/S \"" + Application.StartupPath + "\\tmp\\mnt.txt" + "\"";
            Process.Start(pi);
        }
        private void DetachVHD()
        {
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.FileName = "DISKPART.EXE";
            pi.Arguments = "/S \"" + Application.StartupPath + "\\tmp\\unmnt.txt" + "\"";
            Process.Start(pi);
        }
        private void CreateHSL(string dst, string src)
        {
            if (File.Exists(dst)) File.Delete(dst);
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.WindowStyle = ProcessWindowStyle.Hidden;
            pi.FileName = "cmd";
            pi.Arguments = "/c \"mklink /h \"" + dst + "\" \"" + src + "\"\"";
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
            //원본넣고 초기화
            if (!Directory.Exists(Application.StartupPath + @"\tmp")) Directory.CreateDirectory(Application.StartupPath + @"\tmp");
            String org = pubgPath + @"\" + pakName,
                mid = Application.StartupPath + @"\tmp\" + pakName;
            CreateSL(org, mid);
        }
        private void InjectPak(string pakName)
        {
            String mid = Application.StartupPath + @"\tmp\" + pakName,
                src = resPath + @"\" + pakName;
            CreateSL(mid, src);
        }
        private void EjectPak(string pakName)
        {
            String mid = Application.StartupPath + @"\tmp\" + pakName,
                src = Application.StartupPath + @"\org." + pakName.Split('.')[1];
            CreateSL(mid, src);
        }

        private void SetPUBG()
        {
            //FileSystem.CopyDirectory(textBox1.Text, textBox1.Text + "2",UIOption.AllDialogs);
            foreach(string eachDirectory in Directory.EnumerateDirectories(textBox1.Text,"*.*",System.IO.SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(eachDirectory.Replace(@"\PUBG\",@"\PUBG2\"));
            }
            foreach (string eachFile in Directory.EnumerateFiles(textBox1.Text, "*.*", System.IO.SearchOption.AllDirectories))
            {
                CreateHSL(eachFile.Replace(@"\PUBG\", @"\PUBG2\"), eachFile);
            }
            File.Delete(textBox1.Text + @"2\TslGame\Content\Paks\pakList.json");
            File.Delete(textBox1.Text + @"2\TslGame\Binaries\Win64\TslGame.exe");
            File.Copy(Application.StartupPath + @"\org.json", textBox1.Text + @"2\TslGame\Content\Paks\pakList.json", true);
            File.Copy(Application.StartupPath + @"\org.exe", textBox1.Text + @"2\TslGame\Binaries\Win64\TslGame.exe", true);
            CreateSL(textBox1.Text + @"\TslGame\Binaries\Win64\TslGame.exe", textBox1.Text + @"2\TslGame\Binaries\Win64\TslGame.exe");
            MessageBox.Show("Done");
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            AttachVHD();
            WaitForAttach();
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

        private void Form1_Load(object sender, EventArgs e)
        {
            AttachVHD();
            WaitForAttach();
            textBox2.Text = GetVHDVolumeLabel() + "res";
        }

        private void LinkLabel1_Click(object sender, EventArgs e)
        {
            SetPUBG();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AttachVHD();
            WaitForAttach();
            foreach (FileInfo eachPak in new DirectoryInfo(resPath).GetFiles("*.pak"))
                EjectPak(eachPak.Name);
            EjectPak("pakList.json");
            DetachVHD();
        }
    }
}
