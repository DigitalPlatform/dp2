using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

using ClosedXML.Excel;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

namespace dp2Circulation.OperLog
{
    public static class OperLogReport
    {

        // 按照关键字段分类的统计行
        public class KeyStatisLine
        {
            // 关键分类字段。可以是 ItemRefID 等
            public string Key { get; set; }

            public string ItemBarcode { get; set; } // 第一个册条码号
            public int Count { get; set; }  // 重复写入次数
            public string Operator { get; set; }    // 第一个操作者
            public DateTime OperTime { get; set; }  // 第一个操作时间
        }

        static void WriteLine(
    IXLWorksheet sheet,
    string[] cols,
    List<IXLCell> cells,
    ref int nItemIndex,
    ref int nRowIndex,
    ref List<int> column_max_chars)
        {
            int nColIndex = 2;
            foreach (string s in cols)
            {
                // 统计最大字符数
                ClosedXmlUtil.SetMaxChars(column_max_chars, nColIndex - 1, s);

                IXLCell cell = null;
                cell = sheet.Cell(nRowIndex, nColIndex).SetValue(s);
                if (nColIndex == 2)
                {
                    // cell = sheet.Cell(nRowIndex, nColIndex).SetValue(nItemIndex + 1);
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                cell.Style.Alignment.WrapText = true;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                nColIndex++;
                cells?.Add(cell);
            }

            nItemIndex++;
            nRowIndex++;
        }

        // 日统计行
        public class DateStatisLine
        {
            public string Date { get; set; }    // 日期
            public string Operator { get; set; }    // 操作者
            public int ItemCount { get; set; }  // 册数
            public int WriteCount { get; set; } // 写入次数。可能会大于册数
        }

        public class KeyStatisLine1
        {
            // 关键分类字段。可以是 ItemRefID 等
            public string Key { get; set; }

            public string ItemBarcode { get; set; } // 第一个册条码号
            public int Count { get; set; }  // 重复写入次数
            public string Operator { get; set; }    // 第一个操作者
            public DateTime OperDate { get; set; }  // 第一个操作时间
        }

        static DateTime GetDate(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day);
        }

        // 输出日统计表。
        public static void BuildRfidStatisSheet(
IEnumerable<KeyStatisLine> lines,
IXLWorksheet sheet,
out List<DateStatisLine> results)
        {

            List<int> column_max_chars = new List<int>();

            results =
                lines
                .Select(l=> new KeyStatisLine1 { Key = l.Key,
                    ItemBarcode = l.ItemBarcode,
                    Count = l.Count,
                    Operator = l.Operator,
                    OperDate = GetDate(l.OperTime)})
                // .Where(o => bSeries || string.IsNullOrEmpty(o.State) == false)
                .GroupBy(p => new { p.OperDate, p.Operator })
                .Select(cl => new DateStatisLine
                {
                    Date = (string)cl.First().OperDate.ToLongDateString(),
                    Operator = cl.First().Operator,
                    ItemCount = cl.Count(),
                    WriteCount = cl.Sum(a => a.Count),
                }).ToList();

            if (sheet != null)
            {
                int line = 0;
                // 栏目标题行
                string[] titles = new string[] {
                "日期",
                "操作者",
                "册数",
                "写入次数",
            };

                int nRowIndex = 1;

                WriteLine(
    sheet,
    titles,
    null,   // cells,
    ref line,
    ref nRowIndex,
    ref column_max_chars);

                foreach (var result in results)
                {
                    string[] cols = new string[] {
                    result.Date,
                    result.Operator,
                    result.ItemCount.ToString(),
                    result.WriteCount.ToString()
                };
                    WriteLine(
        sheet,
        cols,
        null,   // cells,
        ref line,
        ref nRowIndex,
        ref column_max_chars);
                }

                PrintOrderForm.AdjectColumnWidth(sheet, column_max_chars);
            }
        }

        // 输出基本表。每行为一册。重复写入同一个 RFID 多次，会被归并为一行
        public static void BuildRfidWriteReport(
IEnumerable<RfidWriteInfo> lines,
IXLWorksheet sheet,
out List<KeyStatisLine> results)
        {
            List<int> column_max_chars = new List<int>();

            results =
                lines
                // .Where(o => bSeries || string.IsNullOrEmpty(o.State) == false)
                .GroupBy(p => (string)p.ItemRefID)
                .Select(cl => new KeyStatisLine
                {
                    Key = (string)cl.First().ItemRefID,
                    ItemBarcode = cl.First().ItemBarcode,
                    Operator = cl.First().Operator,
                    OperTime = cl.First().OperTime,
                    Count = cl.Count(),
                }).ToList();

            if (sheet != null)
            {
                int line = 0;
                // 栏目标题行
                string[] titles = new string[] {
                "参考ID",
                "册条码号",
                "写入次数",
                "操作者",
                "操作时间",
            };

                int nRowIndex = 1;

                WriteLine(
    sheet,
    titles,
    null,   // cells,
    ref line,
    ref nRowIndex,
    ref column_max_chars);

                foreach (var result in results)
                {
                    string[] cols = new string[] {
                    result.Key,
                    result.ItemBarcode,
                    result.Count.ToString(),
                    result.Operator,
                    result.OperTime.ToString()
                };
                    WriteLine(
        sheet,
        cols,
        null,   // cells,
        ref line,
        ref nRowIndex,
        ref column_max_chars);
                }

                PrintOrderForm.AdjectColumnWidth(sheet, column_max_chars);
            }
        }


    }

    // 日志原始信息。每个对象对应于一个日志记录
    public class RfidWriteInfo
    {
        public int Index { get; set; }
        public string Date { get; set; }
        public string ItemBarcode { get; set; }
        public string ItemRefID { get; set; }
        public string Operator { get; set; }
        public DateTime OperTime { get; set; }

        public static RfidWriteInfo Build(string date, int index, XmlDocument dom)
        {
            RfidWriteInfo info = new RfidWriteInfo();
            info.Date = date;
            info.Index = index;

            info.ItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "itemBarcode");
            info.ItemRefID = DomUtil.GetElementText(dom.DocumentElement,
                "itemRefID");
            info.Operator = DomUtil.GetElementText(dom.DocumentElement,
                "operator");
            string operTime = DomUtil.GetElementText(dom.DocumentElement,
            "operTime");
            info.OperTime = DateTimeUtil.FromRfc1123DateTimeString(operTime).ToLocalTime();
            return info;
        }
    }
}
