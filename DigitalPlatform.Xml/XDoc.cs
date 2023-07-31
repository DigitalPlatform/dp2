using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DigitalPlatform.Xml
{
    public class XDoc
    {
        XmlDocument _dom = null;

        public static XDoc Parse(string xml)
        {
            var doc = new XDoc();
            doc.FromString(xml);
            return doc;
        }

        public static XDoc Load(string filename)
        {
            var doc = new XDoc();
            doc.FromFile(filename);
            return doc;
        }

        public void FromString(string xml)
        {
            _dom = new XmlDocument();
            _dom.LoadXml(xml);
        }

        public void FromFile(string filename)
        {
            _dom = new XmlDocument();
            _dom.Load(filename);
        }

        public XElement Root
        {
            get
            {
                if (_dom == null || _dom.DocumentElement == null)
                    return null;
                return new XElement(_dom.DocumentElement);
            }
        }
    }

    public class XAttribute
    {
        XmlAttribute _attr = null;

        public XAttribute(XmlAttribute attr)
        {
            _attr = attr;
        }

        public string Name
        {
            get
            {
                return _attr.Name;
            }
        }

        public string Value
        {
            get
            {
                return _attr.Value;
            }
        }
    }


    public class XNode
    {
        internal XmlNode _node = null;

        public XNode(XmlNode node)
        {
            _node = node;
        }

        public string Name
        {
            get
            {
                return _node.Name;
            }
        }

        public string InnerText
        {
            get
            {
                return _node.InnerText;
            }
        }

        public string InnerXml
        {
            get
            {
                return _node.InnerXml;
            }
        }

        public string OuterXml
        {
            get
            {
                return _node.OuterXml;
            }
        }

        public IEnumerable<XNode> ChildNodes
        {
            get
            {
                var results = new List<XNode>();
                foreach(XmlNode child in _node.ChildNodes)
                {
                    results.Add(new XNode(child));
                }

                return results;
            }
        }
    }

    public class XElement : XNode
    {
        public XElement(XmlElement elment) : base(elment)
        {

        }

        XmlElement _element
        {
            get
            {
                return (XmlElement)_node;
            }
        }

        // TODO: Attribuetes

        public string GetAttribute(string name)
        {
            return _element.GetAttribute(name);
        }

        // 获得全部属性
        public XAttributeList Attributes
        {
            get
            {
                return new XAttributeList(_element.Attributes);
            }
        }

        // 获得一个属性
        public XAttribute Attribute(string name)
        {
            var attr = _element.GetAttributeNode(name);
            return new XAttribute(attr);
        }

        // 获得若干下级元素
        public XElementList Elements(string name)
        {
            if (name == null)
                return new XElementList(_node.SelectNodes("*"));
            return new XElementList(_node.SelectNodes(name));
        }

        // 获得一个下级元素
        public XElement Element(string name)
        {
            var result = _node.SelectSingleNode(name) as XmlElement;
            if (result == null)
                return null;
            return new XElement(result);
        }
    }

    public class XAttributeList : List<XAttribute>
    {
        public XAttributeList(XmlAttributeCollection collection)
        {
            this.Clear();
            foreach(XmlAttribute attr in collection)
            {
                this.Add(new XAttribute(attr));
            }
        }
    }

    public class XElementList : List<XElement>
    {
        public XElementList(XmlNodeList list)
        {
            this.Clear();
            foreach (var node in list)
            {
                if (node is XmlElement)
                    this.Add(new XElement(node as XmlElement));
            }
        }

        public string FirstInnerText
        {
            get
            {
                if (this.Count == 0)
                    return null;
                return this[0].InnerText;
            }
        }

        public string FirstInnerXml
        {
            get
            {
                if (this.Count == 0)
                    return null;
                return this[0].InnerXml;
            }
        }

        public string FirstOuterXml
        {
            get
            {
                if (this.Count == 0)
                    return null;
                return this[0].OuterXml;
            }
        }
    }
}
