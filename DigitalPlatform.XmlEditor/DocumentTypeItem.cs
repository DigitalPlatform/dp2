using System;
using System.Xml;

namespace DigitalPlatform.Xml
{

    public class DocumentTypeItem : NoneNameTextItem
	{
		internal DocumentTypeItem(XmlEditor document)
		{
			this.Name = "#doctype";
			this.m_document = document;
		}

		// parameters:
		//		style	‘›≤ª π”√.
		public override int Initial(XmlNode node, 
			ItemAllocator allocator,
			object style,
            bool bConnect)
		{
			this.Name = node.Name;
			this.SetValue(((XmlDocumentType)node).InternalSubset);

			return 0;
		}


		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return "<!DOCTYPE " + this.Name + " [" + this.GetValue() + "]>";
		}
	}
}
