using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZipUtil
{
    class ZipUtil
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ZipUtil directory zipfilename [-t]\r\n参数 -t 表示压缩前探测目录内文件(和压缩文件中相比)是否有变化，如果没有变化则不压缩了");
                return;
            }

            // 解析参数
            List<string> filenames = new List<string>();    // 存储文件名或者目录名
            List<string> options = new List<string>();  // 存储参数

            foreach (string s in args)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;
                if (s[0] == '-')
                    options.Add(s);
                else
                    filenames.Add(s);
            }


            if (filenames.Count != 2)
            {
                Console.WriteLine("文件名和目录名参数不正确。\r\nUsage: ZipUtil directory zipfilename [-t]");
                return;
            }

            string strDirectory = filenames[0];
            string strZipFileName = filenames[1];

            bool bDetect = options.IndexOf("-t") != -1; // 是否需要预先探测 .zip 内容变化

            int nCount = 0;

            string strError = "";
            int nRet = 0;
            if (bDetect == true && File.Exists(strZipFileName) == true)
            {
                string strTempFileName = Path.GetTempFileName();
                try
                {
                    // 先压缩为一个临时文件
                    nRet = CompressDirectory(strDirectory,
        strTempFileName,
        Encoding.UTF8,
        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nCount = nRet;
                    // 探测是否有变化

                    // 逐字节比较两个文件的异同
                    // return:
                    //      -1  出错
                    //      0   两个文件内容完全相同
                    //      1   两个文件内容不同
                    nRet = CompareZipFile(strTempFileName,
                        strZipFileName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        Console.WriteLine(strZipFileName + " 文件内容没有变化");
                        return;
                    }
                    File.Delete(strZipFileName);
                    File.Move(strTempFileName, strZipFileName);
                }
                finally
                {
                    // 防止遗留临时文件
                    if (File.Exists(strTempFileName) == true)
                        File.Delete(strTempFileName);
                }
            }
            else
            {
                nRet = CompressDirectory(strDirectory,
                    strZipFileName,
                    Encoding.UTF8,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                nCount = nRet;
            }

            Console.WriteLine("compress " + nCount.ToString() + " files to " + strZipFileName);
            return;
        ERROR1:
            Console.WriteLine("error: " + strError);
        }

        // 逐(内含)文件比较两个压缩文件的异同
        // return:
        //      -1  出错
        //      0   两个文件内容完全相同
        //      1   两个文件内容不同
        static int CompareZipFile(string strFileName1,
            string strFileName2,
            out string strError)
        {
            strError = "";
            try
            {
                using (ZipFile zip1 = ZipFile.Read(strFileName1))
                using (ZipFile zip2 = ZipFile.Read(strFileName2))
                {
                    for (int i = 0; i < zip1.Count; i++)
                    {
                        ZipEntry e1 = zip1[i];
                        ZipEntry e2 = zip2[i];

                        if (string.Compare(e1.FileName, e2.FileName) != 0
                            || e1.Attributes != e2.Attributes)
                            return 1;

                        if (e1.CompressedSize != e2.CompressedSize)
                            return 1;

                        if (e1.UncompressedSize != e2.UncompressedSize)
                            return 1;

                        // 解压缩看看是否内容一样
                        // return:
                        //      0   一样
                        //      1   不一样
                        if (CompareEntry(e1, e2) != 0)
                            return 1;
                    }
                }

            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            return 0;
        }

        // return:
        //      0   一样
        //      1   不一样
        static int CompareEntry(ZipEntry e1, ZipEntry e2)
        {
            // 解压缩看看是否内容一样
            using (Stream stream1 = new MemoryStream())
            using (Stream stream2 = new MemoryStream())
            {
                e1.Extract(stream1);
                e2.Extract(stream2);
                if (stream1.Length != stream2.Length)
                    return 1;
                while (true)
                {
                    int nRet = stream1.ReadByte();
                    if (nRet == -1)
                        return 0;
                    if (nRet != stream2.ReadByte())
                        return 1;
                }
            }
        }

        // 逐字节比较两个文件的异同
        // return:
        //      -1  出错
        //      0   两个文件内容完全相同
        //      1   两个文件内容不同
        static int CompareFile(string strFileName1,
            string strFileName2, 
            out string strError)
        {
            strError = "";
            try
            {
                using (FileStream stream1 = File.OpenRead(strFileName1))
                {
                    using (FileStream stream2 = File.OpenRead(strFileName2))
                    {
                        if (stream1.Length != stream2.Length)
                            return 1;

                        while (true)
                        {
                            int nRet = stream1.ReadByte();
                            if (nRet == -1)
                                return 0;

                            if (nRet != stream2.ReadByte())
                                return 1;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        static int CompressDirectory(
            string strDirectory,
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

            string strHead = Path.GetDirectoryName(strDirectory);
            // Console.WriteLine("head=["+strHead+"]");

            using (ZipFile zip = new ZipFile(encoding))
            {
                foreach (string filename in filenames)
                {
                    string strShortFileName = filename.Substring(strHead.Length + 1);
                    string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                    zip.AddFile(filename, directoryPathInArchive);
                }

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
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
    }
}
