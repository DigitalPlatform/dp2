using System;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;


namespace DigitalPlatform
{
    public enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }



	public struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;
	}

	public struct POINT
	{
		public int x;
		public int y;
	}

    public delegate bool CtrlEventHandler(CtrlType sig);

    // this class just wraps some Win32 stuff that we're going to use
    internal class NativeMethods
    {
    }


    public class API
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(CtrlEventHandler handler, bool add);


        public const int WM_NCLBUTTONDOWN = 0x00a1;
        public const int WM_NCHITTEST = 0x0084;

        public const int WM_DEVICECHANGE = 0x0219; //see msdn site
        public const int DBT_DEVNODES_CHANGED = 0x0007;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEREMOVALCOMPLETE = 0x8004;
        public const int DBT_DEVTYPVOLUME = 0x00000002;  

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = (LVM_FIRST + 31);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(HandleRef hwnd, out RECT lpRect);

        [DllImport("gdi32", EntryPoint = "AddFontResource")]
        public static extern int AddFontResourceA(string lpFileName);
        [DllImport("gdi32", EntryPoint = "RemoveFontResource")]
        public static extern int RemoveFontResourceA(string lpFileName);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        public const uint PM_REMOVE = 0x0001;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(ref MSG msg, 
            IntPtr hWnd, 
            uint wFilterMin,
            uint wFilterMax, 
            uint wFlag);

		/*
		 * GetWindow() Constants
		 */
		public const int GW_HWNDFIRST	= 0;
		public const int GW_HWNDLAST	= 1;
		public const int GW_HWNDNEXT	= 2;
		public const int GW_HWNDPREV	= 3;
		public const int GW_OWNER		= 4;
		public const int GW_CHILD		= 5;
		public const int GW_ENABLEDPOPUP	= 6;
		public const int GW_MAX			= 6;

        //

        /// <summary>
        /// The struct used to pass the Glass margins to the Win32 API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }
        /// <summary>
        /// The API used to extend the GLass margins into the client area
        /// </summary>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        /// <summary>
        /// Determins whether the Desktop Windows Manager is enabled
        /// and can therefore display Aero 
        /// </summary>
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        //


		[DllImport("user32.dll", CharSet=CharSet.Auto, EntryPoint="GetWindow",
			 SetLastError=true)]
		public static extern IntPtr GetWindow(
			IntPtr hwnd,
			[MarshalAs(UnmanagedType.U4)] int wFlag);

#if NO
		public static string MimeTypeFrom(string strFileName)
		{
			return MimeTypeFrom(ReadFirst256Bytes(strFileName),
				"");
		}
#endif

#if NO
        public static string MimeTypeFrom(string strFileName)
        {
            string strMime = MimeTypeFrom(ReadFirst256Bytes(strFileName),
                "");

            // 如果通过内容无法判断，则进一步用文件扩展名判断
            if (strMime == "application/octet-stream")
            {
                string strFileExtension = Path.GetExtension(strFileName).ToLower();
                if (strFileExtension == ".rar")
                    return "application/x-rar-compressed";
            }

            return strMime;
        }
#endif
        public static string GetMimeTypeFromFile(string strFileName)
        {
            return MimeTypeFrom(ReadFirst256Bytes(strFileName),
                "");
        }

		// 读取文件前256bytes
		static byte[] ReadFirst256Bytes(string strFileName)
		{
			using(FileStream fileSource = File.Open(
				strFileName,
				FileMode.Open,
				FileAccess.Read, 
				FileShare.ReadWrite))
            { 
				byte[] result = new byte[Math.Min(256, fileSource.Length)];
				fileSource.Read(result, 0, result.Length);

				return result;
			}
		}

		static byte[] ReadFirst256Bytes(Stream fileSource)
		{
			byte[] result = new byte[Math.Min(256, fileSource.Length)];
			fileSource.Read(result, 0, result.Length);

			return result;
		}

		public static string GetMimeTypeFromFile(Stream fileSource)
		{
			return API.MimeTypeFrom(ReadFirst256Bytes(fileSource),
				"");
		}

		static string MimeTypeFrom(
			byte[] dataBytes,
			string mimeProposed) 
		{
			if (dataBytes == null)
				throw new ArgumentNullException("dataBytes");
			string mimeRet = String.Empty;
			IntPtr suggestPtr = IntPtr.Zero, filePtr = IntPtr.Zero, outPtr = IntPtr.Zero;
			if (mimeProposed != null && mimeProposed.Length > 0) 
			{
				//suggestPtr = Marshal.StringToCoTaskMemUni(mimeProposed); // for your experiments ;-)
				mimeRet = mimeProposed;
			}
			int ret = FindMimeFromData(IntPtr.Zero, 
				IntPtr.Zero, 
				dataBytes, 
				dataBytes.Length,
				suggestPtr, 
				0,
				out outPtr,
				0);
			if (ret == 0 && outPtr != IntPtr.Zero) 
			{
				string value = Marshal.PtrToStringUni(outPtr);
				// 2021/6/6
				if (value == "image/pjpeg")
					return "image/jpeg";
				return value;
			}
			return mimeRet;
		}

		[DllImport("urlmon.dll", CharSet=CharSet.Auto)]
		public static extern int FindMimeFromData(IntPtr pBC,
			IntPtr pwzUrl,
			byte[] pBuffer, 
			int cbSize,
			IntPtr pwzMimeProposed,
			int dwMimeFlags,
			out IntPtr ppwzMimeOut,
			int dwReserved );

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern int GetShortPathName(
			[MarshalAs(UnmanagedType.LPTStr)]
			string path,
　			[MarshalAs(UnmanagedType.LPTStr)]
　			StringBuilder shortPath,
　			int shortPathLength);  

		[ DllImport ( "kernel32" ) ]
		public static extern int GetPrivateProfileString (
			string section ,
			string key , 
			string def , 
			StringBuilder retVal ,
			int size ,
			string filePath ) ;

		[ DllImport ( "kernel32" ) ]
		public static extern long WritePrivateProfileString (
			string 
			section ,
			string key ,
			string val ,
			string filePath ) ;


	
		/////

		// WM_???
        public const int WM_USER = 0x0400;

		public const int WM_NULL	= 0x0000;
		public const int WM_CREATE	= 0x0001;
		public const int WM_DESTROY	= 0x0002;
		public const int WM_MOVE	= 0x0003;
		public const int WM_SIZE	= 0x0005;

        public const int WM_SETFOCUS = 0x0007;
        public const int WM_KILLFOCUS = 0x0008;

		public const int WM_GETTEXTLENGTH = 0x000E;
		public const int WM_GETDLGCODE = 0x0087;
		public const int WM_HSCROLL = 0x114;
		public const int WM_VSCROLL = 0x115;


		public const int WM_KEYFIRST  =                   0x0100;
		public const int WM_KEYDOWN   =                   0x0100;
		public const int WM_KEYUP     =                   0x0101;
		public const int WM_CHAR      =                   0x0102;
		public const int WM_DEADCHAR  =                   0x0103;
		public const int WM_SYSKEYDOWN =                  0x0104;
		public const int WM_SYSKEYUP  =                   0x0105;
		public const int WM_SYSCHAR   =                   0x0106;
		public const int WM_SYSDEADCHAR =                 0x0107;
		public const int WM_UNICHAR   =                   0x0109;
		public const int WM_KEYLAST   =                   0x0109;


		//

		public const int DLGC_WANTARROWS =    0x0001;      /* Control wants arrow keys         */
		public const int DLGC_WANTTAB =       0x0002;      /* Control wants tab keys           */
		public const int DLGC_WANTALLKEYS =   0x0004;      /* Control wants all keys           */
		public const int DLGC_WANTMESSAGE =   0x0004;      /* Pass message to control          */
		public const int DLGC_HASSETSEL =     0x0008;      /* Understands EM_SETSEL message    */
		public const int DLGC_DEFPUSHBUTTON = 0x0010;      /* Default pushbutton               */
		public const int DLGC_UNDEFPUSHBUTTON =0x0020;     /* Non-default pushbutton           */
		public const int DLGC_RADIOBUTTON =   0x0040;      /* Radio button                     */
		public const int DLGC_WANTCHARS =     0x0080;      /* Want WM_CHAR messages            */
		public const int DLGC_STATIC =        0x0100;      /* Static item: don't include       */
		public const int DLGC_BUTTON =        0x2000;      /* Button item: can be checked      */



		// SendMessage
		[DllImport("user32")]
		public static extern IntPtr
			SendMessage(IntPtr hWnd, uint Msg, 
			UIntPtr wParam, IntPtr lParam);

		[DllImport("user32")]
		public static extern IntPtr
			SendMessage(IntPtr hWnd, int Msg, 
			IntPtr wParam, IntPtr lParam);

		[DllImport("user32")]
		public static extern IntPtr
			SendMessage(IntPtr hWnd, uint Msg, 
			int wParam, int lParam);
		/*
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, 
			UIntPtr wParam, IntPtr lParam);
		*/

        // PostMessage

        [DllImport("user32")]
        public static extern bool
            PostMessage(IntPtr hWnd, int Msg,
            int wParam, int lParam);

        [DllImport("user32")]
        public static extern IntPtr
    PostMessage(IntPtr hWnd, uint Msg,
    UIntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd,
            int msg,
            IntPtr wparam,
            IntPtr lparam);

		#region EM_?? 消息定义 和 Windows Edit 控件相关功能

		// EM_???
		public const int EM_GETSEL			= 0x00b0;
		public const int EM_SETSEL			= 0x00b1;
		public const int EM_LINESCROLL		= 0x00B6;
		public const int EM_SCROLLCARET		= 0x00B7;
		public const int EM_GETMODIFY		= 0x00B8;
		public const int EM_SETMODIFY		= 0x00B9;
		public const int EM_GETLINECOUNT	= 0x00BA;
		public const int EM_LINEINDEX		= 0x00bb;
		public const int EM_LINEFROMCHAR	= 0x00c9;
		public const int EM_SETTABSTOPS = 0x00CB;

		public const int EM_GETFIRSTVISIBLELINE = 0x00CE;


		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, 
			int msg, 
			int wParam, 
			int [] lParam);

		public static void SetEditTabStops(TextBox edit,
			int[] tabstops)
		{
			SendMessage(edit.Handle,
				EM_SETTABSTOPS,
				tabstops.Length,
				tabstops);
		}

		public static bool GetEditModify(TextBox edit)
		{
			if ( (int)SendMessage(edit.Handle,
				EM_GETMODIFY, 0, 0) == 0)
				return false;
			return true;
		}

		public static void SetEditModify(TextBox edit,
			bool bModified)
		{
			SendMessage(edit.Handle,
				EM_SETMODIFY, Convert.ToInt32(bModified), 0);
		}
		 
		public static int GetEditFirstVisibleLine(TextBox edit)
		{
			return (int)SendMessage(edit.Handle,
				EM_GETFIRSTVISIBLELINE, 0, 0);
		}

		// 得到edit中行总数
		public static int GetEditLines(TextBox edit)
		{
			
			/*
			// save the handle reference for the ExtToolBox
			// HandleRef hr = new HandleRef(this, edit.Handle );

			// Send the EM_LINEFROMCHAR message with the value of
			// -1 in wParam.
			// The return value is the zero-based line number 
			// of the line containing the caret.


			int nWholeLength = (int)SendMessage(edit.Handle,
				WM_GETTEXTLENGTH, 0, 0);

			return (int)SendMessage(edit.Handle,EM_LINEFROMCHAR, nWholeLength, 0) + 1;
			*/

			return (int)SendMessage(edit.Handle, EM_GETLINECOUNT, 
				UIntPtr.Zero, IntPtr.Zero);

		}


		public static void SetEditCurrentCaretPos(
			TextBox edit,
			int x,
			int y,
			bool bScrollIntoView)
		{
			int nStart =  (int)SendMessage(edit.Handle,
				EM_LINEINDEX, 
				new UIntPtr((uint)y),
				IntPtr.Zero);
				
			SendMessage(edit.Handle,
				EM_SETSEL, 
				nStart + x,
				nStart + x);

			if (bScrollIntoView == true) 
			{
				SendMessage(edit.Handle,
					EM_SCROLLCARET, 
					0,
					0);
			}
			
		}


		// 得到edit caret当前行列位置
		public static void GetEditCurrentCaretPos(
			TextBox edit,
			out int x,
			out int y)
		{
			// save the handle reference for the ExtToolBox
			// HandleRef hr = new HandleRef(this, edit.Handle );

			// Send the EM_LINEFROMCHAR message with the value of
			// -1 in wParam.
			// The return value is the zero-based line number 
			// of the line containing the caret.

			int l = (int)SendMessage(edit.Handle, EM_LINEFROMCHAR, new UIntPtr(0xffffffff), IntPtr.Zero);
			// Send the EM_GETSEL message to the ToolBox control.
			// The low-order word of the return value is the
			// character position of the caret relative to the
			// first character in the ToolBox control,
			// i.e. the absolute character index.
			int sel = (int)SendMessage(edit.Handle, EM_GETSEL,UIntPtr.Zero, IntPtr.Zero);
			// get the low-order word from sel
			int ai  = sel & 0xffff; 
			// Send the EM_LINEINDEX message with the value of -1
			// in wParam.
			// The return value is the number of characters that
			// precede the first character in the line containing
			// the caret.
			int li = (int)SendMessage(edit.Handle, EM_LINEINDEX, new UIntPtr(0xffffffff), IntPtr.Zero);
			// Subtract the li (line index) from the ai
			// (absolute character index),
			// The result is the column number of the caret position
			// in the line containing the caret.
			int c = ai - li;

			x = c;
			y = l;
			// cpt = new CharPoint(l+1,c+1);
		}

		#endregion


		// ////////////////////////
		public const int SW_SCROLLCHILDREN =  0x0001;  /* Scroll children within *lprcScroll. */
		public const int SW_INVALIDATE = 0x0002;  /* Invalidate after scrolling */
		public const int SW_ERASE = 0x0004;  /* If SW_INVALIDATE, don't send WM_ERASEBACKGROUND */
		public const int SW_SMOOTHSCROLL = 0x0010;  /* Use smooth scrolling */

		[DllImport("user32")]
		public static extern int ScrollWindowEx(IntPtr hwnd,
			int dx,
			int dy,
			ref RECT lprcScroll,
			IntPtr lprcClip,
			int hrgnUpdate,
			IntPtr lprcUpdate,
			int fuScroll);

		// //////////////////////////

		public const int WS_VSCROLL = 0x00200000;
		public const int WS_HSCROLL = 0x00100000;
		public const int WS_BORDER = 0x00800000;
		public const int WS_POPUP = unchecked((int)0x80000000);
		public const int WS_CHILD = 0x40000000;
		public const int WS_TABSTOP = 0x00010000;

		public const int WS_EX_CLIENTEDGE = 0x00000200;
		public const int WS_EX_TOOLWINDOW = 0x00000080;

        public const UInt32 WS_MINIMIZE = 0x20000000;
        public const UInt32 WS_MAXIMIZE = 0x1000000;

		/*
		[DllImport("user32.dll")]
		static public extern int GetScrollPos(System.IntPtr hWnd, 
			int nBar);
		*/


		public const int SB_HORZ = 0;
		public const int SB_VERT = 1;
		public const int SB_CTL = 2;
		public const int SB_BOTH = 3;

        public const int SB_LINEUP = 0;
		public const int SB_LINEDOWN = 1;
		public const int SB_PAGEUP = 2;
		public const int SB_PAGEDOWN = 3;
		public const int SB_TOP = 6;
		public const int SB_BOTTOM = 7;


		public const int SB_LINELEFT = 0;
		public const int SB_LINERIGHT = 1;
		public const int SB_PAGELEFT = 2;
		public const int SB_PAGERIGHT = 3;
		public const int SB_THUMBPOSITION = 4;
		public const int SB_THUMBTRACK = 5;
		public const int SB_LEFT = 6;
		public const int SB_RIGHT = 7;
		public const int SB_ENDSCROLL = 8;

		public const int SIF_TRACKPOS = 0x10;
		public const int SIF_RANGE = 0x1;
		public const int SIF_POS = 0x4;
		public const int SIF_PAGE = 0x2;
		public const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

		public struct ScrollInfoStruct
		{
			public int cbSize;
			public int fMask;
			public int nMin;
			public int nMax;
			public int nPage;
			public int nPos;
			public int nTrackPos;
		}

		[DllImport("user32.dll", SetLastError=true) ]
		public static extern int GetScrollInfo(
			IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo );

		[DllImport("user32.dll", SetLastError=true) ]
		public static extern int SetScrollInfo(
			IntPtr hWnd, int fnBar, ref ScrollInfoStruct lpScrollInfo, bool fRedraw );


		[DllImport("user32")]
		public static extern bool ShowScrollBar(IntPtr hWnd,
			int wBar,
			bool bShow);
        [DllImport("user32")]
        public static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);

		[DllImport("gdi32")]
		public static extern int SetWindowOrgEx(IntPtr hDC, 
			int nX,
			int nY,
			IntPtr lpPoint);

		[DllImport("gdi32")]
		public static extern int SetViewportOrgEx(IntPtr hDC, 
			int nX,
			int nY,
			IntPtr lpPoint);

        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_RBUTTONDBLCLK = 0x0206;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_MBUTTONDBLCLK = 0x0209;
		public const int MK_LBUTTON	=	0x0001;
		public const int MK_RBUTTON	=	0x0002;
		public const int MK_SHIFT	=	0x0004;
		public const int MK_CONTROL	=	0x0008;
		public const int MK_MBUTTON	=	0x0010;

		public const int MK_XBUTTON1	=	0x0020;
		public const int MK_XBUTTON2	=	0x0040;



		[DllImport("user32.dll")]
		public static extern bool GetCaretPos(ref POINT p);


		public static int HiWord(int number)
		{
			if ((number & 0x80000000) == 0x80000000)
				return (number >> 16);
			else
				return (number >> 16) & 0xffff ;
		}

		public static int LoWord(int number)
		{
			return number & 0xffff;
		}

		public static int MakeLong(int LoWord, int HiWord)
		{
			return (HiWord << 16) | (LoWord & 0xffff);
		}

		public static IntPtr MakeLParam(int LoWord, int HiWord)
		{
			return (IntPtr) ((HiWord << 16) | (LoWord & 0xffff));
		}


		///////
		///

		public const int DT_TOP                       = 0x00000000;
        public const int DT_LEFT = 0x00000000;
        public const int DT_CENTER = 0x00000001;
        public const int DT_RIGHT = 0x00000002;
        public const int DT_VCENTER = 0x00000004;
        public const int DT_BOTTOM = 0x00000008;
        public const int DT_WORDBREAK = 0x00000010;
        public const int DT_SINGLELINE = 0x00000020;
        public const int DT_EXPANDTABS = 0x00000040;
        public const int DT_TABSTOP = 0x00000080;
        public const int DT_NOCLIP = 0x00000100;
        public const int DT_EXTERNALLEADING = 0x00000200;
        public const int DT_CALCRECT = 0x00000400;
        public const int DT_NOPREFIX = 0x00000800;
        public const int DT_INTERNAL = 0x00001000;


        public const int DT_EDITCONTROL = 0x00002000;
        public const int DT_PATH_ELLIPSIS = 0x00004000;
        public const int DT_END_ELLIPSIS = 0x00008000;
        public const int DT_MODIFYSTRING = 0x00010000;
        public const int DT_RTLREADING = 0x00020000;
        public const int DT_WORD_ELLIPSIS = 0x00040000;

        public const int DT_NOFULLWIDTHCHARBREAK = 0x00080000;

        public const int DT_HIDEPREFIX = 0x00100000;
        public const int DT_PREFIXONLY = 0x00200000;

		[DllImport("user32", EntryPoint="DrawTextW", CharSet=CharSet.Unicode, ExactSpelling=true)]
		public static extern int DrawTextW(IntPtr hdc,
			string lpStr,
			int nCount,
			ref RECT lpRect,
			int wFormat); 

		[DllImport("user32", EntryPoint="DrawText")]
		public static extern int DrawTextA(IntPtr hdc,
			string lpStr,
			int nCount,
			ref RECT lpRect,
			int wFormat); 

		[DllImport("gdi32")]
		public static extern IntPtr SelectObject(IntPtr hDC,
			IntPtr hObject);

        /* Background Modes */
        public const int TRANSPARENT = 1;
        public const int OPAQUE =      2;
        public const int BKMODE_LAST = 2;

        [DllImport("gdi32")]
        public static extern int SetBkMode(IntPtr hDC,     // handle to DC
            int iBkMode);  // background mode

		//////////////////
		///

		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;

		public const uint SWP_NOSIZE   = 0x0001;
		public const uint SWP_NOMOVE   = 0x0002;
		public const uint SWP_NOZORDER   = 0x0004;
		public const uint SWP_NOREDRAW   = 0x0008;
		public const uint SWP_NOACTIVATE  = 0x0010;
		public const uint SWP_FRAMECHANGED  = 0x0020;
		public const uint SWP_SHOWWINDOW  = 0x0040;
		public const uint SWP_HIDEWINDOW  = 0x0080;
		public const uint SWP_NOCOPYBITS  = 0x0100;
		public const uint SWP_NOOWNERZORDER = 0x0200;
		public const uint SWP_NOSENDCHANGING = 0x0400;
		public const uint SWP_ASYNCWINDOWPOS = 0x4000;


		[DllImport("User32", CharSet=CharSet.Auto)]
		public static extern int GetWindowLong(IntPtr hWnd, int Index);

		[DllImport("User32", CharSet=CharSet.Auto)]
		public static extern int SetWindowLong(IntPtr hWnd, int Index, int Value);

		[DllImport("User32", ExactSpelling=true)]
		public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
			int x, int y, int cx, int cy, uint uFlags);

		[DllImport("User32", CharSet=CharSet.Auto)]
		public static extern IntPtr SetParent(IntPtr hWndChild,
			IntPtr hWndNewParent);

		[DllImport("User32", CharSet=CharSet.Auto)]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("User32", CharSet=CharSet.Auto)]
		public static extern IntPtr GetWindowThreadProcessId(
			IntPtr hWnd,
			ref int lpdwProcessId);

		[DllImport("User32", CharSet=CharSet.Auto)]
		public static extern IntPtr SetFocus(
			IntPtr hWnd);

		/*
		 * 原型
DNS_STATUS WINAPI DnsQueryConfig(
  DNS_CONFIG_TYPE Config,
  DWORD Flag,
  PWSTR pwsAdapterName,
  PVOID pReserved,
  PVOID pBuffer,
  PDWORD pBufferLength
);
		*/
		[DllImport("dnsapi.dll", CharSet=CharSet.Auto)]
		public static extern int DnsQueryConfig(int Config,
			int Flag,
			string pwsAdapterName,
			int pReserved,
			byte[] pBuffer,
			ref int BufferLength);

		public static int GetLocalDnsConfig(out IPAddress[] ips,
			out string strError)
		{
/*
 * 
 * winerror.h
 * #define ERROR_INVALID_PARAMETER          87L    // dderror
 * 
 */

			ips = null;
			strError = "";

			int nLength = 300;
			int nRet = DnsQueryConfig(6, 0, null, 0, null, ref nLength);
			if (nRet != 0)
			{
				strError = "DnsQueryConfig() fail " + Convert.ToString(nRet);
				return -1;
			}
            byte [] buffer = new byte[nLength];

            nRet = DnsQueryConfig(6, 0, null, 0, buffer, ref nLength);
			if (nRet != 0)
			{
				strError = "DnsQueryConfig() fail ..." + Convert.ToString(nRet);
				return -1;
			}


            BinaryReader reader = new BinaryReader(new MemoryStream(buffer));
            int nCount = reader.ReadInt32();

			ips= new IPAddress[nCount];
			for(int i=0;i<nCount;i++)
			{
				ips[i] = new IPAddress(long.Parse(reader.ReadUInt32().ToString()));

			}

			reader.Close();
			return ips.Length;
		}

		/*
		[DllImport("kernel32.dll", SetLastError=true)]
		static extern int LCMapStringA(int Locale, 
			int dwMapFlags,
			string lpSrcStr, 
			int cchSrc,
			string lpDestStr,
			int cchDest);
		*/

		[Flags]
		public enum LCMap
		{
			IgnoreCase = 0x00000001,
			IgnoreNoSpace = 0x00000002,
			IgnoreSymbols = 0x00000004,
			IgnoreKanaType = 0x00010000,
			IgnoreWidth = 0x00020000,
			FoldCZone = 0x00000010,
			PreComposed = 0x00000020,
			Composite = 0x00000040,
			FoldDigits = 0x00000080,
			LowerCase = 0x00000100,
			UpperCase = 0x00000200,
			SortKey = 0x00000400,
			ByteRev = 0x00000800,
			ExpandLigatures = 0x00002000,
			Hiragana = 0x00100000,
			Katakana = 0x00200000,
			HalfWidth = 0x00400000,
			FullWidth = 0x00800000,
			LinguisticCasing = 0x01000000,
			SimplifiedChinese = 0x02000000,
			TraditionalChinese = 0x04000000
		}

		[DllImport("kernel32.dll",CharSet=CharSet.Auto)]
		public static extern int LCMapString( int lcid,
			int mapflgs,
			string src,
			int slen, 
			StringBuilder dst,
			int dlen);

		public static string GetSortKey(string strSource)
		{
			int nCapacity = strSource.Length * 2;
			StringBuilder dst = new StringBuilder( nCapacity );

			int nRet = LCMapString(0x0804,
				(int)(LCMap.SortKey | LCMap.IgnoreCase | LCMap.IgnoreWidth), 
				strSource, 
				strSource.Length, 
				dst, 
				dst.Capacity);
			dst.Length = nRet;
			// SetStringLength(ref dst);

			return dst.ToString();
		}


		public static string ChineseS2T(string strSource)
		{
			int nCapacity = strSource.Length * 2;
			StringBuilder dst = new StringBuilder( nCapacity );

			int nRet = LCMapString(0x0804,
				(int)LCMap.TraditionalChinese, 
				strSource, 
				strSource.Length, 
				dst, 
				dst.Capacity);
			dst.Length = nRet;
			// SetStringLength(ref dst);

			return dst.ToString();
		}

		static void SetStringLength(ref StringBuilder s)
		{
			for(int i=0;i<s.Length;i++)
			{
				if (s[i] == 0)
				{
					s.Length = i;
					break;
				}
			}
		}

		public static string ChineseT2S(string strSource)
		{
			int nCapacity = strSource.Length * 2;
			StringBuilder dst = new StringBuilder( nCapacity );
			dst.Length = 0;

			int nRet = LCMapString(0x0804,
				(int)LCMap.SimplifiedChinese, 
				strSource, 
				strSource.Length, 
				dst, 
				dst.Capacity);
			dst.Length = nRet;
			//SetStringLength(ref dst);

			return dst.ToString();

		}

        // 获得.net系统目录
        // http://msdn.microsoft.com/msdnmag/issues/04/04/NETMatters/default.aspx
        [DllImport("mscoree.dll")]
        public static extern int GetCORSystemDirectory(
            [MarshalAs(UnmanagedType.LPWStr)]StringBuilder pbuffer,
            int cchBuffer, ref int dwlength);
        public static string GetClrInstallationDirectory()
        {
            int MAX_PATH = 260;
            StringBuilder sb = new StringBuilder(MAX_PATH);
            GetCORSystemDirectory(sb, MAX_PATH, ref MAX_PATH);
            return sb.ToString();
        }

        // defined in winuser.h
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_CHANGECBCHAIN = 0x030D;

        public const int WM_CLOSE = 0x0010;

        public const int INVALID_HANDLE_VALUE = -1;

        [DllImport("User32.dll")]
        public static extern int 
            SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool
               ChangeClipboardChain(IntPtr hWndRemove,
                                    IntPtr hWndNewNext);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr handle,
            int nCmdShow);

/*
 * ShowWindow() Commands
 */
        public const int SW_HIDE            = 0;
        public const int SW_SHOWNORMAL      = 1;
        public const int SW_NORMAL          = 1;
        public const int SW_SHOWMINIMIZED   = 2;
        public const int SW_SHOWMAXIMIZED   = 3;
        public const int SW_MAXIMIZE        = 3;
        public const int SW_SHOWNOACTIVATE  = 4;
        public const int SW_SHOW            = 5;
        public const int SW_MINIMIZE        = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA          = 8;
        public const int SW_RESTORE         = 9;
        public const int SW_SHOWDEFAULT     = 10;
        public const int SW_FORCEMINIMIZE   = 11;
        public const int SW_MAX = 11;


        public const int WM_SHOWWINDOW = 0x0018;
/*
 * Identifiers for the WM_SHOWWINDOW message
 */
        public const uint SW_PARENTCLOSING = 1;
        public const uint SW_OTHERZOOM     = 2;
        public const uint SW_PARENTOPENING = 3;
        public const uint SW_OTHERUNZOOM   = 4;


        [DllImport("wininet.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool InternetGetCookie(
        string lpszUrlName,
        string lpszCookieName,
        StringBuilder lpszCookieData,
        [MarshalAs(UnmanagedType.U4)]
ref int lpdwSize
        );


        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(
        string lpszUrlName,
        string lpszCookieName,
        string lpszCookieData
        );


        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetOption(
        int hInternet,
        int dwOption,
        string lpBuffer,
        int dwBufferLength
        );

        public const int WM_ERASEBKGND = 0x0014;
        public const int WM_PAINT = 0x000F;
        // public const int WM_NULL = 0x0000;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern bool ValidateRect(IntPtr handle, ref RECT rect);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern uint GetMessageTime();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static public extern uint GetDoubleClickTime();


        // IME
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hwnd);

        [DllImport("imm32.dll")]
        public static extern bool ImmGetOpenStatus(IntPtr himc);

        [DllImport("imm32.dll")]
        public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);

        [DllImport("imm32.dll")]
        public static extern bool ImmGetConversionStatus(IntPtr himc, ref   int lpdw, ref   int lpdw2);

        [DllImport("imm32.dll")]
        public static extern int ImmSimulateHotKey(IntPtr hwnd, int lngHotkey);

        private const int IME_CMODE_FULLSHAPE = 0x8;
        private const int IME_CHOTKEY_SHAPE_TOGGLE = 0x11;

        // 将输入法转换为半角状态
        // 2008/6/4
        public static void SetImeHalfShape(Control control)
        {
            IntPtr hImc = ImmGetContext(control.Handle);
            //如果输入法处于打开状态
            if (ImmGetOpenStatus(hImc))
            {
                int iMode = 0;
                int iSentence = 0;
                //检索输入法信息
                bool bSuccess = ImmGetConversionStatus(hImc,
                    ref   iMode,
                    ref   iSentence);
                if (bSuccess)
                {
                    //如果是全角,转换成半角
                    if ((iMode & IME_CMODE_FULLSHAPE) > 0)
                        ImmSimulateHotKey(control.Handle, IME_CHOTKEY_SHAPE_TOGGLE);
                }
            }
        }
        
	}


    #region IProtectFocus Interface - IE7+Vista
    [ComImport, ComVisible(true)]
    [Guid("D81F90A3-8156-44F7-AD28-5ABB87003274")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProtectFocus
    {
        void AllowFocusChange([In, Out] ref bool pfAllow);
    }
    #endregion
}


