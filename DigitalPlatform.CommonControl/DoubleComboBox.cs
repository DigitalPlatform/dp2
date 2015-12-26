using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public partial class DoubleComboBox : UserControl
    {
        public event EventHandler SelectedIndexChanged;

        public DoubleComboBox()
        {
            InitializeComponent();
        }

        public override Size MaximumSize
        {
            get
            {
                Size size = base.MaximumSize;
                int nLimitHeight = this.TextBox.Height * 2 + 4;
                if (size.Height > nLimitHeight
                    || size.Height == 0)
                    size.Height = nLimitHeight;

                return size;
            }
            set
            {
                base.MaximumSize = value;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size size = base.MinimumSize;
                int nLimitHeight = this.ComboBox.Height + this.TextBox.Height + 4;
                // int nLimitWidth = this.ComboBox.Location.X + 50 + 4;
                size.Height = nLimitHeight;
                // size.Width = nLimitWidth;
                if (size.Width > 80)
                    size.Width = 80;

                return size;
            }
            set
            {
                base.MinimumSize = value;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (specified == BoundsSpecified.All)
                base.SetBoundsCore(x, y, width , height, specified);
            else
                base.SetBoundsCore(x, y, width, height, specified);
        }

        public new string Text
        {
            get
            {
                return this.ComboBox.Text;
            }
            set
            {
                this.ComboBox.Text = value;
                SetVisibleState();
            }
        }

        public string OldText
        {
            get
            {
                return this.TextBox.Text;
            }
            set
            {
                this.TextBox.Text = value;
                SetVisibleState();
            }
        }

        // 根据两个值是否相同，设置TextBox是否可见
        // 两个值相同的时候，TextBox不可见；不同的时候，TextBox可见
        void SetVisibleState()
        {
            if (this.ComboBox.Text != this.TextBox.Text)
            {
                this.TextBox.Visible = true;
                //this.Height = 28 * 2;
            }
            else
            {
                this.TextBox.Visible = false;
                //this.Height = 28;
            }
        }

        private void ComboBox_TextChanged(object sender, EventArgs e)
        {
            SetVisibleState();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            SetVisibleState();
        }

        private void ComboBox_SizeChanged(object sender, EventArgs e)
        {
            this.ComboBox.Invalidate();
        }

        private void DoubleComboBox_SizeChanged(object sender, EventArgs e)
        {
            this.ComboBox.Width = this.Width;
            this.TextBox.Width = this.Width;
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, e);
            }
        }
    }
}
