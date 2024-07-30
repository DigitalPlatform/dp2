using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Xml;
using System.Text;

using Newtonsoft.Json;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;
using System.Linq;
using DigitalPlatform.MarcEditor;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// MARC 编辑器控件
    /// </summary>
    // [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public class MarcEditor : System.Windows.Forms.Control//,IGetValueList //UserControl
    {
        internal const int WM_LEFTRIGHT_MOVED = API.WM_USER + 201;

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

        internal Font m_fixedSizeFont = null;
        internal Font m_captionFont = null;

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

        // 文档坐标
        /*public*/
        int m_nDocumentOrgX = 0;
        /*public*/
        int m_nDocumentOrgY = 0;

        // 控件边框
        private BorderStyle m_borderStyle = BorderStyle.Fixed3D;


        // 各种颜色

        /*
		// 文字的颜色
		internal Color defaultTextColor = SystemColors.WindowText;

        public Color TextColor
        {
            get
            {
                return this.defaultTextColor;
            }
            set
            {
                this.defaultTextColor = value;
                this.Invalidate();
            }
        }
         */

        // 竖线条的颜色
        internal Color defaultVertGridColor = Color.LightGray;

        /// <summary>
        /// 竖线条的颜色
        /// </summary>
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

        // ---------------------------------------------

        ToolTip toolTip1 = new ToolTip();

        // 2008/6/4
        internal ImeMode OldImeMode = ImeMode.NoControl;    // off

        /// <summary>
        /// 当前输入法状态
        /// </summary>
        public ImeMode CurrentImeMode
        {
            get
            {
                if (this.curEdit != null)
                    return this.curEdit.ImeMode;
                return ImeMode.NoControl;    // off
            }
            set
            {
                if (this.curEdit != null)
                {
                    if (this.curEdit.ImeMode != value)
                    {
                        this.curEdit.ImeMode = value;
                        API.SetImeHalfShape(this.curEdit);
                    }
                }
                else
                    this.OldImeMode = value;
            }
        }

        // 小文本编辑控件
        internal MyEdit curEdit = null;   // new MyEdit();	//edit控件
        // private bool m_bEditInitialized = false;  //edit控件是否初始化

        // 选中的字段数组下标
        /// <summary>
        /// 当前选中的字段下标数组
        /// </summary>
        public List<int> SelectedFieldIndices = new List<int>();    // 2014/7/10 从 ArrayList 修改为 List<int>

        // 当前获得焦点字段索引号与列号
        // 1: 字段名 2: 指示符 3:字段内容
        internal int m_nFocusCol
        {
            get
            {
                return _focusCol;
            }
            set
            {
                _focusCol = value;
            }
        }

        private int _focusCol = 2;

        // public int m_nFocusedFieldIndex = -1;

        // Shift连续选择时的基准点
        internal int nStartFieldIndex = -1;

        XmlDocument m_domMarcDef = null;

        string m_strMarcDomError = "";

        string _lang = "zh";

        /// <summary>
        /// 当前界面语言代码
        /// </summary>
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

        internal int m_nToolTipIndex = 1;

        bool m_bChanged = false;

        private System.ComponentModel.Container components = null;

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
        public XmlDocument MarcDefDom
        {
            get
            {
                if (this.m_domMarcDef != null)
                    return this.m_domMarcDef;

                if (m_strMarcDomError != "")
                    return null;    // 避免反复报错

#if NO
                string strError = "";

                GetConfigFileEventArgs e = new GetConfigFileEventArgs();
                e.Path = "marcdef";
                e.Stream = null;
                if (this.GetConfigFile != null)
                {
                    this.GetConfigFile(this, e);
                }
                else
                {
                    //throw new Exception("GetConfigFile事件尚未初始化");
                    return null;
                }

                if (e.ErrorInfo != "")
                {
                    strError = "获取marcdef出错，原因:" + e.ErrorInfo;
                    goto ERROR1;
                }
                if (e.Stream != null)
                {
                    e.Stream.Seek(0, SeekOrigin.Begin);
                    this.m_domMarcDef = new XmlDocument();
                    try
                    {
                        this.m_domMarcDef.Load(e.Stream);
                    }
                    catch (Exception ex)
                    {
                        this.m_domMarcDef = null;
                        strError = "加载marcdef配置文件到dom时出错：" + ex.Message;
                        goto ERROR1;
                    }
                    e.Stream.Close();
                }

                return m_domMarcDef;
            ERROR1:
                m_strMarcDomError = strError;
                // throw new Exception(strError);
                return null;
#endif
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
                this.m_strMarcDomError = "";
                this.m_domMarcDef = value;
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
            this.record = new Record(this);

            this.DoubleBuffered = true;

            // 该调用是 Windows.Forms 窗体设计器所必需的。
            InitializeComponent();
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            if (this.DesignMode == false)
                FreeFonts();
            base.Dispose(disposing);
        }
        #endregion

        #region 组件设计器生成的代码
        /// <summary>
        /// 设计器支持所需的方法 - 不要使用代码编辑器 
        /// 修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            if (this.DesignMode)
                return;

            this.SuspendLayout();
            // 
            // MarcEditor
            // 
            // this.Enter += new System.EventHandler(this.MarcEditor_Enter);
            this.EnabledChanged += new System.EventHandler(this.MarcEditor_EnabledChanged);
            this.ResumeLayout(false);
        }
        #endregion

        // 2009/10/24
        internal bool m_bReadOnly = false;

        /// <summary>
        /// 是否为只读状态
        /// </summary>
        public bool ReadOnly
        {
            get
            {
                return this.m_bReadOnly;
            }
            set
            {
                this.m_bReadOnly = value;

                if (this.curEdit != null)
                {
                    this.curEdit.ReadOnly = value;
                    if (this.m_bReadOnly == true)
                    {
                        this.curEdit.Hide();
                    }
                    else
                    {
                        this.curEdit.Show();
                        if (this.curEdit.ImeMode == ImeMode.Disable
                            || this.curEdit.ImeMode == ImeMode.Off)
                        {
                            this.curEdit.ImeMode = ImeMode.NoControl;  // 2009/11/12
                        }
                    }
                }

                this.Invalidate();  // 失效窗口，重新显示
                this.Update();
            }
        }

        static bool _bidiAdjust = false;

        // 是否通过在后部加一个空格字符来达到自动调整 $9 的显示顺序的效果？
        public static bool BidiAdjust
        {
            get
            {
                return _bidiAdjust;
            }
            set
            {
                _bidiAdjust = value;
            }
        }

        /// <summary>
        /// 选定小编辑器的指定范围
        /// </summary>
        /// <param name="nStart">开始位置</param>
        /// <param name="nLength">字符数</param>
        public void SelectCurEdit(int nStart, int nLength)
        {
            if (this.curEdit == null)
                return;
            this.curEdit.Select(nStart, nLength);
        }

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

                // 最后触发
                base.Font = value;
            }
        }

        /// <summary>
        /// 当前等宽字体
        /// </summary>
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
                    this.m_fixedSizeFont = new Font(new FontFamily("Courier New"),
                        this.Font != null ? this.Font.SizeInPoints : 9,
                        FontStyle.Bold,
                        GraphicsUnit.Point);
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

#if NO
        public Font CreateFixedSizeFont()
        {
            return new Font("Courier New", this.Font.Size, FontStyle.Bold);
        }
#endif

        void FreeFonts()
        {
            this.FixedSizeFont = null;
            this.CaptionFont = null;
        }

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
                        this.m_fixedSizeFont = new Font(new FontFamily("宋体"),
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

        static Font NewFont(Font ref_font)
        {
            return new Font(ref_font.FontFamily,
    ref_font.SizeInPoints,
    ref_font.Style,
    GraphicsUnit.Point);
        }

#if NO
        Font CreateCaptionFont()
        {
            return new Font("楷体", this.Font.Size/*, FontStyle.Bold*/);
        }
#endif

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

                if (this.m_borderStyle == BorderStyle.FixedSingle)
                {
                    param.Style |= API.WS_BORDER;
                }
                else if (this.m_borderStyle == BorderStyle.Fixed3D)
                {
                    param.ExStyle |= API.WS_EX_CLIENTEDGE;
                }
                return param;
            }
        }

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

        #endregion

        #region 关于卷滚条的代码

        char m_chOldSubfieldName = (char)0;

        // 重载DefWndProc函数的目的是为了处理卷滚条
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            if (this.DesignMode)
            {
                base.DefWndProc(ref m);
                return;
            }

            switch (m.Msg)
            {
                case WM_LEFTRIGHT_MOVED:
                    {
                        char current = this.FocusedSubfieldName;
                        // TODO: 注意区别同名的子字段
                        if (current != m_chOldSubfieldName)
                        {
                            m_chOldSubfieldName = current;
                            this.FireSelectedFieldChanged();
                        }
                    }
                    break;
                case API.WM_SETFOCUS:
                    {
                        if (GetFocusParameter() == "dont_seteditpos")
                        {

                        }
                        else
                            this.SetEditPos();
                    }
                    return;

                case API.WM_VSCROLL:
                    {
                        switch (API.LoWord(m.WParam.ToInt32()))
                        {
                            case API.SB_BOTTOM:
                                MessageBox.Show("SB_BOTTOM");
                                break;
                            case API.SB_TOP:
                                MessageBox.Show("SB_TOP");
                                break;
                            case API.SB_THUMBTRACK:
                                this.Update();
                                DocumentOrgY = -API.HiWord(m.WParam.ToInt32());
                                break;
                            case API.SB_LINEDOWN:
                                {
                                    DocumentOrgY -= this.record.AverageLineHeight;
                                }
                                break;
                            case API.SB_LINEUP:
                                {
                                    DocumentOrgY += this.record.AverageLineHeight;
                                }
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgY -= this.ClientHeight;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgY += this.ClientHeight;
                                break;
                        }
                    }
                    break;

                case API.WM_HSCROLL:
                    {
                        switch (API.LoWord(m.WParam.ToInt32()))
                        {
                            case API.SB_THUMBPOSITION:
                            case API.SB_THUMBTRACK:
                                DocumentOrgX = -API.HiWord(m.WParam.ToInt32());
                                break;
                            case API.SB_LINEDOWN:
                                DocumentOrgX -= 20;
                                break;
                            case API.SB_LINEUP:
                                DocumentOrgX += 20;
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgX -= this.ClientSize.Width;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgX += this.ClientSize.Width;
                                break;
                        }
                    }
                    break;
                /*
            case API.WM_LBUTTONDBLCLK:
                {
                    API.SendMessage(this.curEdit.Handle,
                        m.Msg,
                        m.WParam, 
                        m.LParam);
                    return;
                }
                break;
                 */
                /*
                // 2008/11/26
            case API.WM_GETDLGCODE:
                {
                    m.Result = new IntPtr(API.DLGC_WANTALLKEYS);
                    return;
                }*/
                // 2024/7/5
                // 要求容器 Form 把所有方向键发给本控件
                case API.WM_GETDLGCODE:
                    m.Result = new IntPtr(API.DLGC_WANTALLKEYS | API.DLGC_WANTARROWS | API.DLGC_WANTCHARS);
                    return;
            }
            base.DefWndProc(ref m);
        }

        // 设卷滚条
        // parameter:
        //		member	
        private void SetScrollBars(ScrollBarMember member)
        {
            if (member == ScrollBarMember.Horz
                || member == ScrollBarMember.Both)
            {
                // 水平方向
                API.ScrollInfoStruct si = new API.ScrollInfoStruct();

                si.cbSize = Marshal.SizeOf(si);
                si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
                si.nMin = 0;
                si.nMax = this.DocumentWidth;
                si.nPage = this.ClientWidth;
                si.nPos = -this.DocumentOrgX;
                API.SetScrollInfo(this.Handle, API.SB_HORZ, ref si, true);
            }

            if (member == ScrollBarMember.Vert
                || member == ScrollBarMember.Both)
            {
                // 垂直方向
                API.ScrollInfoStruct si = new API.ScrollInfoStruct();

                si.cbSize = Marshal.SizeOf(si);
                si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
                si.nMin = 0;
                si.nMax = this.DocumentHeight;
                si.nPage = this.ClientHeight;
                si.nPos = -this.DocumentOrgY;
                API.SetScrollInfo(this.Handle, API.SB_VERT, ref si, true);
            }
        }

        // 当文档尺寸和文档原点改变后，
        // 更新卷滚条，小edit控件 等设施状态，和使文档某区域失效以便文档可见
        // parameters:
        //		scrollBarMember	卷滚条枚举值
        //		iRect	失效区域 当为null，则不失效，如bAll等于true则全部区域
        internal void AfterDocumentChanged(ScrollBarMember scrollBarMember,
            InvalidateRect iRect)
        {
            // 设小edit控件
            this.SetEditPos(false); // 2008/11/26 changed 单纯改变文档尺寸，不应变动小edit的focus特性。也就是说，原来如果没有focus，现在也应没有

            // 设卷滚条信息
            this.SetScrollBars(scrollBarMember);

            // 失效区域
            if (iRect != null)
            {
                if (iRect.bAll == true)
                    this.Invalidate();
                else
                    this.Invalidate(iRect.rect);
            }
        }

        #endregion

        #region 关于小 edit 控件的一些函数

        void InitialEditControl()
        {
            // 2009/11/20
            if (this.curEdit != null)
            {
                this.Controls.Remove(this.curEdit);
                this.curEdit.Dispose(); // 2015/9/16
                this.curEdit = null;
            }

            this.curEdit = new MyEdit();

            curEdit.RightToLeft = RightToLeft.Inherit;
            curEdit.ImeMode = ImeMode.NoControl;    // off
            curEdit.BorderStyle = BorderStyle.None;  // BorderStyle.FixedSingle;
            curEdit.MaxLength = 0;
            curEdit.Multiline = true;
            curEdit.WordWrap = true;

            // 2009/10/24
            if (curEdit.ReadOnly != this.ReadOnly)
                curEdit.ReadOnly = this.ReadOnly;

            this.Controls.Add(curEdit);
        }

        // 为SetEditPos()编写的私有函数
        // 修改小edit控件的大小，并移动位置
        // parameter:
        //		nFieldIndex	字段的行号
        //		nCol	列号
        private void ChangeEditSizeAndMove(int nFieldIndex,
            int nCol)
        {
            Debug.Assert(nFieldIndex >= 0 && nFieldIndex < this.record.Fields.Count, "nFieldIndex参数不合法。");
            Debug.Assert(nCol == 1 || nCol == 2 || nCol == 3, "nCol 只能为 1,2,3");

            Field field = this.record.Fields[nFieldIndex];


            // 如果是没有指示符的字段，需要调整
            // 2008/3/19
            if (Record.IsFirstHeaderFieldName(field.Name) == true
                && (nCol == 1 || nCol == 2))
            {
                Debug.Assert(false, "第一个头标区字段的 1 2 列不让接触");
                // nCol = 3;
            }
            else if (Record.IsControlFieldName(field.Name) == true
                && nCol == 2)
            {
                Debug.Assert(false, "");
                // nCol = 3;
            }

            int nDelta = 0; // 调整系数

            /*
            if (nCol == 1 || nCol == 2)
                nDelta = 3;
             */

            int x = 0;
            int y = 0;
            int nWidth = 0;
            int nHeight = 0;

            nHeight = field.PureHeight;

            /*
            if (nCol == 1 || nCol == 2)
            {
                nHeight += 2;   // 调整
            }
             */

            int nFieldsHeight = this.record.GetFieldsHeight(0,
                nFieldIndex);


            y = this.DocumentOrgY
                + this.TopBlank
                + nFieldsHeight
                + this.record.GridHorzLineHeight
                + this.record.CellTopBlank;

            // 字段名上
            if (nCol == 1)
            {
                x = this.record.NameCaptionTotalWidth
                    + this.record.GridVertLineWidthForSplit
                    + this.record.CellLeftBlank;
                nWidth = this.record.NamePureWidth + 1; //
                this.curEdit.Multiline = false;
                this.curEdit.MaxLength = 3;

                nHeight = this.record.NamePureHeight;
            }
            else if (nCol == 2)
            {

                // 字段指示符上
                x = this.record.NameCaptionTotalWidth
                    + this.record.NameTotalWidth
                    + this.record.GridVertLineWidth
                    + this.record.CellLeftBlank;

                nWidth = this.record.IndicatorPureWidth + 1;    //
                this.curEdit.Multiline = false;
                this.curEdit.MaxLength = 2;

                nHeight = this.record.NamePureHeight;
            }
            else if (nCol == 3)
            {
                // 值上
                x = this.record.NameCaptionTotalWidth
                    + this.record.NameTotalWidth
                    + this.record.IndicatorTotalWidth
                    + this.record.GridVertLineWidth
                    + this.record.CellLeftBlank;
                nWidth = this.record.ValuePureWidth;
                this.curEdit.Multiline = true;
            }
            x += this.DocumentOrgX + this.LeftBlank + nDelta;

            // 按照区域设置不同的字体
            if (nCol == 1 || nCol == 2)
            {
                if (curEdit.Font != this.FixedSizeFont)
                    curEdit.Font = this.FixedSizeFont;

                if (nCol == 1)
                {
                    if (this.ReadOnly == false)
                    {
                        curEdit.ForeColor = this.defaultNameTextColor;
                        curEdit.BackColor = this.defaultNameBackColor;
                    }
                    else
                    {
                        // 2009/10/24
                        curEdit.ForeColor = this.defaultNameTextColor;
                        curEdit.BackColor = SystemColors.Control;
                    }
                }
                if (nCol == 2)
                {
                    if (this.ReadOnly == false)
                    {
                        curEdit.ForeColor = this.m_contentTextColor;
                        curEdit.BackColor = this.defaultIndicatorBackColor;
                    }
                    else
                    {
                        // 2009/10/24
                        curEdit.ForeColor = this.m_contentTextColor;
                        curEdit.BackColor = SystemColors.Control;
                    }
                }
            }
            else
            {
                if (curEdit.Font != this.Font)
                    curEdit.Font = this.Font;
            }

            if (nCol == 1)  // 字段名
            {
                if (this.ReadOnly == false)
                {
                    curEdit.ForeColor = this.defaultNameTextColor;
                    curEdit.BackColor = this.defaultNameBackColor;
                }
                else
                {
                    // 2009/10/24
                    curEdit.ForeColor = this.defaultNameTextColor;
                    curEdit.BackColor = SystemColors.Control;
                }
            }
            if (nCol == 2)  // 指示符
            {
                if (this.ReadOnly == false)
                {
                    curEdit.ForeColor = this.defaultIndicatorTextColor;
                    curEdit.BackColor = this.defaultIndicatorBackColor;
                }
                else
                {
                    // 2009/10/24
                    curEdit.ForeColor = this.defaultIndicatorTextColor;
                    curEdit.BackColor = SystemColors.Control;
                }
            }
            if (nCol == 3)  // 内容
            {
                if (this.ReadOnly == false)
                {
                    curEdit.ForeColor = this.m_contentTextColor;
                    curEdit.BackColor = this.defaultContentBackColor;
                }
                else
                {
                    // 2009/10/24
                    curEdit.ForeColor = this.m_contentTextColor;
                    curEdit.BackColor = SystemColors.Control;
                }
            }

            //  curEdit.AutoSize = false;


            Size oldsize = curEdit.Size;
            Size newsize = new System.Drawing.Size(
                nWidth,
                nHeight);

            Point loc = new System.Drawing.Point(x, y);

            // 从小变大，先move然后改变size
            if (oldsize.Height < newsize.Height)
            {
                curEdit.Location = loc;
                curEdit.Size = newsize;
            }
            else
            {
                // 从大变小，先size然后改变move
                curEdit.Size = newsize;

                curEdit.Location = loc;
            }

            curEdit.Padding = new System.Windows.Forms.Padding(0);
            curEdit.AutoSize = false;

            // 2009/10/24
            if (curEdit.ReadOnly != this.ReadOnly)
                curEdit.ReadOnly = this.ReadOnly;
        }

        internal void InitialFonts()
        {
            /*
            if (this.FixedSizeFont == null)
                this.FixedSizeFont = CreateFixedSizeFont();

            if (this.CaptionFont == null)
                this.CaptionFont = CreateCaptionFont();
             * */
        }

        // 包装版本
        internal void SetEditPos()
        {
            this.SetEditPos(true);
        }

        // 兑现edit控件位置大小到当前字段行上。
        void SetEditPos(bool bFocus)
        {
            if (this.SelectedFieldIndices.Count != 1)
                return;

            int nField = this.FocusedFieldIndex;

            if (this.curEdit == null)
                InitialEditControl();

            if (nField == -1
                || this.FocusedField == null)   // 2008/11/29
            {
                this.curEdit.Text = "";
                this.curEdit.Hide();
                this.curEdit.SelectionStart = 0;
                return;
            }
            else
            {
                this.curEdit.Show();
                if (bFocus == true)
                    this.curEdit.Focus();
            }

            Debug.Assert(nField >= 0 || nField < this.record.Fields.Count, "FocusedFieldIndex下标越界。");

            /*
			if (m_bEditInitialized == false) 
				InitialEditControl();
             * */

            if (this.m_nFocusCol == 1 || this.m_nFocusCol == 2)
                this.curEdit.Overwrite = true;
            else
            {
                if (this.FocusedField.Name == "###")
                    this.curEdit.Overwrite = true;
                else
                    this.curEdit.Overwrite = false;
            }

            // ???
            // controlfield的所在列有限制，需要调整
            if (Record.IsFirstHeaderFieldName(this.FocusedField.Name) == true
                && (this.m_nFocusCol == 1 || this.m_nFocusCol == 2))
            {
                Debug.Assert(false, "第一个头标区的列不能为 1 或 2");
            }
            else if (Record.IsControlFieldName(this.FocusedField.Name) == true
                && this.m_nFocusCol == 2)
            {
                Debug.Assert(false, "控制字段的列不能为 2");
            }

            // 头标区
            if (Record.IsFirstHeaderFieldName(this.FocusedField.Name) == true
                && (this.m_nFocusCol == 1 || this.m_nFocusCol == 2))
            {
                Debug.Assert(false, "第一个头标区的列不能为1或2");
            }

            ChangeEditSizeAndMove(nField, this.m_nFocusCol);
        }

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

        int _historyIndex = 0; // 当前 _changeHistory 中新增事项的位置
        List<ChangeAction> _changeHistory = new List<ChangeAction>();

        public void AppendFieldsDelete(List<Field> old_fields)
        {
#if REMOVED
#if DEBUG
            var first = old_fields[0];
            if (this.record.Fields.IndexOf(first) != 0)
                throw new ArgumentException($"old_field 中的第一项应该是 .record.Fields 中的第一项");
#endif
#endif
            int i = 0;
            foreach (var old_field in old_fields)
            {
                AppendFieldDelete(i, new List<Field>() { old_field });
                i++;
            }
        }

        // indices 里面的索引值和 old_fields 集合中的对象一一对应
        public void AppendFieldsDelete(IEnumerable<int> indices,
            List<Field> old_fields)
        {
            int i = 0;
            foreach (var v in indices)
            {
                AppendFieldDelete(v, new List<Field>() { old_fields[i] });
                i++;
            }
        }

        static int MAX_HISTORY = 1000;

        public void AppendFieldDelete(int start_index,
            List<Field> old_fields)
        {
#if REMOVED
            discardHistory();
            _changeHistory.Add(new ChangeAction
            {
                Action = "FieldDelete",
                Offs = start_index.ToString(),
                OldFields = Field.Clone(old_fields),
                NewFields = new List<Field>()
            });
            _historyIndex = _changeHistory.Count;
            limitHistory();
#endif
            AppendFieldAction("FieldDelete",
                start_index,
                Field.Clone(old_fields),
                new List<Field>());
        }

        public bool CanUndo()
        {
            if (_historyIndex <= 0)
                return false;
            return true;
        }

        public bool CanRedo()
        {
            if (_historyIndex >= _changeHistory.Count)
                return false;
            return true;
        }

        // return:
        //      true    成功
        //      false   被拒绝
        public bool Undo()
        {
            if (_historyIndex <= 0)
                return false;

            bool has_focus = this.Focused || this.curEdit.Focused;
            this.ClearSelectFieldIndices("hidetextbox,invalidate");

            _historyIndex--;
            var item = _changeHistory[_historyIndex];
            undoAction(item);

            if (this.record != null)
            {
                // 把所有字段的高度计算一下
                this.record.CalculateFieldsHeight(0, -1, true);

                // 客户区发生变化后，文档与发生变化
                InvalidateRect iRect = new InvalidateRect();
                iRect.bAll = true;
                this.AfterDocumentChanged(ScrollBarMember.Both,
                    iRect);
            }


            if (has_focus)
            {
                int line = item.TryGetFieldIndex();
                if (line < 0)
                    line = 0;
                int col = item.CaretCol;
                if (col < 1)
                    col = 1;
                int pos = item.CaretPos;
                if (pos < 0)
                    pos = 0;
                if (this.record.Fields.Count > 0)
                    this.SetActiveField(line, col, pos, true);

                // this.Focus();
            }
            return true;
        }

        void undoAction(ChangeAction action)
        {
            // FieldInsert 插入一个或者多个连续的字段 Offs(格式为 field_index) NewFields (OldField 为空) 
            // FieldChange 替换一个或者多个连续的字段 Offs(格式为 field_index) Length NewFields OldFields
            // FieldDelete 删除一个或者多个连续的字段 Offs(格式为 field_index) NewFields(空) OldFields(.Count 表明被删除的字段个数)
            int start_index = Convert.ToInt32(action.Offs);
            if (action.Action == "FieldInsert")
            {
                // 从 offs 位置删除 n 个字段
                if (action.NewFields != null)
                {
                    for (int i = 0; i < action.NewFields.Count; i++)
                    {
                        this.record.Fields._removeAt(start_index);
                    }
                }
                return;
            }
            else if (action.Action == "FieldChange")
            {
                // 在 offs 位置删除 m 个字段
                if (action.NewFields != null)
                {
                    for (int i = 0; i < action.NewFields.Count; i++)
                    {
                        // var field = action.NewFields[i];
                        this.record.Fields._removeAt(start_index);
                    }
                }
                // 在 offs 位置还原 n 个字段
                if (action.OldFields != null)
                {
                    for (int i = 0; i < action.OldFields.Count; i++)
                    {
                        var field = action.OldFields[i];
                        field.container = this.record.Fields;
                        field.Selected = false;
                        this.record.Fields._insert(start_index + i, field);
                    }
                }
                return;
            }
            else if (action.Action == "FieldDelete")
            {
                // 在 offs 位置还原 n 个字段
                if (action.OldFields != null)
                {
                    for (int i = 0; i < action.OldFields.Count; i++)
                    {
                        var field = action.OldFields[i];
                        field.container = this.record.Fields;
                        field.Selected = false;
                        this.record.Fields._insert(start_index + i, field);
                    }
                }
                return;
            }
            else
                throw new ArgumentException($"无法识别的 action.Action '{action.Action}'");
        }

        // return:
        //      true    成功
        //      false   被拒绝
        public bool Redo()
        {
            if (_historyIndex >= _changeHistory.Count)
                return false;

            bool has_focus = this.Focused || this.curEdit.Focused;

            this.ClearSelectFieldIndices("hidetextbox,invalidate");

            var item = _changeHistory[_historyIndex];
            redoAction(item);
            _historyIndex++;

            // TODO: 把 SelectedIndices 复原。是否可以记载在 ChangeAction 中。或者临时推断(比如恢复的字段就是 Focused 的字段)

            if (this.record != null)
            {
                // 把所有字段的高度计算一下
                this.record.CalculateFieldsHeight(0, -1, true);

                // 客户区发生变化后，文档与发生变化
                InvalidateRect iRect = new InvalidateRect();
                iRect.bAll = true;
                this.AfterDocumentChanged(ScrollBarMember.Both,
                    iRect);
            }

            if (has_focus)
            {
                int line = item.TryGetFieldIndex();
                if (line < 0)
                    line = 0;
                int col = item.CaretCol;
                if (col < 1)
                    col = 1;
                int pos = item.CaretPos;
                if (pos < 0)
                    pos = 0;
                this.SetActiveField(line, col, pos, true);

                // this.Focus();
            }
            return true;
        }

        void redoAction(ChangeAction action)
        {
            // FieldInsert 插入一个或者多个连续的字段 Offs(格式为 field_index) NewFields (OldField 为空) 
            // FieldChange 替换一个或者多个连续的字段 Offs(格式为 field_index) Length NewFields OldFields
            // FieldDelete 删除一个或者多个连续的字段 Offs(格式为 field_index) NewFields(空) OldFields(.Count 表明被删除的字段个数)
            int start_index = Convert.ToInt32(action.Offs);
            if (action.Action == "FieldInsert")
            {
                // 在 offs 位置插入 n 个字段
                if (action.NewFields != null)
                {
                    for (int i = 0; i < action.NewFields.Count; i++)
                    {
                        var field = action.NewFields[i];
                        field.container = this.record.Fields;
                        field.Selected = false;
                        this.record.Fields._insert(start_index + i, field);
                    }
                }
                return;
            }
            else if (action.Action == "FieldChange")
            {
                // 在 offs 位置删除 m 个字段，然后插入 n 个字段
                if (action.OldFields != null)
                {
                    for (int i = 0; i < action.OldFields.Count; i++)
                    {
                        this.record.Fields._removeAt(start_index);
                    }
                }
                if (action.NewFields != null)
                {
                    for (int i = 0; i < action.NewFields.Count; i++)
                    {
                        var field = action.NewFields[i];
                        field.container = this.record.Fields;
                        field.Selected = false;
                        this.record.Fields._insert(start_index + i, field);
                    }
                }
                return;
            }
            else if (action.Action == "FieldDelete")
            {
                // 在 offs 位置删除 n 个字段
                if (action.OldFields != null)
                {
                    for (int i = 0; i < action.OldFields.Count; i++)
                    {
                        this.record.Fields._removeAt(start_index);
                    }
                }
                return;
            }
            else
                throw new ArgumentException($"无法识别的 action.Action '{action.Action}'");
        }


        // 丢弃 _historyIndex 以后的集合元素
        void discardHistory()
        {
            while (_changeHistory.Count > _historyIndex)
            {
                _changeHistory.RemoveAt(_historyIndex);
            }
        }

        // 限制修改历史集合的最大尺寸
        void limitHistory()
        {
            while (_changeHistory.Count > MAX_HISTORY)
            {
                _changeHistory.RemoveAt(0);
            }
            if (_historyIndex > _changeHistory.Count)
                _historyIndex = _changeHistory.Count;
        }

        public void AppendInsertFields(int start_index,
            List<Field> new_fields)
        {
            AppendFieldAction(
"FieldInsert",
start_index,
new List<Field>(),
new_fields);
        }

        // 重设 MARC 全部内容以后，调用本函数
        public void AppendReset(List<Field> old_fields,
            List<Field> new_fields)
        {
            AppendFieldAction(
"FieldChange",
0,
old_fields,
new_fields);
        }

        public void AppendFieldChange(
            string action,
            int field_index,
            Field old_field,
            Field new_field)
        {
            AppendFieldAction(
    action,
    field_index,
    old_field == null ? new List<Field>() : new List<Field> { old_field },
    new_field == null ? new List<Field>() : new List<Field> { new_field });
        }
        public void AppendFieldAction(
            string action,
            int field_index,
            List<Field> old_fields,
            List<Field> new_fields)
        {
            discardHistory();
            int caret_pos = 0;
            if (this.curEdit != null)
                caret_pos = this.curEdit.SelectionStart;
            _changeHistory.Add(new ChangeAction
            {
                Action = action,
                Offs = field_index.ToString(),
                OldFields = old_fields == null ? null : Field.Clone(old_fields),
                NewFields = new_fields == null ? null : Field.Clone(new_fields),
                CaretCol = this._focusCol,
                CaretPos = caret_pos,
            });
            _historyIndex = _changeHistory.Count;
            limitHistory();
        }


        int m_nEditControlTextToItemNested = 0;

        // 将Edit控件中的文字内容兑现到nCurLine指向的内存Field对象
        // 不负责修改屏幕图像
        // TODO: 把Edit内容送回内存对象后，应为Edit changed设置false，表示这以后没有额外的改变
        internal void EditControlTextToItem()
        {
            if (m_nEditControlTextToItemNested > 0)
                return; // 防止递归
            m_nEditControlTextToItemNested++;
            try
            {

                // 2008/3/20
                if (this.curEdit == null)
                {
                    // Debug.Assert(false, "");
                    return;
                }

                // 2009/3/6
                // 表示edit内容已经失效，不必送回内存对象了
                if (this.curEdit.ContentIsNull == true)
                    return;

                if (this.FocusedFieldIndex == -1)
                    return;

                Debug.Assert(this.FocusedField != null, "此时FocusedField不可能为null");

                var field = this.FocusedField;
                var field_index = this.record.Fields.IndexOf(field);
                var old_field = field.Clone();
                Field new_field = null;
                if (this.m_nFocusCol == 1)
                {
                    if (field.m_strName != curEdit.Text)
                    {
                        ChangeFieldName(field, curEdit.Text);
                        new_field = field.Clone();
                    }
                }
                else if (this.m_nFocusCol == 2)
                {
                    if (field.m_strIndicator != curEdit.Text)
                    {
                        field.m_strIndicator = curEdit.Text;
                        this.FireTextChanged();
                        new_field = field.Clone();
                    }
                }
                else if (this.m_nFocusCol == 3)
                {
#if BIDI_SUPPORT
                    // string strNewText = curEdit.Text.Replace("\x200e", "");
                    string strNewText = RemoveBidi(curEdit.Text);
                    if (field.m_strValue != strNewText)
                    {
                        field.m_strValue = strNewText;
                        this.FireTextChanged();
                        new_field = field.Clone();
                    }
#else
                    if (this.FocusedField.m_strValue != curEdit.Text)
                    {
                        this.FocusedField.m_strValue = curEdit.Text;
                        this.FireTextChanged();
                    }
#endif
                }
                else
                {
                    Debug.Assert(false, "列号不正确。");
                }

                if (new_field != null)
                    AppendFieldChange("FieldChange",
                        field_index,
                        old_field,
                        new_field);
            }
            finally
            {
                m_nEditControlTextToItemNested--;
            }
        }

        // 将FocusedFieldIndex指向的内存Field对象中的文字内容兑现到Edit控件
        // 不负责修改屏幕图像，Edit控件位置大小可能不正确
        // TODO: 把内存对象内容送给Edit后，应为Edit changed设置false，表示这时候两者内容是一致的
        // 送以前要检查Edit changed是否已经为true，如果那样，表明上一次的eidt内容尚未兑现到内存
        // parameters:
        //      caret_pos 插入符需要设置到的字符位置。如果为 -1 表示不设置插入符位置
        internal void ItemTextToEditControl(int caret_pos = 0)
        {
            if (this.FocusedFieldIndex == -1)
            {
                this.curEdit.Text = "";
                return;
            }

            // 2024/7/17
            if (this.DesignMode && this.FocusedField == null)
                return;

            Debug.Assert(this.FocusedField != null, "此时FocusedField不可能为null");

            var field = this.FocusedField;
            var col = this.m_nFocusCol;

            /*
            // 在 curEdit 中记载所编辑的局部内容处在整个 MARC 记录机内格式中的偏移
            this.curEdit.Offs = this.record.GetFieldOffs(field, col);
            */
            if (col == 1)
            {
                if (field.m_strName != curEdit.Text)
                {
                    curEdit.Text = field.m_strName;
                }
            }
            else if (this.m_nFocusCol == 2)
            {
                if (field.m_strIndicator != curEdit.Text)
                {
                    curEdit.Text = field.m_strIndicator;
                }
            }
            else if (this.m_nFocusCol == 3)
            {
#if BIDI_SUPPORT
                // string strNewValue = this.FocusedField.m_strValue.Replace(new string(Record.KERNEL_SUBFLD, 1), "\x200e" + new string(Record.KERNEL_SUBFLD, 1));

                string strNewValue = AddBidi(field.m_strValue);
                if (strNewValue != curEdit.Text)
                {
                    curEdit.Text = strNewValue;
                }
#else
                if (this.FocusedField.m_strValue != curEdit.Text)
                {
					curEdit.Text = this.FocusedField.m_strValue;
                }
#endif
            }
            else
            {
                Debug.Assert(false, "列号不正确。");
            }

            curEdit.ContentIsNull = false;

            // 为了避免在最后一个字段的字段名最后一个字符输入后，小edit转向指示符域的时候，当前焦点突然不在可视范围内了的一个bug
            if (caret_pos != -1)
            {
                if (caret_pos > curEdit.Text.Length)
                    caret_pos = curEdit.Text.Length;
                curEdit.SelectionStart = caret_pos;
                curEdit.SelectionLength = 0;
            }
        }

        // 添加 BIDI 字符
        public static string AddBidi(string value)
        {
            // return value.Replace(new string(Record.KERNEL_SUBFLD, 1), "\x200e" + new string(Record.KERNEL_SUBFLD, 1));
            StringBuilder result = new StringBuilder();
            int delta = -1;
            foreach (var ch in value.ToCharArray())
            {

                if (ch == Record.KERNEL_SUBFLD)
                {
                    result.Append((char)0x200e);  // 0x200e
                    delta = 0;
                }

                if (BidiAdjust)
                {
                    // $9 后面加一个空格字符
                    if (delta == 2 && ch != ' ' && result.Length > 0 && char.IsDigit(result[result.Length - 1]))
                        result.Append((char)' ');  // 0x202c
                }

                result.Append(ch);

                if (delta >= 0)
                    delta++;
            }

            return result.ToString();
        }

        // 移走 BIDI 字符
        public static string RemoveBidi(string value)
        {
            // return value.Replace("\x200e", "");
            StringBuilder result = new StringBuilder();
            int delta = -1;
            foreach (var ch in value.ToCharArray())
            {
                if (ch == (char)0x200e)
                    continue;

                if (ch == Record.KERNEL_SUBFLD)
                {
                    delta = 0;
                }

                // $9 后面删除一个空格字符
                if (BidiAdjust)
                {
                    if (delta == 2 && ch == ' ')
                        continue;
                }

                result.Append(ch);

                if (delta >= 0)
                    delta++;
            }

            return result.ToString();
        }

        #endregion

        #region 重载的一些函数 HandleCreated paint

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

            /*
			if (this.m_strDefaultNameCaptionBackColor != null && this.m_strDefaultNameCaptionBackColor != "")
				this.DefaultNameCaptionBackColor = ColorUtil.String2Color(this.m_strDefaultNameCaptionBackColor);

			if (m_strDefaultNameBaceColor != null && m_strDefaultNameBaceColor != "")
				this.DefaultNameBackColor = ColorUtil.String2Color(this.m_strDefaultNameBaceColor);
			
			if (m_strDefaultIndicatorBaceColor != null && m_strDefaultIndicatorBaceColor != "")
				this.DefaultIndicatorBackColor = ColorUtil.String2Color(this.m_strDefaultIndicatorBaceColor);
			
			if (m_strDefaultIndicatorBaceColorNotEdit != null && m_strDefaultIndicatorBaceColorNotEdit != "")
				this.DefaultIndicatorBackColorNotEdit = ColorUtil.String2Color(this.m_strDefaultIndicatorBaceColorNotEdit);

			if (m_strDefaultValueBaceColor != null && m_strDefaultValueBaceColor != "")
				this.DefaultValueBackColor = ColorUtil.String2Color(this.m_strDefaultValueBaceColor);
             */

            // this.record = new Record(this);
        }

#if NO
        void EnsureCreateRecord()
        {
            if (this.record == null)
                this.record = new Record(this);
        }
#endif

        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.SizeChanged 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.EventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.SizeChanged 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.EventArgs。</param>
        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnSizeChanged(e);
                return;
            }

            base.OnSizeChanged(e);

            if (this.record != null)
            {
                // 把所有字段的高度计算一下
                this.record.CalculateFieldsHeight(0, -1, true);

                // 客户区发生变化后，文档与发生变化
                InvalidateRect iRect = new InvalidateRect();
                iRect.bAll = true;
                this.AfterDocumentChanged(ScrollBarMember.Both,
                    iRect);
            }
        }

        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.Paint 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.PaintEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.Paint 事件。
        /// </summary>
        /// <param name="pe">包含事件数据的 System.Windows.Forms.PaintEventArgs。</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.DesignMode && this.record == null)
            {
                // TODO: 可以显示警告文字
                using (Brush brush = new SolidBrush(Color.Red))
                {
                    pe.Graphics.FillRectangle(brush, new Rectangle(0, 0, 1000, 1000));
                }
                return;
            }

            /*
            pe.Graphics.SmoothingMode =
System.Drawing.Drawing2D.SmoothingMode.HighQuality;
             */

            int x = this.LeftBlank + this.DocumentOrgX;
            int y = this.TopBlank + this.DocumentOrgY;

            /*
            // 2010/12/16
            if (this.m_fixedSizeFont == null)
                this.m_fixedSizeFont = this.CreateFixedSizeFont();
            if (this.m_captionFont == null)
                this.m_captionFont = this.CreateCaptionFont();
            */
            int focus_line = -1;
            if (this.SelectedFieldIndices.Count == 1
                && this.curEdit.Visible)
                focus_line = this.SelectedFieldIndices[0];

            this.record?.Paint(pe, x, y, focus_line);
        }

        /// <summary>
        /// 刷新字段名提示区域
        /// </summary>
        public void RefreshNameCaption()
        {
            if (this.record != null)
            {
                this.record.RefreshNameCaption();
                this.Invalidate();  // 2007/12/26
            }
        }

        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.MouseDown 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.MouseEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.MouseDown 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.Windows.Forms.MouseEventArgs。</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnMouseDown(e);
                return;
            }

            //this.WriteErrorInfo("走到OnMouseDown里");
            this.Capture = true;

            ClearBlockMode();

            Point p = new Point(e.X, e.Y);

            // 测算单击点的位置
            // parameters:
            //		p	输入位置点
            //		nField	out参数，返回所在的字段
            //		nCol	out参数，返回所在的列号
            // return: 
            //		-1	不在record上
            //		0	在record内 
            //		1	在缝隙上
            // 注: 文档的上下左右空白不算record
            int nRet = HitTest(p,
                out int nField,
                out int nCol);
            if (nRet == 1)
            {
                nDragCol = nCol;
                // 第一次
                nLastTrackerX = e.X;
                DrawTraker();
                goto END1;
            }
            if (nRet == 0)
            {
                if (nField == -1 || nCol == -1)
                    Debug.Assert(false, "不可能的情况");

                // 2024/7/5
                if (e.Button == MouseButtons.Right)
                {
                    PopupMenu(new Point(e.X, e.Y));
                    return;
                }

                // 如果是右键菜单，且在选中项上单击，而且是第一列, 则跳过，不再做选中某项的事情
                if (e.Button == MouseButtons.Right
                    && nCol == 0)
                {
                    if (this.SelectedFieldIndices.IndexOf(nField) != -1)
                    {
                        PopupMenu(new Point(e.X, e.Y));
                        return;
                    }
                }

                // 如果当前选项为一项，且在自身上单击
                if (this.SelectedFieldIndices.Count == 1
                    && this.FocusedFieldIndex == nField)
                {
                    if (this.m_nFocusCol != nCol
                        || this.curEdit.Visible == false/* 2024/7/5 */)
                    {
                        this.SetActiveField(nField, nCol);

                        // 下面这段代码是把插入符定位到单击的地方
                        int x = e.X - curEdit.Location.X;
                        int y = e.Y - curEdit.Location.Y;

                        API.SendMessage(curEdit.Handle,
                            API.WM_LBUTTONDOWN,
                            new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                            API.MakeLParam(x, y));
                        /*
                        // **
                        API.SendMessage(curEdit.Handle,
    API.WM_LBUTTONUP,
    new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
    API.MakeLParam(x, y));
                         * */
                        if (e.Button == MouseButtons.Right)
                        {
                            PopupMenu(new Point(e.X, e.Y));
                            return; // add
                        }
                        goto END1;  // changed
                        // return;
                    }

                    goto END1;
                }


                // 如果同时按下control键
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    this.nStartFieldIndex = nField;

                    // 如果已经存在选中项了，则隐藏小Edit控件
                    if (this.SelectedFieldIndices.Count > 0)
                    {
                        this.HideTextBox();
                    }
                    this.AddSelectedField(nField, nField, false);
                }
                else if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    // 如果还没有设起始键，则设一下，如果已设则不能再设了
                    if (this.nStartFieldIndex == -1)
                    {
                        if (this.SelectedFieldIndices.Count > 0)
                            nStartFieldIndex = (int)this.SelectedFieldIndices[this.SelectedFieldIndices.Count - 1];
                        else
                            nStartFieldIndex = nField;
                    }

                    // 如果已经存在选中项了，则隐藏小Edit控件
                    if (this.SelectedFieldIndices.Count > 0)
                    {
                        this.HideTextBox();
                    }

                    this.AddSelectedField(this.nStartFieldIndex, nField, true);
                }
                else
                {
                    nStartFieldIndex = nField;
                    /*
					if (nCol == 0)
						nCol = 3;
                     */
                    this.SetActiveField(nField, nCol);

                    // 下面这段代码是把插入符定位到单击的地方
                    int x = e.X - curEdit.Location.X;
                    int y = e.Y - curEdit.Location.Y;

                    API.SendMessage(curEdit.Handle,
                        API.WM_LBUTTONDOWN,
                        new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
                        API.MakeLParam(x, y));
                    /*
                    // **
                    API.SendMessage(curEdit.Handle,
    API.WM_LBUTTONUP,
    new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
    API.MakeLParam(x, y));
                     * */
                    if (e.Button == MouseButtons.Right)
                    {
                        PopupMenu(new Point(e.X, e.Y));
                        return; // add
                    }

                    goto END1;  // changed
                    // return;
                }
            }

            if (nRet == -1)
            {
                // 2024/7/30
                if (e.Button == MouseButtons.Right)
                {
                    PopupMenu(new Point(e.X, e.Y));
                    return;
                }

                if (e.Button == MouseButtons.Left)
                    this.Focus();
            }

        END1:
            base.OnMouseDown(e);
        }

        // 是否处在定义块的状态
        internal bool _block_mode = false;
        // 连续拖动定义字段块时，开始的字段编号
        internal int _nCurrentFieldIndex = -1;

        /*
         * 进入方向键定义字段块的状态：
         * nStartFieldIndex 表示起点字段号
         * _nCurrentFieldIndex 表示当前字段号
         * 在响应击键的模块，可以用 _nCurrentFieldIndex 得知当前所在的字段编号。减一就是上一个字段，加一就是下一个字段
         * 当方向键移动的时候，就不断修改 _nCurrentFieldIndex 的值，并且不断修改 SelectedIndices 集合
         * 这时候，小 edit 应该是隐藏的状态。可以考虑用一个虚线框子显示 focus 所在的字段
         * */

#if REMOVED
        internal bool CaretMove(int field_no, bool shift)
        {
            if (shift == false)
            {
                // 确保当前唯一选定的字段为 field_no
                nStartFieldIndex = field_no;
                _nCurrentFieldIndex = field_no;
                _block_mode = false;
            }
            else // shift 按下
            {
                if (_block_mode == false)
                {
                    _block_mode = true;
                    nStartFieldIndex = field_no;
                    _nCurrentFieldIndex = field_no;
                    this.AddSelectedField(field_no, field_no, true);
                }
                else
                {
                    _nCurrentFieldIndex = field_no;
                    this.AddSelectedField(nStartFieldIndex, field_no, true);
                }
            }

            if (_block_mode)
                return true;
            return false;
        }
#endif

        internal bool BlockStart(int direction)
        {
            if (Control.ModifierKeys != Keys.Shift)
            {
                _block_mode = false;
                return false;
            }

            if (_block_mode == false)
            {
                _block_mode = true;
                nStartFieldIndex = this.FocusedFieldIndex;
                _nCurrentFieldIndex = nStartFieldIndex;
                if (direction == -1)
                {
                    if (_nCurrentFieldIndex > 0)
                        _nCurrentFieldIndex--;
                }
                else if (direction == 1)
                {
                    if (_nCurrentFieldIndex < this.record.Fields.Count - 1)
                        _nCurrentFieldIndex++;
                }

                this.AddSelectedField(nStartFieldIndex, _nCurrentFieldIndex, true);
                EnsureCurrentVisible();
                return true;
            }

            return false;
        }

        // 块定义状态，做一次 up 动作
        internal bool BlockUp()
        {
            if (_block_mode == false)
                return false;

            bool shift = Control.ModifierKeys == Keys.Shift;
            if (shift == false)
            {
                LeaveBlockMode();
                return true;
            }

            if (_nCurrentFieldIndex > 0)
            {
                _nCurrentFieldIndex--;
                this.AddSelectedField(nStartFieldIndex, _nCurrentFieldIndex, true);
                EnsureCurrentVisible();
            }

            return true;
        }

        internal bool BlockDown()
        {
            if (_block_mode == false)
                return false;

            bool shift = Control.ModifierKeys == Keys.Shift;
            if (shift == false)
            {
                LeaveBlockMode();
                return true;
            }

            if (_nCurrentFieldIndex < this.record.Fields.Count - 1)
            {
                _nCurrentFieldIndex++;
                this.AddSelectedField(nStartFieldIndex, _nCurrentFieldIndex, true);
                EnsureCurrentVisible();
            }

            return true;
        }

        void LeaveBlockMode()
        {
            // 脱离 block mode。并 SetActiveField() 到 _nCurrentFieldIndex 字段
            _block_mode = false;
            this.SetActiveField(_nCurrentFieldIndex, 1);
            // this.curEdit.SelectionStart = 0;
        }

        List<string> _setFocus_parameters = new List<string>();
        void AddFocusParameter(string text, bool clear_before)
        {
            lock (_setFocus_parameters)
            {
                if (clear_before || this._setFocus_parameters.Count > 100)
                    this._setFocus_parameters.Clear();

                this._setFocus_parameters.Add(text);
            }
        }

        string GetFocusParameter()
        {
            lock (_setFocus_parameters)
            {
                if (_setFocus_parameters.Count == 0)
                    return null;
                string value = _setFocus_parameters[0];
                _setFocus_parameters.RemoveAt(0);
                return value;
            }
        }

        void EnsureCurrentVisible()
        {
            this.HideTextBox();

            if (this.Focused == false)
            {
                // 希望执行 WM_SETFOCUS 时不要自动调用 SetEditPos();
                this.AddFocusParameter("dont_seteditpos", true);
                this.Focus();
                // Debug.WriteLine("EnsureCurrentVisible SetFocus()");
            }

            var field = this.Record.Fields[_nCurrentFieldIndex];

            EnsureVisible(_nCurrentFieldIndex,
                3,
                new Rectangle(0, 0, 10, field.TotalHeight));
        }

        // 清除字段块标志
        internal void ClearBlockMode()
        {
            _block_mode = false;
        }

        void DrawTraker()
        {
            Point p1 = new Point(nLastTrackerX, 0);
            p1 = this.PointToScreen(p1);

            Point p2 = new Point(nLastTrackerX, this.ClientSize.Height);
            p2 = this.PointToScreen(p2);

            ControlPaint.DrawReversibleLine(p1,
                p2,
                SystemColors.Control);
        }

        // 重载鼠标移动事件
        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.MouseMove 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.MouseEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.MouseMove 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.Windows.Forms.MouseEventArgs。</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnMouseMove(e);
                return;
            }

            if (nDragCol != -1)
            {
                Cursor = Cursors.SizeWE;

                // 消上次残余的一根
                DrawTraker();

                nLastTrackerX = e.X;

                // 绘制本次的一根
                DrawTraker();
            }
            else
            {
                Point p = new Point(e.X, e.Y);

                int nCol, nField;
                // parameters:
                //		p	输入位置点
                //		nField	out参数，返回所在的字段
                //		nCol	out参数，返回所在的列号
                //				0 字段说明;
                //				1 字段名;
                //				2 字段指示符
                //				3 字段内容
                // return: 
                //		-1	不在record上
                //		0	在record内 
                //		1	在缝隙上
                int nRet = HitTest(
                    p,
                    out nField,
                    out nCol);
                if (nRet == 0)
                {
                    if (nCol == 1 || nCol == 3)
                    {
                        Cursor = Cursors.IBeam;
                    }
                    else if (nCol == 2)
                    {
                        Field field = this.record.Fields[nField];
                        if (Record.IsControlFieldName(field.Name) == false)
                            Cursor = Cursors.IBeam;
                        else
                            Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        Cursor = Cursors.Arrow;
                    }
                }
                else if (nRet == 1)
                    Cursor = Cursors.SizeWE;
                else
                    Cursor = Cursors.Arrow;
            }

            base.OnMouseMove(e);
        }

        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.MouseUp 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.MouseEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.MouseUp 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.Windows.Forms.MouseEventArgs。</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnMouseUp(e);
                return;
            }

            this.Capture = false;

            if (nDragCol != -1)
            {
                // 消最后残余的一根
                DrawTraker();

                // 做改变列宽度的事情
                int x0 = 0;
                int delta = 0;

                // 把拖动的起点位置x折算成屏幕坐标形态
                if (nDragCol == 0)
                {
                    x0 = this.DocumentOrgX + this.LeftBlank + this.record.NameCaptionTotalWidth;
                }
                else
                {
                }

                // 计算差额
                delta = nLastTrackerX - x0;
                if (delta != 0)
                {
                    if (nDragCol == 0)
                    {
                        this.record.NameCaptionPureWidth += delta;
                        if (this.record.NameCaptionTotalWidth >= this.ClientWidth)
                            this.record.NameCaptionPureWidth -= 20;
                        if (this.record.NameCaptionPureWidth < 0)
                            this.record.NameCaptionPureWidth = 0;
                    }
                    // 迫使每个单元重新测算高度
                    this.record.CalculateFieldsHeight(0, -1, true);
                }

                nLastTrackerX = -1;
                nDragCol = -1;

                this.DocumentOrgX = this.DocumentOrgX;

                // 客户区发生变化后，文档与发生变化
                InvalidateRect iRect = new InvalidateRect();
                iRect.bAll = true;
                this.AfterDocumentChanged(ScrollBarMember.Both,
                    iRect);
            }

            base.OnMouseUp(e);
            /*
			if(e.Button == MouseButtons.Right)
			{	
				PopupMenu(new Point(e.X, e.Y) );
			}
             */
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

                if (this.record == null)
                    return 100;

                return this.record.NameCaptionPureWidth;
            }
            set
            {
                if (this.DesignMode)
                    return;

                if (this.record == null)
                    return;

                this.record.NameCaptionPureWidth = value;

                /*
				if (this.record.NameCaptionTotalWidth >= this.ClientWidth)
					this.record.NameCaptionPureWidth -= 20;
                 * */

                if (this.record.NameCaptionPureWidth < 0)
                    this.record.NameCaptionPureWidth = 0;

                // 迫使每个单元重新测算高度
                this.record.CalculateFieldsHeight(0, -1, true);

                this.DocumentOrgX = this.DocumentOrgX;

                // 客户区发生变化后，文档与发生变化
                InvalidateRect iRect = new InvalidateRect();
                iRect.bAll = true;
                this.AfterDocumentChanged(ScrollBarMember.Both,
                    iRect);
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

        // 在已有的菜单事项上追加事项
        // parameters:
        //      bFull   是否包含一些重复事项
        internal void AppendMenu(ContextMenu contextMenu,
            bool bFull)
        {
            MenuItem menuItem;
            MenuItem subMenuItem;

            // 插入字段(询问字段名)
            menuItem = new MenuItem("插入新字段(询问字段名)");// + strName);
            menuItem.Click += new System.EventHandler(this.InsertField);
            if (this.SelectedFieldIndices.Count > 1
                || this.ReadOnly == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // 插入字段
            menuItem = new MenuItem("插入新字段");
            if (this.ReadOnly == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ** 子菜单

            // 前插
            subMenuItem = new MenuItem("前插");
            subMenuItem.Click += new System.EventHandler(this.InsertBeforeFieldNoDlg);
            menuItem.MenuItems.Add(subMenuItem);
            if (this.SelectedFieldIndices.Count == 1)
            {
                // 头标区不能修改字段名
                if (this.FocusedField.m_strName == "###")
                    subMenuItem.Enabled = false;
                else
                    subMenuItem.Enabled = true;
            }
            else
            {
                subMenuItem.Enabled = false;
            }

            //后插
            subMenuItem = new MenuItem("后插");
            subMenuItem.Click += new System.EventHandler(this.InsertAfterFieldWithoutDlg);
            menuItem.MenuItems.Add(subMenuItem);
            if (this.SelectedFieldIndices.Count == 1)
            {
                subMenuItem.Enabled = true;
            }
            else
            {
                subMenuItem.Enabled = false;
            }

            //末尾
            subMenuItem = new MenuItem("末尾");
            subMenuItem.Click += new System.EventHandler(this.AppendFieldNoDlg);
            menuItem.MenuItems.Add(subMenuItem);


            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 撤消
            menuItem = new MenuItem("撤消(&U)\tCtrl+Z");
            menuItem.Click += new System.EventHandler(this.menuItem_Undo);
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Enabled = this.CanUndo();

            // 重做
            menuItem = new MenuItem("重做(&R)\tCtrl+Y");
            menuItem.Click += new System.EventHandler(this.menuItem_Redo);
            menuItem.Enabled = this.CanRedo();
            contextMenu.MenuItems.Add(menuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 剪切
            menuItem = new MenuItem("剪切字段(Ctrl+X)");// + strName);
            menuItem.Click += new System.EventHandler(this.menuItem_Cut);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count > 0
                && this.ReadOnly == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //复制
            menuItem = new MenuItem("复制字段(Ctrl+C)");// + strName);
            menuItem.Click += new System.EventHandler(this.menuItem_Copy);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            IDataObject ido = GetClipboardDataObject(); // Clipboard.GetDataObject();

            // 插入字段
            menuItem = new MenuItem("从特定格式粘贴 ...");
            if (this.ReadOnly == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            {
                // 从dp2OPAC粘贴整个记录
                subMenuItem = new MenuItem("从 dp2OPAC 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromDp2OPAC);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 从 NLC 粘贴整个记录
                subMenuItem = new MenuItem("从 NLC 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromNlcMarc);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 从tcmarc粘贴整个记录
                subMenuItem = new MenuItem("从 tcmarc 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromTcMarc);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 从 XML 粘贴整个记录
                subMenuItem = new MenuItem("从 MARCXML 粘贴整个记录");// + strName);
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromMarcXml);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 2021/12/16
                // 从 工作单 粘贴整个记录
                subMenuItem = new MenuItem("从 工作单 粘贴整个记录");
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromWorksheet);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)    // 原来是==1
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;

                // 2024/5/20
                // 从 机内格式 粘贴整个记录
                subMenuItem = new MenuItem("从 机内格式 粘贴整个记录");
                subMenuItem.Click += new System.EventHandler(this.menuItem_PasteFromJinei);
                menuItem.MenuItems.Add(subMenuItem);
                if (ido.GetDataPresent(DataFormats.Text)
                    && this.ReadOnly == false)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;
            }

            //粘贴覆盖
            menuItem = new MenuItem("粘贴覆盖字段");// + strName);
            menuItem.Click += new System.EventHandler(this.menuItem_PasteOverwrite);
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text)
                && this.SelectedFieldIndices.Count >= 1
                && this.ReadOnly == false)    // 原来是==1
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //粘贴插入
            menuItem = new MenuItem("粘贴插入字段");
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text) == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // ** 子菜单
            subMenuItem = new MenuItem("前插");// + strName );
            subMenuItem.Click += new System.EventHandler(this.menuItem_PasteInsert_InsertBefore);
            menuItem.MenuItems.Add(subMenuItem);
            if (ido.GetDataPresent(DataFormats.Text) == true)
            {
                if (this.SelectedFieldIndices.Count == 1)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;
            }
            else
                subMenuItem.Enabled = false;

            subMenuItem = new MenuItem("后插");
            subMenuItem.Click += new System.EventHandler(this.menuItem_PasteInsert_InsertAfter);
            menuItem.MenuItems.Add(subMenuItem);
            if (ido.GetDataPresent(DataFormats.Text))
            {
                if (this.SelectedFieldIndices.Count == 1)
                    subMenuItem.Enabled = true;
                else
                    subMenuItem.Enabled = false;
            }
            else
                subMenuItem.Enabled = false;

            subMenuItem = new MenuItem("末尾");
            subMenuItem.Click += new System.EventHandler(this.menuItem_PasteInsert_AppendChild);
            menuItem.MenuItems.Add(subMenuItem);
            if (ido.GetDataPresent(DataFormats.Text))
            {
                subMenuItem.Enabled = true;
            }
            else
                subMenuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("整理");
            contextMenu.MenuItems.Add(menuItem);

            // 
            subMenuItem = new MenuItem("字段重新排序(&S)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_sortFields);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("删除全部空字段、子字段(&D)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_removeEmptyFieldsSubfields);
            menuItem.MenuItems.Add(subMenuItem);


            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("删除全部空子字段(&D)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_removeEmptySubfields);
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("删除全部空字段(&D)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_removeEmptyFields);
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("-");
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("平行模式(&P)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_toParallel);
            menuItem.MenuItems.Add(subMenuItem);

            // 
            subMenuItem = new MenuItem("880 模式(&P)");
            subMenuItem.Click += new System.EventHandler(this.menuItem_to880);
            menuItem.MenuItems.Add(subMenuItem);

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 删除
            menuItem = new MenuItem("删除字段");
            menuItem.Click += new System.EventHandler(this.DeleteFieldWithDlg);
            contextMenu.MenuItems.Add(menuItem);
            if (this.SelectedFieldIndices.Count > 0
                && this.ReadOnly == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;


            if (bFull == true)
            {
                //--------------
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);

                // 定长模板
                // TODO: 当MarcEditor为ReadOnly状态时，定长也应该是ReadOnly状态。或者至少要在修改后确定时警告
                menuItem = new MenuItem("定长模板");
                menuItem.Click += new System.EventHandler(this.GetValueFromTemplate);
                contextMenu.MenuItems.Add(menuItem);
                if (this.SelectedFieldIndices.Count == 1)
                    menuItem.Enabled = true;
                else
                    menuItem.Enabled = false;

            }

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 全选
            menuItem = new MenuItem("全选字段(&A)");
            menuItem.Click += new System.EventHandler(this.Menu_SelectAll);
            contextMenu.MenuItems.Add(menuItem);
            if (this.record.Fields.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 复制机内格式到剪贴板
            menuItem = new MenuItem("复制整个记录(机内格式)");
            menuItem.Click += new System.EventHandler(this.CopyJineiToClipboard);
            contextMenu.MenuItems.Add(menuItem);
            if (this.record.Fields.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 复制工作单到剪贴板
            menuItem = new MenuItem("复制整个记录(工作单格式)");
            menuItem.Click += new System.EventHandler(this.CopyWorksheetToClipboard);
            contextMenu.MenuItems.Add(menuItem);
            if (this.record.Fields.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 2024/5/20
            // 从 机内格式 粘贴整个记录
            menuItem = new MenuItem("粘贴整个记录(机内格式)");
            menuItem.Click += new System.EventHandler(this.menuItem_PasteFromJinei);
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text)
                && this.ReadOnly == false)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            // 从 工作单 粘贴整个记录
            menuItem = new MenuItem("粘贴整个记录(工作单格式)");
            menuItem.Click += new System.EventHandler(this.menuItem_PasteFromWorksheet);
            contextMenu.MenuItems.Add(menuItem);
            if (ido.GetDataPresent(DataFormats.Text)
                && this.ReadOnly == false)    // 原来是==1
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            /*
                        //--------------
                        menuItem = new MenuItem ("-");
                        contextMenu.MenuItems.Add(menuItem);

                        // Marc
                        menuItem = new MenuItem("查看Marc");
                        menuItem.Click += new System.EventHandler(this.ShowMarc);
                        contextMenu.MenuItems.Add(menuItem);
                        if (this.record.Count > 0)
                            menuItem.Enabled = true;
                        else
                            menuItem.Enabled = false;

            */

            /*
            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 从右向左的阅读顺序
            menuItem = new MenuItem("从右向左的阅读顺序(R)");
            menuItem.Click += new System.EventHandler(this.Menu_r2l);
            if (this.RightToLeft == RightToLeft.Yes)
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);
            */

            //--------------
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 属性
            menuItem = new MenuItem("属性");
            menuItem.Click += new System.EventHandler(this.Property_menu);
            contextMenu.MenuItems.Add(menuItem);
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
            string strText = "";
            for (int i = 0; i < this.record.Fields.Count; i++)
            {
                Field field = this.record.Fields[i];
                strText += field.GetFieldMarc(true);
            }
            MarcEditor.TextToClipboardFormat(strText);
        }

        // 复制工作单格式的完整记录到剪贴板
        void CopyWorksheetToClipboard(object sender, EventArgs e)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;
            string strError = "";
            List<string> lines = null;

            // 将机内格式变换为工作单格式
            // return:
            //      -1  出错
            //      0   成功
            int nRet = MarcUtil.CvtJineiToWorksheet(
                this.Marc,
                -1,
                out lines,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strText = "";
            foreach (string line in lines)
            {
                strText += line + "\r\n";
            }

            // 按住 Ctrl 键，则功能变为把子字段符号变化为 '$'，便于写书什么的
            if (bControl)
                strText = strText.Replace("ǂ", "$");

            MarcEditor.TextToClipboard(strText);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        internal void menuItem_sortFields(object sender, EventArgs e)
        {
            bool bHasFocus = this.Focused;

            this.Flush();
            if (this.curEdit != null)
                this.curEdit.ContentIsNull = true;    // 防止后面调用时送回内存

            var old_fields = Field.Clone(GetFields(0, this.record.Fields.Count));
            this.Record.Fields.Sort(new FieldComparer());
            this.Invalidate();

            this.AppendFieldAction("FieldChange",
                0,
                old_fields,
                this.record.Fields);

            // 设第一个节点为当前活动焦点
            if (bHasFocus == true)
            {
                if (this.record.Fields.Count > 0)
                    this.SetActiveField(0, 3, true);
            }

            // 2021/10/28
            this.FireTextChanged();
        }

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

        // 右键弹出上下文菜单
        private void PopupMenu(Point p)
        {
            if (this.curEdit.Visible == false)
            // if (this.m_nFocusCol == 0 || this.SelectedFieldIndices.Count > 1)
            {
                // 小 edit 不可见时，包括选择了一个或者多个字段的各种情况
                ContextMenu contextMenu = new ContextMenu();
                this.AppendMenu(contextMenu, true);
                contextMenu.Show(this, p);
            }
            else
            {
                // 小 edit 可见时

                this.curEdit.PopupMenu(this, p);

                /*
                this.curEdit.PopupMenu(this.curEdit,
                    this.curEdit.PointToClient(this.PointToScreen(p) ) );
                 */
            }

            /*
             */
        }


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

            /*
            // 防止残留的字体影响
            this.m_fixedSizeFont = null;
            this.m_captionFont = null;
             * */

            if (this.record != null)
                this.record.InitializeWidth();

            this.CalcuAllHeight();

            // 是不是还要注意改变curEdit的font?
            this.SetEditPos();

            base.OnFontChanged(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (this.ReadOnly == false
                && (Control.ModifierKeys & (Keys.Control | Keys.Alt)) == 0)
            {

                // 当小 edit 隐藏的情况下，击键消息会发到这里来。
                // 一般的内容字符，需要先观察当前是否存在块定义。如果存在，
                // 则，清除块内容。然后把这个字符作为字段名的第一个字符，创建
                // 一个全新的字段，最后把输入焦点放到这个字段的字段名上
                if (this.SelectedFieldIndices.Count > 0)
                {
                    int start_line = this.SelectedFieldIndices.Min();
                    DeleteFieldWithDlg(false);

                    var field = this.InsertField(start_line,
    e.KeyChar + "  ",
    "  ", //指示符
    "");
                    this.SetActiveField(field, 1);
                    this.curEdit.SelectionStart = 1;

                    e.Handled = true;
                }
            }

            base.OnKeyPress(e);
        }

#if REMOVED
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    {
                        if (this.SelectedFieldIndices.Count > 0)
                            this.DeleteFieldWithDlg();
                        // e.Handled = true;
                    }
                    break;
                case Keys.Up:
                    if (this.BlockUp())
                    {
                        //e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Down:
                    if (this.BlockDown())
                    {
                        //e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Left:
                    if (this.BlockUp())
                    {
                        //e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Right:
                    if (this.BlockDown())
                    {
                        //e.Handled = true;
                        return;
                    }
                    break;
                default:
                    break;
            }

            base.OnPreviewKeyDown(e);
        }
#endif
        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.KeyDown 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.KeyEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.KeyDown 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.Windows.Forms.KeyEventArgs。</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnKeyDown(e);
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Delete:
                case Keys.Back:
                    {
                        if (this.SelectedFieldIndices.Count > 0)
                        {
                            this.DeleteFieldWithDlg();
                            e.Handled = true;
                            /*
                            {
                                int start_line = this.SelectedFieldIndices.Min();
                                DeleteFieldWithDlg(false);
                                if (start_line < this.Record.Fields.Count)
                                    AddSelectedField(start_line, start_line, true);
                            }
                            */
                        }
                    }
                    break;
                case Keys.Up:
                    if (this.BlockUp())
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Down:
                    if (this.BlockDown())
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Left:
                    if (this.BlockUp())
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.Right:
                    if (this.BlockDown())
                    {
                        e.Handled = true;
                        return;
                    }
                    break;
                case Keys.A:
                    if (e.Control == true)
                    {
                        MessageBox.Show(this, "MarcEditor OnKeyDown catch Ctrl+A");
                        e.Handled = true;
                    }
                    break;
                case Keys.X:
                    if (e.Control)
                    {
                        this.menuItem_Cut(null, null);
                        e.Handled = true;
                    }
                    break;
                case Keys.Z:
                    if (e.Control)
                    {
                        this.menuItem_Undo(null, null);
                        e.Handled = true;
                    }
                    break;
                case Keys.Y:
                    if (e.Control)
                    {
                        this.menuItem_Redo(null, null);
                        e.Handled = true;
                    }
                    break;
                case Keys.C:
                    if (e.Control)
                    {
                        this.menuItem_Copy(null, null);
                        e.Handled = true;
                    }
                    break;
                case Keys.V:
                    if (e.Control)
                    {
                        this.menuItem_PasteOverwrite(null, null);
                        e.Handled = true;
                    }
                    break;
                case Keys.U:
                    if (e.Control)
                    {
                        GenerateDataEventArgs ea = new GenerateDataEventArgs();
                        this.OnVerifyData(ea);
                        e.Handled = true;
                    }
                    break;
                default:
                    break;
            }

            base.OnKeyDown(e);
        }

        //
        // 摘要:
        //     引发 System.Windows.Forms.Control.KeyUp 事件。
        //
        // 参数:
        //   e:
        //     包含事件数据的 System.Windows.Forms.KeyEventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Control.KeyUp 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 System.Windows.Forms.KeyEventArgs。</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (this.DesignMode)
            {
                base.OnKeyUp(e);
                return;
            }

            int i = 0;
            i++;
            if (e.Shift == true)
            {
                //MessageBox.Show(this,"无shift");
            }
            else
            {
                // this.nStartFieldIndex = -1;
                //MessageBox.Show(this,"有shift");
            }
            base.OnKeyUp(e);
        }

        #endregion

        #region 公共的属性 函数

        // 当前获得焦点的字段
        /// <summary>
        /// 当前具有输入焦点的字段对象
        /// </summary>
        public Field FocusedField
        {
            get
            {
                if (this.DesignMode)
                    return null;

                if (this.SelectedFieldIndices.Count > 1)
                    return null;    //

                if (this.FocusedFieldIndex == -1)
                    return null;
                Debug.Assert(this.FocusedFieldIndex >= 0, "不可能的情况1");

                // 2008/11/29
                if (this.record.Fields.Count == 0)
                    return null;

                Debug.Assert(this.record.Fields.Count > 0, "不可能的情况2");
                Debug.Assert(this.FocusedFieldIndex < this.record.Fields.Count, "不可能的情况3");
                return (Field)this.record.Fields[this.FocusedFieldIndex];
            }
            set
            {
                if (this.DesignMode)
                    return;

                if (value == null)
                    return;
                int nIndex = this.record.Fields.IndexOf(value);
                if (nIndex == -1)
                    return;
                Debug.Assert(nIndex != -1, "不可能的情况");
                this.SetActiveField(nIndex, 3);
            }
        }

        // TODO: 注意测试当 字段被删除 .Marc 被清除等情况下，FocuedFieldIndex 不应下标越界
        // 当前获得焦点字段的索引号
        /// <summary>
        /// 当前具有输入焦点字段(在全部字段集合中的)下标
        /// </summary>
        public int FocusedFieldIndex
        {
            get
            {
                if (this.DesignMode)
                    return 0;

                if (this.SelectedFieldIndices.Count == 0)
                    return -1;

#if DEBUG
                if (this.SelectedFieldIndices.Count > 1)
                {
                    Debug.Assert(false, "此时选中节点多于一个，不应调FocusedFieldIndex。");
                }
#endif

                return (int)this.SelectedFieldIndices[0];
            }

            // 2007/7/17 
            set
            {
                if (this.DesignMode)
                    return;

                // 先Flush
                this.Flush();

                // 毁坏了FocusField
                // this.SelectedFieldIndices = new List<int>();    //  new ArrayList();
                this.ClearSelectFieldIndices("");   // 除了 .Clear() 以外，还会把相关 Field 对象的 Selected 设置为 false

                if (value != -1)
                {
                    this.SelectedFieldIndices.Add(value);

                    if (this.curEdit != null)
                        this.curEdit.ContentIsNull = true;    // 防止后面SetActiveField()调用时送回内存 2009/3/6

                    try
                    {
                        // 2008/3/19
                        if (value >= 0 && value < this.record.Fields.Count)
                        {
                            Field field = this.record.Fields[value];
                            if (Record.IsFirstHeaderFieldName(field.Name) == true
                                && (this.m_nFocusCol == 1 || this.m_nFocusCol == 2)) // 没有字段名和指示符
                            {
                                this.SetActiveField(value, 3, false);
                            }
                            else if (Record.IsControlFieldName(field.Name) == true
                                && this.m_nFocusCol == 2)   // 没有指示符
                            {
                                this.SetActiveField(value, 3, false);
                            }
                            else
                            {
                                this.SetActiveField(value, this.m_nFocusCol, false);
                            }
                        }
                    }
                    finally
                    {
                        if (this.curEdit != null)
                            this.curEdit.ContentIsNull = true;
                    }
                }
            }
        }

        /*
        public Subfield FocuedSubfield
        {
        }
         */

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

                if (this.FocusedField == null)
                    return (char)0;

                if (this.SelectionStart < 0)
                    return (char)0;

                // 1: 字段名 2: 指示符 3:字段内容
                if (this.m_nFocusCol != 3)
                    return (char)this.m_nFocusCol; // 2011/8/17

                return MarcUtil.SubfieldNameByOffs(
                    this.FocusedField.Value,
                    this.SelectionStart);
            }
        }

        public string GetCurrentLocationString()
        {
            if (this.curEdit == null)
                throw new Exception("this.curEdit 为 null，无法执行 GetCurrentLocationString()");
            if (this.curEdit.Visible == false)
                return "";

            // 当前插入符所在位置
            int current_offs = this.curEdit.SelectionStart;
            // 探测点取中间位置
            if (this.curEdit.SelectionLength > 0)
                current_offs += this.curEdit.SelectionLength / 2;

            return this.Record.GetSubfieldLocationString(
                this.FocusedFieldIndex,
                current_offs);
        }

        internal void Test()
        {
            char subfieldname = MarcUtil.SubfieldNameByOffs(
                this.FocusedField.Value,
                this.SelectionStart);

            string strText = "SelectionStart=" + Convert.ToString(this.SelectionStart + ", subfieldname=" + Convert.ToString(subfieldname));
            MessageBox.Show(this, strText);
        }

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
#if BIDI_SUPPORT
                if (this.curEdit != null)
                    return this.curEdit.SelectionStart - CountFillingChar(this.curEdit.Text, this.curEdit.SelectionStart);
#else
				if (this.curEdit != null)
					return this.curEdit.SelectionStart;
#endif
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
#if BIDI_SUPPORT
                        this.curEdit.SelectionStart = AdjustPos(this.curEdit.Text, value);
#else
                        this.curEdit.SelectionStart = value;
#endif



                    }
                }
            }
        }

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
                    return this.record?.GetMarc();
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    if (this.DesignMode && this.record == null)
                        return;

                    var line = this.FocusedFieldIndex;
                    var col = this._focusCol;
                    var caret_pos = 0;
                    if (this.curEdit != null)
                    {
                        caret_pos = this.curEdit.SelectionStart;
                        this.curEdit.Hide();
                        this.curEdit.Dispose();
                        this.curEdit = null;
                    }
                    // this.HideTextBox();

                    if (this.record != null)
                    {
                        var old_fields = Field.Clone(this.record.Fields);
                        int nRet = this.record.SetMarc(value,
                            true,
                            out string strError);
                        if (nRet == -1)
                        {
                            if (this.DesignMode == true)
                            {
                                // 绘制报错文字图象
                            }
                            else
                                throw (new Exception("SetMarc()出错，原因：" + strError));
                        }
                        AppendReset(old_fields, this.Record.Fields);

                        // 2024/7/10
                        if (line >= this.Record.Fields.Count)
                            line = this.Record.Fields.Count - 1;
                        if (col < 1)
                            col = 1;
                        if (line >= 0)
                            this.SetActiveField(line, col, caret_pos, false);
                    }

                    // 2014/7/10
                    AdjustOriginY();
                });
            }
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("Changed")]
        [DefaultValue(false)]
        public bool Changed
        {
            get
            {
                if (this.DesignMode)
                    return false;

                if (this.SelectedFieldIndices?.Count == 1)
                    this.EditControlTextToItem();   // 容易引起递归。已经解决
                return this.m_bChanged;
            }
            set
            {
                if (this.DesignMode)
                    return;

                this.m_bChanged = value;

                // 2021/14/28
                if (value == true)
                    this.FireTextChanged();
            }
        }

        /// <summary>
        /// 将小编辑器中(暂时)的内容兑现到内存结构
        /// </summary>
        public void Flush()
        {
            if (this.SelectedFieldIndices?.Count == 1)
            {
                this.EditControlTextToItem();
            }
        }

        #endregion

        #region 内部属性 函数

        // 隐藏小edit
        internal void HideTextBox()
        {
            if (this.SelectedFieldIndices.Count == 1)
            {
                // 把旧内容送回内存对象
                EditControlTextToItem();
            }

            if (this.curEdit != null)
                this.curEdit.Hide();
        }

        /// <summary>
        /// 清除表示当前选定字段的若干下标
        /// </summary>
        internal void ClearSelectFieldIndices(string style = "flush,hidetextbox,invalidate")
        {
            // 把最新的内容保存到内存对象中
            if (StringUtil.IsInList("flush", style))
                this.Flush();

            var invalidate = StringUtil.IsInList("invalidate", style);

            // 失效原来全部的字段
            foreach (int nIndex in this.SelectedFieldIndices)
            {
                if (nIndex == -1 || nIndex >= this.record.Fields.Count)
                {
                    // Debug.Assert(false,"SelectedFieldIndices数组中不应有'" + Convert.ToString(nIndex) + "'。");
                    continue;
                }
                Field field = this.record.Fields[nIndex];
                field.Selected = false;
                if (invalidate)
                {
                    // 失效该行
                    Rectangle rect1 = this.GetItemBounds(nIndex,
                    1,
                    BoundsPortion.Field);
                    this.Invalidate(rect1);
                }
            }
            this.SelectedFieldIndices.Clear();
            if (StringUtil.IsInList("hidetextbox", style))
                this.HideTextBox();
        }

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
            this.SetActiveField(field_index, col, set_focus);
            if (this.curEdit != null
                && this.curEdit.Visible)
            {
                if (caret_pos == -1)
                    caret_pos = 0;
                this.curEdit.SelectionStart = caret_pos;
                this.curEdit.SelectionLength = 0;
            }
            return true;
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
        //      bFocus  是否将焦点放到上面
        internal void SetActiveField(int nFieldIndex,
            int nCol,
            int caret_pos,
            bool bFocus)
        {
            if (nFieldIndex < 0 || nFieldIndex >= this.record.Fields.Count)
                throw new ArgumentException("ActiveField()调用错误，不能设当前活动字段为'" + Convert.ToString(nFieldIndex) + "'。下标越界。");

            if (this.SelectedFieldIndices.Count == 1)
            {
                // 把旧内容送回内存对象
                EditControlTextToItem();
            }

            // 清除原来选中的字段
            this.ClearSelectFieldIndices();

            // 设当前活动的行，列
            this.SelectedFieldIndices.Add(nFieldIndex);

            // 破坏掉，免得它里面的内容到达新的焦点字段内 2011/6/20
            if (this.SelectedFieldIndices.Count == 1)
            {
                if (this.curEdit != null)
                {
                    this.curEdit.ContentIsNull = true;
                }
            }

            Field fieldTemp = this.record.Fields[nFieldIndex];
            if (fieldTemp.Name == "###")
            {
                if (nCol != 3)
                    nCol = 3;
            }
            else if (Record.IsControlFieldName(fieldTemp.Name) == true
                && nCol == 2)
            {
                nCol = 3;
            }

            if (nCol == 0)
            {
                // nCol = 3;
                nCol = 1;   // xietao 2006/5/26 changed
            }

            this.m_nFocusCol = nCol;

            // 2008/3/21
            if (this.curEdit == null)
                InitialEditControl();

            if (nFieldIndex == 0)
                this.curEdit.MaxLength = 24;
            else
                this.curEdit.MaxLength = 32767;

            // 设小edit控件的大小和位置
            this.SetEditPos(bFocus);

            // 把新内容赋到小edit控件里
            this.ItemTextToEditControl(caret_pos);

            this.FocusedField.Selected = true;
            if (nFieldIndex != -1 && nFieldIndex < this.record.Fields.Count)
            {
                Rectangle rect2 = this.GetItemBounds(nFieldIndex,
                    1,
                    BoundsPortion.Field);
                this.Invalidate(rect2);
            }

            if (this.SelectedFieldChanged != null)
            {
                this.SelectedFieldChanged(this, new EventArgs());
                m_chOldSubfieldName = this.FocusedSubfieldName;
            }
        }

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

            if (this.SelectedFieldChanged != null)
            {
                this.SelectedFieldChanged(this, new EventArgs());
                m_chOldSubfieldName = this.FocusedSubfieldName;
            }
        }

        public void FireSelectedFieldChanged()
        {
            if (this.SelectedFieldChanged != null)
            {
                this.SelectedFieldChanged(this, new EventArgs());
            }
        }

        // 文档横向编移量
        /// <summary>
        /// 当前文档横向编移量
        /// </summary>
        public int DocumentOrgX
        {
            get
            {
                if (this.DesignMode)
                    return 0;
                return this.m_nDocumentOrgX;
            }
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
        }

        // 文档纵向偏移量
        /// <summary>
        /// 文档纵向偏移量
        /// </summary>
        public int DocumentOrgY
        {
            get
            {
                if (this.DesignMode)
                    return 0;
                return this.m_nDocumentOrgY;
            }
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
        }

        // 文档宽度
        internal int DocumentWidth
        {
            get
            {
                return this.LeftBlank
                    + (this.record != null ? this.record.Width : 0)
                    + this.RightBlank - 1/*微调*/;
            }
        }

        // 文档高度
        internal int DocumentHeight
        {
            get
            {
                return this.TopBlank
                    + (this.record != null ? this.record.Height : 0)
                    + this.BottomBlank;
            }
        }

        // 客户区宽度
        internal int ClientWidth
        {
            get
            {
                return this.ClientSize.Width;
            }
            set
            {
                Size newsize = new Size(value, ClientSize.Height);
                this.ClientSize = newsize;

            }
        }

        // 客户区高度
        internal int ClientHeight
        {
            get
            {
                return this.ClientSize.Height;
            }
            set
            {
                Size newsize = new Size(ClientSize.Width, value);
                this.ClientSize = newsize;
            }
        }

        // 文档发生改变
        internal void FireTextChanged(bool changed = true)
        {
            // this.Changed = changed;
            if (this.DesignMode == false)
                this.m_bChanged = changed;

            EventArgs e = new EventArgs();
            this.OnTextChanged(e);
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
#if NO
			
			XmlNode nodeLabel = null;

            try
            {


                if (this.Lang == "")
                    nodeLabel = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Property/Label");
                else
                {
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                    nsmgr.AddNamespace("xml", Ns.xml);
                    nodeLabel = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Property/Label[@xml:lang='" + this.Lang + "']", nsmgr);
                }
            }
            catch // 防止字段名中不合法字符用于xpath抛出异常
            {
                nodeLabel = null;
            }

			if (nodeLabel == null)
				return "???";

			return DomUtil.GetNodeText(nodeLabel);
#endif
        }

        // 得到从起始行开始的行数的rectangle
        // parameter:
        //		nStartLine	起始行
        //		nLength	行数 -1表示从起始行到末尾
        //		BoundsPortion	区域 如为Field表示取整行，如为FieldAndBottom表示取行与控件底部
        // return:
        //		Rectangle对象
        // 注意: 本函数包括的Rectangle对象永远不包含控件左上右的空白
        //		 返回的Recatangle是实际的坐标
        internal Rectangle GetItemBounds(int nStart,
            int nLength,
            BoundsPortion portion)
        {
            Rectangle rect = new Rectangle(0, 0, 0, 0);

            string strError = "";
            int nRealLength = 0;
            int nRet = Record.GetRealLength(nStart,
                nLength,
                this.record.Fields.Count,
                -1,
                out nRealLength,
                out strError);
            if (nRet == -1)
            {
                Debug.Assert(false, strError);
                return rect;
            }

            if (portion != BoundsPortion.Field
                && portion != BoundsPortion.FieldAndBottom)
            {
                Debug.Assert(false, "portion参数不合法，必须为Line或LineAndBottom");
                return rect;
            }

            rect.X = this.DocumentOrgX
                + this.LeftBlank;

            rect.Y = this.DocumentOrgY
                + this.TopBlank
                + this.record.GetFieldsHeight(0, nStart);

            rect.Width = this.record.Width + 1 /*线条为1时的微调*/;
            rect.Height = this.record.GetFieldsHeight(nStart, nRealLength);

            if (portion == BoundsPortion.FieldAndBottom)
            {
                if (nStart + nRealLength != this.record.Fields.Count)
                {
                    Debug.Assert(false, "调入错误，不取到末尾不可以用LineAndBottom参数");
                    return rect;
                }
                rect.Height += this.record.GridHorzLineHeight
                    + this.BottomBlank;
            }

            return rect;
        }

        // 事项高度因素变化后，善后、兑现显示的函数
        // nOldHeightParam	事项原来的像素现实高度。如果==-1，表示不用此参数，
        //		本函数自动把函数调用时刻的事项Height属性表示的高度当作旧高度，
        //		而把CalcHeight()计算得到的高度当作新高度。
        internal void AfterItemHeightChanged(int nField,
            int nOldHeightParam)
        {
            Field field = this.record.Fields[nField];

            // 得到旧高度
            int nOldHeight = 0;
            if (nOldHeightParam == -1)
            {
                nOldHeight = field.TotalHeight;
                field.CalculateHeight(null, false);
            }
            else
            {
                nOldHeight = nOldHeightParam;
            }

            // 新高度
            int nNewHeight = field.TotalHeight;

            // 新旧两高度的差距
            int nDelta = nNewHeight - nOldHeight;
            if (nDelta == 0)
                return;

            Rectangle rect = new Rectangle(0, 0, 0, 0);
            if (this.FocusedFieldIndex < this.record.Fields.Count - 1)
            {
                // 得到本行以下的区域

                // 得到从起始行开始的行数的rectangle
                // parameter:
                //		nStartLine	起始行
                //		nCount	行数
                //		BoundsPortion	区域 如为line表示取整行，如为LineAndBottom表示取行与控件底部
                // return:
                //		Rectangle对象
                // 注意: 本函数包括的Rectangle对象永远不包含控件左上右的空白
                //		 返回的Recatangle是实际的坐标
                rect = GetItemBounds(this.FocusedFieldIndex + 1,
                    this.record.Fields.Count - (this.FocusedFieldIndex + 1),
                    BoundsPortion.FieldAndBottom);

                // nDelta修正为edit控件尺寸增大以前的下方bounds
                rect.Y = rect.Y - nDelta;

                this.Update();

                // 不能直接用rect，因为ScrollWindowEx对rect为ref类型参数
                RECT rect1 = new RECT();
                rect1.left = rect.Left;
                rect1.top = rect.Top;
                rect1.right = rect.Right;
                rect1.bottom = rect.Bottom;
                API.ScrollWindowEx(this.Handle,
                    0,
                    nDelta,
                    ref rect1,
                    IntPtr.Zero,	//	ref RECT lprcClip,
                    0,	// int hrgnUpdate,
                    IntPtr.Zero,	// ref RECT lprcUpdate,
                    API.SW_INVALIDATE);

                if (rect.Width == 0)
                {
                    Debug.Assert(false, "width不可能为0");
                }
            }

            InvalidateRect iRect = null;
            if (this.FocusedFieldIndex == this.record.Fields.Count - 1)
            {
                iRect = new InvalidateRect();
                iRect.bAll = false;
                iRect.rect = this.GetItemBounds(this.FocusedFieldIndex,
                    1,
                    BoundsPortion.FieldAndBottom);
            }
            else
            {


                iRect = new InvalidateRect();
                iRect.bAll = false;
                iRect.rect = this.GetItemBounds(this.FocusedFieldIndex,
                    this.record.Fields.Count - this.FocusedFieldIndex,
                    BoundsPortion.FieldAndBottom);

                /*

				iRect = new InvalidateRect();
				iRect.bAll = false;

				// 只失效下方的部分
				// 优化版本，利用rect
				iRect.rect = new Rectangle(
					rect.X,
					rect.Y - nNewHeight,
					rect.Width,
					nNewHeight);
 */
            }

            // 相应的文档变化了
            AfterDocumentChanged(ScrollBarMember.Vert,
                iRect);

        }

        // 测算单击点的位置
        // parameters:
        //		p	输入位置点
        //		nField	out参数，返回所在的字段
        //		nCol	out参数，返回所在的列号
        //				0 字段说明;
        //				1 字段名;
        //				2 字段指示符
        //				3 字段内容
        // return: 
        //		-1	不在record上
        //		0	在record内 
        //		1	在缝隙上
        // 注: 文档的上下左右空白不算record
        internal int HitTest(Point p,
            out int nField,
            out int nCol)
        {
            nField = -1;
            nCol = -1;

            // 调整大小的区域宽度
            int nResizeAreaWidth = 4;


            int x = this.DocumentOrgX + this.LeftBlank;
            int y = this.DocumentOrgY + this.TopBlank;

            // 不在record区域
            if (p.Y < y || p.X < x)
                return -1;
            if (p.X > x + this.record.Width)
                return -1;
            if (p.Y > y + this.record.Height)
                return -1;

            // 已确定在record区域，计算精确位置
            for (int i = 0; i < this.record.Fields.Count; i++)
            {
                Field field = this.record.Fields[i];

                if (p.Y >= y
                    && p.Y <= y + field.TotalHeight)
                {
                    nField = i;

                    // 在名称标签与名称间的缝隙上
                    if (p.X >= x + this.record.NameCaptionTotalWidth - (nResizeAreaWidth / 2)
                        && p.X < x + this.record.NameCaptionTotalWidth + (nResizeAreaWidth / 2))
                    {
                        nCol = 0;
                        return 1;
                    }

                    // 名字说明
                    if (p.X >= x + 0
                        && p.X <= x + this.record.NameCaptionTotalWidth)
                    {
                        nCol = 0;
                        return 0;
                    }



                    // 名字
                    if (p.X >= x + this.record.NameCaptionTotalWidth
                        && p.X <= x + this.record.NameCaptionTotalWidth + this.record.NameTotalWidth)
                    {
                        nCol = 1;
                        return 0;
                    }

                    // Indicator上
                    if (p.X > x + this.record.NameCaptionTotalWidth + this.record.NameTotalWidth
                        && p.X <= x + this.record.NameCaptionTotalWidth + this.record.NameTotalWidth + this.record.IndicatorTotalWidth)
                    {
                        nCol = 2;
                        return 0;
                    }

                    // 最后一根线条可以考虑算在value上
                    // Value上
                    if (p.X > x + this.record.NameCaptionTotalWidth + this.record.NameTotalWidth + this.record.IndicatorTotalWidth /*+ this.record.Indicator2TotalWidth*/
                        && p.X <= x + this.record.NameCaptionTotalWidth + this.record.NameTotalWidth + this.record.IndicatorTotalWidth/* + this.record.Indicator2TotalWidth*/ + this.record.ValueTotalWidth)
                    {
                        nCol = 3;
                        return 0;
                    }
                }
                y += field.TotalHeight;
            }
            return -1;
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
            if (this.curEdit != null)
                this.curEdit.EnsureVisible();
        }

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
            Debug.Assert(nFieldIndex >= 0 && nFieldIndex < this.record.Fields.Count, "调入错误");
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

        // 修改字段名后要变换说明信息
        /// <summary>
        /// 修改字段名
        /// </summary>
        /// <param name="field">字段对象</param>
        /// <param name="strNewName">要修改成的字段名</param>
        public void ChangeFieldName(Field field,
            string strNewName)
        {
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
        }

        #endregion

        #region 处理右键菜单的函数

        internal static void TextToClipboard(string strText)
        {
#if BIDI_SUPPORT
            // strText = strText.Replace("\x200e", "");
            strText = RemoveBidi(strText);
#endif
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

        internal static void TextToClipboardFormat(string strText)
        {
#if BIDI_SUPPORT
            // strText = strText.Replace("\x200e", "");
            strText = RemoveBidi(strText);
#endif

            // Make a DataObject.
            DataObject data_object = new DataObject();

            // Add the data in various formats.
            // 普通格式
            data_object.SetData(DataFormats.UnicodeText, strText
                .Replace((char)Record.SUBFLD, '$')
                .Replace((char)Record.FLDEND, '#')
                .Replace((char)Record.RECEND, '*'));
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

        internal static string ClipboardToTextFormat(bool replace = true)
        {
            string result = "";

            StringUtil.RunClipboard(() =>
            {
                IDataObject ido = Clipboard.GetDataObject();

                if (ido.GetDataPresent(typeof(MarcEditorData)) == true)
                {
                    MarcEditorData data = (MarcEditorData)ido.GetData(typeof(MarcEditorData));
                    result = data.Text;
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
                return result.Replace("ǂ", MarcQuery.SUBFLD).Replace("\r\n", "\n").Replace("\n", MarcQuery.FLDEND);
            return result;
        }

        private void Menu_SelectAll(object sender,
            System.EventArgs e)
        {
            Debug.Assert(this.record.Fields.Count > 0, "");

            this.AddSelectedField(0,
                this.record.Fields.Count - 1,
                true);
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

            PropertyDlg dlg = new PropertyDlg();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.MarcEditor = this;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
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
            if (this.SelectedFieldIndices.Count == 0)
            {
                Debug.Assert(false, "在'剪切'时，SelectedFieldIndices.Count不可能为0。");
                return;
            }
            string strText = "";

            for (int i = 0; i < this.SelectedFieldIndices.Count; i++)
            {
                int nIndex = (int)this.SelectedFieldIndices[i];
                Field field = this.record.Fields[nIndex];
                strText += field.GetFieldMarc(true);
            }

            MarcEditor.TextToClipboardFormat(strText);

            int[] fieldIndices = new int[this.SelectedFieldIndices.Count];
            for (int i = 0; i < fieldIndices.Length; i++)
            {
                fieldIndices[i] = (int)this.SelectedFieldIndices[i];
            }
            // this.record.Fields.RemoveAt(fieldIndices);
            this.RemoveFields(fieldIndices);

            // 2007/7/17
            AdjustOriginY();
        }

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
            // 复制当前字段到后面
            if (key == Keys.F)
            {
                menuItem_Copy(this, null);
                menuItem_PasteInsert_InsertAfter(this, null);
            }

            // 复制当前子字段到后面
            if (key == Keys.S)
            {
                this.curEdit.DupCurrentSubfield();
            }


            // 插入 {cr:CALIS}
            if (key == Keys.C)
            {
                this.curEdit.PasteToCurrent("{cr:CALIS}");
            }

            // 插入 {cr:NLC}
            if (key == Keys.N)
            {
                this.curEdit.PasteToCurrent("{cr:NLC}");
            }
        }

        // 复制
        private void menuItem_Copy(object sender,
            System.EventArgs e)
        {
            if (this.SelectedFieldIndices.Count == 0)
            {
                Debug.Assert(false, "在'复制'时，SelectedFieldIndices.Count不可能为0。");
                return;
            }
            string strText = "";
            for (int i = 0; i < this.SelectedFieldIndices.Count; i++)
            {
                int nIndex = (int)this.SelectedFieldIndices[i];
                Field field = this.record.Fields[nIndex];
                strText += field.GetFieldMarc(true);
            }
            MarcEditor.TextToClipboardFormat(strText);
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
                bool bHasFocus = this.Focused;

                // 先删除所有字段
                ClearFields();

                int nFirstIndex = 0;

                int nNewFieldsCount = 0;
                string strFieldsMarc = MarcEditor.ClipboardToText();
                strFieldsMarc = ConvertMarcXmlString(strFieldsMarc);

                // TODO: 这个函数可以改造为两步实现：
                // 1) 一个函数切分MARC多字段字符串为一个一个字段单独字符串
                // 2) 根据上一步切分出来的字符串数组，进行插入或者替换等操作
                /*
                this.record.Fields.InsertInternal(nFirstIndex,
                    strFieldsMarc,
                    out nNewFieldsCount);
                */
                InsertFields(nFirstIndex,
                    strFieldsMarc,
                    out nNewFieldsCount);

                this.SetScrollBars(ScrollBarMember.Both);
                this.Invalidate();

                // 设第一个节点为当前活动焦点
                if (bHasFocus == true)
                {
                    if (this.record.Fields.Count > 0)
                        this.SetActiveField(0, 3, true);
                }
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
            bool bHasFocus = this.Focused;

            // 先删除所有字段
            ClearFields();

            int nFirstIndex = 0;

            int nNewFieldsCount = 0;
            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertWorksheetMarcString(strFieldsMarc);

            // TODO: 这个函数可以改造为两步实现：
            // 1) 一个函数切分MARC多字段字符串为一个一个字段单独字符串
            // 2) 根据上一步切分出来的字符串数组，进行插入或者替换等操作
            /*
            this.record.Fields.InsertInternal(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            InsertFields(nFirstIndex,
    strFieldsMarc,
    out nNewFieldsCount);

            this.SetScrollBars(ScrollBarMember.Both);
            this.Invalidate();

            // 设第一个节点为当前活动焦点
            if (bHasFocus == true)
            {
                if (this.record.Fields.Count > 0)
                    this.SetActiveField(0, 3, true);
            }
        }

        // 从 NLC 粘贴整个记录
        void menuItem_PasteFromNlcMarc(object sender,
            System.EventArgs e)
        {
            bool bHasFocus = this.Focused;

            // 先删除所有字段
            ClearFields();

            int nFirstIndex = 0;

            int nNewFieldsCount = 0;
            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertNlcMarcString(strFieldsMarc);

            // TODO: 这个函数可以改造为两步实现：
            // 1) 一个函数切分MARC多字段字符串为一个一个字段单独字符串
            // 2) 根据上一步切分出来的字符串数组，进行插入或者替换等操作
            /*
            this.record.Fields.InsertInternal(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            InsertFields(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            this.SetScrollBars(ScrollBarMember.Both);
            this.Invalidate();

            // 设第一个节点为当前活动焦点
            if (bHasFocus == true)
            {
                if (this.record.Fields.Count > 0)
                    this.SetActiveField(0, 3, true);
            }
        }


        // 从 tcmarc 粘贴整个记录
        void menuItem_PasteFromTcMarc(object sender,
            System.EventArgs e)
        {
            bool bHasFocus = this.Focused;

            // 先删除所有字段
            ClearFields();

            int nFirstIndex = 0;

            int nNewFieldsCount = 0;
            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertTcmarcMarcString(strFieldsMarc);

            // TODO: 这个函数可以改造为两步实现：
            // 1) 一个函数切分MARC多字段字符串为一个一个字段单独字符串
            // 2) 根据上一步切分出来的字符串数组，进行插入或者替换等操作
            /*
            this.record.Fields.InsertInternal(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            this.InsertFields(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            this.SetScrollBars(ScrollBarMember.Both);
            this.Invalidate();

            // 设第一个节点为当前活动焦点
            if (bHasFocus == true)
            {
                if (this.record.Fields.Count > 0)
                    this.SetActiveField(0, 3, true);
            }
        }

        // 从dp2OPAC Web粘贴整个记录
        void menuItem_PasteFromDp2OPAC(object sender,
            System.EventArgs e)
        {
            bool bHasFocus = this.Focused;

            // 先删除所有字段
            ClearFields();

            int nFirstIndex = 0;

            int nNewFieldsCount = 0;
            string strFieldsMarc = MarcEditor.ClipboardToText();
            strFieldsMarc = ConvertWebMarcString(strFieldsMarc);

            // TODO: 这个函数可以改造为两步实现：
            // 1) 一个函数切分MARC多字段字符串为一个一个字段单独字符串
            // 2) 根据上一步切分出来的字符串数组，进行插入或者替换等操作
            /*
            this.record.Fields.InsertInternal(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            InsertFields(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            this.SetScrollBars(ScrollBarMember.Both);
            this.Invalidate();

            /*
            this.InitialFonts();

            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = true;
            this.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);
             * */

            // 设第一个节点为当前活动焦点
            if (bHasFocus == true)
            {
                if (this.record.Fields.Count > 0)
                    this.SetActiveField(0, 3, true);
            }
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

            // 2022/1/6
            strResult = RemoveBidi(strResult);
            return strResult;
        }

        // 不支持在多选时粘贴覆盖，因为不明确在哪个字段上粘贴覆盖
        // 粘贴覆盖
        private void menuItem_PasteOverwrite(object sender,
            System.EventArgs e)
        {
            /*
            if (this.SelectedFieldIndices.Count == 0)
            {
                MessageBox.Show(this, "没有找到插入位置");
                return;
            }

            Debug.Assert(this.SelectedFieldIndices.Count >= 1, "在粘贴覆盖时，SelectedFieldIndices必须>=1。");
            */

            // 先删除选定的所有字段
            /*
            int[] fieldIndices = new int[this.SelectedFieldIndices.Count];
            for (int i = 0; i < fieldIndices.Length; i++)
            {
                fieldIndices[i] = (int)this.SelectedFieldIndices[i];
            }
            this.record.Fields.RemoveAt(fieldIndices);
            */
            // 记忆被删除的字段
            var indices = this.SelectedFieldIndices.ToArray();
            // this.record.Fields.RemoveAt(indices);
            this.RemoveFields(indices);

            int nFirstIndex = 0;// fieldIndices[0];
            if (indices.Length > 0)
                nFirstIndex = indices[0];

            // 2007/7/17
            // 在被删除的第一个index处进行插入
            // TODO: 有条件应该改变为，顺次在被覆盖的位置进行替换。
            // 如果到最后被覆盖的字段用完，则接着进行插入。如果被覆盖的字段没用
            // 用完，就把剩下的删除。这是为了针对离散选择的情况
            int nNewFieldsCount = 0;
            string strFieldsMarc = MarcEditor.ClipboardToTextFormat();
            // TODO: 这个函数可以改造为两步实现：
            // 1) 一个函数切分MARC多字段字符串为一个一个字段单独字符串
            // 2) 根据上一步切分出来的字符串数组，进行插入或者替换等操作
            /*
            this.record.Fields.InsertInternal(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            InsertFields(nFirstIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            /*
            this.AfterDocumentChanged(ScrollBarMember.Both,
                null);
             * */
            this.SetScrollBars(ScrollBarMember.Both);
            this.Invalidate();


            /*
			int nIndex = this.FocusedFieldIndex;
			Debug.Assert(nIndex >=0 && nIndex < this.record.Fields.Count,"在粘贴覆盖时，FocusFieldIndex越界。");

			string strFieldsMarc = MarcEditor.ClipboardToText();
			
			// 先把新字段插入当前字段的后面
			int nNewFieldsCount = 0;
			this.record.Fields.InsertInternal(nIndex+1,
				strFieldsMarc,
				out nNewFieldsCount);


			// 再删除当前字段
			this.record.Fields.RemoveAt(nIndex);

			Debug.Assert(nIndex + nNewFieldsCount <= this.record.Fields.Count,"不可能的情况");

			// 把新字段中的最后一个字段设为当前字段
			this.SetActiveField(nIndex + nNewFieldsCount-1,3);
						
			// 失效范围
			InvalidateRect iRect = new InvalidateRect();
			iRect.bAll = false;
			iRect.rect = this.GetItemBounds(nIndex,
				-1,
				BoundsPortion.FieldAndBottom);
			this.AfterDocumentChanged(ScrollBarMember.Both,
				iRect);
             * */
        }

        // 粘贴插入_末尾
        private void menuItem_PasteInsert_AppendChild(object sender,
            System.EventArgs e)
        {
            string strFieldsMarc = MarcEditor.ClipboardToTextFormat();
            if (this.record.Fields.Count == 0)
            {
                string strError = "";
                var old_fields = Field.Clone(this.record.Fields);
                int nRet = this.record.SetMarc(strFieldsMarc,
                    false,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                AppendReset(old_fields, this.Record.Fields);
                return;
            }

            int nOldFocusFieldIndex = 0;
            if (this.SelectedFieldIndices.Count == 1)
                nOldFocusFieldIndex = this.FocusedFieldIndex;

            int nIndex = 0;
            if (this.record.Fields.Count > 0)
                nIndex = this.record.Fields.Count;

            int nNewFieldsCount = 0;
            /*
            this.record.Fields.InsertInternal(nIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            // 自带写入操作历史能力
            this.InsertFields(nIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            if (this.record.Fields.Count > 0)
                this.SetActiveField(this.record.Fields.Count - 1, 3);

            // 失效范围
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.GetItemBounds(nOldFocusFieldIndex,
                -1,
                BoundsPortion.FieldAndBottom);
            this.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);

            this.EnsureVisible();   // 2009/3/6
        }

        // 粘贴插入_前插
        private void menuItem_PasteInsert_InsertBefore(object sender,
            System.EventArgs e)
        {
            Debug.Assert(this.SelectedFieldIndices.Count == 1, "在'粘贴插入/前插'时，SelectedFieldIndices必须为1。");

            int nIndex = this.FocusedFieldIndex;
            Debug.Assert(nIndex >= 0 && nIndex < this.record.Fields.Count, "在'粘贴插入/前插'时，FocusFieldIndex越界。");

            string strFieldsMarc = MarcEditor.ClipboardToTextFormat();

            // 这里要特别注意，把原来的焦点清空，以便在给新字段赋值时，不影响老字段
            this.ClearSelectFieldIndices();

            int nNewFieldsCount = 0;
            /*
            this.record.Fields.InsertInternal(nIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            // 自带写入操作历史能力
            this.InsertFields(nIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            // 这里要特别注意，把焦点设上新插入字段的第一个上面
            this.SetActiveField(nIndex, this.m_nFocusCol);

            // 失效范围
            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.GetItemBounds(nIndex,
                -1,
                BoundsPortion.FieldAndBottom);
            this.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);
        }

        // 粘贴插入_后插
        private void menuItem_PasteInsert_InsertAfter(object sender,
            System.EventArgs e)
        {
            Debug.Assert(this.SelectedFieldIndices.Count == 1, "在'粘贴插入/后插'时，SelectedFieldIndices必须为1。");

            int nIndex = this.FocusedFieldIndex;
            Debug.Assert(nIndex >= 0 && nIndex < this.record.Fields.Count, "在'粘贴插入/后插'时，FocusFieldIndex越界。");

            string strFieldsMarc = MarcEditor.ClipboardToTextFormat();

            int nStartIndex = nIndex + 1;

            int nNewFieldsCount = 0;
            /*
            this.record.Fields.InsertInternal(nStartIndex,
                strFieldsMarc,
                out nNewFieldsCount);
            */
            // 自带写入操作历史能力
            this.InsertFields(nStartIndex,
                strFieldsMarc,
                out nNewFieldsCount);

            // 把焦点设为最后一项上
            Debug.Assert(nStartIndex + nNewFieldsCount <= this.record.Fields.Count, "不可能的情况");

            // 把新字段中的最后一个字段设为当前字段
            this.SetActiveField(nStartIndex + nNewFieldsCount - 1, 3);

            InvalidateRect iRect = new InvalidateRect();
            iRect.bAll = false;
            iRect.rect = this.GetItemBounds(nStartIndex,
                -1,
                BoundsPortion.FieldAndBottom);
            this.AfterDocumentChanged(ScrollBarMember.Both,
                iRect);
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

        public void InsertFields(int start_index,
            string fields_marc,
            out int new_fields_count)
        {
            this.record.Fields.InsertInternal(start_index,
    fields_marc,
    out new_fields_count);
            var new_fields = GetFields(start_index, new_fields_count);
            this.AppendInsertFields(start_index,
                new_fields);
        }

        public void ReplaceFields(int start_index,
    string fields_marc,
    out int new_fields_count)
        {
            // 如果这里的 index 不连续，则只能整体保存
            var indices = this.SelectedFieldIndices.ToArray();
            this.record.Fields.RemoveAt(indices);
            this.RemoveFields(indices);

            this.record.Fields.InsertInternal(start_index,
    fields_marc,
    out new_fields_count);
            var new_fields = GetFields(start_index, new_fields_count);
            this.AppendInsertFields(start_index,
                new_fields);
        }

        // 显示Marc记录
        private void ShowMarc(object sender,
            System.EventArgs e)
        {
            MessageBox.Show("'" + this.Marc + "'");
        }

        // 前插字段
        private void InsertBeforeFieldNoDlg(object sender,
            System.EventArgs e)
        {
            Debug.Assert(this.SelectedFieldIndices.Count == 1, "在'前插'时，SelectedFieldIndices数量必须为1");

            if (this.FocusedField.m_strName == "###")
            {
                MessageBox.Show(this, "在头标区前不能插入字段。");
                return;
            }

            bool bControlField = Record.IsControlFieldName(this.DefaultFieldName);
            string strDefaultValue = "";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";


            this.InsertField(this.FocusedFieldIndex,
                this.DefaultFieldName,
                "  ", //" " strIndicator
                strDefaultValue);
        }

        // 给当前字段的后面新增一个字段
        internal void InsertAfterFieldWithoutDlg()
        {
            Debug.Assert(this.SelectedFieldIndices.Count == 1, "在'后插'时，SelectedFieldIndices数量必须为1");

            bool bControlField = Record.IsControlFieldName(this.DefaultFieldName);
            string strDefaultValue = "";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";

            // 调InsertAfterField，把界面也管了
            this.InsertFieldAfter(this.FocusedFieldIndex,
                this.DefaultFieldName,
                "  ",
                strDefaultValue);
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
            if (this.SelectedFieldIndices.Count > 1)
            {
                MessageBox.Show(this, "多选字段的状态下不能插入新字段");
                return;
            }
            // parameters:
            //      nAutoComplate   0: false; 1: true; -1:保持当前记忆状态
            InsertField(this.FocusedFieldIndex, -1, -1);  // true, false
        }

        // 插入字段
        // parameters:
        //      nInsertBefore   0: false; 1: true; -1:保持当前记忆状态 2010/12/12
        //      nAutoComplete   0: false; 1: true; -1:保持当前记忆状态 2008/7/29
        /// <summary>
        /// 插入新字段
        /// </summary>
        /// <param name="nFieldIndex">字段位置下标</param>
        /// <param name="nInsertBofore">是否为插入到 nFieldIndex 位置的前面? 0: 否; 1: 是; -1:保持当前记忆状态</param>
        /// <param name="nAutoComplete">是否自动完成? 0: 否; 1: 是; -1:保持当前记忆状态</param>
        /// <returns>false: 放弃插入新字段; true: 成功</returns>
        public bool InsertField(int nFieldIndex,
            int nInsertBofore,
            int nAutoComplete)
        {
            NewFieldDlg dlg = new NewFieldDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            if (nInsertBofore == -1)
            {
                if (this.AppInfo == null)
                    dlg.InsertBefore = this.m_bInsertBefore;
                else
                    dlg.InsertBefore = this.AppInfo.GetBoolean("marceditor",
                        "newfielddlg_insertbefore",
                        false);
            }
            else
                dlg.InsertBefore = (nInsertBofore == 1 ? true : false);

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
                start_field_name = this.Record.Fields[nFieldIndex].Name;

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

            if (nInsertBofore == -1)
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

            List<string> results = null;
            string strError = "";
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
                out results,
                out strError);
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

            if (dlg.InsertBefore == true)
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
            if (this.record.Fields.Count == 0)
                return;

            var old_fields = GetFields(0, this.record.Fields.Count);
            this.record.Fields.Clear();
            this.SelectedFieldIndices.Clear();
            this.AppendFieldsDelete(old_fields);
        }

        public Field RemoveField(int field_index)
        {
            var old_field = this.record.Fields[field_index];
            this.record.Fields.RemoveAt(field_index);
            this.AppendFieldDelete(field_index, new List<Field> { old_field });
            return old_field;
        }

        public void RemoveFields(IEnumerable<int> indices)
        {
            var old_fields = GetFields(indices);
            this.record.Fields.RemoveAt(indices.ToArray());
            this.AppendFieldsDelete(indices, old_fields);
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
            this.AppendInsertFields(field_index + 1,
                new List<Field> { ret });
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
            this.AppendInsertFields(field_index,
                new List<Field> { ret });
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
            // 2024/7/14
            // 添加操作历史
            var new_fields = new List<Field> { this.record.Fields[old_count] };
            this.AppendInsertFields(old_count, new_fields);

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
            if (index == -1)
                AppendInsertFields(old_count, new List<Field> { ret });
            else
                AppendFieldChange("FieldChange", index, old_field, ret);
            return ret;
        }

        #endregion

        // 带对话框的删除字段
        // return:
        //      false   放弃
        //      true    已经处理
        internal bool DeleteFieldWithDlg(bool show_dialog = true)
        {
            Debug.Assert(this.SelectedFieldIndices.Count > 0, "在'删除'时，SelectedFieldIndices个数必须大于0");

            if (show_dialog)
            {
                string strFieldInfo = "";
                if (this.SelectedFieldIndices.Count == 1)
                {
                    strFieldInfo = "'" + this.FocusedField.Name + "'";
                }
                else
                {
                    strFieldInfo = "选中的'"
                        + Convert.ToString(this.SelectedFieldIndices.Count)//this.FocusedField.m_strName 
                        + "'个";
                }

                string strText = "确实要删除"
                    + strFieldInfo
                    + "字段吗?";
                /*
                DialogResult result = MessageBox.Show(this,
                    strText,
                    "MarcEditor",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No) 
                    return false;
                 */

                DeleteFieldDlg dlg = new DeleteFieldDlg();
                GuiUtil.AutoSetDefaultFont(dlg);

                dlg.Message = strText;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult != DialogResult.Yes)
                    return false;
            }

            int[] fieldIndices = new int[this.SelectedFieldIndices.Count];
            for (int i = 0; i < fieldIndices.Length; i++)
            {
                fieldIndices[i] = (int)this.SelectedFieldIndices[i];
            }
            // this.record.Fields.RemoveAt(fieldIndices);
            this.RemoveFields(fieldIndices);

            // 2007/7/17
            AdjustOriginY();
            return true;
        }

        // 删除大量内容(字段)后，窗口原点偏移量可能处在不合适的位置，令操作者看不到上面显示的内容
        // 这时需要改变原点偏移量
        // 2007/7/17
        void AdjustOriginY()
        {

            if (-this.DocumentOrgY > this.DocumentHeight)
            {
                int nDelta = this.DocumentHeight - this.ClientHeight;
                if (nDelta < 0)
                    nDelta = 0;
                this.DocumentOrgY = -(nDelta);
            }

            // 设卷滚条信息
            this.SetScrollBars(ScrollBarMember.Vert);
        }

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
        internal bool HasTemplateOrValueListDef(
            string strDefName,
            out string strCurName)
        {
            strCurName = "";
            if (this.SelectedFieldIndices.Count > 1)
                return false;

            Debug.Assert(this.FocusedField != null, "FocusedField不可能为null");

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.ValueKernel;

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
                    this.SelectionStart,
                    out strSubfieldName,
                    out nSubfieldDupIndex,
                    out strSubfieldValue);
            }
            else
            {
                return false;
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
                out strError);
        }

        // 菜单：设置当前字段、指示符、子字段缺省值
        /*
        public void SetCurFirstDefaultValue(object sender, EventArgs e)
        {
            SetCurFirstDefaultValue(false);
        }
        */

        internal void SetCurrentDefaultValue(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            int index = (int)item.Tag;

            SetDefaultValue(false, index);
        }

#if NOOOOOOOOOOOOOOOOOO
        // 设置当前位置的缺省值
        // parameters:
        //      bSimulate   如果==true，表示模拟获得缺省值，但是并不在当前位置插入值。注意，某些对种子号有推动作用的宏，这里只能取当前值而不能增量。
        public string SetCurDefaultValue(bool bSimulate)
        {
            if (this.SelectedFieldIndices.Count > 1)
            {
                if (bSimulate == false)
                    Console.Beep();
                return null;
            }

            Debug.Assert(this.FocusedField != null, "FocusedField不可能为null");

            if (bSimulate == false) // 模拟态不能对edit进行干扰?
                this.Flush();   // 因为要新旧值进行比较,所以flush及时把小Edit中文字兑现到内存,很重要

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.ValueKernel;

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
                    this.SelectionStart,
                    out strSubfieldName,
                    out nSubfieldDupIndex,
                    out strSubfieldValue);
            }
            else
            {
                return null;
            }


            string strError;
            string strOutputValue;
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
                    bSimulate,
                    strFieldName,
                    "",
                    out strOutputValue,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    return null;

                // 如果当前插入符在内容上,则不要包含指示符部分
                if (this.m_nFocusCol == 3
                    && strSubfieldName == "")
                {
                    if (this.FocusedField.Indicator.Length == 2)
                    {
                        if (strOutputValue.Length <= 2)
                            strOutputValue = "";
                        else
                            strOutputValue = strOutputValue.Substring(2);
                    }
                }

                if (bSimulate == true)
                {

                    return strOutputValue;
                }

                if (this.m_nFocusCol == 3
    && strSubfieldName == "")
                {
                    if (this.FocusedField.Value != strOutputValue)
                        this.FocusedField.Value = strOutputValue;
                }
                else
                {
                    if (this.FocusedField.IndicatorAndValue != strOutputValue)
                        this.FocusedField.IndicatorAndValue = strOutputValue;
                }
                
                // 不让小edit全选上
                // this.curEdit.SelectionLength = 0;
            }
            else
            {
                // 子字段值情形
                int nOldSelectionStart = this.SelectionStart;

                // 获得宏值
                // parameters:
                //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
                // return:
                //      -1  error
                //      0   not found 
                //      1   found
                nRet = GetDefaultValue(
                    bSimulate,
                    strFieldName,
                    strSubfieldName,
                    out strOutputValue,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }

                if (nRet == 0)
                    return null;

                /*
                MarcUtil.ReplaceSubfield(ref this.FocusedField.Value,
    strSubfieldName,
    nSubfieldDupIndex,
    strSubfieldName + strOutputValue);
                 */

                if (bSimulate == true)
                    return strOutputValue;

                if (strSubfieldName == "#indicator")
                {
                    this.FocusedField.Indicator = strOutputValue;
                }
                else
                {
                    if (strSubfieldValue != strOutputValue)
                    {
                        Subfield subfield = this.FocusedField.Subfields[strSubfieldName, nSubfieldDupIndex];
                        subfield.Value = strOutputValue;
                    }
                }

                /*
                // 不让小edit全选上
                if (nOldSelectionStart < this.curEdit.Text.Length)
                    this.curEdit.SelectionStart = nOldSelectionStart;
                else
                    this.curEdit.SelectionLength = 0;
                 */
            }

            return "";
        ERROR1:
            if (bSimulate == true)
                return "出错: " + strError;

            MessageBox.Show(this, strError);
            return "";
        }
#endif

        // 警告：因为本函数无法让宏真正执行，所以废弃不用
        // 设置当前位置的缺省值。只取缺省值的第一个
        // parameters:
        //      bSimulate   如果==true，表示模拟获得缺省值，但是并不在当前位置插入值。注意，某些对种子号有推动作用的宏，这里只能取当前值而不能增量。
        void SetCurrentDefaultValue(string strValue)
        {
            Debug.Assert(false, "本函数已经被废弃");

            if (this.SelectedFieldIndices.Count > 1)
            {
                Console.Beep();
                return;
            }

            Debug.Assert(this.FocusedField != null, "FocusedField不可能为null");

            this.Flush();   // 因为要新旧值进行比较,所以flush及时把小Edit中文字兑现到内存,很重要

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.ValueKernel;

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
                    this.SelectionStart,
                    out strSubfieldName,
                    out nSubfieldDupIndex,
                    out strSubfieldValue);
            }
            else
            {
                return;
            }

            // string strError;
            // int nRet = 0;

            if (strSubfieldName == "")
            {
                // 字段值或者指示符值情形
                if (this.m_nFocusCol == 3
    && strSubfieldName == "")
                {
                    if (this.FocusedField.Value != strValue)
                        this.FocusedField.Value = strValue;
                }
                else
                {
                    if (this.FocusedField.IndicatorAndValue != strValue)
                        this.FocusedField.IndicatorAndValue = strValue;
                }

                // 不让小edit全选上
                // this.curEdit.SelectionLength = 0;
            }
            else
            {
                // 子字段值情形
                int nOldSelectionStart = this.SelectionStart;

                if (strSubfieldName == "#indicator")
                {
                    this.FocusedField.Indicator = strValue;
                }
                else
                {
                    if (strSubfieldValue != strValue)
                    {
                        Subfield subfield = this.FocusedField.Subfields[strSubfieldName, nSubfieldDupIndex];
                        subfield.Value = strValue;
                    }
                }

                /*
                // 不让小edit全选上
                if (nOldSelectionStart < this.curEdit.Text.Length)
                    this.curEdit.SelectionStart = nOldSelectionStart;
                else
                    this.curEdit.SelectionLength = 0;
                 */
            }

            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
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

            if (bSimulate == false) // 模拟态不能对edit进行干扰?
                this.Flush();   // 因为要新旧值进行比较,所以flush及时把小Edit中文字兑现到内存,很重要

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.ValueKernel;

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
                    this.SelectionStart,
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
                int nOldSelectionStart = this.SelectionStart;

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

                /*
                // 不让小edit全选上
                if (nOldSelectionStart < this.curEdit.Text.Length)
                    this.curEdit.SelectionStart = nOldSelectionStart;
                else
                    this.curEdit.SelectionLength = 0;
                 */
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
            // bool bSimulate,
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

                /*
                strOutputValue = MacroTimeValue(strOutputValue);

                // 替换下划线
                strOutputValue = strOutputValue.Replace("_", " ");


                if (strSubFieldName == "")
                {
                    // 替换子字段符号
                    strOutputValue = strOutputValue.Replace('$', Record.SUBFLD);
                }
                 * */

                results.Add(strOutputValue);
            }

            return results.Count;
        }

#if NOOOOOOOOOOOOOOOOOOO
        // 获得宏值
        // parameters:
        //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
        // return:
        //      -1  error
        //      0   not found 
        //      1   found
        private int GetDefaultValue(
            bool bSimulate,
            string strFieldName,
            string strSubFieldName,
            out string strOutputValue,
            out string strError)
        {
            Debug.Assert(strFieldName != null, "strFieldName参数不能为null");
            Debug.Assert(strSubFieldName != null, "strSubFieldName参数不能为null");
            // Debug.Assert(strValue != null, "strValue参数不能为null");

            strError = "";
            strOutputValue = "";

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

            XmlNode nodeValue = null;

            if (strSubFieldName == "#indicator")
            {
                nodeValue = node.SelectSingleNode("Property/Indicator");
            }
            else
            {
                nodeValue = node.SelectSingleNode("Property/DefaultValue");
            }
            if (nodeValue == null)
                return 0;

            if (strSubFieldName == "#indicator")
            {
                strOutputValue = DomUtil.GetAttr(nodeValue, "DefaultValue");
            }
            else
            {
                strOutputValue = nodeValue.InnerText;
            }

            // 去掉定义值中的\r\n或者单独的\r和\n。这种具有\r\n的效果可能由notepad中折行状态时paste到编辑配置文件对话框并保存来造成.
            strOutputValue = strOutputValue.Replace("\r", "");
            strOutputValue = strOutputValue.Replace("\n", "");


            ParseMacroEventArgs e = new ParseMacroEventArgs();
            e.Macro = strOutputValue;
            e.Simulate = bSimulate;

            TemplateControl_ParseMacro((object)this, e);
            if (String.IsNullOrEmpty(e.ErrorInfo) == false)
            {
                strError = e.ErrorInfo;
                return -1;
            }

            strOutputValue = e.Value;

            /*
            strOutputValue = MacroTimeValue(strOutputValue);

            // 替换下划线
            strOutputValue = strOutputValue.Replace("_", " ");


            if (strSubFieldName == "")
            {
                // 替换子字段符号
                strOutputValue = strOutputValue.Replace('$', Record.SUBFLD);
            }
             */
            return 1;
        }

#endif

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
            strOutputValue = strOutputValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);   // $?

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

        // 菜单：值列表
        internal void GetValueFromValueList(object sender,
            System.EventArgs e)
        {
            this.GetValueFromValueList();
        }

        // 取当前位置的值列表
        /// <summary>
        /// 显示当前位置的指列表对话框
        /// </summary>
        public void GetValueFromValueList()
        {
            Debug.Assert(this.FocusedField != null, "在GetSubFieldValueWithDlg()时，FocusedField不可能为null");

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.ValueKernel;


            string strSubfieldName = "";
            string strSubfieldValue = "";
            int nSubfieldDupIndex = 0;
            MarcEditor.GetCurrentSubfield(strFieldValue,
                this.SelectionStart,
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

                /*
                // 不让小edit全选上
                if (nOldSelectionStart < this.curEdit.Text.Length)
                    this.curEdit.SelectionStart = nOldSelectionStart;
                else
                    this.curEdit.SelectionLength = 0;
                 */
            }

        }

        // 调模板取值函数
        /// <summary>
        /// 打开当前按位置的定长模板对话框
        /// </summary>
        public void GetValueFromTemplate()
        {
            Debug.Assert(this.FocusedField != null, "在GetSubFieldValueWithDlg()时，FocusedField不可能为null");

            string strFieldName = this.FocusedField.Name;
            string strFieldValue = this.FocusedField.ValueKernel;

            string strSubfieldName = "";
            string strSubfieldValue = "";
            int nSubfieldDupIndex = 0;
            MarcEditor.GetCurrentSubfield(strFieldValue,
                this.SelectionStart,
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

                // 不让小edit全选上
                this.curEdit.SelectionLength = 0;
            }
            else if (strSubfieldName == "indicator")
            {

            }
            else
            {
                int nOldSelectionStart = this.SelectionStart;

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
                {
                    /*
					XmlNode valueListNode;
					nRet = GetValueListNode(strFieldName,
						strSubfieldName,
						out valueListNode,
						out strError);
					if (nRet == -1)
					{
						MessageBox.Show(this,strError);
						return;
					}
					if (nRet == 0)
						return;

					Debug.Assert(valueListNode != null,"不可能的情况。");

					ValueListDlg dlg = new ValueListDlg();
					dlg.Text = "值列表 : " + strFieldName + new string(Record.KERNEL_SUBFLD,1) + strSubfieldName;
					nRet = dlg.Initialize(valueListNode,
						this.Lang,
						out strError);
					if (nRet == -1)
					{
						MessageBox.Show(this,strError);
						return;
					}

					dlg.ShowDialog(this);

					if (dlg.DialogResult != DialogResult.OK)
						return;

					if (dlg.listView_valueList.SelectedItems.Count > 0)
						strOutputValue = dlg.listView_valueList.SelectedItems[0].Text;
					else
						strOutputValue = "";
                     */
                    return;
                }

                if (strSubfieldValue != strOutputValue)
                {
                    Subfield subfield = this.FocusedField.Subfields[strSubfieldName, nSubfieldDupIndex];
                    subfield.Value = strOutputValue;
                }

                // 不让小edit全选上
                if (nOldSelectionStart < this.curEdit.Text.Length)
                    this.curEdit.SelectionStart = nOldSelectionStart;
                else
                    this.curEdit.SelectionLength = 0;
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

            strFieldValue = strFieldValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);

            int nCurSubfieldX = -1;
            for (int i = 0; i < strFieldValue.Length; i++)
            {
                if (i >= nSelectionStart)
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

        void CalcuAllHeight()
        {
            if (this.record == null)
                return;

            for (int i = 0; i < this.record.Fields.Count; i++)
            {
                Field field = this.record.Fields[i];
                field.CalculateHeight(null, false);
            }
        }

        private void MarcEditor_EnabledChanged(object sender, EventArgs e)
        {
            if (this.curEdit != null)
            {
                if (this.Enabled == false)
                {
                    this.curEdit.Hide();
                }
                else
                {
                    this.curEdit.Show();
                }
            }

            this.Invalidate();  // 失效窗口，重新显示
            // this.Update();   // 优化
        }


#if NOOOOOOOOOOOOOOOO
        private void MarcEditor_Enter(object sender, EventArgs e)
        {
            if (this.curEdit != null)
            {
                if (this.curEdit.DisableFlush > 0)  // 避免因Multiline改变而激动
                    return;

                this.SetEditPos();
                // this.curEdit.Focus();   // ??
            }
                /*
            else
            {
                // 2008/11/26
                if (this.FocusedFieldIndex == -1)
                    this.FocusedFieldIndex = 0;

                this.Focus();
                this.SetEditPos();
                // this.curEdit.Focus();
            }
                 * */
        }
#endif

        /// <summary>
        /// 当前控件是否拥有输入焦点
        /// </summary>
        public override bool Focused
        {
            get
            {
                if (this.DesignMode)
                    return base.Focused;

                if (base.Focused == true)
                    return true;

                if (this.curEdit != null
                    && this.curEdit.Focused == true)
                    return true;

                return false;
            }
        }

        class MarcEditorState
        {
            // 字段名区域像素宽度
            public int FieldNameCaptionWidth { get; set; }
        }

        public string UiState
        {
            get
            {
                MarcEditorState state = new MarcEditorState
                {
                    FieldNameCaptionWidth = this.FieldNameCaptionWidth,
                };
                return JsonConvert.SerializeObject(state);
            }
            set
            {
                MarcEditorState state = JsonConvert.DeserializeObject<MarcEditorState>(value);
                if (state != null)
                {
                    this.FieldNameCaptionWidth = state.FieldNameCaptionWidth;
                }
            }
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
