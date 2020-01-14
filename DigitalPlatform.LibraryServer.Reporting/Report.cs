using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.EntityFrameworkCore;

using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer.Reporting
{
    public class Report
    {
        public static void BuildReport(LibraryContext context,
    Hashtable param_table,
    ReportWriter writer,
    string strOutputFileName)
        {
            string strDateRange = param_table["dateRange"] as string;

            string strStartDate = "";
            string strEndDate = "";
            if (string.IsNullOrEmpty(strDateRange) == false)
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }

            Hashtable macro_table = new Hashtable();

            macro_table["%daterange%"] = strDateRange;
            macro_table["%library%"] = param_table["libraryCode"] as string;

            switch (writer.Algorithm)
            {
                case "101":
                    BuildReport_101(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "111":
                    BuildReport_111(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "121":
                    BuildReport_121(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "131":
                    BuildReport_131(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "141":
                    BuildReport_141(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "201":
                    BuildReport_201(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "202":
                    BuildReport_202(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
            }

            throw new Exception($"算法 {writer.Algorithm} 没有找到");
        }

        static string GetLibraryCode(string location)
        {
            return StringUtil.ParseTwoPart(location, "/")[0];
        }

        // 某段时间内没有被借出过的图书。这里有个疑问，就是这一段时间以前借了但在这一段时间内来不及还的算不算借过？
        // parameters:
        //      param_table 要求 location 参数。表示一个馆藏地，例如 "/阅览室"。注意使用新版的正规形态，其中必须包含一个斜杠
        public static void BuildReport_202(LibraryContext context,
            Hashtable param_table,
            string strStartDate,
            string strEndDate,
            ReportWriter writer,
            Hashtable macro_table,
            string strOutputFileName)
        {
            string location = param_table["location"] as string;
            // string librarycode = GetLibraryCode(location);

            macro_table["%location%"] = location;

            var items = context.CircuOpers
            .Where(b => // (b.LibraryCode == librarycode) &&
            b.Action == "borrow"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
            .Join(
                context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    item.ItemBarcode,
                    Location = item.Location,
                }
            )
            .DefaultIfEmpty()
            .Where(x => x.Location == location)
            .Select(x => x.ItemBarcode).ToList();

            /*
            var results = context.Items
                .ToList()
                .Where(x => x.Location == location && !items.Contains(x.ItemBarcode))
                .GroupBy(x => x.BiblioRecPath)
                .Select(g => new
                {
                    BiblioRecPath = g.Key,
                    ItemCount = g.Count(),
                    BarcodeList = g.Select(x => x.ItemBarcode).ToArray()
                })
                .Join(context.Biblios,
            item => item.BiblioRecPath,
            biblio => biblio.RecPath,
            (item, biblio) => new
            {
                BiblioRecPath = item.BiblioRecPath,
                Summary = biblio.Summary,
                ItemCount = item.ItemCount,
                Barcodes = string.Join(",", item.BarcodeList),
            })
            .OrderBy(t => t.BiblioRecPath)
            .ToList();

                  <column name="册条码号列表" align="left" sum="no" class="Barcodes" eval="" />

            */

            var results = context.Items
    .Where(x => x.Location == location && !items.Contains(x.ItemBarcode))
    .GroupBy(x => x.BiblioRecPath)
    .Select(g => new
    {
        BiblioRecPath = g.Key,
        ItemCount = g.Count(),
    })
    .Join(context.Biblios,
item => item.BiblioRecPath,
biblio => biblio.RecPath,
(item, biblio) => new
{
    BiblioRecPath = item.BiblioRecPath,
    Summary = biblio.Summary,
    ItemCount = item.ItemCount,
})
.OrderBy(t => t.BiblioRecPath)
.ToList();


            int nRet = writer.OutputRmlReport(
            results,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 按图书种的借阅排行榜
        // parameters:
        //      param_table 要求 location 参数。表示一个馆藏地，例如 "/阅览室"。注意使用新版的正规形态，其中必须包含一个斜杠
        public static void BuildReport_201(LibraryContext context,
            Hashtable param_table,
            string strStartDate,
            string strEndDate,
            ReportWriter writer,
            Hashtable macro_table,
            string strOutputFileName)
        {
            string location = param_table["location"] as string;
            // string librarycode = GetLibraryCode(location);

            var opers = context.CircuOpers
            .Where(b => // (b.LibraryCode == librarycode) &&
            (b.Action == "borrow" || b.Action == "return")
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
            .Join(
                context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    Location = item.Location,
                    BiblioRecPath = item.BiblioRecPath,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0
                }
            )
            .DefaultIfEmpty()
            .Where(x => x.Location == location)
            .GroupBy(x => x.BiblioRecPath)
            .Select(g => new
            {
                BiblioRecPath = g.Key,
                BorrowCount = g.Sum(t => t.BorrowCount),
                ReturnCount = g.Sum(t => t.ReturnCount)
            })
            .DefaultIfEmpty()
            .Join(context.Biblios,
            item => item.BiblioRecPath,
            biblio => biblio.RecPath,
            (item, biblio) => new
            {
                RecPath = item.BiblioRecPath,
                Summary = biblio.Summary,
                BorrowCount = item.BorrowCount,
                ReturnCount = item.ReturnCount
            })
            .OrderByDescending(t => t.BorrowCount).ThenBy(t => t.RecPath)
            .ToList();

            int nRet = writer.OutputRmlReport(
            opers,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 超期的读者和图书清单
        // parameters:
        //      param_table 要求 libraryCode/endDate 参数
        //                  endDate 统计日期，也就是计算超期的日期，如果为空表示今天
        public static void BuildReport_141(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            DateTime end;

            string date = "";
            date = param_table["endDate"] as string;
            if (string.IsNullOrEmpty(date) == false)
            {
                end = DateTimeUtil.Long8ToDateTime(date);
            }
            else
            {
                date = strEndDate;
                end = DateTimeUtil.Long8ToDateTime(strEndDate);
                DateTime today = DateTime.Now;
                if (end > today)
                    end = today;
            }

            string strLibraryCode = param_table["libraryCode"] as string;

            // 筛选分馆
            if (strLibraryCode != "*")
                strLibraryCode += "/";

            // 从全部册记录里面选择那些超期的
            var items = context.Items
                .Where(x => (strLibraryCode == "*" || x.Location.StartsWith(strLibraryCode))
                && string.IsNullOrEmpty(x.Borrower) == false
                && x.ReturningTime < end)
                .Join(context.Patrons,
                item => item.Borrower,
                patron => patron.Barcode,
                (item, patron) => new
                {
                    ItemBarcode = item.ItemBarcode,
                    PatronBarcode = item.Borrower,
                    Period = item.BorrowPeriod,
                    Name = patron.Name,
                    Department = patron.Department,
                    BorrowTime = item.BorrowTime,
                    ReturningTime = item.ReturningTime,
                    Summary = context
                    .Biblios.Where(x => x.RecPath == item.BiblioRecPath)
                    .Select(a => a.Summary)
                    .FirstOrDefault()
                })
            .OrderBy(t => t.BorrowTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

#if NO
        // 超期的读者和图书清单
        // parameters:
        //      parameters  附加的参数。统计日期，也就是计算超期的日期，如果为空表示今天
        public static void BuildReport_141(LibraryContext context,
string strLibraryCode,
string strStartDate,
string strEndDate,
string[] parameters,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            DateTime end = DateTimeUtil.Long8ToDateTime(strEndDate);

            var opers = context.CircuOpers
            .Where(b => b.Action == "borrow")
            .Select(a => new
            {
                a.ItemBarcode,
                a.ReaderBarcode,
                BorrowTime = a.OperTime,
                a.ReturningTime,
                ReturnTime = context
                    .CircuOpers.Where(x => x.ReaderBarcode == a.ReaderBarcode
                    && x.Action == "return"
                    && x.OperTime >= a.OperTime)
                    .Select(x => new { x.OperTime })
                    .OrderBy(x => x.OperTime)
                    .FirstOrDefault().OperTime
            })
            .Where(x => x.ReturnTime == null && x.ReturningTime < end)
            .Join(
                context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    item.ItemBarcode,
                    PatronBarcode = oper.ReaderBarcode,
                    oper.BorrowTime,
                    oper.ReturnTime,
                    oper.ReturningTime,
                    Summary = context
                    .Biblios.Where(x => x.RecPath == item.BiblioRecPath)
                    .Select(a => a.Summary)
                    .FirstOrDefault()
                }
            )
            .Join(
                context.Patrons,
                item => item.PatronBarcode,
                patron => patron.Barcode,
                (item, patron) => new {
                    item.PatronBarcode,
                    item.ItemBarcode,
                    item.BorrowTime,
                    item.ReturningTime,
                    item.Period,
                    patron.Name,
                    patron.Department,
                }
                )
            .DefaultIfEmpty()
            .OrderBy(t => t.BorrowTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            opers,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

#endif

        // 单个读者的借阅清单
        // parameters:
        //      param_table 要求 patronBarcode 参数
        //                  patronBarcode 读者证条码号
        public static void BuildReport_131(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            string patronBarcode = param_table["patronBarcode"] as string;

            macro_table["%readerbarcode%"] = patronBarcode;

            var opers = context.CircuOpers
            .Where(b => (b.ReaderBarcode == patronBarcode)
            && b.Action == "borrow"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
            .Join(
                context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    ItemBarcode = item.ItemBarcode,
                    BorrowTime = oper.OperTime,
                    ReturnTime = context
                    .CircuOpers.Where(x => x.ReaderBarcode == patronBarcode && x.Action == "return" && x.OperTime >= oper.OperTime)
                    .Select(a => new { a.OperTime })
                    .OrderBy(a => a.OperTime)
                    .FirstOrDefault().OperTime,
                    Summary = context
                    .Biblios.Where(x => x.RecPath == item.BiblioRecPath)
                    .Select(a => a.Summary)
                    .FirstOrDefault()
                }
            )
            .DefaultIfEmpty()
            .OrderBy(t => t.BorrowTime)
            .ToList();

            // %name% %readerbarcode% %department%
            var patrons = context.Patrons.Where(x => x.Barcode == patronBarcode)
                .Select(x => new { x.Name, x.Department })
                .ToList();
            macro_table["%name%"] = patrons.Count == 0 ? "" : patrons[0].Name;
            macro_table["%department%"] = patrons.Count == 0 ? "" : patrons[0].Department;

            int nRet = writer.OutputRmlReport(
            opers,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 读者姓名的借阅排行
        // parameters:
        //      param_table 要求 libraryCode 参数
        public static void BuildReport_121(LibraryContext context,
    Hashtable param_table,
    string strStartDate,
    string strEndDate,
    ReportWriter writer,
    Hashtable macro_table,
    string strOutputFileName)
        {
            string strLibraryCode = param_table["libraryCode"] as string;

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

            int nRet = writer.OutputRmlReport(
            opers,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 读者类型的借阅排行
        // parameters:
        //      param_table 要求 libraryCode 参数
        public static void BuildReport_111(LibraryContext context,
    Hashtable param_table,
    string strStartDate,
    string strEndDate,
    ReportWriter writer,
    Hashtable macro_table,
    string strOutputFileName)
        {
            string strLibraryCode = param_table["libraryCode"] as string;

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
                    ReaderType = patron.ReaderType,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0
                }
            )
            .DefaultIfEmpty()
            .GroupBy(x => x.ReaderType)
            .Select(g => new
            {
                PatronType = g.Key,
                BorrowCount = g.Sum(t => t.BorrowCount),
                ReturnCount = g.Sum(t => t.ReturnCount)
            })
            .DefaultIfEmpty()
            .OrderByDescending(t => t.BorrowCount).ThenBy(t => t.PatronType)
            .ToList();

            int nRet = writer.OutputRmlReport(
            opers,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 按部门的图书借阅排行榜
        // parameters:
        //      param_table 要求 libraryCode 参数
        public static void BuildReport_101(LibraryContext context,
            Hashtable param_table,
            string strStartDate,
            string strEndDate,
            ReportWriter writer,
            Hashtable macro_table,
            string strOutputFileName)
        {
            string strLibraryCode = param_table["libraryCode"] as string;

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
            .DefaultIfEmpty()
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
