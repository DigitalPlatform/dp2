using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 导出书目记录和部分下属记录的实用类
    /// 一般是因为选定了若干册记录，要求导出 MARC 文件或者 .bdf 文件这样的需求
    /// </summary>
    public static class DumpBiblioAndPartsSubItems
    {

        public delegate bool Delegate_biblioPrepared(BiblioInfo biblio_info);
        public delegate bool Delegate_itemPrepared(BiblioInfo biblio_info, BiblioInfo item_info);
        public delegate bool Delegate_biblioDone(BiblioInfo biblio_info);
        public delegate bool Delegate_idle();
        // parameters:
        //      biblioRecPathList   按照出现先后的顺序存储书目记录路径
        //      groupTable  书目记录路径 --> List<string> (册记录路径列表)
        // return:
        //      -1  出错
        //      0   中断处理
        //      1   正常结束处理
        public static int Dump(
            LibraryChannel channel,
            Stop stop,
            string strDbType,
            List<string> biblioRecPathList,
            Hashtable groupTable,
            Delegate_biblioPrepared biblioPrepared,
            Delegate_itemPrepared itemPrepared,
            Delegate_biblioDone biblioDone,
            Delegate_idle idle,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            foreach (string strBiblioRecPath in biblioRecPathList)
            {
                // Application.DoEvents();
                if (idle != null)
                {
                    if (idle() == false)
                    {
                        strError = "中断处理";
                        return 0;
                    }  
                }

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return 0;
                }

                string[] results = null;
                byte[] baTimestamp = null;

                stop?.SetMessage("正在获取书目记录 " + strBiblioRecPath);

                long lRet = channel.GetBiblioInfos(
                    stop,
                    strBiblioRecPath,
                    "",
                    new string[] { "xml" },   // formats
                    out results,
                    out baTimestamp,
                    out strError);
                if (lRet == 0)
                    return -1;
                if (lRet == -1)
                    return -1;

                if (results == null || results.Length == 0)
                {
                    strError = "GetBiblioInfos() results error";
                    return -1;
                }

                string strXml = results[0];

                BiblioInfo biblio_info = new BiblioInfo();
                biblio_info.RecPath = strBiblioRecPath;
                biblio_info.OldXml = strXml;
                biblio_info.Timestamp = baTimestamp;

                if (biblioPrepared != null)
                {
                    if (biblioPrepared(biblio_info) == false)
                    {
                        strError = "中断处理";
                        return 0;
                    }
                }

                List<string> item_recpaths = (List<string>)groupTable[strBiblioRecPath];
                foreach (string item_recpath in item_recpaths)
                {
                    // 获得一条记录
                    //return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetRecord(
                        channel,
                        stop,
                        strDbType,
    item_recpath,
    out string strItemXml,
    out byte[] baItemTimestamp,
    out strError);
                    if (nRet == -1)
                        return -1;

#if NO
                            XmlDocument item_dom = new XmlDocument();
                            item_dom.LoadXml(strItemXml);
#endif

                    BiblioInfo item_info = new BiblioInfo();
                    item_info.RecPath = item_recpath;
                    item_info.Timestamp = baItemTimestamp;
                    item_info.OldXml = strItemXml;

                    // DomUtil.RemoveEmptyElements(item_dom.DocumentElement);

                    if (itemPrepared != null)
                    {
                        if (itemPrepared(biblio_info, item_info) == false)
                        {
                            strError = "中断处理";
                            return 0;
                        }
                    }
                }

                if (biblioDone != null)
                {
                    if (biblioDone(biblio_info) == false)
                    {
                        strError = "中断处理";
                        return 0;
                    }
                }
            }

            return 1;
        }

        //return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        static int GetRecord(
            LibraryChannel channel,
            Stop stop,
            string strDbType,
            string strRecPath,
            out string strXml,
            out byte[] baTimestamp,
            out string strError)
        {
            strError = "";
            strXml = "";

            baTimestamp = null;
            string strOutputRecPath = "";
            string strBiblio = "";
            string strBiblioRecPath = "";
            // 获得册记录
            long lRet = 0;

            if (strDbType == "item")
            {
                lRet = channel.GetItemInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else if (strDbType == "order")
            {
                lRet = channel.GetOrderInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else if (strDbType == "issue")
            {
                lRet = channel.GetIssueInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else if (strDbType == "comment")
            {
                lRet = channel.GetCommentInfo(
     stop,
     "@path:" + strRecPath,
     "xml",
     out strXml,
     out strOutputRecPath,
     out baTimestamp,
     "",
     out strBiblio,
     out strBiblioRecPath,
     out strError);
            }
            else
            {
                strError = "未知的 strDbType '" + strDbType + "'";
                return -1;
            }

            if (lRet == 0)
                return 0;  // 是否设定为特殊状态?
            if (lRet == -1)
                return -1;

            return 1;
        }

    }
}
