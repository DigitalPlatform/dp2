using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    /// <summary>
    /// 索取号实用工具类
    /// </summary>
    public static class AccessNoUtility
    {
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

            string[] parts1 = s1.Split('/');
            string[] parts2 = s2.Split('/');

            int nRet = 0;
            int nMaxCount = Math.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < nMaxCount; i++)
            {
                if (i >= parts1.Length) // 2013/3/27 s1.Length BUG!!!
                {
                    if (i >= parts2.Length)
                        return 0;   // 不分胜负
                    return 1;   // s2 更大
                }

                if (i >= parts2.Length)
                {
                    if (i >= parts1.Length)
                        return 0;   // 不分胜负
                    return -1;   // s1 更大
                }

                string p1 = parts1[i];
                string p2 = parts2[i];

                // 第一行采用左对齐进行比较
                if (i == 0)
                {
                    nRet = CompareAccessNoClassLine(p1, p2);
                    if (nRet != 0)
                        return nRet;
                    continue;
                }

                // 其它行用专用方法比较
                nRet = CompareAccessNoRestLine(p1, p2);
                if (nRet != 0)
                    return nRet;
            }

            return 0;
        }

        // 比较一对索取号中的分类号单行
        public static int CompareAccessNoClassLine(string line1,
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
    class CompareSegment
    {
        static string _special_chars = ".-:()=\"<>";

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

        // 前置分割符号，已经翻译为排序顺序整数
        public int Prefix { get; set; }
        // 分段文本内容
        public string Text { get; set; }

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
