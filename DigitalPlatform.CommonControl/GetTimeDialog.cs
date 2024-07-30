using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.IO;

namespace DigitalPlatform.CommonControl
{
    public partial class GetTimeDialog : Form
    {
        public GetTimeDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public DateTime Value
        {
            get
            {
                return this.dateTimePicker1.Value;
            }
            set
            {
                this.dateTimePicker1.Value = value;
            }
        }

        public DateTime Value2
        {
            get
            {
                return this.dateTimePicker2.Value;
            }
            set
            {
                this.dateTimePicker2.Value = value;
            }
        }

        private void button_clearHMS_Click(object sender, EventArgs e)
        {
            DateTime old_value = this.Value;
            this.Value = new DateTime(old_value.Year, old_value.Month, old_value.Day);
        }

        private void button_clearHMS_2_Click(object sender, EventArgs e)
        {
            DateTime old_value = this.Value2;
            this.Value2 = new DateTime(old_value.Year, old_value.Month, old_value.Day) + (TimeSpan.FromDays(1) - TimeSpan.FromSeconds(1));
        }

        bool m_bRangeMode = false;

        public bool RangeMode
        {
            get
            {
                return m_bRangeMode;
            }
            set
            {
                m_bRangeMode = value;

                if (m_bRangeMode == true)
                {
                    this.dateTimePicker2.Visible = true;
                    this.button_clearHMS_2.Visible = true;
                    this.button_setMax2.Visible = true;
                }
                else
                {
                    this.dateTimePicker2.Visible = false;
                    this.button_clearHMS_2.Visible = false;
                    this.button_setMax2.Visible = false;
                }
            }
        }

        public string Rfc1123String
        {
            get
            {
                if (this.RangeMode == false)
                {
                    return DateTimeUtil.Rfc1123DateTimeStringEx(this.Value);
                }

                string strStart = "";
                if (this.Value != DateTimePicker.MinimumDateTime)
                    strStart = DateTimeUtil.Rfc1123DateTimeStringEx(this.Value);

                string strEnd = "";
                if (this.Value2 != MaximumDateTime)
                    strEnd = DateTimeUtil.Rfc1123DateTimeStringEx(this.Value2);

                return strStart
                    + "~"
                    + strEnd;
            }
            set
            {
                if (this.RangeMode == false)
                {
                    // 注意：可能会抛出异常
                    this.Value = DateTimeUtil.FromRfc1123DateTimeString(value).ToLocalTime();
                    return;
                }

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
                    this.Value = DateTimePicker.MinimumDateTime;
                else
                    this.Value = DateTimeUtil.FromRfc1123DateTimeString(strStart).ToLocalTime();

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.Value2 = MaximumDateTime;
                else
                    this.Value2 = DateTimeUtil.FromRfc1123DateTimeString(strEnd).ToLocalTime();
            }
        }

        public string uString
        {
            get
            {
                if (this.RangeMode == false)
                {
                    return this.Value.ToUniversalTime().ToString("u");
                }

                string strStart = "";
                if (this.Value != DateTimePicker.MinimumDateTime)
                    strStart = this.Value.ToUniversalTime().ToString("u");

                string strEnd = "";
                if (this.Value2 != MaximumDateTime)
                    strEnd = this.Value2.ToUniversalTime().ToString("u");

                return strStart
                    + "~"
                    + strEnd;
            }
            set
            {
                if (this.RangeMode == false)
                {
                    // 注意：可能会抛出异常
                    this.Value = DateTimeUtil.FromUTimeString(value).ToLocalTime();
                    return;
                }

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
                    this.Value = DateTimePicker.MinimumDateTime;
                else
                    this.Value = DateTimeUtil.FromUTimeString(strStart).ToLocalTime();

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.Value2 = MaximumDateTime;
                else
                    this.Value2 = DateTimeUtil.FromUTimeString(strEnd).ToLocalTime();
            }
        }

        public string sString
        {
            get
            {
                if (this.RangeMode == false)
                {
                    return this.Value.ToString("s");
                }

                string strStart = "";
                if (this.Value != DateTimePicker.MinimumDateTime)
                    strStart = this.Value.ToString("s");

                string strEnd = "";
                if (this.Value2 != MaximumDateTime)
                    strEnd = this.Value2.ToString("s");

                return strStart
                    + "~"
                    + strEnd;
            }
            set
            {
                if (this.RangeMode == false)
                {
                    // 注意：可能会抛出异常
                    this.Value = FromSTimeString(value);
                    return;
                }

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
                    this.Value = DateTimePicker.MinimumDateTime;
                else
                    this.Value = FromSTimeString(strStart);

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.Value2 = MaximumDateTime;
                else
                    this.Value2 = FromSTimeString(strEnd);
            }
        }

        public static DateTime FromSTimeString(string strTime)
        {
            // 自定义格式字符串为“yyyy'-'MM'-'dd HH':'mm':'ss'Z'”。 
            // 无法描述时区。隐含表达。
            IFormatProvider culture = new CultureInfo("zh-CN", true);
            return DateTime.ParseExact(strTime,
                "s",
                culture);
        }

        DateTime MaximumDateTime
        {
            get
            {
                var value = DateTimePicker.MaximumDateTime;
                // 把时分秒部分推到最大
                // value += TimeSpan.FromDays(1) - TimeSpan.FromSeconds(1);
                value = new DateTime(value.Year, 1, 1);
                value -= TimeSpan.FromSeconds(1);
                return value;
            }
        }

        public string String8
        {
            get
            {
                if (this.RangeMode == false)
                {
                    return DateTimeUtil.DateTimeToString8(this.Value);
                }

                string strStart = "";
                if (this.Value != DateTimePicker.MinimumDateTime)
                    strStart = DateTimeUtil.DateTimeToString8(this.Value);

                string strEnd = "";
                if (this.Value2 != MaximumDateTime)
                    strEnd = DateTimeUtil.DateTimeToString8(this.Value2);

                if (string.IsNullOrEmpty(strStart) && string.IsNullOrEmpty(strEnd))
                    return "";

                return strStart
                    + "-"
                    + strEnd;
            }
            set
            {
                this.dateTimePicker1.CustomFormat = "yyyy-MM-dd";
                this.dateTimePicker2.CustomFormat = "yyyy-MM-dd";
                this.button_clearHMS.Visible = false;
                this.button_clearHMS_2.Visible = false;

                if (this.RangeMode == false)
                {
                    // 注意：可能会抛出异常
                    this.Value = DateTimeUtil.Long8ToDateTime(value);
                    return;
                }

                string strStart = "";
                string strEnd = "";

                int nRet = value.IndexOf("-");
                if (nRet == -1)
                    strStart = value.Trim();
                else
                {
                    strStart = value.Substring(0, nRet).Trim();
                    strEnd = value.Substring(nRet + 1).Trim();
                }

                if (string.IsNullOrEmpty(strStart) == true)
                    this.Value = DateTimePicker.MinimumDateTime;
                else
                    this.Value = DateTimeUtil.Long8ToDateTime(strStart);

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.Value2 = MaximumDateTime;
                else
                    this.Value2 = DateTimeUtil.Long8ToDateTime(strEnd);
            }
        }

        private void button_setMin_Click(object sender, EventArgs e)
        {
            this.Value = DateTimePicker.MinimumDateTime;
        }

        private void button_setMax2_Click(object sender, EventArgs e)
        {
            this.Value2 = MaximumDateTime;
        }
    }
}
