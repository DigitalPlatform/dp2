using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 导出读者详情到 Excel 文件, 的对话框
    /// </summary>
    public partial class ExportPatronExcelDialog : Form
    {
        public ExportPatronExcelDialog()
        {
            InitializeComponent();
        }

        private void ExportPatronExcelDialog_Load(object sender, EventArgs e)
        {

        }

        private void ExportPatronExcelDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        public bool OverwritePrompt
        {
            get;
            set;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_outputExcelFileName.Text) == true)
            {
                strError = "尚未指定输出文件名";
                goto ERROR1;
            }

            // 提醒覆盖文件
            if (this.OverwritePrompt == true
                && File.Exists(this.FileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文件 '" + this.FileName + "' 已经存在。继续操作将覆盖此文件。\r\n\r\n请问是否要覆盖此文件? (OK 覆盖；Cancel 放弃操作)",
                    "导出读者详情",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_getOutputExcelFileName_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.textBox_outputExcelFileName.Text;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_outputExcelFileName.Text = dlg.FileName;
        }

        private void button_chargingHistory_getDateRange_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;
            DateTime start;
            DateTime end;

            nRet = Global.ParseTimeRangeString(this.textBox_chargingHistory_dateRange.Text,
                false,
                out start,
                out end,
                out strError);
            /*
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }*/

            TimeRangeDlg dlg = new TimeRangeDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.Text = "借阅历史日期范围";
            dlg.StartDate = start;
            dlg.EndDate = end;
            dlg.AllowStartDateNull = true;  // 允许起点时间为空
            dlg.AllowEndDateNull = true;  // 允许终点时间为空

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            this.textBox_chargingHistory_dateRange.Text = Global.MakeTimeRangeString(dlg.StartDate, dlg.EndDate);
        }

        public string FileName
        {
            get
            {
                return this.textBox_outputExcelFileName.Text;
            }
            set
            {
                this.textBox_outputExcelFileName.Text = value;
            }
        }

        public string ChargingHistoryDateRange
        {
            get
            {
                return this.textBox_chargingHistory_dateRange.Text;
            }
            set
            {
                this.textBox_chargingHistory_dateRange.Text = value;
            }
        }

        public bool ExportChargingHistory
        {
            get
            {
                return this.checkBox_chargingHistory.Checked;
            }
            set
            {
                this.checkBox_chargingHistory.Checked = value;
            }
        }

        public bool ExportReaderInfo
        {
            get
            {
                return this.checkBox_readerInfo.Checked;
            }
            set
            {
                this.checkBox_readerInfo.Checked = value;
            }
        }

        public bool ExportOverdueInfo
        {
            get
            {
                return this.checkBox_overdueInfo.Checked;
            }
            set
            {
                this.checkBox_overdueInfo.Checked = value;
            }
        }

        public bool ExportBorrowInfo
        {
            get
            {
                return this.checkBox_borrowInfo.Checked;
            }
            set
            {
                this.checkBox_borrowInfo.Checked = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_outputExcelFileName);
                // 此处的缺省值会被忽略
                controls.Add(new ControlWrapper(this.checkBox_readerInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_overdueInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_borrowInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_chargingHistory, false));

                controls.Add(this.textBox_chargingHistory_dateRange);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_outputExcelFileName);
                controls.Add(new ControlWrapper(this.checkBox_readerInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_overdueInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_borrowInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_chargingHistory, false));

                controls.Add(this.textBox_chargingHistory_dateRange);
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_borrowHistory_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_chargingHistory.Checked)
            {
                this.textBox_chargingHistory_dateRange.Enabled = true;
                this.button_chargingHistory_getDateRange.Enabled = true;
            }
            else
            {
                this.textBox_chargingHistory_dateRange.Enabled = false;
                this.button_chargingHistory_getDateRange.Enabled = false;
            }
        }

        private void checkBox_readerInfo_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_readerInfo.Checked == true
                || this.checkBox_overdueInfo.Checked == true
                || this.checkBox_borrowInfo.Checked == true)
            {
                // 在 tabpage 标题上显示一个打勾标记
            }
            else
            {
                // 清除打勾标记
            }
        }
    }
}
