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
    /// 从文件导入盘点，开始时搜集信息的对话框
    /// </summary>
    public partial class InventoryFromFileDialog : Form
    {
        public InventoryFromFileDialog()
        {
            InitializeComponent();

            inventoryBatchNoControl_start_batchNo.AutoScaleMode = this.AutoScaleMode;
        }

        private void InventoryFromFileDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.BatchNo) == true)
            {
                strError = "尚未指定批次号";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.BarcodeFileName) == true)
            {
                strError = "尚未指定条码号文件名";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string BarcodeFileName
        {
            get
            {
                return this.textBox_barcodeFileName.Text;
            }
            set
            {
                this.textBox_barcodeFileName.Text = value;
            }
        }

        public string BatchNo
        {
            get
            {
                return this.inventoryBatchNoControl_start_batchNo.Text;
            }
            set
            {
                this.inventoryBatchNoControl_start_batchNo.Text = value;
            }
        }

        public List<string> LibraryCodeList
        {
            get
            {
                return this.inventoryBatchNoControl_start_batchNo.LibraryCodeList;
            }
            set
            {
                this.inventoryBatchNoControl_start_batchNo.LibraryCodeList = value;
            }
        }

        private void button_findBarcodeFileName_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的册条码号文件名";
            dlg.FileName = this.textBox_barcodeFileName.Text;
            dlg.Filter = "册条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_barcodeFileName.Text = dlg.FileName;
        }
    }
}
