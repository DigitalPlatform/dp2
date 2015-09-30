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
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    // 索取号窗
    // 位于Util目录
    /// <summary>
    /// 索取号窗
    /// </summary>
    public partial class CallNumberForm : MyForm
    {
        // XmlDocument cfg_dom = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

#if NO
        public LibraryChannel Channel = new LibraryChannel();
        public string Lang = "zh";

        public MainForm MainForm = null;
        DigitalPlatform.Stop stop = null;
#endif

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

        /// <summary>
        /// 发起取号的实体记录的路径。用来校正统计过程，排除自己
        /// </summary>
        public string MyselfItemRecPath = "";    // 发起取号的实体记录的路径。用来校正统计过程，排除自己。
        
        /// <summary>
        /// 发起取号的实体记录所从属的书目记录的路径。用来校正统计过程，排除和自己同属于一条书目记录的其它实体记录
        /// </summary>
        public string MyselfParentRecPath = "";    // 发起取号的实体记录所从属的书目记录的路径。用来校正统计过程，排除和自己同属于一条书目记录的其它实体记录。

        /// <summary>
        /// 发起取号的书目记录下属的实体记录中的最新索取号
        /// </summary>
        public List<CallNumberItem> MyselfCallNumberItems = null;   // 发起取号的书目记录下属的实体记录中的最新索取号

        const int TYPE_NORMAL = 0;
        const int TYPE_ERROR = 1;
        const int TYPE_CURRENT = 2;

        #region 浏览列号

        /// <summary>
        /// 浏览列号: 册记录路径
        /// </summary>
        public const int COLUMN_ITEMRECPATH = 0;
        /// <summary>
        /// 浏览列号: 索取号
        /// </summary>
        public const int COLUMN_CALLNUMBER = 1;
        /// <summary>
        /// 浏览列号: 摘要
        /// </summary>
        public const int COLUMN_SUMMARY = 2;
        /// <summary>
        /// 浏览列号: 馆藏地
        /// </summary>
        public const int COLUMN_LOCATION = 3;
        /// <summary>
        /// 浏览列号: 册条码号
        /// </summary>
        public const int COLUMN_BARCODE = 4;
        /// <summary>
        /// 浏览列号: 书目记录路径
        /// </summary>
        public const int COLUMN_BIBLIORECPATH = 5;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public CallNumberForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_number.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.RecPath);
        }

        private void CallNumberForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

#if NO
            this.Channel.Url = this.MainForm.LibraryServerUrl;

            this.Channel.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            this.Channel.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);

            stop = new DigitalPlatform.Stop();
            stop.Register(MainForm.stopManager, true);	// 和容器关联
#endif

            this.GetValueTable -= new GetValueTableEventHandler(CallNumberForm_GetValueTable);
            this.GetValueTable += new GetValueTableEventHandler(CallNumberForm_GetValueTable);


            // 类号
            if (String.IsNullOrEmpty(this.textBox_classNumber.Text) == true)
            {
                this.textBox_classNumber.Text = this.MainForm.AppInfo.GetString(
                    "callnumberform",
                    "classnumber",
                    "");
            }

            // 线索馆藏地点
            if (m_bLocationSetted == false)
            {
                this.comboBox_location.Text = this.MainForm.AppInfo.GetString(
                    "callnumberform",
                    "location",
                    "");
                m_bLocationSetted = true;
            }

            // 是否要返回浏览列
            this.checkBox_returnBrowseCols.Checked = this.MainForm.AppInfo.GetBoolean(
                    "callnumberform",
                    "return_browse_cols",
                    true);

            string strWidths = this.MainForm.AppInfo.GetString(
    "callnumberform",
    "record_list_column_width",
    "");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_number,
                    strWidths,
                    true);
            }

            /*
            if (this.cfg_dom == null)
            {
                string strError = "";
                // 初始化索取号配置信息
                int nRet = InitialCallNumberCfgInfo(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }*/

            if (this.AutoBeginSearch == true)
            {
                API.PostMessage(this.Handle, WM_INITIAL, 0, 0);
            }
        }

        void CallNumberForm_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
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
                        string strError = "";
                        int nRet = 0;

                        ArrangementInfo info = null;
        // return:
        //      -1  error
        //      0   not found
        //      1   found
                        nRet = this.MainForm.GetArrangementInfo(this.LocationString,
                            out info, 
                            out strError);

#if NO
                        string strArrangeGroupName = "";
                        string strZhongcihaoDbname = "";
                        string strClassType = "";
                        string strQufenhaoType = "";

                        // 获得关于一个特定馆藏地点的索取号配置信息
                        // return:
                        //      -1  error
                        //      0   notd found
                        //      1   found
                        nRet = this.MainForm.GetCallNumberInfo(this.LocationString,
                            out strArrangeGroupName,
                            out strZhongcihaoDbname,
                            out strClassType,
                            out strQufenhaoType,
                            out strError);
#endif
                        if (nRet == 0)
                        {
                            this.button_searchClass_Click(null, null);
                            return;
                        }
                        if (nRet == -1)
                        {
                            this.button_searchClass_Click(null, null);
                            return;
                        }

                        if (String.IsNullOrEmpty(info.ZhongcihaoDbname) == true) // strZhongcihaoDbname
                        {
                            this.button_searchClass_Click(null, null);
                        }
                        else
                        {
                            this.button_searchDouble_Click(null, null);
                        }
                        return;
                        /*
                    ERROR1:
                        MessageBox.Show(this, strError);
                        return;
                         * */
                    }
//                    return;
            }
            base.DefWndProc(ref m);
        }

        private void CallNumberForm_FormClosing(object sender, FormClosingEventArgs e)
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

        private void CallNumberForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联
                stop = null;
            }
#endif

            // 类号
            this.MainForm.AppInfo.SetString(
                "callnumberform",
                "classnumber",
                this.textBox_classNumber.Text);

            // 线索馆藏地点
            this.MainForm.AppInfo.SetString(
                "callnumberform",
                "location",
                this.comboBox_location.Text);

            // 是否要返回浏览列
            this.MainForm.AppInfo.SetBoolean(
                    "callnumberform",
                    "return_browse_cols",
                    this.checkBox_returnBrowseCols.Checked);

            string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_number);
            this.MainForm.AppInfo.SetString(
                "callnumberform",
                "record_list_column_width",
                strWidths);


            EventFinish.Set();
        }

#if NO
        void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            this.MainForm.Channel_BeforeLogin(this, e);
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }
#endif

        bool m_bLocationSetted = false;
        /// <summary>
        /// 线索馆藏地点
        /// </summary>
        public string LocationString
        {
            get
            {
                if (this.comboBox_location.Text == "<空>")
                    return "";

                return this.comboBox_location.Text;
            }
            set
            {
                this.comboBox_location.Text = value;
                m_bLocationSetted = true;
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

                    int nRet = FillList(true, 
                        "",
                        out strError);
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

        private void button_searchClass_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                int nRet = FillList(true, 
                    "",
                    out strError);
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
            this.comboBox_location.Enabled = bEnable;
            this.textBox_classNumber.Enabled = bEnable;
            this.textBox_maxNumber.Enabled = bEnable;
            this.textBox_tailNumber.Enabled = bEnable;

            this.button_copyMaxNumber.Enabled = bEnable;
            this.button_getTailNumber.Enabled = bEnable;
            this.button_pushTailNumber.Enabled = bEnable;
            this.button_saveTailNumber.Enabled = bEnable;
            this.button_searchClass.Enabled = bEnable;
            this.button_searchDouble.Enabled = bEnable;
        }

        // 将面板上输入的线索数据库名或者种次号方案名变换为API使用的形态
        static string GetArrangeGroupName(string strLocation)
        {
            if (strLocation == "<空>")
                strLocation = "";

            // 馆藏地点名为空是许可的
            if (String.IsNullOrEmpty(strLocation) == true)
                return "!";

            // 如果第一个字符有!符号，表明是方案名
            if (strLocation[0] == '!')
                return strLocation.Substring(1);

            // 没有！符号，表明是线索馆藏地点名
            return "!" + strLocation;
        }

        int FillList(bool bSort,
            string strStyle,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            Hashtable dbname_table = new Hashtable();   // 实体库名 --> 书目库名 对照表

            this.listView_number.Items.Clear();
            this.MaxNumber = "";

            if (this.ClassNumber == "")
            {
                strError = "尚未指定分类号";
                return -1;
            }

            /*
            if (this.LocationString == "")
            {
                strError = "尚未指定线索馆藏地点";
                return -1;
            }
             * */

            bool bFast = StringUtil.IsInList("fast", strStyle);

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索同类书实体记录 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                string strQueryXml = "";

                long lRet = Channel.SearchOneClassCallNumber(
                    stop,
                    GetArrangeGroupName(this.LocationString),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    "callnumber",
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                if (lRet == 0)
                {
                    strError = "没有命中的记录。";
                    // return 0;   // not found
                    goto END1;
                }


                long lHitCount = lRet;

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                CallNumberSearchResult[] searchresults = null;

                if (stop != null)
                    stop.SetProgressRange(0, lHitCount);

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                        {
                            strError = "用户中断";
                            goto ERROR1;
                        }
                    }

                    long lCurrentPerCount = lPerCount;

                    bool bShift = Control.ModifierKeys == Keys.Shift;
                    string strBrowseStyle = "cols";
                    if (bShift == true || this.checkBox_returnBrowseCols.Checked == false
                        || bFast == true)
                    {
                        strBrowseStyle = "";
                        lCurrentPerCount = lPerCount * 10;
                    }

                    stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = Channel.GetCallNumberSearchResult(
                        stop,
                        GetArrangeGroupName(this.LocationString),
                        // "!" + this.BiblioDbName,
                        "callnumber",   // strResultSetName
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
                        CallNumberSearchResult result_item = searchresults[i];
                        ListViewItem item = new ListViewItem();
                        item.ImageIndex = TYPE_NORMAL;
                        item.Text = result_item.ItemRecPath;

                        if (String.IsNullOrEmpty(result_item.ErrorInfo) == false)
                        {
                            ListViewUtil.ChangeItemText(item, COLUMN_CALLNUMBER, result_item.ErrorInfo);
                            item.ImageIndex = TYPE_ERROR;
                        }
                        else
                            ListViewUtil.ChangeItemText(item, COLUMN_CALLNUMBER, result_item.CallNumber);

                        ListViewUtil.ChangeItemText(item, COLUMN_LOCATION, result_item.Location);
                        ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, result_item.Barcode);

                        string strItemDbName = Global.GetDbName(result_item.ItemRecPath);

                        Debug.Assert(String.IsNullOrEmpty(strItemDbName) == false, "");

                        string strBiblioDbName = (string)dbname_table[strItemDbName];

                        if (String.IsNullOrEmpty(strBiblioDbName) == true)
                        {
                            strBiblioDbName = this.MainForm.GetBiblioDbNameFromItemDbName(strItemDbName);
                            dbname_table[strItemDbName] = strBiblioDbName;
                        }

                        if (string.IsNullOrEmpty(result_item.ParentID) == false)
                            ListViewUtil.ChangeItemText(item, COLUMN_BIBLIORECPATH, strBiblioDbName + "/" + result_item.ParentID);


                        /*
                        if (CheckNumber(result_item.Zhongcihao) == true)
                            item.ImageIndex = TYPE_NORMAL;
                        else
                            item.ImageIndex = TYPE_ERROR;
                         * */

                        /*
                        if (result_item.Cols != null)
                        {
                            if (result_item.Cols.Length > 0)
                                item.SubItems.Add(result_item.Cols[0]);
                            if (result_item.Cols.Length > 1)
                                item.SubItems.Add(result_item.Cols[1]);
                        }*/


                        this.listView_number.Items.Add(item);
                        if (stop != null)
                            stop.SetProgressValue(lStart + i + 1);
                    }
                    this.listView_number.EndUpdate();

                    lStart += searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
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

            END1:
            // 用内存中最新的索取号来刷新
            RefreshByNewlyCallNumberItems();

            if (bSort == true)
            {
                // 排序
                this.listView_number.ListViewItemSorter = new CallNumberListViewItemComparer();
                this.listView_number.ListViewItemSorter = null; // 2011/10/19

                SetGroupBackcolor(this.listView_number,
                    COLUMN_BIBLIORECPATH);

                this.MaxNumber = GetZhongcihaoPart(GetTopNumber(this.listView_number));

                /*
                // 把重复种次号的事项用特殊颜色标出来
                ColorDup();

                this.MaxNumber = GetTopNumber(this.listView_number);    // this.listView_number.Items[0].SubItems[1].Text;
                 * */

            }

            EnsureCurrentItemsVisible();

            if (bFast == false)
            {
                int nRet = GetAllBiblioSummary(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            return 0;
        ERROR1:
            return -1;
        }

        /*
        // 用内存中最新的索取号刷新list
        void RefreshByNewlyCallNumberItems()
        {
            if (this.MyselfCallNumberItems == null)
                return;
            if (this.MyselfCallNumberItems.Count == 0)
                return;

            for (int i = 0; i < this.MyselfCallNumberItems.Count; i++)
            {
                CallNumberItem item = this.MyselfCallNumberItems[i];

                ListViewItem list_item = ListViewUtil.FindItem(this.listView_number, item.RecPath, COLUMN_ITEMRECPATH);
                if (list_item == null)
                    continue;

                ListViewUtil.ChangeItemText(list_item, COLUMN_CALLNUMBER, item.CallNumber);
            }
        }*/

        void EnsureCurrentItemsVisible()
        {
            if (this.m_currentItems.Count == 0)
                return;

            foreach (ListViewItem item in this.m_currentItems)
            {
                item.EnsureVisible();
            }
        }

        List<ListViewItem> m_currentItems = new List<ListViewItem>();

        // 用内存中最新的索取号刷新list
        void RefreshByNewlyCallNumberItems()
        {
            this.m_currentItems.Clear();

            if (this.MyselfCallNumberItems == null)
                return;
            if (this.MyselfCallNumberItems.Count == 0)
                return;


            string strError = "";

#if NO
            string strArrangeGroupName = "";
            string strZhongcihaoDbname = "";
            string strClassType = "";
            string strQufenhaoType = "";
            // 获得关于一个特定馆藏地点的索取号配置信息
            int nRet = this.MainForm.GetCallNumberInfo(this.LocationString,
                out strArrangeGroupName,
                out strZhongcihaoDbname,
                out strClassType,
                out strQufenhaoType,
                out strError);
#endif
            ArrangementInfo info = null;
            int nRet = this.MainForm.GetArrangementInfo(this.LocationString,
                out info,
                out strError);
            if (nRet == 0)
                return;
            if (nRet == -1)
                return;

            // 创建一个hash表，便于查找实体记录的路径
            Hashtable item_recpaths = new Hashtable();
            for (int i = 0; i < this.MyselfCallNumberItems.Count; i++)
            {
                CallNumberItem item = this.MyselfCallNumberItems[i];
                item_recpaths[item.RecPath] = 1;
            }

            // 删除当前书目记录下的全部实体记录
            for (int i = 0; i < this.listView_number.Items.Count; i++)
            {
                ListViewItem list_item = this.listView_number.Items[i];

                string strBiblioRecPath = ListViewUtil.GetItemText(list_item, COLUMN_BIBLIORECPATH);
                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                {
                    if (strBiblioRecPath == this.MyselfParentRecPath)
                    {
                        this.listView_number.Items.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    // 2012/3/22
                    // 当种记录路径为空的时候的替代办法
                    string stItemRecPath = ListViewUtil.GetItemText(list_item, COLUMN_ITEMRECPATH);
                    Debug.Assert(string.IsNullOrEmpty(stItemRecPath) == false, "");
                    if (item_recpaths[stItemRecPath] != null)
                    {
                        this.listView_number.Items.RemoveAt(i);
                        i--;
                    }
                }
            }

            // location --> arrangement name
            Hashtable info_table = new Hashtable();

            // 加入内存中的实体记录
            for (int i = 0; i < this.MyselfCallNumberItems.Count; i++)
            {
                CallNumberItem item = this.MyselfCallNumberItems[i];

                // 只有馆藏地点符合的才能进入
                string strLocation = item.Location;

#if NO
                string strCurrentArrangeGroupName = "";

                // TODO: 可以利用hashtable加速
                nRet = this.MainForm.GetCallNumberInfo(strLocation,
                    out strCurrentArrangeGroupName,
                    out strZhongcihaoDbname,
                    out strClassType,
                    out strQufenhaoType,
                    out strError);
                if (nRet == 0)
                    continue;

                if (strCurrentArrangeGroupName != strArrangeGroupName)
                    continue;
#endif
                // 利用hashtable加速
                // 2014/2/13
                string strCurrentArrangeGroupName = (string)info_table[strLocation];

                if (strCurrentArrangeGroupName == null)
                {
                    ArrangementInfo current_info = null;
                    nRet = this.MainForm.GetArrangementInfo(strLocation,
                        out current_info,
                        out strError);
                    if (nRet == 0)
                        continue;
                    strCurrentArrangeGroupName = current_info.ArrangeGroupName;
                    info_table[strLocation] = strCurrentArrangeGroupName;
                }

                if (strCurrentArrangeGroupName != info.ArrangeGroupName)
                    continue;

                string strCallNumber = StringUtil.BuildLocationClassEntry(item.CallNumber);

                // 2010/3/9
                // 只有分类号部分一致的才让进入
                string strClass = GetClassPart(strCallNumber);
                if (strClass != this.ClassNumber)
                    continue;

                ListViewItem list_item = new ListViewItem();
                list_item.ImageIndex = TYPE_CURRENT;
                list_item.Text = item.RecPath;

                ListViewUtil.ChangeItemText(list_item, COLUMN_CALLNUMBER, strCallNumber);

                ListViewUtil.ChangeItemText(list_item, COLUMN_LOCATION, item.Location);
                ListViewUtil.ChangeItemText(list_item, COLUMN_BARCODE, item.Barcode);
                ListViewUtil.ChangeItemText(list_item, COLUMN_BIBLIORECPATH, this.MyselfParentRecPath);

                this.listView_number.Items.Add(list_item);

                // 加入到属于当前种的ListViewItem数组中，后面会用来EnsureVisible
                this.m_currentItems.Add(list_item);
            }
        }

        // 
        /// <summary>
        /// 从索取号中分离出分类号部分
        /// </summary>
        /// <param name="strCallNumber">索取号</param>
        /// <returns>分类号部分</returns>
        public static string GetClassPart(string strCallNumber)
        {
            string[] lines = strCallNumber.Split(new char[] { '/' });
            if (lines.Length < 1)
                return "";

            string strClass = lines[0].Trim();
            return strClass;
        }

        // 从索取号中分离出种次号(或者著者号)部分
        // 会排除尾部的附加号
        /// <summary>
        /// 从索取号中分离出同类书区分号(种次号或著者号)部分。
        /// 返回前会排除尾部的附加号
        /// </summary>
        /// <param name="strCallNumber">索取号字符串</param>
        /// <returns>同类书区分号部分</returns>
        public static string GetZhongcihaoPart(string strCallNumber)
        {
            string [] lines = strCallNumber.Split(new char [] {'/'});
            if (lines.Length < 2)
                return "";

            string strZhongcihao = lines[1].Trim();
            /*
            int nRet = strZhongcihao.IndexOfAny(new char[] { '.', ',', '=', '-' });
            if (nRet != -1)
                strZhongcihao = strZhongcihao.Substring(0, nRet);

            return strZhongcihao;
             * */
            return GetLeftPureNumberPart(strZhongcihao);
        }

        // 2009/11/24
        /// <summary>
        /// 获得同类书区分号中除了附加号以外的部分
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <returns>返回除了附加号以外的部分</returns>
        public static string GetLeftPureNumberPart(string strText)
        {
            if (String.IsNullOrEmpty(strText) == true)
                return "";

            strText = strText.TrimStart();

            StringBuilder s = new StringBuilder();
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch < '0' || ch > '9')
                    break;
                s.Append(ch);
            }

            return s.ToString();
        }

        /// <summary>
        /// 比较同类书区分号的大小。
        /// 能处理附加号部分
        /// </summary>
        /// <param name="s1">参与比较的同类书区分号之一</param>
        /// <param name="s2">参与比较的同类书区分号之二</param>
        /// <returns>如果等于0，表示两个号码相等；如果大于0，表示之一大于之二；如果小于0，表示之一小于之二</returns>
        public static int CompareZhongcihao(string s1, string s2)
        {
            CanonicalZhongcihaoString(ref s1, ref s2);
            return String.Compare(s1, s2);
        }

        // 2008/9/19
        // 正规化即将比较的字符串
        // 按照'.'等切割符号，从左到右逐段规范化为彼此等长
        static void CanonicalZhongcihaoString(ref string s1, ref string s2)
        {
            string[] a1 = s1.Split(new char[] { '.', ',', '=', '-' });
            string[] a2 = s2.Split(new char[] { '.', ',', '=', '-' });

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

        /*
        // 从已经排序的事项中，取出位置最高事项的种次号。
        // 本函数会自动排除MyselfItemRecPath这条记录
        string GetTopNumber(ListView list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                string strRecPath = item.Text;
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (strRecPath != this.MyselfItemRecPath
                    && strBiblioRecPath != this.MyselfParentRecPath)
                    return ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER);
            }

            // TODO: 如果除了自己以外，并没有其他包含有效种次号的事项了，那也只好用自己的种次号-1来充当？

            return "";  // 没有找到
        }*/

        // 从已经排序的事项中，取出位置最高事项的种次号。
        // 本函数会自动排除MyselfItemRecPath这条记录
        string GetTopNumber(ListView list)
        {
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                string strRecPath = item.Text;
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (strRecPath != this.MyselfItemRecPath
                    && strBiblioRecPath != this.MyselfParentRecPath)
                    return ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER);
            }

            // TODO: 如果除了自己以外，并没有其他包含有效种次号的事项了，那也只好用自己的种次号-1来充当？

            return "";  // 没有找到
        }

        // 获得若干号
        int GetMultiNumber(
            string strStyle,
            out string strOtherMaxNumber,
            out string strMyselfNumber,
            out string strSiblingNumber,
            out string strError)
        {
            strError = "";
            strOtherMaxNumber = "";
            strMyselfNumber = "";
            strSiblingNumber = "";

            int nRet = FillList(true,
                strStyle,
                out strError);
            if (nRet == -1)
                return -1;

            for (int i = 0; i < this.listView_number.Items.Count; i++)
            {
                ListViewItem item = this.listView_number.Items[i];
                string strRecPath = item.Text;
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (String.IsNullOrEmpty(strOtherMaxNumber) == true)
                {
                    if (strRecPath != this.MyselfItemRecPath
                        && strBiblioRecPath != this.MyselfParentRecPath)
                        strOtherMaxNumber = GetZhongcihaoPart(ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER));
                }

                if (strRecPath == this.MyselfItemRecPath
                    && string.IsNullOrEmpty(strRecPath) == false)   // 2013/11/14
                    strMyselfNumber = GetZhongcihaoPart(ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER));

                if (String.IsNullOrEmpty(strSiblingNumber) == true)
                {
                    if (strBiblioRecPath == this.MyselfParentRecPath)
                        strSiblingNumber = GetZhongcihaoPart(ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER));
                }

                // 优化：三个值已经获得，没有必要继续循环了
                if (String.IsNullOrEmpty(strOtherMaxNumber) == false
                    && String.IsNullOrEmpty(strMyselfNumber) == false
                    && String.IsNullOrEmpty(strSiblingNumber) == false)
                    break;
            }

            return 0;
        }

        // 根据排序键值的变化分组设置颜色
        // 算法是将原本的背景颜色变深或者浅一点
        static void SetGroupBackcolor(
            ListView list,
            int nSortColumn)
        {
            string strPrevText = "";
            bool bDark = false;
            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];

                if (item.ImageIndex == TYPE_ERROR)
                    continue;

                string strThisText = "";
                try
                {
                    strThisText = item.SubItems[nSortColumn].Text;
                }
                catch
                {
                }

                if (strThisText != strPrevText)
                {
                    // 变化颜色
                    if (bDark == true)
                        bDark = false;
                    else
                        bDark = true;

                    strPrevText = strThisText;
                }

                if (bDark == true)
                {
                    item.BackColor = Global.Dark(GetItemBackColor(item.ImageIndex), 0.05F);
                }
                else
                {
                    item.BackColor = Global.Light(GetItemBackColor(item.ImageIndex), 0.05F);
                }
            }
        }

        static Color GetItemBackColor(int nType)
        {
            if (nType == TYPE_ERROR)
            {
                return Color.Red;
            }
            else if (nType == TYPE_NORMAL || nType == TYPE_CURRENT)
            {
                return SystemColors.Window;
            }
            else
            {
                throw new Exception("未知的image type");
            }
        }

        // 检索实体和尾号
        private void button_searchDouble_Click(object sender, EventArgs e)
        {
            string strError = "";

            EventFinish.Reset();
            try
            {
                this.textBox_tailNumber.Text = "";   // 预防filllist 提前退出, 忘记处理

                int nRet = FillList(true,
                    "",
                    out strError);
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

        private void listView_number_DoubleClick(object sender, EventArgs e)
        {
            if (this.listView_number.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要操作的事项");
                return;
            }

            string strItemRecPath = ListViewUtil.GetItemText(this.listView_number.SelectedItems[0], 0);

            if (String.IsNullOrEmpty(strItemRecPath) == true)
            {
                MessageBox.Show(this, "实体记录路径为空");
                return;
            }

            string strOpenStyle = "new";
            /*
                if (this.LoadToExistDetailWindow == true)
                    strOpenStyle = "exist";
             * */

            // 装入种册窗/实体窗，用册条码号/记录路径
            // parameters:
            //      strTargetFormType   目标窗口类型 "EntityForm" "ItemInfoForm"
            //      strIdType   标识类型 "barcode" "recpath"
            //      strOpenType 打开窗口的方式 "new" "exist"
            LoadRecord("EntityForm",
                "recpath",
                strOpenStyle);
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

            if (this.listView_number.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定要装入" + strTargetFormName + "的行");
                return;
            }

            string strBarcodeOrRecPath = "";

            if (strIdType == "barcode")
            {
                // barcode
                strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_number.SelectedItems[0], 1);
            }
            else
            {
                Debug.Assert(strIdType == "recpath", "");
                // recpath
                strBarcodeOrRecPath = ListViewUtil.GetItemText(this.listView_number.SelectedItems[0], 0);
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

                    form.MdiParent = this.MainForm;

                    form.MainForm = this.MainForm;
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

                    form.MdiParent = this.MainForm;

                    form.MainForm = this.MainForm;
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

        int m_nInDropDown = 0;

        private void comboBox_location_DropDown(object sender, EventArgs e)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {

                ComboBox combobox = (ComboBox)sender;

                if (combobox.Items.Count == 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    // e1.DbName = this.BiblioDbName;

                    if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else
                    {
                        Debug.Assert(false, "不支持的sender");
                        return;
                    }

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
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

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获得尾号 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.GetOneClassTailNumber(
                    stop,
                    GetArrangeGroupName(this.LocationString),
                    this.ClassNumber,
                    out strTailNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            // return 0;
        ERROR1:
            return -1;
        }

        // 获取尾号
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

        /// <summary>
        /// 保存当前出鞥口中的尾号
        /// </summary>
        /// <param name="strTailNumber">要保存的尾号</param>
        /// <param name="strOutputNumber">返回实际保存的尾号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int SaveTailNumber(
            string strTailNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存尾号 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SetOneClassTailNumber(
                    stop,
                    "save",
                    GetArrangeGroupName(this.LocationString),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strTailNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            // return 0;
        ERROR1:
            return -1;
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

        // 推动尾号。如果已经存在的尾号比strTestNumber还要大，则不推动
        /// <summary>
        /// 推动当前窗口中的尾号。
        /// 如果已经存在的尾号比 strTestNumber 还要大，则不推动
        /// </summary>
        /// <param name="strTestNumber">希望推动到的尾号</param>
        /// <param name="strOutputNumber">返回实际被推动后的尾号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int PushTailNumber(string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在推动尾号 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SetOneClassTailNumber(
                    stop,
                    "conditionalpush",
                    GetArrangeGroupName(this.LocationString),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strTestNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            // return 0;
        ERROR1:
            return -1;
        }

        // 增量尾号
        /// <summary>
        /// 增量当前窗口中的尾号
        /// </summary>
        /// <param name="strDefaultNumber">缺省尾号。如果当前尾号库中尚未建立当前类的尾号，则按照本参数来建立</param>
        /// <param name="strOutputNumber">返回增量后的尾号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 1: 成功</returns>
        public int IncreaseTailNumber(string strDefaultNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在增量尾号 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SetOneClassTailNumber(
                    stop,
                    "increase",
                    GetArrangeGroupName(this.LocationString),
                    // "!" + this.BiblioDbName,
                    this.ClassNumber,
                    strDefaultNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

            // return 0;
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

            Clipboard.SetDataObject(strResult);
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
        /// 得到 根据当前窗口中书目信息统计出来的最大号的加1以后的号
        /// </summary>
        /// <param name="strResult">返回结果</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1: 出错</para>
        /// <para>0: 没有找到</para>
        /// <para>1: 成功</para>
        /// </returns>
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

        int SetDisplayState(out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            string strArrangeGroupName = "";
            string strZhongcihaoDbname = "";
            string strClassType = "";
            string strQufenhaoType = "";

            // 获得关于一个特定馆藏地点的索取号配置信息
            // return:
            //      -1  error
            //      0   notd found
            //      1   found
            nRet = this.MainForm.GetCallNumberInfo(this.LocationString,
                out strArrangeGroupName,
                out strZhongcihaoDbname,
                out strClassType,
                out strQufenhaoType,
                out strError);
#endif
            ArrangementInfo info = null;
            nRet = this.MainForm.GetArrangementInfo(this.LocationString,
                out info,
                out strError);
            if (nRet == 0)
            {
                this.MaxNumberVisible = false;

                this.groupBox_tailNumber.Visible = false;
                this.button_searchDouble.Visible = false;
                this.button_pushTailNumber.Visible = false;

                return 0;
            }

            if (nRet == -1)
                return -1;


            if (info.QufenhaoType.ToLower() == "zhongcihao"
                || info.QufenhaoType == "种次号")
            {
                this.MaxNumberVisible = true;
            }
            else
            {
                this.MaxNumberVisible = false;

                info.ZhongcihaoDbname = "";   // 如果不是种次号类型，也就不理会种次号库名了
            }

            if (String.IsNullOrEmpty(info.ZhongcihaoDbname) == true)
            {
                this.groupBox_tailNumber.Visible = false;
                this.button_searchDouble.Visible = false;
                this.button_pushTailNumber.Visible = false;
            }
            else
            {
                this.groupBox_tailNumber.Visible = true;
                this.button_searchDouble.Visible = true;
                this.button_pushTailNumber.Visible = true;
            }

            return 0;
        }

        bool MaxNumberVisible
        {
            get
            {
                return this.textBox_maxNumber.Visible;
            }
            set
            {
                this.textBox_maxNumber.Visible = value;
                this.button_copyMaxNumber.Visible = value;
                this.label_maxNumber.Visible = value;
            }
        }

        /*
        // 获得MyselfItemRecPath这条记录的CallNumber的区分号部分。如果不存在，则获得同一书目记录下属的第一个CallNumber区分号部分
        string GetMyselfOrSiblingQufenNumber(ListView list)
        {
            string strSiblingNumber = "";

            for (int i = 0; i < list.Items.Count; i++)
            {
                ListViewItem item = list.Items[i];
                string strRecPath = item.Text;
                string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                if (strRecPath == this.MyselfItemRecPath)
                {
                    string strNumber = ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER);

                    strNumber = GetZhongcihaoPart(strNumber);

                    if (String.IsNullOrEmpty(strNumber) == false)
                        return strNumber;
                    continue;
                }


                if (strBiblioRecPath == this.MyselfParentRecPath
                    && String.IsNullOrEmpty(strSiblingNumber) == true)
                {
                    strSiblingNumber = ListViewUtil.GetItemText(item, COLUMN_CALLNUMBER);

                    strSiblingNumber = GetZhongcihaoPart(strSiblingNumber);
                }
            }

            if (String.IsNullOrEmpty(strSiblingNumber) == false)
                return strSiblingNumber;

            return null;  // 没有找到
        }
         * */

        // (外部调用接口)
        // 按照一定的策略，获得种次号
        // TODO: 可以返回一定的提示信息，表明是否从自身获得，还是从其它记录的最大号推算出来
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 按照一定的策略，获得种次号
        /// </summary>
        /// <param name="style">种次号取号的风格</param>
        /// <param name="strClass">类号</param>
        /// <param name="strLocationString">馆藏地点</param>
        /// <param name="strNumber">返回种次号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int GetZhongcihao(
            ZhongcihaoStyle style,
            string strClass,
            string strLocationString,
            out string strNumber,
            out string strError)
        {
            strNumber = "";
            strError = "";
            int nRet = 0;

            this.ClassNumber = strClass;
            this.LocationString = strLocationString;

            // 仅利用书目统计最大号
            if (style == ZhongcihaoStyle.Biblio)
            {
                string strOtherMaxNumber = "";
                string strMyselfNumber = "";
                string strSiblingNumber = "";

                // 获得若干号
                nRet = GetMultiNumber(
                    "fast",
                    out strOtherMaxNumber,
                    out strMyselfNumber,
                    out strSiblingNumber,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (String.IsNullOrEmpty(strMyselfNumber) == false)
                {
                    strNumber = strMyselfNumber;
                    return 1;
                }

                if (String.IsNullOrEmpty(strSiblingNumber) == false)
                {
                    strNumber = strSiblingNumber;
                    return 1;
                }

                if (String.IsNullOrEmpty(strOtherMaxNumber) == false)
                {
                    nRet = StringUtil.IncreaseLeadNumber(strOtherMaxNumber,
                        1,
                        out strNumber,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "为数字 '" + strOtherMaxNumber + "' 增量时发生错误: " + strError;
                        goto ERROR1;
                    }

                    return 1;
                }

                // 2009/2/25
                Debug.Assert(nRet == 0, "");

                string strDefaultValue = "";    // "1"

            REDO_INPUT:
                // 此类从来没有过记录，当前是第一条
                strNumber = InputDlg.GetInput(
                    this,
                    null,
                    "请输入类 '" + strClass + "' 的当前种次号最大号:",
                    strDefaultValue,
            this.MainForm.DefaultFont);
                if (strNumber == null)
                    return 0;	// 放弃整个操作
                if (String.IsNullOrEmpty(strNumber) == true)
                    goto REDO_INPUT;

                return 1;
            }

            /*
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
                    "1");
                if (strNumber == null)
                    return 0;	// 放弃整个操作

                return 1;
            }
            */


            // 每次都利用书目统计最大号来检验、校正尾号
            if (style == ZhongcihaoStyle.BiblioAndSeed
                || style == ZhongcihaoStyle.SeedAndBiblio)
            {
                // TODO: 如果当前记录在内存中存在，就应优先用它。这样可以避免无谓的增量
                if (style == ZhongcihaoStyle.BiblioAndSeed)
                {
                    /*
                    // TODO: 如何避免重复filllist
                    nRet = FillList(true, out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    strNumber = GetMyselfOrSiblingQufenNumber(this.listView_number);
                    if (String.IsNullOrEmpty(strNumber) == false)
                    {
                        return 1;
                    }
                     * */
                    string strOtherMaxNumber = "";
                    string strMyselfNumber = "";
                    string strSiblingNumber = "";

                    // 获得若干号
                    nRet = GetMultiNumber(
                        "fast",
                        out strOtherMaxNumber,
                        out strMyselfNumber,
                        out strSiblingNumber,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (String.IsNullOrEmpty(strMyselfNumber) == false)
                    {
                        strNumber = strMyselfNumber;
                        return 1;
                    }

                    if (String.IsNullOrEmpty(strSiblingNumber) == false)
                    {
                        strNumber = strSiblingNumber;
                        return 1;
                    }
                }

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
                        strTestNumber = ""; // "1"

                REDO_INPUT:
                    // 此类从来没有过记录，当前是第一条
                    strNumber = InputDlg.GetInput(
                        this,
                        null,
                        "请输入类 '" + strClass + "' 的当前种次号最大号:",
                        strTestNumber,
            this.MainForm.DefaultFont);
                    if (strNumber == null)
                        return 0;	// 放弃整个操作
                    if (String.IsNullOrEmpty(strNumber) == true)
                        goto REDO_INPUT;

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
                string strTailNumber = "";

                try
                {
                    strTailNumber = this.TailNumber;
                }
                catch (Exception ex)
                {
                    strError = ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

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
                        strTestNumber = ""; // "1"

                REDO_INPUT:
                    // 此类从来没有过记录，当前是第一条
                    strNumber = InputDlg.GetInput(
                        this,
                        null,
                        "请输入类 '" + strClass + "' 的当前种次号最大号:",
                        strTestNumber,
            this.MainForm.DefaultFont);
                    if (strNumber == null)
                        return 0;	// 放弃整个操作
                    if (String.IsNullOrEmpty(strNumber) == true)
                        goto REDO_INPUT;

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

        int GetAllBiblioSummary(out string strError)
        {
            strError = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.SetMessage("正在获取书目摘要 ...");
            stop.BeginLoop();

            try
            {
                string strPrevBiblioRecPath = "";
                string strPrevSummary = "";
                for (int i = 0; i < this.listView_number.Items.Count; i++)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null)
                    {
                        if (stop.State != 0)
                            return 0;
                    }

                    ListViewItem item = this.listView_number.Items[i];
                    string strSummary = "";
                    string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_BIBLIORECPATH);

                    if (strPrevBiblioRecPath == strBiblioRecPath)
                    {
                        strSummary = strPrevSummary;
                        goto SETTEXT;
                    }

                    string strOutputBiblioRecPath = "";

                    long lRet = Channel.GetBiblioSummary(
                        stop,
                        "@bibliorecpath:" + strBiblioRecPath,
                        "", // strItemRecPath,
                        null,
                        out strOutputBiblioRecPath,
                        out strSummary,
                        out strError);
                    if (lRet == -1)
                    {
                        strSummary = strError;
                    }

                SETTEXT:
                    ListViewUtil.ChangeItemText(item, COLUMN_SUMMARY, strSummary);

                    strPrevBiblioRecPath = strBiblioRecPath;
                    strPrevSummary = strSummary;
                }

            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 0;
        }

        private void checkBox_topmost_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_topmost.Checked == true)
            {
                Debug.Assert(this.MainForm != null || this.MdiParent != null, "");
                if (this.MdiParent != null)
                    this.MainForm = (MainForm)this.MdiParent;
                this.MdiParent = null;
                Debug.Assert(this.MainForm != null, "");
                this.Owner = this.MainForm;
                // this.TopMost = true;
            }
            else
            {
                Debug.Assert(this.MainForm != null, "");
                this.MdiParent = this.MainForm;
                // this.TopMost = false;
            }
        }

        /// <summary>
        /// 窗口是否为浮动状态
        /// </summary>
        public override bool Floating
        {
            get
            {
                return this.checkBox_topmost.Checked;
            }
            set
            {
                this.checkBox_topmost.Checked = value;
            }
        }

        private void CallNumberForm_Activated(object sender, EventArgs e)
        {
            // 2009/8/13
            // this.MainForm.stopManager.Active(this.stop);
        }

        private void comboBox_location_TextChanged(object sender, EventArgs e)
        {
            // 重新设置显示信息
            string strError = "";
            int nRet = SetDisplayState(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        private void listView_number_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_number, e);
        }


#if NOOOOOOOOOOOOOOOOOOOOOOOOO

        // 获得关于一个特定馆藏地点的索取号配置信息
        int GetCallNumberInfo(string strLocation,
            out string strArrangeGroupName,
            out string strZhongcihaoDbname,
            out string strClassType,
            out string strQufenhaoType,
            out string strError)
        {
            strError = "";
            strArrangeGroupName = "";
            strZhongcihaoDbname = "";
            strClassType = "";
            strQufenhaoType = "";

            if (this.cfg_dom == null)
            {
                // 初始化索取号配置信息
                int nRet = InitialCallNumberCfgInfo(out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                    return 0;
                Debug.Assert(this.cfg_dom != null, "");
            }

            if (this.cfg_dom.DocumentElement == null)
                return 0;

            XmlNode node = this.cfg_dom.DocumentElement.SelectSingleNode("group/location[@name='"+strLocation+"']");
            if (node == null)
                return 0;

            XmlNode nodeGroup = node.ParentNode;
            strArrangeGroupName = DomUtil.GetAttr(nodeGroup, "name");
            strZhongcihaoDbname = DomUtil.GetAttr(nodeGroup, "zhongcihaodb");
            strClassType = DomUtil.GetAttr(nodeGroup, "classType");
            strQufenhaoType = DomUtil.GetAttr(nodeGroup, "qufenhaoType");

            return 1;
        }

        // 初始化索取号配置信息
        // return:
        //      -1  error
        //      0   not initial
        //      1   initialized
        int InitialCallNumberCfgInfo(out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.MainForm.CallNumberInfo) == true)
                return 0;

            this.cfg_dom = new XmlDocument();
            this.cfg_dom.LoadXml("<callNumber/>");

            try
            {
                this.cfg_dom.DocumentElement.InnerXml = this.MainForm.CallNumberInfo;
            }
            catch (Exception ex)
            {
                strError = "Set InnerXml error: " + ex.Message;
                return -1;
            }

            return 1;
        }

#endif
    }

    // 
    /// <summary>
    /// 当前内存中的索取号事项
    /// </summary>
    public class CallNumberItem
    {
        /// <summary>
        /// 册记录的路径
        /// </summary>
        public string RecPath = ""; // 册记录的路径
        /// <summary>
        /// 索取号
        /// </summary>
        public string CallNumber = "";  // 索取号

        /// <summary>
        /// 馆藏地点
        /// </summary>
        public string Location = "";
        /// <summary>
        /// 册条码号
        /// </summary>
        public string Barcode = "";
    }

    // 排序
    // Implements the manual sorting of items by columns.
    class CallNumberListViewItemComparer : IComparer
    {
        public CallNumberListViewItemComparer()
        {
        }

        public int Compare(object x, object y)
        {
            string s1 = ((ListViewItem)x).SubItems[CallNumberForm.COLUMN_CALLNUMBER].Text;
            string s2 = ((ListViewItem)y).SubItems[CallNumberForm.COLUMN_CALLNUMBER].Text;

            // 取得区分号行
            s1 = CallNumberForm.GetZhongcihaoPart(s1);
            s2 = CallNumberForm.GetZhongcihaoPart(s2);

            /*
            CanonicalString(ref s1, ref s2);
            return -1 * String.Compare(s1, s2);
             * */
            // 2009/11/4 changed
            return -1 * CallNumberForm.CompareZhongcihao(s1, s2);
        }

    }
}