using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SendFace
{
    class InterceptMouse
    {
        public static event MouseClickEventHandler MouseClick;

        private static LowLevelMouseProc _proc = HookCallback;

        private static IntPtr _hookID = IntPtr.Zero;

        /*
        public static void Main()
        {

            _hookID = SetHook(_proc);

            Application.Run();

            UnhookWindowsHookEx(_hookID);

        }*/

        public static void Start()
        {
            _hookID = SetHook(_proc);
        }

        public static void End()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 &&
                MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {

                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                MouseClickEventArgs e = new MouseClickEventArgs { Location = new Point(hookStruct.pt.x, hookStruct.pt.y) };
                MouseClick?.Invoke(null, e);
                if (e.Handled == true)
                    return (IntPtr)1;
                // Console.WriteLine(hookStruct.pt.x + ", " + hookStruct.pt.y);
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {

            WM_LBUTTONDOWN = 0x0201,

            WM_LBUTTONUP = 0x0202,

            WM_MOUSEMOVE = 0x0200,

            WM_MOUSEWHEEL = 0x020A,

            WM_RBUTTONDOWN = 0x0204,

            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {

            public int x;

            public int y;

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;

            public uint mouseData;

            public uint flags;

            public uint time;

            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }

    public delegate void MouseClickEventHandler(object sender,
MouseClickEventArgs e);

    /// <summary>
    /// 鼠标点按事件的参数
    /// </summary>
    public class MouseClickEventArgs : EventArgs
    {
        public Point Location { get; set; }
        public bool Handled { get; set; }
    }
}
