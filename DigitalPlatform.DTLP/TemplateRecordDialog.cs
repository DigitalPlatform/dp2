using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.DTLP
{
    public partial class TemplateRecordDialog : Form
    {
        public TemplateRecordDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {

            if (textBox_name.Text == "")
            {
                MessageBox.Show(this, "尚未指定模板记录名...");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, System.EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string TemplateName
        {
            get
            {
                return this.textBox_name.Text;
            }
            set
            {
                this.textBox_name.Text = value;
            }
        }

        public string TemplateComment
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
    }
}