using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Collections;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 和实体检索有关的基础功能
    /// 被 ItemSearchForm 和 LabelPrintForm 所使用
    /// </summary>
    public class ItemSearchFormBase : SearchFormBase
    {
        // 实体数据库名 --> 册条码号 列号
        // internal Hashtable m_tableBarcodeColIndex = new Hashtable();

        // internal bool m_bBiblioSummaryColumn = true; // 是否在浏览列表中 加入书目摘要列
        internal int m_nBiblioSummaryColumn = 1; // 在浏览列表中要加入的书目信息列数

        internal bool m_bFirstColumnIsKey = false; // 当前listview浏览列的第一列是否应为key

        // 最近使用过的记录路径文件名
        internal string m_strUsedRecPathFilename = "";

        // 最近使用过的条码号文件名
        internal string m_strUsedBarcodeFilename = "";

        /// <summary>
        /// 当前窗口查询的数据库类型，用于显示的名称形态
        /// </summary>
        public override string DbTypeCaption
        {
            get
            {
                if (this.DbType == "item")
                    return "册";
                else if (this.DbType == "comment")
                    return "评注";
                else if (this.DbType == "order")
                    return "订购";
                else if (this.DbType == "issue")
                    return "期";
                else if (this.DbType == "arrive")
                    return "预约到书";
                else
                    throw new Exception("未知的DbType '" + this.DbType + "'");
            }
        }

        // TODO: 如果册条码号为空，则根据参考 ID 返回 @refID:xxxxxxx 这样形式的内容
        // 根据 ListViewItem 对象，获得册条码号列的内容
        // parameters:
        //      bWarning    是否出现警告对话框
        /// <summary>
        /// 根据浏览行 ListViewItem 对象，获得册条码号列的内容
        /// </summary>
        /// <param name="item">浏览行 ListViewItem 对象</param>
        /// <param name="bWarning">当需要时，是否出现警告对话框</param>
        /// <param name="strBarcode">返回册条码号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>01: 出错; 0: 成功</returns>
        public int GetItemBarcode(
            ListViewItem item,
            bool bWarning,
            out string strBarcode,
            out string strError)
        {
            strError = "";
            strBarcode = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            // 根据记录路径获得数据库名
            string strItemDbName = Global.GetDbName(strRecPath);
            if (string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "从(来自事项第一列的)记录路径 '" + strRecPath + "' 获得库名时出错";
                return -1;
            }

            // 根据数据库名获得 册条码号 列号
            // return:
            //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
            //      其他  返回列 index
            int nCol = GetColumnIndex(strItemDbName, "item_barcode");
            if (nCol == -1)
            {
                // 这个实体库没有在 browse 文件中 册条码号 列
                strError = "警告：实体库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                if (bWarning == true)
                    MessageBox.Show(this, strError);

                nCol = 0;   // 这个大部分情况能奏效

                if (m_nBiblioSummaryColumn == 0)
                    nCol += 1;
                else
                    nCol += m_nBiblioSummaryColumn + 1;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12
            }

            strBarcode = ListViewUtil.GetItemText(item, nCol);
            return 0;
#if NO
            int nCol = -1;
            object o = m_tableBarcodeColIndex[strItemDbName];
            if (o == null)
            {
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                nCol = temp.FindColumnByType("item_barcode");
                if (nCol == -1)
                {
                    // 这个实体库没有在 browse 文件中 册条码号 列
                    strError = "警告：实体库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                    if (bWarning == true)
                        MessageBox.Show(this, strError);

                    nCol = 0;   // 这个大部分情况能奏效
                }
                if (m_bBiblioSummaryColumn == false)
                    nCol += 1;
                else
                    nCol += 2;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12

                m_tableBarcodeColIndex[strItemDbName] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");

            strBarcode = ListViewUtil.GetItemText(item, nCol);
            return 0;
#endif
        }

#if NO
                int GetBarcodeColumnIndex(string strItemDbName)
        {
            int nCol = -1;
            object o = m_tableBarcodeColIndex[strItemDbName];
            if (o == null)
            {
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                nCol = temp.FindColumnByType("item_barcode");
                if (nCol == -1)
                {
#if NO
                    // 这个实体库没有在 browse 文件中 册条码号 列
                    strError = "警告：实体库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 item_barcode 的列。请注意刷新或修改此配置文件";
                    if (bWarning == true)
                        MessageBox.Show(this, strError);
#endif

                    nCol = 0;   // 这个大部分情况能奏效
                }
                if (m_bBiblioSummaryColumn == false)
                    nCol += 1;
                else
                    nCol += 2;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12

                m_tableBarcodeColIndex[strItemDbName] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");
            return nCol;
        }
#endif

        // 获得一个列 type 对应的列 index，针对特定的数据库
        // return:
        //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
        //      其他  返回列 index
        int GetColumnIndex(string strItemDbName, string strColumnType)
        {
            int nCol = -1;

            // 根据数据库名和列 type 去获得 列号
            string strCacheKey = strItemDbName + ":" + strColumnType;

            object o = _columnIndexTable[strCacheKey];
            if (o == null)
            {
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                if (temp == null)
                    return -1;
                nCol = temp.FindColumnByType(strColumnType);
                if (nCol == -1)
                    return -1;

                if (m_nBiblioSummaryColumn == 0)
                    nCol += 1;
                else
                    nCol += m_nBiblioSummaryColumn + 1;

                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12

                _columnIndexTable[strCacheKey] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");
            return nCol;
        }

        public void ClearColumnIndexCache()
        {
            _columnIndexTable.Clear();
        }

        // 列 type 到 列号 index 的对照表
        // key 格式为 数据库名:type，value 为 列 index
        Hashtable _columnIndexTable = new Hashtable();

        // 2015/6/14
        // 获得特定角色的列的值
        // return:
        //      -2  没有找到列 type
        //      -1  出错
        //      >=0 列号
        public int GetColumnText(
            ListViewItem item,
            string strColumnType,
            out string strText,
            out string strError)
        {
            strError = "";
            strText = "";

            string strRecPath = ListViewUtil.GetItemText(item, 0);
            // 根据记录路径获得浏览记录的数据库名。注意，不一定是实体库名
            string strItemDbName = Global.GetDbName(strRecPath);

            if (string.IsNullOrEmpty(strItemDbName) == true)
            {
                strError = "从(来自事项第一列的)记录路径 '" + strRecPath + "' 获得库名时出错";
                return -1;
            }

            // return:
            //      -1  该数据库 browse 配置文件中没有找到这个列 type 定义
            //      其他  返回列 index
            int nCol = GetColumnIndex(strItemDbName, strColumnType);
            if (nCol == -1)
            {
                // 这个实体库没有在 browse 文件中 册条码号 列
                strError = "数据库 '" + strItemDbName + "' 的 browse 配置文件中没有定义 type 为 " + strColumnType + " 的列。请注意刷新或修改此配置文件";
                return -2;
            }

            Debug.Assert(nCol > 0, "");

            strText = ListViewUtil.GetItemText(item, nCol);
            return nCol;
        }

        /// <summary>
        /// 从记录路径文件导入
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <param name="strStyle">风格。"clear" 表示加入新内容前试图清除 list 中已有的内容，会出现对话框警告。如果没有这个值，则追加到现有内容后面</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错 0: 放弃处理 1:正常结束</returns>
        public int ImportFromRecPathFile(string strFileName,
            string strStyle,
            out string strError)
        {
            strError = "";

            SetStatusMessage("");   // 清除以前残留的显示

            if (string.IsNullOrEmpty(strFileName) == true)
            {
                OpenFileDialog dlg = new OpenFileDialog();

                dlg.Title = "请指定要打开的" + this.DbTypeCaption + "记录路径文件名";
                dlg.FileName = this.m_strUsedRecPathFilename;
                dlg.Filter = "记录路径文件 (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() != DialogResult.OK)
                    return 0;

                this.m_strUsedRecPathFilename = dlg.FileName;
            }
            else
                this.m_strUsedRecPathFilename = strFileName;

            StreamReader sr = null;
            // bool bSkipBrowse = false;

            try
            {
                // TODO: 最好自动探测文件的编码方式?
                sr = new StreamReader(this.m_strUsedRecPathFilename, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                strError = "打开文件 " + this.m_strUsedRecPathFilename + " 失败: " + ex.Message;
                goto ERROR1;
            }

            stop.Style = StopStyle.EnableHalfStop;
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入记录路径 ...");
            stop.BeginLoop();

            LibraryChannel channel = this.GetChannel();

            this.EnableControls(false);
            try
            {
                // 导入的事项是没有序的，因此需要清除已有的排序标志
                ListViewUtil.ClearSortColumns(this._listviewRecords);
                stop.SetProgressRange(0, sr.BaseStream.Length);

                List<ListViewItem> items = new List<ListViewItem>();

                if (this._listviewRecords.Items.Count > 0
                    && StringUtil.IsInList("clear", strStyle) == true)
                {
                    DialogResult result = MessageBox.Show(this,
                        "导入前是否要清除命中记录列表中的现有的 " + this._listviewRecords.Items.Count.ToString() + " 行?\r\n\r\n(如果不清除，则新导入的行将追加在已有行后面)\r\n(Yes 清除；No 不清除(追加)；Cancel 放弃导入)",
                        this.DbType + "SearchForm",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return 0;
                    if (result == DialogResult.Yes)
                    {
                        ClearListViewItems();
                    }
                }

                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (stop != null && stop.State != 0)
                    {
                        MessageBox.Show(this, "用户中断");
                        return 0;
                    }

                    string strRecPath = sr.ReadLine();

                    stop.SetProgressValue(sr.BaseStream.Position);

                    if (strRecPath == null)
                        break;

                    // 检查路径的正确性，检查数据库是否为实体库之一
                    string strDbName = Global.GetDbName(strRecPath);
                    if (string.IsNullOrEmpty(strDbName) == true)
                    {
                        strError = "'" + strRecPath + "' 不是合法的记录路径";
                        goto ERROR1;
                    }

                    if (this.DbType == "item")
                    {
                        if (Program.MainForm.IsItemDbName(strDbName) == false)
                        {
                            strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的实体库名。很可能所指定的文件不是实体库的记录路径文件";
                            goto ERROR1;
                        }
                    }
                    else if (this.DbType == "comment")
                    {
                        if (Program.MainForm.IsCommentDbName(strDbName) == false)
                        {
                            strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的评注库名。很可能所指定的文件不是评注库的记录路径文件";
                            goto ERROR1;
                        }
                    }
                    else if (this.DbType == "order")
                    {
                        if (Program.MainForm.IsOrderDbName(strDbName) == false)
                        {
                            strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的订购库名。很可能所指定的文件不是订购库的记录路径文件";
                            goto ERROR1;
                        }
                    }
                    else if (this.DbType == "issue")
                    {
                        if (Program.MainForm.IsIssueDbName(strDbName) == false)
                        {
                            strError = "路径 '" + strRecPath + "' 中的数据库名 '" + strDbName + "' 不是合法的期库名。很可能所指定的文件不是期库的记录路径文件";
                            goto ERROR1;
                        }
                    }
                    else
                        throw new Exception("未知的DbType '" + this.DbType + "'");

                    ListViewItem item = new ListViewItem();
                    item.Text = strRecPath;

                    this._listviewRecords.Items.Add(item);

                    items.Add(item);

#if NO
                    if (bSkipBrowse == false
                        && !(Control.ModifierKeys == Keys.Control))
                    {
                        int nRet = RefreshBrowseLine(item,
                out strError);
                        if (nRet == -1)
                        {
                            DialogResult result = MessageBox.Show(this,
                                "获得浏览内容时出错: " + strError + "。\r\n\r\n是否继续获取浏览内容? (Yes 获取；No 不获取；Cancel 放弃导入)",
                                this.DbType + "SearchForm",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);
                            if (result == System.Windows.Forms.DialogResult.No)
                                bSkipBrowse = true;
                            if (result == System.Windows.Forms.DialogResult.Cancel)
                            {
                                strError = "已中断";
                                break;
                            }
                        }
                    }
#endif

                }

                // 刷新浏览行
                int nRet = RefreshListViewLines(
                    channel,
                    items,
                    "",
                    false,
                    true,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 2014/1/15
                // 刷新书目摘要
                nRet = FillBiblioSummaryColumn(
                    channel,
                    items,
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            finally
            {
                this.EnableControls(true);

                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();
                stop.Style = StopStyle.None;

                if (sr != null)
                    sr.Close();
            }
            return 1;
        ERROR1:
            return -1;
            // MessageBox.Show(this, strError);
        }

        internal void ClearListViewItems()
        {
            this._listviewRecords.Items.Clear();

            /*
            // 2008/11/22 
            this.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(this.listView_records.Columns);
             * */
            ListViewUtil.ClearSortColumns(this._listviewRecords);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this._listviewRecords.Columns.Count; i++)
            {
                this._listviewRecords.Columns[i].Text = i.ToString();
            }

            ClearBiblioTable();
            ClearCommentViewer();
        }

        // 刷新一行内除了记录路径外的其他列
        // parameters:
        //      cols    检索结果中的浏览列
        internal override void RefreshOneLine(ListViewItem item,
            string[] cols,
            bool bClearRestColumns)
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
                    if (this.m_nBiblioSummaryColumn == 0)
                        ListViewUtil.ChangeItemText(item,
                        c + 1,
                        cols[c]);
                    else
                        ListViewUtil.ChangeItemText(item,
                        c + (m_nBiblioSummaryColumn + 1),
                        cols[c]);
                }

                if (bClearRestColumns)
                {
                    // 清除余下的列内容
                    if (this.m_nBiblioSummaryColumn == 0)
                        c += 1;
                    else
                        c += m_nBiblioSummaryColumn + 1;

                    for (; c < item.SubItems.Count; c++)
                    {
                        item.SubItems[c].Text = "";
                    }
                }
            }
        }

        // 实体数据库名 --> parent id 列号
        internal Hashtable m_tableSummaryColIndex = new Hashtable();

        // parameters:
        //      bBeginLoop  是否要 BeginLoop()。在大循环中使用本函数，要用 false    
        // return:
        //      -1  出错
        //      0   用户中断
        //      1   完成
        internal int FillBiblioSummaryColumn(
            LibraryChannel channel,
            List<ListViewItem> items,
            bool bBeginLoop,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (m_nBiblioSummaryColumn == 0)
                return 0;

            Debug.Assert(this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment", "");

            if (stop != null && bBeginLoop == true)
            {
                stop.Style = StopStyle.EnableHalfStop;
                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在刷新浏览行的书目摘要 ...");
                stop.BeginLoop();

                this.EnableControls(false);

                if (stop != null)
                    stop.SetProgressRange(0, items.Count);
            }
            try
            {
                List<ListViewItem> batch = new List<ListViewItem>();
                long lStartIndex = 0;
                // 每 1000 个事项利用一次枚举器，避免 Hashtable 膨胀太大
                foreach (ListViewItem item in items)
                {
                    batch.Add(item);
                    if (batch.Count >= 1000)
                    {
                        // return:
                        //      -2  获得书目摘要的权限不够
                        //      -1  出错
                        //      0   用户中断
                        //      1   完成
                        nRet = _fillBiblioSummaryColumn(
                            channel,
                            batch,
                            lStartIndex,
                            bBeginLoop,
                            true,
                            out strError);
                        if (nRet == -1 || nRet == -2)
                            return -1;
                        if (nRet == 0)
                            return 0;
                        lStartIndex += batch.Count;
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    // return:
                    //      -2  获得书目摘要的权限不够
                    //      -1  出错
                    //      0   用户中断
                    //      1   完成
                    nRet = _fillBiblioSummaryColumn(
                        channel,
                        batch,
                        lStartIndex,
                        bBeginLoop,
                        true,
                        out strError);
                    if (nRet == -1 || nRet == -2)
                        return -1;
                    if (nRet == 0)
                        return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                strError = "FillBiblioSummaryColumn() 出现异常: " + ex.Message;
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

        public virtual List<Order.ColumnProperty> GetBiblioColumns()
        {
            return null;
        }

        public static List<string> GetCaptionList(List<Order.ColumnProperty> defs)
        {
            List<string> results = new List<string>();
            foreach (var def in defs)
            {
                string type = def.Type;
                if (type.StartsWith("biblio_") == false)
                    continue;

                results.Add(def.Caption);
            }

            return results;
        }

        static List<string> GetTypeList(List<Order.ColumnProperty> defs)
        {
            List<string> results = new List<string>();
            foreach(var def in defs)
            {
                string type = def.Type;
                if (type.StartsWith("biblio_") == false)
                    continue;

                type = type.Substring("biblio_".Length);
                results.Add(type);
            }

            return results;
        }

        // parameters:
        //      lStartIndex 调用前已经做过的事项数。为了准确显示 Progress
        // return:
        //      -2  获得书目摘要的权限不够
        //      -1  出错
        //      0   用户中断
        //      1   完成
        internal int _fillBiblioSummaryColumn(
        LibraryChannel channel,
        List<ListViewItem> items,
        long lStartIndex,
        bool bDisplayMessage,
        bool bAutoSearch,
        out string strError)
        {
            strError = "";
            int nRet = 0;

            if (m_nBiblioSummaryColumn == 0)
                return 0;

            Debug.Assert(this.DbType == "item"
                || this.DbType == "order"
                || this.DbType == "issue"
                || this.DbType == "comment"
                || this.DbType == "arrive",
                "");

            bool bShowed = false;

            List<string> biblio_recpaths = new List<string>();  // 尺寸可能比 items 数组小，没有包含里面不具有 parent id 列的事项
            List<int> colindex_list = new List<int>();  // 存储每个 item 对应的 parent id colindex。数组大小等于 items 数组大小

            foreach (ListViewItem item in items)
            {
#if NO
                string strRecPath = ListViewUtil.GetItemText(item, 0);
                // 根据记录路径获得数据库名
                string strItemDbName = Global.GetDbName(strRecPath);
                // 根据数据库名获得 parent id 列号

                int nCol = -1;
                object o = m_tableSummaryColIndex[strItemDbName];
                if (o == null)
                {
                    ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                    nCol = temp.FindColumnByType("parent_id");
                    if (nCol == -1)
                    {
                        colindex_list.Add(-1);
                        continue;   // 这个实体库没有 parent id 列
                    }
                    nCol += 2;
                    if (this.m_bFirstColumnIsKey == true)
                        nCol++; // 2013/11/12
                    m_tableSummaryColIndex[strItemDbName] = nCol;   // 储存起来
                }
                else
                    nCol = (int)o;

                Debug.Assert(nCol > 0, "");

                colindex_list.Add(nCol);

                // 获得 parent id
                string strText = ListViewUtil.GetItemText(item, nCol);

                string strBiblioRecPath = "";
                // 看看是否已经是路径 ?
                if (strText.IndexOf("/") == -1)
                {
                    // 获得对应的书目库名
                    strBiblioRecPath = Program.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        strError = "数据库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        return -1;
                    }
                    strBiblioRecPath = strBiblioRecPath + "/" + strText;

                    ListViewUtil.ChangeItemText(item, nCol, strBiblioRecPath);
                }
                else
                    strBiblioRecPath = strText;
#endif
                int nCol = -1;
                // 获得事项所从属的书目记录的路径
                // return:
                //      -1  出错
                //      0   相关数据库没有配置 parent id 浏览列
                //      1   找到
                nRet = GetBiblioRecPath(
                    channel,
                    item,
                    bAutoSearch,   // true 如果遇到没有 parent id 列的时候速度较慢
                    out nCol,
                    out string strBiblioRecPath,
                    out strError);
                if (nRet == -1)
                {
                    if (bShowed == false)
                    {
                        MessageBox.Show(this, strError);
                        bShowed = true;
                    }
                    colindex_list.Add(-1);
                    continue;
                    // return -1;
                }
                if (nRet == 0)
                {
                    colindex_list.Add(-1);
                    continue;
                }

                // 2017/7/5
                if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                {
                    colindex_list.Add(-1);
                    continue;
                }

                if (string.IsNullOrEmpty(strBiblioRecPath) == false
                    && nCol == -1)
                    colindex_list.Add(0);
                else
                    colindex_list.Add(nCol);

                if (string.IsNullOrEmpty(strBiblioRecPath) == false)
                    biblio_recpaths.Add(strBiblioRecPath);
                else
                {
                    Debug.Assert(false, "");
                }
            }

            var column_def = GetBiblioColumns();
            string format = "summary";
            if (column_def != null)
                format = "table:" + StringUtil.MakePathList(GetTypeList(column_def), "|");

            CacheableBiblioLoader loader = new CacheableBiblioLoader
            {
                Channel = channel,
                Stop = this.stop,
                Format = format,    // "summary", "table",
                GetBiblioInfoStyle = GetBiblioInfoStyle.None,
                RecPaths = biblio_recpaths
            };

            loader.Prompt += loader_Prompt;

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

                int nCol = colindex_list[i];
                if (nCol == -1)
                {
                    ListViewUtil.ChangeItemText(item,
                        this.m_bFirstColumnIsKey == false ? 1 : 2,
                        "");
                    ClearOneChange(item, true); // 清除内存中的修改
                    i++;
                    continue;
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


                if (column_def == null)
                    ListViewUtil.ChangeItemText(item,
                        this.m_bFirstColumnIsKey == false ? 1 : 2,
                        biblio.Content);
                else
                {
                    var columns = GetBiblioColumnText(column_def,
biblio.Content, biblio.RecPath);
                    int start = this.m_bFirstColumnIsKey == false ? 1 : 2;
                    foreach (string text in columns)
                    {
                        ListViewUtil.ChangeItemText(item,
        start++,
        text);
                    }
                }

                ClearOneChange(item, true); // 清除内存中的修改
                i++;
            }

            return 1;
        }

        static List<string> GetBiblioColumnText(List<Order.ColumnProperty> columns,
            string xml,
            string biblio_recpath)
        {
            List<string> results = new List<string>();
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(xml);
            }
            catch (Exception ex)
            {
                results.Add($"!error:书目表格数据出错:{ex.Message}");
                return results;
            }

            foreach (var column in columns)
            {
                string type = column.Type;

                if (type.StartsWith("biblio_") == false)
                {
                    results.Add("");
                    continue;
                }

                type = type.Substring("biblio_".Length);

                if (type == "recpath")
                {
                    results.Add(biblio_recpath);
                    continue;
                }

                XmlElement line = dom.DocumentElement.SelectSingleNode($"line[@type='{type}']") as XmlElement;
                if (line == null)
                {
                    results.Add("");
                    continue;
                }

                results.Add(line.GetAttribute("value")?.Replace("\n", " "));
            }

            return results;
        }

        void loader_Prompt(object sender, MessagePromptEventArgs e)
        {
            if (e.Actions == "yes,no,cancel")
            {
                DialogResult result = AutoCloseMessageBox.Show(this,
    e.MessageText + "\r\n\r\n将自动重试操作\r\n\r\n(点右上角关闭按钮可以中断批处理)",
    20 * 1000,
    "ItemSearchForm");
                if (result == DialogResult.Cancel)
                    e.ResultAction = "cancel";
                else if (result == System.Windows.Forms.DialogResult.No)
                    e.ResultAction = "no";
                else
                    e.ResultAction = "yes";
            }
        }

        /*
操作类型 crashReport -- 异常报告 
主题 dp2circulation 
发送者 renyh@xxx 
媒体类型 text 
内容 发生未捕获的界面线程异常: 
Type: System.ArgumentException
Message: RecPaths 中不应包含空元素
Stack:
在 dp2Circulation.CacheableBiblioLoader.<GetEnumerator>d__0.MoveNext()
在 dp2Circulation.ItemSearchFormBase._fillBiblioSummaryColumn(LibraryChannel channel, List`1 items, Int64 lStartIndex, Boolean bDisplayMessage, Boolean bAutoSearch, String& strError)
在 dp2Circulation.ItemSearchForm.FillBrowseList(LibraryChannel channel, ItemQueryParam query, Int64 lHitCount, Boolean bOutputKeyCount, Boolean bOutputKeyID, Boolean bQuickLoad, String& strError)
在 dp2Circulation.ItemSearchForm.DoSearch(Boolean bOutputKeyCount, Boolean bOutputKeyID, ItemQueryParam input_query, Boolean bClearList)
在 dp2Circulation.ItemSearchForm.toolStripButton_search_Click(Object sender, EventArgs e)
在 System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)
在 System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)
在 System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)
在 System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)
在 System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)
在 System.Windows.Forms.Control.WndProc(Message& m)
在 System.Windows.Forms.ToolStrip.WndProc(Message& m)
在 System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)


dp2Circulation 版本: dp2Circulation, Version=2.28.6347.382, Culture=neutral, PublicKeyToken=null
操作系统：Microsoft Windows NT 6.2.9200.0
操作时间 2017/5/18 12:30:01 (Thu, 18 May 2017 12:30:01 +0800) 
前端地址 xxx 经由 http://dp2003.com/dp2library 
         * * */
        // 获得事项所从属的书目记录的路径
        // parameters:
        //      bAutoSearch 当没有 parent id 列的时候，是否自动进行检索以便获得书目记录路径
        // return:
        //      -1  出错
        //      0   相关数据库没有配置 parent id 浏览列
        //      1   找到
        public virtual int GetBiblioRecPath(
            LibraryChannel channel,
            ListViewItem item,
            bool bAutoSearch,
            out int nCol,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            nCol = -1;
            strBiblioRecPath = "";
            int nRet = 0;

            // 这是事项记录路径
            string strRecPath = ListViewUtil.GetItemText(item, 0);

            if (string.IsNullOrEmpty(strRecPath) == true)
            {
                strError = "浏览行记录路径列没有内容，无法获得书目记录路径";
                return -1;
            }

            // 根据记录路径获得数据库名
            string strItemDbName = Global.GetDbName(strRecPath);
            // 根据数据库名获得 parent id 列号

            object o = m_tableSummaryColIndex[strItemDbName];
            if (o == null)
            {
                if (Program.MainForm.NormalDbProperties == null)
                {
                    strError = "普通数据库属性尚未初始化";
                    return -1;
                }
                ColumnPropertyCollection temp = Program.MainForm.GetBrowseColumnProperties(strItemDbName);
                if (temp == null)
                {
                    strError = "实体库 '" + strItemDbName + "' 没有找到列定义";
                    return -1;
                }

                nCol = temp.FindColumnByType("parent_id");
                if (nCol == -1)
                {
                    if (bAutoSearch == false)
                    {
                        strError = "实体库 " + strItemDbName + " 的浏览格式没有配置 parent id 列";
                        return 0;   // 这个实体库没有 parent id 列
                    }

                    // TODO: 这里关于浏览列 type 的判断应该通盘考虑，设计为通用功能，减少对特定库类型的判断依赖

                    string strQueryString = "";
                    if (this.DbType == "item")
                    {
                        Debug.Assert(this.DbType == "item", "");

                        strQueryString = "@path:" + strRecPath;
                    }
                    else if (this.DbType == "arrive")
                    {
                        int nRefIDCol = temp.FindColumnByType("item_refid");
                        if (nRefIDCol == -1)
                        {
                            strError = "预约到书库 " + strItemDbName + " 的浏览格式没有配置 item_refid 列";
                            return 0;
                        }

                        strQueryString = ListViewUtil.GetItemText(item, nRefIDCol + 2);

                        if (string.IsNullOrEmpty(strQueryString) == false)
                        {
                            strQueryString = "@refID:" + strQueryString;
                        }
                        else
                        {
                            int nItemBarcodeCol = temp.FindColumnByType("item_barcode");
                            if (nRefIDCol == -1)
                            {
                                strError = "预约到书库 " + strItemDbName + " 的浏览格式没有配置 item_barcode 列";
                                return 0;
                            }
                            strQueryString = ListViewUtil.GetItemText(item, nItemBarcodeCol + 2);
                            if (string.IsNullOrEmpty(strQueryString) == true)
                            {
                                strError = "册条码号栏为空，无法获得书目记录路径";
                                return 0;
                            }
                        }
                    }

                    string strItemRecPath = "";
                    if (string.IsNullOrEmpty(strQueryString) == false)
                    {
                        Debug.Assert(this.DbType == "item" || this.DbType == "arrive", "");

                        nRet = SearchTwoRecPathByBarcode(
                            this.stop,
                            channel,
                            strQueryString,    // "@path:" + strRecPath,
                            out strItemRecPath,
                            out strBiblioRecPath,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "获得书目记录路径时出错: " + strError;
                            return -1;
                        }
                        else if (nRet == 0)
                        {
                            strError = "检索词 '" + strQueryString + "' 没有找到记录";
                            return -1;
                        }
                        else if (nRet > 1) // 命中发生重复
                        {
                            strError = "检索词 '" + strQueryString + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                            return -1;
                        }
                    }
                    else
                    {
                        nRet = SearchBiblioRecPath(
                            this.stop,
                            channel,
                            this.DbType,
                            strRecPath,
                            out strBiblioRecPath,
                            out strError);
                    }
                    if (nRet == -1)
                    {
                        strError = "获得书目记录路径时出错: " + strError;
                        return -1;
                    }
                    else if (nRet == 0)
                    {
                        strError = "记录路径 '" + strRecPath + "' 没有找到记录";
                        return -1;
                    }
                    else if (nRet > 1) // 命中发生重复
                    {
                        strError = "记录路径 '" + strRecPath + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误";
                        return -1;
                    }

                    return 1;
                }

                nCol += (m_nBiblioSummaryColumn + 1);
                if (this.m_bFirstColumnIsKey == true)
                    nCol++; // 2013/11/12
                m_tableSummaryColIndex[strItemDbName] = nCol;   // 储存起来
            }
            else
                nCol = (int)o;

            Debug.Assert(nCol > 0, "");

            // 获得 parent id
            string strText = ListViewUtil.GetItemText(item, nCol);

            if (string.IsNullOrEmpty(strText) == false)
            {
                // 看看是否已经是路径 ?
                if (strText.IndexOf("/") == -1)
                {
                    // 获得对应的书目库名
                    strBiblioRecPath = Program.MainForm.GetBiblioDbNameFromItemDbName(this.DbType, strItemDbName);
                    if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                    {
                        strError = this.DbTypeCaption + "类型的数据库名 '" + strItemDbName + "' 没有找到对应的书目库名";
                        return -1;
                    }
                    strBiblioRecPath = strBiblioRecPath + "/" + strText;

                    ListViewUtil.ChangeItemText(item, nCol, strBiblioRecPath);
                }
                else
                    strBiblioRecPath = strText;
            }
            else
            {
                // 2017/5/18
                // 装载浏览格式的中途如果修改服务器相关数据库的 browse 配置文件可能会走到这里
                strError = "parent id 列 (index=" + nCol + ") 内容为空";
                return 1;   // 找到了列号，但是该列内容为空
            }
            return 1;
        }

        // return:
        //      false   出现错误
        //      true    成功
        internal bool FillLineByBarcode(
            LibraryChannel channel,
            string strBarcode,
            ListViewItem item)
        {
            string strError = "";
            string strBiblioRecPath = "";
            string strItemRecPath = "";

            Debug.Assert(this.DbType == "item", "");

            // 检索册条码号，检索出其从属的书目记录路径。
            int nRet = SearchTwoRecPathByBarcode(
                this.stop,
                channel,
                strBarcode,
                out strItemRecPath,
                out strBiblioRecPath,
                out strError);
            if (nRet == -1)
            {
                ListViewUtil.ChangeItemText(item, 2, strError);
                return false;
            }
            else if (nRet == 0)
            {
                ListViewUtil.ChangeItemText(item, 2, "条码号 '" + strBarcode + "' 没有找到记录");
                return false;
            }
            else if (nRet == 1)
            {
                item.Text = strItemRecPath;
            }
            else if (nRet > 1) // 命中发生重复
            {
                ListViewUtil.ChangeItemText(item, 2, "条码号 '" + strBarcode + "' 命中 " + nRet.ToString() + " 条记录，这是一个严重错误");
                return false;
            }

            string strItemDbName = Global.GetDbName(strItemRecPath);
            int index = GetColumnIndex(strItemDbName, "item_barcode");
            if (index == -1)
                index = 0;   // 这个大部分情况能奏效

            ListViewUtil.ChangeItemText(item, index, strBarcode);

            // TODO: 将书目记录路径放入item.Tag中备用
            return true;
        }

        // 根据册条码号，检索出其册记录路径和从属的书目记录路径，以及馆藏地点信息。
        public static int SearchTwoRecPathByBarcode(
            Stop stop,
            LibraryChannel channel,
            string strBarcode,
            out string strItemRecPath,
            out string strLocation,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";
            strItemRecPath = "";
            strLocation = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            long lRet = 0;

#if NO
            Debug.Assert(this.DbType == "item", "");

            if (this.DbType == "item")
                lRet = Channel.GetItemInfo(
                     stop,
                     strBarcode,
                     "xml",
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else
                throw new Exception("SearchTwoRecPathByBarcode()只能在DbType=='item'时使用");
#endif
            lRet = channel.GetItemInfo(
     stop,
     strBarcode,
     "xml",
     out strItemText,
     out strItemRecPath,
     out item_timestamp,
     "recpath",
     out strBiblioText,
     out strBiblioRecPath,
     out strError);
            if (lRet == -1)
                return -1;  // error

            if (lRet == 0)
                return 0;   // not found

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemText);
            }
            catch (Exception ex)
            {
                strError = "item XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");

            return (int)lRet;
        }

        // 根据册条码号，检索出其册记录路径和从属的书目记录路径。
        public static int SearchTwoRecPathByBarcode(
            Stop stop,
            LibraryChannel channel,
            string strBarcode,
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

            long lRet = 0;

#if NO
            Debug.Assert(this.DbType == "item", "");

            if (this.DbType == "item")
                lRet = Channel.GetItemInfo(
                     stop,
                     strBarcode,
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else
                throw new Exception("SearchTwoRecPathByBarcode()只能在DbType=='item'时使用");
#endif
            lRet = channel.GetItemInfo(
     stop,
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

            // TODO: 需要检查一下服务器代码，看看什么情况 strBiblioRecPath 会返回空？
            return (int)lRet;   // not found
        }

        // 根据册记录路径，检索出从属的书目记录路径。
        public static int SearchBiblioRecPath(
            Stop stop,
            LibraryChannel channel,
            string strDbType,
            string strItemRecPath,
            out string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            strBiblioRecPath = "";

            string strItemText = "";
            string strBiblioText = "";

            byte[] item_timestamp = null;

            long lRet = 0;

            if (strDbType == "item")
                lRet = channel.GetItemInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else if (strDbType == "comment")
                lRet = channel.GetCommentInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     // "",
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else if (strDbType == "order")
                lRet = channel.GetOrderInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     // "",
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else if (strDbType == "issue")
                lRet = channel.GetIssueInfo(
                     stop,
                     "@path:" + strItemRecPath,
                     // "",
                     null,
                     out strItemText,
                     out strItemRecPath,
                     out item_timestamp,
                     "recpath",
                     out strBiblioText,
                     out strBiblioRecPath,
                     out strError);
            else
                throw new Exception("未知的DbType '" + strDbType + "'");

            if (lRet == -1)
                return -1;  // error

            // TODO: 需要检查一下服务器代码，看什么时候 strBiblioRecPath 会返回空
            return (int)lRet;   // not found
        }
    }
}
