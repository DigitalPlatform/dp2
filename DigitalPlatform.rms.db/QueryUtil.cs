using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.rms
{
    public class QueryUtil
    {
        // 根据nodeItem得到检索的相关信息
        // parameter:
        //		nodeItem	配置节点
        //		strTarget	out参数，返回检索目标,会是多库的多途径
        //		strWord	    out参数，返回检索词
        //		strMatch	out参数，返回匹配方式
        //		strRelation	out参数，返回关系符
        //		strDataType	out参数，返回检索的数据类型
        //		strIdOrder	out参数，返回id的排序规则
        //		strKeyOrder	out参数，返回key的排序规则
        //		strOrderBy	out参数，返回由order与originOrder组合的排序规则，例如" keystring ASC,idstring DESC "
        //		nMax	    out参数，返回最大条数
        //		strError	out参数，返回出错信息
        // return:
        //		-1	出错
        //		0	成功
        public static int GetSearchInfo(
            XmlElement nodeItem,
            string strOutputStyle,
            out string strTarget,
            out string strWord,
            out string strMatch,
            out string strRelation,
            out string strDataType,
            out string strIdOrder,
            out string strKeyOrder,
            out string strOrderBy,
            out int nMaxCount,
            out string strHint,
            out string strTimeout,
            out string strError)
        {
            strTarget = "";
            strWord = "";
            strMatch = "";
            strRelation = "";
            strDataType = "";
            strIdOrder = "";
            strKeyOrder = "";
            strOrderBy = "";
            nMaxCount = 0;
            strHint = "";
            strTimeout = "";
            strError = "";

            bool bOutputKeyCount = StringUtil.IsInList("keycount", strOutputStyle);
            bool bOutputKeyID = StringUtil.IsInList("keyid", strOutputStyle);

            //--------------------------------------
            //调GetTarget函数，得到检索目标target节点
            XmlElement nodeTarget = QueryUtil.GetTarget(nodeItem);
            if (nodeTarget == null)
            {
                strError = $"item 元素 '{nodeItem.OuterXml}' 的祖先没有找到 target 元素";
                return -1;
            }
            strTarget = DomUtil.GetAttrDiff(nodeTarget, "list")?.Trim();
            if (strTarget == null)
            {
                strError = $"target 元素 '{nodeTarget.OuterXml}' 的 list 属性未定义";
            }
            if (string.IsNullOrEmpty(strTarget))
            {
                strError = $"target 元素 '{nodeTarget.OuterXml}' 的 list 属性值不应为空";
                return -1;
            }

            // 2015/11/8
            strHint = nodeTarget.GetAttribute("hint");

            //-------------------------------------------
            //检索文本 可以为空字符串
            XmlNode nodeWord = nodeItem.SelectSingleNode("word");
            if (nodeWord == null)
            {
                strError = $"item 元素 '{nodeItem.OuterXml}' 下级没有找到 word 元素";
                return -1;
            }
            strWord = nodeWord.InnerText.Trim();    //  // 2012/2/16
            // strWord = strWord.Trim();

            //------------------------------------
            //匹配方式
            XmlNode nodeMatch = nodeItem.SelectSingleNode("match");
            if (nodeMatch == null)
            {
                strError = "检索式的match元素未定义";
                return -1;
            }
            strMatch = nodeMatch.InnerText.Trim(); // 2012/2/16
            if (string.IsNullOrEmpty(strMatch) == true)
            {
                strError = "检索式的match元素内容不能为空字符串";
                return -1;
            }
            if (QueryUtil.CheckMatch(strMatch) == false)
            {
                strError = "检索式的match元素内容'" + strMatch + "'不合法，必须为left,middle,right,exact之一";
                return -1;
            }

            //--------------------------------------------
            //关系操作符
            XmlNode nodeRelation = nodeItem.SelectSingleNode("relation");
            if (nodeRelation == null)
            {
                strError = "检索式的relation元素未定义";
                return -1;
            }
            strRelation = nodeRelation.InnerText.Trim(); // 2012/2/16
            if (string.IsNullOrEmpty(strRelation) == true)
            {
                strError = "检索式的relation元素内容不能为空字符串";
                return -1;
            }
            strRelation = QueryUtil.ConvertLetterToOperator(strRelation);
            if (QueryUtil.CheckRelation(strRelation) == false)
            {
                strError = "检索式的relation元素内容 '" + strRelation + "' 不合法.";
                return -1;
            }

            //-------------------------------------------
            //数据类型
            XmlNode nodeDataType = nodeItem.SelectSingleNode("dataType");
            if (nodeDataType == null)
            {
                strError = "检索式的dataType元素未定义";
                return -1;
            }
            strDataType = nodeDataType.InnerText.Trim(); // 2012/2/16
            if (string.IsNullOrEmpty(strDataType) == true)
            {
                strError = "检索式的dataType元素内容不能为空字符串";
                return -1;
            }
            if (QueryUtil.CheckDataType(strDataType) == false)
            {
                strError = "检索式的dataType元素内容'" + strDataType + "'不合法，必须为string,number";
                return -1;
            }


            // ----------order可以不存在----------
            int nOrderIndex = -1;
            string strOrder = null;
            int nOriginOrderIndex = -1;
            string strOriginOrder = null;

            //id的序  //ASC:升序  //DESC:降序
            XmlNode nodeOrder = nodeItem.SelectSingleNode("order");
            // 当定义了order元素时，才会id进行排序
            if (nodeOrder != null)
            {
                string strOrderText = nodeOrder.InnerText; // 2012/2/16
                strOrderText = strOrderText.Trim().ToUpper();
                if (strOrderText != "ASC"
                    && strOrderText != "DESC")
                {
                    strError = "<order>元素值应为 ASC DESC 之一";
                    return -1;
                }

                if (String.IsNullOrEmpty(strOrderText) == false)
                {
                    // 2010/5/10
                    if (bOutputKeyCount == true)
                    {
                        strOrder = "keystring " + strOrderText;
                        nOrderIndex = DomUtil.GetIndex(nodeOrder);
                        strIdOrder = strOrderText;
                    }
                    else if (bOutputKeyID == true)
                    {
                        strOrder = "keystring " + strOrderText;
                        nOrderIndex = DomUtil.GetIndex(nodeOrder);
                        strIdOrder = strOrderText;
                    }
                    else
                    {
                        strOrder = "idstring " + strOrderText;
                        nOrderIndex = DomUtil.GetIndex(nodeOrder);
                        strIdOrder = strOrderText;
                    }
                }
            }

            //key的序  //ASC:升序  //DESC:降序
            XmlNode nodeOriginOrder = nodeItem.SelectSingleNode("originOrder");
            // 当定义了order元素时，才会id进行排序
            if (nodeOriginOrder != null)
            {
                string strOriginOrderText = nodeOriginOrder.InnerText;   // 2012/2/16
                strOriginOrderText = strOriginOrderText.Trim().ToUpper();
                if (strOriginOrderText != "ASC"
    && strOriginOrderText != "DESC")
                {
                    strError = "<originOrder>元素值应为 ASC DESC 之一";
                    return -1;
                }
                if (string.IsNullOrEmpty(strOriginOrderText) == false)
                {
                    strOriginOrder = "keystring " + strOriginOrderText;
                    nOriginOrderIndex = DomUtil.GetIndex(nodeOriginOrder);
                    strKeyOrder = strOriginOrderText;
                }
            }

            if (strOrder != null
                && strOriginOrder != null)
            {
                if (nOrderIndex == -1
                    || nOriginOrderIndex == -1)
                {
                    strError = "此时nOrderIndex和nOriginOrderIndex都不可能为-1";
                    return -1;
                }
                if (nOrderIndex == nOriginOrderIndex)
                {
                    strError = "nOrderIndex 与 nOriginOrderIndex不可能相等";
                    return -1;
                }
                if (nOrderIndex > nOriginOrderIndex)
                {
                    strOrderBy = strOrder + "," + strOriginOrder;
                }
                else
                {
                    strOrderBy = strOriginOrder + "," + strOrder;
                }
            }
            else
            {
                if (strOrder != null)
                    strOrderBy = strOrder;
                if (strOriginOrder != null)
                    strOrderBy = strOriginOrder;
            }


            //-------------------------------------------
            //最大命中数
            /*
            <item>
                ...
                <maxCount>-1</maxCount>
            </item>
            */
            //XmlNode nodeMaxCount = nodeItem.SelectSingleNode("maxCount");
            string strMaxCount = nodeItem.SelectSingleNode("maxCount")?.InnerText?.Trim();
            //if (nodeMaxCount != null)
            //    strMaxCount = nodeMaxCount.InnerText.Trim(); // 2012/2/16
            if (string.IsNullOrEmpty(strMaxCount) == true)
                strMaxCount = "-1";
            /*
                        if (strMaxCount == "")
                        {
                            strError = "检索式的maxCount元素的值为空字符串";
                            return -1;
                        }
            */
            try
            {
                nMaxCount = Convert.ToInt32(strMaxCount);
                if (nMaxCount < -1)
                {
                    strError = "xml检索式的maxCount的值'" + strMaxCount + "'不合法,必须是数值型";
                    return -1;
                }
            }
            catch
            {
                strError = "xml检索式的maxCount的值'" + strMaxCount + "'不合法,必须是数值型";
                return -1;
            }

            // 2024/12/6
            /*
            <item>
                ...
                <timeout>00:02:00</timeout>
            </item>
            */
            strTimeout = nodeItem.SelectSingleNode("timeout")?.InnerText?.Trim();
            return 0;
        }


        // 检查匹配方式字符串是否合法
        // return:
        //      true    合法
        //      false   不合法
        public static bool CheckMatch(string strMatch)
        {
            strMatch = strMatch.ToLower();

            string strMatchList = "left,middle,right,exact";
            return StringUtil.IsInList(strMatch, strMatchList);
        }

        // 注: 不等号为!=
        public static bool CheckRelation(string strRelation)
        {
            strRelation = strRelation.ToLower();

            string strRelationList = ">,>=,<,<=,=,!=,draw,range,list";
            return StringUtil.IsInList(strRelation, strRelationList);
        }

        // 检查数据类型
        public static bool CheckDataType(string strDataType)
        {
            strDataType = strDataType.ToLower();
            string strDataTypeList = "string,number";
            return StringUtil.IsInList(strDataType, strDataTypeList);
        }

        // 得到上级(最近的一个) target 元素节点
        // parameter:
        //		nodeItem    item节点
        // return:
        //		target节点，没找到返回null
        private static XmlElement GetTarget(XmlElement nodeItem)
        {
            XmlElement nodeCurrent = nodeItem;
            while (true)
            {
                if (nodeCurrent == null)
                    return null;

                if (nodeCurrent.Name == "target")
                    return nodeCurrent;

                nodeCurrent = nodeCurrent.ParentNode as XmlElement;
            }
        }

        // 将字母表示法的关系符改成符号表示法
        // parameter:
        //		strLetterRelation   字母表示法的关系符
        // return:
        //		返回符号表示法的关系符
        public static string ConvertLetterToOperator(string strLetterRelation)
        {
            string strResult = strLetterRelation;
            if (strLetterRelation == "E")
                strResult = "=";

            if (strLetterRelation == "GE")
                strResult = ">=";

            if (strLetterRelation == "LE")
                strResult = "<=";

            if (strLetterRelation == "G")
                strResult = ">";

            if (strLetterRelation == "L")
                strResult = "<";

            if (strLetterRelation == "NE")
                strResult = "!=";

            return strResult;
        }

        // 校验关系，注意可能抛出NoMatch异常
        public static int VerifyRelation(ref string strMatch,
            ref string strRelation,
            ref string strDataType)
        {
            if (strDataType == "number")
            {
                if (strMatch == "left" || strMatch == "right" || strMatch == "middle")
                {
                    NoMatchException ex =
                        new NoMatchException("匹配方式'" + strMatch + "'与数据类型" + strDataType + "不匹配");

                    strMatch = "exact";
                    //修改有两种:1.将left换成exact;2.将dataType设为string,我们先按认为dataType优先，将match改为exact

                    throw (ex);
                }
            }

            if (strDataType == "string")
            {
                if (strMatch == "left" || strMatch == "right" || strMatch == "middle")
                {
                    if (strRelation != "=")
                    {
                        NoMatchException ex =
                            new NoMatchException("关系操作符'" + strRelation + "'与数据类型" + strDataType + "和匹配方式'" + strMatch + "'不匹配");

                        //也可以将left或right改为exact，但意义不大‘
                        strRelation = "=";

                        throw (ex);
                    }
                }
            }
            return 0;
        }
    }
}
