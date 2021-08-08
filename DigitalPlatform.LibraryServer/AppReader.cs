
using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
using DigitalPlatform.Core;
using Newtonsoft.Json;
using System.Linq;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是读者相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 读者记录中 要害元素名列表。注意没有包含 borrows 等流通信息元素
        static string[] _reader_element_names = new string[] {
                "barcode",
                "state",
                "readerType",
                "createDate",
                "expireDate",
                "name",
                "namePinyin",   // 2013/12/20
                "gender",
                "birthday",     // 注：逐步废止这个元素，用 dateOfBirth 替代
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
                "palmprint",    // 2020/12/29
                "rights", // 2014/7/8
                "personalLibrary", // 2014/7/8
                "friends", // 2014/9/9
                "access",   // 2014/9/10
                "refID", // 2015/9/12
                "face", // 2018/12/16
                "http://dp2003.com/dprms:file", // 2021/7/19
            };

        // 读者记录中 读者自己能修改的元素名列表
        static string[] _selfchangeable_reader_element_names = new string[] {
                "displayName",  // 显示名
                "preference",   // 个性化参数
            };

        // 更换 domTarget 中所有 dprms:file 元素
        // 算法是：删除target中的全部<dprms:file>元素，然后将source记录中的全部<dprms:file>元素插入到target记录中
        // return:
        //      false   没有发生实质性改变
        //      true    发生了实质性改变
        public static bool MergeDprmsFile(ref XmlDocument domTarget,
            XmlDocument domSource)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            List<string> oldOuterXmls = new List<string>();
            List<string> newOuterXmls = new List<string>();

            // 删除 target 中的全部 <dprms:file> 元素
            XmlNodeList nodes = domTarget.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                if (node.ParentNode != null)
                {
                    oldOuterXmls.Add(node.OuterXml);
                    node.ParentNode.RemoveChild(node);
                }
            }

            // 然后将source记录中的全部<dprms:file>元素插入到target记录中
            nodes = domSource.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                newOuterXmls.Add(node.OuterXml);

                XmlDocumentFragment fragment = domTarget.CreateDocumentFragment();
                fragment.InnerXml = node.OuterXml;

                domTarget.DocumentElement.AppendChild(fragment);
            }

            // 比较 oldOuterXmls 和 newOuterXmls 之间是否有差异
            return oldOuterXmls.SequenceEqual(newOuterXmls);
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
            return IsCurrentChangeableReaderPath(strReaderRecPath,
                strAccountLibraryCodeList,
                out string strLibraryCode);
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
            string[] parts = strList.Split(new char[] { ',' });
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

        // TODO: 需要硬编码禁止覆盖一些流通专用的字段 borrows 等
        // <DoReaderChange()的下级函数>
        // 合并新旧记录
        // parameters:
        //      important_fields    重要的字段名列表。要检查这些字段是否没有被采纳，如果没有被采纳要报错
        public static int MergeTwoReaderXml(
            string[] element_names,
            string strAction,
            XmlDocument domExistParam,
            XmlDocument domNew,
            string[] important_fields,
            // out string strMergedXml,
            out XmlDocument domMerged,
            out string strError)
        {
            domMerged = null;
            // strMergedXml = "";
            strError = "";

            if (strAction == "changestate")
            {
                element_names = new string[] {
                    "state",
                    "comment",
                    };
            }
            else if (strAction == "changeforegift")
            {
                // 2008/11/11 
                element_names = new string[] {
                    "foregift",
                    "comment",
                    };
            }

            // 2021/7/21
            // 超出 element_names 的部分元素名
            List<string> outof_names = new List<string>();
            if (important_fields != null && important_fields.Length > 0)
            {
                outof_names = new List<string>(important_fields);
                foreach (string s in element_names)
                {
                    outof_names.Remove(s);
                }
                StringUtil.RemoveBlank(ref outof_names);
                /*
                if (outof_names.Count > 0)
                {
                    strImportantError = $"下列元素超过当前用户权限: '{StringUtil.MakePathList(temp)}'";
                }
                */
            }

            // 检查超出部分元素的内容是否即将发生变化
            // 实际发生了内容修改的、超出权限的元素名集合
            List<string> error_names = new List<string>();
            foreach (var strElementName in outof_names)
            {
                /*
                string old_outerXml = GetOuterXml(domExistParam, strElementName);
                string new_outerXml = GetOuterXml(domNew, strElementName);

                if (old_outerXml != new_outerXml)
                    error_names.Add(strElementName);
                */
                // 如果是 fingerprint face palmprint 元素，则只比较 InnerText
                if (strElementName == "fingerprint"
                    || strElementName == "face"
                    || strElementName == "palmprint")
                {
                    string old_innerText = DomUtil.GetElementText(domExistParam.DocumentElement, strElementName);
                    string new_innerText = DomUtil.GetElementText(domNew.DocumentElement, strElementName);

                    if (old_innerText != new_innerText)
                        error_names.Add(strElementName);
                }
                else
                {
                    string old_outerXml = GetOuterXml(domExistParam, strElementName);
                    string new_outerXml = GetOuterXml(domNew, strElementName);

                    if (old_outerXml != new_outerXml)
                        error_names.Add(strElementName);
                }
            }

            if (error_names.Count > 0)
            {
                strError = $"下列元素超过当前用户权限: '{StringUtil.MakePathList(error_names)}'";
                return -1;
            }

            // 2021/7/14
            // 确保 domExistParam 参数不会在本函数内被修改，只修改 domExist
            var domExist = new XmlDocument();
            domExist.LoadXml(domExistParam.OuterXml);

            // 2021/7/19
            bool has_file_element = Array.IndexOf(element_names, "http://dp2003.com/dprms:file") != -1;
            if (has_file_element)
            {
                List<string> temp = new List<string>(element_names);
                temp.Remove("http://dp2003.com/dprms:file");
                element_names = temp.ToArray();
            }

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
                for (int i = 0; i < element_names.Length; i++)
                {
                    string strElementName = element_names[i];
                    // <foregift>元素内容不让SetReaderInfo() API的change action修改
                    if (strElementName == "foregift")
                        continue;

                    // 2021/7/23
                    // string old_outerXml = domExist.DocumentElement.SelectSingleNode(strElementName)?.OuterXml;

                    // 2006/11/29 changed
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        strElementName);

                    // 2013/1/15 <fingerprint>元素单独处理
                    if (strElementName == "fingerprint"
                        || strElementName == "palmprint"
                        || strElementName == "face")
                    {
                        string strTextOld = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                            strElementName);

                        if (string.IsNullOrEmpty(DomUtil.GetElementText(domNew.DocumentElement,
                        strElementName)))
                        {
                            // 2016/4/27
                            // 删除 fingerprint 元素
                            DomUtil.DeleteElement(domExist.DocumentElement, strElementName);
                        }
                        else
                        {
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
                        }
                    }
                    // 2013/6/19 <hire>元素单独处理
                    // 保护 expireDate 属性不被修改
                    else if (strElementName == "hire")
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
                        {
                            // DomUtil.SetAttr(domExist.DocumentElement, "expireDate", strExistExpireDate); // bug!!! 2020/5/19

                            DomUtil.SetAttr(nodeExist, "expireDate", strExistExpireDate);   // 2020/5/19 改掉 bug
                        }
                        else if (string.IsNullOrEmpty(strExistExpireDate) == false)
                        {
                            XmlNode node = DomUtil.SetElementText(domExist.DocumentElement,
                                strElementName,
                                "");
                            DomUtil.SetAttr(node, "expireDate", strExistExpireDate);
                        }
                    }
                    else
                    {
                        // 一般处理
                        DomUtil.SetElementOuterXml(domExist.DocumentElement,
                            strElementName,
                            strTextNew);
                    }

                    /*
                    // 2021/7/23
                    // 判断是否发生了变化
                    string changed_outerXml = domExist.DocumentElement.SelectSingleNode(strElementName)?.OuterXml;
                    if (old_outerXml != changed_outerXml
&& outof_names.IndexOf(strElementName) != -1)
                    {
                        error_names.Add(strElementName);
                    }
                    */
                }

                if (has_file_element)
                {
                    // 删除target中的全部<dprms:file>元素，然后将source记录中的全部<dprms:file>元素插入到target记录中
                    MergeDprmsFile(ref domExist,
                        domNew);
                }
            }
            else if (strAction == "changestate")
            {
                /*
                string[] element_names_onlystate = new string[] {
                    "state",
                    "comment",
                    };
                */
                var element_names_onlystate = element_names;
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
                /*
                // 2008/11/11 
                string[] element_names_onlyforegift = new string[] {
                    "foregift",
                    "comment",
                    };
                */
                var element_names_onlyforegift = element_names;
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
                strError = "strAction 值必须为 change、changestate、changeforegift 和 changereaderbarcode 之一。";
                return -1;
            }

            // 2015/9/12
            // 如果记录中没有 refID 字段，则主动填充
            string strRefID = DomUtil.GetElementText(domExist.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID) == true)
                DomUtil.SetElementText(domExist.DocumentElement, "refID", Guid.NewGuid().ToString());

            // 2020/5/19 
            // 删除以前误为根元素添加的 expireDate 属性
            domExist.DocumentElement.RemoveAttribute("expireDate");

            // strMergedXml = domExist.OuterXml;
            domMerged = domExist;
            return 0;
        }

        static string GetOuterXml(XmlDocument domTarget,
            string element_name)
        {
            XmlNodeList nodes = null;
            if (element_name.Contains(":"))
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);
                element_name = element_name.Replace(DpNs.dprms, "dprms");
                nodes = domTarget.DocumentElement.SelectNodes("//" + element_name, nsmgr);   // "//dprms:file"
            }
            else
                nodes = domTarget.DocumentElement.SelectNodes(element_name);

            if (nodes.Count == 0)
                return null;

            List<string> oldOuterXmls = new List<string>();
            foreach (XmlElement element in nodes)
            {
                oldOuterXmls.Add(element.OuterXml);
            }

            /*
            // TODO: 是否要排序?
            if (oldOuterXmls.Count > 0)
                oldOuterXmls.Sort();
            */

            return StringUtil.MakePathList(oldOuterXmls, "\r\n");
        }

        // 构造出适合保存的新读者记录
        // 主要是为了把待加工的记录中，可能出现的属于“流通信息”的字段去除，避免出现安全性问题
        // return:
        //      -1  出错
        //      0   没有实质性修改
        //      1   发生了实质性修改
        int BuildNewReaderRecord(XmlDocument domNewRec,
            string[] important_fields,
            string rights,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            // 流通元素名列表
            string[] remove_element_names = new string[] {
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

            // *** 滤掉 setreaderinfo:n 权限不包含的那些字段
            // 2021/7/15
            // return:
            //      null    没有找到 getreaderinfo 前缀
            //      ""      找到了前缀，并且 level 部分为空
            //      其他     返回 level 部分
            string write_level = GetReaderInfoLevel("setreaderinfo", rights);
            if (FilterByLevel(dom, write_level, "write") == true)
                bChanged = true;

            // 删除一些敏感元素
            for (int i = 0; i < remove_element_names.Length; i++)
            {
                List<XmlNode> deleted_nodes = DomUtil.DeleteElements(dom.DocumentElement,
                    remove_element_names[i]);
                if (deleted_nodes != null
                    && deleted_nodes.Count > 0)
                    bChanged = true;
            }

            {
                // 如果有已经有了 <fingerprint> 元素，则修正其 timestamp 属性
                // 刷新 timestamp 属性
                XmlNode node = dom.DocumentElement.SelectSingleNode("fingerprint");
                if (node != null)
                    DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));
            }

            {
                // 如果有已经有了<face>元素，则修正其timestamp属性
                // 刷新timestamp属性
                XmlNode node = dom.DocumentElement.SelectSingleNode("face");
                if (node != null)
                    DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));
            }

            {
                // 如果有已经有了 <palmprint> 元素，则修正其 timestamp 属性
                // 刷新 timestamp 属性
                XmlNode node = dom.DocumentElement.SelectSingleNode("palmprint");
                if (node != null)
                    DomUtil.SetAttr(node, "timestamp", DateTime.Now.ToString("u"));
            }

            // 设置首次密码
            string strBirthDate = DomUtil.GetElementText(dom.DocumentElement, "dateOfBirth");
            string strNewPassword = "";
            try
            {
                if (string.IsNullOrEmpty(strBirthDate) == false
                    && StringUtil.IsInList("style-1", _patronPasswordStyle) == false)
                    strNewPassword = DateTimeUtil.DateTimeToString8(DateTimeUtil.FromRfc1123DateTimeString(strBirthDate).ToLocalTime());    // 2015/10/27 修改 bug。原来缺 ToLocalTime()，造成产生的字符串是前一天的日期
                else
                    strNewPassword = Guid.NewGuid().ToString(); // 2017/10/29 如果前端发来的读者记录中没有 dateOfBirth 元素内容，则自动发生一个随机的字符串作为密码，这样读者就无法登录成功，只能去图书馆柜台重设密码，或者用微信公众号的找回密码功能来得到密码(假如创建读者记录时候提供了手机号码)
            }
            catch (Exception ex)
            {
                strError = "出生日期字段值 '" + strBirthDate + "' 不合法: " + ex.Message;
                return -1;
            }

            // 条件化的失效期
            TimeSpan expireLength = GetConditionalPatronPasswordExpireLength(dom);

            XmlDocument domOperLog = null;
            // 修改读者密码
            // return:
            //      -1  error
            //      0   成功
            int nRet = ChangeReaderPassword(
                dom,
                strNewPassword,
                expireLength,   // _patronPasswordExpirePeriod,
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
            {
                DomUtil.SetElementText(dom.DocumentElement, "refID", Guid.NewGuid().ToString());
                bChanged = true;
            }

            // 检查重要元素是否兑现创建
            if (important_fields != null && important_fields.Length > 0)
            {
                // 2021/8/8
                // 实际发生了内容修改的、超出权限的元素名集合
                List<string> error_names = new List<string>();
                foreach (var strElementName in important_fields)
                {
                    // 如果是 fingerprint face palmprint 元素，则只比较 InnerText
                    if (strElementName == "fingerprint"
                        || strElementName == "face"
                        || strElementName == "palmprint")
                    {
                        string old_innerText = DomUtil.GetElementText(domNewRec.DocumentElement, strElementName);
                        string new_innerText = DomUtil.GetElementText(dom.DocumentElement, strElementName);

                        if (old_innerText != new_innerText)
                            error_names.Add(strElementName);
                    }
                    else
                    {
                        string old_outerXml = GetOuterXml(domNewRec, strElementName);
                        string new_outerXml = GetOuterXml(dom, strElementName);

                        if (old_outerXml != new_outerXml)
                            error_names.Add(strElementName);
                    }
                }

                if (error_names.Count > 0)
                {
                    strError = $"创建读者记录被拒绝。下列元素超过当前用户权限: '{StringUtil.MakePathList(error_names)}'";
                    return -1;
                }
            }

            strXml = dom.OuterXml;
            if (bChanged == true)
                return 1;

            return 0;
        }

        // 根据读者记录 XML，获得条件化的读者密码失效前长度
        public TimeSpan GetConditionalPatronPasswordExpireLength(XmlDocument readerdom)
        {
            // 2021/7/8
            // 合成读者记录的最终权限
            int nRet = GetReaderRights(
                readerdom,
                out string rights,
                out string strError);
            if (nRet == -1)
                throw new Exception($"GetReaderRights() 时出错: {strError}");

            return GetConditionalPatronPasswordExpireLength(rights);
        }

        TimeSpan GetConditionalPatronPasswordExpireLength(string rights)
        {
            TimeSpan expireLength = _patronPasswordExpirePeriod;
            if (StringUtil.IsInList("neverexpire", rights))
                expireLength = TimeSpan.MaxValue;
            return expireLength;
        }

        // <DoReaderChange()的下级函数>
        // 比较两个记录, 看看和读者静态信息有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        static int IsReaderInfoChanged(
            string[] element_names,
            XmlDocument dom1,
            XmlDocument dom2)
        {
            for (int i = 0; i < element_names.Length; i++)
            {
                /*
                string strText1 = DomUtil.GetElementText(dom1.DocumentElement,
                    element_names[i]);
                string strText2 = DomUtil.GetElementText(dom2.DocumentElement,
                    element_names[i]);
                 * */
                // 2006/11/29 changed
                string strText1 = DomUtil.GetElementOuterXml(dom1.DocumentElement,
                    element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(dom2.DocumentElement,
                    element_names[i]);


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

            string[] element_names = StringUtil.Append(_reader_element_names, this.PatronAdditionalFields.ToArray());

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
                    string write_level = GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin);
                    // 有setreaderinfo和changereaderstate之一均可
                    // if (StringUtil.IsInList("setreaderinfo", sessioninfo.RightsOrigin) == false)
                    if (write_level == null)
                    {
                        if (StringUtil.IsInList("changereaderstate", sessioninfo.RightsOrigin) == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "修改读者信息(状态字段)被拒绝。不具备 changereaderstate 权限。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                    else if (string.IsNullOrEmpty(write_level) == false)
                    {
                        // 2021/7/16
                        // 对于 setreaderinfo:n 权限，要检查是否包含 state 元素
                        var names = GetElementNames(write_level);
                        if (names.Contains("state") == false)
                        {
                            result.Value = -1;
                            result.ErrorInfo = "修改读者信息(状态字段)被拒绝。当前账户针对读者记录的可修改字段中未包含 状态 字段。";
                            result.ErrorCode = ErrorCode.AccessDenied;
                            return result;
                        }
                    }
                }
                else if (strAction == "changereaderbarcode")
                {
                    // TODO: 对于 setreaderinfo:n 权限，要检查是否包含 barcode 元素

                    if (StringUtil.IsInList("changereaderbarcode", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改读者信息(证条码号字段)被拒绝。不具备 changereaderbarcode 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else if (strAction == "changeforegift")
                {
                    // TODO: 对于 setreaderinfo:n 权限，要检查是否包含 foregift 元素

                    // changereaderforegift
                    if (StringUtil.IsInList("changereaderforegift", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "changeforegift方式修改读者信息被拒绝。不具备 changereaderforegift 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else if (strAction == "delete")
                {
                    // 检查当前账户是否有完全的 setreaderinfo 权限

                    // 2021/7/15
                    // return:
                    //      null    没有找到 getreaderinfo 前缀
                    //      ""      找到了前缀，并且 level 部分为空
                    //      其他     返回 level 部分
                    string write_level = GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin);
                    if (write_level == null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "删除读者记录操作被拒绝。不具备 setreaderinfo (全部字段)权限 或 包含 r_delete 的 setreaderinfo: 权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    if (string.IsNullOrEmpty(write_level) == false
                        && StringUtil.IsInList("r_delete", write_level.Replace("|", ",")) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "删除读者记录操作被拒绝。不具备包含 r_delete 的 setreaderinfo: 权限";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }
                else
                {
                    // if (StringUtil.IsInList("setreaderinfo", sessioninfo.RightsOrigin) == false)
                    if (GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin) == null)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改读者信息被拒绝。不具备 setreaderinfo 权限。";
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

            // 2017/3/2
            if (strAction == "change"
                    || strAction == "changestate"
                    || strAction == "changeforegift"
                    || strAction == "changereaderbarcode")
            {
                if (string.IsNullOrEmpty(strRecPath))
                {
                    strError = "修改读者记录的操作 '" + strAction + "' 不允许记录路径参数 strRecPath 值为空";
                    goto ERROR1;
                }

                // 检查宜于早一点进行。如果这里不检查，则查重证条码号以后，命中了一条读者记录，但因为 strRecPath 中提交的不是合法的路径形态，那么判断的结果就是“证条码号已经在别的记录中存在了”，这种报错会误导用户或开发者
                string strTargetRecId = ResPath.GetRecordId(strRecPath);
                if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                {
                    strError = "修改读者记录的操作 '" + strAction + "' 不允许使用追加形态的记录路径 '" + strRecPath + "'";
                    goto ERROR1;
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

            // 2021/7/21
            string important_fields = domNewRec.DocumentElement?.GetAttribute("importantFields");
            if (domNewRec.DocumentElement != null)
                domNewRec.DocumentElement.RemoveAttribute("importantFields");

            // 2021/8/5
            string data_fields = domNewRec.DocumentElement?.GetAttribute("dataFields");
            if (domNewRec.DocumentElement != null)
                domNewRec.DocumentElement.RemoveAttribute("dataFields");

            // 2021/8/8
            if (strAction == "new" && string.IsNullOrEmpty(data_fields) == false)
            {
                strError = "当 action 为 'new' 时，读者记录 XML 根元素不应包含 dataFields 元素";
                goto ERROR1;
            }

            // return:
            //      -1  出错
            //      0   相等
            //      1   不相等
            nRet = CompareTwoBarcode(domOldRec,
                domNewRec,
                out string strOldBarcode,
                out string strNewBarcode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 注意: oldDom 是前端提供过来的，显然前端可能会说谎，那么这个比较新旧条码号的结果就堪忧了。改进的办法可以是这里真正从读者库取出来，然后进行比较 
            bool bBarcodeChanged = false;
            if (nRet == 1)
                bBarcodeChanged = true;

            // 对读者身份的附加判断
            if (strAction == "change" && sessioninfo.UserType == "reader")
            {
#if NO
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
#endif
                element_names = _selfchangeable_reader_element_names;
            }

            // return:
            //      -1  出错
            //      0   相等
            //      1   不相等
            nRet = CompareTwoDisplayName(domOldRec,
                domNewRec,
                out string strOldDisplayName,
                out string strNewDisplayName,
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
                if (// bBarcodeChanged == true &&
     (strAction == "new"
        /*|| strAction == "change"
        || strAction == "changestate"
        || strAction == "changeforegift"
        || strAction == "changereaderbarcode"*/)
    && String.IsNullOrEmpty(strNewBarcode) == true
    )
                {
                    // 注: change 情况已经在 DoReaderChange() 中检查了
                    if (this.AcceptBlankReaderBarcode == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError + "证条码号不能为空。保存操作失败";
                        result.ErrorCode = ErrorCode.InvalidReaderBarcode;
                        return result;
                    }
                }

                // 对读者证条码号查重，如果必要，并获得strRecPath
                if ( // bBarcodeChanged == true &&
                    (strAction == "new"
                        /*|| strAction == "change"
                        || strAction == "changestate"
                        || strAction == "changeforegift"
                        || strAction == "changereaderbarcode"*/)
                    && String.IsNullOrEmpty(strNewBarcode) == false
                    )
                {

                    // 本函数只负责查重, 并不获得记录体
                    // return:
                    //      -1  error
                    //      其他    命中记录条数(不超过nMax规定的极限)
                    nRet = app.SearchReaderRecDup(
                        // sessioninfo.Channels,
                        channel,
                        strNewBarcode,
                        100,
                        out List<string> aPath,
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
                            strError = "证条码号 '" + strNewBarcode + "' 或 显示名 '" + strNewDisplayName + "' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";
                        else
                            strError = "证条码号 '" + strNewBarcode + "' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";

                        // 2008/8/15 changed
                        result.Value = -1;
                        result.ErrorInfo = strError;
                        result.ErrorCode = ErrorCode.ReaderBarcodeDup;
                        return result;
                    }
                }

                if (strAction == "new"
|| strAction == "change"
|| strAction == "changereaderbarcode"
|| strAction == "move")
                {
                    // 2021/4/17
                    // 如果前端发来的是一个空的记录路径，则自动映射为当前用户管辖的第一个读者库名
                    if (strAction == "new"
                        && string.IsNullOrEmpty(strRecPath))
                    {
                        // 当路径整个为空的时候，自动选用第一个读者库
                        // parameters:
                        //      libraryCodeList 当前用户管辖的一个或者多个馆代码
                        nRet = GetFirstReaderDbName(
                            app,
                            sessioninfo.LibraryCodeList,
                            out string first_dbname,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = $"{strError}， 因此无法新创建读者记录";
                            goto ERROR1;
                        }
                        strRecPath = first_dbname + "/?";
                    }

                    // doverifyreaderfunction 原来在这里
                }

                // 对显示名检查和查重
                if (bDisplayNameChanged == true
                    && (strAction == "new" /*
                        || strAction == "change"
                        || strAction == "changestate"
                        || strAction == "changeforegift"
                        || strAction == "changereaderbarcode"*/)
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
                            strError = "校验显示名 '" + strNewDisplayName + "' 和证条码号(空间)潜在冲突过程中(调用函数DoVerifyBarcodeScriptFunction()时)发生错误: " + strError;
                            goto ERROR1;
                        }

                        Debug.Assert(nRet == 0, "");

                        if (nResultValue == -1)
                        {
                            strError = "校验显示名 '" + strNewDisplayName + "' 和证条码号(空间)潜在冲突过程中发生错误: " + strError;
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

                // 兑现一个命令
                if (strAction == "new")
                {

                    // 检查新记录的路径中的id部分是否正确
                    // 库名部分，前面已经统一检查过了
                    if (String.IsNullOrEmpty(strRecPath) == true)
                    {
                        // 当路径整个为空的时候，自动选用第一个读者库
                        if (String.IsNullOrEmpty(strReaderDbName) == true)
                        {
#if NO
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
#endif
                            // 当路径整个为空的时候，自动选用第一个读者库
                            // parameters:
                            //      libraryCodeList 当前用户管辖的一个或者多个馆代码
                            nRet = GetFirstReaderDbName(
                                app,
                                sessioninfo.LibraryCodeList,
                                out strReaderDbName,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = $"{strError}， 因此无法新创建读者记录";
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
                            string.IsNullOrEmpty(important_fields) ? null : important_fields.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                            sessioninfo.RightsOrigin,
                            out string xml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        domNewRec.LoadXml(xml);
                    }
                    else
                    {
                        // 2008/5/29 
                        // strSavedXml = domNewRec.OuterXml;
                    }

                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (app.IsCurrentChangeableReaderPath(strRecPath,
                        sessioninfo.LibraryCodeList,
                        out string strLibraryCode) == false)
                    {
                        strError = "读者记录路径 '" + strRecPath + "' 的读者库不在当前用户管辖范围内";
                        goto ERROR1;
                    }

                    /*
                    // 2020/3/2
                    // 给 strSaveXml 中添加 libraryCode 元素
                    {
                        XmlDocument domTemp = new XmlDocument();
                        domTemp.LoadXml(strSavedXml);

                        DomUtil.SetElementText(domTemp.DocumentElement, "libraryCode", strLibraryCode);
                        strSavedXml = domTemp.OuterXml;
                    }
                    */
                    // 2021/7/15
                    // 给 domNewRec 中添加 libraryCode 元素
                    {
                        DomUtil.SetElementText(domNewRec.DocumentElement, "libraryCode", strLibraryCode);
                    }

                    // 注：要在 strRecPath 决定后再进行此调用
                    // return:
                    //      -3  条码号错误
                    //      -2  not found script
                    //      -1  出错
                    //      0   成功
                    //      1   校验发现错误(非条码号的其它错误)
                    nRet = this.DoVerifyReaderFunction(
                        sessioninfo,
                        strAction,
                        strRecPath,
                        domNewRec,
                        bForce ? "dontVerifyBarcode" : "", // 2020/4/17 强制保存的时候不进行读者证条码号规则校验
                        out strError);
                    if (nRet != 0)
                    {
                        result.Value = -1;
                        result.ErrorInfo = strError + "。保存操作失败";
                        if (nRet == -3) // 2021/8/6
                            result.ErrorCode = ErrorCode.InvalidReaderBarcode;
                        else
                            result.ErrorCode = ErrorCode.SystemError;
                        return result;
                    }

                    // 2014/7/4
                    if (this.VerifyReaderType == true
                        && bForce == false) // 2020/5/27
                    {
                        /*
                        XmlDocument domTemp = new XmlDocument();
                        domTemp.LoadXml(strSavedXml);

                        {
                            // 检查一个册记录的读者类型是否符合值列表要求
                            // parameters:
                            // return:
                            //      -1  检查过程出错
                            //      0   符合要求
                            //      1   不符合要求
                            nRet = CheckReaderType(domTemp,
                                strLibraryCode,
                                strReaderDbName,
                                out bool changed,
                                out strError);
                            if (nRet == -1 || nRet == 1)
                            {
                                strError = strError + "。创建读者记录操作失败";
                                goto ERROR1;
                            }
                            if (changed)
                                strSavedXml = domTemp.OuterXml;
                        }
                        */
                        {
                            // 检查一个册记录的读者类型是否符合值列表要求
                            // parameters:
                            // return:
                            //      -1  检查过程出错
                            //      0   符合要求
                            //      1   不符合要求
                            nRet = CheckReaderType(domNewRec,
                                strLibraryCode,
                                strReaderDbName,
                                out bool changed,
                                out strError);
                            if (nRet == -1 || nRet == 1)
                            {
                                strError = strError + "。创建读者记录操作失败";
                                goto ERROR1;
                            }
                        }
                    }

                    // TODO: 添加 libraryCode 元素

                    lRet = channel.DoSaveTextRes(strRecPath,
                        domNewRec.OuterXml, // strSavedXml,
                        false,   // include preamble?
                        "content",
                        baOldTimestamp,
                        out byte[] output_timestamp,
                        out string strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strSavedXml = "";
                        strSavedRecPath = strOutputPath;    // 2011/9/6 add
                        baNewTimestamp = output_timestamp;
                        if (channel.OriginErrorCode == ErrorCodeValue.TimestampMismatch)
                        {
                            // 2011/9/6 add
                            strError = "创建新读者记录的时候，数据库内核决定创建新记录的位置 '" + strOutputPath + "' 居然已经存在记录。这通常是因为该数据库的尾号不正常导致的。请提醒系统管理员及时处理这个故障。原始错误信息: " + strError;
                        }
                        else
                            strError = "保存新记录的操作发生错误:" + strError;
                        kernel_errorcode = channel.OriginErrorCode;
                        goto ERROR1;
                    }
                    else // 成功
                    {
                        // *** 限定 domNewRec 中的字段
                        // 2021/7/15
                        // 按照 getreaderinfo:n 中的 level 来过滤记录，避免包含不该让这个用户看到的元素内容
                        {
                            AddPatronOI(domNewRec, strLibraryCode);

                            // 2021/7/15
                            // return:
                            //      null    没有找到 getreaderinfo 前缀
                            //      ""      找到了前缀，并且 level 部分为空
                            //      其他     返回 level 部分
                            string read_level = GetReaderInfoLevel("getreaderinfo", sessioninfo.RightsOrigin);
                            FilterByLevel(domNewRec, read_level);

                            strSavedXml = domNewRec.OuterXml;
                        }

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

                    this.SessionTable.CloseSessionByReaderBarcode(strNewBarcode);
                }
                else if (strAction == "change"
                    || strAction == "changestate"
                    || strAction == "changeforegift"
                    || strAction == "changereaderbarcode")
                {
                    // 限定 element_names 集合，根据当前账户权限中 getreaderinfo:n 和 setreaderinfo:n 中的级别 n

                    // 2021/7/15
                    // return:
                    //      null    没有找到 getreaderinfo 前缀
                    //      ""      找到了前缀，并且 level 部分为空
                    //      其他     返回 level 部分
                    string read_level = GetReaderInfoLevel("getreaderinfo", sessioninfo.RightsOrigin);
                    if (string.IsNullOrEmpty(read_level) == false)
                    {
                        var names = GetElementNames(read_level);
                        element_names = element_names.Intersect(names).ToArray();
                    }

                    string write_level = GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin);
                    if (string.IsNullOrEmpty(write_level) == false)
                    {
                        var names = GetElementNames(write_level);
                        element_names = element_names.Intersect(names).ToArray();
                    }

                    // 2021/8/5
                    // 根据 data_fields 进行元素范围限定
                    if (string.IsNullOrEmpty(data_fields) == false)
                    {
                        var names = StringUtil.SplitList(data_fields);
                        element_names = element_names.Intersect(names).ToArray();
                    }

                    // DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue errorcode;
                    // 执行"change"操作
                    // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
                    // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
                    // return:
                    //      -1  出错
                    //      0   成功
                    nRet = DoReaderChange(
                        sessioninfo,
                        sessioninfo.LibraryCodeList,
                        element_names,
                        important_fields,
                        strAction,
                        bForce,
                        channel,
                        sessioninfo.RightsOrigin,
                        strRecPath,
                        domNewRec,
                        domOldRec,
                        baOldTimestamp,
                        ref domOperLog,
                        out strExistingXml,    // strExistingRecord,
                        out strSavedXml,    // strNewRecord,
                        out baNewTimestamp,
                        out strError,
                        out ErrorCode library_errorcode,
                        out kernel_errorcode);
                    if (nRet == -1)
                    {
                        // 2021/8/5
                        if (library_errorcode != ErrorCode.SystemError)
                        {
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = library_errorcode;
                            return result;
                        }
                        // 失败
                        domOperLog = null;  // 表示不必写入日志
                        goto ERROR1;
                    }

                    this.SessionTable.CloseSessionByReaderBarcode(strNewBarcode);

                    strSavedRecPath = strRecPath;   // 保存过程不会改变记录路径
                }
                else if (strAction == "delete")
                {
                    // 2021/8/3

                    string write_level = GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin);
                    if (string.IsNullOrEmpty(write_level) == false)
                    {
                        // 微调一下，让 element_names 中增加包含 libraryCode 和 oi 这两个原本是服务器自动维护的元素
                        element_names = StringUtil.Append(_reader_element_names, new string[] { "libraryCode", "oi" });

                        var names = GetElementNames(write_level);
                        element_names = element_names.Intersect(names).ToArray();
                    }

                    // 2021/8/5
                    // 根据 data_fields 进行元素范围限定
                    if (string.IsNullOrEmpty(data_fields) == false)
                    {
                        var names = StringUtil.SplitList(data_fields);
                        element_names = element_names.Intersect(names).ToArray();
                    }

                    // return:
                    //      -2  记录中有流通信息，不能删除
                    //      -1  出错
                    //      0   记录本来就不存在
                    //      1   记录成功删除
                    nRet = DoReaderOperDelete(
                        sessioninfo.LibraryCodeList,
                        element_names,
                        important_fields,
                        sessioninfo,
                        bForce,
                        channel,
                        strRecPath,
                        // strOldXml,
                        baOldTimestamp,
                        strOldBarcode,
                        // strNewBarcode,
                        domOldRec,
                        ref strExistingXml,
                        ref baNewTimestamp,
                        ref domOperLog,
                        out ErrorCode library_errorcode,
                        ref kernel_errorcode,
                        out strError);
                    if (nRet == -1)
                    {
                        // 2021/8/5
                        if (library_errorcode != ErrorCode.SystemError)
                        {
                            result.Value = -1;
                            result.ErrorInfo = strError;
                            result.ErrorCode = library_errorcode;
                            return result;
                        }
                        goto ERROR1;
                    }
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

                    this.SessionTable.CloseSessionByReaderBarcode(strNewBarcode);
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
                string strErrorText = "SetReaderInfo() 抛出异常:" + ExceptionUtil.GetDebugText(ex);
                this.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
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

        // 当路径整个为空的时候，自动选用第一个读者库
        // parameters:
        //      libraryCodeList 当前用户管辖的一个或者多个馆代码
        static int GetFirstReaderDbName(
            LibraryApplication app,
            string libraryCodeList,
            out string strReaderDbName,
            out string strError)
        {
            strReaderDbName = "";
            strError = "";

            if (app.ReaderDbs.Count == 0)
            {
                strError = "dp2Library尚未定义读者库";
                return -1;
            }

            // 选用当前用户能管辖的第一个读者库
            // strReaderDbName = app.ReaderDbs[0].DbName;
            List<string> dbnames = app.GetCurrentReaderDbNameList(libraryCodeList);
            if (dbnames.Count > 0)
                strReaderDbName = dbnames[0];
            else
            {
                strReaderDbName = "";

                strError = "当前用户没有管辖任何读者库";
                return -1;
            }

            return 0;
        }

        // 检查一个读者记录的读者类型是否符合值列表要求
        // parameters:
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        internal int CheckReaderType(XmlDocument dom,
            string strLibraryCode,
            string strReaderDbName,
            out bool changed,
            out string strError)
        {
            changed = false;
            strError = "";
            // int nRet = 0;

            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
"readerType");

            List<string> values = null;

            var result = GetValueTable(
strLibraryCode,
"readerType",
strReaderDbName,
false);
            if (result == null || result.Length == 0)
            {
                if (strReaderType == "[default]" || strReaderType == "[blank]")
                {
                    DomUtil.SetElementText(dom.DocumentElement,
                        "readerType",
                        "");
                    changed = true;
                }
                return 0;
            }
            values = new List<string>(result);
            GetPureValue(ref values);
#if REMOVED
            // return:
            //      -1  出错
            //      0   library.xml 中尚未定义 rightsTable 元素
            //      1   成功
            nRet = GetReaderTypes(
                strLibraryCode,
                out values,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;   // 因为没有值列表，什么值都可以

            FOUND:
            GetPureValue(ref values);

#endif



            if (strReaderType == "[blank]")
            {
                strReaderType = "";
                DomUtil.SetElementText(dom.DocumentElement,
                    "readerType",
                    strReaderType);
                changed = true;
                return 0;
            }
            else
            {
                if (strReaderType == "[default]")
                {
                    strReaderType = values[0];
                    DomUtil.SetElementText(dom.DocumentElement,
                        "readerType",
                        strReaderType);
                    changed = true;
                }

                if (string.IsNullOrEmpty(strReaderType)
                    && values.IndexOf("") != -1)
                {
                    // 允许列表中出现 ""
                    return 0;
                }
                else
                {
                    if (IsInList(strReaderType, values) == true)
                        return 0;
                }
            }

            strError = "读者类型 '" + strReaderType + "' 不是合法的值。应为 '" + StringUtil.MakePathList(values) + "' 之一";
            return 1;
        }

#if REMOVED
        // return:
        //      -1  出错
        //      0   library.xml 中尚未定义 rightsTable 元素
        //      1   成功
        public int GetReaderTypes(
            string strLibraryCode,
            out List<string> reader_types,
            out string strError)
        {
            strError = "";
            reader_types = new List<string>();

            XmlElement root = this.LibraryCfgDom?.DocumentElement?.SelectSingleNode("rightsTable") as XmlElement;
            if (root == null)
            {
                strError = "library.xml 中尚未定义 rightsTable 元素";
                return 0;
            }

            reader_types = LoanParam.GetReaderTypes(
root, strLibraryCode);

            return 1;
        }

#endif

        // 对新旧读者记录(或者册记录)中包含的条码号进行比较, 看看是否发生了变化(进而就需要查重)
        // 条码号包含在<barcode>元素中
        // parameters:
        //      strOldBarcode   顺便返回旧记录中的证条码号
        //      strNewBarcode   顺便返回新记录中的证条码号
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
            string[] element_names,
            string importantFields,
            SessionInfo sessioninfo,
            bool bForce,
            RmsChannel channel,
            string strRecPath,
            // string strOldXml,
            byte[] baOldTimestamp,
            string strOldBarcode,
            // string strNewBarcode,
            XmlDocument domOldRec,
            ref string strExistingXml,
            ref byte[] baNewTimestamp,
            ref XmlDocument domOperLog,
            out ErrorCode library_errorcode,
            ref ErrorCodeValue kernel_errorcode,
            out string strError)
        {
            strError = "";
            library_errorcode = ErrorCode.SystemError;
            int nRedoCount = 0;
            int nRet = 0;
            long lRet = 0;

            // 如果记录路径为空, 则先获得记录路径
            if (String.IsNullOrEmpty(strRecPath) == true)
            {

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
                    out List<string> aPath,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    strError = "证条码号为 '" + strOldBarcode + "' 的读者记录已不存在";
                    kernel_errorcode = ErrorCodeValue.NotFound;
                    library_errorcode = ErrorCode.ReaderBarcodeNotFound;
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
                            library_errorcode = ErrorCode.ReaderBarcodeDup;
                            goto ERROR1;
                        }
                    }
                    else
                    {
                        strError = "证条码号 '" + strOldBarcode + "' 已经被下列多条读者记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'，在未指定记录路径的情况下，无法定位和删除。";
                        library_errorcode = ErrorCode.ReaderBarcodeDup;
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

            // byte[] exist_timestamp = null;
            // string strOutputPath = "";
            // string strMetaData = "";

        REDOLOAD:
            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(strRecPath,
                out strExistingXml,
                out string strMetaData,
                out byte [] exist_timestamp,
                out string strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    kernel_errorcode = channel.OriginErrorCode;
                    library_errorcode = ErrorCode.NotFound;
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
                    library_errorcode = ErrorCode.HasCirculationInfo;
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
                    library_errorcode = ErrorCode.TimestampMismatch;
                    baNewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                    goto ERROR1;
                }

                // 是否报错?
                // 功能做的精细一点，需要比较strOldXml和strExistingXml中要害字段是否被改变了，如果没有改变，是不必报错的

                // 如果前端给出了旧记录，就有和库中记录进行比较的基础
                // if (String.IsNullOrEmpty(strOldXml) == false)
                if (domOldRec != null && domOldRec.DocumentElement != null
                    && domOldRec.DocumentElement.HasChildNodes)
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
                        library_errorcode = ErrorCode.TimestampMismatch;    // TODO: 建议给一个专门的错误码表示“期间原记录已经被修改”
                        baNewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                        goto ERROR1;
                    }
                }

                baOldTimestamp = exist_timestamp;
                baNewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
            }

            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
            if (this.IsCurrentChangeableReaderPath(strRecPath,
                strCurrentLibraryCode,
                out string strLibraryCode) == false)
            {
                strError = "读者记录路径 '" + strRecPath + "' 的读者库不在当前用户管辖范围内";
                library_errorcode = ErrorCode.AccessDenied;
                goto ERROR1;
            }

            // 2021/8/3
            string write_level = GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin);
            if (string.IsNullOrEmpty(write_level) == false)
            {
                var outof_names = GetOutofElements(domExist,
                    element_names,
                    new string[] { "password",
                        "libraryCode",
                        "refID",
                        "oi",
                        "info",
                        "borrows",
                        "overdues",
                        "reservations",
                        "outofReservations" }); // 注: 把系统自己管理的一些元素从检测范围排除
                if (outof_names.Count > 0)
                {
                    strError = $"删除读者记录被拒绝。记录中下列元素超过当前用户可修改权限范围: {StringUtil.MakePathList(outof_names)}";
                    kernel_errorcode = ErrorCodeValue.AccessDenied;
                    library_errorcode = ErrorCode.AccessDenied;
                    domOperLog = null;  // 表示不必写入日志
                    return -1;
                }
            }

            // byte[] output_timestamp = null;

            Debug.Assert(strRecPath != "", "");

            lRet = channel.DoDeleteRes(strRecPath,
                baOldTimestamp,
                out byte [] output_timestamp,
                out strError);
            if (lRet == -1)
            {
                // 2009/7/17 
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strError = "证条码号为 '" + strOldBarcode + "' 的读者记录(在删除的时候发现)已不存在";
                    kernel_errorcode = ErrorCodeValue.NotFound;
                    library_errorcode = ErrorCode.NotFound;
                    return 0;
                }

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复删除均遇到时间戳冲突, 超过10次重试仍然失败";
                        baNewTimestamp = output_timestamp;
                        kernel_errorcode = channel.OriginErrorCode;
                        library_errorcode = ErrorCode.TimestampMismatch;
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
            SessionInfo sessioninfo,
            string strCurrentLibraryCode,
            string[] element_names,
            string importantFields,
            string strAction,
            bool bForce,
            RmsChannel channel,
            string rights,
            string strRecPath,
            XmlDocument domNewRec,
            XmlDocument domOldRec,
            byte[] baOldTimestamp,
            ref XmlDocument domOperLog,
            out string strExistingRecord,
            out string strNewRecord,
            out byte[] baNewTimestamp,
            out string strError,
            out ErrorCode library_errorcode,
            out ErrorCodeValue kernel_errorcode)
        {
            strError = "";
            strExistingRecord = "";
            strNewRecord = "";
            baNewTimestamp = null;
            library_errorcode = ErrorCode.SystemError;
            kernel_errorcode = ErrorCodeValue.NoError;

            int nRedoCount = 0;
            bool bExist = true;    // strRecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";

            // 2015/11/16
            string strTargetRecId = ResPath.GetRecordId(strRecPath);
            if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
            {
                strError = "修改读者记录的操作 '" + strAction + "' 不允许使用追加形态的记录路径 '" + strRecPath + "'";
                goto ERROR1;
            }

        REDOLOAD:
            bool bObjectNotFound = false;
            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(strRecPath,
                out strExistXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound
                    || channel.ErrorCode == ChannelErrorCode.NotFoundObjectFile)    // 2019/6/12
                {
                    bObjectNotFound = channel.ErrorCode == ChannelErrorCode.NotFoundObjectFile;
                    // 如果记录不存在, 则构造一条空的记录
                    bExist = false;
                    strExistXml = "<root />";
                    exist_timestamp = null;
                    strOutputPath = strRecPath;
                }
                else
                {
                    strError = "保存操作发生错误, 在读入原有记录阶段:" + strError;
                    kernel_errorcode = channel.OriginErrorCode;
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

            // 2021/8/5
            // 先合并新旧记录，得到真正保存的新记录内容状态。后面再进行新旧比较
            if (bForce == false)
            {
                nRet = MergeTwoReaderXml(
                element_names,
                strAction,
                domExist,
                domNewRec,
                importantFields == null || string.IsNullOrEmpty(importantFields) ? null : importantFields.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                // out string strNewXml,
                out XmlDocument domMerged,
                out strError);
                if (nRet == -1)
                    goto ERROR1;

                /*
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
                */
                domNewRec = domMerged;
            }


            bool bChangeReaderBarcode = false;

            string strOldBarcode = "";
            string strNewBarcode = "";

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

            // 注意: oldDom 是前端提供过来的，显然前端可能会说谎，那么这个比较新旧条码号的结果就堪忧了。改进的办法可以是这里真正从读者库取出来，然后进行比较 
            bool bBarcodeChanged = false;
            if (nRet == 1)
                bBarcodeChanged = true;

            // 对读者身份的附加判断
            if (/*strAction == "change" && */sessioninfo.UserType == "reader")
            {
                if (sessioninfo.Account.Barcode != strNewBarcode)
                {
                    strError = "修改读者信息被拒绝。作为读者不能修改其他读者的读者记录";
                    library_errorcode = ErrorCode.AccessDenied;
                    return -1;
                }

                // element_names = _selfchangeable_reader_element_names;
            }

            if (bExist == true) // 2008/5/29 
            {
                string strDetailInfo = "";  // 关于读者记录里面是否有流通信息的详细提示文字
                bool bHasCirculationInfo = false;   // 读者记录里面是否有流通信息
                bool bDetectCiculationInfo = false; // 是否已经探测过读者记录中的流通信息

                if (bBarcodeChanged)  // 读者证条码号有改变
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


                // parameters:
                //      strOldState   顺便返回旧记录中的状态字符串
                //      strNewState   顺便返回新记录中的状态字符串
                // return:
                //      -1  出错
                //      0   相等
                //      1   不相等
                nRet = CompareTwoState(domExist,
                    domNewRec,
                    out string strOldState,
                    out string strNewState,
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

            string new_barcode = DomUtil.GetElementText(domNewRec.DocumentElement, "barcode");
            // 检查空条码号
            {
                if (string.IsNullOrEmpty(new_barcode)
                    && this.AcceptBlankReaderBarcode == false)
                {
                    strError = strError + "证条码号不能为空。保存操作失败";
                    library_errorcode = ErrorCode.InvalidReaderBarcode;
                    return -1;
                }
            }

            // 对证条码号进行查重
            if (bBarcodeChanged
                && string.IsNullOrEmpty(new_barcode) == false)
            {
                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                nRet = this.SearchReaderRecDup(
                    // sessioninfo.Channels,
                    channel,
                    new_barcode,
                    100,
                    out List<string> aPath,
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
                    if (String.IsNullOrEmpty(strNewDisplayName) == false)
                        strError = "证条码号 '" + strNewBarcode + "' 或 显示名 '" + strNewDisplayName + "' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";
                    else
                    */
                    strError = "证条码号 '" + strNewBarcode + "' 已经被下列读者记录使用了: " + StringUtil.MakePathList(aPath) + "。操作失败。";

                    library_errorcode = ErrorCode.ReaderBarcodeDup;
                    return -1;
                }
            }

            // 校验读者记录
            {
                // 注：要在 strRecPath 决定后再进行此调用
                // return:
                //      -3  条码号错误
                //      -2  not found script
                //      -1  出错
                //      0   成功
                //      1   校验发现错误(非条码号的其它错误)
                nRet = this.DoVerifyReaderFunction(
                    sessioninfo,
                    strAction,
                    strRecPath,
                    domNewRec,
                    bForce ? "dontVerifyBarcode" : "", // 2020/4/17 强制保存的时候不进行读者证条码号规则校验
                    out strError);
                if (nRet != 0)
                {
                    strError = strError + "。保存操作失败";
                    if (nRet == -3) // 2021/8/6
                        library_errorcode = ErrorCode.InvalidReaderBarcode;
                    else
                        library_errorcode = ErrorCode.SystemError;
                    return -1;
                }
            }

            // return:
            //      -1  出错
            //      0   相等
            //      1   不相等
            nRet = CompareTwoDisplayName(domOldRec,
                domNewRec,
                out string strOldDisplayName,
                out string strNewDisplayName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            bool bDisplayNameChanged = false;
            if (nRet == 1)
                bDisplayNameChanged = true;
            // 对显示名检查和查重
            if (bDisplayNameChanged == true
                && (strAction == "change"
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
                        strError = "校验显示名 '" + strNewDisplayName + "' 和证条码号(空间)潜在冲突过程中(调用函数DoVerifyBarcodeScriptFunction()时)发生错误: " + strError;
                        goto ERROR1;
                    }

                    Debug.Assert(nRet == 0, "");

                    if (nResultValue == -1)
                    {
                        strError = "校验显示名 '" + strNewDisplayName + "' 和证条码号(空间)潜在冲突过程中发生错误: " + strError;
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
                nRet = SearchReaderDisplayNameDup(
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

                    library_errorcode = ErrorCode.ReaderBarcodeDup;
                    return -1;
                }

                // 对工作人员帐户名进行查重。虽然不是强制性的，但是可以避免大部分误会
                // 注：工作人员依然可以创建和读者显示名相重的帐户名
                if (SearchUserNameDup(strNewDisplayName) == true)
                {
                    strError = "显示名 '" + strNewDisplayName + "' 已经被工作人员帐户使用。操作失败。";
                    library_errorcode = ErrorCode.ReaderBarcodeDup;
                    return -1;
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
                    strError = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 在编辑期间原记录已发生过修改(保存时发现提交的时间戳和原记录不匹配)";
                    kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch;
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
                        strError = "保存操作发生错误: 数据库中的原记录 (路径为'" + strRecPath + "') 在编辑期间原记录已发生过修改(保存时发现提交的时间戳和原记录不匹配)";

                    kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.TimestampMismatch;
                    return -1;  // timestamp mismatch
                }

                // exist_timestamp此时已经反映了库中被修改后的记录的时间戳
            }

            // TODO: 当strAction==changestate时，只允许<state>和<comment>两个元素内容发生变化

            // 原来 MergeTwo 在这里

            // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
            if (this.IsCurrentChangeableReaderPath(strRecPath,
                strCurrentLibraryCode,
                out string strLibraryCode) == false)
            {
                strError = "读者记录路径 '" + strRecPath + "' 的读者库不在当前用户管辖范围内";
                goto ERROR1;
            }

            // 2020/3/2
            // 给 domNewRec 中添加 libraryCode 元素
            DomUtil.SetElementText(domNewRec.DocumentElement, "libraryCode", strLibraryCode);

            // 2014/7/4
            if (this.VerifyReaderType == true
                && strAction == "change"   // 2020/5/28 除了 change 以外的 changestate changeforegift changereaderbarcode 都不需要检查 readerType 元素
                && bForce == false) // 2020/5/28
            {
                string strReaderDbName = "";

                if (String.IsNullOrEmpty(strRecPath) == false)
                    strReaderDbName = ResPath.GetDbName(strRecPath);

#if NO
                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);
#endif
                // 2020/6/2 待审核状态的读者记录不需要检查 readerType 元素值
                string state = DomUtil.GetElementText(domNewRec.DocumentElement, "state");
                if (StringUtil.IsInList("待审核", state) == false)
                {
                    // 检查一个册记录的读者类型是否符合值列表要求
                    // parameters:
                    // return:
                    //      -1  检查过程出错
                    //      0   符合要求
                    //      1   不符合要求
                    nRet = CheckReaderType(domNewRec,   // domTemp,
                        strLibraryCode,
                        strReaderDbName,
                        out bool changed,
                        out strError);
                    if (nRet == -1 || nRet == 1)
                    {
                        strError = strError + "。修改读者记录操作失败";
                        goto ERROR1;
                    }
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
                strExistComment += DateTime.Now.ToString() + " 证条码号从 '" + strOldBarcode + "' 修改为 '" + strNewBarcode + "'";
                DomUtil.SetElementText(domNewRec.DocumentElement, "comment", strExistComment);
            }

            // 检查 password/@expire 属性
            {
                // return:
                //      -1  出错
                //      0   相等
                //      1   不相等
                nRet = CompareTwoField(
                    "rights",
                    domExist,
                    domNewRec,
                    out string strOldText,
                    out string strNewText,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    // 进行检查
                    // 根据 library.xml 中 login/@patronPasswordExpireLength 和读者记录的 rights 元素，
                    // 添加或者清除读者记录中 password/@expire 属性
                    if (ReadersMonitor.UpdatePasswordExpire(this, domNewRec) == true)
                    {
                        // 对 domNewRec 有改动
                    }
                }
            }

        REDO_SAVE:
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

                    if (bObjectNotFound)
                    {
                        exist_timestamp = output_timestamp;
                        goto REDO_SAVE;
                    }

                    goto REDOLOAD;
                }

                strError = "保存操作发生错误:" + strError;
                kernel_errorcode = channel.OriginErrorCode;
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

                // 2021/7/15
                // 按照 getreaderinfo:n 中的 level 来过滤记录，避免包含不该让这个用户看到的元素内容
                {
                    AddPatronOI(domNewRec, strLibraryCode);

                    // 2021/7/15
                    // return:
                    //      null    没有找到 getreaderinfo 前缀
                    //      ""      找到了前缀，并且 level 部分为空
                    //      其他     返回 level 部分
                    string read_level = GetReaderInfoLevel("getreaderinfo", rights);
                    FilterByLevel(domNewRec, read_level);

                    strNewRecord = domNewRec.OuterXml;
                }

                strError = "保存操作成功。NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.NoError;

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

            // 2016/9/9
            // 修改出纳历史库里面的全部证条码号
            if (bBarcodeChanged
                && this.ChargingOperDatabase != null
                && this.ChargingOperDatabase.Enabled)
                this.ChargingOperDatabase.ChangePatronBarcode(strOldBarcode, strNewBarcode);

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
            // kernel_errorcode = DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue.CommonError;
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

            foreach (XmlElement borrow in nodes)
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
                        out List<string> aPath,
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
                        strError = "册条码号 '" + strItemBarcode + "' 命中多于一条";
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
                out string strParamValue,
                out MatchResult matchresult,
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

            // return:
            //      -1  出错
            //      0   没有找到日历
            //      1   找到日历
            nRet = this.GetReaderCalendar(strReaderType,
                strLibraryCode,
                out Calendar calendar,
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
                        strSummary = "error:" + result.ErrorInfo;
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
                    // 2021/3/11
                    if (result.Value == -1)
                    {
                        strSummary = "error:" + result.ErrorInfo;
                    }
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
                            strSummary = "error:" + result.ErrorInfo;
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

        static string TEST_READER_BARCODE = "_testreader";

        static bool IsTestReaderBarcode(string strBarcode)
        {
            if (string.Compare(strBarcode, TEST_READER_BARCODE, true) == 0)
                return true;
            return false;
        }

        // 获得一个测试用的读者记录
        static string GetTestReaderXml()
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            DomUtil.SetElementText(dom.DocumentElement,
                "barcode", TEST_READER_BARCODE);
            DomUtil.SetElementText(dom.DocumentElement,
                "name", "测试姓名");

            XmlElement borrows = dom.CreateElement("borrows");
            dom.DocumentElement.AppendChild(borrows);
            for (int i = 0; i < 10; i++)
            {
                XmlElement borrow = dom.CreateElement("borrow");
                borrows.AppendChild(borrow);
                borrow.SetAttribute("barcode", "_testitem" + (i + 1).ToString().PadLeft(4, '0'));
                borrow.SetAttribute("recPath", "_测试实体库/" + (i + 1).ToString());
                borrow.SetAttribute("biblioRecPath", "_测试书目库/" + (i + 1).ToString());
            }

            return dom.DocumentElement.OuterXml;
        }

        // 检索，定位一条读者记录
        // return:
        //      -1  出错
        //      0   没有找到。strError 返回了(提示)报错信息, error_code 中有错误码
        //      1   找到
        public int SearchReaderRecord(
    SessionInfo sessioninfo,
    string strBarcode,
    out string strXml,
    out string strOutputPath,
    out byte[] baTimestamp,
    out ErrorCode error_code,
    out string strError)
        {
            strError = "";
            baTimestamp = null;
            strOutputPath = "";
            strXml = "";
            error_code = ErrorCode.NoError;

            List<string> recpaths = null;

#if NO
            // 个人书斋名
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;
#endif

            if (String.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "strBarcode 参数值不能为空";
                return -1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            string strIdcardNumber = "";

            int nRet = 0;
            long lRet = 0;

            // 前端提供临时记录
            if (strBarcode[0] == '<')
            {
                strXml = strBarcode;
                strOutputPath = "?";
                // TODO: 数据库名需要从前端发来的XML记录中获取，或者要知道当前用户的馆代码?
                goto SKIP1;
            }

            bool bOnlyBarcode = false;   // 是否仅仅在 证条码号中寻找

            bool bRecordGetted = false; // 记录释放后已经获取到

            // 命令状态
            if (strBarcode[0] == '@'
                && strBarcode.StartsWith("@refID:") == false)
            {
                // 获得册记录，通过册记录路径
                string strLeadPath = "@path:";
                string strLeadDisplayName = "@displayName:";
                string strLeadBarcode = "@barcode:";

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

                    if (this.IsReaderRecPath(strReaderRecPath) == false)
                    {
                        strError = "记录路径 '" + strReaderRecPath + "' 并不是一个读者库记录路径，因此拒绝操作。";
                        return -1;
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
                        return -1;
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
                            if (strCommand == "prev")
                                strError = "到头";
                            else if (strCommand == "next")
                                strError = "到尾";
                            else
                                strError = "没有找到";
                            error_code = ErrorCode.NotFound;
                            return 0;
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
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "没有找到";
                        error_code = ErrorCode.NotFound;
                        return 0;
                    }
                    // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                    if (this.IsCurrentChangeableReaderPath(strOutputPath,
                        sessioninfo.LibraryCodeList) == false)
                    {
                        strError = "读者记录路径 '" + strOutputPath + "' 的读者库不在当前用户管辖范围内";
                        return -1;
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
                    return -1;
                }

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
                        return -1;
                    if (nRet == 1)
                    {
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
                    // 返回测试读者记录
                    if (IsTestReaderBarcode(strBarcode))
                    {
                        nRet = 1;
                        strXml = GetTestReaderXml();
                        recpaths = new List<string>();
                        recpaths.Add("测试读者/1");
                        baTimestamp = null;
                    }
                    else
                    {
                        // 慢速版本。但能获得重复的记录路径
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   命中1条
                        //      >1  命中多于1条
                        nRet = this.GetReaderRecXml(
                            channel,
                            strBarcode,
                            100,
                            sessioninfo.LibraryCodeList,
                            out recpaths,
                            out strXml,
                            out baTimestamp,
                            out strError);
                    }
                    if (nRet > 0)
                        strOutputPath = recpaths[0];

                }
                finally
                {
                    this.ReaderLocks.UnlockForRead(strBarcode);
#if DEBUG_LOCK_READER
                    this.WriteErrorLog("GetReaderInfo 结束为读者加读锁 '" + strBarcode + "'");
#endif
                }


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
                            return -1;
                        }

                        if (nRet == 0)
                        {
                            error_code = ErrorCode.IdcardNumberNotFound;

                            // text-level: 用户提示
                            strError = string.Format(this.GetString("身份证号s不存在"),   // "身份证号 '{0}' 不存在"
                                strIdcardNumber);
                            return 0;
                        }

                        if (nRet > 0)
                            strOutputPath = recpaths[0];

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
                                return -1;
                            }


                            if (nRet == 0)
                                continue;

                            if (nRet > 0)
                                strOutputPath = recpaths[0];

                            goto SKIP0;
                        }
                    }

                NOT_FOUND:
                    strError = "没有找到";
                    error_code = ErrorCode.NotFound;
                    return 0;
                }

                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (IsTestReaderBarcode(strBarcode) == false
                    && this.IsCurrentChangeableReaderPath(strOutputPath,
                    sessioninfo.LibraryCodeList) == false)
                {
                    strError = "读者记录路径 '" + strOutputPath + "' 的读者库不在当前用户管辖范围内";
                    return -1;
                }

                if (nRet == -1)
                    return -1;

                return nRet;
            }

        SKIP0:
            // 2013/5/21
            if (recpaths != null)
                strOutputPath = StringUtil.MakePathList(recpaths);

            SKIP1:


            XmlDocument readerdom = null;
            if (sessioninfo.UserType == "reader")
            {
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入 XML DOM 时发生错误: " + strError;
                    return -1;
                }

            }

            string strLibraryCode = "";
            if (strOutputPath == "?" || IsTestReaderBarcode(strBarcode))
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
                nRet = this.GetLibraryCode(strOutputPath,
                    out strLibraryCode,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            {
                if (readerdom != null)
                {
                    DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                    DomUtil.SetElementText(readerdom.DocumentElement, "libraryCode", strLibraryCode);
                }
                if (string.IsNullOrEmpty(strXml) == false)
                {
                    XmlDocument temp = new XmlDocument();
                    try
                    {
                        temp.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "读者记录 XML 装入 DOM 时出错:" + ex.Message;
                        return -1;
                    }

                    DomUtil.DeleteElement(temp.DocumentElement, "password");
                    DomUtil.SetElementText(temp.DocumentElement, "libraryCode", strLibraryCode);
                    strXml = temp.DocumentElement.OuterXml;
                }
            }

            return 1;
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
            // if (StringUtil.IsInList("getreaderinfo", sessioninfo.RightsOrigin) == false)
            if (GetReaderInfoLevel("getreaderinfo", sessioninfo.RightsOrigin) == null)
            {
                result.Value = -1;
                result.ErrorInfo = "读取读者信息被拒绝。不具备 getreaderinfo 权限。";
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
            string strOwnerInstitution = null;

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
            if (strBarcode[0] == '@'
                && strBarcode.StartsWith("@refID:") == false)
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

                ParseOI(strBarcode, out string strPureBarcode, out strOwnerInstitution);
                strBarcode = strPureBarcode;

                // 加读锁
                // 可以避免拿到读者记录处理中途的临时状态
#if DEBUG_LOCK_READER
                this.WriteErrorLog("GetReaderInfo 开始为读者加读锁 '" + strBarcode + "'");
#endif
                this.ReaderLocks.LockForRead(strBarcode);

                try
                {
                    // 返回测试读者记录
                    if (IsTestReaderBarcode(strBarcode))
                    {
                        nRet = 1;
                        strXml = GetTestReaderXml();
                        recpaths = new List<string>();
                        recpaths.Add("测试读者/1");
                        baTimestamp = null;
                    }
                    else
                    {
                        // 慢速版本。但能获得重复的记录路径
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
                    if (nRet > 0)
                        strOutputPath = recpaths[0];

#if NO
                    // 快速版本。无法获得全部重复的路径
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.TryGetReaderRecXml(
                        channel,
                        strBarcode,
                        sessioninfo.LibraryCodeList,
                        out strXml,
                        out strOutputPath,
                        out baTimestamp,
                        out strError);
#endif
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

                // 2021/3/20 移动到这里
                // 观察一个读者记录路径，看看是不是在当前用户管辖的读者库范围内?
                if (IsTestReaderBarcode(strBarcode) == false
                    && this.IsCurrentChangeableReaderPath(strOutputPath,
                    sessioninfo.LibraryCodeList) == false)
                {
                    strError = "读者记录路径 '" + strOutputPath + "' 的读者库不在当前用户管辖范围内";
                    goto ERROR1;
                }

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

            // 2021/7/13
            // 无论怎样都要装入 readerdom
            nRet = LibraryApplication.LoadToDom(strXml,
                out readerdom,
                out strError);
            if (nRet == -1)
            {
                strError = "装载读者记录进入 XML DOM 时发生错误: " + strError;
                goto ERROR1;
            }

            if (sessioninfo.UserType == "reader")
            {
                /*
                // TODO: 无论怎样都要装入 readerdom
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入 XML DOM 时发生错误: " + strError;
                    goto ERROR1;
                }
                */

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
            if (strRecPath == "?" || IsTestReaderBarcode(strBarcode))
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

            // 2020/7/17
            if (strOwnerInstitution != null)
            {
                // return:
                //      -1  出错
                //      0   没有通过较验
                //      1   通过了较验
                nRet = VerifyPatronOI(
                    strRecPath,
                    strLibraryCode,
                    strOwnerInstitution,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    result.Value = -1;
                    result.ErrorInfo = strError;
                    result.ErrorCode = ErrorCode.ReaderBarcodeNotFound;
                    return result;
                }
            }

            {
                if (readerdom != null)
                {
                    DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                    DomUtil.SetElementText(readerdom.DocumentElement, "libraryCode", strLibraryCode);
                    // 2020/9/8
                    AddPatronOI(readerdom, strLibraryCode);

                    strXml = null;  // 从此以后不用 strXml，而用 readerdom
                }

                /*
                if (string.IsNullOrEmpty(strXml) == false)
                {
                    XmlDocument temp = new XmlDocument();
                    try
                    {
                        temp.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "读者记录 XML 装入 DOM 时出错:" + ex.Message;
                        goto ERROR1;
                    }

                    DomUtil.DeleteElement(temp.DocumentElement, "password");
                    DomUtil.SetElementText(temp.DocumentElement, "libraryCode", strLibraryCode);
                    // 2020/9/8
                    AddPatronOI(temp, strLibraryCode);
                    strXml = temp.DocumentElement.OuterXml;
                }
                */
            }

            nRet = BuildReaderResults(
                sessioninfo,
                readerdom,
                null,   // strXml,
                GetReaderInfoLevel("getreaderinfo", sessioninfo.Rights),
                strResultTypeList,
                strLibraryCode,
                recpaths,
                strOutputPath,
                baTimestamp,
                OperType.None,
                null,
                "",
                ref results,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#if NO
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
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strSummary = "读者 XML 装入 DOM 出错: " + ex.Message;
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
#endif

            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 在字符串数组中指定下标位置设置一个元素值
        static void SetResult(List<string> results_list,
            int index,
            string strValue)
        {
            while (results_list.Count <= index)
            {
                results_list.Add("");
            }

            results_list[index] = strValue;
        }


        // 创建读者记录返回格式
        // 注：出于安全需要，readerdom 和 strXml 在调用前就应该把里面的 barcode 元素删除
        // parameters:
        //      readerdom   读者 XmlDocument 对象。如果为空，则 strXml 参数中应该有读者记录
        //      strXml      读者 XML 记录。如果 readerdom 为空，可以用这里的值
        int BuildReaderResults(
            SessionInfo sessioninfo,
            XmlDocument readerdom,
            string strXml,
            string level,
            string strResultTypeList,
            string strLibraryCode,  // calendar/advancexml/html 时需要
            List<string> recpaths,    // recpaths 时需要
            string strOutputPath,   // recpaths 时需要
            byte[] baTimestamp,    // timestamp 时需要
            OperType operType,  // html 时需要
            string[] saBorrowedItemBarcode,
            string strCurrentItemBarcode,
            ref string[] results_param,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strResultTypeList) == true)
            {
                results_param = new string[0];
                return 0;
            }

            // 2021/7/13
            if (readerdom == null && string.IsNullOrEmpty(strXml))
            {
                strError = "readerdom 和 strXml 不应同时为空";
                return -1;
            }

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

            XmlDocument filtered_readerdom = new XmlDocument();
            if (readerdom != null)
                filtered_readerdom.LoadXml(readerdom.OuterXml);
            else
                filtered_readerdom.LoadXml(strXml);
            FilterByLevel(filtered_readerdom, level);

            string[] result_types = strResultTypeList.Split(new char[] { ',' });
            //results = new string[result_types.Length];
            List<string> results_list = new List<string>();

            int i = 0;
            foreach (string strResultType in result_types)
            {
                // string strResultType = result_types[i];

                // 2008/4/3 
                // if (String.Compare(strResultType, "calendar", true) == 0)
                if (IsResultType(strResultType, "calendar") == true)
                {
                    /*
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
                    */

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

                    // results[i] = strCalendarName;
                    SetResult(results_list, i, strCalendarName);
                }
                // else if (String.Compare(strResultType, "xml", true) == 0)
                else if (IsResultType(strResultType, "xml") == true
                    || IsResultType(strResultType, "json") == true)
                {
                    string strResultXml = "";
                    nRet = GetPatronXml(filtered_readerdom.OuterXml, // strXml,
        strResultType,
        strLibraryCode,
        out strResultXml,
        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取 " + strResultType + " 格式的 XML 字符串时出错: " + strError;
                        goto ERROR1;
                    }

                    if (IsResultType(strResultType, "json") == true)
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strResultXml);
                        strResultXml = JsonConvert.SerializeXmlNode(dom, Newtonsoft.Json.Formatting.Indented, true);
                    }
                    SetResult(results_list, i, strResultXml);
                }
                else if (String.Compare(strResultType, "timestamp", true) == 0)
                {
                    // 2011/1/27
                    // results[i] = ByteArray.GetHexTimeStampString(baTimestamp);
                    SetResult(results_list, i, ByteArray.GetHexTimeStampString(baTimestamp));
                }
                else if (String.Compare(strResultType, "recpaths", true) == 0)
                {
                    // 2013/5/21
                    if (recpaths != null)
                    {
                        // results[i] = StringUtil.MakePathList(recpaths);
                        SetResult(results_list, i, StringUtil.MakePathList(recpaths));
                    }
                    else
                    {
                        // results[i] = strOutputPath;
                        SetResult(results_list, i, strOutputPath);
                    }
                }
                else if (String.Compare(strResultType, "oi", true) == 0)
                {
                    // oi 第一字符如果是 ! 表示这是出错信息
                    var oi = GetPatronOI(strLibraryCode);
                    SetResult(results_list, i, oi);
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
#if NO
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strSummary = "读者 XML 装入 DOM 出错: " + ex.Message;
                        results[i] = strSummary;
                        continue;
                    }
#endif
                    /*
                    if (readerdom == null)
                    {
                        nRet = LibraryApplication.LoadToDom(strXml,
                            out readerdom,
                            out strError);
                        if (nRet == -1)
                        {
                            strSummary = "读者 XML 装入 DOM 出错: " + strError;
                            // results[i] = strSummary;
                            SetResult(results_list, i, strSummary);
                            goto CONTINUE;
                        }
                    }
                    */

                    strSummary = DomUtil.GetElementText(filtered_readerdom.DocumentElement, "name");
                    // results[i] = strSummary;
                    SetResult(results_list, i, strSummary);
                }
                // else if (String.Compare(strResultType, "advancexml", true) == 0)
                else if (IsResultType(strResultType, "advancexml") == true
                    || IsResultType(strResultType, "advancejson") == true)
                {
                    // 2008/4/3 
                    string strOutputXml = "";
                    nRet = this.GetAdvanceReaderXml(
                        sessioninfo,
                        strResultTypeList,  // strResultType, BUG!!! 2012/4/8
                        strLibraryCode,
                        filtered_readerdom.OuterXml, // strXml,
                        out strOutputXml,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "GetAdvanceReaderXml()出错: " + strError;
                        goto ERROR1;
                    }

                    if (IsResultType(strResultType, "advancejson") == true)
                    {
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strOutputXml);
                        strOutputXml = JsonConvert.SerializeXmlNode(dom, Newtonsoft.Json.Formatting.Indented, true);
                    }

                    SetResult(results_list, i, strOutputXml);
                }
                // else if (String.Compare(strResultType, "html", true) == 0)
                else if (IsResultType(strResultType, "html") == true)
                {
                    string strReaderRecord = "";
                    // 将读者记录数据从XML格式转换为HTML格式
                    nRet = this.ConvertReaderXmlToHtml(
                        sessioninfo,
                        Path.Combine(this.CfgDir, "readerxml2html.cs"),
                        Path.Combine(this.CfgDir, "readerxml2html.cs.ref"),
                        strLibraryCode,
                        filtered_readerdom.OuterXml, // strXml,
                        strOutputPath,  // 2009/10/18 
                        operType,   // OperType.None,
                        saBorrowedItemBarcode,  // null,
                        strCurrentItemBarcode,
                        strResultType,
                        out strReaderRecord,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "ConvertReaderXmlToHtml()出错(脚本程序为" + this.CfgDir + "\\readerxml2html.cs" + "): " + strError;
                        goto ERROR1;
                    }
                    // test strReaderRecord = "<html><body><p>test</p></body></html>";
                    // results[i] = strReaderRecord;
                    SetResult(results_list, i, strReaderRecord);
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
                        filtered_readerdom.OuterXml, // strXml,
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
                    // results[i] = strReaderRecord;
                    SetResult(results_list, i, strReaderRecord);
                }
                else if (IsResultType(strResultType, "structure") == true)
                {
                    // 2021/7/15
                    // structure 是数据结构定义
                    /*
                     * <structure 
                     * visibleFields="name,namePinyin,state,?comment"
                     * writeableFields="name,namePinyin" />
                     * 
                     * */
                    XmlDocument struct_dom = new XmlDocument();
                    struct_dom.LoadXml("<structure />");
                    {
                        // 2021/7/29
                        List<string> base_names = null;

                        // 2021/7/15
                        // return:
                        //      null    没有找到 getreaderinfo 前缀
                        //      ""      找到了前缀，并且 level 部分为空
                        //      其他     返回 level 部分
                        string read_level = GetReaderInfoLevel("getreaderinfo", sessioninfo.RightsOrigin);
                        if (read_level == null)
                            struct_dom.DocumentElement.SetAttribute("visibleFields", "[none]");
                        else if (string.IsNullOrEmpty(read_level) == false)
                        {
                            var names = GetElementNames(read_level);
                            if (base_names != null)
                                names = new List<string>(names.Intersect(base_names));
                            struct_dom.DocumentElement.SetAttribute("visibleFields", StringUtil.MakePathList(names));
                        }
                        else
                        {
                            if (base_names != null)
                                struct_dom.DocumentElement.SetAttribute("visibleFields", StringUtil.MakePathList(base_names));
                            else
                                struct_dom.DocumentElement.SetAttribute("visibleFields", "[all]");
                        }
                    }

                    {
                        // 2021/7/29
                        List<string> base_names = null;
                        if (sessioninfo.UserType == "reader")
                        {
                            base_names = new List<string>(_selfchangeable_reader_element_names);
                        }

                        string write_level = GetReaderInfoLevel("setreaderinfo", sessioninfo.RightsOrigin);
                        if (write_level == null)
                            struct_dom.DocumentElement.SetAttribute("writeableFields", "[none]");
                        else if (string.IsNullOrEmpty(write_level) == false)
                        {
                            var names = GetElementNames(write_level);
                            if (base_names != null)
                                names = new List<string>(names.Intersect(base_names));
                            struct_dom.DocumentElement.SetAttribute("writeableFields", StringUtil.MakePathList(names));
                        }
                        else
                        {
                            if (base_names != null)
                                struct_dom.DocumentElement.SetAttribute("writeableFields", StringUtil.MakePathList(base_names));
                            else
                                struct_dom.DocumentElement.SetAttribute("writeableFields", "[all]");
                        }
                    }

                    SetResult(results_list, i, struct_dom.DocumentElement.OuterXml);
                }
                else
                {
                    strError = "未知的结果类型 '" + strResultType + "'";
                    goto ERROR1;
                }

            CONTINUE:
                i++;
            }

            results_param = results_list.ToArray();
            return 0;
        ERROR1:
            return -1;
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
            out byte[] target_timestamp)
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
                result.ErrorInfo = "移动读者记录的操作被拒绝。不具备 movereaderinfo 权限。";
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
                strError = "strSourceRecPath参数所给出的源记录路径 '" + strSourceRecPath + "' 并不是一个读者库记录路径";
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

                // TODO: 如果新位置的 libraryCode 变了，还要专门改写一次新位置的 XML 记录

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
                string strErrorText = "MoveReaderInfo() 抛出异常:" + ExceptionUtil.GetDebugText(ex);
                this.WriteErrorLog(strErrorText);

                result.Value = -1;
                result.ErrorCode = ErrorCode.SystemError;
                result.ErrorInfo = strErrorText;
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

        // 为读者记录绑定新的号码，或者解除绑定
        // parameters:
        //      strAction   动作。有 bind/unbind
        //      strQueryWord    用于定位读者记录的检索词。
        //          0) 如果以"RI:"开头，表示利用 参考ID 进行检索
        //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
        //          2) 如果以"EM:"开头，表示利用email地址进行检索。注意 email 本身应该是 email:xxxx 这样的形态。也就是说，整个加起来是 EM:email:xxxxx
        //          3) 如果以"TP:"开头，表示利用电话号码进行检索
        //          4) 如果以"ID:"开头，表示利用身份证号进行检索
        //          5) 如果以"CN:"开头，表示利用证件号码进行检索
        //          6) 如果以"UN:"开头，表示利用用户名进行检索，意思就是工作人员的账户名了，不是针对读者绑定
        //          7) 否则用证条码号进行检索
        //      strPassword     读者记录的密码
        //      strBindingID    要绑定的号码。格式如 email:xxxx 或 weixinid:xxxxx
        //      strStyle    风格。multiple/single/singlestrict。默认 single
        //                  multiple 表示允许多次绑定同一类型号码；single 表示同一类型号码只能绑定一次，如果多次绑定以前的同类型号码会被清除; singlestrict 表示如果以前存在同类型号码，本次绑定会是失败
        //                  如果包含 null_password，表示不用读者密码，strPassword 参数无效。但这个功能只能被工作人员使用
        //      strResultTypeList   结果类型数组 xml/html/text/calendar/advancexml/recpaths/summary
        //              其中calendar表示获得读者所关联的日历名；advancexml表示经过运算了的提供了丰富附加信息的xml，例如具有超期和停借期附加信息
        //              advancexml_borrow_bibliosummary/advancexml_overdue_bibliosummary/advancexml_history_bibliosummary
        //      results 返回操作成功后的读者记录
        // return:
        //      -2  权限不够，操作被拒绝
        //      -1  出错
        //      0   因为条件不具备功能没有成功执行
        //      1   功能成功执行
        public int BindPatron(
            SessionInfo sessioninfo,
            string strAction,
            string strQueryWord,
            string strPassword,
            string strBindingID,
            string strStyle,
            string strResultTypeList,
            out string[] results,
            out string strError)
        {
            strError = "";
            results = null;
            int nRet = 0;

            if (sessioninfo.UserType == "reader")
            {

            }
            else
            {
                // 工作人员如果权限足够，可以不输入读者密码。这一般用在解除绑定的时候，假如调用者能确认读者(已绑定号码的)身份。而绑定时候建议不要这样用，因为读者密码还没有验证过，其身份不明。除非工作人员经过验证证件确信就是这个读者。
                // 但，一旦没有读者密码，在登录名命中多个账户的时候，就失去了一重选择的机会
                if (StringUtil.IsInList("null_password", strStyle))
                    strPassword = null;
            }

            // 检查 strBindID 参数
            {
                if (strBindingID.IndexOf(",") != -1)
                {
                    strError = "strBindID 参数值 '" + strBindingID + "' 不合法。不允许包含逗号";
                    return -1;
                }
                string strLeft = "";
                string strRight = "";
                StringUtil.ParseTwoPart(strBindingID, ":", out strLeft, out strRight);
                if (string.IsNullOrEmpty(strLeft)
                    || string.IsNullOrEmpty(strRight))
                {
                    strError = "strBindID 参数值 '" + strBindingID + "' 不合法。应为 xxxx:xxxx 形态";
                    return -1;
                }

                // 2016/11/17
                strLeft = strLeft.ToLower();
                if (strLeft == "ip" || strLeft == "router_ip" || strLeft == "sms")
                {
                    strError = "strBindID 参数值 '" + strBindingID + "' 不合法。冒号左边的名称部分不能使用 '" + strLeft + "'，因为这是系统保留的绑定方式";
                    return -1;
                }
            }

            // 绑定工作人员账户
            if (StringUtil.HasHead(strQueryWord, "UN:") == true)
            {
                if (sessioninfo.UserType == "reader")
                {
                    strError = "读者身份不允许执行绑定工作人员账户的操作";
                    return -1;
                }

                string strUserName = strQueryWord.Substring("UN:".Length);

                // if (strUserName == "public" || strUserName == "reader" || strUserName == "opac" || strUserName == "图书馆")
                if (LibraryServerUtil.IsSpecialUserName(strUserName))
                {
                    strError = "不允许绑定到系统保留的代理账户";
                    return -1;
                }

                UserInfo userinfo = null;
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = this.GetUserInfo(strUserName, out userinfo, out strError);
                if (nRet == -1 || nRet == 0)
                    return -1;

                // 验证密码
                {
                    Account account = null;
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = GetAccount(strUserName,
            out account,
            out strError);
                    if (nRet == -1 || nRet == 0)
                        return -1;

                    if (strPassword != null)
                    {
                        // 2021/7/3
                        // 检查密码失效期
                        if (this._passwordExpirePeriod != TimeSpan.MaxValue)
                        {
                            if (DateTime.Now > account.PasswordExpire)
                            {
                                strError = "密码已经失效";
                                return -1;
                            }
                        }

                        nRet = LibraryServerUtil.MatchUserPassword(strPassword, account.Password, out strError);
                        if (nRet == -1)
                        {
                            strError = "匹配过程出现错误";
                            return -1;
                        }
                        if (nRet == 0)
                        {
                            strError = this.GetString("帐户不存在或密码不正确") + " reader 1";
                            return -1;
                        }
                    }
                }

                bool bChanged = false;

                // bool bMultiple = StringUtil.IsInList("multiple", strStyle); // 若 multiple 和 single 都包含了，则 multiple 有压倒优势

                // 修改读者记录的 email 字段
                string strEmail = userinfo.Binding;
                string strNewEmail = "";
                if (strAction == "bind")
                {
                    /*
                    strNewEmail = AddBindingString(strEmail,
                        strBindingID,
                        bMultiple);
                    */
                    nRet = AddBindingString(strEmail,
                        strBindingID,
                        strStyle,
                        out strNewEmail,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = $"绑定失败: {strError}";
                        return -1;
                    }
                }
                else if (strAction == "unbind")
                {
                    strNewEmail = RemoveBindingString(strEmail, strBindingID);
                }
                else
                {
                    strError = "未知的 strAction 参数值 '" + strAction + "'";
                    return -1;
                }

                if (strNewEmail != strEmail)
                {
                    userinfo.Binding = strNewEmail;
                    bChanged = true;
                }

                if (bChanged == true)
                {
                    nRet = this.ChangeUser(
                        sessioninfo.LibraryCodeList,
                        strUserName,
                        sessioninfo.UserID,
                        userinfo,
                        sessioninfo.ClientAddress,
                        out ErrorCode _,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<account />");
                SetUserXml(userinfo, dom.DocumentElement);

                results = new string[1];
                results[0] = dom.DocumentElement.OuterXml;

                if (bChanged)
                    return 1;

                return 0;
            }

            // 2016/9/27 允许使用 PQR
            {
                string strOutputCode = "";
                // 把二维码字符串转换为读者证条码号
                // parameters:
                //      strReaderBcode  [out]读者证条码号
                // return:
                //      -1      出错
                //      0       所给出的字符串不是读者证号二维码
                //      1       成功      
                nRet = this.DecodeQrCode(strQueryWord,
                    out strOutputCode,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                {
                    // 对 strPassword 有要求
                    // 要求提供一个 hash 字符串，增加一下使用这个方法的难度
                    string strSHA1 = Cryptography.GetSHA1(strQueryWord);
                    if (strPassword != strSHA1)
                    {
                        strError = "PQR 模式下，strPassword 参数不正确";
                        return -1;
                    }

                    strPassword = null; // 后面不再检验读者密码

                    strQueryWord = strOutputCode;
                }
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                return -1;
            }

            int nRedoCount = 0;
        REDO:
            {
                bool bTempPassword = false;
                string strXml = "";
                string strOutputPath = "";
                byte[] timestamp = null;
                string strToken = "";

                // 获得读者记录, 并检查密码是否符合。为登录用途
                // 该函数的特殊性在于，它可以用多种检索入口，而不仅仅是条码号
                // parameters:
                //      strQueryWord 登录名
                //          0) 如果以"RI:"开头，表示利用 参考ID 进行检索
                //          1) 如果以"NB:"开头，表示利用姓名生日进行检索。姓名和生日之间间隔以'|'。姓名必须完整，生日为8字符形式
                //          2) 如果以"EM:"开头，表示利用email地址进行检索。注意 email 本身应该是 email:xxxx 这样的形态。也就是说，整个加起来是 EM:email:xxxxx
                //          3) 如果以"TP:"开头，表示利用电话号码进行检索
                //          4) 如果以"ID:"开头，表示利用身份证号进行检索
                //          5) 如果以"CN:"开头，表示利用证件号码进行检索
                //          6) 否则用证条码号进行检索
                //      strPassword 密码。如果为null，表示不进行密码判断。注意，不是""
                // return:
                //      -2  当前没有配置任何读者库，或者可以操作的读者库
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                nRet = this.GetReaderRecXmlForLogin(
                    channel,
                    "", // strLibraryCodeList,
                    strQueryWord,
                    strPassword,
                    -1,  // nIndex,
                    sessioninfo.ClientIP,
                    null, // strGetToken,
                    out bool passwordExpired,
                    out bTempPassword,
                    out strXml,
                    out strOutputPath,
                    out timestamp,
                    out strToken,
                    out strError);
                if (nRet == -1 || nRet == -2)
                {
                    strError = "以 '" + strQueryWord + "' 检索读者帐户记录出错: " + strError;
                    return -1;
                }

                if (nRet > 1)
                {
                    strError = "以 '" + strQueryWord + "' 检索读者记录时, 因所匹配的帐户多于一个，无法确认读者记录。可改用证条码号进行绑定操作。";
                    return -1;
                }

                if (passwordExpired)
                {
                    strError = "帐户 '" + strQueryWord + "' 密码已经失效";
                    return -1;
                }

                if (nRet == 0)
                {
                    strError = "帐户 '" + strQueryWord + "' 不存在或密码不正确";
                    return -1;
                }

                XmlDocument readerdom = null;
                nRet = LibraryApplication.LoadToDom(strXml,
                    out readerdom,
                    out strError);
                if (nRet == -1)
                {
                    strError = "装载读者记录进入XML DOM时发生错误: " + strError;
                    return -1;
                }

                // 对读者身份的附加判断
                if (sessioninfo.UserType == "reader")
                {
                    string strBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
                    if (sessioninfo.Account.Barcode != strBarcode)
                    {
                        strError = "绑定号码的操作被拒绝。作为读者不能对其他读者的记录进行操作";
                        return -2;
                    }
                }

                bool bChanged = false;

                // bool bMultiple = StringUtil.IsInList("multiple", strStyle); // 若 mutilple 和 single 都包含了，则 multiple 有压倒优势

                // 修改读者记录的 email 字段
                string strEmail = DomUtil.GetElementText(readerdom.DocumentElement, "email");
                string strNewEmail = "";
                if (strAction == "bind")
                {
                    /*
                    strNewEmail = AddBindingString(strEmail,
                        strBindingID,
                        bMultiple);
                    */
                    nRet = AddBindingString(strEmail,
    strBindingID,
    strStyle,
    out strNewEmail,
    out strError);
                    if (nRet == -1)
                    {
                        strError = $"绑定失败: {strError}";
                        return -1;
                    }
                }
                else if (strAction == "unbind")
                {
                    strNewEmail = RemoveBindingString(strEmail, strBindingID);
                }
                else
                {
                    strError = "未知的 strAction 参数值 '" + strAction + "'";
                    return -1;
                }

                // 如果记录中没有 refID 字段，则主动填充
                string strRefID = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
                if (string.IsNullOrEmpty(strRefID) == true)
                {
                    DomUtil.SetElementText(readerdom.DocumentElement, "refID", Guid.NewGuid().ToString());
                    bChanged = true;
                }

                if (strNewEmail != strEmail)
                {
                    DomUtil.SetElementText(readerdom.DocumentElement, "email", strNewEmail);
                    bChanged = true;
                }

                // 把临时密码设置为正式密码
                if (bTempPassword == true
                    && strPassword != null)
                {
                    byte[] output_timestamp = null;
                    // 修改读者密码
                    nRet = ChangeReaderPassword(
                        sessioninfo,
                        strOutputPath,
                        ref readerdom,
                        strPassword,    // TODO: 如果 strPassword == null 会怎么样？
                        _tempPasswordExpirePeriod,
                        false,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    timestamp = output_timestamp;

                    bChanged = true;
                }

                if (bChanged == true)
                {
                    string strRecPath = strOutputPath;
                    byte[] output_timestamp = null;
                    // 保存读者记录
                    long lRet = channel.DoSaveTextRes(strRecPath,
                        readerdom.OuterXml,
                        false,
                        "content", // "content,ignorechecktimestamp",
                        timestamp,   // timestamp,
                        out output_timestamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch
                            && nRedoCount < 10)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        return -1;
                    }

                    timestamp = output_timestamp;

                    // 让缓存失效
                    string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
                    this.ClearLoginCache(strReaderBarcode);   // 及时失效登录缓存
                }

                List<string> recpaths = new List<string>();
                recpaths.Add(strOutputPath);

                // 构造读者记录的返回格式
                DomUtil.DeleteElement(readerdom.DocumentElement, "password");

                // 2016/9/4
                {
                    string strLibraryCode = "";
                    nRet = this.GetLibraryCode(strOutputPath,
            out strLibraryCode,
            out strError);
                    if (nRet == -1)
                        return -1;
                    DomUtil.SetElementText(readerdom.DocumentElement, "libraryCode", strLibraryCode);
                }

                // 2016/12/2
                // 在 readerdom 中去掉一些不必要的元素，以缩减记录尺寸
                DomUtil.DeleteElement(readerdom.DocumentElement, "borrowHistory");
                DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                DomUtil.DeleteElement(readerdom.DocumentElement, "fingerprint");
                DomUtil.DeleteElement(readerdom.DocumentElement, "palmprint");
                DomUtil.DeleteElement(readerdom.DocumentElement, "face");
                DomUtil.DeleteElement(readerdom.DocumentElement, "borrows");

                nRet = BuildReaderResults(
        sessioninfo,
        readerdom,
        readerdom.OuterXml,
        GetReaderInfoLevel("getreaderinfo", sessioninfo.Rights),
        strResultTypeList,
        "", // strLibraryCode,
        recpaths,
        strOutputPath,
        timestamp,
        OperType.None,
        null,
        "",
        ref results,
        out strError);
                if (nRet == -1)
                    return -1;

                if (bChanged)
                    return 1;
                return 0;
            }
        }

        //      expireLength    理想的密码失效前长度。在本函数中还要根据 rights 中的 neverexpire 进行调整
        // 合成读者记录的最终权限
        int GetReaderRights(
            XmlDocument readerdom,
            out string rights,
            out string strError)
        {
            rights = "";
            strError = "";

            // 获得一个参考帐户
            // 从library.xml文件定义 获得一个帐户的信息
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = GetAccount("reader",
                out Account accountref,
                out strError);
            if (nRet == -1)
            {
                // text-level: 用户提示
                strError = string.Format(this.GetString("获得reader参考帐户时出错s"),    // "获得reader参考帐户时出错: {0}"
                    strError);
                // "获得reader参考帐户时出错: " + strError;
                return -1;
            }

            // 追加读者记录中定义的权限值
            rights = DomUtil.GetElementText(readerdom.DocumentElement, "rights");
            if (string.IsNullOrEmpty(rights) == false)
                rights = MergeRights(accountref.Rights, rights);
            else
                rights = accountref.Rights;

            /*
            {
                // 如果读者记录状态有值，则需要从 account.Rights 中删除 patron 权限值
                // 反之则增加 patron 值
                string strState = DomUtil.GetElementText(readerdom.DocumentElement, "state");
                string strTemp = accountref.Rights;
                StringUtil.SetInList(ref strTemp, "patron", string.IsNullOrEmpty(strState));
                accountref.Rights = strTemp;
            }
            */
            return 1;
        }

        // 加入一个绑定号码
        // parameters:
        //      style   single/multiple/singlestrict
        public static int AddBindingString(string strText,
            string strBinding,
            string style,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = strBinding;

            if (string.IsNullOrEmpty(strText) == true)
            {
                strResult = strBinding;
                return 1;
            }

            //                 bool bMultiple = StringUtil.IsInList("multiple", strStyle); // 若 multiple 和 single 都包含了，则 multiple 有压倒优势

            bool single = StringUtil.IsInList("single", style) == true;
            bool strict = StringUtil.IsInList("singlestrict", style) == true;
            bool multiple = StringUtil.IsInList("multiple", style) == true;

            if (single == false && strict == false && multiple == false)
                single = true;    // 缺省时候当作 single 处理

            if (multiple)
            {
                // 查重
                if (FindBindingString(strText, strBinding) != -1)
                    strResult = strText;
                else
                    strResult = strText + "," + strBinding;
            }
            else if (single == true || strict == true)
            {
                string strName = "";
                string strValue = "";
                StringUtil.ParseTwoPart(strBinding, ":", out strName, out strValue);

                List<string> results = new List<string>();
                string[] parts = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    string strLine = s.Trim();
                    if (string.IsNullOrEmpty(strLine))
                        continue;
                    string strLeft = "";
                    string strRight = "";
                    StringUtil.ParseTwoPart(strLine, ":", out strLeft, out strRight);
                    if (strName == strLeft)
                    {
                        if (strict)
                        {
                            strError = $"类型 '{strName}' 先前已经被绑定过了";
                            return -1;
                        }
                        continue;   // 忽视所有同类型的号码
                    }
                    results.Add(strLine);
                }

                results.Add(strBinding);
                strResult = StringUtil.MakePathList(results);
            }
            else
            {
                strError = $"style 参数值 '{style}' 不合法。不具备 multiple single singlestrict 之一";
                return -1;
            }

            return 1;
        }

        // 加入一个绑定号码
        public static string AddBindingString(string strText,
            string strBinding,
            bool bMultiple)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strBinding;

            if (bMultiple == false)
            {
                string strName = "";
                string strValue = "";
                StringUtil.ParseTwoPart(strBinding, ":", out strName, out strValue);

                List<string> results = new List<string>();
                string[] parts = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in parts)
                {
                    string strLine = s.Trim();
                    if (string.IsNullOrEmpty(strLine))
                        continue;
                    string strLeft = "";
                    string strRight = "";
                    StringUtil.ParseTwoPart(strLine, ":", out strLeft, out strRight);
                    if (strName == strLeft)
                        continue;   // 忽视所有同类型的号码
                    results.Add(strLine);
                }

                results.Add(strBinding);
                return StringUtil.MakePathList(results);
            }
            else
            {
                // 查重
                if (FindBindingString(strText, strBinding) != -1)
                    return strText;
                return strText + "," + strBinding;
            }
        }

        // 在一个绑定信息字符串里面，找到一个特定的 xxxx:xxxx 部分的下标
        // return:
        //      -1  没有找到
        //      其他  找到的位置下标。注，元素数量没有把空字符串元素计算在内
        static int FindBindingString(string strText,
            string strBinding)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return -1;

            string[] parts = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (string s in parts)
            {
                string strLine = s.Trim();
                if (string.IsNullOrEmpty(strLine))
                    continue;
                if (strLine == strBinding)
                    return i;
                i++;
            }

            return -1;
        }

        // 去掉一个绑定号码
        static string RemoveBindingString(string strText,
            string strBinding)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            List<string> results = new List<string>();

            string[] parts = strText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                string strLine = s.Trim();
                if (string.IsNullOrEmpty(strLine))
                    continue;
                if (MatchBindingString(strLine, strBinding) == true)
                    continue;   // 忽视发现的号码

                // 2016/11/14
                if (string.IsNullOrEmpty(strBinding) == false && strBinding[0] == '@')
                {
                    if (StringUtil.RegexCompare(strBinding.Substring(1),
    RegexOptions.None,
    strLine) == true)
                        continue;
                }

                results.Add(strLine);
            }

            return StringUtil.MakePathList(results);
        }

        // strText -- weixinid:12345678@单位名称
        // strPattern -- weixinid:12345678@*
        static bool MatchBindingString(string strText, string strPattern)
        {
            if (strText == strPattern)
                return true;

            if (string.IsNullOrEmpty(strPattern) == false && strPattern.EndsWith("*"))
            {
                string head = strPattern.Substring(0, strPattern.Length - 1);
                if (strText.StartsWith(head))
                    return true;
            }

            return false;
        }

        // 按照级别对读者记录中的信息字段进行过滤
        // 注: 无论如何都要删除读者记录中的 password 元素
        public static bool FilterByLevel(XmlDocument readerdom,
            string level,
            string style = "read")
        {
            if (string.IsNullOrEmpty(level))
            {
                var node = DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                if (node != null)
                    return true;
                return false;
            }

            bool read = StringUtil.IsInList("read", style);
            bool write = StringUtil.IsInList("write", style);
            // 如果两个都缺乏，默认为读
            if (read == false && write == false)
                read = true;

            bool changed = true;
            /*
基本字段："borrows","overdues","reservations","outofReservations"
第一级：证状态，发证日期，失效日期，姓名(除第一字以后都被马赛克)
第二级：+ 完整姓名，姓名拼音，显示名，性别，民族，证条码号，参考ID，OI，注释
第三级：+ 单位，职务，地址，读者类型
第四级：+ 电话，email
第五级：+ 权限，存取定义，书斋名称，好友
第六级：+ 身份证号，出生日期
第七级：+ 借阅历史，个性化参数等字段
第八级：+ 租金押金字段 
第九级：+ 指纹，掌纹，人脸特征
             * */
            var names = GetElementNames(level);
            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("*");
            foreach (XmlElement element in nodes)
            {
                var myname = GetMyName(element);

                // 马赛克
                if (names.IndexOf("?" + myname) != -1)
                {
                    if (read)
                    {
                        element.InnerText = Mask(element.InnerText);
                        changed = true;
                        continue;
                    }
                    else if (write)
                    {
                        element.ParentNode.RemoveChild(element);
                        changed = true;
                        continue;
                    }
                }

                if (names.IndexOf(myname) == -1)
                {
                    element.ParentNode.RemoveChild(element);
                    changed = true;
                    continue;
                }
            }

            {
                var node = DomUtil.DeleteElement(readerdom.DocumentElement, "password");
                if (node != null)
                    changed = true;
            }

            return changed;

            string Mask(string text)
            {
                if (text == null || text.Length < 2)
                    return text;
                return new string(text[0], 1) + new string('*', text.Length - 1);
            }
        }

        // 获得专用的元素名，形态为 uri:localName
        static string GetMyName(XmlElement element)
        {
            if (string.IsNullOrEmpty(element.NamespaceURI))
                return element.LocalName;
            return element.NamespaceURI + ":" + element.LocalName;
        }

        public class NameElements
        {
            public string Name { get; set; }
            public string Elements { get; set; }
        }

        public static List<NameElements> _name_elements = new List<NameElements>() {
        new NameElements
        {
            Name = "g_shelf",
            Elements = "info,borrows,overdues,reservations,outofReservations,libraryCode,readerType,"   // libraryCode 和 readerType 是为了显示可借总数和当前可借数的需要
        },
        new NameElements
        {
            Name = "g_facerecognition",
            Elements = "barcode,face",
        },
        new NameElements
        {
            Name = "g_faceregister",
            Elements = "face",
        },
        };

        static List<string> GetGroupElementNames(string name)
        {
            var item = _name_elements.Find(o => o.Name == name);
            if (item == null)
                return null;
            return StringUtil.SplitList(item.Elements);
        }

        // 获得元素名列表
        // parameters:
        //      name    元素集合的名称或者定义。
        //              形态: n|元素名|组名
        //              (n代表数字)
        //              注: 这里使用 dprms.file 元素名，实际上表达的是 "http://dp2003.com/dprms:file"，因为 setreaderinfo:xxx 这里 xxx 之内不允许里面再出现冒号，逗号，竖线
        static List<string> GetElementNames(string name)
        {
            List<string> results = new List<string>();
            var parts = StringUtil.SplitList(name, '|');
            foreach (string part in parts)
            {
                if (StringUtil.IsNumber(part))
                {
                    results.AddRange(GetNumberElementNames(part));
                }
                else if (part.StartsWith("g_"))
                {
                    var result = GetGroupElementNames(part);
                    if (result == null)
                        throw new Exception($"不存在名为 '{part}' 的元素组");
                    results.AddRange(results);
                }
                else if (part.StartsWith("r_"))
                    continue;
                else
                {
                    // 2021/7/23
                    if (part == "dprms.file")
                        results.Add("http://dp2003.com/dprms:file");
                    else
                        results.Add(part);
                }
            }

            // 去掉重复元素
            StringUtil.RemoveDupNoSort(ref results);

            // 2021/8/4
            // 去除发生冲突的名字。比如 ?name 和后方的 name 冲突了，?name 应予删除
            results = RemoveConflictName(results);

            return results;
        }

        // 去除发生冲突的名字。比如 ?name 和后方的 name 冲突了，?name 应予删除
        public static List<string> RemoveConflictName(List<string> names)
        {
            List<string> results = new List<string>(names);
            for (int i = 0; i < results.Count; i++)
            {
                string name = results[i];
                if (name.StartsWith("?") == false)
                    continue;
                name = name.Substring(1);
                for (int j = 0; j < results.Count; j++)
                {
                    if (j == i)
                        continue;
                    if (j > i)
                    {
                        if (name == results[j])
                        {
                            results.RemoveAt(i);
                            i--;
                            break;
                        }
                    }
                    else
                    {
                        if (name == results[j])
                        {
                            results.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }

            return results;
        }

        // 获得一个 level 可以传输的读者 XML 元素名字列表
        // 注: 名字前面有个问号的，表示元素正文在传输中要被马赛克遮盖部分字符
        static List<string> GetNumberElementNames(string level)
        {
            List<string> names = new List<string>();

            // 基本字段。馆代码，读者类型，证条码号，证号，参考 ID，机构代码(OI)，高级信息，借阅的册，违约金, 预约未取参数
            names.AddRange(new string[] {
                "libraryCode",
                "readerType",
                "barcode",
                "cardNumber",
                "refID",
                "oi",
                "info",
                "borrows",
                "overdues",
                "reservations",
                "outofReservations" });

            // 第一级：证状态，发证日期，失效日期，姓名(除第一字以后都被马赛克)
            names.AddRange(new string[] { "state", "createDate", "expireDate" });
            if (level == "1")
            {
                names.Add("?name");
                return names;
            }
            // 第二级：+ 完整姓名，姓名拼音，显示名，性别，民族，注释
            names.AddRange(new string[] { "name", "namePinyin", "displayName", "gender", "nation", "comment" });
            if (level == "2")
                return names;
            // 第三级：+ 单位，职务，地址
            names.AddRange(new string[] { "department", "post", "address" });
            if (level == "3")
                return names;
            // 第四级：+ 电话，email
            names.AddRange(new string[] { "tel", "email" });
            if (level == "4")
                return names;
            // 第五级：+ 权限，存取定义，书斋名称，好友
            names.AddRange(new string[] { "rights", "access", "personalLibrary", "friends" });
            if (level == "5")
                return names;
            // 第六级：+ 身份证号，出生日期
            names.AddRange(new string[] { "idCardNumber", "dateOfBirth" });
            if (level == "6")
                return names;
            // 第七级：+ 借阅历史，个性化参数等字段
            names.AddRange(new string[] { "borrowHistory", "preference" });
            if (level == "7")
                return names;
            // 第八级：+ 租金押金字段 
            names.AddRange(new string[] { "hire", "foregift" });
            if (level == "8")
                return names;
            // 第九级：+ 指纹，掌纹，人脸特征，对象(证照片，人脸图片等)
            names.AddRange(new string[] { "fingerprint", "palmprint", "face", "http://dp2003.com/dprms:file" });
            if (level == "9")
                return names;

            return names;
        }

        // 从 rights 字符串中得到 getreaderinfo 权限的 level 字符串
        // 原理：权限字符串中通常会这样定义 getreaderinfo:1，其中 1 表示 level "1"
        // return:
        //      null    没有找到 getreaderinfo 前缀
        //      ""      找到了前缀，并且 level 部分为空
        //      其他     返回 level 部分
        public static string GetReaderInfoLevel(string prefix, string rights)
        {
            // return:
            //      null    没有找到前缀
            //      ""      找到了前缀，并且值部分为空
            //      其他     返回值部分
            var level = StringUtil.GetParameterByPrefix(rights, prefix);
            return level;
        }

        // 合并两个权限字符串。
        // 如果 rights2 里面有 getreaderinfo:n，那么要去掉 rights1 里面的 getreaderinfo:n，然后合并
        public static string MergeRights(string rights1, string rights2)
        {
            if (string.IsNullOrEmpty(rights1))
                return rights2 == null ? "" : rights2;
            if (string.IsNullOrEmpty(rights2))
                return rights1 == null ? "" : rights1;

            var list1 = new List<string>(rights1.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            var list2 = new List<string>(rights2.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            var delete_list = new List<string>();
            foreach (string r2 in list2)
            {
                string prefix = GetPrefix(r2);
                foreach (string r1 in list1)
                {
                    if (r1 == prefix || r1.Contains(prefix + ":"))
                        delete_list.Add(r1);
                }
            }

            foreach (var right in delete_list)
            {
                list1.Remove(right);
            }

            list1.AddRange(list2);
            return string.Join(",", list1);
        }

        public static string GetPrefix(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            int index = text.IndexOf(":");
            if (index == -1)
                return text;
            return text.Substring(0, index);
        }

        // 获得数据中存在的 element_names 以外的元素名列表
        // 指根元素下的元素，和所有位置的 dprms:file 元素
        // parameters:
        //      element_names   元素名集合
        //      exclude_names   判断时要忽视的元素名
        static List<string> GetOutofElements(XmlDocument readerdom,
            string[] element_names,
            string[] exclude_names)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            List<string> results = new List<string>();
            var nodes = readerdom.DocumentElement.SelectNodes("* | //dprms:file", nsmgr);
            //List<XmlNode> nodes = new List<XmlNode>(readerdom.DocumentElement.SelectNodes("*").Cast<XmlNode>());
            //nodes.AddRange(readerdom.DocumentElement.SelectNodes("//dprms:file").Cast<XmlNode>());
            foreach (XmlElement element in nodes)
            {
                string name = GetMyName(element);

                // 忽略判断一些保留字段
                if (Array.IndexOf(exclude_names, name) != -1)
                    continue;

                if (Array.IndexOf(element_names, name) == -1)
                    results.Add(name);
            }

            StringUtil.RemoveDupNoSort(ref results);
            return results;
        }
    }
}
