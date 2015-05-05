using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 打印图书催缺单操作所需要的 处理订购信息的宿主类
    /// </summary>
    public class BookHost
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


        // 订购对象数组
        List<OneOrder> Orders = new List<OneOrder>();

        /// <summary>
        /// 清除全部订购对象
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

        // 2012/8/31
        // 
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 装入订购记录
        /// </summary>
        /// <param name="strOrderRecPath">订购记录路径</param>
        /// <param name="strRefID">返回参考 ID</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有装载; 1: 已经装载</returns>
        public int LoadOrderRecord(string strOrderRecPath,
            out string strRefID,
            out string strError)
        {
            strError = "";
            strRefID = "";

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
            if (lRet == 0 || lRet == -1)
                return -1;

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

            string strState = DomUtil.GetElementText(order.Dom.DocumentElement, "state");
            if (strState == "已订购" || strState == "已验收")
            {
            }
            else
            {
                strError = "订购记录 '" + strOutputOrderRecPath + "' 的状态不是 已订购 或 已验收，被跳过";
                return 0;
            }


            this.Orders.Add(order);

            strRefID = DomUtil.GetElementText(order.Dom.DocumentElement, "refID");
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

#if NO
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
#endif

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

#if NO
        // 创建每个期对象
        // return:
        //      -1  error
        //      0   无法获得订购时间范围
        //      1   成功
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
                    strError = ex.Message;
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
                        strNextPublishTime = NextPublishTime(strCurrentPublishTime,
                             nIssueCount);
                    }
                    catch (Exception ex)
                    {
                        // 2009/2/8 new add
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
                        strError = ex.Message;
                        return -1;
                    }


                    // 号码自动增量需要知道一个期是否跨年，可以通过查询采购信息得到一年所订阅的期数
                    if (nCurrentIssue >= nIssueCount)
                    {
                        // 跨年了
                        // strNextIssue = "1";
                        nNextIssue = 1;
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
                    && issue.Issue == strIssue)
                {
                    return i;
                }
            }

            return -1;
        }

#endif

#if NO
        public static string ForcePublishTime8(string strPublishTime)
        {
            strPublishTime = CannonicalizePublishTimeString(strPublishTime);
            if (strPublishTime.Length > 8)
                strPublishTime = strPublishTime.Substring(0, 8);

            return strPublishTime;
        }

        // 规范化8字符的日期字符串
        static string CannonicalizePublishTimeString(string strText)
        {
            if (strText.Length == 4)
            {
                strText = strText + "0101";
                goto END1;
            }

            if (strText.Length == 6)
            {
                strText = strText + "01";
                goto END1;
            }
            if (strText.Length == 8)
                goto END1;
            if (strText.Length == 10)
                goto END1;

            throw new Exception("出版日期字符串 '" + strText + "' 格式不正确");

        END1:
            // 检查一下时间字符串是否属于存在的时间
            string strTest = strText.Substring(0, 8);

            try
            {
                DateTimeUtil.Long8ToDateTime(strTest);
            }
            catch (System.ArgumentOutOfRangeException /*ex*/)
            {
                throw new Exception("日期字符串 '" + strText + "' 不正确: 出现了不可能的年、月、日值");
            }
            catch (Exception ex)
            {
                throw new Exception("日期字符串 '" + strText + "' 不正确: " + ex.Message);
            }

            return strText;
        }
#endif

#if NO
        // 移动出版时间
        // 可能会抛出异常
        static void MovePublishTime(List<OneIssue> issues,
            List<int> indices,
            TimeSpan delta)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                int index = indices[i];
                OneIssue issue = issues[index];

                string strRest = "";
                string strPublishTime = issue.PublishTime;

                strPublishTime = DateTimeUtil.CannonicalizePublishTimeString(strPublishTime);

                if (strPublishTime.Length > 8)
                {
                    strRest = strPublishTime.Substring(8);
                    strPublishTime = strPublishTime.Substring(0, 8);
                }

                DateTime time = DateTimeUtil.Long8ToDateTime(strPublishTime);
                time = time + delta;
                strPublishTime = DateTimeUtil.DateTimeToString8(time);
                strPublishTime += strRest;

                issue.PublishTime = strPublishTime;
            }
        }
#endif

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
                string strCurrentYear = current_issue.PublishTime.Substring(0, 4);
                string strCurrentIssue = current_issue.Issue.PadLeft(3, '0');

                if (String.Compare(strYear, strLastYear) >= 0 && string.Compare(strIssue, strLastIssue) > 0
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
        // TODO: 需要完成
        string DumpOrder()
        {
            string strResult = "";
            for (int i = 0; i < this.Orders.Count; i++)
            {
                OneOrder issue = this.Orders[i];

                // strResult += "publish_time [" + issue.Order.PublishTime + "] issue[" + issue.Issue + "] is_guess[" + issue.IsGuess.ToString() + "]\r\n";
            }

            return strResult;
        }

#if NO
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
                        AddIssueByIssueNo(guess_issue);
                        guess_issue.IsGuess = true;
                    }
                    else
                    {
                        OneIssue found = this.Issues[index];
                        string strRealPublishTime = found.PublishTime;
                        string strGuessPublishTime = guess_issue.PublishTime;

                        strRealPublishTime = CannonicalizePublishTimeString(strRealPublishTime);
                        strGuessPublishTime = CannonicalizePublishTimeString(strGuessPublishTime);


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

                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }
#endif 

        // 
        // return:
        //      -1  error
        //      0   没有订购信息，或者(因为订购记录状态不符合要求)被跳过
        //      1   初始化成功
        /// <summary>
        /// 初始化控件
        /// </summary>
        /// <param name="strOrderRecPath">订购记录路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        ///      -1  error
        ///      0   没有订购信息，或者(因为订购记录状态不符合要求)被跳过
        ///      1   初始化成功
        /// </returns>
        public int Initial(string strOrderRecPath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 装入一条订购记录
            string strRefID = "";
            nRet = this.LoadOrderRecord(strOrderRecPath,
                out strRefID,
                out strError);
            if (nRet == -1)
            {
                strError = "在 OrderHost 中装入订购记录 " + strOrderRecPath + " 时发生错误: " + strError;
                return -1;
            }

            if (nRet == 0)
                return 0;
#if NO
            nRet = this.LoadOrderRecords(strOrderRecPath,
                out strError);
            if (nRet == -1)
            {
                strError = "在OrderHost中装入书目记录 " + strOrderRecPath + " 的下属订购记录时发生错误: " + strError;
                return -1;
            }
#endif

            return 1;
        }

        // 获得期各种信息
        // 每期一行，按照书商名进行了汇总
        // TODO: “直订”等需要特殊处理
        // return:
        //      -1  error
        //      0   没有任何信息
        //      >0  信息个数
        /// <summary>
        /// 获得期各种信息
        /// </summary>
        /// <param name="filter">时间过滤器</param>
        /// <param name="issue_infos">返回期信息集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <param name="strWarning">返回警告信息</param>
        /// <returns>
        ///      -1  出错
        ///      0   没有任何信息
        ///      >0  信息个数
        /// </returns>
        public int GetOrderInfo(
            TimeFilter filter,
            out List<IssueInfo> issue_infos,
            out string strError,
            out string strWarning)
        {
            strError = "";
            strWarning = "";

            issue_infos = new List<IssueInfo>();

            string strLastSeller = "";
            int nOrderCount = 0;
            int nArriveCount = 0;
            OneOrder order = null;

            for (int j = 0; j < this.Orders.Count; j++)
            {
                order = this.Orders[j];

                string strCurrentSeller = order.Seller;
                if (strCurrentSeller != strLastSeller
                    && strLastSeller != "")
                {
                    // 将上一段汇总值推出
                    IssueInfo info = new IssueInfo();

                    // 获得预计的出版时间
                    string strPublishTime = "";
                    int nRet = order.GetPublishTime(
                        filter,
                        out strPublishTime,
            out strError);
                    if (nRet == -1)
                    {
                        strWarning += "获得(订购记录路径 '" + order.RecPath + "')预计出版日期时出错: " + strError + "\r\n";
                        continue;
                    }

                    info.PublishTime = strPublishTime;   // 预计出版时间
                    info.OrderTime = order.OrderTime;   // 2012/8/31

                    // info.Issue = order.Issue;
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
            if ((nOrderCount != 0 || nArriveCount != 0)
                && strLastSeller != "")
            {
                IssueInfo info = new IssueInfo();

                // 获得预计的出版时间
                string strPublishTime = "";
                int nRet = order.GetPublishTime(
                    filter,
                    out strPublishTime,
        out strError);
                if (nRet == -1)
                {
                    strWarning += "获得(订购记录路径 '"+order.RecPath+"')预计出版日期时出错: " + strError + "\r\n";
                    goto END1;
                }

                info.PublishTime = strPublishTime;
                info.OrderTime = order.OrderTime;   // 2012/8/31
                // info.Issue = order.Issue;
                info.Seller = strLastSeller;
                info.OrderCount = nOrderCount.ToString();
                info.ArrivedCount = nArriveCount.ToString();
                info.MissingCount = Math.Max(0, nOrderCount - nArriveCount).ToString();
                issue_infos.Add(info);
            }

            END1:

            return issue_infos.Count;
        }

        // 
        /// <summary>
        /// 将IssueInfo数组排序后按照书商名拆分为独立的数组
        /// </summary>
        /// <param name="order_infos">IssueInfo数组</param>
        /// <returns>NamedIssueInfoCollection 的集合</returns>
        public static List<NamedIssueInfoCollection> SortOrderInfo(List<IssueInfo> order_infos)
        {
            List<NamedIssueInfoCollection> results = new List<NamedIssueInfoCollection>();
            NamedIssueInfoCollection one = new NamedIssueInfoCollection();

            order_infos.Sort(new IssueInfoSorter());

            for (int i = 0; i < order_infos.Count; i++)
            {
                IssueInfo info = order_infos[i];

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
        public static void RemoveArrivedOrderInfos(ref List<IssueInfo> issue_infos)
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

#if NO
        // 将IssueInfo数组中处在指定时间范围以外的行移除
        public static void RemoveOutofTimeRangeOrderInfos(ref List<IssueInfo> issue_infos,
            TimeFilter filter)
        {
            // 不过滤
            if (filter.Style == "none")
                return;
            string strLastArrivedPublishTime = "";
            DateTime start = filter.StartTime;
            DateTime end = filter.EndTime;

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

        // 
        /// <summary>
        /// 将IssueInfo数组中处在指定时间范围以外的行移除
        /// </summary>
        /// <param name="issue_infos">要处理的集合</param>
        /// <param name="filter">时间过滤器</param>
        /// <param name="strDebugInfo">返回调试信息</param>
        public static void RemoveOutofTimeRangeOrderInfos(ref List<IssueInfo> issue_infos,
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

#if NO
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
#endif

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
                    order_time = DateTimeUtil.FromRfc1123DateTimeString(info.OrderTime).ToLocalTime();
                    order_time += filter.OrderTimeDelta;
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
                    if (IssueHost.IsInRange(start, end, order_time) == false)
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
                        if (IssueHost.IsInRange(start, end, order_time) == false)
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

        // 
        /// <summary>
        /// 输出IssueInfo数组的调试文本
        /// </summary>
        /// <param name="issue_infos">IssueInfo数组</param>
        /// <returns>调试文本</returns>
        public static string DumpOrderInfos(List<IssueInfo> issue_infos)
        {
            string strResult = "";

            for (int i = 0; i < issue_infos.Count; i++)
            {
                IssueInfo info = issue_infos[i];

                string strOrderTime = "";
                if (string.IsNullOrEmpty(info.OrderTime) == false)
                {
                    DateTime order_time = DateTimeUtil.FromRfc1123DateTimeString(info.OrderTime).ToLocalTime();
                    strOrderTime = order_time.ToShortDateString();
                }


                strResult += "PublishTime[" + info.PublishTime + "]\tIssue[" + info.Issue + "]\tOrderTime[" + strOrderTime + "]\tSeller[" + info.Seller + "]\tOrderCount[" + info.OrderCount + "]\tArrivedCount[" + info.ArrivedCount + "]\r\n";
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

#if NO
    // 具有名字的OrderInfo对象数组。名字就是Seller名
    public class NamedOrderInfoCollection : List<OrderInfo>
    {
        public string Seller = "";
    }
    
public class OrderInfo
    {
        // 出版日期
        public string PublishTime = "";

        // 当年期号
        public string Issue = "";

        // 书商
        public string Seller = "";

        // 订数
        public string OrderCount = "";

        // 已到数
        public string ArrivedCount = "";

        // 缺数
        public string MissingCount = "";
    }

    // 将期信息对象按照书商名称、出版日期排序
    public class OrderInfoSorter : IComparer<OrderInfo>
    {
        int IComparer<OrderInfo>.Compare(OrderInfo x, OrderInfo y)
        {
            int nRet = string.Compare(x.Seller, y.Seller);
            if (nRet != 0)
                return nRet;

            return string.Compare(x.PublishTime, y.PublishTime);
        }
    }
#endif

}
