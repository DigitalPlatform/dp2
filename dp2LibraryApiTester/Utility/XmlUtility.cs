using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace dp2LibraryApiTester
{
    public static class XmlUtility
    {
        public static List<string> GetDprmsFileIds(XmlDocument dom)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DigitalPlatform.Xml.DpNs.dprms);

            var nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            return nodes.Cast<XmlElement>().Select(o => o.GetAttribute("id")).ToList();
        }

        public static int RemoveDprmsFileElements(XmlDocument dom, string id = null)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DigitalPlatform.Xml.DpNs.dprms);

            int count = 0;
            var nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement file in nodes)
            {
                if (string.IsNullOrEmpty(id) == false
                    && file.GetAttribute("id") != id)
                    continue;
                file.ParentNode.RemoveChild(file);
                count++;
            }

            return count;
        }

        public static void AddDprmsFileElement(XmlDocument dom, string id)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            nsmgr.AddNamespace("dprms", DigitalPlatform.Xml.DpNs.dprms);

            var element = dom.CreateElement("dprms", "file", DigitalPlatform.Xml.DpNs.dprms);
            dom.DocumentElement.AppendChild(element);
            element.SetAttribute("id", id);
        }


        // 方法 A: 使用 C14N 规范化并比较
        public static string CanonicalizeByC14N(string xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            var doc = new XmlDocument();
            // C14N 要求保留白空以得到稳定结果
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);

            var transform = new XmlDsigC14NTransform();
            // 将整个文档作为输入
            transform.LoadInput(doc);

            using (var stream = (Stream)transform.GetOutput(typeof(Stream)))
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static bool AreEqualByC14N(string xml1, string xml2)
        {
            if (xml1 == null || xml2 == null) return string.Equals(xml1, xml2);
            var c1 = CanonicalizeByC14N(xml1);
            var c2 = CanonicalizeByC14N(xml2);
            return string.Equals(c1, c2, StringComparison.Ordinal);
        }

        // 方法 B: 使用 XDocument 规一化（移除空白文本、按属性排序）然后 DeepEquals
        public static XDocument NormalizeXDocument(string xml)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            // 删除仅包含空白的文本节点
            var texts = doc.DescendantNodes().OfType<XText>().Where(t => string.IsNullOrWhiteSpace(t.Value)).ToList();
            foreach (var t in texts) t.Remove();

            // 规范化元素属性顺序（便于字符串比较）
            foreach (var el in doc.Descendants())
            {
                var attrs = el.Attributes().OrderBy(a => a.Name.NamespaceName).ThenBy(a => a.Name.LocalName).ToArray();
                if (attrs.Length > 0)
                    el.ReplaceAttributes(attrs);
            }

            return doc;
        }

        public static bool AreEqualByXNode(string xml1, string xml2)
        {
            if (xml1 == null || xml2 == null) return string.Equals(xml1, xml2);
            try
            {
                var d1 = NormalizeXDocument(xml1);
                var d2 = NormalizeXDocument(xml2);
                return XNode.DeepEquals(d1, d2);
            }
            catch
            {
                // 如果任何一方不是合法 XML，回退到字符串比较
                return string.Equals(xml1, xml2);
            }
        }
    }
}
