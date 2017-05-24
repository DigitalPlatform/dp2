using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

using DigitalPlatform.CirculationClient;
// using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 册条码号发生重复时的观察选择对话框
    /// </summary>
    internal partial class ItemBarcodeDupDlg : Form
    {
        WebExternalHost m_webExternalHost_biblio = new WebExternalHost();

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        /// <summary>
        /// 通讯通道
        /// </summary>
        LibraryChannel Channel = null;

        Stop stop = null;
        string[] aPaths = null;

        public string SelectedRecPath = "";

        const int WM_LOAD_ALL_DATA = API.WM_USER + 200;

        public ItemBarcodeDupDlg()
        {
            InitializeComponent();
        }

        // 初始化数据成员
        public int Initial(
            // MainForm mainform,
            string[] aPaths,
            string strMessage,
            LibraryChannel channel,
            Stop stop,
            out string strError)
        {
            strError = "";

            // this.MainForm = mainform;
            this.Channel = channel;
            this.stop = stop;
            this.aPaths = aPaths;

            this.textBox_message.Text = strMessage;

            this.InfoColor = InfoColor.LightRed; // 红色表示警告

            return 0;
        }

        private void ItemBarcodeDupDlg_Load(object sender, EventArgs e)
        {
            this.entityEditControl1.SetReadOnly("all");
            FillRecPath();

            EnableControls(false);

            this.m_webExternalHost_biblio.Initial(// Program.MainForm, 
                this.webBrowser_biblio);
            this.webBrowser_biblio.ObjectForScripting = this.m_webExternalHost_biblio;

            API.PostMessage(this.Handle, WM_LOAD_ALL_DATA, 0, 0);
        }

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
                        return;
                    }
                    // break;

            }
            base.DefWndProc(ref m);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.listView_items.Enabled = bEnable;
            this.entityEditControl1.Enabled = bEnable;
            this.tableLayoutPanel_biblio.Enabled = bEnable;

            if (this.listView_items.SelectedItems.Count == 0)
                this.button_OK.Enabled = false;
            else
                this.button_OK.Enabled = bEnable;

            this.button_Cancel.Enabled = bEnable;

        }

        private void ItemBarcodeDupDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.m_webExternalHost_biblio != null)
                this.m_webExternalHost_biblio.Destroy();

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_items.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未在列表中选定一个册");
                return;
            }

            this.SelectedRecPath = this.listView_items.SelectedItems[0].Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 在listview中填充路径列。不填充其他列
        void FillRecPath()
        {
            this.listView_items.Items.Clear();

            for (int i = 0; i < this.aPaths.Length; i++)
            {

                ListViewItem item = new ListViewItem(this.aPaths[i], 0);
                item.SubItems.Add("点击可装入数据...");

                this.listView_items.Items.Add(item);

                // item.BackColor = SystemColors.Window;
            }
        }

        class ItemInfo
        {
            public string ItemXml = "";
            public string BiblioHtml = "";
            public string RecPath = "";
        }

        /*
        // 填充一行的信息
        int FillLine(
            ListViewItem item,
            string strRecPath,
            string strItemXml,
            string strBiblioHtml,
            out string strError)
        {
            strError = "";

            Debug.Assert(item.Tag == null, "");

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
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
            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strBookType = DomUtil.GetElementText(dom.DocumentElement,
                "bookType");
            string strRegisterNo = DomUtil.GetElementText(dom.DocumentElement,
                "registerNo");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strMergeComment = DomUtil.GetElementText(dom.DocumentElement,
                "mergeComment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement,
                "borrower");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");

            ListViewUtil.ChangeItemText(item, 1, strBarcode);
            // item.SubItems.Add(strBarcode);
            item.SubItems.Add(strState);
            item.SubItems.Add(strLocation);
            item.SubItems.Add(strPrice);
            item.SubItems.Add(strBookType);
            item.SubItems.Add(strRegisterNo);
            item.SubItems.Add(strComment);
            item.SubItems.Add(strMergeComment);
            item.SubItems.Add(strBatchNo);
            item.SubItems.Add(strBorrower);
            item.SubItems.Add(strBorrowDate);
            item.SubItems.Add(strBorrowPeriod);

            ItemInfo info = new ItemInfo();

            info.BiblioHtml = strBiblioHtml;
            info.ItemXml = strItemXml;
            info.RecPath = strRecPath;

            item.Tag = info;    // 储存起来
            item.BackColor = SystemColors.Window;

            return 0;
        }
         * */

        // 填充一行的信息
        int FillLine(
            ListViewItem item,
            out string strError)
        {
            strError = "";

            Debug.Assert(item.Tag != null, "");

            ItemInfo info = (ItemInfo)item.Tag;
            string strRecPath = info.RecPath;
            string strItemXml = info.ItemXml;
            string strBiblioHtml = info.BiblioHtml;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
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
            string strLocation = DomUtil.GetElementText(dom.DocumentElement,
                "location");
            string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            string strBookType = DomUtil.GetElementText(dom.DocumentElement,
                "bookType");
            string strRegisterNo = DomUtil.GetElementText(dom.DocumentElement,
                "registerNo");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strMergeComment = DomUtil.GetElementText(dom.DocumentElement,
                "mergeComment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            string strBorrower = DomUtil.GetElementText(dom.DocumentElement,
                "borrower");
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement,
                "borrowDate");
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement,
                "borrowPeriod");

            ListViewUtil.ChangeItemText(item, 1, strBarcode);
            // item.SubItems.Add(strBarcode);
            ListViewUtil.ChangeItemText(item, 2, strState);
            ListViewUtil.ChangeItemText(item, 3, strLocation);
            ListViewUtil.ChangeItemText(item, 4, strPrice);
            ListViewUtil.ChangeItemText(item, 5, strBookType);
            ListViewUtil.ChangeItemText(item, 6, strRegisterNo);
            ListViewUtil.ChangeItemText(item, 7, strComment);
            ListViewUtil.ChangeItemText(item, 8, strMergeComment);
            ListViewUtil.ChangeItemText(item, 9, strBatchNo);
            ListViewUtil.ChangeItemText(item, 10, strBorrower);
            ListViewUtil.ChangeItemText(item, 11, strBorrowDate);
            ListViewUtil.ChangeItemText(item, 12, strBorrowPeriod);

            item.BackColor = SystemColors.Window;

            return 0;
        }

        // 在下面左右两个窗格中，显示册和书目信息
        int DisplayItemAndBiblioInfo(ListViewItem item,
            out string strError)
        {
            strError = "";

            ItemInfo info = (ItemInfo)item.Tag;

            Debug.Assert(info != null, "");
            if (info == null)
            {
                strError = "listviewitem的Tag尚未初始化";
                return -1;
            }

            int nRet = this.entityEditControl1.SetData(info.ItemXml,
                info.RecPath,
                null,
                out strError);

            this.m_webExternalHost_biblio.StopPrevious();
            this.webBrowser_biblio.Stop();

#if NO
            Global.SetHtmlString(this.webBrowser_biblio,
                info.BiblioHtml,
                Program.MainForm.DataDir,
                "itembarcodedup_biblio");
#endif
            this.m_webExternalHost_biblio.SetHtmlString(info.BiblioHtml,
                "itembarcodedup_biblio");

            // 延迟报错
            if (nRet == -1)
                return -1;

            return 0;
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this.Channel != null)
                this.Channel.Abort();
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

            return 0;
        }

        // 为一行装载数据
        int LoadItemData(ListViewItem item,
            out string strError)
        {
            Debug.Assert(item.Tag == null, "");

            strError = "";

            string strRecPath = item.Text;

            string strItemXml = "";
            string strBiblioHtml = "";

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取路径为 '" + strRecPath + "' 的册信息和书目信息 ...");
            stop.BeginLoop();

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;

            try
            {

                long lRet = Channel.GetItemInfo(
                    stop,
                    "@path:" + strRecPath,
                    "xml",
                    out strItemXml,
                    "html",
                    out strBiblioHtml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;  // error

                if (lRet == 0)
                {
                    strError = "路径为 '" + strRecPath + "' 的册记录没有找到";
                    goto ERROR1;   // not found
                }
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                this.Cursor = oldCursor;
            }

            ItemInfo info = new ItemInfo();

            info.BiblioHtml = strBiblioHtml;
            info.ItemXml = strItemXml;
            info.RecPath = strRecPath;

            item.Tag = info;    // 储存起来

            return 0;
        ERROR1:
            return -1;
        }

        // listview事项选择改变，装载行内容
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
                nRet = DisplayItemAndBiblioInfo(item,
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

            // 在左下、右下显示出来
            nRet = DisplayItemAndBiblioInfo(item,
    out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        private void listView_items_DoubleClick(object sender, EventArgs e)
        {
            this.button_OK_Click(sender, e);
        }

        public InfoColor InfoColor
        {
            get
            {
                if (this.label_colorBar.BackColor == Color.Red)
                    return InfoColor.Red;
                if (this.label_colorBar.BackColor == Color.LightCoral)
                    return InfoColor.LightRed;
                if (this.label_colorBar.BackColor == Color.Yellow)
                    return InfoColor.Yellow;
                if (this.label_colorBar.BackColor == Color.Green)
                    return InfoColor.Green;

                return InfoColor.Green;
            }
            set
            {
                if (value == InfoColor.Red)
                    this.label_colorBar.BackColor = Color.Red;
                else if (value == InfoColor.LightRed)
                    this.label_colorBar.BackColor = Color.LightCoral;
                else if (value == InfoColor.Yellow)
                    this.label_colorBar.BackColor = Color.Yellow;
                else if (value == InfoColor.Green)
                    this.label_colorBar.BackColor = Color.Green;
                else
                    this.label_colorBar.BackColor = Color.Green;
            }
        }
    }
}