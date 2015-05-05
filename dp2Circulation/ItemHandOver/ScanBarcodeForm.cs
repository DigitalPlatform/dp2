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

        private void ScanBarcodeForm_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.8;
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
