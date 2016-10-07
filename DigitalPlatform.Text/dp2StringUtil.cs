using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DigitalPlatform.Text
{
    /// <summary>
    /// dp2 系统中涉及业务逻辑的字符串处理函数
    /// </summary>
    public static class dp2StringUtil
    {
        // 从 volumeInfo.cs 中移动过来
        // 获得出版日期的年份部分
        public static string GetYearPart(string strPublishTime)
        {
            if (String.IsNullOrEmpty(strPublishTime) == true)
                return strPublishTime;

            if (strPublishTime.Length <= 4)
                return strPublishTime;

            return strPublishTime.Substring(0, 4);
        }

        // 2016/10/6
        // 从期记录中获得 父记录ID + 期号 字符串
        public static string GetParentIssue(XmlDocument dom)
        {
            XmlNode node = dom.DocumentElement.SelectSingleNode("parent/text()");
            string strParent = "";
            if (node != null)
                strParent = node.Value;

            node = dom.DocumentElement.SelectSingleNode("publishTime/text()");
            string strYear = "";
            if (node != null)
            {
                string strPublishTime = node.Value;
                strYear = GetYearPart(strPublishTime);
            }

            node = dom.DocumentElement.SelectSingleNode("issue/text()");
            string strIssue = "";
            if (node != null)
                strIssue = node.Value;

            node = dom.DocumentElement.SelectSingleNode("zong/text()");
            string strZong = "";
            if (node != null)
                strZong = node.Value;

            node = dom.DocumentElement.SelectSingleNode("volume/text()");
            string strVolume = "";
            if (node != null)
                strVolume = node.Value;

            return strParent + "|" + strYear + "|" + strIssue + "|" + strZong + "|" + strVolume;
        }


    }
}
