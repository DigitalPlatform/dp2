using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalPlatform
{
    /// <summary>
    /// 索取号实用工具类
    /// </summary>
    public static class AccessNoUtility
    {
        // 检查“索取号-架位号”对照文件的正确性
        // parameters:
        //      style   风格。如果为 loose，表示相邻两行的范围允许出现“交点位置重叠”。缺省为严格模式，表示任何两个范围之间都不允许重叠
        // return:
        //      -1  检查过程出现错误
        //      0   正确
        //      1   有错
        public static int VerifyMapShelfNoFile(string strXmlFilePath,
            string style,
            out string strError)
        {
            strError = "";

            if (File.Exists(strXmlFilePath) == false)
            {
                strError = $"架位对照表文件 '{strXmlFilePath}' 不存在";
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFilePath);
            }
            catch (Exception ex)
            {
                strError = "架位对照表文件 '" + strXmlFilePath + "' 装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            try
            {
                CompleteRange(dom);
            }
            catch (AccessNoException ex)
            {
                strError = "架位对照表文件 '" + strXmlFilePath + "' 出现格式错误: " + ex.Message;
                return -1;
            }

            var loose = style.Split(',').Contains("loose");

            List<string> errors = new List<string>();

            List<string> ranges = new List<string>();
            // 检查所有 accessNoRange 属性内容是否合法
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlElement node in nodes)
            {
                string strRange = node.GetAttribute("accessNoRange");
                if (string.IsNullOrEmpty(strRange))
                    continue;

                // return:
                //      -1  出错
                //      0   没有匹配上
                //      1   匹配上了
                int nRet = MatchRange(strRange,
                    null,
                    out strError);
                if (nRet == -1)
                {
                    errors.Add(strRange + ":" + strError);
                }

                {
                    ranges.AddRange(strRange.Split(new char[] { ',', ';' }));
                }
            }


            // 检查 range 之间是否存在重叠
            for (int i = 0; i < ranges.Count; i++)
            {
                string range1 = ranges[i];
                for (int j = i + 1; j < ranges.Count; j++)
                {
                    string range2 = ranges[j];

                    bool near = j == i + 1; // 是否为相邻范围

                    try
                    {
                        if (near && loose)
                        {
                            if (HasCrossLoose(range1, range2))
                                errors.Add($"范围 '{range1}' 和 '{range2}' 发生了重叠");
                        }
                        else
                        {
                            if (HasCross(range1, range2))
                                errors.Add($"范围 '{range1}' 和 '{range2}' 发生了重叠");
                        }
                    }
                    catch
                    {
                    }
                }
            }

            if (errors.Count > 0)
            {
                strError = "架位对照表文件中发现如下错误:\r\n" + String.Join("\r\n", errors);
                return 1;
            }

            return 0;
        }

        // 通过查找“索取号-架位号”对照文件，用一个索取号获得对应的(理论、永久)架位号
        // parameters:
        //      strDirectory    对照文件所在的目录名。本函数将在这个目录中寻找“馆藏地名.xml”的对照文件
        //      strLocation     馆藏地名。相当于册记录的 location 元素内容的纯净形态
        //      strAccessNo     索取号
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        public static int MapShelfNo(
            string strDirectory,
            string strLocation,
            string strAccessNo,
            out string strShelfNo,
            out string strError)
        {
            strError = "";
            strShelfNo = "";

            string strXmlFilePath = Path.Combine(strDirectory, $"{strLocation.Replace("/", "_")}.xml");
            if (File.Exists(strXmlFilePath) == false)
            {
                strError = $"架位对照表文件 '{strXmlFilePath}' 不存在";
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strXmlFilePath);
            }
            catch (Exception ex)
            {
                strError = "架位对照表文件 '" + strXmlFilePath + "' 装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            try
            {
                CompleteRange(dom);
            }
            catch(AccessNoException ex)
            {
                strError = "架位对照表文件 '" + strXmlFilePath + "' 出现格式错误: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlElement node in nodes)
            {
                string strRange = node.GetAttribute("accessNoRange");
                if (string.IsNullOrEmpty(strRange))
                    continue;

                // return:
                //      -1  出错
                //      0   没有匹配上
                //      1   匹配上了
                int nRet = MatchRange(strRange,
                    strAccessNo,
                    out strError);
                if (nRet == -1)
                {
                    strError = "架位对照表文件中下列片断 '" + node.OuterXml + "' 有错:" + strError;
                    return -1;
                }

                if (nRet == 1)
                {
                    strShelfNo = node.GetAttribute("shelfNo");
                    return 1;
                }
            }

            strError = $"架位对照表文件 '{strXmlFilePath}' 中没有找到和索取号 '{strAccessNo}' 匹配的 shelf 元素";
            return 0;
        }

        // 判断两个 range 是否有重叠部分
        public static bool HasCross(string range1, string range2)
        {
            var result1 = ParseRange(range1);
            var result2 = ParseRange(range2);

            // 1 在 2 的右方，不可能交叉
            if (CompareAccessNo(result1.Start, result2.End) > 0)
                return false;

            // 1 的头和 2 的尾可能交叉
            if (CompareAccessNo(result1.Start, result2.End) == 0)
            {
                if (result1.IncludeStart && result2.IncludeEnd)
                    return true;
                return false;
            }

            // 2 在 1 的右方，不可能交叉
            if (CompareAccessNo(result2.Start, result1.End) > 0)
                return false;

            if (CompareAccessNo(result2.Start, result1.End) == 0)
            {
                if (result2.IncludeStart && result1.IncludeEnd)
                    return true;
                return false;
            }

            return true;
        }

        // 判断两个 range 是否有重叠部分，宽松方式。
        // 宽松方式是指 range1 的末尾和 range2 的开头如果出现点重叠，不算重叠
        public static bool HasCrossLoose(string range1,
            string range2)
        {
            var result1 = ParseRange(range1);
            var result2 = ParseRange(range2);

            // 1 在 2 的右方，不可能交叉
            if (CompareAccessNo(result1.Start, result2.End) > 0)
                return false;

            // 1 的头和 2 的尾可能交叉
            if (CompareAccessNo(result1.Start, result2.End) == 0)
            {
                if (result1.IncludeStart && result2.IncludeEnd)
                    return true;
                return false;
            }

            // 2 在 1 的右方，不可能交叉
            if (CompareAccessNo(result2.Start, result1.End) > 0)
                return false;

            if (CompareAccessNo(result2.Start, result1.End) == 0)
            {
                if (result2.IncludeStart && result1.IncludeEnd)
                {
                    // return true;
                    return false;   // range1 的尾部和 range2 的头部出现点状交叉，特意不算作交叉
                }
                return false;
            }

            return true;
        }

        // 填充完善  shelf/@accessNoRange 属性值中范围的尾部
        static int CompleteRange(XmlDocument dom)
        {
            List<string> errors = new List<string>();

            XmlElement prev_shelf = null;
            ParseResult prev_result = null;

            int count = 0;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlElement node in nodes)
            {
                string strRange = node.GetAttribute("accessNoRange");
                if (string.IsNullOrEmpty(strRange))
                    continue;

                try
                {
                    var current_result = ParseRange(strRange, false);

                    if (prev_result != null)
                    {
                        string error = null;

                        string prev_start = prev_result.Start;
                        string next_start = current_result.Start;
                        if (CompareAccessNo(prev_start, next_start) > 0)
                            error = $"后一个 shelf 的范围开始位置('{next_start}')不应小于前一个 shelf 的范围开始位置('{prev_start}')";

                        if (error != null)
                        {
                            errors.Add(error);
                            prev_result = null;
                            prev_shelf = node;
                            continue;
                        }
                    }

                    if (prev_shelf != null
                        && prev_result != null
                        && current_result != null
                        && string.IsNullOrEmpty(prev_result.End)
                        && string.IsNullOrEmpty(current_result.Start) == false
                        /*&& current_result.IncludeStart == true*/)
                    {
                        string error = null;
                        if (string.IsNullOrEmpty(current_result.Start) == true)
                            error = $"范围 '{strRange}' 不合法: 起始部分为空";
                        if (current_result.IncludeStart == false)
                            error = $"无法为范围 '{prev_shelf.GetAttribute("accessNoRange")}' 自动添加结束部分: 因其后一个 shelf 元素的 accessNoRange 属性值不符合要求(起始部分为“不包含”形态)";

                        if (error != null)
                        {
                            errors.Add(error);
                            prev_result = null;
                            prev_shelf = node;
                            continue;
                        }

                        StringBuilder temp = new StringBuilder();
                        temp.Append(prev_result.ToStart() + "~"
                            + (current_result.IncludeStart == true/* 故意反过来 */ ? "`" : "") + current_result.Start);
                        prev_shelf.SetAttribute("accessNoRange", temp.ToString());
                        count++;
                    }

                    prev_result = current_result;
                    prev_shelf = node;
                }
                catch (AccessNoException ex)
                {
                    errors.Add(ex.Message);
                    prev_result = null;
                    prev_shelf = node;
                    continue;
                }
            }

            if (errors.Count > 0)
            {
                throw new AccessNoException(string.Join("; ", errors));
            }

            return count;
        }


        // 删除每个范围的末端
        public static int RemoveRangeEnd(XmlDocument dom)
        {
            List<string> errors = new List<string>();

            int count = 0;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("shelf");
            foreach (XmlElement node in nodes)
            {
                string strRange = node.GetAttribute("accessNoRange");
                if (string.IsNullOrEmpty(strRange))
                    continue;

                try
                {
                    var result = ParseRange(strRange);

                    if (string.IsNullOrEmpty(result.End) == false)
                    {
                        result.End = null;
                        result.IncludeEnd = false;
                        strRange = result.ToRange();
                        node.SetAttribute("accessNoRange", strRange);
                    }
                }
                catch (AccessNoException ex)
                {
                    errors.Add(ex.Message);
                    continue;
                }
            }

            if (errors.Count > 0)
            {
                throw new AccessNoException(string.Join("; ", errors));
            }

            return count;
        }


        class ParseResult
        {
            public string Start { get; set; }

            public bool IncludeStart { get; set; }

            public string End { get; set; }

            public bool IncludeEnd { get; set; }

            public string ToRange()
            {
                StringBuilder text = new StringBuilder();

                if (String.IsNullOrEmpty(Start) == false)
                {
                    text.Append(ToStart());
                    /*
                    if (IncludeStart == false)
                        text.Append("`" + Start);
                    else
                        text.Append(Start);
                    */
                }

                text.Append("~");

                if (string.IsNullOrEmpty(End) == false)
                {
                    text.Append(ToEnd());
                    /*
                    if (IncludeEnd == false)
                        text.Append("`" + End);
                    else
                        text.Append(End);
                    */
                }

                return text.ToString();
            }

            public string ToStart()
            {
                return ToString(Start, IncludeStart);
            }

            public string ToEnd()
            {
                return ToString(End, IncludeEnd);
            }

            public static string ToString(string pure_text, bool include)
            {
                if (include)
                    return pure_text;
                return "`" + pure_text;
            }
        }

        static ParseResult ParseRange(string strRange,
            bool verify_range = true)
        {
            string[] parts = strRange.Split(new char[] { '~' });
            if (parts.Length != 2)
            {
                throw new AccessNoException("范围字符串 '" + strRange + "' 格式不正确");
            }

            string origin_start = parts[0].Trim();
            string origin_end = parts[1].Trim();
            string start = origin_start;
            string end = origin_end;

            // 识别 start 和 end 中的符号 <>[]
            var include_start = ParseInclude(ref start);
            var include_end = ParseInclude(ref end);

            if (verify_range)
            {
                // 检查 start 和 end 大小关系是否正确
                if (include_start && include_end)
                {
                    if (CompareAccessNo(start, end) > 0)
                    {
                        throw new AccessNoException($"范围 '{origin_start}~{origin_end}' 不合法。起点应该小于等于终点");
                    }
                }
                else
                {
                    // start 和 end 只要有一个不包含，或者都不包含

                    if (CompareAccessNo(start, end) >= 0)
                    {
                        throw new AccessNoException($"范围 '{origin_start}~{origin_end}' 不合法。起点应该小于等于终点");
                    }
                }
            }

            return new ParseResult
            {
                Start = start,
                IncludeStart = include_start,
                End = end,
                IncludeEnd = include_end,
            };
        }

        public class AccessNoException : Exception
        {
            public AccessNoException(string message) : base(message)
            {
            }
        }


        // parameters:
        //      strRange    xxxx~xxxx;xxxx~xxxx;...
        //                  <xxxx~xxxx> 表示头尾都不包含。符号 < 和 > 表示不包含。缺省表示包含
        // return:
        //      -1  出错
        //      0   没有匹配上
        //      1   匹配上了
        public static int MatchRange(string strRangeList,
            string strAccessNo,
            out string strError)
        {
            strError = "";

            string[] ranges = strRangeList.Split(new char[] { ',', ';' });

            foreach (string s in ranges)
            {
                string strRange = s.Trim();
                if (string.IsNullOrEmpty(strRange) == true)
                    continue;

                ParseResult result = null;
                try
                {
                    result = ParseRange(strRange);
                }
                catch (Exception ex)
                {
                    // strError = $"字符串 '{strRangeList}' 中单个索取号范围字符串 '{strRange}' 格式不正确: {ex.Message}";
                    strError = $"索取号范围字符串 '{strRange}' 格式不正确: {ex.Message}";
                    return -1;
                }

                var start = result.Start;
                var end = result.End;
                var include_start = result.IncludeStart;
                var include_end = result.IncludeEnd;
                /*
                string[] parts = strRange.Split(new char[] { '~' });
                if (parts.Length != 2)
                {
                    strError = "字符串 '" + strRangeList + "' 中单个索取号范围字符串 '" + strRange + "' 格式不正确";
                    return -1;
                }

                string origin_start = parts[0].Trim();
                string origin_end = parts[1].Trim();
                string start = origin_start;
                string end = origin_end;

                // 识别 start 和 end 中的符号 <>[]
                var include_start = ParseInclude(ref start);
                var include_end = ParseInclude(ref end);

                // 检查 start 和 end 大小关系是否正确
                if (include_start && include_end)
                {
                    if (CompareAccessNo(start, end) > 0)
                    {
                        strError = $"范围 '{origin_start}~{origin_end}' 不合法。起点应该小于等于终点";
                        return -1;
                    }
                }
                else
                {
                    // start 和 end 只要有一个不包含，或者都不包含

                    if (CompareAccessNo(start, end) >= 0)
                    {
                        strError = $"范围 '{origin_start}~{origin_end}' 不合法。起点应该小于等于终点";
                        return -1;
                    }
                }
                */

                // 2023/5/22
                if (strAccessNo == null)
                    return 0;

                if (include_start)
                {
                    if (CompareAccessNo(strAccessNo, start) < 0)
                        continue;
                }
                else
                {
                    if (CompareAccessNo(strAccessNo, start) <= 0)
                        continue;
                }

                if (include_end)
                {
                    if (CompareAccessNo(strAccessNo, end) > 0)
                        continue;
                }
                else
                {
                    if (CompareAccessNo(strAccessNo, end) >= 0)
                        continue;
                }

                return 1;

                /*
                if (CompareAccessNo(strAccessNo, start) >= 0
                    && CompareAccessNo(strAccessNo, end) <= 0)
                    return 1;
                */
            }
            return 0;
        }

        // 解析出“不包含符号”
        // 默认为包含。不包含表达为 '<1~2'，表示不包含1，包含2;  '1~2>'，表示包含1，不包含2
        // 
        internal static bool ParseInclude(ref string value)
        {
            if (value.StartsWith("`"))
            {
                value = value.Substring(1, value.Length - 1);
                return false;
            }

            if (value.EndsWith("`"))
            {
                value = value.Substring(0, value.Length - 1);
                return false;
            }

            return true;
        }


#if REMOVED
        // 解析出“不包含符号”
        // 默认为包含。不包含表达为 '<1~2'，表示不包含1，包含2;  '1~2>'，表示包含1，不包含2
        // 
        internal static bool ParseInclude(ref string value)
        {
            if (value.StartsWith("["))
            {
                value = value.Substring(1, value.Length - 1);
                return true;
            }

            if (value.StartsWith("<"))
            {
                value = value.Substring(1, value.Length - 1);
                return false;
            }

            if (value.EndsWith("]"))
            {
                value = value.Substring(0, value.Length - 1);
                return true;
            }

            if (value.EndsWith(">"))
            {
                value = value.Substring(0, value.Length - 1);
                return false;
            }

            return true;
        }
#endif

        // 探索版本
        // 比较两个索取号的大小
        // return:
        //      <0  s1 < s2
        //      ==0 s1 == s2
        //      >0  s1 > s2
        public static int CompareAccessNo(string s1,
            string s2,
            bool bRemoveNoSortLine = false)
        {
            if (bRemoveNoSortLine == true)
            {
                // 去掉表示馆藏地的第一行
                if (s1 != null && s1.IndexOf("{") != -1)
                    s1 = BuildLocationClassEntry(s1);
                if (s2 != null && s2.IndexOf("{") != -1)
                    s2 = BuildLocationClassEntry(s2);
            }

            string[] lines1 = s1.Split('/');
            string[] lines2 = s2.Split('/');

            int nRet = 0;
            int nMaxLineCount = Math.Max(lines1.Length, lines2.Length);
            for (int i = 0; i < nMaxLineCount; i++)
            {
                if (i >= lines1.Length) // 2013/3/27 s1.Length BUG!!!
                {
                    if (i >= lines2.Length)
                        return 0;   // 不分胜负
                    return -1;   // s2 更大
                }

                if (i >= lines2.Length)
                {
                    if (i >= lines1.Length)
                        return 0;   // 不分胜负
                    return 1;   // s1 更大
                }

                string line1 = lines1[i];
                string line2 = lines2[i];

                // 第一行采用左对齐进行比较
                if (i == 0)
                {
                    nRet = CompareAccessNoClassLine(line1, line2);
                    if (nRet != 0)
                        return nRet;
                    continue;
                }

                // 其它行用专用方法比较
                nRet = CompareAccessNoRestLine(line1, line2);
                if (nRet != 0)
                    return nRet;
            }

            return 0;
        }

        // 比较一对索取号中的分类号单行
        public static int CompareAccessNoClassLine(string line1,
            string line2)
        {
            var segments1 = CompareSegment.ParseLine(line1.Replace(".", ""));
            var segments2 = CompareSegment.ParseLine(line2.Replace(".", ""));
            int max_count = Math.Max(segments1.Count, segments2.Count);
            for (int i = 0; i < max_count; i++)
            {
                if (i >= segments1.Count)
                {
                    if (segments1.Count == segments2.Count)
                        return 0;
                    return -1;
                }
                if (i >= segments2.Count)
                {
                    if (segments1.Count == segments2.Count)
                        return 0;
                    return 1;
                }
                var seg1 = segments1[i];
                var seg2 = segments2[i];

                var delta = seg1.Prefix - seg2.Prefix;
                if (delta != 0)
                    return delta;

                delta = string.CompareOrdinal(seg1.Text, seg2.Text);
                if (delta != 0)
                    return delta;
            }

            return 0;
        }


        // 比较一对索取号单行
        public static int CompareAccessNoRestLine(string line1,
            string line2)
        {
            var segments1 = CompareSegment.ParseLine(line1);
            var segments2 = CompareSegment.ParseLine(line2);
            int max_count = Math.Max(segments1.Count, segments2.Count);
            for (int i = 0; i < max_count; i++)
            {
                if (i >= segments1.Count)
                {
                    if (segments1.Count == segments2.Count)
                        return 0;
                    return -1;
                }
                if (i >= segments2.Count)
                {
                    if (segments1.Count == segments2.Count)
                        return 0;
                    return 1;
                }
                var seg1 = segments1[i];
                var seg2 = segments2[i];

                var delta = seg1.Prefix - seg2.Prefix;
                if (delta != 0)
                    return delta;

                delta = RightAlignCompare(seg1.Text, seg2.Text);
                if (delta != 0)
                    return delta;
            }

            return 0;
        }

        // 右对齐比较字符串
        // parameters:
        //      chFill  填充用的字符
        public static int RightAlignCompare(string s1, string s2, char chFill = '0')
        {
            if (s1 == null)
                s1 = "";
            if (s2 == null)
                s2 = "";
            int nMaxLength = Math.Max(s1.Length, s2.Length);
            return string.CompareOrdinal(s1.PadLeft(nMaxLength, chFill),
                s2.PadLeft(nMaxLength, chFill));
        }

        // 根据册记录中<accessNo>元素中的原始字符串创建 LocationClass 字符串
        public static string BuildLocationClassEntry(string strCallNumber)
        {
            StringBuilder result = new StringBuilder();
            string[] lines = strCallNumber.Split(new char[] { '/' });
            foreach (string line in lines)
            {
                string strLine = line.Trim();

                // 去掉"{ns}"开头的行
                if (strLine.Length > 0 && strLine[0] == '{')
                {
                    int nRet = strLine.IndexOf("}");
                    if (nRet != -1)
                    {
                        string strCmd = strLine.Substring(0, nRet + 1).Trim().ToLower();
                        if (strCmd == "{ns}")
                            continue;
                        // 否则也要去掉命令部分
                        strLine = strLine.Substring(nRet + 1).Trim();
                    }
                }

                if (result.Length > 0)
                    result.Append("/");
                result.Append(strLine);
            }

            return result.ToString();
        }
    }

    // 用于比较的一个分段。索书号一行可以切割为多个分段
    // 由 Prefix 和 Text 两个成员构成。
    // 比较时，先比较 Prefix；若为分出大小，则继续比较 Text
    // CompareSegment 可以分为三种类型：1) 字母字符 2) 特殊符号 3) 数字字符
    // 字母字符 Segment 的 Prefix 为 0，需要用 Text 来进行比较
    // 特殊符号的 Prefix 用于比较，而 Text 一般为 null
    // 数字字符的 Prefix 为一个恒定的值，需要用 Text 来进行比较
    // 为什么要设计成这样一种结构呢？因为有些特殊符号，不能简单用符号的 char 内码
    // 来进行比较，而要采用一种规定的序。Prefix 就是代表这样一种序的整数
    public class CompareSegment
    {
        // 前置分割符号，已经翻译为排序顺序整数
        public int Prefix { get; set; }
        // 分段文本内容
        public string Text { get; set; }

        public override string ToString()
        {
            return $"Prefix={Prefix},Text='{Text}'";
        }

        // static string _special_chars = ".-:()=\"<>";
        static string _special_chars = "-()\"=<>:+.";
        /*
    关于《中图法》类号排列规则请详见《中图法》第五版使用手册P82/83。
即类号的排列采用由左至右逐位对比的方法进行排列。先比较字母（A~Z）部分，再比较数字（0~9）部分，类号中有辅助符号时，在其前的各位符号（A-Z，0-9）相同的情况下，依下列次序进行比较排列：
- 总论复分号
( ) 国家、地区区分号
““ 种族、民族区分号
= 时代区分号
〈 〉通用时间、地点和环境人员区分号
： 组配符号
＋ 联合符号
例如：
R711 妇科学
R711-62 妇科学手册
R711(711) 加拿大妇科学
R711(711)=535 八十年代的加拿大妇科学
R711=6 二十一世纪妇科学展望
R711＜326＞ 国外妇科学
R711：R83 航海妇科学
R711+R173 妇科学和妇女卫生
R711.1 女性生殖器畸形
        * */

#if ERROR_TEST
        // 获得特殊字符的 index。所谓特殊字符就是用于切割段落的字符
        // 0 字母
        // 1 数字
        // 2和以后 标点符号
        static int GetIndex(char ch, out bool letter_or_digit)
        {
            letter_or_digit = false;
            int index = _special_chars.IndexOf(ch);
            if (index != -1)
                return index + 2;
            if (char.IsLetter(ch))
            {
                letter_or_digit = true;
                return 0;
            }
            else if (char.IsDigit(ch))
            {
                letter_or_digit = true;
                return 1;
            }
            return -1;   // 其它未知符号
        }
#endif

        // 获得特殊字符的 index。所谓特殊字符就是用于切割段落的字符
        // 0 字母
        // 1-n 标点符号
        // n+1 数字
        static int GetIndex(char ch, out bool letter_or_digit)
        {
            letter_or_digit = false;
            int index = _special_chars.IndexOf(ch);
            if (index != -1)
                return index + 1;
            if (char.IsLetter(ch))
            {
                letter_or_digit = true;
                return 0;
            }
            else if (char.IsDigit(ch))
            {
                letter_or_digit = true;
                return _special_chars.Length + 2;
            }
            return -1;   // 其它未知符号
        }

        public static List<CompareSegment> ParseLine(string line)
        {
            if (line == null)
                return new List<CompareSegment>();
            List<CompareSegment> results = new List<CompareSegment>();
            StringBuilder temp = new StringBuilder();
            int temp_index = -1;
            foreach (var ch in line)
            {
                int index = GetIndex(ch, out bool letter_or_digit);
                if ((index != -1 && letter_or_digit == false)
                    || (letter_or_digit == true && index != temp_index))
                {
                    if (temp_index != -1)   // 越过第一次
                        results.Add(new CompareSegment
                        {
                            Prefix = temp_index,
                            Text = temp.ToString()
                        });
                    temp_index = index;
                    temp.Clear();
                    if (letter_or_digit)
                        temp.Append(ch);    // 当前字符也要作为内容利用
                    continue;
                }

                /*
                if (letter_or_digit == true)
                {
                    // var last_char = GetTailChar(temp);
                    // 数字段落和字母段落发生了切换
                    if (index != temp_index)
                    {
                        results.Add(new CompareSegment
                        {
                            Prefix = temp_index,
                            Text = temp.ToString()
                        });
                        temp_index = index;
                        temp.Clear();
                        if (letter_or_digit)
                            temp.Append(ch);    // 当前字符也要作为内容利用
                        continue;
                    }
                }
                */

                temp.Append(ch);
            }

            if (temp.Length > 0)
            {
                results.Add(new CompareSegment
                {
                    Prefix = temp_index,
                    Text = temp.ToString()
                });
            }

            return results;
        }

        static char GetTailChar(StringBuilder text)
        {
            if (text.Length == 0)
                return '\0';
            return text[text.Length - 1];
        }
    }

}
