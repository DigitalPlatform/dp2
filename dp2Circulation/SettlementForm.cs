using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 结算窗
    /// </summary>
    public partial class SettlementForm : MyForm
    {
        const int WM_LOADSIZE = API.WM_USER + 201;

        // 图标下标
        const int ITEMTYPE_AMERCED = 0;
        const int ITEMTYPE_NEWLY_SETTLEMENTED = 1;
        const int ITEMTYPE_OLD_SETTLEMENTED = 2;
        const int ITEMTYPE_UNKNOWN = 3;

        // 列的位置
        const int COLUMN_ID = 0;
        const int COLUMN_STATE = 1;
        const int COLUMN_READERBARCODE = 2;
        const int COLUMN_LIBRARYCODE = 3;
        const int COLUMN_PRICE = 4;
        const int COLUMN_COMMENT = 5;
        const int COLUMN_REASON = 6;
        const int COLUMN_BORROWDATE = 7;
        const int COLUMN_BORROWPERIOD = 8;
        const int COLUMN_RETURNDATE = 9;
        const int COLUMN_RETURNOPERATOR = 10;
        const int COLUMN_BARCODE = 11;
        const int COLUMN_SUMMARY = 12;
        const int COLUMN_AMERCEOPERATOR = 13;
        const int COLUMN_AMERCETIME = 14;
        const int COLUMN_SETTLEMENTOPERATOR = 15;
        const int COLUMN_SETTLEMENTTIME = 16;

        const int COLUMN_RECPATH = 17;

        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        /// <summary>
        /// 构造函数
        /// </summary>
        public SettlementForm()
        {
            this.UseLooping = true; // 2022/11/4

            InitializeComponent();
        }

        private void SettlementForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            // 起始日期
            this.dateControl_start.Text = Program.MainForm.AppInfo.GetString(
                 "settlementform",
                 "start_date",
                 "");

            // 结束日期
            this.dateControl_end.Text = Program.MainForm.AppInfo.GetString(
                "settlementform",
                "end_date",
                "");

            // 起始索引号
            this.textBox_range_startCtlno.Text = Program.MainForm.AppInfo.GetString(
                "settlementform",
                "start_ctlno",
                "");

            // 结束索引号
            this.textBox_range_endCtlno.Text = Program.MainForm.AppInfo.GetString(
                "settlementform",
                "end_ctlno",
                "");

            // 收费操作时间范围
            this.radioButton_range_amerceOperTime.Checked = Program.MainForm.AppInfo.GetBoolean(
                "settlementform",
                "range_amerceopertime",
                true);

            // 索引号范围
            this.radioButton_range_ctlno.Checked = Program.MainForm.AppInfo.GetBoolean(
                "settlementform",
                "range_ctlno",
                false);

            // 状态
            this.comboBox_range_state.Text = Program.MainForm.AppInfo.GetString(
                "settlementform",
                "range_state",
                "<全部>");

            // 按照收费者小计
            this.checkBox_sumByAmerceOperator.Checked = Program.MainForm.AppInfo.GetBoolean(
                "settlementform",
                "sumby_amerceoperator",
                true);

            API.PostMessage(this.Handle, WM_LOADSIZE, 0, 0);
        }

        private void SettlementForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void SettlementForm_FormClosed(object sender, FormClosedEventArgs e)
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
                // 起始日期
                Program.MainForm.AppInfo.SetString(
                    "settlementform",
                    "start_date",
                    this.dateControl_start.Text);
                // 结束日期
                Program.MainForm.AppInfo.SetString(
                    "settlementform",
                    "end_date",
                    this.dateControl_end.Text);

                // 起始索引号
                Program.MainForm.AppInfo.SetString(
                    "settlementform",
                    "start_ctlno",
                    this.textBox_range_startCtlno.Text);

                // 结束索引号
                Program.MainForm.AppInfo.SetString(
                    "settlementform",
                    "end_ctlno",
                    this.textBox_range_endCtlno.Text);

                Program.MainForm.AppInfo.SetBoolean(
                    "settlementform",
                    "range_amerceopertime",
                    this.radioButton_range_amerceOperTime.Checked);

                Program.MainForm.AppInfo.SetBoolean(
                    "settlementform",
                    "range_ctlno",
                    this.radioButton_range_ctlno.Checked);

                // 状态
                Program.MainForm.AppInfo.SetString(
                    "settlementform",
                    "range_state",
                    this.comboBox_range_state.Text);

                // 按照收费者小计
                Program.MainForm.AppInfo.SetBoolean(
                    "settlementform",
                    "sumby_amerceoperator",
                    this.checkBox_sumByAmerceOperator.Checked);
            }

            SaveSize();
        }

        /*public*/
        void LoadSize()
        {
#if NO
            // 设置窗口尺寸状态
            MainForm.AppInfo.LoadMdiChildFormStates(this,
                "mdi_form_state");
#endif

            string strWidths = Program.MainForm.AppInfo.GetString(
                "settlement_form",
                "amerced_list_column_width",
                "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_amerced,
                    strWidths,
                    true);
            }
        }

        /*public*/
        void SaveSize()
        {
#if NO
            MainForm.AppInfo.SaveMdiChildFormStates(this,
                "mdi_form_state");
#endif

            /*
            // 如果MDI子窗口不是MainForm刚刚准备退出时的状态，恢复它。为了记忆尺寸做准备
            if (this.WindowState != Program.MainForm.MdiWindowState)
                this.WindowState = Program.MainForm.MdiWindowState;
             * */

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_amerced);
            Program.MainForm.AppInfo.SetString(
                "settlement_form",
                "amerced_list_column_width",
                strWidths);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOADSIZE:
                    LoadSize();
                    return;
            }
            base.DefWndProc(ref m);
        }

        // 正规化日期时间。
        static void CanonicalizeDate(ref DateTime time,
            string strStyle)
        {
            if (strStyle == "smallbound")   // 最小边界
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    0, 0, 0, 0);
            }
            else if (strStyle == "largebound") // 最大边界
            {
                time = new DateTime(time.Year, time.Month, time.Day,
                    23, 59, 59, 999);
            }
            else
            {
                throw new Exception("未知的 strStyle值 '" + strStyle + "'");
            }
        }


        // 构造XML检索式
        // parameters:
        //      start_time  起始时间。本地时间。
        //      end_time    结束时间。本地时间。
        // return:
        //      -1  出错
        //      0   违约金库名没有配置
        //      1   成功
        int BuildQueryXml(DateTime start_time,
            DateTime end_time,
            string strState,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            string strDbName = "违约金";
            string strFrom = "缴款时间";

            // 对start_time和end_time进行规范
            CanonicalizeDate(ref start_time,
                "smallbound");
            CanonicalizeDate(ref end_time,
                "largebound");


            string strStartTime = DateTimeUtil.Rfc1123DateTimeString(start_time.ToUniversalTime());
            string strEndTime = DateTimeUtil.Rfc1123DateTimeString(end_time.ToUniversalTime());

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.SetMessage("正在获取违约金库名 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取违约金库名 ...",
                null);

            try
            {
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "amerce",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0
                    || string.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "违约金库名没有配置。";
                    return 0;   // not found
                }
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }

            Debug.Assert(strDbName != "", "");

            strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'>"

            // start
            + "<item><word>"
            + StringUtil.GetXmlStringSimple(strStartTime)
            + "</word><match>left</match><relation>" + StringUtil.GetXmlStringSimple(">=") + "</relation><dataType>number</dataType><maxCount>-1</maxCount></item>"

            + "<operator value='AND' />"
            // end
            + "<item><word>"
            + StringUtil.GetXmlStringSimple(strEndTime)
            + "</word><match>left</match><relation>" + StringUtil.GetXmlStringSimple("<=") + "</relation><dataType>number</dataType><maxCount>-1</maxCount></item>"
            + "<lang>" + this.Lang + "</lang></target>";


            if (String.IsNullOrEmpty(strState) == false
                && strState != "<全部>")
            {
                if (strState == "已收费")
                    strState = "amerced";
                else if (strState == "旧结算"
                    || strState == "已结算")  // TODO: 应当修改为“旧结算”
                    strState = "settlemented";

                string strStateXml = "";
                strStateXml = "<target list='" + strDbName + ":" + "状态" + "'>"
                            + "<item><word>"
                            + StringUtil.GetXmlStringSimple(strState)
                            + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item>"
                            + "<lang>" + this.Lang + "</lang></target>";

                strQueryXml = "<group>" + strQueryXml + "<operator value='AND'/>" + strStateXml + "</group>";
            }


            return 1;
        ERROR1:
            return -1;
        }

        // 构造XML检索式
        // parameters:
        //      strStartCtlno  起始索引号。
        //      strEndCtlno 结束索引号。
        // return:
        //      -1  出错
        //      0   违约金库名没有配置
        //      1   成功
        int BuildQueryXml(string strStartCtlno,
            string strEndCtlno,
            string strState,
            out string strQueryXml,
            out string strError)
        {
            strQueryXml = "";
            strError = "";

            if (String.IsNullOrEmpty(strStartCtlno) == true)
                strStartCtlno = "1";
            if (String.IsNullOrEmpty(strEndCtlno) == true)
                strEndCtlno = "9999999999";

            string strDbName = "违约金";
            string strFrom = "__id";

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.SetMessage("正在获取违约金库名 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取违约金库名 ...",
                null);

            try
            {
                long lRet = channel.GetSystemParameter(
                    looping.Progress,
                    "amerce",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0
                    || string.IsNullOrEmpty(strDbName) == true)
                {
                    strError = "违约金库名没有配置。";
                    return 0;   // not found
                }
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                */
            }

            Debug.Assert(strDbName != "", "");

            strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'>"

            + "<item><word>"
            + strStartCtlno + "-" + strEndCtlno
            + "</word><match>exact</match><relation>" + "draw" + "</relation><dataType>number</dataType><maxCount>-1</maxCount></item>"

            + "<lang>" + this.Lang + "</lang></target>";

            if (String.IsNullOrEmpty(strState) == false
    && strState != "<全部>")
            {
                if (strState == "已收费")
                    strState = "amerced";
                else if (strState == "旧结算"
                    || strState == "已结算")  // 应当修改为“旧结算”
                    strState = "settlemented";

                string strStateXml = "";
                strStateXml = "<target list='" + strDbName + ":" + "状态" + "'>"
                            + "<item><word>"
                            + StringUtil.GetXmlStringSimple(strState)
                            + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item>"
                            + "<lang>" + this.Lang + "</lang></target>";

                strQueryXml = "<group>" + strQueryXml + "<operator value='AND'/>" + strStateXml + "</group>";
            }

            return 1;
        ERROR1:
            return -1;
        }

        int m_nInSearching = 0;

        // 从“违约金”库检索出违约金的记录，并显示在listiview中
        int LoadAmercedRecords(
            bool bQuick,
            string strQueryXml,
            out string strError)
        {
            strError = "";

            this.listView_amerced.Items.Clear();
            // 2008/11/22
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_amerced.Columns);

            this.toolStripStatusLabel_items_message1.Text = "";
            this.toolStripStatusLabel_items_message2.Text = "";

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.SetMessage("正在检索违约金记录 ...");
            _stop.BeginLoop();

            this.EnableControls(false);
            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在检索违约金记录 ...",
                "disableControl");

            this.m_nInSearching++;
            try
            {
                /*
                string strDbName = "违约金";
                string strFrom = "缴款时间";
                string strLang = "zh";
                string strQueryXml = "";
                string strStartTime = DateTimeUtil.Rfc1123DateTimeString(start_time.ToUniversalTime());
                string strEndTime = DateTimeUtil.Rfc1123DateTimeString(end_time.ToUniversalTime());

                long lRet = Channel.GetSystemParameter(
                    stop,
                    "amerce",
                    "dbname",
                    out strDbName,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0)
                {
                    strError = "违约金库名没有配置。";
                    return 0;   // not found
                }

                Debug.Assert(strDbName != "", "");

                strQueryXml = "<target list='" + strDbName + ":" + strFrom + "'>"

                    // start
                + "<item><word>"
                + StringUtil.GetXmlStringSimple(strStartTime)
                + "</word><match>left</match><relation>" + StringUtil.GetXmlStringSimple(">=") + "</relation><dataType>number</dataType><maxCount>-1</maxCount></item>"

                + "<operator value='AND' />"
                    // end
                + "<item><word>"
                + StringUtil.GetXmlStringSimple(strEndTime)
                + "</word><match>left</match><relation>" + StringUtil.GetXmlStringSimple("<=") + "</relation><dataType>number</dataType><maxCount>-1</maxCount></item>"
                + "<lang>" + strLang + "</lang></target>";
                 * */

                long lRet = channel.Search(
                    looping.Progress,
                    strQueryXml,
                    "amerced",
                    "", // strOutputStyle
                    out strError);
                if (lRet == 0)
                {
                    strError = "检索未命中";
                    return 0;   // not found
                }
                if (lRet == -1)
                    goto ERROR1;

                int nLoadCount = 0;

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                Record[] searchresults = null;

                looping.Progress.SetProgressRange(0, lHitCount);

                List<string> access_denied_errors = new List<string>();
                // 获得结果集，装入listview
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    looping.Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = channel.GetSearchResult(
                        looping.Progress,
                        "amerced",   // strResultSetName
                        lStart,
                        lPerCount,
                        "id,xml",   // "id"
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        return 0;
                    }

                    // 处理浏览结果
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        var record = searchresults[i];

                        Application.DoEvents();	// 出让界面控制权

                        if (looping.Stopped)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }

                        bool bTempQuick = Control.ModifierKeys == Keys.Control;

                        string strPath = record.Path;

                        looping.Progress.SetMessage("正在装入违约金记录 " + strPath + " " + (lStart + i + 1).ToString() + " / " + lHitCount.ToString() + " ...");

                        /*
                        lRet = channel.GetRecord(looping.Progress,
                            strPath,
                            out byte[] timestamp,
                            out string strXml,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ErrorCode.AccessDenied)
                            {
                                access_denied_errors.Add(strError);
                                goto CONTINUE;
                            }

                            goto ERROR1;
                        }
                        */
                        string strXml = record.RecordBody?.Xml;
                        if (Global.GetErrorCode(record) == ErrorCodeValue.AccessDenied)
                        {
                            access_denied_errors.Add(strPath + ": " + record.RecordBody?.Result?.ErrorString);
                            goto CONTINUE;
                        }

                        if (record.RecordBody != null 
                            && record.RecordBody.Result != null)
                        {
                            if (record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                            {
                                access_denied_errors.Add(strPath + ": " + record.RecordBody?.Result?.ErrorString);
                                goto CONTINUE;
                            }
                        }

                        if (string.IsNullOrEmpty(strXml))
                        {
                            MessageBox.Show(this, $"警告: 记录 '{strPath}' 的 XML 为空");
                            goto CONTINUE;
                        }


                        int nRet = FillAmercedLine(
                            null,
                            looping.Progress,
                            channel,
                            strXml,
                            strPath,
                            (bQuick == true || bTempQuick == true) ? false : true,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        nLoadCount++;
                    CONTINUE:
                        looping.Progress.SetProgressValue(lStart + i + 1);
                    }

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }

                if (nLoadCount != lHitCount)
                {
                    MessageBox.Show(this, "检索命中 " + lHitCount.ToString() + " 条，实际装入 " + nLoadCount.ToString() + " 条");
                }

                if (access_denied_errors.Count > 0)
                {
                    MessageDlg.Show(this, 
                        $"以下是装入违约金记录时遇到“权限不足”或其它类型报错的信息：\r\n{StringUtil.MakePathList(access_denied_errors, "\r\n")}", 
                        "权限不足");
                }
            }
            finally
            {
                this.m_nInSearching--;
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControls(true);
                */
            }

            OnItemTypeChanged();
            return 1;
        ERROR1:
            return -1;
        }

        // 刷新指定id的事项行
        // parameters:
        //      bPrepareStop    是否准备stop循环状态？如果外部调用前已经准备好了，就需要用false调用
        int RefreshAmercedRecords(
            Stop stop,
            LibraryChannel channel,
            // bool bPrepareStop,
            string[] ids,
            out string strError)
        {
            strError = "";

            /*
            if (bPrepareStop == true)
            {
                _stop.OnStop += new StopEventHandler(this.DoStop);
                _stop.SetMessage("正在刷新违约金记录 ...");
                _stop.BeginLoop();

                this.EnableControls(false);
            }
            */
            stop?.SetMessage("正在刷新违约金记录 ...");
            try
            {
                for (int i = 0; i < ids.Length; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    string strID = ids[i];

                    // 根据id得到记录路径
                    ListViewItem item = GetItemByID(strID);
                    if (item == null)
                    {
                        strError = "id '" + strID + "' 在listview中没有找到对应的item";
                        goto ERROR1;
                    }

                    string strPath = item.SubItems[COLUMN_RECPATH].Text;

                    stop?.SetMessage("正在装入记录信息 " + strPath);
                    long lRet = channel.GetRecord(stop,
                        strPath,
                        out byte[] timestamp,
                        out string strXml,
                        out strError);
                    if (lRet == -1)
                    {
                        goto ERROR1;
                    }

                    int nRet = FillAmercedLine(
                        item,
                        stop,
                        channel,
                        strXml,
                        strPath,
                        true,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                }
            }
            finally
            {
                /*
                if (bPrepareStop == true)
                {
                    _stop.EndLoop();
                    _stop.OnStop -= new StopEventHandler(this.DoStop);
                    _stop.Initial("");

                    this.EnableControls(true);
                }
                */
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 根据ID在listview中查出记录路径
        ListViewItem GetItemByID(string strID)
        {
            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];

                if (item.Text == strID)
                    return item;
            }

            return null;    // not found
        }

        public override void UpdateEnable(bool bEnable)
        {
            this.button_next.Enabled = bEnable;

            /*
            this.button_settlement.Enabled = bEnable;
            this.button_undoSettlement.Enabled = bEnable;
             * */
            this.toolStrip_items.Enabled = bEnable;

            /*
            this.dateControl_start.Enabled = bEnable;
            this.dateControl_end.Enabled = bEnable;
             * 
            this.textBox_range_startCtlno.Enabled = bEnable;
            this.textBox_range_endCtlno.Enabled = bEnable;
             * */
            this.radioButton_range_amerceOperTime.Enabled = bEnable;
            this.radioButton_range_ctlno.Enabled = bEnable;

            SetRangeControlsEnabled(bEnable);

            this.comboBox_range_state.Enabled = bEnable;
        }

        // 获得用于显示用途的状态字符串
        static string GetDisplayStateText(string strState)
        {
            if (strState == "amerced")
                return "已收费";

            if (strState == "settlemented")
                return "新结算";

            return strState;
        }

        // 获得用于存储用途的状态字符串
        static string GetOriginStateText(string strState)
        {
            if (strState == "已收费")
                return "amerced";

            if (strState == "新结算")
                return "settlemented";

            if (strState == "旧结算")   // 2009/1/30
                return "settlemented";


            return strState;
        }

        // 填充一个新的amerced行
        // stop已经被外层BeginLoop()了
        // TODO: Summary获得时出错，最好作为警告而不是错误。
        // parameters:
        //      item    ListView事项。如果为null，表示本函数需要创建新的事项
        int FillAmercedLine(
            ListViewItem item,
            Stop stop,
            LibraryChannel channel,
            string strXml,
            string strRecPath,
            bool bFillSummary,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();

            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装载到DOM时发生错误: " + ex.Message;
                return -1;
            }

            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "itemBarcode");
            string strItemRecPath = DomUtil.GetElementText(dom.DocumentElement, "itemRecPath");
            string strSummary = "";
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "readerBarcode");
            string strLibraryCode = DomUtil.GetElementText(dom.DocumentElement, "libraryCode");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
            string strComment = DomUtil.GetElementText(dom.DocumentElement, "comment");
            string strReason = DomUtil.GetElementText(dom.DocumentElement, "reason");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");

            strBorrowDate = DateTimeUtil.LocalTime(strBorrowDate, "u");

            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            string strReturnDate = DomUtil.GetElementText(dom.DocumentElement, "returnDate");

            strReturnDate = DateTimeUtil.LocalTime(strReturnDate, "u");

            string strID = DomUtil.GetElementText(dom.DocumentElement, "id");
            string strReturnOperator = DomUtil.GetElementText(dom.DocumentElement, "returnOperator");
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");

            strState = GetDisplayStateText(strState);   // 2009/1/29

            string strAmerceOperator = DomUtil.GetElementText(dom.DocumentElement, "operator");
            string strAmerceTime = DomUtil.GetElementText(dom.DocumentElement, "operTime");

            strAmerceTime = DateTimeUtil.LocalTime(strAmerceTime, "u");

            string strSettlementOperator = DomUtil.GetElementText(dom.DocumentElement, "settlementOperator");
            string strSettlementTime = DomUtil.GetElementText(dom.DocumentElement, "settlementOperTime");

            strSettlementTime = DateTimeUtil.LocalTime(strSettlementTime, "u");

            if (bFillSummary == true)
            {
                // stop.OnStop += new StopEventHandler(this.DoStop);
                stop?.SetMessage("正在获取摘要 " + strItemBarcode + " ...");
                // stop.BeginLoop();

                try
                {
                    long lRet = channel.GetBiblioSummary(
                        stop,
                        strItemBarcode,
                        strItemRecPath,
                        null,
                        out string strBiblioRecPath,
                        out strSummary,
                        out strError);
                    if (lRet == -1)
                    {
                        strSummary = strError;
                        // return -1;
                    }

                }
                finally
                {
                    // stop.EndLoop();
                    // stop.OnStop -= new StopEventHandler(this.DoStop);
                    // stop.Initial("");
                }
            }

            string strOldState = null;

            if (item == null)
            {
                item = new ListViewItem(strID, 0);
                this.listView_amerced.Items.Add(item);
                strOldState = null;
            }
            else
            {
                strOldState = item.SubItems[COLUMN_STATE].Text;
                item.SubItems.Clear();
                item.Text = strID;
            }

            item.SubItems.Add(strState);
            item.SubItems.Add(strReaderBarcode);
            item.SubItems.Add(strLibraryCode);
            item.SubItems.Add(strPrice);
            item.SubItems.Add(strComment);
            item.SubItems.Add(strReason);
            item.SubItems.Add(strBorrowDate);
            item.SubItems.Add(strBorrowPeriod);
            item.SubItems.Add(strReturnDate);
            item.SubItems.Add(strReturnOperator);
            item.SubItems.Add(strItemBarcode);
            item.SubItems.Add(strSummary);

            item.SubItems.Add(strAmerceOperator);
            item.SubItems.Add(strAmerceTime);
            item.SubItems.Add(strSettlementOperator);
            item.SubItems.Add(strSettlementTime);

            item.SubItems.Add(strRecPath);

            SetItemIconAndColor(strOldState,
                item);

            return 0;
        }

        static void SetItemIconAndColor(string strOldState,
            ListViewItem item)
        {
            string strState = item.SubItems[COLUMN_STATE].Text;

            if (strState == "amerced"
                || strState == "已收费")
            {
                item.ImageIndex = ITEMTYPE_AMERCED;
                item.BackColor = Color.LightYellow;
                item.ForeColor = SystemColors.WindowText;
            }
            else if (strState == "settlemented"
                || strState == "新结算"
                || strState == "旧结算")
            {
                if (strOldState == null)
                {
                    item.ImageIndex = ITEMTYPE_OLD_SETTLEMENTED;
                    item.BackColor = SystemColors.Window;
                    item.ForeColor = Color.Gray;

                    // 2009/1/30
                    ListViewUtil.ChangeItemText(item, COLUMN_STATE, "旧结算");
                }
                else if (strOldState == "settlemented"
                    || strOldState == "新结算")
                {
                    // 状态不变
                    Debug.Assert(item.ImageIndex == ITEMTYPE_NEWLY_SETTLEMENTED, "");
                }
                else if (strOldState == "旧结算")
                {
                    // 状态不变
                    Debug.Assert(item.ImageIndex == ITEMTYPE_OLD_SETTLEMENTED, "");
                }
                else
                {
                    Debug.Assert(strOldState == "amerced" || strOldState == "已收费", "");
                    item.ImageIndex = ITEMTYPE_NEWLY_SETTLEMENTED;
                    item.BackColor = Color.LightGreen;
                    item.ForeColor = SystemColors.WindowText;
                }
            }
            else
            {
                item.ImageIndex = ITEMTYPE_UNKNOWN;    // 未知的类型
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
            }

        }

        void SetNextButtonEnable()
        {
            // string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_range)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_items)
            {
                this.button_next.Enabled = true;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
                this.button_next.Enabled = false;
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

        }

        // 下一步 按钮
        private void button_next_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.tabControl_main.SelectedTab == this.tabPage_range)
            {
                bool bQuick = Control.ModifierKeys == Keys.Control;

                if (this.radioButton_range_amerceOperTime.Checked == true)  // 2009/1/29
                {

                    // 检查两个日期是否为空，和大小关系
                    if (this.dateControl_start.Value == new DateTime((long)0))
                    {
                        strError = "尚未指定起始日期";
                        this.dateControl_start.Focus();
                        goto ERROR1;
                    }

                    if (this.dateControl_end.Value == new DateTime((long)0))
                    {
                        strError = "尚未指定结束日期";
                        this.dateControl_end.Focus();
                        goto ERROR1;
                    }

                    if (this.dateControl_start.Value.Ticks > this.dateControl_end.Value.Ticks)
                    {
                        strError = "起始日期不能大于结束日期";
                        goto ERROR1;
                    }
                }

                string strQueryXml = "";
                int nRet = 0;

                if (this.radioButton_range_amerceOperTime.Checked == true)
                {
                    // return:
                    //      -1  出错
                    //      0   违约金库名没有配置
                    //      1   成功
                    nRet = BuildQueryXml(this.dateControl_start.Value,
                        this.dateControl_end.Value,
                        this.comboBox_range_state.Text,
                        out strQueryXml,
                        out strError);
                }
                else
                {

                    // 构造XML检索式
                    // parameters:
                    //      strStartCtlno  起始索引号。
                    //      strEndCtlno 结束索引号。
                    // return:
                    //      -1  出错
                    //      0   违约金库名没有配置
                    //      1   成功
                    nRet = BuildQueryXml(this.textBox_range_startCtlno.Text,
                        this.textBox_range_endCtlno.Text,
                        this.comboBox_range_state.Text,
                        out strQueryXml,
                        out strError);
                }


                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                    goto ERROR1;    // 违约金库没有配置

                this.tabControl_main.SelectedTab = this.tabPage_items;

                // 装载记录
                nRet = LoadAmercedRecords(
                    bQuick,
                    strQueryXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                return;
            }

            else if (this.tabControl_main.SelectedTab == this.tabPage_items)
            {

                int nAmercedCount = 0;
                int nOldSettlementedCount = 0;
                int nNewlySettlementedCount = 0;
                int nOtherCount = 0;
                GetItemTypesCount(out nAmercedCount,
                    out nOldSettlementedCount,
                    out nNewlySettlementedCount,
                    out nOtherCount);

                if (nAmercedCount == 0)
                {
                    if (nNewlySettlementedCount == 0
                        && nOldSettlementedCount == 0)
                    {
                        MessageBox.Show(this, "当前列表中没有任何新结算和旧结算的事项，无法打印出有内容的结算清单");
                        goto END1;
                    }
                }

                if (nAmercedCount > 0)
                {
                    // 提示结算所有未结算的事项
                    DialogResult result = MessageBox.Show(
                        this,
                        "对当前集合中所有未结算事项进行结算？\r\n\r\n(Yes 结算，并到下一步; No 不结算，到下一步; Cancel 不结算，也不到下一步)",
                        "SettlementForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                    {
                        // 也不切换页
                        goto END1;
                    }
                    if (result == DialogResult.No)
                    {
                        // 不结算，但是切换页
                        this.tabControl_main.SelectedTab = this.tabPage_print;
                        this.button_next.Enabled = false;
                        goto END1;
                    }

                    // 结算，并切换页
                    menu_selectAmerced_Click(this, null);

                    if (
                        (this.toolStripButton_items_useCheck.Checked == true
                        && this.listView_amerced.CheckedItems.Count != 0)
                        ||
                        (this.toolStripButton_items_useCheck.Checked == false
                        && this.listView_amerced.SelectedItems.Count != 0)
                        )
                    {
                        button_settlement_Click(this, null);
                    }
                }

                this.tabControl_main.SelectedTab = this.tabPage_print;
                this.button_next.Enabled = false;
            }
            else if (this.tabControl_main.SelectedTab == this.tabPage_print)
            {
            }
            else
            {
                Debug.Assert(false, "未知的tabpage状态");
            }

        END1:
            this.SetNextButtonEnable();


            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void radioButton_range_amerceOperTime_CheckedChanged(object sender, EventArgs e)
        {
            SetRangeControlsEnabled(true);
        }

        private void radioButton_range_ctlno_CheckedChanged(object sender, EventArgs e)
        {
            SetRangeControlsEnabled(true);
        }

        void SetRangeControlsEnabled(bool bEnable)
        {
            // 2009/1/29
            if (bEnable == false)
            {
                this.dateControl_start.Enabled = false;
                this.dateControl_end.Enabled = false;

                this.textBox_range_startCtlno.Enabled = false;
                this.textBox_range_endCtlno.Enabled = false;
                return;
            }

            if (this.radioButton_range_amerceOperTime.Checked == true)
            {
                this.dateControl_start.Enabled = true;
                this.dateControl_end.Enabled = true;

                this.textBox_range_startCtlno.Enabled = false;
                this.textBox_range_endCtlno.Enabled = false;
            }
            else
            {
                this.dateControl_start.Enabled = false;
                this.dateControl_end.Enabled = false;

                this.textBox_range_startCtlno.Enabled = true;
                this.textBox_range_endCtlno.Enabled = true;
            }

        }

        // 结算
        private void button_settlement_Click(object sender, EventArgs e)
        {
            SettlementAction("settlement");
        }

        // 撤销结算
        private void button_undoSettlement_Click(object sender, EventArgs e)
        {
            SettlementAction("undosettlement");
        }

        // 结算、撤销或者删除
        // TODO: 对旧结算事项的撤销，要慎重，至少要警告一下
        // parameters:
        //      bSettlement 如果为true，表示结算；如果为false，表示撤销结算
        void SettlementAction(string strAction)
        {
            string strError = "";

            string strOperName = "";

            if (strAction == "settlement")
                strOperName = "结算";
            else if (strAction == "undosettlement")
                strOperName = "撤销结算";
            else if (strAction == "delete")
            {
                strOperName = "删除已结算事项";
            }
            else
            {
                strError = "未能识别的 strAction 参数值 '" + strAction + "'";
                goto ERROR1;
            }

            // 构造id列表
            List<string> total_ids = new List<string>();

            List<ListViewItem> items = new List<ListViewItem>();

            if (this.toolStripButton_items_useCheck.Checked == true)
            {
                if (this.listView_amerced.CheckedItems.Count == 0)
                {
                    strError = "尚未勾选要" + strOperName + "的事项";
                    goto ERROR1;
                }

                for (int i = 0; i < this.listView_amerced.CheckedItems.Count; i++)
                {
                    ListViewItem item = this.listView_amerced.CheckedItems[i];
                    items.Add(item);
                }
            }
            else
            {
                if (this.listView_amerced.SelectedItems.Count == 0)
                {
                    strError = "尚未选定要" + strOperName + "的事项";
                    goto ERROR1;
                }

                foreach (ListViewItem item in this.listView_amerced.SelectedItems)
                {
                    // ListViewItem item = this.listView_amerced.SelectedItems[i];
                    items.Add(item);
                }
            }

            int nAmercedCount = 0;
            int nOldSettlementedCount = 0;
            int nNewlySettlementedCount = 0;
            int nOtherCount = 0;

            // 先进行检查和警告
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                if (item.ImageIndex == ITEMTYPE_AMERCED)
                    nAmercedCount++;
                else if (item.ImageIndex == ITEMTYPE_OLD_SETTLEMENTED)
                    nOldSettlementedCount++;
                else if (item.ImageIndex == ITEMTYPE_NEWLY_SETTLEMENTED)
                    nNewlySettlementedCount++;
                else
                    nOtherCount++;
            }

            string strWarning = "";

            if (strAction == "settlement")
            {
                if (nAmercedCount == 0)
                {
                    strError = "当前选定的事项中，因没有包含状态为“已收费”的事项，结算操作无法进行";
                    goto ERROR1;
                }

                if (nOldSettlementedCount + nNewlySettlementedCount > 0)
                    strWarning = "当前选定的事项中有 "
                        + (nOldSettlementedCount + nNewlySettlementedCount).ToString()
                        + " 个已经结算的事项，在结算操作中将被跳过。";
            }
            else if (strAction == "undosettlement")
            {
                if (nOldSettlementedCount + nNewlySettlementedCount == 0)
                {
                    strError = "当前选定的事项中，因没有包含状态为“新结算”和“旧结算”的事项，撤销结算操作无法进行";
                    goto ERROR1;
                }

                if (nOldSettlementedCount > 0)
                {
                    strWarning = "当前选定的事项中有 "
                        + nOldSettlementedCount.ToString()
                        + " 个旧结算事项(即非本次结算的事项)。如果要对它(们)进行撤销结算的操作，将影响到以前已经打印过的结算清单的准确性和权威性。\r\n\r\n确实要对它们进行撤销结算的操作?";
                    DialogResult result = MessageBox.Show(
                        this,
                        strWarning,
                        "SettlementForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                    {
                        strError = "本次撤销结算的操作已经被全部放弃(无论新、旧结算事项)。";
                        goto ERROR1;
                    }

                    strWarning = "";
                }

                if (nAmercedCount > 0)
                    strWarning = "当前选定的事项中有 "
    + nAmercedCount.ToString()
    + " 个未结算(即已收费状态)的事项，在撤销结算的操作中将被跳过。";

            }
            else if (strAction == "delete")
            {
                if (nOldSettlementedCount + nNewlySettlementedCount == 0)
                {
                    strError = "当前选定的事项中，因没有包含状态为“新结算”和“旧结算”的事项，删除已结算事项的操作无法进行";
                    goto ERROR1;
                }

                if (nOldSettlementedCount > 0)
                {
                    strWarning = "确实要从数据库中删除当前选定的 "
    + (nOldSettlementedCount + nNewlySettlementedCount).ToString()
    + " 个已结算事项?\r\n\r\n(警告：删除操作是不可逆的))";
                    DialogResult result = MessageBox.Show(
                        this,
                        strWarning,
                        "SettlementForm",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                    {
                        strError = "取消操作";
                        goto ERROR1;
                    }

                    strWarning = "";
                }

                if (nAmercedCount > 0)
                    strWarning = "当前选定的事项中有 "
+ nAmercedCount.ToString()
+ " 个未结算(即已收费状态)的事项，在删除已结算事项的操作中将被跳过。";

            }

            if (String.IsNullOrEmpty(strWarning) == false)
            {
                MessageBox.Show(this, strWarning);
            }

            // 是否前端就可进行矛盾性检查？
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                string strID = item.Text;
                string strState = item.SubItems[COLUMN_STATE].Text;

                if (strAction == "settlement")
                {
                    if (strState == "settlemented"
                        || strState == "新结算"
                        || strState == "旧结算")
                    {
                        /*
                        strError = "ID为 " + strID + " 的事项状态为"+strState+"，无法再进行"+strAction+"操作。请去除该事项的勾选状态后再重新提交请求。";
                        goto ERROR1;
                         * */
                        continue;
                    }
                }

                if (strAction == "undosettlement")
                {
                    if (strState == "amerced"
                        || strState == "已收费")
                    {
                        /*
                        strError = "ID为 " + strID + " 的事项状态为"+strState+"，无法进行"+strAction+"操作。请去除该事项的勾选状态后再重新提交请求。";
                        goto ERROR1;
                         * */
                        continue;
                    }
                }

                if (strAction == "delete")
                {
                    if (strState == "amerced"
                        || strState == "已收费")
                    {
                        /*
                        strError = "ID为 " + strID + " 的事项状态为"+strState+"，无法进行"+strAction+"操作。请去除该事项的勾选状态后再重新提交请求。"; 
                        goto ERROR1;
                         * */
                        continue;
                    }
                }

                total_ids.Add(strID);
            }

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.SetMessage("正在进行" + strOperName + " ...");
            _stop.BeginLoop();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在进行" + strOperName + " ...",
                "disableControl");

            looping.Progress.SetProgressRange(0, total_ids.Count);
            try
            {
                int nDone = 0;

                int nPerCount = 10;
                int nBatchCount = (total_ids.Count / nPerCount) + ((total_ids.Count % nPerCount) != 0 ? 1 : 0);
                for (int j = 0; j < nBatchCount; j++)
                {
                    // 每轮处理10个id
                    int nThisCount = Math.Min(total_ids.Count - j * nPerCount, nPerCount);
                    string[] ids = new string[nThisCount];
                    total_ids.CopyTo(j * nPerCount, ids, 0, nThisCount);

                    long lRet = channel.Settlement(
                        looping.Progress,
                        strAction,
                        ids,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    // 刷新
                    int nRet = RefreshAmercedRecords(
                        looping.Progress,
                        channel,
                        // false,
                        ids,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    nDone += nThisCount;
                    looping.Progress.SetProgressValue(nDone);
                }

            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControls(true);
                */
            }

            // 结算成功
            MessageBox.Show(this, strOperName + "成功。(处理记录数 " + total_ids.Count.ToString() + " 个)");
            this.OnItemTypeChanged();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 右鼠标键popup菜单
        private void listView_amerced_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            int nAmercedCount = 0;
            int nOldSettlementedCount = 0;
            int nNewlySettlementedCount = 0;
            int nOtherCount = 0;
            GetItemTypesCount(out nAmercedCount,
                out nOldSettlementedCount,
                out nNewlySettlementedCount,
                out nOtherCount);

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            string strSelectedName = "选定的";
            if (this.toolStripButton_items_useCheck.Checked == true)
                strSelectedName = "勾选的";

            int nSelectedCount = 0;
            if (this.toolStripButton_items_useCheck.Checked == true)
                nSelectedCount = this.listView_amerced.CheckedItems.Count;
            else
                nSelectedCount = this.listView_amerced.SelectedItems.Count;

            // 结算
            menuItem = new MenuItem("结算" + strSelectedName + " " + nSelectedCount.ToString() + " 个事项(&S)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_items_settlement_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // 撤销结算
            menuItem = new MenuItem("撤销结算" + strSelectedName + " " + nSelectedCount.ToString() + " 个事项(&S)");
            menuItem.Click += new System.EventHandler(this.toolStripButton_items_undoSettlement_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("移除" + strSelectedName + " " + nSelectedCount.ToString() + " 个事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_removeSelectedItems_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            string strText = "全选(&A)";
            if (this.toolStripButton_items_useCheck.Checked == true)
                strText = "全勾选(&H)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            if (this.toolStripButton_items_useCheck.Checked == true)
                strText = "全清除勾选(&U)";
            else
                strText = "全不选(&U)";

            menuItem = new MenuItem(strText);
            menuItem.Click += new System.EventHandler(this.menu_unSelectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            if (this.toolStripButton_items_useCheck.Checked == true)
                strText = "勾选";
            else
                strText = "选定";

            menuItem = new MenuItem(strText + "全部(" + nAmercedCount.ToString() + "个) 已收费 事项(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAmerced_Click);
            if (nAmercedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem(strText + "全部(" + (nNewlySettlementedCount + nOldSettlementedCount).ToString() + "个) 已结算(包括新结算和旧结算) 事项(&S)");
            menuItem.Click += new System.EventHandler(this.menu_selectSettlemented_Click);
            if (nNewlySettlementedCount + nOldSettlementedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("使用勾选(&C)");
            menuItem.Click += new System.EventHandler(this.menu_toggleUseCheck_Click);
            if (this.toolStripButton_items_useCheck.Checked == true)
                menuItem.Checked = true;
            else
                menuItem.Checked = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 注：“已结算”包括新结算和旧结算
            menuItem = new MenuItem("从数据库中删除当前" + strText + "的 已结算(包括新结算和旧结算) 事项(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteSettlementedItemsFromDb_Click);
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("导出当前" + strText + "的事项到XML文件(&E)");
            menuItem.Click += new System.EventHandler(this.menu_exportToXmlFile_Click);
            if (nSelectedCount == 0 || this.m_nInSearching > 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_amerced, new Point(e.X, e.Y));

        }

        void menu_exportToXmlFile_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.m_nInSearching > 0)
            {
                strError = "当前正在进行另一检索操作，需要先停止它，才能进行导出违约金库记录的操作...";
                goto ERROR1;
            }

            nRet = ExportXmlFile(out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        string m_strOutputXmlFilename = "";

        int ExportXmlFile(out string strError)
        {
            strError = "";

            /*
            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.SetMessage("正在导出违约金记录 ...");
            _stop.BeginLoop();

            this.EnableControls(false);
            */
            var looping = Looping(out LibraryChannel channel,
                "正在导出违约金记录 ...",
                "disableControl");
            try
            {
                List<ListViewItem> items = new List<ListViewItem>();

                if (this.toolStripButton_items_useCheck.Checked == true)
                {
                    if (this.listView_amerced.CheckedItems.Count == 0)
                    {
                        strError = "尚未勾选要导出的事项";
                        return -1;
                    }

                    for (int i = 0; i < this.listView_amerced.CheckedItems.Count; i++)
                    {
                        ListViewItem item = this.listView_amerced.CheckedItems[i];
                        items.Add(item);
                    }
                }
                else
                {
                    if (this.listView_amerced.SelectedItems.Count == 0)
                    {
                        strError = "尚未选定要导出的事项";
                        return -1;
                    }

                    foreach (ListViewItem item in this.listView_amerced.SelectedItems)
                    {
                        // ListViewItem item = this.listView_amerced.SelectedItems[i];
                        items.Add(item);
                    }
                }

                // 准备XML输出文件
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Title = "请指定输出的XML文件";
                dlg.OverwritePrompt = true;
                dlg.CreatePrompt = false;
                dlg.FileName = m_strOutputXmlFilename;
                dlg.Filter = "XML文件 (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                m_strOutputXmlFilename = dlg.FileName;

                // 输出的文件流

                using (FileStream outputfile = File.Create(m_strOutputXmlFilename))
                using (XmlTextWriter writer = new XmlTextWriter(outputfile, Encoding.UTF8)) // Xml格式输出
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    // 写容器元素
                    writer.WriteStartDocument();
                    writer.WriteStartElement("dprms", "collection", DpNs.dprms);

                    looping.Progress.SetProgressRange(0, items.Count);
                    for (int i = 0; i < items.Count; i++)
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (looping.Stopped)
                        {
                            strError = "用户中断";
                            return -1;
                        }

                        looping.Progress.SetMessage("正在导出违约金记录 " + (i + 1).ToString() + " / " + items.Count.ToString() + " ...");

                        ListViewItem item = items[i];
                        string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                        string strXml = "";
                        byte[] timestamp = null;
                        long lRet = channel.GetRecord(
                            looping.Progress,
                            strRecPath,
                            out timestamp,
                            out strXml,
                            out strError);
                        if (lRet == -1)
                            return -1;    // TODO: 提示重试

                        XmlDocument dom = new XmlDocument();
                        try
                        {
                            dom.LoadXml(strXml);
                        }
                        catch (Exception ex)
                        {
                            strError = "XML装入DOM时出错: " + ex.Message;
                            return -1;
                        }
                        dom.DocumentElement.WriteTo(writer);

                        looping.Progress.SetProgressValue(i + 1);
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    // writer.Close();
                }
                return 1;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");
                _stop.HideProgress();

                this.EnableControls(true);
                */
            }
        }

        // 从数据库中删除 勾选的 已结算(settlemented) 事项
        void menu_deleteSettlementedItemsFromDb_Click(object sender, EventArgs e)
        {
            SettlementAction("delete");
        }

        // 移除选定(或勾选)的事项
        void menu_removeSelectedItems_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_items_useCheck.Checked == false)
            {

                if (this.listView_amerced.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, "尚未选定任何需要移除的事项");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    this,
                    "确实要从列表中移除所选定的 " + this.listView_amerced.SelectedItems.Count.ToString() + " 个事项?\r\n\r\n(注：本操作不会从数据库中删除记录)",
                    "SettlementForm",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return;

                List<ListViewItem> items = new List<ListViewItem>();

                foreach (ListViewItem item in this.listView_amerced.SelectedItems)
                {
                    items.Add(item);
                }

                for (int i = 0; i < items.Count; i++)
                {
                    this.listView_amerced.Items.Remove(items[i]);
                }
            }
            else
            {
                if (this.listView_amerced.CheckedIndices.Count == 0)
                {
                    MessageBox.Show(this, "尚未勾选任何需要移除的事项");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    this,
                    "确实要从列表中移除所勾选的 " + this.listView_amerced.CheckedItems.Count.ToString() + " 个事项?\r\n\r\n(注：本操作不会从数据库中删除记录)",
                    "SettlementForm",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return;

                for (int i = this.listView_amerced.CheckedIndices.Count - 1; i >= 0; i--)
                {
                    this.listView_amerced.Items.RemoveAt(this.listView_amerced.CheckedIndices[i]);
                }
            }

            this.OnItemTypeChanged();
        }

        // 全(勾)选
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_items_useCheck.Checked == false)
            {
                for (int i = 0; i < this.listView_amerced.Items.Count; i++)
                {
                    this.listView_amerced.Items[i].Selected = true;
                }
            }
            else
            {
                for (int i = 0; i < this.listView_amerced.Items.Count; i++)
                {
                    this.listView_amerced.Items[i].Checked = true;
                }
            }
        }

        // 全撤销(勾)选
        void menu_unSelectAll_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_items_useCheck.Checked == false)
            {
                for (int i = 0; i < this.listView_amerced.Items.Count; i++)
                {
                    this.listView_amerced.Items[i].Selected = false;
                }
            }
            else
            {
                for (int i = 0; i < this.listView_amerced.Items.Count; i++)
                {
                    this.listView_amerced.Items[i].Checked = false;
                }
            }
        }

        // 只(勾)选 amerced 状态事项
        void menu_selectAmerced_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];
                string strState = item.SubItems[COLUMN_STATE].Text;

                if (this.toolStripButton_items_useCheck.Checked == true)
                {
                    if (strState == "amerced"
                        || strState == "已收费")
                        item.Checked = true;
                    else
                        item.Checked = false;
                }
                else
                {
                    if (strState == "amerced"
                        || strState == "已收费")
                        item.Selected = true;
                    else
                        item.Selected = false;
                }
            }
        }

        // 选定全部 settelmented 状态事项
        // 注：不管新旧结算事项，都选定
        // TODO: 对旧结算事项的撤销，要慎重，至少要警告一下
        void menu_selectSettlemented_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];
                string strState = item.SubItems[COLUMN_STATE].Text;

                if (this.toolStripButton_items_useCheck.Checked == true)
                {
                    if (strState == "settlemented"
                        || strState == "新结算"
                        || strState == "旧结算")
                        item.Checked = true;
                    else
                        item.Checked = false;
                }
                else
                {
                    if (strState == "settlemented"
                        || strState == "新结算"
                        || strState == "旧结算")
                        item.Selected = true;
                    else
                        item.Selected = false;
                }
            }
        }

        // 是否包含价格列?
        static bool HasPriceColumn(PrintOption option)
        {
            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strText = StringUtil.GetLeft(column.Name);


                if (strText == "price"
                    || strText == "金额")
                    return true;
            }

            return false;
        }



        #region 打印相关功能

        // 对即将打印的事项进行检查，看看是不是符合结算流程
        // return:
        //      -1  出错
        //      0   正常
        //      1   有违反流程的情况出现，在strError中描述
        int CheckBeforeSettlementPrint(List<ListViewItem> items,
            out string strError)
        {
            strError = "";
            int nAmercedStateCount = 0;
            int nOldSettlementStateCount = 0;
            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                if (item == null)
                    continue;

                string strState = item.SubItems[COLUMN_STATE].Text;

                if (strState == "amerced"
                    || strState == "已收费")
                {
                    nAmercedStateCount++;
                }

                if (item.ImageIndex == ITEMTYPE_OLD_SETTLEMENTED)
                    nOldSettlementStateCount++;
            }

            if (nAmercedStateCount > 0)
            {
                strError = "当前拟打印的事项集合中，有 " + nAmercedStateCount.ToString() + " 个已收费但未结算的事项(黄底色的事项)，如果这些事项参与打印和统计，那么总计和小计金额将不能代表结算金额。";
            }

            if (nOldSettlementStateCount > 0)
            {
                strError += "当前拟打印的事项集合中，有 " + nOldSettlementStateCount.ToString() + " 个以往(非本次)结算的事项(灰色文字的事项)，如果这些事项参与打印和统计，那么总计和小计金额将不能代表本次结算金额。";
            }

            if (String.IsNullOrEmpty(strError) == false)
                return 1;

            return 0;
        }

        void PrintList(List<ListViewItem> items)
        {
            string strError = "";

            // 创建一个html文件，并显示在HtmlPrintForm中。

            List<string> filenames = null;
            try
            {
                // Debug.Assert(false, "");

                // 构造html页面
                int nRet = BuildHtml(
                    items,
                    out filenames,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                HtmlPrintForm printform = new HtmlPrintForm();

                printform.Text = "打印结算清单";
                // printform.MainForm = Program.MainForm;
                printform.Filenames = filenames;
                Program.MainForm.AppInfo.LinkFormState(printform, "printform_state");
                printform.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(printform);
            }

            finally
            {
                if (filenames != null)
                    Global.DeleteFiles(filenames);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得事项数。排除了空对象，也就是小计行
        static int GetItemCount(List<ListViewItem> items)
        {
            int nResult = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                    nResult++;
            }

            return nResult;
        }

        // 构造html页面
        int BuildHtml(
            List<ListViewItem> items,
            out List<string> filenames,
            out string strError)
        {
            strError = "";

            Hashtable macro_table = new Hashtable();

            // 获得打印参数
            PrintOption option = new SettlementPrintOption(Program.MainForm.UserDir
                // Program.MainForm.DataDir
                );
            option.LoadData(Program.MainForm.AppInfo,
                "settlement_printoption");

            // 检查按收费者小计时，是否具有价格列
            if (this.checkBox_sumByAmerceOperator.Checked == true)
            {
                // 检查是否具有价格列？
                if (HasPriceColumn(option) == false)
                {
                    MessageBox.Show(this, "警告：虽打印要求‘按收费者小计金额’，但‘价格’列并未包含在打印列中。因此小计的金额无法打印出来。");
                }
            }



            // 计算出页总数
            int nTablePageCount = items.Count / option.LinesPerPage;
            if ((items.Count % option.LinesPerPage) != 0)
                nTablePageCount++;

            int nPageCount = nTablePageCount + 1;

            macro_table["%pagecount%"] = nPageCount.ToString();
            macro_table["%linesperpage%"] = option.LinesPerPage.ToString();
            macro_table["%date%"] = DateTime.Now.ToLongDateString();


            filenames = new List<string>();    // 每页一个文件，这个数组存放了所有文件名

            string strFileNamePrefix = Program.MainForm.DataDir + "\\~settlement";

            string strFileName = "";

            // 输出信息页
            // TODO: 要增加“统计页”模板功能。如何用模板来定义循环的行，有一定难度
            {
                int nItemCount = GetItemCount(items);
                string strTotalPrice = GetTotalPrice(items).ToString();

                macro_table["%itemcount%"] = nItemCount.ToString();
                macro_table["%totalprice%"] = strTotalPrice;


                macro_table["%pageno%"] = "1";

                strFileName = strFileNamePrefix + "0" + ".html";

                filenames.Add(strFileName);

                BuildPageTop(option,
                    macro_table,
                    strFileName,
                    false);

                // 内容行
                StreamUtil.WriteText(strFileName,
                    "<table class='totalsum'>");

                // 列标题行
                StreamUtil.WriteText(strFileName,
                    "<tr class='totalsum_columtitle'>");
                StreamUtil.WriteText(strFileName,
                    "<td class='person'>收费者</td>");
                StreamUtil.WriteText(strFileName,
                    "<td class='itemcount'>事项数</td>");
                StreamUtil.WriteText(strFileName,
                    "<td class='price'>金额</td>");
                StreamUtil.WriteText(strFileName,
                    "</tr>");

                // 总计行

                StreamUtil.WriteText(strFileName,
                    "<tr class='totalsum_line'>");
                StreamUtil.WriteText(strFileName,
                    "<td class='person'>总计</td>");
                StreamUtil.WriteText(strFileName,
                    "<td class='itemcount'>" + nItemCount.ToString() + "</td>");
                StreamUtil.WriteText(strFileName,
                    "<td class='price'>" + strTotalPrice + "</td>");
                StreamUtil.WriteText(strFileName,
                    "</tr>");


                if (this.checkBox_sumByAmerceOperator.Checked == true)
                {
                    // 收费人小计

                    // 小计提示行
                    StreamUtil.WriteText(strFileName,
                        "<tr class='amerceoperatorsum_titleline'>");
                    StreamUtil.WriteText(strFileName,
                        "<td class='amerceoperatorsum_titleline' colspan='3'>(以下为按收费者分类的小计金额)</td>");
                    StreamUtil.WriteText(strFileName,
                        "</tr>");

                    for (int i = 0; i < items.Count; i++)
                    {
                        ListViewItem item = items[i];
                        if (item == null)
                        {
                            string strAmerceOperator = "";
                            int nSumItemCount = 0;
                            decimal sum = ComputeSameAmerceOperatorSumPrice(items,
                                i,
                                out strAmerceOperator,
                                out nSumItemCount);
                            StreamUtil.WriteText(strFileName,
                                "<tr class='amerceoperatorsum_line'>");
                            StreamUtil.WriteText(strFileName,
                                "<td class='person'>" + strAmerceOperator + "</td>");
                            StreamUtil.WriteText(strFileName,
                                "<td class='itemcount'>" + nSumItemCount.ToString() + "</td>");
                            StreamUtil.WriteText(strFileName,
                                "<td class='price'>" + sum.ToString() + "</td>");

                            StreamUtil.WriteText(strFileName,
                                "</tr>");
                        }
                    }

                    StreamUtil.WriteText(strFileName,
                        "</table>");

                }

                BuildPageBottom(option,
                    macro_table,
                    strFileName,
                    false);
            }

            // 表格页循环
            for (int i = 0; i < nTablePageCount; i++)
            {
                macro_table["%pageno%"] = (i + 1 + 1).ToString();

                strFileName = strFileNamePrefix + (i + 1).ToString() + ".html";

                filenames.Add(strFileName);

                BuildPageTop(option,
                    macro_table,
                    strFileName,
                    true);
                // 行循环
                for (int j = 0; j < option.LinesPerPage; j++)
                {
                    BuildTableLine(option,
                        items,
                        strFileName, i, j);
                }

                BuildPageBottom(option,
                    macro_table,
                    strFileName,
                    true);
            }


            return 0;
        }

        // 2009/10/10
        // 获得css文件的路径(或者http:// 地址)。将根据是否具有“统计页”来自动处理
        // parameters:
        //      strDefaultCssFileName   “css”模板缺省情况下，将采用的虚拟目录中的css文件名，纯文件名
        string GetAutoCssUrl(PrintOption option,
            string strDefaultCssFileName)
        {
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                return strCssFilePath;
            else
            {
                // return Program.MainForm.LibraryServerDir + "/" + strDefaultCssFileName;    // 缺省的
                return PathUtil.MergePath(Program.MainForm.DataDir, strDefaultCssFileName);    // 缺省的
            }
        }

        int BuildPageTop(PrintOption option,
            Hashtable macro_table,
            string strFileName,
            bool bOutputTable)
        {
            /*
            string strLibraryServerUrl = Program.MainForm.AppInfo.GetString(
    "config",
    "circulation_server_url",
    "");
            int pos = strLibraryServerUrl.LastIndexOf("/");
            if (pos != -1)
                strLibraryServerUrl = strLibraryServerUrl.Substring(0, pos);
             * */

            // string strCssUrl = Program.MainForm.LibraryServerDir + "/settlement.css";
            // 2009/10/10 changed
            string strCssUrl = GetAutoCssUrl(option, "settlement.css");

            /*
            // 2009/10/9
            string strCssFilePath = option.GetTemplatePageFilePath("css");  // 大小写不敏感
            if (String.IsNullOrEmpty(strCssFilePath) == false)
                strCssUrl = strCssFilePath;
            else
                strCssUrl = Program.MainForm.LibraryServerDir + "/settlement.css";    // 缺省的
             * */

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            StreamUtil.WriteText(strFileName,
                "<html><head>" + strLink + "</head><body>");


            // 页眉
            string strPageHeaderText = option.PageHeader;

            if (String.IsNullOrEmpty(strPageHeaderText) == false)
            {
                strPageHeaderText = StringUtil.MacroString(macro_table,
                    strPageHeaderText);

                StreamUtil.WriteText(strFileName,
                    "<div class='pageheader'>" + strPageHeaderText + "</div>");

                /*
                StreamUtil.WriteText(strFileName,
                    "<hr class='pageheader' />");
                 * */
            }

            // 表格标题
            string strTableTitleText = option.TableTitle;

            if (String.IsNullOrEmpty(strTableTitleText) == false)
            {

                strTableTitleText = StringUtil.MacroString(macro_table,
                    strTableTitleText);

                StreamUtil.WriteText(strFileName,
                    "<div class='tabletitle'>" + strTableTitleText + "</div>");
            }

            if (bOutputTable == true)
            {

                // 表格开始
                StreamUtil.WriteText(strFileName,
                    "<table class='table'>");   //   border='1'

                // 栏目标题
                StreamUtil.WriteText(strFileName,
                    "<tr class='column'>");

                for (int i = 0; i < option.Columns.Count; i++)
                {
                    Column column = option.Columns[i];

                    string strCaption = column.Caption;

                    // 如果没有caption定义，就挪用name定义
                    if (String.IsNullOrEmpty(strCaption) == true)
                        strCaption = column.Name;

                    string strClass = StringUtil.GetLeft(column.Name);

                    StreamUtil.WriteText(strFileName,
                        "<td class='" + strClass + "'>" + strCaption + "</td>");
                }

                StreamUtil.WriteText(strFileName,
                    "</tr>");

            }

            return 0;
        }

        // 汇总价格。假定nIndex处在切换行(同一amerceOperator行组的最后一行)
        static decimal ComputeSameAmerceOperatorSumPrice(List<ListViewItem> items,
            int nIndex,
            out string strAmerceOperator,
            out int nCount)
        {
            strAmerceOperator = "";
            nCount = 0;

            if (nIndex - 1 < 0)
                return 0;

            Debug.Assert(items[nIndex] == null, "起点事项应为null");

            decimal total = 0;

            for (int i = nIndex - 1; i >= 0; i--)
            {
                ListViewItem item = items[i];

                if (item == null)
                    break;

                // 顺便获得收费者
                if (String.IsNullOrEmpty(strAmerceOperator) == true)
                    strAmerceOperator = GetColumnContent(item, "amerceOperator");

                string strPrice = GetColumnContent(item, "price");

                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                nCount++;   // 包含了没有价格字符串的那些事项

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDecimal(strPurePrice);
            }

            return total;
        }

        int BuildTableLine(PrintOption option,
    List<ListViewItem> items,
    string strFileName,
    int nPage,
    int nLine)
        {
            int nIndex = nPage * option.LinesPerPage + nLine;

            if (nIndex >= items.Count)
                return 0;

            ListViewItem item = items[nIndex];

            string strAmerceOperator = "";
            string strSumContent = "";
            int nItemCount = 0;
            if (item == null)
            {
                // 汇总前面的价格
                strSumContent = ComputeSameAmerceOperatorSumPrice(items, nIndex, out strAmerceOperator, out nItemCount).ToString();
            }

            // 栏目内容
            string strLineContent = "";

            for (int i = 0; i < option.Columns.Count; i++)
            {
                Column column = option.Columns[i];

                string strContent = "";

                // 表示需要打印小计行
                if (item == null)
                {
                    string strColumnName = StringUtil.GetLeft(column.Name);
                    if (strColumnName == "price"
                        || strColumnName == "金额")
                    {
                        strContent = strAmerceOperator + " 共 " + nItemCount.ToString() + "项 小计：" + strSumContent;
                    }
                    else if (strColumnName == "amerceOperator"
                        || strColumnName == "收费者")
                    {
                        strContent = strAmerceOperator;
                    }
                }
                else
                {
                    strContent = GetColumnContent(item,
                        column.Name);
                }

                if (strContent == "!!!#")
                    strContent = ((nPage * option.LinesPerPage) + nLine + 1).ToString();

                // 截断字符串
                if (column.MaxChars != -1)
                {
                    if (strContent.Length > column.MaxChars)
                    {
                        strContent = strContent.Substring(0, column.MaxChars);
                        strContent += "...";
                    }
                }

                if (String.IsNullOrEmpty(strContent) == true)
                    strContent = "&nbsp;";

                string strClass = StringUtil.GetLeft(column.Name);

                strLineContent +=
                    "<td class='" + strClass + "'>" + strContent + "</td>";
            }

            if (item != null)
            {
                StreamUtil.WriteText(strFileName,
                    "<tr class='content'>");
            }
            else
            {
                StreamUtil.WriteText(strFileName,
                    "<tr class='content_amerceoperator_sum'>");
            }

            StreamUtil.WriteText(strFileName,
                strLineContent);

            StreamUtil.WriteText(strFileName,
                "</tr>");

            return 0;
        }

        int BuildPageBottom(PrintOption option,
    Hashtable macro_table,
    string strFileName,
    bool bOutputTable)
        {


            if (bOutputTable == true)
            {
                // 表格结束
                StreamUtil.WriteText(strFileName,
                    "</table>");
            }

            // 页脚
            string strPageFooterText = option.PageFooter;

            if (String.IsNullOrEmpty(strPageFooterText) == false)
            {
                /*
                StreamUtil.WriteText(strFileName,
                    "<hr class='pagefooter' />");
                 * */


                strPageFooterText = StringUtil.MacroString(macro_table,
                    strPageFooterText);

                StreamUtil.WriteText(strFileName,
        "<div class='pagefooter'>" + strPageFooterText + "</div>");
            }


            StreamUtil.WriteText(strFileName, "</body></html>");

            return 0;
        }

        static decimal GetTotalPrice(List<ListViewItem> items)
        {
            decimal total = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ListViewItem item = items[i];

                if (item == null)
                    continue;

                string strPrice = "";

                try
                {
                    strPrice = item.SubItems[COLUMN_PRICE].Text;
                }
                catch
                {
                    continue;
                }

                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;

                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                if (String.IsNullOrEmpty(strPurePrice) == true)
                    continue;

                total += Convert.ToDecimal(strPurePrice);
            }

            return total;
        }

        #endregion

        // 打印选项
        private void button_print_option_Click(object sender, EventArgs e)
        {
            // 配置标题和风格
            PrintOption option = new SettlementPrintOption(Program.MainForm.UserDir // Program.MainForm.DataDir
                );
            option.LoadData(Program.MainForm.AppInfo,
                "settlement_printoption");


            PrintOptionDlg dlg = new PrintOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            // dlg.MainForm = Program.MainForm;
            dlg.DataDir = Program.MainForm.UserDir; // .DataDir;
            dlg.PrintOption = option;
            dlg.ColumnItems = new string[] {
                "no -- 序号",
                "id -- 记录ID",
                "state -- 状态",
                "readerBarcode -- 读者证条码号",
                "summary -- 摘要",
                "price -- 金额",
                "comment -- 注释",
                "reason -- 原因",
                "borrowDate -- 借阅日期",
                "borrowPeriod -- 借阅时限",
                "returnDate -- 还书日期",
                "returnOperator -- 还书操作者",
                "barcode -- 册条码号",
                "amerceOperator -- 收费者",
                "amerceTime -- 收费日期",
                "settlementOperator -- 结算者",
                "settlementTime -- 结算日期",
                "recpath -- 记录路径"
            };

            Program.MainForm.AppInfo.LinkFormState(dlg, "settlement_printoption_formstate");
            dlg.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            option.SaveData(Program.MainForm.AppInfo,
                "settlement_printoption");
        }

        // 获得栏目内容
        static string GetColumnContent(ListViewItem item,
            string strColumnName)
        {
            // 去掉"-- ?????"部分
            string strText = StringUtil.GetLeft(strColumnName);

            try
            {

                // 要中英文都可以
                switch (strText)
                {
                    case "no":
                    case "序号":
                        return "!!!#";  // 特殊值，表示序号
                    case "id":
                    case "记录ID":
                        return item.SubItems[COLUMN_ID].Text;
                    case "state":
                    case "状态":
                        return item.SubItems[COLUMN_STATE].Text;
                    case "readerBarcode":
                    case "读者证条码号":
                        return item.SubItems[COLUMN_READERBARCODE].Text;
                    case "summary":
                    case "摘要":
                        return item.SubItems[COLUMN_SUMMARY].Text;
                    case "price":
                    case "金额":
                        return item.SubItems[COLUMN_PRICE].Text;
                    case "comment":
                    case "注释":
                        return item.SubItems[COLUMN_COMMENT].Text;
                    case "reason":
                    case "原因":
                        return item.SubItems[COLUMN_REASON].Text;
                    case "borrowDate":
                    case "借阅日期":
                        return item.SubItems[COLUMN_BORROWDATE].Text;
                    case "borrowPeriod":
                    case "借阅时限":
                        return item.SubItems[COLUMN_BORROWPERIOD].Text;
                    case "returnDate":
                    case "还书日期":
                        return item.SubItems[COLUMN_RETURNDATE].Text;
                    case "returnOperator":
                    case "还书操作者":
                        return item.SubItems[COLUMN_RETURNOPERATOR].Text;
                    case "barcode":
                    case "册条码号":
                        return item.SubItems[COLUMN_BARCODE].Text;
                    case "amerceOperator":
                    case "收费者":
                        return item.SubItems[COLUMN_AMERCEOPERATOR].Text;
                    case "amerceTime":
                    case "收费日期":
                        return item.SubItems[COLUMN_AMERCETIME].Text;


                    case "settlementOperator":
                    case "结算者":
                        return item.SubItems[COLUMN_SETTLEMENTOPERATOR].Text;
                    case "settlementTime":
                    case "结算日期":
                        return item.SubItems[COLUMN_SETTLEMENTTIME].Text;
                    case "recpath":
                    case "记录路径":
                        return item.SubItems[COLUMN_RECPATH].Text;
                    default:
                        return "undefined column";
                }
            }

            catch
            {
                return null;    // 表示没有这个subitem下标
            }

        }

        private void listView_amerced_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns.SetFirstColumn(nClickColumn,
                this.listView_amerced.Columns);

            // 排序
            this.listView_amerced.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView_amerced.ListViewItemSorter = null;
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetNextButtonEnable();
        }

        // 打印本次结算部分
        private void button_print_printSettlemented_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 检查当前排序状态和按收费者小计之间是否存在矛盾
            if (this.checkBox_sumByAmerceOperator.Checked == true)
            {

                if (this.SortColumns.Count != 0
                    && this.SortColumns[0].No == COLUMN_AMERCEOPERATOR)
                {
                }
                else
                {
                    ColumnClickEventArgs e1 = new ColumnClickEventArgs(COLUMN_AMERCEOPERATOR);
                    listView_amerced_ColumnClick(this, e1);
                    MessageBox.Show(this, "因打印要求‘按收费者小计金额’，打印前软件已自动将集合内事项按‘收费者’排序。");
                }

            }

            // 如果要打印小计行，需要在items集合中插入null对象，方便打印时针对处理
            string strPrevAmerceOperator = "";

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];

                // 排除其他不该打印的对象
                if (item.ImageIndex != ITEMTYPE_NEWLY_SETTLEMENTED)
                    continue;

                string strAmerceOperator = item.SubItems[COLUMN_AMERCEOPERATOR].Text;

                if (this.checkBox_sumByAmerceOperator.Checked == true)
                {
                    if (strAmerceOperator != strPrevAmerceOperator
                        && items.Count != 0)
                    {
                        items.Add(null);    // 插入一个空对象，表示这里要打印小计行
                    }
                }

                items.Add(item);

                if (this.checkBox_sumByAmerceOperator.Checked == true)
                {
                    strPrevAmerceOperator = strAmerceOperator;
                }
            }

            // 不要忘记 最后一个小计行
            if (this.checkBox_sumByAmerceOperator.Checked == true)
            {
                if (items.Count > 0
                    && items[items.Count - 1] != null)
                    items.Add(null);
            }

            if (items.Count == 0)
            {
                strError = "当前集合中没有状态为 新结算 的事项，因此无法打印";
                goto ERROR1;
            }

            /*

            // 对即将打印的事项进行检查，看看是不是符合结算流程
            // return:
            //      -1  出错
            //      0   正常
            //      1   有违反流程的情况出现，在strError中描述
            int nRet = CheckBeforeSettlementPrint(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
                MessageBox.Show(this, "警告: " + strError);
            */
            PrintList(items);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打印全部
        private void button_print_printAll_Click(object sender, EventArgs e)
        {
            // 检查当前排序状态和按收费者小计之间是否存在矛盾
            if (this.checkBox_sumByAmerceOperator.Checked == true)
            {

                if (this.SortColumns.Count != 0
                    && this.SortColumns[0].No == COLUMN_AMERCEOPERATOR)
                {
                }
                else
                {
                    ColumnClickEventArgs e1 = new ColumnClickEventArgs(COLUMN_AMERCEOPERATOR);
                    listView_amerced_ColumnClick(this, e1);
                    MessageBox.Show(this, "因打印要求‘按收费者小计金额’，打印前软件已自动将集合内事项按‘收费者’排序。");


                    // MessageBox.Show(this, "警告：打印要求‘按收费者小计金额’，但打印前集合内事项并未按‘收费者’排序，这样打印出的小计金额将会不准确。\r\n\r\n要避免这种情况，可在打印前用鼠标左键点‘收费者’栏标题，确保按其排序。");
                }

            }

            // 如果要打印小计行，需要在items集合中插入null对象，方便打印时针对处理
            string strPrevAmerceOperator = "";

            List<ListViewItem> items = new List<ListViewItem>();
            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];

                string strAmerceOperator = item.SubItems[COLUMN_AMERCEOPERATOR].Text;

                if (this.checkBox_sumByAmerceOperator.Checked == true)
                {
                    if (strAmerceOperator != strPrevAmerceOperator
                        && items.Count != 0)
                    {
                        items.Add(null);    // 插入一个空对象，表示这里要打印小计行
                    }
                }

                items.Add(item);

                if (this.checkBox_sumByAmerceOperator.Checked == true)
                {
                    strPrevAmerceOperator = strAmerceOperator;
                }
            }

            // 不要忘记 最后一个小计行
            if (this.checkBox_sumByAmerceOperator.Checked == true)
            {
                if (items.Count > 0
                    && items[items.Count - 1] != null)
                    items.Add(null);
            }

            string strError = "";

            // 对即将打印的事项进行检查，看看是不是符合结算流程
            // return:
            //      -1  出错
            //      0   正常
            //      1   有违反流程的情况出现，在strError中描述
            int nRet = CheckBeforeSettlementPrint(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
                MessageBox.Show(this, "警告: " + strError);

            PrintList(items);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void SettlementForm_Activated(object sender, EventArgs e)
        {
            /*
            Program.MainForm.stopManager.Active(this._stop);
            */
        }

        // 返回列表中各类事项的个数
        void GetItemTypesCount(out int nAmercedCount,
            out int nOldSettlementedCount,
            out int nNewlySettlementedCount,
            out int nOtherCount)
        {
            nAmercedCount = 0;
            nOldSettlementedCount = 0;
            nNewlySettlementedCount = 0;
            nOtherCount = 0;

            for (int i = 0; i < this.listView_amerced.Items.Count; i++)
            {
                ListViewItem item = this.listView_amerced.Items[i];
                if (item.ImageIndex == ITEMTYPE_AMERCED)
                    nAmercedCount++;
                else if (item.ImageIndex == ITEMTYPE_OLD_SETTLEMENTED)
                    nOldSettlementedCount++;
                else if (item.ImageIndex == ITEMTYPE_NEWLY_SETTLEMENTED)
                    nNewlySettlementedCount++;
                else
                    nOtherCount++;
            }
        }

        // 当任意事项的类型发生变化
        void OnItemTypeChanged()
        {
            int nAmercedCount = 0;
            int nOldSettlementedCount = 0;
            int nNewlySettlementedCount = 0;
            int nOtherCount = 0;
            GetItemTypesCount(out nAmercedCount,
                out nOldSettlementedCount,
                out nNewlySettlementedCount,
                out nOtherCount);

            if (nAmercedCount == 0)
                this.toolStripButton_items_selectAmercedItems.Enabled = false;
            else
                this.toolStripButton_items_selectAmercedItems.Enabled = true;

            if (nNewlySettlementedCount + nOldSettlementedCount == 0)
                this.toolStripButton_items_selectSettlementedItems.Enabled = false;
            else
                this.toolStripButton_items_selectSettlementedItems.Enabled = true;

            /*
            string strText = "";
            int nSelectedCount = 0;
            if (this.toolStripButton_items_useCheck.Checked == true)
            {
                strText = "勾选";
                nSelectedCount = this.listView_amerced.CheckedItems.Count;
            }
            else
            {
                strText = "选定";
                nSelectedCount = this.listView_amerced.SelectedItems.Count;
            }*/

            this.toolStripStatusLabel_items_message1.Text = "已收费 " + nAmercedCount.ToString() + ", 新结算 " + nNewlySettlementedCount.ToString() + ", 旧结算 " + nOldSettlementedCount.ToString();
            // this.label_items_message.Text = "已收费 " + nAmercedCount.ToString() + ", 新结算 " + nNewlySettlementedCount.ToString() + ", 旧结算 " + nOldSettlementedCount.ToString() + "    " + strText + " " + nSelectedCount.ToString();
        }

        // 选择发生改变
        private void listView_amerced_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.toolStripButton_items_useCheck.Checked == true)
                return;

            if (this.listView_amerced.SelectedItems.Count == 0)
            {
                this.toolStripButton_items_remove.Enabled = false;

                this.toolStripButton_items_settlement.Enabled = false;
                this.toolStripButton_items_undoSettlement.Enabled = false;

            }
            else
            {
                this.toolStripButton_items_remove.Enabled = true;

                this.toolStripButton_items_settlement.Enabled = true;
                this.toolStripButton_items_undoSettlement.Enabled = true;
            }

            this.toolStripStatusLabel_items_message2.Text = "选定 " + this.listView_amerced.SelectedItems.Count.ToString();
        }

        // 勾选发生改变
        private void listView_amerced_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (this.toolStripButton_items_useCheck.Checked == false)
                return;

            if (this.listView_amerced.CheckedItems.Count == 0)
            {
                this.toolStripButton_items_remove.Enabled = false;

                this.toolStripButton_items_settlement.Enabled = false;
                this.toolStripButton_items_undoSettlement.Enabled = false;
            }
            else
            {
                this.toolStripButton_items_remove.Enabled = true;

                this.toolStripButton_items_settlement.Enabled = true;
                this.toolStripButton_items_undoSettlement.Enabled = true;
            }

            if (e != null)
            {
                // 将checked状态的事项字体加粗，或者反之
                if (e.Item.Checked == true)
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Bold);
                else
                    e.Item.Font = new Font(e.Item.Font, FontStyle.Regular);
            }

            this.toolStripStatusLabel_items_message2.Text = "勾选 " + this.listView_amerced.CheckedItems.Count.ToString();
        }

        // 切换“使用勾选”状态
        void menu_toggleUseCheck_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_items_useCheck.Checked == true)
                this.toolStripButton_items_useCheck.Checked = false;
            else
                this.toolStripButton_items_useCheck.Checked = true;

            toolStripButton_items_useCheck_Click(sender, e);
        }

        private void toolStripButton_items_useCheck_Click(object sender, EventArgs e)
        {
            // 用何种方式来选择?

            if (this.toolStripButton_items_useCheck.Checked == true)
            {
                this.listView_amerced.CheckBoxes = true;

                // 把原来的selected变为checked
                for (int i = 0; i < this.listView_amerced.Items.Count; i++)
                {
                    ListViewItem item = this.listView_amerced.Items[i];
                    if (item.Selected == true)
                    {
                        item.Checked = true;
                        item.Selected = false;
                    }
                    else
                        item.Checked = false;
                }

                listView_amerced_ItemChecked(sender, null);
            }
            else
            {
                // 把原来的checked变为selected
                for (int i = 0; i < this.listView_amerced.Items.Count; i++)
                {
                    ListViewItem item = this.listView_amerced.Items[i];
                    if (item.Checked == true)
                    {
                        item.Selected = true;
                        item.Checked = false;

                        item.Font = new Font(item.Font, FontStyle.Regular);
                    }
                    else
                        item.Selected = false;
                }

                this.listView_amerced.CheckBoxes = false;

                listView_amerced_SelectedIndexChanged(sender, null);
            }
        }

        private void toolStripButton_items_remove_Click(object sender, EventArgs e)
        {
            menu_removeSelectedItems_Click(sender, e);
        }

        private void toolStripButton_items_selectAll_Click(object sender, EventArgs e)
        {
            menu_selectAll_Click(sender, e);
        }

        private void toolStripButton_items_unSelectAll_Click(object sender, EventArgs e)
        {
            menu_unSelectAll_Click(sender, e);
        }

        private void toolStripButton_items_selectAmercedItems_Click(object sender, EventArgs e)
        {
            menu_selectAmerced_Click(sender, e);
        }

        private void toolStripButton_items_selectSettlementedItems_Click(object sender, EventArgs e)
        {
            menu_selectSettlemented_Click(sender, e);
        }

        private void toolStripButton_items_undoSettlement_Click(object sender, EventArgs e)
        {
            SettlementAction("undosettlement");
        }

        private void toolStripButton_items_settlement_Click(object sender, EventArgs e)
        {
            SettlementAction("settlement");
        }


    }

    // 定义了特定缺省值的PrintOption派生类
    internal class SettlementPrintOption : PrintOption
    {
        public SettlementPrintOption(string strDataDir)
        {
            this.DataDir = strDataDir;

            this.PageHeaderDefault = "%date% 收费结算清单 - (共 %pagecount% 页)";
            this.PageFooterDefault = "%pageno%/%pagecount%";

            this.TableTitleDefault = "%date% 收费结算清单";

            this.LinesPerPageDefault = 20;

            // Columns缺省值
            Columns.Clear();

            // "id -- 记录ID",
            Column column = new Column();
            column.Name = "id -- 记录ID";
            column.Caption = "记录ID";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "readerBarcode -- 读者证条码号",
            column = new Column();
            column.Name = "readerBarcode -- 读者证条码号";
            column.Caption = "读者证条码号";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "reason -- 原因",
            column = new Column();
            column.Name = "reason -- 原因";
            column.Caption = "原因";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "price -- 金额",
            column = new Column();
            column.Name = "price -- 金额";
            column.Caption = "金额";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "amerceOperator -- 收费者",
            column = new Column();
            column.Name = "amerceOperator -- 收费者";
            column.Caption = "收费者";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "amerceTime -- 收费日期",
            column = new Column();
            column.Name = "amerceTime -- 收费日期";
            column.Caption = "收费日期";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "settlementOperator -- 结算者",
            column = new Column();
            column.Name = "settlementOperator -- 结算者";
            column.Caption = "结算者";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "settlementTime -- 结算日期",
            column = new Column();
            column.Name = "settlementTime -- 结算日期";
            column.Caption = "结算日期";
            column.MaxChars = -1;
            this.Columns.Add(column);

            // "recpath -- 记录路径"
            column = new Column();
            column.Name = "recpath -- 记录路径";
            column.Caption = "记录路径";
            column.MaxChars = -1;
            this.Columns.Add(column);

        }
    }
}