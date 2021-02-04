using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    public class BiblioPropertyTask : PropertyTask
    {
        // public BiblioSearchForm BiblioSearchForm = null;

        public BiblioInfo BiblioInfo = null;

        public Stop Stop = null;

        public string HTML
        {
            get;
            set;
        }

        public string XML
        {
            get;
            set;
        }

        public string Subrecords
        {
            get;
            set;
        }

        // 打开对话框
        // return:
        //      false   无法打开对话框
        //      true    成功打开
        public override bool OpenWindow()
        {
            return true;
        }

        LibraryChannel channel = null;
        private static readonly Object syncRoot = new Object();

        // 装载数据
        public override bool LoadData()
        {
            string strError = "";
            int nRet = 0;

            string strSubRecords = "";
            BiblioInfo info = this.BiblioInfo;
            if (info == null)
                return true;
            string strRecPath = this.BiblioInfo.RecPath;

            if (string.IsNullOrEmpty(info.OldXml) == true)
            {
                lock (syncRoot)
                {
                    channel = Program.MainForm.GetChannel();
                }
                try
                {
                    // 显示 正在处理
                    this.HTML = GetWaitingHtml("正在获取书目记录 " + strRecPath);

                    ShowData();

                    // 获得书目记录
                    channel.Timeout = new TimeSpan(0, 0, 10);
                    long lRet = channel.GetBiblioInfos(
                        Stop,
                        strRecPath,
                        "",
                        new string[] { "xml", "subrecords:item" },   // formats
                        out string[] results,
                        out byte[] baTimestamp,
                        out strError);
                    if (lRet == 0)
                    {
                        nRet = -1;
                        strError = "获取书目记录 " + strRecPath + " 时出错: " + strError;
                    }
                    else if (lRet == -1)
                    {
                        nRet = -1;
                        strError = "获取书目记录 " + strRecPath + " 时出错: " + strError;
                    }
                    else
                    {
                        if (results == null || results.Length == 0)
                        {
                            strError = "results error";
                            throw new Exception(strError);
                        }

                        if (results != null && results.Length >= 2)
                            strSubRecords = results[1];

                        // TODO: 对 BiblioInfo 的成员进行操作的时候，是否要 lock 一下对象?
                        string strXml = results[0];
                        info.OldXml = strXml;
                        info.Timestamp = baTimestamp;
                        info.RecPath = strRecPath;

                        // 2021/2/4
                        info.Subrecords = strSubRecords;
                    }
                }
                finally
                {
                    LibraryChannel temp_channel = channel;
                    lock (syncRoot)
                    {
                        channel = null;
                    }
                    Program.MainForm.ReturnChannel(temp_channel);
                }
            }

            string strXml1 = "";
            string strHtml2 = "";
            string strXml2 = "";

            if (nRet == -1)
            {
                strHtml2 = HttpUtility.HtmlEncode(strError);
            }
            else
            {
                nRet = BiblioSearchForm.GetXmlHtml(info,
                    out strXml1,
                    out strXml2,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }

            this.HTML = "<html>" +
    Program.MainForm.GetMarcHtmlHeadString(true) +
    "<body>" +
    strHtml2 +
    EntityForm.GetTimestampHtml(info.Timestamp) +
    "</body></html>";

            this.XML = BiblioSearchForm.MergeXml(strXml1, strXml2);

            // 2021/2/4
            this.Subrecords = BuildSubrecordsHtml(info.Subrecords);

            return true;
        }

        static string BuildSubrecordsHtml(string strSubRecords)
        {
            if (string.IsNullOrEmpty(strSubRecords))
                return "";

            if (strSubRecords.StartsWith("error:"))
                return strSubRecords.Substring("error:".Length);

            StringBuilder text = new StringBuilder();
            XmlDocument collection_dom = new XmlDocument();
            try
            {
                text.AppendLine("<html>"+ Program.MainForm.GetSubrecordsHtmlHeadString() + "<body>");

                collection_dom.LoadXml(strSubRecords);

                string itemTotalCount = collection_dom.DocumentElement.GetAttribute("itemTotalCount");

                text.AppendLine("<table>");
                text.AppendLine($"<tr><td colspan='10'>册记录数: {itemTotalCount}</td></tr>");
                text.AppendLine("<tr class='columntitle'>");
                text.AppendLine("<td class='no'>序号</td>");
                text.AppendLine("<td class='location'>馆藏地</td>");
                text.AppendLine("<td class='price'>价格</td>");
                text.AppendLine("<td class='seller'>订购渠道</td>");
                text.AppendLine("<td class='source'>经费来源</td>");
                text.AppendLine("<td class='recpath'>记录路径</td>");
                text.AppendLine("</tr>");

                XmlNodeList nodes = collection_dom.DocumentElement.SelectNodes("item");
                int i = 0;
                foreach (XmlElement item in nodes)
                {
                    string rec_path = item.GetAttribute("recPath");
                    string location = DomUtil.GetElementText(item, "location");
                    string price = DomUtil.GetElementText(item, "price");
                    string seller = DomUtil.GetElementText(item, "seller");
                    string source = DomUtil.GetElementText(item, "source");

                    text.AppendLine("<tr class='content'>");
                    text.AppendLine($"<td class='no'>{(i+1)}</td>");
                    text.AppendLine($"<td class='location'>{HttpUtility.HtmlEncode(location)}</td>");
                    text.AppendLine($"<td class='price'>{HttpUtility.HtmlEncode(price)}</td>");
                    text.AppendLine($"<td class='seller'>{HttpUtility.HtmlEncode(seller)}</td>");
                    text.AppendLine($"<td class='source'>{HttpUtility.HtmlEncode(source)}</td>");
                    text.AppendLine($"<td class='recpath'>{HttpUtility.HtmlEncode(rec_path)}</td>");
                    text.AppendLine("</tr>");

                    i++;
                }

                Int32.TryParse(itemTotalCount, out int value);
                if (i < value)
                {
                    text.AppendLine($"<tr><td colspan='10'>... 有 {value - i} 项被略去 ...</td></tr>");
                }

                text.AppendLine("</table>");
                text.AppendLine("</body></html>");
                return text.ToString();
            }
            catch (Exception ex)
            {
                return "strSubRecords 装入 XMLDOM 时出现异常: "
                    + ex.Message
                    + "。(strSubRecords='" + StringUtil.CutString(strSubRecords, 300) + "')";
            }
        }

        // 从 collection 下级元素中获得指定元素名的部分
        static EntityInfo[] GetItems(XmlDocument collection_dom,
            string strElementName)
        {
            if (collection_dom.DocumentElement == null)
                return null;
            string strTotalCount = collection_dom.DocumentElement.GetAttribute(strElementName + "TotalCount");
            if (string.IsNullOrEmpty(strTotalCount))
                return null;
            int nTotalCount = 0;
            if (Int32.TryParse(strTotalCount, out nTotalCount) == false)
                return null;    // 2020/12/18
            XmlNodeList nodes = collection_dom.DocumentElement.SelectNodes(strElementName);
            if (nodes.Count < nTotalCount)
                return null;    // 迫使后面重新获取

            List<EntityInfo> results = new List<EntityInfo>();
            foreach (XmlElement node in nodes)
            {
                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(node.OuterXml);

                EntityInfo info = new EntityInfo();

                info.OldRecPath = node.GetAttribute("recPath");
                node.RemoveAttribute("recPath");

                info.OldTimestamp = ByteArray.GetTimeStampByteArray(node.GetAttribute("timestamp"));
                node.RemoveAttribute("timestamp");

                info.OldRecord = node.OuterXml;
                results.Add(info);
            }

            return results.ToArray();
        }


        public override bool ShowData()
        {
            if (this.Container == null
    || Program.MainForm == null)
                return false;

            if (Program.MainForm.InvokeRequired)
            {
                return (bool)Program.MainForm.Invoke(new Func<bool>(ShowData));
            }

            if (Program.MainForm.m_commentViewer != null)
            {
                Program.MainForm.m_commentViewer.Text = "MARC内容 '" + this.BiblioInfo?.RecPath + "'";
                Program.MainForm.m_commentViewer.HtmlString = string.IsNullOrEmpty(this.HTML) ? "<html><body></body></html>" : this.HTML;
                Program.MainForm.m_commentViewer.XmlString = this.XML;
                Program.MainForm.m_commentViewer.SubrecordsString = this.Subrecords;
                return true;
            }
            return false;
        }

        public override bool Cancel()
        {
#if NO
            lock (syncRoot)
            {
                if (channel != null)
                    channel.AbortIt();
            }
#endif
            return true;
        }
    }

}
