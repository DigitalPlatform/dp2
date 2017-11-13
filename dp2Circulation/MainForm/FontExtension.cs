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
            foreach (ToolStripItem item in tool.Items)
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
