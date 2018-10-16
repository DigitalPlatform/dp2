using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.XPath;
using System.Text.RegularExpressions;

namespace DigitalPlatform.Marc
{
    /// <summary>
    /// MARC 格式数据处理
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// MarcQuery 函数库的一些全局属性和方法
    /// </summary>
    public class MarcQuery
    {
        /// <summary>
        /// MARC 子字段符号
        /// </summary>
        public static string SUBFLD = new string((char)31, 1);
        /// <summary>
        /// MARC 字段结束符
        /// </summary>
        public static string FLDEND = new string((char)30, 1);
        /// <summary>
        /// MARC 记录结束符
        /// </summary>
        public static string RECEND = new string((char)29, 1);

        /// <summary>
        /// 缺省字符
        /// </summary>
        public static char DefaultChar = '?';

        /// <summary>
        /// 根据机内格式 MARC 字符串，创建若干 MarcInnerField 对象
        /// </summary>
        /// <param name="strText">MARC 机内格式字符串。代表内嵌字段的那个局部。例如 "$1200  $axxxx$fxxxx$1225  $axxxx"</param>
        /// <param name="strLeadingString">返回引导字符串。也就是第一个子字段符号前的部分</param>
        /// <returns>新创建的 MarcInnerField 对象数组</returns>
        public static MarcNodeList createInnerFields(
            string strText,
            out string strLeadingString)
        {
            strLeadingString = "";
            MarcNodeList results = new MarcNodeList();

            strText = strText.Replace(SUBFLD + "1", FLDEND);

            List<string> segments = new List<string>();
            StringBuilder prefix = new StringBuilder(); // 第一个 30 出现以前的一段文字
            StringBuilder segment = new StringBuilder(); // 正在追加的内容段落

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch == FLDEND[0])
                {
                    // 如果先前有累积的，推走
                    if (segment.Length > 0)
                    {
                        segments.Add(segment.ToString());
                        segment.Clear();
                    }

                    //segment.Append(ch);
                    segment.Append(SUBFLD + "1");
                }
                else
                {
                    if (segment.Length > 0 || segments.Count > 0)
                        segment.Append(ch);
                    else
                        prefix.Append(ch);// 第一个子字段符号以前的内容放在这里
                }
            }

            if (segment.Length > 0)
            {
                segments.Add(segment.ToString());
                segment.Clear();
            }

            if (prefix.Length > 0)
                strLeadingString = prefix.ToString();


            foreach (string s in segments)
            {
                string strSegment = s;

                MarcInnerField field = null;
                field = new MarcInnerField();

                // 如果长度不足 5 字符，补齐
                // 5 字符是 $1200 的意思
                if (strSegment.Length < 5)
                    strSegment = strSegment.PadRight(5, '?');

                field.Text = strSegment;
                results.add(field);
                // Debug.Assert(field.Parent == parent, "");
            }

            return results;
        }

        /// <summary>
        /// 根据机内格式 MARC 字符串，创建若干 MarcField (或 MarcOuterField) 对象
        /// </summary>
        /// <param name="strText">MARC 机内格式字符串</param>
        /// <param name="strOuterFieldDef">嵌套字段的定义。缺省为 null，表示不使用嵌套字段。这是一个列举字段名的逗号间隔的列表('*'为通配符)，或者 '@' 字符后面携带一个正则表达式</param>
        /// <returns>新创建的 MarcField 对象集合</returns>
        public static MarcNodeList createFields(
            string strText,
            string strOuterFieldDef = null)
        {
            MarcNodeList results = new MarcNodeList();

            List<string> segments = new List<string>();
            StringBuilder field_text = new StringBuilder(4096);
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch == 30 || ch == 29)
                {
                    // 上一个字段结束
                    segments.Add(field_text.ToString());
                    field_text.Clear();
                }
                else
                {
                    field_text.Append(ch);
                }
            }

            // 剩余的内容
            if (field_text.Length > 0)
            {
                segments.Add(field_text.ToString());
                field_text.Clear();
            }

            foreach (string segment in segments)
            {
                string strSegment = segment;

                // 如果长度不足 3 字符，补齐?
                if (strSegment.Length < 3)
                    strSegment = strSegment.PadRight(3, '?');

                // 创建头标区以后的普通字段
                MarcNode field = null;
                if (string.IsNullOrEmpty(strOuterFieldDef) == false)
                {
                    string strFieldName = strSegment.Substring(0, 3);
                    // return:
                    //		-1	error
                    //		0	not match
                    //		1	match
                    int nRet = MatchName(strFieldName,
                        strOuterFieldDef);
                    if (nRet == 1)
                        field = new MarcOuterField();
                    else
                        field = new MarcField();
                }
                else
                    field = new MarcField();


                field.Text = strSegment;
                results.add(field);
                // Debug.Assert(field.Parent == parent, "");
            }

            return results;
        }

        #region 匹配字段名的辅助函数

        // 比较字符串是否符合正则表达式
        static bool RegexCompare(string strPattern,
            RegexOptions regOptions,
            string strInstance)
        {
            Regex r = new Regex(strPattern, regOptions);
            System.Text.RegularExpressions.Match m = r.Match(strInstance);

            if (m.Success)
                return true;
            else
                return false;
        }

        // 匹配字段名/子字段名
        // pamameters:
        //		strName	名字
        //		strMatchCase	要匹配的要求
        // return:
        //		-1	error
        //		0	not match
        //		1	match
        static int MatchName(string strName,
            string strMatchCase)
        {
            if (strMatchCase == "")	// 如果strMatchCase为空，表示无论什么名字都匹配
                return 1;

            // Regular expression
            if (strMatchCase.Length >= 1
                && strMatchCase[0] == '@')
            {
                if (RegexCompare(strMatchCase.Substring(1),
                    RegexOptions.None,
                    strName) == true)
                    return 1;
                return 0;
            }
            else // 原来的*模式
            {
                if (CmpName(strName, strMatchCase) == 0)
                    return 1;
                return 0;
            }
        }

        // 2013/1/7
        // t的长度可以是s的整倍数
        static int CmpName(string s, string t)
        {
            if (s.Length == t.Length)
                return CmpOneName(s, t);

            if ((t.Length % s.Length) != 0)
            {
                throw new Exception("t '" + t + "'的长度 " + t.Length.ToString() + " 应当为s '" + s + "' 的长度 " + s.Length.ToString() + "  的整倍数");
            }
            int nCount = t.Length / s.Length;
            for (int i = 0; i < nCount; i++)
            {
                int nRet = CmpOneName(s, t.Substring(i * s.Length, s.Length));
                if (nRet == 0)
                    return 0;
            }

            return 1;
        }

        // 含通配符的比较
        static int CmpOneName(string s,
            string t)
        {
            int len = Math.Min(s.Length, t.Length);
            for (int i = 0; i < len; i++)
            {
                if (s[i] == '*' || t[i] == '*')
                    continue;
                if (s[i] != t[i])
                    return (s[i] - t[i]);
            }
            if (s.Length > t.Length)
                return 1;
            if (s.Length < t.Length)
                return -1;
            return 0;
        }

        #endregion

        // 使用代用字符的版本
        /// <summary>
        /// 根据机内格式 MARC 字符串，创建若干 MarcField 对象
        /// </summary>
        /// <param name="chSubfield">子字段符号代用符号</param>
        /// <param name="chFieldEnd">字段结束符代用符号</param>
        /// <param name="strText">MARC 机内格式字符串</param>
        /// <param name="strOuterFieldDef">嵌套字段的定义。缺省为 null，表示不使用嵌套字段。这是一个列举字段名的逗号间隔的列表('*'为通配符)，或者 '@' 字符后面携带一个正则表达式</param>
        /// <returns>新创建的 MarcField 对象集合</returns>
        public static MarcNodeList createFields(
            char chSubfield,
            char chFieldEnd,
            string strText,
            string strOuterFieldDef = null)
        {
            return createFields(strText.Replace(chFieldEnd, FLDEND[0]).Replace(chSubfield, SUBFLD[0]), strOuterFieldDef);
        }

        // 根据机内格式的片断字符串，构造若干MarcSubfield对象
        // parameters:
        //      parent      要赋给新创建的MarcSubfield对象的Parent值
        //      strLeadingString   [out] 第一个 31 字符以前的文本部分
        /// <summary>
        /// 根据机内格式 MARC 字符串，创建若干 MarcSubfield 对象
        /// </summary>
        /// <param name="strText">MARC 机内格式字符串</param>
        /// <param name="strLeadingString">返回前导部分字符串。也就是 strText 中第一个子字段符号以前的部分</param>
        /// <returns>新创建的 MarcSubfield 对象集合</returns>
        public static MarcNodeList createSubfields(
            string strText,
            out string strLeadingString)
        {
            strLeadingString = "";
            MarcNodeList results = new MarcNodeList();

            List<string> segments = new List<string>();
            StringBuilder prefix = new StringBuilder(); // 第一个 31 出现以前的一段文字
            StringBuilder segment = new StringBuilder(); // 正在追加的内容段落

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch == 31)
                {
                    // 如果先前有累积的，推走
                    if (segment.Length > 0)
                    {
                        segments.Add(segment.ToString());
                        segment.Clear();
                    }

                    segment.Append(ch);
                }
                else
                {
                    if (segment.Length > 0 || segments.Count > 0)
                        segment.Append(ch);
                    else
                        prefix.Append(ch);// 第一个子字段符号以前的内容放在这里
                }
            }

            if (segment.Length > 0)
            {
                segments.Add(segment.ToString());
                segment.Clear();
            }

            if (prefix.Length > 0)
                strLeadingString = prefix.ToString();

            foreach (string s in segments)
            {
                MarcSubfield subfield = new MarcSubfield();
                if (s.Length < 2)
                    subfield.Text = MarcQuery.SUBFLD + "?";  // TODO: 或者可以忽略?
                else
                    subfield.Text = s;
                results.add(subfield);
                // Debug.Assert(subfield.Parent == parent, "");
            }

            return results;
        }

        // 使用代用字符的版本
        /// <summary>
        /// 根据机内格式 MARC 字符串，创建若干 MarcSubfield 对象
        /// </summary>
        /// <param name="chSubfield">子字段符号的代用符号</param>
        /// <param name="strText">MARC 机内格式字符串</param>
        /// <param name="strLeadingString">返回前导部分字符串。也就是 strText 中第一个子字段符号以前的部分</param>
        /// <returns>新创建的 MarcSubfield 对象集合</returns>
        public static MarcNodeList createSubfields(
            char chSubfield,
            string strText,
            out string strLeadingString)
        {
            return createSubfields(strText.Replace(chSubfield, SUBFLD[0]), out strLeadingString);
        }

        /// <summary>
        /// 在目标集合中每个元素的 DOM 位置前面(同级)插入源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <param name="target_nodes">目标集合</param>
        public static void insertBefore(
            MarcNodeList source_nodes,
            MarcNodeList target_nodes)
        {
            if (source_nodes.count == 0)
                return;
            if (target_nodes.count == 0)
                return;

            if (target_nodes is ChildNodeList)
            {
                // 数组框架复制，但其中的元素不是复制而是引用
                MarcNodeList temp = new MarcNodeList();
                temp.add(target_nodes);
                target_nodes = temp;
            }

            // 先(从原有DOM位置)摘除当前集合内的全部元素
            source_nodes.detach();
            int i = 0;
            foreach (MarcNode target_node in target_nodes)
            {
                MarcNode target = target_node;
                foreach (MarcNode source_node in source_nodes)
                {
                    MarcNode source = source_node;
                    if (i > 0)  // 第一轮以后，源对象每个都要复制后插入目标位置
                    {
                        source = source.clone();
                        target.before(source);
                    }
                    else
                        target.before(source);
                    target = source;   // 插入后参考位置要顺延
                }
                i++;
            }
        }

        // 在目标集合中每个元素的DOM位置后面(同级)插入源集合内的元素
        // 注1: 如果目标集合为空，则本函数不作任何操作。这样可以防止元素被摘除但没有插入到任何位置
        // 注2：将 源 插入到 目标 元素的DOM位置后面。如果源头元素在DOM树上，则先摘除它然后插入到新位置，不是复制。
        // 注3: 如果目标集合中包含多于一个元素，则分为多轮插入。第二轮以后插入的对象，是源集合中的对象复制出来的新对象
        /// <summary>
        /// 在目标集合中每个元素的 DOM 位置后面(同级)插入源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <param name="target_nodes">目标集合</param>
        public static void insertAfter(
            MarcNodeList source_nodes,
            MarcNodeList target_nodes)
        {
            if (source_nodes.count == 0)
                return;
            if (target_nodes.count == 0)
                return;

            // record.SelectNodes("field[@name='690']")[0].ChildNodes.after(SUBFLD + "x第一个" + SUBFLD + "z第二个");
            if (target_nodes is ChildNodeList)
            {
                // 数组框架复制，但其中的元素不是复制而是引用
                MarcNodeList temp = new MarcNodeList();
                temp.add(target_nodes);
                target_nodes = temp;
            }

            // 先(从原有DOM位置)摘除当前集合内的全部元素
            source_nodes.detach();
            int i = 0;
            foreach (MarcNode target_node in target_nodes)
            {
                MarcNode target = target_node;
                foreach (MarcNode source_node in source_nodes)
                {
                    MarcNode source = source_node;
                    if (i > 0)  // 第一轮以后，源对象每个都要复制后插入目标位置
                    {
                        source = source.clone();
                        target.after(source);
                    }
                    else
                        target.after(source);
                    target = source;   // 插入后参考位置要顺延
                }
                i++;
            }
        }

        // 在目标集合中每个元素的DOM位置 下级末尾 插入源集合内的元素
        /// <summary>
        /// 在目标集合中每个元素的DOM位置 下级末尾 追加源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <param name="target_nodes">目标集合</param>
        public static void append(
    MarcNodeList source_nodes,
    MarcNodeList target_nodes)
        {
            if (source_nodes.count == 0)
                return;
            if (target_nodes.count == 0)
                return;

            // 防范目标集合被动态修改后发生foreach报错
            if (target_nodes is ChildNodeList)
            {
                // 数组框架复制，但其中的元素不是复制而是引用
                MarcNodeList temp = new MarcNodeList();
                temp.add(target_nodes);
                target_nodes = temp;
            }

            // 先(从原有DOM位置)摘除当前集合内的全部元素
            source_nodes.detach();
            int i = 0;
            foreach (MarcNode target_node in target_nodes)
            {
                foreach (MarcNode source_node in source_nodes)
                {
                    MarcNode source = source_node;
                    if (i > 0)  // 第一轮以后，源对象每个都要复制后插入目标位置
                    {
                        source = source.clone();
                        target_node.append(source);
                    }
                    else
                        target_node.append(source);
                }
                i++;
            }
        }

        // 在目标集合中每个元素的DOM位置 下级开头 插入源集合内的元素
        /// <summary>
        /// 在目标集合中每个元素的 DOM 位置下级开头插入源集合内的元素
        /// </summary>
        /// <param name="source_nodes">源集合</param>
        /// <param name="target_nodes">目标集合</param>
        public static void prepend(
    MarcNodeList source_nodes,
    MarcNodeList target_nodes)
        {
            if (source_nodes.count == 0)
                return;
            if (target_nodes.count == 0)
                return;

            // 防范目标集合被动态修改后发生foreach报错
            if (target_nodes is ChildNodeList)
            {
                // 数组框架复制，但其中的元素不是复制而是引用
                MarcNodeList temp = new MarcNodeList();
                temp.add(target_nodes);
                target_nodes = temp;
            }

            // 先(从原有DOM位置)摘除当前集合内的全部元素
            source_nodes.detach();
            int i = 0;
            foreach (MarcNode target_node in target_nodes)
            {
                foreach (MarcNode source_node in source_nodes)
                {
                    MarcNode source = source_node;
                    if (i > 0)  // 第一轮以后，源对象每个都要复制后插入目标位置
                    {
                        source = source.clone();
                        target_node.prepend(source);
                    }
                    else
                        target_node.prepend(source);
                }
                i++;
            }
        }

        /// <summary>
        /// 将 USMARC 记录从 880 模式转换为 平行模式
        /// </summary>
        /// <param name="record">要处理的记录</param>
        public static void ToParallel(MarcRecord record)
        {
            // 选定全部 880 字段
            MarcNodeList fields = record.select("field[@name='880']");
            if (fields.count == 0)
                return;

            foreach (MarcField field in fields)
            {
                string content_6 = field.select("subfield[@name='6']").FirstContent;
                if (string.IsNullOrEmpty(content_6) == true)
                    continue;

                // 拆解 $6 内容
                string strFieldName = "";
                string strNumber = "";
                string strScriptId = "";
                string strOrientation = "";
                _parseSubfield6(content_6,
            out strFieldName,
            out strNumber,
            out strScriptId,
            out strOrientation);

                if (string.IsNullOrEmpty(strScriptId) == true)
                {
                    // 修正 $6 // 例子 ISBN 9789860139976
                    strScriptId = "$1";
                    field.select("subfield[@name='6']").Content = _buildSubfield6(strFieldName, strNumber, strScriptId, strOrientation);
                }

                // 找到关联的字段
                MarcField main_field = _findField(record,
                    strFieldName,
                    field.Name,
                    strNumber);
                if (main_field != null)
                {
                    // 创建一个新的字段，把 880 字段内容复制过去
                    MarcField new_field = new MarcField(strFieldName,
                        field.Indicator, field.Content);
                    // 新字段插入到关联字段后面
                    main_field.after(new_field);
                    // 删除 880 字段
                    field.detach();

                    main_field.select("subfield[@name='6']").Content = _buildSubfield6(strFieldName, strNumber, "", "");
                }
                else
                {
                    // 找不到关联的字段，把 880 字段的字段名修改了即可
                    field.Name = strFieldName;
                }
            }
        }

        // 根据字段名和 $6 内容寻找字段
        // parameters:
        //      strFieldName    要找的字段的字段名
        //      strFieldName6   $6 子字段里面的字段名
        static MarcField _findField(
            MarcRecord record,
            string strFieldName,
            string strFieldName6,
            string strNumber,
            bool bMainField = true)
        {
            MarcNodeList fields = record.select("field[@name='" + strFieldName + "']");
            foreach (MarcField field in fields)
            {
                string content_6 = field.select("subfield[@name='6']").FirstContent;
                if (string.IsNullOrEmpty(content_6) == true)
                    continue;

                // 拆解 $6 内容
                string strCurFieldName = "";
                string strCurNumber = "";
                string strScriptId = "";
                string strOrentation = "";
                _parseSubfield6(content_6,
            out strCurFieldName,
            out strCurNumber,
            out strScriptId,
            out strOrentation);
                if (string.IsNullOrEmpty(strScriptId) == false
                    && bMainField == true)
                    continue;
                if (string.IsNullOrEmpty(strScriptId) == true
                    && bMainField == false)
                    continue;
                if (strCurFieldName == strFieldName6
                    && strCurNumber == strNumber)
                    return field;
            }

            return null;
        }

        // 构造 $6 子字段内容
        static string _buildSubfield6(string strLinkingTag,
            string strOccurrenceNumber,
            string strScriptIdCode,
            string strFieldOrientationCode)
        {
            string strResult = strLinkingTag + "-" + strOccurrenceNumber;
            if (string.IsNullOrEmpty(strScriptIdCode) == true
                && string.IsNullOrEmpty(strFieldOrientationCode) == true)
                return strResult;
            strResult += "/" + strScriptIdCode;
            if (string.IsNullOrEmpty(strFieldOrientationCode) == true)
                return strResult;
            return strResult + "/" + strFieldOrientationCode;
        }

        // 拆解 $6 子字段内容
        static void _parseSubfield6(string strText,
            out string strLinkingTag,
            out string strOccurrenceNumber,
            out string strScriptIdCode,
            out string strFieldOrientationCode)
        {
            strLinkingTag = "";
            strOccurrenceNumber = "";
            strScriptIdCode = "";
            strFieldOrientationCode = "";

            int nRet = strText.IndexOf("-");
            if (nRet == -1)
            {
                strLinkingTag = strText;
                return;
            }

            strLinkingTag = strText.Substring(0, nRet);
            strText = strText.Substring(nRet + 1);
            nRet = strText.IndexOf("/");
            if (nRet == -1)
            {
                strOccurrenceNumber = strText;
                return;
            }

            strOccurrenceNumber = strText.Substring(0, nRet);

            strText = strText.Substring(nRet + 1);
            nRet = strText.IndexOf("/");
            if (nRet == -1)
            {
                strScriptIdCode = strText;
                return;
            }

            strFieldOrientationCode = strText.Substring(nRet + 1);
        }

        /// <summary>
        /// 将 USMARC 记录从 平行模式转换为 880 模式
        /// </summary>
        /// <param name="record">要处理的记录</param>
        public static void To880(MarcRecord record)
        {
            List<MarcField> field_880s = new List<MarcField>();
            foreach (MarcField field in record.ChildNodes)
            {
                if (field.Name == "880")
                    continue;
                string content_6 = field.select("subfield[@name='6']").FirstContent;
                if (string.IsNullOrEmpty(content_6) == true)
                    continue;

                // 拆解 $6 内容
                string strFieldName = "";
                string strNumber = "";
                string strScriptId = "";
                string strOrientation = "";
                _parseSubfield6(content_6,
            out strFieldName,
            out strNumber,
            out strScriptId,
            out strOrientation);

                if (string.IsNullOrEmpty(strScriptId) == true)
                    continue;

                // 至此 field 就是并列字段

                // 找到关联的主字段
                MarcField main_field = _findField(record,
                    strFieldName,
                    field.Name,
                    strNumber);
                if (main_field != null)
                {
                    // 修改并列字段的字段名为 880
                    field.Name = "880";
                    field.select("subfield[@name='6']").Content = _buildSubfield6(main_field.Name, strNumber, strScriptId, strOrientation);

                    // 修改主字段的 $6
                    main_field.select("subfield[@name='6']").Content = _buildSubfield6("880", strNumber, "", "");

                    // 将 880 字段移动到顺次位置
                }
                else
                {
                    // 找不到关联的字段，把并列字段的字段名修改为 880 即可
                    field.Name = "880";
                }

                field_880s.Add(field);
            }

            foreach (MarcField field in field_880s)
            {
                field.detach();
            }

            foreach (MarcField field in field_880s)
            {
                record.ChildNodes.insertSequence(field,
        InsertSequenceStyle.PreferTail);
            }
        }

        /// <summary>
        /// 获得拼音字符串
        /// </summary>
        /// <param name="strHanzi">汉字字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>返回拼音字符串。如果为 null，表示出错了，strError 中有出错信息</returns>
        public delegate string Delegate_getPinyin(string strHanzi, out string strError);

        /// <summary>
        /// 创建平行字段
        /// </summary>
        /// <param name="field">要创建平行字段的，包含汉字字符串的源字段</param>
        /// <param name="bTo880">将 field 字段名转为 880</param>
        /// <param name="getPinyin">获得拼音的函数地址</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功; 1: field 不是中文的字段($6表示)，无法创建平行字段</returns>
        public static int CreateParallelField(MarcField field,
            bool bTo880,
            Delegate_getPinyin getPinyin,
            out string strError)
        {
            strError = "";

            if (field.ChildNodes.count == 0)
                return 1;

            MarcRecord record = (MarcRecord)field.Parent;

            MarcField main_field = null;

            string strFieldName = "";
            string strNumber = "";
            string strScriptId = "";
            string strOrientation = "";

            // 观察平行字段是否已经存在?
            string content_6 = field.select("subfield[@name='6']").FirstContent;
            if (string.IsNullOrEmpty(content_6) == false)
            {
                // 拆解 $6 内容
                _parseSubfield6(content_6,
            out strFieldName,
            out strNumber,
            out strScriptId,
            out strOrientation);
                if (string.IsNullOrEmpty(strScriptId) == true)
                {
                    strError = "field 的 $6 表明不是中文的字段，无法创建平行字段";
                    return 1;
                }

                // 找到关联的字段
                main_field = _findField(record,
                    strFieldName,
                    field.Name,
                    strNumber);
            }

            bool bNewField = false;
            if (main_field == null)
            {
                string strMainFieldName = field.Name;
                if (field.Name == "880")
                {
                    // 只能靠 $6 中 linking tag
                    if (string.IsNullOrEmpty(strFieldName) == true)
                    {
                        strError = "当前字段为 880 字段，但没有 $6 子字段，无法获得对应的字段名，因此函数调用失败";
                        return -1;
                    }
                    strMainFieldName = strFieldName;
                }
                main_field = new MarcField(strMainFieldName,
                    field.Indicator,
                    "");
                if (field.Name != "880")
                    field.before(main_field);
                else
                    record.ChildNodes.insertSequence(main_field,
            InsertSequenceStyle.PreferTail);
                bNewField = true;
            }
            else
            {
                // 内容全部删除。只是占用原有位置
                main_field.Content = "";
                main_field.Indicator = field.Indicator;
            }

            if (string.IsNullOrEmpty(strNumber) == true)
                strNumber = _getNewNumber(record);
            {
                // $6
                MarcSubfield subfield_6 = new MarcSubfield("6",
                    _buildSubfield6(bTo880 == true ? "880" : field.Name,
                    strNumber, "", strOrientation)
                );
                main_field.ChildNodes.add(subfield_6);
            }

            int nHanziCount = 0;

            List<MarcSubfield> remove_subfields = new List<MarcSubfield>();

            // 其余子字段逐个加入
            foreach (MarcSubfield subfield in field.ChildNodes)
            {
                if (subfield.Name == "6")
                    continue;
                // 2014/10/20
                if (subfield.Name == "9"
                    || char.IsUpper(subfield.Name[0]) == true)
                {
                    remove_subfields.Add(subfield);
                    continue;
                }

                string strPinyin = getPinyin(subfield.Content, out strError);
                if (strPinyin == null)
                {
                    strError = "创建拼音的过程出错: " + strError;
                    return -1;
                }
                if (strPinyin != subfield.Content)
                    nHanziCount++;
                main_field.ChildNodes.add(new MarcSubfield(subfield.Name, strPinyin));
            }

            // 2014/10/20
            if (remove_subfields.Count > 0)
            {
                foreach (MarcSubfield subfield in remove_subfields)
                {
                    subfield.detach();
                }
            }

            if (nHanziCount == 0 && bNewField == true)
            {
                main_field.detach();
                return 1;
            }

            // 当前字段加入 $6
            {
                if (string.IsNullOrEmpty(strScriptId) == true)
                    strScriptId = "$1";

                MarcSubfield subfield_6 = null;
                MarcNodeList temp = field.select("subfield[@name='6']");
                if (temp.count > 0)
                {
                    subfield_6 = temp[0] as MarcSubfield;
                    subfield_6.Content = _buildSubfield6(main_field.Name, strNumber, strScriptId, strOrientation);
                }
                else
                {
                    subfield_6 = new MarcSubfield("6",
                        _buildSubfield6(main_field.Name, strNumber, strScriptId, strOrientation)
                    );
                    field.ChildNodes.insert(0, subfield_6);
                }
            }

            if (bTo880)
            {
                field.Name = "880";
            }
            return 0;
        }

        /// <summary>
        /// 把记录中的 880 字段聚集在一起
        /// </summary>
        /// <param name="record">MARC 记录</param>
        public static void PositionField880s(MarcRecord record)
        {
            MarcNodeList fields = record.select("field[@name='880']");
            fields.detach();
            foreach (MarcField field in fields)
            {
                record.ChildNodes.insertSequence(field,
        InsertSequenceStyle.PreferTail);
            }
        }

        // 获得一个未使用过的字段连接编号
        static string _getNewNumber(MarcRecord record)
        {
            List<string> numbers = new List<string>();
            MarcNodeList subfields = record.select("field/subfield[@name='6']");
            foreach (MarcSubfield subfield in subfields)
            {
                // 拆解 $6 内容
                string strFieldName = "";
                string strNumber = "";
                string strScriptId = "";
                string strOrientation = "";
                _parseSubfield6(subfield.Content,
            out strFieldName,
            out strNumber,
            out strScriptId,
            out strOrientation);
                if (string.IsNullOrEmpty(strNumber) == false)
                    numbers.Add(strNumber);
            }

            if (numbers.Count == 0)
                return "01";
            numbers.Sort();
            string strMaxNumber = numbers[numbers.Count - 1];
            int nMax = 0;
            if (Int32.TryParse(strMaxNumber, out nMax) == false)
            {
                throw new Exception("字段编号 '" + strMaxNumber + "' 格式错误，应该为二位数字");
            }

            if (nMax < 0 || nMax > 98)
            {
                throw new Exception("已有最大字段编号 '" + strMaxNumber + "' 格式错误，应该为二位数字，值在 00-98 之间");
            }

            return (nMax + 1).ToString().PadLeft(2, '0');
        }

        // 给 MARC21 的若干子字段添加正确的标点符号
        /// <summary>
        /// 给 MARC21 的若干子字段添加正确的标点符号
        /// </summary>
        /// <param name="record">MARC 记录</param>
        /// <param name="strStyle">风格</param>
        /// <returns>记录是否发生了修改</returns>
        public static bool PunctuationMarc21(MarcRecord record, string strStyle)
        {
            bool bChanged = false;
            MarcNodeList fields = record.select("field[@name='245']");
            foreach (MarcField field in fields)
            {
                if (PunctuationMarc21Field245(field, strStyle) == true)
                    bChanged = true;
            }

            return bChanged;
        }

        // 给 MARC21 记录中 245 字段内的子字段添加正确的标点符号
        /// <summary>
        /// 给 MARC21 记录中 245 字段内的子字段添加正确的标点符号
        /// </summary>
        /// <param name="field">MARC 字段</param>
        /// <param name="strStyle">风格</param>
        /// <returns>记录是否发生了修改</returns>
        public static bool PunctuationMarc21Field245(MarcField field, string strStyle)
        {
            List<SubfieldInfo> infos = new List<SubfieldInfo>();
            foreach (MarcSubfield subfield in field.ChildNodes)
            {
                SubfieldInfo info = new SubfieldInfo();
                info.Subfield = subfield;
                infos.Add(info);
            }

            // 添加标点符号
            // $a题名 (不可重复)
            // $b题名其余部分 空: (少数情况;)
            // $c责任者说明  空/
            // $f首尾日期 ,
            // $g主体日期(集中出版日期)  自己() 
            // $h载体 自己[]
            // $k资料形式 空:
            // $n著作分卷/分节号 .
            // $p著作分卷/分节题名 (在$a$b或者另一个$p后，就是 .。跟在 $n 后面就是 ,)
            // $s版本 .
            // 最后一个子字段尾部要加入一个 .
            {
                MarcSubfield prev_field = null;
                foreach (SubfieldInfo info in infos)
                {
                    switch (info.Subfield.Name)
                    {
                        case "a":
                            if (prev_field != null)
                                info.LeadingPunctuation = " ";
                            break;
                        case "b":
                            // 注：如果前一个子字段末尾是 ';'，则用 ';'
                            info.LeadingPunctuation = ":";
                            break;
                        case "c":
                            info.LeadingPunctuation = "/";
                            break;
                        case "f":
                            info.LeadingPunctuation = ",";
                            break;
                        case "g":
                            info.Punctuation = "()";
                            break;
                        case "h":
                            info.Punctuation = "[]";
                            break;
                        case "k":
                            info.LeadingPunctuation = ":";
                            break;
                        case "n":
                            info.LeadingPunctuation = ".";
                            break;
                        case "p":
                            if (prev_field != null && prev_field.Name == "n")
                                info.LeadingPunctuation = ",";
                            else
                                info.LeadingPunctuation = ".";
                            break;
                        case "s":
                            info.LeadingPunctuation = ".";
                            break;
                    }

                    prev_field = info.Subfield;
                }
            }

            string strOld = field.Content;
            {
                SubfieldInfo prev_info = null;
                foreach (SubfieldInfo info in infos)
                {
                    if (prev_info != null)
                        ResetPunctuation(prev_info.Subfield, prev_info.Punctuation, info.LeadingPunctuation);

                    prev_info = info;
                }

                // 最后一个子字段加上尾部符号 .
                if (string.IsNullOrEmpty(prev_info.Subfield.Content) == true
                    || prev_info.Subfield.Content[prev_info.Subfield.Content.Length - 1] != '.')
                    prev_info.Subfield.Content += ".";
            }

            if (strOld != field.Content)
                return true;

            return false;
        }

        // 给标点符号前面添加必要的空格
        // 注: strDelimeter 应该为 1 字符形态
        static string AddBlank(string strDelimeter, string strDelimeters = "/:")
        {
            if (string.IsNullOrEmpty(strDelimeter))
                return "";
            char ch = strDelimeter[0];
            if (strDelimeters.IndexOf(ch) != -1)
                return " " + strDelimeter;
            return strDelimeter;
        }

        static void ResetPunctuation(MarcSubfield subfield,
            string strPunctuation,
            string strTailPunctuation)
        {
            bool bTrimed = false;
            // 添加本身的符号
            if (string.IsNullOrEmpty(strPunctuation) == false)
            {
                string content = TrimStartEndChar(subfield.Content);

                bTrimed = true;

                if (strPunctuation.Length == 1)
                {
                    subfield.Content = strPunctuation + content;
                }
                else if (strPunctuation.Length == 2)
                {
                    subfield.Content = strPunctuation[0] + content + strPunctuation[1];
                }
            }

            // 添加尾部符号
            {
                string content = null;
                if (bTrimed == false)
                    content = TrimEndChar(subfield.Content);
                else
                    content = subfield.Content;

                if (string.IsNullOrEmpty(strTailPunctuation))
                {
                    subfield.Content = content;
                    return;
                }

                subfield.Content = content + AddBlank(strTailPunctuation);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strText"></param>
        /// <param name="strDelimeters"></param>
        /// <returns></returns>
        public static string TrimEndChar(string strText, string strDelimeters = "./,;:")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";
            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            char tail = strText[strText.Length - 1];
            if (strDelimeters.IndexOf(tail) != -1)
                return strText.Substring(0, strText.Length - 1);
            return strText;
        }

        /// <summary>
        /// 切除一个字符串尾部的某些字符
        /// </summary>
        /// <param name="strText">待处理的字符串</param>
        /// <param name="strDelimeters">包含要清除的字符的字符串</param>
        /// <returns>返回结果字符串</returns>
        public static string TrimStartEndChar(string strText, string strDelimeters = "./,;:")
        {
            if (string.IsNullOrEmpty(strText))
                return "";
            strText = TrimEndChar(strText);
            return Unquote(strText.Trim(), "[]()");
        }

        // 去掉外围括住的符号
        // parameters:
        //      pairs   若干对打算去除的符号。例如 "%%" "()" "[](){}"。如果包含多对符号，则从左到右匹配，用上前面的就用它处理然后返回了，后面的若干对就不发生作用了
        static string Unquote(string strValue, string pairs)
        {
            if (string.IsNullOrEmpty(pairs))
                throw new ArgumentException("pairs 参数值不应为空", "pairs");

            if ((pairs.Length % 2) != 0)
                throw new ArgumentException("pairs 参数值的字符个数应为偶数", "pairs");

            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            for (int i = 0; i < pairs.Length / 2; i++)
            {
                char left = pairs[i * 2];
                if (strValue[0] == left)
                {
                    strValue = strValue.Substring(1);
                    if (strValue.Length == 0)
                        return "";

                    char right = pairs[(i * 2) + 1];
                    if (strValue[strValue.Length - 1] == right)
                        return strValue.Substring(0, strValue.Length - 1);
                }
            }

            return strValue;
        }

        //
        // 摘要:
        //     从当前 System.String 对象移除数组中指定的一组字符的所有尾部匹配项。
        //
        // 参数:
        //   trimChars:
        //     要删除的 Unicode 字符的数组，或 null。
        //
        // 返回结果:
        //     从当前字符串的开头移除所出现的所有 trimChars 参数中的字符后剩余的字符串。 如果 trimChars 为 null 或空数组，则改为删除 Unicode
        //     空白字符。 如果从当前实例无法删除字符，此方法返回未更改的当前实例。
        /// <summary>
        /// 从 System.String 对象移除数组中指定的一组字符的所有尾部匹配项。
        /// </summary>
        /// <param name="text">要处理的字符串</param>
        /// <param name="trimChars">要删除的 Unicode 字符的数组，或 null。</param>
        /// <returns>返回结果:
        /// 从当前字符串的尾部移除所出现的所有 trimChars 参数中的字符后剩余的字符串。
        /// 如果 trimChars 为 null 或空数组，则改为删除 Unicode 空白字符。
        /// 如果从当前实例无法删除字符，此方法返回未更改的当前实例。
        /// </returns>
        public static string TrimEnd(string text, string trimChars)
        {
            List<char> chars = new List<char>(trimChars); 
            return text.TrimEnd(chars.ToArray());
        }

        /// <summary>
        /// 从 System.String 对象移除数组中指定的一组字符的所有尾部匹配项。
        /// </summary>
        /// <param name="text">要处理的字符串</param>
        /// <returns>返回结果:
        /// 从当前字符串的尾部移除所出现的所有 Unicode 空白字符。
        /// 如果从当前实例无法删除字符，此方法返回未更改的当前实例。
        /// </returns>
        public static string TrimEnd(string text)
        {
            return TrimEnd(text, "");
        }

        // 子字段信息
        // 注：标点符号都是西文一个字符。每个标点符号使用时候前面需要加的空格，另行用规则解决，不包含在这里
        class SubfieldInfo
        {
            public string LeadingPunctuation { get; set; }  // 前置标点符号。最终会作用于上一个子字段的末尾

            public string Punctuation { get; set; } // 当前子字段的标点符号

            public MarcSubfield Subfield { get; set; }


        }
    }
}
