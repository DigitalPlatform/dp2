using System;

namespace DigitalPlatform.Xml
{

	public class ProcessingInstructionItem : NoneNameTextItem
	{
		internal ProcessingInstructionItem(XmlEditor document)
		{
			this.Name = "#pi1111";
			this.m_document = document;
		}

		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return "<?" + this.Name + " " + this.GetValue() + "?>";
		}
	}
}
