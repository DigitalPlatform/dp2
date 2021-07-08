using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Circulation
{
    public static class Utility
    {
        // 获得书目记录的下级记录
        // parameters:
        //      strDataName 数据名称。为 firstAccessNo subrecord 之一
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

            long lRet = channel.GetBiblioInfos(
    stop,
    strRecPath,
    "",
    new string[] { "subrecords:item" },   // "subrecords:item|order"
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

    }
}
