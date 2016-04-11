using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 用于扫入条码的小对话框
    /// </summary>
    public partial class ScanBarcodeForm : Form
    {
        public event ScanedEventHandler BarcodeScaned = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ScanBarcodeForm()
        {
            InitializeComponent();
        }

        private void button_scan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_barcode.Text) == true)
            {
                MessageBox.Show(this, "请输入条码号");
                goto END1;
            }

            if (this.BarcodeScaned != null)
            {
                ScanedEventArgs e1 = new ScanedEventArgs();
                e1.Barcode = this.textBox_barcode.Text;

                if (this.checkBox_autoUppercaseBarcode.Checked == true)
                    e1.Barcode = e1.Barcode.ToUpper();

                this.BarcodeScaned(this, e1);
            }

        END1:
            this.textBox_barcode.SelectAll();
            this.textBox_barcode.Focus();
        }

        private void ScanBarcodeForm_Activated(object sender, EventArgs e)
        {
            this.textBox_barcode.SelectAll();
            this.textBox_barcode.Focus();

            this.Opacity = 1.0;
        }

        /*
System.ComponentModel.Win32Exception (0x80004005): 参数错误。
   在 System.Windows.Forms.Form.UpdateLayered()
   在 System.Windows.Forms.Form.set_Opacity(Double value)
   在 dp2Circulation.ScanBarcodeForm.ScanBarcodeForm_Deactivate(Object sender, EventArgs e) 位置 c:\dp2-master\dp2\dp2Circulation\ItemHandOver\ScanBarcodeForm.cs:行号 61
   在 System.Windows.Forms.Form.OnDeactivate(EventArgs e)
   在 System.Windows.Forms.Form.set_Active(Boolean value)
   在 System.Windows.Forms.Form.WmActivate(Message& m)
   在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)
         * */
        private void ScanBarcodeForm_Deactivate(object sender, EventArgs e)
        {
            try
            {
                this.Opacity = 0.8;
            }
            catch 
            {
            }
        }
    }

    /// <summary>
    /// 扫入条码号事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ScanedEventHandler(object sender,
    ScanedEventArgs e);

    /// <summary>
    /// 扫入条码号事件的参数
    /// </summary>
    public class ScanedEventArgs : EventArgs
    {
        public string Barcode = "";
    }
}
