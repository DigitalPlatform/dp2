using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Drawing.Printing;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.Drawing;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;
using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 设计标签的面板控件。包含输入参数的界面，和一个所见即所得的显示控件
    /// </summary>
    public partial class LabelDefControl : UserControl
    {
        public LabelParam LabelParam
        {
            get
            {
                return this.labelDesignControl1.LabelParam;
            }
            set
            {
                this.labelDesignControl1.LabelParam = value;

                this.labelDesignControl1.Invalidate();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LabelDefControl()
        {
            InitializeComponent();

            this.numericUpDown_pageWidth.Maximum = decimal.MaxValue;
            this.numericUpDown_pageHeight.Maximum = decimal.MaxValue;
            this.numericUpDown_labelWidth.Maximum = decimal.MaxValue;
            this.numericUpDown_labelHeight.Maximum = decimal.MaxValue;
            this.numericUpDown_lineSep.Maximum = decimal.MaxValue;

            this.comboBox_currentUnit.SelectedIndex = 0;

            this.textBox_pagePadding.ValidateWarningFormat = "页面内容边距值格式不正确：{0}。请重新输入";
            this.textBox_labelPadding.ValidateWarningFormat = "标签内容边距值格式不正确：{0}。请重新输入";
        }

        /// <summary>
        /// 小数位数
        /// </summary>
        public int DecimalPlaces
        {
            get
            {
                return (int)this.numericUpDown_decimalPlaces.Value;
            }
            set
            {
                this.numericUpDown_decimalPlaces.Value = value;
            }
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer1);
                controls.Add(this.tabControl1);
                controls.Add(this.comboBox_currentUnit);
                controls.Add(this.numericUpDown_decimalPlaces);
                controls.Add(this.listView_lineFormats);
                controls.Add(this.checkBox_gridLine);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer1);
                controls.Add(this.tabControl1);
                controls.Add(this.comboBox_currentUnit);
                controls.Add(this.numericUpDown_decimalPlaces);
                controls.Add(this.listView_lineFormats);
                controls.Add(this.checkBox_gridLine);
                GuiState.SetUiState(controls, value);
            }
        }

        /// <summary>
        /// 构造样例标签内容文字
        /// </summary>
        /// <param name="nLineCount">标签内容的行数</param>
        /// <returns>文字</returns>
        public static string BuildSampleLabelText(int nLineCount = 2)
        {
            StringBuilder text = new StringBuilder(4096);
            for (int i = 0; i < 300; i++)
            {
                for (int j = 0; j < nLineCount; j++)
                {
                    if (j == 0)
                        text.Append("label " + (i + 1).ToString() + "\r\n");
                    else
                    {
                        if ((j % 2) == 0)
                            text.Append("第 " + (j + 1).ToString() + " 行文字\r\n");
                        else
                            text.Append("line " + (j + 1).ToString() + " text\r\n");
                    }
                }
                text.Append("***\r\n");
            }

            return text.ToString();
        }

        /// <summary>
        /// 设置缺省的参数
        /// </summary>
        /// <param name="bSetText">是否要同时设置标签文字内容</param>
        public void SetDefaultValue(bool bSetText = true)
        {
            LabelParam param = new LabelParam();

            param.PageWidth = 640;
            param.PageHeight = 540;

            param.PageMargins = new DecimalPadding(20, 20, 20, 20);

            param.LabelWidth = 100;
            param.LabelHeight = 50;

            param.LabelPaddings = new DecimalPadding(5, 5, 5, 5);

            SetLabelParam(param);

            this.labelDesignControl1.LabelParam = param;

            if (bSetText)
            {
                string strText = BuildSampleLabelText();

                Stream stream = new MemoryStream(Encoding.Default.GetBytes(strText));
                StreamReader sr = new StreamReader(stream, Encoding.Default);

                string strError = "";
                this.labelDesignControl1.SetLabelFile(sr, out strError);
            }

            this.labelDesignControl1.Invalidate();
            _panelVersion++;
            SetChanged();
        }

        GraphicsUnit _currentUnit = GraphicsUnit.Display;

        public GraphicsUnit CurrentUnit
        {
            get
            {
                return this._currentUnit;
            }
            set
            {
                GraphicsUnit old_unit = this._currentUnit;

                // 中间这一段连带发生的 ValueChanged()，可以通过 部件 CurrentUnit 和 this._currentUnit 不一致探测到，可以忽略同步到 label_param
                // this.Value = ConvertValue(this._currentUnit, value, this.Value);
                this.numericUpDown_pageWidth.CurrentUnit = value;
                this.numericUpDown_pageHeight.CurrentUnit = value;

                this.numericUpDown_labelWidth.CurrentUnit = value;
                this.numericUpDown_labelHeight.CurrentUnit = value;

                this.numericUpDown_lineSep.CurrentUnit = value;

                this.textBox_pagePadding.CurrentUnit = value;
                this.textBox_labelPadding.CurrentUnit = value;


                this._currentUnit = value;

                if (old_unit != this._currentUnit)
                {
                    // 行格式 listview 的显示刷新
                    RefreshListLineFormatsUnits();
                }

                if (value == GraphicsUnit.Display)
                    this.comboBox_currentUnit.Text = "1/100 英寸";
                else if (value == GraphicsUnit.Millimeter)
                    this.comboBox_currentUnit.Text = "毫米";
                else
                    throw new Exception("暂不支持 " + value.ToString());
            }
        }

        LabelParam GetLabelParam()
        {
            LabelParam param = new LabelParam();
            param.PageWidth = (double)this.numericUpDown_pageWidth.UniverseValue;
            param.PageHeight = (double)this.numericUpDown_pageHeight.UniverseValue;
            // param.Landscape = this.checkBox_landscape.Checked;
            param.RotateDegree = this.rotateControl1.RotateDegree;

            try
            {
                // 可能会抛出 ArgumentException 异常
                DecimalPadding padding = PaddingDialog.ParsePaddingString(this.textBox_pagePadding.UniverseText);

#if NO
                param.PageMargins = new System.Drawing.Printing.Margins(padding.Left,
                    padding.Right,
                    padding.Top,
                    padding.Bottom);
#endif
                param.PageMargins = padding;
            }
            catch
            {

            }

            param.LabelWidth = (double)this.numericUpDown_labelWidth.UniverseValue;
            param.LabelHeight = (double)this.numericUpDown_labelHeight.UniverseValue;

            try
            {
                // 可能会抛出 ArgumentException 异常
                DecimalPadding padding = PaddingDialog.ParsePaddingString(this.textBox_labelPadding.UniverseText);

#if NO
                param.LabelPaddings = new System.Drawing.Printing.Margins(padding.Left,
                    padding.Right,
                    padding.Top,
                    padding.Bottom);
#endif
                param.LabelPaddings = padding;
            }
            catch
            {

            }

#if NO
            Font font = null;
            if (String.IsNullOrEmpty(this.textBox_labelFont.Text) == false)
            {
                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                font = (Font)converter.ConvertFromString(this.textBox_labelFont.Text);
            }
            else
            {
                font = Control.DefaultFont;
            }
#endif
                string strFontString = this.textBox_labelFont.Text;
                if (Global.IsVirtualBarcodeFont(ref strFontString) == true)
                    param.IsBarcodeFont = true;
                else
                    param.IsBarcodeFont = false;

                param.Font = Global.BuildFont(strFontString);


            param.LineFormats.Clear();
            foreach (ListViewItem item in this.listView_lineFormats.Items)
            {
                LineStore store = item.Tag as LineStore;
                Debug.Assert(store != null, "");

                LineFormat format = new LineFormat();

                strFontString = ListViewUtil.GetItemText(item, COLUMN_FONT);

                if (string.IsNullOrEmpty(strFontString) == false)
                {
                    if (Global.IsVirtualBarcodeFont(ref strFontString) == true)
                        format.IsBarcodeFont = true;
                    else
                        format.IsBarcodeFont = false;

                    format.Font = Global.BuildFont(strFontString);
                }
                else
                    format.Font = null;

                format.Align = ListViewUtil.GetItemText(item, COLUMN_ALIGN);
                SetFormatStart(format, store.UniversalStart);
                SetFormatOffset(format, store.UniversalOffset);

                format.ForeColor = ListViewUtil.GetItemText(item, COLUMN_FORECOLOR);
                format.BackColor = ListViewUtil.GetItemText(item, COLUMN_BACKCOLOR);

                param.LineFormats.Add(format);
            }

            param.LineSep = (double)this.numericUpDown_lineSep.UniverseValue;

            param.DefaultPrinter = this.textBox_printerInfo.Text;
            return param;
        }

        // 可能会抛出异常
        static void SetFormatOffset(LineFormat format, string strOffset)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strOffset,
                ",",
                out strLeft,
                out strRight);
            if (string.IsNullOrEmpty(strLeft) == true)
                format.OffsetX = 0;
            else
                format.OffsetX = double.Parse(strLeft);

            if (string.IsNullOrEmpty(strRight) == true)
                format.OffsetY = 0;
            else
                format.OffsetY = double.Parse(strRight);
        }

        // 可能会抛出异常
        static void SetFormatStart(LineFormat format, string strStart)
        {
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strStart,
                ",",
                out strLeft,
                out strRight);
            if (string.IsNullOrEmpty(strLeft) == true)
                format.StartX = double.NaN;
            else
                format.StartX = double.Parse(strLeft);

            if (string.IsNullOrEmpty(strRight) == true)
                format.StartY = double.NaN;
            else
                format.StartY = double.Parse(strRight);
        }

        class LineStore
        {
            public string UniversalStart = "";
            public string UniversalOffset = "";
        }

        void SetLabelParam(LabelParam param)
        {
            this.numericUpDown_pageWidth.UniverseValue = (decimal)param.PageWidth;
            this.numericUpDown_pageHeight.UniverseValue = (decimal)param.PageHeight;
            // this.checkBox_landscape.Checked = param.Landscape;
            this.rotateControl1.RotateDegree = param.RotateDegree;

            this.textBox_pagePadding.UniverseText = param.PageMargins.Left.ToString() + ","
        + param.PageMargins.Top.ToString() + ","
        + param.PageMargins.Right.ToString() + ","
        + param.PageMargins.Bottom.ToString();

            this.numericUpDown_labelWidth.UniverseValue = (decimal)param.LabelWidth;
            this.numericUpDown_labelHeight.UniverseValue = (decimal)param.LabelHeight;

            this.textBox_labelPadding.UniverseText = param.LabelPaddings.Left.ToString() + ","
                    + param.LabelPaddings.Top.ToString() + ","
                    + param.LabelPaddings.Right.ToString() + ","
                    + param.LabelPaddings.Bottom.ToString();

            if (param.IsBarcodeFont == true)
                this.textBox_labelFont.Text = Global.GetBarcodeFontString(param.Font);
            else
                this.textBox_labelFont.Text = FontUtil.GetFontString(param.Font);

            this.listView_lineFormats.Items.Clear();
            foreach (LineFormat line in param.LineFormats)
            {
                ListViewItem item = new ListViewItem();

                string strFontString = "";

                if (line.Font != null)
                {
                    if (line.IsBarcodeFont == true)
                        strFontString = Global.GetBarcodeFontString(line.Font);
                    else
                        strFontString = FontUtil.GetFontString(line.Font);
                }

                ListViewUtil.ChangeItemText(item, COLUMN_FONT, strFontString);
                ListViewUtil.ChangeItemText(item, COLUMN_ALIGN, line.Align);

                string strStart = GetStartString(this._currentUnit, line.StartX, line.StartY);
                ListViewUtil.ChangeItemText(item, COLUMN_START, strStart);
                string strOffset = GetOffsetString(this._currentUnit, line.OffsetX, line.OffsetY);
                ListViewUtil.ChangeItemText(item, COLUMN_OFFSET, strOffset);

                LineStore store = new LineStore();
                store.UniversalStart = GetStartString(GraphicsUnit.Display, line.StartX, line.StartY);
                store.UniversalOffset = GetOffsetString(GraphicsUnit.Display, line.OffsetX, line.OffsetY);
                item.Tag = store;

                ListViewUtil.ChangeItemText(item, COLUMN_FORECOLOR, line.ForeColor); 
                ListViewUtil.ChangeItemText(item, COLUMN_BACKCOLOR, line.BackColor);
                this.listView_lineFormats.Items.Add(item);
            }

            this.numericUpDown_lineSep.UniverseValue = (decimal)param.LineSep;

            this.textBox_printerInfo.Text = param.DefaultPrinter;
        }

        // 将 1/100 英寸的值转换为指定的单位的字符串值
        static string GetStartString(GraphicsUnit unit,
            double x,
            double y)
        {
            if (double.IsNaN(x) == true && double.IsNaN(y) == true)
                return "";

            if (unit == GraphicsUnit.Display)
                return LabelParam.ToString(x) + "," + LabelParam.ToString(y);

            if (double.IsNaN(x) == false)
                x = (double)UniverseNumericUpDown.ConvertValue(GraphicsUnit.Display, unit, (decimal)x);
            if (double.IsNaN(y) == false)
                y = (double)UniverseNumericUpDown.ConvertValue(GraphicsUnit.Display, unit, (decimal)y);

            return LabelParam.ToString(x) + "," + LabelParam.ToString(y);
        }

        // 将 1/100 英寸的值转换为指定的单位的字符串值
        static string GetOffsetString(GraphicsUnit unit,
            double x,
            double y)
        {
            if (x == 0 && y == 0)
                return "";

            if (unit == GraphicsUnit.Display)
                return LabelParam.ToString(x) + "," + LabelParam.ToString(y);

            x = (double)UniverseNumericUpDown.ConvertValue(GraphicsUnit.Display, unit, (decimal)x);
            y = (double)UniverseNumericUpDown.ConvertValue(GraphicsUnit.Display, unit, (decimal)y);

            return LabelParam.ToString(x) + "," + LabelParam.ToString(y);
        }

        const int COLUMN_FONT = 0;
        const int COLUMN_ALIGN = 1;
        const int COLUMN_START = 2;
        const int COLUMN_OFFSET = 3;
        const int COLUMN_FORECOLOR = 4;
        const int COLUMN_BACKCOLOR = 5;

        private void numericUpDown_labelWidth_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericUpDown_labelWidth.CurrentUnit != this._currentUnit)
                return;

            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        private void numericUpDown_labelHeight_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericUpDown_labelHeight.CurrentUnit != this._currentUnit)
                return;

            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        private void numericUpDown_pageWidth_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericUpDown_pageWidth.CurrentUnit != this._currentUnit)
                return;

            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        private void numericUpDown_pageHeight_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericUpDown_pageHeight.CurrentUnit != this._currentUnit)
                return;

            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        // 验证一个 padding 字符串是否正确，如果不正确，MessageBox() 提示
        // return:
        //      false   不正确
        //      true    正确
        bool ValidatePadding(string strText,
            string strWarningFormat)
        {
            string strError = "";
            if (PaddingDialog.ValidateValueString(strText, out strError) == -1)
            {
                MessageBox.Show(this, string.Format(strWarningFormat, strError));
                return false;
            }

            return true;

        }

        private void button_editPagePadding_Click(object sender, EventArgs e)
        {
            if (ValidatePadding(this.textBox_pagePadding.Text,
                "{0}\r\n\r\n请修改页面内容边距值为正确的格式，然后再重试操作") == false)
                return;

            PaddingDialog dlg = new PaddingDialog();

            dlg.DecimalPlaces = this.DecimalPlaces;
            dlg.Font = this.Font;
            dlg.CurrentUnit = this.CurrentUnit;
            // dlg.UniverseStringValue = this.textBox_pagePadding.UniverseText;
            dlg.StringValue = this.textBox_pagePadding.Text;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            // this.textBox_pagePadding.UniverseText = dlg.UniverseStringValue;
            this.textBox_pagePadding.Text = dlg.StringValue;
        }

        private void button_editLabelPadding_Click(object sender, EventArgs e)
        {
            if (ValidatePadding(this.textBox_labelPadding.Text,
    "{0}\r\n\r\n请修改标签内容边距值为正确的格式，然后再重试操作") == false)
                return;

            PaddingDialog dlg = new PaddingDialog();

            dlg.DecimalPlaces = this.DecimalPlaces;
            dlg.Font = this.Font;
            dlg.CurrentUnit = this.CurrentUnit;
            // dlg.UniverseStringValue = this.textBox_labelPadding.UniverseText;
            dlg.StringValue = this.textBox_labelPadding.Text;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            // this.textBox_labelPadding.UniverseText = dlg.UniverseStringValue;
            this.textBox_labelPadding.Text = dlg.StringValue;
        }

        private void textBox_pagePadding_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_pagePadding.CurrentUnit != this._currentUnit)
                return;

            try
            {
                // 可能会抛出 ArgumentException 异常
                DecimalPadding padding = PaddingDialog.ParsePaddingString(this.textBox_pagePadding.UniverseText);

                LabelParam param = this.LabelParam;

#if NO
                param.PageMargins = new System.Drawing.Printing.Margins(padding.Left,
                    padding.Right,
                    padding.Top,
                    padding.Bottom);
#endif
                param.PageMargins = padding;
                this.labelDesignControl1.Invalidate();
            }
            catch
            {
                
            }
            _panelVersion++;
            SetChanged();

        }

        private void textBox_labelPadding_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox_labelPadding.CurrentUnit != this._currentUnit)
                return;

            try
            {
                // 可能会抛出 ArgumentException 异常
                DecimalPadding padding = PaddingDialog.ParsePaddingString(this.textBox_labelPadding.UniverseText);

                LabelParam param = this.LabelParam;
#if NO
                param.LabelPaddings = new System.Drawing.Printing.Margins(padding.Left,
                    padding.Right,
                    padding.Top,
                    padding.Bottom);
#endif
                param.LabelPaddings = padding;
                this.labelDesignControl1.Invalidate();
            }
            catch
            {

            }
            _panelVersion++;
            SetChanged();
        }

        private void button_labelFont_Click(object sender, EventArgs e)
        {
            FontDialog dlg = new FontDialog();
            dlg.ShowColor = false;
            dlg.Font = Global.BuildFont(this.textBox_labelFont.Text);
            dlg.ShowApply = false;
            dlg.ShowHelp = true;
            dlg.AllowVerticalFonts = false;

            try
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                return;
            }

            this.textBox_labelFont.Text = FontUtil.GetFontString(dlg.Font);
        }

        private void textBox_labelFont_TextChanged(object sender, EventArgs e)
        {
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();

            // 如果是虚拟条码字体，则禁止选择字体的按钮
            string strFontString = this.textBox_labelFont.Text;
            if (Global.IsVirtualBarcodeFont(ref strFontString) == true)
                this.button_labelFont.Enabled = false;
            else
                this.button_labelFont.Enabled = true;
        }

        private void listView_lineFormats_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改 (&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyFormat_Click);
            if (this.listView_lineFormats.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建 (&C)");
            menuItem.Click += new System.EventHandler(this.menu_newFormat_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("上移 (&U)");
            menuItem.Click += new System.EventHandler(this.menu_moveUpFormat_Click);
            if (ListViewUtil.MoveItemEnabled(this.listView_lineFormats, true) == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("下移 (&U)");
            menuItem.Click += new System.EventHandler(this.menu_moveDownFormat_Click);
            if (ListViewUtil.MoveItemEnabled(this.listView_lineFormats, false) == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除 [" + this.listView_lineFormats.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_deleteFormat_Click);
            if (this.listView_lineFormats.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_lineFormats, new Point(e.X, e.Y));		

        }

        // 因为数量单位变化，刷新 lineFormat list 的显示
        void RefreshListLineFormatsUnits()
        {
            foreach (ListViewItem item in this.listView_lineFormats.Items)
            {
                LineStore store = item.Tag as LineStore;
                Debug.Assert(store != null, "");

                if (this._currentUnit == GraphicsUnit.Display)
                {
                    ListViewUtil.ChangeItemText(item, COLUMN_START, store.UniversalStart);
                    ListViewUtil.ChangeItemText(item, COLUMN_OFFSET, store.UniversalOffset);
                }
                else
                {
                    LineFormat format = new LineFormat();
                    SetFormatStart(format, store.UniversalStart);
                    SetFormatOffset(format, store.UniversalOffset);

                    string strStart = GetStartString(this._currentUnit, format.StartX, format.StartY);
                    ListViewUtil.ChangeItemText(item, COLUMN_START, strStart);
                    string strOffset = GetOffsetString(this._currentUnit, format.OffsetX, format.OffsetY);
                    ListViewUtil.ChangeItemText(item, COLUMN_OFFSET, strOffset);
                }
            }
        }

        void menu_modifyFormat_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView_lineFormats.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_lineFormats.SelectedItems[0];
            LineStore store = item.Tag as LineStore;
            Debug.Assert(store != null, "");

            LabelLineFormatDialog dlg = new LabelLineFormatDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.CurrentUnit = this._currentUnit;
            dlg.DecimalPlaces = this.DecimalPlaces;
            dlg.FontString = ListViewUtil.GetItemText(item, COLUMN_FONT);
            dlg.Align = ListViewUtil.GetItemText(item, COLUMN_ALIGN);
#if NO
            dlg.Start = ListViewUtil.GetItemText(item, COLUMN_START);
            dlg.Offset = ListViewUtil.GetItemText(item, COLUMN_OFFSET);
#endif
            dlg.UniversalStart = store.UniversalStart;
            dlg.UniversalOffset = store.UniversalOffset;
            dlg.ForeColorString = ListViewUtil.GetItemText(item, COLUMN_FORECOLOR);
            dlg.BackColorString = ListViewUtil.GetItemText(item, COLUMN_BACKCOLOR);

            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);


            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewUtil.ChangeItemText(item, COLUMN_FONT, dlg.FontString);
            ListViewUtil.ChangeItemText(item, COLUMN_ALIGN, dlg.Align);
            ListViewUtil.ChangeItemText(item, COLUMN_START, dlg.Start); 
            ListViewUtil.ChangeItemText(item, COLUMN_OFFSET, dlg.Offset);

            store.UniversalStart = dlg.UniversalStart;
            store.UniversalOffset = dlg.UniversalOffset;

            ListViewUtil.ChangeItemText(item, COLUMN_FORECOLOR, dlg.ForeColorString);
            ListViewUtil.ChangeItemText(item, COLUMN_BACKCOLOR, dlg.BackColorString);


            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_newFormat_Click(object sender, EventArgs e)
        {
            LabelLineFormatDialog dlg = new LabelLineFormatDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.CurrentUnit = this._currentUnit;
            dlg.DecimalPlaces = this.DecimalPlaces;
            dlg.Align = "left";
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, COLUMN_FONT, dlg.FontString);
            ListViewUtil.ChangeItemText(item, COLUMN_ALIGN, dlg.Align);
            ListViewUtil.ChangeItemText(item, COLUMN_START, dlg.Start);
            ListViewUtil.ChangeItemText(item, COLUMN_OFFSET, dlg.Offset);

            LineStore store = new LineStore();
            store.UniversalStart = dlg.UniversalStart;
            store.UniversalOffset = dlg.UniversalOffset;
            item.Tag = store;

            ListViewUtil.ChangeItemText(item, COLUMN_FORECOLOR, dlg.ForeColorString);
            ListViewUtil.ChangeItemText(item, COLUMN_BACKCOLOR, dlg.BackColorString);

            this.listView_lineFormats.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        void menu_moveUpFormat_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<int> indices = null;
            int nRet = ListViewUtil.MoveItemUpDown(this.listView_lineFormats,
                true,
                out indices,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        void menu_moveDownFormat_Click(object sender, EventArgs e)
        {
            string strError = "";
            List<int> indices = null;
            int nRet = ListViewUtil.MoveItemUpDown(this.listView_lineFormats,
                false,
                out indices,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        void menu_deleteFormat_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            if (this.listView_lineFormats.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            // TODO: 是否对话框询问?

            ListViewUtil.DeleteSelectedItems(this.listView_lineFormats);

            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_lineFormats_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyFormat_Click(sender, e);
        }

        private void numericUpDown_lineSep_ValueChanged(object sender, EventArgs e)
        {
            if (this.numericUpDown_lineSep.CurrentUnit != this._currentUnit)
                return;

            LabelParam param = this.LabelParam;
            param.LineSep = (int)this.numericUpDown_lineSep.Value;
            this.labelDesignControl1.Invalidate();

            _panelVersion++;
            SetChanged();
        }

        int _panelVersion = 0;  // 面板中的数据版本号
        int _xmlVersion = 0;    // XML 中的数据版本号

        private void textBox_xml_TextChanged(object sender, EventArgs e)
        {
            _xmlVersion++;
            SetChanged();
        }

        void SetChanged()
        {
            this._changed = true;
        }

        // 同步面板和 XML 数据
        public void Synchronize()
        {
            string strError = "";
            int nRet = 0;

            if (_xmlVersion < _panelVersion)
            {
                // Panel --> XML
                XmlDocument dom = null;
                nRet = this.LabelParam.ToXmlDocument(out dom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_xml.Text = DomUtil.GetIndentXml(dom);  // dom.OuterXml;

               _xmlVersion = 0;
               _panelVersion = 0;
            }
            else if (_xmlVersion > _panelVersion)
            {
                // XML --> Panel
                XmlDocument dom = new XmlDocument();
                try
                {
                    if (string.IsNullOrEmpty(this.textBox_xml.Text) == false)
                        dom.LoadXml(this.textBox_xml.Text);
                    else
                        dom.LoadXml("<root />");
                }
                catch (Exception ex)
                {
                    strError = "XML 装入 DOM 时出错: " + ex.Message;
                    goto ERROR1;
                }

                LabelParam label_param = null;
                nRet = LabelParam.Build(dom,
                    out label_param,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 放入面板
                SetLabelParam(label_param);

#if NO
                this.PrinterInfo = new PrinterInfo("", this.textBox_printerInfo.Text);
                GetPrintDocument(false);    // 让打印机参数影响 document
#endif
                string strWarning = "";
                FlushDcoumentPrinterInfo(out strWarning);
                this.SetPrinterInfoWarning(strWarning);


                this.LabelParam = label_param;

                _xmlVersion = 0;
                _panelVersion = 0;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void SetPrinterInfoWarning(string strWarning)
        {
            this.label_printInfoWarning.Text = strWarning;
            if (string.IsNullOrEmpty(strWarning) == true)
                this.label_printInfoWarning.Visible = false;
            else
                this.label_printInfoWarning.Visible = true;
        }


        // 将 textBox_printerInfo 的打印机特性兑现到 PrintDocument 中
        bool FlushDcoumentPrinterInfo(out string strWarning)
        {
            strWarning = "";

            PrintDocument document = this.labelDesignControl1.PrintDocument;
            if (document == null)
                return false;

            bool bChanged = false;
            PrinterInfo printerInfo = new PrinterInfo("", this.textBox_printerInfo.Text);


#if NO
                string strPrinterName = document.PrinterSettings.PrinterName;
                if (string.IsNullOrEmpty(printerInfo.PrinterName) == false
                    && printerInfo.PrinterName != strPrinterName)
                {
                    string strOldName = document.PrinterSettings.PrinterName;
                    document.PrinterSettings.PrinterName = printerInfo.PrinterName;
                    if (document.PrinterSettings.IsValid == false)
                    {
                        document.PrinterSettings.PrinterName = strOldName;
                        // TODO: 可以在 textbox 边上出现一个感叹号
                    }
                }
#endif
            string strError = "";
            int nRet = 0;
            string strPrinterName = document.PrinterSettings.PrinterName;
            if (string.IsNullOrEmpty(printerInfo.PrinterName) == false
                && printerInfo.PrinterName != strPrinterName)
            {
                // 按照存储的打印机名选定打印机
                // <returns>0: 成功选定: 1: 没有选定，因为名字不可用。建议后面出现打印机对话框选定</returns>
                nRet = LabelPrintForm.SelectPrinterByName(document,
                    printerInfo.PrinterName,
                    out  strError);
                if (nRet == 1)
                {
                    strWarning = "打印机 " + printerInfo.PrinterName + " 当前不可用，请重新选定打印机";
                    printerInfo.PrinterName = "";
                    // bDisplayPrinterDialog = true;
                    // TODO: 可以在 textbox 边上出现一个感叹号
                }
                bChanged = true;
            }

#if NO
                PaperSize old_papersize = document.DefaultPageSettings.PaperSize;
                if (string.IsNullOrEmpty(printerInfo.PaperName) == false
                    && printerInfo.PaperName != document.DefaultPageSettings.PaperSize.PaperName)
                {
                    PaperSize found = null;
                    foreach (PaperSize ps in document.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.Equals(printerInfo.PaperName))
                        {
                            found = ps;
                            break;
                        }
                    }

                    if (found != null)
                        document.DefaultPageSettings.PaperSize = found;
                    else
                    {
                        // TODO: 可以在 textbox 边上出现一个感叹号
                    }
                }
#endif
            LabelParam label_param = this.LabelParam;
            // 需要自定义纸张
            if ((string.IsNullOrEmpty(label_param.DefaultPrinter) == true || string.IsNullOrEmpty(printerInfo.PaperName) == true)
                && label_param.PageWidth > 0
                && label_param.PageHeight > 0)
            {
                // bCustomPaper = true;

                PaperSize paper_size = new PaperSize("Custom Label",
                    (int)label_param.PageWidth,
                    (int)label_param.PageHeight);
                document.DefaultPageSettings.PaperSize = paper_size;
                bChanged = true;
            }
            else
            {
                if (string.IsNullOrEmpty(printerInfo.PaperName) == false)
                {
                    nRet = LabelPrintForm.SelectPaperByName(document,
                        printerInfo.PaperName,
                        printerInfo.Landscape,
                        true,   // false,
                        out strError);
                    if (nRet == 1)
                    {
                        if (string.IsNullOrEmpty(strWarning) == false)
                            strWarning += "; ";
                        else
                        {
                            if (string.IsNullOrEmpty(printerInfo.PrinterName) == false)
                                strWarning += "打印机 " + printerInfo.PrinterName + " 的";
                        }
                        strWarning += "纸张 " + printerInfo.PaperName + " 当前不可用，请重新选定纸张";
                        printerInfo.PaperName = "";
                        // bDisplayPrinterDialog = true;
                        // TODO: 可以在 textbox 边上出现一个感叹号
                    }
                    bChanged = true;
                }
            }

            // document.DefaultPageSettings.Landscape = label_param.Landscape;

            return bChanged;
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Synchronize();
        }

        public string Xml
        {
            get
            {
                return this.textBox_xml.Text;
            }
            set
            {
                this.textBox_xml.Text = value;
            }
        }

        bool _changed = false;

        public bool Changed
        {
            get
            {
                return this._changed;
            }
            set
            {
                this._changed = value;
            }
        }

        /// <summary>
        /// 样例标签文本
        /// </summary>
        public string SampleLabelText
        {
            get
            {
                return this.textBox_sampleText.Text;
            }
            set
            {
                this.textBox_sampleText.Text = value;
            }
        }

        private void button_setBarcodeFont_Click(object sender, EventArgs e)
        {
            string strFontString = this.textBox_labelFont.Text;

            if (Global.IsVirtualBarcodeFont(ref strFontString) == true)
                return; // 已经是条码字体了

            string strFontName = "";
            string strOther = "";
            StringUtil.ParseTwoPart(strFontString,
                ",",
                out strFontName,
                out strOther);

            this.textBox_labelFont.Text = "barcode," + strOther;
        }

#if NO
        PrinterInfo m_printerInfo = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PrinterInfo PrinterInfo
        {
            get
            {
                return this.m_printerInfo;
            }
            set
            {
                this.m_printerInfo = value;
                // SetTitle();
            }
        }
#endif

        private void button_selectPrinter_Click(object sender, EventArgs e)
        {
#if NO
            PrintDialog printDialog1 = new PrintDialog();
            printDialog1.Document = new System.Drawing.Printing.PrintDocument();

            DialogResult result = printDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                // 记忆打印参数

                // 记忆打印参数
                if (this.PrinterInfo == null)
                    this.PrinterInfo = new PrinterInfo();
                this.PrinterInfo.PrinterName = printDialog1.Document.PrinterSettings.PrinterName;
                this.PrinterInfo.PaperName = printDialog1.Document.DefaultPageSettings.PaperSize.PaperName;

                DisplayPrinterInfo();
            }
#endif
            bool bControl = (Control.ModifierKeys & Keys.Control) != 0;

            PrintDocument doc = SelectPrinterAndPaper(true);

            // 把页面宽度和高度强行设定为和当前纸张一致
            if (doc != null && bControl == false)
            {
                if (doc.DefaultPageSettings.Landscape == false)
                {
                    this.numericUpDown_pageWidth.UniverseValue = doc.DefaultPageSettings.PaperSize.Width;
                    this.numericUpDown_pageHeight.UniverseValue = doc.DefaultPageSettings.PaperSize.Height;
                }
                else
                {
                    this.numericUpDown_pageWidth.UniverseValue = doc.DefaultPageSettings.PaperSize.Height;
                    this.numericUpDown_pageHeight.UniverseValue = doc.DefaultPageSettings.PaperSize.Width;
                }
                // this.checkBox_landscape.Checked = doc.DefaultPageSettings.Landscape;
            }

            this.SetPrinterInfoWarning("");
        }

        private void textBox_printerInfo_TextChanged(object sender, EventArgs e)
        {
#if NO
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
#endif
        }

#if NO
        void DisplayPrinterInfo()
        {
            if (this.PrinterInfo != null)
                this.textBox_printerInfo.Text = this.PrinterInfo.GetText();
            else
                this.textBox_printerInfo.Text = "";
        }
#endif

        // 选择打印机和纸张
        // parameters:
        //      bForceDialog    是否迫使出现打印机对话框
        // return:
        //      null    选择打印机的对话框被取消
        //      其他  返回 PrintDocument 对象
        PrintDocument SelectPrinterAndPaper(bool bForceDialog)
        {
            string strError = "";
            int nRet = 0;

            PrintDialog printDialog1 = new PrintDialog();

            bool bDisplayPrinterDialog = false;
            PrintDocument document = this.labelDesignControl1.PrintDocument;
                
                // new System.Drawing.Printing.PrintDocument();

            printDialog1.Document = document;

            PrinterInfo printerInfo = new PrinterInfo("", this.textBox_printerInfo.Text);

            if (printerInfo != null)
            {
                if (string.IsNullOrEmpty(printerInfo.PrinterName) == false
                    && string.IsNullOrEmpty(printerInfo.PaperName) == false)
                {
                    // 按照存储的打印机名选定打印机
                    nRet = LabelPrintForm.SelectPrinterByName(document,
                        printerInfo.PrinterName,
                        out  strError);
                    if (nRet == 1)
                    {
                        MessageBox.Show(this, "打印机 " + printerInfo.PrinterName + " 当前不可用，请重新选定打印机");
                        printerInfo.PrinterName = "";
                        bDisplayPrinterDialog = true;
                    }

                    if (bDisplayPrinterDialog == false
                        && string.IsNullOrEmpty(printerInfo.PaperName) == false)
                    {
                        nRet = LabelPrintForm.SelectPaperByName(document,
                            printerInfo.PaperName,
                            printerInfo.Landscape,
                            true,   // false,
                            out strError);
                        if (nRet == 1)
                        {
                            MessageBox.Show(this, "打印机 " + printerInfo.PrinterName + " 的纸张类型 " + printerInfo.PaperName + " 当前不可用，请重新选定纸张");
                            printerInfo.PaperName = "";
                            bDisplayPrinterDialog = true;
                        }
                    }

                    // 只要有一个打印机事项没有确定，就要出现打印机对话框
                    if (string.IsNullOrEmpty(printerInfo.PrinterName) == true
                        || string.IsNullOrEmpty(printerInfo.PaperName) == true)
                        bDisplayPrinterDialog = true;
                }
            }
            else
            {
                // 没有首选配置的情况下要出现打印对话框
                bDisplayPrinterDialog = true;
            }

#if NO
            // 如果一开始没有打印机信息 textbox 内容，则依 label_param 中的方向
            if (string.IsNullOrEmpty(this.textBox_printerInfo.Text) == true)
                document.DefaultPageSettings.Landscape = this.LabelParam.Landscape;
#endif

            DialogResult result = DialogResult.OK;
            if (bDisplayPrinterDialog == true
                || bForceDialog == true)
            {
                result = printDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    // 记忆打印参数
                    if (printerInfo == null)
                        printerInfo = new PrinterInfo();
                    printerInfo.PrinterName = document.PrinterSettings.PrinterName;
                    printerInfo.PaperName = PrintUtil.GetPaperSizeString(document.DefaultPageSettings.PaperSize);
                    printerInfo.Landscape = document.DefaultPageSettings.Landscape;

                    // 2014/3/27
                    // this.document.DefaultPageSettings = document.PrinterSettings.DefaultPageSettings;
                    nRet = LabelPrintForm.SelectPaperByName(document,
printerInfo.PaperName,
printerInfo.Landscape,
true,
out strError);
                    if (nRet == 1)
                    {
                        SelectPaperDialog paper_dialog = new SelectPaperDialog();
                        MainForm.SetControlFont(paper_dialog, this.Font, false);
                        paper_dialog.Comment = "纸张 " + printerInfo.PaperName + " 不在打印机 " + printerInfo.PrinterName + " 的可用纸张列表中。\r\n请重新选定纸张";
                        paper_dialog.Document = document;
                        paper_dialog.StartPosition = FormStartPosition.CenterParent;
                        paper_dialog.ShowDialog(this);

                        if (paper_dialog.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                            return document;    // null

                    }

                    printerInfo.PaperName = PrintUtil.GetPaperSizeString(document.DefaultPageSettings.PaperSize);
                    printerInfo.Landscape = document.DefaultPageSettings.Landscape;

                    this.textBox_printerInfo.Text = printerInfo.GetText();

                    // document.DefaultPageSettings = document.PrinterSettings.DefaultPageSettings;
                }
                else
                    return null;
            }

            return document;
        }

        private void radioButton_byInput_CheckedChanged(object sender, EventArgs e)
        {
            RadioChanged();
        }

        private void radioButton_byPaperSize_CheckedChanged(object sender, EventArgs e)
        {
            RadioChanged();
        }

        void RadioChanged()
        {
            if (this.radioButton_byInput.Checked == true)
            {
                this.numericUpDown_pageWidth.Enabled = true;
                this.numericUpDown_pageHeight.Enabled = true;
                this.checkBox_landscape.Enabled = true;

                this.button_selectPrinter.Enabled = false;
                this.textBox_printerInfo.Enabled = false;
            }
            else
            {
                this.numericUpDown_pageWidth.Enabled = false;
                this.numericUpDown_pageHeight.Enabled = false;
                this.checkBox_landscape.Enabled = false;

                this.button_selectPrinter.Enabled = true;
                this.textBox_printerInfo.Enabled = true;
            }
        }

        int _inIndexChange = 0;

        private void comboBox_currentUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            _inIndexChange++;
            try
            {
                if (_inIndexChange == 1)
                {

                    if (this.comboBox_currentUnit.Text == "1/100 英寸")
                        this.CurrentUnit = GraphicsUnit.Display;
                    else if (this.comboBox_currentUnit.Text == "毫米")
                        this.CurrentUnit = GraphicsUnit.Millimeter;
                    else
                        throw new Exception("暂不支持 " + this.comboBox_currentUnit.Text);
                }
            }
            finally
            {
                _inIndexChange--;
            }

        }


        private void textBox_sampleText_DelayTextChanged(object sender, EventArgs e)
        {
            // 将样本内容兑现到 labelDesignControl1 显示
            Stream stream = new MemoryStream(Encoding.Default.GetBytes(this.textBox_sampleText.Text));
            StreamReader sr = new StreamReader(stream, Encoding.Default);

            string strError = "";
            this.labelDesignControl1.SetLabelFile(sr, out strError);
            this.labelDesignControl1.Invalidate();
        }

        private void numericUpDown_sampleText_linesPerLabel_ValueChanged(object sender, EventArgs e)
        {
            this.SampleLabelText = LabelDefControl.BuildSampleLabelText((int)this.numericUpDown_sampleText_linesPerLabel.Value);
        }

        private void textBox_xml_Validating(object sender, CancelEventArgs e)
        {
            // 验证 XML 是否合法
            if (string.IsNullOrEmpty(this.textBox_xml.Text) == false)
            {
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(this.textBox_xml.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "XML 代码格式错误: " + ex.Message);
                    e.Cancel = true;
                    return;
                }
            }

            this.Synchronize();
        }

        private void numericUpDown_decimalPlaces_ValueChanged(object sender, EventArgs e)
        {
            int value = (int)this.numericUpDown_decimalPlaces.Value;

            this.numericUpDown_labelWidth.DecimalPlaces = value;
            this.numericUpDown_labelHeight.DecimalPlaces = value;

            this.numericUpDown_pageWidth.DecimalPlaces = value;
            this.numericUpDown_pageHeight.DecimalPlaces = value;

            this.numericUpDown_lineSep.DecimalPlaces = value;
        }

        private void textBox_printerInfo_SizeChanged(object sender, EventArgs e)
        {
            // 让 label 跟着 textbox 宽度变化
            this.label_printInfoWarning.MaximumSize = new Size(this.textBox_printerInfo.Size.Width, 0);
        }

        private void textBox_printerInfo_DelayTextChanged(object sender, EventArgs e)
        {
            string strWarning = "";
#if NO
            if (FlushDcoumentPrinterInfo(out strWarning) == true)
                this.labelDesignControl1.Invalidate();
#endif
            FlushDcoumentPrinterInfo(out strWarning);

            this.SetPrinterInfoWarning(strWarning);

            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        private void checkBox_landscape_CheckedChanged(object sender, EventArgs e)
        {
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        private void tabPage_page_SizeChanged(object sender, EventArgs e)
        {
            this.textBox_printerInfo.Size = new Size(
                this.tabPage_page.ClientSize.Width - this.textBox_printerInfo.Location.X - 10,
                this.textBox_printerInfo.Height);
        }

        private void rotateControl1_OrentationChanged(object sender, EventArgs e)
        {
            this.labelDesignControl1.LabelParam = this.GetLabelParam();
            _panelVersion++;
            SetChanged();
        }

        private void checkBox_gridLine_CheckedChanged(object sender, EventArgs e)
        {
            string strStyle = this.labelDesignControl1.PrintStyle;
            StringUtil.SetInList(ref strStyle, "TestingGrid", this.checkBox_gridLine.Checked);
            this.labelDesignControl1.PrintStyle = strStyle;
            this.labelDesignControl1.Invalidate();
        }

        public bool GridLine
        {
            get
            {
                return this.checkBox_gridLine.Checked;
            }
            set
            {
                this.checkBox_gridLine.Checked = value;
            }
        }
    }
}
