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
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ZipUtil directory zipfilename");
                return;
            }

            string strDirectory = args[0];
            string strZipFileName = args[1];

            string strError = "";
            int nRet = CompressDirectory(strDirectory, strZipFileName, Encoding.UTF8, out strError);
            if (nRet == -1)
            {
                Console.WriteLine("error: " + strError);
                return;
            }

            Console.WriteLine("compress " + nRet.ToString() + " files to " + strZipFileName);
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
