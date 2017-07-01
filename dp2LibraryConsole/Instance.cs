using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Ionic.Zip;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Range;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using System.Globalization;

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
        public DigitalPlatform.Stop Progress = null;

        /// <summary>
        /// 界面语言代码
        /// </summary>
        public string Lang = "zh";

        public bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                    // 中断长操作，但不退出程序
                    this.Stop();
                    return true;
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    {
                        // Debug.WriteLine("close ...");
                        Console.WriteLine(" 正在退出 ...");
                        Close();
                    }
                    return true;
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Instance()
        {
            if (string.IsNullOrEmpty(this.UserDir) == true)
            {
                this.UserDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "dp2LibraryConsole_v1");
            }
            PathUtil.TryCreateDir(this.UserDir);

            this.AppInfo = new ApplicationInfo(Path.Combine(this.UserDir, "settings.xml"));

            this._currentLocalDir = Directory.GetCurrentDirectory();

#if NO
            this.Channel.Url = this.LibraryServerUrl;

            this.Channel.BeforeLogin -= new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new DigitalPlatform.CirculationClient.BeforeLoginEventHandle(Channel_BeforeLogin);
#endif
            this.PrepareChannel();
        }

        // 中断长操作
        public void Stop()
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        // 关闭资源
        public void Close()
        {
            this.DestoryChannel();

            if (this.AppInfo != null)
            {
                AppInfo.Save();
                AppInfo = null;	// 避免后面再用这个对象
            }
        }

        // 显示命令行提示符
        public void DisplayPrompt()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            Console.WriteLine();
            string strLocal = "本地 " + this._currentLocalDir;
            Console.Write(new string('*', strLocal.Length) + "\r\n远程 " + this._currentDir + "\r\n" + strLocal + "\r\n> ");

            Console.ResetColor();
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
            if (parameters.Count == 0)
                return false;

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
                    Console.WriteLine("登录失败: " + strError);
                    return false;
                }

                Console.WriteLine("登录成功");
                return false;
            }

            if (parameters[0] == "dir")
            {
                // 测试 ..
                // *.*
                // ..\
                // ..\*.*
                // \
                // \*.*

                string strDir = "";
                if (parameters.Count > 1)
                    strDir = parameters[1];

                FileSystemLoader loader = new FileSystemLoader(this._currentLocalDir, strDir);

                int file_count = 0;
                int dir_count = 0;
                foreach (FileSystemInfo si in loader)
                {
                    // TODO: 是否先得到 FullName ，再根据起点目录截断显示后部
                    if (si is DirectoryInfo)
                    {
                        AlignWrite(si.LastWriteTime.ToString("s").Replace("T", " ") + " <dir>      ");
                        Console.WriteLine(si.Name + "/");
                        dir_count++;
                    }

                    if (si is FileInfo)
                    {
                        FileInfo info = si as FileInfo;
                        AlignWrite(info.LastWriteTime.ToString("s").Replace("T", " ") + info.Length.ToString().PadLeft(10, ' ') + "  ");
                        Console.WriteLine(info.Name);
                        file_count++;
                    }
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

                    string strParam = GetCdParmeter(parameters);

                    if (strParam == ".")
                    {
                        // 当前路径不变
                        return false;
                    }
                    else
                    {
#if NO
                        strDir = Path.Combine(this._currentLocalDir, strParam);
                        // 上一句执行完，有可能是这样 strDir = '\publish\dp2libraryxe'，没有盘符
#endif
                        FileSystemLoader.ChangeDirectory(this._currentLocalDir, strParam, out strDir);
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
                List<FileItemInfo> filenames = null;

                string strDir = "";
                if (parameters.Count > 1)
                    strDir = parameters[1];

                nRet = RemoteList(
                    GetRemoteCurrentDir(),  // "!upload/" + this._currentDir,
                    strDir,
                    out filenames,
                    out strError);
                if (nRet == -1)
                {
                    Console.WriteLine("Dir error: " + strError);
                    return false;
                }

                string strDisplayDirectory = "";

                int file_count = 0;
                int dir_count = 0;
                foreach (FileItemInfo info in filenames)
                {
#if NO
                    string strName = "";
                    string strTime = "";
                    string strSize = "";
                    
                    ParseFileName(filename,
            out strName,
            out strTime,
            out strSize);
#endif
                    string strCurrent = Path.GetDirectoryName(info.Name);
                    if (strCurrent != strDisplayDirectory)
                    {
                        Console.WriteLine();
                        Console.WriteLine("远程 " + strCurrent + " 的目录:");
                        Console.WriteLine();

                        strDisplayDirectory = strCurrent;
                    }

                    string strName = "";

                    if (strDisplayDirectory != null)
                    {
                        strName = info.Name.Substring(strDisplayDirectory.Length);
                        if (String.IsNullOrEmpty(strName) == false && strName[0] == '\\')
                            strName = strName.Substring(1);
                    }
                    else
                        strName = info.Name;

                    // TODO: 如何显示文件名是个问题。建议，处在当前目录的，只显示纯文件名；越过当前目录的，显示全路径
                    // 也可以一段一段显示。某一段的事项都是处于同一目录，就在前导显示一句目录名，后面就显示纯文件名
                    // 两种风格都可以实现了看看
                    if (info.Size == -1)
                    {
                        dir_count++;
                        AlignWrite(GetLocalTime(GetLastWriteTime(info)) + " <dir>      ");
                        Console.WriteLine(strName);
                    }
                    else
                    {
                        file_count++;
                        AlignWrite(GetLocalTime(GetLastWriteTime(info)) + info.Size.ToString().PadLeft(10, ' ') + "  ");
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

#if NO
                string strDir = "";
                if (parameters.Count > 1)
                    strDir = parameters[1];
#endif

                string strDir = GetCdParmeter(parameters);

                string strResultDirectory = "";

                // 进行远程 CD 命令
                // parameters:
                //      strCurrentDirectory 当前路径
                //      strPath 试图转去的路径
                //      strResultDirectory  返回成功转过去的结果路径。需把这个路径设为最新的当前路径
                nRet = RemoteChangeDir(
                    GetRemoteCurrentDir(),  // "!upload" + this._currentDir,
                    strDir,
                    out strResultDirectory,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 检查这个远程目录是否存在

                // 用 "/" rdir， strResultDirectory 为 "upload/"

                // this._currentDir = strResultDirectory;
                this._currentDir = strResultDirectory.Substring("upload".Length);   // 2017/3/20

                Debug.Assert(string.IsNullOrEmpty(this._currentDir) == true || this._currentDir.IndexOf("\\") == -1,
                    "this._currentDir 中不允许使用字符 '\\'");
#if DEBUG
                if (string.IsNullOrEmpty(this._currentDir) == false)
                {
                    Debug.Assert(this._currentDir[0] == '\\' || this._currentDir[0] == '/', "远程逻辑路径如果为非空，则第一字符应该是斜杠。但现在是 '" + this._currentDir + "'");
                }
#endif

#if NO
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
#endif

                return false;
            }

            if (parameters[0] == "upload")
            {
                if (parameters.Count < 2)
                {
                    Console.WriteLine("upload 命令用法: upload 源文件或目录 [目标文件或目录]");
                    // 如果目标目录省略，则表示和源目录同名
                    return false;
                }

                string strSource = parameters[1];   // strSource 可能为 "*.*" 这样的模式
                string strTarget = "";  // strTarget 可能空缺

                if (parameters.Count > 2)
                    strTarget = parameters[2];

                nRet = DoUpload(strSource,
            strTarget,
            out strError);
                if (nRet == -1)
                    goto ERROR1;
                return false;
            }

            if (parameters[0] == "rdel" || parameters[0] == "rdelete")
            {
                if (parameters.Count != 2)
                {
                    Console.WriteLine("rdel 命令用法: rdel 远程目录");
                    return false;
                }

                string strDir = "";
                if (parameters.Count > 1)
                    strDir = parameters[1];

                // 删除一个远程文件或者目录
                nRet = RemoteDelete(
                    GetRemoteCurrentDir(),
                    strDir,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                Console.WriteLine("已删除远程文件 " + nRet.ToString() + " 个");
                return false;
            }

            if (parameters[0] == "download")
            {
                if (parameters.Count < 2)
                {
                    Console.WriteLine("download 命令用法: download 源文件或目录 [目标文件或目录]");
                    // 如果目标目录省略，则表示和源目录同名
                    return false;
                }

                string strSource = parameters[1];   // strSource 可能为 "*.*" 这样的模式
                string strTarget = "";  // strTarget 可能空缺

                if (parameters.Count > 2)
                    strTarget = parameters[2];

                nRet = DoDownload(strSource,
strTarget,
out strError);
                if (nRet == -1)
                    goto ERROR1;
                return false;
            }

            Console.WriteLine("unknown command '" + line + "'");
            return false;
        ERROR1:
            Console.WriteLine(strError);
            return false;
        }

        // 进行文件或目录下载
        // return:
        //      -1  出错
        //      0   正常
        int DoDownload(string strSource,
            string strTargetParam,
            out string strError)
        {
            strError = "";

            string strLocalDir = "";
            FileSystemLoader.ChangeDirectory(this._currentLocalDir, strTargetParam, out strLocalDir);
            strLocalDir = (new DirectoryInfo(strLocalDir)).FullName;

            List<FileItemInfo> filenames = null;

            int nRet = RemoteList(
                GetRemoteCurrentDir(),  // "!upload/" + this._currentDir,
                strSource,
                out filenames,
                out strError);
            if (nRet == -1)
                return -1;

            if (filenames == null || filenames.Count == 0)
            {
                strError = strSource + " 没有找到";
                return -1;
            }

            // return:
            //      -1  出错
            //      其他  下载成功的文件数
            nRet = DownloadFiles(
                GetRemoteCurrentDir().Substring(1),  // "", 
                filenames,
                strLocalDir,
                out strError);
            if (nRet == -1)
                return -1;

            Console.WriteLine("共下载文件 " + nRet + " 个");

            return 0;
        }

        string GetRemoteCurrentDir()
        {
#if DEBUG
            if (string.IsNullOrEmpty(this._currentDir) == false)
            {
                Debug.Assert(this._currentDir[0] == '\\' || this._currentDir[0] == '/', "远程逻辑路径如果为非空，则第一字符应该是斜杠。但现在是 '" + this._currentDir + "'");
            }
#endif
            return "!upload" + this._currentDir.Replace("\\", "/"); // 调用文件相关 API 的时候，逻辑路径要求斜杠字符为 '/'
        }

#if NO
        static string GetRemoteDir(string strRelative)
        {
            return "upload" + strRelative.Replace("\\", "/"); // 调用文件相关 API 的时候，逻辑路径要求斜杠字符为 '/'
        }
#endif

        // 获得 cd 命令第二个以及以后的参数
        // 能够把 cd program files 解析出 'program files'
        static string GetCdParmeter(List<string> parameters)
        {
            string strResult = "";
            for (int i = 1; i < parameters.Count; i++)
            {
                if (i > 1)
                    strResult += " ";
                strResult += parameters[i];
            }

            return strResult;
        }

        // 进行文件或目录上载
        // return:
        //      -1  出错
        //      0   正常
        int DoUpload(string strSource,
            string strTargetParam,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            FileSystemLoader loader = new FileSystemLoader(this._currentLocalDir, strSource);
            loader.ListStyle = ListStyle.None;  // 对单一无匹配模式的目录对象不展开其下级

            foreach (FileSystemInfo si in loader)
            {
                // string strSourcePath = GetLocalFullDirectory(strSource);
                string strSourcePath = si.FullName;

                string strTarget = strTargetParam;
                if (string.IsNullOrEmpty(strTargetParam) == true)
                    strTarget = si.Name;

                if (si is DirectoryInfo)
                {
                    string strServerFilePath = "!upload/" + GetFullDirectory(strTarget) + "/~" + Guid.NewGuid().ToString();
                    string strZipFileName = Path.GetTempFileName(); // "c:\\temp\\test.zip"; // TODO: 建议在当前目录创建临时文件，便于观察是否有删除遗漏，和处理
                    File.Delete(strZipFileName);
                    try
                    {
                        // return:
                        //      -1  出错
                        //      0   没有发现需要上传的文件
                        //      1   成功压缩创建了 .zip 文件
                        nRet = CompressDirectory(
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
                        Console.WriteLine("本地目录 " + si.FullName + " 成功上传到远程 " + GetFullDirectory(strTarget));
                        continue;
                    }
                    finally
                    {
                        File.Delete(strZipFileName);
                    }
                }
                else if (si is FileInfo)
                {
                    string strServerFilePath = "!upload/" + GetFullDirectory(strTarget);

#if NO
                    {
                        DateTime time = File.GetLastWriteTime(strSourcePath);
                        string strTemp = ByteArray.GetHexTimeStampString(BitConverter.GetBytes(time.Ticks));

                        byte [] baTimeStamp = ByteArray.GetTimeStampByteArray(strTemp);
                        long lTicks = BitConverter.ToInt64(baTimeStamp, 0);


                        DateTime time1 = new DateTime(lTicks);
                    }
#endif

                    // return:
                    //		-1	出错
                    //		0   上传文件成功
                    nRet = UploadFile(
                null,
                this.Channel,
                strSourcePath,
                strServerFilePath,
                "last_write_time:" + ByteArray.GetHexTimeStampString(BitConverter.GetBytes((long)File.GetLastWriteTimeUtc(strSourcePath).Ticks)),
                null,
                true,
                out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    Console.WriteLine();
                    Console.WriteLine("本地文件 " + si.FullName + " 成功上传到远程 " + GetFullDirectory(strTarget));
                    continue;
                }
            }
            return 0;
        ERROR1:
            return -1;
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
            string[] parts = strText.Split(new char[] { '|' });
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
            // string[] parameters = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // List<string> result = new List<string>(parameters);

            List<string> result0 = StringUtil.SplitString(line,
                " ",
                new string[] { "''" },
                StringSplitOptions.RemoveEmptyEntries);

            List<string> result1 = new List<string>();
            foreach (string s in result0)
            {
                result1.Add(UnQuote(s));
            }

            // 对第一个元素修正一下。从左面开始，如果出现第一个标点符号，就认为这里应该断开
            if (result1.Count > 0)
            {
                string strText = result1[0];
                int index = strText.IndexOfAny(new char[] { '.', '/', '\\' });
                if (index != -1)
                {
                    result1[0] = strText.Substring(0, index);
                    result1.Insert(1, strText.Substring(index));
                }
            }

            return result1;
        }

        static string UnQuote(string strText)
        {
            return strText.Replace("'", "");
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
            Console.BackgroundColor = Console.ForegroundColor;
            // Console.ForegroundColor = ConsoleColor.Black;
            strPassword = Console.ReadLine();
            Console.ResetColor();
            return false;
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
#if NO
                    if (this.Channel != null)
                    {
                        this.Channel.Close();
                        this.Channel = null;
                    }
#endif
                    this.DestoryChannel();

                    if (this.AppInfo != null)
                    {
                        AppInfo.Save();
                        AppInfo = null;	// 避免后面再用这个对象
                    }
                }

                /*
                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;            
                */
            }
            disposed = true;
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

        void DestoryChannel()
        {
            if (this.Channel != null)
            {
                this.Channel.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
                this.Channel.Idle -= Channel_Idle;

                this.Channel.Close();
                this.Channel = null;
            }
        }

        void PrepareChannel()
        {
            this.DestoryChannel();

            this.Channel = new LibraryChannel();
            this.Channel.Url = this.LibraryServerUrl;

            this.Channel.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.Idle += Channel_Idle;
        }

        void EnableCharAnimation(bool bEnable)
        {
            if (bEnable == true)
            {
                _index = 0;
                Console.Write(" ");
            }
            else
            {
                _index = -1;
                Console.Write("\b ");
            }
        }

        int _index = -1; // -1 表示不进行字符动画
        char[] movingChars = new char[] { '/', '-', '\\', '|' };

        void Channel_Idle(object sender, IdleEventArgs e)
        {
            // e.bDoEvents = false;

            if (_index != -1)
            {
                Console.Write("\b");
                Console.Write(new string(movingChars[_index], 1));
                _index++;
                if (_index > 3)
                    _index = 0;

                System.Threading.Thread.Sleep(500);	// 确保动画显示效果
            }
        }

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
                && strUrl != ".")
                this.LibraryServerUrl = strUrl;
            this._userName = strUserName;
            this._password = strPassword;

            // 
            this.PrepareChannel();

            this._currentDir = "";  // 远程当前目录复位

            EnableCharAnimation(true);
            try
            {
                long lRet = this.Channel.IdleLogin(this._userName,
                    this._password,
                    "location=console,client=dp2LibraryConsole|0.01",
                    out strError);
                if (lRet == -1 || lRet == 0)
                    return -1;
            }
            finally
            {
                EnableCharAnimation(false);
            }

            return 0;
        }

        internal void Channel_BeforeLogin(object sender,
    DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
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

                e.Parameters += ",client=dp2libraryconsole|0.01";

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
            }

            e.Cancel = true;
        }

#if NO
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

#endif
        // 进行远程 CD 命令
        // parameters:
        //      strCurrentDirectory 当前路径
        //      strPath 试图转去的路径
        //      strResultDirectory  返回成功转过去的结果路径。需把这个路径设为最新的当前路径
        public int RemoteChangeDir(
            string strCurrentDirectory,
            string strPattern,
            out string strResultDirectory,
            out string strError)
        {
            strError = "";
            strResultDirectory = "";

            FileItemInfo[] infos = null;

            long lRet = this.Channel.ListFile(null,
    "cd",
    strCurrentDirectory,
    strPattern,
    0,
    -1,
    out infos,
    out strError);
            if (lRet == -1)
                return -1;

            if (infos != null && infos.Length > 0)
                strResultDirectory = infos[0].Name;

            return 0;
        }

        public int RemoteList(
            string strCurrentDirectory,
            string strPattern,
            out List<FileItemInfo> fileNames,
            out string strError)
        {
            strError = "";

            fileNames = new List<FileItemInfo>();

            if (string.IsNullOrEmpty(this._userName) == true)
            {
                strError = "尚未登录";
                return -1;
            }

            EnableCharAnimation(true);

            try
            {
                FileItemInfo[] infos = null;

                long lStart = 0;
                for (; ; )
                {
                    long lRet = this.Channel.ListFile(null,
                        "list",
                        strCurrentDirectory,
                        strPattern,
                        lStart,
                        -1,
                        out infos,
                        out strError);
                    if (lRet == -1)
                        return -1;

                    Debug.Assert(infos != null, "");

                    fileNames.AddRange(infos);

                    lStart += infos.Length;
                    if (lStart >= lRet)
                        break;
                }
            }
            finally
            {
                this.EnableCharAnimation(false);
            }

            return 0;
        }

        public int RemoteDelete(
    string strCurrentDirectory,
    string strPattern,
    out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(this._userName) == true)
            {
                strError = "尚未登录";
                return -1;
            }

            EnableCharAnimation(true);

            try
            {

                FileItemInfo[] infos = null;

                return (int)this.Channel.ListFile(null,
                    "delete",
                    strCurrentDirectory,
                    strPattern,
                    0,
                    -1,
                    out infos,
                    out strError);
            }
            finally
            {
                EnableCharAnimation(false);
            }
        }


        // 把子目录中的文件压缩到一个 .zip 文件中
        // parameters:
        //      strDataDir    要压缩的数据目录的全路径。最后不要带有符号 '/'
        // return:
        //      -1  出错
        //      0   没有发现需要上传的文件
        //      1   成功压缩创建了 .zip 文件
        int CompressDirectory(
            string strDataDir,
            string strZipFileName,
            Encoding encoding,
            out string strError)
        {
            strError = "";

            List<string> filenames = null;

            filenames = PathUtil.GetFileNames(strDataDir);

            if (filenames.Count == 0)
                return 0;

            int nCursorLeft = Console.CursorLeft;
            int nCursorTop = Console.CursorTop;

            using (ZipFile zip = new ZipFile(encoding))
            {
                // http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                // https://dotnetzip.codeplex.com/workitem/14087
                // uncommenting the following line can be used as a work-around
                zip.ParallelDeflateThreshold = -1;

                foreach (string filename in filenames)
                {
                    string strShortFileName = filename.Substring(strDataDir.Length + 1);
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

#if NO
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
#endif

        // 曾经显示过的动态 message
        string _message = "";

        // 计算一个字符串的“西文字符宽度”。汉字相当于两个西文字符宽度
        static int GetCharWidth(string strText)
        {
            int result = 0;
            foreach (char c in strText)
            {
                result += StringUtil.IsHanzi(c) == true ? 2 : 1;
            }

            return result;
        }

        void ProgressMessage(int nCursorLeft, int nCursorTop, string strText)
        {
            // 擦除上次显示的内容
            if (string.IsNullOrEmpty(this._message) == false)
            {
                Console.SetCursorPosition(nCursorLeft, nCursorTop);
                Console.Write(new string(' ', GetCharWidth(this._message)));
            }
            // 显示本次文字
            Console.SetCursorPosition(nCursorLeft, nCursorTop);
            Console.Write(strText);
            this._message = strText;
        }

        static string GetSizeString(long size)
        {
            long unit = 1024 * 1024 * 1024;
            if (size >= unit)
                return ((double)size / (double)unit).ToString("f1") + "G";
            unit = 1024 * 1024;
            if (size >= unit)
                return ((double)size / (double)unit).ToString("f1") + "M";
            unit = 1024;
            if (size >= unit)
                return ((double)size / (double)unit).ToString("f1") + "K";

            return size.ToString();
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

#if NO
            string strMime = API.MimeTypeFrom(ResObjectDlg.ReadFirst256Bytes(strClientFilePath),
"");
#endif
            string strMime = PathUtil.MimeTypeFrom(strClientFilePath);

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
                strRange = "0-" + (fi.Length - 1).ToString();

                // 按照100K作为一个chunk
                // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                ranges = RangeList.ChunkRange(strRange,
                    channel.UploadResChunkSize // 500 * 1024
                    );
            }

            if (timestamp == null)
                timestamp = FileUtil.GetFileTimestamp(strClientFilePath);

            byte[] output_timestamp = null;

            // REDOWHOLESAVE:
            string strWarning = "";

            bool bCharRedo = false; // 是否正在击键重做的过程中

            TimeSpan old_timeout = channel.Timeout;

            channel.Timeout = TimeSpan.FromSeconds(10);

            int nCursorLeft = Console.CursorLeft;
            int nCursorTop = Console.CursorTop;

            ProgressEstimate _estimate = new ProgressEstimate();

            _estimate.SetRange(0, fi.Length);
            _estimate.StartEstimate();

            string strTotalSize = GetSizeString(fi.Length);

            try
            {
                for (int j = 0; j < ranges.Length; j++)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    RangeList rl = new RangeList(ranges[j]);
                    long uploaded = ((RangeItem)rl[0]).lStart;

                    string strPercent = "";
                    if (rl.Count >= 1)
                    {
                        double ratio = (double)uploaded / (double)fi.Length;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    string strUploadedSize = GetSizeString(uploaded);


                    string strWaiting = "";
                    if (j == ranges.Length - 1)
                    {
                        strWaiting = " please wait ...";
                        channel.Timeout = new TimeSpan(0, 40, 0);   // 40 分钟
                    }
                    else if (j > 0)
                        strWaiting = "剩余时间 " + ProgressEstimate.Format(_estimate.Estimate(uploaded)) + " 已经过时间 " + ProgressEstimate.Format(_estimate.delta_passed);

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
                        + strUploadedSize + "/" + strTotalSize + " "
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
                            && channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch
                            && nRedoCount == 0)
                        {
                            nRedoCount++;
                            goto REDO;
                        }

                        if (channel.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCode.TimestampMismatch
                            && bCharRedo == true)
                        {
                            bCharRedo = false;
                            goto REDO;
                        }

                        Console.WriteLine("出错: " + strError + "\r\n\r\n是否重试? (Y/N)");
                        ConsoleKeyInfo info = Console.ReadKey();
                        if (info.KeyChar == 'y' || info.KeyChar == 'Y')
                        {
                            Console.WriteLine();
                            nCursorLeft = Console.CursorLeft;
                            nCursorTop = Console.CursorTop + 2;
                            bCharRedo = true;
                            goto REDO;
                        }
                        goto ERROR1;
                    }

                    bCharRedo = false;
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

        // 从 dp2library 服务器下载一个文件
        // return:
        //		-1	出错
        //		0   下载文件成功
        static int DownloadFile(
            Stop stop,
            LibraryChannel channel,
            string strServerFilePath,
            string strClientFilePath,
            out string strError)
        {
            strError = "";

            string strMetaData = "";
            byte[] baOutputTimeStamp = null;
            string strOutputPath = "";
            // parameters:
            //		strOutputFileName	输出文件名。可以为null。如果调用前文件已经存在, 会被覆盖。
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            long lRet = channel.GetRes(
                stop,
                strServerFilePath,
                strClientFilePath,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }

        // return:
        //      -1  出错
        //      其他  下载成功的文件数
        int DownloadFiles(
            string strRemoteBase,
            List<FileItemInfo> filenames,
            string strTargetDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            int nCount = 0;
            foreach (FileItemInfo info in filenames)
            {
                string strDelta = "";
                if (string.IsNullOrEmpty(strRemoteBase) == true)
                {
                    strDelta = Path.GetFileName(info.Name);
                    strRemoteBase = Path.GetDirectoryName(info.Name);
                }
                else
                {
                    if (string.IsNullOrEmpty(strRemoteBase) == false && strRemoteBase[strRemoteBase.Length - 1] == '/')
                        strDelta = info.Name.Substring(strRemoteBase.Length);
                    else
                        strDelta = info.Name.Substring(strRemoteBase.Length + 1);
                }

                if (info.Size != -1)
                {
                    string strLocalPath = Path.Combine(strTargetDir, strDelta);
                    PathUtil.TryCreateDir(Path.GetDirectoryName(strLocalPath));

                    // Console.WriteLine(info.Name);
                    Console.WriteLine(strLocalPath);
                    // TODO: info.Name 中的斜杠应该为 /，以减少转换的麻烦

                    // 从 dp2library 服务器下载一个文件
                    // return:
                    //		-1	出错
                    //		0   下载文件成功
                    nRet = DownloadFile(
            null,
            this.Channel,
                        // "!upload" + info.Name.Replace("\\", "/"),
            "!" + info.Name.Replace("\\", "/"), // 2017/4/8
            strLocalPath,
            out strError);
                    if (nRet == -1)
                        return -1;

                    SetFileTime(info, strLocalPath);

                    nCount++;
                }
                else
                {
                    List<FileItemInfo> filenames1 = null;

                    nRet = RemoteList(
    "!", // GetRemoteCurrentDir(), // "!upload", // "!upload/" + this._currentDir,
    info.Name.Replace("\\", "/"), // info.Name.Substring(1).Replace("\\", "/"),
    out filenames1,
    out strError);
                    if (nRet == -1)
                        return -1;

                    nRet = DownloadFiles(
                        Path.Combine(strRemoteBase, strDelta),
                        filenames1,
                        Path.Combine(strTargetDir, strDelta),
                        out strError);
                    if (nRet == -1)
                        return -1;
                    nCount += nRet;
                }
            }

            return nCount;
        }

        // 将 U 格式的时间字符串转换为 g 格式的时间字符串(并用空格替换里面的 T)
        // parameters:
        //      strUniversalTime    u 格式的时间字符串。GMT 时间值
        static string GetLocalTime(string strUniversalTime)
        {
            DateTime time;
            if (string.IsNullOrEmpty(strUniversalTime) == false
                && DateTime.TryParseExact(strUniversalTime,
                "u",
                CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out time) == true)
            {
                return time.ToLocalTime().ToString("s").Replace("T", " ");
            }

            return strUniversalTime;
        }

        static string GetLastWriteTime(FileItemInfo info)
        {
            if (string.IsNullOrEmpty(info.LastWriteTime) == false)
                return info.LastWriteTime;
            return info.CreateTime;
        }

        static void SetFileTime(FileItemInfo info, string strLocalPath)
        {
            DateTime time;
            if (string.IsNullOrEmpty(info.CreateTime) == false
                && DateTime.TryParseExact(info.CreateTime,
                "u",
                CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out time) == true)
            {
                File.SetCreationTimeUtc(strLocalPath, time);
            }

            if (string.IsNullOrEmpty(info.LastWriteTime) == false
                && DateTime.TryParseExact(info.LastWriteTime,
"u",
CultureInfo.InvariantCulture,
System.Globalization.DateTimeStyles.None,
out time) == true)
            {
                File.SetLastWriteTimeUtc(strLocalPath, time);
            }
        }
#if NO
        // 删除一个远程文件或者目录
        // return:
        //      -1  出错
        //      其他  删除的文件或者目录个数
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

            return (int)lRet;
        }

#endif
    }
}
