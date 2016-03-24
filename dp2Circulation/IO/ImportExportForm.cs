using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class ImportExportForm : BatchPrintFormBase
    {
        public ImportExportForm()
        {
            InitializeComponent();
        }

        private void ImportExportForm_Load(object sender, EventArgs e)
        {
            CreateColumnHeader(this.listView_in);
        }

        private void ImportExportForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ImportExportForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        // 设置listview栏目标题
        void CreateColumnHeader(ListView list)
        {
            ColumnHeader columnHeader_barcode = new ColumnHeader();
            ColumnHeader columnHeader_isbnIssn = new ColumnHeader();
            ColumnHeader columnHeader_state = new ColumnHeader();
            ColumnHeader columnHeader_location = new ColumnHeader();
            ColumnHeader columnHeader_price = new ColumnHeader();
            ColumnHeader columnHeader_bookType = new ColumnHeader();
            ColumnHeader columnHeader_comment = new ColumnHeader();
            ColumnHeader columnHeader_borrower = new ColumnHeader();
            ColumnHeader columnHeader_borrowDate = new ColumnHeader();
            ColumnHeader columnHeader_borrowPeriod = new ColumnHeader();
            ColumnHeader columnHeader_recpath = new ColumnHeader();
            ColumnHeader columnHeader_errorInfo = new ColumnHeader();
            ColumnHeader columnHeader_registerNo = new ColumnHeader();
            ColumnHeader columnHeader_mergeComment = new ColumnHeader();
            ColumnHeader columnHeader_batchNo = new ColumnHeader();
            ColumnHeader columnHeader_biblioRecpath = new ColumnHeader();
            ColumnHeader columnHeader_accessno = new ColumnHeader();

            // 2009/10/27
            ColumnHeader columnHeader_targetRecpath = new ColumnHeader();

            list.Columns.Clear();

            list.Columns.AddRange(new ColumnHeader[] {
            columnHeader_barcode,
            columnHeader_errorInfo,
            columnHeader_isbnIssn,
            columnHeader_state,
            columnHeader_location,
            columnHeader_price,
            columnHeader_bookType,
            columnHeader_registerNo,
            columnHeader_comment,
            columnHeader_mergeComment,
            columnHeader_batchNo,
            columnHeader_borrower,
            columnHeader_borrowDate,
            columnHeader_borrowPeriod,
            columnHeader_recpath,
            columnHeader_biblioRecpath,
            columnHeader_accessno,
            columnHeader_targetRecpath});

            // 
            // columnHeader_barcode
            // 
            columnHeader_barcode.Text = "册条码号";
            columnHeader_barcode.Width = 150;
            // 
            // columnHeader_errorInfo
            // 
            columnHeader_errorInfo.Text = "摘要/错误信息";
            columnHeader_errorInfo.Width = 200;
            // 
            // columnHeader_isbnIssn
            // 
            columnHeader_isbnIssn.Text = "ISBN/ISSN";
            columnHeader_isbnIssn.Width = 160;
            // 
            // columnHeader_state
            // 
            columnHeader_state.Text = "状态";
            columnHeader_state.Width = 100;
            // 
            // columnHeader_location
            // 
            columnHeader_location.Text = "馆藏地点";
            columnHeader_location.Width = 150;
            // 
            // columnHeader_price
            // 
            columnHeader_price.Text = "册价格";
            columnHeader_price.Width = 150;
            // 
            // columnHeader_bookType
            // 
            columnHeader_bookType.Text = "册类型";
            columnHeader_bookType.Width = 150;
            // 
            // columnHeader_registerNo
            // 
            columnHeader_registerNo.Text = "登录号";
            columnHeader_registerNo.Width = 150;
            // 
            // columnHeader_comment
            // 
            columnHeader_comment.Text = "附注";
            columnHeader_comment.Width = 150;
            // 
            // columnHeader_mergeComment
            // 
            columnHeader_mergeComment.Text = "合并注释";
            columnHeader_mergeComment.Width = 150;
            // 
            // columnHeader_batchNo
            // 
            columnHeader_batchNo.Text = "批次号";
            // 
            // columnHeader_borrower
            // 
            columnHeader_borrower.Text = "借阅者";
            columnHeader_borrower.Width = 150;
            // 
            // columnHeader_borrowDate
            // 
            columnHeader_borrowDate.Text = "借阅日期";
            columnHeader_borrowDate.Width = 150;
            // 
            // columnHeader_borrowPeriod
            // 
            columnHeader_borrowPeriod.Text = "借阅期限";
            columnHeader_borrowPeriod.Width = 150;
            // 
            // columnHeader_recpath
            // 
            columnHeader_recpath.Text = "册记录路径";
            columnHeader_recpath.Width = 200;
            // 
            // columnHeader_biblioRecpath
            // 
            columnHeader_biblioRecpath.Text = "种记录路径";
            columnHeader_biblioRecpath.Width = 200;
            // 
            // columnHeader_accessno
            // 
            columnHeader_accessno.Text = "索取号";
            columnHeader_accessno.Width = 200;
            // 
            // columnHeader_targetRecpath
            // 
            columnHeader_targetRecpath.Text = "目标书目记录路径";
            columnHeader_targetRecpath.Width = 200;
        }

        /// <summary>
        /// 最近使用过的条码文件全路径
        /// </summary>
        public string BarcodeFilePath = "";
        /// <summary>
        /// 最近使用过的记录路径文件全路径
        /// </summary>
        public string RecPathFilePath = "";

        private void button_load_loadFromBarcodeFile_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            bool bClearBefore = true;
            if (Control.ModifierKeys == Keys.Control)
                bClearBefore = false;

            if (bClearBefore == true)
                ClearBefore();
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要打开的条码号文件名";
            dlg.FileName = this.BarcodeFilePath;
            dlg.Filter = "条码号文件 (*.txt)|*.txt|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            // this.SourceStyle = "barcodefile";

            int nDupCount = 0;
            string strRecPathFilename = Path.GetTempFileName();
            try
            {
                nRet = ConvertBarcodeFile(dlg.FileName,
                    strRecPathFilename,
                    out nDupCount,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoadFromRecPathFile(strRecPathFilename,
                    "图书", // this.comboBox_load_type.Text,
                    true,
                    new string[] { "summary", "@isbnissn", "targetrecpath" },
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                if (string.IsNullOrEmpty(strRecPathFilename) == false)
                {
                    File.Delete(strRecPathFilename);
                    strRecPathFilename = "";
                }
            }

            if (nDupCount != 0)
            {
                MessageBox.Show(this, "装入过程中有 " + nDupCount.ToString() + "个重复条码事项被忽略。");
            }

#if NO
            // 记忆文件名
            this.BarcodeFilePath = dlg.FileName;
            this.Text = "典藏移交 " + Path.GetFileName(this.BarcodeFilePath);

            // 汇报数据装载情况。
            // return:
            //      0   尚未装载任何数据    
            //      1   装载已经完成
            //      2   虽然装载了数据，但是其中有错误事项
            int nState = ReportLoadState(out strError);
            if (nState != 1)
                goto ERROR1;
#endif
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            // load page
            // this.comboBox_load_type.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_loadFromBatchNo.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_loadFromBarcodeFile.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_loadFromRecPathFile.Enabled = this.ScanMode == true ? false : bEnable;
            this.button_load_scanBarcode.Enabled = this.ScanMode == true ? false : bEnable;

        }

        bool _scanMode = false;

        /// <summary>
        /// 是否处在扫描条码的状态？
        /// </summary>
        public bool ScanMode
        {
            get
            {
                return this._scanMode;
            }
            set
            {
                if (this._scanMode == value)
                    return;

                this._scanMode = value;

                // this.comboBox_load_type.Enabled = !this._scanMode;
                this.button_load_loadFromBarcodeFile.Enabled = !this._scanMode;
                this.button_load_loadFromBatchNo.Enabled = !this._scanMode;
                this.button_load_loadFromRecPathFile.Enabled = !this._scanMode;
                this.button_load_scanBarcode.Enabled = !this._scanMode;

                if (this._scanMode == false)
                {
                    if (this._scanBarcodeForm != null)
                        this._scanBarcodeForm.Close();
                }
                else
                {
                    button_load_scanBarcode_Click(this, new EventArgs());
                }
            }
        }

        ScanBarcodeForm _scanBarcodeForm = null;

        private void button_load_scanBarcode_Click(object sender, EventArgs e)
        {

        }

        // 检查路径所从属书目库是否为图书/期刊库？
        // return:
        //      -1  error
        //      0   不符合要求。提示信息在strError中
        //      1   符合要求
        internal override int CheckItemRecPath(string strPubType,
            string strItemRecPath,
            out string strError)
        {
            strError = "";

            string strItemDbName = Global.GetDbName(strItemRecPath);
            string strBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(strItemDbName);
            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "实体库 '" + strItemDbName + "' 未找到对应的书目库名";
                return -1;
            }

            return 1;
#if NO
            string strIssueDbName = this.MainForm.GetIssueDbName(strBiblioDbName);

            if (strPubType == "图书")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == false)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为期刊型，和当前出版物类型 '" + strPubType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            if (strPubType == "连续出版物")
            {
                if (String.IsNullOrEmpty(strIssueDbName) == true)
                {
                    strError = "路径 '" + strItemRecPath + "' 所从属的书目库 '" + strBiblioDbName + "' 为图书型，和当前出版物类型 '" + strPubType + "' 不一致";
                    return 0;
                }
                return 1;
            }

            strError = "CheckItemRecPath() 未知的出版物类型 '" + strPubType + "'";
            return -1;
#endif
        }

        // 处理一小批记录的装入
        internal override int DoLoadRecords(List<string> lines,
            List<ListViewItem> items,
            bool bFillSummaryColumn,
            string[] summary_col_names,
            out string strError)
        {
            strError = "";

#if DEBUG
            if (items != null)
            {
                Debug.Assert(lines.Count == items.Count, "");
            }
#endif

            List<DigitalPlatform.LibraryClient.localhost.Record> records = new List<DigitalPlatform.LibraryClient.localhost.Record>();

            // 集中获取全部册记录信息
            for (; ; )
            {
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断1";
                    return -1;
                }

                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                string[] paths = new string[lines.Count];
                lines.CopyTo(paths);
            REDO_GETRECORDS:
                long lRet = this.Channel.GetBrowseRecords(
                    this.stop,
                    paths,
                    "id,xml,timestamp", // 注意，包含了 timestamp
                    out searchresults,
                    out strError);
                if (lRet == -1)
                {
                    if (this.InvokeRequired == false)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        this.FormCaption,
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETRECORDS;
                    }
                    return -1;
                }

                records.AddRange(searchresults);

                // 去掉已经做过的一部分
                lines.RemoveRange(0, searchresults.Length);

                if (lines.Count == 0)
                    break;
            }

            // 准备 DOM 和书目摘要等
            List<RecordInfo> infos = null;
            int nRet = GetSummaries(
                bFillSummaryColumn,
                summary_col_names,
                records,
                out infos,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(records.Count == infos.Count, "");

            List<dp2Circulation.AccountBookForm.OrderInfo> orderinfos = new List<dp2Circulation.AccountBookForm.OrderInfo>();

            if (this.InvokeRequired == false)
                this.listView_in.BeginUpdate();
            try
            {
                for (int i = 0; i < infos.Count; i++)
                {
                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断1";
                            return -1;
                        }
                    }

                    RecordInfo info = infos[i];

                    if (info.Record.RecordBody == null)
                    {
                        strError = "请升级 dp2Kernel 到最新版本";
                        return -1;
                    }
                    // stop.SetMessage("正在装入路径 " + strLine + " 对应的记录...");

                    string strOutputItemRecPath = "";
                    ListViewItem item = null;

                    if (items != null)
                        item = items[i];

                    // 根据册条码号，装入册记录
                    // return: 
                    //      -2  册条码号已经在list中存在了
                    //      -1  出错
                    //      1   成功
                    nRet = LoadOneItem(
                        "图书",   // this.LoadType,  // this.comboBox_load_type.Text,
                        bFillSummaryColumn,
                        summary_col_names,
                        "@path:" + info.Record.Path,
                        info,
                        this.listView_in,
                        null,
                        out strOutputItemRecPath,
                        ref item,
                        out strError);


#if NO
                    // 准备装入订购信息
                    if (nRet != -1 && this.checkBox_load_fillOrderInfo.Checked == true)
                    {
                        Debug.Assert(item != null, "");
                        string strRefID = ListViewUtil.GetItemText(item, COLUMN_REFID);
                        if (String.IsNullOrEmpty(strRefID) == false)
                        {
                            OrderInfo order_info = new OrderInfo();
                            order_info.ItemRefID = strRefID;
                            order_info.ListViewItem = item;
                            orderinfos.Add(order_info);
                        }
                    }
#endif

                }
            }
            finally
            {
                if (this.InvokeRequired == false)
                    this.listView_in.EndUpdate();
            }

#if NO
            // 从服务器获得订购记录的路径
            if (orderinfos.Count > 0)
            {
                nRet = LoadOrderInfo(
                    orderinfos,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif

            return 0;
        }
    }
}
