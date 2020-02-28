using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

using Newtonsoft.Json;
using Microsoft.VisualStudio.Threading;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.Text;

namespace dp2ManageCenter
{
    public partial class MainForm : Form, IChannelManager
    {
        // dp2library服务器数组(缺省用户名/密码等)
        public dp2ServerCollection Servers = null;

        FloatingMessageForm _floatingMessage = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

        public MainForm()
        {
            ClientInfo.ProgramName = "dp2managecenter";
            ClientInfo.MainForm = this;

            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ClientInfo.Initial("dp2managecenter");

            this.UiState = ClientInfo.Config.Get("global", "ui_state", ""); // Properties.Settings.Default.ui_state;
                                                                            // 恢复 MainForm 的显示状态
            {
                var state = ClientInfo.Config.Get("mainForm", "state", "");
                if (string.IsNullOrEmpty(state) == false)
                {
                    FormProperty.SetProperty(state, this, ClientInfo.IsMinimizeMode());
                }
            }

            if (_backupLimit != null)
                _backupLimit.Dispose();
            _backupLimit = new AsyncSemaphore(BackupChannelMax);

            if (_operLogLimit != null)
                _operLogLimit.Dispose();
            _operLogLimit = new AsyncSemaphore(OperLogChannelMax);

            LoadBackupTasks();
            LoadOperLogTasks(this.listView_operLogTasks);
            LoadOperLogTasks(this.listView_errorLogTasks);

            ClearHtml();
            ClearTaskConsole();

            // 显示版本号
            this.OutputHistory($"版本号: {ClientInfo.ClientVersion}");

            InitialServers();

            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channels_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channels_AfterLogin);

            // this.LoadTaskDom();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancelAllTask();

            SaveSettings();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Cancel();
            _cancel?.Dispose();

            _cancelRefresh?.Cancel();
            _cancelRefresh?.Dispose();

            this.Servers.ServerChanged -= new dp2ServerChangedEventHandle(Servers_ServerChanged);
            SaveServers();


            this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channels_BeforeLogin);
            this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channels_AfterLogin);
        }

        // 服务器名和默认帐户管理
        public void ManageServers(bool bFirstRun)
        {
            using (ServersDlg dlg = new ServersDlg())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "manageServers", "state");

                dp2ServerCollection newServers = Servers.Dup();

                if (bFirstRun == true)
                {
                    dlg.Text = "首次运行: 创建 dp2library 服务器目标";
                    dlg.FirstRun = true;
                }
                dlg.Servers = newServers;
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                // this.Servers = newServers;
                this.Servers.Import(newServers);
            }
        }

        void SaveServers()
        {
            // 保存到文件
            // parameters:
            //		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
            Servers.Save(null);
            Servers = null;
        }

        void InitialServers()
        {
            // 从文件中装载创建一个dp2ServerCollection对象
            // parameters:
            //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
            //							如果==true，函数直接返回一个新的空ServerCollection对象
            // Exception:
            //			FileNotFoundException	文件没找到
            //			SerializationException	版本迁移时容易出现
            try
            {
                Servers = dp2ServerCollection.Load(
                    Path.Combine(ClientInfo.UserDir, "servers.bin"),  // this.DataDir
                    true);
                Servers.ownerForm = this;
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                Servers = new dp2ServerCollection();
                // 设置文件名，以便本次运行结束时覆盖旧文件
                Servers.FileName = Path.Combine(ClientInfo.UserDir, "servers.bin");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "servers.bin 装载出现异常: " + ex.Message);
            }

            this.Servers.ServerChanged += new dp2ServerChangedEventHandle(Servers_ServerChanged);
        }

        void Servers_ServerChanged(object sender, dp2ServerChangedEventArgs e)
        {
            /*
            foreach (Form child in this.MdiChildren)
            {
                if (child is dp2SearchForm)
                {
                    dp2SearchForm searchform = (dp2SearchForm)child;
                    searchform.RefreshResTree();
                }

            }
            */
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.splitContainer_backupTasks,
                    this.listView_backupTasks,
                    this.listView_operLogTasks,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.splitContainer_backupTasks,
                    this.listView_backupTasks,
                    this.listView_operLogTasks,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        public void ShowMessage(string strMessage,
string strColor = "",
bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        public void ClearMessage()
        {
            this.ShowMessage("");
        }

        void SaveSettings()
        {
            ClientInfo.Config?.Set("global", "ui_state", this.UiState);

            // 保存 MainForm 的显示状态
            {
                var state = FormProperty.GetProperty(this);
                ClientInfo.Config.Set("mainForm", "state", state);
            }

            SaveBackupTasks();
            SaveOperLogTasks(this.listView_operLogTasks);
            SaveOperLogTasks(this.listView_errorLogTasks);

            ClientInfo.Finish();
        }

        #region 浏览器控件

        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(ClientInfo.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";
            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    this.webBrowser1.Navigate("about:blank");
                    doc = this.webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);

        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);
            // Global.ScrollToEnd(this.WebBrowser);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        public static void WriteHtml(WebBrowser webBrowser,
string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");

                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        public static void SetHtmlString(WebBrowser webBrowser,
    string strHtml,
    string strDataDir,
    string strTempFileType)
        {
            // StopWebBrowser(webBrowser);

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", Path.Combine(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".html");
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            Navigate(webBrowser, strTempFilename);  // 2015/7/28
        }

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            AppendHtml(strHtml);
        }

        public void OutputHistory(string strText, int nWarningLevel = 0)
        {
            OutputText(DateTime.Now.ToLongTimeString() + " " + strText, nWarningLevel);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

        #endregion


        #region dp2library 通道

        public LibraryChannelPool _channelPool = new LibraryChannelPool();
        private readonly Object _syncRoot_channelPool = new Object();

        List<LibraryChannel> _channelList = new List<LibraryChannel>();
        public void DoStop(object sender, StopEventArgs e)
        {
            lock (_syncRoot_channelPool)
            {
                foreach (LibraryChannel channel in _channelList)
                {
                    if (channel != null)
                        channel.Abort();
                }
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel(string strServerUrl,
            string strUserName = ".",
            GetChannelStyle style = GetChannelStyle.None)
        {
            if (strUserName == ".")
            {
                dp2Server server = this.Servers[strServerUrl];
                if (server == null)
                    throw new Exception("没有找到 URL 为 " + strServerUrl + " 的服务器对象(为寻找默认用户名 . 阶段)");

                if (strUserName == ".")
                    strUserName = server.DefaultUserName;
            }

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);

            //if ((style & GetChannelStyle.GUI) != 0)
            //    channel.Idle += channel_Idle;

            lock (_syncRoot_channelPool)
            {
                _channelList.Add(channel);
            }
            // TODO: 检查数组是否溢出
            return channel;
        }

        /*
        void channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }
        */

        public void ReturnChannel(LibraryChannel channel)
        {
            // channel.Idle -= channel_Idle;

            this._channelPool.ReturnChannel(channel);
            lock (_syncRoot_channelPool)
            {
                _channelList.Remove(channel);
            }
        }

        void Channels_BeforeLogin(object sender, DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            if (e.FirstTry == true)
            {
                dp2Server server = this.Servers[channel.Url];
                if (server != null)
                {
                    e.UserName = server.DefaultUserName;
                    e.Password = server.DefaultPassword;
                }
                else
                {
                    if (channel != null)
                    {
                        e.UserName = channel.UserName;
                        e.Password = channel.Password;
                    }
                    else
                    {
                        e.ErrorInfo = "没有找到 URL 为 " + channel.Url + " 的服务器对象";
                        e.Failed = true;
                        e.Cancel = true;
                        return;
                    }
                }

                string type = "worker";
                if (e.UserName.StartsWith("~"))
                {
                    e.UserName = e.UserName.Substring(1);
                    type = "reader";
                }

                e.Parameters = $"location=dp2managecenter,type={type}";

                // e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");


                e.Parameters += ",client=dp2managecenter|" + ClientInfo.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            this.Invoke((Action)(() =>
            {
                // 
                IWin32Window owner = this;

                ServerDlg dlg = SetDefaultAccount(
                    e.LibraryServerUrl,
                    null,
                    e.ErrorInfo,
                    owner);
                if (dlg == null)
                {
                    e.Cancel = true;
                    return;
                }

                e.UserName = dlg.UserName;
                e.Password = dlg.Password;
                e.SavePasswordShort = false;

                {
                    string type = "worker";
                    if (e.UserName.StartsWith("~"))
                    {
                        e.UserName = e.UserName.Substring(1);
                        type = "reader";
                    }

                    e.Parameters = $"location=dp2managecenter,type={type}";
                }

                // e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

                e.Parameters += ",client=dp2managecenter|" + ClientInfo.ClientVersion;

                e.SavePasswordLong = true;
                e.LibraryServerUrl = dlg.ServerUrl;
            }));
        }

        ServerDlg SetDefaultAccount(
            string strServerUrl,
            string strTitle,
            string strComment,
            IWin32Window owner)
        {
            dp2Server server = this.Servers[strServerUrl];

            using (ServerDlg dlg = new ServerDlg())
            {
                GuiUtil.SetControlFont(dlg, this.Font);

                if (String.IsNullOrEmpty(strServerUrl) == true)
                {
                }
                else
                {
                    dlg.ServerUrl = strServerUrl;
                }

                if (owner == null)
                    owner = this;

                if (String.IsNullOrEmpty(strTitle) == false)
                    dlg.Text = strTitle;

                dlg.Comment = strComment;
                dlg.UserName = server.DefaultUserName;

                /*
                this.App.AppInfo.LinkFormState(dlg,
                    "dp2_logindlg_state");
                this.Activate();    // 让 MDI 子窗口翻出来到前面
                */
                dlg.ShowDialog(owner);

                // this.AppInfo.UnlinkFormState(dlg);


                if (dlg.DialogResult == DialogResult.Cancel)
                {
                    return null;
                }

                bool bChanged = false;

                if (server.DefaultUserName != dlg.UserName)
                {
                    server.DefaultUserName = dlg.UserName;
                    bChanged = true;
                }

                string strNewPassword = (dlg.SavePassword == true) ?
                dlg.Password : "";
                if (server.DefaultPassword != strNewPassword)
                {
                    server.DefaultPassword = strNewPassword;
                    bChanged = true;
                }

                if (server.SavePassword != dlg.SavePassword)
                {
                    server.SavePassword = dlg.SavePassword;
                    bChanged = true;
                }

                if (server.Url != dlg.ServerUrl)
                {
                    server.Url = dlg.ServerUrl;
                    bChanged = true;
                }

                if (bChanged == true)
                    this.Servers.Changed = true;

                return dlg;
            }
        }

        public string CurrentUserName { get; set; }

        void Channels_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = (LibraryChannel)sender;

            this.CurrentUserName = channel.UserName;

            dp2Server server = this.Servers[channel.Url];
            if (server != null)
            {
                server.Verified = true;
            }
        }

        #endregion

        private void MenuItem_serversSetting_Click(object sender, EventArgs e)
        {
            this.ManageServers(false);
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        void CancelAllTask()
        {
            foreach (ListViewItem item in this.listView_backupTasks.Items)
            {
                var info = GetBackupInfo(item);
                info.CancelTask();
            }

            foreach (ListViewItem item in this.listView_operLogTasks.Items)
            {
                var info = GetOperLogInfo(item);
                info.CancelTask();
            }
        }

        public int BackupChannelMax
        {
            get
            {
                return ClientInfo.Config.GetInt(
                "config",
                "backupChannelMax",
                5);
            }
        }

        public int OperLogChannelMax
        {
            get
            {
                return ClientInfo.Config.GetInt(
                "config",
                "operlogChannelMax",
                5);
            }
        }

        AsyncSemaphore _backupLimit = new AsyncSemaphore(1);
        AsyncSemaphore _operLogLimit = new AsyncSemaphore(1);

        // 先新建全部任务，这时已经用 BatchTask() "start" 请求启动了所有服务器端的备份任务。然后再做一个循环启动下载任务。
        private async void MenuItem_newBackupTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            /*
            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.Text = "请选择 dp2library 服务器";
            dlg.ChannelManager = this;

            dlg.Servers = this.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_SERVER };
            // dlg.Path = this.textBox_dp2library_serverName.Text;

            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;
            */
            var server_names = SelectServerNames();
            if (server_names == null)
                return;

            CancellationToken token = _cancel.Token;

            List<string> warnings = new List<string>();
            this.ShowMessage("正在创建和启动任务");
            try
            {
                var result = await Task.Run<NormalResult>(() =>
                {
                    foreach (var server_name in server_names)
                    {
                        token.ThrowIfCancellationRequested();

                        dp2Server server = this.Servers.GetServerByName(server_name);
                        if (server == null)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"名为 '{server_name}' 的服务器不存在..."
                            };
                        }

                        var new_result = NewBackupTask(server, true);
                        if (new_result.Value == -1)
                        {
                            if (new_result.ErrorCode == "taskAlreadyExist")
                                warnings.Add(new_result.ErrorInfo);
                        }
                    }

                    return new NormalResult();
                });
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }
            catch (Exception ex)
            {
                strError = "程序出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this.ClearMessage();
            }

            if (warnings.Count > 0 && token.IsCancellationRequested == false)
                MessageDlg.Show(this, "警告:\r\n" + StringUtil.MakePathList(warnings, "\r\n"), "警告");
            return;
        ERROR1:
            if (token.IsCancellationRequested == false)
                MessageBox.Show(this, strError);
        }

        void MenuItem_newStoppedBackupTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bControl = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            var server_names = SelectServerNames();
            if (server_names == null)
                return;

            List<string> warnings = new List<string>();
            this.ShowMessage("正在创建和启动任务");
            try
            {
                foreach (var server_name in server_names)
                {
                    dp2Server server = this.Servers.GetServerByName(server_name);
                    if (server == null)
                    {
                        strError = $"名为 '{server_name}' 的服务器不存在...";
                        goto ERROR1;
                    }

                    // 对列表中的事项进行查重
                    var dup = ListViewUtil.FindItem(this.listView_backupTasks, server.Name, OPERLOG_COLUMN_SERVERNAME);
                    if (dup != null)
                    {
                        strError = $"名为 '{server.Name}' 的大备份任务已经存在，无法再次创建";
                        goto ERROR1;
                    }

                    // 询问服务器端文件名。只需要输入一个文件名例如 "!backup/中国建筑科学研究院_2020-02-13_12_41_57.dp2bak" 即可，程序会自动计算出一对文件名
                    List<string> paths = null;
                    if (bControl)
                    {
                        var filename = InputDlg.GetInput(this, "指定大备份文件名", "服务器端大备份文件路径", null, this.Font);
                        if (filename != null)
                        {
                            paths = ComputePaths(filename);
                        }
                    }

                    ListViewItem item = new ListViewItem();
                    this.listView_backupTasks.Items.Add(item);
                    ListViewUtil.ChangeItemText(item, COLUMN_SERVERNAME, server.Name);

                    var info = GetBackupInfo(item);
                    if (paths != null)
                    {
                        info.InitialPathList(paths);
                        SetItemText(item, COLUMN_SERVERFILES, StringUtil.MakePathList(paths));
                    }
                    info.ServerName = server.Name;
                }
            }
            finally
            {
                this.ClearMessage();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据一个文件路径计算出一对文件路径
        static List<string> ComputePaths(string path)
        {
            if (path.StartsWith("!") == false)
                throw new ArgumentException($"路径 '{path}' 不合法。应为感叹号开头的服务器路径形态");

            string directory = Path.GetDirectoryName(path);
            string pure_filename = Path.GetFileNameWithoutExtension(path);
            List<string> results = new List<string>();
            results.Add(Path.Combine(directory, pure_filename + ".dbdef.zip"));
            results.Add(Path.Combine(directory, pure_filename + ".dp2bak"));
            return results;
        }

        const int COLUMN_SERVERNAME = 0;
        const int COLUMN_STATE = 1;
        const int COLUMN_STARTTIME = 2;
        const int COLUMN_PROGRESS = 3;
        const int COLUMN_SERVERFILES = 4;

        public class NewBackupTaskResult : NormalResult
        {
            // [out] 返回新创建的 ListViewItem 对象
            public ListViewItem ListViewItem { get; set; }
            public string OutputFolder { get; set; }
        }

        // 创建和启动一个大备份任务
        NewBackupTaskResult NewBackupTask(dp2Server server,
            // CancellationToken token,
            bool startDownload = true)
        {
            var dup_result = (NewBackupTaskResult)this.Invoke((Func<NewBackupTaskResult>)(() =>
            {
                // 对列表中的事项进行查重
                var dup = ListViewUtil.FindItem(this.listView_backupTasks, server.Name, OPERLOG_COLUMN_SERVERNAME);
                if (dup != null)
                    return new NewBackupTaskResult
                    {
                        Value = -1,
                        ErrorInfo = $"名为 '{server.Name}' 的大备份任务已经存在，无法再次创建",
                        ErrorCode = "taskAlreadyExist"
                    };
                else
                    return new NewBackupTaskResult();
            }));

            if (dup_result.Value == -1)
                return dup_result;

            string strOutputFolder = GetOutputFolder(server.Name);

            ListViewItem item = null;
            this.Invoke((Action)(() =>
            {
                item = new ListViewItem();
                this.listView_backupTasks.Items.Add(item);
                ListViewUtil.ChangeItemText(item, COLUMN_SERVERNAME, server.Name);

            }));

            var info = GetBackupInfo(item);
            info.ServerName = server.Name;
            info.CancelTask();
            info.BeginTask();
            // TODO: 出错时要把错误状态显示和背景颜色都兑现
            info.BackupTask = Backup(server,
                item, strOutputFolder,
                info.CancellationToken,
                startDownload);

            return new NewBackupTaskResult
            {
                Value = 0,
                ListViewItem = item,
                OutputFolder = strOutputFolder,
            };

            /*
            var result = await Backup(server, item, strOutputFolder, token, startDownload);
            if (result.Value == -1)
            {
                SetBackupItemError(item, result.ErrorInfo);
            }

                        return new NewBackupTaskResult
            {
                Value = result.Value,
                ErrorInfo = result.ErrorInfo,
                ListViewItem = item,
                OutputFolder = strOutputFolder,
            };
            */
        }

        // 设置事项为错误状态，并改变背景色为红色
        string SetBackupItemError(ListViewItem item, string error)
        {
            var info = GetBackupInfo(item);
            info.State = "error";
            SetItemText(item, COLUMN_STATE, $"任务出错: {error}");
            SetBackupItemColor(item);
            // TODO: 修改背景颜色为红色
            return error;
        }

        string GetItemText(ListViewItem item, int column)
        {
            return (string)this.Invoke((Func<string>)(() =>
            {
                return ListViewUtil.GetItemText(item, column);
            }));
        }

        void SetItemText(ListViewItem item, int column, string text)
        {
            if (this.IsDisposed)
                return;

            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, column, text);
                // https://stackoverflow.com/questions/2730931/how-to-set-tooltip-for-a-listviewitem
            }));
        }

        /*
        // 时间精确到日即可
        public static string GetCurrentBackupFileName(string server_name)
        {
            DateTime now = DateTime.Now;
            return "MC_" + now.ToString("yyyy_MM_dd") + "_" + server_name + "_" + now.ToString("HHmmssffff") +".dp2bak";
        }
        */

        // 时间精确到日即可
        public static string GetCurrentBackupFileName(string server_name)
        {
            DateTime now = DateTime.Now;
            return "MC_" + now.ToString("yyyy_MM_dd") + "_" + server_name
                // + "_" + now.ToString("HHmmssffff") 
                + ".dp2bak";
        }

        async Task<NormalResult> Backup(dp2Server server,
            ListViewItem item,
            string strOutputFolder,
            CancellationToken token,
            bool startDownload = true)
        {
            BatchTaskInfo resultInfo = null;

            LibraryChannel channel = this.GetChannel(server.Url);
            try
            {
                BackupTaskStart param = new BackupTaskStart
                {
                    BackupFileName = GetCurrentBackupFileName(server.Name),
                    DbNameList = "*"
                };

                BatchTaskStartInfo startinfo = new BatchTaskStartInfo
                {
                    Start = param.ToString(),
                    WaitForBegin = true
                };

                SetItemText(item, COLUMN_STATE, "正在启动服务器端任务");

                // 检查 dp2library 版本号
                var check_result = CheckLibraryServerVersion(channel);
                if (check_result.Value == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = SetBackupItemError(item, check_result.ErrorInfo)
                    };

                // return:
                //      -1  出错
                //      0   启动成功
                //      1   调用前任务已经处于执行状态，本次调用激活了这个任务
                long lRet = channel.BatchTask(
                    null,
                    "大备份",
                    "start",
                    new BatchTaskInfo { StartInfo = startinfo },
                    out resultInfo,
                    out string strError);

                if (resultInfo != null && resultInfo.StartInfo != null)
                {
                    // 把服务器端文件保存起来
                    List<string> paths = StringUtil.SplitList(resultInfo.StartInfo.OutputParam);
                    StringUtil.RemoveBlank(ref paths);
                    SetItemText(item, COLUMN_SERVERFILES, StringUtil.MakePathList(paths));

                    var info = GetBackupInfo(item);
                    info.ServerName = server.Name;
                    info.InitialPathList(paths);
                }

                if (lRet == -1 || lRet == 1)
                {
                    // TODO: 本次激活的情况，需要想办法获得服务器一端的两个文件名
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = SetBackupItemError(item, strError)
                    };
                }

                SetItemText(item, COLUMN_STATE, "服务器端任务已经启动");

                // return new NormalResult();
            }
            finally
            {
                this.ReturnChannel(channel);
            }

            if (startDownload)
            {
                // 开始下载文件
                // TODO: 检查这部分代码是否和 DownloadBackupFiles() 里面的代码有重复
                {

                    SetItemText(item, COLUMN_STATE, "正在下载");

                    var info = GetBackupInfo(item);

                    info.ServerName = server.Name;
                    info.State = "downloading";
                    SetBackupItemColor(item);
                }

                var result = await DownloadBackupFiles(
                    item,
                    strOutputFolder,
                    token,
                    true,
                    true);
                if (result.Value == -1)
                    SetBackupItemError(item, result.ErrorInfo);
                return result;
            }
            else
                return new NormalResult();
        }

        static char[] movingChars = new char[] { '/', '-', '\\', '|' };

        static string GetMovingChar(ref int index)
        {
            if (index < 0 || index > 3)
                index = 0;
            string result = new string(movingChars[index], 1);
            index++;
            if (index > 3)
                index = 0;
            return result;
        }

        int GetOutputFileNames(LibraryChannel channel,
            ListViewItem item,
            out List<string> paths,
            out string strError)
        {
            strError = "";
            paths = new List<string>();

            BatchTaskStartInfo startinfo = new BatchTaskStartInfo
            {
                Param = "getOutputFileNames",
            };
            // return:
            //      -1  出错
            //      0   启动成功
            //      1   调用前任务已经处于执行状态，本次调用激活了这个任务
            long lRet = channel.BatchTask(
                null,
                "大备份",
                "getinfo",
                new BatchTaskInfo { StartInfo = startinfo },
                out BatchTaskInfo resultInfo,
                out strError);
            if (lRet == -1)
                return -1;

            if (resultInfo != null && resultInfo.StartInfo != null)
            {
                // 把服务器端文件保存起来
                paths = StringUtil.SplitList(resultInfo.StartInfo.OutputParam);
                StringUtil.RemoveBlank(ref paths);
                SetItemText(item, COLUMN_SERVERFILES, StringUtil.MakePathList(paths));

                var info = GetBackupInfo(item);
                info.InitialPathList(paths);
            }

            return 0;
        }

        // parameters:
        //      deleteServerFile    下载成功后是否自动删除服务器端文件?
        async Task<NormalResult> DownloadBackupFiles(
    ListViewItem item,
    string strOutputFolder,
    CancellationToken token,
    bool clearLocalFileBefore = false,
    bool deleteServerFileAtEnd = true)
        {
            var info = GetBackupInfo(item);
            if (string.IsNullOrEmpty(info.ServerName))
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = SetBackupItemError(item, "item 中缺乏 info.ServerName")
                };

            dp2Server server = this.Servers.GetServerByName(info.ServerName);
            if (server == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = SetBackupItemError(item, $"没有找到名为 '{info.ServerName}' 的服务器定义")
                };

            LibraryChannel channel = this.GetChannel(server.Url);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);
            try
            {
                // TODO: 一组两个文件，对已经下载完的一个文件不要重复下载
                // TODO: 如果两个文件都已经成功下载，也要提示一下

                // 开始下载文件
                List<string> paths = new List<string>();
                if (info.PathList != null)
                    foreach (var path_item in info.PathList)
                    {
                        paths.Add(path_item.Path);
                    }

                if (paths.Count == 0)
                {
                    // 尝试重新从服务器获取这个信息
                    int nRet = GetOutputFileNames(channel,
    item,
    out paths,
    out string strError);
                    if (nRet == -1 || paths.Count == 0)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = SetBackupItemError(item, $"任务事项 '{info.ServerName}' 中没有任何下载文件事项")
                        };
                }

                StringUtil.RemoveBlank(ref paths);

                var infos = MainForm.BuildDownloadInfoList(paths);

                // 询问是否覆盖已有的目标下载文件。整体询问
                // return:
                //      -1  出错
                //      0   放弃下载
                //      1   同意启动下载
                var result = await AskOverwriteFiles(
                    channel,
                    infos,
                    "dontAskMd5Verify,auto" + (clearLocalFileBefore ? ",clearBefore" : ""),
                    strOutputFolder);
                if (result.Value == -1)
                {
                    SetBackupItemError(item, result.ErrorInfo);
                    return result;
                }

                if (result.Value != 1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = SetBackupItemError(item, "放弃处理")
                    };

                // 把要跳过的文件的状态修改
                // 这样才能让 ListViewItem 的 state 列最后正确显示全部完成
                info.ClearAllPathState();
                if (result.DeletedFiles != null)
                {
                    foreach (var skiped in result.DeletedFiles)
                    {
                        string path = skiped.ServerPath;
                        info.SetPathState(path, "finish");
                    }
                }

                paths = GetFileNames(infos, (i) =>
                {
                    return i.ServerPath;
                });

                // 设置开始时间
                DateTime start_time = DateTime.Now;
                SetItemText(item, COLUMN_STARTTIME, start_time.ToString());
                SetItemText(item, COLUMN_STATE, "正在下载");
                info.StartTime = start_time;
                info.State = "downloading";
                SetBackupItemColor(item);

                if (paths.Count == 0)
                {
                    if (info.IsAllPathFinish())
                    {
                        info.State = "finish";
                        SetItemText(item, COLUMN_STATE, "下载完成");
                    }
                    else
                    {
                        info.State = "finish";
                        SetItemText(item, COLUMN_STATE, "没有发生下载");
                    }

                    // 去删除服务器端的文件
                    if (deleteServerFileAtEnd)
                    {
                        var delete_result = DeleteServerFiles(item);
                        if (delete_result.Value == -1)
                        {
                            SetItemText(item, COLUMN_STATE, $"删除服务器端大备份文件时出错: {delete_result.ErrorInfo}");
                            // TODO: 是否返回出错？
                        }
                    }

                    info.State = "finish";
                    SetBackupItemColor(item);
                }
                else
                {
                    foreach (string path in paths)
                    {
                        token.ThrowIfCancellationRequested();

                        if (string.IsNullOrEmpty(path) == false)
                        {
                            // parameters:
                            //      strOutputFolder 输出目录。
                            //                      [in] 如果为 null，表示要弹出对话框询问目录。如果不为 null，则直接使用这个目录路径
                            //                      [out] 实际使用的目录
                            // return:
                            //      -1  出错
                            //      0   放弃下载
                            //      1   成功启动了下载
                            var download_result = await BeginDownloadFile(
                                info,
                                channel,
                                path,
                                "append",
                                strOutputFolder,
                                    (p, t) =>
                                    {
                                        int index = info.IndexOfPath(p);
                                        SetItemText(item, COLUMN_PROGRESS, $"({(index + 1)}){t} {GetMovingChar(ref info.MovingIndex)}");
                                    },
                                    (p, error) =>
                                    {
                                        // 修改 ListViewItem 状态列为“完成”
                                        if (error == null)
                                        {
                                            info.SetPathState(p, "finish");
                                            if (info.IsAllPathFinish())
                                            {
                                                info.State = "finish";
                                                SetItemText(item, COLUMN_STATE, "下载完成");

                                                // 删除所有服务器端备份文件
                                                if (deleteServerFileAtEnd)
                                                {
                                                    var delete_result = DeleteServerFiles(item);
                                                    if (delete_result.Value == -1)
                                                    {
                                                        SetItemText(item, COLUMN_STATE, $"下载文件完成，但删除服务器端大备份文件时出错: {delete_result.ErrorInfo}");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            info.SetPathState(p, $"error:{error}");
                                            info.State = "error";
                                            SetItemText(item, COLUMN_STATE, info.GetStateListString());
                                        }
                                        SetBackupItemColor(item);
                                    },
                                    token);

                            if (download_result.Value == -1)
                                return download_result;
                            if (download_result.Value == 0)
                                break;
                        }
                    }
                }

                return new NormalResult();
            }
            finally
            {
                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);
            }
        }


        NormalResult CancelServerBackupTask(ListViewItem item)
        {
            var info = GetBackupInfo(item);

            var server_url = GetServerUrl(info.ServerName);

            BatchTaskInfo resultInfo = null;

            LibraryChannel channel = this.GetChannel(server_url);
            try
            {
                SetItemText(item, COLUMN_STATE, "正在撤销服务器端任务");

                // return:
                //      -1  出错
                //      0   启动成功
                //      1   调用前任务已经处于执行状态，本次调用激活了这个任务
                long lRet = channel.BatchTask(
                    null,
                    "大备份",
                    "abort",
                    new BatchTaskInfo(),
                    out resultInfo,
                    out string strError);
                if (lRet == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = SetBackupItemError(item, strError)
                    };
                }

                info.State = "finish";
                SetItemText(item, COLUMN_STATE, "服务器端任务已经撤销");
                SetBackupItemColor(item);
                return new NormalResult();
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        NormalResult DeleteServerFiles(ListViewItem item)
        {
            var info = GetBackupInfo(item);

            if (info.PathList == null || info.PathList.Count == 0)
                return new NormalResult();

            if (string.IsNullOrEmpty(info.ServerName))
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "item 中缺乏 info.ServerName"
                };
            dp2Server server = this.Servers.GetServerByName(info.ServerName);
            if (server == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"没有找到名为 '{info.ServerName}' 的服务器定义"
                };

            LibraryChannel channel = this.GetChannel(server.Url);
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);
            try
            {
                List<string> errors = new List<string>();
                foreach (var path_item in info.PathList)
                {
                    string strPath = path_item.Path;

                    string strCurrentDirectory = Path.GetDirectoryName(strPath);
                    string strFileName = Path.GetFileName(strPath);

                    long nRet = channel.ListFile(
                        null,
                        "delete",
                        strCurrentDirectory,
                        strFileName,
                        0,
                        -1,
                        out FileItemInfo[] infos,
                        out string strError);
                    if (nRet == -1)
                        errors.Add($"删除服务器 {server.Name} 上的备份文件 {strPath} 时出错: {strError}");
                    // 如果文件不存在(nRet == 0)，不算错误
                }

                if (errors.Count > 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = StringUtil.MakePathList(errors, "\r\n")
                    };

                return new NormalResult();
            }
            finally
            {
                channel.Timeout = old_timeout;

                this.ReturnChannel(channel);
            }

            // 刷新显示。把 item 中的文件名列清空

        }

        public class PathItem
        {
            public string Path { get; set; }
            public string State { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ItemInfoBase : IDisposable
        {
            // 不参与序列化
            internal List<DynamicDownloader> _downloaders = new List<DynamicDownloader>();

            // 不参与序列化
            CancellationTokenSource _cancel { get; set; }

            public void AddDownloader(DynamicDownloader downloader)
            {
                this._downloaders.Add(downloader);
            }

            public void RemoveDownloader(DynamicDownloader downloader)
            {
                this._downloaders.Remove(downloader);
                downloader.Close();
            }

            public void CancelDownloaders()
            {
                foreach (var downloader in this._downloaders)
                {
                    downloader.Cancel();
                }
            }

            public void BeginTask()
            {
                /*
                CancelTask();
                this._cancel = new CancellationTokenSource();
                */
            }

            public void CancelTask()
            {
                if (this._cancel != null)
                {
                    this._cancel.Cancel();
                    this._cancel.Dispose();
                    this._cancel = null;
                }

                CancelDownloaders();
            }

            public void Dispose()
            {
                if (_cancel != null)
                {
                    _cancel.Dispose();
                    _cancel = null;
                }
            }

            public CancellationToken CancellationToken
            {
                get
                {
                    if (_cancel == null)
                        _cancel = new CancellationTokenSource();
                    return _cancel.Token;
                }
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class BackupItemInfo : ItemInfoBase
        {
            // 不参与序列化
            public Task<NormalResult> BackupTask { get; set; }

            public int MovingIndex = 0;

            // 服务器名
            [JsonProperty]
            public string ServerName { get; set; }

            // 要下载的文件名集合。服务器端路径形态
            [JsonProperty]
            public List<PathItem> PathList { get; set; }

            // 首次启动任务的时间
            [JsonProperty]
            public DateTime StartTime { get; set; }

            [JsonProperty]
            public string State { get; set; }   // (null)/downloading/finish/error

            public bool IsRunning
            {
                /*
                get
                {
                    if (BackupTask != null)
                    {
                        if (BackupTask.IsCompleted == false)
                            return true;
                    }
                    if (_downloaders == null)
                        return false;
                    return _downloaders.Count > 0;
                }
                */
                get
                {
                    if (BackupTask != null)
                    {
                        return !(BackupTask.IsCompleted);
                    }
                    return false;
                }
            }

            public int IndexOfPath(string path)
            {
                if (this.PathList == null)
                    return -1;
                int i = 0;
                foreach (var item in this.PathList)
                {
                    if (item.Path == path)
                        return i;
                    i++;
                }
                return -1;
            }

            public void SetPathState(string path, string state)
            {
                if (this.PathList == null)
                {
                    this.PathList = new List<PathItem>();
                    this.PathList.Add(new PathItem { Path = path, State = state });
                    return;
                }
                var item = this.PathList.Find((o) => { return o.Path == path; });
                if (item != null)
                    item.State = state;
            }

            // 是否所有文件都完成了?
            public bool IsAllPathFinish()
            {
                if (this.PathList == null)
                    return false;
                foreach (var item in this.PathList)
                {
                    if (item.State == null
                        || item.State.StartsWith("finish") == false)
                        return false;
                }
                return true;
            }

            public void ClearAllPathState()
            {
                if (this.PathList == null)
                    return;
                foreach (var item in this.PathList)
                {
                    item.State = null;
                }
            }

            public void InitialPathList(List<string> paths)
            {
                if (this.PathList == null)
                    this.PathList = new List<PathItem>();
                foreach (var path in paths)
                {
                    this.PathList.Add(new PathItem { Path = path });
                }
            }

            public string GetPathListString()
            {
                if (this.PathList == null)
                    return "";
                List<string> results = new List<string>();
                foreach (var item in this.PathList)
                {
                    results.Add(item.Path);
                }
                return StringUtil.MakePathList(results);
            }

            public string GetStateListString()
            {
                if (this.PathList == null)
                    return "";
                List<string> results = new List<string>();
                foreach (var item in this.PathList)
                {
                    results.Add(item.State);
                }
                return StringUtil.MakePathList(results);
            }
        }

        static BackupItemInfo GetBackupInfo(ListViewItem item)
        {
            BackupItemInfo info = item.Tag as BackupItemInfo;
            if (info == null)
            {
                info = new BackupItemInfo();
                item.Tag = info;
            }
            return info;
        }

        #region 下载大备份文件

        public class AskResult : NormalResult
        {
            // [out]
            public bool Append { get; set; }

            // [out] 询问后，操作者决定从处理文件列表中删除(也就是不下载这些文件)的事项
            public List<DownloadFileInfo> DeletedFiles { get; set; }
        }

        // TODO: 将中途打算删除的文件留到函数返回前一刹那再删除
        // 询问是否覆盖已有的目标下载文件。整体询问
        // parameters:
        //      style   风格。
        //              如果包含 auto，表示尽量自动按照追加下载处理而减少弹出对话框询问
        //              如果包含 clearBefore，表示直接删除本地已有同名文件，不再询问
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   同意启动下载
        public async Task<AskResult> AskOverwriteFiles(// List<string> filenames,
            LibraryChannel channel,
            List<DownloadFileInfo> fileinfos,
            string style,
            string strOutputFolder
            //out bool bAppend,
            //out string strError
            )
        {
            // string strError = "";
            bool bAppend = false;
            if (string.IsNullOrEmpty(strOutputFolder))
            {
                throw new ArgumentException("strOutputFolder 参数值不允许为空");
            }

            bool clearBefore = StringUtil.IsInList("clearBefore", style);

            List<DownloadFileInfo> deleted = new List<DownloadFileInfo>();

            DialogResult md5_result = System.Windows.Forms.DialogResult.Yes;
            // bool bDontAskMd5Verify = false; // 是否要询问 MD5 校验
            bool bDontAskMd5Verify = StringUtil.IsInList("auto", style);    // // 是否要询问 MD5 校验

            // 检查目标文件的存在情况
            foreach (DownloadFileInfo info in fileinfos)
            {
                string filename = info.ServerPath;
                string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(filename));
                info.LocalPath = strTargetPath;

                // all_target_filenames.Add(strTargetPath);

                string strTargetTempPath = info.GetTempFileName();  // DynamicDownloader.GetTempFileName(strTargetPath);

                // 观察临时文件是否已经存在
                if (File.Exists(strTargetTempPath))
                {
                    info.TempFileExists = true;
                    //states.Add("temp_exists");
                    //temp_filenames.Add(strTargetPath);
                    continue;   // 一旦一个文件的临时文件存在，那么就不在探索正式文件是否存在、以及它的 MD5 是否匹配
                }

                // 观察目标文件是否已经存在
                if (File.Exists(strTargetPath))
                {
                    info.LocalFileExists = true;

                    if (clearBefore)
                    {
                        // 把这种情况当作 MD5 不匹配一样处理
                        info.MD5Matched = "no";
                    }
                    else
                    if (filename.StartsWith("!"))
                    {
                        if (bDontAskMd5Verify == false)
                        {
                            md5_result = MessageDialog.Show(this,
                                // "是否需要进行 MD5 验证",
                                "文件 '" + strTargetPath + "' 已经存在。\r\n\r\n是否对它进行服务器侧 MD5 验证?\r\n\r\n",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxDefaultButton.Button1,
                                "后面遇同类情况，不再出现本对话框询问",
                                ref bDontAskMd5Verify,
                                new string[] { "是(验证)", "否(不验证)", "取消本次文件下载任务" },
                                20);
                            if (md5_result == System.Windows.Forms.DialogResult.Cancel)
                                return new AskResult
                                {
                                    Value = 0,
                                    DeletedFiles = deleted
                                };
                        }

                        if (md5_result == System.Windows.Forms.DialogResult.Yes)
                        {
                            var result = await Task.Run<NormalResult>(() =>
                            {
                                CancellationTokenSource cancel = new CancellationTokenSource();

                                // 出现一个对话框，允许中断获取 MD5 的过程
                                FileDownloadDialog dlg = null;
                                this.Invoke((Action)(() =>
                                {
                                    dlg = new FileDownloadDialog();
                                    dlg.Font = this.Font;
                                    dlg.Text = $"正在获取 MD5: {strTargetPath}";
                                    dlg.SourceFilePath = strTargetPath;
                                    dlg.TargetFilePath = null;
                                    // 让 Progress 变为走马灯状态
                                    dlg.StartMarquee();
                                }));
                                dlg.FormClosed += new FormClosedEventHandler(delegate (object o1, FormClosedEventArgs e1)
                                {
                                    cancel.Cancel();
                                });
                                this.Invoke((Action)(() =>
                                {
                                    dlg.Show(this);
                                }));
                                try
                                {
                                    return _checkMD5(channel,
                                        filename,
                                        strTargetPath,
                                        cancel.Token);
                                }
                                finally
                                {
                                    this.Invoke((Action)(() =>
                                    {
                                        dlg.Close();
                                    }));
                                    cancel.Dispose();
                                }
                            });

                            if (result.Value == -1)
                            {
                                return new AskResult
                                {
                                    Value = -1,
                                    ErrorInfo = result.ErrorInfo,
                                    DeletedFiles = deleted
                                };
                            }
                            if (result.Value == 0)
                            {
                                info.MD5Matched = "no";
                                continue;
                            }
                            else if (result.Value == 1)
                                info.MD5Matched = "yes";
                        }
                    }

                    //states.Add("exists");
                    //if (File.Exists(strTargetTempPath))
                    //    File.Delete(strTargetTempPath); // 防范性地删除临时文件
                    //continue;
                }
            }

            // 没有任何目标文件和临时文件存在
            {
                List<string> local_exists = GetFileNames(fileinfos, (info) =>
                {
                    if (info.LocalFileExists)
                        return (info.LocalPath);
                    return null;
                });
                List<string> temp_exists = GetFileNames(fileinfos, (info) =>
                {
                    if (info.TempFileExists)
                        return (info.LocalPath);
                    return null;
                });
                if (local_exists.Count + temp_exists.Count == 0)
                {
                    return new AskResult
                    {
                        Value = 1,
                        Append = false,
                        DeletedFiles = deleted
                    };
                }
            }

            List<string> delete_filenames = new List<string>();
            try
            {
                // MD5 不匹配的文件
                List<string> md5_mismatch_filenames = new List<string>();   // 正式文件存在的，并且 MD5 经过探测发现不匹配的
                md5_mismatch_filenames = GetFileNames(fileinfos, (info) =>
                {
                    if (info.MD5Matched == "no")
                        return info.LocalPath;
                    return null;
                });

                if (md5_mismatch_filenames.Count > 0)
                {
                    DialogResult result = DialogResult.Yes;
                    if (clearBefore == false)
                    {
                        result = MessageBox.Show(this,
    "下列文件中 '" + GetFileNameList(md5_mismatch_filenames, "\r\n") + "' 先前曾经被下载过，但 MD5 验证发现和服务器侧文件不一致。\r\n\r\n是否删除它们然后重新下载?\r\n[是：重新下载; 否: 不下载这些文件; 取消：放弃全部下载]",
    "MainForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    }
                    if (result == DialogResult.Cancel)
                    {
                        delete_filenames.Clear();
                        return new AskResult
                        {
                            Value = 0,
                            Append = bAppend,
                            DeletedFiles = deleted
                        };
                    }
                    if (result == DialogResult.Yes)
                    {
                        // 删除本地文件，确保后面会重新下载
                        ProcessItems(fileinfos, (info) =>
                        {
                            if (info.MD5Matched == "no")
                            {
                                // File.Delete(info.LocalPath);
                                delete_filenames.Add(info.LocalPath);

                                info.MD5Matched = "";
                                info.LocalFileExists = false;
                                delete_filenames.Add(info.GetTempFileName());
                                info.TempFileExists = false;
                            }
                        });
                    }
                    else
                    {
                        // 从文件列表中清除，这样就不会下载这些文件了
                        deleted.AddRange(DeleteItems(fileinfos,
                            (info) =>
                            {
                                return info.MD5Matched == "no";
                            }));
                    }
                }

                // 观察是否有 .tmp 文件存在
                List<string> temp_filenames = GetFileNames(fileinfos, (info) =>
                {
                    if (info.TempFileExists)
                        return info.LocalPath;
                    return null;
                });
                if (temp_filenames.Count > 0)
                {
                    DialogResult result = DialogResult.Yes;
                    if (StringUtil.IsInList("auto", style) == false)
                    {
                        result = MessageBox.Show(this,
            "下列文件 '" + GetFileNameList(temp_filenames, "\r\n") + "' 先前曾经被下载过，但未能完成。\r\n\r\n是否继续下载未完成部分?\r\n[是：从断点继续下载; 否: 重新从头下载; 取消：放弃全部下载]",
            "MainForm",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);
                    }
                    if (result == DialogResult.Cancel)
                    {
                        delete_filenames.Clear();
                        return new AskResult
                        {
                            Value = 0,
                            Append = bAppend,
                            DeletedFiles = deleted
                        };
                    }
                    if (result == DialogResult.Yes)
                    {
                        bAppend = true;
                        ProcessItems(fileinfos, (info) =>
                        {
                            if (info.TempFileExists)
                            {
                                // 保护性删除正式文件，但留下临时文件
                                // info.DeleteLocalFile();
                                delete_filenames.Add(info.LocalPath);

                                info.LocalFileExists = false;

                                info.OverwriteStyle = "append";
                            }
                        });
                    }
                    else
                    {
                        bAppend = false;

                        // 删除临时文件
                        ProcessItems(fileinfos, (info) =>
                        {
                            if (info.TempFileExists)
                            {
                                info.OverwriteStyle = "overwrite";
                                // 删除了临时文件
                                // info.DeleteTempFile();
                                delete_filenames.Add(info.GetTempFileName());
                                info.TempFileExists = false;
                            }
                        });
                    }
                }

                // 询问 MD5 验证过的文件是否重新下载？(建议不必重新下载)
                List<string> md5_matched_filenames = GetFileNames(fileinfos, (info) =>
                {
                    if (info.MD5Matched == "yes")
                        return info.LocalPath;
                    return null;
                });
                if (md5_matched_filenames.Count > 0)
                {
                    DialogResult result = DialogResult.No;
                    // 如果 style 中包含 "auto" 则不要询问
                    if (StringUtil.IsInList("auto", style) == false)
                    {
                        result = MessageBox.Show(this,
            "下列文件中 '" + GetFileNameList(md5_matched_filenames, "\r\n") + "' 先前曾经被下载过，并且 MD5 验证发现和服务器侧文件完全一致。\r\n\r\n是否删除它们然后重新下载?\r\n[是：重新下载; 否: 不下载这些文件; 取消：放弃全部下载]",
            "MainForm",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
                    }
                    if (result == DialogResult.Cancel)
                    {
                        delete_filenames.Clear();
                        return new AskResult
                        {
                            Value = 0,
                            Append = bAppend,
                            DeletedFiles = deleted
                        };
                    }
                    if (result == DialogResult.Yes)
                    {
                        // 删除本地文件，确保后面会重新下载
                        ProcessItems(fileinfos, (info) =>
                        {
                            if (info.MD5Matched == "yes")
                            {
                                // File.Delete(info.LocalPath);
                                delete_filenames.Add(info.LocalPath);
                                info.MD5Matched = "";
                                info.LocalFileExists = false;
                                // info.DeleteTempFile();
                                delete_filenames.Add(info.GetTempFileName());

                                info.TempFileExists = false;
                                info.OverwriteStyle = "overwrite";
                            }
                        });
                    }
                    else
                    {
                        // 从文件列表中清除，这样就不会下载这些文件了
                        deleted.AddRange(DeleteItems(fileinfos,
                            (info) =>
                            {
                                return info.MD5Matched == "yes";
                            }));
                    }
                }

                // 询问其余本地文件存在的，是否重新下载
                List<string> filenames = GetFileNames(fileinfos, (info) =>
                {
                    if (info.LocalFileExists)
                        return info.LocalPath;
                    return null;
                });
                if (filenames.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
        "下列文件中 '" + GetFileNameList(filenames, "\r\n") + "' 先前曾经被下载过。\r\n\r\n是否删除它们然后重新下载?\r\n[是：重新下载; 否: 不下载这些文件; 取消：放弃全部下载]",
        "MainForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                    {
                        delete_filenames.Clear();
                        return new AskResult
                        {
                            Value = 0,
                            Append = bAppend,
                            DeletedFiles = deleted
                        };
                    }
                    if (result == DialogResult.Yes)
                    {
                        // 删除本地文件，确保后面会重新下载
                        ProcessItems(fileinfos, (info) =>
                        {
                            if (info.LocalFileExists == true)
                            {
                                // File.Delete(info.LocalPath);
                                delete_filenames.Add(info.LocalPath);

                                info.MD5Matched = "";
                                info.LocalFileExists = false;
                                // info.DeleteTempFile();
                                delete_filenames.Add(info.GetTempFileName());

                                info.TempFileExists = false;
                                info.OverwriteStyle = "overwrite";
                            }
                        });
                    }
                    else
                    {
                        // 从文件列表中清除，这样就不会下载这些文件了
                        deleted.AddRange(DeleteItems(fileinfos,
                            (info) =>
                            {
                                return info.LocalFileExists;
                            }));
                    }
                }
                return new AskResult
                {
                    Value = 1,
                    Append = bAppend,
                    DeletedFiles = deleted
                };
            }
            finally
            {
                StringUtil.RemoveBlank(ref delete_filenames);
                foreach (string filename in delete_filenames)
                {
                    if (File.Exists(filename))
                        File.Delete(filename);
                }
            }
        }

        internal delegate bool Delegate_processItem2(DownloadFileInfo info);

        internal delegate void Delegate_processItem3(DownloadFileInfo info);

        static void ProcessItems(List<DownloadFileInfo> infos, Delegate_processItem3 func)
        {
            List<string> results = new List<string>();
            foreach (DownloadFileInfo info in infos)
            {
                func(info);
            }
        }

        // parameters:
        //      func    若 func 返回 true 表示要删除
        // return:
        //      返回实际删除的事项
        List<DownloadFileInfo> DeleteItems(List<DownloadFileInfo> fileinfos,
            Delegate_processItem2 func)
        {
            List<DownloadFileInfo> delete_infos = new List<DownloadFileInfo>();
            foreach (DownloadFileInfo info in fileinfos)
            {
                if (func(info) == true)
                    delete_infos.Add(info);
            }

            foreach (DownloadFileInfo info in delete_infos)
            {
                fileinfos.Remove(info);
            }

            return delete_infos;
        }


        static string GetFileNameList(List<string> filenames, string strSep = ",")
        {
            if (filenames.Count < 10)
                return StringUtil.MakePathList(filenames, strSep);
            List<string> temp = new List<string>();
            temp.AddRange(filenames.GetRange(0, 10));
            temp.Add("...");
            return StringUtil.MakePathList(temp, strSep);
        }

        /*
        Task<NormalResult> BeginCheckMD5(string strServerFilePath,
        string strLocalFilePath)
        {
            return Task.Factory.StartNew<NormalResult>(
        () =>
        {
        return _checkMD5(strServerFilePath, strLocalFilePath);
        });
        }
        */



        // result.Value:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        NormalResult _checkMD5(
            LibraryChannel channel,
            string strServerFilePath,
            string strLocalFilePath,
            CancellationToken token)
        {
            // string strError = "";

            /*
            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在验证服务器端文件 " + strServerFilePath + " 的 MD5 校验码 ...");
            Stop.BeginLoop();

            stopManager.Active(Stop);   // testing

            // Application.DoEvents();
            */

            try
            {
                // 检查 MD5
                // return:
                //      -1  出错
                //      0   文件没有找到
                //      1   文件找到
                int nRet = DynamicDownloader.GetServerFileMD5ByTask(
                    channel,
                    null,   // this.Stop,
                    strServerFilePath,
                    new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
                    {
                        // 遇到出错要可以 UI 交互重试

                        if (this.IsDisposed == true)
                        {
                            e1.ResultAction = "cancel";
                            return;
                        }

                        // func_showProgress?.Invoke(strPath, $"中途出错:{e1.MessageText}");

                        this.Invoke((Action)(() =>
                        {
                            if (e1.Actions == "yes,no,cancel")
                            {
                                bool bHideMessageBox = true;

                                string server_name = this.Servers.GetServer(channel.Url)?.Name;
                                if (string.IsNullOrEmpty(server_name))
                                    server_name = channel.Url;

                                DialogResult result = MessageDialog.Show(this,
                                    $"服务器 '{server_name}': { e1.MessageText}\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxDefaultButton.Button1,
                    null,
                    ref bHideMessageBox,
                    new string[] { "重试", "跳过", "放弃" },
                    20);
                                if (result == DialogResult.Cancel)
                                    e1.ResultAction = "cancel";
                                else if (result == System.Windows.Forms.DialogResult.No)
                                    e1.ResultAction = "no";
                                else
                                    e1.ResultAction = "yes";
                            }
                            else
                            {
                                e1.ResultAction = "yes";
                                // TODO: 是否延时一段？
                            }
                        }));
                    }),
                    token,
                    out byte[] server_md5,
                    out string strError);
                if (nRet != 1)
                {
                    strError = "探测服务器端文件 '" + strServerFilePath + "' MD5 时出错: " + strError;
                    return new NormalResult(-1, strError);
                }

                using (FileStream stream = File.OpenRead(strLocalFilePath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] local_md5 = DynamicDownloader.GetFileMd5(stream);
                    if (ByteArray.Compare(server_md5, local_md5) != 0)
                    {
                        strError = "服务器端文件 '" + strServerFilePath + "' 和本地文件 '" + strLocalFilePath + "' MD5 不匹配";
                        return new NormalResult(0, strError);
                    }
                }

                return new NormalResult(1, null);
            }
            finally
            {
                // this.ReturnChannel(channel);
            }
        }

        // 一个文件显示下载进度
        // parameters:
        //      path    服务器端文件名。如果为 null，表示希望直接设置进度文本
        public delegate void Delegate_showProgress(string path, string text);
        // 一个文件下载结束
        public delegate void Delegate_finish(string path, string error);

        public class BeginDownloadResult : NormalResult
        {
            // [out]
            // public DynamicDownloader Downloader { get; set; }
        }

        // 下载单个大备份文件
        // parameters:
        //      strPath 服务器端的文件路径
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        public async Task<BeginDownloadResult> BeginDownloadFile(
            BackupItemInfo info,
            // string strServerUrl,
            LibraryChannel channel,
            string strPath,
            string strAppendStyle,
            string strOutputFolder,
            Delegate_showProgress func_showProgress,
            Delegate_finish func_finish,
            CancellationToken token)
        {
            string strExt = Path.GetExtension(strPath);
            if (strExt == ".~state")
            {
                return new BeginDownloadResult
                {
                    Value = -1,
                    ErrorInfo = "状态文件是一种临时文件，不支持直接下载"
                };
            }

            string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(strPath));

            string strTargetTempPath = DynamicDownloader.GetTempFileName(strTargetPath);

            bool bAppend = false;   // 是否继续下载?

            if (strAppendStyle == "append")
            {
                bAppend = true;
                // 在 append 风格下，如果遇到正式目标文件已经存在，不再重新下载。
                // 注: 如果想要重新下载，需要用 overwrite 风格来调用
                if (File.Exists(strTargetPath))
                {
                    if (File.Exists(strTargetTempPath))
                        File.Delete(strTargetTempPath); // 防范性地删除
                    return new BeginDownloadResult { Value = 1 };
                }
            }
            else if (strAppendStyle == "overwrite")
            {
                bAppend = false;
                if (File.Exists(strTargetPath))
                    File.Delete(strTargetPath);
                if (File.Exists(strTargetTempPath))
                    File.Delete(strTargetTempPath);
            }
            else if (strAppendStyle == "ask")
            {
                // 观察目标文件是否已经存在
                if (File.Exists(strTargetPath))
                {
                    DialogResult result = MessageBox.Show(this,
        "目标文件 '" + strTargetPath + "' 已经存在。\r\n\r\n是否重新下载并覆盖它?\r\n[是：下载并覆盖; 取消：放弃下载]",
        "MainForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return new BeginDownloadResult { Value = 0 };

                    bAppend = false;
                    File.Delete(strTargetPath);
                    if (File.Exists(strTargetTempPath))
                        File.Delete(strTargetTempPath); // 防范性地删除
                }

                // 观察临时文件是否已经存在
                if (File.Exists(strTargetTempPath))
                {
                    DialogResult result = MessageBox.Show(this,
        "目标文件 '" + strTargetPath + "' 先前曾经被下载过，但未能完成。\r\n\r\n是否继续下载未完成部分?\r\n[是：从断点继续下载; 否: 重新从头下载; 取消：放弃下载]",
        "MainForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return new BeginDownloadResult { Value = 0 };

                    if (result == DialogResult.Yes)
                        bAppend = true;
                    else
                    {
                        File.Delete(strTargetTempPath);
                        if (File.Exists(strTargetPath))
                            File.Delete(strTargetPath); // 防范性地删除
                    }
                }
            }
            else
            {
                return new BeginDownloadResult
                {
                    Value = -1,
                    ErrorInfo = "未知的 strAppendStyle 值 '" + strAppendStyle + "'"
                };
            }

            func_showProgress?.Invoke(strPath, "等待通道 ...");

            var releaser = await _backupLimit.EnterAsync(token);

            /*
            LibraryChannel channel = this.GetChannel(strServerUrl);

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);
            */

            DynamicDownloader downloader = new DynamicDownloader(channel,
                strPath,
                strTargetPath);
            info.AddDownloader(downloader);

            // _downloaders.Add(downloader);

            downloader.Closed += new EventHandler(delegate (object o1, EventArgs e1)
            {
                /*
                if (channel != null)
                {
                    channel.Timeout = old_timeout;
                    this.ReturnChannel(channel);
                    channel = null;

                    releaser.Dispose();
                }
                */
                releaser.Dispose();

                {
                    // TODO: "finish:error" 表示下载完成，但服务器端的 .~state 文件内容为 "error"，表示服务器创建文件时出错而停止。比如磁盘空间满等原因
                    if (downloader.State.StartsWith("finish"))
                    {
                        var parts = StringUtil.ParseTwoPart(downloader.State, ":");
                        // 成功完成
                        if (string.IsNullOrEmpty(parts[1]))
                        {
                            func_finish.Invoke(strPath, null);
                            func_showProgress?.Invoke(strPath, "下载完成");
                        }
                        else
                        {
                            func_finish.Invoke(strPath, downloader.ErrorInfo);
                            func_showProgress?.Invoke(strPath, "下载完成但文件有错");
                        }
                    }
                    else
                    {
                        // 因出错中断
                        func_finish.Invoke(strPath, downloader.ErrorInfo);
                        func_showProgress?.Invoke(strPath, "error:" + downloader.ErrorInfo);
                    }
                }
                /*
                DisplayDownloaderErrorInfo(downloader);
                */
                // RemoveDownloader(downloader);
                info.RemoveDownloader(downloader);
            });
            string prev_text = "";
            DateTime prev_time = DateTime.MinValue;
            downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
            {
                string text = GetProgressText(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
                if (text != prev_text || DateTime.Now - prev_time > TimeSpan.FromSeconds(1))
                {
                    func_showProgress?.Invoke(strPath, text);
                    prev_text = text;
                    prev_time = DateTime.Now;
                }
                //if (dlg.IsDisposed == false)
                //    dlg.SetProgress(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
            });
            downloader.Prompt += new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
            {
                if (this.IsDisposed == true)
                {
                    e1.ResultAction = "cancel";
                    return;
                }

                func_showProgress?.Invoke(strPath, $"中途出错:{e1.MessageText}");

                this.Invoke((Action)(() =>
                {
                    if (e1.Actions == "yes,no,cancel")
                    {
                        bool bHideMessageBox = true;

                        string server_name = this.Servers.GetServer(channel.Url)?.Name;
                        if (string.IsNullOrEmpty(server_name))
                            server_name = channel.Url;

                        DialogResult result = MessageDialog.Show(this,
                            $"服务器 '{server_name}': { e1.MessageText}\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
            MessageBoxButtons.YesNoCancel,
            MessageBoxDefaultButton.Button1,
            null,
            ref bHideMessageBox,
            new string[] { "重试", "跳过", "放弃" },
            20);
                        if (result == DialogResult.Cancel)
                            e1.ResultAction = "cancel";
                        else if (result == System.Windows.Forms.DialogResult.No)
                            e1.ResultAction = "no";
                        else
                            e1.ResultAction = "yes";
                    }
                    else
                    {
                        e1.ResultAction = "yes";
                        // TODO: 是否延时一段？
                    }
                }));
            });
            // 注：启动下载但并不等待
            await downloader.StartDownload(bAppend, true);
            return new BeginDownloadResult { Value = 1 };
        }

        // TODO: 最好还能显示这是文件中的第一个还是第二个文件
        // 设置进度条信息
        // parameters:
        //      strText 文字。当 strText 为空的时候，函数用 bytesReceived 和 totalBytesToReceive 刷新设置进度条比例显示。否则只刷新显示 strText 文字
        //      bytesReceived   接收到多少 bytes
        //      totalBytesToReceive 总共计划接收多少 bytes
        static string GetProgressText(
            string strText,
            long bytesReceived,
            long totalBytesToReceive)
        {
            if (totalBytesToReceive > 0)
            {
                double ratio = (double)bytesReceived / (double)totalBytesToReceive;
                /*
                    this.progressBar1.Value = Convert.ToInt32((double)100 * ratio);

                    if (ratio == 100)
                        this.progressBar1.Style = ProgressBarStyle.Marquee;
                    else
                        this.progressBar1.Style = ProgressBarStyle.Continuous;
                        */

                // this.label_message.Text = bytesReceived.ToString() + " / " + totalBytesToReceive.ToString() + " " + this.SourceFilePath;
                return GetLengthText(bytesReceived) + " / " + GetLengthText(totalBytesToReceive);   // + " " + strSourceFilePath;
            }

            if (strText != null)
                return strText;

            return "正在启动 ...";
        }

        public static string[] units = new string[] { "K", "M", "G", "T" };
        public static string GetLengthText(long length)
        {
            decimal v = length;
            int i = 0;
            foreach (string strUnit in units)
            {
                v = decimal.Round(v / 1024, 2);
                if (v < 1024 || i >= units.Length - 1)
                    return v.ToString() + strUnit;

                i++;
            }

            return length.ToString();
        }

        /*
        List<DynamicDownloader> _downloaders = new List<DynamicDownloader>();
        private static readonly Object _syncRoot_downloaders = new Object();
        */

#if NO
        // parameters:
        //      downloader  要清除的 DynamicDownloader 对象。如果为 null，表示全部清除
        void RemoveDownloader(DynamicDownloader downloader,
            bool bTriggerClose = false)
        {
            List<DynamicDownloader> list = new List<DynamicDownloader>();
            lock (_syncRoot_downloaders)
            {
                if (downloader == null)
                {
                    list.AddRange(_downloaders);
                    _downloaders.Clear();
                }
                else
                {
                    list.Add(downloader);
                    // downloader.Close();
                    _downloaders.Remove(downloader);
                }
            }

            foreach (DynamicDownloader current in list)
            {
                current.Close();
            }
        }
#endif

        internal delegate string Delegate_processItem1(DownloadFileInfo info);

        internal static List<string> GetFileNames(List<DownloadFileInfo> infos,
            Delegate_processItem1 func)
        {
            List<string> results = new List<string>();
            foreach (DownloadFileInfo info in infos)
            {
                string strFileName = func(info);
                if (strFileName != null)
                    results.Add(strFileName);
            }

            return results;
        }

        public class DownloadFileInfo
        {
            public string ServerPath { get; set; }
            public string LocalPath { get; set; }
            // 本地文件是否存在
            public bool LocalFileExists { get; set; }
            // 本地和服务器端文件的 MD5 是否匹配
            public string MD5Matched { get; set; }   // 空/yes/no 
                                                     // 本地临时文件是否存在
            public bool TempFileExists { get; set; }

            public string OverwriteStyle { get; set; }  // append/overwrite

            public string GetTempFileName()
            {
                if (string.IsNullOrEmpty(this.LocalPath))
                    return "";
                return DynamicDownloader.GetTempFileName(this.LocalPath);
            }
        }

        public static List<DownloadFileInfo> BuildDownloadInfoList(List<string> filenames)
        {
            List<DownloadFileInfo> results = new List<DownloadFileInfo>();
            foreach (string filename in filenames)
            {
                DownloadFileInfo info = new DownloadFileInfo();
                info.ServerPath = filename;
                results.Add(info);
            }
            return results;
        }

        #endregion

        #region 大备份任务列表的保存和恢复

        void SaveBackupTasks()
        {
            List<BackupItemInfo> infos = new List<BackupItemInfo>();
            foreach (ListViewItem item in this.listView_backupTasks.Items)
            {
                infos.Add(GetBackupInfo(item));
            }

            string value = JsonConvert.SerializeObject(infos);
            ClientInfo.Config.Set("global", "backupTasks", value);
        }

        void LoadBackupTasks()
        {
            this.listView_backupTasks.Items.Clear();

            string value = ClientInfo.Config.Get("global", "backupTasks");
            if (string.IsNullOrEmpty(value))
                return;

            try
            {
                List<BackupItemInfo> infos = JsonConvert.DeserializeObject<List<BackupItemInfo>>(value);
                if (infos == null)
                    return;

                foreach (var info in infos)
                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = info;
                    ListViewUtil.ChangeItemText(item, COLUMN_SERVERNAME, info.ServerName);
                    ListViewUtil.ChangeItemText(item, COLUMN_SERVERFILES, info.GetPathListString());
                    this.listView_backupTasks.Items.Add(item);
                }
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {

            }

            RefreshMenuItems();
        }

        #endregion

        static string GetOutputFolder(string server_name)
        {
            if (string.IsNullOrEmpty(server_name))
                throw new ArgumentException("server_name 参数值不应为空");

            string root = OutputFolderRoot;
            if (string.IsNullOrEmpty(root))
                root = Path.Combine(ClientInfo.UserDir, "backup");

            string strOutputFolder = Path.Combine(root, $"{server_name}");
            PathUtil.CreateDirIfNeed(strOutputFolder);
            return strOutputFolder;
        }

        private async void MenuItem_continueBackupTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            items.AddRange(this.listView_backupTasks.SelectedItems.Cast<ListViewItem>());

            this.ShowMessage("正在启动任务");
            var result = await Task.Run<NormalResult>(() =>
            {
                try
                {
                    // CancellationToken token = _cancel.Token;

                    foreach (ListViewItem item in items)
                    {
                        string server_name = GetItemText(item, COLUMN_SERVERNAME);
                        string strOutputFolder = GetOutputFolder(server_name);

                        var info = GetBackupInfo(item);
                        // 如果下载正在运行，则不允许重复启动
                        if (info.IsRunning == true)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"任务 '{server_name}' 已经在运行中，无法重复启动"
                            };
                        }

                        info.CancelTask();
                        info.BeginTask();
                        info.BackupTask = DownloadBackupFiles(item,
                            strOutputFolder,
                            info.CancellationToken,
                            false,
                            true);
                        /*
                        var result = await DownloadBackupFiles(item, strOutputFolder, token);
                        if (result.Value == -1)
                        {
                            strError = result.ErrorInfo;
                            goto ERROR1;
                        }
                        */
                    }

                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    strError = "程序出现异常: " + ExceptionUtil.GetDebugText(ex);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }
                finally
                {
                    this.ClearMessage();
                }
            });
            if (result.Value == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_backupTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshMenuItems();

            RefreshTaskConsole();
        }

        #region 任务控制台信息

        ListViewItem _activeItem = null;

        CancellationTokenSource _cancelRefresh = new CancellationTokenSource();

        void RefreshTaskConsole()
        {
            if (this.listView_backupTasks.SelectedItems.Count == 1)
            {
                if (_activeItem != this.listView_backupTasks.SelectedItems[0])
                {
                    // 切换显示 Task Console
                    ClearTaskConsole();
                    _activeItem = this.listView_backupTasks.SelectedItems[0];

                    Task.Run(() =>
                    {
                        string server_url = this.Servers.GetServerByName(GetBackupInfo(_activeItem)?.ServerName)?.Url;
                        RefreshTaskConsole(server_url, true, _cancelRefresh.Token);
                    });
                }
            }
            else
            {
                ClearTaskConsole();
            }
        }

        void ClearTaskConsole()
        {
            _activeItem = null;
            ClearWebBrowser(webBrowser_backupTask, true);

            ResultTextDecoder = Encoding.UTF8.GetDecoder();
            CurResultOffs = 0;
            CurResultVersion = 0;

            _cancelRefresh?.Cancel();
            _cancelRefresh.Dispose();
            _cancelRefresh = new CancellationTokenSource();

            this.toolStripStatusLabel_message.Text = "";
        }

        Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder();
        long CurResultOffs = 0;
        long CurResultVersion = 0;

        // parameters:
        //      bRewind 是否顺便把指针拨向从头开始获取
        void ClearWebBrowser(WebBrowser webBrowser,
            bool bRewind)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
            doc.Write("<pre>");
            if (bRewind == true)
                this.CurResultOffs = 0; // 从头开始获取?
        }

        void RefreshTaskConsole(string server_url,
            bool get_message,
            CancellationToken token)
        {
            string strError = "";
            LibraryChannel channel = this.GetChannel(server_url);
            try
            {
                TimeSpan delta = TimeSpan.FromSeconds(0);
                BatchTaskInfo param = new BatchTaskInfo();
                if (get_message == false)
                {
                    param.MaxResultBytes = 0;
                }
                else
                {
                    param.MaxResultBytes = 4096;
                    /*
                    if (i >= 5)  // 如果发现尚未来得及获取的内容太多，就及时扩大“窗口”尺寸
                        param.MaxResultBytes = 100 * 1024;
                        */
                }

                for (int i = 0; ; i++)
                {
                    if (token.IsCancellationRequested)
                        return;

                    // TODO: 可以考虑用一个延时来调节获取信息的速度。如果很快追上了，就降低速度；否则就加快速度
                    try
                    {
                        Task.Delay(delta, token).Wait();
                    }
                    catch (AggregateException)
                    {
                        return;
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    param.ResultOffset = this.CurResultOffs;

                    long lRet = channel.BatchTask(
                        null,
                        "大备份",
                        "getinfo",
                        param,
                        out BatchTaskInfo resultInfo,
                        out strError);
                    if (lRet == -1)
                    {
                        this.Invoke((Action)(() =>
                        {
                            this.toolStripStatusLabel_message.Text = "BatchTask() 请求出错: " + strError;
                        }));
                        // 降低速度
                        delta = TimeSpan.FromSeconds(10);
                        continue;
                    }

                    string text = GetResultText(resultInfo.ResultText);
                    if (this.IsDisposed)
                        return;
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        this.Invoke((Action)(() =>
                        {
                            WriteHtml(this.webBrowser_backupTask, text);
                            ScrollToEnd();
                        }));
                    }

                    // 显示 Progress 文字行
                    {
                        this.Invoke((Action)(() =>
                        {
                            this.toolStripStatusLabel_message.Text = resultInfo.ProgressText;
                        }));
                    }

                    if (get_message == false)
                    {
                        // 没有必要显示累积
                        break;
                    }

                    if (this.CurResultOffs == 0)
                        this.CurResultVersion = resultInfo.ResultVersion;
                    else if (this.CurResultVersion != resultInfo.ResultVersion)
                    {
                        // 说明服务器端result文件其实已经更换
                        this.CurResultOffs = 0; // rewind
                        this.Invoke((Action)(() =>
                        {
                            WriteHtml(this.webBrowser_backupTask,
                            "***新内容 version=" + resultInfo.ResultVersion.ToString() + " ***\r\n");
                            ScrollToEnd();
                        }));
                        goto COINTINU1;
                    }

                    if (resultInfo.ResultTotalLength < param.ResultOffset)
                    {
                        // 说明服务器端result文件其实已经更换
                        this.CurResultOffs = 0; // rewind
                        this.Invoke((Action)(() =>
                        {
                            WriteHtml(this.webBrowser_backupTask,
                            "***新内容***\r\n");
                            ScrollToEnd();
                        }));
                        goto COINTINU1;
                    }
                    else
                    {
                        // 存储用以下次
                        this.CurResultOffs = resultInfo.ResultOffset;
                    }

                COINTINU1:
                    // 如果本次并没有“触底”，需要立即循环获取新的信息。但是循环有一个最大次数，以应对服务器疯狂发生信息的情形。
                    if (resultInfo.ResultOffset >= resultInfo.ResultTotalLength)
                    {
                        // 降低速度
                        delta = TimeSpan.FromSeconds(1);
                        // 减小包
                        param.MaxResultBytes = 4096;
                    }
                    else
                    {
                        // 提高速度
                        delta = TimeSpan.FromSeconds(0);
                        // 加大包
                        param.MaxResultBytes = 100 * 1024;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                this.Invoke((Action)(() =>
                {
                    WriteHtml(this.webBrowser_backupTask,
                    $"*** RefreshTaskConsole() 出现异常 ***\r\n{ExceptionUtil.GetDebugText(ex)}\r\n");
                    ScrollToEnd();
                }));
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        void ScrollToEnd()
        {
            this.webBrowser_backupTask.ScrollToEnd();
        }

        string GetResultText(byte[] baResult)
        {
            if (baResult == null)
                return "";
            if (baResult.Length == 0)
                return "";

            // Decoder ResultTextDecoder = Encoding.UTF8.GetDecoder;
            char[] chars = new char[baResult.Length];

            int nCharCount = this.ResultTextDecoder.GetChars(
                baResult,
                    0,
                    baResult.Length,
                    chars,
                    0);
            Debug.Assert(nCharCount <= baResult.Length, "");
            return new string(chars, 0, nCharCount);
        }

        #endregion

        void RefreshMenuItems()
        {
            // 统计出处于尚未启动状态的事项数
            int nStopCount = 0;
            foreach (ListViewItem item in this.listView_backupTasks.SelectedItems)
            {
                var state = ListViewUtil.GetItemText(item, COLUMN_STATE);
                if (string.IsNullOrEmpty(state))
                    nStopCount++;
            }

            MenuItem_continueBackupTasks.Enabled = nStopCount > 0;
        }

        private void listView_backupTasks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_backupTasks;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新建大备份任务 (&B)");
            menuItem.Click += new System.EventHandler(this.MenuItem_newBackupTasks_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新建静止状态的大备份任务 (&P)");
            menuItem.Click += new System.EventHandler(this.MenuItem_newStoppedBackupTasks_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("停止下载 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&T)");
            menuItem.Click += new System.EventHandler(this.MenuItem_stopBackupTasks_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("重启下载 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.MenuItem_continueBackupTasks_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("撤销服务器端大备份任务 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&C)");
            menuItem.Click += new System.EventHandler(this.menu_cancelServerBackupTask_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除服务器端备份文件 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteServerFile_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("打开服务器文件夹 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_openBackupServerFolder_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("打开本地文件夹 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&F)");
            menuItem.Click += new System.EventHandler(this.menu_openBackupLocalFolder_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_removeBackupTasks_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_backupTasks, new Point(e.X, e.Y));
        }

        void menu_openBackupServerFolder_Click(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();
            foreach (ListViewItem item in this.listView_backupTasks.SelectedItems)
            {
                var info = GetBackupInfo(item);
                var server = this.Servers.GetServerByName(info.ServerName);
                if (server == null)
                {
                    errors.Add($"没有找到名为 '{info.ServerName}' 的服务器");
                    continue;
                }

                ServerFileSystemForm dlg = new ServerFileSystemForm();
                dlg.ServerName = server.Name;
                dlg.ServerUrl = server.Url;
                dlg.SelectedPath = "!backup";
                dlg.Show(this);
            }

            if (errors.Count > 0)
                MessageDlg.Show(this,
                    StringUtil.MakePathList(errors, "\r\n"),
                    "打开服务器文件夹时出错");
        }


        void menu_openBackupLocalFolder_Click(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();
            try
            {
                foreach (ListViewItem item in this.listView_backupTasks.SelectedItems)
                {
                    var info = GetBackupInfo(item);
                    string path = GetOutputFolder(info.ServerName);
                    try
                    {
                        System.Diagnostics.Process.Start(path);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"打开文件夹 '{path}' 时出现异常: {ExceptionUtil.GetAutoText(ex)}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add("程序出现异常: " + ExceptionUtil.GetDebugText(ex));
            }

            if (errors.Count > 0)
                MessageDlg.Show(this,
                    StringUtil.MakePathList(errors, "\r\n"),
                    "打开本地文件夹时出错");
        }

        void MenuItem_stopBackupTasks_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView_backupTasks.SelectedItems)
            {
                var info = GetBackupInfo(item);
                if (info.IsRunning)
                    info.CancelTask();
            }
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;

            ListViewUtil.SelectAllLines(list);
        }

        void menu_removeBackupTasks_Click(object sender, EventArgs e)
        {
            if (this.listView_backupTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移除的事项。");
                return;
            }

            DialogResult result = MessageBox.Show(this,
        "确实要移除选定的 " + this.listView_backupTasks.SelectedItems.Count.ToString() + " 个事项?",
        "MainForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            foreach (ListViewItem item in this.listView_backupTasks.SelectedItems)
            {
                var info = GetBackupInfo(item);
                info.CancelTask();
            }

            ListViewUtil.DeleteSelectedItems(this.listView_backupTasks);
        }

        // 撤销服务器端大备份任务
        async void menu_cancelServerBackupTask_Click(object sender, EventArgs e)
        {
            if (this.listView_backupTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要撤销服务器端任务的事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
        "确实要撤销选定的 " + this.listView_backupTasks.SelectedItems.Count.ToString() + " 个事项对应的服务器端大备份任务?\r\n\r\n注：撤销服务器端大备份任务后，就再也无法进行下载操作",
        "MainForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            items.AddRange(this.listView_backupTasks.SelectedItems.Cast<ListViewItem>());

            StringBuilder error = new StringBuilder();
            this.ShowMessage("正在撤销服务器端大备份任务 ...");
            try
            {
                await Task.Run(() =>
                {
                    foreach (ListViewItem item in items)
                    {
                        var delete_result = CancelServerBackupTask(item);
                        if (delete_result.Value == -1)
                            error.AppendLine(delete_result.ErrorInfo);
                    }
                });
            }
            finally
            {
                this.ClearMessage();
            }

            if (error.Length > 0)
                MessageDlg.Show(this, error.ToString(), "MainForm");
            else
                MessageBox.Show(this, "删除成功");
        }

        // 删除服务器端大备份文件。这个功能一般用于诊断和维护。因为正常下载结束时会自动删除服务器端的大备份文件
        async void menu_deleteServerFile_Click(object sender, EventArgs e)
        {
            if (this.listView_backupTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除服务器端备份文件的事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
        "确实要删除选定的 " + this.listView_backupTasks.SelectedItems.Count.ToString() + " 个事项对应的服务器端备份文件?\r\n\r\n注：删除服务器端备份文件后，就再也无法进行下载操作",
        "MainForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            List<ListViewItem> items = new List<ListViewItem>();
            items.AddRange(this.listView_backupTasks.SelectedItems.Cast<ListViewItem>());

            StringBuilder error = new StringBuilder();
            this.ShowMessage("正在删除服务器端备份文件 ...");
            try
            {
                await Task.Run(() =>
                {
                    foreach (ListViewItem item in items)
                    {
                        var delete_result = DeleteServerFiles(item);
                        if (delete_result.Value == -1)
                            error.AppendLine(delete_result.ErrorInfo);
                    }
                });
            }
            finally
            {
                this.ClearMessage();
            }

            if (error.Length > 0)
                MessageDlg.Show(this, error.ToString(), "MainForm");
            else
                MessageBox.Show(this, "删除成功");
        }

        // 根据行状态设置行背景色
        void SetBackupItemColor(ListViewItem item)
        {
            Debug.Assert(item.ListView == null ||
                item.ListView == this.listView_backupTasks, "");

            this.Invoke((Action)(() =>
            {
                var info = GetBackupInfo(item);
                if (info == null)
                {
                    item.BackColor = SystemColors.Control;
                    item.ForeColor = SystemColors.ControlText;
                    return;
                }
                string state = info.State;
                if (state == "finish")
                {
                    item.BackColor = Color.DarkGreen;
                    item.ForeColor = Color.White;
                }
                else if (state == "error")
                {
                    item.BackColor = Color.DarkRed;
                    item.ForeColor = Color.White;
                }
                else
                {
                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;
                }
            }));
        }

        #region 日志备份

        const int OPERLOG_COLUMN_SERVERNAME = 0;
        const int OPERLOG_COLUMN_STATE = 1;
        const int OPERLOG_COLUMN_STARTTIME = 2;
        const int OPERLOG_COLUMN_PROGRESS = 3;
        // const int OPERLOG_COLUMN_SERVERFILES = 4;

        NormalResult NewOperLogTask(ListView listView,
            dp2Server server)
        {
            string taskTypeName = "日备份任务";
            if (listView == this.listView_errorLogTasks)
                taskTypeName = "错误日志下载任务";

            var dup_result = (NormalResult)this.Invoke((Func<NormalResult>)(() =>
            {
                // 对列表中的事项进行查重
                var dup = ListViewUtil.FindItem(listView, server.Name, OPERLOG_COLUMN_SERVERNAME);
                if (dup != null)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"名为 '{server.Name}' 的{taskTypeName}已经存在，无法再次创建",
                        ErrorCode = "taskAlreadyExist"
                    };
                else
                    return new NormalResult();
            }));

            if (dup_result.Value == -1)
                return dup_result;

            string strOutputFolder = GetOutputLogFolder(listView, server.Name);

            ListViewItem item = null;
            this.Invoke((Action)(() =>
            {
                item = new ListViewItem();
                listView.Items.Add(item);
                ListViewUtil.ChangeItemText(item, OPERLOG_COLUMN_SERVERNAME, server.Name);
            }));

            var info = GetOperLogInfo(item);
            info.CancelTask();
            info.BeginTask();
            info.OperLogTask = GetOperLogFiles(
                    item,
                    strOutputFolder,
                    info.CancellationToken);
            /*
            var result = await Task.Run(() =>
            {
                return GetOperLogFiles(
                    item,
                    strOutputFolder);
            });
            if (result.Value == -1)
            {
                SetOperLogItemError(item, result.ErrorInfo);
            }

            return result;
            */
            return new NormalResult();
        }

        // 备份日志文件。即，把日志文件从服务器拷贝到本地目录。要处理好增量复制的问题。
        // return:
        //      -1  出错
        //      0   放弃下载，或者没有必要下载。提示信息在 strError 中
        //      1   成功启动了下载
        async Task<NormalResult> GetOperLogFiles(
            ListViewItem item,
            string strOutputFolder,
            CancellationToken token)
        {
            if (string.IsNullOrEmpty(strOutputFolder))
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = SetOperLogItemError(item, "strOutputFolder 参数值不应为空")
                };
            }

            try
            {
                var info = GetOperLogInfo(item);
                info.ServerName = ListViewUtil.GetItemText(item, OPERLOG_COLUMN_SERVERNAME);
                info.State = "";
                info.StartTime = DateTime.Now;
                SetOperLogItemColor(item);

                SetItemText(item, OPERLOG_COLUMN_STATE, "正在下载");
                SetItemText(item, OPERLOG_COLUMN_STARTTIME, info.StartTime.ToString());

                DateTime now = DateTime.Now;

                string strLastDate = ReadOperLogMemoryFile(item.ListView, strOutputFolder);
                if (string.IsNullOrEmpty(strLastDate) == false
                    && strLastDate.Length != 8)
                    strLastDate = "";

                // 列出已经下载的文件列表
                // 当天下载当天日期的日志文件，要创建一个同名的状态文件，表示它可能没有完成。以后再处理的时候，如果不再是当天，确保下载完成了，可以删除状态文件
                string pattern = "*.log";
                if (item.ListView == this.listView_errorLogTasks)
                    pattern = "log_*.txt";
                List<OperLogFileInfo> local_files = GetLocalOperLogFileNames(
                    strOutputFolder,
                    pattern,
                    strLastDate);

                // 可能会抛出异常
                List<OperLogFileInfo> server_files = GetServerOperLogFileNames(item, strLastDate);

                // 计算出尚未下载的文件
                string server_folder = "!operlog/";
                if (item.ListView == this.listView_errorLogTasks)
                    server_folder = "!log/";
                List<DownloadFileInfo> fileinfos = GetDownloadFileList(
                    server_folder,
                    local_files,
                    server_files);
                if (fileinfos.Count == 0)
                {
                    WriteOperLogMemoryFile(item.ListView, strOutputFolder, now);

                    SetItemText(item, OPERLOG_COLUMN_STATE, "没有新文件需要下载");
                    // TODO: 设置背景色为绿色
                    info.State = "finish";
                    SetOperLogItemColor(item);

                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "服务器端没有发现新增的日志文件"
                    };
                }

                string strFolder = strOutputFolder;
                // 关注以前曾经下载的，可能服务器端发生了变化的文件。从文件尺寸可以看出来。
                // return:
                //      -1  出错
                //      0   放弃下载
                //      1   成功启动了下载
                var result = await BeginDownloadFiles(
                    item,
                    fileinfos,
                    "append",
                    (bError) =>
                    {
                        // 写入记忆文件，然后提示结束
                        if (bError == false)
                            WriteOperLogMemoryFile(item.ListView, strFolder, now);
                    },
                    strOutputFolder,
                    token);
                if (result.Value == -1)
                {
                    info.State = "error";
                    // 2020/2/28
                    SetItemText(item, OPERLOG_COLUMN_STATE, "出错: " + result.ErrorInfo);
                    SetOperLogItemColor(item);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = SetOperLogItemError(item, "BackupOperLogFiles() 出现异常: " + ex.Message)
                };
            }
        }

        string GetServerUrl(string server_name)
        {
            var server = this.Servers.GetServerByName(server_name);
            if (server == null)
                throw new Exception($"没有找到名为 '{server_name}' 的服务器");
            return server.Url;
        }

        // 下载一系列操作日志文件
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        async Task<NormalResult> BeginDownloadFiles(
            ListViewItem item,
            List<DownloadFileInfo> fileinfos,
            string strAppendStyleParam,
            Delegate_end func_end,
            string strOutputFolder,
            CancellationToken token)
        {
            var item_info = GetOperLogInfo(item);

            if (string.IsNullOrEmpty(strOutputFolder))
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "strOutputFolder 参数值不应为空"
                };

            List<DynamicDownloader> current_downloaders = new List<DynamicDownloader>();

            string server_name = ListViewUtil.GetItemText(item, OPERLOG_COLUMN_SERVERNAME);
            if (string.IsNullOrEmpty(server_name))
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = "item 中服务器名列不应为空"
                };
            /*
            var server = this.Servers.GetServerByName(server_name);
            if (server == null)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"没有找到名为 '{server_name}' 的服务器"
                };
                */

            SetItemText(item, OPERLOG_COLUMN_PROGRESS, "等待通道 ...");

            var releaser = await _operLogLimit.EnterAsync(token);

            LibraryChannel channel = this.GetChannel(GetServerUrl(server_name));

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            /*
            FileDownloadDialog dlg = new FileDownloadDialog();
            dlg.FormClosed += new FormClosedEventHandler(delegate (object o1, FormClosedEventArgs e1)
            {
                foreach (DynamicDownloader current in current_downloaders)
                {
                    current.Cancel();
                }
            });
            dlg.Font = this.Font;
            //dlg.Text = //"正在下载 " + strPath;
            //dlg.SourceFilePath = //strPath;
            //dlg.TargetFilePath = //strTargetPath;
            dlg.Show(this);
            */

            bool bDone = false;
            try
            {
                bool bAppend = false;   // 是否继续下载?
                List<string> errors = new List<string>();
                foreach (DownloadFileInfo fileinfo in fileinfos)
                {
                    // 2020/2/28
                    if (token.IsCancellationRequested)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "中断",
                            ErrorCode = "cancelled"
                        };
                    }

                    string strPath = fileinfo.ServerPath;

                    string strExt = Path.GetExtension(strPath);
                    if (strExt == ".~state")
                    {
                        // strError = "状态文件是一种临时文件，不支持直接下载";
                        // return -1;
                        continue;
                    }

                    string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(strPath));

                    string strTargetTempPath = DynamicDownloader.GetTempFileName(strTargetPath);

                    string strAppendStyle = fileinfo.OverwriteStyle;
                    if (string.IsNullOrEmpty(strAppendStyle))
                        strAppendStyle = strAppendStyleParam;

                    if (strAppendStyle == "append")
                    {
                        bAppend = true;
                        // TODO: 要检查 MD5 是否一致。如果不一致依然要重新下载
                        // 在 append 风格下，如果遇到正式目标文件已经存在，不再重新下载。
                        // 注: 如果想要重新下载，需要用 overwrite 风格来调用
                        if (File.Exists(strTargetPath))
                        {
                            if (File.Exists(strTargetTempPath))
                                File.Delete(strTargetTempPath); // 防范性地删除
                            continue;
                        }
                    }
                    else if (strAppendStyle == "overwrite")
                    {
                        bAppend = false;
                        if (File.Exists(strTargetPath))
                            File.Delete(strTargetPath);
                        if (File.Exists(strTargetTempPath))
                            File.Delete(strTargetTempPath);
                    }
                    else if (strAppendStyle == "ask")
                    {
                        // 观察目标文件是否已经存在
                        if (File.Exists(strTargetPath))
                        {
                            DialogResult result = MessageBox.Show(this,
                "目标文件 '" + strTargetPath + "' 已经存在。\r\n\r\n是否重新下载并覆盖它?\r\n[是：下载并覆盖; 取消：放弃下载]",
                "MainForm",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
                            if (result == DialogResult.Cancel)
                                return new NormalResult
                                {
                                    Value = 0,
                                    ErrorInfo = "放弃下载"
                                };
                            bAppend = false;
                            File.Delete(strTargetPath);
                            if (File.Exists(strTargetTempPath))
                                File.Delete(strTargetTempPath); // 防范性地删除
                        }

                        // 观察临时文件是否已经存在
                        if (File.Exists(strTargetTempPath))
                        {
                            DialogResult result = MessageBox.Show(this,
                "目标文件 '" + strTargetPath + "' 先前曾经被下载过，但未能完成。\r\n\r\n是否继续下载未完成部分?\r\n[是：从断点继续下载; 否: 重新从头下载; 取消：放弃下载]",
                "MainForm",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);
                            if (result == DialogResult.Cancel)
                                return new NormalResult
                                {
                                    Value = 0,
                                    ErrorInfo = "放弃下载"
                                };

                            if (result == DialogResult.Yes)
                                bAppend = true;
                            else
                            {
                                File.Delete(strTargetTempPath);
                                if (File.Exists(strTargetPath))
                                    File.Delete(strTargetPath); // 防范性地删除
                            }
                        }
                    }
                    else
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "未知的 strAppendStyle 值 '" + strAppendStyle + "'"
                        };
                    }

                    DynamicDownloader downloader = new DynamicDownloader(channel,
                        strPath,
                        strTargetPath);
                    downloader.Tag = item;

                    item_info.AddDownloader(downloader);

                    downloader.Closed += new EventHandler(delegate (object o1, EventArgs e1)
                    {
                        // TODO: 如何在 item 中显示多行报错？
                        if (string.IsNullOrEmpty(downloader.ErrorInfo) == false)
                        {
                            string error = "下载 " + downloader.ServerFilePath + "-->" + downloader.LocalFilePath + " 过程中出错: " + downloader.ErrorInfo;
                            SetItemText(item, OPERLOG_COLUMN_STATE, error);
                            errors.Add(error);
                        }
                        // DisplayDownloaderErrorInfo(downloader);

                        item_info.RemoveDownloader(downloader);
                    });
                    downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
                    {
                        /*
                        if (dlg.IsDisposed == false)
                            dlg.SetProgress(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
                            */
                        // TODO: 如何在 item 中显示多个进度?
                        SetItemText(item, OPERLOG_COLUMN_PROGRESS, GetProgressText(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive) + " " + downloader.ServerFilePath);
                    });
                    // 2017/10/7
                    downloader.Prompt += new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
                    {
                        if (this.IsDisposed == true)
                        {
                            e1.ResultAction = "cancel";
                            return;
                        }

                        this.Invoke((Action)(() =>
                        {
                            if (e1.Actions == "yes,no,cancel")
                            {
                                bool bHideMessageBox = true;
                                DialogResult result = MessageDialog.Show(this,
                                    e1.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxDefaultButton.Button1,
                    null,
                    ref bHideMessageBox,
                    new string[] { "重试", "跳过", "放弃" },
                    20);
                                if (result == DialogResult.Cancel)
                                    e1.ResultAction = "cancel";
                                else if (result == System.Windows.Forms.DialogResult.No)
                                    e1.ResultAction = "no";
                                else
                                    e1.ResultAction = "yes";
                            }
                        }));
                    });

                    current_downloaders.Add(downloader);
                }

                if (current_downloaders.Count == 0)
                {
                    SetItemText(item, OPERLOG_COLUMN_STATE, "没有文件需要下载");
                    // 显示绿色背景色
                    item_info.State = "finish";
                    SetOperLogItemColor(item);
                }
                else
                {
                    var result = await SequenceDownloadFiles(current_downloaders,
                        bAppend,
                        token,
                        (bError) =>
                        {
                            if (channel != null)
                            {
                                channel.Timeout = old_timeout;
                                this.ReturnChannel(channel);
                                channel = null;

                                releaser.Dispose();
                            }
                            /*
                            this.Invoke((Action)(() =>
                            {
                                dlg.Close();
                            }));
                            */
                            foreach (DynamicDownloader current in current_downloaders)
                            {
                                current.Close();
                            }

                            // 在 state 列中显示 errors 报错
                            if (errors.Count > 0)
                            {
                                SetItemText(item, OPERLOG_COLUMN_STATE, $"错误({errors.Count}): " + StringUtil.MakePathList(errors, ";"));
                                item_info.State = "error";
                            }
                            else
                            {
                                SetItemText(item, OPERLOG_COLUMN_STATE, $"下载完成({current_downloaders.Count})");
                                item_info.State = "finish";
                                SetItemText(item, OPERLOG_COLUMN_PROGRESS, $"完成");
                            }

                            // 改变 item 背景色
                            SetOperLogItemColor(item);

                            if (func_end != null)
                                func_end(bError);
                        });
                    if (result.Value == -1)
                        return result;
#if NO
                    await Task.Factory.StartNew(() => SequenceDownloadFiles(current_downloaders,
                        bAppend,
                        token,
                        (bError) =>
                        {
                            if (channel != null)
                            {
                                channel.Timeout = old_timeout;
                                this.ReturnChannel(channel);
                                channel = null;

                                releaser.Dispose();
                            }
                            /*
                            this.Invoke((Action)(() =>
                            {
                                dlg.Close();
                            }));
                            */
                            foreach (DynamicDownloader current in current_downloaders)
                            {
                                current.Close();
                            }

                            // 在 state 列中显示 errors 报错
                            if (errors.Count > 0)
                            {
                                SetItemText(item, OPERLOG_COLUMN_STATE, $"错误({errors.Count}): " + StringUtil.MakePathList(errors, ";"));
                                item_info.State = "error";
                            }
                            else
                            {
                                SetItemText(item, OPERLOG_COLUMN_STATE, $"下载完成({current_downloaders.Count})");
                                item_info.State = "finish";
                                SetItemText(item, OPERLOG_COLUMN_PROGRESS, $"完成");
                            }

                            // 改变 item 背景色
                            SetOperLogItemColor(item);

                            if (func_end != null)
                                func_end(bError);
                        }),
        token,
        TaskCreationOptions.LongRunning,
        TaskScheduler.Default);
#endif
                }

                bDone = true;
                return new NormalResult { Value = 1 };
            }
            finally
            {
                if (bDone == false)
                {
                    if (channel != null)
                    {
                        channel.Timeout = old_timeout;
                        this.ReturnChannel(channel);
                        channel = null;

                        releaser.Dispose();
                    }
                    /*
                    this.Invoke((Action)(() =>
                    {
                        dlg.Close();
                    }));
                    */
                    foreach (DynamicDownloader current in current_downloaders)
                    {
                        current.Close();
                    }
                }
            }
        }

        /*
        void DisplayDownloaderErrorInfo(DynamicDownloader downloader)
        {
            if (string.IsNullOrEmpty(downloader.ErrorInfo) == false
                && downloader.ErrorInfo.StartsWith("~") == false)
            {
                this.Invoke((Action)(() =>
                {
                    MessageBox.Show(this, "下载 " + downloader.ServerFilePath + "-->" + downloader.LocalFilePath + " 过程中出错: " + downloader.ErrorInfo);
                }));
                downloader.ErrorInfo = "~" + downloader.ErrorInfo;  // 只显示一次
            }
        }
        */

        public delegate void Delegate_end(bool bError);

        // 顺序执行每个 DynamicDownloader
        async Task<NormalResult> SequenceDownloadFiles(List<DynamicDownloader> downloaders,
            bool bAppend,
            CancellationToken token,
            Delegate_end func_end)
        {
            int i = 0;
            bool bError = false;
            try
            {
                foreach (DynamicDownloader downloader in downloaders)
                {
                    string strNo = "";
                    if (downloaders.Count > 0)
                        strNo = " " + (i + 1).ToString() + "/" + downloaders.Count + " ";

                    this.Invoke((Action)(() =>
                    {
                        ListViewItem item = downloader.Tag as ListViewItem;
                        Debug.Assert(item != null, "");
                        SetItemText(item, OPERLOG_COLUMN_STATE, $"正在下载 {strNo} {downloader.ServerFilePath}");
                    }));
                    await downloader.StartDownload(bAppend, true);
                    if (downloader.IsCancellationRequested
                        || token.IsCancellationRequested)
                    {
                        bError = true;
                        break;
                    }
                    if (downloader.State == "error")
                    {
                        bError = true;
                        break;
                    }
                    i++;
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                bError = true;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = "exception:" + ex.GetType().ToString()
                };
            }
            finally
            {
                func_end(bError);
            }
        }

        string GetMemoryFileName(ListView listView, string strDirectory)
        {
            return Path.Combine(strDirectory,
    listView == this.listView_operLogTasks ?
    "operlog_backup.txt" : "memory_backup.txt");
        }

        // 写入记忆当前日期的文件
        void WriteOperLogMemoryFile(ListView listView, string strDirectory,
            DateTime now)
        {
            string filename = GetMemoryFileName(listView, strDirectory);
            File.WriteAllText(filename, DateTimeUtil.DateTimeToString8(now));
        }

        string ReadOperLogMemoryFile(ListView listView, string strDirectory)
        {
            string filename = GetMemoryFileName(listView, strDirectory);
            if (File.Exists(filename) == false)
                return null;
            return File.ReadAllText(filename);
        }

        List<DownloadFileInfo> GetDownloadFileList(
            string server_folder,
            List<OperLogFileInfo> local_files,
            List<OperLogFileInfo> server_files)
        {
            List<DownloadFileInfo> results = new List<DownloadFileInfo>();

            foreach (OperLogFileInfo server_info in server_files)
            {
                OperLogFileInfo local_info = Find(local_files, server_info.FileName);
                if (local_info != null
                    && local_info.Length == server_info.Length)
                    continue;

                DownloadFileInfo result = new DownloadFileInfo();
                // result.ServerPath = "!operlog/" + server_info.FileName;
                result.ServerPath = server_folder + server_info.FileName;
                if (local_info != null && local_info.Length != server_info.Length)
                    result.OverwriteStyle = "overwrite";
                else
                    result.OverwriteStyle = "append";
                results.Add(result);
            }

            return results;
        }

        static OperLogFileInfo Find(List<OperLogFileInfo> infos, string strFileName)
        {
            foreach (OperLogFileInfo info in infos)
            {
                if (info.FileName == strFileName)
                    return info;
            }

            return null;
        }

        class OperLogFileInfo
        {
            // 纯文件名
            public string FileName { get; set; }

            // 文件内容尺寸
            public long Length { get; set; }
        }

        static NormalResult CheckLibraryServerVersion(LibraryChannel channel)
        {
            long lRet = channel.GetVersion(null,
out string strVersion,
out string strUID,
out string strError);
            if (lRet == -1)
            {
                strError = "针对服务器 " + channel.Url + " 获得版本号的过程发生错误：" + strError;
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            // this.ServerUID = strUID;

            if (string.IsNullOrEmpty(strVersion) == true)
                strVersion = "2.0";

            // this.ServerVersion = strVersion;

            string base_version = "3.23"; // 3.23
            if (StringUtil.CompareVersion(strVersion, base_version) < 0)
            {
                strError = $"dp2library 服务器 '{channel.Url}' 版本必须升级为 " + base_version + " 以上时才能使用大备份和日志备份功能 (当前 dp2library 版本实际为 " + strVersion + ")";
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };
            }

            return new NormalResult();
        }

        List<OperLogFileInfo> GetServerOperLogFileNames(ListViewItem item,
            string strLastDate)
        {
            if (string.IsNullOrEmpty(strLastDate) == false
    && strLastDate.Length != 8)
                throw new ArgumentException("strLastDate 参数值如果不为空，应该是 8 字符", "strLastDate");

            List<OperLogFileInfo> results = new List<OperLogFileInfo>();

            string server_name = ListViewUtil.GetItemText(item, OPERLOG_COLUMN_SERVERNAME);
            if (string.IsNullOrEmpty(server_name))
                throw new Exception("item 中服务器名列不应为空");

            LibraryChannel channel = this.GetChannel(GetServerUrl(server_name));
            try
            {
                // 检查 dp2library 服务器版本号
                var check_result = CheckLibraryServerVersion(channel);
                if (check_result.Value == -1)
                    throw new Exception(check_result.ErrorInfo);

                string category = "!operlog";
                string pattern = "*.log";
                if (item.ListView == this.listView_errorLogTasks)
                {
                    category = "!log";
                    pattern = "log_*.txt";
                }

                FileItemLoader loader = new FileItemLoader(channel,
                    null,
                    category, // "!operlog",
                    pattern);
                foreach (FileItemInfo info in loader)
                {
                    string strName = Path.GetFileName(info.Name);
                    if (string.IsNullOrEmpty(strLastDate) == false
    && string.Compare(strName, strLastDate) < 0)
                        continue;

                    OperLogFileInfo result = new OperLogFileInfo();
                    result.FileName = strName;
                    result.Length = info.Size;
                    results.Add(result);
                }

                return results;
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // parameters:
        //      strLastDate 上次备份的最后日期，8 字符。如果为空，表示当前是首次备份
        static List<OperLogFileInfo> GetLocalOperLogFileNames(
            string strDirectory,
            string pattern,
            string strLastDate)
        {
            if (string.IsNullOrEmpty(strLastDate) == false
                && strLastDate.Length != 8)
                throw new ArgumentException("strLastDate 参数值如果不为空，应该是 8 字符", "strLastDate");

            DirectoryInfo di = new DirectoryInfo(strDirectory);

            FileInfo[] fis = di.GetFiles(pattern);

            List<OperLogFileInfo> results = new List<OperLogFileInfo>();
            foreach (FileInfo fi in fis)
            {
                if (string.IsNullOrEmpty(strLastDate) == false
                    && string.Compare(fi.Name, strLastDate) < 0)
                    continue;

                OperLogFileInfo result = new OperLogFileInfo
                {
                    FileName = fi.Name,
                    Length = fi.Length
                };
                results.Add(result);
            }
            return results;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class OperLogItemInfo : ItemInfoBase
        {
            // 不参与序列化
            public Task<NormalResult> OperLogTask { get; set; }

            // 服务器名
            [JsonProperty]
            public string ServerName { get; set; }

            // 首次启动任务的时间
            [JsonProperty]
            public DateTime StartTime { get; set; }

            [JsonProperty]
            public string State { get; set; }   // (null)/downloading/finish/error

            public bool IsRunning
            {
                get
                {
                    if (OperLogTask != null)
                    {
                        return !(OperLogTask.IsCompleted);
                    }
                    return false;
                }
            }
        }

        static OperLogItemInfo GetOperLogInfo(ListViewItem item)
        {
            OperLogItemInfo info = item.Tag as OperLogItemInfo;
            if (info == null)
            {
                info = new OperLogItemInfo();
                item.Tag = info;
            }
            return info;
        }

        // 根据行状态设置行背景色
        void SetOperLogItemColor(ListViewItem item)
        {
            Debug.Assert(item.ListView == null
                || item.ListView == this.listView_operLogTasks
                || item.ListView == this.listView_errorLogTasks,
                "");

            this.Invoke((Action)(() =>
            {
                var info = GetOperLogInfo(item);
                if (info == null)
                {
                    item.BackColor = SystemColors.Control;
                    item.ForeColor = SystemColors.ControlText;
                    return;
                }
                string state = info.State;
                if (state == "finish")
                {
                    item.BackColor = Color.DarkGreen;
                    item.ForeColor = Color.White;
                }
                else if (state == "error")
                {
                    item.BackColor = Color.DarkRed;
                    item.ForeColor = Color.White;
                }
                else
                {
                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;
                }
            }));
        }

        #endregion

        #region 日备份任务列表的保存和恢复

        void SaveOperLogTasks(ListView listView)
        {
            List<OperLogItemInfo> infos = new List<OperLogItemInfo>();
            foreach (ListViewItem item in listView.Items)
            {
                infos.Add(GetOperLogInfo(item));
            }

            string value = JsonConvert.SerializeObject(infos);
            ClientInfo.Config.Set("global", listView.Name, value);
        }

        void LoadOperLogTasks(ListView listView)
        {
            listView.Items.Clear();

            string value = ClientInfo.Config.Get("global", listView.Name);
            if (string.IsNullOrEmpty(value))
                return;

            try
            {
                List<OperLogItemInfo> infos = JsonConvert.DeserializeObject<List<OperLogItemInfo>>(value);
                if (infos == null)
                    return;

                foreach (var info in infos)
                {
                    ListViewItem item = new ListViewItem
                    {
                        Tag = info
                    };
                    ListViewUtil.ChangeItemText(item, OPERLOG_COLUMN_SERVERNAME, info.ServerName);
                    listView.Items.Add(item);
                }
            }
            catch (Newtonsoft.Json.JsonSerializationException)
            {

            }

            RefreshMenuItems();
        }

        string SetOperLogItemError(ListViewItem item, string error)
        {
            var info = GetOperLogInfo(item);
            info.State = "error";
            SetItemText(item, OPERLOG_COLUMN_STATE, $"任务出错: {error}");
            SetOperLogItemColor(item);
            // TODO: 修改背景颜色为红色
            return error;
            /*
            this.Invoke((Action)(() =>
            {
                // TODO: 修改背景颜色为红色
                ListViewUtil.ChangeItemText(item, OPERLOG_COLUMN_STATE, $"error:{error}");
            }));
            */
        }

        #endregion


        private void listView_operLogTasks_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView_operLogTasks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新建下载日备份任务 (&B)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.MenuItem_newOperLogTasks_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("停止下载 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&T)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.MenuItem_stopOperLogTasks_Click);
            if (this.listView_operLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("重启下载 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.MenuItem_continueOperLogTasks_Click);
            if (this.listView_operLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("打开本地文件夹 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&F)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_openOperLogLocalFolder_Click);
            if (this.listView_operLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("移除 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Tag = this.listView_operLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_removeOperLogTasks_Click);
            if (this.listView_operLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_operLogTasks, new Point(e.X, e.Y));
        }

        // 打开操作日志/错误日志本地文件夹
        void menu_openOperLogLocalFolder_Click(object sender, EventArgs e)
        {
            ListView listView = GetMenuItemTag(sender);
            Debug.Assert(listView != null, "");

            List<string> errors = new List<string>();
            try
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    var info = GetOperLogInfo(item);
                    string strOutputFolder = GetOutputLogFolder(listView, info.ServerName);

                    try
                    {
                        System.Diagnostics.Process.Start(strOutputFolder);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"打开文件夹 '{strOutputFolder}' 时出现异常: {ExceptionUtil.GetAutoText(ex)}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add("程序出现异常: " + ExceptionUtil.GetDebugText(ex));
            }

            if (errors.Count > 0)
                MessageDlg.Show(this,
                    StringUtil.MakePathList(errors, "\r\n"),
                    "打开本地文件夹时出错");
        }

        // 停止(操作日志/错误日志)下载任务
        void MenuItem_stopOperLogTasks_Click(object sender, EventArgs e)
        {
            ListView listView = GetMenuItemTag(sender);
            Debug.Assert(listView != null, "");

            foreach (ListViewItem item in listView.SelectedItems)
            {
                var info = GetOperLogInfo(item);
                if (info.IsRunning)
                    info.CancelTask();
            }
        }

        // TODO: 移除前先停止任务?
        // 移除(操作日志/错误日志)下载任务
        void menu_removeOperLogTasks_Click(object sender, EventArgs e)
        {
            ListView listView = GetMenuItemTag(sender);
            Debug.Assert(listView != null, "");

            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移除的事项。");
                return;
            }

            DialogResult result = MessageBox.Show(this,
        "确实要移除选定的 " + listView.SelectedItems.Count.ToString() + " 个事项?",
        "MainForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            foreach (ListViewItem item in listView.SelectedItems)
            {
                var info = GetOperLogInfo(item);
                if (info.IsRunning)
                    info.CancelTask();
            }

            ListViewUtil.DeleteSelectedItems(listView);
        }

        static ListView GetMenuItemTag(object sender)
        {
            MenuItem menuItem = sender as MenuItem;
            Debug.Assert(menuItem != null, "");
            return menuItem.Tag as ListView;
        }

        // 新建下载操作日志/错误日志任务
        async void MenuItem_newOperLogTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            ListView listView = GetMenuItemTag(sender);
            Debug.Assert(listView != null, "");

            var server_names = SelectServerNames();
            if (server_names == null)
                return;

            CancellationToken token = _cancel.Token;

            List<string> warnings = new List<string>();
            this.ShowMessage("正在创建和启动任务");
            try
            {
                var result = await Task.Run<NormalResult>(() =>
                {
                    foreach (var server_name in server_names)
                    {
                        token.ThrowIfCancellationRequested();

                        dp2Server server = this.Servers.GetServerByName(server_name);
                        if (server == null)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"名为 '{server_name}' 的服务器不存在..."
                            };
                        }

                        var new_result = NewOperLogTask(listView, server);
                        if (new_result.Value == -1)
                        {
                            if (new_result.ErrorCode == "taskAlreadyExist")
                                warnings.Add(new_result.ErrorInfo);
                        }
                    }

                    return new NormalResult();
                });
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }
            catch (Exception ex)
            {
                strError = "程序出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                this.ClearMessage();
            }

            if (warnings.Count > 0 && token.IsCancellationRequested == false)
                MessageDlg.Show(this, "警告:\r\n" + StringUtil.MakePathList(warnings, "\r\n"), "警告");
            return;
        ERROR1:
            if (token.IsCancellationRequested == false)
                MessageBox.Show(this, strError);
        }

        async void MenuItem_continueOperLogTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            ListView listView = GetMenuItemTag(sender);
            Debug.Assert(listView != null, "");

            List<ListViewItem> items = new List<ListViewItem>();
            items.AddRange(listView.SelectedItems.Cast<ListViewItem>());

            this.ShowMessage("正在启动任务");
            var result = await Task.Run<NormalResult>(() =>
            {
                try
                {
                    foreach (ListViewItem item in items)
                    {
                        string server_name = GetItemText(item, OPERLOG_COLUMN_SERVERNAME);
                        var server = this.Servers.GetServerByName(server_name);
                        if (server == null)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"名为 '{server_name}' 的服务器不存在..."
                            };
                        }

                        string strOutputFolder = GetOutputLogFolder(listView, server.Name);

                        var info = GetOperLogInfo(item);

                        // 如果下载正在运行，则不允许重复启动
                        if (info.IsRunning == true)
                        {
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = $"任务 '{server_name}' 已经在运行中，无法重复启动"
                            };
                        }

                        info.CancelTask();
                        info.BeginTask();
                        info.OperLogTask = GetOperLogFiles(
                                item,
                                strOutputFolder,
                                info.CancellationToken);
                        /*
                        if (result.Value == -1)
                        {
                            SetOperLogItemError(item, result.ErrorInfo);
                        }
                        */
                    }

                    return new NormalResult();
                }
                catch (Exception ex)
                {
                    strError = "程序出现异常: " + ExceptionUtil.GetDebugText(ex);
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };
                }
                finally
                {
                    this.ClearMessage();
                }
            });
            if (result.Value == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        string GetOutputLogFolder(ListView listView,
            string serverName)
        {
            string strOutputFolder = "";
            if (listView == this.listView_operLogTasks)
                strOutputFolder = Path.Combine(GetOutputFolder(serverName), "operlog");
            else if (listView == this.listView_errorLogTasks)
                strOutputFolder = Path.Combine(GetOutputFolder(serverName), "log");
            else
                throw new ArgumentException("未知的 listView");

            PathUtil.CreateDirIfNeed(strOutputFolder);
            return strOutputFolder;
        }

        static string OutputFolderRoot
        {
            get
            {
                return ClientInfo.Config.Get("global", "outputFolder");
            }
            set
            {
                ClientInfo.Config.Set("global", "outputFolder", value);
            }
        }

        private void MenuItem_configOutputFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dir_dlg = new FolderBrowserDialog())
            {
                dir_dlg.Description = "请指定下载目标文件夹";
                dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
                dir_dlg.ShowNewFolderButton = true;
                dir_dlg.SelectedPath = OutputFolderRoot;
                dir_dlg.ShowNewFolderButton = true;

                if (dir_dlg.ShowDialog() != DialogResult.OK)
                    return;

                OutputFolderRoot = dir_dlg.SelectedPath;
            }
        }

        private void MenuItem_resetOutputFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
"确实要将输出目录恢复为默认设置值?",
"MainForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            OutputFolderRoot = null;
        }

        private void MenuItem_openOutputFolder_Click(object sender, EventArgs e)
        {
            string root = OutputFolderRoot;
            if (string.IsNullOrEmpty(root))
                root = Path.Combine(ClientInfo.UserDir, "backup");
            try
            {
                System.Diagnostics.Process.Start(root);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        List<string> SelectServerNames()
        {
            using (ServersDlg dlg = new ServersDlg())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "selectServerNames", "state");

                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.Text = "请选择 dp2library 服务器";
                dlg.Mode = "select";
                dlg.Servers = Servers.Dup();
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return null;

                return dlg.SelectedServerNames;
            }
        }

        // 批量修改密码
        private async void MenuItem_changePassword_Click(object sender, EventArgs e)
        {
            var server_names = SelectServerNames();
            if (server_names == null)
                return;

            string new_password = InputDlg.GetInput(this, "修改密码",
                "新密码:",
                null,
                this.Font);
            if (new_password == null)
                return;
            List<string> errors = new List<string>();
            int changed_count = 0;

            this.ShowMessage("正在修改密码");
            try
            {
                await Task.Run(() =>
                {

                    foreach (var server_name in server_names)
                    {
                        var server = this.Servers.GetServerByName(server_name);
                        string old_password = server.DefaultPassword;

                        var result = ChangePassword(
                            server,
                            server.DefaultUserName,
                            server.DefaultPassword,
                            new_password);
                        if (result.Value == -1)
                        {
                            errors.Add(result.ErrorInfo);
                        }
                        else
                        {
                            server.DefaultPassword = new_password;
                            changed_count++;
                        }
                    }
                });
            }
            finally
            {
                this.ClearMessage();
            }

            // 及时保存一次
            if (changed_count > 0)
            {
                this.Servers.Changed = true;
                // this.Servers.Save(null);
            }

            string changed_text = $"修改成功 {changed_count} 个";

            if (errors.Count > 0)
                MessageDlg.Show(this,
                    changed_text + "\r\n\r\n出错：" + StringUtil.MakePathList(errors, "\r\n"),
                    "修改密码过程出错");
            else
                MessageBox.Show(this, changed_text);
        }

        NormalResult ChangePassword(
            dp2Server server,
            string userName,
            string old_password,
            string new_password)
        {

            LibraryChannel channel = this.GetChannel(server.Url, userName);
            try
            {
                // return:
                //      -1  error
                //      0   登录未成功
                //      1   登录成功
                long lRet = channel.Login(userName,
                    old_password,
                    "type=worker,client=dp2managecenter|" + ClientInfo.ClientVersion,
                    out string strError);
                if (lRet == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"针对服务器 {server.Name} 用户 '{userName}' 进行登录失败，无法进行密码修改: {strError}",
                        ErrorCode = "loginFail"
                    };
                }

                if (lRet == 0)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"服务器 {server.Name} 用户 '{userName}' 的旧密码不正确，无法进行密码修改",
                        ErrorCode = "oldPasswordError"
                    };
                }

                // 修改为新密码
                lRet = channel.ChangeUserPassword(
        null,
        userName,
        old_password,
        new_password,
        out strError);
                if (lRet == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"针对服务器 {server.Name} 用户 '{userName}' 进行密码修改时出错: {strError}",
                        ErrorCode = "changePasswordFail"
                    };

                return new NormalResult();
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 批量刷新服务器名
        private async void MenuItem_refreshServerName_Click(object sender, EventArgs e)
        {
            var server_names = SelectServerNames();
            if (server_names == null)
                return;

            List<string> errors = new List<string>();
            int changed_count = 0;

            this.ShowMessage("正在从服务器获取服务器名");
            try
            {
                await Task.Run(() =>
                {
                    foreach (var server_name in server_names)
                    {
                        var server = this.Servers.GetServerByName(server_name);

                        var result = GetLibraryName(
                            server);
                        if (result.Value == -1)
                        {
                            errors.Add(result.ErrorInfo);
                        }
                        else
                        {
                            // TODO: 要对现有的 ServerName 查重。也可以放到最后去做，如果发现重复的，在字符串后面加一个随机的后缀
                            if (string.IsNullOrEmpty(result.LibraryName) == false)
                            {
                                server.Name = result.LibraryName;
                                changed_count++;
                            }
                        }
                    }
                });
            }
            finally
            {
                this.ClearMessage();
            }

            // 及时保存一次
            if (changed_count > 0)
            {
                this.Servers.Changed = true;
            }

            string changed_text = $"修改成功 {changed_count} 个";

            if (errors.Count > 0)
                MessageDlg.Show(this,
                    changed_text + "\r\n\r\n出错：" + StringUtil.MakePathList(errors, "\r\n"),
                    "刷新服务器名过程出错");
            else
                MessageBox.Show(this, changed_text);
        }

        public class GetLibraryNameResult : NormalResult
        {
            public string LibraryName { get; set; }
        }

        GetLibraryNameResult GetLibraryName(dp2Server server)
        {
            LibraryChannel channel = this.GetChannel(server.Url);
            try
            {
                long lRet = channel.GetSystemParameter(null,
    "library",
    "name",
    out string strValue,
    out string strError);
                if (lRet == -1)
                {
                    strError = "针对服务器 " + channel.Url + " 获得图书馆一般信息library/name过程发生错误：" + strError;
                    return new GetLibraryNameResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                    };
                }

                return new GetLibraryNameResult { LibraryName = strValue };
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        private void MenuItem_config_Click(object sender, EventArgs e)
        {
            using (ConfigForm dlg = new ConfigForm())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.ShowDialog(this);
            }
        }

        private void MenuItem_openUserFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(ClientInfo.UserDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(ClientInfo.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void MenuItem_openProgramFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void listView_errorLogTasks_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Tag = this.listView_errorLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新建下载错误日志任务 (&B)");
            menuItem.Tag = this.listView_errorLogTasks;
            menuItem.Click += new System.EventHandler(this.MenuItem_newOperLogTasks_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("停止下载 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&T)");
            menuItem.Tag = this.listView_errorLogTasks;
            menuItem.Click += new System.EventHandler(this.MenuItem_stopOperLogTasks_Click);
            if (this.listView_errorLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("重启下载 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Tag = this.listView_errorLogTasks;
            menuItem.Click += new System.EventHandler(this.MenuItem_continueOperLogTasks_Click);
            if (this.listView_errorLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("打开本地文件夹 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&F)");
            menuItem.Tag = this.listView_errorLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_openOperLogLocalFolder_Click);
            if (this.listView_errorLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("移除 [" + this.listView_operLogTasks.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Tag = this.listView_errorLogTasks;
            menuItem.Click += new System.EventHandler(this.menu_removeOperLogTasks_Click);
            if (this.listView_errorLogTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_errorLogTasks, new Point(e.X, e.Y));
        }
    }
}
