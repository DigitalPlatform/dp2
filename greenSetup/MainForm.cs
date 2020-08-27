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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using GreenInstall;
using Serilog;

namespace greenSetup
{
    public partial class MainForm : Form
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public MainForm()
        {
            InitializeComponent();
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            // _ = install();
        }

#if REMOVED
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
_cancel.Token,
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
#endif
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

        SplashForm _splashForm = null;

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitialLogging();

            GreenInstaller.WriteInfoLog($"**********");
            GreenInstaller.WriteInfoLog($"greensetup 开始执行");

            string args = string.Join(' ', Environment.GetCommandLineArgs());
            GreenInstaller.WriteInfoLog($"命令行参数: {args}");



            _ = Start(IsCmdLineUpdate() ? false : true);
        }

        void ShowSplashWindow()
        {
            string splashFileName = Path.Combine(_binDir, "splash.png");
            if (File.Exists(splashFileName))
            {
                _splashForm = new SplashForm();
                _splashForm.StartPosition = FormStartPosition.CenterScreen;
                _splashForm.ImageFileName = splashFileName;
                _splashForm.Show(this);
            }
        }

        // TODO: 是否在延时一段以后自动关闭？
        void ErrorBox(string message)
        {
            GreenInstaller.WriteInfoLog(message);

            this.Invoke((Action)(() =>
            {
                MessageBox.Show(this, message);
            }));
        }

        // 命令行参数里面是否包含了静默初始化？
        public static bool IsSilently()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = 0;
            foreach (string arg in args)
            {
                if (i > 0
                    && (arg == "silently" || arg == "silent" || arg == "silence"))
                    return true;
                i++;
            }

            return false;
        }

        const string _binDir = "c:\\dp2ssl";

        // 启动 dp2ssl.exe；或首次安装；或升级并启动 dp2ssl.exe
        // parameters:
        //      delayUpdate 是否让 dp2ssl.exe 启动起来再探测升级？
        //                  意思就是，== true，让 dp2ssl.exe 去负责升级，而 greensetup.exe 这里不负责下载升级(只负责展开下载好的 .zip)。这样的特点是启动速度快
        //                  == false，让 greensetup.exe 直接探测下载升级，缺点是 dp2ssl.exe 启动就晚一点
        //                  但首次安装的时候，则是 greensetup.exe 负责下载 .zip 文件并安装。因为此时 dp2ssl.exe 还并不存在
        async Task Start(bool delayUpdate)
        {
            // *** 检查 dp2ssl.exe 是否已经在运行
            if (GreenInstaller.HasModuleStarted("{75BAF3F0-FF7F-46BB-9ACD-8FE7429BF291}") == true)
            {
                ErrorBox("dp2SSL 已经启动了，无法重复启动");
                Application.Exit();
                return;
            }

            // testing
            // await Task.Delay(5000);

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

            GreenInstaller.WriteInfoLog($"检查状态文件。check_result:{check_result.ToString()}");

            // 展开，并启动 dp2ssl.exe
            if (check_result.Value == 3)
            {
                GreenInstaller.WriteInfoLog($"以前遗留的 .zip 文件，展开，并立即启动 dp2ssl.exe");

                // return:
                //      -1  出错
                //      0   成功。不需要 reboot
                //      2   成功，但需要立即重新启动计算机才能让复制的文件生效
                var extract_result = GreenInstaller.ExtractFiles(_binDir);
                if (extract_result.Value == -1)
                {
                    ErrorBox(extract_result.ErrorInfo);
                    return;
                }
                else if (extract_result.Value == 2)
                {
                    // ErrorBox("部分文件更新受阻，请立即重新启动 Windows");
                }
                await ProcessStart(strExePath);
                return;
            }

            /*
            if (check_result.Value == 1
                || check_result.Value == 5)
            {
                ErrorBox(check_result.ErrorInfo);
                Application.Exit();
                return;
            }
            */

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

            if (firstInstall == false && delayUpdate)
            {
                GreenInstaller.WriteInfoLog($"非首次安装情形。立即启动 dp2ssl.exe");

                await ProcessStart(strExePath);
                return;
            }

            string style = "updateGreenSetupExe";
            if (delayUpdate == false)
                style += ",clearStateFile";
            if (firstInstall)
                style += ",mustExpandZip";  // 无论是否有更新，都要展开两个 .zip 文件。因为 dp2ssl.exe 不存在，必须要展开 .zip 才能得到

            GreenInstaller.WriteInfoLog($"style={style}");

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
                style,
                //false,
                //true,
                _cancel.Token,
                (double min, double max, double value, string text) =>
                {
                    try
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
                    }
                    catch
                    {

                    }
                });
            if (result.Value == -1)
            {
                if (result.ErrorCode == "System.Net.WebException")
                {
                    if (File.Exists(strExePath))
                    {
                        await ProcessStart(strExePath);
                        return;
                    }
                }
                ErrorBox(result.ErrorInfo);
                Application.Exit();
                return;
            }

            // result.Value == 2 要提醒重启 Windows 以完成安装
            if (result.Value == 2)
            {
                ErrorBox(result.ErrorInfo);
            }

            // TODO: 从 dp2ssl.exe 中取信息？

            // 首次安装成功
            if (firstInstall)
            {
                GreenInstaller.WriteInfoLog($"首次安装成功");

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
                /*
                if (Directory.Exists(sourceDirectory))
                {
                    string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl");
                    if (Directory.Exists(targetDirectory) == false)
                    {
                        var move_result = GreenInstaller.MoveUserDirectory(sourceDirectory,
                            targetDirectory,
                            _binDir,
                            "maskSource");
                        GreenInstaller.WriteInfoLog($"迁移用户文件夹 sourceDirectory={sourceDirectory}, targetDirectory={targetDirectory}, move_result={move_result.ToString()}");
                    }
                    else
                    {
                        GreenInstaller.WriteInfoLog($"绿色版用户文件夹 targetDirectory={targetDirectory} 已经存在，不再重复进行迁移");
                    }
                }
                */
                string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl");
                {
                    var move_result = GreenInstaller.MoveUserDirectory(sourceDirectory,
                        targetDirectory,
                        _binDir,
                        "maskSource");
                    GreenInstaller.WriteInfoLog($"迁移用户文件夹 sourceDirectory={sourceDirectory}, targetDirectory={targetDirectory}, move_result={move_result.ToString()}");
                }

            }

            await ProcessStart(strExePath);
            return;
        }

        async Task ProcessStart(string strExePath)
        {
            ShowSplashWindow();

            // 先删除启动状态文件
            string stateFileName = Path.Combine(_binDir, "dp2ssl_started");
            try
            {
                File.Delete(stateFileName);
            }
            catch
            {

            }

            string arguments = "";
            if (IsSilently())
                arguments = "silently";

            GreenInstaller.WriteInfoLog($"启动 path='{strExePath}', arguments='{arguments}'");

            var proc = Process.Start(strExePath, arguments);

            // 等待 dp2ssl 创建启动状态文件
            if (_splashForm != null)
            {
                DateTime begin_time = DateTime.Now;
                while (File.Exists(stateFileName) == false)
                {
                    await Task.Delay(100);

                    // 等待最多一分钟
                    if (DateTime.Now - begin_time > TimeSpan.FromSeconds(60))
                        break;
                }

                // 最少要等待 5 秒
                TimeSpan length = TimeSpan.FromSeconds(5);
                if (DateTime.Now - begin_time < length)
                {
                    this.Invoke((Action)(() =>
                    {
                        this.Location = new Point(-1000, -1000);
                        _splashForm.Activate();
                    }));
                    await Task.Delay(length - (DateTime.Now - begin_time));
                }
            }

            // 等待 dp2ssl 主窗口创建，然后 greensetup.exe 才退出
            Application.Exit();
        }

        // 命令行参数中是否有 update?
        public static bool IsCmdLineUpdate()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = 0;
            foreach (string arg in args)
            {
                if (i > 0 && arg == "update")
                    return true;
                i++;
            }

            return false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel.Cancel();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GreenInstaller.WriteInfoLog($"greensetup 关闭退出");
        }

        void InitialLogging()
        {
            //string dataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string logDir = Path.Combine(dataDir, "greensetup_logs");
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "dp2\\dp2ssl\\greensetup_logs");
            Library.TryCreateDir(logDir);

            string pattern = Path.Combine(logDir, "log_{Date}.txt");

            // https://michaelscodingspot.com/logging-in-dotnet/
            Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.RollingFile(pattern, shared: true)
        .CreateLogger();
        }
    }
}
