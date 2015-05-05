using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Runtime.InteropServices;

public partial class restart : System.Web.UI.Page
{

    [DllImport("user32.dll")]
    public static extern int ExitWindowsEx(int uFlags, int dwReason);

    protected void Page_Load(object sender, EventArgs e)
    {
        // ExitWindowsEx(2, 0);
        // System.Diagnostics.Process.Start("ShutDown", "/r");

        System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
        myProcess.StartInfo.WorkingDirectory = Request.MapPath("~/bin");
        myProcess.StartInfo.FileName = Request.MapPath("~/bin/restartWindows.bat");
        myProcess.Start();
    }
}