using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 册记录列表控件
    /// </summary>
    public partial class EntityControl : EntityControlBase
    {

        /*
        // 创建索取号
        public event GenerateDataEventHandler GenerateAccessNo = null;
         * */

        /// <summary>
        /// 校验条码号
        /// </summary>
        public event VerifyBarcodeHandler VerifyBarcode = null;

        /// <summary>
        /// 获得参数值
        /// </summary>
        public event GetParameterValueHandler GetParameterValue = null;

        /// <summary>
        /// 是否要校验册条码号
        /// </summary>
        public bool NeedVerifyItemBarcode
        {
            get
            {
                if (this.GetParameterValue == null)
                    return false;

                GetParameterValueEventArgs e = new GetParameterValueEventArgs();
                e.Name = "NeedVerifyItemBarcode";
                this.GetParameterValue(this, e);

                return DomUtil.IsBooleanTrue(e.Value);
            }
        }

        // 
        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        /// <summary>
        /// 形式校验条码号
        /// </summary>
        /// <param name="strLibraryCode">册所在的图书馆代码</param>
        /// <param name="strBarcode">册条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>      -2  服务器没有配置校验方法，无法校验</para>
        /// <para>      -1  出错</para>
        /// <para>      0   不是合法的条码号</para>
        /// <para>      1   是合法的读者证条码号</para>
        /// <para>      2   是合法的册条码号</para>
        /// </returns>
        public int DoVerifyBarcode(
            string strLibraryCode,
            string strBarcode,
            out string strError)
        {
            if (this.VerifyBarcode == null)
            {
                strError = "尚未挂接VerifyBarcode事件";
                return -1;
            }

            VerifyBarcodeEventArgs e = new VerifyBarcodeEventArgs();
            e.Barcode = strBarcode;
            e.LibraryCode = strLibraryCode;
            this.VerifyBarcode(this, e);
            strError = e.ErrorInfo;
            return e.Result;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityControl()
        {
            InitializeComponent();

            this.m_listView = this.listView;
            this.ItemType = "item";
            this.ItemTypeName = "册";
        }

        // 
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        /// <summary>
        /// 获得一个书目记录下属的全部实体记录路径
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
        public static int GetEntityRecPaths(
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

                long lRet = channel.GetEntities(
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
                        strError = "路径为 '" + entities[i].OldRecPath + "' 的册记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
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
        // 装入实体记录
        // return:
        //      -1  出错
        //      0   没有装载
        //      1   已经装载
        public int LoadEntityRecords(string strBiblioRecPath,
            bool bDisplayOtherLibraryItem,
            out string strError)
        {
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在装入册信息 ...");
            Stop.BeginLoop();

            // this.Update();   // 优化
            // Program.MainForm.Update();


            try
            {
                // string strHtml = "";

                this.ClearEntities();

                long lPerCount = 100; // 每批获得多少个
                long lStart = 0;
                long lResultCount = 0;
                long lCount = -1;
                for (; ; )
                {

                    EntityInfo[] entities = null;

                    Thread.Sleep(500);

                    if (lCount > 0)
                        Stop.SetMessage("正在装入册信息 "+lStart.ToString()+"-"+(lStart+lCount-1).ToString()+" ...");

                    long lRet = Channel.GetEntities(
                        Stop,
                        strBiblioRecPath,
                        lStart,
                        lCount,
                        bDisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
                        "zh",
                        out entities,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;

                    lResultCount = lRet;

                    if (lRet == 0)
                        return 0;

                    Debug.Assert(entities != null, "");

                    this.ListView.BeginUpdate();
                    try
                    {
                        for (int i = 0; i < entities.Length; i++)
                        {
                            if (entities[i].ErrorCode != ErrorCodeValue.NoError)
                            {
                                strError = "路径为 '" + entities[i].OldRecPath + "' 的册记录装载中发生错误: " + entities[i].ErrorInfo;  // NewRecPath
                                return -1;
                            }

                            // 所返回的记录有可能是被过滤掉的
                            if (string.IsNullOrEmpty(entities[i].OldRecord) == true)
                                continue;

                            // 剖析一个册的xml记录，取出有关信息放入listview中
                            BookItem bookitem = new BookItem();

                            int nRet = bookitem.SetData(entities[i].OldRecPath, // NewRecPath
                                     entities[i].OldRecord,
                                     entities[i].OldTimestamp,
                                     out strError);
                            if (nRet == -1)
                                return -1;

                            if (entities[i].ErrorCode == ErrorCodeValue.NoError)
                                bookitem.Error = null;
                            else
                                bookitem.Error = entities[i];

                            this.BookItems.Add(bookitem);


                            bookitem.AddToListView(this.ListView);
                        }
                    }
                    finally
                    {
                        this.ListView.EndUpdate();
                    }

                    lStart += entities.Length;
                    if (lStart >= lResultCount)
                        break;

                    if (lCount == -1)
                        lCount = lPerCount;

                    if (lStart + lCount > lResultCount)
                        lCount = lResultCount - lStart;
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

        private void listView_items_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            bool bHasBillioLoaded = false;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == false)
                bHasBillioLoaded = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyEntity_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newEntity_Click);
            if (bHasBillioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增多个(&U)");
            menuItem.Click += new System.EventHandler(this.menu_newMultiEntity_Click);
            if (bHasBillioLoaded == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            ListViewHitTestInfo hittest_info = this.listView.HitTest(this.PointToClient(Control.MousePosition));
            string strColumnName = "";
            if (hittest_info.Item != null)
            {
                int x = hittest_info.Item.SubItems.IndexOf(hittest_info.SubItem);
                if (x >= 0)
                    strColumnName = this.listView.Columns[x].Text;
            }
            menuItem = new MenuItem("自动复制列 '" + strColumnName + "' (&V)");
            menuItem.Click += new System.EventHandler(this.menu_autoCopyColumn_Click);
            if (bHasBillioLoaded == false
                || hittest_info.Item == null
                || hittest_info.SubItem == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Tag = hittest_info;

            menuItem = new MenuItem("清除列 '" + strColumnName + "' (&C)");
            menuItem.Click += new System.EventHandler(this.menu_autoClearColumn_Click);
            if (bHasBillioLoaded == false
                || hittest_info.Item == null
                || hittest_info.SubItem == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
            menuItem.Tag = hittest_info;


            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // cut 剪切
            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cutEntity_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // copy 复制
            menuItem = new MenuItem("复制(&C) [" + this.listView.SelectedItems.Count.ToString() + "个]");
            menuItem.Click += new System.EventHandler(this.menu_copyEntity_Click);
            if (this.listView.SelectedItems.Count == 0)
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

            // 全选
            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("选定全部同一排架体系的事项(&R)");
            menuItem.Click += new System.EventHandler(this.menu_autoSelectCallNumber_Click);
            if (this.listView.SelectedItems.Count == 0)
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

            // 创建数据
            menuItem = new MenuItem("创建数据[Ctrl+A](&G)");
            menuItem.Click += new System.EventHandler(this.menu_generateData_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // 管理索取号
            menuItem = new MenuItem("管理索取号(&M)");
            menuItem.Click += new System.EventHandler(this.menu_manageCallNumber_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // 创建索取号
            menuItem = new MenuItem("创建索取号(&C)");
            menuItem.Click += new System.EventHandler(this.menu_createCallNumber_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入新开的册窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToNewItemForm_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("装入已经打开的册窗(&E)");
            menuItem.Click += new System.EventHandler(this.menu_loadToExistItemForm_Click);
            if (this.listView.SelectedItems.Count == 0
                || Program.MainForm.GetTopChildWindow<ItemInfoForm>() == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("察看册记录的检索点 (&K)");
            menuItem.Click += new System.EventHandler(this.menu_getKeys_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("标记删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteEntity_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("撤销删除(&U)");
            menuItem.Click += new System.EventHandler(this.menu_undoDeleteEntity_Click);
            if (this.listView.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView, new Point(e.X, e.Y));

        }

        void menu_loadToNewItemForm_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";

            if (this.ListView.SelectedItems.Count == 0)
            {
                strError = "尚未选定要操作的事项";
                goto ERROR1;
            }

            BookItem cur = (BookItem)this.ListView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "bookitem == null";
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
            form.MdiParent = Program.MainForm;
            form.MainForm = Program.MainForm;
            form.Show();

            form.DbType = "item";

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

            BookItem cur = (BookItem)this.ListView.SelectedItems[0].Tag;

            if (cur == null)
            {
                strError = "bookitem == null";
                goto ERROR1;
            }

            string strRecPath = cur.RecPath;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "所选定的事项记录路径为空，尚未在数据库中建立";
                goto ERROR1;
            }

            ItemInfoForm form = Program.MainForm.GetTopChildWindow<ItemInfoForm>();
            if (form == null)
            {
                strError = "当前并没有已经打开的册窗";
                goto ERROR1;
            }
            form.DbType = "item";
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

        // 自动清除列
        void menu_autoClearColumn_Click(object sender, EventArgs e)
        {
            // bool bOldChanged = this.Changed;

            MenuItem menu_item = (MenuItem)sender;

            ListViewHitTestInfo hittest_info = (ListViewHitTestInfo)menu_item.Tag;
            Debug.Assert(hittest_info.Item != null, "");
            int x = hittest_info.Item.SubItems.IndexOf(hittest_info.SubItem);
            Debug.Assert(x != -1, "");

            bool bChanged = false;
            foreach (ListViewItem item in this.listView.SelectedItems)
            {
                BookItem bookitem = (BookItem)item.Tag;

                string strTemp = ListViewUtil.GetItemText(item, x);
                if (String.IsNullOrEmpty(strTemp) == false)
                {
                    bookitem.SetColumnText(x, "");
                    bookitem.RefreshListView();
                    bChanged = true;
                }
            }
            if (bChanged == true)
            {
                this.Changed = bChanged;
            }
        }

        // 选定全部同一排架体系的事项
        void menu_autoSelectCallNumber_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.listView.SelectedItems.Count == 0)
            {
                strError = "至少要选定一个事项";
                goto ERROR1;
            }

            List<string> arrangement_names = new List<string>();
            foreach (ListViewItem item in this.listView.SelectedItems)
            {
                BookItem bookitem = (BookItem)item.Tag;

                // TODO: #reservation, 情况怎么处理？
                string strLocation = bookitem.Location;

                ArrangementInfo info = null;
                // 获得关于一个特定馆藏地点的索取号配置信息
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
            out info,
            out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0 || info == null)
                {
                    strError = "馆藏地点 '" + strLocation + "' 没有定义对应的排架体系";
                    goto ERROR1;
                }

                if (arrangement_names.IndexOf(info.ArrangeGroupName) == -1)
                    arrangement_names.Add(info.ArrangeGroupName);
            }

            Debug.Assert(arrangement_names.Count >= 1, "");

            if (arrangement_names.Count > 1)
            {
                strError = "您所选定的 " + this.listView.SelectedItems.Count.ToString() + " 个事项中，包含了多于一个的排架体系： " + StringUtil.MakePathList(arrangement_names) + "。请将选定的范围约束在仅包含一个排架体系，然后再使用本功能";
                goto ERROR1;
            }

            string strName = arrangement_names[0];
            foreach (ListViewItem item in this.listView.Items)
            {
                BookItem bookitem = (BookItem)item.Tag;

                // TODO: #reservation, 情况怎么处理？
                string strLocation = bookitem.Location;

                ArrangementInfo info = null;
                // 获得关于一个特定馆藏地点的索取号配置信息
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
            out info,
            out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0 || info == null)
                {
                    strError = "馆藏地点 '" + strLocation + "' 没有定义对应的排架体系";
                    goto ERROR1;
                }

                if (info.ArrangeGroupName == strName)
                {
                    item.Selected = true;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 自动复制列
        void menu_autoCopyColumn_Click(object sender, EventArgs e)
        {
            // bool bOldChanged = this.Changed;

            MenuItem menu_item = (MenuItem)sender;

            ListViewHitTestInfo hittest_info = (ListViewHitTestInfo)menu_item.Tag;
            Debug.Assert(hittest_info.Item != null, "");
            int x = hittest_info.Item.SubItems.IndexOf(hittest_info.SubItem);
            Debug.Assert(x != -1, "");

            string strFirstText = "";
            foreach (ListViewItem item in this.listView.SelectedItems)
            {
                string strTemp = ListViewUtil.GetItemText(item, x);
                if (String.IsNullOrEmpty(strTemp) == false)
                {
                    strFirstText = strTemp;
                    break;
                }
            }

            if (string.IsNullOrEmpty(strFirstText) == true)
            {
                MessageBox.Show(this, "列 " + this.listView.Columns[x].Text + " 中没有找到可复制的值...");
                return;
            }

            bool bChanged = false;
            foreach (ListViewItem item in this.listView.SelectedItems)
            {
                BookItem bookitem = (BookItem)item.Tag;

                string strTemp = ListViewUtil.GetItemText(item, x);
                if (String.IsNullOrEmpty(strTemp) == true)
                {
                    bookitem.SetColumnText(x, strFirstText);
                    // ListViewUtil.ChangeItemText(item, x, strFirstText);
                    bookitem.RefreshListView();
                    bChanged = true;
                }
            }
            if (bChanged == true)
            {
                /*
                if (this.ContentChanged != null)
                {
                    ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                    e1.OldChanged = bOldChanged;
                    e1.CurrentChanged = this.Changed;
                    this.ContentChanged(this, e1);
                }
                 * */
                this.Changed = bChanged;
            }
        }

        // 全选
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView);
            /*
            for (int i = 0; i < this.ListView.Items.Count; i++)
            {
                this.ListView.Items[i].Selected = true;
            }
             * */
        }

        // 创建数据
        void menu_generateData_Click(object sender, EventArgs e)
        {
#if NO
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.ListView;
                e1.ScriptEntry = "";    // 启动Ctrl+A菜单
                this.GenerateData(this, e1);
            }
            else
            {
                MessageBox.Show(this, this.GetType().ToString() + "控件没有挂接 GenerateData 事件");
            }
#endif
            this.DoGenerateData("");    // 启动Ctrl+A菜单
        }

        // 管理索取号
        void menu_manageCallNumber_Click(object sender, EventArgs e)
        {
#if NO
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.ListView;
                e1.ScriptEntry = "ManageCallNumber";    // 直接启动ManageCallNumber()函数
                this.GenerateData(this, e1);
            }
            else
            {
                MessageBox.Show(this, "EntityControl没有挂接GenerateData事件");
            }
#endif
            this.DoGenerateData("ManageCallNumber");    // 直接启动ManageCallNumber()函数

        }

        // 创建索取号
        void menu_createCallNumber_Click(object sender, EventArgs e)
        {
#if NO
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                e1.FocusedControl = this.ListView;
                e1.ScriptEntry = "CreateCallNumber";    // 直接启动CreateCallNumber()函数
                this.GenerateData(this, e1);
            }
            else
            {
                MessageBox.Show(this, "EntityControl没有挂接GenerateData事件");
            }
#endif
            this.DoGenerateData("CreateCallNumber");    // 直接启动CreateCallNumber()函数

        }

        // 
        /// <summary>
        /// 在listview中选定指定的事项
        /// </summary>
        /// <param name="bClearSelectionFirst">是否在选定前清除全部已有的选定状态</param>
        /// <param name="bookitems">要选定的事项集合</param>
        /// <returns>世纪选定的事项个数</returns>
        public int SelectItems(
            bool bClearSelectionFirst,
            List<BookItem> bookitems)
        {
            if (bClearSelectionFirst == true)
                ListViewUtil.ClearSelection(this.listView);

            int nSelectedCount = 0;
            foreach (BookItem item in bookitems)
            {
                int nRet = this.Items.IndexOf(item);
                if (nRet == -1)
                    continue;
                item.ListViewItem.Selected = true;
                nSelectedCount++;
            }

            return nSelectedCount;
        }

        // 
        // return:
        //      -1  出错
        //      0   放弃处理
        //      1   已经处理
        /// <summary>
        /// 为当前选定的事项创建索取号
        /// </summary>
        /// <param name="bOverwriteExist">true: 对全部选定的事项都重新创建索取号; false: 只有当前索取号字符串为空的才给进行创建索引号的操作</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃处理; 1: 已经处理</returns>
        public int CreateCallNumber(
            bool bOverwriteExist,
            out string strError)
        {
            strError = "";

#if NO
            if (this.GenerateData == null)
            {
                strError = "EntityControl没有挂接GenerateData事件";
                return -1;
            }
#endif
            if (this.HasGenerateData() == false)
            {
                strError = "EntityControl 没有挂接 GenerateData 事件";
                return -1;
            }

            if (bOverwriteExist == false)
            {
                // 只有当前索取号字符串为空的才给进行创建索引号的操作
                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView.SelectedItems)
                {
                    BookItem bookitem = (BookItem)item.Tag;

                    if (string.IsNullOrEmpty(bookitem.AccessNo) == false)
                        items.Add(item);
                }

                foreach (ListViewItem item in items)
                {
                    item.Selected = false;
                }

                if (this.listView.SelectedItems.Count == 0)
                    return 0;
            }

#if NO
            GenerateDataEventArgs e1 = new GenerateDataEventArgs();
            e1.FocusedControl = this.ListView;
            e1.ScriptEntry = "CreateCallNumber";    // 直接启动CreateCallNumber()函数
            e1.ShowErrorBox = false;
            this.GenerateData(this, e1);
#endif
            GenerateDataEventArgs e1 = this.DoGenerateData("CreateCallNumber", false);// 直接启动CreateCallNumber()函数
            if (e1 == null)
            {
                strError = "e1 null";
                return -1;
            }

            if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = e1.ErrorInfo;
                return -1;
            }

            return 1;
        }

        private void listView_items_DoubleClick(object sender, EventArgs e)
        {
            menu_modifyEntity_Click(this, null);
        }

        // 剪切
        void menu_cutEntity_Click(object sender, EventArgs e)
        {
            ClipboardBookItemCollection newbookitems = new ClipboardBookItemCollection();

            string strNotDeleteList = "";
            int nDeleteCount = 0;

            // bool bOldChanged = this.Changed;

            List<BookItem> deleteitems = new List<BookItem>();

            // 先检查一遍有借阅信息不能删除的情况
            for (int i = 0; i < this.listView.Items.Count; i++)
            {
                ListViewItem item = this.listView.Items[i];

                if (item.Selected == false)
                    continue;

                BookItem bookitem = (BookItem)item.Tag;

                if (String.IsNullOrEmpty(bookitem.Borrower) == false)
                {
                    if (strNotDeleteList != "")
                        strNotDeleteList += ",";
                    strNotDeleteList += bookitem.Barcode;
                    continue;
                }

                nDeleteCount++;
                deleteitems.Add(bookitem);
            }

            if (strNotDeleteList != "")
            {
                string strText = "条码为 '"
                    + strNotDeleteList +
                    "' 的册包含有流通信息, 不能加以标记删除。\r\n\r\n";

                if (nDeleteCount == 0)
                {
                    // 除开不能删除的事项，余下也没有要删的事项了
                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return;
                }


                strText += "是否要继续剪切其余 " + nDeleteCount.ToString() + " 项?";

                DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                    strText,
                    "EntityForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            for (int i = 0; i < deleteitems.Count; i++)
            {
                BookItem bookitem = deleteitems[i];

                // 克隆对象才行
                BookItem dupitem = bookitem.Clone();

                newbookitems.Add(dupitem);
                // 将所涉及的源对象修改为deleted状态

                int nRet = MaskDeleteItem(bookitem,
                    m_bRemoveDeletedItem);
                if (nRet == 0)
                {
                    Debug.Assert(false, "这里MaskDeleteItem()不可能出现返回0的情况呀，因为前面已经预判过了");
                    continue;
                }
            }

            Clipboard.SetDataObject(newbookitems, true);

            // this.SetSaveAllButtonState(true);
            /*
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Changed;
                this.ContentChanged(this, e1);
            }
             * */
            this.Changed = this.Changed;
        }

        // 复制
        void menu_copyEntity_Click(object sender, EventArgs e)
        {
            ClipboardBookItemCollection newbookitems = new ClipboardBookItemCollection();
            StringBuilder text = new StringBuilder();
            List<string> xmls = new List<string>();
            for (int i = 0; i < this.listView.Items.Count; i++)
            {
                ListViewItem item = this.listView.Items[i];

                if (item.Selected == false)
                    continue;

                BookItem bookitem = (BookItem)item.Tag;

                // 克隆对象才行
                BookItem dupitem = bookitem.Clone();

                newbookitems.Add(dupitem);

                // text
                text.Append(Global.BuildLine(item) + "\r\n");

                // xml
                string strXml = "";
                string strError = "";
                int nRet = bookitem.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strXml,
                    out strError);
                if (nRet == -1)
                    xmls.Add("!" + strError);
                else
                    xmls.Add(strXml);
            }

            // DataObject obj = new DataObject(newbookitems);

            // Clipboard.SetDataObject(newbookitems, true);

            DataObject obj = new DataObject();
            obj.SetData(typeof(ClipboardBookItemCollection), newbookitems);
            obj.SetData(text.ToString());
            obj.SetData("xml", xmls);
            Clipboard.SetDataObject(obj, true);
        }

        // 实作粘贴
        int DoPaste(out string strError)
        {
            strError = "";

            // bool bOldChanged = this.Changed;

            /*
if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
{
    strError = "尚未载入书目记录，无法插入册信息";
    goto ERROR1;
}
 * */
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(ClipboardBookItemCollection)) == false)
            {
                strError = "剪贴板中尚不存在ClipboardBookItemCollection类型数据";
                return -1;
            }

            ClipboardBookItemCollection clipbookitems = (ClipboardBookItemCollection)iData.GetData(typeof(ClipboardBookItemCollection));
            if (clipbookitems == null)
            {
                strError = "iData.GetData() return null";
                return -1;
            }

            clipbookitems.RestoreNonSerialized();

            if (this.Items == null)
                this.Items = new BookItemCollection();

            Debug.Assert(this.Items != null, "");

            this.Items.ClearListViewHilight();

            // 准备查重用的refid表格
            Hashtable table = new Hashtable();
            foreach (BookItem bookitem in this.Items)
            {
                if (string.IsNullOrEmpty(bookitem.RefID) == false)
                {
                    if (table.Contains(bookitem.RefID) == true)
                    {
                        strError = "原有册事项中出现了重复的参考ID值 '" + bookitem.RefID + "'";
                        return -1;
                    }

                    if (table.Contains(bookitem.RefID) == false)
                        table.Add(bookitem.RefID, null);
                }
            }

            // 看看即将paste过来的事项，其条码有没有和本窗口现有事项重复的？
            string strDupBarcodeList = "";
            for (int i = 0; i < clipbookitems.Count; i++)
            {
                BookItem bookitem = clipbookitems[i];

                string strBarcode = bookitem.Barcode;

                // refid查重
                if (string.IsNullOrEmpty(bookitem.RefID) == false)
                {
                    if (table.Contains(bookitem.RefID) == true)
                    {
                        /*
                        strError = "剪贴板册事项中出现了重复的参考ID值 '" + bookitem.RefID + "'";
                        return -1;
                         * */
                        bookitem.RefID = "";    // 促使以后重新分配
                    }

                    if (table.Contains(bookitem.RefID) == false)
                        table.Add(bookitem.RefID, null);
                }

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;   // 2008/11/3

                // 对当前窗口内进行条码查重
                BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                if (dupitem != null)
                {
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                    {
                    }
                    else
                    {
                        // 突出显示，以便操作人员观察这条已经存在的记录
                        dupitem.HilightListViewItem(false);

                        // 加入重复条码列表
                        if (strDupBarcodeList != "")
                            strDupBarcodeList += ",";
                        strDupBarcodeList += strBarcode;
                    }
                }
            }

            bool bOverwrite = false;

            if (String.IsNullOrEmpty(strDupBarcodeList) == false)
            {
                DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
    "即将粘贴的下列条码事项在当前窗口中已经存在:\r\n" + strDupBarcodeList + "\r\n\r\n是否要覆盖这些事项? (Yes 覆盖 / No 忽略这些事项，但继续粘贴其他事项 / Cancel 放弃整个粘贴操作)",
    "EntityForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return 0;
                if (result == DialogResult.Yes)
                    bOverwrite = true;
                else
                    bOverwrite = false;
            }

            for (int i = 0; i < clipbookitems.Count; i++)
            {
                BookItem bookitem = clipbookitems[i];

                string strBarcode = bookitem.Barcode;

                BookItem dupitem = null;

                if (String.IsNullOrEmpty(strBarcode) == false)  // 2008/11/3
                {
                    // 对当前窗口内进行条码查重
                    dupitem = this.Items.GetItemByBarcode(strBarcode);
                    if (dupitem != null)
                    {
                        if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                            this.Items.PhysicalDeleteItem(dupitem);
                        else
                        {
                            if (bOverwrite == false)
                                continue;
                            else
                                this.Items.PhysicalDeleteItem(dupitem);
                        }
                    }
                }

                // 插入
                bookitem.Parent = Global.GetRecordID(this.BiblioRecPath);

                // 2017/3/2
                if (string.IsNullOrEmpty(bookitem.RefID))
                {
                    bookitem.RefID = Guid.NewGuid().ToString();
                }

                this.Items.Add(bookitem);

                if (dupitem != null)
                {
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                    {
                        bookitem.ItemDisplayState = ItemDisplayState.Changed;
                        bookitem.Timestamp = dupitem.Timestamp; // 继承当前窗口事项的timestamp
                    }
                    else if (dupitem.ItemDisplayState == ItemDisplayState.Changed
                        || dupitem.ItemDisplayState == ItemDisplayState.Normal)
                    {
                        bookitem.ItemDisplayState = ItemDisplayState.Changed;
                        bookitem.Timestamp = dupitem.Timestamp; // 继承当前窗口事项的timestamp
                    }
                    else
                        bookitem.ItemDisplayState = ItemDisplayState.New;
                }
                else
                    bookitem.ItemDisplayState = ItemDisplayState.New;

                bookitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对
                bookitem.AddToListView(this.listView);
                bookitem.HilightListViewItem(false);
            }

            // this.SetSaveAllButtonState(true);
            /*
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Changed;
                this.ContentChanged(this, e1);
            }
             * */
            this.Changed = this.Changed;
            return 0;
        }

        // 粘贴
        void menu_pasteEntity_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";

            nRet = DoPaste(out strError);
            if (nRet == -1)
                MessageBox.Show(ForegroundWindow.Instance, strError);
        }

        // 修改一个实体
        void menu_modifyEntity_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要编辑的事项");
                return;
            }
            BookItem bookitem = (BookItem)this.listView.SelectedItems[0].Tag;

            ModifyEntity(bookitem);
        }

        void ModifyEntity(BookItem bookitem)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(bookitem != null, "");

            string strOldBarcode = bookitem.Barcode;

            using (EntityEditForm edit = new EntityEditForm())
            {
                this.ParentShowMessage("正在准备数据 ...", "green", false);
                try
                {
                    // 2009/2/24 
                    edit.GenerateData -= new GenerateDataEventHandler(edit_GenerateData);
                    edit.GenerateData += new GenerateDataEventHandler(edit_GenerateData);

                    /*
                    edit.GenerateAccessNo -= new GenerateDataEventHandler(edit_GenerateAccessNo);
                    edit.GenerateAccessNo += new GenerateDataEventHandler(edit_GenerateAccessNo);
                     * */

                    edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15 add 
                    // edit.MainForm = Program.MainForm;
                    edit.ItemControl = this;
                    nRet = edit.InitialForEdit(bookitem,
                        this.Items,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(ForegroundWindow.Instance, strError);
                        return;
                    }
                    edit.StartItem = null;  // 清除原始对象标记
                }
                finally
                {
                    this.ParentShowMessage("", "", false);
                }

            REDO:
                Program.MainForm.AppInfo.LinkFormState(edit, "EntityEditForm_state");
                edit.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(edit);

                if (edit.DialogResult != DialogResult.OK)
                {
                    // TODO: 取消中途记忆的所有种次号
                    return;
                }

                LibraryChannel channel = Program.MainForm.GetChannel();

                // BookItem对象已经被修改
                this.EnableControls(false);
                this.ParentShowMessage("正在对册条码号 '" + bookitem.Barcode + "' 进行查重 ...", "green", false);
                try
                {
                    if (strOldBarcode != bookitem.Barcode // 条码改变了的情况下才查重
                        && String.IsNullOrEmpty(bookitem.Barcode) == false)   // 2008/11/3 不检查空的条码号是否重复
                    {
                        // 对当前窗口内进行条码查重
                        BookItem dupitem = this.Items.GetItemByBarcode(bookitem.Barcode);
                        if (dupitem != bookitem)
                        {
                            string strText = "";
                            if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                                strText = "册条码号 '" + bookitem.Barcode + "' 和本种中未提交之一删除册条码号相重。按“确定”按钮重新输入，或退出对话框后先行提交已有之修改。";
                            else
                                strText = "册条码号 '" + bookitem.Barcode + "' 在本种中已经存在。按“确定”按钮重新输入。";

                            MessageBox.Show(ForegroundWindow.Instance, strText);
                            goto REDO;
                        }

                        // 对所有实体记录进行条码查重
                        if (edit.AutoSearchDup == true
                            && string.IsNullOrEmpty(bookitem.Barcode) == false)
                        {
                            // Debug.Assert(false, "");

                            string[] paths = null;
                            // 册条码号查重。用于(可能是)旧条码号查重。
                            // parameters:
                            //      strBarcode  册条码号。
                            //      strOriginRecPath    出发记录的路径。
                            //      paths   所有命中的路径
                            // return:
                            //      -1  error
                            //      0   not dup
                            //      1   dup
                            nRet = SearchEntityBarcodeDup(
                                channel,
                                bookitem.Barcode,
                                bookitem.RecPath,
                                out paths,
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(ForegroundWindow.Instance, "对册条码号 '" + bookitem.Barcode + "' 进行查重的过程中发生错误: " + strError);
                            else if (nRet == 1) // 发生重复
                            {
                                string pathlist = String.Join(",", paths);

                                string strText = "条码 '" + bookitem.Barcode + "' 在数据库中发现已经被(属于其他种的)下列册记录所使用。\r\n" + pathlist + "\r\n\r\n按“确定”按钮重新编辑册信息，或者根据提示的册记录路径，去修改其他册记录信息。";
                                MessageBox.Show(ForegroundWindow.Instance, strText);

                                goto REDO;
                            }
                        }
                    }

                    // 2017/3/2
                    if (string.IsNullOrEmpty(bookitem.RefID))
                    {
                        bookitem.RefID = Guid.NewGuid().ToString();
                    }

                    if (edit.NextAction == "new")
                    {
                        // 要新增记录
                        this.BeginInvoke(new Action<string>(DoNewEntity), "");
                        return;
                    }
                }
                finally
                {
                    this.ParentShowMessage("", "", false);
                    this.EnableControls(true);

                    Program.MainForm.ReturnChannel(channel);
                }
            }
        }

        /*
        void edit_GenerateAccessNo(object sender, GenerateDataEventArgs e)
        {
            if (this.GenerateAccessNo != null)
            {
                this.GenerateAccessNo(sender, e);
            }
            else
            {
                MessageBox.Show(this, "EntityControl没有挂接GenerateAccessNo事件");
            }
        }*/

        void edit_GenerateData(object sender, GenerateDataEventArgs e)
        {
#if NO
            if (this.GenerateData != null)
            {
                this.GenerateData(sender, e);
            }
            else
            {
                MessageBox.Show(this, "EntityControl没有挂接GenerateData事件");
            }
#endif
            this.DoGenerateData(sender, e);
        }

        // 条码查重
        // return:
        //      -1  出错
        //      0   不重复
        //      1   重复
        /// <summary>
        /// 对一个事项进行册条码号查重
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <param name="myself">发起查重的对象</param>
        /// <param name="bCheckCurrentList">是否要检查当前列表中的(尚未保存的)事项</param>
        /// <param name="bCheckDb">是否对数据库进行查重</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 不重复; 1: 有重复</returns>
        public int CheckBarcodeDup(
            string strBarcode,
            BookItem myself,
            bool bCheckCurrentList,
            bool bCheckDb,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (bCheckCurrentList == true)
            {
                // 对当前list内进行条码查重
                BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                if (dupitem != null && dupitem != myself)
                {
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strError = "册条码号 '" + strBarcode + "' 和本种中未提交之一删除册条码号相重。(若要避免这种情况，需要退出对话框后先行提交已有之修改)";
                    else
                        strError = "册条码号 '" + strBarcode + "' 在本种中已经存在。";
                    return 1;
                }
            }

            // 对所有实体记录进行条码查重
            if (bCheckDb == true)
            {
                string strOriginRecPath = "";

                if (myself != null)
                    strOriginRecPath = myself.RecPath;

                string[] paths = null;
                LibraryChannel channel = Program.MainForm.GetChannel();
                try
                {
                    // 册条码号查重。用于(可能是)旧条码号查重。
                    // parameters:
                    //      strBarcode  册条码号。
                    //      strOriginRecPath    出发记录的路径。
                    //      paths   所有命中的路径
                    // return:
                    //      -1  error
                    //      0   not dup
                    //      1   dup
                    nRet = SearchEntityBarcodeDup(
                        channel,
                        strBarcode,
                        strOriginRecPath,
                        out paths,
                        out strError);
                }
                finally
                {
                    Program.MainForm.ReturnChannel(channel);
                }
                if (nRet == -1)
                {
                    strError = "对册条码 '" + strBarcode + "' 进行查重的过程中发生错误: " + strError;
                    return -1;
                }
                else if (nRet == 1) // 发生重复
                {
                    string pathlist = String.Join(",", paths);

                    strError = "条码号 '" + strBarcode + "' 在数据库中发现已经被(属于其他种的)下列册记录所使用。\r\n" + pathlist + "\r\n\r\n请检查这里输入的条码号是否正确；或根据提示的册记录路径，去修改其他册记录信息，避免条码号重复。";
                    return 1;
                }
            }

            return 0;
        }

        // 对一批事项的条码查重
        // return:
        //      -1  出错
        //      0   不重复
        //      1   重复
        /// <summary>
        /// 对一批事项进行册条码号查重
        /// </summary>
        /// <param name="book_items">要进行查重的事项集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 不重复; 1: 有重复</returns>
        public int CheckBarcodeDup(
            List<BookItem> book_items,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            for (int i = 0; i < book_items.Count; i++)
            {
                BookItem myself = book_items[i];
                string strBarcode = myself.Barcode;

                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                {
                    // 对当前list内进行条码查重
                    BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                    if (dupitem != null && dupitem != myself)
                    {
                        if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                            strError += "册条码号 '" + strBarcode + "' 和本种中未提交之一删除册条码号相重。(若要避免这种情况，需要先行提交已有之修改); ";
                        else
                            strError += "册条码号 '" + strBarcode + "' 在本种中已经存在; ";
                        continue;   // 就不再继续查全数据库了
                    }
                }

                // 对所有实体记录进行条码查重
                {
                    string strOriginRecPath = "";

                    if (myself != null)
                        strOriginRecPath = myself.RecPath;

                    string[] paths = null;
                    string strTempError = "";

                    LibraryChannel channel = Program.MainForm.GetChannel();
                    try
                    {
                        // 册条码号查重。用于(可能是)旧条码号查重。
                        // parameters:
                        //      strBarcode  册条码号。
                        //      strOriginRecPath    出发记录的路径。
                        //      paths   所有命中的路径
                        // return:
                        //      -1  error
                        //      0   not dup
                        //      1   dup
                        nRet = SearchEntityBarcodeDup(
                            channel,
                            strBarcode,
                            strOriginRecPath,
                            out paths,
                            out strTempError);
                    }
                    finally
                    {
                        Program.MainForm.ReturnChannel(channel);
                    }
                    if (nRet == -1)
                    {
                        strError = "对册条码号 '" + strBarcode + "' 进行查重的过程中发生错误: " + strTempError;
                        return -1;
                    }
                    else if (nRet == 1) // 发生重复
                    {
                        string pathlist = String.Join(",", paths);

                        strError += "条码 '" + strBarcode + "' 在数据库中发现已经被(属于其他种的)下列册记录所使用: " + pathlist + "; ";
                    }
                }
            }

            if (String.IsNullOrEmpty(strError) == false)
                return 1;

            return 0;
        }

        // 新增一个实体
        void menu_newEntity_Click(object sender, EventArgs e)
        {
            DoNewEntity("");
        }

        // 新增多个实体
        void menu_newMultiEntity_Click(object sender, EventArgs e)
        {
            int nRet = 0;
            string strError = "";
        // bool bOldChanged = this.Changed;

        REDO_INPUT:
            string strNumber = InputDlg.GetInput(
                this,
                "新增多个实体",
                "要创建的个数: ",
                "2",
            Program.MainForm.DefaultFont);
            if (strNumber == null)
                return;

            int nNumber = 0;
            try
            {
                nNumber = Convert.ToInt32(strNumber);
            }
            catch
            {
                MessageBox.Show(ForegroundWindow.Instance, "必须输入纯数字");
                goto REDO_INPUT;
            }

            for (int i = 0; i < nNumber; i++)
            {
                BookItem bookitem = new BookItem();

                // 设置缺省值
                nRet = SetItemDefaultValues(
                    "normalRegister_default",
                    true,
                    bookitem,
                    out strError);
                if (nRet == -1)
                {
                    strError = "设置缺省值的时候发生错误: " + strError;
                    goto ERROR1;
                }


                bookitem.Barcode = "";
                bookitem.Parent = Global.GetRecordID(this.BiblioRecPath);
                // 2017/3/2
                if (string.IsNullOrEmpty(bookitem.RefID))
                {
                    bookitem.RefID = Guid.NewGuid().ToString();
                }
                // 加入列表
                this.Items.Add(bookitem);
                bookitem.ItemDisplayState = ItemDisplayState.New;
                bookitem.AddToListView(this.listView);
                bookitem.HilightListViewItem(true);

                bookitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对
            }

            // 改变保存按钮状态
            // SetSaveAllButtonState(true);
            /*
            if (this.ContentChanged != null)
            {
                ContentChangedEventArgs e1 = new ContentChangedEventArgs();
                e1.OldChanged = bOldChanged;
                e1.CurrentChanged = this.Changed;
                this.ContentChanged(this, e1);
            }
            */
            this.Changed = this.Changed;

            return;

        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

        // 
        /// <summary>
        /// 新增一个实体，要打开对话框让输入详细信息
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        public void DoNewEntity(string strBarcode)
        {
            string strError = "";
            int nRet = 0;

            // bool bOldChanged = this.Changed;

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未载入书目记录";
                goto ERROR1;
            }

            // 
            if (this.Items == null)
                this.Items = new BookItemCollection();

            Debug.Assert(this.Items != null, "");

            using (EntityEditForm edit = new EntityEditForm())
            {
                BookItem bookitem = null;

                LibraryChannel channel = Program.MainForm.GetChannel();
                this.ParentShowMessage("正在对册条码号 '" + strBarcode + "' 进行查重 ...", "green", false);
                try
                {
                    if (String.IsNullOrEmpty(strBarcode) == false)
                    {
                        // 对当前窗口内进行条码查重
                        BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                        if (dupitem != null)
                        {
                            string strText = "";
                            if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                                strText = "拟新增的册条码号 '" + strBarcode + "' 和本种中未提交之一删除册条码号相重。请先行提交已有之修改，再进行册登记。";
                            else
                                strText = "拟新增的册条码号 '" + strBarcode + "' 在本种中已经存在。";

                            // 警告尚未保存
                            DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                strText + "\r\n\r\n要立即对已存在条码进行修改吗？",
                "EntityForm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

                            // 转为修改
                            if (result == DialogResult.Yes)
                            {
                                /*
                                // 先Undo潜在的Delete状态
                                this.bookitems.UndoMaskDeleteItem(dupitem);
                                 * */

                                ModifyEntity(dupitem);
                                return;
                            }

                            // 突出显示，以便操作人员观察这条已经存在的记录
                            dupitem.HilightListViewItem(true);
                            return;
                        }

                        // 对所有实体记录进行条码查重
                        if (true)
                        {
                            string strItemText = "";
                            string strBiblioText = "";
                            nRet = SearchEntityBarcode(
                                channel,
                                strBarcode,
                                out strItemText,
                                out strBiblioText,
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(ForegroundWindow.Instance, "对册条码号 '" + strBarcode + "' 进行查重的过程中发生错误: " + strError);
                            else if (nRet == 1) // 发生重复
                            {
                                EntityBarcodeFoundDupDlg dlg = new EntityBarcodeFoundDupDlg();
                                MainForm.SetControlFont(dlg, this.Font, false);
                                // dlg.MainForm = Program.MainForm;
                                dlg.BiblioText = strBiblioText;
                                dlg.ItemText = strItemText;
                                dlg.MessageText = "拟新增的册条码号 '" + strBarcode + "' 在数据库中发现已经存在。因此无法新增。";

                                Program.MainForm.AppInfo.LinkFormState(dlg, "EntityBarcodeFoundDupDlg_state");
                                dlg.ShowDialog(this);
                                return;
                            }
                        }
                    } // end of ' if (String.IsNullOrEmpty(strBarcode) == false)

                    this.ParentShowMessage("正在准备数据 ...", "green", false);

                    bookitem = new BookItem();

                    // 设置缺省值
                    nRet = SetItemDefaultValues(
                        "normalRegister_default",
                        true,
                        bookitem,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "设置缺省值的时候发生错误: " + strError;
                        goto ERROR1;
                    }

                    bookitem.Barcode = strBarcode;
                    bookitem.Parent = Global.GetRecordID(this.BiblioRecPath);

                    // 2017/3/2
                    if (string.IsNullOrEmpty(bookitem.RefID))
                    {
                        bookitem.RefID = Guid.NewGuid().ToString();
                    }

                    // 先加入列表
                    this.Items.Add(bookitem);
                    bookitem.ItemDisplayState = ItemDisplayState.New;
                    bookitem.AddToListView(this.listView);
                    bookitem.HilightListViewItem(true);

                    bookitem.Changed = true;    // 因为是新增的事项，无论如何都算修改过。这样可以避免集合中只有一个新增事项的时候，集合的changed值不对

                    // edit = new EntityEditForm();

                    // 2009/2/24 
                    edit.GenerateData -= new GenerateDataEventHandler(edit_GenerateData);
                    edit.GenerateData += new GenerateDataEventHandler(edit_GenerateData);

                    /*
                    edit.GenerateAccessNo -= new GenerateDataEventHandler(edit_GenerateAccessNo);
                    edit.GenerateAccessNo += new GenerateDataEventHandler(edit_GenerateAccessNo);
                     * */

                    edit.BiblioDbName = Global.GetDbName(this.BiblioRecPath);   // 2009/2/15 add
                    edit.Text = "新增册";
                    // edit.MainForm = Program.MainForm;
                    edit.ItemControl = this;
                    edit.DisplayMode = Program.MainForm.AppInfo.GetBoolean(
            "entityform_optiondlg",
            "normalRegister_simple",
            false) == true ? "simple" : "full";
                    nRet = edit.InitialForEdit(bookitem,
                        this.Items,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    this.ParentShowMessage("", "", false);
                    Program.MainForm.ReturnChannel(channel);
                }
                //REDO:
                Program.MainForm.AppInfo.LinkFormState(edit, "EntityEditForm_state");
                edit.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(edit);

                if (edit.DialogResult != DialogResult.OK)
                {
                    // TODO: 取消中途记忆的所有种次号
                }

                if (edit.DialogResult != DialogResult.OK
                    && edit.Item == bookitem    // 表明尚未前后移动，或者移动回到起点，然后Cancel
                    )
                {
                    this.Items.PhysicalDeleteItem(bookitem);

                    this.Changed = this.Changed;
                    return;
                }

                this.Changed = this.Changed;

                // TODO: 2007/10/23
                // 要对本种和所有相关实体库进行条码查重。
                // 如果重了，要保持窗口，以便修改。不过从这个角度，查重最好在对话框关闭前作？
                // 或者重新打开对话框

                if (edit.NextAction == "new")
                {
                    // 要新增记录
                    this.BeginInvoke(new Action<string>(DoNewEntity), "");
                    return;
                }
            }
            return;

        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return;
        }

        // 提交实体保存请求
        // return:
        //      -1  出错
        //      0   没有必要保存
        //      1   保存成功
        /// <summary>
        /// 提交 Items 保存请求
        /// </summary>
        /// <returns>-1: 出错; 0: 没有必要保存; 1: 保存成功</returns>
        public override int SaveItems(
            LibraryChannel channel,
            out string strError)
        {
            // TODO: 是否要先保存以前的选择，功能执行完以后恢复以前的选择?

            ListViewUtil.ClearSelection(this.listView);
            // this.listView.SelectedItems.Clear();

            // 如果必要，创建索取号
            foreach (ListViewItem item in this.listView.Items)
            {
                BookItem bookitem = (BookItem)item.Tag;
                if (bookitem == null)
                    continue;

                if (StringUtil.HasHead(bookitem.AccessNo, "@accessNo") == true)
                {
                    item.Selected = true;
                }
            }

            if (this.listView.SelectedItems.Count > 0)
            {
                GenerateDataEventArgs ret = this.DoGenerateData("CreateCallNumber", false);    // 直接启动CreateCallNumber()函数
                if (string.IsNullOrEmpty(ret.ErrorInfo) == false)
                {
                    strError = "保存册记录前创建索取号失败: " + ret.ErrorInfo + "\r\n\r\n保存没有成功";
                    return -1;
                }
            }

            return base.SaveItems(channel, out strError);
        }

        // 外部调用，设置一个实体记录。
        // 具体动作有：new change delete neworchange
        // parameters:
        //      bWarningBarcodeDup  是否仅仅警告条码重的情况？==true，仅警告，但是依然创建记录；==false，当作出错立即从函数中返回
        //      bookitem    [out]返回相关的BookItem对象
        // return:
        //      0   保存或者修改、删除成功，没有发现册条码重复
        //      1   保存成功，但是发现了册条码重复
        /// <summary>
        /// 设置一个实体记录
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="bWarningBarcodeDup">是否仅仅警告条码重的情况？==true，仅警告，但是依然创建记录；==false，当作出错立即从函数中返回</param>
        /// <param name="strAction">动作。为 new change delete neworchange 之一</param>
        /// <param name="strRefID">参考 ID</param>
        /// <param name="strXml">记录 XML</param>
        /// <param name="bFillDefaultValue">是否填入字段默认值</param>
        /// <param name="bookitem">返回相关的 BookItem 对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 保存或者修改、删除成功，没有发现册条码重复; 1: 保存成功，但是发现了册条码重复</returns>
        public int DoSetEntity(
            LibraryChannel channel,
            bool bWarningBarcodeDup,
            string strAction,
            string strRefID,
            string strXml,
            bool bFillDefaultValue,
            out BookItem bookitem,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            bookitem = null;
            string strWarning = "";

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未载入书目记录";
                return -1;
            }

            if (String.IsNullOrEmpty(strRefID) == true)
            {
                strError = "strRefID参数值不能为空";
                return -1;
            }

            // 2008/9/17 
            if (this.Items == null)
                this.Items = new BookItemCollection();

            // 看看是否有已经存在的记录
            BookItem exist_item = this.Items.GetItemByRefID(strRefID) as BookItem;

            // 2009/12/16 
            if (exist_item != null)
            {
                if (strAction == "neworchange")
                    strAction = "change";
            }
            else
            {
                if (strAction == "neworchange")
                    strAction = "new";
            }

            if (exist_item != null)
            {
                if (strAction == "new")
                {
                    strError = "refid为'" + strRefID + "' 的事项已经存在，不能再重复新增";
                    return -1;
                }
            }
            else
            {
                if (strAction == "change")
                {
                    strError = "refid为'" + strRefID + "' 的事项并不存在，无法进行修改";
                    return -1;
                }

                if (strAction == "delete")
                {
                    strError = "refid为'" + strRefID + "' 的事项并不存在，无法进行删除";
                    return -1;
                }
            }

            string strOperName = "";
            if (strAction == "new")
                strOperName = "新增";
            else if (strAction == "change")
                strOperName = "修改";
            else if (strAction == "delete")
                strOperName = "删除";

            if (strAction == "delete")
            {
                // 标记删除事项
                // return:
                //      0   因为有流通信息，未能标记删除
                //      1   成功删除
                nRet = MaskDeleteItem(exist_item,
                         this.m_bRemoveDeletedItem);
                if (nRet == 0)
                {
                    strError = "refid为'" + strRefID + "' 的册事项因为包含有流通信息，无法进行删除";
                    return -1;
                }

                return 0;   // 1
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML字符串装入XMLDOM时出错: " + ex.Message;
                return -1;
            }

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            if (String.IsNullOrEmpty(strBarcode) == false)
            {
                // 对当前窗口内进行条码查重
                BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                if (dupitem != null)
                {
                    if (strAction == "change" || strAction == "delete")
                    {
                        if (exist_item == dupitem)
                            goto SKIP1;
                    }

                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strError = "拟" + strOperName + "的册信息中，册条码号 '" + strBarcode + "' 和本种中未提交之一删除册条码号相重";
                    else
                        strError = "拟" + strOperName + "的册信息中，册条码号 '" + strBarcode + "' 在本种中已经存在";

                    if (bWarningBarcodeDup == true)
                    {
                        if (string.IsNullOrEmpty(strWarning) == false)
                            strWarning += ";\r\n";
                        strWarning += strError;
                    }
                    else
                        return -1;
                }
            }

        SKIP1:

            // 对所有实体记录进行条码查重
            if (String.IsNullOrEmpty(strBarcode) == false
                && strAction == "new")
            {
                string strItemText = "";
                string strBiblioText = "";
                nRet = SearchEntityBarcode(
                    channel,
                    strBarcode,
                    out strItemText,
                    out strBiblioText,
                    out strError);
                if (nRet == -1)
                {
                    strError = "对册条码号 '" + strBarcode + "' 进行查重的过程中发生错误: " + strError;
                    return -1;
                }
                else if (nRet == 1) // 发生重复
                {
                    strError = "拟新增的册信息中，册条码号 '" + strBarcode + "' 在数据库中发现已经存在。";
                    if (bWarningBarcodeDup == true)
                    {
                        if (string.IsNullOrEmpty(strWarning) == false)
                            strWarning += ";\r\n";
                        strWarning += strError;
                    }
                    else
                        return -1;
                }
            }

            // BookItem bookitem = null;

            if (strAction == "new")
            {
                bookitem = new BookItem();

                // 设置缺省值
                if (bFillDefaultValue)
                {
                    nRet = SetItemDefaultValues(
                        "quickRegister_default",
                        true,
                        bookitem,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "设置缺省值的时候发生错误: " + strError;
                        return -1;
                    }
                }
            }
            else
                bookitem = exist_item;

            bookitem.Barcode = strBarcode;

            Debug.Assert(String.IsNullOrEmpty(strRefID) == false, "");

            bookitem.RefID = strRefID;

            // 为了避免BuildRecord()报错
            bookitem.Parent = Global.GetRecordID(this.BiblioRecPath);

            if (exist_item == null)
            {

                string strExistXml = "";
                nRet = bookitem.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strExistXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                XmlDocument domExist = new XmlDocument();
                try
                {
                    domExist.LoadXml(strExistXml);
                }
                catch (Exception ex)
                {
                    strError = "XML字符串strExistXml装入XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                // 遍历所有一级元素的内容
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("*");
                for (int i = 0; i < nodes.Count; i++)
                {
                    /*
                    string strText = nodes[i].InnerText;
                    if (String.IsNullOrEmpty(strText) == false)
                    {
                        DomUtil.SetElementText(domExist.DocumentElement,
                            nodes[i].Name, strText);
                    }*/

                    string name = nodes[i].Name;

                    // 2009/12/17 changed
                    string strText = nodes[i].OuterXml;
                    if (String.IsNullOrEmpty(strText) == false)
                    {
                        // 2020/5/27
                        // state 元素值需要新旧值合并
                        if (name == "state")
                        {
                            string oldText = DomUtil.GetElementText(domExist.DocumentElement, name);
                            string newText = nodes[i].InnerText.Trim();

                            StringUtil.SetInList(ref oldText, newText, true);
                            DomUtil.SetElementText(domExist.DocumentElement, name, oldText);
                        }
                        else
                            DomUtil.SetElementOuterXml(domExist.DocumentElement,
                               name, strText);
                    }
                }

                nRet = bookitem.SetData(bookitem.RecPath,
                    domExist.OuterXml,
                    null,
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            else
            {
                // 注: OldRecord/Timestamp不希望被改变 2010/3/22
                string strOldXml = bookitem.OldRecord;
                nRet = bookitem.SetData(bookitem.RecPath,
                    strXml,
                    bookitem.Timestamp, // 2010/2/16 changed
                    out strError);
                if (nRet == -1)
                    return -1;
                bookitem.OldRecord = strOldXml;
            }

            /*
            // seller
            string strSeller = DomUtil.GetElementText(dom.DocumentElement,
                "selller");
            if (String.IsNullOrEmpty(strSeller) == false)
                bookitem.Seller = strSeller;

            // source
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            if (String.IsNullOrEmpty(strSeller) == false)
                bookitem.Seller = strSeller;
             * */

            if (this.Items == null)
                this.Items = new BookItemCollection();

            Debug.Assert(this.Items != null, "");

            if (exist_item == null)
            {
                // 2017/3/2
                if (string.IsNullOrEmpty(bookitem.RefID))
                {
                    bookitem.RefID = Guid.NewGuid().ToString();
                }
                this.Items.Add(bookitem);
                bookitem.Parent = Global.GetRecordID(this.BiblioRecPath);
                bookitem.ItemDisplayState = ItemDisplayState.New;
                bookitem.AddToListView(this.listView);
            }
            else
            {
                // 2010/5/5
                if (bookitem.ItemDisplayState != ItemDisplayState.New)
                    bookitem.ItemDisplayState = ItemDisplayState.Changed;
            }

            bookitem.Changed = true;    // 否则“保存”按钮不能Enabled

            // 将刚刚加入的事项滚入可见范围
            bookitem.HilightListViewItem(true);
            bookitem.RefreshListView(); // 2009/12/18 add

            this.EnableControls(true);

            if (String.IsNullOrEmpty(strWarning) == false)
            {
                bookitem.Error = new EntityInfo();
                bookitem.Error.ErrorInfo = strWarning;
                bookitem.RefreshListView();

                strError = strWarning;
                return 1;   // 发现了重复
            }

            return 0;
        }

        // 
        // return:
        //      -1  出错
        //      0   遇到重复，没有加入
        //      1   已加入
        /// <summary>
        /// 快速新增一个实体，不打开对话框
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <returns>-1: 出错; 0 遇到重复，没有加入; 1: 已加入</returns>
        public int DoQuickNewEntity(string strBarcode)
        {
            int nRet = 0;
            string strError = "";

            if (String.IsNullOrEmpty(this.BiblioRecPath) == true)
            {
                strError = "尚未载入书目记录";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(strBarcode) == false)  // 2008/11/3
            {

                // 对当前窗口内进行条码查重
                BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "拟新增的册信息中，册条码号 '" + strBarcode + "' 和本种中未提交之一删除册条码号相重。若确实要新增，请先行提交已有之删除请求。";
                    else
                        strText = "拟新增的册信息中，册条码号 '" + strBarcode + "' 在本种中已经存在。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 0;
                }

                // 对所有实体记录进行条码查重
                if (true)
                {
                    string strItemText = "";
                    string strBiblioText = "";
                    // string strError = "";
                    LibraryChannel channel = Program.MainForm.GetChannel();
                    try
                    {
                        nRet = SearchEntityBarcode(
                            channel,
                            strBarcode,
                            out strItemText,
                            out strBiblioText,
                            out strError);
                    }
                    finally
                    {
                        Program.MainForm.ReturnChannel(channel);
                    }
                    if (nRet == -1)
                    {
                        strError = "对册条码号 '" + strBarcode + "' 进行查重的过程中发生错误: " + strError;
                        goto ERROR1;
                    }
                    else if (nRet == 1) // 发生重复
                    {
                        EntityBarcodeFoundDupDlg dlg = new EntityBarcodeFoundDupDlg();
                        MainForm.SetControlFont(dlg, this.Font, false);
                        // dlg.MainForm = Program.MainForm;
                        dlg.BiblioText = strBiblioText;
                        dlg.ItemText = strItemText;
                        dlg.MessageText = "拟新增的册信息中，条码 '" + strBarcode + "' 在数据库中发现已经存在。";
                        Program.MainForm.AppInfo.LinkFormState(dlg, "EntityBarcodeFoundDupDlg_state");
                        dlg.ShowDialog(this);
                        return 0;
                    }
                }
            }

            BookItem bookitem = new BookItem();

            // 设置缺省值
            nRet = SetItemDefaultValues(
                "quickRegister_default",
                true,
                bookitem,
                out strError);
            if (nRet == -1)
            {
                strError = "设置缺省值的时候发生错误: " + strError;
                goto ERROR1;
            }

            bookitem.Barcode = strBarcode;

            // 2017/3/2
            if (string.IsNullOrEmpty(bookitem.RefID))
            {
                bookitem.RefID = Guid.NewGuid().ToString();
            }

            if (this.Items == null)
                this.Items = new BookItemCollection();

            Debug.Assert(this.Items != null, "");

            this.Items.Add(bookitem);
            bookitem.Parent = Global.GetRecordID(this.BiblioRecPath);
            bookitem.ItemDisplayState = ItemDisplayState.New;
            /* ListViewItem newitem = */
            bookitem.AddToListView(this.listView);
            bookitem.Changed = true;    // 否则“保存”按钮不能Enabled

            // 将刚刚加入的事项滚入可见范围
            //this.listView_items.EnsureVisible(this.listView_items.Items.IndexOf(newitem));
            bookitem.HilightListViewItem(true);

            this.EnableControls(true);
            return 1;
        ERROR1:
            MessageBox.Show(ForegroundWindow.Instance, strError);
            return -1;
        }

        // 撤销删除一个或多个实体
        void menu_undoDeleteEntity_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要撤销删除的事项");
                return;
            }

            this.EnableControls(false);

            try
            {

                /*
                string strBarcodeList = "";
                for (int i = 0; i < this.listView_items.SelectedItems.Count; i++)
                {
                    if (i > 20)
                    {
                        strBarcodeList += "...(共 " + this.listView_items.SelectedItems.Count.ToString() + " 项)";
                        break;
                    }
                    string strBarcode = this.listView_items.SelectedItems[i].Text;
                    strBarcodeList += strBarcode + "\r\n";
                }

                string strWarningText = "以下(条码)册将被撤销删除: \r\n" + strBarcodeList + "\r\n\r\n确实要撤销删除它们?";

                // 警告
                DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                    strWarningText,
                    "EntityForm",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return;
                 * */

                // 实行删除
                List<ListViewItem> selectedItems = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView.SelectedItems)
                {
                    selectedItems.Add(item);
                }

                string strNotUndoList = "";
                int nUndoCount = 0;
                foreach (ListViewItem item in selectedItems)
                {
                    BookItem bookitem = (BookItem)item.Tag;

                    bool bRet = this.Items.UndoMaskDeleteItem(bookitem);

                    if (bRet == false)
                    {
                        if (strNotUndoList != "")
                            strNotUndoList += ",";
                        strNotUndoList += bookitem.Barcode;
                        continue;
                    }

                    nUndoCount++;
                }

                string strText = "";

                if (strNotUndoList != "")
                    strText += "条码为 '" + strNotUndoList + "' 的事项先前并未被标记删除过, 所以现在谈不上撤销删除。\r\n\r\n";

                strText += "共撤销删除 " + nUndoCount.ToString() + " 项。";
                MessageBox.Show(ForegroundWindow.Instance, strText);

            }
            finally
            {
                this.EnableControls(true);
            }
        }

        // 删除一个或多个实体
        void menu_deleteEntity_Click(object sender, EventArgs e)
        {
            if (this.listView.SelectedIndices.Count == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "尚未选择要标记删除的事项");
                return;
            }

            string strBarcodeList = "";
            for (int i = 0; i < this.listView.SelectedItems.Count; i++)
            {
                ListViewItem item = this.listView.SelectedItems[i];
                if (i > 20)
                {
                    strBarcodeList += "...(共 " + this.listView.SelectedItems.Count.ToString() + " 项)";
                    break;
                }
                BookItem bookitem = (BookItem)item.Tag;

                string strBarcode = bookitem.Barcode;
                if (String.IsNullOrEmpty(strBarcode) == true)
                    strBarcode = bookitem.RecPath;
                if (String.IsNullOrEmpty(strBarcode) == true)
                    strBarcode = bookitem.RefID;

                strBarcodeList += strBarcode + "\r\n";
            }

            string strWarningText = "以下册将被标记删除: \r\n" + strBarcodeList + "\r\n\r\n确实要标记删除它们?";

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
                    BookItem bookitem = (BookItem)item.Tag;

                    int nRet = MaskDeleteItem(bookitem,
                        m_bRemoveDeletedItem);

                    if (nRet == 0)
                    {
                        if (strNotDeleteList != "")
                            strNotDeleteList += ",";
                        strNotDeleteList += bookitem.Barcode;
                        continue;
                    }

                    if (string.IsNullOrEmpty(bookitem.RecPath) == false)
                        deleted_recpaths.Add(bookitem.RecPath);

                    /*
                    if (String.IsNullOrEmpty(bookitem.Borrower) == false)
                    {
                        if (strNotDeleteList != "")
                            strNotDeleteList += ",";
                        strNotDeleteList += bookitem.Barcode;
                        continue;
                    }

                    this.bookitems.MaskDeleteItem(m_bRemoveDeletedItem,
                        bookitem);
                     * */
                    nDeleteCount++;
                }

                string strText = "";

                if (strNotDeleteList != "")
                    strText += "条码为 '" + strNotDeleteList + "' 的册包含有流通信息, 未能加以标记删除。\r\n\r\n";

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
            }
            finally
            {
                this.EnableControls(true);
            }
        }



        // 检索册条码号。用于新条码号查重。
        int SearchEntityBarcode(
            LibraryChannel channel,
            string strBarcode,
            out string strItemText,
            out string strBiblioText,
            out string strError)
        {
            strError = "";
            strItemText = "";
            strBiblioText = "";

#if NO
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在对册条码号 '" + strBarcode + "' 进行查重 ...");
            Stop.BeginLoop();
#endif
            Stop.SetMessage("正在对册条码号 '" + strBarcode + "' 进行查重 ...");

            try
            {
                long lRet = channel.GetItemInfo(
                    Stop,
                    strBarcode,
                    "html",
                    out strItemText,
                    "html",
                    out strBiblioText,
                    out strError);
                if (lRet == -1)
                    return -1;  // error

                if (lRet == 0)
                    return 0;   // not found
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

#if NO
        // 册条码号查重。用于(可能是)旧条码号查重。
        // 本函数可以自动排除和当前路径strOriginRecPath重复之情形
        // parameters:
        //      strBarcode  册条码号。
        //      strOriginRecPath    出发记录的路径。
        //      paths   所有命中的路径
        // return:
        //      -1  error
        //      0   not dup
        //      1   dup
        int SearchEntityBarcodeDup(string strBarcode,
            string strOriginRecPath,
            out string[] paths,
            out string strError)
        {
            strError = "";
            paths = null;

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在对册条码号 '" + strBarcode + "' 进行查重 ...");
            stop.BeginLoop();

            try
            {
                long lRet = Channel.SearchItemDup(
                    stop,
                    strBarcode,
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
                        strError = "系统错误: SearchItemDup() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
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
        int SearchEntityBarcodeDup(
            LibraryChannel channel,
            string strBarcode,
    string strOriginRecPath,
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

#if NO
            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在对册条码号 '" + strBarcode + "' 进行查重 ...");
            Stop.BeginLoop();
#endif
            Stop.SetMessage("正在对册条码号 '" + strBarcode + "' 进行查重 ...");

            try
            {
                long lRet = channel.SearchItem(
    Stop,
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

                lRet = channel.GetSearchResult(Stop,
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
                        strError = "系统错误: SearchItem() API返回值为1，但是paths数组的尺寸却不是1, 而是 " + paths.Length.ToString();
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


        // 
        /// <summary>
        /// 根据册条码号加亮事项
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <param name="bClearOtherSelection">是否清除其它事项的选择状态</param>
        public void HilightLine(string strBarcode,
                bool bClearOtherSelection)
        {
            if (bClearOtherSelection == true)
            {
                this.listView.SelectedItems.Clear();
            }

            if (this.Items != null)
            {
                BookItem bookitem = this.Items.GetItemByBarcode(strBarcode);
                if (bookitem != null)
                    bookitem.HilightListViewItem(true);
            }
        }

        // 标记删除事项
        // return:
        //      0   因为有流通信息，未能标记删除
        //      1   成功删除
        /// <summary>
        /// 标记删除事项
        /// </summary>
        /// <param name="bookitem">事项</param>
        /// <param name="bRemoveDeletedItem">是否从 ListView 中移走事项显示</param>
        /// <returns>0: 因为有流通信息，未能标记删除; 1: 成功删除</returns>
        public override int MaskDeleteItem(BookItem bookitem,
            bool bRemoveDeletedItem = false)
        {
            if (String.IsNullOrEmpty(bookitem.Borrower) == false)
                return 0;

            this.Items.MaskDeleteItem(bRemoveDeletedItem,
                bookitem);
            return 1;
        }



#if NOOOOOOOOOOOOOOOO
        // 在this.bookitems中定位和dom关联的事项
        // 顺次根据 记录路径 -- 条码 -- 登录号 来定位
        int LocateBookItem(
            string strRecPath,
            XmlDocument dom,
            out BookItem bookitem,
            out string strBarcode,
            out string strRegisterNo,
            out string strError)
        {
            strError = "";
            bookitem = null;
            strBarcode = "";
            strRegisterNo = "";

            // 提前获取, 以便任何返回路径时, 都可以得到这些值
            strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            strRegisterNo = DomUtil.GetElementText(dom.DocumentElement, "registerNo");

            if (String.IsNullOrEmpty(strRecPath) == false)
            {
                bookitem = this.bookitems.GetItemByRecPath(strRecPath);

                if (bookitem != null)
                    return 1;   // found

            }

            if (String.IsNullOrEmpty(strBarcode) == false)
            {
                bookitem = this.bookitems.GetItemByBarcode(strBarcode);

                if (bookitem != null)
                    return 1;   // found

            }

            if (String.IsNullOrEmpty(strRegisterNo) == false)
            {
                bookitem = this.bookitems.GetItemByRegisterNo(strRegisterNo);

                if (bookitem != null)
                    return 1;   // found
            }

            return 0;
        }
#endif

#if NO
        // 构造事项称呼
        static string GetLocationSummary(string strBarcode,
            string strRegisterNo,
            string strRecPath,
            string strRefID)
        {
            if (String.IsNullOrEmpty(strBarcode) == false)
                return "条码为 '" + strBarcode + "' 的事项";
            if (String.IsNullOrEmpty(strRegisterNo) == false)
                return "登录号为 '" + strRegisterNo + "' 的事项";
            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            // 2008/6/24 
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";

            return "无任何定位信息的事项";
        }
#endif


        // 构造事项称呼
        internal override string GetLocationSummary(BookItem bookitem)
        {
            string strBarcode = bookitem.Barcode;

            if (String.IsNullOrEmpty(strBarcode) == false)
                return "条码为 '" + strBarcode + "' 的事项";

            string strRegisterNo = bookitem.RegisterNo;

            if (String.IsNullOrEmpty(strRegisterNo) == false)
                return "登录号为 '" + strRegisterNo + "' 的事项";

            string strRecPath = bookitem.RecPath;

            if (String.IsNullOrEmpty(strRecPath) == false)
                return "记录路径为 '" + strRecPath + "' 的事项";

            string strRefID = bookitem.RefID;
            // 2008/6/24 
            if (String.IsNullOrEmpty(strRefID) == false)
                return "参考ID为 '" + strRefID + "' 的事项";

            return "无任何定位信息的事项";
        }

        /// <summary>
        /// 根据册条码号 检索出 书目记录 和全部下属册，装入窗口
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchEntity(string strBarcode)
        {
            BookItem result_item = null;
            return this.DoSearchItem("",
                strBarcode,
                out result_item,
                true);
        }

#if NO
        // 
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册条码号 检索出 书目记录 和全部下属册，装入窗口
        /// </summary>
        /// <param name="strBarcode">册条码号</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchEntity(string strBarcode)
        {
            int nRet = 0;
            string strError = "";
            // 先检查是否已在本窗口中?

            // 对当前窗口内进行条码查重
            if (this.Items != null)
            {
                BookItem dupitem = this.Items.GetItemByBarcode(strBarcode);
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "册条码号 '" + strBarcode + "' 正好为本种中未提交之一删除册请求。";
                    else
                        strText = "册条码号 '" + strBarcode + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            string strConfirmItemRecPath = "";
        // 向服务器提交检索请求

            REDO:

            string strBiblioRecPath = "";
            string strItemRecPath = "";

            string strSearchText = "";

            if (String.IsNullOrEmpty(strConfirmItemRecPath) == true)
                strSearchText = strBarcode;
            else
                strSearchText = "@path:" + strConfirmItemRecPath;


            // 检索册条码号，检索出其从属的书目记录路径。
            nRet = SearchTwoRecPathByBarcode(strSearchText,
                out strItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对册条码号 '" + strBarcode + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到包含册条码号 '" + strBarcode + "' 的记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上条码事项
                HilightLine(strBarcode, true);
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                /*
                string strText = "条码 '" + strBarcode + "' 在数据库中发现已经被下列多条册记录所使用。\r\n" + strItemRecPath + "\r\n\r\n请联系系统管理员尽快纠正此数据错误。";
                MessageBox.Show(ForegroundWindow.Instance, strText);
                return -1;
                 * */
                Program.MainForm.PrepareSearch();

                try
                {
                    ItemBarcodeDupDlg dupdlg = new ItemBarcodeDupDlg();
                    // 此时EntityForm的字体还没有初始化
                    MainForm.SetControlFont(dupdlg, Program.MainForm.DefaultFont, false);
                    string strErrorNew = "";
                    string[] aDupPath = strItemRecPath.Split(new char[] { ',' });
                    nRet = dupdlg.Initial(
                        Program.MainForm,
                        aDupPath,
                        "条码 '" + strBarcode + "' 在数据库中发现已经被下列多条册记录所使用。这个问题需要尽快纠正。\r\n\r\n可根据下面列出的详细信息，选择适当的册记录，重试操作。",
                        Program.MainForm.Channel,
                        Program.MainForm.Stop,
                        out strErrorNew);
                    if (nRet == -1)
                    {
                        // 初始化对话框失败
                        MessageBox.Show(ForegroundWindow.Instance, strErrorNew);
                        goto ERROR1;
                    }

                    Program.MainForm.AppInfo.LinkFormState(dupdlg, "ChargingForm_dupdlg_state");
                    dupdlg.ShowDialog(this);
                    Program.MainForm.AppInfo.UnlinkFormState(dupdlg);

                    if (dupdlg.DialogResult == DialogResult.Cancel)
                    {
                        strError = "条码 '" + strBarcode + "' 在数据库中发现已经被下列多条册记录所使用。\r\n" + strItemRecPath + "\r\n\r\n请联系系统管理员尽快纠正此数据错误。";
                        goto ERROR1;
                    }

                    strConfirmItemRecPath = dupdlg.SelectedRecPath;

                    goto REDO;
                }
                finally
                {
                    Program.MainForm.EndSearch();
                }


            }

            return 0;
        ERROR1:
            return -1;
        }

#endif
#if NO
        // 2008/11/2
        // 
        // parameters:
        //      strItemBarcode  [out]返回册记录的册条码号
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册记录路径 检索出 书目记录 和全部下属册，装入窗口
        /// </summary>
        /// <param name="strItemRecPath">册记录路径</param>
        /// <param name="strItemBarcode">返回册记录的册条码号</param>
        /// <param name="bDisplayWarning">是否显示警告信息</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchEntityByRecPath(string strItemRecPath,
            out string strItemBarcode,
            bool bDisplayWarning = true)
        {
            strItemBarcode = "";

            int nRet = 0;
            string strError = "";
            // 先检查是否已在本窗口中?
            // 对当前窗口内进行册记录路径查重
            if (this.Items != null)
            {
                BookItem dupitem = this.Items.GetItemByRecPath(strItemRecPath) as BookItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "册记录 '" + strItemRecPath + "' 正好为本种中未提交之一删除册请求。";
                    else
                        strText = "册记录 '" + strItemRecPath + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    if (bDisplayWarning == true)
                        MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

        // 向服务器提交检索请求
            string strBiblioRecPath = "";
            string strOutputItemRecPath = "";

            string strSearchText = "";

            strSearchText = "@path:" + strItemRecPath;

            // 根据册记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchTwoRecPathByBarcode(strSearchText,
                out strOutputItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对册记录路径 '" + strItemRecPath + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到路径为 '" + strItemRecPath + "' 的册记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上条码事项
                BookItem result_item = HilightLineByItemRecPath(strItemRecPath, true);
                if (result_item != null)
                    strItemBarcode = result_item.Barcode;
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用册记录路径检索绝对不会发生重复现象");
            }

            return 0;
            /*
        ERROR1:
            return -1;
             * */
        }
#endif

#if NO
        // 2010/2/26 
        // 
        // parameters:
        //      strItemBarcode  [out]返回册记录的册条码号
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        /// <summary>
        /// 根据册记录参考ID 检索出 书目记录 和全部下属册，装入窗口
        /// </summary>
        /// <param name="strItemRefID">册记录的参考 ID</param>
        /// <param name="strItemBarcode">返回册记录的册条码号</param>
        /// <returns>-1: 出错; 0: 没有找到; 1: 成功</returns>
        public int DoSearchEntityByRefID(string strItemRefID,
            out string strItemBarcode)
        {
            strItemBarcode = "";

            int nRet = 0;
            string strError = "";

            // 先检查是否已在本窗口中?
            // 对当前窗口内进行册记录参考 ID 查重
            if (this.Items != null)
            {
                BookItem dupitem = this.Items.GetItemByRefID(strItemRefID) as BookItem;
                if (dupitem != null)
                {
                    string strText = "";
                    if (dupitem.ItemDisplayState == ItemDisplayState.Deleted)
                        strText = "册记录 '" + strItemRefID + "' 正好为本种中未提交之一删除册请求。";
                    else
                        strText = "册记录 '" + strItemRefID + "' 在本种中找到。";

                    dupitem.HilightListViewItem(true);

                    MessageBox.Show(ForegroundWindow.Instance, strText);
                    return 1;
                }
            }

            // 向服务器提交检索请求
            string strBiblioRecPath = "";
            string strOutputItemRecPath = "";

            string strSearchText = "";

            strSearchText = "@refID:" + strItemRefID;

            // 根据册记录路径检索，检索出其从属的书目记录路径。
            nRet = SearchTwoRecPathByBarcode(strSearchText,
                out strOutputItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(ForegroundWindow.Instance, "对册记录的参考ID '" + strItemRefID + "' 进行检索的过程中发生错误: " + strError);
                return -1;
            }
            else if (nRet == 0)
            {
                MessageBox.Show(ForegroundWindow.Instance, "没有找到路径为 '" + strItemRefID + "' 的册记录。");
                return 0;
            }
            else if (nRet == 1)
            {
                Debug.Assert(strBiblioRecPath != "", "");
                this.TriggerLoadRecord(strBiblioRecPath);

                // 选上条码事项
                BookItem result_item = HilightLineByItemRefID(strItemRefID, true);
                if (result_item != null)
                    strItemBarcode = result_item.Barcode;
                return 1;
            }
            else if (nRet > 1) // 命中发生重复
            {
                Debug.Assert(false, "用册记录参考ID检索应当不会发生重复现象");
                MessageBox.Show(ForegroundWindow.Instance, "用参考ID '"+strItemRefID+"' 检索命中多于一条，为 "+nRet.ToString()+" 条");
                return -1;
            }

            return 0;
            /*
        ERROR1:
            return -1;
             * */
        }

#endif

        private void listView_items_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;
            this.SortColumns.SetFirstColumn(nClickColumn,
                this.listView.Columns);

            // 排序
            this.listView.ListViewItemSorter = new SortColumnsComparer(this.SortColumns);

            this.listView.ListViewItemSorter = null;
        }

        // 
        // 2009/10/12 
        // parameters:
        //      strPublishTime  出版时间，8字符。
        //                      如果为"*"，表示统配任意出版时间均可
        //                      如果为"<range>"，表示匹配范围型的出版时间字符串，例如"20090101-20091231"
        //                      如果为"<single>"，表示匹配单点型的出版时间字符串，例如"20090115"
        //                      如果为"refids:"引导的字符串，表示要根据refid列表获取若干记录
        /// <summary>
        /// 根据出版时间，匹配“时间范围”符合的册记录
        /// </summary>
        /// <param name="strPublishTime">出版时间，8字符。
        /// <para>如果为"*"，表示统配任意出版时间均可</para>
        /// <para>如果为"&lt;range&gt;"，表示匹配范围型的出版时间字符串，例如"20090101-20091231"</para>
        /// <para>如果为"&lt;single&gt;"，表示匹配单点型的出版时间字符串，例如"20090115"</para>
        /// <para>如果为"refids:"引导的字符串，表示要根据refid列表获取若干记录</para>
        /// </param>
        /// <param name="XmlRecords">返回 XML 字符串集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetItemInfoByPublishTime(string strPublishTime,
            out List<string> XmlRecords,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            XmlRecords = new List<string>();

            if (this.Items == null)
                return 0;

            if (StringUtil.HasHead(strPublishTime, "refids:") == true)
            {
                string strList = strPublishTime.Substring("refids:".Length);
                string[] parts = strList.Split(new char[] { ',' });
                for (int i = 0; i < parts.Length; i++)
                {
                    string strRefID = parts[i];
                    if (String.IsNullOrEmpty(strRefID) == true)
                        continue;

                    BookItem item = this.Items.GetItemByRefID(strRefID) as BookItem;
                    if (item == null)
                    {
                        XmlRecords.Add(null);   // 表示没有找到，但是也占据一个位置
                        continue;
                    }

                    string strItemXml = "";
                    nRet = item.BuildRecord(
                        true,   // 要检查 Parent 成员
                        out strItemXml,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    XmlRecords.Add(strItemXml);
                }
                return 0;
            }

            foreach (BookItem item in this.Items)
            {
                // BookItem item = this.BookItems[i];

                if (item.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    continue;
                }

                // 星号表示通配
                if (strPublishTime == "*")
                {
                }
                else if (strPublishTime == "<single>")
                {
                    nRet = item.PublishTime.IndexOf("-");
                    if (nRet != -1)
                        continue;
                }
                else if (strPublishTime == "<range>")
                {
                    nRet = item.PublishTime.IndexOf("-");
                    if (nRet == -1)
                        continue;
                }
                else
                {
                    /*
                    try
                    {
                        if (Global.InRange(strPublishTime, item.Range) == false)
                            continue;
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        return -1;
                    }
                     * */
                    // TODO: jianglai keneng chuxian fanwei zai item.PublishTime
                    // TODO: shi fou yao paichu hedingben?
                    if (strPublishTime != item.PublishTime)
                        continue;
                }

                string strItemXml = "";
                nRet = item.BuildRecord(
                    true,   // 要检查 Parent 成员
                    out strItemXml,
                    out strError);
                if (nRet == -1)
                    return -1;

                XmlRecords.Add(strItemXml);
            }

            return 1;
        }

#if NO
        // 为BookItem对象设置缺省值
        // parameters:
        //      strCfgEntry 为"normalRegister_default"或"quickRegister_default"
        int SetBookItemDefaultValues(
            string strCfgEntry,
            BookItem bookitem,
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

            int nRet = bookitem.SetData("",
                strNewDefault,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            bookitem.Parent = "";
            bookitem.RecPath = "";

            return 0;
        }
#endif

#if NO
        // 可以使用 SearchBiblioRecPath()
        // 根据册条码号，检索出其册记录路径和从属的书目记录路径。
        int SearchTwoRecPathByBarcode(string strBarcode,
            out string strItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            strItemRecPath = "";


            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            Stop.OnStop += new StopEventHandler(this.DoStop);
            Stop.Initial("正在检索册条码号 '" + strBarcode + "' 所从属的书目记录路径 ...");
            Stop.BeginLoop();

            try
            {
                long lRet = Channel.GetItemInfo(
                    Stop,
                    strBarcode,
                    null,
                    out strItemText,
                    out strItemRecPath,
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

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control == true)
            {
                // Ctrl+A
                menu_generateData_Click(sender, null);
            }
        }

        // 选定(加亮)items事项中符合指定批次号的那些行
        /// <summary>
        /// 选定(加亮) Items 中匹配指定批次号的那些事项
        /// </summary>
        /// <param name="strBatchNo">批次号</param>
        /// <param name="bClearOthersHilight">同时清除其它事项的加亮状态</param>
        public void SelectItemsByBatchNo(string strBatchNo,
            bool bClearOthersHilight)
        {
            this.Items.SelectItemsByBatchNo(strBatchNo,
                bClearOthersHilight);
        }

        /// <summary>
        /// 追加一个新的 册 记录
        /// 也可以直接使用 EntityControlBase.AppendItem()
        /// </summary>
        /// <param name="item">要追加的事项</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int AppendEntity(BookItem item,
            out string strError)
        {
            return this.AppendItem(item, out strError);
        }

    }

    /// <summary>
    /// 获得各种参数值
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetParameterValueHandler(object sender,
        GetParameterValueEventArgs e);

    /// <summary>
    /// 获得各种参数值事件 GetParameterValueHandler 的参数
    /// </summary>
    public class GetParameterValueEventArgs : EventArgs
    {
        /// <summary>
        /// 参数名
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 参数值
        /// </summary>
        public string Value = "";
    }

    /// <summary>
    /// 校验条码
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void VerifyBarcodeHandler(object sender,
        VerifyBarcodeEventArgs e);

    /// <summary>
    /// 校验条码号事件 VerifyBarcodeHandler 的参数
    /// </summary>
    public class VerifyBarcodeEventArgs : EventArgs
    {
        /// <summary>
        /// 馆代码
        /// </summary>
        public string LibraryCode = "";
        /// <summary>
        /// 条码号
        /// </summary>
        public string Barcode = "";
        /// <summary>
        /// [out]出错信息。
        /// </summary>
        public string ErrorInfo = "";

        // return:
        //      -2  服务器没有配置校验方法，无法校验
        //      -1  error
        //      0   不是合法的条码号
        //      1   是合法的读者证条码号
        //      2   是合法的册条码号
        /// <summary>
        /// 返回值
        /// <para>      -2  服务器没有配置校验方法，无法校验</para>
        /// <para>      -1  出错</para>
        /// <para>      0   不是合法的条码号</para>
        /// <para>      1   是合法的读者证条码号</para>
        /// <para>      2   是合法的册条码号</para>
        /// </summary>
        public int Result = -2;
    }

    /// <summary>
    /// 用于剪贴板的 BookItem 集合
    /// </summary>
    [Serializable()]
    public class ClipboardBookItemCollection : List<BookItem>
    {
#if NO
        ArrayList m_list = new ArrayList();

        /// <summary>
        /// 追加一个对象
        /// </summary>
        /// <param name="bookitem">BookItem 对象</param>
        public void Add(BookItem bookitem)
        {
            this.m_list.Add(bookitem);
        }


        public BookItem this[int nIndex]
        {
            get
            {
                return (BookItem)m_list[nIndex];
            }
            set
            {
                m_list[nIndex] = value;
            }
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }
#endif

        // 
        /// <summary>
        /// 恢复那些不能序列化的成员值
        /// </summary>
        public void RestoreNonSerialized()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].RestoreNonSerialized();
            }
        }
    }


    // 
    /// <summary>
    /// 修改实体(册)事项的事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ChangeItemEventHandler(object sender,
        ChangeItemEventArgs e);

    /// <summary>
    /// 修改册事项事件 的参数
    /// </summary>
    public class ChangeItemEventArgs : EventArgs
    {
        // [in]
        /// <summary>
        /// [in] 是否为期刊模式
        /// </summary>
        public bool SeriesMode = false; // 是否为期刊模式
        // [in] 
        /// <summary>
        /// [in] 输入的册条码号
        /// </summary>
        public bool InputItemBarcode = true;
        // [in]
        /// <summary>
        /// [in] 是否要为新的册创建索取号
        /// </summary>
        public bool CreateCallNumber = false;   // 为新的册创建索取号

        // [in]
        /// <summary>
        /// [in] 数据列表
        /// </summary>
        public List<ChangeItemData> DataList = new List<ChangeItemData>();

        // [out]
        /// <summary>
        /// [out] 出错信息。如果为空，表示没有出错
        /// </summary>
        public string ErrorInfo = "";   // [out]如果为非空，表示执行过程出错，这里是出错信息
        // 2010/4/15
        // [out]
        /// <summary>
        /// [out] 警告信息。如果为空，表示没有警告
        /// </summary>
        public string WarningInfo = "";   // [out]如果为非空，表示执行过程出现警告，这里是警告信息
    }

    /// <summary>
    /// 一个数据存储单元
    /// 用于 ChangeItemEventArgs 类
    /// </summary>
    public class ChangeItemData
    {
        /// <summary>
        /// 动作。为 new/delete/change/neworchange 之一
        /// </summary>
        public string Action = "";  // new/delete/change/neworchange
        /// <summary>
        /// 参考 ID
        /// </summary>
        public string RefID = "";   // 参考ID。保持信息联系的一个唯一性ID值
        /// <summary>
        /// 册记录 XML 
        /// </summary>
        public string Xml = ""; // 实体记录XML
        /// <summary>
        /// [out] 出错信息。如果为空，表示没有出错
        /// </summary>
        public string ErrorInfo = "";   // [out]如果为非空，表示执行过程出错，这里是出错信息
        // 2010/4/15
        /// <summary>
        /// [out] 警告信息。如果为空，表示没有警告
        /// </summary>
        public string WarningInfo = "";   // [out]如果为非空，表示执行过程出现警告，这里是警告信息

        // 2010/12/1
        /// <summary>
        /// 套序。例如“1/7”
        /// </summary>
        public string Sequence = "";    // 套序。例如“1/7”
        /// <summary>
        /// 候选的其他价格。格式为: "订购价:CNY12.00;验收价:CNY15.00"
        /// </summary>
        public string OtherPrices = ""; // 候选的其他价格。格式为: "订购价:CNY12.00;验收价:CNY15.00"

    }

    // 如果不这样书写，视图设计器会出现故障
    /// <summary>
    /// EntityControl 类的基础类
    /// </summary>
    public class EntityControlBase : ItemControlBase<BookItem, BookItemCollection>
    {
    }

}
