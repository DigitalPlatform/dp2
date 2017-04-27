using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.IO;
using DigitalPlatform.Drawing;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 标签打印窗
    /// </summary>
    public partial class LabelPrintForm : ItemSearchFormBase    // MyForm
    {
        // bool m_bBiblioSummaryColumn = true; // 是否在浏览列表中 加入书目摘要列

        // bool m_bFirstColumnIsKey = false; // 当前listview浏览列的第一列是否应为key


        PrinterInfo m_printerInfo = null;

        /// <summary>
        /// 构造函数
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

#if NO
        // 最近使用过的记录路径文件名
        string m_strUsedRecPathFilename = "";
#endif

        /// <summary>
        /// 最近导出的册条码号文件名
        /// </summary>
        public string ExportBarcodeFilename = "";
        /// <summary>
        /// 最近导出的记录路径文件名
        /// </summary>
        public string ExportRecPathFilename = "";
        /// <summary>
        /// 最近导出的文本文件名
        /// </summary>
        public string ExportTextFilename = "";

        LabelParam label_param = new LabelParam();

        PrintLabelDocument document = null;

        string m_strPrintStyle = "";    // 打印风格

        /// <summary>
        /// 构造函数
        /// </summary>
        public LabelPrintForm()
        {
            InitializeComponent();

            _listviewRecords = this.listView_records;

            ListViewProperty prop = new ListViewProperty();
            this.listView_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);

            prop.CompareColumn -= new CompareEventHandler(prop_CompareColumn);
            prop.CompareColumn += new CompareEventHandler(prop_CompareColumn);
        }

        void prop_CompareColumn(object sender, CompareEventArgs e)
        {
            if (e.Column.SortStyle.Name == "call_number")
            {
                // 比较两个索取号的大小
                // return:
                //      <0  s1 < s2
                //      ==0 s1 == s2
                //      >0  s1 > s2
                e.Result = StringUtil.CompareAccessNo(e.String1, e.String2, true);
            }
            else if (e.Column.SortStyle.Name == "parent_id")
            {
                // 右对齐比较字符串
                // parameters:
                //      chFill  填充用的字符
                e.Result = StringUtil.CompareRecPath(e.String1, e.String2);
            }
            else
                e.Result = string.Compare(e.String1, e.String2);
        }

#if NO
        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            e.ColumnTitles = Program.MainForm.GetBrowseColumnProperties(e.DbName);
            e.ListViewProperty.SetSortStyle(2, ColumnSortStyle.LeftAlign);
        }
#endif
        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
                // 数量列的排序
                e.ListViewProperty.SetSortStyle(2, ColumnSortStyle.RightAlign);
                return;
            }

            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(e.DbName);
            if (temp != null)
            {
                if (m_bBiblioSummaryColumn == true)
                    e.ColumnTitles.Insert(0, "书目摘要");
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
            }

            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");

            // e.ListViewProperty.SetSortStyle(2, ColumnSortStyle.LeftAlign);   // 应该根据 type为item_barcode 来决定排序方式
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_records.Tag;
            prop.ClearCache();
        }

        private void LabelPrintForm_Load(object sender, EventArgs e)
        {
#if NO
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#endif
#if NO
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            if (string.IsNullOrEmpty(this.textBox_labelFile_labelFilename.Text) == true)
            {
                this.textBox_labelFile_labelFilename.Text = Program.MainForm.AppInfo.GetString(
                    "label_print_form",
                    "label_file_name",
                    "");
            }

            if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
            {
                this.textBox_labelDefFilename.Text = Program.MainForm.AppInfo.GetString(
                    "label_print_form",
                    "label_def_file_name",
                    "");
            }

            if (this.m_bTestingGridSetted == false)
            {
                this.checkBox_testingGrid.Checked = Program.MainForm.AppInfo.GetBoolean(
                    "label_print_form",
                    "print_testing_grid",
                    false);
            }

            string strWidths = Program.MainForm.AppInfo.GetString(
                "label_print_form",
                "record_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_records,
                    strWidths,
                    true);
            }

            // 当前活动的property page
            string strActivePage = Program.MainForm.AppInfo.GetString(
                "label_print_form",
                "active_page",
                "");

            if (String.IsNullOrEmpty(strActivePage) == false)
            {
                if (strActivePage == "itemrecords")
                    this.tabControl_main.SelectedTab = this.tabPage_itemRecords;
                else if (strActivePage == "labelfile")
                    this.tabControl_main.SelectedTab = this.tabPage_labelFile;
            }

            if (this.PrinterInfo == null)
            {
                this.PrinterInfo = Program.MainForm.PreparePrinterInfo("缺省标签");
            }
            SetTitle();
        }

        private void LabelPrintForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (stop != null)
            {
                if (stop.State == 0)    // 0 表示正在处理
                {
                    MessageBox.Show(this, "请在关闭窗口前停止正在进行的长时操作。");
                    e.Cancel = true;
                    return;
                }
            }
#endif
        }

        private void LabelPrintForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetString(
                    "label_print_form",
                    "label_file_name",
                    this.textBox_labelFile_labelFilename.Text);

                Program.MainForm.AppInfo.SetString(
                    "label_print_form",
                    "label_def_file_name",
                    this.textBox_labelDefFilename.Text);

                Program.MainForm.AppInfo.SetBoolean(
                    "label_print_form",
                    "print_testing_grid",
                    this.checkBox_testingGrid.Checked);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_records);
                Program.MainForm.AppInfo.SetString(
                    "label_print_form",
                    "record_list_column_width",
                    strWidths);

                // 当前活动的property page
                string strActivePage = "";

                if (this.tabControl_main.SelectedTab == this.tabPage_labelFile)
                    strActivePage = "labelfile";
                else if (this.tabControl_main.SelectedTab == this.tabPage_itemRecords)
                    strActivePage = "itemrecords";

                Program.MainForm.AppInfo.SetString(
                    "label_print_form",
                    "active_page",
                    strActivePage);

                if (this.PrinterInfo != null)
                {
                    string strType = this.PrinterInfo.Type;
                    if (string.IsNullOrEmpty(strType) == true)
                        strType = "缺省标签";

                    Program.MainForm.SavePrinterInfo(strType,
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
                this.Text = "标签打印";
            else
                this.Text = "打印 -- " + this.PrinterInfo.Type + " -- " + this.PrinterInfo.PaperName + " -- " + this.PrinterInfo.PrinterName + (this.PrinterInfo.Landscape == true ? " --- 横向" : "");
        }

        private void button_labelFile_findLabelFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的标签文件名";
            dlg.FileName = this.textBox_labelFile_labelFilename.Text;
            dlg.Filter = "标签文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_labelFile_labelFilename.Text = dlg.FileName;
        }

        private void button_labelFile_findLabelDefFilename_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的标签定义文件名";
            dlg.FileName = this.textBox_labelDefFilename.Text;
            dlg.Filter = "标签定义文件 (*.xml)|*.xml|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_labelDefFilename.Text = dlg.FileName;
        }

        private void textBox_labelFile_labelFilename_TextChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.textBox_labelFile_labelFilename.Text) == true)
            {
                this.textBox_labelFile_content.Text = "";
                return;
            }

            string strError = "";
            string strContent = "";
            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // return:
            //      -1  出错
            //      0   文件不存在
            //      1   文件存在
            int nRet = Global.ReadTextFileContent(this.textBox_labelFile_labelFilename.Text,
                out strContent,
                out strError);
            if (nRet == 1)
                this.textBox_labelFile_content.Text = strContent;
            else
                this.textBox_labelFile_content.Text = "";
        }

        private void button_printPreview_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_labelFile)
                PrintPreviewFromLabelFile(Control.ModifierKeys == Keys.Control ? true : false);
            else if (this.tabControl_main.SelectedTab == this.tabPage_itemRecords)
                PrintPreviewFromItemRecords(Control.ModifierKeys == Keys.Control ? true : false);
        }

        int GetBiblioInfo(string strBiblioRecPath,
    string strBiblioType,
    out string strBiblio,
    out string strError)
        {
            strError = "";
            strBiblio = "";

            /*
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在执行脚本 ...");
            stop.BeginLoop();

            try
            {*/

            string strBiblioXml = "";   // 向服务器提供的XML记录
            long lRet = this.Channel.GetBiblioInfo(
                null,   // this.stop,
                strBiblioRecPath,
                strBiblioXml,
                strBiblioType,
                out strBiblio,
                out strError);
            return (int)lRet;
            /*
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }*/
        }

        static void WriteErrorText(StreamWriter sw_error, string strText)
        {
            sw_error.WriteLine(strText);
        }

        // 根据册记录路径创建标签文本文件
        int BuildLabelFile(
            string strOutputFilename,
            string strOutputErrorFilename,
            out string strError)
        {
            strError = "";

            int nLabelCount = 0;

            int nErrorCount = 0;    // 记载输出的错误次数。如果处理结束后此值为 0，表示没有输出任何错误信息，但文件可能因为 UTF-8 的 Preamable 有 3 byte 长度。所以需要这个变量来额外判断

            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(strOutputFilename,
                     false,	// append
                     Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "创建标签文件 " + strOutputFilename + " 时出错: " + ex.Message;
                return -1;
            }

            StreamWriter sw_error = null;

            if (String.IsNullOrEmpty(strOutputErrorFilename) == false)
            {
                try
                {
                    sw_error = new StreamWriter(strOutputErrorFilename,
                         false,	// append
                         Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    strError = "创建错误信息文件 " + strOutputErrorFilename + " 时出错: " + ex.Message;
                    if (sw != null)
                        sw.Close();
                    return -1;
                }
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取册记录和创建标签文件 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strAccessNoSource = this.AccessNoSource;

#if NO
                bool bHideMessageBox = false;
                DialogResult result = System.Windows.Forms.DialogResult.No;
#endif

                stop.SetProgressRange(0, this.listView_records.Items.Count);

                for (int i = 0; i < this.listView_records.Items.Count; i++)
                {
                    ListViewItem item = this.listView_records.Items[i];
                    string strRecPath = item.Text;
                    if (String.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    string strAccessPoint = "@path:" + strRecPath;

                    string strOutputRecPath = "";
                    string strResult = "";
                    string strBiblio = "";
                    string strBiblioRecPath = "";
                    byte[] baTimestamp = null;

                    // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                    long lRet = Channel.GetItemInfo(
                        stop,
                        strAccessPoint,
                        "xml",   // strResultType
                        out strResult,
                        out strOutputRecPath,
                        out baTimestamp,
                        "recpath", // strBiblioType
                        out strBiblio,
                        out strBiblioRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得册记录 {" + strRecPath + "} 时发生错误: " + strError;
                        WriteErrorText(sw_error, strError);
                        nErrorCount++;
                        continue;
                    }

                    if (lRet == 0)
                    {
                        strError = "{" + strRecPath + "} 的XML数据没有找到。";
                        WriteErrorText(sw_error, strError);
                        nErrorCount++;
                        continue;
                    }

                    if (lRet > 1)
                    {
                        strError = "{" + strRecPath + "} 对应数据记录多于一条(为 " + lRet.ToString() + " 条)。";
                        WriteErrorText(sw_error, strError);
                        nErrorCount++;
                        continue;
                    }

                    string strXml = "";

                    strXml = strResult;

                    // 看看是否在希望统计的范围内
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "册记录 {" + strRecPath + "} XML装入DOM发生错误: " + ex.Message;
                        WriteErrorText(sw_error, strError);
                        nErrorCount++;
                        continue;
                    }

                    string strAccessNo = DomUtil.GetElementText(dom.DocumentElement,
                        "accessNo");
                    if (string.IsNullOrEmpty(strAccessNo) == false)
                        strAccessNo = StringUtil.GetPlainTextCallNumber(strAccessNo);

                    if (string.IsNullOrEmpty(strAccessNo) == true
    && strAccessNoSource == "从册记录")
                    {
                        strError = "册记录 {" + strRecPath + "} 中没有索取号字段内容";
                        WriteErrorText(sw_error, strError);
                        nErrorCount++;
                        continue;
                    }

                    if (strAccessNoSource == "从书目记录"
                        || (strAccessNoSource == "顺次从册记录、书目记录" && String.IsNullOrEmpty(strAccessNo) == true)
                        )
                    {
                        // 从种中取索取号
                        // TODO: 可以在系统参数中配置几种处理方式
                        // 1) 仅仅从册记录中获得
                        // 2) 仅仅从书目记录中获得
                        // 3) 先从册记录中获得，如果没有再从书目记录中获得
                        // 那么这里就仅仅可以在首次出现特殊情况的时候提示一下即可。或者处理完以后集中总结一下出现在 MessageBox 中

#if NO
                        if (bHideMessageBox == false)
                        {
                            // TODO: 按钮文字较长的时候，应该能自动适应
                            result = MessageDialog.Show(this,
        "册记录 " + strRecPath + " 中索取号字段内容为空。请问是否要对这样的记录，试探从书目记录中 (905字段) 获取索取号?\r\n\r\n(获取) 从书目记录中获取；(跳过) 跳过这条记录; (中断) 中断处理",
            MessageBoxButtons.YesNoCancel,
            MessageBoxDefaultButton.Button2,
            null,
            ref bHideMessageBox,
            new string[] { "获取", "跳过", "中断" });
                        }
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break; 
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            string strContent = "";
                            int nRet = this.GetBiblioInfo(strBiblioRecPath,
                                "@accessno",
                                out strContent,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "从册记录 {" + strRecPath + "} 从属的种记录 " + strBiblioRecPath + " 中取索取号的时候发生错误: " + strError;
                                WriteErrorText(sw_error, strError);
                                nErrorCount++;
                                continue;
                            }

                            if (String.IsNullOrEmpty(strContent.Replace("/", "")) == true)
                            {
                                strBiblio = "";
                                // 种也没有索取号
                                strError = "册记录 {" + strRecPath + "} 中没有索取号，并且其从属的种记录 " + strBiblioRecPath + " 中也没有索书号";
                                WriteErrorText(sw_error, strError);
                                nErrorCount++;
                                continue;
                            }

                            strAccessNo = strContent;
                        }
#endif

                        {
                            string strContent = "";
                            int nRet = this.GetBiblioInfo(strBiblioRecPath,
                                "@accessno",
                                out strContent,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "从册记录 {" + strRecPath + "} 所从属的书目记录 " + strBiblioRecPath + " 中取索取号的时候发生错误: " + strError;
                                WriteErrorText(sw_error, strError);
                                nErrorCount++;
                                continue;
                            }

                            if (String.IsNullOrEmpty(strContent.Replace("/", "")) == true)
                            {
                                strBiblio = "";
                                // 种也没有索取号

                                if (strAccessNo == "顺次从册记录、书目记录")
                                    strError = "册记录 {" + strRecPath + "} 中没有索取号，并且其从属的书目记录 " + strBiblioRecPath + " 中也没有找到索取号";
                                else
                                    strError = "从册记录 {" + strRecPath + "} 所从属的书目记录 " + strBiblioRecPath + " 中没有找到索取号";
                                WriteErrorText(sw_error, strError);
                                nErrorCount++;
                                continue;
                            }

                            strAccessNo = strContent;
                        }

#if NO
                        // 卷号?
                        string strVolume = DomUtil.GetElementText(dom.DocumentElement, "volume");
                        if (String.IsNullOrEmpty(strVolume) == false)
                            strAccessNo += "/" + strVolume;
#endif
                    }



                    string strText = strAccessNo.Replace("/", "\r\n");

                    try
                    {
                        sw.Write(strText + "\r\n***\r\n");
                    }
                    catch (Exception ex)
                    {
                        strError = "写入标签文件 " + strOutputFilename + " 时发生错误: " + ex.Message;
                        return -1;
                    }

                    nLabelCount++;

                    //CONTINUE:
                    stop.SetProgressValue(i);
                } // end of for
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                stop.HideProgress();

                if (sw != null)
                    sw.Close();

                if (sw_error != null)
                    sw_error.Close();

            }

            if (FileUtil.GetFileLength(strOutputErrorFilename) == 0
                || nErrorCount == 0)
                File.Delete(strOutputErrorFilename);

            return 0;
        }

        /// <summary>
        /// 打印预览(根据标签文件)
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机设置对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintPreviewFromLabelFile(bool bDisplayPrinterDialog = false)
        {
            string strError = "";

            this._processing++;
            try
            {
                int nRet = Print(
                    this.textBox_labelDefFilename.Text,
                    this.textBox_labelFile_labelFilename.Text,
                    bDisplayPrinterDialog,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                return 0;
            }
            finally
            {
                this._processing--;
            }
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }
#if NO
        // 
        /// <summary>
        /// 打印预览(根据标签文件)
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机设置对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintPreviewFromLabelFile(bool bDisplayPrinterDialog = false)
        {
            string strError = "";

            int nRet = this.BeginPrint(
                this.textBox_labelFile_labelFilename.Text,
                this.textBox_labelDefFilename.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.document.PreviewMode = true;

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行打印预览</div>");
            this.EnableControls(false);
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

                        // 2014/3/27
                        this.document.DefaultPageSettings = document.PrinterSettings.DefaultPageSettings;

                        SetTitle();
                    }
                }

                TracePrinterInfo();

                printPreviewDialog1.Document = this.document;

                Program.MainForm.AppInfo.LinkFormState(printPreviewDialog1, "labelprintform_printpreviewdialog_state");
                printPreviewDialog1.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(printPreviewDialog1);
            }
            finally
            {
                this.EnableControls(true);

                this.EndPrint();

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行打印预览</div>");
            }

            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }
#endif

        void TracePrinterInfo()
        {
            this.OutputText("打印机名称: " + this.document.PrinterSettings.PrinterName);
            this.OutputText("纸张名称: " + this.document.DefaultPageSettings.PaperSize.ToString());
            this.OutputText("纸张方向: " +
                    (this.document.DefaultPageSettings.Landscape == true ? "横向" : "纵向"));
            this.OutputText("可打印区域: " + this.document.DefaultPageSettings.PrintableArea.ToString());
        }

        /// <summary>
        /// 选定打印机，按打印机名
        /// </summary>
        /// <param name="document">PrintDocument 对象</param>
        /// <param name="strPrinterName">打印机名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>0: 成功选定: 1: 没有选定，因为名字不可用。建议后面出现打印机对话框选定</returns>
        public static int SelectPrinterByName(PrintDocument document,
            string strPrinterName,
            out string strError)
        {
            strError = "";

            string strCurrentPrinterName = document.PrinterSettings.PrinterName;
            if (string.IsNullOrEmpty(strPrinterName) == false
                && strPrinterName != strCurrentPrinterName)
            {
                document.PrinterSettings.PrinterName = strPrinterName;
                if (document.PrinterSettings.IsValid == false)
                {
                    strError = "打印机 " + strPrinterName + " 当前不可用，请重新选定打印机";
                    document.PrinterSettings.PrinterName = strCurrentPrinterName;
                    return 1;
                }
            }

            return 0;
        }

#if NO
        /// <summary>
        /// 选定纸张，按纸张名字
        /// </summary>
        /// <param name="document">PrintDocument 对象</param>
        /// <param name="strPaperName">纸张名字</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>0: 成功选定: 1: 没有选定，因为名字不可用。建议后面出现打印机对话框选定</returns>
        public static int SelectPaperByName(PrintDocument document,
            string strPaperName,
            bool bCheck,
            out string strError)
        {
            strError = "";

            PaperSize old_papersize = document.DefaultPageSettings.PaperSize;
            if ((string.IsNullOrEmpty(strPaperName) == false
                && strPaperName != document.DefaultPageSettings.PaperSize.PaperName)
                || 
                (bCheck == true && string.IsNullOrEmpty(strPaperName) == false))
            {
                PaperSize found = null;
                foreach (PaperSize ps in document.PrinterSettings.PaperSizes)
                {
                    if (ps.PaperName.Equals(strPaperName))
                    {
                        found = ps;
                        break;
                    }
                }

                if (found != null)
                    document.DefaultPageSettings.PaperSize = found;
                else
                {
                    strError = "打印机 " + document.PrinterSettings.PrinterName + " 的纸张类型 " + strPaperName + " 当前不可用，请重新选定纸张";
                    // document.DefaultPageSettings.PaperSize = old_papersize;
                    return 1;
                }
            }

            return 0;
        }
#endif
        /// <summary>
        /// 选定纸张，按纸张名字
        /// </summary>
        /// <param name="document">PrintDocument 对象</param>
        /// <param name="strPaperName">纸张名字</param>
        /// <param name="bLandscape">是否为横向</param>
        /// <param name="bCheck">是否检查纸张包含在打印机的纸张列表中</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>0: 成功选定: 1: 没有选定，因为名字不可用。建议后面出现打印机对话框选定</returns>
        public static int SelectPaperByName(PrintDocument document,
            string strPaperName,
            bool bLandscape,
            bool bCheck,
            out string strError)
        {
            strError = "";

            try
            {
                PaperSize old_papersize = document.DefaultPageSettings.PaperSize;

                PaperSize paper_size = PrintUtil.BuildPaperSize(strPaperName);
                if (paper_size != null)
                {
                    // 进行检查
                    if (bCheck == true)
                    {
                        PaperSize found = null;
                        foreach (PaperSize ps in document.PrinterSettings.PaperSizes)
                        {
                            if (ps.PaperName.Equals(paper_size.PaperName)
                                && ps.Width == paper_size.Width
                                && ps.Height == paper_size.Height)
                            {
                                found = ps;
                                break;
                            }
                        }

                        if (found != null)
                        {
                            paper_size = found;
                            goto END1;
                        }

                        // 如果没有匹配的，则退而求其次，用尺寸匹配一个
                        found = null;
                        foreach (PaperSize ps in document.PrinterSettings.PaperSizes)
                        {
                            if (ps.Width == paper_size.Width
                                && ps.Height == paper_size.Height)
                            {
                                found = ps;
                                break;
                            }
                        }

                        if (found != null)
                        {
                            paper_size = found;
                            goto END1;
                        }

                        strError = "打印机 " + document.PrinterSettings.PrinterName + " 的纸张类型 " + strPaperName + " 当前不可用，请重新选定纸张";
                        return 1;
                    }

                END1:
                    document.DefaultPageSettings.PaperSize = paper_size;    // 注：直接 new PaperSize 这样赋值，会导致打印机对话框中纸张名字为空。也许可以把 PrinterSetting 里面的也修改了就可以了?
                    document.DefaultPageSettings.Landscape = bLandscape;
                }
                else
                {
                    // 试探用名字来查找
                    PaperSize found = null;
                    foreach (PaperSize ps in document.PrinterSettings.PaperSizes)
                    {
                        if (ps.PaperName.Equals(strPaperName) == true)
                        {
                            found = ps;
                            break;
                        }
                    }

                    if (found != null)
                    {
                        paper_size = found;
                        document.DefaultPageSettings.PaperSize = paper_size;
                        document.DefaultPageSettings.Landscape = bLandscape;
                        return 0;
                    }

                    strError = "打印机 " + document.PrinterSettings.PrinterName + " 的纸张类型 " + strPaperName + " 当前不可用，请重新选定纸张";
                    return 1;
                }
#if NO
            if ((string.IsNullOrEmpty(strPaperName) == false
                && strPaperName != document.DefaultPageSettings.PaperSize.PaperName)
                ||
                (bCheck == true && string.IsNullOrEmpty(strPaperName) == false))
            {
                PaperSize found = null;
                foreach (PaperSize ps in document.PrinterSettings.PaperSizes)
                {
                    if (ps.PaperName.Equals(strPaperName))
                    {
                        found = ps;
                        break;
                    }
                }

                if (found != null)
                    document.DefaultPageSettings.PaperSize = found;
                else
                {
                    strError = "打印机 " + document.PrinterSettings.PrinterName + " 的纸张类型 " + strPaperName + " 当前不可用，请重新选定纸张";
                    // document.DefaultPageSettings.PaperSize = old_papersize;
                    return 1;
                }
            }
#endif

                return 0;
            }
            catch(Exception ex)
            {
                // 2017/4/26
                strError = ex.Message;
                return -1;
            }
        }

        /// <summary>
        /// 打印或打印预览
        /// </summary>
        /// <param name="strLabelDefFileName">标签定义文件</param>
        /// <param name="strLabelFileName">标签内容文件</param>
        /// <param name="bDisplayPrinterDialog">是否显示打印机设置对话框</param>
        /// <param name="bPrintPreview">是否进行打印预览。false 表示进行打印</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int Print(
            string strLabelDefFileName,
            string strLabelFileName,
            bool bDisplayPrinterDialog,
            bool bPrintPreview,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行打印"
                + (bPrintPreview == true ? "预览" : "")
                + "</div>");

            try
            {
                nRet = this.BeginPrint(
                    strLabelFileName,
                    strLabelDefFileName,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.document.PreviewMode = bPrintPreview;

                this.EnableControls(false);
                Cursor oldCursor = this.Cursor;
                if (bPrintPreview == false)
                    this.Cursor = Cursors.WaitCursor;
                try
                {
                    bool bCustomPaper = false;

                    if (bPrintPreview == false)
                    {
                        // Allow the user to choose the page range he or she would
                        // like to print.
                        printDialog1.AllowSomePages = true;

                        // Show the help button.
                        printDialog1.ShowHelp = true;
                    }

                    printDialog1.Document = this.document;

                    if (this.PrinterInfo != null)
                    {

                        // this.OutputText("恢复以前的打印机名: " + this.PrinterInfo.PrinterName + ", 纸张名: " + this.PrinterInfo.PaperName);

#if NO
                        string strPrinterName = document.PrinterSettings.PrinterName;
                        if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == false
                            && this.PrinterInfo.PrinterName != strPrinterName)
                        {
                            this.document.PrinterSettings.PrinterName = this.PrinterInfo.PrinterName;
                            if (this.document.PrinterSettings.IsValid == false)
                            {
                                this.document.PrinterSettings.PrinterName = strPrinterName;
                                MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 当前不可用，请重新选定打印机");
                                this.PrinterInfo.PrinterName = "";
                                bDisplayPrinterDialog = true;
                            }
                        }
#endif
                        // 按照存储的打印机名选定打印机
                        nRet = SelectPrinterByName(this.document,
                            this.PrinterInfo.PrinterName,
                            out  strError);
                        if (nRet == 1)
                        {
                            MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 当前不可用，请重新选定打印机");
                            this.PrinterInfo.PrinterName = "";
                            bDisplayPrinterDialog = true;
                        }

#if NO
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
#endif

                        // 需要自定义纸张
                        if (string.IsNullOrEmpty(this.label_param.DefaultPrinter) == true
                            && this.label_param.PageWidth > 0
                            && this.label_param.PageHeight > 0)
                        {
                            bCustomPaper = true;

                            PaperSize paper_size = new PaperSize("Custom Label",
                                (int)label_param.PageWidth,
                                (int)label_param.PageHeight);
                            this.document.DefaultPageSettings.PaperSize = paper_size;
                        }


                        if (// bDisplayPrinterDialog == false && 
                            bCustomPaper == false
                            && string.IsNullOrEmpty(this.PrinterInfo.PaperName) == false)
                        {
                            nRet = SelectPaperByName(this.document,
                                this.PrinterInfo.PaperName,
                                this.PrinterInfo.Landscape,
                                true,   // false,
                                out strError);
                            if (nRet == 1)
                            {
                                MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 的纸张类型 " + this.PrinterInfo.PaperName + " 当前不可用，请重新选定纸张");
                                this.PrinterInfo.PaperName = "";
                                bDisplayPrinterDialog = true;
                            }
                        }

                        // 只要有一个打印机事项没有确定，就要出现打印机对话框
                        if (bCustomPaper == false)
                        {
                            if (string.IsNullOrEmpty(this.PrinterInfo.PrinterName) == true
                                || string.IsNullOrEmpty(this.PrinterInfo.PaperName) == true)
                                bDisplayPrinterDialog = true;
                        }
                    }
                    else
                    {
                        // 没有首选配置的情况下要出现打印对话框
                        bDisplayPrinterDialog = true;
                    }

                    // this.document.DefaultPageSettings.Landscape = label_param.Landscape;

                    DialogResult result = DialogResult.OK;
                    if (bDisplayPrinterDialog == true)
                    {
                        result = printDialog1.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            if (bCustomPaper == true)
                            {
                                PaperSize paper_size = new PaperSize("Custom Label",
                                    (int)label_param.PageWidth,
                                    (int)label_param.PageHeight);
                                this.document.DefaultPageSettings.PaperSize = paper_size;
                            }

                            // 记忆打印参数
                            if (this.PrinterInfo == null)
                                this.PrinterInfo = new PrinterInfo();

                            // this.OutputText("打印机对话框返回后，新选定的打印机名: " + document.PrinterSettings.PrinterName + ", 纸张名: " + document.DefaultPageSettings.PaperSize.PaperName);

                            this.PrinterInfo.PrinterName = document.PrinterSettings.PrinterName;
                            // this.PrinterInfo.PaperName = document.PrinterSettings.DefaultPageSettings.PaperSize.PaperName;  // document.DefaultPageSettings.PaperSize.PaperName
                            this.PrinterInfo.PaperName = PrintUtil.GetPaperSizeString(document.DefaultPageSettings.PaperSize);
                            this.PrinterInfo.Landscape = document.DefaultPageSettings.Landscape;

                            if (bCustomPaper == false)
                            {
                                // 2014/3/27
                                // this.document.DefaultPageSettings = document.PrinterSettings.DefaultPageSettings;
                                nRet = SelectPaperByName(this.document,
                                    this.PrinterInfo.PaperName,
                                    this.PrinterInfo.Landscape,
                                    true,
                                    out strError);
                                if (nRet == 1)
                                {
                                    // MessageBox.Show(this, "打印机 " + this.PrinterInfo.PrinterName + " 的纸张类型 " + this.PrinterInfo.PaperName + " 当前不可用，请重新选定纸张");
                                    //this.PrinterInfo.PaperName = "";
                                    //bDisplayPrinterDialog = true;

                                    this.OutputText("打印机对话框返回后，经过检查，纸张 " + this.PrinterInfo.PaperName + " 不在打印机 " + this.PrinterInfo.PrinterName + " 的可用纸张列表中。出现对话框让用户重新选择纸张");


                                    SelectPaperDialog paper_dialog = new SelectPaperDialog();
                                    MainForm.SetControlFont(paper_dialog, this.Font, false);
                                    paper_dialog.Comment = "纸张 " + this.PrinterInfo.PaperName + " 不在打印机 " + this.PrinterInfo.PrinterName + " 的可用纸张列表中。\r\n请重新选定纸张";
                                    paper_dialog.Document = this.document;
                                    Program.MainForm.AppInfo.LinkFormState(paper_dialog, "paper_dialog_state");
                                    paper_dialog.ShowDialog(this);
                                    Program.MainForm.AppInfo.UnlinkFormState(paper_dialog);

                                    if (paper_dialog.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                                        return 0;

                                    this.OutputText("对话框中新选定的纸张名: " + document.DefaultPageSettings.PaperSize.PaperName);
                                }
                            }

                            this.PrinterInfo.PaperName = PrintUtil.GetPaperSizeString(document.DefaultPageSettings.PaperSize);
                            this.PrinterInfo.Landscape = document.DefaultPageSettings.Landscape;

                            SetTitle();
                        }
                        else
                            return 0;
                    }

                    TracePrinterInfo();

                    if (bPrintPreview == true)
                    {
                        printPreviewDialog1.Document = this.document;

                        Program.MainForm.AppInfo.LinkFormState(printPreviewDialog1, "labelprintform_printpreviewdialog_state");
                        printPreviewDialog1.ShowDialog(this);
                        Program.MainForm.AppInfo.UnlinkFormState(printPreviewDialog1);
                    }
                    else
                    {
                        this.document.Print();
                    }
                }
                finally
                {
                    if (bPrintPreview == false)
                        this.Cursor = oldCursor;
                    this.EnableControls(true);

                    this.EndPrint();    // 关闭标签文件。后面才能删除
                }
            }
            finally
            {
                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行打印"
                + (bPrintPreview == true ? "预览" : "")
                + "</div>");
            }

            return 0;
        ERROR1:
            return -1;
        }


        // 
        /// <summary>
        /// 打印预览(根据册记录)
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机设置对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintPreviewFromItemRecords(bool bDisplayPrinterDialog = false)
        {
            string strError = "";

            // 需要先创建标签文件
            string strLabelFilename = Program.MainForm.NewTempFilename(
                "temp_labelfiles",
                "~label_");
            string strErrorFilename = Program.MainForm.NewTempFilename(
                "temp_labelfiles",
                "~error_");

            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行打印预览</div>");

            this._processing++;
            try
            {
                this.textBox_errorInfo.Text = "当前索取号来源: " + this.AccessNoSource + "\r\n\r\n";

                int nRet = BuildLabelFile(
                    strLabelFilename,
                    strErrorFilename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                FileInfo fi = new FileInfo(strErrorFilename);
                if (fi.Exists && fi.Length > 0)
                {
                    string strContent = "";
                    nRet = Global.ReadTextFileContent(strErrorFilename,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    this.textBox_errorInfo.Text += strContent;

                    DialogResult result = MessageBox.Show(this,
                        "创建标签文件的过程中有报错信息，请问是否继续进行打印预览?\r\n\r\n(Yes 继续打印预览；No 不进行打印预览)",
                        "LabelPrintForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        this.textBox_errorInfo.Focus();
                        return 0;
                    }

                }

                nRet = Print(this.textBox_labelDefFilename.Text,
                    strLabelFilename,
                    bDisplayPrinterDialog,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                if (String.IsNullOrEmpty(strLabelFilename) == false)
                {
                    try
                    {
                        File.Delete(strLabelFilename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "删除临时标签文件 '" + strLabelFilename + "' 时出错: " + ex.Message);
                    }
                }
                if (String.IsNullOrEmpty(strErrorFilename) == false)
                {
                    try
                    {
                        File.Delete(strErrorFilename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "删除临时错误文件 '" + strErrorFilename + "' 时出错: " + ex.Message);
                    }
                }

                this._processing--;
                // Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行打印预览</div>");
            }

            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // parameters:
        //      strLabelFilename    标签文件名
        //      strDefFilename  定义文件名
        int BeginPrint(
            string strLabelFilename,
            string strDefFilename,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strDefFilename) == true)
            {
                strError = "尚未指定标签定义文件名";
                return -1;
            }

            if (String.IsNullOrEmpty(strLabelFilename) == true)
            {
                strError = "尚未指定标签文件名";
                return -1;
            }

            LabelParam label_param = null;

            int nRet = LabelParam.Build(strDefFilename,
                out label_param,
                out strError);
            if (nRet == -1)
                return -1;

            this.label_param = label_param;

            if (this.document != null)
            {
                this.document.Close();
                this.document = null;
            }

            this.document = new PrintLabelDocument();
            nRet = this.document.Open(strLabelFilename,
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

        void document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            this.document.DoPrintPage(this,
                this.label_param,
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

        private void button_print_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_labelFile)
                PrintFromLabelFile();
            else if (this.tabControl_main.SelectedTab == this.tabPage_itemRecords)
                PrintFromItemRecords();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.textBox_labelFile_labelFilename.Enabled = bEnable;
            this.button_labelFile_findLabelFilename.Enabled = bEnable;

            this.textBox_labelDefFilename.Enabled = bEnable;
            this.button_findLabelDefFilename.Enabled = bEnable;

            this.button_print.Enabled = bEnable;
            this.button_printPreview.Enabled = bEnable;

            this.checkBox_testingGrid.Enabled = bEnable;

            this.Update();
        }

        // 
        /// <summary>
        /// 打印(根据标签文件)
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机设置对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintFromLabelFile(bool bDisplayPrinterDialog = true)
        {
            string strError = "";
            this._processing++;
            try
            {
                int nRet = this.Print(
                    this.textBox_labelDefFilename.Text,
                    this.textBox_labelFile_labelFilename.Text,
                    bDisplayPrinterDialog,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                return 0;
            }
            finally
            {
                this._processing--;
            }
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 
        /// <summary>
        /// 打印(根据册记录)
        /// </summary>
        /// <param name="bDisplayPrinterDialog">是否显示打印机设置对话框</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrintFromItemRecords(bool bDisplayPrinterDialog = true)
        {
            string strError = "";

            // 需要先创建标签文件
            string strLabelFilename = Program.MainForm.NewTempFilename(
                "temp_labelfiles",
                "~label_");
            string strErrorFilename = Program.MainForm.NewTempFilename(
    "temp_labelfiles",
    "~error_");

            this._processing++;
            try
            {
                this.textBox_errorInfo.Text = "";

                int nRet = BuildLabelFile(
                    strLabelFilename,
                    strErrorFilename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                FileInfo fi = new FileInfo(strErrorFilename);
                if (fi.Exists && fi.Length > 0)
                {
                    string strContent = "";
                    nRet = Global.ReadTextFileContent(strErrorFilename,
                        out strContent,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    this.textBox_errorInfo.Text = strContent;

                    DialogResult result = MessageBox.Show(this,
                        "创建标签文件的过程中有报错信息，请问是否继续进行打印?\r\n\r\n(Yes 继续打印；No 不进行打印)",
                        "LabelPrintForm",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                    {
                        this.textBox_errorInfo.Focus();
                        return 0;
                    }

                }

                nRet = Print(this.textBox_labelDefFilename.Text,
                    strLabelFilename,
                    bDisplayPrinterDialog,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            }
            finally
            {
                if (String.IsNullOrEmpty(strLabelFilename) == false)
                {
                    try
                    {
                        File.Delete(strLabelFilename);
                    }
                    catch
                    {
                    }
                }
                if (String.IsNullOrEmpty(strErrorFilename) == false)
                {
                    try
                    {
                        File.Delete(strErrorFilename);
                    }
                    catch
                    {
                    }
                }

                this._processing--;
                // Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束执行打印</div>");
            }

            return 0;
        ERROR1:
            MessageBox.Show(this, strError);
            return -1;
        }

        // 
        /// <summary>
        /// 标签(内容)文件名
        /// </summary>
        public string LabelFilename
        {
            get
            {
                return this.textBox_labelFile_labelFilename.Text;
            }
            set
            {
                this.textBox_labelFile_labelFilename.Text = value;
            }
        }

        // 
        /// <summary>
        /// 标签定义文件名
        /// </summary>
        public string LabelDefFilename
        {
            get
            {
                return this.textBox_labelDefFilename.Text;
            }
            set
            {
                this.textBox_labelDefFilename.Text = value;
            }
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bSelected = false;
            string strFirstColumn = "";
            if (this.listView_records.SelectedItems.Count > 0)
            {
                bSelected = true;
                strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            if (String.IsNullOrEmpty(strFirstColumn) == false)
            {
                string strRecPath = "";
                if (bSelected == true)
                    strRecPath = this.listView_records.SelectedItems[0].Text;

                string strOpenStyle = "新开的";
                if (this.LoadToExistDetailWindow == true)
                    strOpenStyle = "已打开的";

                menuItem = new MenuItem("打开 [根据册记录路径 '" + strRecPath + "' 装入到" + strOpenStyle + "种册窗](&O)");
                menuItem.DefaultItem = true;
                menuItem.Click += new System.EventHandler(this.listView_records_DoubleClick);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    menuItem.Enabled = false;
                contextMenu.MenuItems.Add(menuItem);

                string strBarcode = "";
                if (bSelected == true)
                {
                    string strError = "";
                    int nRet = GetItemBarcode(
    this.listView_records.SelectedItems[0],
    false,
    out strBarcode,
    out strError);
                    // strBarcode = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);
                }

                bool bExistEntityForm = (Program.MainForm.GetTopChildWindow<EntityForm>() != null);
                bool bExistItemInfoForm = (Program.MainForm.GetTopChildWindow<ItemInfoForm>() != null);

                //
                menuItem = new MenuItem("打开方式(&T)");
                contextMenu.MenuItems.Add(menuItem);

                // 第一级子菜单

                strOpenStyle = "新开的";

                // 到实体窗，记录路径
                MenuItem subMenuItem = new MenuItem("装入" + strOpenStyle + "实体窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_recPath_newly_Click);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到实体窗，条码号
                subMenuItem = new MenuItem("装入" + strOpenStyle + "实体窗，根据册条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_barcode_newly_Click);
                if (String.IsNullOrEmpty(strBarcode) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到种册窗，记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_entityForm_recPath_newly_Click);
                if (String.IsNullOrEmpty(strRecPath) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到种册窗，条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据册条码 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_entityForm_barcode_newly_Click);
                if (String.IsNullOrEmpty(strBarcode) == true)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

                strOpenStyle = "已打开的";

                // 到实体窗，记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + "实体窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_recPath_exist_Click);
                if (String.IsNullOrEmpty(strRecPath) == true
                    || bExistItemInfoForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到实体窗，条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "实体窗，根据册条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_itemInfoForm_barcode_exist_Click);
                if (String.IsNullOrEmpty(strBarcode) == true
                    || bExistItemInfoForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到种册窗，记录路径
                subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据记录路径 '" + strRecPath + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_entityForm_recPath_exist_Click);
                if (String.IsNullOrEmpty(strRecPath) == true
                    || bExistEntityForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // 到种册窗，条码
                subMenuItem = new MenuItem("装入" + strOpenStyle + "种册窗，根据册条码号 '" + strBarcode + "'");
                subMenuItem.Click += new System.EventHandler(this.menu_entityForm_barcode_exist_Click);
                if (String.IsNullOrEmpty(strBarcode) == true
                    || bExistEntityForm == false)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);
            }

            // // //

            int nPathItemCount = 0;
            int nKeyItemCount = 0;
            GetSelectedItemCount(out nPathItemCount,
                out nKeyItemCount);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutToClipboard_Click);
            if (nPathItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyToClipboard_Click);
            if (nPathItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData != null
                && (iData.GetDataPresent(typeof(string)) == true
                || iData.GetDataPresent(typeof(ClipboardBookItemCollection)) == true)
                )
                bHasClipboardObject = true;
            else
                bHasClipboardObject = false;

            menuItem = new MenuItem("粘贴[前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertBefore_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴[后插](&V)");
            menuItem.Click += new System.EventHandler(this.menu_pasteFromClipboard_insertAfter_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导出到条码号文件 [" + nPathItemCount.ToString() + "] (&B)");
            menuItem.Click += new System.EventHandler(this.menu_exportBarcodeFile_Click);
            if (nPathItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导出到记录路径文件 [" + nPathItemCount.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveToRecordPathFile_Click);
            if (nPathItemCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导出到文本文件 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&T)");
            menuItem.Click += new System.EventHandler(this.menu_exportTextFile_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("从记录路径文件中导入(&I)");
            menuItem.Click += new System.EventHandler(this.menu_importFromRecPathFile_Click);
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("从条码号文件中导入(&R)...");
            menuItem.Click += new System.EventHandler(this.menu_importFromBarcodeFile_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("移除所选择事项 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除列表(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearList_Click);
            if (this.listView_records.Items.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        void menu_selectAllLines_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_records.Items.Count; i++)
            {
                this.listView_records.Items[i].Selected = true;
            }
        }

        void menu_copyToClipboard_Click(object sender, EventArgs e)
        {
            CopyLinesToClipboard(false);
        }

        void menu_cutToClipboard_Click(object sender, EventArgs e)
        {
            CopyLinesToClipboard(true);
        }

        // parameters:
        //      bCut    是否为剪切
        void CopyLinesToClipboard(bool bCut)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            List<int> indices = new List<int>();
            string strTotal = "";
            for (int i = 0; i < this.listView_records.SelectedIndices.Count; i++)
            {
                int index = this.listView_records.SelectedIndices[i];

                ListViewItem item = this.listView_records.Items[index];
                string strLine = Global.BuildLine(item);
                strTotal += strLine + "\r\n";

                if (bCut == true)
                    indices.Add(index);
            }

            Clipboard.SetDataObject(strTotal, true);

            if (bCut == true)
            {
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    int index = indices[i];
                    this.listView_records.Items.RemoveAt(index);
                }
            }

            this.Cursor = oldCursor;
        }

        void menu_pasteFromClipboard_insertBefore_Click(object sender, EventArgs e)
        {
            PasteLines(true);
        }

        void menu_pasteFromClipboard_insertAfter_Click(object sender, EventArgs e)
        {
            PasteLines(false);
        }

        // parameters:
        //      bInsertBefore   是否前插? 如果==true前插，否则后插
        void PasteLines(bool bInsertBefore)
        {
            string strError = "";

            IDataObject ido = Clipboard.GetDataObject();

            if (ido == null)
            {
                strError = "剪贴板中没有内容";
                goto ERROR1;
            }

            if (ido.GetDataPresent(typeof(ClipboardBookItemCollection)) == true)
            {
                ClipboardBookItemCollection clipbookitems = (ClipboardBookItemCollection)ido.GetData(typeof(ClipboardBookItemCollection));
                if (clipbookitems == null)
                {
                    strError = "iData.GetData() return null";
                    goto ERROR1;
                }

                clipbookitems.RestoreNonSerialized();

                int index = -1;

                if (this.listView_records.SelectedIndices.Count > 0)
                    index = this.listView_records.SelectedIndices[0];

                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                this.listView_records.SelectedItems.Clear();
                for (int i = 0; i < clipbookitems.Count; i++)
                {
                    BookItem bookitem = clipbookitems[i];

                    string strBarcode = bookitem.Barcode;
                    string strRecPath = bookitem.RecPath;

                    ListViewItem item = new ListViewItem();
                    item.Text = strRecPath;
                    item.SubItems.Add(strBarcode);

                    if (index == -1)
                        this.listView_records.Items.Add(item);
                    else
                    {
                        if (bInsertBefore == true)
                            this.listView_records.Items.Insert(index, item);
                        else
                            this.listView_records.Items.Insert(index + 1, item);

                        index++;
                    }

                    item.Selected = true;

                }

                this.Cursor = oldCursor;
                return;
            }
            else if (ido.GetDataPresent(typeof(string)) == true)
            {
                string strWhole = (string)ido.GetData(DataFormats.UnicodeText);

                /*
                int index = -1;

                if (this.listView_records.SelectedIndices.Count > 0)
                    index = this.listView_records.SelectedIndices[0];

                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                this.listView_records.SelectedItems.Clear();

                string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    ListViewItem item = Global.BuildListViewItem(
                        this.listView_records,
                        lines[i]);

                    if (index == -1)
                        this.listView_records.Items.Add(item);
                    else
                    {
                        if (bInsertBefore == true)
                            this.listView_records.Items.Insert(index, item);
                        else
                            this.listView_records.Items.Insert(index + 1, item);

                        index++;
                    }

                    item.Selected = true;
                }

                this.Cursor = oldCursor;
                 * */
                DoPasteTabbedText(strWhole,
                    bInsertBefore);
                return;
            }
            else
            {
                strError = "剪贴板中既不存在ClipboardBookItemCollection类型的内容，也不存在string类型内容";
                goto ERROR1;
            }

            //return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void DoPasteTabbedText(string strWhole,
            bool bInsertBefore)
        {
            int index = -1;

            if (this.listView_records.SelectedIndices.Count > 0)
                index = this.listView_records.SelectedIndices[0];

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            this.listView_records.SelectedItems.Clear();

            string[] lines = strWhole.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                ListViewItem item = Global.BuildListViewItem(
                    this.listView_records,
                    lines[i]);

                if (index == -1)
                    this.listView_records.Items.Add(item);
                else
                {
                    if (bInsertBefore == true)
                        this.listView_records.Items.Insert(index, item);
                    else
                        this.listView_records.Items.Insert(index + 1, item);

                    index++;
                }

                item.Selected = true;
            }

            this.Cursor = oldCursor;
        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_records, e);
        }

        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnListViewSelectedIndexChanged(sender, e);
        }


        private void listView_records_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要操作的事项");
                return;
            }

            string strFirstColumn = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);

            if (String.IsNullOrEmpty(strFirstColumn) == false)
            {
                string strOpenStyle = "new";
                if (this.LoadToExistDetailWindow == true)
                    strOpenStyle = "exist";

                // 装入种册窗/实体窗，用册条码号/记录路径
                // parameters:
                //      strTargetFormType   目标窗口类型 "EntityForm" "ItemInfoForm"
                //      strIdType   标识类型 "barcode" "recpath"
                //      strOpenType 打开窗口的方式 "new" "exist"
                LoadRecord("EntityForm",
                    "recpath",
                    strOpenStyle);
            }
            else
            {
                MessageBox.Show(this, "第一列不能为空");
            }
        }

        // 
        /// <summary>
        /// 是否优先装入已经打开的详细窗?
        /// </summary>
        public bool LoadToExistDetailWindow
        {
            get
            {
                return Program.MainForm.AppInfo.GetBoolean(
                    "all_search_form",
                    "load_to_exist_detailwindow",
                    true);
            }
        }

        // 装入种册窗/实体窗，用册条码号/记录路径
        // parameters:
        //      strTargetFormType   目标窗口类型 "EntityForm" "ItemInfoForm"
        //      strIdType   标识类型 "barcode" "recpath"
        //      strOpenType 打开窗口的方式 "new" "exist"
        void LoadRecord(string strTargetFormType,
            string strIdType,
            string strOpenType)
        {
            string strTargetFormName = "种册窗";
            if (strTargetFormType == "ItemInfoForm")
                strTargetFormName = "实体窗";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入" + strTargetFormName + "的行");
                return;
            }

            string strBarcodeOrRecPath = "";

            if (strIdType == "barcode")
            {
                // barcode
                // strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 1);

                string strError = "";
                // 根据 ListViewItem 对象，获得册条码号列的内容
                int nRet = GetItemBarcode(
                    this.listView_records.SelectedItems[0],
                    true,
                    out strBarcodeOrRecPath,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            else
            {
                Debug.Assert(strIdType == "recpath", "");
                // recpath
                strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_records.SelectedItems[0], 0);
            }

            if (strTargetFormType == "EntityForm")
            {
                EntityForm form = null;

                if (strOpenType == "exist")
                {
                    form = MainForm.GetTopChildWindow<EntityForm>();
                    if (form != null)
                        Global.Activate(form);
                }
                else
                {
                    Debug.Assert(strOpenType == "new", "");
                }

                if (form == null)
                {
                    form = new EntityForm();

                    form.MdiParent = Program.MainForm;

                    form.MainForm = Program.MainForm;
                    form.Show();
                }

                if (strIdType == "barcode")
                {
                    // 装载一个册，连带装入种
                    // parameters:
                    //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    form.LoadItemByBarcode(strBarcodeOrRecPath, false);
                }
                else
                {
                    Debug.Assert(strIdType == "recpath", "");

                    // parameters:
                    //      bAutoSavePrev   是否自动提交保存先前发生过的修改？如果==true，是；如果==false，则要出现MessageBox提示
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    form.LoadItemByRecPath(strBarcodeOrRecPath, false);
                }
            }
            else
            {
                Debug.Assert(strTargetFormType == "ItemInfoForm", "");

                ItemInfoForm form = null;

                if (strOpenType == "exist")
                {
                    form = MainForm.GetTopChildWindow<ItemInfoForm>();
                    if (form != null)
                        Global.Activate(form);
                }
                else
                {
                    Debug.Assert(strOpenType == "new", "");
                }

                if (form == null)
                {
                    form = new ItemInfoForm();

                    form.MdiParent = Program.MainForm;

                    form.MainForm = Program.MainForm;
                    form.Show();
                }

                if (strIdType == "barcode")
                {
                    form.LoadRecord(strBarcodeOrRecPath);
                }
                else
                {
                    Debug.Assert(strIdType == "recpath", "");

                    form.LoadRecordByRecPath(strBarcodeOrRecPath, "");
                }
            }
        }

        void menu_itemInfoForm_recPath_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "recpath",
                "new");
        }

        void menu_itemInfoForm_barcode_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "barcode",
                "new");
        }

        void menu_entityForm_recPath_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "recpath",
                "new");
        }

        void menu_entityForm_barcode_newly_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "barcode",
                "new");
        }

        void menu_itemInfoForm_recPath_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "recpath",
                "exist");
        }

        void menu_itemInfoForm_barcode_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("ItemInfoForm",
                "barcode",
                "exist");
        }

        void menu_entityForm_recPath_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "recpath",
                "exist");
        }

        void menu_entityForm_barcode_exist_Click(object sender, EventArgs e)
        {
            LoadRecord("EntityForm",
                "barcode",
                "exist");
        }

        void GetSelectedItemCount(out int nPathItemCount,
    out int nKeyItemCount)
        {
            nPathItemCount = 0;
            nKeyItemCount = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                if (String.IsNullOrEmpty(item.Text) == false)
                    nPathItemCount++;
                else
                    nKeyItemCount++;
            }
        }

        void menu_clearList_Click(object sender, EventArgs e)
        {
            ClearListViewItems();
        }

        // TODO: 优化速度
        void menu_importFromBarcodeFile_Click(object sender, EventArgs e)
        {
            SetStatusMessage("");   // 清除以前残留的显示

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.m_strUsedBarcodeFilename;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedBarcodeFilename = dlg.FileName;

            StreamReader sr = null;
            string strError = "";

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + dlg.FileName + " 失败: " + ex.Message;
                goto ERROR1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入条码号 ...");
            stop.BeginLoop();

            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);


                if (this.listView_records.Items.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this.listView_records.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        this.DbType + "SearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                stop.SetProgressRange(0, sr.BaseStream.Length);

                List<ListViewItem> items = new List<ListViewItem>();

                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }

                    string strBarcode = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);


                    if (strBarcode == null)
                        break;

                    // 

                    ListViewItem item = new ListViewItem();
                    item.Text = "";
                    // ListViewUtil.ChangeItemText(item, 1, strBarcode);

                    this.listView_records.Items.Add(item);

                    FillLineByBarcode(
                        this.Channel,
                        strBarcode, 
                        item);

                    items.Add(item);
                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    this.Channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2014/1/15
                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    this.Channel,
                    items,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                if (sr != null)
                    sr.Close();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ImportFromRecPathFile(null,
                "clear",
                out strError);
            if (nRet == -1)
                goto ERROR1;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

#if NO
        // 从记录路径文件中导入
        void menu_importFromRecPathFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的册记录路径文件名";
            dlg.FileName = this.m_strUsedRecPathFilename;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.m_strUsedRecPathFilename = dlg.FileName;

            StreamReader sr = null;
            string strError = "";

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(dlg.FileName, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + dlg.FileName + " 失败: " + ex.Message;
                goto ERROR1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入记录路径 ...");
            stop.BeginLoop();


            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this.listView_records);


                if (this.listView_records.Items.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this.listView_records.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        "LabelPrintForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            MessageBox.Show(this, "用户中断");
                            return;
                        }
                    }

                    string strRecPath = sr.ReadLine();

                    if (strRecPath == null)
                        break;

                    // TODO: 检查路径的正确性，检查数据库是否为实体库之一

                    ListViewItem item = new ListViewItem();
                    item.Text = strRecPath;

                    this.listView_records.Items.Add(item);
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                // stop.HideProgress();

                if (sr != null)
                    sr.Close();
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

#if NO
        // 导出选择的行中有路径的部分行 的条码栏内容 为条码号文件
        void menu_exportBarcodeFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的条码号文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportBarcodeFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportBarcodeFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportBarcodeFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "条码号文件 '" + this.ExportBarcodeFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    "LabelPrintForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportBarcodeFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (String.IsNullOrEmpty(item.Text) == true)
                        continue;
                    string strBarcode = ListViewUtil.GetItemText(item, 1);
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;
                    sw.WriteLine(strBarcode);   // BUG!!!
                }

                this.Cursor = oldCursor;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "册条码号 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBarcodeFilename;
        }
#endif

        // 导出选择的行中有路径的部分行 的条码栏内容 为条码号文件
        void menu_exportBarcodeFile_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.DbType == "item", "");

            string strError = "";

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的条码号文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportBarcodeFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportBarcodeFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportBarcodeFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文件 '" + this.ExportBarcodeFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    this.DbType + "SearchForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // m_tableBarcodeColIndex.Clear();
            ClearColumnIndexCache();

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportBarcodeFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (String.IsNullOrEmpty(item.Text) == true)
                        continue;

#if NO
                    string strRecPath = ListViewUtil.GetItemText(item, 0);
                    // 根据记录路径获得数据库名
                    string strItemDbName = Global.GetDbName(strRecPath);
                    // 根据数据库名获得 册条码号 列号

                    int nCol = -1;
                    object o = m_tableBarcodeColIndex[strItemDbName];
                    if (o == null)
                    {
                        ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                        nCol = temp.FindColumnByType("item_barcode");
                        if (nCol == -1)
                        {
                            // 这个实体库没有在 browse 文件中 册条码号 列
                            strError = "警告：实体库 '"+strItemDbName+"' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                            MessageBox.Show(this, strError);

                            nCol = 0;   // 这个大部分情况能奏效
                        }
                        if (m_bBiblioSummaryColumn == false)
                            nCol += 1;
                        else 
                            nCol += 2;

                        m_tableBarcodeColIndex[strItemDbName] = nCol;   // 储存起来
                    }
                    else
                        nCol = (int)o;

                    Debug.Assert(nCol > 0, "");

                    string strBarcode = ListViewUtil.GetItemText(item, nCol);
#endif

                    string strBarcode = "";
                    // 根据 ListViewItem 对象，获得册条码号列的内容
                    int nRet = GetItemBarcode(
                        item,
                        true,
                        out strBarcode,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (String.IsNullOrEmpty(strBarcode) == true)
                        continue;
                    sw.WriteLine(strBarcode);   // BUG!!!
                }

            }
            finally
            {
                this.Cursor = oldCursor;
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "册条码号 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportBarcodeFilename;
            return;
        ERROR1:
            MessageBox.Show(strError);
        }

        // 保存选择的行中的有路径的部分行 到记录路径文件
        void menu_saveToRecordPathFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的记录路径文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportRecPathFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportRecPathFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportRecPathFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "记录路径文件 '" + this.ExportRecPathFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    "LabelPrintForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (String.IsNullOrEmpty(item.Text) == true)
                        continue;
                    sw.WriteLine(item.Text);
                }

                this.Cursor = oldCursor;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "册记录路径 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;


        }


        // 保存选择的行到文本文件
        void menu_exportTextFile_Click(object sender, EventArgs e)
        {
            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的文本文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = this.ExportTextFilename;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "文本文件 (*.txt)|*.txt|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.ExportTextFilename = dlg.FileName;

            bool bAppend = true;

            if (File.Exists(this.ExportTextFilename) == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "文本文件 '" + this.ExportTextFilename + "' 已经存在。\r\n\r\n本次输出内容是否要追加到该文件尾部? (Yes 追加；No 覆盖；Cancel 放弃导出)",
                    "LabelPrintForm",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.No)
                    bAppend = false;
                else if (result == DialogResult.Yes)
                    bAppend = true;
                else
                {
                    Debug.Assert(false, "");
                }
            }
            else
                bAppend = false;

            // 创建文件
            StreamWriter sw = new StreamWriter(this.ExportTextFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8);
            try
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    string strLine = Global.BuildLine(item);
                    sw.WriteLine(strLine);
                }

                this.Cursor = oldCursor;
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "行内容 " + this.listView_records.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文本文件 " + this.ExportTextFilename;
        }

        // 从窗口中移走所选择的事项
        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            for (int i = this.listView_records.SelectedIndices.Count - 1; i >= 0; i--)
            {
                this.listView_records.Items.RemoveAt(this.listView_records.SelectedIndices[i]);
            }

            this.Cursor = oldCursor;
        }

#if NO
        void ClearListViewItems()
        {
            this.listView_records.Items.Clear();
            ListViewUtil.ClearSortColumns(this.listView_records);
        }
#endif

        private void LabelPrintForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);
        }

        private void textBox_errorInfo_DoubleClick(object sender, EventArgs e)
        {
            if (textBox_errorInfo.Lines.Length == 0)
                return;

            int x = 0;
            int y = 0;
            API.GetEditCurrentCaretPos(
                textBox_errorInfo,
                out x,
                out y);

            string strLine = textBox_errorInfo.Lines[y];

            // 析出册记录路径。在 {} 中
            int nRet = strLine.IndexOf("{");
            if (nRet == -1)
                goto ERROR1;

            string strRecPath = strLine.Substring(nRet + 1).Trim();
            nRet = strRecPath.IndexOf("}");
            if (nRet != -1)
                strRecPath = strRecPath.Substring(0, nRet).Trim();

            // 选定listview中那一行
            ListViewItem item = ListViewUtil.FindItem(this.listView_records,
                strRecPath,
                0);
            if (item == null)
                goto ERROR1;

            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();

            return;
        ERROR1:
            Console.Beep();
        }

        /// <summary>
        /// 激活“标签文件”属性页
        /// </summary>
        public void ActivateLabelFilePage()
        {
            this.tabControl_main.SelectedTab = this.tabPage_labelFile;
        }

        /// <summary>
        /// 激活“册记录”属性页
        /// </summary>
        public void ActivateItemRecordsPage()
        {
            this.tabControl_main.SelectedTab = this.tabPage_itemRecords;
        }

        // 在窗口打开前TestingGrid是否设置过
        bool m_bTestingGridSetted = false;

        //  
        /// <summary>
        /// 是否打印测试线
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

        private void listView_records_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Text"))
            {
                e.Effect = DragDropEffects.Link;
            }
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView_records_DragDrop(object sender, DragEventArgs e)
        {
            // string strError = "";

            string strWhole = (String)e.Data.GetData("Text");

            DoPasteTabbedText(strWhole,
                false);
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        private void button_editDefFile_Click(object sender, EventArgs e)
        {
            // string strError = "";

#if NO
            if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
            {
                strError = "请先指定标签定义文件名";
                goto ERROR1;
            }
#endif
            string strOldFileName = this.textBox_labelDefFilename.Text;

            LabelDesignForm dlg = new LabelDesignForm();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.DefFileName = this.textBox_labelDefFilename.Text;
            if (string.IsNullOrEmpty(this.textBox_labelFile_content.Text) == false)
                dlg.SampleLabelText = this.textBox_labelFile_content.Text;

            dlg.UiState = Program.MainForm.AppInfo.GetString(
                    "label_print_form",
                    "LabelDesignForm_uiState",
                    "");

            Program.MainForm.AppInfo.LinkFormState(dlg, "LabelDesignForm_state");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            Program.MainForm.AppInfo.SetString(
        "label_print_form",
        "LabelDesignForm_uiState",
        dlg.UiState);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            // 文件名在设计对话框中发生了变化
            if (string.IsNullOrEmpty(strOldFileName) == false
                && strOldFileName != dlg.DefFileName)
            {
                DialogResult result = MessageBox.Show(this,
"您在标签设计对话框中装载了新的标签定义文件名 '" + dlg.DefFileName + "'。\r\n\r\n请问要把这个标签定义文件应用到当前窗口么? \r\n\r\n(Yes: 应用新的文件名; No: 保持以前的文件名不变)",
"LabePrintForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    this.textBox_labelDefFilename.Text = dlg.DefFileName;
            }
            else
                this.textBox_labelDefFilename.Text = dlg.DefFileName;

            // 即便前后文件名没有变化，也要触发一次刷新
            if (strOldFileName == this.textBox_labelDefFilename.Text)
            {
                textBox_labelDefFilename_TextChanged(this, new EventArgs());
            }
            return;
#if NO
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        // TODO: delay
        private void textBox_labelDefFilename_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
                this.button_editDefFile.Text = "创建";
            else
                this.button_editDefFile.Text = "设计";

            // 试图从文件中取得打印机信息，并显示在窗口标题上
            LoadPrinterInfo();
        }

        // 试图从标签定义文件中取得打印机信息，并显示在窗口标题上
        void LoadPrinterInfo()
        {
            string strError = "";

            if (String.IsNullOrEmpty(this.textBox_labelDefFilename.Text) == true)
            {
                strError = "尚未指定标签定义文件名";
                goto ERROR1;
            }

            LabelParam label_param = null;
            int nRet = LabelParam.Build(this.textBox_labelDefFilename.Text,
                out label_param,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 记忆打印参数
            if (string.IsNullOrEmpty(label_param.DefaultPrinter) == false)
            {
                PrinterInfo temp = new PrinterInfo("缺省标签", label_param.DefaultPrinter);
                // 如果 label_param 中 Landscape 和 DefaultPrinter 不一致
                // temp.Landscape = label_param.Landscape;
                this.PrinterInfo = temp;
            }

            return;
        ERROR1:
            return;
        }

        // 从何处获取索取号
        /*
从册记录
从书目记录
顺次从册记录、书目记录
         * */
        string AccessNoSource
        {
            get
            {
                return Program.MainForm.AppInfo.GetString(
                "labelprint",
                "accessNo_source",
                "从册记录");
            }
        }

        internal override bool InSearching
        {
            get
            {
                return false;
            }
        }
    }
}