using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.GUI;

namespace DigitalPlatform.GUI
{
    /// <summary>
    /// 插入事项时不会出现闪烁的ListView
    /// </summary>
    public partial class ListViewControl1 : ListView
    {
        public ListViewControl1()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // TODO: Add custom paint code here

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        bool updating = false;

        public void UpdateItem(int iIndex)
        {
            updating = true;

            this.Update();

            updating = false;
        }

        protected override void WndProc(ref Message messg)
        {
            if (updating)
            {
                // We do not want to erase the background, 
                // turn this message into a null-message
                if ((int)API.WM_ERASEBKGND == messg.Msg)
                    messg.Msg = (int)API.WM_NULL;
            }
            base.WndProc(ref messg);
        }
    }
}
