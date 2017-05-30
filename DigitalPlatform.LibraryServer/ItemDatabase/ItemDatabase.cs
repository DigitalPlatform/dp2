using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

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
    /// 册、期、订购、评注库的公用基础类。这类库的特点，就是都需要一套增删改的API
    /// </summary>
    public class ItemDatabase
    {
        public LibraryApplication App = null;

        public ItemDatabase()
        {
        }

        // 构造函数
        public ItemDatabase(LibraryApplication app)
        {
            this.App = app;
        }

        // (派生类必须重载)
        // 用于显示的事项名称。例如“册” “期” “采购”
        public virtual string ItemName
        {
            get
            {
                throw new Exception("ItemName 尚未实现");
            }
        }

        // 事项内部名称。例如 “Item” “Issue” “Order”
        public virtual string ItemNameInternal
        {
            get
            {
                throw new Exception("ItemNameInternal 尚未实现");
            }
        }

        // (派生类必须重载)
        // 检索时的缺省结果集名称。例如“entities” “issues” “orders”
        public virtual string DefaultResultsetName
        {
            get
            {
                throw new Exception("DefaultResultsetName 尚未实现");
            }
        }

        // (派生类必须重载)
        // 准备写入日志的SetXXX操作字符串。例如“SetEntity” “SetIssue”
        public virtual string OperLogSetName
        {
            get
            {
                throw new Exception("OperLogSetName 未实现");
            }
        }

        public virtual string SetApiName
        {
            get
            {
                throw new Exception("SetApiName 未实现");
            }
        }

        public virtual string GetApiName
        {
            get
            {
                throw new Exception("GetApiName 未实现");
            }
        }

        // (派生类必须重载)
        // 获得事项数据库名
        // return:
        //      -1  error
        //      0   没有找到(书目库)
        //      1   found
        public virtual int GetItemDbName(string strBiblioDbName,
            out string strItemDbName,
            out string strError)
        {
            strItemDbName = "";
            strError = "GetItemDbName() 尚未实现";
            return -1;
        }

        // 2008/12/8
        // 检测数据库名是当前角色么? (注：不特定是哪个书目库下的成员库)
        public virtual bool IsItemDbName(string strItemDbName)
        {
            throw new Exception("尚未实现 IsItemDbName");
        }

        // 2008/12/8
        // 通过事项数据库名找到书目库名
        // return:
        //      -1  error
        //      0   没有找到(事项库)
        //      1   found
        public virtual int GetBiblioDbName(string strItemDbName,
            out string strBiblioDbName,
            out string strError)
        {
            strBiblioDbName = "";
            strError = "GetBiblioDbName() 尚未实现";
            return -1;
        }

        // (派生类必须重载)
        // 观察已存在的记录中，唯一性字段是否和要求的一致
        // return:
        //      -1  出错
        //      0   一致
        //      1   不一致。报错信息在strError中
        public virtual int IsLocateInfoCorrect(
            List<string> locateParams,
            XmlDocument domExist,
            out string strError)
        {
#if NO
            strError = "IsLocateInfoCorrect() 尚未实现";
            return -1;
#endif
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }
            string strRefID = locateParams[0];

            if (String.IsNullOrEmpty(strRefID) == false)
            {
                string strExistingRefID = DomUtil.GetElementText(domExist.DocumentElement,
                    "refID");
                if (strExistingRefID != strRefID)
                {
                    strError = this.ItemName + "记录中<refID>元素中的参考ID '" + strExistingRefID + "' 和通过删除操作参数指定的参考ID '" + strRefID + "' 不一致。";
                    return 1;
                }
            }

            return 0;
        }

        // 派生类必须重载
        // 构造用于获取事项记录的XML检索式
        public virtual int MakeGetItemRecXmlSearchQuery(
            List<string> locateParams,
            int nMax,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "MakeGetItemRecXmlSearchQuery() 尚未实现";
            return -1;
        }

        // 派生类必须重载
        // 构造定位提示信息。用于报错。
        public virtual int GetLocateText(
            List<string> locateParams,
            out string strText,
            out string strError)
        {
#if NO
            strText = "";
            strError = "MakeGetItemRecXmlSearchQuery() 尚未实现";
            return 0;
#endif
            strText = "";
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }
            string strRefID = locateParams[0];

            strText = "参考ID为 '" + strRefID + "'";
            return 0;
        }

        // (派生类必须重载)
        // 定位参数值是否为空?
        // return:
        //      -1  出错
        //      0   不为空
        //      1   为空(这时需要在strError中给出报错说明文字)
        public virtual int IsLocateParamNullOrEmpty(
            List<string> locateParams,
            out string strError)
        {
#if NO
            strError = "IsLocateParamNullOrEmpty() 尚未实现";
            return 0;
#endif
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }
            string strRefID = locateParams[0];


            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strError = "参考ID 为空";
                return 1;
            }

            return 0;
        }

        // 对新旧事项记录中包含的定位信息进行比较, 看看是否发生了变化(进而就需要查重)
        // parameters:
        //      oldLocateParam   顺便返回旧记录中的定位参数
        //      newLocateParam   顺便返回新记录中的定位参数
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        public virtual int CompareTwoItemLocateInfo(
            string strItemDbName,
            XmlDocument domOldRec,
            XmlDocument domNewRec,
            out List<string> oldLocateParam,
            out List<string> newLocateParam,
            out string strError)
        {
#if NO
            oldLocateParam = null;
            newLocateParam = null;

            strError = "CompareTwoItemLocateInfo() 尚未实现";
            return -1;
#endif
            strError = "";

            // 2012/4/1 改造
            string strOldRefID = DomUtil.GetElementText(domOldRec.DocumentElement,
                "refID");

            string strNewRefID = DomUtil.GetElementText(domNewRec.DocumentElement,
                "refID");

            oldLocateParam = new List<string>();
            oldLocateParam.Add(strOldRefID);

            newLocateParam = new List<string>();
            newLocateParam.Add(strNewRefID);

            if (strOldRefID != strNewRefID)
                return 1;   // 不相等

            return 0;   // 相等。
        }

        public virtual void LockItem(List<string> locateParam)
        {
            string strRefID = locateParam[0];

            this.App.EntityLocks.LockForWrite(
                this.ItemNameInternal + ":" + strRefID);
        }

        public virtual void UnlockItem(List<string> locateParam)
        {
            string strRefID = locateParam[0];

            this.App.EntityLocks.UnlockForWrite(
                this.ItemNameInternal + ":" + strRefID);
        }

        // (派生类必须重载)
        // 观察已经存在的记录是否有流通信息
        // return:
        //      -1  出错
        //      0   没有
        //      1   有。报错信息在strError中
        public virtual int HasCirculationInfo(XmlDocument domExist,
            out string strError)
        {
            strError = "";
            return 0;
        }

        // (派生类必须重载)
        // 记录是否允许删除?
        // return:
        //      -1  出错。不允许删除。
        //      0   不允许删除，因为权限不够等原因。原因在strError中
        //      1   可以删除
        public virtual int CanDelete(
            SessionInfo sessioninfo,
            XmlDocument domExist,
            out string strError)
        {
            strError = "";
            return 1;
        }


        // (派生类必须重载)
        // 比较两个记录, 看看和事项要害信息有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        public virtual int IsItemInfoChanged(XmlDocument domExist,
            XmlDocument domOldRec)
        {
            throw new Exception("IsItemInfoChanged() 尚未实现");    // 2009/1/9 changed
        }


        // (派生类必须重载)
        // DoOperChange()和DoOperMove()的下级函数
        // 合并新旧记录
        // parameters:
        // return:
        //      -1  出错
        //      0   正确
        //      1   有部分修改没有兑现。说明在strError中
        public virtual int MergeTwoItemXml(
            SessionInfo sessioninfo,
            XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "MergeTwoItemXml() 尚未实现";

            return -1;
        }

        // 是否允许创建新记录?
        // parameters:
        // return:
        //      -1  出错。不允许修改。
        //      0   不允许创建，因为权限不够等原因。原因在strError中
        //      1   可以创建
        public virtual int CanCreate(
            SessionInfo sessioninfo,
            XmlDocument domNew,
            out string strError)
        {
            strError = "";

            return 1;
        }

        // (派生类必须重载)
        // DoOperChange()和DoOperMove()的下级函数
        // 是否允许对旧记录进行修改?
        // parameters:
        //      strAction   change/move
        // return:
        //      -1  出错。不允许修改。
        //      0   不允许修改，因为权限不够等原因。原因在strError中
        //      1   可以修改
        public virtual int CanChange(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument domExist,
            XmlDocument domNew,
            out string strError)
        {
            strError = "";

            return 1;
        }

        // 构造出适合保存的新事项记录
        // parameters:
        //      bForce  是否为强制保存?
        public virtual int BuildNewItemRecord(
            SessionInfo sessioninfo,
            bool bForce,
            string strBiblioRecId,
            string strOriginXml,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "BuildNewItemRecord() 尚未实现";

            return -1;
        }

        // 获得事项记录
        // 本函数可获得超过1条以上的路径
        // parameters:
        //      timestamp   返回命中的第一条的timestamp
        //      strStyle    如果包含 withresmetadata ,表示要在XML记录中返回<dprms:file>元素内的 __xxx 属性
        // return:
        //      -1  error
        //      0   not found
        //      1   命中1条
        //      >1  命中多于1条
        public int GetItemRecXml(
            // RmsChannelCollection channels,
            RmsChannel channel,
            List<string> locateParams,
            string strStyle,
            out string strXml,
            int nMax,
            out List<string> aPath,
            out byte[] timestamp,
            out string strError)
        {
            aPath = null;

            strXml = "";
            strError = "";
            timestamp = null;
            // 构造检索式

            /*
            string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "出版时间")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strPublishTime)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml += "<operator value='AND'/>";


            strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strIssueDbName + ":" + "父记录")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strParentID)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml = "<group>" + strQueryXml + "</group>";
             * */
            // 构造用于获取事项记录的XML检索式
            string strQueryXml = "";
            int nRet = MakeGetItemRecXmlSearchQuery(
                locateParams,
                nMax,
                out strQueryXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

#if NO
            RmsChannel channel = channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                string strText = "";
                // 构造定位提示信息。用于报错。
                nRet = GetLocateText(
                    locateParams,
                    out strText,
                    out strError);
                if (nRet == -1)
                {
                    strError = "定位信息没有找到。并且GetLocateText()函数报错: " + strError;
                    return 0;
                }

                strError = strText + " 的事项没有找到";
                return 0;
            }

            long lHitCount = lRet;

            // List<string> aPath = null;
            lRet = channel.DoGetSearchResult(
                "default",
                0,
                Math.Min(nMax, lHitCount),
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            Debug.Assert(aPath != null, "");

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error";
                goto ERROR1;
            }

            string strMetaData = "";
            string strOutputPath = "";
            string strGetStyle = "content,data,metadata,timestamp,outputpath";

            if (StringUtil.IsInList("withresmetadata", strStyle) == true)
                strGetStyle += ",withresmetadata";

            lRet = channel.GetRes(aPath[0],
                strGetStyle,
                out strXml,
                out strMetaData,
                out timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 根据 定位信息 对事项库进行查重
        // 本函数只负责查重, 并不获得记录体
        // return:
        //      -1  error
        //      其他    命中记录条数(不超过nMax规定的极限)
        public int SearchItemRecDup(
            // RmsChannelCollection channels,
            RmsChannel channel,
            List<string> locateParams,
            /*
            string strIssueDbName,
            string strParentID,
            string strPublishTime,
             * */
            int nMax,
            out List<string> aPath,
            out string strError)
        {
            strError = "";
            aPath = null;

            // 构造检索式
            string strQueryXml = "";
            int nRet = MakeGetItemRecXmlSearchQuery(
                locateParams,
                100,
                out strQueryXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            /*
            RmsChannel channel = channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
             * */
            Debug.Assert(channel != null, "");

            long lRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            // not found
            if (lRet == 0)
            {
                string strText = "";
                // 构造定位提示信息。用于报错。
                nRet = GetLocateText(
                    locateParams,
                    out strText,
                    out strError);
                if (nRet == -1)
                {
                    strError = "定位信息没有找到。并且GetLocateText()函数报错: " + strError;
                    return 0;
                }


                strError = strText + " 的事项没有找到";
                return 0;
            }

            long lHitCount = lRet;

            lRet = channel.DoGetSearchResult(
                "default",
                0,
                nMax,
                "zh",
                null,
                out aPath,
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (aPath.Count == 0)
            {
                strError = "DoGetSearchResult aPath error 和前面已经命中的条件矛盾";
                goto ERROR1;
            }

            return (int)lHitCount;
        ERROR1:
            return -1;
        }

        // 删除事项记录的操作
        int DoOperDelete(
            SessionInfo sessioninfo,
            RmsChannel channel,
            EntityInfo info,
            List<string> oldLocateParams,
            /*
            string strIssueDbName,
            string strParentID,
            string strOldPublishTime,
             * */
            XmlDocument domOldRec,
            bool bForce,
            bool bSimulate,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            EntityInfo error = null;
            int nRet = 0;
            long lRet = 0;
            string strError = "";

            /*
            // 如果newrecpath为空但是oldrecpath有值，就用oldrecpath的值
            // 2007/10/23
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                if (String.IsNullOrEmpty(info.OldRecPath) == false)
                    info.NewRecPath = info.OldRecPath;
            }*/

            // 2008/6/24
            if (String.IsNullOrEmpty(info.NewRecPath) == false)
            {
                if (info.NewRecPath != info.OldRecPath)
                {
                    strError = "action为delete时, 如果info.NewRecPath不空，则其内容必须和info.OldRecPath一致。(info.NewRecPath='" + info.NewRecPath + "' info.OldRecPath='" + info.OldRecPath + "')";
                    return -1;
                }
            }
            else
            {
                info.NewRecPath = info.OldRecPath;
            }


            string strText = "";
            // 构造定位提示信息。用于报错。
            nRet = GetLocateText(
                oldLocateParams,
                out strText,
                out strError);
            if (nRet == -1)
            {
                strError = "GetLocateText()函数报错: " + strError;
                goto ERROR1;
            }

            // 如果记录路径为空, 则先获得记录路径
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                List<string> aPath = null;

                nRet = IsLocateParamNullOrEmpty(
                    oldLocateParams,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    strError += "info.OldRecord中的" + strError + " 和 info.RecPath参数值为空，同时出现，这是不允许的";
                    goto ERROR1;
                }

                /*
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.App.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
                 * */

                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                nRet = this.SearchItemRecDup(
                    //  sessioninfo.Channels,
                    channel,
                    oldLocateParams,
                    100,
                    out aPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "删除操作中事项查重阶段发生错误:" + strError;
                    goto ERROR1;
                }

                if (nRet == 0)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = strText + " 的记录已不存在";
                    error.ErrorCode = ErrorCodeValue.NotFound;
                    ErrorInfos.Add(error);
                    return -1;
                }

                if (nRet > 1)
                {
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                     * */

                    strError = strText + " 已经被下列多条事项记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'，这是一个严重的系统故障，请尽快通知系统管理员处理。";
                    goto ERROR1;
                }

                info.NewRecPath = aPath[0];
            }

            Debug.Assert(String.IsNullOrEmpty(info.NewRecPath) == false, "");
            // Debug.Assert(strEntityDbName != "", "");

            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";
            string strExistingXml = "";

        REDOLOAD:

            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(info.NewRecPath,
                out strExistingXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = strText + " 的事项记录 '" + info.NewRecPath + "' 已不存在";
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "删除操作发生错误, 在读入原有记录 '" + info.NewRecPath + "' 阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // 把记录装入DOM
            XmlDocument domExist = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistingXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            // 观察已存在的记录中，唯一性字段是否和要求的一致
            // return:
            //      -1  出错
            //      0   一致
            //      1   不一致。报错信息在strError中
            nRet = IsLocateInfoCorrect(
                oldLocateParams,
                domExist,
                out strError);
            if (nRet != 0)
                goto ERROR1;

            if (bForce == false)
            {
                // 观察已经存在的记录是否有流通信息
                // return:
                //      -1  出错
                //      0   没有
                //      1   有。报错信息在strError中
                nRet = HasCirculationInfo(domExist,
                    out strError);
                if (nRet != 0)
                    goto ERROR1;
            }

            if (bForce == false)
            {
                // 记录是否允许删除?
                // return:
                //      -1  出错。不允许删除。
                //      0   不允许删除，因为权限不够等原因。原因在strError中
                //      1   可以删除
                nRet = CanDelete(
                    sessioninfo,
                    domExist,
                    out strError);
                if (nRet != 1)
                    goto ERROR1;
            }


            // 比较时间戳
            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 2008/10/19
                if (bForce == true)
                {
                    error = new EntityInfo(info);
                    error.NewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                    error.ErrorInfo = "数据库中即将删除的册记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // 如果前端给出了旧记录，就有和库中记录进行比较的基础
                if (String.IsNullOrEmpty(info.OldRecord) == false)
                {
                    // 比较两个记录, 看看和事项要害信息有关的字段是否发生了变化
                    // return:
                    //      0   没有变化
                    //      1   有变化
                    nRet = IsItemInfoChanged(domExist,
                        domOldRec);
                    if (nRet == 1)
                    {

                        error = new EntityInfo(info);
                        error.NewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                        error.ErrorInfo = "数据库中即将删除的" + this.ItemName + "记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                        error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }

                info.OldTimestamp = exist_timestamp;
                info.NewTimestamp = exist_timestamp;
            }

            byte[] output_timestamp = null;

            lRet = channel.DoDeleteRes(info.NewRecPath,
                info.OldTimestamp,
                bSimulate ? "simulate" : "",
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复删除均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.ErrorInfo = "删除操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else
            {
                // 成功
                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "delete");

                // 不创建<record>元素

                // 创建<oldRecord>元素
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistingXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);


                // 如果删除成功，则不必要在数组中返回表示成功的信息元素了
            }

            return 0;
        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }


        // 执行API中的"move"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // return:
        //      -1  出错
        //      0   成功
        int DoOperMove(
            SessionInfo sessioninfo,
            // string strUserID,
            RmsChannel channel,
            EntityInfo info,
            bool bSimulate,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            EntityInfo error = null;
            bool bExist = true;    // info.RecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // 检查路径
            if (info.OldRecPath == info.NewRecPath)
            {
                strError = "当action为\"move\"时，info.NewRecordPath路径 '" + info.NewRecPath + "' 和info.OldRecPath '" + info.OldRecPath + "' 必须不相同";
                goto ERROR1;
            }

            // 检查即将覆盖的目标位置是不是有记录，如果有，则不允许进行move操作。
            // 如果要进行带覆盖目标位置记录功能的move操作，前端可以先执行一个delete操作，然后再执行move操作。
            // 这样规定，是为了避免过于复杂的判断逻辑，也便于前端操作者清楚操作的后果。
            // 因为如果允许move带有覆盖目标记录功能，则被覆盖的记录的预删除操作，等于进行了一次事项注销，但这个效用不明显，对前端操作人员准确判断事态并对后果负责(而且可能这种注销需要额外的操作权限)，不利
            bool bAppendStyle = false;  // 目标路径是否为追加形态？
            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                bAppendStyle = true;

            string strOutputPath = "";
            string strMetaData = "";

            if (bAppendStyle == false)
            {
                string strExistTargetXml = "";
                byte[] exist_target_timestamp = null;

                // 获取覆盖目标位置的现有记录
                lRet = channel.GetRes(info.NewRecPath,
                    out strExistTargetXml,
                    out strMetaData,
                    out exist_target_timestamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        // 如果记录不存在, 说明不会造成覆盖态势
                        /*
                        strExistSourceXml = "<root />";
                        exist_source_timestamp = null;
                        strOutputPath = info.NewRecPath;
                         * */
                    }
                    else
                    {
                        error = new EntityInfo(info);
                        error.ErrorInfo = "move操作发生错误, 发生在读入即将覆盖的目标位置 '" + info.NewRecPath + "' 原有记录阶段:" + strError;
                        error.ErrorCode = channel.OriginErrorCode;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }
                else
                {
                    // 如果记录存在，则目前不允许这样的操作
                    strError = "移动(move)操作被拒绝。因为在即将覆盖的目标位置 '" + info.NewRecPath + "' 已经存在" + this.ItemName + "记录。请先删除(delete)这条记录，再进行移动(move)操作";
                    goto ERROR1;
                }
            }


            string strExistSourceXml = "";
            byte[] exist_source_timestamp = null;

            // 先读出数据库中源位置的已有记录
            // REDOLOAD:

            lRet = channel.GetRes(info.OldRecPath,
                out strExistSourceXml,
                out strMetaData,
                out exist_source_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    /*
                    // 如果记录不存在, 则构造一条空的记录
                    bExist = false;
                    strExistSourceXml = "<root />";
                    exist_source_timestamp = null;
                    strOutputPath = info.NewRecPath;
                     * */
                    // 这种情况如果放宽，会有严重的副作用，所以不让放宽
                    strError = "move操作的源记录 '" + info.OldRecPath + "' 在数据库中不存在，所以无法进行移动操作。";
                    goto ERROR1;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "移动操作发生错误, 在读入库中原有源记录(路径在info.OldRecPath) '" + info.OldRecPath + "' 阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // 把两个记录装入DOM

            XmlDocument domSourceExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domSourceExist.LoadXml(strExistSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecord装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_source_timestamp);
            if (nRet != 0)
            {
                // 时间戳不相等了
                // 需要把info.OldRecord和strExistXml进行比较，看看和事项有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                // 比较两个记录, 看看和事项有关的要害字段是否发生了变化
                // return:
                //      0   没有变化
                //      1   有变化
                nRet = IsItemInfoChanged(domOld,
                    domSourceExist);
                if (nRet == 1)
                {
                    error = new EntityInfo(info);
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    error.OldRecord = strExistSourceXml;
                    error.OldTimestamp = exist_source_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "移动操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已被删除。";
                    else
                        error.ErrorInfo = "移动操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已发生过修改";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_source_timestamp此时已经反映了库中被修改后的记录的时间戳
            }

            // 2011/2/11
            nRet = CanChange(
sessioninfo,
"move",
domSourceExist,
domNew,
out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                error = new EntityInfo(info);
                error.ErrorInfo = strError;
                error.ErrorCode = ErrorCodeValue.AccessDenied;
                ErrorInfos.Add(error);
                return -1;
            }

            // 2010/4/8
            // 
            nRet = this.App.SetOperation(
                ref domNew,
                "moved",
                sessioninfo.UserID, // strUserID,
                "",
                out strError);
            if (nRet == -1)
                goto ERROR1;

            string strWarning = "";
            // 合并新旧记录
            // return:
            //      -1  出错
            //      0   正确
            //      1   有部分修改没有兑现。说明在strError中
            string strNewXml = "";
            nRet = MergeTwoItemXml(
                sessioninfo,
                domSourceExist,
                domNew,
                out strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
                strWarning = strError;

            // 移动记录
            byte[] output_timestamp = null;
            string strIdChangeList = "";
            // TODO: Copy后还要写一次？因为Copy并不写入新记录。
            // 其实Copy的意义在于带走资源。否则还不如用Save+Delete
            lRet = channel.DoCopyRecord(info.OldRecPath,
                info.NewRecPath,
                true,   // bDeleteSourceRecord
                bSimulate ? "simulate" : "",
                out strIdChangeList,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "DoCopyRecord() error :" + strError;
                goto ERROR1;
            }

            // Debug.Assert(strOutputPath == info.NewRecPath);
            string strTargetPath = strOutputPath;

            lRet = channel.DoSaveTextRes(strTargetPath,
                strNewXml,
                false,   // include preamble?
                "content" + (bSimulate ? ",simulate" : ""),
                output_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "移动操作中，" + this.ItemName + "记录 '" + info.OldRecPath + "' 已经被成功移动到 '" + strTargetPath + "' ，但在写入新内容时发生错误: " + strError;

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    // 不进行反复处理。
                    // 因为源已经移动，情况很复杂
                }

                if (bSimulate == false)
                {
                    // 仅仅写入错误日志即可。没有Undo
                    this.App.WriteErrorLog(strError);
                }

                error = new EntityInfo(info);
                error.ErrorInfo = "移动操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // 成功
            {
                info.NewRecPath = strOutputPath;    // 兑现保存的位置，因为可能有追加形式的路径

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "move");

                // 新记录
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // 旧记录
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistSourceXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "移动操作成功。NewRecPath中返回了实际保存的路径, NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的源记录稍有差异)。";
                if (string.IsNullOrEmpty(strWarning) == false)
                {
                    error.ErrorInfo = "移动操作成功。但" + strWarning;
                    error.ErrorCode = ErrorCodeValue.PartialDenied;
                }
                else
                    error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;
        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }


        // 执行API中的"change"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // return:
        //      -1  出错
        //      0   成功
        public int DoOperChange(
            // string strUserID,
            SessionInfo sessioninfo,
            RmsChannel channel,
            EntityInfo info,
            bool bForce,
            bool bSimulate,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            EntityInfo error = null;
            bool bExist = true;    // info.RecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // 检查一下路径
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                strError = "info.NewRecPath中的路径不能为空";
                goto ERROR1;
            }

            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?")
            {
                strError = "info.NewRecPath路径 '" + strTargetRecId + "' 中记录ID部分不能为'?'";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strTargetRecId) == true)
            {
                strError = "info.NewRecPath路径 '" + strTargetRecId + "' 中记录ID部分不能为空";
                goto ERROR1;
            }

            if (info.OldRecPath != info.NewRecPath)
            {
                strError = "当action为\"change\"时，info.NewRecordPath路径 '" + info.NewRecPath + "' 和info.OldRecPath '" + info.OldRecPath + "' 必须相同";
                goto ERROR1;
            }

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";


            // 先读出数据库中即将覆盖位置的已有记录
        REDOLOAD:
            lRet = channel.GetRes(info.NewRecPath,
                out strExistXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    // 如果记录不存在, 则构造一条空的记录
                    bExist = false;
                    strExistXml = "<root />";
                    exist_timestamp = null;
                    strOutputPath = info.NewRecPath;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "保存操作发生错误, 在读入原有记录阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }


            // 把两个记录装入DOM

            XmlDocument domExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecord装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 时间戳不相等了
                // 需要把info.OldRecord和strExistXml进行比较，看看和业务有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (bForce == false)
                {
                    // 比较两个记录, 看看和事项有关的字段是否发生了变化
                    // return:
                    //      0   没有变化
                    //      1   有变化
                    nRet = IsItemInfoChanged(domOld,
                        domExist);
                }

                if (nRet == 1 || bForce == true)    // 2008/10/19
                {
                    error = new EntityInfo(info);
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    error.OldRecord = strExistXml;
                    error.OldTimestamp = exist_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已被删除。";
                    else
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已发生过修改";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_timestamp此时已经反映了库中被修改后的记录的时间戳
            }

            // 合并新旧记录
            string strWarning = "";
            string strNewXml = "";
            if (bForce == false)
            {
                // 2011/2/11
                nRet = CanChange(
    sessioninfo,
    "change",
    domExist,
    domNew,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.AccessDenied;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // 2017/3/2
                {
                    string strOldRefID = DomUtil.GetElementText(domNew.DocumentElement, "refID");
                    if (string.IsNullOrEmpty(strOldRefID))
                        DomUtil.SetElementText(domNew.DocumentElement, "refID", Guid.NewGuid().ToString());
                }

                // 2010/4/8
                nRet = this.App.SetOperation(
                    ref domNew,
                    "lastModified",
                    sessioninfo.UserID,
                    "",
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // return:
                //      -1  出错
                //      0   正确
                //      1   有部分修改没有兑现。说明在strError中
                nRet = MergeTwoItemXml(
                    sessioninfo,
                    domExist,
                    domNew,
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strWarning = strError;
            }
            else
            {
                // 2008/10/19
                strNewXml = domNew.OuterXml;
            }

            // 保存新记录
            byte[] output_timestamp = null;
            lRet = channel.DoSaveTextRes(info.NewRecPath,
                strNewXml,
                false,   // include preamble?
                "content" + (bSimulate ? ",simulate" : ""),
                exist_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复保存均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new EntityInfo(info);
                error.ErrorInfo = "保存操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // 成功
            {
                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "change");

                // 新记录
                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "record", strNewXml);
                DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                // 旧记录
                node = DomUtil.SetElementText(domOperLog.DocumentElement,
                    "oldRecord", strExistXml);
                DomUtil.SetAttr(node, "recPath", info.OldRecPath);

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "保存操作成功。NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                if (string.IsNullOrEmpty(strWarning) == false)
                {
                    error.ErrorInfo = "保存操作成功。但" + strWarning;
                    error.ErrorCode = ErrorCodeValue.PartialDenied;
                }
                else
                    error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;
        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // 设置/保存事项信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分。库名可以用来确定书目库，id可以被实体记录用来设置<parent>元素内容。另外书目库名和IssueInfo中的NewRecPath形成映照关系，需要检查它们是否正确对应
        //      issueinfos 要提交的的期信息数组
        // 权限：需要有setissues权限
        // 修改意见: 写入期库中的记录, 还缺乏<operator>和<operTime>字段
        // TODO: 需要改写，增加upgrade中直接写入不查重不创建事件日志的功能
        // TODO: 需要检查订购记录的<parent>元素内容是否合法。不能为问号
        public LibraryServerResult SetItems(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            EntityInfo[] iteminfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = new LibraryServerResult();

            int nRet = 0;
            long lRet = 0;
            string strError = "";

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            if (string.IsNullOrEmpty(strBiblioRecPath) == false)    // 2013/9/26
            {
                if (String.IsNullOrEmpty(strBiblioRecId) == true)
                {
                    strError = "书目记录路径 '" + strBiblioRecPath + "' 中的记录ID部分不能为空";
                    goto ERROR1;
                }
                if (StringUtil.IsPureNumber(strBiblioRecId) == false)
                {
                    strError = "书目记录路径 '" + strBiblioRecPath + "' 中的记录ID部分 '" + strBiblioRecId + "' 格式不正确，应为纯数字";
                    goto ERROR1;
                }
            }

            // 获得书目库对应的事项库名
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;
#if NO
            if (nRet == 0)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的"+this.ItemName+"库名没有定义";
                goto ERROR1;
            }
#endif

            // 2012/3/29
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }
            if (sessioninfo.Channels == null)
            {
                strError = "sessioninfo.Channels == null";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            byte[] output_timestamp = null;
            string strOutputPath = "";

            List<EntityInfo> ErrorInfos = new List<EntityInfo>();

            if (iteminfos == null)
            {
                strError = "iteminfos == null";
                goto ERROR1;
            }

            foreach (EntityInfo info in iteminfos)
            {
                // EntityInfo info = iteminfos[i];
                if (info == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                string strAction = info.Action;

                bool bForce = false;    // 是否为强制操作(强制操作不去除源记录中的流通信息字段内容)
                bool bNoCheckDup = false;   // 是否为不查重?
                bool bNoEventLog = false;   // 是否为不记入事件日志?

                string strStyle = info.Style;

                bool bSimulate = StringUtil.IsInList("simulate", strStyle);

                if (StringUtil.IsInList("force", info.Style) == true)
                {
                    if (sessioninfo.UserType == "reader")
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'force' 的修改" + this.ItemName + "信息的" + strAction + "操作被拒绝。读者身份不能进行这样的操作。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    bForce = true;

                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'force' 的修改" + this.ItemName + "信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (StringUtil.IsInList("nocheckdup", info.Style) == true)
                {
                    bNoCheckDup = true;
                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'nocheckdup' 的修改" + this.ItemName + "信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (StringUtil.IsInList("noeventlog", info.Style) == true)
                {
                    bNoEventLog = true;
                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'noeventlog' 的修改" + this.ItemName + "信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // 对info内的参数进行检查。
                strError = "";

                if (iteminfos.Length > 1  // 2013/9/26 只有一个记录的时候，不必依靠 refid 定位返回信息，因而也就不需要明显给出这个 RefID 成员了
                    && String.IsNullOrEmpty(info.RefID) == true)
                {
                    strError = "info.RefID 没有给出";
                }

                if (string.IsNullOrEmpty(info.NewRecPath) == false
                    && info.NewRecPath.IndexOf(",") != -1)
                {
                    strError = "info.NewRecPath值 '" + info.NewRecPath + "' 中不能包含逗号";
                }
                else if (string.IsNullOrEmpty(info.OldRecPath) == false
                    && info.OldRecPath.IndexOf(",") != -1)
                {
                    strError = "info.OldRecPath值 '" + info.OldRecPath + "' 中不能包含逗号";
                }

                // TODO: 当操作为"delete"时，是否可以允许只设置OldRecPath，而不必设置NewRecPath
                // 如果两个都设置，则要求设置为一致的。
                if (info.Action == "delete")
                {
                    if (String.IsNullOrEmpty(info.NewRecord) == false)
                    {
                        strError = "strAction值为delete时, info.NewRecord参数必须为空";
                    }
                    else if (info.NewTimestamp != null)
                    {
                        strError = "strAction值为delete时, info.NewTimestamp参数必须为空";
                    }
                    // 2008/6/24
                    else if (String.IsNullOrEmpty(info.NewRecPath) == false)
                    {
                        if (info.NewRecPath != info.OldRecPath)
                        {
                            strError = "strAction值为delete时, 如果info.NewRecPath不空，则其内容必须和info.OldRecPath一致。(info.NewRecPath='" + info.NewRecPath + "' info.OldRecPath='" + info.OldRecPath + "')";
                        }
                    }
                }
                else
                {
                    // 非delete情况 info.NewRecord则必须不为空
                    if (String.IsNullOrEmpty(info.NewRecord) == true)
                    {
                        strError = "strAction值为" + info.Action + "时, info.NewRecord参数不能为空";
                    }
                }

                if (info.Action == "new")
                {
                    if (String.IsNullOrEmpty(info.OldRecord) == false)
                    {
                        strError = "strAction值为new时, info.OldRecord参数必须为空";
                    }
                    else if (info.OldTimestamp != null)
                    {
                        strError = "strAction值为new时, info.OldTimestamp参数必须为空";
                    }
                }

                if (strError != "")
                {
                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                // 检查路径中的库名部分
                if (String.IsNullOrEmpty(info.NewRecPath) == false)
                {
                    strError = "";

                    string strDbName = ResPath.GetDbName(info.NewRecPath);

                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "NewRecPath中数据库名不应为空";
                    }

                    if (string.IsNullOrEmpty(strItemDbName) == false    // 有可能前面 strBiblioRecPath 为空，则 strItemDbName 也为空
                        && strDbName != strItemDbName)
                    {
                        // 检测是否为其他语言的等同库名
                        // parameters:
                        //      strDbName   要检测的数据库名
                        //      strNeutralDbName    已知的中立语言数据库名
                        if (this.App.IsOtherLangName(strDbName,
                            strItemDbName) == false)
                        {
                            if (strAction == "copy" || strAction == "move")
                            {
                                // 再看strDbName是否至少是一个实体库
                                if (this.IsItemDbName(strDbName) == false)
                                    strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为" + this.ItemName + "库名";
                            }
                            else
                                strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为 '" + strItemDbName + "'。(因为书目库名为 '" + strBiblioDbName + "'，其对应的" + this.ItemName + "库名应为 '" + strItemDbName + "' )";
                        }
                    }
                    else if (string.IsNullOrEmpty(strItemDbName) == true)   // 2013/9/26
                    {
                        // 要检查看看 strDbName 是否为一个实体库名
                        if (this.IsItemDbName(strDbName) == false)
                            strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为" + this.ItemName + "库名";
                    }

                    if (strError != "")
                    {
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = strError;
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                        continue;
                    }
                }

                // 把(前端发过来的)旧记录装载到DOM
                XmlDocument domOldRec = new XmlDocument();
                try
                {
                    // 用strOldRecord的目的是不想改变info.OldRecord内容, 因为后者可能被复制到输出信息中
                    string strOldRecord = info.OldRecord;
                    if (String.IsNullOrEmpty(strOldRecord) == true)
                        strOldRecord = "<root />";

                    domOldRec.LoadXml(strOldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord XML记录装载到DOM时出错: " + ex.Message;

                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                // 把要保存的新记录装载到DOM
                XmlDocument domNewRec = new XmlDocument();
                try
                {
                    // 用strNewRecord的目的是不想改变info.NewRecord内容, 因为后者可能被复制到输出信息中
                    string strNewRecord = info.NewRecord;

                    if (String.IsNullOrEmpty(strNewRecord) == true)
                        strNewRecord = "<root />";

                    domNewRec.LoadXml(strNewRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.NewRecord XML记录装载到DOM时出错: " + ex.Message;

                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                // locateParam多元组 准备
                List<string> oldLocateParam = null;
                List<string> newLocateParam = null;

                /*
                string strOldPublishTime = "";
                string strNewPublishTime = "";

                string strOldParentID = "";
                string strNewParentID = "";
                 * */

                // 加锁用的参数
                List<string> lockLocateParam = null;
                bool bLocked = false;

                try
                {
                    // 命令new和change的共有部分 -- 出版时间查重, 也需要加锁
                    // delete则需要加锁
                    if (info.Action == "new"
                        || info.Action == "change"
                        || info.Action == "delete"
                        || info.Action == "move")
                    {
                        // 仅仅用来获取一下新出版时间
                        // 看看新旧出版时间是否有差异
                        // 对IssueInfo中的OldRecord和NewRecord中包含的条码号进行比较, 看看是否发生了变化(进而就需要查重)
                        // return:
                        //      -1  出错
                        //      0   相等
                        //      1   不相等
                        nRet = CompareTwoItemLocateInfo(
                            strItemDbName,
                            domOldRec,
                            domNewRec,
                            out oldLocateParam,
                            out newLocateParam,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "CompareTwoIssueNo() error : " + strError;
                            goto ERROR1;
                        }

                        bool bIsOldNewLocateSame = false;
                        if (nRet == 0)
                            bIsOldNewLocateSame = true;
                        else
                            bIsOldNewLocateSame = false;

                        if (info.Action == "new"
                            || info.Action == "change"
                            || info.Action == "move")
                            lockLocateParam = newLocateParam;
                        else if (info.Action == "delete")
                        {
                            // 顺便进行一些检查
                            /*
                            if (String.IsNullOrEmpty(strNewPublishTime) == false)
                            {
                                strError = "没有必要在delete操作的EntityInfo中, 包含NewRecord内容...。相反，注意一定要在OldRecord中包含即将删除的原记录";
                                goto ERROR1;
                            }
                             * */
                            if (String.IsNullOrEmpty(info.NewRecord) == false)
                            {
                                strError = "没有必要在delete操作的EntityInfo中, 包含NewRecord内容...。相反，注意一定要在OldRecord中包含即将删除的原记录";
                                goto ERROR1;
                            }

                            lockLocateParam = oldLocateParam;
                        }

                        nRet = this.IsLocateParamNullOrEmpty(
                            lockLocateParam,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 加锁
                        if (nRet == 0)
                        {
                            this.LockItem(lockLocateParam);
                            bLocked = true;
                        }

                        bool bIsNewLocateParamNull = false;
                        nRet = this.IsLocateParamNullOrEmpty(
                            newLocateParam,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                            bIsNewLocateParamNull = true;
                        else
                            bIsNewLocateParamNull = false;

                        if ((info.Action == "new"
        || info.Action == "change"
        || info.Action == "move")       // delete操作不校验记录
    && bNoCheckDup == false)
                        {
                            nRet = this.DoVerifyItemFunction(
                                sessioninfo,
                                strAction,
                                domNewRec,
                                out strError);
                            if (nRet != 0)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }


                        // 进行出版时间查重
                        // TODO: 查重的时候要注意，如果操作类型为“move”，则可以允许查出和info.OldRecPath重的，因为它即将被删除
                        if (/*bIsOldNewLocateSame == false   // 新旧出版时间不等，才查重。这样可以提高运行效率。
                            &&*/ (info.Action == "new"
                                || info.Action == "change"
                                || info.Action == "move")       // delete操作不查重
                            && bIsNewLocateParamNull == false
                            && bNoCheckDup == false)    // 2008/10/19
                        {
                            /*
                            string strParentID = strNewParentID;

                            if (String.IsNullOrEmpty(strParentID) == true)
                                strParentID = strOldParentID;
                             * */

                            // TODO: 对于期记录，oldLocateParm和newLocateParam中的parentid应当相等，预先检查好

                            List<string> aPath = null;
                            // 根据 父记录ID+出版时间 对期库进行查重
                            // 本函数只负责查重, 并不获得记录体
                            // return:
                            //      -1  error
                            //      其他    命中记录条数(不超过nMax规定的极限)
                            nRet = this.SearchItemRecDup(
                                // sessioninfo.Channels,
                                channel,
                                newLocateParam,
                                100,
                                out aPath,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            bool bDup = false;
                            if (nRet == 0)
                            {
                                bDup = false;
                            }
                            else if (nRet == 1) // 命中一条
                            {
                                if (aPath == null
                                    || aPath.Count == 0)
                                {
                                    strError = "aPath == null || aPath.Count == 0";
                                    goto ERROR1;
                                }

                                Debug.Assert(aPath.Count == 1, "");

                                if (info.Action == "new")
                                {
                                    if (aPath[0] == info.NewRecPath) // 正好是自己
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号

                                }
                                else if (info.Action == "change")
                                {
                                    Debug.Assert(info.NewRecPath == info.OldRecPath, "当操作类型为change时，info.NewRecPath应当和info.OldRecPath相同");
                                    if (aPath[0] == info.OldRecPath) // 正好是自己
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号
                                }
                                else if (info.Action == "move")
                                {
                                    if (aPath[0] == info.OldRecPath) // 正好是源记录
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号
                                }
                                else
                                {
                                    Debug.Assert(false, "这里不可能出现的info.Action值 '" + info.Action + "'");
                                }
                            } // end of if (nRet == 1)
                            else
                            {
                                Debug.Assert(nRet > 1, "");
                                bDup = true;

                                // 因为move操作不允许目标位置存在记录，所以这里就不再费力考虑了
                                // 如果将来move操作允许目标位置存在记录，则这里需要判断：无论源还是目标位置发现条码号重，都不算重。
                            }

                            // 报错
                            if (bDup == true)
                            {
                                /*
                                string[] pathlist = new string[aPath.Count];
                                aPath.CopyTo(pathlist);
                                 * */

                                string strText = "";
                                // 构造定位提示信息。用于报错。
                                nRet = GetLocateText(
                                    newLocateParam,
                                    out strText,
                                    out strError);
                                if (nRet == -1)
                                {
                                    strError = "定位信息重复。并且GetLocateText()函数报错: " + strError;
                                }
                                else
                                {
                                    strError = strText + " 已经被下列" + this.ItemName + "记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/;
                                }

                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError; // "出版时间 '" + strNewPublishTime + "' 已经被下列"+this.ItemName+"记录使用了: " + String.Join(",", pathlist);
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }
                    }

                    // 准备日志DOM
                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");

                    Debug.Assert(String.IsNullOrEmpty(this.OperLogSetName) == false, "");
                    Debug.Assert(Char.IsLower(this.OperLogSetName[0]) == true, this.OperLogSetName + " 的第一个字符应当为小写字母，这是惯例");
                    // 和馆代码模糊有关。如果要写入馆代码，可以考虑滞后写入
                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "operation",
                        OperLogSetName /*"setIssue"*/);

                    // 兑现一个命令
                    if (info.Action == "new")
                    {
                        // 检查新记录的路径中的id部分是否正确
                        // 库名部分，前面已经统一检查过了
                        strError = "";

                        if (String.IsNullOrEmpty(info.NewRecPath) == true)
                        {
                            info.NewRecPath = strItemDbName + "/?";
                        }
                        else
                        {
                            string strID = ResPath.GetRecordId(info.NewRecPath);
                            if (String.IsNullOrEmpty(strID) == true)
                            {
                                strError = "RecPath中id部分应当为'?'";
                            }

                            if (strError != "")
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }

                        // 构造出适合保存的新事项记录
                        string strNewXml = "";
                        nRet = BuildNewItemRecord(
                            sessioninfo,
                            bForce,
                            strBiblioRecId,
                            info.NewRecord,
                            out strNewXml,
                            out strError);
                        if (nRet == -1)
                        {
                            EntityInfo error = new EntityInfo(info);
                            error.ErrorInfo = strError;
                            error.ErrorCode = ErrorCodeValue.CommonError;
                            ErrorInfos.Add(error);
                            continue;
                        }

                        {
                            XmlDocument domNew = new XmlDocument();
                            try
                            {
                                domNew.LoadXml(strNewXml);
                            }
                            catch (Exception ex)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "将拟创建的XML记录装入DOM时出错：" + ex.Message;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }

                            // 2017/3/2
                            {
                                string strOldRefID = DomUtil.GetElementText(domNew.DocumentElement, "refID");
                                if (string.IsNullOrEmpty(strOldRefID))
                                    DomUtil.SetElementText(domNew.DocumentElement, "refID", Guid.NewGuid().ToString());
                            }

                            // 2011/4/11
                            nRet = CanCreate(
                sessioninfo,
                domNew,
                out strError);
                            if (nRet == -1)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                            if (nRet == 0)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = strError;
                                error.ErrorCode = ErrorCodeValue.AccessDenied;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }

                        // 2010/4/8
                        XmlDocument temp = new XmlDocument();
                        temp.LoadXml(strNewXml);
                        nRet = this.App.SetOperation(
                            ref temp,
                            "create",
                            sessioninfo.UserID,
                            "",
                            out strError);
                        if (nRet == -1)
                        {
                            EntityInfo error = new EntityInfo(info);
                            error.ErrorInfo = strError;
                            error.ErrorCode = ErrorCodeValue.CommonError;
                            ErrorInfos.Add(error);
                            continue;
                        }
                        strNewXml = temp.DocumentElement.OuterXml;

                        lRet = channel.DoSaveTextRes(info.NewRecPath,
                            strNewXml,
                            false,   // include preamble?
                            "content" + (bSimulate ? ",simulate" : ""),
                            info.OldTimestamp,
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            EntityInfo error = new EntityInfo(info);
                            error.NewTimestamp = output_timestamp;
                            error.ErrorInfo = "保存新记录的操作发生错误:" + strError;
                            error.ErrorCode = channel.OriginErrorCode;
                            ErrorInfos.Add(error);

                            domOperLog = null;  // 表示不必写入日志
                        }
                        else // 成功
                        {
                            DomUtil.SetElementText(domOperLog.DocumentElement,
                                "action",
                                "new");

                            // 不创建<oldRecord>元素

                            // 创建<record>元素
                            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                                "record", strNewXml);
                            DomUtil.SetAttr(node, "recPath", strOutputPath);

                            // 新记录保存成功，需要返回信息元素。因为需要返回新的时间戳和实际保存的记录路径

                            EntityInfo error = new EntityInfo(info);
                            error.NewRecPath = strOutputPath;

                            error.NewRecord = strNewXml;    // 所真正保存的记录，可能稍有变化, 因此需要返回给前端
                            error.NewTimestamp = output_timestamp;

                            error.ErrorInfo = "保存新记录的操作成功。NewTimeStamp中返回了新的时间戳, RecPath中返回了实际存入的记录路径。";
                            error.ErrorCode = ErrorCodeValue.NoError;
                            ErrorInfos.Add(error);
                        }
                    }
                    else if (info.Action == "change")
                    {
                        // 执行SetIssues API中的"change"操作
                        nRet = DoOperChange(
                            sessioninfo,
                            channel,
                            info,
                            bForce,
                            bSimulate,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // 失败
                            domOperLog = null;  // 表示不必写入日志
                        }
                    }
                    else if (info.Action == "move")
                    {
                        // 执行SetIssues API中的"move"操作
                        nRet = DoOperMove(
                            sessioninfo,
                            channel,
                            info,
                            bSimulate,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // 失败
                            domOperLog = null;  // 表示不必写入日志
                        }
                    }
                    else if (info.Action == "delete")
                    {
                        /*
                        string strParentID = strNewParentID;

                        if (String.IsNullOrEmpty(strParentID) == true)
                            strParentID = strOldParentID;
                         * */

                        // TODO: 对于期记录，oldLocateParm中应当包含parentid，预先检查好

                        // 删除期记录的操作
                        nRet = DoOperDelete(
                            sessioninfo,
                            channel,
                            info,
                            oldLocateParam,
                            domOldRec,
                            bForce,
                            bSimulate,
                            ref domOperLog,
                            ref ErrorInfos);
                        if (nRet == -1)
                        {
                            // 失败
                            domOperLog = null;  // 表示不必写入日志
                        }
                    }
                    else
                    {
                        // 不支持的命令
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = "不支持的操作命令 '" + info.Action + "'";
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                    }

                    // 写入日志
                    if (domOperLog != null
                        && bNoEventLog == false    // 2008/10/19
                        && bSimulate == false)
                    {
                        string strOperTime = this.App.Clock.GetClock();
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "operator",
                            sessioninfo.UserID);   // 操作者
                        DomUtil.SetElementText(domOperLog.DocumentElement,
                            "operTime",
                            strOperTime);   // 操作时间

                        nRet = this.App.OperLog.WriteOperLog(domOperLog,
                            sessioninfo.ClientAddress,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = this.SetApiName + "() API 写入日志时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }
                }
                finally
                {
                    if (bLocked == true)
                        this.UnlockItem(lockLocateParam);
                }

            }

            // 复制到结果中
            errorinfos = new EntityInfo[ErrorInfos.Count];
            for (int i = 0; i < ErrorInfos.Count; i++)
            {
                errorinfos[i] = ErrorInfos[i];
            }

            result.Value = ErrorInfos.Count;  // 返回信息的数量
            return result;
        ERROR1:
            // 这里的报错，是比较严重的错误。如果是数组中部分的请求发生的错误，则不在这里报错，而是通过返回错误信息数组的方式来表现
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 执行脚本函数 VerifyItem
        // parameters:
        // return:
        //      -2  not found script
        //      -1  出错
        //      0   成功
        public int DoVerifyItemFunction(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            Assembly assembly = null;
            // return:
            //      -1  出错
            //      0   Assembly 为空
            //      1   找到 Assembly
            int nRet = this.App.GetAssembly("findBase",
        out assembly,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "未定义<script>脚本代码，无法校验 " + this.ItemName + " 记录。";
                return -2;
            }

            Debug.Assert(assembly != null, "");
#if NO
            if (this.App.m_strAssemblyLibraryHostError != "")
            {
                strError = this.App.m_strAssemblyLibraryHostError;
                return -1;
            }

            if (this.App.m_assemblyLibraryHost == null)
            {
                strError = "未定义<script>脚本代码，无法校验册记录。";
                return -2;
            }
#endif

            Type hostEntryClassType = ScriptManager.GetDerivedClassType(
                this.App.m_assemblyLibraryHost,
                "DigitalPlatform.LibraryServer.LibraryHost");
            if (hostEntryClassType == null)
            {
                strError = "<script>脚本中未找到DigitalPlatform.LibraryServer.LibraryHost类的派生类，无法校验条码号。";
                return -2;
            }

#if NO
            // 迟绑定技术。从assembly中实时寻找特定名字的函数
            MethodInfo mi = hostEntryClassType.GetMethod("VerifyItem");
            if (mi == null)
            {
                strError = "<script>脚本中DigitalPlatform.LibraryServer.LibraryHost类的派生类中，没有提供int VerifyItem(string strAction, XmlDocument itemdom, out string strError)函数，因此无法校验册记录。";
                return -2;
            }
#endif

            LibraryHost host = (LibraryHost)hostEntryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);
            if (host == null)
            {
                strError = "创建 DigitalPlatform.LibraryServer.LibraryHost 类的派生类的对象（构造函数）失败。";
                return -1;
            }

            host.App = this.App;
            host.SessionInfo = sessioninfo;

            // 执行函数
            return VerifyItem(host,
                strAction,
                itemdom,
                out strError);
        }

        // return:
        //      -1  调用出错
        //      0   校验正确
        //      1   校验发现错误
        public virtual int VerifyItem(
            LibraryHost host,
            string strAction,
            XmlDocument itemdom,
            out string strError)
        {
            strError = "";

            return 0;
        }

        // TODO: 对于期记录，需要有限定期范围的能力
        // 获得事项库中全部从属于strBiblioRecPath的记录信息
        // 注：要求每类事项库都有一个“父记录”检索途径
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分
        //      strStyle    "onlygetpath"   仅返回每个路径(OldRecPath)
        //                  "getfirstxml"   是对onlygetpath的补充，仅获得第一个元素的XML记录，其余的依然只返回路径
        //                  "query:父记录+期号|..." 使用特定的检索途径和检索词。...部分表示检索词，例如 1|2005|1|，默认前方一致
        //      items 返回的事项信息数组
        // 权限：权限要在API外面判断(需要有get...s权限)。
        // return:
        //      Result.Value    -1出错 0没有找到 其他 实体记录的个数
        public LibraryServerResult GetItems(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,
            string strLang,
            out EntityInfo[] items)
        {
            items = null;

            LibraryServerResult result = new LibraryServerResult();

            int nRet = 0;
            string strError = "";

            // 规范化参数值
            if (lCount == 0)
                lCount = -1;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的事项库名
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                result.Value = -1;
                result.ErrorInfo = strError;
                result.ErrorCode = ErrorCode.ItemDbNotDef;  // 2016/4/15
                return result;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 2016/10/6
            // 从style字符串中得到 format:XXXX子串
            string strQueryParam = StringUtil.GetStyleParam(strStyle, "query");
            string strFrom = "父记录";
            string strWord = strBiblioRecId;
            string strMatchStyle = "exact";
            if (string.IsNullOrEmpty(strQueryParam) == false)
            {
                List<string> parts = StringUtil.ParseTwoPart(strQueryParam, "|");
                strFrom = parts[0];
                strWord = parts[1];
                strMatchStyle = "left";
            }

            // 检索事项库中全部从属于特定id的记录
            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strItemDbName + ":" + strFrom)       // 2007/9/14
                + "'><item><word>"
                + strWord
                + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
            long lRet = channel.DoSearch(strQueryXml,
                this.DefaultResultsetName,
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "没有找到";
                return result;
            }

            int MAXPERBATCH = 100;

            int nResultCount = (int)lRet;

            if (lCount == -1)
                lCount = nResultCount - (int)lStart;

            // lStart是否越界
            if (lStart >= (long)nResultCount)
            {
                strError = "lStart参数值 " + lStart.ToString() + " 超过了命中结果集的尾部。命中结果数量为 " + nResultCount.ToString();
                goto ERROR1;
            }

            // 修正lCount
            if (lStart + lCount > (long)nResultCount)
            {
                lCount = (long)nResultCount - lStart;
            }

            // 是否超过每批最大值
            if (lCount > MAXPERBATCH)
                lCount = MAXPERBATCH;

            /*
            if (nResultCount > 10000)
            {
                strError = "命中"+this.ItemName+"记录数 " + nResultCount.ToString() + " 超过 10000, 暂时不支持";
                goto ERROR1;
            }
             * */
            bool bOnlyGetPath = StringUtil.IsInList("onlygetpath", strStyle);
            bool bGetFirstXml = StringUtil.IsInList("getfirstxml", strStyle);

            string strColumnStyle = "id,xml,timestamp";
            if (bOnlyGetPath)
                strColumnStyle = "id";

            List<EntityInfo> iteminfos = new List<EntityInfo>();

            /*
            int nStart = 0;
            int nPerCount = 100;
             * */
            int nStart = (int)lStart;
            int nPerCount = Math.Min(MAXPERBATCH, (int)lCount);

            for (; ; )
            {
#if NO
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    this.DefaultResultsetName,
                    nStart,
                    nPerCount,
                    strLang,
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }
#endif
                Record[] searchresults = null;
                lRet = channel.DoGetSearchResult(
                    this.DefaultResultsetName,
    nStart,
    nPerCount,
    strColumnStyle,
    strLang,
    null,
    out searchresults,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    goto ERROR1;
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    goto ERROR1;
                }

                // 获得每条记录
                // for (int i = 0; i < aPath.Count; i++)
                foreach (Record record in searchresults)
                {
                    EntityInfo iteminfo = new EntityInfo();
                    iteminfo.OldRecPath = record.Path;

                    if (bOnlyGetPath == true)
                    {
                        if (bGetFirstXml == false
                            || iteminfos.Count > 0)
                        {
                            // iteminfo.OldRecPath = aPath[i];
                            goto CONTINUE;
                        }
                    }

                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    if (bGetFirstXml && iteminfos.Count == 0
    && !(record.RecordBody != null && string.IsNullOrEmpty(record.RecordBody.Xml) == false))
                    {
                        lRet = channel.GetRes(// aPath[i],
                            record.Path,
                            out strXml,
                            out strMetaData,
                            out timestamp,
                            out strOutputPath,
                            out strError);
                    }
                    else
                    {
                        lRet = 0;
                        if (record.RecordBody != null)
                        {
                            strXml = record.RecordBody.Xml;
                            timestamp = record.RecordBody.Timestamp;
                            strOutputPath = record.Path;
                        }
                        else
                        {
                            strOutputPath = record.Path;
                            iteminfo.ErrorCode = ErrorCodeValue.NotFound;
                        }
                    }

                    if (lRet == -1)
                    {
                        // iteminfo.OldRecPath = aPath[i];
                        iteminfo.OldRecPath = record.Path;

                        iteminfo.ErrorCode = channel.OriginErrorCode;
                        iteminfo.ErrorInfo = channel.ErrorInfo;

                        iteminfo.OldRecord = "";
                        iteminfo.OldTimestamp = null;

                        iteminfo.NewRecPath = "";
                        iteminfo.NewRecord = "";
                        iteminfo.NewTimestamp = null;
                        iteminfo.Action = "";

                        goto CONTINUE;
                    }

                    iteminfo.OldRecPath = strOutputPath;
                    iteminfo.OldRecord = strXml;
                    iteminfo.OldTimestamp = timestamp;

                    iteminfo.NewRecPath = "";
                    iteminfo.NewRecord = "";
                    iteminfo.NewTimestamp = null;
                    iteminfo.Action = "";

                CONTINUE:
                    iteminfos.Add(iteminfo);
                }

                // nStart += aPath.Count;
                nStart += searchresults.Length;
                if (nStart >= nResultCount)
                    break;

                if (iteminfos.Count >= lCount)
                    break;

                // 修正nPerCount
                if (iteminfos.Count + nPerCount > lCount)
                    nPerCount = (int)lCount - iteminfos.Count;
            }

            // 挂接到结果中
#if NO
            items = new EntityInfo[iteminfos.Count];
            for (int i = 0; i < iteminfos.Count; i++)
            {
                items[i] = iteminfos[i];
            }
#endif
            items = iteminfos.ToArray();

            result.Value = nResultCount;    // items.Length;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 对事项查重
        public LibraryServerResult SearchItemDup(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            List<string> locateParam,
            /*
            string strPublishTime,
            string strBiblioRecPath,
             * */
            int nMax,
            out string[] paths)
        {
            paths = null;

            LibraryServerResult result = new LibraryServerResult();
            int nRet = 0;
            string strError = "";

            List<string> aPath = null;

            nRet = this.SearchItemRecDup(
                // Channels,
                channel,
                locateParam,
                nMax,
                out aPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                paths = new string[0];
                result.Value = 0;
                result.ErrorInfo = "没有找到";
                result.ErrorCode = ErrorCode.NotFound;
                return result;
            }

            // 复制到结果中
            paths = new string[aPath.Count];
            for (int i = 0; i < aPath.Count; i++)
            {
                paths[i] = aPath[i];
            }

            result.Value = paths.Length;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // TODO: 是否检查流通信息，需要可以通过参数控制
        // 检索书目记录下属的事项记录，返回少量必要的信息，可以提供后面实做删除时使用
        // parameters:
        //      strStyle    return_record_xml 要在DeleteEntityInfo结构中返回OldRecord内容
        //                  check_circulation_info 检查是否具有流通信息。如果具有则会报错 2012/12/19 把缺省行为变为此参数
        //                  当包含 limit: 时，定义最多取得记录的个数。例如希望最多取得 10 条，可以定义 limit:10
        // return:
        //      -1  error
        //      0   not exist item dbname
        //      1   exist item dbname
        public int SearchChildItems(RmsChannel channel,
            string strBiblioRecPath,
            string strStyle,
            DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord procCheckRecord,
            object param,
            out long lHitCount,
            out List<DeleteEntityInfo> entityinfos,
            out string strError)
        {
            strError = "";
            lHitCount = 0;
            entityinfos = new List<DeleteEntityInfo>();

            int nRet = 0;

            bool bReturnRecordXml = StringUtil.IsInList("return_record_xml", strStyle);
            bool bCheckCirculationInfo = StringUtil.IsInList("check_circulation_info", strStyle);
            bool bOnlyGetCount = StringUtil.IsInList("only_getcount", strStyle);

            string strLimit = StringUtil.GetParameterByPrefix(strStyle, "limit", ":");

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的事项库名
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            if (String.IsNullOrEmpty(strItemDbName) == true)
                return 0;

            // 检索实体库中全部从属于特定id的记录

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录")
                + "'><item><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                strError = "没有找到属于书目记录 '" + strBiblioRecPath + "' 的任何" + this.ItemName + "记录";
                return 0;
            }

            lHitCount = lRet;

            // 仅返回命中条数
            if (bOnlyGetCount == true)
                return 0;

            int nResultCount = (int)lRet;
            int nMaxCount = 10000;
            if (nResultCount > nMaxCount)
            {
                strError = "命中" + this.ItemName + "记录数 " + nResultCount.ToString() + " 超过 " + nMaxCount.ToString() + ", 暂时不支持针对它们的删除操作";
                goto ERROR1;
            }

            string strColumnStyle = "id,xml,timestamp";

            int nLimit = -1;
            if (string.IsNullOrEmpty(strLimit) == false)
                Int32.TryParse(strLimit, out nLimit);

            int nStart = 0;
            int nPerCount = 100;

            if (nLimit != -1 && nPerCount > nLimit)
                nPerCount = nLimit;

            for (; ; )
            {
#if NO
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }
#endif
                Record[] searchresults = null;
                lRet = channel.DoGetSearchResult(
    "entities",
    nStart,
    nPerCount,
    strColumnStyle,
    "zh",
    null,
    out searchresults,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    goto ERROR1;
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    goto ERROR1;
                }

                // 获得每条记录
                // for (int i = 0; i < aPath.Count; i++)
                int i = 0;
                foreach (Record record in searchresults)
                {
                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    DeleteEntityInfo entityinfo = new DeleteEntityInfo();

                    if (record.RecordBody == null || string.IsNullOrEmpty(record.RecordBody.Xml) == true)
                    {
                        // TODO: 这里需要改造为直接从结果集中获取 xml,timestamp
                        lRet = channel.GetRes(record.Path,
                            out strXml,
                            out strMetaData,
                            out timestamp,
                            out strOutputPath,
                            out strError);

                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                continue;

                            strError = "获取" + this.ItemName + "记录 '" + record.Path + "' 时发生错误: " + strError;
                            goto ERROR1;
                            // goto CONTINUE;
                        }
                    }
                    else
                    {
                        strXml = record.RecordBody.Xml;
                        strOutputPath = record.Path;
                        timestamp = record.RecordBody.Timestamp;
                    }

                    entityinfo.RecPath = strOutputPath;
                    entityinfo.OldTimestamp = timestamp;
                    if (bReturnRecordXml == true)
                        entityinfo.OldRecord = strXml;

                    if (bCheckCirculationInfo == true
                        || procCheckRecord != null)
                    {
                        // 检查是否有借阅信息
                        // 把记录装入DOM
                        XmlDocument domExist = new XmlDocument();

                        try
                        {
                            domExist.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = this.ItemName + "记录 '" + record.Path + "' 装载进入DOM时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        // 2016/11/15
                        if (procCheckRecord != null)
                        {
                            nRet = procCheckRecord(
                                nStart + i,
                                strOutputPath,
                                domExist,
                                timestamp,
                                param,
                                out strError);
                            if (nRet != 0)
                                return nRet;
                        }

                        /*
                        entityinfo.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                            "barcode");
                         * */

                        // TODO: 在日志恢复阶段调用本函数时，是否还有必要检查是否具有流通信息？似乎这时应强制删除为好

                        // 观察已经存在的记录是否有流通信息
                        // return:
                        //      -1  出错
                        //      0   没有
                        //      1   有。报错信息在strError中
                        nRet = this.HasCirculationInfo(domExist, out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                        {
                            strError = "拟删除的" + this.ItemName + "记录 '" + entityinfo.RecPath + "' 中" + strError + "(此种情况可能不限于这一条)，不能删除。因此全部删除操作均被放弃。";
                            goto ERROR1;
                        }
                    }

                    // CONTINUE:
                    entityinfos.Add(entityinfo);

                    i++;
                }

                nStart += searchresults.Length;
                if (nStart >= nResultCount)
                    break;
                if (nLimit != -1 && nStart >= nLimit)
                    break;
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 复制属于同一书目记录的全部实体记录
        // parameters:
        //      strAction   copy / move
        // return:
        //      -2  目标实体库不存在，无法进行复制或者删除
        //      -1  error
        //      >=0  实际复制或者移动的实体记录数
        public int CopyBiblioChildItems(RmsChannel channel,
            string strAction,
            List<DeleteEntityInfo> entityinfos,
            string strTargetBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nOperCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement(strAction == "copy" ? "copy" + this.ItemNameInternal + "Records" : "move" + this.ItemNameInternal + "Records");
                domOperLog.DocumentElement.AppendChild(root);
            }

            // 获得目标书目库下属的实体库名
            string strTargetItemDbName = "";
            string strTargetBiblioDbName = ResPath.GetDbName(strTargetBiblioRecPath);
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = this.GetItemDbName(strTargetBiblioDbName,
                out strTargetItemDbName,
                out strError);
            if (nRet == 0 || string.IsNullOrEmpty(strTargetItemDbName) == true)
            {
                return -2;   // 目标实体库不存在
            }

            string strParentID = ResPath.GetRecordId(strTargetBiblioRecPath);
            if (string.IsNullOrEmpty(strParentID) == true)
            {
                strError = "目标书目记录路径 '" + strTargetBiblioRecPath + "' 不正确，无法获得记录号";
                return -1;
            }

            List<string> newrecordpaths = new List<string>();
            List<string> oldrecordpaths = new List<string>();
            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                string strOutputRecPath = "";

                // this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 装入XMLDOM发生错误: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.SetElementText(dom.DocumentElement,
                        "parent",
                        strParentID);

                    // 复制的情况
                    if (strAction == "copy")
                    {
                        // 避免refID重复
                        DomUtil.SetElementText(dom.DocumentElement,
                            "refID",
                            null);
                    }

                    long lRet = channel.DoCopyRecord(info.RecPath,
                         strTargetItemDbName + "/?",
                         strAction == "move" ? true : false,   // bDeleteSourceRecord
                         out output_timestamp,
                         out strOutputRecPath,
                         out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;
                        strError = "复制" + this.ItemName + "记录 '" + info.RecPath + "' 时发生错误: " + strError;
                        goto ERROR1;
                    }

                    // 2011/5/24
                    // 修改xml记录。<parent>元素发生了变化
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(strOutputRecPath,
                        dom.OuterXml,
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    oldrecordpaths.Add(info.RecPath);
                    newrecordpaths.Add(strOutputRecPath);
                }
                finally
                {
                    // this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // 增补到日志DOM中
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                    DomUtil.SetAttr(node, "targetRecPath", strOutputRecPath);
                }

                nOperCount++;
            }

            // 2017/5/30
            if (nOperCount == 0)
                root.ParentNode.RemoveChild(root);

            return nOperCount;
        ERROR1:
            // Undo已经进行过的操作
            if (strAction == "copy")
            {
                string strWarning = "";

                foreach (string strRecPath in newrecordpaths)
                {
                    string strTempError = "";
                    byte[] timestamp = null;
                    byte[] output_timestamp = null;
                REDO_DELETE:
                    long lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strTempError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (timestamp == null)
                            {
                                timestamp = output_timestamp;
                                goto REDO_DELETE;
                            }
                        }
                        strWarning += strTempError + ";";
                    }

                }
                if (string.IsNullOrEmpty(strWarning) == false)
                    strError = strError + "。在Undo过程中，又遇到出错: " + strWarning;
            }
            else if (strAction == "move")
            {
                string strWarning = "";
                for (int i = 0; i < newrecordpaths.Count; i++)
                {
                    byte[] output_timestamp = null;
                    string strOutputRecPath = "";
                    string strTempError = "";
                    long lRet = channel.DoCopyRecord(newrecordpaths[i],
         oldrecordpaths[i],
         true,   // bDeleteSourceRecord
         out output_timestamp,
         out strOutputRecPath,
         out strTempError);
                    if (lRet == -1)
                    {
                        strWarning += strTempError + ";";
                    }
                }
                if (string.IsNullOrEmpty(strWarning) == false)
                    strError = strError + "。在Undo过程中，又遇到出错: " + strWarning;
            }
            return -1;
        }

        // 删除属于同一书目记录的全部实体记录
        // 这是需要提供EntityInfo数组的版本
        // return:
        //      -1  error
        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
        //      >0  实际删除的实体记录数
        public int DeleteBiblioChildItems(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            List<DeleteEntityInfo> entityinfos,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nDeletedCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement("deleted" + this.ItemNameInternal + "Records");
                domOperLog.DocumentElement.AppendChild(root);
            }

#if NO
            RmsChannel channel = Channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
#endif

            // 真正实行删除
            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                int nRedoCount = 0;

            REDO_DELETE:

                // this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {

                    long lRet = channel.DoDeleteRes(info.RecPath,
                        info.OldTimestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        // 如果不重试，让时间戳出错暴露出来。
                        // 如果要重试，也得加上重新读入册记录并判断重新判断无借还信息才能删除

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount > 10)
                            {
                                strError = "重试了10次还不行。删除" + this.ItemName + "记录 '" + info.RecPath + "' 时发生错误: " + strError;
                                goto ERROR1;
                            }
                            nRedoCount++;

                            // 重新读入记录
                            string strMetaData = "";
                            string strXml = "";
                            string strOutputPath = "";
                            string strError_1 = "";

                            lRet = channel.GetRes(info.RecPath,
                                out strXml,
                                out strMetaData,
                                out output_timestamp,
                                out strOutputPath,
                                out strError_1);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                    continue;

                                strError = "在删除" + this.ItemName + "记录 '" + info.RecPath + "' 时发生时间戳冲突，于是自动重新获取记录，但又发生错误: " + strError_1;
                                goto ERROR1;
                                // goto CONTINUE;
                            }

                            // 检查是否有借阅信息
                            // 把记录装入DOM
                            XmlDocument domExist = new XmlDocument();

                            try
                            {
                                if (String.IsNullOrEmpty(strXml) == false)
                                    domExist.LoadXml(strXml);
                                else
                                    domExist.LoadXml("<root />");
                            }
                            catch (Exception ex)
                            {
                                strError = this.ItemName + "记录 '" + info.RecPath + "' XML装载进入DOM时发生错误: " + ex.Message;
                                goto ERROR1;
                            }

                            /*
                            info.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                                "barcode");
                             * */

                            // 观察已经存在的记录是否有流通信息
                            // return:
                            //      -1  出错
                            //      0   没有
                            //      1   有。报错信息在strError中
                            int nRet = this.HasCirculationInfo(domExist, out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 1)
                            {
                                strError = "拟删除的" + this.ItemName + "记录 '" + info.RecPath + "' 中" + strError + "(此种情况可能不限于这一条)，不能删除。";
                                goto ERROR1;
                            }

                            info.OldTimestamp = output_timestamp;
                            goto REDO_DELETE;
                        }

                        strError = "删除" + this.ItemName + "记录 '" + info.RecPath + "' 时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
                finally
                {
                    // this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // 增补到日志DOM中
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                }

                nDeletedCount++;
            }


            return nDeletedCount;
        ERROR1:
            return -1;
        }

        // 删除属于同一书目记录的全部实体记录
        // 这是检索和删除一次进行的版本
        // return:
        //      -1  error
        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
        //      >0  实际删除的实体记录数
        public int DeleteBiblioChildItems(
            // RmsChannelCollection Channels,
            RmsChannel channel,
            string strBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

#if NO
            RmsChannel channel = Channels.GetChannel(this.App.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }
#endif

            List<DeleteEntityInfo> entityinfos = null;
            long lHitCount = 0;

            int nRet = SearchChildItems(channel,
                strBiblioRecPath,
                "check_circulation_info", // 在DeleteEntityInfo结构中*不*返回OldRecord内容
                (DigitalPlatform.LibraryServer.LibraryApplication.Delegate_checkRecord)null,
                null,
                out lHitCount,
                out entityinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            nRet = DeleteBiblioChildItems(
                // Channels,
                channel,
                entityinfos,
                domOperLog,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;
        ERROR1:
            return -1;
        }

#if NOOOOOOOOOOOOOOOOOOOOOOOOOOOO

        // 获得事项信息。多种格式
        // parameters:
        //      strIndex  编号。特殊情况下，可以使用"@path:"引导的订购记录路径(只需要库名和id两个部分)作为检索入口。
        //      strBiblioRecPath    指定书目记录路径
        //      strResultType   指定需要在strResult参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strResult参数中不返回任何数据。无论这个参数为什么值，strItemRecPath中都回返回册记录路径(如果命中了的话)
        //      strItemRecPath  返回册记录路径。可能为逗号间隔的列表，包含多个路径
        //      strBiblioType   指定需要在strBiblio参数中返回的数据格式。为"xml" "html"之一。
        //                      如果为空，则表示strBiblio参数中不返回任何数据。
        //      strOutputBiblioRecPath  输出的书目记录路径。当strIndex的第一字符为'@'时，strBiblioRecPath必须为空，函数返回后，strOutputBiblioRecPath中会包含从属的书目记录路径
        // return:
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        public Result GetCommentInfo(
            List<string> locateParam,
            /*
            string strIndex,
            string strBiblioRecPath,
             * */
            string strResultType,
            out string strResult,
            out string strItemRecPath,
            out byte[] item_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strOutputBiblioRecPath)
        {
            strResult = "";
            strBiblio = "";
            strItemRecPath = "";
            item_timestamp = null;
            strOutputBiblioRecPath = "";

            Result result = new Result();

            int nRet = 0;
            long lRet = 0;

            string strXml = "";
            string strError = "";

            nRet = this.GetCommandItemRecPath(locateParam,
                out strItemRecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
            {
                // 命令行状态，直接返回记录
                string strMetaData = "";
                string strTempOutputPath = "";

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }

                lRet = channel.GetRes(strItemRecPath,
                    out strXml,
                    out strMetaData,
                    out item_timestamp,
                    out strTempOutputPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // 从事项记录<parent>元素中取得书目记录的id，然后拼装成书目记录路径放入strOutputBiblioRecPath
                XmlDocument dom = new XmlDocument();
                try
                {
                    if (String.IsNullOrEmpty(strXml) == true)
                        dom.LoadXml("<root />");
                    else
                        dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "记录 " + strItemRecPath + " 的XML装入DOM时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strItemDbName = ResPath.GetDbName(strItemRecPath);

                // 根据订购库名, 找到对应的书目库名
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = this.GetBiblioDbName(strItemDbName,
                    out strBiblioDbName,
                    out strError);
                if (nRet == -1 || nRet == 0)
                    goto ERROR1;

                strRootID = DomUtil.GetElementText(dom.DocumentElement,
                    "root");
                if (String.IsNullOrEmpty(strRootID) == true)
                {
                    strRootID = DomUtil.GetElementText(dom.DocumentElement,
                        "parent");
                    strError = this.ItemName+"记录 " + strItemRecPath + " 中没有<root>或<parent>元素值，因此无法定位其从属的书目记录";
                    goto ERROR1;
                }
                strBiblioRecPath = strBiblioDbName + "/" + strRootID;
                strOutputBiblioRecPath = strBiblioRecPath;

                result.ErrorInfo = "";
                result.Value = 1;
            }

            string strBiblioRecPath = locateParam[0];

            string strBiblioDbName = "";
            string strCommentDbName = "";
            string strRootID = "";

            {
                /*
                strOutputBiblioRecPath = strBiblioRecPath;

                strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
                // 根据书目库名, 找到对应的期库名
                // return:
                //      -1  出错
                //      0   没有找到(书目库)
                //      1   找到
                nRet = this.GetItemDbName(strBiblioDbName,
                    out strCommentDbName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                    goto ERROR1;
                }

                strRootID = ResPath.GetRecordId(strBiblioRecPath);
                 * */

                List<string> PathList = null;

                nRet = this.GetItemRecXml(
                        sessioninfo.Channels,
                        locateParam,
                        out strXml,
                        100,
                        out PathList,
                        out item_timestamp,
                        out strError);

                if (nRet == 0)
                {
                    result.Value = 0;
                    result.ErrorInfo = "没有找到";
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }

                if (nRet == -1)
                    goto ERROR1;

                /*
                Debug.Assert(PathList != null, "");
                // 构造路径字符串。逗号间隔
                string[] paths = new string[PathList.Count];
                PathList.CopyTo(paths);

                strOrderRecPath = String.Join(",", paths);
                 * */
                strItemRecPath = StringUtil.MakePathList(PathList);

                result.ErrorInfo = strError;
                result.Value = nRet;    // 可能会多于1条
            }



            // 若需要同时取得种记录
            if (String.IsNullOrEmpty(strBiblioType) == false)
            {
                string strBiblioXml = "";

                if (String.Compare(strBiblioType, "recpath", true) == 0)
                {
                    // 如果仅仅需要获得书目记录recpath，则不需要获得书目记录
                    goto DOORDER;
                }

                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "channel == null";
                    goto ERROR1;
                }
                string strMetaData = "";
                byte[] timestamp = null;
                string strTempOutputPath = "";
                lRet = channel.GetRes(strBiblioRecPath,
                    out strBiblioXml,
                    out strMetaData,
                    out timestamp,
                    out strTempOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "获得种记录 '" + strBiblioRecPath + "' 时出错: " + strError;
                    goto ERROR1;
                }

                // 如果只需要种记录的XML格式
                if (String.Compare(strBiblioType, "xml", true) == 0)
                {
                    strBiblio = strBiblioXml;
                    goto DOORDER;
                }


                // 需要从内核映射过来文件
                string strLocalPath = "";

                if (String.Compare(strBiblioType, "html", true) == 0)
                {
                    nRet = app.MapKernelScriptFile(
                        sessioninfo,
                        strBiblioDbName,
                        "./cfgs/loan_biblio.fltx",
                        out strLocalPath,
                        out strError);
                }
                else if (String.Compare(strBiblioType, "text", true) == 0)
                {
                    nRet = app.MapKernelScriptFile(
                        sessioninfo,
                        strBiblioDbName,
                        "./cfgs/loan_biblio_text.fltx",
                        out strLocalPath,
                        out strError);
                }
                else
                {
                    strError = "不能识别的strBiblioType类型 '" + strBiblioType + "'";
                    goto ERROR1;
                }

                if (nRet == -1)
                    goto ERROR1;

                // 将种记录数据从XML格式转换为HTML格式
                string strFilterFileName = strLocalPath;    // app.CfgDir + "\\biblio.fltx";
                nRet = app.ConvertBiblioXmlToHtml(
                        strFilterFileName,
                        strBiblioXml,
                        strBiblioRecPath,
                        out strBiblio,
                        out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

        DOORDER:
            // 取得订购信息
            if (String.IsNullOrEmpty(strResultType) == true
                || String.Compare(strResultType, "recpath", true) == 0)
            {
                strResult = ""; // 不返回任何结果
            }
            else if (String.Compare(strResultType, "xml", true) == 0)
            {
                strResult = strXml;
            }
            else if (String.Compare(strResultType, "html", true) == 0)
            {
                // 将订购记录数据从XML格式转换为HTML格式
                nRet = app.ConvertItemXmlToHtml(
                app.CfgDir + "\\orderxml2html.cs",
                app.CfgDir + "\\orderxml2html.cs.ref",
                    strXml,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (String.Compare(strResultType, "text", true) == 0)
            {
                // 将订购记录数据从XML格式转换为text格式
                nRet = app.ConvertItemXmlToHtml(
                    app.CfgDir + "\\orderxml2text.cs",
                    app.CfgDir + "\\orderxml2text.cs.ref",
                    strXml,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                strError = "未知的订购记录结果类型 '" + strResultType + "'";
                goto ERROR1;
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }
#endif
        // 2015/1/28
        public virtual int BuildLocateParam(// string strBiblioRecPath,
string strRefID,
out List<string> locateParam,
out string strError)
        {
            strError = "派生类尚未实现 BuildLocateParam";
            locateParam = null;
            return -1;
        }

        // 获得命令行中的事项记录路径
        // return:
        //      -1  error
        //      0   不是命令行
        //      1   是命令行
        public virtual int GetCommandItemRecPath(
            List<string> locateParam,
            out string strItemRecPath,
            out string strError)
        {
            throw new Exception("GetCommandItemRecPath() 没有实现");
        }

        // 解析命令行中的事项记录路径
        // return:
        //      -1  error
        //      0   不是命令行
        //      1   是命令行
        public int ParseCommandItemRecPath(
            string strCommandLine,
            out string strItemRecPath,
            out string strError)
        {
            strError = "";
            strItemRecPath = "";

            // 命令状态
            if (strCommandLine[0] == '@')
            {
                // 获得事项记录，通过事项记录路径

                string strLead = "@path:";
                if (strCommandLine.Length <= strLead.Length)
                {
                    strError = "错误的检索词格式: '" + strCommandLine + "'";
                    return -1;
                }

                string strPart = strCommandLine.Substring(0, strLead.Length);
                if (strPart != strLead)
                {
                    strError = "不支持的检索词格式: '" + strCommandLine + "'。目前仅支持'@path:'引导的检索词";
                    return -1;
                }

                strItemRecPath = strCommandLine.Substring(strLead.Length);

                string strItemDbName = ResPath.GetDbName(strItemRecPath);
                // 需要检查一下数据库名是否在允许的事项库名之列
                if (this.IsItemDbName(strItemDbName) == false)
                {
                    strError = this.ItemName + "记录路径 '" + strItemRecPath + "' 中的数据库名 '" + strItemDbName + "' 不在配置的" + this.ItemName + "库名之列";
                    return -1;
                }

                return 1;   // 是命令状态
            }

            return 0;   // 不是命令状态
        }

        // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
        // return:
        //      -1  出错
        //      0   超过管辖范围。strError中有解释
        //      1   在管辖范围内
        public static int DistributeInControlled(string strDistribute,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                return 1;

            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute, out strError);
            if (nRet == -1)
            {
                strError = "馆藏分配字符串 '" + strDistribute + "' 格式不正确";
                return -1;
            }

            foreach (Location location in locations)
            {
                // 空的馆藏地点被视为不在分馆用户管辖范围内
                if (string.IsNullOrEmpty(location.Name) == true)
                {
                    strError = "馆代码 '' 不在范围 '" + strLibraryCodeList + "' 内";
                    return 0;
                }

                string strLibraryCode = "";
                string strPureName = "";

                // 解析
                LibraryApplication.ParseCalendarName(location.Name,
            out strLibraryCode,
            out strPureName);

                if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                {
                    strError = "馆代码 '" + strLibraryCode + "' 不在范围 '" + strLibraryCodeList + "' 内";
                    return 0;
                }
            }

            return 1;
        }
    }
}
