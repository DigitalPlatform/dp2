using System;
using System.Collections.Generic;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 订购事项数据库
    /// locateParam布局
    /// 1) 订购库名 2)父记录id 3)index
    /// </summary>
    public class OrderItemDatabase : ItemDatabase
    {
        // 要害元素名列表
        static string[] core_order_element_names = new string[] {
                "parent",   // 父记录ID
                "index",    // 编号
                "state",    // 状态
                "catalogNo",    // 书目号 2008/8/31
                "seller",   // 书商
                "source",   // 2008/2/15 经费来源
                "range",    // 订购的时间范围
                "issueCount",   // 订购(时间范围内)跨越多少期? 以便算出总价
                "copy", // 复本数
                "price",    // 册、期单价
                "totalPrice",   // 总价
                "orderTime",    // 订购时间
                "orderID",  // 订单号
                "distribute",   // 馆藏分配
                "class",    // 类别 2008/8/31
                "comment",  // 注释
                "batchNo",  // 批次号
                "sellerAddress",    // 书商地址。用于非大宗订购情形 2009/2/13
                "refID",    // 参考ID 2010/3/15 add
                // "operations", // 2010/4/8
                "fixedPrice",   // 码洋。 2018/7/31
                "discount", // 折扣。形态为 0.80 这样的。 2018/7/31
        };

        // DoOperChange()和DoOperMove()的下级函数
        // 合并新旧记录
        // parmeters:
        //      strMergedXml    合并后产生的 XML
        // return:
        //      -1  出错
        //      0   正确
        //      1   有部分修改没有兑现。说明在strError中
        //      2   全部修改都没有兑现。说明在strError中 (2018/10/9)
        //      3   没有发生任何修改。说明在 strError 中 (2025/2/21)
        public override int MergeTwoItemXml(
            SessionInfo sessioninfo,
            string strAction,
            XmlDocument domExist,
            XmlDocument domNew,
            bool outofrangeAsError,
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
                strError = "订购库记录不允许读者进行修改";
                return -1;
            }

            string origin_xml = domExist.OuterXml;

            /*
            // 2025/2/21
            bool not_changed = false;
            if (AreEqual(domExist, domNew))
                not_changed = true;
            */

            string strWarning = "";

            bool bChangePartDeniedParam = false;

            // 合并记录
            // 目前主要是用于合并 dprms:file 元素。其它元素的合并暂时不考虑
            // parameters:
            //      bChangePartDenied   如果本次被设定为 true，则 strError 中返回了关于部分修改的注释信息
            //      domNew  新记录。
            //      domOld  旧记录。函数执行后其内容会被改变
            // return:
            //      -1  error
            //      0   old record not changed
            //      1   old record changed
            nRet = MergeNewOldRec(
                "order",
                sessioninfo.RightsOrigin,
                domNew,
                domExist,
                ref bChangePartDeniedParam,
                out strError);
            if (nRet == -1)
                return -1;

            if (bChangePartDeniedParam)
                strWarning = strError;

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中

            bool bControlled = true;
            string strControlWarning = ""; // 不完全管辖的详情
            if (sessioninfo.GlobalUser == false)
            {
                string strDistribute = DomUtil.GetElementText(domExist.DocumentElement, "distribute");
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
                    bControlled = false;
                    strControlWarning = strError;
                }

                if (bControlled == true)
                {
                    // 再看新内容是不是也全部在管辖之下
                    strDistribute = DomUtil.GetElementText(domNew.DocumentElement, "distribute");
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
                        bControlled = false;
                        strControlWarning = strError;
                    }
                }
            }

            if (bControlled == true // 控制了全部用到的馆藏地点的情形
    || sessioninfo.GlobalUser == true) // 全局用户
            {
                for (int i = 0; i < core_order_element_names.Length; i++)
                {
                    // 2009/10/23 changed inner-->outer
                    string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                        core_order_element_names[i]);

                    DomUtil.SetElementOuterXml(domExist.DocumentElement,
                        core_order_element_names[i], strTextNew);
                }
            }

            // 2025/4/21
            // 检查提交保存的新记录中是否有超出定义范围的元素，如果有则报错返回
            bool outof_range = false;   // 是否出现了超出定义范围的元素
            {
                // 注: 如果提交的内容中出现 operations 等元素，虽然会拒绝保存这部分元素，但不会特意警告
                List<string> outof_range_elements = LibraryApplication.HasOutOfRangeElements(domNew,
                    GetAllElements().ToArray());
                if (outof_range_elements.Count > 0)
                {
                    if (outofrangeAsError)
                    {
                        strError = $"订购记录中出现了元素 {StringUtil.MakePathList(outof_range_elements)}, 超过定义范围，无法保存 ";
                        return -1;
                    }
                    outof_range = true;
                    // 2025/3/6
                    if (string.IsNullOrEmpty(strWarning) == false)
                        strWarning += "; ";
                    strWarning += $"超出定义范围的元素 {StringUtil.MakePathList(outof_range_elements)} 在保存时已被忽略";
                }
            }


            bool bAllDenied = false;    // 对字段的修改是否全部被拒绝

            // 分馆用户要特意单独处理<distribute>元素
            if (sessioninfo.GlobalUser == false
                && bControlled == false)
            {
                string strRefID = DomUtil.GetElementText(domNew.DocumentElement, "refID");

                // 将两个订购XML片断合并
                // parameters:
                //      strLibraryCodeList  当前用户管辖的分馆代码列表
                // return:
                //      -1  出错
                //      0   正常
                //      1   发生了超越范围的修改
                //      2   有部分修改需求没有兑现
                //      3   全部修改都没有兑现 (2018/10/9)
                nRet = MergeOrderNode(domExist.DocumentElement,
        domNew.DocumentElement,
        sessioninfo.LibraryCodeList,
        out string strTempMergedXml,
        out strError);
                if (nRet == -1)
                {
                    strError = "合并新旧记录时出错: " + strError;
                    return -1;
                }
                if (nRet == 1)
                {
                    // 2018/8/30 也报出不完全管辖的理由
                    strError = $"{SessionInfo.GetCurrentUserName(sessioninfo)}对不完全管辖({strControlWarning})的订购数据修改超过权限范围: {strError}";
                    return -1;
                }
                if (nRet == 2 || nRet == 3)
                {
                    if (nRet == 3)
                        bAllDenied = true;
                    strWarning = strError;
                }

                domExist.DocumentElement.InnerXml = strTempMergedXml;
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
            // 2023/2/9
            // 再把 domNew 中的 dprms:file 元素转移到 domExist 中
            LibraryApplication.MergeDprmsFile(ref domExist, domNew);
            */

            strMergedXml = domExist.OuterXml;

            /*
            if (string.IsNullOrEmpty(strWarning) == false
                && bChangePartDeniedParam == true)
            {
                strError = strWarning;
                if (bAllDenied)
                    return 2;
                return 1;
            }
            */

            bool changed = !AreEqualXml(origin_xml, strMergedXml);

            // 2025/4/20
            // 合法范围元素有改变；超出合法范围的元素也有改变
            if (changed == true && outof_range == true)
            {
                strError = strWarning;
                return 1;
            }

            if (changed == false)
            {
                if (outof_range)
                {
                    strError = "全部修改都没有兑现";
                    if (string.IsNullOrEmpty(strWarning) == false)
                        strError = $"{strError}({strWarning})";
                    return 2;
                }

                {
                    strError = "前端提交的记录内容和数据库中的记录内容没有变化，没有发生任何修改";
                    return 3;   // 没有发生任何修改
                }
            }

            /*
            if (AreEqualXml(origin_xml, strMergedXml))
            {
                strError = "没有发生任何修改";
                return 3;   // 没有发生任何修改
            }
            */

            return 0;

            List<string> GetAllElements()
            {
                string[] other_names = {
                "operations",
        };

                List<string> range = new List<string>(core_order_element_names);
                range.AddRange(other_names);
                return range;
            }
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
        //      2   有部分修改需求没有兑现
        //      3   全部修改都没有兑现 (2018/10/9)
        public int MergeOrderNode(XmlNode exist_node,
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
            string strExistFixedPrice = DomUtil.GetElementText(exist_node, "fixedPrice");
            string strExistDiscount = DomUtil.GetElementText(exist_node, "discount");

            string strChangedCopy = DomUtil.GetElementText(new_node, "copy");
            string strChangedPrice = DomUtil.GetElementText(new_node, "price");
            string strChangedFixedPrice = DomUtil.GetElementText(new_node, "fixedPrice");
            string strChangedDiscount = DomUtil.GetElementText(new_node, "discount");

            // 比较两个复本字符串
            {
                IssueItemDatabase.ParseOldNewValue(strExistCopy,
            out string strExistOldValue,
            out string strExistNewValue);

                IssueItemDatabase.ParseOldNewValue(strChangedCopy,
            out string strChangedOldValue,
            out string strChangedNewValue);

                if (strExistOldValue != strChangedOldValue)
                {
                    strError = "订购套数(方括号左边的部分)不允许修改。(原来='" + strExistCopy + "',新的='" + strChangedCopy + "')";
                    return 1;
                }

                // 检查验收套数的改变，是否正好和distribute字符串内的改变吻合
            }

            // 比较两个价格字符串
            {
                IssueItemDatabase.ParseOldNewValue(strExistPrice,
            out string strExistOldValue,
            out string strExistNewValue);

                IssueItemDatabase.ParseOldNewValue(strChangedPrice,
            out string strChangedOldValue,
            out string strChangedNewValue);

                if (strExistOldValue != strChangedOldValue)
                {
                    strError = "订购价(方括号左边的部分)不允许修改。(原来='" + strExistPrice + "',新的='" + strChangedPrice + "')";
                    return 1;
                }
                if (strExistNewValue != strChangedNewValue)
                {
                    //
                    strError = "验收价(方括号中的部分)不允许修改。(原来='" + strExistPrice + "',新的='" + strChangedPrice + "')";
                    return 1;
                }
            }

            // 2018/7/31
            // 比较两个码洋字符串
            {
                IssueItemDatabase.ParseOldNewValue(strExistFixedPrice,
            out string strExistOldValue,
            out string strExistNewValue);

                IssueItemDatabase.ParseOldNewValue(strChangedFixedPrice,
            out string strChangedOldValue,
            out string strChangedNewValue);

                if (strExistOldValue != strChangedOldValue)
                {
                    strError = "订购码洋(方括号左边的部分)不允许修改。(原来='" + strExistPrice + "', 新的='" + strChangedPrice + "')";
                    return 1;
                }
                if (strExistNewValue != strChangedNewValue)
                {
                    strError = "验收码洋(方括号中的部分)不允许修改。(原来='" + strExistPrice + "', 新的='" + strChangedPrice + "')";
                    return 1;
                }
            }

            // 2018/7/31
            // 比较两个折扣字符串
            {
                IssueItemDatabase.ParseOldNewValue(strExistDiscount,
            out string strExistOldValue,
            out string strExistNewValue);

                IssueItemDatabase.ParseOldNewValue(strChangedDiscount,
            out string strChangedOldValue,
            out string strChangedNewValue);

                if (strExistOldValue != strChangedOldValue)
                {
                    strError = "订购折扣(方括号左边的部分)不允许修改。(原来='" + strExistCopy + "', 新的='" + strChangedCopy + "')";
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

            bool bDistributeChanged = false;
            for (int i = 0; i < exist_locations.Count; i++)
            {
                Location exist_location = exist_locations[i];
                Location new_location = new_locations[i];

                if (exist_location.Name != new_location.Name)
                {
                    // 进一步检查是否馆代码部分改变了
                    string strCode1 = "";
                    string strPureName = "";
                    string strCode2 = "";

                    // 解析
                    LibraryApplication.ParseCalendarName(exist_location.Name,
                        out strCode1,
                        out strPureName);
                    LibraryApplication.ParseCalendarName(new_location.Name,
                        out strCode2,
                        out strPureName);
                    // 只要馆代码部分不改变即可
                    if (strCode1 != strCode2)
                    {
                        strError = "第 " + (i + 1).ToString() + " 个馆藏分配事项的名字(的馆代码部分)发生改变 (原来='" + exist_location.Name + "',新的='" + new_location.Name + "')";
                        return 1;
                    }
                    bDistributeChanged = true;
                }

                if (exist_location.RefID != new_location.RefID)
                {
                    string strLibraryCode = "";
                    string strPureName = "";

                    // 解析
                    LibraryApplication.ParseCalendarName(exist_location.Name,
                out strLibraryCode,
                out strPureName);
                    if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == false)
                    {
                        strError = "馆代码 '" + strLibraryCode + "' 不在范围 '" + strLibraryCodeList + "' 内，不允许进行收登操作。";
                        return 1;
                    }

                    bDistributeChanged = true;
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
                strError = "exist_node.OuterXml 装入 XMLDOM 失败: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(dom.DocumentElement, "copy", strChangedCopy);
            DomUtil.SetElementText(dom.DocumentElement, "price", strChangedPrice);
            DomUtil.SetElementText(dom.DocumentElement, "distribute", strNewDistribute);

            strMergedXml = dom.DocumentElement.InnerXml;

            List<string> skips = new List<string>();
            List<string> differents = null;
            skips.Add("distribute");
            skips.Add("operations");
            // parameters:
            //      skips   要跳过的、不参与比较的元素名
            // return:
            //      0   没有差异
            //      1   有差异。differents数组里面返回了有差异的元素名
            nRet = IsItemInfoChanged(new_node,
                dom.DocumentElement,
                skips,
                out differents);
            if (nRet == 1)
            {
                strError = "对下列元素的修改没有兑现: " + StringUtil.MakePathList(differents);
                if (bDistributeChanged)
                    return 2;   // 部分修改兑现
                return 3;   // 所有修改都没有兑现
            }
            if (nRet == 0 && bDistributeChanged == false)
            {
                // 没有任何修改发生
            }

            return 0;
        }

        // parameters:
        //      skips   要跳过的、不参与比较的元素名
        // return:
        //      0   没有差异
        //      1   有差异。differents数组里面返回了有差异的元素名
        public int IsItemInfoChanged(XmlNode new_root,
            XmlNode oldrec_root,
            List<string> skips,
            out List<string> differents)
        {
            differents = new List<string>();

            for (int i = 0; i < core_order_element_names.Length; i++)
            {
                string strElementName = core_order_element_names[i];
                if (skips.IndexOf(strElementName) != -1)
                    continue;

                if (DomUtil.IsEmptyElement(new_root, strElementName) == true
                    && DomUtil.IsEmptyElement(oldrec_root, strElementName) == true)
                    continue;

                string strText1 = DomUtil.GetElementOuterXml(new_root,
                    strElementName);
                string strText2 = DomUtil.GetElementOuterXml(oldrec_root,
                    strElementName);

                if (strText1 != strText2)
                {
                    differents.Add(strText2 + "-->" + strText1);
                }
            }

            if (differents.Count > 0)
                return 1;
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
            for (int i = 0; i < core_order_element_names.Length; i++)
            {
                // 2009/10/24 changed
                string strText1 = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                    core_order_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(domOldRec.DocumentElement,
                    core_order_element_names[i]);

                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }

        public OrderItemDatabase(LibraryApplication app)
            : base(app)
        {

        }

#if NO
        public int BuildLocateParam(string strBiblioRecPath,
            string strIndex,
            out List<string> locateParam,
            out string strError)
        {
            strError = "";
            locateParam = null;

            int nRet = 0;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strOrderDbName = "";

            // 根据书目库名, 找到对应的事项库名
            // return:
            //      -1  出错
            //      0   没有找到(书目库)
            //      1   找到
            nRet = this.GetItemDbName(strBiblioDbName,
                out strOrderDbName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strOrderDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                goto ERROR1;
            }

            string strParentID = ResPath.GetRecordId(strBiblioRecPath);


            locateParam = new List<string>();
            locateParam.Add(strOrderDbName);
            locateParam.Add(strParentID);
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
            string strOrderDbName = "";

            // 根据书目库名, 找到对应的事项库名
            // return:
            //      -1  出错
            //      0   没有找到(书目库)
            //      1   找到
            nRet = this.GetItemDbName(strBiblioDbName,
                out strOrderDbName,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
            {
                strError = "书目库 '" + strBiblioDbName + "' 没有找到";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strOrderDbName) == true)
            {
                strError = "书目库名 '" + strBiblioDbName + "' 对应的" + this.ItemName + "库名没有定义";
                goto ERROR1;
            }
            */

            locateParam = new List<string>();
            locateParam.Add(strRefID);

            return 0;
            /*
        ERROR1:
            return -1;
             * */
        }

#if NO
        // 构造用于获取事项记录的XML检索式
        public override int MakeGetItemRecXmlSearchQuery(
            List<string> locateParams,
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

            string strOrderDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strIndex = locateParams[2];

            strQueryXml = "<target list='"
        + StringUtil.GetXmlStringSimple(strOrderDbName + ":" + "编号")
        + "'><item><word>"
        + StringUtil.GetXmlStringSimple(strIndex)
        + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            strQueryXml += "<operator value='AND'/>";


            strQueryXml += "<target list='"
                    + StringUtil.GetXmlStringSimple(strOrderDbName + ":" + "父记录")
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
                string strDbName = this.App.ItemDbs[i].OrderDbName;

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
            string strOrderDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strIndex = locateParams[2];

            strText = "订购库为 '" + strOrderDbName + "'，父记录ID为 '" + strParentID + "' 编号为 '" + strIndex + "'";
            return 0;
        }
#endif

#if NO1
        // 构造定位提示信息。用于报错。
        public override int GetLocateText(
            List<string> locateParams,
            out string strText,
            out string strError)
        {
            strText = "";
            strError = "";

            // 将数组形态的参数还原
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }
            string strRefID = locateParams[0];

            strText = "参考ID为 '" + strRefID + "'";
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
            string strOrderDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strIndex = locateParams[2];

            if (String.IsNullOrEmpty(strIndex) == false)
            {
                string strExistingIndex = DomUtil.GetElementText(domExist.DocumentElement,
                    "index");
                if (strExistingIndex != strIndex)
                {
                    strError = "订购记录中<index>元素中的编号 '" + strExistingIndex + "' 和通过删除操作参数指定的编号 '" + strIndex + "' 不一致。";
                    return 1;
                }
            }

            return 0;
        }
#endif

#if NO1
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
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }
            string strRefID = locateParams[0];

            if (String.IsNullOrEmpty(strRefID) == false)
            {
                string strExistingRefID = DomUtil.GetElementText(domExist.DocumentElement,
                    "refID");
                if (strExistingRefID != strRefID)
                {
                    strError = "订购记录中<refID>元素中的参考ID '" + strExistingRefID + "' 和通过删除操作参数指定的参考ID '" + strRefID + "' 不一致。";
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

        // 2012/9/29
        // 是否允许对旧记录进行修改(或者移动)? 
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

            if (strAction == "move")
            {
                if (sessioninfo.UserType == "reader")
                {
                    strError = "读者身份的用户不能移动订购记录";
                    return 0;
                }

                if (sessioninfo.GlobalUser == false)
                {
                    // 再看已经存在的内容是不是全部在管辖之下
                    string strDistribute = DomUtil.GetElementText(domExist.DocumentElement, "distribute");
                    // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                    // return:
                    //      -1  出错
                    //      0   超过管辖范围。strError中有解释
                    //      1   在管辖范围内
                    int nRet = DistributeInControlled(strDistribute,
                sessioninfo.LibraryCodeList,
                out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = $"因原记录中出现了超越{SessionInfo.GetCurrentUserName(sessioninfo)}管辖范围的分馆馆藏信息，移动订购记录的操作被拒绝：{strError}";
                        return 0;
                    }
                }

            }

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

            if (sessioninfo.GlobalUser == false)
            {
                // 再看已经存在的内容是不是全部在管辖之下
                string strDistribute = DomUtil.GetElementText(domExist.DocumentElement, "distribute");
                // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                int nRet = DistributeInControlled(strDistribute,
            sessioninfo.LibraryCodeList,
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = $"因出现了超越{SessionInfo.GetCurrentUserName(sessioninfo)}管辖范围的分馆馆藏信息，删除订购记录的操作被拒绝：{strError}";
                    return 0;
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
            string strOrderDbName = locateParams[0];
            string strParentID = locateParams[1];
            string strIndex = locateParams[2];

            if (String.IsNullOrEmpty(strIndex) == true)
            {
                strError = "<index>元素中的编号为空";
                return 1;
            }

            return 0;
        }
#endif

#if NO1
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
            if (locateParams.Count != 1)
            {
                strError = "locateParams数组内的元素必须为1个";
                return -1;
            }
            string strRefID = locateParams[0];


            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strError = "参考ID 为空";
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
                return "订购";
            }
        }

        // 事项名称。
        public override string ItemNameInternal
        {
            get
            {
                return "Order";
            }
        }

        public override string DefaultResultsetName
        {
            get
            {
                return "orders";
            }
        }

        // 准备写入日志的SetXXX操作字符串。例如“SetEntity” “SetIssue”
        public override string OperLogSetName
        {
            get
            {
                return "setOrder";
            }
        }

        public override string SetApiName
        {
            get
            {
                return "SetOrders";
            }
        }

        public override string GetApiName
        {
            get
            {
                return "GetOrders";
            }
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

            if (sessioninfo.GlobalUser == false)
            {
                // 再看新内容是不是全部在管辖之下
                string strDistribute = DomUtil.GetElementText(domNew.DocumentElement, "distribute");
                // 观察一个馆藏分配字符串，看看是否在当前用户管辖范围内
                // return:
                //      -1  出错
                //      0   超过管辖范围。strError中有解释
                //      1   在管辖范围内
                int nRet = DistributeInControlled(strDistribute,
            sessioninfo.LibraryCodeList,
            out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = $"因出现了超越{SessionInfo.GetCurrentUserName(sessioninfo)}管辖范围的分馆馆藏信息，创建订购记录的操作被拒绝：{strError}";
                    return 0;
                }
            }

            return 1;
        }

#if REMOVED
        // 构造出适合保存的新事项记录
        // return:
        //      -1  出错
        //      0   正确
        //      1   有部分修改没有兑现。说明在strError中
        //      2   全部修改都没有兑现。说明在strError中
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
                strError = "装载 strOriginXml 到 DOM时出错: " + ex.Message;
                return -1;
            }

            // 如果 ID 为非空，才会主动修改/写入 parent 元素
            if (string.IsNullOrEmpty(strBiblioRecId) == false)
            {
                // 2010/4/2
                DomUtil.SetElementText(dom.DocumentElement,
                    "parent",
                    strBiblioRecId);
            }

            // 2017/1/13
            DomUtil.RemoveEmptyElements(dom.DocumentElement);

            strXml = dom.OuterXml;
            return 0;
        }
#endif

        // 获得事项数据库名
        // return:
        //      -1  error
        //      0   没有找到(书目库)
        //      1   found
        public override int GetItemDbName(string strBiblioDbName,
            out string strItemDbName,
            out string strError)
        {
            return this.App.GetOrderDbName(strBiblioDbName,
                out strItemDbName,
                out strError);
        }

        // 2012/4/27
        public override bool IsItemDbName(string strItemDbName)
        {
            return this.App.IsOrderDbName(strItemDbName);
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
                return host.VerifyOrder(strAction,
                    itemdom,
                    out strError);
            }
            catch (Exception ex)
            {
                strError = "执行脚本函数 '" + "VerifyOrder" + "' 时出错：" + ExceptionUtil.GetDebugText(ex);
                return -1;
            }

            // return 0;
        }

        // 按照当前账户的权限，来过滤掉原始事项记录中的一些敏感字段
        // 重载的函数建议先调用本基类函数，然后再实现差异化逻辑
        // return:
        //      -2  权限不具备。也可以理解为全部字段都应被过滤掉
        //      -1  出错
        //      0   过滤后记录没有改变
        //      1   过滤后记录发生了改变
        public override int FilterItemRecord(
    SessionInfo sessioninfo,
    XmlDocument item_dom,
    string item_recpath,    // 2025/5/5
    out string strError)
        {
            strError = "";

            bool changed = false;
            // return:
            //      -2  权限不具备。也可以理解为全部字段都应被过滤掉
            //      -1  出错
            //      0   过滤后记录没有改变
            //      1   过滤后记录发生了改变
            int nRet = base.FilterItemRecord(sessioninfo,
                item_dom,
                item_recpath,
                out strError);
            if (nRet == -2 || nRet == -1)
                return nRet;
            if (nRet == 1)
                changed = true;

            if (changed == true)
                return 1;
            return 0;
        }

#if NO1
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

#if NO
            string strOldIndex = DomUtil.GetElementText(domOldRec.DocumentElement,
                "index");

            string strNewIndex = DomUtil.GetElementText(domNewRec.DocumentElement,
                "index");


            string strOldParentID = DomUtil.GetElementText(domOldRec.DocumentElement,
                "parent");

            string strNewParentID = DomUtil.GetElementText(domNewRec.DocumentElement,
                "parent");

            oldLocateParam = new List<string>();
            oldLocateParam.Add(strItemDbName);
            oldLocateParam.Add(strOldParentID);
            oldLocateParam.Add(strOldIndex);

            newLocateParam = new List<string>();
            newLocateParam.Add(strItemDbName);
            newLocateParam.Add(strNewParentID);
            newLocateParam.Add(strNewIndex);

            if (strOldIndex != strNewIndex)
                return 1;   // 不相等

            return 0;   // 相等。
#endif
            // 2012/4/1 改造
            string strOldRefID = DomUtil.GetElementText(domOldRec.DocumentElement,
                "refID");

            string strNewRefID = DomUtil.GetElementText(domNewRec.DocumentElement,
                "refID");

            oldLocateParam = new List<string>();
            oldLocateParam.Add(strOldRefID);

            newLocateParam = new List<string>();
            newLocateParam.Add(strNewRefID);

            if (strOldRefID != strNewRefID)
                return 1;   // 不相等

            return 0;   // 相等。
        }
#endif

#if NO
        public override void LockItem(List<string> locateParam)
        {
            string strItemDbName = locateParam[0];
            string strParentID = locateParam[1];
            string strIndex = locateParam[2];

            this.App.EntityLocks.LockForWrite(
                "order:" + strItemDbName + "|" + strParentID + "|" + strIndex);
        }

        public override void UnlockItem(List<string> locateParam)
        {
            string strItemDbName = locateParam[0];
            string strParentID = locateParam[1];
            string strIndex = locateParam[2];

            this.App.EntityLocks.UnlockForWrite(
                "order:" + strItemDbName + "|" + strParentID + "|" + strIndex);
        }
#endif

#if NO1
        public override void LockItem(List<string> locateParam)
        {
            string strRefID = locateParam[0];

            this.App.EntityLocks.LockForWrite(
                "order:" + strRefID);
        }

        public override void UnlockItem(List<string> locateParam)
        {
            string strRefID = locateParam[0];

            this.App.EntityLocks.UnlockForWrite(
                "order:" + strRefID);
        }
#endif
    }
}
