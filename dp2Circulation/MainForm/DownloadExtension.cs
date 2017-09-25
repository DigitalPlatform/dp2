using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.CirculationClient;
using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using System.Threading.Tasks;
using System.Threading;

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

        // 询问是否覆盖已有的目标下载文件。整体询问
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   同意启动下载
        public int AskOverwriteFiles(List<string> filenames,
            ref string strOutputFolder,
            out bool bAppend,
            out string strError)
        {
            strError = "";
            bAppend = false;
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

            List<string> states = new List<string>();
            List<string> all_target_filenames = new List<string>();
            List<string> temp_filenames = new List<string>();   // 对应临时文件存在的，正式目标文件名 数组

            // 检查目标文件的存在情况
            foreach (string filename in filenames)
            {
                string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(filename));
                all_target_filenames.Add(strTargetPath);

                string strTargetTempPath = DynamicDownloader.GetTempFileName(strTargetPath);

                // 观察目标文件是否已经存在
                if (File.Exists(strTargetPath))
                {
                    states.Add("exists");
                    File.Delete(strTargetTempPath); // 防范性地删除临时文件
                    continue;
                }


                // 观察临时文件是否已经存在
                if (File.Exists(strTargetTempPath))
                {
                    states.Add("temp_exists");
                    temp_filenames.Add(strTargetPath);
                }
            }

            // 没有任何目标文件和临时文件存在
            if (states.Count == 0)
            {
                bAppend = false;
                return 1;
            }

            // 观察是否有 .tmp 文件存在
            if (states.IndexOf("temp_exists") != -1)
            {
                DialogResult result = MessageBox.Show(this,
all_target_filenames.Count.ToString() + " 个目标文件中 '" + StringUtil.MakePathList(temp_filenames) + "' 先前曾经被下载过，但未能完成。\r\n\r\n是否继续下载未完成部分?\r\n[是：从断点继续下载; 否: 重新从头下载; 取消：放弃全部下载]",
"MainForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return 0;
                if (result == DialogResult.Yes)
                {
                    bAppend = true;
                    // 注意后续处理时候，将已经存在正式目标文件的事项从 filenames 中移走，不要下载这些文件
                }
                else
                    bAppend = false;    // 这里并没有删除临时文件。等后面真正下载的时候再删除

                return 1;
            }

            // 观察目标文件是否全部存在
            if (IsAllExists(states))
            {
                DialogResult result = MessageBox.Show(this,
all_target_filenames.Count.ToString() + " 个目标文件 '" + StringUtil.MakePathList(all_target_filenames) + "' 已经全部存在。\r\n\r\n是否重新下载并覆盖它?\r\n[是：下载并覆盖; 取消：放弃全部下载]",
"MainForm",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return 0;
                bAppend = false;
                return 1;
            }

            // 理论上似乎走不到这里
            return 1;
        }

        static bool IsAllExists(List<string> states)
        {
            foreach (string state in states)
            {
                if (state != "exists")
                    return false;
            }

            return true;
        }

        string _usedDownloadFolder = "";

        // parameters:
        //      strAppendStyle  append/overwrite/ask 之一
        //      strOutputFolder 输出目录。
        //                      [in] 如果为 null，表示要弹出对话框询问目录。如果不为 null，则直接使用这个目录路径
        //                      [out] 实际使用的目录
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        public int BeginDownloadFile(string strPath,
            string strAppendStyle,
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
                if (dlg.IsDisposed == false)
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

        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        public int BeginDownloadFiles(List<string> paths,
            string strAppendStyle,
            ref string strOutputFolder,
            out string strError)
        {
            strError = "";

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

            List<DynamicDownloader> current_downloaders = new List<DynamicDownloader>();

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            channel = this.GetChannel();

            old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            FileDownloadDialog dlg = new FileDownloadDialog();
            dlg.FormClosed += new FormClosedEventHandler(delegate(object o1, FormClosedEventArgs e1)
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

            bool bDone = false;
            try
            {
                bool bAppend = false;   // 是否继续下载?

                foreach (string strPath in paths)
                {
                    string strExt = Path.GetExtension(strPath);
                    if (strExt == ".~state")
                    {
                        strError = "状态文件是一种临时文件，不支持直接下载";
                        return -1;
                    }

                    string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(strPath));

                    string strTargetTempPath = DynamicDownloader.GetTempFileName(strTargetPath);

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

                    DynamicDownloader downloader = new DynamicDownloader(channel,
                        strPath,
                        strTargetPath);
                    downloader.Tag = dlg;

                    _downloaders.Add(downloader);

                    downloader.Closed += new EventHandler(delegate(object o1, EventArgs e1)
                    {
                        DisplayDownloaderErrorInfo(downloader);
                        RemoveDownloader(downloader);
                    });
                    downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate(object o1, DownloadProgressChangedEventArgs e1)
                    {
                        if (dlg.IsDisposed == false)
                            dlg.SetProgress(e1.BytesReceived, e1.TotalBytesToReceive);
                    });

                    current_downloaders.Add(downloader);
                }

                Task.Factory.StartNew(() => DownloadFiles(current_downloaders,
                    bAppend,
                    () =>
                    {
                        if (channel != null)
                        {
                            channel.Timeout = old_timeout;
                            this.ReturnChannel(channel);
                            channel = null;
                        }
                        this.Invoke((Action)(() =>
                        {
                            dlg.Close();
                        }));
                        foreach (DynamicDownloader current in current_downloaders)
                        {
                            current.Close();
                        }
                    }),
    CancellationToken.None,
    TaskCreationOptions.LongRunning,
    TaskScheduler.Default);

                bDone = true;
                return 1;
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
                    }
                    this.Invoke((Action)(() =>
                    {
                        dlg.Close();
                    }));
                    foreach (DynamicDownloader current in current_downloaders)
                    {
                        current.Close();
                    }
                }
            }
        }

        delegate void Delegate_end();

        // 顺序执行每个 DynamicDownloader
        void DownloadFiles(List<DynamicDownloader> downloaders,
            bool bAppend,
            Delegate_end func_end)
        {
            foreach (DynamicDownloader downloader in downloaders)
            {
                this.Invoke((Action)(() =>
                {
                    FileDownloadDialog dlg = downloader.Tag as FileDownloadDialog;
                    dlg.SourceFilePath = downloader.ServerFilePath;
                    dlg.TargetFilePath = downloader.LocalFilePath;
                    dlg.Text = "正在下载 " + dlg.SourceFilePath;
                }));
                Task task = downloader.StartDownload(bAppend);
                task.Wait();    // TODO: 这里要允许中断
                if (downloader.IsCancellationRequested)
                    break;
                if (downloader.State == "error")
                    break;
            }

            func_end();
        }

#if NO
        // 顺序执行每个 DynamicDownloader
        void DownloadFiles(List<DynamicDownloader> downloaders,
            bool bAppend,
            CancellationToken token)
        {
            Task task = null;
            foreach (DynamicDownloader downloader in downloaders)
            {
                if (task == null)
                    task = downloader.StartDownload(bAppend);
                else
                    task = task.ContinueWith((prev) =>
                    {
                        if (prev.Exception == null)
                            downloader.StartDownload(bAppend);
                    },
                    token);
            }
        }
#endif

        #endregion
    }
}
