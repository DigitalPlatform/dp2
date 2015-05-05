using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;

/*
 http://www.codeproject.com/KB/shell/balloontipsarticle.aspx
 * 
 *	NOTE 1 : This class and logic will work only and only if the
 *		following key in the registry is set
 *		HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\EnableToolTips\
 * 
 *	NOTE 2 : There needs to be a manifest file in the output location.
 *		This is in order for the close button to work properly.
 *
 *	NOTE 3 : Needs WinXP.
 * 
*/

namespace DigitalPlatform.GUI
{

    public enum TooltipIcon : int
    {
        None,
        Info,
        Warning,
        Error
    }

    public enum BalloonAlignment
    {
        TopLeft,
        TopMiddle,
        TopRight,
        LeftMiddle,
        RightMiddle,
        BottomLeft,
        BottomMiddle,
        BottomRight,
    }

    public enum BalloonPosition
    {
        /// <summary>
        /// Positions using the exact co-ordinates.
        /// So if the co-ordinates are outside the screen,
        /// tip wont be shown.
        /// </summary>
        Absolute,

        /// <summary>
        /// Positions using the co-ordinates as a reference.
        /// Regardless of the co-ordinates, the tip will 
        /// always be shown on the screen.
        /// </summary>
        Track
    }

    public delegate void DeActivateEventHandler();

    internal class MessageTool : NativeWindow
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        public event DeActivateEventHandler DeActivate;

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {
                System.Diagnostics.Debug.WriteLine(m);
                // allow the balloon to close if clicked upon
                if (DeActivate != null)
                {
                    DeActivate();
                }
            }

            base.WndProc(ref m);
        }
    }
    /// <summary>
    /// A sample class to manipulate ballon tooltips.
    /// Windows XP balloon-tips if used properly can 
    /// be very helpful.
    /// This class creates a balloon tooltip in the form of a message.
    /// This becomes useful for showing important information 
    /// quickly to the user.
    /// For example in a data-entry form full of 
    /// controls if an error is made somewhere in entering data
    /// use this to point the bad control.
    /// This helps in a shorter learning cycle of the 
    /// application.
    /// NOTE: the difference between this and HoverBalloon class
    /// is that this can be shown on demand.
    /// </summary>
    public class MessageBalloon : IDisposable
    {
        private MessageTool m_tool = null;
        private Control m_parent;
        private TOOLINFO m_ti;

        private int m_maxWidth = 250;
        private string m_text = "FMS Balloon Tooltip Control Display Message";
        private string m_title = "FMS Balloon Tooltip Message";
        private TooltipIcon m_titleIcon = TooltipIcon.None;
        private BalloonAlignment m_align = BalloonAlignment.TopRight;
        private bool m_absPosn = false;
        private bool m_centerStem = false;

        private const string TOOLTIPS_CLASS = "tooltips_class32";
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WM_USER = 0x0400;
        private readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_NOZORDER = 0x0004;

        [DllImport("User32", SetLastError = true)]
        private static extern int SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            int uFlags);

        [DllImport("User32", SetLastError = true)]
        private static extern int GetClientRect(
            IntPtr hWnd,
            ref RECT lpRect);

        [DllImport("User32", SetLastError = true)]
        private static extern int ClientToScreen(
            IntPtr hWnd,
            ref RECT lpRect);

        [DllImport("User32", SetLastError = true)]
        private static extern int SendMessage(
            IntPtr hWnd,
            int Msg,
            int wParam,
            IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int TTS_ALWAYSTIP = 0x01;
        private const int TTS_NOPREFIX = 0x02;
        private const int TTS_BALLOON = 0x40;
        private const int TTS_CLOSE = 0x80;

        private const int TTM_TRACKPOSITION = WM_USER + 18;
        private const int TTM_SETMAXTIPWIDTH = WM_USER + 24;
        private const int TTM_TRACKACTIVATE = WM_USER + 17;
        private const int TTM_ADDTOOL = WM_USER + 50;
        private const int TTM_SETTITLE = WM_USER + 33;

        private const int TTF_IDISHWND = 0x0001;
        private const int TTF_SUBCLASS = 0x0010;
        private const int TTF_TRACK = 0x0020;
        private const int TTF_ABSOLUTE = 0x0080;
        private const int TTF_TRANSPARENT = 0x0100;
        private const int TTF_CENTERTIP = 0x0002;
        private const int TTF_PARSELINKS = 0x1000;

        [StructLayout(LayoutKind.Sequential)]
        private struct TOOLINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int uFlags;
            public IntPtr hwnd;
            public IntPtr uId;
            public RECT rect;
            public IntPtr hinst;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            [MarshalAs(UnmanagedType.U4)]
            public uint lParam;
        }

        /// <summary>
        /// Creates a new instance of the MessageBalloon.
        /// </summary>
        public MessageBalloon()
        {
            // 2011/9/23
            this.m_ti.cbSize = 0;

            m_tool = new MessageTool();
            m_tool.DeActivate += new DeActivateEventHandler(this.Hide);
        }

        /// <summary>
        /// Creates a new instance of the MessageBalloon.
        /// </summary>
        /// <param name="parent">Set the parent control which will display.</param>
        public MessageBalloon(Control parent)
        {
            m_parent = parent;
            m_tool = new MessageTool();
            m_tool.DeActivate += new DeActivateEventHandler(this.Hide);

        }

        ~MessageBalloon()
        {
            Dispose(false);
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // release managed resources if any
                }

                // release unmanaged resource
                Hide();

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            disposed = true;
        }

        [SecurityPermission(SecurityAction.LinkDemand)]
        private void CreateTool()
        {
            System.Diagnostics.Debug.Assert(
                m_parent.Handle != IntPtr.Zero,
                "parent hwnd is null", "SetToolTip");

            CreateParams cp = new CreateParams();
            cp.ClassName = TOOLTIPS_CLASS;
            cp.Style =
                WS_POPUP |
                TTS_BALLOON |
                TTS_NOPREFIX |
                TTS_ALWAYSTIP |
                TTS_CLOSE;

            // create the tool
            m_tool.CreateHandle(cp);

            // create and fill in the tool tip info
            m_ti = new TOOLINFO();
            m_ti.cbSize = Marshal.SizeOf(m_ti);

            m_ti.uFlags = TTF_TRACK |
                TTF_IDISHWND |
                TTF_TRANSPARENT |
                TTF_SUBCLASS |
                TTF_PARSELINKS;

            // absolute is used tooltip maynot be shown 
            // if coords exceed the corners of the screen
            if (m_absPosn)
            {
                m_ti.uFlags |= TTF_ABSOLUTE;
            }

            if (m_centerStem)
            {
                m_ti.uFlags |= TTF_CENTERTIP;
            }

            m_ti.uId = m_tool.Handle;
            m_ti.lpszText = m_text;
            m_ti.hwnd = m_parent.Handle;

            GetClientRect(m_parent.Handle, ref m_ti.rect);
            ClientToScreen(m_parent.Handle, ref m_ti.rect);

            // make sure we make it the top level window
            SetWindowPos(
                m_tool.Handle,
                HWND_TOPMOST,
                0, 0, 0, 0,
                SWP_NOACTIVATE |
                SWP_NOMOVE |
                SWP_NOSIZE);

            // add the tool tip
            IntPtr ptrStruct = Marshal.AllocHGlobal(Marshal.SizeOf(m_ti));  //  + 100
            Marshal.StructureToPtr(m_ti, ptrStruct, false);

            SendMessage(
                m_tool.Handle, TTM_ADDTOOL, 0, ptrStruct);

            m_ti = (TOOLINFO)Marshal.PtrToStructure(ptrStruct,
                typeof(TOOLINFO));

            SendMessage(
                m_tool.Handle, TTM_SETMAXTIPWIDTH,
                0, new IntPtr(m_maxWidth));

            IntPtr ptrTitle = Marshal.StringToHGlobalAuto(m_title);

            SendMessage(
                m_tool.Handle, TTM_SETTITLE,
                (int)m_titleIcon, ptrTitle);

            SetBalloonPosition(m_ti.rect);

            Marshal.FreeHGlobal(ptrStruct);
            Marshal.FreeHGlobal(ptrTitle);
        }

        private void SetBalloonPosition(RECT rect)
        {
            int x = 0, y = 0;

            // calculate cordinates depending upon aligment
            switch (m_align)
            {
                case BalloonAlignment.TopLeft:
                    x = rect.left;
                    y = rect.top;
                    break;
                case BalloonAlignment.TopMiddle:
                    x = rect.left + (rect.right / 2);
                    y = rect.top;
                    break;
                case BalloonAlignment.TopRight:
                    x = rect.left + rect.right;
                    y = rect.top;
                    break;
                case BalloonAlignment.LeftMiddle:
                    x = rect.left;
                    y = rect.top + (rect.bottom / 2);
                    break;
                case BalloonAlignment.RightMiddle:
                    x = rect.left + rect.right;
                    y = rect.top + (rect.bottom / 2);
                    break;
                case BalloonAlignment.BottomLeft:
                    x = rect.left;
                    y = rect.top + rect.bottom;
                    break;
                case BalloonAlignment.BottomMiddle:
                    x = rect.left + (rect.right / 2);
                    y = rect.top + rect.bottom;
                    break;
                case BalloonAlignment.BottomRight:
                    x = rect.left + rect.right;
                    y = rect.top + rect.bottom;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false, "undefined enum", "default case reached");
                    break;
            }

            //int pt = MAKELONG(ti.rect.left, ti.rect.top);
            int pt = MAKELONG(x, y);
            IntPtr ptr = new IntPtr(pt);

            SendMessage(
                m_tool.Handle, TTM_TRACKPOSITION,
                0, ptr);

        }

        /// <summary>
        /// Shows or hides the tool.
        /// </summary>
        /// <param name="show">0 to hide, -1 to show</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        private void Display(int show)
        {
            // 2011/9/23
            if (this.m_ti.cbSize == 0)
                return;

            Debug.Assert(this.m_ti.cbSize != 0, "");

            IntPtr ptrStruct = Marshal.AllocHGlobal(Marshal.SizeOf(m_ti));  //  + 100

            Marshal.StructureToPtr(m_ti, ptrStruct, false);

            SendMessage(
                m_tool.Handle, TTM_TRACKACTIVATE,
                show, ptrStruct);

            Marshal.FreeHGlobal(ptrStruct);
        }

        /// <summary>
        /// Hides the message if visible.
        /// </summary>
        public void Hide()
        {
            Display(0);
            m_tool.DestroyHandle();
        }

        private int MAKELONG(int loWord, int hiWord)
        {
            return (hiWord << 16) | (loWord & 0xffff);
        }

        /// <summary>
        /// Sets or gets the Title.
        /// </summary>
        public string Title
        {
            get
            {
                return m_title;
            }
            set
            {
                m_title = value;
            }
        }

        /// <summary>
        /// Sets or gets the display icon.
        /// </summary>
        public TooltipIcon TitleIcon
        {
            get
            {
                return m_titleIcon;
            }
            set
            {
                m_titleIcon = value;
            }
        }

        /// <summary>
        /// Sets or get the display text.
        /// </summary>
        public string Text
        {
            get
            {
                return m_text;
            }
            set
            {
                m_text = value;
            }
        }

        /// <summary>
        /// Sets or gets the parent.
        /// </summary>
        public Control Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Sets or gets the placement of the balloon.
        /// </summary>
        public BalloonAlignment Align
        {
            get
            {
                return m_align;
            }
            set
            {
                m_align = value;
            }
        }

        /// <summary>
        /// Sets or gets the positioning of the balloon.
        /// TRUE : Positions using the exact co-ordinates,
        /// if the co-ordinates are outside the screen, tip wont be shown.
        /// FALSE : Positions using the co-ordinates as a reference.
        /// Regardless of the co-ordinates, the tip will 
        /// always be shown on the screen.
        /// </summary>
        public bool UseAbsolutePositioning
        {
            get
            {
                return m_absPosn;
            }
            set
            {
                m_absPosn = value;
            }
        }

        /// <summary>
        /// Sets or gets the stem position 
        /// in the tip. 
        /// TRUE : The stem of the tip is set to center.
        /// An attempt is made to show the tip with the stem
        /// centered, if that would make the tip to be 
        /// hidden partly, stem is not centered.
        /// FALSE: Stem is not centered.
        /// </summary>
        public bool CenterStem
        {
            get
            {
                return m_centerStem;
            }
            set
            {
                m_centerStem = value;
            }
        }

        /// <summary>
        /// Show the Message in a balloon tooltip.
        /// </summary>
        public void Show()
        {
            // recreate window always
            Hide();

            CreateTool();
            Display(-1);
        }

    }
}
