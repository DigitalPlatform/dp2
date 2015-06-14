using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Xml.XPath;

using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.Drawing;

namespace DigitalPlatform.Xml
{
	// XmlEditor是Xml编辑控件
	public class XmlEditor : System.Windows.Forms.Control
	{
		#region 成员变量

		// 关于视觉样式
		public VisualCfg VisualCfg = null;  // 显示的样式。引用关系。
		public ItemWidthList widthList = new ItemWidthList (); // 属于一个控件整体的宽度信息。
		public LayoutStyle m_layoutStyle = LayoutStyle.Horizontal; // 控件整体横竖布局风格

		// 内存对象
		public ItemAllocator allocator = new MyItemAllocator();
		public VirtualRootItem VirtualRoot = null;
		internal ElementItem docRoot = null;  //Xml document Item 根

		// 关于catch
		public bool bCatch = true;        // layout阶段是否要缓存尺寸参数
		public int nTime = 0;             // layout次数
		internal int nTimeStampSeed = 0;  // 时间戳

		// 小文本编辑控件
		public MyEdit curEdit = new MyEdit();	//edit控件

		// 缺省值
		public Color BackColorDefault = SystemColors.Control; // 普通区域背景色
        public Color AttrBackColorDefault = ColorUtil.String2Color("#BFCAE6");//Color.Green;  //属性区域颜色
		public Color BackColorDefaultForEditable = SystemColors.Window; 	// 可编辑区域背景色
		public Color TextColorDefault = SystemColors.WindowText; // 文字颜色
		public Color BorderColorDefault = SystemColors.ControlDark;    // .ControlText;	// 对象边框线条颜色
		private BorderStyle borderStyle = BorderStyle.Fixed3D;  // 控件窗口边框

		// 关于拖动
		private int nLastTrackerX = -1;      // 最后一次拖到的X
		private Visual dragVisual = null;    // 拖动的visual

		// 关于文档及卷滚条
		public int nClientWidth = 0;    //客户区宽度
		public int nClientHeight = 0;   //客户区高度
		public int nDocumentOrgX = 0;   //文档偏移量X
		public int nDocumentOrgY = 0;   //文档偏移量Y
		private int nAverageItemHeight = 20 ;   // 平均事项文字高度，用来做卷滚条移动的值

		// 其它
		public Item m_selectedItem = null; // 当前选中的Item对象
		public XmlText m_curText = null;   // 当前编辑的Text对象

		public bool m_bChanged = false;       //是否发生改变
		bool m_bAllowPaint = true;       // 在 BeginUpdate() 和 EndUpdate()处用 
		public bool m_bFocused = false;  // 是否处理获得焦点状态
		private bool bAutoSize = false;      // 是否自动根据窗口客户区宽度改变文档宽度

        public event GenerateDataEventHandler GenerateData = null;

		private System.ComponentModel.Container components = null;

		#endregion

		#region 构造函数

		public XmlEditor()
		{
			InitializeComponent();
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			// 
			// XmlEditor
			// 
			this.EnabledChanged += new System.EventHandler(this.XmlEditorCtrl_EnabledChanged);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.XmlEditorCtrl_KeyDown);
		}

		#endregion

		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#endregion


        // 为了解决卷滚条,重载此函数
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // 控制竖向卷滚
                case API.WM_VSCROLL:
                    {
                        switch (API.LoWord(m.WParam.ToInt32()))
                        {
                            case API.SB_BOTTOM:
                                MessageBox.Show("SB_BOTTOM");
                                break;
                            case API.SB_TOP:
                                MessageBox.Show("SB_TOP");
                                break;
                            case API.SB_THUMBTRACK:
                                this.Update();
                                DocumentOrgY = -API.HiWord(m.WParam.ToInt32());
                                break;
                            case API.SB_LINEDOWN:
                                DocumentOrgY -= nAverageItemHeight;
                                break;
                            case API.SB_LINEUP:
                                DocumentOrgY += nAverageItemHeight;
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgY -= this.ClientSize.Height;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgY += this.ClientSize.Height;
                                break;
                        }
                    }
                    break;

                // 控制横向卷滚
                case API.WM_HSCROLL:
                    {
                        switch (API.LoWord(m.WParam.ToInt32()))
                        {
                            case API.SB_THUMBPOSITION:
                            case API.SB_THUMBTRACK:
                                DocumentOrgX = -API.HiWord(m.WParam.ToInt32());
                                break;
                            case API.SB_LINEDOWN:
                                DocumentOrgX -= 20;
                                break;
                            case API.SB_LINEUP:
                                DocumentOrgX += 20;
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgX -= this.ClientSize.Width;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgX += this.ClientSize.Width;
                                break;
                        }
                    }
                    break;
            }
            base.DefWndProc(ref m);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 初始化小edit控件
            string strError = "";
            int nRet = this.curEdit.Initial(this,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);
        }


		// 绘制背景图
		protected override void OnPaintBackground(PaintEventArgs e)
		{
            base.OnPaintBackground(e);

			if (this.m_bAllowPaint == false)
				return;

			if (this.VisualCfg == null)
				goto DEFAULT;

			if (this.VisualCfg.strBackPicUrl == "")
				goto DEFAULT;
			
			// 绘制背景图像
            Image image = null;
            string strError = "";
            int nRet = DrawingUtil.GetImageFormUrl(this.VisualCfg.strBackPicUrl,
                out image,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

			this.PaintImageBackground(image);

            image.Dispose();    // 2006/7/26 add

			return;

			DEFAULT:
			{
                Color defaultBackColor = SystemColors.Control;
                Brush brush = new SolidBrush(defaultBackColor);
				e.Graphics.FillRectangle(brush, e.ClipRectangle);
				brush.Dispose();
			}
		}
       
       
        // 绘制背景
        public void PaintImageBackground(Image imageFile)
        {
            Graphics graphics = Graphics.FromImage(imageFile);

            if (this.VisualCfg.backPicStyle == BackPicStyle.Tile)
            {
                int nXDelta = 0;
                int nYDelta = 0;

                int nXCount = 0;
                int nClientWidth;
                int nXStart = (this.nDocumentOrgX) % imageFile.Width;

                nXDelta = (-this.nDocumentOrgX) % imageFile.Width;
                nXDelta = imageFile.Width - nXDelta;

                if (nXDelta != 0)
                    nXCount++;

                nClientWidth = this.ClientSize.Width - nXDelta;
                if (nClientWidth > 0)
                {
                    nXCount += nClientWidth / imageFile.Width;
                }
                nXDelta = nClientWidth % imageFile.Width;
                if (nXDelta != 0)
                    nXCount++;


                int nYCount = 0;
                int nClientHeight;
                int nYStart = (this.nDocumentOrgY) % imageFile.Height;
                nYDelta = (-this.nDocumentOrgY) % imageFile.Height;
                nYDelta = imageFile.Height - nYDelta;

                if (nYDelta != 0)
                    nYCount++;

                nClientHeight = this.ClientSize.Height - nYDelta;
                if (nClientHeight > 0)
                {
                    nYCount += nClientHeight / imageFile.Height;
                }
                nYDelta = nClientHeight % imageFile.Height;
                if (nYDelta != 0)
                    nYCount++;

                for (int i = 0; i < nXCount; i++)
                {
                    for (int j = 0; j < nYCount; j++)
                    {
                        graphics.DrawImage(imageFile,
                            i * imageFile.Width + nXStart,
                            j * imageFile.Height + nYStart,
                            imageFile.Width,
                            imageFile.Height);
                    }
                }
            }
            else if (this.VisualCfg.backPicStyle == BackPicStyle.Fill)
            {
                graphics.DrawImage(imageFile,
                    0 + this.nDocumentOrgX,
                    0 + this.nDocumentOrgY,
                    this.DocumentWidth,
                    this.DocumentHeight);
            }
            else if (this.VisualCfg.backPicStyle == BackPicStyle.Center)
            {
                int nX = this.DocumentWidth / 2 - imageFile.Width / 2;
                int nY = this.DocumentHeight / 2 - imageFile.Height / 2;

                graphics.DrawImage(imageFile,
                    nX + this.nDocumentOrgX,
                    nY + this.nDocumentOrgY,
                    imageFile.Width,
                    imageFile.Height);

            }
            graphics.Dispose();
        }
		
		// 重载绘制函数
		protected override void OnPaint(PaintEventArgs pe)
		{
			if (this.m_bAllowPaint == false)
				return;	// 优化速度

			if (this.VirtualRoot == null) 
			{
				base.OnPaint(pe);
				return;
			}

			// 调根元素的Paint()
			this.VirtualRoot.Paint(pe,
				nDocumentOrgX,
				nDocumentOrgY,
				PaintMember.Both);
		}


		// 为了带上边框，重载该属性
		protected override CreateParams CreateParams
		{
			get 
			{
				CreateParams param = base.CreateParams;
				
                //设为带边框样式
				if (borderStyle == BorderStyle.FixedSingle) 
					param.Style |= API.WS_BORDER;
				else if (borderStyle == BorderStyle.Fixed3D) 
					param.ExStyle |= API.WS_EX_CLIENTEDGE;

				return param;
			}
		}
		
		#region 关于小textbox control的一些函数

		// 将Edit控件中的文字内容兑现到Visual视图上
		// 不负责修改屏幕图像
		internal void EditControlTextToVisual()
		{
			if (this.m_curText != null)
			{
				string strOldValue = this.m_curText.Text;
				if (this.m_curText.Text != this.curEdit.Text)
				{
					this.m_curText.Text = this.curEdit.Text;
					Item item = this.m_curText.GetItem();
					if (item != this.m_selectedItem)
					{
						Debug.Assert(false,"当前Text对应的Item与SelectedItem不一致");
						throw(new Exception("当前Text对应的Item与SelectedItem不一致"));
					}
					if (this.m_selectedItem is ElementItem)
					{
						ElementItem element = (ElementItem)this.m_selectedItem;
						
						if (this.m_curText.Name == "attributes")
							element.m_xmlAttrsTimestamp ++;
						if (this.m_curText.Name == "content")
							element.m_xmlChildrenTimestamp ++;

						element.Flush();	
					}

					if (this.m_selectedItem is AttrItem)
					{
						// 触发事件
						////////////////////////////////////////////////////
						// ItemAttrChanged
						////////////////////////////////////////////////////
						ItemChangedEventArgs args = 
							new ItemChangedEventArgs();
						args.item = this.m_selectedItem;
						args.NewValue = this.m_curText.Text;
						args.OldValue = strOldValue;
						this.fireItemChanged(this,args);
					}

					// 文档发生变化
					this.FireTextChanged();
				}
			}
		}


		// 将当前Visual视图区域的内容兑现到Edit控件
		// 不负责修改屏幕图像
		internal void VisualTextToEditControl()
		{
			if (this.m_curText != null)
			{
				this.curEdit.Text = this.m_curText.Text;
			}
		}


		// 为SetEditPos()编写的私有函数
		public void ChangeEditSizeAndMove(XmlText text)
		{
			if (text == null)
			{
				Debug.Assert (false,"ChangeEditSizeAndMove()，传入的text为null");
				return;
			}

			// edit的旧size
			Size oldsize = curEdit.Size;

			// edit即将被设给的新size
			Size newsize = new Size(0,0);
			newsize = new System.Drawing.Size(
				text.Rect.Width - text.TotalRestWidth,
				text.Rect.Height - text.TotalRestHeight);

			// text相对于窗口的绝对坐标
			Rectangle rectLoc = text.RectAbs;
			rectLoc.Offset(this.nDocumentOrgX ,
				this.nDocumentOrgY);

			// 给edit设的坐标(即text去掉左边边框与空白)
			Point loc = new Point(0,0);
			loc = new System.Drawing.Point(
				rectLoc.X + text.LeftResWidth,
				rectLoc.Y + text.TopResHeight);

			// 避免多绘制区域
			// 从小变大，先move然后改变size
			if (oldsize.Height < newsize.Height)
			{
				curEdit.Location = loc;
				curEdit.Size = newsize;
			}
			else 
			{
				// 从大变小，先size然后改变move
				curEdit.Size = newsize;
				curEdit.Location = loc;
			}
			curEdit.Font = text.GetFont();
		}

		
		// 兑现edit控件位置大小到当前视图对象。
		public void SetEditPos()
		{
			if (this.m_curText == null)
			{
				//curEdit.Hide();
                curEdit.Size = new Size(0, 0);
                //curEdit.Enabled = false;
				return;
			}

			this.curEdit.Show();
            //this.curEdit.Enabled = true;

			// 当未选择其它节点,直接单击xmleditor外的其它控件,
			// 也会调CurText属性,以便把最新的内容放在内存对象中
			// 但edit不应再获得焦点,所以特意设了一个变量bEditorGetFocus来解决这个问题。
			//if (this.bEditGetFocus == true)  
			if (this.m_bFocused == true)
				this.curEdit.Focus();
			ChangeEditSizeAndMove(this.m_curText);
		}

		#endregion
	

		#region 关于卷滚条的函数
		
		// 文档尺寸发生变化
		internal void AfterDocumentChanged(ScrollBarMember member)
		{
			if (bAutoSize == true) 
			{
				this.ClientSize = new Size(this.DocumentWidth, this.DocumentHeight);
			}
			else 
			{
				SetScrollBars(member);
			}
		}

		// 设卷滚条
		void SetScrollBars(ScrollBarMember member)
		{
			if (bAutoSize == true) 
			{
				API.ShowScrollBar(this.Handle,
					API.SB_HORZ,
					false);
				API.ShowScrollBar(this.Handle,
					API.SB_VERT,
					false);
				return;
			}

			nClientWidth = this.ClientSize.Width;
			nClientHeight = this.ClientSize.Height;

			if (member == ScrollBarMember.Horz
				|| member == ScrollBarMember.Both) 
			{
				// 水平方向
				API.ScrollInfoStruct si = new API.ScrollInfoStruct();
				si.cbSize = Marshal.SizeOf(si);
				si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
				si.nMin = 0;
				si.nMax = DocumentWidth ;
				si.nPage = nClientWidth;
				si.nPos = -nDocumentOrgX;
				API.SetScrollInfo(this.Handle, API.SB_HORZ, ref si, true);
			}


			if (member == ScrollBarMember.Vert
				|| member == ScrollBarMember.Both) 
			{
				// 垂直方向
				API.ScrollInfoStruct si = new API.ScrollInfoStruct();
				si.cbSize = Marshal.SizeOf(si);
				si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
				si.nMin = 0;
				si.nMax = DocumentHeight ;
				si.nPage = nClientHeight;
				si.nPos = -nDocumentOrgY;
				API.SetScrollInfo(this.Handle, API.SB_VERT, ref si, true);
			}
		}

		// 文档横向编移量
		public int DocumentOrgX
		{
			get 
			{
				return nDocumentOrgX;
			}
			set 
			{
				int nWidth = DocumentWidth ;
				int nViewportWidth = this.ClientSize.Width;

				int nDocumentOrgX_old = nDocumentOrgX;

				if (nViewportWidth >= nWidth)
					nDocumentOrgX = 0;
				else 
				{
					if (value <= - nWidth + nViewportWidth)
						nDocumentOrgX = -nWidth + nViewportWidth;
					else
						nDocumentOrgX = value;

					if (nDocumentOrgX > 0)
						nDocumentOrgX = 0;
				}

				SetEditPos();
				AfterDocumentChanged(ScrollBarMember.Horz);


				int nDelta = nDocumentOrgX - nDocumentOrgX_old;
				if ( nDelta != 0 ) 
				{
					RECT rect1 = new RECT();
					rect1.left = 0;
					rect1.top = 0;
					rect1.right = this.ClientSize.Width;
					rect1.bottom = this.ClientSize.Height;

					API.ScrollWindowEx(this.Handle,
						nDelta,
						0,
						ref rect1,
						IntPtr.Zero,	//	ref RECT lprcClip,
						0,	// int hrgnUpdate,
						IntPtr.Zero,	// ref RECT lprcUpdate,
						API.SW_INVALIDATE /*| API.SW_SCROLLCHILDREN*/ /*int fuScroll*/);
				}
				//this.Invalidate();
			}
		}

		// 文档纵向偏移量
		public int DocumentOrgY
		{
			get 
			{
				return nDocumentOrgY;
			}
			set 
			{
				int nHeight = DocumentHeight ;
				int nViewportHeight = this.ClientSize.Height;

				int nDocumentOrgY_old = nDocumentOrgY;

				if (nViewportHeight >= nHeight)
					nDocumentOrgY = 0;
				else 
				{
					if (value <= - nHeight + nViewportHeight)
						nDocumentOrgY = -nHeight + nViewportHeight;
					else
						nDocumentOrgY = value;

					if (nDocumentOrgY > 0)
						nDocumentOrgY = 0;
				}

				SetEditPos();
				AfterDocumentChanged(ScrollBarMember.Vert);

				int nDelta = nDocumentOrgY - nDocumentOrgY_old;
				if ( nDelta != 0 ) 
				{
					RECT rect1 = new RECT();
					rect1.left = 0;
					rect1.top = 0;
					rect1.right = this.ClientSize.Width;
					rect1.bottom = this.ClientSize.Height;

					API.ScrollWindowEx(this.Handle,
						0,
						nDelta,
						ref rect1,
						IntPtr.Zero,	//	ref RECT lprcClip,
						0,	// int hrgnUpdate,
						IntPtr.Zero,	// ref RECT lprcUpdate,
						API.SW_INVALIDATE /*| API.SW_SCROLLCHILDREN*/ /*int fuScroll*/);
				}
				//this.Invalidate();
			}
		}

		// 让visual块的rectCaret尺寸可见
		private void EnsureVisible(Visual visual,
			Rectangle rectCaret)
		{
			if (visual == null)
				return;

			int nDelta = visual.RectAbs.Y + visual.Rect.Height
				+ this.nDocumentOrgX 
				+ rectCaret.Y;

			if (nDelta + rectCaret.Height >= this.ClientSize.Height) 
			{
				if (rectCaret.Height >= this.ClientSize.Height) 
					DocumentOrgY = DocumentOrgY - (nDelta + rectCaret.Height) + ClientSize.Height + /*调整系数*/ (rectCaret.Height/2) - (this.ClientSize.Height/2);
				else
					DocumentOrgY = DocumentOrgY - (nDelta + rectCaret.Height) + ClientSize.Height;
			}
			else if (nDelta < 0)
			{
				if (rectCaret.Height >= this.ClientSize.Height) 
					DocumentOrgY = DocumentOrgY - (nDelta) - /*调整系数*/ ( (rectCaret.Height/2) - (this.ClientSize.Height/2));
				else 
					DocumentOrgY = DocumentOrgY - (nDelta);
			}
			else 
			{
				// y不需要卷滚
			}

			////
			// 水平方向
			nDelta = 0;

			nDelta = visual.RectAbs .X + visual.Rect.Width 
				+ this.nDocumentOrgX 
				+ rectCaret.X ;
			

			if (nDelta + rectCaret.Width >= this.ClientSize.Width) 
			{
				if (rectCaret.Width >= this.ClientSize.Width) 
					DocumentOrgX = DocumentOrgX - (nDelta + rectCaret.Width) + ClientSize.Width + /*调整系数*/ (rectCaret.Width/2) - (this.ClientSize.Width/2);
				else
					DocumentOrgX = DocumentOrgX - (nDelta + rectCaret.Width) + ClientSize.Width;
			}
			else if (nDelta < 0)
			{
				if (rectCaret.Width >= this.ClientSize.Width) 
					DocumentOrgX = DocumentOrgX - (nDelta) - /*调整系数*/ ( (rectCaret.Width/2) - (this.ClientSize.Width/2));
				else 
					DocumentOrgX = DocumentOrgX - (nDelta);
			}
			else 
			{
				// x不需要卷滚
			}

		}


		#endregion

		#region override一些事件

		// 鼠标按下的相关事情
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (this.VirtualRoot == null)
			{
				base.OnMouseDown(e);
				return;
			}

			this.Capture = true;

			Point p = new Point(e.X, e.Y);
			//换算成相对doucment的坐标
			p = new Point (p.X - this.nDocumentOrgX ,
				p.Y - this.nDocumentOrgY );

			Visual visual = null;
			int nRet = this.VirtualRoot.HitTest(p,out visual);

			if (visual == null)
				goto FINISH;
			if (nRet == -1)
				goto FINISH;

			Item item = visual.GetItem();



			//**************************************
			//单击到展开按钮
			//***************************************
			ExpandStyle expandChildren = ExpandStyle.None;
			ExpandStyle expandAttrs = ExpandStyle.None;

			ExpandStyle oldChildrenExpand = ExpandStyle.None;
			ExpandStyle oldAttrsExpand = ExpandStyle.None;

			if (visual.IsExpandHandle() == true)
			{
				this.EditControlTextToVisual();

				ElementItem element = (ElementItem)item;	// 既然是可展开的，则必然是ElementItem

				expandChildren = element.m_childrenExpand;
				expandAttrs = element.m_attrsExpand;

				//3.根据展开按钮的名称判断出哪里换了状态
				ExpandHandle myExpandHandle = ((ExpandHandle)visual);
				if (myExpandHandle.Name == "ExpandContent")
				{
					if (expandChildren == ExpandStyle.None)
					{
						Debug.Assert(false, "");
					}
					else 
					{
						expandChildren = (expandChildren == ExpandStyle.Expand) ? ExpandStyle.Collapse : ExpandStyle.Expand;
					}
				}
				else if (myExpandHandle.Name == "ExpandAttributes")
				{
					if (expandAttrs == ExpandStyle.None)
					{
						Debug.Assert(false, "");
					}
					else 
					{
						expandAttrs = (expandAttrs == ExpandStyle.Expand) ? ExpandStyle.Collapse : ExpandStyle.Expand;
					}
				}

				
				oldChildrenExpand = element.m_childrenExpand;
				oldAttrsExpand = element.m_attrsExpand;

				element.ExpandAttrsOrChildren(expandAttrs,
					expandChildren,
					true);


				goto END1;
			}

			//*****************************************
			//在缝上
			//*****************************************
			if (nRet == 2) 
			{
				dragVisual = visual;

				//-----------------------------------------
				//做测试用，意思是把拖动的visual失效，从而变成红色
				dragVisual.bDrag = true;
				Rectangle rectTemp = new Rectangle (dragVisual.RectAbs.X + this.nDocumentOrgX ,
					dragVisual.RectAbs .Y + this.nDocumentOrgY ,
					dragVisual.RectAbs .Width ,
					dragVisual.RectAbs .Height);
				this.Invalidate ( rectTemp);
				//------------------------------------------

				// 第一次
				nLastTrackerX = e.X;
				DrawTracker();
				goto END1;
			}

			END1:

				//m_clickVisual = visual;
				if (visual != null )
				{
					//失效前一个
					if (this.m_selectedItem != null)
					{
						Rectangle rectTemp = new Rectangle (
							this.m_selectedItem.RectAbs.X + this.nDocumentOrgX ,
							this.m_selectedItem.RectAbs .Y + this.nDocumentOrgY ,
							this.m_selectedItem.RectAbs .Width ,
							this.m_selectedItem.RectAbs .Height);
						this.Invalidate (rectTemp);
					}

					this.EditControlTextToVisual();

					//this.m_selectedItem = item;
					if (visual is XmlText)
					{
						this.SetCurText(item,(XmlText)visual);
					}
					else
					{
						this.SetCurText(item,null);
					}
					this.SetActiveItem(item);

					if (this.m_selectedItem != null)
					{
						Rectangle rectTemp = new Rectangle (
							this.m_selectedItem.RectAbs.X + this.nDocumentOrgX ,
							this.m_selectedItem.RectAbs .Y + this.nDocumentOrgY ,
							this.m_selectedItem.RectAbs .Width ,
							this.m_selectedItem.RectAbs .Height);
						this.Invalidate (rectTemp);
					}
				}

			//在文本上
			if ((visual is XmlText) &&  (nRet == 0))
			{
				//这里模拟单击一下，但位置跑了一大截
				curEdit.Focus();

				int x = e.X - curEdit.Location.X;
				int y = e.Y - curEdit.Location.Y;

				API.SendMessage(curEdit.Handle, 
					API.WM_LBUTTONDOWN, 
					new UIntPtr(API.MK_LBUTTON),	//	UIntPtr wParam,
					API.MakeLParam(x,y));
			
			}

			FINISH:
				base.OnMouseDown (e);
		}


		// 右键菜单
		private void PopupMenu(Point p)
		{
			ContextMenu contextMenu = new ContextMenu();

			MenuItem menuItem;
			MenuItem subMenuItem;

			string strName = "''";

			Item item = this.m_selectedItem;
			ElementItem element = null;
				
			if (item is ElementItem)
				element = (ElementItem)item;

			if (item != null)
				strName = "'" + item.Name  + "'";

			// 展开
			menuItem = new MenuItem("展开");
			contextMenu.MenuItems.Add(menuItem);
			if (element != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;


			// 展开属性
			subMenuItem = new MenuItem("属性");
			subMenuItem.Click += new System.EventHandler(this.menuItem_ExpandAttrs);
			menuItem.MenuItems.Add(subMenuItem);
			if (element != null
				&& element != this.VirtualRoot
				&& element.m_attrsExpand == ExpandStyle.Collapse)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;


			// 展开下级
			subMenuItem = new MenuItem("下级");
			subMenuItem.Click += new System.EventHandler(this.menuItem_ExpandChildren);
			menuItem.MenuItems.Add(subMenuItem);
			if (element != null
				&& element.m_childrenExpand == ExpandStyle.Collapse)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;


			// 收缩
			menuItem = new MenuItem("收缩");
			contextMenu.MenuItems.Add(menuItem);
			if (element != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;


			// 收缩属性
			subMenuItem = new MenuItem("属性");
			subMenuItem.Click += new System.EventHandler(this.menuItem_CollapseAttrs);
			menuItem.MenuItems.Add(subMenuItem);
			if (element != null
				&& element != this.VirtualRoot
				&& element.m_attrsExpand == ExpandStyle.Expand)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;


			// 收缩下级
			subMenuItem = new MenuItem("下级");
			subMenuItem.Click += new System.EventHandler(this.menuItem_CollapseChildren);
			menuItem.MenuItems.Add(subMenuItem);
			if (element != null
				&& element.m_childrenExpand == ExpandStyle.Expand)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;

			//--------------
			menuItem = new MenuItem ("-");
			contextMenu.MenuItems .Add (menuItem);

			// 插入属性
			menuItem = new MenuItem("插入属性");
			contextMenu.MenuItems.Add(menuItem);
			if ((element != null && element != this.VirtualRoot)
				|| item is AttrItem )
			{
				menuItem.Enabled = true;
			}
			else
				menuItem.Enabled = false;


			//1.新属性
			subMenuItem = new MenuItem("新属性(尾部)");// + strName);
			subMenuItem.Click += new System.EventHandler(this.menuItem_AppendAttr);
			menuItem.MenuItems.Add(subMenuItem);
			if (( element != null && element != this.VirtualRoot)
				|| item is AttrItem)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;


			//2.前插
			subMenuItem = new MenuItem("前插");// + strName );
			subMenuItem.Click += new System.EventHandler(this.menuItem_InsertSiblingAttr);
			menuItem.MenuItems.Add(subMenuItem);
			if (item is AttrItem)
				subMenuItem.Enabled = true;
			else
				subMenuItem.Enabled = false;

			//--------------
			menuItem = new MenuItem ("-");
			contextMenu.MenuItems .Add (menuItem);


			// 新下级
			menuItem = new MenuItem("新下级");
			contextMenu.MenuItems.Add(menuItem);
			if (element != null)
			{
				menuItem.Enabled = true;
			}
			else
				menuItem.Enabled = false;

			//3.素
			subMenuItem = new MenuItem("元素");// + strName );
			subMenuItem.Click += new System.EventHandler(this.menuItem_AppendChild);
			menuItem.MenuItems.Add(subMenuItem);
			if (element != null)
			{
				if(element == this.VirtualRoot)
				{
					if (element.children.Count == 0)
						subMenuItem.Enabled = true;
					else
						subMenuItem.Enabled = false;
				}
				else
				{
					subMenuItem.Enabled = true;
				}
			}
			else
				subMenuItem.Enabled = false;


			//4.文本
			subMenuItem = new MenuItem("文本");// + strName );
			subMenuItem.Click += new System.EventHandler(this.menuItem_AppendText);
			menuItem.MenuItems.Add(subMenuItem);
			if (element != null && element != this.VirtualRoot)
			{
				bool bText = false;
				int nCount = element.children.Count;
				if (nCount > 0 && (element.children[nCount-1] is TextItem))
					bText = true;

				if (bText == true)
					subMenuItem.Enabled = false;
				else
					subMenuItem.Enabled = true;
			}
			else
			{
				subMenuItem.Enabled = false;
			}


			menuItem = new MenuItem("新同级");
			contextMenu.MenuItems.Add(menuItem);
			if ((element != null && element != this.VirtualRoot && element != this.docRoot)
				|| item is TextItem)
			{
				menuItem.Enabled = true;
			}
			else
				menuItem.Enabled = false;


			//5.新同级元素
			subMenuItem = new MenuItem("元素");// + strName );
			subMenuItem.Click += new System.EventHandler(this.menuItem_InsertSiblingChild);
			menuItem.MenuItems.Add(subMenuItem);
			if ((element != null && element != this.VirtualRoot && element != this.docRoot)
				|| item is TextItem)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;

			//6.新同级文本
			subMenuItem = new MenuItem("文本");// + strName );
			subMenuItem.Click += new System.EventHandler(this.menuItem_InsertSiblingText);
			menuItem.MenuItems.Add(subMenuItem);
			if ((element != null && element != this.VirtualRoot && element != this.docRoot))
			{
				Item frontItem = ItemUtil.GetNearItem(element,
					MoveMember.Front );

				//前后都不是文本节点时有效
				if (!(frontItem is TextItem ))
				{
					subMenuItem.Enabled = true;
				}
				else
				{
					subMenuItem.Enabled = false;
				}
			}
			else
			{
				subMenuItem.Enabled = false;
			}

			//补充一个新建根元素命令

			//新建根元素
			menuItem = new MenuItem("新建根元素");
			menuItem.Click += new System.EventHandler(this.menuItem_CreateRoot);
			contextMenu.MenuItems.Add(menuItem);
			if (this.VirtualRoot == null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;


			//-------
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			//7.删除
			menuItem = new MenuItem("删除");// + strName);
			menuItem.Click += new System.EventHandler(this.menuItem_Delete);
			contextMenu.MenuItems.Add(menuItem);
			if (item != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;

			//--------------
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			// 剪切
			menuItem = new MenuItem("剪切");// + strName);
			menuItem.Click += new System.EventHandler(this.menuItem_Cut);
			contextMenu.MenuItems.Add(menuItem);
			if (item != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;

			//复制
			menuItem = new MenuItem("复制");// + strName);
			menuItem.Click += new System.EventHandler(this.menuItem_Copy);
			contextMenu.MenuItems.Add(menuItem);
			if (item != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;


			//粘贴覆盖
			menuItem = new MenuItem("粘贴覆盖");// + strName);
			menuItem.Click += new System.EventHandler(this.menuItem_PasteOver);
			contextMenu.MenuItems.Add(menuItem);
			IDataObject ido = Clipboard.GetDataObject();
			if (ido.GetDataPresent(DataFormats.Text))
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;

			//粘贴覆盖
			menuItem = new MenuItem("粘贴插入");
			contextMenu.MenuItems.Add(menuItem);
			if (ido.GetDataPresent(DataFormats.Text) && item != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;


			subMenuItem = new MenuItem("同级前插");// + strName );
			subMenuItem.Click += new System.EventHandler(this.menuItem_PasteInsert_InsertBefore);
			menuItem.MenuItems.Add(subMenuItem);
			if (ido.GetDataPresent(DataFormats.Text)
				&& item != null
				&& item != this.VirtualRoot
				&& item != this.docRoot)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;

			subMenuItem = new MenuItem("同级后插");
			subMenuItem.Click += new System.EventHandler(this.menuItem_PasteInsert_InsertAfter);
			menuItem.MenuItems.Add(subMenuItem);
			if (ido.GetDataPresent(DataFormats.Text)
				&& item != null
				&& item != this.VirtualRoot
				&& item != this.docRoot)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;

			subMenuItem = new MenuItem("下级末尾");
			subMenuItem.Click += new System.EventHandler(this.menuItem_PasteInsert_AppendChild);
			menuItem.MenuItems.Add(subMenuItem);
			if (ido.GetDataPresent(DataFormats.Text) && element != null)
			{
				subMenuItem.Enabled = true;
			}
			else
				subMenuItem.Enabled = false;



			//-------
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);



			menuItem = new MenuItem("布局");
			contextMenu.MenuItems.Add(menuItem);

			//横排布局
			subMenuItem = new MenuItem("横排");
			subMenuItem.Click += new System.EventHandler(this.menuItem_Horz);
			menuItem.MenuItems.Add(subMenuItem);
			if (this.LayoutStyle == LayoutStyle.Vertical)
				subMenuItem.Enabled = true;
			else
				subMenuItem.Enabled = false;
				


			//竖排布局
			subMenuItem = new MenuItem("竖排");
			subMenuItem.Click += new System.EventHandler(this.menuItem_Vert);
			menuItem.MenuItems.Add(subMenuItem);
			if (this.LayoutStyle == LayoutStyle.Horizontal)
				subMenuItem.Enabled = true;
			else
				subMenuItem.Enabled = false;



			//-------
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			//优化名字空间
			menuItem = new MenuItem("优化名字空间");
			menuItem.Click += new System.EventHandler(this.menuItem_YuHua);
			contextMenu.MenuItems.Add(menuItem);
			if (element != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;


			//-------
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("Properties");
			menuItem.Click += new System.EventHandler(this.menuItem_Properties);
			contextMenu.MenuItems.Add(menuItem);
			if (item != null)
				menuItem.Enabled = true;
			else
				menuItem.Enabled = false;
			/*
						//-------
						menuItem = new MenuItem("-");
						contextMenu.MenuItems.Add(menuItem);

						menuItem = new MenuItem("Flush");
						menuItem.Click += new System.EventHandler(this.menuItem_Flush);
						contextMenu.MenuItems.Add(menuItem);
						if (seletectedElement != null)
							menuItem.Enabled = true;
						else
							menuItem.Enabled = false;
			*/				
/*
			//-------
			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("测试EnsureVisible");
			menuItem.Click += new System.EventHandler(this.menuItem_test_EnsureVisible);
			contextMenu.MenuItems.Add(menuItem);

*/
			contextMenu.Show(this, p);
		}


		// 拖拽放开时
		private void DragUp()
		{
			this.Capture = false;

			if (dragVisual != null) 
			{
				// 消最后残余的一根
				DrawTracker();
				Item item = dragVisual.GetItem ();

				//得坐相对文档的x
				//再转化成相对窗口的x
				int x0 = dragVisual.getAbsX() + dragVisual.Rect.Width;
				x0 += this.nDocumentOrgX ;
				
				// 计算差额
				int delta = nLastTrackerX - x0;
				if (item != null)
				{
					int nTemp = dragVisual.Rect.Width + delta;
					if (nTemp <= 0)
						nTemp = 2;
					//把新宽度设到数组里，设宽度值的过程中，会把级别号升高
					item.SetValue(dragVisual.GetType().Name,
						nTemp);

					PartWidth partWidth = item.GetPartWidth (dragVisual.GetType ().Name );
					if (partWidth != null )
					{
						if (partWidth.nGradeNo >0)
						{
							//当本宽度级别号大于0时，将所有的级别上升，以致当拖动固定宽度时，不会影响其它宽度
							ItemWidth itemWidth = this.widthList .GetItemWidth (item.GetLevel());
							foreach(PartWidth part in itemWidth )
							{
								part.UpGradeNo ();
							}
						}
						else
						{
							partWidth.UpGradeNo ();
						}
					}
					
					//改变visual的宽度
					dragVisual.Rect.Width = nTemp;// dragVisual.rect .Width + delta;

					//从而layout及下级
					int nWidth,nHeight;
					dragVisual.Layout(dragVisual.Rect.X,
						dragVisual.Rect.Y,
						dragVisual.Rect.Width,
						dragVisual.Rect.Height,
						nTimeStampSeed,
						out nWidth,
						out nHeight,
						LayoutMember.Layout | LayoutMember.Up );
						
					Visual tempContainer = dragVisual.container ;
					if (tempContainer != null)
					{
						//MessageBox.Show (tempContainer.rect .ToString ());
					}

					nTimeStampSeed++;
				{
					//先将当前对象的级别号降到缺省值
					partWidth = item.GetPartWidth (dragVisual.GetType ().Name );
					if (partWidth != null)
						partWidth.BackDefaultGradeNo ();

					//将所有的宽度的级别号降到缺省值
					ItemWidth itemWidth = this.widthList .GetItemWidth (item.GetLevel());
					foreach(PartWidth part in itemWidth )
					{
						part.BackDefaultGradeNo  ();
					}
				}

					//curText从属的Item在被变化的范围内才重设，但改变列宽后，所有的Item都影响到了，所以这儿不用做判断了
					SetEditPos();

					//文档尺寸变化，做一些善后事情
					this.AfterDocumentChanged (ScrollBarMember.Both );

					this.Invalidate ();
				}
				dragVisual.bDrag = false;
				this.dragVisual = null;

				nLastTrackerX = -1;
			}
		}
	

		// 鼠标放开的相关事情
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{	
				PopupMenu(new Point(e.X, e.Y) );
			}
			else 
			{
				this.DragUp();
			}
			//END1:
			base.OnMouseUp(e);
		}
		
		private void DragMove(Point p)
		{
			if (this.VirtualRoot == null)
				return;

			if (dragVisual != null) 
			{
				Cursor = Cursors.SizeWE;
				// 消上次残余的一根
				DrawTracker();
				nLastTrackerX = p.X;
				// 绘制本次的一根
				DrawTracker();
			}
			else 
			{
				//得到相当文档的坐标
				p = new Point (p.X - this.nDocumentOrgX ,
					p.Y - this.nDocumentOrgY );

				Visual visual = null;
				int nRet = -1;
				nRet = this.VirtualRoot.HitTest(p, out visual); //HitT

				if (nRet == 0 && visual is XmlText )  //看一下这里有没有问题
					Cursor = Cursors.IBeam;
				else if (nRet == 2)
					Cursor = Cursors.SizeWE;
				else
					Cursor = Cursors.Arrow;
			}
		}
		

		// 鼠标移动
		protected override void OnMouseMove(MouseEventArgs e)
		{
			this.DragMove(new Point(e.X, e.Y));
			base.OnMouseMove(e);
		}

		// 为拖动画线痕迹的函数
		private void DrawTracker()
		{
			Point p1 = new Point(nLastTrackerX,0);
			p1 = this.PointToScreen(p1);

			Point p2 = new Point(nLastTrackerX, this.ClientSize.Height);
			p2 = this.PointToScreen(p2);

			ControlPaint.DrawReversibleLine(p1,
				p2,
				SystemColors.Control);
		}

		
		// 卷滚鼠标,由editControl来调
		public void MyOnMouseWheel(MouseEventArgs e)
		{
			int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
			int numberOfPixelsToMove = numberOfTextLinesToMove * (int)this.FontTextDefault.GetHeight();

			DocumentOrgY += numberOfPixelsToMove;

			// base.OnMouseWheel(e);
		}


		// 客户端尺寸变化
		protected override void OnSizeChanged(System.EventArgs e)
		{
			if (this.ClientSize .Width -1 > this.DocumentWidth )
			{
				int nRetWidth,nRetHeight;
				if (this.VirtualRoot != null) 
				{
					this.VirtualRoot.Layout (0,
						0,
						this.ClientSize .Width -1,
						0,
						nTimeStampSeed++,
						out nRetWidth,
						out nRetHeight,
						LayoutMember.Layout );

					this.Invalidate ();
				}

			}

			SetScrollBars(ScrollBarMember.Both);
			DocumentOrgY = DocumentOrgY;
			DocumentOrgX = DocumentOrgX;
			base.OnSizeChanged(e);
		}
		
		// EnabledChanged
		private void XmlEditorCtrl_EnabledChanged(object sender, System.EventArgs e)
		{
			if (this.Enabled == false)
			{
				this.SetCurText(null,null);
			}
			else
			{
				this.SetCurText(this.m_selectedItem,null);
			}
		}

		// OnGetFocus
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			this.m_bFocused = true;
			this.curEdit.Focus();
		}

		// OnLostFocus
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			this.m_bFocused = false;
		}

		// 键盘按下，用上用下移动
		private void XmlEditorCtrl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			switch (e.KeyCode) 
			{
				case Keys.Up:
				{

					//得到当前的Item的上一个Item
					Item frontItem = ItemUtil.GetNearItem (this.m_selectedItem,
						MoveMember.Front );

					if (frontItem != null)
					{
						//设为当前的Item
						//this.m_selectedItem = frontItem;
						this.SetCurText(frontItem,null);
						this.SetActiveItem(frontItem);


						e.Handled = true;
						this.Invalidate();
					}
				}
					break;
				case Keys.Down:
				{

					//得到下一个Item
					Item behindItem = ItemUtil.GetNearItem (this.m_selectedItem,
						MoveMember.Behind );

					if (behindItem != null)
					{
						//this.m_selectedItem = behindItem;
						this.SetCurText(behindItem,null);
						this.SetActiveItem(behindItem);


						e.Handled = true;

						this.Invalidate();

					}

				}
					break;
				case Keys.Left:
					break;
				case Keys.Right:
					break;
				default:
					break;
			}
		}
		#endregion


        public void OnGenerateData(GenerateDataEventArgs e)
        {
            if (this.GenerateData != null)
                this.GenerateData(this, e);
        }

		#region 修改内存对象命令 及 右键菜单

		// 优化名字空
		private void menuItem_YuHua(object sender,
			System.EventArgs e)
		{ 
			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show("未选中事项");
				return;
			}

			if (!(item is ElementItem))
			{
				MessageBox.Show("当前选中节点类型不匹配，必须是ElementItem类型");
				return;
			}

			ElementItem element = (ElementItem)item;
			element.YouHua();
		}

		#region 展开收缩

		private void menuItem_ExpandChildren(object sender, EventArgs e)
		{
			this.menuItem_Expand(true,ExpandStyle.Expand);
		}
		private void menuItem_ExpandAttrs(object sender, EventArgs e)
		{
			this.menuItem_Expand(false,ExpandStyle.Expand);
		}
		private void menuItem_CollapseChildren(object sender, EventArgs e)
		{
			this.menuItem_Expand(true,ExpandStyle.Collapse);
		}
		private void menuItem_CollapseAttrs(object sender, EventArgs e)
		{
			this.menuItem_Expand(false,ExpandStyle.Collapse);
		}

		public void menuItem_Expand(bool bChildren,
			ExpandStyle expandStyle)
		{
			if (this.m_selectedItem == null)
			{
				MessageBox.Show(this, "尚未选择选择元素节点");
				return;
			}
			if (!(this.m_selectedItem is ElementItem))
			{
				MessageBox.Show(this, "请选择元素节点进行展开操作");
				return;
			}

			if (bChildren == false)
			{
				this.ExpandAttrs((ElementItem)this.m_selectedItem,
					expandStyle);
			}
			else
			{
				this.ExpandChildren((ElementItem)this.m_selectedItem,
					expandStyle);
			}
		}

		public void ExpandAttrs(ElementItem element,
			ExpandStyle expandStyle)
		{
			element.ExpandAttrsOrChildren(expandStyle,
				element.m_childrenExpand, 
				true);
		}

		public void ExpandChildren(ElementItem element,
			ExpandStyle expandStyle)
		{

			element.ExpandAttrsOrChildren(element.m_attrsExpand, 
				expandStyle,
				true);
		}



		#endregion

		#region Flush()模块

		public void Flush()
		{
			this.SetCurText(this.m_selectedItem,this.m_curText);

			if (this.m_selectedItem == null)
			{
				return;
			}

			if ((this.m_selectedItem is ElementItem))
			{
				((ElementItem)this.m_selectedItem).Flush();
			}
		}

		// 右 -- Flush
		void menuItem_Flush(object sender,
			System.EventArgs e)
		{
			this.Flush();
		}


		#endregion

		#region Item属性

		public void ShowProperties()
		{
			if (this.m_selectedItem == null)
			{
				Debug.Assert (false,"请先选择事项");
				return;
			}

			string strText = "";

			strText += "Changed=" +this.Changed.ToString() + "\r\n";


			strText += "Name=[" + Convert.ToString(this.m_selectedItem.Name)+ "]\r\n";
			if (!(this.m_selectedItem is ElementItem))
				strText += "Value=[" + this.m_selectedItem.Value + "]\r\n";

			//strText += "OuterXml=[" + Convert.ToString(this.m_selectedItem.OuterXml)+ "]\r\n\r\n";
			
			if (this.m_selectedItem is ElementItem)
			{
				ElementItem element = (ElementItem)this.m_selectedItem;

				strText += "AttrsExpand=[" + Convert.ToString(element.AttrsExpand) + "]\r\n";
				strText += "ChildrenExpand=[" + Convert.ToString(element.ChildrenExpand) + "]\r\n\r\n";
				/*
								strText += "NamespaceURI='" + element.NamespaceURI + "'\r\n\r\n";
				*/
				strText += "m_xmlAttrsTimestamp=[" + Convert.ToString(element.m_xmlAttrsTimestamp)+ "]\r\n";
				strText += "m_objAttrsTimestamp=[" + Convert.ToString(element.m_objAttrsTimestamp)+ "]\r\n\r\n";

				strText += "m_xmlChildrenTimestamp=[" + Convert.ToString(element.m_xmlChildrenTimestamp)+ "]\r\n";
				strText += "m_objChildrenTimestamp=[" + Convert.ToString(element.m_objChildrenTimestmap)+ "]\r\n\r\n";

			}

			if (this.m_selectedItem is AttrItem)
			{
				AttrItem attr = (AttrItem)this.m_selectedItem;
				strText += "NamespaceURI=" + attr.NamespaceURI + "\r\n";
			}





			PropertyDlg dlg = new PropertyDlg();
			dlg.textBox_message.Text = strText;
			dlg.ShowDialog(this);
		}
		
		// 右 -- Properties
		void menuItem_Properties(object sender,
			System.EventArgs e)
		{
			this.ShowProperties();

		}

		void menuItem_test_EnsureVisible(object sender,
			System.EventArgs e)
		{
			Item item = this.docRoot.children[this.docRoot.children.Count -1];
			Rectangle rect = new Rectangle(0,
				0,
				0,
				0);
			this.EnsureVisible(item,rect);

		}
		#endregion

		#region 插入属性部分

		// parameter:
		//		strFullName: 可以带前缀 prefix:name
		//		strURi: null 或者 空字符串 不带URI
		public int CreateAttrItemFromUI(string strFullName,
			string strURI,
			out AttrItem attr,
			out string strError)
		{
			strError = "";
			attr = null;

			int nIndex = strFullName.IndexOf(":");
			if (nIndex == 0)
			{
				strError = "元素名称'" + strFullName + "'不合法";
				return -1;
			}
			else if (nIndex > 0)
			{
				string strPrefix = strFullName.Substring(0,nIndex);
				string strLocalName = strFullName.Substring(nIndex+1);
				if (strLocalName == "")
				{
					strError = "元素名称'" + strFullName + "'不合法";
					return -1;
				}
				if (strPrefix == "xmlns")
				{
					attr = this.CreateAttrItem(strFullName);
					attr.IsNamespace = true;
					//attr.LocalName = ""
				}
				else
				{
					if (strURI != null && strURI != "")
					{
						attr =  this.CreateAttrItem(strPrefix,
							strLocalName,
							strURI);
					}
					else
					{
						attr = this.CreateAttrItem(strPrefix,
							strLocalName);
					}
				}
			}
			else
			{
				if (strURI != null && strURI != "")
				{
					strError = "属性名称'" + strFullName + "'未指定前缀";
					return -1;
				}
				attr = this.CreateAttrItem(strFullName);
			}
			return 0;
		}


		// 追加属性，带对话框
		// return:
		//		-1	error
		//		0	successed
		//		-2	取消
		public int AppendAttrWithDlg(ElementItem item,
			out string strError)
		{
			strError = "";

			AttrNameDlg dlg = new AttrNameDlg ();
			dlg.SetInfo("新属性",
				"给'" + item.Name + "'追加新属性",
				item);
			dlg.ShowDialog();
			if (dlg.DialogResult != DialogResult.OK)
				return -2;

			AttrItem attr = null;
			int nRet = this.CreateAttrItemFromUI(dlg.textBox_strElementName.Text,
				dlg.textBox_URI.Text,
				out attr,
				out strError);
			if (nRet == -1)
				return -1;
				
			attr.SetValue(dlg.textBox_value.Text);

			return item.AppendAttr(attr,
				out strError);
		}





		//1.右 -- 新属性
		private void menuItem_AppendAttr(object sender,
			System.EventArgs e)
		{ 
			if (this.m_selectedItem == null) 
			{
				MessageBox.Show(this, "尚未选择基准节点");
				return;
			}

			if ((!(this.m_selectedItem is ElementItem))
				&& (!(this.m_selectedItem is AttrItem)))
			{
				MessageBox.Show(this, "所选择的基准节点类型不正确，必须是ElementItem类型 或者 AttrItem类型");
				return;
			}

			ElementItem selected = null;

			// 要求当前选择的节点一定是ElementItem类型
			if (this.m_selectedItem is ElementItem)
				selected = (ElementItem)this.m_selectedItem;
			else if (this.m_selectedItem is AttrItem)
				selected = this.m_selectedItem.parent;

			if (selected == null)
			{
				Debug.Assert(false,"不可能的情况，前面已经判断好了");
				return;
			}

			string strError;
			int nRet = this.AppendAttrWithDlg(selected,
				out strError);
			if (nRet == -1)
				MessageBox.Show(strError);
		}


		//2.右 -- 新同级属性
		private void menuItem_InsertSiblingAttr(object sender,
			System.EventArgs e)
		{ 
			if (this.m_selectedItem == null)
			{
				Debug.Assert (false,"在'新同级属性'时，SelectedItem为null");
				return;
			}

			//是根节点没法再插入同级
			if (!(this.m_selectedItem is AttrItem))
			{
				Debug.Assert (false,"在'新同级属性'时，SelectedItem不是AttrItem类型");
				return;
			}

			AttrItem startAttr = (AttrItem)this.m_selectedItem;

			string strError;
			int nRet = InsertAttrWithDlg(startAttr,out strError);
			if (nRet == -1)
				MessageBox.Show(strError);
		}

		// return:
		//		-1	error
		//		0	successed
		//		-2	取消
		private int InsertAttrWithDlg(AttrItem startAttr,
			out string strError)
		{
			strError = "";

			ElementItem element = (ElementItem)(startAttr.parent);
			if (element == null)
			{
				strError = "InsertAttrWithDlg()，element不可能为null。";
				return -1;
			}

			AttrNameDlg dlg = new AttrNameDlg ();
			dlg.SetInfo ("新同级属性",
				"给'" + startAttr.Name + "'增加新同级属性",
				element);
			dlg.ShowDialog  ();
			if (dlg.DialogResult != DialogResult.OK )
				return -2;

			AttrItem newAttr = null;
			int nRet = this.CreateAttrItemFromUI(dlg.textBox_strElementName.Text,
				dlg.textBox_URI.Text,
				out newAttr,
				out strError);
			if (nRet == -1)
				return -1;

			newAttr.SetValue(dlg.textBox_value.Text);


			return element.InsertAttr(startAttr,
				newAttr,
				out strError);
		}


		#endregion


		#region 儿子部分

		// parameter:
		//		strFullName: 可以带前缀 prefix:name
		//		strURi: null 或者 空字符串 不带URI
		public int CreateElementItemFromUI(string strFullName,
			string strURI,
			out ElementItem element,
			out string strError)
		{
			strError = "";
			element = null;

			int nIndex = strFullName.IndexOf(":");
			if (nIndex == 0)
			{
				strError = "元素名称'" + strFullName + "'不合法";
				return -1;
			}
			else if (nIndex > 0)
			{
				string strPrefix = strFullName.Substring(0,nIndex);
				string strLocalName = strFullName.Substring(nIndex+1);
				if (strLocalName == "")
				{
					strError = "元素名称'" + strFullName + "'不合法";
					return -1;
				}
				if (strURI != null && strURI != "")
				{
					element =  this.CreateElementItem(strPrefix,
						strLocalName,
						strURI);
				}
				else
				{
					element = this.CreateElementItem(strPrefix,
						strLocalName);
				}
			}
			else
			{
				if (strURI != null && strURI != "")
				{
					strError = "元素名称'" + strFullName + "'未指定前缀";
					return -1;
				}
				element = this.CreateElementItem(strFullName);
			}
			return 0;
		}


				
		private void menuItem_AppendChild(object sender,
			System.EventArgs e)
		{ 
			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show(this,"尚未选择基准节点");
				return;
			}
			if (!(item is ElementItem))
			{
				MessageBox.Show(this,"当前节点类型不合法，必须是ElementItem类型");
				return;
			}

			ElementItem element = (ElementItem)item;

			//1.弹出"新下级元素"对话框,得到元素名
			ElementNameDlg dlg = new ElementNameDlg ();
			dlg.SetInfo ("新下级元素",
				"给'" + element.Name + "'追加新下级元素");
			dlg.ShowDialog  ();
			if (dlg.DialogResult != DialogResult.OK)
				return;

			// 3.创建节点
			string strError;
			ElementItem childItem = null;

			int nRet = this.CreateElementItemFromUI(
				dlg.textBox_strElementName.Text,
				dlg.textBox_URI.Text,
				out childItem,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(this,strError);
				return;
			}
			
			// 4.加入到父亲
			element.AutoAppendChild(childItem);	


			TextItem textItem = null;
			if (dlg.textBox_text.Text != "")
			{
				textItem = this.CreateTextItem();
				textItem.Value = dlg.textBox_text.Text;
				childItem.AutoAppendChild(textItem);  // 一个儿子带着下级，内存对象加大多少
			}
			
			return;	
		}

		// 增加下级文本
		private void menuItem_AppendText(object sender,
			System.EventArgs e)
		{ 
			Item item = this.m_selectedItem;

			if (item == null)
			{
				MessageBox.Show(this,"尚未选择基准节点");
				return;
			}
			if (!(item is ElementItem))
			{
				MessageBox.Show(this,"当前节点类型不合法，必须是ElementItem类型");
				return;
			}

			ElementItem element = (ElementItem)item;

			// 1.创建一个文本节点
			TextItem textItem = this.CreateTextItem();

			// 2.加入到父亲
			string strError;
			int nRet = element.AppendChild(textItem,
				out strError);
			if (nRet == -1)
				MessageBox.Show(this,strError);	
		}


		// return:
		//		-1	error
		//		0	successed
		//		-2	取消
		public int InsertChildWithDlg(Item startItem,
			out string strError)
		{
			strError = "";

			ElementItem myParent = this.m_selectedItem.parent ;
			if (myParent == null)
			{
				strError = "父亲为null，不可能的情况";
				return -1;
			}
			
			//1.打开"新同级"对话框，得到元素名
			ElementNameDlg dlg = new ElementNameDlg ();
			dlg.SetInfo ("新同级元素",
				"给'" + this.m_selectedItem.Name + "'元素增加新同级元素");
			dlg.ShowDialog  ();
			if (dlg.DialogResult != DialogResult.OK )
				return -2;

			// 3.创建一个元素
			ElementItem siblingItem = null;
			int nRet = this.CreateElementItemFromUI(dlg.textBox_strElementName.Text,
				dlg.textBox_URI.Text,
				out siblingItem,
				out strError);
			if (nRet == -1)
				return -1;
			
			TextItem textItem = null;
			if (dlg.textBox_text.Text != "")
			{
				textItem = this.CreateTextItem();
				textItem.SetValue(dlg.textBox_text.Text);
				siblingItem.AppendChildInternal(textItem,false,false); //一个儿子带着下级，内存对象加大多少
			}

			// 4.加到当前元素的前方
            // TODO: try
            // Exception:
            //      可能会抛出PrefixNotDefineException异常
			nRet = myParent.InsertChild(this.m_selectedItem,
				siblingItem,
				out strError);
			if (nRet == -1)
				return -1;

			return 0;
		}

        // 同级前插元素
		private void menuItem_InsertSiblingChild(object sender,
			System.EventArgs e)
		{
			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show(this,"尚未选择基准节点");
				return;
			}

			// 虚根不能插入同级
			if (item == this.VirtualRoot)
			{
				MessageBox.Show (this,"虚根节点不能插入同级元素");
				return;
			}

			//文档根节点不能插入同级
			if (item == this.docRoot)
			{
				MessageBox.Show (this,"文档根节点不能插入同级元素");
				return;
			}

			ElementItem myParent = item.parent ;
			if (myParent == null)
			{
				MessageBox.Show(this,"父亲为null，不可能的情况");
				return;
			}

            bool bInputUri = false;
            string strOldElementName = null;

            REDOINPUT:
			
			//1.打开"新同级"对话框，得到元素名
			ElementNameDlg dlg = new ElementNameDlg();
            dlg.InputUri = bInputUri;
            if (String.IsNullOrEmpty(strOldElementName) == false)
                dlg.textBox_strElementName.Text = strOldElementName;
			dlg.SetInfo ("新同级元素",
				"给'" + item.Name + "'元素增加新同级元素");
			dlg.ShowDialog();
			if (dlg.DialogResult != DialogResult.OK )
				return;

			string strError;
			// 3.创建一个元素
			ElementItem siblingItem = null;
			int nRet = this.CreateElementItemFromUI(
                dlg.textBox_strElementName.Text,
				dlg.textBox_URI.Text,
				out siblingItem,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(this,strError);
				return;
			}
			
			// 4.加到当前元素的前方
            try
            {
                nRet = myParent.InsertChild(item,
                    siblingItem,
                    out strError);
            }
            catch (PrefixNotDefineException ex) // 前缀字符串没有找到URI
            {
                MessageBox.Show(this, ex.Message);
                strOldElementName = dlg.textBox_strElementName.Text;
                bInputUri = true;   // 特别要求输入URI字符串
                goto REDOINPUT; // 要求重新输入
            }

			if (nRet == -1)
			{
				MessageBox.Show(this,strError);
				return;
			}

			TextItem textItem = null;
			if (dlg.textBox_text.Text != "")
			{
				textItem = this.CreateTextItem();
				textItem.Value = dlg.textBox_text.Text;
				siblingItem.AppendChild(textItem);
			}

			return;

		}


		private void menuItem_InsertSiblingText(object sender,
			System.EventArgs e)
		{
			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show(this,"尚未选中基准节点!");
				return;
			}

			// 虚根节点不能插入同级文本
			if (item == this.VirtualRoot)
			{
				MessageBox.Show(this,"虚根节点不能插入同级文本!");
				return;
			}

			// 文档根节点不能插入同级文本
			if (item == this.docRoot)
			{
				MessageBox.Show(this,"文档根节点不能插入同级文本!");
				return;
			}

			ElementItem myParent = item.parent ;
			if (myParent == null)
			{
				MessageBox.Show(this,"当前节点的父亲不可能为null!");
				return;
			}
				
			TextItem siblingText = this.CreateTextItem();

            // TODO: try
            // Exception:
            //      可能会抛出PrefixNotDefineException异常
            string strError;
			int nRet = myParent.InsertChild(item,
				siblingText,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(strError);
			}
		}


		#endregion

		#region 新建根元素

		public void CreateRootWithDlg()
		{
			//1.打开对话框，询问元素名
			ElementNameDlg dlg = new ElementNameDlg ();
			dlg.SetInfo ("新建根元素",
				"新建根元素");
			dlg.ShowDialog();
			if (dlg.DialogResult != DialogResult.OK )
				return;

			if (this.VirtualRoot != null)
			{
				Debug.Assert(false,"当前已存在虚根，不可能的情况");
			}

			string strXml = "";
			string strName = dlg.textBox_strElementName.Text;
			string strValue = dlg.textBox_text.Text;
			strXml = "<" + strName + ">" + strValue + "</" + strName + ">";
			this.SetXml(strXml);
		}

		//7.右 -- 新建根元素
		private void menuItem_CreateRoot(object sender,
			System.EventArgs e)
		{
			this.CreateRootWithDlg();
		}

		#endregion


		#region 删除

		


		//8.右 -- 删除
		public void menuItem_Delete()
		{
			if (this.m_selectedItem == null)
			{
				Debug.Assert (false,"在'删除'时，m_curItem为null");
				return;
			}

			if (this.m_selectedItem == this.VirtualRoot)
			{
				MessageBox.Show("不能删除虚根元素！");
				return;
			}
			/*
						if (this.m_selectedItem == this.docRoot)
						{
							MessageBox.Show("不能删除根元素！");
							return;
						}
			*/
			this.RemoveWithDlg(this.m_selectedItem);
		}
		private void menuItem_Delete(object sender,
			System.EventArgs e)
		{ 
			this.menuItem_Delete();

		}

		public void RemoveWithDlg(Item item)
		{
			string strText = "确实要删除'" 
				+ item.Name 
				+ "'节点吗?";

			if (item is AttrItem)
			{
				strText = "确实要删除'" 
					+ item.Name 
					+ "'属性节点吗?";

				AttrItem attr = (AttrItem)item;
				if (attr.IsNamespace == true)
				{
					bool bCound = attr.parent.CoundDeleteNs(attr.LocalName);
					if (bCound == false)
					{
						strText = "名字空间节点'" + attr.Name + "'正在被使用，确实要强行删除该节点吗?\r\n"
							+ "(使用该名字空间的节点将不合法!)";
					}
					else
					{
						strText += "\r\n(本属性节点是名字空间类型。)";
					}
				}
/*
                ElementItem element = (ElementItem)attr.Parent.GetItem();
                if (element != null)
                {
                    if (element.Name == "dprms:file")
                    {
                        MessageBox.Show(this, "不能删除<dp2rms:file>元素的id属性。");
                        return;
                    }
                }
*/
			}
			else if (item is ElementItem) 
			{
				ElementItem element = (ElementItem)item;

				strText = "确实要删除'" 
					+ element.Name 
					+ "'元素节点吗?";

				if (element.children.Count > 0)
				{
					if (element.children.Count == 1
						&& (element.children[0] is TextItem))
					{
						// 如果只有一个文本节点，就不提醒
					}
					else
					{
						strText += "\r\n(该元素包含下级元素)";
					}
				}
			}
			DialogResult result = MessageBox.Show(this,
				strText,
				"XmlEditor",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);

			if (result == DialogResult.No) 
				return;

			ElementItem myParent = item.parent;
			if (myParent == null)
			{

			}
			myParent.Remove(item);
		}


		#endregion

		#region 剪切，复制，粘贴

		// 右 -- 复制
		public void CopyToClipboard(Item item)
		{
			string strXml = item.OuterXml;
			Clipboard.SetDataObject(strXml);
		}

		public void menuItem_Copy()
		{

		}

		private void menuItem_Copy(object sender,
			System.EventArgs e)
		{ 
			if (this.m_selectedItem == null)
			{
				MessageBox.Show(this,"尚未选中基准节点");
				return;
			}

			this.CopyToClipboard(this.m_selectedItem);

		}

		// 右 -- 剪切

		public void CutToClipboard(Item item)
		{
			string strXml = item.OuterXml;
			Clipboard.SetDataObject(strXml);

			// ???????虚根
			if (item == this.VirtualRoot)
			{
				this.Xml = "";
				this.VirtualRoot = null;
				this.docRoot = null;
			}
			else
			{
				ElementItem myParent = item.parent;
				// 移走当前节点
				myParent.Remove(item);
			}
		}


		private void menuItem_Cut(object sender,
			System.EventArgs e)
		{ 
			if (this.m_selectedItem == null)
			{
				Debug.Assert(false,"在'剪切'时，SelectedItem为null");
				return;
			}
			this.CutToClipboard(this.m_selectedItem);

		}

		// 粘贴覆盖
		private void menuItem_PasteOver(object sender,
			System.EventArgs e)
		{ 
			IDataObject ido = Clipboard.GetDataObject();
			if (ido.GetDataPresent (DataFormats.UnicodeText) == false)
				return;
			string strInputText = (string)ido.GetData(DataFormats.UnicodeText);

			string strError;
			int nRet = this.PasteOverwrite(strInputText,
				this.m_selectedItem,
				true,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(strError);
			}
		}

		// 粘贴插入_同级前插
		private void menuItem_PasteInsert_InsertBefore(object sender,
			System.EventArgs e)
		{
			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show(this,"未选中基准节点");
				return;
			}
			IDataObject ido = Clipboard.GetDataObject();
			if (ido.GetDataPresent (DataFormats.UnicodeText) == false)
			{
				MessageBox.Show(this,"剪切板没有内容");
				return;
			}
			string strInputText = (string)ido.GetData(DataFormats.UnicodeText);


			if (item is AttrItem)
			{
				AttrItem tempAttr = this.CreateAttrItem("temp");
				item.parent.InsertAttrInternal((AttrItem)item,tempAttr,
					true,
					false);
				string strError;
				int nRet = this.PasteOverwrite(strInputText,
					tempAttr,
					false,
					out strError);
				if (nRet == -1)
				{
					MessageBox.Show(strError);
				}
			}
			else
			{
				ElementItem element = this.CreateElementItem("temp");
				if (item != this.VirtualRoot)
				{
                    // TODO: try
                    // Exception:
                    //      可能会抛出PrefixNotDefineException异常
                    item.parent.InsertChildInternal(item,
						element,
						true,
						false);
				}
				else
				{
					element = (ElementItem)item;
				}
                // 这里借用了PasteOverwrite来放入真正的元素结构
				string strError;
				int nRet = this.PasteOverwrite(strInputText,
					element,
					false,
					out strError);
				if (nRet == -1)
				{
					MessageBox.Show(strError);
				}
			}
		}

		// 粘贴插入_同级后插
		private void menuItem_PasteInsert_InsertAfter(object sender,
			System.EventArgs e)
		{
			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show(this,"未选中基准节点");
				return;
			}
			IDataObject ido = Clipboard.GetDataObject();
			if (ido.GetDataPresent (DataFormats.UnicodeText) == false)
			{
				MessageBox.Show(this,"剪切板没有内容");
				return;
			}
			string strInputText = (string)ido.GetData(DataFormats.UnicodeText);

			if (item is AttrItem)
			{
				AttrItem tempAttr = this.CreateAttrItem("temp");

				int nIndex = item.parent.attrs.IndexOf((AttrItem)item);
				if (nIndex == -1)
				{
					MessageBox.Show(this,"属性不区attrs集合，不可能的情况");
					return;
				}
				if (nIndex < item.parent.attrs.Count-1)
				{
					item.parent.InsertAttrInternal((AttrItem)item,
						tempAttr,
						true,
						false);
				}
				else
				{
					item.parent.AppendAttrInternal(tempAttr,
						true,
						false);
				}

				string strError;
				int nRet = this.PasteOverwrite(strInputText,
					tempAttr,
					false,
					out strError);
				if (nRet == -1)
				{
					MessageBox.Show(strError);
				}
			}
			else
			{
				if (item == this.VirtualRoot
					|| item == this.docRoot)
				{
					MessageBox.Show(this,"在虚根或实根上不能插入同级");
					return;
				}
				ElementItem elementTemp = this.CreateElementItem("temp");

				int nIndex = item.parent.children.IndexOf(item);
				if (nIndex == -1)
				{
					MessageBox.Show(this,"属性不区attrs集合，不可能的情况");
					return;
				}
				if (nIndex < item.parent.children.Count-1)
				{
                    // TODO: try
                    // Exception:
                    //      可能会抛出PrefixNotDefineException异常
                    item.parent.InsertChildInternal(nIndex + 1,
						elementTemp,
						true,
						false);
				}
				else
				{
					item.parent.AppendChildInternal(elementTemp,
						true,
						false);
				}


				string strError;
				int nRet = this.PasteOverwrite(strInputText,
					elementTemp,
					false,
					out strError);
				if (nRet == -1)
				{
					MessageBox.Show(strError);
				}
			}
		}

		// 粘贴插入_下级末尾
		private void menuItem_PasteInsert_AppendChild(object sender,
			System.EventArgs e)
		{

			Item item = this.m_selectedItem;
			if (item == null)
			{
				MessageBox.Show(this,"未选中基准节点");
				return;
			}
			if (!(item is ElementItem))
			{
				MessageBox.Show(this,"基准节点类型不匹配，必须是ElementItem类型");
				return;
			}

			IDataObject ido = Clipboard.GetDataObject();
			if (ido.GetDataPresent (DataFormats.UnicodeText) == false)
			{
				MessageBox.Show(this,"剪切板没有内容");
				return;
			}
			string strInputText = (string)ido.GetData(DataFormats.UnicodeText);

			ElementItem elementTemp = this.CreateElementItem("temp");

			ElementItem element = (ElementItem)item;
			element.ChildrenExpand = ExpandStyle.Expand;
			element.m_bWantChildInitial = 1;
			element.AppendChildInternal(elementTemp,true,false);

			string strError;
			int nRet = this.PasteOverwrite(strInputText,
				elementTemp,
				false,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(strError);
			}



		}

		// 用XML文本替代当前节点以及全部下级
		// parameter:
		//		strInputText	输入的即将用来粘贴的文本
		//		startItem	起始item
		public int PasteOverwrite(string strInputText,
			Item startItem,
			bool bSetFocus,
			out string strError)
		{
			strError = "";

			if (String.IsNullOrEmpty(strInputText) == true)
			{
				Debug.Assert(false,"Paste(),strInputText为null 或者 空字符串");
				strError = "Paste(),strInputText为null 或者 空字符串";
				return -1;
			}

			if (startItem == null)
			{
				this.SetXml(strInputText);
				return 0;
			}

			if (startItem == this.VirtualRoot)
			{
				this.SetXml(strInputText);
				return 0;
			}

			// 根据startItem的类型，把输入的字符串拼成xml
			string strXml = "";
			if (startItem is AttrItem)
				strXml = "<root " + strInputText + " />";
			else
				strXml = "<root>" + strInputText + "</root>";


			XmlDocument dom = new XmlDocument();
			try
			{
				dom.LoadXml(strXml);
			}
			catch(Exception ex)
			{
				strError = "paste() error,原因:" + ex.Message;
				return -1;
			}

            // item是新创建的临时元素
			ElementItem item = new ElementItem(this);

			ElementInitialStyle style = new ElementInitialStyle();
			style.attrsExpandStyle = ExpandStyle.Expand;
			style.childrenExpandStyle = ExpandStyle.Expand;
			style.bReinitial = false;

			item.Initial(dom.DocumentElement,this.allocator,style, false);  // !!!

            // myParent是要覆盖的元素的父亲
			ElementItem myParent = (ElementItem)startItem.parent;



			int nIndex = 0;
			bool bAttr = false;
			if (startItem is AttrItem)
			{
				bAttr = true;
				nIndex = myParent.attrs.IndexOf(startItem);
				Debug.Assert(nIndex != -1,"不可能的情况");

				AttrItem startAttr = (AttrItem)startItem;
				foreach(AttrItem attr in item.attrs)
				{
					myParent.InsertAttrInternal(startAttr,
						attr,
						false,
						false);
				}
				myParent.RemoveAttrInternal(startAttr,false);
			}
			else
			{
				bAttr = false;
                // 找到startItem在myParent所有儿子中的索引位置
				nIndex = myParent.children.IndexOf(startItem);
				Debug.Assert(nIndex != -1,"不可能的情况");

                // 在startItem位置前面插入item的所有儿子
				foreach(Item child in item.children)
				{
                    // TODO: try
                    // Exception:
                    //      可能会抛出PrefixNotDefineException异常
                    myParent.InsertChildInternal(startItem,
						child,
						false,
						false);
				}
                // 删除startItem
				myParent.RemoveChildInternal(startItem,false);
			}

			myParent.InitialVisual();

			int nWidth , nHeight;
			myParent.Layout(myParent.Rect.X,
				myParent.Rect.Y,
				myParent.Rect.Width,
				0,   //设为0，主要是高度变化
				this.nTimeStampSeed++,
				out nWidth,
				out nHeight,
				LayoutMember.Layout | LayoutMember.Up);


			if (bSetFocus == true)
			{
				if (bAttr == true)
				{
					Item curItem = myParent.attrs[nIndex];
					this.SetCurText(curItem,null);
					this.SetActiveItem(curItem);
				}
				else
				{
					Item curItem = myParent.children[nIndex];
					this.SetCurText(curItem,null);
					this.SetActiveItem(curItem);
				}
			}
			else
			{
				this.SetCurText(this.m_selectedItem,this.m_curText);
			}

            // 可能会改变文档根, 重设一下
            if (startItem.Parent == this.VirtualRoot)
                this.docRoot = this.GetDocRoot();   // 2006/6/22 xietao



			this.AfterDocumentChanged(ScrollBarMember.Both);
			this.Invalidate();

			// 文档发生变化
			this.FireTextChanged();

			myParent.Flush();
			return 0;
		}



		#endregion

		#region 布局
	
		//右 -- 横排布局
		private void menuItem_Horz(object sender,
			System.EventArgs e)
		{    
			this.LayoutStyle = LayoutStyle.Horizontal ;
		}

		//右 -- 竖排布局
		private void menuItem_Vert(object sender,
			System.EventArgs e)
		{    
			this.LayoutStyle = LayoutStyle.Vertical ;
		}

		#endregion

		# endregion


		#region 一些公共函数

		public Item ActiveItem
		{
			get
			{
				return this.m_selectedItem;
			}
			set
			{
				Item item = value;
				this.SetCurText(item,null);
				this.SetActiveItem(item);
				Rectangle rect = new Rectangle(0,
					0,
					0,
					0);
				this.EnsureVisible(item,rect);
			}
		}

		// 只管设m_selectedItem
		public void SetActiveItem(Item item)
		{
			Item oldItem = this.m_selectedItem;

			this.m_selectedItem = item;

			/////////////////////////////////////
			// 触发ActiveItemChanged事件
			///////////////////////////////////////
			ActiveItemChangedEventArgs args = new ActiveItemChangedEventArgs();
			args.Lastitem = oldItem;
			args.ActiveItem = this.m_selectedItem;
			args.CurText = this.m_curText;

			this.fireActiveItemChanged(this,args);
		}


		public bool Changed
		{
			get
			{
				this.SetCurText(this.m_selectedItem,this.m_curText);
				this.Flush();
				return this.m_bChanged;
			}
			set
			{
				this.InternalSetChanged(value);
			}
		}

		// 内部设bChanged的值
		private void InternalSetChanged(bool bChanged)
		{
			this.m_bChanged = bChanged;		
		}

		// 设改变
		internal void FireTextChanged()
		{
			this.InternalSetChanged(true);

			EventArgs e = new EventArgs();
			this.OnTextChanged(e);


/*
			// 触发TextChanged事件
			if (this.MyTextChanged != null)
			{
				EventArgs e = new EventArgs();
				this.MyTextChanged(this,e);
			}
*/			
		}

		public void BeginUpdate()
		{
			this.m_bAllowPaint = false;
		}

		public void EndUpdate()
		{
			this.m_bAllowPaint = true;
			this.Invalidate();
			this.Update();
		}






		#endregion

		#region 一些公共属性


		public ElementItem DocumentElement 
		{
			get 
			{
				return this.docRoot;
			}
		}

		public ElementItem GetDocRoot()
		{
			if (this.VirtualRoot == null)
				return null;

			foreach(Item item in this.VirtualRoot.children)
			{
				if (item is ElementItem)
					return (ElementItem)item;
			}
			return null;
		}

		public Font FontTextDefault
		{
			get
			{
				Debug.Assert(this.Font != null,"Form 的 Font属性不可能为null");
				return this.Font;
			}
		}



	
		[Category("Appearance")]
		[DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "Fixed3D")]
		public BorderStyle BorderStyle 
		{
			get 
			{
				return borderStyle;
			}
			set 
			{
				borderStyle = value;

				// Get Styles using Win32 calls
				int style = API.GetWindowLong(Handle, API.GWL_STYLE);
				int exStyle = API.GetWindowLong(Handle, API.GWL_EXSTYLE);

				// Modify Styles to match the selected border style
				BorderStyleToWindowStyle(ref style, ref exStyle);

				// Set Styles using Win32 calls
				API.SetWindowLong(Handle, API.GWL_STYLE, style);
				API.SetWindowLong(Handle, API.GWL_EXSTYLE, exStyle);

				// Tell Windows that the frame changed
				API.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
					API.SWP_NOACTIVATE | API.SWP_NOMOVE | API.SWP_NOSIZE |
					API.SWP_NOZORDER | API.SWP_NOOWNERZORDER |
					API.SWP_FRAMECHANGED);
			}
		}


		private void BorderStyleToWindowStyle(ref int style, ref int exStyle)
		{
			style &= ~API.WS_BORDER;
			exStyle &= ~API.WS_EX_CLIENTEDGE;
			switch(borderStyle)
			{
				case BorderStyle.Fixed3D:
					exStyle |= API.WS_EX_CLIENTEDGE;
					break;

				case BorderStyle.FixedSingle:
					style |= API.WS_BORDER;
					break;

				case BorderStyle.None:
					// No border style values
					break;
			}
		}

		//文档宽度
		public int DocumentWidth  
		{
			get 
			{
				if (this.VirtualRoot == null)
					return -1;
				return this.VirtualRoot.Rect.Width;
			}
		}

		//文档高度
		public int DocumentHeight  
		{
			get
			{
				if (this.VirtualRoot == null)
					return -1;	// /???

				return this.VirtualRoot.Rect.Height;
			}
		}

		public void SetCurText(Item item,XmlText text)
		{
			this.EditControlTextToVisual();

			if (text == null)
			{
				if (item != null
					&& (!(item is ElementItem))
					)
				{
					this.m_curText = item.GetVisualText();
				}
				else
					this.m_curText = null;
			}
			else
			{
				this.m_curText = text;
			}

			// 设当前Editor的位置
			this.SetEditPos();

			// 把当前Text的内容赋到edit里
			this.VisualTextToEditControl(); 
		}

		
		// 设数据xml
		public string Xml
		{
			get 
			{
				this.Flush();

				if (this.VirtualRoot == null)
					return "";
				return this.VirtualRoot.GetOuterXml(this.VirtualRoot);
			}
			set
			{
				SetXml(value);
			}
		}


		// 专为供Xml属性设得私有函数
		private void SetXml(string strXml)
		{
			strXml = strXml.Trim();
			if (strXml == "")
			{
				if (this.VirtualRoot != null)
				{
					this.VirtualRoot.FireTreeRemoveEvents(this.VirtualRoot.GetXPath());
				}
				this.VirtualRoot = null;
				this.docRoot = null;
				
				this.SetCurText(null,null);
				this.SetActiveItem(null);

				AfterDocumentChanged(ScrollBarMember.Both);
				this.Invalidate();

				// 文档发生变化
				this.FireTextChanged();
				return;
			}

			if (this.VirtualRoot == null)
			{
				this.VirtualRoot = new VirtualRootItem(this);
				this.VirtualRoot.LayoutStyle = this.m_layoutStyle ;
				this.VirtualRoot.m_bConnected = true;
			}
			else
			{
				this.VirtualRoot.ClearAttrs();
				this.VirtualRoot.ClearChildren();
			}


			XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
			dom.LoadXml(strXml); 

			ElementInitialStyle style = new ElementInitialStyle();
			style.attrsExpandStyle = ExpandStyle.Expand;
			style.childrenExpandStyle = ExpandStyle.Expand;
			style.bReinitial = false;

			this.VirtualRoot.Initial(dom,//dom.DocumentElement,
				allocator,
				style,
                true);

			this.docRoot = this.GetDocRoot();
				

			this.VirtualRoot.InitialVisual();

			int nWidth = 0;
			int nHeight = 0;
			this.VirtualRoot.Layout(0,
				0,
				this.ClientSize .Width -1,
				0 ,
				nTimeStampSeed++,
				out nWidth,
				out nHeight,
				LayoutMember.Layout );	

			this.SetCurText(this.VirtualRoot,null);
			this.SetActiveItem(this.VirtualRoot);


			if (this.m_bFocused == true)
				this.curEdit.Focus();


			AfterDocumentChanged(ScrollBarMember.Both);
			this.Invalidate();

			// 文档发生变化
			this.FireTextChanged();
		}


		//设样式配置文件
		public void SetCfg(VisualCfg visualCfg)
		{
			if (this.VirtualRoot == null)
				return;
			this.VisualCfg = visualCfg;
			int nRetWidth,nRetHeight;
			this.VirtualRoot.Layout (0,
				0,
				this.ClientSize .Width ,
				0,
				this.nTimeStampSeed ++,
				out nRetWidth,
				out nRetHeight,
				LayoutMember.Layout );

			this.AfterDocumentChanged(ScrollBarMember.Both);

			this.Invalidate ();
		}

		// 设排列样式
		public LayoutStyle LayoutStyle
		{
			get
			{
				return m_layoutStyle;
			}
			set
			{
				this.m_layoutStyle  = value;
				int nWidth;
				int nHeight;

				if (this.VirtualRoot == null)
					return;

				//这里有些疑问，该怎么统一处理layoutStyle
				this.VirtualRoot.LayoutStyle = this.m_layoutStyle ;

		
				this.widthList = null;
				this.widthList = new ItemWidthList ();
				
				this.VirtualRoot.Layout (0,
					0,
					this.ClientSize.Width-1 ,
					0,
					nTimeStampSeed++,
					out nWidth,
					out nHeight,
					LayoutMember.Layout );

				this.SetEditPos ();
				AfterDocumentChanged(ScrollBarMember.Both );

				this.Invalidate();
			}
		}
		
		//size样式
		public override bool AutoSize 
		{
			get 
			{
				return bAutoSize;
			}
			set 
			{
				bAutoSize = value;
				AfterDocumentChanged(ScrollBarMember.Both);
			}
		}

		
		//把visual的坐标输出到文件
		public void WriteVisualOrg(string strName)
		{
			this.VirtualRoot.WriteRect (strName);
		}
		
		# endregion

		#region 事件

		//public event EventHandler MyTextChanged;

		public event ActiveItemChangedEventHandle ActiveItemChanged;
		public void fireActiveItemChanged(object sender,
			ActiveItemChangedEventArgs args)
		{
			if (ActiveItemChanged != null)
			{
				ActiveItemChanged(sender,args);
			}
		}


		public event BeforeItemCreateEventHandle BeforeItemCreate;
		
		public event ItemCreatedEventHandle ItemCreated;
		public void fireBeforeItemCreate(object sender,
			BeforeItemCreateEventArgs args)
		{
			if (BeforeItemCreate != null)
			{
				BeforeItemCreate(sender,args);
			}
		}
		public void fireItemCreated(object sender,
			ItemCreatedEventArgs args)
		{
			if (ItemCreated != null)
			{
				ItemCreated(sender,args);
			}
		}

		public event BeforeItemTextChangeEventHandle BeforeItemTextChange;
		public event ItemTextChangedEventHandle ItemTextChanged;
		public void fireBeforeItemTextChange(object sender,
			BeforeItemTextChangeEventArgs args)
		{
			if (BeforeItemTextChange != null)
			{
				BeforeItemTextChange(sender,args);
			}
		}
		public void fireItemTextChanged(object sender,
			ItemTextChangedEventArgs args)
		{
			if (ItemTextChanged != null)
			{
				ItemTextChanged(sender,args);
			}
		}

		public event BeforeItemChangeEventHandle BeforeItemChange;
		public event ItemChangedEventHandle ItemChanged;
		public void fireBeforeItemChange(object sender,
			BeforeItemChangeEventArgs args)
		{
			if (BeforeItemChange != null)
			{
				BeforeItemChange(sender,args);
			}
		}
		public void fireItemChanged(object sender,
			ItemChangedEventArgs args)
		{
			if (ItemChanged != null)
			{
				ItemChanged(sender,args);
			}
		}

		public event BeforeItemDeleteEventHandle BeforeItemDelete;
		public event ItemDeletedEventHandle ItemDeleted;
		public void fireBeforeItemDelete(object sender,
			BeforeItemDeleteEventArgs args)
		{
			if (BeforeItemDelete != null)
			{
				BeforeItemDelete(sender,args);
			}
		}
		public void fireItemDeleted(object sender,
			ItemDeletedEventArgs args)
		{
			if (ItemDeleted != null)
			{
				ItemDeleted(sender,args);
			}
		}


		#endregion

		#region 使用Xpath选节点

/*
		public virtual XPathNavigator CreateNavigator()
		{
			XmlEditorNavigator nav = new XmlEditorNavigator(this);
			return nav;
		}
*/

		#endregion

		#region 创建节点
		
		// 创建一个元素节点
		// strName: 元素名称
		// 注意本函数可以改造成创建带前缀的元素节点 strName格式为: abc:test
		// 前缀及对应URI的定义从上级节点找，如果找到，则创建成功，如果未找到，创建失败。
		public ElementItem CreateElementItem(string strName)
		{
			ElementItem item = new ElementItem(this);
			item.Name = strName;
			item.Prefix = "";
			item.LocalName = ItemUtil.GetLocalName(strName);

			// 新建的节点肯定是要初始化visual结构，并是展开状态的
			item.m_bWantAttrsInitial =1;
			item.m_bWantChildInitial = 1;
			item.AttrsExpand = ExpandStyle.Expand;
			item.ChildrenExpand = ExpandStyle.Expand;
			
			return item;
		}

		// parameter:
		//		strPrefix	前缀
		//		strName	名称
		// 说明: 自动从上级找到对应的URI，如果找不到则创建节点不成功
		public ElementItem CreateElementItem(string strPrefix,
			string strName)
		{
			Debug.Assert(strPrefix != null,"CreateElementItem(),strPrefix参数不能为null");
			Debug.Assert((strName.IndexOf(":") == -1),"CreateElementItem(),strName参数不能再含有前缀");
			
			ElementItem element =
				this.CreateElementItem(strPrefix + ":" + strName);

			element.Prefix = strPrefix;  // 前缀先加好,建立父子关系时再报错
			
			return element;
		}

		public ElementItem CreateElementItem(string strPrefix,
			string strName,
			string strNamespaceURI)
		{
			Debug.Assert(strPrefix != null,"CreateElementItem(),strPrefix参数不能为null");
			Debug.Assert(strNamespaceURI != null,"CreateElementItem(),strNamespaceURI参数不能为null");
			Debug.Assert((strName.IndexOf(":") == -1),"CreateElementItem(),strName参数不能再含有前缀");

			ElementItem element = this.CreateElementItem(
				strPrefix,
				strName);

			element.m_strTempURI = strNamespaceURI; 

			return element;
		}

		// 创建一个属性
		// 注意本函数可以改造成创建带前缀的属性节点 strName格式为: abc:test
		// 前缀及对应URI的定义从上级元素节点找，如果找到，则创建成功，如果未找到，创建失败。
		public AttrItem CreateAttrItem(string strName)
		{
			AttrItem item = new AttrItem(this);
			item.Name = strName;
			item.Prefix = "";
			item.LocalName = ItemUtil.GetLocalName(strName);

			return item;
		}

		// parameter:
		//		strPrefix	前缀
		//		strName	名称
		// 说明: 自动从上级找到对应的URI，如果找不到则创建节点不成功
		public AttrItem CreateAttrItem(string strPrefix,
			string strName)
		{
			Debug.Assert(strPrefix != null,"strPrefix参数不能为null");
			Debug.Assert((strName.IndexOf(":") == -1),"strName参数不能再含有前缀");

			if (strPrefix == null)
			{
				throw new Exception("strPrefix参数不能为null");
			}

			if (strName.IndexOf(":") != -1)
			{
				throw new Exception("strName参数不能再含有前缀");
			}


			AttrItem attr = 
				this.CreateAttrItem(strPrefix + ":" + strName);
			attr.Prefix = strPrefix;
			return attr;
		}

		public AttrItem CreateAttrItem(string strPrefix,
			string strName,
			string strNamespaceURI)
		{
			Debug.Assert(strPrefix != null,"strPrefix参数不能为null");
			Debug.Assert(strNamespaceURI != null,"strNamespaceURI参数不能为null");

			Debug.Assert((strName.IndexOf(":") == -1),"strName参数不能再含有前缀");

			if (strPrefix == null)
			{
				throw new Exception("strPrefix参数不能为null");
			}
			if (strNamespaceURI == null)
			{
				throw new Exception("strNamespaceURI参数不能为null");
			}

			if (strName.IndexOf(":") != -1)
			{
				throw new Exception("strName参数不能再含有前缀");
			}

			AttrItem attr = this.CreateAttrItem(strPrefix,
				strName);
			attr.m_strTempURI = strNamespaceURI;
			return attr;
		}
		// 创建一个文本节点
		public TextItem CreateTextItem()
		{
			TextItem item = new TextItem(this);
			return item;
		}

		// 创建一个ProcessingInstructionItem
		public ProcessingInstructionItem CreateProcessingInstructionItem(string strName,
			string strValue)
		{
			ProcessingInstructionItem item = new ProcessingInstructionItem(this);
			item.Name = strName;
			item.SetValue(strValue);
			return item;
		}

		// 创建一个DeclarationItem
		public DeclarationItem CreateDeclarationItem(string strValue)
		{
			DeclarationItem item = new DeclarationItem(this);
			item.SetValue(strValue);
			return item;
		}

		// 创建一个CommentItem
		// parameter:
		//		strValue	值
		public CommentItem CreateCommentItem(string strValue)
		{
			CommentItem item = new CommentItem(this);
			item.SetValue(strValue);
			return item;
		}

		// 创建一个CDATAItem
		// parameter:
		//		strValue	值
		public CDATAItem CreateCDATAItem(string strValue)
		{
			CDATAItem item = new CDATAItem(this);
			item.SetValue(strValue);
			return item;
		}

		// 创建一个DocumentTypeItem
		// parameter:
		//		strName	名称
		public DocumentTypeItem CreateDocumentTypeItem(string strName,
			string strValue)
		{
			DocumentTypeItem item = new DocumentTypeItem(this);
			item.Name = strName;
			item.SetValue(strValue);
			return item;
		}
		
		// 创建一个EntityReferenceItem
		// parameter:
		//		strName	名称
		public EntityReferenceItem CreateEntityReferenceItem(string strName)
		{
			EntityReferenceItem item = new EntityReferenceItem(this);
			item.Name = strName;
			return item;
		}
		

		#endregion
	}

	#region xmleditor自定义事件

	public delegate void ActiveItemChangedEventHandle(object sender,
	ActiveItemChangedEventArgs e);
	public class ActiveItemChangedEventArgs: EventArgs
	{
		public Item Lastitem = null;
		public Item ActiveItem = null;
		public XmlText CurText = null;
	}

	// 1.BeforeItemCreate
	// 
	public delegate void BeforeItemCreateEventHandle(object sender,
	BeforeItemCreateEventArgs e);
	public class BeforeItemCreateEventArgs: EventArgs
	{
		public Item item = null;
		public Item parent = null;
		public bool bInitial = false;
		public bool Cancel = false;
	}

	// 2.ItemCreated
	public delegate void ItemCreatedEventHandle(object sender,
	ItemCreatedEventArgs e);
	
	public class ItemCreatedEventArgs: EventArgs
	{
		public Item item = null;
		public bool bInitial = false;
	}

	// 3.BeforeItemTextChange
	public delegate void BeforeItemTextChangeEventHandle(object sender,
	BeforeItemTextChangeEventArgs e);
	public class BeforeItemTextChangeEventArgs: EventArgs
	{
		public Item item = null;
		public string OldText = "";
		public string NewText = "";
	}

	// 4.ItemTextChanged
	public delegate void ItemTextChangedEventHandle(object sender,
	ItemTextChangedEventArgs e);
	public class ItemTextChangedEventArgs: EventArgs
	{
		public Item item = null;
		public string OldText = "";
		public string NewText = "";
	}

	// 5.BeforeItemAttrChange
	public delegate void BeforeItemChangeEventHandle(object sender,
	BeforeItemChangeEventArgs e);
	public class BeforeItemChangeEventArgs: EventArgs
	{
		public Item item = null;
		public string OldValue = "";
		public string NewValue = "";
	}

	// 6.ItemChanged
	public delegate void ItemChangedEventHandle(object sender,
	ItemChangedEventArgs e);

	public class ItemChangedEventArgs: EventArgs
	{
		public Item item = null;  // 任何类型，用时区分
		public string OldValue = "";
		public string NewValue = "";
	}


	// 7.BeforeItemDelete
	public delegate void BeforeItemDeleteEventHandle(object sender,
	BeforeItemDeleteEventArgs e);
	public class BeforeItemDeleteEventArgs: EventArgs
	{
		public Item item = null;
	}

	// 8.ItemDeleted
	public delegate void ItemDeletedEventHandle(object sender,
	ItemDeletedEventArgs e);
	public class ItemDeletedEventArgs: EventArgs
	{
		public Item item = null;
		public string XPath = "";
		public bool RecursiveChildEvents = false;
		public bool RiseAttrsEvents = false;

	}

	#endregion

}
