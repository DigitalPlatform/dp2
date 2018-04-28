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
    /// 期事项库
    /// </summary>
    public class IssueItemDatabase : ItemDatabase
    {
        // 要害元素名列表
        static string[] core_issue_element_names = new string[] {
                "parent",
                "state",    // 状态
                "publishTime",  // 出版时间
                "issue",    // 当年期号
                "zong",   // 总期号
                "volume",   // 卷号
                "orderInfo",    // 订购信息
                "comment",  // 注释
                "batchNo",   // 批次号
                "refID",    // 参考ID 2010/2/27 add
                "operations", // 2010/4/7
            };

        // (派生类必须重载)
        // 比较两个记录, 看看和事项要害信息有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        public override int IsItemInfoChanged(XmlDocument domExist,
            XmlDocument domOldRec)
        {
            for (int i = 0; i < core_issue_element_names.Length; i++)
            {
                string strText1 = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                    core_issue_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(domOldRec.DocumentElement,
                    core_issue_element_names[i]);

                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }

        // DoOperChange()和DoOperMove()的下级函数
        // 合并新旧记录
        // return:
        //      -1  出错
        //      0   正确
        //      1   有部分修改没有兑现。说明在strError中
        public override int MergeTwoItemXml(
            SessionInfo sessioninfo,
            XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";
            int nRet = 0;

            if (sessioninfo != null
&& sessioninfo.Account != null
&& sessioninfo.UserType == "reader")
            {
                strError = "期库记录不允许读者进行修改";
                return -1;
            }

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中

            /*
            // 要害元素名列表
            string[] element_names = new string[] {
                "parent",
                "state",    // 状态
                "publishTime",  // 出版时间
                "issue",    // 当年期号
                "zong",   // 总期号
                "volume",   // 卷号
                "orderInfo",    // 订购信息
                "comment",  // 注释
                "batchNo"   // 批次号
            };
             * */

            bool bControlled = true;
            {
                XmlNode nodeExistRoot = domExist.DocumentElement.SelectSingleNode("orderInfo");
                if (nodeExistRoot != null)
                {
                    // 是否全部订购信息片断中的馆藏地点都在当前用户管辖之下?
                    // return:
                    //      -1  出错
                    //      0   不是全部都在管辖范围内
                    //      1   都在管辖范围内
                    nRet = IsAllOrderControlled(nodeExistRoot,
                        sessioninfo.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        bControlled = false;
                }

                if (bControlled == true)
                {
                    // 再看新内容是不是也全部在管辖之下
                    XmlNode nodeNewRoot = domNew.DocumentElement.SelectSingleNode("orderInfo");
                    if (nodeNewRoot != null)
                    {
                        // 是否全部订购信息片断中的馆藏地点都在当前用户管辖之下?
                        // return:
                        //      -1  出错
                        //      0   不是全部都在管辖范围内
                        //      1   都在管辖范围内
                        nRet = IsAllOrderControlled(nodeNewRoot,
                            sessioninfo.LibraryCodeList,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                            bControlled = false;
                    }
                }
            }

            if (bControlled == true // 控制了全部用到的馆藏地点的情形，也可以修改基本字段。并且具有删除 <orderInfo> 中某些片断的能力，只要新记录中不包含这些片断，就等于删除了
                || sessioninfo.GlobalUser == true) // 只有全局用户才能修改基本字段
            {
                for (int i = 0; i < core_issue_element_names.Length; i++)
                {
                    /*
                    string strTextNew = DomUtil.GetElementText(domNew.DocumentElement,
                        element_names[i]);

                    DomUtil.SetElementText(domExist.DocumentElement,
                        element_names[i], strTextNew);
                     * */
                    // 2009/10/24 changed inner-->outer
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        core_issue_element_names[i]);

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        core_issue_element_names[i], strTextNew);
                }
            }

            // 分馆用户要特意单独处理<orderInfo>元素
            if (sessioninfo.GlobalUser == false
                && bControlled == false)
            {
                // 分馆用户提交的<orderInfo>元素内可能包含的<root>元素个数要较少，但并不意味着要删除多余的<root>元素
                XmlNode nodeNewRoot = domNew.DocumentElement.SelectSingleNode("orderInfo");
                XmlNode nodeExistRoot = domExist.DocumentElement.SelectSingleNode("orderInfo");
                if (nodeNewRoot != null && nodeExistRoot == null)
                {
                    //strError = "不允许分馆用户为期记录增补<orderInfo>元素";    // 必须以前的记录就存在<orderInfo>元素
                    //return -1;
                    // 增补
                    nodeExistRoot = domExist.CreateElement("orderInfo");
                    domExist.DocumentElement.AppendChild(nodeExistRoot);
                }

                if (nodeNewRoot == null || nodeExistRoot == null)
                    goto END1;

                // 在已经存在的记录中找出当前用户能管辖的订购片断
                List<XmlNode> exists_overwriteable_nodes = new List<XmlNode>();
                XmlNodeList exist_nodes = nodeExistRoot.SelectNodes("*");
                foreach (XmlNode exist_node in exist_nodes)
                {
                    string strRefID = DomUtil.GetElementText(exist_node, "refID");
                    if (string.IsNullOrEmpty(strRefID) == true)
                        continue;   // 无法定位，所以跳过?
                    string strDistribute = DomUtil.GetElementText(exist_node, "distribute");
                    if (string.IsNullOrEmpty(strDistribute) == true)
                        continue;

                    // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                    // return:
                    //      -1  出错
                    //      0   超过管辖范围。strError中有解释
                    //      1   在管辖范围内
                    nRet = DistributeInControlled(strDistribute,
                sessioninfo.LibraryCodeList,
                out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        exists_overwriteable_nodes.Add(exist_node);
                    }
                }

                // 对新提交的记录中的每个订购片断进行循环
                XmlNodeList new_nodes = nodeNewRoot.SelectNodes("*");
                foreach (XmlNode new_node in new_nodes)
                {
                    string strRefID = DomUtil.GetElementText(new_node, "refID");
                    if (string.IsNullOrEmpty(strRefID) == true)
                    {
                        // 前端提交的一个订购片断refid为空
                        strError = "期记录中的订购XML片断其<refID>元素内容不能为空";
                        return -1;
                    }
                    XmlNode exist_node = nodeExistRoot.SelectSingleNode("*[./refID[text()='" + strRefID + "']]");
                    if (exist_node == null)
                    {
                        // 前端提交的一个订购片断匹配不上refid
                        // 如果新增的XML片断，其中distribute字符串表明全部在管辖范围，还是允许新增
                        string strDistribute = DomUtil.GetElementText(new_node, "distribute");
                        if (string.IsNullOrEmpty(strDistribute) == false)
                        {

                            // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                            // return:
                            //      -1  出错
                            //      0   超过管辖范围。strError中有解释
                            //      1   在管辖范围内
                            nRet = DistributeInControlled(strDistribute,
                        sessioninfo.LibraryCodeList,
                        out strError);
                            if (nRet == -1)
                                return -1;
                            if (nRet == 0)
                            {
                                strError = "受当前用户的分馆用户身份限制，期记录中不允许新增(包括了超出管辖范围馆代码的)订购XML片断。(refID='" + strRefID + "')";
                                return -1;
                            }
                        }

                        // 在domExit中追加
                        XmlNode new_frag = domExist.CreateElement("root");
                        new_frag.InnerXml = new_node.InnerXml;
                        nodeExistRoot.AppendChild(new_frag);
                        continue;
                    }

                    Debug.Assert(exist_node != null, "");

                    string strTempMergedXml = "";
                    // 将两个订购XML片断合并
                    // parameters:
                    //      strLibraryCodeList  当前用户管辖的分馆代码列表
                    // return:
                    //      -1  出错
                    //      0   正常
                    //      1   发生了超越范围的修改
                    nRet = MergeOrderNode(exist_node,
            new_node,
            sessioninfo.LibraryCodeList,
            out strTempMergedXml,
            out strError);
                    if (nRet != 0)
                    {
                        strError = "对期记录中 refid 为 '" + strRefID + "' 的订购片断数据修改超过权限范围: " + strError;
                        return -1;
                    }
                    exist_node.InnerXml = strTempMergedXml;

                    exists_overwriteable_nodes.Remove(exist_node);  // 已经修改过的已存在节点，从数组中去掉
                }

                // 删除那些在新记录中没有出现的，但当前用户实际上能管辖的节点
                foreach (XmlNode node in exists_overwriteable_nodes)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

        END1:
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

            strMergedXml = domExist.OuterXml;
            return 0;
        }

        // 是否全部订购信息片断中的馆藏地点都在当前用户管辖之下?
        // 那些没有名字的(馆藏地点)分配事项，算在分馆用户的管辖范围以外。没有名字的馆藏地点，在订购时视为地点未定
        // return:
        //      -1  出错
        //      0   不是全部都在管辖范围内
        //      1   都在管辖范围内
        int IsAllOrderControlled(XmlNode nodeOrderInfo,
            string strLibraryCodeList,
            out string strError)
        {
            strError = "";

            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                return 1;

            // 对新提交的记录中的每个订购片断进行循环
            XmlNodeList nodes = nodeOrderInfo.SelectNodes("*");
            foreach (XmlNode node in nodes)
            {
                string strDistribute = DomUtil.GetElementText(node, "distribute");
                if (string.IsNullOrEmpty(strDistribute) == true)
                    continue;

                // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                int nRet = DistributeInControlled(strDistribute,
            strLibraryCodeList,
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 0;
#if NO
                LocationCollcetion locations = new LocationCollcetion();
                int nRet = locations.Build(strDistribute, out strError);
                if (nRet == -1)
                {
                    strError = "馆藏分配字符串 '" + strDistribute + "' 格式不正确";
                    return -1;
                }

                foreach (Location location in locations)
                {
                    if (string.IsNullOrEmpty(location.Name) == true)
                        continue;

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
#endif


            }

            return 1;
        }

        // 将两个订购XML片断合并
        // 当旧的和新的都是全管辖范围内，就允许新的全部替换旧的；否则只允许替换<distribute>元素内容
        // parameters:
        //      strLibraryCodeList  当前用户管辖的分馆代码列表
        //      strMergedXml    [out]范围订购<root>元素的InnerXml
        // return:
        //      -1  出错
        //      0   正常
        //      1   发生了超越范围的修改
        public static int MergeOrderNode(XmlNode exist_node,
            XmlNode new_node,
            string strLibraryCodeList,
            out string strMergedXml,
            out string strError)
        {
            strError = "";
            strMergedXml = "";
            int nRet = 0;

            Debug.Assert(SessionInfo.IsGlobalUser(strLibraryCodeList) == false, "全局用户不应调用函数 MergeOrderNode()");

            string strExistDistribute = DomUtil.GetElementText(exist_node, "distribute");
            string strNewDistribute = DomUtil.GetElementText(new_node, "distribute");

            bool bExistControlled = true;
            bool bNewControlled = true;

            if (string.IsNullOrEmpty(strExistDistribute) == false)
            {
                // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                nRet = DistributeInControlled(strExistDistribute,
            strLibraryCodeList,
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    bExistControlled = false;
            }

            if (string.IsNullOrEmpty(strNewDistribute) == false)
            {
                // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                nRet = DistributeInControlled(strNewDistribute,
            strLibraryCodeList,
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    bNewControlled = false;
            }

            if (bExistControlled == true && bNewControlled == true)
            {
                // 当旧的和新的都是全管辖范围内，就允许新的全部替换旧的
                strMergedXml = new_node.InnerXml;
                return 0;
            }

            string strExistCopy = DomUtil.GetElementText(exist_node, "copy");
            string strExistPrice = DomUtil.GetElementText(exist_node, "price");

            string strChangedCopy = DomUtil.GetElementText(new_node, "copy");
            string strChangedPrice = DomUtil.GetElementText(new_node, "price");

            // 比较两个复本字符串
            {
                ParseOldNewValue(strExistCopy,
            out string strExistOldValue,
            out string strExistNewValue);

                ParseOldNewValue(strChangedCopy,
            out string strChangedOldValue,
            out string strChangedNewValue);

                if (strExistOldValue != strChangedOldValue)
                {
                    strError = "订购套数(方括号左边的部分)不允许修改。(原来='"+strExistCopy+"',新的='"+strChangedCopy+"')";
                    return 1;
                }

                // 检查验收套数的改变，是否正好和distribute字符串内的改变吻合
            }

            // 比较两个价格字符串
            {
                ParseOldNewValue(strExistPrice,
            out string strExistOldValue,
            out string strExistNewValue);

                ParseOldNewValue(strChangedPrice,
            out string strChangedOldValue,
            out string strChangedNewValue);

                // 避免用 == 判断。用 IsEqual 判断，可以把 CNY10.00 和 10.00 视作等同
                if (PriceUtil.IsEqual(strExistOldValue, strChangedOldValue) == false)
                {
                    strError = "订购价(方括号左边的部分)不允许修改。(原来='" + strExistPrice + "',新的='" + strChangedPrice + "')";
                    return 1;
                }
                if (PriceUtil.IsEqual(strExistNewValue, strChangedNewValue) == false)
                {
                    strError = "验收价(方括号中的部分)不允许修改。(原来='" + strExistPrice + "',新的='" + strChangedPrice + "')";
                    return 1;
                }
            }

            LocationCollection new_locations = new LocationCollection();
            nRet = new_locations.Build(strNewDistribute, out strError);
            if (nRet == -1)
            {
                strError = "馆藏分配字符串 '" + strNewDistribute + "' 格式不正确";
                return -1;
            }

            LocationCollection exist_locations = new LocationCollection();
            nRet = exist_locations.Build(strExistDistribute, out strError);
            if (nRet == -1)
            {
                strError = "馆藏分配字符串 '" + strExistDistribute + "' 格式不正确";
                return -1;
            }

            if (exist_locations.Count != new_locations.Count)
            {
                strError = "馆藏分配事项个数发生了改变(原来=" + exist_locations.Count.ToString() + "，新的=" + new_locations.Count.ToString() + ")";
                return 1;
            }

            for (int i = 0; i < exist_locations.Count; i++)
            {
                Location exist_location = exist_locations[i];
                Location new_location = new_locations[i];

                if (exist_location.Name != new_location.Name)
                {
                    // 进一步检查是否馆代码部分改变了

                    // 解析
                    LibraryApplication.ParseCalendarName(exist_location.Name,
                        out string strCode1,
                        out string strPureName);
                    LibraryApplication.ParseCalendarName(new_location.Name,
                        out string strCode2,
                        out strPureName);
                    // 只要馆代码部分不改变即可
                    if (strCode1 != strCode2)
                    {
                        strError = "第 " + (i + 1).ToString() + " 个馆藏分配事项的名字(的馆代码部分)发生改变 (原来='" + exist_location.Name + "',新的='" + new_location.Name + "')";
                        return 1;
                    }
                }

                if (exist_location.RefID != new_location.RefID)
                {

                    // 解析
                    LibraryApplication.ParseCalendarName(exist_location.Name,
                out string strLibraryCode,
                out string strPureName);
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "馆代码 '" + strLibraryCode + "' 不在范围 '" + strLibraryCodeList + "' 内，不允许进行收登操作。";
                        return 1;
                    }
                }
            }

            // 将旧的XML片断装入，只修改里面的三个元素值。这样可以保证三个元素以外的原记录内容不被修改
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(exist_node.OuterXml);
            }
            catch (Exception ex)
            {
                strError = "exist_node.OuterXml装入XMLDOM失败: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(dom.DocumentElement, "copy", strChangedCopy);
            DomUtil.SetElementText(dom.DocumentElement, "price", strChangedPrice);
            DomUtil.SetElementText(dom.DocumentElement, "distribute", strNewDistribute);

            strMergedXml = dom.DocumentElement.InnerXml;
            return 0;
        }

        public static string LinkOldNewValue(string strOldValue,
            string strNewValue)
        {
            if (String.IsNullOrEmpty(strNewValue) == true)
                return strOldValue;

            if (strOldValue == strNewValue)
            {
                if (String.IsNullOrEmpty(strOldValue) == true)  // 新旧均为空
                    return "";

                return strOldValue + "[=]";
            }

            return strOldValue + "[" + strNewValue + "]";
        }


        // 分离 "old[new]" 内的两个值
        public static void ParseOldNewValue(string strValue,
            out string strOldValue,
            out string strNewValue)
        {
            strOldValue = "";
            strNewValue = "";
            int nRet = strValue.IndexOf("[");
            if (nRet == -1)
            {
                strOldValue = strValue;
                strNewValue = "";
                return;
            }

            strOldValue = strValue.Substring(0, nRet).Trim();
            strNewValue = strValue.Substring(nRet + 1).Trim();

            // 去掉末尾的']'
            if (strNewValue.Length > 0 && strNewValue[strNewValue.Length - 1] == ']')
                strNewValue = strNewValue.Substring(0, strNewValue.Length - 1);

            if (strNewValue == "=")
                strNewValue = strOldValue;
        }

        public IssueItemDatabase(LibraryApplication app) : base(app)
        {
        }


#if NO
        public int BuildLocateParam(string strBiblioRecPath,
            string strPublishTime,
            out List<string> locateParam,
            out string strError)
        {
            strError = "";
            locateParam = null;

            int nRet = 0;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strItemDbName = "";

            // 根据书目库名, 找到对应的事项库名
            // return:
            //      -1  出错
            //      0   没有找到(书目库)
            //      1   找到
            nRet = this.GetItemDbName(strBiblioDbName,
                out strItemDbName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                goto ERROR1;
            }

            string strParentID = ResPath.GetRecordId(strBiblioRecPath);

            locateParam = new List<string>();
            locateParam.Add(strItemDbName);
            locateParam.Add(strParentID);
            locateParam.Add(strPublishTime);

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
            string strIssueDbName = "";

            // 根据书目库名, 找到对应的事项库名
            // return:
            //      -1  出错
            //      0   没有找到(书目库)
            //      1   找到
            nRet = this.GetItemDbName(strBiblioDbName,
                out strIssueDbName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strIssueDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                goto ERROR1;
            }
             * */

            locateParam = new List<string>();
            locateParam.Add(strRefID);

            return 0;
            /*
        ERROR1:
            return -1;
             * */
        }

#if NO
        // 派生类必须重载
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

            string strIssueDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strPublishTime = locateParams[2];


            strQueryXml = "<target list='"
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
                string strDbName = this.App.ItemDbs[i].IssueDbName;

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
        // 派生类必须重载
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
            string strIssueDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strPublishTime = locateParams[2];

            strText = "出版日期为 '" + strPublishTime + "'，期库为 '" + strIssueDbName + "'，父记录ID为 '" + strParentID + "'";
            return 0;
        }
#endif


#if NO
        // 派生类必须重载
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
            string strIssueDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strPublishTime = locateParams[2];


            if (String.IsNullOrEmpty(strPublishTime) == false)
            {
                string strExistingPublishTime = DomUtil.GetElementText(domExist.DocumentElement,
                    "publishTime");
                if (strExistingPublishTime != strPublishTime)
                {
                    strError = "期记录中<publishTime>元素中的出版时间 '" + strExistingPublishTime + "' 和通过删除操作参数指定的出版时间 '" + strPublishTime + "' 不一致。";
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

            if (sessioninfo.GlobalUser == false)
            {
                XmlNode nodeExistRoot = domExist.DocumentElement.SelectSingleNode("orderInfo");
                if (nodeExistRoot != null)
                {
                    // 是否全部订购信息片断中的馆藏地点都在当前用户管辖之下?
                    // return:
                    //      -1  出错
                    //      0   不是全部都在管辖范围内
                    //      1   都在管辖范围内
                    int nRet = IsAllOrderControlled(nodeExistRoot,
                        sessioninfo.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "因出现了超越当前用户管辖范围的分馆馆藏信息，删除期记录的操作被拒绝：" + strError;
                        return 0;
                    }
                }
            }
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
            string strIssueDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strPublishTime = locateParams[2];

            if (String.IsNullOrEmpty(strPublishTime) == true)
            {
                strError = "<publishTime>元素中的出版时间为空";
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
                return "期";
            }
        }

        // 事项内部名称。
        public override string ItemNameInternal
        {
            get
            {
                return "Issue";
            }
        }

        public override string DefaultResultsetName
        {
            get
            {
                return "issues";
            }
        }

        // 准备写入日志的SetXXX操作字符串。例如“SetEntity” “SetIssue”
        public override string OperLogSetName
        {
            get
            {
                return "setIssue";
            }
        }

        public override string SetApiName
        {
            get
            {
                return "SetIssues";
            }
        }

        public override string GetApiName
        {
            get
            {
                return "GetIssues";
            }
        }

        // 是否允许创建新记录?
        // TODO: 是否允许在有超过管辖范围的订购信息的情况下依然允许创建，但过滤超出的部分<root>订购片断？如果不允许，则dp2circulation前端要进行改造，当分馆用户创建期记录的时候，不要提交超过自己管辖范围的期记录创建
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

            if (sessioninfo.GlobalUser == false)
            {
                XmlNode nodeExistRoot = domNew.DocumentElement.SelectSingleNode("orderInfo");
                if (nodeExistRoot != null)
                {
                    // 是否全部订购信息片断中的馆藏地点都在当前用户管辖之下?
                    // return:
                    //      -1  出错
                    //      0   不是全部都在管辖范围内
                    //      1   都在管辖范围内
                    int nRet = IsAllOrderControlled(nodeExistRoot,
                        sessioninfo.LibraryCodeList,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "因出现了超越当前用户管辖范围的分馆馆藏信息，创建期记录的操作被拒绝：" + strError;
                        return 0;
                    }
                }
            }

            return 1;
        }

        // 构造出适合保存的新事项记录
        public override int BuildNewItemRecord(
            SessionInfo sessioninfo,
            bool bForce,
            string strBiblioRecId,
            string strOriginXml,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strOriginXml);
            }
            catch (Exception ex)
            {
                strError = "装载strOriginXml到DOM时出错: " + ex.Message;
                return -1;
            }

            // 2010/4/2
            DomUtil.SetElementText(dom.DocumentElement,
                "parent",
                strBiblioRecId);

            // 2017/1/13
            DomUtil.RemoveEmptyElements(dom.DocumentElement);

            strXml = dom.OuterXml;
            return 0;
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
            return this.App.GetIssueDbName(strBiblioDbName,
                out strItemDbName,
                out strError);
        }

        // 2012/4/27
        public override bool IsItemDbName(string strItemDbName)
        {
            return this.App.IsIssueDbName(strItemDbName);
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
                return host.VerifyIssue(strAction,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "VerifyIssue" + "' 时出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            return 0;
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

            string strOldPublishTime = DomUtil.GetElementText(domOldRec.DocumentElement,
                "publishTime");

            string strNewPublishTime = DomUtil.GetElementText(domNewRec.DocumentElement,
                "publishTime");

            string strOldParentID = DomUtil.GetElementText(domOldRec.DocumentElement,
                "parent");

            string strNewParentID = DomUtil.GetElementText(domNewRec.DocumentElement,
                "parent");

            oldLocateParam = new List<string>();
            oldLocateParam.Add(strItemDbName);
            oldLocateParam.Add(strOldParentID);
            oldLocateParam.Add(strOldPublishTime);

            newLocateParam = new List<string>();
            newLocateParam.Add(strItemDbName);
            newLocateParam.Add(strNewParentID);
            newLocateParam.Add(strNewPublishTime);

            if (strOldPublishTime != strNewPublishTime)
                return 1;   // 不相等

            return 0;   // 相等
        }
#endif


#if NO
        public override void LockItem(List<string> locateParam)
        {
            string strIssueDbName = locateParam[0];
            string strParentID = locateParam[1];
            string strPublishTime = locateParam[2];

            this.App.EntityLocks.LockForWrite(
                "issue:" + strIssueDbName + "|" + strParentID + "|" + strPublishTime);
        }

        public override void UnlockItem(List<string> locateParam)
        {
            string strIssueDbName = locateParam[0];
            string strParentID = locateParam[1];
            string strPublishTime = locateParam[2];

            this.App.EntityLocks.UnlockForWrite(
                "issue:" + strIssueDbName + "|" + strParentID + "|" + strPublishTime);
        }
#endif

    }
}
