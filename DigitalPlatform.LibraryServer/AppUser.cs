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
                new string [] {"()"},
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

                for (int j = 0; j<sections.Count; j++)
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
                return 0;   // not found

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
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
            try
            {

                strError = "";
                userinfos = null;

                string strXPath = "";

                if (String.IsNullOrEmpty(strUserName) == true)
                {
                    strXPath = "//accounts/account";
                }
                else
                {
                    strXPath = "//accounts/account[@name='" + strUserName + "']";
                }


                List<UserInfo> userList = new List<UserInfo>();

                XmlNodeList nodes = this.LibraryCfgDom.DocumentElement.SelectNodes(strXPath);

                // 过滤为当前能管辖的小范围node数组
                List<XmlNode> smallerlist = new List<XmlNode>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

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
                    XmlNode node = smallerlist[i];

                    string strCurrentLibraryCodeList = DomUtil.GetAttr(node, "libraryCode");

                    UserInfo userinfo = new UserInfo();
                    userinfo.UserName = DomUtil.GetAttr(node, "name");
                    userinfo.Type = DomUtil.GetAttr(node, "type");
                    userinfo.Rights = DomUtil.GetAttr(node, "rights");
                    userinfo.LibraryCode = strCurrentLibraryCodeList;
                    userinfo.Access = DomUtil.GetAttr(node, "access");
                    userinfo.Comment = DomUtil.GetAttr(node, "comment");

                    userList.Add(userinfo);
                }

                userinfos = new UserInfo[userList.Count];
                userList.CopyTo(userinfos);

                return smallerlist.Count;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }
        }

        // 创建新用户
        // TODO: 对DOM加锁
        public int CreateUser(
            string strLibraryCodeList,
            string strUserName,
            string strOperator,
            UserInfo userinfo,
            string strClientAddress,
            out string strError)
        {
            strError = "";

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

            // 2012/9/9
            // 分馆用户只允许创建馆代码属于管辖分馆的帐户
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
            {
                if (string.IsNullOrEmpty(userinfo.LibraryCode) == true
                    || IsListInList(userinfo.LibraryCode, strLibraryCodeList) == false)
                {
                    strError = "当前用户只能创建图书馆代码完全属于 '" + strLibraryCodeList + "' 范围的新用户";
                    return -1;
                }
            }

            int nResultValue = -1;
            // 检查名字空间。
            // return:
            //      -2  not found script
            //      -1  出错
            //      0   成功
            int nRet = this.DoVerifyBarcodeScriptFunction(
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
            XmlNode nodeAccount = null;

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//accounts/account[@name='" + strUserName + "']");
                if (nodeAccount != null)
                {
                    strError = "用户 '" + strUserName + "' 已经存在";
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

                DomUtil.SetAttr(nodeAccount, "name", userinfo.UserName);

                if (String.IsNullOrEmpty(userinfo.Type) == false)
                    DomUtil.SetAttr(nodeAccount, "type", userinfo.Type);

                DomUtil.SetAttr(nodeAccount, "rights", userinfo.Rights);

                DomUtil.SetAttr(nodeAccount, "libraryCode", userinfo.LibraryCode);

                DomUtil.SetAttr(nodeAccount, "access", userinfo.Access);

                DomUtil.SetAttr(nodeAccount, "comment", userinfo.Comment);

                // 设置密码
                if (userinfo.SetPassword == true)
                {
                    string strPassword = Cryptography.Encrypt(userinfo.Password,
                        EncryptKey);
                    DomUtil.SetAttr(nodeAccount, "password", strPassword);
                }

                this.Changed = true;

                // 2014/9/16
                if (userinfo.UserName == "reader")
                    this.ClearLoginCache("");
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            {
                XmlDocument domOperLog = PrepareOperlogDom("new", strOperator);
                XmlNode node = domOperLog.CreateElement("account");
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
            this.m_lock.AcquireReaderLock(m_nLockTimeout);
            try
            {
                // 查重
                XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//accounts/account[@name='" + strUserName + "']");
                if (node != null)
                    return true;

                return false;
            }
            finally
            {
                this.m_lock.ReleaseReaderLock();
            }
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
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName参数值不能为空";
                return -1;
            }

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                // 查重
                XmlNode node = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//accounts/account[@name='" + strUserName + "']");
                if (node == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在";
                    return -1;
                }

                string strExistLibraryCodeList = DomUtil.GetAttr(node, "libraryCode");

                // 2012/9/9
                // 分馆用户只允许修改馆代码属于管辖分馆的帐户
                if (SessionInfo.IsGlobalUser(strLibraryCodeList) == false)
                {
                    if (string.IsNullOrEmpty(strExistLibraryCodeList) == true
                        || IsListInList(strExistLibraryCodeList, strLibraryCodeList) == false)
                    {
                        strError = "当前用户只能修改图书馆代码完全完全属于 '" + strLibraryCodeList + "' 范围的用户的密码";
                        return -1;
                    }
                }

                // 验证旧密码
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

                // 设置新密码
                strNewPassword = Cryptography.Encrypt(strNewPassword,
                        EncryptKey);
                DomUtil.SetAttr(node, "password", strNewPassword);

                this.Changed = true;

                return 0;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            // return 0;
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
            out string strError)
        {
            strError = "";

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

            XmlNode nodeAccount = null;
            string strOldOuterXml = "";

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//accounts/account[@name='" + strUserName + "']");
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在";
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
                        strError = "当前用户只能修改图书馆代码完全属于 '" + strLibraryCodeList + "' 范围的用户信息";
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
                        strError = "当前用户只能将用户信息的馆代码修改到完全属于 '" + strLibraryCodeList + "' 范围内的值";
                        return -1;
                    }
                }

                DomUtil.SetAttr(nodeAccount, "name", userinfo.UserName);
                DomUtil.SetAttr(nodeAccount, "type", userinfo.Type);
                DomUtil.SetAttr(nodeAccount, "rights", userinfo.Rights);
                DomUtil.SetAttr(nodeAccount, "libraryCode", userinfo.LibraryCode);
                DomUtil.SetAttr(nodeAccount, "access", userinfo.Access);
                DomUtil.SetAttr(nodeAccount, "comment", userinfo.Comment);

                // 强制修改密码。无需验证旧密码
                if (userinfo.SetPassword == true)
                {
                    string strPassword = Cryptography.Encrypt(userinfo.Password,
                        EncryptKey);
                    DomUtil.SetAttr(nodeAccount, "password", strPassword);
                }

                this.Changed = true;

                // 2014/9/16
                if (userinfo.UserName == "reader")
                    this.ClearLoginCache("");
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            {
                XmlDocument domOperLog = PrepareOperlogDom("change", strOperator);

                if (string.IsNullOrEmpty(strOldOuterXml) == false)
                {
                    XmlNode node_old = domOperLog.CreateElement("oldAccount");
                    domOperLog.DocumentElement.AppendChild(node_old);
                    node_old = DomUtil.SetElementOuterXml(node_old, strOldOuterXml);
                    DomUtil.RenameNode(node_old,
                        null,
                        "oldAccount");
                }

                XmlNode node = domOperLog.CreateElement("account");
                domOperLog.DocumentElement.AppendChild(node);

                DomUtil.SetElementOuterXml(node, nodeAccount.OuterXml);

                // 写入日志
                int nRet = this.OperLog.WriteOperLog(domOperLog,
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
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName参数值不能为空";
                return -1;
            }

            XmlNode nodeAccount = null;
            string strHashedPassword = "";

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//accounts/account[@name='" + strUserName + "']");
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在";
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
                        strError = "当前用户只能重设 图书馆代码完全属于 '" + strLibraryCodeList + "' 范围的用户的密码";
                        return -1;
                    }
                }

                // 强制修改密码。无需验证旧密码
                strHashedPassword = Cryptography.Encrypt(strNewPassword,
                    EncryptKey);
                DomUtil.SetAttr(nodeAccount, "password", strHashedPassword);

                this.Changed = true;
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }

            {
                XmlDocument domOperLog = PrepareOperlogDom("resetpassword", strOperator);

                XmlNode node = domOperLog.CreateElement("newPassword");
                domOperLog.DocumentElement.AppendChild(node);

                node.InnerText = strHashedPassword;

                // 写入日志
                int nRet = this.OperLog.WriteOperLog(domOperLog,
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
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strError = "strUserName参数值不能为空";
                return -1;
            }

            XmlNode nodeAccount = null;
            string strOldOuterXml = "";

            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {

                // 查重
                nodeAccount = this.LibraryCfgDom.DocumentElement.SelectSingleNode("//accounts/account[@name='" + strUserName + "']");
                if (nodeAccount == null)
                {
                    strError = "用户 '" + strUserName + "' 不存在";
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
                        strError = "当前用户只能删除 图书馆代码完全属于 '" + strLibraryCodeList + "' 范围的用户";
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
                this.m_lock.ReleaseWriterLock();
            }

            {
                XmlDocument domOperLog = PrepareOperlogDom("delete", strOperator);

                if (string.IsNullOrEmpty(strOldOuterXml) == false)
                {
                    XmlNode node_old = domOperLog.CreateElement("oldAccount");
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
            out string strError)
        {
            if (strAction == "new")
            {
                return this.CreateUser(strLibraryCodeList, 
                    info.UserName,
                    strOperator,
                    info,
                    strClientAddress,
                    out strError);
            }

            if (strAction == "change")
            {
                return this.ChangeUser(strLibraryCodeList, 
                    info.UserName,
                    strOperator,
                    info,
                    strClientAddress,
                    out strError);
            }

            if (strAction == "resetpassword")
            {
                return this.ResetUserPassword(strLibraryCodeList, 
                    info.UserName,
                    strOperator,
                    info.Password,
                    strClientAddress,
                    out strError);
            }

            if (strAction == "delete")
            {
                return this.DeleteUser(strLibraryCodeList, 
                    info.UserName,
                    strOperator,
                    strClientAddress,
                    out strError);
            }

            strError = "未知的动作 '" + strAction + "'";
            return -1;
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
        public string LibraryCode = ""; // 图书馆代码 2007/12/15 new add

        [DataMember]
        public string Access = "";  // 关于存取权限的定义 2008/2/28 new add

        [DataMember]
        public string Comment = "";  // 注释 2012/10/8
    }
}
