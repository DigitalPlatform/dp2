using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 卡片打印窗
    /// </summary>
    public partial class CardPrintForm : MyForm
    {
        PrinterInfo m_printerInfo = null;

        /// <summary>
        /// 打印机信息
        /// </summary>
        public PrinterInfo PrinterInfo
        {
            get
            {
                return this.m_printerInfo;
            }
            set
            {
                this.m_printerInfo = value;
                SetTitle();
            }
        }

        ProgressEstimate estimate = new ProgressEstimate();
        /*
        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
         * */

        PrintCardDocument document = null;

        string m_strPrintStyle = "";    // 打印风格

        /// <summary>
        /// 构造函数
        /// </summary>
        public CardPrintForm()
        {
            InitializeComponent();
        }

        private void CardPrintForm_Load(object sender, EventArgs e)
        {
            this.checkBox_cardFile_indent.Checked = this.MainForm.AppInfo.GetBoolean(
    "card_print_form",
    "indent",
    true);

            if (string.IsNullOrEmpty(this.textBox_cardFile_cardFilename.Text) == true)
            {
                this.textBox_cardFile_cardFilename.Text = this.MainForm.AppInfo.GetString(
        "card_print_form",
        "card_file_name",
        "");
            }

            if (m_bTestingGridSetted == false)
            {
                this.checkBox_testingGrid.Checked = this.MainForm.AppInfo.GetBoolean(
                    "card_print_form",
                    "print_testing_grid",
                    false);
            }

            if (this.PrinterInfo == null)
            {
                this.PrinterInfo = this.MainForm.PreparePrinterInfo("缺省卡片");
            }

            SetTitle();
        }

        private void CardPrintForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetString(
    "card_print_form",
    "card_file_name",
    this.textBox_cardFile_cardFilename.Text);

                this.MainForm.AppInfo.SetBoolean(
                    "card_print_form",
                    "print_testing_grid",
                    this.checkBox_testingGrid.Checked);

                this.MainForm.AppInfo.SetBoolean(
        "card_print_form",
        "indent",
        this.checkBox_cardFile_indent.Checked);

                if (this.PrinterInfo != null)
                {
                    string strType = this.PrinterInfo.Type;
                    if (string.IsNullOrEmpty(strType) == true)
                        strType = "缺省卡片";

                    this.MainForm.SavePrinterInfo(strType,
                        this.PrinterInfo);
                }
            }
        }

        /// <summary>
        ///重新设置窗口标题
        /// </summary>
        public void SetTitle()
        {
            if (this.PrinterInfo == null)
                this.Text = "打印";
            else
                this.Text = "打印 -- " + this.PrinterInfo.Type + " -- " + this.PrinterInfo.PaperName + " -- " + this.PrinterInfo.PrinterName + (this.PrinterInfo.Landscape == true ? " --- 横向" : "");
        }

        private void button_cardFile_findCardFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的卡片文件名";
            dlg.FileName = this.textBox_cardFile_cardFilename.Text;
            dlg.Filter = "卡片文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_cardFile_cardFilename.Text = dlg.FileName;

        }

        private void button_print_Click(object sender, EventArgs e)
        {
            PrintFromCardFile();
        }

        private void button_printPreview_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_cardFile)
                PrintPreviewFromCardFile(Control.ModifierKeys == Keys.Control ? true : false);
        }

        // 打印(根据卡片文件)
        /// <summary>
        /// 根据卡片文件进行打印
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintFromCardFile(bool bDisplayPrinterDialog = true)
        {
            string strError = "";
            int nRet = this.BeginPrint(
                this.textBox_cardFile_cardFilename.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            stop.OnStop += new DigitalPlatform.StopEventHandler(stop_OnStop);
            stop.BeginLoop();

            this.EnableControls(false);
            /*
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
             * */
            this.estimate.StartEstimate();

            try
            {

                // Allow the user to choose the page range he or she would
                // like to print.
                printDialog1.AllowSomePages = true;

                // Show the help button.
                printDialog1.ShowHelp = true;

                // Set the Document property to the PrintDocument for 
                // which the PrintPage Event has been handled. To display the
                // dialog, either this property or the PrinterSettings property 
                // must be set 
                printDialog1.Document = this.document;

                if (this.PrinterInfo != null)
                {
                    string strPrinterName = document.PrinterSettings.PrinterName;
                    if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == false
                        && this.PrinterInfo.PrinterName != strPrinterName)
                    {
                        this.document.PrinterSettings.PrinterName = this.PrinterInfo.PrinterName;
                        if (this.document.PrinterSettings.IsValid == false)
                        {
                            MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 当前不可用，请重新选定打印机");
                            this.document.PrinterSettings.PrinterName = strPrinterName;
                            this.PrinterInfo.PrinterName = "";
                            bDisplayPrinterDialog = true;
                        }
                    }

                    PaperSize old_papersize = document.DefaultPageSettings.PaperSize;
                    if (string.IsNullOrEmpty(this.PrinterInfo.PaperName) == false
                        && this.PrinterInfo.PaperName != document.DefaultPageSettings.PaperSize.PaperName)
                    {
                        PaperSize found = null;
                        foreach (PaperSize ps in this.document.PrinterSettings.PaperSizes)
                        {
                            if (ps.PaperName.Equals(this.PrinterInfo.PaperName))
                            {
                                found = ps;
                                break;
                            }
                        }

                        if (found != null)
                            this.document.DefaultPageSettings.PaperSize = found;
                        else
                        {
                            MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 的纸张类型 " + this.PrinterInfo.PaperName + " 当前不可用，请重新选定纸张");
                            document.DefaultPageSettings.PaperSize = old_papersize;
                            this.PrinterInfo.PaperName = "";
                            bDisplayPrinterDialog = true;
                        }
                    }

                    // 只要有一个打印机事项没有确定，就要出现打印机对话框
                    if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == true
                        || string.IsNullOrEmpty(this.PrinterInfo.PaperName) == true)
                        bDisplayPrinterDialog = true;
                }
                else
                {
                    // 没有首选配置的情况下要出现打印对话框
                    bDisplayPrinterDialog = true;
                }

                DialogResult result = DialogResult.OK;
                if (bDisplayPrinterDialog == true)
                {
                    result = printDialog1.ShowDialog();
                }

                // If the result is OK then print the document.
                if (result == DialogResult.OK)
                {
                    try
                    {
                        if (bDisplayPrinterDialog == true)
                        {
                            // 记忆打印参数
                            if (this.PrinterInfo == null)
                                this.PrinterInfo = new PrinterInfo();
                            this.PrinterInfo.PrinterName = document.PrinterSettings.PrinterName;
                            this.PrinterInfo.PaperName = document.DefaultPageSettings.PaperSize.PaperName;
                            this.PrinterInfo.Landscape = document.DefaultPageSettings.Landscape;

                            SetTitle();
                        }

                        this.document.Print();
                    }
                    catch (Exception ex)
                    {
                        strError = "打印过程出错: " + ex.Message;
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                /*
                this.Cursor = oldCursor;
                 * */
                this.EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new DigitalPlatform.StopEventHandler(stop_OnStop);

                this.stop.HideProgress();
            }

            this.EndPrint();
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }



        void stop_OnStop(object sender, DigitalPlatform.StopEventArgs e)
        {
            this.document.Stop = true;
        }

        // 打印预览(根据卡片文件)
        /// <summary>
        /// 根据卡片文件进行打印预览
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintPreviewFromCardFile(bool bDisplayPrinterDialog = false)
        {
            string strError = "";
            int nRet = this.BeginPrint(
                this.textBox_cardFile_cardFilename.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            stop.OnStop += new DigitalPlatform.StopEventHandler(stop_OnStop);
            stop.BeginLoop(); 
            this.EnableControls(false);
            this.estimate.StartEstimate();
            try
            {
                printDialog1.Document = this.document;

                if (this.PrinterInfo != null)
                {
                    string strPrinterName = document.PrinterSettings.PrinterName;
                    if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == false
                        && this.PrinterInfo.PrinterName != strPrinterName)
                    {
                        this.document.PrinterSettings.PrinterName = this.PrinterInfo.PrinterName;
                        if (this.document.PrinterSettings.IsValid == false)
                        {
                            MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 当前不可用，请重新选定打印机");
                            this.document.PrinterSettings.PrinterName = strPrinterName;
                            this.PrinterInfo.PrinterName = "";
                            bDisplayPrinterDialog = true;
                        }
                    }

                    PaperSize old_papersize = document.DefaultPageSettings.PaperSize;
                    if (string.IsNullOrEmpty(this.PrinterInfo.PaperName) == false
                        && this.PrinterInfo.PaperName != document.DefaultPageSettings.PaperSize.PaperName)
                    {
                        PaperSize found = null;
                        foreach (PaperSize ps in this.document.PrinterSettings.PaperSizes)
                        {
                            if (ps.PaperName.Equals(this.PrinterInfo.PaperName))
                            {
                                found = ps;
                                break;
                            }
                        }

                        if (found != null)
                            this.document.DefaultPageSettings.PaperSize = found;
                        else
                        {
                            MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 的纸张类型 " + this.PrinterInfo.PaperName + " 当前不可用，请重新选定纸张");
                            document.DefaultPageSettings.PaperSize = old_papersize;
                            this.PrinterInfo.PaperName = "";
                            bDisplayPrinterDialog = true;
                        }
                    }

                    // 只要有一个打印机事项没有确定，就要出现打印机对话框
                    if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == true
                        || string.IsNullOrEmpty(this.PrinterInfo.PaperName) == true)
                        bDisplayPrinterDialog = true;
                }
                else
                {
                    // 没有首选配置的情况下要出现打印对话框
                    bDisplayPrinterDialog = true;
                }

                DialogResult result = DialogResult.OK;
                if (bDisplayPrinterDialog == true)
                {
                    result = printDialog1.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // 记忆打印参数
                        if (this.PrinterInfo == null)
                            this.PrinterInfo = new PrinterInfo();
                        this.PrinterInfo.PrinterName = document.PrinterSettings.PrinterName;
                        this.PrinterInfo.PaperName = document.DefaultPageSettings.PaperSize.PaperName;
                        this.PrinterInfo.Landscape = document.DefaultPageSettings.Landscape;
                        SetTitle();
                    }
                }

                printPreviewDialog1.Document = this.document;

                this.MainForm.AppInfo.LinkFormState(printPreviewDialog1, "labelprintform_printpreviewdialog_state");
                printPreviewDialog1.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(printPreviewDialog1);

            }
            finally
            {
                this.EnableControls(true);
                stop.EndLoop();
                stop.OnStop -= new DigitalPlatform.StopEventHandler(stop_OnStop);

                this.stop.HideProgress();

            }

            this.EndPrint();
            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // parameters:
        //      strCardFilename    卡片文件名
        int BeginPrint(
            string strCardFilename,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strCardFilename) == true)
            {
                strError = "尚未指定卡片文件名";
                return -1;
            }

            if (this.document != null)
            {
                this.document.Close();
                this.document = null;
            }



            this.document = new PrintCardDocument();
            this.document.Stop = false;

            this.document.SetProgress -= new SetProgressEventHandler(document_SetProgress);
            this.document.SetProgress += new SetProgressEventHandler(document_SetProgress);

            int nRet = this.document.Open(strCardFilename,
                out strError);
            if (nRet == -1)
                return -1;

            this.document.PrintPage -= new System.Drawing.Printing.PrintPageEventHandler(document_PrintPage);
            this.document.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(document_PrintPage);

            if (this.checkBox_testingGrid.Checked == true)
                this.m_strPrintStyle = "TestingGrid";
            else
                this.m_strPrintStyle = "";

            return 0;
        }

        long m_lCount = 0;

        void document_SetProgress(object sender, SetProgressEventArgs e)
        {
            Application.DoEvents();

            if (e.Value == -1)
            {
                this.estimate.SetRange(e.Start, e.End);

                this.stop.SetProgressRange(e.Start, e.End);

                this.progressBar_records.Minimum = (int)e.Start;
                this.progressBar_records.Maximum = (int)e.End;
            }
            else
            {
                if ((this.m_lCount++ % 10) == 1)
                    this.stop.SetMessage("剩余时间 " + ProgressEstimate.Format(this.estimate.Estimate(e.Value)) + " 已经过时间 " + ProgressEstimate.Format(this.estimate.delta_passed));

                this.stop.SetProgressValue(e.Value);
                // this.stop.SetMessage(e.Value.ToString() + " - " + (((double)e.Value / (double)e.End) * 100).ToString() + "%");

                this.progressBar_records.Value = (int)e.Value;
            }
        }

        void document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            this.document.DoPrintPage(this,
                this.m_strPrintStyle,
                e);
        }

        void EndPrint()
        {
            if (this.document != null)
            {
                this.document.Close();
                this.document = null;

            }
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_cardFile_cardFilename.Enabled = bEnable;
            this.button_cardFile_findCardFilename.Enabled = bEnable;

            this.button_print.Enabled = bEnable;
            this.button_printPreview.Enabled = bEnable;

            this.checkBox_testingGrid.Enabled = bEnable;
            this.checkBox_cardFile_indent.Enabled = bEnable;

            this.Update();
        }

        // 在窗口打开前TestingGrid是否设置过
        bool m_bTestingGridSetted = false;
        /// <summary>
        /// 是否打印调试线
        /// </summary>
        public bool TestingGrid
        {
            get
            {
                return this.checkBox_testingGrid.Checked;
            }
            set
            {
                this.checkBox_testingGrid.Checked = value;
                this.m_bTestingGridSetted = true;
            }
        }

        /// <summary>
        /// 当前卡片文件全路径
        /// </summary>
        public string CardFilename
        {
            get
            {
                return this.textBox_cardFile_cardFilename.Text;
            }
            set
            {
                this.textBox_cardFile_cardFilename.Text = value;
            }
        }

        private void textBox_cardFile_cardFilename_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_cardFile_cardFilename.Text) == true)
            {
                this.textBox_cardFile_cardFilename.Text = "";
                return;
            }

            string strError = "";
            string strContent = "";
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = Global.ReadTextFileContent(this.textBox_cardFile_cardFilename.Text,
                100 * 1024, // 100K
                out strContent,
                out strError);
            if (nRet == 1 || nRet == 2)
            {
                bool bExceed = nRet == 2;
                string strXml = "";
                if (this.checkBox_cardFile_indent.Checked == true)
                {
                    nRet = DomUtil.GetIndentXml(strContent,
                        out strXml,
                        out strError);
                    if (nRet == -1)
                    {
                        if (bExceed == false)
                            MessageBox.Show(this, strError);
                        strXml = strContent;
                    }
                }
                else
                    strXml = strContent;

                this.textBox_cardFile_content.Text = 
                    (bExceed == true ? "文件尺寸太大，下面只显示了开头部分...\r\n" : "") + strXml;
            }
            else
                this.textBox_cardFile_content.Text = "";

        }

        private void checkBox_cardFile_indent_CheckedChanged(object sender, EventArgs e)
        {
            // 重新装载文件内容
            textBox_cardFile_cardFilename_TextChanged(sender, e);
        }

        private void CardPrintForm_Activated(object sender, EventArgs e)
        {
            // this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
        }


    }



}
