using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.Drawing;

namespace dp2Circulation
{
    public partial class RotateControl : UserControl
    {
        public event EventHandler OrentationChanged = null;

        int _degree = 0;

        public int RotateDegree
        {
            get
            {
                return this._degree;
            }
            set
            {
                this._degree = value;
                SetImage(value);
                if (this.OrentationChanged != null)
                {
                    this.OrentationChanged(this, new EventArgs());
                }
            }
        }

        // 构造函数
        public RotateControl()
        {
            InitializeComponent();

            this.pictureBox1.Image = this.pictureBox1.InitialImage;
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            // this.Size = this.pictureBox1.Size;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            AutoRotate();
        }

        void AutoRotate()
        {
            int nDegree = (this._degree + 90) % 360;
            this.RotateDegree = nDegree;
        }

        void SetImage(int value)
        {
            int nDegree = value % 360;
            if (nDegree == 0)
            {
                this.pictureBox1.Image = new Bitmap(this.pictureBox1.InitialImage);
                return;
            }

            RotateFlipType type = RotateFlipType.Rotate90FlipNone;
            if (nDegree == 90)
                type = RotateFlipType.Rotate90FlipNone;
            if (nDegree == 180)
                type = RotateFlipType.Rotate180FlipNone;
            if (nDegree == 270)
                type = RotateFlipType.Rotate270FlipNone;

            {
                // Image image = (Image)this.pictureBox1.InitialImage.Clone();  // BUG!!!
                Image image = new Bitmap(this.pictureBox1.InitialImage);    // 2017/2/27 
                if (image != null)
                {
                    image.RotateFlip(type);
                    // this.pictureBox1.Image = image; // 这里可能有内存泄露?

                    ImageUtil.SetImage(this.pictureBox1, image);  // 2016/12/28
                }
            }
        }

    }
}
