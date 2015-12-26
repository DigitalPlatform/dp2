using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 能切换显示不同单位数值的 NumbericUpDown 控件
    /// </summary>
    public class UniverseNumericUpDown : NumericUpDown
    {
        private ToolTip toolTip1;
        // private System.ComponentModel.IContainer components;
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

                this.Value = ConvertValue(old_unit, value, this.Value);

                this._unitString = GetUnitString(this._currentUnit);
                if (this.toolTip1 != null)
                    this.toolTip1.SetToolTip(this.Controls[1], GetUnitString(this.CurrentUnit));
            }
        }

        public static string GetUnitString(GraphicsUnit unit)
        {
            if (unit == GraphicsUnit.Display)
                return "1/100 英寸";
            if (unit == GraphicsUnit.Millimeter)
                return "毫米";

            return unit.ToString();
        }

        /// <summary>
        /// 1/100 英寸的值
        /// </summary>
        public decimal UniverseValue
        {
            get
            {
                return ConvertValue(this._currentUnit, GraphicsUnit.Display, this.Value);
            }
            set
            {
                this.Value = ConvertValue(GraphicsUnit.Display, this._currentUnit, value); ;
            }
        }

        public static decimal ConvertValue(GraphicsUnit from, GraphicsUnit to, decimal value)
        {
            if (from == to)
                return value;

            // 先转为中立的单位 1/100 英寸
            double middle = 0;

            // Specifies the unit of measure of the display device. Typically pixels for video displays, and 1/100 inch for printers.
            if (from == GraphicsUnit.Display)
                middle = (double)value; // 1/100 英寸不变
            // Specifies the document unit (1/300 inch) as the unit of measure.
            if (from == GraphicsUnit.Document)
                middle = (double)value / (double)3; // 1/300 英寸 --> 1/100 英寸不变
            if (from == GraphicsUnit.Inch)
                middle = (double)value * (double)100; // 1 英寸 --> 1/100 英寸不变
            if (from == GraphicsUnit.Millimeter)
                middle = (double)value / (double)0.254; //  毫米 --> 1/100 英寸
            // Specifies a printer's point (1/72 inch) as the unit of measure.
            if (from == GraphicsUnit.Point)
                middle = (double)value * (double)72 / (double)100;    // 1 / 72 英寸 1/100 英寸

            if (to == GraphicsUnit.Display)
                return (decimal)middle;  // 1/100 英寸 --> 1/100 英寸
            if (to == GraphicsUnit.Document)
                return (decimal)(middle * (double)3); // 1/100 英寸 -> 1/300 英寸
            if (to == GraphicsUnit.Inch)
                return (decimal)(middle / (double)100);   // 1/100 英寸 --> 英寸
            if (to == GraphicsUnit.Millimeter)
                return (decimal)(middle * (double)0.254);  // 1/100 英寸 --> 毫米
            if (to == GraphicsUnit.Point)
                return (decimal)(middle * (double)100 / (double)72);  // 1/100 英寸 --> 1 / 72 英寸

            throw new Exception("尚未实现");
        }


        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

#if NO
            if (_initial == false)
            {
                Delegate_ShowTips d = new Delegate_ShowTips(ShowTips);
                this.BeginInvoke(d);
                _initial = true;
            }
#endif
            if (this.toolTip1 == null)
                this.toolTip1 = new System.Windows.Forms.ToolTip();

            if (string.IsNullOrEmpty(this._unitString) == true)
                this._unitString = GetUnitString(this.CurrentUnit);

            if (this.toolTip1 != null
                && string.IsNullOrEmpty(this._unitString) == false)
                this.toolTip1.SetToolTip(this.Controls[1], this._unitString);
        }

    }
}
