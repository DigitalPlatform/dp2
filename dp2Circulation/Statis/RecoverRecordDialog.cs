using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class RecoverRecordDialog : Form
    {
        public List<string> Dates
        {
            get;
            set;
        }

        public List<string> RecPathList
        {
            get;
            set;
        }

        public RecoverRecordDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strStart = "";
            string strEnd = "";
            StringUtil.ParseTwoPart(this.textBox_dateRange.Text, "-",
                out strStart, 
                out strEnd);

            string strWarning = "";
            List<string> dates = null;
            int nRet = OperLogLoader.MakeLogFileNames(strStart,
                strEnd,
                false,  // 是否包含扩展名 ".log"
                out dates,
                out strWarning,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (checkBox_lastFirst.Checked)
            {
                // 排序。最新日期在前
                dates.Sort((a, b) =>
                {
                    return string.Compare(a, b) * -1;
                });
            }

            this.Dates = dates;

            this.RecPathList = StringUtil.SplitList(this.textBox_recPathList.Text, "\r\n");

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

        private void button_findDateRange_Click(object sender, EventArgs e)
        {
            GetOperLogFilenameDlg dlg = new GetOperLogFilenameDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Text = "请指定日志起止日期范围";
            dlg.DateRange = this.textBox_dateRange.Text;
            Program.MainForm.AppInfo.LinkFormState(dlg, "RecoverRecordDialog_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            this.textBox_dateRange.Text = dlg.DateRange;
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_dateRange);
                controls.Add(this.textBox_recPathList);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_dateRange);
                controls.Add(this.textBox_recPathList);
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
