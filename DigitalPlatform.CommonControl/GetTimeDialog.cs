using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            this.Value2 = new DateTime(old_value.Year, old_value.Month, old_value.Day);
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
                if (this.Value2 != DateTimePicker.MaximumDateTime)
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
                    this.Value2 = DateTimePicker.MaximumDateTime;
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
                    return this.Value.ToString("u");
                }

                string strStart = "";
                if (this.Value != DateTimePicker.MinimumDateTime)
                    strStart = this.Value.ToString("u");

                string strEnd = "";
                if (this.Value2 != DateTimePicker.MaximumDateTime)
                    strEnd = this.Value2.ToString("u");

                return strStart
                    + "~"
                    + strEnd;
            }
            set
            {
                if (this.RangeMode == false)
                {
                    // 注意：可能会抛出异常
                    this.Value = DateTimeUtil.FromUTimeString(value);
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
                    this.Value = DateTimeUtil.FromUTimeString(strStart);

                if (string.IsNullOrEmpty(strEnd) == true)
                    this.Value2 = DateTimePicker.MaximumDateTime;
                else
                    this.Value2 = DateTimeUtil.FromUTimeString(strEnd);
            }
        }

        private void button_setMin_Click(object sender, EventArgs e)
        {
            this.Value = DateTimePicker.MinimumDateTime;
        }

        private void button_setMax2_Click(object sender, EventArgs e)
        {
            this.Value2 = DateTimePicker.MaximumDateTime;
        }
    }
}
