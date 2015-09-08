using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DigitalPlatform.IO
{
    /// <summary>
    /// File功能扩展函数
    /// </summary>
    public class FileUtil
    {
        // 检测字符串是否为纯数字(不包含'-','.'号)
        public static bool IsPureNumber(string s)
        {
            if (s == null)
                return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] > '9' || s[i] < '0')
                    return false;
            }
            return true;
        }

        // 文件扩展名是否为 ._1 ._2 ...
        public static bool IsBackupFile(string strFileName)
        {
            string strExt = Path.GetExtension(strFileName);
            if (string.IsNullOrEmpty(strExt) == true)
                return false;
            if (strExt.StartsWith("._") == false)
                return false;
            string strNumber = strExt.Substring(2);
            if (IsPureNumber(strNumber) == true)
                return true;

            return false;
        }

        /// <summary>
        /// 获得文件的长度
        /// </summary>
        /// <param name="strFileName">文件名全路径</param>
        /// <returns>返回文件长度。如果是 -1，表示文件不存在</returns>
        public static long GetFileLength(string strFileName)
        {
            FileInfo fi = new FileInfo(strFileName);
            if (fi.Exists == false)
                return -1;
            return fi.Length;
        }

        // 根据时间戳信息，设置文件的最后修改时间
        public static void SetFileLastWriteTimeByTimestamp(string strFilePath,
            byte[] baTimeStamp)
        {
            if (baTimeStamp == null || baTimeStamp.Length < 8)
                return;
#if NO
            byte [] baTime = new byte[8];
            Array.Copy(baTimeStamp,
    0,
    baTime,
    0,
    8);
#endif
            long lTicks = BitConverter.ToInt64(baTimeStamp, 0);

            FileInfo fileInfo = new FileInfo(strFilePath);
            if (fileInfo.Exists == false)
                return;
            fileInfo.LastWriteTimeUtc = new DateTime(lTicks);
        }

        // 根据文件的最后修改时间和尺寸, 获得时间戳信息
        public static byte[] GetFileTimestamp(string strFilePath)
        {
            byte[] baTimestamp = null;
            FileInfo fileInfo = new FileInfo(strFilePath);
            if (fileInfo.Exists == false)
                return baTimestamp;

            long lTicks = fileInfo.LastWriteTimeUtc.Ticks;
            byte[] baTime = BitConverter.GetBytes(lTicks);

            byte[] baLength = BitConverter.GetBytes((long)fileInfo.Length);
            //Array.Reverse(baLength);

            baTimestamp = new byte[baTime.Length + baLength.Length];
            Array.Copy(baTime,
                0,
                baTimestamp,
                0,
                baTime.Length);
            Array.Copy(baLength,
                0,
                baTimestamp,
                baTime.Length,
                baLength.Length);

            return baTimestamp;
        }

        // 写入日志文件。每天创建一个单独的日志文件
        public static void WriteErrorLog(
            object lockObj,
            string strLogDir,
            string strText,
            string strPrefix = "log_",
            string strPostfix = ".txt")
        {

            lock (lockObj)
            {
                DateTime now = DateTime.Now;
                // 每天一个日志文件
                string strFilename = Path.Combine(strLogDir, strPrefix + DateTimeUtil.DateTimeToString8(now) + strPostfix);
                string strTime = now.ToString();
                StreamUtil.WriteText(strFilename,
                    strTime + " " + strText + "\r\n");
            }
        }

        // 能自动识别文件内容的编码方式的读入文本文件内容模块
        // parameters:
        //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
        // return:
        //      -1  出错 strError中有返回值
        //      0   文件不存在 strError中有返回值
        //      1   文件存在
        //      2   读入的内容不是全部
        public static int ReadTextFileContent(string strFilePath,
            long lMaxLength,
            out string strContent,
            out Encoding encoding,
            out string strError)
        {
            strError = "";
            strContent = "";
            encoding = null;

            if (File.Exists(strFilePath) == false)
            {
                strError = "文件 '" + strFilePath + "' 不存在";
                return 0;
            }

            encoding = FileUtil.DetectTextFileEncoding(strFilePath);

            try
            {
                bool bExceed = false;

                using (FileStream file = File.Open(
        strFilePath,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite))
                {
                    // TODO: 这里的自动探索文件编码方式功能不正确，
                    // 需要专门编写一个函数来探测文本文件的编码方式
                    // 目前只能用UTF-8编码方式
                    using (StreamReader sr = new StreamReader(file, encoding))
                    {
                        if (lMaxLength == -1)
                        {
                            strContent = sr.ReadToEnd();
                        }
                        else
                        {
                            long lLoadedLength = 0;
                            StringBuilder temp = new StringBuilder(4096);
                            for (; ; )
                            {
                                string strLine = sr.ReadLine();
                                if (strLine == null)
                                    break;
                                if (lLoadedLength + strLine.Length > lMaxLength)
                                {
                                    strLine = strLine.Substring(0, (int)(lMaxLength - lLoadedLength));
                                    temp.Append(strLine + " ...");
                                    bExceed = true;
                                    break;
                                }

                                temp.Append(strLine + "\r\n");
                                lLoadedLength += strLine.Length + 2;
                                if (lLoadedLength > lMaxLength)
                                {
                                    temp.Append(strLine + " ...");
                                    bExceed = true;
                                    break;
                                }
                            }
                            strContent = temp.ToString();
                        }
                        /*
                    sr.Close();
                    sr = null;
                         * */
                    }
                }

                if (bExceed == true)
                    return 2;
            }
            catch (Exception ex)
            {
                strError = "打开或读入文件 '" + strFilePath + "' 时出错: " + ex.Message;
                return -1;
            }

            return 1;
        }

        // 获得一个临时文件名
        // parameters:
        //      strPostFix  如果用于表示文件扩展名，应包含点。否则可以当作一般后缀字符串使用
        public static string NewTempFileName(string strDir,
            string strPrefix,
            string strPostFix)
        {
            if (string.IsNullOrEmpty(strDir) == true)
            {
                return Path.GetTempFileName();
            }

            string strFileName = "";
            for (int i = 0; ; i++)
            {
                strFileName = PathUtil.MergePath(strDir, strPrefix + Convert.ToString(i) + strPostFix);

                FileInfo fi = new FileInfo(strFileName);
                if (fi.Exists == false)
                {
                    // 创建一个0 byte的文件
                    FileStream f = File.Create(strFileName);
                    f.Close();
                    return strFileName;
                }
            }
        }

        // 把一个byte[] 写到指定的文件
        // parameter:
        //		strFileName: 文件名,每次新创建,覆盖原来的文件
        //		data: byte数组
        // 编写者: 任延华
        public static void WriteByteArray(string strFileName,
            byte[] data)
        {
            FileStream s = File.Create(strFileName);
            try
            {
                s.Write(data,
                    0,
                    data.Length);
            }
            finally
            {
                s.Close();
            }
        }

        // 改动文件的“最后修改时间”属性
        // 注意可能抛出异常
        public static void ChangeFileLastModifyTimeUtc(string strPath,
            string strTime)
        {
            FileInfo fi = new FileInfo(strPath);
            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strTime);
            fi.LastWriteTimeUtc = time;
            fi.CreationTimeUtc = time;
        }

        // 文件是否存在并且是非空
        public static bool IsFileExsitAndNotNull(string strFilename)
        {

            try
            {
                FileStream file = File.Open(
                    strFilename,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);

                if (file.Length == 0)
                    return false;

                file.Close();
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        // 文件是否存在?
        public static bool FileExist(string strFilePath)
        {
            FileInfo fi = new FileInfo(strFilePath);
            return fi.Exists;
        }


        // 功能:将一个字符串写到文件,当文件不存在时,创建文件;存在,覆盖文件
        // paramter:
        //		strContent:  字符串
        //		strFileName: 文件名
        // 编写者：任延华
        public static void String2File(string strContent,
            string strFileName)
        {
            StreamWriter s = File.CreateText(strFileName);
            try
            {
                s.Write(strContent);
            }
            finally
            {
                s.Close();
            }
        }


        // 功能:文件到字符串，使用直接读到尾的方法
        // strFileName: 文件名
        public static string File2StringE(string strFileName)
        {
            if (strFileName == null
                || strFileName == "")
                return "";
            StreamReader sr = new StreamReader(strFileName, true);
            string strText = sr.ReadToEnd();
            sr.Close();

            return strText;
        }

        // 如果未能探测出来，则当作 936
        public static Encoding DetectTextFileEncoding(string strFilename)
        {
            Encoding encoding = DetectTextFileEncoding(strFilename, null);
            if (encoding == null)
                return Encoding.GetEncoding(936);    // default

            return encoding;
        }

        // 检测文本文件的encoding
        /*
UTF-8: EF BB BF 
UTF-16 big-endian byte order: FE FF 
UTF-16 little-endian byte order: FF FE 
UTF-32 big-endian byte order: 00 00 FE FF 
UTF-32 little-endian byte order: FF FE 00 00 
         * */
        public static Encoding DetectTextFileEncoding(string strFilename,
            Encoding default_encoding)
        {

            byte[] buffer = new byte[4];

            try
            {
                FileStream file = File.Open(
        strFilename,
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite);
                try
                {

                    if (file.Length >= 2)
                    {
                        file.Read(buffer, 0, 2);    // 1, 2 BUG

                        if (buffer[0] == 0xff && buffer[1] == 0xfe)
                        {
                            return Encoding.Unicode;    // little-endian
                        }

                        if (buffer[0] == 0xfe && buffer[1] == 0xff)
                        {
                            return Encoding.BigEndianUnicode;
                        }
                    }

                    if (file.Length >= 3)
                    {
                        file.Read(buffer, 2, 1);
                        if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                        {
                            return Encoding.UTF8;
                        }

                    }

                    if (file.Length >= 4)
                    {
                        file.Read(buffer, 3, 1);

                        // UTF-32 big-endian byte order: 00 00 FE FF 
                        // UTF-32 little-endian byte order: FF FE 00 00 

                        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xfe && buffer[3] == 0xff)
                        {
                            return Encoding.UTF32;    // little-endian
                        }

                        if (buffer[0] == 0xff && buffer[1] == 0xfe && buffer[2] == 0x00 && buffer[3] == 0x00)
                        {
                            return Encoding.GetEncoding(65006);    // UTF-32 big-endian
                        }
                    }

                }
                finally
                {
                    file.Close();
                }
            }
            catch
            {
            }

            return default_encoding;    // default
        }
    }
}
