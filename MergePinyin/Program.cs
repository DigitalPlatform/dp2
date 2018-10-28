using System;
using System.Xml;

namespace MergePinyin
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("MergePinyin usage: mergepinyin pinyin.xml sjhm.xml merge.xml");
                return;
            }
            string pinyin_filename = args[0];
            string sjhm_filename = args[1];
            string merge_filename = args[2];

            XmlDocument pinyin_dom = new XmlDocument();
            pinyin_dom.Load(pinyin_filename);

            XmlDocument sjhm_dom = new XmlDocument();
            sjhm_dom.Load(sjhm_filename);

            // 遍历 sjhm_dom 中的每个 p 元素，合并或者添加到 pinyin_dom 中

            XmlNodeList nodes = sjhm_dom.DocumentElement.SelectNodes("p");
            foreach (XmlElement p in nodes)
            {
                // 汉
                string h = p.GetAttribute("h");
                if (string.IsNullOrEmpty(h))
                    continue;

                Console.Write(h);

                XmlElement target = pinyin_dom.DocumentElement.SelectSingleNode("p[@h='" + h + "']") as XmlElement;
                if (target == null)
                {
                    target = pinyin_dom.CreateElement("p");
                    target.SetAttribute("h", h);
                }

                // *** 添加 s 属性
                target.SetAttribute("s", p.GetAttribute("s"));

                // sjhm.xml 中的拼音
                string p1 = p.GetAttribute("p");
                if (p1 != null)
                    p1 = p1.ToLower();
                // 原来 pinyin.xml 中的拼音
                string old_pinyin = target.GetAttribute("p");
                if (old_pinyin != null)
                {
                    if (old_pinyin != old_pinyin.ToLower())
                    {
                        old_pinyin = old_pinyin.ToLower();
                        target.SetAttribute("p", old_pinyin);
                    }
                }
                if (string.IsNullOrEmpty(p1) == false
                    && p1 != old_pinyin)
                {
                    // *** 添加 p1 属性
                    target.SetAttribute("p1", p1);
                }

            }

            pinyin_dom.Save(merge_filename);
            Console.WriteLine("Finish");
        }
    }
}
