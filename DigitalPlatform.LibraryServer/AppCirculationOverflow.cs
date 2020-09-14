using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

[assembly: InternalsVisibleTo("TestDp2Library")]
namespace DigitalPlatform.LibraryServer
{
    public partial class LibraryApplication
    {
        class BorrowItemInfo
        {
            public string BookType { get; set; }
            public string BorrowDate { get; set; }
            public string Barcode { get; set; }

            public XmlElement Element { get; set; }

            // 是否已经超额？
            public bool Overflowed { get; set; }

            // 是否已经超期?
            public bool Overdued { get; set; }

            // 打算执行的修改动作
            public string ModifyAction { get; set; }

            // 中途出错的信息。出错的 item 不进行调整处理
            public List<string> Errors { get; set; }

            // 按照图书类型进行分组
            public static List<List<BorrowItemInfo>> GroupByBookType(List<BorrowItemInfo> items)
            {
                List<List<BorrowItemInfo>> results = new List<List<BorrowItemInfo>>();
                var groups = items.GroupBy(x => x.BookType).ToList();
                foreach (var group in groups)
                {
                    List<BorrowItemInfo> result = new List<BorrowItemInfo>(group);
                    results.Add(result);
                }

                return results;
            }

            // 统计数组中，超额事项数量
            public static int CountOverflow(List<BorrowItemInfo> items)
            {
                int count = 0;
                foreach (var item in items)
                {
                    if (item.Overflowed == true && string.IsNullOrEmpty(item.ModifyAction))
                        count++;
                }

                return count;
            }

            // 统计数组中，正常借阅(非超额)的数量
            public static int CountNormal(List<BorrowItemInfo> items)
            {
                int count = 0;
                foreach (var item in items)
                {
                    if (item.Overflowed == false || item.ModifyAction == "removeOverflow")
                        count++;
                }

                return count;
            }

            // 改掉一个 overflow
            // return:
            //      != null 成功改掉一个
            //      == null   没能改掉。(原因可能是因为没有可以改动状态的 overflow 元素了)
            public static BorrowItemInfo DecOverflow(List<BorrowItemInfo> items)
            {
                foreach (var item in items)
                {
                    // 已经超期的不让改动
                    if (item.Overdued)
                        continue;

                    // 有错误的事项不进行改动
                    if (item.Errors != null && item.Errors.Count > 0)
                        continue;

                    if (item.Overflowed == true && string.IsNullOrEmpty(item.ModifyAction))
                    {
                        item.ModifyAction = "removeOverflow";
                        return item;
                    }
                }

                return null;
            }
        }

        // parameters:
        //      now 当前时间。本地时间格式。这是用来判断是否超期的依据
        static bool IsOverdue(XmlElement borrow,
            DateTime now,
            out List<string> warnings)
        {
            warnings = new List<string>();
            StringBuilder debugInfo = null;
            // 看看是否已经超期。已经超期的不处理
            string returningDate = borrow.GetAttribute("returningDate");

            // 2020/9/14
            // 较旧的 dp2library 版本中没有 returningDate 属性，这种情况意味着所借图书的借阅时间很早很早了，可以简单当作已经超期来处理
            // 注：也可以考虑这里尝试用 borrowDate 加上 borrowPeriod 来计算出应还时间，只是比较麻烦一点
            if (string.IsNullOrEmpty(returningDate) == true)
                return true;

            debugInfo?.AppendLine($"returningDate='{returningDate}'");

            DateTime returningTime = DateTimeUtil.FromRfc1123DateTimeString(returningDate).ToLocalTime();

            string period = borrow.GetAttribute("borrowPeriod");

            debugInfo?.AppendLine($"borrowPeriod='{period}'");

            int nRet = LibraryApplication.ParsePeriodUnit(period,
out long lPeriodValue,
out string strPeriodUnit,
out string strError);
            if (nRet == -1)
            {
                debugInfo?.AppendLine($"ParsePeriodUnit('{period}') 出错：{strError}。只好把时间单位当作 day 来处理");
                strPeriodUnit = "day";
                // continue;
            }

            // DateTime now = DateTime.Now;
            // 正规化时间
            nRet = DateTimeUtil.RoundTime(strPeriodUnit,
                ref now,
                out strError);
            if (nRet == -1)
            {
                warnings.Add($"正规化时间出错(1)。strPeriodUnit={strPeriodUnit}");
                debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                return false;
            }

            nRet = DateTimeUtil.RoundTime(strPeriodUnit,
                ref returningTime,
                out strError);
            if (nRet == -1)
            {
                warnings.Add($"正规化时间出错(2)。strPeriodUnit={strPeriodUnit}");
                debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                return false;
            }

            if (returningTime < now)
            {
                debugInfo?.AppendLine($"已经超期，跳过处理 (returningTime={returningTime.ToString()}, now={now.ToString()})");
                return true;
            }

            return false;
        }

        // 构造 ItemInfo List
        // parameters:
        //      now 当前时间。本地时间格式。这是用来判断是否超期的依据
        static int BuildBorrowItemInfo(XmlDocument readerdom,
            DateTime now,
            out List<BorrowItemInfo> results,
            out string strError)
        {
            results = new List<BorrowItemInfo>();
            strError = "";

            StringBuilder debugInfo = null;

            var nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            foreach (XmlElement borrow in nodes)
            {
                BorrowItemInfo info = new BorrowItemInfo();
                info.Element = borrow;
                info.BookType = borrow.GetAttribute("type");
                info.Overflowed = borrow.HasAttribute("overflow");
                info.Overdued = IsOverdue(borrow, now, out List<string> warnings);
                if (warnings != null && warnings.Count > 0)
                    info.Errors = warnings;
                info.BorrowDate = borrow.GetAttribute("borrowDate");

                string itemBarcode = borrow.GetAttribute("barcode");
                if (string.IsNullOrEmpty(itemBarcode))
                    itemBarcode = "@refid:" + borrow.GetAttribute("refID");

                info.Barcode = itemBarcode;
                results.Add(info);
            }

            return 0;
        }

        // 获得一册书的借阅参数
        int GetBorrowParam(
            XmlDocument readerdom,
            string bookType,
            string borrowDate,
            out string borrowPeriod,
            out string denyPeriod,
            out string returningDate,
            out string strError)
        {
            strError = "";
            borrowPeriod = "";
            returningDate = "";
            denyPeriod = "";

            string libraryCode = DomUtil.GetElementText(readerdom.DocumentElement, "libraryCode");
            string readerType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");

            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            int nRet = this.GetLoanParam(
            libraryCode,
            readerType,
            bookType,
            "借期",
            out string strBorrowPeriodList,
            out MatchResult matchresult,
            out strError);
            if (nRet == -1)
            {
                strError = $"获得 馆代码 '{ libraryCode }' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 借期 参数时发生错误: {strError}";
                return -1;
            }
            if (nRet < 4)  // nRet == 0
            {
                strError = $"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 借期 参数无法获得: {strError}";
                return -1;
            }

            string[] aPeriod = strBorrowPeriodList.Split(new char[] { ',' });
            if (aPeriod.Length == 0)
            {
                strError = $"'{strBorrowPeriodList}' Split error";
                return -1;
            }

            borrowPeriod = aPeriod[0];
            if (string.IsNullOrEmpty(borrowPeriod))
            {
                strError = $"期限字符串 '{strBorrowPeriodList}' 中第一部分 '{borrowPeriod}' 为空";
                return -1;
            }

            nRet = ParseBorrowPeriod(borrowPeriod,
out string strThisBorrowPeriod,
out denyPeriod,
out strError);
            if (nRet == -1)
            {
                strError = $"ParseBorrowPeroid() '{borrowPeriod}' error";
                return -1;
            }

            DateTime borrowTime = DateTimeUtil.FromRfc1123DateTimeString(borrowDate).ToLocalTime();

            // 计算应还书时间
            nRet = ComputeReturningDay(
borrowTime,
strThisBorrowPeriod,
out DateTime this_return_time,
out strError);
            if (nRet == -1)
            {
                strError = $"ComputeReturningDay() error. borrowTime='{borrowTime}', strThisBorrowPeriod='{strThisBorrowPeriod}'";
                return -1;
            }

            returningDate = DateTimeUtil.Rfc1123DateTimeStringEx(this_return_time.ToLocalTime());
            return 0;
        }

        // 获得特定图书类型的最大可借数
        // return:
        //      -1  出错
        //      其他  此类型图书的最大可借册数
        int GetTypeMax(
            XmlDocument readerdom,
            string bookType,
            out string strError)
        {
            strError = "";

            string libraryCode = DomUtil.GetElementText(readerdom.DocumentElement, "libraryCode");
            string readerType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");

            // 从读者信息中，找出该读者以前已经借阅过的同类图书的册数
            // int nThisTypeCount = readerdom.DocumentElement.SelectNodes("borrows/borrow[@type='" + bookType + "']").Count;

            int nRet = this.GetLoanParam(
//null,
libraryCode,
readerType,
bookType,
"可借册数",
out string strParamValue,
out MatchResult _,
out strError);
            if (nRet == -1)
            {
                strError = $"获得 馆代码 '{ libraryCode }' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 可借册数 参数时发生错误: {strError}";
                return -1;
            }
            if (nRet < 4)
            {
                strError = $"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 可借册数 参数无法获得: {strError}";
                return -1;
            }

            if (Int32.TryParse(strParamValue, out int thisTypeMax) == false)
            {
                strError = $"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 可借册数 参数 '{strParamValue}' 格式错误";
                return -1;
            }

            return thisTypeMax;
        }

        // 获得总的最大可借数
        // return:
        //      -1  出错
        //      其他  最大可借册数
        int GetTotalMax(
            XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            string libraryCode = DomUtil.GetElementText(readerdom.DocumentElement, "libraryCode");
            string readerType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");

            // 看 可借总册数
            int nRet = this.GetLoanParam(
//null,
libraryCode,
readerType,
"",
"可借总册数",
out string strParamValue,
out MatchResult _,
out strError);
            if (nRet == -1)
            {
                strError = $"获得 馆代码 '{ libraryCode }' 中 读者类型 '{ readerType }' 的 可借总册数 参数时发生错误: {strError}";
                return -1;
            }
            if (nRet < 3)
            {
                strError = $"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 的 可借总册数 参数无法获得: {strError}";
                return -1;
            }
            if (Int32.TryParse(strParamValue, out int max) == false)
            {
                strError = $"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 的 可借总册数 参数 '{strParamValue}' 格式错误";
                return -1;
            }

            return max;
        }

        public int AdjustOverflow(
SessionInfo sessioninfo,
XmlDocument readerdom,
StringBuilder debugInfo,
out string strError)
        {
            var result = AdjustOverflow(
readerdom,
this.Clock.Now,
debugInfo);
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                return -1;
            }

            // 修改涉及到的册记录
            if (result.Modifies.Count > 0 && sessioninfo != null)
            {
                foreach (var info in result.Modifies)
                {
                    int nRet = ModifyItemRecord(
        sessioninfo,
        info,
        out strError);
                    if (nRet == -1)
                        return -1;

                    // 写入操作日志
                    nRet = WriteAdjustOverflowOperLog(
                        sessioninfo,
                        info,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            strError = result.ErrorInfo;
            return result.Value;
        }

        // 写入操作日志
        int WriteAdjustOverflowOperLog(
            SessionInfo sessioninfo,
            ItemModifyInfo info,
            out string strError)
        {
            strError = "";

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "libraryCode",
                info.LibraryCode);    // 读者所在的馆代码
            DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "adjustOverflow");
            // DomUtil.SetElementText(domOperLog.DocumentElement, "action", strAction);

            DomUtil.SetElementText(domOperLog.DocumentElement, "borrowID", info.BorrowID);
            DomUtil.SetElementText(domOperLog.DocumentElement, "patronBarcode", info.PatronBarcode);
            DomUtil.SetElementText(domOperLog.DocumentElement, "itemBarcode", info.ItemBarcode);
            DomUtil.SetElementText(domOperLog.DocumentElement, "confirmItemRecPath", info.ConfirmItemRecPath);
            DomUtil.SetElementText(domOperLog.DocumentElement, "borrowDate", info.BorrowDate);

            DomUtil.SetElementText(domOperLog.DocumentElement, "borrowPeriod", info.BorrowPeriod);
            DomUtil.SetElementText(domOperLog.DocumentElement, "returningDate", info.ReturningDate);
            if (string.IsNullOrEmpty(info.DenyPeriod) == false)
                DomUtil.SetElementText(domOperLog.DocumentElement, "denyPeriod", info.DenyPeriod);

            // 修改前的 borrow 元素
            var borrow = domOperLog.DocumentElement.AppendChild(domOperLog.CreateElement("borrow")) as XmlElement;
            DomUtil.SetElementOuterXml(borrow, info.OldBorrowInfo);

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
    sessioninfo.UserID);

            string strOperTime = this.Clock.GetClock();
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTime);

            int nRet = this.OperLog.WriteOperLog(domOperLog,
    sessioninfo.ClientAddress,
    out strError);
            if (nRet == -1)
            {
                strError = "写入 AdjustOverflow 操作日志时发生错误: " + strError;
                return -1;
            }

            return 0;
        }

        internal class AdjustOverflowResult : NormalResult
        {
            public List<ItemModifyInfo> Modifies { get; set; }
        }

        // 针对读者记录中的 borrow 元素中 overflow (尚未超期)的，重新计算是否超额。如果不超额的，修改为正常的借期
        // parameters:
        //      now 当前时间。本地时间格式。这是用来判断是否超期的依据
        // return.Value:
        //      -1  出错
        //      0   成功
        //      1   有警告信息，在 strError 中返回
        internal AdjustOverflowResult AdjustOverflow(
        XmlDocument readerdom,
        DateTime now,
        StringBuilder debugInfo)
        {
            List<ItemModifyInfo> modifies = new List<ItemModifyInfo>();

            int nRet = BuildBorrowItemInfo(readerdom,
                now,
                out List<BorrowItemInfo> items,
                out string strError);
            if (nRet == -1)
                return new AdjustOverflowResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    Modifies = modifies
                };

            // 没有任何在借事项
            if (items.Count == 0)
                return new AdjustOverflowResult
                {
                    Value = 0,
                    Modifies = modifies
                };

            int overflow_count = BorrowItemInfo.CountOverflow(items);
            // 没有超额事项可供调整
            if (overflow_count == 0)
                return new AdjustOverflowResult
                {
                    Value = 0,
                    Modifies = modifies
                };

            // 获得总的最大可借数
            // return:
            //      -1  出错
            //      其他  最大可借册数
            int totalMax = GetTotalMax(
    readerdom,
    out strError);
            if (totalMax == -1)
                return new AdjustOverflowResult
                {
                    Value = -1,
                    ErrorInfo = strError,
                    Modifies = modifies
                };

            List<string> warnings = new List<string>();

            var groups = BorrowItemInfo.GroupByBookType(items);

            // 对每种图书类型，尝试修正到本类型的最大许可外借数
            foreach (var group in groups)
            {
                string bookType = group[0].BookType;

                // 获得特定图书类型的最大可借数
                // return:
                //      -1  出错
                //      其他  此类型图书的最大可借册数
                int typeMax = GetTypeMax(
                    readerdom,
                    bookType,
                    out strError);
                if (typeMax == -1)
                {
                    warnings.Add(strError);
                    continue;   // 放弃这一类型的处理，继续处理其他类型
                }

                while (true)
                {
                    // 统计数组中，正常借阅(非超额)的数量
                    int count = BorrowItemInfo.CountNormal(group);
                    if (count < typeMax)
                    {
                        // 尝试从中改掉一个 overflow
                        // return:
                        //      true 成功改掉一个
                        //      false   没能改掉。(原因可能是因为没有可以改动状态的 overflow 元素了)
                        var item = BorrowItemInfo.DecOverflow(group);
                        if (item == null)
                            break;

                        // 检查总册数是否超额。如果超额，刚才的改动需要 undo
                        int totalCount = BorrowItemInfo.CountNormal(items);
                        if (totalCount > totalMax)
                        {
                            item.ModifyAction = null;   // Undo
                            break;
                        }
                    }
                    else
                        break;
                }
            }

            // 开始修改 borrow 元素
            foreach (var item in items)
            {
                if (item.ModifyAction != "removeOverflow")
                    continue;

                string strOldBorrowInfo = item.Element.OuterXml;

                // 获得一册书的借阅参数
                nRet = GetBorrowParam(
                    readerdom,
                    item.BookType,
                    item.BorrowDate,
                    out string borrowPeriod,
                    out string denyPeriod,
                    out string returningDate,
                    out strError);
                if (nRet == -1)
                {
                    warnings.Add(strError);
                    continue;
                }

                item.Element.SetAttribute("borrowPeriod",
    borrowPeriod);

                if (string.IsNullOrEmpty(denyPeriod) == false)
                    item.Element.SetAttribute("denyPeriod",
                       denyPeriod);
                else
                    item.Element.RemoveAttribute("denyPeriod");

                item.Element.SetAttribute("returningDate",
                    returningDate);

                // 删除 overflow 属性
                item.Element.RemoveAttribute("overflow");

                modifies.Add(new ItemModifyInfo
                {
                    LibraryCode = DomUtil.GetElementText(readerdom.DocumentElement, "libraryCode"),
                    PatronBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode"),
                    BorrowID = item.Element.GetAttribute("borrowID"),
                    ItemBarcode = item.Barcode,
                    BorrowDate = item.Element.GetAttribute("borrowDate"),
                    BorrowPeriod = borrowPeriod,
                    DenyPeriod = denyPeriod,
                    ReturningDate = returningDate,
                    OldBorrowInfo = strOldBorrowInfo,
                });
            }

            /*
            // 修改涉及到的册记录
            if (modifies.Count > 0 && sessioninfo != null)
            {
                foreach (var info in modifies)
                {
                    nRet = ModifyItemRecord(
        sessioninfo,
        info,
        out strError);
                    if (nRet == -1)
                    {
                        warnings.Add($"修改册记录 {info.ItemBarcode} 过程中出错: {strError}");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                    }
                }
            }
            */

            if (warnings.Count > 0)
            {
                strError = StringUtil.MakePathList(warnings, "; ");
                return new AdjustOverflowResult
                {
                    Value = 1,
                    ErrorInfo = strError,
                    Modifies = modifies
                };
            }

            return new AdjustOverflowResult
            {
                Value = 0,
                Modifies = modifies
            };
        }

#if NO
        // 针对读者记录中的 borrow 元素中 overflow (尚未超期)的，重新计算是否超额。如果不超额的，修改为正常的借期
        // return:
        //      -1  出错
        //      0   成功
        //      1   有警告信息，在 strError 中返回
        int AdjustOverflow(
            SessionInfo sessioninfo,
            XmlDocument readerdom,
            StringBuilder debugInfo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            debugInfo.AppendLine($"用于调整的读者记录: {DomUtil.GetIndentXml(readerdom)}");

            List<string> warnings = new List<string>();
            List<ItemModifyInfo> items = new List<ItemModifyInfo>();

            string libraryCode = DomUtil.GetElementText(readerdom.DocumentElement, "libraryCode");
            string readerType = DomUtil.GetElementText(readerdom.DocumentElement, "readerType");
            // List<XmlElement> overflows = new List<XmlElement>();

            debugInfo?.AppendLine($"libraryCode='{libraryCode}'");
            debugInfo?.AppendLine($"readerType='{readerType}'");

            var nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            foreach (XmlElement borrow in nodes)
            {
                debugInfo?.AppendLine($"=== 对 borrow 元素进行处理: {borrow.OuterXml}");
                if (borrow.HasAttribute("overflow") == false)
                {
                    debugInfo?.AppendLine("没有 overflow 属性，跳过处理");
                    continue;
                }

                string no = borrow.GetAttribute("no");
                if (string.IsNullOrEmpty(no) == false)
                {
                    if (Int32.TryParse(no, out int value) == true)
                    {
                        if (value > 0)
                        {
                            debugInfo?.AppendLine("续借的情况跳过处理");
                            continue;   // 续借的情况不考虑
                        }
                    }
                    else
                    {
                        warnings.Add($"续借次数 '{no}' 格式错误");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }
                }

                string itemBarcode = borrow.GetAttribute("barcode");
                if (string.IsNullOrEmpty(itemBarcode))
                    itemBarcode = "@refid:" + borrow.GetAttribute("refID");

                debugInfo?.AppendLine($"条码号='{itemBarcode}'");

                try
                {
                    // 获得借阅开始时间
                    string borrowDate = borrow.GetAttribute("borrowDate");

                    debugInfo?.AppendLine($"borrowDate='{borrowDate}'");

                    DateTime borrowTime = DateTimeUtil.FromRfc1123DateTimeString(borrowDate).ToLocalTime();

                    // 看看是否已经超期。已经超期的不处理
                    {
                        string returningDate = borrow.GetAttribute("returningDate");

                        debugInfo?.AppendLine($"returningDate='{returningDate}'");

                        DateTime returningTime = DateTimeUtil.FromRfc1123DateTimeString(returningDate).ToLocalTime();

                        string period = borrow.GetAttribute("borrowPeriod");

                        debugInfo?.AppendLine($"borrowPeriod='{period}'");

                        nRet = LibraryApplication.ParsePeriodUnit(period,
        out long lPeriodValue,
        out string strPeriodUnit,
        out strError);
                        if (nRet == -1)
                        {
                            debugInfo?.AppendLine($"ParsePeriodUnit('{period}') 出错：{strError}。只好把时间单位当作 day 来处理");
                            strPeriodUnit = "day";
                            // continue;
                        }

                        DateTime now = DateTime.Now;
                        // 正规化时间
                        nRet = DateTimeUtil.RoundTime(strPeriodUnit,
                            ref now,
                            out strError);
                        if (nRet == -1)
                        {
                            warnings.Add($"正规化时间出错(1)。strPeriodUnit={strPeriodUnit}");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }

                        nRet = DateTimeUtil.RoundTime(strPeriodUnit,
                            ref returningTime,
                            out strError);
                        if (nRet == -1)
                        {
                            warnings.Add($"正规化时间出错(2)。strPeriodUnit={strPeriodUnit}");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }

                        if (returningTime < now)
                        {
                            debugInfo?.AppendLine($"已经超期，跳过处理 (returningTime={returningTime.ToString()}, now={now.ToString()})");
                            continue;
                        }
                    }

                    string bookType = borrow.GetAttribute("type");

                    debugInfo?.AppendLine($"bookType='{bookType}'");

                    // 假设要首次借阅这一册，是否会超额？
                    {
                        // 从读者信息中，找出该读者以前已经借阅过的同类图书的册数
                        int nThisTypeCount = readerdom.DocumentElement.SelectNodes("borrows/borrow[@type='" + bookType + "']").Count;

                        nRet = this.GetLoanParam(
    //null,
    libraryCode,
    readerType,
    bookType,
    "可借册数",
    out string strParamValue,
    out MatchResult _,
    out strError);
                        if (nRet == -1)
                        {
                            warnings.Add($"获得 馆代码 '{ libraryCode }' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 可借册数 参数时发生错误: {strError}");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }
                        if (nRet < 4)
                        {
                            warnings.Add($"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 可借册数 参数无法获得: {strError}");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }

                        if (Int32.TryParse(strParamValue, out int thisTypeMax) == false)
                        {
                            warnings.Add($"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 可借册数 参数 '{strParamValue}' 格式错误");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }

                        // 依然超额了。不修改
                        if (nThisTypeCount > thisTypeMax)
                        {
                            debugInfo?.AppendLine($"特定类型的图书超额了，跳过处理。nThisTypeCount={nThisTypeCount}, thisTypeMax={thisTypeMax}, bookType={bookType}, readerType={readerType}");
                            continue;
                        }

                        // 看 可借总册数
                        nRet = this.GetLoanParam(
//null,
libraryCode,
readerType,
"",
"可借总册数",
out strParamValue,
out MatchResult _,
out strError);
                        if (nRet == -1)
                        {
                            warnings.Add($"获得 馆代码 '{ libraryCode }' 中 读者类型 '{ readerType }' 的 可借总册数 参数时发生错误: {strError}");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }
                        if (nRet < 3)
                        {
                            warnings.Add($"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 的 可借总册数 参数无法获得: {strError}");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }
                        if (Int32.TryParse(strParamValue, out int max) == false)
                        {
                            warnings.Add($"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 的 可借总册数 参数 '{strParamValue}' 格式错误");
                            debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                            continue;
                        }

                        // 从读者信息中，找出该读者已经借阅过的册数
                        int count = readerdom.DocumentElement.SelectNodes("borrows/borrow").Count;
                        // 依然超额了。不修改
                        if (count > max)
                        {
                            debugInfo?.AppendLine($"全部图书超额了，跳过处理。count={count}, max={max}, readerType={readerType}");
                            continue;
                        }
                    }


                    // return:
                    //      reader和book类型均匹配 算4分
                    //      只有reader类型匹配，算3分
                    //      只有book类型匹配，算2分
                    //      reader和book类型都不匹配，算1分
                    nRet = this.GetLoanParam(
                    libraryCode,
                    readerType,
                    bookType,
                    "借期",
                    out string strBorrowPeriodList,
                    out MatchResult matchresult,
                    out strError);
                    if (nRet == -1)
                    {
                        warnings.Add($"获得 馆代码 '{ libraryCode }' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 借期 参数时发生错误: {strError}");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }
                    if (nRet < 4)  // nRet == 0
                    {
                        warnings.Add($"馆代码 '{ libraryCode}' 中 读者类型 '{ readerType }' 针对图书类型 '{ bookType }' 的 借期 参数无法获得: {strError}");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }

                    string[] aPeriod = strBorrowPeriodList.Split(new char[] { ',' });
                    if (aPeriod.Length == 0)
                    {
                        warnings.Add($"'{strBorrowPeriodList}' Split error");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }

                    string borrowPeriod = aPeriod[0];
                    if (string.IsNullOrEmpty(borrowPeriod))
                    {
                        warnings.Add($"期限字符串 '{strBorrowPeriodList}' 中第一部分 '{borrowPeriod}' 为空");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }

                    nRet = ParseBorrowPeriod(borrowPeriod,
        out string strThisBorrowPeriod,
        out string strThisDenyPeriod,
        out strError);
                    if (nRet == -1)
                    {
                        warnings.Add($"ParseBorrowPeroid() '{borrowPeriod}' error");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }


                    // 计算应还书时间
                    nRet = ComputeReturningDay(
        borrowTime,
        strThisBorrowPeriod,
        out DateTime this_return_time,
        out strError);
                    if (nRet == -1)
                    {
                        warnings.Add($"ComputeReturningDay() error. borrowTime='{borrowTime}', strThisBorrowPeriod='{strThisBorrowPeriod}'");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                        continue;
                    }

                    borrow.SetAttribute("borrowPeriod",
        strThisBorrowPeriod);
                    // 2016/6/7
                    if (string.IsNullOrEmpty(strThisDenyPeriod) == false)
                        borrow.SetAttribute("denyPeriod",
                           strThisDenyPeriod);
                    else
                        borrow.RemoveAttribute("denyPeriod");

                    string strReturningDate = DateTimeUtil.Rfc1123DateTimeStringEx(this_return_time.ToLocalTime());
                    borrow.SetAttribute("returningDate",
                        strReturningDate);

                    // 删除 overflow 属性
                    borrow.RemoveAttribute("overflow");

                    items.Add(new ItemModifyInfo
                    {
                        ItemBarcode = itemBarcode,
                        BorrowPeriod = strThisBorrowPeriod,
                        DenyPeriod = strThisDenyPeriod,
                        ReturningDate = strReturningDate
                    });
                }
                catch (Exception ex)
                {
                    warnings.Add($"册记录 {itemBarcode} 处理过程出现异常: {ex.Message}");
                    debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                }
            }

            // 修改涉及到的册记录
            if (items.Count > 0)
            {
                foreach (var info in items)
                {
                    nRet = ModifyItemRecord(
        sessioninfo,
        info,
        out strError);
                    if (nRet == -1)
                    {
                        warnings.Add($"修改册记录 {info.ItemBarcode} 过程中出错: {strError}");
                        debugInfo?.Append($"{warnings[warnings.Count - 1]}");
                    }
                }
            }


            if (warnings.Count > 0)
            {
                strError = StringUtil.MakePathList(warnings, "; ");
                return 1;
            }

            return 0;
        }

#endif

        // 修改册记录的信息
        public class ItemModifyInfo
        {
            public string ItemBarcode { get; set; }
            public string ConfirmItemRecPath { get; set; }
            public string BorrowPeriod { get; set; }
            public string DenyPeriod { get; set; }
            public string ReturningDate { get; set; }

            public string OldBorrowInfo { get; set; }
            public string BorrowDate { get; set; }

            public string LibraryCode { get; set; }
            public string PatronBarcode { get; set; }
            public string BorrowID { get; set; }
        }

        // 修改册记录中的借期，并去掉 overflow 元素
        int ModifyItemRecord(
            SessionInfo sessioninfo,
            ItemModifyInfo info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strFrom = "册条码号";

            // 获得册记录
            var result = GetItemRecord(sessioninfo,
info.ItemBarcode,
null,   // strOwnerInstitution,
ref strFrom,
info.ConfirmItemRecPath,
// ref strLibraryCode,
out List<string> aPath,
out string strItemXml,
out string strOutputItemRecPath,
out byte[] item_timestamp);
            if (aPath.Count > 1)
            {
                strError = $"册条码号为 {info.ItemBarcode} 的册记录有 {aPath.Count} 条，无法进行借阅操作";
                return -1;
            }
            if (result.Value == -1)
            {
                strError = result.ErrorInfo;
                return -1;
            }

            XmlDocument itemdom = null;
            nRet = LibraryApplication.LoadToDom(strItemXml,
                out itemdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载册记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            // 修改
            DomUtil.SetElementText(itemdom.DocumentElement, "borrowPeriod", info.BorrowPeriod);
            if (string.IsNullOrEmpty(info.DenyPeriod) == false)
                DomUtil.SetElementText(itemdom.DocumentElement, "denyPeriod", info.DenyPeriod);
            else
                DomUtil.DeleteElement(itemdom.DocumentElement, "denyPeriod");
            DomUtil.SetElementText(itemdom.DocumentElement, "returningDate", info.ReturningDate);
            DomUtil.DeleteElement(itemdom.DocumentElement, "overflow");

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            // 写回册记录
            long lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                itemdom.OuterXml,
                false,
                "content",
                item_timestamp,
                out byte[] output_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
                return -1;

            return 0;
        }


    }
}
