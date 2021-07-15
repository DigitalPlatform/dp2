using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;

using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

namespace DigitalPlatform.LibraryServer
{
    public class LibraryServerUtil
    {
        public static string STATE_EXTENSION = ".~state";

        // 本地文件目录的虚拟前缀字符串
        public static string LOCAL_PREFIX = "!";

        // parameters:
        //      strStyle    skip_check_overdue 跳过检查超期
        // return:
        //      -1  检查过程出错
        //      0   状态不正常
        //      1   状态正常
        public static int CheckPatronState(XmlDocument readerdom,
            string strStyle,
            out string strError)
        {
            // 检查借阅证是否超期，是否有挂失等状态
            // return:
            //      -1  检测过程发生了错误。应当作不能借阅来处理
            //      0   可以借阅
            //      1   证已经过了失效期，不能借阅
            //      2   证有不让借阅的状态
            int nRet = CheckReaderExpireAndState(readerdom,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet != 0)
                return 0;

            strError = "";

            bool skip_check = StringUtil.IsInList("skip_check_overdue", strStyle);

            // 注: 长期断网运行，要跳过检查 overdue 元素
            {
                List<string> errors = new List<string>();
                // 检查是否已经有记载了的<overdue>字段
                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
                if (nodes.Count > 0)
                {
                    // text-level: 用户提示
                    strError = $"您当前有 {nodes.Count} 个违约记录尚未处理";
                    errors.Add(strError);
                }

                {
                    // 检查当前是否有潜在的超期册
                    // return:
                    //      -1  error
                    //      0   没有超期册
                    //      1   有超期册
                    nRet = CheckOverdue(
                        readerdom,
                        out List<string> overdue_infos,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        errors.Add(strError);
                    }
                }

                if (errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(errors, "; ");
                    if (skip_check == false)
                        return 0;
                }
                else
                    strError = "";
            }

            return 1;
        }

        static string ToLocalTime(string strRfc1123, string strFormat)
        {
            try
            {
                return DateTimeUtil.Rfc1123DateTimeStringToLocal(strRfc1123, strFormat);
            }
            catch (Exception ex)
            {
                throw new Exception("时间字符串 '" + strRfc1123 + "' 格式不正确: " + ex.Message);
            }
        }

        // 检查当前是否有潜在的超期册
        // return:
        //      -1  error
        //      0   没有超期册
        //      1   有超期册
        public static int CheckOverdue(XmlDocument readerdom,
            out List<string> overdue_infos,
            out string strError)
        {
            overdue_infos = new List<string>();
            strError = "";

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            if (nodes.Count == 0)
                return 0;

            List<string> errors = new List<string>();

            foreach (XmlElement borrow in nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                try
                {
                    string strBorrowDate = ToLocalTime(borrow.GetAttribute("borrowDate"), "yyyy-MM-dd HH:mm");
                    // string strReturningDate = ToLocalTime(borrow.GetAttribute("returningDate"), "yyyy-MM-dd");
                    string strRecPath = borrow.GetAttribute("recPath");

                    string strPeriod = borrow.GetAttribute("borrowPeriod");
                    string strRfc1123String = borrow.GetAttribute("returningDate");

                    if (string.IsNullOrEmpty(strRfc1123String) == false)
                    {
                        string strUnit = "day";
                        if (strPeriod.IndexOf("hour") != -1)
                            strUnit = "hour";

                        DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strRfc1123String);
                        TimeSpan delta = RoundTime(strUnit, DateTime.Now) - RoundTime(strUnit, time.ToLocalTime());
                        if (strUnit == "hour")
                        {
                            // TODO: 如果没有册条码号则用 refID 代替
                            if (delta.Hours > 0)
                                overdue_infos.Add($"册 {strItemBarcode} 已超期 {delta.Hours} 小时");
                        }
                        else
                        {
                            if (delta.Days > 0)
                                overdue_infos.Add($"册 {strItemBarcode} 已超期 {delta.Days} 天");
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"册 {strItemBarcode} 出现异常: {ex.Message}");
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }

            if (overdue_infos.Count > 0)
            {
                strError = $"有 {overdue_infos.Count} 个在借册已经超期";
                return 1;
            }

            return 0;
        }

        // 注意 time 中的时间应该是本地时间
        public static DateTime RoundTime(string strUnit,
        DateTime time)
        {
            if (strUnit == "day" || string.IsNullOrEmpty(strUnit) == true)
            {
                return new DateTime(time.Year, time.Month, time.Day,
                    12, 0, 0, 0);
            }
            else if (strUnit == "hour")
            {
                return new DateTime(time.Year, time.Month, time.Day,
                    time.Hour, 0, 0, 0);
            }
            else
            {
                throw new ArgumentException("未知的时间单位 '" + strUnit + "'");
            }
        }

        // 检查借阅证是否超期，是否有挂失等状态
        // text-level: 用户提示 OPAC预约功能要调用此函数
        // return:
        //      -1  检测过程发生了错误。应当作不能借阅来处理
        //      0   可以借阅
        //      1   证已经过了失效期，不能借阅
        //      2   证有不让借阅的状态
        public static int CheckReaderExpireAndState(XmlDocument readerdom,
            out string strError)
        {
            strError = "";

            string strExpireDate = DomUtil.GetElementText(readerdom.DocumentElement, "expireDate");
            if (String.IsNullOrEmpty(strExpireDate) == false)
            {
                DateTime expireDate;
                try
                {
                    expireDate = DateTimeUtil.FromRfc1123DateTimeString(strExpireDate);
                }
                catch
                {
                    // text-level: 内部错误
                    strError = $"借阅证失效期 (expireDate 元素) 值 '{strExpireDate}' 格式错误";
                    // "借阅证失效期<expireDate>值 '" + strExpireDate + "' 格式错误";
                    return -1;
                }

                DateTime now = DateTime.UtcNow;

                if (expireDate <= now)
                {
                    // text-level: 用户提示
                    strError = string.Format("今天({0})已经超过借阅证失效期({1})。",
                        now.ToLocalTime().ToLongDateString(),
                        expireDate.ToLocalTime().ToLongDateString());
                    return 1;
                }
            }

            string strState = DomUtil.GetElementText(readerdom.DocumentElement, "state");
            if (String.IsNullOrEmpty(strState) == false)
            {
                // text-level: 用户提示
                strError = string.Format("借阅证的状态为 '{0}'。",
                    strState);
                return 2;
            }

            return 0;
        }

        // 从读者记录 email 元素值中获得 email 地址部分
        public static string GetEmailAddress(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return "";

            // 注: email 元素内容，现在是存储 email 和微信号等多种绑定途径 2016/4/16
            // return:
            //      null    没有找到前缀
            //      ""      找到了前缀，并且值部分为空
            //      其他     返回值部分
            string strReaderEmailAddress = StringUtil.GetParameterByPrefix(strValue,
    "email",
    ":");
            // 读者记录中没有email地址，就无法进行email方式的通知了
            if (String.IsNullOrEmpty(strReaderEmailAddress) == true)
            {
                // 按照以前的 xxxx@xxxx 方式探索一下
                if (strValue.IndexOf(":") != -1 || strValue.IndexOf("@") == -1)
                    return "";
                return strValue;
            }

            return strReaderEmailAddress;
        }

        public static string GetLibraryXmlUid(XmlDocument dom)
        {
            if (dom.DocumentElement == null)
                return null;

            return dom.DocumentElement.GetAttribute("uid");
        }

        public static double GetLibraryXmlVersion(XmlDocument dom)
        {
            // 找到<version>元素
            XmlNode nodeVersion = dom.DocumentElement.SelectSingleNode("version");
            if (nodeVersion != null)
            {
                string strVersion = nodeVersion.InnerText;
                if (String.IsNullOrEmpty(strVersion) == true)
                    strVersion = "0.01";

                double version = 0.01;
                try
                {
                    version = Convert.ToDouble(strVersion);
                }
                catch
                {
                    version = 0.01;
                }

                return version;
            }

            return 0.01;
        }

        // 升级 library.xml 中的用户账户相关信息
        // 文件格式 v2.00(或以下)到v2.01
        // accounts/account 中 password 存储方式改变
        // parameters:
        //      strEncryptKey   原来版本中用到的加密 key 字符串
        public static int UpgradeLibraryXmlUserInfo(
            string strEncryptKey,
            ref XmlDocument dom,
            out string strError)
        {
            strError = "";

            XmlNodeList users = dom.DocumentElement.SelectNodes("accounts/account");
            foreach (XmlElement user in users)
            {
                string strExistPassword = user.GetAttribute("password");
                if (String.IsNullOrEmpty(strExistPassword) == false)
                {
                    string strPlainText = "";
                    try
                    {
                        strPlainText = Cryptography.Decrypt(strExistPassword,
                            strEncryptKey);
                    }
                    catch
                    {
                        strError = "已经存在的旧版(加密后)密码格式不正确";
                        return -1;
                    }

                    string strHashed = "";
                    int nRet = SetUserPassword(strPlainText, out strHashed, out strError);
                    if (nRet == -1)
                    {
                        strError = "SetUserPassword() error: " + strError;
                        return -1;
                    }
                    // user.SetAttribute("password", strHashed);
                    // 2021/7/14
                    SetPasswordValue(user, strHashed);
                }
            }

            return 0;
        }

        // 2021/6/29
        public static void SetPasswordValue(XmlElement account, string password_text)
        {
            XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
            if (password_element == null)
            {
                password_element = account.OwnerDocument.CreateElement("password");
                password_element = account.AppendChild(password_element) as XmlElement;
            }
            password_element.InnerText = password_text;
        }

        // 2015/5/20 新的密码存储策略
        // 验证密码
        // return:
        //      -1  出错
        //      0   不匹配
        //      1   匹配
        public static int MatchUserPassword(
            string strPassword,
            string strHashed,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            // 允许明文空密码
            if (String.IsNullOrEmpty(strHashed) == true)
            {
                if (strPassword != strHashed)
                {
                    strError = "密码不正确";
                    return 0;
                }

                return 1;
            }

            try
            {
                strPassword = Cryptography.GetSHA1(strPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            if (strPassword != strHashed)
            {
                strError = "密码不正确";
                return 0;
            }

            return 1;
        }

        // 2015/5/20 新的密码存储策略
        // 准备用于存储的密码
        // return:
        //      -1  出错
        //      0   成功
        public static int SetUserPassword(
            string strNewPassword,
            out string strHashed,
            out string strError)
        {
            strError = "";
            strHashed = "";

            try
            {
                strHashed = Cryptography.GetSHA1(strNewPassword);
            }
            catch
            {
                strError = "内部错误";
                return -1;
            }

            return 0;
        }

        // 兼容原来用法
        public static int CheckPublishTimeRange(string strText,
    out string strError)
        {
            return CheckPublishTimeRange(strText,
    false,
    out strError);
        }

        // 检查出版时间范围字符串是否合法
        // 如果使用单个出版时间来调用本函数，也是可以的
        // parameters:
        //      bAllowOpen  是否允许开放式的时间范围？所谓开放式的就是： "-" "-20170101" "20170101-"
        // return:
        //      -1  出错
        //      0   正确
        public static int CheckPublishTimeRange(string strText,
            bool bAllowOpen,
            out string strError)
        {
            strError = "";

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                if (bAllowOpen && string.IsNullOrEmpty(strText))
                    return 0;
                return CheckSinglePublishTime(strText,
            out strError);
            }

            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strText, "-", out strLeft, out strRight);
            if (bAllowOpen && string.IsNullOrEmpty(strLeft))
            {

            }
            else
            {
                nRet = CheckSinglePublishTime(strLeft,
                    out strError);
                if (nRet == -1)
                {
                    strError = "出版时间字符串 '" + strText + "' 的起始时间部分 '" + strLeft + "' 格式错误: " + strError;
                    return -1;
                }
            }

            if (bAllowOpen && string.IsNullOrEmpty(strRight))
            {

            }
            else
            {
                nRet = CheckSinglePublishTime(strRight,
                    out strError);
                if (nRet == -1)
                {
                    strError = "出版时间字符串 '" + strText + "' 的结束时间部分 '" + strRight + "' 格式错误: " + strError;
                    return -1;
                }
            }

            return 0;
        }

        // 检查单个出版时间字符串是否合法
        // return:
        //      -1  出错
        //      0   正确
        public static int CheckSinglePublishTime(string strText,
            out string strError)
        {
            strError = "";
            // 检查出版时间格式是否正确
            /*
            if (strText.Length != 4
                && strText.Length != 6
                && strText.Length != 8)
            {
                strError = "出版时间 '" + strText + "' 格式错误。应当为4 6 8 个数字字符";
                return -1;
            }
             * */
            if (strText.Length != 8)
            {
                strError = "出版时间 '" + strText + "' 格式错误。应当为8个数字字符";
                return -1;
            }

            if (StringUtil.IsPureNumber(strText) == false)
            {
                strError = "出版时间 '" + strText + "' 格式错误。必须为纯数字字符";
                return -1;
            }

            if (strText.Length == 8)
            {
                try
                {
                    DateTime now = DateTimeUtil.Long8ToDateTime(strText);
                }
                catch (Exception ex)
                {
                    strError = "出版时间 '" + strText + "' 格式错误: " + ex.Message;
                    return -1;
                }
            }

            return 0;
        }

        /// <summary>
        /// 匹配馆藏地点定义
        /// </summary>
        /// <param name="strLocation">馆藏地点</param>
        /// <param name="strPattern">匹配模式。例如 "海淀分馆/*"</param>
        /// <returns>true 表示匹配上； false 表示没有匹配上</returns>
        public static bool MatchLocationName(string strLocation, string strPattern)
        {
            // 如果没有通配符，则要求完全一致
            if (strPattern.IndexOf("*") == -1)
                return strLocation == strPattern;

            strPattern = strPattern.Replace("*", ".*");
            if (StringUtil.RegexCompare(strPattern,
                RegexOptions.None,
                strLocation) == true)
                return true;
            return false;
        }

        // parameters:
        //      strItemBarcodeParam 册条码号。可以使用 @refID: 前缀
        public static int ReturnChangeReaderAndItemRecord(
            string strAction,
            string strItemBarcodeParam,
            string strReaderBarcode,
            XmlDocument domLog,
            string strRecoverComment,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            out string strError)
        {
            strError = "";


            if (strAction != "return" && strAction != "lost")
            {
                strError = "ReturnChangeReaderAndItemRecord() 只能处理 strAction 为 'return' 和 'lost' 的情况，不能处理 '" + strAction + "'";
                return -1;
            }

            string strReturnOperator = DomUtil.GetElementText(domLog.DocumentElement,
    "operator");

            string strOperTime = DomUtil.GetElementText(domLog.DocumentElement,
    "operTime");

            // *** 修改读者记录
            string strDeletedBorrowFrag = "";
            XmlNode dup_reader_history = null;

            // 既然日志记录中记载的是 @refID: 的形态，那读者记录中 borrows 里面势必记载的也是这个形态
            XmlNode nodeBorrow = readerdom.DocumentElement.SelectSingleNode(
                "borrows/borrow[@barcode='" + strItemBarcodeParam + "']");
            if (nodeBorrow != null)
            {
                if (String.IsNullOrEmpty(strRecoverComment) == false)
                {
                    string strText = strRecoverComment;
                    string strOldRecoverComment = DomUtil.GetAttr(nodeBorrow, "recoverComment");
                    if (String.IsNullOrEmpty(strOldRecoverComment) == false)
                        strText = "(借阅时原注: " + strOldRecoverComment + ") " + strRecoverComment;
                    DomUtil.SetAttr(nodeBorrow, "recoverComment", strText);
                }
                strDeletedBorrowFrag = nodeBorrow.OuterXml;
                nodeBorrow.ParentNode.RemoveChild(nodeBorrow);

                // 获得几个查重需要的参数
                XmlDocument temp = new XmlDocument();
                temp.LoadXml(strDeletedBorrowFrag);
                string strItemBarcode = temp.DocumentElement.GetAttribute("barcode");
                string strBorrowDate = temp.DocumentElement.GetAttribute("borrowDate");
                string strBorrowPeriod = temp.DocumentElement.GetAttribute("borrowPeriod");

                dup_reader_history = readerdom.DocumentElement.SelectSingleNode("borrowHistory/borrow[@barcode='" + strItemBarcode + "' and @borrowDate='" + strBorrowDate + "' and @borrowPeriod='" + strBorrowPeriod + "']");
            }

            // 加入到读者记录借阅历史字段中

            if (string.IsNullOrEmpty(strDeletedBorrowFrag) == false
                && dup_reader_history == null)
            {
                // 看看根下面是否有 borrowHistory 元素
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrowHistory");
                if (root == null)
                {
                    root = readerdom.CreateElement("borrowHistory");
                    readerdom.DocumentElement.AppendChild(root);
                }

                XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                fragment.InnerXml = strDeletedBorrowFrag;

                // 插入到最前面
                XmlNode temp = DomUtil.InsertFirstChild(root, fragment);
                // 2007/6/19
                if (temp != null)
                {
                    // returnDate 加入还书时间
                    DomUtil.SetAttr(temp, "returnDate", strOperTime);

                    // borrowOperator
                    string strBorrowOperator = DomUtil.GetAttr(temp, "operator");
                    // 把原来的operator属性值复制到borrowOperator属性中
                    DomUtil.SetAttr(temp, "borrowOperator", strBorrowOperator);


                    // operator 此时需要表示还书操作者了
                    DomUtil.SetAttr(temp, "operator", strReturnOperator);

                }
                // 如果超过100个，则删除多余的
                while (root.ChildNodes.Count > 100)
                    root.RemoveChild(root.ChildNodes[root.ChildNodes.Count - 1]);

                // 2007/6/19
                // 增量借阅量属性值
                string strBorrowCount = DomUtil.GetAttr(root, "count");
                if (String.IsNullOrEmpty(strBorrowCount) == true)
                    strBorrowCount = "1";
                else
                {
                    long lCount = 1;
                    try
                    {
                        lCount = Convert.ToInt64(strBorrowCount);
                    }
                    catch { }
                    lCount++;
                    strBorrowCount = lCount.ToString();
                }
                DomUtil.SetAttr(root, "count", strBorrowCount);
            }

            // 增添超期信息
            string strOverdueString = DomUtil.GetElementText(domLog.DocumentElement,
                "overdues");
            if (String.IsNullOrEmpty(strOverdueString) == false)
            {
                XmlDocumentFragment fragment = readerdom.CreateDocumentFragment();
                fragment.InnerXml = strOverdueString;

                List<string> existing_ids = new List<string>();

                // 看看根下面是否有overdues元素
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("overdues");
                if (root == null)
                {
                    root = readerdom.CreateElement("overdues");
                    readerdom.DocumentElement.AppendChild(root);
                }
                else
                {
                    // 记载以前已经存在的 id
                    XmlNodeList nodes = root.SelectNodes("overdue");
                    foreach (XmlElement node in nodes)
                    {
                        string strID = node.GetAttribute("id");
                        if (string.IsNullOrEmpty(strID) == false)
                            existing_ids.Add(strID);
                    }
                }

                // root.AppendChild(fragment);
                {
                    // 一个一个加入，丢掉重复 id 属性值得 overdue 元素
                    XmlNodeList nodes = fragment.SelectNodes("overdue");
                    foreach (XmlElement node in nodes)
                    {
                        string strID = node.GetAttribute("id");
                        if (existing_ids.IndexOf(strID) != -1)
                            continue;
                        root.AppendChild(node);
                    }
                }
            }

            if (itemdom != null)
            {

                // *** 检查册记录操作前在借的读者，是否指向另外一个读者。如果是这样，则需要事先消除相关的另一个读者记录的痕迹，也就是说相当于把相关的册给进行还书操作
                string strBorrower0 = DomUtil.GetElementInnerText(itemdom.DocumentElement,
        "borrower");
                if (string.IsNullOrEmpty(strBorrower0) == false
                    && strBorrower0 != strReaderBarcode)
                {
#if NO
                string strRemovedInfo = "";

                // 去除读者记录侧的借阅信息链条
                // return:
                //      -1  出错
                //      0   没有必要修复
                //      1   修复成功
                nRet = RemoveReaderSideLink(
                    // Channels,
                    channel,
                    strBorrower0,
                    strItemBarcodeParam,
                    out strRemovedInfo,
                    out strError);
                if (nRet == -1)
                {
                    this.WriteErrorLog("册条码号为 '" + strItemBarcodeParam + "' 的册记录，在进行还书操作(拟被读者 '" + strReaderBarcode + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件尝试自动修正(删除)此读者记录的半侧借阅信息链。不过，在去除读者记录册借阅链时发生错误: " + strError);
                }
                else
                {
                    this.WriteErrorLog("册条码号为 '" + strItemBarcodeParam + "' 的册记录，在进行还书操作(拟被读者 '" + strReaderBarcode + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件已经自动修正(删除)了此读者记录的半侧借阅信息链。被移走的片断 XML 信息为 '" + strRemovedInfo + "'");
                }
#endif
                }


                // *** 修改册记录
                XmlElement nodeHistoryBorrower = null;

                string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement, "borrower");

                XmlNode dup_item_history = null;
                // 看看相同借者、借阅日期、换回日期的 BorrowHistory/borrower 元素是否已经存在
                {
                    string strBorrowDate = DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate");
                    dup_item_history = itemdom.DocumentElement.SelectSingleNode("borrowHistory/borrower[@barcode='" + strBorrower + "' and @borrowDate='" + strBorrowDate + "' and @returnDate='" + strOperTime + "']");
                }

                if (dup_item_history != null)
                {
                    // 历史信息节点已经存在，就不必加入了

                    // 清空相关元素
                    DomUtil.DeleteElement(itemdom.DocumentElement,
        "borrower");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
    "borrowDate");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
    "returningDate");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
    "borrowPeriod");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
    "operator");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
    "no");
                    DomUtil.DeleteElement(itemdom.DocumentElement,
    "renewComment");
                }
                else
                {
                    // 加入历史信息节点

                    // TODO: 也可从 domLog 中取得信息，创建 borrowHistory 下级事项。但要防范重复加入的情况
                    // 这里判断册记录中 borrower 元素是否为空的做法，具有可以避免重复加入 borrowHistory 下级事项的优点
                    if (string.IsNullOrEmpty(strBorrower) == false)
                    {
                        // 加入到借阅历史字段中
                        {
                            // 看看根下面是否有borrowHistory元素
                            XmlNode root = itemdom.DocumentElement.SelectSingleNode("borrowHistory");
                            if (root == null)
                            {
                                root = itemdom.CreateElement("borrowHistory");
                                itemdom.DocumentElement.AppendChild(root);
                            }

                            nodeHistoryBorrower = itemdom.CreateElement("borrower");

                            // 插入到最前面
                            nodeHistoryBorrower = DomUtil.InsertFirstChild(root, nodeHistoryBorrower) as XmlElement;  // 2015/1/12 增加等号左边的部分

                            // 如果超过100个，则删除多余的
                            while (root.ChildNodes.Count > 100)
                                root.RemoveChild(root.ChildNodes[root.ChildNodes.Count - 1]);
                        }

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
                    "barcode",
                    DomUtil.GetElementText(itemdom.DocumentElement, "borrower"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrower", "");
#endif
                        SetAttribute(ref itemdom,
            "borrower",
            nodeHistoryBorrower,
            "barcode",
            true);

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
                  "borrowDate",
                  DomUtil.GetElementText(itemdom.DocumentElement, "borrowDate"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrowDate", "");
#endif
                        SetAttribute(ref itemdom,
        "borrowDate",
        nodeHistoryBorrower,
        "borrowDate",
        true);

#if NO
                DomUtil.SetAttr(nodeOldBorrower,
      "returningDate",
      DomUtil.GetElementText(itemdom.DocumentElement, "returningDate"));
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "returningDate", "");
#endif
                        SetAttribute(ref itemdom,
        "returningDate",
        nodeHistoryBorrower,
        "returningDate",
        true);

                        SetAttribute(ref itemdom,
                            "borrowPeriod",
                            nodeHistoryBorrower,
                            "borrowPeriod",
                            true);

                        // borrowOperator
                        SetAttribute(ref itemdom,
        "operator",
        nodeHistoryBorrower,
        "borrowOperator",
        true);

                        // operator 本次还书的操作者
                        DomUtil.SetAttr(nodeHistoryBorrower,
                          "operator",
                          strReturnOperator);

                        DomUtil.SetAttr(nodeHistoryBorrower,
              "returnDate",
              strOperTime);

                        // TODO: 0 需要省略
                        SetAttribute(ref itemdom,
        "no",
        nodeHistoryBorrower,
        "no",
        true);

                        // renewComment
                        SetAttribute(ref itemdom,
        "renewComment",
        nodeHistoryBorrower,
        "renewComment",
        true);

                        {
                            string strText = strRecoverComment;
                            string strOldRecoverComment = DomUtil.GetElementText(itemdom.DocumentElement, "recoverComment");
                            if (String.IsNullOrEmpty(strOldRecoverComment) == false)
                                strText = "(借阅时原注: " + strOldRecoverComment + ") " + strRecoverComment;

                            if (String.IsNullOrEmpty(strText) == false)
                            {
                                DomUtil.SetAttr(nodeHistoryBorrower,
                                    "recoverComment",
                                    strText);
                            }
                        }
                    }

                    if (strAction == "lost")
                    {
                        // 修改册记录的<state>
                        string strState = DomUtil.GetElementText(itemdom.DocumentElement,
                            "state");
                        if (nodeHistoryBorrower != null)
                        {
                            DomUtil.SetAttr(nodeHistoryBorrower,
            "state",
            strState);
                        }

                        if (String.IsNullOrEmpty(strState) == false)
                            strState += ",";
                        strState += "丢失";
                        DomUtil.SetElementText(itemdom.DocumentElement,
                            "state", strState);

                        // 将日志记录中的<lostComment>内容追加写入册记录的<comment>中
                        string strLostComment = DomUtil.GetElementText(domLog.DocumentElement,
                            "lostComment");

                        if (strLostComment != "")
                        {
                            string strComment = DomUtil.GetElementText(itemdom.DocumentElement,
                                "comment");

                            if (nodeHistoryBorrower != null)
                            {
                                DomUtil.SetAttr(nodeHistoryBorrower,
                                    "comment",
                                    strComment);
                            }

                            if (String.IsNullOrEmpty(strComment) == false)
                                strComment += "\r\n";
                            strComment += strLostComment;
                            DomUtil.SetElementText(itemdom.DocumentElement,
                                "comment", strComment);
                        }
                    }
                }
            }

            return 0;
        }

        // 从 XML 元素设置到 XML 属性
        public static void SetAttribute(ref XmlDocument dom,
            string strElementName,
            XmlElement nodeBorrow,
            string strAttrName,
            bool bDeleteElement)
        {
            string strValue = DomUtil.GetElementText(dom.DocumentElement,
strElementName);
            if (string.IsNullOrEmpty(strValue) == false)
                nodeBorrow.SetAttribute(strAttrName, strValue);

            if (bDeleteElement == true)
                DomUtil.DeleteElement(dom.DocumentElement, strElementName);
        }

        // 借阅操作，修改读者和册记录
        // parameters:
        //      strItemBarcodeParam 册条码号。可以使用 @refID: 前缀
        //      strLibraryCode  读者记录所从属的读者库的馆代码
        public static int BorrowChangeReaderAndItemRecord(
            string strItemBarcodeParam,
            string strReaderBarcode,
            XmlDocument domLog,
            string strRecoverComment,
            // string strLibraryCode,
            ref XmlDocument readerdom,
            ref XmlDocument itemdom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strOperator = DomUtil.GetElementText(domLog.DocumentElement,
    "operator");

            // *** 修改读者记录
            string strNo = DomUtil.GetElementText(domLog.DocumentElement,
                "no");

            int nNo = 0;
            if (string.IsNullOrEmpty(strNo) == false
                && Int32.TryParse(strNo, out nNo) == false)
            {
                strError = "<no>元素值 '" + strNo + "' 应该为纯数字";
                return -1;
            }

            XmlElement nodeBorrow = null;

            // 既然日志记录中记载的是 @refID: 的形态，那读者记录中 borrows 里面势必记载的也是这个形态
            nodeBorrow = readerdom.DocumentElement.SelectSingleNode("borrows/borrow[@barcode='" + strItemBarcodeParam + "']") as XmlElement;

            if (nodeBorrow != null)
            {
                // 为了提高容错能力，续借操作时不去追究以前是否借阅过
            }
            else
            {
                // 检查<borrows>元素是否存在
                XmlNode root = readerdom.DocumentElement.SelectSingleNode("borrows");
                if (root == null)
                {
                    root = readerdom.CreateElement("borrows");
                    root = readerdom.DocumentElement.AppendChild(root);
                }

                // 加入借阅册信息
                nodeBorrow = readerdom.CreateElement("borrow");
                nodeBorrow = root.AppendChild(nodeBorrow) as XmlElement;
            }

            // 
            // barcode
            DomUtil.SetAttr(nodeBorrow, "barcode", strItemBarcodeParam);

            string strRenewComment = "";

            string strBorrowDate = DomUtil.GetElementText(domLog.DocumentElement,
                "borrowDate");

            if (nNo >= 1)
            {
                // 保存前一次借阅的信息
                strRenewComment = DomUtil.GetAttr(nodeBorrow, "renewComment");

                if (strRenewComment != "")
                    strRenewComment += "; ";

                strRenewComment += "no=" + Convert.ToString(nNo - 1) + ", ";
                strRenewComment += "borrowDate=" + DomUtil.GetAttr(nodeBorrow, "borrowDate") + ", ";
                strRenewComment += "borrowPeriod=" + DomUtil.GetAttr(nodeBorrow, "borrowPeriod") + ", ";
                strRenewComment += "returnDate=" + strBorrowDate + ", ";
                strRenewComment += "operator=" + DomUtil.GetAttr(nodeBorrow, "operator");
            }

            // borrowDate
            DomUtil.SetAttr(nodeBorrow, "borrowDate",
                strBorrowDate);

            // no
            DomUtil.SetAttr(nodeBorrow, "no", Convert.ToString(nNo));

            // borrowPeriod
            string strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
                "borrowPeriod");

            if (String.IsNullOrEmpty(strBorrowPeriod) == true)
            {
                strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
"defaultBorrowPeriod");
                if (String.IsNullOrEmpty(strBorrowPeriod) == true)
                    strBorrowPeriod = "60day";// 简单化处理
            }

            DomUtil.SetAttr(nodeBorrow, "borrowPeriod", strBorrowPeriod);

            // returningDate
            SetAttribute(domLog,
                "returningDate",
                nodeBorrow);

            // renewComment
            {
                if (string.IsNullOrEmpty(strRenewComment) == false)
                    DomUtil.SetAttr(nodeBorrow, "renewComment", strRenewComment);
            }

            // operator
            SetAttribute(domLog,
    "operator",
    nodeBorrow);

            // recoverComment
            if (String.IsNullOrEmpty(strRecoverComment) == false)
                DomUtil.SetAttr(nodeBorrow, "recoverComment", strItemBarcodeParam);

            // type
            SetAttribute(domLog,
                "type",
                nodeBorrow);

            // price
            SetAttribute(domLog,
                "price",
                nodeBorrow);

            if (itemdom != null)
            {
                // *** 检查册记录以前是否存在在借的痕迹，如果存在的话，(如果指向当前读者倒是无妨了反正后面即将要覆盖) 需要事先消除相关的另一个读者记录的痕迹，也就是说相当于把相关的册给进行还书操作
                string strBorrower0 = DomUtil.GetElementInnerText(itemdom.DocumentElement,
                "borrower");
                if (string.IsNullOrEmpty(strBorrower0) == false
                    && strBorrower0 != strReaderBarcode)
                {
#if NO
                // 去除读者记录侧的借阅信息链条
                // return:
                //      -1  出错
                //      0   没有必要修复
                //      1   修复成功
                nRet = RemoveReaderSideLink(
                    // Channels,
                    channel,
                    strBorrower0,
                    strItemBarcodeParam,
                    out string strRemovedInfo,
                    out strError);
                if (nRet == -1)
                {
                    //this.WriteErrorLog("册条码号为 '" + strItemBarcodeParam + "' 的册记录，在进行借书操作(拟被读者 '" + strReaderBarcode + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件尝试自动修正(删除)此读者记录的半侧借阅信息链。不过，在去除读者记录册借阅链时发生错误: " + strError);
                    writeLog?.Invoke($"册条码号为 '{strItemBarcodeParam}' 的册记录，在进行借书操作(拟被读者 '{strReaderBarcode}' 借阅)以前，发现它被另一读者 '{strBorrower0}' 持有，软件尝试自动修正(删除)此读者记录的半侧借阅信息链。不过，在去除读者记录册借阅链时发生错误: {strError}");
                }
                else
                {
                    //this.WriteErrorLog("册条码号为 '" + strItemBarcodeParam + "' 的册记录，在进行借书操作(拟被读者 '" + strReaderBarcode + "' 借阅)以前，发现它被另一读者 '" + strBorrower0 + "' 持有，软件已经自动修正(删除)了此读者记录的半侧借阅信息链。被移走的片断 XML 信息为 '" + strRemovedInfo + "'");
                    writeLog?.Invoke($"册条码号为 '{strItemBarcodeParam}' 的册记录，在进行借书操作(拟被读者 '{strReaderBarcode}' 借阅)以前，发现它被另一读者 '{strBorrower0}' 持有，软件已经自动修正(删除)了此读者记录的半侧借阅信息链。被移走的片断 XML 信息为 '{strRemovedInfo}'");
                }
#endif
                }


                // *** 修改册记录
                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrower", strReaderBarcode);

                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrowDate",
                    strBorrowDate);

                DomUtil.SetElementText(itemdom.DocumentElement,
                    "no",
                    Convert.ToString(nNo));

                DomUtil.SetElementText(itemdom.DocumentElement,
                    "borrowPeriod",
                    strBorrowPeriod);

                DomUtil.SetElementText(itemdom.DocumentElement,
                    "renewComment",
                    strRenewComment);

                DomUtil.SetElementText(itemdom.DocumentElement,
        "operator",
        strOperator);

                // recoverComment
                if (String.IsNullOrEmpty(strRecoverComment) == false)
                {
                    DomUtil.SetElementText(itemdom.DocumentElement,
            "recoverComment",
            strRecoverComment);
                }
            }
            return 0;
        }

        // 从 XML 元素设置到 XML 属性
        public static void SetAttribute(XmlDocument domLog,
            string strAttrName,
            XmlElement nodeBorrow)
        {
            string strValue = DomUtil.GetElementText(domLog.DocumentElement,
strAttrName);
            if (string.IsNullOrEmpty(strValue) == false)
                nodeBorrow.SetAttribute(strAttrName, strValue);
        }

#if REMOVED
        /*
<rfid>
<ownerInstitution>
<item map="海淀分馆/" isil="test" />
<item map="西城/" alternative="xc" />
</ownerInstitution>
<patronMaps>
<item libraryCode="" isil="test" />
<item libraryCode="海淀分馆" isil="xc" />
</patronMaps>
</rfid>
map 为 "/" 或者 "/阅览室" 可以匹配 "图书总库" "阅览室" 这样的 strLocation
map 为 "海淀分馆/" 可以匹配 "海淀分馆/" "海淀分馆/阅览室" 这样的 strLocation
最好单元测试一下这个函数
* */
        // parameters:
        //      cfg_dom 根元素是 rfid
        //      strLibraryCode 馆代码
        //      isil    [out] 返回 ISIL 形态的代码
        //      alternative [out] 返回其他形态的代码
        // return:
        //      true    找到。信息在 isil 和 alternative 参数里面返回
        //      false   没有找到
        public static bool GetPatronOwnerInstitution(
            XmlElement rfid,
            string strLibraryCode,
            out string isil,
            out string alternative)
        {
            isil = "";
            alternative = "";

            if (rfid == null)
                return false;

            XmlNodeList items = rfid.SelectNodes(
                "patronMaps/item");
            List<HitItem> results = new List<HitItem>();
            foreach (XmlElement item in items)
            {
                string strCurrentLibraryCode = item.GetAttribute("libraryCode");
                if (strCurrentLibraryCode == strLibraryCode)
                {
                    HitItem hit = new HitItem { Map = strCurrentLibraryCode, Element = item };
                    results.Add(hit);
                }
            }

            if (results.Count == 0)
                return false;

            // 如果命中多个，要选出 map 最长的那一个返回

            // 排序，大在前
            if (results.Count > 0)
                results.Sort((a, b) => { return b.Map.Length - a.Map.Length; });

            isil = results[0].Element.GetAttribute("isil");
            alternative = results[0].Element.GetAttribute("alternative");
            return true;
        }
#endif

        /*
<rfid>
    <ownerInstitution>
        <item map="海淀分馆/" isil="test" />
        <item map="西城/" alternative="xc" />
    </ownerInstitution>
</rfid>
map 为 "/" 或者 "/阅览室" 可以匹配 "图书总库" "阅览室" 这样的 strLocation
map 为 "海淀分馆/" 可以匹配 "海淀分馆/" "海淀分馆/阅览室" 这样的 strLocation
最好单元测试一下这个函数
 * */
        // parameters:
        //      cfg_dom 根元素是 rfid
        //      strLocation 纯净的 location 元素内容。
        //                  或者用馆代码，比如 "/" 表示总馆；"海淀分馆/" 表示分馆
        //      isil    [out] 返回 ISIL 形态的代码
        //      alternative [out] 返回其他形态的代码
        // return:
        //      true    找到。信息在 isil 和 alternative 参数里面返回
        //      false   没有找到
        // exception:
        //      可能会抛出异常 Exception
        public static bool GetOwnerInstitution(
            XmlElement rfid,
            string strLocation,
            out string isil,
            out string alternative)
        {
            isil = "";
            alternative = "";

            if (rfid == null)
                return false;

            if (strLocation != null
    && strLocation.IndexOfAny(new char[] { '*', '?' }) != -1)
                throw new ArgumentException($"参数 {nameof(strLocation)} 值({strLocation})中不应包含字符 '*' '?'", nameof(strLocation));

            // 分析 strLocation 是否属于总馆形态，比如“阅览室”
            // 如果是总馆形态，则要在前部增加一个 / 字符，以保证可以正确匹配 map 值
            // ‘/’字符可以理解为在馆代码和阅览室名字之间插入的一个必要的符号。这是为了弥补早期做法的兼容性问题
            dp2StringUtil.ParseCalendarName(strLocation,
        out string strLibraryCode,
        out string strRoom);
            if (string.IsNullOrEmpty(strLibraryCode))
                strLocation = "/" + strRoom;

            XmlNodeList items = rfid.SelectNodes(
                "ownerInstitution/item");
            List<HitItem> results = new List<HitItem>();
            foreach (XmlElement item in items)
            {
                string map = item.GetAttribute("map");

                if (StringUtil.RegexCompare(GetRegex(map), strLocation))
                // if (strLocation.StartsWith(map))
                {
                    HitItem hit = new HitItem { Map = map, Element = item };
                    results.Add(hit);
                }
            }

            if (results.Count == 0)
                return false;

            // 如果命中多个，要选出 map 最长的那一个返回

            // 排序，大在前
            if (results.Count > 0)
                results.Sort((a, b) => { return b.Map.Length - a.Map.Length; });

            var element = results[0].Element;
            isil = element.GetAttribute("isil");
            alternative = element.GetAttribute("alternative");

            // 2021/2/1
            if (string.IsNullOrEmpty(isil) && string.IsNullOrEmpty(alternative))
            {
                throw new Exception($"map 元素不合法，isil 和 alternative 属性均为空");
            }
            return true;
        }

        class HitItem
        {
            public XmlElement Element { get; set; }
            public string Map { get; set; }
        }

        static string GetRegex(string pattern)
        {
            if (pattern == null)
                pattern = "";
            if (pattern.Length > 0 && pattern[pattern.Length - 1] != '*')
                pattern += "*";
            return "^" + Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".")
            + "$";
        }

    }
}
