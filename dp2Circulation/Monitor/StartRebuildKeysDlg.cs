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
using DigitalPlatform.LibraryServer.Common;

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

            int nRet = RebuildKeysParam.ParseStart(this.StartInfo.Start,
                out strDbNameList,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            Debug.Assert(strDbNameList != null, "");
            this.textBox_dbNameList.Text = strDbNameList.Replace(",", "\r\n");

            // 通用启动参数
            bool bClearFirst = false;
            string strFunction = "";

            nRet = RebuildKeysParam.ParseTaskParam(
                this.StartInfo.Param,
                out strFunction,
                out bClearFirst,
                out bool quick_mode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (string.IsNullOrEmpty(strFunction) == false)
                this.comboBox_function.Text = strFunction;

            this.checkBox_quickMode.Checked = quick_mode;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void StartArriveMonitorDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 合成参数
            if (this.checkBox_startAtServerBreakPoint.Checked == true)
                this.StartInfo.Start = "";
            else
            {
                // 2020/6/30
                if (string.IsNullOrEmpty(this.textBox_dbNameList.Text))
                {
                    strError = "尚未指定数据库名";
                    goto ERROR1;
                }

                this.StartInfo.Start = RebuildKeysParam.BuildStart(this.textBox_dbNameList.Text.Replace("\r\n", ","));
            }

            if (this.checkBox_quickMode.Checked)
            {
                DialogResult result = MessageBox.Show(this,
    "警告: 快速模式重建检索点的收尾阶段，数据库会处于不可用状态。\r\n\r\n确实要使用快速模式重建检索点? ",
    "StartRebuildKeysDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
            }

            // 通用启动参数
            this.StartInfo.Param = RebuildKeysParam.BuildTaskParam(this.comboBox_function.Text,
                false,
                this.checkBox_quickMode.Checked);

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

        private void checkBox_startAtServerBreakPoint_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_startAtServerBreakPoint.Checked)
            {
                this.textBox_dbNameList.Text = "";
                this.textBox_dbNameList.ReadOnly = true;
                this.checkBox_quickMode.Enabled = false;
            }
            else
            {
                this.textBox_dbNameList.ReadOnly = false;
                this.checkBox_quickMode.Enabled = true;
            }
        }
    }
}