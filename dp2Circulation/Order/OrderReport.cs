using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ClosedXML.Excel;

using static dp2Circulation.PrintOrderForm;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using System.Reflection;

namespace dp2Circulation
{
    public static class OrderReport
    {
        // 按照关键字段分类的统计行
        class KeyStatisLine
        {
            // 关键分类字段。可以是 Seller Source 等
            public string Key { get; set; }

            public long OrderCopies { get; set; }
            public string OrderPrice { get; set; }

            public long AcceptCopies { get; set; }
            public string AcceptPrice { get; set; }

            // 订购码洋
            public string OrderFixedPrice { get; set; }
            // 平均订购折扣
            public string OrderDiscount { get; set; }

            // 验收码洋
            public string AcceptFixedPrice { get; set; }
            // 平均验收折扣
            public string AcceptDiscount { get; set; }


            // 订购种数
            public long OrderBiblioCount { get; set; }
            // 验收种数
            public long AcceptBiblioCount { get; set; }

            // 订购期数
            public long OrderIssueCount { get; set; }
            // 验收期数
            public long AcceptIssueCount { get; set; }

        }

#if NO
        // 根据 Merged ListView 和指定的 Key 列创建报表
        // parameters:
        //      nKeyColumn  用于 Key 的列 index 值。例如 MERGED_COLUMN_SELLER
        //      strKeyCaption   Key 列的列标题。例如“渠道”
        public static void BuildMergedReport(
            IEnumerable<ListViewItem> items,
            int nKeyColumn,
            string strKeyCaption,
            IXLWorksheet sheet)
        {
            List<KeyStatisLine> results = //this.listView_merged.Items
                items
                .Cast<ListViewItem>()
                .GroupBy(p => ListViewUtil.GetItemText(p, nKeyColumn))
                .Select(cl => new KeyStatisLine
                {
                    Key = ListViewUtil.GetItemText(cl.First(), nKeyColumn),
                    OrderCopies = cl.Sum(o => Convert.ToInt32(ListViewUtil.GetItemText(o, MERGED_COLUMN_COPY))),
                    OrderPrice = ConcatPrice(cl, MERGED_COLUMN_PRICE),
                }).ToList();
            int line = 0;
            // 栏目标题行
            {
                int column = 0;
                // Seller
                IXLCell start = PrintOrderForm.WriteExcelCell(
        sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
strKeyCaption);
                // OrderCopies
                WriteExcelCell(
sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
"订购套数");
                // OrderPrice
                IXLCell end = WriteExcelCell(
sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
"订购价");
                IXLRange range = sheet.Range(start, end);
                range.Style.Font.Bold = true;
                range.Style.Fill.BackgroundColor = XLColor.LightGray;
                range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                line++;
            }
            // 内容行
            foreach (var item in results)
            {
                int column = 0;
                // Seller
                WriteExcelCell(
        sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
item.Key);
                // OrderCopies
                WriteExcelCell(
sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
item.OrderCopies);
                // OrderPrice
                WriteExcelCell(
sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
item.OrderPrice);
                // text.Append($"i={i} Seller='{item.Seller}' OrderCopies={item.OrderCopies} OrderPrice={item.OrderPrice}\r\n");
                line++;
            }
        }

        static string ConcatPrice(
            IGrouping<string, ListViewItem> cl,
            int column_index)
        {
            // PrintOrderForm.RemoveChangedChar()
            string strList = ListViewUtil.GetItemText(cl.Aggregate((current, next) =>
            {
                string s1 = ListViewUtil.GetItemText(current, column_index);
                string s2 = ListViewUtil.GetItemText(next, column_index);
                var r = new ListViewItem();
                ListViewUtil.ChangeItemText(r, column_index, s1 + "," + s2);
                return r;
            }), column_index);
            return PriceUtil.TotalPrice(StringUtil.SplitList(strList));
        }

#endif

        // 按照经费来源的 统计

        // 册类型的统计。订购里面似乎只有简单的订购类型。验收了才有册类型

        class ResultLine
        {
            public string ProductName { get; set; }
            public string Quantity { get; set; }
            public string Price { get; set; }
        }

        static string ToString(ListViewItem item)
        {
            StringBuilder text = new StringBuilder();
            foreach (string s in item.SubItems)
            {
                if (text.Length > 0)
                    text.Append("\t");
                text.Append(s);
            }

            return text.ToString();
        }

        // 根据 Origin ListView 和指定的 Key 列创建报表
        // parameters:
        //      nKeyColumn  用于 Key 的列 index 值。例如 ORIGIN_COLUMN_SOURCE
        //      strKeyCaption   Key 列的列标题。例如“经费来源”
        public static void BuildOriginReport(
            bool bSeries,
            IEnumerable<ListViewItem> items,
            string strKeyName,
            string strKeyCaption,
            IXLWorksheet sheet)
        {
            List<int> column_max_chars = new List<int>();

            // 先变换为 LineInfo 数组
            // LineInfo 可以放在固定面板区用 PropertyGrid 界面显示
            List<PrintOrderForm.LineInfo> lines = new List<PrintOrderForm.LineInfo>();
            int i = 0;
            foreach (ListViewItem item in items)
            {
                var current = PrintOrderForm.LineInfo.Build(item, $"原始视图第 {(i + 1)} 行");
                lines.Add(current.Adjust());
                i++;
            }

            List<KeyStatisLine> results =
                lines
                .Where(o => bSeries || string.IsNullOrEmpty(o.State) == false)
                .GroupBy(p => (string)GetPropertyValue(p, strKeyName))
                .Select(cl => new KeyStatisLine
                {
                    Key = (string)GetPropertyValue(cl.First(), strKeyName),

                    OrderIssueCount = cl.Where(a => a.Copy?.OldCopy?.Copy > 0)
                    .Sum(o => Convert.ToInt32(o.IssueCount)),

                    AcceptIssueCount = cl.Where(a => a.Copy?.NewCopy?.Copy > 0)
                    .Sum(o => Convert.ToInt32(o.IssueCount)),

                    OrderCopies = cl.Sum(o => Convert.ToInt32(o.Copy.OldCopy.Copy)),
                    OrderPrice = ConcatLinePrice(cl, "TotalPrice"),

                    AcceptCopies = cl.Sum(o => Convert.ToInt32(o.Copy.NewCopy.Copy)),
                    AcceptPrice = ConcatLinePrice(cl, "AcceptTotalPrice"),

                    OrderBiblioCount = cl.Where(a => a.Copy?.OldCopy?.Copy > 0)
                    .GroupBy(p => p.BiblioRecPath).LongCount(),
                    // cl.GroupBy(p => p.BiblioRecPath).LongCount(),

                    AcceptBiblioCount = cl.Where(a => a.Copy?.NewCopy?.Copy > 0)
                    .GroupBy(p => p.BiblioRecPath).LongCount(),

                    OrderFixedPrice = ConcatLinePrice(cl, "OrderTotalFixedPrice"),    // 可能要乘以套数
                    OrderDiscount = cl.Where(a => a.Copy?.OldCopy?.Copy > 0)
                    .DefaultIfEmpty(new PrintOrderForm.LineInfo()).Average(o => o.OrderDiscount).ToString(),

                    AcceptFixedPrice = ConcatLinePrice(cl, "AcceptTotalFixedPrice"),    // 可能要乘以套数
                    AcceptDiscount = cl.Where(a => a.Copy?.NewCopy?.Copy > 0)
                    .DefaultIfEmpty(new PrintOrderForm.LineInfo()).Average(o => o.AcceptDiscount).ToString(),

                }).ToList();
            int line = 0;
            // 栏目标题行
            string[] titles = new string[] {
                strKeyCaption,
                "订购种数", "*订购期数",
                "订购套数", "订购价",
                "订购码洋", "平均订购折扣",
                "验收种数", "*验收期数",
                "验收套数", "验收价",
                "验收码洋", "平均验收折扣",
            };
            {
                IXLCell start = null;
                IXLCell end = null;

                SetCellInfo info = new SetCellInfo
                {
                    ColumnMaxChars = column_max_chars,
                    Line = line,
                    Column = 0,
                    Sheet = sheet,
                };

                foreach (string title in titles)
                {
                    if (title.StartsWith("*") && bSeries == false)
                        continue;
                    string text = title.StartsWith("*") ? title.Substring(1) : title;

                    IXLCell cell = info.SetCellText(text);
#if NO
                    SetMaxChars(ref column_max_chars,
TABLE_LEFT_BLANK_COLUMS + column,
text.Length);

                    IXLCell cell = PrintOrderForm.WriteExcelCell(
sheet,
TABLE_TOP_BLANK_LINES + line,
TABLE_LEFT_BLANK_COLUMS + column++,
text);
#endif
                    if (start == null)
                        start = cell;
                    end = cell;
                }

                IXLRange range = sheet.Range(start, end);
                range.Style.Font.Bold = true;
                range.Style.Fill.BackgroundColor = XLColor.LightGray;
                range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                line++;
            }
            // 内容行
            foreach (var item in results)
            {
                SetCellInfo info = new SetCellInfo
                {
                    ColumnMaxChars = column_max_chars,
                    Line = line,
                    Column = 0,
                    Sheet = sheet,
                };

                // Seller
                info.SetCellText(item.Key);

                // OrderBiblioCount
                info.SetCellText(item.OrderBiblioCount);

                if (bSeries)
                {
                    // OrderIssueCount
                    info.SetCellText(item.OrderIssueCount);
                }

                // OrderCopies
                info.SetCellText(item.OrderCopies);

                // OrderPrice
                info.SetCellText(item.OrderPrice);

                // OrderFixedPrice
                info.SetCellText(item.OrderFixedPrice);

                // OrderDiscount
                info.SetCellText(item.OrderDiscount);

                // AcceptBiblioCount
                info.SetCellText(item.AcceptBiblioCount);

                if (bSeries)
                {
                    // AcceptIssueCount
                    info.SetCellText(item.AcceptIssueCount);
                }

                // AcceptCopies
                info.SetCellText(item.AcceptCopies);

                // AcceptPrice
                info.SetCellText(item.AcceptPrice);

                // AcceptFixedPrice
                info.SetCellText(item.AcceptFixedPrice);

                // AcceptDiscount
                info.SetCellText(item.AcceptDiscount);

                // text.Append($"i={i} Seller='{item.Seller}' OrderCopies={item.OrderCopies} OrderPrice={item.OrderPrice}\r\n");
                line++;
            }

            AdjectColumnWidth(sheet, column_max_chars);
        }

        class SetCellInfo
        {
            public IXLWorksheet Sheet { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
            // public string Text { get; set; }
            public List<int> ColumnMaxChars { get; set; }

            public IXLCell SetCellText(string text)
            {
                List<int> column_max_chars = this.ColumnMaxChars;
                SetMaxChars(ref column_max_chars,
    TABLE_LEFT_BLANK_COLUMS + this.Column,
    text.Length);
                this.ColumnMaxChars = column_max_chars;
                IXLCell cell = WriteExcelCell(
        this.Sheet,
    TABLE_TOP_BLANK_LINES + this.Line,
    TABLE_LEFT_BLANK_COLUMS + this.Column,
    text);
                this.Column++;
                return cell;
            }

            public void SetCellText(long value)
            {
                List<int> column_max_chars = this.ColumnMaxChars;
                SetMaxChars(ref column_max_chars,
    TABLE_LEFT_BLANK_COLUMS + this.Column,
    value.ToString().Length);
                this.ColumnMaxChars = column_max_chars;
                WriteExcelCell(
        this.Sheet,
    TABLE_TOP_BLANK_LINES + this.Line,
    TABLE_LEFT_BLANK_COLUMS + this.Column,
    value);
                this.Column++;
            }
        }

        static void SetCellText(
            IXLWorksheet sheet,
            int line,
            ref int column,
            string text,
            ref List<int> column_max_chars)
        {
            SetMaxChars(ref column_max_chars,
column,
text.Length);
            WriteExcelCell(
    sheet,
line,
column++,
text);
        }

        static object GetPropertyValue(PrintOrderForm.LineInfo info,
    string strKeyName)
        {
            object result = info.GetType().GetProperty(strKeyName).GetValue(info);
            return result;
#if NO
            if (strKeyName == "seller")
                return info.Seller;
            else if (strKeyName == "source")
                return info.Source;
            throw new ArgumentException($"未知的 strKeyName '{strKeyName}'");
#endif
        }

        static void SetPropertyValue(PrintOrderForm.LineInfo info,
    string strKeyName,
    object value)
        {
            try
            {
                info.GetType().GetProperty(strKeyName).SetValue(info, value);
            }
            catch (Exception ex)
            {
                throw new Exception($"SetProperty() 时发生异常(strKeyName={strKeyName} value={value}): " + ex.Message);
            }
        }

        static string ConcatLinePrice(
    IGrouping<string, PrintOrderForm.LineInfo> cl,
    string strFieldName)
        {
            // PrintOrderForm.RemoveChangedChar()
            string strList = (string)GetPropertyValue(cl.Aggregate((current, next) =>
            {
                // TODO：单价应该乘以套数。或者还有期数也要乘上
                string s1 = (string)GetPropertyValue(current, strFieldName);
                string s2 = (string)GetPropertyValue(next, strFieldName);
                var r = new PrintOrderForm.LineInfo();
                SetPropertyValue(r, strFieldName, s1 + "," + s2);
                return r;
            }), strFieldName);
            return PriceUtil.TotalPrice(StringUtil.SplitList(strList));
        }
    }
}
