using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.DTLP
{
    public partial class EditCfgForm : Form
    {
        bool m_bChanged = false;

        public EditCfgForm()
        {
            InitializeComponent();
        }

        private void EditCfgForm_Load(object sender, EventArgs e)
        {
            this.Changed = false;
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

        public string Content
        {
            get
            {
                return this.textBox_content.Text;
            }
            set
            {
                this.textBox_content.Text = value;


            }
        }

        public string CfgPath
        {
            get
            {
                return this.textBox_cfgPath.Text;
            }
            set
            {
                this.textBox_cfgPath.Text = value;
            }
        }

        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;

                if (value == true)
                    this.button_OK.Text = "±£´æ";
                else
                    this.button_OK.Text = "È·¶¨";
            }
        }

        private void textBox_content_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }

        private void textBox_cfgPath_TextChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }
    }
}