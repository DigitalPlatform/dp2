using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform;
using DigitalPlatform.Core;

namespace dp2Catalog
{
    public partial class TestForm : Form
    {
        public MainForm MainForm = null;

        public TestForm()
        {
            InitializeComponent();
        }

        private void button_getValue_Click(object sender, EventArgs e)
        {
            this.textBox_w3cDtfString.Text = "";

            try
            {
                string strText = this.w3cDtfControl1.ValueString;
                strText = strText.Replace(" ", "#");

                this.textBox_w3cDtfString.Text = strText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }

        private void button_setValue_Click(object sender, EventArgs e)
        {
            this.w3cDtfControl1.ValueString = "";

            try
            {
                this.w3cDtfControl1.ValueString = this.textBox_w3cDtfString.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
            }
        }
    }
}