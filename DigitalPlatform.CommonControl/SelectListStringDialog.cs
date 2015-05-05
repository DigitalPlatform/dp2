using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public partial class SelectListStringDialog : Form
    {
        public SelectListStringDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listBox_content.SelectedIndex == -1)
            {
                MessageBox.Show(this, "请选择一个事项");
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

        public string SelectedValue
        {
            get
            {
                return (string)this.listBox_content.SelectedItem;
            }
            set
            {
                this.listBox_content.SelectedItem = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this.listBox_content.SelectedIndex;
            }
            set
            {
                this.listBox_content.SelectedIndex = value;
            }
        }

        public List<string> Values
        {
            get
            {
                List<string> results = new List<string>();
                foreach (string text in this.listBox_content.Items)
                {
                    results.Add(text);
                }
                return results;
            }
            set
            {
                this.listBox_content.Items.Clear();
                foreach (string text in value)
                {
                    this.listBox_content.Items.Add(text);
                }
            }
        }

        public string Comment
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

        private void listBox_content_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox_content.SelectedIndex != -1)
            {
                this.button_OK.Enabled = true;
            }
            else
            {
                this.button_OK.Enabled = false;
            }
        }

        private void listBox_content_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);
        }


    }
}
