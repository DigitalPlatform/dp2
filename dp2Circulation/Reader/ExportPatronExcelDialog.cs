using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

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

            string strOutputFileName = "";
            // return:
            //      -1  出错
            //      0   文件名不合法
            //      1   文件名合法
            int nRet = CheckExcelFileName(this.textBox_outputExcelFileName.Text,
                true,
                out strOutputFileName,
                out strError);
            if (nRet == -1 || nRet == 0)
                goto ERROR1;

            this.textBox_outputExcelFileName.Text = strOutputFileName;

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

        public bool PrintReaderBarcodeLabel
        {
            get
            {
                return this.checkBox_readerBarcodeLabel.Checked;
            }
            set
            {
                this.checkBox_readerBarcodeLabel.Checked = value;
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
                controls.Add(new ControlWrapper(this.checkBox_readerBarcodeLabel, false));
                controls.Add(new ControlWrapper(this.checkBox_overdueInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_borrowInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_chargingHistory, false));

                controls.Add(this.textBox_chargingHistory_dateRange);

                controls.Add(new ControlWrapper(this.checkBox_filter_notFilter, true));
                controls.Add(new ControlWrapper(this.checkBox_filter_borrowing, false));
                controls.Add(new ControlWrapper(this.checkBox_filter_overdue, false));
                controls.Add(new ControlWrapper(this.checkBox_filter_amerce, false));

                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl_main);
                controls.Add(this.textBox_outputExcelFileName);
                controls.Add(new ControlWrapper(this.checkBox_readerInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_readerBarcodeLabel, false));
                controls.Add(new ControlWrapper(this.checkBox_overdueInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_borrowInfo, true));
                controls.Add(new ControlWrapper(this.checkBox_chargingHistory, false));

                controls.Add(this.textBox_chargingHistory_dateRange);

                controls.Add(new ControlWrapper(this.checkBox_filter_notFilter, true));
                controls.Add(new ControlWrapper(this.checkBox_filter_borrowing, false));
                controls.Add(new ControlWrapper(this.checkBox_filter_overdue, false));
                controls.Add(new ControlWrapper(this.checkBox_filter_amerce, false));

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

        // return:
        //      -1  出错
        //      0   文件名不合法
        //      1   文件名合法
        public static int CheckExcelFileName(string strFileName,
            bool bAutoCorrect,
            out string strOutputFileName,
            out string strError)
        {
            strError = "";
            strOutputFileName = strFileName;

            try
            {
                if (bAutoCorrect == true && Path.HasExtension(strFileName) == false)
                {
                    string strPath = Path.GetDirectoryName(strFileName);
                    if (string.IsNullOrEmpty(strPath) == false)
                        strOutputFileName = strPath + "\\" + Path.GetFileNameWithoutExtension(strFileName) + ".xlsx";
                    else
                        strOutputFileName = Path.GetFileNameWithoutExtension(strFileName) + ".xlsx";

                    return 1;
                }

                string strExtension = Path.GetExtension(strFileName);
                if (strExtension == null)
                {
                    strError = "文件名 '" + strFileName + "' 的扩展名部分不合法。应该为 '.xlsx'";
                    return 0;
                }
                if (strExtension.ToLower() != ".xlsx")
                {
                    strError = "文件名 '" + strFileName + "' 的扩展名部分不合法。应该为 '.xlsx'";
                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "检测文件名的过程出错: " + ex.Message;
                return -1;
            }
        }

        private void checkBox_filter_notFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_filter_notFilter.Checked)
            {
                this.checkBox_filter_overdue.Enabled = false;
                this.checkBox_filter_amerce.Enabled = false;
                this.checkBox_filter_borrowing.Enabled = false;
            }
            else
            {
                this.checkBox_filter_overdue.Enabled = true;
                this.checkBox_filter_amerce.Enabled = true;
                this.checkBox_filter_borrowing.Enabled = true;
            }
        }

        public string Filtering
        {
            get
            {
                List<string> names = new List<string>();
                if (this.checkBox_filter_notFilter.Checked)
                    return "";
                if (this.checkBox_filter_amerce.Checked)
                    names.Add("amerce");
                if (this.checkBox_filter_borrowing.Checked)
                    names.Add("borrwing");
                if (this.checkBox_filter_overdue.Checked)
                    names.Add("overdue");
                return StringUtil.MakePathList(names, ",");
            }
            set
            {
                this.checkBox_filter_amerce.Checked = false;
                this.checkBox_filter_borrowing.Checked = false;
                this.checkBox_filter_overdue.Checked = false;

                if (string.IsNullOrEmpty(value))
                {
                    this.checkBox_filter_notFilter.Checked = true;
                    return;
                }

                List<string> names = StringUtil.SplitList(value);
                foreach (var name in names)
                {
                    if (name == "amerce")
                        this.checkBox_filter_amerce.Checked = true;
                    if (name == "borrowing")
                        this.checkBox_filter_borrowing.Checked = true;
                    if (name == "overdue")
                        this.checkBox_filter_overdue.Checked = true;
                }
            }
        }
    }
}
