using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;
using DigitalPlatform.Range;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和 通讯通道管理 相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 列出指定的通道信息
        // parameters:
        //      strUserName 用户名。如果为空，表示列出全部用户名
        // return:
        //      -1  出错
        //      其他    用户总数（不是本批的个数）
        public int ListChannels(
            string strQuery,
            string strStyle,
            int nStart,
            int nCount,
            out ChannelInfo[] infos,
            out string strError)
        {
            strError = "";
            infos = null;

            Hashtable table = StringUtil.ParseParameters(strQuery);
            List<ChannelInfo> list = null;
            int nRet = this.SessionTable.ListChannels(
                (string)table["ip"],
                (string)table["username"],
                strStyle,
                out list,
                out strError);
            if (nRet == -1)
                return -1;

            // 计算出本次要返回的数量
            if (nCount == -1)
                nCount = Math.Max(0, list.Count - nStart);
            nCount = Math.Min(100, nCount); // 限制每批最多100个

            // 复制出本次需要的局部
            List<ChannelInfo> parts = new List<ChannelInfo>();

            for (int i = nStart; i < Math.Min(nStart + nCount, list.Count); i++)   // 
            {
                parts.Add(list[i]);
            }

            infos = new ChannelInfo[parts.Count];
            parts.CopyTo(infos);

            return list.Count;  // 返回总量
        }

        // 管理通道
        public int ManageChannel(
            string strAction,
            string strStyle,
            ChannelInfo[] requests,
            out ChannelInfo[] results,
            out string strError)
        {
            strError = "";
            results = null;

            if (strAction == "close")
            {
                int nCount = 0;
                foreach(ChannelInfo info in requests)
                {
                    if (string.IsNullOrEmpty(info.SessionID) == false)
                    {
                        bool bRet = this.SessionTable.CloseSessionBySessionID(info.SessionID);
                        if (bRet == true)
                            nCount++;
                    }

                    if (string.IsNullOrEmpty(info.ClientIP) == false)
                    {
                        nCount += this.SessionTable.CloseSessionByClientIP(info.ClientIP);
                    }
                }

                return nCount;
            }


            return 0;
        }
    }
}
