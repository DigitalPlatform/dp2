using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.GcatClient
{
    public partial class IdLoginDialog : Form
    {
        public IdLoginDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_id.Text) == true)
            {
                MessageBox.Show(this, "尚未输入ID");
                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
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

        public bool SaveID
        {
            get
            {
                return this.checkBox_saveID.Checked;
            }
            set
            {
                this.checkBox_saveID.Checked = value;
            }
        }

        private void textBox_id_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_id.Text.IndexOf("/") != -1)
                this.textBox_id.PasswordChar = '*';
            else
                this.textBox_id.PasswordChar = (char)0;
        }
    }
}
