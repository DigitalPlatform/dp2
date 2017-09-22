using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.CirculationClient;
using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关动态文件下载的功能
    /// </summary>
    public partial class MainForm
    {
        #region 下载文件

        private static readonly Object _syncRoot_downloaders = new Object();

        List<DynamicDownloader> _downloaders = new List<DynamicDownloader>();

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

        string _usedDownloadFolder = "";

        // parameters:
        //      strOutputFolder 输出目录。
        //                      [in] 如果为 null，表示要弹出对话框询问目录。如果不为 null，则直接使用这个目录路径
        //                      [out] 实际使用的目录
        public int BeginDownloadFile(string strPath,
            ref string strOutputFolder,
            out string strError)
        {
            strError = "";

            string strExt = Path.GetExtension(strPath);
            if (strExt == ".~state")
            {
                strError = "状态文件是一种临时文件，不支持直接下载";
                return -1;
            }

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

            string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(strPath));

            string strTargetTempPath = DynamicDownloader.GetTempFileName(strTargetPath);

            bool bAppend = false;   // 是否继续下载?
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
                    File.Delete(strTargetTempPath);
            }

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            channel = this.GetChannel();

            old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            FileDownloadDialog dlg = new FileDownloadDialog();
            dlg.Font = this.Font;
            dlg.Text = "正在下载 " + strPath;
            dlg.SourceFilePath = strPath;
            dlg.TargetFilePath = strTargetPath;
            dlg.Show(this);

            DynamicDownloader downloader = new DynamicDownloader(channel,
                strPath,
                strTargetPath);
            downloader.Tag = dlg;

            _downloaders.Add(downloader);

            downloader.Closed += new EventHandler(delegate(object o1, EventArgs e1)
            {
                if (channel != null)
                {
                    channel.Timeout = old_timeout;
                    this.ReturnChannel(channel);
                    channel = null;
                }
                DisplayDownloaderErrorInfo(downloader);
                RemoveDownloader(downloader);
                this.Invoke((Action)(() =>
                {
                    dlg.Close();
                }));
            });
            downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate(object o1, DownloadProgressChangedEventArgs e1)
            {
                dlg.SetProgress(e1.BytesReceived, e1.TotalBytesToReceive);
            });
            dlg.FormClosed += new FormClosedEventHandler(delegate(object o1, FormClosedEventArgs e1)
            {
                downloader.Cancel();

                if (channel != null)
                {
                    channel.Timeout = old_timeout;
                    this.ReturnChannel(channel);
                    channel = null;
                }
                DisplayDownloaderErrorInfo(downloader);
                RemoveDownloader(downloader);
            });

            downloader.StartDownload(bAppend);
            return 1;
        }

        #endregion
    }
}
