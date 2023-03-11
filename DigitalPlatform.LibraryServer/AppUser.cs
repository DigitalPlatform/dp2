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

using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;
using System.Linq;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是和用户管理相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 或者针对一个特定数据库、特定操作的权限定义字符串
        /*
            原始定义字符串格式： "中央库:setbiblioinfo=new,change|getbiblioinfo=xxx;工作库:setbiblioinfo=new"
         * 
         * */
        // parameters:
        //      strDbName   数据库名。如果为空，表示匹配权限字符串中的任意数据库名
        // return:
        //      null    指定的操作类型的权限没有定义
        //      ""      定义了指定类型的操作权限，但是否定的定义
        //      其它      权限列表。* 表示通配的权限列表
        public static string GetDbOperRights(string strAccessString,
            string strDbName,
            string strOperation)
        {
            // string[] segments = strAccessString.Split(new char[] {';'});
            List<string> segments = StringUtil.SplitString(strAccessString,
                ";",
                new string[] { "()" },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < segments.Count; i++)
            {
                string strSegment = segments[i].Trim();
                if (String.IsNullOrEmpty(strSegment) == true)
                    continue;
                string strDbNameList = "";

                int nRet = strSegment.IndexOf(":");
                if (nRet == -1)
                {
                    // 仅有数据库名列表部分，操作名列表和权限列表都为*
                    strDbNameList = strSegment;

                    // 剩余部分
                    strSegment = "*";
                    goto DOMATCH;
                }
                else
                {
                    strDbNameList = strSegment.Substring(0, nRet).Trim();

                    // 剩余部分
                    strSegment = strSegment.Substring(nRet + 1).Trim();
                }

            DOMATCH:
                // string[] sections = strSegment.Split(new char[] {'|'});
                List<string> sections = StringUtil.SplitString(strSegment,
                    "|",
                    new string[] { "()" },
                    StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < sections.Count; j++)
                {
                    string strOperList = "";
                    string strRightsList = "";

                    string strSection = sections[j];
                    if (String.IsNullOrEmpty(strSection) == true)
                        continue;

                    nRet = strSection.IndexOf("=");
                    if (nRet == -1)
                    {
                        // 仅有操作名列表部分，权限列表为*
                        strOperList = strSection;
                        strRightsList = "*";
                    }
                    else
                    {
                        strOperList = strSection.Substring(0, nRet).Trim();
                        strRightsList = strSection.Substring(nRet + 1).Trim();
                    }

                    if (strDbNameList == "*")
                    {
                        // 数据库名通配
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(strDbName) == false    // 如果参数 strDbName 为空，则任何库名都算匹配
                            && StringUtil.IsInList(strDbName, strDbNameList) == false)
                            continue;   // 数据库名不在列表中
                    }

                    if (strOperList == "*")
                    {
                        // 操作名通配
                    }
                    else
                    {
                        if (StringUtil.IsInList(strOperation, strOperList) == false)
                            continue;   // 操作名不在列表中
                    }

                    return strRightsList;
                }
            }

            return null;    // not found
        }

        // 获得一个账户的信息。不受当前用户的管辖范围的限制。所以这个函数只能提供内部使用，要谨慎
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public int GetUserInfo(string strUserName,
            out UserInfo userinfo,
            out string strError)
        {
            strError = "";
            userinfo = null;

            if (string.IsNullOrEmpty(strUserName) == true)
            {
                strError = "用户名不能为空";
                return -1;
            }

            UserInfo[] userinfos = null;
            // return:
            //      -1  出错
            //      其他    用户总数（不是本批的个数）
            int nRet = ListUsers(
                "",
                strUserName,
                0,
                1,
                out userinfos,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 0)
            {
                strError = $"用户 '{strUserName}' 没有找到";
                return 0;   // not found
            }

            if (userinfos == null || userinfos.Length < 1)
            {
                strError = "userinfos error";
                return -1;
            }
            userinfo = userinfos[0];
            return 1;
        }

        // 列出指定的用户
        // parameters:
        //      strUserName 用户名。如果为空，表示列出全部用户名
        // return:
        //      -1  出错
        //      其他    用户总数（不是本批的个数）
        public int ListUsers(
            string strLibraryCodeList,
            string strUserName,
            int nStart,
            int nCount,
            out UserInfo[] userinfos,
            out string strError)
        {
            // this.m_lock.AcquireReaderLock(m_nLockTimeout);
            this.LockForRead();    // 2016/10/16
            try
            {
                strError = "";
                userinfos = null;

                string strXPath = "";

                if (String.IsNullOrEmpty(strUserName) == true)
                {
                    strXPath = "accounts/account";
                }
                else
                {
                    strXPath = "accounts/account[@name='" + strUserName + "']";
                }

                List<UserInfo> userList = new List<UserInfo>();

                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);

                // 过滤为当前能管辖的小范围 node 数组
                List<XmlElement> smallerlist = new List<XmlElement>();
                foreach (XmlElement node in nodes)
                {
                    // 2012/9/9
                    // 分馆用户只允许列出管辖分馆的所有用户
                    if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                    {
                        string strCurrentLibraryCodeList = DomUtil.GetAttr(node, "libraryCode");
                        // TODO: 帐户定义中的馆代码列表中不允许 ,, 这样的情况
                        if (IsListInList(strCurrentLibraryCodeList, strLibraryCodeList) == false)
                            continue;
                    }

                    smallerlist.Add(node);
                }

                if (nCount == -1)
                    nCount = Math.Max(0, smallerlist.Count - nStart);
                nCount = Math.Min(100, nCount); // 限制每批最多100个

                for (int i = nStart; i < Math.Min(nStart + nCount, smallerlist.Count); i++)   // 
                {
                    XmlElement node = smallerlist[i];

                    string strCurrentLibraryCodeList = DomUtil.GetAttr(node, "libraryCode");

                    UserInfo userinfo = new UserInfo();
                    userinfo.UserName = DomUtil.GetAttr(node, "name");
                    userinfo.Type = DomUtil.GetAttr(node, "type");
                    userinfo.Rights = DomUtil.GetAttr(node, "rights");
                    userinfo.LibraryCode = strCurrentLibraryCodeList;
                    userinfo.Access = DomUtil.GetAttr(node, "access");
                    userinfo.Comment = DomUtil.GetAttr(node, "comment");
                    userinfo.Binding = node.GetAttribute("binding");
                    userinfo.Location = node.GetAttribute("location");

                    userList.Add(userinfo);
                }

                userinfos = new UserInfo[userList.Count];
                userList.CopyTo(userinfos);

                return smallerlist.Count;
            }
            finally
            {
                // this.m_lock.ReleaseReaderLock();
                this.UnlockForRead();
            }
        }

        static bool AutoBindingIP(UserInfo info, string strClientAddress)
        {
            string strBinding = info.Binding;
            if (string.IsNullOrEmpty(strBinding))
                return false;

            bool bChanged = false;
            List<string> temp = StringUtil.ParseTwoPart(strClientAddress, "@");
            string ip = temp[0];

            if (ip == "::1" || ip == "127.0.0.1")
                ip = "localhost";

            List<string> results = new List<string>();
            string[] parts = strBinding.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                string strLine = s.Trim();
                if (string.IsNullOrEmpty(strLine))
                    continue;
                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(strLine, ":", out strLeft, out strRight);
                if (strLeft == "ip")
                {
                    if (strRight == "[current]")
                    {
                        // 替换为当前前端的 ip 地址
                        results.Add("ip:" + ip);
                        bChanged = true;
                        continue;
                    }
                }
                results.Add(strLine);
            }

            info.Binding = StringUtil.MakePathList(results, ",");
            return bChanged;
        }

        // 创建新用户
        // TODO: 对DOM加锁
        public int CreateUser(
            string strLibraryCodeList,
            string strUserName,
            string strOperator,
            UserInfo userinfo,
            string strClientAddress,
            out ErrorCode error_code,
            out string strError)
        {
            strError = "";
            error_code = ErrorCode.NoError;

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName 参数值不允许为空";
                // error_code = ErrorCode.ArgumentError;
                return -1;
            }

            if (strUserName != userinfo.UserName)
            {
                strError = "strUserName 参数值和 userinfo.UserName 不一致";
                // error_code = ErrorCode.ArgumentError;
                return -1;
            }

            // 2021/7/9
            if (userinfo.SetPassword == false
                && string.IsNullOrEmpty(userinfo.Password) == false)
            {
                strError = "若要在创建账户的同时设置好初始密码，SetPassword 应设置为 true";
                // error_code = ErrorCode.ArgumentError;
                return -1;
            }

            // 2023/3/5
            // 检查 librarycode_list 中的馆代码是否都存在定义(libraries/library 元素)
            // return:
            //      -1  出错
            //      0   不存在定义，错误信息在 strError 中返回
            //      1   存在定义
            int nRet = CheckLibraryCodeExist(userinfo.LibraryCode,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            // 2012/9/9
            // 分馆用户只允许创建馆代码属于管辖分馆的帐户
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                if (string.IsNullOrEmpty(userinfo.LibraryCode) == true
                    || IsListInList(userinfo.LibraryCode, strLibraryCodeList) == false)
                {
                    strError = $"{GetCurrentUserName(null)}只能创建图书馆代码完全属于 '{strLibraryCodeList}' 范围的新用户";
                    error_code = ErrorCode.AccessDenied;
                    return -1;
                }
            }

            // TODO: 改为用 SearchReaderRecDup() 查重
            int nResultValue = -1;
            // 旧的 C# 校验脚本，和新的校验规则(XML 格式)都能兼容
            // 检查名字空间。
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            nRet = this.DoVerifyBarcodeScriptFunction(
                null,
                "",
                strUserName,
                out nResultValue,
                out strError);
            if (nRet == -2)
            {
                // 没有校验条码号功能，所以无法校验用户名和条码号名字空间的冲突
                goto SKIP_VERIFY;
            }
            if (nRet == -1)
            {
                strError = "校验用户名 '" + strUserName + "' 和条码号潜在冲突过程中(调用函数DoVerifyBarcodeScriptFunction()时)发生错误: " + strError;
                return -1;
            }

            Debug.Assert(nRet == 0, "");

            if (nResultValue == -1)
            {
                strError = "校验用户名 '" + strUserName + "' 和条码号潜在冲突过程中发生错误: " + strError;
                return -1;
            }

            if (nResultValue == 1)
            {
                strError = "名字 '" + strUserName + "' 和条码号名字空间发生冲突，不能作为用户名。";
                return -1;
            }

        SKIP_VERIFY:
            XmlElement nodeAccount = null;

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']") as XmlElement;
                if (nodeAccount != null)
                {
                    strError = "用户 '" + strUserName + "' 已经存在";
                    error_code = ErrorCode.AlreadyExist;
                    return -1;
                }

                XmlNode root = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts");
                if (root == null)
                {
                    root = this.LibraryCfgDom.CreateElement("accounts");
                    this.LibraryCfgDom.DocumentElement.AppendChild(root);
                }

                nodeAccount = this.LibraryCfgDom.CreateElement("account");
                root.AppendChild(nodeAccount);

                // 替换 binding 中的自动绑定 IP 参数
                AutoBindingIP(userinfo, strClientAddress);

                SetUserXml(userinfo, nodeAccount);

#if NO
                DomUtil.SetAttr(nodeAccount, "name", userinfo.UserName);

                if (String.IsNullOrEmpty(userinfo.Type) == false)
                    DomUtil.SetAttr(nodeAccount, "type", userinfo.Type);

                DomUtil.SetAttr(nodeAccount, "rights", userinfo.Rights);

                DomUtil.SetAttr(nodeAccount, "libraryCode", userinfo.LibraryCode);

                DomUtil.SetAttr(nodeAccount, "access", userinfo.Access);

                DomUtil.SetAttr(nodeAccount, "comment", userinfo.Comment);

                DomUtil.SetAttr(nodeAccount, "binding", userinfo.Binding);
#endif
                // 2021/7/9
                // 确保当 SetPassword 为 false 是前端不提供密码
                if (userinfo.SetPassword == false)
                    userinfo.Password = "";

                // 设置密码
                if (userinfo.SetPassword == true
                    || string.IsNullOrEmpty(_passwordStyle) == false    // 如果有强密码要求，则空密码也要参与密码检查
                    )
                {
                    // return:
                    //      -1  出错
                    //      0   不合法(原因在 strError 中返回)
                    //      1   合法
                    nRet = ValidateUserPassword(nodeAccount,
                        userinfo.Password,
                        _passwordStyle,
                        true,
                        out strError);
                    if (nRet != 1)
                    {
                        // 删除刚创建的 account 元素
                        nodeAccount.ParentNode?.RemoveChild(nodeAccount);
                        // error_code = ErrorCode.WeakPassword;
                        return -1;
                    }

                    string type = "bcrypt";
                    nRet = LibraryServerUtil.SetUserPassword(
                        type,
                        userinfo.Password,
                        out string strHashed,
                        out strError);
                    if (nRet == -1)
                    {
                        // 删除刚创建的 account 元素
                        nodeAccount.ParentNode?.RemoveChild(nodeAccount);
                        return -1;
                    }

                    // DomUtil.SetAttr(nodeAccount, "password", strHashed);
                    LibraryServerUtil.SetPasswordValue(nodeAccount,
                        type,
                        strHashed);
                }

                // 注: 无论是否明确要求设置密码(也就是说可能会创建空密码)，都要为密码设置失效期
                if (LibraryServerUtil.IsSpecialUserName(userinfo.UserName) == false)
                    SetPasswordExpire(nodeAccount,
                        _passwordExpirePeriod,
                        DateTime.Now);

                this.Changed = true;

                // 2014/9/16
                if (userinfo.UserName == "reader")
                    this.ClearLoginCache("");
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }

            // 写入日志
            {
                XmlDocument domOperLog = PrepareOperlogDom("new", strOperator);
                XmlElement node = domOperLog.CreateElement("account");
                domOperLog.DocumentElement.AppendChild(node);

                DomUtil.SetElementOuterXml(node, nodeAccount.OuterXml);

                // 写入日志
                nRet = this.OperLog.WriteOperLog(domOperLog,
                    strClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "SetUser() API 写入日志时发生错误: " + strError;
                    return -1;
                }
            }

            return 0;
        }

        public static void SetUserXml(UserInfo userinfo, XmlElement nodeAccount)
        {
            DomUtil.SetAttr(nodeAccount, "name", userinfo.UserName);

            if (String.IsNullOrEmpty(userinfo.Type) == false)
                DomUtil.SetAttr(nodeAccount, "type", userinfo.Type);

            DomUtil.SetAttr(nodeAccount, "rights", userinfo.Rights);

            DomUtil.SetAttr(nodeAccount, "libraryCode", userinfo.LibraryCode);

            DomUtil.SetAttr(nodeAccount, "access", userinfo.Access);

            DomUtil.SetAttr(nodeAccount, "comment", userinfo.Comment);

            DomUtil.SetAttr(nodeAccount, "binding", userinfo.Binding);
            DomUtil.SetAttr(nodeAccount, "location", userinfo.Location);
        }

        XmlDocument PrepareOperlogDom(string strAction,
            string strOperator)
        {
            // 准备日志DOM
            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            // 操作不涉及到读者库，所以没有<libraryCode>元素
            DomUtil.SetElementText(domOperLog.DocumentElement, "operation",
                "setUser");
            DomUtil.SetElementText(domOperLog.DocumentElement, "action",
                strAction);

            string strOperTimeString = this.Clock.GetClock();   // RFC1123格式

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                strOperator);
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTimeString);

            return domOperLog;
        }

        // 对用户名查重
        public bool SearchUserNameDup(string strUserName)
        {
            // this.m_lock.AcquireReaderLock(m_nLockTimeout);
            this.LockForRead();    // 2016/10/16
            try
            {
                // 查重
                XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']");
                if (node != null)
                    return true;

                return false;
            }
            finally
            {
                // this.m_lock.ReleaseReaderLock();
                this.UnlockForRead();
            }
        }

        // 2021/6/29
        public static string GetPasswordValue(XmlElement account,
            out string type)
        {
            type = (account.SelectSingleNode("password") as XmlElement)?.GetAttribute("type");
            return account.SelectSingleNode("password")?.InnerText;
        }

        /* 已经移动到 LibraryServerUtil 中
        // 2021/6/29
        public static void SetPasswordValue(XmlElement account,
            string type,
            string password_text)
        {
            XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
            if (password_element == null)
            {
                password_element = account.OwnerDocument.CreateElement("password");
                password_element = account.AppendChild(password_element) as XmlElement;
            }

            // 2021/8/26
            if (string.IsNullOrEmpty(type))
                password_element.RemoveAttribute("type");
            else
                password_element.SetAttribute("type", type);
            password_element.InnerText = password_text;
        }
        */

        // 2021/7/2
        // parameters:
        //      now     当前时间               
        //      append  == true: 如果 expire 属性中已经有了值，不会修改
        public static bool SetPasswordExpire(XmlElement account,
            TimeSpan passwordExpirePeriod,
            DateTime now,
            bool append = false)
        {
            bool changed = false;

            var rights = account.GetAttribute("rights");
            bool neverExpire = StringUtil.IsInList("neverexpire", rights);

            XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
            if (password_element == null)
            {
                password_element = account.OwnerDocument.CreateElement("password");
                password_element = account.AppendChild(password_element) as XmlElement;
                changed = true;
            }
            if (passwordExpirePeriod == TimeSpan.MaxValue
                || neverExpire == true)
            {
                password_element.RemoveAttribute("expire");
                changed = true;
            }
            else
            {
                var old_expire_value = password_element.GetAttribute("expire");
                if (append == true && string.IsNullOrEmpty(old_expire_value) == false)
                {
                    // (当 now == DateTime.MinValue 时)如果 expire 属性中已经有了值，不会修改
                }
                else
                {
                    string strExpireTime = DateTimeUtil.Rfc1123DateTimeStringEx(now + passwordExpirePeriod); // 本地时间
                    password_element.SetAttribute("expire", strExpireTime);
                    changed = true;
                }
            }

            return changed;
        }

        // 2021/7/2
        public static bool ClearPasswordExpire(XmlElement account)
        {
            XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
            if (password_element == null)
                return false;
            if (password_element.HasAttribute("expire"))
            {
                password_element.RemoveAttribute("expire");
                return true;
            }
            return false;
        }

        // 2021/7/2
        public static DateTime GetPasswordExpire(XmlElement account)
        {
            XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
            if (password_element == null)
                return DateTime.MaxValue;   // 永不失效

            string strExpireTime = password_element.GetAttribute("expire");
            if (string.IsNullOrEmpty(strExpireTime) == true)
                return DateTime.MaxValue;   // 永不失效

            try
            {
                return DateTimeUtil.FromRfc1123DateTimeString(strExpireTime).ToLocalTime();
            }
            catch
            {
                return DateTime.MinValue;   // 立即失效
            }
        }

#if NO
        // 2021/7/2
        // 观察在 password 元素 expire 属性中的失效期
        // parameters:
        //      now 当前时间。本地时间
        //      expire  失效期末端时间。本地时间
        // return:
        //      -1  出错
        //      0   password 元素没有 expire 属性。也就是说永远不会失效
        //      1   已经过了失效期
        //      2   还在失效期以内
        public static int CheckExpireTime(XmlElement account,
            DateTime now,
            out DateTime expire,
            out string strError)
        {
            strError = "";
            expire = new DateTime(0);

            XmlElement password_element = account.SelectSingleNode("password") as XmlElement;
            if (password_element == null)
                return 0;

            string strExpireTime = password_element.GetAttribute("expire");
            if (string.IsNullOrEmpty(strExpireTime) == true)
                return 0;

            try
            {
                expire = DateTimeUtil.FromRfc1123DateTimeString(strExpireTime).ToLocalTime();

                if (now > expire)
                {
                    // 失效期已经过了
                    return 1;
                }
            }
            catch (Exception)
            {
                strError = "密码失效期时间字符串 '" + strExpireTime + "' 格式不正确，应为 RFC1123 格式";
                return -1;
            }

            return 2;   // 尚在失效期以内
        }
#endif

        /*
        // return:
        //      -1  出错
        //      0   不合法(原因在 strError 中返回)
        //      1   合法
        public static int ValidatePassword(
            string passwordStyle,
    XmlElement account,
    string password,
    out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(passwordStyle))
                return 1;
            return ValidatePassword(
    account,
    password,
    passwordStyle,
    out strError);
        }
        */

        // 验证工作人员密码字符串的合法性
        // parameters:
        //      style   风格。style-1 为第一种密码风格
        // return:
        //      -1  出错
        //      0   不合法(原因在 strError 中返回)
        //      1   合法
        public static int ValidateUserPassword(
            XmlElement account,
            string password,
            string style,
            bool check_old_password,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(style))
                return 1;

            List<string> errors = new List<string>();

            // 风格 1
            /*
1. 8个字符，且不能是顺序、逆序或相同
2. 数字加字母组合
3. 密码和用户名不可以一样
4. 临时密码不可以当做正式密码使用
5. 新旧密码不能一样
             * */
            if (StringUtil.IsInList("style-1", style))
            {
                if (string.IsNullOrEmpty(password))
                {
                    errors.Add("密码不允许为空");
                    goto ERROR1;
                }

                if (string.IsNullOrEmpty(password) == true
                    || password.Length < 8)
                    errors.Add("密码字符数不能小于 8");

                if (IsSequence(password))
                    errors.Add("密码内容不能为顺序字符");

                if (ContainsDigit(password) == false || ContainsLetter(password) == false)
                    errors.Add("密码内容必须同时包含数字和字母");

                var userName = GetUserNameValue(account);

                if (password == userName)
                    errors.Add("密码不能和用户名相同");

                if (check_old_password)
                {
                    // 和当前存在的旧密码比较
                    var old_password_hashed = GetPasswordValue(account, out string type);
                    if (string.IsNullOrEmpty(old_password_hashed) == false)
                    {
                        // 验证密码
                        // return:
                        //      -1  出错
                        //      0   不匹配
                        //      1   匹配
                        int nRet = LibraryServerUtil.MatchUserPassword(
                            type,
                            password,
                            old_password_hashed,
                            true,
                            out _);
                        if (nRet == 1)
                            errors.Add("密码不能和旧密码相同");
                    }
                }

                if (errors.Count > 0)
                    goto ERROR1;
            }

            strError = "密码合法";
            return 1;
        ERROR1:
            strError = $"密码不合法: {StringUtil.MakePathList(errors, "; ")}";
            return 0;
        }

        public static string GetUserNameValue(XmlElement account)
        {
            return account.GetAttribute("name");
        }

        static bool ContainsDigit(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            foreach (char ch in text)
            {
                if (char.IsDigit(ch))
                    return true;
            }

            return false;
        }

        static bool ContainsLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            foreach (char ch in text)
            {
                if (char.IsLetter(ch))
                    return true;
            }

            return false;
        }

        // 先后三次判断，是否为内码顺序增加，相同，顺序减少的字符串
        public static bool IsSequence(string text)
        {
            if (IsSequence(text, 1))
                return true;
            if (IsSequence(text, 0))
                return true;
            if (IsSequence(text, -1))
                return true;
            return false;
        }

        // 根据 direction 参数判断，是否为内码顺序增加或相同或顺序减少的字符串
        // parameters:
        //      direction   -1 逐步减小; 0 相同; 1 逐步增加
        public static bool IsSequence(string text, int direction)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // 只有一个字符的，不用判断
            if (text.Length == 1)
                return false;

            // 检查是否逐渐增加
            char prev_ch = (char)0;
            int i = 0;
            foreach (char ch in text)
            {
                if (i > 0)
                {
                    if (ch != prev_ch + direction)
                        return false;
                }
                prev_ch = ch;
                i++;
            }

            return true;
        }

        // 2021/7/16
        public XmlElement FindUserAccount(string strUserName,
            out string strError)
        {
            strError = "";
            var nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']") as XmlElement;
            if (nodeAccount == null)
            {
                strError = "用户 '" + strUserName + "' 不存在";
                return null;
            }
            return nodeAccount;
        }

        // 修改用户密码。这是指用户修改自己帐户的密码，需提供旧密码
        // return:
        //      -1  error
        //      0   succeed
        public int ChangeUserPassword(
            string strLibraryCodeList,
            string strUserName,
            string strOldPassword,
            string strNewPassword,
            string strClientIP,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName 参数值不能为空";
                return -1;
            }

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                // 查重
                var nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']") as XmlElement;
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在 (1)";
                    return -1;
                }

                string strExistLibraryCodeList = nodeAccount.GetAttribute("libraryCode");

                // 2012/9/9
                // 分馆用户只允许修改馆代码属于管辖分馆的帐户
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (string.IsNullOrEmpty(strExistLibraryCodeList) == true
                        || IsListInList(strExistLibraryCodeList, strLibraryCodeList) == false)
                    {
                        strError = $"{GetCurrentUserName(null)}只能修改图书馆代码完全完全属于 '{strLibraryCodeList}' 范围的用户的密码";
                        return -1;
                    }
                }

                // 验证旧密码
#if NO
                // 以前的做法
                string strExistPassword = DomUtil.GetAttr(node, "password");
                if (String.IsNullOrEmpty(strExistPassword) == false)
                {
                    try
                    {
                        strExistPassword = Cryptography.Decrypt(strExistPassword,
                            EncryptKey);
                    }
                    catch
                    {
                        strError = "已经存在的(加密后)密码格式不正确";
                        return -1;
                    }
                }

                if (strExistPassword != strOldPassword)
                {
                    strError = "所提供的旧密码经验证不匹配";
                    return -1;
                }
#endif

                nRet = this.UserNameTable.BeforeLogin(strUserName,
strClientIP,
out strError);
                if (nRet == -1)
                    return -1;

                // string strExistPassword = DomUtil.GetAttr(node, "password");
                string strExistPassword = GetPasswordValue(nodeAccount, out string old_type);

                // 注：这里故意不检查密码是否失效。因为即便密码失效，也要允许修改密码

                // return:
                //      -1  出错
                //      0   不匹配
                //      1   匹配
                nRet = LibraryServerUtil.MatchUserPassword(
                    old_type,
                    strOldPassword,
                    strExistPassword,
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 0 || nRet == 1)
                {
                    // parameters:
                    //      nLoginResult    1:成功 0:用户名或密码不正确 -1:出错
                    string strLogText = this.UserNameTable.AfterLogin(strUserName,
                        strClientIP,
                        nRet);
                    if (string.IsNullOrEmpty(strLogText) == false)
                        this.WriteErrorLog("!!! " + strLogText);
                }

                if (nRet == 0)
                {
                    // TODO: 防范暴力尝试密码
                    strError = "所提供的旧密码经验证不匹配";
                    return -1;
                }

                // 合法性
                // return:
                //      -1  出错
                //      0   不合法(原因在 strError 中返回)
                //      1   合法
                nRet = ValidateUserPassword(nodeAccount,
                    strNewPassword,
                    _passwordStyle,
                    true,
                    out strError);
                if (nRet != 1)
                    return -1;

                // 设置新密码
                string strHashed = "";
                string new_type = "bcrypt";
                nRet = LibraryServerUtil.SetUserPassword(
                    new_type,
                    strNewPassword,
                    out strHashed,
                    out strError);
                if (nRet == -1)
                    return -1;
                // DomUtil.SetAttr(node, "password", strHashed);
                LibraryServerUtil.SetPasswordValue(nodeAccount,
                    new_type,
                    strHashed);
                if (LibraryServerUtil.IsSpecialUserName(strUserName) == false)
                    SetPasswordExpire(nodeAccount, _passwordExpirePeriod, DateTime.Now);

                this.Changed = true;
                return 0;
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }

            // return 0;
        }

        // 检查一个馆代码，看是否匹配指定的馆代码。完整匹配或者局部匹配都返回 true
        static bool CompareLibraryCode(string strText, string strLibraryCode)
        {
            if (strText == null)
                strText = "";
            if (strLibraryCode == null)
                strLibraryCode = "";

            if (strText == strLibraryCode)
                return true;
            if (strText.StartsWith(strLibraryCode) == true)
            {
                // 一致的部分以后的第一个多出来的字符，应该是 '/' 才算匹配
                char ch = strText[strLibraryCode.Length];
                if (ch == '/')
                    return true;
            }

            return false;
        }

        // 检查一个馆藏地字符串，看是否匹配指定的馆代码。完整匹配或者局部匹配都返回 true
        static bool MatchLocationLibraryCode(string strLocation, string strLibraryCode)
        {
            if (strLocation == null)
                strLocation = "";
            if (strLibraryCode == null)
                strLibraryCode = "";

            // 这是总馆的阅览室名。
            if (strLocation.IndexOf("/") == -1)
            {
                // strLocation = “阅览室”  strLibraryCode=""
                if (string.IsNullOrEmpty(strLibraryCode) == true)
                    return true;
                return false;   // 否则其他任何馆代码都无法匹配
            }

            // 下面是 strLocation 为 "海淀分馆/阅览室" 这样的情况

            if (strLocation == strLibraryCode)
                return true;
            if (strLocation.StartsWith(strLibraryCode) == true)
            {
                // 一致的部分以后的第一个多出来的字符，应该是 '/' 才算匹配
                char ch = strLocation[strLibraryCode.Length];
                if (ch == '/')
                    return true;
            }

            return false;
        }

        // 正在编写中
        // 将 library.xml 中的(全部定义中)一个馆代码修改为指定的值
        public int ChangeLibraryCode(
    SessionInfo sessioninfo,
    string strOldLibraryCode,
    string strNewLibraryCode,
    out string strError)
        {
            strError = "";
            if (strOldLibraryCode == null)
                strOldLibraryCode = "";
            if (strNewLibraryCode == null)
                strNewLibraryCode = "";

            if (strOldLibraryCode == strNewLibraryCode)
                return 0;


            // TODO: 检查馆代码，不允许在末尾包含符号 '/'

            bool bChanged = false;

            // 读者库的 libraryCode 属性
            XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes("readerdbgroup/database");
            foreach (XmlElement database in nodes)
            {
                string strLibraryCode = database.GetAttribute("libraryCode");
                if (CompareLibraryCode(strLibraryCode, strOldLibraryCode))
                {
                    if (strLibraryCode == null)
                        strLibraryCode = "";
                    strLibraryCode = strNewLibraryCode + strLibraryCode.Substring(strOldLibraryCode.Length);
                    database.SetAttribute("libraryCode", strLibraryCode);
                    bChanged = true;
                }
            }

            /*
    <locationTypes>
        <item canborrow="no" itemBarcodeNullable="yes">保存本库</item>
        <item canborrow="no" itemBarcodeNullable="yes">阅览室</item>
        <item canborrow="yes" itemBarcodeNullable="yes">流通库</item>
        <item canborrow="yes" itemBarcodeNullable="yes">测试库</item>
        <library code="海淀分馆">
            <item canborrow="yes" itemBarcodeNullable="yes">流通库</item>
            <item canborrow="yes" itemBarcodeNullable="no">班级书架</item>
        </library>
    </locationTypes>
             * */

            // 1) 如果是从 "" --> "非空"，则需要把 locationTypes 直接下属的 item 元素移动到一个 library 元素下级
            // 2) 如果是从 "非空" --> ""，则需要把特定 library 元素的全部下级移动到 locationTypes 元素的直接下级，并删除刚才这个 library 元素
            // 3) 如果是其他情况，也就是 "非空1" --> "非空2"，则找到相应的 library 元素并修改 code 属性即可。
            // 不过需要考虑部分匹配馆代码的情况，也要修改

            {
                XmlElement locationTypes = this.LibraryCfgDom.DocumentElement.SelectSingleNode("locationTypes") as XmlElement;
                if (locationTypes == null)
                {
                    locationTypes = this.LibraryCfgDom.CreateElement("locationTypes");
                    this.LibraryCfgDom.DocumentElement.AppendChild(locationTypes);
                    bChanged = true;
                }

                // 1)
                if (string.IsNullOrEmpty(strOldLibraryCode)
                    && string.IsNullOrEmpty(strNewLibraryCode) == false)
                {
                    // 创建一个新的 library 元素
                    // 创建前要看看它是否已经存在
                    XmlElement library = locationTypes.SelectSingleNode("library[@code='" + strNewLibraryCode + "']") as XmlElement;
                    if (library == null)
                    {
                        library = this.LibraryCfgDom.CreateElement("library");
                        locationTypes.AppendChild(library);
                    }

                    // 把发现的元素都移动到 library 下面
                    XmlNodeList top_items = locationTypes.SelectNodes("item");
                    foreach (XmlElement item in top_items)
                    {
                        library.AppendChild(item);
                    }

                    bChanged = true;
                }

                // 2)
                else if (string.IsNullOrEmpty(strOldLibraryCode) == false
                    && string.IsNullOrEmpty(strNewLibraryCode) == true)
                {
                    XmlElement parent = null;
                    XmlNodeList items = locationTypes.SelectNodes("library[@code='" + strOldLibraryCode + "']/item");
                    foreach (XmlElement item in items)
                    {
                        if (parent == null)
                            parent = item.ParentNode as XmlElement;
                        locationTypes.AppendChild(item);
                        bChanged = true;
                    }

                    if (parent != null)
                        parent.ParentNode.RemoveChild(parent);

                }

                {
                    // 3)

                    // 无法用 xpath 进行定位，必须一个一个 library 元素判断其 code 属性
                    XmlNodeList librarys = locationTypes.SelectNodes("library");
                    foreach (XmlElement library in librarys)
                    {
                        string strLibraryCode = library.GetAttribute("code");
                        if (CompareLibraryCode(strLibraryCode, strOldLibraryCode))
                        {
                            if (strLibraryCode == null)
                                strLibraryCode = "";
                            strLibraryCode = strNewLibraryCode + strLibraryCode.Substring(strOldLibraryCode.Length);
                            library.SetAttribute("code", strLibraryCode);
                            bChanged = true;
                        }
                    }
                }

            }


            /* 排架体系。注意 location 元素 name 属性值包含通配符的情况
    <callNumber>
        <group name="中图法" classType="中图法" qufenhaoType="Cutter-Sanborn Three-Figure,GCAT" zhongcihaodb="" callNumberStyle="索取类号+区分号">
            <location name="保存本库" />
            <location name="阅览室" />
            <location name="流通库" />
        </group>
    </callNumber>
             * */
            XmlNodeList locations = this.LibraryCfgDom.DocumentElement.SelectNodes("callNumber/group/location");
            foreach (XmlElement location in locations)
            {
                string strLocation = location.GetAttribute("name");
                if (MatchLocationLibraryCode(strLocation, strOldLibraryCode))
                {
                    if (strLocation == null)
                        strLocation = "";
                    strLocation = strNewLibraryCode + strLocation.Substring(strOldLibraryCode.Length);
                    location.SetAttribute("name", strLocation);
                    bChanged = true;
                }
            }

            /*
<rightsTable>
        <type reader="本科生">
            <param name="可借总册数" value="10" />
            <param name="可预约册数" value="5" />
            <param name="以停代金因子" value="1.0" />
            <param name="工作日历名" value="基本日历" />
            <type book="普通">
                <param name="可借册数" value="10" />
                <param name="借期" value="31day,15day" />
                <param name="超期违约金因子" value="CNY1.0/day" />
...
        </type>
        <readerTypes>
            <item>本科生</item>
            <item>硕士生</item>
            <item>博士生</item>
            <item>讲师</item>
            <item>教授</item>
        </readerTypes>
        <bookTypes>
            <item>普通</item>
            <item>教材</item>
            <item>教学参考</item>
            <item>原版西文</item>
        </bookTypes>
        <library code="海淀分馆">
...
        </library>
    </rightsTable>
             * */

            // 1) 如果是从 "" --> "非空"，则需要把 rightsTable 直接下属的 除了 library 元素移动到一个 library 元素下级
            // 这个新的 library 元素，后面记住不再处理它
            // 2) 如果是从 "非空" --> ""，则需要把特定 library 元素的全部下级移动到 rightsTable 元素的直接下级，并删除刚才这个 library 元素
            // 3) 如果是其他情况，也就是 "非空1" --> "非空2"，则找到相应的 library 元素并修改 code 属性即可。
            // 如果 library 元素的 code 属性有空的情况，也用此法修改
            // 不过需要考虑部分匹配馆代码的情况，也要修改
            {
                XmlElement rightsTable = this.LibraryCfgDom.DocumentElement.SelectSingleNode("rightsTable") as XmlElement;
                if (rightsTable == null)
                {
                    rightsTable = this.LibraryCfgDom.CreateElement("rightsTable");
                    this.LibraryCfgDom.DocumentElement.AppendChild(rightsTable);
                    bChanged = true;
                }

                // 1)
                if (string.IsNullOrEmpty(strOldLibraryCode)
                    && string.IsNullOrEmpty(strNewLibraryCode) == false)
                {
                    // 创建一个新的 library 元素
                    // 创建前要看看它是否已经存在
                    XmlElement library = rightsTable.SelectSingleNode("library[@code='" + strNewLibraryCode + "']") as XmlElement;
                    if (library == null)
                    {
                        library = this.LibraryCfgDom.CreateElement("library");
                        rightsTable.AppendChild(library);
                    }

                    // 把发现的元素都移动到 library 下面
                    XmlNodeList top_items = rightsTable.SelectNodes("*");    // 所有不是 library 的元素
                    foreach (XmlElement item in top_items)
                    {
                        if (item.Name == "library")
                            continue;
                        library.AppendChild(item);
                    }

                    bChanged = true;
                }

                // 2)
                else if (string.IsNullOrEmpty(strOldLibraryCode) == false
                    && string.IsNullOrEmpty(strNewLibraryCode) == true)
                {
                    XmlElement parent = null;
                    XmlNodeList items = rightsTable.SelectNodes("library[@code='" + strOldLibraryCode + "']/*");
                    foreach (XmlElement item in items)
                    {
                        if (parent == null)
                            parent = item.ParentNode as XmlElement;
                        rightsTable.AppendChild(item);
                        bChanged = true;
                    }

                    if (parent != null)
                        parent.ParentNode.RemoveChild(parent);

                }

                {
                    // 3)

                    // 无法用 xpath 进行定位，必须一个一个 library 元素判断其 code 属性
                    XmlNodeList librarys = rightsTable.SelectNodes("library");
                    foreach (XmlElement library in librarys)
                    {
                        string strLibraryCode = library.GetAttribute("code");
                        if (CompareLibraryCode(strLibraryCode, strOldLibraryCode))
                        {
                            if (strLibraryCode == null)
                                strLibraryCode = "";
                            strLibraryCode = strNewLibraryCode + strLibraryCode.Substring(strOldLibraryCode.Length);
                            library.SetAttribute("code", strLibraryCode);
                            bChanged = true;
                        }
                    }
                }
            }

            if (bChanged == true)
                return 1;
            return 0;
        }

        public int ChangeKernelPassword(
            SessionInfo sessioninfo,
            string strOldPassword,
            string strNewPassword,
            out string strError)
        {
            strError = "";

            // 验证旧密码是否符合 library.xml 中的定义
            if (strOldPassword != this.ManagerPassword)
            {
                strError = "旧密码不吻合。修改 kernel 密码失败";
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            //		value == -1 出错
            //				 0  用户名或密码不正确
            //				 1  成功
            int nRet = channel.Login(this.ManagerUserName,
                strOldPassword,
                out strError);
            if (nRet == -1)
            {
                strError = "登录 dp2kernel 失败：" + strError;
                return -1;
            }
            if (nRet == 0)
            {
                strError = "旧密码和 dp2kernel 帐户不吻合。修改 kernel 密码失败";
                return -1;
            }

            // return:
            //		-1	出错。错误信息在strError中
            //		0	成功。
            nRet = channel.ChangePassword(this.ManagerUserName,
                strOldPassword,
                strNewPassword,
                false,
                out strError);
            if (nRet == -1)
            {
                strError = "在 dp2kernel 中修改密码失败：" + strError;
                return -1;
            }

            this.ManagerPassword = strNewPassword;
            this.Changed = true;
            return 0;
        }

        // 修改用户
        public int ChangeUser(
            string strLibraryCodeList,
            string strUserName,
            string strOperator,
            UserInfo userinfo,
            string strClientAddress,
            out ErrorCode error_code,
            out string strError)
        {
            strError = "";
            error_code = ErrorCode.NoError;
            int nRet = 0;

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName参数值不能为空";
                return -1;
            }

            if (strUserName != userinfo.UserName)
            {
                strError = "strUserName参数值和userinfo.UserName不一致";
                return -1;
            }

            // 2023/3/5
            // 检查 librarycode_list 中的馆代码是否都存在定义(libraries/library 元素)
            // return:
            //      -1  出错
            //      0   不存在定义，错误信息在 strError 中返回
            //      1   存在定义
            nRet = CheckLibraryCodeExist(userinfo.LibraryCode,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            XmlElement nodeAccount = null;
            string strOldOuterXml = "";

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']") as XmlElement;
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在 (2)";
                    error_code = ErrorCode.NotFound;
                    return -1;
                }

                strOldOuterXml = nodeAccount.OuterXml;

                string strExistLibraryCodeList = DomUtil.GetAttr(nodeAccount, "libraryCode");

                // 2012/9/9
                // 分馆用户只允许修改馆代码属于管辖分馆的帐户
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (string.IsNullOrEmpty(strExistLibraryCodeList) == true
                        || IsListInList(strExistLibraryCodeList, strLibraryCodeList) == false)
                    {
                        strError = $"{GetCurrentUserName(null)}只能修改图书馆代码完全属于 '{strLibraryCodeList}' 范围的用户信息";
                        error_code = ErrorCode.AccessDenied;
                        return -1;
                    }
                }

                // 2012/9/9
                // 分馆用户只允许将帐户的馆代码修改到指定范围内
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (string.IsNullOrEmpty(userinfo.LibraryCode) == true
                        || IsListInList(userinfo.LibraryCode, strLibraryCodeList) == false)
                    {
                        strError = $"{GetCurrentUserName(null)}只能将用户信息的馆代码修改到完全属于 '{strLibraryCodeList}' 范围内的值";
                        error_code = ErrorCode.AccessDenied;
                        return -1;
                    }
                }

                DomUtil.SetAttr(nodeAccount, "name", userinfo.UserName);
                DomUtil.SetAttr(nodeAccount, "type", userinfo.Type);
                DomUtil.SetAttr(nodeAccount, "rights", userinfo.Rights);
                DomUtil.SetAttr(nodeAccount, "libraryCode", userinfo.LibraryCode);
                DomUtil.SetAttr(nodeAccount, "access", userinfo.Access);
                DomUtil.SetAttr(nodeAccount, "comment", userinfo.Comment);
                DomUtil.SetAttr(nodeAccount, "binding", userinfo.Binding);
                DomUtil.SetAttr(nodeAccount, "location", userinfo.Location);

                bool neverExpire = StringUtil.IsInList("neverexpire", userinfo.Rights);

                // 强制修改密码。无需验证旧密码
                if (userinfo.SetPassword == true)
                {
                    // return:
                    //      -1  出错
                    //      0   不合法(原因在 strError 中返回)
                    //      1   合法
                    nRet = ValidateUserPassword(nodeAccount,
                        userinfo.Password,
                        _passwordStyle,
                        true,
                        out strError);
                    if (nRet != 1)
                    {
                        // 撤销全部修改
                        DomUtil.SetElementOuterXml(nodeAccount, strOldOuterXml);
                        return -1;
                    }

                    string strHashed = "";
                    string type = "bcrypt";
                    nRet = LibraryServerUtil.SetUserPassword(
                        type,
                        userinfo.Password,
                        out strHashed,
                        out strError);
                    if (nRet == -1)
                    {
                        // 撤销全部修改
                        DomUtil.SetElementOuterXml(nodeAccount, strOldOuterXml);
                        return -1;
                    }

                    // DomUtil.SetAttr(nodeAccount, "password", strHashed);
                    LibraryServerUtil.SetPasswordValue(nodeAccount,
                        type,
                        strHashed);

                    if (neverExpire == false
                        && LibraryServerUtil.IsSpecialUserName(userinfo.UserName) == false)
                        SetPasswordExpire(nodeAccount, _passwordExpirePeriod, DateTime.Now);
                }

                if (neverExpire)
                    ClearPasswordExpire(nodeAccount);
                else
                {
                    if (LibraryServerUtil.IsSpecialUserName(userinfo.UserName) == false)
                    {
                        // 观察以前是否有失效期。如果没有，则主动加上失效期
                        var old_expire = GetPasswordExpire(nodeAccount);
                        if (old_expire == DateTime.MaxValue)
                            SetPasswordExpire(nodeAccount, _passwordExpirePeriod, DateTime.Now);
                    }
                }

                this.Changed = true;

                // 2014/9/16
                if (userinfo.UserName == "reader")
                    this.ClearLoginCache("");
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }

            // 写入日志
            {
                XmlDocument domOperLog = PrepareOperlogDom("change", strOperator);

                if (string.IsNullOrEmpty(strOldOuterXml) == false)
                {
                    XmlElement node_old = domOperLog.CreateElement("oldAccount");
                    domOperLog.DocumentElement.AppendChild(node_old);
                    node_old = DomUtil.SetElementOuterXml(node_old, strOldOuterXml);
                    DomUtil.RenameNode(node_old,
                        null,
                        "oldAccount");
                }

                XmlElement node = domOperLog.CreateElement("account");
                domOperLog.DocumentElement.AppendChild(node);

                DomUtil.SetElementOuterXml(node, nodeAccount.OuterXml);

                // 写入日志
                nRet = this.OperLog.WriteOperLog(domOperLog,
                    strClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "SetUser() API 写入日志时发生错误: " + strError;
                    return -1;
                }
            }
            return 0;
        }

        // list1中的值是否全包含在list2中？
        static bool IsListInList(string strList1, string strList2)
        {
            string[] parts1 = strList1.Split(new char[] { ',' });
            string[] parts2 = strList2.Split(new char[] { ',' });

            int nCount = 0;
            foreach (string s1 in parts1)
            {
                string strText1 = s1.Trim();
                if (string.IsNullOrEmpty(strText1) == true)
                    continue;
                bool bFound = false;
                foreach (string s2 in parts2)
                {
                    string strText2 = s2.Trim();
                    if (string.IsNullOrEmpty(strText2) == true)
                        continue;
                    if (strText1 == strText2)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    return false;

                nCount++;
            }

            if (nCount == 0)
                return false;

            return true;
        }

        // 强制修改用户密码。不修改其他信息。
        public int ResetUserPassword(
            string strLibraryCodeList,
            string strUserName,
            string strOperator,
            string strNewPassword,
            string strClientAddress,
            out ErrorCode error_code,
            out string strError)
        {
            strError = "";
            error_code = ErrorCode.NoError;
            int nRet = 0;

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName参数值不能为空";
                return -1;
            }

            XmlElement nodeAccount = null;
            string strHashedPassword = "";

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']") as XmlElement;
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在 (3)";
                    error_code = ErrorCode.NotFound;
                    return -1;
                }

                string strExistLibraryCodeList = DomUtil.GetAttr(nodeAccount, "libraryCode");

                // 2012/9/9
                // 分馆用户只允许修改馆代码属于管辖分馆的帐户
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (string.IsNullOrEmpty(strExistLibraryCodeList) == true
                        || IsListInList(strExistLibraryCodeList, strLibraryCodeList) == false)
                    {
                        strError = $"{GetCurrentUserName(null)}只能重设 图书馆代码完全属于 '{strLibraryCodeList}' 范围的用户的密码";
                        error_code = ErrorCode.AccessDenied;
                        return -1;
                    }
                }

                // return:
                //      -1  出错
                //      0   不合法(原因在 strError 中返回)
                //      1   合法
                nRet = ValidateUserPassword(nodeAccount,
                    strNewPassword,
                    _passwordStyle,
                    true,
                    out strError);
                if (nRet != 1)
                    return -1;

                // 强制修改密码。无需验证旧密码
                string type = "bcrypt";
                nRet = LibraryServerUtil.SetUserPassword(
                    type,
                    strNewPassword,
                    out strHashedPassword,
                    out strError);
                if (nRet == -1)
                    return -1;
                // DomUtil.SetAttr(nodeAccount, "password", strHashedPassword);
                LibraryServerUtil.SetPasswordValue(nodeAccount,
                    type,
                    strHashedPassword);
                if (LibraryServerUtil.IsSpecialUserName(strUserName) == false)
                    SetPasswordExpire(nodeAccount, _passwordExpirePeriod, DateTime.Now);
                this.Changed = true;
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }

            {
                XmlDocument domOperLog = PrepareOperlogDom("resetpassword", strOperator);

                // 2015/10/17 新增加的元素。此前缺这个元素。建议日志恢复的时候，忽略没有 userName 元素的日志记录
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "userName",
                    strUserName);
#if NO
                XmlNode node = domOperLog.CreateElement("newPassword");
                domOperLog.DocumentElement.AppendChild(node);

                node.InnerText = strHashedPassword;
#endif
                DomUtil.SetElementText(domOperLog.DocumentElement,
                    "newPassword",
                    strHashedPassword);

                // 写入日志
                nRet = this.OperLog.WriteOperLog(domOperLog,
                    strClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "SetUser() API 写入日志时发生错误: " + strError;
                    return -1;
                }
            }

            return 0;
        }

        // 删除用户
        public int DeleteUser(
            string strLibraryCodeList,
            string strUserName,
            string strOperator,
            string strClientAddress,
            out ErrorCode error_code,
            out string strError)
        {
            strError = "";
            error_code = ErrorCode.NoError;

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName参数值不能为空";
                return -1;
            }

            XmlElement nodeAccount = null;
            string strOldOuterXml = "";

            // this.m_lock.AcquireWriterLock(m_nLockTimeout);
            this.LockForWrite();    // 2016/10/16
            try
            {
                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("accounts/account[@name='" + strUserName + "']") as XmlElement;
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在 (4)";
                    error_code = ErrorCode.NotFound;
                    return -1;
                }
                strOldOuterXml = nodeAccount.OuterXml;

                string strExistLibraryCodeList = DomUtil.GetAttr(nodeAccount, "libraryCode");

                // 2012/9/9
                // 分馆用户只允许删除馆代码属于管辖分馆的帐户
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (string.IsNullOrEmpty(strExistLibraryCodeList) == true
                        || IsListInList(strExistLibraryCodeList, strLibraryCodeList) == false)
                    {
                        strError = $"{GetCurrentUserName(null)}只能删除 图书馆代码完全属于 '{strLibraryCodeList}' 范围的用户";
                        error_code = ErrorCode.AccessDenied;
                        return -1;
                    }
                }

                nodeAccount.ParentNode.RemoveChild(nodeAccount);

                this.Changed = true;

                // 2014/9/16
                if (strUserName == "reader")
                    this.ClearLoginCache("");
            }
            finally
            {
                // this.m_lock.ReleaseWriterLock();
                this.UnlockForWrite();
            }

            {
                XmlDocument domOperLog = PrepareOperlogDom("delete", strOperator);

                if (string.IsNullOrEmpty(strOldOuterXml) == false)
                {
                    XmlElement node_old = domOperLog.CreateElement("oldAccount");
                    domOperLog.DocumentElement.AppendChild(node_old);
                    node_old = DomUtil.SetElementOuterXml(node_old, strOldOuterXml);
                    DomUtil.RenameNode(node_old,
                        null,
                        "oldAccount");
                }

                // 写入日志
                int nRet = this.OperLog.WriteOperLog(domOperLog,
                    strClientAddress,
                    out strError);
                if (nRet == -1)
                {
                    strError = "SetUser() API 写入日志时发生错误: " + strError;
                    error_code = ErrorCode.SystemError; // TODO: 建议增加一个日志写入错误的错误码
                    return -1;
                }
            }

            return 0;
        }

        // 包装
        public int SetUser(
            string strLibraryCodeList,
            string strAction,
            string strOperator,
            UserInfo info,
            string strClientAddress,
            out ErrorCode error_code,
            out string strError)
        {
            error_code = ErrorCode.NoError;

            // 2021/8/20
            // 检查 info 内容合法性
            if (info != null)
            {
                if (info.Rights != null
                    && info.Rights.IndexOfAny(new char[] { '\r', '\n' }) != -1)
                {
                    error_code = ErrorCode.InvalidParameter;
                    strError = $"info.Rights 内容 '{info.Rights}' 不合法：含有回车或者换行字符";
                    return -1;
                }
            }

            if (strAction == "new")
            {
                return this.CreateUser(strLibraryCodeList,
                    info.UserName,
                    strOperator,
                    info,
                    strClientAddress,
                    out error_code,
                    out strError);
            }

            if (strAction == "change")
            {
                return this.ChangeUser(strLibraryCodeList,
                    info.UserName,
                    strOperator,
                    info,
                    strClientAddress,
                    out error_code,
                    out strError);
            }

            if (strAction == "resetpassword")
            {
                return this.ResetUserPassword(strLibraryCodeList,
                    info.UserName,
                    strOperator,
                    info.Password,
                    strClientAddress,
                    out error_code,
                    out strError);
            }

            if (strAction == "delete")
            {
                return this.DeleteUser(strLibraryCodeList,
                    info.UserName,
                    strOperator,
                    strClientAddress,
                    out error_code,
                    out strError);
            }

            strError = "未知的动作 '" + strAction + "'";
            return -1;
        }

        static string[] _rights_replace_table = new string[] {
        "listbibliodbfroms-->listdbfroms",
        "setentities-->setiteminfo",
        "setissues-->setissueinfo",
        "setorders-->setorderinfo",
        // "setcomments-->setcommentinfo",
        "getentities-->getiteminfo",
        "getissues-->getissueinfo",
        "getorders-->getoderinfo",
        "getcomments-->getcommentinfo",
        "writeobject-->setobject",
        "getres-->getobject",   // 旧版本的 getres 大致对等于新版的 getobject。getres 权限已经废止
        };

        // 将旧版本的账户权限字符串升级到新版本
        // 1) 将一些别名权限替换为正式名字
        static string UpgradeUserRights(string rights)
        {
            List<string> results = new List<string>();
            var source_rights = rights.Split(',');
            foreach (var source_right in source_rights)
            {
                results.Add(Replace(source_right));
            }

            StringUtil.RemoveDupNoSort(ref results);
            return string.Join(",", results);
            string Replace(string text)
            {
                foreach (var item in _rights_replace_table)
                {
                    var parts = StringUtil.ParseTwoPart(item, "-->");
                    if (text == parts[0])
                        return parts[1];
                }

                return text;
            }
        }

        static string[] _db_type_table = new string[] {
        "biblio",
        "reader",
        "item",
        "order",
        "issue",
        "comment",
        "amerce",
        "arrived",
        "publisher",
        "zhongcihao",
        "dictionary",
        "inventory",
        };

        // 确保 setxxxobject 具有配套的 getxxxobject，如果没有，则添加上
        // return:
        //      -1  出错
        //      0   没有发生增补
        //      1   发生了增补，strError 中返回了增补情况文字描述
        static int ExpandGetXXXObject(string origin_rights,
            out string output_rights,
            out string strError)
        {
            strError = "";
            output_rights = origin_rights;

            List<string> results = new List<string>();
            List<string> append_list = new List<string>();
            var list = origin_rights.Split(',');
            foreach (string origin in list)
            {
                results.Add(origin);
                var get_right = GetXXXRight(origin);
                if (get_right != null)
                {
                    if (StringUtil.IsInList(get_right, origin_rights) == false)
                    {
                        results.Add(get_right);
                        append_list.Add(get_right);
                    }
                }
            }

            StringUtil.RemoveDupNoSort(ref results);
            output_rights = string.Join(",", results);
            if (append_list.Count > 0)
            {
                strError = $"增补了下列权限: {StringUtil.MakePathList(append_list)}";
                return 1;
            }

            return 0;

            string GetXXXRight(string right)
            {
                if (right == "setobject")
                    return "getobject";
                foreach (var db_type in _db_type_table)
                {
                    if ($"set{db_type}object" == right)
                        return $"get{db_type}object";
                }

                return null;
            }
        }
    }

    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class UserInfo
    {
        [DataMember]
        public string UserName = "";    // 用户名

        [DataMember]
        public bool SetPassword = false;    // 是否设置密码
        [DataMember]
        public string Password = "";    // 密码

        [DataMember]
        public string Rights = "";  // 权限值
        [DataMember]
        public string Type = "";    // 账户类型

        [DataMember]
        public string LibraryCode = ""; // 图书馆代码 2007/12/15 

        [DataMember]
        public string Access = "";  // 关于存取权限的定义 2008/2/28 

        [DataMember]
        public string Comment = "";  // 注释 2012/10/8

        [DataMember]
        public string Binding = ""; // 绑定 2016/6/15

        [DataMember]
        public string Location = "";    // 默认位置 2022/2/21
    }
}
