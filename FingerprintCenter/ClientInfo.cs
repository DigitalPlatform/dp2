using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.IO;

namespace FingerprintCenter
{
    /// <summary>
    /// 存储各种程序信息的全局类
    /// </summary>
    public static class ClientInfo
    {
        public static MainForm MainForm { get; set; }

        /// <summary>
        /// 前端，的版本号
        /// </summary>
        public static string ClientVersion { get; set; }

        /// <summary>
        /// 数据目录
        /// </summary>
        public static string DataDir = "";

        /// <summary>
        /// 用户目录
        /// </summary>
        public static string UserDir = "";

        // 附加的一些文件名非法字符。比如 XP 下 Path.GetInvalidPathChars() 不知何故会遗漏 '*'
        static string spec_invalid_chars = "*?:";

        public static string GetValidPathString(string strText, string strReplaceChar = "_")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            char[] invalid_chars = Path.GetInvalidPathChars();
            StringBuilder result = new StringBuilder();
            foreach (char c in strText)
            {
                if (c == ' ')
                    continue;
                if (IndexOf(invalid_chars, c) != -1
                    || spec_invalid_chars.IndexOf(c) != -1)
                    result.Append(strReplaceChar);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        static int IndexOf(char[] chars, char c)
        {
            int i = 0;
            foreach (char c1 in chars)
            {
                if (c1 == c)
                    return i;
                i++;
            }

            return -1;
        }


        /// <summary>
        /// 指纹本地缓存目录
        /// </summary>
        public static string FingerPrintCacheDir(string strUrl)
        {
            string strServerUrl = GetValidPathString(strUrl.Replace("/", "_"));

            return Path.Combine(UserDir, "fingerprintcache\\" + strServerUrl);
        }

        public static string Lang = "zh";

        // parameters:
        //      product_name    例如 "fingerprintcenter"
        public static void InitialDirs(string product_name)
        {
            ClientVersion = Assembly.GetAssembly(typeof(Program)).GetName().Version.ToString();

            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                DataDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            UserDir = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    product_name);
            PathUtil.TryCreateDir(UserDir);
        }

    }
}
