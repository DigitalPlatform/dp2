using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform.Text;

namespace DigitalPlatform.Marc
{
    public class FieldNameItem
    {
        public string StartFieldName = "";
        public string EndFieldName = "";
    }

    // 字段名列表。用于定义字段权限
    public class FieldNameList : List<FieldNameItem>
    {
        // 创建
        public int Build(string strFieldNameList,
            out string strError)
        {
            strError = "";

            string[] segments = strFieldNameList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in segments)
            {
                string strSegment = s.Trim();
                if (string.IsNullOrEmpty(strSegment) == true)
                    continue;

                string strStart = "";
                string strEnd = "";
                if (strSegment == "*"
                    || strSegment == "***"
                    || strSegment == "*-*"
                    || strSegment == "***-***")
                {
                    strStart = "*";
                    strEnd = "*";
                    goto CONTINUE;
                }

                if (strSegment.Length < 3)
                {
                    strError = "每一段长度须至少 3 字符。'" + strSegment + "'";
                    return -1;
                }
                // -

                int nRet = strSegment.IndexOf("-", 3);
                if (nRet != -1)
                {
                    strStart = strSegment.Substring(0, nRet).Trim();
                    strEnd = strSegment.Substring(nRet + 1).Trim();

                    if (strEnd.Length != 3)
                    {
                        strError = "字段名 '" + strEnd + "' 不合法，应当为3字符";
                        return -1;
                    }
                }
                else
                {
                    strStart = strSegment;
                }

                if (strStart.Length != 3)
                {
                    strError = "字段名 '" + strStart + "' 不合法，应当为3字符";
                    return -1;
                }

                CONTINUE:
                FieldNameItem item = new FieldNameItem();
                item.StartFieldName = strStart;
                if (string.IsNullOrEmpty(strEnd) == true)
                    item.EndFieldName = strStart;
                else
                {
                    item.EndFieldName = strEnd;

                    if (string.Compare(item.StartFieldName, item.EndFieldName) > 0)
                    {
                        // 交换
                        string strTemp = item.StartFieldName;
                        item.StartFieldName = item.EndFieldName;
                        item.EndFieldName = strTemp;
                    }
                }

                this.Add(item);
            }

            return 0;
        }

        // 是否包含?
        public bool Contains(string strFieldName)
        {
            foreach (FieldNameItem item in this)
            {
                if (item.StartFieldName == "*")
                    return true;

                if (string.Compare(strFieldName, item.StartFieldName) >= 0
                    && string.Compare(strFieldName, item.EndFieldName) <= 0)
                    return true;
            }

            return false;
        }
    }

    // 分列操作类型的字段名列表
    public class OperationRights
    {
        public FieldNameList InsertFieldNames = new FieldNameList();
        public FieldNameList ReplaceFieldNames = new FieldNameList();
        public FieldNameList DeleteFieldNames = new FieldNameList();

        // 解析分列操作的字段名列表
        // parameters:
        //      strText 内容为这样的形态：insert:001-999;replace:001-999;delete:001-999
        //      strDefaultOperation 缺省的操作类型。就是没有冒号情况下当作什么操作 insert/replace/delete 之一
        public int Build(
            string strText,
            string strDefaultOperation,
            out string strError)
        {
            strError = "";

            this.InsertFieldNames.Clear();
            this.ReplaceFieldNames.Clear();
            this.DeleteFieldNames.Clear();

            string[] segments = strText.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in segments)
            {
                string strSegment = s.Trim();
                if (string.IsNullOrEmpty(strSegment) == true)
                    continue;

                string strOperations = "";
                string strList = "";
                int nRet = strSegment.IndexOf(":");
                if (nRet == -1)
                {
                    strOperations = strDefaultOperation;
                    strList = strSegment;
                }
                else
                {
                    strOperations = strSegment.Substring(0, nRet).Trim();
                    strList = strSegment.Substring(nRet + 1).Trim();
                }

                if (string.IsNullOrEmpty(strList) == true)
                {
                    // strList = "###,001-999";
                    strError = "片断 '" + strSegment + "' 中没有定义字段名列表部分";
                    return -1;
                }

                /*
                if (string.IsNullOrEmpty(strOperations) == true)
                    strOperations = "insert,replace,delete";
                 * */

                FieldNameList list = new FieldNameList();
                nRet = list.Build(strList, out strError);
                if (nRet == -1)
                {
                    strError = "字段名列表部分 '" + strList + "' 不合法";
                    return -1;
                }

                List<string> operations = StringUtil.SplitList(strOperations);
                foreach (string operation in operations)
                {
                    string strOperation = operation.Trim();
                    if (string.IsNullOrEmpty(strOperations) == true)
                        continue;
                    if (strOperation == "insert"
                        || strOperation == "i")
                    {
                        this.InsertFieldNames.AddRange(list);
                    }
                    else if (strOperation == "replace"
                        || strOperation == "r")
                    {
                        this.ReplaceFieldNames.AddRange(list);
                    }
                    else if (strOperation == "delete"
                        || strOperation == "d")
                    {
                        this.DeleteFieldNames.AddRange(list);
                    }
                    else if (string.IsNullOrEmpty(strOperation) == true
                        || strOperation == "*"
                        || strOperation == "all")
                    {
                        this.InsertFieldNames.AddRange(list);
                        this.ReplaceFieldNames.AddRange(list);
                        this.DeleteFieldNames.AddRange(list);
                    }
                    else
                    {
                        strError = "定义 '" + strSegment + "' 中出现了无法识别的操作符 '" + strOperation + "' (应为 insert/replace/delete 之一)";
                        return -1;
                    }
                }
            }

            return 0;
        }
    }
}
