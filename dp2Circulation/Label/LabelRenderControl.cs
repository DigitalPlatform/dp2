using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using System.Diagnostics;

namespace dp2Circulation
{
    /// <summary>
    /// 标签页面渲染控件。
    /// 只显示出一页标签内容
    /// </summary>
    public partial class LabelRenderControl : UserControl
    {
        LabelParam label_param = null;

        PrintLabelDocument document = null;

        string m_strPrintStyle = "";    // "TestingGrid";    // 打印风格

        public string PrintStyle
        {
            get
            {
                return this.m_strPrintStyle;
            }
            set
            {
                this.m_strPrintStyle = value;
            }
        }

        public LabelParam LabelParam
        {
            get
            {
                return this.label_param;
            }
            set
            {
                this.label_param = value;
                if (value != null)
                {
#if NO
                    if (this.AutoScrollMinSize.Width != value.PageWidth
                        || this.AutoScrollMinSize.Height != value.PageHeight)
                        this.AutoScrollMinSize = new Size(value.PageWidth, value.PageHeight);
#endif
                    SetAutoScroll(value);
                }

                this.Invalidate();
            }
        }

        // 设置卷滚区域
        void SetAutoScroll(LabelParam label_param)
        {
            double nWidth = 0;
            double nHeight = 0;

            bool bPrinter = true;   // 是否要考虑打印机的纸张尺寸
            if (label_param != null && string.IsNullOrEmpty(label_param.DefaultPrinter) == true)
                bPrinter = false;

            if (label_param != null)
            {
                if (label_param.RotateDegree == 90 || label_param.RotateDegree == 270)
                {
                    nWidth = label_param.PageHeight;
                    nHeight = label_param.PageWidth;
                }
                else
                {
                    nWidth = label_param.PageWidth;
                    nHeight = label_param.PageHeight;
                }
            }


            if (this.document != null && bPrinter == true)
                nWidth = Math.Max(nWidth, this.document.DefaultPageSettings.Bounds.Width);    // this.document.DefaultPageSettings.PaperSize.Width

            if (this.document != null && bPrinter == true)
                nHeight = Math.Max(nHeight, this.document.DefaultPageSettings.Bounds.Height);    // this.document.DefaultPageSettings.PaperSize.Height

#if NO
            bool bLandscape = false;
            if (label_param != null)
                bLandscape = label_param.Landscape;
            else 
#endif
#if NO
            if (this.document != null)
                bLandscape = this.document.DefaultPageSettings.Landscape;

            if (bLandscape == false)
                this.AutoScrollMinSize = new Size((int)nWidth, (int)nHeight);
            else
                this.AutoScrollMinSize = new Size((int)nHeight, (int)nWidth);
#endif

            this.AutoScrollMinSize = new Size((int)nWidth, (int)nHeight);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LabelRenderControl()
        {
            InitializeComponent();

            //this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            //this.SetStyle(ControlStyles.ResizeRedraw, true);
        }


        // 设置标签内容文件
        public int SetLabelFile(string strLabelFilename,
            out string strError)
        {
            strError = "";

            if (this.document == null)
                this.document = new PrintLabelDocument();

            int nRet = this.document.Open(strLabelFilename,
                out strError);
            if (nRet == -1)
                return -1;

            SetAutoScroll(this.label_param);

            return 0;
        }

        // 设置标签内容文件
        public int SetLabelFile(StreamReader sr,
            out string strError)
        {
            strError = "";

            if (this.document == null)
                this.document = new PrintLabelDocument();

            int nRet = this.document.Open(sr,
                out strError);
            if (nRet == -1)
                return -1;

            SetAutoScroll(this.label_param);
            return 0;
        }

        public PrintDocument PrintDocument
        {
            get
            {
                return this.document;
            }
            set
            {
                this.document = value as PrintLabelDocument;
                SetAutoScroll(this.label_param);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.document != null
                && label_param != null)
            {
                this.document.PreviewMode = true;
                this.document.IsDesignMode = true;
                this.document.OriginPoint = new Point(this.DisplayRectangle.X, this.DisplayRectangle.Y);
                this.document.Rewind();
                // TODO: 要求 <page> 元素的 width 和 height 属性必须具备
                // PageSettings pageSettings = new PageSettings();
#if NO
                Rectangle pageBounds = new Rectangle(0,
                    0,
                    this.label_param.PageWidth,
                    this.label_param.PageHeight);

                                Rectangle marginBounds = new Rectangle(pageBounds.X + this.label_param.PageMargins.Left,
                    pageBounds.Y + this.label_param.PageMargins.Right,
                    pageBounds.Width - this.label_param.PageMargins.Left - this.label_param.PageMargins.Right,
                    pageBounds.Height - this.label_param.PageMargins.Top - this.label_param.PageMargins.Bottom);

#endif
                PageSettings settings = this.document.DefaultPageSettings.Clone() as PageSettings;
                if (string.IsNullOrEmpty(label_param.DefaultPrinter) == true)
                {
                    PaperSize paper_size = new PaperSize("Custom Label", 
                        (int)label_param.PageWidth,
                        (int)label_param.PageHeight);
                    settings.PaperSize = paper_size;
                }

                Rectangle pageBounds = new Rectangle(0,
                    0,
                    settings.PaperSize.Width,
                    settings.PaperSize.Height);
#if NO
                if (label_param.Landscape == true)
                    pageBounds = new Rectangle(0,
                    0,
                    settings.PaperSize.Height,
                    settings.PaperSize.Width);
#endif
                if (settings.Landscape == true)
                    pageBounds = new Rectangle(0,
                    0,
                    settings.PaperSize.Height,
                    settings.PaperSize.Width);

                Rectangle marginBounds = new Rectangle(0,0,0,0);


                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                PrintPageEventArgs e1 = new PrintPageEventArgs(e.Graphics, 
                    marginBounds,
                    pageBounds,
                    settings);  // 
                // e1.HasMorePages = false;
                this.document.DoPrintPage(this,
        this.label_param,
        this.m_strPrintStyle,
        e1);
            }
        }
    }
}
