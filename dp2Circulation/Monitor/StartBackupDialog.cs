using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    public partial class StartBackupDialog : Form
    {
        public BatchTaskStartInfo StartInfo = new BatchTaskStartInfo();

        public StartBackupDialog()
        {
            InitializeComponent();
        }

        private void StartBackupDialog_Load(object sender, EventArgs e)
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

            // 通用启动参数
            string strFunction = "";

            nRet = ParseTaskParam(this.StartInfo.Param,
                out strFunction,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            //if (string.IsNullOrEmpty(strFunction) == false)
            //    this.comboBox_function.Text = strFunction;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
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
            out string strFunction,
            // out bool bClearFirst,
            out string strError)
        {
            strError = "";
            // bClearFirst = false;
            strFunction = "";

            if (String.IsNullOrEmpty(strParam) == true)
                return 0;

            Hashtable table = StringUtil.ParseParameters(strParam);
            strFunction = (string)table["function"];

            return 0;
        }

        static string BuildTaskParam(
            string strFunction)
        {
            Hashtable table = new Hashtable();
            table["function"] = strFunction;
            // table["clear_first"] = bClearFirst ? "yes" : "no";
            return StringUtil.BuildParameterString(table);
        }

        #endregion

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 合成参数
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
                this.StartInfo.Start = "continue";
            else
                this.StartInfo.Start = BuildStart(this.textBox_dbNameList.Text.Replace("\r\n", ","));

            // 通用启动参数
            this.StartInfo.Param = "";  // BuildTaskParam(this.comboBox_function.Text, false);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void checkBox_startAtServerBreakPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_startAtServerBreakPoint.Checked)
            {
                this.textBox_dbNameList.Text = "";
                this.textBox_dbNameList.Enabled = false;
            }
            else
            {
                this.textBox_dbNameList.Enabled = true;
            }
        }

    }
}
