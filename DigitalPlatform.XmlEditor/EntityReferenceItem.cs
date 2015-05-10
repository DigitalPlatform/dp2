using System;
using System.Xml;

namespace DigitalPlatform.Xml
{

	public class EntityReferenceItem : NoneNameTextItem
	{
		internal EntityReferenceItem(XmlEditor document)
		{
			this.Name = "#EntityReferenceItem";
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
			this.SetValue(node.InnerText);

			//this.ReadOnly = true;

			return 0;
		}

		
		public override void InitialLabelText(Label label)
		{
			label.Text = "&" + this.Name + ";";
		}


		internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return "&" + this.Name + ";";
		}
	}
}
