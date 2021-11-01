﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Drawing;

namespace dp2Circulation
{
    public partial class RegisterPalmprintDialog : Form
    {
        // public bool Finished { get; set; }

        public RegisterPalmprintDialog()
        {
            InitializeComponent();
        }

        public string Message
        {
            get
            {
                return this.label_text.Text;
            }
            set
            {
                this.label_text.Text = value;
            }
        }

        public Image Image
        {
            get
            {
                return this.pictureBox1.Image;
            }
            set
            {
                ImageUtil.SetImage(this.pictureBox1, value);

                // this.pictureBox1.Image = value;
            }
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string CancelButtonText
        {
            get
            {
                return this.button_cancel.Text;
            }
            set
            {
                this.button_cancel.Text = value;
            }
        }

        public void DisplayError(string strError, Color backColor)
        {
            // 显示错误
            this.Invoke((Action)(() =>
            {
                this.Image = Charging.PalmprintForm.BuildTextImage(
                strError,
                backColor,
                32,
                this.pictureBox1.Width);
            }));
        }
    }
}
