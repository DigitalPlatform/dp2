using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace dp2ManageCenter
{
    // 显示和管理 dp2library 服务器端文件系统的窗口
    public partial class ServerFileSystemForm : Form
    {
        string _serverName = null;
        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                _serverName = value;
                RefreshTitle();
            }
        }

        public string ServerUrl { get; set; }

        public string SelectedPath { get; set; }

        public ServerFileSystemForm()
        {
            InitializeComponent();
        }

        private void ServerFileSystemForm_Load(object sender, EventArgs e)
        {
            RefreshTitle();
            this.kernelResTree1.Fill();

            if (string.IsNullOrEmpty(this.SelectedPath) == false)
            {
                this.BeginInvoke((Action)(() =>
                {
                    this.kernelResTree1.ExpandPath(this.SelectedPath, out string strError);
                }));
            }
        }

        private void ServerFileSystemForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ServerFileSystemForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void kernelResTree1_GetChannel(object sender, DigitalPlatform.LibraryClient.GetChannelEventArgs e)
        {
            e.Channel = Program.MainForm.GetChannel(this.ServerUrl);
        }

        private void kernelResTree1_ReturnChannel(object sender, DigitalPlatform.LibraryClient.ReturnChannelEventArgs e)
        {
            ((FormClientInfo.MainForm) as MainForm).ReturnChannel(e.Channel);
        }

        void RefreshTitle()
        {
            if (this.IsHandleCreated && this.IsDisposed == false)
                this.Invoke((Action)(() =>
                {
                    this.Text = $"服务器 '{_serverName}' 的文件夹";
                }));
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            this.kernelResTree1.Refresh(this.kernelResTree1.SelectedNode);
        }

        private void kernelResTree1_DownloadFiles(object sender, DownloadFilesEventArgs e)
        {
            string strError = "";
            string strOutputFolder = "";

            if (e.Action == "getmd5")
            {
                Task.Run(() =>
                {
                    GetMd5(e);
                });
                return;
            }


#if NO
            List<dp2Circulation.MainForm.DownloadFileInfo> infos = MainForm.BuildDownloadInfoList(e.FileNames);

            // 询问是否覆盖已有的目标下载文件。整体询问
            // return:
            //      -1  出错
            //      0   放弃下载
            //      1   同意启动下载
            int nRet = Program.MainForm.AskOverwriteFiles(infos,    // e.FileNames,
ref strOutputFolder,
out bool bAppend,
out strError);
            if (nRet == -1)
            {
                e.ErrorInfo = strError;
                return;
            }

            // return:
            //      -1  出错
            //      0   放弃下载
            //      1   成功启动了下载
            nRet = Program.MainForm.BeginDownloadFiles(infos,   // e.FileNames,
                bAppend ? "append" : "overwrite",
                null,
                ref strOutputFolder,
                out strError);
            if (nRet == -1)
                e.ErrorInfo = strError;
#endif
        }

        private void kernelResTree1_UploadFiles(object sender, UploadFilesEventArgs e)
        {
            // Program.MainForm.BeginUploadFiles(e);
        }

        void ShowMessage(string text)
        {
            this.BeginInvoke((Action)(() =>
            {
                this.toolStripStatusLabel_message.Text = text;
            }));
        }

        void ClearMessage()
        {
            this.BeginInvoke((Action)(() =>
            {
                this.toolStripStatusLabel_message.Text = "";
            }));
        }

        void ShowMessageBox(string text)
        {
            this.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(this, text);
            }));
        }

        List<string> _task_ids = new List<string>();

        void GetMd5(DownloadFilesEventArgs e)
        {
            string strError = "";

            List<string> lines = new List<string>();

            CancellationTokenSource cancel = new CancellationTokenSource();
            // 出现一个对话框，允许中断获取 MD5 的过程
            FileDownloadDialog dlg = null;
            this.Invoke((Action)(() =>
            {
                dlg = new FileDownloadDialog();
                dlg.Font = this.Font;
                dlg.Text = $"正在获取 MD5";
                // dlg.SourceFilePath = strTargetPath;
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

            var channel = Program.MainForm.GetChannel(this.ServerUrl);

            // var old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 25, 0);
            // this.ShowMessage("正在获取 MD5 ...");
            try
            {
                foreach (string filepath in e.FileNames)
                {
                    // this.ShowMessage($"正在获取服务器文件 {filepath} 的 MD5 ...");

                    dlg.SetProgress($"正在获取服务器文件 {filepath} 的 MD5 ...", 0, 0);

                    // 检查 MD5
                    // return:
                    //      -1  出错
                    //      0   文件没有找到
                    //      1   文件找到
                    int nRet = DynamicDownloader.GetServerFileMD5ByTask(
                        channel,
                        null,   // this.Stop,
                        filepath,
                        (MessagePromptEventHandler)null,
                        cancel.Token,
                        _task_ids,
                        out byte[] server_md5,
                        out strError);
                    if (nRet != 1)
                    {
                        strError = "探测服务器端文件 '" + filepath + "' MD5 时出错: " + strError;
                        goto ERROR1;
                    }

                    lines.Add($"文件 {filepath} 的 MD5 为 {Convert.ToBase64String(server_md5)}");
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            finally
            {
                //channel.Timeout = old_timeout;
                Program.MainForm.ReturnChannel(channel);
                // this.ClearMessage();

                this.Invoke((Action)(() =>
                {
                    dlg.Close();
                }));
                cancel.Dispose();
            }

            this.Invoke((Action)(() =>
            {
                MessageDialog.Show(this, StringUtil.MakePathList(lines, "\r\n"));
            }));
            return;
        ERROR1:
            ShowMessageBox(strError);
        }

    }
}
