using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
	public class Content : BoxExpandable
	{
		public override ItemRegion GetRegionName()
		{
			if (this.bExpand == true)
				return ItemRegion.Content;
			else
				return ItemRegion.Text;
		}


		// 初始visual函数
		public override void InitialVisual()
		{
			ElementItem item = (ElementItem)this.GetItem();
			XmlEditor editor = item.m_document;
			if (item == null)
				return;

			if (item.m_childrenExpand == ExpandStyle.Collapse)
			{
				this.childrenVisual = null;

				this.MyText.container = this.container;
				this.MyText.Name = "content";

				this.MyText.Text = "尚未初始化";
				// 使用初始item时，赋的值
				//this.MyText.Text = item.strInnerXml;	// m_strContentXml专门用来传递处于收缩状态的element的下级汇总Xml信息


				//把本对象从父亲哪里删除，把m_text加进父亲,
				//实现了实质的替换，不知道以后这个没人管的content跑到哪里了？
				((Box)this.container).AddChildVisual(this.MyText);
				((Box)this.container).childrenVisual.Remove (this);

				item.Flush();
				
			}
			else if (item.m_childrenExpand == ExpandStyle.Expand)
			{
				// 把原初始设好
				foreach(Item child in item.children)
				{
					if (!(child is ElementItem))
					{
						Debug.Assert(child.GetValue() != null,"准备值不能为null");
						child.m_paraValue1 = child.GetValue();
					}
				}

				if (this.childrenVisual != null)
					this.childrenVisual.Clear ();

				this.LayoutStyle = LayoutStyle.Vertical ;

				foreach(Item child in item.children)
				{
					//设子元素的style样式与父亲相同
					child.LayoutStyle = item.LayoutStyle ;
						
					//把child加到content里作为ChildVisual
					child.container = this;
					this.AddChildVisual (child);


					//实现嵌归
					child.InitialVisual();

				}
				
			}
			else 
			{
				Debug.Assert(false, "");
			}
		}


		public override void Layout(int x,
			int y,
			int nInitialWidth,
			int nInitialHeight,
			int nTimeStamp,
			out int nRetWidth,
			out int nRetHeight,
			LayoutMember layoutMember)
		{
			nRetWidth = nInitialWidth;
			nRetHeight = nInitialHeight;

			bool bEnlargeWidth = false;
			if ((layoutMember & LayoutMember.EnlargeWidth  ) == LayoutMember.EnlargeWidth  )
				bEnlargeWidth = true;

			bool bEnlargeHeight = false;
			if ((layoutMember & LayoutMember.EnLargeHeight ) == LayoutMember.EnLargeHeight )
				bEnlargeHeight = true;

			if (bEnlargeWidth == true
				|| bEnlargeHeight == true)
			{
				if (LayoutStyle == LayoutStyle.Horizontal )
				{
					if (bEnlargeHeight == true)
						this.Rect.Height  = nInitialHeight;
				}
				else if (LayoutStyle == LayoutStyle.Vertical )
				{
					if (bEnlargeWidth == true)
						this.Rect.Width = nInitialWidth;
				}

				if ((layoutMember & LayoutMember.Up ) != LayoutMember.Up )
					return;	
			}

			base.Layout (x,
				y,
				nInitialWidth,
				nInitialHeight,
				nTimeStamp,
				out nRetWidth,
				out nRetHeight,
				layoutMember);
		}


	}

}
