using System;
using System.Collections;
using System.Collections.Generic;  // for Registry
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

using Microsoft.Win32;

namespace DigitalPlatform.GUI
{

	// 增补菜单
	public delegate void GuiAppendMenuEventHandle(object sender,
    GuiAppendMenuEventArgs e);

	public class GuiAppendMenuEventArgs: EventArgs
	{
		public ContextMenu ContextMenu = null;
        public ContextMenuStrip ContextMenuStrip = null;
	}

    public class GuiUtil
    {

        // http://stackoverflow.com/questions/4842160/auto-width-of-comboboxs-content
        // 获得 ComboBox 列表事项的最大宽度
        public static int GetComboBoxMaxItemWidth(ComboBox cb)
        {
            int maxWidth = 0, temp = 0;
            foreach (string s in cb.Items)
            {
                temp = TextRenderer.MeasureText(s, cb.Font).Width;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            return maxWidth + SystemInformation.VerticalScrollBarWidth;
        }

        public static float GetSplitterState(SplitContainer container)
        {
            float fValue = (float)container.SplitterDistance /
    (
    container.Orientation == Orientation.Horizontal ?
    (float)container.Height
    :
    (float)container.Width
    )
    ;

            return fValue;
        }

        public static void SetSplitterState(SplitContainer container,
            float fValue)
        {
            try
            {
                container.SplitterDistance = (int)Math.Ceiling(
                (
                container.Orientation == Orientation.Horizontal ?
                (float)container.Height
                :
                (float)container.Width
                )
                * fValue);
            }
            catch
            {
            }
        }


        // 注册IE9 WebControl模式
        public static bool RegisterIE9DocMode()
        {
            try
            {
                // "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION"
                // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION
                RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION", true);
                try
                {
                    if (key == null)
                        key = Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");

                    if (key == null)
                        return false;

                    key.SetValue(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName, 9999, RegistryValueKind.DWord);
                    return true;
                }
                finally
                {
                    if (key != null)
                        key.Close();
                }
            }
            catch (Exception /*ex*/)
            {
                return false;
            }
        }

        public static Font GetDefaultFont()
        {
            try
            {
                FontFamily family = new FontFamily("微软雅黑");
            }
            catch
            {
                return null;
            }

            return new Font(new FontFamily("微软雅黑"), (float)9.0, GraphicsUnit.Point);
        }

        static bool IsFontExist(string strFontName)
        {
            try
            {
                FontFamily family = new FontFamily(strFontName);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获得缺省的编辑器字体名
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultEditorFontName()
        {
            if (IsFontExist("Consolas") == true)
                return "Consolas";
            if (IsFontExist("微软雅黑") == true)
                return "微软雅黑";

            return "宋体";
        }

        public static void AutoSetDefaultFont(Control control)
        {
            Font font = GetDefaultFont();
            if (font == null)
                return;

            SetControlFont(control,
                font,
                false);
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
            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            { }
            else
                control.Font = font;

            ChangeDifferentFaceFont(control, font);
        }

        // TODO: 是否可以保持原有字体比例?
        static void ChangeDifferentFaceFont(Control parent,
            Font font)
        {
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                Font subfont = sub.Font;
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

                // 递归
                ChangeDifferentFaceFont(sub, font);
            }
        }

        // 修改一个控件的字体
        static void ChangeFont(Font font,
            Control item)
        {
            Font subfont = item.Font;
            double ratio = (double)subfont.SizeInPoints / (double)font.SizeInPoints;
            if (subfont.Name != font.Name
                || subfont.SizeInPoints != font.SizeInPoints)
            {
                // item.Font = new Font(font, subfont.Style);
                item.Font = new Font(font.FontFamily, (float)((double)font.SizeInPoints * ratio), subfont.Style, GraphicsUnit.Point);
            }
        }

        static void ChangeDifferentFaceFont(ToolStrip tool,
    Font font)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            for (int i = 0; i < tool.Items.Count; i++)
            {
                ToolStripItem item = tool.Items[i];

                Font subfont = item.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    // item.Font = new Font(font, subfont.Style);
                    item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
                }
            }
        }

        public static RectangleF PaddingRect(
    Padding padding,
    RectangleF rect)
        {
            return PaddingRect(
                padding,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height);
        }

        public static Rectangle PaddingRect(
Padding padding,
Rectangle rect)
        {
            return PaddingRect(
                padding,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height);
        }

        public static RectangleF PaddingRect(
            Padding padding,
            float x,
            float y,
            float w,
            float h)
        {
            return new RectangleF(x + padding.Left,
                y + padding.Top,
                w - padding.Horizontal,
                h - padding.Vertical);
        }

        public static Rectangle PaddingRect(
    Padding padding,
    int x,
    int y,
    int w,
    int h)
        {
            return new Rectangle(x + padding.Left,
                y + padding.Top,
                w - padding.Horizontal,
                h - padding.Vertical);
        }

        public static bool PtInRect(int x,
    int y,
    Rectangle rect)
        {
            if (x < rect.X)
                return false;
            if (x >= rect.Right)
                return false;
            if (y < rect.Y)
                return false;
            if (y >= rect.Bottom)
                return false;
            return true;
        }

        public static bool PtInRect(long x,
long y,
RectangleF rect)
        {
            if (x < rect.X)
                return false;
            if (x >= rect.Right)
                return false;
            if (y < rect.Y)
                return false;
            if (y >= rect.Bottom)
                return false;
            return true;
        }

        // 看看一个窗口句柄是不是MDI子窗口的句柄？
        // 如果是，则返回该MDI子窗口的Form对象
        public static Form IsMdiChildren(Form parent, IntPtr hwnd)
        {
            for (int i = 0; i < parent.MdiChildren.Length; i++)
            {
                if (hwnd == parent.MdiChildren[i].Handle)
                {
                    return parent.MdiChildren[i];
                }
            }
            return null;    // not found
        }

        // 创建一个表示当前打开的MDI子窗口的字符串
        public static string GetOpenedMdiWindowString(Form parent)
        {
            if (parent.ActiveMdiChild == null)
                return null;

            // 得到顶层的MDI Child
            IntPtr hwnd = parent.ActiveMdiChild.Handle;

            if (hwnd == IntPtr.Zero)
                return null;

            // 找到最底部的子窗口
            IntPtr hwndFirst = API.GetWindow(hwnd, API.GW_HWNDLAST);

            // 顺次得到字符串
            string strResult = "";
            hwnd = hwndFirst;
            for (; ; )
            {
                if (hwnd == IntPtr.Zero)
                    break;

                Form temp = IsMdiChildren(parent, hwnd);
                if (temp != null)
                {
                    // 最小化的被忽略
                    if (temp.WindowState != FormWindowState.Minimized)
                        strResult += temp.GetType().ToString() + ",";
                }

                hwnd = API.GetWindow(hwnd, API.GW_HWNDPREV);
            }

            return strResult;
        }

        // 找到tab顺序的下一个控件，但是必须是parent儿子以外的控件
        public static Control GetNextControl(
            Control dialog,
            Control start,
            Control parent,
            bool bForward)
        {
            Control next = null;

            while (true)
            {
                next = dialog.GetNextControl(start, bForward);
                if (next == null)
                    break;
                if (next.Parent != parent)
                    break;
                start = next;
            }

            return next;
        }



    }

    // 当前应用程序的前台窗口
    // 用例: MessageBox.Show(ForegroundWindow.Instance, "Displayed on top!");
    public class ForegroundWindow : IWin32Window
    {
        private static ForegroundWindow _window = new ForegroundWindow();
        private ForegroundWindow() { }

        public static IWin32Window Instance
        {
            get { return _window; }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        IntPtr IWin32Window.Handle
        {
            get
            {
                return GetForegroundWindow();
            }
        }
    }

    public class VeritalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }

}