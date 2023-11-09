
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

namespace dp2Circulation
{
    public static class Utility
    {
        // 获得一个书目记录下属的所有册记录中第一个非空的索取号
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
