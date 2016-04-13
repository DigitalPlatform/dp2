using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 检查借阅信息窗
    /// </summary>
    public partial class CheckBorrowInfoForm : MyForm
    {
#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        DigitalPlatform.Stop stop = null;
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckBorrowInfoForm()
        {
            InitializeComponent();
        }

        private void CheckBorrowInfoForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            Global.ClearForPureTextOutputing(this.webBrowser_resultInfo);

            this.checkBox_displayPriceString.Checked = this.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "display_price_string",
                true);

            this.checkBox_forceCNY.Checked = this.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "force_cny",
                false);

            this.checkBox_overwriteExistPrice.Checked = this.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "overwrite_exist_price",
                false);

        }

        private void CheckBorrowInfoForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void CheckBorrowInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                this.MainForm.AppInfo.SetBoolean(
                    "check_borrowinfo_form",
                    "display_price_string",
                    this.checkBox_displayPriceString.Checked);

                this.MainForm.AppInfo.SetBoolean(
                    "check_borrowinfo_form",
                    "force_cny",
                    this.checkBox_forceCNY.Checked);

                this.MainForm.AppInfo.SetBoolean(
                    "check_borrowinfo_form",
                    "overwrite_exist_price",
                    this.checkBox_overwriteExistPrice.Checked);
            }
        }

        private void button_beginCheckFromReader_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bAutoRepair = Control.ModifierKeys == Keys.Control;

            List<string> barcodes = null;
            int nRet = SearchAllReaderBarcode(out barcodes,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = CheckReaderRecords(barcodes,
                bAutoRepair,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "OK");
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 检索获得所有读者证条码号
        int SearchAllReaderBarcode(out List<string> barcodes,
            out string strError)
        {
            strError = "";

            barcodes = new List<string>();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                long lRet = Channel.SearchReader(stop,
                    "<全部>",
                    "",
                    -1,
                    "证条码",
                    "left",
                    this.Lang,
                    null,   // strResultSetName
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;

                stop.SetProgressRange(0, lCount);

                Global.WriteHtml(this.webBrowser_resultInfo,
    "共有 " + lHitCount.ToString() + " 条读者记录。\r\n");


                Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        goto ERROR1;
                    }

                    Debug.Assert(searchresults != null, "");

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        /*
                        NewLine(this.listView_records,
                            searchresults[i].Path,
                            searchresults[i].Cols);
                         * */
                        barcodes.Add(searchresults[i].Cols[0]);
                    }


                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共有条码 " + lHitCount.ToString() + " 个。已获得条码 " + lStart.ToString() + " 个");
                    stop.SetProgressValue(lStart);

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);

                // 排序、去重
                stop.SetMessage("正在排序和去重");

                // 排序
                barcodes.Sort();

                // 去重
                int nRemoved = 0;
                for (int i = 0; i < barcodes.Count; i++)
                {
                    string strBarcode = barcodes[i];

                    for (int j = i + 1; j < barcodes.Count; j++)
                    {
                        if (strBarcode == barcodes[j])
                        {
                            barcodes.RemoveAt(j);
                            nRemoved++;
                            j--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            return 0;

        ERROR1:
            return -1;
        }

        // parameters:
        //      bAutoRepair 是否同时自动修复
        int CheckReaderRecords(List<string> barcodes,
            bool bAutoRepair,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            stop.OnStop += new StopEventHandler(this.DoStop);
            if (bAutoRepair)
                stop.Initial("正在进行检查和自动修复 ...");
            else
                stop.Initial("正在进行检查 ...");
            stop.BeginLoop();

            EnableControls(false);

            string[] aDupPath = null;
            try
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
    "\r\n" + DateTime.Now.ToString() + "\r\n");

                if (bAutoRepair)
                    Global.WriteHtml(this.webBrowser_resultInfo,
                        "正在进行检查和自动修复...\r\n");
                else
                    Global.WriteHtml(this.webBrowser_resultInfo,
                        "正在进行检查...\r\n");

                stop.SetProgressRange(0, barcodes.Count);
                int nCount = 0;
                int nRepairedCount = 0;
                for (int i = 0; i < barcodes.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strReaderBarcode = barcodes[i];
                    string strOutputReaderBarcode = "";

                    stop.SetMessage("正在检查第 " + (i + 1).ToString() + " 个读者记录，条码为 " + strReaderBarcode);
                    stop.SetProgressValue(i);

                    int nStart = 0;
                    int nPerCount = -1;
                    int nProcessedBorrowItems = 0;
                    int nTotalBorrowItems = 0;

                    bool bFoundError = false;
                    for (; ; )
                    {
                        lRet = Channel.RepairBorrowInfo(
                            stop,
                            "checkfromreader",
                            strReaderBarcode,
                            "",
                            "",
                            nStart,   // 2008/10/27 
                            nPerCount,   // 2008/10/27 
                            out nProcessedBorrowItems,   // 2008/10/27 
                            out nTotalBorrowItems,   // 2008/10/27 
                            out strOutputReaderBarcode,
                            out aDupPath,
                            out strError);

                        string strOffsComment = "";
                        if (nStart > 0)
                        {
                            strOffsComment = "(偏移量" + nStart.ToString() + "开始的" + nProcessedBorrowItems + "个借阅册)";
                        }

                        if (lRet == -1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "检查读者记录 " + strReaderBarcode + " 时" + strOffsComment + "出错: " + strError + "\r\n");
                        }
                        if (lRet == 1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "检查读者记录 " + strReaderBarcode + " 时" + strOffsComment + "发现问题: " + strError + "\r\n");
                            bFoundError = true;
                        }

                        if (nTotalBorrowItems > 0 && nProcessedBorrowItems == 0)
                        {
                            Debug.Assert(false, "当nTotalBorrowItems值大于0的时候(为" + nTotalBorrowItems.ToString() + ")，nProcessedBorrowItems值不能为0");
                            break;
                        }

                        nStart += nProcessedBorrowItems;

                        if (nStart >= nTotalBorrowItems)
                            break;
                    }

                    string strReaderXml = "";
                    string strReaderRecPath = "";
                    // 读者证条码号查重
                    if (this.checkBox_checkReaderBarcodeDup.Checked == true
                        || bFoundError)
                    {
                        byte[] baTimestamp = null;
                        string[] results = null;
                        lRet = Channel.GetReaderInfo(stop,
                            strReaderBarcode,
                            "xml",
                            out results,
                            out strReaderRecPath,
                            out baTimestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "对读者证条码号 " + strReaderBarcode + " 查重时出错: " + strError + "\r\n");
                        }
                        if (lRet > 1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "读者证条码号 " + strReaderBarcode + " 有重复记录 " + lRet.ToString() + "条\r\n");
                        }
                        if (lRet == 1)
                        {
                            strReaderXml = results[0];
                        }
                    }

                    if (bFoundError && string.IsNullOrEmpty(strReaderXml) == false)
                    {
                        int nRet = RepairAllErrorFromReaderSide(strReaderRecPath,
                            strReaderXml,
                            out strError);
                        if (nRet == -1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "*** 修复读者记录 " + strReaderBarcode + " 内链条问题时出错: " + strError + "\r\n");
                        }
                        else
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
    "- 成功修复读者记录 " + strReaderBarcode + " 内链条问题\r\n");
                            nRepairedCount++;
                        }
                    }

                    nCount++;
                }

                if (bAutoRepair)
                    Global.WriteHtml(this.webBrowser_resultInfo,
                        "修复结束。共检查读者记录 " + nCount.ToString() + " 条，修复有问题的读者记录 " + nRepairedCount.ToString() + " 条。\r\n");
                else
                    Global.WriteHtml(this.webBrowser_resultInfo,
    "检查结束。共检查读者记录 " + nCount.ToString() + " 条。\r\n");

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            return 0;
        ERROR1:
            return -1;
        }

#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.button_beginCheckFromReader.Enabled = bEnable;
            this.button_beginCheckFromItem.Enabled = bEnable;
            this.button_clearInfo.Enabled = bEnable;

            this.button_repairReaderSide.Enabled = bEnable;
            this.button_repairItemSide.Enabled = bEnable;
            this.textBox_itemBarcode.Enabled = bEnable;
            this.textBox_readerBarcode.Enabled = bEnable;

            this.button_batchAddItemPrice.Enabled = bEnable;

            this.checkBox_checkItemBarcodeDup.Enabled = bEnable;
            this.checkBox_checkReaderBarcodeDup.Enabled = bEnable;

            this.checkBox_displayPriceString.Enabled = bEnable;
            this.checkBox_forceCNY.Enabled = bEnable;
            this.checkBox_overwriteExistPrice.Enabled = bEnable;

            this.textBox_single_readerBarcode.Enabled = bEnable;
            this.textBox_single_itemBarcode.Enabled = bEnable;
            this.button_single_checkFromItem.Enabled = bEnable;
            this.button_single_checkFromReader.Enabled = bEnable;
        }

        private void button_beginCheckFromItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bAutoRepair = Control.ModifierKeys == Keys.Control;

            List<string> barcodes = null;
            int nRet = SearchAllItemBarcode(out barcodes,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = CheckItemRecords(barcodes,
                bAutoRepair,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "OK");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 检索获得所有册条码号
        int SearchAllItemBarcode(out List<string> barcodes,
            out string strError)
        {
            strError = "";

            barcodes = new List<string>();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                long lRet = Channel.SearchItem(
                    stop,
                    "<all>",
                    "",
                    -1,
                    "册条码",
                    "left",
                    this.Lang,
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                long lStart = 0;
                long lCount = lHitCount;

                stop.SetProgressRange(0, lCount);

                Global.WriteHtml(this.webBrowser_resultInfo,
    "共有 " + lHitCount.ToString() + " 条册记录。\r\n");


                Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    lRet = Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        goto ERROR1;
                    }

                    Debug.Assert(searchresults != null, "");

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        /*
                        NewLine(this.listView_records,
                            searchresults[i].Path,
                            searchresults[i].Cols);
                         * */
                        barcodes.Add(searchresults[i].Cols[0]);
                    }


                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    stop.SetMessage("共有条码 " + lHitCount.ToString() + " 个。已获得条码 " + lStart.ToString() + " 个");
                    stop.SetProgressValue(lStart);

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                // 排序、去重
                stop.SetMessage("正在排序和去重");

                // 排序
                barcodes.Sort();

                // 去重
                int nRemoved = 0;
                for (int i = 0; i < barcodes.Count; i++)
                {
                    string strBarcode = barcodes[i];

                    for (int j = i + 1; j < barcodes.Count; j++)
                    {
                        if (strBarcode == barcodes[j])
                        {
                            barcodes.RemoveAt(j);
                            nRemoved++;
                            j--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            return 0;

        ERROR1:
            return -1;
        }

        // 检索获得所有册条码号(另一版本，输出到文件)
        int SearchAllItemBarcode(string strBarcodeFilename,
            out string strError)
        {
            strError = "";

            // 创建文件
            StreamWriter sw = new StreamWriter(strBarcodeFilename,
                false,	// append
                System.Text.Encoding.UTF8);

            try
            {


                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在检索 ...");
                stop.BeginLoop();

                EnableControls(false);

                try
                {
                    long lRet = Channel.SearchItem(
                        stop,
                        "<all>",
                        "",
                        -1,
                        "册条码",
                        "left",
                        this.Lang,
                        null,   // strResultSetName
                        "",    // strSearchStyle
                        "", // strOutputStyle
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    long lHitCount = lRet;

                    long lStart = 0;
                    long lCount = lHitCount;

                    Global.WriteHtml(this.webBrowser_resultInfo,
        "共有 " + lHitCount.ToString() + " 条册记录。\r\n");


                    Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        lRet = Channel.GetSearchResult(
                            stop,
                            null,   // strResultSetName
                            lStart,
                            lCount,
                            "id,cols",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        if (lRet == 0)
                        {
                            strError = "未命中";
                            goto ERROR1;
                        }

                        Debug.Assert(searchresults != null, "");

                        // 处理浏览结果
                        for (int i = 0; i < searchresults.Length; i++)
                        {
                            // barcodes.Add(searchresults[i].Cols[0]);
                            sw.Write(searchresults[i].Cols[0] + "\r\n");
                        }


                        lStart += searchresults.Length;
                        lCount -= searchresults.Length;

                        stop.SetMessage("共有条码 " + lHitCount.ToString() + " 个。已获得条码 " + lStart.ToString() + " 个");

                        if (lStart >= lHitCount || lCount <= 0)
                            break;
                    }

                    // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
                }
                finally
                {
                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }

            return 0;

        ERROR1:
            return -1;
        }

        // parameters:
        //      bAutoRepair 是否同时自动修复
        int CheckItemRecords(List<string> barcodes,
            bool bAutoRepair,
            out string strError)
        {
            strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            if (bAutoRepair)
                stop.Initial("正在进行检查和自动修复 ...");
            else
                stop.Initial("正在进行检查 ...");
            stop.BeginLoop();

            EnableControls(false);

            string[] aDupPath = null;
            try
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
"\r\n" + DateTime.Now.ToString() + "\r\n");

                if (bAutoRepair)
                    Global.WriteHtml(this.webBrowser_resultInfo,
                        "正在进行检查和自动修复...\r\n");
                else
                    Global.WriteHtml(this.webBrowser_resultInfo,
                        "正在进行检查...\r\n");

                stop.SetProgressRange(0, barcodes.Count);

                int nCount = 0;
                int nRepairedCount = 0;
                for (int i = 0; i < barcodes.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strItemBarcode = barcodes[i];
                    string strOutputReaderBarcode = "";

                    stop.SetMessage("正在检查第 " + (i + 1).ToString() + " 个册记录，条码为 " + strItemBarcode);
                    stop.SetProgressValue(i);

                    int nProcessedBorrowItems = 0;
                    int nTotalBorrowItems = 0;

                    long lRet = Channel.RepairBorrowInfo(
                        stop,
                        "checkfromitem",
                        "",
                        strItemBarcode,
                        "",
                        0,
                        -1,
                        out nProcessedBorrowItems,   // 2008/10/27 
                        out nTotalBorrowItems,   // 2008/10/27 
                        out strOutputReaderBarcode,
                        out aDupPath,
                        out strError);
                    if (lRet == -1 || lRet == 1)
                    {
                        if (Channel.ErrorCode == ErrorCode.ItemBarcodeDup)
                        {
                            List<string> linkedPath = new List<string>();

                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "检查册记录 " + strItemBarcode + " 时发现册条码号命中重复记录 " + aDupPath.Length.ToString() + "个 -- " + StringUtil.MakePathList(aDupPath) + "。\r\n");

                            for (int j = 0; j < aDupPath.Length; j++)
                            {
                                string strText = " 检查其中第 " + (j + 1).ToString() + " 个，路径为 " + aDupPath[j] + ": ";

                                string[] aDupPathTemp = null;
                                // string strOutputReaderBarcode = "";
                                long lRet_2 = Channel.RepairBorrowInfo(
                                    stop,
                                    "checkfromitem",
                                    "",
                                    strItemBarcode,
                                    aDupPath[j],
                        0,
                        -1,
                        out nProcessedBorrowItems,   // 2008/10/27 
                        out nTotalBorrowItems,   // 2008/10/27 
                                    out strOutputReaderBarcode,
                                    out aDupPathTemp,
                                    out strError);
                                if (lRet_2 == -1)
                                {
                                    goto ERROR1;
                                }
                                if (lRet_2 == 1)
                                {
                                    strText += "发现问题: " + strError + "\r\n";

                                    Global.WriteHtml(this.webBrowser_resultInfo,
                                        strText);

                                    {
                                        int nRet = RepairErrorFromItemSide(strItemBarcode,
                                    aDupPath[j],
                                            out strError);
                                        if (nRet == -1)
                                        {
                                            Global.WriteHtml(this.webBrowser_resultInfo,
                                                "*** 修复册记录 " + strItemBarcode + " 内链条问题时出错: " + strError + "\r\n");
                                        }
                                        else
                                        {
                                            Global.WriteHtml(this.webBrowser_resultInfo,
                    "- 成功修复册记录 " + strItemBarcode + " 内链条问题\r\n");
                                            nRepairedCount++;
                                        }
                                    }
                                }

                            } // end of for

                            continue;
                        }

                        /*
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时出错: " + strError + "\r\n");
                         * */
                        if (lRet == -1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "检查册记录 " + strItemBarcode + " 时出错: " + strError + "\r\n");
                        }
                        if (lRet == 1)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                "检查册记录 " + strItemBarcode + " 时发现问题: " + strError + "\r\n");
                            {
                                int nRet = RepairErrorFromItemSide(strItemBarcode,
                                    "",
                                    out strError);
                                if (nRet == -1)
                                {
                                    Global.WriteHtml(this.webBrowser_resultInfo,
                                        "*** 修复册记录 " + strItemBarcode + " 内链条问题时出错: " + strError + "\r\n");
                                }
                                else
                                {
                                    Global.WriteHtml(this.webBrowser_resultInfo,
            "- 成功修复册记录 " + strItemBarcode + " 内链条问题\r\n");
                                    nRepairedCount++;
                                }
                            }
                        }
                        continue;
                    } // end of return -1

                    if (lRet == -1)
                    {
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时出错: " + strError + "\r\n");
                    }
                    if (lRet == 1)
                    {
                        Debug.Assert(false, "应该走不到这里");
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时发现问题: " + strError + "\r\n");
                    }

                    nCount++;
                }

                Global.WriteHtml(this.webBrowser_resultInfo,
                    "检查结束。共检查册记录 " + nCount.ToString() + " 条。\r\n");
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
            }

            return 0;

        ERROR1:
            return -1;
        }

        private void button_repairReaderSide_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = RepairError(
                "repairreaderside",
                this.textBox_readerBarcode.Text,
                this.textBox_itemBarcode.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "修复成功。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 从册侧出发，修复一个册记录和相关读者记录的链条错误
        // return:
        //      -1  错误。
        //      0   没有必要修复
        //      1   已经修复
        int RepairErrorFromItemSide(string strItemBarcode,
            string strConfirmItemRecPath,
            out string strError)
        {
            strError = "";

            // 获得册记录
            string strBiblioRecPath = "";
            string strItemRecPath = "";
            byte[] item_timestamp = null;

            string strItemXml = "";
            string strBiblioText = "";

            // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
            long lRet = Channel.GetItemInfo(
                stop,
                strItemBarcode,
                "xml",   // strResultType
                out strItemXml,
                out strItemRecPath,
                out item_timestamp,
                "",  // strBiblioType
                out strBiblioText,
                out strBiblioRecPath,
                out strError);
            if (lRet == -1)
            {
                // 改用 strConfirmItemRecPath 试一下
                if (string.IsNullOrEmpty(strConfirmItemRecPath) == false)
                {
                    lRet = Channel.GetItemInfo(
stop,
"@path:" + strConfirmItemRecPath,
"xml",   // strResultType
out strItemXml,
out strItemRecPath,
out item_timestamp,
"",  // strBiblioType
out strBiblioText,
out strBiblioRecPath,
out strError);
                    if (lRet == -1)
                    {
                        strError = "获取路径为 '" + strConfirmItemRecPath + "' 的册记录时出错:" + strError;
                        return -1;
                    }
                }
                strError = "获取册条码号为 '" + strItemBarcode + "' 的册记录时出错:" + strError;
                return -1;
            }

            XmlDocument itemdom = new XmlDocument();
            try
            {
                itemdom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "册记录装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(strItemBarcode))
            {
                strItemBarcode = DomUtil.GetElementText(itemdom.DocumentElement, "refID");
                if (string.IsNullOrEmpty(strItemBarcode))
                {
                    strError = "册记录中既没有证条码号，也没有 参考 ID 字段内容，无法进行修复";
                    return -1;
                }
                strItemBarcode = "@refID:" + strItemBarcode;
            }

            string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement, "borrower");
            if (string.IsNullOrEmpty(strBorrower) == true)
            {
                strError = "没有必要修复";
                return 0;
            }

            string[] aDupPath = null;

            string strOutputReaderBarcode = "";
            int nProcessedBorrowItems = 0;
            int nTotalBorrowItems = 0;
            lRet = Channel.RepairBorrowInfo(
                stop,
                "repairitemside",
                strBorrower,
                strItemBarcode,
                strConfirmItemRecPath,
                0,
                -1,
                out nProcessedBorrowItems,   // 2008/10/27 
                out nTotalBorrowItems,   // 2008/10/27 
                out strOutputReaderBarcode,
                out aDupPath,
                out strError);
            if (lRet == -1)
            {
                if (Channel.ErrorCode == ErrorCode.NoError)
                    return 0;
                return -1;
            }
            else
                return 1;
        }

        // 从读者侧出发，修复一个读者记录中，所有册记录的链条错误
        // return:
        //      -1  错误。可能有部分册已经修复成功
        //      其他  共修复多少个册事项
        int RepairAllErrorFromReaderSide(string strReaderRecPath,
            string strReaderXml,
            out string strError)
        {
            strError = "";

            XmlDocument readerdom = new XmlDocument();
            try
            {
                readerdom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "读者记录装入 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            string strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "barcode");
            if (string.IsNullOrEmpty(strReaderBarcode))
            {
                strReaderBarcode = DomUtil.GetElementText(readerdom.DocumentElement, "refID");
                if (string.IsNullOrEmpty(strReaderBarcode))
                {
                    strError = "读者记录 '" + strReaderRecPath + "' 中既没有证条码号，也没有 参考 ID 字段内容，无法进行修复";
                    return -1;
                }
                strReaderBarcode = "@refID:" + strReaderBarcode;
            }

            int nRepairedCount = 0;
            List<string> errors = new List<string>();

            XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("borrows/borrow");
            foreach (XmlElement borrow in nodes)
            {
                string strItemBarcode = borrow.GetAttribute("barcode");
                string strConfirmItemRecPath = borrow.GetAttribute("recPath");

                string[] aDupPath = null;

                string strOutputReaderBarcode = "";
                int nProcessedBorrowItems = 0;
                int nTotalBorrowItems = 0;
                long lRet = Channel.RepairBorrowInfo(
                    stop,
                    "repairreaderside",
                    strReaderBarcode,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    0,
                    -1,
                    out nProcessedBorrowItems,   // 2008/10/27 
                    out nTotalBorrowItems,   // 2008/10/27 
                    out strOutputReaderBarcode,
                    out aDupPath,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode != ErrorCode.NoError)
                        errors.Add(strError);
                }
                else
                    nRepairedCount++;
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "; ");
                return -1;
            }
            return nRepairedCount;
        }

        int RepairError(
            string strAction,
            string strReaderBarcode,
            string strItemBarcode,
            out string strError)
        {
            strError = "";
            int nProcessedBorrowItems = 0;
            int nTotalBorrowItems = 0;

            Debug.Assert(strAction == "repairreaderside" || strAction == "repairitemside", "");

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行修复 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                string strConfirmItemRecPath = "";
            REDO:
                string[] aDupPath = null;

                string strOutputReaderBarcode = "";

                long lRet = Channel.RepairBorrowInfo(
                    stop,
                    strAction,  // "repairreaderside",
                    strReaderBarcode,
                    strItemBarcode,
                    strConfirmItemRecPath,
                    0,
                    -1,
                    out nProcessedBorrowItems,   // 2008/10/27 
                    out nTotalBorrowItems,   // 2008/10/27 
                    out strOutputReaderBarcode,
                    out aDupPath,
                    out strError);
                if (lRet == -1)
                {
                    if (Channel.ErrorCode == ErrorCode.ItemBarcodeDup)
                    {
                        // this.MainForm.PrepareSearch();
                        LibraryChannel channel = this.MainForm.GetChannel();
                        try
                        {
                            ItemBarcodeDupDlg dupdlg = new ItemBarcodeDupDlg();
                            MainForm.SetControlFont(dupdlg, this.Font, false);
                            string strErrorNew = "";
                            int nRet = dupdlg.Initial(
                                this.MainForm,
                                aDupPath,
                                "因册条码号发生重复，修复操作被拒绝。\r\n\r\n可根据下面列出的详细信息，选择适当的册记录，重试操作。\r\n\r\n原始出错信息:\r\n" + strError,
                                channel,    // this.MainForm.Channel,
                                this.MainForm.Stop,
                                out strErrorNew);
                            if (nRet == -1)
                            {
                                // 初始化对话框失败
                                MessageBox.Show(this, strErrorNew);
                                goto ERROR1;
                            }

                            this.MainForm.AppInfo.LinkFormState(dupdlg, "CheckBorrowInfoForm_dupdlg_state");
                            dupdlg.ShowDialog(this);
                            this.MainForm.AppInfo.UnlinkFormState(dupdlg);

                            if (dupdlg.DialogResult == DialogResult.Cancel)
                                goto ERROR1;

                            strConfirmItemRecPath = dupdlg.SelectedRecPath;

                            goto REDO;
                        }
                        finally
                        {
                            this.MainForm.ReturnChannel(channel);
                            // this.MainForm.EndSearch();
                        }
                    }

                    goto ERROR1;
                } // end of return -1

            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 0;
        ERROR1:
            return -1;
        }

        private void button_repairItemSide_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = RepairError(
                "repairitemside",
                this.textBox_readerBarcode.Text,
                this.textBox_itemBarcode.Text,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "修复成功。");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 批增加册价格
        private void button_batchAddItemPrice_Click(object sender, EventArgs e)
        {
            string strError = "";


            if (this.checkBox_overwriteExistPrice.Checked == true)
            {
                DialogResult result = MessageBox.Show(this,
    "确实要覆盖已经存在的价格字符串? 这是一个很不平常的操作。",
    "CheckBorrowInfoForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    MessageBox.Show(this, "放弃处理");
                    return;
                }
            }

            // List<string> barcodes = null;
            string strBarcodeFilename = Path.GetTempFileName();

            try
            {
                int nRet = SearchAllItemBarcode(strBarcodeFilename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = BatchAddItemPrice(strBarcodeFilename,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                File.Delete(strBarcodeFilename);
            }

            MessageBox.Show(this, "OK");

            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        int BatchAddItemPrice(string strBarcodeFilename,
            out string strError)
        {
            strError = "";

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(strBarcodeFilename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + strBarcodeFilename + " 失败: " + ex.Message;
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在批增加册价格 ...");
            stop.BeginLoop();

            EnableControls(false);

            try
            {
                int nCount = 0;

                Global.ClearForPureTextOutputing(this.webBrowser_resultInfo);

                for (int i = 0; ; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    // string strItemBarcode = barcodes[i];
                    string strItemBarcode = sr.ReadLine();

                    if (strItemBarcode == null)
                        break;

                    if (String.IsNullOrEmpty(strItemBarcode) == true)
                        continue;

                    stop.SetMessage("正在检查第 " + (i + 1).ToString() + " 个册记录，条码为 " + strItemBarcode);

                    int nRedoCount = 0;
                REDO:

                    // 获得书目记录路径
                    string strBiblioRecPath = "";
                    string strItemRecPath = "";
                    byte[] item_timestamp = null;

                    string strItemXml = "";
                    string strBiblioText = "";

                    // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                    long lRet = Channel.GetItemInfo(
                        stop,
                        strItemBarcode,
                        "xml",   // strResultType
                        out strItemXml,
                        out strItemRecPath,
                        out item_timestamp,
                        "recpath",  // strBiblioType
                        out strBiblioText,
                        out strBiblioRecPath,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "获得书目记录、路径发生错误: " + strError;

                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时出错(1): " + strError + "\r\n");
                        continue;
                    }

                    if (lRet == 0)
                    {
                        strError = "册条码号 " + strItemBarcode + " 对应的XML数据没有找到。";
                        Global.WriteHtml(this.webBrowser_resultInfo,
                           "检查册记录 " + strItemBarcode + " 时出错(2): " + strError + "\r\n");
                        continue;
                    }

                    if (lRet > 1)
                    {
                        strError = "册条码号 " + strItemBarcode + " 对应数据多于一条。";
                        Global.WriteHtml(this.webBrowser_resultInfo,
                           "检查册记录 " + strItemBarcode + " 时出错(3): " + strError + "\r\n");
                        continue;
                    }

                    XmlDocument itemdom = new XmlDocument();
                    try
                    {
                        itemdom.LoadXml(strItemXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "册记录装入DOM失败: " + ex.Message;
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时出错(4): " + strError + "\r\n");
                        continue;
                    }

                    // 如果为追加
                    if (this.checkBox_overwriteExistPrice.Checked == false)
                    {
                        // 看看册记录中是否已经有了价格信息?
                        if (HasPrice(itemdom) == true)
                            continue;
                    }

                    // 获得biblio part price
                    string strPartName = "@price";
                    string strResultValue = "";

                    // Result.Value -1出错 0没有找到 1找到
                    lRet = Channel.GetBiblioInfo(
                        stop,
                        strBiblioRecPath,
                        "", // strBiblioXml
                        strPartName,    // 包含'@'符号
                        out strResultValue,
                        out strError);
                    if (lRet == -1)
                    {
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时出错(5): " + strError + "\r\n");
                        continue;
                    }

                    if (lRet == 0)
                    {
                        strError = "书目数据 '" + strBiblioRecPath + "' 中没有价格信息";
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时警告(5): " + strError + "\r\n");
                        continue;
                    }

                    // 规范化价格字符串
                    string strPrice = CanonicalizePrice(strResultValue,
                        this.checkBox_forceCNY.Checked);

                    // 加入价格信息
                    int nRet = AddPrice(ref itemdom,
                        strPrice,
                        out strError);
                    if (nRet == -1)
                    {
                        Global.WriteHtml(this.webBrowser_resultInfo,
                            "检查册记录 " + strItemBarcode + " 时出错(6): " + strError + "\r\n");
                        continue;
                    }

                    // 保存册记录

                    EntityInfo[] entities = new EntityInfo[1];
                    EntityInfo info = new EntityInfo();

                    info.RefID = Guid.NewGuid().ToString(); // 2008/4/14 
                    info.Action = "change";
                    info.OldRecPath = strItemRecPath;    // 2007/6/2 
                    info.NewRecPath = strItemRecPath;

                    info.NewRecord = itemdom.OuterXml;
                    info.NewTimestamp = null;

                    info.OldRecord = strItemXml;
                    info.OldTimestamp = item_timestamp;
                    entities[0] = info;

                    EntityInfo[] errorinfos = null;

                    lRet = Channel.SetEntities(
                        stop,
                        strBiblioRecPath,
                        entities,
                        out errorinfos,
                        out strError);
                    if (lRet == -1)
                    {
                        Global.WriteHtml(this.webBrowser_resultInfo,
    "检查册记录 " + strItemBarcode + " 时出错(7): " + strError + "\r\n");
                        continue;
                    }

                    {
                        // 如果时间戳不匹配？重做
                        if (errorinfos != null
                            && errorinfos.Length == 1)
                        {
                            // 正常信息处理
                            if (errorinfos[0].ErrorCode == ErrorCodeValue.NoError)
                            {
                            }
                            else if (errorinfos[0].ErrorCode == ErrorCodeValue.TimestampMismatch
                                && nRedoCount < 10)
                            {
                                nRedoCount++;
                                goto REDO;
                            }
                            else
                            {
                                Global.WriteHtml(this.webBrowser_resultInfo,
                                    "检查册记录 " + strItemBarcode + " 时出错(8): " + errorinfos[0].ErrorInfo + "\r\n");
                                continue;
                            }
                        }

                    }

                    if (this.checkBox_displayPriceString.Checked == true)
                    {
                        if (strResultValue != strPrice)
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                strItemBarcode + ": " + strResultValue + " --> " + strPrice + " \r\n");
                        }
                        else
                        {
                            Global.WriteHtml(this.webBrowser_resultInfo,
                                strItemBarcode + ": " + strPrice + " \r\n");
                        }
                    }

                    nCount++;
                }


                Global.WriteHtml(this.webBrowser_resultInfo,
                    "处理结束。共增补价格字符串 " + nCount.ToString() + " 个。\r\n");


            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                if (sr != null)
                    sr.Close();
            }

            return 0;
        ERROR1:
            return -1;
        }

        static bool HasPrice(XmlDocument itemdom)
        {
            string strPrice = DomUtil.GetElementText(itemdom.DocumentElement,
                "price");

            if (String.IsNullOrEmpty(strPrice) == true)
                return false;

            return true;
        }

        static int AddPrice(ref XmlDocument itemdom,
            string strPrice,
            out string strError)
        {
            strError = "";

            DomUtil.SetElementText(itemdom.DocumentElement,
                "price",
                strPrice);

            return 0;
        }

        /*
~~~~~~~
    乐山师院数据来源多，以前的种价格字段格式著录格式多样，有“CNY25.00元”、
“25.00”、“￥25.00元”、“￥25.00”、“CNY25.00”、“cny25.00”、“25.00
元”等等，现在他们确定以后全采用“CNY25.00”格式著录。
    CALIS中，许可重复010$d来表达价格实录和获赠或其它币种价格。所以，可能乐山
师院也有少量的此类重复价格子字段的数据。
    为省成本，批处理或册信息编辑窗中，建议只管一个价格字段，别的都不管（如果
没有价格字段，则转换为空而非零）。
    转换时，是否可以兼顾到用中文全角输入的数字如“２５.００”或小数点是中文
全解但标点选择的是英文标点如“．”？

~~~~
处理步骤：
1) 全部字符转换为半角
2) 抽出纯数字部分
3) 观察前缀或者后缀，如果有CNY cny ￥ 元等字样，可以确定为人民币。
前缀和后缀完全为空，也可确定为人民币。
否则，保留原来的前缀。         * */
        // 正规化价格字符串
        static string CanonicalizePrice(string strPrice,
            bool bForceCNY)
        {
            // 全角字符变换为半角
            strPrice = Global.ConvertQuanjiaoToBanjiao(strPrice);

            if (bForceCNY == true)
            {
                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                return "CNY" + strPurePrice;
            }

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            string strError = "";

            int nRet = PriceUtil.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return strPrice;    // 无法parse

            bool bCNY = false;
            strPrefix = strPrefix.Trim();
            strPostfix = strPostfix.Trim();

            if (String.IsNullOrEmpty(strPrefix) == true
                && String.IsNullOrEmpty(strPostfix) == true)
            {
                bCNY = true;
                goto DONE;
            }


            if (strPrefix.IndexOf("CNY") != -1
                || strPrefix.IndexOf("cny") != -1
                || strPrefix.IndexOf("ＣＮＹ") != -1
                || strPrefix.IndexOf("ｃｎｙ") != -1
                || strPrefix.IndexOf('￥') != -1)
            {
                bCNY = true;
                goto DONE;
            }

            if (strPostfix.IndexOf("元") != -1)
            {
                bCNY = true;
                goto DONE;
            }

        DONE:
            // 人民币
            if (bCNY == true)
                return "CNY" + strValue;

            // 其他货币
            return strPrefix + strValue + strPostfix;

        }

        private void button_clearInfo_Click(object sender, EventArgs e)
        {
            Global.ClearForPureTextOutputing(this.webBrowser_resultInfo);
        }

        private void button_single_checkFromItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行检查 ...");
            stop.BeginLoop();

            EnableControls(false);

            string[] aDupPath = null;
            string strText = "";
            try
            {
                string strItemBarcode = this.textBox_single_itemBarcode.Text;
                string strOutputReaderBarcode = "";

                int nProcessedBorrowItems = 0;
                int nTotalBorrowItems = 0;

                long lRet = Channel.RepairBorrowInfo(
                    stop,
                    "checkfromitem",
                    "",
                    strItemBarcode,
                    "",
                    0,
                    -1,
                    out nProcessedBorrowItems,   // 2008/10/27 
                    out nTotalBorrowItems,   // 2008/10/27 
                    out strOutputReaderBarcode,
                    out aDupPath,
                    out strError);
                if (lRet == -1 || lRet == 1)
                {
                    if (Channel.ErrorCode == ErrorCode.ItemBarcodeDup)
                    {
                        List<string> linkedPath = new List<string>();

                        strText += "检查册记录 " + strItemBarcode + " 时发现册条码号命中重复记录 " + aDupPath.Length.ToString() + "个。检查其中\r\n";

                        for (int j = 0; j < aDupPath.Length; j++)
                        {
                            strText += " 第 " + (j + 1).ToString() + " 个，路径为 " + aDupPath[j] + " \r\n";

                            string[] aDupPathTemp = null;
                            long lRet_2 = Channel.RepairBorrowInfo(
                                stop,
                                "checkfromitem",
                                "",
                                strItemBarcode,
                                aDupPath[j],
                    0,
                    -1,
                    out nProcessedBorrowItems,   // 2008/10/27 
                    out nTotalBorrowItems,   // 2008/10/27 
                                out strOutputReaderBarcode,
                                out aDupPathTemp,
                                out strError);
                            if (lRet_2 == -1)
                            {
                                goto ERROR1;
                            }
                            if (lRet_2 == 1)
                            {
                                strText += "  发现问题: " + strError + "\r\n";
                            }

                        } // end of for

                        goto END1;
                    }

                    if (lRet == -1)
                    {
                        strText += "检查册记录 " + strItemBarcode + " 时出错: " + strError + "\r\n";
                    }
                    if (lRet == 1)
                    {
                        strText += "检查册记录 " + strItemBarcode + " 时发现问题: " + strError + "\r\n";
                    }
                    goto END1;
                } // end of return -1 or 1

                if (lRet == -1)
                {
                    strText += "检查册记录 " + strItemBarcode + " 时出错: " + strError + "\r\n";
                }
                if (lRet == 1)
                {
                    strText += "检查册记录 " + strItemBarcode + " 时发现问题: " + strError + "\r\n";
                }


                if (string.IsNullOrEmpty(strItemBarcode) == false)
                {
                    string[] paths = null;
                    /*
                    lRet = Channel.SearchItemDup(stop,
                        strItemBarcode,
                        100,
                        out paths,
                        out strError);
                     * */
                    lRet = SearchEntityBarcode(stop,
                        strItemBarcode,
                        out paths,
                        out strError);
                    if (lRet == -1)
                    {
                        strText += "对册条码号 " + strItemBarcode + " 查重时出错: " + strError + "\r\n";
                    }
                    if (lRet > 1)
                    {
                        strText += "册条码号 " + strItemBarcode + " 有重复记录 " + paths.Length.ToString() + "条\r\n";
                    }
                }


            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

        END1:
            if (strText == "")
                MessageBox.Show(this, "没有发现问题。");
            else
                MessageBox.Show(this, strText);
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        int SearchEntityBarcode(
            Stop stop,
            string strBarcode,
            out string[] paths,
            out string strError)
        {
            strError = "";
            paths = null;

            if (string.IsNullOrEmpty(strBarcode) == true)
            {
                strError = "不应用册条码号为空来查重";
                return -1;
            }

            long lRet = Channel.SearchItem(
stop,
"<全部>",
strBarcode,
100,
"册条码号",
"exact",
"zh",
"dup",
"", // strSearchStyle
"", // strOutputStyle
out strError);
            if (lRet == -1)
                return -1;  // error

            if (lRet == 0)
                return 0;   // not found

            long lHitCount = lRet;

            List<string> aPath = null;
            lRet = Channel.GetSearchResult(stop,
                "dup",
                0,
                Math.Min(lHitCount, 100),
                "zh",
                out aPath,
                out strError);
            if (lRet == -1)
                return -1;

            paths = new string[aPath.Count];
            aPath.CopyTo(paths);

            return (int)lHitCount;
        }


        private void CheckBorrowInfoForm_Activated(object sender, EventArgs e)
        {
            this.MainForm.stopManager.Active(this.stop);
        }

        private void button_single_checkFromReader_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bAutoRepair = Control.ModifierKeys == Keys.Control;

            if (this.textBox_single_readerBarcode.Text == "")
            {
                strError = "尚未指定要检查的读者证条码号";
                goto ERROR1;
            }

            List<string> barcodes = new List<string>();
            barcodes.Add(this.textBox_single_readerBarcode.Text);

            nRet = CheckReaderRecords(barcodes,
                bAutoRepair,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}