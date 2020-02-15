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

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryServer.Common;
using DigitalPlatform.Text;
using Newtonsoft.Json;

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

            LoadBackupTasks();

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

        // 服务器名和缺省帐户管理
        public void ManageServers(bool bFirstRun)
        {
            ServersDlg dlg = new ServersDlg();
            // GuiUtil.SetControlFont(dlg, this.DefaultFont);

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

            SaveBackupTasks();

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

        List<LibraryChannel> _channelList = new List<LibraryChannel>();
        public void DoStop(object sender, StopEventArgs e)
        {
            // TODO: 加锁
            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
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

            // TODO: 加锁
            _channelList.Add(channel);
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
            _channelList.Remove(channel);
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

            ServerDlg dlg = new ServerDlg();
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

        private async void MenuItem_newBackupTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            GetDp2ResDlg dlg = new GetDp2ResDlg();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.ChannelManager = this;

            dlg.Servers = this.Servers;
            dlg.EnabledIndices = new int[] { dp2ResTree.RESTYPE_SERVER };
            // dlg.Path = this.textBox_dp2library_serverName.Text;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.ShowMessage("正在启动任务");

            try
            {
                // TODO: 空或者星号代表所有服务器
                dp2Server server = this.Servers.GetServerByName(dlg.Path);
                if (server == null)
                {
                    strError = $"名为 '{dlg.Path}' 的服务器不存在...";
                    goto ERROR1;
                }

                var result = await NewBackupTask(server);
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
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

        const int COLUMN_SERVERNAME = 0;
        const int COLUMN_STATE = 1;
        const int COLUMN_STARTTIME = 2;
        const int COLUMN_PROGRESS = 3;
        const int COLUMN_SERVERFILES = 4;

        async Task<NormalResult> NewBackupTask(dp2Server server)
        {
            string strOutputFolder = GetOutputFolder(server.Name);

            ListViewItem item = new ListViewItem();
            this.listView_backupTasks.Items.Add(item);
            ListViewUtil.ChangeItemText(item, COLUMN_SERVERNAME, server.Name);

            var result = await Backup(server, item, strOutputFolder);
            if (result.Value == -1)
            {
                SetItemError(item, result.ErrorInfo);
            }

            return result;
        }

        void SetItemError(ListViewItem item, string error)
        {
            this.Invoke((Action)(() =>
            {
                // TODO: 修改背景颜色为红色
                ListViewUtil.ChangeItemText(item, COLUMN_STATE, $"error:{error}");
            }));
        }

        void SetItemText(ListViewItem item, int column, string text)
        {
            if (this.IsDisposed)
                return;

            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, column, text);
            }));
        }

        async Task<NormalResult> Backup(dp2Server server,
            ListViewItem item,
            string strOutputFolder)
        {
            LibraryChannel channel = this.GetChannel(server.Url);
            try
            {
                BackupTaskStart param = new BackupTaskStart();
                param.BackupFileName = "";
                param.DbNameList = "*";

                BatchTaskStartInfo startinfo = new BatchTaskStartInfo
                {
                    Start = param.ToString(),
                    WaitForBegin = true
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
                    out BatchTaskInfo resultInfo,
                    out string strError);
                if (lRet == -1 || lRet == 1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError
                    };

                // 开始下载文件
                {
                    List<string> paths = StringUtil.SplitList(resultInfo.StartInfo.OutputParam);

                    StringUtil.RemoveBlank(ref paths);

                    /*
                    var infos = MainForm.BuildDownloadInfoList(paths);

                    // 询问是否覆盖已有的目标下载文件。整体询问
                    // return:
                    //      -1  出错
                    //      0   放弃下载
                    //      1   同意启动下载
                    var result = await AskOverwriteFiles(
                        channel,
                        infos,
                        strOutputFolder);
                    if (result.Value == -1)
                        return result;
                    if (result.Value != 1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "放弃处理"
                        };

                    paths = GetFileNames(infos, (i) =>
                    {
                        return i.ServerPath;
                    });
                    */

                    // 设置开始时间
                    //DateTime start_time = DateTime.Now;
                    //SetItemText(item, COLUMN_STARTTIME, start_time.ToString());
                    SetItemText(item, COLUMN_STATE, "正在下载");
                    SetItemText(item, COLUMN_SERVERFILES, StringUtil.MakePathList(paths));

                    var info = GetInfo(item);

                    info.InitialPathList(paths);
                    info.ServerName = server.Name;
                    // info.StartTime = start_time;
                    info.State = "downloading";
                    SetItemColor(item);

#if NO
                    foreach (string path in paths)
                    {
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
                            int nRet = BeginDownloadFile(
                                channel.Url,
                                path,
                                "append",
                                strOutputFolder,
                                (p, t) =>
                                {
                                    // TODO: 一个完成，一个还没有完成，如何表示？
                                    SetItemText(item, COLUMN_PROGRESS, t);
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
                                        }
                                    }
                                    else
                                    {
                                        info.SetPathState(p, $"error:{error}");
                                        info.State = "error";
                                        SetItemText(item, COLUMN_STATE, info.GetStateListString());
                                    }
                                    SetItemColor(item);
                                },
                                out strError);
                            if (nRet == -1)
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError
                                };
                            if (nRet == 0)
                                break;
                        }
                    }

#endif
                }
                // return new NormalResult();
            }
            finally
            {
                this.ReturnChannel(channel);
            }

            return await DownloadBackupFiles(
                item,
                strOutputFolder);
        }

        // parameters:
        //      deleteServerFile    下载成功后是否自动删除服务器端文件?
        async Task<NormalResult> DownloadBackupFiles(
    ListViewItem item,
    string strOutputFolder,
    bool deleteServerFile = true)
        {
            var info = GetInfo(item);
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
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"任务事项 '{info.ServerName}' 中没有任何下载文件事项"
                    };

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
                    strOutputFolder);
                if (result.Value == -1)
                    return result;
                if (result.Value != 1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "放弃处理"
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
                SetItemColor(item);

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
                    if (deleteServerFile)
                    {
                        var delete_result = DeleteServerFiles(item);
                        if (delete_result.Value == -1)
                        {
                            SetItemText(item, COLUMN_STATE, $"删除服务器端大备份文件时出错: {delete_result.ErrorInfo}");
                            // TODO: 是否返回出错？
                        }
                    }

                    info.State = "finish";
                    SetItemColor(item);
                }
                else
                {
                    foreach (string path in paths)
                    {
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
                            int nRet = BeginDownloadFile(
                                channel.Url,
                                path,
                                "append",
                                strOutputFolder,
                                    (p, t) =>
                                    {
                                        int index = info.IndexOfPath(p);
                                        SetItemText(item, COLUMN_PROGRESS, $"({(index + 1)}){t}");
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
                                                if (deleteServerFile)
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
                                        SetItemColor(item);
                                    },
                                out string strError);
                            if (nRet == -1)
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = strError
                                };
                            if (nRet == 0)
                                break;
                        }
                    }
                }
            }
            finally
            {
                this.ReturnChannel(channel);
            }

            // 自动删除服务器端大备份文件
            if (deleteServerFile)
            {
                var delete_result = DeleteServerFiles(item);
                if (delete_result.Value == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"下载文件成功，但删除服务器端大备份文件时出错: {delete_result.ErrorInfo}"
                    };
                }
            }

            return new NormalResult();
        }

        NormalResult DeleteServerFiles(ListViewItem item)
        {
            var info = GetInfo(item);

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

        public class BackupItemInfo
        {
            // 服务器名
            public string ServerName { get; set; }

            // 要下载的文件名集合。服务器端路径形态
            public List<PathItem> PathList { get; set; }

            // 首次启动任务的时间
            public DateTime StartTime { get; set; }

            public string State { get; set; }   // (null)/dowloading/finish/error

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

        static BackupItemInfo GetInfo(ListViewItem item)
        {
            BackupItemInfo info = item.Tag as BackupItemInfo;
            if (info == null)
            {
                info = new BackupItemInfo();
                item.Tag = info;
            }
            return info;
        }

        #region 下载文件

        public class AskResult : NormalResult
        {
            // [out]
            public bool Append { get; set; }

            // [out] 询问后，操作者决定从处理文件列表中删除(也就是不下载这些文件)的事项
            public List<DownloadFileInfo> DeletedFiles { get; set; }
        }

        // TODO: 将中途打算删除的文件留到函数返回前一刹那再删除
        // 询问是否覆盖已有的目标下载文件。整体询问
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   同意启动下载
        public async Task<AskResult> AskOverwriteFiles(// List<string> filenames,
            LibraryChannel channel,
            List<DownloadFileInfo> fileinfos,
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


            List<DownloadFileInfo> deleted = new List<DownloadFileInfo>();

            DialogResult md5_result = System.Windows.Forms.DialogResult.Yes;
            bool bDontAskMd5Verify = false; // 是否要询问 MD5 校验

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
                                return _checkMD5(channel, filename, strTargetPath);
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
                    DialogResult result = MessageBox.Show(this,
        "下列文件中 '" + GetFileNameList(md5_mismatch_filenames, "\r\n") + "' 先前曾经被下载过，但 MD5 验证发现和服务器侧文件不一致。\r\n\r\n是否删除它们然后重新下载?\r\n[是：重新下载; 否: 不下载这些文件; 取消：放弃全部下载]",
        "MainForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
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
                    DialogResult result = MessageBox.Show(this,
        "下列文件 '" + GetFileNameList(temp_filenames, "\r\n") + "' 先前曾经被下载过，但未能完成。\r\n\r\n是否继续下载未完成部分?\r\n[是：从断点继续下载; 否: 重新从头下载; 取消：放弃全部下载]",
        "MainForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
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
                    DialogResult result = MessageBox.Show(this,
        "下列文件中 '" + GetFileNameList(md5_matched_filenames, "\r\n") + "' 先前曾经被下载过，并且 MD5 验证发现和服务器侧文件完全一致。\r\n\r\n是否删除它们然后重新下载?\r\n[是：重新下载; 否: 不下载这些文件; 取消：放弃全部下载]",
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
            string strLocalFilePath)
        {
            string strError = "";

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
                int nRet = DynamicDownloader.GetServerFileMD5(
                    channel,
                    null,   // this.Stop,
                    strServerFilePath,
                    out byte[] server_md5,
                    out strError);
                // TODO: 遇到出错要可以 UI 交互重试
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

        // parameters:
        //      strPath 服务器端的文件路径
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        public int BeginDownloadFile(
            string strServerUrl,
            string strPath,
            string strAppendStyle,
            string strOutputFolder,
            Delegate_showProgress func_showProgress,
            Delegate_finish func_finish,
            out string strError)
        {
            strError = "";

            string strExt = Path.GetExtension(strPath);
            if (strExt == ".~state")
            {
                strError = "状态文件是一种临时文件，不支持直接下载";
                return -1;
            }

            /*
            if (string.IsNullOrEmpty(strOutputFolder))
            {
                FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

                dir_dlg.Description = "请指定下载目标文件夹";
                dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
                dir_dlg.ShowNewFolderButton = true;
                dir_dlg.SelectedPath = _usedDownloadFolder;

                if (dir_dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                _usedDownloadFolder = dir_dlg.SelectedPath;

                strOutputFolder = dir_dlg.SelectedPath;
            }
            */

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
                    return 1;
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
                        return 0;
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
                        return 0;
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
                strError = "未知的 strAppendStyle 值 '" + strAppendStyle + "'";
                return -1;
            }

            LibraryChannel channel = this.GetChannel(strServerUrl);

            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            DynamicDownloader downloader = new DynamicDownloader(channel,
                strPath,
                strTargetPath);
            // downloader.Tag = dlg;

            _downloaders.Add(downloader);

            downloader.Closed += new EventHandler(delegate (object o1, EventArgs e1)
            {
                if (channel != null)
                {
                    channel.Timeout = old_timeout;
                    this.ReturnChannel(channel);
                    channel = null;
                }

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
                RemoveDownloader(downloader);
            });
            string prev_text = "";
            DateTime prev_time = DateTime.MinValue;
            downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
            {
                string text = GetProgressText(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
                if (text != prev_text && DateTime.Now - prev_time > TimeSpan.FromSeconds(1))
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

                        string server_name = this.Servers.GetServer(strServerUrl)?.Name;
                        if (string.IsNullOrEmpty(server_name))
                            server_name = strServerUrl;

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
            downloader.StartDownload(bAppend);
            return 1;
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


        List<DynamicDownloader> _downloaders = new List<DynamicDownloader>();
        private static readonly Object _syncRoot_downloaders = new Object();

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
                infos.Add(GetInfo(item));
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

            string strOutputFolder = Path.Combine(ClientInfo.UserDir, $"backup\\{server_name}");
            PathUtil.CreateDirIfNeed(strOutputFolder);
            return strOutputFolder;
        }

        private async void MenuItem_continueBackupTasks_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.ShowMessage("正在启动任务");
            try
            {
                foreach (ListViewItem item in this.listView_backupTasks.Items)
                {
                    string server_name = ListViewUtil.GetItemText(item, COLUMN_SERVERNAME);
                    string strOutputFolder = GetOutputFolder(server_name);

                    var result = await DownloadBackupFiles(item, strOutputFolder);
                    if (result.Value == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }
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
                        string server_url = this.Servers.GetServerByName(GetInfo(_activeItem)?.ServerName)?.Url;
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

            menuItem = new MenuItem("重启下载 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.MenuItem_continueBackupTasks_Click);
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


            menuItem = new MenuItem("移除 [" + this.listView_backupTasks.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelected_Click);
            if (this.listView_backupTasks.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_backupTasks, new Point(e.X, e.Y));
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListView list = (ListView)((MenuItem)sender).Tag;

            ListViewUtil.SelectAllLines(list);
        }

        void menu_removeSelected_Click(object sender, EventArgs e)
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

            ListViewUtil.DeleteSelectedItems(this.listView_backupTasks);
        }

        async void menu_deleteServerFile_Click(object sender, EventArgs e)
        {
            if (this.listView_backupTasks.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除服务器端备份文件的事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
        "确实要删除选定的 " + this.listView_backupTasks.SelectedItems.Count.ToString() + " 个事项对应的服务器端备份文件?",
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
        void SetItemColor(ListViewItem item)
        {
            this.Invoke((Action)(() =>
            {
                var info = GetInfo(item);
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
    }
}
