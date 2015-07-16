using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform.Text;
using System.Text.RegularExpressions;
using DigitalPlatform.IO;
using System.Xml;
using System.Collections;

namespace DigitalPlatform.LibraryServer
{
    public class LibraryServerUtil
    {
        public static double GetLibraryXmlVersion(XmlDocument dom)
        {
            // 找到<version>元素
            XmlNode nodeVersion = dom.DocumentElement.SelectSingleNode("version");
            if (nodeVersion != null)
            {
                string strVersion = nodeVersion.InnerText;
                if (String.IsNullOrEmpty(strVersion) == true)
                    strVersion = "0.01";

                double version = 0.01;
                try
                {
                    version = Convert.ToDouble(strVersion);
                }
                catch
                {
                    version = 0.01;
                }

                return version;
            }

            return 0.01;
        }

        // 升级 library.xml 中的用户账户相关信息
        // 文件格式 v2.00(或以下)到v2.01
        // accounts/account 中 password 存储方式改变
        // parameters:
        //      strEncryptKey   原来版本中用到的加密 key 字符串
        public static int UpgradeLibraryXmlUserInfo(
            string strEncryptKey,
            ref XmlDocument dom,
            out string strError)
        {
            strError = "";

            XmlNodeList users = dom.DocumentElement.SelectNodes("//accounts/account");
            foreach (XmlElement user in users)
            {
                string strExistPassword = user.GetAttribute("password");
                if (String.IsNullOrEmpty(strExistPassword) == false)
                {
                    string strPlainText = "";
                    try
                    {
                        strPlainText = Cryptography.Decrypt(strExistPassword,
                            strEncryptKey);
                    }
                    catch
                    {
                        strError = "已经存在的旧版(加密后)密码格式不正确";
                        return -1;
                    }

                    string strHashed = "";
                    int nRet = SetUserPassword(strPlainText, out strHashed, out strError);
                    if (nRet == -1)
                    {
                        strError = "SetUserPassword() error: " + strError;
                        return -1;
                    }
                    user.SetAttribute("password", strHashed);
                }
            }

            return 0;
        }

        // 2015/5/20 新的密码存储策略
        // 验证密码
        // return:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        public static int MatchUserPassword(
            string strPassword,
            string strHashed,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            // 允许明文空密码
            if (String.IsNullOrEmpty(strHashed) == true)
            {
                if (strPassword != strHashed)
                {
                    strError = "密码不正确";
                    return 0;
                }

                return 1;
            }

            try
            {
                strPassword = Cryptography.GetSHA1(strPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            if (strPassword != strHashed)
            {
                strError = "密码不正确";
                return 0;
            }

            return 1;
        }

        // 2015/5/20 新的密码存储策略
        // 准备用于存储的密码
        // return:
        //      -1  出错
        //      0   成功
        public static int SetUserPassword(
            string strNewPassword,
            out string strHashed,
            out string strError)
        {
            strError = "";
            strHashed = "";

            try
            {
                strHashed = Cryptography.GetSHA1(strNewPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            return 0;
        }


        // 检查出版时间范围字符串是否合法
        // 如果使用单个出版时间来调用本函数，也是可以的
        // return:
        //      -1  出错
        //      0   正确
        public static int CheckPublishTimeRange(string strText,
            out string strError)
        {
            strError = "";

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                return CheckSinglePublishTime(strText,
            out strError);
            }

            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, "-", out strLeft, out strRight);
            nRet = CheckSinglePublishTime(strLeft,
                out strError);
            if (nRet == -1)
            {
                strError = "出版时间字符串 '" + strText + "' 的起始时间部分 '" + strLeft + "' 格式错误: " + strError;
                return -1;
            }

            nRet = CheckSinglePublishTime(strRight,
                out strError);
            if (nRet == -1)
            {
                strError = "出版时间字符串 '" + strText + "' 的结束时间部分 '" + strRight + "' 格式错误: " + strError;
                return -1;
            }

            return 0;
        }

        // 检查单个出版时间字符串是否合法
        // return:
        //      -1  出错
        //      0   正确
        public static int CheckSinglePublishTime(string strText,
            out string strError)
        {
            strError = "";
            // 检查出版时间格式是否正确
            /*
            if (strText.Length != 4
                && strText.Length != 6
                && strText.Length != 8)
            {
                strError = "出版时间 '" + strText + "' 格式错误。应当为4 6 8 个数字字符";
                return -1;
            }
             * */
            if (strText.Length != 8)
            {
                strError = "出版时间 '" + strText + "' 格式错误。应当为8个数字字符";
                return -1;
            }

            if (StringUtil.IsPureNumber(strText) == false)
            {
                strError = "出版时间 '" + strText + "' 格式错误。必须为纯数字字符";
                return -1;
            }

            if (strText.Length == 8)
            {
                try
                {
                    DateTime now = DateTimeUtil.Long8ToDateTime(strText);
                }
                catch (Exception ex)
                {
                    strError = "出版时间 '" + strText + "' 格式错误: " + ex.Message;
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 匹配馆藏地点定义
        /// </summary>
        /// <param name="strLocation">馆藏地点</param>
        /// <param name="strPattern">匹配模式。例如 "海淀分馆/*"</param>
        /// <returns>true 表示匹配上； false 表示没有匹配上</returns>
        public static bool MatchLocationName(string strLocation, string strPattern)
        {
            // 如果没有通配符，则要求完全一致
            if (strPattern.IndexOf("*") == -1)
                return strLocation == strPattern;

            strPattern = strPattern.Replace("*", ".*");
            if (StringUtil.RegexCompare(strPattern,
                RegexOptions.None,
                strLocation) == true)
                return true;
            return false;
        }
    }
}
