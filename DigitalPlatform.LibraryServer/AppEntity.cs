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
using DigitalPlatform.rms.Client.rmsws_localhost;


namespace DigitalPlatform.LibraryServer
{
    /// <summary>
    /// 本部分是典藏业务(册)相关的代码
    /// </summary>
    public partial class LibraryApplication
    {
        // 要害元素名列表
        static string[] core_entity_element_names = new string[] {
                "parent",
                "barcode",
                "state",
                "publishTime",   // 2007/10/24 
                "location",
                "seller",   // 2007/10/24 
                "source",   // 2008/2/15  经费来源
                "price",
                "bookType",
                "registerNo",
                "comment",
                "mergeComment",
                "batchNo",
                "volume",    // 2007/10/19 
                "refID",    // 2008/4/16 
                "accessNo", // 2008/12/12 
                "intact",   // 2009/10/11 
                "binding",  // 2009/10/11 
                "operations", // 2009/10/24 
                "bindingCost",  // 2012/6/1 装订费
                "biblio",   //  2016/12/8
                "oldRefID", // 2016/12/19
                "shelfNo", // 2017/6/15 架号。例如 10-1 表示第十个架的第一排
                "currentLocation",  // 2019/7/27 新增
                "uid",  // 2020/8/23 RFID 标签的 UID
            };

        static string[] transfer_entity_element_names = new string[] {
                "location",
                "shelfNo", // 架号。例如 10-1 表示第十个架的第一排
                "accessNo",
                "currentLocation",  // 2019/7/27 新增
                "batchNo",
                "operations",
            };

        // 2020/10/12
        static string[] setuid_entity_element_names = new string[] {
                "uid",
                "batchNo",
                "operations",
            };

        // 用于借书还书的工作元素
        static string[] checkinout_element_names = new string[] {
                "borrower",
                "borrowPeriod",
                "borrowDate",
                "returningDate",
                "denyPeriod",
                "checkInOutDate",   // 2020/9/18 新增
                "borrowHistory",
                "operations",
                "libraryCode",
                "oi",
                "http://dp2003.com/dprms:file",
            };

        static string GetNamespaceName(XmlElement element)
        {
            if (string.IsNullOrEmpty(element.NamespaceURI))
                return element.LocalName;
            return element.NamespaceURI + ":" + element.LocalName;
        }

        // (核心元素名以外)一些和借还业务有关的元素名，和其他元素名
        static string[] other_names = {
                "borrower",
                "borrowerReaderType",
                "borrowerRecPath",
                "borrowDate",
                "borrowPeriod",
                "returningDate",
                "lastReturningDate",
                "operator",
                "no",
                "renewComment",
                "borrowID"};

        // 2020/9/17
        // 检查新记录中是否有超出定义范围的元素
        int CheckOutofRangeElements(XmlDocument domNew,
            out string strError)
        {
            strError = "";
            List<string> range = new List<string>(core_entity_element_names);
            range.AddRange(other_names);
            if (this.ItemAdditionalFields != null && this.ItemAdditionalFields.Count > 0)
                range.AddRange(this.ItemAdditionalFields);
            range.AddRange(checkinout_element_names);
            var out_of = HasOutOfRangeElements(domNew, range);
            if (out_of.Count > 0)
            {
                strError = $"册记录中出现了元素 {StringUtil.MakePathList(out_of)}, 超过定义范围，无法保存 ";
                return -1;
            }

            return 0;
        }

        static List<string> HasOutOfRangeElements(XmlDocument dom,
                    List<string> element_names)
        {
            List<string> out_of = new List<string>();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*", nsmgr);
            foreach (XmlElement element in nodes)
            {
                var name = GetNamespaceName(element);
                int index = element_names.IndexOf(name);
                if (index == -1)
                    out_of.Add(name);
            }

            return out_of;
        }

        // TODO: 需要单元测试一下
        // 合并新旧记录的 currentLocation 元素内容
        static int MergeCurrentLocation(XmlDocument domExist,
            XmlDocument domNew,
            out string strError)
        {
            strError = "";

            string oldValue = DomUtil.GetElementText(domExist.DocumentElement, "currentLocation");
            string newValue = DomUtil.GetElementText(domNew.DocumentElement, "currentLocation");

            // 没有改变过
            if (oldValue == newValue)
                return 0;

            // 若新值不包含 '*' 字符
            if (newValue == null && newValue.Contains("*") == false)
                return 0;

            var parts = StringUtil.ParseTwoPart(newValue, ":");
            string new_left = parts[0];
            string new_right = parts[1];

            parts = StringUtil.ParseTwoPart(oldValue, ":");
            string old_left = parts[0];
            string old_right = parts[1];

            bool changed = false;
            if (new_left == "*")
            {
                new_left = old_left;
                changed = true;
            }
            if (new_right == "*")
            {
                new_right = old_right;
                changed = true;
            }

            // 没有发生替换
            if (changed == false)
                return 0;

            // 合成
            if (string.IsNullOrEmpty(new_right))
                newValue = new_left;
            else
                newValue = new_left + ":" + new_right;
            DomUtil.SetElementText(domNew.DocumentElement,
                "currentLocation",
                newValue);
            return 1;
        }

        // <DoEntityOperChange()的下级函数>
        // 合并新旧记录
        // parameters:
        //      element_names   要害元素名列表。如果为 null，表示会用到 core_entity_element_names
        //      check_outof_elements    是否检查并报错超范围的 XML 元素。
        int MergeTwoEntityXml(XmlDocument domExist,
            XmlDocument domNew,
            string[] element_names,
            bool check_outof_elements,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            if (element_names == null)
            {
                // 2020/9/7
                if (this.ItemAdditionalFields != null && this.ItemAdditionalFields.Count > 0)
                    element_names = StringUtil.Append(core_entity_element_names, this.ItemAdditionalFields.ToArray());
                else
                    element_names = core_entity_element_names;
            }

            // 2020/9/17
            // 检查提交保存的新记录中是否有超出定义范围的元素，如果有则报错返回
            if (check_outof_elements)
            {
                int nRet = CheckOutofRangeElements(domNew, out strError);
                if (nRet == -1)
                    return -1;
            }

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中
            foreach (string name in element_names)
            {
                /*
                string strTextNew = DomUtil.GetElementText(domNew.DocumentElement,
                    core_entity_element_names[i]);

                DomUtil.SetElementText(domExist.DocumentElement,
                    core_entity_element_names[i], strTextNew);
                 * */
                // 2016/12/8
                {
                    XmlElement node_new = domNew.DocumentElement.SelectSingleNode(name) as XmlElement;
                    if (node_new != null)
                    {
                        // 看看 dprms:missing 属性是否存在
                        if (node_new.GetAttributeNode("missing", DpNs.dprms) != null)
                            continue;
                    }
                }

                string strTextNew = DomUtil.GetElementOuterXml(domNew.DocumentElement,
                    name);

                DomUtil.SetElementOuterXml(domExist.DocumentElement,
                    name, strTextNew);
            }

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

        // <DoEntityOperChange()的下级函数>
        // 比较两个记录, 看看和册登录有关的字段是否发生了变化
        // return:
        //      0   没有变化
        //      1   有变化
        static int IsRegisterInfoChanged(XmlDocument dom1,
            XmlDocument dom2,
            string[] element_names)
        {
            if (element_names == null)
                element_names = core_entity_element_names;
            for (int i = 0; i < element_names.Length; i++)
            {
                /*
                string strText1 = DomUtil.GetElementText(dom1.DocumentElement,
                    core_entity_element_names[i]);
                string strText2 = DomUtil.GetElementText(dom2.DocumentElement,
                    core_entity_element_names[i]);
                 * */
                // 2009/10/24 changed 因为<operator>元素内可能有内嵌的XML代码
                string strText1 = DomUtil.GetElementOuterXml(dom1.DocumentElement,
                    core_entity_element_names[i]);
                string strText2 = DomUtil.GetElementOuterXml(dom2.DocumentElement,
                    core_entity_element_names[i]);

                if (strText1 != strText2)
                    return 1;
            }

            return 0;
        }


        // 状态字符串中是否包含“加工中”？
        public static bool IncludeStateProcessing(string strStateString)
        {
            if (StringUtil.IsInList("加工中", strStateString) == true)
                return true;
            return false;
        }

        // 如果返回值不是0，就中断循环并返回
        public delegate int Delegate_checkRecord(
            int index,
            string strRecPath,
            XmlDocument dom,
            byte[] baTimestamp,
            object param,
            out string strError);

        // 2016/11/15 改造为不用 GetRes() 获取记录
        // 检索书目记录下属的实体记录，返回少量必要的信息，可以提供后面实做删除时使用
        // parameters:
        //      strStyle    check_borrow_info,count_borrow_info,return_record_xml
        //                  当包含 check_borrow_info 时，发现第一个流通信息，本函数就立即返回-1
        //                  当包含 count_borrow_info 时，函数要统计全部流通信息的个数
        //                  当包含 libraryCodes: 时，表示仅获得所列分馆代码的册记录。注意多个馆代码之间用竖线分隔
        //                  当包含 limit: 时，定义最多取得记录的个数。例如希望最多取得 10 条，可以定义 limit:10
        // return:
        //      -2  not exist entity dbname
        //      -1  error
        //      >=0 含有流通信息的实体记录个数, 当strStyle包含count_borrow_info时。
        public int SearchChildEntities(RmsChannel channel,
            string strBiblioRecPath,
            string strStyle,
            Delegate_checkRecord procCheckRecord,
            object param,
            out long lHitCount,
            out List<DeleteEntityInfo> entityinfos,
            out string strError)
        {
            strError = "";
            lHitCount = 0;
            entityinfos = new List<DeleteEntityInfo>();

            int nRet = 0;

            // TODO: bReturnRecordXml 为 false 的时候，还有必要在获取浏览记录阶段 style 里面包含 'xml' 么？

            bool bCheckBorrowInfo = StringUtil.IsInList("check_borrow_info", strStyle);
            bool bCountBorrowInfo = StringUtil.IsInList("count_borrow_info", strStyle);
            bool bReturnRecordXml = StringUtil.IsInList("return_record_xml", strStyle);
            bool bOnlyGetCount = StringUtil.IsInList("only_getcount", strStyle);

            if (bCheckBorrowInfo == true
                && bCountBorrowInfo == true)
            {
                strError = "strStyle中check_borrow_info和count_borrow_info不能同时具备";
                return -1;
            }

            string strLibraryCodeParam = StringUtil.GetParameterByPrefix(strStyle, "libraryCodes", ":");
            string strLimit = StringUtil.GetParameterByPrefix(strStyle, "limit", ":");

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的实体库名
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            // 2008/12/5 
            if (String.IsNullOrEmpty(strItemDbName) == true)
                return 0;

            // 检索实体库中全部从属于特定id的记录
            string strQueryXml = "";
            if (string.IsNullOrEmpty(strLibraryCodeParam) == true)
            {
                strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录")       // 2007/9/14 
                    + "'><item><word>"
                    + strBiblioRecId
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
            }
            else
            {
                // 仅仅取得当前用户管辖的分馆的册记录
                List<string> codes = StringUtil.SplitList(strLibraryCodeParam, '|'); // sessioninfo.LibraryCodeList
                foreach (string strCode in codes)
                {
                    string strOneQueryXml = "<target list='"
         + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录+馆藏地点")
         + "'><item><word>"
         + StringUtil.GetXmlStringSimple(strBiblioRecId + "|" + strCode + "/")
         + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
                    if (string.IsNullOrEmpty(strQueryXml) == false)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneQueryXml;
                }
                if (codes.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }
            }

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                strError = "没有找到属于书目记录 '" + strBiblioRecPath + "' 的任何实体记录";
                return 0;
            }

            lHitCount = lRet;

            // 仅返回命中条数
            if (bOnlyGetCount == true)
                return 0;

            int nResultCount = (int)lRet;

            if (nResultCount > 10000)
            {
                strError = "命中册记录数 " + nResultCount.ToString() + " 超过 10000, 暂时不支持针对它们的删除操作";
                goto ERROR1;
            }

            string strColumnStyle = "id,xml,timestamp";

            int nLimit = -1;
            if (string.IsNullOrEmpty(strLimit) == false)
                Int32.TryParse(strLimit, out nLimit);

            int nBorrowInfoCount = 0;

            int nStart = 0;
            int nPerCount = 100;

            if (nLimit != -1 && nPerCount > nLimit)
                nPerCount = nLimit;
            for (; ; )
            {
#if NO
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }
#endif
                Record[] searchresults = null;
                lRet = channel.DoGetSearchResult(
    "entities",
    nStart,
    nPerCount,
    strColumnStyle,
    "zh",
    null,
    out searchresults,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    goto ERROR1;
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    goto ERROR1;
                }


                // 获得每条记录
                // for (int i = 0; i < aPath.Count; i++)
                int i = 0;
                foreach (Record record in searchresults)
                {
                    // EntityInfo info = new EntityInfo();
                    // info.OldRecPath = record.Path;

                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    DeleteEntityInfo entityinfo = new DeleteEntityInfo();

                    if (record.RecordBody == null || string.IsNullOrEmpty(record.RecordBody.Xml) == true)
                    {
                        lRet = channel.GetRes(record.Path,
                            out strXml,
                            out strMetaData,
                            out timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                continue;

                            strError = "获取实体记录 '" + record.Path + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }

                    }
                    else
                    {
                        strXml = record.RecordBody.Xml;
                        strOutputPath = record.Path;
                        timestamp = record.RecordBody.Timestamp;
                    }

                    entityinfo.RecPath = strOutputPath;
                    entityinfo.OldTimestamp = timestamp;
                    if (bReturnRecordXml == true)
                        entityinfo.OldRecord = strXml;

                    if (bCheckBorrowInfo == true
                        || bCountBorrowInfo == true
                        || procCheckRecord != null)
                    {
                        // 检查是否有借阅信息
                        // 把记录装入DOM
                        XmlDocument domExist = new XmlDocument();

                        try
                        {
                            domExist.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "实体记录 '" + strOutputPath + "' 装载进入DOM时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        if (procCheckRecord != null)
                        {
                            nRet = procCheckRecord(
                                nStart + i,
                                strOutputPath,
                                domExist,
                                timestamp,
                                param,
                                out strError);
                            if (nRet != 0)
                                return nRet;
                        }

                        entityinfo.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                            "barcode");

                        // TODO: 在日志恢复阶段调用本函数时，是否还有必要检查是否具有流通信息？似乎这时应强制删除为好

                        // 观察已经存在的记录是否有流通信息
                        string strDetail = "";
                        bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist, out strDetail);

                        if (bHasCirculationInfo == true)
                        {
                            if (bCheckBorrowInfo == true)
                            {
                                strError = "拟删除的册记录 '" + entityinfo.RecPath + "' 中包含有流通信息(" + strDetail + ")(此种情况可能不限于这一条)，不能删除。因此全部删除操作均被放弃。";
                                goto ERROR1;
                            }
                            if (bCountBorrowInfo == true)
                                nBorrowInfoCount++;
                        }
                    }

                    // CONTINUE:
                    entityinfos.Add(entityinfo);

                    i++;
                }

                nStart += searchresults.Length;
                if (nStart >= nResultCount)
                    break;
                if (nLimit != -1 && nStart >= nLimit)
                    break;
            }

            return nBorrowInfoCount;
        ERROR1:
            return -1;
        }

#if NO
        // 获得书目记录下属的实体记录，返回少量必要的信息，可以提供后面实做删除时使用
        // parameters:
        //      strStyle    return_record_xml
        // return:
        //      -1  error
        //      0   succeed
        public int SearchChildRecords(RmsChannel channel,
            List<string> aPath,
            string strStyle,
            out List<DeleteEntityInfo> entityinfos,
            out string strError)
        {
            strError = "";
            entityinfos = new List<DeleteEntityInfo>();

            int nRet = 0;

            bool bReturnRecordXml = StringUtil.IsInList("return_record_xml", strStyle);

            // 获得每条记录
            for (int i = 0; i < aPath.Count; i++)
            {
                string strMetaData = "";
                string strXml = "";
                byte[] timestamp = null;
                string strOutputPath = "";

                long lRet = channel.GetRes(aPath[i],
                    out strXml,
                    out strMetaData,
                    out timestamp,
                    out strOutputPath,
                    out strError);
                DeleteEntityInfo entityinfo = new DeleteEntityInfo();

                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        continue;   // 是否报错?

                    strError = "获取下级记录 '" + aPath[i] + "' 时发生错误: " + strError;
                    goto ERROR1;
                    // goto CONTINUE;
                }

                entityinfo.RecPath = strOutputPath;
                entityinfo.OldTimestamp = timestamp;
                if (bReturnRecordXml == true)
                    entityinfo.OldRecord = strXml;
#if NO
                    if (bCheckBorrowInfo == true
                        || bCountBorrowInfo == true)
                    {
                        // 检查是否有借阅信息
                        // 把记录装入DOM
                        XmlDocument domExist = new XmlDocument();

                        try
                        {
                            domExist.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "实体记录 '" + aPath[i] + "' 装载进入DOM时发生错误: " + ex.Message;
                            goto ERROR1;
                        }

                        entityinfo.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                            "barcode");

                        // TODO: 在日志恢复阶段调用本函数时，是否还有必要检查是否具有流通信息？似乎这时应强制删除为好

                        // 观察已经存在的记录是否有流通信息
                        string strDetail = "";
                        bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist, out strDetail);


                        if (bHasCirculationInfo == true)
                        {
                            if (bCheckBorrowInfo == true)
                            {
                                strError = "拟删除的册记录 '" + entityinfo.RecPath + "' 中包含有流通信息(" + strDetail + ")(此种情况可能不限于这一条)，不能删除。因此全部删除操作均被放弃。";
                                goto ERROR1;
                            }
                            if (bCountBorrowInfo == true)
                                nBorrowInfoCount++;
                        }
                    }
#endif

                // CONTINUE:
                entityinfos.Add(entityinfo);
            }

            return 0;
        ERROR1:
            return -1;
        }
#endif

        // return:
        //      -2  目标实体库不存在
        //      -1  出错
        //      0   存在
        public int DetectTargetChildDbExistence(
            string strDbType,
            string strTargetBiblioDbName,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            // 获得目标书目库下属的实体库名
            string strTargetItemDbName = "";
            // string strTargetBiblioDbName = ResPath.GetDbName(strTargetBiblioRecPath);

            if (strDbType == "item")
            {
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = this.GetItemDbName(strTargetBiblioDbName,
                    out strTargetItemDbName,
                    out strError);
            }
            else if (strDbType == "order")
            {
                nRet = this.GetOrderDbName(strTargetBiblioDbName,
                    out strTargetItemDbName,
                    out strError);
            }
            else if (strDbType == "issue")
            {
                nRet = this.GetIssueDbName(strTargetBiblioDbName,
                    out strTargetItemDbName,
                    out strError);
            }
            else if (strDbType == "comment")
            {
                nRet = this.GetCommentDbName(strTargetBiblioDbName,
                    out strTargetItemDbName,
                    out strError);
            }
            else
            {
                strError = "无法识别的数据库类型 '" + strDbType + "'";
                return -1;
            }

            if (nRet == 0 || string.IsNullOrEmpty(strTargetItemDbName) == true)
            {
                return -2;   // 目标实体库不存在
            }

            return 0;
        }

        // 复制属于同一书目记录的全部实体记录
        // TODO: 返回记录路径变迁信息
        // parameters:
        //      strAction   copy / move
        // return:
        //      -2  目标实体库不存在，无法进行复制或者删除
        //      -1  error
        //      >=0  实际复制或者移动的实体记录数
        public int CopyBiblioChildEntities(RmsChannel channel,
            string strAction,
            List<DeleteEntityInfo> entityinfos,
            string strTargetBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nOperCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement(strAction == "copy" ? "copyEntityRecords" : "moveEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }

            // 获得目标书目库下属的实体库名
            string strTargetItemDbName = "";
            string strTargetBiblioDbName = ResPath.GetDbName(strTargetBiblioRecPath);
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            int nRet = this.GetItemDbName(strTargetBiblioDbName,
                out strTargetItemDbName,
                out strError);
            if (nRet == 0 || string.IsNullOrEmpty(strTargetItemDbName) == true)
            {
                return -2;   // 目标实体库不存在
            }

            string strParentID = ResPath.GetRecordId(strTargetBiblioRecPath);
            if (string.IsNullOrEmpty(strParentID) == true)
            {
                strError = "目标书目记录路径 '" + strTargetBiblioRecPath + "' 不正确，无法获得记录号";
                return -1;
            }

            List<string> newrecordpaths = new List<string>();
            List<string> oldrecordpaths = new List<string>();
            List<string> parentids = new List<string>();
            List<string> oldrecords = new List<string>();

            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                string strOutputRecPath = "";

                string strNewBarcode = "";  // 复制中修改后的册条码号

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 装入XMLDOM发生错误: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.SetElementText(dom.DocumentElement,
                        "parent",
                        strParentID);

                    // 复制的情况，要避免出现操作后的条码号重复现象
                    if (strAction == "copy")
                    {
                        string strNewGuid = ShortGuid.NewGuid().ToString().ToUpper();
                        // 修改册条码号，避免发生条码号重复
                        string strOldItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                            "barcode");
                        if (String.IsNullOrEmpty(strOldItemBarcode) == false)
                        {
                            strNewBarcode = strOldItemBarcode + "_" + strNewGuid;
                            DomUtil.SetElementText(dom.DocumentElement,
                                "barcode",
                                strNewBarcode);
                        }

                        // *** 后面这几个清除动作要作为规则出现
                        string strOldRefID = DomUtil.GetElementText(dom.DocumentElement,
                            "refID");
                        DomUtil.SetElementText(dom.DocumentElement,
    "oldRefID",
    strOldRefID);

                        // 替换 refid
                        DomUtil.SetElementText(dom.DocumentElement,
                            "refID",
                            strNewGuid);

                        // 把借者清除
                        // (源实体记录中如果有借阅信息，在普通界面上是无法删除此记录的。只能用出纳窗正规进行归还，然后才能删除)
                        {
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrower",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowPeriod",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowDate",
                                null);
                        }
                    }

                    // TODO: 可以顺便确认有没有对象资源。如果没有，就省略CopyRecord操作

                    long lRet = channel.DoCopyRecord(info.RecPath,
                         strTargetItemDbName + "/?",
                         strAction == "move" ? true : false,   // bDeleteSourceRecord
                         out output_timestamp,
                         out strOutputRecPath,
                         out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;
                        strError = "复制实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                        goto ERROR1;
                    }

                    // 修改xml记录。<parent>元素发生了变化
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(strOutputRecPath,
                        dom.OuterXml,
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    oldrecordpaths.Add(info.RecPath);
                    newrecordpaths.Add(strOutputRecPath);
                    parentids.Add(strParentID);
                    if (strAction == "move")
                        oldrecords.Add(info.OldRecord);
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // 增补到日志DOM中
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                    DomUtil.SetAttr(node, "targetRecPath", strOutputRecPath);

                    // 2014/1/5
                    if (string.IsNullOrEmpty(strNewBarcode) == false)
                        DomUtil.SetAttr(node, "newBarcode", strNewBarcode);
                }

                nOperCount++;
            }

            return nOperCount;
        ERROR1:
            // Undo已经进行过的操作
            // TODO: 写入错误日志

            if (strAction == "copy")
            {
                string strWarning = "";

                foreach (string strRecPath in newrecordpaths)
                {
                    string strTempError = "";
                    byte[] timestamp = null;
                    byte[] output_timestamp = null;
                REDO_DELETE:
                    long lRet = channel.DoDeleteRes(strRecPath,
                        timestamp,
                        out output_timestamp,
                        out strTempError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (timestamp == null)
                            {
                                timestamp = output_timestamp;
                                goto REDO_DELETE;
                            }
                        }
                        strWarning += strTempError + ";";
                    }

                }
                if (string.IsNullOrEmpty(strWarning) == false)
                    strError = strError + "。在Undo过程中，又遇到出错: " + strWarning;
            }
            else if (strAction == "move")
            {
                string strWarning = "";
                for (int i = 0; i < newrecordpaths.Count; i++)
                {
                    byte[] output_timestamp = null;
                    string strOutputRecPath = "";
                    string strTempError = "";
                    // TODO: 如果确认没有对象，就可以省略这一步
                    long lRet = channel.DoCopyRecord(newrecordpaths[i],
         oldrecordpaths[i],
         true,   // bDeleteSourceRecord
         out output_timestamp,
         out strOutputRecPath,
         out strTempError);
                    if (lRet == -1)
                    {
                        strWarning += strTempError + ";";
                    }

                    // 修改xml记录。<parent>元素发生了变化
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(oldrecordpaths[i],
                        oldrecords[i],
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strTempError);
                    if (lRet == -1)
                    {
                        strWarning += strTempError + ";";
                    }
                }
                if (string.IsNullOrEmpty(strWarning) == false)
                    strError = strError + "。在Undo过程中，又遇到出错: " + strWarning;
            }
            return -1;
        }

        // 复制属于同一书目记录的全部实体记录
        // parameters:
        //      strAction   copy / move
        // return:
        //      -1  error
        //      >=0  实际复制或者移动的实体记录数
        public int CopyBiblioChildRecords(RmsChannel channel,
            string strAction,
            List<DeleteEntityInfo> entityinfos,
            List<string> target_recpaths,
            string strTargetBiblioRecPath,
            List<string> newbarcodes,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            if (entityinfos.Count != target_recpaths.Count)
            {
                strError = "entityinfos.Count (" + entityinfos.Count.ToString() + ") != target_recpaths.Count (" + target_recpaths.Count.ToString() + ")";
                return -1;
            }

            int nOperCount = 0;

            string strParentID = ResPath.GetRecordId(strTargetBiblioRecPath);
            if (string.IsNullOrEmpty(strParentID) == true)
            {
                strError = "目标书目记录路径 '" + strTargetBiblioRecPath + "' 不正确，无法获得记录号";
                return -1;
            }

            List<string> newrecordpaths = new List<string>();
            List<string> oldrecordpaths = new List<string>();
            List<string> parentids = new List<string>();
            List<string> oldrecords = new List<string>();

            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];
                string strTargetRecPath = target_recpaths[i];

                string strNewBarcode = newbarcodes[i];

                byte[] output_timestamp = null;
                string strOutputRecPath = "";

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(info.OldRecord);
                    }
                    catch (Exception ex)
                    {
                        strError = "记录 '" + info.RecPath + "' 装入XMLDOM发生错误: " + ex.Message;
                        goto ERROR1;
                    }
                    DomUtil.SetElementText(dom.DocumentElement,
                        "parent",
                        strParentID);

                    // 复制的情况，要避免出现操作后的条码号重复现象
                    if (strAction == "copy")
                    {
                        // 修改册条码号，避免发生条码号重复
                        string strOldItemBarcode = DomUtil.GetElementText(dom.DocumentElement,
                            "barcode");
                        if (String.IsNullOrEmpty(strOldItemBarcode) == false)
                        {
                            // 2014/1/5
                            if (string.IsNullOrEmpty(strNewBarcode) == true)
                                strNewBarcode = "temp_" + strOldItemBarcode;
                            DomUtil.SetElementText(dom.DocumentElement,
                                "barcode",
                                strNewBarcode);
                        }

                        // 2014/1/5
                        DomUtil.SetElementText(dom.DocumentElement,
                            "refID",
                            null);

                        // 把借者清除
                        // (源实体记录中如果有借阅信息，在普通界面上是无法删除此记录的。只能用出纳窗正规进行归还，然后才能删除)
                        {
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrower",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowPeriod",
                                null);
                            DomUtil.SetElementText(dom.DocumentElement,
                                "borrowDate",
                                null);
                        }
                    }

                    // TODO: 可以顺便确认有没有对象资源。如果没有，就省略CopyRecord操作

                    long lRet = channel.DoCopyRecord(info.RecPath,
                         strTargetRecPath,
                         strAction == "move" ? true : false,   // bDeleteSourceRecord
                         out output_timestamp,
                         out strOutputRecPath,
                         out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;
                        strError = "复制实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                        goto ERROR1;
                    }



                    // 修改xml记录。<parent>元素发生了变化
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath1 = "";
                    lRet = channel.DoSaveTextRes(strOutputRecPath,
                        dom.OuterXml,
                        false,
                        "content", // ,ignorechecktimestamp
                        output_timestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath1,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    oldrecordpaths.Add(info.RecPath);
                    newrecordpaths.Add(strOutputRecPath);
                    parentids.Add(strParentID);
                    if (strAction == "move")
                        oldrecords.Add(info.OldRecord);
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                nOperCount++;
            }

            return nOperCount;
        ERROR1:
            // 不要Undo
            return -1;
        }

        // 删除属于同一书目记录的全部实体记录
        // 这是需要提供EntityInfo数组的版本
        // return:
        //      -1  error
        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
        //      >0  实际删除的实体记录数
        public int DeleteBiblioChildEntities(RmsChannel channel,
            List<DeleteEntityInfo> entityinfos,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nDeletedCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement("deletedEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }

            // 真正实行删除
            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                int nRedoCount = 0;

            REDO_DELETE:

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {

                    long lRet = channel.DoDeleteRes(info.RecPath,
                        info.OldTimestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        // 如果不重试，让时间戳出错暴露出来。
                        // 如果要重试，也得加上重新读入册记录并判断重新判断无借还信息才能删除

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount > 10)
                            {
                                strError = "重试了10次还不行。删除实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                                goto ERROR1;
                            }
                            nRedoCount++;

                            // 重新读入记录
                            string strMetaData = "";
                            string strXml = "";
                            string strOutputPath = "";
                            string strError_1 = "";

                            lRet = channel.GetRes(info.RecPath,
                                out strXml,
                                out strMetaData,
                                out output_timestamp,
                                out strOutputPath,
                                out strError_1);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                    continue;

                                strError = "在删除实体记录 '" + info.RecPath + "' 时发生时间戳冲突，于是自动重新获取记录，但又发生错误: " + strError_1;
                                goto ERROR1;
                                // goto CONTINUE;
                            }

                            // 检查是否有借阅信息
                            // 把记录装入DOM
                            XmlDocument domExist = new XmlDocument();

                            try
                            {
                                domExist.LoadXml(strXml);
                            }
                            catch (Exception ex)
                            {
                                strError = "实体记录 '" + info.RecPath + "' XML装载进入DOM时发生错误: " + ex.Message;
                                goto ERROR1;
                            }

                            info.ItemBarcode = DomUtil.GetElementText(domExist.DocumentElement,
                                "barcode");

                            // 观察已经存在的记录是否有流通信息
                            string strDetail = "";
                            bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                                out strDetail);
                            if (bHasCirculationInfo == true)
                            {
                                strError = "拟删除的册记录 '" + info.RecPath + "' 中包含有流通信息(" + strDetail + ")(此种情况可能不限于这一条)，不能删除。";
                                goto ERROR1;
                            }


                            info.OldTimestamp = output_timestamp;
                            goto REDO_DELETE;
                        }

                        strError = "删除实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // 增补到日志DOM中
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                }

                nDeletedCount++;
            }


            return nDeletedCount;
        ERROR1:
            return -1;
        }

        // 删除属于同一书目记录的全部实体记录。注意，不检查下属册记录的路通信息
        // 这是检索和删除一次进行的版本
        // return:
        //      -1  error
        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
        //      >0  实际删除的实体记录数
        public int DeleteBiblioChildEntities(RmsChannel channel,
            string strBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            List<DeleteEntityInfo> entityinfos = null;
            long lHitCount = 0;
            // return:
            //      -2  not exist entity dbname
            //      -1  error
            //      >=0 含有流通信息的实体记录个数
            int nRet = SearchChildEntities(channel,
                strBiblioRecPath,
                "",   // "check_borrow_info",    // 2011/4/24
                (Delegate_checkRecord)null,
                null,
                out lHitCount,
                out entityinfos,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == -2)
            {
                Debug.Assert(entityinfos.Count == 0, "");
            }
            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            nRet = DeleteBiblioChildEntities(channel,
                entityinfos,
                domOperLog,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 0;
        ERROR1:
            return -1;
        }

#if NOOOOOOOOOOOOOOOOOOO
        // 删除属于同一书目记录的全部实体记录
        // return:
        //      -1  error
        //      0   没有找到属于书目记录的任何实体记录，因此也就无从删除
        //      >0  实际删除的实体记录数
        public int DeleteBiblioChildEntities(RmsChannelCollection Channels,
            string strBiblioRecPath,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的实体库名
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            RmsChannel channel = Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 检索实体库中全部从属于特定id的记录

            string strQueryXml = "<target list='" + strItemDbName + ":" + "父记录" + "'><item><word>"
                + strBiblioRecId
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                strError = "没有找到属于书目记录 '" + strBiblioRecPath + "' 的任何实体记录";
                return 0;
            }

            int nResultCount = (int)lRet;

            if (nResultCount > 500)
            {
                strError = "命中册记录数 " + nResultCount.ToString() + " 超过 500, 暂时不支持针对它们的删除操作";
                goto ERROR1;
            }

            List<EntityInfo> entityinfos = new List<EntityInfo>();

            int nStart = 0;
            int nPerCount = 100;
            for (; ; )
            {
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }

                // 获得每条记录
                for (int i = 0; i < aPath.Count; i++)
                {
                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    lRet = channel.GetRes(aPath[i],
                        out strXml,
                        out strMetaData,
                        out timestamp,
                        out strOutputPath,
                        out strError);
                    EntityInfo entityinfo = new EntityInfo();

                    if (lRet == -1)
                    {
                        /*
                        entityinfo.RecPath = aPath[i];
                        entityinfo.ErrorCode = channel.OriginErrorCode;
                        entityinfo.ErrorInfo = channel.ErrorInfo;

                        entityinfo.OldRecord = "";
                        entityinfo.OldTimestamp = null;
                        entityinfo.NewRecord = "";
                        entityinfo.NewTimestamp = null;
                        entityinfo.Action = "";
                         * */
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        strError = "获取实体记录 '" + aPath[i] + "' 时发生错误: " + strError;
                        goto ERROR1;
                        // goto CONTINUE;
                    }

                    entityinfo.RecPath = strOutputPath;
                    entityinfo.OldTimestamp = timestamp;
                    /*
                    entityinfo.OldRecord = strXml;
                    entityinfo.NewRecord = "";
                    entityinfo.NewTimestamp = null;
                    entityinfo.Action = "";
                     * */
                    // 检查是否有借阅信息
                    // 把记录装入DOM
                    XmlDocument domExist = new XmlDocument();

                    try
                    {
                        domExist.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "strXml装载进入DOM时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    // 观察已经存在的记录是否有流通信息
                    if (IsEntityHasCirculationInfo(domExist) == true)
                    {
                        strError = "拟删除的册记录 '" + entityinfo.RecPath + "' 中包含有流通信息(此种情况可能不限于这一条)，不能删除。";
                        goto ERROR1;
                    }

                // CONTINUE:
                    entityinfos.Add(entityinfo);
                }

                nStart += aPath.Count;
                if (nStart >= nResultCount)
                    break;
            }

            int nDeletedCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement("deletedEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }


            // 真正实行删除
            for (int i = 0; i < entityinfos.Count; i++)
            {
                EntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                int nRedoCount = 0;
            REDO_DELETE:
                lRet = channel.DoDeleteRes(info.RecPath,
                    info.OldTimestamp,
                    out output_timestamp,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                        continue;

                    if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        if (nRedoCount > 10)
                        {
                            strError = "重试了10次还不行。删除实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                            goto ERROR1;
                        }
                        nRedoCount++;
                        info.OldTimestamp = output_timestamp;
                        goto REDO_DELETE;
                    }

                    strError = "删除实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                    goto ERROR1;
                }

                // 增补到日志DOM中
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                }

                nDeletedCount++;
            }


            return nDeletedCount;
        ERROR1:
            return -1;
        }
#endif

        // 包装后的版本，兼容以前的脚本调用
        public LibraryServerResult GetEntities(
    SessionInfo sessioninfo,
    string strBiblioRecPath,
    long lStart,
    long lCount,
    out EntityInfo[] entities)
        {
            return GetEntities(
    sessioninfo,
    strBiblioRecPath,
    lStart,
    lCount,
    "",
    "zh",
    out entities);
        }

        // 从style字符串中得到 librarycode:XXXX子串
        // 注意，如果xxxx中是多个馆代码，要表达为 "code1|code2"这样的形态。本函数能自动把'|'替换为','
        static string GetLibraryCodeParam(string strStyle)
        {
            // 2017/1/18 加上了保护
            if (string.IsNullOrEmpty(strStyle))
                return null;

            string[] parts = strStyle.Split(new char[] { ',' });
            foreach (string strPart in parts)
            {
                string strText = strPart.Trim();
                if (StringUtil.HasHead(strText, "librarycode:") == true)
                    return strText.Substring("librarycode:".Length).Trim().Replace("|", ",");
            }

            return null;
        }


        // 获得册信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分。如果用 @path-list: 引导，表示这里是根据给出的册记录路径来获取册记录
        //      lStart  返回从第几个开始    2009/6/7 add
        //      lCount  总共返回几个。0和-1都表示全部返回(0是为了兼容旧API)
        //      entityinfos 返回的实体信息数组
        //      strStyle    "opac" 把实体记录按照OPAC要求进行加工，增补一些元素
        //                  "onlygetpath"   仅返回每个路径
        //                  "getfirstxml"   是对onlygetpath的补充，仅获得第一个元素的XML记录，其余的依然只返回路径
        // strStyle 中筛选分馆的册，有以下几种情况
        // 全局用户，不过滤
        //      什么都不用特意指定
        // 分馆用户，获得全部分馆
        //      style中要包含 getotherlibraryitem
        // 分馆用户，只获得自己管辖的分馆
        //      什么都不用特意指定
        // 全局用户，只返回指定的分馆
        //      style中要包含 librarycode:xxxx
        // 分馆用户，只返回指定的分馆。注意，这不一定是指分馆用户管辖的分馆
        //      style中要包含 librarycode:xxxx
        // 注: librarycode:xxxx 中的 xxx 部分可以是多个馆代码的列表，用 | 分隔
        //
        // 权限：需要有getentities权限
        // return:
        //      Result.Value    -1出错 0没有找到 其他 总的实体记录的个数(本次返回的，可以通过entities.Count得到)
        public LibraryServerResult GetEntities(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            long lStart,
            long lCount,
            string strStyle,    // 2011/1/21
            string strLang,     // 2011/1/21
            out EntityInfo[] entities)
        {
            entities = null;

            LibraryServerResult result = new LibraryServerResult();

            // 权限字符串
            if (StringUtil.IsInList("getentities,getiteminfo,order", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "获得册信息 操作被拒绝。不具备 order、getiteminfo 或 getentities 权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // 规范化参数值
            if (lCount == 0)
                lCount = -1;

            int nRet = 0;
            string strError = "";

            // 有以下几种情况
            // 全局用户，不过滤
            //      什么都不用特意指定
            // 分馆用户，获得全部分馆
            //      style中要包含 getotherlibraryitem
            // 分馆用户，只获得自己管辖的分馆
            //      什么都不用特意指定
            // 全局用户，只返回指定的分馆
            //      style中要包含 librarycode:xxxx
            // 分馆用户，只返回指定的分馆。注意，这不一定是指分馆用户管辖的分馆
            //      style中要包含 librarycode:xxxx

            string strLibraryCodeParam = GetLibraryCodeParam(strStyle);
            if (sessioninfo.GlobalUser == false && string.IsNullOrEmpty(strLibraryCodeParam) == true)
                strLibraryCodeParam = sessioninfo.LibraryCodeList;

            bool bGetOtherLibraryItem = StringUtil.IsInList("getotherlibraryitem", strStyle);
            /*
            if (bGetOtherLibraryItem == true)
                strLibraryCodeParam = null;
             * */

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的实体库名
            string strItemDbName = "";
            nRet = this.GetItemDbName(strBiblioDbName,
                 out strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            if (String.IsNullOrEmpty(strItemDbName) == true)
            {
                result.Value = -1;
                result.ErrorInfo = "书目库 '" + strBiblioDbName + "' 未定义下属的实体库";
                result.ErrorCode = ErrorCode.ItemDbNotDef;
                return result;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            // 检索实体库中全部从属于特定id的记录

            string strQueryXml = "";

            if ((sessioninfo.GlobalUser == true && string.IsNullOrEmpty(strLibraryCodeParam) == true)
                || bGetOtherLibraryItem == true)
            {
                strQueryXml = "<target list='"
                     + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录")       // 2007/9/14 
                     + "'><item><word>"
                     + strBiblioRecId
                     + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
            }
            else
            {
                // 仅仅取得当前用户管辖的分馆的册记录
                List<string> codes = StringUtil.SplitList(strLibraryCodeParam); // sessioninfo.LibraryCodeList
                foreach (string strCode in codes)
                {
                    string strOneQueryXml = "<target list='"
         + StringUtil.GetXmlStringSimple(strItemDbName + ":" + "父记录+馆藏地点")
         + "'><item><word>"
         + StringUtil.GetXmlStringSimple(strBiblioRecId + "|" + strCode + "/")
         + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>" + "zh" + "</lang></target>";
                    if (string.IsNullOrEmpty(strQueryXml) == false)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strQueryXml) == false, "");
                        strQueryXml += "<operator value='OR'/>";
                    }

                    strQueryXml += strOneQueryXml;
                }
                if (codes.Count > 0)
                {
                    strQueryXml = "<group>" + strQueryXml + "</group>";
                }
            }

            long lRet = channel.DoSearch(strQueryXml,
                "entities",
                "", // strOuputStyle
                out strError);
            if (lRet == -1)
                goto ERROR1;

            if (lRet == 0)
            {
                result.Value = 0;
                result.ErrorInfo = "没有找到";
                return result;
            }

            int MAXPERBATCH = 100;

            int nResultCount = (int)lRet;

            if (lCount == -1)
                lCount = nResultCount - (int)lStart;

            // lStart是否越界
            if (lStart >= (long)nResultCount)
            {
                strError = "lStart参数值 " + lStart.ToString() + " 超过了命中结果集的尾部。命中结果数量为 " + nResultCount.ToString();
                goto ERROR1;
            }

            // 2010/12/16
            // 修正lCount
            if (lStart + lCount > (long)nResultCount)
            {
                // strError = "lStart参数值 " + lStart.ToString() + " 和lCount参数值 " + lCount.ToString() + " 之和大于命中结果数量 " + nResultCount.ToString();
                // goto ERROR1;
                lCount = (long)nResultCount - lStart;
            }

            // 是否超过每批最大值
            if (lCount > MAXPERBATCH)
                lCount = MAXPERBATCH;

            /*
            // 2009/6/7 
            if (lCount > 0)
                nResultCount = Math.Min(nResultCount-(int)lStart, (int)lCount);
             * */

            /*
            if (nResultCount > 10000)
            {
                strError = "命中册记录数 " + nResultCount.ToString() + " 超过 10000, 暂时不支持";
                goto ERROR1;
            }*/

            bool bOnlyGetPath = StringUtil.IsInList("onlygetpath", strStyle);
            bool bGetFirstXml = StringUtil.IsInList("getfirstxml", strStyle);

            string strColumnStyle = "id,xml,timestamp";
            if (bOnlyGetPath)
                strColumnStyle = "id";

            List<EntityInfo> entityinfos = new List<EntityInfo>();

            int nStart = (int)lStart;
            int nPerCount = Math.Min(MAXPERBATCH, (int)lCount); // 2009/6/7 changed
            for (; ; )
            {
#if NO
                List<string> aPath = null;
                lRet = channel.DoGetSearchResult(
                    "entities",
                    nStart,
                    nPerCount,
                    strLang,    // 2012/4/16 // "zh",
                    null,
                    out aPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                if (aPath.Count == 0)
                {
                    strError = "aPath.Count == 0";
                    goto ERROR1;
                }
#endif
                Record[] searchresults = null;
                lRet = channel.DoGetSearchResult(
    "entities",
    nStart,
    nPerCount,
    strColumnStyle,
    strLang,
    null,
    out searchresults,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (searchresults == null)
                {
                    strError = "searchresults == null";
                    goto ERROR1;
                }
                if (searchresults.Length == 0)
                {
                    strError = "searchresults.Length == 0";
                    goto ERROR1;
                }

                // 获得每条记录
                // for (int i = 0; i < aPath.Count; i++)
                foreach (Record record in searchresults)
                {
                    EntityInfo entityinfo = new EntityInfo();
                    entityinfo.OldRecPath = record.Path;

                    if (bOnlyGetPath == true)
                    {
                        if (bGetFirstXml == false
                            || entityinfos.Count > 0)
                        {
                            // entityinfo.OldRecPath = aPath[i];
                            goto CONTINUE;
                        }
                    }

                    string strMetaData = "";
                    string strXml = "";
                    byte[] timestamp = null;
                    string strOutputPath = "";

                    if (bGetFirstXml && entityinfos.Count == 0
                        && !(record.RecordBody != null && string.IsNullOrEmpty(record.RecordBody.Xml) == false))
                    {
                        lRet = channel.GetRes(
                            // aPath[i],
                            record.Path,
                            out strXml,
                            out strMetaData,
                            out timestamp,
                            out strOutputPath,
                            out strError);
                    }
                    else
                    {
                        lRet = 0;
                        if (record.RecordBody != null)
                        {
                            strXml = record.RecordBody.Xml;
                            timestamp = record.RecordBody.Timestamp;
                            strOutputPath = record.Path;
                        }
                        else
                        {
                            strOutputPath = record.Path;
                            entityinfo.ErrorCode = ErrorCodeValue.NotFound;
                        }
                    }

                    if (lRet == -1)
                    {
                        // entityinfo.OldRecPath = aPath[i];
                        entityinfo.OldRecPath = record.Path;
                        entityinfo.ErrorCode = channel.OriginErrorCode;
                        entityinfo.ErrorInfo = channel.ErrorInfo;

                        entityinfo.OldRecord = "";
                        entityinfo.OldTimestamp = null;

                        entityinfo.NewRecPath = "";
                        entityinfo.NewRecord = "";
                        entityinfo.NewTimestamp = null;
                        entityinfo.Action = "";
                        goto CONTINUE;
                    }

                    XmlDocument itemdom = null;

                    // 修改<borrower>
                    if (sessioninfo.GlobalUser == false // 分馆用户必须要过滤，因为要修改<borrower>
                        && string.IsNullOrEmpty(strXml) == false)
                    {
                        nRet = LibraryApplication.LoadToDom(strXml,
                            out itemdom,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        {
                            // 检查一个册记录的馆藏地点是否符合当前用户管辖的馆代码列表要求
                            // return:
                            //      -1  检查过程出错
                            //      0   符合要求
                            //      1   不符合要求
                            nRet = CheckItemLibraryCode(itemdom,
                                        sessioninfo.LibraryCodeList,
                                        out string strLibraryCode,
                                        out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            if (nRet == 1)
                            {
                                // 把借阅人的证条码号覆盖
                                string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                                    "borrower");
                                if (string.IsNullOrEmpty(strBorrower) == false)
                                    DomUtil.SetElementText(itemdom.DocumentElement,
                                        "borrower", new string('*', strBorrower.Length));
                                strXml = itemdom.DocumentElement.OuterXml;
                            }
                        }
                    }

                    // 把实体记录按照OPAC要求进行加工，增补一些元素
                    if (StringUtil.IsInList("opac", strStyle) == true
                        && string.IsNullOrEmpty(strXml) == false)
                    {
                        if (itemdom == null)
                        {
                            nRet = LibraryApplication.LoadToDom(strXml,
                                out itemdom,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        nRet = AddOpacInfos(
                            sessioninfo,
                            strLang,
                            record.Path,
                            ref itemdom,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        strXml = itemdom.DocumentElement.OuterXml;
                    }

                    entityinfo.OldRecPath = strOutputPath;
                    entityinfo.OldRecord = strXml;
                    entityinfo.OldTimestamp = timestamp;

                    entityinfo.NewRecPath = "";
                    entityinfo.NewRecord = "";
                    entityinfo.NewTimestamp = null;
                    entityinfo.Action = "";
                CONTINUE:
                    entityinfos.Add(entityinfo);
                }

                // nStart += aPath.Count;
                nStart += searchresults.Length;
                if (nStart >= nResultCount)
                    break;
                if (entityinfos.Count >= lCount)
                    break;

                // 修正nPerCount
                if (entityinfos.Count + nPerCount > lCount)
                    nPerCount = (int)lCount - entityinfos.Count;
            }

            // 挂接到结果中
#if NO
            entities = new EntityInfo[entityinfos.Count];
            for (int i = 0; i < entityinfos.Count; i++)
            {
                entities[i] = entityinfos[i];
            }
#endif
            entities = entityinfos.ToArray();
            result.Value = nResultCount;   // entities.Length;
            return result;
        ERROR1:
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        // 给册记录内增加OPAC信息
        // parameters:
        //      strLibraryCode  读者记录所从属的读者库的馆代码
        int AddOpacInfos(
            SessionInfo sessioninfo,
            string strLang,
            string item_recpath,
            ref XmlDocument item_dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
            string strState = DomUtil.GetElementText(item_dom.DocumentElement, "state");

            // <borrowerRecPath>是2012/9/8以后为实体记录新增的一个元素，内容是借者读者记录的路径
            string strBorrowerRecPath = DomUtil.GetElementText(item_dom.DocumentElement, "borrowerRecPath");

            string strBorrowerLibraryCode = ""; // 当前册的借阅者所在的馆代码
            if (string.IsNullOrEmpty(strBorrowerRecPath) == false)
            {
                nRet = this.GetLibraryCode(strBorrowerRecPath,
                    out strBorrowerLibraryCode,
                    out strError);
                /*
                if (nRet == -1)
                    goto ERROR1;
                 * */
                // TODO: 如何报错?
            }

            //////////
            // 馆藏地点
            string strLocation = DomUtil.GetElementText(item_dom.DocumentElement, "location");
            // 去掉#reservation部分
            strLocation = StringUtil.GetPureLocationString(strLocation);

            // 检查册所属的馆藏地点是否合读者所在的馆藏地点吻合
            // 当前册所在的馆代码

            // 解析
            ParseCalendarName(strLocation,
        out string strItemLibraryCode,
        out string strPureLocationName);


            // 根据状态是否为空, 设置checkbox状态
            if (string.IsNullOrEmpty(strState) == false)
            {
                string strText = "此册因状态为 " + strState + " 而不能外借。";
                XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                    "canBorrow", strText);
                DomUtil.SetAttr(node, "canBorrow", "false");
                goto SKIP1;
            }
            else
            {
                // 注：全局用户就当每个分馆都可借。但OPAC items 界面上实际上要到输入读者证件条码号提交预约后才知道效果
                if (sessioninfo.GlobalUser == false
                    && StringUtil.IsInList(strItemLibraryCode, sessioninfo.LibraryCodeList) == false)
                {
                    string strText = "此册因属其他分馆 " + strItemLibraryCode + " 而不能借阅。";
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                        "canBorrow", strText);
                    DomUtil.SetAttr(node, "canBorrow", "false");
                    goto SKIP1;
                }
            }

            // 根据馆藏地点是否允许借阅, 设置checkbox状态
            // 个人书斋不在 library.xml 中定义
            if (IsPersonalLibraryRoom(strPureLocationName) == true)    // 2015/6/14
                goto SKIP1;

            StringBuilder debugInfo = null;
            // 检查册是否允许被借出
            // return:
            //      -1  出错
            //      0   借阅操作应该被拒绝
            //      1   借阅操作应该被允许
            nRet = CheckCanBorrow(
                strItemLibraryCode, // 读者的 librarycode?
                false,
                sessioninfo.Account,
                sessioninfo.Account?.ReaderDomPath,
                sessioninfo.Account?.PatronDom,
                item_recpath,
                item_dom,
                ref debugInfo,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
    "canBorrow", strError);
                DomUtil.SetAttr(node, "canBorrow", "false");
            }

            if (String.IsNullOrEmpty(strError) == false)
            {
                XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
    "stateMessage", strError);
            }

#if NO
                bool bResultValue = false;
                string strMessageText = "";

                // 执行脚本函数ItemCanBorrow
                // parameters:
                //      bResultValue    [out] 是否允许外借。如果返回 true，表示允许外借；false 表示不允许外借
                // return:
                //      -2  not found script
                //      -1  出错
                //      0   成功
                nRet = this.DoItemCanBorrowScriptFunction(
                    false,
                    sessioninfo.Account,
                    null,
                    item_dom,
                    out bResultValue,
                    out strMessageText,
                    out strError);
                if (nRet == -1)
                {
                    strMessageText = strError;
                }

                if (nRet == -2)
                {


                    {
                        List<LocationType> locations = this.GetLocationTypes(strItemLibraryCode);
                        LocationType location = locations.Find((o) => { return o.Location == strPureLocationName; });

                        if (location == null
                            || string.IsNullOrEmpty(location.CanBorrow) == true
                            || location.CanBorrow == "no")
                        {
                            string strText = "此册因属馆藏地点 " + strLocation + " 而不能外借。";
                            XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                                "canBorrow", strText);
                            DomUtil.SetAttr(node, "canBorrow", "false");
                        }

                        // 执行脚本
                        if (location.CanBorrow != "yes")
                        {
                            if (location.CanBorrow.StartsWith("javascript:") == false)
                            {
                                strError = "locationTypes//item 元素定义不合法：出现了无法识别的脚本代码: '" + location.CanBorrow + "'";
                                return -1;
                            }
                            string strScript = location.CanBorrow.Substring("javascript:".Length);
                            Engine engine = new Engine(cfg => cfg.AllowClr(typeof(MarcQuery).Assembly))
            .SetValue("account", sessioninfo.Account)
            .SetValue("readerRecord", JsValue.Null)
            .SetValue("itemRecord", new ItemRecord(item_dom));

                            engine.Execute("var DigitalPlatform = importNamespace('DigitalPlatform');\r\n"
                                + strScript) // execute a statement
                                ?.GetCompletionValue() // get the latest statement completion value
                                ?.ToObject()?.ToString() // converts the value to .NET
                                ;
                            string result = GetString(engine, "result", "no");
                            string message = GetString(engine, "message", "");
                            //if (debugInfo != null)
                            //    debugInfo.Append("馆藏地事项 '" + location.ToString() + "' 脚本执行后返回 result='" + result + "'(message='" + message + "')\r\n");

                            if (string.IsNullOrEmpty(result) == true
                                || result == "no")
                            {
                                // 不允许外借
                                // text-level: 用户提示
                                if (string.IsNullOrEmpty(message))
                                    strError = "此册因属馆藏地点 " + strLocation + " 并且脚本执行结果表示不能外借。";
                                else
                                    strError = "此册因属馆藏地点 " + strLocation + " 而不能外借。原因: " + message;

                                XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                                    "canBorrow", strError);
                                DomUtil.SetAttr(node, "canBorrow", "false");
                            }
                        }
#if NO
                            List<string> locations = this.GetLocationTypes(strItemLibraryCode, true);
                            if (locations.IndexOf(strPureLocationName) == -1)
                            {
                                string strText = "此册因属馆藏地点 " + strLocation + " 而不能外借。";
                                XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                                    "canBorrow", strText);
                                DomUtil.SetAttr(node, "canBorrow", "false");
                            }
#endif

                    }

                }
                else
                {
                    if (bResultValue == false)
                    {
                        XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
            "canBorrow", strMessageText);
                        DomUtil.SetAttr(node, "canBorrow", "false");
                    }
                }


                if (String.IsNullOrEmpty(strMessageText) == false)
                {
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
        "stateMessage", strMessageText);
                }

            }
#endif


        SKIP1:
            // 状态
            // string strState = DomUtil.GetElementText(dom.DocumentElement, "state");

            // 借者条码
            //string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");

            // 续借次
            //string strNo = DomUtil.GetElementText(dom.DocumentElement, "no");

            // 借阅日期
            string strBorrowDate = DomUtil.GetElementText(item_dom.DocumentElement, "borrowDate");
            string strTime = strBorrowDate;
            if (String.IsNullOrEmpty(strTime) == false)
            {
                try
                {
                    strTime = DateTimeUtil.LocalTime(strTime);
                }
                catch
                {
                    strTime = "时间格式错误 -- " + strTime;
                }
            }

            string strClass = "";

            // <borrowerReaderType>是2009/9/18以后为实体记录新增的一个元素，是从读者记录中<readerType>中复制过来的
            string strBorrowerReaderType = DomUtil.GetElementText(item_dom.DocumentElement, "borrowerReaderType");

            // 借阅期限
            string strPeriod = DomUtil.GetElementText(item_dom.DocumentElement, "borrowPeriod");

            string strOverDue = ""; // 超期情况字符串。已经被规范过，不超期的时候这个字符串为空值
            string strOriginOverdue = "";   // 超期情况字符串，没有加工过，如果是不超期的时候，则会说还有多少天到期
            long lOver = 0;
            string strPeriodUnit = "";

            if (String.IsNullOrEmpty(strBorrowDate) == false)
            {
                // 获得日历
                Calendar calendar = null;

                if (String.IsNullOrEmpty(strBorrowerReaderType) == false)
                {
                    // return:
                    //      -1  出错
                    //      0   没有找到日历
                    //      1   找到日历
                    nRet = this.GetReaderCalendar(strBorrowerReaderType,
                        strBorrowerLibraryCode,
                        out calendar,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        calendar = null;
                    }
                }

                // 检查超期情况。
                // return:
                //      -1  数据格式错误
                //      0   没有发现超期
                //      1   发现超期   strError中有提示信息
                //      2   已经在宽限期内，很容易超期 2009/3/13 
                nRet = this.CheckPeriod(
                    calendar,   // 2009/9/18 changed
                    strBorrowDate,
                    strPeriod,
                    out lOver,
                    out strPeriodUnit,
                    out strError);

                strOriginOverdue = strError;

                if (nRet == -1)
                    strOverDue = strError;  // 错误信息

                if (nRet == 1)
                    strOverDue = this.GetString("已超期");
                else if (nRet == 2) // 2009/9/18 
                    strOverDue = this.GetString("已在宽限期内，即将到期");

                /*
                if (nRet == 1 || nRet == 0)
                    strOverDue = strError;	// "已超期";
                 * */
                if (nRet == 1)
                    strClass = "over";
                else if (nRet == 2) // 2009/9/18 
                    strClass = "warning";
                else if (nRet == 0 && lOver >= -5)
                    strClass = "warning";

                // strPeriod = this.GetDisplayTimePeriodStringEx(strPeriod);

                // 超期情况
                {
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                        "overdueInfo", strOverDue);
                    if (String.IsNullOrEmpty(strClass) == false)
                        DomUtil.SetAttr(node, "type", strClass);
                }

                {
                    XmlNode node = DomUtil.SetElementText(item_dom.DocumentElement,
                            "originOverdueInfo", strOriginOverdue);
                    DomUtil.SetAttr(node, "calendar", calendar != null ? calendar.Name : "");
                    DomUtil.SetAttr(node, "over", lOver.ToString());
                    DomUtil.SetAttr(node, "unit", strPeriodUnit);
                }
            }

            return 0;
        }

        // 校验一个册记录
        // return:
        //      -1  出错。出错信息在 strError 中返回
        //      0   校验没有发现错误。result 中返回了校验结果
        //      1   校验发现了错误。result 中返回了校验结果
        int VerifyEntity(
            SessionInfo sessioninfo,
            EntityInfo info,
            out EntityInfo result,
            out string strError)
        {
            strError = "";
            result = new EntityInfo();
            result.OldRecPath = info.OldRecPath;
            result.RefID = info.RefID;
            result.ErrorCode = ErrorCodeValue.CommonError;

            List<string> errors = new List<string>();

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            string strRecPath = info.OldRecPath;
            string strXml = info.OldRecord;
            if (string.IsNullOrEmpty(strXml))
            {
                // 检查路径中的库名部分
                if (String.IsNullOrEmpty(strRecPath) == false)
                {
                    strError = "";

                    string strDbName = ResPath.GetDbName(strRecPath);

                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "OldRecPath中数据库名不应为空";
                        goto ERROR1;
                    }

                    // 要检查看看 strDbName 是否为一个实体库名
                    if (this.IsItemDbName(strDbName) == false)
                    {
                        strError = "OldRecPath中数据库名 '" + strDbName + "' 不正确，应为实体库名";
                        goto ERROR1;
                    }
                }

                // 从数据库中读出记录

                long lRet = channel.GetRes(info.NewRecPath,
                    out strXml,
                    out string strMetaData,
                    out byte[] exist_timestamp,
                    out string strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode == ChannelErrorCode.NotFound)
                    {
                        result.ErrorCode = ErrorCodeValue.NotFound;
                        goto ERROR1;
                    }

                    goto ERROR1;
                }
            }

            XmlDocument domExist = new XmlDocument();
            try
            {
                domExist.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "strXml 装载进入 DOM 时发生错误: " + ex.Message;
                goto ERROR1;
            }

            int nRet = this.DoVerifyItemFunction(
    sessioninfo,
    "", // strAction,
    domExist,
    out strError);
            if (nRet != 0)
                errors.Add(strError);

#if NO
            string strBarcode = DomUtil.GetElementText(domExist.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(strBarcode) == false)
            {
                // 查重
                List<string> aPath = null;
                // 根据册条码号对实体库进行查重
                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                nRet = SearchItemRecDup(
                    channel,
                    strBarcode,
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

                    if (aPath[0] == strRecPath) // 正好是自己
                        bDup = false;
                    else
                        bDup = true;// 别的记录中已经使用了这个条码号
                } // end of if (nRet == 1)
                else
                {
                    Debug.Assert(nRet > 1, "");
                    bDup = true;
                }

                // 报错
                if (bDup == true)
                    errors.Add("册条码号 '" + strBarcode + "' 已经被下列册记录使用了: " + StringUtil.MakePathList(aPath));
            }
#endif
            nRet = SearchDup(channel,
            domExist,
            strRecPath,
            "barcode",
            "册条码号",
            ref errors,
            out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = SearchDup(channel,
domExist,
strRecPath,
"refID",
"参考ID",
ref errors,
out strError);
            if (nRet == -1)
                goto ERROR1;

            if (this.VerifyRegisterNoDup)
            {
                nRet = SearchDup(channel,
    domExist,
    strRecPath,
    "registerNo",
    "登录号",
    ref errors,
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            if (errors.Count > 0)
            {
                result.ErrorInfo = StringUtil.MakePathList(errors, "; ");
                return 1;
            }

            result.ErrorCode = ErrorCodeValue.NoError;
            return 0;
        ERROR1:
            result.ErrorInfo = strError;
            return -1;
        }

        int SearchDup(RmsChannel channel,
            XmlDocument domExist,
            string strRecPath,
            string strElementName,
            string strFrom,
            ref List<string> errors,
            out string strError)
        {
            strError = "";

            string strContent = DomUtil.GetElementText(domExist.DocumentElement, strElementName);
            if (string.IsNullOrEmpty(strContent) == false)
            {
                // 查重
                // 对实体库进行查重
                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                int nRet = SearchItemRecDup(
                    channel,
                    strContent,
                    strFrom,
                    100,
                    out List<string> aPath,
                    out strError);
                if (nRet == -1)
                    return -1;

                bool bDup = false;
                if (nRet == 0)
                {
                    bDup = false;
                }
                else if (nRet == 1) // 命中一条
                {
                    Debug.Assert(aPath.Count == 1, "");

                    if (aPath[0] == strRecPath) // 正好是自己
                        bDup = false;
                    else
                        bDup = true;// 别的记录中已经使用了这个条码号
                } // end of if (nRet == 1)
                else
                {
                    Debug.Assert(nRet > 1, "");
                    bDup = true;
                }

                // 报错
                if (bDup == true)
                    errors.Add(strFrom + " '" + strContent + "' 已经被下列册记录使用了: " + StringUtil.MakePathList(aPath));
            }

            return 0;
        }

        // 设置/保存册信息
        // parameters:
        //      strBiblioRecPath    书目记录路径，仅包含库名和id部分。库名可以用来确定书目库，id可以被实体记录用来设置<parent>元素内容。另外书目库名和EntityInfo中的NewRecPath形成映照关系，需要检查它们是否正确对应
        //      entityinfos 要提交的的实体信息数组
        //      EntityInfo.Style    onlyWriteLog 只写入操作日志，不修改册记录 (2020/10/14)
        // 权限：需要有setentities权限
        // TODO: 写入册库中的记录, 还缺乏<operator>和<operTime>字段
        // TODO: 需要检查册记录的<parent>元素内容是否合法。不能为问号
        public LibraryServerResult SetEntities(
            SessionInfo sessioninfo,
            string strBiblioRecPath,
            EntityInfo[] entityinfos,
            out EntityInfo[] errorinfos)
        {
            errorinfos = null;

            LibraryServerResult result = new LibraryServerResult();

            // 权限字符串
            if (StringUtil.IsInList("setentities,setiteminfo,order", sessioninfo.RightsOrigin) == false)
            {
                result.Value = -1;
                result.ErrorInfo = "保存册信息 操作被拒绝。不具备 order、setiteminfo 或 setentities 权限。";
                result.ErrorCode = ErrorCode.AccessDenied;
                return result;
            }

            // 个人书斋名
            string strPersonalLibrary = "";
            if (sessioninfo.UserType == "reader"
                && sessioninfo.Account != null)
            {
                strPersonalLibrary = sessioninfo.Account.PersonalLibrary;
                if (string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    result.Value = -1;
                    result.ErrorInfo = "保存册信息 操作被拒绝。读者身份不具备个人书斋权限";
                    result.ErrorCode = ErrorCode.AccessDenied;
                    return result;
                }
            }

            int nRet = 0;
            long lRet = 0;
            string strError = "";

            string strBiblioDbName = ResPath.GetDbName(strBiblioRecPath);
            string strBiblioRecId = ResPath.GetRecordId(strBiblioRecPath);

            // 获得书目库对应的实体库名
            nRet = this.GetItemDbName(strBiblioDbName,
                 out string strItemDbName,
                 out strError);
            if (nRet == -1)
                goto ERROR1;

            // 检查实体库名 2014/9/5
            if (string.IsNullOrEmpty(strBiblioDbName) == false
                && string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "书目库 '" + strBiblioDbName + "' 不具备下属的实体库，设置实体记录的操作失败";
                goto ERROR1;
            }

            RmsChannel channel = sessioninfo.Channels.GetChannel(this.WsUrl);
            if (channel == null)
            {
                strError = "get channel error";
                goto ERROR1;
            }

            byte[] output_timestamp = null;
            string strOutputPath = "";

            List<EntityInfo> ErrorInfos = new List<EntityInfo>();

            foreach (EntityInfo info in entityinfos)
            {
                string strAction = info.Action;

                if (strAction == "verify")
                {
                    nRet = VerifyEntity(
                        sessioninfo,
                        info,
                        out EntityInfo verify_result,
                        out strError);
                    if (nRet == -1)
                    {
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = strError;
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                        continue;
                    }
                    ErrorInfos.Add(verify_result);
                    continue;
                }

                bool bForce = false;    // 是否为强制操作(强制操作不去除源记录中的流通信息字段内容)
                bool bNoCheckDup = false;   // 是否为不查重?
                bool bNoEventLog = false;   // 是否为不记入事件日志?
                bool bNoOperations = false; // 是否为不要覆盖<operations>内容
                bool bSimulate = StringUtil.IsInList("simulate", info.Style);     // 是否为模拟操作? 2015/6/9
                bool bAutoPostfix = StringUtil.IsInList("autopostfix", info.Style); // 是否为自动加入后缀模式。指为发生重复的册条码号，登录号，自动添加后缀字符串

                string batchNo = StringUtil.GetParameterByPrefix(info.Style, "batchNo");

                string strStyle = info.Style;

                // 加工 style 字符串，便于写入日志
                //if (bAutoPostfix)
                //    StringUtil.SetInList(ref strStyle, "autopostfix", true);

                if (info.Action == "forcenew"
                    || info.Action == "forcechange"
                    || info.Action == "forcedelete")
                {
                    bForce = true;

                    // 将strAction内容修改为不带有force前缀部分
                    info.Action = info.Action.Remove(0, "force".Length);

                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改册信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改册信息的" + strAction + "操作被拒绝。只有全局用户并具备restore权限才能进行这样的操作。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 加工 style 字符串，便于写入日志
                    if (StringUtil.IsInList("force", strStyle) == false)
                        StringUtil.SetInList(ref strStyle, "force", true);

                }
                // 2008/10/6 
                else if (StringUtil.IsInList("force", info.Style) == true)
                {
                    bForce = true;

                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'force' 的修改册信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "修改册信息的" + strAction + "操作被拒绝。只有全局用户并具备restore权限才能进行这样的操作。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (bAutoPostfix && info.Action != "new")
                {
                    strError = "Style 'autopostfix' 只能在当 Action 为 'new' 或 'forcenew' 时使用";
                    goto ERROR1;
                }

                // 2008/10/6 
                if (StringUtil.IsInList("nocheckdup", info.Style) == true)
                {
                    bNoCheckDup = true;
                }

                if (StringUtil.IsInList("noeventlog", info.Style) == true)
                {
                    bNoEventLog = true;
                }

                if (StringUtil.IsInList("nooperations", info.Style) == true)
                {
                    bNoOperations = true;
                }

                if (bNoCheckDup == true)
                {
                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'nocheckdup' 的修改册信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }

                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'nocheckdup' 的修改册信息的" + strAction + "操作被拒绝。只有全局用户并具备 restore 权限才能进行这样的操作。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                if (bNoEventLog == true)
                {
                    if (StringUtil.IsInList("restore", sessioninfo.RightsOrigin) == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'noeventlog' 的修改册信息的" + strAction + "操作被拒绝。不具备 restore 权限。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                    // 2012/9/11
                    if (sessioninfo.GlobalUser == false)
                    {
                        result.Value = -1;
                        result.ErrorInfo = "带有风格 'noeventlog' 的修改册信息的" + strAction + "操作被拒绝。只有全局用户并具备restore权限才能进行这样的操作。";
                        result.ErrorCode = ErrorCode.AccessDenied;
                        return result;
                    }
                }

                // 对 info 内的参数进行检查。
                strError = "";

                // 2008/2/17 
                if (entityinfos.Length > 1  // 2013/9/26 只有一个记录的时候，不必依靠 refid 定位返回信息，因而也就不需要调用者明显给出这个 RefID 成员了
                    && String.IsNullOrEmpty(info.RefID) == true)
                {
                    strError = "info.RefID 没有给出";
                }

                if (info.NewRecPath != null
                    && info.NewRecPath.IndexOf(",") != -1)
                {
                    strError = "info.NewRecPath 值 '" + info.NewRecPath + "' 中不能包含逗号";
                }
                else if (info.OldRecPath != null
                    && info.OldRecPath.IndexOf(",") != -1)
                {
                    strError = "info.OldRecPath 值 '" + info.OldRecPath + "' 中不能包含逗号";
                }

                // 当操作为"delete"时，是否可以允许只设置OldRecPath，而不必设置NewRecPath
                // 如果两个都设置，则要求设置为一致的。
                // 2007/11/12 
                if (info.Action == "delete")
                {
                    if (String.IsNullOrEmpty(info.NewRecord) == false)
                    {
                        strError = "strAction值为delete时, info.NewRecord参数必须为空";
                    }
                    else if (info.NewTimestamp != null)
                    {
                        strError = "strAction值为delete时, info.NewTimestamp参数必须为空";
                    }
                    // 2008/6/24 
                    else if (String.IsNullOrEmpty(info.NewRecPath) == false)
                    {
                        if (info.NewRecPath != info.OldRecPath)
                        {
                            strError = "strAction值为delete时, 如果info.NewRecPath不空，则其内容必须和info.OldRecPath一致。(info.NewRecPath='" + info.NewRecPath + "' info.OldRecPath='" + info.OldRecPath + "')";
                        }
                    }
                }
                else
                {
                    // 非delete情况 info.NewRecord则必须不为空
                    if (String.IsNullOrEmpty(info.NewRecord) == true)
                    {
                        strError = "strAction值为" + info.Action + "时, info.NewRecord参数不能为空";
                    }
                }

                if (info.Action == "new")
                {
                    if (String.IsNullOrEmpty(info.OldRecord) == false)
                    {
                        strError = "strAction值为new时, info.OldRecord参数必须为空";
                    }
                    else if (info.OldTimestamp != null)
                    {
                        strError = "strAction值为new时, info.OldTimestamp参数必须为空";
                    }
                }

                if (string.IsNullOrEmpty(strError) == false)
                {
                    EntityInfo error = new EntityInfo(info)
                    {
                        ErrorInfo = strError,
                        ErrorCode = ErrorCodeValue.CommonError
                    };
                    ErrorInfos.Add(error);
                    continue;
                }

                // 检查路径中的库名部分
                if (String.IsNullOrEmpty(info.NewRecPath) == false)
                {
                    strError = "";

                    string strDbName = ResPath.GetDbName(info.NewRecPath);

                    if (String.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "NewRecPath中数据库名不应为空";
                    }

                    if (string.IsNullOrEmpty(strItemDbName) == false    // 有可能前面 strBiblioRecPath 为空，则 strItemDbName 也为空
                        && strDbName != strItemDbName)
                    {
                        // 检测是否为其他语言的等同库名
                        // parameters:
                        //      strDbName   要检测的数据库名
                        //      strNeutralDbName    已知的中立语言数据库名
                        if (this.IsOtherLangName(strDbName,
                            strItemDbName) == false)
                        {
                            if (strAction == "copy" || strAction == "move")
                            {
                                // 再看strDbName是否至少是一个实体库
                                if (this.IsItemDbName(strDbName) == false)
                                    strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为实体库名";
                            }
                            else
                                strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为 '" + strItemDbName + "'。(因为书目库名为 '" + strBiblioDbName + "'，其对应的实体库名应为 '" + strItemDbName + "' )";
                        }
                    }
                    else if (string.IsNullOrEmpty(strItemDbName) == true)   // 2013/9/26
                    {
                        // 要检查看看 strDbName 是否为一个实体库名
                        if (this.IsItemDbName(strDbName) == false)
                            strError = "RecPath中数据库名 '" + strDbName + "' 不正确，应为实体库名";
                    }

                    if (strError != "")
                    {
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = strError;
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                        continue;
                    }
                }

                // 把(前端发过来的)旧记录装载到DOM
                XmlDocument domOldRec = new XmlDocument();
                try
                {
                    // 用strOldRecord的目的是不想改变info.OldRecord内容, 因为后者可能被复制到输出信息中
                    string strOldRecord = info.OldRecord;
                    if (String.IsNullOrEmpty(strOldRecord) == true)
                        strOldRecord = "<root />";

                    domOldRec.LoadXml(strOldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord XML记录装载到DOM时出错: " + ex.Message;

                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }


                // REDO_ENTITY:

                // 把要保存的新记录装载到 DOM
                XmlDocument domNewRec = new XmlDocument();
                try
                {
                    // 用strNewRecord的目的是不想改变info.NewRecord内容, 因为后者可能被复制到输出信息中
                    string strNewRecord = info.NewRecord;

                    if (String.IsNullOrEmpty(strNewRecord) == true)
                        strNewRecord = "<root />";

                    domNewRec.LoadXml(strNewRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.NewRecord XML记录装载到DOM时出错: " + ex.Message;

                    EntityInfo error = new EntityInfo(info);
                    error.ErrorInfo = strError;
                    error.ErrorCode = ErrorCodeValue.CommonError;
                    ErrorInfos.Add(error);
                    continue;
                }

                string strOldBarcode = "";
                string strNewBarcode = "";

                // 对册条码号加锁?
                // TODO: 以后可以统一改为对 refid 加锁
                string strLockBarcode = "";

                try
                {
                    // 命令new和change的共有部分 -- 条码号查重, 也需要加锁
                    // delete则需要加锁
                    if (info.Action == "new"
                        || info.Action == "change"
                        || info.Action == "transfer"
                        || info.Action == "setuid"
                        || info.Action == "delete"
                        || info.Action == "move")
                    {

                        // 仅仅用来获取一下新条码号
                        // 看看新旧册条码号是否有差异
                        // 对EntityInfo中的OldRecord和NewRecord中包含的条码号进行比较, 看看是否发生了变化(进而就需要查重)
                        // TODO: 无论是馆藏地，还是册条码号不同，都要对 (馆藏地+册条码号) 组合进行查重
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
                        {
                            strError = "CompareTwoBarcode() error : " + strError;
                            goto ERROR1;
                        }

                        if (info.Action == "new"
                            || info.Action == "change"
                            || (info.Action == "transfer" && StringUtil.IsInList("dont_lock", info.Style) == false)
                            || info.Action == "setuid"
                            || info.Action == "move")
                            strLockBarcode = strNewBarcode;
                        else if (info.Action == "delete")
                        {
                            // 顺便进行一些检查
                            if (String.IsNullOrEmpty(strNewBarcode) == false)
                            {
                                strError = "没有必要在delete操作的EntityInfo中, 包含NewRecord内容...。相反，注意一定要在OldRecord中包含即将删除的原记录";
                                goto ERROR1;
                            }
                            strLockBarcode = strOldBarcode;
                        }

                        // 加锁
                        if (String.IsNullOrEmpty(strLockBarcode) == false)
                            this.EntityLocks.LockForWrite(strLockBarcode);

#if NO
                        // 2014/1/10
                        // 检查空条码号
                        if ((info.Action == "new"
        || info.Action == "change"
        || info.Action == "move")       // delete操作不检查
    && String.IsNullOrEmpty(strNewBarcode) == true)
                        {
                            if (this.AcceptBlankItemBarcode == false)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "册条码号不能为空";
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }
#endif
                        if ((info.Action == "new"
                                || info.Action == "change"
                                || info.Action == "transfer"
                                || info.Action == "setuid"
                                || info.Action == "move")       // delete操作不校验记录
                            && bNoCheckDup == false
                            // && bSimulate == false
                            )
                        {
                            {
                                // 2012/12/31 先检查一次册记录中的馆藏地字段
                                // return:
                                //      -1  检查过程出错
                                //      0   符合要求
                                //      1   不符合要求
                                nRet = CheckItemLibraryCode(domNewRec,
                                    sessioninfo,
                                    // sessioninfo.LibraryCodeList,
                                    out string strLibraryCode,
                                    out strError);
                                if (nRet == -1)
                                {
                                    EntityInfo error = new EntityInfo(info)
                                    {
                                        ErrorInfo = "检查分馆代码时出错: " + strError,
                                        ErrorCode = ErrorCodeValue.CommonError
                                    };
                                    ErrorInfos.Add(error);
                                    // domOperLog = null;  // 表示不必写入日志
                                    continue;
                                }
                                // 对全局用户也要检查，唯独 restore 时候不检查
                                if (bForce == false && nRet != 0)
                                {
                                    EntityInfo error = new EntityInfo(info)
                                    {
                                        ErrorInfo = "即将创建的册记录内容中的馆藏地点不符合要求: " + strError,
                                        ErrorCode = ErrorCodeValue.CommonError
                                    };
                                    ErrorInfos.Add(error);
                                    // domOperLog = null;  // 表示不必写入日志
                                    continue;
                                }
                            }

                            // 2017/5/4 restore 的时候不要执行校验册记录的函数
                            if (bForce == false)
                            {
                                nRet = this.DoVerifyItemFunction(
                                    sessioninfo,
                                    strAction,
                                    domNewRec,
                                    out strError);
                                if (nRet != 0)
                                {
                                    EntityInfo error = new EntityInfo(info);
                                    error.ErrorInfo = strError;
                                    error.ErrorCode = ErrorCodeValue.CommonError;
                                    ErrorInfos.Add(error);
                                    continue;
                                }
                            }
                        }

                        // 进行册条码号查重
                        // TODO: 查重的时候要注意，如果操作类型为“move”，则可以允许查出和info.OldRecPath重的，因为它即将被删除
                        if (/*nRet == 1   // 新旧条码号不等，才查重。这样可以提高运行效率。BUG!!! 这个做法不可靠，因为前端发来的oldrecord不可信任
                            &&*/ (info.Action == "new"
                                || info.Action == "change"
                                || info.Action == "move")       // delete操作不查重
                            && String.IsNullOrEmpty(strNewBarcode) == false
                            && bNoCheckDup == false    // 2008/10/6 
                                                       // && bSimulate == false    // 要想跳过查重，可以使用 nocheckdup
                            )
                        {


#if NO
                            List<string> aPath = null;
                            // 根据册条码号对实体库进行查重
                            // 本函数只负责查重, 并不获得记录体
                            // return:
                            //      -1  error
                            //      其他    命中记录条数(不超过nMax规定的极限)
                            nRet = SearchItemRecDup(
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

                                if (info.Action == "new")
                                {
                                    if (aPath[0] == info.NewRecPath) // 正好是自己
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号
                                }
                                else if (info.Action == "change")
                                {
                                    if (info.NewRecPath != info.OldRecPath)
                                    {
                                        strError = "参数不正确。SetEntities() 当操作类型为 change 时，info.NewRecPath('" + info.NewRecPath + "') 应当和 info.OldRecPath('" + info.OldRecPath + "') 值相同";

                                        EntityInfo error = new EntityInfo(info);
                                        error.ErrorInfo = strError;
                                        error.ErrorCode = ErrorCodeValue.CommonError;
                                        ErrorInfos.Add(error);
                                        continue;
                                    }

                                    Debug.Assert(info.NewRecPath == info.OldRecPath, "当操作类型为change时，info.NewRecPath应当和info.OldRecPath相同");
                                    if (aPath[0] == info.OldRecPath) // 正好是自己
                                        bDup = false;
                                    else
                                        bDup = true;    // 别的记录中已经使用了这个条码号
                                }
                                else if (info.Action == "move")
                                {
                                    if (aPath[0] == info.OldRecPath) // 正好是源记录
                                        bDup = false;
                                    else
                                        bDup = true;// 别的记录中已经使用了这个条码号
                                }
                                else
                                {
                                    Debug.Assert(false, "这里不可能出现的info.Action值 '" + info.Action + "'");
                                }
                            } // end of if (nRet == 1)
                            else
                            {
                                Debug.Assert(nRet > 1, "");
                                bDup = true;

                                // 因为move操作不允许目标位置存在记录，所以这里就不再费力考虑了
                                // 如果将来move操作允许目标位置存在记录，则这里需要判断：无论源还是目标位置发现条码号重，都不算重。
                            }

                            // 报错
                            if (bDup == true)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "条码号 '" + strNewBarcode + "' 已经被下列册记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                continue;
                            }

#endif

                            {
                                int old_count = ErrorInfos.Count;
                                // return:
                                //      -1  出错
                                //      0   正常
                                //      1   出现问题，需要立即 continue 处理下一个 item
                                nRet = SearchDup(
                channel,
                info,
                strNewBarcode,
                "册条码号",
                ref ErrorInfos,
                out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                if (nRet == 1)
                                {
                                    if (bAutoPostfix && info.Action == "new")
                                    {
                                        strNewBarcode += "_" + ShortGuid.NewGuid().ToString().ToUpper();
                                        // domNewRec 修改 册条码号
                                        DomUtil.SetElementText(domNewRec.DocumentElement, "barcode", strNewBarcode);
                                        ModifyState(domNewRec, "数据错误");
                                        info.NewRecord = domNewRec.OuterXml;
                                        if (ErrorInfos.Count == old_count + 1)
                                            ErrorInfos.RemoveAt(ErrorInfos.Count - 1);
                                    }
                                    else
                                        continue;
                                }
                            }

                            // 2017/9/29
                            string strNewRefID = DomUtil.GetElementText(domNewRec.DocumentElement, "refID");
                            if (string.IsNullOrEmpty(strNewRefID) == false)
                            {
                                nRet = SearchDup(
    channel,
    info,
    strNewRefID,
    "参考ID",
    ref ErrorInfos,
    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                if (nRet == 1)
                                {
                                    continue;
                                }
                            }

                            if (this.VerifyRegisterNoDup)
                            {
                                // 2017/9/29
                                string strRegisterNo = DomUtil.GetElementText(domNewRec.DocumentElement, "registerNo");
                                if (string.IsNullOrEmpty(strRegisterNo) == false)
                                {
                                    int old_count = ErrorInfos.Count;
                                    nRet = SearchDup(
        channel,
        info,
        strRegisterNo,
        "登录号",
        ref ErrorInfos,
        out strError);
                                    if (nRet == -1)
                                        goto ERROR1;
                                    if (nRet == 1)
                                    {
                                        if (bAutoPostfix && info.Action == "new")
                                        {
                                            strRegisterNo += "_" + ShortGuid.NewGuid().ToString();
                                            // domNewRec 修改 登录号
                                            DomUtil.SetElementText(domNewRec.DocumentElement, "registerNo", strRegisterNo);
                                            ModifyState(domNewRec, "数据错误");
                                            info.NewRecord = domNewRec.OuterXml;
                                            if (ErrorInfos.Count == old_count + 1)
                                                ErrorInfos.RemoveAt(ErrorInfos.Count - 1);
                                        }
                                        else
                                            continue;
                                    }
                                }
                            }
                        }
                    }

                    // 准备日志DOM
                    XmlDocument domOperLog = new XmlDocument();
                    domOperLog.LoadXml("<root />");
                    DomUtil.SetElementText(domOperLog.DocumentElement, "operation", "setEntity");

                    // 兑现一个命令
                    if (info.Action == "new")
                    {
                        // 检查新记录的路径中的id部分是否正确
                        // 库名部分，前面已经统一检查过了
                        strError = "";

                        if (String.IsNullOrEmpty(info.NewRecPath) == true)
                        {
                            info.NewRecPath = strItemDbName + "/?";
                        }
                        else
                        {
                            string strID = ResPath.GetRecordId(info.NewRecPath);
                            if (String.IsNullOrEmpty(strID) == true)
                            {
                                strError = "RecPath中id部分应当为'?'";
                            }

                            if (strError != "")
                            {
                                EntityInfo error = new EntityInfo(info)
                                {
                                    ErrorInfo = strError,
                                    ErrorCode = ErrorCodeValue.CommonError
                                };
                                ErrorInfos.Add(error);
                                continue;
                            }
                        }

                        // 构造出适合保存的新册记录
                        // 主要是为了把待加工的记录中，可能出现的属于“流通信息”的字段去除，避免出现安全性问题
                        // TODO: 如果strNewXml中出现了流通字段，是否需要警告前端，甚至直接报错？因为这样可以引起前端的注意，避免前端以为自己通过新创建实体记录加借还信息“成功”了。
                        // 当然这个警告任务，也可以主要由前端自己承担，最好
                        string strNewXml = "";
                        if (bForce == false)
                        {
                            nRet = BuildNewEntityRecord(info.NewRecord,
                                out strNewXml,
                                out strError);
                            if (nRet == -1)
                            {
                                EntityInfo error = new EntityInfo(info)
                                {
                                    ErrorInfo = strError,
                                    ErrorCode = ErrorCodeValue.CommonError
                                };
                                ErrorInfos.Add(error);
                                continue;
                            }

                            // 2010/4/8
                            XmlDocument temp = new XmlDocument();
                            temp.LoadXml(strNewXml);
                            if (bForce == false && bNoOperations == false)
                            {
                                // 注意强制创建记录的时候，不要覆盖<operations>里面的内容
                                nRet = SetOperation(
                                    ref temp,
                                    "create",
                                    sessioninfo.UserID,
                                    "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    EntityInfo error = new EntityInfo(info)
                                    {
                                        ErrorInfo = strError,
                                        ErrorCode = ErrorCodeValue.CommonError
                                    };
                                    ErrorInfos.Add(error);
                                    continue;
                                }
                            }
                            strNewXml = temp.DocumentElement.OuterXml;
                        }
                        else
                        {
                            // 2008/5/29 
                            strNewXml = info.NewRecord;
                        }

                        if (bSimulate)
                        {
                            domOperLog = null;  // 表示不必写入日志
                        }

                        {

                            // 注意：即便是全局用户，也要用函数 CheckItemLibraryCode() 获得馆代码

                            // 分馆用户只能保存馆藏地点为自己管辖范围的册记录
                            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
                            // return:
                            //      -1  检查过程出错
                            //      0   符合要求
                            //      1   不符合要求
                            nRet = CheckItemLibraryCode(strNewXml,
                                sessioninfo,
                                // sessioninfo.LibraryCodeList,
                                out string strLibraryCode,
                                out strError);
                            if (nRet == -1)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "检查分馆代码时出错: " + strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                domOperLog = null;  // 表示不必写入日志
                                continue;
                            }
#if NO
                            if (sessioninfo.GlobalUser == false
                                || sessioninfo.UserType == "reader")
                            {
                                if (nRet != 0)
                                {
                                    EntityInfo error = new EntityInfo(info);
                                    /*
                                    if (nRet == -1)
                                        error.ErrorInfo = "检查分馆代码时出错: " + strError;
                                    else */
                                    error.ErrorInfo = "即将创建的册记录内容中的馆藏地点不符合要求: " + strError;
                                    error.ErrorCode = ErrorCodeValue.CommonError;
                                    ErrorInfos.Add(error);
                                    domOperLog = null;  // 表示不必写入日志
                                    continue;
                                }
                            }
#endif
                            if (bForce == false && nRet != 0)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.ErrorInfo = "即将创建的册记录内容中的馆藏地点不符合要求: " + strError;
                                error.ErrorCode = ErrorCodeValue.CommonError;
                                ErrorInfos.Add(error);
                                domOperLog = null;  // 表示不必写入日志
                                continue;
                            }

                            // 2014/7/3
                            if (this.VerifyBookType == true)
                            {
                                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                                if (String.IsNullOrEmpty(strEntityDbName) == true)
                                {
                                    strError = "从路径 '" + info.NewRecPath + "' 中获得数据库名时失败";
                                    goto ERROR1;
                                }

                                XmlDocument domTemp = new XmlDocument();
                                domTemp.LoadXml(strNewXml);

                                // 检查一个册记录的图书类型是否符合值列表要求
                                // parameters:
                                // return:
                                //      -1  检查过程出错
                                //      0   符合要求
                                //      1   不符合要求
                                nRet = CheckItemBookType(domTemp,
                                    strEntityDbName,
                                    out strError);
                                if (nRet == -1 || nRet == 1)
                                {
                                    EntityInfo error = new EntityInfo(info);
                                    error.ErrorInfo = "即将创建的册记录内容中的图书类型不符合要求: " + strError;
                                    error.ErrorCode = ErrorCodeValue.CommonError;
                                    ErrorInfos.Add(error);
                                    domOperLog = null;  // 表示不必写入日志
                                    continue;
                                }
                            }

                            lRet = channel.DoSaveTextRes(info.NewRecPath,
                                strNewXml,
                                false,   // include preamble?
                                "content" + (bSimulate ? ",simulate" : ""),
                                info.OldTimestamp,
                                out output_timestamp,
                                out strOutputPath,
                                out strError);
                            if (lRet == -1)
                            {
                                EntityInfo error = new EntityInfo(info);
                                error.NewTimestamp = output_timestamp;
                                error.ErrorInfo = "保存新记录的操作发生错误:" + strError;
                                error.ErrorCode = channel.OriginErrorCode;
                                ErrorInfos.Add(error);

                                domOperLog = null;  // 表示不必写入日志
                            }
                            else // 成功
                            {
                                if (domOperLog != null)
                                {
                                    DomUtil.SetElementText(domOperLog.DocumentElement,
        "libraryCode",
        strLibraryCode);    // 册所在的馆代码

                                    DomUtil.SetElementText(domOperLog.DocumentElement, "action", "new");

                                    if (String.IsNullOrEmpty(strStyle) == false)
                                        DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                                    // 不创建<oldRecord>元素

                                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                                        "record", strNewXml);
                                    DomUtil.SetAttr(node, "recPath", strOutputPath);
                                }

                                // 新记录保存成功，需要返回信息元素。因为需要返回新的时间戳和实际保存的记录路径

                                EntityInfo error = new EntityInfo(info);
                                error.NewRecPath = strOutputPath;

                                error.NewRecord = strNewXml;    // 所真正保存的记录，可能稍有变化, 因此需要返回给前端
                                error.NewTimestamp = output_timestamp;

                                error.ErrorInfo = "保存新记录的操作成功。NewTimeStamp中返回了新的时间戳, RecPath中返回了实际存入的记录路径。";
                                error.ErrorCode = ErrorCodeValue.NoError;
                                ErrorInfos.Add(error);
                            }
                        }
                    }
                    else if (info.Action == "change"
                        || info.Action == "transfer"
                        // || info.Action == "setuid"
                        || info.Action == "setuid") // 2020/10/12
                    {
                        if (bSimulate == true)
                        {
                            // 检查权限?
                            domOperLog = null;  // 表示不必写入日志
                        }

                        {
                            // 执行SetEntities API中的"change"操作
                            nRet = DoEntityOperChange(
                                info.Action,
                                strStyle,
                                sessioninfo,
                                channel,
                                info,
                                ref domOperLog,
                                ref ErrorInfos);
                            if (nRet == -1)
                            {
                                // 失败
                                domOperLog = null;  // 表示不必写入日志
                            }
                        }
                    }
                    else if (info.Action == "move")
                    {
                        if (bSimulate == true)
                        {
                            // 检查权限?
                            domOperLog = null;  // 表示不必写入日志
                        }

                        {
                            // 执行SetEntities API中的"move"操作
                            nRet = DoEntityOperMove(
                                strStyle,
                                sessioninfo,
                                channel,
                                info,
                                ref domOperLog,
                                ref ErrorInfos);
                            if (nRet == -1)
                            {
                                // 失败
                                domOperLog = null;  // 表示不必写入日志
                            }
                        }
                    }
                    else if (info.Action == "delete")
                    {
                        if (bSimulate == true)
                        {
                            // 检查权限?
                            domOperLog = null;  // 表示不必写入日志
                        }

                        {
                            // 删除册记录的操作
                            nRet = DoEntityOperDelete(
                                sessioninfo,
                                // bForce,
                                strStyle,
                                channel,
                                info,
                                strOldBarcode,
                                strNewBarcode,
                                domOldRec,
                                ref domOperLog,
                                ref ErrorInfos);
                            if (nRet == -1)
                            {
                                // 失败
                                domOperLog = null;  // 表示不必写入日志
                            }
                        }
                    }
                    else
                    {
                        // 不支持的命令
                        EntityInfo error = new EntityInfo(info);
                        error.ErrorInfo = "不支持的操作命令 '" + info.Action + "'";
                        error.ErrorCode = ErrorCodeValue.CommonError;
                        ErrorInfos.Add(error);
                    }

                    // 写入日志
                    if (domOperLog != null
                        && bNoEventLog == false)    // 2008/10/6 
                    {
                        // 2019/11/25
                        // 在日志记录中写入批次号
                        if (batchNo != null)
                            DomUtil.SetElementText(domOperLog.DocumentElement, "batchNo", batchNo);

                        string strOperTime = this.Clock.GetClock();
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operator",
                            sessioninfo.UserID);   // 操作者
                        DomUtil.SetElementText(domOperLog.DocumentElement, "operTime",
                            strOperTime);   // 操作时间

                        nRet = this.OperLog.WriteOperLog(domOperLog,
                            sessioninfo.ClientAddress,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "SetEntities() API 写入日志时发生错误: " + strError;
                            goto ERROR1;
                        }
                    }
                }
                finally
                {
                    if (String.IsNullOrEmpty(strLockBarcode) == false)
                        this.EntityLocks.UnlockForWrite(strLockBarcode);
                }
            }

            // 复制到结果中
            errorinfos = new EntityInfo[ErrorInfos.Count];
            for (int i = 0; i < ErrorInfos.Count; i++)
            {
                errorinfos[i] = ErrorInfos[i];
            }

            result.Value = ErrorInfos.Count;  // 返回信息的数量
            return result;
        ERROR1:
            // 这里的报错，是比较严重的错误。如果是数组中部分的请求发生的错误，则不在这里报错，而是通过返回错误信息数组的方式来表现
            result.Value = -1;
            result.ErrorInfo = strError;
            result.ErrorCode = ErrorCode.SystemError;
            return result;
        }

        #region SetEntities() 下级函数

        // 为 state 元素增加一个子串
        static void ModifyState(XmlDocument dom, string one)
        {
            string old_value = DomUtil.GetElementText(dom.DocumentElement, "state");
            string new_value = old_value;
            StringUtil.SetInList(ref new_value, one, true);
            if (old_value != new_value)
                DomUtil.SetElementText(dom.DocumentElement, "state", new_value);
        }

        // return:
        //      -1  出错
        //      0   正常
        //      1   出现重复，ErrorInfos 中已经加入了新元素
        int SearchDup(
            RmsChannel channel,
            EntityInfo info,
            string strNewBarcode,
            string strCaption,
            ref List<EntityInfo> ErrorInfos,
            out string strError)
        {
            strError = "";

            // 根据册条码号对实体库进行查重
            // 本函数只负责查重, 并不获得记录体
            // return:
            //      -1  error
            //      其他    命中记录条数(不超过nMax规定的极限)
            int nRet = SearchItemRecDup(
                // sessioninfo.Channels,
                channel,
                strNewBarcode,
                strCaption,
                100,
                out List<string> aPath,
                out strError);
            if (nRet == -1)
                return -1;

            bool bDup = false;
            if (nRet == 0)
            {
                bDup = false;
            }
            else if (nRet == 1) // 命中一条
            {
                Debug.Assert(aPath.Count == 1, "");

                if (info.Action == "new")
                {
                    if (aPath[0] == info.NewRecPath) // 正好是自己
                        bDup = false;
                    else
                        bDup = true;// 别的记录中已经使用了这个条码号
                }
                else if (info.Action == "change")
                {
                    if (info.NewRecPath != info.OldRecPath)
                    {
                        strError = "参数不正确。SetEntities() 当操作类型为 change 时，info.NewRecPath('" + info.NewRecPath + "') 应当和 info.OldRecPath('" + info.OldRecPath + "') 值相同";
#if NO
                        EntityInfo error = new EntityInfo(info)
                        {
                            ErrorInfo = strError,
                            ErrorCode = ErrorCodeValue.CommonError
                        };
                        ErrorInfos.Add(error);
                        return 1;   // continue
#endif
                        return -1;
                    }

                    Debug.Assert(info.NewRecPath == info.OldRecPath, "当操作类型为change时，info.NewRecPath应当和info.OldRecPath相同");
                    if (aPath[0] == info.OldRecPath) // 正好是自己
                        bDup = false;
                    else
                        bDup = true;    // 别的记录中已经使用了这个条码号
                }
                else if (info.Action == "move")
                {
                    if (aPath[0] == info.OldRecPath) // 正好是源记录
                        bDup = false;
                    else
                        bDup = true;// 别的记录中已经使用了这个条码号
                }
                else
                {
                    Debug.Assert(false, "这里不可能出现的info.Action值 '" + info.Action + "'");
                }
            } // end of if (nRet == 1)
            else
            {
                Debug.Assert(nRet > 1, "");
                bDup = true;

                // 因为move操作不允许目标位置存在记录，所以这里就不再费力考虑了
                // 如果将来move操作允许目标位置存在记录，则这里需要判断：无论源还是目标位置发现条码号重，都不算重。
            }

            // 报错
            if (bDup == true)
            {
                EntityInfo error = new EntityInfo(info)
                {
                    ErrorInfo = strCaption + " '" + strNewBarcode + "' 已经被下列册记录使用了: " + StringUtil.MakePathList(aPath),
                    ErrorCode = ErrorCodeValue.AlreadyExist
                };
                ErrorInfos.Add(error);
                return 1;   // continue
            }

            return 0;
        }

        // 包装后版本
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        int CheckItemLibraryCode(string strXml,
            string strLibraryCodeList,
            out string strError)
        {
            string strLibraryCode = "";
            return CheckItemLibraryCode(strXml,
                strLibraryCodeList,
                out strLibraryCode,
                out strError);
        }

        // 不但检查工作人员身份，也检查读者身份
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        int CheckItemLibraryCode(string strXml,
            SessionInfo sessioninfo,
            out string strLibraryCode,
            out string strError)
        {
            strLibraryCode = "";

            string strLibraryCodeList = "";
            if (sessioninfo.UserType == "reader")
            {
                string strPersonalLibrary = "";
                if (sessioninfo.Account != null)
                    strPersonalLibrary = sessioninfo.Account.PersonalLibrary;
                if (string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    strError = "读者身份且不具备个人书斋定义";
                    return 1;
                }

                if (sessioninfo.GlobalUser == false)
                    strLibraryCodeList = sessioninfo.LibraryCodeList + "/" + strPersonalLibrary;
                else
                    strLibraryCodeList = strPersonalLibrary;
            }
            else
                strLibraryCodeList = sessioninfo.LibraryCodeList;

            return CheckItemLibraryCode(strXml, strLibraryCodeList, out strLibraryCode, out strError);
        }

        // 检查一个册记录的馆藏地点是否符合馆代码列表要求
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strLibraryCode  [out]册记录中的馆代码
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        int CheckItemLibraryCode(string strXml,
            string strLibraryCodeList,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strLibraryCode = "";

            /*
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                return 0;
             * */

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "册记录XML装入XMLDOM时间出错: " + ex.Message;
                return -1;
            }

            return CheckItemLibraryCode(dom,
                strLibraryCodeList,
                out strLibraryCode,
                out strError);
        }

        // 包装后版本
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        int CheckItemLibraryCode(XmlDocument dom,
            string strLibraryCodeList,
            out string strError)
        {
            string strLibraryCode = "";
            return CheckItemLibraryCode(dom,
                strLibraryCodeList,
                out strLibraryCode,
                out strError);
        }

        // 不但检查工作人员身份，也检查读者身份
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        int CheckItemLibraryCode(XmlDocument dom,
            SessionInfo sessioninfo,
            out string strLibraryCode,
            out string strError)
        {
            strLibraryCode = "";

            string strLibraryCodeList = "";
            if (sessioninfo.UserType == "reader")
            {
                string strPersonalLibrary = "";
                if (sessioninfo.Account != null)
                    strPersonalLibrary = sessioninfo.Account.PersonalLibrary;
                if (string.IsNullOrEmpty(strPersonalLibrary) == true)
                {
                    strError = "读者身份且不具备个人书斋定义";
                    return 1;
                }

                // 2020/7/9
                if (sessioninfo.GlobalUser == false)
                    strLibraryCodeList = sessioninfo.LibraryCodeList + "/" + strPersonalLibrary;
                else
                    strLibraryCodeList = strPersonalLibrary;
            }
            else
                strLibraryCodeList = sessioninfo.LibraryCodeList;

            return CheckItemLibraryCode(dom, strLibraryCodeList, out strLibraryCode, out strError);
        }

        // TODO: location 海淀分馆/阅览室 应该属于 libraryCodeList 海淀分馆/*
        // 检查一个册记录的馆藏地点是否符合馆代码列表要求
        // parameters:
        //      strLibraryCodeList  当前用户管辖的馆代码列表
        //      strLibraryCode  [out]册记录中的馆代码
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        public int CheckItemLibraryCode(XmlDocument dom,
            string strLibraryCodeList,
            out string strLibraryCode,
            out string strError)
        {
            strError = "";
            strLibraryCode = "";

            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
#if NO
            // 去掉 #xxx, 部分
            if (strLocation.IndexOf("#") != -1)
            {
                string[] parts = strLocation.Split(new char[] { ',' });
                bool bFound = false;
                foreach (string s in parts)
                {
                    string strText = s.Trim();
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                    if (strText[0] == '#')
                        continue;
                    strLocation = strText;
                    break;
                }
                if (bFound == false)
                    strLocation = "";
            }
#endif
            strLocation = StringUtil.GetPureLocationString(strLocation);

            string strPureName = "";

            // 将馆藏地点字符串分解为 馆代码+地点名 两个部分
            ParseCalendarName(strLocation,
        out strLibraryCode,
        out strPureName);

            // 2016/12/31
            // 检查 strLibraryCode 是否在合法的馆藏地列表范围内
            if (IsValidLibraryCode(strLibraryCode) == false)
            {
                strError = "馆藏地 '" + strLocation + "' 中的馆代码 '" + strLibraryCode + "' 不是一个已经定义的馆代码";
                return 1;
            }

            // 先试探一下，馆藏地点字符串是否和 strLibraryCodeList 完全一致。
            // 这种检测主要是为了处理 strLibraryCodeList 传来 "西城分馆/集贤斋" 这样的个人书斋全路径的情况
            if (strLocation == strLibraryCodeList)
            {
                // TODO: 2016/4/3 适当时候需要让 strLibrayCode = strLocation，以表示尽量精确的(读者所在馆代码)身份
                // 在管辖范围内
                return 0;
            }

            // 即便是全局用户，也要到得到馆代码后函数才能返回
            if (SessionInfo.IsGlobalUser(strLibraryCodeList) == true)
                return 0;

            if (string.IsNullOrEmpty(strLibraryCode) == true)
                goto NOTMATCH;

            if (StringUtil.IsInList(strLibraryCode, strLibraryCodeList) == true)
            {
                // 在管辖范围内
                return 0;
            }
        NOTMATCH:
            strError = "馆藏地点 '" + strLocation + "' 不在 '" + strLibraryCodeList + "' 管辖范围内";
            return 1;
        }

        // 检查一个册记录的图书类型是否符合值列表要求
        // parameters:
        // return:
        //      -1  检查过程出错
        //      0   符合要求
        //      1   不符合要求
        public int CheckItemBookType(XmlDocument dom,
            string strItemDbName,
            out string strError)
        {
            strError = "";
            // int nRet = 0;
            string strLibraryCode = "";

            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            strLocation = StringUtil.GetPureLocationString(strLocation);

            string strPureName = "";
            // 解析日历名
            ParseCalendarName(strLocation,
        out strLibraryCode,
        out strPureName);

            List<string> values = null;

            var result = GetValueTable(
    strLibraryCode,
    "bookType",
    strItemDbName,
    false);
            if (result == null || result.Length == 0)
                return 0;
            values = new List<string>(result);
            GetPureValue(ref values);

#if NO
            // 试探 书目库名

            string strBiblioDbName = "";
            // 根据实体库名, 找到对应的书目库名
            // 注意，返回1的时候，strBiblioDbName也有可能为空
            // return:
            //      -1  出错
            //      0   没有找到
            //      1   找到
            nRet = GetBiblioDbNameByItemDbName(strItemDbName,
            out strBiblioDbName,
            out strError);
            if (nRet == 0 || nRet == -1)
            {
                strError = "根据实体库名 '" + strItemDbName + "' 中获得书目库名时失败";
                return -1;
            }

            values = GetOneLibraryValueTable(
                strLibraryCode,
                "bookType",
                strBiblioDbName);
            if (values.Count > 0)
                goto FOUND;

            // 试探 实体库名

            // 获得一个图书馆代码下的值列表
            // parameters:
            //      strLibraryCode  馆代码
            //      strTableName    表名。如果为空，表示任意name参数值均匹配
            //      strDbName   数据库名。如果为空，表示任意dbname参数值均匹配。
            values = GetOneLibraryValueTable(
                strLibraryCode,
                "bookType",
                strItemDbName);
            if (values.Count > 0)
                goto FOUND;

            // 试探不使用数据库名
            values = GetOneLibraryValueTable(
    strLibraryCode,
    "bookType",
    "");
            if (values.Count > 0)
                goto FOUND;


            return 0;   // 因为没有值列表，什么值都可以

            FOUND:
            GetPureValue(ref values);

#endif
            string strBookType = DomUtil.GetElementText(dom.DocumentElement,
    "bookType");

            if (string.IsNullOrEmpty(strBookType)
    && values.IndexOf("") != -1)
            {
                // 允许列表中出现 ""
                return 0;
            }
            else
            {
                if (IsInList(strBookType, values) == true)
                    return 0;
            }

            strError = "图书类型 '" + strBookType + "' 不是合法的值。应为 '" + StringUtil.MakePathList(values) + "' 之一";
            return 1;
        }

        static void GetPureValue(ref List<string> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string strText = values[i].Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                values[i] = StringUtil.GetPureSelectedValue(strText);
            }
        }

        static bool IsInList(string strBookType, List<string> values)
        {
            foreach (string strValue in values)
            {
                string strText = strValue.Trim();
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                strText = StringUtil.GetPureSelectedValue(strText);
                if (string.IsNullOrEmpty(strText) == true)
                    continue;
                if (strBookType == strText)
                    return true;
            }

            return false;
        }

#if NO
        /// <summary>
        /// 过滤掉最外面的 {} 字符
        /// </summary>
        /// <param name="strText">待过滤的字符串</param>
        /// <returns>过滤后的字符串</returns>
        static string GetPureSelectedValue(string strText)
        {
            for (; ; )
            {
                int nRet = strText.IndexOf("{");
                if (nRet == -1)
                    return strText;
                int nStart = nRet;
                nRet = strText.IndexOf("}", nStart + 1);
                if (nRet == -1)
                    return strText;
                int nEnd = nRet;
                strText = strText.Remove(nStart, nEnd - nStart + 1).Trim();
            }
        }
#endif

        /*
         * 被CompareTwoBarcode所替代
        // 对EntityInfo中的OldRecord和NewRecord中包含的条码号进行比较, 看看是否发生了变化(进而就需要查重)
        // parameters:
        //      strOldBarcode   顺便返回旧记录中的条码号
        //      strNewBarcode   顺便返回新记录中的条码号
        // return:
        //      -1  出错
        //      0   相等
        //      1   不相等
        static int CompareEntityBarcode(EntityInfo info,
            out string strOldBarcode,
            out string strNewBarcode,
            out string strError)
        {
            strError = "";

            strOldBarcode = "";
            strNewBarcode = "";

            if (String.IsNullOrEmpty(info.OldRecord) == false)
            {
                XmlDocument old_dom = new XmlDocument();
                try
                {
                    old_dom.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "装载info.OldRecord到DOM时发生错误: " + ex.Message;
                    return -1;
                }

                strOldBarcode = DomUtil.GetElementText(old_dom.DocumentElement, "barcode");
            }

            if (String.IsNullOrEmpty(info.NewRecord) == false)
            {
                XmlDocument new_dom = new XmlDocument();
                try
                {
                    new_dom.LoadXml(info.NewRecord);
                }
                catch (Exception ex)
                {
                    strError = "装载info.NewRecord到DOM时发生错误: " + ex.Message;
                    return -1;
                }

                strNewBarcode = DomUtil.GetElementText(new_dom.DocumentElement, "barcode");
            }

            if (strOldBarcode != strNewBarcode)
                return 1;   // 不相等

            return 0;   // 相等
        }
         */

        static bool HasMissingAttr(XmlDocument dom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            XmlNodeList attrs = dom.DocumentElement.SelectNodes("*/@dprms:missing", nsmgr);
            if (attrs.Count > 0)
                return true;
            return false;
        }

        // 构造出适合保存的新册记录
        // 主要是为了把待加工的记录中，可能出现的属于“流通信息”的字段去除，避免出现安全性问题
        // parameters:
        int BuildNewEntityRecord(string strOriginXml,
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

            // 2016/12/8
            // 检查XML。不应该存在 dprms:missing 属性
            if (HasMissingAttr(dom) == true)
            {
                strError = "用于创建实体记录的 XML 中不应使用 dprms:missing 属性";
                return -1;
            }

            // 流通元素名列表
            string[] element_names = new string[] {
                "borrower",
                "borrowDate",
                "borrowPeriod",
                "borrowHistory",
            };

            // 删除流通元素
            foreach (string name in element_names)
            {
#if NO
                DomUtil.SetElementText(dom.DocumentElement,
                    element_names[i], "");
#endif
                DomUtil.DeleteElement(dom.DocumentElement, name);
            }

            // 2020/9/17
            // 检查新记录中是否有超出定义范围的元素
            int nRet = CheckOutofRangeElements(dom, out strError);
            if (nRet == -1)
                return -1;

            // 2017/1/13
            DomUtil.RemoveEmptyElements(dom.DocumentElement);
            strXml = dom.OuterXml;
            return 0;
        }

        // 删除册记录的操作
        int DoEntityOperDelete(
            SessionInfo sessioninfo,
            // bool bForce,
            string strStyle,
            RmsChannel channel,
            EntityInfo info,
            string strOldBarcode,
            string strNewBarcode,   // TODO: 本参数是否可以废除?
            XmlDocument domOldRec,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            EntityInfo error = null;
            int nRet = 0;
            long lRet = 0;
            string strError = "";

            bool bForce = StringUtil.IsInList("force", strStyle);

            // 2008/6/24 
            if (String.IsNullOrEmpty(info.NewRecPath) == false)
            {
                if (info.NewRecPath != info.OldRecPath)
                {
                    strError = "action为delete时, 如果info.NewRecPath不空，则其内容必须和info.OldRecPath一致。(info.NewRecPath='" + info.NewRecPath + "' info.OldRecPath='" + info.OldRecPath + "')";
                    return -1;
                }
            }
            else
            {
                info.NewRecPath = info.OldRecPath;
            }

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            // 如果记录路径为空, 则先获得记录路径
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {

                if (String.IsNullOrEmpty(strOldBarcode) == true)
                {
                    strError = "info.OldRecord中的<barcode>元素中的册条码号，和info.RecPath参数值，不能同时为空。";
                    goto ERROR1;
                }

                // 本函数只负责查重, 并不获得记录体
                // return:
                //      -1  error
                //      其他    命中记录条数(不超过nMax规定的极限)
                nRet = this.SearchItemRecDup(
                    // sessioninfo.Channels,
                    channel,
                    strOldBarcode,
                    "册条码",
                    100,
                    out List<string> aPath,
                    out strError);
                if (nRet == -1)
                {
                    strError = "删除操作中条码号查重阶段发生错误:" + strError;
                    goto ERROR1;
                }

                if (nRet == 0)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "册条码号为 '" + strOldBarcode + "' 的册记录已不存在";
                    error.ErrorCode = ErrorCodeValue.NotFound;
                    ErrorInfos.Add(error);
                    return -1;
                }

                if (nRet > 1)
                {
                    /*
                    string[] pathlist = new string[aPath.Count];
                    aPath.CopyTo(pathlist);
                     * */

                    // 在删除操作中，遇到重复的是很平常的事情。只要
                    // info.OldRecPath能够清晰地指出要删除的那一条，就可以执行删除
                    if (String.IsNullOrEmpty(info.OldRecPath) == false)
                    {
                        if (aPath.IndexOf(info.OldRecPath) == -1)
                        {
                            strError = "条码号 '" + strOldBarcode + "' 已经被下列多条册记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'，但并不包括info.OldRecPath所指的路径 '" + info.OldRecPath + "'。删除操作失败。";
                            goto ERROR1;
                        }
                        info.NewRecPath = info.OldRecPath;
                    }
                    else
                    {
                        strError = "条码号 '" + strOldBarcode + "' 已经被下列多条册记录使用了: " + StringUtil.MakePathList(aPath)/*String.Join(",", pathlist)*/ + "'，在没有指定info.OldRecPath的情况下，无法定位和删除。";
                        goto ERROR1;
                    }
                }
                else
                {
                    Debug.Assert(nRet == 1, "");

                    info.NewRecPath = aPath[0];
                    // strEntityDbName = ResPath.GetDbName(strRecPath);
                }
            }

            Debug.Assert(String.IsNullOrEmpty(info.NewRecPath) == false, "");
            // Debug.Assert(strEntityDbName != "", "");

            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";
            string strExistingXml = "";

        REDOLOAD:

            // 先读出数据库中此位置的已有记录
            lRet = channel.GetRes(info.NewRecPath,
                out strExistingXml,
                out strMetaData,
                out exist_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "册条码号为 '" + strOldBarcode + "' 的册记录 '" + info.NewRecPath + "' 已不存在";
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "删除操作发生错误, 在读入原有记录 '" + info.NewRecPath + "' 阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
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

            // 观察已经存在的记录中，册条码号是否和strOldBarcode一致
            if (String.IsNullOrEmpty(strOldBarcode) == false)
            {
                string strExistingBarcode = DomUtil.GetElementText(domExist.DocumentElement, "barcode");
                if (strExistingBarcode != strOldBarcode)
                {
                    strError = "路径为 '" + info.NewRecPath + "' 的册记录中<barcode>元素中的册条码号 '" + strExistingBarcode + "' 和strOldXml中<barcode>元素中的册条码号 '" + strOldBarcode + "' 不一致。拒绝删除(如果允许删除，则会造成不经意删除了别的册记录的危险)。";
                    goto ERROR1;
                }
            }

            if (bForce == false)
            {
                // 观察已经存在的记录是否有流通信息
                string strDetail = "";
                bool bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                    out strDetail);

                if (bHasCirculationInfo == true)
                {
                    strError = "拟删除的册记录 '" + info.NewRecPath + "' 中包含有流通信息(" + strDetail + ")，不能删除。";
                    goto ERROR1;
                }
            }

            // 比较时间戳
            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 2008/5/29 
                if (bForce == true)
                {
                    error = new EntityInfo(info);
                    error.NewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                    error.ErrorInfo = "数据库中即将删除的册记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // 如果前端给出了旧记录，就有和库中记录进行比较的基础
                if (String.IsNullOrEmpty(info.OldRecord) == false)
                {
                    // 比较两个记录, 看看和册要害信息有关的字段是否发生了变化
                    // return:
                    //      0   没有变化
                    //      1   有变化
                    nRet = IsRegisterInfoChanged(domExist,
                        domOldRec,
                        null);
                    if (nRet == 1)
                    {

                        error = new EntityInfo(info);
                        error.NewTimestamp = exist_timestamp;   // 让前端知道库中记录实际上发生过变化
                        error.ErrorInfo = "数据库中即将删除的册记录已经发生了变化，请重新装载、仔细核对后再行删除。";
                        error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }

                info.OldTimestamp = exist_timestamp;
                info.NewTimestamp = exist_timestamp;
            }

            // 只有order权限的情况
            if (StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
            {
                // 2009/11/26 changed
                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "从路径 '" + info.NewRecPath + "' 中获得数据库名时失败";
                    goto ERROR1;
                }

                string strBiblioDbName = "";

                // 根据实体库名, 找到对应的书目库名
                // 注意，返回1的时候，strBiblioDbName也有可能为空
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetBiblioDbNameByItemDbName(strEntityDbName,
                out strBiblioDbName,
                out strError);
                if (nRet == 0 || nRet == -1)
                {
                    strError = "根据实体库名 '" + strEntityDbName + "' 中获得书目库名时失败";
                    goto ERROR1;
                }

                // BUG !!! string strBiblioDbName = ResPath.GetDbName(info.NewRecPath);

                // 非工作库
                if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                {
                    // 非工作库。要求<state>包含“加工中”
                    string strState = DomUtil.GetElementText(domExist.DocumentElement,
                        "state");
                    if (IncludeStateProcessing(strState) == false)
                    {
                        strError = "当前帐户只有order权限而没有setiteminfo(或setentities)权限，不能用delete功能删除从属于非工作库的、状态不包含“加工中”的实体记录 '" + info.NewRecPath + "'";
                        goto ERROR1;    // TODO: 如何返回AccessDenied错误码呢?
                    }
                }
            }

            string strLibraryCode = "";
            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
            // return:
            //      -1  检查过程出错
            //      0   符合要求
            //      1   不符合要求
            nRet = CheckItemLibraryCode(domExist,
                sessioninfo,
                // sessioninfo.LibraryCodeList,
                out strLibraryCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 检查旧记录是否属于管辖范围
            if (sessioninfo.GlobalUser == false)
            {
                if (nRet != 0)
                {
                    strError = "即将被删除的册记录 '" + info.NewRecPath + "' 其馆藏地点不符合要求: " + strError;
                    goto ERROR1;
                }
            }

            byte[] output_timestamp = null;

            lRet = channel.DoDeleteRes(info.NewRecPath,
                info.OldTimestamp,
                bSimulate ? "simulate" : "",
                out output_timestamp,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复删除均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.ErrorInfo = "删除操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else
            {
                if (domOperLog != null)
                {
                    // 成功
                    DomUtil.SetElementText(domOperLog.DocumentElement,
        "libraryCode",
        strLibraryCode);    // 册所在的馆代码

                    DomUtil.SetElementText(domOperLog.DocumentElement, "action", "delete");
                    if (String.IsNullOrEmpty(strStyle) == false)
                        DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                    // 不创建<record>元素

                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "oldRecord", strExistingXml);
                    DomUtil.SetAttr(node, "recPath", info.NewRecPath);
                }

                // 如果删除成功，则不必要在数组中返回表示成功的信息元素了
            }

            return 0;
        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // TODO: 如果外部没有进行查重，这里需要检查数据库中已经存在的记录的条码号和即将保存的是否发生变化，发生变化则需要追加查重
        // 执行SetEntities API中的"change"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // parameters:
        //      strAction   change 或 transfer
        // return:
        //      -1  出错
        //      0   成功
        int DoEntityOperChange(
            // bool bForce,
            string strAction,
            string strStyle,
            SessionInfo sessioninfo,
            RmsChannel channel,
            EntityInfo info,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            int nRedoCount = 0;
            EntityInfo error = null;
            bool bExist = true;    // info.RecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            bool bForce = StringUtil.IsInList("force", strStyle);

            // 检查一下路径
            if (String.IsNullOrEmpty(info.NewRecPath) == true)
            {
                strError = "info.NewRecPath中的路径不能为空";
                goto ERROR1;
            }

            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?")
            {
                strError = "info.NewRecPath路径 '" + strTargetRecId + "' 中记录ID部分不能为'?'";
                goto ERROR1;
            }
            if (String.IsNullOrEmpty(strTargetRecId) == true)
            {
                strError = "info.NewRecPath路径 '" + strTargetRecId + "' 中记录ID部分不能为空";
                goto ERROR1;
            }

            if (info.OldRecPath != info.NewRecPath)
            {
                strError = "当action为\"change\"时，info.NewRecordPath路径 '" + info.NewRecPath + "' 和info.OldRecPath '" + info.OldRecPath + "' 必须相同";
                goto ERROR1;
            }

            bool bNoOperations = false; // 是否为不要覆盖<operations>内容
            if (StringUtil.IsInList("nooperations", strStyle) == true)
            {
                bNoOperations = true;
            }

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            string strExistXml = "";
            byte[] exist_timestamp = null;
            string strOutputPath = "";
            string strMetaData = "";

        // 先读出数据库中即将覆盖位置的已有记录
        REDOLOAD:

            lRet = channel.GetRes(info.NewRecPath,
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
                    strOutputPath = info.NewRecPath;
                }
                else
                {
                    error = new EntityInfo(info)
                    {
                        ErrorInfo = "保存操作发生错误, 在读入原有记录阶段:" + strError,
                        ErrorCode = channel.OriginErrorCode
                    };
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // 把两个记录装入DOM
            XmlDocument domExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domExist.LoadXml(strExistXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecord装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            string strSourceLibraryCode = "";

            if (bExist == true)
            {
                // 只有order权限的情况
                if (StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                    && StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                    && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
                {
                    // 2009/11/26 changed
                    string strEntityDbName = ResPath.GetDbName(info.OldRecPath);
                    if (String.IsNullOrEmpty(strEntityDbName) == true)
                    {
                        strError = "从路径 '" + info.OldRecPath + "' 中获得数据库名时失败";
                        goto ERROR1;
                    }

                    // 根据实体库名, 找到对应的书目库名
                    // 注意，返回1的时候，strBiblioDbName也有可能为空
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetBiblioDbNameByItemDbName(strEntityDbName,
                    out string strBiblioDbName,
                    out strError);
                    if (nRet == 0 || nRet == -1)
                    {
                        strError = "根据实体库名 '" + strEntityDbName + "' 中获得书目库名时失败";
                        goto ERROR1;
                    }
                    // BUG !!! string strBiblioDbName = ResPath.GetDbName(info.OldRecPath);

                    // 非工作库
                    if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                    {
                        // 非工作库。要求<state>包含“加工中”
                        string strState = DomUtil.GetElementText(domExist.DocumentElement,
                            "state");
                        if (IncludeStateProcessing(strState) == false)
                        {
                            strError = "当前帐户只有 order 权限而没有 setiteminfo (或setentities)权限，不能用 change 功能修改从属于非工作库的、状态不包含“加工中”的实体记录 '" + info.OldRecPath + "'(此种记录的状态要包含“加工中”才能允许修改)";
                            goto ERROR1;
                        }
                    }
                }

                // *** 检查旧记录的馆藏地点
                // 检查一个册记录的馆藏地点是否符合馆代码列表要求
                // return:
                //      -1  检查过程出错
                //      0   符合要求
                //      1   不符合要求
                nRet = CheckItemLibraryCode(domExist,
                    sessioninfo,
                    // sessioninfo.LibraryCodeList,
                    out strSourceLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 检查旧记录是否属于管辖范围
                if (sessioninfo.GlobalUser == false
                    || sessioninfo.UserType == "reader")
                {
                    if (nRet != 0)
                    {
                        strError = "即将被修改的册记录 '" + info.NewRecPath + "' 其馆藏地点不符合要求: " + strError;
                        goto ERROR1;
                    }
                }
            }

            string strOldBarcode = "";
            string strNewBarcode = "";

            if (bExist == true) // 2009/3/9 
            {
                // 比较新旧记录的条码号是否有改变
                // return:
                //      -1  出错
                //      0   相等
                //      1   不相等
                nRet = CompareTwoBarcode(domExist,
                    domNew,
                    out strOldBarcode,
                    out strNewBarcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                bool bHasCirculationInfo = false;   // 册记录里面是否有流通信息
                                                    // bool bDetectCiculationInfo = false; // 是否已经探测过册记录中的流通信息

                // 观察已经存在的记录是否有流通信息
                bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                    out string strDetailInfo);  // strDetailInfo 关于册记录里面是否有流通信息的详细提示文字
                // bDetectCiculationInfo = true;

                if (nRet == 1)  // 册条码号有改变
                {
                    // TODO: 顺带检查一下 oi 元素是否被改变
                    // 2020/10/12
                    if (strAction == "setuid")
                    {
                        strError = "修改操作(setuid)被拒绝。册记录 '" + info.NewRecPath + "' 中册条码号元素内容不允许改变。(当前册条码号 '" + strOldBarcode + "'，试图修改为条码号 '" + strNewBarcode + "')";
                        goto ERROR1;
                    }

                    if (bHasCirculationInfo == true
                        && bForce == false
                        && strAction != "transfer" && strAction != "setuid")
                    {
                        // TODO: 可否增加允许同时修改所关联的已借阅读者记录修改能力?
                        // 值得注意的是如何记录进操作日志，将来如何进行recover的问题
                        strError = "修改操作被拒绝。因册记录 '" + info.NewRecPath + "' 中包含有流通信息(" + strDetailInfo + ")，所以修改它时册条码号元素内容不能改变。(当前册条码号 '" + strOldBarcode + "'，试图修改为条码号 '" + strNewBarcode + "')";
                        goto ERROR1;
                    }
                }

                // 记录中有流通信息时，不允许修改的元素
                string[] field_names = new string[] {
                    "location",
                    "accessNo" };

                if (bHasCirculationInfo == true)
                {
                    foreach (string field_name in field_names)
                    {
                        // 比较新旧记录中的馆藏地点是否有改变
                        // return:
                        //      -1  出错
                        //      0   相等
                        //      1   不相等
                        nRet = CompareTwoField(
                            field_name, // "location",
                            domExist,
                            domNew,
                            out string strOldLocation,
                            out string strNewLocation,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)  // 有改变
                        {
                            // 注: transfer 操作是否允许带着流通信息修改 location?

                            if (bForce == false)
                            {
                                // 值得注意的是如何记录进操作日志，将来如何进行recover的问题
                                strError = $"修改操作被拒绝。因册记录 '{  info.NewRecPath  }' 中包含有流通信息({ strDetailInfo  })，所以修改它时 {field_name} 元素内容不能改变。(当前值 '{ strOldLocation }'，试图修改为 '{ strNewLocation }')";
                                goto ERROR1;
                            }
                        }
                    }
                }

                // 比较新旧记录的状态是否有改变，如果从其他状态修改为“注销”状态，则应引起注意，后面要进行必要的检查

                // parameters:
                //      strOldState   顺便返回旧记录中的状态字符串
                //      strNewState   顺便返回新记录中的状态字符串
                // return:
                //      -1  出错
                //      0   相等
                //      1   不相等
                nRet = CompareTwoState(domExist,
                    domNew,
                    out string strOldState,
                    out string strNewState,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1 && strAction != "transfer" && strAction != "setuid")
                {
                    if ((strOldState != "注销" && strOldState != "丢失")
                        && (strNewState == "注销" || strNewState == "丢失")
                        && bForce == false)
                    {
#if NO
                        // 观察已经存在的记录是否有流通信息
                        if (bDetectCiculationInfo == false)
                        {
                            bHasCirculationInfo = IsEntityHasCirculationInfo(domExist,
                                out strDetailInfo);
                            bDetectCiculationInfo = true;
                        }
#endif

                        if (bHasCirculationInfo == true)
                        {
                            // Debug.Assert(bDetectCiculationInfo == true, "");
                            strError = "注销(或丢失)操作被拒绝。因拟被注销的册记录 '" + info.NewRecPath + "' 中包含有流通信息(" + strDetailInfo + ")。(当前册状态 '" + strOldState + "', 试图修改为新状态 '" + strNewState + "')";
                            goto ERROR1;
                        }
                    }

                    // 如果新记录状态没有包含“加工中”(而旧记录状态包含了“加工中”)，要进行预约检查
                    if (bHasCirculationInfo == false
                        && IncludeStateProcessing(strOldState) == true && IncludeStateProcessing(strNewState) == false)
                    {

                        // 察看本册预约情况, 并进行初步处理
                        // TODO: 如果为注销处理，需要通知等待者，书已经注销了，不用再等待
                        // return:
                        //      -1  error
                        //      0   没有修改
                        //      1   进行过修改
                        nRet = DoItemReturnReservationCheck(
                            false,
                            ref domNew,
                            out string strReservationReaderBarcode,
                            out string strNotifyID,
                            out strError);
                        if (nRet == -1)
                        {
                            this.WriteErrorLog("SetEntities()修改册记录 " + info.OldRecPath + " 的操作中，检查预约队列的操作失败(但是修改册记录的操作不一定失败)。错误描述: " + strError);
                            goto ERROR1;
                        }

                        if (String.IsNullOrEmpty(strReservationReaderBarcode) == false
                            && bSimulate == false)
                        {
                            // 被删除的通知记录。不用。
                            // 通知预约到书的操作
                            // 出于对读者库加锁方面的便利考虑, 单独做了此函数
                            // return:
                            //      -1  error
                            //      0   没有找到<request>元素
                            nRet = DoReservationNotify(
                                null,
                                channel,
                                strReservationReaderBarcode,
                                true,   // 需要函数内加锁
                                strNewBarcode,
                                false,  // 不在大架
                                false,  // 不需要再修改当前册记录，因为前面已经修改过了
                                strNotifyID,
                                out List<string> DeletedNotifyRecPaths,
                                out strError);
                            if (nRet == -1)
                            {
                                this.WriteErrorLog("SetEntities()修改册记录 " + info.OldRecPath + " 的操作中，检查预约队列的操作操作已经成功, 但是预约到书通知功能失败, 原因: " + strError);
                            }

                            /*
                            if (this.Statis != null)
                            this.Statis.IncreaseEntryValue(
                                strLibraryCode,
                                "出纳",
                                "预约到书册",
                                1);
                             * */
                        }

                    }
                    // endif 如果新记录状态没有包含“加工中”...
                }
            }

            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_timestamp);
            if (nRet != 0)
            {
                // 时间戳不相等了
                // 需要把info.OldRecord和strExistXml进行比较，看看和册登录有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                if (bForce == false)
                {
                    // 2020/10/12
                    string[] elements = null;
                    if (strAction == "transfer")
                        elements = transfer_entity_element_names;
                    else if (strAction == "setuid")
                        elements = setuid_entity_element_names;

                    // 比较两个记录, 看看和册登录有关的字段是否发生了变化
                    // return:
                    //      0   没有变化
                    //      1   有变化
                    nRet = IsRegisterInfoChanged(domOld,
                        domExist,
                        elements/*strAction == "transfer" ? transfer_entity_element_names : null*/);
                }

                if (nRet == 1 || bForce == true) // 2008/5/29 changed
                {
                    error = new EntityInfo(info);
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    error.OldRecord = strExistXml;
                    error.OldTimestamp = exist_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已被删除。";
                    else
                        error.ErrorInfo = "保存操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已发生过修改";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_timestamp此时已经反映了库中被修改后的记录的时间戳
            }

            // 2020/12/11
            // 检查 currentLocation 元素内容是否为 *:xxx 或者 xxx:* 形态
            nRet = MergeCurrentLocation(domExist,
    domNew,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            // 合并新旧记录
            string strNewXml = "";
            if (bForce == false)
            {
                if (bNoOperations == false)
                {
                    // 2010/4/8
                    nRet = SetOperation(
    ref domNew,
    "lastModified",
    sessioninfo.UserID,
    "",
    out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 2020/10/12
                string[] elements = null;
                if (strAction == "transfer")
                    elements = transfer_entity_element_names;
                else if (strAction == "setuid")
                    elements = setuid_entity_element_names;

                nRet = MergeTwoEntityXml(domExist,
                    domNew,
                    elements,   // strAction == "transfer" ? transfer_entity_element_names : null,
                    StringUtil.IsInList("outofrangeAsError", strStyle),
                    out strNewXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                // 2016/12/8
                // 检查 XML 中不应存在 dprms:missing 属性
                if (HasMissingAttr(domNew) == true)
                {
                    strError = "用于强行覆盖保存的实体记录的 XML 中不应使用 dprms:missing 属性";
                    goto ERROR1;
                }

                // 2008/5/29 
                strNewXml = domNew.OuterXml;
            }

            string strTargetLibraryCode = "";
            {
                // 检查一个册记录的馆藏地点是否符合馆代码列表要求
                // return:
                //      -1  检查过程出错
                //      0   符合要求
                //      1   不符合要求
                nRet = CheckItemLibraryCode(strNewXml,
                    sessioninfo,
                    // sessioninfo.LibraryCodeList,
                    out strTargetLibraryCode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                // 检查新记录是否属于管辖范围
                if (sessioninfo.GlobalUser == false
                    || sessioninfo.UserType == "reader")
                {
                    if (nRet != 0)
                    {
                        strError = "册记录新内容中的馆藏地点不符合要求: " + strError;
                        goto ERROR1;
                    }
                }
#endif
                if (bForce == false && nRet != 0)
                {
                    strError = "册记录新内容中的馆藏地点不符合要求: " + strError;
                    goto ERROR1;
                }
            }

            // 2014/7/3
            if (this.VerifyBookType == true)
            {
                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "从路径 '" + info.NewRecPath + "' 中获得数据库名时失败";
                    goto ERROR1;
                }

                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);

                // 检查一个册记录的读者类型是否符合值列表要求
                // parameters:
                // return:
                //      -1  检查过程出错
                //      0   符合要求
                //      1   不符合要求
                nRet = CheckItemBookType(domTemp,
                    strEntityDbName,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;
            }

            // 保存新记录
            byte[] output_timestamp = exist_timestamp;
            if (StringUtil.IsInList("onlyWriteLog", strStyle))
            {
                // 跳过写入册记录
                lRet = 0;
            }
            else
            {
                lRet = channel.DoSaveTextRes(info.NewRecPath,
        strNewXml,
        false,   // include preamble?
        "content" + (bSimulate ? ",simulate" : ""),
        exist_timestamp,
        out output_timestamp,
        out strOutputPath,
        out strError);
            }

            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复保存均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }

                error = new EntityInfo(info);
                error.ErrorInfo = "保存操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // 成功
            {
                if (domOperLog != null)
                {
                    DomUtil.SetElementText(domOperLog.DocumentElement,
        "libraryCode",
        strSourceLibraryCode + "," + strTargetLibraryCode);    // 册所在的馆代码

                    DomUtil.SetElementText(domOperLog.DocumentElement,
                        "action", strAction == "transfer" || strAction == "setuid" ? strAction : "change");
                    if (String.IsNullOrEmpty(strStyle) == false)
                        DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                    // 新记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "record", strNewXml);
                    DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                    // 旧记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "oldRecord", strExistXml);
                    DomUtil.SetAttr(node, "recPath", info.OldRecPath);
                }

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "保存操作成功。NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的新记录稍有差异)。";
                error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;

        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // 清除原有的操作记载
        public int ClearOperation(
            ref string strXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入DOM时发生错误: " + ex.Message;
                return -1;
            }

            // TODO: 如果根元素就是<operations>呢？
            for (; ; )
            {
                XmlNode nodeOperations = dom.DocumentElement.SelectSingleNode("operations");
                if (nodeOperations != null)
                    nodeOperations.ParentNode.RemoveChild(nodeOperations);
                else
                    break;
            }

            strXml = dom.OuterXml;
            return 0;
        }

        // 设置或者刷新一个操作记载
        // parameters:
        //      bAppend 是否以追加的方式加入新的操作信息.如果==false，表示替代一个同strOperName的原有节点
        //      nMaxCount   <operation>元素的最大数目.如果超过这个数目，则自动清除从第二个开始的若干个元素。第一个元素通常是create操作的信息，故特意保留
        public int SetOperation(
            ref string strXml,
            string strOperName,
            string strOperator,
            string strComment,
            bool bAppend,
            int nMaxCount,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入DOM时发生错误: " + ex.Message;
                return -1;
            }

            int nRet = SetOperation(
                ref dom,
                strOperName,
                strOperator,
                strComment,
                bAppend,
                nMaxCount,
                out strError);
            if (nRet == -1)
                return -1;

            strXml = dom.OuterXml;
            return nRet;
        }

        // 2011/11/30
        // 包装后版本，总是替代
        public int SetOperation(
    ref XmlDocument dom,
    string strOperName,
    string strOperator,
    string strComment,
    out string strError)
        {
            return SetOperation(
    ref dom,
    strOperName,
    strOperator,
    strComment,
    false,
    100,
    out strError);
        }

        // 设置或者刷新一个操作记载
        // parameters:
        //      bAppend 是否以追加的方式加入新的操作信息.如果==false，表示替代一个同strOperName的原有节点
        //      nMaxCount   <operation>元素的最大数目.如果超过这个数目，则自动清除从第二个开始的若干个元素。第一个元素通常是create操作的信息，故特意保留
        public int SetOperation(
            ref XmlDocument dom,
            string strOperName,
            string strOperator,
            string strComment,
            bool bAppend,
            int nMaxCount,
            out string strError)
        {
            strError = "";

            if (dom.DocumentElement == null)
            {
                strError = "dom.DocumentElement == null";
                return -1;
            }

            XmlNode nodeOperations = dom.DocumentElement.SelectSingleNode("operations");
            if (nodeOperations == null)
            {
                nodeOperations = dom.CreateElement("operations");
                dom.DocumentElement.AppendChild(nodeOperations);
            }

            XmlNode node = nodeOperations.SelectSingleNode("operation[@name='" + strOperName + "']");
            if (node == null || bAppend == true)
            {
                node = dom.CreateElement("operation");
                nodeOperations.AppendChild(node);
                DomUtil.SetAttr(node, "name", strOperName);
            }

            string strTime = this.Clock.GetClock();

            DomUtil.SetAttr(node, "time", strTime);
            DomUtil.SetAttr(node, "operator", strOperator);
            if (String.IsNullOrEmpty(strComment) == false)
                DomUtil.SetAttr(node, "comment", strComment);

            // 删除超出nMaxCount个数的<operation>元素
            XmlNodeList nodes = nodeOperations.SelectNodes("operation");
            if (nodes.Count > nMaxCount)
            {
                for (int i = 0; i < nodes.Count - nMaxCount; i++)
                {
                    if (i + 1 >= nodes.Count)
                        break;
                    XmlNode current = nodes[i + 1];
                    current.ParentNode.RemoveChild(current);
                }
            }

            return 0;
        }

        // 执行SetEntities API中的"move"操作
        // 1) 操作成功后, NewRecord中有实际保存的新记录，NewTimeStamp为新的时间戳
        // 2) 如果返回TimeStampMismatch错，则OldRecord中有库中发生变化后的“原记录”，OldTimeStamp是其时间戳
        // return:
        //      -1  出错
        //      0   成功
        int DoEntityOperMove(
            string strStyle,
            SessionInfo sessioninfo,
            RmsChannel channel,
            EntityInfo info,
            ref XmlDocument domOperLog,
            ref List<EntityInfo> ErrorInfos)
        {
            // int nRedoCount = 0;
            EntityInfo error = null;
            bool bExist = true;    // info.RecPath所指的记录是否存在?

            int nRet = 0;
            long lRet = 0;

            string strError = "";

            // 检查路径
            if (info.OldRecPath == info.NewRecPath)
            {
                strError = "当action为\"move\"时，info.NewRecordPath路径 '" + info.NewRecPath + "' 和info.OldRecPath '" + info.OldRecPath + "' 必须不相同";
                goto ERROR1;
            }

            bool bSimulate = StringUtil.IsInList("simulate", strStyle);

            // 检查即将覆盖的目标位置是不是有记录，如果有，则不允许进行move操作。
            // 如果要进行带覆盖目标位置记录功能的move操作，前端可以先执行一个delete操作，然后再执行move操作。
            // 这样规定，是为了避免过于复杂的判断逻辑，也便于前端操作者清楚操作的后果。
            // 因为如果允许move带有覆盖目标记录功能，则被覆盖的记录的预删除操作，等于进行了一次注销，但这个效用不明显，对前端操作人员准确判断事态并对后果负责(而且可能这种注销需要额外的操作权限)，不利
            bool bAppendStyle = false;  // 目标路径是否为追加形态？
            string strTargetRecId = ResPath.GetRecordId(info.NewRecPath);

            if (strTargetRecId == "?" || String.IsNullOrEmpty(strTargetRecId) == true)
                bAppendStyle = true;

            string strOutputPath = "";
            string strMetaData = "";

            if (bAppendStyle == false)
            {
                string strExistTargetXml = "";
                byte[] exist_target_timestamp = null;

                // 获取覆盖目标位置的现有记录
                lRet = channel.GetRes(info.NewRecPath,
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
                        /*
                        strExistSourceXml = "<root />";
                        exist_source_timestamp = null;
                        strOutputPath = info.NewRecPath;
                         * */
                    }
                    else
                    {
                        error = new EntityInfo(info);
                        error.ErrorInfo = "移动操作发生错误, 在读入即将覆盖的目标位置 '" + info.NewRecPath + "' 原有记录阶段:" + strError;
                        error.ErrorCode = channel.OriginErrorCode;
                        ErrorInfos.Add(error);
                        return -1;
                    }
                }
                else
                {
                    // 如果记录存在，则目前不允许这样的操作
                    strError = "移动(move)操作被拒绝。因为在即将覆盖的目标位置 '" + info.NewRecPath + "' 已经存在册记录。请先删除(delete)这条记录，再进行移动(move)操作";
                    goto ERROR1;
                }
            }

            string strExistSourceXml = "";
            byte[] exist_source_timestamp = null;

            // 先读出数据库中源位置的已有记录
            // REDOLOAD:

            lRet = channel.GetRes(info.OldRecPath,
                out strExistSourceXml,
                out strMetaData,
                out exist_source_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    /*
                    // 如果记录不存在, 则构造一条空的记录
                    bExist = false;
                    strExistSourceXml = "<root />";
                    exist_source_timestamp = null;
                    strOutputPath = info.NewRecPath;
                     * */
                    // 这种情况如果放宽，会有严重的副作用，所以不让放宽
                    strError = "移动(move)操作的源记录 '" + info.OldRecPath + "' 在数据库中不存在，所以无法进行移动操作。";
                    goto ERROR1;
                }
                else
                {
                    error = new EntityInfo(info);
                    error.ErrorInfo = "移动(move)操作发生错误, 在读入库中原有源记录(路径在info.OldRecPath) '" + info.OldRecPath + "' 阶段:" + strError;
                    error.ErrorCode = channel.OriginErrorCode;
                    ErrorInfos.Add(error);
                    return -1;
                }
            }

            // 把两个记录装入DOM

            XmlDocument domSourceExist = new XmlDocument();
            XmlDocument domNew = new XmlDocument();

            try
            {
                domSourceExist.LoadXml(strExistSourceXml);
            }
            catch (Exception ex)
            {
                strError = "strExistXml装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            try
            {
                domNew.LoadXml(info.NewRecord);
            }
            catch (Exception ex)
            {
                strError = "info.NewRecord装载进入DOM时发生错误: " + ex.Message;
                goto ERROR1;
            }

            // 观察时间戳是否发生变化
            nRet = ByteArray.Compare(info.OldTimestamp, exist_source_timestamp);
            if (nRet != 0)
            {
                // 时间戳不相等了
                // 需要把info.OldRecord和strExistXml进行比较，看看和册登录有关的元素（要害元素）值是否发生了变化。
                // 如果这些要害元素并未发生变化，就继续进行合并、覆盖保存操作

                XmlDocument domOld = new XmlDocument();

                try
                {
                    domOld.LoadXml(info.OldRecord);
                }
                catch (Exception ex)
                {
                    strError = "info.OldRecord装载进入DOM时发生错误: " + ex.Message;
                    goto ERROR1;
                }

                // 比较两个记录, 看看和册登录有关的字段是否发生了变化
                // return:
                //      0   没有变化
                //      1   有变化
                nRet = IsRegisterInfoChanged(domOld,
                    domSourceExist,
                    null);
                if (nRet == 1)
                {
                    error = new EntityInfo(info);
                    // 错误信息中, 返回了修改过的原记录和新时间戳
                    error.OldRecord = strExistSourceXml;
                    error.OldTimestamp = exist_source_timestamp;

                    if (bExist == false)
                        error.ErrorInfo = "移动操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已被删除。";
                    else
                        error.ErrorInfo = "移动操作发生错误: 数据库中的原记录 (路径为'" + info.OldRecPath + "') 已发生过修改";
                    error.ErrorCode = ErrorCodeValue.TimestampMismatch;
                    ErrorInfos.Add(error);
                    return -1;
                }

                // exist_source_timestamp此时已经反映了库中被修改后的记录的时间戳
            }

            string strSourceLibraryCode = "";
            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
            // return:
            //      -1  检查过程出错
            //      0   符合要求
            //      1   不符合要求
            nRet = CheckItemLibraryCode(domSourceExist,
                sessioninfo,
                // sessioninfo.LibraryCodeList,
                out strSourceLibraryCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 检查旧记录是否属于管辖范围
            if (sessioninfo.GlobalUser == false
                || sessioninfo.UserType == "reader")
            {
                if (nRet != 0)
                {
                    strError = "即将被移动的册记录其馆藏地点不符合要求: " + strError;
                    goto ERROR1;
                }
            }

            bool bNoOperations = false; // 是否为不要覆盖<operations>内容
            if (StringUtil.IsInList("nooperations", strStyle) == true)
            {
                bNoOperations = true;
            }

            if (bNoOperations == false)
            {
                // 2010/4/8
                // 
                nRet = SetOperation(
    ref domNew,
    "moved",
    sessioninfo.UserID,
    "",
    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 合并新旧记录
            nRet = MergeTwoEntityXml(domSourceExist,
                domNew,
                null,
                StringUtil.IsInList("outofrangeAsError", strStyle),
                out string strNewXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 只有order权限的情况
            if (StringUtil.IsInList("setiteminfo", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("setentities", sessioninfo.RightsOrigin) == false
                && StringUtil.IsInList("order", sessioninfo.RightsOrigin) == true)
            {
                // 2009/11/26 changed
                string strEntityDbName = ResPath.GetDbName(info.OldRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "从路径 '" + info.OldRecPath + "' 中获得数据库名时失败";
                    goto ERROR1;
                }

                string strBiblioDbName = "";

                // 根据实体库名, 找到对应的书目库名
                // 注意，返回1的时候，strBiblioDbName也有可能为空
                // return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                nRet = GetBiblioDbNameByItemDbName(strEntityDbName,
                out strBiblioDbName,
                out strError);
                if (nRet == 0 || nRet == -1)
                {
                    strError = "根据实体库名 '" + strEntityDbName + "' 中获得书目库名时失败";
                    goto ERROR1;
                }

                // 非工作库
                if (IsOrderWorkBiblioDb(strBiblioDbName) == false)
                {
                    // 非工作库。要求<state>包含“加工中”
                    string strState = DomUtil.GetElementText(domSourceExist.DocumentElement,
                        "state");
                    if (IncludeStateProcessing(strState) == false)
                    {
                        strError = "当前帐户只有order权限而没有setiteminfo(或setentities)权限，不能用move功能删除从属于非工作库的、状态不包含“加工中”的实体记录 '" + info.OldRecPath + "'";
                        goto ERROR1;
                    }
                }

                // TODO: 如果原样移动，目标记录并不被修改，似乎也该允许?
            }

            // 注：原来 DoCopy() 在这个位置

            // 检查一个册记录的馆藏地点是否符合馆代码列表要求
            // return:
            //      -1  检查过程出错
            //      0   符合要求
            //      1   不符合要求
            nRet = CheckItemLibraryCode(strNewXml,
                sessioninfo,
                // sessioninfo.LibraryCodeList,
                out string strTargetLibraryCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

#if NO
            // 检查新记录是否属于管辖范围
            if (sessioninfo.GlobalUser == false
                || sessioninfo.UserType == "reader")
            {
                if (nRet != 0)
                {
                    strError = "册记录新内容中的馆藏地点不符合要求: " + strError;
                    goto ERROR1;
                }
            }
#endif
            bool bForce = StringUtil.IsInList("force", strStyle);
            if (bForce == false && nRet != 0)
            {
                strError = "册记录新内容中的馆藏地点不符合要求: " + strError;
                goto ERROR1;
            }

            // 2014/7/3
            if (this.VerifyBookType == true)
            {
                string strEntityDbName = ResPath.GetDbName(info.NewRecPath);
                if (String.IsNullOrEmpty(strEntityDbName) == true)
                {
                    strError = "从路径 '" + info.NewRecPath + "' 中获得数据库名时失败";
                    goto ERROR1;
                }

                XmlDocument domTemp = new XmlDocument();
                domTemp.LoadXml(strNewXml);

                // 检查一个册记录的读者类型是否符合值列表要求
                // parameters:
                // return:
                //      -1  检查过程出错
                //      0   符合要求
                //      1   不符合要求
                nRet = CheckItemBookType(domTemp,
                    strEntityDbName,
                    out strError);
                if (nRet == -1 || nRet == 1)
                    goto ERROR1;
            }

            // ***
            // 移动记录
            // 注意移动和覆盖操作要尽量靠近，避免中途失败(引起 Undo 麻烦)

            // TODO: Copy后还要写一次？因为Copy并不写入新记录。(注：Copy/Move有时候会跨库，这样记录中<parent>需要改变)
            // 其实Copy的意义在于带走资源。否则还不如用Save+Delete
            lRet = channel.DoCopyRecord(info.OldRecPath,
                info.NewRecPath,
                true,   // bDeleteSourceRecord
                bSimulate ? "simulate" : "",
                out string strIdChangeList,
                out byte[] output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = "DoCopyRecord() error :" + strError;
                goto ERROR1;
            }

            // Debug.Assert(strOutputPath == info.NewRecPath);
            string strTargetPath = strOutputPath;

            // TODO: 这一步如果失败，理论上应该 Undo 刚才的 DoCopyRecord() 然后返回
            lRet = channel.DoSaveTextRes(strTargetPath,
                strNewXml,
                false,   // include preamble?
                "content" + (bSimulate ? ",simulate" : ""),
                output_timestamp,
                out output_timestamp,
                out strOutputPath,
                out strError);
            if (lRet == -1)
            {
                strError = $"WriteEntities() API move 操作中，实体记录 '{ info.OldRecPath }' 已经成功移动到 '{ strTargetPath }' ，但在写入新内容时发生错误: " + strError + "。注意此时被移动的册记录内容可能处在错误状态(比如 parent 元素没有来得及修改)，会引起一些系统故障";

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    // 不进行反复处理。
                    // 因为源已经移动，情况很复杂
                }

                // 仅仅写入错误日志即可。没有Undo
                this.WriteErrorLog(strError);

                /*
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    if (nRedoCount > 10)
                    {
                        strError = "反复保存(DoCopyRecord())均遇到时间戳冲突, 超过10次重试仍然失败";
                        goto ERROR1;
                    }
                    // 发现时间戳不匹配
                    // 重复进行提取已存在记录\比较的过程
                    nRedoCount++;
                    goto REDOLOAD;
                }*/


                error = new EntityInfo(info);
                error.ErrorInfo = "移动操作发生错误:" + strError;
                error.ErrorCode = channel.OriginErrorCode;
                ErrorInfos.Add(error);
                return -1;
            }
            else // 成功
            {
                info.NewRecPath = strOutputPath;    // 兑现保存的位置，因为可能有追加形式的路径

                if (domOperLog != null)
                {
                    DomUtil.SetElementText(domOperLog.DocumentElement,
        "libraryCode",
        strSourceLibraryCode + "," + strTargetLibraryCode);    // 册所在的馆代码

                    DomUtil.SetElementText(domOperLog.DocumentElement, "action", "move");
                    if (String.IsNullOrEmpty(strStyle) == false)
                        DomUtil.SetElementText(domOperLog.DocumentElement, "style", strStyle);

                    // 新记录
                    XmlNode node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "record", strNewXml);
                    DomUtil.SetAttr(node, "recPath", info.NewRecPath);

                    // 旧记录
                    node = DomUtil.SetElementText(domOperLog.DocumentElement,
                        "oldRecord", strExistSourceXml);
                    DomUtil.SetAttr(node, "recPath", info.OldRecPath);
                }

                // 保存成功，需要返回信息元素。因为需要返回新的时间戳
                error = new EntityInfo(info);
                error.NewTimestamp = output_timestamp;
                error.NewRecord = strNewXml;

                error.ErrorInfo = "移动操作成功。NewRecPath中返回了实际保存的路径, NewTimeStamp中返回了新的时间戳，NewRecord中返回了实际保存的新记录(可能和提交的源记录稍有差异)。";
                error.ErrorCode = ErrorCodeValue.NoError;
                ErrorInfos.Add(error);
            }

            return 0;
        ERROR1:
            error = new EntityInfo(info);
            error.ErrorInfo = strError;
            error.ErrorCode = ErrorCodeValue.CommonError;
            ErrorInfos.Add(error);
            return -1;
        }

        // 探测册记录中是否含有流通信息?
        // parameters:
        //      strDetail   输出详细描述信息
        static bool IsEntityHasCirculationInfo(XmlDocument dom,
            out string strDetail)
        {
            strDetail = "";
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower").Trim();

            if (String.IsNullOrEmpty(strBorrower) == true)
                return false;
            strDetail = "被读者 " + strBorrower + " 借阅";
            return true;
        }

        #endregion

#if NO
        // 根据册条码号列表，得到记录路径列表
        // 如果有条码号没有命中记录，则相应位置返回空字符串；如果有条码号命中多条记录，则相应位置返回字符'!'开头的报错信息
        public int GetItemRecPathList(
            RmsChannelCollection channels,
            string strBarcodeList,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            List<string> results = new List<string>();
            string [] barcodes = strBarcodeList.Split(new char[] {','});
            foreach (string barcode in barcodes)
            {
                string strBarcode = barcode.Trim();

                if (string.IsNullOrEmpty(strBarcode) == true)
                {
                    results.Add("");
                    continue;
                }

                string strXml = "";
                List<string> aPath = null;
                byte[] timestamp = null;
                // return:
                //      -1  error
                //      0   not found
                //      1   命中1条
                //      >1  命中多于1条
                int nRet = this.GetItemRec(
                    channels,
                    strBarcode,
                    "",
                    out strXml,
                    10,
                    out aPath,
                    out timestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    results.Add("");
                    continue;
                }

                if (nRet > 1)
                {
                    results.Add("!册条码号 '' 不唯一。命中记录 "+nRet.ToString()+" 条");
                    continue;
                }

                if (aPath == null || aPath.Count == 0)
                {
                    strError = "aPath出错";
                    return -1;
                }
                results.Add(aPath[0]);
            }

            strResult = StringUtil.MakePathList(results);
            return 1;
        }
#endif

        static int IndexOfFirst(
            List<string> list,
            string one,
            bool bIgnoreCase)
        {
            int index = 0;
            foreach (string s in list)
            {
                if (string.Compare(s, one, bIgnoreCase) == 0)
                    return index;
                index++;
            }

            return -1;
        }

        static List<int> IndexOf(
    List<string> list,
    string one,
    bool bIgnoreCase)
        {
            List<int> results = new List<int>();
            int index = 0;
            foreach (string s in list)
            {
                if (string.Compare(s, one, bIgnoreCase) == 0)
                    results.Add(index);
                index++;
            }

            return results;
        }

        // 根据册条码号列表，得到记录路径列表
        // 如果有条码号没有命中记录，则相应位置返回空字符串；如果有条码号命中多条记录，则相应位置返回字符'!'开头的报错信息
        public int GetItemRecPathList(
            // RmsChannelCollection channels,
            RmsChannel channel,
            string strDbType,
            string strFrom,
            string strWordList,
            bool bIgnoreCase,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";
            int nRet = 0;

            string[] words = strWordList.Split(new char[] { ',' });

            // 整理数组
            List<string> word_list = new List<string>();
            foreach (string word in words)
            {
                string strWord = word.Trim();
                if (string.IsNullOrEmpty(strWord) == true)
                {
                    word_list.Add("");
                    continue;
                }
                word_list.Add(strWord);
            }

            int nMaxCount = Math.Max(word_list.Count * 3, 1000);    // 至少为 1000
            List<Record> records = null;
            bool bMixRefID = false;

            // return:
            //      -1  出错
            //      0   一个也没有命中
            //      >0  命中的总个数。注意，这不一定是results中返回的元素个数。results中返回的个数还要受到nMax的限制，不一定等于全部命中个数
            nRet = this.GetItemRec(
    channel,
    strDbType,
    strWordList,
    strFrom,
    nMaxCount,
    "keyid,id,key",    // 要返回key，这样才知道是否发生了条码号重复
    out bMixRefID,
    out records,
    out strError);
            if (nRet == -1)
                return -1;

            int nHitCount = nRet;

            List<string> results = new List<string>();
            for (int i = 0; i < word_list.Count; i++)
            {
                results.Add("");
            }

#if NO
            // TODO: 如果 word_list 中有重复的
            // 按照key归并?
            foreach (Record record in records)
            {
                if (record.Keys == null || record.Keys.Length == 0)
                {
                    strError = "record.Keys error";
                    return -1;
                }

                string strKey = record.Keys[0].Key;

                int nIndex = IndexOf(word_list, strKey, bIgnoreCase);
                if (nIndex == -1)
                {
                    strError = "很奇怪出现了 key '" + strKey + "' 在wordlist '" + strWordList + "' 中没有匹配的项";
                    return -1;
                }

                // 是否发生了命中检索词重复?
                if (string.IsNullOrEmpty(results[nIndex]) == false)
                    results[nIndex] = "!" + strFrom + " '" + strKey + "' 检索命中不唯一";
                else
                {
                    Debug.Assert(string.IsNullOrEmpty(record.Path) == false, "");
                    results[nIndex] = record.Path;
                }
            }
#endif

            // 注意 word_list 中可能有重复的 key
            // 按照key归并?
            foreach (Record record in records)
            {
                if (record.Keys == null || record.Keys.Length == 0)
                {
                    strError = "record.Keys error";
                    return -1;
                }

                string strKey = record.Keys[0].Key;
                if (bMixRefID == true && record.Keys[0].From == "refID")    // 2016/5/24 增加 bMixRefID
                    strKey = "@refID:" + strKey;    // TODO: 前缀用法需要统一。比如前端发来的册条码号也故意指定了前缀怎么办？

                List<int> indices = IndexOf(word_list, strKey, bIgnoreCase);
                if (indices.Count == 0)
                {
                    strError = "1) 很奇怪出现了 key '" + strKey + "' 在wordlist '" + strWordList + "' 中没有匹配的项";
                    return -1;
                }

                foreach (int nIndex in indices)
                {
                    // 是否发生了命中检索词重复?
                    if (string.IsNullOrEmpty(results[nIndex]) == false)
                        results[nIndex] = "!" + strFrom + " '" + strKey + "' 检索命中不唯一";
                    else
                    {
                        Debug.Assert(string.IsNullOrEmpty(record.Path) == false, "");
                        results[nIndex] = record.Path;
                    }
                }
            }

#if TESTING
            ///
            records = new List<Record>();   // 测试用
            int nHitCount = 0;  // 测试用
            List<string> results = new List<string>();
            for (int i = 0; i < word_list.Count; i++)
            {
                results.Add("");
            }
            ///
#endif

            if (nHitCount > records.Count)
            {
                // 命中的全部记录没有取完，这样就可能发生有的册条码实际上命中了但没有来得及读入的情况
                // 测试的时候注意要制造这种情况进行测试

                // 将没有命中的检索词再次组织检索
                List<string> temp_words = new List<string>();
                for (int i = 0; i < results.Count; i++)
                {
                    if (string.IsNullOrEmpty(results[i]) == true)
                        temp_words.Add(word_list[i]);
                }

                if (temp_words.Count == 0)
                    goto END1;  // 目前已没有未命中的检索词，所以不用再进行检索了。这种情况可能是最后一个条码号重复命中太多造成的

                // 对这些检索词改为一个一个检索
                List<string> temp_results = new List<string>();
                foreach (string temp_word in temp_words)
                {
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   命中1条
                    //      >1  命中多于1条
                    nRet = this.GetOneItemRec(
                        channel,
                        strDbType,
                        temp_word,
                        strFrom,
                        "",
                        out string strXml,
                        10,
                        out List<string> aPath,
                        out byte[] timestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        temp_results.Add("");   // 没有命中
                    else if (nRet > 1)
                        temp_results.Add("!" + strFrom + " '" + temp_word + "' 检索命中不唯一");
                    else
                    {
                        Debug.Assert(nRet == 1, "");
                        if (aPath == null || aPath.Count < 1)
                        {
                            strError = "aPath error";
                            return -1;
                        }
                        temp_results.Add(aPath[0]);
                    }
                }

                // 插入当前结果中
                if (temp_results.Count != temp_words.Count)
                {
                    strError = "GetOneItemRec() 循环返回的结果数目和检索词个数不符合";
                    return -1;
                }

                Debug.Assert(temp_results.Count == temp_words.Count, "");

                for (int i = 0; i < temp_words.Count; i++)
                {
                    string word = temp_words[i];
#if NO
                    // TODO: 这里需要测试一下
                    int nPos = IndexOfFirst(word_list, word, bIgnoreCase);
                    if (nRet == -1)
                    {
                        strError = "很奇怪出现了 temp_word '" + word + "' 在wordlist '" + strWordList + "' 中没有匹配的项";
                        return -1;
                    }

                    results[nPos] = temp_results[i];
#endif
                    // 2016/1/6
                    List<int> indices = IndexOf(word_list, word, bIgnoreCase);
                    if (indices.Count == 0)
                    {
                        strError = "2) 很奇怪出现了 temp_word '" + word + "' 在wordlist '" + strWordList + "' 中没有匹配的项";
                        return -1;
                    }

                    foreach (int nPos in indices)
                    {
                        results[nPos] = temp_results[i];
                    }
                }
            }

        END1:
            strResult = StringUtil.MakePathList(results);
            return 1;
        }

        // 为册记录 XML 内添加 biblio 元素
        // 如果必要，补充 refID 元素
        // return:
        //      -1  出错
        //      0   没有发生修改
        //      1   发生了修改
        static int AddBiblio(string strBiblioXml,
            bool bOverwrite,
            ref string strXml,
            out string strRefID,
            out string strError)
        {
            strError = "";
            strRefID = "";

            XmlDocument item_dom = new XmlDocument();
            try
            {
                item_dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "册记录 XML 装入 XMLDOM 出错: " + ex.Message;
                return -1;
            }
            XmlElement exist_biblio = item_dom.DocumentElement.SelectSingleNode("biblio") as XmlElement;
            if (exist_biblio != null && bOverwrite == false)
                return 0;

            if (exist_biblio == null)
            {
                exist_biblio = item_dom.CreateElement("biblio");
                item_dom.DocumentElement.AppendChild(exist_biblio);
            }

            XmlDocument biblio_dom = new XmlDocument();
            try
            {
                biblio_dom.LoadXml(strBiblioXml);
            }
            catch (Exception ex)
            {
                strError = "书目记录 XML 装入 XMLDOM 出错: " + ex.Message;
                return -1;
            }

            if (biblio_dom.DocumentElement == null)
            {
                strError = "书目记录缺乏 XML 根元素";
                return -1;
            }

            exist_biblio.InnerXml = biblio_dom.DocumentElement.OuterXml;

            // TODO: 是否删除书目部分的 file operations 元素?

            DomUtil.RemoveEmptyElements(item_dom.DocumentElement, false);

            strRefID = DomUtil.GetElementText(item_dom.DocumentElement, "refID");
            if (string.IsNullOrEmpty(strRefID))
            {
                strRefID = Guid.NewGuid().ToString();
                DomUtil.SetElementText(item_dom.DocumentElement, "refID", strRefID);
            }

            strXml = item_dom.DocumentElement.OuterXml;
            return 1;
        }

        // 为实体记录添加 biblio 元素
        // return:
        //      -1  error
        //      0   没有找到属于书目记录的任何实体记录，因此也就无从修改
        //      >0  实际修改的实体记录数
        public int AddBiblioToChildEntities(RmsChannel channel,
            List<DeleteEntityInfo> entityinfos,
            string strBiblioXml,
            bool bOverwrite,
            XmlDocument domOperLog,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            if (entityinfos == null || entityinfos.Count == 0)
                return 0;

            int nDeletedCount = 0;

            XmlNode root = null;

            if (domOperLog != null)
            {
                root = domOperLog.CreateElement("changedEntityRecords");
                domOperLog.DocumentElement.AppendChild(root);
            }

            for (int i = 0; i < entityinfos.Count; i++)
            {
                DeleteEntityInfo info = entityinfos[i];

                byte[] output_timestamp = null;
                int nRedoCount = 0;
                string strRefID = "";

            REDO_CHANGE:

                string strXml = info.OldRecord;
                // 为册记录 XML 内添加 biblio 元素
                // return:
                //      -1  出错
                //      0   没有发生修改
                //      1   发生了修改
                int nRet = AddBiblio(strBiblioXml,
                    bOverwrite,
                    ref strXml,
                    out strRefID,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    continue;

                this.EntityLocks.LockForWrite(info.ItemBarcode);
                try
                {
                    byte[] baOutputTimestamp = null;
                    string strOutputRecPath = "";
                    lRet = channel.DoSaveTextRes(info.RecPath,
                        strXml,
                        false,
                        "content", // ,ignorechecktimestamp
                        info.OldTimestamp,
                        out baOutputTimestamp,
                        out strOutputRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            continue;

                        // 如果不重试，让时间戳出错暴露出来。
                        // 如果要重试，也得加上重新读入册记录并判断重新判断无借还信息才能删除

                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            if (nRedoCount > 10)
                            {
                                strError = "重试了10次还不行。修改实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                                goto ERROR1;
                            }
                            nRedoCount++;

                            // 重新读入记录
                            string strMetaData = "";
                            string strOutputPath = "";
                            string strError_1 = "";

                            lRet = channel.GetRes(info.RecPath,
                                out strXml,
                                out strMetaData,
                                out output_timestamp,
                                out strOutputPath,
                                out strError_1);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                                    continue;

                                strError = "在修改实体记录 '" + info.RecPath + "' 时发生时间戳冲突，于是自动重新获取记录，但又发生错误: " + strError_1;
                                goto ERROR1;
                            }

                            info.OldRecord = strXml;
                            info.OldTimestamp = output_timestamp;
                            goto REDO_CHANGE;
                        }

                        strError = "修改实体记录 '" + info.RecPath + "' 时发生错误: " + strError;
                        goto ERROR1;
                    }
                }
                finally
                {
                    this.EntityLocks.UnlockForWrite(info.ItemBarcode);
                }

                // 增补到日志DOM中
                if (domOperLog != null)
                {
                    Debug.Assert(root != null, "");

                    XmlNode node = domOperLog.CreateElement("record");
                    root.AppendChild(node);

                    DomUtil.SetAttr(node, "recPath", info.RecPath);
                    // 2017/3/11
                    if (string.IsNullOrEmpty(strRefID) == false)
                        DomUtil.SetAttr(node, "refID", strRefID);
                    DomUtil.SetAttr(node, "action", "storeBiblio"); // 表示创建了 biblio 元素
                }

                nDeletedCount++;
            }

            return nDeletedCount;
        ERROR1:
            return -1;
        }
    }

    // 实体信息
    public class DeleteEntityInfo
    {
        public string RecPath = ""; // 记录路径

        public string OldRecord = "";   // 旧记录
        public byte[] OldTimestamp = null;  // 旧记录对应的时间戳

        public string ItemBarcode = ""; // 册条码号
    }

    // 实体信息
    [DataContract(Namespace = "http://dp2003.com/dp2library/")]
    public class EntityInfo
    {
        [DataMember]
        public string RefID = "";  // 2008/2/17  前端发出Set...请求时给出的识别id，服务器包含在响应中，便于前端在响应后继续处理

        [DataMember]
        public string OldRecPath = "";  // 原记录路径 2007/6/2 
        [DataMember]
        public string OldRecord = "";   // 旧记录
        [DataMember]
        public byte[] OldTimestamp = null;  // 旧记录对应的时间戳

        [DataMember]
        public string NewRecPath = ""; // 新记录路径
        [DataMember]
        public string NewRecord = "";   // 新记录
        [DataMember]
        public byte[] NewTimestamp = null;  // 新记录对应的时间戳

        [DataMember]
        public string Action = "";   // 要执行的操作(get时此项无用) 值为new change delete move 4种之一。change要求OldRecPath和NewRecPath一样。move不要求两者一样。把move操作单列出来，主要是为了日志统计的便利。

        [DataMember]
        public string Style = "";   // 2008/10/6  风格。常用作附加的特性参数。例如: nocheckdup,noeventlog,force

        [DataMember]
        public string ErrorInfo = "";   // 出错信息
        [DataMember]
        public ErrorCodeValue ErrorCode = ErrorCodeValue.NoError;   // 出错码（表示属于何种类型的错误）

        public EntityInfo(EntityInfo info)
        {
            this.RefID = info.RefID;
            this.OldRecPath = info.OldRecPath;
            this.OldRecord = info.OldRecord;
            this.OldTimestamp = info.OldTimestamp;
            this.NewRecPath = info.NewRecPath;
            this.NewRecord = info.NewRecord;
            this.NewTimestamp = info.NewTimestamp;
            this.Action = info.Action;
            this.ErrorInfo = info.ErrorInfo;
            this.ErrorCode = info.ErrorCode;
        }

        public EntityInfo()
        {

        }
    }


}
