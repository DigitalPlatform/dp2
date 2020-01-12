using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.IO;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class Report
    {
        public static void BuildReport121(LibraryContext context,
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
                    PatronBarcode = patron.Barcode,
                    Name = patron.Name,
                    Department = patron.Department,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0
                }
            )
            .DefaultIfEmpty()
            .GroupBy(x => new { x.PatronBarcode, x.Name, x.Department },
            (key, items) => new
            {
                PatronBarcode = key.PatronBarcode,
                key.Name,
                key.Department,
                BorrowCount = items.Sum(t => t.BorrowCount),
                ReturnCount = items.Sum(t => t.ReturnCount)
            }
            )
            .OrderByDescending(t => t.BorrowCount).ThenBy(t => t.Name)
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

        // 测试创建按部门的图书借阅排行榜
        public static void BuildReport101(LibraryContext context,
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
            .GroupBy(x => x.Department)
            .Select(g => new
            {
                Department = g.Key,
                BorrowCount = g.Sum(t => t.BorrowCount),
                ReturnCount = g.Sum(t => t.ReturnCount)
            })
            .DefaultIfEmpty()
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

        // 测试创建按部门的图书借阅排行榜
        public static void TestBuildReport0(LibraryContext context,
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
