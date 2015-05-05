using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 显示和编辑两条 MARC 书目记录的对话框
    /// </summary>
    public partial class TwoBiblioDialog : Form
    {
        string m_strOldMessage = "";

        /// <summary>
        /// 够奥函数
        /// </summary>
        public TwoBiblioDialog()
        {
            InitializeComponent();
        }

        private void TwoBiblioDialog_Load(object sender, EventArgs e)
        {
            this.marcEditor1.Changed = false;
            this.marcEditor2.Changed = false;

            this.m_strOldMessage = this.MessageText;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 源 MARC 字符串
        /// </summary>
        public string MarcSource
        {
            get
            {
                return this.marcEditor1.Marc;
            }
            set
            {
                this.marcEditor1.Marc = value;
            }
        }

        /// <summary>
        /// 源是否为只读
        /// </summary>
        public bool ReadOnlySource
        {
            get
            {
                return this.marcEditor1.ReadOnly;
            }
            set
            {
                this.marcEditor1.ReadOnly = value;
            }
        }

        /// <summary>
        /// 目标 MARC 字符串
        /// </summary>
        public string MarcTarget
        {
            get
            {
                return this.marcEditor2.Marc;
            }
            set
            {
                this.marcEditor2.Marc = value;
            }
        }

        /// <summary>
        /// 目标是否为只读
        /// </summary>
        public bool ReadOnlyTarget
        {
            get
            {
                return this.marcEditor2.ReadOnly;
            }
            set
            {
                this.marcEditor2.ReadOnly = value;
            }
        }

        /// <summary>
        /// 消息字符串
        /// </summary>
        public string MessageText
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        /// <summary>
        /// 源标签文字
        /// </summary>
        public string LabelSourceText
        {
            get
            {
                return this.label_left.Text;
            }
            set
            {
                this.label_left.Text = value;
            }
        }

        /// <summary>
        /// 目标标签文字
        /// </summary>
        public string LabelTargetText
        {
            get
            {
                return this.label_right.Text;
            }
            set
            {
                this.label_right.Text = value;
            }
        }

        /// <summary>
        /// 是否允许直接修改目标记录
        /// </summary>
        public bool EditTarget
        {
            get
            {
                return this.checkBox_editTarget.Checked;
            }
            set
            {
                this.checkBox_editTarget.Checked = value;
            }
        }

        private void button_yes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();

        }

        private void button_no_Click(object sender, EventArgs e)
        {
            if (this.checkBox_editTarget.Checked == true)
            {
                if (this.marcEditor2.Changed == true)
                {
                    DialogResult result = MessageBox.Show(this,
                        "是否要放弃刚才对窗口中目标记录内容的修改?",
                        "TwoBiblioDialog",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.No)
                        return;
                }
            }

            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void checkBox_editTarget_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_editTarget.Checked == false)
            {
                this.marcEditor1.ReadOnly = false;
                this.marcEditor2.ReadOnly = true;

                this.button_yes.Text = "覆盖(&O)";
                this.button_no.Text = "不覆盖(&N)";
                this.MessageText = this.m_strOldMessage;
            }
            else
            {
                this.marcEditor1.ReadOnly = true;
                this.marcEditor2.ReadOnly = false;

                this.button_yes.Text = "保存(&S)";
                this.button_no.Text = "不保存(&N)";

                this.MessageText = "请问是否要保存对目标记录的修改?";
            }
        }
    }
}