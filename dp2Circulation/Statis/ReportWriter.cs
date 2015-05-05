using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
// using System.Data.SQLite;
using System.Collections;
using System.Diagnostics;
using DigitalPlatform.IO;
using System.Globalization;
using System.Data.Common;

using DigitalPlatform.dp2.Statis;
using DigitalPlatform.Text;
using System.Data;
using System.Collections.Specialized;
using DigitalPlatform.Xml;
using System.Data.SQLite;

namespace dp2Circulation
{
    /// <summary>
    /// 用于创建报表的类
    /// </summary>
    public class ReportWriter
    {
        public string CfgFile = "";

        // 报表配置文件 XMLDOM
        XmlDocument _cfgDom = null;

        XmlDocument columns_dom = null;

        /// <summary>
        /// 栏目定义
        /// </summary>
        public List<PrintColumn000> Columns = new List<PrintColumn000>();

        public bool SumLine = true;

        public int Initial(string strCfgFile,
            out string strError)
        {
            strError = "";

            this.CfgFile = strCfgFile;

            this._cfgDom = new XmlDocument();
            try
            {
                _cfgDom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return -1;
            }


            XmlNodeList nodes = _cfgDom.DocumentElement.SelectNodes("columns/column");

            if (nodes.Count > 0)
            {
                columns_dom = new XmlDocument();
                columns_dom.AppendChild(columns_dom.CreateElement("columns"));
            }

            {
                int i = 0;
                foreach (XmlElement node in nodes)
                {
                    string strName = node.GetAttribute("name");
                    string strType = node.GetAttribute("type");
                    string strAlign = node.GetAttribute("align");
                    string strSum = node.GetAttribute("sum");
                    string strClass = node.GetAttribute("class");
                    string strEval = node.GetAttribute("eval");

                    PrintColumn000 col = new PrintColumn000();
                    this.Columns.Add(col);

                    col.ColumnNumber = i;
                    col.Title = strName;
                    col.Eval = strEval;

                    if (string.IsNullOrEmpty(strClass) == true)
                        strClass = "c" + (i + 1).ToString();
                    col.CssClass = strClass;

                    col.Sum = StringUtil.GetBooleanValue(strSum, false);

                    col.DataType = GetColumnDataType(strType);

                    XmlElement column = columns_dom.CreateElement("column");
                    columns_dom.DocumentElement.AppendChild(column);
                    column.SetAttribute("class", strClass);
                    column.SetAttribute("align", strAlign);
                    if (string.IsNullOrEmpty(strType) == false)
                        column.SetAttribute("type", strType);
                    i++;
                }
            }

#if NO
            string strTitle = _cfgDom.DocumentElement.GetAttribute("title").Replace("\\r", "\r");
            strTitle = Global.MacroString(macro_table, strTitle);

            string strComment = _cfgDom.DocumentElement.GetAttribute("titleComment").Replace("\\r", "\r");
            strComment = Global.MacroString(macro_table, strComment);
#endif

            return 0;
        }

        static ColumnDataType GetColumnDataType(string strType)
        {
            ColumnDataType result = ColumnDataType.Auto;
            if (string.IsNullOrEmpty(strType) == true)
                return result;

            Enum.TryParse<ColumnDataType>(strType, true, out result);
            return result;
        }

        // 从报表配置文件中获得 <columnSortStyle> 元素文本值
        public string GetColumnSortStyle()
        {
            return DomUtil.GetElementText(this._cfgDom.DocumentElement,
                "columnSortStyle");
        }

        public bool GetFresh()
        {
            XmlNode nodeProperty = this._cfgDom.DocumentElement.SelectSingleNode("property");
            if (nodeProperty != null)
            {
                string strError = "";
                bool bValue = false;
                int nRet = DomUtil.GetBooleanParam(nodeProperty,
                    "fresh",
                    false,
                    out bValue,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
                return bValue;
            }
            return false;
        }
#if NO
        // 根据一个表格按照缺省特性创建一个Report对象
        // parameters:
        //		strDefaultValue	全部列的缺省值
        //				null表示不改变缺省值""，否则为strDefaultValue指定的值
        //		bSum	是否全部列都要参加合计
        //      bContentColumn  是否考虑内容行中比指定的栏目多出来的栏目
        public static Report BuildReport(SQLiteDataReader table,
            string strColumnTitles,
            string strDefaultValue,
            bool bSum,
            bool bContentColumn = true)
        {
            // Debug.Assert(false, "");
            if (table.HasRows == false)
                return null;	// 无法创建。内容必须至少一行以上

            Report report = new Report();

            // Line line = table.FirstHashLine();	// 随便得到一行。这样不要求table排过序

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
                nColumnCount = Math.Max(table.FieldCount, nTitleCount);


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
                    // 2007/10/26 new add
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

#endif

        public int OutputRmlReport(
            DbDataReader data_reader, 
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            string strTitle = DomUtil.GetElementText(_cfgDom.DocumentElement, "title").Replace("\\r", "\r");
            strTitle = Global.MacroString(macro_table, strTitle);

            string strComment = DomUtil.GetElementText(_cfgDom.DocumentElement, "titleComment").Replace("\\r", "\r");
            strComment = Global.MacroString(macro_table, strComment);


            string strCreateTime = DateTime.Now.ToString();

            string strCssContent = this._cfgDom.DocumentElement.GetAttribute("css").Replace("\\r", "\r\n").Replace("\\t", "\t");

            // 写入输出文件
            if (string.IsNullOrEmpty(strOutputFileName) == true)
            {
                Debug.Assert(false, "");
                // strOutputFileName = this.NewOutputFileName();
            }
            else
            {
                // 确保目录被创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
            }

            try
            {
                using (XmlTextWriter writer = new XmlTextWriter(strOutputFileName, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    writer.WriteStartDocument();
                    writer.WriteStartElement("report");
                    writer.WriteAttributeString("version", "0.01");

                    if (string.IsNullOrEmpty(strTitle) == false)
                    {
                        writer.WriteStartElement("title");
                        WriteTitles(writer, strTitle);
                        writer.WriteEndElement();
                    }

                    if (string.IsNullOrEmpty(strComment) == false)
                    {
                        writer.WriteStartElement("comment");
                        WriteTitles(writer, strComment);
                        writer.WriteEndElement();
                    }

                    if (string.IsNullOrEmpty(strCreateTime) == false)
                    {
                        writer.WriteStartElement("createTime");
                        writer.WriteString(strCreateTime);
                        writer.WriteEndElement();
                    }

                    if (string.IsNullOrEmpty(strCssContent) == false)
                    {
                        writer.WriteStartElement("style");
                        writer.WriteCData("\r\n" + strCssContent + "\r\n");
                        writer.WriteEndElement();
                    }

                    // XmlNode node = dom.DocumentElement.SelectSingleNode("columns");
                    if (columns_dom != null && columns_dom.DocumentElement != null)
                        columns_dom.DocumentElement.WriteTo(writer);

                    OutputRmlTable(
                        data_reader,
                        writer);

                    writer.WriteEndElement();   // </report>
                    writer.WriteEndDocument();
                }
            }
            catch (Exception ex)
            {
                strError = "写入文件 '" + strOutputFileName + "' 的过程中出现错误: " + ex.Message;
                return -1;
            }

            // TODO: 没有必要?
            File.SetAttributes(strOutputFileName, FileAttributes.Archive);

#if NO
            string strHtmlFileName = Path.Combine(Path.GetDirectoryName(strOutputFileName), Path.GetFileNameWithoutExtension(strOutputFileName) + ".html");
            int nRet = Report.RmlToHtml(strOutputFileName,
                strHtmlFileName,
                out strError);
            if (nRet == -1)
                return -1;
#endif

            return 1;
        }

        static Jurassic.ScriptEngine engine = null;

        // 输出 RML 格式的表格
        // 本函数负责写入 <table> 元素
        // parameters:
        //      nTopLines   顶部预留多少行
        void OutputRmlTable(
            DbDataReader data_reader,
            XmlTextWriter writer,
            int nMaxLines = -1)
        {
            // StringBuilder strResult = new StringBuilder(4096);
            int i, j;

#if NO
            if (nMaxLines == -1)
                nMaxLines = table.Count;
#endif

            writer.WriteStartElement("table");
            writer.WriteAttributeString("class", "table");

            writer.WriteStartElement("thead");
            writer.WriteStartElement("tr");

            int nEvalCount = 0; // 具有 eval 的栏目个数
            for (j = 0; j < this.Columns.Count; j++)
            {
                PrintColumn000 column = this.Columns[j];
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
                sums = new object[this.Columns.Count];
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

            int nLineCount = 0;

            // 内容行循环
            for (i = 0; ; i++)  // i < Math.Min(nMaxLines, table.Count)
            {
                if (data_reader.Read() == false)
                {
                    if (data_reader.NextResult() == false)
                        break;

                    if (data_reader.Read() == false)
                        break;
                }

                nLineCount++;
#if NO
                if (table.HasRows == false)
                    break;
#endif
                // Line line = table[i];

                if (engine != null)
                    engine.SetGlobalValue("line", data_reader);

                string strLineCssClass = "content";
#if NO
                if (report.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = i;
                    e.LineCssClass = strLineCssClass;
                    report.OutputLine(this, e);
                    if (e.Output == false)
                        continue;

                    strLineCssClass = e.LineCssClass;
                }
#endif

                // strResult.Append("<tr class='" + strLineCssClass + "'>\r\n");
                writer.WriteStartElement("tr");
                writer.WriteAttributeString("class", strLineCssClass);

                // 列循环
                for (j = 0; j < this.Columns.Count; j++)
                {
                    PrintColumn000 column = this.Columns[j];

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
                            engine.SetGlobalValue("rowNumber", nLineCount.ToString());
                            strText = engine.Evaluate(column.Eval).ToString();
                        }
                        else if (column.DataType == ColumnDataType.PriceDouble)
                        {
                            if (data_reader.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = data_reader.GetDouble(column.ColumnNumber);
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
                        else if (column.DataType == ColumnDataType.PriceDecimal)
                        {
                            if (data_reader.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = data_reader.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == ColumnDataType.PriceDecimal)
                        {
                            if (data_reader.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = data_reader.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == ColumnDataType.Price)
                        {
                            // Debug.Assert(false, "");
                            if (data_reader.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = data_reader.GetString(column.ColumnNumber);    // 
                        }
                        else if (column.DataType == ColumnDataType.String)
                        {
                            // strText = data_reader.GetString(column.ColumnNumber/*, column.DefaultValue*/);
                            // 2014/8/28
                            object o = data_reader.GetValue(column.ColumnNumber);
                            if (o != null)
                                strText = o.ToString();
                            else
                                strText = "";
                        }
                        else
                        {
                            object o = data_reader.GetValue(column.ColumnNumber);
                            if (o != null)
                                strText = o.ToString();
                            else
                                strText = "";
                        }
                    }
                    else
                    {
                        strText = data_reader.GetString(0);   // line.Entry;
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
                                object v = null;
                                    
                                if (column.ColumnNumber < data_reader.FieldCount)
                                    v = data_reader.GetValue(column.ColumnNumber);
#if NO
                                if (report.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    report.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;

                                    v = e.Value;
                                }
#endif

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
                SumLineReader sum_line = null;
                if (engine != null)
                {
                    // 准备 Line 对象
                    sum_line = new SumLineReader();
                    sum_line.FieldValues = sums;
                    sum_line.Read();
#if NO
                    for (j = 1; j < this.Columns.Count; j++)
                    {
                        PrintColumn000 column = this.Columns[j];
                        if (column.Sum == true
                            && sums[j] != null)
                        {
                            sum_line.SetValue(j - 1, sums[j]);
                        }
                    }
#endif
                    engine.SetGlobalValue("line", sum_line);
                    engine.SetGlobalValue("rowNumber", "");
                }

                // strResult.Append("<tr class='sum'>\r\n");
                writer.WriteStartElement("tfoot");
                writer.WriteStartElement("tr");
                writer.WriteAttributeString("class", "sum");

                for (j = 0; j < this.Columns.Count; j++)
                {
                    PrintColumn000 column = this.Columns[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计(" + nLineCount.ToString() + "行)";
                    else if (column.Sum == true)
                    {

                        if (string.IsNullOrEmpty(column.Eval) == false)
                        {
                            strText = engine.Evaluate(column.Eval).ToString();
                        }
                        else if (column.Sum == true
                            && sums[j] != null)
                        {
                            if (column.DataType == ColumnDataType.PriceDouble)
                                strText = ((double)sums[j]).ToString("N", nfi);
                            else if (column.DataType == ColumnDataType.PriceDecimal)
                                strText = ((decimal)sums[j]).ToString("N", nfi);
                            else if (column.DataType == ColumnDataType.Price)
                            {
                                strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                            }
                            else
                                strText = Convert.ToString(sums[j]);

                            if (column.DataType == ColumnDataType.Currency)
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

                    }

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

        object AddValue(ColumnDataType datatype,
object o1,
object o2)
        {
            if (o1 == null && o2 == null)
                return null;
            if (o1 == null)
                return o2;
            if (o2 == null)
                return o1;
            if (datatype == ColumnDataType.Auto)
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
            if (datatype == ColumnDataType.Number)
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
            if (datatype == ColumnDataType.String)
            {
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 String 类型累加");
            }
            if (datatype == ColumnDataType.Price) // 100倍金额整数
            {
                return (Int64)o1 + (Int64)o2;
            }
            if (datatype == ColumnDataType.PriceDouble)  // double，用来表示金额。也就是最多只有两位小数部分 -- 注意，有累计误差问题，以后建议废止
            {
                return (double)o1 + (double)o2;
            }
            if (datatype == ColumnDataType.PriceDecimal) // decimal，用来表示金额。
            {
                return (decimal)o1 + (decimal)o2;
            }
            if (datatype == ColumnDataType.Currency)
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

        static void WriteTitles(XmlTextWriter writer,
    string strTitleString)
        {
            List<string> titles = StringUtil.SplitList(strTitleString, '\r');
            WriteTitles(writer, titles);
        }

        static void WriteTitles(XmlTextWriter writer,
    List<string> titles)
        {
            int i = 0;
            foreach (string title in titles)
            {
                if (i > 0)
                    writer.WriteElementString("br", "");
                writer.WriteString(title);
                i++;
            }
        }
    }

    // 列数据类型
    public enum ColumnDataType
    {
        Auto = 0,
        String = 1,
        Number = 2,
        Price = 3,// 100倍金额整数
        PriceDouble = 4,    // double，用来表示金额。也就是最多只有两位小数部分 -- 注意，有累计误差问题，以后建议废止
        PriceDecimal = 5,   // decimal，用来表示金额。
        Currency = 6,   // 货币字符串。带有货币单位的字符串，可能是若干个子串连接起来的
        RecPath = 7,    // 记录路径，短的形式。例如“中文图书/1”
    }

    // 一个列的格式输出属性
    public class PrintColumn000
    {
        public string Title = "";	// 列标题

        string m_strCssClass = "";

        // CSS样式类。2007/5/18 new add
        public string CssClass
        {
            get
            {
                return m_strCssClass;
            }
            set
            {
                m_strCssClass = value;
            }
        }

        public bool Hidden = false;	// 列是否在输出时隐藏
        public int ColumnNumber = -1;	// 在列号，从 0 开始计数。 -1 表示尚未初始化

        public string DefaultValue = "";	// 数据缺省值

        public bool Sum = false;	// 本列是否需要“合计”

        public ColumnDataType DataType = ColumnDataType.Auto;

        public int Width = -1;	// 列宽度

        public int Colspan = 1; // 

        public string Eval = "";    // 
    }

    public class SumLineReader : DbDataReader
    {
        bool _fetched = false;  // 是否被读过

        public object[] FieldValues = null;

        public SumLineReader()
        {
        }

        // 摘要:
        //     Closes the datareader, potentially closing the connection as well if CommandBehavior.CloseConnection
        //     was specified.
        public override void Close()
        {
        }


        // 摘要:
        //     Not implemented. Returns 0
        public override int Depth {
            get
            {
                return 0;
            }
        }
        //
        // 摘要:
        //     Returns the number of columns in the current resultset
        public override int FieldCount {
            get
            {
                if (this.FieldValues == null)
                    return 0;
                return this.FieldValues.Length;
            }
        }

        //
        // 摘要:
        //     Retrieve the count of records affected by an update/insert command. Only
        //     valid once the data reader is closed!
        public override int RecordsAffected {
            get
            {
                if (this._fetched == true)
                    return 1;
                return 0;
            }
        }

        // 摘要:
        //     Indexer to retrieve data from a column given its i
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     The value contained in the column
        public override object this[int i] {
            get
            {
                return this.FieldValues[i];
            }
        }
        //
        // 摘要:
        //     Indexer to retrieve data from a column given its name
        //
        // 参数:
        //   name:
        //     The name of the column to retrieve data for
        //
        // 返回结果:
        //     The value contained in the column
        public override object this[string name] {
            get
            {
                throw new Exception("尚未实现");
            }
        }

        //
        // 摘要:
        //     Returns True if the specified column is null
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     True or False
        public override bool IsDBNull(int i)
        {
            if (this.FieldValues == null)
                return true;
            if (i >= this.FieldValues.Length)
                return true;

            return this.FieldValues[i] == null;
        }


        //
        // 摘要:
        //     Returns True if the resultset has rows that can be fetched
        public override bool HasRows
        {
            get
            {
                if (this._fetched == false)
                    return true;
                return false;
            }
        }

        //
        // 摘要:
        //     Returns True if the data reader is closed
        public override bool IsClosed {
            get
            {
                return false;
            }
        }


        //
        // 摘要:
        //     Reads the next row from the resultset
        //
        // 返回结果:
        //     True if a new row was successfully loaded and is ready for processing
        public override bool Read()
        {
            if (this._fetched == true)
                return false;
            this._fetched = true;
            return true;
        }

        //
        // 摘要:
        //     Retrieves the column as a boolean value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     bool
        public override bool GetBoolean(int i)
        {
            object o = this.FieldValues[i];
            if ( o is bool)
                return (bool)o;
            throw new Exception("列 "+i.ToString()+" 不是 bool 类型");
        }

        //
        // 摘要:
        //     Retrieves the column as a single byte value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     byte
        public override byte GetByte(int i)
        {
            object o = this.FieldValues[i];
            if ( o is byte)
                return (byte)o;
            throw new Exception("列 "+i.ToString()+" 不是 byte 类型");
        }

        //
        // 摘要:
        //     Retrieves a column as an array of bytes (blob)
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        //   fieldOffset:
        //     The zero-based index of where to begin reading the data
        //
        //   buffer:
        //     The buffer to write the bytes into
        //
        //   bufferoffset:
        //     The zero-based index of where to begin writing into the array
        //
        //   length:
        //     The number of bytes to retrieve
        //
        // 返回结果:
        //     The actual number of bytes written into the array
        //
        // 备注:
        //     To determine the number of bytes in the column, pass a null value for the
        //     buffer. The total length will be returned.
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("尚未实现");
            return 0;
        }

        //
        // 摘要:
        //     Returns the column as a single character
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     char
        public override char GetChar(int i)
        {
            object o = this.FieldValues[i];
            if (o is char)
                return (char)o;
            throw new Exception("列 " + i.ToString() + " 不是 byte 类型");
        }


        //
        // 摘要:
        //     Retrieves a column as an array of chars (blob)
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        //   fieldoffset:
        //     The zero-based index of where to begin reading the data
        //
        //   buffer:
        //     The buffer to write the characters into
        //
        //   bufferoffset:
        //     The zero-based index of where to begin writing into the array
        //
        //   length:
        //     The number of bytes to retrieve
        //
        // 返回结果:
        //     The actual number of characters written into the array
        //
        // 备注:
        //     To determine the number of characters in the column, pass a null value for
        //     the buffer. The total length will be returned.
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            object o = this.FieldValues[i];
            if ( o is string)
            {
                string strText = (string)o;
                if (buffer != null)
                    strText.CopyTo((int)fieldoffset, buffer, bufferoffset, length);
                return strText.Length;
            }
            throw new Exception("列 "+i.ToString()+" 不是 byte 类型");
        }

        //
        // 摘要:
        //     Retrieves the name of the back-end datatype of the column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     string
        public override string GetDataTypeName(int i)
        {
            return "";
        }

        //
        // 摘要:
        //     Retrieve the column as a date/time value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     DateTime
        public override DateTime GetDateTime(int i)
        {
            return new DateTime(0);
        }

        //
        // 摘要:
        //     Retrieve the column as a decimal value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     decimal
        public override decimal GetDecimal(int i)
        {
            object o = this.FieldValues[i];
            if ( o is double)
                return (decimal)(double)o;
                        if ( o is decimal)
                return (decimal)o;
                                    if ( o is int)
                return (decimal)o;
                                    if ( o is long)
                return (decimal)o;
                                    if ( o is string)
                                    {
                                        decimal v = 0;
                                        decimal.TryParse((string)o, out v);
                                        return v;
                                    }

            throw new Exception("列 "+i.ToString()+" 不是 decimal 类型");
        }

        //
        // 摘要:
        //     Returns the column as a double
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     double
        public override double GetDouble(int i)
        {
            object o = this.FieldValues[i];
            if ( o is double)
                return (double)o;
                        if ( o is decimal)
                return (double)o;
                                    if ( o is int)
                return (double)o;
                                    if ( o is long)
                return (double)o;
                                    if ( o is string)
                                    {
                                        double v = 0;
                                        double.TryParse((string)o, out v);
                                        return v;
                                    }

            throw new Exception("列 "+i.ToString()+" 不是 decimal 类型");
        }

        //
        // 摘要:
        //     Enumerator support
        //
        // 返回结果:
        //     Returns a DbEnumerator object.
        public override IEnumerator GetEnumerator()
        {
            return null;
        }
        //
        // 摘要:
        //     Returns the .NET type of a given column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Type
        public override Type GetFieldType(int i)
        {
            return this.FieldValues[i].GetType();
        }
        //
        // 摘要:
        //     Returns a column as a float value
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     float
        public override float GetFloat(int i)
        {
                        object o = this.FieldValues[i];
            if ( o is float)
                return (float)o;
                        if ( o is decimal)
                return (float)o;
                                    if ( o is int)
                return (float)o;
                                    if ( o is long)
                return (float)o;
                                    if ( o is string)
                                    {
                                        float v = 0;
                                        float.TryParse((string)o, out v);
                                        return v;
                                    }

            throw new Exception("列 "+i.ToString()+" 不是 float 类型");

        }

        //
        // 摘要:
        //     Returns the column as a Guid
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Guid
        public override Guid GetGuid(int i)
        {
            return new Guid();
        }
        //
        // 摘要:
        //     Returns the column as a short
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Int16
        public override short GetInt16(int i)
        {
            return (short)this.FieldValues[i];
        }
        //
        // 摘要:
        //     Retrieves the column as an int
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Int32
        public override int GetInt32(int i)
        {
            return (int)this.FieldValues[i];
        }

        //
        // 摘要:
        //     Retrieves the column as a long
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     Int64
        public override long GetInt64(int i)
        {
            return (long)this.FieldValues[i];
        }

        //
        // 摘要:
        //     Retrieves the name of the column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     string
        public override string GetName(int i)
        {
            return null;
        }
        //
        // 摘要:
        //     Retrieves the i of a column, given its name
        //
        // 参数:
        //   name:
        //     The name of the column to retrieve
        //
        // 返回结果:
        //     The int i of the column
        public override int GetOrdinal(string name)
        {
            return -1;
        }
        //
        // 摘要:
        //     Schema information in SQLite is difficult to map into .NET conventions, so
        //     a lot of work must be done to gather the necessary information so it can
        //     be represented in an ADO.NET manner.
        //
        // 返回结果:
        //     Returns a DataTable containing the schema information for the active SELECT
        //     statement being processed.
        public override DataTable GetSchemaTable()
        {
            return null;
        }
        //
        // 摘要:
        //     Retrieves the column as a string
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     string
        public override string GetString(int i)
        {
            return this.FieldValues[i].ToString();
        }
        //
        // 摘要:
        //     Retrieves the column as an object corresponding to the underlying datatype
        //     of the column
        //
        // 参数:
        //   i:
        //     The index of the column to retrieve
        //
        // 返回结果:
        //     object
        public override object GetValue(int i)
        {
            return this.FieldValues[i];
        }

        //
        // 摘要:
        //     Returns a collection containing all the column names and values for the current
        //     row of data in the current resultset, if any. If there is no current row
        //     or no current resultset, an exception may be thrown.
        //
        // 返回结果:
        //     The collection containing the column name and value information for the current
        //     row of data in the current resultset or null if this information cannot be
        //     obtained.
        public NameValueCollection GetValues()
        {
            return null;
        }

        //
        // 摘要:
        //     Retreives the values of multiple columns, up to the size of the supplied
        //     array
        //
        // 参数:
        //   values:
        //     The array to fill with values from the columns in the current resultset
        //
        // 返回结果:
        //     The number of columns retrieved
        public override int GetValues(object[] values)
        {
            values = this.FieldValues;
            return this.FieldValues.Length;
        }

        //
        // 摘要:
        //     Moves to the next resultset in multiple row-returning SQL command.
        //
        // 返回结果:
        //     True if the command was successful and a new resultset is available, False
        //     otherwise.
        public override bool NextResult()
        {
            return false;
        }

    }
}
