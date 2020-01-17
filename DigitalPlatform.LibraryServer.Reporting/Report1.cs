using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
                case "212":
                    BuildReport_212(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "301":
                    BuildReport_301(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "302":
                    BuildReport_302(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "411":
                    BuildReport_411(context,
param_table,
strStartDate,
strEndDate,
"setOrder",
writer,
macro_table,
strOutputFileName);
                    return;
                case "412":
                    BuildReport_412(context,
param_table,
strStartDate,
strEndDate,
"setOrder",
writer,
macro_table,
strOutputFileName);
                    return;
                case "421":
                    BuildReport_421(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "422":
                    BuildReport_422(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "431":
                    BuildReport_431(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "432":
                    BuildReport_432(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "441":
                    BuildReport_441(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "442":
                    BuildReport_442(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "443":
                    BuildReport_443(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "451":
                    BuildReport_411(context,
param_table,
strStartDate,
strEndDate,
"setIssue",
writer,
macro_table,
strOutputFileName);
                    return;
                case "452":
                    BuildReport_412(context,
param_table,
strStartDate,
strEndDate,
"setIssue",
writer,
macro_table,
strOutputFileName);
                    return;
                case "471":
                    BuildReport_471(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "472":
                    BuildReport_472(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "481":
                    BuildReport_481(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "482":
                    BuildReport_482(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "491":
                    BuildReport_491(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "492":
                    BuildReport_492(context,
param_table,
strStartDate,
strEndDate,
writer,
macro_table,
strOutputFileName);
                    return;
                case "493":
                    BuildReport_493(context,
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

        // 获取对象操作量。按分类。注意操作者既可能是工作人员，也可能是读者
        // parameters:
        public static void BuildReport_493(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            string classType = param_table["classType"] as string;
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            string libraryCode0 = libraryCode;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;
            macro_table["%class%"] = classType;

#if NO
            var items = context.GetResOpers
                .Where(b => b.Operation == "getRes"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Patrons,
                oper => oper.Operator,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    PatronLibraryCode = patron == null ? null : patron.LibraryCode,
                    oper.Operator,
                    oper.Size,
                    oper.XmlRecPath
                }
                )
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    oper.PatronLibraryCode,
                    UserLibraryCode = user == null ? null : user.LibraryCodeList,
                    oper.Operator,
                    oper.XmlRecPath,
                    GetCount = 1,
                    GetSize = Convert.ToInt64(oper.Size),
                    TotalCount = 1,
                }
                )
                .Where(x => (x.UserLibraryCode != null && (libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1))
                || (x.PatronLibraryCode != null && (libraryCode0 == "*" || x.PatronLibraryCode == libraryCode0)))
                .LeftJoin(
                context.Keys,
                item => item.XmlRecPath,
                key => key.BiblioRecPath,
                (item, key) => new
                {
                    key.Type,
                    Class = string.IsNullOrEmpty(key.Text) ? "" : key.Text.Substring(0, 1),
                    item.GetCount,
                    item.GetSize,
                }
            )
            .Where(x => x.Type == classType)
            .GroupBy(x => x.Class)
            .Select(g => new
            {
                Class = g.Key,
                GetCount = g.Sum(x => x.GetCount),
                GetSize = g.Sum(x => x.GetSize),
            })
                .OrderBy(x => x.Class)
            .ToList();
#endif

            var items = context.GetResOpers
                .Where(b => b.Operation == "getRes"
                    && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                    && string.Compare(b.Date, strStartDate) >= 0
                    && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                    context.Patrons,
                    oper => oper.Operator,
                    patron => patron.Barcode,
                    (oper, patron) => new
                    {
                        // 多次 join 时的典型写法，传递对象而不是属性：
                        oper,
                        patron, // 注意可能为空
                    })
                .LeftJoin(
                    context.Users,
                    j1 => j1.oper.Operator,
                    user => user.ID,
                    (j1, user) => new
                    {
                        // 因为后面 where 马上要用到，所以给一个具体的名字
                        PatronLibraryCode = j1.patron == null ? null : j1.patron.LibraryCode,
                        UserLibraryCode = user == null ? null : user.LibraryCodeList,

                        // 向后传递对象
                        j1.oper,
                        // user,
                    })
                .Where(x => (x.UserLibraryCode != null && (libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1))
                    || (x.PatronLibraryCode != null && (libraryCode0 == "*" || x.PatronLibraryCode == libraryCode0)))
                .LeftJoin(
                    context.Keys,
                    j2 => new { key1 = j2.oper.XmlRecPath, key2 = classType },
                    key => new { key1 = key.BiblioRecPath, key2 = key.Type },
                    (j2, key) => new
                    {
                        Class = string.IsNullOrEmpty(key.Text) ? "" : key.Text.Substring(0, 1),
                        GetCount = 1,
                        GetSize = Convert.ToInt64(j2.oper.Size),
                    }
                )
                .GroupBy(x => x.Class)
                .Select(g => new
                {
                    Class = g.Key,
                    GetCount = g.Sum(x => x.GetCount),
                    GetSize = g.Sum(x => x.GetSize),
                })
                .OrderBy(x => x.Class)
                .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 获取对象操作量。按操作者。注意操作者既可能是工作人员，也可能是读者
        // parameters:
        public static void BuildReport_492(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            string libraryCode0 = libraryCode;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.GetResOpers
                .Where(b => b.Operation == "getRes"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Patrons,
                oper => oper.Operator,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    PatronLibraryCode = patron == null ? null : patron.LibraryCode,
                    // Operation = oper.Action,
                    oper.Operator,
                    // oper.Action,
                    // oper.XmlRecPath,
                    oper.Size,
                }
                )
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    oper.PatronLibraryCode,
                    UserLibraryCode = user == null ? null : user.LibraryCodeList,
                    // oper.Action,
                    oper.Operator,
                    // oper.Size,
                    // oper.XmlRecPath,
                    GetCount = 1,
                    GetSize = Convert.ToInt64(oper.Size),
                    TotalCount = 1,
                }
                )
                .Where(x => (x.UserLibraryCode != null && (libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1))
                || (x.PatronLibraryCode != null && (libraryCode0 == "*" || x.PatronLibraryCode == libraryCode0)))
            .GroupBy(x => x.Operator)
            .Select(g => new
            {
                Operator = g.Key,
                GetCount = g.Sum(x => x.GetCount),
                GetSize = g.Sum(x => x.GetSize),
                TotalCount = g.Sum(x => x.TotalCount),
            })
                .LeftJoin(
                context.Patrons,
                oper => oper.Operator,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    oper.Operator,
                    oper.GetCount,
                    oper.GetSize,
                    oper.TotalCount,
                    Name = patron == null ? null : patron.Name,
                    Department = patron == null ? null : patron.Department,
                }
                ).OrderBy(x => x.Operator)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 获取对象流水
        // parameters:
        public static void BuildReport_491(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            string libraryCode0 = libraryCode;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.GetResOpers
                .Where(b => b.Operation == "getRes"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Patrons,
                oper => oper.Operator,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    PatronLibraryCode = patron == null ? null : patron.LibraryCode,
                    Operation = oper.Action,
                    oper.OperTime,
                    oper.Operator,
                    oper.Action,
                    oper.XmlRecPath,
                    oper.Size,
                    oper.ObjectID,
                }
                )
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    oper.PatronLibraryCode,
                    UserLibraryCode = user == null ? null : user.LibraryCodeList,
                    oper.Action,
                    oper.Operator,
                    oper.OperTime,
                    oper.XmlRecPath,
                    oper.Size,
                    oper.ObjectID,
                }
                )
                .Where(x => (x.UserLibraryCode != null && (libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1))
                || (x.PatronLibraryCode != null && (libraryCode0 == "*" || x.PatronLibraryCode == libraryCode0)))
            .LeftJoin(
                context.Biblios,
                oper => oper.XmlRecPath,
                biblio => biblio.RecPath,
                (oper, biblio) => new
                {
                    Operation = oper.Action,
                    BiblioRecPath = biblio == null ? null : biblio.RecPath,
                    Summary = biblio.Summary,
                    ObjectSize = oper.Size,
                    oper.ObjectID,
                    OperTime = oper.OperTime,
                    Operator = oper.Operator
                }
            )
            .OrderBy(x => x.OperTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 入馆登记工作量，按门名称
        // parameters:
        public static void BuildReport_482(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.PassGateOpers
                .Where(b => b.Operation == "passgate"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .Select(oper => new
                {
                    oper.GateName,
                    PassCount = oper.Action == "" ? 1 : 0,
                    TotalCount = 1,
                })
            .GroupBy(x => x.GateName)
            .Select(g => new
            {
                GateName = g.Key,
                PassCount = g.Sum(x => x.PassCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .OrderBy(x => x.GateName)
            .ToList();

            /*
            var items = context.PassGateOpers
                .Where(b => b.Operation == "passgate"
    && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
&& string.Compare(b.Date, strStartDate) >= 0
&& string.Compare(b.Date, strEndDate) <= 0)
                .GroupBy(x => x.GateName)
                .Select(g => new
                {
                    GateName = g.Key,
                    PassCount = g.Count(x => x.Action == "passgate"),
                    TotalCount = g.Count(),
                })
                .OrderBy(x => x.GateName)
                .ToList();
                */

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 入馆登记流水
        // 用读者记录的馆代码来进行分馆筛选; 或者用日志记录的馆代码来筛选?
        // parameters:
        public static void BuildReport_481(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.PassGateOpers
                .Where(b => b.Operation == "passgate"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Patrons,
                oper => oper.ReaderBarcode,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    GateName = oper.GateName,
                    PatronBarcode = oper.ReaderBarcode,
                    PatronName = patron.Name,
                    patron.Department,
                    Operation = oper.Action,
                    oper.OperTime,
                    oper.Operator,
                }
                )
            .OrderBy(x => x.OperTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 违约金工作量，按操作者
        // parameters:
        public static void BuildReport_472(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.AmerceOpers
                .Where(b => b.Operation == "amerce"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .Select(oper => new
                {
                    oper.Operator,
                    AmerceCount = oper.Action == "amerce" ? 1 : 0,
                    AmerceMoney = oper.Action == "amerce" ? oper.Unit + oper.Price.ToString() : "",

                    ModifyCount = oper.Action == "modifyprice" ? 1 : 0,
                    ModifyMoney = oper.Action == "modifyprice" ? oper.Unit + oper.Price.ToString() : "",

                    UndoCount = oper.Action == "undo" ? 1 : 0,
                    UndoMoney = oper.Action == "undo" ? oper.Unit + oper.Price.ToString() : "",

                    ExpireCount = oper.Action == "expire" ? 1 : 0,

                    TotalCount = 1,
                }
                )
                .AsEnumerable()
            .GroupBy(x => x.Operator)
            .Select(g => new
            {
                Operator = g.Key,
                AmerceCount = g.Sum(x => x.AmerceCount),
                AmerceMoney = g.SumPrice(x => x.AmerceMoney),
                ModifyCount = g.Sum(x => x.ModifyCount),
                ModifyMoney = g.SumPrice(x => x.ModifyMoney),
                UndoCount = g.Sum(x => x.UndoCount),
                UndoMoney = g.SumPrice(x => x.UndoMoney),
                ExpireCount = g.Sum(x => x.ExpireCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .Select(x => new
            {
                x.Operator,
                x.AmerceCount,
                x.AmerceMoney,
                x.ModifyCount,
                x.ModifyMoney,
                x.UndoCount,
                x.UndoMoney,
                x.ExpireCount,
                x.TotalCount,
                FinalAmerceMoney = Substract(x.AmerceMoney, x.UndoMoney),
            })
            .OrderBy(x => x.Operator)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        public static string Substract(string p1, string p2)
        {
            var price = new PriceUtil();
            price.Set(p1).Substract(p2);
            return price.ToString();
        }

        public static string Divide(string p1, string p2)
        {
            var price = new PriceUtil();
            price.Set(p1).Divide(p2);
            return price.ToString();
        }

        /*
        public static string SumPrice(this IEnumerable<string> collection)
        {
            return collection.Aggregate((a, b) => PriceUtil.Add(a, b));
        }
        */

        // 违约金流水
        // 用读者记录的馆代码来进行分馆筛选; 或者用日志记录的馆代码来筛选?
        // parameters:
        public static void BuildReport_471(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            /*
            DateTime start_time = DateTimeUtil.Long8ToDateTime(strStartDate);

            // TODO: 12 点
            DateTime end_time = DateTimeUtil.Long8ToDateTime(strEndDate);
            */

            var items = context.AmerceOpers
                .Where(b => b.Operation == "amerce"
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Patrons,
                oper => oper.ReaderBarcode,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    oper.AmerceRecPath,
                    oper.Reason,
                    oper.Price,
                    oper.Unit,
                    oper.ReaderBarcode,
                    PatronName = patron.Name,
                    patron.Department,
                    oper.ItemBarcode,
                    Operation = oper.Action,
                    oper.OperTime,
                    oper.Operator,
                }
                )
                .LeftJoin(context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    oper.AmerceRecPath,
                    oper.Reason,
                    oper.Price,
                    oper.Unit,
                    oper.ReaderBarcode,
                    oper.PatronName,
                    oper.Department,
                    oper.ItemBarcode,
                    item.BiblioRecPath,
                    oper.Operation,
                    oper.OperTime,
                    oper.Operator
                })
            .LeftJoin(
                context.Biblios,
                oper => oper.BiblioRecPath,
                biblio => biblio.RecPath,
                (oper, biblio) => new
                {
                    oper.AmerceRecPath,
                    oper.Reason,
                    Price = oper.Price,
                    oper.Unit,
                    PatronBarcode = oper.ReaderBarcode,
                    oper.PatronName,
                    oper.Department,
                    oper.ItemBarcode,
                    biblio.Summary,
                    oper.Operation,
                    oper.OperTime,
                    oper.Operator
                }
            )
            .OrderBy(x => x.OperTime)
            /*
            .AsEnumerable()
            .Select(item => new
            {
                item.AmerceRecPath,
                item.Reason,
                Price = item.Price,
                item.Unit,
                item.PatronBarcode,
                item.PatronName,
                item.Department,
                item.ItemBarcode,
                item.Summary,
                item.Operation,
                item.OperTime,
                item.Operator
            }) */
            .ToList();

            // TODO: 进行 sum?

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 出纳工作量，按馆藏地点
        // parameters:
        public static void BuildReport_443(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.CircuOpers
                .Where(b => (b.Operation == "borrow" || b.Operation == "return")
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    item.Location,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    RenewCount = oper.Action == "renew" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0,
                    LostCount = oper.Action == "lost" ? 1 : 0,
                    ReadCount = oper.Action == "read" ? 1 : 0,
                    TotalCount = 1,
                })
            .GroupBy(x => x.Location)
            .Select(g => new
            {
                Location = g.Key,
                BorrowCount = g.Sum(x => x.BorrowCount),
                RenewCount = g.Sum(x => x.RenewCount),
                ReturnCount = g.Sum(x => x.ReturnCount),
                LostCount = g.Sum(x => x.LostCount),
                ReadCount = g.Sum(x => x.ReadCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .OrderBy(x => x.Location)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 出纳工作量，按操作者
        // parameters:
        public static void BuildReport_442(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.CircuOpers
                .Where(b => (b.Operation == "borrow" || b.Operation == "return")
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .Select(oper => new
                {
                    oper.Operator,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    RenewCount = oper.Action == "renew" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0,
                    LostCount = oper.Action == "lost" ? 1 : 0,
                    ReadCount = oper.Action == "read" ? 1 : 0,
                    TotalCount = 1,
                }
                )
            .GroupBy(x => x.Operator)
            .Select(g => new
            {
                Operator = g.Key,
                BorrowCount = g.Sum(x => x.BorrowCount),
                RenewCount = g.Sum(x => x.RenewCount),
                ReturnCount = g.Sum(x => x.ReturnCount),
                LostCount = g.Sum(x => x.LostCount),
                ReadCount = g.Sum(x => x.ReadCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .OrderBy(x => x.Operator)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 出纳流水
        // 用读者记录的馆代码来进行分馆筛选; 或者用日志记录的馆代码来筛选?
        // parameters:
        public static void BuildReport_441(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            DateTime start_time = DateTimeUtil.Long8ToDateTime(strStartDate);

            // TODO: 12 点
            DateTime end_time = DateTimeUtil.Long8ToDateTime(strEndDate);

            var items = context.CircuOpers
                .Where(b => (b.Operation == "borrow" || b.Operation == "return")
                && (libraryCode == "*" || b.LibraryCode.IndexOf(libraryCode) != -1)
                && string.Compare(b.Date, strStartDate) >= 0
                && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Patrons,
                oper => oper.ReaderBarcode,
                patron => patron.Barcode,
                (oper, patron) => new
                {
                    oper.ReaderBarcode,
                    PatronName = patron.Name,
                    patron.Department,
                    oper.ItemBarcode,
                    Operation = oper.Action,
                    oper.OperTime,
                    oper.Operator,
                }
                )
                .LeftJoin(context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    oper.ReaderBarcode,
                    oper.PatronName,
                    oper.Department,
                    oper.ItemBarcode,
                    item.BiblioRecPath,
                    oper.Operation,
                    oper.OperTime,
                    oper.Operator
                })
            .LeftJoin(
                context.Biblios,
                oper => oper.BiblioRecPath,
                biblio => biblio.RecPath,
                (oper, biblio) => new
                {
                    PatronBarcode = oper.ReaderBarcode,
                    oper.PatronName,
                    oper.Department,
                    oper.ItemBarcode,
                    biblio.Summary,
                    oper.Operation,
                    oper.OperTime,
                    oper.Operator
                }
            )
            .OrderBy(x => x.OperTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 册登记工作量
        // 和 BuildReport_412() 的差异是多输出了一个 transfer(转移) 列
        // parameters:
        public static void BuildReport_432(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            var items = context.ItemOpers
                .Where(b => b.Operation == "setEntity"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    UserLibraryCode = user == null ? "" : user.LibraryCodeList,
                    oper.Action,
                    oper.Operator,
                    NewCount = oper.Action == "new" ? 1 : 0,
                    ChangeCount = oper.Action == "change" ? 1 : 0,
                    DeleteCount = oper.Action == "delete" ? 1 : 0,
                    CopyCount = oper.Action == "copy" ? 1 : 0,
                    MoveCount = oper.Action == "move" ? 1 : 0,
                    TransferCount = oper.Action == "transfer" ? 1 : 0,
                    TotalCount = 1,
                }
                )
                .Where(x => libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1)
            .GroupBy(x => x.Operator)
            .Select(g => new
            {
                Operator = g.Key,
                NewCount = g.Sum(x => x.NewCount),
                ChangeCount = g.Sum(x => x.ChangeCount),
                DeleteCount = g.Sum(x => x.DeleteCount),
                CopyCount = g.Sum(x => x.CopyCount),
                MoveCount = g.Sum(x => x.MoveCount),
                TransferCount = g.Sum(x => x.TransferCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .OrderBy(x => x.Operator)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 册登记流水
        // 和 BuildReport_411() 的区别是多输出一列 ItemBarcode
        // parameters:
        public static void BuildReport_431(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            DateTime start_time = DateTimeUtil.Long8ToDateTime(strStartDate);

            // TODO: 12 点
            DateTime end_time = DateTimeUtil.Long8ToDateTime(strEndDate);

            // 权且用操作者的所属馆代码来匹配 libraryCode
            var items = context.ItemOpers
                .Where(b => b.Operation == "setEntity"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    UserLibraryCode = user.LibraryCodeList,
                    oper.BiblioRecPath,
                    oper.ItemRecPath,
                    oper.Operation,
                    oper.Action,
                    oper.OperTime,
                    oper.Operator,
                }
                )
                .Where(x => libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1)
            .LeftJoin(
                context.Biblios,
                oper => oper.BiblioRecPath,
                biblio => biblio.RecPath,
                (oper, biblio) => new
                {
                    Operation = oper.Operation + "." + oper.Action,
                    oper.ItemRecPath,
                    BiblioRecPath = oper.BiblioRecPath,
                    Summary = biblio.Summary,
                    OperTime = oper.OperTime,
                    Operator = oper.Operator
                }
            )
            .LeftJoin(
                context.Items,
                oper => oper.ItemRecPath,
                item => item.ItemRecPath,
                (oper, item) => new
                {
                    oper.Operation,
                    oper.ItemRecPath,
                    oper.BiblioRecPath,
                    oper.Summary,
                    oper.OperTime,
                    oper.Operator,
                    item.ItemBarcode,
                }
            ).OrderBy(x => x.OperTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 编目工作量
        // parameters:
        public static void BuildReport_422(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            // TODO: 订购记录怎么看出是哪个分馆的? 1) 从操作者的权限可以看出 2) 从日志记录的 libraryCode 可以看出 3) 从订购记录的 distribute 元素可以看出
            var items = context.BiblioOpers
                .Where(b => b.Operation == "setBiblioInfo"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    UserLibraryCode = user == null ? "" : user.LibraryCodeList,
                    oper.Action,
                    oper.Operator,
                    NewCount = oper.Action == "new" ? 1 : 0,
                    ChangeCount = oper.Action == "change" ? 1 : 0,
                    DeleteCount = oper.Action == "delete" ? 1 : 0,
                    CopyCount = oper.Action == "copy" ? 1 : 0,
                    MoveCount = oper.Action == "move" ? 1 : 0,
                    TotalCount = 1,
                }
                )
                .Where(x => libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1)
            .GroupBy(x => x.Operator)
            .Select(g => new
            {
                Operator = g.Key,
                NewCount = g.Sum(x => x.NewCount),
                ChangeCount = g.Sum(x => x.ChangeCount),
                DeleteCount = g.Sum(x => x.DeleteCount),
                CopyCount = g.Sum(x => x.CopyCount),
                MoveCount = g.Sum(x => x.MoveCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .OrderBy(x => x.Operator)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 编目流水
        // parameters:
        public static void BuildReport_421(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;


            // 编目操作都是全局的，不属于某个分馆。这里权且按照工作人员所属的分馆来进行统计
            var items = context.BiblioOpers
                .Where(b => b.Operation == "setBiblioInfo"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    UserLibraryCode = user.LibraryCodeList,
                    oper.BiblioRecPath,
                    oper.Operation,
                    oper.Action,
                    oper.OperTime,
                    oper.Operator,
                }
                )
                .Where(x => libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1)
            .LeftJoin(
                context.Biblios,
                oper => oper.BiblioRecPath,
                biblio => biblio.RecPath,
                (oper, biblio) => new
                {
                    Operation = oper.Operation + "." + oper.Action,
                    BiblioRecPath = oper.BiblioRecPath,
                    Summary = biblio.Summary,
                    OperTime = oper.OperTime,
                    Operator = oper.Operator
                }
            )
            .OrderBy(x => x.OperTime)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 订购工作量
        // parameters:
        //      operation   为 setEntity 或 setOrder 等
        public static void BuildReport_412(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
string operation,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            // TODO: 订购记录怎么看出是哪个分馆的? 1) 从操作者的权限可以看出 2) 从日志记录的 libraryCode 可以看出 3) 从订购记录的 distribute 元素可以看出
            var items = context.ItemOpers
                .Where(b => b.Operation == operation
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    UserLibraryCode = user == null ? "" : user.LibraryCodeList,
                    oper.Action,
                    oper.Operator,
                    NewCount = oper.Action == "new" ? 1 : 0,
                    ChangeCount = oper.Action == "change" ? 1 : 0,
                    DeleteCount = oper.Action == "delete" ? 1 : 0,
                    CopyCount = oper.Action == "copy" ? 1 : 0,
                    MoveCount = oper.Action == "move" ? 1 : 0,
                    TotalCount = 1,
                }
                )
                .Where(x => libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1)
            .GroupBy(x => x.Operator)
            .Select(g => new
            {
                Operator = g.Key,
                NewCount = g.Sum(x => x.NewCount),
                ChangeCount = g.Sum(x => x.ChangeCount),
                DeleteCount = g.Sum(x => x.DeleteCount),
                CopyCount = g.Sum(x => x.CopyCount),
                MoveCount = g.Sum(x => x.MoveCount),
                TotalCount = g.Sum(x => x.TotalCount),
            })
            .OrderBy(x => x.Operator)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 订购流水
        // parameters:
        //      operation   为 setEntity 或 setOrder 等
        public static void BuildReport_411(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
string operation,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            // 注: libraryCode 要求是一个馆代码，或者 *
            string libraryCode = param_table["libraryCode"] as string;
            if (libraryCode != "*")
                libraryCode = "," + libraryCode + ",";

            macro_table["%library%"] = libraryCode;

            /*
            DateTime start_time = DateTimeUtil.Long8ToDateTime(strStartDate);

            // TODO: 12 点
            DateTime end_time = DateTimeUtil.Long8ToDateTime(strEndDate);
            */

            /*
            var items = from oper in context.ItemOpers
                        join biblio in context.Biblios on oper.BiblioRecPath equals biblio.RecPath
                        into joined
                        from result in joined.DefaultIfEmpty()
                        select new
                        {
                            Operation = oper.Operation + "." + oper.Action,
                            oper.ItemRecPath,
                            BiblioRecPath = oper.BiblioRecPath,
                            Summary = result == null ? "" : result.Summary,
                            OperTime = oper.OperTime,
                            Operator = oper.Operator
                        };
                        */

            // TODO: 订购记录怎么看出是哪个分馆的? 1) 从操作者的权限可以看出 2) 从日志记录的 libraryCode 可以看出 3) 从订购记录的 distribute 元素可以看出
            var items = context.ItemOpers
                .Where(b => b.Operation == operation
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
                .LeftJoin(
                context.Users,
                oper => oper.Operator,
                user => user.ID,
                (oper, user) => new
                {
                    UserLibraryCode = user.LibraryCodeList,
                    oper.BiblioRecPath,
                    oper.ItemRecPath,
                    oper.Operation,
                    oper.Action,
                    oper.OperTime,
                    oper.Operator,
                }
                )
                .Where(x => libraryCode == "*" || x.UserLibraryCode.IndexOf(libraryCode) != -1)
            .LeftJoin(
                context.Biblios,
                oper => oper.BiblioRecPath,
                biblio => biblio.RecPath,
                (oper, biblio) => new
                {
                    Operation = oper.Operation + "." + oper.Action,
                    oper.ItemRecPath,
                    BiblioRecPath = oper.BiblioRecPath,
                    Summary = biblio.Summary,
                    OperTime = oper.OperTime,
                    Operator = oper.Operator
                }
            )
            .OrderBy(x => x.OperTime)
            .ToList();

            /*
            var items = context.ItemOpers
                .SelectMany(a => context.Biblios
.Where(b => b.RecPath == a.BiblioRecPath
&& a.Operation == "setEntity"
&& string.Compare(a.Date, strStartDate) >= 0
&& string.Compare(a.Date, strEndDate) <= 0)
.DefaultIfEmpty()
.Select(b => new {
    Operation = a.Action,
    ItemRecPath = a.ItemRecPath,
    BiblioRecPath = a.BiblioRecPath,
    Summary = b == null ? "" : b.Summary,
    a.OperTime,
    a.Operator
})
                ).ToList();
                */

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // 图书在架情况
        // 注：只有 strEndDate 有效。缺点是只能在 strEndDate 当天统计册记录中的 borrower 才准确
        public static void BuildReport_302(LibraryContext context,
Hashtable param_table,
string strStartDate,
string strEndDate,
ReportWriter writer,
Hashtable macro_table,
string strOutputFileName)
        {
            string location = param_table["location"] as string;
            string classType = param_table["classType"] as string;

            macro_table["%location%"] = location;
            macro_table["%class%"] = classType;

            // TODO: 12 点
            DateTime end_time = DateTimeUtil.Long8ToDateTime(strEndDate);

            var items = context.Items
            .Where(b => b.Location == location
            // && b.CreateTime >= start_time
            && b.CreateTime <= end_time)
            .Join(
                context.Keys,
                item => item.BiblioRecPath,
                key => key.BiblioRecPath,
                (item, key) => new
                {
                    // item.ItemBarcode,
                    item.Location,
                    ClassType = key.Type,
                    BiblioRecPath = item.BiblioRecPath,
                    ItemCount = 1,
                    InnerCount = string.IsNullOrEmpty(item.Borrower) ? 1 : 0,
                    OuterCount = string.IsNullOrEmpty(item.Borrower) ? 0 : 1,
                    Class = string.IsNullOrEmpty(key.Text) ? "" : key.Text.Substring(0, 1),
                }
            )
            .DefaultIfEmpty()
            .Where(x => x.Location == location && x.ClassType == classType)
            .AsEnumerable()
            .GroupBy(x => x.Class)
            .Select(g => new
            {
                Class = g.Key,
                ItemCount = g.Sum(x => x.ItemCount),
                InnerCount = g.Sum(x => x.InnerCount),
                OuterCount = g.Sum(x => x.OuterCount),
            })
            .Select(x => new
            {
                x.Class,
                x.ItemCount,
                x.InnerCount,
                x.OuterCount,
                Percent = String.Format("{0,3:N}%", ((double)x.OuterCount / (double)x.ItemCount) * (double)100)
            })
            .OrderBy(x => x.Class)
            .ToList();

            // https://damieng.com/blog/2014/09/04/optimizing-sum-count-min-max-and-average-with-linq
            // sum line
            var sums = items.GroupBy(g => 1)
                .Select(g => new
                {
                    Class = "",
                    ItemCount = g.Sum(x => x.ItemCount),
                    InnerCount = g.Sum(x => x.InnerCount),
                    OuterCount = g.Sum(x => x.OuterCount),
                })
            .Select(x => new
            {
                x.Class,
                x.ItemCount,
                x.InnerCount,
                x.OuterCount,
                Percent = String.Format("{0,3:N}%", ((double)x.OuterCount / (double)x.ItemCount) * (double)100)
            })
            .ToList();

            Debug.Assert(sums.Count == 1);

            int nRet = writer.OutputRmlReport(
            items,
            sums[0],
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        public static void BuildReport_301(LibraryContext context,
    Hashtable param_table,
    string strStartDate,
    string strEndDate,
    ReportWriter writer,
    Hashtable macro_table,
    string strOutputFileName)
        {
            string location = param_table["location"] as string;
            string classType = param_table["classType"] as string;

            macro_table["%location%"] = location;
            macro_table["%class%"] = classType;

            // TODO: 0 点
            DateTime start_time = DateTimeUtil.Long8ToDateTime(strStartDate);
            // TODO: 12 点
            DateTime end_time = DateTimeUtil.Long8ToDateTime(strEndDate);

            var items = context.Items
            .Where(b => b.Location == location
            // && b.CreateTime >= start_time
            && b.CreateTime <= end_time)
            .Join(
                context.Keys,
                item => item.BiblioRecPath,
                key => key.BiblioRecPath,
                (item, key) => new
                {
                    // item.ItemBarcode,
                    item.Location,
                    ClassType = key.Type,
                    BiblioRecPath = item.BiblioRecPath,
                    ItemCount = 1,
                    NewItemCount = item.CreateTime >= start_time ? 1 : 0,
                    NewBiblioRecPath = item.CreateTime >= start_time ? item.BiblioRecPath : null,
                    Class = string.IsNullOrEmpty(key.Text) ? "" : key.Text.Substring(0, 1),
                }
            )
            .DefaultIfEmpty()
            .Where(x => x.Location == location && x.ClassType == classType)
            .AsEnumerable()
            .GroupBy(x => x.Class)
            .Select(g => new
            {
                Class = g.Key,
                StartItemCount = g.Sum(x => x.ItemCount),
                StartBiblioCount = g.GroupBy(x => x.BiblioRecPath).Count(),
                DeltaItemCount = g.Sum(x => x.NewItemCount),
                DeltaBiblioCount = g.GroupBy(x => x.NewBiblioRecPath)
                .Where(x => x.Key != null).Count(),
            })
            .OrderBy(x => x.Class)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 某段时间内、某馆藏地内按照分类的借阅排行
        // parameters:
        //      param_table 要求 location/dateRange/classType 参数。
        //                  location 表示一个馆藏地，例如 "/阅览室"。注意使用新版的正规形态，其中必须包含一个斜杠
        public static void BuildReport_212(LibraryContext context,
            Hashtable param_table,
            string strStartDate,
            string strEndDate,
            ReportWriter writer,
            Hashtable macro_table,
            string strOutputFileName)
        {
            string location = param_table["location"] as string;
            string classType = param_table["classType"] as string;

            macro_table["%location%"] = location;
            macro_table["%class%"] = classType;

            var items = context.CircuOpers
            .Where(b => // (b.LibraryCode == librarycode) &&
            b.Action == "borrow" || b.Action == "return"
            && string.Compare(b.Date, strStartDate) >= 0
            && string.Compare(b.Date, strEndDate) <= 0)
            .Join(
                context.Items,
                oper => oper.ItemBarcode,
                item => item.ItemBarcode,
                (oper, item) => new
                {
                    // item.ItemBarcode,
                    item.Location,
                    BiblioRecPath = item.BiblioRecPath,
                    BorrowCount = oper.Action == "borrow" ? 1 : 0,
                    ReturnCount = oper.Action == "return" ? 1 : 0,
                    Class = context.Keys
                .Where(x => x.BiblioRecPath == item.BiblioRecPath && x.Type == classType)
                .Select(x => x.Text).FirstOrDefault()
                }
            )
            .DefaultIfEmpty()
            .Where(x => x.Location == location)
            .GroupBy(x => x.Class)
            .Select(g => new
            {
                Class = string.IsNullOrEmpty(g.Key) ? "" : g.Key.Substring(0, 1),
                BorrowCount = g.Sum(x => x.BorrowCount),
                ReturnCount = g.Sum(x => x.ReturnCount)
            })
            .OrderByDescending(x => x.BorrowCount).ThenBy(x => x.Class)
            .ToList();

            int nRet = writer.OutputRmlReport(
            items,
            null,
            macro_table,
            strOutputFileName,
            out string strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


        // 某段时间内没有被借出过的图书。这里有个疑问，就是这一段时间以前借了但在这一段时间内来不及还的算不算借过？
        // parameters:
        //      param_table 要求 location/dataRange 参数。表示一个馆藏地，例如 "/阅览室"。注意使用新版的正规形态，其中必须包含一个斜杠
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
            null,
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
            null,
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
            null,
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
            null,
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

            // TODO: 如何比较日志记录中的 libraryCode ? 应该用 ,code, 来比较?
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
            null,
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
            null,
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
            null,
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
            null,
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
