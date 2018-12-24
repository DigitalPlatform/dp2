using System;
using System.Collections.Generic;
using System.Windows.Forms;

using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 时间切片对话框
    /// </summary>
    internal partial class DateSliceDialog : Form
    {
        // 是否为时间范围状态。
        // 在时间范围状态下，切片有关的界面要素会被隐藏
        public bool TimeRangeMode { get; set; }

        public DateSliceDialog()
        {
            InitializeComponent();
        }

        public DateTime OrderDate = new DateTime(0);

        public DateTime StartTime
        {
            get
            {
                return RoundStartTime(this.dateControl_start.Value);
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
                return RoundEndTime(this.dateControl_end.Value);
            }
            set
            {
                this.dateControl_end.Value = value;
            }
        }

        public string uTimeRange
        {
            get
            {
                string strStart = "";
                if (this.StartTime != DateTimePicker.MinimumDateTime)
                    strStart = this.StartTime.ToString("u");

                string strEnd = "";
                if (this.EndTime != DateTimePicker.MaximumDateTime)
                    strEnd = this.EndTime.ToString("u");

                return strStart
                    + "~"
                    + strEnd;
            }
            set
            {
                string strStart = "";
                string strEnd = "";

                int nRet = value.IndexOf("~");
                if (nRet == -1)
                    strStart = value.Trim();
                else
                {
                    strStart = value.Substring(0, nRet).Trim();
                    strEnd = value.Substring(nRet + 1).Trim();
                }

                if (string.IsNullOrEmpty(strStart) == true)
                    this.StartTime = DateTimePicker.MinimumDateTime;
                else
                    this.StartTime = DateTimeUtil.FromUTimeString(strStart);

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.EndTime = DateTimePicker.MaximumDateTime;
                else
                    this.EndTime = DateTimeUtil.FromUTimeString(strEnd);
            }
        }

        public string Rfc1123TimeRange
        {
            get
            {
                string strStart = "";
                if (this.StartTime != DateTimePicker.MinimumDateTime)
                    strStart = DateTimeUtil.Rfc1123DateTimeStringEx(this.StartTime);

                string strEnd = "";
                if (this.EndTime != DateTimePicker.MaximumDateTime)
                    strEnd = DateTimeUtil.Rfc1123DateTimeStringEx(this.EndTime);

                return strStart
                    + "~"
                    + strEnd;
            }
            set
            {
                string strStart = "";
                string strEnd = "";

                int nRet = value.IndexOf("~");
                if (nRet == -1)
                    strStart = value.Trim();
                else
                {
                    strStart = value.Substring(0, nRet).Trim();
                    strEnd = value.Substring(nRet + 1).Trim();
                }

                if (string.IsNullOrEmpty(strStart) == true)
                    this.StartTime = DateTimePicker.MinimumDateTime;
                else
                    this.StartTime = DateTimeUtil.FromRfc1123DateTimeString(strStart).ToLocalTime();

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.EndTime = DateTimePicker.MaximumDateTime;
                else
                    this.EndTime = DateTimeUtil.FromRfc1123DateTimeString(strEnd).ToLocalTime();
            }
        }


        public string Slice
        {
            get
            {
                return this.comboBox_slice.Text;
            }
            set
            {
                this.comboBox_slice.Text = value;
            }
        }

        public string QuickSet
        {
            get
            {
                return this.comboBox_quickSetTimeRange.Text;
            }
            set
            {
                this.comboBox_quickSetTimeRange.Text = value;
            }
        }

        List<TimeSlice> m_timeSlices = new List<TimeSlice>();

        public List<TimeSlice> TimeSlices
        {
            get
            {
                return m_timeSlices;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.TimeRangeMode == false)
            {
                int nRet = BuildSlices(out string strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        // 正规化起始时间
        static DateTime RoundStartTime(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day,
                0, 0, 0, 0);
        }

        // 正规化结束时间。设定为结束日的后一日凌晨，在进行比较的时候，用<endtime就正好
        static DateTime RoundEndTime(DateTime time)
        {
            time = new DateTime(time.Year, time.Month, time.Day,
                0, 0, 0, 0);
            return time + new TimeSpan(1, 0, 0, 0); // 后面一天
        }

        int BuildSlices(out string strError)
        {
            strError = "";

            this.m_timeSlices = new List<TimeSlice>();
            if (this.comboBox_slice.Text == "<不切片>")
            {
                // 只有一片
                TimeSlice slice = new TimeSlice();
                slice.Start = this.StartTime;
                slice.Length = this.EndTime - this.StartTime;
                slice.Caption = DateTimeUtil.ToDateString(slice.Start) + "-" + DateTimeUtil.ToDateString(slice.Start + slice.Length);
                this.m_timeSlices.Add(slice);
                return 0;
            }

            if (this.comboBox_slice.Text == "日")
            {
                DateTime start = this.StartTime;
                DateTime end = this.EndTime;
                for (; ; )
                {
                    if (start >= end)
                        break;
                    TimeSlice slice = new TimeSlice();
                    slice.Start = start;
                    slice.Length = new TimeSpan(1, 0, 0, 0);
                    slice.Caption = DateTimeUtil.ToDateString(slice.Start);
                    if (start.Day == 1)
                        slice.Style = "dark";   // 每月第一日加重背景显示
                    this.m_timeSlices.Add(slice);

                    start += new TimeSpan(1, 0, 0, 0);
                }

                return 0;
            }

            if (this.comboBox_slice.Text == "月")
            {
                DateTime start = this.StartTime;
                // 校正为整月第一天
                start = new DateTime(start.Year, start.Month, 1);
                DateTime end = this.EndTime;
                for (; ; )
                {
                    if (start >= end)
                        break;
                    DateTime end_month;
                    if (start.Month == 12)
                        end_month = new DateTime(start.Year+1, 1, 1);
                    else
                        end_month = new DateTime(start.Year, start.Month+1, 1);

                    TimeSlice slice = new TimeSlice();
                    slice.Start = start;
                    slice.Length = end_month - start;
                    slice.Caption = DateTimeUtil.ToMonthString(slice.Start);
                    if (start.Month == 1)
                        slice.Style = "dark";   // 每年第一个月加重背景显示
                    this.m_timeSlices.Add(slice);

                    start = end_month;
                }

                return 0;
            }

            if (this.comboBox_slice.Text == "年")
            {
                DateTime start = this.StartTime;
                // 校正为整年第一天
                start = new DateTime(start.Year, 1, 1);
                DateTime end = this.EndTime;
                for (; ; )
                {
                    if (start >= end)
                        break;
                    DateTime end_year = new DateTime(start.Year + 1, 1, 1);

                    TimeSlice slice = new TimeSlice();
                    slice.Start = start;
                    slice.Length = end_year - start;
                    slice.Caption = DateTimeUtil.ToYearString(slice.Start);
                    this.m_timeSlices.Add(slice);

                    start = end_year;
                }

                return 0;
            }

            strError = "无法识别的时间间隔 '" + this.comboBox_slice.Text + "'";
            return -1;
        }

        void QuickSetTimeRange(Control control)
        {
            string strStartDate = "";
            string strEndDate = "";

            string strName = control.Text.Replace(" ", "").Trim();

            if (strName == "订购日至今日")
            {
                DateTime now = DateTime.Now;

                strStartDate = DateTimeUtil.DateTimeToString8(this.OrderDate);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "今天" || strName == "今日(一日)")
            {
                DateTime now = DateTime.Now;

                strStartDate = DateTimeUtil.DateTimeToString8(now);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本周")
            {
                DateTime now = DateTime.Now;
                int nDelta = (int)now.DayOfWeek; // 0-6 sunday - saturday
                DateTime start = now - new TimeSpan(nDelta, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                // strEndDate = DateTimeUtil.DateTimeToString8(start + new TimeSpan(7, 0,0,0));
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "本月")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 6) + "01";
            }
            else if (strName == "本年")
            {
                DateTime now = DateTime.Now;
                strEndDate = DateTimeUtil.DateTimeToString8(now);
                strStartDate = strEndDate.Substring(0, 4) + "0101";
            }
            else if (strName == "最近七天" || strName == "最近7天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(7 - 1, 0, 0, 0);

                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十天" || strName == "最近30天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(30 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三十一天" || strName == "最近31天")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(31 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三百六十五天" || strName == "最近365天" || strName == "最近一年" || strName == "最近1年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近二年" || strName == "最近2年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(2 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近三年" || strName == "最近3年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(3 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else if (strName == "最近十年" || strName == "最近10年")
            {
                DateTime now = DateTime.Now;
                DateTime start = now - new TimeSpan(10 * 365 - 1, 0, 0, 0);
                strStartDate = DateTimeUtil.DateTimeToString8(start);
                strEndDate = DateTimeUtil.DateTimeToString8(now);
            }
            else
            {
                MessageBox.Show(this, "无法识别的周期 '" + strName + "'");
                return;
            }

            this.dateControl_start.Value = DateTimeUtil.Long8ToDateTime(strStartDate);
            this.dateControl_end.Value = DateTimeUtil.Long8ToDateTime(strEndDate);
        }

        private void comboBox_quickSetTimeRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.IsHandleCreated == false)
                return;

            Delegate_QuickSetTimeRange d = new Delegate_QuickSetTimeRange(QuickSetTimeRange);
            this.BeginInvoke(d, new object[] { sender });
        }

        delegate void Delegate_QuickSetTimeRange(Control control);

        private void DateSliceDialog_Load(object sender, EventArgs e)
        {
            if (this.TimeRangeMode)
            {
                this.label_slice.Visible = false;
                this.comboBox_slice.Visible = false;
            }
        }
    }

    // 
    /// <summary>
    /// 一个时间切片
    /// </summary>
    public class TimeSlice
    {
        /// <summary>
        /// 标题
        ///  表格中用于表示时间刻度的标题文字
        /// </summary>
        public string Caption = ""; // 表格中用于表示时间刻度的标题文字

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime Start;

        /// <summary>
        /// 结束时间
        /// </summary>
        public TimeSpan Length;

        /// <summary>
        /// 风格
        /// </summary>
        public string Style = "";   // dark 表示需要加重背景显示
    }
}
