using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;    // LocationCollection
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 期记录列表控件
    /// </summary>
    public partial class IssueControl : IssueControlBase
    {
        public event GetBiblioEventHandler GetBiblio = null;
        // 
        /// <summary>
        /// 准备验收
        /// </summary>
        public event PrepareAcceptEventHandler PrepareAccept = null;

        // 
        /// <summary>
        /// 创建实体数据
        /// </summary>
        public event GenerateEntityEventHandler GenerateEntity = null;

        /// <summary>
        /// 修改册对象
        /// </summary>
        public event ChangeItemEventHandler ChangeItem = null;

        /// <summary>
        /// 目标记录路径
        /// </summary>
        public string TargetRecPath = "";   // 4种状态：1)这里的路径和当前记录路径一致，表明实体记录就创建在当前记录下；2)这里的路径和当前记录路径不一致，种记录已经存在，需要在它下面创建实体记录；3) 这里的路径仅有库名部分，表示种记录不存在，需要根据当前记录的MARC来创建；4) 这里的路径为空，表示需要通过菜单选择目标库，然后处理方法同3)

        /// <summary>
        /// 验收批次号
        /// </summary>
        public string AcceptBatchNo = "";   // 验收批次号

        /// <summary>
        /// 是否要在验收操作末段自动出现允许输入册条码号的界面?
        /// </summary>
        public bool InputItemsBarcode = true;   // 是否要在验收操作末段自动出现允许输入册条码号的界面?

        /*
        /// <summary>
        /// 是否为新创建的册记录设置“加工中”状态
        /// </summary>
        public bool SetProcessingState = true;   // 是否为新创建的册记录设置“加工中”状态 2009/10/19
        */

        /// <summary>
        /// 是否为新创建的册记录创建索取号
        /// </summary>
        public bool CreateCallNumber = false;   // 是否为新创建的册记录创建索取号 2012/5/7

        // 
        /// <summary>
        /// 获得订购信息
        /// </summary>
        public event GetOrderInfoEventHandler GetOrderInfo = null;

        // 
        /// <summary>
        /// 获得册信息
        /// </summary>
        public event GetItemInfoEventHandler GetItemInfo = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public IssueControl()
        {
            InitializeComponent();

            this.m_listView = this.listView;
            this.ItemType = "issue";
            this.ItemTypeName = "期";
        }

        // 
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 获得一个书目记录下属的全部期记录路径
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
        public static int GetIssueRecPaths(
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
                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断";
                    return -1;
                }
                EntityInfo[] entities = null;

                /*
                if (lCount > 0)
                    stop.SetMessage("正在装入册信息 " + lStart.ToString() + "-" + (lStart + lCount - 1).ToString() + " ...");
                 * */

                long lRet = channel.GetIssues(
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
                        strError = "路径为 '" + entities[i].OldRecPath + "' 的期记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
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
        // 装入期记录
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        public int LoadIssueRecords(string strBiblioRecPath,
            out string strError)
        {
            this.BiblioRecPath = strBiblioRecPath;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在装入期信息 ...");
            Stop.BeginLoop();

            this.Update();
            // Program.MainForm.Update();

            try
            {
                // string strHtml = "";
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                this.ClearIssues();

                // 2012/5/9 改写为循环方式
                for (; ; )
                {
                    EntityInfo[] issues = null;

                    long lRet = Channel.GetIssues(
                        Stop,
                        strBiblioRecPath,
                            lStart,
                            lCount,
                            "",
                            "zh",
                        out issues,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    if (lRet == 0)
                        return 0;

                    lResultCount = lRet;

                    Debug.Assert(issues != null, "");

                    this.ListView.BeginUpdate();
                    try
                    {
                        for (int i = 0; i < issues.Length; i++)
                        {
                            if (issues[i].ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + issues[i].OldRecPath + "' 的期记录装载中发生错误: " + issues[i].ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            // 剖析一个期的xml记录，取出有关信息放入listview中
                            IssueItem issueitem = new IssueItem();

                            int nRet = issueitem.SetData(issues[i].OldRecPath, // NewRecPath
                                     issues[i].OldRecord,
                                     issues[i].OldTimestamp,
                                     out strError);
                            if (nRet == -1)
                                return -1;

                            if (issues[i].ErrorCode == ErrorCodeValue.NoError)
                                issueitem.Error = null;
                            else
                                issueitem.Error = issues[i];

                            this.Items.Add(issueitem);


                            issueitem.AddToListView(this.ListView);
                        }
                    }
                    finally
                    {
                        this.ListView.EndUpdate();
                    }

                    lStart += issues.Length;
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

            return 1;
        ERROR1:
            return -1;
        }

#endif

        // 新增一个期，要打开对话框让输入详细信息
        void DoNewIssue(/*string strPublishTime*/)
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
                this.Items = new IssueItemCollection();

            Debug.Assert(this.Items != null, "");

            bool bOldChanged = this.Items.Changed;

#if NO
            if (String.IsNullOrEmpty(strPublishTime) == false)
            {

                // 对当前窗口内进行出版时间查重
                IssueItem dupitem = this.IssueItems.GetItemByPublishTime(
                    strPublishTime,
                    null);
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的出版时间 '" + strPublishTime + "' 和本种中未提交之一删除出版时间相重。请先行提交已有之修改，再进行期记到。";
                    else
                        strText = "拟新增的出版时间 '" + strPublishTime + "' 在本种中已经存在。";

                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
        strText + "\r\n\r\n要立即对已存在出版时间进行修改吗？",
        "EntityForm",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);

                    // 转为修改
                    if (result == DialogResult.Yes)
                    {
                        ModifyIssue(dupitem);
                        return;
                    }

                    // 突出显示，以便操作人员观察这条已经存在的记录
                    dupitem.HilightListViewItem(true);
                    return;
                }

                // 对(本种)所有期记录进行出版时间查重
                if (true)
                {
                    string strIssueText = "";
                    string strBiblioText = "";
                    nRet = SearchIssuePublishTime(strPublishTime,
                        this.BiblioRecPath,
                        out strIssueText,
                        out strBiblioText,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(ForegroundWindow.Instance, "对出版时间 '" + strPublishTime + "' 进行查重的过程中发生错误: " + strError);
                    else if (nRet == 1) // 发生重复
                    {
                        IssuePublishTimeFoundDupDlg dlg = new IssuePublishTimeFoundDupDlg();
                        MainForm.SetControlFont(dlg, this.Font, false);
                        dlg.MainForm = Program.MainForm;
                        dlg.BiblioText = strBiblioText;
                        dlg.IssueText = strIssueText;
                        dlg.MessageText = "拟新增的出版时间 '" + strPublishTime + "' 在数据库中发现已经存在。因此无法新增。";
                        dlg.ShowDialog(this);
                        return;
                    }
                }

            } // end of ' if (String.IsNullOrEmpty(strPublishTime) == false)
#endif

            IssueItem issueitem = new IssueItem();

            // 设置缺省值
            nRet = SetItemDefaultValues(
                "issue_normalRegister_default",
                true,
                issueitem,
                out strError);
            if (nRet == -1)
            {
                strError = "设置缺省值的时候发生错误: " + strError;
                goto ERROR1;
            }

#if NO
            issueitem.PublishTime = strPublishTime;
#endif
            issueitem.Parent = Global.GetRecordID(this.BiblioRecPath);


            // 先加入列表
            this.Items.Add(issueitem);
            issueitem.ItemDisplayState = ItemDisplayState.New;
            issueitem.AddToListView(this.listView);
            issueitem.HilightListViewItem(true);

            issueitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对

            IssueEditForm edit = new IssueEditForm();

            edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15
            edit.Text = "新增期";
            // edit.MainForm = Program.MainForm;
            nRet = edit.InitialForEdit(issueitem,
                this.Items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            //REDO:
            Program.MainForm.AppInfo.LinkFormState(edit, "IssueEditForm_state");
            edit.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(edit);

            if (edit.DialogResult != DialogResult.OK
                && edit.Item == issueitem    // 表明尚未前后移动，或者移动回到起点，然后Cancel
                )
            {
                this.Items.PhysicalDeleteItem(issueitem);

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

            // 2017/3/2
            if (string.IsNullOrEmpty(issueitem.RefID))
            {
                issueitem.RefID = Guid.NewGuid().ToString();
            }

            // 要对本种进行出版日期和参考ID查重。
            // 如果重了，要保持窗口，以便修改。不过从这个角度，查重最好在对话框关闭前作？
            // 或者重新打开对话框
            string strPublishTime = issueitem.PublishTime;
            if (String.IsNullOrEmpty(strPublishTime) == false)
            {

                // 需要排除掉刚加入的自己: issueitem。
                List<IssueItem> excludeItems = new List<IssueItem>();
                excludeItems.Add(issueitem);

                // 对当前窗口内进行出版时间查重
                IssueItem dupitem = this.Items.GetItemByPublishTime(
                    strPublishTime,
                    excludeItems);
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的出版时间 '" + strPublishTime + "' 和本种中未提交之一删除出版时间相重。请先行提交已有之修改，再进行期记到。";
                    else
                        strText = "拟新增的出版时间 '" + strPublishTime + "' 在本种中已经存在。";

                    // 警告尚未保存
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
        strText + "\r\n\r\n要立即对新记录的出版时间进行修改吗？\r\n(Yes 进行修改; No 不修改，让发生重复的新记录进入列表; Cancel 放弃刚刚创建的新记录)",
        "EntityForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);

                    // 转为修改
                    if (result == DialogResult.Yes)
                    {
                        ModifyIssue(issueitem);
                        return;
                    }

                    // 放弃刚刚创建的记录
                    if (result == DialogResult.Cancel)
                    {
                        this.Items.PhysicalDeleteItem(issueitem);

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
            } // end of ' if (String.IsNullOrEmpty(strPublishTime) == false)

            return;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

        // 以后需要用到的时候才修改参数
#if NO
        // 检索期参考ID。用于新参考ID的查重。
        int SearchIssueRefID(string strRefID,
            string strBiblioRecPath,
            out string strIssueText,
            out string strBiblioText,
            out string strError)
        {
            strError = "";
            strIssueText = "";
            strBiblioText = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                byte[] issue_timestamp = null;
                string strIssueRecPath = "";
                string strOutputBiblioRecPath = "";

                long lRet = Channel.GetIssueInfo(
                    stop,
                    strRefID,
                    strBiblioRecPath,
                    "html",
                    out strIssueText,
                    out strIssueRecPath,
                    out issue_timestamp,
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

        // 参考ID查重。用于(可能是)旧参考ID查重。
        // 本函数可以自动排除和当前路径strOriginRecPath重复之情形
        // parameters:
        //      strRefID  参考ID。
        //      strOriginRecPath    出发记录的路径。
        //      paths   所有命中的路径
        // return:
        //      -1  error
        //      0   not dup
        //      1   dup
        int SearchIssueRefIdDup(
            Stop stop,  // 2022/11/1
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
            stop?.Initial("正在对参考ID '" + strRefID + "' 进行查重 ...");

            try
            {
                /*
                long lRet = Channel.SearchIssueDup(
                    stop,
                    strRefID,
                    strBiblioRecPath,
                    100,
                    out paths,
                    out strError);
                 * */
                long lRet = channel.SearchIssue(
    stop,
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

                lRet = channel.GetSearchResult(stop,
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

                if (lHitCount == 1)
                {
                    // 检索命中一条。看看路径是否和出发记录一样
                    if (paths.Length != 1)
                    {
                        strError = "系统错误: SearchIssue() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
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
                stop?.Initial("");
            }

            return 1;   // found
        }

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

#if NO
        // 为IssueItem对象设置缺省值
        // parameters:
        //      strCfgEntry 为"issue_normalRegister_default"或"issue_quickRegister_default"
        int SetIssueItemDefaultValues(
            string strCfgEntry,
            IssueItem issueitem,
            out string strError)
        {
            strError = "";

            string strNewDefault = Program.MainForm.AppInfo.GetString(
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

            int nRet = issueitem.SetData("",
                strNewDefault,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            issueitem.Parent = "";
            issueitem.RecPath = "";

            return 0;
        }
#endif

        void ModifyIssue(IssueItem issueitem)
        {
            Debug.Assert(issueitem != null, "");

            bool bOldChanged = this.Items.Changed;

            string strOldRefID = issueitem.RefID;
            string strOldPublishTime = issueitem.PublishTime;

            IssueEditForm edit = new IssueEditForm();

            edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15
            // edit.MainForm = Program.MainForm;
            edit.ItemControl = this;
            string strError = "";
            int nRet = edit.InitialForEdit(issueitem,
                this.Items,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, strError);
                return;
            }
            edit.StartItem = null;  // 清除原始对象标记

        REDO:
            Program.MainForm.AppInfo.LinkFormState(edit, "IssueEditForm_state");
            edit.ShowDialog(this);
            Program.MainForm.AppInfo.UnlinkFormState(edit);

            if (edit.DialogResult != DialogResult.OK)
                return;
#if NO
            // IssueItem对象已经被修改
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = true;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, true);

            LibraryChannel channel = Program.MainForm.GetChannel();
            var looping = BeginLoop((s, e) => Program.MainForm.DoStop(s, e),
                "...");

            this.EnableControls(false);
            try
            {
                // 对出版日期查重
                if (strOldPublishTime != issueitem.PublishTime) // 出版日期改变了的情况下才查重
                {
                    if (string.IsNullOrEmpty(issueitem.PublishTime) == true)
                    {
                        MessageBox.Show(ForegroundWindow.Instance, "出版日期不能为空。按“确定”按钮重新输入。");
                        goto REDO;
                    }

                    // 需要排除掉自己: issueitem。
                    List<IssueItem> excludeItems = new List<IssueItem>();
                    excludeItems.Add(issueitem);

                    // 对当前窗口内进行参考ID查重
                    IssueItem dupitem = this.Items.GetItemByPublishTime(
                        issueitem.PublishTime,
                        excludeItems);
                    if (dupitem != null)
                    {
                        string strText = "";
                        if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                            strText = "出版日期 '" + issueitem.RefID + "' 和本种中未提交之一删除出版日期相重。按“确定”按钮重新输入，或退出对话框后先行提交已有之修改。";
                        else
                            strText = "出版日期 '" + issueitem.RefID + "' 在本种中已经存在。按“确定”按钮重新输入。";

                        MessageBox.Show(ForegroundWindow.Instance, strText);
                        goto REDO;
                    }

                    // 注：出版日期的查重只对本种类期记录有意义。不同的种下属的期记录，出版日期是允许重复的
                }

                // 对参考ID进行查重
                if (string.IsNullOrEmpty(issueitem.RefID) == false)
                {
                    // 需要排除掉自己: issueitem。
                    List<BookItemBase> excludeItems = new List<BookItemBase>();
                    excludeItems.Add(issueitem);

                    // 对当前窗口内进行参考ID查重
                    IssueItem dupitem = this.Items.GetItemByRefID(
                        issueitem.RefID,
                        excludeItems) as IssueItem;
                    if (dupitem != null)
                    {
                        string strText = "";
                        if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                            strText = "参考ID '" + issueitem.RefID + "' 和本种中未提交之一删除参考ID相重。按“确定”按钮重新输入，或退出对话框后先行提交已有之修改。";
                        else
                            strText = "参考ID '" + issueitem.RefID + "' 在本种中已经存在。按“确定”按钮重新输入。";

                        MessageBox.Show(ForegroundWindow.Instance, strText);
                        goto REDO;
                    }

                    // 对所有期记录进行参考ID查重
                    if (edit.AutoSearchDup == true)
                    {
                        // Debug.Assert(false, "");

                        // 参考ID查重。
                        // parameters:
                        //      strOriginRecPath    出发记录的路径。
                        //      paths   所有命中的路径
                        // return:
                        //      -1  error
                        //      0   not dup
                        //      1   dup
                        nRet = SearchIssueRefIdDup(
                            looping.Progress,
                            channel,
                            issueitem.RefID,
                            // this.BiblioRecPath,
                            issueitem.RecPath,
                            out string[] paths,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, "对参考ID '" + issueitem.RefID + "' 进行查重的过程中发生错误: " + strError);

                        else if (nRet == 1) // 发生重复
                        {
                            string pathlist = String.Join(",", paths);

                            string strText = "参考ID '" + issueitem.RefID + "' 在数据库中发现已经被(属于其他种的)下列期记录所使用。\r\n" + pathlist + "\r\n\r\n按“确定”按钮重新编辑期信息，或者根据提示的期记录路径，去修改其他期记录信息。";
                            MessageBox.Show(ForegroundWindow.Instance, strText);
                            goto REDO;
                        }
                    }
                }

                // 2017/3/2
                if (string.IsNullOrEmpty(issueitem.RefID))
                {
                    issueitem.RefID = Guid.NewGuid().ToString();
                }
            }
            finally
            {
                this.EnableControls(true);
                looping.Dispose();
                Program.MainForm.ReturnChannel(channel);
            }
        }

#if NO
        // 分批进行保存
        // return:
        //      -2  已经警告(部分成功，部分失败)
        //      -1  出错
        //      0   保存成功，没有错误和警告
        int SaveIssues(EntityInfo[] issues,
            out string strError)
        {
            strError = "";

            bool bWarning = false;
            EntityInfo[] errorinfos = null;

            int nBatch = 100;
            for (int i = 0; i < (issues.Length / nBatch) + ((issues.Length % nBatch) != 0 ? 1 : 0); i++)
            {
                int nCurrentCount = Math.Min(nBatch, issues.Length - i * nBatch);
                EntityInfo[] current = EntityControl.GetPart(issues, i * nBatch, nCurrentCount);

                int nRet = SaveIssueRecords(this.BiblioRecPath,
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

        // 提交期保存请求
        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        public int DoSaveIssues()
        {
            // 2008/9/17
            if (this.Items == null)
                return 0;

            EnableControls(false);
            try
            {
                string strError = "";
                int nRet = 0;

                if (this.Items == null)
                {
                    /*
                    strError = "没有期信息需要保存";
                    goto ERROR1;
                     * */
                    return 0;
                }

                // 检查全部事项的Parent值是否适合保存
                // return:
                //      -1  有错误，不适合保存
                //      0   没有错误
                nRet = this.Items.CheckParentIDForSave(out strError);
                if (nRet == -1)
                {
                    strError = "保存期信息失败，原因：" + strError;
                    goto ERROR1;
                }

                EntityInfo[] issues = null;
                // EntityInfo[] errorinfos = null;

                // 构造需要提交的期信息数组
                nRet = BuildSaveIssues(
                    out issues,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (issues == null || issues.Length == 0)
                    return 0; // 没有必要保存

#if NO
                nRet = SaveIssueRecords(this.BiblioRecPath,
                    issues,
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
                nRet = SaveIssues(issues, out strError);
                if (nRet == -2)
                    return -1;  // SaveIssues()已经MessageBox()显示过了
                if (nRet == -1)
                    goto ERROR1;

                this.Changed = false;
                Program.MainForm.StatusBarMessage = "期信息 提交 / 保存 成功";
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

        // 构造用于保存的期信息数组
        int BuildSaveIssues(
            out EntityInfo[] issues,
            out string strError)
        {
            strError = "";
            issues = null;
            int nRet = 0;

            Debug.Assert(this.Items != null, "");

            List<EntityInfo> issueArray = new List<EntityInfo>();

            foreach (IssueItem issueitem in this.Items)
            {
                // IssueItem issueitem = this.IssueItems[i];

                if (issueitem.ItemDisplayState == ItemDisplayState.Normal)
                    continue;

                EntityInfo info = new EntityInfo();

                // 2010/2/27 add
                if (String.IsNullOrEmpty(issueitem.RefID) == true)
                {
                    issueitem.RefID = Guid.NewGuid().ToString();
                    issueitem.RefreshListView();
                }

                info.RefID = issueitem.RefID;  // 2008/2/17

                string strXml = "";
                nRet = issueitem.BuildRecord(out strXml,
                        out strError);
                if (nRet == -1)
                    return -1;

                if (issueitem.ItemDisplayState == ItemDisplayState.New)
                {
                    info.Action = "new";
                    info.NewRecPath = "";
                    info.NewRecord = strXml;
                    info.NewTimestamp = null;
                }

                if (issueitem.ItemDisplayState == ItemDisplayState.Changed)
                {
                    info.Action = "change";

                    Debug.Assert(String.IsNullOrEmpty(issueitem.RecPath) == false, "issueitem.RecPath 不能为空");

                    info.OldRecPath = issueitem.RecPath; // 2007/6/2
                    info.NewRecPath = issueitem.RecPath;

                    info.NewRecord = strXml;
                    info.NewTimestamp = null;

                    info.OldRecord = issueitem.OldRecord;
                    info.OldTimestamp = issueitem.Timestamp;
                }

                if (issueitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    info.Action = "delete";
                    info.OldRecPath = issueitem.RecPath; // NewRecPath

                    info.NewRecord = "";
                    info.NewTimestamp = null;

                    info.OldRecord = issueitem.OldRecord;
                    info.OldTimestamp = issueitem.Timestamp;
                }

                issueArray.Add(info);
            }

            // 复制到目标
            issues = new EntityInfo[issueArray.Count];
            for (int i = 0; i < issueArray.Count; i++)
            {
                issues[i] = issueArray[i];
            }

            return 0;
        }

#endif

#if NO
        // 保存期记录
        // 不负责刷新界面和报错
        int SaveIssueRecords(string strBiblioRecPath,
            EntityInfo[] issues,
            out EntityInfo[] errorinfos,
            out string strError)
        {
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在保存期信息 ...");
            Stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetIssues(
                    Stop,
                    strBiblioRecPath,
                    issues,
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

#endif

#if NO
        // 把报错信息中的成功事项的状态修改兑现
        // 并且彻底去除没有报错的“删除”IssueItem事项（内存和视觉上）
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
                /*
                XmlDocument dom = new XmlDocument();

                string strNewXml = errorinfos[i].NewRecord;
                string strOldXml = errorinfos[i].OldRecord;

                if (String.IsNullOrEmpty(strNewXml) == false)
                {
                    dom.LoadXml(strNewXml);
                }
                else if (String.IsNullOrEmpty(strOldXml) == false)
                {
                    dom.LoadXml(strOldXml);
                }
                else
                {
                    // 找不到出版日期来定位
                    Debug.Assert(false, "找不到定位的出版日期");
                    // 是否单独显示出来?
                    continue;
                }
                 * */

                IssueItem issueitem = null;

                string strError = "";

                if (String.IsNullOrEmpty(errorinfos[i].RefID) == true)
                {
                    MessageBox.Show(ForegroundWindow.Instance, "服务器返回的EntityInfo结构中RefID为空");
                    return true;
                }

                /*
                string strPublishTime = "";
                // 在listview中定位和dom关联的事项
                // 顺次根据 记录路径 -- 出版时间 来定位
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = LocateIssueItem(
                    errorinfos[i].OldRecPath,   // 原来是NewRecPath
                    dom,
                    out issueitem,
                    out strPublishTime,
                    out strError);
                 * */
                nRet = LocateIssueItem(
                    errorinfos[i].RefID,
                    OrderControl.GetOneRecPath(errorinfos[i].NewRecPath, errorinfos[i].OldRecPath),
                    out issueitem,
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
                    issueitem.PublishTime,  // strPublishTime, 
                    errorinfos[i].NewRecPath);

                // 正常信息处理
                if (errorinfos[i].ErrorCode == ErrorCodeValue.NoError)
                {
                    if (errorinfos[i].Action == "new")
                    {
                        issueitem.OldRecord = errorinfos[i].NewRecord;
                        nRet = issueitem.ResetData(
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
                        issueitem.OldRecord = errorinfos[i].NewRecord;

                        nRet = issueitem.ResetData(
                            errorinfos[i].NewRecPath,
                            errorinfos[i].NewRecord,
                            errorinfos[i].NewTimestamp,
                            out strError);
                        if (nRet == -1)
                            MessageBox.Show(ForegroundWindow.Instance, strError);

                        issueitem.ItemDisplayState = ItemDisplayState.Normal;
                    }

                    // 对于保存后变得不再属于本种的，要在listview中消除
                    if (String.IsNullOrEmpty(issueitem.RecPath) == false)
                    {
                        string strTempItemDbName = Global.GetDbName(issueitem.RecPath);
                        string strTempBiblioDbName = Program.MainForm.GetBiblioDbNameFromIssueDbName(strTempItemDbName);

                        Debug.Assert(String.IsNullOrEmpty(strTempBiblioDbName) == false, "");
                        // TODO: 这里要正规报错

                        string strTempBiblioRecPath = strTempBiblioDbName + "/" + issueitem.Parent;

                        if (strTempBiblioRecPath != this.BiblioRecPath)
                        {
                            this.Items.PhysicalDeleteItem(issueitem);
                            continue;
                        }
                    }

                    issueitem.Error = null;   // 还是显示 空?

                    issueitem.Changed = false;
                    issueitem.RefreshListView();
                    continue;
                }

                // 报错处理
                issueitem.Error = errorinfos[i];
                issueitem.RefreshListView();

                strWarning += strLocationSummary + "在提交期保存过程中发生错误 -- " + errorinfos[i].ErrorInfo + "\r\n";
            }


            // 最后把没有报错的，那些成功删除事项，都从内存和视觉上抹除
            for (int i = 0; i < this.Items.Count; i++)
            {
                IssueItem issueitem = this.Items[i] as IssueItem;
                if (issueitem.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    if (issueitem.ErrorInfo == "")
                    {
                        this.Items.PhysicalDeleteItem(issueitem);
                        i--; 
                    }
                }
            }

#if NO
            // 修改Changed状态
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Items.Changed;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, this.Items.Changed);

            // 
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strWarning += "\r\n请注意修改期信息后重新提交保存";
                MessageBox.Show(ForegroundWindow.Instance, strWarning);
                return true;
            }

            return false;
        }

#endif

#if NO
        // 构造事项称呼
        static string GetLocationSummary(
            string strPublishTime,
            string strRecPath)
        {
            if (String.IsNullOrEmpty(strPublishTime) == false)
                return "出版时间为 '" + strPublishTime + "' 的事项";
            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            return "无任何定位信息的事项";
        }
#endif

        // 构造事项称呼
        internal override string GetLocationSummary(IssueItem bookitem)
        {
            string strPublishTime = bookitem.PublishTime;
            if (String.IsNullOrEmpty(strPublishTime) == false)
                return "出版时间为 '" + strPublishTime + "' 的事项";

            string strRecPath = bookitem.RecPath;

            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            string strRefID = bookitem.RefID;
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";

            return "无任何定位信息的事项";
        }

#if NO
        // 在this.issueitems中定位和strRefID关联的事项
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateIssueItem(
            string strRefID,
            string strRecPath,
            out IssueItem issueitem,
            out string strError)
        {
            strError = "";

            // 优先用记录路径来定位
            if (string.IsNullOrEmpty(strRecPath) == false
                && Global.IsAppendRecPath(strRecPath) == false)
            {
                issueitem = this.Items.GetItemByRecPath(strRecPath) as IssueItem;
                if (issueitem != null)
                    return 1;   // found
            }

            // 然后用参考ID来定位
            issueitem = this.Items.GetItemByRefID(strRefID, null) as IssueItem;

            if (issueitem != null)
                return 1;   // found

            strError = "没有找到 记录路径为 '" + strRecPath + "'，并且 参考ID 为 '" + strRefID + "' 的IssueItem事项";
            return 0;
        }
#endif

#if NOOOOOOOOOOOOOOOOO
        // 在this.issueitems中定位和dom关联的事项
        // 顺次根据 记录路径 -- 出版时间 来定位
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int LocateIssueItem(
            string strRecPath,
            XmlDocument dom,
            out IssueItem issueitem,
            out string strPublishTime,
            out string strError)
        {
            strError = "";
            issueitem = null;
            strPublishTime = "";

            // 提前获取, 以便任何返回路径时, 都可以得到这些值
            strPublishTime = DomUtil.GetElementText(dom.DocumentElement, 
                "publishTime");

            if (String.IsNullOrEmpty(strRecPath) == false)
            {
                issueitem = this.issueitems.GetItemByRecPath(strRecPath);

                if (issueitem != null)
                    return 1;   // found

            }

            if (String.IsNullOrEmpty(strPublishTime) == false)
            {
                issueitem = this.issueitems.GetItemByPublishTime(
                    strPublishTime,
                    null);
                if (issueitem != null)
                    return 1;   // found

            }

            return 0;
        }
#endif

        private void ListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool bHasBiblioLoaded = false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
                bHasBiblioLoaded = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("记到(&A)");
            menuItem.Click += new System.EventHandler(this.menu_manageIssue_Click);
            if (bHasBiblioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyIssue_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newIssue_Click);
            if (bHasBiblioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装订(&B)");
            menuItem.Click += new System.EventHandler(this.menu_binding_Click);
            if (bHasBiblioLoaded == false)  // 为什么?
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("验收结束时立即输入册条码号(&I)");
            menuItem.Click += new System.EventHandler(this.menu_toggleInputItemsBarcode_Click);
            if (this.InputItemsBarcode == true)
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("为新验收的册设置“加工中”状态(&P)");
            menuItem.Click += new System.EventHandler(this.menu_toggleSetProcessingState_Click);
            if (AcceptForm.SetProcessingState == true)
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("为新验收的册创建索取号(&C)");
            menuItem.Click += new System.EventHandler(this.menu_toggleAutoCreateCallNumber_Click);
            if (this.CreateCallNumber == true)
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入新开的期窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToNewItemForm_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入已经打开的期窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToExistItemForm_Click);
            if (this.listView.SelectedItems.Count == 0
                || Program.MainForm.GetTopChildWindow<ItemInfoForm>() == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("察看期记录的检索点 (&K)");
            menuItem.Click += new System.EventHandler(this.menu_getKeys_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            /*

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // cut 剪切
            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // copy 复制
            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyEntity_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(ClipboardBookItemCollection)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            // paste 粘贴
            menuItem = new MenuItem("粘贴(&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteEntity_Click);
            if (bHasClipboardObject == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 改变归属
            menuItem = new MenuItem("改变归属(&B)");
            menuItem.Click += new System.EventHandler(this.menu_changeParent_Click);
            if (this.listView_items.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

             * */

            // 全选
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("标记删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteIssue_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("撤销删除(&U)");
            menuItem.Click += new System.EventHandler(this.menu_undoDeleteIssue_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView, new Point(e.X, e.Y));
        }


        // 全选
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView);
        }

        async void menu_loadToNewItemForm_Click(object sender, EventArgs e)
        {
            await LoadToItemInfoFormAsync(true);
        }

        async void menu_loadToExistItemForm_Click(object sender, EventArgs e)
        {
            await LoadToItemInfoFormAsync(false);
        }

        // 自动为新创建的册创建索取号
        void menu_toggleAutoCreateCallNumber_Click(object sender, EventArgs e)
        {
            if (this.CreateCallNumber == true)
                this.CreateCallNumber = false;
            else
                this.CreateCallNumber = true;
        }


        // 验收结束时立即输入册条码号
        void menu_toggleInputItemsBarcode_Click(object sender, EventArgs e)
        {
            if (this.InputItemsBarcode == true)
                this.InputItemsBarcode = false;
            else
                this.InputItemsBarcode = true;
        }

        void menu_toggleSetProcessingState_Click(object sender, EventArgs e)
        {
            if (AcceptForm.SetProcessingState == true)
                AcceptForm.SetProcessingState = false;
            else
                AcceptForm.SetProcessingState = true;
        }

        // 装订
        void menu_binding_Click(object sender, EventArgs e)
        {
            DoBinding("装订", "binding");
        }

        // 进行装订
        // parameters:
        //      strLayoutMode   "auto" "accepting" "binding"。auto为自动模式，accepting为全部行为记到，binding为全部行为装订
        void DoBinding(string strTitle,
            string strLayoutMode)
        {
            string strError = "";
            int nRet = 0;

            // 验收前的准备工作
            if (this.PrepareAccept != null)
            {
                PrepareAcceptEventArgs e = new PrepareAcceptEventArgs();
                e.SourceRecPath = this.BiblioRecPath;
                this.PrepareAccept(this, e);
                if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = e.ErrorInfo;
                    goto ERROR1;
                }

                if (e.Cancel == true)
                    return;

                this.TargetRecPath = e.TargetRecPath;
                this.AcceptBatchNo = e.AcceptBatchNo;
                this.InputItemsBarcode = e.InputItemsBarcode;
                // this.SetProcessingState = e.SetProcessingState;
                this.CreateCallNumber = e.CreateCallNumber;

                if (String.IsNullOrEmpty(e.WarningInfo) == false)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                        "警告: \r\n" + e.WarningInfo + "\r\n\r\n继续进行验收?",
                            "IssueControl",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
            }

            // 
            if (this.Items == null)
                this.Items = new IssueItemCollection();

            Debug.Assert(this.Items != null, "");
            bool bOldChanged = this.Items.Changed;
            //bool bChanged = false;

            try
            {
                using (BindingForm dlg = new BindingForm())
                {
                    dlg.Text = strTitle;
                    // dlg.MainForm = Program.MainForm;
                    dlg.AppInfo = Program.MainForm.AppInfo;
                    dlg.BiblioDbName = Global.GetDbName(this.BiblioRecPath);
                    if (this.PrepareAccept != null)
                    {
                        dlg.AcceptBatchNoInputed = true;
                        // dlg.AcceptBatchNo = this.AcceptBatchNo;
                        Program.MainForm.AppInfo.SetString(
                            "binding_form",
                            "accept_batchno",
                            this.AcceptBatchNo);
                    }

                    dlg.Operator = Program.MainForm.DefaultUserName;
#if NO
                if (this.Channel != null)
                    dlg.LibraryCodeList = this.Channel.LibraryCodeList;
#endif
                    if (Program.MainForm != null)
                        dlg.LibraryCodeList = Program.MainForm._currentLibraryCodeList;

                    dlg.SetProcessingState = AcceptForm.SetProcessingState;
                    /*
                    dlg.GetItemInfo -= new GetItemInfoEventHandler(dlg_GetItemInfo);
                    dlg.GetItemInfo += new GetItemInfoEventHandler(dlg_GetItemInfo);
                     * */

                    dlg.GetOrderInfo -= new GetOrderInfoEventHandler(dlg_GetOrderInfo);
                    dlg.GetOrderInfo += new GetOrderInfoEventHandler(dlg_GetOrderInfo);

                    dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
                    dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

                    dlg.GenerateData -= new GenerateDataEventHandler(dlg_GenerateData);
                    dlg.GenerateData += new GenerateDataEventHandler(dlg_GenerateData);

                    dlg.GetBiblio -= dlg_GetBiblio;
                    dlg.GetBiblio += dlg_GetBiblio;

                    dlg.GetMacroValue -= Dlg_GetMacroValue;
                    dlg.GetMacroValue += Dlg_GetMacroValue;
                    // TODO: 如果册listview中有标记删除的对象？要求先提交，才能进行装订

                    // 汇集全部册信息
                    List<String> ItemXmls = new List<string>();
                    List<string> all_item_refids = new List<string>();  // 调用对话框以前，全部册的refif数组
                    {
                        GetItemInfoEventArgs e = new GetItemInfoEventArgs();
                        e.BiblioRecPath = this.BiblioRecPath;
                        e.PublishTime = "*";
                        dlg_GetItemInfo(this, e);
                        for (int i = 0; i < e.ItemXmls.Count; i++)
                        {
                            XmlDocument dom = new XmlDocument();
                            try
                            {
                                dom.LoadXml(e.ItemXmls[i]);
                            }
                            catch (Exception ex)
                            {
                                strError = "XML装入DOM时出错: " + ex.Message;
                                goto ERROR1;
                            }
                            string strRefID = DomUtil.GetElementText(dom.DocumentElement,
                                "refID");
                            if (String.IsNullOrEmpty(strRefID) == false)
                            {
                                all_item_refids.Add(strRefID);
                                ItemXmls.Add(e.ItemXmls[i]);
                            }
                            else
                            {
                                Debug.Assert(false, "");
                            }
                        }
                        ItemXmls = e.ItemXmls;  // 直接用
                    }

                    // 汇集期信息
                    List<String> IssueXmls = new List<string>();
                    List<string> all_issue_refids = new List<string>();  // 调用对话框以前，全部期的refif数组
                    List<string> IssueObjectXmls = new List<string>();
                    foreach (IssueItem issue_item in this.Items)
                    {
                        // IssueItem issue_item = this.IssueItems[i];

                        if (issue_item.ItemDisplayState == ItemDisplayState.Deleted)
                        {
                            strError = "当前存在标记删除的期事项，必须先提交保存后，才能使用期管理功能";
                            goto ERROR1;
                        }

                        if (String.IsNullOrEmpty(issue_item.RefID) == true)
                        {
                            issue_item.RefID = Guid.NewGuid().ToString();
                            issue_item.Changed = true;
                            issue_item.RefreshListView();
                            Debug.Assert(String.IsNullOrEmpty(issue_item.RefID) == false, "");
                        }

                        string strIssueXml = "";
                        nRet = issue_item.BuildRecord(
                            true,   // 要检查 Parent 成员
                            out strIssueXml,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 给期记录 XML 中增加 recPath 元素
                        AddRecPath(ref strIssueXml, issue_item.RecPath);

                        IssueXmls.Add(strIssueXml);
                        IssueObjectXmls.Add(issue_item.ObjectXml);

                        Debug.Assert(String.IsNullOrEmpty(issue_item.RefID) == false, "");
                        all_issue_refids.Add(issue_item.RefID);
                    }

#if OLD_INITIAL

            // 将已有的的合订本册信息反映到对话框中
            {
                GetItemInfoEventArgs e = new GetItemInfoEventArgs();
                e.BiblioRecPath = this.BiblioRecPath;
                e.PublishTime = "<range>";
                dlg_GetItemInfo(this, e);
                for (int i = 0; i < e.ItemXmls.Count; i++)
                {
                    ItemBindingItem design_item =
                        dlg.AppendBindItem(e.ItemXmls[i],
                        out strError);
                    if (design_item == null)
                        goto ERROR1;
                }
            }

            List<string> issued_item_refids = dlg.AllIssueMembersRefIds;
            List<string> none_issued_refids = new List<string>();
            for (int i = 0; i < all_refids.Count; i++)
            {
                string strRefID = all_refids[i];
                if (String.IsNullOrEmpty(strRefID) == true)
                    continue;
                if (issued_item_refids.IndexOf(strRefID) == -1)
                {
                    none_issued_refids.Add(strRefID);
                }
            }

            if (none_issued_refids.Count > 0)
            {
                GetItemInfoEventArgs e = new GetItemInfoEventArgs();
                e.BiblioRecPath = this.BiblioRecPath;
                // refid字符串内不允许有逗号
                e.PublishTime = "refids:" + StringUtil.MakePathList(none_issued_refids);
                dlg_GetItemInfo(this, e);

                nRet = dlg.AppendNoneIssueSingleItems(e.ItemXmls, out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            nRet = dlg.Initial(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)  // 警告
                MessageBox.Show(this, strError);
#endif
                    dlg.LoadState();

                    nRet = dlg.NewInitial(
                        strLayoutMode,
                        ItemXmls,
                        IssueXmls,
                        IssueObjectXmls,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 1)  // 警告
                        MessageBox.Show(this, strError);

                    dlg.Changed = false;

                    Program.MainForm.AppInfo.LinkFormState(dlg,
                        "binding_form_state");
                    dlg.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(dlg);

                    if (dlg.DialogResult != DialogResult.OK)
                        return;

                    // *** 兑现对册的修改
                    {
                        // 先用全部refid装满，然后去掉那些还存在的，剩下的就是该删除的了
                        List<string> deleting_bind_refids = new List<string>();
                        deleting_bind_refids.AddRange(all_item_refids);

                        List<string> Xmls = new List<string>();
                        List<ItemBindingItem> allitems = dlg.AllItems;  // 加速
                                                                        // 遍历对象数组，兑现 修改/创建/删除 动作
                        for (int i = 0; i < allitems.Count; i++)
                        {
                            ItemBindingItem bind_item = allitems[i];

                            deleting_bind_refids.Remove(bind_item.RefID);

                            if (bind_item.Changed == true)
                            {
                                // 根据refid找到这个册对象，并兑现修改
                                // 如果没有找到，则创建之
                                Xmls.Add(bind_item.Xml);
                            }
                        }

                        if (this.ChangeItem != null)
                        {
                            string strWarning = "";
                            // 根据册XML数据，自动创建或者修改册对象
                            // return:
                            //      -1  error
                            //      0   没有修改
                            //      1   修改了
                            nRet = ChangeItems(Xmls,
                                out strWarning,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            //if (nRet == 1)
                            //    bChanged = true;

                            if (String.IsNullOrEmpty(strWarning) == false)
                                MessageBox.Show(this, strWarning);

                            // 删除实体数据
                            if (deleting_bind_refids.Count != 0)
                            {
                                nRet = DeleteItemRecords(deleting_bind_refids,
                                    out List<string> deleted_ids,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                                //if (deleted_ids.Count > 0)
                                //    bChanged = true;

                            }
                        }
                    }

                    // *** 兑现对期的修改
                    {
                        // 先用全部refid装满，然后去掉那些还存在的，剩下的就是该删除的了
                        List<string> deleting_issue_refids = new List<string>();
                        deleting_issue_refids.AddRange(all_issue_refids);

                        List<string> Xmls = new List<string>(); // 要创建或者修改的期记录
                        List<string> ObjectXmls = new List<string>();
                        // 遍历对象数组，创建deleting_issue_refids和Xmls
                        foreach (IssueBindingItem issue_item in dlg.Issues)
                        {
                            // IssueBindingItem issue_item = dlg.Issues[i];

                            if (issue_item.Virtual == true)
                                continue;

                            deleting_issue_refids.Remove(issue_item.RefID);

                            if (issue_item.Changed == true
                                || issue_item.ObjectChanged == true)
                            {
                                // 根据refid找到这个期对象，并兑现修改
                                // 如果没有找到，则创建之
                                Xmls.Add(issue_item.Xml);
                                ObjectXmls.Add(issue_item.ObjectXml);
                            }
                        }

                        // 根据册XML数据，自动创建或者修改期对象
                        // return:
                        //      -1  error
                        //      0   succeed
                        nRet = ChangeIssues(Xmls,
                            ObjectXmls,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;

                        // 删除期对象
                        if (deleting_issue_refids.Count != 0)
                        {
                            nRet = DeleteIssueRecords(deleting_issue_refids,
                                out List<string> deleted_ids,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            //if (deleted_ids.Count > 0)
                            //    bChanged = true;
                        }
                    }
                }
            }
            finally
            {
                //if (this.Items.Changed == true)
                //    bChanged = true;

#if NO
                if (this.ContentChanged != null
                    && bChanged == true)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = true;
                    this.ContentChanged(this, e1);
                }
#endif
                TriggerContentChanged(bOldChanged, true);
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void Dlg_GetMacroValue(object sender, GetMacroValueEventArgs e)
        {
            TriggerGetMacroValue(sender, e);
        }

        void dlg_GetBiblio(object sender, GetBiblioEventArgs e)
        {
            this.GetBiblio?.Invoke(sender, e);
        }

        // 给期记录 XML 中增加 recPath 元素
        // 这是期记录 XML 在记到控件中临时需要的元素，在提交保存到 dp2library 之前注意删除这个元素
        static void AddRecPath(ref string strIssueXml, string strRecPath)
        {
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strIssueXml);
            }
            catch
            {
                return;
            }
            DomUtil.SetElementText(dom.DocumentElement, _tempRecPathElementName, strRecPath);
            strIssueXml = dom.DocumentElement.OuterXml;
        }

        // 这是期记录 XML 在记到控件中临时需要的元素，在提交保存到 dp2library 之前注意删除这个元素
        internal const string _tempRecPathElementName = "recPath";

        void dlg_GenerateData(object sender, GenerateDataEventArgs e)
        {
#if NO
            if (this.GenerateData != null)
            {
                this.GenerateData(sender, e);
            }
            else
            {
                MessageBox.Show(this, "IssueControl没有挂接GenerateData事件");
            }
#endif
            DoGenerateData(sender, e);
        }

        // 根据册XML数据，创建或者修改册对象
        // return:
        //      -1  error
        //      0   没有修改
        //      1   修改了
        int ChangeItems(List<string> Xmls,
            out string strWarning,
            out string strError)
        {
            strError = "";
            strWarning = "";

            // int nRet = 0;
            bool bChanged = false;

            if (this.ChangeItem == null)
            {
                strError = "ChangeItem事件尚未挂接";
                return -1;
            }

            ChangeItemEventArgs data_container = new ChangeItemEventArgs();
            data_container.InputItemBarcode = this.InputItemsBarcode;
            data_container.CreateCallNumber = this.CreateCallNumber;
            data_container.SeriesMode = true;
            for (int i = 0; i < Xmls.Count; i++)
            {
                string strXml = Xmls[i];

                XmlDocument item_dom = new XmlDocument();
                try
                {
                    item_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "册记录 的XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                string strRefID = DomUtil.GetElementText(item_dom.DocumentElement,
                    "refID");

                ChangeItemData e = new ChangeItemData();

                e.Action = "neworchange";
                e.RefID = strRefID;
                e.Xml = strXml;

                data_container.DataList.Add(e);

                bChanged = true;
            } // end of for i

            if (data_container.DataList != null
                && data_container.DataList.Count > 0)
            {
                // 调用外部挂接的事件
                this.ChangeItem(this, data_container);
                string strErrorText = "";

                if (String.IsNullOrEmpty(data_container.ErrorInfo) == false)
                {
                    strError = data_container.ErrorInfo;
                    return -1;
                }

                for (int i = 0; i < data_container.DataList.Count; i++)
                {
                    ChangeItemData data = data_container.DataList[i];
                    if (String.IsNullOrEmpty(data.ErrorInfo) == false)
                    {
                        strErrorText += data.ErrorInfo;
                    }
                    if (String.IsNullOrEmpty(data.WarningInfo) == false)
                    {
                        strWarning += data.WarningInfo;
                    }
                }

                if (String.IsNullOrEmpty(strErrorText) == false)
                {
                    strError = strErrorText;
                    return -1;
                }

                if (String.IsNullOrEmpty(data_container.WarningInfo) == false)
                    strWarning += data_container.WarningInfo;
            }

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 
        // return:
        //      -1  出错
        //      0   没有移除的
        //      >0  移除的个数
        /// <summary>
        /// 移除publishtime重复的事项
        /// </summary>
        /// <param name="Xmls">XML 字符串集合。处理过程中会从中删除出版时间重复的那些字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有删除的; >0: 删除的的个数</returns>
        public int RemoveDupPublishTime(ref List<string> Xmls,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (this.Items == null)
                this.Items = new IssueItemCollection();

            int nRemovedCount = 0;
            for (int i = 0; i < Xmls.Count; i++)
            {
                string strXml = Xmls[i];

                XmlDocument issue_dom = new XmlDocument();
                try
                {
                    issue_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "期记录 的XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                string strPublishTime = DomUtil.GetElementText(issue_dom.DocumentElement,
                    "publishTime");
                if (String.IsNullOrEmpty(strPublishTime) == true)
                {
                    Debug.Assert(String.IsNullOrEmpty(strPublishTime) == false, "");
                    strError = "序号(从0开始计数)为 " + (i + nRemovedCount).ToString() + " 的期记录XML中没有<publishTime>元素...";
                    return -1;
                }

                // 看看是否有已经存在的记录
                IssueItem exist_item = this.Items.GetItemByPublishTime(strPublishTime, null);
                if (exist_item != null)
                {
                    Xmls.RemoveAt(i);
                    i--;
                    nRemovedCount++;

                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ",";
                    strError += strPublishTime;
                }


            } // end of for i

            return nRemovedCount;
        }

        // 
        // TODO: 循环中出错时，要继续做下去，最后再报错？
        // return:
        //      -1  error
        //      0   succeed
        /// <summary>
        /// 根据期 XML 数据，创建或者修改期对象
        /// </summary>
        /// <param name="Xmls">表示期记录的 XML 字符串集合</param>
        /// <param name="ObjectXmls">表示期记录下属对象的 XML 字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1: 出错</para>
        /// <para>0 : 成功</para>
        /// </returns>
        public int ChangeIssues(List<string> Xmls,
            List<string> ObjectXmls,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.Items == null)
                this.Items = new IssueItemCollection();

            for (int i = 0; i < Xmls.Count; i++)
            {
                string strXml = Xmls[i];
                string strObjectXml = "";

                if (ObjectXmls != null && i < ObjectXmls.Count)
                    strObjectXml = ObjectXmls[i];

                XmlDocument issue_dom = new XmlDocument();
                try
                {
                    issue_dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "期记录 的XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                string strRefID = DomUtil.GetElementText(issue_dom.DocumentElement,
                    "refID");
                if (String.IsNullOrEmpty(strRefID) == true)
                {
                    Debug.Assert(String.IsNullOrEmpty(strRefID) == false, "");
                    strError = "序号为 " + i.ToString() + " 的期记录XML中没有<refID>元素...";
                    return -1;
                }

                string strAction = "";

                // 看看是否有已经存在的记录
                IssueItem exist_item = this.Items.GetItemByRefID(strRefID, null) as IssueItem;
                if (exist_item != null)
                    strAction = "change";
                else
                    strAction = "new";

                /*
                string strOperName = "";
                if (strAction == "new")
                    strOperName = "新增";
                else if (strAction == "change")
                    strOperName = "修改";
                else if (strAction == "delete")
                    strOperName = "删除";
                 * */

                IssueItem issue_item = null;

                if (strAction == "new")
                {
                    issue_item = new IssueItem();

                    // 设置缺省值?
                }
                else
                    issue_item = exist_item;

                // 为了避免BuildRecord()报错
                issue_item.Parent = Global.GetRecordID(this.BiblioRecPath);

                if (exist_item == null)
                {
                    nRet = issue_item.SetData(issue_item.RecPath,
                        issue_dom.OuterXml,
                        null,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (String.IsNullOrEmpty(issue_item.Parent) == true)
                    {
                        // 新创建的记录可能没有.Parent内容，需要补上
                        Debug.Assert(String.IsNullOrEmpty(this.BiblioRecPath) == false, "");
                        string strID = Global.GetRecordID(this.BiblioRecPath);
                        issue_item.Parent = strID;
                    }
                }
                else
                {
                    // 注: OldRecord/Timestamp不希望被改变 2010/3/22
                    string strOldXml = issue_item.OldRecord;

#if DEBUG
                    if (issue_item.ItemDisplayState != ItemDisplayState.New)
                    {
                        Debug.Assert(String.IsNullOrEmpty(issue_item.RecPath) == false, "");
                    }
#endif

                    nRet = issue_item.SetData(issue_item.RecPath,
                        issue_dom.OuterXml,
                        issue_item.Timestamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    issue_item.OldRecord = strOldXml;
                }

                if (exist_item == null)
                {
                    // 2017/3/2
                    if (string.IsNullOrEmpty(issue_item.RefID))
                    {
                        issue_item.RefID = Guid.NewGuid().ToString();
                    }
                    this.Items.Add(issue_item);
                    issue_item.ItemDisplayState = ItemDisplayState.New;
                    issue_item.AddToListView(this.listView);
                }
                else
                {
                    // 新创建的行不能改为changed行
                    if (issue_item.ItemDisplayState != ItemDisplayState.New)
                        issue_item.ItemDisplayState = ItemDisplayState.Changed;
                }

                issue_item.ObjectXml = strObjectXml;

                issue_item.Changed = true;    // 否则“保存”按钮不能Enabled

                // 将刚刚加入的事项滚入可见范围
                issue_item.HilightListViewItem(true);
                issue_item.RefreshListView(); // 2009/12/18 add
            } // end of for i

            return 0;
        }

        int DeleteIssueRecords(List<string> deleting_issue_refids,
            out List<string> deleted_ids,
            out string strError)
        {
            deleted_ids = new List<string>();
            strError = "";
            int nRet = 0;

            if (this.Items == null)
                this.Items = new IssueItemCollection();

            for (int i = 0; i < deleting_issue_refids.Count; i++)
            {
                string strRefID = deleting_issue_refids[i];
                Debug.Assert(String.IsNullOrEmpty(strRefID) == false, "");

                // 看看是否有已经存在的记录
                IssueItem exist_item = this.Items.GetItemByRefID(strRefID, null) as IssueItem;
                if (exist_item == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                // 标记删除事项
                // return:
                //      0   因为有册信息，未能标记删除
                //      1   成功删除
                nRet = MaskDeleteItem(exist_item,
                         this.m_bRemoveDeletedItem);
                if (nRet == 0)
                {
                    strError = "refid为 '" + strRefID + "' 的期事项因为包含有册信息，无法进行删除";
                    return -1;
                }

                deleted_ids.Add(strRefID);

            } // end of for i

            return 0;
        }

        // 记到
        void menu_manageIssue_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                // DoIssueManage();
                DoBinding("记到", "accepting");
            }
            else
                DoBinding("记到", "auto");    // 
        }

#if NO
        void DoIssueManage()
        {
            string strError = "";
            int nRet = 0;

            // 验收前的准备工作
            if (this.PrepareAccept != null)
            {
                PrepareAcceptEventArgs e = new PrepareAcceptEventArgs();
                e.SourceRecPath = this.BiblioRecPath;
                this.PrepareAccept(this, e);
                if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = e.ErrorInfo;
                    goto ERROR1;
                }

                if (e.Cancel == true)
                    return;

                this.TargetRecPath = e.TargetRecPath;
                this.AcceptBatchNo = e.AcceptBatchNo;
                this.InputItemsBarcode = e.InputItemsBarcode;
                this.SetProcessingState = e.SetProcessingState;
                this.CreateCallNumber = e.CreateCallNumber;

                if (String.IsNullOrEmpty(e.WarningInfo) == false)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                        "警告: \r\n" + e.WarningInfo + "\r\n\r\n继续进行验收?",
                            "IssueControl",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
            }

            // 
            if (this.Items == null)
                this.Items = new IssueItemCollection();

            Debug.Assert(this.Items != null, "");

            IssueManageForm dlg = new IssueManageForm();
            // dlg.MainForm = Program.MainForm;
            // 2009/2/15
            dlg.BiblioDbName = Global.GetDbName(this.BiblioRecPath);

            // 将已有的期信息反映到对话框中
            foreach (IssueItem item in this.Items)
            {
                // IssueItem item = this.IssueItems[i];

                if (item.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    strError = "当前存在标记删除的期事项，必须先提交保存后，才能使用期管理功能";
                    goto ERROR1;
                }

                string strIssueXml = "";
                nRet = item.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strIssueXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                IssueManageItem design_item =
                    dlg.AppendNewItem(strIssueXml, out strError);
                if (design_item == null)
                    goto ERROR1;

                design_item.Tag = (object)item; // 建立连接关系
            }

            dlg.Changed = false;

            dlg.GetOrderInfo -= new GetOrderInfoEventHandler(dlg_GetOrderInfo);
            dlg.GetOrderInfo += new GetOrderInfoEventHandler(dlg_GetOrderInfo);

            dlg.GetItemInfo -= new GetItemInfoEventHandler(dlg_GetItemInfo);
            dlg.GetItemInfo += new GetItemInfoEventHandler(dlg_GetItemInfo);

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            /*
            dlg.GenerateEntity -= new GenerateEntityEventHandler(dlg_GenerateEntity);
            dlg.GenerateEntity += new GenerateEntityEventHandler(dlg_GenerateEntity);
             * */

            Program.MainForm.AppInfo.LinkFormState(dlg,
                "issue_manage_form_state");

            dlg.ShowDialog(this);

            Program.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            bool bOldChanged = this.Items.Changed;

            // 保存集合内的所有元素
            IssueItemCollection save_items = new IssueItemCollection();
            save_items.AddRange(this.Items);

            IssueItemCollection mask_delete_items = new IssueItemCollection();
            mask_delete_items.AddRange(this.Items);

            // 清除集合内的所有元素
            this.Items.Clear();

            List<IssueItem> changed_issueitems = new List<IssueItem>();

            List<IssueManageItem> items = dlg.Items;
            for (int i = 0; i < items.Count; i++)
            {
                IssueManageItem design_item = items[i];

                if (design_item.Changed == false)
                {
                    // 复原
                    IssueItem issue_item = (IssueItem)design_item.Tag;
                    Debug.Assert(issue_item != null, "");

                    // 2017/3/2
                    if (string.IsNullOrEmpty(issue_item.RefID))
                    {
                        issue_item.RefID = Guid.NewGuid().ToString();
                    }

                    this.Items.Add(issue_item);
                    issue_item.AddToListView(this.listView);

                    mask_delete_items.Remove(issue_item);
                    continue;
                }

                IssueItem issueitem = new IssueItem();

                // 对于全新创建的行
                if (design_item.Tag == null)
                {
                    // 促使将来以追加保存
                    issueitem.RecPath = "";

                    issueitem.ItemDisplayState = ItemDisplayState.New;
                }
                else
                {
                    // 复原recpath
                    IssueItem issue_item = (IssueItem)design_item.Tag;

                    // 复原一些必要的值
                    issueitem.RecPath = issue_item.RecPath;
                    issueitem.Timestamp = issue_item.Timestamp;
                    issueitem.OldRecord = issue_item.OldRecord;

                    // issueitem.ItemDisplayState = ItemDisplayState.Changed;

                    // 2009/1/6 changed
                    issueitem.ItemDisplayState = issue_item.ItemDisplayState;

                    if (issueitem.ItemDisplayState != ItemDisplayState.New)
                    {
                        // 注: 状态为New的不能修改为Changed，这是一个例外
                        issueitem.ItemDisplayState = ItemDisplayState.Changed;
                    }

                    mask_delete_items.Remove(issue_item);
                }

                issueitem.Parent = Global.GetRecordID(this.BiblioRecPath);

                issueitem.PublishTime = design_item.PublishTime;
                issueitem.Issue = design_item.Issue;
                issueitem.Volume = design_item.Volume;
                issueitem.Zong = design_item.Zong;
                issueitem.OrderInfo = design_item.OrderInfo;
                issueitem.RefID = design_item.RefID;    // 2010/2/27 add

                changed_issueitems.Add(issueitem);

                // 2017/3/2
                if (string.IsNullOrEmpty(issueitem.RefID))
                {
                    issueitem.RefID = Guid.NewGuid().ToString();
                }
                // 先加入列表
                this.Items.Add(issueitem);

                issueitem.AddToListView(this.listView);
                issueitem.HilightListViewItem(true);

                issueitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对
            }

            // 标记删除某些元素
            foreach (IssueItem issue_item in mask_delete_items)
            {
                // IssueItem issue_item = mask_delete_items[i];

                // 2009/2/10
                bool bFound = false;
                // 检查有没有出版日期重复、状态为新增的其他行?
                foreach (IssueItem temp in this.Items)
                {
                    // IssueItem temp = this.IssueItems[j];
                    if (issue_item.PublishTime == temp.PublishTime
                        && temp.ItemDisplayState == ItemDisplayState.New)
                    {
                        temp.ItemDisplayState = ItemDisplayState.Changed;
                        temp.Timestamp = issue_item.Timestamp;
                        temp.OldRecord = issue_item.OldRecord;
                        temp.RecPath = issue_item.RecPath;
                        temp.RefreshListView();
                        bFound = true;
                        break;
                    }
                }
                if (bFound == true)
                    continue;

                // 2017/3/2
                if (string.IsNullOrEmpty(issue_item.RefID))
                {
                    issue_item.RefID = Guid.NewGuid().ToString();
                }
                // 先加入列表
                this.Items.Add(issue_item);
                issue_item.AddToListView(this.listView);

                nRet = MaskDeleteItem(issue_item,
                        m_bRemoveDeletedItem);


            }

            if (this.GenerateEntity != null)
            {
                // 删除实体数据
                if (dlg.DeletingIds.Count != 0)
                {
                    List<string> deleted_ids = null;
                    nRet = DeleteItemRecords(dlg.DeletingIds,
                        out deleted_ids,
                        out strError);
                    if (nRet == -1)
                    {
                        this.Items.Clear();
                        this.Items.AddRange(save_items);
                        // 刷新显示
                        this.Items.AddToListView(this.listView);
                        goto ERROR1;
                    }
                }

                // 根据验收数据，自动创建实体数据
                nRet = GenerateEntities(changed_issueitems,
                    out strError);
                if (nRet == -1)
                {
                    // 放弃创建实体记录，或者实体记录创建失败后，应还原期记录的修改前状态
                    this.Items.Clear();
                    this.Items.AddRange(save_items);
                    // 刷新显示
                    this.Items.AddToListView(this.listView);
                    goto ERROR1;
                }
            }

#if NO
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = true;
                this.ContentChanged(this, e1);
            }
#endif
            TriggerContentChanged(bOldChanged, true);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif
        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = Program.MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(ForegroundWindow.Instance, strError);
            e.values = values;
        }

        /*
        void dlg_GenerateEntity(object sender, GenerateEntityEventArgs e)
        {
            if (this.GenerateEntity != null)
                this.GenerateEntity(sender, e);
        }*/

        // parameters:
        //      deleted_ids 已经成功删除的id
        int DeleteItemRecords(List<string> ids,
            out List<string> deleted_ids,
            out string strError)
        {
            strError = "";
            deleted_ids = new List<string>();

            Debug.Assert(this.GenerateEntity != null, "");

            GenerateEntityEventArgs data_container = new GenerateEntityEventArgs();
            // data_container.InputItemBarcode = this.InputItemsBarcode;
            data_container.SeriesMode = true;

            for (int i = 0; i < ids.Count; i++)
            {
                GenerateEntityData e = new GenerateEntityData();

                e.Action = "delete";
                e.RefID = ids[i];
                e.Xml = "";

                data_container.DataList.Add(e);
            }

            if (data_container.DataList != null
    && data_container.DataList.Count > 0)
            {
                // 调用外部挂接的事件
                this.GenerateEntity(this, data_container);
                string strErrorText = "";

                if (String.IsNullOrEmpty(data_container.ErrorInfo) == false)
                {
                    strError = data_container.ErrorInfo;
                    return -1;
                }

                for (int i = 0; i < data_container.DataList.Count; i++)
                {
                    GenerateEntityData data = data_container.DataList[i];
                    if (String.IsNullOrEmpty(data.ErrorInfo) == false)
                    {
                        strErrorText += data.ErrorInfo;
                    }
                    else
                        deleted_ids.Add(data.RefID);
                }

                if (String.IsNullOrEmpty(strErrorText) == false)
                {
                    strError = strErrorText;
                    return -1;
                }
            }

            return 0;
        }


        // 根据验收数据，自动创建实体数据
        // return:
        //      -1  error
        //      0   succeed
        int GenerateEntities(List<IssueItem> issueitems,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.GenerateEntity == null)
            {
                strError = "GenerateEntity事件尚未挂接";
                return -1;
            }

            GenerateEntityEventArgs data_container = new GenerateEntityEventArgs();
            data_container.InputItemBarcode = this.InputItemsBarcode;
            data_container.SetProcessingState = AcceptForm.SetProcessingState;
            data_container.CreateCallNumber = this.CreateCallNumber;
            data_container.SeriesMode = true;

            for (int i = 0; i < issueitems.Count; i++)
            {
                IssueItem issue_item = issueitems[i];

                if (String.IsNullOrEmpty(issue_item.OrderInfo) == true)
                    continue;

                string strIssueXml = "";
                nRet = issue_item.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strIssueXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                XmlDocument issue_dom = new XmlDocument();
                try
                {
                    issue_dom.LoadXml(strIssueXml);
                }
                catch (Exception ex)
                {
                    strError = "期记录 '" + issue_item.PublishTime + "' 的XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                bool bOrderChanged = false;

                // 针对一个期内每个订购记录的循环
                XmlNodeList order_nodes = issue_dom.DocumentElement.SelectNodes("orderInfo/*");
                for (int j = 0; j < order_nodes.Count; j++)
                {
                    XmlNode order_node = order_nodes[j];

                    string strDistribute = DomUtil.GetElementText(order_node, "distribute");

                    LocationCollection locations = new LocationCollection();
                    nRet = locations.Build(strDistribute,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    bool bLocationChanged = false;

                    // 为每个馆藏地点创建一个实体记录
                    for (int k = 0; k < locations.Count; k++)
                    {
                        Location location = locations[k];

                        // TODO: 要注意两点：1) 已经验收过的行，里面出现*的refid，是否要再次创建册？这样效果结识，反复用的时候有好处
                        // 2) 没有验收足的时候，是不是要按照验收足来循环了？检查一下

                        // 已经创建过的事项，跳过
                        if (location.RefID != "*")
                            continue;

                        GenerateEntityData e = new GenerateEntityData();

                        e.Action = "new";
                        e.RefID = Guid.NewGuid().ToString();
                        location.RefID = e.RefID;   // 修改到馆藏地点字符串中

                        bLocationChanged = true;

                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml("<root />");

                        // 2009/10/19
                        // 状态
                        if (AcceptForm.SetProcessingState == true)
                        {
                            // 增补“加工中”值
                            string strOldState = DomUtil.GetElementText(dom.DocumentElement,
                                "state");
                            DomUtil.SetElementText(dom.DocumentElement,
                                "state", Global.AddStateProcessing(strOldState));
                        }

                        // seller
                        string strSeller = DomUtil.GetElementText(order_node,
                            "seller");

                        // seller内是单纯值
                        DomUtil.SetElementText(dom.DocumentElement,
                            "seller", strSeller);

                        string strOldValue = "";
                        string strNewValue = "";

                        // source
                        string strSource = DomUtil.GetElementText(order_node,
                            "source");


                        // source内采用新值
                        // 分离 "old[new]" 内的两个值
                        dp2StringUtil.ParseOldNewValue(strSource,
                            out strOldValue,
                            out strNewValue);
                        DomUtil.SetElementText(dom.DocumentElement,
                            "source", strNewValue);

                        // price
                        string strPrice = DomUtil.GetElementText(order_node,
                            "price");

                        // price内采用新值
                        dp2StringUtil.ParseOldNewValue(strPrice,
                            out strOldValue,
                            out strNewValue);
                        DomUtil.SetElementText(dom.DocumentElement,
                            "price", strNewValue);

                        // location
                        string strLocation = location.Name;
                        DomUtil.SetElementText(dom.DocumentElement,
                            "location", strLocation);

                        // publishTime
                        DomUtil.SetElementText(dom.DocumentElement,
                            "publishTime", issue_item.PublishTime);

                        // volume 其实是当年期号、总期号、卷号在一起的一个字符串
                        string strVolume = VolumeInfo.BuildItemVolumeString(
                            dp2StringUtil.GetYearPart(issue_item.PublishTime),
                            issue_item.Issue,
                            issue_item.Zong,
                            issue_item.Volume);
                        DomUtil.SetElementText(dom.DocumentElement,
                            "volume", strVolume);

                        // 批次号
                        DomUtil.SetElementText(dom.DocumentElement,
                            "batchNo", this.AcceptBatchNo);

                        e.Xml = dom.OuterXml;

                        data_container.DataList.Add(e);
                    }

                    // 馆藏地点字符串有变化，需要反映给调主
                    if (bLocationChanged == true)
                    {
                        strDistribute = locations.ToString();
                        DomUtil.SetElementText(order_node,
                            "distribute", strDistribute);
                        bOrderChanged = true;
                        // order_item.RefreshListView();
                    }

                } // end of for j

                if (bOrderChanged == true)
                {
                    issue_item.OrderInfo = DomUtil.GetElementInnerXml(issue_dom.DocumentElement,
                        "orderInfo");
                    issue_item.Changed = true;
                    issue_item.RefreshListView();
                }

            } // end of for i

            if (data_container.DataList != null
                && data_container.DataList.Count > 0)
            {
                // 调用外部挂接的事件
                this.GenerateEntity(this, data_container);
                string strErrorText = "";

                if (String.IsNullOrEmpty(data_container.ErrorInfo) == false)
                {
                    strError = data_container.ErrorInfo;
                    return -1;
                }

                for (int i = 0; i < data_container.DataList.Count; i++)
                {
                    GenerateEntityData data = data_container.DataList[i];
                    if (String.IsNullOrEmpty(data.ErrorInfo) == false)
                    {
                        strErrorText += data.ErrorInfo;
                    }
                }

                if (String.IsNullOrEmpty(strErrorText) == false)
                {
                    strError = strErrorText;
                    return -1;
                }
            }

            return 0;
        }

        void dlg_GetItemInfo(object sender, GetItemInfoEventArgs e)
        {
            if (this.GetItemInfo != null)
                this.GetItemInfo(sender, e);
        }

        void dlg_GetOrderInfo(object sender, GetOrderInfoEventArgs e)
        {
            if (this.GetOrderInfo != null)
                this.GetOrderInfo(sender, e);
        }

        void menu_modifyIssue_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要编辑的事项");
                return;
            }
            IssueItem issueitem = (IssueItem)this.listView.SelectedItems[0].Tag;

            ModifyIssue(issueitem);
        }

        void menu_newIssue_Click(object sender, EventArgs e)
        {
            DoNewIssue();
        }


        // 撤销删除一个或多个期
        void menu_undoDeleteIssue_Click(object sender, EventArgs e)
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
                    IssueItem issueitem = (IssueItem)item.Tag;

                    bool bRet = this.Items.UndoMaskDeleteItem(issueitem);

                    if (bRet == false)
                    {
                        if (strNotUndoList != "")
                            strNotUndoList += ",";
                        strNotUndoList += issueitem.PublishTime;
                        continue;
                    }

                    nUndoCount++;
                }

                string strText = "";

                if (strNotUndoList != "")
                    strText += "出版时间为 '" + strNotUndoList + "' 的事项先前并未被标记删除过, 所以现在谈不上撤销删除。\r\n\r\n";

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
                TriggerContentChanged(bOldChanged, this.Items.Changed);
            }
            finally
            {
                this.EnableControls(true);
            }
        }

        // 删除一个或多个期
        void menu_deleteIssue_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要标记删除的事项");
                return;
            }

            string strPublishTimeList = "";
            for (int i = 0; i < this.listView.SelectedItems.Count; i++)
            {
                if (i > 20)
                {
                    strPublishTimeList += "...(共 " + this.listView.SelectedItems.Count.ToString() + " 项)";
                    break;
                }
                string strPublishTime = this.listView.SelectedItems[i].Text;
                strPublishTimeList += strPublishTime + "\r\n";
            }

            string strWarningText = "以下(出版日期)期将被标记删除: \r\n" + strPublishTimeList + "\r\n\r\n确实要标记删除它们?";

            // 警告
            DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                strWarningText,
                "EntityForm",
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
                    IssueItem issueitem = (IssueItem)item.Tag;

                    int nRet = MaskDeleteItem(issueitem,
                        m_bRemoveDeletedItem);

                    if (nRet == 0)
                    {
                        if (strNotDeleteList != "")
                            strNotDeleteList += ",";
                        strNotDeleteList += issueitem.PublishTime;
                        continue;
                    }

                    if (string.IsNullOrEmpty(issueitem.RecPath) == false)
                        deleted_recpaths.Add(issueitem.RecPath);

                    nDeleteCount++;
                }

                string strText = "";

                if (strNotDeleteList != "")
                    strText += "出版时间为 '" + strNotDeleteList + "' 的期包含有册信息, 未能加以标记删除。\r\n\r\n";

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

        // 
        // return:
        //      0   因为有册信息，未能标记删除
        //      1   成功删除
        /// <summary>
        /// 标记删除事项
        /// </summary>
        /// <param name="issueitem">事项</param>
        /// <param name="bRemoveDeletedItem">是否从 ListView 中移走事项显示</param>
        /// <returns>0: 因为某些原因，未能标记删除; 1: 成功删除</returns>
        public override int MaskDeleteItem(IssueItem issueitem,
            bool bRemoveDeletedItem)
        {
            // TODO:如何判断一个期有下属的册信息？
            // 或者说册信息中没有流通信息仍可以删除？
            /*
            if (String.IsNullOrEmpty(issueitem.Borrower) == false)
                return 0;
             * */

            this.Items.MaskDeleteItem(bRemoveDeletedItem,
                issueitem);

            this.Changed = this.Changed;    // 2022/11/17
            return 1;
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyIssue_Click(this, null);
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

        private void ListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ColumnSortStyle sortStyle = ColumnSortStyle.LeftAlign;

            // 2009/2/16
            // 第4/5/6列为号码数字，排序风格特殊
            if (nClickColumn == 3
                || nClickColumn == 4
                || nClickColumn == 5)
                sortStyle = ColumnSortStyle.RightAlign;
            else if (nClickColumn == 9)
                sortStyle = ColumnSortStyle.RecPath;

            this.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                this.listView.Columns,
                true);

            // 排序
            this.listView.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView.ListViewItemSorter = null;
        }

#if NO
        // 2010/4/27
        // 根据期记录路径 检索出 书目记录 和全部下属期记录，装入窗口
        // parameters:
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        public int DoSearchIssueByRecPath(string strIssueRecPath)
        {
            int nRet = 0;
            string strError = "";
            // 先检查是否已在本窗口中?

            // 对当前窗口内进行册记录路径查重
            if (this.Items != null)
            {
                IssueItem dupitem = this.Items.GetItemByRecPath(strIssueRecPath) as IssueItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "期记录 '" + strIssueRecPath + "' 正好为本种中未提交之一删除期请求。";
                    else
                        strText = "期刊记录 '" + strIssueRecPath + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";

            // 根据期记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchBiblioRecPath(strIssueRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对期记录路径 '" + strIssueRecPath + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到路径为 '" + strIssueRecPath + "' 的期记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上期事项
                IssueItem result_item = HilightLineByItemRecPath(strIssueRecPath, true);
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用期记录路径检索绝对不会发生重复现象");
            }

            return 0;
        }

#endif

#if NO
        // 根据期记录路径加亮事项
        public IssueItem HilightLineByItemRecPath(string strItemRecPath,
                bool bClearOtherSelection)
        {
            IssueItem issueitem = null;

            if (bClearOtherSelection == true)
            {
                this.ListView.SelectedItems.Clear();
            }

            if (this.Items != null)
            {
                issueitem = this.Items.GetItemByRecPath(strItemRecPath) as IssueItem;
                if (issueitem != null)
                    issueitem.HilightListViewItem(true);
            }

            return issueitem;
        }
#endif

#if NO
        // 根据期记录路径，检索出其从属的书目记录路径。
        int SearchBiblioRecPath(string strIssueRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索期记录 '" + strIssueRecPath + "' 所从属的书目记录路径 ...");
            Stop.BeginLoop();

            try
            {
                string strIndex = "@path:" + strIssueRecPath;
                string strOutputItemRecPath = "";

                long lRet = Channel.GetIssueInfo(
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
    }

    /// <summary>
    /// 获得宏的值
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetMacroValueHandler(object sender,
    GetMacroValueEventArgs e);

    /// <summary>
    /// GetMacroValueHandler的参数
    /// </summary>
    public class GetMacroValueEventArgs : EventArgs
    {
        /// <summary>
        /// 宏名
        /// </summary>
        public string MacroName = "";
        /// <summary>
        /// 宏的值
        /// </summary>
        public string MacroValue = "";
    }

    #region 从 IssueManageControl 移动过来

    /// <summary>
    /// 获得订购信息事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetOrderInfoEventHandler(object sender,
        GetOrderInfoEventArgs e);

    /// <summary>
    /// 获得订购信息事件的参数
    /// </summary>
    public class GetOrderInfoEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 书目记录路径
        /// </summary>
        public string BiblioRecPath = "";   // [in] 书目记录路径

        /// <summary>
        /// [in] 期的出版时间
        /// </summary>
        public string PublishTime = ""; // [in] 期的出版时间

        /// <summary>
        /// [in] 当前用户管辖的分馆代码列表。空表示全部管辖 
        /// </summary>
        public string LibraryCodeList = ""; // [in] 当前用户管辖的分馆代码列表。空表示全部管辖 

        /// <summary>
        /// [out] 符合条件的订购记录数组
        /// </summary>
        public List<string> OrderXmls = new List<string>(); // [out] 符合条件的订购记录数组

        /// <summary>
        /// [out] 出错信息。如果为空则表示没有任何错误
        /// </summary>
        public string ErrorInfo = "";   // [out] 出错信息。如果为空则表示没有任何错误
    }

    // 2009/10/12
    /// <summary>
    /// 获得册信息事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetItemInfoEventHandler(object sender,
        GetItemInfoEventArgs e);

    /// <summary>
    /// 获得册信息事件的参数
    /// </summary>
    public class GetItemInfoEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 书目记录路径
        /// </summary>
        public string BiblioRecPath = "";   // [in] 书目记录路径

        /// <summary>
        /// [in] 期的出版时间
        /// </summary>
        public string PublishTime = ""; // [in] 期的出版时间

        /// <summary>
        /// [out] 符合条件的册记录数组
        /// </summary>
        public List<string> ItemXmls = new List<string>(); // [out] 符合条件的册记录数组

        /// <summary>
        /// [out] 出错信息。如果为空则表示没有任何错误
        /// </summary>
        public string ErrorInfo = "";   // [out] 出错信息。如果为空则表示没有任何错误
    }



    #endregion

    // 如果不这样书写，视图设计器会出现故障
    /// <summary>
    /// IssueControl 类的基础类
    /// </summary>
    public class IssueControlBase : ItemControlBase<IssueItem, IssueItemCollection>
    {
    }


}
