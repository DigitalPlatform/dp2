using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace GreenInstall
{
    /// <summary>
    /// 实用函数
    /// </summary>
    public static class Library
    {
        public delegate void Delegate_copy(string sourceFileName, string targetFileName);
        // 拷贝目录
        // 遇到有同名文件会覆盖
        public static int CopyDirectory(string strSourceDir,
            string strTargetDir,
            Delegate_copy copy,
            bool bDeleteTargetBeforeCopy,
            out string strError)
        {
            strError = "";

            try
            {
                DirectoryInfo di = new DirectoryInfo(strSourceDir);

                if (di.Exists == false)
                {
                    strError = "源目录 '" + strSourceDir + "' 不存在...";
                    return -1;
                }

                if (bDeleteTargetBeforeCopy == true)
                {
                    if (Directory.Exists(strTargetDir) == true)
                    {
                        RemoveReadOnlyAttr(strTargetDir);   // 怕即将删除的目录中有隐藏文件妨碍删除

                        Directory.Delete(strTargetDir, true);
                    }
                }

                try
                {
                    // 如果目录不存在则创建之
                    // return:
                    //      false   已经存在
                    //      true    刚刚新创建
                    // exception:
                    //      可能会抛出异常 System.IO.DirectoryNotFoundException (未能找到路径“...”的一部分)
                    TryCreateDir(strTargetDir);
                }
                catch (Exception ex)
                {
                    strError = "创建目录 '" + strTargetDir + "' 时出现异常: " + ex.Message;
                    return -1;
                }

                FileSystemInfo[] subs = di.GetFileSystemInfos();

                foreach (FileSystemInfo sub in subs)
                {
                    // 复制目录
                    if ((sub.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        int nRet = CopyDirectory(sub.FullName,
                            Path.Combine(strTargetDir, sub.Name),
                            copy,
                            bDeleteTargetBeforeCopy,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        continue;
                    }
                    // 复制文件
                    if (copy != null)
                        copy(sub.FullName, Path.Combine(strTargetDir, sub.Name));
                    else
                        File.Copy(sub.FullName,
                            Path.Combine(strTargetDir, sub.Name),
                            true);
                }
            }
            catch (Exception ex)
            {
                strError = ex.Message;  //  ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            return 0;
        }

        // 移除文件目录内所有文件的 ReadOnly 属性
        public static void RemoveReadOnlyAttr(string strSourceDir)
        {
            // string strCurrentDir = Directory.GetCurrentDirectory();

            DirectoryInfo di = new DirectoryInfo(strSourceDir);

            FileSystemInfo[] subs = di.GetFileSystemInfos();

            for (int i = 0; i < subs.Length; i++)
            {

                // 递归
                if ((subs[i].Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    RemoveReadOnlyAttr(subs[i].FullName);
                }
                else
                    File.SetAttributes(subs[i].FullName, FileAttributes.Normal);

            }
        }

        // 如果目录不存在则创建之
        // return:
        //      false   已经存在
        //      true    刚刚新创建
        // exception:
        //      盘符不存在的情况下，可能会抛出异常 System.IO.DirectoryNotFoundException (未能找到路径“...”的一部分)
        public static bool TryCreateDir(string strDir)
        {
            DirectoryInfo di = new DirectoryInfo(strDir);
            if (di.Exists == false)
            {
                di.Create();
                return true;
            }

            return false;
        }

        public static bool TryParseRfc1123DateTimeString(string strTime, out DateTime time)
        {
            try
            {
                time = FromRfc1123DateTimeString(strTime);
                return true;
            }
            catch
            {
                time = new DateTime(0);
                return false;
            }
        }

        // 把字符串转换为DateTime对象
        // 注意返回的是GMT时间
        // 注意可能抛出异常
        public static DateTime FromRfc1123DateTimeString(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                throw new Exception("时间字符串为空");

            string strError = "";
            string strMain = "";
            string strTimeZone = "";
            TimeSpan offset;
            // 将RFC1123字符串中的timezone部分分离出来
            // parameters:
            //      strMain [out]去掉timezone以后的左边部分
            //      strTimeZone [out]timezone部分
            int nRet = SplitRfc1123TimeZoneString(strTime,
            out strMain,
            out strTimeZone,
            out offset,
            out strError);
            if (nRet == -1)
                throw new Exception(strError);

            DateTime parsedBack;
            string[] formats = {
                "ddd, dd MMM yyyy HH':'mm':'ss",   // [ddd, ] 'GMT'
                "dd MMM yyyy HH':'mm':'ss",
                "ddd, dd MMM yyyy HH':'mm",
                "dd MMM yyyy HH':'mm",
                                };

            bool bRet = DateTime.TryParseExact(strMain,
                formats,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out parsedBack);
            if (bRet == false)
            {
                strError = "时间字符串 '" + strTime + "' 不是RFC1123格式";
                throw new Exception(strError);
            }

            return parsedBack - offset;
        }

        // 将RFC1123字符串中的timezone部分分离出来
        // parameters:
        //      strMain [out]去掉timezone以后的左边部分// ，并去掉左边逗号以左的部分
        //      strTimeZone [out]timezone部分
        static int SplitRfc1123TimeZoneString(string strTimeParam,
            out string strMain,
            out string strTimeZone,
            out TimeSpan offset,
            out string strError)
        {
            strError = "";
            strMain = "";
            strTimeZone = "";
            offset = new TimeSpan(0);
            int nRet = 0;

            string strTime = strTimeParam.Trim();

            /*
            // 去掉逗号以左的部分
            int nRet = strTime.IndexOf(",");
            if (nRet != -1)
                strTime = strTime.Substring(nRet + 1).Trim();
             * */

            // 一位字母
            if (strTime.Length > 2
                && strTime[strTime.Length - 2] == ' ')
            {
                strMain = strTime.Substring(0, strTime.Length - 2).Trim();
                strTimeZone = strTime.Substring(strTime.Length - 1);
                if (strTimeZone == "J")
                {
                    strError = "RFC1123字符串 '" + strTimeParam + "' 格式错误： 最后一位TimeZone字符，不能为'J'";
                    return -1;
                }

                if (strTimeZone == "Z")
                    return 0;

                int nHours = 0;

                if (strTimeZone[0] >= 'A' && strTimeZone[0] < 'J')
                    nHours = -(strTimeZone[0] - 'A' + 1);
                else if (strTimeZone[0] >= 'K' && strTimeZone[0] <= 'M')
                    nHours = -(strTimeZone[0] - 'B' + 1);
                else if (strTimeZone[0] >= 'N' && strTimeZone[0] <= 'Y')
                    nHours = strTimeZone[0] - 'N' + 1;

                offset = new TimeSpan(nHours, 0, 0);
                return 0;
            }

            // ( "+" / "-") 4DIGIT
            if (strTime.Length > 5
                && (strTime[strTime.Length - 5] == '+' || strTime[strTime.Length - 5] == '-'))
            {
                strMain = strTime.Substring(0, strTime.Length - 5).Trim();
                strTimeZone = strTime.Substring(strTime.Length - 5);

                try
                {
                    offset = GetOffset(strTimeZone);
                }
                catch (Exception ex)
                {
                    strError = ex.Message;  //  ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                return 0;
            }

            string[] modes = {
                            "GMT",
                            "UT",
                            "EST",
                            "EDT",
                            "CST",
                            "CDT",
                            "MST",
                            "MDT",
                            "PST",
                            "PDT"};
            if (strTime.Length <= 3)
            {
                strError = "RFC1123字符串 '" + strTimeParam + "' 格式错误： 字符数不足";
                return -1;
            }

            string strPart = strTime.Substring(strTime.Length - 3);
            foreach (string mode in modes)
            {
                nRet = strPart.LastIndexOf(mode);
                if (nRet != -1)
                {
                    nRet = strTime.LastIndexOf(mode);
                    Debug.Assert(nRet != -1, "");

                    strMain = strTime.Substring(0, nRet).Trim();
                    strTimeZone = mode;

                    if (strTimeZone == "GMT" || strTimeZone == "UT")
                        return 0;

                    string strDigital = "";

                    switch (strTimeZone)
                    {
                        case "EST":
                            strDigital = "-0500";
                            break;
                        case "EDT":
                            strDigital = "-0400";
                            break;
                        case "CST":
                            strDigital = "-0600";
                            break;
                        case "CDT":
                            strDigital = "-0500";
                            break;
                        case "MST":
                            strDigital = "-0700";
                            break;
                        case "MDT":
                            strDigital = "-0600";
                            break;
                        case "PST":
                            strDigital = "-0800";
                            break;
                        case "PDT":
                            strDigital = "-0700";
                            break;
                        default:
                            strError = "error";
                            return -1;
                    }

                    try
                    {
                        offset = GetOffset(strDigital);
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;  //  ExceptionUtil.GetAutoText(ex);
                        return -1;
                    }

                    return 0;
                }
            }

            strError = "RFC1123字符串 '" + strTimeParam + "' 格式错误： TimeZone部分不合法";
            return -1;
        }

        static TimeSpan GetOffset(string strDigital)
        {
            if (strDigital.Length != 5)
                throw new Exception("strDigital必须为5字符");

            int hours = Convert.ToInt32(strDigital.Substring(1, 2));
            int minutes = Convert.ToInt32(strDigital.Substring(3, 2));
            TimeSpan offset = new TimeSpan(hours, minutes, 0);
            if (strDigital[0] == '-')
                offset = new TimeSpan(offset.Ticks * -1);

            return offset;
        }
    }


    public class NormalResult
    {
        public int Value { get; set; }
        public string ErrorInfo { get; set; }
        public string ErrorCode { get; set; }

        public NormalResult(NormalResult result)
        {
            this.Value = result.Value;
            this.ErrorInfo = result.ErrorInfo;
            this.ErrorCode = result.ErrorCode;
        }

        public NormalResult(int value, string error)
        {
            this.Value = value;
            this.ErrorInfo = error;
        }

        public NormalResult()
        {

        }

        public override string ToString()
        {
            return $"Value={Value},ErrorInfo={ErrorInfo},ErrorCode={ErrorCode}";
        }
    }
}
