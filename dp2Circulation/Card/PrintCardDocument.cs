using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Diagnostics;
using System.Xml;

using System.Windows.Forms;

using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    internal class PrintCardDocument : PrintDocument
    {
        public bool Stop = false;

        /// <summary>
        /// 设置进度的事件
        /// </summary>
        public event SetProgressEventHandler SetProgress = null;

        public string XmlFilename = "";
        Stream m_file = null;
        XmlTextReader m_reader = null;
        bool EOF = false;

        int m_nPageNo = 0;  // 0表示没有初始化
        List<Page> m_pages = new List<Page>();

        PageSetting m_pagesetting = null;

        public int Open(string strInputFileName,
    out string strError)
        {
            strError = "";

            try
            {
                m_file = File.Open(strInputFileName,
                    FileMode.Open,
                    FileAccess.Read);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + strInputFileName + " 失败: " + ex.Message;
                return -1;
            }

            if (this.SetProgress != null)
            {
                SetProgressEventArgs e = new SetProgressEventArgs();
                e.Start = 0;
                e.End = m_file.Length;
                e.Value = -1;
                this.SetProgress(this, e);
            }

            this.XmlFilename = strInputFileName;

            this.m_reader = new XmlTextReader(m_file);

            while (true)
            {
                bool bRet = m_reader.Read();
                if (bRet == false)
                {
                    strError = "没有根元素";
                    return -1;
                }
                if (m_reader.NodeType == XmlNodeType.Element)
                    break;
            }

            string strPageSettingXml = "";
            // return:
            //      -1  error
            //      0   normal
            //      1   reach file end。strXml中无内容
            int nRet = GetPageSetting(
            out strPageSettingXml,
            out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                this.m_pagesetting = new PageSetting();
                nRet = this.m_pagesetting.Build(strPageSettingXml,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                this.m_pagesetting = null;  // 表示XML文件中没有定义
            }

            /*
            m_reader.Close();
            m_file = File.Open(strInputFileName,
    FileMode.Open,
    FileAccess.Read);
             * */


            m_file.Position = 0;
            this.m_reader = new XmlTextReader(m_file);

            this.m_nPageNo = 0;
            this.m_pages.Clear();
            this.EOF = false;

            return 0;
        }

        public void Close()
        {
            if (m_file != null)
            {
                m_file.Close();
                m_file = null;
            }

            this.m_reader = null;
        }

        // return:
        //      -1  error
        //      0   normal
        //      1   reach file end。strXml中无内容
        public int GetOneDocument(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (this.Stop == true)
            {
                strError = "用户中断";
                return -1;
            }

            try
            {

                bool bRet = false;
                while (m_reader.Name != "document")
                {
                // REDO:
                    while (true)
                    {
                        bRet = m_reader.Read();
                        if (bRet == false)
                            return 1;
                        if (m_reader.NodeType == XmlNodeType.Element)
                            break;

                    }

                    if (bRet == false)
                        return 1;	// 结束

                    /*
                    if (m_reader.Name != "document")
                        goto REDO;
                     * */
                }

                strXml = m_reader.ReadOuterXml();

                if (this.SetProgress != null)
                {
                    SetProgressEventArgs e = new SetProgressEventArgs();
                    e.Start = 0;
                    e.End = m_file.Length;
                    e.Value = m_file.Position;
                    // Debug.WriteLine(m_file.Position.ToString());

                    this.SetProgress(this, e);
                }
                return 0;
            }
            catch (Exception ex)
            {
                strError = "读取XML文件 '" + this.XmlFilename + "' 时发生错误: " + ex.Message;
                return -1;
            }
        }


        // return:
        //      -1  error
        //      0   normal
        //      1   reach file end。strXml中无内容
        public int GetPageSetting(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            bool bRet = false;

            REDO:
            while (true)
            {
                bRet = m_reader.Read();
                if (bRet == false)
                    return 1;
                if (m_reader.NodeType == XmlNodeType.Element)
                    break;
            }

            if (bRet == false)
                return 1;	// 结束

            if (m_reader.Name != "pageSetting")
                goto REDO;
            try
            {
                strXml = m_reader.ReadOuterXml();
            }
            catch (Exception ex)
            {
                strError = "读取XML内容时出错: " + ex.Message;
                return -1;
            }

            return 0;
        }

        internal void DoPrintPage(
    IWin32Window owner,
    string strStyle,
    PrintPageEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (e.Cancel == true)
                return;

            // 如果XML文件中没有页面参数，则采用当前实际的
            if (this.m_pagesetting == null)
            {
                this.m_pagesetting = new PageSetting();
                this.m_pagesetting.Width = e.PageBounds.Width;
                this.m_pagesetting.Height = e.PageBounds.Height;
            }
            else
            {
                if (this.m_pagesetting.Width == 0)
                    this.m_pagesetting.Width = e.PageBounds.Width;
                if (this.m_pagesetting.Height == 0)
                    this.m_pagesetting.Height = e.PageBounds.Height;

            }

            bool bTestingGrid = false;
            if (StringUtil.IsInList("TestingGrid", strStyle) == true)
                bTestingGrid = true;

            int nYCount = 0;
            int nXCount = 0;

            // 垂直方向的个数
            nYCount = (int)Math.Floor((float)e.PageBounds.Height
                / this.m_pagesetting.Height);

            // 2012/4/24
            if (nYCount == 0)
                nYCount = 1;

            // 水平方向的个数
            nXCount = (int)Math.Floor((float)e.PageBounds.Width
            / this.m_pagesetting.Width);

            // 2012/4/24
            if (nXCount == 0)
                nXCount = 1;

            int from = 0;
            int to = 0;
            bool bOutput = true;
            if (e.PageSettings.PrinterSettings.PrintRange == PrintRange.SomePages)
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

                    Debug.Assert(this.m_nPageNo >= 1, "this.m_nPageNo >= 1 不满足");
                    long nLabelCount = (nXCount * nYCount) * (this.m_nPageNo - 1);

                    // 
                    if (this.EOF == false)
                    {
                        // parameters:
                        //      nCount  希望最少获得的page对象数
                        // return:
                        //      0   普通
                        //      1   文件已经到达末尾，后面再也没有任何<document>了
                        nRet = GetPages(
                            e.Graphics,
                            (int)nLabelCount,
                            true,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                        {
                            this.EOF = true;
                            if (this.m_pages.Count <= nLabelCount)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        for (int i = 0; i < nLabelCount; i++)
                        {
                            this.m_pages.RemoveAt(0);
                        }
                    }
                }

            }
            else
            {
                if (this.m_nPageNo == 0)
                    this.m_nPageNo = 1; // 一般性的初始化
            }


            // 加快运行速度
            float nXDelta = e.PageSettings.PrintableArea.Left;
            float nYDelta = e.PageSettings.PrintableArea.Top;


            if (this.OriginAtMargins == false)
            {
                nXDelta = 0;
                nYDelta = 0;
            }

            float nPrintableWidth = e.PageSettings.PrintableArea.Width;
            float nPrintableHeight = e.PageSettings.PrintableArea.Height;


            // 绘制可打印区域
            // 黄色
            if (bTestingGrid == true && bOutput == true)
            {
                float nXOffs = 0;
                float nYOffs = 0;

                // 如果为正式打印，左上角(0,0)已经就是可以打印区域的左上角
                // 如果为preview模式，则左上角要向右向下移动，才能模拟出显示效果

                if (this.OriginAtMargins == false)
                {
                    nXOffs = e.PageSettings.PrintableArea.Left;
                    nYOffs = e.PageSettings.PrintableArea.Top;
                }

                using (Pen pen = new Pen(Color.Green, (float)1))
                {
                    DrawFourAngel(
                        e.Graphics,
                        pen,
                        nXOffs,
                        nYOffs,
                        nPrintableWidth,
                        nPrintableHeight,
                        50);    // 半英寸
                }
            }

            // 绘制内容区域边界(也就是排除了页面边空的中间部分)
            // 绿色
            if (bTestingGrid == true && bOutput == true)
            {
                using (Pen pen = new Pen(Color.Green, (float)3))
                {

                    /*
                    e.Graphics.DrawRectangle(pen,
                        label_param.PageMargins.Left - nXDelta,
                        label_param.PageMargins.Top - nYDelta,
                        e.PageBounds.Width - label_param.PageMargins.Left - label_param.PageMargins.Right,
                        e.PageBounds.Height - label_param.PageMargins.Top - label_param.PageMargins.Bottom);
                    */

                }
            }

            // bool bEOF = false;

#if NO
            if (this.m_pages.Count == 0)
            {

                string strXml = "";
                // TODO: 如何提前判断这是最后一个document
                // return:
                //      -1  error
                //      0   normal
                //      1   reach file end
                nRet = GetOneDocument(
    out strXml,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    bEOF = true;
                    goto END1;
                }
                else
                {
                    List<Page> temp_pages = null;
                    nRet = BuildPages(
                        e.Graphics,
                        strXml,
                            this.m_pagesetting.Width,
                            this.m_pagesetting.Height,
                        out temp_pages,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    this.m_pages.AddRange(temp_pages);
                }
            }
#endif

            if (this.EOF == false)
            {
                // parameters:
                //      nCount  希望最少获得的page对象数
                // return:
                //      0   普通
                //      1   文件已经到达末尾，后面再也没有任何<document>了
                nRet = GetPages(
                    e.Graphics,
                    (nXCount * nYCount) + 1,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    this.EOF = true;
            }

            Debug.Assert(this.m_pages.Count > 0, "this.m_pages.Count > 0 不满足");

            float y = 0;
            for (int i = 0; i < nYCount; i++)
            {
                float x = 0;
                for (int j = 0; j < nXCount; j++)
                {
                    if (this.m_pages.Count == 0)
                        goto END1;
                    Page current_page = this.m_pages[0];
                    this.m_pages.RemoveAt(0);

                    if (bOutput == true)
                    {
                        nRet = DoPrintPage(e.Graphics,
                            x,
                            y,
current_page,
out strError);
                        if (nRet == -1)
                            goto ERROR1;                       

                        // 绘制标签边界
                        // 黑色
                        if (bTestingGrid == true)
                        {
                            using (Pen pen = new Pen(Color.Black, (float)1))
                            {
                                e.Graphics.DrawRectangle(pen,
                                    x - nXDelta,
                                    y - nYDelta,
                                    this.m_pagesetting.Width,
                                    this.m_pagesetting.Height);
                            }
                        }
                    } // end if bOutput == true

                    x += this.m_pagesetting.Width;
                }

                y += this.m_pagesetting.Height;
            }

#if NO

            Page current_page = this.m_pages[0];
            this.m_pages.RemoveAt(0);


            nRet = PrintPage(e.Graphics,
            current_page,
            out strError);
            if (nRet == -1)
                goto ERROR1;
            // float y = label_param.PageMargins.Top;
#endif


            END1:
            // If more lines exist, print another page.
            if (this.EOF == true && this.m_pages.Count == 0)
            {
                e.HasMorePages = false;
                return;
            }
            else
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

            this.m_nPageNo++;
            e.HasMorePages = true;
            return;
        ERROR1:
            MessageBox.Show(owner, strError);
        }

        // parameters:
        //      nCount  希望最少获得的page对象数
        //      bClear  是否清除前面nCount个m_pages数组成员
        // return:
        //      0   普通
        //      1   文件已经到达末尾，后面再也没有任何<document>了
        int GetPages(
            Graphics g,
            int nCount,
            bool bClear,
            out string strError)
        {
            strError = "";

            if (this.m_pages.Count >= nCount)
                return 0;

            while (this.m_pages.Count < nCount)
            {
                string strXml = "";
                // TODO: 如何提前判断这是最后一个document
                // return:
                //      -1  error
                //      0   normal
                //      1   reach file end
                int nRet = GetOneDocument(
                    out strXml,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    return 1;
                }

                Debug.Assert(nRet == 0, "GetOneDocument() 返回值 '"+nRet.ToString()+"' 超出预计范围");

                List<Page> temp_pages = null;
                nRet = BuildPages(
                    g,
                    strXml,
                    this.m_pagesetting.Width,
                    this.m_pagesetting.Height,
                    out temp_pages,
                    out strError);
                if (nRet == -1)
                    return -1;
                int nStart = this.m_pages.Count;
                this.m_pages.AddRange(temp_pages);

                // 防止内存膨胀
                if (bClear == true)
                {
                    for (int i = nStart; i < Math.Min(this.m_pages.Count, nCount); i++)
                    {
                        this.m_pages[i] = null;
                    }
                }
            }

            return 0;
        }

        int DoPrintPage(Graphics g,
            float x0,
            float y0,
            Page page,
            out string strError)
        {
            strError = "";

            /*
            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Far;
             * */

            for (int i = 0; i < page.Lines.Count; i++)
            {
                PrintLine line = page.Lines[i];
                Debug.Assert(string.IsNullOrEmpty(line.FontDef) == false, "string.IsNullOrEmpty(line.FontDef) == false 不满足");

                Font font = Global.BuildFont(line.FontDef);
                Color color = GetColor(line.ColorDef);

                try
                {
                    float x = line.X + x0 + line.Indent;
                    float y = line.Y + y0;

                    float per_sep = 0;

                    if (line.HorzAlign == HorzAlign.Right
                        && line.Boxes.Count > 0)
                    {
                        float right_blank = line.Width - line.Indent - GetLineWidth(line);
                        if (right_blank > 0)
                            x += right_blank;
                    }
                    else if (line.HorzAlign == HorzAlign.Center
                        && line.Boxes.Count > 0)
                    {
                        float right_blank = line.Width - line.Indent - GetLineWidth(line);
                        // x += line.Indent;   // - 2012/4/23 change
                        x += right_blank/2;
                    }
                    else if (line.HorzAlign == HorzAlign.LeftRight
                        && line.Boxes.Count > 1
                        && line.IsParagraphTail == false)
                    {
                        float right_blank = line.Width - line.Indent - GetLineWidth(line);
                        if (right_blank > 0)
                            per_sep = right_blank / ( line.Boxes.Count - 1);
                    }

                    for (int j = 0; j < line.Boxes.Count; j++)
                    {
                        Box box = line.Boxes[j];

                        //  如果中间有字体变化
                        string strFontString = box.FontDef;
                        if (String.IsNullOrEmpty(strFontString) == false)
                        {
                            font.Dispose();
                            font = Global.BuildFont(strFontString);
                        }

                        //  如果中间有颜色变化
                        string strColorString = box.ColorDef;
                        if (String.IsNullOrEmpty(strColorString) == false)
                        {
                            color = GetColor(strColorString);
                        }

                        using (Brush brush = new SolidBrush(color))
                        {
                            g.DrawString(box.Text,
                                font,
                                brush,
                                new PointF(x, y + (line.Height * line.BaseRatio) - box.Base));
                        }
                        x += box.Width + box.LeftBlank + per_sep;
                    }
                }
                finally
                {
                    font.Dispose();
                }
            }

            return 0;
        }

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
        // parameters:
        //      width   页面的宽度
        //      height  页面的高度
        int BuildPages(
            Graphics g,
            string strXml,
            float width,
            float height,
            out List<Page> pages,
            out string strError)
        {
            strError = "";
            pages = new List<Page>();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            int nRet = 0;

            Padding padding = new Padding(0,0,0,0);
            string strPadding = DomUtil.GetAttr(dom.DocumentElement,
                "padding");
            if (String.IsNullOrEmpty(strPadding) == false)
            {
                nRet = GetPadding(strPadding,
                out padding,
                out strError);
                if (nRet == -1)
                {
                    strError = "元素<"+dom.DocumentElement.Name+">中padding属性值 '"+strPadding+"' 格式错误: " + strError;
                    return -1;
                }
            }

            // 计算出中间区域的尺寸。排除了header和footer
            float headerHeight = 0;
            float footerHeight = 0;
            XmlNode nodeHeader = dom.DocumentElement.SelectSingleNode("header");
            if (nodeHeader != null)
                headerHeight = (float)Convert.ToDouble(DomUtil.GetAttr(nodeHeader, "height"));
            XmlNode nodeFooter = dom.DocumentElement.SelectSingleNode("footer");
            if (nodeFooter != null)
                footerHeight = (float)Convert.ToDouble(DomUtil.GetAttr(nodeFooter, "height"));

            nRet = DoColumns(g,
                dom.DocumentElement,
                padding.Left,
                headerHeight + padding.Top,
                width - padding.Horizontal,
                height - headerHeight - footerHeight - padding.Vertical,
                false,
                null,
                "",
                ref pages,
                out strError);
            if (nRet == -1)
                return -1;

            // 处理所有valign=bottom的段落

            // 在确知有多少页以后，再处理header和footer
            if (nodeHeader != null)
            {
                nRet = DoHeader(
                    g,
                    nodeHeader,
                    padding.Left,
                    padding.Top,
                    width - padding.Horizontal,
                    headerHeight,
                    ref pages,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (nodeFooter != null)
            {
                nRet = DoFooter(
                    g,
                    nodeFooter,
                    padding.Left,
                    height - padding.Bottom - footerHeight,
                    width - padding.Horizontal,
                    footerHeight,
                    ref pages,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        static int GetPadding(string strPadding,
            out Padding padding,
            out string strError)
        {
            strError = "";
            int nLeft = 0;
            int nTop = 0;
            int nRight = 0;
            int nBottom = 0;

            string[] parts = strPadding.Split(new char[] { ',' });
            if (parts.Length != 4)
            {
                strError = "格式错误。应当为4个逗号间隔的数字";
                padding = new Padding(0, 0, 0, 0);
                return -1;
            }
            if (parts.Length > 0)
                nLeft = Convert.ToInt32(parts[0]);
            if (parts.Length > 1)
                nTop = Convert.ToInt32(parts[1]);
            if (parts.Length > 2)
                nRight = Convert.ToInt32(parts[2]);
            if (parts.Length > 3)
                nBottom = Convert.ToInt32(parts[3]);

            padding = new Padding(nLeft, nTop, nRight, nBottom);
            return 0;
        }

        int DoHeader(
            Graphics g,
            XmlNode nodeHeader,
            float x,
            float y,
            float width,
            float height,
            ref List<Page> pages,
            out string strError)
        {
            strError = "";

            // 
            string strStyle = DomUtil.GetAttr(nodeHeader, "style");

            if (StringUtil.IsInList("hidewhenonepage", strStyle) == true
                && pages.Count == 1)
                return 0;

            // 准备宏值表  %pageno% %pagecount%
            Hashtable macro_table = new Hashtable();
            macro_table["%pagecount%"] = pages.Count.ToString();


            for (int i = 0; i < pages.Count; i++)
            {
                Page page = pages[i];
                List<Page> temp_pages = new List<Page>();
                temp_pages.Add(page);


                macro_table["%pageno%"] = (i + 1).ToString();

                string strHeaderCondition = "";
                if (pages.Count == 1)
                    strHeaderCondition += ",onlyonepage";
                if (i == 0)
                    strHeaderCondition += ",firstpage";
                if (i == pages.Count - 1)
                    strHeaderCondition += ",tailpage";

                int nRet = DoColumns(g,
        nodeHeader,
        x,
        y,
        width,
        height,
        true,
        macro_table,
        strHeaderCondition,
        ref temp_pages,
        out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        int DoFooter(
                        Graphics g,
XmlNode nodeHeader,
float x,
float y,
float width,
float height,
ref List<Page> pages,
out string strError)
        {
            strError = "";

            // 
            string strStyle = DomUtil.GetAttr(nodeHeader, "style");

            if (StringUtil.IsInList("hidewhenonepage", strStyle) == true
                && pages.Count == 1)
                return 0;

            // 准备宏值表  %pageno% %pagecount%
            Hashtable macro_table = new Hashtable();
            macro_table["%pagecount%"] = pages.Count.ToString();

            for (int i = 0; i < pages.Count; i++)
            {
                Page page = pages[i];
                List<Page> temp_pages = new List<Page>();
                temp_pages.Add(page);


                macro_table["%pageno%"] = (i + 1).ToString();



                string strHeaderCondition = "";
                if (pages.Count == 1)
                    strHeaderCondition += ",onlyonepage";
                if (i == 0)
                    strHeaderCondition += ",firstpage";
                if (i == pages.Count - 1)
                    strHeaderCondition += ",tailpage";

                int nRet = DoColumns(g,
        nodeHeader,
        x,
        y,
        width,
        height,
        true,
        macro_table,
        strHeaderCondition,
        ref temp_pages,
        out strError);
                if (nRet == -1)
                    return -1;
            } 
            return 0;
        }

        // parameters:
        //      height  分页以前的单页允许高度
        //      bIsHeaderFooter 是否保持不增加page
        int DoColumns(
            Graphics g,
            XmlNode nodeDocument,
            float x,
            float y,
            float width,
            float height,
            bool bIsHeaderFooter,
            Hashtable macro_table,
            string strHeaderCondition,
            ref List<Page> pages,
            out string strError)
        {
            strError = "";

            List<RectParam> rects = new List<RectParam>();

            // 列出根下面的<Column>元素
            XmlNodeList nodes = nodeDocument.SelectNodes("column");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];



                // 获得矩形参数
                RectParam rect = new RectParam();
                rect.WidthString = DomUtil.GetAttr(node, "width");
                rects.Add(rect);

            }

            // 分配宽度。固定宽度剩下以后的，就是auto的平分
            float rest = width;    // 剩下的宽度
            // 1) 处理数字宽度
            int nAutoCount = 0;
            for (int i = 0; i < rects.Count; i++)
            {
                RectParam rect = rects[i];
                string strWidth = rect.WidthString;
                if (StringUtil.IsPureNumber(strWidth) == true)
                {
                    rect.Width = (float)Convert.ToDouble(strWidth);
                    rest -= rect.Width;
                }
                else
                {
                    if (string.Compare(strWidth, "auto", true) != 0)
                    {
                        strError = "<column>元素的width属性值 '"+strWidth+"' 格式错误";
                        return -1;
                    }
                    rect.Width = -1;
                    nAutoCount++;
                }
            }

            // 2) 处理auto宽度
            if (nAutoCount > 0)
            {
                float nAverWidth = rest / nAutoCount;
                if (nAverWidth < 0)
                    nAverWidth = 0;
                for (int i = 0; i < rects.Count; i++)
                {
                    RectParam rect = rects[i];
                    if (rect.Width == -1)
                        rect.Width = nAverWidth;
                }
            }

            // 3)填充 X Y Height
            float start_x = x;
            for (int i = 0; i < rects.Count; i++)
            {
                RectParam rect = rects[i];
                rect.X = start_x;
                start_x += rect.Width;

                rect.Y = y;
                rect.Height = height;   // 一页内最大高度
            }

            List<RectGroup> rect_groups = new List<RectGroup>();

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                if (bIsHeaderFooter == true)
                {
                    string strStyle = DomUtil.GetAttr(node, "style");
                    if (StringUtil.IsInList("hidewhenonepage", strStyle) == true
    && StringUtil.IsInList("onlyonepage", strHeaderCondition) == true)
                        continue;
                    if (StringUtil.IsInList("hidewhenfirstpage", strStyle) == true
&& StringUtil.IsInList("firstpage", strHeaderCondition) == true)
                        continue;
                    if (StringUtil.IsInList("hidewhentailpage", strStyle) == true
&& StringUtil.IsInList("tailpage", strHeaderCondition) == true)
                        continue;
                }

                // 获得矩形参数
                RectParam rect = rects[i];

                RectGroup rect_group = new RectGroup();
                rect_groups.Add(rect_group);

                int nRet = DoRect(
                    g,
                    node,
                    rect,
                    bIsHeaderFooter,
                    macro_table,
                    rect_group,
                    ref pages,
                    out strError);
                if (nRet == -1)
                    return -1;
            }


            if (bIsHeaderFooter == false)
            {
                // 处理valign=bottom的<p>
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    // 获得矩形参数
                    RectParam rect = rects[i];

                    RectGroup rect_group = rect_groups[i];
                    for (int j = rect_group.Paragraphs.Count - 1; j >= 0; j--)
                    {
                        ParagraphGroup p_group = rect_group.Paragraphs[j];
                        if (p_group.Format.VertAlign == "bottom")
                        {
                            ResetLines(pages,
    rect,
    p_group.Lines);

                        }
                        else
                            break;  // 只要中间不连续，就中断
                    }
                }
            }

            return 0;
        }

        void ResetLines(List<Page> pages,
            RectParam rect_def,
            List<PrintLine> lines)
        {
            int iPage = pages.Count - 1;
            Page current_page = pages[iPage];

            float current_height = rect_def.Height;
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                PrintLine line = lines[i];

                if (current_height - line.Height < 0)
                {
                    // 倒退一页
                    current_height = rect_def.Height;
                    iPage--;
                    if (iPage >= 0)
                        current_page = pages[iPage];
                    else
                    {
                        break;
                    }

                }

                // 把line从pages中原来位置移走
                foreach (Page page in pages)
                {
                    if (page.Lines.IndexOf(line) != -1)
                    {
                        page.Lines.Remove(line);
                        break;
                    }
                }

                line.X = rect_def.X + rect_def.Padding.Left;
                line.Y = rect_def.Y + current_height - rect_def.Padding.Bottom -line.Height ;
                current_page.Lines.Add(line);

                current_height -= line.Height;
            }

        }

        // 处理一个矩形区域
        int DoRect(
            Graphics g,
            XmlNode nodeContainer,
            RectParam rect_def,
            bool bRemainPage,
            Hashtable macro_table,
            RectGroup rect_group,
            ref List<Page> pages,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Padding padding = new Padding(0, 0, 0, 0);
            string strPadding = DomUtil.GetAttr(nodeContainer,
                "padding");
            if (String.IsNullOrEmpty(strPadding) == false)
            {
                nRet = GetPadding(strPadding,
                out padding,
                out strError);
                if (nRet == -1)
                {
                    strError = "元素<" + nodeContainer.Name + ">中padding属性值 '" + strPadding + "' 格式错误: " + strError;
                    return -1;
                }
            }

            rect_def.Padding = padding;


            List<PrintLine> lines = new List<PrintLine>();

            // 将一个容器元素下级的全部内容切割为Line数组
            nRet = Process(g,
                rect_def,
                nodeContainer,
                macro_table,
                rect_group,
        ref lines,
        out strError);
            if (nRet == -1)
                return -1;

            SetLastLine(lines);

            // 组装到Page中
            int iPage = 0;
            Page current_page = null;
            if (pages.Count > iPage)
                current_page = pages[iPage];
            else
            {
                current_page = new Page();
                pages.Add(current_page);
            }
            float current_height = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                PrintLine line = lines[i];

                if (current_height + line.Height > rect_def.Height - rect_def.Padding.Vertical
                    && current_page.Lines.Count > 0    // 至少有一个行
                    && bRemainPage == false)
                {
                    // 新增一页
                    current_height = 0;
                    iPage++;
                    if (pages.Count > iPage)
                        current_page = pages[iPage];
                    else
                    {
                        current_page = new Page();
                        pages.Add(current_page);
                    }

                }

                // 
                line.X += rect_def.X + rect_def.Padding.Left;
                line.Y += rect_def.Y + rect_def.Padding.Top + current_height;
                current_page.Lines.Add(line);

                current_height += line.Height;
            }

            return iPage;
        }

#if NO
        static void AdjuestLineDelta(Line line,
            float delta)
        {
            for (int i = 0; i < line.Boxes.Count; i++)
            {
                Box box = line.Boxes[i];
                box.Delta += delta;
            }
        }
#endif

        static Color GetColor(string strColorString)
        {
            // Create the ColorConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

            return (Color)converter.ConvertFromString(strColorString);
        }

        static string GetColorString(Color color)
        {
            // Create the ColorConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Color));

            return converter.ConvertToString(color);
        }

#if NO
        static Font GetFont(string strFontString)
        {
            // 2014/3/27
            if (String.IsNullOrEmpty(strFontString) == true)
                return Control.DefaultFont;

            // Create the FontConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

            Font font = (Font)converter.ConvertFromString(strFontString);

            // GDI+ 新安装字体后不能显现的 BUG 绕过方法
            if (string.IsNullOrEmpty(font.OriginalFontName) == false
                && font.OriginalFontName != font.Name)
            {
                List<FontFamily> families = new List<FontFamily>(GlobalVars.PrivateFonts.Families);

                FontFamily t = families.Find(f => string.Compare(f.Name, font.OriginalFontName, true) == 0);

                // if (families.Exists(f => string.Compare(f.Name, font.OriginalFontName, true) == 0) == true)
                if ( t != null)
                    font = new Font(t, font.Size, font.Style);
            }

            return font;
        }
#endif

        static string GetFontString(Font font)
        {
            // Create the FontConverter.
            System.ComponentModel.TypeConverter converter =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

            return converter.ConvertToString(font);
        }

        // 设置(折行前)最后一行标志
        static void SetLastLine(List<PrintLine> lines)
        {
            if (lines.Count == 0)
                return;
            PrintLine line = lines[lines.Count - 1];
            line.IsParagraphTail = true;

        }

        // parameters:
        //      state  ParagraphFirst 是否为首次处理一个<p>
        int Process(
            Graphics g,
            RectParam rect_def,
            XmlNode node,
            Hashtable macro_table,
            RectGroup rect_group,
            ref List<PrintLine> lines,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (node.NodeType == XmlNodeType.Element
                && node.Name == "br")
            {
                SetLastLine(lines);

                Color color;
                Font font = GetNodeFont(node, out color);

                ParagraphFormat format = GetNodeParagraphFormat(node);

                try
                {
                    nRet = BuildLines(g,
            rect_def,
            "\r\n",
            font,
            color,
            format,
            ref lines,
            out strError);
                    if (nRet == -1)
                        return -1;
                }
                finally
                {
                    font.Dispose();
                }

                return 0;
            }

            int nStartLines = lines.Count;
            int nLinesLimit = -1;
            ParagraphGroup p_group = null;

            // <p>的开头，触发一下换行
            if (node.NodeType == XmlNodeType.Element
                && node.Name == "p")
            {
                SetLastLine(lines);

                Color color;
                Font font = GetNodeFont(node, out color);

                ParagraphFormat format = GetNodeParagraphFormat(node);
                nLinesLimit = (int)format.MaxLines;

                p_group = new ParagraphGroup();
                p_group.Format = format;
                rect_group.Paragraphs.Add(p_group);

                try
                {

                    nRet = BuildLines(g,
            rect_def,
            null,
            font,
            color,
            format,
            ref lines,
            out strError);
                    if (nRet == -1)
                        return -1;


                }
                finally
                {
                    font.Dispose();
                }
            }

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode child = node.ChildNodes[i];

                if (child.NodeType == XmlNodeType.Text)
                {
                    string strText = child.Value;
                    strText = strText.Replace("\n", "\r");
                    strText = strText.Replace("\r", "");

                    if (macro_table != null)
                    {
                        strText = StringUtil.MacroString(macro_table,
        strText);
                    }

                    Color color;
                    Font font = GetNodeFont(node, out color);

                    ParagraphFormat format = GetNodeParagraphFormat(node);

                    try
                    {
                        nRet = BuildLines(g,
                rect_def,
                strText,
                font,
                color,
                format,
                ref lines,
                out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    finally
                    {
                        font.Dispose();
                    }
                }

                if (child.NodeType == XmlNodeType.Element)
                {
                    int x_delta = 0;
                    int y_delta = 0;
                    RectParam current_rect_def = rect_def;
                    if (child.Name == "p")
                    {
                        // 2012/4/23
                        // 让<p>的padding属性起作用
                        ParagraphFormat format = GetNodeParagraphFormat(child);
                        current_rect_def = new RectParam(rect_def);
                        current_rect_def.Padding = AddPadding(current_rect_def.Padding, format.Padding);
                        x_delta = format.Padding.Left;
                        y_delta = format.Padding.Top;
                    }

                    int nStartLine = 0;
                    if (lines != null)
                        nStartLine = lines.Count;

                    nRet = Process(
                        g,
                        current_rect_def,
                        child,
                        macro_table,
                        rect_group,
                        ref lines,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // 对新增的行进行平移
                    if (x_delta != 0 || y_delta != 0)
                    {
                        for (int j = nStartLine; j < lines.Count; j++)
                        {
                            PrintLine line = lines[j];

                            // 
                            line.X += x_delta;
                            line.Y += y_delta;
                        }
                    }
                }
            }

            // 删除多余的行
            if (nLinesLimit != -1)
            {
                // TODO 如果发生切断，是不是最后要加... ?
                while (lines.Count > nStartLines + nLinesLimit)
                {
                    lines.RemoveAt(lines.Count - 1);
                }
            }

            if (p_group != null)
            {
                for (int i = nStartLines; i < lines.Count; i++)
                {
                    p_group.Lines.Add(lines[i]);
                }
            }

            return 0;
        }

        static Padding AddPadding(Padding p1, Padding p2)
        {
            return new Padding(p1.Left + p2.Left,
                p1.Top + p2.Top,
                p1.Right + p2.Right,
                p1.Bottom + p2.Bottom);
        }

        // 获得一个XML元素位置的字体
        // 从下级元素向上级元素寻找
        Font GetNodeFont(XmlNode node,
            out Color color)
        {
            color = Color.Black;

            Debug.Assert(node.NodeType == XmlNodeType.Element, "node.NodeType == XmlNodeType.Element 不满足");
            string strFontName = "";
            string strFontSize = "";
            string strFontStyle = "";
            string strFontColor = "";
            while (node != null)
            {
                if (node.Name == "font")
                {
                    if (String.IsNullOrEmpty(strFontName) == true)
                        strFontName = DomUtil.GetAttr(node, "name");
                    if (String.IsNullOrEmpty(strFontSize) == true)
                        strFontSize = DomUtil.GetAttr(node, "size");
                    if (String.IsNullOrEmpty(strFontStyle) == true)
                        strFontStyle = DomUtil.GetAttr(node, "style");
                    if (String.IsNullOrEmpty(strFontColor) == true)
                        strFontColor = DomUtil.GetAttr(node, "color");
                }

                node = node.ParentNode;
            }

            if (String.IsNullOrEmpty(strFontName) == true)
                strFontName = this.m_pagesetting.FontName;
            if (String.IsNullOrEmpty(strFontSize) == true)
                strFontSize = this.m_pagesetting.FontSize;
            if (String.IsNullOrEmpty(strFontStyle) == true)
                strFontStyle = this.m_pagesetting.FontStyle;
            if (String.IsNullOrEmpty(strFontColor) == true)
                strFontColor = this.m_pagesetting.FontColor;

            string strFontString = strFontName;

            if (String.IsNullOrEmpty(strFontSize) == false)
                strFontString += "," + strFontSize;
            
            if (String.IsNullOrEmpty(strFontStyle) == false)
                strFontString += ",style=" + strFontStyle;

            if (String.IsNullOrEmpty(strFontColor) == false)
                color = GetColor(strFontColor);

            return Global.BuildFont(strFontString);
        }

        // 获得一个XML元素位置的段落格式
        // 从下级元素向上级元素寻找
        ParagraphFormat GetNodeParagraphFormat(XmlNode node)
        {
            Debug.Assert(node.NodeType == XmlNodeType.Element, "node.NodeType == XmlNodeType.Element 不满足");
            string strIndent = "";
            string strLineBreak = "";
            string strAlign = "";
            string strMaxLines = "";
            string strVertAlign = "";

            // <p>本身的padding属性
            Padding padding = new Padding(0, 0, 0, 0);
            string strPadding = DomUtil.GetAttr(node,
    "padding");
            if (String.IsNullOrEmpty(strPadding) == false)
            {
                string strError = "";
                int nRet = GetPadding(strPadding,
                out padding,
                out strError);
                if (nRet == -1)
                {
                    strError = "元素<" + node.Name + ">中padding属性值 '" + strPadding + "' 格式错误: " + strError;
                    throw new Exception(strError);
                }
            }


            while (node != null)
            {
                if (node.Name == "p")
                {
                    if (String.IsNullOrEmpty(strIndent) == true)
                        strIndent = DomUtil.GetAttr(node, "indent");
                    if (String.IsNullOrEmpty(strLineBreak) == true)
                        strLineBreak = DomUtil.GetAttr(node, "lineBreak");
                    if (String.IsNullOrEmpty(strAlign) == true)
                        strAlign = DomUtil.GetAttr(node, "align");
                    if (String.IsNullOrEmpty(strMaxLines) == true)
                        strMaxLines = DomUtil.GetAttr(node, "maxLines");
                    if (String.IsNullOrEmpty(strVertAlign) == true)
                        strVertAlign = DomUtil.GetAttr(node, "valign");
                }

                node = node.ParentNode;
            }

            if (String.IsNullOrEmpty(strIndent) == true)
                strIndent = this.m_pagesetting.Indent.ToString();

            ParagraphFormat format = new ParagraphFormat();
            if (string.IsNullOrEmpty(strIndent) == false)
            {
                if (float.TryParse(strIndent, out format.Indent) == false)
                {
                    string strError = "indent属性值 '" + strIndent + "' 格式不正确";
                    throw new Exception(strError);
                }
            }
            else
                format.Indent = this.m_pagesetting.Indent;

            if (String.IsNullOrEmpty(strLineBreak) == false)
                format.LineBreak = strLineBreak;
            else
                format.LineBreak = this.m_pagesetting.LineBreak;

            if (String.IsNullOrEmpty(strAlign) == false)
                format.Align = strAlign;
            else
                format.Align = this.m_pagesetting.Align;

            if (String.IsNullOrEmpty(strVertAlign) == false)
                format.VertAlign = strVertAlign;
            else
                format.VertAlign = this.m_pagesetting.VertAlign;

            if (String.IsNullOrEmpty(strMaxLines) == true)
                format.MaxLines = -1;
            else
            {
                try
                {
                    format.MaxLines = Convert.ToInt64(strMaxLines);
                }
                catch (Exception /*ex*/)
                {
                    string strError = "maxLines属性值 '" + strMaxLines + "' 格式不正确";
                    throw new Exception(strError);
                }
            }

            format.Padding = padding;

            return format;
        }

        static HorzAlign GetHorzAlign(string strText)
        {
            if (strText == "left")
                return HorzAlign.Left;
            if (strText == "center")
                return HorzAlign.Center;
            if (strText == "right")
                return HorzAlign.Right;
            if (strText == "leftright")
                return HorzAlign.LeftRight;

            return HorzAlign.Left;
        }

        // 将一个字符串切割为Line数组
        // parameters:
        //      strTextParam    ==null 表示触发<p>前面的初始化换行
        //                      == "\r\n" 执行<br/>动作
        int BuildLines(Graphics g,
            RectParam rect_def,
            string strTextParam,
            Font font,
            Color color,
            ParagraphFormat p_format,
            ref List<PrintLine> lines,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            float fLineHeight = 0;
            string strFontString = GetFontString(font);
            string strFontColor = GetColorString(color);

            // 表示建立一个空的<p>元素显示位置
            if (strTextParam == null)
            {
                SizeF size = g.MeasureString("1", font);
                fLineHeight = size.Height;

                float emheight = font.FontFamily.GetEmHeight(FontStyle.Regular);
                float ascent = font.FontFamily.GetCellAscent(FontStyle.Regular);

                PrintLine current_line = null;

                current_line = new PrintLine();
                current_line.HorzAlign = GetHorzAlign(p_format.Align);
                current_line.Height = fLineHeight;
                current_line.Width = rect_def.Width - rect_def.Padding.Horizontal;
                current_line.FontDef = strFontString;
                current_line.ColorDef = strFontColor;
                current_line.BaseRatio = ascent / emheight;
                lines.Add(current_line);
                current_line.Indent = p_format.Indent;

                return 0;
            }

            // 回车换行
            if (strTextParam == "\r\n")
            {
                SizeF size = g.MeasureString("1", font);
                fLineHeight = size.Height;

                float emheight = font.FontFamily.GetEmHeight(FontStyle.Regular);
                float ascent = font.FontFamily.GetCellAscent(FontStyle.Regular);

                PrintLine current_line = null;

                // 另起一行
                current_line = new PrintLine();
                current_line.HorzAlign = GetHorzAlign(p_format.Align);
                current_line.Height = fLineHeight;
                current_line.Width = rect_def.Width - rect_def.Padding.Horizontal;
                current_line.FontDef = strFontString;
                current_line.ColorDef = strFontColor;
                current_line.BaseRatio = ascent / emheight;
                lines.Add(current_line);
                current_line.Indent = p_format.Indent;

                return 0;
            }


            for (int j=0; ;j++ )
            {
                if (String.IsNullOrEmpty(strTextParam) == true)
                    break;

                float emheight = font.FontFamily.GetEmHeight(FontStyle.Regular);
                float ascent = font.FontFamily.GetCellAscent(FontStyle.Regular);

                int nLen = Math.Min(30, strTextParam.Length);
                string strText = strTextParam.Substring(0, nLen);
                strTextParam = strTextParam.Remove(0, nLen);

                CharacterRange[] ranges = new CharacterRange[strText.Length];
                for (int i = 0; i < strText.Length; i++)
                {
                    ranges[i] = new CharacterRange(i, 1);
                }

                StringFormat format = new StringFormat();
                format.FormatFlags = StringFormatFlags.NoClip ;
                format.SetMeasurableCharacterRanges(ranges);


                Region[] regions = new Region[strText.Length];

                {
                    SizeF size = g.MeasureString(strText, font);
                    fLineHeight = size.Height;
                    RectangleF layout = new RectangleF(0, 0, size.Width, size.Height);
                    regions = g.MeasureCharacterRanges(
                        strText,
                        font,
                        layout,
                        format);
                }

                // 切割为若干行
                {
                    float x = 0;
                    PrintLine current_line = null;
                    if (lines.Count == 0)
                    {
                        current_line = new PrintLine();
                        current_line.HorzAlign = GetHorzAlign(p_format.Align);
                        current_line.Height = fLineHeight;
                        current_line.Width = rect_def.Width - rect_def.Padding.Horizontal;
                        current_line.FontDef = strFontString;
                        current_line.ColorDef = strFontColor;
                        current_line.BaseRatio = ascent / emheight;
                        lines.Add(current_line);
                        x = p_format.Indent;
                        current_line.Indent = p_format.Indent;

                    }
                    else
                    {
                        current_line = lines[lines.Count - 1];
                        x = GetLineWidth(current_line) + current_line.Indent;
                    }

                    for (int i = 0; i < regions.Length; i++)
                    {
                        string strCurrentChar = strText.Substring(i, 1);

                        Region region = regions[i];
                        RectangleF rect = region.GetBounds(g);
                        if (x + rect.Width > rect_def.Width - rect_def.Padding.Horizontal)
                        {
                            if (p_format.LineBreak == "word")
                            {
                                // return:
                                //      -1   无需特殊处理
                                //      其他  需要切断的位置
                                nRet = DetectLineBreak(strCurrentChar,
                                    current_line);
                            }
                            else
                                nRet = -1;

                            if (nRet != -1)
                            {
                                PrintLine prev_line = current_line;

                                // 另起一行
                                current_line = new PrintLine();
                                current_line.HorzAlign = GetHorzAlign(p_format.Align);
                                current_line.Width = rect_def.Width - rect_def.Padding.Horizontal;
                                current_line.BaseRatio = prev_line.BaseRatio;
                                lines.Add(current_line);
                                x = 0;

                                int nStart = nRet;

                                // 准备字体、颜色字符串
                                string strTempFontString = prev_line.FontDef;
                                string strTempColorString = prev_line.ColorDef;
                                for (int k = 0; k <= nStart; k++)
                                {
                                    Box box = prev_line.Boxes[k];
                                    if (string.IsNullOrEmpty(box.FontDef) == false)
                                        strTempFontString = box.FontDef;
                                    if (string.IsNullOrEmpty(box.ColorDef) == false)
                                        strTempColorString = box.ColorDef;
                                }

                                current_line.FontDef = strTempFontString;
                                current_line.ColorDef = strTempColorString;

                                // 切断，同时获得行高度
                                float fTempLineHeight = 0;
                                int nCount = prev_line.Boxes.Count;
                                for (int k = nStart; k < nCount; k++)
                                {
                                    Box box = prev_line.Boxes[nStart];
                                    current_line.Boxes.Add(box);
                                    prev_line.Boxes.RemoveAt(nStart);
                                    if (fTempLineHeight < box.Height)
                                        fTempLineHeight = box.Height;
                                }

                                current_line.Height = fTempLineHeight;
                            }
                            else
                            {
                                // 另起一行
                                current_line = new PrintLine();
                                current_line.HorzAlign = GetHorzAlign(p_format.Align);
                                current_line.Height = fLineHeight;
                                current_line.Width = rect_def.Width - rect_def.Padding.Horizontal;
                                current_line.FontDef = strFontString;
                                current_line.ColorDef = strFontColor;
                                current_line.BaseRatio = ascent / emheight;
                                lines.Add(current_line);
                                x = 0;

                            }
                        }
                        else
                        {
                        }


                        if (current_line.Boxes.Count == 0
                            && lines.Count > 1
                            && strCurrentChar == " ")
                        {
                            // 忽略第二行以后最左端的空格
                        }
                        else
                        {
                            Box box = new Box();
                            box.Text = strCurrentChar;
                            box.Width = rect.Width;
                            box.Height = rect.Height;
                            box.Base = fLineHeight * ascent / emheight;
                            if (i == 0 && j == 0)
                            {
                                box.FontDef = strFontString;
                                box.ColorDef = strFontColor;
                            }
                            if (current_line.Height < fLineHeight)
                            {
                                current_line.Height = fLineHeight;
                            }
                            current_line.Boxes.Add(box);

                            x += rect.Width;
                        }

                    }
                }
            }

            return 0;
        }



        static string SepChars = " ~!@#$%^&*()_+| `-=\\/,.;':<>{}[]?";

        // return:
        //      -1   无需特殊处理
        //      其他  需要切断的位置
        static int DetectLineBreak(string strCurrentChar,
            PrintLine line)
        {
            // 如果是西文分隔符号
            if (SepChars.IndexOf(strCurrentChar) != -1)
                return -1;

            // 如果是中文字符
            if (StringUtil.IsHanzi(strCurrentChar[0]) == true)
                return -1;

            for (int i = line.Boxes.Count - 1; i >= 0; i--)
            {
                Box box = line.Boxes[i];
                if (SepChars.IndexOf(box.Text) != -1)
                {
                    if (i == line.Boxes.Count - 1)
                        return -1;
                    return i + 1;
                }
                if (StringUtil.IsHanzi(box.Text[0]) == true)
                {
                    if (i == line.Boxes.Count - 1)
                        return -1;
                    return i + 1;
                }
            }

            return -1;  // 如果整个line都是无法切割，就干脆放弃按词折行
        }

        // 获得一个行的最右端位置
        static float GetLineWidth(PrintLine line)
        {
            float x = 0;
            for (int i = 0; i < line.Boxes.Count; i++)
            {
                Box box = line.Boxes[i];
                x += box.LeftBlank + box.Width;
            }

            return x;
        }

#if NO
        // 将一个<p>元素内容切割为Line数组
        int CutLines(Graphics g,
            RectParam rect_def,
            XmlNode nodeP,
            out List<Line> lines,
            out string strError)
        {
            strError = "";
            lines = new List<Line>();

            float fLineHeight = 0;
            string strFontString = "";

            string strText = nodeP.InnerText;

            if (String.IsNullOrEmpty(strText) == true)
            {
                // TODO 只有一行的高度
                using (Font font = new Font("Times New Roman", 16.0F))
                {
                    // TODO: 需要测试一下空字符串是否可以用于MesasureString()
                    SizeF size = g.MeasureString(strText, font);
                    fLineHeight = size.Height;
                    strFontString = GetFontString(font);
                }
                Line current_line = new Line();
                current_line.Height = fLineHeight;
                current_line.FontDef = strFontString;
                lines.Add(current_line);

                return 0;
            }

            CharacterRange[] ranges = new CharacterRange[strText.Length];
            for (int i = 0; i < strText.Length; i++)
            {
                ranges[i] = new CharacterRange(i, 1);
            }

            StringFormat format = new StringFormat();
            format.FormatFlags = StringFormatFlags.NoClip;
            format.SetMeasurableCharacterRanges(ranges);


            Region[] regions = new Region[strText.Length];

            using (Font font = new Font("Times New Roman", 16.0F))
            {

                SizeF size = g.MeasureString(strText, font);
                fLineHeight = size.Height;
                RectangleF layout = new RectangleF(0, 0, size.Width, size.Height);
                regions = g.MeasureCharacterRanges(
                    strText,
                    font,
                    layout,
                    format);
                strFontString = GetFontString(font);
            }

            // 切割为若干行
            {
                Line current_line = new Line();
                current_line.Height = fLineHeight;
                current_line.FontDef = strFontString;
                lines.Add(current_line);
                float x = 0;
                for (int i = 0; i < regions.Length; i++)
                {
                    Region region = regions[i];
                    RectangleF rect = region.GetBounds(g);
                    if (x > rect_def.Width)
                    {
                        // 另起一行
                        current_line = new Line();
                        current_line.Height = fLineHeight;
                        current_line.FontDef = strFontString;
                        lines.Add(current_line);
                        Box box = new Box();
                        box.Text = strText.Substring(i, 1);
                        box.Width = rect.Width;
                        box.Height = rect.Height;
                        current_line.Boxes.Add(box);
                        x = 0;
                    }
                    else
                    {
                        Box box = new Box();
                        box.Text = strText.Substring(i, 1);
                        box.Width = rect.Width;
                        box.Height = rect.Height;
                        current_line.Boxes.Add(box);
                    }

                    x += rect.Width;
                }
            }

            return 0;
        }

#endif

    }

    // 一个页面
    internal class Page
    {
        public List<PrintLine> Lines = new List<PrintLine>();
    }

    internal class PrintLine
    {
        public string FontDef = ""; // 字体定义
        public string ColorDef = "";

        public List<Box> Boxes = new List<Box>();
        public float X = 0;
        public float Y = 0;

        public float Width = 0;


        public float Height = 0;

        public float BaseRatio = (float)0.9;    // 上部占据整个高度的比率
        public float Indent = 0;
        public HorzAlign HorzAlign = HorzAlign.Left;

        public bool IsParagraphTail = false;    // 是否为自然段<p>的最后一个行，或者发生过折行<br/>的行
    }

    internal enum HorzAlign
    {
        Left = 0,
        Center = 1,
        Right = 2,
        LeftRight = 3,
    }

    // 一个最小的矩形显示单元
    internal class Box
    {
        public string FontDef = ""; // 字体定义。如果为空，表示参考前一个。如果已经是最前一个，则参考Line的FontDef
        public string ColorDef = "";

        public string Text = "";
        public float Width = 0;
        public float Height = 0;
        public float LeftBlank = 0;

        public float Base = 0; // 基线
    }

    internal class RectParam
    {
        public float X = 0;
        public float Y = 0;

        public float Width = 0;
        public string WidthString = "";

        public Padding Padding = new Padding(0, 0, 0, 0);

        public float Height = 0;

        public RectParam(RectParam rect_ref)
        {
            this.X = rect_ref.X;
            this.Y = rect_ref.Y;
            this.Width = rect_ref.Width;
            this.WidthString = rect_ref.WidthString;
            this.Padding = new Padding(rect_ref.Padding.Left,
                rect_ref.Padding.Top,
                rect_ref.Padding.Right,
                rect_ref.Padding.Bottom);
            this.Height = rect_ref.Height;
        }

        public RectParam()
        {
        }
    }

    internal class PageSetting
    {
        public float Width = 0;
        public float Height = 0;
        public string FontName = "Times New Roman";
        public string FontSize = "16";
        public string FontStyle = "";
        public string FontColor = "Black";
        public float Indent = 0;    // 缺省的段落缩进
        public string LineBreak = "word";  // 折行规则 word char
        public string Align = "left";   // 水平对齐 left center right leftright
        public string VertAlign = "top";   // 垂直对齐 top bottom

        public int Build(string strPageSettingXml,
            out string strError)
        {
            strError = "";

            XmlDocument setting_dom = new XmlDocument();
            try
            {
                setting_dom.LoadXml(strPageSettingXml);
            }
            catch (Exception ex)
            {
                strError = "<pageSetting>片断装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            string strWidth = DomUtil.GetAttr(setting_dom.DocumentElement,
                "width");
            if (string.IsNullOrEmpty(strWidth) == false)
            {
                if (float.TryParse(strWidth, out this.Width) == false)
                {
                    strError = "<pageSetting>的width属性值 '" + strWidth + "' 格式不正确";
                    return -1;
                }
            }

            string strHeight = DomUtil.GetAttr(setting_dom.DocumentElement,
                "height");
            if (string.IsNullOrEmpty(strHeight) == false)
            {
                if (float.TryParse(strHeight, out this.Height) == false)
                {
                    strError = "<pageSetting>的height属性值 '" + strWidth + "' 格式不正确";
                    return -1;
                }
            }

            XmlNode nodeFont = setting_dom.DocumentElement.SelectSingleNode("font");
            if (nodeFont != null)
            {

                string strValue = DomUtil.GetAttr(nodeFont, "name");
                if (String.IsNullOrEmpty(strValue) == false)
                    this.FontName = strValue;

                strValue = DomUtil.GetAttr(nodeFont, "size");
                if (String.IsNullOrEmpty(strValue) == false)
                    this.FontSize = strValue;

                strValue = DomUtil.GetAttr(nodeFont, "style");
                if (String.IsNullOrEmpty(strValue) == false)
                    this.FontStyle = strValue;

                strValue = DomUtil.GetAttr(nodeFont, "color");
                if (String.IsNullOrEmpty(strValue) == false)
                    this.FontColor = strValue; 
            }
            nodeFont = null;

            XmlNode nodeP = setting_dom.DocumentElement.SelectSingleNode("p");
            if (nodeP != null)
            {

                string strValue = DomUtil.GetAttr(nodeP, "indent");
                if (String.IsNullOrEmpty(strValue) == false)
                {
                    if (float.TryParse(strValue, out this.Indent) == false)
                    {
                        strError = "<pageSetting>的<p>的indent属性值 '" + strValue + "' 格式不正确";
                        return -1;
                    }
                }

                strValue = DomUtil.GetAttr(nodeP, "lineBreak");
                if (String.IsNullOrEmpty(strValue) == false)
                {
                    strValue = strValue.ToLower();
                    if (strValue != "word" && strValue != "char")
                    {
                        strError = "<pageSetting>的<p>的lineBreak属性值 '" + strValue + "' 格式不正确，应该为word或char之一";
                        return -1;
                    }
                    this.LineBreak = strValue;
                }

                strValue = DomUtil.GetAttr(nodeP, "align");
                if (String.IsNullOrEmpty(strValue) == false)
                {
                    strValue = strValue.ToLower();
                    if (strValue != "left"
                        && strValue != "right"
                        && strValue != "center"
                        && strValue != "leftright")
                    {
                        strError = "<pageSetting>的<p>的align属性值 '" + strValue + "' 格式不正确，应该为left/center/right/leftright之一";
                        return -1;
                    }
                    this.Align = strValue;
                }

                strValue = DomUtil.GetAttr(nodeP, "valign");
                if (String.IsNullOrEmpty(strValue) == false)
                {
                    strValue = strValue.ToLower();
                    if (strValue != "top"
                        && strValue != "bottom")
                    {
                        strError = "<pageSetting>的<p>的valign属性值 '" + strValue + "' 格式不正确，应该为top/bottom之一";
                        return -1;
                    }
                    this.VertAlign = strValue;
                }
            }
            nodeP = null;

            return 0;
        }
    }

    /*
    public class ProcessState
    {
        public bool ParagraphFirst = true;
    }
     * */

    internal class ParagraphFormat
    {
        public float Indent = 0;
        public string LineBreak = "word";
        public string Align = "left";
        public string VertAlign = "top";
        public Int64 MaxLines = -1; // 一个段落的最多显示行数。缺省为-1，表示不限制 

        public Padding Padding = new Padding(0,0,0,0);
    }

    // 一个独立的矩形区域
    internal class RectGroup
    {
        // 下属的各个段落
        public List<ParagraphGroup> Paragraphs = new List<ParagraphGroup>();
    }

    internal class ParagraphGroup
    {
        public ParagraphFormat Format = null;
        // 下属的各个行
        public List<PrintLine> Lines = new List<PrintLine>();
    }

    /// <summary>
    /// 设置进度的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void SetProgressEventHandler(object sender,
SetProgressEventArgs e);

    // 设置范围：Value = -1
    // 否则为设置Value值，而Start和End为参考用
    /// <summary>
    /// 设置进度事件的参数
    /// </summary>
    public class SetProgressEventArgs : EventArgs
    {
        /// <summary>
        /// 开始位置
        /// </summary>
        public long Start = 0;
        /// <summary>
        /// 结束位置
        /// </summary>
        public long End = 0;
        /// <summary>
        /// 当前值
        /// </summary>
        public long Value = 0;
    }
}
