using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;

using DigitalPlatform;	// Stop类
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Marc;

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和 从dt1000升级 相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        long m_lIdSeed = 0;

        // 将刚从dt1000升级上来的读者和实体记录进行交叉处理
        // parameters:
        //      nStart      从第几个借阅的册事项开始处理
        //      nCount      共处理几个借阅的册事项
        //      nProcessedBorrowItems   [out]本次处理了多少个借阅册事项
        //      nTotalBorrowItems   [out]当前读者一共包含有多少个借阅册事项
        // result.Value
        //      -1  错误。
        //      0   成功。
        //      1   有警告
        public LibraryServerResult CrossRefBorrowInfo(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strReaderBarcode,
            int nStart,
            int nCount,
            out int nProcessedBorrowItems,
            out int nTotalBorrowItems)
        {
            string strError = "";
            nTotalBorrowItems = 0;
            nProcessedBorrowItems = 0;

            int nRet = 0;
            string strWarning = "";
            int nRedoCount = 0;

            // string strCheckError = "";

            LibraryServerResult result = new LibraryServerResult();
            // int nErrorCount = 0;

        REDO_CHANGE_READERREC:

            // 加读者记录锁
#if DEBUG_LOCK_READER
            this.WriteErrorLog("CrossRefBorrowInfo 开始为读者加写锁 '" + strReaderBarcode + "'");
#endif
            this.ReaderLocks.LockForWrite(strReaderBarcode);

            try // 读者记录锁定范围开始
            {
                // 读入读者记录
                string strReaderXml = "";
                string strOutputReaderRecPath = "";
                byte[] reader_timestamp = null;
                nRet = this.GetReaderRecXml(
                    // Channels,
                    channel,
                    strReaderBarcode,
                    out strReaderXml,
                    out strOutputReaderRecPath,
                    out reader_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = "读者证条码号 '" + strReaderBarcode + "' 不存在";
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
                if (nRet == -1)
                {
                    strError = "读入读者记录时发生错误: " + strError;
                    goto ERROR1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    goto ERROR1;
                }

                bool bReaderRecChanged = false;

                // 修改读者记录中overdues/overdue中的价格单位，并加入id
                // return:
                //      -1  error
                //      0   not changed
                //      1   changed
                nRet = ModifyReaderRecord(
                    ref readerdom,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    bReaderRecChanged = true;

                // TODO: strWarning内容如何处理？
                XmlNodeList nodesBorrow = readerdom.DocumentElement.SelectNodes("borrows/borrow");

                nTotalBorrowItems = nodesBorrow.Count;

                if (nTotalBorrowItems == 0)
                {
                    result.Value = 0;
                    result.ErrorInfo = "读者记录中没有借还信息。";
                    return result;
                }

                if (nStart >= nTotalBorrowItems)
                {
                    strError = "nStart参数值" + nStart.ToString() + "大于当前读者记录中的借阅册个数" + nTotalBorrowItems.ToString();
                    goto ERROR1;
                }

                nProcessedBorrowItems = 0;
                for (int i = nStart; i < nTotalBorrowItems; i++)
                {
                    if (nCount != -1 && nProcessedBorrowItems >= nCount)
                        break;

                    // 一个API最多做10条
                    if (nProcessedBorrowItems >= 10)
                        break;

                    XmlNode nodeBorrow = nodesBorrow[i];

                    string strItemBarcode = DomUtil.GetAttr(nodeBorrow, "barcode");

                    nProcessedBorrowItems++;

                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                    {
                        strWarning += "读者记录中<borrow>元素barcode属性值不能为空; ";
                        continue;
                    }

                    string strBorrowDate = DomUtil.GetAttr(nodeBorrow, "borrowDate");
                    string strBorrowPeriod = DomUtil.GetAttr(nodeBorrow, "borrowPeriod");

                    if (String.IsNullOrEmpty(strBorrowDate) == true)
                    {
                        strWarning += "读者记录中<borrow>元素borrowDate属性不能为空; ";
                        continue;
                    }


                    if (String.IsNullOrEmpty(strBorrowPeriod) == true)
                    {
                        strWarning += "读者记录中<borrow>元素borrowPeriod属性不能为空; ";
                        continue;
                    }

                    // 把实体记录借阅信息详细化
                    // return:
                    //      0   册条码号没有找到对应的册记录
                    //      1   成功
                    nRet = ModifyEntityRecord(
                        // Channels,
                        channel,
                        null,   // strEntityRecPath
                        strItemBarcode,
                        strReaderBarcode,
                        strBorrowDate,
                        strBorrowPeriod,
                        out strError);
                    if (nRet == -1)
                    {
                        strWarning += "ModifyEntityRecord() [strItemBarcode='" + strItemBarcode + "' strReaderBarcode='" + strReaderBarcode + "'] error : " + strError + "; ";
                        continue;
                    }

                    // 2008/10/7
                    if (nRet == 0)
                    {
                        strWarning += "册条码号 '" + strItemBarcode + "' 对应的记录不存在; ";
                        continue;
                    }
                }


                if (bReaderRecChanged == true)
                {
                    byte[] output_timestamp = null;
                    string strOutputPath = "";

#if NO
                    RmsChannel channel = Channels.GetChannel(this.WsUrl);
                    if (channel == null)
                    {
                        strError = "get channel error";
                        goto ERROR1;
                    }
#endif

                    // 写回读者记录
                    long lRet = channel.DoSaveTextRes(strOutputReaderRecPath,
                        readerdom.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            nRedoCount++;
                            if (nRedoCount > 10)
                            {
                                strError = "写回读者记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                                goto ERROR1;
                            }
                            goto REDO_CHANGE_READERREC;
                        }
                        goto ERROR1;
                    }

                    // 及时更新时间戳
                    reader_timestamp = output_timestamp;
                }


            }
            finally
            {
                this.ReaderLocks.UnlockForWrite(strReaderBarcode);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("CrossRefBorrowInfo 结束为读者加写锁 '" + strReaderBarcode + "'");
#endif
            }

            if (String.IsNullOrEmpty(strWarning) == false)
            {
                result.Value = 1;
                result.ErrorInfo = strWarning;
            }
            else
                result.Value = 0;

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }


        // 修改读者记录中overdues/overdue中的价格单位，并加入id
        // return:
        //      -1  error
        //      0   not changed
        //      1   changed
        int ModifyReaderRecord(
            ref XmlDocument readerdom,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            // int nRet = 0;
            bool bChanged = false;

            // 列出所有<overdue>节点
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strID = DomUtil.GetAttr(node, "id");

                // 2008/5/21
                if (String.IsNullOrEmpty(strID) == true)
                {
                    // 创建id
                    strID = "upgrade_dt1000_" + this.m_lIdSeed.ToString();
                    this.m_lIdSeed++;
                    DomUtil.SetAttr(node, "id", strID);
                }

                /* 2008/5/21 commented
                if (string.IsNullOrEmpty(strID) == false)
                    continue;   // 新格式，不必作了。
                 * */

                string strPrice = DomUtil.GetAttr(node, "price");

                if (String.IsNullOrEmpty(strPrice) == false
                    && StringUtil.IsPureNumber(strPrice) == true)
                {
                    // 只有纯数字才作
                }
                else
                    continue;

                long lOldPrice = 0;

                try
                {
                    lOldPrice = Convert.ToInt64(strPrice);
                }
                catch
                {
                    strWarning += "价格字符串 '' 格式不正确，应当为纯数字。";
                    continue;
                }

                // 转换为元
                double dPrice = ((double)lOldPrice) / 100;

                strPrice = "CNY" + dPrice.ToString();   // +"yuan";

                DomUtil.SetAttr(node, "price", strPrice);

                // 2008/5/21
                string strType = DomUtil.GetAttr(node, "type");
                if (String.IsNullOrEmpty(strType) == false)
                    DomUtil.SetAttr(node, "reason", strType + "。从dt1000升级过来");

                /*
                // 创建id
                strID = "upgrade_dt1000_" + this.m_lIdSeed.ToString();
                this.m_lIdSeed++;

                DomUtil.SetAttr(node, "id", strID);

                string strReason = "超期违约金(从dt1000升级而来)";

                DomUtil.SetAttr(node, "reason", strReason);
                 * */

                bChanged = true;
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 把实体记录借阅信息详细化
        // parameters:
        //      strEntityRecPath    册记录路径。如果本参数值为空，则表示希望通过strItemBarcode参数来找到册记录
        // return:
        //      -1  error
        //      0   册条码号没有找到对应的册记录
        //      1   成功
        int ModifyEntityRecord(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strEntityRecPath,
            string strItemBarcode,
            string strReaderBarcode,
            string strBorrowDate,
            string strBorrowPeriod,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;
            int nRedoCount = 0;

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "strItemBarcode参数不能为空";
                return -1;
            }

            // RmsChannel channel = null;

            REDO_CHANGE:

            string strOutputItemRecPath = "";
            byte[] item_timestamp = null;

            string strItemXml = "";
            List<string> aPath = null;

#if NO
            if (channel == null)
            {
                channel = Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }
            }
#endif

            if (String.IsNullOrEmpty(strEntityRecPath) == false)
            {
                string strStyle = "content,data,metadata,timestamp,outputpath";
                string strMetaData = "";
                lRet = channel.GetRes(strEntityRecPath,
                    strStyle,
                    out strItemXml,
                    out strMetaData,
                    out item_timestamp,
                    out strOutputItemRecPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        return 0;
                    }
                    strError = "ModifyEntityRecord()通过册记录路径 '"+strEntityRecPath+"' 读入册记录时发生错误: " + strError;
                    return -1;
                }
            }
            else
            {
                // 从册条码号获得册记录
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetItemRecXml(
                    // Channels,
                    channel,
                    strItemBarcode,
                    out strItemXml,
                    100,
                    out aPath,
                    out item_timestamp,
                    out strError);
                if (nRet == 0)
                {
                    // 册条码号不存在也是需要修复的情况之一。
                    return 0;
                }
                if (nRet == -1)
                {
                    strError = "ModifyEntityRecord()读入册记录时发生错误: " + strError;
                    return -1;
                }

                if (aPath.Count > 1)
                {
                    // TODO: 需要将入围的记录全部提取出来，然后看borrower符合读者证条码号的那一条(或者多条?)
                    // 可以参考UpgradeDt1000Loan中的SearchEntityRecord()函数
                    /*
                    strError = "因册条码号 '" + strItemBarcode + "' 检索命中多条册记录: " + StringUtil.MakePathList(aPath) + "，修改册记录的操作ModifyEntityRecord()无法进行";
                    return -1;
                     * */

                    int nSuccessCount = 0;
                    string strTempError = "";
                    // 递归
                    for (int i = 0; i < aPath.Count; i++)
                    {
                        string strTempPath = aPath[i];

                        if (String.IsNullOrEmpty(strTempPath) == true)
                        {
                            Debug.Assert(false, "");
                            continue;
                        }

                        // return:
                        //      -1  error
                        //      0   册条码号没有找到对应的册记录
                        //      1   成功
                        nRet = ModifyEntityRecord(
                            // Channels,
                            channel,
                            strTempPath,
                            strItemBarcode,
                            strReaderBarcode,
                            strBorrowDate,
                            strBorrowPeriod,
                            out strError);
                        if (nRet == -1 && nSuccessCount == 0)
                        {
                            if (String.IsNullOrEmpty(strTempError) == false)
                                strTempError += "; ";
                            strTempError += "试探册记录 '" + strTempPath + "' 时发生错误: " + strError;
                        }

                        if (nRet == 1)
                        {
                            // 改为存储成功信息
                            if (nSuccessCount == 0)
                                strTempError = "";

                            if (String.IsNullOrEmpty(strTempError) == false)
                                strTempError += "; ";
                            strTempError += strTempPath;

                            nSuccessCount++;
                        }
                    }

                    if (nSuccessCount > 0)
                    {
                        strError = "册条码号 '" + strItemBarcode + "' 检索命中" + aPath.Count.ToString() + "条册记录: " + StringUtil.MakePathList(aPath) + "，后面对它们进行了逐条试探，有 " + nSuccessCount.ToString() + " 条记录符合预期的要求，册中借阅信息得到增强。借阅信息得到增强的册记录路径如下: " + strTempError;
                        return 1;
                    }
                    else
                    {
                        strError = "册条码号 '" + strItemBarcode + "' 检索命中" + aPath.Count.ToString() + "条册记录: " + StringUtil.MakePathList(aPath) + "，后面对它们进行了逐条试探，但是没有一条记录符合预期的要求。试探过程报错如下: " + strTempError;
                        return -1;
                    }

                    /*
                    result.Value = -1;
                    result.ErrorInfo = "册条码号为 '" + strItemBarcode + "' 的册记录有 " + aPath.Count.ToString() + " 条，无法进行修复操作。请在附加册记录路径后重新提交修复操作。";
                    result.ErrorCode = ErrorCode.ItemBarcodeDup;

                    aDupPath = new string[aPath.Count];
                    aPath.CopyTo(aDupPath);
                    return result;
                     * */

                }
                else
                {
                    Debug.Assert(nRet == 1, "");
                    Debug.Assert(aPath.Count == 1, "");

                    if (nRet == 1)
                    {
                        strOutputItemRecPath = aPath[0];
                    }
                }
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

            // 校验册条码号参数是否和XML记录中完全一致
            string strTempItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement,
                "barcode");
            if (strItemBarcode != strTempItemBarcode)
            {
                strError = "修改册记录ModifyEntityRecord()操作被拒绝。因册条码号参数 '" + strItemBarcode + "' 和册记录中<barcode>元素内的册条码号值 '" + strTempItemBarcode + "' 不一致。";
                return -1;
            }

            // 看看册记录中是否有指回读者记录的链
            string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                "borrower");
            if (strBorrower != strReaderBarcode)
            {
                // strError = "ModifyEntityRecord()操作被拒绝。您所请求要修复的链，本是一条完整正确的链。可直接进行普通还书操作。";
                strError = "修改册记录ModifyEntityRecord()操作被拒绝。因册记录 " + strOutputItemRecPath + " 中的[borrower]值 '" + strBorrower + "' 和发源(来找册条码号 '" + strItemBarcode + "')的读者证条码号 '" + strReaderBarcode + "' 不一致，不能构成一条完整正确的链。请及时排除此故障。";
                return -1;
            }

            // 2007/1/1注：应当看看记录中<borrower>元素是否有内容才改写<borrowDate>和<borrowPeriod>元素。

            DomUtil.SetElementText(itemdom.DocumentElement, "borrowDate", strBorrowDate);
            DomUtil.SetElementText(itemdom.DocumentElement, "borrowPeriod", strBorrowPeriod);

#if NO
            if (channel == null)
            {
                channel = Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }
            }
#endif
            byte[] output_timestamp = null;
            string strOutputPath = "";

            // 写回册记录
            lRet = channel.DoSaveTextRes(strOutputItemRecPath,
                itemdom.OuterXml,
                false,
                "content",  // ,ignorechecktimestamp
                item_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    nRedoCount++;
                    if (nRedoCount > 10)
                    {
                        strError = "ModifyEntityRecord()写回册记录的时候,遇到时间戳冲突,并因此重试10次,仍失败...";
                        return -1;
                    }
                    goto REDO_CHANGE;
                }
                return -1;
            } // end of 写回册记录失败

            return 1;
        }
    }
}
