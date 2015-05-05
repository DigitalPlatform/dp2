using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 编辑 padding 4 个数字的对话框
    /// </summary>
    public partial class PaddingDialog : Form
    {
        public PaddingDialog()
        {
            InitializeComponent();

            this.numericUpDown_left.Maximum = decimal.MaxValue;
            this.numericUpDown_top.Maximum = decimal.MaxValue;
            this.numericUpDown_right.Maximum = decimal.MaxValue;
            this.numericUpDown_bottom.Maximum = decimal.MaxValue;
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
                this.numericUpDown_left.CurrentUnit = value;
                this.numericUpDown_right.CurrentUnit = value;

                this.numericUpDown_top.CurrentUnit = value;
                this.numericUpDown_bottom.CurrentUnit = value;

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
                return this.numericUpDown_left.DecimalPlaces;
            }
            set
            {
                this.numericUpDown_left.DecimalPlaces = value;
                this.numericUpDown_right.DecimalPlaces = value;
                this.numericUpDown_top.DecimalPlaces = value;
                this.numericUpDown_bottom.DecimalPlaces = value;
            }
        }

        public decimal LeftValue
        {
            get
            {
                return this.numericUpDown_left.UniverseValue;
            }
            set
            {
                this.numericUpDown_left.UniverseValue = value;
            }
        }

        public decimal RightValue
        {
            get
            {
                return this.numericUpDown_right.UniverseValue;
            }
            set
            {
                this.numericUpDown_right.UniverseValue = value;
            }
        }

        public decimal TopValue
        {
            get
            {
                return this.numericUpDown_top.UniverseValue;
            }
            set
            {
                this.numericUpDown_top.UniverseValue = value;
            }
        }

        public decimal BottomValue
        {
            get
            {
                return this.numericUpDown_bottom.UniverseValue;
            }
            set
            {
                this.numericUpDown_bottom.UniverseValue = value;
            }
        }

        /// <summary>
        /// 字符串值
        /// 当前单位，CurrentUnit
        /// </summary>
        public string StringValue
        {
            get
            {
                return this.numericUpDown_left.Value.ToString() + ","
                    + this.numericUpDown_top.Value.ToString() + ","
                    + this.numericUpDown_right.Value.ToString() + ","
                    + this.numericUpDown_bottom.Value.ToString();
            }
            set
            {
#if NO
                string[] parts = value.Split(new char[] {','});
                if (parts.Length != 4)
                    return;

                int v = 0;
                int.TryParse(parts[0], out v);
                this.Left = v;

                int.TryParse(parts[1], out v);
                this.Top = v;

                int.TryParse(parts[2], out v);
                this.Right = v;

                int.TryParse(parts[3], out v);
                this.Bottom = v;
#endif
                try
                {
                    // 可能会抛出 ArgumentException 异常
                    DecimalPadding padding = ParsePaddingString(value);
                    this.numericUpDown_left.Value = padding.Left;
                    this.numericUpDown_top.Value = padding.Top;
                    this.numericUpDown_right.Value = padding.Right;
                    this.numericUpDown_bottom.Value = padding.Bottom;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 字符串值
        /// 始终为 1/100 英寸单位，不管界面如何显示变化
        /// </summary>
        public string UniverseStringValue
        {
            get
            {
                return (this.LeftValue).ToString() + ","
                    + (this.TopValue).ToString() + ","
                    + (this.RightValue).ToString() + ","
                    + (this.BottomValue).ToString();
            }
            set
            {
#if NO
                string[] parts = value.Split(new char[] {','});
                if (parts.Length != 4)
                    return;

                int v = 0;
                int.TryParse(parts[0], out v);
                this.Left = v;

                int.TryParse(parts[1], out v);
                this.Top = v;

                int.TryParse(parts[2], out v);
                this.Right = v;

                int.TryParse(parts[3], out v);
                this.Bottom = v;
#endif
                try
                {
                    // 可能会抛出 ArgumentException 异常
                    DecimalPadding padding = ParsePaddingString(value);
                    this.LeftValue = padding.Left;
                    this.TopValue = padding.Top;
                    this.RightValue = padding.Right;
                    this.BottomValue = padding.Bottom;
                }
                catch
                {
                }
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


        /// <summary>
        /// 解析 Padding 字符串
        /// 可能会抛出 ArgumentException 异常
        /// </summary>
        /// <param name="strValue">要解析的字符串</param>
        /// <returns>返回 DecimalPadding 对象</returns>
        public static DecimalPadding ParsePaddingString(string strValue)
        {
            DecimalPadding padding = new DecimalPadding();

            if (string.IsNullOrEmpty(strValue) == true)
                return padding;

            strValue = strValue.Trim();
            if (string.IsNullOrEmpty(strValue) == true)
                return padding;

            strValue = strValue.Replace("，", ","); // 将中文逗号替换为西文逗号

            string[] parts = strValue.Split(new char[] { ',' });
            if (parts.Length != 4)
                throw new ArgumentException("padding 字符串 '" + strValue + "' 不合法。应该是逗号间隔的 4 个数字");

            decimal v = 0;
            decimal.TryParse(parts[0], out v);
            padding.Left = v;

            decimal.TryParse(parts[1], out v);
            padding.Top = v;

            decimal.TryParse(parts[2], out v);
            padding.Right = v;

            decimal.TryParse(parts[3], out v);
            padding.Bottom = v;

            return padding;
        }

        /// <summary>
        /// 验证字符串格式
        /// 应当是逗号间隔的 4 个浮点数
        /// </summary>
        /// <param name="strText">要验证的内容</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1 表示有错误; 0 表示正确</returns>
        public static int ValidateValueString(string strText, out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strText) == true)
                return 0;

            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return 0;

#if NO
            if (strText.IndexOfAny(new char[] {'，'}) != -1)
            {
                strError = "内容中不能使用中文逗号。应该使用西文逗号";
                return -1;
            }
#endif
            strText = strText.Replace("，", ","); // 将中文逗号替换为西文逗号

            string[] numbers = strText.Split(new char[] { ',' });
            if (numbers.Length != 4)
            {
                strError = "'" + strText + "' 格式不正确。应该是逗号间隔的 4 个数字";
                return -1;
            }
            foreach (string number in numbers)
            {
                if (string.IsNullOrEmpty(number) == true)
                {
                    strError = "'" + strText + "' 中，(两个逗号之间)不应出现空的部分";
                    return -1;
                }

                string strNumber = number.Trim();
                if (string.IsNullOrEmpty(strNumber) == true)
                {
                    strError = "'" + strText + "' 中，(两个逗号之间)不应出现空的部分";
                    return -1;
                }

                decimal v = 0;
                if (decimal.TryParse(strNumber, out v) == false)
                {
                    strError = "'" + strText + "'中，'" + strNumber + "' 部分格式不正确，应该是一个数字";
                    return -1;
                }
            }

            return 0;
        }

        // 设置四个为同样的值
        void SetFourValue(decimal value)
        {
            if (this.numericUpDown_left.Value != value)
                this.numericUpDown_left.Value = value;

            if (this.numericUpDown_right.Value != value)
                this.numericUpDown_right.Value = value;

            if (this.numericUpDown_top.Value != value)
                this.numericUpDown_top.Value = value;

            if (this.numericUpDown_bottom.Value != value)
                this.numericUpDown_bottom.Value = value;
        }

        private void numericUpDown_ValueChanged(object sender, EventArgs e)
        {

            if (Control.ModifierKeys == Keys.Control)
            {
                SetFourValue((sender as NumericUpDown).Value);
            }
        }
    }

    public class DecimalPadding
    {
        public decimal Left = 0;
        public decimal Top = 0;
        public decimal Right = 0;
        public decimal Bottom = 0;

        public DecimalPadding()
        {
        }

        public DecimalPadding(decimal left, decimal right, decimal top, decimal bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }
}
