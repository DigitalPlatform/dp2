using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalPlatform.GUI
{
    // http://blog.csdn.net/fanweiwei/article/details/1780742
    /// 
    /// avicap 的摘要说明。
    /// 
    public class showVideo
    {
        // showVideo calls
        [DllImport("avicap32.dll")]
        public static extern IntPtr capCreateCaptureWindowA(byte[] lpszWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, int nID);
        [DllImport("avicap32.dll")]
        public static extern bool capGetDriverDescriptionA(short wDriver, byte[] lpszName, int cbName, byte[] lpszVer, int cbVer);
        [DllImport("User32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);
        [DllImport("User32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, short wParam, int lParam);
        [DllImport("User32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, short wParam, FrameEventHandler lParam);
        [DllImport("User32.dll")]
        public static extern bool SendMessage(IntPtr hWnd, int wMsg, int wParam, ref BITMAPINFO lParam);
        [DllImport("User32.dll")]
        public static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        [DllImport("avicap32.dll")]
        public static extern int capGetVideoFormat(IntPtr hWnd, IntPtr psVideoFormat, int wSize);

        // Constants
        public const int WM_USER = 0x400;
        public const int WS_CHILD = 0x40000000;
        public const int WS_VISIBLE = 0x10000000;
        public const int SWP_NOMOVE = 0x2;
        public const int SWP_NOZORDER = 0x4;
        public const int WM_CAP_DRIVER_CONNECT = WM_USER + 10;
        public const int WM_CAP_DRIVER_DISCONNECT = WM_USER + 11;
        public const int WM_CAP_SET_CALLBACK_FRAME = WM_USER + 5;
        public const int WM_CAP_SET_PREVIEW = WM_USER + 50;
        public const int WM_CAP_SET_PREVIEWRATE = WM_USER + 52;
        public const int WM_CAP_SET_VIDEOFORMAT = WM_USER + 45;

        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct VIDEOHDR
        {
            [MarshalAs(UnmanagedType.I4)]
            public int lpData;
            [MarshalAs(UnmanagedType.I4)]
            public int dwBufferLength;
            [MarshalAs(UnmanagedType.I4)]
            public int dwBytesUsed;
            [MarshalAs(UnmanagedType.I4)]
            public int dwTimeCaptured;
            [MarshalAs(UnmanagedType.I4)]
            public int dwUser;
            [MarshalAs(UnmanagedType.I4)]
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] dwReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biSize;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biWidth;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biHeight;
            [MarshalAs(UnmanagedType.I2)]
            public short biPlanes;
            [MarshalAs(UnmanagedType.I2)]
            public short biBitCount;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biCompression;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biSizeImage;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biXPelsPerMeter;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biYPelsPerMeter;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biClrUsed;
            [MarshalAs(UnmanagedType.I4)]
            public Int32 biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            [MarshalAs(UnmanagedType.Struct, SizeConst = 40)]
            public BITMAPINFOHEADER bmiHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public Int32[] bmiColors;
        }

        public delegate void FrameEventHandler(IntPtr lwnd, IntPtr lpVHdr);

        // Public methods
        public static object GetStructure(IntPtr ptr, ValueType structure)
        {
            return Marshal.PtrToStructure(ptr, structure.GetType());
        }

        public static object GetStructure(int ptr, ValueType structure)
        {
            return GetStructure(new IntPtr(ptr), structure);
        }

        public static void Copy(IntPtr ptr, byte[] data)
        {
            Marshal.Copy(ptr, data, 0, data.Length);
        }

        public static void Copy(int ptr, byte[] data)
        {
            Copy(new IntPtr(ptr), data);
        }

        public static int SizeOf(object structure)
        {
            return Marshal.SizeOf(structure);
        }
    }

    //Web Camera Class
    public class WebCamera
    {
        // Constructur
        public WebCamera(IntPtr handle, int width, int height)
        {
            mControlPtr = handle;
            mWidth = width;
            mHeight = height;
        }

        // delegate for frame callback
        public delegate void RecievedFrameEventHandler(byte[] data);
        public event RecievedFrameEventHandler RecievedFrame;

        private IntPtr lwndC; // Holds the unmanaged handle of the control
        private IntPtr mControlPtr; // Holds the managed pointer of the control
        private int mWidth;
        private int mHeight;

        private showVideo.FrameEventHandler mFrameEventHandler; // Delegate instance for the frame callback - must keep alive! gc should NOT collect it

        // Close the web camera
        public void CloseWebcam()
        {
            this.capDriverDisconnect(this.lwndC);
        }

        // start the web camera
        public void StartWebCam()
        {
            byte[] lpszName = new byte[100];
            byte[] lpszVer = new byte[100];

            showVideo.capGetDriverDescriptionA(0, lpszName, 100, lpszVer, 100);
            this.lwndC = showVideo.capCreateCaptureWindowA(lpszName, showVideo.WS_VISIBLE + showVideo.WS_CHILD, 0, 0, mWidth, mHeight, mControlPtr, 0);

            if (this.capDriverConnect(this.lwndC, 0))
            {
                this.capPreviewRate(this.lwndC, 66);
                this.capPreview(this.lwndC, true);
                showVideo.BITMAPINFO bitmapinfo = new showVideo.BITMAPINFO();
                bitmapinfo.bmiHeader.biSize = showVideo.SizeOf(bitmapinfo.bmiHeader);
                bitmapinfo.bmiHeader.biWidth = 352; //  352;
                bitmapinfo.bmiHeader.biHeight = 288;    //  288;
                bitmapinfo.bmiHeader.biPlanes = 1;
                bitmapinfo.bmiHeader.biBitCount = 24;
                // this.capSetVideoFormat(this.lwndC, ref bitmapinfo, showVideo.SizeOf(bitmapinfo));
                this.mFrameEventHandler = new showVideo.FrameEventHandler(FrameCallBack);
                this.capSetCallbackOnFrame(this.lwndC, this.mFrameEventHandler);
                showVideo.SetWindowPos(this.lwndC, 0, 0, 0, mWidth, mHeight, 6);
            }
        }

        // private functions
        private bool capDriverConnect(IntPtr lwnd, short i)
        {
            return showVideo.SendMessage(lwnd, showVideo.WM_CAP_DRIVER_CONNECT, i, 0);
        }

        private bool capDriverDisconnect(IntPtr lwnd)
        {
            return showVideo.SendMessage(lwnd, showVideo.WM_CAP_DRIVER_DISCONNECT, 0, 0);
        }

        private bool capPreview(IntPtr lwnd, bool f)
        {
            return showVideo.SendMessage(lwnd, showVideo.WM_CAP_SET_PREVIEW, f, 0);
        }

        private bool capPreviewRate(IntPtr lwnd, short wMS)
        {
            return showVideo.SendMessage(lwnd, showVideo.WM_CAP_SET_PREVIEWRATE, wMS, 0);
        }

        private bool capSetCallbackOnFrame(IntPtr lwnd, showVideo.FrameEventHandler lpProc)
        {
            return showVideo.SendMessage(lwnd, showVideo.WM_CAP_SET_CALLBACK_FRAME, 0, lpProc);
        }

        private bool capSetVideoFormat(IntPtr hCapWnd, ref showVideo.BITMAPINFO BmpFormat, int CapFormatSize)
        {
            return showVideo.SendMessage(hCapWnd, showVideo.WM_CAP_SET_VIDEOFORMAT, CapFormatSize, ref BmpFormat);
        }

        /* 2025/2/24
应用程序: dp2Circulation.exe
Framework 版本: v4.0.30319
说明: 由于未经处理的异常，进程终止。
异常信息: System.AccessViolationException
   在 System.Runtime.InteropServices.Marshal.CopyToManaged(IntPtr, System.Object, Int32, Int32)
   在 DigitalPlatform.GUI.showVideo.Copy(IntPtr, Byte[])
   在 DigitalPlatform.GUI.showVideo.Copy(Int32, Byte[])
   在 DigitalPlatform.GUI.WebCamera.FrameCallBack(IntPtr, IntPtr)
   在 System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG ByRef)
   在 System.Windows.Forms.Application+ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr, Int32, Int32)
   在 System.Windows.Forms.Application+ThreadContext.RunMessageLoopInner(Int32, System.Windows.Forms.ApplicationContext)
   在 System.Windows.Forms.Application+ThreadContext.RunMessageLoop(Int32, System.Windows.Forms.ApplicationContext)
   在 dp2Circulation.Program.Main()
        * */
        private void FrameCallBack(IntPtr lwnd, IntPtr lpVHdr)
        {
            showVideo.VIDEOHDR videoHeader = new showVideo.VIDEOHDR();
            byte[] VideoData;
            videoHeader = (showVideo.VIDEOHDR)showVideo.GetStructure(lpVHdr, videoHeader);
            VideoData = new byte[videoHeader.dwBytesUsed];
            showVideo.Copy(videoHeader.lpData, VideoData);
            if (this.RecievedFrame != null)
                this.RecievedFrame(VideoData);
        }

        public Image Capture()
        {
            showVideo.SendMessage(this.lwndC, 0x41e, 0, 0);
            IDataObject obj1 = Clipboard.GetDataObject();
            if (obj1.GetDataPresent(typeof(Bitmap)))
            {
                return (Image)obj1.GetData(typeof(Bitmap));
            }

            return null;
        }
    }
}

