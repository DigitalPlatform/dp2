using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using DigitalPlatform.Text;
using System.Xml;
using System.Web.UI;
using System.IO;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using ClosedXML.Excel;
using DigitalPlatform.Xml;


namespace DigitalPlatform.dp2.Statis
{
    // 报表格式化输出有关的类

    /*
	// 列数据类型
	public enum DataType
	{
		Auto = 0,
		String = 1,
		Number = 2,
		Price = 3,	// 100倍金额整数
	}
	*/

    // 一个列的格式输出属性
    public class PrintColumn
    {
        public string Title = "";	// 列标题

        string m_strCssClass = "";

        // CSS样式类。2007/5/18
        public string CssClass
        {
            get
            {
#if NO
                if (String.IsNullOrEmpty(m_strCssClass) == true)
                    return Title;   // 如果没有设置css类，则用列标题来代替
#endif
                return m_strCssClass;
            }
            set
            {
                m_strCssClass = value;
            }
        }

        public bool Hidden = false;	// 列是否在输出时隐藏
        public int ColumnNumber = -2;	// 在table中的真实列号 -1 代表行标题

        public string DefaultValue = "";	// 数据缺省值

        public bool Sum = false;	// 本列是否需要“合计”

        public DataType DataType = DataType.Auto;

        public int Width = -1;	// 列宽度

        public int Colspan = 1; // 2013/6/14

        public string Eval = "";    // 2014/6/1
    }

    // 报表。实际上是一个格式输出属性数组
    public class Report : ArrayList
    {
        public event OutputLineEventHandler OutputLine = null;
        public event SumCellEventHandler SumCell = null;

        public bool SumLine = false;	// 是否需要在最后 输出 合计行


        public new PrintColumn this[int nIndex]
        {
            get
            {
                return (PrintColumn)base[nIndex];
            }
            set
            {
                base[nIndex] = value;
            }
        }



        // 根据一个表格按照缺省特性创建一个Report对象
        // parameters:
        //		strDefaultValue	全部列的缺省值
        //				null表示不改变缺省值""，否则为strDefaultValue指定的值
        //		bSum	是否全部列都要参加合计
        //      bContentColumn  是否考虑内容行中比指定的栏目多出来的栏目
        public static Report BuildReport(Table table,
            string strColumnTitles,
            string strDefaultValue,
            bool bSum,
            bool bContentColumn = true)
        {
            // Debug.Assert(false, "");
            if (table.Count == 0)
                return null;	// 无法创建。内容必须至少一行以上

            Report report = new Report();

            Line line = table.FirstHashLine();	// 随便得到一行。这样不要求table排过序

            // 列标题
            {
                PrintColumn column = new PrintColumn();
                column.ColumnNumber = -1;
                report.Add(column);
            }

            int nTitleCount = 0;

            if (strColumnTitles != null)
            {
                string[] aName = strColumnTitles.Split(new Char[] { ',' });
                nTitleCount = aName.Length;
            }

            int nColumnCount = nTitleCount;
            if (bContentColumn == true)
                nColumnCount = Math.Max(line.Count + 1, nTitleCount);


            // 检查表格第一行
            // 因为列标题column已经加入，所以现在最多加入nTitleCount-1栏
            for (int i = 0; i < nColumnCount - 1; i++)
            {
                PrintColumn column = new PrintColumn();
                column.ColumnNumber = i;

                if (strDefaultValue != null)
                    column.DefaultValue = strDefaultValue;

                column.Sum = bSum;

                report.Add(column);
            }


            // 添加列标题
            if (strColumnTitles != null)
            {
                string[] aName = strColumnTitles.Split(new Char[] { ',' });

                /*
                if (aName.Length < report.Count)
                {
                    string strError = "列定义 '" + strColumnTitles + "' 中的列数 " + aName.Length.ToString() + "小于报表实际最大列数 " + report.Count.ToString();
                    throw new Exception(strError);
                }*/


                int j = 0;
                for (j = 0; j < report.Count; j++)
                {
                    // 2007/10/26
                    if (j >= aName.Length)
                        break;

                    string strText = "";

                    strText = aName[j];

                    string strNameText = "";
                    string strNameClass = "";

                    int nRet = strText.IndexOf("||");
                    if (nRet == -1)
                        strNameText = strText;
                    else
                    {
                        strNameText = strText.Substring(0, nRet);
                        strNameClass = strText.Substring(nRet + 2);
                    }


                    PrintColumn column = (PrintColumn)report[j];
                    if (j < aName.Length)
                    {
                        column.Title = strNameText;
                        column.CssClass = strNameClass;
                    }
                }
            }

            report.SumLine = bSum;

            // 计算 colspan
            PrintColumn current = null;
            foreach (PrintColumn column in report)
            {
                if (string.IsNullOrEmpty(column.Title) == false
                    && column.Title[0] == '+'
                    && current != null)
                {
                    column.Colspan = 0; // 表示这是一个从属的列
                    current.Colspan++;
                }
                else
                    current = column;
            }

            return report;
        }

        static int ParseLines(string strInnerXml,
            out List<string> lines,
            out string strError)
        {
            lines = new List<string>();
            strError = "";

            XmlDocument dom = new XmlDocument();
            // dom.LoadXml("<root />");
            dom.AppendChild(dom.CreateElement("root"));

            try
            {
                dom.DocumentElement.InnerXml = strInnerXml;
            }
            catch (Exception ex)
            {
                strError = "InnerXml 装载时出错: " + ex.Message;
                return -1;
            }

            // TODO: 只有 <br /> 才分隔，其他的要联成一片
            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Text)
                {
                    lines.Add(node.InnerText);
                }
            }

            return 0;
        }

        // RML 格式转换为 Excel 文件
        public static int RmlToExcel(string strRmlFileName,
    string strExcelFileName,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            using (Stream stream = File.OpenRead(strRmlFileName))
            using (XmlTextReader reader = new XmlTextReader(stream))
            {
                while (true)
                {
                    bool bRet = reader.Read();
                    if (bRet == false)
                    {
                        strError = "文件 " + strRmlFileName + " 没有根元素";
                        return -1;
                    }
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                }

                ExcelDocument doc = ExcelDocument.Create(strExcelFileName);
                try
                {
                    doc.NewSheet("Sheet1");

                    int nColIndex = 0;
                    int _lineIndex = 0;

                    string strTitle = "";
                    string strComment = "";
                    string strCreateTime = "";
                    // string strCss = "";
                    List<ColumnStyle> col_defs = null;

                    while (true)
                    {
                        bool bRet = reader.Read();
                        if (bRet == false)
                            break;
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "title")
                            {
                                strTitle = reader.ReadInnerXml();
                            }
                            else if (reader.Name == "comment")
                            {
                                strComment = reader.ReadInnerXml();
                            }
                            else if (reader.Name == "createTime")
                            {
                                strCreateTime = reader.ReadElementContentAsString();
                            }
                            else if (reader.Name == "style")
                            {
                                // strCss = reader.ReadElementContentAsString();
                            }
                            else if (reader.Name == "columns")
                            {
                                // 从 RML 文件中读入 <columns> 元素
                                nRet = ReadColumnStyle(reader,
            out col_defs,
            out strError);
                                if (nRet == -1)
                                {
                                    strError = "ReadColumnStyle() error : " + strError;
                                    return -1;
                                }

                            }
                            else if (reader.Name == "table")
                            {
                                List<string> lines = null;

                                nRet = ParseLines(strTitle,
           out lines,
           out strError);
                                if (nRet == -1)
                                {
                                    strError = "解析 title 内容 '" + strTitle + "' 时发生错误: " + strError;
                                    return -1;
                                }

                                // 输出标题文字
                                nColIndex = 0;
                                foreach (string t in lines)
                                {
                                    List<CellData> cells = new List<CellData>();
                                    cells.Add(new CellData(nColIndex, t));
                                    doc.WriteExcelLine(_lineIndex, cells);
                                    _lineIndex++;
                                }

                                nRet = ParseLines(strComment,
out lines,
out strError);
                                if (nRet == -1)
                                {
                                    strError = "解析 comment 内容 '" + strTitle + "' 时发生错误: " + strError;
                                    return -1;
                                }
                                nColIndex = 0;
                                foreach (string t in lines)
                                {
                                    List<CellData> cells = new List<CellData>();
                                    cells.Add(new CellData(nColIndex, t));
                                    doc.WriteExcelLine(_lineIndex, cells);

                                    _lineIndex++;
                                }

                                // 空行
                                _lineIndex++;

                            }
                            else if (reader.Name == "tr")
                            {
                                // 输出一行
                                List<CellData> cells = null;
                                nRet = ReadLine(reader,
                                    col_defs,
            out cells,
            out strError);
                                if (nRet == -1)
                                {
                                    strError = "ReadLine error : " + strError;
                                    return -1;
                                }
                                doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                                _lineIndex++;
                            }
                        }
                    }

                    // create time
                    {
                        _lineIndex++;
                        List<CellData> cells = new List<CellData>();
                        cells.Add(new CellData(0, "创建时间"));
                        cells.Add(new CellData(1, strCreateTime));
                        doc.WriteExcelLine(_lineIndex, cells);

                        _lineIndex++;
                    }

                }
                finally
                {
                    doc.SaveWorksheet();
                    doc.Close();
                }
            }

            return 0;
        }

        // 从 RML 文件中读入 <tr> 元素
        static int ReadLine(XmlTextReader reader,
            List<ColumnStyle> col_defs,
            out List<CellData> cells,
            out string strError)
        {
            strError = "";
            cells = new List<CellData>();
            int col_index = 0;

            int nColIndex = 0;
            while (true)
            {
                if (reader.Read() == false)
                    break;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "th" || reader.Name == "td")
                    {
                        string strText = reader.ReadElementContentAsString();

                        CellData new_cell = null;

                        string strType = "";

                        // 2014/8/16
                        if (col_defs != null
                            && col_index < col_defs.Count)
                            strType = col_defs[col_index].Type;

                        if (strType == "String")
                            new_cell = new CellData(nColIndex++, strText, true, 0);
                        else if (strType == "Number")
                        {
                            new_cell = new CellData(nColIndex++, strText, false, 0);
                        }
                        else // "Auto")
                        {
                            bool isString = !IsExcelNumber(strText);

                            new_cell = new CellData(nColIndex++, strText, isString, 0);
                        }

                        cells.Add(new_cell);

                        col_index++;
                    }
                }
                if (reader.NodeType == XmlNodeType.EndElement
    && reader.Name == "tr")
                    break;
            }

            return 0;
        }

        // 检测字符串是否为纯数字(前面可以包含一个'-'号)
        public static bool IsExcelNumber(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return false;

            bool bFoundNumber = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '-' && bFoundNumber == false)
                {
                    continue;
                }
                if (s[i] == '%' && i == s.Length - 1)
                {
                    // 最末一个字符为 %
                    continue;
                }
                if (s[i] > '9' || s[i] < '0')
                    return false;
                bFoundNumber = true;
            }
            return true;
        }

        class ColumnStyle
        {
            public string Class = "";
            public string Align = "";   // left/center/right
            public string Type = "";    // String/Currency/Auto/Number
        }

        // 从 RML 文件中读入 <columns> 元素
        static int ReadColumnStyle(XmlTextReader reader,
            out List<ColumnStyle> styles,
            out string strError)
        {
            strError = "";
            styles = new List<ColumnStyle>();

            while (true)
            {
                if (reader.Read() == false)
                    break;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "column")
                    {
                        ColumnStyle style = new ColumnStyle();
                        style.Align = reader.GetAttribute("align");
                        style.Class = reader.GetAttribute("class");
                        style.Type = reader.GetAttribute("type");
                        styles.Add(style);
                    }
                }
                if (reader.NodeType == XmlNodeType.EndElement
    && reader.Name == "columns")
                    break;
            }

            return 0;
        }

#if NO
        // 假定当前 node 下面的 element 每个都不会太大
        static void DumpNode(XmlTextReader reader,
            XmlWriter writer)
        {
            string strName = reader.Name;
            while (true)
            {
                if (reader.Read() == false)
                    break;
                if (reader.NodeType == XmlNodeType.Element)
                    writer.WriteRaw(reader.ReadOuterXml());

                if (reader.NodeType == XmlNodeType.EndElement
    && reader.Name == strName)
                    break;
            }
        }
#endif

        // RML 格式转换为 HTML 文件
        // parameters:
        //      strCssTemplate  CSS 模板。里面 %columns% 代表各列的样式
        public static int RmlToHtml(string strRmlFileName,
            string strHtmlFileName,
            string strCssTemplate,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            try
            {
                using (Stream stream = File.OpenRead(strRmlFileName))
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    while (true)
                    {
                        bool bRet = reader.Read();
                        if (bRet == false)
                        {
                            strError = "文件 " + strRmlFileName + " 没有根元素";
                            return -1;
                        }
                        if (reader.NodeType == XmlNodeType.Element)
                            break;
                    }

                    /*
                     * https://msdn.microsoft.com/en-us/library/system.xml.xmlwriter.writestring(v=vs.110).aspx
The default behavior of an XmlWriter created using Create is to throw an ArgumentException when attempting to write character values in the range 0x-0x1F (excluding white space characters 0x9, 0xA, and 0xD). These invalid XML characters can be written by creating the XmlWriter with the CheckCharacters property set to false. Doing so will result in the characters being replaced with numeric character entities (&#0; through &#0x1F). Additionally, an XmlTextWriter created with the new operator will replace the invalid characters with numeric character entities by default.
                     * */
                    using (XmlWriter writer = XmlWriter.Create(strHtmlFileName,
                        new XmlWriterSettings
                        {
                            Indent = true,
                            OmitXmlDeclaration = true,
                            CheckCharacters = false // 2016/6/3
                        }))
                    {
                        writer.WriteDocType("html", "-//W3C//DTD XHTML 1.0 Transitional//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd", null);
                        writer.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
                        // writer.WriteAttributeString("xml", "lang", "", "en");

                        string strTitle = "";
                        string strComment = "";
                        string strCreateTime = "";
                        string strCss = "";
                        List<ColumnStyle> styles = null;

                        while (true)
                        {
                            bool bRet = reader.Read();
                            if (bRet == false)
                                break;
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                if (reader.Name == "title")
                                {
                                    strTitle = reader.ReadInnerXml();
                                }
                                else if (reader.Name == "comment")
                                {
                                    strComment = reader.ReadInnerXml();
                                }
                                else if (reader.Name == "createTime")
                                {
                                    strCreateTime = reader.ReadElementContentAsString();
                                }
                                else if (reader.Name == "style")
                                {
                                    strCss = reader.ReadElementContentAsString();
                                }
                                else if (reader.Name == "columns")
                                {
                                    // 从 RML 文件中读入 <columns> 元素
                                    nRet = ReadColumnStyle(reader,
                out styles,
                out strError);
                                    if (nRet == -1)
                                    {
                                        strError = "ReadColumnStyle() error : " + strError;
                                        return -1;
                                    }
                                }
                                else if (reader.Name == "table")
                                {
                                    writer.WriteStartElement("head");

                                    writer.WriteStartElement("meta");
                                    writer.WriteAttributeString("http-equiv", "Content-Type");
                                    writer.WriteAttributeString("content", "text/html; charset=utf-8");
                                    writer.WriteEndElement();

                                    // title
                                    {
                                        writer.WriteStartElement("title");
                                        // TODO 读入的时候直接形成 lines
                                        writer.WriteString(strTitle.Replace("<br />", " ").Replace("<br/>", " "));
                                        writer.WriteEndElement();
                                    }

                                    // css
                                    if (string.IsNullOrEmpty(strCss) == false)
                                    {
                                        writer.WriteStartElement("style");
                                        writer.WriteAttributeString("media", "screen");
                                        writer.WriteAttributeString("type", "text/css");
                                        writer.WriteString(strCss);
                                        writer.WriteEndElement();
                                    }

                                    // CSS 模板
                                    else if (string.IsNullOrEmpty(strCssTemplate) == false)
                                    {
                                        StringBuilder text = new StringBuilder();
                                        foreach (ColumnStyle style in styles)
                                        {
                                            string strAlign = style.Align;
                                            if (string.IsNullOrEmpty(strAlign) == true)
                                                strAlign = "left";
                                            text.Append("TABLE.table ." + style.Class + " {"
                                                + "text-align: " + strAlign + "; }\r\n");
                                        }

                                        writer.WriteStartElement("style");
                                        writer.WriteAttributeString("media", "screen");
                                        writer.WriteAttributeString("type", "text/css");
                                        writer.WriteString("\r\n" + strCssTemplate.Replace("%columns%", text.ToString()) + "\r\n");
                                        writer.WriteEndElement();
                                    }

                                    writer.WriteEndElement();   // </head>

                                    writer.WriteStartElement("body");

                                    if (string.IsNullOrEmpty(strTitle) == false)
                                    {
                                        writer.WriteStartElement("div");
                                        writer.WriteAttributeString("class", "tabletitle");
                                        writer.WriteRaw(strTitle);
                                        writer.WriteEndElement();
                                    }

                                    if (string.IsNullOrEmpty(strComment) == false)
                                    {
                                        writer.WriteStartElement("div");
                                        writer.WriteAttributeString("class", "titlecomment");
                                        writer.WriteRaw(strComment);
                                        writer.WriteEndElement();
                                    }

                                    // writer.WriteRaw(reader.ReadOuterXml());
                                    // DumpNode(reader, writer);
                                    writer.WriteNode(reader, true);

                                    {
                                        writer.WriteStartElement("div");
                                        writer.WriteAttributeString("class", "createtime");
                                        writer.WriteString("创建时间: " + strCreateTime);
                                        writer.WriteEndElement();
                                    }

                                    writer.WriteEndElement();   // </body>
                                }
                            }
                        }

                        writer.WriteEndElement();   // </html>
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "RmlToHtml() 出现异常: " + ExceptionUtil.GetDebugText(ex)
                    + "\r\nstrRmlFileName=" + strRmlFileName
                    + "\r\nstrHtmlFileName=" + strHtmlFileName
                    + "\r\nstrCssTemplate=" + strCssTemplate;
                throw new Exception(strError);
            }
        }

        static Jurassic.ScriptEngine engine = null;


        // 输出 RML 格式的表格
        // 本函数负责写入 <table> 元素
        // parameters:
        //      nTopLines   顶部预留多少行
        public void OutputRmlTable(Table table,
            XmlTextWriter writer,
            int nMaxLines = -1)
        {
            // StringBuilder strResult = new StringBuilder(4096);
            int i, j;

            if (nMaxLines == -1)
                nMaxLines = table.Count;

            writer.WriteStartElement("table");
            writer.WriteAttributeString("class", "table");

            writer.WriteStartElement("thead");
            writer.WriteStartElement("tr");

            int nEvalCount = 0; // 具有 eval 的栏目个数
            for (j = 0; j < this.Count; j++)
            {
                PrintColumn column = (PrintColumn)this[j];
                if (column.Colspan == 0)
                    continue;

                if (string.IsNullOrEmpty(column.Eval) == false)
                    nEvalCount++;

                writer.WriteStartElement("th");
                if (string.IsNullOrEmpty(column.CssClass) == false)
                    writer.WriteAttributeString("class", column.CssClass);
                if (column.Colspan > 1)
                    writer.WriteAttributeString("colspan", column.Colspan.ToString());

                writer.WriteString(column.Title);
                writer.WriteEndElement();   // </th>
            }

            writer.WriteEndElement();   // </tr>
            writer.WriteEndElement();   // </thead>


            // 合计数组
            object[] sums = null;   // 2008/12/1 new changed

            if (this.SumLine)
            {
                sums = new object[this.Count];
                for (i = 0; i < sums.Length; i++)
                {
                    sums[i] = null;
                }
            }

            NumberFormatInfo nfi = new CultureInfo("zh-CN", false).NumberFormat;
            nfi.NumberDecimalDigits = 2;

            writer.WriteStartElement("tbody");

            // Jurassic.ScriptEngine engine = null;
            if (nEvalCount > 0 && engine == null)
            {
                engine = new Jurassic.ScriptEngine();
                engine.EnableExposedClrTypes = true;
            }

            // 内容行循环
            for (i = 0; i < Math.Min(nMaxLines, table.Count); i++)
            {
                Line line = table[i];

                if (engine != null)
                    engine.SetGlobalValue("line", line);

                string strLineCssClass = "content";
                if (this.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = i;
                    e.LineCssClass = strLineCssClass;
                    this.OutputLine(this, e);
                    if (e.Output == false)
                        continue;

                    strLineCssClass = e.LineCssClass;
                }

                // strResult.Append("<tr class='" + strLineCssClass + "'>\r\n");
                writer.WriteStartElement("tr");
                writer.WriteAttributeString("class", strLineCssClass);

                // 列循环
                for (j = 0; j < this.Count; j++)
                {
                    PrintColumn column = (PrintColumn)this[j];

                    if (column.ColumnNumber < -1)
                    {
                        throw (new Exception("PrintColumn对象ColumnNumber列尚未初始化，位置" + Convert.ToString(j)));
                    }

                    string strText = "";
                    if (column.ColumnNumber != -1)
                    {
                        if (string.IsNullOrEmpty(column.Eval) == false)
                        {
                            // engine.SetGlobalValue("cell", line.GetObject(column.ColumnNumber));
                            strText = engine.Evaluate(column.Eval).ToString();
                        }
                        else if (column.DataType == DataType.PriceDouble)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = line.GetDouble(column.ColumnNumber);
                                /*
                                NumberFormatInfo provider = new NumberFormatInfo();
                                provider.NumberDecimalDigits = 2;
                                provider.NumberGroupSeparator = ".";
                                provider.NumberGroupSizes = new int[] { 3 };
                                strText = Convert.ToString(v, provider);
                                 * */
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.Price)
                        {
                            // Debug.Assert(false, "");
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = line.GetPriceString(column.ColumnNumber);
                        }
                        else
                            strText = line.GetString(column.ColumnNumber, column.DefaultValue);
                    }
                    else
                    {
                        strText = line.Entry;
                    }

                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        writer.WriteAttributeString("class", column.CssClass);
                    writer.WriteString(strText);
                    writer.WriteEndElement();   // </td>

                    if (this.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {
                        try
                        {
                            // if (column.DataType != DataType.Currency)
                            {
                                object v = line.GetObject(column.ColumnNumber);
                                if (this.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    this.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;

                                    v = e.Value;
                                }

                                if (sums[j] == null)
                                    sums[j] = v;
                                else
                                {
                                    sums[j] = AddValue(column.DataType,
            sums[j],
            v);
                                    // sums[j] = ((decimal)sums[j]) + v;
                                }
                            }
                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + i.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，抛出异常: " + ex.Message);
                        }
                    }
                }

                // strResult.Append("</tr>\r\n");
                writer.WriteEndElement();   // </tr>
            }

            writer.WriteEndElement();   // </tbody>

            if (this.SumLine == true)
            {
                Line sum_line = null;
                if (engine != null)
                {
                    // 准备 Line 对象
                    sum_line = new Line(0);
                    for (j = 1; j < this.Count; j++)
                    {
                        PrintColumn column = (PrintColumn)this[j];
                        if (column.Sum == true
                            && sums[j] != null)
                        {
                            sum_line.SetValue(j - 1, sums[j]);
                        }
                    }
                    engine.SetGlobalValue("line", sum_line);
                }

                // strResult.Append("<tr class='sum'>\r\n");
                writer.WriteStartElement("tfoot");
                writer.WriteStartElement("tr");
                writer.WriteAttributeString("class", "sum");

                for (j = 0; j < this.Count; j++)
                {
                    PrintColumn column = (PrintColumn)this[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计";
                    else if (string.IsNullOrEmpty(column.Eval) == false)
                    {
                        strText = engine.Evaluate(column.Eval).ToString();
                    }
                    else if (column.Sum == true
                        && sums[j] != null)
                    {
                        if (column.DataType == DataType.PriceDouble)
                            strText = ((double)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.PriceDecimal)
                            strText = ((decimal)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.Price)
                        {
                            strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                        }
                        else
                            strText = Convert.ToString(sums[j]);

                        if (column.DataType == DataType.Currency)
                        {
                            string strSomPrice = "";
                            string strError = "";
                            // 汇总价格
                            int nRet = PriceUtil.SumPrices(strText,
            out strSomPrice,
            out strError);
                            if (nRet == -1)
                                strText = strError;
                            else
                                strText = strSomPrice;
                        }
                    }
                    else
                        strText = column.DefaultValue;  //  "&nbsp;";

#if NO
                    doc.WriteExcelCell(
    _lineIndex,
    j,
    strText,
    true);
#endif
                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        writer.WriteAttributeString("class", column.CssClass);
                    writer.WriteString(strText);
                    writer.WriteEndElement();   // </td>
                }

                // strResult.Append("</tr>\r\n");
                writer.WriteEndElement();   // </tr>
                writer.WriteEndElement();   // </tfoot>
            }

            writer.WriteEndElement();   // </table>
        }

        // 构造 Excel 表格的一些参数
        public class ExcelTableConfig
        {
            public string FontName { get; set; }    // 默认字体名
            public int StartRow { get; set; }   // 表格开始的行号。最小是 1
            public int StartCol { get; set; }   // 表格开始的列号。最小是 1
        }

        // return:
        //      -1  出错
        //      0   放弃或中断
        //      1   成功
        public int ExportToExcel(
            Table table,
            ExcelTableConfig config,
            IXLWorksheet sheet,
            out string strError)
        {
            strError = "";

            // 每个列的最大字符数
            List<int> column_max_chars = new List<int>();

            List<XLAlignmentHorizontalValues> alignments = new List<XLAlignmentHorizontalValues>();
            foreach (PrintColumn header in this)
            {
                alignments.Add(XLAlignmentHorizontalValues.Left);

                column_max_chars.Add(0);
            }


            // string strFontName = list.Font.FontFamily.Name;

            int nRowIndex = 0;
            //int nColIndex = 1;
            int i = 0;
            foreach (PrintColumn header in this)
            {
                IXLCell cell = sheet.Cell(config.StartRow + nRowIndex, config.StartCol + i).SetValue(DomUtil.ReplaceControlCharsButCrLf(header.Title, '*'));
                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                cell.Style.Font.Bold = true;
                if (string.IsNullOrEmpty(config.FontName) == false)
                    cell.Style.Font.FontName = config.FontName;
                cell.Style.Alignment.Horizontal = alignments[i];
                i++;
            }
            nRowIndex++;

            // 合计数组
            object[] sums = null;

            if (this.SumLine)
            {
                sums = new object[this.Count];
                for (i = 0; i < sums.Length; i++)
                {
                    sums[i] = null;
                }
            }

            NumberFormatInfo nfi = new CultureInfo("zh-CN", false).NumberFormat;
            nfi.NumberDecimalDigits = 2;

            for (int line_index = 0; line_index < table.Count; line_index++)
            {
                Line line = table[line_index];

                string strLineCssClass = "content";
                if (this.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = line_index;
                    e.LineCssClass = strLineCssClass;
                    this.OutputLine(this, e);
                    if (e.Output == false)
                        continue;

                    strLineCssClass = e.LineCssClass;
                }

                List<CellData> cells = new List<CellData>();

                // 列循环
                for (int j = 0; j < this.Count; j++)
                {

                    PrintColumn column = (PrintColumn)this[j];

                    if (column.ColumnNumber < -1)
                    {
                        throw (new Exception("PrintColumn对象ColumnNumber列尚未初始化，位置" + Convert.ToString(j)));
                    }


                    string strText = "";
                    if (column.ColumnNumber != -1)
                    {
                        if (column.DataType == DataType.PriceDouble)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = line.GetDouble(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.Price)
                        {
                            // Debug.Assert(false, "");
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = line.GetPriceString(column.ColumnNumber);
                        }
                        else
                            strText = line.GetString(column.ColumnNumber, column.DefaultValue);
                    }
                    else
                    {
                        strText = line.Entry;
                    }

#if NO
                    doc.WriteExcelCell(
    _lineIndex,
    j,
    strText,
    true);
#endif
                    {
                        // 统计最大字符数
                        int nChars = column_max_chars[j];
                        if (strText != null && strText.Length > nChars)
                        {
                            column_max_chars[j] = strText.Length;
                        }
                        IXLCell cell = sheet.Cell(config.StartRow + nRowIndex, config.StartCol + j).SetValue(DomUtil.ReplaceControlCharsButCrLf(strText, '*'));

                        // 尽可能用数字表达
                        if (j > 0 && (column.DataType == DataType.Auto || column.DataType == DataType.Number))
                        {
                            Int64 v = 0;
                            if (Int64.TryParse(strText, out v) == true)
                                cell.SetValue(v);
                        }

                        cell.Style.Alignment.WrapText = true;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        if (string.IsNullOrEmpty(config.FontName) == false)
                            cell.Style.Font.FontName = config.FontName;
                        cell.Style.Alignment.Horizontal = alignments[j];
                    }

                    if (this.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {
                        try
                        {
                            object v = line.GetObject(column.ColumnNumber);
                            if (this.SumCell != null)
                            {
                                SumCellEventArgs e = new SumCellEventArgs();
                                e.DataType = column.DataType;
                                e.ColumnNumber = column.ColumnNumber;
                                e.LineIndex = line_index;
                                e.Line = line;
                                e.Value = v;
                                this.SumCell(this, e);
                                if (e.Value == null)
                                    continue;

                                v = e.Value;
                            }

                            if (sums[j] == null)
                                sums[j] = v;
                            else
                            {
                                sums[j] = AddValue(column.DataType,
        sums[j],
        v);
                                // sums[j] = ((decimal)sums[j]) + v;
                            }
                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + line_index.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，抛出异常: " + ex.Message);
                        }
                    }

                }
                nRowIndex++;
            }

            // 合计 行
            if (this.SumLine == true)
            {
                for (int j = 0; j < this.Count; j++)
                {
                    PrintColumn column = (PrintColumn)this[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计";
                    else if (column.Sum == true
                        && sums[j] != null)
                    {
                        if (column.DataType == DataType.PriceDouble)
                            strText = ((double)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.PriceDecimal)
                            strText = ((decimal)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.Price)
                        {
                            strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                        }
                        else
                            strText = Convert.ToString(sums[j]);

                        if (column.DataType == DataType.Currency)
                        {
                            string strSumPrice = "";
                            // 汇总价格
                            int nRet = PriceUtil.SumPrices(strText,
            out strSumPrice,
            out strError);
                            if (nRet == -1)
                                strText = strError;
                            else
                                strText = strSumPrice;
                        }
                    }
                    else
                        strText = column.DefaultValue;  //  "&nbsp;";

                    IXLCell cell = sheet.Cell(config.StartRow + nRowIndex, config.StartCol + j).SetValue(DomUtil.ReplaceControlCharsButCrLf(strText, '*'));

                    // 尽可能用数字表达
                    if (j > 0 && (column.DataType == DataType.Auto || column.DataType == DataType.Number))
                    {
                        Int64 v = 0;
                        if (Int64.TryParse(strText, out v) == true)
                            cell.SetValue(v);
                    }
                    
                    cell.Style.Alignment.WrapText = true;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    if (string.IsNullOrEmpty(config.FontName) == false)
                        cell.Style.Font.FontName = config.FontName;
                    cell.Style.Alignment.Horizontal = alignments[j];
                }
            }

#if NO
            double char_width = 20; // ClosedXmlUtil.GetAverageCharPixelWidth(list);

            // 字符数太多的列不要做 width auto adjust
            const int MAX_CHARS = 30;   // 60
            int i = 0;
            foreach (IXLColumn column in sheet.Columns())
            {
                int nChars = column_max_chars[i];
                if (nChars < MAX_CHARS)
                    column.AdjustToContents();
                else
                    column.Width = (double)list.Columns[i].Width / char_width;  // Math.Min(MAX_CHARS, nChars);
                i++;
            }
#endif

            // sheet.Columns().AdjustToContents();

            // sheet.Rows().AdjustToContents();

            return 1;
        }

        // 输出 Excel 格式的表格
        // parameters:
        //      nTopLines   顶部预留多少行
        public void OutputExcelTable(Table table,
            ExcelDocument doc,
            int nTopLines,
            int nMaxLines = -1)
        {
            // StringBuilder strResult = new StringBuilder(4096);
            int i, j;

            if (nMaxLines == -1)
                nMaxLines = table.Count;

            int _lineIndex = nTopLines;

            // 表格标题
            // strResult.Append("<tr class='column'>\r\n");

            int nColIndex = 0;
            for (j = 0; j < this.Count; j++)
            {
                PrintColumn column = (PrintColumn)this[j];
                if (column.Colspan == 0)
                    continue;

                if (column.Colspan > 1)
                {
                    doc.WriteExcelTitle(_lineIndex,
            nColIndex,
            column.Colspan,
            column.Title);
#if NO
                    cells.Add(new CellData(nColIndex, column.Title));
#endif
                    nColIndex += column.Colspan;
                }
                else
                {
                    doc.WriteExcelCell(
_lineIndex,
nColIndex++,
column.Title,
true);
#if NO
                    cells.Add(new CellData(nColIndex, column.Title));
#endif
                }
            }

#if NO
            if (cells.Count > 0)
                doc.WriteExcelLine(_lineIndex, cells);
#endif

            // 合计数组
            object[] sums = null;   // 2008/12/1 new changed

            if (this.SumLine)
            {
                sums = new object[this.Count];
                for (i = 0; i < sums.Length; i++)
                {
                    sums[i] = null;
                }
            }

            NumberFormatInfo nfi = new CultureInfo("zh-CN", false).NumberFormat;
            nfi.NumberDecimalDigits = 2;

            // 内容行循环
            for (i = 0; i < Math.Min(nMaxLines, table.Count); i++)
            {
                Line line = table[i];

                string strLineCssClass = "content";
                if (this.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = i;
                    e.LineCssClass = strLineCssClass;
                    this.OutputLine(this, e);
                    if (e.Output == false)
                        continue;

                    strLineCssClass = e.LineCssClass;
                }

                // strResult.Append("<tr class='" + strLineCssClass + "'>\r\n");
                _lineIndex++;

                List<CellData> cells = new List<CellData>();

                // 列循环
                for (j = 0; j < this.Count; j++)
                {

                    PrintColumn column = (PrintColumn)this[j];

                    if (column.ColumnNumber < -1)
                    {
                        throw (new Exception("PrintColumn对象ColumnNumber列尚未初始化，位置" + Convert.ToString(j)));
                    }


                    string strText = "";
                    if (column.ColumnNumber != -1)
                    {
                        if (column.DataType == DataType.PriceDouble)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = line.GetDouble(column.ColumnNumber);
                                /*
                                NumberFormatInfo provider = new NumberFormatInfo();
                                provider.NumberDecimalDigits = 2;
                                provider.NumberGroupSeparator = ".";
                                provider.NumberGroupSizes = new int[] { 3 };
                                strText = Convert.ToString(v, provider);
                                 * */
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.Price)
                        {
                            // Debug.Assert(false, "");
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = line.GetPriceString(column.ColumnNumber);
                        }
                        else
                            strText = line.GetString(column.ColumnNumber, column.DefaultValue);
                    }
                    else
                    {
                        strText = line.Entry;
                    }

#if NO
                    doc.WriteExcelCell(
    _lineIndex,
    j,
    strText,
    true);
#endif
                    cells.Add(new CellData(j, strText));

                    if (this.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {
                        try
                        {
                            // if (column.DataType != DataType.Currency)
                            {
                                object v = line.GetObject(column.ColumnNumber);
                                if (this.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    this.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;

                                    v = e.Value;
                                }

                                if (sums[j] == null)
                                    sums[j] = v;
                                else
                                {
                                    sums[j] = AddValue(column.DataType,
            sums[j],
            v);
                                    // sums[j] = ((decimal)sums[j]) + v;
                                }
                            }
                            /*
                        else
                        {
                            string v = line.GetString(column.ColumnNumber);
                            if (this.SumCell != null)
                            {
                                SumCellEventArgs e = new SumCellEventArgs();
                                e.DataType = column.DataType;
                                e.ColumnNumber = column.ColumnNumber;
                                e.LineIndex = i;
                                e.Line = line;
                                e.Value = v;
                                this.SumCell(this, e);
                                if (e.Value == null)
                                    continue;
                                v = (string)e.Value;
                            }
                            sums[j] = PriceUtil.JoinPriceString((string)sums[j],
                                v);
                        }
                             * */
                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + i.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，抛出异常: " + ex.Message);
                        }
                    }


                }

                // strResult.Append("</tr>\r\n");
                doc.WriteExcelLine(_lineIndex, cells);
            }

            if (this.SumLine == true)
            {
                // strResult.Append("<tr class='sum'>\r\n");
                _lineIndex++;
                List<CellData> cells = new List<CellData>();
                for (j = 0; j < this.Count; j++)
                {
                    PrintColumn column = (PrintColumn)this[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计";
                    else if (column.Sum == true
                        && sums[j] != null)
                    {
                        if (column.DataType == DataType.PriceDouble)
                            strText = ((double)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.PriceDecimal)
                            strText = ((decimal)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.Price)
                        {
                            strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                        }
                        else
                            strText = Convert.ToString(sums[j]);

                        if (column.DataType == DataType.Currency)
                        {
                            string strSomPrice = "";
                            string strError = "";
                            // 汇总价格
                            int nRet = PriceUtil.SumPrices(strText,
            out strSomPrice,
            out strError);
                            if (nRet == -1)
                                strText = strError;
                            else
                                strText = strSomPrice;
                        }
                    }
                    else
                        strText = column.DefaultValue;  //  "&nbsp;";

#if NO
                    doc.WriteExcelCell(
    _lineIndex,
    j,
    strText,
    true);
#endif
                    cells.Add(new CellData(j, strText));

                }

                // strResult.Append("</tr>\r\n");
                doc.WriteExcelLine(_lineIndex, cells);
            }
        }

        // 输出html格式的表格
        public string HtmlTable(Table table,
            int nMaxLines = -1)
        {
            StringBuilder strResult = new StringBuilder(4096);
            int i, j;

            if (nMaxLines == -1)
                nMaxLines = table.Count;

            strResult.Append("<table class='table'>\r\n");    //  border='0' bgcolor=Gainsboro cellspacing='2' cellpadding='2'

            // 表格标题
            strResult.Append("<tr class='column'>\r\n");

            for (j = 0; j < this.Count; j++)
            {
                PrintColumn column = (PrintColumn)this[j];
                if (column.Colspan == 0)
                    continue;

                string strText = column.Title;
                string strColspan = "";
                if (column.Colspan > 1)
                    strColspan = " colspan='" + column.Colspan.ToString() + "' ";
                strResult.Append("<td class='"
                    + column.CssClass
                    + "'"
                    + strColspan + ">" + strText + "</td>\r\n");
            }

            strResult.Append("</tr>\r\n");

            // 合计数组
            object[] sums = null;   // 2008/12/1 new changed

            if (this.SumLine)
            {
                sums = new object[this.Count];
                for (i = 0; i < sums.Length; i++)
                {
                    sums[i] = null;
                }
            }

            NumberFormatInfo nfi = new CultureInfo("zh-CN", false).NumberFormat;
            nfi.NumberDecimalDigits = 2;

            // 内容行循环
            for (i = 0; i < Math.Min(nMaxLines, table.Count); i++)
            {
                Line line = table[i];

                string strLineCssClass = "content";
                if (this.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = i;
                    e.LineCssClass = strLineCssClass;
                    this.OutputLine(this, e);
                    if (e.Output == false)
                        continue;

                    strLineCssClass = e.LineCssClass;
                }

                strResult.Append("<tr class='" + strLineCssClass + "'>\r\n");

                // 列循环
                for (j = 0; j < this.Count; j++)
                {

                    PrintColumn column = (PrintColumn)this[j];

                    if (column.ColumnNumber < -1)
                    {
                        throw (new Exception("PrintColumn对象ColumnNumber列尚未初始化，位置" + Convert.ToString(j)));
                    }


                    string strText = "";
                    if (column.ColumnNumber != -1)
                    {
#if NO
                        if (column.ColumnNumber == 9)
                            Debug.Assert(false, "");
#endif

                        if (column.DataType == DataType.PriceDouble)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = line.GetDouble(column.ColumnNumber);
                                /*
                                NumberFormatInfo provider = new NumberFormatInfo();
                                provider.NumberDecimalDigits = 2;
                                provider.NumberGroupSeparator = ".";
                                provider.NumberGroupSizes = new int[] { 3 };
                                strText = Convert.ToString(v, provider);
                                 * */
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = line.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.Price)
                        {
                            // Debug.Assert(false, "");
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = line.GetPriceString(column.ColumnNumber);
                        }
                        else
                            strText = line.GetString(column.ColumnNumber, column.DefaultValue);

                    }
                    else
                    {
                        strText = line.Entry;
                    }


                    strResult.Append("<td class='"
                        + column.CssClass
                        + "'>" + strText + "</td>\r\n");

                    if (this.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {
                        try
                        {
                            // if (column.DataType != DataType.Currency)
                            {
                                object v = line.GetObject(column.ColumnNumber);
                                if (this.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    this.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;

                                    v = e.Value;
                                }

                                if (sums[j] == null)
                                    sums[j] = v;
                                else
                                {
                                    sums[j] = AddValue(column.DataType,
            sums[j],
            v);
                                    // sums[j] = ((decimal)sums[j]) + v;
                                }
                            }
                            /*
                        else
                        {
                            string v = line.GetString(column.ColumnNumber);
                            if (this.SumCell != null)
                            {
                                SumCellEventArgs e = new SumCellEventArgs();
                                e.DataType = column.DataType;
                                e.ColumnNumber = column.ColumnNumber;
                                e.LineIndex = i;
                                e.Line = line;
                                e.Value = v;
                                this.SumCell(this, e);
                                if (e.Value == null)
                                    continue;
                                v = (string)e.Value;
                            }
                            sums[j] = PriceUtil.JoinPriceString((string)sums[j],
                                v);
                        }
                             * */
                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + i.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，抛出异常: " + ex.Message);
                        }
                    }
                }

                strResult.Append("</tr>\r\n");
            }

            if (this.SumLine == true)
            {
                strResult.Append("<tr class='sum'>\r\n");

                for (j = 0; j < this.Count; j++)
                {
                    PrintColumn column = (PrintColumn)this[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计";
                    else if (column.Sum == true
                        && sums[j] != null)
                    {
                        if (column.DataType == DataType.PriceDouble)
                            strText = ((double)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.PriceDecimal)
                            strText = ((decimal)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.Price)
                        {
                            strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                        }
                        else
                            strText = Convert.ToString(sums[j]);

                        if (column.DataType == DataType.Currency)
                        {
                            string strSomPrice = "";
                            string strError = "";
                            // 汇总价格
                            int nRet = PriceUtil.SumPrices(strText,
                                out strSomPrice,
                                out strError);
                            if (nRet == -1)
                                strText = strError;
                            else
                                strText = strSomPrice;
                        }
                    }
                    else
                        strText = column.DefaultValue;  //  "&nbsp;";

                    strResult.Append("<td class='"
                        + column.CssClass
                        + "'>" + strText + "</td>\r\n");
                }

                strResult.Append("</tr>\r\n");
            }

            strResult.Append("</table>\r\n");
            return strResult.ToString();
        }

        object AddValue(DataType datatype,
            object o1,
            object o2)
        {
            if (o1 == null && o2 == null)
                return null;
            if (o1 == null)
                return o2;
            if (o2 == null)
                return o1;
            if (datatype == DataType.Auto)
            {
                if (o1 is Int64)
                    return (Int64)o1 + (Int64)o2;
                if (o1 is Int32)
                    return (Int32)o1 + (Int32)o2;
                if (o1 is double)
                    return (double)o1 + (double)o2;
                if (o1 is decimal)
                    return (decimal)o1 + (decimal)o2;
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 Auto 类型累加");
            }
            if (datatype == DataType.Number)
            {
                if (o1 is Int64)
                    return (Int64)o1 + (Int64)o2;
                if (o1 is Int32)
                    return (Int32)o1 + (Int32)o2;
                if (o1 is double)
                    return (double)o1 + (double)o2;
                if (o1 is decimal)
                    return (decimal)o1 + (decimal)o2;

                throw new Exception("无法支持的 Number 类型累加");
            }
            if (datatype == DataType.String)
            {
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 String 类型累加");
            }
            if (datatype == DataType.Price) // 100倍金额整数
            {
                return (Int64)o1 + (Int64)o2;
            }
            if (datatype == DataType.PriceDouble)  // double，用来表示金额。也就是最多只有两位小数部分 -- 注意，有累计误差问题，以后建议废止
            {
                return (double)o1 + (double)o2;
            }
            if (datatype == DataType.PriceDecimal) // decimal，用来表示金额。
            {
                return (decimal)o1 + (decimal)o2;
            }
            if (datatype == DataType.Currency)
            {
                // 这一举容易发现列 数据类型 的错误
                return PriceUtil.JoinPriceString((string)o1,
                    (string)o2);
#if NO
                // 这一句更健壮一些
                return PriceUtil.JoinPriceString(Convert.ToString(o1),
                    Convert.ToString(o2));
#endif
            }
            throw new Exception("无法支持的 " + datatype.ToString() + " 类型累加");
        }

#if NO
        // 合并比率
        // 100% + 100% = 100%
        // 50% + 50% = 25%
        static string JoinPercentString(string s1, string s2)
        {
            string t1 = s1.Replace("%", "");
            string t2 = s2.Replace("%", "");

            if (string.IsNullOrEmpty(t1) == true
                && string.IsNullOrEmpty(t2) == true)
                return "";
            if (string.IsNullOrEmpty(t1) == true)
                return t2;
            if (string.IsNullOrEmpty(t2) == true)
                return t1;

            Decimal v1 = 0;
            Decimal v2 = 0;

            if (decimal.TryParse(t1, out v1) == false)
                return t2;
            if (decimal.TryParse(t2, out v2) == false)
                return t1;

            return (v1 + v2 / (decimal)200).ToString();
        }
#endif

        // 输出text格式的表格
        public string TextTable(Table table,
            int nMaxLines = -1)
        {
            StringBuilder strResult = new StringBuilder(4096);
            int i, j;

            if (nMaxLines == -1)
                nMaxLines = table.Count;
            // 表格标题
            for (j = 0; j < this.Count; j++)
            {
                PrintColumn column = (PrintColumn)this[j];
                string strText = column.Title;

                if (column.Colspan == 0)
                    strText = "";   // tab 字符不减少

                if (j != 0)
                    strResult.Append("\t");
                strResult.Append(strText);
            }

            strResult.Append("\r\n");

            // 合计数组
            object[] sums = null;

            if (this.SumLine)
            {
                sums = new object[this.Count];
                for (i = 0; i < sums.Length; i++)
                {
                    sums[i] = null;
                }
            }

            // 内容行循环
            for (i = 0; i < Math.Min(nMaxLines, table.Count); i++)
            {
                Line line = table[i];

                if (this.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = i;
                    this.OutputLine(this, e);
                    if (e.Output == false)
                        continue;
                }

                // 列循环
                for (j = 0; j < this.Count; j++)
                {

                    PrintColumn column = (PrintColumn)this[j];

                    if (column.ColumnNumber < -1)
                    {
                        throw (new Exception("PrintColumn对象ColumnNumber列尚未初始化，位置" + Convert.ToString(j)));
                    }

                    string strText = "";
                    if (column.ColumnNumber != -1)
                    {
                        if (column.DataType == DataType.Price)
                        {
                            if (line.IsNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = line.GetPriceString(column.ColumnNumber);
                        }
                        else
                            strText = line.GetString(column.ColumnNumber, column.DefaultValue);
                    }
                    else
                    {
                        strText = line.Entry;
                    }

                    if (j != 0)
                        strResult.Append("\t");
                    strResult.Append(strText);

                    if (this.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {

                        /*
                        try
                        {
                            sums[j] += line.GetDouble(column.ColumnNumber);
                        }
                        catch	// 俘获可能因字符串转换为数值抛出的异常
                        {
                        }*/

                        try
                        {
                            // if (column.DataType != DataType.Currency)
                            {
                                object v = line.GetObject(column.ColumnNumber);
                                if (this.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    this.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;
                                    v = (decimal)e.Value;
                                }
                                if (sums[j] == null)
                                    sums[j] = v;
                                else
                                    sums[j] = AddValue(column.DataType,
            sums[j],
            v);
                            }
                            /*
                            else
                            {
                                string v = line.GetString(column.ColumnNumber);
                                if (this.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    this.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;
                                    v = (string)e.Value;
                                }
                                sums[j] = PriceUtil.JoinPriceString((string)sums[j],
                                    v);
                            }
                             * */

                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + i.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，抛出异常: " + ex.Message);
                        }

                    }


                }

                strResult.Append("\r\n");

            }

            if (this.SumLine == true)
            {
                for (j = 0; j < this.Count; j++)
                {
                    PrintColumn column = (PrintColumn)this[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计";
                    else if (column.Sum == true
                        && sums[j] != null)
                    {
                        if (column.DataType == DataType.Price)
                            strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                        else
                            strText = Convert.ToString(sums[j]);

                        if (column.DataType == DataType.Currency)
                        {
                            string strSomPrice = "";
                            string strError = "";
                            // 汇总价格
                            int nRet = PriceUtil.SumPrices(strText,
            out strSomPrice,
            out strError);
                            if (nRet == -1)
                                strText = strError;
                            else
                                strText = strSomPrice;
                        }
                    }
                    else
                        strText = " ";

                    if (j != 0)
                        strResult.Append("\t");
                    strResult.Append(strText);
                }

                strResult.Append("\r\n");
            }

            return strResult.ToString();
        }

        public string OutputToExcel(Table table)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要输出的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            // dlg.FileName = this.ExportExcelFilename;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return null;

            // this.ExportExcelFilename = dlg.FileName;

            ExcelDocument doc = ExcelDocument.Create(dlg.FileName);
            try
            {
                doc.Stylesheet = GenerateStyleSheet();

                this.OutputExcelTable(table, doc, 2);
            }
            finally
            {
                doc.Close();
            }

            return dlg.FileName;
        }

        private static Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 0 - The default font.
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 1 - The bold font.
                        new Bold(),
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 2 - The Italic font.
                        new Italic(),
                        new FontSize() { Val = 11 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" }),
                    new DocumentFormat.OpenXml.Spreadsheet.Font(                                                               // Index 2 - The Times Roman font. with 16 size
                        new FontSize() { Val = 16 },
                        new DocumentFormat.OpenXml.Spreadsheet.Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Times New Roman" })
                ),
                new Fills(
                    new Fill(                                                           // Index 0 - The default fill.
                        new PatternFill() { PatternType = PatternValues.None }),
                    new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                        new PatternFill() { PatternType = PatternValues.Gray125 }),
                    new Fill(                                                           // Index 2 - The yellow fill.
                        new PatternFill(
                            new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
                        ) { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(                                                         // Index 0 - The default border.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()),
                    new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                        new LeftBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new RightBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new TopBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new BottomBorder(
                            new DocumentFormat.OpenXml.Spreadsheet.Color() { Auto = true }
                        ) { Style = BorderStyleValues.Thin },
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                          // Index 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                    new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 1 - Bold 
                    new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 2 - Italic
                    new CellFormat() { FontId = 3, FillId = 0, BorderId = 0, ApplyFont = true },       // Index 3 - Times Roman
                    new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true },       // Index 4 - Yellow Fill
                    new CellFormat(                                                                   // Index 5 - Alignment
                        new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                    ) { /*FontId = 1, FillId = 0, BorderId = 0, */ApplyAlignment = true },
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }      // Index 6 - Border
                )
            ); // return
        }

    }

    // 合计累加阶段的 每一次累加
    public delegate void SumCellEventHandler(object sender,
        SumCellEventArgs e);

    public class SumCellEventArgs : EventArgs
    {
        public int ColumnNumber = 0; // [in] 列号。从0开始编号
        public object Value = 0;    // [in,out] 单元值。调用中修改为0，可以实现忽略该单元的作用
        public long LineIndex = 0;  // [in]行号
        public Line Line = null;    // [in]行对象。用Line.Entry可以获得事项名
        public DataType DataType = DataType.Auto;
    }

    // 输出表格行 每一次
    public delegate void OutputLineEventHandler(object sender,
        OutputLineEventArgs e);

    public class OutputLineEventArgs : EventArgs
    {
        public int Index = -1;      // [in] 行的index
        public Line Line = null;    // [in] 当前准备输出操作的行对象
        public string LineCssClass = "";    // [in][out] 行的css class内容
        public bool Output = true;  // [out] 是否需要输出
    }
}
