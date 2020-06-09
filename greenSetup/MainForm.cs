using GreenInstall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace greenSetup
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            _ = install();
        }

        async Task install()
        {
            /*
            string exePath = Assembly.GetExecutingAssembly().Location;
            bool updateGreenSetupExe = exePath.ToLower() != "c:\\dp2ssl\\greensetup.exe";
            */

            double ratio = 1;
            var result = await GreenInstaller.InstallFromWeb("http://dp2003.com/dp2ssl/v1_dev",
"c:\\dp2ssl",
// null,
false,
true,
(double min, double max, double value, string text) =>
{
    this.Invoke(new Action(() =>
    {
        if (text != null)
            label_message.Text = text;
        if (min != -1)
            progressBar1.Minimum = (Int32)min;
        if (max != -1)
        {
            if (max <= Int32.MaxValue)
            {
                ratio = 1;
                progressBar1.Maximum = (Int32)max;
            }
            else
            {
                ratio = Int32.MaxValue / max;
                progressBar1.Maximum = Int32.MaxValue;
            }
        }
        if (value != -1)
            progressBar1.Value = (int)((double)value * ratio);
    }));
});
            if (result.Value == -1)
            {
                MessageBox.Show(this, result.ErrorInfo);
                return;
            }

            // TODO: 从 dp2ssl.exe 中取信息？

            // 创建桌面快捷方式
            GreenInstaller.CreateShortcut(
                "desktop",
                "dp2SSL 自助借还(绿色)",
                "dp2SSL 自助借还(绿色)",
                "c:\\dp2ssl\\dp2ssl.exe");

            // 迁移用户文件夹
            string sourceDirectory = Path.Combine(
Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
"dp2ssl");
            if (Directory.Exists(sourceDirectory))
            {
                string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl");
                var move_result = GreenInstaller.MoveUserDirectory(sourceDirectory,
                    targetDirectory,
                    "c:\\dp2ssl",
                    "maskSource");
            }
            return;
        }

        private void button_createShortcut_Click(object sender, EventArgs e)
        {


            // 创建桌面快捷方式
            GreenInstaller.CreateShortcut(
                "desktop",
                "dp2SSL 自助借还(绿色)",
                "dp2SSL 自助借还(绿色)",
                "c:\\dp2ssl\\dp2ssl.exe");

            /*
            GreenInstaller.CreateShortcutToStartMenu(
"dp2ssl 绿色",
"c:\\dp2ssl\\dp2ssl.exe",
true);
            */
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _ = Start();
        }

        void ErrorBox(string message)
        {
            this.Invoke((Action)(() => {
                MessageBox.Show(this, message);
            }));
        }

        const string _binDir = "c:\\dp2ssl";

        async Task Start()
        {
            // *** 检查 dp2ssl.exe 是否已经在运行
            if (GreenInstaller.HasModuleStarted("{75BAF3F0-FF7F-46BB-9ACD-8FE7429BF291}") == true)
            {
                ErrorBox("dp2SSL 已经启动了，无法重复启动");
                Application.Exit();
                return;
            }

            string strExePath = Path.Combine(_binDir, "dp2ssl.exe");
            bool firstInstall = File.Exists(strExePath) == false;

            // *** 检查状态文件
            // result.Value
            //      -1  出错
            //      0   不存在状态文件
            //      1   正在下载 .zip 过程中。.zip 不完整
            //      2   当前 .zip 和 .exe 已经一样新
            //      3   当前 .zip 比 .exe 要新。需要展开 .zip 进行更新安装
            //      4   下载 .zip 失败。.zip 不完整
            //      5   当前 .zip 比 .exe 要新，需要重启计算机以便展开的文件生效
            var check_result = GreenInstaller.CheckStateFile(_binDir);
            // 展开，并启动 dp2ssl.exe
            if (check_result.Value == 3)
            {
                var extract_result = GreenInstaller.ExtractFiles(_binDir);
                if (extract_result.Value == -1)
                {
                    ErrorBox(extract_result.ErrorInfo);
                    return;
                }
                Process.Start(strExePath);
                Application.Exit();
                /*
                this.BeginInvoke((Action)(() => {
                    this.Close();
                }));
                */
                return;
            }

            if (check_result.Value == 1
                || check_result.Value == 5)
            {
                ErrorBox(check_result.ErrorInfo);
                Application.Exit();
                return;
            }

            /*
            Debugger.Launch();

            // 拷贝 greensetup.exe
            string exePath = Assembly.GetExecutingAssembly().Location.ToLower();
            string targetExePath = Path.Combine(_binDir, "greensetup.exe"); 
            if (exePath.EndsWith("greensetup.exe")
                && File.Exists(targetExePath) == false
                && exePath.ToLower() != targetExePath.ToLower())
            {
                MessageBox.Show(this, "copy greensetup.exe");
                Library.TryCreateDir(_binDir);
                File.Copy(exePath, targetExePath, true);
            }
            else
                MessageBox.Show(this, "not copy greensetup.exe");
            */

            // *** 从 Web 升级
            double ratio = 1;
            // 从 Web 服务器安装或者升级绿色版
            // result.Value:
            //      -1  出错
            //      0   经过检查发现没有必要升级
            //      1   成功
            //      2   成功，但需要立即重新启动计算机才能让复制的文件生效
            var result = await GreenInstaller.InstallFromWeb("http://dp2003.com/dp2ssl/v1_dev",
_binDir,
// null,
false,
true,
(double min, double max, double value, string text) =>
{
    this.Invoke(new Action(() =>
    {
        if (text != null)
            label_message.Text = text;
        if (min != -1)
            progressBar1.Minimum = (Int32)min;
        if (max != -1)
        {
            if (max <= Int32.MaxValue)
            {
                ratio = 1;
                progressBar1.Maximum = (Int32)max;
            }
            else
            {
                ratio = Int32.MaxValue / max;
                progressBar1.Maximum = Int32.MaxValue;
            }
        }
        if (value != -1)
            progressBar1.Value = (int)((double)value * ratio);
    }));
});
            if (result.Value == -1)
            {
                if (result.ErrorCode == "System.Net.WebException")
                {
                    if (File.Exists(strExePath))
                    {
                        Process.Start(strExePath);
                        Application.Exit();
                        return;
                    }
                }
                ErrorBox(result.ErrorInfo);
                Application.Exit();
                return;
            }

            // TODO: 从 dp2ssl.exe 中取信息？

            // 首次安装成功
            if (firstInstall)
            {
                // 创建桌面快捷方式
                GreenInstaller.CreateShortcut(
                    "desktop",
                    "dp2SSL 自助借还(绿色)",
                    "dp2SSL 自助借还(绿色)",
                    Path.Combine(_binDir, "greensetup.exe"));   // "dp2ssl.exe"

                GreenInstaller.CreateShortcut(
    "startup",
    "dp2SSL 自助借还(绿色)",
    "dp2SSL 自助借还(绿色)",
    Path.Combine(_binDir, "greensetup.exe"));   // "dp2ssl.exe"

                // 迁移用户文件夹
                string sourceDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "dp2ssl");
                if (Directory.Exists(sourceDirectory))
                {
                    string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl");
                    var move_result = GreenInstaller.MoveUserDirectory(sourceDirectory,
                        targetDirectory,
                        _binDir,
                        "maskSource");
                }
            }

            Process.Start(strExePath);
            Application.Exit();
            return;
        }
    }
}
