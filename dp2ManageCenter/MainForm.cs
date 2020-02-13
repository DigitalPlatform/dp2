using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

            ClearHtml();

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
            // SaveTaskDom();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Cancel();
            _cancel?.Dispose();

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
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
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

        private async void MenuItem_startBackupTask_Click(object sender, EventArgs e)
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

                var result = await StartBackupTask(server);
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

        async Task<NormalResult> StartBackupTask(dp2Server server)
        {
            string strOutputFolder = Path.Combine(ClientInfo.UserDir, $"backup\\{server.Name}");
            PathUtil.CreateDirIfNeed(strOutputFolder);

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

                    paths = GetFileNames(infos, (info) =>
                    {
                        return info.ServerPath;
                    });
                    foreach (string path in paths)
                    {
                        if (string.IsNullOrEmpty(path) == false)
                        {
                            // 设置开始时间
                            SetItemText(item, COLUMN_STARTTIME, DateTime.Now.ToString());

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
                                (text) =>
                                {
                                    SetItemText(item, COLUMN_PROGRESS, text);
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
                }
                return new NormalResult();
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        #region 下载文件

        public class AskResult : NormalResult
        {
            public bool Append { get; set; }
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
                                return new AskResult { Value = 0 };
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
                                    ErrorInfo = result.ErrorInfo
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
                        Append = false
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
                            Append = bAppend
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
                        DeleteItems(fileinfos, (info) =>
                        {
                            return info.MD5Matched == "no";
                        });
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
                            Append = bAppend
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
                            Append = bAppend
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
                        DeleteItems(fileinfos, (info) =>
                        {
                            return info.MD5Matched == "yes";
                        });
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
                            Append = bAppend
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
                        DeleteItems(fileinfos, (info) =>
                        {
                            return info.LocalFileExists;
                        });
                    }
                }
                return new AskResult
                {
                    Value = 1,
                    Append = bAppend
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

        // func 返回 true 表示要删除
        void DeleteItems(List<DownloadFileInfo> fileinfos, Delegate_processItem2 func)
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


        public delegate void Delegate_showProgress(string text);

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
                /*
                DisplayDownloaderErrorInfo(downloader);
                */
                RemoveDownloader(downloader);
            });
            string prev_text = "";
            downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
            {
                string text = GetProgressText(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
                if (text != prev_text)
                {
                    func_showProgress?.Invoke(text);
                    prev_text = text;
                }
                //if (dlg.IsDisposed == false)
                //    dlg.SetProgress(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
            });
            downloader.Prompt += new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
            {
                e1.ResultAction = "yes";
#if NO
                if (dlg.IsDisposed == true)
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
#endif
            });
            downloader.StartDownload(bAppend);
            return 1;
        }

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
    }
}
