using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.GUI;

namespace dp2Circulation
{
    /// <summary>
    /// MainForm 有关字体的功能
    /// </summary>
    public partial class MainForm
    {
        /// <summary>
        /// 缺省的字体名
        /// </summary>
        public string DefaultFontString
        {
            get
            {
                return this.AppInfo.GetString(
                    "Global",
                    "default_font",
                    "");
            }
            set
            {
                this.AppInfo.SetString(
                    "Global",
                    "default_font",
                    value);
            }
        }

        /// <summary>
        /// 缺省字体
        /// </summary>
        new public Font DefaultFont
        {
            get
            {
                string strDefaultFontString = this.DefaultFontString;
                if (String.IsNullOrEmpty(strDefaultFontString) == true)
                {
                    return GuiUtil.GetDefaultFont();    // 2015/5/8
                    // return null;
                }

                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strDefaultFontString);
            }
        }

        // parameters:
        //      bForce  是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置
        /// <summary>
        /// 设置控件字体
        /// </summary>
        /// <param name="control">控件</param>
        /// <param name="font">字体</param>
        /// <param name="bForce">是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置</param>
        public static void SetControlFontOld(Control control,
            Font font,
            bool bForce = false)
        {
            if (font == null)
            {
                if (bForce == false)
                    return;
                font = Control.DefaultFont;
            }
            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            { }
            else
            {
                Debug.Assert(font != control.Font, "");
                Font old_font = control.Font;
                control.Font = font;

#if DISPOSE_FONT
                // 2017/11/10
                if (old_font != null)
                    old_font.Dispose();
#endif
            }

            ChangeDifferentFaceFont(control, font);
        }

        // parameters:
        //      bForce  是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置
        public static void SetControlFont(Control control,
            Font font,
            bool bForce = false)
        {
            if (font == null)
            {
                if (bForce == false)
                    return;
                font = Control.DefaultFont;
            }

            /*
            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            { }
            else
                control.Font = font;

            ChangeDifferentFaceFont(control, font);
            */


            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            {
            }
            else
            {
                Debug.Assert(font != control.Font, "");

                // 新旧整体字体尺寸比例。即旧字体要变成新的，要乘以多少
                float ratio = font.SizeInPoints / control.Font.SizeInPoints;
                ChangeDifferentFaceFontEx(control, font, ratio);
            }
        }


        static void ChangeDifferentFaceFontEx(Control parent,
Font font,
float ratio = 1.0F)
        {
            // 注: 不能先修改 parent 自己的 Font。因为一旦修改，子级的 Font 全部会被连带修改，算法就无法兑现了
            // 一定要等到所子级改完，父级才能修改

            if (parent is ToolStrip)
            {
                // 修改所有事项的字体，如果字体名不一样的话
                foreach (ToolStripItem item in (parent as ToolStrip).Items)
                {
                    Font subfont = item.Font;
                    if (subfont.Name != font.Name
                        || ratio != 1.0F)
                    {
                        item.Font = new Font(font.FontFamily,
                            ratio * subfont.SizeInPoints,
                            subfont.Style,  // 保留原来的 Style
                            GraphicsUnit.Point);
                    }
                }
            }

            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                /*
                Font subfont = sub.Font;

                if (subfont.Name != font.Name
                    || ratio != 1.0F)
                {
                    subfont = new Font(font.FontFamily,
                        ratio * subfont.SizeInPoints,
                        subfont.Style,
                        GraphicsUnit.Point);
                }
                */
                // 递归
                ChangeDifferentFaceFontEx(sub, font, ratio);
            }

            // 最后修改 parent 自己的 .Font
            if (parent.Font.Name != font.Name
                || parent.Font.Style != font.Style
                || ratio != 1.0F)
            {
                parent.Font = new Font(font.FontFamily,
                    ratio * parent.Font.SizeInPoints,
                    parent.Font.Style,  // 保留原来的 Style
                    GraphicsUnit.Point);
            }
        }



        static void ChangeDifferentFaceFont(Control parent,
            Font font)
        {
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                // Font subfont = sub.Font;

#if NO
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    sub.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);


                    // sub.Font = new Font(font, subfont.Style);
                }
#endif
                ChangeFont(font, sub);

                if (sub is ToolStrip)
                {
                    ChangeDifferentFaceFont((ToolStrip)sub, font);
                }

                if (sub is SplitContainer)
                {
                    ChangeSplitContainerFont(sub as SplitContainer, font);
                }

                // 递归
                ChangeDifferentFaceFont(sub, font);
            }
        }

        static void ChangeSplitContainerFont(SplitContainer container, Font font)
        {
            ChangeFont(font, container.Panel1);

            foreach (Control control in container.Panel1.Controls)
            {
                ChangeDifferentFaceFont(control, font);
            }

            ChangeFont(font, container.Panel2);

            foreach (Control control in container.Panel2.Controls)
            {
                ChangeDifferentFaceFont(control, font);
            }
        }

        static Size GetImageScalingSize()
        {
            using (Graphics g = Program.MainForm.CreateGraphics())
            {
                int width = Convert.ToInt32(16 * (g.DpiX / 96F));
                int height = Convert.ToInt32(16 * (g.DpiY / 96F));
                return new Size(
                    Math.Max(18, width),
                    Math.Max(18, height));   // 2016/4/27
            }
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.InvalidOperationException
Message: 集合已修改；可能无法执行枚举操作。
Stack:
在 System.Collections.ArrayList.ArrayListEnumeratorSimple.MoveNext()
在 dp2Circulation.MainForm.ChangeDifferentFaceFont(ToolStrip tool, Font font)
在 dp2Circulation.MainForm.ChangeDifferentFaceFont(Control parent, Font font)
在 dp2Circulation.MainForm.SetControlFont(Control control, Font font, Boolean bForce)
在 dp2Circulation.MainForm.MenuItem_configuration_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripMenuItem.OnClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.ToolStripDropDown.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.ToolStripDropDown.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.30.6550.17227, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.1.7601 Service Pack 1
本机 MAC 地址:xxx 
操作时间 2017/12/7 13:40:23 (Thu, 07 Dec 2017 13:40:23 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
* */
        static void ChangeDifferentFaceFont(ToolStrip tool,
    Font font)
        {
#if NO
            using (Graphics g = Program.MainForm.CreateGraphics())
            {
                tool.ImageScalingSize = new Size(Convert.ToInt32(16 * (g.DpiX / 96F)),
                    Convert.ToInt32(16 * (g.DpiY / 96F)));   // 2016/4/27
            }
#endif
            tool.ImageScalingSize = GetImageScalingSize();

            // 修改所有事项的字体，如果字体名不一样的话

            // 2017/12/13 先把事项放入一个 List，避免枚举中途枚举器发生变化导致抛出异常
            List<ToolStripItem> items = new List<ToolStripItem>();
            foreach (ToolStripItem item in tool.Items)
            {
                items.Add(item);
            }

            foreach(ToolStripItem item in items)
            {
                item.ImageScaling = ToolStripItemImageScaling.SizeToFit;

                Font subfont = item.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {

                    // item.Font = new Font(font, subfont.Style);
                    item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
#if DISPOSE_FONT
                    if (subfont != null)
                    {
                        subfont.Dispose();
                        subfont = null;
                    }
#endif
                }

                if (item is ToolStripMenuItem)
                {
                    ChangeDropDownItemsFont(item as ToolStripMenuItem, font);
                }
            }
        }

        static void ChangeDropDownItemsFont(ToolStripMenuItem menu, Font font)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            foreach (ToolStripItem item in menu.DropDownItems)
            {
                item.ImageScaling = ToolStripItemImageScaling.SizeToFit;

                Font subfont = item.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {

                    // item.Font = new Font(font, subfont.Style);
                    item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
#if DISPOSE_FONT
                    if (subfont != null)
                    {
                        subfont.Dispose();
                        subfont = null;
                    }
#endif
                }

                if (item is ToolStripMenuItem)
                {
                    ChangeDropDownItemsFont(item as ToolStripMenuItem, font);
                }
            }
        }

        // 修改一个控件的字体
        static void ChangeFont(Font font,
            Control item)
        {
            Font subfont = item.Font;
            float ratio = subfont.SizeInPoints / font.SizeInPoints;
            if (subfont.Name != font.Name
                || subfont.SizeInPoints != font.SizeInPoints)
            {
                // item.Font = new Font(font, subfont.Style);
                item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
#if DISPOSE_FONT
                // 2017/11/10
                if (subfont != null)
                {
                    subfont.Dispose();
                    subfont = null;
                }
#endif
            }

        }

#if NO
        static void ChangeDifferentFaceFont(SplitContainer tool,
Font font)
        {
            ChangeFont(font, tool.Panel1);
            // 递归
            ChangeDifferentFaceFont(tool.Panel1, font);

            ChangeFont(font, tool.Panel2);

            // 递归
            ChangeDifferentFaceFont(tool.Panel2, font);
        }
#endif

    }
}
