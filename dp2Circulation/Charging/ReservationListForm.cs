
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 列出待办预约请求的窗口
    /// 针对工作人员掌握配书请求
    /// 针对读者个人藏书管理而设计
    /// </summary>
    public partial class ReservationListForm : MyForm
    {
        List<ReservationItem> _items = new List<ReservationItem>();

        WebExternalHost m_chargingInterface = new WebExternalHost();

        public ReservationListForm()
        {
            InitializeComponent();
        }

        private void ReservationListForm_Load(object sender, EventArgs e)
        {
            // webBrowser_borrowHistory
            this.m_chargingInterface.Initial(// Program.MainForm, 
                this.webBrowser1);
            //this.m_chargingInterface.GetLocalPath -= new GetLocalFilePathEventHandler(m_webExternalHost_GetLocalPath);
            //this.m_chargingInterface.GetLocalPath += new GetLocalFilePathEventHandler(m_webExternalHost_GetLocalPath);
            // this.m_chargingInterface.CallFunc += m_chargingInterface_CallFunc;
            this.webBrowser1.ObjectForScripting = this.m_chargingInterface;

            this.BeginInvoke(new Action(ListReservations));
        }

        private void ReservationListForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ReservationListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.m_chargingInterface != null)
                this.m_chargingInterface.Destroy();
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                this.toolStrip1.Enabled = bEnable;
            }));
        }

        class MyReaderInfo
        {
            public string ReaderXml = "";
            public string LibraryCode = "";
            public string PersonalLibrary = "";
            public string Name = "";
        }

        // 获得登录者的读者信息。登录者应为读者身份
        // return:
        //      -1  出错
        //      0   读者记录不存在
        //      1   成功
        int GetMyReaderInfo(out MyReaderInfo info,
            out string strError)
        {
            strError = "";
            info = new MyReaderInfo();

            string strUserName = Program.MainForm.AppInfo.GetString(
    "default_account",
    "username",
    "");
            bool bIsReader = Program.MainForm.AppInfo.GetBoolean(
    "default_account",
    "isreader",
    false);
            if (bIsReader == false)
            {
                strError = "当前用户不是读者身份，无法使用本窗口";
                return -1;
            }

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获得读者记录 ...");
            _stop.BeginLoop();

            EnableControls(false);

            try
            {
                _stop.SetMessage("正在装入读者记录 " + strUserName + " ...");

                string[] results = null;
                byte[] baTimestamp = null;
                string strRecPath = "";
                long lRet = Channel.GetReaderInfo(
                    _stop,
                    strUserName,
                    "xml",   // this.RenderFormat, // "html",
                    out results,
                    out strRecPath,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                {
                    strError = "证条码号为 '" + strUserName + "' 的读者记录没有找到 ...";
                    return 0;   // not found
                }
                if (lRet == -1)
                    return -1;

                if (results == null || results.Length == 0)
                {
                    strError = "返回的results不正常。";
                    return -1;
                }
                string strResult = "";
                strResult = results[0];

                if (lRet > 1)
                {
                    strError = "证条码号为 '" + strUserName + "' 的读者记录命中多条 (" + lRet + ") ...";
                    return -1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strResult);
                }
                catch (Exception ex)
                {
                    strError = "XML 装入 DOM 时出错" + ex.Message;
                    return -1;
                }

                info.ReaderXml = strResult;
                info.Name = DomUtil.GetElementText(dom.DocumentElement, "name");
                // 根据读者库查到馆代码
                info.LibraryCode = Program.MainForm.GetReaderDbLibraryCode(Global.GetDbName(strRecPath));
                info.PersonalLibrary = DomUtil.GetElementText(dom.DocumentElement, "personalLibrary");

            }
            finally
            {
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
            }

            return 1;
        }

        // 列出所有读者的所有预约到书信息
        int ListAllReservations(out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(Program.MainForm.ArrivedDbName) == true)
            {
                strError = "当前服务器尚未配置预约到书库名";
                return -1;
            }

            this._items.Clear();
            this.ClearHtml();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在检索预约到书记录 ...");
            _stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strQueryWord = "arrived";
                string strFrom = "状态";
                string strMatchStyle = "exact";
                string strQueryXml = "<target list='" + Program.MainForm.ArrivedDbName + ":"
                    + strFrom + "'><item><word>"
    + StringUtil.GetXmlStringSimple(strQueryWord)
    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + this.Lang + "</lang></target>";

                string strOutputStyle = "";
                long lRet = Channel.Search(_stop,
                    strQueryXml,
                    "",
                    strOutputStyle,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;

                _stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (_stop != null && _stop.State != 0)
                    {
                        strError = "中断";
                        return -1;
                    }

                    lRet = Channel.GetSearchResult(
                        _stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id,xml", // bOutputKeyCount == true ? "keycount" : "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error";
                        return -1;
                    }

                    if (lRet == 0)
                        return 0;

                    // List<string> paths = new List<string>();

                    int i = 0;
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                    {
                        ReservationItem item = new ReservationItem(record.Path,
                            record.RecordBody.Xml,
                            record.RecordBody.Timestamp);
                        this._items.Add(item);

                        // paths.Add(record.Path);
                        _stop.SetProgressValue(lStart + i);
                        i++;
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    _stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                    _stop.SetProgressValue(lStart);
                }
            }
            finally
            {
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
            }

            FillItems(this._items);
            return 1;
        }

        // 列出当前读者的所有预约到书信息
        int ListPersonReservations(MyReaderInfo info,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(info.PersonalLibrary) == true)
            {
                strError = "当前读者 '" + info.Name + "' 没有个人书斋";
                return -1;
            }

            if (string.IsNullOrEmpty(Program.MainForm.ArrivedDbName) == true)
            {
                strError = "当前服务器尚未配置预约到书库名";
                return -1;
            }

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在检索预约到书记录 ...");
            _stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strQueryWord = "";
                if (string.IsNullOrEmpty(info.LibraryCode) == true)
                    strQueryWord = info.PersonalLibrary;
                else
                    strQueryWord = info.LibraryCode + "/" + info.PersonalLibrary;
                string strFrom = "馆藏地点";
                string strMatchStyle = "exact";
                string strQueryXml = "<target list='" + Program.MainForm.ArrivedDbName + ":"
                    + strFrom + "'><item><word>"
    + StringUtil.GetXmlStringSimple(strQueryWord)
    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + this.Lang + "</lang></target>";

                string strOutputStyle = "";
                long lRet = Channel.Search(_stop,
                    strQueryXml,
                    "",
                    strOutputStyle,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;

                _stop.SetProgressRange(0, lHitCount);

                long lStart = 0;
                long lCount = lHitCount;
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (_stop != null && _stop.State != 0)
                    {
                        strError = "中断";
                        return -1;
                    }

                    lRet = Channel.GetSearchResult(
                        _stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id,xml", // bOutputKeyCount == true ? "keycount" : "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "GetSearchResult() error";
                        return -1;
                    }

                    if (lRet == 0)
                        return 0;

                    List<string> paths = new List<string>();

                    int i = 0;
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                    {
                        // record.RecordBody.Xml;
                        paths.Add(record.Path);
                        _stop.SetProgressValue(lStart + i);
                        i++;
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    _stop.SetMessage("共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条");

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                    _stop.SetProgressValue(lStart);
                }
            }
            finally
            {
                EnableControls(true);

                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
            }

            return 1;
        }

        void ListReservations()
        {
            string strError = "";

#if NO
            {
                MyReaderInfo info = null;
                // 获得登录者的读者信息。登录者应为读者身份
                // return:
                //      -1  出错
                //      0   读者记录不存在
                //      1   成功
                int nRet = GetMyReaderInfo(out info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 列出所有预约到书信息
                nRet = ListPersonReservations(info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
#endif
            // 列出所有预约到书信息
            int nRet = ListAllReservations(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #region HTML 操作

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "default\\charginghistory.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    this.webBrowser1.Navigate("about:blank");
                    doc = this.webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        void FillItems(List<ReservationItem> items)
        {
            this.ClearMessage();

            StringBuilder text = new StringBuilder();

            // string strBinDir = Environment.CurrentDirectory;
            string strBinDir = Program.MainForm.UserDir;    // 2017/2/23

            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "default\\charginghistory.css");
            string strSummaryJs = Path.Combine(Program.MainForm.DataDir, "getsummary.js");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strScriptHead = "<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-1.4.4.min.js\"></script>"
                + "<script type=\"text/javascript\" src=\"%bindir%/jquery/js/jquery-ui-1.8.7.min.js\"></script>"
                + "<script type='text/javascript' charset='UTF-8' src='" + strSummaryJs + "'></script>";
            string strStyle = @"<style type='text/css'>
</style>";
            text.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                + strLink
                + strScriptHead.Replace("%bindir%", strBinDir)
                + strStyle
                + "</head><body>");

            text.Append("<table>");
            text.Append("<tr>");
            text.Append("<td class='nowrap'>序号</td>");
            text.Append("<td class='nowrap'>状态</td>");
            text.Append("<td class='nowrap'>读者信息</td>");
            text.Append("<td class='nowrap'>册信息</td>");
            text.Append("<td class='nowrap'>馆藏地点</td>");
            text.Append("<td class='nowrap'>索取号</td>");
            text.Append("<td class='nowrap'>暂存位置</td>");
            text.Append("<td class='nowrap'>配书操作者</td>");
            text.Append("<td class='nowrap'>配书操作时间</td>");
            text.Append("</tr>");

            int nStart = 0;
            foreach (ReservationItem item in items)
            {
                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(item.Xml);

                // state
                string state = DomUtil.GetElementText(item_dom.DocumentElement,
                    "state");

                // itemBarcode or itemRefID
                string itemBarcode = DomUtil.GetElementText(item_dom.DocumentElement,
                    "itemBarcode");
                if (string.IsNullOrEmpty(itemBarcode))
                    itemBarcode = "@refID:" + DomUtil.GetElementText(item_dom.DocumentElement,
                    "itemRefID");

                // location
                string location = DomUtil.GetElementText(item_dom.DocumentElement,
    "location");

                // accessNo
                string accessNo = DomUtil.GetElementText(item_dom.DocumentElement,
    "accessNo");

                // readerBarcode or patronRefID
                string readerBarcode = DomUtil.GetElementText(item_dom.DocumentElement,
                    "readerBarcode");
                if (string.IsNullOrEmpty(readerBarcode))
                    itemBarcode = "@refID:" + DomUtil.GetElementText(item_dom.DocumentElement,
                    "patronRefID");

                // box
                string box = DomUtil.GetElementText(item_dom.DocumentElement,
    "box");
                // boxingOperator
                string boxingOperator = DomUtil.GetElementText(item_dom.DocumentElement,
    "boxingOperator");
                // boxingDate
                string boxingDate = DateTimeUtil.LocalTime(DomUtil.GetElementText(item_dom.DocumentElement,
    "boxingDate"), "u");

                text.Append("<tr class='" + HttpUtility.HtmlEncode("") + "'>");
                text.Append("<td>" + (nStart + 1).ToString() + "</td>");
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(state) + "</td>");

                text.Append("<td>");
                text.Append("<div class='nowrap'>" + HttpUtility.HtmlEncode(readerBarcode) + "</div>");
                text.Append("<div class='nowrap'>" + HttpUtility.HtmlEncode("") + "</div>");
                text.Append("</td>");

                text.Append("<td>");
                text.Append("<div>" + HttpUtility.HtmlEncode(itemBarcode) + "</div>");
                text.Append("<div class='summary pending'>BC:" + HttpUtility.HtmlEncode(itemBarcode) + "</div>");
                text.Append("</td>");

                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(location) + "</td>");

                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(accessNo) + "</td>");

                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(box) + "</td>");

                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(boxingOperator) + "</td>");
                text.Append("<td class='nowrap'>" + HttpUtility.HtmlEncode(boxingDate) + "</td>");

                text.Append("</tr>");
                nStart++;
            }
            text.Append("</table>");
            text.Append("</body></html>");

            this.m_chargingInterface.SetHtmlString(text.ToString(), "reservation");
        }

        #endregion

    }

    public class ReservationItem
    {
        public string RecPath { get; set; }
        public string Xml { get; set; }
        public byte[] Timestamp { get; set; }

        public ReservationItem(string recPath,
            string xml,
            byte[] timestamp)
        {
            this.RecPath = RecPath;
            this.Xml = xml;
            this.Timestamp = timestamp;
        }
    }
}
