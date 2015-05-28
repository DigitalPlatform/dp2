using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform.CommonControl;
using System.Collections;
using System.Windows.Forms;
using System.Web;
using System.Diagnostics;
using System.Drawing;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient.localhost;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 各种检索窗的基础类。
    /// 提供了浏览和脚本修改记录的功能
    /// </summary>
    public class SearchFormBase : MyForm
    {
        // 数据库类型
        /// <summary>
        /// 要查询的数据库类型。"item" 表示查询实体库；"order" 表示查询订购库；"issue" 表示查询期库；"comment" 表示查询评注库; "patron" 表示读者库
        /// </summary>
        public string DbType = "item";  // comment order issue patron

        internal ListView _listviewRecords = null;

        const int WM_SELECT_INDEX_CHANGED = API.WM_USER + 300;

        Commander commander = null;

        CommentViewerForm m_commentViewer = null;

        internal Hashtable m_biblioTable = new Hashtable(); // 读者记录路径 --> 读者信息
        int m_nInViewing = 0;

        internal virtual bool InSearching
        {
            get
            {
                throw new Exception("尚未重载 InSearching");
                // return false;
            }
        }

        /// <summary>
        /// 当前窗口查询的数据库类型，用于显示的名称形态
        /// </summary>
        public virtual string DbTypeCaption
        {
            get
            {
                throw new Exception("尚未实现");
            }
        }

        /// <summary>
        /// 窗口 Load 时被触发
        /// </summary>
        public override void OnMyFormLoad()
        {
            this.commander = new Commander(this);
            this.commander.IsBusy -= new IsBusyEventHandler(commander_IsBusy);
            this.commander.IsBusy += new IsBusyEventHandler(commander_IsBusy);

            base.OnMyFormLoad();
        }

        /// <summary>
        /// 窗口 Closing 时被触发
        /// </summary>
        /// <param name="e">事件参数</param>
        public override void OnMyFormClosing(FormClosingEventArgs e)
        {
            if (this.m_nChangedCount > 0)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内有 " + m_nChangedCount + " 项修改尚未保存。若此时关闭窗口，现有未保存信息将丢失。\r\n\r\n确实要关闭窗口? ",
                    "SearchForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    // return;
                }
            }

            base.OnMyFormClosing(e);
        }

        /// <summary>
        /// 窗口 Closed 时被触发。在 base.OnFormClosed(e) 之前被调用
        /// </summary>
        public override void OnMyFormClosed()
        {
            base.OnMyFormClosed();

            this.commander.Destroy();

            if (this.m_commentViewer != null)
            {
                this.m_commentViewer.ExitWebBrowser();  // 虽然 CommentViwerForm 的 Dispose() 里面也作了释放，但为了保险起见，这里也释放一次
                this.m_commentViewer.Close();
            }
        }


        void commander_IsBusy(object sender, IsBusyEventArgs e)
        {
            e.IsBusy = this.m_nInViewing > 0;
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SELECT_INDEX_CHANGED:
                    {
                        if (this._listviewRecords.SelectedIndices.Count == 0)
                            SetStatusMessage("");
                        else
                        {
                            if (this._listviewRecords.SelectedIndices.Count == 1)
                            {
                                // this.label_message.Text = "第 " + (this._listviewRecords.SelectedIndices[0] + 1).ToString() + " 行";
                                SetStatusMessage("第 " + (this._listviewRecords.SelectedIndices[0] + 1).ToString() + " 行");
                            }
                            else
                            {
                                SetStatusMessage("从 " + (this._listviewRecords.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this._listviewRecords.SelectedIndices.Count.ToString() + " 个事项");
                            }
                        }

                        ListViewUtil.OnSeletedIndexChanged(this._listviewRecords,
                            0,
                            null);

                        if (this.m_biblioTable != null)
                        {
                            if (CanCallNew(commander, m.Msg) == true)
                                DoViewComment(false);
                        }
                    }
                    return;
            }
            try
            {
                base.DefWndProc(ref m);
            }
            catch
            {
            }
        }

        /*public*/
        bool CanCallNew(Commander commander, int msg)
        {
            if (this.m_nInViewing > 0)
            {
                // 缓兵之计
                // this.Stop();
                commander.AddMessage(msg);
                return false;   // 还不能启动
            }

            return true;    // 可以启动
        }

        // 在状态行显示文字信息
        internal virtual void SetStatusMessage(string strMessage)
        {
        }

        internal void OnListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            this.commander.AddMessage(WM_SELECT_INDEX_CHANGED);
        }

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

        int GetBiblioInfo(
bool bCheckSearching,
ListViewItem item,
out BiblioInfo info,
out string strError)
        {
            strError = "";
            info = null;

            if (this.m_biblioTable == null)
                return 0;

            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return 0;

            // 存储所获得书目记录 XML
            info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
            {
                info = new BiblioInfo();
                info.RecPath = strRecPath;
                this.m_biblioTable[strRecPath] = info;
            }

            if (string.IsNullOrEmpty(info.OldXml) == true)
            {
                if (bCheckSearching == true)
                {
                    if (this.InSearching == true)
                        return 0;
                }

#if NO
                string[] results = null;
                byte[] baTimestamp = null;
                string strOutputRecPath = "";
                // 获得读者记录
                long lRet = Channel.GetReaderInfo(
    stop,
    "@path:" + strRecPath,
    "xml",
    out results,
    out strOutputRecPath,
    out baTimestamp,
    out strError);

                if (lRet == 0)
                    return -1;  // 是否设定为特殊状态?
                if (lRet == -1)
                    return -1;

                if (results == null || results.Length == 0)
                {
                    strError = "results error";
                    return -1;
                }

                string strXml = results[0];
#endif
                string strXml = "";
                byte[] baTimestamp = null;

                // 获得一条记录
                //return:
                //      -1  出错
                //      0   没有找到
                //      1   找到
                int nRet = GetRecord(
                    strRecPath,
                    out strXml,
                    out baTimestamp,
                    out strError);
                if (nRet == 0 || nRet == 0)
                    return -1;

                info.OldXml = strXml;
                info.Timestamp = baTimestamp;
                info.RecPath = strRecPath;
            }

            return 1;
        }

        // 获得一条记录
        //return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        internal virtual int GetRecord(
            string strRecPath,
            out string strXml,
            out byte[] baTimestamp,
            out string strError)
        {
            strError = "尚未实现";
            strXml = "";
            baTimestamp = null;
#if NO
            string[] results = null;
            string strOutputRecPath = "";
            // 获得读者记录
            long lRet = Channel.GetReaderInfo(
stop,
"@path:" + strRecPath,
"xml",
out results,
out strOutputRecPath,
out baTimestamp,
out strError);

            if (lRet == 0)
                return 0;  // 是否设定为特殊状态?
            if (lRet == -1)
                return -1;

            if (results == null || results.Length == 0)
            {
                strError = "results error";
                return -1;
            }

            strXml = results[0];
            return 1;
#endif
            return -1;
        }

        // 保存一条记录
        // 保存成功后， info.Timestamp 会被更新
        // return:
        //      -2  时间戳不匹配
        //      -1  出错
        //      0   成功
        internal virtual int SaveRecord(string strRecPath,
            BiblioInfo info,
            out byte [] baNewTimestamp,
            out string strError)
        {
            strError = "尚未实现";
            baNewTimestamp = null;
            return -1;
        }


        internal virtual string GetHeadString(bool bAjax = true)
        {
            return "";
        }

        internal virtual int GetXmlHtml(BiblioInfo info,
    out string strXml,
    out string strHtml2,
    out string strError)
        {
            strError = "";

            strXml = "";
            strHtml2 = "";

            return 0;
        }

        void _doViewComment(bool bOpenWindow)
        {
            string strError = "";
            string strHtml = "";
            // string strXml = "";

            // 优化，避免无谓地进行服务器调用
            if (bOpenWindow == false)
            {
                if (this.MainForm.PanelFixedVisible == false
                    && (m_commentViewer == null || m_commentViewer.Visible == false))
                    return;
                // 2013/3/7
                if (this.MainForm.CanDisplayItemProperty() == false)
                    return;
            }

            if (this.m_biblioTable == null
                || this._listviewRecords.SelectedItems.Count != 1)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }

            ListViewItem item = this._listviewRecords.SelectedItems[0];
#if NO
            string strRecPath = this._listviewRecords.SelectedItems[0].Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }
#endif

            // BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            BiblioInfo info = null;
            int nRet = GetBiblioInfo(
                true,
                item,
                out info,
                out strError);
            if (info == null)
            {
                if (this.m_commentViewer != null)
                    this.m_commentViewer.Clear();
                return;
            }


            string strXml = "";
            string strHtml2 = "";

            if (nRet == -1)
            {
                strHtml2 = HttpUtility.HtmlEncode(strError);
            }
            else
            {
                nRet = GetXmlHtml(info,
                    out strXml,
                    out strHtml2,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            strHtml = "<html>" +
    GetHeadString() +
    "<body>" +
    strHtml2 +
    EntityForm.GetTimestampHtml(info.Timestamp) +
    "</body></html>";

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

            m_commentViewer.Text = "MARC内容 '" + info.RecPath + "'";
            m_commentViewer.HtmlString = strHtml;
            m_commentViewer.XmlString = strXml; //  MergeXml(strXml1, strXml2);
            m_commentViewer.FormClosed -= new FormClosedEventHandler(marc_viewer_FormClosed);
            m_commentViewer.FormClosed += new FormClosedEventHandler(marc_viewer_FormClosed);
            // this.MainForm.AppInfo.LinkFormState(m_viewer, "comment_viewer_state");
            // m_viewer.ShowDialog(this);
            // this.MainForm.AppInfo.UnlinkFormState(m_viewer);
            if (bOpenWindow == true)
            {
                if (m_commentViewer.Visible == false)
                {
                    this.MainForm.AppInfo.LinkFormState(m_commentViewer, "marc_viewer_state");
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

        void marc_viewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_commentViewer != null)
            {
                this.MainForm.AppInfo.UnlinkFormState(m_commentViewer);
                this.m_commentViewer = null;
            }
        }

        // 观察一个事项是否在内存中修改过
        internal bool IsItemChanged(ListViewItem item)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return false;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return false;

            if (string.IsNullOrEmpty(info.NewXml) == false)
                return true;

            return false;
        }

        internal int GetItemsChangeCount(List<ListViewItem> items)
        {
            if (this.m_nChangedCount == 0)
                return 0;   // 提高速度

            int nResult = 0;
            foreach (ListViewItem item in items)
            {
                if (IsItemChanged(item) == true)
                    nResult ++;
            }
            return nResult;
        }

        /// <summary>
        /// 刷新所选择的行。也就是重新从数据库中装载浏览列
        /// </summary>
        public void RrefreshAllItems()
        {
            string strError = "";
            int nRet = 0;

            if (this._listviewRecords.Items.Count == 0)
                return;

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this._listviewRecords.Items)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                if (IsItemChanged(item) == true)
                    nChangedCount++;
            }

            // 警告未保存的内容会丢失
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
    "要刷新的 " + this._listviewRecords.SelectedItems.Count.ToString() + " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
    "SearchForm",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            nRet = RefreshListViewLines(items,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 
        /// <summary>
        /// 刷新所选择的行。也就是重新从数据库中装载浏览列
        /// </summary>
        public void RrefreshSelectedItems()
        {
            string strError = "";
            int nRet = 0;

            if (this._listviewRecords.SelectedItems.Count == 0)
            {
                strError = "尚未选择要刷新的浏览行";
                goto ERROR1;
            }

            int nChangedCount = 0;
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this._listviewRecords.SelectedItems)
            {
                if (string.IsNullOrEmpty(item.Text) == true)
                    continue;
                items.Add(item);

                if (IsItemChanged(item) == true)
                    nChangedCount++;
            }

            // 警告未保存的内容会丢失
            if (nChangedCount > 0)
            {
                DialogResult result = MessageBox.Show(this,
    "要刷新的 " + this._listviewRecords.SelectedItems.Count.ToString() + " 个事项中有 " + nChangedCount.ToString() + " 项修改后尚未保存。如果刷新它们，修改内容会丢失。\r\n\r\n是否继续刷新? (OK 刷新；Cancel 放弃刷新)",
    "SearchForm",
    MessageBoxButtons.OKCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                    return;
            }

            nRet = RefreshListViewLines(items,
                true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            DoViewComment(false);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 刷新浏览行
        /// </summary>
        /// <param name="items_param">要刷新的 ListViewItem 集合</param>
        /// <param name="bBeginLoop">是否要调用 stop.BeginLoop() </param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int RefreshListViewLines(List<ListViewItem> items_param,
            bool bBeginLoop,
            out string strError)
        {
            strError = "";

            if (items_param.Count == 0)
                return 0;

            if (stop != null && bBeginLoop == true)
            {
                stop.Style = StopStyle.EnableHalfStop;
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在刷新浏览行 ...");
                stop.BeginLoop();

                this.EnableControls(false);
            }

            try
            {
                List<ListViewItem> items = new List<ListViewItem>();
                List<string> recpaths = new List<string>();
                foreach (ListViewItem item in items_param)
                {
                    if (string.IsNullOrEmpty(item.Text) == true)
                        continue;
                    items.Add(item);
                    recpaths.Add(item.Text);

                    ClearOneChange(item, true);
                }

                if (stop != null)
                    stop.SetProgressRange(0, items.Count);

                BrowseLoader loader = new BrowseLoader();
                loader.Channel = Channel;
                loader.Stop = stop;
                loader.RecPaths = recpaths;
                loader.Format = "id,cols";

                int i = 0;
                foreach (DigitalPlatform.CirculationClient.localhost.Record record in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    Debug.Assert(record.Path == recpaths[i], "");

                    if (stop != null)
                    {
                        stop.SetMessage("正在刷新浏览行 " + record.Path + " ...");
                        stop.SetProgressValue(i);
                    }

                    ListViewItem item = items[i];

#if NO
                    if (record.Cols == null)
                    {
                        int c = 0;
                        foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                        {
                            if (c != 0)
                                subitem.Text = "";
                            c++;
                        }
                    }
                    else
                    {
                        for (int c = 0; c < record.Cols.Length; c++)
                        {
                            ListViewUtil.ChangeItemText(item,
                            c + 1,
                            record.Cols[c]);
                        }

                        // TODO: 是否清除余下的列内容?
                    }
#endif
                    RefreshOneLine(item, record.Cols);

                    i++;
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
            finally
            {
                if (stop != null && bBeginLoop == true)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");
                    stop.HideProgress();
                    stop.Style = StopStyle.None;

                    this.EnableControls(true);
                }
            }
        }

        // 刷新一行内除了记录路径外的其他列
        // parameters:
        //      cols    检索结果中的浏览列
        internal virtual void RefreshOneLine(ListViewItem item,
            string [] cols)
        {
            if (cols == null)
            {
                int c = 0;
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    if (c != 0)
                        subitem.Text = "";
                    c++;
                }
            }
            else
            {
                int c = 0;
                for (; c < cols.Length; c++)
                {
                    ListViewUtil.ChangeItemText(item,
                    c + 1,
                    cols[c]);
                }

                c++;
                // 清除余下的列内容
                for (; c < item.SubItems.Count; c++)
                {
                    item.SubItems[c].Text = "";
                }
            }
        }

        // 接收一个事项的修改信息到内存
        // parameters:
        //      bClearBiblioInfo    是否顺便清除事项的 BiblioInfo 信息
        internal void AcceptOneChange(ListViewItem item,
            bool bClearBiblioInfo = false)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return;

            if (String.IsNullOrEmpty(info.NewXml) == false)
            {
                info.OldXml = info.NewXml;
                info.NewXml = "";

                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
            }

            if (bClearBiblioInfo == true)
                this.m_biblioTable.Remove(strRecPath);
        }

        // 清除一个事项的修改信息
        // parameters:
        //      bClearBiblioInfo    是否顺便清除事项的 BiblioInfo 信息
        internal void ClearOneChange(ListViewItem item,
            bool bClearBiblioInfo = false)
        {
            string strRecPath = item.Text;
            if (string.IsNullOrEmpty(strRecPath) == true)
                return;

            BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
            if (info == null)
                return;

            if (String.IsNullOrEmpty(info.NewXml) == false)
            {
                info.NewXml = "";

                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;

                this.m_nChangedCount--;
                Debug.Assert(this.m_nChangedCount >= 0, "");
            }

            if (bClearBiblioInfo == true)
                this.m_biblioTable.Remove(strRecPath);
        }

        internal void ClearBiblioTable()
        {
            this.m_biblioTable = new Hashtable();
            this.m_nChangedCount = 0;
        }

        internal void ClearCommentViewer()
        {
            if (this.m_commentViewer != null)
                this.m_commentViewer.Clear();
        }

        internal int m_nChangedCount = 0;
        internal string m_strUsedMarcQueryFilename = "";

        // 
        /// <summary>
        /// 接受选定的修改
        /// 此功能难以被一般用户理解。接受了的为何反而不能保存了？
        /// </summary>
        public void AcceptSelectedChangedRecords()
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可接收到内存");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this._listviewRecords.SelectedItems)
                {
                    AcceptOneChange(item);
                }
            }
            finally
            {
                this.Cursor = oldCursor;
            }

            DoViewComment(false);
        }

        // 
        /// <summary>
        /// 丢弃选定的修改
        /// </summary>
        public void ClearSelectedChangedRecords()
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可丢弃");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this._listviewRecords.SelectedItems)
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


        // 
        /// <summary>
        /// 丢弃全部修改
        /// </summary>
        public void ClearAllChangedRecords()
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项可丢弃");
                return;
            }
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in this._listviewRecords.Items)
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

        // 
        /// <summary>
        /// 保存选定事项的修改
        /// </summary>
        public void SaveSelectedChangedRecords()
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项需要保存");
                return;
            }

            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this._listviewRecords.SelectedItems)
            {
                if (IsItemChanged(item) == true)
                    items.Add(item);
            }

            if (items.Count == 0)
            {
                strError = "选定的范围内没有任何需要保存的事项";
                goto ERROR1;
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 
        /// <summary>
        /// 保存全部修改事项
        /// </summary>
        public void SaveAllChangedRecords()
        {
            // TODO: 确实要?

            if (this.m_nChangedCount == 0)
            {
                MessageBox.Show(this, "当前没有任何修改过的事项需要保存");
                return;
            }

            string strError = "";

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in this._listviewRecords.Items)
            {
                if (IsItemChanged(item) == true)
                    items.Add(item);
            }

            if (items.Count == 0)
            {
                strError = "警告：当前没有任何修改过的事项需要保存";   // 正常情况不应该到这里来，应该 m_nChangeCount 早就等于 0 了
                goto ERROR1;
            }

            int nRet = SaveChangedRecords(items,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            strError = "处理完成。\r\n\r\n" + strError;
            MessageBox.Show(this, strError);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 保存对指定事项的修改
        /// </summary>
        /// <param name="items">事项集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int SaveChangedRecords(List<ListViewItem> items,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            int nReloadCount = 0;
            int nSavedCount = 0;

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存"+this.DbTypeCaption+"记录 ...");
            stop.BeginLoop();

            this.EnableControls(false);
            // this._listviewRecords.Enabled = false;
            try
            {
                stop.SetProgressRange(0, items.Count);
                for (int i = 0; i < items.Count; i++)
                {
                    if (stop != null && stop.State != 0)
                    {
                        strError = "已中断";
                        return -1;
                    }

                    ListViewItem item = items[i];
                    string strRecPath = item.Text;
                    if (string.IsNullOrEmpty(strRecPath) == true)
                    {
                        stop.SetProgressValue(i);
                        goto CONTINUE;
                    }

                    BiblioInfo info = (BiblioInfo)this.m_biblioTable[strRecPath];
                    if (info == null)
                        goto CONTINUE;

                    if (string.IsNullOrEmpty(info.NewXml) == true)
                        goto CONTINUE;

                    // string strOutputPath = "";

                    stop.SetMessage("正在保存" + this.DbTypeCaption + "记录 " + strRecPath);

                    // ErrorCodeValue kernel_errorcode;

                    byte[] baNewTimestamp = null;

                REDO_SAVE:
                    // return:
                    //      -2  时间戳不匹配
                    //      -1  出错
                    //      0   成功
                    nRet = SaveRecord(strRecPath,
                        info,
                        out baNewTimestamp,
                        out strError);
#if NO
                    string strExistingXml = "";
                    string strSavedXml = "";
                    // string strSavedPath = "";
                    long lRet = Channel.SetReaderInfo(
    stop,
    "change",
    strRecPath,
    info.NewXml,
    info.OldXml,
    info.Timestamp,
    out strExistingXml,
    out strSavedXml,
    out strOutputPath,
    out baNewTimestamp,
    out kernel_errorcode,
    out strError);
#endif
                    if (nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
"保存" + this.DbTypeCaption + "记录 " + strRecPath + " 时出错: " + strError + "。\r\n\r\n请问是否要重试保存此记录? \r\n\r\n([是] 重试；\r\n[否] 不保存此记录、但继续处理后面的记录保存; \r\n[取消] 中断整批保存操作)",
this.DbTypeCaption + "查询",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                            goto REDO_SAVE;
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            return -1;
                        goto CONTINUE;
                    }

                    if (nRet == -2)
                    {
                        DialogResult result = MessageBox.Show(this,
"保存" + this.DbTypeCaption + "记录 " + strRecPath + " 时遭遇时间戳不匹配: " + strError + "。\r\n\r\n此记录已无法被保存。\r\n\r\n请问现在是否要顺便重新装载此记录? \r\n\r\n(Yes 重新装载；\r\nNo 不重新装载、但继续处理后面的记录保存; \r\nCancel 中断整批保存操作)",
this.DbTypeCaption+"查询",
MessageBoxButtons.YesNoCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (result == System.Windows.Forms.DialogResult.Cancel)
                            break;
                        if (result == System.Windows.Forms.DialogResult.No)
                            goto CONTINUE;

                        // 重新装载书目记录到 OldXml

#if NO
                            string[] results = null;
                            string strOutputRecPath = "";
                            lRet = Channel.GetReaderInfo(
    stop,
    "@path:" + strRecPath,
    "xml",
    out results,
    out strOutputRecPath,
    out baNewTimestamp,
    out strError);
#endif
                        string strXml = "";
                        // 获得一条记录
                        //return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = GetRecord(
                            strRecPath,
                            out strXml,
                            out baNewTimestamp,
                            out strError);
                        if (nRet == 0)
                        {
                            // TODO: 警告后，把 item 行移除？
                            return -1;
                        }
                        if (nRet == -1)
                            return -1;

                        info.OldXml = strXml;
                        info.Timestamp = baNewTimestamp;
                        nReloadCount++;
                        goto CONTINUE;
                    }

                    info.Timestamp = baNewTimestamp;
                    info.OldXml = info.NewXml;
                    info.NewXml = "";

                    item.BackColor = SystemColors.Window;
                    item.ForeColor = SystemColors.WindowText;

                    nSavedCount++;

                    this.m_nChangedCount--;
                    Debug.Assert(this.m_nChangedCount >= 0, "");

                CONTINUE:
                    stop.SetProgressValue(i);
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
                // this._listviewRecords.Enabled = true;
            }

            // 2013/10/22
            nRet = RefreshListViewLines(items,
                true,
                out strError);
            if (nRet == -1)
                return -1;

            DoViewComment(false);

            strError = "";
            if (nSavedCount > 0)
                strError += "共保存" + this.DbTypeCaption + "记录 " + nSavedCount + " 条";
            if (nReloadCount > 0)
            {
                if (string.IsNullOrEmpty(strError) == false)
                    strError += " ; ";
                strError += "有 " + nReloadCount + " 条" + this.DbTypeCaption + "记录因为时间戳不匹配而重新装载旧记录部分(请观察后重新保存)";
            }

            return 0;
        }

        internal static void ChangeField(ref XmlDocument dom,
    string strElementName,
    string strNewValue,
    ref StringBuilder debug,
    ref bool bChanged)
        {
            string strOldValue = DomUtil.GetElementText(dom.DocumentElement,
    strElementName);

            if (strOldValue != strNewValue)
            {
                DomUtil.SetElementText(dom.DocumentElement,
                    strElementName,
                    strNewValue);
                bChanged = true;

                debug.Append("<" + strElementName + "> '" + strOldValue + "' --> '" + strNewValue + "'\r\n");
            }
        }

        internal static void ChangeField(ref XmlDocument dom,
            string strElementName,
            string strAttrName,
            string strNewValue,
            ref StringBuilder debug,
            ref bool bChanged)
        {
            XmlNode node = dom.DocumentElement.SelectSingleNode(strElementName);
            if (node == null)
            {
                if (string.IsNullOrEmpty(strAttrName) == true)
                    return;
                node = dom.CreateElement(strElementName);
                dom.DocumentElement.AppendChild(node);
            }

            string strOldValue = DomUtil.GetElementText(node,
                strElementName);

            if (strOldValue != strNewValue)
            {
                DomUtil.SetAttr(node,
                    strAttrName,
                    strNewValue);
                bChanged = true;

                debug.Append("<" + strElementName + " " + strAttrName + "=? > '" + strOldValue + "' --> '" + strNewValue + "'\r\n");
            }
        }

        // 快速修改记录
        // return:
        //      -1  呼错
        //      0   放弃
        //      1   成功
        internal int QuickChangeItemRecords(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this._listviewRecords.SelectedItems.Count == 0)
            {
                strError = "尚未选择要快速修改的" + this.DbTypeCaption + "记录事项";
                return -1;
            }


            List<OneAction> actions = null;
            XmlDocument cfg_dom = null;

            if (this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment"
                || this.DbType == "patron")
            {
                ChangeItemActionDialog dlg = new ChangeItemActionDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.DbType = this.DbType;
                dlg.Text = "快速修改" + this.DbTypeCaption + "记录 -- 请指定动作参数";
                dlg.MainForm = this.MainForm;
                dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
                dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

                this.MainForm.AppInfo.LinkFormState(dlg, this.DbType + "searchform_quickchangedialog_state");
                dlg.ShowDialog(this);
                this.MainForm.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return 0;   // 放弃

                actions = dlg.Actions;
                cfg_dom = dlg.CfgDom;
            }

            DateTime now = DateTime.Now;

            // TODO: 检查一下，看看是否一项修改动作都没有
            this.MainForm.OperHistory.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 开始执行快速修改" + this.DbTypeCaption + "记录</div>");

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("快速修改" + this.DbTypeCaption + "记录 ...");
            stop.BeginLoop();

            EnableControls(false);
            this._listviewRecords.Enabled = false;

            int nProcessCount = 0;
            int nChangedCount = 0;
            try
            {
                stop.SetProgressRange(0, this._listviewRecords.SelectedItems.Count);

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (ListViewItem item in this._listviewRecords.SelectedItems)
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
    new string [] {"重新修改","继续修改","放弃"});
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

                ListViewPatronLoader loader = new ListViewPatronLoader(this.Channel,
                    stop,
                    items,
                    this.m_biblioTable);
                loader.DbTypeCaption = this.DbTypeCaption;

                int i = 0;
                foreach (LoaderItem item in loader)
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null
                        && stop.State != 0)
                    {
                        strError = "用户中断";
                        return -1;
                    }

                    stop.SetProgressValue(i);

                    BiblioInfo info = item.BiblioInfo;

                    Debug.Assert(info != null, "");

                    this.MainForm.OperHistory.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(info.RecPath) + "</div>");

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        if (bOldSource == true)
                        {
                            dom.LoadXml(info.OldXml);
                            // 放弃上一次的修改
                            if (string.IsNullOrEmpty(info.NewXml) == false)
                            {
                                info.NewXml = "";
                                this.m_nChangedCount--;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == false)
                                dom.LoadXml(info.NewXml);
                            else
                                dom.LoadXml(info.OldXml);
                        }
                    }
                    catch (Exception ex)
                    {
                        strError = "装载XML到DOM时发生错误: " + ex.Message;
                        return -1;
                    }

                    string strDebugInfo = "";


                        // 修改一个订购记录 XmlDocument
                        // return:
                        //      -1  出错
                        //      0   没有实质性修改
                        //      1   发生了修改
                        nRet = ModifyItemRecord(
                            cfg_dom,
                            actions,
                            ref dom,
                            now,
                            out strDebugInfo,
                            out strError);
                        if (nRet == -1)
                            return -1;

                    this.MainForm.OperHistory.AppendHtml("<div class='debug normal'>" + HttpUtility.HtmlEncode(strDebugInfo).Replace("\r\n", "<br/>") + "</div>");

                    nProcessCount++;

                    if (nRet == 1)
                    {
                        string strXml = dom.OuterXml;
                        Debug.Assert(info != null, "");
                        if (info != null)
                        {
                            if (string.IsNullOrEmpty(info.NewXml) == true)
                                this.m_nChangedCount++;
                            info.NewXml = strXml;
                        }

                        item.ListViewItem.BackColor = SystemColors.Info;
                        item.ListViewItem.ForeColor = SystemColors.InfoText;
                    }

                    i++;
                    nChangedCount++;
                }
            }
            finally
            {
                EnableControls(true);
                this._listviewRecords.Enabled = true;

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                this.MainForm.OperHistory.AppendHtml("<div class='debug end'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString()) + " 结束快速修改" + this.DbTypeCaption + "记录</div>");
            }

            DoViewComment(false);
            strError = "修改" + this.DbTypeCaption + "记录 " + nChangedCount.ToString() + " 条 (共处理 " + nProcessCount.ToString() + " 条)\r\n\r\n(注意修改并未自动保存。请在观察确认后，使用保存命令将修改保存回" + this.DbTypeCaption + "库)";
            return 1;
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
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

        // 修改一个订购记录 XmlDocument
        // return:
        //      -1  出错
        //      0   没有实质性修改
        //      1   发生了修改
        int ModifyItemRecord(
            XmlDocument cfg_dom,
            List<OneAction> actions,
            ref XmlDocument dom,
            DateTime now,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            bool bChanged = false;
            int nRet = 0;

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
#if NO
                    // 从 CfgDom 中查找元素名
                    XmlNode node = cfg_dom.DocumentElement.SelectSingleNode("//caption[@text='"+action.FieldName+"']");
                    if (node == null)
                    {
                        strError = "字段名 '"+action.FieldName+"' 在配置文件中没有定义";
                        return -1;
                    }
                    strElementName = DomUtil.GetAttr(node, "element");
#endif
                    nRet = OneActionDialog.GetElementName(
                        cfg_dom,
                        action.FieldName,
                        out strElementName,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }

                if (string.IsNullOrEmpty(action.Action) == true)
                {
                    // 替换内容

                    if (strElementName.IndexOf("@") == -1)
                    {
                        ChangeField(ref dom,
    strElementName,
    action.FieldValue,
    ref debug,
    ref bChanged);
                    }
                    else
                    {
                        string strElement = "";
                        string strAttrName = "";

                        ParseElementName(strElementName, out strElement, out strAttrName);
                        ChangeField(ref dom,
strElement,
strAttrName,
    action.FieldValue,
ref debug,
ref bChanged);
                    }
                }
                else
                {
                    // add/remove
                    string strState = DomUtil.GetElementText(dom.DocumentElement,
                        strElementName);

                    string strOldState = strState;

                    if (action.Action == "add")
                    {
                        if (String.IsNullOrEmpty(action.FieldValue) == false)
                            StringUtil.SetInList(ref strState, action.FieldValue, true);
                    }
                    if (action.Action == "remove")
                    {
                        if (String.IsNullOrEmpty(action.FieldValue) == false)
                            StringUtil.SetInList(ref strState, action.FieldValue, false);
                    }

                    if (strOldState != strState)
                    {
                        DomUtil.SetElementText(dom.DocumentElement,
                            strElementName,
                            strState);
                        bChanged = true;

                        debug.Append("<" + strElementName + "> '" + strOldState + "' --> '" + strState + "'\r\n");
                    }
                }
            }

            strDebugInfo = debug.ToString();

            if (bChanged == true)
                return 1;

            return 0;
        }

        // 解析 element@attr
        static void ParseElementName(string strText,
            out string strElementName,
            out string strAttrName)
        {
            strElementName = "";
            strAttrName = "";

            int nRet = strText.IndexOf("@");
            if (nRet == -1)
            {
                strElementName = strText;
                return;
            }

            strElementName = strText.Substring(0, nRet);
            strAttrName = strText.Substring(nRet + 1);
        }

        Hashtable _tableColIndex = new Hashtable();

        // 获得指定类型的的列的值
        // 通过 browse 配置文件中的类型来指定
        // parameters:
        //      nDelta  要调整的列号基数值。一般为 1
        // return:
        //      -1  出错
        //      0   指定的列没有找到
        //      1   找到
        public int GetTypedColumnText(
            ListViewItem item,
            string strType,
            int nDelta,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            // 根据记录路径获得数据库名
            string strItemDbName = Global.GetDbName(strRecPath);
            // 根据数据库名获得 册条码号 列号

            int nCol = -1;
            object o = _tableColIndex[strItemDbName + "|" + strType];
            if (o == null)
            {
                ColumnPropertyCollection temp = this.MainForm.GetBrowseColumnProperties(strItemDbName);
                nCol = temp.FindColumnByType(strType);
                if (nCol == -1)
                {
                    // 这个实体库没有在 browse 文件中 册条码号 列
                    strError = "警告：" + this.DbTypeCaption + "库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 " + strType + " 的列。请注意刷新或修改此配置文件";
                    return 0;
                }

                nCol += nDelta;


                _tableColIndex[strItemDbName + "|" + strType] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");

            strResult = ListViewUtil.GetItemText(item, nCol);

            return 1;
        }

        /// <summary>
        /// 向浏览框末尾新加入一行
        /// </summary>
        /// <param name="strLine">要加入的行内容。每列内容之间用字符 '\t' 间隔</param>
        /// <returns>新创建的 ListViewItem 对象</returns>
        public ListViewItem AddLineToBrowseList(string strLine)
        {
            ListViewItem item = Global.BuildListViewItem(
    this._listviewRecords,
    strLine);

            this._listviewRecords.Items.Add(item);
            return item;
        }
    }
}
