using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ClosedXML.Excel;
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
        }

        static void WriteLine(
    IXLWorksheet sheet,
    string [] cols,
    List<IXLCell> cells,
    ref int nItemIndex,
    ref int nRowIndex,
    ref List<int> column_max_chars)
        {
            int nColIndex = 2;
            foreach (string s in cols)
            {
                // 统计最大字符数
                ReaderSearchForm.SetMaxChars(ref column_max_chars, nColIndex - 1, ReaderSearchForm.GetCharWidth(s));

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

        public static void BuildRfidWriteReport(
IEnumerable<RfidWriteInfo> lines,
IXLWorksheet sheet)
        {
            List<int> column_max_chars = new List<int>();

            List<KeyStatisLine> results =
                lines
                // .Where(o => bSeries || string.IsNullOrEmpty(o.State) == false)
                .GroupBy(p => (string)p.ItemRefID)
                .Select(cl => new KeyStatisLine
                {
                    Key = (string)cl.First().ItemRefID,
                    ItemBarcode = cl.First().ItemBarcode,
                    Operator = cl.First().Operator,
                    Count = cl.Count(),
                }).ToList();
            int line = 0;
            // 栏目标题行
            string[] titles = new string[] {
                "参考ID",
                "册条码号", 
                "写入次数",
                "操作者",
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

    // 日志原始信息。每个对象对应于一个日志记录
    public class RfidWriteInfo
    {
        public int Index { get; set; }
        public string Date { get; set; }
        public string ItemBarcode { get; set; }
        public string ItemRefID { get; set; }
        public string Operator { get; set; }

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
            return info;
        }
    }
}
