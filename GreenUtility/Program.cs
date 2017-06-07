using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

using Ionic.Zip;

/*
 * 制作和解压绿色更新安装包的实用工具程序
 * 用法： greenutility -action:xxx -source:xxxxx -target:xxxxx -wait:xxxx -files:xxxx
 * 源目录内应该有 *.csproj 文件；打包后形成的 .zip 文件会被放到目标目录内
 * action 可用值有 build install
 * wait 是一个 .exe 的纯文件名，例如 dp2circulation.exe
 * files 要安装的 .zip 文件名。纯文件名，逗号间隔的字符串
 * */
namespace GreenUtility
{
    class Program
    {
        static string TempDir = "";

        static void Main(string[] args)
        {
            string strError = "";
            int nRet = 0;

            // Debug.Assert(false, "");

            string strAction = "";
            string strSourceDir = "";
            string strTargetDir = "";
            string strWaitExe = ""; // 要等待这个 .exe 进程退出才能进行安装操作
            string strFiles = "";   // 安装那些 .zip 文件。纯文件名列表

            foreach(string arg in args)
            {
                if (arg.StartsWith("-source:") == true)
                    strSourceDir = arg.Substring("-source:".Length);
                if (arg.StartsWith("-target:") == true)
                    strTargetDir = arg.Substring("-target:".Length);
                if (arg.StartsWith("-action:") == true)
                    strAction = arg.Substring("-action:".Length).ToLower();
                if (arg.StartsWith("-wait:") == true)
                    strWaitExe = arg.Substring("-wait:".Length).ToLower();
                if (arg.StartsWith("-files:") == true)
                    strFiles = arg.Substring("-files:".Length).ToLower();
            }

            if (string.IsNullOrEmpty(strSourceDir) == true)
            {
                strError = "缺乏 -source:xxxx 参数";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strTargetDir) == true)
            {
                strError = "缺乏 -target:xxxx 参数";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strFiles) == true)
                strFiles = "app.zip,data.zip";  // 缺省值

            string strTempDir = "c:\\~dp2circulation_temp_file";
            CreateDirIfNeed(strTempDir);
            TempDir = strTempDir;

            if (strAction == "build")
            {
                string strProjectFileName = GetProjectFileName(strSourceDir);
                if (string.IsNullOrEmpty(strProjectFileName) == true)
                {
                    strError = "源目录 '" + strSourceDir + "' 中没有任何 .csproj 文件";
                    goto ERROR1;
                }

                nRet = CreateZipFile(strProjectFileName,
            strTargetDir,
            out strError);
                if (nRet == -1)
                    goto ERROR1;

                Console.WriteLine("创建成功");
                return;
            }

            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);


            if (strAction == "install")
            {
                Console.WriteLine("*** GreenUtility.exe 绿色安装工具 ***");

                if (string.IsNullOrEmpty(strWaitExe) == false)
                {
                    Console.WriteLine("等待 " + strWaitExe + " 结束 ...");

                    bool bRet = WaitProcessEnd(strWaitExe, new TimeSpan(0, 0, 10));
                    if (bRet == false)
                    {
                        if (runAsAdmin == false)
                        {
                            //strError = "等待 10 秒后进程 " + strWaitExe + " 依然没有退出。放弃 install";
                            //goto ERROR1;

                            // 如果此法失败，则尝试用管理员权限进行安装
                            if (RestartAsAdministrator() == false)
                                goto ERROR1;
                            return;
                        }
                    }
                }

#if NO

                // strTargetDir 一般为 c:\dp2circulation

                string strNewDir = strTargetDir + "_new";
                string strOldDir = strTargetDir + "_old";

                // *** 将 c:\dp2circulation 目录中的全部文件，复制到 c:\dp2circulation_new 中
                if (Directory.Exists(strTargetDir) == true)
                {
                    Console.WriteLine("复制 "+strTargetDir+" 到 "+strNewDir+" ...");

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

                // *** 将 app.zip 和 data.zip 展开覆盖到 c:\dp2circulation_new 目录
                string strZipFileName = Path.Combine(strSourceDir, "app.zip");
                Console.WriteLine("展开 " + strZipFileName + " 到 " + strNewDir + " ...");
                nRet = ExtractFile(strZipFileName,
                    strNewDir,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strZipFileName = Path.Combine(strSourceDir, "data.zip");
                Console.WriteLine("展开 " + strZipFileName + " 到 " + strNewDir + " ...");
                nRet = ExtractFile(strZipFileName,
                    strNewDir,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // (准备工作) 将以前遗留的 c:\dp2circulation_old 目录删除
                DeleteDirectory(strOldDir);
                // *** 将 c:\dp2circulation 目录改名为 c:\dp2circulation_old 目录
                // 如果改名失败，则终止
                try
                {
                    Console.WriteLine("将目录 " + strTargetDir + " 改名为 " + strOldDir + " ...");

                    Directory.Move(strTargetDir, strOldDir);
                }
                catch (Exception ex)
                {
                    strError = "将目录 " + strTargetDir + " 改名为 " + strOldDir + " 时出错: " + ex.Message;
                    goto ERROR1;
                }

                // *** 将 c:\dp2circulation_new 目录改名为 c:\dp2circulation 目录
                // 如果改名失败，需将上一步 Undo，然后终止
                try
                {
                    Console.WriteLine("将目录 " + strNewDir + " 改名为 " + strTargetDir + " ...");

                    Directory.Move(strNewDir, strTargetDir);
                }
                catch (Exception ex)
                {
                    Directory.Move(strOldDir, strTargetDir);
                    strError = "将目录 " + strNewDir + " 改名为 " + strTargetDir + " 时出错: " + ex.Message;
                    goto ERROR1;
                }
#endif
                if (runAsAdmin == false)
                {
                    // 安装软件。用目录改名法
                    nRet = Install_1(
                        strSourceDir,
                        strTargetDir,
                        strFiles,
                        strWaitExe,
                        out strError);
                    if (nRet == -1)
                    {
                        // 如果此法失败，则尝试用管理员权限进行安装
                        if (RestartAsAdministrator() == false)
                            goto ERROR1;
                    }
                }
                else
                {
                    // 安装软件。用 MoveFileEx 法
                    // return:
                    //      -1  出错
                    //      0   成功。不需要 reboot
                    //      1   成功。需要 reboot
                    nRet = Install_2(
            strSourceDir,
            strTargetDir,
            strFiles,
            strWaitExe,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                        MessageBox.Show("dp2circulation 安装复制文件已经完成。请重启 Windows，以完成安装");
                }

                Console.WriteLine("升级安装成功");
                return;
            }
            return;
        ERROR1:
            Console.WriteLine(strError);
        }

        static bool RestartAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,
                // so instead we launch the app as administrator in a new process.
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";
                string [] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    string[] parameters = new string[args.Length - 1];
                    Array.Copy(args, 1, parameters, 0, parameters.Length);
                    processInfo.Arguments = string.Join(" ", parameters);
                }

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    MessageBox.Show("GreenUtility 无法运行。\r\n\r\n因为安装复制文件的需要，必须在 Administrator 权限下才能运行");
                    return false;
                }

                // Shut down the current process
                // Application.Exit();
            }

            return true;
        }

        // 将目录路径变为临时形态。例如，将 c:\dp2circulation_old 变为 c:\~dp2circulation_old
        static void MakeTempStyle(ref string strPath)
        {
            strPath = Path.Combine(Path.GetDirectoryName(strPath),
                "~" + Path.GetFileName(strPath));
        }

        // 安装软件。用目录改名法
        static int Install_1(
            string strSourceDir,
            string strTargetDir,
            string strFiles,
            string strWaitExe,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // strTargetDir 一般为 c:\dp2circulation

            string strNewDir = strTargetDir + "_new";
            string strOldDir = strTargetDir + "_old";

            MakeTempStyle(ref strNewDir);
            MakeTempStyle(ref strOldDir);

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

            // *** 将 app.zip 和 data.zip 展开覆盖到 c:\dp2circulation_new 目录
            string[] files = strFiles.Split(new char[] { ',' });
            foreach (string file in files)
            {
                string strZipFileName = Path.Combine(strSourceDir, file);
                Console.WriteLine("展开 " + strZipFileName + " 到 " + strNewDir + " ...");
                // return:
                //      -1  出错
                //      0   成功。不需要 reboot
                //      1   成功。需要 reboot
                nRet = ExtractFile(strZipFileName,
                    strNewDir,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

#if NO
            strZipFileName = Path.Combine(strSourceDir, "data.zip");
            Console.WriteLine("展开 " + strZipFileName + " 到 " + strNewDir + " ...");
            // return:
            //      -1  出错
            //      0   成功。不需要 reboot
            //      1   成功。需要 reboot
            nRet = ExtractFile(strZipFileName,
                strNewDir,
                false,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            // (准备工作) 将以前遗留的 c:\dp2circulation_old 目录删除
            try
            {
                Directory.Delete(strOldDir, true);
            }
            catch (DirectoryNotFoundException)
            {
                // 不存在就算了
            } 
            catch(Exception ex)
            {
                strError = "删除目录 " + strOldDir + " 时出错: " + ex.Message;
                goto ERROR1;
            }
            
            // *** 将 c:\dp2circulation 目录改名为 c:\dp2circulation_old 目录
            // 如果改名失败，则终止
            try
            {
                Console.WriteLine("将目录 " + strTargetDir + " 改名为 " + strOldDir + " ...");

                Directory.Move(strTargetDir, strOldDir);
            }
            catch (Exception ex)
            {
                strError = "将目录 " + strTargetDir + " 改名为 " + strOldDir + " 时出错: " + ex.Message;
                goto ERROR1;
            }

            // *** 将 c:\dp2circulation_new 目录改名为 c:\dp2circulation 目录
            // 如果改名失败，需将上一步 Undo，然后终止
            try
            {
                Console.WriteLine("将目录 " + strNewDir + " 改名为 " + strTargetDir + " ...");

                Directory.Move(strNewDir, strTargetDir);
            }
            catch (Exception ex)
            {
                Directory.Move(strOldDir, strTargetDir);
                strError = "将目录 " + strNewDir + " 改名为 " + strTargetDir + " 时出错: " + ex.Message;
                goto ERROR1;
            }

            return 0;
        ERROR1:
            return -1;
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
            string strWaitExe,
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
                            if (ExtractFile(e, strTargetDir, bAllowDelayOverwrite) == true)
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
            bool bAllowDelayOverwrite)
        {
            string strTempDir = TempDir;

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
                    CreateDirIfNeed(Path.GetDirectoryName(strTargetPath));

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
                                throw new Exception("MoveFileEx() '"+strLastFileName+"' '"+strTargetPath+"' 失败");
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
        public static extern bool MoveFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            MoveFileFlags dwFlags);

#endregion


        // 在指定目录中找到第一个 *.csproj 文件名
        static string GetProjectFileName(string strSourceDir)
        {
            DirectoryInfo di = new DirectoryInfo(strSourceDir);
            FileInfo [] fis = di.GetFiles("*.csproj");
            foreach(FileInfo fi in fis)
            {
                return fi.FullName;
            }

            return null;
        }

        static int CreateZipFile(string strProjectFileName,
            string strTargetDir,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strProjectFileName);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strProjectFileName + "' 到 XmlDocument 时出错: " + ex.Message;
                return -1;
            }

            string strSourceDir = Path.GetDirectoryName(strProjectFileName);
            List<string> filenames = new List<string>();

#if NO
            /*
    <PublishFile Include="acceptorigin.css">
      <Visible>False</Visible>
      <Group>
      </Group>
      <TargetPath>
      </TargetPath>
      <PublishState>DataFile</PublishState>
      <IncludeHash>True</IncludeHash>
      <FileType>File</FileType>
    </PublishFile>
             * */
            XmlNodeList publishs = dom.DocumentElement.SelectNodes("//PublishFile");
            foreach (XmlElement publishFile in publishs)
            {
                XmlElement publishState = publishFile.SelectSingleNode("PublishState") as XmlElement;
                if (publishState == null)
                    continue;
                string strPublishState = publishState.InnerText.Trim();
                if (strPublishState == "Exclude")
                    continue;
                string strInclude = publishFile.GetAttribute("Include");
                if (string.IsNullOrEmpty(strInclude) == true)
                    continue;

                string strSourceFileName = Path.Combine(strSourceDir, strInclude);
                if (File.Exists(strSourceFileName) == false)
                {
                    strError = "拟复制的文件 '" + strSourceFileName + "' 不存在";
                    return -1;
                }
                filenames.Add(strSourceFileName);
            }
#endif
            /*
    <Content Include="exchangeratetable.css" />
    <Content Include="getsummary.js">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="history.css">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <None Include="isbn.xml" />
    <Content Include="itemhandover.css">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
             * */
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            _referenceTable.Clear();
            BuildReferenceTable(dom, nsmgr, ref _referenceTable);

            XmlNodeList contents = dom.DocumentElement.SelectNodes("//ns:Content", nsmgr);
            foreach (XmlElement content in contents)
            {
                XmlElement copyto = content.SelectSingleNode("ns:CopyToOutputDirectory", nsmgr) as XmlElement;
                if (copyto == null)
                    continue;
                string strCopyTo = copyto.InnerText.Trim();
                if (string.IsNullOrEmpty(strCopyTo) == true)
                    continue;

                string strInclude = content.GetAttribute("Include");
                if (string.IsNullOrEmpty(strInclude) == true)
                    continue;

                string strSourceFileName = Path.Combine(strSourceDir, strInclude);
                if (File.Exists(strSourceFileName) == false)
                {
                    strError = "拟复制的文件 '" + strSourceFileName + "' 不存在";
                    return -1;
                }
                filenames.Add(strSourceFileName);
            }

            string strZipFileName = Path.Combine(strTargetDir, "data.zip");

            int nRet = Compress(
                strSourceDir,
                filenames,
                Encoding.UTF8,
                strZipFileName,
                out strError);
            if (nRet == -1)
                return -1;

            // .exe .exe.config .exe.manifest
            string strBinSourceDir = Path.Combine(strSourceDir, "bin\\debug");
            strZipFileName = Path.Combine(strTargetDir, "app.zip");

            filenames = GetExeFileNames(strBinSourceDir);

            // Debug.Assert(false, "");
            // 添加一些可能遗漏的文件 2016/3/29
            /*
    <PublishFile Include="DocumentFormat.OpenXml">
      <Visible>False</Visible>
      <Group>
      </Group>
      <TargetPath>
      </TargetPath>
      <PublishState>Include</PublishState>
      <IncludeHash>True</IncludeHash>
      <FileType>Assembly</FileType>
    </PublishFile>
             * */
            XmlNodeList publishFiles = dom.DocumentElement.SelectNodes("//ns:PublishFile", nsmgr);
            foreach(XmlElement publishFile in publishFiles)
            {
                string strFileType = GetNodeInnerText(publishFile.SelectSingleNode("ns:FileType", nsmgr));
                string strPublishState = GetNodeInnerText(publishFile.SelectSingleNode("ns:PublishState", nsmgr));
                if (strFileType != "Assembly" || strPublishState != "Include")
                    continue;
                string strFileName = publishFile.GetAttribute("Include") + ".dll";
                string strHintPath = (string)_referenceTable[strFileName];
                if (string.IsNullOrEmpty(strHintPath))
                    continue;
                string strFilePath = Path.Combine(strSourceDir, strHintPath);

                // 复制文件到 bin 目录
                string strTargetPath = Path.Combine(strBinSourceDir, strFileName);
                File.Copy(strFilePath, strTargetPath, true);

                Console.WriteLine("追加文件: " + strTargetPath);
                strFilePath = new FileInfo(strTargetPath).FullName;
                if (filenames.IndexOf(strTargetPath) == -1)
                    filenames.Add(strTargetPath);
            }

            nRet = Compress(
strBinSourceDir,
filenames,
Encoding.UTF8,
strZipFileName,
out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        static Hashtable _referenceTable = new Hashtable(); // Reference 对照表。纯文件名 --> 相对路径

        static void BuildReferenceTable(XmlDocument dom,
            XmlNamespaceManager nsmgr,
            ref Hashtable referenceTable)
        {
            /*
    <Reference Include="DocumentFormat.OpenXml, Version=2.5.5631.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>packages\DocumentFormat.OpenXml.2.5\lib\DocumentFormat.OpenXml.dll</HintPath>
    </Reference>
             * */
            XmlNodeList references = dom.DocumentElement.SelectNodes("//ns:Reference", nsmgr);
            foreach (XmlElement reference in references)
            {
                string strHintPath = GetNodeInnerText(reference.SelectSingleNode("ns:HintPath", nsmgr));
                if (string.IsNullOrEmpty(strHintPath))
                    continue;
                string strFileName = GetLeft(reference.GetAttribute("Include")) + ".dll";
                referenceTable[strFileName] = strHintPath;
            }
        }

        // 获得逗号左边的字符串
        static string GetLeft(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            int nRet = strText.IndexOf(",");
            if (nRet == -1)
                return strText;
            return strText.Substring(0, nRet).Trim();
        }

        static string GetNodeInnerText(XmlNode node)
        {
            if (node == null)
                return "";
            return node.InnerText.Trim();
        }

        static int Compress(
    string strBaseDir,
    List<string> filenames,
    Encoding encoding,
    string strZipFileName,
    out string strError)
        {
            strError = "";

            if (filenames.Count == 0)
                return 0;

            using (ZipFile zip = new ZipFile(encoding))
            {
                // http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                // https://dotnetzip.codeplex.com/workitem/14087
                // uncommenting the following line can be used as a work-around
                zip.ParallelDeflateThreshold = -1;

                foreach (string filename in filenames)
                {
                    string strShortFileName = filename.Substring(strBaseDir.Length + 1);
                    string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                    zip.AddFile(filename, directoryPathInArchive);
                }

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save(strZipFileName);
            }

            return 1;
        }

        // 获得一个目录下的全部文件名。包括子目录中的
        static List<string> GetExeFileNames(string strDataDir)
        {
            // Application.DoEvents();

            DirectoryInfo di = new DirectoryInfo(strDataDir);

            List<string> result = new List<string>();

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                string strFileName = fi.Name.ToLower();
                string strExtention = Path.GetExtension(strFileName);
                if (strExtention == ".exe"
                    || strExtention == ".dll"
                    || strFileName.EndsWith(".exe.config") == true
                    || strFileName.EndsWith(".exe.manifest") == true)
                    result.Add(fi.FullName);
            }

#if NO
            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                result.AddRange(GetExeFileNames(subdir.FullName));
            }
#endif

            return result;
        }

        #region CopyDirectory()

        public delegate bool FileNameFilterProc(FileSystemInfo fi);

        // 拷贝目录
        // return:
        //      -1  出错
        //      >=0 复制的文件总数
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            FileNameFilterProc filter_proc,
            out string strError)
        {
            strError = "";

            int nCount = 0;
            try
            {
                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

#if NO
                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                        Directory.Delete(strTargetDir, true);
                }
#endif

                CreateDirIfNeed(strTargetDir);

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                foreach (FileSystemInfo sub in subs)
                {
                    if (filter_proc != null && filter_proc(sub) == false)
                        continue;

                    // 复制目录
                    if ((sub.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(sub.FullName,
                            Path.Combine(strTargetDir, sub.Name),
                            filter_proc,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        nCount += nRet;
                        continue;
                    }
                    // 复制文件
                    string source = sub.FullName;
                    string target = Path.Combine(strTargetDir, sub.Name);
                    // 如果目标文件已经存在，并且修后修改时间相同，则不复制了
                    if (File.Exists(target) == true && File.GetLastWriteTimeUtc(source) == File.GetLastWriteTimeUtc(target))
                        continue;
                    File.Copy(source, target, true);
                    // 把最后修改时间设置为和 source 一样
                    File.SetLastWriteTimeUtc(target, File.GetLastWriteTimeUtc(source));
                    nCount++;
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return nCount;
        }

        // 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
        public static bool CreateDirIfNeed(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        #endregion

        public static void DeleteDirectory(string strDirPath)
        {
            try
            {
                Directory.Delete(strDirPath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // 不存在就算了
            }
        }

        // return:
        //      true 成功等到进程结束了
        //      false 到超时也没有等到进程结束
        static bool WaitProcessEnd(string strName, TimeSpan timeout)
        {
            DateTime start = DateTime.Now;
            while (true)
            {
                if (DateTime.Now - start > timeout)
                    return false;

                System.Diagnostics.Process[] process_list = System.Diagnostics.Process.GetProcesses();

                bool bFound = false;
                foreach (Process process in process_list)
                {
                    string ModuleName = "";
                    try
                    {
                        ModuleName = process.MainModule.ModuleName;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (ModuleName.StartsWith(strName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    return true;

                Thread.Sleep(2000);
            }
        }

    }
}
