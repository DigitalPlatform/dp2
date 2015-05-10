using System;

namespace DigitalPlatform.Xml
{	
	// 类似于处理指令那样的文本节点
	// 其特点为:
	//	1) label显示的名字左边有'?'
	//	2) this.Value不作转义(而TextItem是要做转义的)
	public class NoneNameTextItem : TextItem
	{

		public virtual void InitialLabelText(Label label)
		{
			// 一点点差异
			if (this.Name.Length > 0 && this.Name[0] == '#')
				label.Text = this.Name;	
			else
				label.Text = "? " + this.Name;
		}


		// 本函数对于派生类来说一般不要重载。一般重载InitialVisualSpecial()即可。
		// 因为本函数代码前后部分基本是共用的，只有中段采用调用InitialVisualSpecial()的办法。
		// 不过如果重载了本函数，并且不想在其中调用InitialVisualSpecial()，则需要自行实现全部功能
		public override void  InitialVisual()
		{
			if (this.childrenVisual != null)
				this.childrenVisual.Clear();

			// 加Label
			Label label = new Label();
			label.container = this;

			InitialLabelText(label);

			this.AddChildVisual(label);


			// 定义一个总框
			Box boxTotal = new Box ();
			boxTotal.Name = "BoxTotal";
			boxTotal.container = this;
			this.AddChildVisual (boxTotal);

			// 外面总框的layoutStyle样式为竖排
			boxTotal.LayoutStyle = LayoutStyle.Vertical;

			///
			InitialVisualSpecial(boxTotal);
			///

			//如果boxTotal只有一个box，则设为横排
			if (boxTotal.childrenVisual != null && boxTotal.childrenVisual .Count == 1)
				boxTotal.LayoutStyle = LayoutStyle.Horizontal ;

/*
			Comment comment = new Comment ();
			comment.container = this;
			this.AddChildVisual(comment);
*/			
		}
	}
}
