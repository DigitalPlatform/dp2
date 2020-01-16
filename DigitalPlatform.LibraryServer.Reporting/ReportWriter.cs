using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class ReportWriter
    {
        // 报表算法。101/111/121 ...
        public string Algorithm { get; set; }

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


        public int OutputRmlReport<T>(
            IEnumerable<T> data_reader,
            object sum,
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            string strTitle = DomUtil.GetElementText(_cfgDom.DocumentElement, "title").Replace("\\r", "\r");
            strTitle = StringUtil.MacroString(macro_table, strTitle);

            string strComment = DomUtil.GetElementText(_cfgDom.DocumentElement, "titleComment").Replace("\\r", "\r");
            strComment = StringUtil.MacroString(macro_table, strComment);

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
                PathUtil.TryCreateDir(Path.GetDirectoryName(strOutputFileName));
            }

#if NO
            try
            {
#endif
            using (XmlTextWriter writer = new XmlTextWriter(strOutputFileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();
                writer.WriteStartElement("report");
                WriteAttributeString(writer, "version", "0.01");

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
                    WriteString(writer, strCreateTime);
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
                    sum,
                    writer);

                writer.WriteEndElement();   // </report>
                writer.WriteEndDocument();
            }

#if NO
            }
            catch (Exception ex)
            {
                strError = "写入文件 '" + strOutputFileName + "' 的过程中出现异常: " + ExceptionUtil.GetDebugText(ex);
                return -1;
            }
#endif

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

        static bool TryGetFieldValue(object obj, string name, out object result)
        {
            var type = obj.GetType();
            var info = type.GetProperty(name);
            if (info == null)
            {
                result = $"成员 '{name}' 没有找到";
                return false;
            }
            result = info.GetValue(obj);
            return true;
        }

        // 输出 RML 格式的表格
        // 本函数负责写入 <table> 元素
        // parameters:
        //      nTopLines   顶部预留多少行
        void OutputRmlTable<T>(
            IEnumerable<T> data_reader,
            object sum,
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
            WriteAttributeString(writer, "class", "table");

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
                    WriteAttributeString(writer, "class", column.CssClass);
                if (column.Colspan > 1)
                    WriteAttributeString(writer, "colspan", column.Colspan.ToString());

                WriteString(writer, column.Title);
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

            var handle = data_reader.GetEnumerator();

            // 内容行循环
            for (i = 0; ; i++)  // i < Math.Min(nMaxLines, table.Count)
            {
                if (handle.MoveNext() == false)
                    break;

                nLineCount++;
#if NO
                if (table.HasRows == false)
                    break;
#endif
                // Line line = table[i];

                if (engine != null)
                {
                    engine.SetGlobalValue("line", data_reader);
                }

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
                WriteAttributeString(writer, "class", strLineCssClass);

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
                        // Debug.Assert(column.ColumnNumber < data_reader.FieldCount, "");

                        if (string.IsNullOrEmpty(column.Eval) == false)
                        {
                            // engine.SetGlobalValue("cell", line.GetObject(column.ColumnNumber));
                            engine.SetGlobalValue("rowNumber", nLineCount.ToString());
                            engine.SetGlobalValue("currency", new PriceUtil());
                            strText = engine.Evaluate(column.Eval).ToString();
                        }
#if NO
                        else if (column.DataType == ColumnDataType.PriceDouble)
                        {
                            if (data_reader.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = data_reader.GetDouble(column.ColumnNumber);
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
#endif
                        else
                        {
                            TryGetFieldValue(handle.Current,
                                column.CssClass,
                                out object o);
                            // (column.ColumnNumber);
                            if (o != null)
                                strText = o.ToString();
                            else
                                strText = "";

                        }
                    }
                    else
                    {
                        // strText = data_reader.GetString(0);   // line.Entry;
                    }

                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        WriteAttributeString(writer, "class", column.CssClass);
                    WriteString(writer, strText);
                    writer.WriteEndElement();   // </td>

                    if (this.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {
                        string strDebugInfo = "";
                        try
                        {
                            // if (column.DataType != DataType.Currency)
                            {

                                // if (column.ColumnNumber < data_reader.FieldCount)
                                // v = data_reader.GetValue(column.ColumnNumber);
                                TryGetFieldValue(handle.Current,
                                    column.CssClass,
                                    out object v);


                                if (sums[j] == null)
                                    sums[j] = v;
                                else
                                {
                                    strDebugInfo = GetDebugInfo(column.DataType,
            sums[j],
            v);
                                    sums[j] = AddValue(column.DataType,
            sums[j],
            v);
                                    // sums[j] = ((decimal)sums[j]) + v;
                                }
                            }
                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + i.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，出现异常(strDebugInfo='" + strDebugInfo + "'): " + ExceptionUtil.GetAutoText(ex));
                        }
                    }
                }

                // strResult.Append("</tr>\r\n");
                writer.WriteEndElement();   // </tr>
            }

            writer.WriteEndElement();   // </tbody>

            if (sum != null)
            {
                writer.WriteStartElement("tfoot");
                writer.WriteStartElement("tr");
                WriteAttributeString(writer, "class", "sum");

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
                            engine.SetGlobalValue("currency", new PriceUtil());
                            strText = engine.Evaluate(column.Eval).ToString();
                        }
                        else if (column.Sum == true)
                        {
                            TryGetFieldValue(sum,
column.CssClass,
out object o);
                            // (column.ColumnNumber);
                            if (o != null)
                                strText = o.ToString();
                            else
                                strText = "";
                        }
                    }

                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        WriteAttributeString(writer, "class", column.CssClass);
                    WriteString(writer, strText);
                    writer.WriteEndElement();   // </td>
                }

                writer.WriteEndElement();   // </tr>
                writer.WriteEndElement();   // </tfoot>
            }
            else if (this.SumLine == true)
            {
                /*
                SumLineReader sum_line = null;
                if (engine != null)
                {
                    // 准备 Line 对象
                    sum_line = new SumLineReader();
                    sum_line.FieldValues = sums;
                    sum_line.Read();
                    engine.SetGlobalValue("line", sum_line);
                    engine.SetGlobalValue("rowNumber", "");
                }
                */

                // strResult.Append("<tr class='sum'>\r\n");
                writer.WriteStartElement("tfoot");
                writer.WriteStartElement("tr");
                WriteAttributeString(writer, "class", "sum");

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
                            engine.SetGlobalValue("currency", new PriceUtil());
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
                                strText = Int64ToPrice((Int64)sums[j]);
                            }
                            else
                                strText = Convert.ToString(sums[j]);

                            if (column.DataType == ColumnDataType.Currency)
                            {
                                // 汇总价格
                                int nRet = PriceUtil.SumPrices(strText,
                out string strSumPrice,
                out string strError);
                                if (nRet == -1)
                                    strText = strError;
                                else
                                    strText = strSumPrice;
                            }
                        }
                        else
                            strText = column.DefaultValue;  //  "&nbsp;";

                    }

                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        WriteAttributeString(writer, "class", column.CssClass);
                    WriteString(writer, strText);
                    writer.WriteEndElement();   // </td>
                }

                // strResult.Append("</tr>\r\n");
                writer.WriteEndElement();   // </tr>
                writer.WriteEndElement();   // </tfoot>
            }

            writer.WriteEndElement();   // </table>
        }

        public static string Int64ToPrice(Int64 v)
        {
            Decimal d = Convert.ToDecimal(v / (double)100);
            return d.ToString();
        }

        static string GetDebugInfo(ColumnDataType datatype,
object o1,
object o2)
        {
            return "datatype=" + datatype.ToString()
                + ",o1=" + (o1 == null ? "<null>" : o1.GetType().ToString())
                + ",o2=" + (o2 == null ? "<null>" : o2.GetType().ToString());
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
                    return (Int64)o1 + Convert.ToInt64(o2); // 2016/11/24
                if (o1 is Int32)
                    return (Int32)o1 + (Int32)o2;
                if (o1 is double)
                {
#if NO
                    if (o2 is long)
                    {
                        return (double)o1 + Convert.ToDouble(o2);
                    }
                    return (double)o1 + (double)o2;
#endif
                    return (double)o1 + Convert.ToDouble(o2);
                }
                if (o1 is decimal)
                    return (decimal)o1 + (decimal)o2;
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 Auto 类型累加 o1 type=" + o1.GetType().ToString() + ", o2 type=" + o2.GetType().ToString());
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
                if (o1 is string)   // 2015/7/16
                {
                    Int64 v1 = 0;
                    Int64 v2 = 0;
                    Int64.TryParse(o1 as string, out v1);
                    Int64.TryParse(o2 as string, out v2);
                    return (v1 + v2).ToString();
                }

                throw new Exception("无法支持的 Number 类型累加 o1 type=" + o1.GetType().ToString() + ", o2 type=" + o2.GetType().ToString());
            }
            if (datatype == ColumnDataType.String)
            {
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 String 类型累加 o1 type=" + o1.GetType().ToString() + ", o2 type=" + o2.GetType().ToString());
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
#if NO
                // 这一句容易发现列 数据类型 的错误
                return PriceUtil.JoinPriceString((string)o1,
                    (string)o2);
#endif
                return PriceUtil.Add((string)o1,
                    (string)o2);
#if NO
                // 这一句更健壮一些
                return PriceUtil.JoinPriceString(Convert.ToString(o1),
                    Convert.ToString(o2));
#endif
            }
            throw new Exception("无法支持的 " + datatype.ToString() + " 类型累加 o1 type=" + o1.GetType().ToString() + ", o2 type=" + o2.GetType().ToString());
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
                WriteString(writer, title);
                i++;
            }
        }

        static void WriteString(XmlWriter writer, string strText)
        {
            writer.WriteString(DomUtil.ReplaceControlCharsButCrLf(strText, '*'));
        }

        static void WriteAttributeString(XmlWriter writer, string strName, string strValue)
        {
            writer.WriteAttributeString(DomUtil.ReplaceControlCharsButCrLf(strName, '*'),
                DomUtil.ReplaceControlCharsButCrLf(strValue, '*'));
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

        // CSS样式类。2007/5/18
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

}
