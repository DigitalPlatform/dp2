using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    // http://stackoverflow.com/questions/10574998/multiline-textbox-auto-adjust-its-height-according-to-the-amount-of-text
    /// <summary>
    /// 能根据文本内容自动调整高度的订制 TextBox 控件
    /// </summary>
    public class AutoHeightTextBox : TextBox
    {
        int _lineCount = 1;

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (this.Visible == true)
                _lineCount = 0; // 迫使后面重新初始化 Height
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (this.Visible == true)
                SetHeight();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            SetHeight();
        }

        public void SetHeight()
        {
            var numberOfLines = API.SendMessage(this.Handle, API.EM_GETLINECOUNT, 0, 0).ToInt32();
            numberOfLines = Math.Max(1, numberOfLines);

            if (numberOfLines != _lineCount)
            {
                int nBorderWidth = 0;
                if (this.BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
                    nBorderWidth = SystemInformation.Border3DSize.Height * 2;
                else if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
                    nBorderWidth = SystemInformation.BorderSize.Height * 2;

                int nNewHeight = (this.Font.Height + 2) * numberOfLines + nBorderWidth;
                if (this.Height != nNewHeight)
                    this.Height = nNewHeight;

                _lineCount = numberOfLines;
            }
        }
    }
}
