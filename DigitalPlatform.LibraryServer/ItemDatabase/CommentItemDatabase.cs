﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;
using System.Linq;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 评注数据库。订购荐购意见、书评
    /// 2008/12/8
    /// </summary>
    public class CommentItemDatabase : ItemDatabase
    {
        // 要害元素名列表
        static string[] core_comment_element_names = new string[] {
                "parent",   // 
                "index",    // 编号
                "state",    // 状态
                "type",    // 类型
                "title",    // 标题
                // "creator",  // 创建者
                "subject",
                "summary",
                "content", // 文字内容
                "createTime",   // 创建时间
                //"lastModified",    // 最后修改时间
                //"lastModifier",    // 最后修改者
                "follow",   // 所跟从的(帖子)记录ID
                "refID",    // 参考ID
                // "operations", // 2023/2/24 注: operations 应该由服务器决定，前端无法修改
                "orderSuggestion",  // 2010/11/8
                // "libraryCode",  // 2012/10/3
        };

        static string[] readerchangeable_comment_element_names = new string[] {
                "parent",   // 2003/2/23
                "title",    // 标题
                "subject",
                "summary",
                "content", // 文字内容
                "follow",   // 所跟从的(帖子)记录ID
                "orderSuggestion",  // 2010/11/8
        };

        // 服务器自动维护的评注记录字段名。PartialDenied 时候不用警告这些字段名
        static string[] _auto_maintain_comment_element_names = new string[] {
                "libraryCode",
                "operations",
                "http://dp2003.com/dprms:file",
        };

        /* 2023/2/2 注:
         * 注意 reader 账户需要具有 setcommentobject 权限，读者在 OPAC 界面上才能顺利上传和修改评注记录的图片附件
         * 
         * */

        // DoOperChange()和DoOperMove()的下级函数
        // 合并新旧记录
        // return:
        //      -1  出错
        //      0   正确
        //      1   有部分修改没有兑现。说明在strError中
        public override int MergeTwoItemXml(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument domExistParam,
            XmlDocument domNewParam,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            XmlDocument domExist = new XmlDocument();
            domExist.LoadXml(domExistParam.OuterXml);

            XmlDocument domNew = new XmlDocument();
            domNew.LoadXml(domNewParam.OuterXml);

            string strWarning = "";

            bool bChangePartDeniedParam = false;
            // 合并记录
            // parameters:
            //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
            //      domNew  新记录。
            //      domOld  旧记录。函数执行后其内容会被改变
            // return:
            //      -1  error
            //      0   old record not changed
            //      1   old record changed
            int nRet = MergeNewOldRec(
                "comment",
                sessioninfo.RightsOrigin,
                domNew,
                domExist,
                ref bChangePartDeniedParam,
                out strError);
            if (nRet == -1)
                return -1;

            if (bChangePartDeniedParam)
                strWarning = strError;

            var element_table = new List<string>(core_comment_element_names);

            if (sessioninfo != null
                && sessioninfo.Account != null
                && sessioninfo.UserType == "reader")
            {
                // 2018/8/15
                // 注意: 读者也可能具备修改 state 的能力，如果读者具备 managecomment 权限的话
                bool bManager = false;
                if (string.IsNullOrEmpty(sessioninfo.UserID) == true
    || StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
                    bManager = false;
                else
                    bManager = true;

                // 对不具备管理 comment 权限的读者，降低修改字段的权限范围
                // TODO: 对于读者身份，创建记录的时候可以写入 parent 元素，但修改记录的时候不允许修改 parent 元素
                if (bManager == false && strAction != "delete")
                    element_table = new List<string>(readerchangeable_comment_element_names);
            }

            if (strAction == "delete")
                element_table.Add("creator");

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中

            /*
            // 要害元素名列表
            string[] element_names = new string[] {
                "parent",   // 父记录ID。也就是所从属的上一级评注记录的id
                "index",    // 编号
                "state",    // 状态
                "title",    // 标题
                "creator",  // 创建者
                "createTime",   // 创建时间
                "lastModifyTime",    // 最后修改时间
                "root",   // 根记录ID。也就是所从属的书目记录ID
                "content", // 文字内容
            };*/

            for (int i = 0; i < element_table.Count; i++)
            {
                /*
                string strTextNew = DomUtil.GetElementText(domNew.DocumentElement,
                    core_comment_element_names[i]);

                DomUtil.SetElementText(domExist.DocumentElement,
                    core_comment_element_names[i], strTextNew);
                 * */
                string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                    element_table[i]);

                DomUtil.SetElementOuterXml(domExist.DocumentElement,
                    element_table[i], strTextNew);
            }

#if OLDCODE
            // 清除以前的<dprms:file>元素
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = domExist.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlNode node in nodes)
            {
                node.ParentNode.RemoveChild(node);
            }
            // 兑现新记录中的 dprms:file 元素
            nodes = domNew.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement node in nodes)
            {
                XmlDocumentFragment frag = domExist.CreateDocumentFragment();
                frag.InnerXml = node.OuterXml;
                domExist.DocumentElement.AppendChild(frag);
            }
#endif

            /*
            // 2012/10/3
            // 当前用户所管辖的馆代码
            DomUtil.SetElementText(domExist.DocumentElement,
                "libraryCode",
                sessioninfo.LibraryCodeList);
             * */

            // 2023/2/27
            // 观察被阻止修改修改的元素
            {
                List<string> unprocessed_element_names = new List<string>();
                unprocessed_element_names.AddRange(LibraryApplication.GetUnprocessedElementNames(
                    domExistParam,
                    element_table.ToArray(),
                    false));
                unprocessed_element_names.AddRange(LibraryApplication.GetUnprocessedElementNames(
    domExist,
    element_table.ToArray(),
    false));
                StringUtil.RemoveDupNoSort(ref unprocessed_element_names);

                // 根据元素名集合，看看其中哪些元素内容原本期望发生变化
                var denied_element_names = new List<string>();
                denied_element_names.AddRange(LibraryApplication.DetectElementChanging(
domExistParam,
domNewParam,
unprocessed_element_names.Except(_auto_maintain_comment_element_names)));
                // 

                if (denied_element_names.Count > 0)
                {
                    bChangePartDeniedParam = true;
                    if (string.IsNullOrEmpty(strWarning) == false)
                        strWarning += "; ";
                    strWarning += $"下列元素被拒绝修改: {StringUtil.MakePathList(denied_element_names)}";
                }
            }

            // 2023/3/23
            // 为旧版本记录增补 creator 元素
            // UpgradeCommentRecord(domExist);

            // 修改者不能改变最初的馆代码
            strMergedXml = domExist.OuterXml;

            if (string.IsNullOrEmpty(strWarning) == false)
            {
                strError = strWarning;
                return 1;
            }

            return 0;
        }

        // (派生类必须重载)
        // 比较两个记录, 看看和事项要害信息有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        public override int IsItemInfoChanged(XmlDocument domExist,
            XmlDocument domOldRec)
        {
            for (int i = 0; i < core_comment_element_names.Length; i++)
            {
                string strText1 = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                    core_comment_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(domOldRec.DocumentElement,
                    core_comment_element_names[i]);

                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }

        public CommentItemDatabase(LibraryApplication app)
            : base(app)
        {
        }

#if NO
        public int BuildLocateParam(
            string strBiblioRecPath,
            string strIndex,
            out List<string> locateParam,
            out string strError)
        {
            strError = "";
            locateParam = null;

            int nRet = 0;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strCommentDbName = "";

            // 根据书目库名, 找到对应的事项库名
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
            if (String.IsNullOrEmpty(strCommentDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                goto ERROR1;
            }

            string strRootID = ResPath.GetRecordId(strBiblioRecPath);

            locateParam = new List<string>();
            locateParam.Add(strCommentDbName);
            locateParam.Add(strRootID);
            locateParam.Add(strIndex);

            return 0;
        ERROR1:
            return -1;
        }
#endif
        public override int BuildLocateParam(// string strBiblioRecPath,
string strRefID,
out List<string> locateParam,
out string strError)
        {
            strError = "";
            locateParam = null;

            /*
            int nRet = 0;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strCommentDbName = "";

            // 根据书目库名, 找到对应的事项库名
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
            if (String.IsNullOrEmpty(strCommentDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                goto ERROR1;
            }
             * */

            locateParam = new List<string>();
            locateParam.Add(strRefID);
            return 0;
        }

#if NO
        // 构造用于获取事项记录的XML检索式
        public override int MakeGetItemRecXmlSearchQuery(
            List<string> locateParams,
            int nMax,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 3)
            {
                strError = "locateParams数组内的元素必须为3个";
                return -1;
            }

            string strCommentDbName = locateParams[0];
            string strRootID = locateParams[1];
            string strIndex = locateParams[2];

            strQueryXml = "<target list='"
        + StringUtil.GetXmlStringSimple(strCommentDbName + ":" + "编号")
        + "'><item><word>"
        + StringUtil.GetXmlStringSimple(strIndex)
        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml += "<operator value='AND'/>";

            strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strCommentDbName + ":" + "根记录")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strRootID)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml = "<group>" + strQueryXml + "</group>";

            return 0;
        }
#endif
        // 构造用于获取事项记录的XML检索式
        public override int MakeGetItemRecXmlSearchQuery(
            List<string> locateParams,
            int nMax,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }

            string strRefID = locateParams[0];

            // 构造检索式
            int nDbCount = 0;
            for (int i = 0; i < this.App.ItemDbs.Count; i++)
            {
                string strDbName = this.App.ItemDbs[i].CommentDbName;

                if (String.IsNullOrEmpty(strDbName) == true)
                    continue;

                string strOneDbQuery = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + "参考ID")
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strRefID)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>" + nMax.ToString() + "</maxCount></item><lang>zh</lang></target>";

                if (nDbCount > 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                    strQueryXml += "<operator value='OR'/>";
                }

                strQueryXml += strOneDbQuery;
                nDbCount++;
            }

            if (nDbCount > 0)
            {
                strQueryXml = "<group>" + strQueryXml + "</group>";
            }

            return 0;
        }


#if NO
        // 构造定位提示信息。用于报错。
        public override int GetLocateText(
            List<string> locateParams,
            out string strText,
            out string strError)
        {
            strText = "";
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 3)
            {
                strError = "locateParams数组内的元素必须为3个";
                return -1;
            }
            string strCommentrDbName = locateParams[0];
            string strRootID = locateParams[1];
            string strIndex = locateParams[2];

            strText = "评注库为 '" + strCommentrDbName + "'，根记录ID为 '" + strRootID + "' 编号为 '" + strIndex + "'";
            return 0;
        }
#endif

#if NO
        // 观察已存在的记录中，唯一性字段是否和要求的一致
        // return:
        //      -1  出错
        //      0   一致
        //      1   不一致。报错信息在strError中
        public override int IsLocateInfoCorrect(
            List<string> locateParams,
            XmlDocument domExist,
            out string strError)
        {
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 3)
            {
                strError = "locateParams数组内的元素必须为3个";
                return -1;
            }
            string strCommentDbName = locateParams[0];
            string strRootID = locateParams[1];
            string strIndex = locateParams[2];

            if (String.IsNullOrEmpty(strIndex) == false)
            {
                string strExistingIndex = DomUtil.GetElementText(domExist.DocumentElement,
                    "index");
                if (strExistingIndex != strIndex)
                {
                    strError = "评注记录中<index>元素中的编号 '" + strExistingIndex + "' 和通过删除操作参数指定的编号 '" + strIndex + "' 不一致。";
                    return 1;
                }
            }

            return 0;
        }
#endif

        // 观察已经存在的记录是否有流通信息
        // return:
        //      -1  出错
        //      0   没有
        //      1   有。报错信息在strError中
        public override int HasCirculationInfo(XmlDocument domExist,
            out string strError)
        {
            strError = "";
            return 0;
        }

        // 是否允许创建新记录?
        // parameters:
        // return:
        //      -1  出错。不允许修改。
        //      0   不允许创建，因为权限不够等原因。原因在strError中
        //      1   可以创建
        public override int CanCreate(
            SessionInfo sessioninfo,
            XmlDocument domNew,
            out string strError)
        {
            strError = "";

            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                return -1;
            }

            // 已经注销的读者不允许创建评注
            if (sessioninfo.UserType == "reader")
            {
                string strReaderState = DomUtil.GetElementText(sessioninfo.Account.PatronDom.DocumentElement,
                    "state");
                if (StringUtil.IsInList("注销", strReaderState) == true)
                {
                    strError = "读者证状态为 注销， 不能创建评注记录";
                    return 0;
                }
            }

            return 1;
        }

        // 是否允许对旧记录进行修改? 
        // parameters:
        // return:
        //      -1  出错。不允许修改。
        //      0   不允许修改，因为权限不够等原因。原因在strError中
        //      1   可以修改
        public override int CanChange(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument domExist,
            XmlDocument domNew,
            out string strError)
        {
            strError = "";

            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                return -1;
            }

            // 2023/3/23
            UpgradeCommentRecord(domExist);
            UpgradeCommentRecord(domNew);
            // 注: exist 和 new 都自动加上 creator 元素，避免交叉的时候，交叉算法以为 creator 发生了删除(也就是说 new 里面少了 creator 元素导致交叉算法出现 PartialDenied 警告)

            // 如果经过 UpgradeCommentRecord() 以后 domNew 依然没有 creator 元素(可能是因为 operations 元素缺失)
            {
                var new_creator_node = domNew.DocumentElement.SelectSingleNode("creator") as XmlElement;
                var exist_creator_node = domExist.DocumentElement.SelectSingleNode("creator") as XmlElement;

                if (new_creator_node == null && exist_creator_node != null)
                {
                    DomUtil.SetElementText(domNew.DocumentElement,
                        "creator",
                        exist_creator_node.InnerText.Trim());
                }
            }


            string strOldUserID = DomUtil.GetElementText(domExist.DocumentElement,
    "creator");
            string strNewUserID = DomUtil.GetElementText(domNew.DocumentElement,
    "creator");

            /*
            if (strNewUserID != strOldUserID)
            {
                strError = "不允许修改已有评注记录的 creator 元素";    // 读者身份的用户
                return 0;
            }
            */

            // 已经注销的读者不允许修改评注
            if (sessioninfo.UserType == "reader")
            {
                string strReaderState = DomUtil.GetElementText(sessioninfo.Account.PatronDom.DocumentElement,
                    "state");
                if (StringUtil.IsInList("注销", strReaderState) == true)
                {
                    strError = "读者证状态为 注销， 不能修改任何评注记录";
                    return 0;
                }
            }

            // 普通读者和工作人员
            if (StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
            {
                string strOperator = sessioninfo.UserID;
                if (strOperator != strOldUserID)
                {
                    strError = $"不允许当前用户 '{strOperator}' 修改由其他用户 '{strOldUserID}' 创建的评注记录";
                    return 0;
                }

                string strState = DomUtil.GetElementText(domExist.DocumentElement,
"state");
                if (StringUtil.IsInList("锁定", strState) == true)
                {
                    strError = "不允许修改处于锁定状态的评注记录";
                    return 0;
                }
            }

            /*
            // 2012/10/4
            if (sessioninfo.GlobalUser == false)
            {
                string strLibraryCode = DomUtil.GetElementText(domExist.DocumentElement,
    "libraryCode");
                // 检查当前用户是否管辖评注记录
                if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                {
                    strError = "当前评注记录(<libraryCode>元素中)的馆代码 '" + strLibraryCode + "' 不在当前用户的馆代码 '" + sessioninfo.LibraryCodeList + "' 管辖范围内。修改评注记录的操作被拒绝";
                    return 0;
                }
            }
            */

            return 1;
        }

        // 记录是否允许删除?
        // return:
        //      -1  出错。不允许删除。
        //      0   不允许删除，因为权限不够等原因。原因在strError中
        //      1   可以删除
        public override int CanDelete(
            SessionInfo sessioninfo,
            XmlDocument domExist,
            out string strError)
        {
            strError = "";

            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                return -1;
            }

            // 2023/3/23
            UpgradeCommentRecord(domExist);

            // 已经注销的读者不允许删除评注
            if (sessioninfo.UserType == "reader")
            {
                string strReaderState = DomUtil.GetElementText(sessioninfo.Account.PatronDom.DocumentElement,
    "state");
                if (StringUtil.IsInList("注销", strReaderState) == true)
                {
                    strError = "读者证状态为 注销， 不能删除任何评注记录";
                    return 0;
                }
            }

            // 普通读者和工作人员
            if (StringUtil.IsInList("managecomment", sessioninfo.RightsOrigin) == false)
            {
                string strNewUserID = sessioninfo.UserID;
                string strOldUserID = DomUtil.GetElementText(domExist.DocumentElement,
                    "creator");
                if (strNewUserID != strOldUserID)
                {
                    strError = $"不允许当前用户 '{strNewUserID}' 删除由其他用户 '{strOldUserID}' 创建的评注记录";
                    return 0;
                }

                string strState = DomUtil.GetElementText(domExist.DocumentElement,
                    "state");
                if (StringUtil.IsInList("锁定", strState) == true)
                {
                    strError = "不允许删除处于锁定状态的评注记录";
                    return 0;
                }
            }

            /*
            // 2012/10/4
            if (sessioninfo.GlobalUser == false)
            {
                string strLibraryCode = DomUtil.GetElementText(domExist.DocumentElement,
    "libraryCode");
                // 检查当前用户是否管辖评注记录
                if (StringUtil.IsInList(strLibraryCode, sessioninfo.LibraryCodeList) == false)
                {
                    strError = "当前评注记录(<libraryCode>元素中)的馆代码 '" + strLibraryCode + "' 不在当前用户的馆代码 '" + sessioninfo.LibraryCodeList + "' 管辖范围内。删除评注记录的操作被拒绝";
                    return 0;
                }
            }
            */
            return 1;
        }

#if NO
        // 定位参数值是否为空?
        // return:
        //      -1  出错
        //      0   不为空
        //      1   为空(这时需要在strError中给出报错说明文字)
        public override int IsLocateParamNullOrEmpty(
            List<string> locateParams,
            out string strError)
        {
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 3)
            {
                strError = "locateParams数组内的元素必须为3个";
                return -1;
            }
            string strCommentDbName = locateParams[0];
            string strRootID = locateParams[1];
            string strIndex = locateParams[2];

            if (String.IsNullOrEmpty(strIndex) == true)
            {
                strError = "<index>元素中的编号为空";
                return 1;
            }

            return 0;
        }
#endif

        // 事项名称。
        public override string ItemName
        {
            get
            {
                return "评注";
            }
        }

        // 事项名称。
        public override string ItemNameInternal
        {
            get
            {
                return "Comment";
            }
        }

        public override string DefaultResultsetName
        {
            get
            {
                return "comments";
            }
        }

        // 准备写入日志的SetXXX操作字符串。例如“SetEntity” “SetIssue”
        public override string OperLogSetName
        {
            get
            {
                return "setComment";
            }
        }

        public override string SetApiName
        {
            get
            {
                return "SetComments";
            }
        }

        public override string GetApiName
        {
            get
            {
                return "GetComments";
            }
        }

        // 给旧版本的评注记录增添 creator 元素
        static bool UpgradeCommentRecord(XmlDocument dom)
        {
            var creator_node = dom.DocumentElement.SelectSingleNode("creator") as XmlElement;
            if (creator_node != null)
                return false;
            var operation_node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='create']") as XmlElement;
            if (operation_node == null)
                return false;
            var operator_value = operation_node.GetAttribute("operator");
            if (string.IsNullOrEmpty(operator_value))
                return false;
            DomUtil.SetElementText(dom.DocumentElement,
                "creator",
                operator_value);
            return true;
        }

        // 构造出适合保存的新事项记录
        // parameters:
        //      bForce  是否为强制保存?
        //              如果是强制保存，则直接把 strOriginXml 当作结果；
        //              否则，会将 strExistingXml(已存在内容) 和 strOriginXml(新内容) 按照当前账户权限合并
        //      strExistingXml  创建前，原位置已经存在的记录内容
        //      strOriginXml    打算创建的新内容
        // return:
        //      -1  出错
        //      0   正确
        //      1   有部分修改没有兑现。说明在strError中
        //      2   全部修改都没有兑现。说明在strError中
        public override int BuildNewItemRecord(
            SessionInfo sessioninfo,
            bool bForce,
            string strBiblioRecId,
            string strExistingXml,
            string strOriginXml,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            int nPartialRet = 0;
            string warning = "";

            // return:
            //      -1  出错
            //      0   正确
            //      1   有部分修改没有兑现。说明在strError中
            //      2   全部修改都没有兑现。说明在strError中
            int nRet = base.BuildNewItemRecord(sessioninfo,
            bForce,
            strBiblioRecId,
            strExistingXml,
            strOriginXml,
            out strXml,
            out strError);
            if (nRet == -1)
                return -1;
            nPartialRet = nRet;
            if (nRet == 1 || nRet == 2)
            {
                warning = strError;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "装载 strXml 到DOM时出错: " + ex.Message;
                return -1;
            }


            // 2024/5/24
            if (bForce)
                goto END1;


            /*
            // 如果 ID 为非空，才会主动修改/写入 parent 元素
            if (string.IsNullOrEmpty(strBiblioRecId) == false)
            {
                // 2010/4/2
                DomUtil.SetElementText(dom.DocumentElement,
                "parent",
                strBiblioRecId);
            }
            */
            // 2023/2/23
            // 由 dp2library 服务器强制产生 creator 元素
            DomUtil.SetElementText(dom.DocumentElement,
                "creator",
                sessioninfo.UserID);

            // 2012/10/3
            // 当前用户所管辖的馆代码
            DomUtil.SetElementText(dom.DocumentElement,
                "libraryCode",
                sessioninfo.LibraryCodeList);

        /*
        // 2017/1/13
        DomUtil.RemoveEmptyElements(dom.DocumentElement);
        */
        END1:
            strXml = dom.OuterXml;
            if (string.IsNullOrEmpty(warning) == false)
                strError = warning;
            return nPartialRet;
        }

        // 获得事项数据库名
        // return:
        //      -1  error
        //      0   没有找到(书目库)
        //      1   found
        public override int GetItemDbName(string strBiblioDbName,
            out string strItemDbName,
            out string strError)
        {
            return this.App.GetCommentDbName(strBiblioDbName,
                out strItemDbName,
                out strError);
        }

        // 2012/4/27
        public override bool IsItemDbName(string strItemDbName)
        {
            return this.App.IsCommentDbName(strItemDbName);
        }

#if NO
        // 对新旧事项记录中包含的定位信息进行比较, 看看是否发生了变化(进而就需要查重)
        // parameters:
        //      oldLocateParam   顺便返回旧记录中的定位参数
        //      newLocateParam   顺便返回新记录中的定位参数
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        public override int CompareTwoItemLocateInfo(
            string strItemDbName,
            XmlDocument domOldRec,
            XmlDocument domNewRec,
            out List<string> oldLocateParam,
            out List<string> newLocateParam,
            out string strError)
        {
            strError = "";


            string strOldIndex = DomUtil.GetElementText(domOldRec.DocumentElement,
                "index");

            string strNewIndex = DomUtil.GetElementText(domNewRec.DocumentElement,
                "index");


            string strOldRootID = DomUtil.GetElementText(domOldRec.DocumentElement,
                "root");

            string strNewRootID = DomUtil.GetElementText(domNewRec.DocumentElement,
                "root");

            oldLocateParam = new List<string>();
            oldLocateParam.Add(strItemDbName);
            oldLocateParam.Add(strOldRootID);
            oldLocateParam.Add(strOldIndex);

            newLocateParam = new List<string>();
            newLocateParam.Add(strItemDbName);
            newLocateParam.Add(strNewRootID);
            newLocateParam.Add(strNewIndex);

            if (strOldIndex != strNewIndex)
                return 1;   // 不相等

            return 0;   // 相等。
        }
#endif

#if NO
        public override void LockItem(List<string> locateParam)
        {
            string strItemDbName = locateParam[0];
            string strRootID = locateParam[1];
            string strIndex = locateParam[2];

            this.App.EntityLocks.LockForWrite(
                "comment:" + strItemDbName + "|" + strRootID + "|" + strIndex);
        }

        public override void UnlockItem(List<string> locateParam)
        {
            string strItemDbName = locateParam[0];
            string strRootID = locateParam[1];
            string strIndex = locateParam[2];

            this.App.EntityLocks.UnlockForWrite(
                "comment:" + strItemDbName + "|" + strRootID + "|" + strIndex);
        }
#endif

        // 获得命令行中的事项记录路径
        // return:
        //      -1  error
        //      0   不是命令行
        //      1   是命令行
        public override int GetCommandItemRecPath(
            List<string> locateParam,
            out string strItemRecPath,
            out string strError)
        {
            strItemRecPath = "";
            strError = "";

            if (locateParam.Count < 3)
            {
                strError = "locateParam参数必须为3个元素以上。第3个元素为可能的命令行";
                return -1;
            }

            string strCommandLine = locateParam[3];

            // 解析命令行中的事项记录路径
            // return:
            //      -1  error
            //      0   不是命令行
            //      1   是命令行
            return ParseCommandItemRecPath(
                strCommandLine,
                out strItemRecPath,
                out strError);
        }

        public override int VerifyItem(
LibraryHost host,
string strAction,
XmlDocument itemdom,
out string strError)
        {
            strError = "";

            // 执行函数
            try
            {
                return host.VerifyComment(strAction,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "VerifyComment" + "' 时出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            // return 0;
        }
    }
}
