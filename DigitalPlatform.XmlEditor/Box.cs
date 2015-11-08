using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
	//******************************************************
	// BoxVisual 类
	//******************************************************
	public class Box : Visual
	{

		public   ArrayList childrenVisual = null;   //包含的visual,并不是数据的层次


		// 加一个子visual
		public void AddChildVisual(Visual visual)
		{
			if (childrenVisual == null)
				childrenVisual = new ArrayList ();
			childrenVisual.Add (visual);
		}


		// 清除所有子visual
		public void ClearChildVisual()
		{
			if (childrenVisual != null)
				childrenVisual.Clear ();
		}


	

		public override ItemRegion GetRegionName()
		{
			if (this.Name == "BoxTotal")
				return ItemRegion.BoxTotal ;
			else if (this.Name == "BoxAttributes")
				return ItemRegion.BoxAttributes ;
			else if (this.Name == "BoxContent")
				return ItemRegion.BoxContent ;
			return ItemRegion.No ;
		}	

		
		public override int HitTest(Point p,
			out Visual retVisual)
		{
			retVisual = null;
			
			//有儿子时,先看儿子
			if (this.childrenVisual != null)
			{
				Point p1 = new Point (p.X - this.Rect.X,
					p.Y - this.Rect.Y);

				//先认弱者,这里是没有必要的，因为线条归各自的区域管，
				Visual visual = null;
				int nCount = this.childrenVisual.Count ;
				for(int i = nCount -1;i>=0;i--)
				{
					visual = (Visual)childrenVisual[i];

					if (p1.X >= visual.Rect.X 
						&& p1.X < (visual.Rect.X + visual.Rect.Width)
						&& p1.Y >= visual.Rect.Y 
						&& p1.Y < (visual.Rect.Y + visual.Rect.Height ))
					{
						// int nRet = -1;
						return visual.HitTest(p1,out retVisual);
						
					}
				}
			}

			//当没儿子，或者儿子一个都不符合，那么就看自己了


			retVisual = null;
			int nResizeAreaWidth = 4;   //缝隙的宽度
			//在缝上
			if ( p.X >= this.Rect.X + this.Rect.Width - (nResizeAreaWidth/2)
				&& p.X < this.Rect.X + this.Rect.Width + (nResizeAreaWidth/2)) 
			{
				retVisual = this;
				return  2;
			}

			//不在区域
			if (p.X < this.Rect.X 
				|| p.Y < this.Rect.Y )
			{
				return -1;
			}
			if (p.X > this.Rect.X + this.Rect.Width 
				|| p.Y > this.Rect.Y + this.Rect.Height )
			{
				return -1;
			}

			//在线条和空白
			//1. 左线条空白处
			if (p.X > this.Rect.X 
				&& p.X < this.Rect.X + this.LeftResWidth
				&& p.Y >= this.Rect.Y
				&& p.Y < this.Rect.Y + this.Rect.Height)
			{
				retVisual = this;
				return -1;
			}

			// 2.右线条空白处
			if (p.X > this.Rect.X + this.Rect.Width - this.RightResWidth
				&& p.X < this.Rect.X + this.Rect.Width
				&& p.Y >= this.Rect.Y
				&& p.Y < this.Rect.Y + this.Rect.Height)
			{
				retVisual = this;
				return -1;
			}
			// 3.上线条空白处
			if (p.Y >= this.Rect.Y
				&& p.Y < this.Rect.Y + this.TopResHeight
				&& p.X >= this.Rect.X
				&& p.X < this.Rect.X + this.Rect.Width)
			{
				retVisual = this;
				return -1;
			}
			// 4.下线条空白处
			if (p.Y >= this.Rect.Y + this.Rect.Height - this.BottomResHeight
				&& p.Y < this.Rect.Y + this.Rect.Height
				&& p.X >= this.Rect.X
				&& p.X < this.Rect.X + this.Rect.Width)
			{
				retVisual = this;
				return -1;
			}

			
			//在文字区
			if (p.X >= this.Rect.X + this.LeftResWidth 
				&& p.Y >= this.Rect.Y + this.TopResHeight 
				&& p.X < this.Rect.X + this.Rect.Width - this.RightResWidth
				&& p.Y < this.Rect.Y + this.Rect.Height - this.BottomResHeight)
			{
				retVisual = this;
				return 0;
			}
			return -1;
		}



		// 布局
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
			if (bEnlargeWidth == true	|| bEnlargeHeight == true)
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
	
						//横排
						if (myContainer.LayoutStyle == LayoutStyle.Horizontal )
						{
							//影响兄弟
							foreach(Visual child in myContainer.childrenVisual )
							{
								if (child.Equals (this) == true)
									continue;

								child.Layout (
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
								myContainer.Rect.Width ,
								nMyHeight ,
								nTimeStamp,
								out nRetWidth,
								out nRetHeight,
								layoutMember);
						}
						//竖排
						if (myContainer.LayoutStyle == LayoutStyle.Vertical )
						{
							int nTempTotalHeight = 0;
							foreach(Visual childVisual in myContainer.childrenVisual )
							{
								nTempTotalHeight += childVisual.Rect.Height ;
							}
							nTempTotalHeight += myContainer.TotalRestHeight;;

							
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
			
				if (LayoutStyle == LayoutStyle.Horizontal )
				{
					if (bEnlargeHeight == true)
					{
						this.Rect.Height  = nInitialHeight;
						foreach(Visual child in this.childrenVisual )
						{
							child.Layout (0,
								0,
								0,
								nInitialHeight,
								nTimeStamp,
								out nRetWidth,
								out nRetHeight,
								layoutMember);
						}
					}
				}
				else if (LayoutStyle == LayoutStyle.Vertical )
				{
					if (bEnlargeWidth== true)
					{
						this.Rect.Width = nInitialWidth;
						foreach(Visual child in this.childrenVisual )
						{
							child.Layout (0,
								0,
								nInitialWidth,
								0,
								nTimeStamp,
								out nRetWidth,
								out nRetHeight,
								layoutMember);
						}
					}
				}
				return;	
			}

			Item item = GetItem();
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
					+ strLevelString + "LayoutStyle=" + this.LayoutStyle.ToString () + "\r\n"
					+ strLevelString + "使总次数变为" + item.m_document.nTime  + "\r\n";
				StreamUtil.WriteText ("I:\\debug.txt",strTempInfo);
			}

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
						strTempInfo += strLevelString + "----------结束------\r\n";
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




			//计算每一小块用得参数
			int nPartWidth = 0;  
			int nPartHeight = 0;
			int nRetPartWidth = 0;;  //返回
			int nRetPartHeight = 0;

			int nTotalWidth = 0; //横排总宽度
			int nMaxHeight = 0; //横排时的最大高度,当变大，要重新算

			int nMaxWidth = 0; //竖排时的最大宽度，当变大，要重新算
			int nTotalHeight = 0; //竖排总高度

			Visual visual = null;

			ArrayList aVisualUnDefineWidth = null;  //没有定义宽度的Visual组成的数组
			PartInfo partInfo = null;  //用来计宽度的对象

			//横排
			if (LayoutStyle == LayoutStyle.Horizontal )
			{
				//******************************************
				//1.只测宽度,用等号
				//*******************************************
				if ((layoutMember == LayoutMember.CalcuWidth ))
				{
					nTotalWidth = 0;  //总宽度赋0
					if (aVisualUnDefineWidth != null)
						aVisualUnDefineWidth.Clear ();
					else 
						aVisualUnDefineWidth = new ArrayList ();

					if (this.childrenVisual != null)
					{
						//把数组中找到定义的宽度
						for(int i = 0 ; i < this.childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							PartWidth  partWidth = item.GetPartWidth (visual.GetType ().Name );
						
							//没找到对象，或级别号小于等于0，加到未定义宽度数组
							if (partWidth == null
								|| partWidth.nGradeNo <= 0)
							{
								aVisualUnDefineWidth.Add (visual);
								continue;
							}
							nPartWidth = partWidth.nWidth ;
							nTotalWidth += nPartWidth;
						}
					}

					//算那些没有在数组里定义的宽度的区域
					if (aVisualUnDefineWidth != null 
						&& aVisualUnDefineWidth.Count >0)
					{
						//计算得到其它项的平均宽度
						int nTemp = nInitialWidth
							- nTotalWidth
							- this.TotalRestWidth;

						nPartWidth = nTemp/(aVisualUnDefineWidth.Count);

						for(int i=0;i<aVisualUnDefineWidth.Count ;i++)
						{
							visual = (Visual)aVisualUnDefineWidth[i];

							visual.Layout (0,
								0,
								nPartWidth,
								0,   //不关心高度
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.CalcuWidth );   //只测宽度

							nTotalWidth += nRetPartWidth;
						}
					}

					//算返回宽度
					nTotalWidth += this.TotalRestWidth;
					nRetWidth = (nRetWidth > nTotalWidth) ? nRetWidth : nTotalWidth;
					
					goto END1;
				}

				//*****************************************
				//2.即测宽度，又测高度
				//*******************************************
				if (((layoutMember & LayoutMember.CalcuWidth ) == LayoutMember.CalcuWidth )
					&& ((layoutMember & LayoutMember.CalcuHeight) == LayoutMember.CalcuHeight ))
				{
					nTotalWidth = 0;  //总宽度赋0
					if (aVisualUnDefineWidth != null)
						aVisualUnDefineWidth.Clear ();
					else
						aVisualUnDefineWidth = new ArrayList ();


					//最大高度
					nMaxHeight = nInitialHeight 
						- this.TotalRestHeight;

					if (this.childrenVisual != null)
					{
						for(int i = 0 ; i < this.childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							PartWidth  partWidth = item.GetPartWidth (visual.GetType ().Name );
						
							//没找到对象，或级别号小于等于0，加到未定义宽度数组
							if (partWidth == null
								|| partWidth.nGradeNo <= 0)
							{
								aVisualUnDefineWidth.Add (visual);
								continue;
							}

							nPartWidth = partWidth.nWidth ;

							visual.Layout (0,
								0,
								nPartWidth,  //传入一个固定宽度
								nMaxHeight,
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.CalcuWidth | LayoutMember.CalcuHeight  );   //只测宽度

							if (nRetPartHeight > nMaxHeight)
								nMaxHeight = nRetPartHeight;

							//总宽度增加?，宽度是否会发生改变
							nTotalWidth += nRetPartWidth;
						}
					}

					//算那些没有在数组里定义的宽度的区域
					if (aVisualUnDefineWidth != null && aVisualUnDefineWidth.Count >0)
					{
						int nTemp = nInitialWidth
							- nTotalWidth
							- this.TotalRestWidth;

						nPartWidth = nTemp/(aVisualUnDefineWidth.Count);

						for(int i=0;i<aVisualUnDefineWidth.Count ;i++)
						{
							visual = (Visual)aVisualUnDefineWidth[i];

							visual.Layout (0,
								0,
								nPartWidth,
								nMaxHeight,   //0,   //不关心高度
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.CalcuWidth | LayoutMember.CalcuHeight );   //只测宽度

							nTotalWidth += nRetPartWidth;
							if (nRetPartHeight > nMaxHeight)
								nMaxHeight = nRetPartHeight;
						}
					}

					nTotalWidth += this.TotalRestWidth;
					nRetWidth = (nRetWidth > nTotalWidth) ? nRetWidth : nTotalWidth;
					if (nRetWidth < 0)
						nRetWidth = 0;
					
					nMaxHeight += this.TotalRestHeight;;
					nRetHeight = (nRetHeight > nMaxHeight) ? nRetHeight : nMaxHeight;
					if (nRetHeight < 0)
						nRetHeight = 0;
					goto END1;
				}

				//******************************************
				//3.实算
				//********************************************
				if( (layoutMember & LayoutMember.Layout )== LayoutMember.Layout )
				{
					nTotalWidth = 0;  //总宽度赋0
					if (aVisualUnDefineWidth != null)
						aVisualUnDefineWidth.Clear ();
					else
						aVisualUnDefineWidth = new ArrayList ();

					//最大高度
					nMaxHeight = nInitialHeight
						- this.TotalRestHeight;

					//这个数组用来记下每个part的宽度
					ArrayList aWidth = new ArrayList ();

					if (this.childrenVisual != null)
					{
						for(int i = 0 ; i < this.childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							PartWidth  partWidth = item.GetPartWidth (visual.GetType ().Name );
						
							//没找到对象，或级别号小于等于0，加到未定义宽度数组
							if (partWidth == null
								|| partWidth.nGradeNo <= 0)
							{
								aVisualUnDefineWidth.Add (visual);
								continue;
							}
							nPartWidth = partWidth.nWidth ;
							nTotalWidth += nPartWidth;    //这里加的目的是为了以后减少

							//记到数组里
							partInfo = new PartInfo ();
							partInfo.strName = visual.GetType ().Name ;
							partInfo.nWidth = nPartWidth;
							aWidth.Add(partInfo);
						}
					}

					//算那些没有在数组里定义的宽度的区域
					if (aVisualUnDefineWidth != null && aVisualUnDefineWidth.Count >0)
					{
						//这儿引起了没法以最小宽度计算
						int nTemp = nInitialWidth
							- nTotalWidth
							- this.TotalRestWidth;

						nPartWidth = nTemp/(aVisualUnDefineWidth.Count);
						//nPartWidth可能为负数

						for(int i=0;i<aVisualUnDefineWidth.Count ;i++)
						{
							visual = (Visual)aVisualUnDefineWidth[i];
							nTotalWidth += nPartWidth;

							//记到数组里
							partInfo = new PartInfo ();
							partInfo.strName = visual.GetType ().Name ;
							partInfo.nWidth = nPartWidth;
							aWidth.Add(partInfo);
						}
					}

					item.SetValue (this.GetType().Name,nRetWidth);

					//根据布局样式排列一下
					int nXDelta = this.LeftResWidth;
					int nYDelta = this.TopResHeight;

					if (this.childrenVisual != null)
					{
						nTotalWidth = 0;
						int i;
						for(i = 0 ; i < childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							nPartWidth = GetRememberWidth(aWidth,visual.GetType ().Name );
						
							visual.Layout (0 + nXDelta,
								0 + nYDelta,
								nPartWidth,
								0,
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.Layout );
						
							nXDelta += visual.Rect.Width ;
							nTotalWidth += visual.Rect.Width ;
							if (visual.Rect.Height > nMaxHeight)
								nMaxHeight = visual.Rect.Height ;
						}

						for(i = 0 ; i < childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							if (visual.Rect.Height < nMaxHeight)
								visual.Rect.Height = nMaxHeight;
						}
					}

					nTotalWidth += this.TotalRestWidth;
					nRetWidth = (nRetWidth > nTotalWidth) ? nRetWidth : nTotalWidth;
					if (nRetWidth < 0)
						nRetWidth = 0;
					
					nMaxHeight += this.TotalRestHeight;
					nRetHeight = (nRetHeight > nMaxHeight) ? nRetHeight : nMaxHeight;
					if (nRetHeight < 0)
						nRetHeight = 0;

					//把自己的rect设好
					this.Rect = new Rectangle (x,
						y,
						nRetWidth,
						nRetHeight);

					//goto END1;
				}

			}

			//竖排
			if (LayoutStyle == LayoutStyle.Vertical  )
			{
				//******************************************
				//1.只测宽度,用等号
				//*******************************************
				if ((layoutMember == LayoutMember.CalcuWidth ))
				{
					nMaxWidth = nInitialWidth
						- this.TotalRestWidth;

					if (nMaxWidth < 0 )
						nMaxWidth = 0;
					if (this.childrenVisual != null)
					{
						for(int i = 0 ; i < this.childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							visual.Layout (0,
								0,
								nMaxWidth,
								0,
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.CalcuWidth );

							if (nRetPartWidth > nMaxWidth)
								nMaxWidth = nRetPartWidth;
						}
					}
					nMaxWidth += this.TotalRestWidth;
					nRetWidth = (nRetWidth > nMaxWidth) ? nRetWidth : nMaxWidth;

					goto END1;
				}

				//*****************************************
				//2.即测宽度，又测高度
				//*******************************************
				if (((layoutMember & LayoutMember.CalcuWidth ) == LayoutMember.CalcuWidth )
					&& ((layoutMember & LayoutMember.CalcuHeight) == LayoutMember.CalcuHeight ))
				{
					//最大宽度
					nMaxWidth = nInitialWidth 
						- this.TotalRestWidth;

					nTotalHeight= 0;  //总高度赋0

					if (this.childrenVisual != null)
					{
						for(int i = 0 ; i < this.childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							visual.Layout (0,
								0,
								nMaxWidth,
								0,
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.CalcuWidth | LayoutMember.CalcuHeight  );

							if (nRetPartWidth > nMaxWidth)
								nMaxWidth = nRetPartWidth;

							nTotalHeight += nRetPartHeight;
						}
					}

					nMaxWidth += this.TotalRestWidth;
					nRetWidth = (nRetWidth > nMaxWidth) ? nRetWidth : nMaxWidth;

					nTotalHeight += this.TotalRestHeight;
					nRetHeight = (nRetHeight > nTotalHeight) ? nRetHeight : nTotalHeight;
					goto END1;
				}

				//******************************************
				//3.实算
				//********************************************
				if( (layoutMember & LayoutMember.Layout )== LayoutMember.Layout )
				{
					//最大宽度
					nMaxWidth = nInitialWidth 
						- this.TotalRestWidth;
					
					nTotalHeight= 0;  //总高度赋0

					item.SetValue (this.GetType().Name,nRetWidth);

					//根据布局样式排列一下
					int nXDelta = this.LeftResWidth;
					int nYDelta = this.TopResHeight;

					if (this.childrenVisual != null)
					{
						for(int i=0 ;i<childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							nPartHeight = 0;
						
							visual.Layout (0 + nXDelta ,
								0 + nYDelta,
								nMaxWidth,
								nPartHeight,
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.Layout  );
				
							nYDelta += visual.Rect.Height;

							if (visual.Rect.Width > nMaxWidth)
								nMaxWidth = visual.Rect.Width;

							nTotalHeight += visual.Rect.Height;
						}

					}
					nMaxWidth += this.TotalRestWidth;
					nRetWidth = (nRetWidth > nMaxWidth) ? nRetWidth : nMaxWidth;
					if (nRetWidth < 0)
						nRetWidth = 0;

					nTotalHeight += this.TotalRestHeight;
					nRetHeight = (nRetHeight > nTotalHeight) ? nRetHeight : nTotalHeight;
					if (nRetHeight < 0)
						nRetHeight = 0;


					//把自己的rect设好
					this.Rect = new Rectangle (x,
						y,
						nRetWidth,
						nRetHeight);


					//goto END1;
				}

				//****************************************
				//4.只测高度,这里有疑问
				//*****************************************
				if (layoutMember == LayoutMember.CalcuHeight)
				{
					//最大宽度
					nMaxWidth = nInitialWidth 
						- this.TotalRestWidth;

					nTotalHeight= 0;  //总高度赋0
					if (this.childrenVisual != null)
					{
						for(int i = 0 ; i < this.childrenVisual.Count ;i++)
						{
							visual = (Visual)childrenVisual[i];
							visual.Layout (0,
								0,
								nMaxWidth,
								0,
								nTimeStamp,
								out nRetPartWidth,
								out nRetPartHeight,
								LayoutMember.CalcuHeight  );

							if (nRetPartWidth > nMaxWidth)
								nMaxWidth = nRetPartWidth;

							nTotalHeight += nRetPartHeight;
						}
					}
					nTotalHeight += this.TotalRestHeight;
					nRetHeight = (nRetHeight > nTotalHeight) ? nRetHeight : nTotalHeight;

					goto END1;
				}
			}

			if ((layoutMember & LayoutMember.Up  ) == LayoutMember.Up )
			{
				Visual.UpLayout(this,nTimeStamp);
			}

			END1:
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


		//得到在记下的宽度
		public int GetRememberWidth(ArrayList aWidth,string strName)
		{
			foreach(PartInfo partInfo in aWidth)
			{
				if (partInfo.strName == strName)
					return partInfo.nWidth;
			}
			return -1;
		}


		public override void Paint(PaintEventArgs pe,
			int nBaseX,
			int nBaseY,
			PaintMember paintMember)
		{
			Rectangle rectPaintThis = new Rectangle (0,0,0,0);
			rectPaintThis = new Rectangle (nBaseX + this.Rect.X,
				nBaseY + this.Rect.Y,
				this.Rect.Width,
				this.Rect.Height);

			if (rectPaintThis.IntersectsWith (pe.ClipRectangle )== false)
				return;

			Item item = this.GetItem ();
			if (item == null)
				return;
			
			//?
			Object colorDefault = null;
			XmlEditor editor = item.m_document;
			if (editor != null && editor.VisualCfg != null)
				colorDefault = editor.VisualCfg.transparenceColor ;
			if (colorDefault != null)
			{
				if (((Color)colorDefault).Equals (BackColor) == true)
					goto SKIPDRAWBACK;

			}
            using (Brush brush = new SolidBrush(this.BackColor))
            {
                pe.Graphics.FillRectangle(brush, rectPaintThis);
            }

			SKIPDRAWBACK:

				if (editor != null && editor.VisualCfg == null)
				{
				}
				else
				{
					//调DrawLines画边框
					this.DrawLines(rectPaintThis,
						this.TopBorderHeight,
						this.BottomBorderHeight,
						this.LeftBorderWidth,
						this.RightBorderWidth,
						this.BorderColor);
				}

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


				if (editor != null 
					&& editor.VisualCfg == null)
				{

					if (i == this.childrenVisual.Count-1)
					{
						int nDelta = this.RectAbs.Y + this.Rect.Height 
							- visual.RectAbs.Y - visual.Rect.Height - Visual.BOTTOMBORDERHEIGHT;

						if (nDelta > 0)
						{
							// 画下方线条
							this.DrawLines(new Rectangle(rectPaintChild.X,
								rectPaintChild.Y,
								rectPaintChild.Width,
								rectPaintChild.Height + Visual.BOTTOMBORDERHEIGHT),
								0,
								Visual.BOTTOMBORDERHEIGHT,
								0,
								0,
								visual.BorderColor);
						}

					}

					if (i <= 0)
						continue;
			
					if (this.LayoutStyle == LayoutStyle.Vertical)
					{
						// 只画上方线条
						this.DrawLines(rectPaintChild,
							visual.TopBorderHeight,
							0,
							0,
							0,
							visual.BorderColor);
					}
					else if (this.LayoutStyle == LayoutStyle.Horizontal)
					{
						this.DrawLines(rectPaintChild,
							0,
							0,
							visual.LeftBorderWidth,
							0,
							visual.BorderColor);
					}
				}
			}
		}
	}

}
