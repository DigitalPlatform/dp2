using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

using DigitalPlatform.GUI;

namespace DigitalPlatform.GUI
{
    public class AutoSizeLable : Label
    {
        private bool mGrowing;
        private int mMaxWidth;
        public AutoSizeLable()
        {
            this.AutoSize = false;
            mMaxWidth = 150;
        }
        [DefaultValue(150)]
        public int MaxWidth
        {
            get { return mMaxWidth; }
            set { mMaxWidth = value; resizeLabel(); }
        }
        private void resizeLabel()
        {
            if (mGrowing) return;
            try
            {
                mGrowing = true;
                Size sz = new Size(mMaxWidth, Int32.MaxValue);
                sz = TextRenderer.MeasureText(this.Text, this.Font, sz, TextFormatFlags.WordBreak);
                if (sz.Height > this.Font.Height) sz = new Size(mMaxWidth, sz.Height);
                this.ClientSize = new Size(sz.Width + this.Padding.Horizontal, sz.Height + this.Padding.Vertical);
            }
            finally
            {
                mGrowing = false;
            }
        }

#if NO
        private void resizeLabel()
        {
            if (mGrowing) return;
            try
            {
                mGrowing = true;
                Size sz = new Size(this.ClientSize.Width, Int32.MaxValue);
                sz = TextRenderer.MeasureText(this.Text, this.Font, sz, TextFormatFlags.WordBreak);
                this.ClientSize = new Size(this.ClientSize.Width, sz.Height + this.Padding.Vertical);
            }
            finally
            {
                mGrowing = false;
            }
        }
#endif

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            resizeLabel();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            resizeLabel();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            resizeLabel();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            resizeLabel();
        }
    }
}
