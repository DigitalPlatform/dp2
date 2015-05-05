using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DigitalPlatform
{
    public partial class ClipboardMonitor : Form
    {
        // for clipboard chain
        IntPtr nextClipboardViewer = (IntPtr)API.INVALID_HANDLE_VALUE;

        bool bChained = false;
        int nPrevent = 0;   // 重入防止机制

        public bool Ignore = false; // 是否(临时)禁止事件(ClipboardChanged)通知剪贴板内容变化

        public event ClipboardChangedEventHandle ClipboardChanged = null;

        public ClipboardMonitor()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // TODO: Add custom paint code here

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        public bool Chain
        {
            get
            {
                return this.bChained;
            }
            set
            {
                if (this.bChained == value)
                    return;
                if (value == true && this.Created == false)
                    this.CreateControl();

                ChainClipboard(value);
            }
        }

        void ChainClipboard(bool bOn)
        {
            if (bOn == true)
            {
                if (this.bChained == true)
                {
                    Debug.Assert(false, "已经chain上了");
                    return;
                }

                this.nPrevent++;    // 阻止第一次

                nextClipboardViewer = (IntPtr)API.SetClipboardViewer((int)
                                       this.Handle);
                if (nextClipboardViewer == this.Handle)
                {
                    Debug.Assert(false, "我自己和链条上的下一个窗口居然是同一个");
                }
                Debug.Assert(nextClipboardViewer != (IntPtr)API.INVALID_HANDLE_VALUE, "");

                this.bChained = true;

                this.nPrevent--;
            }
            else
            {
                if (this.bChained == true)
                {
                    Debug.Assert(nextClipboardViewer != (IntPtr)API.INVALID_HANDLE_VALUE);

                    API.ChangeClipboardChain(this.Handle, nextClipboardViewer);
                    this.bChained = false;
                }
                else
                {
                    Debug.Assert(false, "已经脱离chain了");
                }
            }

        }


        protected override void
                  WndProc(ref System.Windows.Forms.Message m)
        {

            switch (m.Msg)
            {
                    /*
                case API.WM_CLOSE:
                    if (this.bChained == true)
                        ChainClipboard(false);
                    break;
                     */

                case API.WM_DRAWCLIPBOARD:
                    if (this.bChained == true)
                    {
                        if (this.ClipboardChanged != null && this.Ignore == false)
                        {
                            ClipboardChangedEventArgs e = new ClipboardChangedEventArgs();
                            this.ClipboardChanged(this, e);
                        }

                        // 第一次API.SetClipboardViewer调用, 会触发这个事件, 因此需要用bChained变量来阻止这一次
                        API.SendMessage(nextClipboardViewer,
                            m.Msg,
                            m.WParam,
                            m.LParam);
                    }
                    break;

                case API.WM_CHANGECBCHAIN:
                    if (m.WParam == nextClipboardViewer)
                    {
                        nextClipboardViewer = m.LParam;
                    }
                    else
                    {
                        if (this.bChained == true)
                        {
                            API.SendMessage(nextClipboardViewer,
                                m.Msg,
                                m.WParam,
                                m.LParam);
                        }
                    }
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }

    // 剪贴板内容发生改变
    public delegate void ClipboardChangedEventHandle(object sender,
    ClipboardChangedEventArgs e);

    public class ClipboardChangedEventArgs : EventArgs
    {
    }
}
