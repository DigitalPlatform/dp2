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


namespace dp2Circulation
{
    /// <summary>
    /// 用于启动 服务器同步 后台任务 的对话框
    /// </summary>
    internal partial class StartServerReplicationDlg : Form
    {
        string _taskName = "服务器同步";
        public string TaskName
        {
            get
            {
                return this._taskName;
            }
            set
            {
                this._taskName = value;
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
        public StartServerReplicationDlg()
        {
            InitializeComponent();
        }

        private void StartLogRecoverDlg_Load(object sender, EventArgs e)
        {
            string strError = "";

            try
            {
                // 起始位置参数
                ServerReplicationStart start = ServerReplicationStart.FromString(this.StartInfo.Start);

                this.textBox_startFileName.Text = start.Date;
                this.textBox_startIndex.Text = start.Index.ToString();

                // 通用启动参数
                ServerReplicationParam param = ServerReplicationParam.FromString(this.StartInfo.Param);

                this.comboBox_recoverLevel.Text = param.RecoverLevel;
                this.checkBox_clearBefore.Checked = param.ClearFirst;
                this.checkBox_clearBefore.Checked = param.ContinueWhenError;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartLogRecoverDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.comboBox_recoverLevel.Visible == true
                && this.comboBox_recoverLevel.Text == "")
            {
                strError = "尚未指定 恢复级别";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_startFileName.Text) == false
                && this.textBox_startFileName.Text.Length != 8)
            {
                strError = "起始日期 应为 8 字符形态";
                goto ERROR1;
            }

            ServerReplicationStart start = new ServerReplicationStart();
            start.Date = this.textBox_startFileName.Text.Substring(0, 8);
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
                        strError = "记录序号 '" + this.textBox_startIndex.Text + "' 必须为纯数字";
                        goto ERROR1;
                    }
                }
                start.Index = index;
            }

            this.StartInfo.Start = start.ToString();

            // 通用启动参数
            string strRecoverLevel = this.comboBox_recoverLevel.Text;
            int nRet = strRecoverLevel.IndexOf('(');
            if (nRet != -1)
                strRecoverLevel = strRecoverLevel.Substring(0, nRet).Trim();

            ServerReplicationParam param = new ServerReplicationParam();
            param.RecoverLevel = strRecoverLevel;
            param.ClearFirst = this.checkBox_clearBefore.Checked;
            param.ContinueWhenError = this.checkBox_continueWhenError.Checked;

            this.StartInfo.Param = param.ToString();

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

#if NO
        // 解析 开始 参数
        static int ParseLogRecoverStart(string strStart,
            out long index,
            out string strFileName,
            out string strError)
        {
            strError = "";
            index = 0;
            strFileName = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            int nRet = strStart.IndexOf('@');
            if (nRet == -1)
            {
                try
                {
                    index = Convert.ToInt64(strStart);
                }
                catch (Exception)
                {
                    strError = "启动参数 '" + strStart + "' 格式错误：" + "如果没有@，则应为纯数字。";
                    return -1;
                }
                return 0;
            }

            try
            {
                index = Convert.ToInt64(strStart.Substring(0, nRet).Trim());
            }
            catch (Exception)
            {
                strError = "启动参数 '" + strStart + "' 格式错误：'" + strStart.Substring(0, nRet).Trim() + "' 部分应当为纯数字。";
                return -1;
            }


            strFileName = strStart.Substring(nRet + 1).Trim();
            return 0;
        }

        /// <summary>
        /// 解析日志恢复参数
        /// </summary>
        /// <param name="strParam">待解析的参数字符串</param>
        /// <param name="strRecoverLevel">日志恢复级别</param>
        /// <param name="bClearFirst">在恢复前是否清除现有的数据库记录</param>
        /// <param name="bContinueWhenError">出错后是否继续批处理</param>
        /// <param name="strError">错误信息。当本方法发生错误时</param>
        /// <returns>-1: 出错。错误信息在 strError 参数中返回；0: 成功</returns>
        public static int ParseLogRecoverParam(string strParam,
            out string strRecoverLevel,
            out bool bClearFirst,
            out bool bContinueWhenError,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            bContinueWhenError = false;
            strRecoverLevel = "";

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

            /*
            Logic = 0,  // 逻辑操作
            LogicAndSnapshot = 1,   // 逻辑操作，若失败则转用快照恢复
            Snapshot = 3,   // （完全的）快照
            Robust = 4, // 最强壮的容错恢复方式
             * */

            strRecoverLevel = DomUtil.GetAttr(dom.DocumentElement,
                "recoverLevel");
            string strClearFirst = DomUtil.GetAttr(dom.DocumentElement,
                "clearFirst");
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            // 2016/3/8
            bContinueWhenError = DomUtil.GetBooleanParam(dom.DocumentElement,
                "continueWhenError",
                false);

            return 0;
        }
#endif
    }
}