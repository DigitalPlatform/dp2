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
using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 检查借阅信息窗
    /// </summary>
    public partial class CheckBorrowInfoForm : MyForm
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckBorrowInfoForm()
        {
            InitializeComponent();
        }

        private void CheckBorrowInfoForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
#if NO
            MainForm.AppInfo.LoadMdiChildFormStates(this,
    "mdi_form_state");
            this.Channel.Url = Program.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            // Global.ClearForPureTextOutputing(this.webBrowser_resultInfo);
            ClearHtml();

            this.checkBox_displayPriceString.Checked = Program.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "display_price_string",
                true);

            this.checkBox_forceCNY.Checked = Program.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "force_cny",
                false);

            this.checkBox_overwriteExistPrice.Checked = Program.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "overwrite_exist_price",
                false);

            this.checkBox_displayRecords.Checked = Program.MainForm.AppInfo.GetBoolean(
                "check_borrowinfo_form",
                "display_record",
                true);

            {
                this.tabControl_main.TabPages.Remove(this.tabPage_batchAddItemPrice);
                this.tabPage_batchAddItemPrice.Dispose();
                this.tabPage_batchAddItemPrice = null;
            }

            this.Channel = null;    // testing
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
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
            {
                Program.MainForm.AppInfo.SetBoolean(
                    "check_borrowinfo_form",
                    "display_price_string",
                    this.checkBox_displayPriceString.Checked);

                Program.MainForm.AppInfo.SetBoolean(
                    "check_borrowinfo_form",
                    "force_cny",
                    this.checkBox_forceCNY.Checked);

                Program.MainForm.AppInfo.SetBoolean(
                    "check_borrowinfo_form",
                    "overwrite_exist_price",
                    this.checkBox_overwriteExistPrice.Checked);

                Program.MainForm.AppInfo.SetBoolean(
    "check_borrowinfo_form",
    "display_record",
    this.checkBox_displayRecords.Checked);
            }
        }

        public bool DisplayRecords
        {
            get
            {
                return (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_displayRecords.Checked;
                }));
            }
            set
            {
                this.Invoke((Action)(() =>
                {
                    this.checkBox_displayRecords.Checked = value;
                }));
            }
        }

#if OLD
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
#endif

        private void button_beginCheckFromReader_Click(object sender, EventArgs e)
        {
            bool bAutoRepair = Control.ModifierKeys == Keys.Control;
            BeginCheckFromReader(bAutoRepair);
        }

        void BeginCheckFromReader(bool bAutoRepair)
        {
            string strError = "";

            // barcode --> 重复次数
            Hashtable barcode_table = new Hashtable();

            bool checkDup = this.checkBox_checkReaderBarcodeDup.Checked;

            CancellationToken token = new CancellationToken();
            _ = Task.Factory.StartNew(
                () =>
                {
                    DisplayText("");

                    DateTime start = DateTime.Now;
                    if (bAutoRepair)
                        DisplayText($"{start} 正在进行检查和自动修复...");
                    else
                        DisplayText($"{start} 正在进行检查...");

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在检索 ...");
                    stop.BeginLoop();

                    EnableControls(false);
                    try
                    {
                        int count = 0;
                        int repaired_count = 0;
                        // result.Value
                        //      -1  出错
                        //      >=0 实际获得的读者记录条数
                        var result = DownloadAllPatronRecord(
                            (channel, record) =>
                            {
                                count++;
                                // parameters:
                                //      bAutoRepair 是否同时自动修复
                                // return:
                                //      -1  出错
                                //      0   没有必要处理
                                //      1   已经处理
                                int ret = CheckReaderRecord(channel,
                                        record.Path,
                                        record.RecordBody.Xml,
                                        bAutoRepair,
                                        checkDup ? barcode_table : null,
                                        out string error);
                                if (ret == -1)
                                {
                                    if (channel.ErrorCode == ErrorCode.RequestCanceled)
                                        throw new ChannelException(channel.ErrorCode, error);
                                }
                                else
                                    repaired_count += ret;
                            },
                            (text) =>
                            {
                            },
                            token);
                        if (result.Value == -1)
                        {
                            DisplayError(result.ErrorInfo);
                            strError = result.ErrorInfo;
                        }
                        else
                            strError = "OK";

                        DateTime end = DateTime.Now;

                        if (bAutoRepair)
                            DisplayText($"{end} 修复结束。共检查读者记录 {count} 条，修复有问题的读者记录 {repaired_count} 条。");
                        else
                            DisplayText($"{end} 检查结束。共检查读者记录 {count} 条。");

                        ShowMessageBox(strError);
                        return;
                    }
                    catch (Exception ex)
                    {
                        strError = $"异常: {ex.Message}";
                        goto ERROR1;
                    }
                    finally
                    {
                        EnableControls(true);

                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                        stop.HideProgress();
                    }
                ERROR1:
                    ShowMessageBox(strError);
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            // MessageBox.Show(this, "OK");
            return;
        }

        public delegate void Delegate_processRecord(
            LibraryChannel channel,
            Record record);
        public delegate void Delegate_writeLog(string text);

        // result.Value
        //      -1  出错
        //      >=0 实际获得的读者记录条数
        public NormalResult DownloadAllPatronRecord(
            Delegate_processRecord processRecord,
            Delegate_writeLog writeLog,
            CancellationToken token)
        {
            writeLog?.Invoke($"开始下载全部读者记录到本地缓存");
            LibraryChannel channel = this.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为读者记录检索需要一定时间
            try
            {
            // int nRedoCount = 0;
            REDO:
                if (token.IsCancellationRequested)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "用户中断"
                    };

                string resultset_name = "#checkborrow_" + Guid.NewGuid().ToString();

                // 检索全部读者库记录
                long lRet = channel.SearchReader(null,  // stop,
                    "<all>",
                    "",
                    -1,
                    "__id",
                    "left",
                    "zh",
                    resultset_name,
                    "", // strOutputStyle
                    out string strError);
                if (lRet == -1)
                {
                    writeLog?.Invoke($"SearchReader() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                    /*
                    // 一次重试机会
                    if (lRet == -1
                        && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                        && nRedoCount < 2)
                    {
                        nRedoCount++;
                        goto REDO;
                    }
                    */
                    MessagePromptEventArgs e = new MessagePromptEventArgs();
                    e.MessageText = $"检索全部读者记录时发生错误： " + strError;
                    e.Actions = "yes,no";
                    loader_Prompt(this, e);
                    if (e.ResultAction == "yes")
                        goto REDO;

                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = strError,
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                long hitcount = lRet;

                writeLog?.Invoke($"共检索命中读者记录 {hitcount} 条");
                DisplayText($"共有 {hitcount} 条读者记录。");

                DateTime search_time = DateTime.Now;

                // Hashtable pii_table = new Hashtable();
                // int skip_count = 0;
                // int error_count = 0;

                if (hitcount > 0)
                {
                    stop.SetProgressRange(0, hitcount);

                    // 把超时时间改短一点
                    var timeout0 = channel.Timeout;
                    channel.Timeout = TimeSpan.FromSeconds(20);
                    try
                    {
                        // 获取和存储记录
                        ResultSetLoader loader = new ResultSetLoader(channel,
            stop,
            resultset_name,
            $"id,xml,timestamp",
            "zh");
                        loader.Prompt += this.loader_Prompt;

                        int i = 0;
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                        {
                            if (token.IsCancellationRequested)
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = "用户中断"
                                };

                            stop.SetMessage($"正在处理 {record.Path} ...");

                            processRecord?.Invoke(channel, record);

                            i++;

                            stop.SetProgressValue(i);
                        }
                    }
                    finally
                    {
                        channel.Timeout = timeout0;
                    }
                }

                // writeLog?.Invoke($"plan.StartDate='{plan.StartDate}'。skip_count={skip_count}, error_count={error_count}。返回");

                return new NormalResult
                {
                    Value = (int)hitcount,
                };
            }
            catch (ChannelException ex)
            {
                if (ex.ErrorCode == ErrorCode.RequestCanceled)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "用户中断",
                    };

                MainForm.WriteErrorLog($"DownloadAllPatronRecordAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                writeLog?.Invoke($"DownloadAllPatronRecord() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllPatronRecord() 出现异常：{ex.Message}"
                };
            }
            catch (Exception ex)
            {
                MainForm.WriteErrorLog($"DownloadAllPatronRecordAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                writeLog?.Invoke($"DownloadAllPatronRecord() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllPatronRecord() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                writeLog?.Invoke($"结束下载全部读者记录到本地缓存");
            }
        }

        // parameters:
        //      bAutoRepair 是否同时自动修复
        // return:
        //      -1  出错
        //      0   没有必要处理
        //      1   已经处理
        int CheckReaderRecord(LibraryChannel channel,
            string recpath,
            string xml,
            bool bAutoRepair,
            Hashtable barcode_table,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            int nRepairedCount = 0;
            int nCount = 0;

            // string[] aDupPath = null;
            try
            {
                if (string.IsNullOrEmpty(xml))
                    return 0;

                XmlDocument reader_dom = new XmlDocument();
                try
                {
                    reader_dom.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    strError = $"读者 XML 记录装入 XMLDOM 时出错: {ex.Message}";
                    return -1;
                }

                string strReaderBarcode = DomUtil.GetElementText(reader_dom.DocumentElement, "barcode");

                string caption = $"{strReaderBarcode}({recpath})";

                // 2022/2/21
                if (string.IsNullOrEmpty(strReaderBarcode))
                {
                    DisplayError($"读者记录 { recpath } 证条码号(barcode 元素)为空，格式不合法。请尽快修正此问题");
                    DisplayRecord(null, null, $"<pp>{recpath}<pp>");
                    return -1;
                    /*
                    strError = $"读者记录 {caption} 的证条码号为空，没有必要进行检查";
                    return 0;
                    */
                }

                // 条码号查重
                if (barcode_table != null && string.IsNullOrEmpty(strReaderBarcode) == false)
                {
                    int dup_count = 1;
                    if (barcode_table.ContainsKey(strReaderBarcode) == false)
                    {
                        barcode_table[strReaderBarcode] = dup_count;
                    }
                    else
                    {
                        dup_count = (int)barcode_table[strReaderBarcode];
                        dup_count++;
                        barcode_table[strReaderBarcode] = dup_count;
                    }

                    if (dup_count > 1)
                    {
                        DisplayCheckError($"读者证条码号 { strReaderBarcode } 有重复记录 { dup_count }条。({recpath})");
                    }
                }

                var nodes = reader_dom.DocumentElement.SelectNodes("borrows/borrow");
                if (nodes.Count == 0)
                    return 0;   // 没有必要检查

                // string strReaderBarcode = barcodes[i];
                string strOutputReaderBarcode = "";

                int nStart = 0;
                int nPerCount = -1;
                int nProcessedBorrowItems = 0;
                int nTotalBorrowItems = 0;

                bool bFoundError = false;
                for (; ; )
                {
                REDO_REPAIR:
                    lRet = channel.RepairBorrowInfo(
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
                        out string[] aDupPath,
                        out strError);

                    string strOffsComment = "";
                    if (nStart > 0)
                    {
                        strOffsComment = "(偏移量" + nStart.ToString() + "开始的" + nProcessedBorrowItems + "个借阅册)";
                    }

                    if (lRet == -1)
                    {
                        if (channel.ErrorCode == ErrorCode.RequestCanceled)
                            return -1;

                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = strError;
                        e.Actions = "yes,no";
                        loader_Prompt(this, e);
                        if (e.ResultAction == "yes")
                            goto REDO_REPAIR;

                        DisplayError($"检查读者记录 { caption} 时{ strOffsComment }出错: { strError}");
                        // DisplayError(strError);
                        return -1;
                    }
                    if (lRet == 1)
                    {
                        DisplayCheckError($"检查读者记录 { caption } 时{ strOffsComment }发现问题: ", PlainText(strError));
                        DisplayRecord(strReaderBarcode, null, strError);
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

                if (bFoundError
                    && bAutoRepair
                    && string.IsNullOrEmpty(xml) == false)
                {
                    int nRet = RepairAllErrorFromReaderSide(
                        channel,
                        recpath,
                        xml,
                        out strError);
                    if (nRet == -1)
                    {
                        DisplayRepairError("*** 修复读者记录 " + caption + " 内链条问题时出错: " + strError);
                    }
                    else
                    {
                        DisplaySucceed("- 成功修复读者记录 " + caption + " 内链条问题");
                        nRepairedCount++;
                    }
                }

                nCount++;
            }
            finally
            {
            }

            if (bAutoRepair)
                return nRepairedCount;
            return nCount;
        }

        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(Program.MainForm.DataDir, "checkborrowinfo.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = webBrowser_resultInfo.Document;

                if (doc == null)
                {
                    webBrowser_resultInfo.Navigate("about:blank");
                    doc = webBrowser_resultInfo.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser_resultInfo,
                "<html><head>" + strLink + strJs + "</head><body>");
        }

        // Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode(text) + "</div>");

        static string PlainText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string xml = "<root>" + text + "</root>";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return text;
            }

            return dom.DocumentElement.InnerText.Trim();
        }

        // 显示文字中的 <ib> <pb> <ip> <pp> 标记中所指的册记录或者读者记录
        void DisplayRecord(string strReaderBarcode,
            string strItemBarcode,
            string strError)
        {
            if (this.DisplayRecords == false)
                return;

            if (string.IsNullOrEmpty(strError))
                return;

            string xml = "<root>" + strError + "</root>";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch
            {
                return;
            }

            StringBuilder text = new StringBuilder();

            List<LinkInfo> links = new List<LinkInfo>();

            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                links.Add(new LinkInfo { Type = "patronBarcode", Value = strReaderBarcode });
            }

            if (string.IsNullOrEmpty(strItemBarcode) == false)
            {
                links.Add(new LinkInfo { Type = "itemBarcode", Value = strItemBarcode });
            }

            var nodes = dom.DocumentElement.SelectNodes("*");
            foreach (XmlElement node in nodes)
            {
                // 可能有空的 link 文字
                if (string.IsNullOrEmpty(node.InnerText.Trim()))
                    continue;

                LinkInfo current = null;
                if (node.Name == "ib")
                {
                    current = new LinkInfo
                    {
                        Type = "itemBarcode",
                        Value = node.InnerText.Trim()
                    };
                }
                else if (node.Name == "pb")
                {
                    current = new LinkInfo
                    {
                        Type = "patronBarcode",
                        Value = node.InnerText.Trim()
                    };
                }
                else if (node.Name == "ip")
                {
                    current = new LinkInfo
                    {
                        Type = "itemPath",
                        Value = node.InnerText.Trim()
                    };
                }
                else if (node.Name == "pp")
                {
                    current = new LinkInfo
                    {
                        Type = "patronPath",
                        Value = node.InnerText.Trim()
                    };
                }

                if (current != null)
                {
                    links.Add(current);
                }
            }

            List<LinkInfo> targets = new List<LinkInfo>();
            foreach (var current in links)
            {
                var newly = LinkInfo.AddLink(targets, current);
                if (newly)
                {
                    LibraryChannel channel = this.GetChannel();
                    try
                    {
                        int ret = current.GetData(this,
                            channel,
                            out string error);
                        if (ret == -1 || ret == 0)
                        {
                            string strClass = "record item";
                            if (current.Type.StartsWith("patron"))
                                strClass = "record patron";
                            text.AppendLine($"<span class='{strClass}'><div class='debug error'>" + HttpUtility.HtmlEncode(error) + "</div></span>");

                            // LinkInfo.RemoveLink(targets, current);
                            continue;
                        }
                    }
                    finally
                    {
                        this.ReturnChannel(channel);
                    }
                    string html = BuildItemHtml(current);
                    if (html != null)
                    {
                        string strClass = "record item";
                        if (current.Type.StartsWith("patron"))
                            strClass = "record patron";

                        text.AppendLine($"<span class='{strClass}'><div class='title'>{HttpUtility.HtmlEncode(current.GetTitle())}</div>" + html + "</span>");
                    }
                }
            }

            if (text.Length > 0)
            {
                this.Invoke((Action)(() =>
                {
                    Global.WriteHtml(this.webBrowser_resultInfo,
                        text.ToString()
                        );
                }));
            }
        }

        class LinkInfo
        {
            public string Type { get; set; }
            public string Value { get; set; }

            public string RecPath { get; set; }
            public string Barcode { get; set; }
            public string RefID { get; set; }
            public string Xml { get; set; }

            public string GetTitle()
            {
                if (this.Type.StartsWith("item"))
                {
                    if (string.IsNullOrEmpty(Barcode))
                        return $"册 @refID:{RefID} ({RecPath})";

                    return $"册 {Barcode} ({RecPath})";
                }
                else if (this.Type.StartsWith("patron"))
                {
                    return $"读者 {Barcode} ({RecPath})";
                }
                return RecPath;
            }

            public static bool RemoveLink(List<LinkInfo> links, LinkInfo link)
            {
                return links.Remove(link);
            }

            // 尝试加入一个 LinkInfo 对象到集合中。如果在集合中已经存在相同的记录，则不会加入
            // return:
            //      false   没有加入
            //      true    已经加入
            public static bool AddLink(List<LinkInfo> links, LinkInfo link)
            {
                foreach (var current in links)
                {
                    if (current.Type.StartsWith("item"))
                    {
                        if (link.Type.StartsWith("item") == false)
                            continue;

                        if (link.Type == "itemBarcode"
    && link.Value == current.Value)
                            return false;

                        if (link.Type == "itemBarcode"
                            && link.Value == current.Barcode)
                            return false;
                        if (link.Type == "itemBarcode"
    && link.Value == ("@refID:" + current.RefID))
                            return false;
                        if (link.Type == "itemPath"
    && link.Value == current.RecPath)
                            return false;
                    }
                    else if (current.Type.StartsWith("patron"))
                    {
                        if (link.Type.StartsWith("patron") == false)
                            continue;

                        if (link.Type == "patronBarcode"
    && link.Value == current.Value)
                            return false;

                        if (link.Type == "patronBarcode"
                            && link.Value == current.Barcode)
                            return false;
                        if (link.Type == "patronPath"
    && link.Value == current.RecPath)
                            return false;
                    }
                }

                links.Add(link);

                return true;
            }

            public int GetData(
                CheckBorrowInfoForm form,
                LibraryChannel channel,
                out string strError)
            {
                strError = "";
                long lRet = 0;

            REDO_GETDATA:
                string xml = "";
                string recpath = "";
                if (this.Type == "itemBarcode")
                {
                    // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                    lRet = channel.GetItemInfo(
                        null,
                        this.Value,
                        "xml",
                        out xml,
                        out recpath,
                        out _,
                        "",  // strBiblioType
                        out _,
                        out _,
                        out strError);
                    if (lRet == 0)
                    {
                        strError = $"册 {this.Value} 不存在";
                        return 0;
                    }
                }
                else if (this.Type == "itemPath")
                {
                    // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                    lRet = channel.GetItemInfo(
                        null,
                        "@path:" + this.Value,
                        "xml",
                        out xml,
                        out recpath,
                        out _,
                        "",  // strBiblioType
                        out _,
                        out _,
                        out strError);
                    if (lRet == 0)
                    {
                        strError = $"册 {this.Value} 不存在";
                        return 0;
                    }
                }

                else if (this.Type == "patronBarcode")
                {
                    lRet = channel.GetReaderInfo(null,
this.Value,
"xml",
out string[] results,
out recpath,
out _,
out strError);
                    if (lRet == 0)
                    {
                        strError = $"读者 {this.Value} 不存在";
                        return -1;
                    }
                    if (lRet != -1 && results != null && results.Length > 0)
                        xml = results[0];
                }

                else if (this.Type == "patronPath")
                {
                    lRet = channel.GetReaderInfo(null,
"@path:" + this.Value,
"xml",
out string[] results,
out recpath,
out _,
out strError);
                    if (lRet == 0)
                    {
                        strError = $"读者 {this.Value} 不存在";
                        return -1;
                    }
                    if (lRet != -1 && results != null && results.Length > 0)
                        xml = results[0];
                }
                else
                {
                    strError = $"未知的 Type '{this.Type}'";
                    return -1;
                }

                if (lRet == -1)
                {
                    MessagePromptEventArgs e = new MessagePromptEventArgs();
                    e.MessageText = $"获取记录 {this.Value} 时发生错误： " + strError;
                    e.Actions = "yes,no";
                    form.loader_Prompt(this, e);
                    if (e.ResultAction == "yes")
                        goto REDO_GETDATA;

                    strError = $"获取记录 {this.Value} 时出错: {strError}";
                    return -1;
                }

                this.Xml = xml;
                this.RecPath = recpath;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    strError = $"{recpath} XML 装入 DOM 出错: {ex.Message}";
                    return -1;
                }

                this.Barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                this.RefID = DomUtil.GetElementText(dom.DocumentElement, "refID");
                return 1;
            }
        }

        static string BuildItemHtml(LinkInfo link)
        {
            XmlDocument item_dom = new XmlDocument();
            try
            {
                item_dom.LoadXml(link.Xml);
            }
            catch (Exception ex)
            {
                return $"记录 {link.RecPath} 的 XML 记录装入 XMLDOM 出错: {ex.Message}";
            }

            DomUtil.RemoveEmptyElements(item_dom.DocumentElement);

            if (link.Type.StartsWith("item"))
            {
                string[] names = new string[] {
                "borrowHistory"
                };
                foreach (var name in names)
                {
                    DomUtil.DeleteElement(item_dom.DocumentElement, name);
                }
            }
            else if (link.Type.StartsWith("patron"))
            {
                string[] names = new string[] {
                "borrowHistory",
                "face",
                "fingerprint",
                "palmprint",
                "devolvedBorrows",
                "outofReservations",
                "hire",
                "file",
                };
                foreach (var name in names)
                {
                    DomUtil.DeleteElement(item_dom.DocumentElement, name);
                }
            }

            return DigitalPlatform.Marc.MarcUtil.GetHtmlOfXml(item_dom.DocumentElement.OuterXml,
false);
        }

#if NO
        public static string GetHtmlOfXml(string strFragmentXml,
    bool bSubfieldReturn)
        {
            StringBuilder strResult = new StringBuilder("\r\n<table class='item'>", 4096);

            if (string.IsNullOrEmpty(strFragmentXml) == false)
            {
                strResult.Append(DigitalPlatform.Marc.MarcUtil.GetFragmentHtml(strFragmentXml));
            }

            strResult.Append("\r\n</table>");
            return strResult.ToString();
        }
#endif

        void DisplayError(string title, string strText)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div>"
                    + "<div class='debug warning'>" + HttpUtility.HtmlEncode(title) + "</div>"
                    + "<div class='debug error'>" + HttpUtility.HtmlEncode(strText).Replace("。", "。<br/>") + "</div>"
                    + "</div>"
                    );
            }));
        }

        void DisplayError(string strText)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div>"
                    + "<div class='debug error'>" + HttpUtility.HtmlEncode(strText) + "</div>"
                    + "</div>"
                    );
            }));
        }

        void DisplayCheckError(string title, string strText)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div>"
                    + "<div class='debug warning'>" + HttpUtility.HtmlEncode(title) + "</div>"
                    + "<div class='debug check'>" + HttpUtility.HtmlEncode(strText).Replace("。", "。<br/>") + "</div>"
                    + "</div>"
                    );
            }));
        }

        void DisplayCheckError(string strText)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div>"
                    + "<div class='debug check'>" + HttpUtility.HtmlEncode(strText) + "</div>"
                    + "</div>"
                    );
            }));
        }

        void DisplayRepairError(string strText)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div>"
                    + "<div class='debug error'>" + HttpUtility.HtmlEncode(strText) + "</div>"
                    + "</div>"
                    );
            }));
        }

        void DisplaySucceed(string strText)
        {
            this.Invoke((Action)(() =>
            {
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div>"
                    + "<div class='debug green'>" + HttpUtility.HtmlEncode(strText) + "</div>"
                    + "</div>"
                    );
            }));
        }

        void DisplayText(string strText)
        {
            this.Invoke((Action)(() =>
            {
                /*
                Global.WriteHtml(this.webBrowser_resultInfo,
                    HttpUtility.HtmlEncode(strText) + "<br/>");
                */
                Global.WriteHtml(this.webBrowser_resultInfo,
                    "<div class='info'>" + HttpUtility.HtmlEncode(strText) + "</div>");
                // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
                this.webBrowser_resultInfo.Document.Window.ScrollTo(0,
                    this.webBrowser_resultInfo.Document.Body.ScrollRectangle.Height);
            }));
        }

#if OLD
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

#endif

#if OLD
        // parameters:
        //      bAutoRepair 是否同时自动修复
        int CheckReaderRecords(
            LibraryChannel channel,
            List<string> barcodes,
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

                    if (bFoundError
                        && bAutoRepair
                        && string.IsNullOrEmpty(strReaderXml) == false)
                    {
                        int nRet = RepairAllErrorFromReaderSide(
                            channel,
                            strReaderRecPath,
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
#endif


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
            this.Invoke((Action)(() =>
            {
                this.button_beginCheckFromReader.Enabled = bEnable;
                this.button_beginCheckFromItem.Enabled = bEnable;
                this.button_clearInfo.Enabled = bEnable;
                this.button_beginRepairFromItem.Enabled = bEnable;
                this.button_beginRepairFromReader.Enabled = bEnable;

                /*
                this.button_repairReaderSide.Enabled = bEnable;
                this.button_repairItemSide.Enabled = bEnable;
                this.textBox_itemBarcode.Enabled = bEnable;
                this.textBox_readerBarcode.Enabled = bEnable;
                */

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

                this.button_single_repairFromItem.Enabled = bEnable;
                this.button_single_repairFromReader.Enabled = bEnable;

                this.checkBox_displayRecords.Enabled = bEnable;
            }));
        }

#if OLD
        private void button_beginCheckFromItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bAutoRepair = Control.ModifierKeys == Keys.Control;

            List<string> barcodes = null;
            int nRet = SearchAllItemBarcode(
                true,
                out barcodes,
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
#endif

        private void button_beginCheckFromItem_Click(object sender, EventArgs e)
        {
            bool bAutoRepair = Control.ModifierKeys == Keys.Control;
            BeginCheckFromItem(bAutoRepair);
        }

        void BeginCheckFromItem(bool bAutoRepair)
        {
            string strError = "";

            // barcode --> 重复次数
            Hashtable barcode_table = new Hashtable();

            bool checkDup = this.checkBox_checkItemBarcodeDup.Checked;

            CancellationToken token = new CancellationToken();
            _ = Task.Factory.StartNew(
                () =>
                {
                    DisplayText("");

                    DateTime start = DateTime.Now;
                    if (bAutoRepair)
                        DisplayText($"{start} 正在进行检查和自动修复...");
                    else
                        DisplayText($"{start} 正在进行检查...");

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在检索 ...");
                    stop.BeginLoop();

                    EnableControls(false);
                    try
                    {
                        int count = 0;
                        int repaired_count = 0;

                        List<string> unprocessed_dbnames = new List<string>();

                        // result.Value
                        //      -1  出错
                        //      >=0 实际获得的读者记录条数
                        var result = DownloadAllEntityRecord(
                            null,
                            unprocessed_dbnames,
                            (channel, record) =>
                            {
                                count++;

                                // string format = "id,cols,format:@coldef:*/barcode|*/borrower";
                                if (record.Cols == null || record.Cols.Length < 2)
                                {
                                    DisplayError($"发现不正常的记录 {record.Path} {(record.Cols != null && record.Cols.Length > 0 ? record.Cols[0] : "record.Cols 为空")}");
                                    return;
                                }

                                string barcode = record.Cols[0];
                                string borrower = record.Cols[1];
                                if (string.IsNullOrEmpty(borrower)
                                    && string.IsNullOrEmpty(barcode) == false)
                                    return; // 跳过不是在借状态的册。但如果册条码号为空则不跳过，还要在后面继续处理

                                string xml = "";
                                // 册条码号允许为空。这时候要获得 refID 元素
                                if (string.IsNullOrEmpty(barcode))
                                {
                                    // return:
                                    //      -1  出错
                                    //      0   没有找到
                                    //      >=1 命中的条数
                                    int nRet = GetItemInfo(
                                        channel,
                                        "@path:" + record.Path,
                                        out xml,
                                        out string recpath,
                                        out string get_error);
                                    if (nRet == -1 || nRet == 0)
                                    {
                                        DisplayError(get_error);
                                        return;
                                    }
                                }
                                else
                                {
                                    // 合成一条册记录
                                    XmlDocument item_dom = new XmlDocument();
                                    item_dom.LoadXml("<root />");
                                    DomUtil.SetElementText(item_dom.DocumentElement, "barcode", barcode);
                                    DomUtil.SetElementText(item_dom.DocumentElement, "borrower", borrower);
                                    xml = item_dom.DocumentElement.OuterXml;
                                }

                                // parameters:
                                //      bAutoRepair 是否同时自动修复
                                int ret = CheckItemRecord(channel,
                                        record.Path,
                                        xml,  // record.RecordBody.Xml,
                                        bAutoRepair,
                                        checkDup ? barcode_table : null,
                                        out string error);
                                if (ret == -1)
                                {
                                    if (channel.ErrorCode == ErrorCode.RequestCanceled)
                                        throw new ChannelException(channel.ErrorCode, error);
                                }
                                else
                                    repaired_count += ret;

                            },
                            (text) =>
                            {
                            },
                            token);
                        if (result.Value == -1)
                        {
                            DisplayError(result.ErrorInfo);
                            strError = result.ErrorInfo;
                        }
                        else
                            strError = "OK";

                        DateTime end = DateTime.Now;

                        if (bAutoRepair)
                            DisplayText($"{end} 修复结束。共检查册记录 {count} 条，修复有问题的册记录 {repaired_count} 条。");
                        else
                            DisplayText($"{end} 检查结束。共检查册记录 {count} 条。");
                        ShowMessageBox(strError);
                        return;
                    }
                    catch (Exception ex)
                    {
                        strError = $"异常: {ex.Message}";
                        goto ERROR1;
                    }
                    finally
                    {
                        EnableControls(true);

                        stop.EndLoop();
                        stop.OnStop -= new StopEventHandler(this.DoStop);
                        stop.Initial("");
                        stop.HideProgress();
                    }
                ERROR1:
                    ShowMessageBox(strError);
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            // MessageBox.Show(this, "OK");
            return;
        }

        // parameters:
        //      unprocessed_dbnames 返回没有来得及处理的实体库名
        public NormalResult DownloadAllEntityRecord(
            List<string> item_dbnames,
            List<string> unprocessed_dbnames,
            Delegate_processRecord processRecord,
            Delegate_writeLog writeLog,
            CancellationToken token)
        {
            writeLog?.Invoke($"开始下载全部册记录到本地缓存。item_dbnames={StringUtil.MakePathList(item_dbnames)}");

            int total_processed = 0;

            LibraryChannel channel = this.GetChannel();
            var old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(5);  // 设置 5 分钟。因为册记录检索需要一定时间
            try
            {
                bool first_round = false;
                if (item_dbnames == null)
                {
                    first_round = true;
                    long lRet = channel.GetSystemParameter(
    null,
    "item",
    "dbnames",
    out string strValue,
    out string strError);
                    if (lRet == -1)
                    {
                        writeLog?.Invoke($"下载全部册记录到本地缓存出错: {strError}");
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    }
                    item_dbnames = StringUtil.SplitList(strValue);
                    StringUtil.RemoveBlank(ref item_dbnames);
                }

                unprocessed_dbnames.AddRange(item_dbnames);

                DisplayText($"共有 {item_dbnames.Count} 个实体库。{StringUtil.MakePathList(item_dbnames, ",")}");

                int db_index = -1;
                foreach (string name in item_dbnames)
                {
                    db_index++;
                    // func_showProgress?.Invoke($"正在从 {dbName} 获取信息 ...");

                    // name 形态为 数据库名:开始偏移
                    string dbName = name;
                    long start = 0;
                    {
                        var parts = StringUtil.ParseTwoPart(name, ":");
                        dbName = parts[0];
                        string offset = parts[1];
                        if (string.IsNullOrEmpty(offset) == false)
                        {
                            if (long.TryParse(offset, out start) == false)
                            {
                                string error = $"条目 '{name}' 格式不正确。应为 数据库名:偏移量 形态";
                                writeLog?.Invoke($"下载全部册记录到本地缓存出错: {error}");
                                return new NormalResult
                                {
                                    Value = -1,
                                    ErrorInfo = error
                                };
                            }
                        }
                    }

                // int nRedoCount = 0;
                REDO:
                    if (token.IsCancellationRequested)
                    {
                        string error = "用户中断";
                        writeLog?.Invoke($"下载全部册记录到本地缓存出错: {error}");
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = error
                        };
                    }
                    string resultset_name = "#download_" + Guid.NewGuid().ToString();
                    // 检索一个实体库的全部记录
                    long lRet = channel.SearchItem(null,
    dbName, // "<all>",
    "",
    -1,
    "__id",
    "left",
    "zh",
    resultset_name,   // strResultSetName
    "", // strSearchStyle
    "", // strOutputStyle
    out string strError);
                    if (lRet == -1)
                    {
                        strError = $"检索实体库 {dbName} 时出错: {strError}";

                        writeLog?.Invoke($"SearchItem() 出错, strError={strError}, channel.ErrorCode={channel.ErrorCode}");

                        /*
                        // 一次重试机会
                        if (lRet == -1
                            && (channel.ErrorCode == ErrorCode.RequestCanceled || channel.ErrorCode == ErrorCode.RequestError)
                            && nRedoCount < 2)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        */

                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = strError;
                        e.Actions = "yes,no,cancel";
                        loader_Prompt(this, e);
                        if (e.ResultAction == "no")
                            continue;
                        else if (e.ResultAction == "yes")
                            goto REDO;

                        writeLog?.Invoke($"下载全部册记录到本地缓存出错: {strError}");
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError,
                            ErrorCode = channel.ErrorCode.ToString()
                        };
                    }

                    long hitcount = lRet;

                    writeLog?.Invoke($"{dbName} 共检索命中册记录 {hitcount} 条");
                    DisplayText($"(库 {db_index + 1}/{item_dbnames.Count}) {dbName} 中有 {hitcount} 条册记录。");


                    DateTime search_time = DateTime.Now;

                    int skip_count = 0;
                    int error_count = 0;
                    int succeed_count = 0;

                    if (hitcount > 0)
                    {
                        stop.SetProgressRange(0, hitcount);

                        // string strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/uid";

                        // 把超时时间改短一点
                        var timeout0 = channel.Timeout;
                        channel.Timeout = TimeSpan.FromSeconds(20);
                        try
                        {
                            string format = "id,cols,format:@coldef:*/barcode|*/borrower";

                            // 获取和存储记录
                            ResultSetLoader loader = new ResultSetLoader(channel,
                stop,
                resultset_name,
                format, //$"id,xml",
                "zh");
                            loader.Start = start;

                            loader.Prompt += this.loader_Prompt;
                            long i = start;
                            foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    int index = IndexOf(unprocessed_dbnames, dbName);
                                    if (index != -1)
                                    {
                                        unprocessed_dbnames.RemoveAt(index);
                                        unprocessed_dbnames.Insert(index, dbName + ":" + i);
                                    }

                                    string error = "用户中断";
                                    writeLog?.Invoke($"下载全部册记录到本地缓存出错: {error}");
                                    return new NormalResult
                                    {
                                        Value = -1,
                                        ErrorInfo = error
                                    };
                                }

                                stop.SetMessage($"正在处理 {record.Path} ... (实体库 {db_index + 1}/{item_dbnames.Count})");

                                // 
                                processRecord?.Invoke(channel, record);

                                i++;
                                succeed_count++;

                                stop.SetProgressValue(i);
                            }
                        }
                        finally
                        {
                            channel.Timeout = timeout0;
                        }
                    }

                    total_processed += succeed_count;

                    // writeLog?.Invoke($"dbName='{dbName}'。skip_count={skip_count}, error_count={error_count}");
                    writeLog?.Invoke($"实体库 '{dbName}' 下载完成 {succeed_count} 条记录");

                    {
                        int index = IndexOf(unprocessed_dbnames, dbName);
                        if (index != -1)
                            unprocessed_dbnames.RemoveAt(index);
                    }
                }

                writeLog?.Invoke($"下载全部册记录到本地缓存，全部数据库成功完成");

                return new NormalResult
                {
                    Value = total_processed,
                };
            }
            catch (ChannelException ex)
            {
                if (ex.ErrorCode == ErrorCode.RequestCanceled)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "用户中断",
                    };

                ClientInfo.WriteErrorLog($"DownloadAllEntityRecordAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                writeLog?.Invoke($"DownloadAllEntityRecordAsync() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllEntityRecordAsync() 出现异常：{ex.Message}"
                };
            }
            catch (Exception ex)
            {
                ClientInfo.WriteErrorLog($"DownloadAllEntityRecordAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");

                writeLog?.Invoke($"DownloadAllEntityRecordAsync() 出现异常：{ExceptionUtil.GetDebugText(ex)}");

                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"DownloadAllEntityRecordAsync() 出现异常：{ex.Message}"
                };
            }
            finally
            {
                channel.Timeout = old_timeout;
                this.ReturnChannel(channel);

                writeLog?.Invoke($"结束下载全部册记录到本地缓存。unprocessed_dbnames={StringUtil.MakePathList(unprocessed_dbnames)}");
            }

            int IndexOf(List<string> names, string db_name)
            {
                int j = 0;
                foreach (var one in names)
                {
                    if (db_name == one || one.StartsWith(db_name + ":"))
                        return j;
                    j++;
                }

                return -1;
            }
        }

#if OLD
        // 检索获得所有册条码号
        int SearchAllItemBarcode(
            bool bHasBorrower,
            out List<string> barcodes,
            out string strError)
        {
            strError = "";

            barcodes = new List<string>();

            Hashtable dup_table = new Hashtable();  // barcode --> 记录路径

            TimeSpan old_timeout = this.Channel.Timeout;
            this.Channel.Timeout = TimeSpan.FromMinutes(30);

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
                    "check",   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                long lHitCount = lRet;

                stop.SetProgressRange(0, lHitCount);

                Global.WriteHtml(this.webBrowser_resultInfo,
    "共有 " + lHitCount.ToString() + " 条册记录。\r\n");

                string strStyle = "id,cols,format:@coldef:*/barcode|*/borrower";

                this.Channel.Timeout = TimeSpan.FromSeconds(60);

                ResultSetLoader loader = new ResultSetLoader(this.Channel,
                    stop,
                    "check",
                    strStyle);
                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                int geted = 0;
                foreach (Record record in loader)
                {
                    if ((geted % 100) == 0)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    geted++;

                    if (record.Cols == null || record.Cols.Length < 2)
                        goto CONTINUE;
                    string strBorrower = record.Cols[1];
                    string strBarcode = record.Cols[0];
                    if (string.IsNullOrEmpty(strBarcode))
                        goto CONTINUE;

                    strBarcode = strBarcode.Trim();
                    if (string.IsNullOrEmpty(strBarcode))
                        goto CONTINUE;

                    // 判重
                    if (this.checkBox_checkItemBarcodeDup.Checked
                        && string.IsNullOrEmpty(strBarcode) == false)
                    {
                        if (dup_table.ContainsKey(strBarcode))
                        {
                            string strOldRecPath = (string)dup_table[strBarcode];
                            Global.WriteHtml(this.webBrowser_resultInfo,
    "册条码号 " + strBarcode + " 发现重复。记录 " + strOldRecPath + " 和 " + record.Path + "\r\n");

                        }
                        else
                            dup_table[strBarcode] = record.Path;
                    }

                    if (bHasBorrower == true
                        && string.IsNullOrEmpty(strBorrower))
                        goto CONTINUE;

                    barcodes.Add(strBarcode);

                CONTINUE:
                    stop.SetMessage("共有条码 " + lHitCount.ToString() + " 个。已获得条码 " + barcodes.Count + " / " + geted.ToString() + " 个");
                    stop.SetProgressValue(geted);
                }

                // 排序、去重
                stop.SetMessage("正在排序和去重");

                // 排序
                barcodes.Sort();
                StringUtil.RemoveDup(ref barcodes, true);
            }
            catch (Exception ex)
            {
                strError = "SearchAllItemBarcode() 出现异常: " + ExceptionUtil.GetDebugText(ex);
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.Channel.Timeout = old_timeout;
            }

            return 0;
        ERROR1:
            return -1;
        }
#endif

        // "yes,no,cancel"
        // "yes,no"
        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            this.Invoke((Action)(() =>
            {
                // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
                if (e.Actions == "yes,no,cancel")
                {
                    //      DialogResult.Retry 表示超时了
                    //      DialogResult.OK 表示点了 OK 按钮
                    //      DialogResult.Cancel 表示点了右上角的 Close 按钮
                    //      DialogResult.Ignore 表示点了 跳过 按钮
                    DialogResult result = AutoCloseMessageBox.ShowIgnore(this,
        e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
        20 * 1000,
        "CheckBorrowInfoForm");
                    if (result == DialogResult.Cancel)
                        e.ResultAction = "cancel";
                    else if (result == DialogResult.Ignore)
                        e.ResultAction = "no";
                    else
                        e.ResultAction = "yes";
                }
                else if (e.Actions == "yes,no")
                {
                    //      DialogResult.Retry 表示超时了
                    //      DialogResult.OK 表示点了 OK 按钮
                    //      DialogResult.Cancel 表示点了右上角的 Close 按钮
                    DialogResult result = AutoCloseMessageBox.Show(this,
        e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
        20 * 1000,
        "CheckBorrowInfoForm");
                    if (result == DialogResult.Cancel)
                        e.ResultAction = "no";
                    else
                        e.ResultAction = "yes";
                }
                else
                    throw new Exception($"不支持的 actions '{e.Actions}'");
            }));
        }

#if OLD
        // 检索获得所有册条码号(另一版本，输出到文件)
        int SearchAllItemBarcode(string strBarcodeFilename,
            bool bHasBorrower,
            out string strError)
        {
            strError = "";

            string strStyle = "id,cols,format:@coldef:*/barcode|*/borrower";

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
                            strStyle,   // "id,cols",
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
                        foreach (Record record in searchresults)
                        {
                            if (record.Cols == null || record.Cols.Length < 2)
                                continue;
                            string strBorrower = record.Cols[1];
                            string strBarcode = record.Cols[0];
                            if (string.IsNullOrEmpty(strBarcode))
                                continue;
                            if (bHasBorrower == true
                                && string.IsNullOrEmpty(strBorrower))
                                continue;
                            sw.Write(strBarcode + "\r\n");
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
#endif
        // parameters:
        //      bAutoRepair 是否同时自动修复
        // return:
        //      -1  出错
        //      0   没有必要处理
        //      1   已经处理
        int CheckItemRecord(LibraryChannel channel,
            string recpath,
            string xml,
            bool bAutoRepair,
            Hashtable barcode_table,
            out string strError)
        {
            strError = "";

            int nCount = 0;
            int nRepairedCount = 0;

            string[] aDupPath = null;
            try
            {
                if (string.IsNullOrEmpty(xml))
                    return 0;

                XmlDocument item_dom = new XmlDocument();
                try
                {
                    item_dom.LoadXml(xml);
                }
                catch (Exception ex)
                {
                    strError = $"实体 XML 记录装入 XMLDOM 时出错: {ex.Message}";
                    return -1;
                }

                string strItemBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(strItemBarcode))
                {
                    string refID = DomUtil.GetElementText(item_dom.DocumentElement, "refID");
                    if (string.IsNullOrEmpty(refID))
                    {
                        DisplayError($"册记录 { recpath } 既没有 barcode 元素，也没有 refID 元素，格式不合法。请尽快修正此问题");
                        DisplayRecord(null, null, $"<ip>{recpath}<ip>");
                        return -1;
                    }
                    strItemBarcode = "@refID:" + refID;
                }

                string caption = $"{strItemBarcode}({recpath})";

                // 条码号查重
                if (barcode_table != null && string.IsNullOrEmpty(strItemBarcode) == false)
                {
                    int dup_count = 1;
                    if (barcode_table.ContainsKey(strItemBarcode) == false)
                    {
                        barcode_table[strItemBarcode] = dup_count;
                    }
                    else
                    {
                        dup_count = (int)barcode_table[strItemBarcode];
                        dup_count++;
                        barcode_table[strItemBarcode] = dup_count;
                    }

                    if (dup_count > 1)
                    {
                        DisplayCheckError($"册条码号 { strItemBarcode } 有重复记录 { dup_count }条。({recpath})");
                    }
                }

                string strReaderBarcode = DomUtil.GetElementText(item_dom.DocumentElement, "borrower");
                if (string.IsNullOrEmpty(strReaderBarcode))
                    return 0;   // 没有必要检查

                string strOutputReaderBarcode = "";

                //stop.SetMessage("正在检查第 " + (i + 1).ToString() + " 个册记录，条码为 " + strItemBarcode);
                //stop.SetProgressValue(i);

                int nProcessedBorrowItems = 0;
                int nTotalBorrowItems = 0;

            REDO_REPAIR:
                long lRet = channel.RepairBorrowInfo(
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
                    /*
                    if (lRet == -1
                        && channel.ErrorCode == ErrorCode.RequestCanceled)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = $"检查册记录 {caption} 时发生错误： " + strError;
                        e.Actions = "yes,no,cancel";
                        loader_Prompt(this, e);
                        if (e.ResultAction == "cancel")
                            throw new ChannelException(channel.ErrorCode, strError);
                        else if (e.ResultAction == "yes")
                            goto REDO_REPAIR;

                        DisplayError($"检查册记录 { caption} 时出错: { strError}");
                        return -1;
                    }
                    */
                    if (lRet == -1)
                    {
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = $"检查册记录 {caption} 时发生错误： " + strError;
                        e.Actions = "yes,no";
                        loader_Prompt(this, e);
                        if (e.ResultAction == "yes")
                            goto REDO_REPAIR;

                        DisplayError($"检查册记录 { caption } 时出错: ", PlainText(strError));
                        DisplayRecord(strReaderBarcode, strItemBarcode, strError);
                    }

                    if (channel.ErrorCode == ErrorCode.ItemBarcodeDup)
                    {
                        List<string> linkedPath = new List<string>();

                        DisplayCheckError("检查册记录 " + caption + " 时发现册条码号命中重复记录 " + aDupPath.Length.ToString() + "个 -- " + StringUtil.MakePathList(aDupPath) + "。");

                        for (int j = 0; j < aDupPath.Length; j++)
                        {
                            string strText = " 检查其中第 " + (j + 1).ToString() + " 个，路径为 " + aDupPath[j] + ": ";


                        REDO_2:
                            string[] aDupPathTemp = null;
                            // string strOutputReaderBarcode = "";
                            long lRet_2 = channel.RepairBorrowInfo(
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
                                MessagePromptEventArgs e = new MessagePromptEventArgs();
                                e.MessageText = $"检查册记录 {caption} 时发生错误： " + strError;
                                e.Actions = "yes,no,cancel";
                                loader_Prompt(this, e);
                                if (e.ResultAction == "no")
                                    continue;
                                else if (e.ResultAction == "yes")
                                    goto REDO_2;

                                goto ERROR1;
                            }
                            if (lRet_2 == 1)
                            {
                                strText += "发现问题: " + strError;

                                DisplayCheckError(strText);

                                if (bAutoRepair)
                                {
                                    int nRet = RepairErrorFromItemSide(
                                        channel,
                                        strItemBarcode,
                                        aDupPath[j],
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        DisplayRepairError("*** 修复册记录 " + caption + " 内链条问题时出错: " + strError);
                                    }
                                    else
                                    {
                                        DisplaySucceed("- 成功修复册记录 " + caption + " 内链条问题");
                                        nRepairedCount++;
                                    }
                                }
                            }

                        } // end of for

                        return 1;
                    }

                    if (lRet == 1)
                    {
                        DisplayCheckError($"检查册记录 { caption } 时发现问题: ", PlainText(strError));
                        DisplayRecord(strReaderBarcode, strItemBarcode, strError);

                        if (bAutoRepair)
                        {
                            int nRet = RepairErrorFromItemSide(
                                channel,
                                strItemBarcode,
                                "",
                                out strError);
                            if (nRet == -1)
                            {
                                DisplayRepairError("*** 修复册记录 " + caption + " 内链条问题时出错: " + strError);
                            }
                            else
                            {
                                DisplaySucceed("- 成功修复册记录 " + caption + " 内链条问题");
                                nRepairedCount++;
                            }
                        }
                    }
                    return 1;
                } // end of return -1

                /*
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
                */

                nCount++;
            }
            finally
            {
            }

            if (bAutoRepair)
                return nRepairedCount;
            return nCount;
        ERROR1:
            return -1;
        }

#if OLD
        // parameters:
        //      bAutoRepair 是否同时自动修复
        int CheckItemRecords(
            LibraryChannel channel,
            List<string> barcodes,
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

                    long lRet = channel.RepairBorrowInfo(
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
                                long lRet_2 = channel.RepairBorrowInfo(
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

                                    if (bAutoRepair)
                                    {
                                        int nRet = RepairErrorFromItemSide(
                                            channel,
                                            strItemBarcode,
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

                            if (bAutoRepair)
                            {
                                int nRet = RepairErrorFromItemSide(
                                    channel,
                                    strItemBarcode,
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
#endif

        // 零星修复，从读者侧
        private void button_single_repairFromReader_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.ClearHtml();

            string strReaderBarcode = this.textBox_single_readerBarcode.Text;
            string strItemBarcode = this.textBox_single_itemBarcode.Text;

            if (string.IsNullOrEmpty(strItemBarcode))
            {
                // 修复读者相关的全部错误

                // return:
                //      -1  错误。可能有部分册已经修复成功
                //      其他  共修复多少个册事项
                nRet = RepairAllErrorFromReaderSide(
                    strReaderBarcode,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有发现任何错误";
                    goto ERROR1;
                }
            }
            else
            {
                // 修复和指定册相关的错误

                nRet = RepairError(
                    "repairreaderside",
                    strReaderBarcode,
                    strItemBarcode,
                    out strError);
            }
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
        int RepairErrorFromItemSide(
            LibraryChannel channel,
            string strItemBarcode,
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

        REDO_GETITEM:
            // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
            long lRet = channel.GetItemInfo(
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
                // 处理通讯错误
                MessagePromptEventArgs e = new MessagePromptEventArgs();
                e.MessageText = $"获取册 {strItemBarcode} 时发生错误： " + strError;
                e.Actions = "yes,no";
                loader_Prompt(this, e);
                if (e.ResultAction == "yes")
                    goto REDO_GETITEM;

                // 改用 strConfirmItemRecPath 试一下
                if (string.IsNullOrEmpty(strConfirmItemRecPath) == false)
                {
                REDO_GETITEM2:
                    lRet = channel.GetItemInfo(
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
                        // 处理通讯错误
                        MessagePromptEventArgs e1 = new MessagePromptEventArgs();
                        e1.MessageText = $"获取册 {"@path:" + strConfirmItemRecPath} 时发生错误： " + strError;
                        e1.Actions = "yes,no";
                        loader_Prompt(this, e1);
                        if (e1.ResultAction == "yes")
                            goto REDO_GETITEM2;

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

        REDO_REPAIR:
            string[] aDupPath = null;

            string strOutputReaderBarcode = "";
            int nProcessedBorrowItems = 0;
            int nTotalBorrowItems = 0;
            lRet = channel.RepairBorrowInfo(
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
                if (channel.ErrorCode == ErrorCode.NoError)
                    return 0;

                // 处理通讯错误
                MessagePromptEventArgs e = new MessagePromptEventArgs();
                e.MessageText = $"修复册记录 {strItemBarcode} 时发生错误： " + strError;
                e.Actions = "yes,no";
                loader_Prompt(this, e);
                if (e.ResultAction == "yes")
                    goto REDO_REPAIR;

                return -1;
            }
            else
                return 1;
        }

        // return:
        //      -1  错误。可能有部分册已经修复成功
        //      其他  共修复多少个册事项
        int RepairAllErrorFromReaderSide(
    string strReaderBarcode,
    out string strError)
        {
            strError = "";
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行修复 ...");
            stop.BeginLoop();

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();
            try
            {
            REDO_GET:
                long lRet = channel.GetReaderInfo(stop,
        strReaderBarcode,
        "xml",
        out string[] results,
        out string strReaderRecPath,
        out _,
        out strError);
                if (lRet == -1)
                {
                    MessagePromptEventArgs e = new MessagePromptEventArgs();
                    e.MessageText = $"获取读者记录 {strReaderBarcode} 时发生错误： " + strError;
                    e.Actions = "yes,no";
                    loader_Prompt(this, e);
                    if (e.ResultAction == "yes")
                        goto REDO_GET;
                    return -1;
                }
                if (lRet == 0)
                {
                    strError = $"证条码号 '{strReaderBarcode}' 没有找到";
                    return -1;
                }

                if (results == null || results.Length == 0)
                {
                    strError = "results error";
                    return -1;
                }

                string strReaderXml = results[0];

                // return:
                //      -1  错误。可能有部分册已经修复成功
                //      其他  共修复多少个册事项
                return RepairAllErrorFromReaderSide(
                    channel,
                    strReaderRecPath,
                    strReaderXml,
                    out strError);
            }
            finally
            {
                this.ReturnChannel(channel);

                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }

        // 从读者侧出发，修复一个读者记录中，所有册记录的链条错误
        // parameters:
        //      strReaderXml    读者记录 XML。从中可以获知有哪些册条码号(相关的链)需要尝试进行修复
        // return:
        //      -1  错误。可能有部分册已经修复成功
        //      其他  共修复多少个册事项
        int RepairAllErrorFromReaderSide(
            LibraryChannel channel,
            string strReaderRecPath,
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

            REDO_REPAIR:
                long lRet = channel.RepairBorrowInfo(
                    stop,
                    "repairreaderside",
                    strReaderBarcode,
                    strItemBarcode,
                    "", // 2022/1/10 // strConfirmItemRecPath,
                    0,
                    -1,
                    out int nProcessedBorrowItems,   // 2008/10/27 
                    out int nTotalBorrowItems,   // 2008/10/27 
                    out string strOutputReaderBarcode,
                    out string[] aDupPath,
                    out strError);
                if (lRet == -1)
                {
                    if (channel.ErrorCode != ErrorCode.NoError)
                    {
                        // 处理通讯错误
                        MessagePromptEventArgs e = new MessagePromptEventArgs();
                        e.MessageText = $"修复读者记录 {strReaderBarcode} 时发生错误： " + strError;
                        e.Actions = "yes,no,cancel";
                        loader_Prompt(this, e);
                        if (e.ResultAction == "no")
                            continue;
                        else if (e.ResultAction == "yes")
                            goto REDO_REPAIR;

                        Debug.Assert(e.ResultAction == "cancel");
                        return -1;
                    }

                    if (channel.ErrorCode != ErrorCode.NoError)
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

            LibraryChannel channel = this.GetChannel();

            try
            {
                string strConfirmItemRecPath = "";
            REDO:
                string[] aDupPath = null;

                string strOutputReaderBarcode = "";

                long lRet = channel.RepairBorrowInfo(
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
                    MessagePromptEventArgs e = new MessagePromptEventArgs();
                    e.MessageText = $"修复记录 {strReaderBarcode} {strItemBarcode} 时发生错误： " + strError;
                    e.Actions = "yes,no";
                    loader_Prompt(this, e);
                    if (e.ResultAction == "yes")
                        goto REDO;

                    if (channel.ErrorCode == ErrorCode.ItemBarcodeDup)
                    {
                        LibraryChannel channel0 = this.GetChannel();
                        try
                        {
                            ItemBarcodeDupDlg dupdlg = new ItemBarcodeDupDlg();
                            MainForm.SetControlFont(dupdlg, this.Font, false);
                            string strErrorNew = "";
                            int nRet = dupdlg.Initial(
                                // Program.MainForm,
                                aDupPath,
                                "因册条码号发生重复，修复操作被拒绝。\r\n\r\n可根据下面列出的详细信息，选择适当的册记录，重试操作。\r\n\r\n原始出错信息:\r\n" + strError,
                                channel0,    // Program.MainForm.Channel,
                                Program.MainForm.Stop,
                                out strErrorNew);
                            if (nRet == -1)
                            {
                                // 初始化对话框失败
                                MessageBox.Show(this, strErrorNew);
                                goto ERROR1;
                            }

                            Program.MainForm.AppInfo.LinkFormState(dupdlg, "CheckBorrowInfoForm_dupdlg_state");
                            dupdlg.ShowDialog(this);
                            Program.MainForm.AppInfo.UnlinkFormState(dupdlg);

                            if (dupdlg.DialogResult == DialogResult.Cancel)
                                goto ERROR1;

                            strConfirmItemRecPath = dupdlg.SelectedRecPath;

                            goto REDO;
                        }
                        finally
                        {
                            this.ReturnChannel(channel0);
                        }
                    }

                    goto ERROR1;
                } // end of return -1
            }
            finally
            {
                this.ReturnChannel(channel);

                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 零星修复，从册侧
        private void button_single_repairFromItem_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.ClearHtml();

            string strItemBarcode = this.textBox_single_itemBarcode.Text;
            string strReaderBarcode = this.textBox_single_readerBarcode.Text;

            if (string.IsNullOrEmpty(strReaderBarcode))
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获取册记录 ...");
                stop.BeginLoop();

                EnableControls(false);

                LibraryChannel channel = this.GetChannel();

                try
                {
                REDO_GET:
                    // Result.Value -1出错 0没有找到 1找到 >1命中多于1条
                    long lRet = channel.GetItemInfo(
                        stop,
                        strItemBarcode,
                        "xml",
                        out string strItemXml,
                        out string strItemRecPath,
                        out _,
                        "",  // strBiblioType
                        out _,
                        out _,
                        out strError);
                    if (lRet == -1)
                    {
                        MessagePromptEventArgs e1 = new MessagePromptEventArgs();
                        e1.MessageText = $"获取册 {strItemBarcode} 时发生错误： " + strError;
                        e1.Actions = "yes,no";
                        loader_Prompt(this, e1);
                        if (e1.ResultAction == "yes")
                            goto REDO_GET;

                        goto ERROR1;
                    }
                    if (lRet == 0)
                        goto ERROR1;

                    XmlDocument item_dom = new XmlDocument();
                    try
                    {
                        item_dom.LoadXml(strItemXml);
                    }
                    catch (Exception ex)
                    {
                        strError = $"册记录 XML 装入 XMLDOM 出错: {ex.Message}";
                        goto ERROR1;
                    }

                    string borrower = DomUtil.GetElementText(item_dom.DocumentElement, "borrower");
                    if (string.IsNullOrEmpty(borrower))
                    {
                        strError = $"册 {strItemBarcode} 处于在架状态，无法单独进行修复。请输入证条码号再尝试修复一次";
                        goto ERROR1;
                    }
                    strReaderBarcode = borrower;
                }
                finally
                {
                    this.ReturnChannel(channel);

                    EnableControls(true);

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                }
            }

            nRet = RepairError(
                "repairitemside",
                strReaderBarcode,
                strItemBarcode,
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
            throw new NotImplementedException();
#if OLD
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
                    false,
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
#endif
        }

#if OLD
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

                    // TODO: 册记录如果已经有 refid 要沿用已有的
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
#endif

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
            // Global.ClearForPureTextOutputing(this.webBrowser_resultInfo);
            ClearHtml();
        }

#if OLD
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

#endif

        // 零星检查，从册侧
        private void button_single_checkFromItem_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.ClearHtml();

            bool bAutoRepair = Control.ModifierKeys == Keys.Control;

            string strItemBarcode = this.textBox_single_itemBarcode.Text;
            string strReaderBarcode = this.textBox_single_readerBarcode.Text;

            if (string.IsNullOrEmpty(strItemBarcode))
            {
                strError = "尚未指定要检查的册条码号";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strReaderBarcode) == false)
            {
                strError = "零星检查册记录时，不允许输入证条码号";
                goto ERROR1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行检查 ...");
            stop.BeginLoop();

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();

            //string[] aDupPath = null;
            //string strText = "";
            try
            {
            REDO_GET:
                // 根据册条码号获得册记录
                long lRet = channel.GetItemInfo(stop,
                    strItemBarcode,
                    "xml",
                    out string xml,
                    out string recpath,
                    out _,
                    "",
                    out _,
                    out _,
                    out strError);
                if (lRet == -1)
                {
                    MessagePromptEventArgs e1 = new MessagePromptEventArgs();
                    e1.MessageText = $"获取册 {strItemBarcode} 时发生错误： " + strError;
                    e1.Actions = "yes,no";
                    loader_Prompt(this, e1);
                    if (e1.ResultAction == "yes")
                        goto REDO_GET;

                    goto ERROR1;
                }

                if (lRet == 0)
                {
                    strError = $"册 {strItemBarcode} 不存在";
                    goto ERROR1;
                }

                int nRet = CheckItemRecord(channel,
                    recpath,
                    xml,
                    bAutoRepair,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strError = PlainText(strError);

                if (string.IsNullOrEmpty(strError))
                    MessageBox.Show(this, "没有发现问题。");
                else
                    MessageBox.Show(this, strError);
                return;
            }
            finally
            {
                this.ReturnChannel(channel);

                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

#if OLD
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

            lRet = Channel.GetSearchResult(stop,
                "dup",
                0,
                Math.Min(lHitCount, 100),
                "zh",
                out List<string> aPath,
                out strError);
            if (lRet == -1)
                return -1;

            paths = new string[aPath.Count];
            aPath.CopyTo(paths);

            return (int)lHitCount;
        }
#endif

        private void CheckBorrowInfoForm_Activated(object sender, EventArgs e)
        {
            Program.MainForm.stopManager.Active(this.stop);
        }

        // 零星检查，从读者侧
        private void button_single_checkFromReader_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.ClearHtml();

            bool bAutoRepair = Control.ModifierKeys == Keys.Control;

            string strReaderBarcode = this.textBox_single_readerBarcode.Text;
            string strItemBarcode = this.textBox_single_itemBarcode.Text;

            if (string.IsNullOrEmpty(strReaderBarcode))
            {
                strError = "尚未指定要检查的读者证条码号";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strItemBarcode) == false)
            {
                strError = "零星检查读者记录时，不允许输入册条码号";
                goto ERROR1;
            }

            /*
            List<string> barcodes = new List<string>();
            barcodes.Add(this.textBox_single_readerBarcode.Text);

            nRet = CheckReaderRecords(barcodes,
                bAutoRepair,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            */

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行检查 ...");
            stop.BeginLoop();

            EnableControls(false);

            LibraryChannel channel = this.GetChannel();
            try
            {
            REDO_GET:
                // 根据证条码号获得读者记录 XML
                long lRet = channel.GetReaderInfo(stop,
                    strReaderBarcode,
                    "xml,recpaths",
                    out string[] results,
                    out strError);
                if (lRet == -1)
                {
                    MessagePromptEventArgs e1 = new MessagePromptEventArgs();
                    e1.MessageText = $"获取读者 {strReaderBarcode} 时发生错误： " + strError;
                    e1.Actions = "yes,no";
                    loader_Prompt(this, e1);
                    if (e1.ResultAction == "yes")
                        goto REDO_GET;

                    goto ERROR1;
                }
                if (lRet == 0)
                {
                    strError = $"证条码号为 '{strReaderBarcode}' 的读者记录不存在";
                    goto ERROR1;
                }

                string xml = results[0];
                string recpath = results[1];

                nRet = CheckReaderRecord(channel,
                    recpath,
                    xml,
                    bAutoRepair,
                    null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                strError = PlainText(strError);

                if (string.IsNullOrEmpty(strError))
                    MessageBox.Show(this, "没有发现问题。");
                else
                    MessageBox.Show(this, strError);
                return;
            }
            finally
            {
                this.ReturnChannel(channel);

                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 带有重试功能的 GetItemInfo
        // return:
        //      -1  出错
        //      0   没有找到
        //      >=1 命中的条数
        int GetItemInfo(
            LibraryChannel channel,
            string strItemBarcode,
            out string xml,
            out string recpath,
            out string strError)
        {
        REDO_GET:
            // 根据册条码号获得册记录
            long lRet = channel.GetItemInfo(stop,
                strItemBarcode,
                "xml",
                out xml,
                out recpath,
                out _,
                "",
                out _,
                out _,
                out strError);
            if (lRet == -1)
            {
                MessagePromptEventArgs e1 = new MessagePromptEventArgs();
                e1.MessageText = $"获取册 {strItemBarcode} 时发生错误： " + strError;
                e1.Actions = "yes,no";
                loader_Prompt(this, e1);
                if (e1.ResultAction == "yes")
                    goto REDO_GET;

                return -1;
            }

            if (lRet == 0)
            {
                strError = $"册 {strItemBarcode} 不存在";
                return 0;
            }

            return (int)lRet;
        }

        // 批修复，从读者侧
        private void button_beginRepairFromReader_Click(object sender, EventArgs e)
        {
            BeginCheckFromReader(true);
        }

        // 批修复，从册侧
        private void button_beginRepairFromItem_Click(object sender, EventArgs e)
        {
            BeginCheckFromItem(true);
        }
    }
}