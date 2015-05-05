using System;
using System.Collections.Generic;
using System.Text;

namespace DigitalPlatform.dp2.Statis
{
    // 实用static函数
    public class StatisUtil
    {
        public static string Int64ToPrice(Int64 v)
        {
            // 整数部分
            Int64 v1 = v / 100;

            // 小数部分
            Int64 v2 = v % 100;

            return Convert.ToString(v1) + "." + Convert.ToString(v2).PadLeft(2, '0');
        }
    }
}
