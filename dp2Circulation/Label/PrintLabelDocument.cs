using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;

using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.QrCode;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.Drawing;

namespace dp2Circulation
{
    internal class PrintLabelDocument : PrintDocument
    {
        StreamReader _sr = null;

        int m_nPageNo = 0;  // 0表示没有初始化

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
            }
            base.Dispose(disposing);
        }

        // parameters:
        //      reader  注意，本函数的调用者*不要*释放这个 reader
        public int Open(StreamReader reader,
            out string strError)
        {
            strError = "";

            this.Close();
            _sr = reader;
            this.m_nPageNo = 0;
            return 0;
        }

        public int Open(string strLabelFilename,
            out string strError)
        {
            strError = "";

            this.Close();
            try
            {
                _sr = new StreamReader(strLabelFilename, Encoding.GetEncoding(936));
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + strLabelFilename + " 时发生错误: " + ex.Message;
                return -1;
            }

            this.m_nPageNo = 0;

            /*
            this.BeginPrint -= new PrintEventHandler(PrintLabelDocument_BeginPrint);
            this.BeginPrint += new PrintEventHandler(PrintLabelDocument_BeginPrint);
             * */

            return 0;
        }

        public void Rewind()
        {
            if (this._sr != null)
            {
                this._sr.ReadToEnd();    // 必须有这一句，否则下次会读出缓冲区中的内容
                this._sr.BaseStream.Seek(0, SeekOrigin.Begin);
            }
            this.m_nPageNo = 0;
        }

        void PrintLabelDocument_BeginPrint(object sender,
            System.Drawing.Printing.PrintEventArgs e)
        {
            // this.PrintController = new NewPrintController(this.PrintController);
        }

        public void Close()
        {
            if (_sr != null)
            {
                _sr.Close();
                _sr = null;
            }
        }

        // return:
        //      -1  error
        //      0   normal
        //      1   reach file end
        public int GetLabelLines(out List<string> lines,
            out string strError)
        {
            strError = "";

            lines = new List<string>();
            for (; ; )
            {
                string strLine = _sr.ReadLine();
                if (strLine == null)
                    return 1;

                if (strLine == "***")
                {
                    if (_sr.EndOfStream == true)
                        return 1;
                    return 0;
                }

                lines.Add(strLine);
            }

            // return 0;
        }

        static void DrawFourAngel(
    Graphics g,
    Pen pen,
    RectangleF rect,
    float line_length)
        {
            DrawFourAngel(g,
                pen,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                line_length);
        }

        // TODO: 用 DrawLines ，转弯的部分接头?
        static void DrawFourAngel(
            Graphics g,
            Pen pen,
            float x,
            float y,
            float w,
            float h,
            float line_length)
        {
            // *** 左上

            // 横线
            g.DrawLine(pen,
                new PointF(x + 1, y + 1),
                new PointF(x + 1 + line_length, y + 1)
                );

            // 竖线
            g.DrawLine(pen,
                new PointF(x + 1, y + 1),
                new PointF(x + 1, y + 1 + line_length)
                );

            // *** 右上

            // 横线
            g.DrawLine(pen,
                new PointF(x + w - 1, y + 1),
                new PointF(x + w - 1 - line_length, y + 1)
                );

            // 竖线
            g.DrawLine(pen,
                new PointF(x + w - 1, y + 1),
                new PointF(x + w - 1, y + 1 + line_length)
                );

            // *** 左下

            // 横线
            g.DrawLine(pen,
                new PointF(x + 1, y + h - 1),
                new PointF(x + 1 + line_length, y + h - 1)
                );

            // 竖线
            g.DrawLine(pen,
                new PointF(x + 1, y + h - 1),
                new PointF(x + 1, y + h - 1 - line_length)
                );

            // *** 右下

            // 横线
            g.DrawLine(pen,
                new PointF(x + w - 1, y + h - 1),
                new PointF(x + w - 1 - line_length, y + h - 1)
                );

            // 竖线
            g.DrawLine(pen,
                new PointF(x + w - 1, y + h - 1),
                new PointF(x + w - 1, y + h - 1 - line_length)
                );
        }


        /*
        // 测算出一页里面有多少个label
        static int GetLabelCountOfOnePage(LabelParam label_param,
            PrintPageEventArgs e)
        {
            int nYCount = 0;
            int nXCount = 0;

            // 垂直方向的个数
            nYCount = (e.PageBounds.Height - label_param.PageMargins.Top - label_param.PageMargins.Bottom)
                / label_param.Height;
            // 水平方向的个数
            nXCount = (e.PageBounds.Width - label_param.PageMargins.Left - label_param.PageMargins.Right)
            / label_param.Width;

            return nXCount * nYCount;
        }
         * */

        public Point OriginPoint
        {
            get;
            set;
        }

        /// <summary>
        /// 是否处于预览模式
        /// 程序根据这个标志决定图形原点位置
        /// </summary>
        public bool PreviewMode
        {
            get;
            set;
        }

        /// <summary>
        /// 是否处在设计状态
        /// 在设计状态下，会显示出配置的页面和当前打印机的页面图像，便于对比
        /// </summary>
        public bool IsDesignMode
        {
            get;
            set;
        }

        static DecimalPadding RotatePadding(DecimalPadding padding, bool bLandsape)
        {
            if (bLandsape == false)
                return padding;

            return new DecimalPadding(padding.Bottom,   // bottom -->left
                padding.Top,    // top-->right
                padding.Left,   // left-->top
                padding.Right); // right-->bottom
        }

        static RectangleF RotateRectangle(RectangleF rect, bool bLandsape)
        {
            if (bLandsape == false)
                return rect;

            return new RectangleF(rect.Y,
                rect.X,
                rect.Height,
                rect.Width);
        }

        // 旋转后中心归位
        static void CenterMove(int nRotateDegree,
            float width,
            float height,
            out float x,
            out float y)
        {
            x = 0;
            y = 0;

            if (nRotateDegree == 0
                || nRotateDegree == 360)
                return;

            if (nRotateDegree == 90)
            {
                x = height;
                y = 0;
                return;
            }

            if (nRotateDegree == 180)
            {
                x = width;
                y = height;
                return;
            }

            if (nRotateDegree == 270)
            {
                x = 0;
                y = width;
                return;
            }

            Debug.Assert(false, "不应该出现的旋转度数 " + nRotateDegree.ToString());
        }

        internal void DoPrintPage(
            IWin32Window owner,
            LabelParam label_param,
            string strStyle,
            PrintPageEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (e.Cancel == true)
                return;

            bool bTestingGrid = false;
            if (StringUtil.IsInList("TestingGrid", strStyle) == true)
                bTestingGrid = true;

            int nYCount = 0;
            int nXCount = 0;

            double PageWidth = label_param.PageWidth;
            double PageHeight = label_param.PageHeight;

            int nPageWidth = e.PageBounds.Width;    // PageBounds 中已经是按照 Landscape 处理过的方向了
            if (PageWidth != 0)
                nPageWidth = (int)PageWidth;

            int nPageHeight = e.PageBounds.Height;
            if (PageHeight != 0)
                nPageHeight = (int)PageHeight;

            DecimalPadding PageMargins = RotatePadding(label_param.PageMargins,
                e.PageSettings.Landscape);  // label_param.Landscape
#if NO
            // 垂直方向的个数
            nYCount = (e.PageBounds.Height - label_param.PageMargins.Top - label_param.PageMargins.Bottom)
                / label_param.Height;
            // 水平方向的个数
            nXCount = (e.PageBounds.Width - label_param.PageMargins.Left - label_param.PageMargins.Right)
            / label_param.Width;
#endif
            // 垂直方向的个数
            nYCount = (int)
                (
                (double)(nPageHeight - PageMargins.Top - PageMargins.Bottom)
                / (double)label_param.LabelHeight
                );
            // 水平方向的个数
            nXCount = (int)
                (
                (double)(nPageWidth - PageMargins.Left - PageMargins.Right)
                / (double)label_param.LabelWidth
                );

            int from = 0;
            int to = 0;
            bool bOutput = true;
            // 如果 e.PageSettings.PrinterSettings.FromPage == 0，会被当作打印第一页
            if (e.PageSettings.PrinterSettings.PrintRange == PrintRange.SomePages
                && e.PageSettings.PrinterSettings.FromPage >= 1)
            {
                from = e.PageSettings.PrinterSettings.FromPage;
                to = e.PageSettings.PrinterSettings.ToPage;

                // 交换，保证from为小
                if (from > to)
                {
                    int temp = to;
                    to = from;
                    from = temp;
                }

                if (this.m_nPageNo == 0)
                {
                    this.m_nPageNo = from;

                    Debug.Assert(this.m_nPageNo >= 1, "");
                    long nLabelCount = (nXCount * nYCount) * (this.m_nPageNo - 1);

                    // 从文件中跳过这么多label的内容行
                    for (int i = 0; i < nLabelCount; i++)
                    {
                        List<string> lines = null;
                        nRet = this.GetLabelLines(out lines,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        if (nRet == 1)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                /*
                if (this.m_nPageNo >= from
                    && this.m_nPageNo <= to)
                {
                    bOutput = true;
                }
                else
                {
                    bOutput = false;
                }
                 * */
            }
            else
            {
                if (this.m_nPageNo == 0)
                    this.m_nPageNo = 1; // 一般性的初始化
            }


            // 加快运行速度
            float nXDelta = e.PageSettings.PrintableArea.Left;
            float nYDelta = e.PageSettings.PrintableArea.Top;

            /*
            if (this.PrintController.IsPreview == true)
            {
                nXDelta = 0;
                nYDelta = 0;
            }
             * */

            if (this.OriginAtMargins == true
                || this.PreviewMode == true)   // false
            {
                // true 如果图形起始于页面边距。否则起始于可打印区域
                nXDelta = 0;
                nYDelta = 0;
            }

            if (this.OriginPoint != null)
            {
                nXDelta -= this.OriginPoint.X;
                nYDelta -= this.OriginPoint.Y;
            }

#if NO
            float nPrintableWidth = e.PageSettings.PrintableArea.Width;
            float nPrintableHeight = e.PageSettings.PrintableArea.Height;
#endif


            if (this.IsDesignMode)
            {
                // 绘制整个纸张背景 白色
                using (Brush brushBack = new SolidBrush(Color.White))
                {
                    RectangleF rectPaper = new RectangleF(0 + 1 - nXDelta,
                        0 + 1 - nYDelta,
                        e.PageBounds.Width - 2,
                        e.PageBounds.Height - 2);
                    e.Graphics.FillRectangle(brushBack, rectPaper);
                }
            }

            // 绘制可打印区域
            // 鲜红色
            if (bTestingGrid == true && bOutput == true)
            {
                float nXOffs = 0;
                float nYOffs = 0;

                // 如果为正式打印，左上角(0,0)已经就是可以打印区域的左上角
                // 如果为preview模式，则左上角要向右向下移动，才能模拟出显示效果

#if NO
                if (this.OriginAtMargins == true
                    || this.PreviewMode == true)
                {
                    nXOffs = e.PageSettings.PrintableArea.Left;
                    nYOffs = e.PageSettings.PrintableArea.Top;
                }
#endif


                if (this.OriginPoint != null)
                {
                    nXOffs += this.OriginPoint.X;
                    nYOffs += this.OriginPoint.Y;
                }

                RectangleF rect = RotateRectangle(e.PageSettings.PrintableArea,
                    e.PageSettings.Landscape);  // label_param.Landscape

                if (this.OriginAtMargins == true
    || this.PreviewMode == true)
                {
                }
                else
                {
                    rect.X = 0;
                    rect.Y = 0;
                }

                rect.Offset(nXOffs, nYOffs);

                using (Pen pen = new Pen(Color.Red, (float)1))
                {
                    DrawFourAngel(
    e.Graphics,
    pen,
    rect,
    50);    // 半英寸
                }
            }

            // 加入变换
            e.Graphics.TranslateTransform(-nXDelta, -nYDelta);
            nXDelta = 0;
            nYDelta = 0;

            if (label_param.RotateDegree != 0)
            {
                float x_offs, y_offs;
                CenterMove(label_param.RotateDegree,
            (float)label_param.PageWidth,  // e.PageBounds.Width,
            (float)label_param.PageHeight, // e.PageBounds.Height,
            out x_offs,
            out y_offs);
                e.Graphics.TranslateTransform(x_offs, y_offs);
                e.Graphics.RotateTransform((float)label_param.RotateDegree);
            }

            if (this.IsDesignMode)
            {
                // 绘制配置文件的页面区域
                if (PageHeight > 0 && PageWidth > 0)
                {
                    using (Brush brushBack = new SolidBrush(Color.FromArgb(128, Color.LightYellow)))
                    {
                        RectangleF rectPaper = new RectangleF(0 - nXDelta,
                            0 - nYDelta,
                            (float)PageWidth,
                            (float)PageHeight);
                        e.Graphics.FillRectangle(brushBack, rectPaper);
                    }
                }
            }

            // 绘制内容区域边界(也就是排除了页面边空的中间部分)
            // 淡绿色
            if (bTestingGrid == true && bOutput == true)
            {
                using (Pen pen = new Pen(Color.FromArgb(0, 100, 0), (float)2)) // 3
                {

#if NO
                e.Graphics.DrawRectangle(pen,
                    PageMargins.Left - nXDelta,
                    PageMargins.Top - nYDelta,
                    e.PageBounds.Width - PageMargins.Left - PageMargins.Right,
                    e.PageBounds.Height - PageMargins.Top - PageMargins.Bottom);
#endif
                    e.Graphics.DrawRectangle(pen,
        (float)PageMargins.Left - nXDelta,
        (float)PageMargins.Top - nYDelta,
        (float)nPageWidth - (float)PageMargins.Left - (float)PageMargins.Right,
        (float)nPageHeight - (float)PageMargins.Top - (float)PageMargins.Bottom);

                }
            }

            bool bEOF = false;

            float y = (float)PageMargins.Top;
            // 每一行的循环
            for (int i = 0; i < nYCount; i++)
            {
                bool bDisplay = true;
                if (this.IsDesignMode == true)
                {
                    RectangleF rectLine = new RectangleF(
    (float)0 - nXDelta,
    (float)y - nYDelta,
    (float)label_param.LabelWidth * nXCount,
    (float)label_param.LabelHeight);
                    if (rectLine.Top > e.Graphics.ClipBounds.Bottom)
                    {
                        // Debug.WriteLine("break line loop at " + i.ToString());
                        break;  // 当前行在剪裁区域的下方，可以中断循环了
                    }
                    if (rectLine.IntersectsWith(e.Graphics.ClipBounds) == false)
                    {
                        // Debug.WriteLine("skip line " + i.ToString());
                        bDisplay = false;
                    }
                }
                float x = (float)PageMargins.Left;
                // 每一列的循环
                for (int j = 0; j < nXCount; j++)
                {
                    List<string> lines = null;
                    nRet = this.GetLabelLines(out lines,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 1)
                        bEOF = true;

                    if (bOutput == true && bDisplay == true)
                    {
                        // 标签
                        RectangleF rectLabel = new RectangleF(
    (float)x - nXDelta,
    (float)y - nYDelta,
    (float)label_param.LabelWidth,
    (float)label_param.LabelHeight);

                        if (rectLabel.Left > e.Graphics.ClipBounds.Right)
                        {
                            // Debug.WriteLine("break label loop at i=" + i.ToString() + " j=" + j.ToString());
                            // 当前标签在剪裁区域的右方，可以不要显示后面的标签了
                            bDisplay = false;
                        }

                        if (this.IsDesignMode == false
                            || rectLabel.IntersectsWith(e.Graphics.ClipBounds) == true)
                        {
                            // Debug.WriteLine("i="+i.ToString()+" j="+j.ToString()+" rectLabel = "+rectLabel.ToString()+", clipbounds " + e.Graphics.ClipBounds.ToString());
                            // 标签内容区域
                            RectangleF rectContent = new RectangleF(
                                    (float)x + (float)label_param.LabelPaddings.Left - nXDelta,
                                    (float)y + (float)label_param.LabelPaddings.Top - nYDelta,
                                    (float)label_param.LabelWidth - (float)label_param.LabelPaddings.Left - (float)label_param.LabelPaddings.Right - 1,
                                    (float)label_param.LabelHeight - (float)label_param.LabelPaddings.Top - (float)label_param.LabelPaddings.Bottom - 1);


                            // 绘制标签边界
                            // 灰色
                            if (bTestingGrid == true)
                            {
                                // 标签白色背景
                                if (this.IsDesignMode == true)
                                {
                                    using (Brush brushBack = new SolidBrush(Color.FromArgb(200, Color.White)))
                                    {
                                        e.Graphics.FillRectangle(brushBack, rectLabel);
                                    }
                                }

                                // 标签边界
                                using (Pen pen = new Pen(Color.FromArgb(200, 200, 200), this.IsDesignMode ? (float)0.5 : (float)1))
                                {
                                    e.Graphics.DrawRectangle(pen,
                                        rectLabel.X,
                                        rectLabel.Y,
                                        rectLabel.Width,
                                        rectLabel.Height);
                                }


                                // 绘制标签内部文字区域边界
                                // 淡红色

                                using (Pen pen = new Pen(Color.FromArgb(255, 200, 200), this.IsDesignMode ? (float)0.5 : (float)1))
                                {
                                    e.Graphics.DrawRectangle(pen,
                                        rectContent.X,
                                        rectContent.Y,
                                        rectContent.Width,
                                        rectContent.Height);

                                }
                            }

                            // 绘制一个标签的全部文字
                            PaintLabelContent(e.Graphics,
                                rectContent,
                                lines,
                                label_param,
                                x,
                                y,
                                nXDelta,
                                nYDelta,
                                bTestingGrid);

                        } // end if IntersectsWith



                    } // end if bOutput == true


                    x += (float)label_param.LabelWidth;
                }

                //CONTINUE_LINE:
                y += (float)label_param.LabelHeight;
            }

            // If more lines exist, print another page.
            if (bEOF == false)
            {
                if (e.PageSettings.PrinterSettings.PrintRange == PrintRange.SomePages)
                {
                    if (this.m_nPageNo >= to)
                    {
                        e.HasMorePages = false;
                        return;
                    }
                }
            }
            else
            {
                e.HasMorePages = false;
                return;
            }

            this.m_nPageNo++;
            e.HasMorePages = true;
            return;
        ERROR1:
            MessageBox.Show(owner, strError);
        }

        // 绘制一个标签的全部文字
        // parameters:
        //      rectContent 整个文字区域的矩形
        void PaintLabelContent(Graphics g,
            RectangleF rectContent,
            List<string> lines,
            LabelParam label_param,
            float x,
            float y,
            float nXDelta,
            float nYDelta,
            bool bTestingGrid)
        {
            using (Region old_clip = g.Clip)
            {
                g.IntersectClip(rectContent);

                float y0 = 0;
                for (int k = 0; k < lines.Count; k++)
                {
                    string strText = lines[k];

                    LineFormat format = null;
                    if (k < label_param.LineFormats.Count)
                        format = label_param.LineFormats[k];

                    Font this_font = null;
                    bool bIsBarcodeFont = false;
                    if (format != null && format.Font != null)
                    {
                        this_font = format.Font;
                        bIsBarcodeFont = format.IsBarcodeFont;
                    }
                    else
                    {
                        this_font = label_param.Font;
                        bIsBarcodeFont = label_param.IsBarcodeFont;
                    }

                    //if (bIsBarcodeFont == true && string.IsNullOrEmpty(strText) == false)
                    //    strText = "*" + strText + "*";
                    Font save_font = this_font;

                    if (this.DesignMode)
                        this_font = ReCreateFont(this_font, g);
                    try
                    {
                        float nLineHeight = this_font.GetHeight(g);

                        RectangleF rect = new RectangleF((float)x + (float)label_param.LabelPaddings.Left - nXDelta,
                            (float)y + (float)label_param.LabelPaddings.Top + y0 - nYDelta,
                            (float)label_param.LabelWidth - (float)label_param.LabelPaddings.Left - (float)label_param.LabelPaddings.Right,
                            nLineHeight);

                        bool bAbsLocation = false;
                        // 行格式的 start 和 offset
                        if (format != null)
                        {
                            if (double.IsNaN(format.StartX) == false)
                                rect.X = (float)format.StartX;
                            if (double.IsNaN(format.StartY) == false)
                            {
                                rect.Y = (float)format.StartY;
                                bAbsLocation = true;    // Y 绝对定位后，行高度不参与累计
                            }
                            rect.Offset((float)format.OffsetX, (float)format.OffsetY);

                            y0 += (float)format.OffsetY;    // Y 偏移后，累计值也跟着调整

                            if (double.IsNaN(format.Width) == false)
                                rect.Width = (float)format.Width;
                            if (double.IsNaN(format.Height) == false)
                                rect.Height = (float)format.Height;
                        }

                        StringFormat s_format = new StringFormat();
                        if (format != null)
                        {
                            if (format.Align == "right")
                                s_format.Alignment = StringAlignment.Far;
                            else if (format.Align == "center")
                                s_format.Alignment = StringAlignment.Center;
                            else
                                s_format.Alignment = StringAlignment.Near;

                            s_format.Trimming = StringTrimming.EllipsisCharacter;
                            // s_format.LineAlignment = StringAlignment.Center;
                        }

                        if (format != null && string.IsNullOrEmpty(format.BackColor) == false)
                        {
                            using (Brush brush = new SolidBrush(GetColor(format.BackColor)))
                            {
                                g.FillRectangle(brush, rect);
                            }
                        }

                        if (bIsBarcodeFont)
                        {
                            // strText = strText.Trim(new char[] { '*' });

                            // 应该是 1/100 inch 单位
                            float textHeight = MeasureOcrTextHeight(
        g,
        rect,
        strText);
                            RectangleF target = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height - textHeight);

                            GraphicsUnit u = g.PageUnit;
                            Hashtable param = new Hashtable();
                            param["type"] = "39";
                            param["code"] = strText;
                            param["width"] = ((int)(target.Width * 2)).ToString();
                            param["height"] = ((int)(target.Height * 2)).ToString();
                            param["margin"] = "0";
                            using (Image image = BuildQrCodeImage(param))
                            {
                                RectangleF source = new RectangleF(0, 0, image.Width, image.Height);
                                g.DrawImage(image, target, source, GraphicsUnit.Pixel);
                            }

                            RectangleF rectText = new RectangleF(rect.X,
                                rect.Y + target.Height,
                                rect.Width,
                                textHeight);
                            PaintOcrFont(
                                g,
                                rectText,
                                strText);
                        }
                        else
                        {
                            Brush brushText = null;
                            try
                            {
                                if (format != null && string.IsNullOrEmpty(format.ForeColor) == false)
                                {
                                    brushText = new SolidBrush(GetColor(format.ForeColor));
                                }
                                else
                                    brushText = System.Drawing.Brushes.Black;

                                g.DrawString(strText,
                                    this_font,
                                    brushText,
                                    rect,
                                    s_format);
                            }
                            finally
                            {
                                if (brushText != System.Drawing.Brushes.Black)
                                    brushText.Dispose();
                            }
                        }



                        // 文字行区域边界
                        // 黑色点
                        if (bTestingGrid == true && label_param.LineSep > 0)
                        {
                            using (Pen pen = new Pen(Color.Black, (float)1))
                            {
                                pen.DashPattern = new float[] { 1F, 3F, 1F, 3F };
                                g.DrawLine(pen,
                                    new PointF(rect.Left, rect.Top),
                                    new PointF(rect.Right, rect.Top)
                                    );
                                g.DrawLine(pen,
                                    new PointF(rect.Left + 2, rect.Bottom),
                                    new PointF(rect.Right, rect.Bottom)
                                    );
                            }
                        }

                        if (bAbsLocation == false)
                            y0 += // nLineHeight
                                rect.Height + (float)label_param.LineSep;

                    }
                    finally
                    {
                        if (this_font != save_font)
                            this_font.Dispose();
                    }
                }

                g.Clip = old_clip;
            } // end of using clip

        }

        Font ReCreateFont(Font font, Graphics g)
        {
            if (g.DpiY != 96)
            {
                float height = font.SizeInPoints / 72 * 96;
                return new Font(font.FontFamily, height, font.Style, GraphicsUnit.Pixel);
            }

            return font;
        }
#if NO
        static PointF GetPixelPoint(Graphics g, PointF point)
        {
            return new PointF(point.X * g.DpiX / 100,
    point.Y * g.DpiY / 100);
        }

        static RectangleF GetPixelRect(Graphics g, RectangleF rect)
        {
            return new RectangleF(rect.X * g.DpiX / 100,
                rect.Y * g.DpiY / 100,
                rect.Width * g.DpiX / 100,
                rect.Height * g.DpiY / 100);
        }

        static float GetPixelX(Graphics g, float display)
        {
            // display 值为 1/100 英寸
            return (display * g.DpiX) / 100;
        }

        static float GetPixelY(Graphics g, float display)
        {
            // display 值为 1/100 英寸
            return (display * g.DpiY) / 100;
        }

        static double GetPixelX(Graphics g, double display)
        {
            // display 值为 1/100 英寸
            return (display * g.DpiX) / 100;
        }

        static double GetPixelY(Graphics g, double display)
        {
            // display 值为 1/100 英寸
            return (display * g.DpiY) / 100;
        }
#endif

        static float MeasureOcrTextHeight(
    Graphics g,
    RectangleF rect,
    string strText)
        {
            float height = rect.Height / 3; // 预计的文字高度
            for (; ; )
            {
                float ratio = 0.33F;
                using (Font font = Global.BuildFont(
                    "OCR-B 10 BT",
                    height,
                    GraphicsUnit.Pixel))
                {
                    SizeF size = g.MeasureString(strText, font);
                    if (size.Width <= rect.Width)
                        return height;
                    ratio = ((size.Width - rect.Width) / rect.Width) + 0.04F; // 超过的百分比
                }

                if (height <= 1)
                    return Math.Max(1, height);

                // 减小 height
                // 每次减小 1/3
                height -= height * ratio;
            }
        }

        static void PaintOcrFont(
            Graphics g,
            RectangleF rect,
            string strText)
        {
            using (Font font = Global.BuildFont(
                "OCR-B 10 BT",
                rect.Height,
                GraphicsUnit.Pixel))
            {
                StringFormat s_format = new StringFormat();
                s_format.Alignment = StringAlignment.Center;
                Brush brushText = System.Drawing.Brushes.Black;

                g.DrawString(strText,
                    font,
                    brushText,
                    rect,
                    s_format);
            }
        }

        // parameters:
        //      strType 39 / 空 / 
        static Image BuildQrCodeImage(Hashtable param_table)
        {
            // Hashtable param_table = StringUtil.ParseParameters(path, ',', '=', "url");
            string strType = (string)param_table["type"];
            string strCode = (string)param_table["code"];
            string strWidth = (string)param_table["width"];
            string strHeight = (string)param_table["height"];
            string strMargin = (string)param_table["margin"];

            int nWidth = 200;
            int nHeight = 200;
            int nMargin = 10;

            if (string.IsNullOrEmpty(strWidth) == false)
                Int32.TryParse(strWidth, out nWidth);
            if (string.IsNullOrEmpty(strHeight) == false)
                Int32.TryParse(strHeight, out nHeight);
            if (string.IsNullOrEmpty(strMargin) == false)
                Int32.TryParse(strMargin, out nMargin);


            string strCharset = "ISO-8859-1";
            bool bDisableECI = false;

            BarcodeFormat format = BarcodeFormat.QR_CODE;
            if (strType == "39" || strType == "code_39")
            {
                format = BarcodeFormat.CODE_39;
                strCode = strCode.ToUpper();    // 小写字符会无法编码
            }
            else if (strType == "ean_13")
            {
                format = BarcodeFormat.EAN_13;
                strCode = strCode.ToUpper();
            }

            EncodingOptions options = new QrCodeEncodingOptions
            {
                Height = nWidth,    // 400,
                Width = nHeight,    // 400,
                DisableECI = bDisableECI,
                ErrorCorrection = ErrorCorrectionLevel.L,
                CharacterSet = strCharset // "UTF-8"
            };

            if (strType == "39" || strType == "code_39"
                || strType == "ean_13")
                options = new EncodingOptions
                {
                    Width = nWidth, // 500,
                    Height = nHeight,   // 100,
                    Margin = nMargin,
                    PureBarcode = true
                };

            var writer = new BarcodeWriter
            {
                // Format = BarcodeFormat.QR_CODE,
                Format = format,
                // Options = new EncodingOptions
                Options = options
            };

            try
            {
                return writer.Write(strCode);
            }
            catch (Exception ex)
            {
#if NO
                Stream result = BuildTextImage("异常: " + ex.Message, Color.FromArgb(255, Color.DarkRed));
                result.Seek(0, SeekOrigin.Begin);
                return result;
#endif
                return null;
            }
        }

#if NO
        static Stream BuildTextImage(string strText,
Color color,
int nWidth = 400)
        {
            // 文字图片
            return ArtText.BuildArtText(
                strText,
                "Consolas", // "Microsoft YaHei",
                (float)16,
                FontStyle.Bold,
                color,
                Color.Transparent,
                Color.Gray,
                ArtEffect.None,
                ImageFormat.Png,
                nWidth);
        }
#endif

        public static Color GetColor(string strColorString)
        {
            // Create the ColorConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

            return (Color)converter.ConvertFromString(strColorString);
        }

        public static string GetColorString(Color color)
        {
            // Create the ColorConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

            return converter.ConvertToString(color);
        }
    }
}

