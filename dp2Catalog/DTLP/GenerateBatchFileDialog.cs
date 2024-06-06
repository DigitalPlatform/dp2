using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Catalog.DTLP
{
    public partial class GenerateBatchFileDialog : Form
    {
        public GenerateBatchFileDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(StartPath))
            {
                strError = "请输入起始路径";
                goto ERROR1;
            }
            if (string.IsNullOrEmpty(EndPath))
            {
                strError = "请输入结束路径";
                goto ERROR1;
            }
            this.DialogResult = DialogResult.OK;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            return;
        }

        public string StartPath
        {
            get
            {
                return this.textBox_startPath.Text;
            }
            set
            {
                this.textBox_startPath.Text = value;
            }
        }

        public string EndPath
        {
            get
            {
                return this.textBox_endPath.Text;
            }
            set
            {
                this.textBox_endPath.Text = value;
            }
        }

        public string StartPathLabel
        {
            get
            {
                return this.label_startPath.Text;
            }
            set
            {
                this.label_startPath.Text = value;
            }
        }

        public string EndPathLabel
        {
            get
            {
                return this.label_endPath.Text;
            }
            set
            {
                this.label_endPath.Text = value;
            }
        }
    }
}
