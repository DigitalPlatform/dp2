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
    internal partial class StartArriveMonitorDlg : Form
    {
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        public StartArriveMonitorDlg()
        {
            InitializeComponent();
        }

        private void StartArriveMonitorDlg_Load(object sender, EventArgs e)
        {
            // 起始位置参数
            string strRecordID = "";
            string strError = "";

            int nRet = ParseArriveMonitorStart(this.StartInfo.Start,
                out strRecordID,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_startIndex.Text = strRecordID;

            // 通用启动参数
            bool bLoop = false;

            nRet = ParseArriveMonitorParam(this.StartInfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.checkBox_loop.Checked = bLoop;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartArriveMonitorDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 解析 开始 参数
        // parameters:
        //      strStart    启动字符串。格式为XML
        //                  如果自动字符串为"!breakpoint"，表示从服务器记忆的断点信息开始
        int ParseArriveMonitorStart(string strStart,
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
        public static int ParseArriveMonitorParam(string strParam,
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
            // 警告一下“循环执行”被清除的后果。一般来说，不要这样启动“预约到书”后台任务
            if (this.checkBox_loop.Checked == false)
            {
                DialogResult result = MessageBox.Show(this,
    "确实要清除“循环执行”? \r\n\r\n注: 如果清除了“循环执行”，则本轮后台批处理任务执行完成后，不会自动定时运行。这将导致预约到书后台任务无法及时处理(除非手动启动预约到书后台任务, 或者重启 dp2library 服务器模块)",
    "StartArriveMonitorDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            // 合成参数
            this.StartInfo.Start = MakeBreakPointString(this.textBox_startIndex.Text);

            // 通用启动参数
            this.StartInfo.Param = MakeArriveMonitorParam(this.checkBox_loop.Checked);

            this.DialogResult = DialogResult.OK;
            this.Close();
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

            // 2023/10/25
            dom.DocumentElement.SetAttribute("activate", "true");
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
    }
}