using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// 2021/10/22
// 将从 https://www.loc.gov/marc/countries/countries_code.html
// 获得的 MARC 国家代码条目转换为 marcvaluelist 文件中的 XML 格式

/*
    <ValueList name="008_15/3">
        <Item>
            <Value>eng</Value>
            <Label xml:lang="en">english</Label>
            <Label xml:lang="zh">英语</Label>
        </Item>

 * 
 * */

namespace ConvertMarcCountriesCode
{
    class Program
    {
        static void Main(string[] args)
        {
            string source_path = Path.Combine(Environment.CurrentDirectory, "source.txt");
            string target_path = Path.Combine(Environment.CurrentDirectory, "target.xml");

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            var lines = File.ReadAllLines(source_path, Encoding.UTF8);
            foreach(var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith("#"))
                    continue;

                int pos = line.IndexOf("\t");
                if (pos == -1)
                    continue;

                string left = line.Substring(0, pos).Trim();
                string right = line.Substring(pos + 1).Trim();

                if (left.StartsWith("-"))
                {
                    right = "(Discontinued)" + right;
                    left = left.Substring(1);
                }

                // 确保 left 为 3 chars
                if (left.Length < 3)
                    left = left.PadRight(3, '_');

                var item = dom.CreateElement("Item");
                dom.DocumentElement.AppendChild(item);

                var value = dom.CreateElement("Value");
                value.InnerText = left;
                item.AppendChild(value);

                var label = dom.CreateElement("Label");
                label.InnerText = right;
                var attr = dom.CreateAttribute("xml", "lang", "http://www.w3.org/XML/1998/namespace");
                attr.Value = "en";
                label.SetAttributeNode(attr);
                // label.SetAttribute("lang", , "en");
                item.AppendChild(label);
            }

            dom.Save(target_path);
        }
    }
}
