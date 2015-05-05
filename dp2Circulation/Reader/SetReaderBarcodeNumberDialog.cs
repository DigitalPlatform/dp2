using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 是否用身份证号当作证条码号 ?
    /// </summary>
    internal partial class SetReaderBarcodeNumberDialog : Form
    {
        public string InitialSelect = "no";
        const int WM_FIRST_SETFOCUS = API.WM_USER + 200;

        public SetReaderBarcodeNumberDialog()
        {
            InitializeComponent();
        }

        private void SetReaderBarcodeNumberDialog_Load(object sender, EventArgs e)
        {
            API.PostMessage(this.Handle, WM_FIRST_SETFOCUS, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_FIRST_SETFOCUS:
                    if (this.InitialSelect == "yes")
                    {
                        this.AcceptButton = this.button_yes;
                        this.button_yes.Focus();
                    }
                    else if (this.InitialSelect == "no")
                    {
                        this.AcceptButton = this.button_no;
                        this.button_no.Focus();
                    } 
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void button_yes_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.Close();
        }

        private void button_no_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            this.Close();
        }

        public bool DontAsk
        {
            get
            {
                return this.checkBox_dontAsk.Checked;
            }
            set
            {
                this.checkBox_dontAsk.Checked = value;
            }
        }
    }
}
