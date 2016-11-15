using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;

using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
// using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    // public partial class CommentControl : UserControl
    /// <summary>
    /// 评注记录列表控件
    /// </summary>
    public partial class CommentControl : CommentControlBase
    {
#if NO
        // 引用外部即可
        WebExternalHost m_webExternalHost = null;
        public WebExternalHost WebExternalHost
        {
            get
            {
                return this.m_webExternalHost;
            }
            set
            {
                this.m_webExternalHost = value;
            }
        }
#endif
#if NO
        public event LoadRecordHandler LoadRecord = null;
        // 参与排序的列号数组
        SortColumns SortColumns = new SortColumns();

        public bool m_bRemoveDeletedItem = false;   // 在删除事项时, 是否从视觉上抹除这些事项(实际上内存里面还保留有即将提交的事项)?

#endif

        CommentViewerForm m_commentViewer = null;

        /// <summary>
        /// 加入主题词 (UNIMARC 610字段)
        /// </summary>
        public event AddSubjectEventHandler AddSubject = null;

#if NO
        /// <summary>
        /// 通讯通道
        /// </summary>
        public LibraryChannel Channel = null;

        /// <summary>
        /// 停止控制
        /// </summary>
        public DigitalPlatform.Stop Stop = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 获得宏的值
        /// </summary>
        public event GetMacroValueHandler GetMacroValue = null;

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        /// <summary>
        /// 界面许可 / 禁止状态发生改变
        /// </summary>
        public event EnableControlsHandler EnableControlsEvent = null;

        string m_strBiblioRecPath = "";

        public CommentItemCollection Items = null;

#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommentControl()
        {
            InitializeComponent();

            this.m_listView = this.listView;
            this.ItemType = "comment";
            this.ItemTypeName = "评注";
        }
#if NO
        public int CommentCount
        {
            get
            {
                if (this.Items != null)
                    return this.Items.Count;

                return 0;
            }
        }

        // 将listview中的评注事项修改为new状态
        public void ChangeAllItemToNewState()
        {
            foreach (CommentItem commentitem in this.Items)
            {
                // CommentItem commentitem = this.CommentItems[i];

                if (commentitem.ItemDisplayState == ItemDisplayState.Normal
                    || commentitem.ItemDisplayState == ItemDisplayState.Changed
                    || commentitem.ItemDisplayState == ItemDisplayState.Deleted)   // 注意未提交的deleted也变为new了
                {
                    commentitem.ItemDisplayState = ItemDisplayState.New;
                    commentitem.RefreshListView();
                    commentitem.Changed = true;    // 这一句决定了使能后如果立即关闭窗口，是否会警告(实体修改)内容丢失
                }
            }
        }

        public string BiblioRecPath
        {
            get
            {
                return this.m_strBiblioRecPath;
            }
            set
            {
                this.m_strBiblioRecPath = value;

                if (this.Items != null)
                {
                    string strID = Global.GetRecordID(value);
                    this.Items.SetParentID(strID);
                }

            }
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                if (this.Items == null)
                    return false;

                return this.Items.Changed;
            }
            set
            {
                if (this.Items != null)
                    this.Items.Changed = value;
            }
        }

        // 清除listview中的全部事项
        public void Clear()
        {
            this.ListView.Items.Clear();

            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.ListView.Columns);

            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();

            this.pieChartControl1.Values = new decimal[0];
        }

        // 清除评注有关信息
        public void ClearComments()
        {
            this.Clear();
            this.Items = new CommentItemCollection();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
        }

        public int CountOfVisibleCommentItems()
        {
            return this.ListView.Items.Count;
        }

        public int IndexOfVisibleCommentItems(CommentItem commentitem)
        {
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                CommentItem cur = (CommentItem)this.ListView.Items[i].Tag;

                if (cur == commentitem)
                    return i;
            }

            return -1;
        }

        public CommentItem GetAtVisibleCommentItems(int nIndex)
        {
            return (CommentItem)this.ListView.Items[nIndex].Tag;
        }
#endif

        // 
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 获得一个书目记录下属的全部评注记录路径
        /// </summary>
        /// <param name="stop">Stop对象</param>
        /// <param name="channel">通讯通道</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="recpaths">返回记录路径字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1 出错</para>
        /// <para>0 没有装载</para>
        /// <para>1 已经装载</para>
        /// </returns>
        public static int GetCommentRecPaths(
            Stop stop,
            LibraryChannel channel,
            string strBiblioRecPath,
            out List<string> recpaths,
            out string strError)
        {
            strError = "";
            recpaths = new List<string>();

            long lPerCount = 100; // 每批获得多少个
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;
            for (; ; )
            {
                if (stop != null)
                {
                    if (stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }
                }
                EntityInfo[] entities = null;

                /*
                if (lCount > 0)
                    stop.SetMessage("正在装入册信息 " + lStart.ToString() + "-" + (lStart + lCount - 1).ToString() + " ...");
                 * */

                long lRet = channel.GetComments(
                    stop,
                    strBiblioRecPath,
                    lStart,
                    lCount,
                    "onlygetpath",
                    "zh",
                    out entities,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lResultCount = lRet;

                if (lRet == 0)
                    return 0;

                Debug.Assert(entities != null, "");


                for (int i = 0; i < entities.Length; i++)
                {
                    if (entities[i].ErrorCode != ErrorCodeValue.NoError)
                    {
                        strError = "路径为 '" + entities[i].OldRecPath + "' 的评注记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
                        return -1;
                    }

                    recpaths.Add(entities[i].OldRecPath);
                }

                lStart += entities.Length;
                if (lStart >= lResultCount)
                    break;

                if (lCount == -1)
                    lCount = lPerCount;

                if (lStart + lCount > lResultCount)
                    lCount = lResultCount - lStart;
            }

            return 1;
        ERROR1:
            return -1;
        }

#if NO
        // 装入评注记录
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        public int LoadCommentRecords(string strBiblioRecPath,
            out string strError)
        {
            this.BiblioRecPath = strBiblioRecPath;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在装入评注信息 ...");
            Stop.BeginLoop();

            this.Update();

            try
            {
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                this.ClearComments();

                // 2012/5/9 改写为循环方式
                for (; ; )
                {
                    EntityInfo[] comments = null;

                    long lRet = Channel.GetComments(
                        Stop,
                        strBiblioRecPath,
                        lStart,
                        lCount,
                        "",
                        "zh",
                        out comments,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    if (lRet == 0)
                        return 0;

                    lResultCount = lRet;

                    Debug.Assert(comments != null, "");

                    this.ListView.BeginUpdate();
                    try
                    {
                        for (int i = 0; i < comments.Length; i++)
                        {
                            if (comments[i].ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + comments[i].OldRecPath + "' 的评注记录装载中发生错误: " + comments[i].ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            // 剖析一个评注xml记录，取出有关信息放入listview中
                            CommentItem commentitem = new CommentItem();

                            int nRet = commentitem.SetData(comments[i].OldRecPath, // NewRecPath
                                     comments[i].OldRecord,
                                     comments[i].OldTimestamp,
                                     out strError);
                            if (nRet == -1)
                                return -1;

                            if (comments[i].ErrorCode == ErrorCodeValue.NoError)
                                commentitem.Error = null;
                            else
                                commentitem.Error = comments[i];

                            this.Items.Add(commentitem);

                            commentitem.AddToListView(this.ListView);
                        }
                    }
                    finally
                    {
                        this.ListView.EndUpdate();
                    }

                    lStart += comments.Length;
                    if (lStart >= lResultCount)
                        break;
                }
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }

            RefreshOrderSuggestionPie();

            return 1;
        ERROR1:
            return -1;
        }

#endif

        /// <summary>
        /// 装载 Item 记录
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="preload_entities">预先装载好的事项集合</param>
        /// <param name="strStyle">装载风格</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有装载; 1: 已经装载</returns>
        public override int LoadItemRecords(
            LibraryChannel channel,
            string strBiblioRecPath,
            EntityInfo[] preload_entities,
            // bool bDisplayOtherLibraryItem,
            string strStyle,
            out string strError)
        {
            int nRet = base.LoadItemRecords(
                channel,
                strBiblioRecPath,
                preload_entities,
                strStyle,
                out strError);
            if (nRet == -1)
                return nRet;

            RefreshOrderSuggestionPie();
            return nRet;
        }

        /// <summary>
        /// 填充馆代码组合框列表
        /// </summary>
        /// <param name="strLibraryCodeList">馆代码列表</param>
        public void SetLibraryCodeFilter(string strLibraryCodeList)
        {
            this.comboBox_libraryCodeFilter.Items.Clear();

            this.comboBox_libraryCodeFilter.Items.Add("<全部分馆>");

            if (Global.IsGlobalUser(strLibraryCodeList) == true)
            {
                return;
            }

            this.comboBox_libraryCodeFilter.Items.Add(strLibraryCodeList);
        }

        /// <summary>
        /// 刷新订购统计饼图
        /// </summary>
        public void RefreshOrderSuggestionPie()
        {
            int nYes = 0;
            int nNo = 0;
            int nNull = 0;
            int nOther = 0;

            this.pieChartControl1.Values = new decimal[0];

            string strFilter = this.comboBox_libraryCodeFilter.Text;
            if (strFilter == "<全部分馆>")
                strFilter = "";

            // parameters:
            //      strLibraryCodeList  馆代码列表，用于过滤。仅统计这个列表中的。如果为null表示全部统计
            //      nYes    建议订购的数量
            //      nNo     建议不订购的数量
            //      nNull   没有表态，但也是“订购征询”的数量
            //      nOther  "订购征询"以外的数量
            this.Items.GetOrderSuggestion(
                strFilter, // strLibraryCodeList,
                out nYes,
                out nNo,
                out nNull,
                out nOther);

            int nTotal = nYes + nNo + nNull;
            if (nTotal > 0)
            {
                List<decimal> values = new List<decimal>();
                values.Add(nYes);
                values.Add(nNo);
                values.Add(nNull);

                List<float> displacements = new List<float>();
                displacements.Add(0.0F);    // 0.2F
                displacements.Add(0.0F);
                displacements.Add(0.0F);

                List<string> texts = new List<string>();
                texts.Add(
                    nYes != 0 ?
                    "荐 " + nYes.ToString() + " = " + GetPercent(nYes, nTotal) + "" : "");
                texts.Add(
                    nNo != 0 ?
                    "不 " + nNo.ToString() + " = " + GetPercent(nNo, nTotal) + "" : "");
                texts.Add(
        nNull != 0 ?
        "无 " + nNull.ToString() + " = " + GetPercent(nNull, nTotal) + "" : "");

                List<Color> colors = new List<Color>();
                colors.Add(Color.FromArgb(200, 100, 200, 50));    // green
                colors.Add(Color.FromArgb(200, 230, 0, 0)); // red
                colors.Add(Color.FromArgb(200, 200, 200, 200)); // white


                this.pieChartControl1.Values = values.ToArray();
                this.pieChartControl1.Texts = texts.ToArray();
                this.pieChartControl1.Colors = colors.ToArray();
                this.pieChartControl1.SliceRelativeDisplacements = displacements.ToArray();
                this.pieChartControl1.SliceRelativeHeight = 0.05F;

                /*
                int nMargin = Math.Min(this.pieChartControl1.Width, this.pieChartControl1.Height) / 5;
                this.pieChartControl1.LeftMargin = nMargin;
                this.pieChartControl1.RightMargin = nMargin;
                this.pieChartControl1.TopMargin = nMargin;
                this.pieChartControl1.BottomMargin = nMargin;
                 * */
                SetPieChartMargin();


                this.pieChartControl1.ShadowStyle = System.Drawing.PieChart.ShadowStyle.UniformShadow;
                this.pieChartControl1.EdgeColorType = System.Drawing.PieChart.EdgeColorType.DarkerThanSurface;
            }

        }

        static string GetPercent(double v1, double v2)
        {
            double ratio = v1 / v2;
            // return String.Format("{0,3:N}", ratio * (double)100) + "%";
            return String.Format("{0:0%}", ratio);
        }

#if NO
        // 分批进行保存
        // return:
        //      -2  已经警告(部分成功，部分失败)
        //      -1  出错
        //      0   保存成功，没有错误和警告
        int SaveComments(EntityInfo[] comments,
            out string strError)
        {
            strError = "";

            bool bWarning = false;
            EntityInfo[] errorinfos = null;

            int nBatch = 100;
            for (int i = 0; i < (comments.Length / nBatch) + ((comments.Length % nBatch) != 0 ? 1 : 0); i++)
            {
                int nCurrentCount = Math.Min(nBatch, comments.Length - i * nBatch);
                EntityInfo[] current = EntityControl.GetPart(comments, i * nBatch, nCurrentCount);

                int nRet = SaveCommentRecords(this.BiblioRecPath,
                    current,
                    out errorinfos,
                    out strError);

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                if (RefreshOperResult(errorinfos) == true)
                    bWarning = true;

                if (nRet == -1)
                    return -1;
            }

            if (bWarning == true)
                return -2;
            return 0;
        }

        // 提交评注保存请求
        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        public int DoSaveComments()
        {
            if (this.Items == null)
                return 0;

            EnableControls(false);

            try
            {
                string strError = "";
                int nRet = 0;

                if (this.Items == null)
                {
                    return 0;
                }

                // 检查全部事项的Parent值是否适合保存
                // return:
                //      -1  有错误，不适合保存
                //      0   没有错误
                nRet = this.Items.CheckParentIDForSave(out strError);
                if (nRet == -1)
                {
                    strError = "保存评注信息失败，原因：" + strError;
                    goto ERROR1;
                }

                EntityInfo[] comments = null;

                // 构造需要提交的评注信息数组
                nRet = BuildSaveComments(
                    out comments,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (comments == null || comments.Length == 0)
                    return 0; // 没有必要保存

#if NO
                EntityInfo[] errorinfos = null;
                nRet = SaveCommentRecords(this.BiblioRecPath,
                    comments,
                    out errorinfos,
                    out strError);

                // 把出错的事项和需要更新状态的事项兑现到显示、内存
                RefreshOperResult(errorinfos);

                if (nRet == -1)
                {
                    goto ERROR1;
                }
#endif
                // return:
                //      -2  已经警告(部分成功，部分失败)
                //      -1  出错
                //      0   保存成功，没有错误和警告
                nRet = SaveComments(comments, out strError);
                if (nRet == -2)
                    return -1;  // SaveComments()已经MessageBox()显示过了
                if (nRet == -1)
                    goto ERROR1;

                this.Changed = false;
                this.MainForm.StatusBarMessage = "评注信息 提交 / 保存 成功";
                DoViewComment(false);
                return 1;
            ERROR1:
                MessageBox.Show(ForegroundWindow.Instance, strError);
                return -1;
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 根据评注记录路径加亮事项
        public CommentItem HilightLineByItemRecPath(string strItemRecPath,
                bool bClearOtherSelection)
        {
            CommentItem commentitem = null;

            if (bClearOtherSelection == true)
            {
                this.ListView.SelectedItems.Clear();
            }

            if (this.Items != null)
            {
                commentitem = this.Items.GetItemByRecPath(strItemRecPath) as CommentItem;
                if (commentitem != null)
                    commentitem.HilightListViewItem(true);
            }

            return commentitem;
        }

#endif

#if NO
        // 2011/6/30 
        // 根据评注记录路径 检索出 书目记录 和全部下属评注记录，装入窗口
        // parameters:
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int DoSearchCommentByRecPath(string strCommentRecPath)
        {
            int nRet = 0;
            string strError = "";
            // 先检查是否已在本窗口中?

            // 对当前窗口内进行册记录路径查重
            if (this.Items != null)
            {
                CommentItem dupitem = this.Items.GetItemByRecPath(strCommentRecPath) as CommentItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "评注记录 '" + strCommentRecPath + "' 正好为本种中未提交之一删除评注请求。";
                    else
                        strText = "评注记录 '" + strCommentRecPath + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";


            // 根据评注记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchBiblioRecPath(strCommentRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对评注记录路径 '" + strCommentRecPath + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到路径为 '" + strCommentRecPath + "' 的评注记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上评注事项
                CommentItem result_item = HilightLineByItemRecPath(strCommentRecPath, true);
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用评注记录路径检索绝对不应发生重复现象 -- 可是居然发生了");
            }

            return 0;
        }
#endif

#if NO
        // 根据评注记录路径，检索出其从属的书目记录路径。
        int SearchBiblioRecPath(string strCommentRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索评注记录 '" + strCommentRecPath + "' 所从属的书目记录路径 ...");
            Stop.BeginLoop();

            try
            {
                string strIndex = "@path:" + strCommentRecPath;
                string strOutputItemRecPath = "";

                long lRet = Channel.GetCommentInfo(
                    Stop,
                    strIndex,
                    // "", // strBiblioRecPath,
                    null,
                    out strItemText,
                    out strOutputItemRecPath,
                    out item_timestamp,
                    "recpath",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                return (int)lRet;   // not found
            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }
        }
#endif

#if NO
        // return:
        //      -1  出错。已经用MessageBox报错
        //      0   没有装载
        //      1   成功装载
        public int DoLoadRecord(string strBiblioRecPath)
        {
            if (this.LoadRecord == null)
                return 0;

            LoadRecordEventArgs e = new LoadRecordEventArgs();
            e.BiblioRecPath = strBiblioRecPath;
            this.LoadRecord(this, e);
            return e.Result;
        }
#endif

        // 
        /// <summary>
        /// 汇总已经推荐过本书目的评注信息，HTML格式
        /// </summary>
        /// <param name="strHtml">返回 HTML 字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetOrderSuggestionHtml(out string strHtml,
            out string strError)
        {
            strHtml = "";
            strError = "";

            if (this.Items == null)
                return 0;

            StringBuilder result = new StringBuilder(4096);

            foreach (CommentItem comment in this.Items)
            {
                // 跳过不是订购征询类型的记录
                if (comment.TypeString != "订购征询")
                    continue;
                // 跳过不是 推荐订购 的那些条目
                if (comment.OrderSuggestion != "yes")
                    continue;

                result.Append("<tr class='content'>");

                result.Append("<td class='title'>" + HttpUtility.HtmlEncode(comment.Title) + "</td>");
                result.Append("<td class='creator'>" + HttpUtility.HtmlEncode(comment.Creator) + "</td>");
                result.Append("<td class='content'>" + HttpUtility.HtmlEncode(comment.Content).Replace("\\r", "<br/>") + "</td>");

                result.Append("</tr>");
            }

            if (result.Length == 0)
                return 0;

            StringBuilder columntitle = new StringBuilder(4096);
            columntitle.Append("<tr class='column'>");
            columntitle.Append("<td class='title'>标题</td>");
            columntitle.Append("<td class='creator'>作者</td>");
            columntitle.Append("<td class='content'>正文</td>");
            columntitle.Append("</tr>");

            strHtml = "<table class='comments'>" + columntitle.ToString() + result.ToString() + "</table>";
            return 0;
        }

#if NO
        // 构造用于保存的评注信息数组
        int BuildSaveComments(
            out EntityInfo[] comments,
            out string strError)
        {
            strError = "";
            comments = null;
            int nRet = 0;

            Debug.Assert(this.Items != null, "");

            List<EntityInfo> commentArray = new List<EntityInfo>();

            foreach (CommentItem commentitem in this.Items)
            {
                // CommentItem commentitem = this.CommentItems[i];

                if (commentitem.ItemDisplayState == ItemDisplayState.Normal)
                    continue;

                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(commentitem.RefID) == true)
                {
                    commentitem.RefID = Guid.NewGuid().ToString();
                    commentitem.RefreshListView();
                }

                info.RefID = commentitem.RefID;  // 2008/2/17 

                string strXml = "";
                nRet = commentitem.BuildRecord(out strXml,
                        out strError);
                if (nRet == -1)
                    return -1;

                if (commentitem.ItemDisplayState == ItemDisplayState.New)
                {
                    info.Action = "new";
                    info.NewRecPath = "";
                    info.NewRecord = strXml;
                    info.NewTimestamp = null;
                }

                if (commentitem.ItemDisplayState == ItemDisplayState.Changed)
                {
                    info.Action = "change";
                    info.OldRecPath = commentitem.RecPath;
                    info.NewRecPath = commentitem.RecPath;

                    info.NewRecord = strXml;
                    info.NewTimestamp = null;

                    info.OldRecord = commentitem.OldRecord;
                    info.OldTimestamp = commentitem.Timestamp;
                }

                if (commentitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    info.Action = "delete";
                    info.OldRecPath = commentitem.RecPath; // NewRecPath

                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    info.OldRecord = commentitem.OldRecord;
                    info.OldTimestamp = commentitem.Timestamp;
                }

                commentArray.Add(info);
            }

            // 复制到目标
            comments = new EntityInfo[commentArray.Count];
            for (int i = 0; i < commentArray.Count; i++)
            {
                comments[i] = commentArray[i];
            }

            return 0;
        }

        // 构造用于修改归属的信息数组
        // 如果strNewBiblioPath中的书目库名发生变化，那评注记录都要在评注库之间移动，因为评注库和书目库有一定的捆绑关系。
        int BuildChangeParentRequestComments(
            List<CommentItem> commentitems,
            string strNewBiblioRecPath,
            out EntityInfo[] entities,
            out string strError)
        {
            strError = "";
            entities = null;
            int nRet = 0;

            string strSourceBiblioDbName = Global.GetDbName(this.BiblioRecPath);
            string strTargetBiblioDbName = Global.GetDbName(strNewBiblioRecPath);

            // 检查一下目标书目库名是不是合法的书目库名
            if (MainForm.IsValidBiblioDbName(strTargetBiblioDbName) == false)
            {
                strError = "目标库名 '" + strTargetBiblioDbName + "' 不在系统定义的书目库名之列";
                return -1;
            }

            // 获得目标书目记录id
            string strTargetBiblioRecID = Global.GetRecordID(strNewBiblioRecPath);   // !!!
            if (String.IsNullOrEmpty(strTargetBiblioRecID) == true)
            {
                strError = "因目标书目记录路径 '" + strNewBiblioRecPath + "' 中没有包含ID部分，无法进行操作";
                return -1;
            }
            if (strTargetBiblioRecID == "?")
            {
                strError = "目标书目记录路径 '" + strNewBiblioRecPath + "' 中记录ID不应为问号";
                return -1;
            }
            if (Global.IsPureNumber(strTargetBiblioRecID) == false)
            {
                strError = "目标书目记录路径 '" + strNewBiblioRecPath + "' 中记录ID应为纯数字";
                return -1;
            }

            bool bMove = false; // 是否需要移动评注记录
            string strTargetCommentDbName = "";  // 目标评注库名

            if (strSourceBiblioDbName != strTargetBiblioDbName)
            {
                // 书目库发生了改变，才有必要移动。否则仅仅修改评注记录的<parent>即可
                bMove = true;
                strTargetCommentDbName = MainForm.GetCommentDbName(strTargetBiblioDbName);

                if (String.IsNullOrEmpty(strTargetCommentDbName) == true)
                {
                    strError = "书目库 '" + strTargetBiblioDbName + "' 并没有从属的评注库定义。操作失败";
                    return -1;
                }
            }

            Debug.Assert(commentitems != null, "");

            List<EntityInfo> entityArray = new List<EntityInfo>();

            for (int i = 0; i < commentitems.Count; i++)
            {
                CommentItem commentitem = commentitems[i];

                EntityInfo info = new EntityInfo();

                if (String.IsNullOrEmpty(commentitem.RefID) == true)
                {
                    Debug.Assert(false, "commentitem.RefID应当为只读，并且不可能为空");
                    /*
                    commentitem.RefID = Guid.NewGuid().ToString();
                    commentitem.RefreshListView();
                     * */
                }

                info.RefID = commentitem.RefID;
                commentitem.Parent = strTargetBiblioRecID;

                string strXml = "";
                nRet = commentitem.BuildRecord(out strXml,
                        out strError);
                if (nRet == -1)
                    return -1;

                info.OldRecPath = commentitem.RecPath;
                if (bMove == false)
                {
                    info.Action = "change";
                    info.NewRecPath = commentitem.RecPath;
                }
                else
                {
                    info.Action = "move";
                    Debug.Assert(String.IsNullOrEmpty(strTargetCommentDbName) == false, "");
                    info.NewRecPath = strTargetCommentDbName + "/?";  // 把评注记录移动到另一个评注库中，追加成一条新记录，而旧记录自动被删除
                }

                info.NewRecord = strXml;
                info.NewTimestamp = null;

                info.OldRecord = commentitem.OldRecord;
                info.OldTimestamp = commentitem.Timestamp;

                entityArray.Add(info);
            }

            // 复制到目标
            entities = new EntityInfo[entityArray.Count];
            for (int i = 0; i < entityArray.Count; i++)
            {
                entities[i] = entityArray[i];
            }

            return 0;
        }

        // 保存评注记录
        // 不负责刷新界面和报错
        int SaveCommentRecords(string strBiblioRecPath,
            EntityInfo[] comments,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在保存评注信息 ...");
            Stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.SetComments(
                    Stop,
                    strBiblioRecPath,
                    comments,
                    out errorinfos,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

            }
            finally
            {
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }

        // 把报错信息中的成功事项的状态修改兑现
        // 并且彻底去除没有报错的“删除”CommentItem事项（内存和视觉上）
        // return:
        //      false   没有警告
        //      true    出现警告
        bool RefreshOperResult(EntityInfo[] errorinfos)
        {
            int nRet = 0;

            string strWarning = ""; // 警告信息

            if (errorinfos == null)
                return false;

            bool bOldChanged = this.Items.Changed;

            for (int i = 0; i < errorinfos.Length; i++)
            {
                CommentItem commentitem = null;

                string strError = "";

                if (String.IsNullOrEmpty(errorinfos[i].RefID) == true)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "服务器返回的EntityInfo结构中RefID为空");
                    return true;
                }

                nRet = LocateCommentItem(
                    errorinfos[i].RefID,
                    OrderControl.GetOneRecPath(errorinfos[i].NewRecPath, errorinfos[i].OldRecPath),
                    out commentitem,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "定位错误信息 '" + errorinfos[i].ErrorInfo + "' 所在行的过程中发生错误:" + strError);
                    continue;
                }

                if (nRet == 0)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "无法定位索引值为 " + i.ToString() + " 的错误信息 '" + errorinfos[i].ErrorInfo + "'");
                    continue;
                }

                string strLocationSummary = GetLocationSummary(
                    commentitem.Index,    // strIndex,
                    errorinfos[i].NewRecPath,
                    errorinfos[i].RefID);

                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                {
                    if (errorinfos[i].Action == "new")
                    {
                        commentitem.OldRecord = errorinfos[i].NewRecord;
                        nRet = commentitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, strError);
                    }
                    else if (errorinfos[i].Action == "change"
                        || errorinfos[i].Action == "move")
                    {
                        commentitem.OldRecord = errorinfos[i].NewRecord;

                        nRet = commentitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, strError);

                        commentitem.ItemDisplayState = ItemDisplayState.Normal;
                    }

                    // 对于保存后变得不再属于本种的，要在listview中消除
                    if (String.IsNullOrEmpty(commentitem.RecPath) == false)
                    {
                        string strTempCommentDbName = Global.GetDbName(commentitem.RecPath);
                        string strTempBiblioDbName = this.MainForm.GetBiblioDbNameFromCommentDbName(strTempCommentDbName);

                        Debug.Assert(String.IsNullOrEmpty(strTempBiblioDbName) == false, "");
                        // TODO: 这里要正规报错

                        string strTempBiblioRecPath = strTempBiblioDbName + "/" + commentitem.Parent;

                        if (strTempBiblioRecPath != this.BiblioRecPath)
                        {
                            this.Items.PhysicalDeleteItem(commentitem);
                            continue;
                        }
                    }

                    commentitem.Error = null;   // 还是显示 空?

                    commentitem.Changed = false;
                    commentitem.RefreshListView();
                    continue;
                }

                // 报错处理
                commentitem.Error = errorinfos[i];
                commentitem.RefreshListView();

                strWarning += strLocationSummary + "在提交评注保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }


            // 最后把没有报错的，那些成功删除事项，都从内存和视觉上抹除
            for (int i = 0; i < this.Items.Count; i++)
            {
                CommentItem commentitem = this.Items[i] as CommentItem;
                if (commentitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    if (commentitem.ErrorInfo == "")
                    {
                        this.Items.PhysicalDeleteItem(commentitem);
                        i--;
                    }
                }
            }

            // 修改Changed状态
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Items.Changed;
                this.ContentChanged(this, e1);
            }

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改评注信息后重新提交保存";
                MessageBox.Show(ForegroundWindow.Instance, strWarning);
                return true;
            }

            return false;
        }


        // 构造事项称呼
        static string GetLocationSummary(
            string strIndex,
            string strRecPath,
            string strRefID)
        {
            if (String.IsNullOrEmpty(strIndex) == false)
                return "编号为 '" + strIndex + "' 的事项";
            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";


            return "无任何定位信息的事项";
        }

#endif

        // 构造事项称呼
        internal override string GetLocationSummary(CommentItem bookitem)
        {
            string strIndex = bookitem.Index;

            if (String.IsNullOrEmpty(strIndex) == false)
                return "编号为 '" + strIndex + "' 的事项";

            string strRecPath = bookitem.RecPath;

            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            string strRefID = bookitem.RefID;
            // 2008/6/24 
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";

            return "无任何定位信息的事项";
        }

#if NO
        void EnableControls(bool bEnable)
        {
            if (this.EnableControlsEvent == null)
                return;

            EnableControlsEventArgs e = new EnableControlsEventArgs();
            e.bEnable = bEnable;
            this.EnableControlsEvent(this, e);
        }
#endif

#if NO
        // 在this.commentitems中定位和strRefID关联的事项
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateCommentItem(
            string strRefID,
            string strRecPath,
            out CommentItem commentitem,
            out string strError)
        {
            strError = "";

            // 优先用记录路径来定位
            if (string.IsNullOrEmpty(strRecPath) == false
                && Global.IsAppendRecPath(strRecPath) == false)
            {
                commentitem = this.Items.GetItemByRecPath(strRecPath) as CommentItem;
                if (commentitem != null)
                    return 1;   // found
            }

            // 然后用参考ID来定位
            commentitem = this.Items.GetItemByRefID(strRefID, null) as CommentItem;

            if (commentitem != null)
                return 1;   // found

            strError = "没有找到 记录路径为 '" + strRecPath + "'，并且 参考ID 为 '" + strRefID + "' 的CommentItem事项";
            return 0;
        }

#endif

        private void ListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool bHasBillioLoaded = false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
                bHasBillioLoaded = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("查看(&V)");
            menuItem.Click += new System.EventHandler(this.menu_viewComment_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            menuItem.DefaultItem = true;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyComment_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newComment_Click);
            if (bHasBillioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);



            // 改变归属
            menuItem = new MenuItem("改变归属(&B)");
            menuItem.Click += new System.EventHandler(this.menu_changeParent_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入新开的评注窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToNewItemForm_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入已经打开的评注窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToExistItemForm_Click);
            if (this.listView.SelectedItems.Count == 0
                || this.MainForm.GetTopChildWindow<ItemInfoForm>() == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("察看评注记录的检索点 (&K)");
            menuItem.Click += new System.EventHandler(this.menu_getKeys_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("增添自由词(&S)");
            menuItem.Click += new System.EventHandler(this.menu_addSubject_Click);
            if (this.listView.SelectedItems.Count == 0 || this.AddSubject == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("标记删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteComment_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("撤销删除(&U)");
            menuItem.Click += new System.EventHandler(this.menu_undoDeleteComment_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView, new Point(e.X, e.Y));
        }

        // 增添自由词
        void menu_addSubject_Click(object sender, EventArgs e)
        {
            if (this.AddSubject == null)
            {
                MessageBox.Show(this, "CommentControl没有挂接AddSubject事件");
                return;
            }

            List<string> new_subjects = new List<string>();
            List<string> hidden_subjects = new List<string>();
            foreach (ListViewItem item in this.listView.SelectedItems)
            {
                CommentItem comment_item = (CommentItem)item.Tag;
                if (comment_item == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                List<string> temp = StringUtil.SplitList(comment_item.Content.Replace("\\r", "\n"), '\n');

                hidden_subjects.AddRange(temp);

                if (StringUtil.IsInList("已处理", comment_item.State) == false)
                    new_subjects.AddRange(temp);
            }

            StringUtil.RemoveDupNoSort(ref hidden_subjects);
            StringUtil.RemoveBlank(ref hidden_subjects);
            StringUtil.RemoveDupNoSort(ref new_subjects);
            StringUtil.RemoveBlank(ref new_subjects);

            {
                AddSubjectEventArgs e1 = new AddSubjectEventArgs();
                e1.FocusedControl = this.listView;
                e1.NewSubjects = new_subjects;
                e1.HiddenSubjects = hidden_subjects;
                this.AddSubject(this, e1);

                if (string.IsNullOrEmpty(e1.ErrorInfo) == false && e1.ShowErrorBox == false)
                    MessageBox.Show(this, e1.ErrorInfo);

                if (e1.Canceled == true)
                    return;
            }

            bool bOldChanged = this.Items.Changed;

            // 修改评注记录的状态
            foreach (ListViewItem item in this.listView.SelectedItems)
            {
                CommentItem comment_item = (CommentItem)item.Tag;
                if (comment_item == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                string strState = comment_item.State;
                string strOldState = strState;

                Global.ModifyStateString(ref strState, "已处理", "");

                if (strState == strOldState)
                    continue;   // 没有必要修改

                comment_item.State = strState;
                comment_item.RefreshListView();
                comment_item.Changed = true;
            }

#if NO
            if (this.ContentChanged != null
                && bOldChanged != this.Items.Changed)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Items.Changed;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, this.Items.Changed);
        }

        /// <summary>
        /// 装入新开的评注窗
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        void menu_loadToNewItemForm_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未选定要操作的事项";
                goto ERROR1;
            }

            CommentItem cur = (CommentItem)this.ListView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "CommentItem == null";
                goto ERROR1;
            }

            string strRecPath = cur.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "所选定的事项记录路径为空，尚未在数据库中建立";
                goto ERROR1;
            }

            ItemInfoForm form = null;

            form = new ItemInfoForm();
            form.MdiParent = this.MainForm;
            form.MainForm = this.MainForm;
            form.Show();

            form.DbType = "comment";

            form.LoadRecordByRecPath(strRecPath, "");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            LoadToItemInfoForm(true);

        }

        void menu_loadToExistItemForm_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未选定要操作的事项";
                goto ERROR1;
            }

            CommentItem cur = (CommentItem)this.ListView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "CommentItem == null";
                goto ERROR1;
            }

            string strRecPath = cur.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "所选定的事项记录路径为空，尚未在数据库中建立";
                goto ERROR1;
            }

            ItemInfoForm form = this.MainForm.GetTopChildWindow<ItemInfoForm>();
            if (form == null)
            {
                strError = "当前并没有已经打开的评注窗";
                goto ERROR1;
            }
            form.DbType = "comment";
            Global.Activate(form);
            if (form.WindowState == FormWindowState.Minimized)
                form.WindowState = FormWindowState.Normal;

            form.LoadRecordByRecPath(strRecPath, "");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            LoadToItemInfoForm(false);

        }

        void menu_viewComment_Click(object sender, EventArgs e)
        {
            DoViewComment(true);
        }

        // 在记录中增加<_recPath>元素
        /*public*/
        static int AddRecPath(ref string strXml,
            string strRecPath,
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
                strError = "XML字符串装入DOM时出错: " + ex.Message;
                return -1;
            }

            DomUtil.SetElementText(dom.DocumentElement,
                "_recPath",
                strRecPath);
            strXml = dom.DocumentElement.OuterXml;
            return 0;
        }

        void DoViewComment(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
            }

            /*
            if (this.ListView.SelectedItems.Count == 0)
                return;
             * */
            if (this.listView.SelectedItems.Count != 1)
            {
                // 2012/10/8
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();

                return;
            }

            CommentItem commentitem = (CommentItem)this.listView.SelectedItems[0].Tag;
            //if (String.IsNullOrEmpty(commentitem.RecPath) == true)
            //    return;

            int nRet = commentitem.BuildRecord(
                true,   // 要检查 Parent 成员
                out strXml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 2012/12/28
            // 在记录中增加<_recPath>元素
            nRet = AddRecPath(ref strXml,
                commentitem.RecPath,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = GetCommentHtml(
                null,
                strXml,
                out strHtml,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            bool bNew = false;
            if (this.m_commentViewer == null
                || (bOpenWindow == true && this.m_commentViewer.Visible == false))
            {
                m_commentViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_commentViewer, this.Font, false);
                bNew = true;
            }

            m_commentViewer.MainForm = this.MainForm;  // 必须是第一句

            if (bNew == true)
                m_commentViewer.InitialWebBrowser();

            m_commentViewer.Text = "评注 '" + commentitem.RecPath + "'";
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = strXml;
            m_commentViewer.FormClosed -= new FormClosedEventHandler(m_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(m_viewer_FormClosed);
            // this.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_commentViewer, "comment_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    this.MainForm.CurrentPropertyControl = null;
                }
                else
                {
                    if (m_commentViewer.WindowState == FormWindowState.Minimized)
                        m_commentViewer.WindowState = FormWindowState.Normal;
                    m_commentViewer.Activate();
                }
            }
            else
            {
                if (m_commentViewer.Visible == true)
                {

                }
                else
                {
                    if (this.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewComment() 出错: " + strError);
        }

        void m_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        /*public*/
        int GetCommentHtml(
            LibraryChannel channel_param,
            string strXml,
            out string strHtml,
            out string strError)
        {
            strError = "";
            strHtml = "";

            LibraryChannel channel = channel_param;
            if (channel == null)
                channel = this.MainForm.GetChannel();
#if NO
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在获得评注 HTML 信息 ...");
            Stop.BeginLoop();

            this.Update();
#endif
            Stop.SetMessage("正在获得评注 HTML 信息 ...");

            try
            {
                string strOutputCommentRecPath = "";
                byte[] baTimestamp = null;
                string strBiblio = "";
                string strOutputBiblioRecPath = "";

                long lRet = channel.GetCommentInfo(
                    Stop,
                    strXml,
                    // "",
                    "html",
                    out strHtml,
                    out strOutputCommentRecPath,
                    out baTimestamp,
                    "",
                    out strBiblio,
                    out strOutputBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                if (channel_param == null)
                    this.MainForm.ReturnChannel(channel);
#if NO
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
#endif
                Stop.SetMessage("");
            }

            return 1;
        ERROR1:
            return -1;
        }

#if NO
        public int GetCommentHtml(string strCommentRecPath,
            out string strHtml,
            out string strXml,
            out string strError)
        {
            strError = "";
            strHtml = "";
            strXml = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在装入评注 HTML/XML 信息 ...");
            stop.BeginLoop();

            this.Update();

            try
            {
                string strOutputCommentRecPath = "";
                byte [] baTimestamp = null;
                string strBiblio = "";
                string strOutputBiblioRecPath = "";
               
                long lRet = Channel.GetCommentInfo(
                    stop,
                    string.IsNullOrEmpty(strCommentRecPath) == false && strCommentRecPath[0] == '<' ? strCommentRecPath : "@path:" + strCommentRecPath,
                    // "",
                    "html",
                    out strHtml,
                    out strOutputCommentRecPath,
                    out baTimestamp,
                    "",
                    out strBiblio,
                    out strOutputBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                lRet = Channel.GetCommentInfo(
    stop,
    "@path:" + strCommentRecPath,
    // "",
    "xml",
    out strXml,
    out strOutputCommentRecPath,
    out baTimestamp,
    "",
    out strBiblio,
    out strOutputBiblioRecPath,
    out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;
        ERROR1:
            return -1;
        }
#endif

        void menu_modifyComment_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要编辑的事项");
                return;
            }
            CommentItem commentitem = (CommentItem)this.listView.SelectedItems[0].Tag;

            ModifyComment(commentitem);
        }

#if NO
        // 为CommentItem对象设置缺省值
        // parameters:
        //      strCfgEntry 为"comment_normalRegister_default"或"comment_quickRegister_default"
        int SetCommentItemDefaultValues(
            string strCfgEntry,
            CommentItem commentitem,
            out string strError)
        {
            strError = "";

            string strNewDefault = this.MainForm.AppInfo.GetString(
    "entityform_optiondlg",
    strCfgEntry,
    "<root />");

            // 字符串strNewDefault包含了一个XML记录，里面相当于一个记录的原貌。
            // 但是部分字段的值可能为"@"引导，表示这是一个宏命令。
            // 需要把这些宏兑现后，再正式给控件
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strNewDefault);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            // 遍历所有一级元素的内容
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strText = nodes[i].InnerText;
                if (strText.Length > 0 && strText[0] == '@')
                {
                    // 兑现宏
                    nodes[i].InnerText = DoGetMacroValue(strText);
                }
            }

            strNewDefault = dom.OuterXml;

            int nRet = commentitem.SetData("",
                strNewDefault,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            commentitem.Parent = "";
            commentitem.RecPath = "";

            return 0;
        }
#endif

#if NO
        string DoGetMacroValue(string strMacroName)
        {
            if (this.GetMacroValue != null)
            {
                GetMacroValueEventArgs e = new GetMacroValueEventArgs();
                e.MacroName = strMacroName;
                this.GetMacroValue(this, e);

                return e.MacroValue;
            }

            return null;
        }
#endif

        void ModifyComment(
            CommentItem commentitem)
        {
            Debug.Assert(commentitem != null, "");

            bool bOldChanged = this.Items.Changed;

            string strOldIndex = commentitem.Index;

            CommentEditForm edit = new CommentEditForm();

            edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);
            edit.MainForm = this.MainForm;
            edit.ItemControl = this;
            string strError = "";
            int nRet = edit.InitialForEdit(commentitem,
                this.Items,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, strError);
                return;
            }
            edit.StartItem = null;  // 清除原始对象标记

        REDO:
            this.MainForm.AppInfo.LinkFormState(edit, "CommentEditForm_state");
            edit.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(edit);

            if (edit.DialogResult != DialogResult.OK)
                return;

            RefreshOrderSuggestionPie();

            DoViewComment(false);

#if NO
            // CommentItem对象已经被修改
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = true;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, true);

            LibraryChannel channel = this.MainForm.GetChannel();

            this.EnableControls(false);
            try
            {
                if (strOldIndex != commentitem.Index) // 编号改变了的情况下才查重
                {
                    // 需要排除掉自己: commentitem。
                    List<CommentItem> excludeItems = new List<CommentItem>();
                    excludeItems.Add(commentitem);


                    // 对当前窗口内进行编号查重
                    CommentItem dupitem = this.Items.GetItemByIndex(
                        commentitem.Index,
                        excludeItems);
                    if (dupitem != null)
                    {
                        string strText = "";
                        if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                            strText = "编号 '" + commentitem.Index + "' 和本种中未提交之一删除编号相重。按“确定”按钮重新输入，或退出对话框后先行提交已有之修改。";
                        else
                            strText = "编号 '" + commentitem.Index + "' 在本种中已经存在。按“确定”按钮重新输入。";

                        MessageBox.Show(ForegroundWindow.Instance, strText);
                        goto REDO;
                    }

                    // 对(本种)所有评注记录进行编号查重
                    if (edit.AutoSearchDup == true
                        && string.IsNullOrEmpty(commentitem.RefID) == false)
                    {
                        // Debug.Assert(false, "");

                        string[] paths = null;
                        // 编号查重。
                        // parameters:
                        //      strOriginRecPath    出发记录的路径。
                        //      paths   所有命中的路径
                        // return:
                        //      -1  error
                        //      0   not dup
                        //      1   dup
                        nRet = SearchCommentRefIdDup(
                            channel,
                            commentitem.RefID,
                            // this.BiblioRecPath,
                            commentitem.RecPath,
                            out paths,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, "对参考ID '" + commentitem.RefID + "' 进行查重的过程中发生错误: " + strError);

                        else if (nRet == 1) // 发生重复
                        {
                            string pathlist = String.Join(",", paths);

                            string strText = "参考ID '" + commentitem.RefID + "' 在数据库中发现已经被(属于其他种的)下列评注记录所使用。\r\n" + pathlist + "\r\n\r\n按“确定”按钮重新编辑评注信息，或者根据提示的评注记录路径，去修改其他评注记录信息。";
                            MessageBox.Show(ForegroundWindow.Instance, strText);

                            goto REDO;
                        }
                    }
                }

            }
            finally
            {
                this.EnableControls(true);

                this.MainForm.ReturnChannel(channel);
            }
        }

#if NO
        // 参考ID查重。用于(可能是)旧参考ID查重。
        // 本函数可以自动排除和当前路径strOriginRecPath重复之情形
        // parameters:
        //      strOriginRecPath    出发记录的路径。
        //      paths   所有命中的路径
        // return:
        //      -1  error
        //      0   not dup
        //      1   dup
        int SearchCommentRefIdDup(string strRefID,
            string strBiblioRecPath,
            string strOriginRecPath,
            out string[] paths,
            out string strError)
        {
            strError = "";
            paths = null;

            if (string.IsNullOrEmpty(strRefID) == true)
            {
                strError = "不应用参考ID为空来查重";
                return -1;
            }

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SearchCommentDup(
                    stop,
                    strRefID,
                    strBiblioRecPath,
                    100,
                    out paths,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found

                if (lRet == 1)
                {
                    // 检索命中一条。看看路径是否和出发记录一样
                    if (paths.Length != 1)
                    {
                        strError = "系统错误: SearchCommentDup() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
                        return -1;
                    }

                    if (paths[0] != strOriginRecPath)
                        return 1;   // 发现重复的了

                    return 0;   // 不重复
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;   // found
        }
#endif
        int SearchCommentRefIdDup(
            LibraryChannel channel,
            string strRefID,
            // string strBiblioRecPath,
    string strOriginRecPath,
    out string[] paths,
    out string strError)
        {
            strError = "";
            paths = null;

            if (string.IsNullOrEmpty(strRefID) == true)
            {
                strError = "不应用参考ID为空来查重";
                return -1;
            }

#if NO
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            Stop.BeginLoop();
#endif
            Stop.SetMessage("正在对参考ID '" + strRefID + "' 进行查重 ...");

            try
            {
                long lRet = channel.SearchComment(
    Stop,
    "<全部>",
    strRefID,
    100,
    "参考ID",
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
                lRet = channel.GetSearchResult(Stop,
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

                if (lHitCount == 1)
                {
                    // 检索命中一条。看看路径是否和出发记录一样
                    if (paths.Length != 1)
                    {
                        strError = "系统错误: SearchComment() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
                        return -1;
                    }

                    if (paths[0] != strOriginRecPath)
                        return 1;   // 发现重复的了

                    return 0;   // 不重复
                }
            }
            finally
            {
#if NO
                Stop.EndLoop();
                Stop.OnStop -= new StopEventHandler(this.DoStop);
                Stop.Initial("");
#endif
                Stop.SetMessage("");
            }

            return 1;   // found
        }


        void menu_newComment_Click(object sender, EventArgs e)
        {
            DoNewComment();
        }

        // 新增一个评注事项，要打开对话框让输入详细信息
        void DoNewComment(
            /*string strIndex*/)
        {
            string strError = "";
            int nRet = 0;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未载入书目记录";
                goto ERROR1;
            }

            // 
            if (this.Items == null)
                this.Items = new CommentItemCollection();

            Debug.Assert(this.Items != null, "");

            bool bOldChanged = this.Items.Changed;

#if NO
            if (String.IsNullOrEmpty(strIndex) == false)
            {

                // 对当前窗口内进行编号查重
                CommentItem dupitem = this.CommentItems.GetItemByIndex(
                    strIndex,
                    null);
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的编号 '" + strIndex + "' 和本种中未提交之一删除编号相重。请先行提交已有之修改，再进行新评注操作。";
                    else
                        strText = "拟新增的编号 '" + strIndex + "' 在本种中已经存在。";

                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
        strText + "\r\n\r\n要立即对已存在编号进行修改吗？",
        "CommentControl",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);

                    // 转为修改
                    if (result == DialogResult.Yes)
                    {
                        ModifyComment(dupitem);
                        return;
                    }

                    // 突出显示，以便操作人员观察这条已经存在的记录
                    dupitem.HilightListViewItem(true);
                    return;
                }

                // 对(本种)所有评注记录进行编号查重
                if (true)
                {
                    string strCommentText = "";
                    string strBiblioText = "";
                    nRet = SearchCommentIndex(strIndex,
                        this.BiblioRecPath,
                        out strCommentText,
                        out strBiblioText,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(ForegroundWindow.Instance, "对编号 '" + strIndex + "' 进行查重的过程中发生错误: " + strError);
                    else if (nRet == 1) // 发生重复
                    {
                        OrderIndexFoundDupDlg dlg = new OrderIndexFoundDupDlg();
                        MainForm.SetControlFont(dlg, this.Font, false);
                        dlg.MainForm = this.MainForm;
                        dlg.BiblioText = strBiblioText;
                        dlg.OrderText = strCommentText;
                        dlg.MessageText = "拟新增的编号 '" + strIndex + "' 在数据库中发现已经存在。因此无法新增。";
                        dlg.ShowDialog(this);
                        return;
                    }
                }

            } // end of ' if (String.IsNullOrEmpty(strIndex) == false)
#endif

            CommentItem commentitem = new CommentItem();

            // 设置缺省值
            nRet = SetItemDefaultValues(
                "comment_normalRegister_default",
                true,
                commentitem,
                out strError);
            if (nRet == -1)
            {
                strError = "设置缺省值的时候发生错误: " + strError;
                goto ERROR1;
            }

#if NO
            commentitem.Index = strIndex;
#endif
            commentitem.Parent = Global.GetRecordID(this.BiblioRecPath);

            // 先加入列表
            this.Items.Add(commentitem);
            commentitem.ItemDisplayState = ItemDisplayState.New;
            commentitem.AddToListView(this.listView);
            commentitem.HilightListViewItem(true);

            commentitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对


            CommentEditForm edit = new CommentEditForm();

            edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);
            edit.Text = "新增评注事项";
            edit.MainForm = this.MainForm;
            nRet = edit.InitialForEdit(commentitem,
                this.Items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            //REDO:
            this.MainForm.AppInfo.LinkFormState(edit, "CommentEditForm_state");
            edit.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(edit);

            if (edit.DialogResult != DialogResult.OK
                && edit.Item == commentitem    // 表明尚未前后移动，或者移动回到起点，然后Cancel
                )
            {
                this.Items.PhysicalDeleteItem(commentitem);

#if NO
                // 改变保存按钮状态
                // SetSaveAllButtonState(true);
                if (this.ContentChanged != null)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = this.Items.Changed;
                    this.ContentChanged(this, e1);
                }
#endif
                TriggerContentChanged(bOldChanged, this.Items.Changed);

                return;
            }

            RefreshOrderSuggestionPie();
            DoViewComment(false);

#if NO
            // 改变保存按钮状态
            // SetSaveAllButtonState(true);
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = true;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, true);



            // 要对本种和所有相关实体库进行编号查重。
            // 如果重了，要保持窗口，以便修改。不过从这个角度，查重最好在对话框关闭前作？
            // 或者重新打开对话框
            string strRefID = commentitem.RefID;
            if (String.IsNullOrEmpty(strRefID) == false)
            {

                // 需要排除掉刚加入的自己: commentitem。
                List<BookItemBase> excludeItems = new List<BookItemBase>();
                excludeItems.Add(commentitem);

                // 对当前窗口内进行编号查重
                CommentItem dupitem = this.Items.GetItemByRefID(
                    strRefID,
                    excludeItems) as CommentItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的参考ID '" + strRefID + "' 和本种中未提交之一删除参考ID相重。请先行提交已有之修改，再进行新增评注操作。";
                    else
                        strText = "拟新增的参考ID '" + strRefID + "' 在本种中已经存在。";

                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
        strText + "\r\n\r\n要立即对新记录的参考ID进行修改吗？\r\n(Yes 进行修改; No 不修改，让发生重复的新记录进入列表; Cancel 放弃刚刚创建的新记录)",
        "CommentControl",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);

                    // 转为修改
                    if (result == DialogResult.Yes)
                    {
                        ModifyComment(commentitem);
                        return;
                    }

                    // 放弃刚刚创建的记录
                    if (result == DialogResult.Cancel)
                    {
                        this.Items.PhysicalDeleteItem(commentitem);

#if NO
                        // 改变保存按钮状态
                        // SetSaveAllButtonState(true);
                        if (this.ContentChanged != null)
                        {
                            ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                            e1.OldChanged = bOldChanged;
                            e1.CurrentChanged = this.Items.Changed;
                            this.ContentChanged(this, e1);
                        }
#endif
                        TriggerContentChanged(bOldChanged, this.Items.Changed);

                        return;
                    }

                    // 突出显示，以便操作人员观察这条已经存在的记录
                    dupitem.HilightListViewItem(true);
                    return;
                }
            } // end of ' if (String.IsNullOrEmpty(strIndex) == false)

            return;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

#if NO
        // 检索评注编号。用于新编号查重。
        // 注：仅用strRefID无法获得评注记录，必须加上书目记录路径才行
        int SearchCommentRefID(string strRefID,
            string strBiblioRecPath,
            out string strOrderText,
            out string strBiblioText,
            out string strError)
        {
            strError = "";
            strOrderText = "";
            strBiblioText = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                byte[] comment_timestamp = null;
                string strCommentRecPath = "";
                string strOutputBiblioRecPath = "";

                long lRet = Channel.GetCommentInfo(
                    stop,
                    strRefID,
                    strBiblioRecPath,
                    "html",
                    out strOrderText,
                    out strCommentRecPath,
                    out comment_timestamp,
                    "html",
                    out strBiblioText,
                    out strOutputBiblioRecPath,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            return 1;   // found
        }
#endif

#if NO
        // 改变归属
        // 即修改评注信息的<parent>元素内容，使指向另外一条书目记录
        void menu_changeParent_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未指定要修改归属的事项";
                goto ERROR1;
            }

            // TODO: 如果有尚未保存的,是否要提醒先保存?

            string strNewBiblioRecPath = InputDlg.GetInput(
                this,
                "请指定新的书目记录路径",
                "书目记录路径(格式'库名/ID'): ",
                "",
            this.MainForm.DefaultFont);

            if (strNewBiblioRecPath == null)
                return;

            // TODO: 最好检查一下这个路径的格式。合法的书目库名可以在MainForm中找到

            if (String.IsNullOrEmpty(strNewBiblioRecPath) == true)
            {
                strError = "尚未指定新的书目记录路径，放弃操作";
                goto ERROR1;
            }

            if (strNewBiblioRecPath == this.BiblioRecPath)
            {
                strError = "指定的新书目记录路径和当前书目记录路径相同，放弃操作";
                goto ERROR1;
            }

            List<CommentItem> selectedcommentitems = new List<CommentItem>();
            foreach (ListViewItem item in this.ListView.SelectedItems)
            {
                CommentItem commentitem = (CommentItem)item.Tag;

                selectedcommentitems.Add(commentitem);
            }

            EntityInfo[] comments = null;

            nRet = BuildChangeParentRequestComments(
                selectedcommentitems,
                strNewBiblioRecPath,
                out comments,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (comments == null || comments.Length == 0)
                return; // 没有必要保存

#if NO
            EntityInfo[] errorinfos = null;
            nRet = SaveCommentRecords(strNewBiblioRecPath,
                entities,
                out errorinfos,
                out strError);

            // 把出错的事项和需要更新状态的事项兑现到显示、内存
            // 是否有能力把归属已经改变的事项排除出listview?
            RefreshOperResult(errorinfos);

            if (nRet == -1)
            {
                goto ERROR1;
            }
#endif
            nRet = SaveComments(comments, out strError);
            if (nRet == -1)
                goto ERROR1;

            this.MainForm.StatusBarMessage = "评注信息 修改归属 成功";
            return;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
        }
#endif

        // 删除一个或多个评注事项
        void menu_deleteComment_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要标记删除的事项");
                return;
            }

            string strIndexList = "";
            for (int i = 0; i < this.listView.SelectedItems.Count; i++)
            {
                if (i > 20)
                {
                    strIndexList += "...(共 " + this.listView.SelectedItems.Count.ToString() + " 项)";
                    break;
                }
                string strIndex = this.listView.SelectedItems[i].Text;
                strIndexList += strIndex + "\r\n";
            }

            string strWarningText = "以下(编号的)评注事项将被标记删除: \r\n" + strIndexList + "\r\n\r\n确实要标记删除它们?";

            // 警告
            DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                strWarningText,
                "CommentControl",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Cancel)
                return;

            List<string> deleted_recpaths = new List<string>();

            this.EnableControls(false);

            try
            {
                bool bOldChanged = this.Items.Changed;

                // 实行删除
                List<ListViewItem> selectedItems = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView.SelectedItems)
                {
                    selectedItems.Add(item);
                }

                string strNotDeleteList = "";
                int nDeleteCount = 0;
                foreach (ListViewItem item in selectedItems)
                {
                    CommentItem commentitem = (CommentItem)item.Tag;

                    int nRet = MaskDeleteItem(commentitem,
                        m_bRemoveDeletedItem);

                    if (nRet == 0)
                    {
                        if (strNotDeleteList != "")
                            strNotDeleteList += ",";
                        strNotDeleteList += commentitem.Index;
                        continue;
                    }

                    if (string.IsNullOrEmpty(commentitem.RecPath) == false)
                        deleted_recpaths.Add(commentitem.RecPath);

                    nDeleteCount++;
                }

                string strText = "";

                if (strNotDeleteList != "")
                    strText += "编号为 '" + strNotDeleteList + "' 的评注事项未能加以标记删除。\r\n\r\n";

                if (deleted_recpaths.Count == 0)
                    strText += "共直接删除 " + nDeleteCount.ToString() + " 项。";
                else if (nDeleteCount - deleted_recpaths.Count == 0)
                    strText += "共标记删除 "
                        + deleted_recpaths.Count.ToString()
                        + " 项。\r\n\r\n(注：所标记删除的事项，要到“提交”后才会真正从服务器删除)";
                else
                    strText += "共标记删除 "
    + deleted_recpaths.Count.ToString()
    + " 项；直接删除 "
    + (nDeleteCount - deleted_recpaths.Count).ToString()
    + " 项。\r\n\r\n(注：所标记删除的事项，要到“提交”后才会真正从服务器删除)";

                MessageBox.Show(ForegroundWindow.Instance, strText);

#if NO
                if (this.ContentChanged != null
                    && bOldChanged != this.Items.Changed)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = this.Items.Changed;
                    this.ContentChanged(this, e1);
                }
#endif
                TriggerContentChanged(bOldChanged, this.Items.Changed);


            }
            finally
            {
                this.EnableControls(true);
            }
        }

#if NO
        // 标记删除事项
        // return:
        //      0   因为有册信息，未能标记删除
        //      1   成功删除
        int MaskDeleteItem(CommentItem commentitem,
            bool bRemoveDeletedItem)
        {
            this.Items.MaskDeleteItem(bRemoveDeletedItem,
                commentitem);
            return 1;
        }
#endif

        // 撤销删除一个或多个评注事项
        void menu_undoDeleteComment_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要撤销删除的事项");
                return;
            }

            this.EnableControls(false);

            try
            {
                bool bOldChanged = this.Items.Changed;

                // 实行Undo
                List<ListViewItem> selectedItems = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView.SelectedItems)
                {
                    selectedItems.Add(item);
                }

                string strNotUndoList = "";
                int nUndoCount = 0;
                foreach (ListViewItem item in selectedItems)
                {
                    CommentItem commentitem = (CommentItem)item.Tag;

                    bool bRet = this.Items.UndoMaskDeleteItem(commentitem);

                    if (bRet == false)
                    {
                        if (strNotUndoList != "")
                            strNotUndoList += ",";
                        strNotUndoList += commentitem.Index;
                        continue;
                    }

                    nUndoCount++;
                }

                string strText = "";

                if (strNotUndoList != "")
                    strText += "编号为 '" + strNotUndoList + "' 的事项先前并未被标记删除过, 所以现在谈不上撤销删除。\r\n\r\n";

                strText += "共撤销删除 " + nUndoCount.ToString() + " 项。";
                MessageBox.Show(ForegroundWindow.Instance, strText);

#if NO
                if (this.ContentChanged != null
    && bOldChanged != this.Items.Changed)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = this.Items.Changed;
                    this.ContentChanged(this, e1);
                }
#endif
                if (bOldChanged != this.Items.Changed)
                    TriggerContentChanged(bOldChanged, this.Items.Changed);

            }
            finally
            {
                this.EnableControls(true);
            }
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            // menu_modifyComment_Click(this, null);
            DoViewComment(true);
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoViewComment(false);
        }

        private void comboBox_libraryCodeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshOrderSuggestionPie();
        }

        private void pieChartControl1_SizeChanged(object sender, EventArgs e)
        {
            SetPieChartMargin();
        }

        // 根据PieChart控件尺寸设置margin参数
        void SetPieChartMargin()
        {
            int nMinWidth = Math.Min(this.pieChartControl1.Width, this.pieChartControl1.Height);
            int nMargin = nMinWidth / 8;
            nMinWidth -= nMargin * 2;
            int nHorzMargin = (this.pieChartControl1.Width - nMinWidth) / 2;
            int nVertMargin = (this.pieChartControl1.Height - nMinWidth) / 2;
            this.pieChartControl1.LeftMargin = nHorzMargin;
            this.pieChartControl1.RightMargin = nHorzMargin;
            this.pieChartControl1.TopMargin = nVertMargin;
            this.pieChartControl1.BottomMargin = nVertMargin;
        }

        private void comboBox_libraryCodeFilter_SizeChanged(object sender, EventArgs e)
        {
            this.comboBox_libraryCodeFilter.Invalidate();
        }

        public override string ErrorInfo
        {
            get
            {
                return base.ErrorInfo;
            }
            set
            {
                base.ErrorInfo = value;
                if (this.splitContainer_main != null)
                {
                    if (string.IsNullOrEmpty(value) == true)
                        this.splitContainer_main.Visible = true;
                    else
                        this.splitContainer_main.Visible = false;
                }
            }
        }

    }

    /// <summary>
    /// 添加自由词事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void AddSubjectEventHandler(object sender,
        AddSubjectEventArgs e);

    /// <summary>
    /// 添加自由词事件的参数
    /// </summary>
    public class AddSubjectEventArgs : EventArgs
    {
        // 
        /// <summary>
        /// [in] 当前焦点所在的子控件
        /// </summary>
        public object FocusedControl = null;    // [in]

        /// <summary>
        /// [in] 评注记录中打算新增的主题词，不包括状态具有"已处理"的。最后实际新增的内容可能会被修改
        /// </summary>
        public List<string> NewSubjects = null;    // [in] 评注记录中打算新增的主题词，不包括状态具有"已处理"的。最后实际新增的内容可能会被修改

        /// <summary>
        /// [in] 评注记录中打算新增的全部主题词，包括状态具有"已处理"的。
        /// </summary>
        public List<string> HiddenSubjects = null;    // [in] 评注记录中打算新增的全部主题词，包括状态具有"已处理"的。

        /// <summary>
        /// [out] 是否要放弃 CommentControl 的后续操作？ 后续操作指修改评注记录状态的操作
        /// </summary>
        public bool Canceled = false;           // [out] 是否要放弃 CommentControl 的后续操作？ 后续操作指修改评注记录状态的操作

        /// <summary>
        /// [in] 是否要在事件处理过程中显示出错 MessageBox
        /// </summary>
        public bool ShowErrorBox = true;        // [in]是否要在事件处理过程中显示出错MessageBox

        /// <summary>
        /// [out] 出错信息。事件处理过程中发生错误时会使用此成员
        /// </summary>
        public string ErrorInfo = "";           // [out]出错信息。事件处理过程中发生错误
    }

    // 如果不这样书写，视图设计器会出现故障
    /// <summary>
    /// ConmentControl 类的基础类
    /// </summary>
    public class CommentControlBase : ItemControlBase<CommentItem, CommentItemCollection>
    {
    }
}
