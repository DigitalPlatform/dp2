using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Web;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

using Ionic.Zip;
using System.Data.SQLite;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Xml;

using DigitalPlatform.dp2.Statis;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.Range;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Script;

namespace dp2Circulation
{
    /// <summary>
    /// 报表窗
    /// </summary>
    public partial class ReportForm : MyForm
    {
        ReportConfigBuilder _cfg = new ReportConfigBuilder();

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            this.UiState = this.MainForm.AppInfo.GetString(GetReportSection(), "ui_state", "");

#if NO
            string strError = "";
            int nRet = this.MainForm.VerifySerialCode("report", out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.MainForm, "报表窗需要先设置序列号才能使用");
                API.PostMessage(this.Handle, API.WM_CLOSE, 0, 0);
                return;
            }
#endif

            DelayCheck();
        }

        delegate void Delegate_Check();

        void DelayCheck()
        {
            Delegate_Check d = new Delegate_Check(Check);
            this.BeginInvoke(d);
        }

        void Check()
        {
            string strError = "";
            int nRet = _cfg.LoadCfgFile(GetBaseDirectory(), "report_def.xml", out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            _cfg.FillList(this.listView_libraryConfig);

            SetStartButtonStates();
            SetDailyReportButtonState();
            // DelaySetUploadButtonState();
            BeginUpdateUploadButtonText();

            if (this.MainForm.ServerVersion < 2.31)
                MessageBox.Show(this, "报表窗需要和 dp2library 2.31 以上版本配套使用。(当前 dp2library 版本为 " + this.MainForm.ServerVersion.ToString() + ")\r\n\r\n请及时升级 dp2library 到最新版本");
            else
            {
                double version = 0;
                // 读入断点信息的版本号
                // return:
                //      -1  出错
                //      0   文件不存在
                //      1   成功
                nRet = GetBreakPointVersion(
                out version,
                out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                else if (nRet == 1)
                {
                    if (version < _version)
                    {
                        MessageBox.Show(this, "由于程序升级，本地存储的结构定义发生改变，请注意稍后重新从头创建本地存储");
                    }
                }
            }
        }

        private void ReportForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ReportForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopUpdateUploadButtonText();

            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.Close();
                }
                catch
                {
                }
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
                this.MainForm.AppInfo.SetString(
                    GetReportSection(), 
                    "ui_state", 
                    this.UiState);

            // 删除所有输出文件
            if (this.OutputFileNames != null)
            {
                Global.DeleteFiles(this.OutputFileNames);
                this.OutputFileNames = null;
            }

            if (_cfg != null)
                _cfg.Save();
        }


        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.tabControl1.Enabled = bEnable;
            this.toolStrip_main.Enabled = bEnable;
        }


        string _connectionString = "";
        // const int INSERT_BATCH = 100;  // 300;



        // 根据日志文件创建本地 operlogxxx 表
        int DoCreateOperLogTable(
            long lProgressStart,
            string strStartDate,
            string strEndDate,
            out string strLastDate,
            out long lLastIndex,
            out string strError)
        {
            strError = "";
            strLastDate = "";
            lLastIndex = 0;

            int nRet = 0;

            // strEndDate 里面可能会包含 ":0-99" 这样的附加成分
            string strLeft = "";
            string strEndRange = "";
            StringUtil.ParseTwoPart(strEndDate,
                ":",
                out strLeft,
                out strEndRange);
            strEndDate = strLeft;

            string strStartRange = "";
            StringUtil.ParseTwoPart(strStartDate,
                ":",
                out strLeft,
                out strStartRange);
            strStartDate = strLeft;

            // TODO: start 和 end 都有 range，而且 start 和 end 是同一天怎么办?

            // 删除不必要的索引
            {
                this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

                foreach (string type in OperLogTable.DbTypes)
                {
                    nRet = OperLogTable.DeleteAdditionalIndex(
                        type,
                        this._connectionString,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            List<string> filenames = null;

            string strWarning = "";

            // 根据日期范围，发生日志文件名
            // parameters:
            //      strStartDate    起始日期。8字符
            //      strEndDate  结束日期。8字符
            // return:
            //      -1  错误
            //      0   成功
            nRet = OperLogStatisForm.MakeLogFileNames(strStartDate,
                strEndDate,
                true,  // true,
                out filenames,
                out strWarning,
                out strError);
            if (nRet == -1)
                return -1;

            if (String.IsNullOrEmpty(strWarning) == false)
                MessageBox.Show(this, strWarning);

            if (filenames.Count > 0 && string.IsNullOrEmpty(strEndRange) == false)
            {
                filenames[filenames.Count - 1] = filenames[filenames.Count - 1] + ":" + strEndRange;
            }
            if (filenames.Count > 0 && string.IsNullOrEmpty(strStartRange) == false)
            {
                filenames[0] = filenames[0] + ":" + strStartRange;
            }

            this.Channel.Timeout = new TimeSpan(0, 1, 0);   // 一分钟

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                ProgressEstimate estimate = new ProgressEstimate();

                OperLogLoader loader = new OperLogLoader();
                loader.Channel = this.Channel;
                loader.Stop = this.Progress;
                // loader.owner = this;
                loader.estimate = estimate;
                loader.FileNames = filenames;
                loader.nLevel = 2;  //  this.MainForm.OperLogLevel;
                loader.AutoCache = false;
                loader.CacheDir = "";
                loader.Filter = "borrow,return,setReaderInfo,setBiblioInfo,setEntity,setOrder,setIssue,setComment,amerce,passgate,getRes";

                loader.ProgressStart = lProgressStart;

                loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                // List<OperLogLine> circu_lines = new List<OperLogLine>();
                MultiBuffer buffer = new MultiBuffer();
                buffer.Initial();
                OperLogLineBase.MainForm = this.MainForm;

                try
                {
                    int nRecCount = 0;
                    foreach (OperLogItem item in loader)
                    {
                        string strXml = item.Xml;

                        if (string.IsNullOrEmpty(strXml) == true)
                        {
                            nRecCount++;
                            continue;
                        }

                        {
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(strXml);
                            }
                            catch (Exception ex)
                            {
                                strError = item.Date + " 中偏移为 " + item.Index.ToString() + " 的日志记录 XML 装载到 DOM 时出错: " + ex.Message;
                                DialogResult result = MessageBox.Show(this,
strError + "\r\n\r\n是否跳过此条记录继续处理?",
"ReportForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                                if (result == DialogResult.No)
                                    return -1;
                                continue;
                            }

                            string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
#if NO
                                if (strOperation != "borrow" && strOperation != "return")
                                {
                                    nRecCount++;
                                    continue;
                                }
#endif
                            nRet = buffer.AddLine(
                                strOperation,
                                dom,
                                item.Date,
                                item.Index,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            // -2 不要报错
                        }

                        bool bForce = false;
                        if (nRecCount >= 4000)
                            bForce = true;
                        nRet = buffer.WriteToDb(connection,
                            true,
                            bForce,
                            out strError);
                        if (bForce == true)
                        {
                            strLastDate = item.Date;
                            lLastIndex = item.Index + 1;
                            nRecCount = 0;
                        }
                        nRecCount++;
#if NO
                            if (circu_lines.Count >= INSERT_BATCH
    || (circu_lines.Count > 0 && nCircuRecCount >= 1000))
                            {
                                // 写入数据库一次
                                nRet = OperLogLine.AppendOperLogLines(
                                    connection,
                                    circu_lines,
                                    true,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                circu_lines.Clear();

                                strLastDate = item.Date;
                                lLastIndex = item.Index + 1;
                                nCircuRecCount = 0;
                            }

                            nCircuRecCount++;
#endif

                    }
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetDebugText(ex);
                    return -1;
                }

#if NO
                    if (circu_lines.Count > 0)
                    {
                        // 写入数据库一次
                        nRet = OperLogLine.AppendOperLogLines(
                            connection,
                            circu_lines,
                            true,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // 表示处理完成
                        strLastDate = "";
                        lLastIndex = 0;
                    }
#endif
                nRet = buffer.WriteToDb(connection,
                    true,
                    true,   // false,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 表示处理完成
                strLastDate = "";
                lLastIndex = 0;
            }

            return 0;
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            // TODO: 不再出现此对话框。不过重试有个次数限制，同一位置失败多次后总要出现对话框才好
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = MessageBox.Show(this,
    e.MessageText + "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试;  否: 跳过本次操作，继续后面的操作; 取消: 停止全部操作)",
    "ReportForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                    e.ResultAction = "yes";
                else if (result == DialogResult.Cancel)
                    e.ResultAction = "cancel";
                else
                    e.ResultAction = "no";
            }
        }

        // 根据当前命中数，调整进度条总范围
        void AdjustProgressRange(long lOldCount, long lNewCount)
        {
            if (this.stop == null)
                return;

            long lDelta = lNewCount - lOldCount;
            if (lDelta != 0)
            {
                this.stop.SetProgressRange(this.stop.ProgressMin, this.stop.ProgressMax + lDelta);
                if (this._estimate != null)
                    this._estimate.EndPosition += lDelta;
            }
        }

        // 复制册记录
        // parameters:
        //      lIndex  [in] 起点 index
        //              [out] 返回中断位置的 index
        int BuildItemRecords(
            string strItemDbNameParam,
            long lOldCount,
            ref long lProgress,
            ref long lIndex,
            out string strError)
        {
            strError = "";

            int nRet = 0;
            lProgress += lIndex;

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                long lRet = this.Channel.SearchItem(stop,
                    strItemDbNameParam,
                    "", // (lIndex+1).ToString() + "-", // 
                    -1,
                    "__id",
                    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                    "zh",
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0; 
                
                long lHitCount = lRet;

                AdjustProgressRange(lOldCount, lHitCount);

                long lStart = lIndex;
                long lCount = lHitCount - lIndex;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                // bool bOutputBiblioRecPath = false;
                // bool bOutputItemRecPath = false;
                string strStyle = "";

                {
                    // bOutputBiblioRecPath = true;
                    strStyle = "id,cols,format:@coldef:*/barcode|*/location|*/accessNo|*/parent|*/state|*/operations/operation[@name='create']/@time|*/borrower|*/borrowDate|*/borrowPeriod|*/returningDate|*/price";
                }

                // 实体库名 --> 书目库名
                Hashtable dbname_table = new Hashtable();

                List<ItemLine> lines = new List<ItemLine>();

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                        return -1;
                    }

                    lRet = this.Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        return 0;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

                        ItemLine line = new ItemLine();
                        line.ItemRecPath = searchresult.Path;
                        line.ItemBarcode = searchresult.Cols[0];
                        line.Location = searchresult.Cols[1];
                        line.AccessNo = searchresult.Cols[2];

                        line.State = searchresult.Cols[4];
#if NO
                        try
                        {
                            line.CreateTime = DateTimeUtil.Rfc1123DateTimeStringToLocal(
                                searchresult.Cols[5], "u");
                        }
                        catch
                        {
                        }
#endif
                        line.CreateTime = SQLiteUtil.GetLocalTime(searchresult.Cols[5]);

                        line.Borrower = searchresult.Cols[6];
                        line.BorrowTime = SQLiteUtil.GetLocalTime(searchresult.Cols[7]);
                        line.BorrowPeriod = searchresult.Cols[8];
                        // line.ReturningTime = ItemLine.GetLocalTime(searchresult.Cols[9]);

                        if (string.IsNullOrEmpty(line.BorrowTime) == false)
                        {
                            string strReturningTime = "";
                            // parameters:
                            //      strBorrowTime   借阅起点时间。u 格式
                            //      strReturningTime    返回应还时间。 u 格式
                            nRet = AmerceOperLogLine.BuildReturingTimeString(line.BorrowTime,
                line.BorrowPeriod,
                out strReturningTime,
                out strError);
                            if (nRet == -1)
                            {
                                line.ReturningTime = "";
                            }
                            else
                                line.ReturningTime = strReturningTime;
                        }
                        else
                            line.ReturningTime = "";

                        string strPrice = searchresult.Cols[10];
                        long value = 0;
                        string strUnit = "";
                        nRet = AmerceOperLogLine.ParsePriceString(strPrice,
                out value,
                out strUnit,
                out strError);
                        if (nRet == -1)
                        {
                            line.Price = 0;
                            line.Unit = "";
                        }
                        else
                        {
                            line.Price = value;
                            line.Unit = strUnit;
                        }

                        string strItemDbName = Global.GetDbName(searchresult.Path);
                        string strBiblioDbName = (string)dbname_table[strItemDbName];
                        if (string.IsNullOrEmpty(strBiblioDbName) == true)
                        {
                            strBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(strItemDbName);
                            dbname_table[strItemDbName] = strBiblioDbName;
                        }

                        string strBiblioRecPath = strBiblioDbName + "/" + searchresult.Cols[3];

                        line.BiblioRecPath = strBiblioRecPath;
                        lines.Add(line);
                    }

                    if (true)
                    {
#if NO
                        int nStart = 0;
                        for (;; )
                        {
                            List<ItemLine> lines1 = new List<ItemLine>();
                            int nLength = Math.Min(100, lines.Count - nStart);
                            if (nLength <= 0)
                                break;
                            lines1.AddRange(lines.GetRange(nStart, nLength));
                            // 插入一批记录
                            nRet = ItemLine.AppendItemLines(
                                connection,
                                lines1,
                                true,   // 用 false 可以在测试阶段帮助发现重叠插入问题
                                out strError);
                            if (nRet == -1)
                                return -1;
                            nStart += nLength;
                        }
#endif

                        // 插入一批记录
                        nRet = ItemLine.AppendItemLines(
                            connection,
                            lines,
                            true,   // 用 false 可以在测试阶段帮助发现重叠插入问题
                            out strError);
                        if (nRet == -1)
                            return -1;
                        lIndex += lines.Count;
                        lines.Clear();
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    lProgress += searchresults.Length;
                    // stop.SetProgressValue(lProgress);
                    SetProgress(lProgress);

                    stop.SetMessage(strItemDbNameParam + " " + lStart.ToString() + "/" + lHitCount.ToString() + " "
                        + GetProgressTimeString(lProgress));

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                if (lines.Count > 0)
                {
                    Debug.Assert(false, "");
                } 

                return 0;
            }
        }

        // safe set progress value, between max and min
        void SetProgress(long lProgress)
        {
            if (lProgress <= stop.ProgressMax)
                stop.SetProgressValue(lProgress);
            else if (stop.ProgressValue < stop.ProgressMax)
                stop.SetProgressValue(stop.ProgressMax);
        }

        // 复制读者记录
        int BuildReaderRecords(
            string strReaderDbNameParam,
            long lOldCount,
            ref long lProgress,
            ref long lIndex,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            lProgress += lIndex;

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                long lRet = this.Channel.SearchReader(stop,
                    strReaderDbNameParam,
                    "", // (lIndex + 1).ToString() + "-", // 
                    -1,
                    "__id",
                    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                    "zh",
                    null,   // strResultSetName
                    // "",    // strSearchStyle
                    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;

                AdjustProgressRange(lOldCount, lHitCount);

                long lStart = lIndex;
                long lCount = lHitCount - lIndex;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                string strStyle = "id,cols,format:@coldef:*/barcode|*/department|*/readerType|*/name|*/state";

                // 读者库名 --> 图书馆代码
                Hashtable librarycode_table = new Hashtable();

                List<ReaderLine> lines = new List<ReaderLine>();
                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                        return -1;
                    }


                    lRet = this.Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        return 0;
                    }

                    // 处理浏览结果

                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

                        ReaderLine line = new ReaderLine();
                        line.ReaderRecPath = searchresult.Path;
                        line.ReaderBarcode = searchresult.Cols[0];
                        line.Department = searchresult.Cols[1];
                        line.ReaderType = searchresult.Cols[2];
                        line.Name = searchresult.Cols[3];
                        line.State = searchresult.Cols[4];

                        string strReaderDbName = Global.GetDbName(searchresult.Path);
                        string strLibraryCode = (string)librarycode_table[strReaderDbName];
                        if (string.IsNullOrEmpty(strLibraryCode) == true)
                        {
                            strLibraryCode = this.MainForm.GetReaderDbLibraryCode(strReaderDbName);
                            librarycode_table[strReaderDbName] = strLibraryCode;
                        }
                        line.LibraryCode = strLibraryCode;
                        lines.Add(line);
                    }

#if NO
                    if (lines.Count >= INSERT_BATCH
                        || ((lStart + searchresults.Length >= lHitCount || lCount - searchresults.Length <= 0) && lines.Count > 0)
                        )
#endif
                    {
                        // 插入一批读者记录
                        nRet = ReaderLine.AppendReaderLines(
                            connection,
                            lines,
                            true,   // 用 false 可以在测试阶段帮助发现重叠插入问题
                            out strError);
                        if (nRet == -1)
                            return -1;

                        lIndex += lines.Count;
                        lines.Clear();
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    // lIndex += searchresults.Length;
                    lProgress += searchresults.Length;
                    // stop.SetProgressValue(lProgress);
                    SetProgress(lProgress);

                    stop.SetMessage(strReaderDbNameParam + " " + lStart.ToString() + "/" + lHitCount.ToString() + " "
                        + GetProgressTimeString(lProgress));

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                if (lines.Count > 0)
                {
                    Debug.Assert(false, "");
                } 

                return 0;
            }
        }

        // 复制书目记录
        int BuildBiblioRecords(
            string strBiblioDbNameParam,
            long lOldCount,
            ref long lProgress,
            ref long lIndex,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            lProgress += lIndex;

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                string strQueryXml = "";

                long lRet = this.Channel.SearchBiblio(stop,
                    strBiblioDbNameParam,
                    "", // (lIndex + 1).ToString() + "-", // 
                    -1,
                    "recid",     // "__id",
                    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                    "zh",
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                    return -1;
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;

                AdjustProgressRange(lOldCount, lHitCount);

                long lStart = lIndex;
                long lCount = lHitCount - lIndex;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                // string strStyle = "id,cols,format:@coldef:*/barcode|*/department|*/readerType|*/name";
                string strStyle = "id";

                // 读者库名 --> 图书馆代码
                // Hashtable librarycode_table = new Hashtable();

                List<BiblioLine> lines = new List<BiblioLine>();
                List<string> biblio_recpaths = new List<string>();
                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                        return -1;
                    }

                    lRet = this.Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        return 0;
                    }

                    // 处理浏览结果

                    foreach (DigitalPlatform.CirculationClient.localhost.Record searchresult in searchresults)
                    {
                        // DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

                        BiblioLine line = new BiblioLine();
                        line.BiblioRecPath = searchresult.Path;
                        lines.Add(line);

                        biblio_recpaths.Add(searchresult.Path);
                    }

#if NO
                    if (lines.Count >= INSERT_BATCH
                        || ((lStart + searchresults.Length >= lHitCount || lCount - searchresults.Length <= 0) && lines.Count > 0)
                        )
#endif
                    {
                        Debug.Assert(biblio_recpaths.Count == lines.Count, "");

                        // 获得书目摘要
                        BiblioLoader loader = new BiblioLoader();
                        loader.Channel = this.Channel;
                        loader.Stop = this.Progress;
                        loader.Format = "summary";
                        loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                        loader.RecPaths = biblio_recpaths;

                        try
                        {
                            int i = 0;
                            foreach (BiblioItem item in loader)
                            {
                                // this.Progress.SetMessage("正在加入 " + (i + 1).ToString() + "/" + targetLeft.Count.ToString() + " 个书目摘要，可能需要较长时间 ...");

                                BiblioLine line = lines[i];
                                if (string.IsNullOrEmpty(item.Content) == false)
                                {
                                    if (item.Content.Length > 4000)
                                        line.Summary = item.Content.Substring(0, 4000);
                                    else
                                        line.Summary = item.Content;
                                }

                                i++;
                            }
                        }
                        catch (Exception ex)
                        {
                            strError = "ReportForm {72A00ADB-1F9F-45FA-A31E-6956569045D9} exception: " + ExceptionUtil.GetAutoText(ex);
                            return -1;
                        }
                        biblio_recpaths.Clear();

                        // 插入一批书目记录
                        nRet = BiblioLine.AppendBiblioLines(
                            connection,
                            lines,
                            true,   // 用 false 可以在测试阶段帮助发现重叠插入问题
                            out strError);
                        if (nRet == -1)
                            return -1;

                        lIndex += lines.Count;
                        lines.Clear();
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    // lIndex += searchresults.Length;
                    lProgress += searchresults.Length;
                    // stop.SetProgressValue(lProgress);
                    SetProgress(lProgress);

                    stop.SetMessage(strBiblioDbNameParam + " " + lStart.ToString() + "/" + lHitCount.ToString() + " "
                        + GetProgressTimeString(lProgress));

                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                if (lines.Count > 0)
                {
                    Debug.Assert(false, "");
                } 
                
                return 0;
            }
        }

        // 从计划文件获得所有分类号检索途径 style
        internal int GetClassFromStylesFromFile(
            out List<BiblioDbFromInfo> styles,
            out string strError)
        {
            strError = "";
            styles = new List<BiblioDbFromInfo>();

            string strBreakPointFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");
            XmlDocument task_dom = new XmlDocument();
            try
            {
                task_dom.Load(strBreakPointFileName);
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strBreakPointFileName + "' 时出错: " + ex.Message;
                return -1;
            }

            return GetClassFromStyles(
                task_dom.DocumentElement,
                out styles,
                out strError);
        }

        // 从计划文件获得所有分类号检索途径 style
        internal int GetClassFromStylesFromFile(out List<string> styles,
            out string strError)
        {
            strError = "";
            styles = new List<string>();

            string strBreakPointFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");
            XmlDocument task_dom = new XmlDocument();
            try
            {
                task_dom.Load(strBreakPointFileName);
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strBreakPointFileName + "' 时出错: " + ex.Message;
                return -1;
            }

            return GetClassFromStyles(
                task_dom.DocumentElement,
                out styles,
                out strError);
        }

        // 从计划文件中获得所有分类号检索途径 style
        internal int GetClassFromStyles(
            XmlElement root,
            out List<string> styles,
            out string strError)
        {
            strError = "";
            styles = new List<string>();

            XmlNodeList nodes = root.SelectNodes("classStyles/style");
            foreach (XmlElement element in nodes)
            {
                styles.Add(element.GetAttribute("style"));
            }
            return 0;
        }

        // 从计划文件中获得所有分类号检索途径 style
        internal int GetClassFromStyles(
            XmlElement root,
            out List<BiblioDbFromInfo> styles,
            out string strError)
        {
            strError = "";
            styles = new List<BiblioDbFromInfo>();

            XmlNodeList nodes = root.SelectNodes("classStyles/style");
            foreach (XmlElement element in nodes)
            {
                BiblioDbFromInfo info = new BiblioDbFromInfo();
                info.Caption = element.GetAttribute("caption");
                info.Style = element.GetAttribute("style");
                styles.Add(info);
            }
            return 0;
        }

        // 记忆书目库的分类号 style 列表
        int MemoryClassFromStyles(XmlElement root,
            out string strError)
        {
            strError = "";
            List<BiblioDbFromInfo> styles = null;
            int nRet = GetClassFromStylesFromMainform(out styles,
            out strError);
            if (nRet == -1)
                return -1;
            if (styles.Count == 0)
            {
                strError = "书目库尚未配置分类号检索点";
                return 0;
            }

            XmlElement container = root.SelectSingleNode("classStyles") as XmlElement;
            if (container == null)
            {
                container = root.OwnerDocument.CreateElement("classStyles");
                root.AppendChild(container);
            }
            else
                container.RemoveAll();

            foreach (BiblioDbFromInfo info in styles)
            {
                XmlElement style_element = root.OwnerDocument.CreateElement("style");
                container.AppendChild(style_element);
                style_element.SetAttribute("style", info.Style);
                style_element.SetAttribute("caption", info.Caption);
            }

            return 1;
        }

        // 获得所有分类号检索途径 style
        internal int GetClassFromStylesFromMainform(
            out List<BiblioDbFromInfo> styles,
            out string strError)
        {
            strError = "";
            styles = new List<BiblioDbFromInfo>();

            for (int i = 0; i < this.MainForm.BiblioDbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.MainForm.BiblioDbFromInfos[i];
                if (StringUtil.IsInList("__class", info.Style) == true)
                {
                    string strStyle = GetPureStyle(info.Style);
                    if (string.IsNullOrEmpty(strStyle) == true)
                    {
                        strError = "检索途径 " + info.Caption + " 的 style 值 '" + info.Style + "' 其中应该有至少一个不带 '_' 前缀的子串";
                        return -1;
                    }
                    BiblioDbFromInfo style = new BiblioDbFromInfo();
                    style.Caption = info.Caption;
                    style.Style = strStyle;
                    styles.Add(style);
                }
            }

            return 0;
        }

#if NO
        // 获得所有分类号检索途径 style
        internal int GetClassFromStyles(out List<string> styles,
            out string strError)
        {
            strError = "";
            styles = new List<string>();

            for (int i = 0; i < this.MainForm.BiblioDbFromInfos.Length; i++)
            {
                BiblioDbFromInfo info = this.MainForm.BiblioDbFromInfos[i];
                if (StringUtil.IsInList("__class", info.Style) == true)
                {
                    string strStyle = GetPureStyle(info.Style);
                    if (string.IsNullOrEmpty(strStyle) == true)
                    {
                        strError = "检索途径 "+info.Caption+" 的 style 值 '"+info.Style+"' 其中应该有至少一个不带 '_' 前缀的子串";
                        return -1;
                    }
                    styles.Add(strStyle);
                }
            }

            return 0;
        }
#endif

        // 获得不是 _ 和 __ 打头的 style 值
        static string GetPureStyle(string strText)
        {
            List<string> results = new List<string>();
            string[] parts = strText.Split(new char [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                if (s[0] == '_')
                    continue;
                results.Add(s);
            }

            return StringUtil.MakePathList(results);
        }

        // 复制分类号条目
        int BuildClassRecords(
            string strBiblioDbNameParam,
            string strClassFromStyle,
            string strClassTableName,
            long lOldCount,
            ref long lProgress,
            ref long lIndex,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            lProgress += lIndex;

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                string strQueryXml = "";

                long lRet = this.Channel.SearchBiblio(stop,
                    strBiblioDbNameParam,
                    "", // 
                    -1,
                    strClassFromStyle,     // "__id",
                    "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                    "zh",
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "keyid", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                {
                    if (this.Channel.ErrorCode == ErrorCode.FromNotFound)
                        return 0;
                    return -1;
                }
                if (lRet == 0)
                    return 0;

                long lHitCount = lRet;

                AdjustProgressRange(lOldCount, lHitCount);

                long lStart = lIndex;
                long lCount = lHitCount - lIndex;
                DigitalPlatform.CirculationClient.localhost.Record[] searchresults = null;

                // string strStyle = "id,cols,format:@coldef:*/barcode|*/department|*/readerType|*/name";
                string strStyle = "keyid,id,key";

                // 装入浏览格式
                List<ClassLine> lines = new List<ClassLine>();
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，用户中断...";
                        return -1;
                    }

                    lRet = this.Channel.GetSearchResult(
                        stop,
                        null,   // strResultSetName
                        lStart,
                        lCount,
                        strStyle, // bOutputKeyCount == true ? "keycount" : "id,cols",
                        "zh",
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        strError = "检索共命中 " + lHitCount.ToString() + " 条，已装入 " + lStart.ToString() + " 条，" + strError;
                        return -1;
                    }

                    if (lRet == 0)
                    {
                        return 0;
                    }

                    // 处理浏览结果
                    foreach (DigitalPlatform.CirculationClient.localhost.Record searchresult in searchresults)
                    {
                        // DigitalPlatform.CirculationClient.localhost.Record searchresult = searchresults[i];

                        ClassLine line = new ClassLine();
                        line.BiblioRecPath = searchresult.Path;
                        if (searchresult.Keys != null && searchresult.Keys.Length > 0)
                            line.Class = searchresult.Keys[0].Key;
                        lines.Add(line);

                    }

#if NO
                    if (lines.Count >= INSERT_BATCH
                        || ((lStart + searchresults.Length >= lHitCount || lCount - searchresults.Length <= 0) && lines.Count > 0)
                        )
#endif
                    {
                        // 插入一批分类号记录
                        nRet = ClassLine.AppendClassLines(
                            connection,
                            strClassTableName,
                            lines,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        lIndex += lines.Count;
                        lines.Clear();
                    }

                    lStart += searchresults.Length;
                    lCount -= searchresults.Length;

                    // lIndex += searchresults.Length;
                    lProgress += searchresults.Length;
                    // stop.SetProgressValue(lProgress);
                    SetProgress(lProgress);

                    stop.SetMessage(strBiblioDbNameParam + " " + strClassFromStyle + " " + lStart.ToString() + "/" + lHitCount.ToString() + " "
                        + GetProgressTimeString(lProgress));
                     
                    if (lStart >= lHitCount || lCount <= 0)
                        break;
                }

                if (lines.Count > 0)
                {
                    Debug.Assert(false, "");
                } 

                return 0;
            }
        }

#if NO
        // TODO strCfgFile 修改为使用 writer
        // 输出 Excel 报表文件
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int OutputExcelReport(
    Table table,
    string strCfgFile,
    Hashtable macro_table,
    string strOutputFileName,
    out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return -1;
            }

            List<bool> sums = new List<bool>();
            string strColumDefString = "";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("columns/column");
            foreach (XmlNode node in nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                string strAlign = DomUtil.GetAttr(node, "align");
                string strSum = DomUtil.GetAttr(node, "sum");

                bool bSum = StringUtil.GetBooleanValue(strSum, false);
                sums.Add(bSum);

                if (string.IsNullOrEmpty(strColumDefString) == false)
                    strColumDefString += ",";
                strColumDefString += strName;
            }

            string strTitle = DomUtil.GetElementText(dom.DocumentElement,
                "title").Replace("\\r", "|");
            string strTitleComment = DomUtil.GetElementText(dom.DocumentElement,
                "titleComment").Replace("\\r", "|");
            string strColumnSortStyle = DomUtil.GetElementText(dom.DocumentElement,
                "columnSortStyle");

            strTitle = Global.MacroString(macro_table, strTitle);
            strTitleComment = Global.MacroString(macro_table, strTitleComment);

            List<string> title_lines = StringUtil.SplitList(strTitle, '|');
            List<string> title_comment_lines = StringUtil.SplitList(strTitleComment, '|');

            Report report = Report.BuildReport(table,
                strColumDefString,  // "部门||department,借书(册)||borrowitem",
                "",
                false);
            if (report == null)
                return 0;

            int i = 0;
            foreach (PrintColumn column in report)
            {
                if (i >= sums.Count)
                    break;  // 因为 Columns 因为 hint 的缘故，可能会比这里定义的要多

                column.Sum = sums[i];
                i++;
            }

            // 写入输出文件
            if (string.IsNullOrEmpty(strOutputFileName) == true)
                strOutputFileName = this.NewOutputFileName();
            else
            {
                // 确保目录被创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
            }

            ExcelDocument doc = null;
            doc = ExcelDocument.Create(strOutputFileName);
            try
            {
                doc.NewSheet("Sheet1");

                // 输出标题文字
                int nColIndex = 0;
                int _lineIndex = 0;
                foreach (string t in title_lines)
                {
                    List<CellData> cells = new List<CellData>();
                    cells.Add(new CellData(nColIndex, t));
                    doc.WriteExcelLine(_lineIndex, cells);

#if NO
                    doc.WriteExcelCell(
                        _lineIndex,
                        nColIndex,
                        t,
                        true);
#endif
                    _lineIndex++;
                }

                foreach (string t in title_comment_lines)
                {
                    List<CellData> cells = new List<CellData>();
                    cells.Add(new CellData(nColIndex, t));
                    doc.WriteExcelLine(_lineIndex, cells);

#if NO
                    doc.WriteExcelCell(
                        _lineIndex,
                        nColIndex,
                        t,
                        true);
#endif
                    _lineIndex++;
                }

                // 输出 Excel 格式的表格
                // parameters:
                //      nTopLines   顶部预留多少行
                report.OutputExcelTable(table,
                    doc,
                    title_lines.Count + title_comment_lines.Count + 1,
                    -1);

                doc.SaveWorksheet();
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close();
                    doc = null;
                }
            }

            // File.SetAttributes(strOutputFileName, FileAttributes.Archive);
            return 1;
        }

#endif

#if NO
        static void WriteTitles(XmlTextWriter writer,
            string strTitleString)
        {
            List<string> titles = StringUtil.SplitList(strTitleString, '\r');
            WriteTitles(writer, titles);
        }

        static void WriteTitles(XmlTextWriter writer,
            List<string> titles)
        {
            int i = 0;
            foreach (string title in titles)
            {
                if (i > 0)
                    writer.WriteElementString("br", "");
                writer.WriteString(title);
                i++;
            }
        }
#endif

#if NO
        // 根据一个表格按照缺省特性创建一个Report对象
        // parameters:
        //		strDefaultValue	全部列的缺省值
        //				null表示不改变缺省值""，否则为strDefaultValue指定的值
        //		bSum	是否全部列都要参加合计
        //      bContentColumn  是否考虑内容行中比指定的栏目多出来的栏目
        public static Report BuildReport(SQLiteDataReader table,
            string strColumnTitles,
            string strDefaultValue,
            bool bSum,
            bool bContentColumn = true)
        {
            // Debug.Assert(false, "");
            if (table.HasRows == false)
                return null;	// 无法创建。内容必须至少一行以上

            Report report = new Report();

            // Line line = table.FirstHashLine();	// 随便得到一行。这样不要求table排过序

            // 列标题
            {
                PrintColumn column = new PrintColumn();
                column.ColumnNumber = -1;
                report.Add(column);
            }

            int nTitleCount = 0;

            if (strColumnTitles != null)
            {
                string[] aName = strColumnTitles.Split(new Char[] { ',' });
                nTitleCount = aName.Length;
            }

            int nColumnCount = nTitleCount;
            if (bContentColumn == true)
                nColumnCount = Math.Max(table.FieldCount, nTitleCount);


            // 检查表格第一行
            // 因为列标题column已经加入，所以现在最多加入nTitleCount-1栏
            for (int i = 0; i < nColumnCount - 1; i++)
            {
                PrintColumn column = new PrintColumn();
                column.ColumnNumber = i;

                if (strDefaultValue != null)
                    column.DefaultValue = strDefaultValue;

                column.Sum = bSum;

                report.Add(column);
            }


            // 添加列标题
            if (strColumnTitles != null)
            {
                string[] aName = strColumnTitles.Split(new Char[] { ',' });

                /*
                if (aName.Length < report.Count)
                {
                    string strError = "列定义 '" + strColumnTitles + "' 中的列数 " + aName.Length.ToString() + "小于报表实际最大列数 " + report.Count.ToString();
                    throw new Exception(strError);
                }*/


                int j = 0;
                for (j = 0; j < report.Count; j++)
                {
                    // 2007/10/26
                    if (j >= aName.Length)
                        break;

                    string strText = "";

                    strText = aName[j];

                    string strNameText = "";
                    string strNameClass = "";

                    int nRet = strText.IndexOf("||");
                    if (nRet == -1)
                        strNameText = strText;
                    else
                    {
                        strNameText = strText.Substring(0, nRet);
                        strNameClass = strText.Substring(nRet + 2);
                    }


                    PrintColumn column = (PrintColumn)report[j];
                    if (j < aName.Length)
                    {
                        column.Title = strNameText;
                        column.CssClass = strNameClass;
                    }
                }
            }

            report.SumLine = bSum;

            // 计算 colspan
            PrintColumn current = null;
            foreach (PrintColumn column in report)
            {
                if (string.IsNullOrEmpty(column.Title) == false
                    && column.Title[0] == '+'
                    && current != null)
                {
                    column.Colspan = 0; // 表示这是一个从属的列
                    current.Colspan++;
                }
                else
                    current = column;
            }

            return report;
        }

#endif

#if NO

        static Jurassic.ScriptEngine engine = null;


        // 输出 RML 格式的表格
        // 本函数负责写入 <table> 元素
        // parameters:
        //      nTopLines   顶部预留多少行
        public void OutputRmlTable(
            Report report,
            SQLiteDataReader table,
            XmlTextWriter writer,
            int nMaxLines = -1)
        {
            // StringBuilder strResult = new StringBuilder(4096);
            int i, j;

#if NO
            if (nMaxLines == -1)
                nMaxLines = table.Count;
#endif

            writer.WriteStartElement("table");
            writer.WriteAttributeString("class", "table");

            writer.WriteStartElement("thead");
            writer.WriteStartElement("tr");

            int nEvalCount = 0; // 具有 eval 的栏目个数
            for (j = 0; j < report.Count; j++)
            {
                PrintColumn column = (PrintColumn)report[j];
                if (column.Colspan == 0)
                    continue;

                if (string.IsNullOrEmpty(column.Eval) == false)
                    nEvalCount++;

                writer.WriteStartElement("th");
                if (string.IsNullOrEmpty(column.CssClass) == false)
                    writer.WriteAttributeString("class", column.CssClass);
                if (column.Colspan > 1)
                    writer.WriteAttributeString("colspan", column.Colspan.ToString());

                writer.WriteString(column.Title);
                writer.WriteEndElement();   // </th>
            }

            writer.WriteEndElement();   // </tr>
            writer.WriteEndElement();   // </thead>


            // 合计数组
            object[] sums = null;   // 2008/12/1 new changed

            if (report.SumLine)
            {
                sums = new object[report.Count];
                for (i = 0; i < sums.Length; i++)
                {
                    sums[i] = null;
                }
            }

            NumberFormatInfo nfi = new CultureInfo("zh-CN", false).NumberFormat;
            nfi.NumberDecimalDigits = 2;

            writer.WriteStartElement("tbody");

            // Jurassic.ScriptEngine engine = null;
            if (nEvalCount > 0 && engine == null)
            {
                engine = new Jurassic.ScriptEngine();
                engine.EnableExposedClrTypes = true;
            }

            // 内容行循环
            for (i = 0; ; i++)  // i < Math.Min(nMaxLines, table.Count)
            {
                if (table.HasRows == false)
                    break;
                // Line line = table[i];

                if (engine != null)
                    engine.SetGlobalValue("reader", table);

                string strLineCssClass = "content";
#if NO
                if (report.OutputLine != null)
                {
                    OutputLineEventArgs e = new OutputLineEventArgs();
                    e.Line = line;
                    e.Index = i;
                    e.LineCssClass = strLineCssClass;
                    report.OutputLine(this, e);
                    if (e.Output == false)
                        continue;

                    strLineCssClass = e.LineCssClass;
                }
#endif

                // strResult.Append("<tr class='" + strLineCssClass + "'>\r\n");
                writer.WriteStartElement("tr");
                writer.WriteAttributeString("class", strLineCssClass);

                // 列循环
                for (j = 0; j < report.Count; j++)
                {
                    PrintColumn column = (PrintColumn)report[j];

                    if (column.ColumnNumber < -1)
                    {
                        throw (new Exception("PrintColumn对象ColumnNumber列尚未初始化，位置" + Convert.ToString(j)));
                    }

                    string strText = "";
                    if (column.ColumnNumber != -1)
                    {
                        if (string.IsNullOrEmpty(column.Eval) == false)
                        {
                            // engine.SetGlobalValue("cell", line.GetObject(column.ColumnNumber));
                            strText = engine.Evaluate(column.Eval).ToString();
                        }
                        else if (column.DataType == DataType.PriceDouble)
                        {
                            if (table.IsDBNull(column.ColumnNumber /**/) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                double v = table.GetDouble(column.ColumnNumber);
                                /*
                                NumberFormatInfo provider = new NumberFormatInfo();
                                provider.NumberDecimalDigits = 2;
                                provider.NumberGroupSeparator = ".";
                                provider.NumberGroupSizes = new int[] { 3 };
                                strText = Convert.ToString(v, provider);
                                 * */
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (table.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = table.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.PriceDecimal)
                        {
                            if (table.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;
                            else
                            {
                                decimal v = table.GetDecimal(column.ColumnNumber);
                                strText = v.ToString("N", nfi);
                            }
                        }
                        else if (column.DataType == DataType.Price)
                        {
                            // Debug.Assert(false, "");
                            if (table.IsDBNull(column.ColumnNumber) == true)
                                strText = column.DefaultValue;	// 2005/5/26
                            else
                                strText = table.GetString(column.ColumnNumber);    // 
                        }
                        else
                            strText = table.GetString(column.ColumnNumber/*, column.DefaultValue*/);
                    }
                    else
                    {
                        strText = table.GetString(0);   // line.Entry;
                    }

                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        writer.WriteAttributeString("class", column.CssClass);
                    writer.WriteString(strText);
                    writer.WriteEndElement();   // </td>

                    if (report.SumLine == true
                        && column.Sum == true
                        && column.ColumnNumber != -1)
                    {
                        try
                        {
                            // if (column.DataType != DataType.Currency)
                            {
                                object v = table.GetValue(column.ColumnNumber);
#if NO
                                if (report.SumCell != null)
                                {
                                    SumCellEventArgs e = new SumCellEventArgs();
                                    e.DataType = column.DataType;
                                    e.ColumnNumber = column.ColumnNumber;
                                    e.LineIndex = i;
                                    e.Line = line;
                                    e.Value = v;
                                    report.SumCell(this, e);
                                    if (e.Value == null)
                                        continue;

                                    v = e.Value;
                                }
#endif

                                if (sums[j] == null)
                                    sums[j] = v;
                                else
                                {
                                    sums[j] = AddValue(column.DataType,
            sums[j],
            v);
                                    // sums[j] = ((decimal)sums[j]) + v;
                                }
                            }
                        }
                        catch (Exception ex)	// 俘获可能因字符串转换为整数抛出的异常
                        {
                            throw new Exception("在累加 行 " + i.ToString() + " 列 " + column.ColumnNumber.ToString() + " 值的时候，抛出异常: " + ex.Message);
                        }
                    }
                }

                // strResult.Append("</tr>\r\n");
                writer.WriteEndElement();   // </tr>
            }

            writer.WriteEndElement();   // </tbody>

            if (report.SumLine == true)
            {
                Line sum_line = null;
                if (engine != null)
                {
                    // 准备 Line 对象
                    sum_line = new Line(0);
                    for (j = 1; j < report.Count; j++)
                    {
                        PrintColumn column = (PrintColumn)report[j];
                        if (column.Sum == true
                            && sums[j] != null)
                        {
                            sum_line.SetValue(j - 1, sums[j]);
                        }
                    }
                    engine.SetGlobalValue("line", sum_line);
                }

                // strResult.Append("<tr class='sum'>\r\n");
                writer.WriteStartElement("tfoot");
                writer.WriteStartElement("tr");
                writer.WriteAttributeString("class", "sum");

                for (j = 0; j < report.Count; j++)
                {
                    PrintColumn column = (PrintColumn)report[j];
                    string strText = "";

                    if (j == 0)
                        strText = "合计";
                    else if (string.IsNullOrEmpty(column.Eval) == false)
                    {
                        strText = engine.Evaluate(column.Eval).ToString();
                    }
                    else if (column.Sum == true
                        && sums[j] != null)
                    {
                        if (column.DataType == DataType.PriceDouble)
                            strText = ((double)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.PriceDecimal)
                            strText = ((decimal)sums[j]).ToString("N", nfi);
                        else if (column.DataType == DataType.Price)
                        {
                            strText = StatisUtil.Int64ToPrice((Int64)sums[j]);
                        }
                        else
                            strText = Convert.ToString(sums[j]);

                        if (column.DataType == DataType.Currency)
                        {
                            string strSomPrice = "";
                            string strError = "";
                            // 汇总价格
                            int nRet = PriceUtil.SumPrices(strText,
            out strSomPrice,
            out strError);
                            if (nRet == -1)
                                strText = strError;
                            else
                                strText = strSomPrice;
                        }
                    }
                    else
                        strText = column.DefaultValue;  //  "&nbsp;";

#if NO
                    doc.WriteExcelCell(
    _lineIndex,
    j,
    strText,
    true);
#endif
                    writer.WriteStartElement(j == 0 ? "th" : "td");
                    if (string.IsNullOrEmpty(column.CssClass) == false)
                        writer.WriteAttributeString("class", column.CssClass);
                    writer.WriteString(strText);
                    writer.WriteEndElement();   // </td>
                }

                // strResult.Append("</tr>\r\n");
                writer.WriteEndElement();   // </tr>
                writer.WriteEndElement();   // </tfoot>
            }

            writer.WriteEndElement();   // </table>
        }

        object AddValue(DataType datatype,
    object o1,
    object o2)
        {
            if (o1 == null && o2 == null)
                return null;
            if (o1 == null)
                return o2;
            if (o2 == null)
                return o1;
            if (datatype == DataType.Auto)
            {
                if (o1 is Int64)
                    return (Int64)o1 + (Int64)o2;
                if (o1 is Int32)
                    return (Int32)o1 + (Int32)o2;
                if (o1 is double)
                    return (double)o1 + (double)o2;
                if (o1 is decimal)
                    return (decimal)o1 + (decimal)o2;
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 Auto 类型累加");
            }
            if (datatype == DataType.Number)
            {
                if (o1 is Int64)
                    return (Int64)o1 + (Int64)o2;
                if (o1 is Int32)
                    return (Int32)o1 + (Int32)o2;
                if (o1 is double)
                    return (double)o1 + (double)o2;
                if (o1 is decimal)
                    return (decimal)o1 + (decimal)o2;

                throw new Exception("无法支持的 Number 类型累加");
            }
            if (datatype == DataType.String)
            {
                if (o1 is string)
                    return (string)o1 + (string)o2;

                throw new Exception("无法支持的 String 类型累加");
            }
            if (datatype == DataType.Price) // 100倍金额整数
            {
                return (Int64)o1 + (Int64)o2;
            }
            if (datatype == DataType.PriceDouble)  // double，用来表示金额。也就是最多只有两位小数部分 -- 注意，有累计误差问题，以后建议废止
            {
                return (double)o1 + (double)o2;
            }
            if (datatype == DataType.PriceDecimal) // decimal，用来表示金额。
            {
                return (decimal)o1 + (decimal)o2;
            }
            if (datatype == DataType.Currency)
            {
                // 这一举容易发现列 数据类型 的错误
                return PriceUtil.JoinPriceString((string)o1,
                    (string)o2);
#if NO
                // 这一句更健壮一些
                return PriceUtil.JoinPriceString(Convert.ToString(o1),
                    Convert.ToString(o2));
#endif
            }
            throw new Exception("无法支持的 " + datatype.ToString() + " 类型累加");
        }

#endif

#if NO
        // 输出 RML 报表文件
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int OutputRmlReport(
            SQLiteDataReader table,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return -1;
            }

            XmlDocument columns_dom = null;

            List<bool> sums = new List<bool>();
            string strColumDefString = "";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("columns/column");

            if (nodes.Count > 0)
            {
                columns_dom = new XmlDocument();
                columns_dom.AppendChild(columns_dom.CreateElement("columns"));
            }

            List<string> evals = new List<string>();
            {
                int i = 0;
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strAlign = DomUtil.GetAttr(node, "align");
                    string strSum = DomUtil.GetAttr(node, "sum");
                    string strClass = DomUtil.GetAttr(node, "class");
                    string strEval = DomUtil.GetAttr(node, "eval");

                    evals.Add(strEval);

                    if (string.IsNullOrEmpty(strClass) == true)
                        strClass = "c" + (i + 1).ToString();

                    bool bSum = StringUtil.GetBooleanValue(strSum, false);
                    sums.Add(bSum);

                    if (string.IsNullOrEmpty(strColumDefString) == false)
                        strColumDefString += ",";
                    strColumDefString += strName + "||" + strClass;

                    XmlElement column = columns_dom.CreateElement("column");
                    columns_dom.DocumentElement.AppendChild(column);
                    column.SetAttribute("class", strClass);
                    column.SetAttribute("align", strAlign);
                    i++;
                }
            }

            string strTitle = DomUtil.GetElementText(dom.DocumentElement,
    "title").Replace("\\r", "\r");
            strTitle = Global.MacroString(macro_table, strTitle);

            string strComment = DomUtil.GetElementText(dom.DocumentElement,
"titleComment").Replace("\\r", "\r");
            strComment = Global.MacroString(macro_table, strComment);

#if NO
            Report report = Report.BuildReport(table,
                strColumDefString,  // "部门||department,借书(册)||borrowitem",
                "",
                true,
                false); // 不包括内容中多余的列
#endif
            Report report = BuildReport(table,
    strColumDefString,  // "部门||department,借书(册)||borrowitem",
    "",
    true,
    false); // 不包括内容中多余的列

            if (report == null)
                return 0;

            {
                int i = 0;
                foreach (PrintColumn column in report)
                {
                    if (i >= sums.Count)
                        break;  // 因为 Columns 因为 hint 的缘故，可能会比这里定义的要多

                    column.Eval = evals[i];
                    column.Sum = sums[i];
                    i++;
                }
            }

            string strCreateTime = DateTime.Now.ToString();

            string strCssContent = DomUtil.GetElementText(dom.DocumentElement, "css").Replace("\\r", "\r\n").Replace("\\t", "\t");

            // 写入输出文件
            if (string.IsNullOrEmpty(strOutputFileName) == true)
            {
                Debug.Assert(false, "");
                strOutputFileName = this.NewOutputFileName();
            }
            else
            {
                // 确保目录被创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
            }

            using (XmlTextWriter writer = new XmlTextWriter(strOutputFileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();
                writer.WriteStartElement("report");
                writer.WriteAttributeString("version", "0.01");

                writer.WriteStartElement("title");
                WriteTitles(writer, strTitle);
                writer.WriteEndElement();

                writer.WriteStartElement("comment");
                WriteTitles(writer, strComment);
                writer.WriteEndElement();

                writer.WriteStartElement("createTime");
                writer.WriteString(strCreateTime);
                writer.WriteEndElement();

                if (string.IsNullOrEmpty(strCssContent) == false)
                {
                    writer.WriteStartElement("style");
                    writer.WriteCData("\r\n" + strCssContent + "\r\n");
                    writer.WriteEndElement();
                }

                // XmlNode node = dom.DocumentElement.SelectSingleNode("columns");
                if (columns_dom != null && columns_dom.DocumentElement != null)
                    columns_dom.DocumentElement.WriteTo(writer);

                // 写入输出文件
                if (string.IsNullOrEmpty(strOutputFileName) == true)
                    strOutputFileName = this.NewOutputFileName();
                else
                {
                    // 确保目录被创建
                    PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
                }

                OutputRmlTable(
                    report,
                    table,
                    writer);

                writer.WriteEndElement();   // </report>
                writer.WriteEndDocument();
            }

            File.SetAttributes(strOutputFileName, FileAttributes.Archive);

#if NO
            string strHtmlFileName = Path.Combine(Path.GetDirectoryName(strOutputFileName), Path.GetFileNameWithoutExtension(strOutputFileName) + ".html");
            int nRet = Report.RmlToHtml(strOutputFileName,
                strHtmlFileName,
                out strError);
            if (nRet == -1)
                return -1;
#endif

            return 1;
        }

        // 输出 RML 报表文件
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int OutputRmlReport(
            Table table,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return -1;
            }

            XmlDocument columns_dom = null;

            List<bool> sums = new List<bool>();
            string strColumDefString = "";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("columns/column");

            if (nodes.Count > 0)
            {
                columns_dom = new XmlDocument();
                columns_dom.AppendChild(columns_dom.CreateElement("columns"));
            }

            List<string> evals = new List<string>();
            {
                int i = 0;
                foreach (XmlNode node in nodes)
                {
                    string strName = DomUtil.GetAttr(node, "name");
                    string strAlign = DomUtil.GetAttr(node, "align");
                    string strSum = DomUtil.GetAttr(node, "sum");
                    string strClass = DomUtil.GetAttr(node, "class");
                    string strEval = DomUtil.GetAttr(node, "eval");

                    evals.Add(strEval);

                    if (string.IsNullOrEmpty(strClass) == true)
                        strClass = "c" + (i + 1).ToString();

                    bool bSum = StringUtil.GetBooleanValue(strSum, false);
                    sums.Add(bSum);

                    if (string.IsNullOrEmpty(strColumDefString) == false)
                        strColumDefString += ",";
                    strColumDefString += strName + "||" + strClass;

                    XmlElement column = columns_dom.CreateElement("column");
                    columns_dom.DocumentElement.AppendChild(column);
                    column.SetAttribute("class", strClass);
                    column.SetAttribute("align", strAlign);
                    i++;
                }
            }

            string strTitle = DomUtil.GetElementText(dom.DocumentElement,
    "title").Replace("\\r", "\r");
            strTitle = Global.MacroString(macro_table, strTitle);

            string strComment = DomUtil.GetElementText(dom.DocumentElement,
"titleComment").Replace("\\r", "\r");
            strComment = Global.MacroString(macro_table, strComment);

            Report report = Report.BuildReport(table,
                strColumDefString,  // "部门||department,借书(册)||borrowitem",
                "",
                true,
                false); // 不包括内容中多余的列
            if (report == null)
                return 0;

            {
                int i = 0;
                foreach (PrintColumn column in report)
                {
                    if (i >= sums.Count)
                        break;  // 因为 Columns 因为 hint 的缘故，可能会比这里定义的要多

                    column.Eval = evals[i];
                    column.Sum = sums[i];
                    i++;
                }
            }

            string strCreateTime = DateTime.Now.ToString();

            string strCssContent = DomUtil.GetElementText(dom.DocumentElement, "css").Replace("\\r", "\r\n").Replace("\\t", "\t");

            // 写入输出文件
            if (string.IsNullOrEmpty(strOutputFileName) == true)
            {
                Debug.Assert(false, "");
                strOutputFileName = this.NewOutputFileName();
            }
            else
            {
                // 确保目录被创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
            }

            using (XmlTextWriter writer = new XmlTextWriter(strOutputFileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;

                writer.WriteStartDocument();
                writer.WriteStartElement("report");
                writer.WriteAttributeString("version", "0.01");

                writer.WriteStartElement("title");
                WriteTitles(writer, strTitle);
                writer.WriteEndElement();

                writer.WriteStartElement("comment");
                WriteTitles(writer, strComment);
                writer.WriteEndElement();

                writer.WriteStartElement("createTime");
                writer.WriteString(strCreateTime);
                writer.WriteEndElement();

                if (string.IsNullOrEmpty(strCssContent) == false)
                {
                    writer.WriteStartElement("style");
                    writer.WriteCData("\r\n" + strCssContent + "\r\n");
                    writer.WriteEndElement();
                }

                // XmlNode node = dom.DocumentElement.SelectSingleNode("columns");
                if (columns_dom != null && columns_dom.DocumentElement != null)
                    columns_dom.DocumentElement.WriteTo(writer);

                // 写入输出文件
                if (string.IsNullOrEmpty(strOutputFileName) == true)
                    strOutputFileName = this.NewOutputFileName();
                else
                {
                    // 确保目录被创建
                    PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
                }

                report.OutputRmlTable(table,
                    writer);

                writer.WriteEndElement();   // </report>
                writer.WriteEndDocument();
            }

            File.SetAttributes(strOutputFileName, FileAttributes.Archive);

#if NO
            string strHtmlFileName = Path.Combine(Path.GetDirectoryName(strOutputFileName), Path.GetFileNameWithoutExtension(strOutputFileName) + ".html");
            int nRet = Report.RmlToHtml(strOutputFileName,
                strHtmlFileName,
                out strError);
            if (nRet == -1)
                return -1;
#endif

            return 1;
        }

#endif

#if NO
        // TODO writer
        // 输出 HTML 报表文件
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int OutputHtmlReport1(
            Table table,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return -1;
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return -1;
            }

            List<bool> sums = new List<bool>();
            string strColumDefString = "";
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("columns/column");
            foreach (XmlNode node in nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                string strAlign = DomUtil.GetAttr(node, "align");
                string strSum = DomUtil.GetAttr(node, "sum");
                string strClass = DomUtil.GetAttr(node, "class");

                bool bSum = StringUtil.GetBooleanValue(strSum, false);
                sums.Add(bSum);

                if (string.IsNullOrEmpty(strColumDefString) == false)
                    strColumDefString += ",";
                strColumDefString += strName + "||" +strClass;
            }

            string strTitle = DomUtil.GetElementText(dom.DocumentElement,
    "title").Replace("\\r", "<br/>");
            string strTitleComment = DomUtil.GetElementText(dom.DocumentElement,
                "titleComment").Replace("\\r", "<br/>");

            strTitle = Global.MacroString(macro_table, strTitle);
            strTitleComment = Global.MacroString(macro_table, strTitleComment);

            Report report = Report.BuildReport(table,
                strColumDefString,  // "部门||department,借书(册)||borrowitem",
                "&nbsp;",
                true);
            if (report == null)
                return 0;

            int i = 0;
            foreach (PrintColumn column in report)
            {
                if (i >= sums.Count)
                    break;  // 因为 Columns 因为 hint 的缘故，可能会比这里定义的要多

                column.Sum = sums[i];
                i++;
            }

            string strCreateTime = HttpUtility.HtmlEncode("报表创建时间 " + DateTime.Now.ToString());

            // string strCssFileName = "";
            string strCssContent = DomUtil.GetElementText(dom.DocumentElement, "css").Replace("\\r", "\r\n").Replace("\\t", "\t");

            string strHead = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\"><head>"
                + "<meta http-equiv='Content-Type' content=\"text/html; charset=utf-8\">"
                + "<title>"+strTitle.Replace("<br/>", " ")+"</title>"
                // + "<link rel='stylesheet' href='"+strCssFileName+"' type='text/css'>"
                + "<style media='screen' type='text/css'>"
                + strCssContent
                + "</style>"
                + "</head><body>"
                + "<div class='tabletitle'>" + strTitle + "</div>"
                + "<div class='titlecomment'>" + strTitleComment + "</div>";
            string strTail = "<div class='createtime'>"+strCreateTime+"</div></body></html>";

            string strHtml = strHead + report.HtmlTable(table) + strTail;

            // 写入输出文件
            if (string.IsNullOrEmpty(strOutputFileName) == true)
                strOutputFileName = this.NewOutputFileName();
            else
            {
                // 确保目录被创建
                PathUtil.CreateDirIfNeed(Path.GetDirectoryName(strOutputFileName));
            }

            this.WriteToOutputFile(strOutputFileName,
                strHtml,
                Encoding.UTF8);
            // File.SetAttributes(strOutputFileName, FileAttributes.Archive);

            return 1;
        }

#endif




#if NO
        // 从报表配置文件中获得 <columnSortStyle> 元素文本值
        static string GetColumnSortStyle(string strCfgFile)
        {
            string strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strCfgFile);
            }
            catch (FileNotFoundException)
            {
                strError = "配置文件 '" + strCfgFile + "' 没有找到";
                return "";
            }
            catch (Exception ex)
            {
                strError = "报表配置文件 " + strCfgFile + " 打开错误: " + ex.Message;
                return "";
            }

            return DomUtil.GetElementText(dom.DocumentElement,
                "columnSortStyle");
        }
#endif

        // 按照指定的单位名称列表，列出借书册数
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_102_report(string strLibraryCode,
            string strDateRange,
            string strCfgFile,
            // string strTitle,    // 例如： 各年级
            Hashtable macro_table,
            string strNameTable,
            string strOutputFileName,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<string> departments = StringUtil.SplitList(strNameTable);
            if (departments.Count == 0)
                return 0;

            Table tableDepartment = new Table(3);

#if NO
            foreach (string department in departments)
            {
                string strCommand = "";
                nRet = CreateReaderReportCommand(
                    strLibraryCode,
                    strDateRange,
                    "102",
                    department,
                    out strCommand,
                    out strError);
                if (nRet == -1)
                    return -1;
                nRet = RunQuery(
                    strCommand,
                    ref tableDepartment,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#endif
            List<string> commands = new List<string>();
            foreach (string department in departments)
            {
                string strCommand = "";
                nRet = CreateReaderReportCommand(
                    strLibraryCode,
                    strDateRange,
                    "102",
                    department,
                    out strCommand,
                    out strError);
                if (nRet == -1)
                    return -1;
                commands.Add(strCommand);
            }

            nRet = RunQuery(
    commands,
    ref tableDepartment,
    out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;
            // macro_table["%daterange%"] = strDateRange;

            ReportWriter writer = null;
            nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            string strColumnSortStyle = writer.GetColumnSortStyle();
            if (string.IsNullOrEmpty(strColumnSortStyle) == true)
                strColumnSortStyle = "0:d,-1:a";    // 先按照册数从大到小；然后按照名称从小到大
            else
                strColumnSortStyle = SortColumnCollection.NormalToTable(strColumnSortStyle);

            tableDepartment.Sort(strColumnSortStyle);

            // 观察表格中是否全部行为 0
            for(int i = 0; i < tableDepartment.Count; i++)
            {
                Line line = tableDepartment[i];
                if (line.GetInt64(0) > 0)
                    goto FOUND;
            }

            return 0;
        FOUND:
            macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;

            TableReader reader = new TableReader(tableDepartment);

            return writer.OutputRmlReport(
                reader,
                macro_table,
                strOutputFileName,
                out strError);
#if NO
            return OutputRmlReport(
                tableDepartment,
                strCfgFile,
                macro_table,
                strOutputFileName,
                out strError);
#endif

        }

        ObjectCache<ReportWriter> _writerCache = new ObjectCache<ReportWriter>();

        int GetReportWriter(string strCfgFile,
            out ReportWriter writer,
            out string strError)
        {
            strError = "";

            writer = this._writerCache.FindObject(strCfgFile);
            if (writer == null)
            {
                writer = new ReportWriter();
                int nRet = writer.Initial(strCfgFile, out strError);
                if (nRet == -1)
                    return -1;
                this._writerCache.SetObject(strCfgFile, writer);
            }

            return 0;
        }



        // 这是创建到一个子目录(会在子目录中创建很多文件和下级目录)，而不是输出到一个文件
        // return:
        //      -1  出错
        //      0   没有创建目录
        //      1   创建了目录
        int Create_131_report(string strLibraryCode,
            string strDateRange,
            string strCfgFile,
            // string strTitle,    // 例如： 各年级
            Hashtable macro_table,
            // string strNameTable,
            string strOutputDir,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // macro_table["%library%"] = strLibraryCode;

            Table reader_table = null;

            // 获得一个分馆内读者记录的证条码号和单位名称
            nRet = GetAllReaderDepartments(
                    strLibraryCode,
                    ref reader_table,
                    out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strOutputDir) == false)
            {
                // 不能使用根目录
                string strRoot = Directory.GetDirectoryRoot(strOutputDir);
                if (PathUtil.IsEqual(strRoot, strOutputDir) == true)
                {
                }
                else
                    PathUtil.DeleteDirectory(strOutputDir);
            }

            if (reader_table.Count == 0)
            {
                // 下级全部目录此时已经删除
                return 0;
            }

            ReportWriter writer = null;
            nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

#if NO
            string strColumnSortStyle = writer.GetColumnSortStyle();
            if (string.IsNullOrEmpty(strColumnSortStyle) == true)
                strColumnSortStyle = "1:a";
            else
                strColumnSortStyle = SortColumnCollection.NormalToTable(strColumnSortStyle);
#endif

            reader_table.Sort("1:a,-1:a");    // 

            int nWriteCount = 0;    // 创建了多少个具体的报表

            // 所有部门目录已经删除，然后再开始创建

            // stop.SetProgressRange(0, reader_table.Count);
            for (int i = 0; i < reader_table.Count; i++)
            {
                Application.DoEvents();
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断...";
                    return -1;
                }

                Line line = reader_table[i];
                string strReaderBarcode = line.Entry;
                string strName = line.GetString(0);
                string strDepartment = line.GetString(1);

                string strDepartmentName = strDepartment.Replace(" ", "_");
                if (string.IsNullOrEmpty(strDepartmentName) == true)
                    strDepartmentName = "其他部门";

                string strPureFileName = GetValidPathString(strDepartmentName) + "\\" + GetValidPathString(strReaderBarcode + "_" + strName) + ".rml";
                string strOutputFileName = "";

                try
                {
                    strOutputFileName = Path.Combine(strOutputDir,
                        // strLibraryCode + "\\" + 
                         strPureFileName);    // xlsx
                }
                catch (System.ArgumentException ex)
                {
                    strError = "文件名字符串 '"+strPureFileName+"' 中有非法字符。" + ex.Message;
                    return -1;
                }

                stop.SetMessage("正在创建报表文件 " + strOutputFileName + " " + (i + 1).ToString() + "/" + reader_table.Count.ToString() + " ...");

#if NO
                Table tableList = null;
                nRet = CreateReaderReport(
                    strLibraryCode,
                    strDateRange,
                    "131",
                    strReaderBarcode,
                    ref tableList,
                    out strError);
                if (nRet == -1)
                    return -1;

                tableList.Sort(strColumnSortStyle);  // "1:a" 按照借书时间排序
#endif
                macro_table["%name%"] = strName;
                macro_table["%department%"] = strDepartment;
                macro_table["%readerbarcode%"] = strReaderBarcode;

                // macro_table["%linecount%"] = tableList.Count.ToString();
                macro_table["%daterange%"] = strDateRange;

                List<string> commands = new List<string>();

                    string strCommand = "";
                    nRet = CreateReaderReportCommand(
                        strLibraryCode,
                        strDateRange,
                        "131",
                        strReaderBarcode,
                        out strCommand,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    commands.Add(strCommand);

                nRet = RunQuery(
                    commands,
    writer,
    strOutputFileName,
    macro_table,
    "创建 131 表时",
    out strError);
                if (nRet == -1)
                    return -1;
#if NO
                nRet = CreateReaderReport(
    strLibraryCode,
    strDateRange,
    "131",
    strReaderBarcode,
    writer,
    strOutputFileName,
    macro_table,
    out strError);
                if (nRet == -1)
                    return -1;
#endif

                // TODO: 没有数据的读者，是否在 index.xml 也创建一个条目?
                if (nRet == 1)
                {
                    // 将一个统计文件条目写入到 131 子目录中的 index.xml 的 DOM 中
                    // parameters:
                    //      strOutputDir    index.xml 所在目录
                    nRet = Write_131_IndexXml(
                        strDepartmentName,
                        strName,
                        strReaderBarcode,
                        strOutputDir,
                        strOutputFileName,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    nWriteCount++;
                }
            }

            if (nWriteCount > 0
                && (this._fileType & FileType.HTML) != 0)
            {
                string strIndexXmlFileName = Path.Combine(strOutputDir, "index.xml");
                string strIndexHtmlFileName = Path.Combine(strOutputDir, "index.html");

                if (stop != null)
                    stop.SetMessage("正在创建 " + strIndexHtmlFileName);

                // 根据 index.xml 文件创建 index.html 文件
                nRet = CreateIndexHtmlFile(strIndexXmlFileName,
                    strIndexHtmlFileName,
                    out strError);
                if (nRet == -1)
                    return -1;

            }

            if (nWriteCount > 0)
                return 1;
            return 0;
        }

        // 附加的一些文件名非法字符。比如 XP 下 Path.GetInvalidPathChars() 不知何故会遗漏 '*'
        static string spec_invalid_chars = "*?:";

        public static string GetValidPathString(string strText, string strReplaceChar = "_")
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            // string invalid_chars = new string(Path.GetInvalidPathChars());
            // invalid_chars += "\t";

            // 2014/9/19 修改 BUG
            char [] invalid_chars = Path.GetInvalidPathChars();
            StringBuilder result = new StringBuilder();
            foreach (char c in strText)
            {
                if (c == ' ')
                    continue;
                if (IndexOf(invalid_chars, c) != -1
                    || spec_invalid_chars.IndexOf(c) != -1)
                    result.Append(strReplaceChar);
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        static int IndexOf(char[] chars, char c)
        {
            int i = 0;
            foreach (char c1 in chars)
            {
                if (c1 == c)
                    return i;
                i++;
            }

            return -1;
        }

        // 101 111 121 122
        // 121 表 按照读者 *姓名* 分类的借书册数表
        // 122 表 按照读者 *姓名* 没有借书的读者
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_1XX_report(string strLibraryCode,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            string strReportType,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateReaderReportCommand(
                strLibraryCode,
                strDateRange,
                strReportType,  // "121",
                "",
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
    "创建 "+strReportType+" 表时",
out strError);
        }



        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_201_report(string strLibraryCode,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateBookReportCommand(
                strLibraryCode,
                strDateRange,
                "201",
                "",
                null,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
    "创建 201 表时",
out strError);
        }

        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_202_report(string strLibraryCode,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateBookReportCommand(
                strLibraryCode,
                strDateRange,
                "202",
                "",
                null,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
    "创建 202 表时",
out strError);
        }

        // 212 213
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_212_report(
            string strLocation,
            string strClassType,
            string strClassCaption,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            List<string> filters,
            string strOutputFileName,
            string strReportType,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;
            macro_table["%class%"] = string.IsNullOrEmpty(strClassCaption) == false ? strClassCaption : strClassType;

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateBookReportCommand(
                strLocation,
                strDateRange,
                strReportType,  // "212",
                strClassType,
                filters,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
    "创建 "+strReportType+" 表时",
out strError);
        }

#if NO
        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_301_report(
            string strLocation,
            string strClassType,
            string strClassCaption,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            List<string> filters,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;
            macro_table["%class%"] = string.IsNullOrEmpty(strClassCaption) == false ? strClassCaption : strClassType;
            macro_table["%createtime%"] = DateTime.Now.ToLongDateString();

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = ""; 
            nRet = CreateStorageReportCommand(
                strLocation,
                strDateRange,
                "301",
                strClassType,
                filters,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
out strError);
        }

#endif

        // 获得存量需要的时间字符串
        // 20120101-20140101 --> 00010101-20111231
        static int BuildPrevDateString(string strDateRange,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);
                // 2014/4/11
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            DateTime start = DateTimeUtil.Long8ToDateTime(strStartDate);
            start -= new TimeSpan(1, 0, 0, 0);  // 前一天
            strResult = DateTimeUtil.DateTimeToString8(new DateTime(0)) + "-" +DateTimeUtil.DateTimeToString8(start);

            return 0;
        }

        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_301_report(
            string strLocation,
            string strClassType,
            string strClassCaption,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            List<string> filters,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;
            macro_table["%class%"] = string.IsNullOrEmpty(strClassCaption) == false ? strClassCaption : strClassType;
            macro_table["%createtime%"] = DateTime.Now.ToLongDateString();

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            // 存量
            string strResult = "";
                    // 获得存量需要的时间字符串
        // 20120101-20140101 --> 00010101-20111231
            nRet = BuildPrevDateString(strDateRange,
                out strResult,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateStorageReportCommand(
                strLocation,
                strResult,
                "301",
                strClassType,
                filters,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            List<object[]> amount_table = new List<object[]>();

            nRet = RunQuery(
commands,
ref amount_table,
out strError);
            if (nRet == -1)
                return -1;

            // 增量
            commands.Clear();

            strCommand = "";
            nRet = CreateStorageReportCommand(
                strLocation,
                strDateRange,
                "301",
                strClassType,
                filters,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            List<object[]> delta_table = new List<object[]>();

            nRet = RunQuery(
commands,
ref delta_table,
out strError);
            if (nRet == -1)
                return -1;

            if (delta_table.Count == 0 && amount_table.Count == 0)
                return 0;

            Table result_table = Merge301Table(amount_table, delta_table);
            result_table.Sort("-1:a");

            macro_table["%linecount%"] = result_table.Count.ToString();

            TableReader reader = new TableReader(result_table);

            return writer.OutputRmlReport(
                reader,
                macro_table,
                strOutputFileName,
                out strError);

        }

        static string GetString(object o)
        {
            if (o is System.DBNull)
                return "";
            return (string)o;
        }

        static Table Merge301Table(List<object[]> amount_table, List<object[]> delta_table)
        {
            Table result_table = new Table(0);
            foreach (object[] line in amount_table)
            {
                string strClass = GetString(line[0]);

                result_table.IncValue(strClass, 0, (Int64)line[1]);   // 2015/4/2 bug 0
                result_table.IncValue(strClass, 2, (Int64)line[2]);  // 2015/4/2 bug 0
            }
            foreach (object[] line in delta_table)
            {
                string strClass = GetString(line[0]);

                result_table.IncValue(strClass, 1, (Int64)line[1]);  // 2015/4/2 bug 0
                result_table.IncValue(strClass, 3, (Int64)line[2]);  // 2015/4/2 bug 0
            }
            return result_table;
        }

        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_302_report(
            string strLocation,
            string strClassType,
            string strClassCaption,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            List<string> filters,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;
            macro_table["%class%"] = string.IsNullOrEmpty(strClassCaption) == false ? strClassCaption : strClassType;
            macro_table["%createtime%"] = DateTime.Now.ToLongDateString();

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateStorageReportCommand(
                strLocation,
                strDateRange,
                "302",
                strClassType,
                filters,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
    "创建 302 表时",
out strError);
        }

        // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_4XX_report(string strLibraryCode,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            string strOutputFileName,
            string strType,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            // Table tableDepartment = null;
            nRet = CreateWorkerReportCommand(
                strLibraryCode,
                strDateRange,
                strType,
                "",
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            if (strType == "472")
            {
                // Table table = new Table(0);
                List<object[]> table = new List<object[]>();

                nRet = RunQuery(
commands,
ref table,
out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 0;

                // table.Sort("-1:a");

                Table result_table = MergeCurrency(table);
                result_table.Sort("-1:a");

                macro_table["%linecount%"] = table.Count.ToString();

                TableReader reader = new TableReader(result_table);

                return writer.OutputRmlReport(
                    reader,
                    macro_table,
                    strOutputFileName,
                    out strError);

            }

            return RunQuery(
    commands,
writer,
strOutputFileName,
macro_table,
    "创建 "+strType+" 表时",
out strError);

            
#if NO
            string strColumnSortStyle = GetColumnSortStyle(strCfgFile);
            if (string.IsNullOrEmpty(strColumnSortStyle) == true)
            {
                if (strType == "421")
                    strColumnSortStyle = "4:a";    // "4:a" 操作时间
                else if (strType == "422")
                    strColumnSortStyle = "0:a";    // "0:a" 操作者
                else if (strType == "431")
                    strColumnSortStyle = "5:a";    // "5:a" 操作时间
                else if (strType == "432")
                    strColumnSortStyle = "0:a";    // "0:a" 操作者
                if (strType == "441")
                    strColumnSortStyle = "6:a";
                else if (strType == "442")
                    strColumnSortStyle = "0:a";
                else if (strType == "443")
                    strColumnSortStyle = "0:a";
            }

            tableDepartment.Sort(SortColumnCollection.NormalToTable(strColumnSortStyle));

            macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;

            return OutputRmlReport(
                tableDepartment,
                strCfgFile,
                macro_table,
                strOutputFileName,
                out strError);
#endif
        }

                // return:
        //      -1  出错
        //      0   没有创建文件(因为输出的表格为空)
        //      1   成功创建文件
        int Create_493_report(
            string strLibraryCode,
            string strClassType,
            string strClassCaption,
            string strDateRange,
            string strCfgFile,
            Hashtable macro_table,
            List<string> filters,
            string strOutputFileName,
            out string strError)
        {
            strError = "";

            // macro_table["%linecount%"] = tableDepartment.Count.ToString();
            macro_table["%daterange%"] = strDateRange;
            macro_table["%class%"] = string.IsNullOrEmpty(strClassCaption) == false ? strClassCaption : strClassType;
            macro_table["%createtime%"] = DateTime.Now.ToLongDateString();

            ReportWriter writer = null;
            int nRet = GetReportWriter(strCfgFile,
                out writer,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> commands = new List<string>();

            string strCommand = "";
            nRet = CreateClassReportCommand(
                strLibraryCode,
                strDateRange,
                "493",
                strClassType,
                filters,
                out strCommand,
                out strError);
            if (nRet == -1)
                return -1;
            commands.Add(strCommand);

            return RunQuery(
                commands,
                writer,
                strOutputFileName,
                macro_table,
                "创建 493 表时",
                out strError);
        }
#if NO
        // source : operator unit amerce_count amerce_money undo_count undo_money expire_count total_count
        // target : operator amerce_count amerce_money undo_count undo_money expire_count total_count
        static Table MergeCurrency(Table table)
        {
            Table result_table = new Table(0);
            // 合并各种货币单位
            // operator unit amerce_count amerce_money undo_count undo_money expire_count total_count
            for (int i = 0; i < table.Count; i++)
            {
                Line line = table[i];
                string strKey = line.Entry;
                string strUnit = line.GetString(0); // unit

                // amerce_count
                result_table.IncValue(strKey, 0, line.GetInt64(1), 0);
                // amerce_money
                IncPrice(line,
            strUnit,
            2,
            result_table,
            1);
                // undo_count
                result_table.IncValue(strKey, 2, line.GetInt64(3), 0);
                // undo_money
                IncPrice(line,
            strUnit,
            4,
            result_table,
            3);
                // expire_count
                result_table.IncValue(strKey, 4, line.GetInt64(5), 0);
                // total_count
                result_table.IncValue(strKey, 5, line.GetInt64(6), 0);
            }

            return result_table;
        }

#endif
        static int SOURCE_OPERATOR = 0;
        static int SOURCE_UNII = 1;
        static int SOURCE_AMERCE_COUNT = 2;
        static int SOURCE_AMERCE_MONEY = 3;
        static int SOURCE_MODIFY_COUNT = 4;
        static int SOURCE_MODIFY_MONEY = 5;
        static int SOURCE_UNDO_COUNT = 6;
        static int SOURCE_UNDO_MONEY = 7;
        static int SOURCE_EXPIRE_COUNT = 8;
        static int SOURCE_TOTAL_COUNT = 9;

        static int TARGET_AMERCE_COUNT = 0;
        static int TARGET_AMERCE_MONEY = 1;
        static int TARGET_MODIFY_COUNT = 2;
        static int TARGET_MODIFY_MONEY = 3;
        static int TARGET_UNDO_COUNT = 4;
        static int TARGET_UNDO_MONEY = 5;
        static int TARGET_EXPIRE_COUNT = 6;
        static int TARGET_TOTAL_COUNT = 7;


        // source : operator unit amerce_count amerce_money undo_count undo_money expire_count total_count
        // target : operator amerce_count amerce_money undo_count undo_money expire_count total_count
        static Table MergeCurrency(List<object []> table)
        {
            Table result_table = new Table(0);
            // 合并各种货币单位
            // operator unit amerce_count amerce_money undo_count undo_money expire_count total_count
            foreach (object [] line in table)
            {
                string strOperator = (string)line[SOURCE_OPERATOR];
                string strUnit = (string)line[SOURCE_UNII]; // unit

                // amerce_count
                result_table.IncValue(strOperator, TARGET_AMERCE_COUNT, (Int64)line[SOURCE_AMERCE_COUNT]);   // 2015/4/2 bug 0
                // amerce_money
                IncPrice(line,
            strUnit,
            SOURCE_AMERCE_MONEY,
            result_table,
            TARGET_AMERCE_MONEY);

                // modify_count
                result_table.IncValue(strOperator, TARGET_MODIFY_COUNT, (Int64)line[SOURCE_MODIFY_COUNT]);   // 2015/4/2 bug 0
                // modify_money
                IncPrice(line,
            strUnit,
            SOURCE_MODIFY_MONEY,
            result_table,
            TARGET_MODIFY_MONEY);


                // undo_count
                result_table.IncValue(strOperator, TARGET_UNDO_COUNT, (Int64)line[SOURCE_UNDO_COUNT]);   // 2015/4/2 bug 0
                // undo_money
                IncPrice(line,
            strUnit,
            SOURCE_UNDO_MONEY,
            result_table,
            TARGET_UNDO_MONEY);
                // expire_count
                result_table.IncValue(strOperator, TARGET_EXPIRE_COUNT, (Int64)line[SOURCE_EXPIRE_COUNT]);   // 2015/4/2 bug 0
                // total_count
                result_table.IncValue(strOperator, TARGET_TOTAL_COUNT, (Int64)line[SOURCE_TOTAL_COUNT]);   // 2015/4/2 bug 0
            }

            return result_table;
        }


        // 将货币单位和数字拼接为一个字符串
        // 注意负号应该在第一字符
        static string GetCurrencyString(string strUnit, decimal value)
        {
            if (value >= 0)
                return strUnit + value.ToString();
            return "-" + strUnit + (-value).ToString();
        }

        static void IncPrice(object [] line,
    string strUnit,
    int source_column_index,
    Table result_table,
    int result_column_index)
        {
            object o = line[source_column_index];
            if (o == null)
                return;
            if (o is System.DBNull)
                return;

            decimal value = Convert.ToDecimal(o);
            if (value == 0)
                return;

            if (string.IsNullOrEmpty(strUnit) == true)
                strUnit = "CNY";

            string strNewPrice = GetCurrencyString(strUnit, value);

            Line result_line = result_table[(string)line[0]];
            if (result_line != null)
            {
                string strExitingPrice = result_line.GetString(result_column_index);

                strNewPrice = PriceUtil.JoinPriceString(strExitingPrice, strNewPrice);
            }
            result_table.SetValue((string)line[0], result_column_index, strNewPrice);
        }
#if NO
        static void IncPrice(Line line, 
            string strUnit,
            int source_column_index,
            Table result_table,
            int result_column_index)
        {
            decimal value = line.GetDecimal(source_column_index);
            if (value == 0)
                return;

            if (string.IsNullOrEmpty(strUnit) == true)
                strUnit = "CNY";

            string strNewPrice = GetCurrencyString(strUnit, value);

            Line result_line = result_table[line.Entry];
            if (result_line != null)
            {
                string strExitingPrice = result_line.GetString(result_column_index);

                strNewPrice = PriceUtil.JoinPriceString(strExitingPrice, strNewPrice);
            }
            result_table.SetValue(line.Entry, result_column_index, strNewPrice);
        }
#endif

        int RunQuery(
List<string> commands,
ref List<object []> table,
out string strError)
        {
            strError = "";

            StringBuilder text = new StringBuilder();
            foreach (string command in commands)
            {
                if (string.IsNullOrEmpty(command) == true)
                {
                    strError = "command 不应为空";
                    return -1;
                } 
                text.Append(command);
                text.Append("\r\n\r\n");
            }

            this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(text.ToString(),
    connection))
                {

                    try
                    {
                        using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.Default))
                        {
                            // 如果记录不存在
                            if (dr == null
                                || dr.HasRows == false)
                                return 0;

                            for (; ; )
                            {
                                // 如果记录已经存在
                                while (dr.Read())
                                {
                                    // string strKey = GetString(dr, 0);

                                    object [] values = new object [dr.FieldCount];
                                    dr.GetValues(values);
                                    table.Add(values);
#if NO
                                    for (int i = 1; i < dr.FieldCount; i++)
                                    {
                                        
                                    }
#endif
                                }

                                if (dr.NextResult() == false)
                                    break;
                            }

                            return 1;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + text.ToString();
                        return -1;
                    }
                } // end of using command
            }
        }

        int RunQuery(
    List<string> commands,
ref Table table,
out string strError)
        {
            strError = "";

            StringBuilder text = new StringBuilder();
            foreach (string command in commands)
            {
                if (string.IsNullOrEmpty(command) == true)
                {
                    strError = "command 不应为空";
                    return -1;
                }
                text.Append(command);
                text.Append("\r\n\r\n");
            }

            this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(text.ToString(),
    connection))
                {
                    try
                    {
                        using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.Default))
                        {
                            // 如果记录不存在
                            if (dr == null
                                || dr.HasRows == false)
                                return 0;

                            for (; ; )
                            {
                                // 如果记录已经存在
                                while (dr.Read())
                                {
                                    string strKey = GetString(dr, 0);

                                    for (int i = 1; i < dr.FieldCount; i++)
                                    {
                                        if (dr.IsDBNull(i) == true)
                                        {
                                            table.SetValue(strKey, i - 1, null);
                                            continue;
                                        }
                                        Type type = dr.GetFieldType(i);
                                        if (type.Equals(typeof(string)) == true)
                                            table.SetValue(strKey, i - 1, GetString(dr, i));
                                        else if (type.Equals(typeof(double)) == true)
                                            table.SetValue(strKey, i - 1, GetDouble(dr, i));
                                        else if (type.Equals(typeof(Int64)) == true)
                                            table.SetValue(strKey, i - 1, dr.GetInt64(i));
                                        else if (type.Equals(typeof(object)) == true)
                                            table.SetValue(strKey, i - 1, dr.GetValue(i));
                                        else
                                            table.SetValue(strKey, i - 1, dr.GetInt32(i));
                                    }
                                }

                                if (dr.NextResult() == false)
                                    break;
                            }

                            return 1;
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + text.ToString();
                        return -1;
                    }
                } // end of using command
            }
        }

        int RunQuery(
            List<string> commands,
            ReportWriter writer,
            string strOutputFileName,
            Hashtable macro_table,
            string strErrorInfoTitle,
            out string strError)
        {
            strError = "";

            StringBuilder text = new StringBuilder();
            foreach (string command in commands)
            {
                if (string.IsNullOrEmpty(command) == true)
                {
                    strError = "command 不应为空";
                    return -1;
                } 
                text.Append(command);
                text.Append("\r\n\r\n");
            }

            this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(text.ToString(),
    connection))
                {
                    try
                    {
                        using (SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.Default))
                        {
                            // 如果记录不存在
                            if (dr == null
                                || dr.HasRows == false)
                                return 0;

                            return writer.OutputRmlReport(
                                dr,
                                macro_table,
                                strOutputFileName,
                                out strError);
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = strErrorInfoTitle + " 执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + text.ToString();
                        return -1;
                    }
                } // end of using command
            }
        }

        // 创建读者报表，关于流通业务
        // 1) 按照读者自然单位分类的借书册数表 101
        // 2) 按照指定的单位分类的借书册数表 102
        // 3) 按照读者类型分类的借书册数表 111
        // 4) 按照读者姓名分类的借书册数表 121
        // 5) 没有借书的读者 122
        // 6) 每个读者的借阅清单 131
        int CreateReaderReportCommand(
            string strLibraryCode,
            string strDateRange,
            string strStyle,
            string strParameters,
            out string strCommand,
            out string strError)
        {
            strError = "";
            strCommand = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);

                // 2014/3/19
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            if (StringUtil.IsInList("101", strStyle) == true)
            {
                // 101 表 按照读者 *自然单位* 分类的借书册数表
#if NO
                strCommand = "select reader.department, count(*) as count "
                     + " FROM operlogcircu left outer JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY reader.department ORDER BY count DESC, reader.department;";
#endif
                // 2015/6/17 增加了 return 列
                strCommand = "select reader.department, "
                    + " count(case operlogcircu.operation when 'borrow' then operlogcircu.action end) as borrow, "
                    + " count(case operlogcircu.operation when 'return' then operlogcircu.action end) as return "
                    + " FROM operlogcircu left outer JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY reader.department ORDER BY borrow DESC, reader.department;";
            }
            else if (StringUtil.IsInList("102", strStyle) == true)
            {
                // 102 表 按照 *指定的单位* 分类的借书册数表
                // 这里每次只能获得一个单位的一行数据。需要按照不同单位 (strParameters) 多次循环调用本函数
#if NO
                strCommand = "select '" + strParameters + "' as department, count(*) as count "
                     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' AND reader.department like '" + strParameters + "' "
                     + "  ORDER BY count DESC, department;";
#endif
                // 2015/6/17 增加了 return 列
                strCommand = "select '" + strParameters + "' as department, "
                    + " count(case operlogcircu.operation when 'borrow' then operlogcircu.action end) as borrow, "
                    + " count(case operlogcircu.operation when 'return' then operlogcircu.action end) as return "
                     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' AND reader.department like '" + strParameters + "' "
                     + "  ORDER BY borrow DESC, department;";

            }
            else if (StringUtil.IsInList("111", strStyle) == true)
            {
                // 111 表 按照读者 *自然类型* 分类的借书册数表
#if NO
                strCommand = "select reader.readertype, count(*) as count "
                     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY reader.readertype ORDER BY count DESC, reader.readertype;";
#endif
                // 2015/6/17 增加了 return 列
                strCommand = "select reader.readertype, "
                    + " count(case operlogcircu.operation when 'borrow' then operlogcircu.action end) as borrow, "
                    + " count(case operlogcircu.operation when 'return' then operlogcircu.action end) as return "
     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
     + " WHERE operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
     + "     AND reader.librarycode = '" + strLibraryCode + "' "
     + " GROUP BY reader.readertype ORDER BY borrow DESC, reader.readertype;";

            }
            else if (StringUtil.IsInList("121", strStyle) == true)
            {
                // 121 表 按照读者 *姓名* 分类的借书册数表
#if NO
                strCommand = "select operlogcircu.readerbarcode, reader.name, reader.department, count(*) as count "
                     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY operlogcircu.readerbarcode ORDER BY count DESC, reader.department, operlogcircu.readerbarcode ;";
#endif
                // 2015/6/17 增加了 return 列
                strCommand = "select operlogcircu.readerbarcode, reader.name, reader.department, "
                    + " count(case operlogcircu.operation when 'borrow' then operlogcircu.action end) as borrow, "
                    + " count(case operlogcircu.operation when 'return' then operlogcircu.action end) as return "
     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
     + " WHERE operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
     + "     AND reader.librarycode = '" + strLibraryCode + "' "
     + " GROUP BY operlogcircu.readerbarcode ORDER BY borrow DESC, reader.department, operlogcircu.readerbarcode ;";

            }
            else if (StringUtil.IsInList("122", strStyle) == true)
            {
                /*
create temp table tt as select operlogcircu.readerbarcode as tt FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode  WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow'      AND operlogcircu.date >= '20120101' AND operlogcircu.date <= '20121201'      AND reader.librarycode = '合肥望湖小学' ;

select readerbarcode, name, department from reader  WHERE librarycode = '合肥望湖小学'  AND readerbarcode not in  tt  AND (select count(*) from tt) > 0  ORDER BY department, readerbarcode ;
                 * */
                // 122 表 按照读者 *姓名* 没有借书的读者
                strCommand =
                     "create temp table tt as select operlogcircu.readerbarcode "
                     + " FROM operlogcircu JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "';"
                     + " select readerbarcode, name, department from reader "
                     + " WHERE (select count(*) from tt) > 0 "
                     + " AND librarycode = '" + strLibraryCode + "' "
                     + " AND readerbarcode not in tt "
                     + " AND state = '' "   // 状态值为空的读者才能参与此项统计
                     + " ORDER BY department, readerbarcode ;";
                // nNumber = 122;
            }
            else if (StringUtil.IsInList("131", strStyle) == true)
            {
                // 131 表 每个读者的借阅清单
                strCommand = "select oper1.itembarcode, biblio.summary, oper1.opertime as 'borrowtime', oper2.opertime as 'returntime' from operlogcircu as oper1 "
                        + " left join operlogcircu as oper2 on oper2.itembarcode <> '' AND oper1.itembarcode = oper2.itembarcode and oper2.readerbarcode <> '' AND oper1.readerbarcode = oper2.readerbarcode and oper2.operation = 'return' and oper1.opertime <= oper2.opertime  "
                        + " left JOIN item ON oper1.itembarcode <> '' AND oper1.itembarcode = item.itembarcode "
                        + " left JOIN biblio ON item.bibliorecpath <> '' AND biblio.bibliorecpath = item.bibliorecpath "
                        + " where oper1.operation = 'borrow' and oper1.action = 'borrow' "
                        + "     AND oper1.date >= '" + strStartDate + "' AND oper1.date <= '" + strEndDate + "' "
                        + "     AND oper1.readerbarcode = '" + strParameters + "' "
                        + " group by oper1.readerbarcode, oper1.itembarcode, oper1.opertime order by oper1.readerbarcode, oper1.opertime ; ";
            }
            else if (StringUtil.IsInList("141", strStyle) == true)
            {
                DateTime now = DateTimeUtil.Long8ToDateTime(strEndDate);
                now = new DateTime(now.Year, now.Month, now.Day,
                    12, 0, 0, 0);
                string strToday = now.ToString("s");
                // 141 表 超期读者清单
                strCommand = "select item.borrower, reader.name, reader.department, item.itembarcode, biblio.summary, item.borrowtime, item.borrowperiod, item.returningtime "
                        + " from item "
                        + " left JOIN reader ON item.borrower <> '' AND item.borrower = reader.readerbarcode "
                        + " left JOIN biblio ON item.bibliorecpath <> '' AND biblio.bibliorecpath = item.bibliorecpath "
                        + " where item.borrowtime <> '' "
                        + "     AND item.returningtime < '" + strToday + "' "
                        + "     AND reader.librarycode = '" + strLibraryCode + "' "
                        + " order by reader.department, item.borrower, item.returningtime ; ";
            }
            else
            {
                strError = "无法支持的 strStyle '"+strStyle+"'";
                return -1;
            }

            return 0;
        }

        static string GetString(SQLiteDataReader dr, int index)
        {
            if (dr.IsDBNull(index) == true)
                return "";
            else
                return dr.GetString(index);
        }

        static double GetDouble(SQLiteDataReader dr, int index)
        {
            if (dr.IsDBNull(index) == true)
                return 0;
            else
                return dr.GetDouble(index);
        }

        // 创建图书报表，关于流通业务
        // 1) 201 按照图书种分类的借书册数表
        // 2) 202 从来没有借出的图书 *种* 。册数列表示种下属的册数，不是被借出的册数
        // 4) 212 表 按照图书 *分类* 分类的借书册数表
        int CreateBookReportCommand(
            string strLocation, // "名称/"
            string strDateRange,
            string strStyle,
            string strParameters,
            List<string> filters,
            out string strCommand,
            out string strError)
        {
            strError = "";
            strCommand = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);
                // 2014/4/11
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            string strLocationLike = " item.location like '" + strLocation + "%' ";
            if (string.IsNullOrEmpty(strLocation) == true)
                strLocationLike = " item.location = '' ";   // 2014/5/28
            else if (strLocation == "/")
                strLocationLike = " (item.location like '/%' OR item.location not like '%/%') ";   // 全局的馆藏点比较特殊

            if (StringUtil.IsInList("201", strStyle) == true)
            {
                // 201 表 按照图书 *种* 分类的借书册数表
#if NO
                strCommand = "select item.bibliorecpath, biblio.summary, count(*) as count "
                     + " FROM operlogcircu "
                    + " JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
                     + " JOIN biblio ON item.bibliorecpath <> '' AND biblio.bibliorecpath = item.bibliorecpath "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND " + strLocationLike
                     + " GROUP BY item.bibliorecpath ORDER BY count DESC ;";
#endif
                // 2015/6/17 增加了 return 列
                strCommand = "select item.bibliorecpath, biblio.summary, "
                    + " count(case operlogcircu.operation when 'borrow' then operlogcircu.action end) as borrow, "
                    + " count(case operlogcircu.operation when 'return' then operlogcircu.action end) as return "
     + " FROM operlogcircu "
    + " JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
     + " JOIN biblio ON item.bibliorecpath <> '' AND biblio.bibliorecpath = item.bibliorecpath "
     + " WHERE operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
     + "     AND " + strLocationLike
     + " GROUP BY item.bibliorecpath ORDER BY borrow DESC ;";

            }
            else if (StringUtil.IsInList("202", strStyle) == true)
            {
                // 202 表 从来没有借出的图书 *种* 。册数列表示种下属的册数，不是被借出的册数
                strCommand = "select item.bibliorecpath, biblio.summary, count(*) as count "
                     + " FROM item "
                     + " JOIN biblio ON item.bibliorecpath <> '' AND biblio.bibliorecpath = item.bibliorecpath "
                     + " WHERE item.bibliorecpath not in "
                     + " ( select item.bibliorecpath "
                     + " FROM operlogcircu JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
                     + " WHERE operlogcircu.operation = 'borrow' and operlogcircu.action = 'borrow' "
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND "+strLocationLike+" ) "
                     + " AND " + strLocationLike    // 限定 item 表里面的记录范围为分馆的册
                     + " AND substr(item.createtime,1,10) <= '" + strEndDate.Insert(6, "-").Insert(4, "-") + "' "  // 限定册记录创建的时间在 end 以前
                     + " GROUP BY item.bibliorecpath ORDER BY item.bibliorecpath;";
            }
            else if (StringUtil.IsInList("212", strStyle) == true
                || StringUtil.IsInList("213", strStyle) == true)
            {
#if NO
                string strOperation = "borrow";

                if (StringUtil.IsInList("213", strStyle) == true)
                    strOperation = "return";
#endif

                string strClassTableName = "class_" + strParameters;

                int nRet = PrepareDistinctClassTable(
            strClassTableName,
            out strError);
                if (nRet == -1)
                    return -1;

                string strDistinctClassTableName = "class_" + strParameters + "_d";
                string strClassColumn = BuildClassColumnFragment(strDistinctClassTableName,
    filters,
    "other");
#if NO
                // 去掉重复的 bibliorecpath 条目
                string strSubSelect = " ( select * from " + strClassTableName + " group by bibliorecpath) a ";
#endif

                // 212 表 按照图书 *分类* 分类的借书册数表
#if NO
                // 213 表 按照图书 *分类* 分类的还书册数表
                strCommand = 
                    // "select substr(" + strDistinctClassTableName + ".class,1,1) as classhead, count(*) as count "
                    "select " + strClassColumn + " as classhead, count(*) as count "
                    // strCommand = "select " + strClassTableName + ".class as class, count(*) as count "
                     + " FROM operlogcircu left outer JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
                     + " left outer JOIN " + strDistinctClassTableName + " ON item.bibliorecpath <> '' AND " + strDistinctClassTableName + ".bibliorecpath = item.bibliorecpath "
                     + " WHERE operlogcircu.operation = '" + strOperation + "' "   // and operlogcircu.action = 'borrow'
                     + "     AND operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND " + strLocationLike
                     + " GROUP BY classhead ORDER BY classhead ;";
#endif
                // 2015/6/17 212 和 213 表合并为 212 表

                strCommand =
                    // "select substr(" + strDistinctClassTableName + ".class,1,1) as classhead, count(*) as count "
    "select " + strClassColumn + " as classhead, "

                    + " count(case operlogcircu.operation when 'borrow' then operlogcircu.action end) as borrow, "
                    + " count(case operlogcircu.operation when 'return' then operlogcircu.action end) as return "
                    // strCommand = "select " + strClassTableName + ".class as class, count(*) as count "
     + " FROM operlogcircu left outer JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
     + " left outer JOIN " + strDistinctClassTableName + " ON item.bibliorecpath <> '' AND " + strDistinctClassTableName + ".bibliorecpath = item.bibliorecpath "
     + " WHERE operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
     + "     AND " + strLocationLike
     + " GROUP BY classhead ORDER BY classhead ;";
            }
            else
            {
                strError = "不支持的 strStyle '"+strStyle+"'";
                return -1;
            }

            return 0;
        }

        // filters 中不允许空字符串
        static string BuildClassColumnFragment(string strClassTableName,
            List<string> filters,
            string strStyle)
        {
            if (filters == null || filters.Count == 0)
                return "substr(" + strClassTableName + ".class,1,1)";

            StringBuilder text = new StringBuilder();
            text.Append("( case ");
            foreach (string filter in filters)
            {
                text.Append(" when " + strClassTableName + ".class like '" + filter + "%' then '" + filter + "' ");
            }
            if (strStyle == "rest1")
                text.Append(" else substr(" + strClassTableName + ".class,1,1) ");
            else if (strStyle == "other")
                text.Append(" else '其它' ");
            text.Append(" end )");

            return text.ToString();
        }

        // 创建图书报表，关于典藏业务
        // 1) 301 按照馆藏地点的当前全部 图书分类 种册统计
        // 2) 302 在架情况(是否被借出)
        int CreateStorageReportCommand(
    string strLocation, // "名称/"
    string strDateRange,
    string strStyle,
    string strClassType,
            List<string> filters,
    out string strCommand,
    out string strError)
        {
            strError = "";
            strCommand = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);
                // 2014/4/11
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            string strLocationLike = " item.location like '" + strLocation + "%' ";
            if (string.IsNullOrEmpty(strLocation) == true)
                strLocationLike = " item.location = '' ";   // 2014/5/28
            else if (strLocation == "/")
                strLocationLike = " (item.location like '/%' OR item.location not like '%/%') ";   // 全局的馆藏点比较特殊

            if (StringUtil.IsInList("301", strStyle) == true)
            {
                /*
select class1, count(path1) as bcount, sum (icount) from
(
select substr(class_clc.class,1,1) as class1, item.bibliorecpath as path1, count(item.itemrecpath) as icount
 FROM item 
 JOIN class_clc ON class_clc.bibliorecpath = item.bibliorecpath 
     WHERE item.location like '合肥望湖小学/%'
group by path1 
)
group by class1
                 * 
                 * */
                string strClassTableName = "class_" + strClassType;

                int nRet = PrepareDistinctClassTable(
strClassTableName,
out strError);
                if (nRet == -1)
                    return -1;

                string strDistinctClassTableName = "class_" + strClassType + "_d";

                string strClassColumn = BuildClassColumnFragment(strDistinctClassTableName,
                    filters,
                    "other");

                strStartDate = strStartDate.Insert(6, "-").Insert(4, "-");
                strEndDate = strEndDate.Insert(6, "-").Insert(4, "-");

                // 301 表 按照图书 *分类* 分类的图书册数表
                strCommand = "select classhead, count(path1) as bcount, sum (icount) from ( "
                    // + "select substr(" + strDistinctClassTableName + ".class,1,1) as classhead, item.bibliorecpath as path1, count(item.itemrecpath) as icount "
                     + "select " + strClassColumn + " as classhead, item.bibliorecpath as path1, count(item.itemrecpath) as icount "
                     + " FROM item "
                     + " LEFT OUTER JOIN " + strDistinctClassTableName + " ON item.bibliorecpath <> '' AND " + strDistinctClassTableName + ".bibliorecpath = item.bibliorecpath "
                     + "     WHERE " + strLocationLike
                     + " AND substr(item.createtime,1,10) >= '" + strStartDate +"' "  // 限定册记录创建的时间在 start 以后
                     + " AND substr(item.createtime,1,10) <= '" + strEndDate + "' "  // 限定册记录创建的时间在 end 以前
                     + " GROUP BY path1 "
                     + " ) group by classhead ORDER BY classhead ;";
                // left outer join 是包含了左边找不到右边的那些行， 然后 class 列为 NULL
            }
            else if (StringUtil.IsInList("302", strStyle) == true)
            {
                /*
select substr(class_clc.class,1,1) as class1, 
count(case when item.borrower <> '' then item.borrower end) as outitems, 
count(case when item.borrower = '' then item.borrower end) as initems, 
count(item.itemrecpath) as icount,
printf("%.2f%", 100.0 * count(case when item.borrower <> '' then item.borrower end) / count(item.itemrecpath)) as percent
 FROM item 
 JOIN class_clc ON class_clc.bibliorecpath = item.bibliorecpath 
     WHERE item.location like '合肥望湖小学/%'
group by class1 
                 * 
                 * */
                string strClassTableName = "class_" + strClassType;

                int nRet = PrepareDistinctClassTable(
strClassTableName,
out strError);
                if (nRet == -1)
                    return -1;

                string strDistinctClassTableName = "class_" + strClassType + "_d";

                string strClassColumn = BuildClassColumnFragment(strDistinctClassTableName,
    filters,
    "other");

                strEndDate = strEndDate.Insert(6, "-").Insert(4, "-");

                // 302 表 册在架情况
                strCommand = // "select substr(" + strDistinctClassTableName + ".class,1,1) as classhead, "
                    "select " + strClassColumn + " as classhead, "
                    + " count(case when item.borrower <> '' then item.borrower end) as outitems, " 
                    + " count(case when item.borrower = '' then item.borrower end) as initems, "
                    + " count(item.itemrecpath) as icount "        
                    // + " printf(\"%.2f%\", 100.0 * count(case when item.borrower <> '' then item.borrower end) / count(item.itemrecpath)) as percent "
                     + " FROM item "
                     + " LEFT OUTER JOIN " + strDistinctClassTableName + " ON item.bibliorecpath <> '' AND " + strDistinctClassTableName + ".bibliorecpath = item.bibliorecpath "
                     + "     WHERE " + strLocationLike
                     + " AND substr(item.createtime,1,10) <= '" + strEndDate + "' "  // 限定册记录创建的时间在 end 以前
                     + " GROUP BY classhead  ORDER BY classhead ;";
                // left outer join 是包含了左边找不到右边的那些行， 然后 class 列为 NULL
            }
            else
            {
                strError = "不支持的 strStyle '" + strStyle + "'";
                return -1;
            }

            return 0;
        }

        // 获得一个分馆内读者记录的所有单位名称
        public int GetAllReaderDepartments(
            string strLibraryCode,
            out List<string> results,
            out string strError)
        {
            strError = "";

            results = new List<string>();

            string strCommand = "select department, count(*) as count "
                     + " FROM reader "
                     + " WHERE librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY department ;";

            this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {

                    try
                    {
                        SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
                        try
                        {
                            // 如果记录不存在
                            if (dr == null
                                || dr.HasRows == false)
                                return 0;

                            // 如果记录已经存在
                            while (dr.Read())
                            {
                                results.Add(dr.GetString(0));
                            }
                        }
                        finally
                        {
                            dr.Close();
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }

            return 0;
        }

        // 获得一个分馆内册记录的所有馆藏地点名称
        // parameters:
        //      bRoot   是否包含分馆这个名称。如果 == true，表示要包含，在 results 中会返回一个这样的 "望湖小学/"
        public int GetAllItemLocations(
            string strLibraryCode,
            bool bRoot,
            out List<string> results,
            out string strError)
        {
            strError = "";

            results = new List<string>();


            string strLibraryCodeFilter = "";
            if (string.IsNullOrEmpty(strLibraryCode) == true)
            {
                strLibraryCodeFilter = "(location like '/%' OR location not like '%/%') ";
            }
            else
            {
                strLibraryCodeFilter = "location like '" + strLibraryCode + "/%' ";
            }

            string strCommand = "select location, count(*) as count "
         + " FROM item "
         + " WHERE " + strLibraryCodeFilter + " "
         + " GROUP BY location ;";

            this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {

                    try
                    {
                        SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
                        try
                        {
                            // 如果记录不存在
                            if (dr == null
                                || dr.HasRows == false)
                                goto END1;

                            // 如果记录已经存在
                            while (dr.Read())
                            {
                                results.Add(dr.GetString(0));
                            }
                        }
                        finally
                        {
                            dr.Close();
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行 SQL 语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }

        END1:
            if (bRoot == true)
            {
                string strRoot = strLibraryCode + "/";
                if (results.IndexOf(strRoot) == -1)
                    results.Insert(0, strRoot);
            }

            return 0;
        }

        // 获得一个分馆内读者记录的证条码号、姓名和单位名称
        public int GetAllReaderDepartments(
            string strLibraryCode,
            ref Table table,
            out string strError)
        {
            strError = "";

            if (table == null)
                table = new Table(2);

            string strCommand = "select readerbarcode, name, department "
                     + " FROM reader "
                     + " WHERE librarycode = '" + strLibraryCode + "' "
                     + " ORDER BY department ;";

            this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(strCommand,
    connection))
                {

                    try
                    {
                        SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
                        try
                        {
                            // 如果记录不存在
                            if (dr == null
                                || dr.HasRows == false)
                                return 0;

                            // 如果记录已经存在
                            while (dr.Read())
                            {
                                string strKey = GetString(dr, 0);
                                // 证条码号 姓名
                                table.SetValue(strKey, 0, GetString(dr, 1));
                                // 单位
                                table.SetValue(strKey, 1, GetString(dr, 2));
                            }
                        }
                        finally
                        {
                            dr.Close();
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + strCommand;
                        return -1;
                    }
                } // end of using command
            }

            return 0;
        }

        // 从一个逗号间隔的字符串中析出 3 位数字
        static int GetStyleNumber(string strStyle)
        {
            string[] parts = strStyle.Split(new char [] {','});
            foreach (string s in parts)
            {
                if (s.Length == 3 && StringUtil.IsPureNumber(s) == true)
                {
                    Int32 v = 0;
                    Int32.TryParse(s, out v);
                    return v;
                }
            }

            return 0;
        }

        // 创建业务操作报表，关于采购\编目\典藏\流通\期刊\读者管理业务

        // 1) 订购流水 411
        // 2) 订购工作量 按工作人员 412
        // 1) 编目流水 421
        // 2) 编目工作量 按工作人员 422
        // 1) 册登记流水 431
        // 2) 册登记工作量 按工作人员 432
        // 1) 出纳流水 441
        // 2) 出纳工作量 按工作人员 442
        // 2) 出纳工作量 按馆藏地点 443
        // 1) 期登记流水 451
        // 2) 期登记工作量 按工作人员 452
        // 1) 违约金流水 471
        // 2) 违约金工作量 按工作人员 472
        // 1) 入馆登记流水 481
        // 2) 入馆登记工作量 按门名称 482
        // 1) 获取对象流水 491
        // 2) 获取对象工作量 按操作者 492
        int CreateWorkerReportCommand(
            string strLibraryCode,
            string strDateRange,
            string strStyle,
            string strParameters,
            out string strCommand,
            out string strError)
        {
            strError = "";
            strCommand = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);

                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            int nNumber = GetStyleNumber(strStyle);

            if (nNumber == 421)
            {
                // 421 表，编目流水
                strCommand = "select '', operlogbiblio.action, operlogbiblio.bibliorecpath, biblio.summary, operlogbiblio.opertime, operlogbiblio.operator "  // 
                     + " FROM operlogbiblio  "
                     + " left outer JOIN biblio ON operlogbiblio.bibliorecpath <> '' AND operlogbiblio.bibliorecpath = biblio.bibliorecpath "
                     + " left outer JOIN user ON operlogbiblio.operator <> '' AND operlogbiblio.operator = user.id "
                     + " WHERE "
                     + "     operlogbiblio.date >= '" + strStartDate + "' AND operlogbiblio.date <= '" + strEndDate + "' "
                     + "     AND user.librarycodelist like '%," + strLibraryCode + ",%' "
                     + " ORDER BY operlogbiblio.opertime ;";
            }
            else if (nNumber == 422)
            {
                // 422 表，每个工作人员编目各类工作量
                strCommand = "select operlogbiblio.operator,  "  // 
                    + "  count(case operlogbiblio.action when 'new' then operlogbiblio.action end) as new, "
                    + "  count(case operlogbiblio.action when 'change' then operlogbiblio.action end) as change, "
                    + "  count(case operlogbiblio.action when 'delete' then operlogbiblio.action when 'onlydeletebiblio' then 'delete' when 'onlydeletesubrecord' then 'delete' end) as del, "
                    + "  count(case operlogbiblio.action when 'copy' then operlogbiblio.action when 'onlycopybiblio' then 'copy' end) as copy, "
                    + "  count(case operlogbiblio.action when 'move' then operlogbiblio.action when 'onlymovebiblio' then 'move' end) as move, "
                    + "  count(*) as total "
                     + " FROM operlogbiblio "
                     + " left outer JOIN user ON operlogbiblio.operator <> '' AND operlogbiblio.operator = user.id "
                     + " WHERE "
                     + "     operlogbiblio.date >= '" + strStartDate + "' AND operlogbiblio.date <= '" + strEndDate + "' "
                     + "     AND user.librarycodelist like '%," + strLibraryCode + ",%' "
                     + " GROUP BY operlogbiblio.operator ORDER BY operlogbiblio.operator ;";
                ;
            }
            else if (nNumber == 431)
            {
                // 431 表，册登记流水

                string strTableName = "operlogitem";

                strCommand = "select '', " + strTableName + ".action, " + strTableName + ".bibliorecpath, biblio.summary, item.itembarcode, " + strTableName + ".itemrecpath, " + strTableName + ".opertime, " + strTableName + ".operator "  // 
                     + " FROM " + strTableName + "  "
                     + " left outer JOIN item ON " + strTableName + ".itemrecpath <> '' AND " + strTableName + ".itemrecpath = item.itemrecpath "
                     + " left outer JOIN biblio ON " + strTableName + ".bibliorecpath <> '' AND " + strTableName + ".bibliorecpath = biblio.bibliorecpath "
                     + " left outer JOIN user ON " + strTableName + ".operator <> '' AND " + strTableName + ".operator = user.id "
                     + " WHERE "
                     + "     " + strTableName + ".date >= '" + strStartDate + "' AND " + strTableName + ".date <= '" + strEndDate + "' "
                     + "     AND user.librarycodelist like '%," + strLibraryCode + ",%' "
                     + " ORDER BY " + strTableName + ".opertime ;";
            }
            else if (nNumber == 411
                // || nNumber == 431
                || nNumber == 451)
            {
                // 411 表，订购流水
                // 431 表，册登记流水
                // 451 表，期登记流水

                string strTableName = "";
                if (nNumber == 411)
                    strTableName = "operlogorder";
                //else if (nNumber == 431)
                //    strTableName = "operlogitem";
                else if (nNumber == 451)
                    strTableName = "operlogissue";

                strCommand = "select '', " + strTableName + ".action, " + strTableName + ".bibliorecpath, biblio.summary, " + strTableName + ".itemrecpath, " + strTableName + ".opertime, " + strTableName + ".operator "  // 
                     + " FROM " + strTableName + "  "
                     + " left outer JOIN biblio ON " + strTableName + ".bibliorecpath <> '' AND " + strTableName + ".bibliorecpath = biblio.bibliorecpath "
                     + " left outer JOIN user ON " + strTableName + ".operator <> '' AND " + strTableName + ".operator = user.id "
                     + " WHERE "
                     + "     " + strTableName + ".date >= '" + strStartDate + "' AND " + strTableName + ".date <= '" + strEndDate + "' "
                     + "     AND user.librarycodelist like '%," + strLibraryCode + ",%' "
                     + " ORDER BY " + strTableName + ".opertime ;";
            }
            else if (nNumber == 412
                || nNumber == 432
                || nNumber == 452)
            {
                // 412 表，每个工作人员订购各类工作量
                // 432 表，每个工作人员册登记各类工作量
                // 452 表，每个工作人员期登记各类工作量

                string strTableName = "";
                if (nNumber == 412)
                    strTableName = "operlogorder";
                else if (nNumber == 432)
                    strTableName = "operlogitem";
                else if (nNumber == 452)
                    strTableName = "operlogissue";

                strCommand = "select " + strTableName + ".operator,  "  // 
                    + "  count(case " + strTableName + ".action when 'new' then " + strTableName + ".action end) as new, "
                    + "  count(case " + strTableName + ".action when 'change' then " + strTableName + ".action end) as change, "
                    + "  count(case " + strTableName + ".action when 'delete' then " + strTableName + ".action end) as del, "
                    + "  count(case " + strTableName + ".action when 'copy' then " + strTableName + ".action end) as copy, "
                    + "  count(case " + strTableName + ".action when 'move' then " + strTableName + ".action end) as move, "
                    + "  count(*) as total "
                     + " FROM " + strTableName + " "
                     + " left outer JOIN user ON " + strTableName + ".operator <> '' AND " + strTableName + ".operator = user.id "
                     + " WHERE "
                     + "     " + strTableName + ".date >= '" + strStartDate + "' AND " + strTableName + ".date <= '" + strEndDate + "' "
                     + "     AND user.librarycodelist like '%," + strLibraryCode + ",%' "
                     + " GROUP BY " + strTableName + ".operator "
                        + " ORDER BY " + strTableName + ".operator ;";
            }
            else if (nNumber == 441)
            {
                // 441 表，出纳流水
                strCommand = "select '', operlogcircu.readerbarcode, reader.name, operlogcircu.action, operlogcircu.itembarcode, biblio.summary,  operlogcircu.opertime , operlogcircu.operator "  // 
                     + " FROM operlogcircu left outer JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
                     + " left outer JOIN biblio ON item.bibliorecpath <> '' AND item.bibliorecpath = biblio.bibliorecpath "
                     + " left outer JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE "
                     + "     operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " ORDER BY operlogcircu.opertime ;";
            }
            else if (nNumber == 442)
            {
                // 442 表，每个工作人员各类工作量
                strCommand = "select operlogcircu.operator,  "  // 
                    + "  count(case operlogcircu.action when 'borrow' then operlogcircu.action end) as borrow, "
                    + "  count(case operlogcircu.action when 'renew' then operlogcircu.action end) as renew, "
                    + "  count(case operlogcircu.action when 'return' then operlogcircu.action end) as return, "
                    + "  count(case operlogcircu.action when 'lost' then operlogcircu.action end) as lost, "
                    + "  count(*) as total "
                     + " FROM operlogcircu "
                     + " left outer JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " WHERE "
                     + "     operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY operlogcircu.operator "
                    +" ORDER BY operlogcircu.operator ;";
            }
            else if (nNumber == 443)
            {
                // 443 表，每个馆藏地点各类工作量
                strCommand = "select item.location,  "  // 
                    + "  count(case operlogcircu.action when 'borrow' then operlogcircu.action end) as borrow, "
                    + "  count(case operlogcircu.action when 'renew' then operlogcircu.action end) as renew, "
                    + "  count(case operlogcircu.action when 'return' then operlogcircu.action end) as return, "
                    + "  count(case operlogcircu.action when 'lost' then operlogcircu.action end) as lost, "
                    + "  count(*) as total "
                     + " FROM operlogcircu "
                     + " left outer JOIN reader ON operlogcircu.readerbarcode <> '' AND operlogcircu.readerbarcode = reader.readerbarcode "
                     + " left outer JOIN item ON operlogcircu.itembarcode <> '' AND operlogcircu.itembarcode = item.itembarcode "
                     + " WHERE "
                     + "     operlogcircu.date >= '" + strStartDate + "' AND operlogcircu.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY item.location ORDER BY item.location ;";
            }
            else if (nNumber == 471)
            {
                // 471 表，违约金流水
                strCommand = "select '', operlogamerce.action, operlogamerce.price, operlogamerce.unit, operlogamerce.amercerecpath, operlogamerce.reason, operlogamerce.itembarcode,biblio.summary, operlogamerce.readerbarcode, reader.name, operlogamerce.opertime, operlogamerce.operator "  // 
                     + " FROM operlogamerce "
                     + " left outer JOIN item ON operlogamerce.itembarcode <> '' AND operlogamerce.itembarcode <> '' AND operlogamerce.itembarcode = item.itembarcode "
                     + " left outer JOIN biblio ON item.bibliorecpath <> '' AND item.bibliorecpath = biblio.bibliorecpath "
                     + " left outer JOIN reader ON operlogamerce.readerbarcode <> '' AND operlogamerce.readerbarcode = reader.readerbarcode "
                     // + " left outer JOIN user ON operlogamerce.operator = user.id "
                     + " WHERE "
                     + "     operlogamerce.date >= '" + strStartDate + "' AND operlogamerce.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " ORDER BY operlogamerce.opertime ;";
            }
            else if (nNumber == 472)
            {
                /*
select operlogamerce.operator,  
operlogamerce.unit,
 count(case operlogamerce.action when 'amerce' then operlogamerce.action end) as amerce, 
 sum(case operlogamerce.action when 'amerce' then operlogamerce.price end) / 100.0 as amerce_money, 
 count(case operlogamerce.action when 'undo' then operlogamerce.action end) as undo, 
 sum(case operlogamerce.action when 'undo' then operlogamerce.price end) / 100.0 as undo_money, 
 count(case operlogamerce.action when 'expire' then operlogamerce.action end) as expire, 
 count(*) as total
from operlogamerce
 left outer JOIN reader ON operlogamerce.readerbarcode = reader.readerbarcode 
 WHERE    
   reader.librarycode = '合肥望湖小学' 
   GROUP BY operlogamerce.operator, operlogamerce.unit;
                 * 
                 * */

                // 472 表，每个工作人员违约金工作量
                strCommand = "select operlogamerce.operator,  "  // 
                    + " operlogamerce.unit, "
                    + "  count(case operlogamerce.action when 'amerce' then operlogamerce.action end) as amerce_count,"
                    + "  sum(case operlogamerce.action when 'amerce' then operlogamerce.price end) / 100.0 as amerce_money,"
                    + "  count(case operlogamerce.action when 'modifyprice' then operlogamerce.action end) as modify_count,"
                    + "  sum(case operlogamerce.action when 'modifyprice' then operlogamerce.price end) / 100.0 as modify_money,"
                    + "  count(case operlogamerce.action when 'undo' then operlogamerce.action end) as undo_count,  "
                    + "  sum(case operlogamerce.action when 'undo' then operlogamerce.price end) / 100.0 as undo_money, "
                    + "  count(case operlogamerce.action when 'expire' then operlogamerce.action end) as expire_count,  "
                    + "  count(*) as total_count "
                     + " FROM operlogamerce "
                     + " left outer JOIN reader ON operlogamerce.readerbarcode <> '' AND operlogamerce.readerbarcode = reader.readerbarcode "
                     + " WHERE "
                     + "     operlogamerce.date >= '" + strStartDate + "' AND operlogamerce.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY operlogamerce.operator, operlogamerce.unit "
                    + " ORDER BY operlogamerce.operator ;";
            }
            else if (nNumber == 481)
            {
                // 481 表，入馆登记流水
                strCommand = "select '', operlogpassgate.action, operlogpassgate.gatename, operlogpassgate.readerbarcode, reader.name, operlogpassgate.opertime, operlogpassgate.operator "  // 
                     + " FROM operlogpassgate "
                     + " left outer JOIN reader ON operlogpassgate.readerbarcode <> '' AND operlogpassgate.readerbarcode = reader.readerbarcode "
                    // + " left outer JOIN user ON operlogamerce.operator = user.id "
                     + " WHERE "
                     + "     operlogpassgate.date >= '" + strStartDate + "' AND operlogpassgate.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " ORDER BY operlogpassgate.opertime ;";
            }
            else if (nNumber == 482)
            {
                // 482 表，每个门名称的入馆登记数量
                strCommand = "select operlogpassgate.gatename,  "  // 
                     + "  count(*) as pass_count, "
                     + "  count(*) as total_count "
                     + " FROM operlogpassgate "
                     + " left outer JOIN reader ON operlogpassgate.readerbarcode <> '' AND operlogpassgate.readerbarcode = reader.readerbarcode "
                     + " WHERE "
                     + "     operlogpassgate.date >= '" + strStartDate + "' AND operlogpassgate.date <= '" + strEndDate + "' "
                     + "     AND reader.librarycode = '" + strLibraryCode + "' "
                     + " GROUP BY operlogpassgate.gatename "
                    + " ORDER BY operlogpassgate.gatename ;";
            }
            else if (nNumber == 491)
            {
                // 491 表，获取对象流水
                // TODO: 可以加上读者姓名和单位列
                strCommand = "select '', operloggetres.action, operloggetres.xmlrecpath, biblio.summary, operloggetres.objectid, operloggetres.size, operloggetres.opertime, operloggetres.operator "  // 
                     + " FROM operloggetres "
                     + " left outer JOIN biblio ON operloggetres.xmlrecpath <> '' AND operloggetres.xmlrecpath = biblio.bibliorecpath "
                     + " left outer JOIN reader ON operloggetres.operator <> '' AND operloggetres.operator = reader.readerbarcode "
                     + " left outer JOIN user ON operloggetres.operator = user.id "
                     + " WHERE "
                     + "     operloggetres.date >= '" + strStartDate + "' AND operloggetres.date <= '" + strEndDate + "' "
                     + "     AND ( reader.librarycode = '" + strLibraryCode + "' OR user.librarycodelist like '%," + strLibraryCode + ",%') "
                     + " ORDER BY operloggetres.opertime ;";
            }
            else if (nNumber == 492)
            {
                // 492 表，每个操作者获取对象的量
                strCommand = "select operloggetres.operator, reader.name, reader.department, "  // 
                    // + " operloggetres.unit, "
                    + "  count(case operloggetres.action when '' then operloggetres.action end) as get_count,"
                    + "  sum(case operloggetres.action when '' then operloggetres.size end) as get_size,"
                    + "  count(*) as total_count "
                     + " FROM operloggetres "
                     + " left outer JOIN reader ON operloggetres.operator <> '' AND operloggetres.operator = reader.readerbarcode "
                     + " left outer JOIN user ON operloggetres.operator = user.id "
                     + " WHERE "
                     + "     operloggetres.date >= '" + strStartDate + "' AND operloggetres.date <= '" + strEndDate + "' "
                     + "     AND ( reader.librarycode = '" + strLibraryCode + "' OR user.librarycodelist like '%," + strLibraryCode + ",%') "
                     + " GROUP BY operloggetres.operator "
                    + " ORDER BY operloggetres.operator ;";
            }
            else
            {
                strError = "CreateWorkerReport() 中 strStyle=" + strStyle + " 没有分支处理";
                return -1;
            }

            return 0;
        }

        // 创建分类报表，关于获取对象
        // 1) 493 按照分类的获取对象数字
        int CreateClassReportCommand(
            // string strLocation, // "名称/"
            string strLibraryCode,
    string strDateRange,
    string strStyle,
    string strClassType,
            List<string> filters,
    out string strCommand,
    out string strError)
        {
            strError = "";
            strCommand = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            if (StringUtil.IsInList("493", strStyle) == true)
            {
                /*
select substr(class_clc.class,1,1) as class1,
  count(operloggetres.action) as get_count,
  sum(operloggetres.size) as get_size
  FROM operloggetres 
 left outer JOIN reader ON operloggetres.operator <> '' AND operloggetres.operator = reader.readerbarcode 
 left outer JOIN user ON operloggetres.operator = user.id 
 JOIN class_clc ON class_clc.bibliorecpath = operloggetres.xmlrecpath 
        where ...
 GROUP BY class1 
 ORDER BY class1

                 * 
                 * */
                string strClassTableName = "class_" + strClassType;

                int nRet = PrepareDistinctClassTable(
strClassTableName,
out strError);
                if (nRet == -1)
                    return -1;

                string strDistinctClassTableName = "class_" + strClassType + "_d";

                string strClassColumn = BuildClassColumnFragment(strDistinctClassTableName,
                    filters,
                    "other");

#if NO
                strStartDate = strStartDate.Insert(6, "-").Insert(4, "-");
                strEndDate = strEndDate.Insert(6, "-").Insert(4, "-");
#endif

                // 493 表 按照图书 *分类* 的获取对象数字
                strCommand = "select " + strClassColumn + " as classhead, "
                    + " count(operloggetres.action) as get_count, "
                    + " sum(operloggetres.size) as get_size "
                     + " FROM operloggetres "
                     + " left outer JOIN reader ON operloggetres.operator <> '' AND operloggetres.operator = reader.readerbarcode "
                    + " left outer JOIN user ON operloggetres.operator = user.id "
                     + " LEFT OUTER JOIN " + strDistinctClassTableName + " ON operloggetres.xmlrecpath <> '' AND " + strDistinctClassTableName + ".bibliorecpath = operloggetres.xmlrecpath "
                     + " WHERE "
                     + "     operloggetres.date >= '" + strStartDate + "' AND operloggetres.date <= '" + strEndDate + "' "
                     + "     AND ( reader.librarycode = '" + strLibraryCode + "' OR user.librarycodelist like '%," + strLibraryCode + ",%') "
                     + " GROUP BY classhead ORDER BY classhead ;";
                // left outer join 是包含了左边找不到右边的那些行， 然后 class 列为 NULL
            }
            else if (StringUtil.IsInList("???", strStyle) == true)
            {
            }
            else
            {
                strError = "不支持的 strStyle '" + strStyle + "'";
                return -1;
            }

            return 0;
        }

        // 即将废弃
        private void toolStripButton_printHtml_Click(object sender, EventArgs e)
        {

            HtmlPrintForm printform = new HtmlPrintForm();

            printform.Text = "打印统计结果";
            printform.MainForm = this.MainForm;

            Debug.Assert(this.OutputFileNames != null, "");
            printform.Filenames = this.OutputFileNames;
            this.MainForm.AppInfo.LinkFormState(printform, "printform_state");
            printform.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(printform);
        }

        #region 统计结果 HTML 文件管理
        /// <summary>
        /// 输出的 HTML 统计结果文件名集合
        /// </summary>
        public List<string> OutputFileNames = new List<string>(); // 存放输出的html文件

        int m_nFileNameSeed = 1;

        /// <summary>
        /// 获得一个新的输出文件名
        /// </summary>
        /// <returns>输出文件名</returns>
        public string NewOutputFileName()
        {
            string strFileNamePrefix = this.MainForm.DataDir + "\\~report_";
            // string strFileNamePrefix = GetOutputFileNamePrefix();

            string strFileName = strFileNamePrefix + "_" + this.m_nFileNameSeed.ToString() + ".html";

            this.m_nFileNameSeed++;

            this.OutputFileNames.Add(strFileName);

            return strFileName;
        }

        /// <summary>
        /// 将字符串内容写入指定的文本文件。如果文件中已经存在内容，则被本次写入的覆盖
        /// </summary>
        /// <param name="strFileName">文本文件名</param>
        /// <param name="strText">要写入文件的字符串</param>
        /// <param name="encoding">编码方式</param>
        public void WriteToOutputFile(string strFileName,
            string strText,
            Encoding encoding)
        {
            using (StreamWriter sw = new StreamWriter(strFileName,
                false,	// append
                encoding))
            {
                sw.Write(strText);
            }
        }

        /// <summary>
        /// 从磁盘上删除一个输出文件，并从 OutputFileNames 集合中移走其文件名
        /// </summary>
        /// <param name="strFileName">文件名</param>
        public void DeleteOutputFile(string strFileName)
        {
            int nIndex = this.OutputFileNames.IndexOf(strFileName);
            if (nIndex != -1)
                this.OutputFileNames.RemoveAt(nIndex);

            try
            {
                File.Delete(strFileName);
            }
            catch
            {
            }
        }

        #endregion

        private void listView_libraryConfig_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改分馆配置 (&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyConfig_Click);
            if (this.listView_libraryConfig.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("创建分馆配置(&C)");
            menuItem.Click += new System.EventHandler(this.menu_newConfig_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("自动创建全部分馆配置 (&A)");
            menuItem.Click += new System.EventHandler(this.menu_autoConfig_Click);
            if (this.listView_libraryConfig.Items.Count != 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("增全分馆配置 [" + this.listView_libraryConfig.SelectedItems.Count.ToString() + "] (&A)");
            menuItem.Click += new System.EventHandler(this.menu_autoAppendConfig_Click);
            if (this.listView_libraryConfig.Items.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除 [" + this.listView_libraryConfig.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_deleteConfig_Click);
            if (this.listView_libraryConfig.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

#if NO
            string strLibraryCode = "";
            if (this.listView_libraryConfig.SelectedItems.Count == 1)
                strLibraryCode = ListViewUtil.GetItemText(this.listView_libraryConfig.SelectedItems[0], 0);
#endif

            menuItem = new MenuItem("创建选定分馆的最新报表 [" + this.listView_libraryConfig.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_createSelectedLibraryReport_Click);
            if (this.listView_libraryConfig.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

#if NO
            // 创建报表
            {
                menuItem = new MenuItem("创建报表(&R)");
                contextMenu.MenuItems.Add(menuItem);

                MenuItem subMenuItem = new MenuItem("借阅清单 (&B)...");
                subMenuItem.Click += new System.EventHandler(this.menu_createBorrowListReport_Click);
                if (this.listView_libraryConfig.SelectedItems.Count == 0)
                    subMenuItem.Enabled = false;
                menuItem.MenuItems.Add(subMenuItem);

                // ---
                subMenuItem = new MenuItem("-");
                menuItem.MenuItems.Add(subMenuItem);

            }
#endif

            contextMenu.Show(this.listView_libraryConfig, new Point(e.X, e.Y));		
        }

        // 根据配置文件类型，找到配置文件名
        static int FindCfgFileByType(XmlNode nodeLibrary,
            string strTypeParam,
            out string strCfgFile,
            out string strError)
        {
            strError = "";
            XmlNodeList nodes = nodeLibrary.SelectNodes("reports/report");
            foreach (XmlNode node in nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");
                if (strType == strTypeParam)
                {
                    strCfgFile = DomUtil.GetAttr(node, "cfgFile");
                    return 1;
                }
            }

            strCfgFile = "";
            return 0;
        }

        // 增全分馆配置
        void menu_autoAppendConfig_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bChanged = false;
            foreach (ListViewItem item in this.listView_libraryConfig.SelectedItems)
            {
                string strLibraryCode = ListViewUtil.GetItemText(item, 0);
                strLibraryCode = GetOriginLibraryCode(strLibraryCode);

                XmlNode nodeLibrary = this._cfg.GetLibraryNode(strLibraryCode);
                if (nodeLibrary == null)
                {
                    strError = "在配置文件中没有找到馆代码为 '" + strLibraryCode + "' 的 <library> 元素";
                    goto ERROR1;
                }

                LibraryReportConfigForm dlg = new LibraryReportConfigForm();

                dlg.MainForm = this.MainForm;
                dlg.ReportForm = this;
                dlg.LoadData(nodeLibrary);
                dlg.ModifyMode = true;

                // 自动增全报表配置
                // return:
                //      -1  出错
                //      >=0 新增的报表类型个数
                nRet = dlg.AutoAppend(out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet > 0)
                {
                    bChanged = true;

                    dlg.SetData(nodeLibrary);
                }

            }

            if (bChanged == true)
                this._cfg.Save();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 首次自动创建全部分馆的配置
        void menu_autoConfig_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            LibraryReportConfigForm dlg = new LibraryReportConfigForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.ReportForm = this;


            List<string> librarycodes = this.MainForm.GetAllLibraryCode();
            foreach (string code in librarycodes)
            {
                nRet = dlg.AutoCreate(code, out strError);
                if (nRet == -1)
                    goto ERROR1;

                XmlNode nodeLibrary = null;
                // 创建一个新的 <library> 元素。要对 code 属性进行查重
                // parameters:
                //      -1  出错
                //      0   成功
                //      1   已经有这个 code 属性的元素了
                nRet = this._cfg.CreateNewLibraryNode(dlg.LibraryCode,
                    out nodeLibrary,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    continue;

                dlg.SetData(nodeLibrary);

                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, 0, GetDisplayLibraryCode(dlg.LibraryCode));
                this.listView_libraryConfig.Items.Add(item);
                ListViewUtil.SelectLine(item, true);

            }

            this._cfg.Save();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 修改一个分馆配置
        void menu_modifyConfig_Click(object sender, EventArgs e)
        {
            string strError = "";
            //int nRet = 0;

            if (this.listView_libraryConfig.SelectedItems.Count == 0)
            {
                strError = "尚未选定要修改的事项";
                goto ERROR1;
            }

            ListViewItem item = this.listView_libraryConfig.SelectedItems[0];
            string strLibraryCode = ListViewUtil.GetItemText(item, 0);
            strLibraryCode = GetOriginLibraryCode(strLibraryCode);

            XmlNode nodeLibrary = this._cfg.GetLibraryNode(strLibraryCode);
            if (nodeLibrary == null)
            {
                strError = "在配置文件中没有找到馆代码为 '" + strLibraryCode + "' 的 <library> 元素";
                goto ERROR1;
            }

            LibraryReportConfigForm dlg = new LibraryReportConfigForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.ReportForm = this;
            dlg.LoadData(nodeLibrary);
            dlg.ModifyMode = true;

            this.MainForm.AppInfo.LinkFormState(dlg, "LibraryReportConfigForm_state");
            dlg.UiState = this.MainForm.AppInfo.GetString(GetReportSection(), "LibraryReportConfigForm_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString(GetReportSection(), "LibraryReportConfigForm_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            dlg.SetData(nodeLibrary);

#if NO
            ListViewUtil.ChangeItemText(item, 0, dlg.LibraryCode);
#endif

            this._cfg.Save();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 创建一个新的分馆配置
        void menu_newConfig_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            LibraryReportConfigForm dlg = new LibraryReportConfigForm();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.ReportForm = this;

            nRet = dlg.AutoCreate("", out strError);
            if (nRet == -1)
                goto ERROR1;

            REDO:
            this.MainForm.AppInfo.LinkFormState(dlg, "LibraryReportConfigForm_state");
            dlg.UiState = this.MainForm.AppInfo.GetString(GetReportSection(), "LibraryReportConfigForm_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString(GetReportSection(), "LibraryReportConfigForm_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            XmlNode nodeLibrary = null;
            // 创建一个新的 <library> 元素。要对 code 属性进行查重
            // parameters:
            //      -1  出错
            //      0   成功
            //      1   已经有这个 code 属性的元素了
            nRet = this._cfg.CreateNewLibraryNode(dlg.LibraryCode,
                out nodeLibrary,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                MessageBox.Show(this, strError + "\r\n\r\n请修改馆代码");
                goto REDO;
            }

            dlg.SetData(nodeLibrary);

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, 0, GetDisplayLibraryCode(dlg.LibraryCode));
            this.listView_libraryConfig.Items.Add(item);
            ListViewUtil.SelectLine(item, true);

            this._cfg.Save();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除选定的分馆配置
        void menu_deleteConfig_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_libraryConfig.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的事项";
                goto ERROR1;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除选定的 " + this.listView_libraryConfig.SelectedItems.Count.ToString() + " 个事项?",
"ReportForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            foreach(ListViewItem item in this.listView_libraryConfig.SelectedItems)
            {
                string strLibraryCode = ListViewUtil.GetItemText(item, 0);
                strLibraryCode = GetOriginLibraryCode(strLibraryCode);

                XmlNode nodeLibrary = this._cfg.GetLibraryNode(strLibraryCode);
                if (nodeLibrary == null)
                {
                    strError = "在配置文件中没有找到馆代码为 '" + strLibraryCode + "' 的 <library> 元素";
                    // goto ERROR1;
                    continue;
                }

                nodeLibrary.ParentNode.RemoveChild(nodeLibrary);
            }

            ListViewUtil.DeleteSelectedItems(this.listView_libraryConfig);

            if (this._cfg != null)
                this._cfg.Save();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存分馆参数配置
        private void toolStripButton_libraryConfig_save_Click(object sender, EventArgs e)
        {
            if (this._cfg != null)
                this._cfg.Save();
        }

        private void listView_libraryConfig_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyConfig_Click(sender, e);
        }

        // 获得本窗口选用了的馆代码
        // parameters:
        //      bAll    是否返回全部分馆的关代码? true 返回全部；false 只返回选定了的
        public List<string> GetLibraryCodes(bool bAll = true)
        {
            List<string> results = new List<string>();
            foreach (ListViewItem item in this.listView_libraryConfig.Items)
            {
                if (bAll == false && item.Selected == false)
                    continue;
                string strLibraryCode = ListViewUtil.GetItemText(item, 0);
                strLibraryCode = GetOriginLibraryCode(strLibraryCode);

                results.Add(strLibraryCode);
            }

            return results;
        }

        // 0.01 (2014/4/30) 第一个版本
        // 0.02 (2014/5/6) item 表 增加了两个 index: item_itemrecpath_index 和 item_biliorecpath_index
        // 0.03 (2014/5/29) item 表增加了 borrower borrowtime borrowperiod returningtime 等字段
        // 0.04 (2014/6/2) breakpoint 文件根元素下增加了 classStyles 元素
        // 0.05 (2014/6/12) item 表增加了 price unit 列； operlogamerce 表的 price 列分开为 price 和 unit 列
        // 0.06 (2014/6/16) operlogxxx 表中增加了 subno 字段
        // 0.07 (2014/6/19) operlogitem 表增加了 itembarcode 字段
        // 0.08 (2014/11/6) reader 表增加了 state 字段 
        // 0.09 (2015/7/14) 增加了 operlogpassgate 和 operloggetres 表
        static double _version = 0.09;

        // TODO: 最好把第一次初始化本地 sql 表的动作也纳入 XML 文件中，这样做单项任务的时候，就不会毁掉其他的表
        // 创建批处理计划
        // 根元素的 state 属性， 值为 first 表示正在进行首次创建，尚未完成; daily 表示已经创建完，进入每日同步阶段
        int BuildPlan(string strTypeList,
            out XmlDocument task_dom,
            out string strError)
        {
            strError = "";
            long lRet = 0;
            int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在计划任务 ...");
            stop.BeginLoop();

            try
            {

                task_dom = new XmlDocument();
                task_dom.LoadXml("<root />");

                // 开始处理时的日期
                string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

                // 获得日志文件中记录的总数
                // parameters:
                //      strDate 日志文件的日期，8 字符
                // return:
                //      -1  出错
                //      0   日志文件不存在，或者记录数为 0
                //      >0  记录数
                long lCount = GetOperLogCount(strEndDate,
                    out strError);
                if (nRet == -1)
                    return -1;

                DomUtil.SetAttr(task_dom.DocumentElement, "version", _version.ToString());

                DomUtil.SetAttr(task_dom.DocumentElement,
                    "state", "first");  // 表示首次创建尚未完成

                // 记载首次创建的结束时间点
                DomUtil.SetAttr(task_dom.DocumentElement, "end_date", strEndDate);
                DomUtil.SetAttr(task_dom.DocumentElement, "index", lCount.ToString());

                // *** 创建用户表
                if (strTypeList == "*"
                    || StringUtil.IsInList("user", strTypeList) == true)
                {

                    XmlNode node = task_dom.CreateElement("user");
                    task_dom.DocumentElement.AppendChild(node);
                }

                // *** 创建 item 表
                if (strTypeList == "*"
                    || StringUtil.IsInList("item", strTypeList) == true)
                {
                    // 获得全部实体库名
                    List<string> item_dbnames = new List<string>();
                    if (this.MainForm.BiblioDbProperties != null)
                    {
                        for (int i = 0; i < this.MainForm.BiblioDbProperties.Count; i++)
                        {
                            BiblioDbProperty property = this.MainForm.BiblioDbProperties[i];

                            if (String.IsNullOrEmpty(property.ItemDbName) == false)
                                item_dbnames.Add(property.ItemDbName);
                        }
                    }

                    // 获得每个实体库的尺寸
                    foreach (string strItemDbName in item_dbnames)
                    {
                        stop.SetMessage("正在计划任务 检索 " + strItemDbName + " ...");

                        lRet = this.Channel.SearchItem(stop,
            strItemDbName,
            "", // 
            -1,
            "__id",
            "left",
            "zh",
            null,   // strResultSetName
            "",    // strSearchStyle
            "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
            out strError);
                        if (lRet == -1)
                            return -1;

                        XmlNode node = task_dom.CreateElement("database");
                        task_dom.DocumentElement.AppendChild(node);

                        DomUtil.SetAttr(node, "name", strItemDbName);
                        DomUtil.SetAttr(node, "type", "item");
                        DomUtil.SetAttr(node, "count", lRet.ToString());
                    }
                }

                // *** 创建 reader 表
                if (strTypeList == "*"
                    || StringUtil.IsInList("reader", strTypeList) == true)
                {
                    // 获得全部读者库名
                    List<string> reader_dbnames = new List<string>();
                    if (this.MainForm.ReaderDbNames != null)
                    {
                        foreach (string s in this.MainForm.ReaderDbNames)
                        {
                            if (String.IsNullOrEmpty(s) == false)
                                reader_dbnames.Add(s);
                        }
                    }

                    // 
                    foreach (string strReaderDbName in reader_dbnames)
                    {
                        stop.SetMessage("正在计划任务 检索 " + strReaderDbName + " ...");
                        lRet = this.Channel.SearchReader(stop,
            strReaderDbName,
            "", // 
            -1,
            "__id",
            "left",
            "zh",
            null,   // strResultSetName
            "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
            out strError);
                        if (lRet == -1)
                            return -1;

                        XmlNode node = task_dom.CreateElement("database");
                        task_dom.DocumentElement.AppendChild(node);

                        DomUtil.SetAttr(node, "name", strReaderDbName);
                        DomUtil.SetAttr(node, "type", "reader");
                        DomUtil.SetAttr(node, "count", lRet.ToString());

                    }
                }

                // *** 创建 biblio 表
                // *** 创建 class 表
                if (strTypeList == "*"
                    || StringUtil.IsInList("biblio", strTypeList) == true)
                {

                    // 获得全部书目库名
                    List<string> biblio_dbnames = new List<string>();
                    if (this.MainForm.BiblioDbProperties != null)
                    {
                        foreach (BiblioDbProperty prop in this.MainForm.BiblioDbProperties)
                        {
                            if (String.IsNullOrEmpty(prop.DbName) == false)
                                biblio_dbnames.Add(prop.DbName);
                        }
                    }

#if NO

                    // 获得所有分类号检索途径 style
                    List<string> styles = new List<string>();
                    nRet = GetClassFromStyles(out styles,
                        out strError);
                    if (nRet == -1)
                        return -1;
#endif
                    // 记忆书目库的分类号 style 列表
                    nRet = MemoryClassFromStyles(task_dom.DocumentElement,
            out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        DialogResult result = MessageBox.Show(this,
strError + "\r\n\r\n建议先中断处理，配置好书目库的分类号检索点再重新创建本地存储。如果此时继续处理，则会无法同步分类号信息，以后也无法创建和分类号有关的报表。\r\n\r\n是否继续处理?",
"ReportForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                        if (result == System.Windows.Forms.DialogResult.No)
                            return -1;  // 
                    }

                    // 从计划文件中获得所有分类号检索途径 style
                    List<string> styles = new List<string>();
                    nRet = GetClassFromStyles(
                        task_dom.DocumentElement,
                        out styles,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    //
                    foreach (string strBiblioDbName in biblio_dbnames)
                    {
                        stop.SetMessage("正在计划任务 检索 " + strBiblioDbName + " ...");
                        string strQueryXml = "";
                        lRet = this.Channel.SearchBiblio(stop,
                            strBiblioDbName,
                            "", // 
                            -1,
                            "recid",     // "__id",
                            "left",
                            "zh",
                            null,   // strResultSetName
                            "",    // strSearchStyle
                            "", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                            out strQueryXml,
                            out strError);
                        if (lRet == -1)
                            return -1;

                        XmlNode node = task_dom.CreateElement("database");
                        task_dom.DocumentElement.AppendChild(node);

                        DomUtil.SetAttr(node, "name", strBiblioDbName);
                        DomUtil.SetAttr(node, "type", "biblio");
                        DomUtil.SetAttr(node, "count", lRet.ToString());

                        foreach (string strStyle in styles)
                        {
                            stop.SetMessage("正在计划任务 检索 " + strBiblioDbName + " " + strStyle + " ...");
                            lRet = this.Channel.SearchBiblio(stop,
                                strBiblioDbName,
                                "", // 
                                -1,
                                strStyle,     // "__id",
                                "left", // this.textBox_queryWord.Text == "" ? "left" : "exact",    // 原来为left 2007/10/18 changed
                                "zh",
                                null,   // strResultSetName
                                "",    // strSearchStyle
                                "keyid", //strOutputStyle, // (bOutputKeyCount == true ? "keycount" : ""),
                                out strQueryXml,
                                out strError);
                            if (lRet == -1)
                            {
                                if (this.Channel.ErrorCode == ErrorCode.FromNotFound)
                                    continue;
                                return -1;
                            }
                            string strClassTableName = "class_" + strStyle;

                            XmlNode class_node = task_dom.CreateElement("class");
                            node.AppendChild(class_node);

                            DomUtil.SetAttr(class_node, "from_style", strStyle);
                            DomUtil.SetAttr(class_node, "class_table_name", strClassTableName);
                            DomUtil.SetAttr(class_node, "count", lRet.ToString());
                        }
                    }
                }

                // *** 创建日志表
                if (strTypeList == "*"
                    || StringUtil.IsInList("operlog", strTypeList) == true)
                {
                    string strFirstDate = "";
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetFirstOperLogDate(out strFirstDate,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获得第一个日志文件日期时出错: " + strError;
                        return -1;
                    }

                    if (nRet == 1)
                    {
                        // 记载第一个日志文件日期
                        DomUtil.SetAttr(task_dom.DocumentElement,
                            "first_operlog_date",
                            strFirstDate);

                        this.MainForm.AppInfo.SetString(GetReportSection(),
                            "daily_report_end_date",
                            strFirstDate);

                        XmlNode node = task_dom.CreateElement("operlog");
                        task_dom.DocumentElement.AppendChild(node);

                        DomUtil.SetAttr(node, "start_date", strFirstDate);  // "20060101"
                        DomUtil.SetAttr(node, "end_date", strEndDate + ":0-" + (lCount - 1).ToString());
                    }
                }

                return 0;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }
        }

        // 获得第一个(实有的)日志文件日期
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetFirstOperLogDate(out string strFirstDate,
            out string strError)
        {
            strFirstDate = "";
            strError = "";

            DigitalPlatform.CirculationClient.localhost.OperLogInfo[] records = null;
            // 获得日志
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围，本次调用无效
            long lRet = this.Channel.GetOperLogs(
                null,
                "",
                0,
                -1,
                1,
                "getfilenames",
                "", // strFilter
                out records,
                out strError);
            if (lRet == -1)
                return -1;
            if (lRet == 0)
                return 0;

            if (records == null || records.Length < 1)
            {
                strError = "records error";
                return -1;
            }

            if (string.IsNullOrEmpty(records[0].Xml) == true
                || records[0].Xml.Length < 8)
            {
                strError = "records[0].Xml error";
            }

            strFirstDate = records[0].Xml.Substring(0, 8);
            return 1;
        }

        ProgressEstimate _estimate = new ProgressEstimate();

        string GetProgressTimeString(long lProgressValue)
        {
#if NO
            string strStage = "";
            if (this._stage >= 0)
                strStage = "第 " + (this._stage + 1).ToString() + " 阶段";
#endif

            return "剩余时间 " + ProgressEstimate.Format(_estimate.Estimate(lProgressValue)) + " 已经过时间 " + ProgressEstimate.Format(_estimate.delta_passed);
        }

        // 初始化若干 operlog 表
        int CreateOperLogTables(out string strError)
        {
            strError = "";

            // string[] types = {"circu" };

            foreach (string type in OperLogTable.DbTypes)
            {
                int nRet = OperLogTable.CreateOperLogTable(
                    type,
                    this._connectionString,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
#if NO
            int nRet = OperLogLine.CreateOperLogTable(
this._connectionString,
out strError);
            if (nRet == -1)
                return -1;
#endif

            return 0;
        }

        // 执行首次创建本地存储的计划
        // parameters:
        //      task_dom    存储了计划信息的 XMlDocument 对象。执行后，里面的信息会记载了断点信息等。如果完全完成，则保存前可以仅仅留下结束点信息
        int DoPlan(ref XmlDocument task_dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建本地存储 ...");
            stop.BeginLoop();

            try
            {
                this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

                // 初始化各种表，除了 operlogXXX 表以外
                string strInitilized = DomUtil.GetAttr(task_dom.DocumentElement,
                    "initial_tables");
                if (strInitilized != "finish")
                {
                    stop.SetMessage("正在初始化本地数据库 ...");
                    nRet = ItemLine.CreateItemTable(
        this._connectionString,
        out strError);
                    if (nRet == -1)
                        return -1;

                    nRet = ReaderLine.CreateReaderTable(
        this._connectionString,
        out strError);
                    if (nRet == -1)
                        return -1;

                    nRet = BiblioLine.CreateBiblioTable(
        this._connectionString,
        out strError);
                    if (nRet == -1)
                        return -1;

                    {
                        List<string> styles = new List<string>();

                        // 获得所有分类号检索途径 style
                        nRet = GetClassFromStyles(
                            task_dom.DocumentElement,
                            out styles,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        foreach (string strStyle in styles)
                        {
                            nRet = ClassLine.CreateClassTable(
                                this._connectionString,
                                "class_" + strStyle,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                    }

#if NO
                    stop.SetMessage("正在初始化本地数据库的日志表 ...");
                    Application.DoEvents();

                    nRet = CreateOperLogTables(out strError);
                    if (nRet == -1)
                        return -1;
#endif

                    DomUtil.SetAttr(task_dom.DocumentElement,
                        "initial_tables", "finish");
                }

                // 先累计总记录数，以便设置进度条
                long lTotalCount = 0;
                XmlNodeList nodes = task_dom.DocumentElement.SelectNodes("database/class | database");
                foreach (XmlNode node in nodes)
                {
                    string strState = DomUtil.GetAttr(node, "state");
                    if (strState == "finish")
                        continue;

                    long lCount = 0;
                    nRet = DomUtil.GetIntegerParam(node,
                        "count",
                        0,
                        out lCount,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    lTotalCount += lCount;
                }

                stop.SetProgressRange(0, lTotalCount * 2); // 第一阶段，占据进度条一半
                long lProgress = 0;

                _estimate.SetRange(0, lTotalCount * 2);
                _estimate.StartEstimate();

                foreach (XmlNode node in task_dom.DocumentElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    if (node.Name == "database")
                    {
                        string strDbName = DomUtil.GetAttr(node, "name");
                        string strType = DomUtil.GetAttr(node, "type");
                        string strState = DomUtil.GetAttr(node, "state");

                        long lIndex = 0;
                        nRet = DomUtil.GetIntegerParam(node,
                            "index",
                            0,
                            out lIndex,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        long lCurrentCount = 0;
                        nRet = DomUtil.GetIntegerParam(node,
                            "count",
                            0,
                            out lCurrentCount,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (strType == "item" && strState != "finish")
                        {
                            nRet = BuildItemRecords(
        strDbName,
        lCurrentCount,
        ref lProgress,
        ref lIndex,
        out strError);
                            if (nRet == -1)
                            {
                                DomUtil.SetAttr(node, "index", lIndex.ToString());
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }

                        if (strType == "reader" && strState != "finish")
                        {
                            nRet = BuildReaderRecords(
        strDbName,
        lCurrentCount,
        ref lProgress,
        ref lIndex,
        out strError);
                            if (nRet == -1)
                            {
                                DomUtil.SetAttr(node, "index", lIndex.ToString());
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }

                        if (strType == "biblio")
                        {
                            if (strState != "finish")
                            {
                                nRet = BuildBiblioRecords(
            strDbName,
            lCurrentCount,
        ref lProgress,
        ref lIndex,
            out strError);
                                if (nRet == -1)
                                {
                                    DomUtil.SetAttr(node, "index", lIndex.ToString());
                                    return -1;
                                }
                                DomUtil.SetAttr(node, "state", "finish");
                            }

                            XmlNodeList class_nodes = node.SelectNodes("class");
                            foreach (XmlNode class_node in class_nodes)
                            {
                                string strFromStyle = DomUtil.GetAttr(class_node, "from_style");
                                string strClassTableName = DomUtil.GetAttr(class_node, "class_table_name");
                                strState = DomUtil.GetAttr(class_node, "state");
                                lIndex = 0;
                                nRet = DomUtil.GetIntegerParam(class_node,
                                    "index",
                                    0,
                                    out lIndex,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                //
                                nRet = DomUtil.GetIntegerParam(class_node,
    "count",
    0,
    out lCurrentCount,
    out strError);
                                if (nRet == -1)
                                    return -1;


                                if (strState != "finish")
                                {
                                    nRet = BuildClassRecords(
                                        strDbName,
                                        strFromStyle,
                                        strClassTableName,
                                        lCurrentCount,
                                        ref lProgress,
                                        ref lIndex,
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        DomUtil.SetAttr(class_node, "index", lIndex.ToString());
                                        return -1;
                                    }
                                    DomUtil.SetAttr(class_node, "state", "finish");
                                }
                            }
                        }
                    }

                    if (node.Name == "user")
                    {
                        string strState = DomUtil.GetAttr(node, "state");

#if NO
                        string strTableInitilized = DomUtil.GetAttr(node,
    "initial_tables");
                        if (strTableInitilized != "finish")
                        {
                            stop.SetMessage("正在初始化本地数据库的用户表 ...");
                            Application.DoEvents();

                            this._connectionString = GetOperlogConnectionString();
                            nRet = UserLine.CreateUserTable(
                                this._connectionString,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            DomUtil.SetAttr(node,
                                "initial_tables", "finish");
                        }
#endif

                        if (strState != "finish")
                        {
                            nRet = DoCreateUserTable(
                                out strError);
                            if (nRet == -1)
                            {
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }
                    }

                    if (node.Name == "operlog")
                    {
                        string strTableInitilized = DomUtil.GetAttr(node,
    "initial_tables");

                        string strStartDate = DomUtil.GetAttr(node, "start_date");
                        string strEndDate = DomUtil.GetAttr(node, "end_date");
                        string strState = DomUtil.GetAttr(node, "state");

                        if (string.IsNullOrEmpty(strStartDate) == true)
                        {
                            // strStartDate = "20060101";
                            strError = "start_date 属性值不应为空: " + node.OuterXml;
                            return -1;
                        }
                        if (string.IsNullOrEmpty(strEndDate) == true)
                        {
                            // strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);
                            strError = "end_date 属性值不应为空: " + node.OuterXml;
                            return -1;
                        }

                        if (strTableInitilized != "finish")
                        {
                            stop.SetMessage("正在初始化本地数据库的日志表 ...");
                            Application.DoEvents();

                            nRet = CreateOperLogTables(out strError);
                            if (nRet == -1)
                                return -1;

                            DomUtil.SetAttr(node,
                                "initial_tables", "finish");
                        }

                        if (strState != "finish")
                        {
                            string strLastDate = "";
                            long lLastIndex = 0;
                            // TODO: 中断时断点记载
                            // TODO: 进度条应该是重新设置的
                            nRet = DoCreateOperLogTable(
                                -1,
                                strStartDate,
                                strEndDate,
                                out strLastDate,
                                out lLastIndex,
                                out strError);
                            if (nRet == -1)
                            {
                                if (string.IsNullOrEmpty(strLastDate) == false)
                                    DomUtil.SetAttr(node, "start_date", strLastDate + ":" + lLastIndex.ToString() + "-");
                                return -1;
                            }
                            DomUtil.SetAttr(node, "state", "finish");
                        }
                    }
                }

                // TODO: 全部完成后，需要在 task_dom 中清除不必要的信息
                DomUtil.SetAttr(task_dom.DocumentElement,
                    "state", "daily");  // 表示首次创建已经完成，进入每日同步阶段
                return 0;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }
        }

        // 创建 user 表
        int DoCreateUserTable(out string strError)
        {
            strError = "";

            stop.SetMessage("正在初始化本地数据库的用户表 ...");
            Application.DoEvents();

            this._connectionString = GetOperlogConnectionString();
            int nRet = UserLine.CreateUserTable(
                this._connectionString,
                out strError);
            if (nRet == -1)
                return -1;

            using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
            {
                connection.Open();

                int nStart = 0;
                for (; ; )
                {
                    UserInfo[] users = null;
                    long lRet = Channel.GetUser(
                        stop,
                        "list",
                        "",
                        nStart,
                        -1,
                        out users,
                        out strError);
                    if (lRet == -1)
                        return -1;
                    if (lRet == 0)
                    {
                        strError = "不存在用户信息。";
                        return 0;   // not found
                    }

                    Debug.Assert(users != null, "");

                    List<UserLine> lines = new List<UserLine>();
                    for (int i = 0; i < users.Length; i++)
                    {
                        UserInfo info = users[i];

                        UserLine line = new UserLine();
                        line.ID = info.UserName;
                        line.Rights = info.Rights;
                        line.LibraryCodeList = "," + info.LibraryCode + ",";
                        lines.Add(line);
                    }

                    // 插入一批用户记录
                    nRet = UserLine.AppendClassLines(
                        connection,
                        lines,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    nStart += users.Length;
                    if (nStart >= lRet)
                        break;
                }
            }

            return 0;
        }

        // 清除断点文件
        void ClearBreakPoint()
        {
            string strBreakPointFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");

            File.Delete(strBreakPointFileName);

            SetStartButtonStates();
            SetDailyReportButtonState();
        }

        // 创建本地存储
        // TODO: 中间从服务器复制表的阶段，也应该可以中断，以后可以从断点继续。会出现一个对话框，询问是否继续
        private void button_start_createLocalStorage_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strBreakPointFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");

            // File.Delete(strBreakPointFileName);

            // XmlDocument task_dom = null;

            XmlDocument task_dom = new XmlDocument();
            try
            {
                task_dom.Load(strBreakPointFileName);
            }
            catch (FileNotFoundException)
            {
                task_dom = null;
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strBreakPointFileName + "' 时出错: " + ex.Message;
                goto ERROR1;
            }

            if (task_dom == null)
            {
                // 创建批处理计划
                nRet = BuildPlan("*",    // * "operlog"
                    out task_dom,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                task_dom.Save(strBreakPointFileName);
            }

            try
            {
                nRet = DoPlan(ref task_dom,
                    out strError);
            }
            finally
            {
                task_dom.Save(strBreakPointFileName);
            }

            if (nRet == -1)
                goto ERROR1;

            SetStartButtonStates();
            SetDailyReportButtonState();
            MessageBox.Show(this, "处理完成");
            return;
        ERROR1:
            SetStartButtonStates();
            SetDailyReportButtonState();
            MessageBox.Show(this, strError);
        }

        void SetStartButtonStates()
        {
            string strError = "";
            int nRet = 0;

            string strEndDate = "";
            long lIndex = 0;
            string strState = "";

            // 读入断点信息
            // return:
            //      -1  出错
            //      0   正常
            //      1   首次创建尚未完成
            nRet = LoadDailyBreakPoint(
                out strEndDate,
                out lIndex,
                out strState,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            if (strState == "daily")
            {
                this.button_start_dailyReplication.Enabled = true;
                this.button_start_createLocalStorage.Enabled = false;
            }
            else if (string.IsNullOrEmpty(strState) == true || strState == "first")
            {
                this.button_start_dailyReplication.Enabled = false;
                this.button_start_createLocalStorage.Enabled = true;
            }
            else
            {
                this.button_start_dailyReplication.Enabled = false;
                this.button_start_createLocalStorage.Enabled = false;
            }

            if (string.IsNullOrEmpty(strEndDate) == true)
                this.button_start_dailyReplication.Text = "每日同步";
            else
                this.button_start_dailyReplication.Text = "每日同步 " + strEndDate + "-";

            if (strState == "first")
            {
                this.button_start_createLocalStorage.Text = "继续创建本地存储";
            }
            else
            {
                this.button_start_createLocalStorage.Text = "首次创建本地存储";
            }

            this.checkBox_start_enableFirst.Checked = false;
        }

        // 写入断点信息
        int WriteDailyBreakPoint(
            string strEndDate,
            long lIndex,
            out string strError)
        {
            strError = "";

            string strFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (FileNotFoundException)
            {
                dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strFileName + "' 时出错: " + ex.Message;
                return -1;
            }


            DomUtil.SetAttr(dom.DocumentElement, "end_date", strEndDate);
            DomUtil.SetAttr(dom.DocumentElement, "index", lIndex.ToString());

            dom.Save(strFileName);
            return 0;
        }

        // 读入断点信息的版本号
        // return:
        //      -1  出错
        //      0   文件不存在
        //      1   成功
        int GetBreakPointVersion(
            out double version,
            out string strError)
        {
            strError = "";
            version = 0;

            string strFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strFileName + "' 时出错: " + ex.Message;
                return -1;
            }

            int nRet = DomUtil.GetDoubleParam(dom.DocumentElement, "version",
                0,
                out version,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // 读入断点信息
        // return:
        //      -1  出错
        //      0   正常
        //      1   首次创建尚未完成
        int LoadDailyBreakPoint(
            out string strEndDate,
            out long lIndex,
            out string strState,
            out string strError)
        {
            strError = "";
            strEndDate = "";
            strState = "";
            lIndex = 0;

            string strFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (FileNotFoundException)
            {
                dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                strError = "装载文件 '" + strFileName + "' 时出错: " + ex.Message;
                return -1;
            }

            strEndDate = DomUtil.GetAttr(dom.DocumentElement, "end_date");
            int nRet = DomUtil.GetIntegerParam(dom.DocumentElement, "index",
                0, 
                out lIndex,
                out strError);
            if (nRet == -1)
                return -1;

            strState = DomUtil.GetAttr(dom.DocumentElement,
                "state");
            if (strState != "daily")
                return 1;   // 首次创建尚未完成

            return 0;
        }

        // 获得和当前服务器、用户相关的报表信息本地存储目录
        string GetBaseDirectory()
        {
            // 2015/6/20 将数据库文件存储在和每个 dp2library 服务器和用户名相关的目录中
            string strDirectory = Path.Combine(this.MainForm.ServerCfgDir, ReportForm.GetValidPathString(this.MainForm.GetCurrentUserName()));
            PathUtil.CreateDirIfNeed(strDirectory);
            return strDirectory;
        }

        string GetOperlogConnectionString()
        {
            // return SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

            return SQLiteUtil.GetConnectionString(GetBaseDirectory(), "operlog.bin");
        }

        // DoReplication() 过程中使用的 class 属性列表
        List<BiblioDbFromInfo> _classFromStyles = new List<BiblioDbFromInfo>();

        // 同步
        // 注：中途遇到异常(例如 Loader 抛出异常)，可能会丢失 INSERT_BATCH 条以内的日志记录写入 operlog 表
        // parameters:
        //      strLastDate   处理中断或者结束时返回最后处理过的日期
        //      last_index  处理或中断返回时最后处理过的位置。以后继续处理的时候可以从这个偏移开始
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        int DoReplication(
            string strStartDate,
            string strEndDate,
            // long index,
            out string strLastDate,
            out long last_index,
            out string strError)
        {
            strError = "";
            strLastDate = "";
            last_index = -1;    // -1 表示尚未处理

            bool bUserChanged = false;

            // strStartDate 里面可能会包含 ":1-100" 这样的附加成分
            string strLeft = "";
            string strRight = "";
            StringUtil.ParseTwoPart(strStartDate,
                ":",
                out strLeft,
                out strRight);
            strStartDate = strLeft;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在进行同步 ...");
            stop.BeginLoop();

            try
            {
                List<BiblioDbFromInfo> styles = null;
                // 获得所有分类号检索途径 style
                int nRet = GetClassFromStylesFromFile(out styles,
                    out strError);
                if (nRet == -1)
                    return -1;
                
                _classFromStyles = styles;

                _updateBiblios.Clear();
                _updateItems.Clear();
                _updateReaders.Clear();

                string strWarning = "";
                List<string> dates = null;
                nRet = OperLogStatisForm.MakeLogFileNames(strStartDate,
                    strEndDate,
                    false,  // 是否包含扩展名 ".log"
                    out dates,
                    out strWarning,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (dates.Count > 0 && string.IsNullOrEmpty(strRight) == false)
                {
                    dates[0] = dates[0] + ":" + strRight;
                }

                this.Channel.Timeout = new TimeSpan(0, 1, 0);   // 一分钟

                this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

                using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
                {
                    connection.Open();

                    ProgressEstimate estimate = new ProgressEstimate();

                    OperLogLoader loader = new OperLogLoader();
                    loader.Channel = this.Channel;
                    loader.Stop = this.Progress;
                    // loader.owner = this;
                    loader.estimate = estimate;
                    loader.FileNames = dates;
                    loader.nLevel = 2;  // this.MainForm.OperLogLevel;
                    loader.AutoCache = false;
                    loader.CacheDir = "";

                    loader.Prompt -= new MessagePromptEventHandler(loader_Prompt);
                    loader.Prompt += new MessagePromptEventHandler(loader_Prompt);

                    // List<OperLogLine> lines = new List<OperLogLine>();
                    MultiBuffer buffer = new MultiBuffer();
                    buffer.Initial();
                    OperLogLineBase.MainForm = this.MainForm;

                    int nRecCount = 0;

                    string strLastItemDate = "";
                    long lLastItemIndex = -1;
                    foreach (OperLogItem item in loader)
                    {
                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                            return 0;
                        }

                        if (stop != null)
                            stop.SetMessage("正在同步 " + item.Date + " " + item.Index.ToString() + " " + estimate.Text + "...");

                        if (string.IsNullOrEmpty(item.Xml) == true)
                            goto CONTINUE;


                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(item.Xml);
                        }
                        catch (Exception ex)
                        {
                            strError = "日志记录 " + item.Date + " " + item.Index.ToString() + " XML 装入 DOM 的时候发生错误: " + ex.Message;
                            DialogResult result = MessageBox.Show(this,
strError + "\r\n\r\n是否跳过此条记录继续处理?",
"ReportForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                return -1;

                            // 记入日志，继续处理
                            this.GetErrorInfoForm().WriteHtml(strError + "\r\n");
                            continue;
                        }

                        string strOperation = DomUtil.GetElementText(dom.DocumentElement, "operation");
#if NO
                        if (strOperation == "borrow" || strOperation == "return")
                        {
                            OperLogLine line = null;
                            nRet = OperLogLine.Xml2Line(dom, item.Date, item.Index,
                                out line,
                                out strError);
                            if (nRet == -1)
                                return -1;
                            lines.Add(line);

                            if (lines.Count >= INSERT_BATCH)
                            {
                                // 写入 operlog 表一次
                                nRet = OperLogLine.AppendOperLogLines(
                                    connection,
                                    lines,
                                    true,
                                    out strError);
                                if (nRet == -1)
                                    return -1;
                                lines.Clear();

                                // 记忆
                                strLastDate = item.Date;
                                last_index = item.Index + 1;
                            }
                        }
#endif
                        if (strOperation == "setUser")
                        {
                            bUserChanged = true;
                            goto CONTINUE;
                        }
                        else
                        {
                            // 在内存中增加一行，关于 operlogXXX 表的信息
                            nRet = buffer.AddLine(
        strOperation,
        dom,
        item.Date,
        item.Index,
        out strError);
                            if (nRet == -1)
                                return -1;
                            bool bForce = false;
                            if (nRecCount >= 4000)
                                bForce = true;
                            nRet = buffer.WriteToDb(connection,
                                true,
                                bForce,
                                out strError);
                            if (bForce == true)
                            {
                                // 记忆
                                strLastDate = item.Date;
                                last_index = item.Index + 1;
                                nRecCount = 0;
                            }
                            nRecCount++;
                        }

                        // 将一条日志记录中的动作兑现到 item reader biblio class_ 表
                        // return:
                        //      -1  出错
                        //      0   中断
                        //      1   完成
                        nRet = ProcessLogRecord(
                            connection,
                            item,
                            dom,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "同步 " + item.Date + " " + item.Index.ToString() + " 时出错: " + strError;

                            // TODO: 最好有个冻结按钮
                            DialogResult result = AutoCloseMessageBox.Show(this, strError + "\r\n\r\n(点右上角关闭按钮可以中断批处理)", 5000);
                            if (result != System.Windows.Forms.DialogResult.OK)
                                return -1;  // TODO: 缓存中没有兑现的怎么办?

                            // 记入日志，继续处理
                            this.GetErrorInfoForm().WriteHtml(strError + "\r\n");
                        }

                        // lProcessCount++;

                    CONTINUE:
                        // 便于循环外获得这些值
                        strLastItemDate = item.Date;
                        lLastItemIndex = item.Index + 1;

                        // index = 0;  // 第一个日志文件后面的，都从头开始了

                    }

                    // 缓存中尚未最后兑现的部分
                    nRet = FlushUpdate(
                        connection,
                        out strError);
                    if (nRet == -1)
                        return -1;

#if NO
                    // 最后一批
                    if (lines.Count > 0)
                    {
                        // 写入 operlog 表一次
                        nRet = OperLogLine.AppendOperLogLines(
                            connection,
                            lines,
                            true,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        lines.Clear();

                        // 记忆
                        strLastDate = strLastItemDate;
                        last_index = lLastItemIndex;
                    }
#endif
                    // 最后一批
                    nRet = buffer.WriteToDb(connection,
    true,
    true,   // false,
    out strError);
                    if (nRet == -1)
                        return -1;

                    // 记忆
                    strLastDate = strLastItemDate;
                    last_index = lLastItemIndex;
                }

                if (bUserChanged == true)
                {
                    nRet = DoCreateUserTable(out strError);
                    if (nRet == -1)
                        return -1;
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "ReportForm DoReplication() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }
        }

        int FlushUpdate(
            SQLiteConnection connection,
            out string strError)
        {
            strError = "";

            int nRet = CommitUpdateBiblios(
    connection,
    out strError);
            if (nRet == -1)
            {
                strError = "FlushUpdate() 中 CommitUpdateBiblios() 出错: " + strError;
                return -1;
            }

            nRet = CommitDeleteBiblios(
connection,
out strError);
            if (nRet == -1)
            {
                strError = "FlushUpdate() 中 CommitDeleteBiblios() 出错: " + strError;
                return -1;
            }

            nRet = CommitUpdateItems(
connection,
out strError);
            if (nRet == -1)
            {
                strError = "FlushUpdate() 中 CommitUpdateItems() 出错: " + strError;
                return -1;
            }
            nRet = CommitUpdateReaders(
connection,
out strError);
            if (nRet == -1)
            {
                strError = "FlushUpdate() 中 CommitUpdateReaders() 出错: " + strError;
                return -1;
            }

            return 0;
        }

        #region 日志同步

        // 将一条日志记录中的动作兑现到 item reader biblio class_ 表
        // return:
        //      -1  出错
        //      0   中断
        //      1   完成
        int ProcessLogRecord(
            SQLiteConnection connection,
            // DigitalPlatform.CirculationClient.localhost.OperLogInfo info,
            OperLogItem info,
            XmlDocument dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            if (string.IsNullOrEmpty(info.Xml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(info.Xml);
            }
            catch (Exception ex)
            {
                strError = "日志记录装载到DOM时出错: " + ex.Message;
                return -1;
            }
#endif

            string strOperation = DomUtil.GetElementText(dom.DocumentElement,
    "operation");
            if (strOperation == "setBiblioInfo")
            {
                nRet = this.TraceSetBiblioInfo(
                    connection,
                    dom,
                    out strError);
            }
            else if (strOperation == "setEntity")
            {
                nRet = this.TraceSetEntity(
                    connection,
                    dom,
                    out strError);
            }
            else if (strOperation == "setReaderInfo")
            {
                nRet = this.TraceSetReaderInfo(
                    connection,
                    dom,
                    out strError);
            }
            else if (strOperation == "borrow")
            {
                nRet = this.TraceBorrow(
                    connection,
                    dom,
                    out strError);
            }
            else if (strOperation == "return")
            {
                nRet = this.TraceReturn(
                    connection,
                    dom,
                    out strError);
            } 

            if (nRet == -1)
            {
                string strAction = DomUtil.GetElementText(dom.DocumentElement,
                        "action");
                strError = "operation=" + strOperation + ";action=" + strAction + ": " + strError;
                return -1;
            }

            return 1;
        }

        int DeleteBiblioRecord(
            SQLiteConnection connection,
            string strBiblioRecPath,
            bool bDeleteBiblio,
            bool bDeleteSubrecord,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            // 把前面积累的关于修改书目记录的请求全部兑现了
            if (this._updateBiblios.Count > 0)
            {
                nRet = CommitUpdateBiblios(
                    connection,
                    out strError);
                if (nRet == -1)
                {
                    strError = "DeleteBiblioRecord() 中 CommitUpdateBiblios() 出错: " + strError;
                    return -1;
                }
            }

            if (bDeleteBiblio == false
    && bDeleteSubrecord == false)
            {
                return 0;
            }

            // 把请求放入队列
            UpdateBiblio update = new UpdateBiblio();
            update.BiblioRecPath = strBiblioRecPath;
            update.DeleteBiblio = bDeleteBiblio;
            update.DeleteSubrecord = bDeleteSubrecord;
            // update.BiblioXml = strBiblioXml;
            _deleteBiblios.Add(update);

            if (this._updateBiblios.Count >= 100)
            {
                nRet = CommitDeleteBiblios(
        connection,
        out strError);
                if (nRet == -1)
                {
                    strError = "UpdateBiblioRecord() 中 CommitUpdateBiblios() 出错: " + strError;
                    return -1;
                }
            }

            return 1;
        }

        int CommitDeleteBiblios(
    SQLiteConnection connection,
    out string strError)
        {
            strError = "";
            //int nRet = 0;

            if (this._deleteBiblios.Count == 0)
                return 0;

#if NO
            List<BiblioDbFromInfo> styles = null;
            // 获得所有分类号检索途径 style
            nRet = GetClassFromStyles(out styles,
                out strError);
            if (nRet == -1)
                return -1;
#endif
            Debug.Assert(this._classFromStyles != null, "");

            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {

                StringBuilder text = new StringBuilder(4096);
                int i = 0;
                foreach (UpdateBiblio update in this._deleteBiblios)
                {
                    string strBiblioRecPath = update.BiblioRecPath;
                    Debug.Assert(string.IsNullOrEmpty(strBiblioRecPath) == false, "");
                    if (update.DeleteBiblio)
                    {
                        foreach (BiblioDbFromInfo style in this._classFromStyles)
                        {
                            // 删除 class 记录
                            text.Append("delete from class_" + style.Style + " where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");
                        }
                    }
                    SQLiteUtil.SetParameter(command,
    "@bibliorecpath" + i.ToString(),
    strBiblioRecPath);

                    if (update.DeleteBiblio)
                    {
                        // 删除 biblio 记录
                        text.Append("delete from biblio where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");
                    }

                    if (update.DeleteSubrecord)
                    {
                        // 删除 item 记录
                        text.Append("delete from item where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");

#if NO
                    // 删除 order 记录
                    text.Append("delete from order where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");

                    // 删除 issue 记录
                    text.Append("delete from issue where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");

                    // 删除 comment 记录
                    text.Append("delete from comment where bibliorecpath = @bibliorecpath" + i.ToString() + " ;");
#endif
                    }

                    i++;
                }

                if (text.Length > 0)
                {
                    IDbTransaction trans = connection.BeginTransaction();
                    try
                    {
                        command.CommandText = text.ToString();
                        int nCount = command.ExecuteNonQuery();
                        if (trans != null)
                        {
                            trans.Commit();
                            trans = null;
                        }
                    }
                    finally
                    {
                        if (trans != null)
                            trans.Rollback();
                    }
                }
            }

            this._deleteBiblios.Clear();

            return 0;
        }

        List<UpdateBiblio> _deleteBiblios = new List<UpdateBiblio>();

        // 应当是连续的 Update 操作，才能缓存。中间有 Delete 操作，就要把前面的缓存队先后，立即执行 Delete
        class UpdateBiblio
        {
            public string BiblioRecPath = "";
            public string BiblioXml = "";

            // 是否删除书目记录部分
            public bool DeleteBiblio = true;
            // 是否删除下级记录
            public bool DeleteSubrecord = true;

            public string Summary = ""; // [out]
            public string KeysXml = ""; // [out]
        }

        List<UpdateBiblio> _updateBiblios = new List<UpdateBiblio>();

        // 更新 biblio 表 和 class_xxx 表中的行
        int UpdateBiblioRecord(
            SQLiteConnection connection,
            string strBiblioRecPath,
            string strBiblioXml,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strDbName = Global.GetDbName(strBiblioRecPath);
            if (this.MainForm.IsBiblioDbName(strDbName) == false)
                return 0;

            // 把前面积累的关于删除书目记录的请求全部兑现了
            if (this._deleteBiblios.Count > 0)
            {
                nRet = CommitDeleteBiblios(
                    connection,
                    out strError);
                if (nRet == -1)
                {
                    strError = "UpdateBiblioRecord() 中 CommitDeleteBiblios() 出错: " + strError;
                    return -1;
                }
            }

            // 把请求放入队列
            UpdateBiblio update = new UpdateBiblio();
            update.BiblioRecPath = strBiblioRecPath;
            update.DeleteBiblio = false;    //  没有用到
            update.DeleteSubrecord = false; // 没有用到
            // update.BiblioXml = strBiblioXml;
            _updateBiblios.Add(update);

            if (this._updateBiblios.Count >= 100)
            {
                nRet = CommitUpdateBiblios(
        connection,
        out strError);
                if (nRet == -1)
                {
                    strError = "UpdateBiblioRecord() 中 CommitUpdateBiblios() 出错: " + strError;
                    return -1;
                }
            }
            return 0;
        }

        int CommitUpdateBiblios(
            SQLiteConnection connection,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this._updateBiblios.Count == 0)
                return 0;

            List<UpdateBiblio> temp_updates = new List<UpdateBiblio>();
            List<string> recpaths = new List<string>();
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (UpdateBiblio update in _updateBiblios)
            {
                temp_updates.Add(update);

                recpaths.Add(update.BiblioRecPath);
#if NO
                if (text.Length > 0)
                    text.Append("<!-->");
                text.Append(update.BiblioXml);

                if (text.Length > 500 * 1024
                    || (text.Length > 0 && i == _updateBiblios.Count - 1))
                {
                    // 调用一次 API 获得检索点和书目摘要
                    string[] formats = new string[2];
                    formats[0] = "keys";
                    formats[1] = "summary";

                    string[] results = null;
                    byte[] timestamp = null;
                REDO:
                    long lRet = Channel.GetBiblioInfos(
                        Progress,
                        "@path-list:" + StringUtil.MakePathList(recpaths),
                        text.ToString(),
                        formats,
                        out results,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
        "获取书目记录信息 (" + StringUtil.MakePathList(recpaths) + ") 的操作发生错误： " + strError + "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试; 取消: 停止操作)",
        "ReportForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.OK)
                            goto REDO;
                        if (result == DialogResult.Cancel)
                            return -1;
                    }

                    if (results == null)
                    {
                        strError = "results == null ";
                        return -1;
                    }
                    if (results.Length < temp_updates.Count * 2)
                    {
                        strError = "results.Length " + results.Length + " 不正确，应该为 " + temp_updates.Count * 2;
                        return -1;
                    }

                    for (int j = 0; j < temp_updates.Count; j++)
                    {
                        UpdateBiblio temp_update = temp_updates[j];
                        temp_update.KeysXml = results[j * 2];
                        temp_update.Summary = results[j * 2 + 1];
                    }

                    temp_updates.Clear();
                    recpaths.Clear();
                    text.Clear();
                }
#endif

                if (recpaths.Count >= 100
    || (recpaths.Count > 0 && i >= _updateBiblios.Count - 1))
                {
                    // 调用一次 API 获得检索点和书目摘要
                    string[] formats = new string[2];
                    formats[0] = "keys";
                    formats[1] = "summary";

                    string[] results = null;
                    byte[] timestamp = null;
                REDO:
                    long lRet = Channel.GetBiblioInfos(
                        Progress,
                        "@path-list:" + StringUtil.MakePathList(recpaths),
                        text.ToString(),
                        formats,
                        out results,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
        "获取书目记录信息 (" + StringUtil.MakePathList(recpaths) + ") 的操作发生错误： " + strError + "\r\n\r\n是否重试操作?\r\n\r\n(是: 重试; 取消: 停止操作)",
        "ReportForm",
        MessageBoxButtons.OKCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.OK)
                            goto REDO;
                        if (result == DialogResult.Cancel)
                            return -1;
                    }

                    if (lRet > 0)
                    {
                        if (results == null)
                        {
                            strError = "results == null ";
                            return -1;
                        }
                        if (results.Length < temp_updates.Count * 2)
                        {
                            strError = "results.Length " + results.Length + " 不正确，应该为 " + temp_updates.Count * 2;
                            return -1;
                        }

                        for (int j = 0; j < temp_updates.Count; j++)
                        {
                            UpdateBiblio temp_update = temp_updates[j];
                            temp_update.KeysXml = results[j * 2];
                            temp_update.Summary = results[j * 2 + 1];
                        }
                    }

                    temp_updates.Clear();
                    recpaths.Clear();
                    text.Clear();
                }

                i++;
            }

            Debug.Assert(text.Length == 0, "");
            Debug.Assert(recpaths.Count == 0, "");

            // 更新 SQL 表 
            nRet = CommitUpdateBiblioRecord(
                connection,
                this._updateBiblios,
                out strError);
            if (nRet == -1)
                return -1;

            this._updateBiblios.Clear();

            return 1;
        }

        // 更新 SQL 表 
        int CommitUpdateBiblioRecord(
            SQLiteConnection connection,
            List<UpdateBiblio> updates,
            out string strError)
        {
            strError = "";

            StringBuilder command_text = new StringBuilder(4096);
            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {

                int i = 0;
                foreach (UpdateBiblio update in updates)
                {
                    if (string.IsNullOrEmpty(update.Summary) == true
                        && string.IsNullOrEmpty(update.BiblioRecPath) == true)
                        continue;

                    bool bBiblioRecPathParamSetted = false;
                    if (string.IsNullOrEmpty(update.Summary) == false)
                    {
                        // 把书目摘要写入 biblio 表
                        command_text.Append("insert or replace into biblio values (@bibliorecpath" + i + ", @summary" + i + ") ;");

                        SQLiteUtil.SetParameter(command,
            "@bibliorecpath" + i,
            update.BiblioRecPath);
                        bBiblioRecPathParamSetted = true;

                        SQLiteUtil.SetParameter(command,
         "@summary" + i,
         update.Summary);
                    }

                    // *** 取得分类号 keys

                    if (_classFromStyles.Count == 0)
                        goto CONTINUE;

                    if (string.IsNullOrEmpty(update.KeysXml) == true)
                        goto CONTINUE;

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(update.KeysXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "update.KeysXml XML 装入 DOM 时出错: " + ex.Message;
                        return -1;
                    }

                    int j = 0;
                    foreach (BiblioDbFromInfo style in _classFromStyles)
                    {
                        XmlNodeList nodes = dom.DocumentElement.SelectNodes("k[@f='" + style.Caption + "']");
                        List<string> keys = new List<string>();
                        foreach (XmlNode node in nodes)
                        {
                            keys.Add(DomUtil.GetAttr(node, "k"));
                        }

                        command_text.Append("delete from class_" + style.Style + " where bibliorecpath = @bibliorecpath" + i +" ;");

                        if (bBiblioRecPathParamSetted == false)
                        {
                            SQLiteUtil.SetParameter(command,
"@bibliorecpath" + i,
update.BiblioRecPath);
                            bBiblioRecPathParamSetted = true;
                        }
                        /*
                            SQLiteUtil.SetParameter(command,
                "@bibliorecpath",
                update.BiblioRecPath);
                         * */

                        foreach (string key in keys)
                        {
                            // 把分类号写入分类号表
                            command_text.Append("insert into class_" + style.Style + " values (@bibliorecpath"+i+", @class_" + i + "_" + j + ") ;");

                            SQLiteUtil.SetParameter(command,
             "@class_" + i + "_" + j,
             key);
                            j++;
                        }
                    }

                CONTINUE:
                    i++;
                }

                IDbTransaction trans = connection.BeginTransaction();
                try
                {
                    command.CommandText = command_text.ToString();
                    int nCount = command.ExecuteNonQuery();
                    if (trans != null)
                    {
                        trans.Commit();
                        trans = null;
                    }
                }
                catch (Exception ex)
                {
                    strError = "更新 biblio 表时出错.\r\n"
                        + ex.Message + "\r\n"
                        + "SQL 命令:\r\n"
                        + command_text.ToString();
                    return -1;
                }
                finally
                {
                    if (trans != null)
                        trans.Rollback();
                }
            }

            return 0;
        }

        // Return() API 恢复动作
        /*
<root>
  <operation>return</operation> 操作类型
  <itemBarcode>0000001</itemBarcode> 册条码号
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:45 GMT</operTime> 操作时间
  <overdues>...</overdues> 超期信息 通常内容为一个字符串，为一个<overdue>元素XML文本片断
  
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
  
</root>

         * */
        public int TraceReturn(
SQLiteConnection connection,
XmlDocument domLog,
out string strError)
        {
            strError = "";

            //long lRet = 0;
            int nRet = 0;

            string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
    "readerBarcode");
            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "<readerBarcode>元素值为空";
                return -1;
            }

            // 读入册记录
            string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                "confirmItemRecPath");
            string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                "itemBarcode");
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "<strItemBarcode>元素值为空";
                return -1;
            }

            ItemLine line = new ItemLine();

            // line.Full = false;
            line.Level = 1;
            line.ItemBarcode = strItemBarcode;
            line.Borrower = "";
            line.BorrowTime = "";
            line.BorrowPeriod = "";
            line.ReturningTime = "";
            line.ItemRecPath = strConfirmItemRecPath;

            this._updateItems.Add(line);
            if (this._updateItems.Count >= 100)
            {
                nRet = CommitUpdateItems(
                    connection,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }

        // Borrow() API 恢复动作
        /*
<root>
  <operation>borrow</operation> 操作类型
  <readerBarcode>R0000002</readerBarcode> 读者证条码号
  <itemBarcode>0000001</itemBarcode>  册条码号
  <borrowDate>Fri, 08 Dec 2006 04:17:31 GMT</borrowDate> 借阅日期
  <borrowPeriod>30day</borrowPeriod> 借阅期限
  <no>0</no> 续借次数。0为首次普通借阅，1开始为续借
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 04:17:31 GMT</operTime> 操作时间
  <confirmItemRecPath>...</confirmItemRecPath> 辅助判断用的册记录路径
  
  <readerRecord recPath='...'>...</readerRecord>	最新读者记录
  <itemRecord recPath='...'>...</itemRecord>	最新册记录
</root>
         * */
        public int TraceBorrow(
SQLiteConnection connection,
XmlDocument domLog,
out string strError)
        {
            strError = "";

            //long lRet = 0;
            int nRet = 0;

            string strReaderBarcode = DomUtil.GetElementText(domLog.DocumentElement,
    "readerBarcode");
            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                strError = "<readerBarcode>元素值为空";
                return -1;
            }

            // 读入册记录
            string strConfirmItemRecPath = DomUtil.GetElementText(domLog.DocumentElement,
                "confirmItemRecPath");
            string strItemBarcode = DomUtil.GetElementText(domLog.DocumentElement,
                "itemBarcode");
            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "<strItemBarcode>元素值为空";
                return -1;
            }

            string strBorrowDate = SQLiteUtil.GetLocalTime(DomUtil.GetElementText(domLog.DocumentElement,
                "borrowDate"));
            string strBorrowPeriod = DomUtil.GetElementText(domLog.DocumentElement,
                "borrowPeriod");
            //string strReturningDate = ItemLine.GetLocalTime(DomUtil.GetElementText(domLog.DocumentElement,
            //    "returningDate"));

            string strReturningTime = "";

            if (string.IsNullOrEmpty(strBorrowDate) == false)
            {
                // parameters:
                //      strBorrowTime   借阅起点时间。u 格式
                //      strReturningTime    返回应还时间。 u 格式
                nRet = AmerceOperLogLine.BuildReturingTimeString(strBorrowDate,
    strBorrowPeriod,
    out strReturningTime,
    out strError);
                if (nRet == -1)
                {
                    strReturningTime = "";
                }
            }
            else
                strReturningTime = "";


            ItemLine line = new ItemLine();

            // line.Full = false;
            line.Level = 1;
            line.ItemBarcode = strItemBarcode;
            line.Borrower = strReaderBarcode;
            line.BorrowTime = strBorrowDate;
            line.BorrowPeriod = strBorrowPeriod;
            line.ReturningTime = strReturningTime;
            line.ItemRecPath = strConfirmItemRecPath;

            this._updateItems.Add(line);
            if (this._updateItems.Count >= 100)
            {
                nRet = CommitUpdateItems(
                    connection,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }

        // SetReaderInfo() API 恢复动作
        /*
<root>
	<operation>setReaderInfo</operation> 操作类型
	<action>...</action> 具体动作。有new change delete move 4种
	<record recPath='...'>...</record> 新记录
    <oldRecord recPath='...'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
    <changedEntityRecord itemBarcode='...' recPath='...' oldBorrower='...' newBorrower='...' /> 若干个元素。表示连带发生修改的册记录
	<operator>test</operator> 操作者
	<operTime>Fri, 08 Dec 2006 09:01:38 GMT</operTime> 操作时间
</root>

注: new 的时候只有<record>元素，delete的时候只有<oldRecord>元素，change的时候两者都有

         * */
        public int TraceSetReaderInfo(
SQLiteConnection connection,
XmlDocument domLog,
out string strError)
        {
            strError = "";

            //long lRet = 0;
            int nRet = 0;

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            if (strAction == "new"
                || strAction == "change"
                || strAction == "move")
            {
                XmlNode node = null;
                string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "record",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<record>元素";
                    return -1;
                }
                string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                string strOldRecord = "";
                string strOldRecPath = "";
                if (strAction == "move")
                {
                    strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }

                    strOldRecPath = DomUtil.GetAttr(node, "recPath");
                    if (string.IsNullOrEmpty(strOldRecPath) == true)
                    {
                        strError = "日志记录中<oldRecord>元素内缺recPath属性值";
                        return -1;
                    }

                    // 如果移动过程中没有修改，则要用旧的记录内容写入目标
                    if (string.IsNullOrEmpty(strRecord) == true)
                        strRecord = strOldRecord;
                }

                // 在 SQL reader 库中写入一条读者记录
                nRet = WriteReaderRecord(connection,
                    strNewRecPath,
                    strRecord,
                    out strError);
                if (nRet == -1)
                {
                    strError = "写入读者记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                    return -1;
                }

                // 2015/9/11
                XmlNodeList nodes = domLog.DocumentElement.SelectNodes("changedEntityRecord");
                foreach (XmlElement item in nodes)
                {
                    string strItemBarcode = item.GetAttribute("itemBarcode");
                    string strItemRecPath = item.GetAttribute("recPath");
                    string strOldReaderBarcode = item.GetAttribute("oldBorrower");
                    string strNewReaderBarcode = item.GetAttribute("newBorrower");

                    nRet = TraceChangeBorrower(
                        connection,
                        strItemBarcode,
                        strItemRecPath,
                        strNewReaderBarcode,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "修改册记录 '" + strItemRecPath + "' 的 borrower 字段时发生错误: " + strError;
                        return -1;
                    }
                }

                if (strAction == "move")
                {
                    // 兑现缓存
                    nRet = CommitUpdateReaders(
    connection,
    out strError);
                    if (nRet == -1)
                        return -1;

                    // 删除读者记录
                    nRet = ReaderLine.DeleteReaderLine(
                        connection,
                        strOldRecPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }
            else if (strAction == "delete")
            {
                XmlNode node = null;
                string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "oldRecord",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                // 兑现缓存
                nRet = CommitUpdateReaders(
connection,
out strError);
                if (nRet == -1)
                    return -1;

                // 删除读者记录
                nRet = ReaderLine.DeleteReaderLine(
                    connection,
                    strRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

        public int TraceChangeBorrower(
            SQLiteConnection connection,
            string strItemBarcode,
            string strItemRecPath,
            string strNewBorrower,
            out string strError)
        {
            strError = "";

            //long lRet = 0;
            int nRet = 0;

            if (String.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "strItemBarcode 参数不应为空";
                return -1;
            }

            ItemLine line = new ItemLine();

            // line.Full = false;
            line.Level = 2;
            line.ItemBarcode = strItemBarcode;
            line.Borrower = strNewBorrower;
            line.ItemRecPath = strItemRecPath;

            this._updateItems.Add(line);
            if (this._updateItems.Count >= 100)
            {
                nRet = CommitUpdateItems(
                    connection,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }

        // SetEntities() API 恢复动作
        /* 日志记录格式
<root>
  <operation>setEntity</operation> 操作类型
  <action>new</action> 具体动作。有new change delete 3种
  <style>...</style> 风格。有force nocheckdup noeventlog 3种
  <record recPath='中文图书实体/3'><root><parent>2</parent><barcode>0000003</barcode><state>状态2</state><location>阅览室</location><price></price><bookType>教学参考</bookType><registerNo></registerNo><comment>test</comment><mergeComment></mergeComment><batchNo>111</batchNo><borrower></borrower><borrowDate></borrowDate><borrowPeriod></borrowPeriod></root></record> 记录体
  <oldRecord recPath='中文图书实体/3'>...</oldRecord> 被覆盖或者删除的记录 动作为change和delete时具备此元素
  <operator>test</operator> 操作者
  <operTime>Fri, 08 Dec 2006 08:41:46 GMT</operTime> 操作时间
</root>

注：1) 当<action>为delete时，没有<record>元素。为new时，没有<oldRecord>元素。
	2) <record>中的内容, 涉及到流通的<borrower><borrowDate><borrowPeriod>等, 在日志恢复阶段, 都应当无效, 这几个内容应当从当前位置库中记录获取, 和<record>中其他内容合并后, 再写入数据库
	3) 一次SetEntities()API调用, 可能创建多条日志记录。
         
         * */
        public int TraceSetEntity(
    SQLiteConnection connection,
    XmlDocument domLog,
    out string strError)
        {
            strError = "";

            //long lRet = 0;
            int nRet = 0;

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            if (strAction == "new"
    || strAction == "change"
    || strAction == "move")
            {
                XmlNode node = null;
                string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "record",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<record>元素";
                    return -1;
                }

                string strNewRecPath = DomUtil.GetAttr(node, "recPath");

                // 
                string strOldRecord = "";
                string strOldRecPath = "";
                if (strAction == "move")
                {
                    strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                        "oldRecord",
                        out node);
                    if (node == null)
                    {
                        strError = "日志记录中缺<oldRecord>元素";
                        return -1;
                    }

                    strOldRecPath = DomUtil.GetAttr(node, "recPath");
                }

                string strCreateOperTime = "";
                
                if (strAction == "new")
                    strCreateOperTime = DomUtil.GetElementText(domLog.DocumentElement, "operTime");

                // 在 SQL item 库中写入一条册记录
                nRet = WriteItemRecord(connection,
                    strNewRecPath,
                    strRecord,
                    strCreateOperTime,
                    out strError);
                if (nRet == -1)
                {
                    strError = "写入册记录 '" + strNewRecPath + "' 时发生错误: " + strError;
                    return -1;
                }

                if (strAction == "move")
                {
                    // 兑现前面的缓存
                    nRet = CommitUpdateItems(
            connection,
            out strError);
                    if (nRet == -1)
                        return -1;

                    // 删除册记录
                    nRet = ItemLine.DeleteItemLine(
                        connection,
                        strOldRecPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

            }
            else if (strAction == "delete")
            {
                XmlNode node = null;
                string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "oldRecord",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                // 兑现前面的缓存
                nRet = CommitUpdateItems(
        connection,
        out strError);
                if (nRet == -1)
                    return -1;

                // 删除册记录
                nRet = ItemLine.DeleteItemLine(
                    connection,
                    strRecPath,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                strError = "无法识别的<action>内容 '" + strAction + "'";
                return -1;
            }

            return 0;
        }

        // SetBiblioInfo() API 或 CopyBiblioInfo() API 的恢复动作
        // 函数内，使用return -1;还是goto ERROR1; 要看错误发生的时候，是否还有价值继续探索SnapShot重试。如果是，就用后者。
        /*
<root>
  <operation>setBiblioInfo</operation> 
  <action>...</action> 具体动作 有 new/change/delete/onlydeletebiblio/onlydeletesubrecord 和 onlycopybiblio/onlymovebiblio/copy/move
  <record recPath='中文图书/3'>...</record> 记录体 动作为new/change/ *move* / *copy* 时具有此元素(即delete时没有此元素)
  <oldRecord recPath='中文图书/3'>...</oldRecord> 被覆盖、删除或者移动的记录 动作为change/ *delete* / *move* / *copy* 时具备此元素
  <deletedEntityRecords> 被删除的实体记录(容器)。只有当<action>为delete时才有这个元素。
	  <record recPath='中文图书实体/100'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。
	  ...
  </deletedEntityRecords>
  <copyEntityRecords> 被复制的实体记录(容器)。只有当<action>为*copy*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
	  ...
  </copyEntityRecords>
  <moveEntityRecords> 被移动的实体记录(容器)。只有当<action>为*move*时才有这个元素。
	  <record recPath='中文图书实体/100' targetRecPath='中文图书实体/110'>...</record> 这个元素可以重复。注意元素内文本内容目前为空。recPath属性为源记录路径，targetRecPath为目标记录路径
	  ...
  </moveEntityRecords>
  <copyOrderRecords /> <moveOrderRecords />
  <copyIssueRecords /> <moveIssueRecords />
  <copyCommentRecords /> <moveCommentRecords />
  <operator>test</operator> 
  <operTime>Fri, 08 Dec 2006 10:12:20 GMT</operTime> 
</root>

逻辑恢复delete操作的时候，检索出全部下属的实体记录删除。
快照恢复的时候，可以根据operlogdom直接删除记录了path的那些实体记录
         * */
        public int TraceSetBiblioInfo(
            SQLiteConnection connection,
            XmlDocument domLog,
            out string strError)
        {
            strError = "";

            //long lRet = 0;
            int nRet = 0;

            string strAction = DomUtil.GetElementText(domLog.DocumentElement,
                "action");

            if (strAction == "new" || strAction == "change")
            {
                XmlNode node = null;
                string strRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "record",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<record>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                if (string.IsNullOrEmpty(strRecPath) == true)
                    return 0;   // 轮空

                string strTimestamp = DomUtil.GetAttr(node, "timestamp");

                // 把书目摘要写入 biblio 表

                // 把分类号写入若干分类号表

                nRet = UpdateBiblioRecord(
        connection,
        strRecPath,
        strRecord,
        out strError);
                if (nRet == -1)
                    return -1;

                // 在 biblio 表中写入 summary 为空或者特殊标志的记录，最后按照标记全部重新获得?
            }
            else if (strAction == "onlymovebiblio"
                || strAction == "onlycopybiblio"
                || strAction == "move"
                || strAction == "copy")
            {
                XmlNode node = null;
                string strTargetRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "record",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<record>元素";
                    return -1;
                }
                string strTargetRecPath = DomUtil.GetAttr(node, "recPath");

                if (string.IsNullOrEmpty(strTargetRecPath) == true)
                    return 0;

                string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "oldRecord",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strOldRecPath = DomUtil.GetAttr(node, "recPath");

                string strMergeStyle = DomUtil.GetElementText(domLog.DocumentElement,
    "mergeStyle");

#if NO
                    bool bSourceExist = true;
                    // 观察源记录是否存在
                    if (string.IsNullOrEmpty(strOldRecPath) == true)
                        bSourceExist = false;
                    else
                    {
                    }

                    if (bSourceExist == true
                        && string.IsNullOrEmpty(strTargetRecPath) == false)
                    {
                        // 注: 实际上是从本地复制到本地
                        // 复制书目记录
                        lRet = channel.DoCopyRecord(strOldRecPath,
                            strTargetRecPath,
                            strAction == "onlymovebiblio" ? true : false,   // bDeleteSourceRecord
                            out output_timestamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            strError = "DoCopyRecord() error :" + strError;
                            goto ERROR1;
                        }
                    }


                    // 准备需要写入目标位置的记录
                    if (bSourceExist == false)
                    {
                        if (String.IsNullOrEmpty(strTargetRecord) == true)
                        {
                            if (String.IsNullOrEmpty(strOldRecord) == true)
                            {
                                strError = "源记录 '" + strOldRecPath + "' 不存在，并且<record>元素无文本内容，这时<oldRecord>元素也无文本内容，无法获得要写入的记录内容";
                                return -1;
                            }

                            strTargetRecord = strOldRecord;
                        }
                    }
#endif
                // 如果目标记录没有记载，就尽量用源记录
                if (String.IsNullOrEmpty(strTargetRecord) == true)
                {
                    if (String.IsNullOrEmpty(strOldRecord) == true)
                    {
                        strError = "源记录 '" + strOldRecPath + "' 不存在，并且<record>元素无文本内容，这时<oldRecord>元素也无文本内容，无法获得要写入的记录内容";
                        return -1;
                    }

                    strTargetRecord = strOldRecord;
                }

                // 如果有“新记录”内容
                if (string.IsNullOrEmpty(strTargetRecPath) == false
                    && String.IsNullOrEmpty(strTargetRecord) == false)
                {
                    // 写入新的书目记录
                    nRet = UpdateBiblioRecord(
connection,
strTargetRecPath,
strTargetRecord,
out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 复制或者移动下级子记录
                if (strAction == "move"
                || strAction == "copy")
                {
                    nRet = CopySubRecords(
                        connection,
                        domLog,
                        strAction,
                        // string strSourceBiblioRecPath,
                        strTargetRecPath,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                if (strAction == "move" || strAction == "onlymovebiblio")
                {
                    // 删除旧的书目记录
                    nRet = DeleteBiblioRecord(
                        connection,
                        strOldRecPath,
                        true,
                        true,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }
            else if (strAction == "delete"
                || strAction == "onlydeletebiblio"
                || strAction == "onlydeletesubrecord")
            {
                XmlNode node = null;
                string strOldRecord = DomUtil.GetElementText(domLog.DocumentElement,
                    "oldRecord",
                    out node);
                if (node == null)
                {
                    strError = "日志记录中缺<oldRecord>元素";
                    return -1;
                }
                string strRecPath = DomUtil.GetAttr(node, "recPath");

                if (string.IsNullOrEmpty(strRecPath) == false)
                {
                    // 删除书目记录
                    nRet = DeleteBiblioRecord(
                        connection,
                        strRecPath,
                        strAction == "delete" || strAction == "onlydeletebiblio" ? true : false,
                        strAction == "delete" || strAction == "onlydeletesubrecord" ? true : false,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
            }

            return 0;
        }

        // TODO: 需要扩展为也能复制 order issue comment 记录
        int CopySubRecords(
            SQLiteConnection connection,
            XmlDocument dom,
            string strAction,
            // string strSourceBiblioRecPath,
            string strTargetBiblioRecPath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (dom == null || dom.DocumentElement == null)
                return 0;

            string strElement = "";
            if (strAction == "move")
                strElement = "moveEntityRecords";
            else if (strAction == "copy")
                strElement = "copyEntityRecords";

            XmlNodeList nodes = dom.DocumentElement.SelectNodes(strElement + "/record");
            if (nodes.Count == 0)
                return 0;

            nRet = CommitUpdateBiblios(
    connection,
    out strError);
            if (nRet == -1)
            {
                strError = "CopySubRecords() 中 CommitUpdateBiblios() 出错: " + strError;
                return -1;
            }
            nRet = CommitDeleteBiblios(
connection,
out strError);
            if (nRet == -1)
            {
                strError = "CopySubRecords() 中 CommitDeleteBiblios() 出错: " + strError;
                return -1;
            }

            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            using (SQLiteCommand command = new SQLiteCommand("",
connection))
            {
                SQLiteUtil.SetParameter(command,
"@t_bibliorecpath",
strTargetBiblioRecPath);

                foreach (XmlNode node in nodes)
                {
                    string strSourceRecPath = DomUtil.GetAttr(node, "recPath");
                    string strTargetRecPath = DomUtil.GetAttr(node, "targetRecPath");

                    if (strAction == "copy")
                    {
                        string strNewBarcode = DomUtil.GetAttr(node, "newBarocde");
                        // TODO: 目标位置实体记录已经存在怎么办 ?
                        // 目标册记录的 barcode 字段要修改为空
                        text.Append("insert or replace into item (itemrecpath, itembarcode, location, accessno, bibliorecpath) ");
                        text.Append("select @t_itemrecpath" + i + " as itemrecpath, @newbarcode" + i + " as itembarcode, location, accessno, @t_bibliorecpath as bibliorecpath from item where itemrecpath = @s_itemrecpath" + i + " ; ");

                        SQLiteUtil.SetParameter(command,
"@newbarcode" + i,
strNewBarcode);
                    }
                    else
                    {
                        // *** 如果目标位置有记录，而源位置没有记录，应该是直接在目标记录上修改

                        // 确保源位置有记录
                        text.Append("insert or ignore into item (itemrecpath, itembarcode, location, accessno, bibliorecpath) ");
                        text.Append("select @s_itemrecpath" + i + " as itemrecpath, itembarcode, location, accessno, @t_bibliorecpath as bibliorecpath from item where itemrecpath = @t_itemrecpath" + i + " ; ");

                        // 如果目标位置已经有记录，先删除
                        text.Append("delete from item where itemrecpath = @t_itemrecpath" + i + " ;");


                        // 等于是修改 item 表的 itemrecpath 字段内容
                        text.Append("update item SET itemrecpath = @t_itemrecpath" + i + " , bibliorecpath = @t_bibliorecpath where itemrecpath=@s_itemrecpath" + i + " ;");
                    }

                    SQLiteUtil.SetParameter(command,
"@t_itemrecpath" + i,
strTargetRecPath);
                    SQLiteUtil.SetParameter(command,
"@s_itemrecpath" + i,
strSourceRecPath);
                    i++;
                }

                IDbTransaction trans = connection.BeginTransaction();
                try
                {
                    command.CommandText = text.ToString();
                    int nCount = command.ExecuteNonQuery();
                    if (trans != null)
                    {
                        trans.Commit();
                        trans = null;
                    }
                }
                finally
                {
                    if (trans != null)
                        trans.Rollback();
                }
            }

            return 0;
        }


        List<ItemLine> _updateItems = new List<ItemLine>();

        // 在 SQL item 库中写入一条册记录
        // parameters:
        //      strLogCreateTime    日志操作记载的创建时间。不是创建动作的其他时间，不要放在这里
        int WriteItemRecord(SQLiteConnection connection,
            string strItemRecPath,
            string strItemXml,
            string strLogCreateTime,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                strError = "XML 装入 DOM 出错: " + ex.Message;
                return -1;
            }

            string strParentID = DomUtil.GetElementText(dom.DocumentElement,
                "parent");
            // 根据 册/订购/期/评注 记录路径和 parentid 构造所从属的书目记录路径
            string strBiblioRecPath = this.MainForm.BuildBiblioRecPath("item",
                strItemRecPath,
                strParentID);
            if (string.IsNullOrEmpty(strBiblioRecPath) == true)
            {
                strError = "根据册记录路径 '" + strItemRecPath + "' 和 parentid '" + strParentID + "' 构造书目记录路径出错";
                return 0;
            }

            ItemLine line = null;
            //  XML 记录变换为 SQL 记录
            int nRet = ItemLine.Xml2Line(dom,
            strItemRecPath,
            strBiblioRecPath,
            strLogCreateTime,
            out line,
            out strError);
            if (nRet == -1)
                return -1;

            this._updateItems.Add(line);

            if (this._updateItems.Count >= 100)
            {
                nRet = CommitUpdateItems(
                    connection,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        int CommitUpdateItems(
            SQLiteConnection connection,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this._updateItems.Count == 0)
                return 0;

            // 插入一批册记录
            nRet = ItemLine.AppendItemLines(
                connection,
                _updateItems,
                true,
                out strError);
            if (nRet == -1)
                return -1;

            this._updateItems.Clear();
            return 0;
        }

        List<ReaderLine> _updateReaders = new List<ReaderLine>();

        // 在 SQL reader 库中写入一条读者记录
        int WriteReaderRecord(SQLiteConnection connection,
            string strReaderRecPath,
            string strReaderXml,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strReaderRecPath) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strReaderXml);
            }
            catch (Exception ex)
            {
                strError = "WriteReaderRecord XML 装入 DOM 出错: " + ex.Message;
                return -1;
            }

            // 根据读者库名，得到馆代码
            string strReaderDbName = Global.GetDbName(strReaderRecPath);

            string strLibraryCode = this.MainForm.GetReaderDbLibraryCode(strReaderDbName);

            ReaderLine line = null;
            //  XML 记录变换为 SQL 记录
            int nRet = ReaderLine.Xml2Line(dom,
                strReaderRecPath,
                strLibraryCode,
                out line,
                out strError);
            if (nRet == -1)
                return -1;

            _updateReaders.Add(line);


            if (this._updateReaders.Count >= 100)
            {
                nRet = CommitUpdateReaders(
                    connection,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            return 0;
        }

        int CommitUpdateReaders(
    SQLiteConnection connection,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this._updateReaders.Count == 0)
                return 0;


            // 插入一批读者记录
            nRet = ReaderLine.AppendReaderLines(
                connection,
                this._updateReaders,
                true,
                out strError);
            if (nRet == -1)
                return -1;

            this._updateReaders.Clear();
            return 0;
        }

        #endregion

        // 获得日志文件中记录的总数
        // parameters:
        //      strDate 日志文件的日期，8 字符
        // return:
        //      -1  出错
        //      0   日志文件不存在，或者记录数为 0
        //      >0  记录数
        long GetOperLogCount(string strDate,
            out string strError)
        {
            strError = "";

            string strXml = "";
            long lAttachmentTotalLength = 0;
            byte[] attachment_data = null;

            long lRecCount = 0;

            // 获得日志文件尺寸
            // return:
            //      -1  error
            //      0   file not found
            //      1   succeed
            //      2   超过范围
            long lRet = this.Channel.GetOperLog(
                null,
                strDate + ".log",
                -1,    // lIndex,
                -1, // lHint,
                "getcount",
                "", // strFilter
                out strXml,
                out lRecCount,
                0,  // lAttachmentFragmentStart,
                0,  // nAttachmentFramengLength,
                out attachment_data,
                out lAttachmentTotalLength,
                out strError);
            if (lRet == 0)
            {
                lRecCount = 0;
                return 0;
            }
            if (lRet != 1)
                return -1;
            Debug.Assert(lRecCount >= 0, "");

            return lRecCount;
        }


        // 执行每日同步任务
        // 从上次记忆的断点位置，开始同步
        private void button_start_dailyReplication_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            string strEndDate = "";
            long lIndex = 0;
            string strState = "";
            // 读入每日同步断点信息
            // return:
            //      -1  出错
            //      0   正常
            //      1   首次创建尚未完成
            nRet = LoadDailyBreakPoint(
                out strEndDate,
                out lIndex,
                out strState,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (strState != "daily")
            {
                strError = "首次创建尚未完成。必须完成后才能进行每日同步";
                goto ERROR1;
            }

            // TODO: 如果 strEndDate 为空，则需要设定为一个较早的时间
            // 还可以提醒，说以前并没有作第一次操作
            // 特殊情况下，因为一个数据库都是空的，无法做第一次操作？需要验证一下

            // 第一次操作完成后，其按钮应该发灰，后面只能做每日创建。另有一个功能可以清除以前的信息，然后第一次的按钮又可以使用了

            string strToday = DateTimeUtil.DateTimeToString8(DateTime.Now);

            string strLastDate = "";
            long last_index = 0;

            try
            {
                // return:
                //      -1  出错
                //      0   中断
                //      1   完成
                nRet = DoReplication(
                    strEndDate + ":" + lIndex.ToString() + "-",
                    strToday,
                    // long index,
                    out strLastDate,
                    out last_index,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

#if NO
                MessageBox.Show(this, "nRet=" + nRet.ToString());

                MessageBox.Show(this, strLastDate);
                MessageBox.Show(this, last_index.ToString());
#endif

                // 如果结束的日期小于今天
                if (nRet == 1   // 正常完成
                    // && string.IsNullOrEmpty(strLastDate) == false && last_index != -1
                    && string.Compare(strLastDate, strToday) < 0)
                {
                    // 把断点设置为今天的开始
                    strLastDate = strToday;
                    last_index = 0;
                }

            }
            finally
            {
                // 写入每日同步断点信息
                if (string.IsNullOrEmpty(strLastDate) == false
                    && last_index != -1)
                {
                    string strError_1 = "";
                    nRet = WriteDailyBreakPoint(
                        strLastDate,
                        last_index,
                        out strError_1);
                    if (nRet == -1)
                        MessageBox.Show(this, strError_1);
                }
            }

            SetStartButtonStates();
            SetDailyReportButtonState();
            MessageBox.Show(this, "处理完成");
            return;
        ERROR1:
            SetStartButtonStates();
            SetDailyReportButtonState();
            MessageBox.Show(this, strError);
        }


        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            // Keys pure_key = (keyData & (~(Keys.Control | Keys.Shift | Keys.Alt)));
            Keys pure_key = (keyData & Keys.KeyCode);
            Debug.WriteLine(pure_key.ToString());

            if (Control.ModifierKeys == Keys.Control
                && pure_key == Keys.Enter)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    SQLiteQuery();
                    return true;
                }
            }

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            // return false;
            return base.ProcessDialogKey(keyData);
        }

        List<int> _resultColumnWidths = new List<int>();

        void SQLiteQuery()
        {
            string strError = "";
            // int nRet = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在查询数据库 ...");
            stop.BeginLoop();

            this.listView_query_results.BeginUpdate();
            this.timer_qu.Start();
            try
            {

                // 保留以前的列宽度
                for (int i = 0; i < this.listView_query_results.Columns.Count; i++)
                {
                    if (_resultColumnWidths.Count <= i)
                        _resultColumnWidths.Add(100);
                    _resultColumnWidths[i] = this.listView_query_results.Columns[i].Width;
                }

                this.listView_query_results.Clear();

                this._connectionString = GetOperlogConnectionString();  //  SQLiteUtil.GetConnectionString(this.MainForm.UserDir, "operlog.bin");

                using (SQLiteConnection connection = new SQLiteConnection(this._connectionString))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(this.textBox_query_command.Text, connection))
                    {
                        try
                        {
                            SQLiteDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
                            try
                            {
                                // 如果记录不存在
                                if (dr == null
                                    || dr.HasRows == false)
                                    return;

                                // 设置列标题
                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    ColumnHeader header = new ColumnHeader();
                                    header.Text = dr.GetName(i);

                                    if (_resultColumnWidths.Count <= i)
                                        _resultColumnWidths.Add(100);

                                    header.Width = this._resultColumnWidths[i]; // 恢复列宽度
                                    this.listView_query_results.Columns.Add(header);
                                }

                                int nCount = 0;
                                // 如果记录已经存在
                                while (dr.Read())
                                {
                                    Application.DoEvents();
                                    if (stop != null && stop.State != 0)
                                    {
                                        strError = "用户中断...";
                                        goto ERROR1;
                                    }

                                    ListViewItem item = new ListViewItem();
                                    for (int i = 0; i < dr.FieldCount; i++)
                                    {
                                        ListViewUtil.ChangeItemText(item, i, dr.GetValue(i).ToString());
                                    }
                                    this.listView_query_results.Items.Add(item);
                                    nCount++;

                                    if ((nCount % 1000) == 0)
                                        stop.SetMessage(nCount.ToString());
                                }
                            }
                            finally
                            {
                                dr.Close();
                            }
                        }
                        catch (SQLiteException ex)
                        {
                            strError = "执行SQL语句发生错误: " + ex.Message + "\r\nSQL 语句: " + this.textBox_query_command.Text;
                            goto ERROR1;
                        }
                    }
                }
            }
            finally
            {
                this.timer_qu.Stop();
                this.listView_query_results.EndUpdate();

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_query_do_Click(object sender, EventArgs e)
        {
            SQLiteQuery();
        }


        #region ErrorInfoForm

        /// <summary>
        /// 错误信息窗
        /// </summary>
        public HtmlViewerForm ErrorInfoForm = null;

        // 获得错误信息窗
        internal HtmlViewerForm GetErrorInfoForm()
        {
            if (this.ErrorInfoForm == null
                || this.ErrorInfoForm.IsDisposed == true
                || this.ErrorInfoForm.IsHandleCreated == false)
            {
                this.ErrorInfoForm = new HtmlViewerForm();
                this.ErrorInfoForm.ShowInTaskbar = false;
                this.ErrorInfoForm.Text = "错误信息";
                this.ErrorInfoForm.Show(this);
                this.ErrorInfoForm.WriteHtml("<pre>");  // 准备文本输出
            }

            return this.ErrorInfoForm;
        }


        // 清除错误信息窗口中残余的内容
        internal void ClearErrorInfoForm()
        {
            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.HtmlString = "<pre>";
                }
                catch
                {
                }
            }
        }

        #endregion

        private void checkBox_start_enableFirst_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_start_enableFirst.Checked == true)
            {
                string strError = "";
                string strEndDate = "";
                long lIndex = 0;
                string strState = "";

                // 读入断点信息
                // return:
                //      -1  出错
                //      0   正常
                //      1   首次创建尚未完成
                int nRet = LoadDailyBreakPoint(
                    out strEndDate,
                    out lIndex,
                    out strState,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                if (strState == "first" || strState == "daily")
                {
                    DialogResult temp_result = MessageBox.Show(this,
    "重新从头创建本地存储，需要清除当前的断点信息。\r\n\r\n确实要这样做?",
    "ReportForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (temp_result == System.Windows.Forms.DialogResult.No)
                        return;
                    ClearBreakPoint();
                }

                this.button_start_createLocalStorage.Enabled = true;
            }
        }

        // 获得显示用的馆代码形态
        public static string GetDisplayLibraryCode(string strLibraryCode)
        {
            if (string.IsNullOrEmpty(strLibraryCode) == true)
                return "[全局]";
            return strLibraryCode;
        }

        // 获得内部用的官代码形态
        public static string GetOriginLibraryCode(string strDisplayText)
        {
            if (strDisplayText == "[全局]")
                return "";

            return strDisplayText;
        }

        // 只创建选定分馆的报表
        void menu_createSelectedLibraryReport_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

#if SN
            nRet = this.MainForm.VerifySerialCode("report", false, out strError);
            if (nRet == -1)
            {
                MessageBox.Show( "创建报表功能尚未被许可");
                return;
            }
#endif

            string strTaskFileName = Path.Combine(GetBaseDirectory(), "dailyreport_task.xml");
            XmlDocument task_dom = new XmlDocument();

            if (File.Exists(strTaskFileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
"发现上次创建报表的任务被中断过，尚未完成。\r\n\r\n是否从断点位置继续处理?\r\n\r\n(是)继续处理; (否)从头开始处理; (取消)放弃全部处理",
"ReportForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        task_dom.Load(strTaskFileName);
                        goto DO_TASK;
                    }
                    catch (Exception ex)
                    {
                        strError = "装载文件 '" + strTaskFileName + "' 到 XMLDOM 时出错: " + ex.Message;
                        goto ERROR1;
                    }
                }
            }

            task_dom.LoadXml("<root />");

            File.Delete(strTaskFileName);
            strTaskFileName = "";

            if (this.listView_libraryConfig.SelectedItems.Count == 0)
            {
                strError = "尚未选定要创建报表的分馆事项";
                goto ERROR1;
            }

            ListViewItem firsr_item = this.listView_libraryConfig.SelectedItems[0];
            string strFirstLibraryCode = ListViewUtil.GetItemText(firsr_item, 0);
            strFirstLibraryCode = GetOriginLibraryCode(strFirstLibraryCode);

            XmlNode nodeFirstLibrary = this._cfg.GetLibraryNode(strFirstLibraryCode);
            if (nodeFirstLibrary == null)
            {
                strError = "在配置文件中没有找到馆代码为 '" + strFirstLibraryCode + "' 的 <library> 元素";
                goto ERROR1;
            }

            // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
            string strLastDate = this.MainForm.AppInfo.GetString(GetReportSection(),
"daily_report_end_date",
"20130101");

            string strEndDate = DateTimeUtil.DateTimeToString8(DateTime.Now);

            string strRealEndDate = "";

#if NO
            List<string> report_names = new List<string>();
            XmlNodeList nodes = nodeLibrary.SelectNodes("reports/report");
            foreach (XmlNode node in nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                report_names.Add(strName);
            }
#endif

            string strReportNameList = this.MainForm.AppInfo.GetString(GetReportSection(),
    "createwhat_reportnames",
    "");

            // 询问创建报表的时间范围
            // 询问那些频率的日期需要创建
            // 询问那些报表需要创建
            CreateWhatsReportDialog dlg = new CreateWhatsReportDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.DateRange = this.MainForm.AppInfo.GetString(GetReportSection(),
    "createwhat_daterange",
    "");
            if (string.IsNullOrEmpty(dlg.DateRange) == true)
                dlg.DateRange = strLastDate + "-" + strEndDate; // 从上次最后处理时间，到今天
            dlg.Freguency = this.MainForm.AppInfo.GetString(GetReportSection(),
    "createwhat_frequency",
    "year,month,day");
            // dlg.ReportsNames = report_names;
            dlg.LoadReportList(nodeFirstLibrary);

            if (string.IsNullOrEmpty(strReportNameList) == false)
                dlg.SelectedReportsNames = StringUtil.SplitList(strReportNameList, "|||");
            this.MainForm.AppInfo.LinkFormState(dlg, "CreateWhatsReportDialog_state");
            dlg.UiState = this.MainForm.AppInfo.GetString(GetReportSection(), "CreateWhatsReportDialog_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString(GetReportSection(), "CreateWhatsReportDialog_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            this.MainForm.AppInfo.SetString(GetReportSection(),
"createwhat_reportnames",
StringUtil.MakePathList(dlg.SelectedReportsNames, "|||"));
            this.MainForm.AppInfo.SetString(GetReportSection(),
"createwhat_frequency",
dlg.Freguency);
            this.MainForm.AppInfo.SetString(GetReportSection(),
"createwhat_daterange",
dlg.DateRange);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            List<string> freq_types = StringUtil.SplitList(dlg.Freguency);
            if (string.IsNullOrEmpty(dlg.Freguency) == true)
            {
                freq_types.Add("year");
                freq_types.Add("month");
                freq_types.Add("day");
            }
#if NO
            List<BiblioDbFromInfo> class_styles = null;
            // 获得所有分类号检索途径 style
            nRet = GetClassFromStylesFromFile(out class_styles,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在规划任务 ...");
            stop.BeginLoop();
            try
            {
#if NO

                // 创建必要的索引
                this._connectionString = GetOperlogConnectionString();
                stop.SetMessage("正在检查和创建 SQL 索引 ...");
                foreach (string type in OperLogTable.DbTypes)
                {
                    nRet = OperLogTable.CreateAdditionalIndex(
                        type,
                        this._connectionString,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 删除所有先前复制出来的 class 表
                nRet = DeleteAllDistinctClassTable(out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif

                // CloseIndexXmlDocument();
                try
                {
                    // 获得本窗口选用了的馆代码
                    List<string> librarycodes = this.GetLibraryCodes(false);

                    foreach (string strLibraryCode in librarycodes)
                    {
                        Application.DoEvents();

                        XmlElement library_element = task_dom.CreateElement("library");
                        task_dom.DocumentElement.AppendChild(library_element);
                        library_element.SetAttribute("code", strLibraryCode);

                        foreach (string strTimeType in freq_types)
                        {
                            List<string> report_names = new List<string>();
                            if (string.IsNullOrEmpty(dlg.Freguency) == true)
                            {
                                // 每个报表都有独特的频率，选出符合频率的报表
                                foreach (string strReportName in dlg.SelectedReportsNames)
                                {
                                    if (dlg.GetReportFreq(strReportName).IndexOf(strTimeType) == -1)
                                        continue;
                                    report_names.Add(strReportName);
                                }

                                if (report_names.Count == 0)
                                    continue;
                            }
                            else
                            {
                                report_names = dlg.SelectedReportsNames;
                            }

                            List<OneTime> times = null;

                            // parameters:
                            //      strType 时间单位类型。 year month week day 之一
                            nRet = GetTimePoints(
                                strTimeType,
                                dlg.DateRange,  // 可以使用 2013 这样的表示一年的范围的
                                false,
                                out strRealEndDate,
                                out times,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            foreach (OneTime time in times)
                            {
                                // stop.SetMessage("正在创建 " + GetDisplayLibraryCode(strLibraryCode) + " " + time.Time + " 的报表");

                                Application.DoEvents();
                                if (stop != null && stop.State != 0)
                                {
                                    strError = "中断";
                                    goto ERROR1;
                                }

                                bool bTailTime = false; // 是否为本轮最后一个(非探测)时间
                                if (times.IndexOf(time) == times.Count - 1
                                    && strTimeType != "free")
                                {
                                    if (time.Detect == false)
                                        bTailTime = true;
                                }

                                Debug.Assert(report_names.Count > 0, "");

                                XmlElement item_element = task_dom.CreateElement("item");
                                library_element.AppendChild(item_element);
                                item_element.SetAttribute("timeType", strTimeType);
                                item_element.SetAttribute("time", time.ToString());
                                item_element.SetAttribute("isTail", bTailTime ? "true" : "false");
                                item_element.SetAttribute("reportNames", StringUtil.MakePathList(report_names, "|||"));

#if NO
                                nRet = CreateOneTimeReports(
    strTimeType,
    time,
    bTailTime,
    // times,
    strLibraryCode,
    report_names,
    class_styles,
    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
#endif

                            }
                        }

                    }
                }
                finally
                {
                    // CloseIndexXmlDocument();
                }

#if NO
                // 删除所有先前复制出来的 class 表
                nRet = DeleteAllDistinctClassTable(out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

            // 由于没有修改报表最后时间，所以“每日报表”按钮状态和文字没有变化 

            strTaskFileName = Path.Combine(GetBaseDirectory(), "dailyreport_task.xml");
            task_dom.Save(strTaskFileName); // 预先保存一次

            DO_TASK:
            nRet = DoDailyReportTask(
                ref task_dom,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            else
                File.Delete(strTaskFileName);   // 任务完成，删除任务文件
            return;
        ERROR1:
            if (task_dom != null && string.IsNullOrEmpty(strTaskFileName) == false)
                task_dom.Save(strTaskFileName);

            MessageBox.Show(this, strError);

#if NO
            SetUploadButtonState();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif

        }

        delegate void Delegate_SetUploadButtonText(string strText, string strEnabled);

        internal void SetUploadButtonText(string strText, string strEnabled)
        {
            if (this.InvokeRequired == true)
            {
                Delegate_SetUploadButtonText d = new Delegate_SetUploadButtonText(SetUploadButtonText);
                this.BeginInvoke(d, new object[] { strText, strEnabled });
                return;
            }
            this.button_start_uploadReport.Text = strText;
            if (strEnabled == "true")
                this.button_start_uploadReport.Enabled = true;
            if (strEnabled == "false")
                this.button_start_uploadReport.Enabled = false;
        }

        // 设置 “每日报表” 按钮的状态和文字
        // 当每日同步结束后，或者修改了最后统计日期后，需要更新这个按钮的状态
        void SetDailyReportButtonState()
        {
            string strError = "";
            string strRange = "";
            // return:
            //      -1  出错
            //      0   报表已经是最新状态。strError 中有提示信息
            //      1   获得了可以用于处理的范围字符串。strError 中没有提示信息
            int nRet = GetDailyReportRangeString(out strRange,
                out strError);
            if (nRet == -1 || nRet == 0)
                this.button_start_dailyReport.Text = "每日报表 " + strError;
            else
                this.button_start_dailyReport.Text = "每日报表 " + strRange;

            if (string.IsNullOrEmpty(strRange) == true)
                this.button_start_dailyReport.Enabled = false;
            else
                this.button_start_dailyReport.Enabled = true;
        }

        // 已经创建唯一事项表的原始 classtable 的名字列表
        Hashtable _classtable_nametable = new Hashtable();

        int PrepareDistinctClassTable(
            string strTableName,
            out string strError)
        {
            strError = "";

            if (_classtable_nametable.ContainsKey(strTableName) == true)
                return 0;

            int nRet = ClassLine.CreateDistinctClassTable(this._connectionString,
                strTableName,
                strTableName + "_d",
                out strError);
            if (nRet == -1)
                return -1;

            _classtable_nametable[strTableName] = true;

            return 0;
        }

        // 删除所有先前复制出来的 class 表
        int DeleteAllDistinctClassTable(out string strError)
        {
            strError = "";

            foreach (string strTableName in this._classtable_nametable.Keys)
            {
                int nRet = ClassLine.DeleteClassTable(this._connectionString,
                    strTableName + "_d",
                out strError);
                if (nRet == -1)
                    return -1;
            }

            this._classtable_nametable.Clear();
            return 0;
        }

        // 要创建的文件类型
        [Flags]
        enum FileType
        {
            RML = 0x01,
            HTML = 0x02,
            Excel = 0x04,
        }

        FileType _fileType = FileType.RML;  // | FileType.HTML;  // | FileType.Excel;

        // 每日增量创建报表
        private void button_start_dailyReport_Click(object sender, EventArgs e)
        {
            // 需要记忆一个最原始的开始时间。如果没有，就从首次创建本地存储的开始有日志文件的时间
            // 从这个时间开始，检查每年、每月、每周、每日的报表是否满足创建的时间条件
            // 每日的报表，倒着检查时间即可。到了一个已经创建过的日子，就停止检查

            string strError = "";
            int nRet = 0;

#if SN
            nRet = this.MainForm.VerifySerialCode("report", false, out strError);
            if (nRet == -1)
            {
                MessageBox.Show("创建报表功能尚未被许可");
                return;
            }
#endif

            bool bFirst = false;    // 是否为第一次做
            string strTaskFileName = Path.Combine(GetBaseDirectory(), "dailyreport_task.xml");
            XmlDocument task_dom = new XmlDocument();

            if (File.Exists(strTaskFileName) == true)
            {
                DialogResult result = MessageBox.Show(this,
"发现上次创建报表的任务被中断过，尚未完成。\r\n\r\n是否从断点位置继续处理?\r\n\r\n(是)继续处理; (否)从头开始处理; (取消)放弃全部处理",
"ReportForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    try
                    {
                        task_dom.Load(strTaskFileName);
                        goto DO_TASK;
                    }
                    catch (Exception ex)
                    {
                        strError = "装载文件 '"+strTaskFileName+"' 到 XMLDOM 时出错: " +ex.Message;
                        goto ERROR1;
                    }
                }
            }


            task_dom.LoadXml("<root />");
            File.Delete(strTaskFileName);
            strTaskFileName = "";

#if NO
            // 获得上次处理的末尾日期
            // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
            string strLastDay = this.MainForm.AppInfo.GetString(GetReportSection(),
    "daily_report_end_date",
    "");
            if (string.IsNullOrEmpty(strLastDay) == true)
            {
                strError = "当前尚未配置上次统计最末日期";
                goto ERROR1;
            }

            // 当天日期
            string strEndDay = DateTimeUtil.DateTimeToString8(DateTime.Now);
#endif
            string strRange = "";
            // 获得即将执行的每日报表的时间范围
            // return:
            //      -1  出错
            //      0   报表已经是最新状态。strError 中有提示信息
            //      1   获得了可以用于处理的范围字符串。strError 中没有提示信息
            nRet = GetDailyReportRangeString(out strRange,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 0)
                goto ERROR1;

            // List<BiblioDbFromInfo> class_styles = null;

            // 看看是不是首次执行
            {
                string strFileName = Path.Combine(GetBaseDirectory(), "report_breakpoint.xml");
                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.Load(strFileName);
                }
                catch (FileNotFoundException)
                {
                    dom.LoadXml("<root />");
                }
                catch (Exception ex)
                {
                    strError = "装载文件 '" + strFileName + "' 时出错: " + ex.Message;
                    goto ERROR1;
                }

                string strFirstDate = DomUtil.GetAttr(dom.DocumentElement, "first_operlog_date");
                string strLastDay = this.MainForm.AppInfo.GetString(GetReportSection(),
                    "daily_report_end_date",
                    "");
                if (strFirstDate == strLastDay)
                    bFirst = true;
                else
                    bFirst = false;

#if NO
                // 获得所有分类号检索途径 style
                nRet = GetClassFromStyles(
                    dom.DocumentElement,
                    out class_styles,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif
            }

#if NO
            // 装载上次部分完成的名字表
            nRet = LoadDoneTable(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (this._doneTable.Count > 0)
            {
                MessageBox.Show(this, "上次任务已经完成 " + this._doneTable.Count.ToString() + " 个事项，本次将从断点继续进行处理");
            }
#endif

            string strRealEndDate = "";
            // bool bFoundReports = false;

            // 获得本窗口全部馆代码
            List<string> librarycodes = this.GetLibraryCodes();

            if (librarycodes.Count == 0)
            {
                strError = "尚未配置任何分馆的报表。请先到 “报表配置” 属性页配置好报表";
                goto ERROR1;
            }

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在规划任务 ...");
            stop.BeginLoop();
            try
            {
#if NO
                // 创建必要的索引
                this._connectionString = GetOperlogConnectionString();
                stop.SetMessage("正在检查和创建 SQL 索引 ...");
                foreach (string type in OperLogTable.DbTypes)
                {
                    Application.DoEvents();

                    nRet = OperLogTable.CreateAdditionalIndex(
                        type,
                        this._connectionString,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 删除所有先前复制出来的 class 表
                nRet = DeleteAllDistinctClassTable(out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif

                List<string> types = new List<string>();
                types.Add("year");
                types.Add("month");
                types.Add("day");

                foreach (string strLibraryCode in librarycodes)
                {
                    Application.DoEvents();

                    XmlElement library_element = task_dom.CreateElement("library");
                    task_dom.DocumentElement.AppendChild(library_element);
                    library_element.SetAttribute("code", strLibraryCode);

                    try
                    {
                        foreach (string strTimeType in types)
                        {
                            Application.DoEvents();
                            List<OneTime> times = null;

                            // parameters:
                            //      strType 时间单位类型。 year month week day 之一
                            nRet = GetTimePoints(
                                strTimeType,
                                strRange,   // strLastDay + "-" + strEndDay,
                                !bFirst,
                                out strRealEndDate,
                                out times,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            foreach (OneTime time in times)
                            {
                                // stop.SetMessage("正在创建 " + GetDisplayLibraryCode(strLibraryCode) + " " + time.Time + " 的报表");

                                Application.DoEvents();
                                if (stop != null && stop.State != 0)
                                {
                                    strError = "中断";
                                    goto ERROR1;
                                }

                                bool bTailTime = false; // 是否为本轮最后一个(非探测)时间
                                if (times.IndexOf(time) == times.Count - 1)
                                {
                                    if (time.Detect == false)
                                        bTailTime = true;
                                }

                                XmlElement item_element = task_dom.CreateElement("item");
                                library_element.AppendChild(item_element);
                                item_element.SetAttribute("timeType", strTimeType);
                                item_element.SetAttribute("time", time.ToString());
                                // item_element.SetAttribute("times", OneTime.TimesToString(times));
                                item_element.SetAttribute("isTail", bTailTime ? "true" : "false");


#if NO
                                // return:
                                //      -1  出错
                                //      0   没有任何匹配的报表
                                //      1   成功处理
                                nRet = CreateOneTimeReports(
                                    strTimeType,
                                    time,
                                    times,
                                    strLibraryCode,
                                    null,
                                    class_styles,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                if (nRet == 1)
                                    bFoundReports = true;
#endif
                            }
                        }

                    }
                    finally
                    {
                        // CloseIndexXmlDocument();
                    }
                }

#if NO
                // 删除所有先前复制出来的 class 表
                nRet = DeleteAllDistinctClassTable(out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);
            }

#if NO
            // 删除遗留信息文件
            nRet = SaveDoneTable(
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif

            task_dom.DocumentElement.SetAttribute("realEndDate", strRealEndDate);
#if NO
            if (string.IsNullOrEmpty(strRealEndDate) == false)
            {
                // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
                this.MainForm.AppInfo.SetString(GetReportSection(),
                    "daily_report_end_date",
                    GetNextDate(strRealEndDate));
                SetDailyReportButtonState();
            }

            SetUploadButtonState();

            if (bFoundReports == false)
                MessageBox.Show(this, "当前没有任何报表配置可供创建报表。请先去“报表配置”属性页配置好各个分馆的报表");
#endif
            strTaskFileName = Path.Combine(GetBaseDirectory(), "dailyreport_task.xml");
            task_dom.Save(strTaskFileName); // 预先保存一次

            DO_TASK:
            nRet = DoDailyReportTask(
                ref task_dom,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            else
                File.Delete(strTaskFileName);   // 任务完成，删除任务文件
            return;
        ERROR1:
#if NO
            {
                string strError1 = "";
                // 中间出错，或者中断，保存遗留信息文件
                nRet = SaveDoneTable(
                    false,
                    out strError1);
                if (nRet == -1)
                    MessageBox.Show(this, strError1);
            }
#endif
            if (task_dom != null && string.IsNullOrEmpty(strTaskFileName) == false)
                task_dom.Save(strTaskFileName);

            MessageBox.Show(this, strError);
        }

        void ClearCache()
        {
            this._writerCache.Clear();
            this._libraryLocationCache.Clear();
        }

        // 每日增量创建报表
        // return:
        //      -1  出错，或者中断
        //      0   没有任何配置的报表
        //      1   成功
        int DoDailyReportTask(
            ref XmlDocument task_dom,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if SN
            nRet = this.MainForm.VerifySerialCode("report", false, out strError);
            if (nRet == -1)
            {
                strError = "创建报表功能尚未被许可";
                goto ERROR1;
            }
#endif

            int nDoneCount = 0;

            ClearCache();

            if (DomUtil.GetIntegerParam(task_dom.DocumentElement,
                "doneCount",
                0,
                out nDoneCount,
                out strError) == -1)
                goto ERROR1;

            List<BiblioDbFromInfo> class_styles = null;

            // 从计划文件获得所有分类号检索途径 style
            nRet = GetClassFromStylesFromFile(
                out class_styles,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // string strRealEndDate = "";
            bool bFoundReports = false;

            this.EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在创建报表 ...");
            stop.BeginLoop();
            try
            {
                // 创建必要的索引
                this._connectionString = GetOperlogConnectionString();
                stop.SetMessage("正在检查和创建 SQL 索引 ...");
                foreach (string type in OperLogTable.DbTypes)
                {
                    Application.DoEvents();

                    nRet = OperLogTable.CreateAdditionalIndex(
                        type,
                        this._connectionString,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                // 删除所有先前复制出来的 class 表
                nRet = DeleteAllDistinctClassTable(out strError);
                if (nRet == -1)
                    goto ERROR1;

                XmlNodeList all_item_nodes = task_dom.DocumentElement.SelectNodes("library/item");
                stop.SetProgressRange(0, all_item_nodes.Count + nDoneCount);

                _estimate.SetRange(0, all_item_nodes.Count + nDoneCount);
                _estimate.StartEstimate();

                XmlNodeList library_nodes = task_dom.DocumentElement.SelectNodes("library");
                int i = nDoneCount;
                stop.SetProgressValue(i);
                foreach (XmlElement library_element in library_nodes)
                {
                    Application.DoEvents();

                    string strLibraryCode = library_element.GetAttribute("code");

                    XmlNodeList item_nodes = library_element.SelectNodes("item");

                    foreach (XmlElement item_element in item_nodes)
                    {
                        string strTimeType = item_element.GetAttribute("timeType");
                        OneTime time = OneTime.FromString(item_element.GetAttribute("time"));
                        // List<OneTime> times = OneTime.TimesFromString(item_element.GetAttribute("times"));
                        bool bTailTime = DomUtil.IsBooleanTrue(item_element.GetAttribute("isTail"));
                        List<string> report_names = StringUtil.SplitList(item_element.GetAttribute("reportNames"), "|||");
                        if (report_names.Count == 0)
                            report_names = null;

                        stop.SetMessage("正在创建 " + GetDisplayLibraryCode(strLibraryCode) + " " + time.Time + " 的报表。" + GetProgressTimeString(i));

                        Application.DoEvents();
                        if (stop != null && stop.State != 0)
                        {
                            strError = "中断";
                            goto ERROR1;
                        }

                        // return:
                        //      -1  出错
                        //      0   没有任何匹配的报表
                        //      1   成功处理
                        nRet = CreateOneTimeReports(
                            strTimeType,
                            time,
                            bTailTime,
                            // times,
                            strLibraryCode,
                            report_names,
                            class_styles,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 1)
                            bFoundReports = true;

                        item_element.ParentNode.RemoveChild(item_element);  // 做过的报表事项, 从 task_dom 中删除
                        nDoneCount++;

                        i++;
                        stop.SetProgressValue(i);
                    }

                        // fileType 没有 html 的时候，不要创建 index.html 文件
                    if ((this._fileType & FileType.HTML) != 0)
                    {
                        string strOutputDir = GetReportOutputDir(strLibraryCode);
                        string strIndexXmlFileName = Path.Combine(strOutputDir, "index.xml");
                        string strIndexHtmlFileName = Path.Combine(strOutputDir, "index.html");

                        if (stop != null)
                            stop.SetMessage("正在创建 " + strIndexHtmlFileName);

                        // 根据 index.xml 文件创建 index.html 文件
                        nRet = CreateIndexHtmlFile(strIndexXmlFileName,
                            strIndexHtmlFileName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                }

                // 删除所有先前复制出来的 class 表
                nRet = DeleteAllDistinctClassTable(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                this.EnableControls(true);

                task_dom.DocumentElement.SetAttribute("doneCount", nDoneCount.ToString());
            }

            ShrinkIndexCache(true);
#if NO
            // 删除遗留信息文件
            nRet = SaveDoneTable(
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;
#endif
            string strRealEndDate = task_dom.DocumentElement.GetAttribute("realEndDate");

            if (string.IsNullOrEmpty(strRealEndDate) == false)
            {
                // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
                this.MainForm.AppInfo.SetString(GetReportSection(),
                    "daily_report_end_date",
                    GetNextDate(strRealEndDate));
                SetDailyReportButtonState();
            }

            // SetUploadButtonState();
            BeginUpdateUploadButtonText();

#if NO
            if (bFoundReports == false)
                MessageBox.Show(this, "当前没有任何报表配置可供创建报表。请先去“报表配置”属性页配置好各个分馆的报表");
#endif
            if (bFoundReports == false)
            {
                strError = "当前没有任何报表配置可供创建报表。请先去“报表配置”属性页配置好各个分馆的报表";
                return 0;
            }

            ClearCache();
            this.MainForm.StatusBarMessage = "耗费时间 " + ProgressEstimate.Format(_estimate.delta_passed);
            return 1;
        ERROR1:
#if NO
            {
                string strError1 = "";
                // 中间出错，或者中断，保存遗留信息文件
                nRet = SaveDoneTable(
                    false,
                    out strError1);
                if (nRet == -1)
                    MessageBox.Show(this, strError1);
            }
            MessageBox.Show(this, strError);
#endif
            ClearCache();
            ShrinkIndexCache(true);
            return -1;
        }

        // 获得和当前服务器、用户相关的报表窗配置 section 名字字符串
        string GetReportSection()
        {
            string strServerUrl = ReportForm.GetValidPathString(this.MainForm.LibraryServerUrl.Replace("/", "_"));

            return "r_" + strServerUrl + "_" + ReportForm.GetValidPathString(this.MainForm.GetCurrentUserName());
        }

        // 获得即将执行的每日报表的时间范围
        // return:
        //      -1  出错
        //      0   报表已经是最新状态。strError 中有提示信息
        //      1   获得了可以用于处理的范围字符串。strError 中没有提示信息
        int GetDailyReportRangeString(out string strRange,
            out string strError)
        {
            strRange = "";
            strError = "";

            // 获得上次处理的末尾日期
            // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
            string strLastDay = this.MainForm.AppInfo.GetString(GetReportSection(),
    "daily_report_end_date",
    "");
            if (string.IsNullOrEmpty(strLastDay) == true)
            {
                strError = "尚未配置报表最末日期";
                return -1;
            }

            DateTime start;
            try
            {
                start = DateTimeUtil.Long8ToDateTime(strLastDay);
            }
            catch
            {
                strError = "开始日期 '" + strLastDay + "' 不合法。应该是 8 字符的日期格式";
                return -1;
            }

            string strDailyEndDate = "";
            {
                long lIndex = 0;
                string strState = "";

                // 读入断点信息
                // return:
                //      -1  出错
                //      0   正常
                //      1   首次创建尚未完成
                int nRet = LoadDailyBreakPoint(
                    out strDailyEndDate,
                    out lIndex,
                    out strState,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得日志同步最后日期时出错: " + strError;
                    return -1;
                }
                if (nRet == 1)
                {
                    strError = "首次创建本地存储尚未完成，无法创建报表";
                    return -1;
                }
            }

            DateTime daily_end;
            try
            {
                daily_end = DateTimeUtil.Long8ToDateTime(strDailyEndDate);
            }
            catch
            {
                strError = "日志同步最后日期 '" + strDailyEndDate + "' 不合法。应该是 8 字符的日期格式";
                return -1;
            }

            // 两个日期都不允许超过今天
            if (start >= daily_end)
            {
                // strError = "上次统计最末日期 '"+strLastDay+"' 不应晚于 日志同步最后日期 " + strDailyEndDate + " 的前一天";
                // return -1;
                strError = "报表已经是最新";
                return 0;
            }

            DateTime end = daily_end - new TimeSpan(1, 0, 0, 0, 0);
            string strEndDate = DateTimeUtil.DateTimeToString8(end);

            if (strLastDay == strEndDate)
                strRange = strLastDay;  // 缩略表示
            else
                strRange = strLastDay + "-" + strEndDate;
            return 1;
        }

        // 获得一个日期的下一天
        // parameters:
        //      strDate 8字符的时间格式
        static string GetNextDate(string strDate)
        {
            DateTime start;
            try
            {
                start = DateTimeUtil.Long8ToDateTime(strDate);
            }
            catch
            {
                return strDate; // 返回原样的字符串
            }

            return DateTimeUtil.DateTimeToString8(start + new TimeSpan(1, 0, 0, 0, 0));
        }

        // 一个处理时间
        class OneTime
        {
            public string Time = "";
            public bool Detect = false; // 是否要探测这个时间已经做过? true 表示要探测。false 表示无论如何都要做

            public OneTime()
            {
            }

            public OneTime(string strTime)
            {
                this.Time = strTime;
            }

            public OneTime(string strTime, bool bDetect)
            {
                this.Time = strTime;
                this.Detect = bDetect;
            }

            public override string ToString()
            {
                return this.Time + "|" + (this.Detect == true ? "true" : "false");
            }

            public static OneTime FromString(string strText)
            {
                string strTime = "";
                string strDetect = "";
                StringUtil.ParseTwoPart(strText,
                    "|",
                    out strTime,
                    out strDetect);
                OneTime result = new OneTime();
                result.Time = strTime;
                result.Detect = DomUtil.IsBooleanTrue(strDetect);

                return result;
            }

            public static string TimesToString(List<OneTime> times)
            {
                if (times == null)
                    return "";

                StringBuilder text = new StringBuilder();
                foreach (OneTime time in times)
                {
                    if (text.Length > 0)
                        text.Append(",");
                    text.Append(time.ToString());
                }

                return text.ToString();
            }

            public static List<OneTime> TimesFromString(string strText)
            {
                List<OneTime> results = new List<OneTime>();
                if (string.IsNullOrEmpty(strText) == true)
                    return results;

                string[] segments = strText.Split(new char[] {','});
                foreach (string strTime in segments)
                {
                    results.Add(OneTime.FromString(strTime));
                }

                return results;
            }
        }

        // 获得上个月的 4 字符时间
        static string GetPrevMonthString(DateTime current)
        {
            if (current.Month == 1)
                return (current.Year - 1).ToString().PadLeft(4, '0') + "12";
            return current.Year.ToString().PadLeft(4, '0') + (current.Month - 1).ToString().PadLeft(2, '0');
        }

        // parameters:
        //      strType 时间单位类型。 year month week day 之一
        //      strDateRange 日期范围。其中结束日期不允许超过今天。因为今天的日志可能没有同步完
        //      bDetect 是否要增加一个探测时间值? 根据开始的日期，如果属于每月一号则负责探测上一个月； 如果属于 1 月 1 号则负责探测上一年
        //      strRealEndDate  返回实际处理完的最后一天
        int GetTimePoints(
            string strType,
            string strDateRange,
            bool bDetect,
            out string strRealEndDate,
            out List<OneTime> values,
            out string strError)
        {
            strError = "";
            values = new List<OneTime>();
            strRealEndDate = "";

            string strStartDate = "";
            string strEndDate = "";

            try
            {
                // 将日期字符串解析为起止范围日期
                // throw:
                //      Exception
                DateTimeUtil.ParseDateRange(strDateRange,
                    out strStartDate,
                    out strEndDate);

                // 2014/3/19
                if (string.IsNullOrEmpty(strEndDate) == true)
                    strEndDate = strStartDate;
            }
            catch (Exception)
            {
                strError = "日期范围字符串 '" + strDateRange + "' 格式不正确";
                return -1;
            }

            DateTime start;
            try
            {
                start = DateTimeUtil.Long8ToDateTime(strStartDate);
            }
            catch
            {
                strError = "统计日期范围 '" + strDateRange + "' 中的开始日期 '" + strStartDate + "' 不合法。应该是 8 字符的日期格式";
                return -1;
            }

            DateTime end;
            try
            {
                end = DateTimeUtil.Long8ToDateTime(strEndDate);
            }
            catch
            {
                strError = "统计日期范围 '" + strDateRange + "' 中的结束日期 '" + strEndDate + "' 不合法。应该是 8 字符的日期格式";
                return -1;
            }

            string strDailyEndDate = "";
            {
                long lIndex = 0;
                string strState = "";

                // 读入断点信息
                // return:
                //      -1  出错
                //      0   正常
                //      1   首次创建尚未完成
                int nRet = LoadDailyBreakPoint(
                    out strDailyEndDate,
                    out lIndex,
                    out strState,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得日志同步最后日期时出错: " + strError;
                    return -1;
                }
                if (nRet == 1)
                {
                    strError = "首次创建本地存储尚未完成，无法创建报表";
                    return -1;
                }
            }

#if NO
            DateTime now = DateTime.Now;
            now = now.Date; // 只取日期部分
#endif
            DateTime daily_end;
            try
            {
                daily_end = DateTimeUtil.Long8ToDateTime(strDailyEndDate);
            }
            catch
            {
                strError = "日志同步最后日期 '" + strDailyEndDate + "' 不合法。应该是 8 字符的日期格式";
                return -1;
            }

            // 两个日期都不允许超过今天
            if (start >= daily_end)
            {
                strError = "统计时间范围的起点不应晚于 日志同步最后日期 "+strDailyEndDate+" 的前一天";
                return -1;
            }

            if (end >= daily_end)
                end = daily_end - new TimeSpan(1, 0, 0, 0, 0);

            strRealEndDate = DateTimeUtil.DateTimeToString8(end);

            DateTime end_plus_one = end + new TimeSpan(1, 0, 0, 0, 0);

            if (strType == "free")
            {
                {
                    OneTime time = new OneTime(DateTimeUtil.DateTimeToString8(start) + "-" + DateTimeUtil.DateTimeToString8(end));
                    values.Add(time);
                }
            }
            else if (strType == "year")
            {
                int nFirstYear = start.Year;

                if (bDetect == true
                    && start.Month == 1 && start.Day == 1)
                {
                    // 每年 1 月 1 号，负责探测上一年
                    OneTime time = new OneTime((start.Year - 1).ToString().PadLeft(4, '0'), true);
                    values.Add(time);
                }

                int nEndYear = end_plus_one.Year;
                for (int nYear = nFirstYear; nYear < nEndYear; nYear++)
                {
                    OneTime time = new OneTime(nYear.ToString().PadLeft(4, '0'));
                    values.Add(time);
                }
            }
            else if (strType == "month")
            {
                if (bDetect == true
    && start.Month == 1)
                {
                    // 每月 1 号，负责探测上个月
                    OneTime time = new OneTime(GetPrevMonthString(start), true);
                    values.Add(time);
                }

                DateTime current = new DateTime(start.Year, start.Month, 1);
                DateTime end_month = new DateTime(end_plus_one.Year, end_plus_one.Month, 1);
                while (current < end_month)
                {
                    values.Add(new OneTime(current.Year.ToString().PadLeft(4, '0') + current.Month.ToString().PadLeft(2, '0')));
                    // 下一个月
                    if (current.Month >= 12)
                        current = new DateTime(current.Year + 1, 1, 1);
                    else
                        current = new DateTime(current.Year, current.Month + 1, 1);
                }
            }
            else if (strType == "day")
            {
                DateTime current = new DateTime(start.Year, start.Month, start.Day);
                while (current <= end)
                {
                    values.Add(new OneTime(current.Year.ToString().PadLeft(4, '0')
                        + current.Month.ToString().PadLeft(2, '0')
                        + current.Day.ToString().PadLeft(2, '0')));
                    // 下一天
                    current += new TimeSpan(1, 0, 0, 0);
                }
            }
            else if (strType == "week")
            {
                strError = "暂不支持 week";
                return -1;
            }

            return 0;
        }

        // 特定分馆的报表输出目录
        string GetReportOutputDir(string strLibraryCode)
        {
            // return Path.Combine(this.MainForm.UserDir, "reports\\" + GetValidPathString(GetDisplayLibraryCode(strLibraryCode)));

            // 2015/6/20 将创建好的报表文件存储在和每个 dp2library 服务器和用户名相关的目录中
            return Path.Combine(GetBaseDirectory(), "reports\\" + GetValidPathString(GetDisplayLibraryCode(strLibraryCode)));
        }

        // parameters:
        //      nodeLibrary 配置文件中的 library 元素节点。如果为 null，表示取全局缺省的模板
        int LoadHtmlTemplate(XmlNode nodeLibrary,
            out string strTemplate,
            out string strError)
        {
            strTemplate = "";
            strError = "";

            string strCssTemplateDir = Path.Combine(this.MainForm.UserDir, "report_def");   //  Path.Combine(this.MainForm.UserDir, "report_def");

            string strHtmlTemplate = "";

            if (nodeLibrary != null)
                strHtmlTemplate = DomUtil.GetAttr(nodeLibrary, "htmlTemplate");

            if (string.IsNullOrEmpty(strHtmlTemplate) == true)
                strHtmlTemplate = "default";

            string strFileName = Path.Combine(strCssTemplateDir, GetValidPathString(strHtmlTemplate) + ".css");
            if (File.Exists(strFileName) == false)
            {
                strError = "CSS 模板文件 '"+strFileName+"' 不存在";
                return -1;
            }

            Encoding encoding;
        // return:
        //      -1  出错 strError中有返回值
        //      0   文件不存在 strError中有返回值
        //      1   文件存在
        //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strFileName,
                -1,
                out strTemplate,
                out encoding,
                out strError);
            if (nRet != 1)
                return -1;

            return 0;
        }

#if NO
        // 根据时间字符串得到 子目录名
        // 2014 --> 2014
        // 201401 --> 2014/01
        // 20140101 --> 2014/01/01
        static string GetSubDirName(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                return "";
            if (strTime.Length == 4)
                return strTime;
            if (strTime.Length == 6)
                return strTime.Insert(4, "/");
            if (strTime.Length == 8)
                return strTime.Insert(6, "/").Insert(4, "/");
            return strTime;
        }
#endif
        // 根据时间字符串得到 子目录名
        // 2014 --> 2014
        // 201401 --> 2014/201401
        // 20140101 --> 2014/201401/20140101
        static string GetSubDirName(string strTime)
        {
            if (string.IsNullOrEmpty(strTime) == true)
                return "";
            if (strTime.Length == 4)
                return strTime;
            if (strTime.Length == 6)
                return strTime.Substring(0, 4) + "/" + strTime;
            if (strTime.Length == 8)
                return strTime.Substring(0, 4) + "/" + strTime.Substring(0, 6) + "/" + strTime;
            return strTime;
        }

#if NO
        Hashtable _doneTable = new Hashtable();

        int LoadDoneTable(
    out string strError)
        {
            strError = "";

            this._doneTable.Clear();

            string strBreakPointFileName = Path.Combine(this.MainForm.UserDir, "dailyreport_breakpoint.txt");
            if (File.Exists(strBreakPointFileName) == false)
                return 0;

            using (StreamReader sr = new StreamReader(strBreakPointFileName, Encoding.UTF8))
            {
                for (; ; )
                {
                    string strText = sr.ReadLine();
                    if (strText == null)
                        break;
                    this._doneTable[strText] = true;
                }
            }

            return 0;
        }

        int SaveDoneTable(
            bool bDelete,
            out string strError)
        {
            strError = "";

            string strBreakPointFileName = Path.Combine(this.MainForm.UserDir, "dailyreport_breakpoint.txt");
            if (bDelete == true)
            {
                this._doneTable.Clear();
                File.Delete(strBreakPointFileName);
                return 0;
            }

            using (StreamWriter sw = new StreamWriter(strBreakPointFileName, false, Encoding.UTF8))
            {
                foreach (string key in this._doneTable.Keys)
                {
                    sw.WriteLine(key);
                }
            }

            this._doneTable.Clear();
            return 0;
        }
#endif
        // 获得适合用作报表名或文件名 的 地点名称字符串
        static string GetLocationCaption(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "[空]";

            if (strText[strText.Length - 1] == '/')
                return strText.Substring(0, strText.Length - 1) + "[全部]";

            return strText;
        }

        // 分管的管部馆藏地点 cache
        ObjectCache<List<string>> _libraryLocationCache = new ObjectCache<List<string>>();


        // 创建一个特定时间段(一个分馆)的若干报表
        // 要讲创建好的报表写入相应目录的 index.xml 中
        // parameters:
        //      strTimeType 时间单位类型。 year month week day 之一
        //      times   本轮的全部时间字符串。strTime 一定在其中。通过 times 和 strTime，能看出 strTime 时间是不是数组最后一个元素
        // return:
        //      -1  出错
        //      0   没有任何匹配的报表
        //      1   成功处理
        int CreateOneTimeReports(
            string strTimeType,
            OneTime time,
            bool bTailTime,
            // List<OneTime> times,
            string strLibraryCode,
            List<string> report_names,
            List<BiblioDbFromInfo> class_styles,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            bool bTailTime = false; // 是否为本轮最后一个(非探测)时间
            if (times.IndexOf(time) == times.Count - 1)
            {
                if (time.Detect == false)
                    bTailTime = true;
            }
#endif

            // 特定分馆的报表输出目录
            // string strReportsDir = Path.Combine(this.MainForm.UserDir, "reports/" + (string.IsNullOrEmpty(strLibraryCode) == true ? "global" : strLibraryCode));
            string strReportsDir = GetReportOutputDir(strLibraryCode);
            PathUtil.CreateDirIfNeed(strReportsDir);

            // 输出文件目录
            // string strOutputDir = Path.Combine(strReportsDir, time.Time);
            // 延迟到创建表格的时候创建子目录

            string strOutputDir = Path.Combine(strReportsDir, GetValidPathString(GetSubDirName(time.Time)));


            // 看看目录是否已经存在
            if (time.Detect)
            {
                DirectoryInfo di = new DirectoryInfo(strOutputDir);
                if (di.Exists == true)
                    return 0;
            }


#if NOOOO
            List<string> class_styles = new List<string>();

            // 获得所有分类号检索途径 style
            nRet = GetClassFromStyles(out class_styles,
                out strError);
            if (nRet == -1)
                return -1;
#if NO
                class_styles.Add("clc");
                class_styles.Add("hnb");
#endif
#endif

#if NO
            List<BiblioDbFromInfo> class_styles = new List<BiblioDbFromInfo>();
            // 获得所有分类号检索途径 style
            nRet = GetClassFromStyles(out class_styles,
                out strError);
            if (nRet == -1)
                return -1;
#endif

            //foreach (string strLibraryCode in librarycodes)
            //{
            XmlNode nodeLibrary = this._cfg.GetLibraryNode(strLibraryCode);
            if (nodeLibrary == null)
            {
                strError = "在配置文件中没有找到馆代码为 '" + strLibraryCode + "' 的 <library> 元素";
                return -1;
            }

            string strTemplate = "";

            nRet = LoadHtmlTemplate(nodeLibrary,
                out strTemplate,
                out strError);
            if (nRet == -1)
                return -1;
            this._cssTemplate = strTemplate;

            List<XmlNode> report_nodes = new List<XmlNode>();
            if (report_names != null)
            {
                foreach (string strName in report_names)
                {
                    XmlNode node = nodeLibrary.SelectSingleNode("reports/report[@name='" + strName + "']");
                    if (node == null)
                    {
                        continue;
#if NO
                        strError = "在配置文件中没有找到馆代码为 '" + strLibraryCode + "' 的 <library> 元素下的 name 属性值为 '"+strName+"' 的 report 元素";
                        return -1;
                        // TODO: 出现 MessageBox 警告，但可以选择继续
#endif
                    }
                    report_nodes.Add(node);
                }

                if (report_nodes.Count == 0)
                    return 0;   // 没有任何匹配的报表
            }
            else
            {
                XmlNodeList nodes = nodeLibrary.SelectNodes("reports/report");
                if (nodes.Count == 0)
                    return 0;   // 这个分馆 当前没有配置任何报表
                foreach (XmlNode node in nodes)
                {
                    report_nodes.Add(node);
                }
            }

            foreach (XmlNode node in report_nodes)
            {
                Application.DoEvents();
                string strName = DomUtil.GetAttr(node, "name");
                string strReportType = DomUtil.GetAttr(node, "type");
                string strCfgFile = DomUtil.GetAttr(node, "cfgFile");
                string strNameTable = DomUtil.GetAttr(node, "nameTable");
                string strFreq = DomUtil.GetAttr(node, "frequency");

#if NO
                ReportConfigStruct config = null;
                // 从报表配置文件中获得各种配置信息
                // return:
                //      -1  出错
                //      0   没有找到配置文件
                //      1   成功
                nRet = ReportDefForm.GetReportConfig(strCfgFile,
                    out config,
                    out strError);
                if (nRet == -1)
                    return -1;
#endif
                ReportWriter writer = null;
                nRet = GetReportWriter(strCfgFile,
                    out writer,
                    out strError);
                if (nRet == -1)
                    return -1;

                // *** 判断频率
                if (report_names == null)
                {
                    if (StringUtil.IsInList(strTimeType, strFreq) == false)
                        continue;
                }

                // 在指定了报表名称列表的情况下，频率不再筛选

                // 判断新鲜程度。只有本轮最后一次才创建报表
#if NO
                if (config.Fresh == true && bTailTime == false)
                    continue;
#endif
                if (writer.GetFresh() == true && bTailTime == false)
                    continue;

                Hashtable macro_table = new Hashtable();
                macro_table["%library%"] = strLibraryCode;
                string strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");

                string strDoneName = strLibraryCode + "|"
                    + time.Time + "|"
                    + strName + "|"
                    + strReportType + "|";
#if NO
                if (this._doneTable.ContainsKey(strDoneName) == true)
                    continue;   // 前次已经做过了
#endif
                int nAdd = 0;   // 0 表示什么也不做。 1表示要加入 -1 表示要删除

                if (strReportType == "102")
                {
                    // *** 102
                    // 按照指定的单位名称列表，列出借书册数
                    nRet = Create_102_report(strLibraryCode,
                        time.Time,
                        strCfgFile,
                        // "选定的部门",    // 例如： 各年级
                        macro_table,
                        strNameTable,
                        strOutputFileName,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        nAdd = -1;
                    else if (nRet == 1)
                        nAdd = 1;
                }
                else if (strReportType == "101"
                    || strReportType == "111"
                    || strReportType == "121"
                    || strReportType == "122"
                    || strReportType == "141")
                {
                    nRet = Create_1XX_report(strLibraryCode,
                        time.Time,
                        strCfgFile,
                        macro_table,
                        strOutputFileName,
                        strReportType,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        nAdd = -1;
                    else if (nRet == 1)
                        nAdd = 1;
                }
                else if (strReportType == "131")
                {
                    string str131Dir = Path.Combine(strOutputDir, "table_131");
                    // 这是创建到一个子目录(会在子目录中创建很多文件和下级目录)，而不是输出到一个文件
                    nRet = Create_131_report(strLibraryCode,
                        time.Time,
                        strCfgFile,
                        macro_table,
                        str131Dir,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    // if (nRet == 1)
                    {
                        // 将 131 目录事项写入 index.xml
                        nRet = WriteIndexXml(
                            strTimeType,
                            time.Time,
                            strName,
                            "", // strReportsDir,
                            str131Dir,
                            strReportType,
                            nRet == 1,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }

                else if (strReportType == "201"
                    || strReportType == "202"
                    || strReportType == "212"
                    || strReportType == "213") // begin of 2xx
                {
                    if (strReportType == "212" && class_styles.Count == 0)
                        continue;
                    if (strReportType == "213")
                        continue;   // 213 表已经被废止，其原有功能被合并到 212 表

                    // 获得分馆的所有馆藏地点

                    List<string> locations = null;
                    locations = this._libraryLocationCache.FindObject(strLibraryCode);
                    if (locations == null)
                    {
                        nRet = GetAllItemLocations(
                            strLibraryCode,
                            true,
                            out locations,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        this._libraryLocationCache.SetObject(strLibraryCode, locations);
                    }

                    foreach (string strLocation in locations)
                    {
                        Application.DoEvents();

                        macro_table["%location%"] = GetLocationCaption(strLocation);

                        // 这里稍微特殊一点，循环要写入多个输出文件
                        if (string.IsNullOrEmpty(strOutputFileName) == true)
                            strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");

                        if (strReportType == "201")
                        {
                            nRet = Create_201_report(strLocation,
                        time.Time,
                                strCfgFile,
                                macro_table,
                                strOutputFileName,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else if (strReportType == "202")
                        {
                            nRet = Create_202_report(strLocation,
                        time.Time,
                                strCfgFile,
                                macro_table,
                                strOutputFileName,
                                out strError);
                            if (nRet == -1)
                                return -1;
                        }
                        else if (strReportType == "212"
                            || strReportType == "213")
                        {
                            // List<string> names = StringUtil.SplitList(strNameTable);
                            List<OneClassType> class_table = null;
                            nRet = OneClassType.BuildClassTypes(strNameTable,
            out class_table,
            out strError);
                            if (nRet == -1)
                            {
                                strError = "报表类型 '" + strReportType + "' 的名字表定义不合法： " + strError;
                                return -1;
                            }

                            foreach (BiblioDbFromInfo style in class_styles)
                            {
                                Application.DoEvents();

#if NO
                                if (names.Count > 0)
                                {
                                    // 只处理设定的那些 class styles
                                    if (names.IndexOf(style.Style) == -1)
                                        continue;
                                }
#endif
                                OneClassType current_type = null;
                                if (class_table.Count > 0)
                                {
                                    // 只处理设定的那些 class styles
                                    int index = OneClassType.IndexOf(class_table, style.Style);
                                    if (index == -1)
                                        continue;
                                    current_type = class_table[index];
                                }

                                // 这里稍微特殊一点，循环要写入多个输出文件
                                if (string.IsNullOrEmpty(strOutputFileName) == true)
                                    strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");

                                nRet = Create_212_report(strLocation,
                                    style.Style,
                                    style.Caption,
                                    time.Time,
                                    strCfgFile,
                                    macro_table,
                                    current_type == null ? null : current_type.Filters,
                                    strOutputFileName,
                                    strReportType,
                                    out strError);
                                if (nRet == -1)
                                    return -1;

                                // if (nRet == 1)
                                {
                                    // 写入 index.xml
                                    nRet = WriteIndexXml(
                                        strTimeType,
                                        time.Time,
                                        GetLocationCaption(strLocation) + "-" + style.Caption,  // 把名字区别开。否则写入 <report> 会重叠覆盖
                                        strName,    // strReportsDir,
                                        strOutputFileName,
                                        strReportType,
                            nRet == 1,
                                        out strError);
                                    if (nRet == -1)
                                        return -1;

                                    strOutputFileName = "";
                                }
                            }
                        }

                        if (File.Exists(strOutputFileName) == true)
                        {
                            // 写入 index.xml
                            nRet = WriteIndexXml(
                                strTimeType,
                                time.Time,
                                GetLocationCaption(strLocation),  // 把名字区别开。否则写入 <report> 会重叠覆盖
                                strName,    // strReportsDir,
                                strOutputFileName,
                                strReportType,
                                true,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            strOutputFileName = "";
                        }
                    }

                    // TODO: 总的馆藏地点还要来一次

                } // end 2xx
                else if (strReportType == "301"
                    || strReportType == "302") // begin of 3xx
                {
                    // 获得分馆的所有馆藏地点
#if NO
                    List<string> locations = null;
                    nRet = GetAllItemLocations(
                        strLibraryCode,
                        true,
                        out locations,
                        out strError);
                    if (nRet == -1)
                        return -1;
#endif

                    List<string> locations = null;
                    locations = this._libraryLocationCache.FindObject(strLibraryCode);
                    if (locations == null)
                    {
                        nRet = GetAllItemLocations(
                            strLibraryCode,
                            true,
                            out locations,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        this._libraryLocationCache.SetObject(strLibraryCode, locations);
                    }

                    foreach (string strLocation in locations)
                    {
                        macro_table["%location%"] = GetLocationCaption(strLocation);

                        // 这里稍微特殊一点，循环要写入多个输出文件
                        if (string.IsNullOrEmpty(strOutputFileName) == true)
                            strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");


                        if (strReportType == "301"
                            || strReportType == "302")
                        {
                            // List<string> names = StringUtil.SplitList(strNameTable);
                            List<OneClassType> class_table = null;
                            nRet = OneClassType.BuildClassTypes(strNameTable,
            out class_table,
            out strError);
                            if (nRet == -1)
                            {
                                strError = "报表类型 '"+strReportType+"' 的名字表定义不合法： " + strError;
                                return -1;
                            }

                            foreach (BiblioDbFromInfo style in class_styles)
                            {
                                Application.DoEvents();

#if NO
                                if (names.Count > 0)
                                {
                                    // 只处理设定的那些 class styles
                                    if (names.IndexOf(style.Style) == -1)
                                        continue;
                                }
#endif
                                OneClassType current_type = null;
                                if (class_table.Count > 0)
                                {
                                    // 只处理设定的那些 class styles
                                    int index = OneClassType.IndexOf(class_table, style.Style);
                                    if (index == -1)
                                        continue;
                                    current_type = class_table[index];
                                }


                                // 这里稍微特殊一点，循环要写入多个输出文件
                                if (string.IsNullOrEmpty(strOutputFileName) == true)
                                    strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");

                                if (strReportType == "301")
                                    nRet = Create_301_report(strLocation,
                                        style.Style,
                                        style.Caption,
                                        time.Time,
                                        strCfgFile,
                                        macro_table,
                                        current_type == null ? null : current_type.Filters,
                                        strOutputFileName,
                                        out strError);
                                else if (strReportType == "302")
                                    nRet = Create_302_report(strLocation,
        style.Style,
        style.Caption,
        time.Time,
        strCfgFile,
        macro_table,
                                        current_type == null ? null : current_type.Filters,
        strOutputFileName,
        out strError);
                                if (nRet == -1)
                                    return -1;

                                // if (nRet == 1)
                                {
                                    // 写入 index.xml
                                    nRet = WriteIndexXml(
                                        strTimeType,
                                        time.Time,
                                        GetLocationCaption(strLocation) + "-" + style.Caption,  // 把名字区别开。否则写入 <report> 会重叠覆盖
                                        strName,    // strReportsDir,
                                        strOutputFileName,
                                        strReportType,
                             nRet == 1,
                                       out strError);
                                    if (nRet == -1)
                                        return -1;

                                    strOutputFileName = "";
                                }
                            }
                        }

                        if (File.Exists(strOutputFileName) == true)
                        {
                            // 写入 index.xml
                            nRet = WriteIndexXml(
                                strTimeType,
                                time.Time,
                                GetLocationCaption(strLocation),  // 把名字区别开。否则写入 <report> 会重叠覆盖
                                strName,    // strReportsDir,
                                strOutputFileName,
                                strReportType,
                                true,
                                out strError);
                            if (nRet == -1)
                                return -1;

                            strOutputFileName = "";
                        }
                    }

                    // TODO: 总的馆藏地点还要来一次

                } // end 3xx
                else if (strReportType == "411"
                    || strReportType == "412"
                    || strReportType == "421"
                    || strReportType == "422"
                    || strReportType == "431"
                    || strReportType == "432"
                    || strReportType == "441"
                    || strReportType == "442"
                    || strReportType == "443"
                    || strReportType == "451"
                    || strReportType == "452"
                    || strReportType == "471"
                    || strReportType == "472"
                    || strReportType == "481"
                    || strReportType == "482"
                    || strReportType == "491"
                    || strReportType == "492"
                    )
                {
                    nRet = Create_4XX_report(strLibraryCode,
                        time.Time,
                        strCfgFile,
                        macro_table,
                        strOutputFileName,
                        strReportType,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                        nAdd = -1;
                    else if (nRet == 1)
                        nAdd = 1;
                }
                else if (strReportType == "493")
                {
#if NO
                        // 这里稍微特殊一点，循环要写入多个输出文件
                        if (string.IsNullOrEmpty(strOutputFileName) == true)
                            strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");
#endif
                    List<OneClassType> class_table = null;
                    nRet = OneClassType.BuildClassTypes(strNameTable,
    out class_table,
    out strError);
                    if (nRet == -1)
                    {
                        strError = "报表类型 '" + strReportType + "' 的名字表定义不合法： " + strError;
                        return -1;
                    }

                    foreach (BiblioDbFromInfo style in class_styles)
                    {
                        Application.DoEvents();

                        OneClassType current_type = null;
                        if (class_table.Count > 0)
                        {
                            // 只处理设定的那些 class styles
                            int index = OneClassType.IndexOf(class_table, style.Style);
                            if (index == -1)
                                continue;
                            current_type = class_table[index];
                        }

                        // 这里稍微特殊一点，循环要写入多个输出文件
                        if (string.IsNullOrEmpty(strOutputFileName) == true)
                            strOutputFileName = Path.Combine(strOutputDir, Guid.NewGuid().ToString() + ".rml");

                        nRet = Create_493_report(
                            strLibraryCode,
                            style.Style,
                            style.Caption,
                            time.Time,
                            strCfgFile,
                            macro_table,
                            current_type == null ? null : current_type.Filters,
                            strOutputFileName,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        // if (nRet == 1)
                        {
                            // 写入 index.xml
                            nRet = WriteIndexXml(
                                strTimeType,
                                time.Time,
                                style.Caption,  // 把名字区别开。否则写入 <report> 会重叠覆盖
                                strName,    // strReportsDir,
                                strOutputFileName,
                                strReportType,
                     nRet == 1,
                               out strError);
                            if (nRet == -1)
                                return -1;

                            strOutputFileName = "";
                        }
                    }

                }
                else
                {
                    strError = "未知的 strReportType '" + strReportType + "'";
                    return -1;
                }

#if NO
                this._doneTable[strDoneName] = true;
#endif

                if (string.IsNullOrEmpty(strOutputFileName) == false
                    && nAdd != 0)
                {
                    if (nAdd == 1 && File.Exists(strOutputFileName) == false)
                    {
                    }
                    else
                    {
                        // 写入 index.xml
                        nRet = WriteIndexXml(
                            strTimeType,
                            time.Time,
                            strName,
                            "", // strReportsDir,
                            strOutputFileName,
                            strReportType,
                            nAdd == 1,
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }
            // }

            return 1;
        }

        #region index.xml

#if NO
        XmlDocument _indexDom = null;   // index.xml 文件的 DOM 对象
        string _strIndexXmlFileName = "";   // index.xml 文件的文件名全路径
#endif

#if NO
        // 保存 index.xml 的 DOM 到文件。或者用于开始处理前的初始化
        void CloseIndexXmlDocument()
        {
            if (string.IsNullOrEmpty(_strIndexXmlFileName) == false)
            {
                // 保存上一个文件
                Debug.Assert(_indexDom != null, "");
                _indexDom.Save(_strIndexXmlFileName);
                File.SetAttributes(_strIndexXmlFileName, FileAttributes.Archive);

                _indexDom = null;
                _strIndexXmlFileName = "";
            }
        }
#endif
        public static void RemoveArchiveAttribute(string strFileName)
        {
            // File.SetAttributes(strFileName, FileAttributes.Normal);
        }

        static bool IsEmpty(XmlDocument dom)
        {
            if (dom.DocumentElement.ChildNodes.Count == 0
                && dom.DocumentElement.Name != "dir"
                && dom.DocumentElement.Name != "report")
                return true;
            return false;
        }

        ObjectCache<XmlDocument> _indexCache = new ObjectCache<XmlDocument>();

        // 收缩 cache 尺寸
        // parameters:
        //      bShrinkAll  是否全部收缩
        void ShrinkIndexCache(bool bShrinkAll)
        {
            if (this._indexCache.Count > 10
                || bShrinkAll == true)
            {
                foreach (string filename in this._indexCache.Keys)
                {
                    XmlDocument dom = this._indexCache.FindObject(filename);
                    if (dom == null)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    // string strHashCode = dom.GetHashCode().ToString();

                    if (IsEmpty(dom) == true)
                    {
                        // 2014/6/12
                        // 如果 DOM 为空，则要删除物理文件
                        try
                        {
                            File.Delete(filename);
                        }
                        catch (DirectoryNotFoundException)
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            dom.Save(filename);
                        }
                        catch (DirectoryNotFoundException)
                        {
                            PathUtil.CreateDirIfNeed(Path.GetDirectoryName(filename));
                            dom.Save(filename);
                        }
                    }
                }

                this._indexCache.Clear();
            }
        }

        // 将一个统计文件条目写入到 index.xml 的 DOM 中
        // 注：要确保每个报表的名字 strTableName 是不同的。如果同一报表要在不同条件下输出多次，需要把条件字符串也加入到名字中
        // parameters:
        //      strOutputDir    报表输出目录。例如 c:\users\administrator\dp2circulation_v2\reports
        int WriteIndexXml(
            string strTimeType,
            string strTime,
            string strTableName,
            // string strOutputDir,
            string strGroupName,
            string strReportFileName,
            string strReportType,
            bool bAdd,
            out string strError)
        {
            strError = "";

            // 这里决定在分馆所述的目录内，如何划分 index.xml 文件的层级和个数
            string strOutputDir = Path.GetDirectoryName(strReportFileName);
            string strFileName = Path.Combine(strOutputDir, "index.xml");

            XmlDocument index_dom = this._indexCache.FindObject(strFileName);
            if (index_dom == null && bAdd == false)
            {
                try
                {
                    File.Delete(strReportFileName);
                }
                catch (DirectoryNotFoundException)
                {
                } 
                return 0;
            }
            if (index_dom == null)
            {
                index_dom = new XmlDocument();
                if (File.Exists(strFileName) == true)
                {
                    try
                    {
                        index_dom.Load(strFileName);
                    }
                    catch (Exception ex)
                    {
                        strError = "装入文件 " + strFileName + " 时出错: " + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    index_dom.LoadXml("<root />");
                }
                this._indexCache.SetObject(strFileName, index_dom);
            }

            // string strHashCode = index_dom.GetHashCode().ToString();

            // 根据时间类型创建一个 index.xml 中的 item 元素
            XmlNode item = null;
            if (strReportType == "131")
            {
                item = CreateDirNode(index_dom.DocumentElement,
                    strTableName + "-" + strReportType,
                    bAdd ? 1 : -1);
                if (bAdd == false)
                    return 0;
                Debug.Assert(item != null, "");

                string strNewFileName = "." + strReportFileName.Substring(strOutputDir.Length);
                DomUtil.SetAttr(item, "link", strNewFileName.Replace("\\", "/"));
            }
            else
            {
                if (string.IsNullOrEmpty(strGroupName) == false)
                {
                    item = CreateReportNode(index_dom.DocumentElement,
                        strGroupName + "-" + strReportType,
                        strTableName,
                        bAdd);
                }
                else
                {
                    item = CreateReportNode(index_dom.DocumentElement,
                        strGroupName,
                        strTableName + "-" + strReportType,
                        bAdd);
                }

                if (bAdd == false)
                {
                    // TODO: 还需要删除 index.xml 中条目指向的原有文件
                    try
                    {
                        File.Delete(strReportFileName);
                    }
                    catch (DirectoryNotFoundException)
                    {
                    }

                    return 0;
                }

                Debug.Assert(item != null, "");

                /*
                 * 文件名不能包含任何以下字符：\ / : * ?"< > |
                 * 
    对于命名的文件、 文件夹或快捷方式是有效的字符包括字母 (A-Z) 和数字 (0-9)，再加上下列特殊字符的任意组合：
       ^   Accent circumflex (caret)
       &   Ampersand
       '   Apostrophe (single quotation mark)
       @   At sign
       {   Brace left
       }   Brace right
       [   Bracket opening
       ]   Bracket closing
       ,   Comma
       $   Dollar sign
       =   Equal sign
       !   Exclamation point
       -   Hyphen
       #   Number sign
       (   Parenthesis opening
       )   Parenthesis closing
       %   Percent
       .   Period
       +   Plus
       ~   Tilde
       _   Underscore             * */
                string strName = DomUtil.GetAttr(item, "name").Replace("/", "+");

                Debug.Assert(strGroupName.IndexOf("/") == -1, "");

                Debug.Assert(strName.IndexOf("|") == -1, "");
                // 将文件名改名
                string strFileName1 = Path.Combine(Path.GetDirectoryName(strReportFileName),
                    GetValidPathString((string.IsNullOrEmpty(strGroupName) == false ? strGroupName + "-" : "") + strName)
                    + Path.GetExtension(strReportFileName));
                if (File.Exists(strFileName1) == true)
                    File.Delete(strFileName1);

#if NO
                FileAttributes attr = File.GetAttributes(strReportHtmlFileName);
#endif

                File.Move(strReportFileName, strFileName1);
                strReportFileName = strFileName1;

#if NO
                FileAttributes attr1 = File.GetAttributes(strReportHtmlFileName);
                Debug.Assert(attr == attr1, "");
#endif
                string strNewFileName = "." + strReportFileName.Substring(strOutputDir.Length);

                // 创建 .html 文件
                if ((this._fileType & FileType.HTML) != 0)
                {
                    string strHtmlFileName = Path.Combine(Path.GetDirectoryName(strReportFileName), Path.GetFileNameWithoutExtension(strReportFileName) + ".html");
                    int nRet = Report.RmlToHtml(strReportFileName,
                        strHtmlFileName,
                        this._cssTemplate,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    RemoveArchiveAttribute(strHtmlFileName);
                }

                // 创建 Excel 文件
                if ((this._fileType & FileType.Excel) != 0)
                {
                    string strExcelFileName = Path.Combine(Path.GetDirectoryName(strReportFileName), Path.GetFileNameWithoutExtension(strReportFileName) + ".xlsx");
                    int nRet = Report.RmlToExcel(strReportFileName,
                        strExcelFileName,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    RemoveArchiveAttribute(strExcelFileName);
                }

                // 删除以前的对照关系
                string strOldFileName = DomUtil.GetAttr(item, "link");
                if (string.IsNullOrEmpty(strOldFileName) == false)
                // && PathUtil.IsEqual(strOldFileName, strHtmlFileName) == false
                {
                    string strOldPhysicalPath = GetRealPath(strOutputDir,
        strOldFileName);

                    if (PathUtil.IsEqual(strOldPhysicalPath, strReportFileName) == false
                    && File.Exists(strOldPhysicalPath) == true)
                    {
                        try
                        {

                            File.Delete(strOldPhysicalPath);
                        }
                        catch
                        {
                        }
                    }
                }

                DomUtil.SetAttr(item, "link", strNewFileName.Replace("\\", "/"));
#if DEBUG
                // 检查文件是否存在
                if (File.Exists(strReportFileName) == false)
                {
                    strError = strTime + " " + strTableName + " 的文件 '" + strReportFileName + "' 不存在";
                    return -1;
                }
#endif
            }

            // _indexDom.Save(strFileName);
            ShrinkIndexCache(false);
#if NO
#if DEBUG
            FileAttributes attr1 = File.GetAttributes(strFileName);
            Debug.Assert((attr1 & FileAttributes.Archive) == FileAttributes.Archive, "");
#endif
#endif

            // File.SetAttributes(strFileName, FileAttributes.Archive);
            return 0;
        }

        // 获得文件的物理路径
        // parameters:
        //      strFileName 为 "./2013/some" 或者 "c"\\dir\、file" 这样的 
        static string GetRealPath(string strOutputDir,
            string strFileName)
        {

            if (StringUtil.HasHead(strFileName, "./") == true
                || StringUtil.HasHead(strFileName, ".\\") == true)
            {
                return strOutputDir + strFileName.Substring(1);
            }

            return strFileName;
        }

        static XmlElement NewLeafElement(
            XmlNode parent,
            string strReportName,
            string strReportType)
        {
            string strElementName = "report";
            if (strReportType == "131")
                strElementName = "dir";
            XmlNode item = parent.SelectSingleNode(strElementName + "[@name='" + strReportName + "']");
            if (item == null)
            {
                item = parent.OwnerDocument.CreateElement(strElementName);
                parent.AppendChild(item);
                DomUtil.SetAttr(item, "name", strReportName);
                DomUtil.SetAttr(item, "type", strReportType);
            }

            return item as XmlElement;
        }

        // 根据时间类型创建一个 index.xml 中的 report(或dir) 元素
        // 要根据时间，创建一些列 dir 父元素。返回前已经写好 name 和 type 属性了
        static XmlNode CreateItemNode(XmlNode root,
            string strTimeType,
            string strTime,
            string strReportName,
            string strReportType)
        {
            Debug.Assert(strTime.Length >= 4, "");

            // 2014/6/7
            if (strTimeType == "free")
            {
                string strDirName = strTime;
                XmlNode new_node = root.SelectSingleNode("dir[@name='" + strDirName + "']");
                if (new_node == null)
                {
                    new_node = root.OwnerDocument.CreateElement("dir");
                    root.AppendChild(new_node);
                    DomUtil.SetAttr(new_node, "name", strDirName);
                    DomUtil.SetAttr(new_node, "type", "free");
                }

                return NewLeafElement(new_node, strReportName, strReportType);
            }

            string strYear = strTime.Substring(0, 4);
            XmlNode year = root.SelectSingleNode("dir[@name='" + strYear + "']");
            if (year == null)
            {
                year = root.OwnerDocument.CreateElement("dir");
                root.AppendChild(year);
                DomUtil.SetAttr(year, "name", strYear);
                DomUtil.SetAttr(year, "type", "year");
            }

            if (strTimeType == "year")
            {
#if NO
                XmlNode item = year.SelectSingleNode("report[@name='" + strReportName + "']");
                if (item == null)
                {
                    item = year.OwnerDocument.CreateElement("report");
                    year.AppendChild(item);
                    DomUtil.SetAttr(item, "name", strReportName);
                }
                return item;
#endif
                return NewLeafElement(year, strReportName, strReportType);
            }

            Debug.Assert(strTime.Length >= 6, "");
            string strMonth = strTime.Substring(0, 6);
            XmlNode month = year.SelectSingleNode("dir[@name='" + strMonth + "']");
            if (month == null)
            {
                month = root.OwnerDocument.CreateElement("dir");
                year.AppendChild(month);
                DomUtil.SetAttr(month, "name", strMonth);
                DomUtil.SetAttr(month, "type", "month");
            }

            if (strTimeType == "month")
            {
#if NO
                XmlNode item = month.SelectSingleNode("report[@name='" + strReportName + "']");
                if (item == null)
                {
                    item = month.OwnerDocument.CreateElement("report");
                    month.AppendChild(item);
                    DomUtil.SetAttr(item, "name", strReportName);
                }
                return item;
#endif
                return NewLeafElement(month, strReportName, strReportType);
            }

            Debug.Assert(strTime.Length >= 8, "");
            string strDay = strTime.Substring(0, 8);
            XmlNode day = month.SelectSingleNode("dir[@name='" + strDay + "']");
            if (day == null)
            {
                day = root.OwnerDocument.CreateElement("dir");
                month.AppendChild(day);
                DomUtil.SetAttr(day, "name", strDay);
                DomUtil.SetAttr(day, "type", "day");
            }

            if (strTimeType == "day")
            {
#if NO
                XmlNode item = day.SelectSingleNode("report[@name='" + strReportName + "']");
                if (item == null)
                {
                    item = day.OwnerDocument.CreateElement("report");
                    day.AppendChild(item);
                    DomUtil.SetAttr(item, "name", strReportName);
                }
                return item;
#endif
                return NewLeafElement(day, strReportName, strReportType);
            }

            throw new ArgumentException("未知的 strTimeType 值 '"+strTimeType+"'", "strTimeType");
        }

        // 根据 index.xml 文件创建 index.html 文件
        // return:
        //      -1  出错
        //      0   没有创建。因为 index.xml 文件不存在
        //      1   创建成功
        int CreateIndexHtmlFile(string strIndexXmlFileName,
            string strIndexHtmlFileName,
            out string strError)
        {
            strError = "";

            if (File.Exists(strIndexXmlFileName) == false)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strIndexXmlFileName);
            }
            catch (Exception ex)
            {
                strError = "装入文件 " + strIndexXmlFileName + " 时出错: " + ex.Message;
                return -1;
            }

            
            StringBuilder text = new StringBuilder(4096);

            text.Append("<html><body>");

            XmlNode dir = dom.DocumentElement;  // TODO: 也可以用 <dir> 元素的上级
            OutputHtml(dir as XmlElement, text);

            text.Append("</body></html>");

            WriteToOutputFile(strIndexHtmlFileName,
                text.ToString(),
                Encoding.UTF8);
            // index.html 不要上传
            // File.SetAttributes(strIndexHtmlFileName, FileAttributes.Archive);
            return 1;
        }

        string _cssTemplate = "";

        // TODO: 需要用缓存优化
        // 将一个统计文件条目写入到 131 子目录中的 index.xml 的 DOM 中
        // parameters:
        //      strOutputDir    index.xml 所在目录
        int Write_131_IndexXml(
            string strDepartment,
            string strPersonName,
            string strPatronBarcode,
            string strOutputDir,
            string strReportFileName,
            out string strError)
        {
            strError = "";

            string strFileName = Path.Combine(strOutputDir, "index.xml");

            XmlDocument index_dom = this._indexCache.FindObject(strFileName);
            if (index_dom == null)
            {
                index_dom = new XmlDocument();
                if (File.Exists(strFileName) == true)
                {
                    try
                    {
                        index_dom.Load(strFileName);
                    }
                    catch (Exception ex)
                    {
                        strError = "装入文件 " + strFileName + " 时出错: " + ex.Message;
                        return -1;
                    }
                }
                else
                {
                    index_dom.LoadXml("<root />");
                }
                this._indexCache.SetObject(strFileName, index_dom);
            }

            // 创建一个 index.xml 中的 item 元素
            XmlNode item = Create_131_ItemNode(index_dom.DocumentElement,
                strDepartment,
                strPersonName + "-" + strPatronBarcode);
            Debug.Assert(item != null, "");
            DomUtil.SetAttr(item, "type", "131");

            string strNewFileName = "." + strReportFileName.Substring(strOutputDir.Length);

            // 创建 .html 文件
            if ((this._fileType & FileType.HTML) != 0)
            {
                string strHtmlFileName = Path.Combine(Path.GetDirectoryName(strReportFileName), Path.GetFileNameWithoutExtension(strReportFileName) + ".html");
                int nRet = Report.RmlToHtml(strReportFileName,
                    strHtmlFileName,
                    this._cssTemplate,
                    out strError);
                if (nRet == -1)
                    return -1;
                RemoveArchiveAttribute(strHtmlFileName);
            }

            // 创建 Excel 文件
            if ((this._fileType & FileType.Excel) != 0)
            {
                string strExcelFileName = Path.Combine(Path.GetDirectoryName(strReportFileName), Path.GetFileNameWithoutExtension(strReportFileName) + ".xlsx");
                int nRet = Report.RmlToExcel(strReportFileName,
                    strExcelFileName,
                    out strError);
                if (nRet == -1)
                    return -1;
                RemoveArchiveAttribute(strExcelFileName);
            }

            // 删除以前的对照关系
            string strOldFileName = DomUtil.GetAttr(item, "link");
            if (string.IsNullOrEmpty(strOldFileName) == false)
            // && PathUtil.IsEqual(strOldFileName, strHtmlFileName) == false
            {
                string strOldPhysicalPath = GetRealPath(strOutputDir,
    strOldFileName);

                if (PathUtil.IsEqual(strOldPhysicalPath, strReportFileName) == false
                && File.Exists(strOldPhysicalPath) == true)
                {
                    try
                    {

                        File.Delete(strOldPhysicalPath);
                    }
                    catch
                    {
                    }
                }
            }

            DomUtil.SetAttr(item, "link", strNewFileName.Replace("\\", "/"));
#if DEBUG
            // 检查文件是否存在
            if (File.Exists(strReportFileName) == false)
            {
                strError = "文件 '" + strReportFileName + "' 不存在";
                return -1;
            }
#endif

            // index_dom.Save(strFileName);
            ShrinkIndexCache(false);

            // 131 目录的 index.html 不要上传
            // File.SetAttributes(strFileName, FileAttributes.Archive);
            return 0;
        }

        // 创建一个 index.xml 中的 report 元素
        static XmlNode CreateReportNode(XmlNode root,
            string strGroupName,
            string strReportName,
            bool bAdd)
        {
            XmlNode start = root;
            if (string.IsNullOrEmpty(strGroupName) == false)
            {
                XmlNode group = null;
                if (bAdd == false)
                {
                    group = CreateDirNode(root, strGroupName, 0);
                    if (group == null)
                        return null;
                }
                else
                {
                    group = CreateDirNode(root, strGroupName, 1);
                    DomUtil.SetAttr(group, "type", "group");
                }
                start = group;
            }

            XmlNode item = start.SelectSingleNode("report[@name='" + strReportName + "']");
            if (bAdd == false)
            {
                if (item != null)
                    item.ParentNode.RemoveChild(item);
                return item;
            }
            if (item == null)
            {
                item = start.OwnerDocument.CreateElement("report");
                start.AppendChild(item);
                DomUtil.SetAttr(item, "name", strReportName);
            }

            return item;
        }

        // parameters:
        //      nAdd    -1: delete 0: detect  1: add
        static XmlNode CreateDirNode(XmlNode root,
            string strDirName,
            int nAdd)
        {
            XmlNode dir = root.SelectSingleNode("dir[@name='" + strDirName + "']");
            if (nAdd == 0)
                return dir;
            if (nAdd == -1)
            {
                if (dir != null)
                    dir.ParentNode.RemoveChild(dir);
                return dir;
            }
            Debug.Assert(nAdd == 1, "");
            if (dir == null)
            {
                dir = root.OwnerDocument.CreateElement("dir");
                root.AppendChild(dir);
                DomUtil.SetAttr(dir, "name", strDirName);
            }

            return dir;
        }

        // 创建一个 131 类型的 index.xml 中的 item 元素
        static XmlNode Create_131_ItemNode(XmlNode root,
            string strDepartment,
            string strReportName)
        {
            XmlNode department = null;

            if (string.IsNullOrEmpty(strDepartment) == false)
            {
                department = root.SelectSingleNode("dir[@name='" + strDepartment + "']");
                if (department == null)
                {
                    department = root.OwnerDocument.CreateElement("dir");
                    root.AppendChild(department);
                    DomUtil.SetAttr(department, "name", strDepartment);
                }
            }
            else
            {
                department = root;
                // strDepartment 如果为空,则不用创建 dir 元素了
            }

            XmlNode item = department.SelectSingleNode("report[@name='" + strReportName + "']");
            if (item == null)
            {
                item = department.OwnerDocument.CreateElement("report");
                department.AppendChild(item);
                DomUtil.SetAttr(item, "name", strReportName);
            }
            return item;
        }


        static string GetDisplayTimeString(string strTime)
        {
            if (strTime.Length == 8)
                return strTime.Substring(0, 4) + "." + strTime.Substring(4, 2) + "." + strTime.Substring(6, 2);
            if (strTime.Length == 6)
                return strTime.Substring(0, 4) + "." + strTime.Substring(4, 2);

            return strTime;
        }

        void OutputHtml(
            XmlElement dir,
            StringBuilder text)
        {

            {
                string strLink = dir.GetAttribute("link");
                string strDirName = GetDisplayTimeString(dir.GetAttribute("name"));
                if (string.IsNullOrEmpty(strLink) == true)
                {
                    text.Append("<div>");
                    text.Append(HttpUtility.HtmlEncode(strDirName));
                    text.Append("</div>");
                }
                else
                {
                    text.Append("<li>");
                    text.Append("<a href='" + strLink + "' >");
                    text.Append(HttpUtility.HtmlEncode(strDirName) + " ...");
                    text.Append("</a>");
                    text.Append("</li>");
                    return;
                }
            }

            text.Append("<ul>");

            XmlNodeList reports = dir.SelectNodes("report");
            foreach (XmlElement report in reports)
            {
                string strName = report.GetAttribute("name");
                string strLink = report.GetAttribute("link");

                // link 加工为 .html 形态
                if ((this._fileType & FileType.HTML) != 0)
                {
                    strLink = Path.Combine(Path.GetDirectoryName(strLink), Path.GetFileNameWithoutExtension(strLink) + ".html");
                }

                text.Append("<li>");

                text.Append("<a href='" + strLink + "' >");
                text.Append(HttpUtility.HtmlEncode(strName));
                text.Append("</a>");

                text.Append("</li>");
            }

            XmlNodeList dirs = dir.SelectNodes("dir");
            foreach (XmlElement sub_dir in dirs)
            {
                OutputHtml(sub_dir, text);
            }

            text.Append("</ul>");
        }

        #endregion

        private void toolStripButton_setReportEndDay_Click(object sender, EventArgs e)
        {
            // string strError = "";

            // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
            string strLastDate = this.MainForm.AppInfo.GetString(GetReportSection(),
"daily_report_end_date",
"20130101");

            REDO:
            strLastDate = InputDlg.GetInput(
    this,
    "设置报表最末日期",
    "报表的最末日期: ",
    strLastDate,
    this.MainForm.DefaultFont);
            if (strLastDate == null)
                return;

            if (string.IsNullOrEmpty(strLastDate) == true
                || strLastDate.Length != 8
                || StringUtil.IsNumber(strLastDate) == false)
            {
                MessageBox.Show(this, "所输入的日期字符串 '"+strLastDate+"' 不合法，应该是 8 字符的时间格式。请重新输入");
                goto REDO;
            }

            // 这个日期是上次处理完成的那一天的后一天，也就是说下次处理，从这天开始即可
            this.MainForm.AppInfo.SetString(GetReportSection(),
    "daily_report_end_date",
    strLastDate);
            SetDailyReportButtonState();
            return;
#if NO
        ERROR1:
            MessageBox.Show(this, strError);
#endif
        }

        // 根据最大尺寸，一次取得一部分文件名
        // return:
        //      -1   出错
        //      其它  part_filenames 中包含的文字的总尺寸
        long GetPartFileNames(ref List<string> filenames,
            long lMaxSize,
            out List<string> part_filenames,
            out string strError)
        {
            strError = "";
            part_filenames = new List<string>();

            long lTotalSize = 0;
            for(int i = 0;i<filenames.Count; i++)
            {
                string strFileName = filenames[i];
                FileInfo fi = new FileInfo(strFileName);
                if (lTotalSize + fi.Length > lMaxSize
                    && part_filenames.Count > 0)
                    return lTotalSize;
                lTotalSize += fi.Length;
                part_filenames.Add(strFileName);
                filenames.RemoveAt(0);
                i--;
            }

            return lTotalSize;
        }

        // dp2Library 协议上传报表
        void UploadReportByDp2library()
        {
            string strError = "";
            int nRet = 0;

#if SN
            nRet = this.MainForm.VerifySerialCode("report", false, out strError);
            if (nRet == -1)
            {
                strError = "上传报表功能尚未被许可";
                goto ERROR1;
            }
#endif

            string strReportDir = Path.Combine(GetBaseDirectory(), "reports");
            string strZipFileName = Path.Combine(GetBaseDirectory(), "reports.zip");

            string strServerFileName = "!upload/reports/reports.zip";

            DirectoryInfo di = new DirectoryInfo(strReportDir);
            if (di.Exists == false)
            {
                strError = "报表尚未创建，无法上传";
                goto ERROR1;
            }

            List<string> filenames = null;
            long lZipFileLength = 0;
            long lUnzipFileLength = 0;
            long lUploadedFiles = 0;

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在上传报表 ...");
            stop.BeginLoop();

            try
            {
                if (stop != null)
                    stop.SetMessage("正在搜集文件名 ...");

                Application.DoEvents();

                // 获得全部文件名
                filenames = GetFileNames(strReportDir, FileAttributes.Archive);
                if (filenames.Count == 0)
                {
                    strError = "没有发现需要上传的文件";
                    goto NOT_FOUND;
                }

                long lTotalFiles = filenames.Count;

                // 分批进行上传
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    if (filenames.Count == 0)
                        break;

                    List<string> part_filenames = null;

                    // 取得一部分文件名
                    // 根据最大尺寸，一次取得一部分文件名
                    // return:
                    //      -1   出错
                    //      其它  part_filenames 中包含的文字的总尺寸
                    long lRet = GetPartFileNames(ref filenames,
            5 * 1024 * 1024,
            out part_filenames,
            out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    lUnzipFileLength += lRet;

                    if (part_filenames.Count == 0)
                        break;

                    // return:
                    //      -1  出错
                    //      0   没有发现需要上传的文件
                    //      1   成功压缩创建了 .zip 文件
                    nRet = CompressReport(
                        stop,
                        strReportDir,
                        strZipFileName,
                        Encoding.UTF8,
                        part_filenames,
                        // out filenames,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                    {
                        Debug.Assert(false, "");
                        strError = "没有发现需要上传的文件";
                        goto ERROR1;
                    }

                    FileInfo fi = new FileInfo(strZipFileName);
                    lZipFileLength += fi.Length;

                    stop.SetProgressRange(0, lTotalFiles);
                    stop.SetProgressValue(lUploadedFiles);

                    // return:
                    //		-1	出错
                    //		0   上传文件成功
                    nRet = UploadFile(
                        this.stop,
                        this.Channel,
                        strZipFileName,
                        strServerFileName,
                        "extractzip",
                        null,
                        true,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    bool bDelete = this.DeleteReportFileAfterUpload;
                    foreach (string filename in part_filenames)
                    {
                        if (bDelete == true && IsRmlFileName(filename))
                        {
                            try
                            {
                                File.Delete(filename);
                            }
                            catch
                            {
                            }
                        }
                        else
                            File.SetAttributes(filename, FileAttributes.Normal);
                    }

                    lUploadedFiles += part_filenames.Count;
                    stop.SetProgressValue(lUploadedFiles);
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            // SetUploadButtonState();
            BeginUpdateUploadButtonText();

            this.MainForm.StatusBarMessage = "完成上传 " + lUploadedFiles.ToString() + " 个文件, 总尺寸" + lUnzipFileLength .ToString()+ "，压缩后尺寸 " + lZipFileLength.ToString();
            if (this.DeleteReportFileAfterUpload == true && lUploadedFiles > 0)
                this.MainForm.StatusBarMessage += "。文件上传后，本地文件已经被删除";
            return;
        NOT_FOUND:
            this.MainForm.StatusBarMessage = strError;
            return;
        ERROR1:
            BeginUpdateUploadButtonText();
            MessageBox.Show(this, strError);
        }

        static bool IsRmlFileName(string strFileName)
        {
            if (string.Compare(Path.GetExtension(strFileName), ".rml", true) == 0)
                return true;
            return false;
        }

        void UploadReportByFtp()
        {
            string strError = "";
            int nUploadCount = 0;

            int nRet = 0;
#if SN
            nRet = this.MainForm.VerifySerialCode("report", false, out strError);
            if (nRet == -1)
            {
                strError = "上传报表功能尚未被许可";
                goto ERROR1;
            }
#endif

            FtpUploadDialog dlg = new FtpUploadDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            this.MainForm.AppInfo.LinkFormState(dlg, "FtpUploadDialog_state");
            dlg.UiState = this.MainForm.AppInfo.GetString(GetReportSection(), "FtpUploadDialog_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString(GetReportSection(), "FtpUploadDialog_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            if (stop != null)
                stop.SetMessage("正在搜集文件名 ...");

            Application.DoEvents();

            string strReportDir = Path.Combine(GetBaseDirectory(), "reports");
            List<string> filenames = null;

            bool bDelete = this.DeleteReportFileAfterUpload;

            Application.DoEvents();

            filenames = GetFileNames(strReportDir, FileAttributes.Archive);

            if (filenames.Count == 0)
            {
                strError = "没有发现要上传的报表文件";
                goto ERROR1;
            }

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在上传报表 ...");
            stop.BeginLoop();

            try
            {
                Hashtable dir_table = new Hashtable();

                stop.SetProgressRange(0, filenames.Count);
                int i = 0;
                foreach (string filename in filenames)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    // string strPath = Path.GetDirectoryName(filename.Substring(strReportDir.Length + 1));
                    string strPath = filename.Substring(strReportDir.Length + 1);

                    stop.SetMessage("正在上传 " + filename);

                    // 上传文件
                    // 自动创建所需的目录
                    // 不会抛出异常
                    nRet = FtpUploadDialog.SafeUploadFile(ref dir_table,
            filename,
            dlg.FtpServerUrl,
            (string.IsNullOrEmpty(dlg.TargetDir) == false ? dlg.TargetDir + "/" : "")
            + strPath,
            dlg.UserName,
            dlg.Password,
            out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (bDelete == true && IsRmlFileName(filename))
                    {
                        try
                        {
                            File.Delete(filename);
                        }
                        catch
                        {
                        }
                    }
                    else 
                        File.SetAttributes(filename, FileAttributes.Normal);

                    i++;
                    stop.SetProgressValue(i);

                    nUploadCount++;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            if (nUploadCount > 0)
            {
                // SetUploadButtonState();
                BeginUpdateUploadButtonText();
            }
            this.MainForm.StatusBarMessage = "完成上传 " + nUploadCount.ToString() + " 个文件";
            if (this.DeleteReportFileAfterUpload == true && nUploadCount > 0)
                this.MainForm.StatusBarMessage += "。文件上传后，本地文件已经被删除";
            return;
        ERROR1:
            if (nUploadCount > 0)
            {
                // SetUploadButtonState();
                BeginUpdateUploadButtonText();
            } 
            MessageBox.Show(this, strError);
        }

        // 上传报表
        private void button_start_uploadReport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.comboBox_start_uploadMethod.Text) == true
                || this.comboBox_start_uploadMethod.Text == "dp2Library")
                UploadReportByDp2library();
            else if (this.comboBox_start_uploadMethod.Text == "FTP")
                UploadReportByFtp();
            else
                MessageBox.Show(this, "未知的上传方式 '"+this.comboBox_start_uploadMethod.Text+"'");
        }

        // 上传文件到到 dp2lbrary 服务器
        // parameters:
        //      timestamp   时间戳。如果为 null，函数会自动根据文件信息得到一个时间戳
        //      bRetryOverwiteExisting   是否自动在时间戳不一致的情况下覆盖已经存在的服务器文件。== true，表示当发现时间戳不一致的时候，自动用返回的时间戳重试覆盖
        // return:
        //		-1	出错
        //		0   上传文件成功
        static int UploadFile(
            Stop stop,
            LibraryChannel channel,
            string strClientFilePath,
            string strServerFilePath,
            string strStyle,
            byte[] timestamp,
            bool bRetryOverwiteExisting,
            // string strMessagePrefix,
            out string strError)
        {
            strError = "";

            string strResPath = strServerFilePath;

            string strMime = API.MimeTypeFrom(ResObjectDlg.ReadFirst256Bytes(strClientFilePath),
"");

            // 检测文件尺寸
            FileInfo fi = new FileInfo(strClientFilePath);
            if (fi.Exists == false)
            {
                strError = "文件 '" + strClientFilePath + "' 不存在...";
                return -1;
            }

            string[] ranges = null;

            if (fi.Length == 0)
            {
                // 空文件
                ranges = new string[1];
                ranges[0] = "";
            }
            else
            {
                string strRange = "";
                strRange = "0-" + Convert.ToString(fi.Length - 1);

                // 按照100K作为一个chunk
                // TODO: 实现滑动窗口，根据速率来决定chunk尺寸
                ranges = RangeList.ChunkRange(strRange,
                    500 * 1024);
            }

            if (timestamp == null)
                timestamp = FileUtil.GetFileTimestamp(strClientFilePath);

            byte[] output_timestamp = null;

            // REDOWHOLESAVE:
            string strWarning = "";

            TimeSpan old_timeout = channel.Timeout;
            try
            {

                for (int j = 0; j < ranges.Length; j++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strWaiting = "";
                    if (j == ranges.Length - 1)
                    {
                        strWaiting = " 请耐心等待...";
                        channel.Timeout = new TimeSpan(0, 40, 0);   // 40 分钟

                    }

                    string strPercent = "";
                    RangeList rl = new RangeList(ranges[j]);
                    if (rl.Count >= 1)
                    {
                        double ratio = (double)((RangeItem)rl[0]).lStart / (double)fi.Length;
                        strPercent = String.Format("{0,3:N}", ratio * (double)100) + "%";
                    }

                    if (stop != null)
                        stop.SetMessage( // strMessagePrefix + 
                            "正在上载 " + ranges[j] + "/"
                            + Convert.ToString(fi.Length)
                            + " " + strPercent + " " + strClientFilePath + strWarning + strWaiting);
                    int nRedoCount = 0;
                REDO:
                    long lRet = channel.SaveResObject(
                        stop,
                        strResPath,
                        strClientFilePath,
                        strClientFilePath,
                        strMime,
                        ranges[j],
                        // j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
                        timestamp,
                        strStyle,
                        out output_timestamp,
                        out strError);
                    timestamp = output_timestamp;

                    strWarning = "";

                    if (lRet == -1)
                    {
                        // 如果是第一个 chunk，自动用返回的时间戳重试一次覆盖
                        if (bRetryOverwiteExisting == true
                            && j == 0
                            && channel.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCode.TimestampMismatch
                            && nRedoCount == 0)
                        {
                            nRedoCount++;
                            goto REDO;
                        }
                        goto ERROR1;
                    }
                }
            }
            finally
            {
                channel.Timeout = old_timeout;
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 把报表子目录中的文件压缩到一个 .zip 文件中
        // parameters:
        //      strReportDir    最后不要带有符号 '/'
        // return:
        //      -1  出错
        //      0   没有发现需要上传的文件
        //      1   成功压缩创建了 .zip 文件
        static int CompressReport(
            Stop stop,
            string strReportDir,
            string strZipFileName,
            Encoding encoding,
            List<string> filenames,
            // out List<string> filenames,
            out string strError)
        {
            strError = "";

#if NO
            if (stop != null)
                stop.SetMessage("正在搜集文件名 ...");

            Application.DoEvents();

            filenames = GetFileNames(strReportDir, FileAttributes.Archive);
#endif

            if (filenames.Count == 0)
                return 0;

            bool bRangeSetted = false;
            using (ZipFile zip = new ZipFile(encoding))
            {
                foreach (string filename in filenames)
                {
                    Application.DoEvents();

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    string strShortFileName = filename.Substring(strReportDir.Length + 1);
                    if (stop != null)
                        stop.SetMessage("正在压缩 " + strShortFileName);
                    string directoryPathInArchive = Path.GetDirectoryName(strShortFileName);
                    zip.AddFile(filename, directoryPathInArchive);
                }

                if (stop != null)
                    stop.SetMessage("正在写入压缩文件 ...");

                Application.DoEvents();

                zip.SaveProgress += (s, e) =>
                    {
                        Application.DoEvents();
                        if (stop != null && stop.State != 0)
                        {
                            e.Cancel = true;
                            return;
                        }

                        if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                        {
                            if (bRangeSetted == false)
                            {
                                stop.SetProgressRange(0, e.EntriesTotal);
                                bRangeSetted = true;
                            }

                            stop.SetProgressValue(e.EntriesSaved);
                        }
                    };

                zip.UseZip64WhenSaving = Zip64Option.AsNecessary;
                zip.Save(strZipFileName);

                stop.HideProgress();

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }
            }

            return 1;
        }


        // 获得一个目录下的全部文件名。包括子目录中的
        static List<string> GetFileNames(string strDataDir,
            FileAttributes attr)
        {
            // Application.DoEvents();

            DirectoryInfo di = new DirectoryInfo(strDataDir);

            List<string> result = new List<string>();

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if ((fi.Attributes & attr) == attr)
                {
                    string strExtention = Path.GetExtension(fi.Name).ToLower();
                    if (strExtention == ".xml" || strExtention == ".rml")
                        result.Add(fi.FullName);
                }
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                result.AddRange(GetFileNames(subdir.FullName, attr));
            }

            return result;
        }

        private void timer_qu_Tick(object sender, EventArgs e)
        {
            this.listView_query_results.ForceUpdate();
        }

        private void listView_query_results_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_query_results.SelectedIndices.Count == 0)
                this.MainForm.StatusBarMessage = "";
            else
                this.MainForm.StatusBarMessage = this.listView_query_results.SelectedIndices[0].ToString();
        }

        /// <summary>
        /// 获取或设置控件尺寸状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl1);
                controls.Add(this.listView_libraryConfig);
                controls.Add(this.splitContainer_query);
                controls.Add(this.comboBox_start_uploadMethod);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.tabControl1);
                controls.Add(this.listView_libraryConfig);
                controls.Add(this.splitContainer_query);
                controls.Add(this.comboBox_start_uploadMethod);
                GuiState.SetUiState(controls, value);
            }
        }

        // 获得一个目录下的全部 .rml 文件名。包括子目录中的
        static List<string> GetRmlFileNames(string strDataDir)
        {
            Application.DoEvents();

            DirectoryInfo di = new DirectoryInfo(strDataDir);

            List<string> result = new List<string>();

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                    string strExtention = Path.GetExtension(fi.Name).ToLower();
                    if (strExtention == ".rml")
                        result.Add(fi.FullName);
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                result.AddRange(GetRmlFileNames(subdir.FullName));
            }

            return result;
        }

        private void toolStripButton_convertFormat_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            ConvertReportFormatDialog dlg = new ConvertReportFormatDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            this.MainForm.AppInfo.LinkFormState(dlg, "ConvertReportFormatDialog_state");
            dlg.UiState = this.MainForm.AppInfo.GetString(GetReportSection(), "ConvertReportFormatDialog_ui_state", "");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.SetString(GetReportSection(), "ConvertReportFormatDialog_ui_state", dlg.UiState);
            this.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            string strTemplate = "";

            if (dlg.ToHtml == true)
            {
                nRet = LoadHtmlTemplate(null,   // nodeLibrary,
                    out strTemplate,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            int nCount = 0;
            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在转换格式 ...");
            stop.BeginLoop();

            try
            {

                if (stop != null)
                    stop.SetMessage("正在搜集文件名 ...");

                Application.DoEvents();

                List<string> filenames = GetRmlFileNames(dlg.ReportDirectory);

                if (filenames.Count == 0)
                {
                    strError = "所指定的目录 '" + dlg.ReportDirectory + "' 中没有任何 .tml 文件";
                    goto ERROR1;
                }

                foreach (string strFileName in filenames)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    if (stop != null)
                        stop.SetMessage("正在转换文件 " + strFileName);

                    // 创建 .html 文件
                    if (dlg.ToHtml == true)
                    {
                        string strHtmlFileName = Path.Combine(Path.GetDirectoryName(strFileName), Path.GetFileNameWithoutExtension(strFileName) + ".html");
                        nRet = Report.RmlToHtml(strFileName,
                            strHtmlFileName,
                            strTemplate,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        RemoveArchiveAttribute(strHtmlFileName);
                        nCount++;
                    }

                    // 创建 Excel 文件
                    if (dlg.ToExcel == true)
                    {
                        string strExcelFileName = Path.Combine(Path.GetDirectoryName(strFileName), Path.GetFileNameWithoutExtension(strFileName) + ".xlsx");
                        nRet = Report.RmlToExcel(strFileName,
                            strExcelFileName,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        RemoveArchiveAttribute(strExcelFileName);
                        nCount++;
                    }
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
            }

            MessageBox.Show(this, "成功转换文件 " + nCount.ToString() + " 个");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        FileCounting _counting = null;

        // 启动更新 上载报表 按钮文字的线程
        void BeginUpdateUploadButtonText()
        {
            if (this._counting == null)
            {
                this._counting = new FileCounting();
                this._counting.ReportForm = this;
                this._counting.Directory = Path.Combine(GetBaseDirectory(), "reports");
            }
            this._counting.BeginThread();
        }

        void StopUpdateUploadButtonText()
        {
            if (this._counting != null)
                this._counting.StopThread(true);
        }

        private void tabPage_option_Enter(object sender, EventArgs e)
        {
            // Debug.WriteLine("Enter page");
            this.checkBox_option_deleteReportFileAfterUpload.Checked = this.MainForm.AppInfo.GetBoolean(GetReportSection(),
                "deleteReportFileAfterUpload",
                true);
        }

        private void tabPage_option_Leave(object sender, EventArgs e)
        {
            // Debug.WriteLine("Leave page");
            this.MainForm.AppInfo.SetBoolean(GetReportSection(),
                "deleteReportFileAfterUpload",
                this.checkBox_option_deleteReportFileAfterUpload.Checked);
        }

        /// <summary>
        /// 是否要在上载成功后自动删除本地报表文件
        /// </summary>
        public bool DeleteReportFileAfterUpload
        {
            get
            {
                return this.MainForm.AppInfo.GetBoolean(GetReportSection(),
    "deleteReportFileAfterUpload",
    true);
            }
        }

        private void listView_query_results_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_queryResult_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制  [" + this.listView_query_results.SelectedItems.Count.ToString() + "] (&C)");
            menuItem.Click += new System.EventHandler(this.menu_queryResult_copyToClipboard_Click);
            if (this.listView_query_results.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("保存到 Excel 文件 [" + this.listView_query_results.SelectedItems.Count.ToString() + "] (&S) ...");
            menuItem.Click += new System.EventHandler(this.menu_queryResult_saveToExcel_Click);
            if (this.listView_query_results.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_query_results, new Point(e.X, e.Y));		

        }

        void menu_queryResult_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView_query_results);
        }

        string _exportExcelFileName = "";

        void menu_queryResult_saveToExcel_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_query_results.SelectedItems.Count == 0)
            {
                strError = "尚未选择要保存的行";
                goto ERROR1;
            }

            // 询问文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的 Excel 文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = _exportExcelFileName;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "Excel 文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            _exportExcelFileName = dlg.FileName;

            this.EnableControls(false);
            try
            {
                ExcelDocument doc = ExcelDocument.Create(dlg.FileName);
                try
                {
                    doc.NewSheet("Sheet1");

                    int nColIndex = 0;
                    int _lineIndex = 0;

                    // 姓名
                    List<CellData> cells = new List<CellData>();

                    foreach (ColumnHeader column in this.listView_query_results.Columns)
                    {
                        cells.Add(new CellData(nColIndex++, column.Text));

                    }

                    doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.None);
                    _lineIndex++;

                    foreach (ListViewItem item in this.listView_query_results.SelectedItems)
                    {
                        cells = new List<CellData>();
                        nColIndex = 0; 
                        foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                        {
                            cells.Add(new CellData(nColIndex++, subitem.Text));
                        }
                        doc.WriteExcelLine(_lineIndex, cells, WriteExcelLineStyle.AutoString);
                        _lineIndex++;
                    }
                }
                finally
                {
                    doc.SaveWorksheet();
                    doc.Close();
                }
            }
            finally
            {
                this.EnableControls(true);
            }

            this.MainForm.StatusBarMessage = "导出成功。";
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_queryResult_copyToClipboard_Click(object sender, EventArgs e)
        {
            Global.CopyLinesToClipboard(this, this.listView_query_results, false);
        }
    }

    class FileCounting : ThreadBase
    {
        public string Directory = "";
        public ReportForm ReportForm = null;

        internal ReaderWriterLock m_lock = new ReaderWriterLock();
        internal static int m_nLockTimeout = 5000;	// 5000=5秒

        // 工作线程每一轮循环的实质性工作
        public override void Worker()
        {
            this.m_lock.AcquireWriterLock(m_nLockTimeout);
            try
            {
                if (this.Stopped == true)
                    return;

                this.ReportForm.SetUploadButtonText("上传报表 ?", "false");

                string strReportDir = this.Directory;

                DirectoryInfo di = new DirectoryInfo(strReportDir);
                if (di.Exists == false)
                {
                    // 报表目录不存在
                    this.ReportForm.SetUploadButtonText("上传报表", "false");
                    return;
                }

                long lCount = CountFiles(this.Directory, FileAttributes.Archive);

                this.ReportForm.SetUploadButtonText("上传报表 (" + lCount.ToString() + ")",
                    "true");

                this.Stopped = true;   // 只作一轮就停止
            }
            finally
            {
                this.m_lock.ReleaseWriterLock();
            }
        }

        // 获得一个目录下的全部文件数目。包括子目录中的
        long CountFiles(string strDataDir, FileAttributes attr)
        {
            DirectoryInfo di = new DirectoryInfo(strDataDir);

            long lCount = 0;
            List<string> result = new List<string>();

            FileInfo[] fis = di.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (this.Stopped == true)
                    return lCount;

                if ((fi.Attributes & attr) == attr)
                {
                    // Thread.Sleep(1); 测试用
                    string strExtention = Path.GetExtension(fi.Name).ToLower();
                    if (strExtention == ".xml" || strExtention == ".rml")
                        lCount++;
                }
            }

            // 处理下级目录，递归
            DirectoryInfo[] dis = di.GetDirectories();
            foreach (DirectoryInfo subdir in dis)
            {
                if (this.Stopped == true)
                    return lCount;
                lCount += CountFiles(subdir.FullName, attr);
            }

            return lCount;
        }

    }

    class OneClassType
    {
        /// <summary>
        /// 分类号 type。例如 clc
        /// </summary>
        public string ClassType = "";
        /// <summary>
        /// 类名细目。不允许元素为空字符串。如果 .Count == 0 ，表示不使用细目，统一截取第一个字符作为细目
        /// </summary>
        public List<string> Filters = new List<string>();

        // 根据名字表创建分类号分级结构
        /* 名字表形态如下
         * clc
         *  A
         *  B
         * stt
         * nhb
         * */
        public static int BuildClassTypes(string strNameTable,
            out List<OneClassType> results,
            out string strError)
        {
            strError = "";

            results = new List<OneClassType>();
            List<string> lines = StringUtil.SplitList(strNameTable);
            OneClassType current = null;
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) == true)
                    continue;
                string strLine = line.TrimEnd();
                if (string.IsNullOrEmpty(strLine) == true)
                    continue;
                if (strLine[0] != ' ')
                {
                    current = new OneClassType();
                    results.Add(current);
                    current.ClassType = strLine.Trim();
                }
                else
                {
                    if (current == null)
                    {
                        strError = "第一行的第一字符不能为空格";
                        return -1;
                    }
                    string strText = strLine.Substring(1).Trim();
                    current.Filters.Add(strText);
                }
            }

            // 将 Filters 数组排序，大的在前
            // 这样让 I1 比 I 先匹配上
            foreach (OneClassType type in results)
            {
                type.Filters.Sort(
                    delegate(string s1, string s2)
                    {
                        return -1 * string.Compare(s1, s2);
                    });
            }

            return 0;
        }

        public static int IndexOf(List<OneClassType> types, string strTypeName)
        {
            int i = 0;
            foreach (OneClassType type in types)
            {
                if (type.ClassType == strTypeName)
                    return i;
                i++;
            }
            return -1;
        }
    }
}
