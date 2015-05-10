using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;


namespace DigitalPlatform.Xml
{
	public class Visual
	{
		#region 成员变量

        //容器
		public Visual container = null; 
		// 本对象的区域
		public Rectangle Rect = new Rectangle(0,0,0,0);
       
        //做Catch用的对象
        public SizeCatch sizeCatch = new SizeCatch();


		private string m_strName = "";

		//？有一些疑问
		//public bool bExpandable = false;    //是否可以展开

		//测试用,是否是当前拖动的节点
		public bool bDrag = false;

        // 布局
		protected LayoutStyle layoutStyle = LayoutStyle.Horizontal;

		public const int TOPBORDERHEIGHT = 1;       // 上方线条的高度
		public const int BOTTOMBORDERHEIGHT = 1;    // 下方线条的高度
		public const int LEFTBORDERWIDTH = 1;       // 左方线条的宽度
		public const int RIGHTBORDERWIDTH = 1;      // 右方线条的宽度

		#endregion

        // 布局风格
		public virtual LayoutStyle LayoutStyle
		{
			get
			{
				return this.layoutStyle;
			}
			set
			{
				this.layoutStyle = value;
			}
		}

        // 名称
		public string Name
		{
			get
			{
				return this.m_strName;
			}
			set
			{
				this.m_strName = value;
			}
		}

        // 左方外围的宽度
		public int LeftResWidth
		{
			get
			{
				return this.LeftBlank
					+ this.LeftBorderWidth;
			}
		}
        
        // 右方外围的宽度
		public int RightResWidth
		{
			get
			{
				return this.RightBlank
					+ this.RightBorderWidth;
			}
		}

        // 上方外围的高度
		public int TopResHeight
		{
			get
			{
				return this.TopBlank
					+ this.TopBorderHeight;
			}
		}

        // 下方外围的高度
		public int BottomResHeight
		{
			get
			{
				return this.BottomBlank
					+ this.BottomBorderHeight;
			}
		}
        // 总共外围的宽度
		public int TotalRestWidth
		{
			get
			{
				return this.LeftBlank 
					+ this.RightBlank
					+ this.LeftBorderWidth
					+ this.RightBorderWidth;
			}
		}

        // 总共外围的高度
		public int TotalRestHeight
		{
			get			
			{
				return this.TopBlank
					+ this.BottomBlank
					+ this.TopBorderHeight
					+ this.BottomBorderHeight;
			}
		}



		#region 关于坐标的属性和函数

		// 得到相对于根的rectangle
		public Rectangle RectAbs
		{
			get
			{
				int nRootX = 0;
				int nRootY = 0;
				getAbs(out nRootX,
					out nRootY);

				return new Rectangle(nRootX,
					nRootY,
					this.Rect.Width,
					this.Rect.Height);
			}
		}

		// 得到相对根的x
		public int getAbsX()
		{
			int nX,nY;
			getAbs(out nX,out nY);
			return nX;
		}

		// 得到相对于根的x,y
		public void getAbs(out int nX,
			out int nY)
		{
			nX = 0;
			nY = 0;
			Visual visual = this;
			while(true)
			{
				if (visual == null)
					break;
				nX += visual.Rect.X ;
				nY += visual.Rect.Y ; 
				visual = visual.container;
			}
		}

		#endregion

		#region 一些公共属性

		public bool IsWriteInfo
		{
			get
			{
				return false;
			}
		}

		public bool Catch
		{
			get
			{
				Item item = this.GetItem ();
				return item.m_document.bCatch ;
			}
		}
		#endregion

		#region 关于visual的层次的函数

		// 得到visual的层号
		public int GetVisualLevel()
		{
			int nLevel = 0;
			Visual visual = this;
			while(true) 
			{
				if (visual == null)
					break;
				visual = visual.container ;
				nLevel ++;
			}
			return nLevel;
		}

		// 根据visual层号得到字符串
		public string GetStringFormLevel(int nLevel)
		{
			string strResult = "";
			for(int i=0;i<nLevel ;i++)
			{
				strResult += "  ";
			}
			return strResult;
		}

		
		// 得到路径
		public string GetPath()
		{
			string strPath = "";
			Visual visual = this;
		
			while(true)
			{
				if (visual == null)
					break;

				if (strPath != "")
					strPath += "\\";

				strPath += this.GetType ().Name  ;

				Visual parent = visual.container ;
				if (parent != null)
				{
					strPath += "["+((Box)parent).childrenVisual .IndexOf (visual)+"]";
				}
				visual = parent;
			}

			return strPath;
		}

		#endregion

		#region  关于样式配置信息的属性

        // 左方空白
		public int LeftBlank
		{
			get
			{
				return GetDigitalValue(ValueStyle.LeftBlank );
			}
		}
		
        // 右方空白
        public int RightBlank
		{
			get
			{
				return GetDigitalValue(ValueStyle.RightBlank );
			}
		}
		
        // 上方空白
        public int TopBlank
		{
			get
			{
				return GetDigitalValue(ValueStyle.TopBlank);
			}
		}

        // 下方空白
		public int BottomBlank
		{
			get
			{
				return GetDigitalValue(ValueStyle.BottomBlank);
			}
		}


		// 上方线条的高度
		public int TopBorderHeight
		{
			get
			{
				int nTopBorderHeight = GetDigitalValue(ValueStyle.TopBorderHeight);
				if(nTopBorderHeight != -1)
					return nTopBorderHeight;

				
				if (this is VirtualRootItem)
					return 0;//Visual.TOPBORDERHEIGHT;

				Visual parent = this.container;
				if (parent != null)
				{
					if (((Box)parent).LayoutStyle == LayoutStyle.Vertical)
					{
						int nIndex = ((Box)parent).childrenVisual.IndexOf(this);
						if (nIndex == 0)
							return 0;
						return Visual.TOPBORDERHEIGHT;
					}
				}
				return 0;

			}
		}

		// 下方线条的高度
		public int BottomBorderHeight
		{
			get
			{
				int nBottomBorderHeight = GetDigitalValue(ValueStyle.BottomBorderHeight);
				if(nBottomBorderHeight != -1)
					return nBottomBorderHeight;

				if (this is VirtualRootItem)
					return Visual.BOTTOMBORDERHEIGHT;
				else
					return 0;
			}
		}

		// 左方线条的宽度
		public int LeftBorderWidth
		{
			get
			{
				int nLeftBorderWidth = GetDigitalValue(ValueStyle.LeftBorderWidth);
				if(nLeftBorderWidth != -1)
					return nLeftBorderWidth;


				if (this is VirtualRootItem)
					return Visual.LEFTBORDERWIDTH;

				Visual parent = this.container;
				if (parent != null)
				{
					if (((Box)parent).LayoutStyle == LayoutStyle.Horizontal)
					{
						int nIndex = ((Box)parent).childrenVisual.IndexOf(this);
						if (nIndex == 0)
							return 0;

						return Visual.LEFTBORDERWIDTH;
					}
				}
				return 0;
			}
		}

		// 右方线条的宽度
		public int RightBorderWidth
		{
			get
			{
				int nRightBorderWidth = GetDigitalValue(ValueStyle.RightBorderWidth);
				if(nRightBorderWidth != -1)
					return nRightBorderWidth;

				if (this is VirtualRootItem)
					return Visual.RIGHTBORDERWIDTH;
				else 
					return 0;
			}
		}

        // 获取数值
		public int GetDigitalValue(ValueStyle valueStyle)
		{
			Item item = this.GetItem ();
			if (item == null)
				return 0;

			ItemRegion region = GetRegionName();
			if (region == ItemRegion.No )
				return 0;
			
			if (valueStyle == ValueStyle.LeftBlank )
				return item.GetLeftBlank(region);

			if (valueStyle == ValueStyle.RightBlank )
				return item.GetRightBlank (region);

			if (valueStyle == ValueStyle.TopBlank )
				return item.GetTopBlank  (region);

			if (valueStyle == ValueStyle.BottomBlank  )
				return item.GetBottomBlank(region);

			if (valueStyle == ValueStyle.TopBorderHeight)
				return item.GetTopBorderHeight(region);

			if (valueStyle == ValueStyle.BottomBorderHeight)
				return item.GetBottomBorderHeight(region);

			if (valueStyle == ValueStyle.LeftBorderWidth)
				return item.GetLeftBorderWidth(region);

			if (valueStyle == ValueStyle.RightBorderWidth)
				return item.GetRightBorderWidth(region);

			return 0;
		}

        // 线条颜色
		public Color BorderColor
		{
			get
			{
				return GetColor(ValueStyle.BorderColor );
			}
		}

        // 背景颜色
		public virtual Color BackColor
		{
			get
			{
				if (this.bDrag == true)
				{
					return Color.Red;
				}
				else
				{
					return GetColor(ValueStyle.BackColor);
				}
			}
		}

        // 获取颜色
		public Color GetColor(ValueStyle valueStyle)
		{
			Item item = this.GetItem ();
			ItemRegion region = GetRegionName();
			
			if (valueStyle == ValueStyle.BackColor  )
				return item.GetBackColor(region);
			if (valueStyle == ValueStyle.BorderColor )
				return item.GetBorderColor (region);

			return Color.Red;  // 代表错误
		}

		// 取角色
		public virtual ItemRegion GetRegionName()  //虚函数，派生类重写
		{
			return ItemRegion.No ;
		}

		#endregion

		#region Visual的一些公共函数

		// 找到本visual从属于Item，可以中间隔了多层visual
		public Item GetItem()
		{
			Item item = null;
			Visual visual = this;
			while(true)
			{
				if (visual == null)
					break;

				if (Visual.IsDerivedFromItem(visual) == true)
				{
					item = (Item)visual;
					break;
				}
				visual = visual.container;
			}
			// Debug.Assert(item != null, "");
			return item;
		}

        // 画线条
        // parameters:
        //      myRect              Rectangle区域
        //      nTopBorderHeight    上方线条的高度
        //      nBottomBorderHeight 下方线条的高度
        //      nLeftBorderWidth    左方线条的宽度
        //      nRightBorderWidth   下方线条的宽度
        //      color               颜色
        // return:
        //      void
		public void DrawLines(Rectangle myRect,
			int nTopBorderHeight,
			int nBottomBorderHeight,
			int nLeftBorderWidth,
			int nRightBorderWidth,
			Color color)
		{
			if (nTopBorderHeight < 0
				|| nBottomBorderHeight < 0
				|| nLeftBorderWidth < 0
				|| nRightBorderWidth < 0)
			{
				return;
			}

			if (nTopBorderHeight > myRect.Height 
				|| nBottomBorderHeight > myRect.Height)
			{
				/*
								Debug.Assert (false,"区域:" + this.GetType ().Name 
									+ " 名称:'" + this.strName + "'\r\n"
									+ "DrawLine传入的水平线条高度" + nBorderHorzHeight + "大于矩形高度" + myRect.Height);
				*/
				return;
			}

			if (nLeftBorderWidth > myRect.Width
				|| nRightBorderWidth > myRect.Width)
			{
				/*
								Debug.Assert (false,"区域:" + this.GetType ().Name 
									+ " 名称:'" + this.strName + "'\r\n"
									+ "DrawLine传入的垂直线条宽度" + nBorderVertWidth + "大于矩形宽度" + myRect.Width );
				*/
				return;
			}

			Pen penLeft = null;  //左边垂直钢笔
			Pen penRight = null;  // 右边的垂直钢笔
			Pen penTop = null;  //上方的水平钢笔
			Pen penBottom = null;  //下方的水平钢笔

			//penVert = new Pen(color, nBorderVertWidth);
			//penHorz = new Pen(color, nBorderHorzHeight);

			penLeft = new Pen(color,nLeftBorderWidth);
			penRight = new Pen(color,nRightBorderWidth);
			penTop = new Pen(color,nTopBorderHeight);
			penBottom = new Pen(color,nBottomBorderHeight);


			//int nHorzDelta = nBorderVertWidth / 2;
			//int nVertDelta = nBorderHorzHeight / 2;

			int nLeftDelta = nLeftBorderWidth / 2;
			int nRightDelta = nRightBorderWidth / 2;
			int nTopDelta = nTopBorderHeight / 2;
			int nBottomDelta = nBottomBorderHeight / 2 ;

			//int nHorzMode = nBorderVertWidth % 2;
			//int nVertMode = nBorderHorzHeight % 2;

			int nLeftMode = nLeftBorderWidth % 2;
			int nRightMode = nRightBorderWidth % 2;
			int nTopMode = nTopBorderHeight % 2;
			int nBottomMode = nBottomBorderHeight % 2;

			Rectangle rectMiddle = new Rectangle(
				myRect.X + nLeftDelta,
				myRect.Y + nTopDelta,
				myRect.Width  - nLeftDelta - nRightDelta,
				myRect.Height  - nTopDelta - nBottomDelta);

			Item item = this.GetItem ();
			if (item == null)
			{
				Debug.Assert (false,"DrawLine找到的item 为null");
				return;
			}

			XmlEditor editor = item.m_document;
			if (editor == null)
			{
				Debug.Assert (false,"DrawLine找到的xmleditor 为null");
				return;
			}
			
			Graphics g = Graphics.FromHwnd(editor.Handle);
		
			//上方
			if (nTopBorderHeight > 0)
			{
				g.DrawLine (penTop,
					rectMiddle.Left ,rectMiddle.Top ,
					rectMiddle.Right ,rectMiddle.Top );
			}

			//下方
			if (nBottomBorderHeight > 0)
			{
				g.DrawLine (penBottom,
					rectMiddle.Left,rectMiddle.Bottom ,
					rectMiddle.Right,rectMiddle.Bottom );
			}

			int nLeftTemp = nLeftDelta + nLeftMode;
			if (nLeftBorderWidth == 1)
			{
				if (nLeftMode == 0)
					nLeftTemp = nLeftDelta -1;
				else
					nLeftTemp = nLeftDelta;
			}
			//左方
			if (nLeftBorderWidth > 0)
			{
				g.DrawLine (penLeft,
					rectMiddle.Left ,rectMiddle.Top/* - nLeftDelta*/,
					rectMiddle.Left ,rectMiddle.Bottom/* + nLeftTemp*/);
			}

			int nRightTemp = nRightDelta + nRightMode;
			if (nRightBorderWidth == 1)
			{
				if (nRightMode == 0)
					nRightTemp = nRightDelta -1;
				else
					nRightTemp = nRightDelta;
			}
			//右方
			if (nRightBorderWidth > 0)
			{
				g.DrawLine (penRight,
					rectMiddle.Right ,rectMiddle.Top - nRightDelta,
					rectMiddle.Right ,rectMiddle.Bottom + nRightTemp);
			}
			if (penLeft != null)
				penLeft.Dispose ();
			if (penRight != null)
				penRight.Dispose ();
			if (penTop != null)
				penTop.Dispose ();
			if (penBottom != null)
				penBottom.Dispose ();
		}
	
		#endregion

		#region 静态函数

        // 向上递归布局
		public static void UpLayout(Visual visual,
			int nTimeStamp)
		{
			Box myContainer = (Box)(visual.container);
			if (myContainer != null)
			{
				int nMyRetWidth,nMyRetHeight;
				int nWidth = 0;
				int nHeight = 0;
				if (myContainer.LayoutStyle == LayoutStyle.Horizontal )
				{
					nWidth = 0;
					nHeight = 0;
					foreach(Visual child in myContainer.childrenVisual )
					{
						nWidth += child.Rect.Width ;

						child.Layout (child.Rect.X ,
							child.Rect.Y,
							child.Rect.Width ,
							0,
							nTimeStamp,
							out nMyRetWidth,
							out nMyRetHeight,
							LayoutMember.Layout );

						if (child.Rect.Height > nHeight)
							nHeight = child.Rect.Height ;
					}
					foreach(Visual child in myContainer.childrenVisual )
					{
						child.Rect.Height = nHeight;
					}
				}
				else if (myContainer.LayoutStyle == LayoutStyle.Vertical )
				{
					nWidth = visual.Rect.Width ;
					nHeight = visual.Rect.Height ;
					//先把兄弟计算一下
					foreach(Visual child in myContainer.childrenVisual )
					{
						if (child.Equals (visual) == true)
							continue;

						child.Layout (child.Rect.X ,
							child.Rect.Y,
							visual.Rect.Width ,
							0,
							nTimeStamp,
							out nMyRetWidth,
							out nMyRetHeight,
							LayoutMember.Layout );

						if (child.Rect.Width > nWidth)
							nWidth = child.Rect.Width ;
						nHeight += child.Rect.Height ;
					}
				}

				myContainer.Rect.Width = nWidth + myContainer.TotalRestWidth;
				myContainer.Rect.Height = nHeight + myContainer.TotalRestHeight;
				//设兄弟坐标
				int nXDelta = myContainer.LeftResWidth;
				int nYDelta = myContainer.TopResHeight;
				if (myContainer.LayoutStyle == LayoutStyle.Horizontal )
				{
					foreach(Visual childVisual in myContainer.childrenVisual )
					{
						childVisual.Rect.X = nXDelta;
						childVisual.Rect.Y = nYDelta;
						nXDelta += childVisual.Rect.Width ;
					}
				}
				else if (myContainer.LayoutStyle == LayoutStyle.Vertical  )
				{
					foreach(Visual childVisual in myContainer.childrenVisual )
					{
						childVisual.Rect.X = nXDelta;
						childVisual.Rect.Y = nYDelta;
						nYDelta += childVisual.Rect.Height;
					}
				}
				myContainer.Layout(myContainer.Rect.X,
					myContainer.Rect.Y,
					myContainer.Rect.Width,
					myContainer.Rect.Height,
					nTimeStamp,
					out nMyRetWidth,
					out nMyRetHeight,
					LayoutMember.Up );
			}
		}


        // 改变兄弟布局
        // parameters:
        //      startVisual 起始visual
        //      nWidth      宽度 -1无效
        //      nHeight     高度 -1无效
        // return:
        //      void
		public static void ChangeSibling(Visual startVisual,
			int nWidth,
			int nHeight)
		{
			Box myContainer = (Box)(startVisual.container );
			if (myContainer == null)
				return;

			int nRetWidth,nRetHeight;

			if (nHeight != -1)
			{
				foreach(Visual child in myContainer.childrenVisual )
				{
					if (child.Equals (startVisual) == true)
						continue;
					child.Layout (child.Rect.X,
						child.Rect.Y,
						child.Rect.Width,
						nHeight,
						-1,
						out nRetWidth,
						out nRetHeight,
						LayoutMember.EnLargeHeight );
				}
			}

			if (nWidth != -1)
			{
				foreach(Visual child in myContainer.childrenVisual )
				{
					if (child.Equals (startVisual) == true)
						continue;
					child.Layout (child.Rect.X,
						child.Rect.Y,
						nWidth,
						child.Rect.Height,
						-1,
						out nRetWidth,
						out nRetHeight,
						LayoutMember.EnlargeWidth );
				}
			}

		}


		// 判断visual是否从Item类派生
        // parameters:
        //      visual  Visual对象
        // return:
        //      true    visual是从Item类派生
        //      false   不是
		public static bool IsDerivedFromItem(Visual visual)
		{
			ArrayList aDeriveTypes = null;
			Type t = visual.GetType ();
			while(true)
			{
				if (t.Name == "Object")
					break;
				if (aDeriveTypes == null)
					aDeriveTypes = new ArrayList ();
				aDeriveTypes.Add (t );
				t = t.BaseType ;
			}

			if (aDeriveTypes != null)
			{
				for(int i= 0; i < aDeriveTypes.Count ;i++)
				{
					string strName1 = ((Type)aDeriveTypes[i]).FullName;
					string strName2 = typeof(Item).FullName;
					if (strName1 == strName2)
						return true;
				}
			}
			return false;
		}


		// 判断visual是否从Box类派生
		public static bool IsDerivedFromBox(Visual visual)
		{
			ArrayList aDeriveTypes = null;
			Type t = visual.GetType ();
			while(true)
			{
				if (t.Name == "Object")
					break;
				if (aDeriveTypes == null)
					aDeriveTypes = new ArrayList ();
				aDeriveTypes.Add (t);
				t = t.BaseType ;
			}

			if (aDeriveTypes != null)
			{
				for(int i= 0; i < aDeriveTypes.Count ;i++)
				{
					string strName1 = ((Type)aDeriveTypes[i]).FullName;
					string strName2 = typeof(Box).FullName;
					if (strName1 == strName2)
						return true;
				}
			}
			return false;
		}
		
        // 判断visual是否从TextVisual类派生
		public static bool IsDerivedFromTextVisual(Visual visual)
		{
			ArrayList aDeriveTypes = null;
			Type t = visual.GetType ();
			while(true)
			{
				if (t.Name == "Object")
					break;
				if (aDeriveTypes == null)
					aDeriveTypes = new ArrayList ();
				aDeriveTypes.Add (t);
				t = t.BaseType ;
			}

			if (aDeriveTypes != null)
			{
				for(int i= 0; i < aDeriveTypes.Count ;i++)
				{
					string strName1 = ((Type)aDeriveTypes[i]).FullName;
					string strName2 = typeof(TextVisual).FullName;
					if (strName1 == strName2)
						return true;
				}
			}
			return false;
		}
		#endregion

		#region 定义了一些虚函数

		// 判断当前对象是否是ExpandHandle
		public virtual bool IsExpandHandle()
		{
			return false;
		}

		// 当layoutMember为CalcuWidth不给rectange赋值，实际布局才赋值
        // parameters:
        //      x               x坐标
        //      y               y坐标
		//      nInitialWidth   初始宽度
		//      nInitialHeight  初始高度
        //      nRetWidth       返回需要的宽度
		//      nRectHeight     返回需要的高度
		//      layoutMember    功能参数
		public virtual void Layout(int x,
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


			///////////////////////////////////////////
			//1.首先判断是否为Enlarge参数///////////////////
			///////////////////////////////////////////
			bool bEnlargeWidth = false;
			if ((layoutMember & LayoutMember.EnlargeWidth  ) == LayoutMember.EnlargeWidth  )
				bEnlargeWidth = true;

			bool bEnlargeHeight = false;
			if ((layoutMember & LayoutMember.EnLargeHeight ) == LayoutMember.EnLargeHeight )
				bEnlargeHeight = true;

			if (bEnlargeWidth == true
				|| bEnlargeHeight == true)
			{
				//父亲和兄弟都影响了
				if ((layoutMember & LayoutMember.Up ) == LayoutMember.Up )
				{
					if (bEnlargeHeight == true)
					{
						this.Rect.Height = nInitialHeight;

						Box myContainer = (Box)(this.container );
						if (myContainer == null)
							return;
	
						if (myContainer.LayoutStyle == LayoutStyle.Horizontal )
						{
							//影响兄弟
							foreach(Visual child in myContainer.childrenVisual )
							{
								if (child.Equals (this) == true)
									continue;

								child.Layout(
									child.Rect.X,
									child.Rect.Y,
									child.Rect.Width,
									this.Rect.Height,
									nTimeStamp,
									out nRetWidth,
									out nRetHeight,
									LayoutMember.EnLargeHeight );
							}

							int nMyHeight = this.Rect.Height;

							foreach(Visual child in myContainer.childrenVisual )
							{
								if (child.Rect.Height > nMyHeight)
									nMyHeight = child.Rect.Height ;
							}
							nMyHeight += myContainer.TotalRestHeight;

							myContainer.Layout (
								myContainer.Rect.X,
								myContainer.Rect.Y,
								myContainer.Rect.Width,
								nMyHeight,
								nTimeStamp,
								out nRetWidth,
								out nRetHeight,
								layoutMember);

						}

						if (myContainer.LayoutStyle == LayoutStyle.Vertical )
						{
							int nTempTotalHeight = 0;
							foreach(Visual childVisual in myContainer.childrenVisual )
							{
								nTempTotalHeight += childVisual.Rect.Height;
							}
							nTempTotalHeight += myContainer.TotalRestHeight;

							
							myContainer.Layout (
								myContainer.Rect.X,
								myContainer.Rect.Y,
								myContainer.Rect.Width,
								nTempTotalHeight,
								nTimeStamp,
								out nRetWidth,
								out nRetHeight,
								layoutMember);

							//设兄弟坐标
							int nXDelta = myContainer.LeftResWidth;
							int nYDelta = myContainer.TopResHeight;
							foreach(Visual childVisual in myContainer.childrenVisual )
							{
								childVisual.Rect.X = nXDelta;
								childVisual.Rect.Y = nYDelta;
								nYDelta += childVisual.Rect.Height;
							}
						}
						return;
					}
				}
				if (bEnlargeHeight == true)
					this.Rect.Height  = nInitialHeight;

				if (bEnlargeHeight == true)
					this.Rect.Width = nInitialWidth;

				return;	
			}


			//2.输入信息///////////////////////////////////////
			Item item = this.GetItem();
			Debug.Assert(item != null, "");

			item.m_document.nTime ++;
			string strTempInfo = "";
			
			int nTempLevel = this.GetVisualLevel ();
			string strLevelString = this.GetStringFormLevel (nTempLevel);
			if (this.IsWriteInfo == true)
			{
				strTempInfo = "\r\n"
					+ strLevelString + "******************************\r\n"
					+ strLevelString + "这是第" + nTempLevel + "层的" + this.GetType ().Name + "调layout开始\r\n" 
					+ strLevelString + "参数为:\r\n"
					+ strLevelString + "x=" + x + "\r\n"
					+ strLevelString + "y=" + y + "\r\n"
					+ strLevelString + "nInitialWidth=" + nInitialWidth + "\r\n"
					+ strLevelString + "nInitialHeight=" + nInitialHeight + "\r\n"
					+ strLevelString + "nTimeStamp=" + nTimeStamp + "\r\n"
					+ strLevelString + "layoutMember=" + layoutMember.ToString () + "\r\n"
					+ strLevelString + "LayoutStyle=无\r\n"
					+ strLevelString + "使总次数变为" + item.m_document.nTime  + "\r\n";
				StreamUtil.WriteText ("I:\\debug.txt",strTempInfo);
			}


			//3.Catch///////////////////////////////////
			if (Catch == true)
			{
				//当输入参数相同时，直接返回catch内容
				if (sizeCatch.nInitialWidth == nInitialWidth
					&& sizeCatch.nInitialHeight == nInitialHeight
					&& (sizeCatch.layoutMember == layoutMember))
				{
					if (this.IsWriteInfo == true)
					{
						strTempInfo = "\r\n"
							+ strLevelString + "------------------"
							+ strLevelString + "与缓存时相同\r\n"
							+ strLevelString + "传入的值: initialWidth:"+nInitialWidth + " initialHeight:" + nInitialHeight + " timeStamp: " + nTimeStamp + " layoutMember:" + layoutMember.ToString () + "\r\n"
							+ strLevelString + "缓存的值: initialWidth:"+sizeCatch.nInitialWidth + " initialHeight:" + sizeCatch.nInitialHeight + " timeStamp: " + sizeCatch.nTimeStamp + " layoutMember:" + sizeCatch.layoutMember.ToString () + "\r\n";
					}

					if ((layoutMember & LayoutMember.Layout) != LayoutMember.Layout )
					{
						if (this.IsWriteInfo == true)
						{
							strTempInfo += strLevelString + "不是实做，直接返回缓冲区值\r\n";
						}

						nRetWidth = sizeCatch.nRetWidth  ;
						nRetHeight = sizeCatch.nRetHeight  ;

						if (this.IsWriteInfo == true)
						{
							strTempInfo +=   strLevelString + "----------结束------\r\n";
							StreamUtil.WriteText ("I:\\debug.txt",strTempInfo);
						}

						goto END1;
					}
					else
					{
						if (this.IsWriteInfo == true)
						{
							strTempInfo += strLevelString + "包含实做，向下继续\r\n";
						}
					}
					if (this.IsWriteInfo == true)
					{
						strTempInfo +=   strLevelString + "----------结束------\r\n";
						StreamUtil.WriteText ("I:\\debug.txt",strTempInfo);
					}
				}
				else
				{
					if (this.IsWriteInfo == true)
					{
						strTempInfo = "\r\n"
							+ strLevelString + "------------------"
							+ strLevelString + "与缓存时不同\r\n"
							+ strLevelString + "传入的值: initialWidth:"+nInitialWidth + " initialHeight:" + nInitialHeight + " timeStamp: " + nTimeStamp + " layoutMember:" + layoutMember.ToString () + "\r\n"
							+ strLevelString + "缓存的值: initialWidth:"+sizeCatch.nInitialWidth + " initialHeight:" + sizeCatch.nInitialHeight + " timeStamp: " + sizeCatch.nTimeStamp + " layoutMember:" + sizeCatch.layoutMember.ToString () + "\r\n";

						strTempInfo +=   strLevelString + "----------结束------\r\n";
						StreamUtil.WriteText ("I:\\debug.txt",strTempInfo);
					}
				}
			}


			//4.下面进行各项测量或实算//////////////////////////

			nRetWidth = nInitialWidth;
			nRetHeight = nInitialHeight;

			//得到宽度
			int nTempWidth = GetWidth();
/*
			if (nRetWidth < 0) //(nTempWidth > nRetWidth)
				nRetWidth = 0;
				//nRetWidth = nTempWidth;
*/
			if (nRetWidth < nTempWidth)
				nRetWidth = nTempWidth;

			//1)只测宽度
			if (layoutMember == LayoutMember.CalcuWidth)   //测宽度
				goto END1;

			//得到高度
			int nTempHeight = GetHeight(nRetWidth
				- this.TotalRestWidth);

			if (nTempHeight > nRetHeight )
				nRetHeight = nTempHeight;
			if (nRetHeight < 0)
				nRetHeight = 0;

			//2)测高度(两种情况，只测高度 或 即测高度又测宽度)
			if ((layoutMember & LayoutMember.CalcuHeight ) == LayoutMember.CalcuHeight )
				goto END1;

			//3)实算
			if ((layoutMember & LayoutMember.Layout) == LayoutMember.Layout )  //真正布局
			{
				this.Rect = new Rectangle (x,
					y,
					nRetWidth,
					nRetHeight);

				//把宽度记到数组里
				item.SetValue (this.GetType().Name,
					nRetWidth);

				//goto END1;
			}

			if ((layoutMember & LayoutMember.Up  ) == LayoutMember.Up )
			{
				Visual.UpLayout(this,nTimeStamp);
			}


			END1:

				//***做得catch***
				sizeCatch.SetValues (nInitialWidth,
					nInitialHeight,
					nRetWidth,
					nRetHeight,
					nTimeStamp,
					layoutMember);

			if (this.IsWriteInfo == true)
			{
				strTempInfo = "";
				strTempInfo = "\r\n"
					+ strLevelString + "这是第" + nTempLevel + "层的" + this.GetType ().Name + "调layout结束\r\n" 
					+ strLevelString + "返回值为: \r\n"
					+ strLevelString + "x=" + x + "\r\n"
					+ strLevelString + "y=" + y + "\r\n"
					+ strLevelString + "nRetWidth=" + nRetWidth + "\r\n"
					+ strLevelString + "nRetHeight=" + nRetHeight + "\r\n"
					+ strLevelString + "Rect.X=" + this.Rect.X + "\r\n"
					+ strLevelString + "Rect.Y=" + this.Rect.Y + "\r\n"
					+ strLevelString + "Rect.Width=" + this.Rect.Width + "\r\n"
					+ strLevelString + "Rect.Height=" + this.Rect.Height + "\r\n"
					+ strLevelString + "****************************\r\n\r\n" ;

				StreamUtil.WriteText ("I:\\debug.txt",strTempInfo);
			}
		}

        // 宽度
		public virtual int GetWidth()
		{
			return 0;
		}

        // 高度
		public virtual int GetHeight(int nWidth)
		{
			return 0;
		}


		// 根据传入的相对坐标，得到击中的Visual对象
        // parameters:
		//      p           传入的相对坐标
		//      retVisual   out参数，返回击中的visual
        // return:
        //      -1  坐标不在本区域
        //      0   文字区
        //      1   空白
        //      2   缝隙上
		public virtual int HitTest(Point p,
			out Visual retVisual)
		{
			retVisual = null;
			return -1;
		}


		// 绘图
        // parameters:
        //      pe          PaintEventArgs对象
		//      nBaseX      x基坐标
		//      nBaseY      y基坐标 
		//      paintMember 绘制成员:content,border,both
        // return:
        //      void
		public virtual void Paint(PaintEventArgs pe,
			int nBaseX,
			int nBaseY,
			PaintMember paintMember)
		{
		}

		// 用于输出rect信息
		public virtual void WriteRect(string strName)
		{
		}

		
		#endregion
	}





	//用来做缓冲的类
	public class SizeCatch
	{
		public object data = null;

		public int nInitialWidth = -1;
		public int nInitialHeight = -1;

		public int nRetWidth = -1;
		public int nRetHeight = -1;

		public int nTimeStamp = -1;

		public LayoutMember layoutMember = LayoutMember.None ;

		public void SetValues(int initialWidth,
			int initialHeight,
			int retWidth,
			int retHeight,
			int tempStamp,
			LayoutMember mylayoutMember)
		{
			nInitialWidth = initialWidth;
			nInitialHeight = initialHeight;

			nRetWidth = retWidth;
			nRetHeight = retHeight;

			nTimeStamp = tempStamp;
			this.layoutMember = mylayoutMember;
		}
	}

	//用来临时visual宽度的类
	public class PartInfo
	{
		public string strName;
		public int  nWidth;
		public int nHeight;
	}
}
