using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 能在列表内容中显示制表符栏位的 ComboBox
    /// </summary>
    public partial class TabComboBox : ComboBox
    {
        [Category("Appearance")]
        [DescriptionAttribute("Left Part FontStyle")]
        [DefaultValue(typeof(FontStyle), "Regular")]
        public FontStyle LeftFontStyle
        {
            get
            {
                return this.m_leftFontStyle;
            }
            set
            {
                this.m_leftFontStyle = value;
            }
        }

        FontStyle m_leftFontStyle = FontStyle.Regular;

        [Category("Appearance")]
        [DescriptionAttribute("Right Part FontStyle")]
        [DefaultValue(typeof(FontStyle), "Regular")]
        public FontStyle RightFontStyle
        {
            get
            {
                return this.m_rightFontStyle;
            }
            set
            {
                this.m_rightFontStyle = value;
            }
        }

        FontStyle m_rightFontStyle = FontStyle.Regular;

        public bool RemoveRightPartAtTextBox = true;    // 在 textbox 域中删除文字的右边部分(也就是'\t'右边的部分，包括'\t')

        const int WM_CHANGED = API.WM_USER + 300;

        public TabComboBox()
        {
            try
            {
                InitializeComponent();
            }
            catch
            {

            }
        }

        /*
        // https://github.com/dotnet/winforms/issues/5406
        protected override void OnGotFocus(EventArgs e)
        {
            //Directly back
            return;

            base.OnGotFocus(e);
        }
        */

        private void TabComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            string strText = (string)this.Items[e.Index];
            string strLeft = "";
            string strRight = "";
            int nRet = strText.IndexOf("\t");
            if (nRet == -1)
                strLeft = strText;
            else
            {
                strLeft = strText.Substring(0, nRet);
                strRight = strText.Substring(nRet + 1);
            }

            using (Brush brushBack = new SolidBrush(e.BackColor))
            {
                e.Graphics.FillRectangle(brushBack, e.Bounds);
            }

            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                {
                    Font font = null;
                    try
                    {
                        if (this.LeftFontStyle == FontStyle.Regular)
                            font = e.Font;
                        else
                            font = new Font(e.Font, this.LeftFontStyle);

                        e.Graphics.DrawString(strLeft, font, brush, e.Bounds);
                    }
                    finally
                    {
                        if (font != e.Font && font != null)
                            font.Dispose();
                    }
                }

                SizeF size = e.Graphics.MeasureString(strRight, e.Font);
                RectangleF rightBound = new RectangleF(e.Bounds.Right - size.Width,
                    e.Bounds.Y,
                    e.Bounds.Width,
                    e.Bounds.Height);

                {
                    Font font = null;
                    try
                    {
                        if (this.RightFontStyle == FontStyle.Regular)
                            font = e.Font;
                        else
                            font = new Font(e.Font, this.RightFontStyle);

                        e.Graphics.DrawString(strRight, font, brush, rightBound);
                    }
                    finally
                    {
                        if (font != e.Font && font != null)
                            font.Dispose();
                    }
                }
            }
        }

        private void TabComboBox_TextChanged(object sender, EventArgs e)
        {
            if (RemoveRightPartAtTextBox == true)
            {
                // 此处不应用 this.BeginInvoke() 代替，因为 this 窗口可能还没有创建。
                API.PostMessage(this.Handle, WM_CHANGED, 0, 0);
            }
        }

        public static string GetLeftPart(string strText)
        {
            if (strText == null)
                return null;
            int nRet = strText.IndexOf("\t");
            if (nRet == -1)
                return strText;

            return strText.Substring(0, nRet).Trim();
        }

        // 删除掉批次号右边的部分
        void RemoveRightPart()
        {
            string strText = this.Text;
            int nRet = strText.IndexOf("\t");
            if (nRet != -1)
                this.Text = strText.Substring(0, nRet);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CHANGED:
                    RemoveRightPart();
                    return;
            }

            // Debug.WriteLine(m.Msg.ToString());
            base.DefWndProc(ref m);
        }

    }
}
