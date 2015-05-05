using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 违约/交费 信息控件
    /// </summary>
    [ToolboxData("<{0}:FellBackInfoControl runat=server></{0}:FellBackInfoControl>")]
    public class FellBackInfoControl : ReaderInfoBase
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.FellBackInfoControl.cs",
                typeof(FellBackInfoControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }


        protected override void RenderContents(HtmlTextWriter output)
        {
            // output.Write(Text);

            string strError = "";


            // return:
            //      -1  出错
            //      0   成功
            //      1   尚未登录
            int nRet = this.LoadReaderXml(out strError);
            if (nRet == -1)
            {
                output.Write(strError);
                return;
            }

            if (nRet == 1)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            // 读者类别		
            string strReaderType = DomUtil.GetElementText(ReaderDom.DocumentElement, "readerType");

            string strResult = "";

            // 超期记录
            XmlNodeList nodes = ReaderDom.DocumentElement.SelectNodes("overdues/overdue");

            strResult += this.GetPrefixString(
                this.GetString("违约交费信息"),    // "违约/交费信息"
                null);
            strResult += "<table class='fellbackinfo'>";
            strResult += "<tr class='columntitle'><td>"
                + this.GetString("册条码号")
                + "</td><td>"
                + this.GetString("书目摘要")
                + "</td><td>"
                + this.GetString("说明")
                + "</td><td>"
                + this.GetString("金额")
                + "</td><td>"
                + this.GetString("借阅情况")
                + "</td><td>"
                + this.GetString("ID")
                + "</td></tr>";

            if (nodes.Count == 0)
            {
                strResult += "<tr class='dark' >";
                strResult += "<td class='comment' colspan='6'>"
                    + this.GetString("无违约交费信息")    // "(无违约/交费信息)"
                    + "</td>";
                strResult += "</tr>";
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strReason = DomUtil.GetAttr(node, "reason");
                string strBorrowDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "borrowDate"));
                string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strReturnDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "returnDate"));
                string strID = DomUtil.GetAttr(node, "id");
                string strPrice = DomUtil.GetAttr(node, "price");
                string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");

                string strPriceString = DomUtil.GetAttr(node, "priceString");

                string strBarcodeLink = "<a href='" + app.LibraryServerUrl + "/book.aspx?barcode=" + strBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";

#if NO
                // 获得摘要
                string strSummary = "";

                if (String.IsNullOrEmpty(strBarcode) == false)  // 2009/4/16 new add
                {
                    string strBiblioRecPath = "";
                    long lRet = sessioninfo.Channel.GetBiblioSummary(
                        null,
                        strBarcode,
                        null,
                        null,
                        out strBiblioRecPath,
                        out strSummary,
                        out strError);
                    if (lRet == -1 || lRet == 0)
                        strSummary = strError;
                    /*
                    LibraryServerResult result = app.GetBiblioSummary(
                        sessioninfo,
                        strBarcode,
                        null,
                        null,
                        out strBiblioRecPath,
                        out strSummary);
                    if (result.Value == -1 || result.Value == 0)
                        strSummary = result.ErrorInfo;
                     * */
                }
#endif

                if (String.IsNullOrEmpty(strPriceString) == false)
                    strPrice = strPriceString;

                string strTrClass = " class='dark' ";

                if ((i % 2) == 1)
                    strTrClass = " class='light' ";

                strResult += "<tr " + strTrClass + ">";
                strResult += "<td class='barcode' nowrap>" + strBarcodeLink + "</td>";
                strResult += "<td class='summary pending' >" + strBarcode + "</td>";
                strResult += "<td class='reason' >" + strReason + "</td>";
                strResult += "<td class='price'>" + strPrice + "</td>";
                strResult += "<td class='borrowinfo'>"
                    + "<div class='borrowdate'>"
                    + this.GetString("借阅日期")
                    + ":" + strBorrowDate + "</div>"
                    + "<div class='borrowperiod'>"
                    + this.GetString("期限")
                    + ":    " + strBorrowPeriod + "</div>"
                    + "<div class='returndate'>"
                    + this.GetString("还书日期")
                    + ":" + strReturnDate + "</div>"
                    + "</td>";
                strResult += "<td class='id'>" + strID + "</td>";
                strResult += "</tr>";

            }

            XmlNode nodeOverdues = ReaderDom.DocumentElement.SelectSingleNode("overdues");
            string strPauseMessage = "";
            if (nodeOverdues != null)
                strPauseMessage = DomUtil.GetAttr(nodeOverdues, "pauseMessage");

            if (string.IsNullOrEmpty(strPauseMessage) == false)
            {
                // 汇报以停代金情况
                    strResult += "<td class='price' colspan='6'>"
                        + strPauseMessage
                        + " ("
                        + this.GetString("什么叫以停代金")  // "什么叫“<a href='./pauseBorrowing.html'>以停代金</a>”?"
                        + ")</td>";
            }

            strResult += "</table>";

            strResult += this.GetPostfixString();

            output.Write(strResult);
        }

    }
}
