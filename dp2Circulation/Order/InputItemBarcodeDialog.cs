using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 采购模块的验收时请求用户初次输入册条码号的对话框
    /// </summary>
    internal partial class InputItemBarcodeDialog : Form
    {
        FloatingMessageForm _floatingMessage = null;

        public IApplicationInfo AppInfo = null;

        public bool SeriesMode = false;

        public event VerifyBarcodeHandler VerifyBarcode = null;
        public event DetectBarcodeDupHandler DetectBarcodeDup = null;

        public EntityControl EntityControl = null;  // 相关的EntityControl

        // 文本框中文字相对于内存是否改变
        bool m_bTextChanged = false;

        const int WM_ACTIVATE_BARCODE_INPUT = API.WM_USER + 201;

        public List<InputBookItem> BookItems = null;

        // 保存最初的条码号
        List<string> m_oldBarcodes = null;

        int m_nIndex = -1;  // 当前输入域对应的行号

        bool m_bChanged = false;

        int m_nOriginDisplayColumnWidth = 0;

        const int COLUMN_PRICE = 6;
        const int COLUMN_REF_PRICE = 7;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        public InputItemBarcodeDialog()
        {
            InitializeComponent();
        }

        private void InputItemBarcodeDialog_Load(object sender, EventArgs e)
        {
            if (this.AppInfo != null)
            {
                string strWidths = this.AppInfo.GetString(
                    "input_item_barcode_dialog",
                    "list_column_width",
                    "");
                if (String.IsNullOrEmpty(strWidths) == false)
                {
                    ListViewUtil.SetColumnHeaderWidth(this.listView_barcodes,
                        strWidths,
                        true);
                }
            }

            this.m_nOriginDisplayColumnWidth = this.columnHeader_volumeDisplay.Width;

            if (this.SeriesMode == false)
            {
                this.columnHeader_volumeDisplay.Width = 0;
            }

            FillBookItemList();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.Show(this);
            }
        }

        private void InputItemBarcodeDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.AppInfo != null)
            {
                if (this.columnHeader_volumeDisplay.Width == 0)
                    this.columnHeader_volumeDisplay.Width = this.m_nOriginDisplayColumnWidth;

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_barcodes);
                this.AppInfo.SetString(
                    "input_item_barcode_dialog",
                    "list_column_width",
                    strWidths);
            }
        }

        void SetFloatMessage(string strColor,
string strText)
        {
            if (this.InvokeRequired)
            {
                // 事件是在多线程上下文中触发的，需要 Invoke 显示信息
                this.BeginInvoke(new Action<string, string>(SetFloatMessage), strColor, strText);
                return;
            }

            if (strColor == "waiting")
                this._floatingMessage.RectColor = Color.FromArgb(80, 80, 80);
            else
                this._floatingMessage.RectColor = Color.Purple;

            this._floatingMessage.Text = strText;
        }

        // 将 textbox 的修改兑现到内存中
        int UpdateData(out string strError)
        {
            strError = "";

            if (this.m_nIndex == -1)
                return 0;

            if (this.m_bTextChanged == false)
                return 0;

            int index = this.m_nIndex;

            InputBookItem book_item = this.BookItems[index];

            string strCurrentBarcode = book_item.BookItem.Barcode;

            if (strCurrentBarcode != this.textBox_itemBarcode.Text)
            {
                if (string.IsNullOrEmpty(this.textBox_itemBarcode.Text) == false)
                {
                    // 2015/7/22
                    // 在 listview 内对册条码号进行查重
                    ListViewItem dup = ListViewUtil.FindItem(this.listView_barcodes, this.textBox_itemBarcode.Text, 0);
                    if (dup != null)
                    {
                        strError = "册条码号 '" + this.textBox_itemBarcode.Text + "' 在当前列表中已经存在，不允许重复登入";
                        return -1;
                    }
                }

                // 校验barcode合法性
                if (this.VerifyBarcode != null
                    && this.textBox_itemBarcode.Text != "") // 2009/1/15
                {
                    this.SetFloatMessage("waiting", "正在验证册条码号，请稍候 ...");
                    Application.DoEvents();
                    // Thread.Sleep(5000);
                    try
                    {
                        VerifyBarcodeEventArgs e = new VerifyBarcodeEventArgs();
                        e.Barcode = this.textBox_itemBarcode.Text;
                        if (string.IsNullOrEmpty(Program.MainForm.BarcodeValidation))
                            e.LibraryCode = Global.GetLibraryCode(StringUtil.GetPureLocation(book_item.BookItem.Location)); // 2016/4/18
                        else
                            e.LibraryCode = StringUtil.GetPureLocation(book_item.BookItem.Location); // 2019/7/12

                        this.VerifyBarcode(this, e);
                        // return:
                        //      -2  服务器没有配置校验方法，无法校验
                        //      -1  error
                        //      0   不是合法的条码号
                        //      1   是合法的读者证条码号
                        //      2   是合法的册条码号
                        if (e.Result != -2)
                        {
                            if (e.Result != 2)
                            {
                                if (String.IsNullOrEmpty(strError) == false)
                                    strError = e.ErrorInfo;
                                else
                                {
                                    // 如果从服务器端没有得到出错信息，则补充
                                    //      -1  error
                                    if (e.Result == -1)
                                        strError = "在校验条码号 '" + e.Barcode + "' 时出错";
                                    //      0   不是合法的条码号
                                    else if (e.Result == 0)
                                        strError = $"'{e.Barcode}' (馆藏地属于 '{e.LibraryCode}')不是合法的条码号";
                                    //      1   是合法的读者证条码号
                                    else if (e.Result == 1)
                                        strError = "'" + e.Barcode + "' 是读者证条码号(而不是册条码号)";
                                }
                                return -1;
                            }
                        }

                    }
                    finally
                    {
                        this.SetFloatMessage("", "");
                    }
                }

                book_item.BookItem.Barcode = this.textBox_itemBarcode.Text;
                this.Changed = true;
                ListViewItem item = this.listView_barcodes.Items[index];
                item.Font = new Font(item.Font, FontStyle.Bold);    // 加粗字体表示内容被改变了

                book_item.BookItem.Changed = true;
                book_item.BookItem.RefreshListView();

                // 修改ListViewItem显示
                this.listView_barcodes.Items[index].Text = this.textBox_itemBarcode.Text;

                this.m_bTextChanged = false;
                return 1;
            }

            return 0;
        }
        
        private void button_register_Click(object sender, EventArgs e)
        {
            if (this.listView_barcodes.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选定当前行");
                return;
            }

            this.Enabled = false;
            try
            {
                string strError = "";
                /*
发生未捕获的界面线程异常: 
Type: System.ArgumentOutOfRangeException
Message: InvalidArgument=Value of '0' is not valid for 'index'.
Parameter name: index
Stack:
at System.Windows.Forms.ListView.SelectedIndexCollection.get_Item(Int32 index)
at dp2Circulation.InputItemBarcodeDialog.button_register_Click(Object sender, EventArgs e)
                 * * */
                // 2015/7/22 注：UpdateData() 方法里面可能涉及到 Channel 的调用，会出让界面控制权，这时候操作者如果鼠标点击 ListView，可能会改变 SelectedIndices。
                // 为此，需要暂时禁止整个对话框
                int nRet = UpdateData(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            finally
            {
                this.Enabled = true;

                this.textBox_itemBarcode.SelectAll();
                this.textBox_itemBarcode.Focus();
            }

            Debug.Assert(this.listView_barcodes.SelectedIndices.Count > 0, "");

            // 选定后一行
            int index = this.listView_barcodes.SelectedIndices[0];

            this.listView_barcodes.SelectedItems.Clear();

            // 如果后面没有行了
            if (index >= this.listView_barcodes.Items.Count - 1)
            {
                // this.listView_barcodes.SelectedItems[0].Selected = false;
                return;
            }

            index++;
            this.listView_barcodes.Items[index].Selected = true;
            this.listView_barcodes.EnsureVisible(index);
        }

        void FillBookItemList()
        {
            this.listView_barcodes.Items.Clear();
            this.m_oldBarcodes = new List<string>();

            if (this.BookItems == null)
                return;

            for (int i = 0; i < this.BookItems.Count; i++)
            {
                InputBookItem book_item = this.BookItems[i];

                Debug.Assert(book_item != null, "");

                ListViewItem item = new ListViewItem();
                // 条码
                item.Text = book_item.BookItem.Barcode;
                // 卷期信息
                string strVolumeDisplayString = IssueManageControl.BuildVolumeDisplayString(
                    book_item.BookItem.PublishTime,
                    book_item.BookItem.Volume);
                item.SubItems.Add(strVolumeDisplayString);

                // 套序
                // 2010/12/1
                item.SubItems.Add(book_item.Sequence);

                // 馆藏地点
                item.SubItems.Add(book_item.BookItem.Location);
                // 订购渠道
                item.SubItems.Add(book_item.BookItem.Seller);
                // 经费来源
                item.SubItems.Add(book_item.BookItem.Source);
                // 价格
                item.SubItems.Add(book_item.BookItem.Price);
                // 其他价格
                item.SubItems.Add(book_item.OtherPrices);

                item.Tag = book_item;

                this.listView_barcodes.Items.Add(item);

                this.m_oldBarcodes.Add(book_item.BookItem.Barcode);
            }

            // 选定第一个事项
            if (this.listView_barcodes.Items.Count > 0)
            {
                this.listView_barcodes.Items[0].Selected = true;
            }

            /*
            // 让最后一个事项可见
            if (this.listView_barcodes.Items.Count > 0)
                this.listView_barcodes.EnsureVisible(this.listView_barcodes.Items.Count - 1);
             * */
        }

        void UpdateBookItemsPrice()
        {
            for (int i = 0; i < this.listView_barcodes.Items.Count; i++)
            {
                ListViewItem item = this.listView_barcodes.Items[i];

                InputBookItem book_item = (InputBookItem)item.Tag;

                string strNewPrice = ListViewUtil.GetItemText(item, COLUMN_PRICE);
                if (strNewPrice != book_item.BookItem.Price)
                {
                    book_item.BookItem.Price = strNewPrice;
                    book_item.BookItem.RefreshListView();
                }
            }
        }

        private void textBox_itemBarcode_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_register;
        }

        private void textBox_itemBarcode_Leave(object sender, EventArgs e)
        {
            this.AcceptButton = null;
        }

        static List<BookItem> GetBookItemList(List<InputBookItem> items)
        {
            List<BookItem> results = new List<BookItem>();
            for (int i = 0; i < items.Count; i++)
            {
                results.Add(items[i].BookItem);
            }

            return results;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.button_OK.Enabled = false;

            try
            {
                // 警告尚未输入条码的行
                int nBlankCount = 0;
                for (int i = 0; i < this.listView_barcodes.Items.Count; i++)
                {
                    string strBarcode = this.listView_barcodes.Items[i].Text;
                    if (String.IsNullOrEmpty(strBarcode) == true)
                        nBlankCount++;
                }

                if (nBlankCount > 0)
                {
                    DialogResult result = MessageBox.Show(this,
                        "当前有 "+nBlankCount.ToString()+" 个册尚未输入条码。\r\n\r\n确实要结束条码输入? ",
                        "InputItemBarcodeDialog",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;

                    // 否则继续
                }

                // 条码查重?
                string strError = "";
                int nRet = this.UpdateData(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                if (this.DetectBarcodeDup != null)
                {
                    DetectBarcodeDupEventArgs e1 = new DetectBarcodeDupEventArgs();
                    e1.EntityControl = this.EntityControl;
                    e1.BookItems = GetBookItemList(this.BookItems);
                    this.DetectBarcodeDup(this, e1);

                    if (e1.Result == -1 || e1.Result == 1)
                    {
                        // TODO: 可否包含MessageBox标题?
                        MessageBox.Show(this, e1.ErrorInfo.Replace("; ", "\r\n"));
                        return;
                    }
                }

                UpdateBookItemsPrice();
            }
            finally
            {
                this.button_OK.Enabled = true;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            RestoreOldBarcodes();

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void RestoreOldBarcodes()
        {
            if (this.m_oldBarcodes == null)
                return;

            for (int i = 0; i < this.m_oldBarcodes.Count; i++)
            {
                InputBookItem book_item = this.BookItems[i];

                if (book_item.BookItem.Barcode != this.m_oldBarcodes[i])
                {
                    book_item.BookItem.Barcode = this.m_oldBarcodes[i];
                    book_item.BookItem.RefreshListView();
                }
            }
        }

        private void listView_barcodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = UpdateData(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            if (this.listView_barcodes.SelectedItems.Count == 0)
            {
                this.toolStripButton_modifyByBiblioPrice.Enabled = false;
                this.toolStripButton_modifyByOrderPrice.Enabled = false;
                this.toolStripButton_modifyByArrivePrice.Enabled = false;
                this.toolStripButton_modifyPrice.Enabled = false;
                this.toolStripButton_discount.Enabled = false;
            }
            else
            {
                this.toolStripButton_modifyByBiblioPrice.Enabled = true;
                this.toolStripButton_modifyByOrderPrice.Enabled = true;
                this.toolStripButton_modifyByArrivePrice.Enabled = true;
                this.toolStripButton_modifyPrice.Enabled = true;
                this.toolStripButton_discount.Enabled = true;
            }

            if (this.listView_barcodes.SelectedItems.Count != 1)
            {
                this.textBox_itemBarcode.Text = "";
                this.textBox_itemBarcode.Enabled = false;
                this.button_register.Enabled = false;
                m_nIndex = -1;
                return;
            }

            int nIndex = this.listView_barcodes.SelectedIndices[0];

            // 将变化后的行条码装入textbox
            if (nIndex != m_nIndex)
            {
                ListViewItem item = this.listView_barcodes.Items[nIndex];

                this.textBox_itemBarcode.Enabled = true;
                this.button_register.Enabled = true;

                this.textBox_itemBarcode.Text = item.Text;

                m_nIndex = nIndex;
                this.m_bTextChanged = false;
            }

            if (this.checkBox_alwaysFocusInputBox.Checked == true)
                API.PostMessage(this.Handle, WM_ACTIVATE_BARCODE_INPUT, 0, 0);
       }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_ACTIVATE_BARCODE_INPUT:
                    if (this.textBox_itemBarcode.Enabled == true)
                    {
                        this.textBox_itemBarcode.SelectAll();
                        this.textBox_itemBarcode.Focus();
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void textBox_itemBarcode_TextChanged(object sender, EventArgs e)
        {
            this.m_bTextChanged = true;
        }

        /*
        public bool BarcodeMode
        {
            get
            {
                if (this.toolStripButton_barcodeMode.Checked == true)
                    return true;
                return false;
            }
            set
            {
                if (value == true)
                {
                    this.toolStripButton_barcodeMode.Checked = true;
                    this.toolStripButton_priceMode.Checked = false;
                }
                else
                {
                    this.toolStripButton_barcodeMode.Checked = false;
                    this.toolStripButton_priceMode.Checked = true;
                }
            }
        }
         * */

        private void listView_barcodes_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.listView_barcodes.SelectedItems.Count == 1)
            {
                if (this.checkBox_alwaysFocusInputBox.Checked == true)
                    API.PostMessage(this.Handle, WM_ACTIVATE_BARCODE_INPUT, 0, 0);
            }

            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("按书目价 重设价格(&B)");
            menuItem.Click += new System.EventHandler(this.menu_modifyPriceByBiblioPrice_Click);
            if (this.listView_barcodes.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("按订购价 重设价格(&O)");
            menuItem.Click += new System.EventHandler(this.menu_modifyPriceByOrderPrice_Click);
            if (this.listView_barcodes.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("按验收价 重设价格(&A)");
            menuItem.Click += new System.EventHandler(this.menu_modifyPriceByArrivePrice_Click);
            if (this.listView_barcodes.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("重设价格(&M)");
            menuItem.Click += new System.EventHandler(this.menu_modifyPrice_Click);
            if (this.listView_barcodes.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("附加折扣(&M)");
            menuItem.Click += new System.EventHandler(this.menu_appendDiscount_Click);
            if (this.listView_barcodes.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(this.listView_barcodes, new Point(e.X, e.Y));	
        }

        // 按照订购价重设价格
        void menu_modifyPriceByOrderPrice_Click(object sender, EventArgs e)
        {
            ModifyPriceBy("订购价");
        }

        void ModifyPriceBy(string strRefName)
        {
            foreach (ListViewItem item in this.listView_barcodes.SelectedItems)
            {
                // ListViewItem item = this.listView_barcodes.SelectedItems[i];
                string strRefPrice = ListViewUtil.GetItemText(item, COLUMN_REF_PRICE);

                // 将逗号间隔的参数表解析到Hashtable中
                // parameters:
                //      strText 字符串。形态如 "名1=值1,名2=值2"
                Hashtable table = StringUtil.ParseParameters(strRefPrice,
                    ';',
                    ':');

                ListViewUtil.ChangeItemText(item, COLUMN_PRICE, (string)table[strRefName]);
            }
        }

        // 按照验收价重设价格
        void menu_modifyPriceByArrivePrice_Click(object sender, EventArgs e)
        {
            ModifyPriceBy("验收价");
        }

        // 按照书目价重设价格
        void menu_modifyPriceByBiblioPrice_Click(object sender, EventArgs e)
        {
            ModifyPriceBy("书目价");
        }

        public string UsedDiscountString
        {
            get
            {
                if (this.AppInfo == null)
                    return "";
                return this.AppInfo.GetString(
                    "input_item_barcode_dialog",
                    "used_discount",
                    "0.75");
            }
            set
            {
                if (this.AppInfo == null)
                    return;
                this.AppInfo.SetString(
                    "input_item_barcode_dialog",
                    "used_discount",
                    value);

            }

        }

        // 附加折扣
        void menu_appendDiscount_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strDiscountPart = InputDlg.GetInput(
    this,
    "为已有的价格字符串附加折扣部分",
    "折扣: ",
    this.UsedDiscountString,
    this.Font);
            if (strDiscountPart == null)
                return;

            strDiscountPart = strDiscountPart.Trim();

            if (string.IsNullOrEmpty(strDiscountPart) == true)
            {
                strError = "所输入的折扣部分为空，放弃处理";
                goto ERROR1;
            }

            if (strDiscountPart[0] == '*')
                strDiscountPart = strDiscountPart.Substring(1).Trim();

            if (string.IsNullOrEmpty(strDiscountPart) == true)
            {
                strError = "所输入的折扣部分的有效部分为空，放弃处理";
                goto ERROR1;
            }

            this.UsedDiscountString = strDiscountPart;  // 记忆

            foreach (ListViewItem item in this.listView_barcodes.SelectedItems)
            {
                // ListViewItem item = this.listView_barcodes.SelectedItems[i];
                string strOldPrice = ListViewUtil.GetItemText(item, COLUMN_PRICE);
                if (string.IsNullOrEmpty(strOldPrice) == true)
                {
                    strError = "第 "+(this.listView_barcodes.Items.IndexOf(item) + 1).ToString()+" 个事项价格部分为空，无法附加折扣部分。操作中断";
                    goto ERROR1;
                }

                int nRet = strOldPrice.IndexOf("*");
                if (nRet != -1)
                    strOldPrice = strOldPrice.Substring(0, nRet).Trim();

                strOldPrice += "*" + strDiscountPart;

                ListViewUtil.ChangeItemText(item, COLUMN_PRICE, strOldPrice);
            }
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 重设价格
        void menu_modifyPrice_Click(object sender, EventArgs e)
        {
            string strNewPrice = InputDlg.GetInput(
    this,
    "重设选定的事项的价格",
    "价格: ",
    "",
    this.Font);
            if (strNewPrice == null)
                return;

            foreach (ListViewItem item in this.listView_barcodes.SelectedItems)
            {
                // ListViewItem item = this.listView_barcodes.SelectedItems[i];
                ListViewUtil.ChangeItemText(item, COLUMN_PRICE, strNewPrice);
            }
        }

        private void checkBox_alwaysFocusInputBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_alwaysFocusInputBox.Checked == true)
                API.PostMessage(this.Handle, WM_ACTIVATE_BARCODE_INPUT, 0, 0);
        }

        private void toolStripButton_modifyByBiblioPrice_Click(object sender, EventArgs e)
        {
            menu_modifyPriceByBiblioPrice_Click(sender, e);
        }

        private void toolStripButton_modifyByOrderPrice_Click(object sender, EventArgs e)
        {
            menu_modifyPriceByOrderPrice_Click(sender, e);

        }

        private void toolStripButton_modifyByArrivePrice_Click(object sender, EventArgs e)
        {
            menu_modifyPriceByArrivePrice_Click(sender, e);

        }

        private void toolStripButton_modifyPrice_Click(object sender, EventArgs e)
        {
            menu_modifyPrice_Click(sender, e);
        }

        private void toolStripButton_discount_Click(object sender, EventArgs e)
        {
            menu_appendDiscount_Click(sender, e);
        }
    }

    
    /// <summary>
    /// 对条码进行查重
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void DetectBarcodeDupHandler(object sender,
    DetectBarcodeDupEventArgs e);

    /// <summary>
    /// 对条码进行查重的参数
    /// </summary>
    public class DetectBarcodeDupEventArgs : EventArgs
    {
        /// <summary>
        /// EntityControl 控件
        /// </summary>
        public EntityControl EntityControl = null;

        /// <summary>
        /// BookItem 的集合
        /// </summary>
        public List<BookItem> BookItems = null;

        /// <summary>
        /// 返回出错信息
        /// </summary>
        public string ErrorInfo = "";

        // return:
        //      -1  出错。错误信息在ErrorInfo中
        //      0   没有重
        //      1   有重。信息在ErrorInfo中
        /// <summary>
        /// 查重结果：
        /// <para>-1:  出错。错误信息在ErrorInfo中</para>
        /// <para>0:   没有重</para>
        /// <para>1:   有重。信息在ErrorInfo中</para>
        /// </summary>
        public int Result = 0;
    }

    internal class InputBookItem
    {
        public string Sequence = "";    // 套序。例如“1/7”
        public string OtherPrices = ""; // 候选的其他价格。格式为: "订购价:CNY12.00;验收价:CNY15.00"
        public BookItem BookItem = null;
    }
}