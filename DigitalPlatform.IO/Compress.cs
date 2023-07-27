using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalPlatform.IO
{
    public static class CompressUtil
    {

        #region Compression

        public delegate void delegate_displayText(string text);

        // return:
        //      -1  出错
        //      0   成功。不需要 reboot
        //      1   成功。需要 reboot
        public static int ExtractFile(string strZipFileName,
            string strTargetDir,
            bool bAllowDelayOverwrite,
            string strTempDir,
            delegate_displayText func_display,
            CancellationToken token,
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
                        if (token.IsCancellationRequested)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        string strTargetPath = Path.Combine(strTargetDir, e.FileName);
                        func_display?.Invoke(strTargetPath);

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
        public static bool ExtractFile(ZipEntry e,
            string strTargetDir,
            string strTempDir,
            bool bAllowDelayOverwrite)
        {
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

                    // Console.WriteLine("展开文件 " + strTargetPath);
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
                            /*
MOVEFILE_DELAY_UNTIL_REBOOT
4 (0x4)
The system does not move the file until the operating system is restarted. The system moves the file immediately after AUTOCHK is executed, but before creating any paging files. Consequently, this parameter enables the function to delete paging files from previous startups.
This value can be used only if the process is in the context of a user who belongs to the administrators group or the LocalSystem account.

This value cannot be used with MOVEFILE_COPY_ALLOWED.
                            * */
                            // 注: 当前进程为 Administrator 权限时才能使用 MOVEFILE_DELAY_UNTIL_REBOOT
                            if (MoveFileEx(strLastFileName, strTargetPath, MoveFileFlags.DelayUntilReboot | MoveFileFlags.ReplaceExisting) == false)
                            {
                                var error_code = Marshal.GetLastWin32Error();
                                throw new Exception($"MoveFileEx() '{strLastFileName}' --> '{strTargetPath}' 失败，Win32 错误码 {error_code}");
                            }
                            File.SetLastWriteTime(strLastFileName, e.LastModified);
                            // Console.WriteLine("延迟展开文件 " + strTargetPath);
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

        // 2023/7/26
        // 强制拷贝。即便目标文件已经被锁定也能拷贝(拷贝后需要重启 Windows 才能生效)
        // return:
        //      false   普通拷贝完成，不需要重新启动
        //      true    需要重新启动
        static bool TryCopyFile(
            string strSourcePath,
            DateTime lastModified,
            string strTargetPath,
            string strTempDir,
            bool bAllowDelayOverwrite)
        {
            int nErrorCount = 0;
            for (; ; )
            {
                try
                {
                    // 确保目标目录已经创建
                    PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strTargetPath));

                    File.Copy(strSourcePath, strTargetPath, true);
                    File.SetLastWriteTime(strTargetPath, lastModified);

                    // Console.WriteLine("展开文件 " + strTargetPath);
                    return false;
                }
                catch (Exception ex)
                {
                    if (nErrorCount > 10 || bAllowDelayOverwrite)
                    {
                        if (bAllowDelayOverwrite == false)
                            throw new Exception("复制文件 " + strSourcePath + " 到 " + strTargetPath + " 的过程中出现错误: " + ex.Message);
                        else
                        {
                            string strLastFileName = Path.Combine(strTempDir, Guid.NewGuid().ToString());
                            File.Move(strSourcePath, strLastFileName);
                            strSourcePath = "";
                            if (MoveFileEx(strLastFileName, strTargetPath, MoveFileFlags.DelayUntilReboot | MoveFileFlags.ReplaceExisting) == false)
                                throw new Exception("MoveFileEx() '" + strLastFileName + "' '" + strTargetPath + "' 失败");
                            File.SetLastWriteTime(strLastFileName, lastModified);
                            // Console.WriteLine("延迟展开文件 " + strTargetPath);
                            return true;
                        }
                    }

                    nErrorCount++;
                    Thread.Sleep(1000);
                }
            }
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
        static extern bool MoveFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            MoveFileFlags dwFlags);

        #endregion

        // 压缩一个目录到 .zip 文件
        // parameters:
        //      strBase 在 .zip 文件中的文件名要从全路径中去掉的前面部分
        // Exception:
        //      如果目标文件目录不存在，会抛出异常
        public static int CompressDirectory(
            string strDirectory,
            string strBase,
            string strZipFileName,
            Encoding encoding,
            out string strError)
        {
            strError = "";

            try
            {
                DirectoryInfo di = new DirectoryInfo(strDirectory);
                if (di.Exists == false)
                {
                    strError = "directory '" + strDirectory + "' not exist";
                    return -1;
                }
                strDirectory = di.FullName;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            if (File.Exists(strZipFileName) == true)
            {
                try
                {
                    File.Delete(strZipFileName);
                }
                catch
                {
                }
            }

            List<string> filenames = GetFileNames(strDirectory);

            if (filenames.Count == 0)
                return 0;

            // string strHead = Path.GetDirectoryName(strDirectory);
            // Console.WriteLine("head=["+strHead+"]");

            using (ZipFile zip = new ZipFile(encoding))
            {
                // https://stackoverflow.com/questions/21583512/access-denied-to-a-tmp-path
                // zip.TempFileFolder = System.IO.Path.GetTempPath();

                // http://stackoverflow.com/questions/15337186/dotnetzip-badreadexception-on-extract
                // https://dotnetzip.codeplex.com/workitem/14087
                // uncommenting the following line can be used as a work-around
                zip.ParallelDeflateThreshold = -1;

                foreach (string filename in filenames)
                {
                    // string strShortFileName = filename.Substring(strHead.Length + 1);
                    string strShortFileName = filename.Substring(strBase.Length + 1);
                    string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                    zip.AddFile(filename, directoryPathInArchive);
                }

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;

                // 2020/6/4
                // Directory.CreateDirectory(Path.GetDirectoryName(strZipFileName));

                zip.Save(strZipFileName);
            }

            return filenames.Count;
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


        #endregion

    }
}
