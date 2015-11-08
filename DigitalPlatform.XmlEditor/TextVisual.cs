using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.IO;

namespace DigitalPlatform.Xml
{
	public class TextVisual : Visual
	{
		#region 成员变量

		private string strText = null;

		public bool Editable = true;

		#endregion

		#region TextVisual的属性

		// 做成属性的原因是将来随时从配置文件中找,不用m_*变量保存
		// TextVisual是基类，还派生了一些类
		public virtual string Text
		{
			get
			{
				Debug.Assert(this.strText != null,"尚未初始化111");
				return strText;
			}
			set
			{
				strText = value;
			}
		}
		public override int GetWidth()
		{
			return this.TotalRestWidth;
		}

		#endregion

		#region 关于Cfg的属性

		public Font GetFont() 
		{
			Item item = this.GetItem();
			if (item == null)
				return null;

			ItemRegion region = GetRegionName();
			return item.GetFont(region);
		}

		public Color TextColor
		{
			get
			{
				Item item = this.GetItem ();
				if (item == null)
				{
					return Color.Black ;
				}

				ItemRegion region = GetRegionName();
				if (region == ItemRegion.No )
				{
					return Color.Black ;
				}

				return item.GetTextColor  (region);
			}
		}
		#endregion

		#region 重写基类的虚函数

        // 根据传入的相对坐标，得到击中的Visual对象
        // parameters:
        //      p           传入的相对坐标
        //      retVisual   out参数，返回击中的visual
        // return:
        //      -1  坐标不在本区域
        //      0   文字区
        //      1   空白
        //      2   缝隙上
		public override int HitTest(Point p,
			out Visual retVisual)
		{
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
				&& p.Y > this.Rect.Y
				&& p.Y < this.Rect.Y + this.Rect.Height)
			{
				retVisual = this;
				return -1;
			}

			// 2.右线条空白处
			if (p.X > this.Rect.X + this.Rect.Width - this.RightResWidth
				&& p.X < this.Rect.X + this.Rect.Width
				&& p.Y > this.Rect.Y
				&& p.Y < this.Rect.Y + this.Rect.Height)
			{
				retVisual = this;
				return -1;
			}
			// 3.上线条空白处
			if (p.Y > this.Rect.Y
				&& p.Y < this.Rect.Y + this.TopResHeight
				&& p.X > this.Rect.X
				&& p.X < this.Rect.X + this.Rect.Width)
			{
				retVisual = this;
				return -1;
			}
			// 4.下线条空白处
			if (p.Y > this.Rect.Y + this.Rect.Height - this.BottomResHeight
				&& p.Y < this.Rect.Y + this.Rect.Height
				&& p.X > this.Rect.X
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



		public override int GetHeight(int nWidth)
		{
			if (this.Text == null)
				return 0;

			Item item = this.GetItem ();
            using (Graphics g = Graphics.FromHwnd(item.m_document.Handle))
            {
                SizeF size = g.MeasureString(this.Text,
                    GetFont(),
                    nWidth,
                    new StringFormat());
                int nTempHeight = (int)size.Height;
                if (nTempHeight < 0)
                    nTempHeight = 0;

                return nTempHeight + this.TotalRestHeight;
            }
		}

		public override void Paint(PaintEventArgs pe,
			int nBaseX,
			int nBaseY,
			PaintMember paintMember)
		{
			if (this.Rect.Width == 0
				|| this.Rect.Height == 0)
				return;

			Rectangle rectPaint = new Rectangle (nBaseX + this.Rect.X ,
				nBaseY + this.Rect.Y,
				this.Rect.Width, 
				this.Rect.Height);

			//背景色
			Item item = this.GetItem ();
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
                pe.Graphics.FillRectangle(brush, rectPaint);
            }

			SKIPDRAWBACK:

				//调DrawLines画边框
				if (editor != null && editor.VisualCfg == null)
				{
				}
				else
				{
					this.DrawLines(rectPaint,
						this.TopBorderHeight,
						this.BottomBorderHeight,
						this.LeftBorderWidth,
						this.RightBorderWidth,
						this.BorderColor);
				}

			//内容区域
			rectPaint = new Rectangle (nBaseX + this.Rect.X + this.LeftResWidth/*LeftBlank*/,
				nBaseY + this.Rect.Y + this.TopResHeight/*this.TopBlank*/,
				this.Rect.Width - this.TotalRestWidth/*this.LeftBlank - this.RightBlank*/,
				this.Rect.Height - this.TotalRestHeight/*this.TopBlank - this.BottomBlank*/);
			
			Font font1 = this.GetFont();
			using(Font font = new Font(font1.Name,font1.Size))
            using (Brush brush = new SolidBrush(TextColor))
            {

                pe.Graphics.DrawString(Text,
                    font,
                    brush,
                    rectPaint,
                    new StringFormat());
            }
		}

		#endregion
	}
}
