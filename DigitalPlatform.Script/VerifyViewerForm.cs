using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Script
{
    public partial class VerifyViewerForm : Form
    {
        /// <summary>
        /// 停靠
        /// </summary>
        public event DoDockEventHandler DoDockEvent = null;

        public bool Docked = false;

        // public MainForm MainForm = null;

        public event LocateEventHandler Locate = null;

        public VerifyViewerForm()
        {
            InitializeComponent();
        }

        public string ResultString
        {
            get
            {
                return this.textBox_verifyResult.Text;
            }
            set
            {
                this.textBox_verifyResult.Text = value;
            }
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            DoDock(true);
        }

        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏

            /*
            this.MainForm.CurrentVerifyResultControl = this.textBox_verifyResult;
            if (bShowFixedPanel == true
                && this.MainForm.PanelFixedVisible == false)
                this.MainForm.PanelFixedVisible = true;

            this.Docked = true;
            this.Visible = false;
             * */
            if (this.DoDockEvent != null)
            {
                DoDockEventArgs e = new DoDockEventArgs();
                e.ShowFixedPanel = bShowFixedPanel;
                this.DoDockEvent(this, e);
            }
        }

        #region 防止控件泄露

        // 不会被自动 Dispose 的 子 Control，放在这里托管，避免内存泄漏
        List<Control> _freeControls = new List<Control>();

        public void AddFreeControl(Control control)
        {
            ControlExtention.AddFreeControl(_freeControls, control);
        }

        public void RemoveFreeControl(Control control)
        {
            ControlExtention.RemoveFreeControl(_freeControls, control);
        }

        public void DisposeFreeControls()
        {

            ControlExtention.DisposeFreeControls(_freeControls);
        }

        #endregion


        public void Clear()
        {
            this.TryInvoke(() =>
            {
                this.textBox_verifyResult.Text = "";
            });
        }

        public TextBox ResultControl
        {
            get
            {
                return this.textBox_verifyResult;
            }
        }

        private void textBox_verifyResult_DoubleClick(object sender, EventArgs e)
        {
            if (this.Locate == null)
                return;
            if (textBox_verifyResult.Lines.Length == 0)
                return;

            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                this.textBox_verifyResult,
                out x,
                out y);

            string strLine = "";

            try
            {
                strLine = textBox_verifyResult.Lines[y];
            }
            catch
            {
                return;
            }

            // 析出"(字段名，子字段名, 字符位置)"值

            int nRet = strLine.IndexOf("(");
            if (nRet == -1)
                return;
            strLine = strLine.Substring(nRet + 1);
            nRet = strLine.IndexOf(")");
            if (nRet != -1)
                strLine = strLine.Substring(0, nRet);
            strLine = strLine.Trim();

            LocateEventArgs e1 = new LocateEventArgs();
            e1.Location = strLine;
            this.Locate(this, e1);
        }

        // Dock 停靠以后，this.Visible == true，只能用 ResultControl
        void TryInvoke(Action method)
        {
            this.ResultControl.TryInvoke(method);
        }

        T TryGet<T>(Func<T> func)
        {
            return this.ResultControl.TryGet(func);
        }
    }

    //
    public delegate void DoDockEventHandler(object sender,
DoDockEventArgs e);

    public class DoDockEventArgs : EventArgs
    {
        public bool ShowFixedPanel = false; // [in]
    }

    // 
    public delegate void LocateEventHandler(object sender,
        LocateEventArgs e);

    public class LocateEventArgs : EventArgs
    {
        public string Location = "";
    }
}
