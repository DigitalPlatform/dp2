using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace DigitalPlatform.Marc
{
    // 字段
    /// <summary>
    /// 字段对象
    /// </summary>
	public class Field
    {
        /// <summary>
        /// 容器，也就是当前字段对象所从属的字段对象数组
        /// </summary>
		internal FieldCollection container = null;

        internal string m_strName = "";
        internal string m_strIndicator = "";
        internal string m_strValue = "";
        internal string m_strNameCaption = "字段说明";

        internal int PureHeight = 20;

        /// <summary>
        /// 当前对象是否处在选中状态
        /// </summary>
		public bool Selected = false;

        /// <summary>
        /// 构造函数
        /// </summary>
		public Field()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="field_collection">拥有本对象的字段对象数组</param>
		public Field(FieldCollection field_collection)
        {
            this.container = field_collection;
        }

        // 字段名
        /// <summary>
        /// 获取或设置字段名
        /// </summary>
        public string Name
        {
            get
            {
                return this.m_strName;
            }
            set
            {
                if (this.m_strName != value)
                {
                    this.m_strName = value;

                    if (this.container == null)
                        return;

                    this.m_strNameCaption = this.container.MarcEditor.GetLabel(this.m_strName);

                    if (this.container.MarcEditor.FocusedField == this
                        && this.container.MarcEditor.m_nFocusCol == 1)
                    {
                        this.container.MarcEditor.curEdit.Text = this.m_strName;
                    }

                    // 失效???用不到判断当前元素是末尾元素从而使用BoundsPortion.FieldAndBottom
                    Rectangle rect = this.container.MarcEditor.GetItemBounds(this.container.IndexOf(this),
                        1,
                        BoundsPortion.Field);
                    this.container.MarcEditor.Invalidate(rect);

                    // 文档发生改变
                    this.container.MarcEditor.FireTextChanged();
                }
            }
        }

        internal void RefreshNameCaption()
        {
            this.m_strNameCaption = this.container.MarcEditor.GetLabel(this.m_strName);
        }

        // 字段指示符
        /// <summary>
        /// 获取或设置字段指示符
        /// </summary>
        public string Indicator
        {
            get
            {
                return this.m_strIndicator;
            }
            set
            {
                if (this.m_strIndicator != value)
                {
                    this.m_strIndicator = value;

                    if (this.container == null)
                        return;


                    if (this.container != null
                        && this.container.MarcEditor.FocusedField == this
                        && this.container.MarcEditor.m_nFocusCol == 2
                        && Record.IsControlFieldName(this.Name) == false)
                    {
                        this.container.MarcEditor.curEdit.Text = this.m_strIndicator;
                    }


                    // 失效???用不用判断当前元素是末尾元素从而使用BoundsPortion.FieldAndBottom
                    Rectangle rect = this.container.MarcEditor.GetItemBounds(this.container.IndexOf(this),
                        1,
                        BoundsPortion.Field);
                    this.container.MarcEditor.Invalidate(rect);

                    // 文档发生改变
                    this.container.MarcEditor.FireTextChanged();
                }
            }
        }

        // 字段值。get没有替换^符号
        internal string ValueKernel
        {
            get
            {
                return this.m_strValue;
            }
            /*
			set
			{
				string strValue = value;
                strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

                if (this.container == null)
                    return;
                if (this.container.MarcEditor == null)
                    return;

				if (this.m_strValue != strValue)
				{
					this.m_strValue = strValue;

					if (this.container.MarcEditor.FocusedField == this
						&& this.container.MarcEditor.m_nFocusCol == 3)
					{
						this.container.MarcEditor.curEdit.Text = this.m_strValue;
					}

					// 失效???用不到判断当前元素是末尾元素从而使用BoundsPortion.FieldAndBottom
					Rectangle rect = this.container.MarcEditor.GetItemBounds(this.container.IndexOf(this),
						-1,
						BoundsPortion.FieldAndBottom);
					this.container.MarcEditor.Invalidate(rect);

					// 文档发生改变
					this.container.MarcEditor.FireTextChanged();
				}
			}
             */
        }

        /// <summary>
        /// 获取或设置 字段指示符联合字段内容
        /// </summary>
        public string IndicatorAndValue
        {
            get
            {
                return this.Indicator + this.Value;
            }
            set
            {
                if (Record.IsControlFieldName(this.Name) == true)
                {
                    this.Value = value;
                    return;
                }

                if (value.Length >= 2)
                {
                    this.Indicator = value.Substring(0, 2);
                    this.Value = value.Substring(2);
                }
                else
                {
                    this.Indicator = value.PadRight(2, ' ');    // 填充空白
                    this.Value = "";
                }
            }
        }

        // 字段值。get替换了^符号
        /// <summary>
        /// 获取或设置 字段值，也就是字段内容
        /// </summary>
        public string Value
        {
            get
            {
                return this.m_strValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);
            }
            set
            {
                string strValue = value;
                strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);

                if (this.container == null)
                    return;
                if (this.container.MarcEditor == null)
                    return;

                if (this.m_strValue != strValue)
                {
                    this.m_strValue = strValue;

                    this.CalculateHeight(null, false);  // 重新计算高度 2014/11/4

                    if (this.container.MarcEditor.FocusedField == this
                        && this.container.MarcEditor.m_nFocusCol == 3)
                    {
                        this.container.MarcEditor.curEdit.Text = this.m_strValue;
                    }

                    // 失效???用不到判断当前元素是末尾元素从而使用BoundsPortion.FieldAndBottom
                    Rectangle rect = this.container.MarcEditor.GetItemBounds(this.container.IndexOf(this),
                        -1,
                        BoundsPortion.FieldAndBottom);
                    this.container.MarcEditor.Invalidate(rect);

                    // 文档发生改变
                    this.container.MarcEditor.FireTextChanged();
                }
            }
        }

        // 字段名称说明
        internal string NameCaption
        {
            get
            {
                return this.m_strNameCaption;
            }
        }

        // 本字段总高度
        /// <summary>
        /// 获取本字段的显示区域的高度
        /// </summary>
        public int TotalHeight
        {
            get
            {
                return this.container.record.GridHorzLineHeight
                    + this.container.record.CellTopBlank
                    + this.PureHeight
                    + this.container.record.CellBottomBlank;
            }
        }

        /// <summary>
        /// 获取或设置本字段的 MARC 字符串 (机内格式)
        /// </summary>
		public string Text
        {
            get
            {
                return this.GetFieldMarc(false);
            }
            set
            {
                this.SetFieldMarc(value);

                if (this.container == null)
                    return;
                if (this.container.MarcEditor == null)
                    return;

                // 失效???用不到判断当前元素是末尾元素从而使用BoundsPortion.FieldAndBottom
                Rectangle rect = this.container.MarcEditor.GetItemBounds(this.container.IndexOf(this),
                    -1,
                    BoundsPortion.FieldAndBottom);

                // 应把失效区域计算出来，进行优化
                InvalidateRect iRect = new InvalidateRect();
                iRect.bAll = false;
                iRect.rect = rect;
                this.container.MarcEditor.AfterDocumentChanged(ScrollBarMember.Vert,
                    iRect);
            }
        }

        // 获得字段的MARC格式
        // parameters:
        //      bAddFLDEND  是否加字段结束符
        // return:
        //      字段的MARC字符串
        /// <summary>
        /// 获取本字段的 MARC 字符串 (机内格式)
        /// </summary>
        /// <param name="bAddFLDEND">是否包含字段结束符</param>
        /// <returns>MARC 字符串</returns>
		public string GetFieldMarc(bool bAddFLDEND)
        {
            if (this.Name == "###") // 头标区
            {
                if (this.m_strValue.Length < 24)
                    this.m_strValue = this.m_strValue.PadRight(24, '?');
                return this.m_strValue;
            }


            if (Record.IsControlFieldName(this.m_strName) == true) // 控制字段
            {
                if (this.Indicator != "")
                {
                    //Debug.Assert(false,"不可能的情况，控制字段无字段指示符");
                    this.m_strIndicator = "";
                }
            }
            else
            {
                if (this.Indicator.Length != 2)
                {
                    //Debug.Assert(false,"不可能的情况，字段指示符1必须是两位");

                    if (this.m_strIndicator.Length > 2)
                        this.m_strIndicator = this.m_strIndicator.Substring(0, 2);
                    else if (this.m_strIndicator.Length < 2)
                        this.m_strIndicator = this.m_strIndicator.PadLeft(2, ' ');
                }
            }

            // TODO: 看看这里的 string 拼接是否可以加速
            string strFieldMarc = this.m_strName + this.m_strIndicator + this.m_strValue;

            if (bAddFLDEND == true)
                strFieldMarc += Record.FLDEND;


            strFieldMarc = strFieldMarc.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);
#if BIDI_SUPPORT
            // strFieldMarc = strFieldMarc.Replace("\x200e", "");
            strFieldMarc = MarcEditor.RemoveBidi(strFieldMarc);
#endif
            return strFieldMarc;
        }

        /// <summary>
        /// 设置本字段的 MARC 字符串 (机内格式)
        /// </summary>
        /// <param name="strFieldMarc">本字段的 MARC 字符串</param>
        public void SetFieldMarc(string strFieldMarc)
        {
            SetFieldMarc(strFieldMarc, true);
        }

        /// <summary>
        /// 设置本字段的 MARC 字符串 (机内格式)
        /// </summary>
        /// <param name="strFieldMarc">本字段的 MARC 字符串</param>
        /// <param name="bFlushEdit">是否自动刷新小 Edit</param>
		internal void SetFieldMarc(string strFieldMarc, bool bFlushEdit)
        {
            if (this.container == null)
                return;

            int index = this.container.IndexOf(this);

            if (index == 0)
            {
                this.m_strValue = strFieldMarc;
                // 2011/4/21
                this.CalculateHeight(null,
    true);
                return;
            }

            if (strFieldMarc.Length < 3)
                strFieldMarc = strFieldMarc + new string(' ', 3 - strFieldMarc.Length);

            string strName = "";
            string strIndicator = "";
            string strValue = "";

            strName = strFieldMarc.Substring(0, 3);
            if (Record.IsControlFieldName(strName) == true)
            {
                strIndicator = "";
                strValue = strFieldMarc.Substring(3);
            }
            else
            {
                if (strFieldMarc.Length < 5)
                    strFieldMarc = strFieldMarc + new string(' ', 5 - strFieldMarc.Length);

                strIndicator = strFieldMarc.Substring(3, 2);

                strValue = strFieldMarc.Substring(5);
            }

            string strCaption = this.container.MarcEditor.GetLabel(strName);

            strValue = strValue.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);
            this.m_strNameCaption = strCaption;
            this.m_strName = strName;
            this.m_strIndicator = strIndicator;
            this.m_strValue = strValue;
            if (this.container.MarcEditor.FocusedField == this)
            {
                /*
                if (this.container.MarcEditor.m_nFocusCol == 2)
    				this.container.MarcEditor.curEdit.Text = this.m_strValue;
                 * */
                if (bFlushEdit == true) // 2014/7/10
                    this.container.MarcEditor.ItemTextToEditControl();  // 2009/3/6 changed
            }
            this.CalculateHeight(null,
                true);

            this.container.MarcEditor.FireTextChanged();
        }

        // 计算行的高度
        //		g	Graphics对象，如果为null，则自动找
        //		bIgnoreEdit	是否忽略小edit控件 false不忽略
        internal void CalculateHeight(Graphics g_param, bool bIgnoreEdit)
        {
            Graphics g = g_param;
            if (g == null)
                g = Graphics.FromHwnd(this.container.MarcEditor.Handle);

            try
            {
                Font font = this.container.MarcEditor.Font;//.DefaultTextFont;

                Font fixedfont = this.container.MarcEditor.FixedSizeFont;

                //IntPtr hFontOld = IntPtr.Zero;

                // 计算Name
                /*
                SizeF size = g.MeasureString(this.m_strName,
                    font, 
                    this.container.record.NamePureWidth, 
                    new StringFormat());
                 */
                SizeF size = TextRenderer.MeasureText(g,
                    this.m_strName,
                    fixedfont,
                    new Size(container.record.NamePureWidth, -1),
                    MarcEditor.editflags);


                int h1 = (int)size.Height;

                // 计算Indicator1
                /*
                size = g.MeasureString(this.m_strIndicator,
                    font, 
                    container.record.IndicatorPureWidth,
                    new StringFormat());
                 */
                size = TextRenderer.MeasureText(g,
                    this.m_strIndicator,
                    fixedfont,
                    new Size(container.record.IndicatorPureWidth, -1),
                    MarcEditor.editflags);

                int h2 = (int)size.Height;

                if (h1 < h2)
                    h1 = h2;


                // 计算m_strValue
                /*
                size = g.MeasureString(this.m_strValue,
                    font, 
                    container.record.ValuePureWidth,
                    new StringFormat());
                 */
#if BIDI_SUPPORT
                // string strValue = this.m_strValue.Replace(new string(Record.KERNEL_SUBFLD, 1), "\x200e" + new string(Record.KERNEL_SUBFLD, 1));
                string strValue = MarcEditor.AddBidi(this.m_strValue);
#endif
                size = TextRenderer.MeasureText(g,
#if BIDI_SUPPORT
 strValue == "" ? "lg" : strValue,
#else
                this.m_strValue == "" ? "lg" : this.m_strValue,
#endif
 font,
                    new Size(container.record.ValuePureWidth, -1),
                    MarcEditor.editflags);

                int h3 = (int)size.Height;

                if (h1 < h3)
                    h1 = h3;

                // 注意这里故意没算NameCaption的高度
                /*
                // 计算NameCaption
                size = g.MeasureString(this.strNameCaption,
                    font, 
                    container.NameCaptionPureWidth,
                    new StringFormat());
                int h4 = (int)size.Height;

                if (h1 < h4)
                    h1 = h4;
                */

                if (this.container.MarcEditor.SelectedFieldIndices.Count == 1)
                {
                    Field FocusedField = this.container.MarcEditor.FocusedField;
                    // 如果 bIgnoreEdit == true，不考虑当前行edit控件的既有高度
                    if (bIgnoreEdit == false
                        && FocusedField != null
                        && this == FocusedField)
                    {
                        int h5 = this.container.MarcEditor.curEdit.Height;
                        if (h1 < h5)
                            h1 = h5;
                    }
                }

                this.PureHeight = h1;
            }
            finally
            {
                if (g_param == null)
                    g.Dispose();
            }
        }


        // 把本行绘制出来
        // parameters:
        //		pe	用PaintEventArgs对象而不用Graphics对象的目的是为了其有ClipRectangle成员，所以使绘制优化
        //		nBaseX	x坐标
        //		nBaseY	y坐标
        //      edit_on 小编辑器是否处在本字段上。
        //              如果是，.Select 也绘制为普通状态(本函数调主会考虑绘制 focus 虚线框)。
        //              如果否，.Selected 绘制为选中状态
        // return:
        //		void
        internal void Paint(PaintEventArgs pe,
            int nBaseX,
            int nBaseY,
            bool edit_on)
        {
            var record = this.container.record;

            // -----------------------------------------
            // 计算出本行的总共区域
            // 每行区域中包括左上的线，不包括右下的线
            Rectangle totalRect = new Rectangle(
                nBaseX,
                nBaseY,
                record.TotalLineWidth,
                this.TotalHeight);

            // 优化
            if (totalRect.IntersectsWith(pe.ClipRectangle) == false)
                return;

            // -----------------------------------------
            // 每个单元格包括左上的线，不包括右下的线

            // 绘NameCaption
            Rectangle nameCaptionRect = new Rectangle(
                nBaseX,
                nBaseY,
                record.NameCaptionTotalWidth,
                this.TotalHeight);
            if (nameCaptionRect.IntersectsWith(pe.ClipRectangle) == true)
            {
                this.DrawCell(pe.Graphics,
                    0,
                    nameCaptionRect,
                    edit_on);
            }

            // 绘Name
            Rectangle nameRect = new Rectangle(
                nBaseX + record.NameCaptionTotalWidth,
                nBaseY,
                record.NameTotalWidth - record.CellBlanking,
                this.TotalHeight);
            if (nameRect.IntersectsWith(pe.ClipRectangle) == true)
            {
                this.DrawCell(pe.Graphics,
                    1,
                    nameRect,
                    edit_on);
            }

            // 绘Indicator
            Rectangle indicatorRect = new Rectangle(
                nBaseX + record.NameCaptionTotalWidth + this.container.record.NameTotalWidth,
                nBaseY,
                record.IndicatorTotalWidth - record.CellBlanking,
                this.TotalHeight);
            if (indicatorRect.IntersectsWith(pe.ClipRectangle) == true)
            {
                this.DrawCell(pe.Graphics,
                    2,
                    indicatorRect,
                    edit_on);
            }

            // 绘m_strValue
            Rectangle valueRect = new Rectangle(
                nBaseX + record.NameCaptionTotalWidth + record.NameTotalWidth + this.container.record.IndicatorTotalWidth /*+ this.container.Indicator2TotalWidth*/,
                nBaseY,
                record.ValueTotalWidth,
                this.TotalHeight);
            if (valueRect.IntersectsWith(pe.ClipRectangle) == true)
            {
                this.DrawCell(pe.Graphics,
                    3,
                    valueRect,
                    edit_on);
            }
        }

        // 画单元格，包括背景，文字 和 左上线条
        // parameter:
        //		g	Graphics对象
        //		nCol	列号 
        //				0 字段说明;
        //				1 字段名;
        //				2 字段指示符 
        //				3 字段内容
        //		rect	区域 如果为null，则自动根据列号计算 但目前不支持
        // return:
        //		void
        internal void DrawCell(Graphics g,
            int nCol,
            Rectangle rect,
            bool edit_on)
        {
            Debug.Assert(g != null, "g参数不能为null");

            string strText = "";
            int nWidth = 0;

            var marcEditor = this.container.MarcEditor;
            var record = this.container.record;

            bool bEnabled = marcEditor.Enabled;
            bool bReadOnly = marcEditor.ReadOnly;
            bool selected = this.Selected;
            if (edit_on)
                selected = false;

            var is_control_field = Record.IsControlFieldName(this.Name);
            var inset = marcEditor.InsetStyleCellBorder;
            Brush brush = null;
            try
            {
                if (nCol == 0)
                {
                    // NameCaption

                    Color backColor;

                    if (bEnabled == false || bReadOnly == true)
                        backColor = SystemColors.Control;
                    else
                        backColor = marcEditor.defaultNameCaptionBackColor;

                    // 如果本行为当前活动行，则名称部分高亮显示
                    if (selected)   //marcEditor.CurField == this)
                    {
                        if (backColor.GetBrightness() < 0.5f)
                            backColor = ControlPaint.Light(backColor);
                        else
                            backColor = ControlPaint.Dark(backColor);
                    }

                    strText = this.m_strNameCaption;
                    nWidth = record.NameCaptionPureWidth;
                    brush = new SolidBrush(backColor);
                }
                else if (nCol == 1)
                {
                    // Name
                    Color backColor;

                    if (selected)
                        backColor = marcEditor.defaultSelectedBackColor;
                    else if (bEnabled == false || bReadOnly == true)
                        backColor = SystemColors.Control;
                    else
                        backColor = marcEditor.defaultNameBackColor;

                    if (this.Name == "###")
                        backColor = record.marcEditor.defaultNameCaptionBackColor;

                    /*
                    // 如果本行为当前活动行，则名称部分高亮显示
                    if (this.Selected == true)  // marcEditor.FocusedField == this)
                    {
                        backColor = ControlPaint.Light(backColor);
                    }
                    */

                    strText = this.m_strName;
                    nWidth = record.NamePureWidth;
                    brush = new SolidBrush(backColor);
                }
                else if (nCol == 2)
                {
                    // Indicator
                    Color backColor;

                    if (selected)
                        backColor = marcEditor.defaultSelectedBackColor;
                    else if (bEnabled == false || bReadOnly == true)
                        backColor = SystemColors.Control;
                    else
                        backColor = marcEditor.defaultIndicatorBackColor;

                    if (is_control_field)
                        backColor = marcEditor.defaultIndicatorBackColorDisabled;

                    /*
                    // 如果本行为当前活动行，则名称部分高亮显示
                    if (this.Selected == true)  // marcEditor.FocusedField == this)
                    {
                        backColor = ControlPaint.Light(backColor);
                    }
                    */

                    strText = this.m_strIndicator;
                    nWidth = record.IndicatorPureWidth;
                    brush = new SolidBrush(backColor);
                }
                else if (nCol == 3)
                {
                    // m_strValue
                    strText = this.m_strValue;
                    nWidth = record.ValuePureWidth + 0;  // 1为微调,正好!
                    if (selected)
                        brush = new SolidBrush(marcEditor.defaultSelectedBackColor);
                    else if (bEnabled == false || bReadOnly == true)
                        brush = new SolidBrush(SystemColors.Control);
                    else
                        brush = new SolidBrush(marcEditor.defaultContentBackColor);
                }
                else
                {
                    Debug.Assert(false, "nCol的值'" + Convert.ToString(nCol) + "'不合法");
                }

                //               new Point(-marcEditor.DocumentOrgX + 0, -marcEditor.DocumentOrgY + marcEditor.DocumentHeight),
                //new Point(-marcEditor.DocumentOrgX + marcEditor.DocumentWidth, - marcEditor.DocumentOrgY + 0),

                if (marcEditor.GradientBack)
                {
                    using (LinearGradientBrush linGrBrush = new LinearGradientBrush(
           new Point(0, 0),
           new Point(marcEditor.DocumentWidth, 0),
           Color.FromArgb(255, 240, 240, 240),  // 240, 240, 240
           Color.FromArgb(255, 255, 255, 255)   // Opaque red
           ))  // Opaque blue
                    {
                        linGrBrush.GammaCorrection = true;

                        // --------画背景----------------------------

                        if ((nCol == 1 || nCol == 2 || nCol == 3)
                            && (bEnabled == true && bReadOnly == false && selected == false))
                        {
                            g.FillRectangle(linGrBrush, rect);
                        }
                        else
                            g.FillRectangle(brush, rect);
                    }
                }
                else
                    g.FillRectangle(brush, rect);

                // --------画线条----------------------------

                // 只画上，左

                // *** 绘制陷入效果的边框
                if (inset)
                {
                    // 2024/7/7
                    if (nCol == 1 || (nCol == 2 && is_control_field == false))
                    {
                        // 四边边框
                        {
                            Rectangle temp_rect = rect;
                            temp_rect.Height = 4 + record.NamePureHeight;
                            ControlPaint.DrawBorder(g,
                                temp_rect,
                                SystemColors.Control, 2, ButtonBorderStyle.Inset,
                                SystemColors.Control, 2, ButtonBorderStyle.Inset,
                                SystemColors.Control, 2, ButtonBorderStyle.Inset,
                                SystemColors.Control, 2, ButtonBorderStyle.Inset);
                        }
                        // 下方坚硬部分
                        {
                            Rectangle temp_rect = rect;
                            int delta = 4 + record.NamePureHeight;
                            if (temp_rect.Height > delta)
                            {
                                temp_rect.Y += delta;
                                temp_rect.Height -= delta;
                                using (var brush0 = new SolidBrush(SystemColors.Control))
                                {
                                    g.FillRectangle(brush0, temp_rect);
                                }
                            }
                        }
                    }
                    else if (nCol == 3)
                    {
                        // 只有左侧一根竖线
                        ControlPaint.DrawBorder(g,
        rect,
        SystemColors.Control, 2, ButtonBorderStyle.Inset,
        Color.White, 0, ButtonBorderStyle.None,
        Color.White, 0, ButtonBorderStyle.None,
        Color.White, 0, ButtonBorderStyle.None);
                    }
                }

                int nGridWidth = 0;
                if (nCol == 1)
                    nGridWidth = record.GridVertLineWidthForSplit;
                else
                    nGridWidth = record.GridVertLineWidth;

                if (inset == false)
                {
                    // *** 绘制平坦风格的背景

                    // 画上方的线条
                    Field.DrawLines(g,
                        rect,
                        record.GridHorzLineHeight,   // 上
                        0,
                        0,
                        0,
                        marcEditor.defaultHorzGridColor);

                    // 画左方的线条
                    // 

                    // indicator左边的竖线短一点
                    if (nCol == 2)
                    {
                        rect.Y += 2;
                        rect.Height = record.NamePureHeight;
                    }

                    Field.DrawLines(g,
                        rect,
                        0,
                        0,
                        nGridWidth, // 左
                        0,
                        marcEditor.defaultVertGridColor);

                    if (nCol == 2)  // 还原
                    {
                        rect.Y -= 2;
                    }
                }

                // --------画文字----------------------------
                if (nWidth > 0)
                {
                    Rectangle textRect = new Rectangle(
                        rect.X + nGridWidth/*this.container.GridVertLineWidth*/ + record.CellLeftBlank,
                        rect.Y + this.container.record.GridHorzLineHeight + record.CellTopBlank,
                        nWidth,
                        this.PureHeight);

                    // 这里的 font 是引用，因此不需要释放
                    Font font = null;
                    if (nCol == 0)
                    {
                        font = marcEditor.CaptionFont;
                        //Debug.Assert(font != null, "");
                    }
                    else if (nCol == 1 || nCol == 2)
                    {
                        font = marcEditor.FixedSizeFont;
                        //Debug.Assert(font != null, "");
                    }
                    else
                    {
                        font = marcEditor.Font;
                        // Debug.Assert(font != null, "");
                    }

                    if (font == null)
                        font = marcEditor.Font;

                    Debug.Assert(font != null, "");


                    // System.Drawing.Text.TextRenderingHint oldrenderhint = g.TextRenderingHint;
                    // g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                    if (nCol == 0)  // 字段名提示
                    {
                        /*
                        StringFormat format = StringFormat.GenericDefault; //new StringFormat();
                        g.DrawString(strText,
                            font,
                            brush,	// System.Drawing.Brushes.Blue,
                            textRect,
                            format);
                         */

                        Color textcolor = marcEditor.defaultNameCaptionTextColor;

                        if (selected == true)
                        {
                            // textcolor = marcEditor.defaultSelectedTextColor;

                            textcolor = ReverseColor(textcolor);
                        }

                        TextRenderer.DrawText(
                            g,
                            strText,
                            font,
                            textRect,
                            textcolor,
                            TextFormatFlags.EndEllipsis);

                    }
                    else if (nCol == 1)    // 字段名
                    {
                        Color textcolor = marcEditor.defaultNameTextColor;
                        if (selected)
                            textcolor = marcEditor.defaultSelectedTextColor;

                        TextRenderer.DrawText(
                            g,
                            strText,
                            font,
                            textRect,
                            textcolor,
                            MarcEditor.editflags);  // TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                    }
                    else if (nCol == 2)    // 指示符
                    {
                        Color textcolor = marcEditor.defaultIndicatorTextColor;
                        if (selected)
                            textcolor = marcEditor.defaultSelectedTextColor;

                        TextRenderer.DrawText(
                            g,
                            strText,
                            font,
                            textRect,
                            textcolor,
                            MarcEditor.editflags);  // TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
                    }
                    else
                    {   // 内容
                        Color textcolor = marcEditor.m_contentTextColor;
                        if (selected)
                            textcolor = marcEditor.defaultSelectedTextColor;

#if BIDI_SUPPORT
                        // strText = strText.Replace(new string(Record.KERNEL_SUBFLD, 1), "\x200e" + new string(Record.KERNEL_SUBFLD, 1));
                        strText = MarcEditor.AddBidi(strText);
#endif

                        TextRenderer.DrawText(
                            g,
                            strText,
                            font,
                            textRect,
                            textcolor,
                            MarcEditor.editflags);  // TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

#if REMOVED
                        int flags = DT_CALCRECT | DT_WORDBREAK | DT_EDITCONTROL;
                        IntPtr dc = g.GetHdc();
                        try
                        {
                            Rect drawTextRC = new Rect(textRect);
                            int height = DrawText(dc, 
                                strText,
                                strText.Length,
                                ref drawTextRC,
                                flags);
                        }
                        finally
                        {
                            g.ReleaseHdc(dc);
                        }
#endif
                    }
                }
            }
            finally
            {
                if (brush != null)
                    brush.Dispose();
            }

#if NO
            // 2015/10/19
            if (linGrBrush != null)
                linGrBrush.Dispose();
#endif
        }

        // 获得反相颜色
        static Color ReverseColor(Color color)
        {
            return Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B);
        }

        // 画线条
        // 上、下、左、右 四根线条
        internal static void DrawLines(Graphics g,
            Rectangle myRect,
            int nTopBorderHeight,
            int nBottomBorderHeight,
            int nLeftBorderWidth,
            int nRightBorderWidth,
            Color color)
        {
            if (nTopBorderHeight < 0
                || nBottomBorderHeight < 0
                || nLeftBorderWidth < 0
                || nRightBorderWidth < 0)
                return;

            if (nTopBorderHeight > myRect.Height
                || nBottomBorderHeight > myRect.Height)
                return;

            if (nLeftBorderWidth > myRect.Width
                || nRightBorderWidth > myRect.Width)
                return;

            //左边垂直钢笔
            using (Pen penLeft = new Pen(color, nLeftBorderWidth))
            //右边垂直钢笔
            using (Pen penRight = new Pen(color, nRightBorderWidth))
            //上方的水平钢笔
            using (Pen penTop = new Pen(color, nTopBorderHeight))
            //下方的水平钢笔
            using (Pen penBottom = new Pen(color, nBottomBorderHeight))
            {
                int nLeftDelta = nLeftBorderWidth / 2;
                int nRightDelta = nRightBorderWidth / 2;
                int nTopDelta = nTopBorderHeight / 2;
                int nBottomDelta = nBottomBorderHeight / 2;

                int nLeftMode = nLeftBorderWidth % 2;
                int nRightMode = nRightBorderWidth % 2;
                int nTopMode = nTopBorderHeight % 2;
                int nBottomMode = nBottomBorderHeight % 2;

                Rectangle rectMiddle = new Rectangle(0, 0, 0, 0);
                if (nTopBorderHeight == 0
                    && nBottomBorderHeight == 0
                    && nLeftBorderWidth == 0)
                {
                    rectMiddle = new Rectangle(
                        myRect.X,
                        myRect.Y,
                        myRect.Width - nRightDelta,
                        myRect.Height);
                }
                else if (nLeftBorderWidth == 0
                    && nRightBorderWidth == 0
                    && nTopBorderHeight == 0)
                {
                    rectMiddle = new Rectangle(
                        myRect.X,
                        myRect.Y,
                        myRect.Width,
                        myRect.Height - nBottomDelta);
                }
                else
                {
                    rectMiddle = new Rectangle(
                        myRect.X + nLeftDelta,
                        myRect.Y + nTopDelta,
                        myRect.Width - nLeftDelta - nRightDelta,
                        myRect.Height - nTopDelta - nBottomDelta);

                }

                //上方
                if (nTopBorderHeight > 0)
                {
                    if (nLeftBorderWidth == 0
                        && nRightBorderWidth == 0
                        && nBottomBorderHeight == 0)
                    {
                        if (nTopBorderHeight == 1)
                        {
                            g.DrawLine(penTop,
                                rectMiddle.Left, rectMiddle.Top,
                                rectMiddle.Right, rectMiddle.Top);
                        }
                        else
                        {
                            g.DrawLine(penTop,
                                rectMiddle.Left, rectMiddle.Top,
                                rectMiddle.Right + 1, rectMiddle.Top);
                        }
                    }
                    else
                    {
                        g.DrawLine(penTop,
                            rectMiddle.Left, rectMiddle.Top,
                            rectMiddle.Right, rectMiddle.Top);
                    }
                }

                //下方
                if (nBottomBorderHeight > 0)
                {
                    if (nLeftBorderWidth == 0
                        && nRightBorderWidth == 0
                        && nTopBorderHeight == 0)
                    {
                        if (nBottomBorderHeight == 1)
                        {
                            g.DrawLine(penBottom,
                                rectMiddle.Left, rectMiddle.Bottom,
                                rectMiddle.Right - 1, rectMiddle.Bottom);
                        }
                        else
                        {
                            g.DrawLine(penBottom,
                                rectMiddle.Left, rectMiddle.Bottom - nBottomMode,
                                rectMiddle.Right, rectMiddle.Bottom - nBottomMode);
                        }
                    }
                    else
                    {
                        g.DrawLine(penBottom,
                            rectMiddle.Left, rectMiddle.Bottom,
                            rectMiddle.Right, rectMiddle.Bottom);
                    }
                }

                int nLeftTemp = nLeftDelta + nLeftMode;
                if (nLeftBorderWidth == 1)
                {
                    if (nLeftMode == 0)
                        nLeftTemp = nLeftDelta - 1;
                    else
                        nLeftTemp = nLeftDelta;
                }
                //左方
                if (nLeftBorderWidth > 0)
                {
                    if (nTopBorderHeight == 0
                        && nBottomBorderHeight == 0
                        && nRightBorderWidth == 0)
                    {
                        if (nLeftBorderWidth == 1)
                        {
                            g.DrawLine(penRight,
                                rectMiddle.Left, rectMiddle.Top - nLeftDelta,
                                rectMiddle.Left, rectMiddle.Bottom);
                        }
                        else
                        {
                            g.DrawLine(penLeft,
                                rectMiddle.Left, rectMiddle.Top,
                                rectMiddle.Left, rectMiddle.Bottom + 1);
                        }
                    }
                    else
                    {
                        g.DrawLine(penLeft,
                            rectMiddle.Left, rectMiddle.Top,
                            rectMiddle.Left, rectMiddle.Bottom);
                    }
                }

                int nRightTemp = nRightDelta + nRightMode;
                if (nRightBorderWidth == 1)
                {
                    if (nRightMode == 0)
                        nRightTemp = nRightDelta - 1;
                    else
                        nRightTemp = nRightDelta;
                }
                //右方
                if (nRightBorderWidth > 0)
                {
                    if (nTopBorderHeight == 0
                        && nBottomBorderHeight == 0
                        && nLeftBorderWidth == 0)
                    {
                        if (nRightBorderWidth == 1)
                        {
                            g.DrawLine(penRight,
                                rectMiddle.Right, rectMiddle.Top - nRightDelta,
                                rectMiddle.Right, rectMiddle.Bottom - 1);
                        }
                        else
                        {
                            g.DrawLine(penRight,
                                rectMiddle.Right - nRightMode, rectMiddle.Top,
                                rectMiddle.Right - nRightMode, rectMiddle.Bottom);
                        }
                    }
                    else
                    {
                        g.DrawLine(penRight,
                            rectMiddle.Right, rectMiddle.Top - nRightDelta,
                            rectMiddle.Right, rectMiddle.Bottom + nRightTemp);
                    }
                }

            }
#if NO
			penLeft.Dispose ();
			penRight.Dispose ();
			penTop.Dispose ();
			penBottom.Dispose ();
#endif
        }

        // 子字段集合
        // 通过get得到的集合，remove其中的subfield对象，field中不能兑现。
        // 需要set那个remove后的集合回来，才能兑现
        /// <summary>
        /// 获取或设置子字段对象集合
        /// </summary>
		public SubfieldCollection Subfields
        {
            get
            {
                if (Record.IsControlFieldName(this.m_strName) == true)
                    return null;
                return SubfieldCollection.BuildSubfields(this);
            }
            set
            {
                if (value != null)
                {
                    value.Container = this;

                    value.Flush();  // Flush()中必定作了针对this的事情
                }
            }
        }

#if REMOVED
        #region DrawText

        [DllImport("coredll.dll")]
        static extern int DrawText(IntPtr hdc, string lpStr, int nCount, ref Rect lpRect, int wFormat);

        private const int DT_CALCRECT = 0x00000400;
        private const int DT_WORDBREAK = 0x00000010;
        private const int DT_EDITCONTROL = 0x00002000;

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public Rect(Rectangle r)
            {
                this.Left = r.Left;
                this.Top = r.Top;
                this.Bottom = r.Bottom;
                this.Right = r.Right;
            }
        }

        #endregion
#endif
    }
}
