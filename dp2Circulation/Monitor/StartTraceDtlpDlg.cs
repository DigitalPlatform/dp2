using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 跟踪DTLP 
    /// 此对话框已经被废止
    /// </summary>
    internal partial class StartTraceDtlpDlg : Form
    {
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        public StartTraceDtlpDlg()
        {
            InitializeComponent();
        }

        private void StartTraceDtlpDlg_Load(object sender, EventArgs e)
        {
            // 起始位置参数
            int index = 0;
            string strFileName = "";
            string strError = "";
            int nRet = 0;

            if (this.StartInfo.Start == "!breakpoint")
                this.checkBox_startAtServerBreakPoint.Checked = true;
            else
            {
                string strTemp1;
                string strTemp2;
                string strLogStartOffset = "";
                nRet = ParseTraceDtlpStart(
                    this.StartInfo.Start,
                    out index,
                    out strLogStartOffset,
                    out strFileName,
                    out strTemp1,
                    out strTemp2,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_startFileName.Text = strFileName;
                this.textBox_startIndex.Text = index.ToString();
            }

            // 通用启动参数
            bool bDump = false;
            bool bClearFirst = false;
            bool bLoop = true;

            nRet = ParseTraceDtlpParam(this.StartInfo.Param,
                out bDump,
                out bClearFirst,
                out bLoop,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.checkBox_dump.Checked = bDump;
            this.checkBox_clearBefore.Checked = bClearFirst;
            this.checkBox_loop.Checked = bLoop;

            // 设置好初始的Enalbed状态
            checkBox_startAtServerBreakPoint_CheckedChanged(this, null);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartTraceDtlpDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 构造断点字符串
        static string MakeBreakPointString(
            long indexLog,
            string strLogStartOffset,
            string strLogFileName,
            string strRecordID,
            string strOriginDbName)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // <dump>
            XmlNode nodeDump = dom.CreateElement("dump");
            dom.DocumentElement.AppendChild(nodeDump);

            DomUtil.SetAttr(nodeDump, "recordid", strRecordID);
            DomUtil.SetAttr(nodeDump, "origindbname", strOriginDbName);

            // <trace>
            XmlNode nodeTrace = dom.CreateElement("trace");
            dom.DocumentElement.AppendChild(nodeTrace);

            DomUtil.SetAttr(nodeTrace, "index", indexLog.ToString());
            DomUtil.SetAttr(nodeTrace, "startoffset", strLogStartOffset);
            DomUtil.SetAttr(nodeTrace, "logfilename", strLogFileName);

            return dom.OuterXml;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 合成参数
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
            {
                this.StartInfo.Start = "!breakpoint";
            }
            else
            {
                if (this.textBox_startFileName.Text == "")
                    this.StartInfo.Start = "";
                else
                {
                    long index = 0;
                    if (this.textBox_startIndex.Text != "")
                    {
                        try
                        {
                            index = Convert.ToInt64(this.textBox_startIndex.Text);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(this, "记录 ID '" + this.textBox_startIndex.Text + "' 必须为纯数字");
                            return;
                        }
                    }

                    this.StartInfo.Start = MakeBreakPointString(
                        index,
                        "",
                        this.textBox_startFileName.Text,
                        "", //strRecordID,
                        "" //strOriginDbName
                        );
                }

            }



            // 通用启动参数
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            if (checkBox_startAtServerBreakPoint.Checked == true)
            {
                // 采用服务器断点时，dump值服务器自会决定，而clearFirst则为no，因为clear阶段是不可能中断的（既然从断点开始，就表明先前成功执行过了（如果必要的话））。
                DomUtil.SetAttr(dom.DocumentElement,
                    "dump",
                    "no");
                DomUtil.SetAttr(dom.DocumentElement,
                    "clearFirst",
                    "no");
            }
            else
            {
                DomUtil.SetAttr(dom.DocumentElement,
                    "dump",
                    (this.checkBox_dump.Checked == true ? "yes" : "no"));
                DomUtil.SetAttr(dom.DocumentElement,
                    "clearFirst",
                    (this.checkBox_clearBefore.Checked == true ? "yes" : "no"));
            }

            DomUtil.SetAttr(dom.DocumentElement,
                "loop",
                (this.checkBox_loop.Checked == true ? "yes" : "no"));

            this.StartInfo.Param = dom.OuterXml;


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        // 解析通用启动参数
        // 格式
        /*
         * <root dump='...' clearFirst='...' loop='...'/>
         * dump缺省为false
         * clearFirst缺省为false
         * loop缺省为true
         * 
         * 
         * */
        public static int ParseTraceDtlpParam(string strParam,
            out bool bDump,
            out bool bClearFirst,
            out bool bLoop,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bDump = false;
            bLoop = true;

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strParam);
            }
            catch (Exception ex)
            {
                strError = "strParam参数装入XML DOM时出错: " + ex.Message;
                return -1;
            }


            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            string strDump = DomUtil.GetAttr(dom.DocumentElement,
    "dump");
            if (strDump.ToLower() == "yes"
                || strDump.ToLower() == "true")
                bDump = true;
            else
                bDump = false;

            string strLoop = DomUtil.GetAttr(dom.DocumentElement,
"loop");
            if (strLoop.ToLower() == "yes"
                || strLoop.ToLower() == "true")
                bLoop = true;
            else
                bLoop = false;

            return 0;
        }


        static int ParseTraceDtlpStart(string strStart,
            out int indexLog,
            out string strLogStartOffset,
            out string strLogFileName,
            out string strRecordID,
            out string strOriginDbName,
            out string strError)
        {
            strError = "";
            indexLog = 0;
            strLogFileName = "";
            strLogStartOffset = "";
            strRecordID = "";
            strOriginDbName = "";

            // int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                DateTime now = DateTime.Now;
                // 当天的日志文件，以便捷输入而已
                strLogFileName = now.Year.ToString().PadLeft(4, '0')
                + now.Month.ToString().PadLeft(2, '0')
                + now.Day.ToString().PadLeft(2, '0');
                return 0;
            }

            if (strStart == "!breakpoint")
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strStart);
            }
            catch (Exception ex)
            {
                strError = "装载XML字符串进入DOM时发生错误: " + ex.Message;
                return -1;
            }

            XmlNode nodeDump = dom.DocumentElement.SelectSingleNode("dump");
            if (nodeDump != null)
            {
                strRecordID = DomUtil.GetAttr(nodeDump, "recordid");
                strOriginDbName = DomUtil.GetAttr(nodeDump, "origindbname");
            }

            XmlNode nodeTrace = dom.DocumentElement.SelectSingleNode("trace");
            if (nodeTrace != null)
            {
                string strIndex = DomUtil.GetAttr(nodeTrace, "index");
                if (String.IsNullOrEmpty(strIndex) == true)
                    indexLog = 0;
                else
                {
                    try
                    {
                        indexLog = Convert.ToInt32(strIndex);
                    }
                    catch
                    {
                        strError = "<trace>元素中index属性值 '" + strIndex + "' 格式错误，应当为纯数字";
                        return -1;
                    }
                }

                strLogStartOffset = DomUtil.GetAttr(nodeTrace, "startoffs");
                strLogFileName = DomUtil.GetAttr(nodeTrace, "logfilename");
            }

            return 0;
        }

        private void checkBox_startAtServerBreakPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
            {
                this.textBox_startFileName.Enabled = false;
                this.textBox_startIndex.Enabled = false;
                this.checkBox_clearBefore.Enabled = false;
                this.checkBox_dump.Enabled = false;
            }
            else
            {
                this.textBox_startFileName.Enabled = true;
                this.textBox_startIndex.Enabled = true;
                this.checkBox_clearBefore.Enabled = true;
                this.checkBox_dump.Enabled = true;
            }

        }
    }
}