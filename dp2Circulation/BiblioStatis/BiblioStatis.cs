using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// BiblioStatisForm (书目统计窗) 统计方案的宿主类
    /// </summary>
    public class BiblioStatis : StatisHostBase
    {
        /// <summary>
        /// 提示框事件
        /// </summary>
        public event MessagePromptEventHandler Prompt = null;

        /// <summary>
        /// 本对象所关联的 BiblioStatisForm (书目统计窗)
        /// </summary>
        public BiblioStatisForm BiblioStatisForm = null;	// 引用

        /// <summary>
        /// 当前书目库的 数据格式
        /// </summary>
        public string CurrentDbSyntax = "";

        /// <summary>
        /// 当前书目记录路径
        /// </summary>
        public string CurrentRecPath = "";    // 

        /// <summary>
        /// 当前书目记录在整批中的下标。从 0 开始计数。如果为 -1，表示尚未开始处理
        /// </summary>
        public long CurrentRecordIndex = -1; // 

        /// <summary>
        /// 当前正在处理的书目 XML 记录，XmlDocument 类型
        /// </summary>
        public XmlDocument BiblioDom = null;    // Xml装入XmlDocument

        string _biblioFormat = "xml";
        public string BiblioFormat
        {
            get
            {
                return _biblioFormat;
            }
            set
            {
                _biblioFormat = value;
            }
        }

        string m_strXml = "";
        /// <summary>
        /// 当前正在处理的书目 XML 记录，字符串类型
        /// </summary>
        public string Xml
        {
            get
            {
                return this.m_strXml;
            }
            set
            {
                this.m_strXml = value;
            }
        }

        /// <summary>
        /// 当前书目记录的时间戳
        /// </summary>
        public byte[] Timestamp = null;

        /// <summary>
        /// 当前书目记录的 MARC 机内格式字符串
        /// </summary>
        public string MarcRecord = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        public BiblioStatis()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// 将一条 MARC 记录保存到当前正在处理的书目记录的数据库原始位置
        /// 所谓当前位置由 this.CurrentRecPath 决定
        /// 提交保存所采用的时间戳是 this.Timestamp
        /// </summary>
        /// <param name="strMARC">MARC机内格式字符串</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError 中; 0: 成功</returns>
        public int SaveMarcRecord(string strMARC,
            out string strError)
        {
            strError = "";

            string strXml = this.Xml;
            int nRet = MarcUtil.Marc2XmlEx(strMARC,
                this.CurrentDbSyntax,
                ref strXml, // 2015/10/12
                out strError);
            if (nRet == -1)
                return -1;

            string strOutputPath = "";
            byte[] baNewTimestamp = null;
            nRet = this.BiblioStatisForm.SaveXmlBiblioRecordToDatabase(this.CurrentRecPath,
                strXml,
                this.Timestamp,
                out strOutputPath,
                out baNewTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            this.Timestamp = baNewTimestamp;
            return 0;
        }


        // 每一记录，在触发MARCFilter之前
        /// <summary>
        /// 处理一条记录之前。在统计方案执行中，第三阶段，针对每条记录被调用一次，在 OnRecord() 之前触发
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void PreFilter(object sender, StatisEventArgs e)
        {

        }

        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.BiblioStatisForm.MainForm.DataDir, "~biblio_statis");
        }

        // 通用版本
        List<ItemInfo> GetItemInfos(string strDbType,
            string strHowToGetItemRecord,
            ref List<ItemInfo> item_infos)
        {
            // 优化速度
            if (item_infos != null)
                return item_infos;

            // 如果当前书目库下没有包含实体库，调用会抛出异常。特殊处理
            // TODO: 是否需要用hashtable优化速度?
            string strBiblioDBName = Global.GetDbName(this.CurrentRecPath);
            string strItemDbName = "";

            if (strDbType == "item")
                strItemDbName = this.BiblioStatisForm.MainForm.GetItemDbName(strBiblioDBName);
            else if (strDbType == "order")
                strItemDbName = this.BiblioStatisForm.MainForm.GetOrderDbName(strBiblioDBName);
            else if (strDbType == "issue")
                strItemDbName = this.BiblioStatisForm.MainForm.GetIssueDbName(strBiblioDBName);
            else if (strDbType == "comment")
                strItemDbName = this.BiblioStatisForm.MainForm.GetCommentDbName(strBiblioDBName);
            else
            {
                throw new Exception("未知的 strDbType '" + strDbType + "'");
            }

            if (String.IsNullOrEmpty(strItemDbName) == true)
                return new List<ItemInfo>();    // 返回一个空的数组

            item_infos = new List<ItemInfo>();

            long lPerCount = 100; // 每批获得多少个
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;
            for (; ; )
            {

                string strStyle = "";
                if (strHowToGetItemRecord == "delay")
                    strStyle = "onlygetpath";
                else if (strHowToGetItemRecord == "first")
                    strStyle = "onlygetpath,getfirstxml";

                EntityInfo[] infos = null;
                string strError = "";
                long lRet = 0;

                REDO:

                if (strDbType == "item")
                    lRet = this.BiblioStatisForm.Channel.GetEntities(
                         null,
                         this.CurrentRecPath,
                         lStart,
                         lCount,
                         strStyle,
                         "zh",
                         out infos,
                         out strError);
                else if (strDbType == "order")
                    lRet = this.BiblioStatisForm.Channel.GetOrders(
                         null,
                         this.CurrentRecPath,
                         lStart,
                         lCount,
                         strStyle,
                         "zh",
                         out infos,
                         out strError);
                else if (strDbType == "issue")
                    lRet = this.BiblioStatisForm.Channel.GetIssues(
                         null,
                         this.CurrentRecPath,
                         lStart,
                         lCount,
                         strStyle,
                         "zh",
                         out infos,
                         out strError);
                else if (strDbType == "comment")
                    lRet = this.BiblioStatisForm.Channel.GetComments(
                         null,
                         this.CurrentRecPath,
                         lStart,
                         lCount,
                         strStyle,
                         "zh",
                         out infos,
                         out strError);

                if (lRet == -1)
                {
                    // 2018/6/6
                    if (this.Prompt != null)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs
                        {
                            MessageText = "获得 " + strDbType + " 记录时发生错误： " + strError,
                            Actions = "yes,no,cancel"
                        };
                        this.Prompt(this, e);
                        if (e.ResultAction == "cancel")
                            throw new ChannelException(this.BiblioStatisForm.Channel.ErrorCode, strError);
                        else if (e.ResultAction == "yes")
                            goto REDO;
                        else
                        {
                            // no 也是抛出异常。因为继续下一批代价太大
                            throw new ChannelException(this.BiblioStatisForm.Channel.ErrorCode, strError);
                        }
                    }
                    else
                        throw new ChannelException(this.BiblioStatisForm.Channel.ErrorCode, strError);
                }

                lResultCount = lRet;    // 2009/11/23 

                if (infos == null)
                    return item_infos;

                for (int i = 0; i < infos.Length; i++)
                {
                    EntityInfo info = infos[i];
                    string strXml = info.OldRecord;

                    /*
                    if (String.IsNullOrEmpty(strXml) == true)
                        continue;
                     * */

                    ItemInfo item_info = new ItemInfo(strDbType);
                    item_info.Container = this;
                    item_info.RecPath = info.OldRecPath;
                    item_info.Timestamp = info.OldTimestamp;
                    item_info.OldRecord = strXml;

                    item_infos.Add(item_info);
                }

                lStart += infos.Length;
                if (lStart >= lResultCount)
                    break;

                if (lCount == -1)
                    lCount = lPerCount;

                if (lStart + lCount > lResultCount)
                    lCount = lResultCount - lStart;

            } // end of for

            return item_infos;
        }

        #region 实体库
        List<ItemInfo> m_itemInfos = null;

        /// <summary>
        /// 如何获得当前书目记录下属的 册 记录 ?
        /// all/delay/first  一次性全部获得/延迟获得/首次获得第一条
        /// </summary>
        public string HowToGetItemRecord = "all";   // all/delay/first  一次性全部获得/延迟获得/首次获得第一条

        internal void ClearItemDoms()
        {
            this.m_itemInfos = null;
        }

        /// <summary>
        /// 获得当前书目记录下属的 册记录信息集合
        /// </summary>
        public List<ItemInfo> ItemInfos
        {
            get
            {
                return this.GetItemInfos("item",
                    this.HowToGetItemRecord,
                    ref this.m_itemInfos);
            }
        }

        // 保存修改过的册信息。
        // 调用本函数前，要修改Dom成员
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 保存当前书目记录下属的 册记录信息
        /// </summary>
        /// <param name="iteminfos">要保存的册记录信息集合</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中； 0: 成功</returns>
        public int SaveItemInfo(List<ItemInfo> iteminfos,
            out string strError)
        {
            return SaveItemInfo("item",
                iteminfos,
                out strError);
        }

        #endregion

        #region 订购库
        List<ItemInfo> m_orderInfos = null;

        /// <summary>
        /// 如何获得当前记录下属的 订购 记录 ?
        /// all/delay/first  一次性全部获得/延迟获得/首次获得第一条
        /// </summary>
        public string HowToGetOrderRecord = "all";   // all/delay/first  一次性全部获得/延迟获得/首次获得第一条

        internal void ClearOrderDoms()
        {
            this.m_orderInfos = null;
        }

        /// <summary>
        /// 获得获得当前书目记录下属的 订购记录信息集合
        /// </summary>
        public List<ItemInfo> OrderInfos
        {
            get
            {
                return this.GetItemInfos("order",
                    this.HowToGetOrderRecord,
                    ref this.m_orderInfos);
            }
        }

        // 保存修改过的订购信息。
        // 调用本函数前，要修改Dom成员
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 保存当前书目记录下属的 订购记录信息
        /// </summary>
        /// <param name="orderinfos">要保存的订购记录信息集合</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中； 0: 成功</returns>
        public int SaveOrderInfo(List<ItemInfo> orderinfos,
            out string strError)
        {
            return SaveItemInfo("order",
                orderinfos,
                out strError);
        }

        #endregion

        #region 期库
        List<ItemInfo> m_issueInfos = null;

        /// <summary>
        /// 如何获得当前书目记录下属的 期 记录 ?
        /// all/delay/first  一次性全部获得/延迟获得/首次获得第一条
        /// </summary>
        public string HowToGetIssueRecord = "all";   // all/delay/first  一次性全部获得/延迟获得/首次获得第一条

        internal void ClearIssueDoms()
        {
            this.m_issueInfos = null;
        }

        /// <summary>
        /// 获得当前书目记录下属的 期记录信息集合
        /// </summary>
        public List<ItemInfo> IssueInfos
        {
            get
            {
                return this.GetItemInfos("issue",
                    this.HowToGetIssueRecord,
                    ref this.m_issueInfos);
            }
        }

        // 保存修改过的期信息。
        // 调用本函数前，要修改Dom成员
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 保存当前书目记录下属的 期记录信息
        /// </summary>
        /// <param name="issueinfos">要保存的期记录信息集合</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中； 0: 成功</returns>
        public int SaveIssueInfo(List<ItemInfo> issueinfos,
            out string strError)
        {
            return SaveItemInfo("issue",
                issueinfos,
                out strError);
        }
        #endregion

        #region 评注库
        List<ItemInfo> m_commentInfos = null;

        /// <summary>
        /// 如何获得当前记录下属的 评注 记录 ?
        /// all/delay/first  一次性全部获得/延迟获得/首次获得第一条
        /// </summary>
        public string HowToGetCommentRecord = "all";   // all/delay/first  一次性全部获得/延迟获得/首次获得第一条

        internal void ClearCommentDoms()
        {
            this.m_commentInfos = null;
        }

        /// <summary>
        /// 获得获得当前书目记录下属的 评注记录信息集合
        /// </summary>
        public List<ItemInfo> CommentInfos
        {
            get
            {
                return this.GetItemInfos("comment",
                    this.HowToGetCommentRecord,
                    ref this.m_commentInfos);
            }
        }

        // 保存修改过的评注信息。
        // 调用本函数前，要修改Dom成员
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 保存当前书目记录下属的 评注记录信息
        /// </summary>
        /// <param name="commentinfos">要保存的评注记录信息集合</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中； 0: 成功</returns>
        public int SaveCommentInfo(List<ItemInfo> commentinfos,
            out string strError)
        {
            return SaveItemInfo("comment",
                commentinfos,
                out strError);
        }

        #endregion

        // (各种数据库类型通用版本)
        // 保存修改过的册信息。
        // 调用本函数前，要修改Dom成员
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 保存当前书目记录下属的 册/订购/期/评注记录信息
        /// </summary>
        /// <param name="strDbType">下属数据库类型。为 item/order/issue/comment 之一</param>
        /// <param name="iteminfos">要保存的评注记录信息集合</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错。错误信息在 strError 中； 0: 成功</returns>
        public int SaveItemInfo(
            string strDbType,
            List<ItemInfo> iteminfos,
            out string strError)
        {
            strError = "";
            List<EntityInfo> entityArray = new List<EntityInfo>();

            for (int i = 0; i < iteminfos.Count; i++)
            {
                ItemInfo item = iteminfos[i];

                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(item.RefID) == true)
                {
                    item.RefID = Guid.NewGuid().ToString();
                }

                info.RefID = item.RefID;

                DomUtil.SetElementText(item.Dom.DocumentElement,
                    "parent", Global.GetRecordID(CurrentRecPath));

                string strXml = item.Dom.DocumentElement.OuterXml;

                info.OldRecPath = item.RecPath;
                info.Action = "change";
                info.NewRecPath = item.RecPath;

                info.NewRecord = strXml;
                info.NewTimestamp = null;

                info.OldRecord = item.OldRecord;
                info.OldTimestamp = item.Timestamp;

                entityArray.Add(info);
            }

            // 复制到目标
            EntityInfo[] entities = null;
            entities = new EntityInfo[entityArray.Count];
            for (int i = 0; i < entityArray.Count; i++)
            {
                entities[i] = entityArray[i];
            }

            EntityInfo[] errorinfos = null;

            long lRet = 0;

            if (strDbType == "item")
                lRet = this.BiblioStatisForm.Channel.SetEntities(
                     null,   // this.BiblioStatisForm.stop,
                     this.CurrentRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else if (strDbType == "order")
                lRet = this.BiblioStatisForm.Channel.SetOrders(
                     null,   // this.BiblioStatisForm.stop,
                     this.CurrentRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else if (strDbType == "issue")
                lRet = this.BiblioStatisForm.Channel.SetIssues(
                     null,   // this.BiblioStatisForm.stop,
                     this.CurrentRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else if (strDbType == "comment")
                lRet = this.BiblioStatisForm.Channel.SetComments(
                     null,   // this.BiblioStatisForm.stop,
                     this.CurrentRecPath,
                     entities,
                     out errorinfos,
                     out strError);
            else
            {
                strError = "未知的 strDbType '" + strDbType + "'";
                return -1;
            }
            if (lRet == -1)
                return -1;

            // string strWarning = ""; // 警告信息

            if (errorinfos == null)
                return 0;

            strError = "";
            for (int i = 0; i < errorinfos.Length; i++)
            {
                if (String.IsNullOrEmpty(errorinfos[i].RefID) == true)
                {
                    strError = "服务器返回的EntityInfo结构中RefID为空";
                    return -1;
                }

                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                    continue;

                strError += errorinfos[i].RefID + "在提交保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }

            if (String.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }


        /*
        void LineBreak(ref string strText,
            int nLineWidth,
            string strHead,
            string strDelimiters)
        {
            int nStart = 0;
            for (int i=0; ;i++)
            {


            }

        }
         * */

    }

    /// <summary>
    /// 册/订购/期/评注信息
    /// </summary>
    public class ItemInfo
    {
        /// <summary>
        /// 数据库类型。为 item/order/issue/comment 之一 
        /// </summary>
        public string DbType = "item";

        /// <summary>
        /// 记录路径
        /// </summary>
        public string RecPath = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        /// <summary>
        /// 宿主对象
        /// </summary>
        public BiblioStatis Container = null;

        XmlDocument m_dom = null;

        /// <summary>
        /// 获取：记录内容的 XmlDocument 形态
        /// </summary>
        public XmlDocument Dom
        {
            get
            {
                if (m_dom != null)
                    return m_dom;

                string strXml = this.OldRecord;
                this.m_dom = new XmlDocument();
                this.m_dom.LoadXml(strXml);
                return m_dom;
            }
        }

        string m_strOldRecord = "";

        /// <summary>
        /// 获取：旧记录
        /// </summary>
        public string OldRecord
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strOldRecord) == false)
                    return m_strOldRecord;

                if (string.IsNullOrEmpty(this.RecPath) == true)
                    throw new Exception("ItemInfo的RecPath为空，无法获得OldRecord");

                string strBarcodeOrRecPath = "@path:" + this.RecPath;
                string strItemXml = "";
                string strOutputItemRecPath = "";
                byte[] item_timestamp = null;
                string strBiblioText = "";
                string strBiblioRecPath = "";
                string strError = "";
                long lRet = 0;

                if (this.DbType == "item")
                    lRet = this.Container.BiblioStatisForm.Channel.GetItemInfo(
         null,
         strBarcodeOrRecPath,
         "xml",
         out strItemXml,
         out strOutputItemRecPath,
         out item_timestamp,
         "",
         out strBiblioText,
         out strBiblioRecPath,
         out strError);
                else if (this.DbType == "order")
                    lRet = this.Container.BiblioStatisForm.Channel.GetOrderInfo(
         null,
         strBarcodeOrRecPath,
         "xml",
         out strItemXml,
         out strOutputItemRecPath,
         out item_timestamp,
         "",
         out strBiblioText,
         out strBiblioRecPath,
         out strError);
                else if (this.DbType == "issue")
                    lRet = this.Container.BiblioStatisForm.Channel.GetIssueInfo(
         null,
         strBarcodeOrRecPath,
         "xml",
         out strItemXml,
         out strOutputItemRecPath,
         out item_timestamp,
         "",
         out strBiblioText,
         out strBiblioRecPath,
         out strError);
                else if (this.DbType == "comment")
                    lRet = this.Container.BiblioStatisForm.Channel.GetCommentInfo(
         null,
         strBarcodeOrRecPath,
         "xml",
         out strItemXml,
         out strOutputItemRecPath,
         out item_timestamp,
         "",
         out strBiblioText,
         out strBiblioRecPath,
         out strError);
                else
                {
                    throw new Exception("无法识别的 DbType '" + this.DbType + "'");
                }

                if (lRet == -1 || lRet == 0)
                    throw new Exception(strError);
                this.m_strOldRecord = strItemXml;
                this.Timestamp = item_timestamp;
                return strItemXml;
            }
            set
            {
                this.m_strOldRecord = value;
            }
        }

        /*
        public string RefID
        {
            get
            {
                return DomUtil.GetElementText(this.Dom.DocumentElement, "refID");
            }
        }
         * */
        /// <summary>
        /// 参考 ID
        /// </summary>
        public string RefID = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strDbType">数据库类型。值为 item order issue comment 之一</param>
        public ItemInfo(string strDbType)
        {
            Debug.Assert(strDbType == "item"
                || strDbType == "order"
                || strDbType == "issue"
                || strDbType == "comment",
                "");
            this.DbType = strDbType;
        }
    }

}

