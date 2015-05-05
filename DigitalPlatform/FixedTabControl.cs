using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DigitalPlatform
{
    public class FixedTabControl : TabControl
    {
        [DllImportAttribute("uxtheme.dll")]
        private static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);

        protected override void OnHandleCreated(EventArgs e)
        {
            SetWindowTheme(this.Handle, "", "");
            base.OnHandleCreated(e);
        }
    }
}




