using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform
{
    public static class ProgramUtil
    {
        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern void GetProcessDpiAwareness(IntPtr hprocess, out PROCESS_DPI_AWARENESS awareness);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        public static void SetDpiAwareness()
        {
            // Vista on up = 6
            // http://stackoverflow.com/questions/17406850/how-can-we-check-if-the-current-os-is-win8-or-blue
            if (
                Environment.OSVersion.Version.Major > 6
                || (Environment.OSVersion.Version.Major == 6
                    && Environment.OSVersion.Version.Minor >= 2)
                )
            {
                // http://stackoverflow.com/questions/32148151/setprocessdpiawareness-not-having-effect
                /*
I've been trying to disable the DPI awareness on a ClickOnce application.
I quickly found out, it is not possible to specify it in the manifest, because ClickOnce does not support asm.v3 in the manifest file.
                 * */
                try
                {
                    // https://msdn.microsoft.com/en-us/library/windows/desktop/dn302122(v=vs.85).aspx
                    var result = SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_System_DPI_Aware);
                    // var setDpiError = Marshal.GetLastWin32Error();
                }
                catch
                {

                }

#if NO
                        PROCESS_DPI_AWARENESS awareness;
                        GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out awareness);
                        var getDpiError = Marshal.GetLastWin32Error();
#endif
            }
        }
    }
}
