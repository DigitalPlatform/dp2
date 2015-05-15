using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.IO;
using DigitalPlatform.Range;
using DigitalPlatform.Xml;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace dp2LibraryConsole
{
    /// <summary>
    /// 一个实例
    /// </summary>
    public class Instance : IDisposable
    {
        string _currentDir = "";    // 当前服务器路径

        string _currentLocalDir = "";   // 当前本地路径

        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = new LibraryChannel();

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 界面语言代码
        /// </summary>
        public string Lang = "zh";

        public Instance()
        {
            if (string.IsNullOrEmpty(this.UserDir) == true)
            {
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2LibraryConsole_v1");
            }
            PathUtil.CreateDirIfNeed(this.UserDir);

            this.AppInfo = new ApplicationInfo(Path.Combine(this.UserDir, "settings.xml"));

            this._currentLocalDir = Directory.GetCurrentDirectory();

            this.Channel.Url = this.LibraryServerUrl;

            this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
        }

        // 显示命令行提示符
        public void DisplayPrompt()
        {
            Console.WriteLine();
            string strLocal = "本地 " + this._currentLocalDir;
            Console.Write(new string('-', strLocal.Length) + "\r\n" + strLocal + "\r\n远程 " + this._currentDir + "\r\n>");
        }

        // return:
        //      false   正常，继续
        //      true    退出命令
        public bool ProcessCommand(string line)
        {
            if (line == "exit" || line == "quit")
                return true;

            string strError = "";
            int nRet = 0;

            List<string> parameters = ParseParameters(line);

            if (parameters[0] == "login")
            {
                string strHost = "";
                string strUserName = "";
                string strPassword = "";

                if (parameters.Count > 1)
                    strHost = parameters[1];
                if (parameters.Count > 2)
                    strUserName = parameters[2];
                if (parameters.Count > 3)
                    strPassword = parameters[3];

                if (parameters.Count <= 3)
                {
                    if (GetUserNamePassword(out strHost,
                        out strUserName,
                        out strPassword) == true)
                        return true;
                }

                nRet = Login(strHost, strUserName, strPassword, out strError);
                if (nRet == -1)
                {
                    Console.WriteLine("Login error: " + strError);
                    return false;
                }

                Console.WriteLine("Login succeed");
                return false;
            }

            if (parameters[0] == "dir")
            {
                string strDir = this._currentLocalDir;
                if (parameters.Count > 1)
                    strDir = parameters[1];

                int file_count = 0;
                int dir_count = 0;

                DirectoryInfo di = new DirectoryInfo(strDir);

                Console.WriteLine();
                Console.WriteLine("本地 " + di.FullName + " 的目录:");
                Console.WriteLine();

                DirectoryInfo[] dis = di.GetDirectories();
                foreach (DirectoryInfo info in dis)
                {
                    AlignWrite(info.LastWriteTime.ToString("u") + " <dir>      ");
                    Console.WriteLine(info.Name + "/");
                    dir_count++;
                }

                FileInfo[] fis = di.GetFiles();
                foreach (FileInfo info in fis)
                {
                    AlignWrite(info.LastWriteTime.ToString("u") + info.Length.ToString().PadLeft(10, ' ') + "  ");
                    Console.WriteLine(info.Name);
                    file_count++;
                }

                Console.WriteLine();

                if (dir_count > 0)
                    AlignWriteLine(dir_count.ToString() + " direcotries");
                if (file_count > 0 || dir_count == 0)
                    AlignWriteLine(file_count.ToString() + " files");
                return false;
            }

            if (parameters[0] == "cd")
            {
                if (parameters.Count == 1)
                {
                    Console.WriteLine(this._currentLocalDir);    // 显示当前路径
                    return false;
                }
                if (parameters.Count > 1)
                {
                    string strDir = "";

                    if (parameters[1] == "..")
                        strDir = Path.GetDirectoryName(this._currentLocalDir);
                    else if (parameters[1] == ".")
                    {
                        // 当前路径不变
                        return false;
                    }
                    else
                    {
                        strDir = Path.Combine(this._currentLocalDir, parameters[1]);
                        // 上一句执行完，有可能是这样 strDir = '\publish\dp2libraryxe'，没有盘符
                    }

                    // 要检验一下目录是否存在
                    if (Directory.Exists(strDir) == false)
                    {
                        Console.WriteLine("本地目录 '" + strDir + "' 不存在");
                        return false;
                    }

                    strDir = (new DirectoryInfo(strDir)).FullName;
                    this._currentLocalDir = strDir;
                }
                return false;
            }

            if (parameters[0] == "rdir")
            {
                List<string> filenames = null;

                string strDir = this._currentDir;
                if (parameters.Count > 1)
                    strDir = parameters[1];

                Console.WriteLine();
                Console.WriteLine("远程 " + this._currentDir + " 的目录:");
                Console.WriteLine();

                nRet = Dir(this._currentDir,
            out filenames,
            out strError);
                if (nRet == -1)
                {
                    Console.WriteLine("Dir error: " + strError);
                    return false;
                }

                int file_count = 0;
                int dir_count = 0;
                foreach (string filename in filenames)
                {
                    string strName = "";
                    string strTime = "";
                    string strSize = "";
                    
                    ParseFileName(filename,
            out strName,
            out strTime,
            out strSize);

                    if (strSize == "dir")
                    {
                        dir_count++;
                        AlignWrite(strTime + " <dir>      ");
                        Console.WriteLine(strName);
                    }
                    else
                    {
                        file_count++;
                        AlignWrite(strTime + strSize.PadLeft(10, ' ') + "  ");
                        Console.WriteLine(strName);
                    }
                }

                Console.WriteLine();

                if (dir_count > 0)
                    AlignWriteLine(dir_count.ToString() + " direcotries");
                if (file_count > 0 || dir_count == 0)
                    AlignWriteLine(file_count.ToString() + " files");
                return false;
            }

            if (parameters[0] == "rcd")
            {
                if (parameters.Count == 1)
                {
                    Console.WriteLine(this._currentDir);    // 显示当前路径
                    return false;
                }
                if (parameters.Count > 1)
                {
                    string strDir = "";

                    if (parameters[1] == "..")
                    {
                        if (string.IsNullOrEmpty(this._currentDir) == true)
                        {
                            Console.WriteLine("远程目录当前已经是根目录了");
                            return false;
                        }
                        strDir = Path.GetDirectoryName(this._currentDir);
                    }
                    else if (parameters[1] == ".")
                    {
                        // 当前路径不变
                        return false;
                    }
                    else
                    {
                        // strDir = Path.Combine(this._currentDir, parameters[1]);
                        strDir = GetFullDirectory(parameters[1]);
                    }

                    // 要检验一下目录是否存在
                    nRet = RemoteDirExists(strDir);
                    if (nRet == -1)
                        return false;
                    if (nRet == 0)
                    {
                        Console.WriteLine("远程目录 '" + strDir + "' 不存在");
                        return false;
                    }

                    this._currentDir = strDir;
                }
                return false;
            }

            if (parameters[0] == "upload")
            {
                if (parameters.Count != 3)
                {
                    Console.WriteLine("upload 命令用法: upload 源目录 目标目录");
                    return false;
                }

                string strSource = parameters[1];
                string strTarget = parameters[2];

                string strSourcePath = GetLocalFullDirectory(strSource);

                if (Directory.Exists(strSourcePath) == true)
                {
                    string strServerFilePath = "!upload/" + GetFullDirectory(strTarget) + "/reports.zip";
                    string strZipFileName = Path.GetTempFileName();
                    try
                    {
                        // return:
                        //      -1  出错
                        //      0   没有发现需要上传的文件
                        //      1   成功压缩创建了 .zip 文件
                        nRet = CompressDirecotry(
                    strSourcePath,
                    strZipFileName,
                    Encoding.UTF8,
                    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // return:
                        //		-1	出错
                        //		0   上传文件成功
                        nRet = UploadFile(
                    null,
                    this.Channel,
                    strZipFileName,
                    strServerFilePath,
                                "extractzip",
                    null,
                    true,
                    out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        Console.WriteLine();
                        Console.WriteLine("本地目录 " + GetLocalFullDirectory(strSource) + " 成功上传到远程 " + GetFullDirectory(strTarget));
                        return false;
                    }
                    finally
                    {
                        File.Delete(strZipFileName);
                    }
                }
                else if (File.Exists(strSourcePath) == true)
                {
                    string strServerFilePath = "!upload/" + GetFullDirectory(strTarget);
                    // return:
                    //		-1	出错
                    //		0   上传文件成功
                    nRet = UploadFile(
                null,
                this.Channel,
                strSourcePath,
                strServerFilePath,
                            "",
                null,
                true,
                out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    Console.WriteLine();
                    Console.WriteLine("本地文件 " + GetLocalFullDirectory(strSource) + " 成功上传到远程 " + GetFullDirectory(strTarget));
                    return false;
                }
            }

            if (parameters[0] == "rdel" || parameters[0] == "rdelete")
            {
                if (parameters.Count != 2)
                {
                    Console.WriteLine("rdel 命令用法: rdel 远程目录");
                    return false;
                }

                string strRemoteFilePath = GetFullDirectory(parameters[1]); // 远程逻辑路径
                string strRemote = "!upload/" + strRemoteFilePath;  // 远程物理路径
                // 删除一个远程文件或者目录
                nRet = DeleteRemoteFile(
                    null,
                    this.Channel,
                    strRemote,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                Console.WriteLine("已删除远程文件 " + strRemoteFilePath);
                return false;
            }

            Console.WriteLine("unknown command '" + line + "'");
            return false;
            ERROR1:
                Console.WriteLine(strError);
                return false;
        }

        // 解析从 dp2library 返回的文件名字符串
        // name|time|size
        // 如果是目录名 size 部分为 "dir"
        static void ParseFileName(string strText,
            out string strName,
            out string strTime,
            out string strSize)
        {
            strName = "";
            strTime = "";
            strSize = "";
            string[] parts = strText.Split(new char[] {'|'});
            if (parts.Length > 0)
                strName = parts[0];
            if (parts.Length > 1)
                strTime = parts[1];
            if (parts.Length > 2)
                strSize = parts[2];
        }

        string GetFullDirectory(string strInput)
        {
            return Path.Combine(this._currentDir, strInput);
        }

        string GetLocalFullDirectory(string strInput)
        {
            return Path.Combine(this._currentLocalDir, strInput);
        }

        static void AlignWriteLine(string strText)
        {
            Console.WriteLine(strText.PadLeft(30, ' '));
        }

        static void AlignWrite(string strText)
        {
            Console.Write(strText.PadLeft(30, ' '));
        }

        // reutrn:
        //      -1  出错
        //      0   不存在
        //      1   存在
        int RemoteDirExists(string strDir)
        {
            if (string.IsNullOrEmpty(strDir) == true)
                return 1;

            string strUpLevel = Path.GetDirectoryName(strDir);

            string strPureName = Path.GetFileName(strDir);
            List<string> filenames = null;
            string strError = "";
            int nRet = Dir(strUpLevel,
out filenames,
out strError);
            if (nRet == -1)
            {
                Console.WriteLine("Dir error: " + strError);
                return -1;
            }

            // strPureName = MakeDirectory(strPureName);

            foreach (string filename in filenames)
            {
                string strName = "";
                string strTime = "";
                string strSize = "";

                ParseFileName(filename,
        out strName,
        out strTime,
        out strSize);
                if (strSize == "dir" && strName.ToLower() == strPureName.ToLower())
                    return 1;
            }

            return 0;
        }

        static bool IsDirectoryPath(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return true;
            strPath = strPath.Replace("\\", "/");

            if (strPath[strPath.Length - 1] == '/')
                return true;
            return false;
        }

        static string MakeDirectory(string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return "/";
            strPath = strPath.Replace("\\", "/");

            if (strPath[strPath.Length - 1] == '/')
                return strPath;

            return strPath + "/";
        }

        static List<string> ParseParameters(string line)
        {
            string[] parameters = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> result = new List<string>(parameters);

            return result;
        }

        // 获得输入的用户名和密码
        // return:
        //      false   正常，继续
        //      true    退出命令
        bool GetUserNamePassword(
            out string strHost,
            out string strUserName,
            out string strPassword)
        {
            strHost = "";
            strUserName = "";
            strPassword = "";

            Console.Write("Host: (当前 Host Url '" + this.LibraryServerUrl + "')");
            strHost = Console.ReadLine();

            Console.Write("User Name:");
            strUserName = Console.ReadLine();

            Console.Write("Password:");
            strPassword = Console.ReadLine();
            return false;
        }

        public void Dispose()
        {
            if (this.Channel != null)
                this.Channel.Close();

            if (this.AppInfo != null)
            {
                AppInfo.Save();
                AppInfo = null;	// 避免后面再用这个对象
            }
        }

        string LibraryServerUrl
        {
            get
            {
                return AppInfo.GetString(
    "server",
    "url",
    "http://localhost:8001/dp2library");
            }
            set
            {
                AppInfo.SetString(
"server",
"url",
value);
            }
        }

        string _userName = "";
        string _password = "";

        /// <summary>
        /// 配置存储
        /// </summary>
        public ApplicationInfo AppInfo = null;

        /// <summary>
        /// 用户目录
        /// </summary>
        public string UserDir = "";

        // parameters:
        //      strUrl  服务器 URL。如果为空，则表示沿用 this.LibraryServerUrl 当前的值
        // return:
        //      -1  出错
        //      0   成功
        public int Login(string strUrl, 
            string strUserName, 
            string strPassword,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strUrl) == false
                || strUrl == ".")
                this.LibraryServerUrl = strUrl;
            this._userName = strUserName;
            this._password = strPassword;

            this.Channel.Close();
            this.Channel = new LibraryChannel();
            this.Channel.Url = this.LibraryServerUrl;
            this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);

            return 0;
        }

        internal void Channel_BeforeLogin(object sender,
    DigitalPlatform.CirculationClient.BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                e.UserName = this._userName;
                e.Password = this._password;

                bool bIsReader = false;

                string strLocation = "console";
                e.Parameters = "location=" + strLocation;
                if (bIsReader == true)
                    e.Parameters += ",type=reader";

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            e.Cancel = true;
        }

        public int Dir(string strCurrentDir, 
            out List<string> filenames,
            out string strError)
        {
            strError = "";

            filenames = new List<string>();

            if (string.IsNullOrEmpty(this._userName) == true)
            {
                strError = "尚未登录";
                return -1;
            }

            string strValue = "";
            long lRet = this.Channel.GetSystemParameter(null,
                "listUploadFileNames",
                strCurrentDir,
                out strValue,
                out strError);
            if (lRet == -1)
                return -1;

            string[] values = strValue.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
            filenames.AddRange(values);
            return 0;
        }

        // 把子目录中的文件压缩到一个 .zip 文件中
        // parameters:
        //      strReportDir    最后不要带有符号 '/'
        // return:
        //      -1  出错
        //      0   没有发现需要上传的文件
        //      1   成功压缩创建了 .zip 文件
        int CompressDirecotry(
            string strReportDir,
            string strZipFileName,
            Encoding encoding,
            out string strError)
        {
            strError = "";

            List<string> filenames = null;

            filenames = GetFileNames(strReportDir);

            if (filenames.Count == 0)
                return 0;

            int nCursorLeft = Console.CursorLeft;
            int nCursorTop = Console.CursorTop;

            using (ZipFile zip = new ZipFile(encoding))
            {
                foreach (string filename in filenames)
                {
                    string strShortFileName = filename.Substring(strReportDir.Length + 1);
                    ProgressMessage(nCursorLeft, nCursorTop, "compressing " + strShortFileName);

                    string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                    zip.AddFile(filename, directoryPathInArchive);
                }

                zip.SaveProgress += (s, e) =>
                {
                    if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                    {
                        ProgressMessage(nCursorLeft, nCursorTop, "saved " + e.EntriesSaved);
                    }
                };

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save(strZipFileName);
            }

            ProgressMessage(nCursorLeft, nCursorTop, "");
            return 1;
        }

        // 获得一个目录下的全部文件名。包括子目录中的
        static List<string> GetFileNames(string strDataDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDataDir);

            List<string> result = new List<string>();

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                result.Add(fi.FullName);
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                result.AddRange(GetFileNames(subdir.FullName));
            }

            return result;
        }

        // 曾经显示过的动态 message
        string _message = "";

        void ProgressMessage(int nCursorLeft, int nCursorTop, string strText)
        {
            // 擦除上次显示的内容
            if (string.IsNullOrEmpty(this._message) == false)
            {
                Console.SetCursorPosition(nCursorLeft, nCursorTop);
                Console.Write(new string(' ', this._message.Length));
            }
            // 显示本次文字
            Console.SetCursorPosition(nCursorLeft, nCursorTop);
            Console.Write(strText);
            this._message = strText;
        }

        // 上传文件到到 dp2lbrary 服务器
        // parameters:
        //      timestamp   时间戳。如果为 null，函数会自动根据文件信息得到一个时间戳
        //      bRetryOverwiteExisting   是否自动在时间戳不一致的情况下覆盖已经存在的服务器文件。== true，表示当发现时间戳不一致的时候，自动用返回的时间戳重试覆盖
        // return:
        //		-1	出错
        //		0   上传文件成功
        int UploadFile(
            Stop stop,
            LibraryChannel channel,
            string strClientFilePath,
            string strServerFilePath,
            string strStyle,
            byte[] timestamp,
            bool bRetryOverwiteExisting,
            out string strError)
        {
            strError = "";

            string strResPath = strServerFilePath;

            string strMime = API.MimeTypeFrom(ResObjectDlg.ReadFirst256Bytes(strClientFilePath),
"");

            // 检测文件尺寸
            FileInfo fi = new FileInfo(strClientFilePath);
            if (fi.Exists == false)
            {
                strError = "文件 '" + strClientFilePath + "' 不存在...";
                return -1;
            }

            string[] ranges = null;

            if (fi.Length == 0)
            {
                // 空文件
                ranges = new string[1];
                ranges[0] = "";
            }
            else
            {
                string strRange = "";
                strRange = "0-" + Convert.ToString(fi.Length - 1);

                // 按照100K作为一个chunk
                // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                ranges = RangeList.ChunkRange(strRange,
                    500 * 1024);
            }

            if (timestamp == null)
                timestamp = FileUtil.GetFileTimestamp(strClientFilePath);

            byte[] output_timestamp = null;

            // REDOWHOLESAVE:
            string strWarning = "";

            TimeSpan old_timeout = channel.Timeout;

            int nCursorLeft = Console.CursorLeft;
            int nCursorTop = Console.CursorTop;

            try
            {
                for (int j = 0; j < ranges.Length; j++)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strWaiting = "";
                    if (j == ranges.Length - 1)
                    {
                        strWaiting = " please wait ...";
                        channel.Timeout = new TimeSpan(0, 40, 0);   // 40 分钟
                    }

                    string strPercent = "";
                    RangeList rl = new RangeList(ranges[j]);
                    if (rl.Count >= 1)
                    {
                        double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

#if NO
                    if (stop != null)
                        stop.SetMessage( // strMessagePrefix + 
                            "正在上载 " + ranges[j] + "/"
                            + Convert.ToString(fi.Length)
                            + " " + strPercent + " " + strClientFilePath + strWarning + strWaiting);
#endif
                    ProgressMessage(nCursorLeft, nCursorTop,
                        "uploading "
                        // + ranges[j] + "/"  + Convert.ToString(fi.Length)
                        + " " + strPercent + " " 
                        // + strClientFilePath
                        + strWarning + strWaiting);
                    int nRedoCount = 0;
                REDO:
                    long lRet = channel.SaveResObject(
                        stop,
                        strResPath,
                        strClientFilePath,
                        strClientFilePath,
                        strMime,
                        ranges[j],
                        // j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                        timestamp,
                        strStyle,
                        out output_timestamp,
                        out strError);
                    timestamp = output_timestamp;

                    strWarning = "";

                    if (lRet == -1)
                    {
                        // 如果是第一个 chunk，自动用返回的时间戳重试一次覆盖
                        if (bRetryOverwiteExisting == true
                            && j == 0
                            && channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.TimestampMismatch
                            && nRedoCount == 0)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                channel.Timeout = old_timeout;

                ProgressMessage(nCursorLeft, nCursorTop, "");
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 删除一个远程文件或者目录
        static int DeleteRemoteFile(
            Stop stop,
            LibraryChannel channel,
            string strServerFilePath,
            out string strError)
        {
            strError = "";

            string strOutputResPath = "";
            byte[] baOutputTimestamp = null;
                    // 写入资源
            long lRet = channel.WriteRes(
                stop,
                strServerFilePath,
                "",
                0,
                null,
                "",
                "delete",
                null,
                out strOutputResPath,
                out baOutputTimestamp,
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }
    }
}
