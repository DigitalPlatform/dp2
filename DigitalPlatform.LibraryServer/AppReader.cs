
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
    /// 本部分是读者相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 读者记录中 要害元素名列表
        static string[] reader_element_names = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",   // 2013/12/20
                "gender",
                "birthday",
                "dateOfBirth",
                "idCardNumber",
                "department",
                "post", // 2009/7/17 
                "address",
                "tel",
                "email",
                "comment",
                "zhengyuan",
                "hire",
                "cardNumber",   // 借书证号。为和原来的(100$b)兼容，也为了将来放RFID卡号 2008/10/14 
                "foregift", // 押金。2008/11/11 
                "displayName",  // 显示名
                "preference",   // 个性化参数
                "outofReservations",    // 预约未取参数
                "nation",   // 2011/9/24
                "fingerprint", // 2012/1/15
                "rights", // 2014/7/8
                "personalLibrary", // 2014/7/8
                "friends", // 2014/9/9
                "access",   // 2014/9/10
                "refID", // 2015/9/12
            };

        // 读者记录中 读者自己能修改的元素名列表
        static string[] selfchangeable_reader_element_names = new string[] {
                "displayName",  // 显示名
                "preference",   // 个性化参数
            };

        // 删除target中的全部<dprms:file>元素，然后将source记录中的全部<dprms:file>元素插入到target记录中
        public static void MergeDprmsFile(ref XmlDocument domTarget,
            XmlDocument domSource)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            // 删除target中的全部<dprms:file>元素
            XmlNodeList nodes = domTarget.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node.ParentNode != null)
                    node.ParentNode.RemoveChild(node);
            }

            // 然后将source记录中的全部<dprms:file>元素插入到target记录中
            nodes = domSource.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                XmlDocumentFragment fragment = domTarget.CreateDocumentFragment();
                fragment.InnerXml = node.OuterXml;

                domTarget.DocumentElement.AppendChild(fragment);
            }
        }

#if NO
        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
        public bool IsCurrentChangeableReaderPath(string strReaderRecPath,
            string strAccountLibraryCodeList)
        {
            string strDbName = ResPath.GetDbName(strReaderRecPath);
            if (string.IsNullOrEmpty(strDbName) == true)
                return false;

            List<string> dbnames = GetCurrentReaderDbNameList(strAccountLibraryCodeList);
            if (dbnames.IndexOf(strDbName) != -1)
                return true;
            return false;
        }
#endif
        // 包装后的版本
        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
        public bool IsCurrentChangeableReaderPath(string strReaderRecPath,
            string strAccountLibraryCodeList)
        {
            string strLibraryCode = "";
            return IsCurrentChangeableReaderPath(strReaderRecPath,
                strAccountLibraryCodeList,
                out strLibraryCode);
        }

        // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内? 戍边获得读者库(strReaderRecPath)的馆代码
        public bool IsCurrentChangeableReaderPath(string strReaderRecPath,
            string strAccountLibraryCodeList,
            out string strLibraryCode)
        {
            strLibraryCode = "";

            string strDbName = ResPath.GetDbName(strReaderRecPath);
            if (string.IsNullOrEmpty(strDbName) == true)
                return false;

            if (IsReaderDbName(strDbName, out strLibraryCode) == false)
                return false;

            if (SessionInfo.IsGlobalUser(strAccountLibraryCodeList) == true)
                return true;

            if (StringUtil.IsInList(strLibraryCode, strAccountLibraryCodeList) == true)
                return true;

            return false;
        }

        // 获得当前用户能管辖的读者库名列表
        public List<string> GetCurrentReaderDbNameList(string strAccountLibraryCodeList)
        {
            List<string> dbnames = new List<string>();
            for (int i = 0; i < this.ReaderDbs.Count; i++)
            {
                string strDbName = this.ReaderDbs[i].DbName;
                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                if (string.IsNullOrEmpty(strAccountLibraryCodeList) == false)
                {
                    string strLibraryCode = this.ReaderDbs[i].LibraryCode;
                    // 匹配图书馆代码
                    // parameters:
                    //      strSingle   单个图书馆代码。空的总是不能匹配
                    //      strList     图书馆代码列表，例如"第一个,第二个"，或者"*"。空表示都匹配
                    // return:
                    //      false   没有匹配上
                    //      true    匹配上
                    if (LibraryApplication.MatchLibraryCode(strLibraryCode, strAccountLibraryCodeList) == false)
                        continue;
                }

                dbnames.Add(strDbName);
            }

            return dbnames;
        }

        // 匹配图书馆代码
        // parameters:
        //      strSingle   单个图书馆代码。空的总是不能匹配
        //      strList     图书馆代码列表，例如"第一个,第二个"，或者"*"。一个星号或者空表示都匹配
        // return:
        //      false   没有匹配上
        //      true    匹配上
        public static bool MatchLibraryCode(string strSingle, string strList)
        {
            if (string.IsNullOrEmpty(strSingle) == true
                && SessionInfo.IsGlobalUser(strList) == true)
                return true;

            if (string.IsNullOrEmpty(strSingle) == true)
                return false;
            if (SessionInfo.IsGlobalUser(strList) == true)
                return true;
            string[] parts = strList.Split(new char[] {','});
            foreach (string s in parts)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;
                string strOne = s.Trim();
                if (string.IsNullOrEmpty(strOne) == true)
                    continue;
                if (strOne == "*")
                    return true;
                if (strOne == strSingle)
                    return true;
            }

            return false;
        }

        // 将元素名<birthday>替换为<dateOfBirth>
        static bool RenameBirthday(XmlDocument dom)
        {
            if (dom == null || dom.DocumentElement == null)
                return false;

            bool bChanged = false;
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//birthday");
            foreach (XmlNode node in nodes)
            {
                XmlNode nodeNew = dom.CreateElement("dateOfBirth");
                if (node != dom.DocumentElement)
                {
                    node.ParentNode.InsertBefore(nodeNew, node);

                    nodeNew.InnerXml = node.InnerXml;
                    node.ParentNode.RemoveChild(node);
                    bChanged = true;
                }
            }

            return bChanged;
        }

        // <DoReaderChange()的下级函数>
        // 合并新旧记录
        static int MergeTwoReaderXml(
            string [] reader_element_names,
            string strAction,
            XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            if (strAction == "change"
                || strAction == "changereaderbarcode")
            {
                /*
                // 要害元素名列表
                string[] reader_element_names = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",   // 2013/12/20
                "gender",
                "birthday",
                "dateOfBirth",
                "idCardNumber",
                "department",
                "post", // 2009/7/17 
                "address",
                "tel",
                "email",
                "comment",
                "zhengyuan",
                "hire",
                "cardNumber",   // 借书证号。为和原来的(100$b)兼容，也为了将来放RFID卡号  2008/10/14 
            };
                */
                RenameBirthday(domExist);
                RenameBirthday(domNew);

                // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中

                for (int i = 0; i < reader_element_names.Length; i++)
                {
                    string strElementName = reader_element_names[i];
                    // <foregift>元素内容不让SetReaderInfo() API的change action修改
                    if (strElementName == "foregift")
                        continue;

                    // 2006/11/29 changed
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        strElementName);

                    // 2013/1/15 <fingerprint>元素单独处理
                    if (strElementName == "fingerprint")
                    {
                        string strTextOld = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                            strElementName);
                        // 如果元素文本或者属性发生变化
                        if (strTextNew != strTextOld)
                        {
                            DomUtil.SetElementOuterXml(domExist.DocumentElement,
    strElementName,
    strTextNew);
                            // 刷新timestamp属性
                            XmlNode node = domExist.DocumentElement.SelectSingleNode(strElementName);
                            if (node != null)
                                DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));
                        }
                        continue;
                    }

                    // 2013/6/19 <hire>元素单独处理
                    // 保护 expireDate 属性不被修改
                    if (strElementName == "hire")
                    {
                        XmlNode nodeExist = domExist.DocumentElement.SelectSingleNode("hire");
                        // XmlNode nodeNew = domNew.DocumentElement.SelectSingleNode("hire");

                        string strExistExpireDate = "";
                        if (nodeExist != null)
                            strExistExpireDate = DomUtil.GetAttr(nodeExist, "expireDate");

                        DomUtil.SetElementOuterXml(domExist.DocumentElement,
                            strElementName,
                            strTextNew);

                        // 将 expireDate 覆盖回去
                        nodeExist = domExist.DocumentElement.SelectSingleNode("hire");
                        if (nodeExist != null)
                            DomUtil.SetAttr(domExist.DocumentElement, "expireDate", strExistExpireDate);
                        else if (string.IsNullOrEmpty(strExistExpireDate) == false)
                        {
                            XmlNode node = DomUtil.SetElementText(domExist.DocumentElement,
                                strElementName,
                                "");
                            DomUtil.SetAttr(node, "expireDate", strExistExpireDate);
                        }

                        continue;
                    }

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        strElementName,
                        strTextNew);
                }

                // 删除target中的全部<dprms:file>元素，然后将source记录中的全部<dprms:file>元素插入到target记录中
                MergeDprmsFile(ref domExist,
                    domNew);

            }
            else if (strAction == "changestate")
            {
                string[] element_names_onlystate = new string[] {
                    "state",
                    "comment",
                    };
                for (int i = 0; i < element_names_onlystate.Length; i++)
                {
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        element_names_onlystate[i]);

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        element_names_onlystate[i],
                        strTextNew);

                }

                // 不修改<dprms:file>
            }
            else if (strAction == "changeforegift")
            {
                // 2008/11/11 
                string[] element_names_onlyforegift = new string[] {
                    "foregift",
                    "comment",
                    };
                for (int i = 0; i < element_names_onlyforegift.Length; i++)
                {
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        element_names_onlyforegift[i]);

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        element_names_onlyforegift[i],
                        strTextNew);

                }

                // 不修改<dprms:file>
            }
            else
            {
                strError = "strAction值必须为change、changestate、changeforegift 和 changereaderbarcode 之一。";
                return -1;
            }

            // 2015/9/12
            // 如果记录中没有 refID 字段，则主动填充
            string strRefID = DomUtil.GetElementText(domExist.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID) == true)
                DomUtil.SetElementText(domExist.DocumentElement, "refID", Guid.NewGuid().ToString());

            strMergedXml = domExist.OuterXml;
            return 0;
        }

        // 构造出适合保存的新读者记录
        // 主要是为了把待加工的记录中，可能出现的属于“流通信息”的字段去除，避免出现安全性问题
        // return:
        //      -1  出错
        //      0   没有实质性修改
        //      1   发生了实质性修改
        static int BuildNewReaderRecord(XmlDocument domNewRec,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            // 流通元素名列表
            string[] element_names = new string[] {
                "borrows",
                "overdues",
                "reservations",
                "borrowHistory",
                "outofReservations",
                "hire", // 2008/11/11 
                "foregift", // 2008/11/11 
            };

            // TODO: 需要测试本函数，看看<hire>元素的属性值真能去掉么?

            XmlDocument dom = new XmlDocument();

            dom.LoadXml(domNewRec.OuterXml);

            RenameBirthday(dom);

            bool bChanged = false;
            for (int i = 0; i < element_names.Length; i++)
            {
                List<XmlNode> deleted_nodes = DomUtil.DeleteElements(dom.DocumentElement,
                    element_names[i]);
                if (deleted_nodes != null
                    && deleted_nodes.Count > 0)
                    bChanged = true;
            }

            // 如果有已经有了<fingerprint>元素，则修正其timestamp属性
            // 刷新timestamp属性
            XmlNode node = dom.DocumentElement.SelectSingleNode("fingerprint");
            if (node != null)
                DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));

            // TODO: 设置首次密码
            string strBirthDate = DomUtil.GetElementText(dom.DocumentElement, "dateOfBirth");
            string strNewPassword = "";
            try
            {
                if (string.IsNullOrEmpty(strBirthDate) == false)
                    strNewPassword = DateTimeUtil.DateTimeToString8(DateTimeUtil.FromRfc1123DateTimeString(strBirthDate));
            }
            catch (Exception ex)
            {
                strError = "出生日期字段值 '" + strBirthDate + "' 不合法: " + ex.Message;
                return -1;
            }

            XmlDocument domOperLog = null;
            // 修改读者密码
            // return:
            //      -1  error
            //      0   成功
            int nRet = ChangeReaderPassword(
                dom,
                strNewPassword,
                ref domOperLog,
                out strError);
            if (nRet == -1)
            {
                strError = "初始化读者记录密码时出错: " + strError;
                return -1;
            }

            // 2015/9/12
            // 如果记录中没有 refID 字段，则主动填充
            string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID) == true)
                DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());

            strXml = dom.OuterXml;
            if (bChanged == true)
                return 1;

            return 0;
        }

        // <DoReaderChange()的下级函数>
        // 比较两个记录, 看看和读者静态信息有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        static int IsReaderInfoChanged(
            string [] reader_element_names,
            XmlDocument dom1,
            XmlDocument dom2)
        {
            for (int i = 0; i < reader_element_names.Length; i++)
            {
                /*
                string strText1 = DomUtil.GetElementText(dom1.DocumentElement,
                    element_names[i]);
                string strText2 = DomUtil.GetElementText(dom2.DocumentElement,
                    element_names[i]);
                 * */
                // 2006/11/29 changed
                string strText1 = DomUtil.GetElementOuterXml(dom1.DocumentElement,
                    reader_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(dom2.DocumentElement,
                    reader_element_names[i]);


                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }


        // 修改读者记录
        // TODO: 是否要提供条码号重的情况下强制写入的功能？
        // 需要一并发来旧记录的原因, 是为了和数据库中当前可能已经变化了的记录进行比较，
        // 如果SetReaderInfo能覆盖的部分字段，这一部分没有发生实质性变化，整条记录仅仅是
        // 流通实时信息发生了变化，本函数就能仍适当合并后保存记录，而不会返回错误，增加
        // 了API的可用性。如果实际运用中不允许发回旧记录，可发来空字符串，就会牺牲上述
        // 可用性，变成，不论数据库中当前记录的改变具体在那些字段范围，都只能报错返回了。
        // paramters:
        //      strAction    操作。new change delete changestate changeforegift forcenew forcechange forcedelete changereaderbarcode
        //      strRecPath  希望保存到的记录路径。可以为空。
        //      strNewXml   希望保存的记录体
        //      strOldXml   原先获得的旧记录体。可以为空。
        //      baOldTimestamp  原先获得旧记录的时间戳。可以为空。
        //      strExistringXml 覆盖操作失败时，返回数据库中已经存在的记录，供前端参考
        //      strSavedXml 实际保存的新记录。内容可能和strNewXml有所差异。
        //      strSavedRecPath 实际保存的记录路径
        //      baNewTimestamp  实际保存后的新时间戳
        // return:
        //      result -1失败 0 正常 1部分字段被拒绝(注意这个是否实现？记得还有一个专门的错误码可以使用)
        // 权限：
        //      读者不能修改任何人的读者记录，包括他自己的。
        //      工作人员则要看 setreaderinfo权限是否具备
        //      特殊操作可能还需要 changereaderstate 和 changereaderforegift changereaderbarcode 权限
        // 日志:
        //      要产生日志
        public LibraryServerResult SetReaderInfo(
            SessionInfo sessioninfo,
            string strAction,
            string strRecPath,
            string strNewXml,
            string strOldXml,
            byte[] baOldTimestamp,
            out string strExistingXml,
            out string strSavedXml,
            out string strSavedRecPath,
            out byte[] baNewTimestamp,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode)
        {
            strExistingXml = "";
            strSavedXml = "";
            strSavedRecPath = "";
            baNewTimestamp = null;

            string[] element_names = reader_element_names;

            LibraryServerResult result = new LibraryServerResult();

            LibraryApplication app = this;

            kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            bool bForce = false;
            if (strAction == "forcenew"
                || strAction == "forcechange"
                || strAction == "forcedelete")
            {
                if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改读者信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                bForce = true;

                // 将strAction内容修改为不带有force前缀部分
                strAction = strAction.Remove(0, "force".Length);
            }
            else
            {
                // 权限字符串
                if (strAction == "changestate")
                {
                    // 有setreaderinfo和changereaderstate之一均可
                    if (StringUtil.IsInList("setreaderinfo", sessioninfo.RightsOrigin) == false)
                    {
                        if (StringUtil.IsInList("changereaderstate", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "修改读者信息被拒绝。不具备changereaderstate权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }
                else if (strAction == "changereaderbarcode")
                {
                    if (StringUtil.IsInList("changereaderbarcode", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改读者信息被拒绝。不具备 changereaderbarcode 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else if (strAction == "changeforegift")
                {
                    // changereaderforegift
                    if (StringUtil.IsInList("changereaderforegift", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "changeforegift方式修改读者信息被拒绝。不具备changereaderforegift权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else
                {
                    if (StringUtil.IsInList("setreaderinfo", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改读者信息被拒绝。不具备setreaderinfo权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            // 对读者身份的附加判断
            if (strAction != "change" && sessioninfo.UserType == "reader")
            {
                // 不允许读者修改其他读者的记录,不允许读者创建读者记录.但是允许读者修改自己的记录中的某些元素
                result.Value = -1;
                result.ErrorInfo = "读者身份执行 '" + strAction + "' 的修改读者信息操作操作被拒绝";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            // 参数检查
            if (strAction == "delete")
            {
                if (String.IsNullOrEmpty(strNewXml) == false)
                {
                    strError = "strAction 值为 delete 时, strNewXml 参数必须为空";
                    goto ERROR1;
                }
                if (baNewTimestamp != null)
                {
                    strError = "strAction 值 为delete 时, baNewTimestamp 参数必须为空";
                    goto ERROR1;
                }
            }
            else
            {
                // 非 delete 情况 strNewXml 则必须不为空
                if (String.IsNullOrEmpty(strNewXml) == true)
                {
                    strError = "strAction 值为 " + strAction + " 时, strNewXml 参数不能为空";
                    goto ERROR1;
                }
            }

            // 2007/11/12 
            if (strAction == "new")
            {
                if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    strError = "strAction 值为 new 时, strOldXml 参数必须为空";
                    goto ERROR1;
                }
                if (baOldTimestamp != null)
                {
                    strError = "strAction 值为 new 时, baOldTimestamp 参数必须为空";
                    goto ERROR1;
                }
            }
            else
            {
                if (this.TestMode == true || sessioninfo.TestMode == true)
                {
                    // 检查评估模式
                    // return:
                    //      -1  检查过程出错
                    //      0   可以通过
                    //      1   不允许通过
                    nRet = CheckTestModePath(strRecPath,
                        out strError);
                    if (nRet != 0)
                    {
                        strError = "修改读者记录的操作被拒绝: " + strError;
                        goto ERROR1;
                    }
                }
            }

            // 把旧记录装载到DOM
            XmlDocument domOldRec = new XmlDocument();
            try
            {
                if (String.IsNullOrEmpty(strOldXml) == true)
                    strOldXml = "<root />";

                domOldRec.LoadXml(strOldXml);
            }
            catch (Exception ex)
            {
                strError = "strOldXml XML 记录装载到 DOM 时出错: " + ex.Message;
                goto ERROR1;
            }

            // 把要保存的新记录装载到DOM
            XmlDocument domNewRec = new XmlDocument();
            try
            {
                if (String.IsNullOrEmpty(strNewXml) == true)
                    strNewXml = "<root />";

                domNewRec.LoadXml(strNewXml);
            }
            catch (Exception ex)
            {
                strError = "strNewXml XML 记录装载到 DOM 时出错: " + ex.Message;
                goto ERROR1;
            }

            string strOldBarcode = "";
            string strNewBarcode = "";

            // return:
            //      -1  出错
            //      0   相等
            //      1   不相等
            nRet = CompareTwoBarcode(domOldRec,
                domNewRec,
                out strOldBarcode,
                out strNewBarcode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 对读者身份的附加判断
            if (strAction == "change" && sessioninfo.UserType == "reader")
            {
                /*
                // 暂时不允许读者自己修改任何读者的信息
                // 今后修改为：读者只能修改自己的记录，而且只能修改某些字段（其他修改被忽略）。
                result.Value = -1;
                result.ErrorInfo = "修改读者信息被拒绝。作为读者不能修改读者记录";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
                 * */

                if (sessioninfo.Account.Barcode != strNewBarcode)
                {
                    result.Value = -1;
                    result.ErrorInfo = "修改读者信息被拒绝。作为读者不能修改其他读者的读者记录";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }

                element_names = selfchangeable_reader_element_names;
            }

            bool bBarcodeChanged = false;
            if (nRet == 1)
                bBarcodeChanged = true;

            string strOldDisplayName = "";
            string strNewDisplayName = "";

            // return:
            //      -1  出错
            //      0   相等
            //      1   不相等
            nRet = CompareTwoDisplayName(domOldRec,
                domNewRec,
                out strOldDisplayName,
                out strNewDisplayName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            bool bDisplayNameChanged = false;
            if (nRet == 1)
                bDisplayNameChanged = true;

            string strLockBarcode = "";

            if (strAction == "new"
                || strAction == "change"
                || strAction == "changestate"
                || strAction == "changereaderbarcode")
                strLockBarcode = strNewBarcode;
            else if (strAction == "delete")
            {
                // 顺便进行一些检查
                if (String.IsNullOrEmpty(strNewBarcode) == false)
                {
                    strError = "没有必要在 delete 操作的 strNewXml 参数中, 包含新记录内容...。相反，注意一定要在 strOldXml 参数中包含即将删除的原记录";
                    goto ERROR1;
                }
                strLockBarcode = strOldBarcode;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 加读者记录锁
            if (String.IsNullOrEmpty(strLockBarcode) == false)
            {
#if DEBUG_LOCK_READER
                app.WriteErrorLog("SetReaderInfo 开始为读者加写锁 '" + strLockBarcode + "'");
#endif
                app.ReaderLocks.LockForWrite(strLockBarcode);
            }
            try
            {
                // 2014/1/10
                // 检查空条码号
                if (bBarcodeChanged == true
    && (strAction == "new"
        || strAction == "change"
        || strAction == "changestate"
        || strAction == "changeforegift"
        || strAction == "changereaderbarcode")
    && String.IsNullOrEmpty(strNewBarcode) == true
    )
                {
                    if (this.AcceptBlankReaderBarcode == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError + "证条码号不能为空。保存操作失败";
                        result.ErrorCode = ErrorCode.InvalidReaderBarcode;
                        return result;
                    }
                }

                if (strAction == "new"
        || strAction == "change"
        || strAction == "changereaderbarcode"
        || strAction == "move")
                {
                    nRet = this.DoVerifyReaderFunction(
                        sessioninfo,
                        strAction,
                        domNewRec,
                        out strError);
                    if (nRet != 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }
                }

                // 对读者证条码号查重，如果必要，并获得strRecPath
                if (bBarcodeChanged == true
                    && (strAction == "new"
                        || strAction == "change"
                        || strAction == "changestate"
                        || strAction == "changeforegift"
                        || strAction == "changereaderbarcode")
                    && String.IsNullOrEmpty(strNewBarcode) == false
                    )
                {

                    // 验证条码号
                    if (this.VerifyBarcode == true)
                    {
                        // return:
                        //	0	invalid barcode
                        //	1	is valid reader barcode
                        //	2	is valid item barcode
                        int nResultValue = 0;

                        // return:
                        //      -2  not found script
                        //      -1  出错
                        //      0   成功
                        nRet = this.DoVerifyBarcodeScriptFunction(
                            null,
                            sessioninfo.LibraryCodeList,
                            strNewBarcode,
                            out nResultValue,
                            out strError);
                        if (nRet == -2 || nRet == -1 || nResultValue != 1)
                        {
                            if (nRet == -2)
                                strError = "library.xml 中没有配置条码号验证函数，无法进行条码号验证";
                            else if (nRet == -1)
                            {
                                strError = "验证读者证条码号的过程中出错"
                                    + (string.IsNullOrEmpty(strError) == true ? "" : ": " + strError);
                            }
                            else if (nResultValue != 1)
                            {
                                strError = "条码号 '" + strNewBarcode + "' 经验证发现不是一个合法的读者证条码号"
                                    + (string.IsNullOrEmpty(strError) == true ? "" : "(" + strError + ")");
                            }
                            result.Value = -1;
                            result.ErrorInfo = strError + "。保存操作失败";
                            result.ErrorCode = ErrorCode.InvalidReaderBarcode;
                            return result;
                        }
                    }

                    List<string> aPath = null;

                    // 本函数只负责查重, 并不获得记录体
                    // return:
                    //      -1  error
                    //      其他    命中记录条数(不超过nMax规定的极限)
                    nRet = app.SearchReaderRecDup(
                        // sessioninfo.Channels,
                        channel,
                        strNewBarcode,
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
                        Debug.Assert(aPath.Count == 1, "");

                        // 如果输入参数中没有指定strRecPath
                        if (String.IsNullOrEmpty(strRecPath) == true)
                        {
                            if (strAction == "new") // 2006/12/23 add
                                bDup = true;
                            else
                                strRecPath = aPath[0];
                        }
                        else
                        {
                            if (aPath[0] == strRecPath) // 正好是自己
                            {
                                bDup = false;
                            }
                            else
                            {
                                // 别的记录中已经使用了这个条码号
                                bDup = true;
                            }
                        }
                    }
                    else
                    {
                        Debug.Assert(nRet > 1, "");
                        bDup = true;
                    }

                    // 报错
                    if (bDup == true)
                    {
                        /*
                        string[] pathlist = new string[aPath.Count];
                        aPath.CopyTo(pathlist);


                        strError = "条码号 '" + strNewBarcode + "' 已经被下列读者记录使用了: " + String.Join(",", pathlist) + "。操作失败。";
                         * */
                        if (String.IsNullOrEmpty(strNewDisplayName) == false)
                            strError = "条码号 '" + strNewBarcode + "' 或 显示名 '"+strNewDisplayName+"' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";
                        else
                            strError = "条码号 '" + strNewBarcode + "' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";

                        // 2008/8/15 changed
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }
                }

                // 对显示名检查和查重
                if (bDisplayNameChanged == true
                    && (strAction == "new"
                        || strAction == "change"
                        || strAction == "changestate"
                        || strAction == "changeforegift"
                        || strAction == "changereaderbarcode")
                    && String.IsNullOrEmpty(strNewDisplayName) == false
                    )
                {
                    {
                        int nResultValue = -1;
                        // 检查名字空间。
                        // return:
                        //      -2  not found script
                        //      -1  出错
                        //      0   成功
                        nRet = this.DoVerifyBarcodeScriptFunction(
                            null,
                            "",
                            strNewDisplayName,
                            out nResultValue,
                            out strError);
                        if (nRet == -2)
                        {
                            // 没有校验条码号功能，所以无法校验用户名和条码号名字空间的冲突
                            goto SKIP_VERIFY;
                        }
                        if (nRet == -1)
                        {
                            strError = "校验显示名 '" + strNewDisplayName + "' 和条码号(空间)潜在冲突过程中(调用函数DoVerifyBarcodeScriptFunction()时)发生错误: " + strError;
                            goto ERROR1;
                        }

                        Debug.Assert(nRet == 0, "");

                        if (nResultValue == -1)
                        {
                            strError = "校验显示名 '" + strNewDisplayName + "' 和条码号(空间)潜在冲突过程中发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (nResultValue == 1)
                        {
                            // TODO: 需要多语种
                            strError = "显示名 '" + strNewDisplayName + "' 和读者证条码号名字空间发生冲突，不能作为显示名。";
                            goto ERROR1;
                        }
                    }

                SKIP_VERIFY:
                    List<string> aPath = null;

                    // 防止和其他读者的显示名相重复
                    // 本函数只负责查重, 并不获得记录体
                    // return:
                    //      -1  error
                    //      其他    命中记录条数(不超过nMax规定的极限)
                    nRet = app.SearchReaderDisplayNameDup(
                        // sessioninfo.Channels,
                        channel,
                        strNewDisplayName,
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
                        Debug.Assert(aPath.Count == 1, "");

                        // 如果输入参数中没有指定strRecPath
                        if (String.IsNullOrEmpty(strRecPath) == true)
                        {
                            if (strAction == "new")
                                bDup = true;
                            else
                                strRecPath = aPath[0];
                        }
                        else
                        {
                            if (aPath[0] == strRecPath) // 正好是自己
                            {
                                bDup = false;
                            }
                            else
                            {
                                // 别的记录中已经使用了这个条码号
                                bDup = true;
                            }
                        }
                    }
                    else
                    {
                        Debug.Assert(nRet > 1, "");
                        bDup = true;
                    }

                    // 报错
                    if (bDup == true)
                    {
                        strError = "显示名 '" + strNewDisplayName + "' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";

                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }

                    // 对工作人员帐户名进行查重。虽然不是强制性的，但是可以避免大部分误会
                    // 注：工作人员依然可以创建和读者显示名相重的帐户名
                    if (SearchUserNameDup(strNewDisplayName) == true)
                    {
                        strError = "显示名 '" + strNewDisplayName + "' 已经被工作人员帐户使用。操作失败。";
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }
                }

                string strReaderDbName = "";

                if (String.IsNullOrEmpty(strRecPath) == false)
                    strReaderDbName = ResPath.GetDbName(strRecPath);    // BUG. 缺乏'strReaderDbName = ' 2008/6/4 changed

                // 准备日志DOM
                XmlDocument domOperLog = new XmlDocument();
                domOperLog.LoadXml("<root />");

                DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "setReaderInfo");
                // 2014/11/17
                if (bForce == true)
                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "style", "force");
                }

#if NO
                RmsChannel channel = sessioninfo.Channels.GetChannel(app.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    goto ERROR1;
                }
#endif

                // 兑现一个命令
                if (strAction == "new")
                {
                    this.SessionTable.CloseSessionByReaderBarcode(strNewBarcode);

                    // 检查新记录的路径中的id部分是否正确
                    // 库名部分，前面已经统一检查过了
                    if (String.IsNullOrEmpty(strRecPath) == true)
                    {
                        // 当路径整个为空的时候，自动选用第一个读者库
                        if (String.IsNullOrEmpty(strReaderDbName) == true)
                        {
                            if (app.ReaderDbs.Count == 0)
                            {
                                strError = "dp2Library尚未定义读者库， 因此无法新创建读者记录。";
                                goto ERROR1;
                            }

                            // 选用当前用户能管辖的第一个读者库
                            // strReaderDbName = app.ReaderDbs[0].DbName;
                            List<string> dbnames = app.GetCurrentReaderDbNameList(sessioninfo.LibraryCodeList);
                            if (dbnames.Count > 0)
                                strReaderDbName = dbnames[0];
                            else
                            {
                                strReaderDbName = "";

                                strError = "当前用户没有管辖任何读者库， 因此无法新创建读者记录。";
                                goto ERROR1;
                            }
                        }

                        strRecPath = strReaderDbName + "/?";
                    }
                    else
                    {
                        string strID = ResPath.GetRecordId(strRecPath);
                        if (String.IsNullOrEmpty(strID) == true)
                        {
                            strError = "RecPath中id部分应当为'?'";
                            goto ERROR1;
                        }

                        // 2007/11/12
                        // 加上了这句话，就禁止了action为new时的定id保存功能。这个功能本来是被允许的。不过禁止后，更可避免概念混淆、出错。
                        if (strID != "?")
                        {
                            strError = "当strAction为new时，strRecPath必须为 读者库名/? 形态，或空(空表示取第一个读者库的当前最尾号)。(但目前strRecPath为'" + strRecPath + "')";
                            goto ERROR1;
                        }
                    }

                    // 构造出适合保存的新读者记录
                    if (bForce == false)
                    {
                        // 主要是为了把待加工的记录中，可能出现的属于“流通信息”的字段去除，避免出现安全性问题
                        nRet = BuildNewReaderRecord(domNewRec,
                            out strSavedXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        // 2008/5/29 
                        strSavedXml = domNewRec.OuterXml;
                    }

                    string strLibraryCode = "";
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (app.IsCurrentChangeableReaderPath(strRecPath,
                        sessioninfo.LibraryCodeList,
                        out strLibraryCode) == false)
                    {
                        strError = "读者记录路径 '" + strRecPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }

                    // 2014/7/4
                    if (this.VerifyReaderType == true)
                    {
                        XmlDocument domTemp = new XmlDocument();
                        domTemp.LoadXml(strSavedXml);

                        // 检查一个册记录的读者类型是否符合值列表要求
                        // parameters:
                        // return:
                        //      -1  检查过程出错
                        //      0   符合要求
                        //      1   不符合要求
                        nRet = CheckReaderType(domTemp,
                            strLibraryCode,
                            strReaderDbName,
                            out strError);
                        if (nRet == -1 || nRet == 1)
                        {
                            strError = strError + "。创建读者记录操作失败";
                            goto ERROR1;
                        }
                    }

                    byte[] output_timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.DoSaveTextRes(strRecPath,
                        strSavedXml,
                        false,   // include preamble?
                        "content",
                        baOldTimestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strSavedXml = "";
                        strSavedRecPath = strOutputPath;    // 2011/9/6 add
                        baNewTimestamp = output_timestamp;
                        if (channel.OriginErrorCode == ErrorCodeValue.TimestampMismatch)
                        {
                            // 2011/9/6 add
                            strError = "创建新读者记录的时候，数据库内核决定创建新记录的位置 '"+strOutputPath+"' 居然已经存在记录。这通常是因为该数据库的尾号不正常导致的。请提醒系统管理员及时处理这个故障。原始错误信息: " + strError;
                        }
                        else
                            strError = "保存新记录的操作发生错误:" + strError;
                        kernel_errorcode = channel.OriginErrorCode;
                        goto ERROR1;
                    }
                    else // 成功
                    {
                        DomUtil.SetElementText(domOperLog.DocumentElement,
"libraryCode",
strLibraryCode);    // 读者所在的馆代码

                        DomUtil.SetElementText(domOperLog.DocumentElement, "action", "new");

                        // 不创建<oldRecord>元素

                        XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement, "record", strNewXml);
                        DomUtil.SetAttr(node, "recPath", strOutputPath);

                        // 新记录保存成功，需要返回信息元素。因为需要返回新的时间戳和实际保存的记录路径
                        strSavedRecPath = strOutputPath;
                        // strSavedXml     // 所真正保存的记录，可能稍有变化, 因此需要返回给前端
                        baNewTimestamp = output_timestamp;

                        // 成功
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "修改读者信息",
                            "创建新记录数",
                            1);
                    }
                }
                else if (strAction == "change"
                    || strAction == "changestate"
                    || strAction == "changeforegift"
                    || strAction == "changereaderbarcode")
                {
                    this.SessionTable.CloseSessionByReaderBarcode(strNewBarcode);

                    // DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue errorcode;
                    // 执行"change"操作
                    // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
                    // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = DoReaderChange(
                        sessioninfo.LibraryCodeList,
                        element_names,
                        strAction,
                        bForce,
                        channel,
                        strRecPath,
                        domNewRec,
                        domOldRec,
                        baOldTimestamp,
                        ref domOperLog,
                        out strExistingXml,    // strExistingRecord,
                        out strSavedXml,    // strNewRecord,
                        out baNewTimestamp,
                        out strError,
                        out kernel_errorcode);
                    if (nRet == -1)
                    {
                        // 失败
                        domOperLog = null;  // 表示不必写入日志
                        goto ERROR1;
                    }

                    strSavedRecPath = strRecPath;   // 保存过程不会改变记录路径
                }
                else if (strAction == "delete")
                {
                    this.SessionTable.CloseSessionByReaderBarcode(strNewBarcode);

                    // return:
                    //      -2  记录中有流通信息，不能删除
                    //      -1  出错
                    //      0   记录本来就不存在
                    //      1   记录成功删除
                    nRet = DoReaderOperDelete(
                        sessioninfo.LibraryCodeList,
                       element_names,
                       sessioninfo,
                       bForce,
                       channel,
                       strRecPath,
                       strOldXml,
                       baOldTimestamp,
                       strOldBarcode,
                        // strNewBarcode,
                       domOldRec,
                       ref strExistingXml,
                       ref baNewTimestamp,
                       ref domOperLog,
                       ref kernel_errorcode,
                       out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == -2)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.HasCirculationInfo;
                        return result;
                    }

                    // 记录没有找到
                    if (nRet == 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                        return result;
                    }
                }
                else
                {
                    // 不支持的命令
                    strError = "不支持的操作命令 '" + strAction + "'";
                    goto ERROR1;
                }

                // 写入日志
                if (domOperLog != null)
                {
                    string strOperTime = app.Clock.GetClock();
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                        sessioninfo.UserID);   // 操作者
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                        strOperTime);   // 操作时间

                    nRet = app.OperLog.WriteOperLog(domOperLog,
                        sessioninfo.ClientAddress,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "SetReaderInfo() API 写入日志时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = "抛出异常:" + ex.Message;
                return result;
            }
            finally
            {
                if (String.IsNullOrEmpty(strLockBarcode) == false)
                {
                    app.ReaderLocks.UnlockForWrite(strLockBarcode);
#if DEBUG_LOCK_READER
                    app.WriteErrorLog("SetReaderInfo 结束为读者加写锁 '" + strLockBarcode + "'");
#endif

                }
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        #region SetReaderInfo() 下级函数

        // 检查一个册记录的读者类型是否符合值列表要求
        // parameters:
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        int CheckReaderType(XmlDocument dom,
            string strLibraryCode,
            string strReaderDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<string> values = null;

            // 试探 读者库名

            // 获得一个图书馆代码下的值列表
            // parameters:
            //      strLibraryCode  馆代码
            //      strTableName    表名。如果为空，表示任意name参数值均匹配
            //      strDbName   数据库名。如果为空，表示任意dbname参数值均匹配。
            values = GetOneLibraryValueTable(
                strLibraryCode,
                "readerType",
                strReaderDbName);
            if (values != null && values.Count > 0)
                goto FOUND;

            // 试探不使用数据库名
            values = GetOneLibraryValueTable(
    strLibraryCode,
    "readerType",
    "");
            if (values != null && values.Count > 0)
                goto FOUND;

            return 0;   // 因为没有值列表，什么值都可以

            FOUND:
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
    "readerType");

            if (IsInList(strReaderType, values) == true)
                return 0;

            GetPureValue(ref values);
            strError = "读者类型 '" + strReaderType + "' 不是合法的值。应为 '" + StringUtil.MakePathList(values) + "' 之一";
            return 1;
        }

        // 对新旧读者记录(或者册记录)中包含的条码号进行比较, 看看是否发生了变化(进而就需要查重)
        // 条码号包含在<barcode>元素中
        // parameters:
        //      strOldBarcode   顺便返回旧记录中的条码号
        //      strNewBarcode   顺便返回新记录中的条码号
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        static int CompareTwoBarcode(
    XmlDocument domOldRec,
    XmlDocument domNewRec,
    out string strOldBarcode,
    out string strNewBarcode,
    out string strError)
        {
            return CompareTwoField(
                "barcode",
                domOldRec,
                domNewRec,
                out strOldBarcode,
                out strNewBarcode,
                out strError);
        }

        // 对新旧记录中包含的字段进行比较, 看看是否发生了变化(进而就需要查重)
        // parameters:
        //      strOldText   顺便返回旧记录中的字段内容
        //      strNewText   顺便返回新记录中的字段内容
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        static int CompareTwoField(
            string strElementName,
            XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldText,
            out string strNewText,
            out string strError)
        {
            strError = "";

            strOldText = "";
            strNewText = "";

            strOldText = DomUtil.GetElementText(domOldRec.DocumentElement, strElementName);

            strNewText = DomUtil.GetElementText(domNewRec.DocumentElement, strElementName);

            if (strOldText != strNewText)
                return 1;   // 不相等

            return 0;   // 相等
        }

        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        static int CompareTwoDisplayName(XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldDisplayName,
            out string strNewDisplayName,
            out string strError)
        {
            strError = "";

            strOldDisplayName = "";
            strNewDisplayName = "";

            strOldDisplayName = DomUtil.GetElementText(domOldRec.DocumentElement, "displayName");

            strNewDisplayName = DomUtil.GetElementText(domNewRec.DocumentElement, "displayName");

            if (strOldDisplayName != strNewDisplayName)
                return 1;   // 不相等

            return 0;   // 相等
        }

        // 对新旧读者记录(或者册记录)中包含的<state>状态字段进行比较, 看看是否发生了变化
        // 状态包含在<state>元素中
        // parameters:
        //      strOldState   顺便返回旧记录中的状态字符串
        //      strNewState   顺便返回新记录中的状态字符串
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        static int CompareTwoState(XmlDocument domOldRec,
            XmlDocument domNewRec,
            out string strOldState,
            out string strNewState,
            out string strError)
        {
            strError = "";

            strOldState = "";
            strNewState = "";

            strOldState = DomUtil.GetElementText(domOldRec.DocumentElement, "state");
            strOldState = strOldState.Trim();

            strNewState = DomUtil.GetElementText(domNewRec.DocumentElement, "state");
            strNewState = strNewState.Trim();



            if (strOldState != strNewState)
                return 1;   // 不相等

            return 0;   // 相等
        }

        // 删除读者记录的操作
        // return:
        //      -2  记录中有流通信息，不能删除
        //      -1  出错
        //      0   记录本来就不存在
        //      1   记录成功删除
        int DoReaderOperDelete(
            string strCurrentLibraryCode,
            string [] element_names,
            SessionInfo sessioninfo,
            bool bForce,
            RmsChannel channel,
            string strRecPath,
            string strOldXml,
            byte[] baOldTimestamp,
            string strOldBarcode,
            // string strNewBarcode,
            XmlDocument domOldRec,
            ref string strExistingXml,
            ref byte[] baNewTimestamp,
            ref XmlDocument domOperLog,
            ref DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode,
            out string strError)
        {
            strError = "";

            int nRedoCount = 0;
            int nRet = 0;
            long lRet = 0;

            // 如果记录路径为空, 则先获得记录路径
            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                List<string> aPath = null;

                if (String.IsNullOrEmpty(strOldBarcode) == true)
                {
                    strError = "strOldXml中的<barcode>元素中的证条码号，和strRecPath参数值，不能同时为空。";
                    goto ERROR1;
                }

                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                nRet = this.SearchReaderRecDup(
                    // sessioninfo.Channels,
                    channel,
                    strOldBarcode,
                    100,
                    out aPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    strError = "证条码号为 '" + strOldBarcode + "' 的读者记录已不存在";
                    kernel_errorcode = ErrorCodeValue.NotFound;
                    // goto ERROR1;
                    return 0;   // 2009/7/17 changed
                }


                if (nRet > 1)
                {
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                     * */

                    // 2007/11/22 
                    // 在删除操作中，遇到重复的是很平常的事情。只要
                    // strRecPath能够清晰地指出要删除的那一条，就可以执行删除
                    if (String.IsNullOrEmpty(strRecPath) == false)
                    {
                        if (aPath.IndexOf(strRecPath) == -1)
                        {
                            strError = "证条码号 '" + strOldBarcode + "' 已经被下列多条读者记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'，但并不包括strRecPath所指的路径 '" + strRecPath + "'。删除操作失败。";
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "证条码号 '" + strOldBarcode + "' 已经被下列多条读者记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'，在未指定记录路径的情况下，无法定位和删除。";
                        goto ERROR1;
                    }
                }
                else
                {

                    strRecPath = aPath[0];
                    // strReaderDbName = ResPath.GetDbName(strRecPath);
                }
            }

            // 删除动作，API 的 strRecPath 参数可能为空，所以这里要单独检查一次
            if (this.TestMode == true || sessioninfo.TestMode == true)
            {
                // 检查评估模式
                // return:
                //      -1  检查过程出错
                //      0   可以通过
                //      1   不允许通过
                nRet = CheckTestModePath(strRecPath,
                    out strError);
                if (nRet != 0)
                {
                    strError = "删除读者记录的操作被拒绝: " + strError;
                    goto ERROR1;
                }
            }

            // Debug.Assert(strReaderDbName != "", "");

            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";

        REDOLOAD:


            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(strRecPath,
                out strExistingXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    kernel_errorcode = channel.OriginErrorCode;
                    goto ERROR1;
                }
                else
                {
                    strError = "删除操作发生错误, 在读入原有记录阶段:" + strError;
                    kernel_errorcode = channel.OriginErrorCode;
                    goto ERROR1;
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

            string strExistingBarcode = DomUtil.GetElementText(domExist.DocumentElement, "barcode");


            // 观察已经存在的记录中，证条码号是否和strOldBarcode一致
            if (String.IsNullOrEmpty(strOldBarcode) == false)
            {
                if (strExistingBarcode != strOldBarcode)
                {
                    strError = "路径为 '" + strRecPath + "' 的读者记录中<barcode>元素中的证条码号 '" + strExistingBarcode + "' 和strOldXml中<barcode>元素中的证条码号 '" + strOldBarcode + "' 不一致。拒绝删除(如果允许删除，则会造成不经意删除了别的读者记录的危险)。";
                    goto ERROR1;
                }
           }

            // 清除 LoginCache
            // this.LoginCache.Remove(strExistingBarcode);
            this.ClearLoginCache(strExistingBarcode);

            // 观察已经存在的记录是否有流通信息
            string strDetailInfo = "";
            bool bHasCirculationInfo = IsReaderHasCirculationInfo(domExist,
                out strDetailInfo);

            if (bForce == false)
            {
                if (bHasCirculationInfo == true)
                {
                    strError = "删除操作被拒绝。因拟删除的读者记录 '" + strRecPath + "' 中包含有 " + strDetailInfo + "";
                    goto ERROR2;
                }
            }

            // 比较时间戳
            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(baOldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 2008/5/29 
                if (bForce == true)
                {
                    strError = "数据库中即将删除的读者记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                    kernel_errorcode = ErrorCodeValue.TimestampMismatch;
                    baNewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                    goto ERROR1;
                }

                // 是否报错?
                // 功能做的精细一点，需要比较strOldXml和strExistingXml中要害字段是否被改变了，如果没有改变，是不必报错的

                // 如果前端给出了旧记录，就有和库中记录进行比较的基础
                if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    // 比较两个记录, 看看和读者静态信息有关的字段是否发生了变化
                    // return:
                    //      0   没有变化
                    //      1   有变化
                    nRet = IsReaderInfoChanged(
                        element_names,
                        domExist,
                        domOldRec);
                    if (nRet == 1)
                    {

                        strError = "数据库中即将删除的读者记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                        kernel_errorcode = ErrorCodeValue.TimestampMismatch;

                        baNewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                        goto ERROR1;
                    }
                }

                baOldTimestamp = exist_timestamp;
                baNewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
            }

            string strLibraryCode = "";
            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
            if (this.IsCurrentChangeableReaderPath(strRecPath,
                strCurrentLibraryCode,
                out strLibraryCode) == false)
            {
                strError = "读者记录路径 '" + strRecPath + "' 的读者库不在当前用户管辖范围内";
                goto ERROR1;
            }

            byte[] output_timestamp = null;

            Debug.Assert(strRecPath != "", "");

            lRet = channel.DoDeleteRes(strRecPath,
                baOldTimestamp,
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                // 2009/7/17 
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strError = "证条码号为 '" + strOldBarcode + "' 的读者记录(在删除的时候发现)已不存在";
                    kernel_errorcode = ErrorCodeValue.NotFound;
                    return 0;
                }

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复删除均遇到时间戳冲突, 超过10次重试仍然失败";
                        baNewTimestamp = output_timestamp;
                        kernel_errorcode = channel.OriginErrorCode;
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }


                baNewTimestamp = output_timestamp;
                strError = "删除操作发生错误:" + strError;
                kernel_errorcode = channel.OriginErrorCode;
                goto ERROR1;
            }
            else
            {
                // 成功
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "delete");

                // 不创建<record>元素

                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "oldRecord", strExistingXml);
                    DomUtil.SetAttr(node, "recPath", strRecPath);
                }

                // 2014/11/17
                if (bForce == true)
                {
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
        "style", "force");
                    if (string.IsNullOrEmpty(strDetailInfo) == false
                        && bHasCirculationInfo == true)
                        DomUtil.SetAttr(node, "description", strDetailInfo);
                }

                // 如果删除成功，则不必要在数组中返回表示成功的信息元素了

                /// 
                if (this.Statis != null)
                    this.Statis.IncreaseEntryValue(strLibraryCode,
    "修改读者信息",
    "删除记录数",
    1);
            }

            return 1;
        ERROR1:
            kernel_errorcode = ErrorCodeValue.CommonError;
            domOperLog = null;  // 表示不必写入日志
            return -1;
        ERROR2:
            kernel_errorcode = ErrorCodeValue.CommonError;
            domOperLog = null;  // 表示不必写入日志
            return -2;
        }


        // 执行"change"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // return:
        //      -1  出错
        //      0   成功
        int DoReaderChange(
            string strCurrentLibraryCode,
            string [] element_names,
            string strAction,
            bool bForce,
            RmsChannel channel,
            string strRecPath,
            XmlDocument domNewRec,
            XmlDocument domOldRec,
            byte[] baOldTimestamp,
            ref XmlDocument domOperLog,
            out string strExistingRecord,
            out string strNewRecord,
            out byte[] baNewTimestamp,
            out string strError,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue errorcode)
        {
            strError = "";
            strExistingRecord = "";
            strNewRecord = "";
            baNewTimestamp = null;
            errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

            int nRedoCount = 0;
            bool bExist = true;    // strRecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";

        REDOLOAD:

            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(strRecPath,
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
                    strOutputPath = strRecPath;
                }
                else
                {
                    strError = "保存操作发生错误, 在读入原有记录阶段:" + strError;
                    errorcode = channel.OriginErrorCode;
                    return -1;
                }
            }

            // 把记录装入DOM
            XmlDocument domExist = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            bool bChangeReaderBarcode = false;

            string strOldBarcode = "";
            string strNewBarcode = "";

            if (bExist == true) // 2008/5/29 
            {

                // 比较新旧记录的条码号是否有改变
                // return:
                //      -1  出错
                //      0   相等
                //      1   不相等
                nRet = CompareTwoBarcode(domExist,
                    domNewRec,
                    out strOldBarcode,
                    out strNewBarcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                string strDetailInfo = "";  // 关于读者记录里面是否有流通信息的详细提示文字
                bool bHasCirculationInfo = false;   // 读者记录里面是否有流通信息
                bool bDetectCiculationInfo = false; // 是否已经探测过读者记录中的流通信息

                if (nRet == 1)  // 读者证条码号有改变
                {
                    // 观察已经存在的记录是否有流通信息
                    bHasCirculationInfo = IsReaderHasCirculationInfo(domExist,
                        out strDetailInfo);
                    bDetectCiculationInfo = true;

                    if (bHasCirculationInfo == true)
                    {
                        if (strAction != "changereaderbarcode"
                            && bForce == false)
                        {
                            strError = "(在读者记录中尚有借还信息时)修改读者证条码号的操作被拒绝。建议用 changereaderbarcode 动作进行此项操作；或者用 forcechange 动作。"
    + "因读者记录 '" + strRecPath + "' 中包含有 " + strDetailInfo + "，所以修改它时证条码号字段内容不能改变。(当前证条码号 '" + strOldBarcode + "'，试图修改为条码号 '" + strNewBarcode + "')";
                            goto ERROR1;
                        }

                        // TODO: 可否增加允许同时修改所关联的已借阅册记录修改能力?
                        // 值得注意的是如何记录进操作日志，将来如何进行recover的问题
                        bChangeReaderBarcode = true;
                    }
                }

                // 清除 LoginCache
#if NO
                this.LoginCache.Remove(strOldBarcode);
                if (strNewBarcode != strOldBarcode)
                    this.LoginCache.Remove(strNewBarcode);
#endif
                this.ClearLoginCache(strOldBarcode);
                if (strNewBarcode != strOldBarcode)
                    this.ClearLoginCache(strNewBarcode);

                // 2009/1/23 

                // 比较新旧记录的状态是否有改变，如果从其他状态修改为“注销”状态，则应引起注意，后面要进行必要的检查

                string strOldState = "";
                string strNewState = "";

                // parameters:
                //      strOldState   顺便返回旧记录中的状态字符串
                //      strNewState   顺便返回新记录中的状态字符串
                // return:
                //      -1  出错
                //      0   相等
                //      1   不相等
                nRet = CompareTwoState(domExist,
                    domNewRec,
                    out strOldState,
                    out strNewState,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                {
                    if (strOldState != "注销" && strNewState == "注销"
                        && bForce == false)
                    {
                        // 观察已经存在的记录是否有流通信息
                        if (bDetectCiculationInfo == false)
                        {
                            bHasCirculationInfo = IsReaderHasCirculationInfo(domExist,
                                out strDetailInfo);
                            bDetectCiculationInfo = true;
                        }

                        if (bHasCirculationInfo == true)
                        {
                            Debug.Assert(bDetectCiculationInfo == true, "");
                            strError = "注销操作被拒绝。因拟被注销的读者记录 '" + strRecPath + "' 中包含有 " + strDetailInfo + "。(当前证状态 '" + strOldState + "', 试图修改为新状态 '" + strNewState + "')";
                            goto ERROR1;
                        }
                    }
                }
            }

            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(baOldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                if (bForce == true)
                {
                    // 2008/5/29 
                    // 在强制修改模式下，时间戳不一致意义重大，直接返回出错，而不进行要害字段的比对判断
                    strError = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 已发生过修改";
                    errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch;
                    return -1;  // timestamp mismatch
                }

                // 时间戳不相等了
                // 需要把domOldRec和strExistXml进行比较，看看和读者信息有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                // 比较两个记录, 看看和册登录有关的字段是否发生了变化
                // return:
                //      0   没有变化
                //      1   有变化
                nRet = IsReaderInfoChanged(
                    element_names,
                    domOldRec,
                    domExist);
                if (nRet == 1)
                {
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    strExistingRecord = strExistXml;
                    baNewTimestamp = exist_timestamp;

                    if (bExist == false)
                        strError = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 已被删除。";
                    else
                        strError = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 已发生过修改";

                    errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch;
                    return -1;  // timestamp mismatch
                }

                // exist_timestamp此时已经反映了库中被修改后的记录的时间戳
            }

            // TODO: 当strAction==changestate时，只允许<state>和<comment>两个元素内容发生变化

            if (bForce == false)
            {
                string strNewXml = "";
                nRet = MergeTwoReaderXml(
                    element_names,
                    strAction,
                    domExist,
                    domNewRec,
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                domNewRec = new XmlDocument();
                try
                {
                    domNewRec.LoadXml(strNewXml);
                }
                catch (Exception ex)
                {
                    strError = "(1)读者记录装入 XMLDOM 时出错: " + ex.Message;
                    return -1;
                }
            }

            string strLibraryCode = "";
            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
            if (this.IsCurrentChangeableReaderPath(strRecPath,
                strCurrentLibraryCode,
                out strLibraryCode) == false)
            {
                strError = "读者记录路径 '" + strRecPath + "' 的读者库不在当前用户管辖范围内";
                goto ERROR1;
            }

            // 2014/7/4
            if (this.VerifyReaderType == true)
            {
                string strReaderDbName = "";

                if (String.IsNullOrEmpty(strRecPath) == false)
                    strReaderDbName = ResPath.GetDbName(strRecPath);

#if NO
                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);
#endif

                // 检查一个册记录的读者类型是否符合值列表要求
                // parameters:
                // return:
                //      -1  检查过程出错
                //      0   符合要求
                //      1   不符合要求
                nRet = CheckReaderType(domNewRec,   // domTemp,
                    strLibraryCode,
                    strReaderDbName,
                    out strError);
                if (nRet == -1 || nRet == 1)
                {
                    strError = strError + "。修改读者记录操作失败";
                    goto ERROR1;
                }
            }

            // 注：bForce 为 true 时，效果是允许直接修改读者记录而并不修改相关册记录里的回链证条码号。这是为备份恢复而准备的功能。在备份恢复操作中，后面自然有人去操心恢复册记录，不必劳烦这里去操心联动修改了
            if (bChangeReaderBarcode && bForce == false)
            {
                // 要修改读者记录的附注字段
                string strExistComment = DomUtil.GetElementText(domNewRec.DocumentElement, "comment");
                if (string.IsNullOrEmpty(strExistComment) == true)
                    strExistComment = "";
                else
                    strExistComment += "; ";
                strExistComment += DateTime.Now.ToString() + " 证条码号从 '"+strOldBarcode+"' 修改为 '"+strNewBarcode+"'";
                DomUtil.SetElementText(domNewRec.DocumentElement, "comment", strExistComment);

            }

            // 保存新记录
            byte[] output_timestamp = null;
            lRet = channel.DoSaveTextRes(strRecPath,
    domNewRec.OuterXml, // strNewXml,
    false,   // include preamble?
    "content",
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
                        strError = "反复遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                strError = "保存操作发生错误:" + strError;
                errorcode = channel.OriginErrorCode;
                return -1;
            }
            else // 成功
            {
                DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strLibraryCode);    // 读者所在的馆代码

                DomUtil.SetElementText(domOperLog.DocumentElement, "action", "change");

                XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement, "record", domNewRec.OuterXml);  // strNewXml
                DomUtil.SetAttr(node, "recPath", strRecPath);

                node = DomUtil.SetElementText(domOperLog.DocumentElement, "oldRecord", strExistXml);
                DomUtil.SetAttr(node, "recPath", strRecPath);

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                baNewTimestamp = output_timestamp;
                strNewRecord = domNewRec.OuterXml;  // strNewXml;

                strError = "保存操作成功。NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

                /// 
                {
                    if (strAction == "change")
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "修改读者信息",
                            "修改记录数",
                            1);
                    }
                    else if (strAction == "changestate")
                    {
                        string strNewState = DomUtil.GetElementText(domNewRec.DocumentElement,
                            "state");
                        if (String.IsNullOrEmpty(strNewBarcode) == true)
                            strNewState = "[可用]";
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                            strLibraryCode,
                            "修改读者信息之状态",
                            strNewState, 1);
                    }
                    else if (strAction == "changeforegift")
                    {
                        if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(strLibraryCode,
                            "修改读者信息之押金",
                            "次数",
                            1);
                    }
                }
            }

            // 注：bForce 为 true 时，效果是允许直接修改读者记录而并不修改相关册记录里的回链证条码号。这是为备份恢复而准备的功能。在备份恢复操作中，后面自然有人去操心恢复册记录，不必劳烦这里去操心联动修改了
            if (bChangeReaderBarcode && bForce == false)
            {
                // parameters:
                //      domNewRec   拟保存的新读者记录
                //      strOldReaderBarcode 旧的证条码号
                // return:
                //      -1  出错。错误信息已写入系统错误日志
                //      0   成功
                nRet = ChangeRelativeItemRecords(
                    channel,
                    domNewRec,
                    strOldBarcode,
                    domOperLog,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;

        ERROR1:
            errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.CommonError;
            return -1;
        }

        // parameters:
        //      domNewRec   拟保存的新读者记录
        //      strOldReaderBarcode 旧的证条码号
        // return:
        //      -1  出错。错误信息已写入系统错误日志
        //      0   成功
        int ChangeRelativeItemRecords(
            // SessionInfo sessioninfo,
            RmsChannel channel,
            XmlDocument domNewRec,
            string strOldReaderBarcode,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            XmlDocument domNewRec = new XmlDocument();
            try
            {
                domNewRec.LoadXml(strNewXml);
            }
            catch(Exception ex)
            {
                strError = "读者记录装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }
#endif

            string strNewReaderBarcode = DomUtil.GetElementText(domNewRec.DocumentElement, "barcode");

            XmlNodeList nodes = domNewRec.DocumentElement.SelectNodes("borrows/borrow");

            List<string> item_barcodes = new List<string>();
            foreach (XmlElement borrow in nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                item_barcodes.Add(strItemBarcode);
            }

            foreach(XmlElement borrow in nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                string strItemRecPath = borrow.GetAttribute("recPath");

                // 修改一条册记录，的 borrower 元素内容
                // parameters:
                //      -2  保存记录时出错
                //      -1  一般性错误
                //      0   成功
                nRet = ChangeBorrower(
                    // sessioninfo,
                    channel,
                    strItemBarcode,
                    strItemRecPath,
                    strOldReaderBarcode,
                    strNewReaderBarcode,
                    false,
                    out strError);
                if (nRet == -1 || nRet == -2)
                {
                    strError = "修改读者记录所关联的在借册记录时出错：" + strError
                        + "。下列册条码号的册记录尚未执行修改: " + StringUtil.MakePathList(item_barcodes)
                        + "。为消除数据不一致，请系统管理员手工将这些册记录里的 borrower 元素文本值修改为 " + strNewReaderBarcode;
                    this.WriteErrorLog(strError);
                    return -1;
                }

                // 处理成功的，就从列表中排除
                item_barcodes.Remove(strItemBarcode);

                if (domOperLog != null)
                {
                    XmlNode nodeLogRecord = domOperLog.CreateElement("changedEntityRecord");
                    domOperLog.DocumentElement.AppendChild(nodeLogRecord);
                    DomUtil.SetAttr(nodeLogRecord, "itemBarcode", strItemBarcode);
                    DomUtil.SetAttr(nodeLogRecord, "recPath", strItemRecPath);
                    DomUtil.SetAttr(nodeLogRecord, "oldBorrower", strOldReaderBarcode);
                    DomUtil.SetAttr(nodeLogRecord, "newBorrower", strNewReaderBarcode);
                }
            }

            return 0;
        }

        // 修改一条册记录，的 borrower 元素内容
        // parameters:
        //      bLogRecover    是否为日志恢复时被调用？日志恢复时本函数检查不会那么严格
        // return:
        //      -2  保存记录时出错
        //      -1  一般性错误
        //      0   成功
        int ChangeBorrower(
            // SessionInfo sessioninfo,
            RmsChannel channel,
            string strItemBarcode,
            string strConfirmItemRecPath,
            string strOldBorrower,
            string strNewBorrower,
            bool bLogRecover,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            long lRet = 0;

#if NO
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }
#endif

            // 加册记录锁
            this.EntityLocks.LockForWrite(strItemBarcode);

            try // 册记录锁定范围开始
            {
                string strItemXml = "";
                byte[] item_timestamp = null;
                string strOutputItemRecPath = "";

                // 如果已经有确定的册记录路径
                if (String.IsNullOrEmpty(strConfirmItemRecPath) == false)
                {
                    if (bLogRecover == false)
                    {
                        // 检查路径中的库名，是不是实体库名
                        // return:
                        //      -1  error
                        //      0   不是实体库名
                        //      1   是实体库名
                        nRet = this.CheckItemRecPath(strConfirmItemRecPath,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            strError = strConfirmItemRecPath + strError;
                            return -1;
                        }
                    }

                    string strMetaData = "";

                    lRet = channel.GetRes(strConfirmItemRecPath,
                        out strItemXml,
                        out strMetaData,
                        out item_timestamp,
                        out strOutputItemRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // text-level: 内部错误
                        if (bLogRecover == false)
                        {
                            strError = "根据 strConfirmItemRecPath '" + strConfirmItemRecPath + "' 获得册记录失败: " + strError;
                            return -1;
                        }
                        // 注：如果是日志恢复，还会继续向后执行，试图用册条码号获得册记录
                    }
                }

                if (string.IsNullOrEmpty(strItemXml) == true
                    && string.IsNullOrEmpty(strItemBarcode) == false)
                {
                    // 从册条码号获得册记录

                    List<string> aPath = null;
                    // 获得册记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetItemRecXml(
                        channel,
                        strItemBarcode,
                        out strItemXml,
                        100,
                        out aPath,
                        out item_timestamp,
                        out strError);
                    if (nRet == 0)
                    {
                        // text-level: 用户提示
                        strError = string.Format(this.GetString("册条码号s不存在"),   // "册条码号 '{0}' 不存在"
                            strItemBarcode);

                        return -1;
                    }
                    if (nRet == -1)
                    {
                        // text-level: 内部错误
                        strError = "读入册记录时发生错误: " + strError;
                        return -1;
                    }

                    if (aPath.Count > 1)
                    {
                        // this.WriteErrorLog(result.ErrorInfo);   
                        strError = "册条码号 '"+strItemBarcode+"' 命中多于一条";
                        return -1;
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
                    // text-level: 内部错误
                    strError = "装载册记录进入 XML DOM 时发生错误: " + strError;
                    return -1;
                }

                if (bLogRecover == false)
                {
                    string strExistingBorrower = DomUtil.GetElementText(itemdom.DocumentElement, "borrower");
                    if (strExistingBorrower != strOldBorrower)
                    {
                        strError = "册记录 '" + strOutputItemRecPath + "' 中原有的 borrower 元素内容为 '" + strExistingBorrower + "'，和期待的 '" + strOldBorrower + "' 不同。对册记录的修改被放弃";
                        return -1;
                    }
                }

                DomUtil.SetElementText(itemdom.DocumentElement, "borrower", strNewBorrower);

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
                    // 2015/9/2
                    if (bLogRecover == false)
                        this.WriteErrorLog("Borrow() 写入册记录 '" + strOutputItemRecPath + "' 时出错: " + strError);
                    return -2;
                }

                return 0;
            } // 册记录锁定范围结束
            finally
            {
                // 解册记录锁
                this.EntityLocks.UnlockForWrite(strItemBarcode);    // strItemBarcode 在整个函数中不允许被修改
            }
        }

        // 读者记录是否包含有 流通信息?
        // 2009/1/25 改造过，一次性返回全部详细信息
        // parameters:
        //      strDetail   输出详细描述信息
        static bool IsReaderHasCirculationInfo(XmlDocument dom,
            out string strDetail)
        {
            strDetail = "";
            int nRet = 0;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//borrows/borrow");
            if (nodes.Count > 0)
            {
                Debug.Assert(String.IsNullOrEmpty(strDetail) == true, "");
                strDetail = nodes.Count.ToString() + "个在借册";
            }

            nodes = dom.DocumentElement.SelectNodes("//overdues/overdue");
            if (nodes.Count > 0)
            {
                if (String.IsNullOrEmpty(strDetail) == false)
                    strDetail += "、";
                strDetail = nodes.Count.ToString() + "个交费请求";
            }

            string strForegift = DomUtil.GetElementText(dom.DocumentElement,
                "foregift");
            // 计算押金值，看看是否为0?
            if (String.IsNullOrEmpty(strForegift) == false)
            {
                string strError = "";
                List<string> results = null;
                // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
                nRet = PriceUtil.SumPrices(strForegift,
                    out results,
                    out strError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strDetail) == false)
                        strDetail += "、";
                    strDetail = "押金余额(但金额字符串 '" + strForegift + "' 格式有误:" + strError + ")";
                    goto END1;
                }

                // 看看若干个价格字符串是否都表示了0?
                // return:
                //      -1  出错
                //      0   不为0
                //      1   为0
                nRet = PriceUtil.IsZero(results,
                    out strError);
                if (nRet == -1)
                {
                    if (String.IsNullOrEmpty(strDetail) == false)
                        strDetail += "、";
                    strDetail = "押金余额(但对金额字符串 '" + strForegift + "' 进行是否为零判断的时发生错误: " + strError + ")";
                    goto END1;
                }

                if (nRet == 0)
                {
                    if (String.IsNullOrEmpty(strDetail) == false)
                        strDetail += "、";
                    strDetail = "押金余额";
                    goto END1;
                }
            }

            // TODO: 是否还要看看 //reservations/request ?


            END1:
            if (String.IsNullOrEmpty(strDetail) == false)
                return true;

            return false;
        }


        #endregion


        // 为读者XML添加附加信息
        // parameters:
        //      strLibraryCode  读者记录所从属的恶读者库的馆代码
        public int GetAdvanceReaderXml(
            SessionInfo sessioninfo,
            string strStyle,
            string strLibraryCode,
            string strReaderXml,
            out string strOutputXml,
            out string strError)
        {
            strOutputXml = "";
            strError = "";
            string strWarning = "";
            int nRet = 0;

            XmlDocument readerdom = null;
            nRet = LibraryApplication.LoadToDom(strReaderXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                return -1;
            }

            // 读者类别		
            string strReaderType = DomUtil.GetElementText(readerdom.DocumentElement,
                "readerType");

            XmlNode nodeInfo = readerdom.CreateElement("info");
            readerdom.DocumentElement.AppendChild(nodeInfo);

            // 可借总册数
            int nMaxBorrowItems = 0;
            XmlNode nodeInfoItem = readerdom.CreateElement("item");
            nodeInfo.AppendChild(nodeInfoItem);
            DomUtil.SetAttr(nodeInfoItem, "name", "可借总册数");

            string strParamValue = "";
            MatchResult matchresult;
            // return:
            //      reader和book类型均匹配 算4分
            //      只有reader类型匹配，算3分
            //      只有book类型匹配，算2分
            //      reader和book类型都不匹配，算1分
            nRet = this.GetLoanParam(
                //null,
                strLibraryCode,
                strReaderType,
                "",
                "可借总册数",
                out strParamValue,
                out matchresult,
                out strError);
            if (nRet == -1 || nRet < 3)
                DomUtil.SetAttr(nodeInfoItem, "error", strError);
            else
            {
                DomUtil.SetAttr(nodeInfoItem, "value", strParamValue);

                try
                {
                    nMaxBorrowItems = System.Convert.ToInt32(strParamValue);
                }
                catch
                {
                    strWarning += "当前读者 可借总册数 参数 '" + strParamValue + "' 格式错误";
                }
            }

            // 获得日历
            nodeInfoItem = readerdom.CreateElement("item");
            nodeInfo.AppendChild(nodeInfoItem);
            DomUtil.SetAttr(nodeInfoItem, "name", "日历名");


            Calendar calendar = null;
            // return:
            //      -1  出错
            //      0   没有找到日历
            //      1   找到日历
            nRet = this.GetReaderCalendar(strReaderType,
                strLibraryCode,
                out calendar,
                out strError);
            if (nRet == -1 || nRet == 0)
            {
                strWarning += strError;
                calendar = null;
                DomUtil.SetAttr(nodeInfoItem, "error", strError);
            }
            else
            {
                if (calendar != null)
                    DomUtil.SetElementText(nodeInfoItem, "value", calendar.Name);
                else
                    DomUtil.SetElementText(nodeInfoItem, "value", "");
            }

            // 全部<borrow>元素
            XmlNodeList borrow_nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");

            int nFreeBorrowCount = Math.Max(0, nMaxBorrowItems - borrow_nodes.Count);

            // 当前还可借
            nodeInfoItem = readerdom.CreateElement("item");
            nodeInfo.AppendChild(nodeInfoItem);
            DomUtil.SetAttr(nodeInfoItem, "name", "当前还可借");
            DomUtil.SetAttr(nodeInfoItem, "value", nFreeBorrowCount.ToString());

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            for (int i = 0; i < borrow_nodes.Count; i++)
            {
                XmlNode node = borrow_nodes[i];

                /*
                string strNo = DomUtil.GetAttr(node, "no");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                string strSummary = "";
                 * */
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");

                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");

                if (StringUtil.IsInList("advancexml_borrow_bibliosummary", strStyle) == true)
                {
                    string strSummary = "";
                    string strBiblioRecPath = "";
                    LibraryServerResult result = this.GetBiblioSummary(
                        sessioninfo,
                        channel,
                        strBarcode,
                        strConfirmItemRecPath,
                        null,
                        out strBiblioRecPath,
                        out strSummary);
                    if (result.Value == -1)
                    {
                        // strSummary = result.ErrorInfo;
                    }
                    else
                    {
                        /*
                        // 截断
                        if (strSummary.Length > 25)
                            strSummary = strSummary.Substring(0, 25) + "...";

                        if (strSummary.Length > 12)
                            strSummary = strSummary.Insert(12, "<br/>");
                         * */
                    }

                    DomUtil.SetAttr(node, "summary", strSummary);
                }

                {
                    string strOverdue = "";
                    long lOver = 0;
                    string strPeriodUnit = "";
                    // 检查超期情况。
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 2009/3/13 
                    nRet = this.CheckPeriod(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                    {
                        DomUtil.SetAttr(node, "isOverdue", "error");
                        strOverdue = strError;
                    }
                    else if (nRet == 1)
                    {
                        DomUtil.SetAttr(node, "isOverdue", "yes");
                        strOverdue = strError;	// "已超期";
                    }
                    else
                    {
                        DomUtil.SetAttr(node, "isOverdue", "no");
                        strOverdue = strError;	// 可能也有一些必要的信息，例如非工作日
                    }

                    DomUtil.SetAttr(node, "overdueInfo", strOverdue);
                }

                {
                    string strOverdue = "";
                    long lOver = 0;
                    string strPeriodUnit = "";
                    // bool bOverdue = false;  // 是否超期

                    DateTime timeReturning = DateTime.MinValue;
                    string strTips = "";

                    DateTime timeNextWorkingDay;

                    // 获得还书日期
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 
                    nRet = this.GetReturningTime(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out timeReturning,
                        out timeNextWorkingDay,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                        strOverdue = strError;
                    else
                    {
                        strTips = strError;
                        if (nRet == 1)
                        {
                            // bOverdue = true;
                            strOverdue = " ("
                                + string.Format(this.GetString("已超期s"),  // 已超期 {0}
                                                this.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                + ")";
                        }
                    }

                    DomUtil.SetAttr(node, "overdueInfo1", strOverdue);
                    DomUtil.SetAttr(node, "timeReturning", DateTimeUtil.Rfc1123DateTimeStringEx(timeReturning.ToLocalTime()));  // 2012/6/1 增加 ToLocalTime()
                }

            }

            if (String.IsNullOrEmpty(strWarning) == true)
            {
#if NO
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "warning", null);
#endif
                DomUtil.DeleteElement(readerdom.DocumentElement,
                    "warning");
            }
            else
            {
                DomUtil.SetElementText(readerdom.DocumentElement,
                    "warning", strWarning);
            }

            // 为 borrowHistory/borrow 元素添加书目摘要属性
            if (StringUtil.IsInList("advancexml_history_bibliosummary", strStyle))
            {
                XmlNodeList history_nodes = readerdom.DocumentElement.SelectNodes("borrowHistory/borrow");
                foreach (XmlElement borrow in history_nodes)
                {
                    string strBarcode = borrow.GetAttribute("barcode");
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;
                    string strConfirmItemRecPath = borrow.GetAttribute("recPath");
                    string strSummary = "";
                    string strBiblioRecPath = "";
                    LibraryServerResult result = this.GetBiblioSummary(
                        sessioninfo,
                        channel,
                        strBarcode,
                        strConfirmItemRecPath,
                        null,
                        out strBiblioRecPath,
                        out strSummary);
                    borrow.SetAttribute("summary", strSummary);
                }
            }

            // 全部<overdue>元素
            bool bFillSummary = StringUtil.IsInList("advancexml_overdue_bibliosummary", strStyle);
            XmlNodeList overdue_nodes = readerdom.DocumentElement.SelectNodes("overdues/overdue");
            foreach (XmlElement node in overdue_nodes)
            {
                // XmlNode node = overdue_nodes[i];
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");

                if (bFillSummary == true)
                {
                    if (String.IsNullOrEmpty(strBarcode) == false)
                    {
                        string strSummary = "";
                        string strBiblioRecPath = "";
                        LibraryServerResult result = this.GetBiblioSummary(
                            sessioninfo,
                            channel,
                            strBarcode,
                            strConfirmItemRecPath,
                            null,
                            out strBiblioRecPath,
                            out strSummary);
                        if (result.Value == -1)
                        {
                            // strSummary = result.ErrorInfo;
                        }

                        DomUtil.SetAttr(node, "summary", strSummary);
                    }
                }

                string strReason = DomUtil.GetAttr(node, "reason");
                string strBorrowDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "borrowDate"));
                string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strReturnDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "returnDate"));
                string strID = DomUtil.GetAttr(node, "id");
                string strPrice = DomUtil.GetAttr(node, "price");
                string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");

                // 以停代金
                string strPauseError = "";
                string strPauseInfo = "";
                if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true
                    && String.IsNullOrEmpty(strOverduePeriod) == false)
                {
                    string strPauseStart = DomUtil.GetAttr(node, "pauseStart");

                    string strUnit = "";
                    long lOverduePeriod = 0;

                    // 分析期限参数
                    nRet = LibraryApplication.ParsePeriodUnit(strOverduePeriod,
                        out lOverduePeriod,
                        out strUnit,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "在分析期限参数的过程中发生错误: " + strError;
                        strPauseError += strError;
                    }

                    long lResultValue = 0;
                    string strPauseCfgString = "";
                    nRet = this.ComputePausePeriodValue(strReaderType,
                        strLibraryCode,
                            lOverduePeriod,
                            out lResultValue,
                        out strPauseCfgString,
                            out strError);
                    if (nRet == -1)
                    {
                        strError = "在计算以停代金周期的过程中发生错误: " + strError;
                        strPauseError += strError;
                    }

                    // text-level: 用户提示
                    /*
                    if (String.IsNullOrEmpty(strPauseStart) == false)
                    {
                        strPauseInfo = "从 " + DateTimeUtil.LocalDate(strPauseStart) + " 开始，";
                    }
                    strPauseInfo += "停借期 " + lResultValue.ToString() + app.GetDisplayTimeUnitLang(strUnit) + " (计算过程如下: 超期 " + lOverduePeriod.ToString() + app.GetDisplayTimeUnitLang(strUnit) + "，读者类型 " + strReaderType + " 的 以停代金因子 为 " + strPauseCfgString + ")";
                     * */
                    if (String.IsNullOrEmpty(strPauseStart) == false)
                    {
                        strPauseInfo = string.Format(this.GetString("从s开始，停借期s"),
                            // "从 {0} 开始，停借期 {1} (计算过程如下: 超期 {2}，读者类型 {3} 的 以停代金因子 为 {4})"
                            DateTimeUtil.LocalDate(strPauseStart),
                            lResultValue.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            lOverduePeriod.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            strReaderType,
                            strPauseCfgString);
                    }
                    else
                    {
                        strPauseInfo = string.Format(this.GetString("停借期s"),
                            // "停借期 {0} (计算过程如下: 超期 {1}，读者类型 {2} 的 以停代金因子 为 {3})"
                            lResultValue.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            lOverduePeriod.ToString() + this.GetDisplayTimeUnitLang(strUnit),
                            strReaderType,
                            strPauseCfgString);
                    }
                }

                if (String.IsNullOrEmpty(strPauseInfo) == false)
                {
                    strPrice = string.Format(this.GetString("违约金或以停代金"),    // "{0} -- 或 -- {1}"
                        strPrice,
                        strPauseInfo);
                    // " 或 "

                    DomUtil.SetAttr(node, "priceString", strPrice);
                }
                else if (String.IsNullOrEmpty(strPauseError) == false)
                {
                    DomUtil.SetAttr(node, "priceString", strPauseError);
                }
            }

            if (StringUtil.IsInList("pauseBorrowing", this.OverdueStyle) == true)
            {
                // 获得日历
                strError = "";
                // 汇报以停代金情况
                string strPauseMessage = "";
                nRet = this.HasPauseBorrowing(
                    calendar,
                    strLibraryCode,
                    readerdom,
                    out strPauseMessage,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strPauseMessage = "在计算以停代金的过程中发生错误: " + strError;
                }
                if (nRet == 1 || String.IsNullOrEmpty(strPauseMessage) == false)
                {
                    XmlNode node = readerdom.DocumentElement.SelectSingleNode("overdues");
                    if (node == null)
                    {
                        node = readerdom.CreateElement("overdues");
                        readerdom.DocumentElement.AppendChild(node);
                    }

                    DomUtil.SetAttr(node, "pauseMessage", strPauseMessage);
                }
            }

            strOutputXml = readerdom.OuterXml;
            return 0;
        }

        // 获得读者信息
        // parameters:
        //      strBarcode  读者证条码号。如果前方引导以"@path:"，则表示读者记录路径。在@path引导下，路径后面还可以跟随 "$prev"或"$next"表示方向
        //                  可以使用读者证号二维码
        //                  TODO: 是否可以使用身份证号?
        //      strResultTypeList   结果类型数组 xml/html/text/calendar/advancexml/recpaths/summary
        //              其中calendar表示获得读者所关联的日历名；advancexml表示经过运算了的提供了丰富附加信息的xml，例如具有超期和停借期附加信息
        //      strRecPath  [out] 读者记录路径。如果命中多个读者记录，这里是逗号分隔的路径列表字符串。最多 100 个路径
        // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
        // 权限: 
        //		工作人员或者读者，必须有getreaderinfo权限
        //		如果为读者, 附加限制还只能看属于自己的读者信息
        public LibraryServerResult GetReaderInfo(
            SessionInfo sessioninfo,
            string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp)
        {
            results = null;
            baTimestamp = null;
            strRecPath = "";

            List<string> recpaths = null;

            LibraryServerResult result = new LibraryServerResult();

            // 个人书斋名
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;

            // 权限判断

            // 权限字符串
            if (StringUtil.IsInList("getreaderinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "读取读者信息被拒绝。不具备getreaderinfo权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            string strError = "";
            // 2007/12/2 
            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "strBarcode参数值不能为空";
                goto ERROR1;
            }

            // 对读者身份的附加判断
            if (sessioninfo.UserType == "reader")
            {
                // TODO: 如果使用身份证号，似乎这里会遇到阻碍
                if (strBarcode[0] != '@'
                    && StringUtil.HasHead(strBarcode, "PQR:") == false)
                {
                    if (StringUtil.IsIdcardNumber(strBarcode) == true)
                    {
                        // 2013/5/20
                        // 延迟判断
                    }
                    else if (strBarcode != sessioninfo.Account.Barcode 
                        && string.IsNullOrEmpty(strPersonalLibrary) == true)
                    {
                        // 注：具有个人书斋的，还可以继续向后执行
                        result.Value = -1;
                        result.ErrorInfo = "获得读者信息被拒绝。作为读者只能察看自己的读者记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // 后面还要判断
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strIdcardNumber = "";
            string strXml = "";

            string strOutputPath = "";

            int nRet = 0;
            long lRet = 0;

            // 前端提供临时记录
            if (strBarcode[0] == '<')
            {
                strXml = strBarcode;
                strRecPath = "?";
                strOutputPath = "?";
                // TODO: 数据库名需要从前端发来的XML记录中获取，或者要知道当前用户的馆代码?
                goto SKIP1;
            }

            bool bOnlyBarcode = false;   // 是否仅仅在 证条码号中寻找

            bool bRecordGetted = false; // 记录释放后已经获取到

            // 命令状态
            if (strBarcode[0] == '@')
            {
                // 获得册记录，通过册记录路径
                string strLeadPath = "@path:";
                string strLeadDisplayName = "@displayName:";
                string strLeadBarcode = "@barcode:";

                /*
                if (strBarcode.Length <= strLead.Length)
                {
                    strError = "错误的检索词格式: '" + strBarcode + "'";
                    goto ERROR1;
                }
                string strPart = strBarcode.Substring(0, strLead.Length);
                 * */
                if (StringUtil.HasHead(strBarcode, strLeadPath) == true)
                {
                    string strReaderRecPath = strBarcode.Substring(strLeadPath.Length);

                    // 2008/6/20 
                    // 继续分离出(方向)命令部分
                    string strCommand = "";
                    nRet = strReaderRecPath.IndexOf("$");
                    if (nRet != -1)
                    {
                        strCommand = strReaderRecPath.Substring(nRet + 1);
                        strReaderRecPath = strReaderRecPath.Substring(0, nRet);
                    }

#if NO
                    string strReaderDbName = ResPath.GetDbName(strReaderRecPath);
                    // 需要检查一下数据库名是否在允许的读者库名之列
                    if (this.IsReaderDbName(strReaderDbName) == false)
                    {
                        strError = "读者记录路径 '" + strReaderRecPath + "' 中的数据库名 '" + strReaderDbName + "' 不在配置的读者库名之列，因此拒绝操作。";
                        goto ERROR1;
                    }
#endif
                    if (this.IsReaderRecPath(strReaderRecPath) == false)
                    {
                        strError = "记录路径 '" + strReaderRecPath + "' 并不是一个读者库记录路径，因此拒绝操作。";
                        goto ERROR1;
                    }

                    string strMetaData = "";

                    // 2008/6/20 changed
                    string strStyle = "content,data,metadata,timestamp,outputpath";

                    if (String.IsNullOrEmpty(strCommand) == false
            && (strCommand == "prev" || strCommand == "next"))
                    {
                        strStyle += "," + strCommand;
                    }

                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strReaderRecPath,
                        sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strReaderRecPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }

                    lRet = channel.GetRes(strReaderRecPath,
                        strStyle,
                        out strXml,
                        out strMetaData,
                        out baTimestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        {
                            result.Value = 0;
                            if (strCommand == "prev")
                                result.ErrorInfo = "到头";
                            else if (strCommand == "next")
                                result.ErrorInfo = "到尾";
                            else
                                result.ErrorInfo = "没有找到";
                            result.ErrorCode = ErrorCode.NotFound;
                            return result;
                        }

                        nRet = -1;
                    }
                    else
                    {
                        nRet = 1;
                    }

                    bRecordGetted = true;
                }
                else if (StringUtil.HasHead(strBarcode, strLeadDisplayName) == true)
                {
                    // 2011/2/19
                    string strDisplayName = strBarcode.Substring(strLeadDisplayName.Length);

                    // 通过读者显示名获得读者记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetReaderRecXmlByDisplayName(
                        // sessioninfo.Channels,
                        channel,
                        strDisplayName,
                        out strXml,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        result.ErrorInfo = "没有找到";
                        result.ErrorCode = ErrorCode.NotFound;
                        return result;
                    }
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputPath,
                        sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                    bRecordGetted = true;
                }
                else if (StringUtil.HasHead(strBarcode, strLeadBarcode) == true)
                {
                    strBarcode = strBarcode.Substring(strLeadBarcode.Length);
                    bOnlyBarcode = true;
                    bRecordGetted = false;
                }
                else
                {
                    strError = "不支持的检索词格式: '" + strBarcode + "'。目前仅支持'@path:'和'@displayName:'引导的检索词";
                    goto ERROR1;
                }

                result.ErrorInfo = strError;
                result.Value = nRet;
                //
            }
            
            // 从证条码号获得
            if (bRecordGetted == false)
            {
                if (string.IsNullOrEmpty(strBarcode) == false)
                {
                    string strOutputCode = "";
                    // 把二维码字符串转换为读者证条码号
                    // parameters:
                    //      strReaderBcode  [out]读者证条码号
                    // return:
                    //      -1      出错
                    //      0       所给出的字符串不是读者证号二维码
                    //      1       成功      
                    nRet = this.DecodeQrCode(strBarcode,
                        out strOutputCode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)
                    {
                        // strQrCode = strBarcode;
                        strBarcode = strOutputCode;
                    }
                }

                // 加读锁
                // 可以避免拿到读者记录处理中途的临时状态
#if DEBUG_LOCK_READER
                this.WriteErrorLog("GetReaderInfo 开始为读者加读锁 '" + strBarcode + "'");
#endif
                this.ReaderLocks.LockForRead(strBarcode);

                try
                {

                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetReaderRecXml(
                        // sessioninfo.Channels,
                        channel,
                        strBarcode,
                        100,
                        sessioninfo.LibraryCodeList,
                        out recpaths,
                        out strXml,
                        // out strOutputPath,
                        out baTimestamp,
                        out strError);

                }
                finally
                {
                    this.ReaderLocks.UnlockForRead(strBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("GetReaderInfo 结束为读者加读锁 '" + strBarcode + "'");
#endif
                }

#if NO
                if (SessionInfo.IsGlobalUser(sessioninfo.LibraryCodeList) == false
                    && nRet > 0)
                {
                    // nRet 被修正
                    nRet = FilterReaderRecPath(ref recpaths,
                        sessioninfo.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
#endif

                if (nRet > 0)
                    strOutputPath = recpaths[0];

                if (nRet == 0)
                {
                    if (bOnlyBarcode == true)
                        goto NOT_FOUND;
                    // 如果是身份证号，则试探检索“身份证号”途径
                    if (StringUtil.IsIdcardNumber(strBarcode) == true)
                    {
                        strIdcardNumber = strBarcode;
                        strBarcode = "";

                        // 通过特定检索途径获得读者记录
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        /*
                        nRet = this.GetReaderRecXmlByFrom(
                            sessioninfo.Channels,
                            strIdcardNumber,
                            "身份证号",
                            out strXml,
                            out strOutputPath,
                            out baTimestamp,
                            out strError);
                         * */
                        nRet = this.GetReaderRecXmlByFrom(
    // sessioninfo.Channels,
    channel,
    null,
    strIdcardNumber,
    "身份证号",
    100,
    sessioninfo.LibraryCodeList,
    out recpaths,
    out strXml,
                            // out strOutputPath,
    out baTimestamp,
    out strError);
                        if (nRet == -1)
                        {
                            // text-level: 内部错误
                            strError = "用身份证号 '" + strIdcardNumber + "' 读入读者记录时发生错误: " + strError;
                            goto ERROR1;
                        }
#if NO
                        if (SessionInfo.IsGlobalUser(sessioninfo.LibraryCodeList) == false
                            && nRet > 0)
                        {
                            // nRet 被修正
                            nRet = FilterReaderRecPath(ref recpaths,
                                sessioninfo.LibraryCodeList,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
#endif

                        if (nRet == 0)
                        {
                            result.Value = 0;
                            // text-level: 用户提示
                            result.ErrorInfo = string.Format(this.GetString("身份证号s不存在"),   // "身份证号 '{0}' 不存在"
                                strIdcardNumber);
                            result.ErrorCode = ErrorCode.IdcardNumberNotFound;
                            return result;
                        }



                        if (nRet > 0)
                            strOutputPath = recpaths[0];

                        /*
                 * 不必明显报错，前端从返回值已经可以看出有重
                        if (nRet > 1)
                        {
                            // text-level: 用户提示
                            result.Value = -1;
                            result.ErrorInfo = "用身份证号 '" + strIdcardNumber + "' 检索读者记录命中 " + nRet.ToString() + " 条，因此无法用身份证号来进行借还操作。请改用证条码号来进行借还操作。";
                            result.ErrorCode = ErrorCode.IdcardNumberDup;
                            return result;
                        }
                        Debug.Assert(nRet == 1, "");
                         * */

                        result.ErrorInfo = strError;
                        result.Value = nRet;
                        goto SKIP0;
                    }
                    else
                    {
                        // 如果需要，从读者证号等辅助途径进行检索
                        foreach (string strFrom in this.PatronAdditionalFroms)
                        {
                            nRet = this.GetReaderRecXmlByFrom(
// sessioninfo.Channels,
channel,
null,
strBarcode,
strFrom,
100,
sessioninfo.LibraryCodeList,
out recpaths,
out strXml,
out baTimestamp,
out strError);
                            if (nRet == -1)
                            {
                                // text-level: 内部错误
                                strError = "用" + strFrom + " '" + strBarcode + "' 读入读者记录时发生错误: " + strError;
                                goto ERROR1;
                            }

#if NO
                            if (SessionInfo.IsGlobalUser(sessioninfo.LibraryCodeList) == false
                                && nRet > 0)
                            {
                                // nRet 被修正
                                nRet = FilterReaderRecPath(ref recpaths,
                                    sessioninfo.LibraryCodeList,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                            }
#endif

                            if (nRet == 0)
                                continue;

                            if (nRet > 0)
                                strOutputPath = recpaths[0];

                            result.ErrorInfo = strError;
                            result.Value = nRet;
                            goto SKIP0;
                        }
                    }

                NOT_FOUND:
                    result.Value = 0;
                    result.ErrorInfo = "没有找到";
                    result.ErrorCode = ErrorCode.NotFound;
                    return result;
                }


                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (this.IsCurrentChangeableReaderPath(strOutputPath,
                    sessioninfo.LibraryCodeList) == false)
                {
                    strError = "读者记录路径 '" + strOutputPath + "' 的读者库不在当前用户管辖范围内";
                    goto ERROR1;
                }


                /*
                 * 不必明显报错，前端从返回值已经可以看出有重
                if (nRet > 1)
                {
                    result.Value = nRet;
                    result.ErrorInfo = "读者证条码号 '" +strBarcode+ "' 命中 " +nRet.ToString() + " 条。这是一个严重错误，请系统管理员尽快排除。";
                    result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                    return result;
                }
                 * */

                if (nRet == -1)
                    goto ERROR1;

                result.ErrorInfo = strError;
                result.Value = nRet;
            }

        SKIP0:
            // strRecPath = strOutputPath;
            // 2013/5/21
            if (recpaths != null)
                strRecPath = StringUtil.MakePathList(recpaths);
            else
                strRecPath = strOutputPath;

        SKIP1:
            if (String.IsNullOrEmpty(strResultTypeList) == true)
            {
                results = null; // 不返回任何结果
                return result;
            }

            XmlDocument readerdom = null;
            if (sessioninfo.UserType == "reader")
            {
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入 XML DOM 时发生错误: " + strError;
                    goto ERROR1;
                }


                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader"
                    && string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    string strBarcode1 = DomUtil.GetElementText(readerdom.DocumentElement,
            "barcode");
                    if (strBarcode1 != sessioninfo.Account.Barcode)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "获得读者信息被拒绝。作为读者只能察看自己的读者记录";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
            }

            string strLibraryCode = "";
            if (strRecPath == "?")
            {
                // 从当前用户管辖的馆代码中选择第一个
                // TODO: 如果发来的XML记录中有读者库名和馆代码帮助判断则更好
                List<string> librarycodes = StringUtil.FromListString(sessioninfo.LibraryCodeList);
                if (librarycodes != null && librarycodes.Count > 0)
                    strLibraryCode = librarycodes[0];
                else
                    strLibraryCode = "";
            }
            else
            {
                nRet = this.GetLibraryCode(strRecPath,
                    out strLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            string[] result_types = strResultTypeList.Split(new char[] { ',' });
            results = new string[result_types.Length];

            for (int i = 0; i < result_types.Length; i++)
            {
                string strResultType = result_types[i];

                // 2008/4/3 
                // if (String.Compare(strResultType, "calendar", true) == 0)
                if (IsResultType(strResultType, "calendar") == true)
                {
                    if (readerdom == null)
                    {
                        nRet = LibraryApplication.LoadToDom(strXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }

                    string strReaderType = DomUtil.GetElementText(readerdom, "readerType");

                    // 获得日历
                    DigitalPlatform.LibraryServer.Calendar calendar = null;
                    // return:
                    //      -1  出错
                    //      0   没有找到日历
                    //      1   找到日历
                    nRet = this.GetReaderCalendar(strReaderType,
                        strLibraryCode,
                        out calendar,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        calendar = null;
                    }

                    string strCalendarName = "";

                    if (calendar != null)
                        strCalendarName = calendar.Name;

                    results[i] = strCalendarName;
                }
                // else if (String.Compare(strResultType, "xml", true) == 0)
                else if (IsResultType(strResultType, "xml") == true)
                {
                    // results[i] = strXml;
                    string strResultXml = "";
                    nRet = GetItemXml(strXml,
        strResultType,
        out strResultXml,
        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取 " + strResultType + " 格式的 XML 字符串时出错: " + strError;
                        goto ERROR1;
                    }
                    results[i] = strResultXml;
                }
                else if (String.Compare(strResultType, "timestamp", true) == 0)
                {
                    // 2011/1/27
                    results[i] = ByteArray.GetHexTimeStampString(baTimestamp);
                }
                else if (String.Compare(strResultType, "recpaths", true) == 0)
                {
                    // 2013/5/21
                    if (recpaths != null)
                        results[i] = StringUtil.MakePathList(recpaths);
                    else
                        results[i] = strOutputPath;
                }
                else if (String.Compare(strResultType, "advancexml_borrow_bibliosummary", true) == 0
                    || String.Compare(strResultType, "advancexml_overdue_bibliosummary", true) == 0
                    || String.Compare(strResultType, "advancexml_history_bibliosummary", true) == 0
                    )
                {
                    // 2011/1/27
                    continue;
                }
                // else if (String.Compare(strResultType, "summary", true) == 0)
                else if (IsResultType(strResultType, "summary") == true)
                {
                    // 2013/11/15
                    string strSummary = "";
                    XmlDocument dom = new XmlDocument();
                    try {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strSummary = "读者 XML 装入 DOM 出错: " +ex.Message;
                        results[i] = strSummary;
                        continue;
                    }
                    strSummary = DomUtil.GetElementText(dom.DocumentElement, "name");
                    results[i] = strSummary;
                }
                // else if (String.Compare(strResultType, "advancexml", true) == 0)
                else if (IsResultType(strResultType, "advancexml") == true)
                {
                    // 2008/4/3 
                    string strOutputXml = "";
                    nRet = this.GetAdvanceReaderXml(
                        sessioninfo,
                        strResultTypeList,  // strResultType, BUG!!! 2012/4/8
                        strLibraryCode,
                        strXml,
                        out strOutputXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "GetAdvanceReaderXml()出错: " + strError;
                        goto ERROR1;
                    }
                    results[i] = strOutputXml;
                }
                // else if (String.Compare(strResultType, "html", true) == 0)
                else if (IsResultType(strResultType, "html") == true)
                {

                    string strReaderRecord = "";
                    // 将读者记录数据从XML格式转换为HTML格式
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        this.CfgDir + "\\readerxml2html.cs",
                        this.CfgDir + "\\readerxml2html.cs.ref",
                        strLibraryCode,
                        strXml,
                        strOutputPath,  // 2009/10/18 
                        OperType.None,
                        null,
                        "",
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()出错(脚本程序为" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    // test strReaderRecord = "<html><body><p>test</p></body></html>";
                    results[i] = strReaderRecord;
                }
                // else if (String.Compare(strResultType, "text", true) == 0)
                else if (IsResultType(strResultType, "text") == true)
                {
                    string strReaderRecord = "";
                    // 将读者记录数据从XML格式转换为text格式
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        this.CfgDir + "\\readerxml2text.cs",
                        this.CfgDir + "\\readerxml2text.cs.ref",
                        strLibraryCode,
                        strXml,
                        strOutputPath,  // 2009/10/18 
                        OperType.None,
                        null,
                        "",
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()出错(脚本程序为" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    results[i] = strReaderRecord;
                }
                else
                {
                    strError = "未知的结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }
            }

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        static bool IsResultType(string strResultType, string strName)
        {
            if (String.Compare(strResultType, strName, true) == 0
                   || StringUtil.HasHead(strResultType, strName + ":") == true)
                return true;
            return false;
        }

        // 根据馆代码，将不被管辖的 读者库记录路径字符串 筛选删除
        // return:
        //      -1  出错
        //      其他  recpaths 数组的元素总数
        int FilterReaderRecPath(ref List<string> recpaths,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            if (recpaths == null)
                return 0;

            List<string> results = new List<string>();
            foreach (string strReaderRecPath in recpaths)
            {
                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (this.IsCurrentChangeableReaderPath(strReaderRecPath,
                    strLibraryCodeList) == true)
                    results.Add(strReaderRecPath);
            }

            recpaths = results;
            return recpaths.Count;
        }

        // 检测一下数据库名是否在允许的读者库名之列
        public bool IsReaderRecPath(string strRecPath)
        {
            string strReaderDbName = ResPath.GetDbName(strRecPath);
            return this.IsReaderDbName(strReaderDbName);
        }

        // 移动读者记录
        // parameters:
        //      strTargetRecPath    [in][out]目标记录路径
        // return:
        // result.Value:
        //      -1  error
        //      0   已经成功移动
        // 权限：
        //      需要movereaderinfo权限
        // 日志:
        //      要产生日志
        public LibraryServerResult MoveReaderInfo(
            SessionInfo sessioninfo,
            string strSourceRecPath,
            ref string strTargetRecPath,
            out byte [] target_timestamp)
        {
            string strError = "";
            target_timestamp = null;
            int nRet = 0;
            long lRet = 0;
            // bool bChanged = false;  // 是否发生过实质性改动

            LibraryServerResult result = new LibraryServerResult();

            // 权限字符串
            if (StringUtil.IsInList("movereaderinfo", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "移动读者记录的操作被拒绝。不具备movereaderinfo权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // 检查源和目标记录路径不能相同
            if (strSourceRecPath == strTargetRecPath)
            {
                strError = "源和目标读者记录路径不能相同";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strSourceRecPath) == true)
            {
                strError = "源读者记录路径不能为空";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strTargetRecPath) == true)
            {
                strError = "目标读者记录路径不能为空";
                goto ERROR1;
            }

            // 检查两个路径是否都是读者库路径
            if (this.IsReaderRecPath(strSourceRecPath) == false)
            {
                strError = "strSourceRecPath参数所给出的源记录路径 '"+strSourceRecPath+"' 并不是一个读者库记录路径";
                goto ERROR1;
            }
            if (this.IsReaderRecPath(strTargetRecPath) == false)
            {
                strError = "strTargetRecPath参数所给出的目标记录路径 '" + strTargetRecPath + "' 并不是一个读者库记录路径";
                goto ERROR1;
            }
            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "channel == null";
                goto ERROR1;
            }

            // 读出源记录
            string strExistingSourceXml = "";
            byte[] exist_soutce_timestamp = null;
            string strTempOutputPath = "";
            string strMetaData = "";
            int nRedoCount = 0;

        REDOLOAD:

            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(strSourceRecPath,
                out strExistingSourceXml,
                out strMetaData,
                out exist_soutce_timestamp,
                out strTempOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strError = "源记录 '" + strSourceRecPath + "' 不存在";
                    // errorcode = channel.OriginErrorCode;
                    goto ERROR1;
                }
                else
                {
                    strError = "移动操作发生错误, 在读入源记录 '" + strSourceRecPath + "' 阶段:" + strError;
                    // errorcode = channel.OriginErrorCode;
                    goto ERROR1;
                }
            }

            string strSourceLibraryCode = "";
            string strTargetLibraryCode = "";
            // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
            if (String.IsNullOrEmpty(strTempOutputPath) == false)
            {
                // 检查当前操作者是否管辖这个读者库
                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (this.IsCurrentChangeableReaderPath(strTempOutputPath,
        sessioninfo.LibraryCodeList,
        out strSourceLibraryCode) == false)
                {
                    strError = "源读者记录路径 '" + strTempOutputPath + "' 从属的读者库不在当前用户管辖范围内";
                    goto ERROR1;
                }
            }

            // 把记录装入DOM
            XmlDocument domExist = new XmlDocument();
            try
            {
                domExist.LoadXml(strExistingSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistingSourceXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            string strLockBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                "barcode");

            // 加读者记录锁
            if (String.IsNullOrEmpty(strLockBarcode) == false)
            {
#if DEBUG_LOCK_READER
                this.WriteErrorLog("MoveReaderInfo 开始为读者加写锁 '" + strLockBarcode + "'");
#endif
                this.ReaderLocks.LockForWrite(strLockBarcode);
            }
            try
            {
                // 锁定后重新读入一次源读者记录。这是因为担心第一次为了获得证条码号的读取和锁定之间存在可能被其他地方修改了此条记录的可能
                byte[] temp_timestamp = null;
                lRet = channel.GetRes(strSourceRecPath,
                    out strExistingSourceXml,
                    out strMetaData,
                    out temp_timestamp,
                    out strTempOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "移动操作发生错误, 在重新读入源记录 '" + strSourceRecPath + "' 阶段:" + strError;
                    goto ERROR1;
                }

                nRet = ByteArray.Compare(exist_soutce_timestamp, temp_timestamp);
                if (nRet != 0)
                {
                    // 重新把记录装入DOM
                    domExist = new XmlDocument();
                    try
                    {
                        domExist.LoadXml(strExistingSourceXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strExistingSourceXml装载进入DOM时发生错误(2): " + ex.Message;
                        goto ERROR1;
                    }

                    // 重新核对条码号
                    if (strLockBarcode != DomUtil.GetElementText(domExist.DocumentElement,
                "barcode"))
                    {
                        if (nRedoCount < 10)
                        {
                            nRedoCount++;
                            goto REDOLOAD;
                        }
                        strError = "争夺锁定过程中发生太多次的错误。请稍后重试移动操作";
                        goto ERROR1;
                    }

                    exist_soutce_timestamp = temp_timestamp;
                }


                // 检查即将覆盖的目标位置是不是有记录，如果有，则不允许进行move操作。
                bool bAppendStyle = false;  // 目标路径是否为追加形态？
                string strTargetRecId = ResPath.GetRecordId(strTargetRecPath);

                if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                {
                    // 2009/11/1 
                    if (String.IsNullOrEmpty(strTargetRecId) == true)
                        strTargetRecPath += "/?";

                    bAppendStyle = true;
                }


                if (bAppendStyle == false)
                {
                    string strExistTargetXml = "";
                    byte[] exist_target_timestamp = null;
                    string strOutputPath = "";

                    // 获取覆盖目标位置的现有记录
                    lRet = channel.GetRes(strTargetRecPath,
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
                        }
                        else
                        {
                            strError = "移动操作发生错误, 在读入即将覆盖的目标位置 '" + strTargetRecPath + "' 原有记录阶段:" + strError;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        // 如果记录存在，则目前不允许这样的操作
                        strError = "移动操作被拒绝。因为在即将覆盖的目标位置 '" + strTargetRecPath + "' 已经存在记录。除非先删除(delete)这条记录，才能进行移动(move)操作";
                        goto ERROR1;
                    }
                }

                // 看看读者记录所从属的读者库的馆代码，是否被当前用户管辖
                if (String.IsNullOrEmpty(strTargetRecPath) == false)
                {
                    // 检查当前操作者是否管辖这个读者库
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strTargetRecPath,
            sessioninfo.LibraryCodeList,
            out strTargetLibraryCode) == false)
                    {
                        strError = "目标读者记录路径 '" + strTargetRecPath + "' 从属的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }
                }

                // 移动记录
                // byte[] output_timestamp = null;
                string strOutputRecPath = "";

                // TODO: Copy后还要写一次？因为Copy并不写入新记录。
                // 其实Copy的意义在于带走资源。否则还不如用Save+Delete
                lRet = channel.DoCopyRecord(strSourceRecPath,
                     strTargetRecPath,
                     true,   // bDeleteSourceRecord
                     out target_timestamp,
                     out strOutputRecPath,
                     out strError);
                if (lRet == -1)
                {
                    strError = "DoCopyRecord() error :" + strError;
                    goto ERROR1;
                }

                strTargetRecPath = strOutputRecPath;

                /*
                if (String.IsNullOrEmpty(strNewBiblio) == false)
                {
                    this.BiblioLocks.LockForWrite(strOutputRecPath);

                    try
                    {
                        // TODO: 如果新的、已存在的xml没有不同，或者新的xml为空，则这步保存可以省略
                        string strOutputBiblioRecPath = "";
                        lRet = channel.DoSaveTextRes(strOutputRecPath,
                            strNewBiblio,
                            false,
                            "content", // ,ignorechecktimestamp
                            output_timestamp,
                            out baOutputTimestamp,
                            out strOutputBiblioRecPath,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        this.BiblioLocks.UnlockForWrite(strOutputRecPath);
                    }
                }
                */

            }
            catch (Exception ex)
            {
                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = "抛出异常:" + ex.Message;
                return result;
            }
            finally
            {
                if (String.IsNullOrEmpty(strLockBarcode) == false)
                {
                    this.ReaderLocks.UnlockForWrite(strLockBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("MoveReaderInfo 结束为读者加写锁 '" + strLockBarcode + "'");
#endif
                }
            }

            XmlDocument domOperLog = new XmlDocument();
            domOperLog.LoadXml("<root />");
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "libraryCode",
    strSourceLibraryCode + "," + strTargetLibraryCode);    // 读者所在的馆代码
            DomUtil.SetElementText(domOperLog.DocumentElement,
                "operation", "setReaderInfo");
            DomUtil.SetElementText(domOperLog.DocumentElement,
    "action", "move");

            string strOperTimeString = this.Clock.GetClock();   // RFC1123格式

            DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                sessioninfo.UserID);
            DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                strOperTimeString);

            XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "record", "");
            DomUtil.SetAttr(node, "recPath", strTargetRecPath);

            node = DomUtil.SetElementText(domOperLog.DocumentElement,
                "oldRecord", strExistingSourceXml);
            DomUtil.SetAttr(node, "recPath", strSourceRecPath);

            nRet = this.OperLog.WriteOperLog(domOperLog,
                sessioninfo.ClientAddress,
                out strError);
            if (nRet == -1)
            {
                strError = "MoveReaderInfo() API 写入日志时发生错误: " + strError;
                goto ERROR1;
            }

            result.Value = 1;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 为两个读者记录互相添加好友关系
        public int AddFriends(
            SessionInfo sessioninfo,
            string strReaderBarcode1,
            string strReaderBarcode2,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            List<string> barcodes = new List<string>();
            barcodes.Add(strReaderBarcode1);
            barcodes.Add(strReaderBarcode2);

            barcodes.Sort();

#if DEBUG_LOCK_READER
            this.WriteErrorLog("AddFriends 开始为读者加写锁 '" + barcodes[0] + "' 和 '" + barcodes[1] + "'");
#endif

            // 加读者记录锁
            // 排序后加锁，可以防止死锁
            this.ReaderLocks.LockForWrite(barcodes[0]);
            this.ReaderLocks.LockForWrite(barcodes[1]);

            try // 读者记录锁定范围开始
            {

                // 读入读者记录
                string strReaderXml1 = "";
                byte[] reader_timestamp1 = null;
                string strOutputReaderRecPath1 = "";
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode1,
                    out strReaderXml1,
                    out strOutputReaderRecPath1,
                    out reader_timestamp1,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "读入读者记录 '" + strReaderBarcode1 + "' 时发生错误: " + strError;
                    return -1;
                }

                string strReaderXml2 = "";
                byte[] reader_timestamp2 = null;
                string strOutputReaderRecPath2 = "";
                nRet = this.GetReaderRecXml(
                    // sessioninfo.Channels,
                    channel,
                    strReaderBarcode2,
                    out strReaderXml2,
                    out strOutputReaderRecPath2,
                    out reader_timestamp2,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "读入读者记录 '" + strReaderBarcode2 + "' 时发生错误: " + strError;
                    return -1;
                }

                XmlDocument readerdom1 = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml1,
                    out readerdom1,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "装载读者记录 '" + strReaderBarcode1 + "' 进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                XmlDocument readerdom2 = null;
                nRet = LibraryApplication.LoadToDom(strReaderXml2,
                    out readerdom2,
                    out strError);
                if (nRet == -1)
                {
                    // text-level: 内部错误
                    strError = "装载读者记录 '" + strReaderBarcode2 + "' 进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                string strFriends1 = DomUtil.GetElementText(readerdom1.DocumentElement, "friends");
                string strFriends2 = DomUtil.GetElementText(readerdom2.DocumentElement, "friends");

                string strNewFriends1 = strFriends1;
                string strNewFriends2 = strFriends2;

                StringUtil.SetInList(ref strNewFriends1, strReaderBarcode2, true);
                StringUtil.SetInList(ref strNewFriends2, strReaderBarcode1, true);

                if (strNewFriends1 == strFriends1)
                    readerdom1 = null;

                if (strNewFriends2 == strFriends2)
                    readerdom2 = null;

#if NO
                RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
                if (channel == null)
                {
                    strError = "get channel error";
                    return -1;
                }
#endif

                // 写回读者记录
                if (readerdom1 != null)
                {
                    byte[] output_timestamp1 = null;
                    string strOutputPath1 = "";
                    long lRet = channel.DoSaveTextRes(strOutputReaderRecPath1,
                        readerdom1.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp1,
                        out output_timestamp1,
                        out strOutputPath1,
                        out strError);
                    if (lRet == -1)
                    {
                        // text-level: 内部错误
                        strError = "写入读者记录 '" + strReaderBarcode1 + "' 过程中，发生错误: " + strError;
                        return -1;
                    }
                }

                // 写回读者记录
                if (readerdom2 != null)
                {
                    byte[] output_timestamp2 = null;
                    string strOutputPath2 = "";
                    long lRet = channel.DoSaveTextRes(strOutputReaderRecPath2,
                        readerdom2.OuterXml,
                        false,
                        "content",  // ,ignorechecktimestamp
                        reader_timestamp2,
                        out output_timestamp2,
                        out strOutputPath2,
                        out strError);
                    if (lRet == -1)
                    {
                        // text-level: 内部错误
                        strError = "写入读者记录 '" + strReaderBarcode1 + "' 过程中，发生错误: " + strError;
                        return -1;
                    }
                }
            } // 读者记录锁定范围结束
            finally
            {
                this.ReaderLocks.UnlockForWrite(barcodes[1]);
                this.ReaderLocks.UnlockForWrite(barcodes[0]);
#if DEBUG_LOCK_READER
                this.WriteErrorLog("AddFriends 结束为读者加写锁 '" + barcodes[1] + "' 和 '" + barcodes[0] + "'");
#endif
            }

            return 0;
        }
    }
}
