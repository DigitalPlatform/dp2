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
    /// 设置 UNIMARC 和 MARC21 书目记录缺省值的对话框
    /// </summary>
    public partial class MarcDefaultDialog : Form
    {
        public MarcDefaultDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string UnimarcDefault
        {
            get
            {
                return this.textBox_unimarc_default.Text;
            }
            set
            {
                this.textBox_unimarc_default.Text = value;
            }
        }

        public string Marc21Default
        {
            get
            {
                return this.textBox_marc21_default.Text;
            }
            set
            {
                this.textBox_marc21_default.Text = value;
            }
        }

        public string UnimarcHiddenFields
        {
            get
            {
                return this.textBox_unimarc_hiddenFields.Text;
            }
            set
            {
                this.textBox_unimarc_hiddenFields.Text = value;
            }
        }

        public string Marc21HiddenFields
        {
            get
            {
                return this.textBox_marc21_hiddenFields.Text;
            }
            set
            {
                this.textBox_marc21_hiddenFields.Text = value;
            }
        }
    }
}
