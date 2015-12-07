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
            this.textBox_tel.Text = this.UserItem.tel;
            this.textBox_comment.Text = this.UserItem.comment;
        }

        void BuildUserItem()
        {
            if (this.UserItem == null)
                this.UserItem = new UserItem();

            this.UserItem.userName = this.textBox_userName.Text;
            this.UserItem.password = this.textBox_password.Text;
            this.UserItem.department = this.textBox_department.Text;
            this.UserItem.rights = this.textBox_rights.Text;
            this.UserItem.tel = this.textBox_tel.Text;
            this.UserItem.comment = this.textBox_comment.Text;
        }

        private void UserDialog_Load(object sender, EventArgs e)
        {
            this.LoadFromUserItem();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.BuildUserItem();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

    }
}
