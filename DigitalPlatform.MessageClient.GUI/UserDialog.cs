using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.MessageClient
{
    public partial class UserDialog : Form
    {
        // 是否为修改模式。false 表示为创建模式；true 表示为修改模式
        public bool ChangeMode
        {
            get;
            set;
        }

        public UserDialog()
        {
            InitializeComponent();
        }

        public UserItem UserItem
        {
            get;
            set;
        }

        void LoadFromUserItem()
        {
            if (this.UserItem == null)
                return;

            this.textBox_userName.Text = this.UserItem.userName;
            this.textBox_password.Text = this.UserItem.password;
            this.textBox_department.Text = this.UserItem.department;
            this.textBox_rights.Text = this.UserItem.rights;
            this.textBox_duty.Text = this.UserItem.duty;
            this.textBox_tel.Text = this.UserItem.tel;
            this.textBox_comment.Text = this.UserItem.comment;
            this.textBox_groups.Text = this.UserItem.groups == null? "" : string.Join(",", this.UserItem.groups);
        }

        void BuildUserItem()
        {
            if (this.UserItem == null)
                this.UserItem = new UserItem();

            this.UserItem.userName = this.textBox_userName.Text;
            this.UserItem.password = this.textBox_password.Text;
            this.UserItem.department = this.textBox_department.Text;
            this.UserItem.rights = this.textBox_rights.Text;
            this.UserItem.duty = this.textBox_duty.Text;
            this.UserItem.tel = this.textBox_tel.Text;
            this.UserItem.comment = this.textBox_comment.Text;
            this.UserItem.groups = this.textBox_groups.Text.Split(new char[] {','});
        }

        private void UserDialog_Load(object sender, EventArgs e)
        {
            if (this.ChangeMode)
                this.checkBox_changePassword.Visible = true;
            else
            {
                this.checkBox_changePassword.Checked = true;
                this.checkBox_changePassword.Visible = false;
            }

            checkBox_changePassword_CheckedChanged(this, new EventArgs());

            this.LoadFromUserItem();

            this.Changed = false;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.checkBox_changePassword.Checked == true)
            {
                if (this.textBox_password.Text != this.textBox_confirmPassword.Text)
                {
                    strError = "密码 和 确认密码 不一致。请重新输入";
                    goto ERROR1;
                }
            }

            this.BuildUserItem();

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

        private void checkBox_changePassword_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_changePassword.Checked == true)
            {
                this.textBox_password.Enabled = true;
                this.textBox_confirmPassword.Enabled = true;
            }
            else
            {
                this.textBox_password.Enabled = false;
                this.textBox_confirmPassword.Enabled = false;
            }
        }

        public bool ChangePassword
        {
            get
            {
                return this.checkBox_changePassword.Checked;
            }
            set
            {
                this.checkBox_changePassword.Checked = value;
            }
        }

        // 对话框打开期间，字段内容是否发生过修改。注：不包含密码和确认密码字段
        public bool Changed
        {
            get;
            set;
        }

        private void textBox_comment_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }
    }
}
