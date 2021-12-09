using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Core;

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

        Task<NormalResult> BeginCheckMD5(string strServerFilePath,
            string strLocalFilePath)
        {
            return Task.Factory.StartNew<NormalResult>(
    () =>
    {
        return _checkMD5(strServerFilePath, strLocalFilePath);
    });
        }

        List<string> _task_ids = new List<string>();

        // result.Value:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        NormalResult _checkMD5(string strServerFilePath,
            string strLocalFilePath)
        {
            string strError = "";

            LibraryChannel channel = this.GetChannel();

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在验证服务器端文件 " + strServerFilePath + " 的 MD5 校验码 ...");
            Stop.BeginLoop();

            stopManager.Active(Stop);   // testing

            // Application.DoEvents();

            try
            {
                // 2020/2/26 改用 ...ByTask()
                // 检查 MD5
                // return:
                //      -1  出错
                //      0   文件没有找到
                //      1   文件找到
                int nRet = DynamicDownloader.GetServerFileMD5ByTask(
                    channel,
                    Stop,   // this.Stop,
                    strServerFilePath,
                    new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
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
                    }),
                    new CancellationToken(),
                    _task_ids,
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

#if NO
                byte[] local_md5 = BeginGetLocalMD5(strLocalFilePath);
                if (ByteArray.Compare(server_md5, local_md5) != 0)
                {
                    strError = "服务器端文件 '" + strServerFilePath + "' 和本地文件 '" + strLocalFilePath + "' MD5 不匹配";
                    return 0;
                }
#endif

                return new NormalResult(1, null);
            }
            finally
            {

                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
            }
        }

        static byte[] BeginGetLocalMD5(string strLocalFilePath)
        {
            using (FileStream stream = File.OpenRead(strLocalFilePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return DynamicDownloader.GetFileMd5(stream);
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
#if NO
            public void DeleteTempFile()
            {
                if (string.IsNullOrEmpty(this.LocalPath))
                    return;
                string strTargetTempPath = DynamicDownloader.GetTempFileName(this.LocalPath);
                if (File.Exists(strTargetTempPath))
                    File.Delete(strTargetTempPath);
            }

            public void DeleteLocalFile()
            {
                if (string.IsNullOrEmpty(this.LocalPath))
                    return;

                if (File.Exists(this.LocalPath))
                    File.Delete(this.LocalPath);
            }
#endif
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

        internal delegate string Delegate_processItem1(DownloadFileInfo info);

        // 
        internal delegate bool Delegate_processItem2(DownloadFileInfo info);

        internal delegate void Delegate_processItem3(DownloadFileInfo info);


        internal static List<string> GetFileNames(List<DownloadFileInfo> infos, Delegate_processItem1 func)
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

        static void RemoveByLocalPath(List<DownloadFileInfo> infos, string strLocalPath)
        {
            List<DownloadFileInfo> delete_infos = new List<DownloadFileInfo>();
            foreach (DownloadFileInfo info in infos)
            {
                if (info.LocalPath == strLocalPath)
                    delete_infos.Add(info);
            }

            foreach (DownloadFileInfo info in delete_infos)
            {
                infos.Remove(info);
            }
        }

#if NO
        // 统计本地文件文件存在的列表
        static List<string> GetLocalExists(List<DownloadFileInfo> infos)
        {
            List<string> results = new List<string>();
            foreach (DownloadFileInfo info in infos)
            {
                if (info.LocalFileExists)
                    results.Add(info.LocalPath);
            }

            return results;
        }
#endif
#if NO
        // 统计本地临时文件存在的列表。注意返回的不是临时文件名的列表，而是临时文件对应的本地文件名的列表
        static List<string> GetTempExists(List<DownloadFileInfo> infos)
        {
            List<string> results = new List<string>();
            foreach (DownloadFileInfo info in infos)
            {
                if (info.TempFileExists)
                    results.Add(info.LocalPath);
            }

            return results;
        }
#endif

#if NO
        // 统计 MD5 不匹配的本地文件的列表
        static List<string> GetMd5MismatchList(List<DownloadFileInfo> infos)
        {
            List<string> results = new List<string>();
            foreach (DownloadFileInfo info in infos)
            {
                if (info.MD5Matched == "no")
                    results.Add(info.LocalPath);
            }

            return results;
        }

        // 统计 MD5 匹配的本地文件的列表
        static List<string> GetMd5MatchList(List<DownloadFileInfo> infos)
        {
            List<string> results = new List<string>();
            foreach (DownloadFileInfo info in infos)
            {
                if (info.MD5Matched == "yes")
                    results.Add(info.LocalPath);
            }

            return results;
        }
#endif

#if NO
        // 删除 MD5 不匹配的本地文件，并修改相应状态
        static void DeleteMd5MismatchLocalFiles(List<DownloadFileInfo> infos)
        {
            foreach (DownloadFileInfo info in infos)
            {
                if (info.MD5Matched == "no")
                {
                    File.Delete(info.LocalPath);
                    info.MD5Matched = "";
                    info.LocalFileExists = false;
                    info.DeleteTempFile();
                    info.TempFileExists = false;
                }
            }
        }
#endif

#if NO
        // 清除 MD5 不匹配的本地文件相关事项。这样这些文件就不会被后续下载处理了
        static void RemoveMd5MismatchItems(List<DownloadFileInfo> infos)
        {
            List<DownloadFileInfo> delete_infos = new List<DownloadFileInfo>();
            foreach (DownloadFileInfo info in infos)
            {
                if (info.MD5Matched == "no")
                    delete_infos.Add(info);
            }

            foreach (DownloadFileInfo info in delete_infos)
            {
                infos.Remove(info);
            }
        }
#endif

        // 一边等待，一边允许界面活动
        static void GuiWait(Task task, int timeout = 100)
        {
            while (task.Wait(timeout) == false)
            {
                Application.DoEvents();
            }
        }

        // TODO: 将中途打算删除的文件留到函数返回前一刹那再删除
        // 询问是否覆盖已有的目标下载文件。整体询问
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   同意启动下载
        public int AskOverwriteFiles(// List<string> filenames,
            List<DownloadFileInfo> fileinfos,
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

            DialogResult md5_result = System.Windows.Forms.DialogResult.Yes;
            bool bDontAskMd5Verify = false; // 是否要询问 MD5 校验

            //List<string> states = new List<string>();
            //List<string> all_target_filenames = new List<string>();
            //List<string> temp_filenames = new List<string>();   // 对应临时文件存在的，正式目标文件名 数组

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
                                return 0;
                        }

                        if (md5_result == System.Windows.Forms.DialogResult.Yes)
                        {
                            Task<NormalResult> task = BeginCheckMD5(filename, strTargetPath);
                            GuiWait(task);
                            if (task.Result.Value == -1)
                            {
                                strError = task.Result.ErrorInfo;
                                return -1;
                            }
                            if (task.Result.Value == 0)
                            {
                                info.MD5Matched = "no";
                                continue;
                            }
                            else if (task.Result.Value == 1)
                                info.MD5Matched = "yes";
#if NO
                            // 检查 MD5 是否匹配
                            // return:
                            //      -1  出错
                            //      0   不匹配
                            //      1   匹配
                            int nRet = CheckMD5(filename,
                    strTargetPath,
                    out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 0)
                            {
                                info.MD5Matched = "no";
                                //md5_mismatch_filenames.Add(strTargetPath);
#if NO
                                // TODO: 最后一刻才删除
                                File.Delete(strTargetPath); // MD5 不匹配，重新下载
                                if (File.Exists(strTargetTempPath))
                                    File.Delete(strTargetTempPath); // 防范性地删除临时文件
#endif
                                continue;
                            }
                            else if (nRet == 1)
                                info.MD5Matched = "yes";
#endif
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
                    bAppend = false;
                    return 1;
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
                        return 0;
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
                        return 0;
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
                        return 0;
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
                        return 0;
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

                return 1;
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
#if NO
            // 观察目标文件是否全部存在
            if (IsAllExists(states))
            {
                DialogResult result = MessageBox.Show(this,
all_target_filenames.Count.ToString() + " 个目标文件 '" + GetFileNameList(all_target_filenames, "\r\n") + "' 已经全部存在。\r\n\r\n是否重新下载并覆盖它们?\r\n[是：下载并覆盖; 取消：放弃全部下载]",
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
#endif

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

            downloader.Closed += new EventHandler(delegate (object o1, EventArgs e1)
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
            downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
            {
                if (dlg.IsDisposed == false)
                    dlg.SetProgress(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
            });
            // 2017/10/7
            downloader.Prompt += new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
            {
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
            });
            dlg.FormClosed += new FormClosedEventHandler(delegate (object o1, FormClosedEventArgs e1)
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

            try
            {
                _ = downloader.StartDownload(bAppend);
            }
            catch(Exception ex)
            {
                strError = $"开始下载时出现异常: {ex.Message}";
                WriteErrorLog($"开始下载时出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return -1;
            }
            return 1;
        }

        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        public int BeginDownloadFiles(// List<string> paths,
            List<DownloadFileInfo> fileinfos,
            string strAppendStyleParam,
            Delegate_end func_end,
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

            bool bDone = false;
            try
            {
                bool bAppend = false;   // 是否继续下载?

                foreach (DownloadFileInfo info in fileinfos)
                {
                    string strPath = info.ServerPath;

                    string strExt = Path.GetExtension(strPath);
                    if (strExt == ".~state")
                    {
                        strError = "状态文件是一种临时文件，不支持直接下载";
                        return -1;
                    }

                    string strTargetPath = Path.Combine(strOutputFolder, Path.GetFileName(strPath));

                    string strTargetTempPath = DynamicDownloader.GetTempFileName(strTargetPath);

                    string strAppendStyle = info.OverwriteStyle;
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

                    downloader.Closed += new EventHandler(delegate (object o1, EventArgs e1)
                    {
                        DisplayDownloaderErrorInfo(downloader);
                        RemoveDownloader(downloader);
                    });
                    downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
                    {
                        if (dlg.IsDisposed == false)
                            dlg.SetProgress(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
                    });
                    // 2017/10/7
                    downloader.Prompt += new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
                    {
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
                    });

                    current_downloaders.Add(downloader);
                }

                _ = Task.Factory.StartNew(async () => await SequenceDownloadFilesAsync(current_downloaders,
                    bAppend,
                    (bError) =>
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

                        if (func_end != null)
                            func_end(bError);
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

        public delegate void Delegate_end(bool bError);

        // 顺序执行每个 DynamicDownloader
        private async Task SequenceDownloadFilesAsync(List<DynamicDownloader> downloaders,
            bool bAppend,
            Delegate_end func_end)
        {
            int i = 0;
            bool bError = false;
            foreach (DynamicDownloader downloader in downloaders)
            {
                string strNo = "";
                if (downloaders.Count > 0)
                    strNo = " " + (i + 1).ToString() + "/" + downloaders.Count + " ";

                this.Invoke((Action)(() =>
                {
                    FileDownloadDialog dlg = downloader.Tag as FileDownloadDialog;
                    dlg.SourceFilePath = downloader.ServerFilePath;
                    dlg.TargetFilePath = downloader.LocalFilePath;
                    dlg.Text = "正在下载 " + strNo + dlg.SourceFilePath;
                }));
                /*
                Task task = downloader.StartDownload(bAppend);
                task.Wait();    // TODO: 这里要允许中断
                */
                await downloader.StartDownload(bAppend);    // 2021/12/8 改为 await
                if (downloader.IsCancellationRequested)
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

            func_end(bError);
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

        // 备份日志文件。即，把日志文件从服务器拷贝到本地目录。要处理好增量复制的问题。
        // return:
        //      -1  出错
        //      0   放弃下载，或者没有必要下载。提示信息在 strError 中
        //      1   成功启动了下载
        public int BackupOperLogFiles(ref string strOutputFolder,
            out string strError)
        {
            strError = "";
            // if (string.IsNullOrEmpty(strOutputFolder))
            {
                FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

                dir_dlg.Description = "请指定下载目标文件夹(注意每次尽量指定同一个文件夹，这样软件就只下载增量部分)";
                dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
                dir_dlg.ShowNewFolderButton = true;
                dir_dlg.SelectedPath = strOutputFolder;

                if (dir_dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                strOutputFolder = dir_dlg.SelectedPath;
            }

            try
            {
                DateTime now = DateTime.Now;

                string strLastDate = ReadOperLogMemoryFile(strOutputFolder);
                if (string.IsNullOrEmpty(strLastDate) == false
                    && strLastDate.Length != 8)
                    strLastDate = "";

                // 列出已经下载的文件列表
                // 当天下载当天日期的日志文件，要创建一个同名的状态文件，表示它可能没有完成。以后再处理的时候，如果不再是当天，确保下载完成了，可以删除状态文件
                List<OperLogFileInfo> local_files = GetLocalOperLogFileNames(strOutputFolder, strLastDate);
                List<OperLogFileInfo> server_files = GetServerOperLogFileNames(strLastDate);

                // 计算出尚未下载的文件
                List<DownloadFileInfo> fileinfos = GetDownloadFileList(local_files, server_files);
                if (fileinfos.Count == 0)
                {
                    WriteOperLogMemoryFile(strOutputFolder, now);
                    strError = "服务器端没有发现新增的日志文件";
                    return 0;
                }

                string strFolder = strOutputFolder;
                // 关注以前曾经下载的，可能服务器端发生了变化的文件。从文件尺寸可以看出来。
                // return:
                //      -1  出错
                //      0   放弃下载
                //      1   成功启动了下载
                int nRet = BeginDownloadFiles(
                    fileinfos,
                    "append",
                    (bError) =>
                    {
                        // 写入记忆文件，然后提示结束
                        if (bError == false)
                            WriteOperLogMemoryFile(strFolder, now);

                        // this.ShowMessageBox("备份日志文件完成");
                        try
                        {
                            System.Diagnostics.Process.Start(strFolder);
                        }
                        catch (Exception ex)
                        {
                            this.ShowMessageBox("Process.Start() fail: " + ExceptionUtil.GetAutoText(ex));
                        }
                    },
                    ref strOutputFolder,
                    out strError);
                if (nRet == 0)
                    return 0;

                return 1;
            }
            catch (Exception ex)
            {
                strError = "BackupOperLogFiles() 出现异常: " + ex.Message;
                return -1;
            }
        }

        // 写入记忆当前日期的文件
        static void WriteOperLogMemoryFile(string strDirectory,
            DateTime now)
        {
            string filename = Path.Combine(strDirectory, "operlog_backup.txt");
            File.WriteAllText(filename, DateTimeUtil.DateTimeToString8(now));
        }

        static string ReadOperLogMemoryFile(string strDirectory)
        {
            string filename = Path.Combine(strDirectory, "operlog_backup.txt");
            if (File.Exists(filename) == false)
                return null;
            return File.ReadAllText(filename);
        }

        List<DownloadFileInfo> GetDownloadFileList(List<OperLogFileInfo> local_files,
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
                result.ServerPath = "!operlog/" + server_info.FileName;
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

        List<OperLogFileInfo> GetServerOperLogFileNames(string strLastDate)
        {
            if (string.IsNullOrEmpty(strLastDate) == false
    && strLastDate.Length != 8)
                throw new ArgumentException("strLastDate 参数值如果不为空，应该是 8 字符", "strLastDate");

            List<OperLogFileInfo> results = new List<OperLogFileInfo>();

            LibraryChannel channel = this.GetChannel();
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获取服务器端日志列表 ...");
            Stop.BeginLoop();

            stopManager.Active(Stop);

            try
            {
                FileItemLoader loader = new FileItemLoader(channel,
                    Stop,
                    "!operlog",
                    "*.log");
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
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");

                this.ReturnChannel(channel);
            }
        }

        // parameters:
        //      strLastDate 上次备份的最后日期，8 字符。如果为空，表示当前是首次备份
        List<OperLogFileInfo> GetLocalOperLogFileNames(string strDirectory,
            string strLastDate)
        {
            if (string.IsNullOrEmpty(strLastDate) == false
                && strLastDate.Length != 8)
                throw new ArgumentException("strLastDate 参数值如果不为空，应该是 8 字符", "strLastDate");

            DirectoryInfo di = new DirectoryInfo(strDirectory);

            FileInfo[] fis = di.GetFiles("*.log");

            List<OperLogFileInfo> results = new List<OperLogFileInfo>();
            foreach (FileInfo fi in fis)
            {
                if (string.IsNullOrEmpty(strLastDate) == false
                    && string.Compare(fi.Name, strLastDate) < 0)
                    continue;

                OperLogFileInfo result = new OperLogFileInfo();
                result.FileName = fi.Name;
                result.Length = fi.Length;
                results.Add(result);
            }
            return results;
        }



        #region 上传文件功能

#if NO
        // return:
        //      -1  出错
        //      0   放弃下载
        //      1   成功启动了下载
        public int BeginUploadFile(UploadFilesEventArgs e,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            channel = this.GetChannel();

            old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            FileDownloadDialog dlg = new FileDownloadDialog();
            dlg.Font = this.Font;
            dlg.Text = "正在上传 " + strPath;
            dlg.SourceFilePath = strPath;
            dlg.TargetFilePath = strTargetPath;
            dlg.Show(this);

            DynamicDownloader downloader = new DynamicDownloader(channel,
                strPath,
                strTargetPath);
            downloader.Tag = dlg;

            _downloaders.Add(downloader);

            downloader.Closed += new EventHandler(delegate (object o1, EventArgs e1)
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
            downloader.ProgressChanged += new DownloadProgressChangedEventHandler(delegate (object o1, DownloadProgressChangedEventArgs e1)
            {
                if (dlg.IsDisposed == false)
                    dlg.SetProgress(e1.Text, e1.BytesReceived, e1.TotalBytesToReceive);
            });
            // 2017/10/7
            downloader.Prompt += new MessagePromptEventHandler(delegate (object o1, MessagePromptEventArgs e1)
            {
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
            });
            dlg.FormClosed += new FormClosedEventHandler(delegate (object o1, FormClosedEventArgs e1)
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
#endif

        public void BeginUploadFiles(UploadFilesEventArgs e)
        {
            // 检查 dp2library 版本
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.14") < 0)
                throw new Exception($"上传文件功能必须和 dp2library 3.14 或以上版本配套使用(然而当前连接的 dp2library 版本是 {Program.MainForm.ServerVersion})");

            LibraryChannel channel = null;
            TimeSpan old_timeout = new TimeSpan(0);

            channel = this.GetChannel();

            old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 5, 0);

            Stop stop = new Stop();

            FileDownloadDialog dlg = new FileDownloadDialog();
            dlg.FormClosed += new FormClosedEventHandler(delegate (object o1, FormClosedEventArgs e1)
            {
                stop.DoStop();
                if (channel != null)
                {
                    channel.TryAbortIt();
                }
            });
            dlg.Font = this.Font;
            dlg.Show(this);

            stop.OnProgressChanged += new ProgressChangedEventHandler(delegate (object o1, ProgressChangedEventArgs e1)
            {
                dlg.SetProgress(e1.Message, // StringUtil.GetPercentText(e1.Value - e1.Start, e1.End - e1.Start),
                    e1.Value - e1.Start, 
                    e1.End - e1.Start);
            });
            stop.BeginLoop();

            Task.Factory.StartNew(() =>
            {
                string strError = "";
                foreach (string localfilename in e.SourceFileNames)
                {
                    if (stop.IsStopped)
                    {
                        strError = "用户中断";
                        dlg.SetText(strError);
                        goto ERROR1;
                    }

                    string strTargetPath = Path.Combine(e.TargetFolder, Path.GetFileName(localfilename)).Replace("\\", "/");
                    // channel.UploadResChunkSize = nChunkSize;

                    this.Invoke((Action)(() =>
                    {
                        dlg.Text = "正在上传 " + strTargetPath;
                        dlg.SourceFilePath = localfilename;
                        dlg.TargetFilePath = strTargetPath;
                    }));

                    bool _hide_dialog = false;
                    int _hide_dialog_count = 0;

                    int nRet = channel.UploadObject(
                stop,
                localfilename,
                strTargetPath,
                "_checkMD5," + ((StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.120") >= 0) ? "gzip" : ""),
                null,   // timestamp,
                true,
                true,
                (c, m, buttons, sec) =>
                {
                    DialogResult result = DialogResult.Yes;
                    if (_hide_dialog == false)
                    {
                        this.Invoke((Action)(() =>
                        {
                            result = MessageDialog.Show(this,
                        m,
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxDefaultButton.Button1,
                        "此后不再出现本对话框",
                        ref _hide_dialog,
                        buttons,    // new string[] { "重试", "中断" },
                        sec);
                        }));
                        _hide_dialog_count = 0;
                    }
                    else
                    {
                        _hide_dialog_count++;
                        if (_hide_dialog_count > 10)
                            _hide_dialog = false;
                    }

                    if (result == DialogResult.Yes)
                        return buttons[0];
                    else if (result == DialogResult.No)
                        return buttons[1];
                    return buttons[2];
                },
                out byte[] temp_timestamp,
                out strError);
                    if (nRet == -1)
                    {
                        strError = $"上传 '{ localfilename}' --> '{ strTargetPath }' 时出错: {strError}";
                        dlg.SetText(strError);
                        goto ERROR1;
                    }
                }

                this.Invoke((Action)(() =>
                {
                    e.FuncEnd?.Invoke(false);
                }));
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
                return;
                ERROR1:
                this.Invoke((Action)(() =>
                {
                    e.FuncEnd?.Invoke(false);
                }));
                if (channel != null)
                {
                    channel.Timeout = old_timeout;
                    this.ReturnChannel(channel);
                    channel = null;
                }
                this.Invoke((Action)(() =>
                {
                    dlg.Close();
                    MessageBox.Show(this, strError);
                }));
            },
CancellationToken.None,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);

        }

        #endregion
    }
}
