using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml;

using DigitalPlatform.Text;
using System.Text;
using System.Linq;

namespace DigitalPlatform.Marc
{
    // 记录
    /// <summary>
    /// MARC 记录对象
    /// </summary>
    public class Record
    {
        /// <summary>
        /// 字段结束符 字符
        /// </summary>
		public const char FLDEND = (char)30;	// 字段结束符
        /// <summary>
        /// 记录结束符 字符
        /// </summary>
		public const char RECEND = (char)29;	// 记录结束符
        /// <summary>
        /// 子字段符号 字符
        /// </summary>
		public const char SUBFLD = (char)31;	// 子字段指示符

        /// <summary>
        /// 子字段符号内部代用字符
        /// </summary>
        public const char KERNEL_SUBFLD = '▼';	// '‡';  子字段指示符内部代用符号

        /// <summary>
        /// 本记录所从属的 MARC 编辑器
        /// </summary>
		public MarcEditor marcEditor = null;

        /// <summary>
        /// 本记录下属的字段集合
        /// </summary>
		public FieldCollection Fields = new FieldCollection();

        /// <summary>
        /// 字段名显示的宽度。像素数
        /// </summary>
		public int NamePureWidth = 30;	// 字段名的宽度
        /// <summary>
        /// 字段名显示的高度。像素数
        /// </summary>
        public int NamePureHeight = 16;

        /// <summary>
        /// 字段指示符显示宽度。像素数
        /// </summary>
		public int IndicatorPureWidth = 25;// 字段指示符
        /// <summary>
        /// 字段指示符显示高度。像素数
        /// </summary>
        public int IndicatorPureHeight = 16;

        //public int ValuePureWidth = 100;	//字段值的宽度
        /// <summary>
        /// 字段名说明文字部分的显示宽度。像素数
        /// </summary>
        public int NameCaptionPureWidth = 100;  //字段名的说明

        /// <summary>
        /// 线条宽度。像素数
        /// </summary>
		public int GridVertLineWidth = 1;	// 线条宽度，用于画左右的线条
        /// <summary>
        /// 分割线条宽度。像素数
        /// </summary>
		public int GridVertLineWidthForSplit = 3; // 用来说明与字段的分界

        /// <summary>
        /// 线条高度。像素数
        /// </summary>
		public int GridHorzLineHeight = 1;	// 线条高度，用于画上下的线条

        /// <summary>
        /// 单元的顶部空白。像素数
        /// </summary>
		public int CellTopBlank = 2;
        /// <summary>
        /// 单元的左边空白。像素数
        /// </summary>
		public int CellLeftBlank = 3;
        /// <summary>
        /// 单元的底部空白。像素数
        /// </summary>
		public int CellBottomBlank = 2;
        /// <summary>
        /// 单元的右边空白。像素数
        /// </summary>
		public int CellRightBlank = 3;

        // 2024/7/7
        // 各个单元之间的水平间隔
        public int CellBlanking = 3;

        /// <summary>
        /// 平均行高度。像素数
        /// </summary>
		public int AverageLineHeight = 20;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="editor">MARC 编辑器</param>
		public Record(MarcEditor editor)
        {
            this.marcEditor = editor;
            this.Fields.record = this;

            // 初始化各区域宽度
            this.InitializeWidth();
        }


        // 值的纯净宽度，界面剩余宽度都宽值的宽度
        internal int ValuePureWidth
        {
            get
            {
                int nWidth = this.marcEditor.ClientWidth
                    - this.marcEditor.LeftBlank
                    - this.marcEditor.RightBlank
                    - this.NameCaptionTotalWidth
                    - this.NameTotalWidth
                    - this.IndicatorTotalWidth
                    - this.GridVertLineWidth
                    - this.CellLeftBlank
                    - this.CellRightBlank
                    - this.GridVertLineWidth;
                if (nWidth <= 0)
                    nWidth = 100;
                return nWidth;
            }
        }

        // Name总共的宽度
        internal int NameTotalWidth
        {
            get
            {
                return this.GridVertLineWidthForSplit/*this.GridVertLineWidth */
                    + this.CellLeftBlank
                    + this.NamePureWidth
                    + this.CellRightBlank
                    + this.CellBlanking;
            }
        }

        // Name说明总共的宽度
        internal int NameCaptionTotalWidth
        {
            get
            {
                return this.GridVertLineWidth
                    + this.CellLeftBlank
                    + this.NameCaptionPureWidth
                    + this.CellRightBlank;
            }
        }

        // Indicator总共的宽度
        internal int IndicatorTotalWidth
        {
            get
            {
                return this.GridVertLineWidth
                    + this.CellLeftBlank
                    + this.IndicatorPureWidth
                    + this.CellRightBlank
                    + this.CellBlanking;
            }
        }

        // Value总共的宽度
        internal int ValueTotalWidth
        {
            get
            {
                return this.GridVertLineWidth
                    + this.CellLeftBlank
                    + this.ValuePureWidth
                    + this.CellRightBlank;
            }
        }

        // 总宽度，不包括最右边的线条
        internal int TotalLineWidth
        {
            get
            {
                return this.NameCaptionTotalWidth
                    + this.NameTotalWidth
                    + this.IndicatorTotalWidth
                    //+ this.Indicator2TotalWidth
                    + this.ValueTotalWidth;
            }
        }

        // 记录总宽度，包括最右边的线条
        internal int Width
        {
            get
            {
                return TotalLineWidth + this.GridVertLineWidth;
            }
        }

        // 记录总高度，包括最下边的线条
        internal int Height
        {
            get
            {
                return this.GetFieldsHeight(0, -1) + this.GridHorzLineHeight;
            }
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2catalog 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentException
Message: 无法找到字体“Courier New”。
Stack:
在 System.Drawing.FontFamily.CreateFontFamily(String name, FontCollection fontCollection)
在 System.Drawing.FontFamily..ctor(String name)
在 DigitalPlatform.Marc.MarcEditor.get_FixedSizeFont()
在 DigitalPlatform.Marc.Record.InitializeWidth()
在 DigitalPlatform.Marc.Record..ctor(MarcEditor editor)
在 DigitalPlatform.Marc.MarcEditor.OnHandleCreated(EventArgs e)
在 System.Windows.Forms.Control.WmCreate(Message& m)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Catalog 版本: dp2Catalog, Version=2.4.5711.37216, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/22 9:55:03 (Sat, 22 Aug 2015 09:55:03 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
 
         * */
        // 初始化各区域宽度
        internal void InitializeWidth()
        {
            using (Graphics g = Graphics.FromHwnd(this.marcEditor.Handle))
            {
                string strTempName = "###";
                // Font font = this.marcEditor.Font;//.DefaultTextFont;

                /*
                if (this.marcEditor.m_fixedSizeFont == null)
                    this.marcEditor.m_fixedSizeFont = this.marcEditor.CreateFixedSizeFont();
                 * */

                Font fixedfont = null;

                try
                {
                    fixedfont = this.marcEditor.FixedSizeFont;  // 在有的 Windows XP 操作系统下无法找到 Courier New 字体，这一句会抛出异常
                }
                catch
                {
                    fixedfont = this.marcEditor.Font;
                }

                Size size = MeasureSize(g,
                    fixedfont,
                    strTempName);
                this.NamePureWidth = size.Width + 1;
                this.NamePureHeight = size.Height;

                string strTempIndicator = "99";
                size = MeasureSize(g,
                    fixedfont,
                    strTempIndicator);
                this.IndicatorPureWidth = size.Width + 1;
                this.IndicatorPureHeight = size.Height;
            }
        }

        // 根据Font，得到字符串的宽度
        // parameter:
        //		g	Graphics对象
        //		font	Font对象
        //		strText	字符串
        // return:
        //		字符串的宽度
        /*public*/
        static Size MeasureSize(Graphics g,
 Font font,
 string strText)
        {
#if BIDI_SUPPORT
            // strText = strText.Replace(new string(Record.KERNEL_SUBFLD, 1), "\x200e" + new string(Record.KERNEL_SUBFLD, 1));
            strText = MarcEditor.AddBidi(strText);
#endif
            return TextRenderer.MeasureText(g,
                strText,
                font,
                new Size(1000, 1000),
                MarcEditor.editflags);
        }

        // 获得指定行数的高数
        // parameter:
        //		nStart	起始行号
        //		nLength	长度 -1:表示从nStart到末尾
        // return:
        //		-1	出错
        //		>=0 指定行的高度 
        // 注意该函数返回的行高不包括最下面的线条
        internal int GetFieldsHeight(int nStart,
            int nLength)
        {
            int nResultHeight = 0;

            int nOutputLength;
            string strError;
            int nRet = GetRealLength(nStart,
                nLength,
                this.Fields.Count,
                -1,
                out nOutputLength,
                out strError);
            if (nRet == -1)
            {
                Debug.Assert(false, strError);
                throw new Exception(strError);
                //return -1;
            }

            for (int i = nStart; i < nStart + nOutputLength; i++)
            {
                Field field = this.Fields[i];
                nResultHeight += field.TotalHeight;
            }

            return nResultHeight;
        }

        // 计算指定字段的高度
        // parameters:
        //		nStart	起始行号
        //		nLength	长度 -1: 表示从nStart到末尾
        //		bIgnoreEdit	是否忽略小edit控件 false不忽略
        // return:
        //		-1	出错
        //		0	成功
        internal void CalculateFieldsHeight(int nStart,
            int nLength,
            bool bIgnoreEdit)
        {
            int nOutputLength;
            string strError;
            int nRet = GetRealLength(nStart,
                nLength,
                this.Fields.Count,
                -1,
                out nOutputLength,
                out strError);
            if (nRet == -1)
            {
                Debug.Assert(false, strError);
                throw new Exception(strError);
                //return -1;
            }

            for (int i = nStart; i < nStart + nOutputLength; i++)
            {
                Field field = this.Fields[i];
                field.CalculateHeight(null, bIgnoreEdit);
            }
        }

        // 检索范围是否合法,并返回真正能够取的长度
        // parameter:
        //		nStart          起始位置
        //		nNeedLength     需要的长度
        //		nTotalLength    数据实际总长度
        //		nMaxLength      限制的最大长度
        //		nOutputLength   out参数,返回的可以用的长度
        //		strError        out参数,返回出错信息
        // return:
        //		-1  出错
        //		0   成功
        internal static int GetRealLength(int nStart,
            int nNeedLength,
            int nTotalLength,
            int nMaxLength,
            out int nOutputLength,
            out string strError)
        {
            nOutputLength = 0;
            strError = "";

            // 起始值,或者总长度不合法
            if (nStart < 0
                || nTotalLength < 0)
            {
                strError = "范围错误:nStart < 0 或 nTotalLength <0 \r\n";
                return -1;
            }
            if (nStart != 0
                && nStart >= nTotalLength)
            {
                strError = "范围错误:起始值大于总长度\r\n";
                return -1;
            }

            nOutputLength = nNeedLength;
            if (nOutputLength == 0)
            {
                return 0;
            }

            if (nOutputLength == -1)  // 从开始到全部
                nOutputLength = nTotalLength - nStart;

            if (nStart + nOutputLength > nTotalLength)
                nOutputLength = nTotalLength - nStart;

            // 限制了最大长度
            if (nMaxLength != -1 && nMaxLength >= 0)
            {
                if (nOutputLength > nMaxLength)
                    nOutputLength = nMaxLength;
            }
            return 0;
        }

        // 看一个字段名是否是头标区。头标区没有字段名和指示符
        // parameters:
        //		strFieldName	字段名
        // return:
        //		true	是控制字段
        //		false	不是控制字段
        /// <summary>
        /// 检查一个字段名是否为表示头标区的特殊字段名。"hdr" 和 "###" 是系统内部用来表示头标区的两个特殊字段名
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <returns>是否特殊字段名</returns>
        public static bool IsHeaderFieldName(string strFieldName)
        {
            if (String.Compare(strFieldName, "hdr", true) == 0)
                return true;

            if (String.Compare(strFieldName, "###", true) == 0)
                return true;

            return false;
        }

        // ### 只能用作第一个头标区的名字。hdr 可以用作多个头标区字段名字
        public static bool IsFirstHeaderFieldName(string strFieldName)
        {
            if (String.Compare(strFieldName, "###", true) == 0)
                return true;

            return false;
        }

        // 看一个字段名是否是控制字段。所谓控制字段没有指示符概念
        // parameters:
        //		strFieldName	字段名
        // return:
        //		true	是控制字段
        //		false	不是控制字段
        /// <summary>
        /// 检查一个字段名是否是控制字段的字段名。控制字段就是没有字段指示符的那些特殊字段
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <returns>是否控制字段</returns>
        public static bool IsControlFieldName(string strFieldName)
        {
            if (String.Compare(strFieldName, "hdr", true) == 0)
                return true;

            if (String.Compare(strFieldName, "###", true) == 0)
                return true;

            if (
                (
                String.Compare(strFieldName, "001") >= 0
                && String.Compare(strFieldName, "009") <= 0
                )

                || String.Compare(strFieldName, "-01") == 0
                || strFieldName == "FMT"    // 2024/3/8
                )
                return true;

            return false;
        }


        // 将一个Marc机内格式，变成一个字段数组
        // parameter:
        //		strMarc	Marc数据
        //		fields	out参数,返回字段数组
        // return:
        //		void
        /// <summary>
        /// 根据一个 MARC 机内格式字符串获得一个 字段内容的数组
        /// </summary>
        /// <param name="strMarc">MARC 机内格式字符串</param>
        /// <param name="fields">返回字符串数组。每个字符串表示一个字段</param>
        internal static void GetMarcFields(string strMarc,
            out List<string> fields)
        {
            fields = new List<string>();

            if (String.IsNullOrEmpty(strMarc) == true)
                return;

            // List<string> aField = new List<string>();
            string strText = strMarc;

            string strField = "";

            if (strText.Length < 24)
                strText = strText + new string(' ', 24 - strText.Length);
            strField = strText.Substring(0, 24);
            fields.Add(strField);

            strText = strText.Substring(24);

            List<string> restFields = Record.GetFields(strText, out _);

            // 这里的预先处理头标区，说明了GetFields()函数的不合理 2009/3/5

            fields.AddRange(restFields);
        }


        // 依据字段结束符切割出表示字段的字符串数组
        // 注意，如果切割出来的第一个元素用于插入头标区，则应当理解为，如果它的长度大于24字符，则要把剩余的部分当作一个新的字段看待 2009/3/5
        // 空字符串也算做一个字段??
        // parameters:
        //      contains_field_end  [out] 是否包含至少一个字段结束符？
        internal static List<string> GetFields(string strText,
            out bool contains_field_end)
        {
            contains_field_end = false;

            List<string> fields = new List<string>();

            if (String.IsNullOrEmpty(strText) == true)
                return fields;

            string[] tempfields = strText.Split(new char[] { Record.FLDEND });
            if (tempfields.Length > 0)
            {
                // 如果末尾是一个空的字符串，则要甩掉它
                if (String.IsNullOrEmpty(tempfields[tempfields.Length - 1]) == true)
                {
                    for (int i = 0; i < tempfields.Length - 1; i++)
                    {
                        fields.Add(tempfields[i]);
                    }
                    contains_field_end = true;
                }
                else
                {
                    fields.AddRange(tempfields);
                    // fields = tempfields;
                }
            }

            if (fields.Count > 1)
                contains_field_end = true;
            return fields;
        }

        // 得到Marc字符串
        // 如果有不合法的怎么办,Debug.Assert，是不可能的情况
        // return:
        //		Marc记录 机内格式
        internal string GetMarc()
        {
            this.marcEditor.Flush();

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < this.Fields.Count; i++)
            {
                Field field = this.Fields[i];
                text.Append(field.GetFieldMarc(true));
            }
            var strMarc = text.ToString();
            Debug.Assert(strMarc.IndexOf((char)0x200e) == -1, "");
            return strMarc;
        }

        // 设Marc数据设到内存对象中
        // 操作历史: 不负责添加操作历史
        // parameters:
        //		strMarc	Marc记录 机内格式
        internal int SetMarc(string strMarc,
            bool bCheckMarcDef,
            out string strError)
        {
            strError = "";

            bool bHasFocus = this.marcEditor.Focused;

            if (strMarc == null)
                strMarc = "";

            if (string.IsNullOrEmpty(strMarc))
                strMarc = MarcEditor.default_marc;
            if (strMarc.Length < 24)
                strMarc = strMarc + new string('?', 24 - strMarc.Length);

            // 清空原来的内存对象
            this.marcEditor.ClearSelectFieldIndices();
            this.Fields.Clear();

            /*
        if (bCheckMarcDef == true && this.marcEditor.MarcDefDom == null)
        {
            GetConfigFileEventArgs ar = new GetConfigFileEventArgs();
            ar.Path = "marcdef";
            ar.Stream = null;

            this.marcEditor.OnGetConfigFile(ar);
            if (ar.ErrorInfo != "")
            {
                strError = "获取marcdef出错，原因:" + ar.ErrorInfo;
                return -1;
            }
            if (ar.Stream != null)
            {
                ar.Stream.Seek(0, SeekOrigin.Begin);
                this.marcEditor.MarcDefDom = new XmlDocument();
                try
                {
                    this.marcEditor.MarcDefDom.Load(ar.Stream);
                }
                catch(Exception ex )
                {
                    this.marcEditor.MarcDefDom = null;
                    strError = "加载marcdef配置文件到dom时出错：" + ex.Message;
                    return -1;
                }
                ar.Stream.Close();
            }
        }
             * 
             * */

            strMarc = strMarc.Replace(Record.SUBFLD, Record.KERNEL_SUBFLD);
            Record.GetMarcFields(strMarc,
                out List<string> fields);
            if (fields == null)
                return 0;

            // TODO: 在每个子字段符号前插入\x200e符号
            for (int i = 0; i < fields.Count; i++)
            {
                string strText = fields[i];

                int nOutputPosition;

                string strName = "";
                string strIndicator = "";
                string strValue = "";
                if (i == 0) // 取头标区
                {
                    strValue = strText;
                    if (strValue.Length != 24)
                    {
                        if (strValue.Length > 24)
                            strValue = strText.Substring(0, 24);
                        else
                            strValue = strText + new string(' ', 24 - strText.Length);
                    }
                    this.Fields.AddInternal("###",
                        "",
                        strValue,
                        false, //bFireTextChanged
                        false, //bInOrder
                        out nOutputPosition);
                    continue;
                }

                if (strText.Length < 3)
                    strText = strText + new string(' ', 3 - strText.Length);

                strName = strText.Substring(0, 3);
                if (IsControlFieldName(strName) == true)
                {
                    strIndicator = "";
                    strValue = strText.Substring(3);
                }
                else
                {
                    if (strText.Length < 5)
                        strText = strText + new string(' ', 5 - strText.Length);

                    strIndicator = strText.Substring(3, 2);
                    strValue = strText.Substring(5);
                }
                // 可以考虑把字段加好了再统计计算显示页面
                this.Fields.AddInternal(strName,
                    strIndicator,
                    strValue,
                    false,  //bFireTextChanged
                    false,  //bInOrder
                    out nOutputPosition);
            }

            // 总体触发一次TextChnaged事件
            this.marcEditor.FireTextChanged();

            this.marcEditor.InitialFonts();

            // 设第一个节点为当前活动焦点
            if (bHasFocus == true)
            {
                if (this.Fields.Count > 0)
                    this.marcEditor.SetActiveField(0, 3, true);
            }
            else
            {
                /*
                if (this.Fields.Count > 0)
                    this.marcEditor.SetActiveField(0, 3, false);
                 * */
            }

            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = true;
            this.marcEditor.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);

            return 0;
        }

        internal void RefreshNameCaption()
        {
            // 循环行
            for (int i = 0; i < this.Fields.Count; i++)
            {
                Field field = this.Fields[i];
                field.RefreshNameCaption();
            }
        }

        // 绘制
        internal void Paint(PaintEventArgs pe,
            int nBaseX,
            int nBaseY,
            int focus_line)
        {
            // 2019/4/28
            if (this.Fields == null || this.marcEditor == null)
                return;

            if (this.Fields.Count == 0)
                return;

            int x = nBaseX;
            int y = nBaseY;

            var fields = new List<Field>(this.Fields);

            // 循环行
            int i = 0;
            foreach (var field in fields) // this.Fields
            {
                // Field current_field = this.Fields[i];

                // 优化
                if (y > pe.ClipRectangle.Y + pe.ClipRectangle.Height)
                    break;

                bool focus_on = focus_line == i;
                // 调Field的Paint函数
                field.Paint(pe, x, y, focus_on);

                // 绘制 focus 行虚框
                if (focus_on)
                {
                    var height = field.TotalHeight;
                    var width = this.NameCaptionTotalWidth + this.NameTotalWidth + this.IndicatorTotalWidth + this.ValueTotalWidth;
                    using (Pen pen = new Pen(this.marcEditor.m_contentTextColor, (float)1))
                    {
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        pe.Graphics.DrawRectangle(pen,
            new Rectangle(x, y, width - 1, height - 1));
                    }
                }

                y += field.TotalHeight;

                i++;
            }

            // 绘共同的右下的线////////////
            Rectangle rect = new Rectangle(nBaseX,
                nBaseY,
                this.Width,
                this.Height);

            // 调整
            if (this.GridVertLineWidth == 1
                && this.GridHorzLineHeight == 1)
            {
                rect.Height = rect.Height - 1;
            }
            // 下
            Field.DrawLines(pe.Graphics,
                rect,
                0,
                this.GridHorzLineHeight,
                0,
                0,
                this.marcEditor.defaultHorzGridColor);

            // 调整
            if (this.GridVertLineWidth == 1
                && this.GridHorzLineHeight == 1)
            {
                rect.Height = rect.Height + 1;
            }
            if (this.GridVertLineWidth == 1
                || this.GridHorzLineHeight == 1)
            {
                rect.Width = rect.Width - 1;
            }
            // 右
            Field.DrawLines(pe.Graphics,
                rect,
                0,
                0,
                0,
                this.GridVertLineWidth,
                this.marcEditor.defaultVertGridColor);
        }

        public string GetFieldLocationString(int index)
        {
            if (index >= this.Fields.Count)
                throw new ArgumentException($"index={index} 超过当前许可的字段集合下标范围");
            string name = this.Fields[index].Name;
            int name_count = 0; // 相同字段名的计数
            int i = 0;
            foreach (var field in this.Fields)
            {
                if (i == index)
                    return name + ":" + name_count.ToString();
                if (field.Name == name)
                    name_count++;
                i++;
            }

            return name;
        }

        // parameters:
        //      field_index 字段下标
        //      caret_offs  当前插入符在字段内容中的下标
        public string GetSubfieldLocationString(int field_index,
            int caret_offs)
        {
            var field = this.Fields[field_index];
            var field_location = GetFieldLocationString(field_index);

            var content_text = this.marcEditor.curEdit.Text;
            // return:
            //      0   不在子字段上
            //      1   在子字段上
            var ret = MyEdit.GetCurrentSubfieldCaretInfo(
                content_text,
                caret_offs,
                out string strSubfieldName,
                out string strSufieldContent,
                out int nStart,
                out int nContentStart,
                out int nContentLength,
                out bool forbidden);
            if (ret == 0)
                return field_location;

            MarcField marc_field = new MarcField(field.m_strName,
                field.m_strIndicator,
                content_text.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD));

            int name_count = -1; // 相同字段名的计数
            int offs = 0;
            foreach (MarcSubfield subfield in marc_field.select("subfield"))
            {
                if (offs >= nContentStart
                    || offs >= nStart)
                {
                    if (name_count <= 0)
                        return field_location + "/" + strSubfieldName;
                    return field_location + "/" + strSubfieldName + ":" + name_count.ToString();
                }
                if (subfield.Name == strSubfieldName)
                    name_count++;
                offs += subfield.Text.Length;
            }

            return field_location + "/" + strSubfieldName;
        }

        // 根据定位字符串，返回字段 index，列号，插入符位置(在本区域内)
        // parameters:
        //      caret_pos   [out] 返回在 col 区域中的插入符位置。注意如果 BIDI 支持模式下，这是考虑 curEdit.Text 中加入了方向字符以后的插入符位置
        public void GetLocation(string locationString,
            out int field_index,
            out int col,
            out int caret_pos)
        {
            field_index = -1;
            col = -1;
            caret_pos = -1;

            var levels = locationString.Split('/');
            if (levels.Length > 2)
                throw new ArgumentException("GetLocation() 暂不能处理级别超过两级的情况");
            if (levels.Length == 0
                || (levels.Length == 1 && (levels[0] == "hdr" || levels[0] == "###")))
            {
                // 定位到头标区
                field_index = 0;
                col = 3;
                return;
            }

            int i = 0;
            foreach (var level in levels)
            {
                var parts = StringUtil.ParseTwoPart(level, ":");
                var name = parts[0];
                var count_string = parts[1];

                int count = 0;
                if (string.IsNullOrEmpty(count_string) == false)
                {
                    if (Int32.TryParse(count_string, out count) == false)
                        throw new ArgumentException($"级别 '{level}' 中重复次数部分格式不合法");
                }

                if (i == 0)
                {
                    // 第一级出现 ###，当作选中头标区
                    if (name == "###" || name == "hdr")
                    {
                        // 定位到头标区
                        field_index = 0;
                        col = 3;
                        return;
                    }

                    var ret = LocateFieldIndex(name, count);
                    if (ret == -1)
                        return;
                    field_index = ret;
                }
                else if (i == 1)
                {
                    // 定位 col 和 caret_pos
                    string field_content = this.Fields[field_index].m_strValue;
                    col = 3;
                    caret_pos = LocationCaretPos(field_content,
    name,
    count);
                    if (caret_pos == -1)
                        caret_pos = 0;
                    return;
                }


                i++;
            }
        }

        public static int LocationCaretPos(string field_content,
            string subfield_name,
            int dup_count)
        {
#if BIDI_SUPPORT
            field_content = MarcEditor.AddBidi(field_content);
#endif
            var KERNEL_SUBFIELD = new string(Record.KERNEL_SUBFLD, 1);
            field_content = field_content.Replace(KERNEL_SUBFIELD, (char)0x01 + KERNEL_SUBFIELD);
            var parts = field_content.Split((char)0x01);
            int start = 0;
            int dup = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                string current_subfield_name = "";
                if (part.Length >= 2 && part[0] == Record.KERNEL_SUBFLD)
                    current_subfield_name = part.Substring(1, 1);

                // 跳过不是子字段的一段文字
                if (string.IsNullOrEmpty(current_subfield_name))
                {
                    start += part.Length;
                    continue;
                }
                string current_subfield_content = "";
                if (part.Length >= 3)
                    current_subfield_content = part.Substring(2);

                if (current_subfield_name == subfield_name)
                {
                    if (dup_count == dup)
                        return start + current_subfield_name.Length * 2;
                    dup++;
                }

                start += part.Length;
            }

            return -1;
        }

        public int LocateFieldIndex(string name,
            int dup_count)
        {
            var hits = this.Fields.ToList().Where(o => o.m_strName == name).ToList();
            if (dup_count == 0)
            {
                if (hits.Count == 0)
                    return -1;
                var field = hits[0];
                return this.Fields.IndexOf(field);
            }
            if (hits.Count <= dup_count)
                return -1;
            return this.Fields.IndexOf(hits[dup_count]);
        }

        // parameters:
        //      col 列号。
        //          0 1 返回字段头部(字段名第一字符)的偏移
        //          2 返回字段指示符第一字符的偏移
        //          3 返回字段内容第一字符的偏移
        public int GetFieldOffs(Field field, int col)
        {
            int start = 0;
            for (int i = 0; i < this.Fields.Count; i++)
            {
                var current_field = this.Fields[i];
                if (current_field == field)
                {
                    if (col == 0 || col == 1)
                        return start;
                    else if (col == 2)
                        return start + current_field.m_strName.Length;
                    else if (col == 3)
                        return start + current_field.m_strName.Length + current_field.m_strIndicator.Length;
                    else
                        throw new ArgumentException($"col 值应为 0 1 2 3 之一");
                }
                start += current_field.m_strName.Length
                    + current_field.m_strIndicator.Length
                    + current_field.m_strValue.Length;
            }

            return -1;  // not found
        }

        // 获得一个字段首字符在机内格式记录中的偏移
        // parameters:
        //      col 列号。
        //          0 1 返回字段头部(字段名第一字符)的偏移
        //          2 返回字段指示符第一字符的偏移
        //          3 返回字段内容第一字符的偏移
        public int GetFieldOffs(int field_index, int col)
        {
            int start = 0;
            for (int i = 0; i < this.Fields.Count; i++)
            {
                var current_field = this.Fields[i];
                if (i == field_index)
                {

                    if (col == 0 || col == 1)
                        return start;
                    else if (col == 2)
                        return start + current_field.m_strName.Length;
                    else if (col == 3)
                        return start + current_field.m_strName.Length + current_field.m_strIndicator.Length;
                    else
                        throw new ArgumentException($"col 值应为 0 1 2 3 之一");
                }
                start += current_field.m_strName.Length
                    + current_field.m_strIndicator.Length
                    + current_field.m_strValue.Length;
            }

            return -1;  // not found
        }

    }
}
