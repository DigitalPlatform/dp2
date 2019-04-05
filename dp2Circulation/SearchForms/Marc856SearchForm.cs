
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
// using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 856 字段检索窗
    /// </summary>
    public partial class Marc856SearchForm : MyForm
    {
        Hashtable m_biblioTable = new Hashtable(); // 书目记录路径 --> 书目信息

        public Marc856SearchForm()
        {
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_records.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(COLUMN_RECPATH, ColumnSortStyle.RecPath);
            prop.SetSortStyle(COLUMN_FIELDINDEX, ColumnSortStyle.RightAlign);
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_records.Tag;
            prop.ClearCache();
        }

        private void Marc856SearchForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                this.UiState = Program.MainForm.AppInfo.GetString(
                    "marc856searchform", 
                    "uistate", "");

            CreateColumns();
        }

        private void Marc856SearchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.m_nChangedCount > 0)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 " + m_nChangedCount + " 项修改尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "Marc856SearchForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    // return;
                }
            }
        }

        private void Marc856SearchForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.m_commentViewer != null)
            {
                this.m_commentViewer.ExitWebBrowser();  // 虽然 CommentViwerForm 的 Dispose() 里面也作了释放，但为了保险起见，这里也释放一次
                this.m_commentViewer.Close();
            }

            if (Program.MainForm != null && Program.MainForm.AppInfo != null)
                Program.MainForm.AppInfo.SetString("marc856searchform",
                    "uistate",
                    this.UiState);
        }

        // 新加入一行
        public ListViewItem AddLine(
            string strBiblioRecPath,
            BiblioInfo biblio_info,
            MarcField field,
            int index)
        {
            // 将 BiblioInfo 储存起来
            BiblioInfo existing = (BiblioInfo)this.m_biblioTable[strBiblioRecPath];
            if (existing == null)
                this.m_biblioTable[strBiblioRecPath] = biblio_info;

            ListViewItem item = new ListViewItem();
            SetItemColumns(item, strBiblioRecPath, field, index);
            this.listView_records.Items.Add(item);

            LineInfo line_info = new LineInfo();
            line_info.OldField = field.Text;
            line_info.Index = index;
            item.Tag = line_info;
            return item;
        }

        // 设置 ListViewItem 的各列
        void SetItemColumns(ListViewItem item, 
            string strBiblioRecPath,
            MarcField field, 
            int index)
        {
            // 设置各列内容
            string u = field.select("subfield[@name='u']").FirstContent;
            string x = field.select("subfield[@name='x']").FirstContent;

            // 读取参数的时候，不在意内部顺序问题
            Hashtable table = StringUtil.ParseParameters(x, ';', ':');
            string strType = (string)table["type"];
            string strRights = (string)table["rights"];
            string strSize = (string)table["size"];
            string strSource = (string)table["source"];

            if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strBiblioRecPath);

            ListViewUtil.ChangeItemText(item, COLUMN_FIELDINDEX, index.ToString());

            ListViewUtil.ChangeItemText(item, COLUMN_INDICATOR1, field.Indicator1.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_INDICATOR2, field.Indicator2.ToString());

            ListViewUtil.ChangeItemText(item, COLUMN_URL, u);
            ListViewUtil.ChangeItemText(item, COLUMN_F,
                field.select("subfield[@name='f']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_Q,
    field.select("subfield[@name='q']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_S,
    field.select("subfield[@name='s']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_Y,
    field.select("subfield[@name='y']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_Z,
    field.select("subfield[@name='z']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_X,
    x);

            ListViewUtil.ChangeItemText(item, COLUMN_RIGHTS, strRights);
            ListViewUtil.ChangeItemText(item, COLUMN_SIZE, strSize);
            ListViewUtil.ChangeItemText(item, COLUMN_SOURCE, strSource);
            ListViewUtil.ChangeItemText(item, COLUMN_TYPE, strType);

            ListViewUtil.ChangeItemText(item, COLUMN_2,
    field.select("subfield[@name='2']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_3,
    field.select("subfield[@name='3']").FirstContent);
            ListViewUtil.ChangeItemText(item, COLUMN_8,
    field.select("subfield[@name='8']").FirstContent);
        }

        const int COLUMN_RECPATH = 0;   // 书目记录路径
        const int COLUMN_SUMMARY = 1;   // 书目摘要
        const int COLUMN_FIELDINDEX = 2;    // 字段序号
        const int COLUMN_INDICATOR1 = 3;    // 指示符1
        const int COLUMN_INDICATOR2 = 4;    // 指示符2

        const int COLUMN_URL = 5;       // $u URL

        const int COLUMN_F = 6;         // $f 电子文件名
        const int COLUMN_Q = 7;         // $q 电子格式类型
        const int COLUMN_S = 8;         // $s 文件大小
        const int COLUMN_Y = 9;         // $y 链接文字
        const int COLUMN_Z = 10;         // $z 公开注释

        const int COLUMN_X = 11;         // $x 内部注释

        const int COLUMN_RIGHTS = 12;    // $x.rights
        const int COLUMN_SIZE = 13;      // $x.size
        const int COLUMN_SOURCE = 14;    // $x.source
        const int COLUMN_TYPE = 15;      // $x.type

        const int COLUMN_2 = 16;         // $2 其它接入方式
        const int COLUMN_3 = 17;         // $3 特定部分说明
        const int COLUMN_8 = 18;         // $8 字段关联或序号


        void CreateColumns()
        {
            string[] titles = new string[] {
                "书目摘要",
                "字段序号",
                "指示符1",
                "指示符2",
                "$u URL",
        "$f 电子文件名",
        "$q 电子格式类型",
        "$s 文件大小",
        "$y 链接文字",
        "$z 公开注释",
        "$x 内部注释",
                "$x rights 权限",
                "$x size 尺寸",
                "$x source 来源",
        "$x type 类型",
        "$2 其它接入方式",
        "$3 特定部分说明",
        "$8 字段关联或序号",

            };
            int i = 1;
            foreach (string title in titles)
            {
                ColumnHeader header = new ColumnHeader();

                if (i < this.listView_records.Columns.Count)
                    header = this.listView_records.Columns[i];
                else
                {
                    header = new ColumnHeader();
                    header.Width = 100;
                    this.listView_records.Columns.Add(header);
                }

                header.Text = title;
                i++;
            }
        }

        // parameters:
        //      lStartIndex 调用前已经做过的事项数。为了准确显示 Progress
        // return:
        //      -2  获得书目摘要的权限不够
        //      -1  出错
        //      0   用户中断
        //      1   完成
        public int FillBiblioSummaryColumn(
            LibraryChannel channel,
            List<ListViewItem> items,
            long lStartIndex,
            bool bDisplayMessage,
            bool bPrepareLoop,
            out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (bPrepareLoop)
            {
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在填充书目摘要 ...");
                stop.BeginLoop();
            }

            try
            {
                stop.SetProgressRange(0, items.Count);

                List<string> biblio_recpaths = new List<string>();  // 尺寸可能比 items 数组小，没有包含里面不具有 parent id 列的事项
                // List<int> colindex_list = new List<int>();  // 存储每个 item 对应的 parent id colindex。数组大小等于 items 数组大小
                foreach (ListViewItem item in items)
                {
                    string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
                    biblio_recpaths.Add(strBiblioRecPath);
                }

                CacheableBiblioLoader loader = new CacheableBiblioLoader();
                loader.Channel = channel;   //  this.Channel;
                loader.Stop = this.stop;
                loader.Format = "summary";
                loader.GetBiblioInfoStyle = GetBiblioInfoStyle.None;
                loader.RecPaths = biblio_recpaths;

                var enumerator = loader.GetEnumerator();

                int i = 0;
                foreach (ListViewItem item in items)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return 0;
                    }

                    string strRecPath = ListViewUtil.GetItemText(item, 0);
                    if (stop != null && bDisplayMessage == true)
                    {
                        stop.SetMessage("正在刷新浏览行 " + strRecPath + " 的书目摘要 ...");
                        stop.SetProgressValue(lStartIndex + i);
                    }

                    try
                    {
                        bool bRet = enumerator.MoveNext();
                        if (bRet == false)
                        {
                            Debug.Assert(false, "还没有到结尾, MoveNext() 不应该返回 false");
                            // TODO: 这时候也可以采用返回一个带没有找到的错误码的元素
                            strError = "error 1";
                            return -1;
                        }
                    }
                    catch (ChannelException ex)
                    {
                        strError = ex.Message;
                        if (ex.ErrorCode == ErrorCode.AccessDenied)
                            return -2;
                        return -1;
                    }

                    BiblioItem biblio = (BiblioItem)enumerator.Current;
                    // Debug.Assert(biblio.RecPath == strRecPath, "m_loader 和 items 的元素之间 记录路径存在严格的锁定对应关系");

                    ListViewUtil.ChangeItemText(item,
                        1,
                        biblio.Content);
                    i++;
                    stop.SetProgressValue(i);
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "填充书目摘要的过程出现异常: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }
            finally
            {
                if (bPrepareLoop)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();
                }
            }
        }

        private void listView_records_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("删除 856 字段 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteSelectedItems_Click);
            if (this.listView_records.SelectedIndices.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("快速修改 856 字段 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&Q)");
            menuItem.Click += new System.EventHandler(this.menu_quickChange_Click);
            if (this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("丢弃修改 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearSelectedChangedRecords_Click);
            if (this.m_nChangedCount == 0 || this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("丢弃全部修改 [" + this.m_nChangedCount.ToString() + "] (&L)");
            menuItem.Click += new System.EventHandler(this.menu_clearAllChangedRecords_Click);
            if (this.m_nChangedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("保存选定的修改 [" + this.listView_records.SelectedItems.Count.ToString() + "] (&S)");
            menuItem.Click += new System.EventHandler(this.menu_saveSelectedChangedRecords_Click);
            if (this.m_nChangedCount == 0 || this.listView_records.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("保存全部修改 [" + this.m_nChangedCount.ToString() + "] (&A)");
            menuItem.Click += new System.EventHandler(this.menu_saveAllChangedRecords_Click);
            if (this.m_nChangedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_records, new Point(e.X, e.Y));
        }

        // 删除所选择的 856 字段
        void menu_deleteSelectedItems_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = DeleteItems(out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet != 0)
                this.ShowMessage(strError, "green", true);
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        int DeleteItems(out string strError)
        {
            strError = "";

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要删除的 856 字段事项";
                return -1;
            }

            DialogResult result = MessageBox.Show(this,
    "确实要删除所选定 "+this.listView_records.SelectedItems.Count+" 个 856 字段?",
    "Marc856SearchForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result == System.Windows.Forms.DialogResult.No)
                return 0;

            int nDeleteCount = 0;
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                LineInfo line_info = (LineInfo)item.Tag;
                if (line_info == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (IsItemChanged(item) == false)
                    this.m_nChangedCount++;

                line_info.State = "delete";
                line_info.NewField = "";    // 如果删除之前有内容修改，需要丢弃这些修改

                // 刷新显示
                item.BackColor = SystemColors.Info;
                item.ForeColor = Color.Red;

                nDeleteCount++;
            }

            DoViewComment(false);
            strError = "已标记删除 "+nDeleteCount+" 个事项，但并未兑现到书目库。\r\n当保存修改时才会真正从书目库中删除。";
            return 1;
        }

        // 丢弃选定的修改
        void menu_clearSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            if (this.m_nChangedCount == 0)
            {
                this.ShowMessage("当前没有任何修改过的事项可丢弃", "error", true);
                return;
            }

            this.ClearMessage();

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    ClearOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
        }

        // 丢弃全部修改
        void menu_clearAllChangedRecords_Click(object sender, EventArgs e)
        {
            if (this.m_nChangedCount == 0)
            {
                this.ShowMessage("当前没有任何修改过的事项可丢弃", "error", true);
                return;
            }

            this.ClearMessage();

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    ClearOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
        }

        // 保存全部修改事项
        void menu_saveAllChangedRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.m_nChangedCount == 0)
            {
                strError = "当前没有任何修改过的事项需要保存";
                goto ERROR1;
            }

            this.ClearMessage();

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.Items)
            {
                items.Add(item);
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            this.ShowMessage(strError, "green", true);
            return;
        ERROR1:
            this.ShowMessage(strError, "red", true);
        }

        void menu_saveSelectedChangedRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.m_nChangedCount == 0)
            {
                strError = "当前没有任何修改过的事项需要保存";
                goto ERROR1;
            }

            this.ClearMessage();

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this.listView_records.SelectedItems)
            {
                items.Add(item);
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            // MessageBox.Show(this, strError);
            this.ShowMessage(strError, "green", true);
            return;
        ERROR1:
            // MessageBox.Show(this, strError);
            this.ShowMessage(strError, "red", true);
        }

        int SaveChangedRecords(List<ListViewItem> items,
    out string strError)
        {
            strError = "";
            int nRet = 0;

            int nReloadCount = 0;
            int nSavedCount = 0;

            List<string> changed_biblio_recpaths = new List<string>();
            Hashtable relation_table = new Hashtable(); // 书目记录路径 --> List<ListViewItem>

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存书目记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            this.listView_records.Enabled = false;
            try
            {
                // 创建书目记录和册事项的对照关系。注意这是针对全部行的，不仅仅是已经选择的行。因为删除操作可能会涉及到没有选择的行的 index 列内容刷新显示
                foreach (ListViewItem item in this.listView_records.Items)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "已中断";
                        return -1;
                    }
                    string strBiblioRecPath = item.Text;
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                        continue;

                    {
                        // 记载关系
                        List<ListViewItem> array = (List<ListViewItem>)relation_table[strBiblioRecPath];
                        if (array == null)
                        {
                            array = new List<ListViewItem>();
                            relation_table[strBiblioRecPath] = array;
                        }
                        array.Add(item);
                    }
                }

                foreach (ListViewItem item in items)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "已中断";
                        return -1;
                    }

                    string strBiblioRecPath = item.Text;
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                        continue;

                    LineInfo line_info = (LineInfo)item.Tag;
                    if (line_info == null)
                    {
                        strError = "line_info == null";
                        return -1;
                    }

                    if (IsItemChanged(item) == false)
                        continue;

                    BiblioInfo biblio_info = (BiblioInfo)this.m_biblioTable[strBiblioRecPath];
                    if (biblio_info == null)
                    {
                        strError = "1 在 m_biblioTable 中没有找到路径为 '" + strBiblioRecPath + "' 的书目记录信息";
                        return -1;
                        // goto CONTINUE;
                    }

                    {
                        // 把对字段的修改合并到书目记录中
                        // return:
                        //      -1  出错
                        //      0   书目记录没有发生修改
                        //      1   书目记录发生了修改
                        nRet = MergeChange(line_info,
                            biblio_info,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        // 记载发生过修改的书目记录路径
                        if (nRet == 1)
                        {
                            if (changed_biblio_recpaths.IndexOf(strBiblioRecPath) == -1)
                                changed_biblio_recpaths.Add(strBiblioRecPath);
                        }
                    }

                    if (line_info.State == "delete")
                    {
                        // 把同一书目记录的删除位置以后的 856 字段的 index 都减去 1
                        nRet = ChangeFieldIndex(
                            relation_table,
                            item,
                            -1,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        this.listView_records.Items.Remove(item);
                        this.m_nChangedCount--;
                        Debug.Assert(this.m_nChangedCount >= 0, "");
                        continue;
                    }

                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;

                    this.m_nChangedCount--;
                    Debug.Assert(this.m_nChangedCount >= 0, "");

                    Debug.Assert(string.IsNullOrEmpty(line_info.NewField) == false, "");
                    if (string.IsNullOrEmpty(line_info.NewField) == false)
                    {
                        line_info.OldField = line_info.NewField;
                        line_info.NewField = "";
                    }

                    {
                        // 刷新浏览行的显示
                        MarcField field = new MarcField(line_info.OldField);
                        SetItemColumns(item, null, field, line_info.Index);
                    }
                }

                // TODO: 如果要优化算法的话，可以建立书目记录和浏览行之间的联系，在书目记录保存成功后才修改 line_info.NewField 和刷新浏览行显示。这样的好处是，一旦中途出错，还有干净重新保存的可能
                stop.SetProgressRange(0, changed_biblio_recpaths.Count);
                int i = 0;
                foreach(string strBiblioRecPath in changed_biblio_recpaths)
                {
                    BiblioInfo biblio_info = (BiblioInfo)this.m_biblioTable[strBiblioRecPath];
                    if (biblio_info == null)
                    {
                        strError = "2 在 m_biblioTable 中没有找到路径为 '" + strBiblioRecPath + "' 的书目记录信息";
                        return -1;
                        // goto CONTINUE;
                    }

                    if (string.IsNullOrEmpty(biblio_info.NewXml) == true)
                    {
                        Debug.Assert(false, "");
                        goto CONTINUE;
                    }

                    // 暂不处理外来记录的保存
                    // TODO: 此时警告不能保存?
                    if (biblio_info.RecPath.IndexOf("@") != -1)
                    {
                        Debug.Assert(false, "");
                        goto CONTINUE;
                    }

                    string strOutputPath = "";

                    stop.SetMessage("正在保存书目记录 " + strBiblioRecPath);

                    byte[] baNewTimestamp = null;

                    long lRet = Channel.SetBiblioInfo(
                        stop,
                        "change",
                        strBiblioRecPath,
                        "xml",
                        biblio_info.NewXml,
                        biblio_info.Timestamp,
                        "",
                        out strOutputPath,
                        out baNewTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        if (Channel.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            DialogResult result = MessageBox.Show(this,
    "保存书目记录 " + strBiblioRecPath + " 时遭遇时间戳不匹配: " + strError + "。\r\n\r\n此记录已无法被保存。\r\n\r\n请问现在是否要顺便重新装载此记录? \r\n\r\n(Yes 重新装载；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
    "Marc856SearchForm",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                                break;
                            if (result == System.Windows.Forms.DialogResult.No)
                                goto CONTINUE;

                            // 重新装载书目记录到 OldXml
                            string[] results = null;
                            // byte[] baTimestamp = null;
                            lRet = Channel.GetBiblioInfos(
                                stop,
                                strBiblioRecPath,
                                "",
                                new string[] { "xml" },   // formats
                                out results,
                                out baNewTimestamp,
                                out strError);
                            if (lRet == 0)
                            {
                                // TODO: 警告后，把 item 行移除？
                                return -1;
                            }
                            if (lRet == -1)
                                return -1;
                            if (results == null || results.Length == 0)
                            {
                                strError = "results error";
                                return -1;
                            }
                            biblio_info.OldXml = results[0];
                            biblio_info.Timestamp = baNewTimestamp;
                            nReloadCount++;
                            goto CONTINUE;
                        }

                        return -1;
                    }

                    // 检查是否有部分字段被拒绝
                    if (Channel.ErrorCode == ErrorCode.PartialDenied)
                    {
                        DialogResult result = MessageBox.Show(this,
"保存书目记录 " + strBiblioRecPath + " 时部分字段被拒绝。\r\n\r\n此记录已部分保存成功。\r\n\r\n请问现在是否要顺便重新装载此记录以便观察? \r\n\r\n(Yes 重新装载(到旧记录部分)；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
"BiblioSearchForm",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;
                        // 重新装载书目记录到 OldXml
                        string[] results = null;
                        // byte[] baTimestamp = null;
                        lRet = Channel.GetBiblioInfos(
                            stop,
                            strBiblioRecPath,
                            "",
                            new string[] { "xml" },   // formats
                            out results,
                            out baNewTimestamp,
                            out strError);
                        if (lRet == 0)
                        {
                            // TODO: 警告后，把 item 行移除？
                            return -1;
                        }
                        if (lRet == -1)
                            return -1;
                        if (results == null || results.Length == 0)
                        {
                            strError = "results error";
                            return -1;
                        }
                        biblio_info.OldXml = results[0];
                        biblio_info.Timestamp = baNewTimestamp;
                        nReloadCount++;
                        goto CONTINUE;
                    }

                    biblio_info.Timestamp = baNewTimestamp;
                    biblio_info.OldXml = biblio_info.NewXml;
                    biblio_info.NewXml = "";

                    nSavedCount++;
                CONTINUE:
                    stop.SetProgressValue(i);
                    i++;
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.EnableControls(true);
                this.listView_records.Enabled = true;
            }

#if NO
            // 刷新书目摘要
            nRet = FillBiblioSummaryColumn(items,
                0,
                true,
                true,
                out strError);
            if (nRet == -1)
                return -1;
#endif

            DoViewComment(false);

            strError = "";
            if (nSavedCount > 0)
                strError += "共保存书目记录 " + nSavedCount + " 条";
            if (nReloadCount > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += " ; ";
                strError += "有 " + nReloadCount + " 条书目记录因为时间戳不匹配或部分字段被拒绝而重新装载旧记录部分(请观察后重新保存)";
            }

            return 0;
        }

        // 同一书目记录下，修改比 item 位置靠后的事项的 Index
        static int ChangeFieldIndex(
            Hashtable relation_table,
            ListViewItem item, 
            int nDelta,
            out string strError)
        {
            strError = "";

            string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);
            List<ListViewItem> array = (List<ListViewItem>)relation_table[strBiblioRecPath];
            if (array == null)
            {
                strError = "strBiblioRecPath '"+strBiblioRecPath+"' 的 array 在 relation_table 中没有找到";
                return -1;
            }

            LineInfo info = (LineInfo)item.Tag;

            foreach(ListViewItem current_item in array)
            {
                if (current_item == item)
                    continue;
                LineInfo current_info = (LineInfo)current_item.Tag;
                if (current_info.Index > info.Index)
                {
                    current_info.Index += nDelta;
                    ListViewUtil.ChangeItemText(current_item, COLUMN_FIELDINDEX, current_info.Index.ToString());
                }
            }

            return 0;
        }

        // 把对字段的修改合并到书目记录中
        // return:
        //      -1  出错
        //      0   书目记录没有发生修改
        //      1   书目记录发生了修改
        int MergeChange(LineInfo line_info, 
            BiblioInfo biblio_info,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strXml = biblio_info.NewXml;
            if (string.IsNullOrEmpty(strXml) == true)
                strXml = biblio_info.OldXml;

            string strMARC = "";
            string strMarcSyntax = "";
            // 将XML格式转换为MARC格式
            // 自动从数据记录中获得MARC语法
            nRet = MarcUtil.Xml2Marc(strXml,
                true,
                null,
                out strMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
            {
                strError = "XML 转换到 MARC 记录时出错: " + strError;
                return -1;
            }

            if (string.IsNullOrEmpty(line_info.State) == true
                && string.IsNullOrEmpty(line_info.NewField) == true)
            {
                strError = "一般修改情况下的 line_info.NewField 不应该为空";
                return -1;
            }

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");
            if (fields.count > line_info.Index)
            {
                if (line_info.State == "delete")
                    fields[line_info.Index].detach();
                else
                    fields[line_info.Index].Text = line_info.NewField;
            }
            else
            {
                if (line_info.State == "delete")
                    return 0;

                Debug.Assert(false, "除非插入情况，否则走不到这里");
                // 添加足够的 856 字段
                MarcField tail = null;
                for (int i = fields.count; i < line_info.Index + 1; i++)
                {
                    tail = new MarcField("856", "  ");
                    record.ChildNodes.insertSequence(tail, InsertSequenceStyle.PreferTail);
                }

                Debug.Assert(tail != null, "");
                tail.Text = line_info.NewField;
            }

            strMARC = record.Text;
            nRet = MarcUtil.Marc2XmlEx(strMARC,
    strMarcSyntax,
    ref strXml,
    out strError);
            if (nRet == -1)
            {
                strError = "MARC 转换到 XML 记录时出错: " + strError;
                return -1;
            }

            biblio_info.NewXml = strXml;
            return 1;
        }

        void menu_quickChange_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            nRet = QuickChangeItemRecords(out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet != 0)
                MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        internal int m_nChangedCount = 0;

        internal int GetItemsChangeCount(List<ListViewItem> items)
        {
            if (this.m_nChangedCount == 0)
                return 0;   // 提高速度

            int nResult = 0;
            foreach (ListViewItem item in items)
            {
                if (IsItemChanged(item) == true)
                    nResult++;
            }
            return nResult;
        }

        // 清除一个事项的修改信息
        void ClearOneChange(ListViewItem item)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
                return;

            if (string.IsNullOrEmpty(info.State) == false)
            {
                info.State = "";

                Debug.Assert(String.IsNullOrEmpty(info.NewField) == true, "在 State 有值的情况下 NewField 应该为空");

                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
                return;
            }

            if (String.IsNullOrEmpty(info.NewField) == false)
            {
                info.NewField = "";
                // 刷新除了记录路径和书目摘要的其余列的显示
                MarcField field = new MarcField(info.OldField);
                SetItemColumns(item, null, field, info.Index);

                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
            }

            // TODO: 要允许 insert 或 delete 动作可以被清除，需要设计成在保存前才往书目记录里面兑现修改。

        }
        // 观察一个事项是否在内存中修改过
        internal bool IsItemChanged(ListViewItem item)
        {
            LineInfo info = (LineInfo)item.Tag;
            if (info == null)
                return false;

            if (string.IsNullOrEmpty(info.NewField) == false)
                return true;

            if (string.IsNullOrEmpty(info.State) == false)
                return true;

            return false;
        }

        // 快速修改记录
        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        internal int QuickChangeItemRecords(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.listView_records.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的 856 字段事项";
                return -1;
            }

            List<OneAction> actions = null;
            XmlDocument cfg_dom = null;

            {
                ChangeItemActionDialog dlg = new ChangeItemActionDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.ElementCaption = "子字段";
                dlg.DbType = "856";
                dlg.Text = "快速修改 856 字段 -- 请指定动作参数";
                // dlg.MainForm = Program.MainForm;

                dlg.UiState = Program.MainForm.AppInfo.GetString(
"marc856search_form",
"ChangeItemActionDialog_uiState",
"");

                Program.MainForm.AppInfo.LinkFormState(dlg, "marc856searchform_quickchangedialog_state");
                dlg.ShowDialog(this);
                Program.MainForm.AppInfo.UnlinkFormState(dlg);

                Program.MainForm.AppInfo.SetString(
"marc856search_form",
"ChangeItemActionDialog_uiState",
dlg.UiState);

                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return 0;   // 放弃

                actions = dlg.Actions;
                cfg_dom = dlg.CfgDom;
            }

            DateTime now = DateTime.Now;

            // TODO: 检查一下，看看是否一项修改动作都没有
            Program.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行快速修改 856 字段</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("快速修改 856 字段 ...");
            stop.BeginLoop();

            EnableControls(false);
            this.listView_records.Enabled = false;

            int nProcessCount = 0;
            int nChangedCount = 0;
            try
            {
                stop.SetProgressRange(0, this.listView_records.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this.listView_records.SelectedItems)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;

                    items.Add(item);
                }

                bool bOldSource = true; // 是否要从 OldXml 开始做起

                int nChangeCount = this.GetItemsChangeCount(items);
                if (nChangeCount > 0)
                {
                    bool bHideMessageBox = true;
                    DialogResult result = MessageDialog.Show(this,
                        "当前选定的 " + items.Count.ToString() + " 个事项中有 " + nChangeCount + " 项修改尚未保存。\r\n\r\n请问如何进行修改? \r\n\r\n(重新修改) 重新进行修改，忽略以前内存中的修改; \r\n(继续修改) 以上次的修改为基础继续修改; \r\n(放弃) 放弃整个操作",
    MessageBoxButtons.YesNoCancel,
    MessageBoxDefaultButton.Button1,
    null,
    ref bHideMessageBox,
    new string[] { "重新修改", "继续修改", "放弃" });
                    if (result == DialogResult.Cancel)
                    {
                        strError = "放弃";
                        return 0;
                    }
                    if (result == DialogResult.No)
                    {
                        bOldSource = false;
                    }
                }

                int i = 0;
                foreach (ListViewItem item in items)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    stop.SetProgressValue(i);

                    LineInfo info = (LineInfo)item.Tag;
                    if (info == null)
                    {
                        strError = "item.Tag == null";
                        return -1;
                    }
                    Debug.Assert(info != null, "");

                    string strField = "";

                    if (bOldSource == true)
                    {
                        strField = info.OldField;
                        // 放弃上一次的修改
                        if (string.IsNullOrEmpty(info.State) == false)
                        {
                            info.State = "";
                            this.m_nChangedCount--;

                            if (string.IsNullOrEmpty(info.NewField) == false)
                                strField = info.NewField;
                            else
                                strField = info.OldField;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(info.NewField) == false)
                            {
                                info.NewField = "";
                                this.m_nChangedCount--;
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(info.State) == false)
                        {
                            // 在删除的基础上继续修改 --- 等于承认删除状态，并无法修改，只好跳过
                            i++;
                            continue;
                        }
                        if (string.IsNullOrEmpty(info.NewField) == false)
                            strField = info.NewField;
                        else
                            strField = info.OldField;
                    }

                    string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(strBiblioRecPath + ":" + info.Index) + "</div>");

                    string strDebugInfo = "";

                    // 修改一个订购记录 XmlDocument
                    // return:
                    //      -1  出错
                    //      0   没有实质性修改
                    //      1   发生了修改
                    nRet = Modify856Field(
                        cfg_dom,
                        actions,
                        ref strField,
                        now,
                        out strDebugInfo,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    Program.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode(strDebugInfo).Replace("\r\n", "<br/>") + "</div>");

                    nProcessCount++;

                    if (nRet == 1)
                    {
                        Debug.Assert(info != null, "");
                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewField) == true)
                                this.m_nChangedCount++;
                            info.NewField = strField;
                        }

                        // 刷新除了记录路径和书目摘要的其余列的显示
                        MarcField field = new MarcField(info.NewField);
                        SetItemColumns(item, null, field, info.Index);

                        item.BackColor = SystemColors.Info;
                        item.ForeColor = SystemColors.InfoText;
                    }

                    i++;
                    nChangedCount++;
                }
            }
            finally
            {
                EnableControls(true);
                this.listView_records.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                Program.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束快速修改 856 字段</div>");
            }

            DoViewComment(false);
            strError = "修改 856 字段 " + nChangedCount.ToString() + " 个 (共处理 " + nProcessCount.ToString() + " 个)\r\n\r\n(注意修改并未自动保存。请在观察确认后，使用保存命令将修改保存回书目库)";
            return 1;
        }

        // 修改一个 856 字段
        // return:
        //      -1  出错
        //      0   没有实质性修改
        //      1   发生了修改
        int Modify856Field(
            XmlDocument cfg_dom,
            List<OneAction> actions,
            ref string strField,
            DateTime now,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            bool bChanged = false;
            int nRet = 0;

            MarcField field = new MarcField(strField);

            StringBuilder debug = new StringBuilder(4096);

            foreach (OneAction action in actions)
            {
                // 找到元素名
                if (string.IsNullOrEmpty(action.FieldName) == true)
                    continue;
                string strElementName = "";
                if (action.FieldName[0] == '{' || action.FieldName[0] == '<')
                {
                    strElementName = OneActionDialog.Unquote(action.FieldName);
                }
                else
                {
                    nRet = OneActionDialog.GetElementName(
                        cfg_dom,
                        action.FieldName,
                        out strElementName,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                // 将值字符串中的宏替换
                string strFieldValue = action.FieldValue;
                if (strFieldValue.IndexOf("%") != -1)
                {
                    this._macroFileName = Path.Combine(Program.MainForm.UserDir, "856_macrotable.xml");
                    if (File.Exists(this._macroFileName) == false)
                    {
                        strError = "宏定义文件 '" + this._macroFileName + "' 不存在，无法进行宏替换";
                        return -1;
                    }
                    string strResult = "";
                    // 解析宏
                    nRet = MacroUtil.Parse(
                        false,
                        strFieldValue,
                        ParseOneMacro,
                        out strResult,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "替换字符串 '" + strFieldValue + "' 中的宏时出错: " + strError;
                        return -1;
                    }

                    strFieldValue = strResult;
                }

                if (string.IsNullOrEmpty(action.Action) == true)
                {
                    // 替换内容
#if NO
                        ChangeSubfield(ref strField,
    strElementName,
    strFieldValue,
    ref debug,
    ref bChanged);
#endif
                    if (SetSubfield(field, strElementName, strFieldValue, ref debug) == true)
                        bChanged = true;
                }
                else
                {
                    // add/remove
                    string strState = GetSubfield(field,
                        strElementName);

                    string strOldState = strState;

                    if (action.Action == "delete")
                    {
                        SetSubfield(field, strElementName, null, ref debug);
                        bChanged = true;
                        continue;
                    }

                    if (action.Action == "add")
                    {
                        if (String.IsNullOrEmpty(action.FieldValue) == false)
                            StringUtil.SetInList(ref strState,
                                strFieldValue,  // action.FieldValue, 
                                true);
                    }
                    if (action.Action == "remove")
                    {
                        if (String.IsNullOrEmpty(action.FieldValue) == false)
                            StringUtil.SetInList(ref strState,
                                strFieldValue,  // action.FieldValue, 
                                false);
                    }

                    if (strOldState != strState)
                    {
                        SetSubfield(field, strElementName, strState, ref debug);
                        bChanged = true;

                        // debug.Append("<" + strElementName + "> '" + strOldState + "' --> '" + strState + "'\r\n");
                    }
                }
            }

            strDebugInfo = debug.ToString();

            if (bChanged == true)
            {
                strField = field.Text;
                return 1;
            }

            return 0;
        }

        // 获得一个子字段内容，或局部内容
        // parameters:
        //      strSubfieldName 可能为 "a"，也可能为 "x.rights" 形态
        static string GetSubfield(MarcField field, string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true
                || strPath.Length < 1)
                throw new ArgumentException("strPath 参数值应为一个字符以上", "strPath");

            string strSubfieldName = "";
            string strPartName = "";

            StringUtil.ParseTwoPart(strPath, ".", out strSubfieldName, out strPartName);

            string strContent = field.select("subfield[@name='"+strSubfieldName+"']").FirstContent;
            if (string.IsNullOrEmpty(strPartName) == true)
                return strContent;

            // 读取参数的时候，不在意内部顺序问题
            Hashtable table = StringUtil.ParseParameters(strContent, ';', ':');
            return (string)table[strPartName];
        }

        // parameters:
        //      strValue    要修改成的值。如果为 null，表示删除这个子字段/部分。如果为 "" 则表示把内容设为空，但子字段还会被保留
        // return:
        //      是否发生了修改
        static bool SetSubfield(MarcField field,
            string strPath,
            string strValue,
            ref StringBuilder debug)
        {
            if (string.IsNullOrEmpty(strPath) == true
    || strPath.Length < 1)
                throw new ArgumentException("strPath 参数值应为一个字符以上", "strPath");

            if (strPath == "indicator1")
            {
                if (string.IsNullOrEmpty(strValue) == true)
                    throw new ArgumentException("当操作 indicator 的时候， strValue 参数值不允许为空", "strValue");
                if (field.Indicator1 == strValue[0])
                    return false;
                field.Indicator1 = strValue[0];
                return true;
            }
            if (strPath == "indicator2")
            {
                if (string.IsNullOrEmpty(strValue) == true)
                    throw new ArgumentException("当操作 indicator 的时候， strValue 参数值不允许为空", "strValue");
                if (field.Indicator2 == strValue[0])
                    return false;
                field.Indicator2 = strValue[0];
                return true;
            }

            string strSubfieldName = "";
            string strPartName = "";

            StringUtil.ParseTwoPart(strPath, ".", out strSubfieldName, out strPartName);
            
            MarcNodeList subfields = field.select("subfield[@name='" + strSubfieldName + "']");
            if (subfields.count == 0)
            {
                if (strValue == null)
                    return false;   // 正好，也不用删除了

                string strContent = strValue;
                if (string.IsNullOrEmpty(strPartName) == false)
                    strContent = strPartName + ":" + strValue;
                field.ChildNodes.insertSequence(new MarcSubfield(strSubfieldName, strContent));
                debug.Append("新增 <" + strPath + "> '" + strContent + "'\r\n");
                return true;
            }
            else
            {
                // 对整个子字段内容进行操作。只修改第一个子字段内容
                if (string.IsNullOrEmpty(strPartName) == true)
                {
                    if (strValue == null)
                    {
                        subfields[0].detach();
                        return true;
                    }
                    if (subfields[0].Content == strValue)
                        return false;
                    subfields[0].Content = strValue;
                    return true;
                }

                string strContent = subfields[0].Content;
                ParamList table = ParamList.Build(strContent, ';', ':');

                if (strValue == null)
                    table.Remove(strPartName);
                else
                    table[strPartName] = strValue;

                string strNewContent = table.ToString(';', ':');

                if (strContent == strNewContent)
                    return false;
                subfields[0].Content = strNewContent;
                debug.Append("<" + strPartName + "> '" + strContent + "' --> '" + strNewContent + "'\r\n");
                return true;
            }
        }

        string _macroFileName = "";
        // return:
        //      -1  出错。错误信息在 strError 中
        //      0   不能处理的宏
        //      1   成功处理，返回结果在 strValue 中
        int ParseOneMacro(bool bSimulate,
            string strName,
            out string strValue,
            out string strError)
        {
            strError = "";

            strName = StringUtil.Unquote(strName, "%%");  // 去掉百分号

            strValue = "";
            // 从marceditor_macrotable.xml文件中解析宏
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = MacroUtil.GetFromLocalMacroTable(
                _macroFileName, // Path.Combine(Program.MainForm.DataDir, "marceditor_macrotable.xml"),
                strName,
                bSimulate,
                out strValue,
                out strError);
            if (nRet == -1)
            {
                return -1;
            }
            if (nRet == 0)
                return 0;
            return 1;
        }

        CommentViewerForm m_commentViewer = null;
        int m_nInViewing = 0;

        internal void DoViewComment(bool bOpenWindow)
        {
            m_nInViewing++;
            try
            {
                _doViewComment(bOpenWindow);
            }
            finally
            {
                m_nInViewing--;
            }
        }

        static string BuildOpacText(string strField)
        {
            if (string.IsNullOrEmpty(strField) == true)
                return "";
            StringBuilder text = new StringBuilder();
            MarcField field = new MarcField(strField);
            text.Append("指示符1:" + field.Indicator1 + "\r\n");
            text.Append("指示符2:" + field.Indicator2 + "\r\n");
            foreach(MarcSubfield subfield in field.ChildNodes)
            {
                if (subfield.Name == "x")
                {
                    string [] parts = subfield.Content.Split(new char []{';'});
                    foreach(string part in parts)
                    {
                        string left = "";
                        string right = "";
                        StringUtil.ParseTwoPart(part, ":", out left, out right);
                        text.Append("$" + subfield.Name + "." + left + ":" + right + "\r\n");
                    }
                    continue;
                }

                text.Append("$" + subfield.Name + ":" + subfield.Content + "\r\n");
            }

            return text.ToString();
        }

        internal int GetXmlHtml(LineInfo info,
out string strXml,
out string strHtml2,
out string strError)
        {
            strError = "";
            int nRet = 0;

            strXml = "";
            strHtml2 = "";

            if (string.IsNullOrEmpty(info.NewField) == false
                && string.IsNullOrEmpty(info.OldField) == false
                && string.IsNullOrEmpty(info.State) == true)
            {
                // 创建展示两个 OPAC 记录差异的 HTML 字符串
                // return:
                //      -1  出错
                //      0   成功。两边相等
                //      1   两边不相等
                nRet = MarcDiff.DiffOpacHtml(
                    BuildOpacText(info.OldField),
                    BuildOpacText(info.NewField),
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    return -1;
                return 0;
            }

            nRet = MarcDiff.DiffOpacHtml(
    BuildOpacText(info.OldField),
    string.IsNullOrEmpty(info.State) == false? "" : null,
    out strHtml2,
    out strError);
            if (nRet == -1)
                return -1;
            return 0;
        }

        internal string GetHeadString(bool bAjax = true)
        {
            string strCssFilePath = Path.Combine(Program.MainForm.DataDir, "default\\fieldhtml.css");

            if (bAjax == true)
                return
                    "<head>" +
                    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
                    "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>" +
                    "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>" +
                    //"<script type='text/javascript' src='%datadir%/jquery.js'></script>" +
                    "<script type='text/javascript' charset='UTF-8' src='%datadir%\\getsummary.js" + "'></script>" +
                    "</head>";
            return
    "<head>" +
    "<LINK href='" + strCssFilePath + "' type='text/css' rel='stylesheet'>" +
    "</head>";
        }

        void _doViewComment(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            // string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (Program.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (Program.MainForm.CanDisplayItemProperty() == false)
                    return;
            }

            if (this.m_biblioTable == null
                || this.listView_records.SelectedItems.Count != 1)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            ListViewItem item = this.listView_records.SelectedItems[0];

            LineInfo info = (LineInfo)item.Tag;

            string strXml = "";
            string strHtml2 = "";


            int nRet = GetXmlHtml(info,
                    out strXml,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

            strHtml = "<html>" +
    GetHeadString() +
    "<body>" +
    strHtml2 +
    // EntityForm.GetTimestampHtml(info.Timestamp) +
    "</body></html>";

            bool bNew = false;
            if (this.m_commentViewer == null
                || (bOpenWindow == true && this.m_commentViewer.Visible == false))
            {
                m_commentViewer = new CommentViewerForm();
                MainForm.SetControlFont(m_commentViewer, this.Font, false);
                bNew = true;
            }

            // m_commentViewer.MainForm = Program.MainForm;  // 必须是第一句

            if (bNew == true)
                m_commentViewer.InitialWebBrowser();

            string strBiblioRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            m_commentViewer.Text = "MARC内容 " + strBiblioRecPath + " " + info.Index;
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = strXml; //  MergeXml(strXml1, strXml2);
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // Program.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // Program.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    Program.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
                    m_commentViewer.Show(this);
                    m_commentViewer.Activate();

                    Program.MainForm.CurrentPropertyControl = null;
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
                    if (Program.MainForm.CurrentPropertyControl != m_commentViewer.MainControl)
                        m_commentViewer.DoDock(false); // 不会自动显示FixedPanel
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, "DoViewComment() 出错: " + strError);
        }

        void marc_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                Program.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        internal void ClearCommentViewer()
        {
            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();
        }

        private void listView_records_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoViewComment(false);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.listView_records.Enabled = bEnable;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_records);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_records);
                GuiState.SetUiState(controls, value);
            }
        }

        private void listView_records_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewUtil.OnColumnClick(this.listView_records, e);
        }
    }

    class LineInfo
    {
#if NO
        /// <summary>
        /// 856 字段对象
        /// </summary>
        public MarcField Field = null;
#endif
        /// <summary>
        /// 本 856 字段在 MARC 记录中同名字段集合内的下标
        /// </summary>
        public int Index = -1;

        /// <summary>
        /// 旧的 856 字段文本
        /// </summary>
        public string OldField = "";

        /// <summary>
        /// 新的 856 字段文本
        /// </summary>
        public string NewField = "";

        /// <summary>
        /// 状态。如果为 "delete" 表示事项即将被删除
        /// </summary>
        public string State = "";

    }
}
