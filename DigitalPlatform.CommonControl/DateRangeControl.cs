using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CommonControl
{
    public partial class DateRangeControl : UserControl
    {
        int IngoreTextChange = 0;

        [Category("New Event")]
        public event EventHandler DateTextChanged = null;

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
            }
        }

        public DateRangeControl()
        {
            InitializeComponent();

            this.dateTimePicker_start.Value = this.dateTimePicker_start.MinDate;
            this.dateTimePicker_end.Value = this.dateTimePicker_end.MinDate;
            RefreshDisplay(this.dateTimePicker_start);
            RefreshDisplay(this.dateTimePicker_end);
        }

        public override Size MaximumSize
        {
            get
            {
                Size size = base.MaximumSize;
                int nLimitHeight = this.dateTimePicker_end.Location.Y + this.dateTimePicker_end.Height
                        + 4;
                if (size.Height > nLimitHeight
                    || size.Height == 0)
                    size.Height = nLimitHeight;

                return size;
            }
            set
            {
                base.MaximumSize = value;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size size = base.MinimumSize;
                int nLimitHeight = this.dateTimePicker_end.Location.Y + this.dateTimePicker_end.Height
                        + 4;
                int nLimitWidth = this.dateTimePicker_end.Location.X + this.dateTimePicker_end.Width
                        + 4;
                size.Height = nLimitHeight;
                size.Width = nLimitWidth;

                return size;
            }
            set
            {
                base.MinimumSize = value;
            }
        }

        public override string Text
        {
            get
            {
                string strStart = "";

                if (this.dateTimePicker_start.Value != this.dateTimePicker_start.MinDate)
                    strStart = this.dateTimePicker_start.Value.ToString("yyyyMMdd");

                string strEnd = "";
                if (this.dateTimePicker_end.Value != this.dateTimePicker_start.MinDate)
                    strEnd = this.dateTimePicker_end.Value.ToString("yyyyMMdd");

                if (String.IsNullOrEmpty(strStart) == true
                    && String.IsNullOrEmpty(strEnd) == true)
                    return "";

                return strStart + "-" + strEnd;
            }
            set
            {
                SetStartEnd(value);
            }
        }

        void SetStartEnd(string strValue)
        {
            this.dateTimePicker_start.Value = this.dateTimePicker_start.MinDate;
            this.dateTimePicker_end.Value = this.dateTimePicker_start.MinDate;

            strValue = strValue.Trim();

            if (String.IsNullOrEmpty(strValue) == true)
                return;

            string strStart = "";
            string strEnd = "";

            int nRet = strValue.IndexOf("-");
            if (nRet == -1)
            {
                strStart = strValue;
                if (strStart.Length != 8)
                    throw new Exception("时间范围 '" + strValue + "' 格式不正确");

                strEnd = "";
            }
            else
            {
                strStart = strValue.Substring(0, nRet).Trim();

                if (strStart.Length != 8
                    && string.IsNullOrEmpty(strStart) == false)
                    throw new Exception("时间范围 '" + strValue + "' 内 '" + strStart + "' 格式不正确");

                strEnd = strValue.Substring(nRet + 1).Trim();

                if (strEnd.Length != 8
                    && string.IsNullOrEmpty(strEnd) == false)
                    throw new Exception("时间范围 '" + strValue + "' 内 '" + strEnd + "' 格式不正确");
            }

            if (String.IsNullOrEmpty(strStart) == false)
                this.dateTimePicker_start.Value = Long8ToDateTime(strStart);
            if (String.IsNullOrEmpty(strEnd) == false)
                this.dateTimePicker_end.Value = Long8ToDateTime(strEnd);
        }

        public static DateTime Long8ToDateTime(string strDate8)
        {
            if (strDate8.Length != 8)
                throw new Exception("日期字符串格式必须为8字符。");

            int nYear = Convert.ToInt32(strDate8.Substring(0, 4));
            int nMonth = Convert.ToInt32(strDate8.Substring(4, 2));
            int nDay = Convert.ToInt32(strDate8.Substring(6, 2));

            return new DateTime(nYear, nMonth, nDay);
        }

        private void dateTimePicker_start_ValueChanged(object sender, EventArgs e)
        {
            RefreshDisplay(this.dateTimePicker_start);

            if (IngoreTextChange > 0)
                return;

            if (this.DateTextChanged != null)
            {
                this.DateTextChanged(sender, e);
            }
        }

        void RefreshDisplay(DateTimePicker picker)
        {
            if (picker.Value == picker.MinDate)
                picker.CustomFormat = " ";
            else
                picker.CustomFormat = "yyyy-MM-dd";
        }

        private void dateTimePicker_end_ValueChanged(object sender, EventArgs e)
        {
            RefreshDisplay(this.dateTimePicker_end);

            if (IngoreTextChange > 0)
                return;

            if (this.DateTextChanged != null)
            {
                this.DateTextChanged(sender, e);
            }
        }

        // 获得最近三年的年份字符串
        List<string> GetRecentYear()
        {
            List<string> results = new List<string>();
            int nCurrentYear = DateTime.Now.Year;

            results.Add((nCurrentYear - 1).ToString().PadLeft(4, '0'));
            results.Add(nCurrentYear.ToString().PadLeft(4, '0'));
            results.Add((nCurrentYear + 1).ToString().PadLeft(4, '0'));

            return results;
        }

        private void label_start_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            List<string> years = GetRecentYear();

            for (int i = 0; i < years.Count; i++)
            {
                string strYear = years[i];
                //
                menuItem = new MenuItem(strYear);
                contextMenu.MenuItems.Add(menuItem);

                // 
                string strPart = "全年";

                QuickSetParam param = new QuickSetParam();
                param.Year = strYear;
                param.Part = strPart;

                MenuItem subMenuItem = new MenuItem(strPart);
                subMenuItem.Click += new System.EventHandler(this.menu_quickSet_Click);
                subMenuItem.Tag = param;
                menuItem.MenuItems.Add(subMenuItem);

                // 
                strPart = "上半年";
                param = new QuickSetParam();
                param.Year = strYear;
                param.Part = strPart;

                subMenuItem = new MenuItem(strPart);
                subMenuItem.Click += new System.EventHandler(this.menu_quickSet_Click);
                subMenuItem.Tag = param;
                menuItem.MenuItems.Add(subMenuItem);

                // 
                strPart = "下半年";
                param = new QuickSetParam();
                param.Year = strYear;
                param.Part = strPart;

                subMenuItem = new MenuItem(strPart);
                subMenuItem.Click += new System.EventHandler(this.menu_quickSet_Click);
                subMenuItem.Tag = param;
                menuItem.MenuItems.Add(subMenuItem);
            }

            menuItem = new MenuItem("清空");
            menuItem.Click += menu_clear_Click;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.label_start, new Point(e.X, e.Y));
        }

        void menu_clear_Click(object sender, EventArgs e)
        {
            this.dateTimePicker_start.Value = this.dateTimePicker_start.MinDate;
            this.dateTimePicker_end.Value = this.dateTimePicker_end.MinDate;
        }

        void menu_quickSet_Click(object sender, EventArgs e)
        {
            MenuItem menu = (MenuItem)sender;

            QuickSetParam param = (QuickSetParam)menu.Tag;

            if (param.Part == "全年")
            {
                this.Text = param.Year + "0101-" + param.Year + "1231";
                return;
            }
            if (param.Part == "上半年")
            {
                this.Text = param.Year + "0101-" + param.Year + "0630";
                return;
            }
            if (param.Part == "下半年")
            {
                this.Text = param.Year + "0701-" + param.Year + "1231";
                return;
            }
            throw new Exception("未知的part参数值 '" + param.Part + "'");
        }

    }

    public class QuickSetParam
    {
        public string Year = "";
        public string Part = "";    // 全年/上半年/下半年 等等
    }
}
