using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DigitalPlatform;
using DigitalPlatform.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace dp2Circulation
{
    /// <summary>
    /// 绿色安装包有关的功能
    /// </summary>
    public class GreenProgram
    {
        // 复制出一个绿色安装
        // parameters:
        //      strProgramDir   程序目录
        //      strDataDir  数据目录
        //      strTargetDir    目标安装目录
        // return:
        //      -1  出错
        //      0   没有必要创建。(也许是因为当前程序正是从备用位置启动的)
        //      1   已经创建
        public static int CopyGreen(
            string strProgramDir,
            string strDataDir,
            string strTargetDir,
            out string strError)
        {
            strError = "";

            if (PathUtil.IsEqual(strProgramDir, strTargetDir) == true)
            {
                // MessageBox.Show("本来是从备用位置启动，没有必要复制了");
                return 0;
            }

            // 将两个要害文件的最后修改时间进行比较？如果没有变化就不要复制了

            int nRet = CopyDirectory(strProgramDir,
                strTargetDir,
                FileNameFilter,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = CopyDirectory(strDataDir,
    strTargetDir,
    FileNameFilter,
    out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        static bool FileNameFilter(FileSystemInfo fi)
        {
            // Application.DoEvents();
            return true;
        }

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

                PathUtil.CreateDirIfNeed(strTargetDir);

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
                    nCount++;
                }
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return nCount;
        }

#if NO
        public static void CreateShortcutToDesktop(
            string linkName,
            string strAppPath)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            using (StreamWriter writer = new StreamWriter(deskDir + "\\" + linkName + ".url"))
            {
                // string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + strAppPath);
                writer.WriteLine("IconIndex=0");
                string icon = strAppPath.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
                writer.Flush();
            }
        }
#endif

        public static void CreateShortcutToDesktop(
            string linkName,
            string strAppPath,
            bool bOverwriteExist = true)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            string strLnkFilePath = deskDir + "\\" + linkName + ".lnk";

            if (bOverwriteExist == false && File.Exists(strLnkFilePath) == true)
                return;

            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
            dynamic shell = Activator.CreateInstance(t);
            try
            {
                var lnk = shell.CreateShortcut(strLnkFilePath);
                try
                {
                    lnk.TargetPath = strAppPath;    //  @"C:\something";
                    lnk.IconLocation = strAppPath + ", 0";
                    lnk.WorkingDirectory = Path.GetDirectoryName(strAppPath);
                    lnk.Save();
                }
                finally
                {
                    Marshal.FinalReleaseComObject(lnk);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(shell);
            }
        }


    }
}
