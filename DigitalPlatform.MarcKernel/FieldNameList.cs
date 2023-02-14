using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using DigitalPlatform.Text;

namespace DigitalPlatform.Marc
{
    public class FieldNameItem
    {
        public string StartFieldName = "";
        public string EndFieldName = "";

        public static FieldNameItem Build(string strSegment,
            out string strError)
        {
            strError = "";

            if (strSegment.Contains(","))
            {
                strError = "strSegment 中不允许包含逗号。只能是一个起止范围形态，不允许是多个起止范围的形态";
                return null;
            }

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
                return null;
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
                    return null;
                }
            }
            else
            {
                strStart = strSegment;
            }

            if (strStart.Length != 3)
            {
                strError = "字段名 '" + strStart + "' 不合法，应当为3字符";
                return null;
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

            return item;
        }

        public override string ToString()
        {
            if (StartFieldName == "*" && EndFieldName == "*")
                return "***";
            if (StartFieldName == EndFieldName)
                return StartFieldName;
            if (string.IsNullOrEmpty(EndFieldName))
                return StartFieldName;
            return StartFieldName + "-" + EndFieldName;
        }

        public bool IsAll()
        {
            if (StartFieldName == "*" && EndFieldName == "*")
                return true;
            return false;
        }

        // 判断是否头标区
        public bool IsHeader()
        {
            if (StartFieldName == "###")
                return true;
            return false;
        }

        // 检查两个范围是否相交
        public static bool IsCross(FieldNameItem item1,
            FieldNameItem item2)
        {
            if (item1.IsAll())
                return true;
            if (item2.IsAll())
                return true;

            if (item1.IsHeader() && item2.IsHeader())
                return true;

            // 两个中只有一个是头标区
            // 一个是头标区，另一个不是，那么就不相交
            if (item1.IsHeader() || item2.IsHeader())
                return false;

            // start1 -- end1 start2 -- end2
            if (string.Compare(item1.EndFieldName, item2.StartFieldName) < 0)
                return false;

            // start2 -- end2 start1 -- end1
            if (string.Compare(item2.EndFieldName, item1.StartFieldName) < 0)
                return false;

            return true;
        }

        // 从 list1 中减去 list2 代表的范围
        public static FieldNameList Sub(FieldNameList list1, FieldNameList list2)
        {
            FieldNameList results = list1;
            foreach (var item2 in list2)
            {
                results = FieldNameItem.Sub(results, item2);
                if (results.Count == 0)
                    break;
            }
            return results;
        }

        // 从 list 中减去一个 item 代表的范围
        public static FieldNameList Sub(FieldNameList list, FieldNameItem item)
        {
            FieldNameList results = new FieldNameList();
            foreach (var current_item in list)
            {
                results.AddRange(FieldNameItem.Sub(current_item, item));
            }
            return results;
        }

        // item1 减去 item2
        public static FieldNameList Sub(FieldNameItem item1, FieldNameItem item2)
        {
            if (item1.IsAll() && item2.IsAll())
                return new FieldNameList(); // 空集合

            if (IsCross(item1, item2) == false)
                return new FieldNameList { item1 };

            FieldNameList results = new FieldNameList();
            if (item1.IsAll())
            {
                // item1 转换为 ###,000-999 形态
                results.Add(FieldNameItem.Build("###", out _));
                results.Add(new FieldNameItem { StartFieldName = "000", EndFieldName = "999" });

                // 然后做减法
                return FieldNameItem.Sub(results, item2);
                /*
                // ###,000-999
                results.Add(new FieldNameItem { StartFieldName = "###" });
                if (string.Compare(item2.StartFieldName, "000") > 0)
                    results.Add(new FieldNameItem
                    {
                        StartFieldName = "000",   // 表示尽可能小
                        EndFieldName = PrevNumber(item2.StartFieldName) // prev
                    });
                if (string.Compare(item2.EndFieldName, "999") < 0)
                    results.Add(new FieldNameItem
                    {
                        StartFieldName = NextNumber(item2.EndFieldName),    // next
                        EndFieldName = "999"  // 表示尽可能大
                    });
                return results;
                */
            }

            if (item2.IsAll())
                return new FieldNameList(); // 空集合

            if (item1.IsHeader() && item2.IsHeader())
                return new FieldNameList(); // 空集合

            if (item1.IsHeader() && item2.IsHeader() == false)
                return new FieldNameList { item1 };

            if (item1.IsHeader() == false && item2.IsHeader())
                return new FieldNameList { item1 };

            // [start1 --                  -- end1]
            //            [start2 -- end2]
            // 第一段                      第二段
            if (string.Compare(item1.StartFieldName, item2.StartFieldName) <= 0
                && string.Compare(item2.EndFieldName, item1.EndFieldName) <= 0)
            {
                // 第一段
                try
                {
                    var new_item = new FieldNameItem
                    {
                        StartFieldName = item1.StartFieldName,
                        EndFieldName = PrevNumber(item2.StartFieldName)
                    };
                    if (IsValid(new_item))
                        results.Add(new_item);
                }
                catch(OverflowException)
                {

                }

                // 第二段
                {
                    var new_item = new FieldNameItem
                    {
                        StartFieldName = NextNumber(item2.EndFieldName),    // next
                        EndFieldName = item1.EndFieldName
                    };
                    if (IsValid(new_item))
                        results.Add(new_item);
                }
                return results;
            }

            // [start1 --         -- end1]
            //            [start2 --        -- end2]
            if (string.Compare(item1.StartFieldName, item2.StartFieldName) <= 0
    && string.Compare(item1.EndFieldName, item2.EndFieldName) <= 0)
            {
                // 只有一段
                try
                {
                    var new_item = new FieldNameItem
                    {
                        StartFieldName = item1.StartFieldName,
                        EndFieldName = PrevNumber(item2.StartFieldName)
                    };
                    if (IsValid(new_item))
                        results.Add(new_item);
                }
                catch(OverflowException)
                {

                }

                return results;
            }

            //         [start1 -- end1]
            // [start2                   -- end2]
            // 空
            if (string.Compare(item2.StartFieldName, item1.StartFieldName) <= 0
    && string.Compare(item1.EndFieldName, item2.EndFieldName) <= 0)
            {
                return new FieldNameList();
            }

            //         [start1          -- end1]
            // [start2         -- end2]
            //                          只有一段
            if (string.Compare(item2.StartFieldName, item1.StartFieldName) <= 0
&& string.Compare(item2.EndFieldName, item1.EndFieldName) <= 0)
            {
                // 只有一段
                {
                    var new_item = new FieldNameItem
                    {
                        StartFieldName = NextNumber(item2.EndFieldName),
                        EndFieldName = item1.EndFieldName
                    };
                    if (IsValid(new_item))
                        results.Add(new_item);
                }

                return results;
            }

            throw new Exception("不可能走到这里");
        }

        static bool IsValid(FieldNameItem item)
        {
            if (item.StartFieldName.Length > 3 || item.EndFieldName.Length > 3)
                return false;

            if (string.Compare(item.StartFieldName, item.EndFieldName) <= 0)
                return true;
            return false;
        }

        static string NextNumber(string number)
        {
            // TODO: '9' 后面应该是 'A'。数字和字母组成的进制
            return StringUtil.IncreaseNumber(number, 1);
        }

        static string PrevNumber(string number)
        {
            if (number == "000")
                throw new OverflowException();
            // TODO: 'A' 前面应该是 '9'。数字和字母组成的进制
            return StringUtil.IncreaseNumber(number, -1);
        }
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

#if OLDCODE
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
#endif
                var item = FieldNameItem.Build(strSegment, out strError);
                if (item == null)
                    return -1;

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

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            foreach (FieldNameItem item in this)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append(item.ToString());
            }

            return text.ToString();
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
