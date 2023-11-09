using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    public class ItemPropertyTask : PropertyTask
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

        public string DbType
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

            BiblioInfo info = this.BiblioInfo;
            string strRecPath = this.BiblioInfo.RecPath;

            if (string.IsNullOrEmpty(info.OldXml) == true
                && string.IsNullOrEmpty(info.NewXml) == true)   // 2023/4/1
            {
                lock (syncRoot)
                {
                    channel = Program.MainForm.GetChannel();
                }
                try
                {
                    // 显示 正在处理
                    this.HTML = GetWaitingHtml("正在获取 "+this.DbType+" 记录 " + strRecPath);
                    ShowData();

                    byte [] baTimestamp = null;
                    string strOutputRecPath = "";
                    string strBiblio = "";
                    string strBiblioRecPath = "";
                    string strXml = "";

                    // 获得记录
                    channel.Timeout = LibraryChannel.MinTimeout;
                    long lRet = 0;
                    if (this.DbType == "item")
                    {
                        lRet = channel.GetItemInfo(
             this.Stop,
             "@path:" + strRecPath,
             "xml",
             out strXml,
             out strOutputRecPath,
             out baTimestamp,
             "",
             out strBiblio,
             out strBiblioRecPath,
             out strError);
                    }
                    else if (this.DbType == "order")
                    {
                        lRet = channel.GetOrderInfo(
             this.Stop,
             "@path:" + strRecPath,
             "xml",
             out strXml,
             out strOutputRecPath,
             out baTimestamp,
             "",
             out strBiblio,
             out strBiblioRecPath,
             out strError);
                    }
                    else if (this.DbType == "issue")
                    {
                        lRet = channel.GetIssueInfo(
             this.Stop,
             "@path:" + strRecPath,
             "xml",
             out strXml,
             out strOutputRecPath,
             out baTimestamp,
             "",
             out strBiblio,
             out strBiblioRecPath,
             out strError);
                    }
                    else if (this.DbType == "comment")
                    {
                        lRet = channel.GetCommentInfo(
             this.Stop,
             "@path:" + strRecPath,
             "xml",
             out strXml,
             out strOutputRecPath,
             out baTimestamp,
             "",
             out strBiblio,
             out strBiblioRecPath,
             out strError);
                    }
                    else if (this.DbType == "patron")
                    {
                        string[] results = null;
                        // 获得读者记录
                        lRet = channel.GetReaderInfo(
            this.Stop,
            "@path:" + strRecPath,
            "xml",
            out results,
            out strOutputRecPath,
            out baTimestamp,
            out strError);
                        if (lRet == 1)
                        {
                            if (results == null || results.Length == 0)
                            {
                                strError = "results error";
                                nRet = -1;
                            }
                            else
                                strXml = results[0];
                        }
                    }
                    else
                    {
                        lRet = -1;
                        strError = "无法识别的 DbType '"+this.DbType+"'";
                    }

                    if (lRet == 0)
                    {
                        nRet = -1;
                        strError = "获取记录 " + strRecPath + " 时出错: " + strError;
                    }
                    else if (lRet == -1)
                    {
                        nRet = -1;
                        strError = "获取记录 " + strRecPath + " 时出错: " + strError;
                    }
                    else
                    {
                        info.OldXml = strXml;
                        info.Timestamp = baTimestamp;
                        info.RecPath = strRecPath;
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

            if (nRet == -1)
            {
                strHtml2 = HttpUtility.HtmlEncode(strError);
            }
            else
            {
                nRet = ItemSearchForm.GetXmlHtml(info,
                    out strXml1,
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

            this.XML = strXml1;
            return true;
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
                Program.MainForm.m_commentViewer.Text = "记录内容 '" + this.BiblioInfo.RecPath + "'";
                Program.MainForm.m_commentViewer.HtmlString = this.HTML;
                Program.MainForm.m_commentViewer.XmlString = this.XML;
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
