using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Web;

using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;


namespace DigitalPlatform.Marc
{
    public class MarcDiff
    {
        public static string SEP = "<td class='sep'></td>";
        // 根据字段权限定义过滤出允许的内容
        // return:
        //      -1  出错
        //      0   成功
        //      1   有部分字段被修改或滤除
        public static int FilterFields(string strFieldNameList,
            ref string strMarc,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            bool bChanged = false;

            if (string.IsNullOrEmpty(strMarc) == true)
                return 0;

            string strHeader = "";
            string strBody = "";

            SplitMarc(strMarc,
                out strHeader,
                out strBody);

            if (strHeader.Length < 24)
            {
                strHeader = strHeader.PadRight(24, '?');
                bChanged = true;
            }

            FieldNameList list = new FieldNameList();
            nRet = list.Build(strFieldNameList, out strError);
            if (nRet == -1)
            {
                strError = "字段权限定义 '"+strFieldNameList+"' 不合法: " + strError;
                return -1;
            }

            StringBuilder text = new StringBuilder(4096);
            if (list.Contains("###") == true)
                text.Append(strHeader);
            else
            {
                bChanged = true;
                text.Append(new string('?', 24));
            }

            string[] fields = strBody.Split(new char[] {(char)30}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in fields)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;

                string strFieldName = GetFieldName(s);
                if (list.Contains(strFieldName) == false)
                {
                    bChanged = true;
                    continue;
                }

                text.Append(s);
                text.Append((char)30);
            }

            strMarc = text.ToString();
            if (bChanged == true)
                return 1;
            return 0;
        }

        // 末尾是否有内码为 1 的字符？
        static bool HasMask(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return false;
            if (strText[strText.Length - 1] == (char)1)
                return true;
            return false;
        }

        // 去掉末尾的内码为 1 的字符
        static string RemoveMask(string strText)
        {
            if (HasMask(strText) == true)
                return strText.Substring(0, strText.Length - 1);
            return strText;
        }

        // 按照字段修改权限定义，合并新旧两个 MARC 记录
        // parameters:
        //      strDefaultOperation   insert/replace/delete 之一或者逗号间隔组合
        // return:
        //      -1  出错
        //      0   成功
        //      1   有部分修改要求被拒绝
        public static int MergeOldNew(
            string strDefaultOperation,
            string strFieldNameList,
            string strOldMarc,
            ref string strNewMarc,
            out string strComment,
            out string strError)
        {
            strError = "";
            strComment = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strOldMarc) == true && string.IsNullOrEmpty(strNewMarc) == true)
                return 0;

#if NO
            bool bNew = false;
            bool bDelete = false;
            bool bChange = false;

            if (string.IsNullOrEmpty(strAction) == true)
            {
                bNew = true;
                bDelete = true;
                bChange = true;
            }
            else
            {
                bNew = StringUtil.IsInList("new", strAction);
                bDelete = StringUtil.IsInList("delete", strAction);
                bChange = StringUtil.IsInList("change", strAction);
            }
#endif

            string strOldHeader = "";
            string strNewHeader = "";
            string strOldBody = "";
            string strNewBody = "";

            SplitMarc(strOldMarc,
                out strOldHeader,
                out strOldBody);
            SplitMarc(strNewMarc,
                out strNewHeader,
                out strNewBody);

            if (strOldHeader.Length < 24)
                strOldHeader = strOldHeader.PadRight(24, '?');
            if (strNewHeader.Length < 24)
                strNewHeader = strNewHeader.PadRight(24, '?');

#if NO
            FieldNameList list = new FieldNameList();
            nRet = list.Build(strFieldNameList, out strError);
            if (nRet == -1)
            {
                strError = "字段权限定义 '" + strFieldNameList + "' 不合法: " + strError;
                return -1;
            }
#endif
            
            OperationRights rights_table = new OperationRights();
            nRet = rights_table.Build(strFieldNameList,
                strDefaultOperation,
                out strError);
            if (nRet == -1)
            {
                strError = "字段权限定义 '" + strFieldNameList + "' 不合法: " + strError;
                return -1;
            }

            var differ = new MarcDiffer();
            var builder = new MarcDiffBuilder(differ);
            var result = builder.BuildDiffModel(strOldBody, strNewBody);

            Debug.Assert(result.OldText.Lines.Count == result.NewText.Lines.Count, "");

            /*
    public enum ChangeType
    {
        Unchanged,
        Deleted,
        Inserted,
        Imaginary,
        Modified
    }
             * */
            // 字段名如果最后增加了一个 ！ 字符，表示这是因为 856 权限原因被拒绝操作的字段。如果 856 字段名后面没有 ！ 字符，则表示是因为 fieldnamelist 定义缘故被拒绝的
            // 被拒绝插入的字段名
            List<string> denied_insert_fieldnames = new List<string>();
            // 被拒绝删除的字段名
            List<string> denied_delete_fieldnames = new List<string>();
            // 被拒绝修改的字段名
            List<string> denied_change_fieldnames = new List<string>();

            bool bNotAccepted = false;
            List<string> fields = new List<string>();
            for (int index = 0; index < result.NewText.Lines.Count; index ++ )
            {
                var newline = result.NewText.Lines[index];
                var oldline = result.OldText.Lines[index];

                // 2013/11/18
                if (string.IsNullOrEmpty(newline.Text) == true
    && string.IsNullOrEmpty(oldline.Text) == true)
                    continue;

                string strNewFieldName = GetFieldName(newline.Text);
                // string strOldFieldName = GetFieldName(oldline.Text);

                if (newline.Type == ChangeType.Unchanged)
                {
                    fields.Add(RemoveMask(newline.Text));
                }
                else if (newline.Type == ChangeType.Inserted)
                {
                    Debug.Assert(strNewFieldName != null, "");

                    // 注：有标记的字段不允许插入
                    bool bMask = HasMask(newline.Text);

                    if (rights_table.InsertFieldNames.Contains(strNewFieldName) == true
                        && bMask == false)
                        fields.Add(newline.Text);
                    else
                    {
                        denied_insert_fieldnames.Add(strNewFieldName 
                            + (bMask ? "!" : ""));
                        bNotAccepted = true;
                    }
                }
                else if (newline.Type == ChangeType.Modified)
                {
                    Debug.Assert(strNewFieldName != null, "");

                    Debug.Assert(oldline.Type == ChangeType.Modified, "");

                    // 看新旧字段名是否都在可修改范围内
                    string strOldFieldName = GetFieldName(oldline.Text);
                    Debug.Assert(strOldFieldName != null, "");

                    // 注：有标记的字段不允许用于替换
                    bool bMask = HasMask(newline.Text);

                    if (rights_table.ReplaceFieldNames.Contains(strNewFieldName) == true
                        && rights_table.ReplaceFieldNames.Contains(strOldFieldName) == true
                        && bMask == false)
                        fields.Add(newline.Text);
                    else
                    {
                        fields.Add(result.OldText.Lines[index].Text);    // 依然采用修改前的值
                        bNotAccepted = true;

                        denied_change_fieldnames.Add(strOldFieldName
                            + (bMask ? "!" : ""));
                    }
                }
                else if (oldline.Type == ChangeType.Deleted)
                {
                    string strOldFieldName = GetFieldName(oldline.Text);
                    Debug.Assert(strOldFieldName != null, "");

                    if (rights_table.DeleteFieldNames.Contains(strOldFieldName) == false)
                    {
                        fields.Add(oldline.Text);   // 不允许删除
                        bNotAccepted = true;

                        denied_delete_fieldnames.Add(strOldFieldName);
                    }
                    else
                    {
                        // bNotAccepted = true; // BUG
                    }
                }
            }

            StringBuilder text = new StringBuilder(4096);
            if (rights_table.ReplaceFieldNames.Contains("###") == true
                || rights_table.InsertFieldNames.Contains("###") == true)
                text.Append(strNewHeader);
            else
            {
                if (strNewHeader != strOldHeader)
                {
                    bNotAccepted = true;
                    denied_change_fieldnames.Add("###");    // ### 表示头标区
                }
                text.Append(strOldHeader);
            }

            foreach (string s in fields)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;
                text.Append(s);
                text.Append((char)30);
            }

            if (denied_insert_fieldnames.Count > 0)
                strComment += "字段 " + StringUtil.MakePathList(denied_insert_fieldnames)+ " 被拒绝插入";

            if (denied_delete_fieldnames.Count > 0)
            {
                if (string.IsNullOrEmpty(strComment) == false)
                    strComment += " ; ";
                strComment += "字段 " + StringUtil.MakePathList(denied_delete_fieldnames) + " 被拒绝删除";
            }

            if (denied_change_fieldnames.Count > 0)
            {
                if (string.IsNullOrEmpty(strComment) == false)
                    strComment += " ; ";
                strComment += "字段 " + StringUtil.MakePathList(denied_change_fieldnames) + " 被拒绝修改";
            }

            strNewMarc = text.ToString();
            if (bNotAccepted == true)
                return 1;
            return 0;
        }

        // 获得一段文本前三个字符，作为字段名
        static string GetFieldName(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return null;

            if (strText.Length >= 3)
                return strText.Substring(0, 3);
            return strText.PadRight(3, '?');
        }

        // 把一个 MARC 记录拆分为 头标区 和 字段区 两个部分
        static void SplitMarc(string strMARC,
            out string strHeader,
            out string strBody)
        {
            strHeader = "";
            strBody = "";

            if (String.IsNullOrEmpty(strMARC) == true)
                return;

            // 整理尾部字符
            char tail = strMARC[strMARC.Length - 1];
            if (tail == 29)
            {
                strMARC = strMARC.Substring(0, strMARC.Length - 1);

                if (String.IsNullOrEmpty(strMARC) == true)
                    return;
            }

            if (strMARC.Length < 24)
            {
                strHeader = strMARC;
                strHeader = strHeader.PadRight(24, '?');
                return;
            }

            strHeader = strMARC.Substring(0, 24);
            strBody = strMARC.Substring(24);
        }

        // 包装后的版本
        public static int DiffHtml(
    string strOldMarc,
    string strNewMarc,
    out string strHtml,
    out string strError)
        {
            return DiffHtml(
                strOldMarc,
                "",
                "",
                strNewMarc,
                "",
                "",
                out strHtml,
                out strError);
        }

        // 包装后的版本
        public static int DiffHtml(
    string strOldMarc,
    string strOldFragmentXml,
            string strOldImageFragment,
    string strNewMarc,
    string strNewFragmentXml,
            string strNewImageFragment,
    out string strHtml,
    out string strError)
        {
            return DiffHtml(
            "",
            strOldMarc,
            strOldFragmentXml,
            strOldImageFragment,
            "",
            strNewMarc,
            strNewFragmentXml,
            strNewImageFragment,
            out strHtml,
            out strError);
        }

        static string GetImageHtml(string strImageFragment)
        {
            return "\r\n<td class='content' colspan='3'>"    //  
                + strImageFragment
                + "</td>";
        }

        // 创建展示两个 MARC 记录差异的 HTML 字符串
        // return:
        //      -1  出错
        //      0   成功
        public static int DiffHtml(
            string strOldTitle,
            string strOldMarc,
            string strOldFragmentXml,
            string strOldImageFragment,
            string strNewTitle,
            string strNewMarc,
            string strNewFragmentXml,
            string strNewImageFragment,
            out string strHtml,
            out string strError)
        {
            strError = "";
            strHtml = "";
            int nRet = 0;

            if (string.IsNullOrEmpty(strOldMarc) == true && string.IsNullOrEmpty(strNewMarc) == true)
                return 0;

            string strOldHeader = "";
            string strNewHeader = "";
            string strOldBody = "";
            string strNewBody = "";

            SplitMarc(strOldMarc,
                out strOldHeader,
                out strOldBody);
            SplitMarc(strNewMarc,
                out strNewHeader,
                out strNewBody);

            if (strOldHeader.Length < 24)
                strOldHeader = strOldHeader.PadRight(24, '?');
            if (strNewHeader.Length < 24)
                strNewHeader = strNewHeader.PadRight(24, '?');

            var marc_differ = new MarcDiffer();
            var marc_builder = new MarcDiffBuilder(marc_differ);
            var marc_diff_result = marc_builder.BuildDiffModel(strOldBody, strNewBody);

            Debug.Assert(marc_diff_result.OldText.Lines.Count == marc_diff_result.NewText.Lines.Count, "");

            /*
    public enum ChangeType
    {
        Unchanged,
        Deleted,
        Inserted,
        Imaginary,
        Modified
    }
             * */

            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);

            if (string.IsNullOrEmpty(strOldTitle) == false
                || string.IsNullOrEmpty(strNewTitle) == false)
            {
                string strLineClass = "header";
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");

                strResult.Append(BuildHeaderHtml(false, strOldTitle));
                strResult.Append(SEP);
                strResult.Append(BuildHeaderHtml(false, strNewTitle));
                strResult.Append("\r\n</tr>");
            }


            {
                string strLineClass = "header";
                bool bModified = strOldHeader != strNewHeader;
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");

                strResult.Append(BuildHeaderHtml(bModified, strOldHeader));
                strResult.Append(SEP);
                strResult.Append(BuildHeaderHtml(bModified, strNewHeader));
                strResult.Append("\r\n</tr>");
            }

            if (string.IsNullOrEmpty(strOldImageFragment) == false
|| string.IsNullOrEmpty(strNewImageFragment) == false)
            {
                string strLineClass = "header";
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");

                strResult.Append(GetImageHtml(strOldImageFragment));
                strResult.Append(SEP);
                strResult.Append(GetImageHtml(strNewImageFragment));
                strResult.Append("\r\n</tr>");
            }


            for (int index = 0; index < marc_diff_result.NewText.Lines.Count; index++)
            {
                var newline = marc_diff_result.NewText.Lines[index];
                var oldline = marc_diff_result.OldText.Lines[index];

                if (string.IsNullOrEmpty(newline.Text) == true
                    && string.IsNullOrEmpty(oldline.Text) == true)
                    continue;

                string strLineClass = "datafield";
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                // 创建一个字段的 HTML 局部 三个 <td>
                strResult.Append(BuildFieldHtml( oldline.Type, oldline.Text));
                strResult.Append(SEP);
                strResult.Append(BuildFieldHtml(newline.Type, newline.Text));
                strResult.Append("\r\n</tr>");

#if NO
                if (newline.Type == ChangeType.Unchanged)
                {

                }
                else if (newline.Type == ChangeType.Inserted)
                {

                }
                else if (newline.Type == ChangeType.Modified)
                {

                }
                else if (oldline.Type == ChangeType.Deleted)
                {

                }
#endif
            }

#if NO
            if (string.IsNullOrEmpty(strOldFragmentXml) == false || string.IsNullOrEmpty(strNewFragmentXml) == false)
            {
                var xml_differ = new Differ();
                var xml_builder = new SideBySideDiffBuilder(xml_differ);
                var xml_diff_result = xml_builder.BuildDiffModel(GetComparableXmlString(strOldFragmentXml), GetComparableXmlString(strNewFragmentXml));

                Debug.Assert(xml_diff_result.OldText.Lines.Count == xml_diff_result.NewText.Lines.Count, "");

                for (int index = 0; index < xml_diff_result.NewText.Lines.Count; index++)
                {
                    var newline = xml_diff_result.NewText.Lines[index];
                    var oldline = xml_diff_result.OldText.Lines[index];

                    string strLineClass = "datafield";

                    if (newline.Type == ChangeType.Modified)
                    {

                    }

                    strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                    // 创建一个 XML 字段的 HTML 局部 三个 <td>
                    strResult.Append(BuildFragmentFieldHtml(oldline.Type, oldline.Text));
                strResult.Append(SEP);
                    strResult.Append(BuildFragmentFieldHtml(newline.Type, newline.Text));
                    strResult.Append("\r\n</tr>");
                }

            }
#endif

            if (string.IsNullOrEmpty(strOldFragmentXml) == false || string.IsNullOrEmpty(strNewFragmentXml) == false)
            {
                string strLineClass = "sepline";
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                strResult.Append("<td class='sepline' colspan='3'>&nbsp;</td>");
                strResult.Append("<td class='cross'>&nbsp;</td>");
                strResult.Append("<td class='sepline' colspan='3'>&nbsp;</td>");
                strResult.Append("\r\n</tr>");
            }

            strResult.Append(DiffXml(
                0,
                strOldFragmentXml,
                XmlNodeType.Element,
                strNewFragmentXml,
                XmlNodeType.Element));

            strResult.Append("</table>");
            strHtml = strResult.ToString();

            return 0;
        }

        public static string GetIndentXml(string strXml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                return strXml;
            }

            return DomUtil.GetIndentXml(dom.DocumentElement);
        }

        public static string GetIndentInnerXml(string strXml)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                return strXml;
            }

            return DomUtil.GetIndentInnerXml(dom.DocumentElement);
        }

        static string GetPlanTextDiffHtml(
            int nLevel,
            string strOldFragmentXml,
            string strNewFragmentXml)
        {
            string strLineClass = "datafield";
            StringBuilder strResult = new StringBuilder(4096);

            // Plain Text
            ChangeType type = ChangeType.Unchanged;
            if (strOldFragmentXml != strNewFragmentXml)
                type = ChangeType.Modified;

            strResult.Append("\r\n<tr class='" + strLineClass + "'>");
            strResult.Append(BuildFragmentFieldHtml(nLevel, type, strOldFragmentXml));
            strResult.Append(SEP);
            strResult.Append(BuildFragmentFieldHtml(nLevel, type, strNewFragmentXml));
            strResult.Append("\r\n</tr>");

            return strResult.ToString();
        }

        static string DiffXml(
            int nLevel,
            string strOldFragmentXml,
            XmlNodeType old_nodetype,
            string strNewFragmentXml,
            XmlNodeType new_nodetype)
        {
            if (string.IsNullOrEmpty(strOldFragmentXml) == true && string.IsNullOrEmpty(strNewFragmentXml) == true)
                return "";

            if (old_nodetype != XmlNodeType.Element || new_nodetype != XmlNodeType.Element)
            {
                if (old_nodetype == XmlNodeType.Element)
                    strOldFragmentXml = GetIndentXml(strOldFragmentXml);
                if (new_nodetype == XmlNodeType.Element)
                    strNewFragmentXml = GetIndentXml(strNewFragmentXml);
                return GetPlanTextDiffHtml(
                    nLevel,
                    strOldFragmentXml,
                    strNewFragmentXml);
            }

            string strOldChildren = "";
            string strOldBegin = "";
            string strOldEnd = "";
            List<XmlNode> old_childnodes = null;
            string strOldElementName = "";
            GetComparableXmlString(strOldFragmentXml,
                out strOldChildren,
                out old_childnodes,
                out strOldElementName,
                out strOldBegin,
                out strOldEnd);

            string strNewChildren = "";
            string strNewBegin = "";
            string strNewEnd = "";
            List<XmlNode> new_childnodes = null;
            string strNewElementName = "";
            GetComparableXmlString(strNewFragmentXml,
                out strNewChildren,
                out new_childnodes,
                out strNewElementName,
                out strNewBegin,
                out strNewEnd);

            bool bSpecialCompare = false;   // 是否属于特殊情况： 根元素名不相同，但仍需要比较下级
            if (strOldElementName != strNewElementName
                && nLevel == 0)
                bSpecialCompare = true;

            if (strOldElementName != strNewElementName && bSpecialCompare == false)
            {
                // 元素名不一样了并且不是根级别，就没有必要做细节比较了
                // 意思是说如果是根级别，即便根元素名不一样，也要比较其下级
                if (old_nodetype == XmlNodeType.Element)
                    strOldFragmentXml = GetIndentXml(strOldFragmentXml);
                if (new_nodetype == XmlNodeType.Element)
                    strNewFragmentXml = GetIndentXml(strNewFragmentXml);
                return GetPlanTextDiffHtml(
    nLevel,
    strOldFragmentXml,
    strNewFragmentXml);
            }

            string strLineClass = "datafield";
            StringBuilder strResult = new StringBuilder(4096);

            if (nLevel > 0 || bSpecialCompare == true)
            {
                ChangeType begin_type = ChangeType.Unchanged;
                if (strOldBegin != strNewBegin)
                    begin_type = ChangeType.Modified;

                // Begin
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                strResult.Append(BuildFragmentFieldHtml(nLevel, begin_type, strOldBegin));
                strResult.Append(SEP);
                strResult.Append(BuildFragmentFieldHtml(nLevel, begin_type, strNewBegin));
                strResult.Append("\r\n</tr>");
            }

            // return "\r\n<td class='content' colspan='3'></td>";

            if (string.IsNullOrEmpty(strOldChildren) == false || string.IsNullOrEmpty(strNewChildren) == false)
            {
                var xml_differ = new Differ();
                var xml_builder = new SideBySideDiffBuilder(xml_differ);
                var xml_diff_result = xml_builder.BuildDiffModel(strOldChildren, strNewChildren);

                Debug.Assert(xml_diff_result.OldText.Lines.Count == xml_diff_result.NewText.Lines.Count, "");
                int old_index = 0;
                int new_index = 0;
                for (int index = 0; index < xml_diff_result.NewText.Lines.Count; index++)
                {
                    var newline = xml_diff_result.NewText.Lines[index];
                    var oldline = xml_diff_result.OldText.Lines[index];

                    XmlNode new_node = null;
                    if (newline.Type != ChangeType.Imaginary)
                        new_node = new_childnodes[new_index++];

                    XmlNode old_node = null;
                    if (oldline.Type != ChangeType.Imaginary)
                        old_node = old_childnodes[old_index++];

                    if (newline.Type == ChangeType.Modified)
                    {
                        strResult.Append(DiffXml(nLevel + 1,
                            oldline.Text,
                            old_node.NodeType,
                            newline.Text,
                            new_node.NodeType));
                        continue;
                    }

                    string strOldText = "";
                    if (old_node != null && old_node.NodeType == XmlNodeType.Element)
                        strOldText = GetIndentXml(oldline.Text);
                    else 
                        strOldText = oldline.Text;

                    string strNewText = "";
                    if (new_node != null && new_node.NodeType == XmlNodeType.Element)
                        strNewText = GetIndentXml(newline.Text);
                    else
                        strNewText = newline.Text;

                    strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                    // 创建一个 XML 字段的 HTML 局部 三个 <td>
                    strResult.Append(BuildFragmentFieldHtml(nLevel + 1, oldline.Type, strOldText));
                    strResult.Append(SEP);
                    strResult.Append(BuildFragmentFieldHtml(nLevel + 1, newline.Type, strNewText));
                    strResult.Append("\r\n</tr>");
                }
            }

            if (nLevel > 0 || bSpecialCompare == true)
            {
                ChangeType end_type = ChangeType.Unchanged;
                if (strOldEnd != strNewEnd)
                    end_type = ChangeType.Modified;

                // End
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                strResult.Append(BuildFragmentFieldHtml(nLevel, end_type, strOldEnd));
                strResult.Append(SEP);
                strResult.Append(BuildFragmentFieldHtml(nLevel, end_type, strNewEnd));
                strResult.Append("\r\n</tr>");
            }

            return strResult.ToString();
        }

        // 根据 XML 字符串，构造出第一级子元素字符串列表
        static int GetComparableXmlString(string strXml,
            out string strChildrenText,
            out List<XmlNode> childnodes,
            out string strElementName,
            out string strRootBegin,
            out string strRootEnd)
        {
            strChildrenText = "";
            strRootBegin = "";
            strRootEnd = "";
            childnodes = new List<XmlNode>();
            strElementName = "";

            if (string.IsNullOrEmpty(strXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = false;
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                return -1;
            }
            StringBuilder result = new StringBuilder(4096);
            foreach (XmlNode node in dom.DocumentElement.ChildNodes)
            {
                string strText = node.OuterXml.Replace("\r\n", "").Trim();
                if (string.IsNullOrEmpty(strText) == false)
                {
                    if (result.Length > 0)
                        result.Append("\r\n");
                    result.Append(strText);
                    childnodes.Add(node);
                }
            }

            // TODO: 遇到 <record> 元素要跳过
            strChildrenText = result.ToString();
            strRootBegin = GetElementBeginString(dom.DocumentElement);
            strRootEnd = GetElementEndString(dom.DocumentElement);
            strElementName = dom.DocumentElement.Name;
            return 0;
        }

        static string GetElementBeginString(XmlElement element)
        {
            StringBuilder result = new StringBuilder(4096);
            result.Append("<" + element.Name);
            foreach (XmlAttribute attr in element.Attributes)
            {
                result.Append(" " + attr.Name + "=\"" + attr.Value + "\"");
            }
            result.Append(">");
            return result.ToString();
        }

        static string GetElementEndString(XmlElement element)
        {
            return "</" + element.Name + ">";
        }

        // 将前方连续的若干空格字符替换为 &nbsp;
        public static string ReplaceLeadingTab(string strText)
        {
            StringBuilder result = new StringBuilder(4096);
            int i = 0;
            foreach (char c in strText)
            {
                if (c == ' ')
                    result.Append("&nbsp;");
                else 
                {
                    result.Append(strText.Substring(i));
                    break;
                }

                i ++;
            }

            return result.ToString();
        }

        // 创建一个 XML 字段的 HTML 局部 三个 <td>
        static string BuildFragmentFieldHtml(
            int nLevel,
            ChangeType type,
            string strField)
        {
            if (string.IsNullOrEmpty(strField) == true)
                return "\r\n<td class='content' colspan='3'></td>";

            string strTypeClass = "";
            if (type == ChangeType.Modified)
                strTypeClass = " modified";
            else if (type == ChangeType.Inserted)
                strTypeClass = " inserted";
            else if (type == ChangeType.Deleted)
                strTypeClass = " deleted";
            else if (type == ChangeType.Imaginary)
                strTypeClass = " imaginary";
            else if (type == ChangeType.Unchanged)
                strTypeClass = " unchanged";

            string strLevel = "";
            if (nLevel > 0)
                strLevel = strLevel.PadRight(nLevel, ' ').Replace(" ","    ");  // .Replace(" ", "&nbsp;&nbsp;&nbsp;&nbsp;");
            // 
            string[] lines = HttpUtility.HtmlEncode(strField).Replace("\r\n", "\n").Split(new char[] { '\n' });
            StringBuilder result = new StringBuilder(4096);
            foreach (string line in lines)
            {
                if (result.Length > 0)
                    result.Append("<br/>");
                result.Append(ReplaceLeadingTab(strLevel + line));
            }

            return "\r\n<td class='content" + strTypeClass + "' colspan='3'>" + result + "</td>";
        }
        // 创建一个字段的 HTML 局部 三个 <td>
        static string BuildFieldHtml(
            ChangeType type,
            string strField,
            bool bSubfieldReturn = false)
        {
            if (string.IsNullOrEmpty(strField) == true)
                return "\r\n<td class='content' colspan='3'></td>";

            string strLineClass = "";
            string strFieldName = "";
            string strIndicatior = "";
            string strContent = "";

            // 取字段名
            if (strField.Length < 3)
            {
                strFieldName = strField;
                strField = "";
            }
            else
            {
                strFieldName = strField.Substring(0, 3);
                strField = strField.Substring(3);
            }

            // 取指示符
            if (MarcUtil.IsControlFieldName(strFieldName) == true)
            {
                strLineClass = "controlfield";
                strField = strField.Replace(' ', '_');
            }
            else
            {
                if (strField.Length < 2)
                {
                    strIndicatior = strField;
                    strField = "";
                }
                else
                {
                    strIndicatior = strField.Substring(0, 2);
                    strField = strField.Substring(2);
                }
                strIndicatior = strIndicatior.Replace(' ', '_');

                strLineClass = "datafield";

                // 1XX字段有定长内容
                if (strFieldName.Length >= 1 && strFieldName[0] == '1')
                {
                    strField = strField.Replace(' ', '_');
                    strLineClass += " fixedlengthsubfield";
                }
            }

            strContent = MarcUtil.GetHtmlFieldContent(strField,
    bSubfieldReturn);

            string strTypeClass = "";
            if (type == ChangeType.Modified)
                strTypeClass = " modified";
            else if (type == ChangeType.Inserted)
                strTypeClass = " inserted";
            else if (type == ChangeType.Deleted)
                strTypeClass = " deleted";
            else if (type == ChangeType.Imaginary)
                strTypeClass = " imaginary";
            else if (type == ChangeType.Unchanged)
                strTypeClass = " unchanged";

            // 
            return "\r\n<td class='fieldname" + strTypeClass + "'>" + strFieldName + "</td>"
                + "<td class='indicator" + strTypeClass + "'>" + strIndicatior + "</td>"
                + "<td class='content" + strTypeClass + "'>" + strContent + "</td>";
        }

        static string BuildHeaderHtml(bool bModified, string strField)
        {
            if (string.IsNullOrEmpty(strField) == true)
            {
                // return "\r\n<td class='content' colspan='3'></td>";
                return "<td class='fieldname'></td>"
    + "<td class='indicator'></td>"
    + "<td class='content'></td>";
            }

            // string strLineClass = "";
            string strFieldName = "";
            string strIndicatior = "";
            string strContent = "";

            // strLineClass = "header";
            strField = strField.Replace(' ', '_');

            strContent = MarcUtil.GetHtmlFieldContent(strField, false);

            string strTypeClass = "";
            if (bModified)
                strTypeClass = " modified";
            // 
            return "\r\n<td class='fieldname" + strTypeClass + "'>" + strFieldName + "</td>"
                + "<td class='indicator" + strTypeClass + "'>" + strIndicatior + "</td>"
                + "<td class='content" + strTypeClass + "'>" + strContent + "</td>";
        }



        // 创建展示两个 XML 记录差异的 HTML 字符串
        // return:
        //      -1  出错
        //      0   成功
        public static int DiffXml(
            string strOldFragmentXml,
            string strNewFragmentXml,
            out string strHtml,
            out string strError)
        {
            strError = "";
            strHtml = "";

            if (string.IsNullOrEmpty(strOldFragmentXml) == true && string.IsNullOrEmpty(strNewFragmentXml) == true)
                return 0;

            /*
    public enum ChangeType
    {
        Unchanged,
        Deleted,
        Inserted,
        Imaginary,
        Modified
    }
             * */

            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);

            strResult.Append(DiffXml(
                0,
                strOldFragmentXml,
                XmlNodeType.Element,
                strNewFragmentXml,
                XmlNodeType.Element));

            strResult.Append("</table>");
            strHtml = strResult.ToString();

            return 0;
        }

        #region 比较 OPAC 字段差异的

        // 创建展示两个 OPAC 记录差异的 HTML 字符串
        // return:
        //      -1  出错
        //      0   成功。两边相等
        //      1   两边不相等
        public static int DiffOpacHtml(
            string strOldText,
            string strNewText,
            out string strHtml,
            out string strError)
        {
            strError = "";
            strHtml = "";

            if (string.IsNullOrEmpty(strOldText) == true && string.IsNullOrEmpty(strNewText) == true)
                return 0;

            var marc_differ = new Differ();
            var marc_builder = new SideBySideDiffBuilder(marc_differ);
            var marc_diff_result = marc_builder.BuildDiffModel(strOldText, strNewText);

            Debug.Assert(marc_diff_result.OldText.Lines.Count == marc_diff_result.NewText.Lines.Count, "");

            /*
    public enum ChangeType
    {
        Unchanged,
        Deleted,
        Inserted,
        Imaginary,
        Modified
    }
             * */

            bool bChanged = false;

            StringBuilder strResult = new StringBuilder("\r\n<table class='marc'>", 4096);

            for (int index = 0; index < marc_diff_result.NewText.Lines.Count; index++)
            {
                var newline = marc_diff_result.NewText.Lines[index];
                var oldline = marc_diff_result.OldText.Lines[index];

                if (string.IsNullOrEmpty(newline.Text) == true
                    && string.IsNullOrEmpty(oldline.Text) == true)
                    continue;

                if (oldline.Type != ChangeType.Unchanged
                    || newline.Type != ChangeType.Unchanged)
                    bChanged = true;

                string strLineClass = "datafield";
                strResult.Append("\r\n<tr class='" + strLineClass + "'>");
                // 创建一个字段的 HTML 局部 三个 <td>
                strResult.Append(BuildOpacFieldHtml(oldline.Type, oldline.Text));
                strResult.Append(SEP);
                strResult.Append(BuildOpacFieldHtml(newline.Type, newline.Text));
                strResult.Append("\r\n</tr>");
            }

            strResult.Append("</table>");
            strHtml = strResult.ToString();

            if (bChanged == false)
                return 0;

            return 1;
        }

        // 创建一个字段的 HTML 局部 三个 <td>
        static string BuildOpacFieldHtml(
            ChangeType type,
            string strField)
        {
            if (string.IsNullOrEmpty(strField) == true)
                return "\r\n<td class='content' colspan='2'></td>";

            // string strLineClass = "";
            string strFieldName = "";
            string strContent = "";

            int nRet = strField.IndexOf(":");
            if (nRet == -1)
            {
                strFieldName = strField;
                strContent = "";
            }
            else
            {
                strFieldName = strField.Substring(0, nRet).Trim();
                strContent = strField.Substring(nRet + 1).Trim();
            }

            //    strLineClass = "datafield";

            string strTypeClass = "datafield";
            if (type == ChangeType.Modified)
                strTypeClass += " modified";
            else if (type == ChangeType.Inserted)
                strTypeClass += " inserted";
            else if (type == ChangeType.Deleted)
                strTypeClass += " deleted";
            else if (type == ChangeType.Imaginary)
                strTypeClass += " imaginary";
            else if (type == ChangeType.Unchanged)
                strTypeClass += " unchanged";

            // 
#if NO
            return "\r\n<td class='fieldname" + strTypeClass + "'>" + HttpUtility.HtmlEncode(strFieldName) + "</td>"
                + "<td class='indicator" + strTypeClass + "'>" + "" + "</td>"
                + "<td class='content" + strTypeClass + "'>" + HttpUtility.HtmlEncode(strContent) + "</td>";
#endif
            return "\r\n<td class='fieldname" + strTypeClass + "'>" + HttpUtility.HtmlEncode(strFieldName) + "</td>"
    + "<td class='content" + strTypeClass + "'>" + HttpUtility.HtmlEncode(strContent) + "</td>";

        }

        #endregion
    }

    public class OpacField
    {
        public string Name = "";
        public string Value = "";

        public OpacField()
        {
        }

        public OpacField(string strName, string strValue)
        {
            Name = strName;
            Value = strValue;
        }
    }
}
