using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

using Ionic.Zip;

using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.Install
{
#if REMOVED
    /// <summary>
    /// 绿色安装
    /// </summary>
    public static class GreenInstaller
    {
        /*
        // 软件安装包下载路径。例如 http://dp2003.com/dp2ssl/v1
        public string DownloadUrl { get; set; }

        // 安装目标路径。例如 c:\dp2ssl
        public string InstallDirectory { get; set; }

        // 用户目录。例如 c:\ProgramData\dp2\dp2ssl 或 c:\Users\xietao\dp2ssl
        // 如果为空，则默认 当前 Windows 用户的用户目录的 xxxx 子目录 (xxxx 为产品名，例如 dp2ssl 或者 dp2circulation_v2 之类)
        public string UserDirectory { get; set; }
        */

        // 迁移用户文件夹
        public static NormalResult MoveUserDirectory(string sourceDirectory,
            string targetDirectory,
            string style)
        {
            int nRet = PathUtil.CopyDirectory(sourceDirectory, 
                targetDirectory,
                false, 
                out string strError);
            if (nRet == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"CopyDirectory() 出错: {strError}"
                };

            // 在源目录中做出标记，以便以后用到这个目录的程序会警告退出
            if (StringUtil.IsInList("maskSource", style))
            {
                string strMaskFileName = Path.Combine(sourceDirectory, "userDirectoryMask.txt");
                File.WriteAllText(strMaskFileName, $"removed:此用户文件夹已经被移动到 {targetDirectory}");
            }

            return new NormalResult();
        }

        // -1 -1 n only change progress value
        // -1 -1 -1 hide progress bar
        public delegate void Delegate_setProgress(double min, double max, double value, string text);

        // 从 Web 服务器安装或者升级绿色版
        // result.Value:
        //      -1  出错
        //      0   经过检查发现没有必要升级
        //      1   成功
        //      2   成功，但需要立即重新启动计算机才能让复制的文件生效
        public static async Task<NormalResult> InstallFromWeb(string downloadUrl,
            string installDirectory,
            string userDirectory,
            // string waitExe,
            bool delayUpdate,
            Delegate_setProgress setProgress)
        {
            string strBaseUrl = downloadUrl;
            if (strBaseUrl[strBaseUrl.Length - 1] != '/')
                strBaseUrl += "/";

            string strBinDir = installDirectory;

            string strUtilDir = GetUtilDir(strBinDir);

            setProgress?.Invoke(0, 0, 0, "开始自动更新(绿色安装)");

            // 检查状态文件
            // result.Value
            //      -1  出错
            //      0   不存在状态文件
            //      1   正在下载 .zip 过程中。.zip 不完整
            //      2   当前 .zip 和 .exe 已经一样新
            //      3   当前 .zip 比 .exe 要新。需要展开 .zip 进行更新安装
            //      4   下载 .zip 失败。.zip 不完整
            //      5   当前 .zip 比 .exe 要新，需要重启计算机以便展开的文件生效
            var check_result = CheckStateFile(strBinDir);
            if (check_result.Value == -1)
                return check_result;
            if (check_result.Value == 1 || check_result.Value == 3 || check_result.Value == 5)
                return new NormalResult();

            // 希望下载的文件。纯文件名
            List<string> filenames = new List<string>() {
                // "greenutility.zip", // 这是工具软件，不算在 dp2circulation 范围内
                "app.zip",
                "data.zip"};

            // 发现更新了并下载的文件。纯文件名
            List<string> updated_filenames = new List<string>();

            // 需要确保最后被展开的文件。如果下载了而未展开，则下次下载的时候会发现文件已经是最新了，从而不会下载，也不会展开。这就有漏洞了
            // 那么就要在下载和展开这个全过程中断的时候，记住删除已经下载的文件。这样可以迫使下次一定要下载和展开
            List<string> temp_filepaths = new List<string>();

            try
            {
                int downloadCount = 0;
                foreach (string filename in filenames)
                {
                    string strUrl = // "http://dp2003.com/dp2circulation/v2/"
                        strBaseUrl
                        + filename;
                    string strLocalFileName = Path.Combine(strBinDir, filename).ToLower();

                    if (File.Exists(strLocalFileName) == true)
                    {
                        // this.DisplayBackgroundText("检查文件版本 " + strUrl + " ...\r\n");

                        // 判断 http 服务器上一个文件是否已经更新
                        // return:
                        //      -1  出错
                        //      0   没有更新
                        //      1   已经更新
                        var result = await IsServerFileUpdated(strUrl,
                            File.GetLastWriteTimeUtc(strLocalFileName));
                        if (result.Value == -1)
                        {
                            if (downloadCount > 0)
                                WriteStateFile(strBinDir, "downloadError");

                            return result;
                        }
                        if (result.Value == 1)
                            updated_filenames.Add(filename);
#if NO
                        else
                            this.DisplayBackgroundText("没有更新。\r\n");
#endif
                    }
                    else
                        updated_filenames.Add(filename);

                    if (updated_filenames.IndexOf(filename) != -1)
                    {
                        WriteStateFile(strBinDir, "downloading");

                        setProgress?.Invoke(0, 0, 0, "下载 " + strUrl + " 到 " + strLocalFileName + " ...\r\n");

                        var result = await DownloadFileAsync(strUrl,
                            strLocalFileName);
                        if (result.Value == -1)
                        {
                            WriteStateFile(strBinDir, "downloadError");
                            return result;
                        }

                        // 下载成功的本地文件，随时可能被删除，如果整个流程没有完成的话
                        temp_filepaths.Add(strLocalFileName);

                        downloadCount++;
                    }
                }

#if NO
                string strGreenUtilityExe = Path.Combine(strUtilDir, "greenutility.exe");

                if (updated_filenames.IndexOf("greenutility.zip") != -1
                    || File.Exists(strGreenUtilityExe) == false)
                {
                    // 将 greenutility.zip 展开到 c:\dp2circulation_temp
                    string strZipFileName = Path.Combine(strBinDir, "greenutility.zip").ToLower();
                    string strTargetDir = strUtilDir;

                    setProgress?.Invoke(0, 0, 0, "展开文件 " + strZipFileName + " 到 " + strTargetDir + " ...\r\n");
                    try
                    {
                        using (ZipFile zip = ZipFile.Read(strZipFileName))
                        {
                            foreach (ZipEntry e in zip)
                            {
                                e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string strError = "展开文件 '" + strZipFileName + "' 到目录 '" + strTargetDir + "' 时出现异常" + ex.Message;
                        // 删除文件，以便下次能重新下载和展开
                        try
                        {
                            File.Delete(Path.Combine(strTargetDir, "greenutility.zip"));
                        }
                        catch
                        {

                        }
                        /*
                        ReportError("dp2circulation 展开 greenutility.zip 时出错", strError);
                        return;
                        */
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };
                    }

                    updated_filenames.Remove("greenutility.zip");
                    temp_filepaths.Remove(strZipFileName);
                }

#endif

                List<string> _updatedGreenZipFileNames = new List<string>();
#if TEST
            // 测试
            this._updatedGreenZipFileNames = new List<string>();
            this._updatedGreenZipFileNames.Add("app.zip");
#else
                // 给 MainForm 一个标记，当它退出的时候，会自动展开 .zip 文件完成升级安装
                _updatedGreenZipFileNames = updated_filenames;
#endif

                WriteStateFile(strBinDir, "downloadComplete");

                if (delayUpdate)
                {

                    if (_updatedGreenZipFileNames.Count > 0)
                        setProgress?.Invoke(0, 0, 0, "dp2circulation 绿色安装包升级文件已经准备就绪。当退出 dp2circulation 时会自动进行安装。\r\n");
                    else
                        setProgress?.Invoke(0, 0, 0, "没有发现更新。\r\n");

                    temp_filepaths.Clear(); // 这样 finally 块就不会删除这些文件了
                }
                else
                {
                    temp_filepaths.Clear(); // 这样 finally 块就不会删除这些文件了

                    if (_updatedGreenZipFileNames != null && _updatedGreenZipFileNames.Count > 0)
                    {
                        setProgress?.Invoke(0, 0, 0, "正在解压文件");

                        // TODO: 直接解压到目标位置即可
                        string files = StringUtil.MakePathList(_updatedGreenZipFileNames);
                        // 安装软件。用 MoveFileEx 法
                        // return:
                        //      -1  出错
                        //      0   成功。不需要 reboot
                        //      1   成功。需要 reboot
                        int nRet = Install_2(strBinDir,
                            strBinDir,
                            files,
                            Path.GetTempPath(),
                            out string strError);
                        if (nRet == -1)
                        {
                            // TODO: 这里是否需要删除两个 .zip 文件以便迫使后面重新下载和展开？
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        }
                        if (nRet == 1)
                        {
                            WriteStateFile(strBinDir, "waitingReboot");

                            return new NormalResult
                            {
                                Value = 2   // 表示需要重启计算机
                            };
                        }
                        /*
                        StartGreenUtility(_updatedGreenZipFileNames,
                            strBinDir,
                            waitExe);
                        */
                        WriteStateFile(strBinDir, "installed");
                    }
                }
                return new NormalResult { Value = 1 };
            }
            finally
            {
                foreach (string filepath in temp_filepaths)
                {
                    File.Delete(filepath);
                }
            }

            /*
            return;
        ERROR1:
            // ShowMessageBox(strError);
            this.DisplayBackgroundText("绿色更新过程出错: " + strError + "\r\n");
            ReportError("dp2circulation GreenUpdate() 出错", strError);
            */
        }

        // result.Value
        //      -1  出错
        //      0   不存在状态文件
        //      1   正在下载 .zip 过程中。.zip 不完整
        //      2   当前 .zip 和 .exe 已经一样新
        //      3   当前 .zip 比 .exe 要新。需要展开 .zip 进行更新安装
        //      4   下载 .zip 失败。.zip 不完整
        //      5   当前 .zip 比 .exe 要新，需要重启计算机以便展开的文件生效
        static NormalResult CheckStateFile(string installDirectory)
        {
            string stateFileName = Path.Combine(installDirectory, "install_state.txt");
            if (File.Exists(stateFileName) == false)
                return new NormalResult { Value = 0 };
            string content = File.ReadAllText(stateFileName);
            if (content == "installed")
                return new NormalResult
                {
                    Value = 2,
                    ErrorCode = "installed"
                };
            if (content == "downloadComplete")
                return new NormalResult
                {
                    Value = 3,
                    ErrorCode = "downloadComplete"
                };
            if (content == "downloadError")
                return new NormalResult
                {
                    Value = 4,
                    ErrorCode = "downloadError"
                };
            if (content == "waitingReboot")
                return new NormalResult
                {
                    Value = 5,
                    ErrorCode = "waitingReboot"
                };
            return new NormalResult
            {
                Value = 1,
                ErrorCode = "downloading"
            };
        }

        static NormalResult WriteStateFile(string installDirectory,
            string content)
        {
            string stateFileName = Path.Combine(installDirectory, "install_state.txt");
            PathUtil.CreateDirIfNeed(installDirectory);
            File.WriteAllText(stateFileName, content);
            return new NormalResult();
        }

        // 安装软件。用 MoveFileEx 法
        // return:
        //      -1  出错
        //      0   成功。不需要 reboot
        //      1   成功。需要 reboot
        static int Install_2(
            string strSourceDir,
            string strTargetDir,
            string strFiles,
            string strTempDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // strTargetDir 一般为 c:\dp2circulation

#if NO
            string strNewDir = strTargetDir + "_new";
            string strOldDir = strTargetDir + "_old";

            // *** 将 c:\dp2circulation 目录中的全部文件，复制到 c:\dp2circulation_new 中
            if (Directory.Exists(strTargetDir) == true)
            {
                Console.WriteLine("复制 " + strTargetDir + " 到 " + strNewDir + " ...");

                // app.zip 和 data.zip 不要复制
                // return:
                //      -1  出错
                //      >=0 复制的文件总数
                nRet = CopyDirectory(strTargetDir,
                    strNewDir,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
#endif

            // *** 将 app.zip 和 data.zip 展开覆盖到 c:\dp2circulation_new 目录
            bool bNeedReboot = false;
            string[] files = strFiles.Split(new char[] { ',' });
            foreach (string file in files)
            {
                string strZipFileName = Path.Combine(strSourceDir, file);
                Console.WriteLine("展开 " + strZipFileName + " 到 " + strTargetDir + " ...");
                // return:
                //      -1  出错
                //      0   成功。不需要 reboot
                //      1   成功。需要 reboot
                nRet = ExtractFile(strZipFileName,
                    strTargetDir,
                    strTempDir,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    bNeedReboot = true;
            }

#if NO
            strZipFileName = Path.Combine(strSourceDir, "data.zip");
            Console.WriteLine("展开 " + strZipFileName + " 到 " + strTargetDir + " ...");
            // return:
            //      -1  出错
            //      0   成功。不需要 reboot
            //      1   成功。需要 reboot
            nRet = ExtractFile(strZipFileName,
                strTargetDir,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
                bNeedReboot = true;
#endif

            if (bNeedReboot)
                return 1;
            return 0;
        ERROR1:
            return -1;
        }

        // return:
        //      -1  出错
        //      0   成功。不需要 reboot
        //      1   成功。需要 reboot
        static int ExtractFile(string strZipFileName,
            string strTargetDir,
            string strTempDir,
            bool bAllowDelayOverwrite,
            out string strError)
        {
            strError = "";

            bool bNeedReboot = false;
            try
            {
                using (ZipFile zip = ZipFile.Read(strZipFileName))
                {
                    foreach (ZipEntry e in zip)
                    {
                        // e.Extract(this.UserDir, ExtractExistingFileAction.OverwriteSilently);
                        if ((e.Attributes & FileAttributes.Directory) == 0)
                        {
                            if (ExtractFile(e,
                                strTargetDir,
                                strTempDir,
                                bAllowDelayOverwrite) == true)
                                bNeedReboot = true;

                        }
                        else
                            e.Extract(strTargetDir, ExtractExistingFileAction.OverwriteSilently);

                    }
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            if (bNeedReboot == true)
                return 1;
            return 0;
        }

        // parameters:
        //      bAllowDelayOverwrite    是否允许延迟到 reboot 后的覆盖
        // return:
        //      false   正常结束
        //      true    发生了 MoveFileEx，需要 reboot 才会发生作用
        static bool ExtractFile(ZipEntry e,
            string strTargetDir,
            string strTempDir,
            bool bAllowDelayOverwrite)
        {
            /*
            string strTempDir = "c:\\~dp2circulation_temp_file";
            CreateDirIfNeed(strTempDir);
            TempDir = strTempDir;
             * */

            // string strTempDir = TempDir;

            string strTempPath = Path.Combine(strTempDir, Path.GetFileName(e.FileName));
            string strTargetPath = Path.Combine(strTargetDir, e.FileName);

            using (FileStream stream = new FileStream(strTempPath, FileMode.Create))
            {
                e.Extract(stream);
            }

            int nErrorCount = 0;
            for (; ; )
            {
                try
                {
                    // 确保目标目录已经创建
                    PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strTargetPath));

                    File.Copy(strTempPath, strTargetPath, true);
                    File.SetLastWriteTime(strTargetPath, e.LastModified);

                    Console.WriteLine("展开文件 " + strTargetPath);
                }
                catch (Exception ex)
                {
                    if (nErrorCount > 10 || bAllowDelayOverwrite)
                    {
                        if (bAllowDelayOverwrite == false)
                            throw new Exception("复制文件 " + strTempPath + " 到 " + strTargetPath + " 的过程中出现错误: " + ex.Message);
                        else
                        {
                            string strLastFileName = Path.Combine(strTempDir, Guid.NewGuid().ToString());
                            File.Move(strTempPath, strLastFileName);
                            strTempPath = "";
                            if (MoveFileEx(strLastFileName, strTargetPath, MoveFileFlags.DelayUntilReboot | MoveFileFlags.ReplaceExisting) == false)
                                throw new Exception("MoveFileEx() '" + strLastFileName + "' '" + strTargetPath + "' 失败");
                            File.SetLastWriteTime(strLastFileName, e.LastModified);
                            Console.WriteLine("延迟展开文件 " + strTargetPath);
                            return true;
                        }
                    }

                    nErrorCount++;
                    Thread.Sleep(1000);
                    continue;
                }
                break;
            }
            if (string.IsNullOrEmpty(strTempPath) == false)
                File.Delete(strTempPath);

            return false;
        }

#region MoveFileEx

        [Flags]
        internal enum MoveFileFlags
        {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool MoveFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            MoveFileFlags dwFlags);

#endregion

        // 启动绿色安装小工具。因为 dp2circulation 正在运行时无法覆盖替换文件，所以需要另外启动一个小程序来完成这个任务
        public static int StartGreenUtility(List<string> _updatedGreenZipFileNames,
            string strBinDir,
            string waitExe)
        {
            if (_updatedGreenZipFileNames.Count == 0)
                throw new ArgumentException("调用 StartGreenUtility() 前应该准备好 _updatedGreenZipFileNames 内容");

            // string strBinDir = GetBinDir();
            string strUtilDir = GetUtilDir(strBinDir);

            string strExePath = Path.Combine(strUtilDir, "greenutility.exe");
            string strParameters = "-silence -action:install -source:"
                + strBinDir  // source 是指存储了 .zip 文件的目录
                + " -target:" + strBinDir // target 是指最终要安装的目录 
                + (string.IsNullOrEmpty(waitExe) ? "" : $" -wait:{waitExe}")   // " -wait:dp2circulation.exe"
                + " -files:" + StringUtil.MakePathList(_updatedGreenZipFileNames);
            try
            {
                var process = System.Diagnostics.Process.Start(strExePath, strParameters);
                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Win32Exception ex)
            {
                // 改为抛出包含 Win32 错误码的异常
                // https://msdn.microsoft.com/en-us/library/ms681382(v=vs.85).aspx
                // ERROR_ACCESS_DENIED
                // 5 (0x5)
                // Access is denied.
                int error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                throw new Exception("GetLastWin32Error [" + error.ToString() + "], ex.NativeErrorCode = " + ex.NativeErrorCode + ", ex.ErrorCode=" + ex.ErrorCode, ex);
            }

            // this._updatedGreenZipFileNames.Clear(); // 避免后面再次调用本函数
        }

#region 下级函数

        static string GetUtilDir(string strBinDir)
        {
            return Path.Combine(Path.GetDirectoryName(strBinDir), "~" + Path.GetFileName(strBinDir) + "_greenutility");
        }

        // 判断 http 服务器上一个文件是否已经更新
        // return:
        //      -1  出错
        //      0   没有更新
        //      1   已经更新
        static async Task<NormalResult> IsServerFileUpdated(string strUrl,
            DateTime local_lastmodify)
        {
            var webRequest = System.Net.WebRequest.Create(strUrl);
            webRequest.Method = "HEAD";
            webRequest.Timeout = 5000;
            try
            {
                using (var response = await webRequest.GetResponseAsync() as HttpWebResponse)
                {
                    string strLastModified = response.GetResponseHeader("Last-Modified");
                    if (string.IsNullOrEmpty(strLastModified) == true)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "header 中无法获得 Last-Modified 值"
                        };
                    }

                    if (DateTimeUtil.TryParseRfc1123DateTimeString(strLastModified, out DateTime time) == false)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"从响应中取出的 Last-Modified 字段值 '{ strLastModified}' 格式不合法"
                        };
                    }

                    if (time > local_lastmodify)
                        return new NormalResult { Value = 1 };

                    return new NormalResult { Value = 0 };
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = ex.Message,
                            ErrorCode = "notFound"
                        };
                    }
                }
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                    ErrorCode = ex.GetType().ToString()
                };
            }
            catch (Exception ex)
            {
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = ExceptionUtil.GetAutoText(ex),
                    ErrorCode = ex.GetType().ToString()
                };
            }
        }

        class MyWebClient : WebClient
        {
            public int Timeout = -1;
            public int ReadWriteTimeout = -1;

            HttpWebRequest _request = null;

            protected override WebRequest GetWebRequest(Uri address)
            {
                _request = (HttpWebRequest)base.GetWebRequest(address);

#if NO
            this.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
            request.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
#endif
                if (this.Timeout != -1)
                    _request.Timeout = this.Timeout;
                if (this.ReadWriteTimeout != -1)
                    _request.ReadWriteTimeout = this.ReadWriteTimeout;
                return _request;
            }

            public void Cancel()
            {
                if (this._request != null)
                    this._request.Abort();
            }
        }


        // 从 http 服务器下载一个文件
        // 阻塞式
        static async Task<NormalResult> DownloadFileAsync(string strUrl,
    string strLocalFileName)
        {
            using (MyWebClient webClient = new MyWebClient())
            {
                webClient.ReadWriteTimeout = 30 * 1000; // 30 秒，在读写之前 - 2015/12/3
                webClient.Timeout = 30 * 60 * 1000; // 30 分钟，整个下载过程 - 2015/12/3
                webClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                string strTempFileName = strLocalFileName + ".temp";

                // 2020/6/4
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strTempFileName));

                // TODO: 先下载到临时文件，然后复制到目标文件
                try
                {
                    await webClient.DownloadFileTaskAsync(new Uri(strUrl, UriKind.Absolute), strTempFileName);

                    File.Delete(strLocalFileName);
                    File.Move(strTempFileName, strLocalFileName);
                    return new NormalResult();
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        var response = ex.Response as HttpWebResponse;
                        if (response != null)
                        {
                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = $"{strUrl} 不存在 ({ex.Message})",
                                    ErrorCode = "notFound"
                                };
                            }
                        }
                    }

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ExceptionUtil.GetDebugText(ex),
                        ErrorCode = ex.GetType().ToString()
                    };
                }
                catch (Exception ex)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = ExceptionUtil.GetDebugText(ex),
                        ErrorCode = ex.GetType().ToString()
                    };
                }
            }
        }

#endregion

    }

#endif
}
