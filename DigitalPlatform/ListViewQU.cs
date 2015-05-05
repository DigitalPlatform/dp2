using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace DigitalPlatform.GUI
{
    public class ListViewQU : ListViewNF
    {
        int m_nInUpdate = 0;
        public bool SuppressUpdate = false; // 是否阻止中间刷新

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case API.WM_KEYDOWN:
                    
                case API.WM_HSCROLL:
                    InvokeForce();
                    break;
                case API.WM_VSCROLL:
                    InvokeForce();
                    break;
                default:
                    break;
            }
            base.DefWndProc(ref m);
        }

        void InvokeForce()
        {
            if (this.m_nInUpdate > 0 && SuppressUpdate == false)
            {
                Delegate_ForceUpdate d = new Delegate_ForceUpdate(ForceUpdate);
                this.BeginInvoke(d);
            } 
        }

        delegate void Delegate_ForceUpdate();


        public void ForceUpdate()
        {
            if (this.m_nInUpdate > 0 && SuppressUpdate == false)
            {
                this.EndUpdate();
                this.Update();  // 2012/10/2
                this.BeginUpdate();
            }
        }

        public new void BeginUpdate()
        {
            base.BeginUpdate();
            m_nInUpdate++;
        }

        public new void EndUpdate()
        {
            m_nInUpdate--;
            base.EndUpdate();
        }

        public bool InUpdate
        {
            get
            {
                if (this.m_nInUpdate > 0)
                    return true;
                return false;
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            InvokeForce();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            InvokeForce();
        }
    }
}
