
using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DocumentFormat.OpenXml.EMMA;

namespace dp2Circulation
{
    public static class Utility
    {
        // 获得一个书目记录下属的所有册记录中第一个非空的索取号
        // 注: 不会限制最多册记录条数
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   成功
        public static int GetFirstAccessNo(
    LibraryChannel channel,
    Stop stop,
    string strRecPath,
    out string strResult,
    out string strError)
        {
            strError = "";
            strResult = "";

            SubItemLoader sub_loader = new SubItemLoader();
            sub_loader.BiblioRecPath = strRecPath;
            sub_loader.Channel = channel;
            sub_loader.Stop = stop;
            sub_loader.DbType = "item";

            // sub_loader.Prompt

            foreach (EntityInfo info in sub_loader)
            {
                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    strError = "路径为 '" + info.OldRecPath + "' 的订购记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                    return -1;
                }

                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(info.OldRecord);
                string accessNo = DomUtil.GetElementText(item_dom.DocumentElement,
                    "accessNo");

                if (string.IsNullOrEmpty(accessNo) == false)
                {
                    strResult = accessNo;
                    return 1;
                }
            }

            return 0;
        }

        // 如果返回 ture 表示希望继续进行统计，如果返回 false 表示希望中断统计
        public delegate bool delegate_statis(XmlDocument itemdom);

        // 2024/5/17
        // 对一个书目记录下属的所有册记录进行统计运算
        // 注: 不会限制最多册记录条数
        // return:
        //      -1  出错
        //      其它  经过的记录条数。注意，不是指书目记录下级的全部册记录条数。
        public static int StatisSubItems(
    LibraryChannel channel,
    Stop stop,
    string strRecPath,
    delegate_statis func_statis,
    out string strError)
        {
            strError = "";

            SubItemLoader sub_loader = new SubItemLoader();
            sub_loader.BiblioRecPath = strRecPath;
            sub_loader.Channel = channel;
            sub_loader.Stop = stop;
            sub_loader.DbType = "item";

            // sub_loader.Prompt
            int count = 0;
            foreach (EntityInfo info in sub_loader)
            {
                if (info.ErrorCode != ErrorCodeValue.NoError)
                {
                    strError = "路径为 '" + info.OldRecPath + "' 的订购记录装载中发生错误: " + info.ErrorInfo;  // NewRecPath
                    return -1;
                }

                XmlDocument item_dom = new XmlDocument();
                item_dom.LoadXml(info.OldRecord);

                /*
                string accessNo = DomUtil.GetElementText(item_dom.DocumentElement,
                    "accessNo");

                if (string.IsNullOrEmpty(accessNo) == false)
                {
                    strResult = accessNo;
                    return 1;
                }
                */
                count++;
                if (func_statis?.Invoke(item_dom) == false)
                    return count;
            }

            return count;
        }

        // 获得书目记录下级所有册记录的价格总和
        // parameters:
        //      strRecPath  书目记录路径
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   成功
        public static int GetTotalPrice(LibraryChannel channel,
            Stop stop,
            string strRecPath,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            List<string> prices = new List<string>();
            var ret = StatisSubItems(channel,
                stop,
                strRecPath,
                (item_dom) =>
                {
                    var price = DomUtil.GetElementText(item_dom.DocumentElement,
                        "price");
                    if (string.IsNullOrEmpty(price) == false)
                        prices.Add(price);
                    return true;
                },
                out strError);
            if (ret == -1)
                return -1;
            if (ret == 0)
                return 0;
            strResult = PriceUtil.TotalPrice(prices);
            return 1;
        }

        // 获得书目记录的下级记录
        // parameters:
        //      strDataName 数据名称。为 firstAccessNo subrecords itemCount之一
        //                  itemCount 即下属册记录数。注意和当前 dp2library 账户管辖的馆代码有关，只计算能管辖的册记录数
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   成功
        public static int GetSubRecords(
    LibraryChannel channel,
    Stop stop,
    string strRecPath,
    string strDataName,
    out string strResult,
    out string strError)
        {
            strError = "";
            strResult = "";

            string[] formats = new string[] { "subrecords:item" };
            if (strDataName == "itemCount")
                formats = new string[] { "subcount:item" };

            // 注: 最多获得 10 条册记录
            long lRet = channel.GetBiblioInfos(
    stop,
    strRecPath,
    "",
    formats,   // "subrecords:item|order"
    out string[] results,
    out byte[] baTimestamp,
    out strError);
            if (lRet == -1 || lRet == 0)
                return -1;
            if (results == null || results.Length == 0)
                return 0;
            string strSubRecords = results[0];
            if (strDataName == "firstAccessNo")
                strResult = GetFirstAccessNo(strSubRecords);
            else if (strDataName == "itemCount")
                strResult = strSubRecords;
            else if (strDataName == "subrecords" || string.IsNullOrEmpty(strDataName))
                strResult = strSubRecords;
            return 1;
        }

        static string GetFirstAccessNo(string strSubRecords)
        {
            if (string.IsNullOrEmpty(strSubRecords))
                return "";

            if (strSubRecords.StartsWith("error:"))
                return strSubRecords.Substring("error:".Length);

            XmlDocument collection_dom = new XmlDocument();
            try
            {
                collection_dom.LoadXml(strSubRecords);

                {
                    string errorInfo = "";
                    string itemTotalCount = collection_dom.DocumentElement.GetAttribute("itemTotalCount");
                    if (itemTotalCount == "-1")
                    {
                        string itemErrorCode = collection_dom.DocumentElement.GetAttribute("itemErrorCode");
                        string itemErrorInfo = collection_dom.DocumentElement.GetAttribute("itemErrorInfo");
                        errorInfo = $"{itemErrorCode}:{itemErrorInfo}";
                        return errorInfo;
                    }

                    XmlNodeList nodes = collection_dom.DocumentElement.SelectNodes("item");
                    int i = 0;
                    foreach (XmlElement item in nodes)
                    {
                        string rec_path = item.GetAttribute("recPath");
                        string location = DomUtil.GetElementText(item, "location");
                        string price = DomUtil.GetElementText(item, "price");
                        string seller = DomUtil.GetElementText(item, "seller");
                        string source = DomUtil.GetElementText(item, "source");
                        string accessNo = DomUtil.GetElementText(item, "accessNo");

                        if (string.IsNullOrEmpty(accessNo) == false)
                            return accessNo;
                        i++;
                    }

                    /*
                    Int32.TryParse(itemTotalCount, out int value);
                    if (i < value)
                    {
                        text.AppendLine($"<tr><td colspan='10'>... 有 {value - i} 项被略去 ...</td></tr>");
                    }
                    */
                    return "";  // not found
                }
            }
            catch (Exception ex)
            {
                return "strSubRecords 装入 XMLDOM 时出现异常: "
                    + ex.Message
                    + "。(strSubRecords='" + StringUtil.CutString(strSubRecords, 300) + "')";
            }
        }

#if REMOVED
        static string GetItemCount(string strSubRecords)
        {
            if (string.IsNullOrEmpty(strSubRecords))
                return "0";

            if (strSubRecords.StartsWith("error:"))
                return strSubRecords.Substring("error:".Length);

            XmlDocument collection_dom = new XmlDocument();
            try
            {
                collection_dom.LoadXml(strSubRecords);

                {
                    string errorInfo = "";
                    string itemTotalCount = collection_dom.DocumentElement.GetAttribute("itemTotalCount");
                    if (itemTotalCount == "-1")
                    {
                        string itemErrorCode = collection_dom.DocumentElement.GetAttribute("itemErrorCode");
                        string itemErrorInfo = collection_dom.DocumentElement.GetAttribute("itemErrorInfo");
                        errorInfo = $"{itemErrorCode}:{itemErrorInfo}";
                        return errorInfo;
                    }

                    return itemTotalCount;

                    XmlNodeList nodes = collection_dom.DocumentElement.SelectNodes("item");
                    int i = 0;
                    foreach (XmlElement item in nodes)
                    {
                        string rec_path = item.GetAttribute("recPath");
                        string location = DomUtil.GetElementText(item, "location");
                        string price = DomUtil.GetElementText(item, "price");
                        string seller = DomUtil.GetElementText(item, "seller");
                        string source = DomUtil.GetElementText(item, "source");
                        string accessNo = DomUtil.GetElementText(item, "accessNo");

                        if (string.IsNullOrEmpty(accessNo) == false)
                            return accessNo;
                        i++;
                    }

                    /*
                    Int32.TryParse(itemTotalCount, out int value);
                    if (i < value)
                    {
                        text.AppendLine($"<tr><td colspan='10'>... 有 {value - i} 项被略去 ...</td></tr>");
                    }
                    */
                    return "";  // not found
                }
            }
            catch (Exception ex)
            {
                return "strSubRecords 装入 XMLDOM 时出现异常: "
                    + ex.Message
                    + "。(strSubRecords='" + StringUtil.CutString(strSubRecords, 300) + "')";
            }
        }

#endif
    }
}
