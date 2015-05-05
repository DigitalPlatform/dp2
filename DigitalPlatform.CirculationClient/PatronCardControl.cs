using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.Xml;
using System.Drawing.Drawing2D;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 读者信息卡片控件
    /// 根据推荐的尺寸，自动规划字体字号，显示布局
    /// </summary>
    public partial class PatronCardControl : Control
    {
        PatronCardInfo _info = new PatronCardInfo();

        PatronCardStyle _style = new PatronCardStyle();
        public PatronCardStyle PatronCardStyle
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
            }
        }

        public string Xml
        {
            get
            {
                return "";
            }
            set
            {
                this._info.Clear();

                if (string.IsNullOrEmpty(value) == true)
                    this._info.Clear();
                else
                {
                    string strError = "";
                    int nRet = this._info.SetData(value,
                        out strError);
                    if (nRet == -1)
                        throw new Exception(strError);
                }

                this.Relayout();
            }
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                this._info.Name = value;
                this.Relayout();
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public PatronCardControl()
        {
            this.DoubleBuffered = true;

            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            this._info.Paint(pe.Graphics,
                0,0,
                this.PatronCardStyle);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            this.Relayout();
        }

        public void Relayout()
        {
            this._info.Layout(Graphics.FromHwnd(this.Handle),
    this.PatronCardStyle,
    this.Size.Width,
    this.Size.Height);
            this.Invalidate();
        }
    }

    /// <summary>
    /// 读者卡片的视觉样式
    /// </summary>
    public class PatronCardStyle
    {
        public float PhotoRatio = 0.7F; // 照片的宽高比

        public int PhtoMaxWidth = 50;   // 照片最大宽度。-1 表示不限制

        // 证条码号 文字颜色
        public Color BarcodeTextColor = Color.Black;

        // 姓名 文字颜色
        public Color NameTextColor = Color.Black;
        public FontStyle NameFontStyle = FontStyle.Bold;

        // 单位 文字颜色
        public Color DepartmentTextColor = Color.Black;
    }

    /// <summary>
    /// 读者卡片信息
    /// 存储显示所需要的一切信息
    /// 只要提供了这些信息，可以随时在 Graphics 对象上绘制出来
    /// </summary>
    public class PatronCardInfo
    {
        public string Name = "";
        public string Barcode = "";
        public string Department = "";

        public int BorrowItemsCount = 0;    // 当前总共借阅了多少册
        public int FreeItemsCount = 0;  // 还可借阅多少册
        public int OverdueItemsCount = 0;   // 已经超期的册数
        public int AmercingItemsCount = 0;  // 尚未交费的事项数目

        int _nPhotoWidth = 0;   // 照片区域宽度
        int _nTextWidth = 0;    // 文字区域宽度

        int _nBarcodeHeight = 0;    // 证条码号文字高度
        int _nNameHeight = 0;   // 姓名文字高度
        int _nDepartmentHeight = 0; // 单位文字高度

        // 装入数据
        public int SetData(
            string strPatronXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strPatronXml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 时出错" + ex.Message;
                return -1;
            }

            this.Name = DomUtil.GetElementText(dom.DocumentElement, "name");
            this.Barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            this.Department = DomUtil.GetElementText(dom.DocumentElement, "department");

            return 0;
        }

        // 初始化尺寸单位
        // 如果尺寸太小，就只显示一个姓名
        // parameters:
        //      photo_ratio 照片宽高比。例如 0.7F
        public void Layout(Graphics g,
            PatronCardStyle style,
            int nWidth,
            int nHeight)
        {
            // 如果宽度大于高度，则照片在最左边
            _nPhotoWidth = (int)((float)nHeight * style.PhotoRatio);
            if (_nPhotoWidth >= 100)
                _nPhotoWidth = 100;
            if (_nPhotoWidth > style.PhtoMaxWidth)
                _nPhotoWidth = style.PhtoMaxWidth;

            _nTextWidth = nWidth - _nPhotoWidth;

            // 证号
            _nBarcodeHeight = Math.Min(50, nHeight / 4);
            // 姓名
            _nNameHeight = Math.Min(100, nHeight / 2);
            // 单位
            _nDepartmentHeight = Math.Min(50, nHeight / 4);

            // TODO: 单位可以最多是两行

            // 如果高度大于宽度，则照片在顶部
        }

        public void Paint(Graphics g,
            long x,
            long y,
            PatronCardStyle style)
        {

            // 证条码号
            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            // format.FormatFlags |= StringFormatFlags.FitBlackBox;
            format.Alignment = StringAlignment.Near;

            {
                RectangleF textRect = new RectangleF(x + _nPhotoWidth,
    y + 0,
    _nTextWidth,
    _nBarcodeHeight);

                Font font = new Font("微软雅黑", GetHeight(_nBarcodeHeight), FontStyle.Regular, GraphicsUnit.Pixel);

                g.DrawString(
                    this.Barcode,
                    font,
                    new SolidBrush(style.BarcodeTextColor),
                    textRect,
                    format);
            }

            // 姓名
            {
                RectangleF textRect = new RectangleF(x + _nPhotoWidth,
    y + _nBarcodeHeight,
    _nTextWidth,
    _nNameHeight);

                Font font = new Font("微软雅黑", 
                    GetHeight(_nNameHeight), 
                    style.NameFontStyle, GraphicsUnit.Pixel);

                g.DrawString(
                    this.Name,
                    font,
                    new SolidBrush(style.NameTextColor),
                    textRect,
                    format);
            }

            // 单位
            {
                RectangleF textRect = new RectangleF(x + _nPhotoWidth,
    y + _nBarcodeHeight + _nNameHeight,
    _nTextWidth,
    _nDepartmentHeight);

                Font font = new Font("微软雅黑", GetHeight(_nDepartmentHeight), FontStyle.Regular, GraphicsUnit.Pixel);

                g.DrawString(
                    this.Department,
                    font,
                    new SolidBrush(style.DepartmentTextColor),
                    textRect,
                    format);
            }
        }

        static float GetHeight(float v)
        {
            return v * 0.8F;
        }

        public void Clear()
        {
            this.Barcode = "";
            this.Name = "";
            this.Department = "";
        }
    }

    /// <summary>
    /// 视觉区域基础类
    /// </summary>
    public class VisualBase
    {
        public VisualBase Container = null;

        public long X = 0;
        public long Y = 0;
        public long Width = 0;
        public long Height = 0;

        Font m_font = null;
        public Font DisplayFont
        {
            get
            {
                if (this.m_font != null)
                    return this.m_font;
                if (this.Container != null)
                    return this.Container.DisplayFont;
                return null;
            }
        }

        public VisualRoot GetRoot()
        {
            VisualBase o = this;
            while (o != null)
            {
                if (o is VisualRoot)
                    return o as VisualRoot;
                o = o.Container;
            }

            return null;
        }

        public Control GetControl()
        {
            VisualRoot root = this.GetRoot();
            if (root == null)
                return null;
            return root.Control;
        }

        public Graphics GetGraphics()
        {
            Control control = this.GetControl();
            if (control == null)
                return null;
            return Graphics.FromHwnd(control.Handle);
        }

        // 重新布局
        public virtual void Relayout(Graphics g)
        {
        }
    }

    public class VisualRoot : VisualBase
    {
        public Control Control = null;

    }

    /// <summary>
    /// 文字框
    /// </summary>
    public class VisualCell : VisualBase
    {
        string m_strText = "";
        public string Text
        {
            get
            {
                return this.m_strText;
            }
            set
            {
                // 提供给外部调用
                if (value != this.m_strText)
                {
                    this.m_strText = value;

                    // 尺寸修改，可以导致向上级对象的连续更新高度宽度

                    // 更新高度参数
                    Graphics g = this.GetGraphics();
                    this.Relayout(g);

                    // 导致父对象重新布局

                    // 失效本对象区域
                }
            }
        }

        // 重新布局
        public override void Relayout(Graphics g)
        {
            // 更新高度参数
            int nHeight = GetTextHeight(g, (int)this.Width);

            // 导致父对象重新布局
            if (nHeight != this.TextHeight
                && this.Container != null)
            {
                this.Container.Relayout(g);
            }

        }

        internal int TextHeight = 0;

        public int GetTextHeight(Graphics g, int nWidthParam)
        {
            int nWidth = nWidthParam;

            // Padding cell_padding = this.Container.Control.m_cellPadding;
            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = g.MeasureString(this.m_strText,
                this.DisplayFont,
                nWidth,
                format);

            this.TextHeight = (int)size.Height; // 保存下来

            return (int)size.Height;
        }

    }

    /// <summary>
    /// 表格的一行
    /// </summary>
    public class VisualRow
    {
        public List<VisualCell> Cells = new List<VisualCell>();
    }

    /// <summary>
    /// 一个表格
    /// </summary>
    public class VisualTable : VisualBase
    {
        public List<VisualRow> Rows = new List<VisualRow>();
    }
}
