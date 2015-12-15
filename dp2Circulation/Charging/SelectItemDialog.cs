using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
// using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 通过 ISBN 等检索出书目记录，然后列出下属的侧记录供选择的对话框
    /// 用于出纳窗
    /// </summary>
    public partial class SelectItemDialog : MyForm
    {
        /// <summary>
        /// Stop 管理器
        /// </summary>
        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

        // FloatingMessageForm _floatingMessage = null;

        /// <summary>
        /// 自动操作唯一事项
        /// </summary>
        public bool AutoOperSingleItem = false;

        /// <summary>
        /// 功能类型
        /// 根据它决定某些事项显示为灰色文字
        /// </summary>
        public string FunctionType = "borrow";  // borrow/return/renew

        /// <summary>
        /// 验证还书时的读者证条码号
        /// </summary>
        public string VerifyBorrower = "";  // 

        /// <summary>
        /// 是否要在对话框打开时自动开始检索
        /// </summary>
        public bool AutoSearch = false;

        // 命中的书目记录路径
        List<string> _biblioRecPaths = new List<string>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public SelectItemDialog()
        {
            InitializeComponent();

            this.statusStrip1.Renderer = new TransparentToolStripRenderer(this.statusStrip1);

            this.SupressSizeSetting = true;  // 不需要基类 MyForm 的尺寸设定功能
        }

        private void SelectItemDialog_Load(object sender, EventArgs e)
        {
            this.MainForm.FillBiblioFromList(this.comboBox_from);

#if NO
            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.Show(this);
            }
#endif
            this._floatingMessage.RectColor = Color.Purple;

            stopManager.Initial(this.button_stop,
(object)this.toolStripStatusLabel1,
(object)this.toolStripProgressBar1);

            if (stop != null)
                stop.Unregister();

            stop = new DigitalPlatform.Stop();
            stop.Register(stopManager, true);	// 和容器关联

            if (this.AutoSearch == true
                && string.IsNullOrEmpty(this.textBox_queryWord.Text) == false)
            {
                this.BeginInvoke(new Action<object, EventArgs>(button_search_Click), this, new EventArgs());
            }

            if (StringUtil.CompareVersion(this.MainForm.ServerVersion, "2.33") < 0)
            {
                MessageBox.Show(this, "选择册记录功能要求 dp2Library 版本必须在 2.33 以上。当前 dp2Library 的版本为 " + this.MainForm.ServerVersion.ToString() + "，请及时升级");
            }
        }

        private void SelectItemDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            if (_floatingMessage != null)
                _floatingMessage.Close();
#endif
        }

        void SetFloatMessage(string strColor,
            string strText)
        {
            if (strColor == "waiting")
                this._floatingMessage.RectColor = Color.FromArgb(80, 80, 80);
            else
                this._floatingMessage.RectColor = Color.Purple;

            this._floatingMessage.Text = strText;
        }

        // 获得具有实体库的全部书目库名列表
        string GetBiblioDbNames()
        {
            List<string> results = new List<string>();
            if (this.MainForm.BiblioDbProperties != null)
            {
                foreach (BiblioDbProperty prop in this.MainForm.BiblioDbProperties)
                {
                    if (string.IsNullOrEmpty(prop.DbName) == false &&
                        string.IsNullOrEmpty(prop.ItemDbName) == false)
                    {
                        results.Add(prop.DbName);
                    }
                }
            }

            return StringUtil.MakePathList(results);
        }

        int m_nInSearching = 0;

        private void button_search_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 按下 Control 的时候空字符串也可以检索
            if (!(Control.ModifierKeys == Keys.Control)
                && string.IsNullOrEmpty(this.textBox_queryWord.Text) == true)
            {
                strError = "请输入检索词";
                goto ERROR1;
            }

            this._biblioRecPaths.Clear();
            this.dpTable_items.Rows.Clear();

            LibraryChannel channel = this.GetChannel();

            Progress.Style = StopStyle.EnableHalfStop;
            Progress.OnStop += new StopEventHandler(this.DoStop);
            Progress.Initial("正在检索 ...");
            Progress.BeginLoop();

            this.EnableControls(false);

            m_nInSearching++;

            this.SetFloatMessage("waiting", "正在检索 ...");

            try
            {
                if (this.comboBox_from.Text == "")
                {
                    strError = "尚未选定检索途径";
                    goto ERROR1;
                }
                string strFromStyle = "";

                try
                {
                    strFromStyle = this.MainForm.GetBiblioFromStyle(this.comboBox_from.Text);
                }
                catch (Exception ex)
                {
                    strError = "GetBiblioFromStyle() exception: " + ExceptionUtil.GetAutoText(ex);
                    goto ERROR1;
                }

                if (String.IsNullOrEmpty(strFromStyle) == true)
                {
                    strError = "GetFromStyle()没有找到 '" + this.comboBox_from.Text + "' 对应的style字符串";
                    goto ERROR1;
                }

                string strMatchStyle = "left";  // BiblioSearchForm.GetCurrentMatchStyle(this.comboBox_matchStyle.Text);
                if (this.textBox_queryWord.Text == "")
                {
                    if (strMatchStyle == "null")
                    {
                        this.textBox_queryWord.Text = "";

                        // 专门检索空值
                        strMatchStyle = "exact";
                    }
                    else
                    {
                        // 为了在检索词为空的时候，检索出全部的记录
                        strMatchStyle = "left";
                    }
                }
                else
                {
                    if (strMatchStyle == "null")
                    {
                        strError = "检索空值的时候，请保持检索词为空";
                        goto ERROR1;
                    }
                }

                string strQueryWord = GetBiblioQueryString();

                string strQueryXml = "";
                long lRet = channel.SearchBiblio(Progress,
                    this.GetBiblioDbNames(),    // "<全部>",
                    strQueryWord,   // this.textBox_queryWord.Text,
                    1000,
                    strFromStyle,
                    strMatchStyle,
                    this.Lang,
                    null,   // strResultSetName
                    "",    // strSearchStyle
                    "", // strOutputStyle
                    out strQueryXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                // TODO: 最多检索1000条的限制，可以作为参数配置？

                long lHitCount = lRet;

                if (lHitCount == 0)
                {
                    strError = "从途径 '" + strFromStyle + "' 检索 '" + strQueryWord + "' 没有命中";
                    this.SetFloatMessage("", strError);
                    this.textBox_queryWord.SelectAll();
                    this.textBox_queryWord.Focus();
                    return;
                }

                long lStart = 0;
                long lPerCount = Math.Min(50, lHitCount);
                DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                // 装入浏览格式
                for (; ; )
                {
                    Application.DoEvents();	// 出让界面控制权

                    if (Progress != null)
                    {
                        if (Progress.State != 0)
                        {
                            // MessageBox.Show(this, "用户中断");
                            break;  // 已经装入的还在
                        }
                    }

                    Progress.SetMessage("正在装入书目记录ID " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                    lRet = channel.GetSearchResult(
                        Progress,
                        null,   // strResultSetName
                        lStart,
                        lPerCount,
                        "id", // "id,cols",
                        this.Lang,
                        out searchresults,
                        out strError);
                    if (lRet == -1)
                    {
                        if (Progress.State != 0)
                        {
                            // MessageBox.Show(this, "用户中断");
                            break;
                        }

                        goto ERROR1;
                    }

                    if (lRet == 0)
                    {
                        strError = "未命中";
                        goto ERROR1;
                    }

                    // 处理浏览结果
                    foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                    {
                        this._biblioRecPaths.Add(record.Path);
                    }

                    lStart += searchresults.Length;
                    // lCount -= searchresults.Length;
                    if (lStart >= lHitCount || lPerCount <= 0)
                        break;
                }

                this.SetFloatMessage("waiting", "正在装入册记录 ...");
                Progress.SetProgressRange(0, this._biblioRecPaths.Count);
                // 将每条书目记录下属的册记录装入
                int i = 0;
                foreach (string strBiblioRecPath in this._biblioRecPaths)
                {
                    Application.DoEvents();

                    if (Progress.State != 0)
                        break;

                    nRet = LoadBiblioSubItems(
                        channel,
                        strBiblioRecPath,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    Progress.SetProgressValue(++i);
                }
                // MessageBox.Show(this, Convert.ToString(lRet) + " : " + strError);
            }
            finally
            {
                Progress.EndLoop();
                Progress.OnStop -= new StopEventHandler(this.DoStop);
                Progress.Initial("");
                Progress.HideProgress();

                // this.button_search.Enabled = true;
                this.EnableControls(true);

                this.ReturnChannel(channel);

                m_nInSearching--;
            }

            int nClickableCount = GetClickableCount();

            if (nClickableCount == 0)
                this.SetFloatMessage("", "没有可用的册记录");
            else
            {
                this.SetFloatMessage("", "");
                SelectFirstUseableItem();
                if (nClickableCount == 1
                    && this.AutoOperSingleItem == true)
                {
                    this.dpTable_items_DoubleClick(this, new EventArgs());
                }
            }
            this.textBox_queryWord.SelectAll();
            return;

        ERROR1:
            this.SetFloatMessage("", strError);
            if (this.Visible == true)
                MessageBox.Show(this, strError);
            this.textBox_queryWord.SelectAll();
            this.textBox_queryWord.Focus();
        }

        // 获得当前可选项目的个数
        int GetClickableCount()
        {
            int nCount = 0;
            foreach (DpRow row in this.dpTable_items.Rows)
            {
                if (row.IsSeperator)
                    continue;
                if (IsGray(row) == false)
                    nCount++;
            }

            return nCount;
        }

        // 选定第一个可用的事项，并滚入可见范围
        void SelectFirstUseableItem()
        {
            this.dpTable_items.ClearAllSelections();
            foreach (DpRow row in this.dpTable_items.Rows)
            {
                if (row.IsSeperator)
                    continue;
                if (IsGray(row) == false)
                {
                    row.Selected = true;
                    row.EnsureVisible();
                    return;
                }
            }
        }

        static void SetGrayText(DpRow row)
        {
            RowTag tag = new RowTag();
            tag.GrayText = true;
            row.Tag = tag;
            row.ForeColor = SystemColors.GrayText;
        }

        // 将一条书目记录下属的若干册记录装入列表
        // return:
        //      -2  用户中断
        //      -1  出错
        //      >=0 装入的册记录条数
        int LoadBiblioSubItems(
            LibraryChannel channel,
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";

            Progress.SetMessage("正在装入书目记录 '" + strBiblioRecPath + "' 下属的册记录 ...");

            int nCount = 0;

            long lPerCount = 100; // 每批获得多少个
            long lStart = 0;
            long lResultCount = 0;
            long lCount = -1;
            for (; ; )
            {
                if (Progress.State != 0)
                {
                    strError = "用户中断";
                    return -2;
                }

                EntityInfo[] entities = null;

                long lRet = channel.GetEntities(
         Progress,
         strBiblioRecPath,
         lStart,
         lCount,
         "",  // bDisplayOtherLibraryItem == true ? "getotherlibraryitem" : "",
         "zh",
         out entities,
         out strError);
                if (lRet == -1)
                    return -1;

                lResultCount = lRet;

                if (lRet == 0)
                    return nCount;

                Debug.Assert(entities != null, "");

                foreach (EntityInfo entity in entities)
                {
                    string strXml = entity.OldRecord;

                    XmlDocument dom = new XmlDocument();
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(strXml) == false)
                                dom.LoadXml(strXml);
                            else
                                dom.LoadXml("<root />");
                        }
                        catch (Exception ex)
                        {
                            strError = "XML 装入 DOM 出错: " + ex.Message;
                            return -1;
                        }
                    }

                    DpRow row = new DpRow();

                    string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
                    string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
                    if (this.FunctionType == "borrow")
                    {
                        // 在借的册、或者状态有值的需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == false
                            || string.IsNullOrEmpty(strState) == false)
                            SetGrayText(row);
                    }
                    else if (this.FunctionType == "return")
                    {
                        // 没有在借的册需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == true)
                            SetGrayText(row);
                        if (string.IsNullOrEmpty(this.VerifyBorrower) == false)
                        {
                            // 验证还书时，不是要求的读者所借阅的册，显示为灰色
                            if (strBorrower != this.VerifyBorrower)
                                SetGrayText(row);
                        }
                    }
                    else if (this.FunctionType == "renew")
                    {
                        // 没有在借的册需要显示为灰色
                        if (string.IsNullOrEmpty(strBorrower) == true)
                            SetGrayText(row);
                    }

                    string strBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
                    string strRefID = DomUtil.GetElementText(dom.DocumentElement, "refID");

                    // 状态
                    DpCell cell = new DpCell();
                    cell.Text = strState;
                    row.Add(cell);

                    // 册条码号
                    cell = new DpCell();
                    if (string.IsNullOrEmpty(strBarcode) == false)
                        cell.Text = strBarcode;
                    else
                        cell.Text = "@refID:" + strRefID;
                    row.Add(cell);

                    // 在借情况
                    cell = new DpCell();
                    if (string.IsNullOrEmpty(strBorrower) == false)
                    {
                        string strReaderSummary = this.MainForm.GetReaderSummary(strBorrower, false);
                        bool bError = (string.IsNullOrEmpty(strReaderSummary) == false && strReaderSummary[0] == '!');

                        if (bError == true)
                            cell.BackColor = Color.FromArgb(180, 0, 0);
                        else
                        {
                            if (IsGray(row) == true)
                                cell.BackColor = Color.FromArgb(220, 220, 0);
                            else
                                cell.BackColor = Color.FromArgb(180, 180, 0);
                        }

                        if (bError == false)
                            cell.Font = new System.Drawing.Font(this.dpTable_items.Font.FontFamily.Name, this.dpTable_items.Font.Size * 2, FontStyle.Bold);

                        cell.ForeColor = Color.FromArgb(255, 255, 255);
                        cell.Alignment = DpTextAlignment.Center;
                        cell.Text = strReaderSummary;
                        // TODO: 后面还可加上借阅时间，应还时间
                    }
                    row.Add(cell);

                    // 书目摘要
                    string strSummary = "";
                    if (entity.ErrorCode != ErrorCodeValue.NoError)
                    {
                        strSummary = entity.ErrorInfo;
                    }
                    else
                    {
                        int nRet = this.MainForm.GetBiblioSummary("@bibliorecpath:" + strBiblioRecPath,
                            "",
                            false,
                            out strSummary,
                            out strError);
                        if (nRet == -1)
                            strSummary = strError;
                    }
                    cell = new DpCell();
                    cell.Text = strSummary;
                    row.Add(cell);

                    // 卷册
                    string strVolumn = DomUtil.GetElementText(dom.DocumentElement, "volumn");
                    cell = new DpCell();
                    cell.Text = strVolumn;
                    row.Add(cell);

                    // 地点
                    string strLocation = DomUtil.GetElementText(dom.DocumentElement, "location");
                    cell = new DpCell();
                    cell.Text = strLocation;
                    row.Add(cell);

                    // 价格
                    string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
                    cell = new DpCell();
                    cell.Text = strPrice;
                    row.Add(cell);

                    // 册记录路径
                    cell = new DpCell();
                    cell.Text = entity.OldRecPath;
                    row.Add(cell);

                    this.dpTable_items.Rows.Add(row);
                    nCount++;
                }

                lStart += entities.Length;
                if (lStart >= lResultCount)
                    break;

                if (lCount == -1)
                    lCount = lPerCount;

                if (lStart + lCount > lResultCount)
                    lCount = lResultCount - lStart;
            }

            if (lStart > 0)
            {
                DpRow row = new DpRow();
                row.Style = DpRowStyle.Seperator;
                this.dpTable_items.Rows.Add(row);
            }

            return nCount;
        }

        const int COLUMN_STATE = 0;
        const int COLUMN_ITEMBARCODE = 1;
        const int COLUMN_BORROWINFO = 2;
        const int COLUMN_SUMMARY = 3;
        const int COLUMN_VOLUMN = 4;
        const int COLUMN_LOCATION = 5;
        const int COLUMN_PRICE = 6;
        const int COLUMN_ITEMRECPATH = 7;

        string GetBiblioQueryString()
        {
            string strText = this.textBox_queryWord.Text;
            int nRet = strText.IndexOf(';');
            if (nRet != -1)
            {
                strText = strText.Substring(0, nRet).Trim();
                this.textBox_queryWord.Text = strText;
            }

            /*
            if (this.checkBox_autoDetectQueryBarcode.Checked == true)
            {
                if (strText.Length == 13)
                {
                    string strHead = strText.Substring(0, 3);
                    if (strHead == "978")
                    {
                        this.textBox_queryWord.Text = strText + " ;自动用" + strText.Substring(3, 9) + "来检索";
                        return strText.Substring(3, 9);
                    }
                }
            }*/

            return strText;
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this.comboBox_from.Enabled = bEnable;
            this.textBox_queryWord.Enabled = bEnable;
            this.button_search.Enabled = bEnable;
        }

        private void SelectItemDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Progress != null)
                this.Progress.DoStop();
        }

        private void dpTable_items_DoubleClick(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_items.SelectedRows.Count != 1)
            {
                strError = "请选择一行";
                goto ERROR1;
            }

            // 检查是否为灰色文字
            if (Control.ModifierKeys == Keys.Control)
            {
                // 按下 Control 键盘的时候灰色事项也可以操作
            }
            else
            {
                DpRow row = this.dpTable_items.SelectedRows[0];
                if (IsGray(row) == true)
                {
                    strError = "当前功能不允许选择此行。";
                    if (this.FunctionType == "borrow")
                        strError += "只能对尚未借出的、状态为空的册进行借书操作";

                    if (this.FunctionType == "return"
                        && string.IsNullOrEmpty(this.VerifyBorrower) == false)
                        strError += "(验证还书时)只能对在借状态的、并且是特定读者借了的册进行还书操作";
                    else if (this.FunctionType == "return")
                        strError += "只能对在借状态的册进行还书操作";

                    if (this.FunctionType == "renew")
                        strError += "只能对在借状态的册进行续借操作";
                    goto ERROR1;
                }
            }

            // 如果正在检索
            if (this.m_nInSearching > 0)
                Progress.DoStop();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
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
            if (keyData == Keys.Enter)
            {
                if (this.textBox_queryWord.Focused == true)
                    button_search_Click(this, new EventArgs());
                else
                    dpTable_items_DoubleClick(this, new EventArgs());
                return true;
            }

            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        public string SelectedItemBarcode
        {
            get
            {
                if (this.dpTable_items.SelectedRows.Count == 0)
                    return "";

                DpRow row = this.dpTable_items.SelectedRows[0];
                return row[COLUMN_ITEMBARCODE].Text;
            }
        }

        public string QueryWord
        {
            get
            {
                return this.textBox_queryWord.Text;
            }
            set
            {
                this.textBox_queryWord.Text = value;
            }
        }

        public string From
        {
            get
            {
                return this.textBox_queryWord.Text;
            }
            set
            {
                this.textBox_queryWord.Text = value;
            }
        }

        class RowTag
        {
            public bool GrayText = false;
        }

        static bool IsGray(DpRow row)
        {
            if (row.Tag == null)
                return false;
            RowTag tag = row.Tag as RowTag;
            return tag.GrayText;
        }

        private void dpTable_items_PaintBack(object sender, PaintBackArgs e)
        {
            if (!(e.Item is DpRow))
            {
                if (e.Item is DpCell)
                {
                    DpCell cell = e.Item as DpCell;

                    if (cell.BackColor != Color.Transparent)
                    {
                        using (Brush brush = new SolidBrush(cell.BackColor))
                        {
                            e.pe.Graphics.FillRectangle(brush, e.Rect);
                        }
                    }
                }
                return;
            }

            DpRow row = e.Item as DpRow;

            bool bGray = IsGray(row);

            if (row.Selected == true)
            {
                if (row.Control.Focused == true)
                {
                    using (Brush brush = new SolidBrush(
                        bGray ? Color.FromArgb(240, 240, 240) : row.Control.HighlightBackColor
                        ))
                    {
                        e.pe.Graphics.FillRectangle(brush, e.Rect);
                    }
                }
                else
                {
                    // textColor = SystemColors.InactiveCaptionText;
                    using (Brush brush = new SolidBrush(
                        bGray ? Color.FromArgb(240, 240, 240) : row.Control.InactiveHighlightBackColor
                        ))
                    {
                        e.pe.Graphics.FillRectangle(brush, e.Rect);
                    }
                }
            }
            else
            {
                if (row.BackColor != Color.Transparent)
                {
                    using (Brush brush = new SolidBrush(
                        row.Control.BackColor
                        ))
                    {
                        e.pe.Graphics.FillRectangle(brush, e.Rect);
                    }
                }
            }
        }

        /// <summary>
        /// UI 状态
        /// </summary>
        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.dpTable_items);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.dpTable_items);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            dpTable_items_DoubleClick(sender, e);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void dpTable_items_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dpTable_items.SelectedRows.Count != 1)
            {
                this.button_OK.Enabled = false;
            }
            else
            {
                // 检查是否为灰色文字
                DpRow row = this.dpTable_items.SelectedRows[0];
                if (IsGray(row) == true)
                    this.button_OK.Enabled = false;
                else
                    this.button_OK.Enabled = true;
            }

        }

        private void dpTable_items_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ProcessDialogKey(Keys.Enter);
                e.Handled = true;
            }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
                stopManager.DoStopAll(null);
            else
                stopManager.DoStopActive();

        }

        private void dpTable_items_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            //ToolStripMenuItem subMenuItem = null;
            ToolStripSeparator menuSepItem = null;

            DpRow selected_row = null;
            if (this.dpTable_items.SelectedRows.Count > 0)
            {
                selected_row = this.dpTable_items.SelectedRows[0];
            }

            // 
            menuItem = new ToolStripMenuItem("打开到 册窗(&I)");
            if (this.dpTable_items.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToItemInfoForm_Click);
            contextMenu.Items.Add(menuItem);

            // 
            menuItem = new ToolStripMenuItem("打开到 种册窗(&E)");
            if (this.dpTable_items.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_loadToEntityForm_Click);
            contextMenu.Items.Add(menuItem);

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 
            menuItem = new ToolStripMenuItem("复制 [" + this.dpTable_items.SelectedRows.Count.ToString() + "] (&D)");
            if (this.dpTable_items.SelectedRows.Count > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_copy_Click);
            contextMenu.Items.Add(menuItem);

            contextMenu.Show(this.dpTable_items, e.Location);
        }

        void menuItem_copy_Click(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            StringBuilder strTotal = new StringBuilder(4096);

            foreach (DpRow row in this.dpTable_items.SelectedRows)
            {
                strTotal.Append(GetRowText(row) + "\r\n");
            }

            Clipboard.SetDataObject(strTotal.ToString(), true);

            this.Cursor = oldCursor;
        }

        // 获得一个 DpRow 行的用于 Copy 的文本
        static string GetRowText(DpRow row)
        {
            StringBuilder text = new StringBuilder(4096);
            int i = 0;
            foreach (DpCell cell in row)
            {
                // 跳过第一列
                if (i > 0)
                {
                    if (text.Length > 0)
                        text.Append("\t");
                    text.Append(cell.Text);
                }

                i++;
            }

            return text.ToString();
        }
        // 打开到 册窗
        void menuItem_loadToItemInfoForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_items.SelectedRows.Count == 0)
            {
                strError = "尚未选定要打开的册事项";
                goto ERROR1;
            }

            DpRow selected_row = this.dpTable_items.SelectedRows[0];
            string strItemBarcode = selected_row[COLUMN_ITEMBARCODE].Text;

            if (string.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "所选定的册事项不具备册条码号信息";
                goto ERROR1;
            }
            ItemInfoForm form = this.MainForm.EnsureItemInfoForm();
            Global.Activate(form);

            form.LoadRecord(strItemBarcode);

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打开到 种册窗
        void menuItem_loadToEntityForm_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.dpTable_items.SelectedRows.Count == 0)
            {
                strError = "尚未选定要打开的册事项";
                goto ERROR1;
            }

            DpRow selected_row = this.dpTable_items.SelectedRows[0];
            string strItemBarcode = selected_row[COLUMN_ITEMBARCODE].Text;

            if (string.IsNullOrEmpty(strItemBarcode) == true)
            {
                strError = "所选定的册事项不具备册条码号信息";
                goto ERROR1;
            }

            EntityForm form = this.MainForm.EnsureEntityForm();
            Global.Activate(form);

            form.LoadItemByBarcode(strItemBarcode, false);

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

    }

#if NO
    // TODO: 最好用 TransparentToolStripRenderer 代替
    class MyRenderer : ToolStripSystemRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (Brush brush = new SolidBrush(SystemColors.ControlDark))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
            // base.OnRenderToolStripBackground(e);
        }

        // 去掉下面那根线
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // base.OnRenderToolStripBorder(e);
        }
    }
#endif
}
