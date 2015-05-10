using System;

namespace DigitalPlatform.Xml
{
    public class CDATAItem : NoneNameTextItem
    {
        internal CDATAItem(XmlEditor document)
        {
            this.Name = "#cdata-section";
            this.m_document = document;
        }

        internal override string GetOuterXml(ElementItem FragmentRoot)
        {
            return "<![CDATA[" + this.GetValue() + "]]>";
        }
    }
}
