using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public partial class W3cDtfDialog : Form
    {
        public string Value
        {
            get
            {
                return this.w3cDtfControl1.ValueString;
            }
            set
            {
                this.w3cDtfControl1.ValueString = value;
            }
        }

        public W3cDtfDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 检查正确性
            try
            {
                string strText = this.w3cDtfControl1.ValueString;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
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