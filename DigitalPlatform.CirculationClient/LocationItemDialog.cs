using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.Text;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 馆藏点对话框
    /// </summary>
    public partial class LocationItemDialog : Form
    {
        /// <summary>
        /// 是否为创建模式
        /// </summary>
        public bool CreateMode = false; // 是否为创建模式？==true为创建模式；==false为修改模式

        /// <summary>
        /// 图书馆代码列表字符串。提供给 combobox 使用
        /// </summary>
        public string LibraryCodeList
        {
            get
            {
                StringBuilder text = new StringBuilder();
                foreach (string s in this.comboBox_libraryCode.Items)
                {
                    if (text.Length > 0)
                        text.Append(",");
                    text.Append(s);
                }
                return text.ToString();
            }
            set
            {
                List<string> values = StringUtil.SplitList(value);
                this.comboBox_libraryCode.Items.Clear();
                foreach (string s in values)
                {
                    this.comboBox_libraryCode.Items.Add(s);
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LocationItemDialog()
        {
            InitializeComponent();
        }

        private void LocationItemDialog_Load(object sender, EventArgs e)
        {
            // 如果只有一项列表事项，而当前为空白，则自动设置好这一项
            if (this.CreateMode == true
                && string.IsNullOrEmpty(this.comboBox_libraryCode.Text) == true
                && this.comboBox_libraryCode.Items.Count > 0)
                this.comboBox_libraryCode.Text = (string)this.comboBox_libraryCode.Items[0];
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_location.Text == "")
            {
                // 允许馆藏地点为空，但是要确认一下

                DialogResult msgResult = MessageBox.Show(this,
                    "确实要把馆藏地点名称设置为空?",
                    "LocationItemDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (msgResult == DialogResult.No)
                    return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 馆代码
        /// </summary>
        public string LibraryCode
        {
            get
            {
                return this.comboBox_libraryCode.Text;
            }
            set
            {
                this.comboBox_libraryCode.Text = value;
            }
        }

        /// <summary>
        /// 馆藏点字符串
        /// </summary>
        public string LocationString
        {
            get
            {
                return this.textBox_location.Text;
            }
            set
            {
                this.textBox_location.Text = value;
            }
        }

        /// <summary>
        /// 是否允许外借
        /// </summary>
        public bool CanBorrow
        {
            get
            {
                return this.checkBox_canBorrow.Checked;
            }
            set
            {
                this.checkBox_canBorrow.Checked = value;
            }
        }

        /// <summary>
        /// 册条码号可为空
        /// </summary>
        public bool ItemBarcodeNullable
        {
            get
            {
                return this.checkBox_itemBarcodeNullable.Checked;
            }
            set
            {
                this.checkBox_itemBarcodeNullable.Checked = value;
            }
        }
    }
}