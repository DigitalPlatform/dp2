using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using LibraryStudio.Forms;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static LibraryStudio.Forms.MarcField;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// MARC 编辑器控件
    /// </summary>
    // [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class MarcEditor : MarcControl
    {
        /// <summary>
        /// 获取一个特定模板的XML定义
        /// </summary>
        public event GetTemplateDefEventHandler GetTemplateDef = null;  // 外部接口，获取一个特定模板的XML定义

        /// <summary>
        /// 字段选定状态发生变化
        /// </summary>
        public event EventHandler SelectedFieldChanged = null;

        /// <summary>
        /// 饮用外部的 ApplicationInfo 对象。用于获取和存储一些需要持久的值
        /// </summary>
        public IApplicationInfo AppInfo = null;  // 用于获取和存储一些需要持久的值

        internal bool m_bAutoComplete = true;
        internal bool m_bInsertBefore = false;

        internal uint m_nLastClickTime = 0;
        internal Rectangle m_rectLastClick = new Rectangle();
        internal const TextFormatFlags editflags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.ExpandTabs | TextFormatFlags.NoPrefix;  // | TextFormatFlags.RightToLeft;

        //internal Font m_fixedSizeFont = null;
        //internal Font m_captionFont = null;

        /// <summary>
        /// 是否要把回车键触发自动创建数据的信号
        /// </summary>
        public bool EnterAsAutoGenerate = false;

        #region 变量

        /// <summary>
        /// 当前记录的 MARC 类型。缺省为 "unimarc"
        /// </summary>
        public string MarcSyntax = "unimarc";

        private Record record = null;

        // 内容与控件边框上下左右的空白
        /// <summary>
        /// 内容区域的顶部空白高度
        /// </summary>
        public int TopBlank = 0;
        /// <summary>
        /// 内容区域的底部空白高度
        /// </summary>
        public int BottomBlank = 0;
        /// <summary>
        /// 内容区域的左边空白宽度
        /// </summary>
        public int LeftBlank = 0;
        /// <summary>
        /// 内容区域的右边空白宽度
        /// </summary>
        public int RightBlank = 1;

        /*
        // 文档坐标
        int m_nDocumentOrgX = 0;
        int m_nDocumentOrgY = 0;
        */

#if BORDER
        // 控件边框
        private BorderStyle m_borderStyle = BorderStyle.Fixed3D;   // BorderStyle.Fixed3D;
#endif


        // 各种颜色

        // 竖线条的颜色
        internal Color defaultVertGridColor = Color.LightGray;

        /// <summary>
        /// 竖线条的颜色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color VertGridColor
        {
            get
            {
                return this.defaultVertGridColor;
            }
            set
            {
                this.defaultVertGridColor = value;
                this.Invalidate();
            }
        }

        // 2024/7/7
        // 是否要绘制渐变背景色
        internal bool GradientBack = false;

        // 2024/7/7
        // 是否要绘制老式内陷式单元边框
        internal bool InsetStyleCellBorder = true;

        // 横线条的颜色
        internal Color defaultHorzGridColor = Color.LightGray;

        /// <summary>
        /// 横线条的颜色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color HorzGridColor
        {
            get
            {
                return this.defaultHorzGridColor;
            }
            set
            {
                this.defaultHorzGridColor = value;
                this.Invalidate();
            }
        }

        // 字段名提示的文字色
        internal Color defaultNameCaptionTextColor = SystemColors.InfoText;

        /// <summary>
        /// 字段名提示的文字色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color NameCaptionTextColor
        {
            get
            {
                return this.defaultNameCaptionTextColor;
            }
            set
            {
                this.defaultNameCaptionTextColor = value;
                this.Invalidate();
            }
        }

        // 2024/7/5
        internal Color defaultSelectedBackColor = SystemColors.Highlight;   // Color.FromArgb(90, 90, 255);

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color SelectedBackColor
        {
            get
            {
                return this.defaultSelectedBackColor;
            }
            set
            {
                this.defaultSelectedBackColor = value;
                this.Invalidate();
            }
        }

        // 2024/7/5
        internal Color defaultSelectedTextColor = SystemColors.HighlightText;   // Color.FromArgb(255, 255, 255); 

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color SelectedTextColor
        {
            get
            {
                return this.defaultSelectedTextColor;
            }
            set
            {
                this.defaultSelectedTextColor = value;
                this.Invalidate();
            }
        }

        // 字段名提示的背景色
        internal Color defaultNameCaptionBackColor = SystemColors.Info;

        /// <summary>
        /// 字段名提示的背景色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color NameCaptionBackColor
        {
            get
            {
                return this.defaultNameCaptionBackColor;
            }
            set
            {
                this.defaultNameCaptionBackColor = value;
                this.Invalidate();
            }
        }

        // 字段名的文字色
        internal Color defaultNameTextColor = Color.Blue;   // SystemColors.ActiveCaptionText;

        /// <summary>
        /// 字段名的文字色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color NameTextColor
        {
            get
            {
                return this.defaultNameTextColor;
            }
            set
            {
                this.defaultNameTextColor = value;
                this.Invalidate();
            }
        }

        // 字段名的背景色
        internal Color defaultNameBackColor = SystemColors.Window;

        /// <summary>
        /// 字段名的背景色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color NameBackColor
        {
            get
            {
                return this.defaultNameBackColor;
            }
            set
            {
                this.defaultNameBackColor = value;
                this.Invalidate();
            }
        }

        // 指示符的文字色
        internal Color defaultIndicatorTextColor = Color.Green; // SystemColors.Window;//SystemColors.Control;

        /// <summary>
        /// 指示符的文字色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color IndicatorTextColor
        {
            get
            {
                return this.defaultIndicatorTextColor;
            }
            set
            {
                this.defaultIndicatorTextColor = value;
                this.Invalidate();
            }
        }

        // 指示符的背景色
        internal Color defaultIndicatorBackColor = SystemColors.Window;//SystemColors.Control;

        /// <summary>
        /// 指示符的背景色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color IndicatorBackColor
        {
            get
            {
                return this.defaultIndicatorBackColor;
            }
            set
            {
                this.defaultIndicatorBackColor = value;
                this.Invalidate();
            }
        }

        // 不能编辑的指示符背景颜色
        internal Color defaultIndicatorBackColorDisabled = SystemColors.Control;

        /// <summary>
        /// 不能编辑的指示符背景颜色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color IndicatorBackColorDisabled
        {
            get
            {
                return this.defaultIndicatorBackColorDisabled;
            }
            set
            {
                this.defaultIndicatorBackColorDisabled = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 内容的文字色的缺省值
        /// </summary>
        public static Color DefaultContentTextColor = SystemColors.WindowText; // 缺省色
        // 内容的文字色
        internal Color m_contentTextColor = DefaultContentTextColor;

        /// <summary>
        /// 内容的文字色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color ContentTextColor
        {
            get
            {
                return this.m_contentTextColor;
            }
            set
            {
                this.m_contentTextColor = value;
                this.Invalidate();
            }
        }

        // 内容的背景色
        internal Color defaultContentBackColor = SystemColors.Window;

        /// <summary>
        /// 内容的背景色
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color ContentBackColor
        {
            get
            {
                return this.defaultContentBackColor;
            }
            set
            {
                this.defaultContentBackColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// 新插入一个字段时的缺省字段名
        /// </summary>
        public string DefaultFieldName = "***";

        // 2008/6/4
        internal ImeMode OldImeMode = ImeMode.NoControl;    // off

        /// <summary>
        /// 当前输入法状态
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ImeMode CurrentImeMode
        {
            get
            {
                return this.ImeMode;
            }
            set
            {
                if (this.ImeMode != value)
                {
                    this.ImeMode = value;
                    API.SetImeHalfShape(this);
                }
            }
        }

#if OLD___
        // 小文本编辑控件
        internal MyEdit curEdit = null;
#endif

        // 选中的字段数组下标
        /// <summary>
        /// 当前选中的字段下标数组
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ReadOnlyCollection<int> SelectedFieldIndices
        {
            get
            {
                // 根据 selection start~end 横跨的若干连续字段的 index 设置
                var dom = this.GetDomRecord();
                var ret = dom.LocateFields(dom.SelectionStart,
                    dom.SelectionEnd,
                    out int index,
                    out int count);
                if (ret == false)
                    return new ReadOnlyCollection<int>(new List<int>());
                var results = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    results.Add(i);
                }
                return results.AsReadOnly();
            }
        }

        // 当前获得焦点字段索引号与列号
        // 1: 字段名 2: 指示符 3:字段内容
        internal int m_nFocusCol
        {
            get
            {
                var dom = GetDomRecord();
                var ret = dom.LocateField(dom.CaretOffset,
                    out int field_index,
                    out int offs_in_field);
                if (ret == false)
                    return -1;

                return dom.DetectPart(field_index,
                    offs_in_field);
            }
        }

        // public int m_nFocusedFieldIndex = -1;

        // Shift连续选择时的基准点
        internal int nStartFieldIndex = -1;

        XmlDocument m_domMarcDef = null;

        string m_strMarcDomError = "";

        string _lang = "zh";

        /// <summary>
        /// 当前界面语言代码
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Lang
        {
            get
            {
                return this._lang;
            }
            set
            {
                this._lang = value;
            }
        }

        int nDragCol = -1;	// -1表示当前没有按下鼠标左键。其余值，例如0,1,...，表示鼠标左键按下，数字表示按下拖动的位置
        int nLastTrackerX = -1;

        #endregion

        // 2007/7/20
        /// <summary>
        /// 清除 MarcDefDom
        /// </summary>
        public void ClearMarcDefDom()
        {
            this.MarcDefDom = null;
        }

        // 存储了MARC定义的XmlDocument对象
        /// <summary>
        /// 存储了 MARC 结构定义的 XmlDocument 对象
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XmlDocument MarcDefDom
        {
            get
            {
                if (this.m_domMarcDef != null)
                    return this.m_domMarcDef;

                if (m_strMarcDomError != "")
                    return null;    // 避免反复报错

                string strError = "";

                GetConfigDomEventArgs e = new GetConfigDomEventArgs();
                e.Path = "marcdef";
                e.XmlDocument = null;
                if (this.GetConfigDom != null)
                {
                    this.GetConfigDom(this, e);
                }
                else
                {
                    //throw new Exception("GetConfigFile事件尚未初始化");
                    return null;
                }

                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = "获取marcdef dom出错，原因:" + e.ErrorInfo;
                    goto ERROR1;
                }

                this.m_domMarcDef = e.XmlDocument;
                return this.m_domMarcDef;
            ERROR1:
                m_strMarcDomError = strError;
                return null;
            }
            set
            {
                this._labelCache.Clear();
                this.m_strMarcDomError = "";
                this.m_domMarcDef = value;

                InvalidateCaptionArea();
            }
        }

        #region 事件

        /// <summary>
        /// 获得配置文件
        /// </summary>
        public event GetConfigFileEventHandle GetConfigFile = null;
        /// <summary>
        /// 获得配置文件的 XmlDocument 对象
        /// </summary>
        public event GetConfigDomEventHandle GetConfigDom = null;

        /// <summary>
        /// 自动创建数据
        /// </summary>
        public event GenerateDataEventHandler GenerateData = null;

        /// <summary>
        /// 校验数据
        /// </summary>
        public event GenerateDataEventHandler VerifyData = null;    // 2011/6/19

        /// <summary>
        /// 解析宏
        /// </summary>
        public event ParseMacroEventHandler ParseMacro = null;

        // 2008/3/19
        /// <summary>
        /// Ctrl + 字母键 按下
        /// </summary>
        public event ControlLetterKeyPressEventHandler ControlLetterKeyPress = null;

        // return:
        //      true    键被处理
        //      false   键没有被处理，或者事件根本没有被挂接
        internal bool OnControlLetterKeyPress(Keys keyData)
        {
            if (this.ControlLetterKeyPress != null)
            {
                ControlLetterKeyPressEventArgs e = new ControlLetterKeyPressEventArgs();
                e.KeyData = keyData;
                e.Handled = false;
                // 过程中可能会改变 e.Handled
                this.ControlLetterKeyPress(this, e);
                return e.Handled;
            }

            return false;
        }

        internal void OnGetConfigFile(GetConfigFileEventArgs e)
        {
            if (this.GetConfigFile != null)
            {
                this.GetConfigFile(this, e);
            }
        }

        internal void OnGenerateData(GenerateDataEventArgs e)
        {
            if (this.GenerateData != null)
                this.GenerateData(this, e);
        }

        internal void OnVerifyData(GenerateDataEventArgs e)
        {
            if (this.VerifyData != null)
                this.VerifyData(this, e);
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MarcEditor()
        {
            this.PadWhileEditing = true;
            this.DeleteKeyStyle = DeleteKeyStyle.DeleteKeyAsDeleteField;

            this.record = new Record(this);
            // this.DoubleBuffered = true;

            CreateCheckTimer();

            // 该调用是 Windows.Forms 窗体设计器所必需的。
            InitializeComponent();

            this.GetFieldCaption = (field) =>
            {
                if (field.IsHeader)
                {
                    if (this.Lang.StartsWith("zh"))
                        return $"头标区";
                    return "Header";
                }
                return GetCachedLabel(field.FieldName);
            };
            int caret_field_index = -1;
            this.CaretMoved += (sender, e) =>
            {
                if (this.SelectedFieldChanged == null)
                    return;
                // TODO: 当 caret_field_index 所在的字段改变字段名时也应触发本事件

                var current_index = this.CaretFieldIndex;
                Debug.WriteLine($"field_index={current_index}");
                if (current_index != caret_field_index)
                {
                    caret_field_index = current_index;
                    // 触发 SelectedFieldChanged 事件
                    SelectedFieldChanged?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        //
        // 摘要:
        //     释放由 System.Windows.Forms.Control 和它的子控件占用的非托管资源，另外还可以释放托管资源。
        //
        // 参数:
        //   disposing:
        //     为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。
        /// <summary>
        /// 释放由 System.Windows.Forms.Control 和它的子控件占用的非托管资源，另外还可以释放托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region 组件设计器生成的代码

        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 设计器支持所需的方法 - 不要使用代码编辑器 
        /// 修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            if (this.DesignMode)
                return;
            components = new System.ComponentModel.Container();
        }
        #endregion

        // TODO: 需要测试一下
        /// <summary>
        /// 选定小编辑器的指定范围
        /// </summary>
        /// <param name="nStart">开始位置</param>
        /// <param name="nLength">字符数</param>
        public void SelectCurEdit(int nStart, int nLength)
        {
            /*
            if (this.curEdit == null)
                return;
            this.curEdit.Select(nStart, nLength);
            */

            // 为了兼容
            var dom = GetDomRecord();
            var ret = dom.LocateField(dom.CaretOffset,
                out int field_index,
                out int offs_in_field);
            if (ret == false)
                return;

            dom.GetFieldOffsRange(field_index,
                1,
                out int start,
                out _);

            var col = dom.DetectPart(field_index,
                offs_in_field);

            // 1: 字段名 2: 指示符 3:字段内容
            var caret = this.record.Fields[field_index]
                .GetRegionCaretPos(col);

            this.Select(start + caret, start + caret, start + caret);
        }

#if REMOVED
        // 设置this.Font的时候，this.FixedSizeFont和this.CaptionFont都会受到影响。具体来说就是字号还是采用和this.Font一样的字号，但字体名用原来的(如果有的话)
        // 如果要设置完全独立的this.FixedSizeFont和this.CaptionFont，请在设置好this.Font以后再设置它们
        /// <summary>
        /// 当前字体
        /// </summary>
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
#if SAFE
                if (this.DesignMode)
                {
                    base.Font = value;
                    return;
                }
#endif
                // ChangeRelativeFonts(value);

                // 最后触发
                base.Font = value;

                // 2025/11/20
                // 确保触发
                OnFontChanged(new EventArgs());
            }
        }
#endif

#if REMOVED
        // 因为主要字体改变，需要连带改变其它相关字体的尺寸
        void ChangeRelativeFonts(Font value)
        {
            if (value == null)
            {
                this.m_fixedSizeFont = value;
                this.m_captionFont = value;
                base.Font = value;  // 2019/5/18 从 this.Font 改过来
                return;
            }

            // 初始化内部等宽字体
            if (this.m_fixedSizeFont == null)
                this.m_fixedSizeFont = value;
            else
                this.m_fixedSizeFont = new Font(this.m_fixedSizeFont.FontFamily,
                    value.SizeInPoints,
                    FontStyle.Bold,
                    GraphicsUnit.Point);

            // 初始化标签字体
            if (this.m_captionFont == null)
                this.m_captionFont = value;
            else
            {
                // TODO: 测试一下是否会导致内存泄漏
                this.m_captionFont = new Font(this.m_captionFont.FontFamily, value.SizeInPoints, GraphicsUnit.Point);
            }
        }

#endif

#if REMOVED
        /// <summary>
        /// 当前等宽字体
        /// </summary>
        // C#
        public Font FixedSizeFont
        {
            get
            {
#if SAFE
        if (this.DesignMode)
            return this.Font;
#endif
                if (this.m_fixedSizeFont == null)
                {
                    float size = this.Font != null ? this.Font.SizeInPoints : 9f;
                    string familyName = "Courier New";
                    bool found = FontFamily.Families.Any(f => string.Equals(f.Name, familyName, StringComparison.OrdinalIgnoreCase));
                    try
                    {
                        if (found)
                            this.m_fixedSizeFont = new Font(new FontFamily(familyName), size, FontStyle.Bold, GraphicsUnit.Point);
                        else
                            this.m_fixedSizeFont = new Font(this.Font?.FontFamily ?? SystemFonts.DefaultFont.FontFamily, size, FontStyle.Bold, GraphicsUnit.Point);
                    }
                    catch
                    {
                        // 最后兜底，保证不抛出
                        this.m_fixedSizeFont = new Font(this.Font?.FontFamily ?? SystemFonts.DefaultFont.FontFamily, size, FontStyle.Bold, GraphicsUnit.Point);
                    }
                }
                return this.m_fixedSizeFont;
            }
            set
            {
#if SAFE
        if (this.DesignMode)
            return;
#endif
                if (this.m_fixedSizeFont != null)
                    this.m_fixedSizeFont.Dispose();
                this.m_fixedSizeFont = value;
            }
        }
#endif

#if REMOVED
        void FreeFonts()
        {
            this.m_fixedSizeFont?.Dispose();
            this.m_captionFont?.Dispose();
        }
#endif

#if REMOVED
        /// <summary>
        /// 当前提示区字体
        /// </summary>
        public Font CaptionFont
        {
            get
            {
#if SAFE
                if (this.DesignMode)
                    return this.Font;
#endif
                if (this.m_captionFont == null)
                {
                    if (this.Font != null)
                        this.m_captionFont = NewFont(this.Font); // this.Font;
                    else
                        /*this.m_fixedSizeFont bug*/
                        m_captionFont = new Font(new FontFamily("宋体"),
9,
GraphicsUnit.Point);
                }
                return this.m_captionFont;
            }
            set
            {
#if SAFE
                if (this.DesignMode)
                    return;
#endif
                if (this.m_captionFont != null)
                    this.m_captionFont.Dispose();

                this.m_captionFont = value;
            }
        }
#endif

        static Font NewFont(Font ref_font)
        {
            return new Font(ref_font.FontFamily,
    ref_font.SizeInPoints,
    ref_font.Style,
    GraphicsUnit.Point);
        }

        #region 让控件有边框

        // 重载CreateParams的目的是为了让控件有边框
        //
        // 摘要:
        //     获取创建控件句柄时所需要的创建参数。
        //
        // 返回结果:
        //     System.Windows.Forms.CreateParams，包含创建控件的句柄时所需的创建参数。
        /// <summary>
        /// 获取创建控件句柄时所需要的创建参数。
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                if (this.DesignMode)
                    return base.CreateParams;

                CreateParams param = base.CreateParams;

#if BORDER
                if (this.m_borderStyle == BorderStyle.FixedSingle)
                {
                    param.Style |= API.WS_BORDER;
                }
                else if (this.m_borderStyle == BorderStyle.Fixed3D)
                {
                    param.ExStyle |= API.WS_EX_CLIENTEDGE;
                }
#endif
                return param;
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DestroyCheckTimer();
            base.OnHandleDestroyed(e);
        }

#if BORDER
        /// <summary>
        /// 边框风格
        /// </summary>
        [Category("Appearance")]
        [DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "Fixed3D")]
        public BorderStyle BorderStyle
        {
            get
            {
                return this.m_borderStyle;
            }
            set
            {
                if (this.DesignMode)
                    return;

                this.m_borderStyle = value;

                // Get Styles using Win32 calls
                int style = API.GetWindowLong(Handle, API.GWL_STYLE);
                int exStyle = API.GetWindowLong(Handle, API.GWL_EXSTYLE);

                // Modify Styles to match the selected border style
                BorderStyleToWindowStyle(ref style, ref exStyle);

                // Set Styles using Win32 calls
                API.SetWindowLong(Handle, API.GWL_STYLE, style);
                API.SetWindowLong(Handle, API.GWL_EXSTYLE, exStyle);

                // Tell Windows that the frame changed
                API.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    API.SWP_NOACTIVATE | API.SWP_NOMOVE | API.SWP_NOSIZE |
                    API.SWP_NOZORDER | API.SWP_NOOWNERZORDER |
                    API.SWP_FRAMECHANGED);
            }
        }

        private void BorderStyleToWindowStyle(ref int style, ref int exStyle)
        {
            style &= ~API.WS_BORDER;
            exStyle &= ~API.WS_EX_CLIENTEDGE;
            switch (this.m_borderStyle)
            {
                case BorderStyle.Fixed3D:
                    exStyle |= API.WS_EX_CLIENTEDGE;
                    break;

                case BorderStyle.FixedSingle:
                    style |= API.WS_BORDER;
                    break;

                case BorderStyle.None:
                    // No border style values
                    break;
            }
        }
#endif


        #endregion

        #region 关于小 edit 控件的一些函数


        internal List<Field> GetFields(int start_index,
            int length)
        {
            List<Field> results = new List<Field>();
            for (int i = start_index;
                i < Math.Min(start_index + length, this.record.Fields.Count);
                i++)
            {
                results.Add(this.record.Fields[i]);
            }

            return results;
        }

        #endregion

        #region 重载的一些函数 HandleCreated paint

#if REMOVED
        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.HandleCreated 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.EventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.HandleCreated 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.EventArgs。</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnHandleCreated(e);
                return;
            }

            base.OnHandleCreated(e);
        }
#endif

        /// <summary>
        /// 刷新字段名提示区域
        /// </summary>
        public void RefreshNameCaption()
        {
            // 出于兼容性保留函数

            // 清除缓存，确保可以重新获得提示文字
            this.ClearLabelCache();
            InvalidateCaptionArea();

            /*
            if (this.record != null)
            {
                this.record.RefreshNameCaption();
                this.Invalidate();  // 2007/12/26
            }
            */
        }

        // TODO
        public bool InCtrlK
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///字段名区域宽度
        /// </summary>
        [Category("Appearance")]
        [DescriptionAttribute("Field Name Caption Area Width")]
        [DefaultValue(typeof(int), "100")]
        public int FieldNameCaptionWidth
        {
            get
            {
                if (this.DesignMode)
                    return 100;

                return this.CaptionPixelWidth;
                /*
                if (this.record == null)
                    return 100;

                return this.record.NameCaptionPureWidth;
                */
            }
            set
            {
                if (this.DesignMode)
                    return;

                if (this.record == null)
                    return;

                if (value < 0)
                    value = 0;
                this.CaptionPixelWidth = value;
#if REMOVED
                this.record.NameCaptionPureWidth = value;

                /*
				if (this.record.NameCaptionTotalWidth >= this.ClientWidth)
					this.record.NameCaptionPureWidth -= 20;
                 * */

                if (this.record.NameCaptionPureWidth < 0)
                    this.record.NameCaptionPureWidth = 0;

                // 迫使每个单元重新测算高度
                // this.record.CalculateFieldsHeight(0, -1, true);

                // this.DocumentOrgX = this.DocumentOrgX;
#endif
            }
        }

        static IDataObject GetClipboardDataObject()
        {
            IDataObject ido = null;

            StringUtil.RunClipboard(() =>
            {
                ido = Clipboard.GetDataObject();
            });

            return ido;
        }


        // 繁体到简体
        void menuItem_t2s(object sender, EventArgs e)
        {
            if (T2S() == false)
                Console.Beep();
        }

        public bool T2S()
        {
            var text = this.GetSelectedContent();
            if (string.IsNullOrEmpty(text))
                return false;

            this.SoftlyPaste(API.ChineseT2S(text));
            return true;
        }

        void menuItem_s2t(object sender, EventArgs e)
        {
            if (S2T() == false)
                Console.Beep();
        }

        public bool S2T()
        {
            var text = this.GetSelectedContent();
            if (string.IsNullOrEmpty(text))
                return false;

            this.SoftlyPaste(API.ChineseS2T(text));
            return true;
        }

        void menuItem_removeEmptyFieldsSubfields(object sender, EventArgs e)
        {
            string strError = "";

            string strMARC = this.Marc;

            // 删除空的子字段
            // return:
            //      -1  error
            //      0   没有修改
            //      1   发生了修改
            int nRet = MarcUtil.RemoveEmptySubfields(ref strMARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 删除空的字段
            // return:
            //      -1  error
            //      0   没有修改
            //      1   发生了修改
            nRet = MarcUtil.RemoveEmptyFields(ref strMARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
                this.Marc = strMARC;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 转换为平行模式
        void menuItem_toParallel(object sender, EventArgs e)
        {
            string strMARC = this.Marc;

            MarcRecord record = new MarcRecord(strMARC);
            MarcQuery.ToParallel(record);

            this.Marc = record.Text;
        }

        // 转换为 880 模式
        void menuItem_to880(object sender, EventArgs e)
        {
            string strMARC = this.Marc;

            MarcRecord record = new MarcRecord(strMARC);
            MarcQuery.To880(record);

            this.Marc = record.Text;
        }

        void menuItem_removeEmptyFields(object sender, EventArgs e)
        {
            string strError = "";

            string strMARC = this.Marc;

            // 删除空的字段
            // return:
            //      -1  error
            //      0   没有修改
            //      1   发生了修改
            int nRet = MarcUtil.RemoveEmptyFields(ref strMARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
                this.Marc = strMARC;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menuItem_removeEmptySubfields(object sender, EventArgs e)
        {
            string strError = "";

            string strMARC = this.Marc;

            // 删除空的子字段
            // return:
            //      -1  error
            //      0   没有修改
            //      1   发生了修改
            int nRet = MarcUtil.RemoveEmptySubfields(ref strMARC,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
                this.Marc = strMARC;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 2024/5/20
        // 复制机内格式的完整记录到剪贴板
        void CopyJineiToClipboard(object sender, EventArgs e)
        {
            /*
            string strText = "";
            for (int i = 0; i < this.record.Fields.Count; i++)
            {
                Field field = this.record.Fields[i];
                strText += field.GetFieldMarc(true);
            }
            MarcEditor.TextToClipboardFormat(strText);
            */
            MarcEditor.TextToClipboardFormat(this.Content);
        }

        // 复制工作单格式的完整记录到剪贴板
        void CopyWorksheetToClipboard(object sender, EventArgs e)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            // 将机内格式变换为工作单格式
            // return:
            //      -1  出错
            //      0   成功
            int nRet = MarcUtil.CvtJineiToWorksheet(
                this.Marc,
                -1,
                out List<string> lines,
                out string strError);
            if (nRet == -1)
                goto ERROR1;

            string strText = "";
            foreach (string line in lines)
            {
                strText += line + "\r\n";
            }

            // 按住 Ctrl 键，则功能变为把子字段符号变化为 '$'，便于写书什么的
            if (bControl)
                strText = strText.Replace((char)31/*"ǂ"*/, '$');

            MarcEditor.TextToClipboard(strText);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if REMOVED
        internal void menuItem_sortFields(object sender, EventArgs e)
        {
            // TODO: 将所有字段变为字符串，然后对字符串排序，最后重新创建内容
            List<string> fields = new List<string>();
            foreach (var field in this.record.Fields)
            {
                fields.Add(field.Text);
            }

            if (fields.Count <= 2)
                return;

            var sort_fields = fields.GetRange(1, fields.Count - 1);
            sort_fields.Sort();

            sort_fields.Insert(0, fields[0]);

            this.record.Fields.Clear();
            this.record.Fields.InsertFields(0, sort_fields);
            this.Select(0, 0, 0);
            // this.FireTextChanged();
        }
#endif

        void Menu_r2l(object sender, EventArgs e)
        {
            if (this.RightToLeft == RightToLeft.Inherit)
            {
                this.RightToLeft = RightToLeft.Yes;
                return;
            }

            if (this.RightToLeft == RightToLeft.Yes)
                this.RightToLeft = RightToLeft.No;
            else
                this.RightToLeft = RightToLeft.Yes;
        }


#if REMOVED
        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.FontChanged 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.EventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.FontChanged 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.EventArgs。</param>
        protected override void OnFontChanged(
            EventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnFontChanged(e);
                return;
            }

            // ChangeRelativeFonts(this.Font);

            base.OnFontChanged(e);
        }
#endif

        #endregion

        #region 公共的属性 函数

        // 为兼容以前脚本代码保留
        // 当前具有输入焦点的字段
        /// <summary>
        /// 当前具有输入焦点的字段对象
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Field FocusedField
        {
            get
            {
                if (this.DesignMode)
                    return null;

                if (this.record.Fields.Count == 0)
                    return null;

                return this.record.Fields.GetFocusedField();
            }
            set
            {
                if (this.DesignMode)
                    return;

                if (value == null)
                    return;

                var focused_field = this.record.Fields.GetFocusedField();
                if (focused_field.Equal(value))
                    return;

                int nIndex = this.record.Fields.IndexOf(value);
                if (nIndex == -1)
                    return;
                this.SetActiveField(nIndex, 3);
            }
        }

        // TODO: 注意测试当 字段被删除 .Marc 被清除等情况下，FocuedFieldIndex 不应下标越界
        // 当前获得焦点字段的索引号
        /// <summary>
        /// 当前具有输入焦点字段(在全部字段集合中的)下标
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int FocusedFieldIndex
        {
            get
            {
                if (this.DesignMode)
                    return 0;

                // return FocusedField?.Index ?? -1;
                var index = this.CaretFieldIndex;
                // TODO: 可否直接表达 CaretFieldIndex 的在最后一个字段以后 index?
                if (index >= this.GetDomRecord().FieldCount)
                    return -1;
                return index;
            }
            set
            {
                if (this.DesignMode)
                    return;

                if (value != -1)
                {
                    SetActiveField(value, 3);
                }
            }
        }

        // 不会抛出异常的版本
        public int SafeFocusedFieldIndex
        {
            get
            {
                if (this.DesignMode)
                    return 0;

                try
                {
                    return FocusedFieldIndex;
                }
                catch
                {
                    return -1;
                }
            }
        }


        // 1表示字段名上 2 表示在指示符上
        /// <summary>
        /// 当前具有输入焦点的子字段名。
        /// 若为 (char)0 表示当前没有字段具有输入焦点; 若为 (char)1 表示当前输入焦点在字段名上; 若为 (char)2 表示当前输入焦点在字段指示符上
        /// </summary>
        public char FocusedSubfieldName
        {
            get
            {
                if (this.DesignMode)
                    return (char)0;

                var dom = GetDomRecord();
                var ret = dom.LocateField(dom.CaretOffset,
                    out int field_index,
                    out int offs_in_field);
                if (ret == false)
                    return (char)0;

                var col = dom.DetectPart(field_index,
                    offs_in_field);

                // 1: 字段名 2: 指示符 3:字段内容
                if (col != 3)
                    return (char)col;

                var text = this.record.Fields[field_index].Text;
                return MarcUtil.SubfieldNameByOffs(
                    text,
                    offs_in_field);
            }
        }

        // parameters:
        //      caret_offs_in_end_level   [out] 返回在最后一级 level 的全文(包括字段名部分)中的插入符偏移
        public string GetCurrentLocationString(out int caret_offs_in_end_level)
        {
            caret_offs_in_end_level = 0;
            //if (this.curEdit == null)
            //    throw new Exception("this.curEdit 为 null，无法执行 GetCurrentLocationString()");
            //if (this.curEdit.Visible == false)
            //    return "";

            // 当前插入符所在位置
            //int current_offs = this.curEdit.SelectionStart;
            // 探测点取中间位置
            //if (this.curEdit.SelectionLength > 0)
            //    current_offs += this.curEdit.SelectionLength / 2;

            return GetSubfieldLocationString(
                this.CaretOffset,
                out caret_offs_in_end_level);
            // 简化处理。直接用全剧 offs
            //caret_offs_in_end_level = 0;
            //return Math.Min(this.BlockStartOffset, this.BlockEndOffset).ToString();
        }


        /*
        // 获得子字段周边信息，扩展版本，能获得 offs 所在的、不是子字段的区域的信息
        // parameters:
        //      offs        在当前字段内的 offs 偏移量
        //      right_most    当插入符处在内容末端时，是否认为命中最后一个子字段
        public SubfieldBound GetSubfieldBoundsEx(
    int offs,
    bool right_most = false)         * 
         * */

        // TODO: 准备单元测试
        // parameters:
        //      field_index 字段下标
        //      caret_offs  当前插入符在字段内容中的下标
        //      caret_offs_in_end_level   [out] 返回在最后一级 level 的全文(包括字段名部分)中的插入符偏移
        public string GetSubfieldLocationString(
            //int field_index,
            //int caret_offs,
            int global_offs,
            out int caret_offs_in_end_level)
        {
            caret_offs_in_end_level = 0;

            int field_index = this.CaretFieldIndex;
            if (field_index >= this.MarcRecord.FieldCount)
                return "";

            var field = this.Record.Fields[field_index];
            var field_location = this.Record.GetFieldLocationString(field_index);

            var content_text = field.Value;

            this.MarcRecord.GetFieldOffsRange(field_index,
                out int start,
                out int end);
            int offs_in_field = this.CaretOffset - start;
            var m_field = this.MarcRecord.GetField(field_index);
            var info = m_field.GetSubfieldBoundsEx(
    offs_in_field,
    true);
            if (info.Name == "!name" || info.Name == "!indicator")
                return field_location;

            /*
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
            {
                // TODO
                return field_location;
            }
            */
            int name_indicator_length = field.Name.Length + field.Indicator.Length;
            int offs_in_content = offs_in_field - name_indicator_length;

            int nContentStart = info.ContentStartOffs - name_indicator_length;
            int nStart = info.StartOffs - name_indicator_length;
            string strSubfieldName = info.Name;

            DigitalPlatform.Marc.MarcField marc_field = new MarcField(field.Name,
                field.Indicator,
                content_text);

            int name_count = -1; // 相同字段名的计数
            int offs = marc_field.Leading.Length;
            foreach (MarcSubfield subfield in marc_field.select("subfield"))
            {
                if (offs >= nContentStart
                    || offs >= nStart)
                {
                    caret_offs_in_end_level = offs_in_content - offs;
                    if (name_count <= 0)
                        return field_location + "/" + strSubfieldName;
                    return field_location + "/" + strSubfieldName + ":" + name_count.ToString();
                }
                if (subfield.Name == strSubfieldName)
                    name_count++;
                offs += subfield.Text.Length;
            }

            if (name_count > 0)
                return field_location + "/" + strSubfieldName + ":" + name_count.ToString();    // 2024/8/12
            else
                return field_location + "/" + strSubfieldName;
        }


#if REMOVED
        internal void Test()
        {
            char subfieldname = MarcUtil.SubfieldNameByOffs(
                this.FocusedField.Value,
                this.SelectionStart);

            string strText = "SelectionStart=" + Convert.ToString(this.SelectionStart + ", subfieldname=" + Convert.ToString(subfieldname));
            MessageBox.Show(this, strText);
        }
#endif

        // 计算nPos位置左边一共有多少个Unicode方向字符
        static int CountFillingChar(string strText,
            int nPos)
        {
            int nCount = 0;
            for (int i = 0; i < nPos; i++)
            {
                if (strText[i] == 0x200e)
                    nCount++;
            }
            return nCount;
        }

        // 调正位置。把纯净位置折算为含有方向字符以后的位置
        static int AdjustPos(string strText,
    int nPos)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return 0;
            int nCount = 0;
            for (int i = 0; i < nPos; i++)
            {
                if (nPos == nCount)
                    return i;
                if (strText[i] != 0x200e)
                    nCount++;
            }
            return strText.Length;
        }

#if REMOVED
        // value中的列号
        /// <summary>
        /// 小编辑器中当前选定范围的开始位置
        /// </summary>
        public int SelectionStart
        {
            get
            {
                if (this.DesignMode)
                    return -1;

				if (this.curEdit != null)
					return this.curEdit.SelectionStart;
                return -1;
            }
            set
            {
                if (this.DesignMode)
                    return;

                if (value >= 0 && this.curEdit != null)
                {
                    if (value < this.curEdit.Text.Length)
                    {
                        this.curEdit.SelectionStart = value;
                    }
                }
            }
        }

#endif

        /*
                // 选中的字段数组
                public Field[] SelectedFields 
                {
                    get
                    {
                        ArrayList aField = new ArrayList();
                        foreach(int i in this.SelectedFieldIndices)
                        {
                            int nIndex = (int)this.SelectedFieldIndices[i];
                            if (nIndex == -1)
                                continue;
                            Field field = (Field)this.record[nIndex];
                            aField.Add(field);
                        }

                        Field[] fields = new Field[aField.Count];
                        for(int i=0;i<fields.Length;i++)
                        {
                            Field field = (Field)aField[i];
                            fields[i] = field;
                        }

                        return fields;
                    }
                }
        */

        /// <summary>
        /// 获得 Record 对象
        /// </summary>
        public Record Record
        {
            get
            {
                return this.record;
            }
        }

        // internal const string default_marc = "012345678901234567890123001A0000001";
        // internal const string default_marc = "?????nam0 22?????3i 45  001A0000001";
        internal const string default_marc = "?????nam0 22?????3i 45  ";

        // 
        // 设数据xml
        /// <summary>
        /// 当前 MARC 字符串(机内格式)
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("MARC record")]
        [DefaultValue(typeof(string), default_marc)]
        public string Marc
        {
            get
            {
                return this.TryGet(() =>
                {
                    if (this.DesignMode && this.record == null)
                        return default_marc;
                    return base.Content;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    if (this.DesignMode && this.record == null)
                        return;

                    // 兼容原来的效果: 设置 Changed 为 true; 不清除编辑历史
                    base.SetContent(value,
                        set_changed: true,
                        clear_history: false);

                    // 删除大量内容(字段)后，窗口原点偏移量可能处在不合适的位置，令操作者看不到上面显示的内容
                    // 这时需要改变原点偏移量
                    // AdjustOriginY();
                });
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string Content
        {
            get
            {
                return base.Content;
            }
            set
            {
                base.Content = value;
            }
        }

#if REMOVED
        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("Changed")]
        [DefaultValue(false)]
        public new bool Changed
        {
            get
            {
                if (this.DesignMode)
                    return false;
                return base.Changed;
            }
            set
            {
                if (this.DesignMode)
                    return;

                base.Changed = value;

                if (value == true)
                    this.FireTextChanged();
            }
        }
#endif

        /// <summary>
        /// 将小编辑器中(暂时)的内容兑现到内存结构
        /// </summary>
        public void Flush()
        {
            // 为兼容考虑，暂时保留

            /*
            if (this.SelectedFieldIndices?.Count == 1)
            {
                this.EditControlTextToItem();
            }
            */
        }

        #endregion

        #region 内部属性 函数

        public bool SetActiveField(string locationString,
            bool set_focus)
        {
            // 把 locationString 转换为 field_index col
            this.Record.GetLocation(locationString,
                out int field_index,
                out int col,
                out int caret_pos);
            if (field_index == -1)
                return false;
            if (col == -1)
                col = 3;

            var field = this.Record.Fields[field_index];
            int name_indicator_length = 0;
            if (col == 2)
                name_indicator_length = field.Name.Length;
            else if (col == 3)
                name_indicator_length = field.Name.Length + field.Indicator.Length;
            if (this.MarcRecord.GetFieldOffsRange(field_index,
                out int start,
                out int end) == false)
            {
                this.SetActiveField(field_index, col, set_focus);
                return false;
            }
            int offs = start + name_indicator_length + caret_pos;
            this.Select(offs, offs, offs);
            /*
            this.SetActiveField(field_index, col, set_focus);
            if (this.curEdit != null
                && this.curEdit.Visible)
            {
                if (caret_pos == -1)
                    caret_pos = 0;
                this.curEdit.SelectionStart = caret_pos;
                this.curEdit.SelectionLength = 0;
            }
            */
            return true;
            /*
            // 简化。直接定位全局 offs
            if (Int32.TryParse(locationString, out int value) == false)
            {
                throw new ArgumentException($"locationString 参数值 '{locationString}' 不合法。应为一个整数");
                // return false;
            }

            this.Select(value, value, value);
            return true;
            */
        }

        // 包装版本
        /// <summary>
        /// 设置活动字段
        /// </summary>
        /// <param name="field">字段对象</param>
        /// <param name="nCol">插入符的列号。列号的含义：1: 字段名 2: 指示符 3:字段内容</param>
        public void SetActiveField(Field field,
            int nCol)
        {
            this.SetActiveField(field, nCol, true);
        }

        // 把一个字段设为当前活动的字段(对象版本)
        internal void SetActiveField(Field field,
            int nCol,
            bool bFocus)
        {
            int nIndex = this.record.Fields.IndexOf(field);
            Debug.Assert(nIndex != -1, "field不可能不在record里");

            this.SetActiveField(nIndex, nCol, bFocus);
        }

        // 包装版本
        internal void SetActiveField(int nFieldIndex,
            int nCol)
        {
            // 2024/10/16 用户反馈上下方向键无法上下移动插入符了
            /*
            if (nFieldIndex > 0
    && _nCurrentFieldIndex >= this.Record.Fields.Count)
                return;
            */

            this.SetActiveField(nFieldIndex, nCol, 0, true);
        }

        // 兼容以前调用
        internal void SetActiveField(int nFieldIndex,
    int nCol,
    bool bFocus)
        {
            SetActiveField(nFieldIndex,
    nCol,
    0,
    bFocus);
        }

        // 把一个字段设为当前活动的字段
        // parameters:
        //      caret_pos   小 edit 要设置的插入符的字符位置。从本区域单独计数
        //      focus_to_edit  是否将焦点放到上面
        internal void SetActiveField(int nFieldIndex,
            int nCol,
            int caret_pos,
            bool bFocus)
        {
            if (nFieldIndex < 0 || nFieldIndex >= this.record.Fields.Count)
            {
                // throw new ArgumentException("ActiveField()调用错误，不能设当前活动字段为'" + Convert.ToString(nFieldIndex) + "'。下标越界。");
                return;
            }

            var dom = GetDomRecord();

            dom.GetFieldOffsRange(nFieldIndex,
                1,
                out int start,
                out int end);
            int caret_offs = 0;


            dom.Select(caret_offs, caret_offs, caret_offs);

            // col: 0 提示区; 1 字段名; 2 指示符; 3 内容
            Field fieldTemp = this.record.Fields[nFieldIndex];
            if (fieldTemp.IsHeader)
            {
            }
            else if (fieldTemp.IsControlField)
            {
                if (nCol >= 2)
                    caret_offs += 3;
            }
            else if (nCol == 2)
                caret_offs += 3;
            else if (nCol == 3)
                caret_offs += 5;

            if (caret_offs > fieldTemp.Text.Length)
                caret_offs = fieldTemp.Text.Length;

            /*
            if (nCol == 1)
                caret_offs = 0;
            else if (nCol == 2)
                caret_offs = fieldTemp.Name.Length;
            else if (nCol == 3)
                caret_offs = fieldTemp.Name.Length + fieldTemp.Indicator.Length;
            */
            caret_offs += start;
            this.Select(caret_offs, caret_offs, caret_offs);
        }

#if REMOVED
        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxxxx
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.Exception
Message: 下标越界。
Stack:
在 DigitalPlatform.Marc.FieldCollection.get_Item(Int32 nIndex)
在 DigitalPlatform.Marc.MarcEditor.AddSelectedField(Int32 nStartFieldIndex, Int32 nEndFieldIndex, Boolean bClear)
在 DigitalPlatform.Marc.MarcEditor.OnMouseDown(MouseEventArgs e)
在 System.Windows.Forms.Control.WmMouseDown(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.OnMessage(Message& m)
在 System.Windows.Forms.Control.ControlNativeWindow.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.4.5697.17821, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 5.1.2600 Service Pack 3 
操作时间 2015/8/7 14:43:27 (Fri, 07 Aug 2015 14:43:27 +0800) 
前端地址 xxxxx 经由 http://dp2003.com/dp2library 
         * 
         * */
        // 把一个字段设为当前活动的字段
        internal void AddSelectedField(int nStartFieldIndex,
            int nEndFieldIndex,
            bool bClear)
        {
            int nIndex1 = 0;
            int nIndex2 = 0;

            if (nStartFieldIndex > nEndFieldIndex)
            {
                nIndex1 = nEndFieldIndex;
                nIndex2 = nStartFieldIndex;
            }
            else
            {
                nIndex1 = nStartFieldIndex;
                nIndex2 = nEndFieldIndex;
            }
            if (bClear == true)
            {
                this.ClearSelectFieldIndices();
            }

            for (int i = nIndex1; i <= nIndex2; i++)
            {
                // 2015/8/8
                // 保护下标范围
                if (i < 0 || i >= this.Record.Fields.Count)
                    continue;
                this.SelectedFieldIndices.Add(i);
                Field field = this.record.Fields[i];
                field.Selected = true;
                Rectangle rect2 = this.GetItemBounds(i,
                    1,
                    BoundsPortion.Field);
                this.Invalidate(rect2);
            }

            // 2024/8/15
            // 如果已经存在选中项了，则隐藏小Edit控件
            if (this.SelectedFieldIndices.Count > 1)
            {
                this.HideTextBox();
                if (this.Focused == false)
                {
                    // 希望执行 WM_SETFOCUS 时不要自动调用 SetEditPos();
                    this.AddFocusParameter("dont_seteditpos", true);
                    this.Focus();
                }
            }

            if (this.SelectedFieldChanged != null)
            {
                this.SelectedFieldChanged(this, new EventArgs());
                //m_chOldSubfieldName = this.FocusedSubfieldName;
            }
        }

#endif

        public void FireSelectedFieldChanged()
        {
            if (this.SelectedFieldChanged != null)
            {
                this.SelectedFieldChanged(this, new EventArgs());
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            // 检查当前 caret 所在的字段、子字段名是否改变，如果改变就
            // 触发 SelectFieldChanged 事件
            base.OnTextChanged(e);
            ScheduleEvent();
        }

        // 文档横向编移量
        /// <summary>
        /// 当前文档横向编移量
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int DocumentOrgX
        {
            get
            {
                if (this.DesignMode)
                    return 0;
                return -this.HorizontalScroll.Value;
            }
            set
            {
                // 为了兼容
            }
            /*
            set
            {
                if (this.DesignMode)
                    return;

                int nDocumentOrgX_old = this.m_nDocumentOrgX;

                // 视图大于文档
                if (this.ClientWidth >= this.DocumentWidth)
                {
                    this.m_nDocumentOrgX = 0;
                }
                else
                {
                    if (value <= -this.DocumentWidth + this.ClientWidth)
                        this.m_nDocumentOrgX = -this.DocumentWidth + this.ClientWidth;
                    else
                        this.m_nDocumentOrgX = value;

                    if (this.m_nDocumentOrgX > 0)
                        this.m_nDocumentOrgX = 0;
                }

                // 修改卷滚条
                AfterDocumentChanged(ScrollBarMember.Both,
                    null);

                // 卷屏
                int nDelta = this.m_nDocumentOrgX - nDocumentOrgX_old;
                if (nDelta != 0)
                {
                    RECT rect = new RECT();
                    rect.left = 0;
                    rect.top = 0;
                    rect.right = this.ClientWidth;
                    rect.bottom = this.ClientHeight;

                    API.ScrollWindowEx(this.Handle,
                        nDelta,
                        0,
                        ref rect,
                        IntPtr.Zero,	//	ref RECT lprcClip,
                        0,	// int hrgnUpdate,
                        IntPtr.Zero,	// ref RECT lprcUpdate,
                        API.SW_INVALIDATE);
                }
            }
            */
        }

        // 文档纵向偏移量
        /// <summary>
        /// 文档纵向偏移量
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int DocumentOrgY
        {
            get
            {
                if (this.DesignMode)
                    return 0;
                return -this.VerticalScroll.Value;
            }
            set
            {
                // 为了兼容
            }
            /*
            set
            {
                if (this.DesignMode)
                    return;

                int nDocumentOrgY_old = this.m_nDocumentOrgY;
                if (this.ClientHeight >= this.DocumentHeight)
                {
                    this.m_nDocumentOrgY = 0;
                }
                else
                {
                    if (value <= -this.DocumentHeight + this.ClientHeight)
                        this.m_nDocumentOrgY = -this.DocumentHeight + this.ClientHeight;
                    else
                        this.m_nDocumentOrgY = value;

                    if (this.m_nDocumentOrgY > 0)
                        this.m_nDocumentOrgY = 0;
                }

                AfterDocumentChanged(ScrollBarMember.Both,
                    null);


                // 屏幕需要卷滚的区域
                int nDelta = this.m_nDocumentOrgY
                    - nDocumentOrgY_old;
                if (nDelta != 0)
                {
                    RECT rect = new RECT();
                    rect.left = 0;
                    rect.top = 0;
                    rect.right = this.ClientSize.Width;
                    rect.bottom = this.ClientSize.Height;

                    API.ScrollWindowEx(this.Handle,
                        0,
                        nDelta,
                        ref rect,
                        IntPtr.Zero,	//	ref RECT lprcClip,
                        0,	// int hrgnUpdate,
                        IntPtr.Zero,	// ref RECT lprcUpdate,
                        API.SW_INVALIDATE);
                }
            }
            */
        }

        // 文档宽度
        internal int DocumentWidth
        {
            get
            {
                return this.AutoScrollMinSize.Width;
            }
        }

        // 文档高度
        internal int DocumentHeight
        {
            get
            {
                return this.AutoScrollMinSize.Height;
            }
        }

        // 客户区宽度
        internal int ClientWidth
        {
            get
            {
                return this.ClientSize.Width;
            }
            /*
            set
            {
                Size newsize = new Size(value, ClientSize.Height);
                this.ClientSize = newsize;
            }
            */
        }

        // 客户区高度
        internal int ClientHeight
        {
            get
            {
                return this.ClientSize.Height;
            }
            /*
            set
            {
                Size newsize = new Size(ClientSize.Width, value);
                this.ClientSize = newsize;
            }
            */
        }

#if REMOVED
        // 文档发生改变
        internal void FireTextChanged(bool changed = true)
        {
            // this.Changed = changed;
            if (this.DesignMode == false)
                base.Changed = changed;

            EventArgs e = new EventArgs();
            this.OnTextChanged(e);
        }
#endif
        Hashtable _labelCache = new Hashtable();

        public void ClearLabelCache()
        {
            _labelCache?.Clear();
        }

        string GetCachedLabel(string field_name)
        {
            if ((field_name?.Length ?? 0) != 3)
                return "";
            var value = _labelCache[field_name] as string;
            if (value == null)
            {
                value = GetLabel(field_name);
                if (value == null)
                    value = "";
                if (_labelCache.Count > 2000)
                    _labelCache.Clear();
                _labelCache[field_name] = value;
            }

            return value;
        }

        // 从配置信息中得到一个字段的指定语言版本的标签名称
        // parameters:
        //		strFieldName	字段名
        //		strLang	语言版本
        // return:
        //		找不到返回"???"，找到返回具体的标签信息
        internal string GetLabel(string strFieldName)
        {
            if (this.MarcDefDom == null)
                return "????";

            XmlNode nodeProperty = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Property");
            if (nodeProperty == null)
                return "???";

            // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
            // parameters:
            //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
            string strValue = DomUtil.GetXmlLangedNodeText(
        this.Lang,
        nodeProperty,
        "Label",
        true);

            if (String.IsNullOrEmpty(strValue) == true)
                strValue = "???";


            return strValue;
        }

#if NO
		// 把错误信息写到日志文件里
		public void WriteErrorInfo(string strText)
		{
			string strOutputFileName = "I:\\debug.txt";//Environment.CurrentDirectory + "\\" + "debug.txt"; // 测试用

			string strTime = DateTime.Now.ToString();
			StreamUtil.WriteText(strOutputFileName, 
				strTime + " " + strText + "\r\n");
		}
#endif
        /// <summary>
        /// 如果必要，滚动文档，确保输入焦点位置位于可见范围
        /// </summary>
        public void EnsureVisible()
        {
            this.EnsureCaretVisible();
        }

#if REMOVED
        // 使字段的某区域可见
        // parameter:
        //		nCol	列号 
        //				0 字段说明;
        //				1 字段名;
        //				2 字段指示符
        //				3 字段值
        /// <summary>
        /// 如果必要滚动文档，让指定的位置处在可见位置
        /// </summary>
        /// <param name="nFieldIndex">字段下标</param>
        /// <param name="nCol">列号。列号的用法：0: 字段说明; 1: 字段名; 2: 字段指示符; 3: 字段值</param>
        /// <param name="rectCaret">插入符外围矩形</param>
        public void EnsureVisible(int nFieldIndex,
            int nCol,
            Rectangle rectCaret)
        {
            Debug.Assert(nFieldIndex >= 0 && nFieldIndex < this.record.Fields.Count, $"nFieldIndex 值 {nFieldIndex} 不在合法范围 0-{this.record.Fields.Count} 内");
            Debug.Assert(nCol == 0
                || nCol == 1
                || nCol == 2
                || nCol == 3, "nCol参数值无效");

            Field field = this.record.Fields[nFieldIndex];

            int nDelta = this.DocumentOrgY
                + this.TopBlank
                + this.record.GetFieldsHeight(0, nFieldIndex)
                + rectCaret.Y;

            if (nDelta + rectCaret.Height >= this.ClientHeight)
            {
                if (rectCaret.Height >= this.ClientHeight)
                    this.DocumentOrgX = this.DocumentOrgY - (nDelta + rectCaret.Height) + this.ClientHeight + /*调整系数*/ (rectCaret.Height / 2) - (this.ClientHeight / 2);
                else
                    this.DocumentOrgY = this.DocumentOrgY - (nDelta + rectCaret.Height) + this.ClientHeight;
            }
            else if (nDelta < 0)
            {
                if (rectCaret.Height >= this.ClientHeight)
                    this.DocumentOrgY = this.DocumentOrgY - (nDelta) - /*调整系数*/ ((rectCaret.Height / 2) - (this.ClientHeight / 2));
                else
                    this.DocumentOrgY = this.DocumentOrgY - (nDelta);
            }
            else
            {
                // y不需要卷滚
            }

            ////
            // 水平方向
            nDelta = 0;

            if (nCol == 0)
            {
                nDelta = this.DocumentOrgX
                    + this.LeftBlank
                    + rectCaret.X;
            }
            else if (nCol == 1)
            {
                nDelta = this.DocumentOrgX
                    + this.LeftBlank
                    + this.record.NameCaptionTotalWidth
                    + rectCaret.X;
            }
            else if (nCol == 2)
            {
                nDelta = this.DocumentOrgX
                    + this.LeftBlank
                    + this.record.NameCaptionTotalWidth
                    + this.record.NameTotalWidth
                    + rectCaret.X;
            }
            else if (nCol == 3)
            {
                nDelta = this.DocumentOrgX
                    + this.LeftBlank
                    + this.record.NameCaptionTotalWidth
                    + this.record.NameTotalWidth
                    + this.record.IndicatorTotalWidth
                    + rectCaret.X;
            }

            if (nDelta + rectCaret.Width >= this.ClientWidth)
            {
                if (rectCaret.Width >= this.ClientWidth)
                    this.DocumentOrgX = this.DocumentOrgX - (nDelta + rectCaret.Width) + this.ClientWidth + /*调整系数*/ (rectCaret.Width / 2) - (this.ClientWidth / 2);
                else
                    this.DocumentOrgX = this.DocumentOrgX - (nDelta + rectCaret.Width) + this.ClientWidth;
            }
            else if (nDelta < 0)
            {
                if (rectCaret.Width >= this.ClientWidth)
                    this.DocumentOrgX = this.DocumentOrgX - (nDelta) - /*调整系数*/ ((rectCaret.Width / 2) - (this.ClientWidth / 2));
                else
                    this.DocumentOrgX = this.DocumentOrgX - (nDelta);
            }
            else
            {
                // x不需要卷滚
            }

        }

#endif

        // 修改字段名后要变换说明信息
        /// <summary>
        /// 修改字段名
        /// </summary>
        /// <param name="field">字段对象</param>
        /// <param name="strNewName">要修改成的字段名</param>
        public void ChangeFieldName(Field field,
            string strNewName)
        {
            field.Name = strNewName;
#if REMOVED
            // 此处应使用Name，让界面跟着改变
            string strOldName = field.Name;
            if (strOldName != strNewName)
            {
                // 旧字段名是控制字段，新字段名是非控制字段 的情况
                if (Record.IsControlFieldName(strOldName) == true
                    && Record.IsControlFieldName(strNewName) == false)
                {
                    field.Name = strNewName;
                    string strAllValue = field.ValueKernel;
                    if (strAllValue.Length < 2)
                        strAllValue = strAllValue + new string(' ', 2 - strAllValue.Length);

                    field.Indicator = strAllValue.Substring(0, 2);
                    if (strAllValue.Length > 2)
                        field.Value = strAllValue.Substring(2);
                    else
                        field.Value = "";
                }
                else if (Record.IsControlFieldName(strOldName) == false
                    && Record.IsControlFieldName(strNewName) == true)
                {
                    // 旧字段名是非控制字段，新字段名是控制字段 的情况

                    field.Name = strNewName;
                    string strValue = this.FocusedField.Indicator + this.FocusedField.ValueKernel;
                    field.Indicator = "";
                    field.Value = strValue;
                }
                else
                {
                    field.Name = strNewName;
                    if (StringUtil.IsPureNumber(strNewName) == true)
                    {
                        if (Record.IsControlFieldName(strNewName) == true)
                        {
                            if (field.Indicator != "")
                                field.Indicator = "";
                        }
                        else
                        {
                            if (field.Indicator.Length != 2)
                            {
                                if (field.Indicator.Length > 2)
                                    field.Indicator = field.Indicator.Substring(0, 2);
                                else
                                    field.Indicator = field.Indicator.PadRight(2, ' ');
                            }
                        }
                    }
                }
            }

#endif
        }

        #endregion

        #region 处理右键菜单的函数

        internal static void TextToClipboard(string strText)
        {
            StringUtil.RunClipboard(() => { Clipboard.SetDataObject(strText); });
        }

        [Serializable()]
        public class MarcEditorData
        {
            public string Text { get; set; }

            public MarcEditorData(string text)
            {
                this.Text = text;
            }
        }

        // 将文本变换为更为通俗的格式，连同原先的机内格式，作为两种格式一并进入 Windows 剪贴板
        internal static void TextToClipboardFormat(string strText)
        {
            // Make a DataObject.
            DataObject data_object = new DataObject();

            // Add the data in various formats.
            // 普通格式
            data_object.SetData(DataFormats.UnicodeText, strText
                .Replace(Record.SUBFLD.ToString(), "$$")    // $
                .Replace(Record.FLDEND.ToString(), "\r\n")  // #
                .Replace(Record.RECEND.ToString(), "***")); // *
            // 专用格式
            data_object.SetData(new MarcEditorData(strText));

            // Place the data in the Clipboard.
            StringUtil.RunClipboard(() =>
            {
                Clipboard.SetDataObject(data_object);
            });
        }

        internal static string ClipboardToText()
        {
            string result = "";

            StringUtil.RunClipboard(() =>
            {
                IDataObject ido = Clipboard.GetDataObject();
                if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
                    result = "";
                else
                    result = (string)ido.GetData(DataFormats.UnicodeText);
            });

            return result;
        }

        // 检查 Windows 剪贴板中的数据，优先选用 MarcEditorData 类型的数据。
        // 如果不存在 MarcEditorData 类型的数据，则观察普通格式的数据中是否具备回车换行符号，针对性加以变换处理
        internal static string ClipboardToTextFormat(bool replace = true)
        {
            string result = "";

            StringUtil.RunClipboard(() =>
            {
                IDataObject ido = Clipboard.GetDataObject();

                if (ido == null)
                    return;

                if (ido.GetDataPresent(typeof(MarcEditorData)) == true)
                {
                    MarcEditorData data = (MarcEditorData)ido.GetData(typeof(MarcEditorData));
                    result = data?.Text;
                    return;
                }

                if (ido.GetDataPresent(DataFormats.UnicodeText) == true)
                {
                    result = (string)ido.GetData(DataFormats.UnicodeText);
                    return;
                }

                if (ido.GetDataPresent(DataFormats.Text) == true)
                {
                    result = (string)ido.GetData(DataFormats.Text);
                    return;
                }

                result = null;
            });

            // 2024/6/15
            if (replace)
            {
                if (result == null)
                    return null;

                // 询问是否要替换 \r\n
                if (result.Contains("\n"))
                {
                    // return:
                    //      null    用户取消对话框
                    //      其他    所输入的值
                    var ret = ListDialog.GetInput(Application.OpenForms[0],
                        "文本中出现了回车换行符号",
                        "请问如何处理?",
                        new string[] { "多个字段", "单个字段" },
                        0);
                    if (ret == null)
                        return null;
                    if (ret == "多个字段")
                        result = result.Replace("\r\n", "\n").Replace("\n", MarcQuery.FLDEND);
                    else
                        result = result.Replace("\r", "*").Replace("\n", "*");
                }

                return result.Replace("ǂ", MarcQuery.SUBFLD)
                    .Replace("$$", MarcQuery.SUBFLD)
                    ;
            }
            return result;
        }

        private void Menu_SelectAll(object sender,
            System.EventArgs e)
        {
            Debug.Assert(this.record.Fields.Count > 0, "");

            this.SelectAll();

            FireSelectedFieldChanged();
        }

        // 属性
        private void Property_menu(object sender,
            System.EventArgs e)
        {
            /*
            // 测试用
			//string strMessage = "SelectedFieldIndices.Count=" + Convert.ToString(this.SelectedFieldIndices.Count) + "\r\n";
			//strMessage += "nStartFieldIndex=" + Convert.ToString(this.nStartFieldIndex)+ "\r\n";

            if (this.FocusedField != null)
			    MessageBox.Show(this,this.FocusedField.m_strValue);
            */

            using (PropertyDlg dlg = new PropertyDlg())
            {
                GuiUtil.AutoSetDefaultFont(dlg);
                dlg.MarcEditor = this;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
            }
        }

        void menuItem_Undo(object sender, EventArgs e)
        {
            if (this.CanUndo())
            {
                this.Undo();
            }
        }

        void menuItem_Redo(object sender, EventArgs e)
        {
            if (this.CanRedo())
            {
                this.Redo();
            }
        }

        // 剪切
        private void menuItem_Cut(object sender,
            System.EventArgs e)
        {
            this.SoftlyCut();
        }

#if REMOVED
        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            // 去掉Control/Shift/Alt 以后的纯净的键码
            Keys pure_key = (keyData & (~(Keys.Control | Keys.Shift | Keys.Alt)));

            if (this.curEdit._k)
            {
                this.DoCtrlK(pure_key);
                this.curEdit._k = false;
                return true;
            }

            // Ctrl + K
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.K)
            {
                this.curEdit._k = true;
                return true;
            }

            return base.ProcessCmdKey(ref m, keyData);
        }
#endif

#if REMOVED
        // 当小 edit 隐藏时，通过这里接管Ctrl+各种键
        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
            Keys keyData)
        {
            // 去掉Control/Shift/Alt 以后的纯净的键码
            Keys pure_key = (keyData & (~(Keys.Control | Keys.Shift | Keys.Alt)));

            if (Control.ModifierKeys == Keys.Control
                && (pure_key == Keys.Enter || pure_key == Keys.LineFeed))
            {
                // TODO: 回车换行
                return true;
            }

            /*
            // Ctrl + M
            if (Control.ModifierKeys == Keys.Control
                && pure_key == Keys.M)
            {
                EditControlTextToItem();
                // 调定长模板
                GetValueFromTemplate();
                return true;
            }
            */

            // Ctrl + X
            // ? 好像不管用
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.X)
            {
                this.menuItem_Cut(null, null);
                return true;
            }

            // Ctrl + C
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.C)
            {
                this.menuItem_Copy(null, null);
                return true;
            }

            // Ctrl + V
            if ((keyData & Keys.Control) == Keys.Control
                && pure_key == Keys.V)
            {
                this.menuItem_PasteOverwrite(null, null);
                return true;
            }

            // Ctrl+U 校验数据
            if (keyData == (Keys.U | Keys.Control))
            {
                GenerateDataEventArgs ea = new GenerateDataEventArgs();
                this.OnVerifyData(ea);
                return true;
            }

            // 其余未处理的键
            if ((keyData & Keys.Control) == Keys.Control)
            {
                bool bRet = this.OnControlLetterKeyPress(pure_key);
                if (bRet == true)
                    return true;
            }

            // Del 删除所选的若干字段
            if (keyData == Keys.Delete)
            {
                int start_line = this.SelectedFieldIndices.Min();
                DeleteFieldWithDlg(false);
                if (start_line < this.Record.Fields.Count)
                    AddSelectedField(start_line, start_line, true);
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }
#endif

        public void DoCtrlK(Keys key)
        {
            /*
            // 复制当前 字段 到后面
            if (key == Keys.F)
            {
                menuItem_Copy(this, null);
                menuItem_PasteInsert_InsertAfter(this, null);
            }

            // 复制当前 子字段 到后面
            if (key == Keys.S)
            {
                this.curEdit.DupCurrentSubfield();
            }
            */

            // 繁体 --> 简体
            if (key == Keys.J)
            {
                this.T2S();
            }

            // 简体 --> 繁体
            if (key == Keys.L)
            {
                this.S2T();
            }

            // 插入 {cr:CALIS}
            if (key == Keys.C)
            {
                this.SoftlyPaste("{cr:CALIS}");
            }

            // 插入 {cr:NLC}
            if (key == Keys.N)
            {
                this.SoftlyPaste("{cr:NLC}");
            }
        }

        // 复制
        private void menuItem_Copy(object sender,
            System.EventArgs e)
        {
            this.Copy();
        }

        static string ConvertMarcXmlString(string strXml)
        {
            int nRet = MarcUtil.Xml2Marc(strXml, false,
                null,
                out string marcSyntax,
                out string strMARC,
                out string strError);
            if (nRet == -1)
            {
                throw new Exception(strError);
            }

            return strMARC;
        }

        // 2022/1/2
        /*
FMT	BK
LDR	-----nam0 22-----   450 
001	011528318
005	20211208153309.0
010	|a 978-7-5001-6208-7 |d CNY32.00
100	|a 20211208d2022    em y0chiy50      ea
1011	|a chi |c fre
102	|a CN |b 110000
105	|a a   z   000fy
106	|a r
2001	|a 小王子 |b 专著 |d Le petit prince |f (法)安托万·德·圣·埃克苏佩里著 |g 李玉民译 |z fre
210	|a 北京 |c 中译出版社 |d 2022
215	|a 10,112页 |c 图 |d 21cm
096	|a I565.8 |b aks
CAT	|a ZWCFWB11 |b 01 |c 20211208 |l NLC01 |h 1533
OWN	|a ZC161
SYS	011528318
         * */
        static string ConvertNlcMarcString(string strMARC)
        {
            string strResult = strMARC.Replace("\r\n", "\r");
            string[] lines = strResult.Split(new char[] { '\r' });
            string strHeader = "01034nam0 2200277   45  ";   // 头标区缺省内容
            string strTotal = "";
            foreach (string s in lines)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;

                {
                    string strTemp = s;
                    if (strTemp[3] == '\t' && strTemp.Length >= 4)
                        strTemp = strTemp.Remove(3, 1).Insert(3, "  ");
                    else if (strTemp[4] == '\t' && strTemp.Length >= 5)
                        strTemp = strTemp.Remove(4, 1).Insert(4, " ");

                    if (strTemp.Length >= 5 && MarcUtil.IsControlFieldName(strTemp.Substring(0, 3)) == true)
                        strTemp = strTemp.Remove(3, 2);

                    if (strTemp.StartsWith("LDR"))
                    {
                        strTemp = strTemp.Substring(5);
                        if (strTemp.Length > 24)
                            strTemp = strTemp.Substring(0, 24);
                        else if (strTemp.Length < 24)
                            strTemp = strTemp.PadRight(24, ' ');

                        // 放到最开头
                        strHeader = strTemp;
                    }
                    else
                        strTotal += RemoveBlank(strTemp) + new string((char)30, 1);
                }
            }

            // 2024/11/27
            // 删除 | 左侧的 tab 键
            strTotal = strTotal.Replace("\t|", "|");

            return strHeader + strTotal.Replace("|", new string((char)31, 1));
        }

        // 移走 '|' 左右的空格
        static string RemoveBlank(string text)
        {
            StringBuilder result = new StringBuilder();
            int offs = -1;  // 当前字符处于当前子字段内哪个偏移位置
            int dollar_index = -1;  // 子字段的序号
            foreach (char ch in text.ToCharArray())
            {
                if (ch == '|')
                {
                    dollar_index++;
                    // 删除 | 前面的一个空格(从第二个子字段开始)
                    if (dollar_index > 0
                        && result.Length > 0
                        && result[result.Length - 1] == ' ')
                        result.Remove(result.Length - 1, 1);
                    offs = 0;
                }
                else if (offs >= 0)
                    offs++;

                if (offs == 2 && ch == ' ')
                {

                }
                else
                    result.Append(ch);
            }

            return result.ToString().TrimEnd();
        }

        /* http://z39.tcmarc.net/Index.asp?one=1
001    012012004066
005    20121222232554.0
010    ■a978-988-19901-1-2■dCNY48.00
100    ■a20121223d2011    em y0chiy0110    ea
101 0  ■achi
102    ■aCN■b810000
105    ■ah   z   000yy
106    ■ar
200 1  ■a评说 鼓励 鞭策■9ping shuo gu li bian ce■f张脉峰主编
210    ■a[香港]■c中国文化出版社■d2011
215    ■a139页■c摹真■d24cm
312    ■a封面题名:陆其明六十年海军作品选评说 鼓励 鞭策
517 1  ■a陆其明六十年海军作品选评说鼓励鞭策■9lu qi ming liu shi nian hai jun zuo pin xuan ping shuo gu li bian ce
606 0  ■a中国文学■x当代文学■x文学评论
690    ■aI206.7■v4
701  0 ■a张脉峰■9zhang mai feng■4主编
801  0 ■aCN■b91MARC■c20121223
         * */
        static string ConvertTcmarcMarcString(string strMARC)
        {
            string strResult = strMARC.Replace("\r\n", "\r");
            string[] lines = strResult.Split(new char[] { '\r' });
            string strTotal = "01034nam0 2200277   45  ";   // 头标区
            foreach (string s in lines)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;
                if (s.Length >= 7)
                {
                    string strTemp = s.Remove(3, 1).Remove(5, 1);
                    if (strTemp.Length >= 5 && MarcUtil.IsControlFieldName(strTemp.Substring(0, 3)) == true)
                        strTemp = strTemp.Remove(3, 2);

                    strTotal += strTemp + new string((char)30, 1);
                }
                else
                {
                    string strTemp = s.PadRight(7, ' ');
                    strTotal += strTemp.Remove(3, 1).Remove(5, 1) + new string((char)30, 1);
                    // strTotal += s + s.Remove(3, 1).Remove(5, 1) + new string((char)30, 1);
                }
            }

            strResult = strTotal.Replace("■", new string((char)31, 1)).Replace("|", new string((char)31, 1));
            return strResult;
        }

        // 将工作单格式的字符串转换为机内格式的字符串
        // parameters:
        //      get_first_record    是否只取第一个记录。也就是说遇到 *** 行的时候终止处理
        /*
01310nam0 2200157   45__
-01/132.147.160.100/读者库/ctlno/0002118|7badce52100000002b
-01/读者库(临时)/ctlno/0000002|883947414000000022
100  ǂaD000002ǂc工作证号
110  ǂa教工ǂd在职
2001 ǂa测试姓名ǂAWu Mei Juǂb女
300  ǂa中文系
400  ǂa6550171ǂc家庭住址
980  ǂa2006ǂb1ǂa2008ǂb11ǂa2009ǂb6ǂa2010ǂb5ǂa2011ǂb9ǂa2013ǂb3ǂa2014ǂb3
982  ǂa教工ǂAJiao Gongǂc3
986  ǂaA0170967ǂfǂt20140109ǂv20140331ǂaA0137758ǂfǂt20140109ǂv20140331ǂaA0154258ǂfǂt20140109ǂv20140331
989  ǂaA0079821ǂt20060219ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0090491ǂt20080107ǂaA0083852ǂt20090608ǂaA0133097ǂt20090612ǂaA0133097ǂt20090612ǂaA0133097ǂt20090612ǂaA0133097ǂt20090612ǂaA0133097ǂt20090612ǂaA0041328ǂt20100702ǂaA0000232ǂt20100702ǂaA0086399ǂt20100702ǂaA0086503ǂt20100702ǂaA0010970ǂt20100702ǂaA0109749ǂt20110617ǂaA0097770ǂt20110617ǂaA0146162ǂt20110617ǂaA0146160ǂt20110617ǂaA0146144ǂt20110617ǂaA0107659ǂt20110703ǂaA0124241ǂt20110703ǂaA0109749ǂt20110703ǂaA0087051ǂt20111129ǂaA0170776ǂt20130617ǂaA0010989ǂt20130617ǂaA0055348ǂt20130617ǂaA0170967ǂt20140109ǂaA0137758ǂt20140109ǂaA0154258ǂt20140109
997  ǂa|吴梅菊||,|ǂh1b0463476e6d0a593b1c1b79abf082a2ǂv0.04
***
** */
        static string ConvertWorksheetMarcString(string strMARC,
            bool get_first_record = true)
        {
            string strResult = strMARC.Replace("\r\n", "\r");
            string[] lines = strResult.Split(new char[] { '\r' });
            string strTotal = "";
            int i = 0;
            foreach (string s in lines)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;

                if (get_first_record && s == "***")
                    break;

                if (i == 0)
                {
                    string strTemp = s.PadRight(24, ' ').Replace("_", " ");
                    strTotal += strTemp;
                }
                else
                {
                    if (s.Length >= 5)
                    {
                        strTotal += s + new string((char)30, 1);
                    }
                    else
                    {
                        string strTemp = s.PadRight(5, ' ');
                        strTotal += strTemp + new string((char)30, 1);
                    }
                }
                i++;
            }

            strResult = strTotal.Replace("ǂ", new string((char)31, 1));
            return strResult;
        }

        // 从 XML 粘贴整个记录
        void menuItem_PasteFromMarcXml(object sender,
            System.EventArgs e)
        {
            try
            {
                // bool bHasFocus = this.Focused;

                string strFieldsMarc = MarcEditor.ClipboardToText();
                strFieldsMarc = ConvertMarcXmlString(strFieldsMarc);

                this.SoftlyPaste(strFieldsMarc);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"异常: {ex.Message}");
            }
        }

        // 2024/5/20
        // 从 机内格式 粘贴整个记录
        void menuItem_PasteFromJinei(object sender, EventArgs e)
        {
            string strFieldsMarc = MarcEditor.ClipboardToTextFormat();
            this.Marc = strFieldsMarc;
        }

        // 从 工作单 粘贴整个记录
        void menuItem_PasteFromWorksheet(object sender,
            System.EventArgs e)
        {
            // bool bHasFocus = this.Focused;

            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertWorksheetMarcString(strFieldsMarc);

            this.SoftlyPaste(strFieldsMarc);

        }

        // 从 NLC 粘贴整个记录
        void menuItem_PasteFromNlcMarc(object sender,
            System.EventArgs e)
        {
            // bool bHasFocus = this.Focused;

            // 先删除所有字段
            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertNlcMarcString(strFieldsMarc);

            this.SoftlyPaste(strFieldsMarc);
        }


        // 从 tcmarc 粘贴整个记录
        void menuItem_PasteFromTcMarc(object sender,
            System.EventArgs e)
        {
            bool bHasFocus = this.Focused;

            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertTcmarcMarcString(strFieldsMarc);

            this.SoftlyPaste(strFieldsMarc);
        }

        // 从dp2OPAC Web粘贴整个记录
        void menuItem_PasteFromDp2OPAC(object sender,
            System.EventArgs e)
        {
            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertWebMarcString(strFieldsMarc);

            this.SoftlyPaste(strFieldsMarc);
        }

        static string ConvertWebMarcString(string strMARC)
        {
            string strResult = strMARC.Replace("\r", "");
            strResult = strResult.Replace("\n", "");
            strResult = strResult.Replace("\t", "");
            strResult = strResult.Replace(" ", "");

            strResult = strResult.Replace("_", " ");
            strResult = strResult.Replace("‡", new string((char)31, 1));
            strResult = strResult.Replace("¶", new string((char)30, 1));
            return strResult;
        }

        // 不支持在多选时粘贴覆盖，因为不明确在哪个字段上粘贴覆盖
        // 粘贴覆盖
        private void menuItem_PasteOverwrite(object sender,
            EventArgs e)
        {
            string strFieldsMarc = MarcEditor.ClipboardToTextFormat();
            this.SoftlyPaste(strFieldsMarc);
        }

        List<Field> GetFields(IEnumerable<int> indices)
        {
            List<Field> results = new List<Field>();
            foreach (var v in indices)
            {
                results.Add(this.Record.Fields[v].Clone());
            }
            return results;
        }

        // 检查一个整数集合是否为连续增量的值
        static bool IsIndexContinue(IEnumerable<int> indices)
        {
            int prev = -1;
            int i = 0;
            foreach (var v in indices)
            {
                if (i > 0 && v != prev + 1)
                    return false;
                i++;
            }

            return true;
        }

        // 前插字段
        private void InsertBeforeFieldNoDlg(object sender,
            System.EventArgs e)
        {
            // Debug.Assert(this.SelectedFieldIndices.Count == 1, "在'前插'时，SelectedFieldIndices数量必须为1");

            var index = this.CaretFieldIndex;

            if (index == 0/*this.FocusedField?.IsHeader ?? false*/)
            {
                MessageBox.Show(this, "在头标区前不能插入字段。");
                return;
            }

            bool bControlField = Record.IsControlFieldName(this.DefaultFieldName);
            string strDefaultValue = "";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";


            this.InsertField(index, // this.FocusedFieldIndex,
                this.DefaultFieldName,
                "  ", //" " strIndicator
                strDefaultValue);
        }

        // 给当前字段的后面新增一个字段
        internal void InsertAfterFieldWithoutDlg()
        {
            // Debug.Assert(this.SelectedFieldIndices.Count == 1, "在'后插'时，SelectedFieldIndices数量必须为1");

            var index = this.CaretFieldIndex;

            bool bControlField = Record.IsControlFieldName(this.DefaultFieldName);
            string strDefaultValue = "";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";

            // 调InsertAfterField，把界面也管了
            this.InsertFieldAfter(index,
                this.DefaultFieldName,
                "  ",
                strDefaultValue);

            // 将 caret 定位刚插入的内容的最后一个字符以右。注意，要在字段结束符以左。
            var caret_offs = this.SelectionEnd - 1;
            this.Select(caret_offs, caret_offs, caret_offs);
            this.EnsureVisible();
        }

        // 后插字段
        private void InsertAfterFieldWithoutDlg(object sender,
            System.EventArgs e)
        {
            this.InsertAfterFieldWithoutDlg();
        }

        // 插入字段(有对话框提示)
        private void InsertField(object sender, EventArgs e)
        {
#if OLD
            if (this.SelectedFieldIndices.Count > 1)
            {
                MessageBox.Show(this, "多选字段的状态下不能插入新字段");
                return;
            }
            // parameters:
            //      nAutoComplate   0: false; 1: true; -1:保持当前记忆状态
            InsertField(this.FocusedFieldIndex, -1, -1);  // true, false
#endif
            InsertField(this.CaretFieldIndex, -1, -1);  // true, false
        }

        // 兼容以前脚本，保留此函数
        // 2025/1/10
        // 从当前插入符位置把字段内容一劈为二。
        // 当前字段内容只留下一劈为二的左侧部分。
        // 下方新增一个字段，内容为一劈为二的右侧部分。
        // parameters:
        //      nFieldIndex 字段 index。如果为 -1，表示为当前插入符所在的字段
        //      right_prev_text 分割后，要自动在右边一段文字头部增加的内容。增加后一起算作右侧内容
        public bool SplitField(int nFieldIndex = -1,
            string right_prev_text = "")
        {
            if (nFieldIndex == -1)
                nFieldIndex = this.CaretFieldIndex;

            if (nFieldIndex == -1)
                return false;

            if (this.CaretFieldRegion != FieldRegion.Content)
                return false;
            //if (this._focusCol != 3)
            //    return false;

            if (this.GetDomRecord().LocateField(this.CaretOffset,
                out int field_index,
                out int offs_in_field) == false)
                return false;


            var current_field = this.Record.Fields[nFieldIndex];
            var start_field_name = "000";
            if (nFieldIndex == 0)   // 头标区不允许切割
                return false;
            else
                start_field_name = current_field.Name;

            var indicator = current_field.Indicator;

            int name_indicator_length = current_field.Name.Length + current_field.Indicator.Length;
            string left = current_field.Value.Substring(0, offs_in_field - name_indicator_length);
            string right = current_field.Value.Substring(offs_in_field - name_indicator_length);
            //this.curEdit.GetLeftRight(out string left,
            //    out string right);

            if (string.IsNullOrEmpty(right_prev_text) == false)
                right = right_prev_text + right;

            current_field.Value = left;
            //this.curEdit.TextWithHeight = left;
            //this.Flush();    // 促使通知外界

            var new_field = this.InsertFieldAfter(nFieldIndex,
                start_field_name,
                current_field.Indicator,
                right);

            // 设焦点位置
            if (new_field != null)
                this.SetActiveField(new_field, 3);

            this.EnsureVisible();
            return true;
        }


        // 插入字段
        // parameters:
        //      nInsertBefore   0: false; 1: true; -1:保持当前记忆状态 2010/12/12
        //      nAutoComplete   0: false; 1: true; -1:保持当前记忆状态 2008/7/29
        /// <summary>
        /// 插入新字段
        /// </summary>
        /// <param name="nFieldIndex">字段插入位置下标</param>
        /// <param name="nInsertBefore">是否为插入到 nFieldIndex 位置的前面? 0: 否; 1: 是; -1:保持当前记忆状态</param>
        /// <param name="nAutoComplete">是否自动完成? 0: 否; 1: 是; -1:保持当前记忆状态</param>
        /// <returns>false: 放弃插入新字段; true: 成功</returns>
        public bool InsertField(int nFieldIndex,
            int nInsertBefore,
            int nAutoComplete)
        {
            NewFieldDlg dlg = new NewFieldDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            if (nInsertBefore == -1)
            {
                if (this.AppInfo == null)
                    dlg.InsertBefore = this.m_bInsertBefore;
                else
                    dlg.InsertBefore = this.AppInfo.GetBoolean("marceditor",
                        "newfielddlg_insertbefore",
                        false);
            }
            else
                dlg.InsertBefore = (nInsertBefore == 1 ? true : false);

            // dlg.InsertBefore = bInsertBefore;
            if (nAutoComplete == -1)
            {
                if (this.AppInfo == null)
                    dlg.AutoComplete = this.m_bAutoComplete;
                else
                    dlg.AutoComplete = this.AppInfo.GetBoolean("marceditor",
                        "newfielddlg_autocomplete",
                        false);
            }
            else
                dlg.AutoComplete = (nAutoComplete == 1 ? true : false);

            // 2024/6/14
            var start_field_name = this.DefaultFieldName;
            if (nFieldIndex == 0)
                start_field_name = "001";
            else
                start_field_name = GetRefFieldName();

            string GetRefFieldName()
            {
                if (nFieldIndex >= this.Record.Fields.Count)
                {
                    if (this.Record.Fields.Count <= 1)
                        return this.DefaultFieldName;
                    return this.Record.Fields[this.Record.Fields.Count - 1].Name;
                }
                return this.Record.Fields[nFieldIndex].Name;
            }

            dlg.FieldName = start_field_name; //  this.DefaultFieldName;
            dlg.MarcDefDom = this.MarcDefDom;
            dlg.Lang = this.Lang;

            /*
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
             * */

            if (this.AppInfo != null)
                this.AppInfo.LinkFormState(dlg, "marceditor_newfielddlg_state");
            dlg.ShowDialog(this);
            if (this.AppInfo != null)
                this.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return false;

            // 记忆
            if (nAutoComplete == -1)
            {
                if (this.AppInfo == null)
                    this.m_bAutoComplete = dlg.AutoComplete;
                else
                    this.AppInfo.SetBoolean("marceditor",
                        "newfielddlg_autocomplete",
                        dlg.AutoComplete);
            }

            if (nInsertBefore == -1)
            {
                if (this.AppInfo == null)
                    this.m_bInsertBefore = dlg.InsertBefore;
                else
                    this.AppInfo.SetBoolean("marceditor",
                        "newfielddlg_insertbefore",
                        dlg.InsertBefore);
            }

            bool bControlField = Record.IsControlFieldName(dlg.FieldName);
            string strDefaultValue = "";
            string strIndicator = "  ";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";
            else
                strIndicator = "";

            // 获得宏值
            // parameters:
            //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
            // return:
            //      -1  error
            //      0   not found 
            //      1   found
            int nRet = GetDefaultValue(
                0,  // index,
                dlg.FieldName,
                "",
                out List<string> results,
                out string strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            if (results != null
                && results.Count > 0)
            {
                strDefaultValue = results[0];
                if (bControlField == false)
                    SplitDefaultValue(strDefaultValue,
                        out strIndicator,
                        out strDefaultValue);
            }

            if (dlg.InsertBefore == true
                || nFieldIndex >= this.Record.Fields.Count)
            {
                if (nFieldIndex == 0)
                {
                    MessageBox.Show(this, "头标区前面不能插入新字段");
                    return false;
                }

                var ret = this.InsertField(nFieldIndex,
                    dlg.FieldName,
                    strIndicator,//"  ",//指示符
                    strDefaultValue);
            }
            else
            {
                var ret = this.InsertFieldAfter(nFieldIndex,
                    dlg.FieldName,
                    strIndicator,//"  ",//指示符
                    strDefaultValue);
            }

            // 将 caret 定位刚插入的内容的最后一个字符以右。注意，要在字段结束符以左。
            var caret_offs = this.SelectionEnd - 1;
            this.Select(caret_offs, caret_offs, caret_offs);
            this.EnsureVisible();
            return true;
        }

        // 2011/8/16
        // 从缺省值字符串中分离出字段指示符和纯粹字段内容部分
        // 函数调用前，strText中可能含有指示符，也可能没有
        static void SplitDefaultValue(string strText,
                        out string strIndicator,
                        out string strContent)
        {
            strIndicator = "  ";
            strContent = "";

            if (string.IsNullOrEmpty(strText) == true)
                return;

            int nRet = strText.IndexOf((char)MarcUtil.SUBFLD);
            if (nRet == -1)
            {
                if (strText.Length < 2)
                {
                    strContent = strText;
                    return;
                }

                strIndicator = strText.Substring(0, 2);
                strContent = strText.Substring(2);
                return;
            }

            if (nRet >= 2)
            {
                strIndicator = strText.Substring(0, 2);
                strContent = strText.Substring(2);
                return;
            }

            strContent = strText;
        }

        // 追加字段到末尾
        private void AppendFieldNoDlg(object sender,
            System.EventArgs e)
        {
            bool bControlField = Record.IsControlFieldName(this.DefaultFieldName);
            string strDefaultValue = "";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";


            /*
            this.record.Fields.Add(this.DefaultFieldName,
                "  ",//strIndicator
                strDefaultValue,
                false);
            */
            AddField(this.DefaultFieldName,
                "  ",
                strDefaultValue,
                false);

            this.EnsureVisible();
        }

        #region 对外 API

        public void ClearFields()
        {
            this.GetDomRecord().Clear();
        }

        public Field RemoveField(int field_index)
        {
            var old_field = this.record.Fields[field_index].Clone();
            this.GetDomRecord().DeleteField(field_index);
            return old_field;
        }

        public Field InsertFieldAfter(int field_index,
            string field_name,
            string indicator,
            string default_value)
        {
            var old_count = this.record.Fields.Count;
            var ret = this.record.Fields.InsertAfter(field_index,
                field_name,
                indicator,
                default_value);
            return ret;
        }

        public Field InsertField(int field_index,
            string field_name,
            string indicator,
            string default_value)
        {
            var old_count = this.record.Fields.Count;
            var ret = this.record.Fields.Insert(field_index,
                field_name,
                indicator,
                default_value);
            return ret;
        }

        // 在末尾添加一个字段
        public Field AddField(string field_name,
            string indicator,
            string default_value,
            bool in_order)
        {
            int old_count = this.record.Fields.Count;
            var ret = this.record.Fields.Add(field_name,
    indicator,
    default_value,
    in_order);
            return ret;
        }

        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName)
        {
            return this.record.Fields.GetFirstSubfield(
        strFieldName,
        strSubfieldName);
        }

        public Field SetFirstSubfield(string strFieldName,
    string strSubfieldName,
    string strSubfieldValue)
        {
            int old_count = this.record.Fields.Count;
            var ret = this.record.Fields.SetFirstSubfield(strFieldName,
    strSubfieldName,
    strSubfieldValue,
    out Field old_field);
            var index = this.record.Fields.IndexOf(ret);
            return ret;
        }

        #endregion


        // return:
        //      false   放弃
        //      true    已经处理
        internal bool DeleteFieldWithDlg(bool show_dialog = true)
        {
            // Debug.Assert(this.SelectedFieldIndices.Count > 0, "在'删除'时，SelectedFieldIndices个数必须大于0");

            List<int> indices = new List<int>();

            var dom = this.GetDomRecord();
            if (this.HasSelection() == false)
            {
                indices.Add(this.CaretFieldIndex);
            }
            else
            {
                var ret = dom.LocateFields(dom.SelectionStart,
    dom.SelectionEnd,
    out int field_index,
    out int count);
                if (ret == false)
                {
                    MessageBox.Show(this, "尚未选择要删除的字段");
                    return false;
                }
                for (int i = 0; i < count; i++)
                {
                    indices.Add(field_index + i);
                }
            }

            return this.DeleteFields(indices, show_dialog);
        }


#if OLD_VERSION
        // 带对话框的删除字段
        // 注: 能走到这里调用本函数，selection 内文字已经被清除了?
        // return:
        //      false   放弃
        //      true    已经处理
        internal bool DeleteFieldWithDlg(bool show_dialog = true)
        {
            Debug.Assert(this.SelectedFieldIndices.Count > 0, "在'删除'时，SelectedFieldIndices个数必须大于0");

            var dom = this.GetDomRecord();
            var ret = dom.LocateFields(dom.CaretOffset,
dom.CaretOffset,
out int field_index,
out int count);
            if (ret == false)
            {
                MessageBox.Show(this, "尚未选择要删除的字段");
                return false;
            }

            if (show_dialog)
            {
                string strFieldInfo = "";
                if (count == 1)
                {
                    strFieldInfo = "'" + this.FocusedField.Name + "'";
                }
                else
                {
                    strFieldInfo = $"选中的 {count} 个";
                }

                string strText = "确实要删除"
                    + strFieldInfo
                    + "字段吗?";
                DeleteFieldDlg dlg = new DeleteFieldDlg();
                GuiUtil.AutoSetDefaultFont(dlg);

                dlg.Message = strText;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.Yes)
                    return false;
            }

            dom.DeleteField(field_index, count);
            return true;
        }
#endif

        // 删除字段
        internal void DeleteFieldWithDlg(object sender,
            System.EventArgs e)
        {
            this.DeleteFieldWithDlg();
        }

        private bool HasTemplateOrValueListDef(
            string strDefName,
            string strFieldName,
            string strSubFieldName,
            out string strError)
        {
            Debug.Assert(strFieldName != null, "strFieldName参数不能为null");
            Debug.Assert(strSubFieldName != null, "strSubFieldName参数不能为null");

            strError = "";

            // 检查MarcDefDom是否存在
            if (this.MarcDefDom == null)
            {
                strError = m_strMarcDomError;
                return false;
            }

            // 根据字段名找到配置文件中的该字段的定义
            XmlNode node = null;

            if (strSubFieldName == "" || strSubFieldName == "#indicator")
            {
                // 只找到字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']");
            }
            else
            {
                // 找到子字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubFieldName + "']");
            }

            if (node == null)
            {
                return false;   // not found def
            }

            XmlNodeList nodes = null;

            if (strDefName == "template")
            {
                // 下级有至少一个<Char>定义
                nodes = node.SelectNodes("Char");
            }
            else if (strDefName == "valuelist")
            {
                // 下级有<ValueList>定义
                nodes = node.SelectNodes("Property/ValueList");
            }
            else
            {
                throw new Exception("strDefName值应当为template和valuelist之一。");
            }

            if (nodes.Count >= 1)
                return true;

            return false;
        }

        // 探测当前位置是否存在定长模板定义
        // parameters:
        //      strCurName  当前所在位置的字段、子字段名
        // return:
        //      -1  不存在当前位置
        //      0   存在定义
        //      1   存在定义
        internal int HasTemplateOrValueListDef(
            string strDefName,
            out string strCurName)
        {
            strCurName = "";
            if (this.SelectedFieldIndices.Count > 1)
                return 0;

            if (this.FocusedField == null)
                return -1;
            Debug.Assert(this.FocusedField != null, "FocusedField不可能为null");

            string strFieldName = this.FocusedField?.Name;
            string strFieldValue = this.FocusedField?.Value;

            string strSubfieldName = "";
            string strSubfieldValue = "";

            int nSubfieldDupIndex = 0;

            if (this.m_nFocusCol == 1)
            {
                strSubfieldName = "";
            }
            else if (this.m_nFocusCol == 2)
            {
                strSubfieldName = "";
            }
            else if (this.m_nFocusCol == 3)
            {
                MarcEditor.GetCurrentSubfield(strFieldValue,
                    this.GetCurrentSelectionStart(),
                    out strSubfieldName,
                    out nSubfieldDupIndex,
                    out strSubfieldValue);
            }
            else
            {
                return 0;
            }


            if (strSubfieldName != "" && strSubfieldName != "#indicator")
                strCurName = strFieldName + new String(Record.KERNEL_SUBFLD, 1) + strSubfieldName;
            else
                strCurName = strFieldName;

            string strError;

            return HasTemplateOrValueListDef(
                strDefName,
                strFieldName,
                strSubfieldName,
                out strError) == true ? 1 : 0;
        }

        // 为了兼容以前的小 edit .SelectionStart
        int GetCurrentSelectionStart()
        {
            var dom = this.GetDomRecord();
            var ret = dom.LocateField(dom.SelectionStart,
                out int index,
                out int offs_in_field);
            if (ret == false)
                return 0;
            var field = dom.GetField(index);
            if (field.IsHeader)
                return offs_in_field;
            if (field.IsControlField)
            {
                if (offs_in_field >= 3)
                    return offs_in_field - 3;
                return offs_in_field;
            }

            if (offs_in_field >= 5)
                return offs_in_field - 5;
            if (offs_in_field >= 3)
                return offs_in_field - 3;
            return offs_in_field;
        }

        // 设置当前位置的缺省值。只取缺省值的第一个
        // parameters:
        //      bSimulate   如果==true，表示模拟获得缺省值，但是并不在当前位置插入值。注意，某些对种子号有推动作用的宏，这里只能取当前值而不能增量。
        //      index   如果有多个值，这里指定要设置的值的下标。必须在bSimulate == false时使用。如果bSimulate == true，则此参数应当使用-1，以明确表示不用它
        /// <summary>
        /// 设置当前位置的缺省值。
        /// 如果有多个缺省值，只取缺省值的第一个
        /// </summary>
        /// <param name="bSimulate">是否模拟获得缺省值。如果==true，表示模拟获得缺省值，但是并不在当前位置插入值。注意，某些对种子号有推动作用的宏，这里只能取当前值而不能增量</param>
        /// <param name="index">如果有多个值，这里指定要设置的值的下标。必须在 bSimulate == false 时使用。如果 bSimulate == true，则此参数应当使用-1，以明确表示不用它</param>
        /// <returns>返回模拟获得的缺省值</returns>
        public List<string> SetDefaultValue(bool bSimulate,
            int index)
        {
            if (bSimulate == true && index != -1)
            {
                Debug.Assert(false, "当bSimulate==true时，index必须为-1");
            }

            if (this.SelectedFieldIndices.Count > 1)
            {
                if (bSimulate == false)
                    Console.Beep();
                return new List<string>();
            }

            List<string> results = new List<string>();

            // 2024/7/17
            if (this.FocusedField == null)
            {
                Console.Beep();
                return new List<string>();
            }

            Debug.Assert(this.FocusedField != null, "FocusedField不可能为null");

            string strFieldName = this.FocusedField?.Name;
            string strFieldValue = this.FocusedField?.Value;

            string strSubfieldName = "";
            string strSubfieldValue = "";

            int nSubfieldDupIndex = 0;

            if (this.m_nFocusCol == 1)
            {
                strSubfieldName = "";
            }
            else if (this.m_nFocusCol == 2)
            {
                strSubfieldName = "#indicator";
            }
            else if (this.m_nFocusCol == 3)
            {
                MarcEditor.GetCurrentSubfield(
                    strFieldValue,
                    this.GetCurrentSelectionStart(),
                    out strSubfieldName,
                    out nSubfieldDupIndex,
                    out strSubfieldValue);
            }
            else
            {
                return new List<string>();
            }

            string strError;
            // string strOutputValue;
            int nRet = 0;

            if (strSubfieldName == "")
            {
                // 字段值或者指示符值情形

                // 获得宏值
                // parameters:
                //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
                // return:
                //      -1  error
                //      0   not found 
                //      1   found
                nRet = GetDefaultValue(
                    index,  // bSimulate,
                    strFieldName,
                    "",
                    out results,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    return new List<string>();

                // 如果当前插入符在内容上,则不要包含指示符部分
                if (this.m_nFocusCol == 3
                    && strSubfieldName == "")
                {
                    if (this.FocusedField.Indicator.Length == 2)
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            string strOutputValue = results[i];

                            if (strOutputValue.Length <= 2)
                                strOutputValue = "";
                            else
                                strOutputValue = strOutputValue.Substring(2);

                            results[i] = strOutputValue;
                        }
                    }
                }

                if (bSimulate == true)
                {
                    return results;
                }

                if (this.m_nFocusCol == 3
    && strSubfieldName == "")
                {
                    if (this.FocusedField.Value != results[index])
                        this.FocusedField.Value = results[index];
                }
                else
                {
                    if (this.FocusedField.IndicatorAndValue != results[index])
                        this.FocusedField.IndicatorAndValue = results[index];
                }

                // 不让小edit全选上
                // this.curEdit.SelectionLength = 0;
            }
            else
            {
                // 子字段值情形
                // int nOldSelectionStart = this.SelectionStart;

                // 获得宏值
                // parameters:
                //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
                // return:
                //      -1  error
                //      0   not found 
                //      1   found
                nRet = GetDefaultValue(
                    index,  // bSimulate,
                    strFieldName,
                    strSubfieldName,
                    out results,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                if (nRet == 0)
                    return new List<string>();

                if (bSimulate == true)
                    return results;

                if (strSubfieldName == "#indicator")
                {
                    this.FocusedField.Indicator = results[index];
                }
                else
                {
                    if (strSubfieldValue != results[index])
                    {
                        Subfield subfield = this.FocusedField.Subfields[strSubfieldName, nSubfieldDupIndex];
                        subfield.Value = results[index];
                    }
                }
            }

            return results;
        ERROR1:
            if (bSimulate == true)
            {
                results.Add("出错: " + strError);
                return results;
            }

            MessageBox.Show(this, strError);
            return new List<string>();
        }

        // 获得宏值
        // parameters:
        //      nPushIndex  需要实做的字符串事项的下标。如果为-1，表示没有要实做的事项(即全部都是模拟)
        //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
        // return:
        //      -1  error
        //      0   not found 
        //      >0  found 结果的个数
        private int GetDefaultValue(
            int nPushIndex,
            string strFieldName,
            string strSubFieldName,
            out List<string> results,
            out string strError)
        {
            Debug.Assert(strFieldName != null, "strFieldName参数不能为null");
            Debug.Assert(strSubFieldName != null, "strSubFieldName参数不能为null");
            // Debug.Assert(strValue != null, "strValue参数不能为null");

            strError = "";
            results = new List<string>();

            // 检查MarcDefDom是否存在
            if (this.MarcDefDom == null)
            {
                strError = m_strMarcDomError;
                return -1;
            }

            // 根据字段名找到配置文件中的该字段的定义
            XmlNode node = null;

            if (strSubFieldName == "" || strSubFieldName == "#indicator")
            {
                // 只找到字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']");
            }
            else
            {
                // 找到子字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubFieldName + "']");
            }

            if (node == null)
            {
                return 0;   // not found def
            }

            XmlNodeList value_nodes = null;

            if (strSubFieldName == "#indicator")
            {

                value_nodes = node.SelectNodes("Property/Indicator/Property/DefaultValue");
            }
            else
            {
                value_nodes = node.SelectNodes("Property/DefaultValue");
            }

            if (value_nodes.Count == 0)
                return 0;

            for (int i = 0; i < value_nodes.Count; i++)
            {
                string strOutputValue = value_nodes[i].InnerText;

                // 去掉定义值中的\r\n或者单独的\r和\n。这种具有\r\n的效果可能由notepad中折行状态时paste到编辑配置文件对话框并保存来造成.
                strOutputValue = strOutputValue.Replace("\r", "");
                strOutputValue = strOutputValue.Replace("\n", "");

                // 子字段符号
                strOutputValue = strOutputValue.Replace("\\", new string((char)31, 1));

                ParseMacroEventArgs e = new ParseMacroEventArgs();
                e.Macro = strOutputValue;
                // e.Simulate = bSimulate;
                if (i == nPushIndex)
                    e.Simulate = false; // 实做
                else
                    e.Simulate = true;  // 模拟

                TemplateControl_ParseMacro((object)this, e);
                if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = e.ErrorInfo;
                    return -1;
                }

                strOutputValue = e.Value;
                results.Add(strOutputValue);
            }

            return results.Count;
        }

        // 通过模板取固定字段的值
        // parameter:
        //		strFieldName	字段名称
        //		strSubFieldName	子字段名称 如果为空表示获得字段的定长模板，如果为 "indicator" 表示获得指示符的定长模板
        // return:
        //		-1	出错
        //		0	没找到 可能是模板文件不存在，或者对应的配置事项不存在
        //		1	找到
        private int GetValueFromTemplate(string strFieldName,
            string strSubFieldName,
            string strValue,
            out string strOutputValue,
            out string strError)
        {
            Debug.Assert(strFieldName != null, "GetValueFromTemplate()，strFieldName参数不能为null");
            Debug.Assert(strSubFieldName != null, "GetValueFromTemplate()，strSubFieldName参数不能为null");
            Debug.Assert(strValue != null, "GetValueFromTemplate()，strValue参数不能为null");

            // 2017/1/31
            if (strFieldName == "hdr")
                strFieldName = "###";

            strError = "";
            strOutputValue = strValue;

            // 检查MarcDefDom是否存在
            if (this.MarcDefDom == null)
            {
                strError = this.m_strMarcDomError;
                return 0;   // 2008/3/19恢复。 原来为什么要注释掉?
            }

            string strCmd = StringUtil.GetLeadingCommand(strValue);
            if (string.IsNullOrEmpty(strCmd) == false)
                strValue = strValue.Substring(strCmd.Length + 2);

            XmlNode nodeDef = null;
            string strTitle = "";

            // 首先尝试从外部接口获得模板定义
            if (this.GetTemplateDef != null)
            {
                GetTemplateDefEventArgs e = new GetTemplateDefEventArgs();
                e.FieldName = strFieldName;
                e.SubfieldName = strSubFieldName;
                e.Value = strValue;

                this.GetTemplateDef(this, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = "在通过外部接口获取字段名为 '" + strFieldName + "' 子字段名为 '" + strSubFieldName + "' 的模板定义XML时出错: \r\n" + e.ErrorInfo;
                    return -1;
                }
                if (e.Canceled == false)
                {
                    nodeDef = e.DefNode;
                    strTitle = e.Title;
                    goto FOUND;
                }

                // e.Canceled == true 表示希望MarcEditor来自己获得定义
            }

            // *** MarcEditor来自己获得定义

            // 根据字段名找到配置文件中的该字段的定义
            XmlNodeList nodes = null;
            if (strSubFieldName == "")
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field[@name='" + strFieldName + "']");
            else if (strSubFieldName == "indicator")    // 2021/10/25
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field[@name='" + strFieldName + "']/Indicator");
            else
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubFieldName + "']");

            if (nodes.Count == 0)
            {
                strError = "MARC定义文件中没有找到字段名为 '" + strFieldName + "' 子字段名为 '" + strSubFieldName + "' 的字段/子字段/字段指示符 定义";
                return 0;
            }

            if (nodes.Count > 1)
            {
                List<string> lines = new List<string>();
                int i = 0;
                foreach (XmlNode node in nodes)
                {
                    string strType = DomUtil.GetAttr(node, "type");
                    lines.Add((i + 1).ToString() + ") " + strType);
                    i++;
                }

                SelectListStringDialog select_def_dlg = new SelectListStringDialog();
                GuiUtil.AutoSetDefaultFont(select_def_dlg);

                select_def_dlg.Text = "请选择模板定义";
                select_def_dlg.Values = lines;
                select_def_dlg.Comment = "MARC定义文件中发现 字段名为 '" + strFieldName + "' 子字段名为 '" + strSubFieldName + "' 的模板定义有 " + nodes.Count.ToString() + " 处。\r\n\r\n请选择其中一个";
                select_def_dlg.StartPosition = FormStartPosition.CenterScreen;
                select_def_dlg.ShowDialog(this);
                if (select_def_dlg.DialogResult == DialogResult.Cancel)
                {
                    strError = "放弃选择模板定义";
                    return 0;
                }
                Debug.Assert(select_def_dlg.SelectedIndex != -1, "");
                nodeDef = nodes[select_def_dlg.SelectedIndex];
                strTitle = "定长模板 : " + strFieldName
                    + (string.IsNullOrEmpty(strSubFieldName) == false ? new String(Record.KERNEL_SUBFLD, 1) + strSubFieldName : "")
                    + " -- " + select_def_dlg.SelectedValue;
            }
            else
            {
                nodeDef = nodes[0];
            }
        FOUND:
            FixedTemplateDlg dlg = new FixedTemplateDlg();
            // GuiUtil.AutoSetDefaultFont(dlg);
            if (string.IsNullOrEmpty(strTitle) == false)
                dlg.Text = "定长模板 : " + strTitle;
            else
                dlg.Text = "定长模板 : " + strFieldName
                    + (string.IsNullOrEmpty(strSubFieldName) == false ? new String(Record.KERNEL_SUBFLD, 1) + strSubFieldName : "");

            // 加事件
            // dlg.TemplateControl.GetConfigFile -= this.GetConfigFile;
            // dlg.TemplateControl.GetConfigFile += this.GetConfigFile;
            dlg.TemplateControl.GetConfigDom -= this.GetConfigDom;
            dlg.TemplateControl.GetConfigDom += this.GetConfigDom;

            dlg.TemplateControl.MarcDefDom = this.MarcDefDom;
            dlg.TemplateControl.Lang = this.Lang;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.GetTemplateDef += new GetTemplateDefEventHandler(dlg_GetTemplateDef);

            int nRet = dlg.Initial(nodeDef,
                this.Lang,
                strValue,
                out strError);
            if (nRet == 0)
                return 0;

            if (nRet == -1)
                return -1;

            dlg.TemplateControl.ParseMacro += new ParseMacroEventHandler(TemplateControl_ParseMacro);

            dlg.ShowDialog(this);

            dlg.TemplateControl.ParseMacro -= new ParseMacroEventHandler(TemplateControl_ParseMacro);

            if (dlg.DialogResult == DialogResult.OK)
            {
                strOutputValue = dlg.TemplateControl.Value;
                if (string.IsNullOrEmpty(strCmd) == false)
                    strOutputValue = "{" + strCmd + "}" + strOutputValue;
            }

            return 1;
        }

        void dlg_GetTemplateDef(object sender, GetTemplateDefEventArgs e)
        {
            if (this.GetTemplateDef != null)
                this.GetTemplateDef(sender, e);
        }

        // 兑现宏
        void TemplateControl_ParseMacro(object sender, ParseMacroEventArgs e)
        {
            // 将一些基本的宏兑现
            // %year%%m2%%d2%%h2%%min2%%sec2%.%hsec%

            string strOutputValue = dp2StringUtil.MacroTimeValue(e.Macro);

            // 替换下划线
            // 只替换前面连续的'_'
            // strOutputValue = strOutputValue.Replace("_", " ");

            // 替换字符串最前面一段连续的字符
            strOutputValue = StringUtil.ReplaceContinue(strOutputValue, '_', ' ');

            // 替换子字段符号
            // strOutputValue = strOutputValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);   // $?

            e.Value = strOutputValue;
            e.ErrorInfo = "";

            // 如果是一般的宏, MARC编辑器控件就可以解决
            // 如果控件外围没有支持事件, 也只能这里解决部分
            if (e.Macro.IndexOf("%") == -1 || this.ParseMacro == null)
            {
                return;
            }
            else
            {
                // 否则还需要求助于宿主
                ParseMacroEventArgs e1 = new ParseMacroEventArgs();
                e1.Macro = e.Value; // 第一次处理过的, 再级联处理
                e1.Simulate = e.Simulate;
                this.ParseMacro(this, e1);

                e.Value = e1.Value;
                e.ErrorInfo = e1.ErrorInfo;
                return;
            }
        }

        // 菜单：定长模板
        internal void GetValueFromTemplate(object sender,
            System.EventArgs e)
        {
            this.GetValueFromTemplate();
        }

        // 取当前位置的值列表
        /// <summary>
        /// 显示当前位置的指列表对话框
        /// </summary>
        public void GetValueFromValueList()
        {
            if (this.FocusedField == null)
                return;

            Debug.Assert(this.FocusedField != null, "在GetSubFieldValueWithDlg()时，FocusedField不可能为null");

            string strFieldName = this.FocusedField?.Name;
            string strFieldValue = this.FocusedField?.Value;

            string strSubfieldName = "";
            string strSubfieldValue = "";
            int nSubfieldDupIndex = 0;

            MarcEditor.GetCurrentSubfield(strFieldValue,
                this.GetCurrentSelectionStart(),
                out strSubfieldName,
                out nSubfieldDupIndex,
                out strSubfieldValue);

            string strError;
            string strOutputValue;
            int nRet = 0;
            if (strSubfieldName == "")
            {
            }
            else
            {
                // 不是定长子字段的情况
                {
                    List<XmlNode> valueListNodes = null;
                    nRet = GetValueListNodes(strFieldName,
                        strSubfieldName,
                        out valueListNodes,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }
                    if (nRet == 0)
                        return;

                    Debug.Assert(valueListNodes != null, "不可能的情况。");

                    ValueListDlg dlg = new ValueListDlg();
                    GuiUtil.AutoSetDefaultFont(dlg);
                    dlg.Text = "值列表 : " + strFieldName + new string(Record.KERNEL_SUBFLD, 1) + strSubfieldName;
                    nRet = dlg.Initialize(valueListNodes,
                        this.Lang,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }
                    dlg.SelectedValue = strSubfieldValue;
                    // dlg.StartPosition = FormStartPosition.CenterScreen;
                    dlg.AppInfo = this.AppInfo; // 为了能够保持列宽度

                    if (this.AppInfo != null)
                        this.AppInfo.LinkFormState(dlg,
                            "marceditor_valuelistdialog_state");
                    dlg.ShowDialog(this);
                    if (this.AppInfo != null)
                        this.AppInfo.UnlinkFormState(dlg);


                    if (dlg.DialogResult != DialogResult.OK)
                        return;

                    strOutputValue = dlg.SelectedValue;
                }

                if (strSubfieldValue != strOutputValue)
                {
                    Subfield subfield = this.FocusedField.Subfields[strSubfieldName, nSubfieldDupIndex];
                    subfield.Value = strOutputValue;
                }
            }
        }

        // 调模板取值函数
        /// <summary>
        /// 打开当前按位置的定长模板对话框
        /// </summary>
        public void GetValueFromTemplate()
        {
            if (this.FocusedField == null)
                return;

            Debug.Assert(this.FocusedField != null, "在GetSubFieldValueWithDlg()时，FocusedField不可能为null");

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.Value;


            string strSubfieldName = "";
            string strSubfieldValue = "";
            int nSubfieldDupIndex = 0;
            MarcEditor.GetCurrentSubfield(strFieldValue,
                this.GetCurrentSelectionStart(),
                out strSubfieldName,
                out nSubfieldDupIndex,
                out strSubfieldValue);

            string strError;
            string strOutputValue;
            int nRet = 0;
            if (strSubfieldName == "")
            {
                // 获取字段的定长模板
                nRet = this.GetValueFromTemplate(strFieldName,
                    "",
                    strFieldValue,
                    out strOutputValue,
                    out strError);

                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                if (nRet == 0)
                    return;

                // 此处应使用Value
                if (strFieldValue != strOutputValue)
                    this.FocusedField.Value = strOutputValue;
            }
            else if (strSubfieldName == "indicator")
            {

            }
            else
            {
                // int nOldSelectionStart = this.SelectionStart;

                nRet = this.GetValueFromTemplate(strFieldName,
                    strSubfieldName,
                    strSubfieldValue,
                    out strOutputValue,
                    out strError);

                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                // 不是定长子字段的情况
                if (nRet == 0)
                    return;

                if (strSubfieldValue != strOutputValue)
                {
                    Subfield subfield = this.FocusedField.Subfields[strSubfieldName, nSubfieldDupIndex];
                    subfield.Value = strOutputValue;
                }
            }
        }

        /*
            <Field name='801'>
                <Property>
                    <Label xml:lang='en'>Originating Source</Label>
                    <Label xml:lang='cn'>记录来源</Label>
                </Property>
                <Subfield name='a'>
                    <Property>
                        <Label xml:lang='en'></Label>
                        <Label xml:lang='cn'>国家代码</Label>
                        <ValueList ref='countrycode' />
                    </Property>
                </Subfield>
        */
        // return:
        //		-1	出错
        //		0	未找到对应的ValueList
        //		1	找到
        /*public*/
        int GetValueListNodes(string strFieldName,
            string strSubfieldName,
            out List<XmlNode> valueListNodes,
            out string strError)
        {
            valueListNodes = new List<XmlNode>();
            strError = "";

            if (this.MarcDefDom == null)
                return 0;


            string strXPath = "Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubfieldName + "']/Property/ValueList";
            XmlNodeList nodes = this.MarcDefDom.DocumentElement.SelectNodes(strXPath);
            if (nodes.Count == 0)
                return 0;

            foreach (XmlNode node in nodes)
            {
                valueListNodes.Add(node);
            }

            while (true)
            {
                bool bFoundRef = false;

                for (int i = 0; i < valueListNodes.Count; i++)
                {
                    XmlNode node = valueListNodes[i];
                    //找ref
                    string strRef = DomUtil.GetAttr(node, "ref");
                    if (string.IsNullOrEmpty(strRef) == true)
                        continue;

                    bFoundRef = true;

                    // 未挂接事件
                    if (this.GetConfigDom == null)
                        return 0;

                    GetConfigDomEventArgs e = new GetConfigDomEventArgs();
                    e.Path = strRef;
                    e.XmlDocument = null;

                    this.GetConfigDom(this, e);
                    if (e.ErrorInfo != "")
                    {
                        strError = "获取'" + strFieldName + "$" + strSubfieldName + "'对应的ValueList出错，原因:" + e.ErrorInfo;
                        return -1;
                    }
                    if (e.XmlDocument == null)
                        return 0;

                    int nIndex = strRef.IndexOf('#');
                    string strSource = "";
                    string strValueListName = "";
                    if (nIndex != -1)
                    {
                        strSource = strRef.Substring(0, nIndex);
                        strValueListName = strRef.Substring(nIndex + 1);
                    }
                    else
                    {
                        strValueListName = strRef;
                    }

                    // 把原有的node从数组中删除
                    valueListNodes.Remove(node);
                    i--;

                    XmlNode node_valuelist = e.XmlDocument.SelectSingleNode("//ValueList[@name='" + strValueListName + "']");
                    if (node_valuelist == null)
                    {
                        strError = "未找到路径为'" + strRef + "'的节点。";
                        return -1;
                    }
                    valueListNodes.Add(node_valuelist);
                }

                if (bFoundRef == false)
                    break;
            }
            return 1;
        }

        #endregion

        #region 关于小edit控件插入符的静态函数

        // (即将废弃)
        // 注: nSelectionStart 是指小 edit 中的 SelectionStart
        // 根据插入符位置，在字段值中得到当前子字段的信息
        // parameters:
        //		strFieldValue	字段值
        //		nSelectionStart	当前插入符位置
        //		strSubfieldName	out参数，返回当前子字段名称
        //		nSubfieldDupIndex	out参数，返回当前子字段重复的序号
        //		strSubfieldValue	out参数，返回当前子字段的值
        /// <summary>
        /// 根据插入符位置，在字段值中得到当前子字段的信息
        /// </summary>
        /// <param name="strFieldValue">字段值</param>
        /// <param name="nSelectionStart">当前插入符位置</param>
        /// <param name="strSubfieldName">返回当前子字段名称</param>
        /// <param name="nSubfieldDupIndex">返回当前子字段重复的序号</param>
        /// <param name="strSubfieldValue">返回当前子字段的值</param>
        public static void GetCurrentSubfield(string strFieldValue,
            int nSelectionStart,
            out string strSubfieldName,
            out int nSubfieldDupIndex,
            out string strSubfieldValue)
        {
            strSubfieldName = "";
            nSubfieldDupIndex = 0;
            strSubfieldValue = "";

            // strFieldValue = strFieldValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);

            int nCurSubfieldX = -1;
            for (int i = 0; i < strFieldValue.Length; i++)
            {
                // 2024/8/2 之前为 i > nSelectionStart
                if ((i > 0 && i >= nSelectionStart)
                    || (i == 0 && i > nSelectionStart)/*专门针对插入符在内容区域偏移 0 的情况*/)
                    break;

                char ch = strFieldValue[i];

                // 是子字段符号
                if (ch == Record.SUBFLD)
                    nCurSubfieldX = i;
            }

            if (nCurSubfieldX != -1
                && strFieldValue.Length > nCurSubfieldX + 1)
            {
                strSubfieldName = strFieldValue.Substring(nCurSubfieldX + 1, 1);

                //???
                string strLeft = strFieldValue.Substring(0, nCurSubfieldX);
                for (; ; )
                {
                    int nTempIndex = strLeft.IndexOf(Record.SUBFLD.ToString() + strSubfieldName);
                    if (nTempIndex == -1)
                        break;
                    nSubfieldDupIndex++;

                    strLeft = strLeft.Substring(nTempIndex + 2);
                }

                if (strFieldValue.Length > nCurSubfieldX + 1 + 1)
                {
                    strSubfieldValue = strFieldValue.Substring(nCurSubfieldX + 1 + 1);
                    int nPosition = strSubfieldValue.IndexOf(Record.SUBFLD);
                    if (nPosition != -1)
                        strSubfieldValue = strSubfieldValue.Substring(0, nPosition);
                }
            }

        }
        #endregion

        class MarcEditorState
        {
            // 字段名区域像素宽度
            public int FieldNameCaptionWidth { get; set; }

            // 2025/12/26
            public char HighlightBlankChar { get; set; } = ' ';

            public string ColorThemeName { get; set; }

            public string Font { get; set; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string UiState
        {
            get
            {
                MarcEditorState state = new MarcEditorState
                {
                    FieldNameCaptionWidth = this.FieldNameCaptionWidth,
                    HighlightBlankChar = this.HighlightBlankChar,
                    ColorThemeName = this.ColorThemeName,
                    Font = GetFontString(this.Font),
                };
                return JsonConvert.SerializeObject(state);
            }
            set
            {
                MarcEditorState state = JsonConvert.DeserializeObject<MarcEditorState>(value);
                if (state != null)
                {
                    this.BeginUpdate();
                    try
                    {
                        this.FieldNameCaptionWidth = state.FieldNameCaptionWidth;
                        this.HighlightBlankChar = state.HighlightBlankChar == 0 ? ' ' : state.HighlightBlankChar;
                        this.ColorThemeName = state.ColorThemeName;
                        var font = GetFont(state.Font);
                        if (font != null)
                            this.Font = font;
                    }
                    finally
                    {
                        this.EndUpdate();
                    }
                }
            }
        }

        public static Font GetFont(string strFontString)
        {
            if (String.IsNullOrEmpty(strFontString) == false)
            {
                var converter = TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strFontString);
            }

            return null;
        }

        public static string GetFontString(Font font)
        {
            var converter = TypeDescriptor.GetConverter(typeof(Font));
            return converter.ConvertToString(font);
        }

        public int CalcuTextLineHeight(Graphics g_param)
        {
            Graphics g = g_param;
            if (g == null)
                g = Graphics.FromHwnd(this.Handle);

            var size = TextRenderer.MeasureText(g,
                "lg",
                this.Font,
                new Size(100, -1),
                MarcEditor.editflags);

            return size.Height;
        }

        #region 为了兼容

        public int SelectionStart { get; set; }

        #endregion

        // 阻止在字段末尾删除一个字符
        public override bool ProcessDeleteChar(HitInfo info,
            DeleteKeyStyle style,
            bool delay)
        {
            // 让 Shift+Delete 执行原有 MarcControl 删除字符功能(包含能删除字段结束符)
            if ((Control.ModifierKeys & Keys.Shift) != 0)
                return base.ProcessDeleteChar(info, DeleteKeyStyle.DeleteFieldTerminator, delay);

            // 否则这里打补丁实现
            var index = info.ChildIndex;
            if (index >= this.MarcRecord.FieldCount)
                return false;

            var field = this.MarcRecord.GetField(index);
            if (field == null)
                return base.ProcessDeleteChar(info, style, delay);

            if (index == 0)
            {
                if (info.Offs >= field.PureTextLength)
                    return false;
            }

            Debug.Assert(index > 0);

            this.MarcRecord.GetFieldOffsRange(index,
                out int start,
                out int end);

            // 阻止在字段最后一个字符以右进行删除操作。原操作是把下一个字段拉上来连接到一起
            if (info.Offs >= end - 1)
                return false;

            return base.ProcessDeleteChar(info, style, delay);
        }
    }

    internal class InvalidateRect
    {
        public bool bAll = false;
        public Rectangle rect = new Rectangle(0, 0, 0, 0);
    }

    // 行中的各部分
    internal enum LinePart
    {
        Entire = 0,	// 全部
        Name = 1,	// 名称部分
        Indicator = 2,	// 指示符部分
        Value = 3,	// 值部分
    }

    // 卷滚条枚举值
    internal enum ScrollBarMember
    {
        Vert = 0,
        Horz = 1,
        Both = 2,
        None = 3,
    }

    internal enum BoundsPortion
    {
        Field = 0,  //一行，包括左上的线条,不包括右下的线条
        FieldAndBottom, //行和底部(lines底部的线条 和 控件底部的空白)
    }

    /// <summary>
    /// MARC 编辑器控件
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
