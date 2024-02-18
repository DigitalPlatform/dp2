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
using DigitalPlatform.LibraryServer.Common;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 用于启动 dp2Library 日志恢复 后台任务 的对话框
    /// </summary>
    internal partial class StartLogRecoverDlg : Form
    {
        string _taskName = "日志恢复";  // 日志恢复/创建 MongoDB 日志库/服务器同步
        public string TaskName
        {
            get
            {
                return this._taskName;
            }
            set
            {
                this._taskName = value;
                if (value == "创建 MongoDB 日志库")
                {
                    this.comboBox_recoverLevel.Visible = false;
                    this.label_recoverLevel.Visible = false;
                }
                if (value == "服务器同步")
                {
                    this.comboBox_recoverLevel.Visible = false;
                    this.label_recoverLevel.Visible = false;

                    this.checkBox_clearBefore.Visible = false;
                }
            }
        }

        /// <summary>
        /// 后台任务启动参数
        /// </summary>
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        /// <summary>
        /// 构造函数
        /// </summary>
        public StartLogRecoverDlg()
        {
            InitializeComponent();
        }

        private void StartLogRecoverDlg_Load(object sender, EventArgs e)
        {
            // 起始位置参数
            long index = 0;
            string strFileName = "";
            string strError = "";

            int nRet = LogRecoverStart.ParseLogRecoverStart(this.StartInfo.Start,
                out index,
                out strFileName,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_startFileName.Text = strFileName;
            this.textBox_startIndex.Text = index.ToString();

            // 通用启动参数
            string strRecoverLevel = "";
            bool bClearFirst = false;
            bool bContinueWhenError = false;

            nRet = LogRecoverParam.ParseLogRecoverParam(this.StartInfo.Param,
                out string strDirectory,
                out strRecoverLevel,
                out bClearFirst,
                out bContinueWhenError,
                out string strStyle,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_logDirectory.Text = strDirectory;
            this.comboBox_recoverLevel.Text = strRecoverLevel;
            this.checkBox_clearBefore.Checked = bClearFirst;
            this.checkBox_clearBefore.Checked = bContinueWhenError;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartLogRecoverDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.comboBox_recoverLevel.Visible == true
                && this.comboBox_recoverLevel.Text == "")
            {
                MessageBox.Show(this, "尚未指定 恢复级别");
                return;
            }

            try
            {
                this.StartInfo.Start = LogRecoverStart.Build(this.textBox_startFileName.Text,
                    this.textBox_startIndex.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }
#if REMOVED
            // 合成参数
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
                this.StartInfo.Start = index.ToString() + "@" + this.textBox_startFileName.Text;
            }
#endif
            // 通用启动参数
            string strRecoverLevel = "";

            if (this.comboBox_recoverLevel.Visible == true)
            {
                strRecoverLevel = this.comboBox_recoverLevel.Text;
                int nRet = strRecoverLevel.IndexOf('(');
                if (nRet != -1)
                    strRecoverLevel = strRecoverLevel.Substring(0, nRet).Trim();
            }
                string strDirectory = this.textBox_logDirectory.Text;



            this.StartInfo.Param = LogRecoverParam.Build(
strDirectory,
strRecoverLevel,
this.checkBox_clearBefore.Checked,
this.checkBox_continueWhenError.Checked,
"");

#if REMOVED
            // 通用启动参数
            string strRecoverLevel = this.comboBox_recoverLevel.Text;
            int nRet = strRecoverLevel.IndexOf('(');
            if (nRet != -1)
                strRecoverLevel = strRecoverLevel.Substring(0, nRet).Trim();
            string strDirectory = this.textBox_logDirectory.Text;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");
            if (this.comboBox_recoverLevel.Visible == true)
            {
                DomUtil.SetAttr(dom.DocumentElement,
                    "recoverLevel",
                    strRecoverLevel);
            }
            DomUtil.SetAttr(dom.DocumentElement,
                "clearFirst",
                (this.checkBox_clearBefore.Checked == true ? "yes" : "no"));

            DomUtil.SetAttr(dom.DocumentElement,
    "continueWhenError",
    (this.checkBox_continueWhenError.Checked == true ? "yes" : "no"));

            if (string.IsNullOrEmpty(strDirectory) == false)
                dom.DocumentElement.SetAttribute("directory", strDirectory);


            this.StartInfo.Param = dom.OuterXml;
#endif
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}