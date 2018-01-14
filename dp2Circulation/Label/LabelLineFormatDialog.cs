using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Drawing;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 编辑一个标签行格式的对话框
    /// </summary>
    public partial class LabelLineFormatDialog : Form
    {
        public LabelLineFormatDialog()
        {
            InitializeComponent();
        }

        GraphicsUnit _currentUnit = GraphicsUnit.Display;

        // 当前度量单位
        public GraphicsUnit CurrentUnit
        {
            get
            {
                return this._currentUnit;
            }
            set
            {
                this.numericUpDown_offsetX.CurrentUnit = value;
                this.numericUpDown_offsetY.CurrentUnit = value;

                this._currentUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to display in the spin box (also known as an up-down control).
        /// </summary>
        public int DecimalPlaces
        {
            get
            {
                return this.numericUpDown_offsetX.DecimalPlaces;
            }
            set
            {
                this.numericUpDown_offsetX.DecimalPlaces = value;
                this.numericUpDown_offsetY.DecimalPlaces = value;
            }
        }

        // Offset 字符串，当前度量单位
        public string Offset
        {
            get
            {
                if (this.numericUpDown_offsetX.Value == 0
                    && this.numericUpDown_offsetY.Value == 0)
                    return "";

                return this.numericUpDown_offsetX.Value.ToString() + "," + this.numericUpDown_offsetY.Value.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    this.numericUpDown_offsetX.Value = (decimal)0;
                    this.numericUpDown_offsetY.Value = (decimal)0;
                    return;
                }

                double left = 0;
                double right = 0;
                LabelParam.ParsetTwoDouble(value,
                    false,
                    out left,
                    out right);
                this.numericUpDown_offsetX.Value = (decimal)left;
                this.numericUpDown_offsetY.Value = (decimal)right;
            }
        }

        // Offset 字符串，1/100 英寸度量单位
        public string UniversalOffset
        {
            get
            {
                if (this.numericUpDown_offsetX.Value == 0
                    && this.numericUpDown_offsetY.Value == 0)
                    return "";

                return this.numericUpDown_offsetX.UniverseValue.ToString()
    + ","
    + this.numericUpDown_offsetY.UniverseValue.ToString();
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    this.numericUpDown_offsetX.Value = (decimal)0;
                    this.numericUpDown_offsetY.Value = (decimal)0;
                    return;
                }

                double left = 0;
                double right = 0;
                LabelParam.ParsetTwoDouble(value,
                    false,
                    out left,
                    out right);

#if NO
                this.numericUpDown_offsetX.Value = UniverseNumericUpDown.ConvertValue(
                    GraphicsUnit.Display,
                    this._currentUnit,
                    (decimal)left);
                this.numericUpDown_offsetY.Value = UniverseNumericUpDown.ConvertValue(
                    GraphicsUnit.Display,
                    this._currentUnit,
                    (decimal)right);
#endif
                this.numericUpDown_offsetX.UniverseValue = (decimal)left;
                this.numericUpDown_offsetY.UniverseValue = (decimal)right;
            }
        }

        // Start 字符串，当前度量单位
        public string Start
        {
            get
            {
                if (string.IsNullOrEmpty(this.textBox_startX.Text) == true
                    && string.IsNullOrEmpty(this.textBox_startY.Text) == true)
                    return "";

                return this.textBox_startX.Text + "," + this.textBox_startY.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    this.textBox_startX.Text = "";
                    this.textBox_startY.Text = "";
                    return;
                }

                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(value,
                    ",",
                    out strLeft,
                    out strRight);

                this.textBox_startX.Text = strLeft;
                this.textBox_startY.Text = strRight;
            }
        }

        // Size 字符串，当前度量单位
        public string SizeString
        {
            get
            {
                if (string.IsNullOrEmpty(this.textBox_width.Text) == true
                    && string.IsNullOrEmpty(this.textBox_height.Text) == true)
                    return "";

                return this.textBox_width.Text + "," + this.textBox_height.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    this.textBox_width.Text = "";
                    this.textBox_height.Text = "";
                    return;
                }

                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(value,
                    ",",
                    out strLeft,
                    out strRight);

                this.textBox_width.Text = strLeft;
                this.textBox_height.Text = strRight;
            }
        }

        static string ToString(double v)
        {
            if (double.IsNaN(v) == true)
                return "";
            return v.ToString();
        }

        // Start 字符串，1/100 英寸度量单位
        public string UniversalStart
        {
            get
            {
                if (string.IsNullOrEmpty(this.textBox_startX.Text) == true
                    && string.IsNullOrEmpty(this.textBox_startY.Text) == true)
                    return "";

                if (this._currentUnit == GraphicsUnit.Display)
                    return this.textBox_startX.Text + "," + this.textBox_startY.Text;

                double left = double.NaN;
                double right = double.NaN;

                if (string.IsNullOrEmpty(this.textBox_startX.Text) == false)
                    left = double.Parse(this.textBox_startX.Text);
                if (string.IsNullOrEmpty(this.textBox_startY.Text) == false)
                    right = double.Parse(this.textBox_startY.Text);

                if (double.IsNaN(left) == false)
                    left = (double)UniverseNumericUpDown.ConvertValue(this._currentUnit, GraphicsUnit.Display,
                     (decimal)left);
                if (double.IsNaN(right) == false)
                    right = (double)UniverseNumericUpDown.ConvertValue(this._currentUnit, GraphicsUnit.Display,
                     (decimal)right);
                return ToString(left) + "," + ToString(right);
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    this.textBox_startX.Text = "";
                    this.textBox_startY.Text = "";
                    return;
                }

                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(value,
                    ",",
                    out strLeft,
                    out strRight);

                if (this._currentUnit == GraphicsUnit.Display)
                {
                    this.textBox_startX.Text = strLeft;
                    this.textBox_startY.Text = strRight;
                    return;
                }

                double left = double.NaN;
                double right = double.NaN;

                if (string.IsNullOrEmpty(strLeft) == false)
                    left = double.Parse(strLeft);
                if (string.IsNullOrEmpty(strRight) == false)
                    right = double.Parse(strRight);

                if (double.IsNaN(left) == false)
                {
                    left = (double)UniverseNumericUpDown.ConvertValue(
                        GraphicsUnit.Display,
                        this._currentUnit,
                        (decimal)left);
                    this.textBox_startX.Text = left.ToString();
                }
                else 
                    this.textBox_startX.Text = "";

                if (double.IsNaN(right) == false)
                {
                    right = (double)UniverseNumericUpDown.ConvertValue(
                        GraphicsUnit.Display,
                        this._currentUnit,
                        (decimal)right);
                    this.textBox_startY.Text = right.ToString();
                }
                else
                    this.textBox_startY.Text = "";

            }
        }

        // Size 字符串，1/100 英寸度量单位
        public string UniversalSize
        {
            get
            {
                if (string.IsNullOrEmpty(this.textBox_width.Text) == true
                    && string.IsNullOrEmpty(this.textBox_height.Text) == true)
                    return "";

                if (this._currentUnit == GraphicsUnit.Display)
                    return this.textBox_width.Text + "," + this.textBox_height.Text;

                double left = double.NaN;
                double right = double.NaN;

                if (string.IsNullOrEmpty(this.textBox_width.Text) == false)
                    left = double.Parse(this.textBox_width.Text);
                if (string.IsNullOrEmpty(this.textBox_height.Text) == false)
                    right = double.Parse(this.textBox_height.Text);

                if (double.IsNaN(left) == false)
                    left = (double)UniverseNumericUpDown.ConvertValue(this._currentUnit, GraphicsUnit.Display,
                     (decimal)left);
                if (double.IsNaN(right) == false)
                    right = (double)UniverseNumericUpDown.ConvertValue(this._currentUnit, GraphicsUnit.Display,
                     (decimal)right);
                return ToString(left) + "," + ToString(right);
            }
            set
            {
                if (string.IsNullOrEmpty(value) == true)
                {
                    this.textBox_width.Text = "";
                    this.textBox_height.Text = "";
                    return;
                }

                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(value,
                    ",",
                    out strLeft,
                    out strRight);

                if (this._currentUnit == GraphicsUnit.Display)
                {
                    this.textBox_width.Text = strLeft;
                    this.textBox_height.Text = strRight;
                    return;
                }

                double left = double.NaN;
                double right = double.NaN;

                if (string.IsNullOrEmpty(strLeft) == false)
                    left = double.Parse(strLeft);
                if (string.IsNullOrEmpty(strRight) == false)
                    right = double.Parse(strRight);

                if (double.IsNaN(left) == false)
                {
                    left = (double)UniverseNumericUpDown.ConvertValue(
                        GraphicsUnit.Display,
                        this._currentUnit,
                        (decimal)left);
                    this.textBox_width.Text = left.ToString();
                }
                else
                    this.textBox_width.Text = "";

                if (double.IsNaN(right) == false)
                {
                    right = (double)UniverseNumericUpDown.ConvertValue(
                        GraphicsUnit.Display,
                        this._currentUnit,
                        (decimal)right);
                    this.textBox_height.Text = right.ToString();
                }
                else
                    this.textBox_height.Text = "";
            }
        }

        // 当前度量单位
        public double OffsetX
        {
            get
            {
                return (double)this.numericUpDown_offsetX.Value;
            }
            set
            {
                this.numericUpDown_offsetX.Value = (decimal)value;
            }
        }

        // 当前度量单位
        public double OffsetY
        {
            get
            {
                return (double)this.numericUpDown_offsetY.Value;
            }
            set
            {
                this.numericUpDown_offsetY.Value = (decimal)value;
            }
        }

        // 1/100 英寸
        public double UniversalStartX
        {
            get
            {
                if (string.IsNullOrEmpty(this.textBox_startX.Text) == true)
                    return double.NaN;

                return (double)UniverseNumericUpDown.ConvertValue(this._currentUnit, GraphicsUnit.Display,
                    (decimal)double.Parse(this.textBox_startX.Text));
            }
            set
            {
                if (double.IsNaN(value) == true)
                {
                    this.textBox_startX.Text = "";
                    return;
                }

                this.textBox_startX.Text = UniverseNumericUpDown.ConvertValue(
                    GraphicsUnit.Display,
                    this._currentUnit,
                    (decimal)value).ToString() ;
            }
        }

        public double StartY
        {
            get
            {
                if (string.IsNullOrEmpty(this.textBox_startY.Text) == true)
                    return double.NaN;
                return double.Parse(this.textBox_startY.Text);
            }
            set
            {
                if (double.IsNaN(value) == true)
                {
                    this.textBox_startY.Text = "";
                    return;
                }

                this.textBox_startY.Text = value.ToString();
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string FontString
        {
            get
            {
                return this.textBox_fontString.Text;
            }
            set
            {
                this.textBox_fontString.Text = value;
            }
        }

        public string Align
        {
            get
            {
                return this.comboBox_align.Text;
            }
            set
            {
                this.comboBox_align.Text = value;
            }
        }

        public string ForeColorString
        {
            get
            {
                return this.textBox_foreColor.Text;
            }
            set
            {
                this.textBox_foreColor.Text = value;
            }
        }

        public string BackColorString
        {
            get
            {
                return this.textBox_backColor.Text;
            }
            set
            {
                this.textBox_backColor.Text = value;
            }
        }

        public string StyleString
        {
            get
            {
                return this.textBox_styleString.Text;
            }
            set
            {
                this.textBox_styleString.Text = value;
            }
        }

        private void button_getFont_Click(object sender, EventArgs e)
        {
            FontDialog dlg = new FontDialog();
            dlg.ShowColor = false;
            dlg.Font = Global.BuildFont(this.textBox_fontString.Text);
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

            this.textBox_fontString.Text = FontUtil.GetFontString(dlg.Font);
        }

        private void textBox_fontString_TextChanged(object sender, EventArgs e)
        {
            // 如果是虚拟条码字体，则禁止选择字体的按钮
            string strFontString = this.textBox_fontString.Text;
            if (Global.IsVirtualBarcodeFont(ref strFontString) == true)
                this.button_getFont.Enabled = false;
            else
                this.button_getFont.Enabled = true;
        }

        private void button_setBarcodeFont_Click(object sender, EventArgs e)
        {
            string strFontString = this.textBox_fontString.Text;

            if (Global.IsVirtualBarcodeFont(ref strFontString) == true)
                return; // 已经是条码字体了

            string strFontName = "";
            string strOther = "";
            StringUtil.ParseTwoPart(strFontString,
                ",",
                out strFontName,
                out strOther);

            this.textBox_fontString.Text = "barcode," + strOther;
        }

        private void button_setForeColor_Click(object sender, EventArgs e)
        {
            Color old_color = Color.Black;
            if (string.IsNullOrEmpty(this.textBox_foreColor.Text) == false)
                old_color = PrintLabelDocument.GetColor(this.textBox_foreColor.Text);

            ColorDialog dlg = new ColorDialog();

            dlg.Color = old_color;
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            if (dlg.Color == Color.Black)
                this.textBox_foreColor.Text = "";
            else
                this.textBox_foreColor.Text = PrintLabelDocument.GetColorString(dlg.Color);
        }

        // TODO: 能方便设置白色和透明色
        private void button_setBackColor_Click(object sender, EventArgs e)
        {
            Color trans_color = Color.Transparent;  //  Color.FromArgb(192, 192, 193);  // 使用一个罕见颜色
            Color old_color = trans_color;
            if (string.IsNullOrEmpty(this.textBox_backColor.Text) == false)
                old_color = PrintLabelDocument.GetColor(this.textBox_backColor.Text);

            ColorDialog dlg = new ColorDialog();

            dlg.Color = old_color;
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            if (dlg.Color == trans_color)
                this.textBox_backColor.Text = "";
            else
                this.textBox_backColor.Text = PrintLabelDocument.GetColorString(dlg.Color);
        }
    }
}
