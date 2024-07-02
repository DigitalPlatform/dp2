using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.Text
{
    public class RomanToNumber
    {
        // https://www.c-sharpcorner.com/article/convert-roman-to-numbers-in-c-sharp/
        public static int RomanToInt(string s)
        {
            int sum = 0;
            Dictionary<char, int> romanNumbersDictionary = new Dictionary<char, int>()
            {
                { 'I', 1 },
                { 'V', 5 },
                { 'X', 10 },
                { 'L', 50 },
                { 'C', 100 },
                { 'D', 500 },
                { 'M', 1000 }
            };
            for (int i = 0; i < s.Length; i++)
            {
                char currentRomanChar = s[i];
                romanNumbersDictionary.TryGetValue(currentRomanChar, out int num);
                if (i + 1 < s.Length && romanNumbersDictionary[s[i + 1]] > romanNumbersDictionary[currentRomanChar])
                {
                    sum -= num;
                }
                else
                {
                    sum += num;
                }
            }
            return sum;
        }


        public static string ReplaceRomanDigitToNumber(
            string text)
        {
            // 找到罗马数字所在的范围
            int start = -1;
            int length = 0;
            bool is_upper = true;

            int i = 0;
            foreach (var ch in text)
            {
                if (IsRomanDigit(ch))
                {
                    if (start == -1)
                    {
                        start = i;
                        is_upper = char.IsUpper(ch);
                    }
                    else
                    {
                        var current_is_upper = char.IsUpper(ch);
                        if (current_is_upper != is_upper)
                            break;
                    }

                    length++;
                }
                else
                {
                    if (start != -1)
                        break;
                }

                i++;
            }

            if (start == -1)
                return text;    // 没有找到任何罗马数字部分

            string old_value = text.Substring(start, length).ToUpper();
            string new_value = RomanToInt(old_value).ToString();

            // 把新内容替换到原来的位置
            return text.Substring(0, start)
                + new_value
                + text.Substring(start + length);
        }

        public static bool IsRomanDigit(char ch)
        {
            ch = char.ToUpper(ch);
            /*
I             1
V             5
X             10
L             50
C             100
D             500
M             1000
            * 
             * 
             * */
            switch (ch)
            {
                case 'I':
                case 'V':
                case 'X':
                case 'L':
                case 'C':
                case 'D':
                case 'M':
                    return true;
            }
            return false;
        }

    }
}
