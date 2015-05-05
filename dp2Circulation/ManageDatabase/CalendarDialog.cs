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
    /// <summary>
    /// 编辑日历的对话框
    /// </summary>
    public partial class CalendarDialog : Form
    {
        public CalendarDialog()
        {
            InitializeComponent();
        }

        private void CalendarDialog_Load(object sender, EventArgs e)
        {
            string strLibraryCode1 = "";
            string strPureName1 = "";

            Global.ParseCalendarName(this._strCalendarName,
        out strLibraryCode1,
        out strPureName1);

            this.comboBox_libraryCode.Text = strLibraryCode1;
            this.textBox_name.Text = strPureName1;

            string strError = "";
            int nRet = this.calenderControl1.SetData(this.textBox_timeRange.Text,
    1,
    this._content,
    out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

        }

        private void CalendarDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void CalendarDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strData = "";

            if (string.IsNullOrEmpty(this.textBox_name.Text) == true)
            {
                strError = "尚未指定日历名";
                goto ERROR1;
            }

            if (this.textBox_name.Text.IndexOf("/") != -1)
            {
                strError = "日历名中不允许包含字符 '/'";
                goto ERROR1;
            }

            if (this.comboBox_libraryCode.Text.IndexOf("/") != -1)
            {
                strError = "馆代码中不允许包含字符 '/'";
                goto ERROR1;
            }

            // 合成日历名
#if NO
            if (string.IsNullOrEmpty(this.comboBox_libraryCode.Text ) == true)
                this._strCalendarName = this.textBox_name.Text;
            else
                this._strCalendarName = this.comboBox_libraryCode.Text + "/" + this.textBox_name.Text;
#endif
            this._strCalendarName = Global.BuildCalendarName(this.comboBox_libraryCode.Text, this.textBox_name.Text);

            int nRet = calenderControl1.GetDates(1,
    out strData,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            this._content = strData;

            this.textBox_timeRange.Text = this.calenderControl1.GetRangeString();

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

        string _strCalendarName = "";   // 全路径，包含馆代码。例如 "海淀分馆/基本日历"

        /// <summary>
        /// 日历名字
        /// </summary>
        public string CalendarName
        {
            get
            {
                return this._strCalendarName;
            }
            set
            {
                this._strCalendarName = value;
            }
        }

        /// <summary>
        /// 日历时间范围
        /// </summary>
        public string Range
        {
            get
            {
                return this.textBox_timeRange.Text;
            }
            set
            {
                this.textBox_timeRange.Text = value;
            }
        }

        /// <summary>
        /// 日历注释
        /// </summary>
        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        string _content = "";

        /// <summary>
        /// 日历内容
        /// </summary>
        public string Content
        {
            get
            {
                return this._content;
            }
            set
            {
                this._content = value;
            }
        }

        private void textBox_timeRange_Enter(object sender, EventArgs e)
        {
        }

        private void calenderControl1_Leave(object sender, EventArgs e)
        {
            this.textBox_timeRange.Text = this.calenderControl1.GetRangeString();
        }

        bool _bReadOnly = false;

        /// <summary>
        /// 是否为只读状态
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return this._bReadOnly;
            }
            set
            {
                this._bReadOnly = value;

                this.textBox_comment.ReadOnly = value;
                this.calenderControl1.ReadOnly = value;
                this.button_OK.Enabled = !value;

                if (this._bCreateMode == true && value == false)
                {
                    // 放开
                    this.comboBox_libraryCode.Enabled = true;
                    this.textBox_name.ReadOnly = false;
                }
                else
                {
                    // 禁止
                    this.comboBox_libraryCode.Enabled = false;
                    this.textBox_name.ReadOnly = true;
                }
            }
        }

        bool _bCreateMode = false;
        /// <summary>
        /// 是否为创建状态。false 表示为修改状态
        /// </summary>
        public bool CreateMode
        {
            get
            {
                return this._bCreateMode;
            }
            set
            {
                this._bCreateMode = value;
                if (value == true)
                {
                    this.comboBox_libraryCode.Enabled = true;
                    this.textBox_name.ReadOnly = false;
                }
                else
                {
                    this.comboBox_libraryCode.Enabled = false;
                    this.textBox_name.ReadOnly = true;
                }
            }
        }

        /// <summary>
        /// 是否为全局用户
        /// 全局用户，馆代码可以直接修改
        /// </summary>
        public bool IsGlobalUser = false;

        /// <summary>
        /// 当前用户所管辖的全部馆代码列表
        /// </summary>
        public List<string> OwnerLibraryCodes
        {
            get
            {
                List<string> results = new List<string>();
                foreach (string s in this.comboBox_libraryCode.Items)
                {
                    results.Add(s);
                }
                return results;
            }
            set
            {
                this.comboBox_libraryCode.Items.Clear();
                if (value != null)
                {
                    foreach (string s in value)
                    {
                        this.comboBox_libraryCode.Items.Add(s);
                    }
                }

                if (this.comboBox_libraryCode.Items.Count > 0 && this.IsGlobalUser == false)
                    this.comboBox_libraryCode.DropDownStyle = ComboBoxStyle.DropDownList;
                else
                    this.comboBox_libraryCode.DropDownStyle = ComboBoxStyle.DropDown;
            }
        }
    }
}
