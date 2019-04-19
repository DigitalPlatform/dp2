using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.CommonControl
{
    public partial class CheckedComboBox : UserControl
    {
        public bool HideCloseButton = true;
        public int DropDownHeight = -1; // -1 表示用缺省值
        public int DropDownWidth = -1;  // -1 表示用缺省值

        // 当下拉列表出现的时候。也就是PropertyStringDialog对话框打开前
        [Category("New Event")]
        public event EventHandler DropDown = null;

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler TextChanged
        {
            // http://stackoverflow.com/questions/9370448/add-attribute-to-base-event
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }
#if NO
        public new event EventHandler TextChanged
        {
            add
            {
                this.textBox_text.TextChanged += value;
            }
            remove
            {
                this.textBox_text.TextChanged -= value;
            }
        }
#endif

        public event ItemCheckedEventHandler ItemChecked = null;

        public bool ReturnFirstPart = true;

        string m_strCaption = "";

        // 全部可用值的列表
        public List<string> Items = new List<string>();


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

        // 把textbox的边框理解为总的边框
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

        /*
        public override Color BackColor
        {
            get
            {
                return this.textBox_text.BackColor;
            }
            set
            {
                this.textBox_text.BackColor = value;
                base.BackColor = value;
            }
        }*/

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
                this.textBox_text.Enabled = value;
                // this.button_dropDownList.Enabled = value;

                /*
                if (value == false)
                {
                    this.m_savedBackColor = this.BackColor;
                    this.BackColor = SystemColors.Control;
                }
                else
                    this.BackColor = this.m_savedBackColor;
                 * */

                base.Enabled = value;   //  Enabled; BUG!!! 2017/3/5 消除

                this.Invalidate();
                this.Update();
            }
        }

        bool m_bInitial = false;
        Point m_oldTextBoxLocation;

        public CheckedComboBox()
        {
            try
            {
                this.m_bInitial = true;
                InitializeComponent();
                this.m_bInitial = false;

                m_oldTextBoxLocation = this.textBox_text.Location;
                OnTextBoxHeightChanged();   // 初始化高度
            }
            catch (Exception ex)
            {
                // 2019/4/19
                //if (this.DesignMode == false)
                //    throw ex;
            }
        }

#if NO
        void OnTextBoxHeightChanged()
        {
            int nDelta = this.textBox_text.Location.Y;
            this.Height = this.textBox_text.Height + 2 * nDelta + this.Margin.Vertical + this.Padding.Vertical + this.textBox_text.Margin.Vertical;


            /*
            this.button_dropDownList.Height = this.Height - 2;  // 让出边框
            this.button_dropDownList.Location = new Point(this.ClientRectangle.Width - this.button_dropDownList.Width,
                this.button_dropDownList.Location.Y);
            this.textBox_text.Width = this.ClientRectangle.Width - this.button_dropDownList.Width*2;
             * */
            nDelta = this.button_dropDownList.Location.Y;
            int nCenterY = this.Height / 2;
            this.button_dropDownList.Location = new Point(this.button_dropDownList.Location.X,
                nCenterY - this.button_dropDownList.Height / 2 );

        }
#endif
        Padding KernelMargin
        {
            get
            {
                int nDelta = 0;
                return new System.Windows.Forms.Padding(this.Margin.Left + nDelta, this.Margin.Top + nDelta,
                    this.Margin.Right + nDelta, this.Margin.Bottom + nDelta);
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

        int m_nInHeightChanging = 0;

        void OnTextBoxHeightChanged()
        {
            try
            {
                if (this.m_nInHeightChanging > 0)
                    return;

                int nDelta = this.m_oldTextBoxLocation.Y;

                this.m_nInHeightChanging++;
                this.Height = this.textBox_text.Height
                    + 2 * nDelta /*+ this.KernelMargin.Vertical*/ + this.Padding.Vertical
                    + BorderSize.Height * 2;
                this.m_nInHeightChanging--;

                this.textBox_text.Location = new Point(/*this.KernelMargin.Left + */this.Padding.Left + this.m_oldTextBoxLocation.X,
                    /*this.KernelMargin.Top + */this.Padding.Top + this.m_oldTextBoxLocation.Y);
                this.textBox_text.Width = this.Width /*- this.KernelMargin.Horizontal*/ - this.Padding.Horizontal - this.RectButton.Width - BorderSize.Width * 2 - 1;

                this.Invalidate();
            }
            catch
            {

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

        public override string Text
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

        private void button_dropDownList_Click(object sender, EventArgs e)
        {

        }

        private void textBox_text_TextChanged(object sender, EventArgs e)
        {
            this.OnTextChanged(e);

#if NO
            if (this.TextChanged != null)
                this.TextChanged(this, e);
#endif
        }

        public void SelectAll()
        {
            this.textBox_text.SelectAll();
        }

        public new bool Focus()
        {
            return this.textBox_text.Focus();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            OnTextBoxHeightChanged();
        }

        private void button_dropDownList_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("MouseDown");

            /*
             * // TODO: MouseDown消息在下拉的LostFocus以后才得到，暂时无法解决识别问题
            // 下拉已经显示的情况下，点本按钮
            if (dlg != null && dlg.IsDisposed == false)
            {
                // 不再重复打开
                return;
            }
             * */

            if (this.DropDown != null)
                this.DropDown(this, e);

            PropertyStringDialog dlg = null;

            dlg = new PropertyStringDialog();
            dlg.Font = this.Font;
            dlg.CheckedComboBox = this;
            dlg.Text = this.Caption;
            dlg.PropertyString = this.textBox_text.Text;
            dlg.Items = this.Items;
            dlg.ReturnFirstPart = this.ReturnFirstPart;

            dlg.HideCloseButton = this.HideCloseButton;
            dlg.StartPosition = FormStartPosition.Manual;
            dlg.Location = this.PointToScreen(
                new Point(0,
                0 + this.Size.Height)
                );
            if (this.DropDownWidth == -1)
            {
                int nWidth = this.Width;
                if (dlg.Width < nWidth)
                    dlg.Width = nWidth;
            }
            else
            {
                dlg.Width = this.DropDownWidth;
            }

            if (this.DropDownHeight != -1)
                dlg.Height = this.DropDownHeight;

            dlg.Show(); // 无模式对话框
            // dlg.ShowDialog(this);

            /*
            if (dlg.DialogResult != DialogResult.OK)
                return;
             * */
            try
            {
                while (dlg.IsDisposed == false)
                {
                    Application.DoEvents();
                }
                Thread.Sleep(1);
            }
            catch
            {
            }

            this.textBox_text.Text = dlg.PropertyString;
        }

        public void OnItemChecked(ItemCheckedEventArgs e)
        {
            if (this.ItemChecked != null)
                this.ItemChecked(this, e);
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
            }
        }

        private void CheckedComboBox_Paint(object sender, PaintEventArgs e)
        {
            if (this.textBox_text.Enabled == false)
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
                    /*
                    ControlPaint.DrawComboButton(e.Graphics,
        this.RectButton,
        ButtonState.Inactive | ButtonState.Flat);
                     * */
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

        private void textBox_text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (this.textBox_text.Enabled == true)
                {
                    button_dropDownList_MouseDown(sender, null);
                }
            }

            this.OnKeyDown(e);
        }

        private void textBox_text_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void CheckedComboBox_FontChanged(object sender, EventArgs e)
        {
            OnTextBoxHeightChanged();
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

        private void CheckedComboBox_MouseClick(object sender, MouseEventArgs e)
        {
            /*
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (GuiUtil.PtInRect(e.X, e.Y, this.RectButton) == true)
                {
                    button_dropDownList_MouseDown(sender, e);
                }
            }
             * */
        }

        private void CheckedComboBox_PaddingChanged(object sender, EventArgs e)
        {
            OnTextBoxHeightChanged();
        }

        private void CheckedComboBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left
                && this.textBox_text.Enabled == true)
            {
                if (GuiUtil.PtInRect(e.X, e.Y, this.RectButton) == true)
                {
                    button_dropDownList_MouseDown(sender, e);
                }
            }
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

        // 处理“所有”事项和其他事项的排斥关系
        public static void ProcessItemChecked(ItemCheckedEventArgs e, string strList)
        {
            ListView list = e.Item.ListView;

            if (StringUtil.IsInList(e.Item.Text.ToLower(), strList))
            {
                if (e.Item.Checked == true)
                {
                    // 如果当前勾选了“全部”，则清除其余全部事项的勾选
                    foreach (ListViewItem item in list.Items)
                    {
                        if (StringUtil.IsInList(item.Text.ToLower(), strList))
                            continue;
                        if (item.Checked != false)
                            item.Checked = false;
                    }
                }
            }
            else
            {
                if (e.Item.Checked == true)
                {
                    // 如果勾选的不是“全部”，则要清除“全部”上可能的勾选
                    foreach (ListViewItem item in list.Items)
                    {
                        if (StringUtil.IsInList(item.Text.ToLower(), strList))
                        {
                            if (item.Checked != false)
                                item.Checked = false;
                        }
                    }
                }
            }
        }

    }
}
