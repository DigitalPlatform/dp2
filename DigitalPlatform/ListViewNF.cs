using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DigitalPlatform.GUI
{
    // http://stackoverflow.com/questions/442817/c-flickering-listview-on-update
    public class ListViewNF : System.Windows.Forms.ListView
    {
        public delegate void ColumnContextMenuHandler(object sender, ColumnHeader columnHeader);
        public event ColumnContextMenuHandler ColumnContextMenuClicked = null;

        bool _OnItemsArea = false;
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _OnItemsArea = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _OnItemsArea = false;
        }

        const int WM_CONTEXTMENU = 0x007B;

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == WM_CONTEXTMENU)
            {
                if (!_OnItemsArea && this.Handle != m.WParam)
                {
                    // 水平方向
                    API.ScrollInfoStruct si = new API.ScrollInfoStruct();

                    si.cbSize = Marshal.SizeOf(si);
                    si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
                    API.GetScrollInfo(this.Handle, API.SB_HORZ, ref si);

                    Point p = base.PointToClient(MousePosition);
                    int totalWidth = 0;

                    int nColumnHeaderHeight = ListViewUtil.GetColumnHeaderHeight(this);
                    if (p.Y >= 0 && p.Y < nColumnHeaderHeight)
                    {
                        foreach (ColumnHeader column in base.Columns)
                        {

                            totalWidth += column.Width;
                            if (p.X + si.nPos < totalWidth)
                            {
                                if (ColumnContextMenuClicked != null)
                                {
                                    ColumnContextMenuClicked(this, column);
                                    return;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            base.WndProc(ref m);
        }

        public ListViewNF()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }
}
