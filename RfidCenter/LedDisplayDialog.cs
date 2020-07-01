using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RfidCenter
{
    public partial class LedDisplayDialog : Form
    {
        public LedDisplayDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string LedName
        {
            get
            {
                return this.textBox_ledName.Text;
            }
            set
            {
                this.textBox_ledName.Text = value;
            }
        }

        public string X
        {
            get
            {
                return this.textBox_x.Text;
            }
            set
            {
                this.textBox_x.Text = value;
            }
        }

        public string Y
        {
            get
            {
                return this.textBox_y.Text;
            }
            set
            {
                this.textBox_y.Text = value;
            }
        }

        public string FontSize
        {
            get
            {
                return this.textBox_fontSize.Text;
            }
            set
            {
                this.textBox_fontSize.Text = value;
            }
        }

        public string Effect
        {
            get
            {
                return this.textBox_effect.Text;
            }
            set
            {
                this.textBox_effect.Text = value;
            }
        }

        public string MoveSpeed
        {
            get
            {
                return this.textBox_moveSpeed.Text;
            }
            set
            {
                this.textBox_moveSpeed.Text = value;
            }
        }

        public string Duration
        {
            get
            {
                return this.textBox_duration.Text;
            }
            set
            {
                this.textBox_duration.Text = value;
            }
        }

        public string ExtendStyle
        {
            get
            {
                return this.textBox_style.Text;
            }
            set
            {
                this.textBox_style.Text = value;
            }
        }

        public string DisplayText
        {
            get
            {
                return this.textBox_text.Text;
            }
            set
            {
                this.textBox_text.Text = value;
            }
        }

        public string HorzAlign
        {
            get
            {
                return this.comboBox_horzAlign.Text;
            }
            set
            {
                this.comboBox_horzAlign.Text = value;
            }
        }

        public string VertAlign
        {
            get
            {
                return this.comboBox_vertAlign.Text;
            }
            set
            {
                this.comboBox_vertAlign.Text = value;
            }
        }
    }
}
