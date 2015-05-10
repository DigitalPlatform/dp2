using System;

namespace DigitalPlatform.Xml
{
	public class DeclarationItem : NoneNameTextItem
	{
		internal DeclarationItem(XmlEditor document)
		{
			this.Name = "#Declaration";
			this.m_document = document;
		}

		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return "<?" + this.Name + " " + this.GetValue() + "?>";
		}
	}
}
