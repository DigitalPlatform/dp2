using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.GUI;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 种次号窗
    /// </summary>
    public partial class ZhongcihaoForm : MyForm
    {
        /// <summary>
        /// 最近导出过的记录路径文件全路径
        /// </summary>
        public string ExportRecPathFilename = "";   // 使用过的导出路径文件

        /// <summary>
        /// 检索结束信号
        /// </summary>
        public AutoResetEvent EventFinish = new AutoResetEvent(false);

        string m_strMaxNumber = null;
        string m_strTailNumber = null;

        /// <summary>
        /// 是否要(在窗口打开后)自动启动检索
        /// </summary>
        public bool AutoBeginSearch = false;

        const int WM_INITIAL = API.WM_USER + 201;

        const int TYPE_NORMAL = 0;
        const int TYPE_ERROR = 1;
        const int TYPE_CURRENT = 2;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ZhongcihaoForm()
        {
            this.UseLooping = true; // 2022/11/4

            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_number.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);
        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            e.ColumnTitles = new ColumnPropertyCollection();
            ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(e.DbName);
            if (temp != null)
                e.ColumnTitles.AddRange(temp);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件

            /*
            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");
             * */
        }

        private void ZhongcihaoForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }

            // 类号
            if (String.IsNullOrEmpty(this.textBox_classNumber.Text) == true)
            {
                this.textBox_classNumber.Text = Program.MainForm.AppInfo.GetString(
                    "zhongcihao_form",
                    "classnumber",
                    "");
            }

            // 线索书目库名
            if (String.IsNullOrEmpty(this.comboBox_biblioDbName.Text) == true)
            {
                this.comboBox_biblioDbName.Text = Program.MainForm.AppInfo.GetString(
                    "zhongcihao_form",
                    "biblio_dbname",
                    "");
            }

            // 是否要返回浏览列
            this.checkBox_returnBrowseCols.Checked = Program.MainForm.AppInfo.GetBoolean(
                    "zhongcihao_form",
                    "return_browse_cols",
                    true);

            string strWidths = Program.MainForm.AppInfo.GetString(
"zhongcihao_form",
"record_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_number,
                    strWidths,
                    true);
            }


            if (this.AutoBeginSearch == true)
            {
                API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
            }

        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_INITIAL:
                    {
                        this.button_searchDouble_Click(null, null);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }


        private void ZhongcihaoForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void ZhongcihaoForm_FormClosed(object sender, FormClosedEventArgs e)
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
                // 类号
                Program.MainForm.AppInfo.SetString(
                    "zhongcihao_form",
                    "classnumber",
                    this.textBox_classNumber.Text);

                // 线索书目库名
                Program.MainForm.AppInfo.SetString(
                    "zhongcihao_form",
                    "biblio_dbname",
                    this.comboBox_biblioDbName.Text);

                // 是否要返回浏览列
                Program.MainForm.AppInfo.SetBoolean(
            "zhongcihao_form",
            "return_browse_cols",
            this.checkBox_returnBrowseCols.Checked);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_number);
                Program.MainForm.AppInfo.SetString(
                    "zhongcihao_form",
                    "record_list_column_width",
                    strWidths);
            }

            EventFinish.Set();
        }

        /// <summary>
        /// 书目库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return this.comboBox_biblioDbName.Text;
            }
            set
            {
                this.comboBox_biblioDbName.Text = value;
            }
        }

        string _biblioRecPath = "";

        // 发起取号的书目记录的路径。用来校正统计过程，排除自己。
        /// <summary>
        /// 发起取号的书目记录的路径
        /// </summary>
        public string MyselfBiblioRecPath
        {
            get
            {
                return this._biblioRecPath;
            }
            set
            {
                this._biblioRecPath = value;

                // 2014/4/9
                string strBiblioDbName = Global.GetDbName(value);
                if (string.IsNullOrEmpty(strBiblioDbName) == false)
                    this.BiblioDbName = strBiblioDbName;
            }
        }

        /// <summary>
        /// 类号
        /// </summary>
        public string ClassNumber
        {
            get
            {
                return this.textBox_classNumber.Text;
            }
            set
            {
                this.textBox_classNumber.Text = value;
            }
        }


        /// <summary>
        /// 最大号
        /// </summary>
        public string MaxNumber
        {
            get
            {
                if (String.IsNullOrEmpty(m_strMaxNumber) == true)
                {
                    string strError = "";

                    int nRet = FillList(true, out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return m_strMaxNumber;
                ERROR1:
                    throw (new Exception(strError));
                }
                return m_strMaxNumber;
            }
            set
            {
                this.textBox_maxNumber.Text = value;
                m_strMaxNumber = value;
            }
        }

        /// <summary>
        /// 尾号
        /// </summary>
        public string TailNumber
        {
            get
            {
                if (String.IsNullOrEmpty(m_strTailNumber) == true)
                {
                    string strError = "";

                    string strTailNumber = "";
                    int nRet = SearchTailNumber(out strTailNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    m_strTailNumber = strTailNumber;
                    return m_strTailNumber;
                ERROR1:
                    throw (new Exception(strError));

                }
                return m_strTailNumber;
            }
            set
            {
                string strError = "";
                string strOutputNumber = "";
                int nRet = SaveTailNumber(value,
                    out strOutputNumber,
                    out strError);
                if (nRet == -1)
                    throw (new Exception(strError));
                else
                    m_strTailNumber = strOutputNumber;	// 刷新记忆
            }
        }


        // 检索
        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                int nRet = FillList(true, out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 不获得本类尾号
                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.TryInvoke((Action)(() =>
            {
                this.comboBox_biblioDbName.Enabled = bEnable;
                this.textBox_classNumber.Enabled = bEnable;
                this.textBox_maxNumber.Enabled = bEnable;
                this.textBox_tailNumber.Enabled = bEnable;

                this.button_copyMaxNumber.Enabled = bEnable;
                this.button_getTailNumber.Enabled = bEnable;
                this.button_pushTailNumber.Enabled = bEnable;
                this.button_saveTailNumber.Enabled = bEnable;
                this.button_searchClass.Enabled = bEnable;
                this.button_searchDouble.Enabled = bEnable;
            }));
        }

        int FillList(bool bSort,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            this.listView_number.Items.Clear();
            this.listView_number.ListViewItemSorter = null;
            this.MaxNumber = "";

            /*
            if (dom == null)
            {
                strError = "请先调用GetGlobalCfgFile()函数";
                return -1;
            }
             * */

            if (this.ClassNumber == "")
            {
                strError = "尚未指定分类号";
                return -1;
            }

            if (this.BiblioDbName == "")
            {
                strError = "尚未指定书目库名";
                return -1;
            }

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在检索同类书记录 ...");
            _stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在检索同类书记录 ...",
                "disableControl");
            try
            {
                long lRet = channel.SearchUsedZhongcihao(
                    looping.Progress,
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    "zhongcihao",
                    out string strQueryXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0)
                {
                    strError = "没有命中的记录。";
                    return 0;   // not found
                }

                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);

                ZhongcihaoSearchResult[] searchresults = null;

                if (looping != null)
                    looping.Progress.SetProgressRange(0, lHitCount);

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (looping.Stopped)
                    {
                        strError = "用户中断";
                        goto ERROR1;
                    }

                    long lCurrentPerCount = lPerCount;

                    bool bShift = Control.ModifierKeys == Keys.Shift;
                    string strBrowseStyle = "cols";
                    if (bShift == true || this.checkBox_returnBrowseCols.Checked == false)
                    {
                        strBrowseStyle = "";
                        lCurrentPerCount = lPerCount * 10;
                    }

                    looping.Progress.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = channel.GetZhongcihaoSearchResult(
                        looping.Progress,
                        GetZhongcihaoDbGroupName(this.BiblioDbName),
                        // "!" + this.BiblioDbName,
                        "zhongcihao",   // strResultSetName
                        lStart,
                        lCurrentPerCount,
                        strBrowseStyle, // style
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

                    // 处理浏览结果
                    this.listView_number.BeginUpdate();
                    for (int i = 0; i < searchresults.Length; i++)
                    {
                        ZhongcihaoSearchResult result_item = searchresults[i];
                        ListViewItem item = new ListViewItem();
                        item.Text = result_item.Path;

                        item.SubItems.Add(result_item.Zhongcihao);

#if NO
                        if (CheckNumber(result_item.Zhongcihao) == true)
                            item.ImageIndex = TYPE_NORMAL;
                        else
                            item.ImageIndex = TYPE_ERROR;
#endif
                        item.ImageIndex = TYPE_NORMAL;

                        if (result_item.Cols != null)
                        {
                            ListViewUtil.EnsureColumns(this.listView_number, result_item.Cols.Length + 1);
                            for (int j = 0; j < result_item.Cols.Length; j++)
                            {
                                ListViewUtil.ChangeItemText(item, j + 2, result_item.Cols[j]);
                            }
                        }

                        this.listView_number.Items.Add(item);
                        if (looping != null)
                            looping.Progress.SetProgressValue(lStart + i + 1);
                    }
                    this.listView_number.EndUpdate();

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
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

                EnableControls(true);
                */
            }

            if (bSort == true)
            {
                // 排序
                this.listView_number.ListViewItemSorter = new ZhongcihaoListViewItemComparer();
                this.listView_number.ListViewItemSorter = null;

                // 把重复种次号的事项用特殊颜色标出来
                ColorDup();

                EnsureStartRecordVisible();

                this.MaxNumber = GetTopNumber(this.listView_number);    // this.listView_number.Items[0].SubItems[1].Text;
            }

            return 0;
        ERROR1:
            return -1;
        }

        // 让发起记录卷滚进入可见范围
        void EnsureStartRecordVisible()
        {
            if (string.IsNullOrEmpty(this.MyselfBiblioRecPath) == true)
                return;
            ListViewItem item = ListViewUtil.FindItem(this.listView_number, this.MyselfBiblioRecPath, 0);
            if (item != null)
            {
                item.ImageIndex = TYPE_CURRENT;
                item.Font = new Font(item.Font, FontStyle.Bold);
                item.BackColor = Color.Yellow;
                item.EnsureVisible();
            }
        }

        // 检查种次号格式是否正确
        // 种次号必须为纯数字
        // return:
        //      true    正确
        //      false   错误
        static bool CheckNumber(string strText)
        {
            if (StringUtil.IsPureNumber(strText) == true)
                return true;

            return false;
        }

        // 从已经排序的事项中，取出位置最高事项的种次号。
        // 本函数会自动排除MyselfBiblioRecPath这条记录
        string GetTopNumber(ListView list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                string strRecPath = item.Text;
                if (strRecPath != this.MyselfBiblioRecPath)
                    return item.SubItems[1].Text;
            }

            // TODO: 如果除了自己以外，并没有其他包含有效种次号的事项了，那也只好用自己的种次号-1来充当？

            return "";  // 没有找到
        }

        // 使相邻重复行变色
        void ColorDup()
        {
            string strPrevNumber = "";
            Color color1 = Color.FromArgb(220, 220, 220);
            Color color2 = Color.FromArgb(230, 230, 230);
            Color color = color1;
            int nDupCount = 0;
            for (int i = 0; i < this.listView_number.Items.Count; i++)
            {
                string strNumber = this.listView_number.Items[i].SubItems[1].Text;

                // 2014/4/9
                // 截出第一部分进行比较
                int index = strNumber.IndexOfAny(new char[] { '/', '.', ',', '=', '-', '#' });
                if (index != -1)
                    strNumber = strNumber.Substring(0, index);

                if (strNumber == strPrevNumber)
                {
                    if (i >= 1 && nDupCount == 0)
                        this.listView_number.Items[i - 1].BackColor = color;

                    this.listView_number.Items[i].BackColor = color;
                    nDupCount++;
                }
                else
                {
                    if (nDupCount >= 1)
                    {
                        // 换一下颜色
                        if (color == color1)
                            color = color2;
                        else
                            color = color1;
                    }

                    nDupCount = 0;

                    this.listView_number.Items[i].BackColor = SystemColors.Window;

                }


                strPrevNumber = strNumber;
            }

        }


        // 检索尾号，放入面板中界面元素
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int PanelGetTailNumber(out string strError)
        {
            strError = "";
            this.textBox_tailNumber.Text = "";

            string strTailNumber = "";
            int nRet = SearchTailNumber(out strTailNumber,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 0;

            this.textBox_tailNumber.Text = strTailNumber;
            // this.label_tailNumberTitle.Text = "库'" + this.ZhongcihaoDbName + "'中的尾号(&T):";
            return 1;
        }


        /// <summary>
        ///  检索获得种次号库中对应类目的尾号。此功能比较单纯，所获得的结果并不放入面板界面元素
        /// </summary>
        /// <param name="strTailNumber">返回尾号</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1出错;0没有找到;1找到</returns>
        public int SearchTailNumber(
            out string strTailNumber,
            out string strError)
        {
            strTailNumber = "";

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获得尾号 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获得尾号 ...",
                "disableControl");
            try
            {
                long lRet = channel.GetZhongcihaoTailNumber(
                    looping.Progress,
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    out strTailNumber,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */
            }
        }

        // 推动尾号。如果已经存在的尾号比strTestNumber还要大，则不推动
        /// <summary>
        /// 推动尾号。如果已经存在的尾号比strTestNumber还要大，则不推动
        /// </summary>
        /// <param name="strTestNumber">用于比对的尾号</param>
        /// <param name="strOutputNumber">返回推动后的尾号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int PushTailNumber(string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在推动尾号 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在推动尾号 ...",
                "disableControl");
            try
            {
                long lRet = channel.SetZhongcihaoTailNumber(
                    looping.Progress,
                    "conditionalpush",
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strTestNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */
            }
        }

        /// <summary>
        /// 设置尾号
        /// </summary>
        /// <param name="strTailNumber">要设置的尾号</param>
        /// <param name="strOutputNumber">实际设置的尾号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int SaveTailNumber(
            string strTailNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在保存尾号 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在保存尾号 ...",
                "disableControl");
            try
            {
                long lRet = channel.SetZhongcihaoTailNumber(
                    looping.Progress,
                    "save",
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strTailNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */
            }
        }

        // 获得尾号
        private void button_getTailNumber_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                this.textBox_tailNumber.Text = "";   // 预先清空，以防误会

                // 获得本类尾号
                int nRet = PanelGetTailNumber(out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "类 '" + this.ClassNumber + "' 的尾号尚不存在";
                    goto ERROR1;
                }

                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 保存尾号
        private void button_saveTailNumber_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.textBox_tailNumber.Text == "")
            {
                strError = "尚未输入要保存的尾号";
                goto ERROR1;
            }

            EventFinish.Reset();
            try
            {
                string strOutputNumber = "";

                // 保存本类尾号
                int nRet = SaveTailNumber(this.textBox_tailNumber.Text,
                    out strOutputNumber,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 用检索得到的同类书中实际用到的最大号，试探性推动种次号库中的尾号
        private void button_pushTailNumber_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strOutputNumber = "";
            // 推动尾号
            int nRet = PushTailNumber(this.textBox_maxNumber.Text,
                out strOutputNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.textBox_tailNumber.Text = strOutputNumber;
            // MessageBox.Show(this, "推动尾号成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 得到当前书目中统计出来的最大号的加1以后的号
        // return:
        //      -1  error
        //      0   not found
        //      1   succeed
        /// <summary>
        /// 得到当前书目中统计出来的最大号的加1以后的号
        /// </summary>
        /// <param name="strResult">返回结果</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int GetMaxNumberPlusOne(out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";
            string strMaxNumber = "";

            try
            {
                strMaxNumber = this.MaxNumber;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strMaxNumber) == true)
                return 0;

            int nRet = StringUtil.IncreaseLeadNumber(strMaxNumber,
                1,
                out strResult,
                out strError);
            if (nRet == -1)
            {
                strError = "为数字 '" + strMaxNumber + "' 增量时发生错误: " + strError;
                goto ERROR1;

            }
            return 1;
        ERROR1:
            return -1;
        }

        // 复制比当前书目中统计出来的最大号还大1的号
        private void button_copyMaxNumber_Click(object sender, EventArgs e)
        {
            string strResult = "";
            string strError = "";

            // 得到当前书目中统计出来的最大号的加1以后的号
            // return:
            //      -1  error
            //      1   succeed
            int nRet = GetMaxNumberPlusOne(out strResult,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            if (nRet == 0)
                strResult = "1";    // 如果当前从书目中无法统计出最大号，则视为得到"0"，而加1以后正好为"1"

            // Clipboard.SetDataObject(strResult);
            StringUtil.RunClipboard(() =>
            {
                Clipboard.SetDataObject(strResult);
            });
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 检索两种：同类书、尾号
        private void button_searchDouble_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                this.textBox_tailNumber.Text = "";   // 预防filllist 提前退出, 忘记处理

                int nRet = FillList(true, out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 一并获得本类尾号
                nRet = PanelGetTailNumber(out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void comboBox_biblioDbName_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_biblioDbName.Items.Count > 0)
                return;

            // this.comboBox_biblioDbName.Items.Add("<全部>");

            if (Program.MainForm.BiblioDbProperties != null)
            {
                foreach (var property in Program.MainForm.BiblioDbProperties)
                {
                    // BiblioDbProperty property = Program.MainForm.BiblioDbProperties[i];
                    this.comboBox_biblioDbName.Items.Add(property.DbName);
                }
            }
        }

        // 将面板上输入的线索数据库名或者种次号方案名变换为API使用的形态
        static string GetZhongcihaoDbGroupName(string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return "";

            // 如果第一个字符有!符号，表明是方案名
            if (strText[0] == '!')
                return strText.Substring(1);

            // 没有！符号，表明是线索数据库名
            return "!" + strText;
        }

        // 增量尾号
        /// <summary>
        /// 增量尾号
        /// </summary>
        /// <param name="strDefaultNumber">缺省时的尾号。如果当前类没有尾号，则使用它</param>
        /// <param name="strOutputNumber">返回增量后的尾号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int IncreaseTailNumber(string strDefaultNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            /*
            EnableControls(false);

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在增量尾号 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在增量尾号 ...",
                "disableControl");
            try
            {
                long lRet = channel.SetZhongcihaoTailNumber(
                    looping.Progress,
                    "increase",
                    GetZhongcihaoDbGroupName(this.BiblioDbName),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strDefaultNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    return -1;

                return (int)lRet;
            }
            finally
            {
                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                EnableControls(true);
                */
            }
        }

        #region 协调外部调用的函数

        /// <summary>
        /// 等待检索结束
        /// </summary>
        public void WaitSearchFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }

        #endregion

        // 按照一定的策略，获得种次号
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 按照一定的策略，获得种次号
        /// </summary>
        /// <param name="style">种次号取号的风格</param>
        /// <param name="strClass">类号</param>
        /// <param name="strBiblioDbName">书目库名</param>
        /// <param name="strNumber">返回种次号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int GetNumber(
            ZhongcihaoStyle style,
            string strClass,
            string strBiblioDbName,
            out string strNumber,
            out string strError)
        {
            strNumber = "";
            strError = "";
            int nRet = 0;

            this.ClassNumber = strClass;
            this.BiblioDbName = strBiblioDbName;

            // 仅利用书目统计最大号
            if (style == ZhongcihaoStyle.Biblio)
            {
                // 得到当前书目中统计出来的最大号的加1以后的号
                // return:
                //      -1  error
                //      1   succeed
                nRet = GetMaxNumberPlusOne(out strNumber,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 1)
                    return 1;

                // 2009/2/25
                Debug.Assert(nRet == 0, "");

                // 此类从来没有过记录，当前是第一条
                strNumber = InputDlg.GetInput(
                    this,
                    null,
                    "请输入类 '" + strClass + "' 的当前种次号最大号:",
                    "1",
            Program.MainForm.DefaultFont);
                if (strNumber == null)
                    return 0;	// 放弃整个操作

                return 1;
            }

            // 每次都利用书目统计最大号来检验、校正尾号
            if (style == ZhongcihaoStyle.BiblioAndSeed
                || style == ZhongcihaoStyle.SeedAndBiblio)
            {

                string strTailNumber = this.TailNumber;

                // 如果本类尚未创建种次号条目
                if (String.IsNullOrEmpty(strTailNumber) == true)
                {
                    // 毕竟初始值还是利用了统计结果
                    string strTestNumber = "";
                    // 得到当前书目中统计出来的最大号的加1以后的号
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   succeed
                    nRet = GetMaxNumberPlusOne(out strTestNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                        strTestNumber = "1";

                    // 此类从来没有过记录，当前是第一条
                    strNumber = InputDlg.GetInput(
                        this,
                        null,
                        "请输入类 '" + strClass + "' 的当前种次号最大号:",
                        strTestNumber,
            Program.MainForm.DefaultFont);
                    if (strNumber == null)
                        return 0;	// 放弃整个操作

                    // dlg.TailNumber = strNumber;	

                    nRet = PushTailNumber(strNumber,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return 1;
                }
                else // 本类已经有种次号条目
                {
                    // 检查和统计值的关系
                    string strTestNumber = "";
                    // 得到当前书目中统计出来的最大号的加1以后的号
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   succeed
                    nRet = GetMaxNumberPlusOne(out strTestNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                    {
                        // 依靠现有尾号增量即可
                        nRet = this.IncreaseTailNumber("1",
                            out strNumber,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        return 1;
                    }

                    // 用统计出来的号推动当前尾号，就起到了检验的作用
                    nRet = PushTailNumber(strTestNumber,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 如果到这里就返回，效果为保守型增量，即如果当前记录反复取号而不保存，则尾号不盲目增量。当然缺点也是很明显的 -- 有可能多个窗口取出重号来
                    if (style == ZhongcihaoStyle.BiblioAndSeed)
                        return 1;

                    if (strTailNumber != strNumber)  // 如果实际发生了推动，就要这个号，不必增量了
                        return 1;

                    // 依靠现有尾号增量
                    nRet = this.IncreaseTailNumber("1",
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return 1;
                }

                // return 1;
            }

            // 仅利用(种次号库)尾号
            if (style == ZhongcihaoStyle.Seed)
            {
                string strTailNumber = this.TailNumber;

                // 如果本类尚未创建种次号条目
                if (String.IsNullOrEmpty(strTailNumber) == true)
                {
                    // 毕竟初始值还是利用了统计结果
                    string strTestNumber = "";
                    // 得到当前书目中统计出来的最大号的加1以后的号
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   succeed
                    nRet = GetMaxNumberPlusOne(out strTestNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                        strTestNumber = "1";
                    // 此类从来没有过记录，当前是第一条
                    strNumber = InputDlg.GetInput(
                        this,
                        null,
                        "请输入类 '" + strClass + "' 的当前种次号最大号:",
                        strTestNumber,
            Program.MainForm.DefaultFont);
                    if (strNumber == null)
                        return 0;	// 放弃整个操作

                    // dlg.TailNumber = strNumber;	

                    nRet = PushTailNumber(strNumber,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    return 1;
                }
                else // 本类已经有种次号项目，增量即可
                {
                    nRet = this.IncreaseTailNumber("1",
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }

                return 1;
            }





            return 1;
        ERROR1:
            return -1;
        }

        // 双击：将书目记录装入详细窗
        private void listView_number_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_number.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要装入详细窗的事项");
                return;
            }
            string strPath = this.listView_number.SelectedItems[0].SubItems[0].Text;

            EntityForm form = new EntityForm();

            form.MdiParent = Program.MainForm;

            form.MainForm = Program.MainForm;
            form.Show();
            form.LoadRecordOld(strPath, "", true);

        }

        private void ZhongcihaoForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);

        }

        private void listView_number_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;



            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAllLines_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导出所选择的 " + this.listView_number.SelectedItems.Count.ToString() + " 个事项到记录路径文件(&S)...");
            menuItem.Click += new System.EventHandler(this.menu_saveToRecordPathFile_Click);
            if (this.listView_number.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_number, new Point(e.X, e.Y));
        }

        void menu_selectAllLines_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                ListViewUtil.SelectAllLines(this.listView_number);
            }
            finally
            {
                this.Cursor = oldCursor;
            }
        }

        // 保存到记录路径文件
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
                    "BiblioSearchForm",
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
            using (StreamWriter sw = new StreamWriter(this.ExportRecPathFilename,
                bAppend,	// append
                System.Text.Encoding.UTF8))
            {
                Cursor oldCursor = this.Cursor;
                this.Cursor = Cursors.WaitCursor;

                foreach (ListViewItem item in this.listView_number.SelectedItems)
                {
                    // ListViewItem item = this.listView_number.SelectedItems[i];
                    sw.WriteLine(item.Text);
                }

                this.Cursor = oldCursor;
            }

            string strExportStyle = "导出";
            if (bAppend == true)
                strExportStyle = "追加";

            Program.MainForm.StatusBarMessage = "书目记录路径 " + this.listView_number.SelectedItems.Count.ToString() + "个 已成功" + strExportStyle + "到文件 " + this.ExportRecPathFilename;
        }

        private void listView_number_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSelectedIndexChanged(this.listView_number,
                0,
                new List<int> { 1 });
        }

    }

    // 排序
    // Implements the manual sorting of items by columns.
    class ZhongcihaoListViewItemComparer : IComparer
    {
        public ZhongcihaoListViewItemComparer()
        {
        }

        public int Compare(object x, object y)
        {
            // 种次号字符串需要右对齐 2007/10/12
            string s1 = ((ListViewItem)x).SubItems[1].Text;
            string s2 = ((ListViewItem)y).SubItems[1].Text;

#if NO
            CanonicalString(ref s1, ref s2);
            return -1 * String.Compare(s1, s2);
#endif
            return -1 * CompareString(s1, s2);
        }

        // 比较两个字符串
        // 先按照 / 切割为多个部分。然后每个部分进行互相比较
        static int CompareString(string s1, string s2)
        {
            string[] parts1 = s1.Split(new char[] { '/' });
            string[] parts2 = s2.Split(new char[] { '/' });

            int nCount = Math.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < nCount; i++)
            {
                if (i >= parts1.Length)
                    return -1;
                if (i >= parts2.Length)
                    return 1;

                string p1 = parts1[i];
                string p2 = parts2[i];

                CanonicalString(ref p1, ref p2);
                int nRet = String.Compare(s1, s2);
                if (nRet != 0)
                    return nRet;

            }

            return 0;
        }

        // 2008/9/19
        // 正规化即将比较的字符串
        // 按照'.'等切割符号，从左到右逐段规范化为彼此等长
        static void CanonicalString(ref string s1, ref string s2)
        {
            string[] a1 = s1.Split(new char[] { '.', ',', '=', '-', '#' });
            string[] a2 = s2.Split(new char[] { '.', ',', '=', '-', '#' });

            string result1 = "";
            string result2 = "";
            int i = 0;
            for (; ; i++)
            {
                if (i >= a1.Length)
                    break;
                if (i >= a2.Length)
                    break;
                string c1 = a1[i];
                string c2 = a2[i];
                int nMaxLength = Math.Max(c1.Length, c2.Length);
                result1 += c1.PadLeft(nMaxLength, '0') + ".";
                result2 += c2.PadLeft(nMaxLength, '0') + ".";
            }

            for (int j = i + 1; j < a1.Length; j++)
            {
                result1 += a1[j] + ".";
            }

            for (int j = i + 1; j < a2.Length; j++)
            {
                result2 += a2[j] + ".";
            }

            s1 = result1;
            s2 = result2;
        }

    }

    // 
    /// <summary>
    /// 种次号取号的风格
    /// </summary>
    public enum ZhongcihaoStyle
    {
        /// <summary>
        /// 仅利用书目统计最大号
        /// </summary>
        Biblio = 1, // 仅利用书目统计最大号
        /// <summary>
        /// 每次都利用书目统计最大号来检验、校正尾号。偏重书目统计值，不盲目增量尾号
        /// </summary>
        BiblioAndSeed = 2,  // 每次都利用书目统计最大号来检验、校正尾号。偏重书目统计值，不盲目增量尾号。
        /// <summary>
        /// 每次都利用书目统计最大号来检验、校正尾号。偏重(尾号库的)尾号，每次都增量尾号
        /// </summary>
        SeedAndBiblio = 3, // 每次都利用书目统计最大号来检验、校正尾号。偏重尾号，每次都增量尾号
        /// <summary>
        /// 仅利用(种次号库)尾号
        /// </summary>
        Seed = 4, // 仅利用(种次号库)尾号
    }

}