using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using System.IO;

using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using System.Diagnostics;

namespace DigitalPlatform.Xml
{
	public abstract class Item : Box,IXPathNavigable
	{
		#region 成员变量

		public ElementItem parent = null;       // 父亲
		public XmlEditor m_document = null;     // XmlEditor

		public string BaseURI = "";  // XPathNavigator的需要,任何节点都有BaseURI
		
		// 当没有对应的visual结构时,用m_paraValue表示该Item的值
		// 当有了visual结构,则直接使用visual的text
		// 本变量对ElementItem对象无意义
		public string m_paraValue1 = "";

		public bool m_bConnected = false;

		#endregion

	

		#region 关于样式配置文件的一些属性

		/////////////////////////////////////////////////////////
		// some property about cfg
		/////////////////////////////////////////////////////////

		public override ItemRegion GetRegionName()
		{
			return ItemRegion.Frame;
		}


		// 供GetBackColor(region)、GetTextColor(region)、GetBorderColor(region)调的私有函数
        // parameters:
		//      region        枚举值，取哪个区域
		//      valueStyle    枚举值，取哪种类型的值
        // return:
        //      Color对象
		Color GetColor(ItemRegion region,ValueStyle valueStyle)
		{
			XmlEditor editor = this.m_document;
			if (editor.VisualCfg == null)
				goto END1;

			VisualStyle style = editor.VisualCfg.GetVisualStyle(this,region);
			if (style == null)
				goto END1;

			if (valueStyle == ValueStyle.BackColor )
				return style.BackColor ;
			else if (valueStyle == ValueStyle.TextColor )
				return style.TextColor ;
			else if (valueStyle == ValueStyle.BorderColor )
				return style.BorderColor  ;
			
			END1:
				//缺省值
				if (valueStyle == ValueStyle.BackColor)
				{
                    if (region == ItemRegion.Text)
                        return editor.BackColorDefaultForEditable;
                    else if (this is AttrItem)
                        return editor.AttrBackColorDefault;
                    else
                        return editor.BackColorDefault;
				}
				else if (valueStyle == ValueStyle.TextColor )
				{
					return editor.TextColorDefault;
				}
				else if (valueStyle == ValueStyle.BorderColor )
				{
					return editor.BorderColorDefault;
				}
				else 
				{
					return Color.Red;
				}
		}
	
		
		// 供尺寸值得调的私有函数.
        // parameters:
		//      region      枚举值，取哪个区域
		//      valueStyle  枚举值，取哪种类型的值
		int GetPixelValue(ItemRegion region,ValueStyle valueStyle)
		{
			XmlEditor editor = this.m_document;

			if (editor == null)
				goto END1;

			if (editor.VisualCfg == null)
				goto END1;

			VisualStyle style = editor.VisualCfg.GetVisualStyle(this,region);
			if (style == null)
				goto END1;

			if (valueStyle == ValueStyle.LeftBlank)
				return style.LeftBlank ;
			else if (valueStyle == ValueStyle.RightBlank)
				return style.RightBlank ;
			else if (valueStyle == ValueStyle.TopBlank)
				return style.TopBlank ;
			else if (valueStyle == ValueStyle.BottomBlank)
				return style.BottomBlank ;
			else if (valueStyle == ValueStyle.TopBorderHeight)
				return style.TopBorderHeight;
			else if (valueStyle == ValueStyle.BottomBorderHeight)
				return style.BottomBorderHeight;
			else if (valueStyle == ValueStyle.LeftBorderWidth)
				return style.LeftBorderWidth;
			else if (valueStyle == ValueStyle.RightBorderWidth)
				return style.RightBorderWidth;			
			END1:
				//缺省值
				if (valueStyle == ValueStyle.LeftBlank )
				{
					if (region == ItemRegion.ExpandAttributes 
						|| region == ItemRegion.ExpandContent )
						return 2;  //?
					else
						return 0;
				}
				else if (valueStyle == ValueStyle.RightBlank)
				{
					if (region == ItemRegion.ExpandAttributes 
						|| region == ItemRegion.ExpandContent )
						return 2;  // ?
					else
						return 0;
				}
				else if (valueStyle == ValueStyle.TopBlank)
					return 0;
				else if (valueStyle == ValueStyle.BottomBlank )
					return 0;
				else if (valueStyle == ValueStyle.TopBorderHeight)
				{
					return -1;
				}
				else if (valueStyle == ValueStyle.BottomBorderHeight)
				{
					return -1;
				}
				else if (valueStyle == ValueStyle.LeftBorderWidth)
				{
					return -1;
				}
				else if (valueStyle == ValueStyle.RightBorderWidth)
				{
					return -1;
				}

			return 0;
		}

		
		// 得到字体
        // parameters:
		//      region  枚举值，哪个区域的字体
        // return:
        //      Font对象
		public Font GetFont(ItemRegion region)
		{
			XmlEditor editor = this.m_document;
			if (editor.VisualCfg == null)
				goto END1;
			
			VisualStyle style = editor.VisualCfg.GetVisualStyle (this,region);
			if (style == null)
				goto END1;

			return style.Font ;
		
			END1:
				return editor.FontTextDefault;
		}

		
		// 以下为方便使用的具体小函数/////////////////////////////////////////
		public int GetLeftBlank(ItemRegion region)
		{
			return GetPixelValue(region,ValueStyle.LeftBlank );
		}


		public int GetRightBlank(ItemRegion region)
		{
			return GetPixelValue(region,ValueStyle.RightBlank );
		}


		public int GetTopBlank(ItemRegion region)
		{
			return GetPixelValue(region,ValueStyle.TopBlank );
		}


		public int GetBottomBlank(ItemRegion region)
		{
			return GetPixelValue(region,ValueStyle.BottomBlank );
		}


		public Color GetBackColor(ItemRegion region)
		{
			return GetColor(region,ValueStyle.BackColor);
		}


		public Color GetTextColor(ItemRegion region)
		{
			return GetColor(region,ValueStyle.TextColor);
		}

		public int GetTopBorderHeight(ItemRegion region)
		{
			return this.GetPixelValue(region,ValueStyle.TopBorderHeight);
		}

		public int GetBottomBorderHeight(ItemRegion region)
		{
			return this.GetPixelValue(region,ValueStyle.BottomBorderHeight);
		}

		public int GetLeftBorderWidth(ItemRegion region)
		{
			return this.GetPixelValue(region,ValueStyle.LeftBorderWidth);
		}

		public int GetRightBorderWidth(ItemRegion region)
		{
			return this.GetPixelValue(region,ValueStyle.RightBorderWidth);
		}

		public Color GetBorderColor(ItemRegion region)
		{
			return GetColor(region, ValueStyle.BorderColor  );
		}

		#endregion

		#region 关于宽度数组中配置信息的函数

		// 给数组中设值
		public void SetValue(string strName,int nValue)
		{
			ItemWidth width = m_document.widthList. GetItemWidth(this.GetLevel());
			if (width != null)
			{
				width.SetValue (strName,nValue);
			}
		}

		// 从数组中取值
		public int GetValue(string strName)
		{
			ItemWidth width = m_document.widthList. GetItemWidth(this.GetLevel());
			if (width == null)
				return -1;

			return width.GetValue (strName);
		}


		public PartWidth GetPartWidth(string strName)
		{
			ItemWidth width = m_document.widthList. GetItemWidth(this.GetLevel());

			if (width == null)
				return null;

			return width.GetPartWidth  (strName);
		}

		#endregion

		#region 公共函数
        
		public ElementItem Parent
		{
			get
			{
				return this.parent;
			}
		}

		// 对外公布的属性
		public string Value
		{
			get
			{
				return this.GetValue();
			}
			set
			{
				this.SetValue(value);
			}
		}

		// 得到层号, 从1开始计数(1是虚根)
		internal int GetLevel()
		{
			int nLevel = 1;
			Item item = this;
			while(true) 
			{
				if (item == null)
					break;
				item = item.parent;
				nLevel ++;
			}
			return nLevel;
		}

		// 得到XPath
		public string GetXPath()
		{
			ElementItem root = (ElementItem)this.m_document.docRoot;

			string strPath;
			ItemUtil.Item2Path (root,this,out strPath);
			return strPath;
		}

		// 对ElementItem无意义,而换成GetContentText() 和 GetAttributesText()
		// 一定是在初始化好visual结构,才能调此函数
		// region:
		//		-1	一般的文本节点，不分区域
		//		0	属性区
		//		1	儿子区
		// style:
		//		当前区域的状态
		internal virtual Text GetVisualText()
		{
			if (this.m_paraValue1 != null)
			{
				Debug.Assert(false,"还没有初始化visual结构,不能调此函数");
				return null;
			}

			if (this.childrenVisual == null)
			{
				Debug.Assert(false,"还没有初始化visual结构,不能调此函数");
				return null;
			}

			Box boxTotal = this.GetBoxTotal();
			if (boxTotal == null)
			{
				Debug.Assert(false,"不可能没有BoxTotal");
				return null;
			}


			if (boxTotal.childrenVisual == null)
				return null;

			Text text = null;
			foreach(Visual childVisual in boxTotal.childrenVisual)
			{
				if (childVisual is Text
					/*&& childVisual.Name == ""*/)
				{
					text = (Text)childVisual;
				}
			}

			if (text == null)
			{
				Debug.Assert(false,"不可能没有Text对象");
				return null;
			}
			return text;
		}

	
		// ElementItem要重写这个函数
		public virtual string GetValue()
		{
			string strValue = null;

			if (this.m_paraValue1 == null)
			{
				// 说明已建立的visual结构,从visual里得到值
				Text text = this.GetVisualText();
				if (text == null)
				{
					Debug.Assert(false,"m_paraValue为null时,visual不可能不存在");
					throw new Exception("m_paraValue为null时,visual不可能不存在");
				}
				Debug.Assert(text.Text != null,"");
				strValue = text.Text;  //注意用转换成xmlstring
			}
			else
			{
				strValue = this.m_paraValue1;
			}

			return StringUtil.GetVisualableStringSimple(strValue);
		}

		// ElementItem要重写这个函数
		public virtual void SetValue(string strValue)
		{
			strValue = StringUtil.GetXmlStringSimple(strValue);

			if (this.m_paraValue1 == null)
			{
				// 说明已建立的visual结构,从visual里得到值
				Text text = this.GetVisualText();
				if (text == null)
				{
					Debug.Assert(false,"m_paraValue为null时,visual不可能不存在");
					throw new Exception("m_paraValue为null时,visual不可能不存在");
				}
				text.Text = strValue;  //??????还用转换成xmlstring?不用
			}
			else
			{
				this.m_paraValue1 = strValue;
			}

			//????
			ElementItem myParent = this.parent;
			if (myParent != null)
				myParent.m_objAttrsTimestamp++;
		}

		// ElementItem	<a>test</a>
		// AttrItem	a="test"
		// TextItem test
		// 派生类不必要再重写该属性,只需重写GetOutXml()就可以了
		public virtual string OuterXml 
		{
			get
			{
				return this.GetOuterXml(null);
			}
			set 
			{
				throw(new Exception("尚未实现OuterXml set功能"));
			}
		}

		// parameter:
		//		FragmentRoot	是否带名字空间,该参数仅对ElementItem有意义
		internal virtual string GetOuterXml(ElementItem FragmentRoot)
		{
			return "";
		}



		// 得到childrenVisual中的Label对象
		public Label GetLable()
		{
			if (this.childrenVisual == null)
				return null;

			Label label = null;
			for(int i=0;i<this.childrenVisual.Count;i++)
			{
				Visual visual = (Visual)this.childrenVisual[i];
				if (visual is Label)
				{
					label = (Label)visual;
					break;
				}
			}
			return label;
		}

		public Box GetBoxTotal()
		{
			if (this.childrenVisual == null)
				return null;

			Box boxTotal = null;
			for(int i=0;i<this.childrenVisual.Count;i++)
			{
				Visual visual = (Visual)this.childrenVisual[i];
				if (visual.Name == "BoxTotal")
				{
					boxTotal = (Box)visual;
					break;
				}
			}
			return boxTotal;
		}


		
		public Item GetNextSibling()
		{

			ElementItem myParent = this.parent;
			if (myParent == null)
				return null;

			int nIndex = myParent.children.IndexOf (this);
			if (nIndex == -1)
				return null;

			if (myParent.children.Count <= nIndex +1)
				return null;

			return myParent.children [nIndex + 1];
		}

		#region 用xpath选节点

		public virtual XPathNavigator CreateNavigator()
		{
			XmlEditorNavigator nav = new XmlEditorNavigator(this);
			return nav;
		}

		// strXpath	可以是相对路径
		public ItemList SelectItems(string strXpath)
		{
			ItemList items = new ItemList();

			XPathNavigator nav = this.CreateNavigator();
		
			XPathNodeIterator ni = nav.Select(strXpath);
			while(ni.MoveNext())
			{
				Item item = ((XmlEditorNavigator)ni.Current).Item;
				items.Add(item);
			}
			return items;
		}


		public ItemList SelectItems(string strXpath,
			XmlNamespaceManager mngr)
		{
			ItemList items = new ItemList();

			XPathNavigator nav = this.CreateNavigator();
			XPathExpression expr = nav.Compile(strXpath);
			expr.SetContext(mngr);


			XPathNodeIterator ni = nav.Select(expr);
			while(ni.MoveNext())
			{
				Item item = ((XmlEditorNavigator)ni.Current).Item;
				items.Add(item);
			}
			return items;
		}

		public Item SelectSingleItem(string strXpath)
		{
			XPathNavigator nav = this.CreateNavigator();
			XPathNodeIterator ni = nav.Select(strXpath);
			ni.MoveNext();
			return ((XmlEditorNavigator)ni.Current).Item;
		}

		public Item SelectSingleItem(string strXpath,
			XmlNamespaceManager mngr)
		{
			ItemList items = this.SelectItems(strXpath,
				mngr);
			if (items.Count == 0)
				return null;

			return items[0];
		}

		#endregion

		#endregion

		#region 初始化Item层次


		// 用node初始化本对象和下级
		// parameters:
		//		node	XmlNode节点
		//		allocator	对象创建器。用来构造下级元素对象
		//		style	状态,展开 收缩等 仅对ElementItem有意义
		// return:
		//		-1  出错
		//		-2  中途cancel;
		//		0   successed
		public virtual int Initial(XmlNode node, 
			ItemAllocator allocator,
            object style,
            bool bConnect)
		{
			return 0;
		}


		#endregion

		#region 初始化Visual层次

		// 此函数等待派生类重写
		public virtual void InitialVisualSpecial(Box boxtotal)
		{

		}

		// 本函数对于派生类来说一般不要重载。一般重载InitialVisualSpecial()即可。
		// 因为本函数代码前后部分基本是共用的，只有中段采用调用InitialVisualSpecial()的办法。
		// 不过如果重载了本函数，并且不想在其中调用InitialVisualSpecial()，则需要自行实现全部功能
		public virtual void  InitialVisual()
		{
			if (this.childrenVisual != null)
				this.childrenVisual.Clear();

			// 加Label
			Label label = new Label();
			label.container = this;
			label.Text = this.Name;
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
			if (boxTotal.childrenVisual != null 
				&& boxTotal.childrenVisual .Count == 1)
			{
				boxTotal.LayoutStyle = LayoutStyle.Horizontal ;
			}

			/*  暂时不加comment区域
			Comment comment = new Comment ();
			comment.container = this;
			this.AddChildVisual  (comment);
			*/

		}


	
		#endregion

		#region 重写虚函数
		
		// 为什么要重写虚属性,item是从visual派生,而visual派生类并没有具体实现这个函数
		public override void Paint(PaintEventArgs pe,
			int nBaseX,
			int nBaseY,
			PaintMember paintMember)
		{
			// 1.计算出本对象的实际区域
			Rectangle rectPaintThis = new Rectangle (0,0,0,0);
			rectPaintThis = new Rectangle (nBaseX + this.Rect.X,
				nBaseY + this.Rect.Y,
				this.Rect.Width ,
				this.Rect.Height);
			if (rectPaintThis.IntersectsWith(pe.ClipRectangle )== false)
				return;

			Brush brush = null;

			// 2.画背景色
			//	如有缺省透明色,当前颜色与透明色相同则不画了
			Object colorDefault = null;
			XmlEditor editor = this.m_document;
			if (editor != null && editor.VisualCfg != null)
				colorDefault = editor.VisualCfg.transparenceColor;
			if (colorDefault != null)  //缺省颜色
			{
				if (((Color)colorDefault).Equals(BackColor) == true)
					goto SkipDrawBack;
			}

			brush = new SolidBrush(this.BackColor);
			pe.Graphics.FillRectangle (brush,rectPaintThis);

			// 跳过画背景色			
			SkipDrawBack:



				// 3.画线条
				// 当单线条时，自己不画,靠儿子来画
				if (editor != null
					&& editor.VisualCfg == null)
				{
				}
				else
				{
					// 配置文件定了的时候
					this.DrawLines(rectPaintThis,
						this.TopBorderHeight,
						this.BottomBorderHeight,
						this.LeftBorderWidth,
						this.RightBorderWidth,
						this.BorderColor);
				}

			// 4.画儿子
			if (childrenVisual == null)
				return;

			for(int i=0;i<this.childrenVisual.Count;i++)
			{
				Visual visual = (Visual)(this.childrenVisual[i]);

				Rectangle rectPaintChild =
					new Rectangle(
					nBaseX + this.Rect.X + visual.Rect.X,
					nBaseY + this.Rect.Y + visual.Rect.Y,
					visual.Rect.Width,
					visual.Rect.Height);

				if (rectPaintChild.IntersectsWith(pe.ClipRectangle ) == true)
				{
					visual.Paint(pe,
						nBaseX + this.Rect.X,
						nBaseY + this.Rect.Y,
						paintMember);
				}

				if (i <= 0)
					continue;

				if (editor != null
					&& editor.VisualCfg == null)
				{
					this.DrawLines(rectPaintChild,
						0,
						0,
						visual.LeftBorderWidth,
						0,
						visual.BorderColor);
				}
			}


			/*

						// 5.给当前活动的Item加亮显示
						XmlEditor myEditor = this.m_document;
						if (myEditor != null)
						{
							if (this == myEditor.SelectedItem)
							{
								//测试用线条框
								rectPaint = new Rectangle (nBaseX + this.rect .X,
									nBaseY + this.rect .Y,
									this.rect .Width ,
									this.rect .Height-1);
								Pen pen = new Pen(Color.White,1);
								pe.Graphics .DrawRectangle (pen,rectPaint );
								pen.Dispose ();
							}
						}
			*/			
		}


		#endregion
	} 



	// 元素和属性共有的基类。一般不直接实例化
	public class ElementAttrBase : Item
	{
		public string Prefix = null;
		
		public string LocalName = null;

		internal string m_strTempURI = null;

		// 该元素是不是命名空间节点 
		// 说明,目前我们的命令空间节点与属性节点是放在一点的,所以用一个属性来区别
		public bool IsNamespace = false;  

		public virtual string NamespaceURI 
		{
			get 
			{
				return null;
			}
		}
	}



	// Item集合
	public class ItemList : CollectionBase
	{
		public Item this[int index]
		{
			get 
			{
				return (Item)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}
		public void Add(Item item)
		{
			InnerList.Add(item);
		}

		public  void Insert(int index,Item item)
		{
			InnerList.Insert (index,item);
		}

		public void Remove(Item item)
		{
			InnerList.Remove (item);
		}


		public int IndexOf (Item item)
		{
			return InnerList.IndexOf (item);
		}
	}
}
