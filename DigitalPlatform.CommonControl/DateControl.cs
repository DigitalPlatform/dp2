using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;

using DigitalPlatform.GUI;

namespace DigitalPlatform.CommonControl
{
    public partial class DateControl : UserControl
    {
        string m_strCaption = "";

        int IngoreTextChange = 0;

        [Category("New Event")]
        public event EventHandler DateTextChanged = null;

        public DateControl()
        {
            this.m_bInitial = true;
            InitializeComponent();
            this.m_bInitial = false;

            m_oldTextBoxLocation = this.maskedTextBox_date.Location;

            OnTextBoxHeightChanged();   // 初始化高度
        }

        [Category("Appearance")]
        [DescriptionAttribute("Caption")]
        [DefaultValue(typeof(string), "")]
        public string Caption
        {
            get
            {
                return this.m_strCaption;
            }
            set
            {
                this.m_strCaption = value;
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "None")]
        public new BorderStyle BorderStyle
        {
            get
            {
                return base.BorderStyle;
            }
            set
            {
                base.BorderStyle = value;

                OnTextBoxHeightChanged();   // 边框改变，导致高度改变

            }
        }

        bool m_bEnabled = true;

        Color m_savedBackColor = SystemColors.Window;

        public new bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                this.maskedTextBox_date.Enabled = value;

                base.Enabled = Enabled;

                this.Invalidate();
                this.Update();
            }
        }


        private void maskedTextBox_date_TextChanged(object sender, EventArgs e)
        {
            if (IngoreTextChange > 0)
                return;

            if (this.DateTextChanged != null)
            {
                this.DateTextChanged(this, e);  // 205/5/25 以前为 sender
            }

            // this.OnTextChanged(e);
        }

        public override string Text
        {
            get
            {
                return this.maskedTextBox_date.Text;
            }
            set
            {
                this.maskedTextBox_date.Text = value;
            }
        }


        // 当前value是否为空值
        public bool IsValueNull()
        {
            if (this.Value == new DateTime((long)0))
                return true;
            return false;
        }


        public DateTime Value
        {
            get
            {
                // get pure text
                string strPureText = GetPureDateText();

                if (strPureText.Trim() == "")
                    return new DateTime((long)0);

                try
                {
                    DateTime date = DateTime.Parse(this.maskedTextBox_date.Text,
                        this.maskedTextBox_date.Culture,
                        DateTimeStyles.NoCurrentDateDefault);
                    return date;
                }
                catch
                {
                    return new DateTime((long)0);
                }

            }
            set
            {
                if (value == new DateTime((long)0))
                {
                    this.maskedTextBox_date.Text = "";
                    return;
                }

                this.maskedTextBox_date.Text = GetDateString(value);
            }
        }

        static string GetDateString(DateTime date)
        {
            return date.Year.ToString().PadLeft(4, '0') + "年"
                + date.Month.ToString().PadLeft(2, '0') + "月"
                + date.Day.ToString().PadLeft(2, '0') + "日";
        }

        string GetPureDateText()
        {
            // get pure text
            MaskFormat oldformat = this.maskedTextBox_date.TextMaskFormat;

            this.IngoreTextChange++;

            this.maskedTextBox_date.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
            string strPureText = this.maskedTextBox_date.Text;
            this.maskedTextBox_date.TextMaskFormat = oldformat;

            this.IngoreTextChange--;

            return strPureText;
        }

        private void maskedTextBox_date_Validating(object sender, CancelEventArgs e)
        {

            string strPureText = GetPureDateText();

            if (strPureText.Trim() == "")
                return; // blank value

            try
            {
                DateTime date = DateTime.Parse(this.maskedTextBox_date.Text,
                    this.maskedTextBox_date.Culture,
                    DateTimeStyles.NoCurrentDateDefault);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, this.Text + " error : " + ex.Message);
                e.Cancel = true;
            }

        }



        private void button_findDate_Click(object sender, EventArgs e)
        {
            GetDateDlg dlg = new GetDateDlg();
            GuiUtil.SetControlFont(dlg, this.Font);

            dlg.Text = this.Caption;
            dlg.DateTime = this.Value;
            // dlg.StartLocation = Control.MousePosition;

            dlg.StartPosition = FormStartPosition.Manual;
            dlg.Location = this.PointToScreen(
                new Point(this.Margin.Horizontal,
                4 + this.Height + this.Margin.Vertical)
                );
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.Value = dlg.DateTime;
        }

        private void DateControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left
    && this.maskedTextBox_date.Enabled == true)
            {
                if (GuiUtil.PtInRect(e.X, e.Y, this.RectButton) == true)
                {
                    button_findDate_Click(sender, e);
                }
            }
        }

        Rectangle RectButton
        {
            get
            {
                int nButtonHeight = this.Height /*- this.KernelMargin.Vertical*/ - BorderSize.Height * 2 - 1;
                int nButtonWidth = (int)((float)nButtonHeight * 0.75);

                int nCenterY = this.Height / 2;
                int x = this.ClientRectangle.Width - nButtonWidth - 1/* - this.KernelMargin.Right*/;
                int y = nCenterY - nButtonHeight / 2 - BorderSize.Height - 1;
                return new Rectangle(x, y, nButtonWidth, nButtonHeight);
            }
        }

        Size BorderSize
        {
            get
            {
                Size border_size = new System.Drawing.Size(0, 0);
                if (this.BorderStyle == BorderStyle.Fixed3D)
                    border_size = SystemInformation.Border3DSize;
                else if (this.BorderStyle == System.Windows.Forms.BorderStyle.FixedSingle)
                    border_size = SystemInformation.BorderSize;

                return border_size;
            }
        }

        Padding KernelMargin
        {
            get
            {
                int nDelta = 0;
                return new System.Windows.Forms.Padding(this.Margin.Left + nDelta, this.Margin.Top + nDelta,
                    this.Margin.Right + nDelta, this.Margin.Bottom + nDelta);
            }
        }


        bool m_bInitial = false;
        Point m_oldTextBoxLocation;

        int m_nInHeightChanging = 0;

        void OnTextBoxHeightChanged()
        {
            if (this.m_nInHeightChanging > 0)
                return;

            int nDelta = this.m_oldTextBoxLocation.Y;

            this.m_nInHeightChanging++;
            this.Height = this.maskedTextBox_date.Height
                + 2 * nDelta /*+ this.KernelMargin.Vertical*/ + this.Padding.Vertical
                + BorderSize.Height * 2;
            this.m_nInHeightChanging--;


            this.maskedTextBox_date.Location = new Point(/*this.KernelMargin.Left + */this.Padding.Left + this.m_oldTextBoxLocation.X,
                /*this.KernelMargin.Top + */this.Padding.Top + this.m_oldTextBoxLocation.Y);
            this.maskedTextBox_date.Width = this.Width /*- this.KernelMargin.Horizontal*/ - this.Padding.Horizontal - this.RectButton.Width - BorderSize.Width * 2 - 1;

            this.Invalidate();
        }

        public void SelectAll()
        {
            this.maskedTextBox_date.SelectAll();
        }

        public new bool Focus()
        {
            return this.maskedTextBox_date.Focus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            OnTextBoxHeightChanged();
        }

        static void DrawFlatComboButton(Graphics g,
    Rectangle rect,
    bool bActive)
        {
            using (Brush brush = new SolidBrush(
                SystemColors.ButtonFace))
            {
                g.FillRectangle(brush, rect);
            }

            if (bActive == true)
            {
                using (Pen penBorder = new Pen(SystemColors.Window))
                {
                    g.DrawRectangle(penBorder, rect);
                }
            }

            int nCenterX = rect.X + rect.Width / 2 + (rect.Width % 2);
            int nCenterY = rect.Y + rect.Height / 2 + (rect.Height % 2);

            using (Pen pen = new Pen(
                bActive ? SystemColors.ControlText : SystemColors.GrayText))
            {
                Point pt1 = new Point(nCenterX - 2, nCenterY - 1);
                Point pt2 = new Point(nCenterX + 2, nCenterY - 1);
                g.DrawLine(pen, pt1, pt2);

                pt1 = new Point(nCenterX - 1, nCenterY);
                pt2 = new Point(nCenterX + 1, nCenterY);
                g.DrawLine(pen, pt1, pt2);

                pt1 = new Point(nCenterX, nCenterY + 1);
                using (Brush brush = new SolidBrush(pen.Color))
                {
                    g.FillRectangle(brush, pt1.X, pt1.Y, 1, 1); // draw one pixel
                }
                // g.DrawLine(pen, pt1, pt2);
            }
        }

        private void DateControl_Paint(object sender, PaintEventArgs e)
        {
            if (this.maskedTextBox_date.Enabled == false)
            {
                using (Brush brush = new SolidBrush(SystemColors.Control))
                {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }

                using (Pen pen = new Pen(Color.DarkGray))
                {

                    Rectangle rect = this.ClientRectangle;
                    rect.Height--;
                    rect.Width--;

                    // 画边框
                    e.Graphics.DrawRectangle(pen, rect);
                }

                // 按钮
                if (this.m_flatstyle == System.Windows.Forms.FlatStyle.Flat)
                {
                    DrawFlatComboButton(e.Graphics, this.RectButton, false);
                }
                else
                {
                    if (ComboBoxRenderer.IsSupported == true)
                    {
                        ComboBoxRenderer.DrawDropDownButton(e.Graphics,
    this.RectButton,
                            System.Windows.Forms.VisualStyles.ComboBoxState.Disabled);
                    }
                    else
                    {
                        ControlPaint.DrawComboButton(e.Graphics,
    this.RectButton,
    ButtonState.Inactive);
                    }
                }
            }
            else
            {
                using (Brush brush = new SolidBrush(SystemColors.Window))
                {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }

                // 按钮
                if (this.m_flatstyle == System.Windows.Forms.FlatStyle.Flat)
                {
                    DrawFlatComboButton(e.Graphics, this.RectButton, true);
                }
                else
                {
                    if (ComboBoxRenderer.IsSupported == true)
                    {
                        ComboBoxRenderer.DrawDropDownButton(e.Graphics,
    this.RectButton,
                            System.Windows.Forms.VisualStyles.ComboBoxState.Normal);
                    }
                    else
                    {
                        ControlPaint.DrawComboButton(e.Graphics,
    this.RectButton,
    ButtonState.Normal);
                    }
                }
            }
        }

        FlatStyle m_flatstyle = FlatStyle.Standard;

        [Category("Appearance")]
        [DescriptionAttribute("FlatStyle")]
        [DefaultValue(typeof(FlatStyle), "Standard")]
        public FlatStyle FlatStyle
        {
            get
            {
                return this.m_flatstyle;
            }
            set
            {
                this.m_flatstyle = value;
            }
        }

        private void DateControl_FontChanged(object sender, EventArgs e)
        {
            OnTextBoxHeightChanged();
        }

        private void DateControl_PaddingChanged(object sender, EventArgs e)
        {
            OnTextBoxHeightChanged();

        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            // 模仿ComboBox TextBox等，Location和Size可以被缩放，但是Margin和Padding不变
            Padding margin = this.Margin;
            Padding Padding = this.Padding;
            base.ScaleControl(factor, specified);
            this.Margin = margin;
            this.Padding = Padding;
        }

        public new Control.ControlCollection Controls
        {
            get
            {
                if (this.m_bInitial == false)
                    return new ControlCollection(this);
                else
                    return base.Controls;
            }
        }
    }
}
