using System;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public partial class DoubleTextBox : UserControl
    {
        public DoubleTextBox()
        {
            InitializeComponent();
        }


#if NO
        public override Size GetPreferredSize(Size proposedSize)
        {
            Size size = base.GetPreferredSize(proposedSize);
            size.Height = this.TextBox.Height * 2 + 4;
            return size;
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            int nLimitHeight = this.TextBox.Height * 2 + 4;
            if (height > nLimitHeight)
                height = nLimitHeight;

            base.SetBoundsCore(x, y, width, height, specified);
        }
#endif

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
                int nLimitHeight = this.TextBox.Height * 2 + 4;
                // int nLimitWidth = this.TextBox.Location.X + this.TextBox.Width + 4;
                size.Height = nLimitHeight;
                // size.Width = nLimitWidth;

                return size;
            }
            set
            {
                base.MinimumSize = value;
            }
        }

#if NO
        protected override Size SizeFromClientSize(Size clientSize)
        {
            return base.SizeFromClientSize(clientSize);
        }

        protected override void SetClientSizeCore(int x, int y)
        {
            base.SetClientSizeCore(x, y);
        }
#endif

        public bool ReadOnly
        {
            get
            {
                return this.TextBox.ReadOnly;
            }
            set
            {
                this.TextBox.ReadOnly = value;
            }
        }

        public override string Text
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

        public string OldText
        {
            get
            {
                return this.SecondTextBox.Text;
            }
            set
            {
                this.SecondTextBox.Text = value;
                SetVisibleState();
            }
        }

        // 根据两个值是否相同，设置TextBox是否可见
        // 两个值相同的时候，TextBox不可见；不同的时候，TextBox可见
        void SetVisibleState()
        {
            if (this.TextBox.Text != this.SecondTextBox.Text)
            {
                this.SecondTextBox.Visible = true;
                //this.Height = 28 * 2;
                this.SecondTextBox.Location = new Point(this.SecondTextBox.Location.X,
                    this.TextBox.Height + 2);   // 没有这句话以前，SecondTextBox可能会靠下，看不到的位置
            }
            else
            {
                this.SecondTextBox.Visible = false;
                //this.Height = 28;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            SetVisibleState();
        }

        private void SecondTextBox_TextChanged(object sender, EventArgs e)
        {
            SetVisibleState();

        }

        private void DoubleTextBox_SizeChanged(object sender, EventArgs e)
        {
            this.TextBox.Width = this.Width;
            this.SecondTextBox.Width = this.Width;
        }
    }
}
