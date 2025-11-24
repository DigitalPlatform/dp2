// *** 出纳流水 ***
// 编写者:谢涛
// 2012/10/6 创建
// 2012/10/11 为表格增加了详情栏
// 2012/10/29 借和还的行显示为不同的底色
// 2025/11/17 改为内置统计方案

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Drawing;	// Size

using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.dp2.Statis;

namespace dp2Circulation.defaultHosts
{
    /// <summary>
    /// 内置的统计方案 出纳流水
    /// </summary>
    public class ChargingDetailStatis : OperLogStatis
    {
        // 统计表集合
        TimeRangedStatisTableCollection Tables = null;

        int nMaxColumn = 6;

        bool m_bOutputAllInOneTable = false;
        bool m_bOutputYearTable = false;
        bool m_bOutputMonthTable = false;
        bool m_bOutputDayTable = false;

        public override void OnBegin(object sender, StatisEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ProjectDir))
                throw new ArgumentException("this.ProjectDir 不应为空");

            this.OperLogFilter = "borrow,return";

            // 获得输入参数
            HtmlInputDialog window = new HtmlInputDialog();

            window.Text = "指定统计特性";
            window.Url = this.ProjectDir + "\\input.html";
            window.Size = new Size(700, 500);
            window.ShowDialog(this.OperLogStatisForm);
            if (window.DialogResult != DialogResult.OK)
            {
                e.Continue = ContinueType.SkipAll;
                return;
            }

            // MessageBox.Show(window.SubmitUrl);


            if (window.SubmitUrl == "action://ok/")
            {
                // MessageBox.Show("[" + window.SubmitResult["OutputAllInOneTable"] + "]");

                this.m_bOutputAllInOneTable = (window.SubmitResult["OutputAllInOneTable"] == "true");
                this.m_bOutputYearTable = (window.SubmitResult["OutputYearTable"] == "true");
                this.m_bOutputMonthTable = (window.SubmitResult["OutputMonthTable"] == "true");
                this.m_bOutputDayTable = (window.SubmitResult["OutputDayTable"] == "true");
            }
            else
            {
                e.Continue = ContinueType.SkipAll;
                return;
            }

            this.ClearConsoleForPureTextOutputing();

            this.Tables = new TimeRangedStatisTableCollection(nMaxColumn,
                this.m_bOutputAllInOneTable,
                this.m_bOutputYearTable,
                this.m_bOutputMonthTable,
                this.m_bOutputDayTable
                );
        }

        public override void OnRecord(object sender, StatisEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(this.Xml);
            }
            catch (Exception ex)
            {
                strError = "Load Xml to DOM error: " + ex.Message;
                goto ERROR1;
            }

            this.WriteTextToConsole(this.CurrentLogFileName + ":" + this.CurrentRecordIndex.ToString() + "\r\n");

            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
            string strAction = DomUtil.GetElementText(dom.DocumentElement, "action");

            // 调整旧版本日志记录中缺乏精确<action>类型renew的问题
            if (strOperation == "borrow" && strAction == "")
            {
                string strNo = DomUtil.GetElementText(dom.DocumentElement, "no");

                if (strNo != "0")
                    strAction = "renew";
            }

            string strOperationName = "";

            if (strOperation == "borrow" && strAction != "renew")
                strOperationName = "借";
            else if (strOperation == "borrow" && strAction == "renew")
                strOperationName = "续借";
            else if (strOperation == "return" && strAction == "return")
                strOperationName = "还";
            else if (strOperation == "return" && strAction == "lost")
                strOperationName = "声明丢失";
            else
                return;

            string strOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            if (strReaderBarcode == strOperator)
                strOperator = "读者";

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");

            string strOperTime = GetRfc1123DisplayString(
                DomUtil.GetElementText(dom.DocumentElement, "operTime"));

            string strDetail = "";
            if (strOperation == "borrow")
            {
                string strBorrowDate = GetRfc1123DisplayString(
                    DomUtil.GetElementText(dom.DocumentElement, "borrowDate"));
                string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");

                strDetail = "期限: " + strBorrowPeriod;
            }
            else if (strOperation == "return")
            {
                string strOverdues = DomUtil.GetElementText(dom.DocumentElement, "overdues");
                if (string.IsNullOrEmpty(strOverdues) == true)
                    goto SKIP1;

                XmlDocument frag_dom = new XmlDocument();
                frag_dom.LoadXml("<root />");
                try
                {
                    frag_dom.DocumentElement.InnerXml = strOverdues;
                }
                catch (Exception ex)
                {
                    strDetail = "overdues片断装入XMLDOM出错: " + ex.Message;
                    goto SKIP1;
                }

                foreach (XmlNode node in frag_dom.DocumentElement.ChildNodes)
                {
                    string strHtml = "";
                    nRet = AmerceForm.GetOverdueInfoString(node,
                        false,
                        out strHtml,
                        out strError);
                    if (nRet == -1)
                    {
                        strDetail = strError;
                        goto SKIP1;
                    }

                    strDetail += "<p>有违约金：</p>" + strHtml;
                }
            }

        SKIP1:

            string strItemIndex = this.CurrentLogFileName + ":" + this.CurrentRecordIndex.ToString();

            // 操作 列号0
            this.Tables.SetValue(CurrentDate, strItemIndex, 0, strOperationName);

            // 读者 列号1
            this.Tables.SetValue(CurrentDate, strItemIndex, 1, strReaderBarcode + " : " + this.OperLogStatisForm.GetPatronSummary(strReaderBarcode));

            // 册 列号2
            this.Tables.SetValue(CurrentDate, strItemIndex, 2, strItemBarcode + " : " + this.OperLogStatisForm.GetItemSummary(strItemBarcode, 40));

            // 详情 列号3
            this.Tables.SetValue(CurrentDate, strItemIndex, 3, strDetail);

            // 操作时间 列号4
            this.Tables.SetValue(CurrentDate, strItemIndex, 4, strOperTime);

            // 操作者 列号5
            this.Tables.SetValue(CurrentDate, strItemIndex, 5, strOperator);

            return;
        ERROR1:
            DialogResult result = MessageBox.Show(this.OperLogStatisForm,
    strError + "\r\n\r\n是否继续处理?",
    "统计",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
            if (result == DialogResult.No)
                e.Continue = ContinueType.SkipAll;
        }

        public override void OnEnd(object sender, StatisEventArgs e)
        {
            if (this.Tables == null)
                return;

            for (int i = 0; i < this.Tables.Count; i++)
            {
                TimeRangedStatisTable table = this.Tables[i];

                table.Table.Sort(new ItemComparer());

                // 发生序号
                for (int j = 0; j < table.Table.Count; j++)
                {
                    Line line = table.Table[j];
                    line.Entry = (j + 1).ToString();
                }

                Report report = Report.BuildReport(table.Table,
                "序号||index,操作||operation,读者||reader,册||item,详情||detail,操作时间||opertime,操作者||operator",
                "&nbsp;",
                false);

                if (report == null) // 空表格
                    continue;

                report.OutputLine += new OutputLineEventHandler(report_outputline);

                string strHead = "<html><head>"
                    + "<meta http-equiv='Content-Type' content=\"text/html; charset=utf-8\">"
                    + "<title></title>"
                    + "<link rel='stylesheet' href='" + this.ProjectDir + "/style.css' type='text/css'>"
                    + "</head><body>"
                    + "<div class='tabletitle'>出纳流水<br/>" + table.TimeRangeName + "</div>";
                string strTail = "</body></html>";

                string strHtml = strHead + report.HtmlTable(table.Table) + strTail;

                // this.WriteTextToConsole("</pre>" + strHtml);

                // 写入输出文件
                string strOutputFileName = this.NewOutputFileName();
                this.WriteToOutputFile(strOutputFileName,
                    strHtml,
                    Encoding.UTF8);
            }
        }

        void report_outputline(object sender, OutputLineEventArgs e)
        {
            string strOperationName = e.Line.GetString(0);
            if (strOperationName == "借")
                e.LineCssClass += " borrow";
            else if (strOperationName == "还")
                e.LineCssClass += " return";
        }

        public static string GetRfc1123DisplayString(string strRfc1123TimeString)
        {
            if (string.IsNullOrEmpty(strRfc1123TimeString) == true)
                return "";

            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123TimeString, "G");
            }
            catch (Exception ex)
            {
                return "解析 RFC1123 时间字符串 '" + strRfc1123TimeString + "' 时出错: " + ex.Message;
            }
        }
    }

    public class ItemComparer : IComparer<Line>
    {
        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer<Line>.Compare(Line line1, Line line2)
        {
            string strFileName1 = "";
            string strIndex1 = "";
            ParseEntry(line1.Entry,
            out strFileName1,
            out strIndex1);

            string strFileName2 = "";
            string strIndex2 = "";
            ParseEntry(line2.Entry,
            out strFileName2,
            out strIndex2);

            int nRet = string.Compare(strFileName1, strFileName2);
            if (nRet != 0)
                return nRet;

            int nMaxLength = Math.Max(strIndex1.Length, strIndex2.Length);
            strIndex1 = strIndex1.PadLeft(nMaxLength - strIndex1.Length, '0');
            strIndex2 = strIndex2.PadLeft(nMaxLength - strIndex2.Length, '0');

            return string.Compare(strIndex1, strIndex2);
        }

        void ParseEntry(string strText,
            out string strFileName,
            out string strIndex)
        {
            int nRet = strText.IndexOf(":");
            if (nRet == -1)
            {
                strFileName = strText;
                strIndex = "";
                return;
            }

            strFileName = strText.Substring(0, nRet).Trim();
            strIndex = strText.Substring(nRet + 1).Trim();
        }
    }
}


