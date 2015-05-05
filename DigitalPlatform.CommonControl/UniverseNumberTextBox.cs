using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 能切换显示不同单位数值列表的 TextBox 控件
    /// </summary>
    public class UniverseNumberTextBox : TextBox
    {
        /// <summary>
        /// 出现验证结果对话框的时候，内容的格式。有 2 个参数，分别是错误提示内容、值字符串
        /// </summary>
        public string ValidateWarningFormat = "{0}";

        private ToolTip toolTip1;

        GraphicsUnit _currentUnit = GraphicsUnit.Display;

        string _unitString = "";

        public GraphicsUnit CurrentUnit
        {
            get
            {
                return this._currentUnit;
            }
            set
            {
                GraphicsUnit old_unit = this._currentUnit;

                this._currentUnit = value;

                this.Text = ConvertValue(old_unit, value, this.Text);


                this._unitString = UniverseNumericUpDown.GetUnitString(this._currentUnit);
                if (this.toolTip1 != null)
                    this.toolTip1.SetToolTip(this, UniverseNumericUpDown.GetUnitString(this.CurrentUnit));
            }
        }



        /// <summary>
        /// 1/100 英寸单位下的值
        /// </summary>
        public string UniverseText
        {
            get
            {
                return ConvertValue(this._currentUnit, GraphicsUnit.Display, this.Text);
            }
            set
            {
                this.Text = ConvertValue(GraphicsUnit.Display, this._currentUnit, value);
            }
        }

        public static string ConvertValue(GraphicsUnit from, GraphicsUnit to, string strText)
        {
            if (from == to)
                return strText;

            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return "";

#if NO
            if (strText.IndexOfAny(new char[] {'，'}) != -1)
            {
                strError = "内容中不能使用中文逗号。应该使用西文逗号";
                return -1;
            }
#endif
            strText = strText.Replace("，", ","); // 将中文逗号替换为西文逗号

            string[] numbers = strText.Split(new char [] {','});
            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (string number in numbers)
            {
                if (i > 0)
                    text.Append(",");

                decimal v = 0;
                if (decimal.TryParse(number, out v) == false)
                    text.Append(number);
                else
                    text.Append(UniverseNumericUpDown.ConvertValue(from, to, v).ToString());

                i++;
            }

            return text.ToString();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (this.toolTip1 == null)
                this.toolTip1 = new System.Windows.Forms.ToolTip();

            if (string.IsNullOrEmpty(this._unitString) == true)
                this._unitString = UniverseNumericUpDown.GetUnitString(this.CurrentUnit);

            if (this.toolTip1 != null
                && string.IsNullOrEmpty(this._unitString) == false)
                this.toolTip1.SetToolTip(this, this._unitString);
        }

        /// <summary>
        /// 验证字符串格式
        /// 应当是逗号间隔的浮点数
        /// </summary>
        /// <param name="strText">要验证的内容</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1 表示有错误; 0 表示正确</returns>
        public static int ValidateNumberListString(string strText, out string strError)
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
            strText = strText.Replace("，",","); // 将中文逗号替换为西文逗号

            string[] numbers = strText.Split(new char[] { ',' });
            foreach (string number in numbers)
            {
                if (string.IsNullOrEmpty(number) == true)
                {
                    strError = "'"+strText+"' 中，(两个逗号之间)不应出现空的部分";
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
                    strError = "'" + strText + "'中，'"+strNumber+"' 部分格式不正确，应该是一个数字";
                    return -1;
                }
            }

            return 0;
        }

        protected override void OnValidating(System.ComponentModel.CancelEventArgs e)
        {
            base.OnValidating(e);

            string strError = "";
            int nRet = ValidateNumberListString(this.Text, out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.Parent, 
                    string.IsNullOrEmpty(this.ValidateWarningFormat) == true ?
                    strError : string.Format(this.ValidateWarningFormat, strError, this.Text));
                e.Cancel = true;
            }
        }
    }
}
