using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    internal partial class OutputAcountBookTextFileDialog : Form
    {
        public OutputAcountBookTextFileDialog()
        {
            InitializeComponent();
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

        // 按列配置截断长文字
        public bool Truncate
        {
            get
            {
                return this.checkBox_truncate.Checked;
            }
            set
            {
                this.checkBox_truncate.Checked = value;
            }
        }

        // 输出统计部分
        public bool OutputStatisPart
        {
            get
            {
                return this.checkBox_outputStatisPart.Checked;
            }
            set
            {
                this.checkBox_outputStatisPart.Checked = value;
            }
        }

        public string MessageText
        {
            get
            {
                return this.label_message.Text;
            }
            set
            {
                this.label_message.Text = value;
            }
        }
    }
}