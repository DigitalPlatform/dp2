using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.rms.Client
{
    public partial class ExportDataDialog : Form
    {
        public ExportDataDialog()
        {
            InitializeComponent();
        }

        private void ExportDataDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.AllRecords == false)
            {
                if (string.IsNullOrEmpty(this.textBox_startNo.Text) == true)
                {
                    strError = "尚未指定起始记录ID";
                    goto ERROR1;
                }
                if (string.IsNullOrEmpty(this.textBox_endNo.Text) == true)
                {
                    strError = "尚未指定结束记录ID";
                    goto ERROR1;
                }
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

        public string StartID
        {
            get
            {
                return this.textBox_startNo.Text;
            }
            set
            {
                this.textBox_startNo.Text = value;
            }
        }

        public string EndID
        {
            get
            {
                return this.textBox_endNo.Text;
            }
            set
            {
                this.textBox_endNo.Text = value;
            }
        }

        public string DbPath
        {
            get
            {
                return this.textBox_dbPath.Text;
            }
            set
            {
                this.textBox_dbPath.Text = value;
            }

        }

        public bool AllRecords
        {
            get
            {
                return this.radioButton_all.Checked;
            }
            set
            {
                m_nPreventNest++;

                try
                {
                    if (value == true)
                    {
                        this.radioButton_all.Checked = true;
                        this.radioButton_startEnd.Checked = false;
                    }
                    else
                    {
                        this.radioButton_all.Checked = false;
                        this.radioButton_startEnd.Checked = true;
                    }

                }
                finally
                {
                    m_nPreventNest--;
                }

            }
        }

        int m_nPreventNest = 0;

        private void radioButton_all_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_all.Checked == true
    && m_nPreventNest == 0)
            {
                m_nPreventNest++;
                this.textBox_startNo.Text = "1";
                this.textBox_endNo.Text = "9999999999";
                m_nPreventNest--;
            }
        }

        private void textBox_startNo_TextChanged(object sender, EventArgs e)
        {
            if (m_nPreventNest == 0)
            {
                m_nPreventNest++;   // 防止radioButton_all_CheckedChanged()随动
                this.radioButton_startEnd.Checked = true;
                m_nPreventNest--;
            }
        }

        private void textBox_endNo_TextChanged(object sender, EventArgs e)
        {
            if (m_nPreventNest == 0)
            {
                m_nPreventNest++;      // 防止radioButton_all_CheckedChanged()随动
                this.radioButton_startEnd.Checked = true;
                m_nPreventNest--;
            }
        }

        private void button_setStartIdMin_Click(object sender, EventArgs e)
        {
            this.textBox_startNo.Text = "1";
        }

        private void button_setEndIdMax_Click(object sender, EventArgs e)
        {
            this.textBox_endNo.Text = "9999999999";
        }
    }
}
