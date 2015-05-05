using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DigitalPlatform;
using System.Collections;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    public partial class NewInventoryForm : BatchPrintFormBase
    {
        string BarcodeFilePath = "";
        string RecPathFilePath = "";

        const int COLUMN_ICON1 = 0;
        const int COLUMN_ICON2 = 1;
        const int COLUMN_BARCODE = 2;
        const int COLUMN_SUMMARY = 3;
        const int COLUMN_RECPATH = 4;   // 册记录路径
        const int COLUMN_BIBLIORECPATH = 5;   // 书目记录路径

        public NewInventoryForm()
        {
            InitializeComponent();
        }

        private void NewInventoryForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            this.BarcodeFilePath = this.MainForm.AppInfo.GetString(
    "inventory_form",
    "barcode_filepath",
    "");

            this.RecPathFilePath = this.MainForm.AppInfo.GetString(
"inventory_form",
"recpath_filepath",
"");

        }

        private void NewInventoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void NewInventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.MainForm.AppInfo.SetString(
"inventory_form",
"barcode_filepath",
this.BarcodeFilePath);

            this.MainForm.AppInfo.SetString(
                "inventory_form",
                "recpath_filepath",
                this.RecPathFilePath);
        }

        void ClearBefore()
        {
            this.dpTable1.Rows.Clear();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // load page
            this.comboBox_load_type.Enabled = bEnable;
            // this.button_load_loadFromBatchNo.Enabled = bEnable;
            this.button_load_loadFromBarcodeFile.Enabled = bEnable;
            this.button_load_loadFromRecPathFile.Enabled = bEnable;

#if NO
            // next button
            if (bEnable == true)
                SetNextButtonEnable();
            else
                this.button_next.Enabled = false;
#endif

            // verify page
            this.textBox_verify_itemBarcode.Enabled = bEnable;
            this.button_verify_load.Enabled = bEnable;
            this.checkBox_verify_autoUppercaseBarcode.Enabled = bEnable;
            this.button_verify_loadFromBarcodeFile.Enabled = bEnable;

            // print page
            this.button_print_option.Enabled = bEnable;
            this.button_print_printList.Enabled = bEnable;
        }


        private void button_load_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bClearBefore = true;
            if (Control.ModifierKeys == Keys.Control)
                bClearBefore = false;

            if (bClearBefore == true)
                ClearBefore();

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            // dlg.FileName = this.BarcodeFilePath;
            // dlg.InitialDirectory = 
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            EnableControls(false);
            // MainForm.ShowProgress(true);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在将册条码号转换为记录路径 ...");
            stop.BeginLoop();

            try
            {
                Hashtable barcode_table = new Hashtable();
                int nDupCount = 0;
                List<string> lines = new List<string>();

                using (StreamReader sr = new StreamReader(dlg.FileName))
                {
                    for (; ; )
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        if (barcode_table[strLine] != null)
                        {
                            nDupCount++;
                            continue;
                        }

                        barcode_table[strLine] = true;
                        lines.Add(strLine);
                    }
                }

                if (lines.Count == 0)
                {
                    strError = "条码号文件为空";
                    goto ERROR1;
                }

                stop.SetProgressRange(0, lines.Count);

                ItemBarcodeLoader loader = new ItemBarcodeLoader();
                loader.Channel = this.Channel;
                loader.Stop = this.stop;
                loader.Barcodes = lines;

                int i = 0;
                foreach (EntityItem item in loader)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    DpRow row = new DpRow();
                    // icon1
                    DpCell cell = new DpCell();
                    row.Add(cell);

                    // icon2
                    cell = new DpCell();
                    row.Add(cell);

                    // barcode
                    cell = new DpCell();
                    cell.Text = item.Barcode;
                    row.Add(cell);

                    // summary
                    cell = new DpCell();
                    // 如果出错
                    if (string.IsNullOrEmpty(item.ErrorInfo) == false)
                        cell.Text = item.ErrorInfo;
                    row.Add(cell);

                    // recpath
                    cell = new DpCell();
                    cell.Text = item.RecPath;
                    row.Add(cell);

                    this.dpTable1.Rows.Add(row);

                    i++;
                    stop.SetProgressValue(i);
                }


                // BrowseLoader
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
