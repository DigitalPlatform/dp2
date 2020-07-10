using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace greenSetup
{
    public partial class SplashForm : Form
    {
        public string ImageFileName { get; set; }

        public SplashForm()
        {
            InitializeComponent();
        }

        private void SplashForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ImageFileName) == false)
            {
                var image = Image.FromFile(ImageFileName);
                this.pictureBox1.Image = image;

                // 设置窗口大小
                var old_size = this.Size;
                this.Size = this.pictureBox1.ClientSize;

                // 重新居中
                int x_delta = this.Size.Width - old_size.Width;
                int y_delta = this.Size.Height - old_size.Height;
                this.Location = new Point(this.Location.X - (x_delta / 2),
                    this.Location.Y - (y_delta / 2));
            }
        }

    }
}
