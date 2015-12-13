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
    internal partial class StartPatronReplicationDlg : Form
    {
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        public StartPatronReplicationDlg()
        {
            InitializeComponent();
        }

        private void StartDkywReplicationDlg_Load(object sender, EventArgs e)
        {
            // 起始位置参数
            string strRecordID = "";
            string strError = "";

            int nRet = ParseDkywReplicationStart(this.StartInfo.Start,
                out strRecordID,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strRecordID == "!breakpoint")
            {
                this.textBox_startIndex.Text = "";
                this.checkBox_startAtServerBreakPoint.Checked = true;
                // this.textBox_startIndex.Enabled = false;
            }
            else
            {
                this.textBox_startIndex.Text = strRecordID;
                this.checkBox_startAtServerBreakPoint.Checked = false;
            }

            // 通用启动参数
            bool bLoop = false;

            nRet = ParseDkywReplicationParam(this.StartInfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.checkBox_loop.Checked = bLoop;

            checkBox_startAtServerBreakPoint_CheckedChanged(null, null);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartDkywReplicationDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 解析 开始 参数
        // parameters:
        //      strStart    启动字符串。格式为XML
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        // return:
        //      -1  出错
        //      0   正确
        int ParseDkywReplicationStart(string strStart,
            out string strRecordID,
            out string strError)
        {
            strError = "";
            strRecordID = "";

            // int nRet = 0;

            if (String.IsNullOrEmpty(strStart) == true)
            {
                strRecordID = "1";
                return 0;
            }

            // 2009/7/16
            if (strStart == "!breakpoint")
            {
                strRecordID = strStart;
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

            XmlNode nodeLoop = dom.DocumentElement.SelectSingleNode("loop");
            if (nodeLoop != null)
            {
                strRecordID = DomUtil.GetAttr(nodeLoop, "recordid");
            }

            return 0;
        }

        // 解析通用启动参数
        // 格式
        /*
         * <root loop='...'/>
         * loop缺省为true
         * 
         * */
        public static int ParseDkywReplicationParam(string strParam,
            out bool bLoop,
            out string strError)
        {
            strError = "";
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

            // 缺省为true
            string strLoop = DomUtil.GetAttr(dom.DocumentElement,
    "loop");
            if (strLoop.ToLower() == "no"
                || strLoop.ToLower() == "false")
                bLoop = false;
            else
                bLoop = true;

            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 合成参数
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
            {
                this.StartInfo.Start = "!breakpoint";
                // 通用启动参数
                this.StartInfo.Param = MakeArriveMonitorParam(this.checkBox_loop.Checked);
            }
            else
            {
                if (this.textBox_startIndex.Text == "")
                {
                    strError = "尚未指定起点记录号";
                    this.textBox_startIndex.Focus();
                    goto ERROR1;
                }

                DialogResult result = MessageBox.Show(this,
                    "指定起点记录号的方式容易导致数据重复跟踪，对系统正常运行产生不利的影响。一般情况下，选择从服务器保留的断点开始处理为好。\r\n\r\n确实要继续(按照指定起点记录号的方式进行处理)?",
                    "StartDkywReplicationDlg",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;

                this.StartInfo.Start = MakeBreakPointString(this.textBox_startIndex.Text);

                // 通用启动参数
                this.StartInfo.Param = MakeArriveMonitorParam(this.checkBox_loop.Checked);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 构造断点字符串
        static string MakeBreakPointString(
            string strRecordID)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // <loop>
            XmlNode nodeLoop = dom.CreateElement("loop");
            dom.DocumentElement.AppendChild(nodeLoop);

            DomUtil.SetAttr(nodeLoop, "recordid", strRecordID);

            return dom.OuterXml;
        }

        public static string MakeArriveMonitorParam(
bool bLoop)
        {
            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            DomUtil.SetAttr(dom.DocumentElement, "loop",
                bLoop == true ? "yes" : "no");

            return dom.OuterXml;
        }

        private void checkBox_startAtServerBreakPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
            {
                this.textBox_startIndex.Enabled = false;
            }
            else
            {
                this.textBox_startIndex.Enabled = true;
            }

        }
    }
}