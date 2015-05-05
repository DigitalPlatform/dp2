using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    internal partial class ModifyCommentDialog : Form
    {
        int DisableNewTextChange = 0;

        int DisableCurrentTextChange = 0;   // 是否禁止CurrentOldComment被跟随修改? >0表示禁止

        public string OriginOldComment = "";    // 最初的"旧注释"内容
        public string CurrentOldComment = "";   // 在窗口内修改过的“旧注释”内容

        public ModifyCommentDialog()
        {
            InitializeComponent();
        }

        private void ModifyCommentDialog_Load(object sender, EventArgs e)
        {
            this.CurrentOldComment = this.OriginOldComment;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radioButton_append_CheckedChanged(object sender, EventArgs e)
        {
            OnCheckedChanged();

            if (this.radioButton_append.Checked == true)
            {
                this.radioButton_overwrite.Checked = false;
            }
            else
            {
                this.radioButton_overwrite.Checked = true;
            }

        }

        void OnCheckedChanged()
        {
            if (this.radioButton_append.Checked == true)
            {
                this.textBox_appendComment.ReadOnly = false;
                this.button_newComment_insertDateTime.Enabled = true;

                this.textBox_comment.ReadOnly = true;

                // 一下子复原到最初的状态
                this.DisableCurrentTextChange++;
                this.textBox_comment.Text = this.OriginOldComment;
                this.DisableCurrentTextChange--;

                this.button_oldComment_insertDateTime.Enabled = false;

                this.label_comment.Text = "注: 当最后提交到服务器的时候，新注释将自动追加在旧注释的后面。";
            }
            else
            {
                this.textBox_appendComment.ReadOnly = true;

                this.DisableNewTextChange++;
                this.textBox_appendComment.Text = "";
                this.DisableNewTextChange --;

                this.button_newComment_insertDateTime.Enabled = false;

                this.textBox_comment.ReadOnly = false;
                // 从修改后的最后状态继续进行
                this.DisableCurrentTextChange++;
                this.textBox_comment.Text = this.CurrentOldComment;
                this.DisableCurrentTextChange--;

                this.button_oldComment_insertDateTime.Enabled = true;

                this.label_comment.Text = "注: 当最后提交到服务器的时候，旧注释将被修改。";
            }
        }

        public bool IsAppend
        {
            get
            {
                if (this.radioButton_append.Checked == true)
                    return true;
                return false;
            }
            set
            {
                if (value == true)
                {
                    this.radioButton_append.Checked = true;
                }
                else
                {
                    this.radioButton_append.Checked = false;
                }
            }
        }

        public string AppendComment
        {
            get
            {
                return this.textBox_appendComment.Text;
            }
            set
            {
                this.DisableNewTextChange++;
                this.textBox_appendComment.Text = value;
                this.DisableNewTextChange--;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.DisableCurrentTextChange++;
                this.textBox_comment.Text = value;
                this.DisableCurrentTextChange--;
            }
        }

        public string ID
        {
            get
            {
                return this.textBox_id.Text;
            }
            set
            {
                this.textBox_id.Text = value;
            }
        }

        public string Price
        {
            get
            {
                return this.textBox_price.Text;
            }
            set
            {
                this.textBox_price.Text = value;
            }
        }

        private void radioButton_overwrite_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_overwrite.Checked == true)
            {
                this.radioButton_append.Checked = false;
            }
            else
            {
                this.radioButton_append.Checked = true;
            }
        }

        private void textBox_oldComment_TextChanged(object sender, EventArgs e)
        {
            if (this.DisableCurrentTextChange > 0)
                return;

            this.CurrentOldComment = this.textBox_comment.Text;
            this.button_OK.Enabled = true;
        }

        private void textBox_newComment_TextChanged(object sender, EventArgs e)
        {
            if (this.DisableNewTextChange > 0)
                return;

            this.button_OK.Enabled = true;
        }

        private void button_oldComment_insertDateTime_Click(object sender, EventArgs e)
        {
            this.textBox_comment.Paste(DateTime.Now.ToString());

            this.textBox_comment.Focus();

        }

        private void button_newComment_insertDateTime_Click(object sender, EventArgs e)
        {
            this.textBox_appendComment.Paste(DateTime.Now.ToString());

            this.textBox_appendComment.Focus();

        }

        private void button_oldComment_insertDateTime_MouseHover(object sender, EventArgs e)
        {
            this.toolTip_usage.Show("插入当前日期时间", this.button_oldComment_insertDateTime);

        }

        private void button_newComment_insertDateTime_MouseHover(object sender, EventArgs e)
        {
            this.toolTip_usage.Show("插入当前日期时间", this.button_newComment_insertDateTime);
        }

        private void textBox_appendComment_Validating(object sender, CancelEventArgs e)
        {
            if (this.textBox_appendComment.Text.IndexOfAny(new char[] { '<', '>' }) != -1)
            {
                MessageBox.Show(this, "注释文字中不允许包含符号 '<' '>'");
                e.Cancel = true;
            }

        }

        private void textBox_comment_Validating(object sender, CancelEventArgs e)
        {
            if (this.textBox_comment.Text.IndexOfAny(new char[] { '<', '>' }) != -1)
            {
                MessageBox.Show(this, "注释文字中不允许包含符号 '<' '>'");
                e.Cancel = true;
            }
        }
    }
}