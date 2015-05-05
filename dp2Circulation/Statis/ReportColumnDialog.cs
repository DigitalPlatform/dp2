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
    public partial class ReportColumnDialog : Form
    {
        public ReportColumnDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_name.Text) == true)
            {
                strError = "尚未指定栏目名";
                goto ERROR1;
            }
            if (string.IsNullOrEmpty(this.comboBox_align.Text) == true)
            {
                strError = "尚未指定对齐方式";
                goto ERROR1;
            }

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

        public string ColumnName
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

        public string DataType
        {
            get
            {
                return this.comboBox_dataType.Text;
            }
            set
            {
                this.comboBox_dataType.Text = value;
            }
        }

        public string ColumnAlign
        {
            get
            {
                return this.comboBox_align.Text;
            }
            set
            {
                this.comboBox_align.Text = value;
            }
        }

        public bool ColumnSum
        {
            get
            {
                return this.checkBox_sum.Checked;
            }
            set
            {
                this.checkBox_sum.Checked = value;
            }
        }

        public string CssClass
        {
            get
            {
                return this.textBox_cssClass.Text;
            }
            set
            {
                this.textBox_cssClass.Text = value;
            }
        }

        public string Eval
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

        private void textBox_name_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox_align_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
