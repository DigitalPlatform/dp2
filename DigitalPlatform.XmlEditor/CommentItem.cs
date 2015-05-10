using System;

namespace DigitalPlatform.Xml
{

	public class CommentItem : NoneNameTextItem
	{
		internal CommentItem(XmlEditor document)
		{
			this.Name = "#comment";
			this.m_document = document;
		}

		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return "<!--" + this.GetValue() + "-->";
		}
	}
}
