using System;
using System.Xml;

namespace DigitalPlatform.Xml
{
	// ÐéÀà
	public abstract class ItemAllocator
	{
		public virtual Item newItem(XmlNode node,
			XmlEditor document)
		{
			return null;
		}
	}

	public class MyItemAllocator : ItemAllocator
	{
		public override Item newItem(XmlNode node,
			XmlEditor document)
		{
			Item item = null;

			if (node.NodeType == XmlNodeType.Element) 
			{
				item = new ElementItem(document);
			}
			else if (node.NodeType == XmlNodeType.Attribute )
			{
				item = new AttrItem(document);
			}
			else if (node.NodeType == XmlNodeType.Text) 
			{
				item = new TextItem(document);
			}
			else if (node.NodeType == XmlNodeType.ProcessingInstruction )
			{
				item = new ProcessingInstructionItem(document);
			}
			else if (node.NodeType == XmlNodeType.XmlDeclaration )
			{
				item = new DeclarationItem(document);
			}
			else if (node.NodeType == XmlNodeType.Comment)
			{
				item = new CommentItem(document);
			}
			else if (node.NodeType == XmlNodeType.CDATA)
			{
				item = new CDATAItem(document);
			}
			else if (node.NodeType == XmlNodeType.DocumentType)
			{
				item = new DocumentTypeItem(document);
			}
			else if (node.NodeType == XmlNodeType.EntityReference)
			{
				item = new EntityReferenceItem(document);
			}

			//item.m_document = document;
			return item;
		}
	}

}
