using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace dp2Circulation
{
    /// <summary>
    /// 打印选项中 输入一个栏目参数的 对话框
    /// </summary>
    internal partial class PrintColumnDlg : Form
    {
        public PrintColumnDlg()
        {
            InitializeComponent();
        }

        public int MaxChars
        {
            get
            {
                return (int)this.numericUpDown_maxChars.Value;
            }
            set
            {
                this.numericUpDown_maxChars.Value = value;
            }
        }

        public int WidthChars
        {
            get
            {
                return (int)this.numericUpDown_widthChars.Value;
            }
            set
            {
                this.numericUpDown_widthChars.Value = value;
            }
        }

        public string ColumnName
        {
            get
            {
                return this.comboBox_columnName.Text;
            }
            set
            {
                this.comboBox_columnName.Text = value;
            }
        }

        public string ColumnCaption
        {
            get
            {
                return this.textBox_caption.Text;
            }
            set
            {
                this.textBox_caption.Text = value;
            }
        }

        // 2014/6/3
        /// <summary>
        /// 脚本
        /// </summary>
        public string ColumnEvalue
        {
            get
            {
                return this.textBox_eval.Text;
            }
            set
            {
                this.textBox_eval.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.comboBox_columnName.Text == "")
            {
                MessageBox.Show(this, "尚未指定栏目名");
                return;
            }

            /*
            if (this.textBox_caption.Text == "")
            {
                MessageBox.Show(this, "尚未指定文字标题");
                return;
            }*/


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 当栏目名选定后，栏目标题文字跟随变化
        private void comboBox_columnName_DropDownClosed(object sender, EventArgs e)
        {
        }

        private void comboBox_columnName_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nRet = this.comboBox_columnName.Text.IndexOf("--");
            if (nRet == -1)
                return;

            string strRight = this.comboBox_columnName.Text.Substring(nRet + 2).Trim();
            this.textBox_caption.Text = strRight;
        }

        // 下拉列表事项
        /// <summary>
        /// 栏目名下拉列表事项
        /// </summary>
        public string[] ColumnItems
        {
            get
            {
                string[] result = new string[this.comboBox_columnName.Items.Count];
                this.comboBox_columnName.Items.CopyTo(result, 0);
                return result;
            }
            set
            {
                this.comboBox_columnName.Items.Clear();
                this.comboBox_columnName.Items.AddRange(value);
            }
        }
    }
}