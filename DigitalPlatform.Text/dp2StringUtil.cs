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
        // 内核数据库 keys 配置文件中的脚本部分会用到此函数
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


        // 2016/10/6
        // 从实体记录中获得 定位期 的检索式字符串
        // 如果是合订册，则可能返回多个字符串。普通册返回(最多)一个字符串
        // return:
        //      空集合
        //      其他
        public static List<IssueString> GetIssueQueryStringFromItemXml(XmlDocument dom)
        {
            if (dom == null || dom.DocumentElement == null)
                return new List<IssueString>();

            // 看看记录中是否有 binding/item 元素
            /*
- <binding>
  <item publishTime="20160101" volume="2016,no.1, 总.100, v.10" refID="1e651c7d-bcce-442a-b574-ceee7b4b81e0" price="CNY12" /> 
  <item publishTime="20160201" volume="2016,no.2, 总.101, v.10" refID="64e4cc27-36df-42ce-a90b-9d4cfa6631dd" price="CNY12" /> 
  <item publishTime="20160301" volume="2016,no.3, 总.102, v.10" refID="574b3639-a033-4bbe-8ac3-804251cb13de" price="CNY12" /> 
  <item publishTime="20160401" volume="2016,no.4, 总.103, v.10" refID="" missing="true" /> 
  <item publishTime="20160501" volume="2016,no.5, 总.104, v.10" refID="9d0a2975-2c88-4f29-9b62-2f864a850259" price="CNY12" /> 
  </binding>
             * 
             * */
            bool bAttr = false;  // 是否用属性方式存储字段
            List<XmlElement> items = new List<XmlElement>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("binding/item");
            if (nodes.Count == 0)
            {
                items.Add(dom.DocumentElement);
                bAttr = false;
            }
            else
            {
                foreach (XmlElement item in nodes)
                {
                    items.Add(item);
                }
                bAttr = true;
            }

            List<IssueString> results = new List<IssueString>();
            foreach (XmlElement item in items)
            {
                string strVolumeString = GetField(item, "volume", bAttr);

                if (string.IsNullOrEmpty(strVolumeString) == true)
                    continue;

                string strYear = "";
                string strIssue = "";
                string strZong = "";
                string strVolume = "";

                // 解析当年期号、总期号、卷号的字符串
                VolumeInfo.ParseItemVolumeString(strVolumeString,
                    out strYear,
                    out strIssue,
                    out strZong,
                    out strVolume);

                // 若 volume 元素中不包含年份，则从 publishTime 元素中取
                if (string.IsNullOrEmpty(strYear))
                {
                    string strPublishTime = GetField(item, "publishTime", bAttr);
                    strYear = dp2StringUtil.GetYearPart(strPublishTime);
                }

                results.Add(new IssueString(strVolumeString, strYear + "|" + strIssue + "|" + strZong + "|" + strVolume));
            }

            return results;
        }

        static string GetField(XmlElement item, string fieldName, bool bAttr)
        {
            if (bAttr)
                return item.GetAttribute(fieldName);
            {
                XmlNode node = item.SelectSingleNode("volume/text()");
                if (node != null)
                    return node.Value;

                return "";
            }
        }

        public static string GetCoverImageIDFromIssueRecord(XmlDocument dom,
    string strPreferredType = "MediumImage")
        {
            string strLargeUrl = "";
            string strMediumUrl = "";   // type:FrontCover.MediumImage
            string strUrl = ""; // type:FronCover
            string strSmallUrl = "";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement node in nodes)
            {
                string strUsage = node.GetAttribute("usage");
                string strID = node.GetAttribute("id");

                if (string.IsNullOrEmpty(strUsage) == true)
                    continue;

                // . 分隔 FrontCover.MediumImage
                if (StringUtil.HasHead(strUsage, "FrontCover." + strPreferredType) == true)
                    return strID;

                if (StringUtil.HasHead(strUsage, "FrontCover.SmallImage") == true)
                    strSmallUrl = strID;
                else if (StringUtil.HasHead(strUsage, "FrontCover.MediumImage") == true)
                    strMediumUrl = strID;
                else if (StringUtil.HasHead(strUsage, "FrontCover.LargeImage") == true)
                    strLargeUrl = strID;
                else if (StringUtil.HasHead(strUsage, "FrontCover") == true)
                    strUrl = strID;

            }

            if (string.IsNullOrEmpty(strLargeUrl) == false)
                return strLargeUrl;
            if (string.IsNullOrEmpty(strMediumUrl) == false)
                return strMediumUrl;
            if (string.IsNullOrEmpty(strUrl) == false)
                return strUrl;
            return strSmallUrl;
        }

        class DpNs
        {
            public const string dprms = "http://dp2003.com/dprms";
            public const string dpdc = "http://dp2003.com/dpdc";
            public const string unimarcxml = "http://dp2003.com/UNIMARC";
        }
    }


    public class IssueString
    {
        // 用于显示
        public string Volume { get; set; }
        // 用于查询
        public string Query { get; set; }

        public IssueString(string volume, string query)
        {
            this.Volume = volume;
            this.Query = query;
        }
    }

}
