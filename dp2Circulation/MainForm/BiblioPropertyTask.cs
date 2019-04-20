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
                        new string[] { "xml" },   // formats
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

                        // TODO: 对 BiblioInfo 的成员进行操作的时候，是否要 lock 一下对象?
                        string strXml = results[0];
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
                Program.MainForm.m_commentViewer.Text = "MARC内容 '" + this.BiblioInfo?.RecPath + "'";
                Program.MainForm.m_commentViewer.HtmlString = string.IsNullOrEmpty(this.HTML) ? "<html><body></body></html>" : this.HTML;
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
