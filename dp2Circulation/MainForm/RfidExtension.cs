using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关 RFID 的功能
    /// </summary>
    public partial class MainForm
    {
        /*
<rfid>
	<ownerInstitution>
		<item map="海淀分馆/" isil="test" />
		<item map="西城/" alternative="xc" />
	</ownerInstitution>
</rfid>
    map 为 "/" 或者 "/阅览室" 可以匹配 "图书总库" "阅览室" 这样的 strLocation
    map 为 "海淀分馆/" 可以匹配 "海淀分馆/" "海淀分馆/阅览室" 这样的 strLocation
    最好单元测试一下这个函数
         * */
        // parameters:
        //      cfg_dom 根元素是 rfid
        //      strLocation 纯净的 location 元素内容。
        //      isil    [out] 返回 ISIL 形态的代码
        //      alternative [out] 返回其他形态的代码
        // return:
        //      true    找到。信息在 isil 和 alternative 参数里面返回
        //      false   没有找到
        public static bool GetOwnerInstitution(
            XmlDocument cfg_dom,
            string strLocation,
            out string isil,
            out string alternative)
        {
            isil = "";
            alternative = "";

            if (cfg_dom == null)
                return false;

            // 分析 strLocation 是否属于总馆形态，比如“阅览室”
            // 如果是总馆形态，则要在前部增加一个 / 字符，以保证可以正确匹配 map 值
            // ‘/’字符可以理解为在馆代码和阅览室名字之间插入的一个必要的符号。这是为了弥补早期做法的兼容性问题
            Global.ParseCalendarName(strLocation,
        out string strLibraryCode,
        out string strRoom);
            if (string.IsNullOrEmpty(strLibraryCode))
                strLocation = "/" + strRoom;

            XmlNodeList items = cfg_dom.DocumentElement.SelectNodes(
                "ownerInstitution/item");
            List<HitItem> results = new List<HitItem>();
            foreach (XmlElement item in items)
            {
                string map = item.GetAttribute("map");
                if (strLocation.StartsWith(map))
                {
                    HitItem hit = new HitItem { Map = map, Element = item };
                    results.Add(hit);
                }
            }

            if (results.Count == 0)
                return false;

            // 如果命中多个，要选出 map 最长的那一个返回

            // 排序，大在前
            if (results.Count > 0)
                results.Sort((a, b) => { return b.Map.Length - a.Map.Length; });

            var element = results[0].Element;
            isil = element.GetAttribute("isil");
            alternative = element.GetAttribute("alternative");

            // 2021/2/1
            if (string.IsNullOrEmpty(isil) && string.IsNullOrEmpty(alternative))
            {
                throw new Exception($"map 元素不合法，isil 和 alternative 属性均为空");
            }
            return true;
#if NO
            foreach (XmlElement item in items)
            {
                string map = item.GetAttribute("map");
                if (strLocation.StartsWith(map))
                {
                    isil = item.GetAttribute("isil");
                    alternative = item.GetAttribute("alternative");
                    return true;
                }
            }
#endif

            return false;
        }

        class HitItem
        {
            public XmlElement Element { get; set; }
            public string Map { get; set; }
        }

        // 从册记录的卷册信息字符串中，获得符合 RFID 标准的 SetInformation 信息
        public static string GetSetInformation(string strVolume)
        {
            if (strVolume.IndexOf("(") == -1)
                return null;
            int offs = strVolume.IndexOf("(");
            if (offs == -1)
                return null;
            strVolume = strVolume.Substring(offs + 1).Trim();
            offs = strVolume.IndexOf(")");
            if (offs != -1)
                strVolume = strVolume.Substring(0, offs).Trim();

            strVolume = StringUtil.Unquote(strVolume, "()");
            offs = strVolume.IndexOf(",");
            if (offs == -1)
                return null;
            List<string> parts = StringUtil.ParseTwoPart(strVolume, ",");
            // 2 4 6 字符
            string left = parts[0].Trim(' ').TrimStart('0');
            string right = parts[1].Trim(' ').TrimStart('0');
            if (StringUtil.IsNumber(left) == false
                || StringUtil.IsNumber(right) == false)
                return null;

            // 看值是否超过 0-255
            if (int.TryParse(left, out int v) == false)
                return null;
            if (v < 0 || v > 255)
                return null;
            if (int.TryParse(right, out v) == false)
                return null;
            if (v < 0 || v > 255)
                return null;

            int max_length = Math.Max(left.Length, right.Length);
            if (max_length == 0 || max_length > 3)
                return null;
            return left.PadLeft(max_length, '0') + right.PadLeft(max_length, '0');
        }
    }
}
