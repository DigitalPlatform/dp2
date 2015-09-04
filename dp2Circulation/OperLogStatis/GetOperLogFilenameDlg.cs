using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.IO;
using DigitalPlatform.Text;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 用于获得单个日志文件名，或者范围日志文件名的对话框
    /// </summary>
    internal partial class GetOperLogFilenameDlg : Form
    {
        /// <summary>
        /// 是否为单行模式？
        /// </summary>
        public bool SingleMode = false; // 

        /// <summary>
        /// 输出所选择的文件名
        /// </summary>
        public List<string> OperLogFilenames = new List<string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public GetOperLogFilenameDlg()
        {
            InitializeComponent();
        }

        private void GetOperLogFilenameDlg_Load(object sender, EventArgs e)
        {
            // 获得初始化值
            if (this.OperLogFilenames != null
                && this.OperLogFilenames.Count >= 1)
            {
                string strDate = this.OperLogFilenames[0];
                if (strDate.Length > 8)
                    strDate = strDate.Substring(0, 8);

                try
                {
                    this.dateControl_start.Value = DateTimeUtil.Long8ToDateTime(strDate);
                }
                catch
                {
                }
            }

            if (this.SingleMode == true)
                label_start.Text = "日志所在日期(&D):";
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            OperLogFilenames.Clear();

            string strStartDate = "";
            
            if (this.dateControl_start.IsValueNull() == false)
                strStartDate = DateTimeUtil.DateTimeToString8(this.dateControl_start.Value);

            string strEndDate = "";
            
            if (this.dateControl_end.IsValueNull() == false)
                strEndDate = DateTimeUtil.DateTimeToString8(this.dateControl_end.Value);

            if (String.IsNullOrEmpty(strEndDate) == true
                && String.IsNullOrEmpty(strStartDate) == true)
            {
                strError = "尚未指定时间";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strEndDate) == true)
            {
                OperLogFilenames.Add(strStartDate + ".log");
                goto END1;
            }

            if (String.IsNullOrEmpty(strStartDate) == true)
            {
                OperLogFilenames.Add(strEndDate + ".log");
                goto END1;
            }

            string strWarning = "";
            List<string> LogFileNames = null;
            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            int nRet = OperLogStatisForm.MakeLogFileNames(strStartDate,
                strEndDate,
                true,
                out LogFileNames,
                out strWarning,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

            this.OperLogFilenames = LogFileNames;

            END1:
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

        void VisibleEndControls(bool bVisible)
        {
            this.label_end.Visible = bVisible;
            this.dateControl_end.Visible = bVisible;
        }

        private void dateControl_start_DateTextChanged(object sender, EventArgs e)
        {
            if (this.SingleMode == false
                && this.dateControl_start.IsValueNull() == false)
            {
                VisibleEndControls(true);
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this.dateControl_start.Value;
            }
            set
            {
                this.dateControl_start.Value = value;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return this.dateControl_end.Value;
            }
            set
            {
                this.dateControl_end.Value = value;
            }
        }

        public string DateRange
        {
            get
            {
                return DateTimeUtil.DateTimeToString8(this.dateControl_start.Value)
                    + "-" + DateTimeUtil.DateTimeToString8(this.dateControl_end.Value);
            }
            set
            {
                string strStart = "";
                string strEnd = "";
                StringUtil.ParseTwoPart(value, "-", out strStart, out strEnd);
                if (string.IsNullOrEmpty(strStart) == false)
                {
                    try
                    {
                        this.dateControl_start.Value = DateTimeUtil.Long8ToDateTime(strStart);
                    }
                    catch
                    {
                        this.dateControl_start.Value = DateTime.Now;
                    }
                }
                else
                    this.dateControl_start.Value = DateTime.Now;

                if (string.IsNullOrEmpty(strEnd) == false)
                {
                    try
                    {
                        this.dateControl_end.Value = DateTimeUtil.Long8ToDateTime(strEnd);
                    }
                    catch
                    {
                        this.dateControl_end.Value = DateTime.Now;
                    }
                }
                else
                    this.dateControl_end.Value = DateTime.Now;
            }
        }
    }
}