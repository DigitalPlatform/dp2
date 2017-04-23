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
    /// “保存读者记录到 ...”对话框
    /// </summary>
    public partial class ReaderSaveToDialog : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderSaveToDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 消息文字
        /// </summary>
        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }

        /// <summary>
        /// 记录路径
        /// </summary>
        public string RecPath
        {
            get
            {
                return this.comboBox_readerDbName.Text + "/" + this.textBox_recordID.Text;
            }
            set
            {
                int nRet = value.IndexOf("/");
                if (nRet == -1)
                {
                    this.comboBox_readerDbName.Text = value;
                }
                else
                {
                    this.comboBox_readerDbName.Text = value.Substring(0, nRet);
                    this.textBox_recordID.Text = value.Substring(nRet + 1);
                }
            }
        }

        /// <summary>
        /// 记录 ID
        /// </summary>
        public string RecID
        {
            get
            {
                return this.textBox_recordID.Text;
            }
            set
            {
                this.textBox_recordID.Text = value;
            }
        }

        /// <summary>
        /// "记录 ID" 输入框的 Enabled 状态
        /// </summary>
        public bool EnableRecID
        {
            get
            {
                return this.textBox_recordID.Enabled;
            }
            set
            {
                this.textBox_recordID.Enabled = value;
            }
        }

        // combobox内的库名选择变化后，记录ID变化为"?"
        private void comboBox_readerDbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.textBox_recordID.Text = "?";
        }

        private void comboBox_readerDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_readerDbName.Items.Count > 0)
                return;

            if (Program.MainForm.ReaderDbNames == null)
                return;

            for (int i = 0; i < Program.MainForm.ReaderDbNames.Length; i++)
            {
                this.comboBox_readerDbName.Items.Add(Program.MainForm.ReaderDbNames[i]);
            }

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.comboBox_readerDbName.Text) == true)
            {
                MessageBox.Show(this, "尚未指定读者库名");
                return;
            }

            if (String.IsNullOrEmpty(this.textBox_recordID.Text) == true)
            {
                MessageBox.Show(this, "尚未指定记录ID");
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
