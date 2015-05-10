using System;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Text;


namespace DigitalPlatform.Xml
{
	// 文本节点
	public class TextItem : Item
	{
		public TextItem()
		{
		}

		internal TextItem(XmlEditor document)
		{
			this.Name = "#text";
			this.m_document = document;
		}


		// parameters:
		//		style	暂不使用.
		public override int Initial(XmlNode node, 
			ItemAllocator allocator,
            object style,
            bool bConnect)
		{
			this.Name = node.Name;
			if (node.Value != null)
			{
				this.SetValue(node.Value);
			}
			else
			{
				Debug.Assert(false,"node节点的Value竟然为null");
			}

			return 0;
		}

		// parameters:
		//		style	初始化风格。暂未使用。
		public override void InitialVisualSpecial(Box boxTotal)
		{
			Text text = new Text ();
			text.Name = "TextOfTextItem";
			text.container = boxTotal;
			Debug.Assert(this.m_paraValue1 != null,"初始值不能为null");
			text.Text = this.GetValue(); //this.m_paraValue;;
			boxTotal.AddChildVisual(text);

			this.m_paraValue1 = null;

/*
			if (this.parent .ReadOnly == true
				|| this.ReadOnly == true)
			{
				text.Editable = false;
			}
*/			
		}
		
        internal override string GetOuterXml(ElementItem FragmentRoot)
		{
			return StringUtil.GetXmlStringSimple(this.GetValue());
		}
	}

}
