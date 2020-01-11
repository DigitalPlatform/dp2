using DigitalPlatform.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class Report
    {
        // 测试创建按部门的图书借阅排行榜
        public static void TestBuildReport(LibraryContext context,
            string strLibraryCode,
            string strDateRange,
            ReportWriter writer,
            Hashtable macro_table,
            string strOutputFileName)
        {
            // 将日期字符串解析为起止范围日期
            // throw:
            //      Exception
            DateTimeUtil.ParseDateRange(strDateRange,
                out string strStartDate,
                out string strEndDate);
            if (string.IsNullOrEmpty(strEndDate) == true)
                strEndDate = strStartDate;

            /*
            var opers = context.CircuOpers
    .Where(b => string.Compare(b.Date, strStartDate) >= 0 && string.Compare(b.Date, strEndDate) <= 0)
    .ToList();
    */
            var opers = context.CircuOpers
            .Where(b => (strLibraryCode == "*" || b.LibraryCode == strLibraryCode)
            && string.Compare(b.Date, strStartDate) >= 0 
            && string.Compare(b.Date, strEndDate) <= 0)
            .Join(
                context.Patrons,
                oper => oper.ReaderBarcode,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    Department = patron.Department,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0
                }
            )
            .AsEnumerable()
            .GroupBy(x => x.Department)
            .Select(g => new
            {
                Department = g.FirstOrDefault().Department,
                BorrowCount = g.Sum(t => t.BorrowCount),
                ReturnCount = g.Sum(t => t.ReturnCount)
            })
            .OrderByDescending(t => t.BorrowCount).ThenBy(t => t.Department)
            .ToList();

            macro_table["%daterange%"] = strDateRange;
            macro_table["%library%"] = strLibraryCode;

            int nRet = writer.OutputRmlReport(
            opers,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        public static bool MatchLibraryCode(string libraryCode, string pattern)
        {
            if (pattern == "*")
                return true;
            return string.Compare(libraryCode, pattern) == 0;
        }
    }
}
