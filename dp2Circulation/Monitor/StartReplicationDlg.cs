using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.Text;
using System.Collections;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 用于启动 dp2Library 同步 后台任务 的对话框
    /// </summary>
    internal partial class StartReplicationDlg : Form
    {
        /// <summary>
        /// 后台任务启动参数
        /// </summary>
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        /// <summary>
        /// 构造函数
        /// </summary>
        public StartReplicationDlg()
        {
            InitializeComponent();
        }

        private void StartReplicationDlg_Load(object sender, EventArgs e)
        {
            // 起始位置参数
            long index = 0;
            string strFileName = "";
            string strServer = "";
            string strError = "";

            int nRet = ParseStart(this.StartInfo.Start,
                out index,
                out strFileName,
                out strServer,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strFileName == "continue")
            {
                this.tabControl1.SelectedTab = this.tabPage_continue;
            }
            else if (strFileName == "copy_and_continue")
            {
                this.tabControl1.SelectedTab = this.tabPage_copyAndRep;
            }
            else
            {
                this.tabControl1.SelectedTab = this.tabPage_specDay;

                // this.comboBox_startServer.Text = strServer;
                this.textBox_startFileName.Text = strFileName;
                // this.textBox_startIndex.Text = index.ToString();
            }

            // 通用启动参数
            // string strLevel = "";
            bool bClearFirst = false;

            nRet = ParseTaskParam(this.StartInfo.Param,
                // out strLevel,
                out bClearFirst,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // this.comboBox_replicationLevel.Text = strLevel;
            this.checkBox_clearBefore.Checked = bClearFirst;


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartReplicationDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            /*
            if (this.comboBox_replicationLevel.Text == "")
            {
                MessageBox.Show(this, "尚未指定 同步级别");
                return;
            }
             * */

            // 合成参数
            if (this.tabControl1.SelectedTab == this.tabPage_specDay)
            {
                if (this.textBox_startFileName.Text == "")
                {
                    MessageBox.Show(this, "尚未指定 日志文件名");
                    return;
                }

                this.StartInfo.Start = BuildStart(
                        "", // this.textBox_startIndex.Text,
                        this.textBox_startFileName.Text,
                        "" /*this.comboBox_startServer.Text*/);
            }
            else if (this.tabControl1.SelectedTab == this.tabPage_continue)
            {
                this.StartInfo.Start = BuildStart(
                    "",
                    "continue",
                    "");
            }
            else
            {
                this.StartInfo.Start = BuildStart(
                    "",
                    "copy_and_continue",
                    "");
            }

            // 通用启动参数
#if NO
            string strLevel = this.comboBox_replicationLevel.Text;
            int nRet = strLevel.IndexOf('(');
            if (nRet != -1)
                strLevel = strLevel.Substring(0, nRet).Trim();
#endif

            this.StartInfo.Param = BuildTaskParam(/*strLevel, */this.checkBox_clearBefore.Checked);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 解析 开始 参数
        static int ParseStart(string strStart,
            out long index,
            out string strDate,
            out string strServer,
            out string strError)
        {
            strError = "";
            index = 0;
            strDate = "";
            strServer = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strStart);
            string strIndex = (string)table["index"];
            if (string.IsNullOrEmpty(strIndex) == true)
                index = 0;
            else
            {
                if (long.TryParse(strIndex, out index) == false)
                {
                    strError = "index 参数值 '" + strIndex + "' 不合法，应为纯数字";
                    return -1;
                }
            }

            strDate = (string)table["date"];

            strServer = (string)table["server"];

            return 0;
        }

        // 构造开始参数，也是断点字符串
        static string BuildStart(
            string strIndex,
            string strDate,
            string strServer)
        {
            Hashtable table = new Hashtable();
            table["index"] = strIndex;
            table["date"] = strDate;
            table["server"] = strServer;

            return StringUtil.BuildParameterString(table);
        }

        static int ParseTaskParam(string strParam,
            // out string strLevel,
            out bool bClearFirst,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            // strLevel = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            // strLevel = (string)table["level"];

            string strClearFirst = (string)table["clear_first"];
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            return 0;
        }

        static string BuildTaskParam(
            // string strLevel,
            bool bClearFirst)
        {
            Hashtable table = new Hashtable();
            // table["level"] = strLevel;
            table["clear_first"] = bClearFirst ? "yes" : "no";
            return StringUtil.BuildParameterString(table);
        }
    }
}