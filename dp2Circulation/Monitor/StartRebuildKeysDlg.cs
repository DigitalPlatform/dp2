using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    internal partial class StartRebuildKeysDlg : Form
    {
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        public StartRebuildKeysDlg()
        {
            InitializeComponent();
        }

        private void StartArriveMonitorDlg_Load(object sender, EventArgs e)
        {
            // 起始位置参数
            string strDbNameList = "";
            string strError = "";

            int nRet = ParseStart(this.StartInfo.Start,
                out strDbNameList,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Debug.Assert(strDbNameList != null, "");
            this.textBox_dbNameList.Text = strDbNameList.Replace(",", "\r\n");

#if NO
            // 通用启动参数
            bool bLoop = false;

            nRet = ParseArriveMonitorParam(this.StartInfo.Param,
                out bLoop,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartArriveMonitorDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        #region 参数字符串处理
        // 这些函数也被 dp2Library 前端使用

        // 解析 开始 参数
        // 参数原始存储的时候，为了避免在参数字符串中发生混淆，数据库名之间用 | 间隔
        static int ParseStart(string strStart,
            out string strDbNameList,
            out string strError)
        {
            strError = "";
            strDbNameList = "";

            if (String.IsNullOrEmpty(strStart) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strStart);
            strDbNameList = (string)table["dbnamelist"];
            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace("|", ",");
            return 0;
        }

        // 构造开始参数，也是断点字符串
        // 参数原始存储的时候，为了避免在参数字符串中发生混淆，数据库名之间用 | 间隔
        static string BuildStart(
            string strDbNameList)
        {
            if (string.IsNullOrEmpty(strDbNameList) == false)
                strDbNameList = strDbNameList.Replace(",", "|");

            Hashtable table = new Hashtable();
            table["dbnamelist"] = strDbNameList;

            return StringUtil.BuildParameterString(table);
        }

        // 解析通用启动参数
        public static int ParseTaskParam(string strParam,
            out string strLevel,
            out bool bClearFirst,
            out string strError)
        {
            strError = "";
            bClearFirst = false;
            strLevel = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            strLevel = (string)table["level"];

            string strClearFirst = (string)table["clear_first"];
            if (strClearFirst.ToLower() == "yes"
                || strClearFirst.ToLower() == "true")
                bClearFirst = true;
            else
                bClearFirst = false;

            return 0;
        }

        static string BuildTaskParam(
            string strLevel,
            bool bClearFirst)
        {
            Hashtable table = new Hashtable();
            table["level"] = strLevel;
            table["clear_first"] = bClearFirst ? "yes" : "no";
            return StringUtil.BuildParameterString(table);
        }

        #endregion

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 合成参数
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
                this.StartInfo.Start = "";
            else
                this.StartInfo.Start = BuildStart(this.textBox_dbNameList.Text.Replace("\r\n", ","));

#if NO
            // 通用启动参数
            this.StartInfo.Param = MakeArriveMonitorParam(this.checkBox_loop.Checked);
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