using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestLucene
{
    public partial class DocumentDialog : Form
    {
        public DocumentDialog()
        {
            InitializeComponent();
        }

        public string ID
        {
            get
            {
                return this.textBox_id.Text;
            }
            set
            {
                this.textBox_id.Text = value;
            }
        }

        public string Title
        {
            get
            {
                return this.textBox_title.Text;
            }
            set
            {
                this.textBox_title.Text = value;
            }
        }

        public string Title2
        {
            get
            {
                return this.textBox_title2.Text;
            }
            set
            {
                this.textBox_title2.Text = value;
            }
        }

        public string Author
        {
            get
            {
                return this.textBox_author.Text;
            }
            set
            {
                this.textBox_author.Text = value;
            }
        }

        public string Author2
        {
            get
            {
                return this.textBox_author2.Text;
            }
            set
            {
                this.textBox_author2.Text = value;
            }
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
    }
}
