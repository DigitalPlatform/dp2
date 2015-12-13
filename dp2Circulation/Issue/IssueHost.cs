using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
// using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

/*
 * 1) 需要输出一个纯文本文件，里面详细描述了每种期刊的期分布情况，和探测出缺期的原理，便于调试。要区分已经建立的期节点和(根据订购信息)预测的期节点
 * 
 * 
 * */

namespace dp2Circulation
{
    /// <summary>
    /// 打印期刊催缺单操作所需要的 处理期信息的宿主类
    /// 它在内存中存储和模拟了期的结构
    /// </summary>
    public class IssueHost
    {
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = null;

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;


        // 
        /// <summary>
        /// 书目记录路径
        /// </summary>
        public string BiblioRecPath = "";

        // 期对象数组
        List<OneIssue> Issues = new List<OneIssue>();

        // 订购对象数组
        List<OneOrder> Orders = new List<OneOrder>();

        /// <summary>
        /// 清除期对象集合
        /// </summary>
        public void ClearIssues()
        {
            this.Issues.Clear();
        }

        /// <summary>
        /// 清除订购对象集合
        /// </summary>
        public void ClearOrders()
        {
            this.Orders.Clear();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

#if NO
        // 装入期记录
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        public int LoadIssueRecords(string strBiblioRecPath,
            out string strError)
        {
            this.BiblioRecPath = strBiblioRecPath;
            this.ClearIssues();
            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入期信息 ...");
            stop.BeginLoop();
             * */

            try
            {
                // string strHtml = "";
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;

                // 2012/5/9 改写为循环方式
                for (; ; )
                {
                    EntityInfo[] issues = null;

                    long lRet = Channel.GetIssues(
                        stop,
                        strBiblioRecPath,
                        lStart,
                        lCount,
                        "",
                        "zh",
                        out issues,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                        return 0;

                    lResultCount = lRet;

                    Debug.Assert(issues != null, "");

                    for (int i = 0; i < issues.Length; i++)
                    {
                        if (issues[i].ErrorCode != ErrorCodeValue.NoError)
                        {
                            strError = "路径为 '" + issues[i].OldRecPath + "' 的期记录装载中发生错误: " + issues[i].ErrorInfo;  // NewRecPath
                            return -1;
                        }

                        OneIssue issue = new OneIssue();
                        int nRet = issue.LoadRecord(issues[i].OldRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "路径为 '" + issues[i].OldRecPath + "' 的期记录用于初始化OneIssue对象时发生错误: " + strError;
                            return -1;
                        }

                        this.Issues.Add(issue);
                    }

                    lStart += issues.Length;
                    if (lStart >= lResultCount)
                        break;
                }
            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                 * */
            }

            return 1;
        ERROR1:
            return -1;
        }
#endif

        // 2012/8/30
        // 
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 装入期记录
        /// </summary>
        /// <param name="strOrderRefID">订购记录的参考 ID</param>
        /// <param name="strOrderTime">订购时间</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有装载
        ///      1   已经装载
        /// </returns>
        public int LoadIssueRecords(string strOrderRefID,
            string strOrderTime,
            out string strError)
        {
            this.ClearIssues();

            // 如果是期刊的订购库，还需要通过订购记录的refid获得期记录，从期记录中才能得到馆藏分配信息
            string strOutputStyle = "";
            long lRet = Channel.SearchIssue(Stop,
"<全部>",
strOrderRefID,
-1,
"订购参考ID",
"exact",
"zh",
"tempissue",
"",
strOutputStyle,
out strError);
            if (lRet == -1)
            {
                strError = "检索 订购参考ID 为 " + strOrderRefID + " 的期记录时出错: " + strError;
                return -1;
            }
            if (lRet == 0)
                return 0;


            long lHitCount = lRet;
            long lStart = 0;
            long lCount = lHitCount;
            DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

            // 获取命中结果
            for (; ; )
            {

                lRet = Channel.GetSearchResult(
                    Stop,
                    "tempissue",
                    lStart,
                    lCount,
                    "id",
                    "zh",
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    strError = "获取结果集时出错: " + strError;
                    return -1;
                }
                if (lRet == 0)
                {
                    strError = "获取结果集时出错: lRet = 0";
                    return -1;
                }

                for (int i = 0; i < searchresults.Length; i++)
                {
                    DigitalPlatform.LibraryClient.localhost.Record searchresult = searchresults[i];

                    string strIssueRecPath = searchresult.Path;

                    string strIssueXml = "";
                    string strOutputIssueRecPath = "";

                    string strBiblioText = "";
                    string strOutputBiblioRecPath = "";
                    byte[] item_timestamp = null;

                    lRet = Channel.GetIssueInfo(
Stop,
"@path:" + strIssueRecPath,
                        // "",
"xml",
out strIssueXml,
out strOutputIssueRecPath,
out item_timestamp,
"recpath",
out strBiblioText,
out strOutputBiblioRecPath,
out strError);
                    if (lRet == -1 || lRet == 0)
                    {
                        strError = "获取期记录 " + strIssueRecPath + " 时出错: " + strError;
                        return -1;
                    }

                    OneIssue issue = new OneIssue();
                    int nRet = issue.LoadRecord(strIssueXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "路径为 '" + strOutputIssueRecPath + "' 的期记录用于初始化OneIssue对象时发生错误: " + strError;
                        return -1;
                    }

                    // 限定
                    issue.OrderRefIDs.Add(strOrderRefID);
                    issue.OrderTime = strOrderTime;

                    this.Issues.Add(issue);

#if NO
                        // 寻找 /orderInfo/* 元素
                        XmlNode nodeRoot = issue_dom.DocumentElement.SelectSingleNode("orderInfo/*[refID/text()='" + strRefID + "']");
                        if (nodeRoot == null)
                        {
                            strError = "期记录 '" + strOutputIssueRecPath + "' 中没有找到<refID>元素值为 '" + strRefID + "' 的订购内容节点...";
                            return -1;
                        }

                        string strDistribute = DomUtil.GetElementText(nodeRoot, "distribute");

                        distributes.Add(strDistribute);
#endif
                }

                lStart += searchresults.Length;
                lCount -= searchresults.Length;

                if (lStart >= lHitCount || lCount <= 0)
                    break;
            }

            return 1;
        }

        // 2012/8/30
        // 
        // parameters:
        //      strOrderTime    返回订购记录的订购时间。RFC1123格式
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 装入订购记录
        /// </summary>
        /// <param name="strOrderRecPath">订购记录路径</param>
        /// <param name="strRefID">返回订购记录的参考 ID</param>
        /// <param name="strOrderTime">返回订购记录的订购时间。RFC1123格式</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有装载
        ///      1   已经装载
        /// </returns>
        public int LoadOrderRecord(string strOrderRecPath,
            out string strRefID,
            out string strOrderTime,
            out string strError)
        {
            strError = "";
            strRefID = "";
            strOrderTime = "";

            this.ClearOrders();

            string strOutputBiblioRecPath = "";
            byte[] order_timestamp = null;
            string strOutputOrderRecPath = "";
            string strResult = "";
            string strBiblio = "";

            long lRet = Channel.GetOrderInfo(
                Stop,
                "@path:" + strOrderRecPath,
                "xml",
                out strResult,
                out strOutputOrderRecPath,
                out order_timestamp,
                "recpath",
                out strBiblio,
                out strOutputBiblioRecPath,
                out strError);
            if (lRet == -1)
                return -1;
            if (lRet == 0)
                return 0;

            this.BiblioRecPath = strOutputBiblioRecPath;

            OneOrder order = new OneOrder();
            int nRet = order.LoadRecord(
                strOutputOrderRecPath,
                strResult,
                out strError);
            if (nRet == -1)
            {
                strError = "路径为 '" + strOutputOrderRecPath + "' 的订购记录用于初始化OneOrder对象时发生错误: " + strError;
                return -1;
            }

            this.Orders.Add(order);

            strRefID = DomUtil.GetElementText(order.Dom.DocumentElement, "refID");
            strOrderTime = DomUtil.GetElementText(order.Dom.DocumentElement, "orderTime");
            return 1;
        }

#if NO
        // 装入订购记录
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        public int LoadOrderRecords(string strBiblioRecPath,
            out string strError)
        {
            this.BiblioRecPath = strBiblioRecPath;
            this.ClearOrders();

            try
            {
                // string strHtml = "";
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;

                // 2012/5/9 改写为循环方式
                for (; ; )
                {
                    EntityInfo[] orders = null;

                    long lRet = Channel.GetOrders(
                        stop,
                        strBiblioRecPath,
                            lStart,
                            lCount,
                            "",
                            "zh",
                        out orders,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                        return 0;

                    lResultCount = lRet;

                    Debug.Assert(orders != null, "");

                    for (int i = 0; i < orders.Length; i++)
                    {
                        if (orders[i].ErrorCode != ErrorCodeValue.NoError)
                        {
                            strError = "路径为 '" + orders[i].OldRecPath + "' 的订购记录装载中发生错误: " + orders[i].ErrorInfo;  // NewRecPath
                            return -1;
                        }

                        OneOrder order = new OneOrder();
                        int nRet = order.LoadRecord(
                            orders[i].OldRecPath,
                            orders[i].OldRecord,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "路径为 '" + orders[i].OldRecPath + "' 的订购记录用于初始化OneOrder对象时发生错误: " + strError;
                            return -1;
                        }

                        this.Orders.Add(order);
                    }

                    lStart += orders.Length;
                    if (lStart >= lResultCount)
                        break;
                }
            }
            finally
            {
                /*
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                 * */
            }

            return 1;
        ERROR1:
            return -1;
        }
#endif

        // 获得可用的最大订购时间范围
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetMaxOrderRange(out string strStartDate,
            out string strEndDate,
            out string strError)
        {
            strStartDate = "";
            strEndDate = "";
            strError = "";

            if (this.Orders == null || this.Orders.Count == 0)
                return 0;

            for (int i = 0; i < this.Orders.Count; i++)
            {
                XmlDocument dom = this.Orders[i].Dom;

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                int nRet = strRange.IndexOf("-");
                if (nRet == -1)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，缺乏-";
                    return -1;
                }

                string strStart = strRange.Substring(0, nRet).Trim();
                string strEnd = strRange.Substring(nRet + 1).Trim();

                if (strStart.Length != 8)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，左边部分字符数不为8";
                    return -1;
                }
                if (strEnd.Length != 8)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，右边部分字符数不为8";
                    return -1;
                }

                if (strStartDate == "")
                    strStartDate = strStart;
                else
                {
                    if (String.Compare(strStartDate, strStart) > 0)
                        strStartDate = strStart;
                }

                if (strEndDate == "")
                    strEndDate = strEnd;
                else
                {
                    if (String.Compare(strEndDate, strEnd) < 0)
                        strEndDate = strEnd;
                }
            }

            if (strStartDate == "")
            {
                Debug.Assert(strEndDate == "", "");
                return 0;
            }

            return 1;
        }

        // 获得一年内的期总数
        // return:
        //      -1  出错
        //      0   无法获得
        //      1   获得
        int GetOneYearIssueCount(string strPublishYear,
            out int nValue,
            out string strError)
        {
            strError = "";
            nValue = 0;

            if (this.Orders == null || this.Orders.Count == 0)
                return 0;

            for (int i = 0; i < this.Orders.Count; i++)
            {
                XmlDocument dom = this.Orders[i].Dom;

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                float years = Global.Years(strRange);
                if (years != 0)
                {
                    nValue = Convert.ToInt32((float)nIssueCount * (1 / years));
                }
            }

            return 1;
        }

        // 检测一个出版时间是否在已经订购的范围内
        // 可能会抛出异常
        bool InOrderRange(string strPublishTime)
        {
            if (this.Orders == null || this.Orders.Count == 0)
                return false;

            for (int i = 0; i < this.Orders.Count; i++)
            {
                XmlDocument dom = this.Orders[i].Dom;

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                // 星号表示通配
                if (strPublishTime != "*")
                {
                    if (Global.InRange(strPublishTime, strRange) == false)
                        continue;
                }

                return true;
            }

            return false;
        }

#if NO
        // 预测下一期的出版时间
        // exception:
        //      可能因strPublishTime为不可能的日期而抛出异常
        // parameters:
        //      strPublishTime  当前这一期出版时间
        //      nIssueCount 一年内出多少期
        static string NextPublishTime(string strPublishTime,
            int nIssueCount)
        {
            DateTime now = DateTimeUtil.Long8ToDateTime(strPublishTime);

            // 一年一期
            if (nIssueCount == 1)
            {
                return DateTimeUtil.DateTimeToString8(DateTimeUtil.NextYear(now));
            }

            // 一年两期
            if (nIssueCount == 2)
            {
                // 6个月以后的同日
                for (int i = 0; i < 6; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年三期
            if (nIssueCount == 3)
            {
                // 4个月以后的同日
                for (int i = 0; i < 4; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年4期
            if (nIssueCount == 4)
            {
                // 3个月以后的同日
                for (int i = 0; i < 3; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年5期 和一年6期处理办法一样
            // 一年6期
            if (nIssueCount == 5 || nIssueCount == 6)
            {
                // 
                // 2个月以后的同日
                for (int i = 0; i < 2; i++)
                {
                    now = DateTimeUtil.NextMonth(now);
                }

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年7/8/9/10/11期 和一年12期处理办法一样
            // 一年12期
            if (nIssueCount >= 7 && nIssueCount <= 12)
            {
                // 1个月以后的同日
                now = DateTimeUtil.NextMonth(now);

                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年24期
            if (nIssueCount == 24)
            {
                // 15天以后
                now += new TimeSpan(15, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年36期
            if (nIssueCount == 36)
            {
                // 10天以后
                now += new TimeSpan(10, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            // 一年48期
            if (nIssueCount == 48)
            {
                // 7天以后
                now += new TimeSpan(7, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }

            return "????????";  // 无法处理的情形
        }
#endif

        // 
        // return:
        //      -1  error
        //      0   无法获得订购时间范围
        //      1   成功
        /// <summary>
        /// 创建每个期对象
        /// </summary>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  error
        ///      0   无法获得订购时间范围
        ///      1   成功
        /// </returns>
        public int CreateIssues(out string strError)
        {
            strError = "";

            List<OneIssue> issues = new List<OneIssue>();

            string strStartDate = "";
            string strEndDate = "";
            // 获得可用的最大订购时间范围
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetMaxOrderRange(out strStartDate,
                out strEndDate,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "无法获得订购时间范围";
                return 0;
            }

            // 在时间范围内寻找当年期号为'1'的已经存在的期节点，
            // 如果不存在，则假定第一期的当年期号为'1'
            string strCurrentPublishTime = strStartDate;
            int nCurrentIssue = 1;

            // 进行循环，增补全部节点
            for (; ; )
            {
                try
                {
                    // 检查一下这个出版时间是否超过订购时间范围?
                    if (InOrderRange(strCurrentPublishTime) == false)
                        break;  // 避免最后多插入一个
                }
                catch (Exception ex)
                {
                    strError = "IssueHost InOrderRange() exception: " + ExceptionUtil.GetAutoText(ex);
                    return -1;
                }

                OneIssue issue = new OneIssue();

                issue.LoadRecord("<root />", out strError);
                issue.PublishTime = strCurrentPublishTime;
                issue.Issue = nCurrentIssue.ToString();

                issues.Add(issue);


                string strNextPublishTime = "";
                int nNextIssue = 0;
                /*
                string strNextIssue = "";
                string strNextZong = "";
                string strNextVolume = "";
                 * */

                {
                    int nIssueCount = 0;
                    // 获得一年内的期总数
                    // return:
                    //      -1  出错
                    //      0   无法获得
                    //      1   获得
                    nRet = GetOneYearIssueCount(strCurrentPublishTime,
                        out nIssueCount,
                        out strError);

                    try
                    {
                        // 预测下一期的出版时间
                        // parameters:
                        //      strPublishTime  当前这一期出版时间
                        //      nIssueCount 一年内出多少期
                        strNextPublishTime = BindingControl.NextPublishTime(strCurrentPublishTime,
                             nIssueCount);
                    }
                    catch (Exception ex)
                    {
                        // 2009/2/8
                        strError = "在获得日期 '" + strCurrentPublishTime + "' 的后一期出版日期时发生错误: " + ex.Message;
                        return -1;
                    }

                    if (strNextPublishTime == "????????")
                        break;

                    try
                    {

                        // 检查一下这个出版时间是否超过订购时间范围?
                        if (InOrderRange(strNextPublishTime) == false)
                            break;  // 避免最后多插入一个
                    }
                    catch (Exception ex)
                    {
                        strError = "IssueHost InOrderRange() {303315A3-E6B2-4720-A073-BCBCB98196BD} exception: " + ex.Message;
                        return -1;
                    }


                    // 号码自动增量需要知道一个期是否跨年，可以通过查询采购信息得到一年所订阅的期数
                    if (nCurrentIssue >= nIssueCount)
                    {
                        // 跨年了
                        // strNextIssue = "1";
                        nNextIssue = 1;

                        // 2012/9/1
                        strNextPublishTime = DateTimeUtil.NextYear(strCurrentPublishTime.Substring(0, 4)) + "0101";
                    }
                    else
                    {
                        // strNextIssue = (nCurrentIssue + 1).ToString();
                        nNextIssue = nCurrentIssue + 1;
                    }

                    /*
                    strNextZong = IncreaseNumber(ref_item.Zong);
                    if (nRefIssue >= nIssueCount && nIssueCount > 0)
                        strNextVolume = IncreaseNumber(ref_item.Volume);
                    else
                        strNextVolume = ref_item.Volume;
                    */
                }

                // nCreateCount++;

                strCurrentPublishTime = strNextPublishTime;
                nCurrentIssue = nNextIssue;
            }


            // 将猜测的期节点合并到this.Issues数组中
            nRet = MergeGuessIssues(issues,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // 匹配期号。strRange中可能为"3/4/5"这样的形态，要能正确匹配上
        static bool MatchIssueNo(string strRange,
            string strOne)
        {
            string[] parts = strRange.Split(new char[] {'/'});
            foreach (string s in parts)
            {
                string strCurrent = s.Trim();
                if (string.IsNullOrEmpty(strCurrent) == true)
                    continue;
                if (strCurrent == strOne)
                    return true;
            }

            return false;
        }

        // 在this.Issues中根据年、期号查找一个节点
        // return:
        //      -1  not found
        //      >=0 found
        int FindIssue(string strYear,
            string strIssue,
            int nStartIndex)
        {

            for (int i = nStartIndex; i < this.Issues.Count; i++)
            {
                OneIssue issue = this.Issues[i];
                string strCurrentYear = issue.PublishTime.Substring(0, 4);
                if (strYear == strCurrentYear
                    && MatchIssueNo(issue.Issue, strIssue) == true)
                {
                    return i;
                }
            }

            return -1;
        }


        // 移动出版时间
        // 可能会抛出异常
        // TODO: 如果移动后超过了允许的时间范围怎么办?
        // parameters:
        //      issues  全部待处理的期对象数组
        //      indices 下标数组。指上面issues数组的下标
        static void MovePublishTime(List<OneIssue> issues,
            List<int> indices,
            TimeSpan delta)
        {
            // 统计越过年范围的程度
            List<TimeSpan> exceeds = new List<TimeSpan>();
            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                OneIssue issue = issues[index];

                string strRest = "";
                string strPublishTime = issue.PublishTime;

                strPublishTime = DateTimeUtil.CanonicalizePublishTimeString(strPublishTime);

                if (strPublishTime.Length > 8)
                {
                    strRest = strPublishTime.Substring(8);
                    strPublishTime = strPublishTime.Substring(0, 8);
                }

                DateTime time = DateTimeUtil.Long8ToDateTime(strPublishTime);
                int nYear = time.Year;
                time = time + delta;

                // 看看一个时间越过指定年份多少距离
                TimeSpan exceed = GetExceedValue(nYear, time);
                if (exceed != new TimeSpan())
                    exceeds.Add(exceed);
            }

            // 调整 delta
            if (exceeds.Count > 0)
            {
                exceeds.Sort();
                if (exceeds[0] < new TimeSpan(0))
                    delta -= exceeds[0];
                else
                    delta -= exceeds[exceeds.Count - 1];
            }

            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                OneIssue issue = issues[index];

                string strRest = "";
                string strPublishTime = issue.PublishTime;

                strPublishTime = DateTimeUtil.CanonicalizePublishTimeString(strPublishTime);

                if (strPublishTime.Length > 8)
                {
                    strRest = strPublishTime.Substring(8);
                    strPublishTime = strPublishTime.Substring(0, 8);
                }

                DateTime time = DateTimeUtil.Long8ToDateTime(strPublishTime);
                int nYear = time.Year;
                time = time + delta;

                Debug.Assert(nYear == time.Year, "");

                strPublishTime = DateTimeUtil.DateTimeToString8(time);
                strPublishTime += strRest;

                issue.PublishTime = strPublishTime;
            }
        }

        // 2012/9/1
        // 看看一个时间越过指定年份多少距离
        static TimeSpan GetExceedValue(int nYear,
            DateTime time)
        {
            if (time.Year > nYear)
                return time - new DateTime(nYear, 12, 31);
            if (time.Year == nYear)
                return new TimeSpan();
            return time - new DateTime(nYear, 1, 1);
        }

#if NO
        // 按照出版时间，将一个期节点插入this.Issues数组的适当位置
        // 可能会抛出异常
        void AddIssueByIssueNo(OneIssue issue)
        {
            // 规范为10位
            string strYear = issue.PublishTime.Substring(0, 4);
            string strIssue = issue.Issue.PadLeft(3, '0');

            string strLastYear = "0000";
            string strLastIssue = "000";

            for (int i = 0; i < this.Issues.Count; i++)
            {
                OneIssue current_issue = this.Issues[i];

                // 规范为10位
                string strCurrentYear = current_issue.PublishTime.Substring(0,4);
                string strCurrentIssue = current_issue.Issue.PadLeft(3, '0');

                if (String.Compare(strYear,strLastYear)>=0 && string.Compare(strIssue, strLastIssue) > 0
                    && String.Compare(strYear, strCurrentYear) <= 0 && String.Compare(strIssue, strCurrentIssue) <= 0)
                {
                    this.Issues.Insert(i, issue);
                    return;
                }

                strLastYear = strCurrentYear;
                strLastIssue = strCurrentIssue;
            }

            this.Issues.Add(issue);
        }
#endif

#if NOOOOOOOOOOOOOOOOOOOOOO
        // 按照出版时间，将一个期节点插入this.Issues数组的适当位置
        // 可能会抛出异常
        void AddIssueByPublishTime(OneIssue issue)
        {
            // 规范为10位
            string strPublishTime = issue.PublishTime;
            strPublishTime = CannonicalizepublishTimeString(strPublishTime);
            strPublishTime = strPublishTime.PadRight(10, '0');

            for (int i = 0; i < this.Issues.Count; i++)
            {
                OneIssue current_issue = this.Issues[i];

                // 规范为10位
                string strCurrentPublishTime = current_issue.PublishTime;
                strCurrentPublishTime = CannonicalizepublishTimeString(strCurrentPublishTime);
                strCurrentPublishTime = strCurrentPublishTime.PadRight(10, '0');

                if (strPublishTime < strCurrentPublishTime)
                {
                    this.Issues.Insert(i, issue);
                    return;
                }
            }

            this.Issues.Add(issue);
        }
#endif

        /// <summary>
        /// 输出全部期对象的调试信息
        /// </summary>
        /// <returns>调试信息</returns>
        public string DumpIssue()
        {
            string strResult = "";
            for (int i = 0; i < this.Issues.Count; i++)
            {
                OneIssue issue = this.Issues[i];

                strResult += "publish_time ["+ issue.PublishTime + "] issue[" + issue.Issue + "] is_guess[" + issue.IsGuess.ToString() + "]\r\n";
            }

            return strResult;
        }

        // 将猜测的期节点合并到this.Issues数组中
        int MergeGuessIssues(List<OneIssue> guess_issues,
            out string strError)
        {
            strError = "";

            try
            {
                List<int> not_matchs = new List<int>();
                int nLastIndex = 0;
                TimeSpan last_delta = new TimeSpan(0);
                for (int i = 0; i < guess_issues.Count; i++)
                {
                    OneIssue guess_issue = guess_issues[i];

                    string strYear = guess_issue.PublishTime.Substring(0, 4);
                    string strIssue = guess_issue.Issue;

                    // 在this.Issues中根据年、期号查找一个节点
                    // return:
                    //      -1  not found
                    //      >=0 found
                    int index = FindIssue(strYear,
                        strIssue,
                        nLastIndex);
                    if (index == -1)
                    {
                        not_matchs.Add(i);  // 没有匹配上的下标

                        // 将一个期节点插入this.Issues数组的适当位置
                        // 可能会抛出异常
                        // AddIssueByIssueNo(guess_issue);
                        this.Issues.Add(guess_issue);   // 后面需要排序

                        guess_issue.IsGuess = true;
                    }
                    else
                    {
                        OneIssue found = this.Issues[index];
                        string strRealPublishTime = found.PublishTime;
                        string strGuessPublishTime = guess_issue.PublishTime;

                        strRealPublishTime = DateTimeUtil.CanonicalizePublishTimeString(strRealPublishTime);
                        strGuessPublishTime = DateTimeUtil.CanonicalizePublishTimeString(strGuessPublishTime);


                        // 看看差多少天，然后对前面没有匹配的节点的出版时间进行相应的平移
                        DateTime real = DateTimeUtil.Long8ToDateTime(strRealPublishTime);
                        DateTime guess = DateTimeUtil.Long8ToDateTime(strGuessPublishTime);
                        TimeSpan delta = real - guess;

                        last_delta = delta;

                        // 移动出版时间
                        // 可能会抛出异常
                        MovePublishTime(guess_issues,
                            not_matchs,
                            delta);
                        not_matchs.Clear();
                    }
                }

                // 最后一段没有匹配上的
                if (not_matchs.Count > 0
                    && last_delta != new TimeSpan(0))
                {
                    // 移动出版时间
                    // 可能会抛出异常
                    MovePublishTime(guess_issues,
                        not_matchs,
                        last_delta);
                    not_matchs.Clear();
                }

                // 按照publishtime和issue排序
                this.Issues.Sort(new OneIssueComparer());

                return 0;
            }
            catch (Exception ex)
            {
                strError = "IssueHost this.Issues.Sort() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
        }

        // 
        // return:
        //      -1  error
        //      0   没有订购信息
        //      1   初始化成功
        /// <summary>
        /// 初始化控件
        /// </summary>
        /// <param name="strOrderRecPath">订购记录路径</param>
        /// <param name="bGuess">是否要猜测未到的期号</param>
        /// <param name="debugInfo">追加调试信息。如果调用前为 null，表示函数执行过程中不产生调试信息</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有订购信息
        ///      1   初始化成功
        /// </returns>
        public int Initial(string strOrderRecPath,
            bool bGuess,
            ref StringBuilder debugInfo,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 装入一条订购记录
            string strRefID = "";
            string strOrderTime = "";
            // return:
            //      -1  出错
            //      0   没有装载
            //      1   已经装载
            nRet = this.LoadOrderRecord(strOrderRecPath,
                out strRefID,
                out strOrderTime,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                strError = "在IssueHost中装入订购记录 " + strOrderRecPath + " 时发生错误: " + strError;
                return -1;
            }

            Debug.Assert(nRet == 1, "");

            if (debugInfo != null)
                debugInfo.Append("检索订购记录 '" + strOrderRecPath + "' 正常命中。refid ["+strRefID+"] ordertime ["+strOrderTime+"]\r\n");

            // 根据订购记录的 refid 装入期记录
            nRet = this.LoadIssueRecords(strRefID,
                strOrderTime,
                out strError);
            if (nRet == -1)
            {
                strError = "在IssueHost中装入和订购记录 " + strRefID + " 关联的期记录时发生错误:" + strError;
                return -1;
            }

            if (debugInfo != null)
            {
                if (nRet == 0)
                    debugInfo.Append("refid '"+strRefID+"' 没有命中任何期记录\r\n");
                else
                    debugInfo.Append("refid '" + strRefID + "' 命中 "+this.Issues.Count.ToString()+" 个期记录\r\n");
            }

            // 即使没有命中任何期记录，也要继续处理

            if (bGuess == true)
            {
                // 创建每个期对象
                // return:
                //      -1  error
                //      0   无法获得订购时间范围
                //      1   成功
                nRet = this.CreateIssues(out strError);
                if (nRet == -1)
                {
                    strError = "在IssueHost中CreateIssues() " + strOrderRecPath + " error: " + strError;
                    return -1;
                }
            }

            if (nRet == 0)
                return 0;

            for (int i = 0; i < this.Issues.Count; i++)
            {
                OneIssue issue = this.Issues[i];

                // 和本期时间匹配的若干个订购对象建立联系
                // return:
                //      -1  error
                //      0   not found
                //      >0  匹配的个数
                nRet = issue.LinkOrders(
                    this.Orders,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 1;
        }

        // 
        // 每期一行，按照书商名进行了汇总
        // TODO: “直订”等需要特殊处理
        // return:
        //      -1  error
        //      0   没有任何信息
        //      >0  信息个数
        /// <summary>
        /// 获得期各种信息
        /// </summary>
        /// <param name="issue_infos">返回期信息集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有任何信息
        ///      >0  信息个数
        /// </returns>
        public int GetIssueInfo(
            out List<IssueInfo> issue_infos,
            out string strError)
        {
            strError = "";

            issue_infos = new List<IssueInfo>();

            for (int i = 0; i < this.Issues.Count; i++)
            {
                OneIssue issue = this.Issues[i];

                string strLastSeller = "";
                int nOrderCount = 0;
                int nArriveCount = 0;

                issue.Orders.Sort(new OrderSorter());
                for (int j = 0; j < issue.Orders.Count; j++)
                {
                    OneOrder order = issue.Orders[j];

                    string strCurrentSeller = order.Seller;
                    if (strCurrentSeller != strLastSeller
                        && strLastSeller != "")
                    {
                        // 将上一段汇总值推出
                        IssueInfo info = new IssueInfo();

                        info.PublishTime = issue.PublishTime;
                        info.OrderTime = issue.OrderTime;   // 2012/8/31
                        info.Issue = issue.Issue;
                        info.Seller = strLastSeller;
                        info.OrderCount = nOrderCount.ToString();
                        info.ArrivedCount = nArriveCount.ToString();
                        info.MissingCount = Math.Max(0, nOrderCount - nArriveCount).ToString();
                        issue_infos.Add(info);

                        nOrderCount = 0;
                        nArriveCount = 0;
                    }

                    nOrderCount += order.OldCopyValue;
                    nArriveCount += order.NewCopyValue;
                    strLastSeller = strCurrentSeller;
                }

                // 将最后一段汇总值推出
                if (strLastSeller != "")
                {
                    IssueInfo info = new IssueInfo();

                    info.PublishTime = issue.PublishTime;
                    info.OrderTime = issue.OrderTime;   // 2012/8/31
                    info.Issue = issue.Issue;
                    info.Seller = strLastSeller;
                    info.OrderCount = nOrderCount.ToString();
                    info.ArrivedCount = nArriveCount.ToString();
                    info.MissingCount = Math.Max(0, nOrderCount - nArriveCount).ToString();
                    issue_infos.Add(info);
                }

            }

            return issue_infos.Count;
        }

        // 
        /// <summary>
        /// 将IssueInfo数组排序后按照书商名拆分为独立的数组
        /// </summary>
        /// <param name="issue_infos">IssueInfo数组</param>
        /// <returns>NamedIssueInfoCollection 的集合</returns>
        public static List<NamedIssueInfoCollection> SortIssueInfo(List<IssueInfo> issue_infos)
        {
            List<NamedIssueInfoCollection> results = new List<NamedIssueInfoCollection>();
            NamedIssueInfoCollection one = new NamedIssueInfoCollection();

            issue_infos.Sort(new IssueInfoSorter());

            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                string strSeller = info.Seller;

                if (info.Seller != one.Seller
                    && one.Count > 0)
                {
                    results.Add(one);
                    one = new NamedIssueInfoCollection();
                }

                one.Seller = info.Seller;
                one.Add(info);
            }

            if (one.Count > 0)
            {
                results.Add(one);
            }

            return results;
        }

        // 
        /// <summary>
        /// 将IssueInfo数组中已经到齐的行移除
        /// </summary>
        /// <param name="issue_infos">要处理的集合</param>
        public static void RemoveArrivedIssueInfos(ref List<IssueInfo> issue_infos)
        {
            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                if (info.MissingCount == "0")
                {
                    issue_infos.RemoveAt(i);
                    i--;
                }
            }
        }

#if NOOOOOOOOOOO
        // 将IssueInfo数组中处在指定时间范围以外的行移除
        public static void RemoveOutofTimeRangeIssueInfos(ref List<IssueInfo> issue_infos,
            TimeFilter filter)
        {
#if NO
            string strLastArrivedPublishTime = "";
            // 寻找实到1册以上的最后一期
            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                int nArrivedCount = 0;

                try
                {
                    nArrivedCount = Convert.ToInt32(info.ArrivedCount);
                }
                catch
                {
                }

                if (nArrivedCount > 0)
                {
                    string strTemp = DateTimeUtil.ForcePublishTime8(info.PublishTime);

                    if (string.Compare(strTemp, strLastArrivedPublishTime) > 0)
                        strLastArrivedPublishTime = strTemp;
                }
            }

            // 校正end，使得它表示最后一个实际到达的期的出版日期
            if (String.IsNullOrEmpty(strLastArrivedPublishTime) == false)
            {
                DateTime last = DateTimeUtil.Long8ToDateTime(strLastArrivedPublishTime);
                if (last > end)
                    end = last;
            }
#endif
            if (filter.Style == "none")
                return;

            DateTime start = filter.StartTime;
            DateTime end = filter.EndTime;

            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                string strPublishTime = DateTimeUtil.ForcePublishTime8(info.PublishTime);

                DateTime publish_time = DateTimeUtil.Long8ToDateTime(strPublishTime);

                if (publish_time < start || (publish_time > end && end != new DateTime(0)))
                {
                    issue_infos.RemoveAt(i);
                    i--;
                }
            }
        }
#endif
        /// <summary>
        /// 获得用于显示的期信息字符串
        /// </summary>
        /// <param name="info">期信息对象</param>
        /// <returns>用于显示的字符串</returns>
        public static string GetIssueString(IssueInfo info)
        {
            return info.PublishTime + " 出版的 第" + info.Issue + "期";
        }

        /// <summary>
        /// 获得时间范围字符串
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>时间范围字符串</returns>
        public static string GetRangeString(DateTime start, DateTime end)
        {
            end -= new TimeSpan(24, 0, 0);
            return start.ToShortDateString() + "-" + end.ToShortDateString();
        }

        // 
        /// <summary>
        /// 将IssueInfo数组中处在指定时间范围以外的行移除
        /// </summary>
        /// <param name="issue_infos">要处理的集合</param>
        /// <param name="filter">时间过滤器</param>
        /// <param name="strDebugInfo">返回调试信息</param>
        public static void RemoveOutofTimeRangeIssueInfos(ref List<IssueInfo> issue_infos,
            TimeFilter filter,
            out string strDebugInfo)
        {
            strDebugInfo = "";

            if (filter.Style == "none")
            {
                strDebugInfo = "不进行时间过滤";
                return;
            }

            DateTime start = filter.StartTime;
            DateTime end = filter.EndTime;

            // 寻找实到1册以上的最后一期。这是一个技巧，因为如果某期虽然超过催缺的范围，但它实际上到了，表明比这期时间还要早的期应该也到了。这样就要考虑实际的情况，而不是拘泥操作者设定的时间
            if (filter.VerifyArrivedIssue == true)
            {
                string strLastArrivedPublishTime = "";
                // 寻找实到1册以上的最后一期
                for (int i = 0; i < issue_infos.Count; i++)
                {
                    IssueInfo info = issue_infos[i];

                    int nArrivedCount = 0;

                    try
                    {
                        nArrivedCount = Convert.ToInt32(info.ArrivedCount);
                    }
                    catch
                    {
                    }

                    if (nArrivedCount > 0)
                    {
                        string strTemp = DateTimeUtil.ForcePublishTime8(info.PublishTime);

                        if (string.Compare(strTemp, strLastArrivedPublishTime) > 0)
                            strLastArrivedPublishTime = strTemp;
                    }
                }

                // 校正end，使得它表示最后一个实际到达的期的出版日期
                if (String.IsNullOrEmpty(strLastArrivedPublishTime) == false)
                {
                    DateTime last = DateTimeUtil.Long8ToDateTime(strLastArrivedPublishTime);
                    if (last > end)
                    {
                        strDebugInfo += "filter的末尾时间从 "+end.ToShortDateString()+" 修正到实际已到的最后一期 "+last.ToShortDateString()+"\r\n";
                        end = last;
                    }
                }
            }

            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                DateTime publish_time = new DateTime(0);
                string strPublishTime = info.PublishTime;
                if (string.IsNullOrEmpty(strPublishTime) == false)
                {
                    strPublishTime = DateTimeUtil.ForcePublishTime8(strPublishTime);
                    publish_time = DateTimeUtil.Long8ToDateTime(strPublishTime);
                }

                DateTime order_time = new DateTime(0);
                string strOrderTime = info.OrderTime;
                if (string.IsNullOrEmpty(strOrderTime) == false)
                {
                    try
                    {
                        order_time = DateTimeUtil.FromRfc1123DateTimeString(info.OrderTime).ToLocalTime();
                        order_time += filter.OrderTimeDelta;
                    }
                    catch (Exception ex)
                    {
                        // 2015/1/27
                        strDebugInfo += "警告: " + IssueHost.GetIssueString(info) + " 订购时间 '" + info.OrderTime + "' 格式错误: " + ex.Message + "\r\n";
                        order_time = new DateTime(0);
                    }
                }

                if (filter.Style == "publishtime")
                {
                    if (string.IsNullOrEmpty(strPublishTime) == true)
                    {
                        // 如果坚持要用出版时间，则不处理
                        strDebugInfo += IssueHost.GetIssueString(info) + " 因出版日期为空，被排除\r\n";
                        goto DO_REMOVE;
                    }
                    // 用出版时间来判断
                    if (IssueHost.IsInRange(start, end, publish_time) == false)
                    {
                        strDebugInfo += IssueHost.GetIssueString(info) + " 因出版日期 "+publish_time.ToShortDateString()+" 不在 "+IssueHost.GetRangeString(start,end)+" 范围内，被排除\r\n";
                        goto DO_REMOVE;
                    }
                    goto CONTINUE;
                }

                if (filter.Style == "ordertime")
                {
                    if (string.IsNullOrEmpty(strOrderTime) == true)
                    {
                        // 如果坚持要用订购时间，则不处理
                        strDebugInfo += IssueHost.GetIssueString(info) + " 因订购日期为空，被排除\r\n";
                        goto DO_REMOVE;
                    }
                    // 用订购时间来判断
                    if (order_time != new DateTime(0)
                        && IssueHost.IsInRange(start, end, order_time) == false)
                    {
                        strDebugInfo += IssueHost.GetIssueString(info) + " 因订购日期推测的出版日期 " + order_time.ToShortDateString() + " 不在 " + IssueHost.GetRangeString(start, end) + " 范围内，被排除\r\n";
                        goto DO_REMOVE;
                    }
                    goto CONTINUE;
                }

                if (filter.Style == "both")
                {
                    if (string.IsNullOrEmpty(strPublishTime) == false)
                    {
                        // 用出版时间来判断
                        if (IssueHost.IsInRange(start, end, publish_time) == false)
                        {
                            strDebugInfo += IssueHost.GetIssueString(info) + " 因出版日期 " + publish_time.ToShortDateString() + " 不在 " + IssueHost.GetRangeString(start, end) + " 范围内，被排除\r\n";
                            goto DO_REMOVE;
                        }
                        goto CONTINUE;
                    }

                    if (string.IsNullOrEmpty(strOrderTime) == false)
                    {
                        // 用订购时间来判断
                        if (order_time != new DateTime(0)
                            && IssueHost.IsInRange(start, end, order_time) == false)
                        {
                            strDebugInfo += IssueHost.GetIssueString(info) + " 因订购日期推测的出版日期 " + order_time.ToShortDateString() + " 不在 " + IssueHost.GetRangeString(start, end) + " 范围内，被排除\r\n";
                            goto DO_REMOVE;
                        }
                        goto CONTINUE;
                    }

                    // 两个时间都为空
                    goto DO_REMOVE;
                }

            CONTINUE:
                strDebugInfo += IssueHost.GetIssueString(info) + " 被保留\r\n";
                continue;
            DO_REMOVE:
                issue_infos.RemoveAt(i);
                i--;
            }
        }

        // 看一个时刻是否在范围内
        // start是包含的，end是不包含的
        internal static bool IsInRange(DateTime start,
            DateTime end,
            DateTime current)
        {
            if (current < start || (current >= end && end != new DateTime(0)))
                return false;
            return true;
        }

        // 
        /// <summary>
        /// 输出IssueInfo数组的调试文本
        /// </summary>
        /// <param name="issue_infos">IssueInfo数组</param>
        /// <returns>调试文本</returns>
        public static string DumpIssueInfos(List<IssueInfo> issue_infos)
        {
            string strResult = "";

            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                strResult += "PublishTime[" + info.PublishTime + "]\tIssue[" + info.Issue + "]\tSeller[" + info.Seller + "]\tOrderCount[" + info.OrderCount + "]\tArrivedCount[" + info.ArrivedCount + "]\r\n";
            }

            return strResult;
        }

        /// <summary>
        /// 获得渠道地址 XML
        /// </summary>
        /// <param name="strSeller">渠道</param>
        /// <returns>XML 字符串</returns>
        public string GetAddressXml(string strSeller)
        {
            if (this.Orders == null)
                return null;

            for (int i = 0; i < this.Orders.Count; i++)
            {
                OneOrder order = this.Orders[i];

                if (order.Seller == strSeller)
                    return order.SellerAddress;
            }

            return null;
        }
    }

    // 
    /// <summary>
    /// 具有名字的IssueInfo对象数组。名字就是渠道名
    /// </summary>
    public class NamedIssueInfoCollection : List<IssueInfo>
    {
        /// <summary>
        /// 渠道名
        /// </summary>
        public string Seller = "";
    }

    /// <summary>
    /// 期信息
    /// </summary>
    public class IssueInfo
    {
        // 
        /// <summary>
        /// 出版日期
        /// </summary>
        public string PublishTime = "";

        // 
        /// <summary>
        /// 订购日期
        /// </summary>
        public string OrderTime = "";

        // 
        /// <summary>
        /// 当年期号
        /// </summary>
        public string Issue = "";

        // 
        /// <summary>
        /// 渠道(书商)
        /// </summary>
        public string Seller = "";

        // 
        /// <summary>
        /// 订数
        /// </summary>
        public string OrderCount = "";

        // 
        /// <summary>
        /// 已到数
        /// </summary>
        public string ArrivedCount = "";

        // 
        /// <summary>
        /// 缺数
        /// </summary>
        public string MissingCount = "";
    }

    // 期对象
    /// <summary>
    /// 一个期对象
    /// </summary>
    public class OneIssue
    {
        /// <summary>
        /// 期记录的 XmlDocument
        /// </summary>
        public XmlDocument Dom = null;

        // 
        /// <summary>
        /// 是否为猜测的节点
        /// </summary>
        public bool IsGuess = false;

        /// <summary>
        /// 订购时间。RFC1123 格式
        /// </summary>
        public string OrderTime = "";   // RFC1123

        // 
        /// <summary>
        /// 和本期时间匹配的若干个订购对象
        /// </summary>
        public List<OneOrder> Orders = null;

        // 
        /// <summary>
        /// 限定关联的订购记录的参考 ID 集合
        /// </summary>
        public List<string> OrderRefIDs = new List<string>();

        // 
        // return:
        //      -1  error
        //      0   not found
        //      >0  匹配的个数
        /// <summary>
        /// 和本期时间匹配的若干个订购对象建立联系
        /// </summary>
        /// <param name="orders">订购对象集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有找到
        ///      >0  匹配的个数
        /// </returns>
        public int LinkOrders(
            List<OneOrder> orders,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            this.Orders = new List<OneOrder>();

            // 先从期记录的<orderInfo>元素下取
            List<string> XmlRecords = new List<string>();
            XmlNodeList nodes = this.Dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    OneOrder order = new OneOrder();
                    nRet = order.LoadRecord(
                        "",
                        nodes[i].OuterXml,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    if (this.OrderRefIDs.IndexOf(order.RefID) == -1)
                        continue;

                    this.Orders.Add(order);
                }

                return this.Orders.Count;
            }

            // 如果期记录中没有订购信息，再从订购记录中取
            if (orders == null || orders.Count == 0)
                return 0;

            string strPublishTime = this.PublishTime;

            for (int i = 0; i < orders.Count; i++)
            {
                XmlDocument dom = orders[i].Dom;

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                // 星号表示通配
                if (strPublishTime != "*")
                {
                    if (Global.InRange(strPublishTime, strRange) == false)
                        continue;
                }

                if (this.OrderRefIDs.Count > 0)
                {
                    if (this.OrderRefIDs.IndexOf(orders[i].RefID) == -1)
                        continue;
                }

                this.Orders.Add(orders[i]);
            }

            return this.Orders.Count;
        }

        /// <summary>
        /// 装载记录 XML
        /// </summary>
        /// <param name="strXml">XML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int LoadRecord(string strXml,
            out string strError)
        {
            strError = "";

            this.Dom = new XmlDocument();
            try
            {
                this.Dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时发生错误: " + ex.Message;
                return -1;
            }

            OrderRefIDs.Clear();
            return 0;
        }

        /// <summary>
        /// 参考 ID
        /// </summary>
        public string RefID
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "refID");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "refID",
                    value);
            }
        }

        /// <summary>
        /// 出版时间
        /// </summary>
        public string PublishTime
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "publishTime");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "publishTime",
                    value);
            }
        }

        /// <summary>
        /// 当年期号
        /// </summary>
        public string Issue
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "issue");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "issue",
                    value);
            }
        }
    }

    // 订购对象
    /// <summary>
    /// 一个订购对象
    /// </summary>
    public class OneOrder
    {
        /// <summary>
        /// 订购记录路径
        /// </summary>
        public string RecPath = ""; // 订购记录路径

        /// <summary>
        /// 订购记录 XmlComent
        /// </summary>
        public XmlDocument Dom = null;

        // parameters:
        //      strRecPath  订购记录路径
        /// <summary>
        /// 装载记录 XML
        /// </summary>
        /// <param name="strRecPath">记录路径</param>
        /// <param name="strXml">XML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int LoadRecord(
            string strRecPath,
            string strXml,
            out string strError)
        {
            strError = "";

            this.RecPath = strRecPath;

            this.Dom = new XmlDocument();
            try
            {
                this.Dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时发生错误: " + ex.Message;
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Seller
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "seller");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "seller",
                    value);
            }
        }

        /// <summary>
        /// 参考 ID
        /// </summary>
        public string RefID
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "refID");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "refID",
                    value);
            }
        }

        // 
        /// <summary>
        /// 获得预计的出版时间
        /// </summary>
        /// <param name="filter">时间过滤器</param>
        /// <param name="strPublishTime">返回出版时间</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetPublishTime(
            TimeFilter filter,
            out string strPublishTime,
            out string strError)
        {
            strError = "";
            strPublishTime = "";

            if (this.Dom == null)
                throw new Exception("dom尚未初始化");

            string strValue = DomUtil.GetElementText(this.Dom.DocumentElement,
"range");
            if (string.IsNullOrEmpty(strValue) == false)
            {
                int nRet = strValue.IndexOf("-");
                if (nRet == -1)
                {
                    if (strValue.Length == 8)
                    {
                        strPublishTime = strValue;
                        return 0;
                    }
                }
                else
                {
                    string strLeft = strValue.Substring(0, nRet).Trim();
                    // 取后一个时间点
                    strValue = strValue.Substring(nRet + 1);
                    if (strValue.Length == 8)
                    {
                        strPublishTime = strValue;
                        return 0;
                    }
                    if (string.IsNullOrEmpty(strValue) == true)
                    {
                        // 2012/9/1
                        // 右端时间为空
                        strValue = strLeft; // 采用左端时间
                        if (strValue.Length == 8)
                        {
                            strPublishTime = strValue;
                            return 0;
                        }
                    }
                }

                // 格式错误
                strError = "<range>值 '" + strValue + "' 格式错误";
                return -1;
            }

#if NO
            strValue = this.OrderTime;
            //strValue = DomUtil.GetElementText(this.Dom.DocumentElement,
            //    "orderTime");
            if (string.IsNullOrEmpty(strValue) == true)
            {
                strError = "订购记录中尚未写入出版时间<orderTime>内容。这通常是因为没有经过打印订单步骤造成的。";
                return -1;
            }
            DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strValue).ToLocalTime();
            // TODO: 将来这里可以用脚本来计算出出版时间
            time += filter.OrderTimeDelta;
            strPublishTime = DateTimeUtil.DateTimeToString8(time);
#endif
            return 0;
        }
    
        /// <summary>
        /// 时间范围
        /// </summary>
        public string Range
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
    "range");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "range",
                    value);
            }
        }

        /// <summary>
        /// 订购时间
        /// </summary>
        public string OrderTime
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "orderTime");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "orderTime",
                    value);
            }
        }

        /// <summary>
        /// 复本数
        /// </summary>
        public string Copy
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                return DomUtil.GetElementText(this.Dom.DocumentElement,
                    "copy");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementText(this.Dom.DocumentElement,
                    "copy",
                    value);
            }
        }

        /// <summary>
        /// 订购复本数
        /// </summary>
        public int OldCopyValue
        {
            get
            {
                string strOldValue = "";
                string strNewValue = "";
                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(this.Copy,
                    out strOldValue,
                    out strNewValue);

                // 可能有乘号
                string strLeftCopy = OrderDesignControl.GetCopyFromCopyString(strOldValue);
                string strRightCopy = OrderDesignControl.GetRightFromCopyString(strOldValue);

                try
                {
                    return Convert.ToInt32(strLeftCopy);
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 验收复本数
        /// </summary>
        public int NewCopyValue
        {
            get
            {
                string strOldValue = "";
                string strNewValue = "";
                // 分离 "old[new]" 内的两个值
                OrderDesignControl.ParseOldNewValue(this.Copy,
                    out strOldValue,
                    out strNewValue);

                // 可能有乘号
                string strLeftCopy = OrderDesignControl.GetCopyFromCopyString(strNewValue);
                string strRightCopy = OrderDesignControl.GetRightFromCopyString(strNewValue);

                try
                {
                    return Convert.ToInt32(strLeftCopy);
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 渠道地址
        /// </summary>
        public string SellerAddress
        {
            get
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                // 2009/9/17 changed
                return DomUtil.GetElementInnerXml(this.Dom.DocumentElement,
                    "sellerAddress");
            }
            set
            {
                if (this.Dom == null)
                    throw new Exception("dom尚未初始化");

                DomUtil.SetElementInnerXml(this.Dom.DocumentElement,
                    "sellerAddress",
                    value);
            }
        }
    }

    // 将订购信息对象按照书商名称排序
    internal class OrderSorter : IComparer<OneOrder>
    {
        int IComparer<OneOrder>.Compare(OneOrder x, OneOrder y)
        {
            return string.Compare(x.Seller, y.Seller);
        }
    }

    // 将期信息对象按照书商名称、出版年份+期号排序
    internal class IssueInfoSorter : IComparer<IssueInfo>
    {
        int IComparer<IssueInfo>.Compare(IssueInfo x, IssueInfo y)
        {
            int nRet = string.Compare(x.Seller, y.Seller);
            if (nRet != 0)
                return nRet;

            // 2012/9/1
            {
                string strXYearPart = IssueUtil.GetYearPart(x.PublishTime);
                string strYYearPart = IssueUtil.GetYearPart(y.PublishTime);

                /*
                int nMaxWidth = Math.Max(x.Issue.Length, y.Issue.Length);
                string strXIssue = x.Issue.PadLeft(nMaxWidth, '0');
                string strYIssue = y.Issue.PadLeft(nMaxWidth, '0');
                 * */
                string strXIssue = x.Issue.Trim();
                string strYIssue = y.Issue.Trim();
                OneIssueComparer.FixingWidth(ref strXIssue, ref strYIssue);

                nRet = string.Compare(strXYearPart + "!" + strXIssue, strYYearPart + "!" + strYIssue);
                if (nRet != 0)
                    return nRet;
            }

            return string.Compare(x.PublishTime, y.PublishTime);
        }
    }

    // 比较出版年份+期号。小的在前
    internal class OneIssueComparer : IComparer<OneIssue>
    {
        // 2012/10/12
        // 把期号规整为固定宽度。这之前还要从“3/4/5”特殊形态中把"3"取出
        public static void FixingWidth(ref string strIssue1,
            ref string strIssue2)
        {
            int nRet = strIssue1.IndexOf("/");
            if (nRet != -1)
                strIssue1 = strIssue1.Substring(0, nRet).Trim();
            nRet = strIssue2.IndexOf("/");
            if (nRet != -1)
                strIssue2 = strIssue2.Substring(0, nRet).Trim();

            int nMaxWidth = Math.Max(strIssue1.Length, strIssue2.Length);
            strIssue1 = strIssue1.PadLeft(nMaxWidth, '0');
            strIssue2 = strIssue2.PadLeft(nMaxWidth, '0');
        }

        int IComparer<OneIssue>.Compare(OneIssue x, OneIssue y)
        {
            {
                string strXYearPart = IssueUtil.GetYearPart(x.PublishTime);
                string strYYearPart = IssueUtil.GetYearPart(y.PublishTime);

                /*
                int nMaxWidth = Math.Max(x.Issue.Length, y.Issue.Length);
                string strXIssue = x.Issue.PadLeft(nMaxWidth, '0');
                string strYIssue = y.Issue.PadLeft(nMaxWidth, '0');
                 * */
                string strXIssue = x.Issue.Trim();
                string strYIssue = y.Issue.Trim();
                FixingWidth(ref strXIssue, ref strYIssue);

                int nRet = string.Compare(strXYearPart + "!" + strXIssue, strYYearPart + "!" + strYIssue);
                if (nRet != 0)
                    return nRet;
            }

            return string.Compare(x.PublishTime, y.PublishTime);
        }
#if NO
        int IComparer<OneIssue>.Compare(OneIssue x, OneIssue y)
        {
            string s1 = x.PublishTime;
            string s2 = y.PublishTime;

            int nRet = String.Compare(s1, s2);
            if (nRet == 0)
            {
                int nMaxWidth = Math.Max(x.Issue.Length, y.Issue.Length);
                string strXIssue = x.Issue.PadLeft(nMaxWidth, '0');
                string strYIssue = y.Issue.PadLeft(nMaxWidth, '0');

                return String.Compare(strXIssue, strYIssue);
            }

            return nRet;
        }
#endif

    }
}
