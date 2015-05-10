using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
	public class Attributes: BoxExpandable
	{
		#region 重写基类的虚函数

		public override ItemRegion GetRegionName()
		{
			if (this.bExpand == true)
				return ItemRegion.Attributes;
			else
				return ItemRegion.Text ;
		}


		public override void InitialVisual()
		{
			ElementItem item = (ElementItem)this.GetItem();
			if (item == null)
				return;

            // 收缩态
			if (item.m_attrsExpand == ExpandStyle.Collapse)
			{
				this.childrenVisual = null;

				this.MyText.container = this.container ;
				this.MyText.Name = "attributes";
				this.MyText.Text = "尚未初始化"; //item.strAttrsValue;


				// 把this.MyText加进本对象的父亲中,把this从父亲中删除
				((Box)this.container).AddChildVisual(this.MyText);
				((Box)this.container).childrenVisual.Remove (this);

				item.Flush();
			}
			else if (item.m_attrsExpand == ExpandStyle.Expand)
			{
				foreach(AttrItem attr in item.attrs)
				{
					Debug.Assert(attr.GetValue() != null,"准备值不能为null");
					attr.m_paraValue1 = attr.GetValue();
				}

				if (this.childrenVisual != null)
				{
					this.childrenVisual.Clear();
				}

                // 竖向排列
				this.LayoutStyle = LayoutStyle.Vertical;

				foreach(AttrItem attr in item.attrs)
				{
					// 设子元素的style样式与父亲相同
					attr.LayoutStyle = item.LayoutStyle ;
						
					// 把child加到content里作为ChildVisual
					attr.container = this;
					this.AddChildVisual(attr);

					// 此处
					Debug.Assert(attr.m_paraValue1 != null,"此时不应为null");
					// 实现嵌归
					attr.InitialVisual();//elementStyle);
				}
			}
			else 
			{
				Debug.Assert(false, "");
			}
		}


		#endregion
	}

}
