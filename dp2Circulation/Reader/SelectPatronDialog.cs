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
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 当命中多个读者记录时，供选择一个读者用的对话框
    /// </summary>
    internal partial class SelectPatronDialog : MyForm
    {
        public bool NoBorrowHistory = false;

        WebExternalHost m_webExternalHost_patron = new WebExternalHost();

        DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();

        List<string> m_recpaths = new List<string>();

        const int COLUMN_BARCODE = 0;
        const int COLUMN_STATE = 1;
        const int COLUMN_NAME = 2;
        const int COLUMN_GENDER = 3;
        const int COLUMN_DEPARTMENT = 4;
        const int COLUMN_IDCARDNUMBER = 5;
        const int COLUMN_COMMENT = 6;
        const int COLUMN_RECPATH = 7;

        public string SelectedRecPath = "";

        public string SelectedBarcode = "";

        public string SelectedHtml = "";

        public bool Overflow = false;

        public SelectPatronDialog()
        {
            this.UseLooping = true; // 2022/11/3

            InitializeComponent();
        }

        public List<string> RecPaths
        {
            get
            {
                return this.m_recpaths;
            }
            set
            {
                this.m_recpaths = value;
            }
        }

        private void SelectPatronDialog_Load(object sender, EventArgs e)
        {
            stopManager.Initial(
                this,
                this.toolStripButton_stop,
                (object)this.toolStripLabel_message,
                (object)null);
            // 本窗口独立管理 stopManager
            this._loopingHost.StopManager = stopManager;
            // 2023/3/16
            this._loopingHost.GroupName = "";

            /*
            _stop = new DigitalPlatform.Stop();
            _stop.Register(this._stopManager, true);	// 和容器关联
            */
            FillRecPath();

            EnableControls(false);

            this.m_webExternalHost_patron.Initial(//Program.MainForm, 
                this.webBrowser_patron);
            this.webBrowser_patron.ObjectForScripting = this.m_webExternalHost_patron;

            MessageVisible = MessageVisible;

            // API.PostMessage(this.Handle, WM_LOAD_ALL_DATA, 0, 0);
            this.BeginInvoke(new Action(Initial));
        }

        private void SelectPatronDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
#if NO
            if (this._processing > 0)
            {
                if (this._stopManager != null)
                    this._stopManager.DoStopAll(null);
                this.Stopped = true;
                e.Cancel = true;
            }
#endif
        }

        private void SelectPatronDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.m_webExternalHost_patron != null)
                this.m_webExternalHost_patron.Destroy();

            /*
            if (_stop != null) // 脱离关联
            {
                _stop.Unregister();	// 和容器关联
                _stop = null;
            }
            */
        }

        // 在 listview 中填充路径列。不填充其他列
        void FillRecPath()
        {
            this.listView_items.Items.Clear();

            for (int i = 0; i < this.RecPaths.Count; i++)
            {
                ListViewItem item = new ListViewItem("", 0);
                // item.SubItems.Add("点击可装入数据...");

                ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, this.RecPaths[i]);

                this.listView_items.Items.Add(item);

                // item.BackColor = SystemColors.Window;
            }
        }

        // 初始化数据成员
        public int Initial(
            // MainForm mainform,
            List<string> paths,
            string strMessage,
            out string strError)
        {
            strError = "";

            // this.MainForm = mainform;
            this.RecPaths = paths;

            this.textBox_message.Text = strMessage;
            return 0;
        }

        void Initial()
        {
            string strError = "";
            int nRet = LoadAllItemData(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            // 然后许可界面
            EnableControls(true);

            // 默认选中第一行
            if (this.listView_items.Items.Count > 0
                && this.listView_items.SelectedItems.Count == 0)
                this.listView_items.Items[0].Selected = true;

            // 去掉全选
            this.textBox_message.Select(0, 0);
            // 更换焦点
            this.listView_items.Focus();

            if (this.Overflow == true)
            {
                MessageBox.Show(this, "当前窗口内未能显示命中的全部相关读者记录，请使用读者查询窗甄别和选定读者记录");
            }
            return;
        }
#if NO
        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LOAD_ALL_DATA:
                    {
                        string strError = "";
                        int nRet = LoadAllItemData(out strError);
                        if (nRet == -1)
                            MessageBox.Show(this, strError);

                        // 然后许可界面
                        EnableControls(true);

                        // 默认选中第一行
                        if (this.listView_items.Items.Count > 0
                            && this.listView_items.SelectedItems.Count == 0)
                            this.listView_items.Items[0].Selected = true;

                        // 去掉全选
                        this.textBox_message.Select(0, 0);
                        // 更换焦点
                        this.listView_items.Focus();

                        if (this.Overflow == true)
                        {
                            MessageBox.Show(this, "当前窗口内未能显示命中的全部相关读者记录，请使用读者查询窗甄别和选定读者记录");
                        }
                        return;
                    }
                // break;

            }
            base.DefWndProc(ref m);
        }
#endif

        public override void UpdateEnable(bool bEnable)
        {
            this.listView_items.Enabled = bEnable;

            if (this.listView_items.SelectedItems.Count == 0)
                this.button_OK.Enabled = false;
            else
                this.button_OK.Enabled = bEnable;

            this.button_Cancel.Enabled = bEnable;
        }

        class ItemInfo
        {
            public string PatronXml = "";
            public string PatronHtml = "";
            public string RecPath = "";
        }
        // 填充一行的信息
        int FillLine(
            ListViewItem item,
            out string strError)
        {
            strError = "";

            Debug.Assert(item.Tag != null, "");

            ItemInfo info = (ItemInfo)item.Tag;
            string strRecPath = info.RecPath;
            string strPatronXml = info.PatronXml;
            string strBiblioHtml = info.PatronHtml;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strPatronXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM失败: " + ex.Message;
                return -1;
            }

            string strBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");
            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            string strGender = DomUtil.GetElementText(dom.DocumentElement,
                "gender");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            string strIdCardNumber = DomUtil.GetElementText(dom.DocumentElement,
                "idCardNumber");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            ListViewUtil.ChangeItemText(item, COLUMN_BARCODE, strBarcode);
            // item.SubItems.Add(strBarcode);
            ListViewUtil.ChangeItemText(item, COLUMN_STATE, strState);
            ListViewUtil.ChangeItemText(item, COLUMN_NAME, strName);
            ListViewUtil.ChangeItemText(item, COLUMN_GENDER, strGender);
            ListViewUtil.ChangeItemText(item, COLUMN_DEPARTMENT, strDepartment);
            ListViewUtil.ChangeItemText(item, COLUMN_IDCARDNUMBER, strIdCardNumber);
            ListViewUtil.ChangeItemText(item, COLUMN_COMMENT, strComment);

            ListViewUtil.ChangeItemText(item, COLUMN_RECPATH, strRecPath);

            // item.BackColor = SystemColors.Window;

            return 0;
        }

        // 在下面 浏览器控件 中显示读者信息
        int DisplayPatronInfo(ListViewItem item,
            out string strError)
        {
            strError = "";

            ItemInfo info = (ItemInfo)item.Tag;

            Debug.Assert(info != null, "");
            if (info == null)
            {
                strError = "listviewitem 的 Tag 尚未初始化";
                return -1;
            }

            this.m_webExternalHost_patron.StopPrevious();
            this.webBrowser_patron.Stop();

#if NO
            Global.SetHtmlString(this.webBrowser_patron,
                info.PatronHtml,
                Program.MainForm.DataDir,
                "selectpatrondialog");
#endif
            this.m_webExternalHost_patron.SetHtmlString(info.PatronHtml,
                "selectpatrondialog");

            this.SelectedHtml = info.PatronHtml;
            return 0;
        }

        // 装载全部行的数据
        int LoadAllItemData(out string strError)
        {
            strError = "";

            for (int i = 0; i < this.listView_items.Items.Count; i++)
            {
                ListViewItem item = this.listView_items.Items[i];
                if (item.Tag != null)
                    continue;

                int nRet = LoadItemData(item,
                    out strError);
                if (nRet == -1)
                    return -1;

                nRet = FillLine(
                    item,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            // 检查是否有重复的证条码号
            List<string> barcodes = new List<string>();
            foreach (ListViewItem item in this.listView_items.Items)
            {
                string strBarcde = ListViewUtil.GetItemText(item, COLUMN_BARCODE);
                if (string.IsNullOrEmpty(strBarcde) == true)
                    continue;
                barcodes.Add(strBarcde);
            }

            if (barcodes.Count > 0)
            {
                int nCount = barcodes.Count;
                StringUtil.RemoveDupNoSort(ref barcodes);
                if (nCount != barcodes.Count)
                {
                    MessageBox.Show(this, "发现有重复的证条码号，这是一个严重错误，请联系管理员");
                }
            }

            return 0;
        }

        // 为一行装载数据
        int LoadItemData(ListViewItem item,
            out string strError)
        {
            Debug.Assert(item.Tag == null, "");

            strError = "";

            // TODO: 要能获取暗黑风格的 HTML
            string strFormatList = "xml,html";
            if (this.NoBorrowHistory == true)
                strFormatList += ":noborrowhistory";

            string strRecPath = ListViewUtil.GetItemText(item, COLUMN_RECPATH);

            string strPatronXml = "";
            string strPatronHtml = "";

            /*
            LibraryChannel channel = this.GetChannel();

            _stop.OnStop += new StopEventHandler(this.DoStop);
            _stop.Initial("正在获取路径为 '" + strRecPath + "' 的读者记录 ...");
            _stop.BeginLoop();
            */
            var looping = Looping(out LibraryChannel channel,
                "正在获取路径为 '" + strRecPath + "' 的读者记录 ...",
                null);

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {
                long lRet = channel.GetReaderInfo(
                    looping.Progress,
                    "@path:" + strRecPath,
                    strFormatList,  // "xml,html",
                    out string[] results,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;  // error

                if (lRet == 0)
                {
                    strError = "路径为 '" + strRecPath + "' 的读者记录没有找到";
                    goto ERROR1;   // not found
                }

                if (results == null || results.Length < 2)
                {
                    strError = "results error";
                    goto ERROR1;
                }

                strPatronXml = results[0];
                strPatronHtml = results[1];
            }
            finally
            {
                this.Cursor = oldCursor;

                looping.Dispose();
                /*
                _stop.EndLoop();
                _stop.OnStop -= new StopEventHandler(this.DoStop);
                _stop.Initial("");

                this.ReturnChannel(channel);
                */
            }

            ItemInfo info = new ItemInfo();

            info.PatronHtml = strPatronHtml;
            info.PatronXml = strPatronXml;
            info.RecPath = strRecPath;

            item.Tag = info;    // 储存起来
            return 0;
        ERROR1:
            return -1;
        }

        private void listView_items_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_items.SelectedItems.Count == 0)
            {
                this.button_OK.Enabled = false;
                this.AcceptButton = null;
                return;
            }

            this.button_OK.Enabled = true;
            this.AcceptButton = this.button_OK;

            ListViewItem item = this.listView_items.SelectedItems[0];

            string strError = "";
            int nRet = 0;

            if (item.Tag != null) // 已经填充了值
            {
                nRet = DisplayPatronInfo(item,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                return;
            }

            nRet = LoadItemData(item,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = FillLine(
                item,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 在下方显示出来
            nRet = DisplayPatronInfo(item,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_items.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定一个读者记录");
                return;
            }

            this.SelectedRecPath = ListViewUtil.GetItemText(listView_items.SelectedItems[0], COLUMN_RECPATH);
            this.SelectedBarcode = ListViewUtil.GetItemText(listView_items.SelectedItems[0], COLUMN_BARCODE);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listView_items_DoubleClick(object sender, EventArgs e)
        {
            this.button_OK_Click(sender, e);
        }

        private void webBrowser_patron_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            // if (Program.MainForm.SuppressScriptErrors == true)
            e.Handled = true;
        }

        public bool ColorBarVisible
        {
            get
            {
                return this.label_colorBar.Visible;
            }
            set
            {
                this.label_colorBar.Visible = value;
            }
        }

        public bool MessageVisible
        {
            get
            {
                return this.textBox_message.Visible;
            }
            set
            {
                this.textBox_message.Visible = value;
                if (value == false)
                    this.splitContainer_rightMain.SplitterDistance = 0;
                else
                    this.splitContainer_rightMain.SplitterDistance = 43;
            }
        }
    }
}
