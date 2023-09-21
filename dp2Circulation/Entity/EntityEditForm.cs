using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.Web;
using System.IO;

using Newtonsoft.Json;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.RFID.UI;
using DigitalPlatform.RFID;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.CommonControl;
using static dp2Circulation.MainForm;
using static dp2Circulation.CallNumberForm;

namespace dp2Circulation
{
    /// <summary>
    /// 册记录编辑对话框
    /// </summary>
    public partial class EntityEditForm : EntityEditFormBase
    // ItemEditFormBase<BookItem, BookItemCollection>
    {
        // Ctrl+A自动创建数据
        /// <summary>
        /// 自动创建数据
        /// </summary>
        public event GenerateDataEventHandler GenerateData = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public EntityEditForm()
        {
            InitializeComponent();

            _editing = this.entityEditControl_editing;
            _existing = this.entityEditControl_existing;

            _label_editing = this.label_editing;
            _button_editing_undoMaskDelete = this.button_editing_undoMaskDelete;
            _button_editing_nextRecord = this.toolStripButton_next; //  this.button_editing_nextRecord;
            _button_editing_prevRecord = this.toolStripButton_prev; //  this.button_editing_prevRecord;

            _checkBox_autoSearchDup = this.checkBox_autoSearchDup;

            _button_OK = this.button_OK;
            _button_Cancel = this.button_Cancel;

            _textBox_message = this.textBox_message;
            _splitContainer_main = this.splitContainer_itemArea;
            _tableLayoutPanel_main = this.tableLayoutPanel_main;

            this.chipEditor_existing.Title = "标签中原有内容";
            this.chipEditor_editing.Title = "即将写入的内容";

            // 2020/9/15 移动到这里
            LoadExternalFields();
        }

        /// <summary>
        /// 编辑区显示模式
        /// </summary>
        public string DisplayMode
        {
            get
            {
                return this.entityEditControl_editing.DisplayMode;
            }
            set
            {
                this.entityEditControl_editing.DisplayMode = value;
                this.entityEditControl_existing.DisplayMode = value;
            }
        }

        /// <summary>
        /// 当前记录的编辑器
        /// </summary>
        public EntityEditControl Editing
        {
            get
            {
                return entityEditControl_editing;
            }
        }

        /// <summary>
        /// 已存在记录的编辑器
        /// </summary>
        public EntityEditControl Existing
        {
            get
            {
                return entityEditControl_existing;
            }
        }

        /// <summary>
        /// 获取索取号事项集合
        /// </summary>
        /// <returns>CallNumberItem 事项集合</returns>
        public List<CallNumberItem> GetCallNumberItems()
        {
            List<CallNumberItem> callnumber_items = this.Items.GetCallNumberItems();

            CallNumberItem item = null;

            int index = this.Items.IndexOf(this.Item);
            if (index == -1)
            {
                // 增补一个对象
                item = new CallNumberItem();
                callnumber_items.Add(item);

                item.CallNumber = "";   // 不要给出当前的，以免影响到取号结果
            }
            else
            {
                // 刷新自己的位置
                item = callnumber_items[index];
                item.CallNumber = entityEditControl_editing.AccessNo;
            }

            item.RecPath = this.entityEditControl_editing.RecPath;

#if REF
            // 2017/4/6
            if (string.IsNullOrEmpty(item.RecPath))
            {
                if (string.IsNullOrEmpty(entityEditControl_editing.RefID) == true)
                {
                    // throw new Exception("entityEditControl_editing 的 RefID 成员不应为空"); // TODO: 可以考虑增加健壮性，当时发生 RefID 字符串
                    entityEditControl_editing.RefID = Guid.NewGuid().ToString();
                }

                item.RecPath = "@refID:" + entityEditControl_editing.RefID;
            }
#endif

            item.Location = entityEditControl_editing.LocationString;
            item.Barcode = entityEditControl_editing.Barcode;

            return callnumber_items;
        }

        private void EntityEditForm_Load(object sender, EventArgs e)
        {
            this.entityEditControl_editing.GetAccessNoButton.Click -= new EventHandler(button_getAccessNo_Click);
            this.entityEditControl_editing.GetAccessNoButton.Click += new EventHandler(button_getAccessNo_Click);

            this.entityEditControl_editing.LocationStringChanged -= new TextChangeEventHandler(entityEditControl_editing_LocationStringChanged);
            this.entityEditControl_editing.LocationStringChanged += new TextChangeEventHandler(entityEditControl_editing_LocationStringChanged);

            //
        }

        void LoadExternalFields()
        {
            string strError = "";
            // 从配置文件装载字段配置，初始化这些字段
            string strFileName = Path.Combine(Program.MainForm.UserDir, "item_extend.xml");
            if (File.Exists(strFileName) == true)
            {
                int nRet = this.entityEditControl_editing.LoadConfig(strFileName,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
                //if (nRet == -1)
                //    this.ShowMessage(strError, "red", true);
                nRet = this.entityEditControl_existing.LoadConfig(strFileName,
                    out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);
            }
        }

        void entityEditControl_editing_LocationStringChanged(object sender, TextChangeEventArgs e)
        {
            string strError = "";

            if (this.entityEditControl_editing.Initializing == false
                && string.IsNullOrEmpty(this.entityEditControl_editing.AccessNo) == false
                && this.entityEditControl_editing.AccessNo != "@accessNo")
            {
                // MessageBox.Show(this, "修改 old '"+e.OldText+"' new '"+e.NewText+"'" );

                ArrangementInfo old_info = null;
                string strOldName = "[not found]";
                // 获得关于一个特定馆藏地点的索取号配置信息
                // <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
                int nRet = Program.MainForm.GetArrangementInfo(e.OldText,
                    out old_info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strOldName = old_info.ArrangeGroupName;

                ArrangementInfo new_info = null;
                string strNewName = "[not found]";
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(e.NewText,
                   out new_info,
                   out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strNewName = new_info.ArrangeGroupName;

                if (strOldName != strNewName)
                {
                    DialogResult result = MessageBox.Show(this,
    "您修改了馆藏地点，因而变动了记录所从属的排架体系，现有的索取号已不再适合变动后的排架体系。\r\n\r\n是否要把窗口中索取号字段内容清空，以便您稍后重新创建索取号?",
    "EntityEditForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.No)
                        return;
                    this.entityEditControl_editing.AccessNo = "";
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public List<MemoTailNumber> ProtectedNumbers = new List<MemoTailNumber>();

        // 获得索取号
        void button_getAccessNo_Click(object sender, EventArgs e)
        {
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                if (Control.ModifierKeys == Keys.Control)
                    e1.ScriptEntry = "ManageCallNumber";
                else
                    e1.ScriptEntry = "CreateCallNumber";
                e1.FocusedControl = sender; // sender为最原始的子控件
                this.GenerateData(this, e1);
                var parameter = e1.Parameter as GetCallNumberParameter;
                if (parameter != null)
                {
                    if (parameter.ProtectedNumbers != null)
                        this.ProtectedNumbers.AddRange(parameter.ProtectedNumbers);
                }
            }
        }

        private void EntityEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        EntityControl EntityControl
        {
            get
            {
                return (EntityControl)this.ItemControl;
            }
        }

        /// <summary>
        /// 结束前的校验
        /// </summary>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 出错; 0: 没有错误</returns>
        internal override int FinishVerify(out string strError)
        {
            strError = "";
            int nRet = 0;

            // TODO: 将来这里要允许使用整个 location 字符串，而不仅仅是馆代码，来发起条码号校验
            string strLocation = this.entityEditControl_editing.LocationString;
            string strLibraryCode = Global.GetLibraryCode(StringUtil.GetPureLocation(strLocation));

            string strBarcode = this.entityEditControl_editing.Barcode;

            // 检查册条码号形式是否合法
            if (String.IsNullOrEmpty(strBarcode) == false   // 2009/2/23 
                && this.EntityControl != null
                && this.EntityControl.NeedVerifyItemBarcode == true)
            {
                // 形式校验条码号
                // return:
                //      -2  服务器没有配置校验方法，无法校验
                //      -1  error
                //      0   不是合法的条码号
                //      1   是合法的读者证条码号
                //      2   是合法的册条码号
                nRet = this.EntityControl.DoVerifyBarcode(
                    string.IsNullOrEmpty(Program.MainForm.BarcodeValidation) ? strLibraryCode : strLocation,    // 2019/7/12
                    strBarcode,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 输入的条码格式不合法
                if (nRet == 0)
                {
                    strError = "您输入的条码 " + strBarcode + " 格式不正确(" + strError + ")。请重新输入。";
                    goto ERROR1;
                }

                // 实际输入的是读者证条码号
                if (nRet == 1)
                {
                    strError = "您输入的条码号 " + strBarcode + " 是读者证条码号。请输入册条码号。";
                    goto ERROR1;
                }

                // 对于服务器没有配置校验功能，但是前端发出了校验要求的情况，警告一下
                if (nRet == -2)
                    MessageBox.Show(this, "警告：前端册管理窗开启了校验条码功能，但是服务器端缺乏相应的脚本函数，无法校验条码。\r\n\r\n若要避免出现此警告对话框，请关闭前端册管理窗校验功能");
            }

            // 馆藏地点字符串里面不能有星号
            // string strLocation = this.entityEditControl_editing.LocationString;
            if (strLocation.IndexOf("*") != -1)
            {
                strError = "馆藏地点字符串中不允许出现字符 '*'";
                goto ERROR1;
            }

            // 价格字符串中不允许出现 @
            string strPrice = this.entityEditControl_editing.Price;
            if (strPrice.IndexOf("@") != -1)
            {
                strError = "价格字符串中不允许出现字符 '@'";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(strPrice) == false)
            {
                CurrencyItem item = null;
                // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
                nRet = PriceUtil.ParseSinglePrice(strPrice,
                    out item,
                    out strError);
                if (nRet == -1)
                {
                    strError = "价格字符串格式不合法: " + strError;
                    goto ERROR1;
                }
            }

            string strIssueDbName = "";

            if (string.IsNullOrEmpty(this.BiblioDbName) == false)
                strIssueDbName = Program.MainForm.GetIssueDbName(this.BiblioDbName);

            if (string.IsNullOrEmpty(strIssueDbName) == false)
            {
                // 2014/10/23
                if (string.IsNullOrEmpty(this.entityEditControl_editing.PublishTime) == false)
                {
                    // 检查出版时间范围字符串是否合法
                    // 如果使用单个出版时间来调用本函数，也是可以的
                    // return:
                    //      -1  出错
                    //      0   正确
                    nRet = LibraryServerUtil.CheckPublishTimeRange(this.entityEditControl_editing.PublishTime,
                        out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                }

                // 2014/10/23
                if (string.IsNullOrEmpty(this.entityEditControl_editing.Volume) == false)
                {
                    List<VolumeInfo> infos = null;
                    nRet = VolumeInfo.BuildVolumeInfos(this.entityEditControl_editing.Volume,
                        out infos,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "卷期字符串 '" + this.entityEditControl_editing.Volume + "' 格式错误: " + strError;
                        goto ERROR1;
                    }
                }
            }

            return 0;
        ERROR1:
            return -1;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            OnButton_OK_Click(sender, e);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        internal override int RestoreVerify(out string strError)
        {
            strError = "";
            int nRet = 0;

            // TODO: 是否当这个checkbox为false的时候，至少也要检查本种之类的重复情形？
            // 如果这里不检查，可否在提交保存的时候，先查完本种之类的重复，才真正向服务器提交?
            if (this.checkBox_autoSearchDup.Checked == true
                && this.EntityControl != null
                && String.IsNullOrEmpty(this.entityEditControl_editing.Barcode) == false)   // 2008/11/3 不检查空的条码号是否重复
            {
                // Debug.Assert(false, "");
                // 条码查重
                // return:
                //      -1  出错
                //      0   不重复
                //      1   重复
                nRet = this.EntityControl.CheckBarcodeDup(
                    this.entityEditControl_editing.Barcode,
                    this.Item,
                    true,   // bCheckCurrentList,
                    true,   // bCheckDb,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return -1;   // 重复
            }

            return 0;
        }

        private void EntityEditForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);

            Program.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            Program.MainForm.MenuItem_font.Enabled = false;
            Program.MainForm.MenuItem_restoreDefaultFont.Enabled = false;

            //if (string.IsNullOrEmpty(RfidManager.Url) == false)
            //    RfidManager.Pause = false;
        }

        // 撤销标记删除状态
        private void button_editing_undoMaskDelete_Click(object sender, EventArgs e)
        {
            if (this.Items != null)
            {
                this.Items.UndoMaskDeleteItem(this.Item);
                this.entityEditControl_editing.SetReadOnly("librarian");
                this.checkBox_autoSearchDup.Enabled = true;
                // this.button_OK.Enabled = entityEditControl_editing.Changed;
            }
        }

#if NO
        private void button_editing_prevRecord_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);

            LoadPrevOrNextItem(true);
            EnablePrevNextRecordButtons();

            this.EnableControls(true);
        }

        private void button_editing_nextRecord_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);

            LoadPrevOrNextItem(false);
            EnablePrevNextRecordButtons();

            this.EnableControls(true);
        }
#endif

        private void entityEditControl_editing_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            // this.button_OK.Enabled = e.CurrentChanged;
            SetOkButtonState();

            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl) == false)
            {
                try
                {
                    BookItem item = this.Item.Clone();
                    item.RecordDom = this._editing.DataDom;
                    this.chipEditor_editing.LogicChipItem = BuildChip(item);
                    this.toolStripButton_saveRfid.Enabled = true;
                }
                catch (Exception ex)
                {
                    // 2021/2/2
                    this.toolStripButton_saveRfid.Enabled = false;

                    SetMessage(ex.Message);
                }
#if NO
                int nRet = this.Restore(true, out string strError);
                if (nRet != -1)
                {
                    try
                    {
                        this.chipEditor_editing.LogicChipItem = BuildChip(this.Item);
                    }
                    catch (Exception ex)
                    {
                        SetMessage(ex.Message);
                    }
                }
#endif
            }
        }

        static string DoAction(
            string strAction,
            string strValue)
        {
            string strError = "";
            string strResult = "";
            int nNumber = 0;
            int nRet = 0;

            if (strAction == "minus")
            {
                nNumber = -1;

                // 给一个被字符引导的数字增加一个数量。
                // 例如 B019 + 1 变成 B020
                nRet = StringUtil.IncreaseLeadNumber(strValue,
                    nNumber,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    strResult = strError;
                return strResult;
            }
            else if (strAction == "plus")
            {
                nNumber = 1;

                // 给一个被字符引导的数字增加一个数量。
                // 例如 B019 + 1 变成 B020
                nRet = StringUtil.IncreaseLeadNumber(strValue,
                    nNumber,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    strResult = strError;
                return strResult;
            }
            else if (strAction == "copy")
                return strValue;
            else
                return "未知的strAction值 '" + strAction + "'";
        }

        // entityeditcontrol的某个输入域触发了按键
        private void entityEditControl_editing_ControlKeyDown(object sender,
            ControlKeyEventArgs e)
        {
            string strAction = "copy";

            bool bUp = false;

            Debug.WriteLine("keycode=" + e.e.KeyCode.ToString());

            if (e.e.KeyCode == Keys.A && e.e.Control == true)
            {
                if (this.GenerateData != null)
                {
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    e1.FocusedControl = sender; // sender为 EntityEditControl
                    this.GenerateData(this, e1);
                }
                e.e.SuppressKeyPress = true;    // 2015/5/28
                return;
            }
            else if (e.e.KeyCode == Keys.PageDown && e.e.Control == true)
            {
                // this.button_editing_nextRecord_Click(null, null);
                this.toolStripButton_next_Click(null, null);
                return;
            }
            else if (e.e.KeyCode == Keys.PageUp && e.e.Control == true)
            {
                // this.button_editing_prevRecord_Click(null, null);
                this.toolStripButton_prev_Click(null, null);
                return;
            }
            else if (e.e.KeyCode == Keys.OemOpenBrackets && e.e.Control == true)
            {
                bUp = true; // 从上面拷贝
            }
            else if (e.e.KeyCode == Keys.OemCloseBrackets && e.e.Control == true)
            {
                bUp = false;    // 从下面拷贝
            }
            else if (e.e.KeyCode == Keys.OemMinus && e.e.Control == true)
            {
                bUp = true; // 从上面减量
                strAction = "minus";
            }
            else if (e.e.KeyCode == Keys.Oemplus && e.e.Control == true)
            {
                bUp = true;    // 从上面增量
                strAction = "plus";
            }
            else if (e.e.KeyCode == Keys.D0 && e.e.Control == true)
            {
                bUp = false; // 从下面减量
                strAction = "minus";
            }
            else if (e.e.KeyCode == Keys.D9 && e.e.Control == true)
            {
                bUp = false;    // 从下面增量
                strAction = "plus";
            }
            else
                return;

            string strError = "";
            BookItem bookitem = GetPrevOrNextItem(bUp, out strError);
            if (bookitem == null)
                return;
            switch (e.Name)
            {
                case "PublishTime":
                    this.entityEditControl_editing.PublishTime =
                        DoAction(strAction, bookitem.PublishTime);
                    break;
                case "Seller":
                    this.entityEditControl_editing.Seller =
                        DoAction(strAction, bookitem.Seller);
                    break;
                case "Source":
                    this.entityEditControl_editing.Source =
                        DoAction(strAction, bookitem.Source);
                    break;
                case "Intact":
                    this.entityEditControl_editing.Intact =
                        DoAction(strAction, bookitem.Intact);
                    break;
                case "Binding":
                    this.entityEditControl_editing.Binding =
                        DoAction(strAction, bookitem.Binding);
                    break;
                case "Operations":
                    this.entityEditControl_editing.Operations =
                        DoAction(strAction, bookitem.Operations);
                    break;
                case "Price":
                    this.entityEditControl_editing.Price =
                        DoAction(strAction, bookitem.Price);
                    break;
                case "Barcode":
                    this.entityEditControl_editing.Barcode =
                        DoAction(strAction, bookitem.Barcode);
                    break;
                case "State":
                    this.entityEditControl_editing.State =
                        DoAction(strAction, bookitem.State);
                    break;
                case "Location":
                    this.entityEditControl_editing.LocationString =
                        DoAction(strAction, bookitem.Location);
                    break;
                case "Comment":
                    this.entityEditControl_editing.Comment =
                        DoAction(strAction, bookitem.Comment);
                    break;
                case "Borrower":
                    Console.Beep();
                    //this.entityEditControl_editing.Borrower = bookitem.Borrower;
                    break;
                case "BorrowDate":
                    Console.Beep();
                    //this.entityEditControl_editing.BorrowDate = bookitem.BorrowDate;
                    break;
                case "BorrowPeriod":
                    Console.Beep();
                    //this.entityEditControl_editing.BorrowPeriod = bookitem.BorrowPeriod;
                    break;
                case "RecPath":
                    Console.Beep();
                    //this.entityEditControl_editing.RecPath = bookitem.RecPath;
                    break;
                case "BookType":
                    this.entityEditControl_editing.BookType =
                        DoAction(strAction, bookitem.BookType);
                    break;
                case "RegisterNo":
                    this.entityEditControl_editing.RegisterNo =
                        DoAction(strAction, bookitem.RegisterNo);
                    break;
                case "MergeComment":
                    this.entityEditControl_editing.MergeComment =
                        DoAction(strAction, bookitem.MergeComment);
                    break;
                case "BatchNo":
                    this.entityEditControl_editing.BatchNo =
                        DoAction(strAction, bookitem.BatchNo);
                    break;
                case "Volume":
                    this.entityEditControl_editing.Volume =
                        DoAction(strAction, bookitem.Volume);
                    break;
                case "AccessNo":
                    this.entityEditControl_editing.AccessNo =
                        DoAction(strAction, bookitem.AccessNo);
                    break;
                case "RefID":
                    Console.Beep();
                    // this.entityEditControl_editing.RefID = bookitem.RefID;  // 2009/6/2
                    break;
                default:
                    Debug.Assert(false, "未知的栏目名称 '" + e.Name + "'");
                    return;
            }

        }

        private void entityEditControl_editing_ControlKeyPress(object sender, ControlKeyPressEventArgs e)
        {

        }

        private void toolStripButton_prev_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);

            LoadPrevOrNextItem(true);
            EnablePrevNextRecordButtons();

            this.EnableControls(true);
        }

        private void toolStripButton_next_Click(object sender, EventArgs e)
        {
            this.EnableControls(false);

            LoadPrevOrNextItem(false);
            EnablePrevNextRecordButtons();

            this.EnableControls(true);
        }

        private void toolStripButton_option_Click(object sender, EventArgs e)
        {
            using (EntityFormOptionDlg dlg = new EntityFormOptionDlg())
            {
                MainForm.SetControlFont(dlg, this.Font, false);
                // dlg.MainForm = Program.MainForm;
                dlg.DisplayStyle = "normal_entity";
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
            }
        }

        public string NextAction
        {
            get;
            set;
        }

        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            // 关闭窗口，并促使后继调用新增功能
            this.NextAction = "new";
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        void DisplayRfidPanel(bool display)
        {
            if (this.splitContainer_back.Panel2Collapsed != !display)
                this.splitContainer_back.Panel2Collapsed = !display;
            // this.panel_rfid.Visible = display;
        }

        void SetMessage(string text)
        {
            this.textBox_message.Text = text;
            this.textBox_message.Visible = text != null;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // 把芯片内容装载显示
            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl) == false)
            {
                if (this.Item != null)
                {
                    try
                    {
                        this.chipEditor_editing.LogicChipItem = BuildChip(this.Item);
                    }
                    catch (Exception ex)
                    {
                        SetMessage(ex.Message);
                    }
                }
            }

            DisplayRfidPanel(string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl) == false);

#if NO
            if (this._editing != null)
            {
                LoadItem(this.Item);

                EnablePrevNextRecordButtons();

                // 参考记录
                if (this.Item != null
                    && this.Item.Error != null)
                {

                    this._splitContainer_main.Panel1Collapsed = false;

                    string strError = "";
                    int nRet = FillExisting(out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);

                    this._existing.SetReadOnly("all");

                    // 突出差异内容
                    this._editing.HighlightDifferences(this._existing);
                }
                else
                {
                    this._tableLayoutPanel_main.RowStyles[0].Height = 0F;
                    this._textBox_message.Visible = false;

                    this._label_editing.Visible = false;
                    this._splitContainer_main.Panel1Collapsed = true;
                    this._existing.Enabled = false;
                }
            }
#endif
        }

        bool EnsureCreateAccessNo(BookItem book_item)
        {
            if (book_item.AccessNo.StartsWith("@"))
            {
                button_getAccessNo_Click(this.entityEditControl_editing.GetAccessNoButton, new EventArgs());
                return true;
            }

            return false;
        }

        // 根据 BookItem 对象构造一个 LogicChipItem 对象
        public static LogicChipItem BuildChip(BookItem book_item)
        {
            if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "3.11") < 0)
                throw new Exception("当前连接的 dp2library 必须为 3.11 或以上版本，才能使用 RFID 有关功能");

            LogicChipItem result = new LogicChipItem();

            result.AFI = LogicChipItem.DefaultBookAFI;
            result.DSFID = LogicChipItem.DefaultDSFID;
            result.EAS = LogicChipItem.DefaultBookEAS;

            // barcode --> PII
            result.NewElement(ElementOID.PII, book_item.Barcode);

            string pure_location = StringUtil.GetPureLocation(book_item.Location);
            // location --> OwnerInstitution 要配置映射关系
            // 定义一系列前缀对应的 ISIL 编码。如果 location 和前缀前方一致比对成功，则得到 ISIL 编码
            MainForm.GetOwnerInstitution(
                Program.MainForm.RfidCfgDom,
                pure_location,
                out string isil,
                out string alternative);
            if (string.IsNullOrEmpty(isil) == false)
            {
                result.NewElement(ElementOID.OwnerInstitution, isil);
            }
            else if (string.IsNullOrEmpty(alternative) == false)
            {
                result.NewElement(ElementOID.AlternativeOwnerInstitution, alternative);
            }
            else
            {
                // 2021/2/1
                // 当前册记录没有找到对应的机构代码。不适合创建 RFID 标签
                throw new Exception($"馆藏地 '{pure_location}' 没有定义机构代码，无法创建 RFID 标签");
            }

            // SetInformation？
            // 可以考虑用 volume 元素映射过来。假设 volume 元素内容符合 (xx,xx) 格式
            string value = MainForm.GetSetInformation(book_item.Volume);
            if (value != null)
                result.NewElement(ElementOID.SetInformation, value);

            // TypeOfUsage?
            // (十六进制两位数字)
            // 10 一般流通馆藏
            // 20 非流通馆藏。保存本库? 加工中?
            // 70 被剔旧的馆藏。和 state 元素应该有某种对应关系，比如“注销”
            {
                string typeOfUsage = "";
                if (StringUtil.IsInList("注销", book_item.State) == true
                    || StringUtil.IsInList("丢失", book_item.State) == true)
                    typeOfUsage = "70";
                else if (string.IsNullOrEmpty(book_item.State) == false
                    && StringUtil.IsInList("加工中", book_item.State) == true)
                    typeOfUsage = "20";
                else
                    typeOfUsage = "10";

                result.NewElement(ElementOID.TypeOfUsage, typeOfUsage);
            }

            // AccessNo --> ShelfLocation
            // 注意去掉 {ns} 部分
            result.NewElement(ElementOID.ShelfLocation,
                StringUtil.GetPlainTextCallNumber(book_item.AccessNo)
                );

            return result;
        }

        // 左侧编辑器是否成功装载过
        bool _leftLoaded = false;

        // 写入右侧的信息到标签
        private void toolStripButton_saveRfid_Click(object sender, EventArgs e)
        {
            string strError = "";

            BookItem item = this.Item.Clone();
            {
                item.RecordDom = this._editing.DataDom;
                // 确保自动创建索取号
                EnsureCreateAccessNo(item);

                // 2020/10/27
                // 检查册记录编辑器里面 PII (册条码号) 是否为空
                string barcode = DomUtil.GetElementText(item.RecordDom.DocumentElement, "barcode");
                if (string.IsNullOrEmpty(barcode))
                {
                    strError = "在写入 RFID 标签以前，请先为册记录输入正确的册条码号";
                    goto ERROR1;
                }
            }

            // 写入以前，装载标签内容到左侧，然后调整右侧(中间可能会警告)。然后再保存

            // string pii = this.chipEditor_editing.LogicChipItem.FindElement(ElementOID.PII).Text;
            string pii = this.Item.ItemDisplayState == ItemDisplayState.New ?
                "" : GetPII(this.Item.OldRecord);   // 从修改前的册记录中获得册条码号

            // 注: 如果 this.Item.OldRecord 中旧记录的册条码号和新记录中的不同，
            // 这种情况下装载读写器上的标签的原有内容，如果：
            // 1) 标签的 PII 和旧记录中的册条码号相同，意味着操作者把这册图书的标签(本次改写标签之前的状态)放到读写器上了，那么这种情况不应该警告;
            // 2) 标签的 PII 和新记录中的册条码号相同，意味着操作者在别的什么地方抢先把标签修改到位了(但比较可疑)，那么这种情况似乎也不应该警告;
            // 3) 标签的 PII 和上述册条码号两种值都不相同，那么应该警告。
            // 不过需要注意一种情况，就是操作者通过定义册登记的默认值中的“册条码号”为一个具体的号码，这个时候 this.Item.OldRecord 中的册条码号只是这个默认模板内容的号码，而并不是什么实体库中的册记录的册条码号。其实这个时候册记录还是新增编辑状态，根本没有创建保存过
            // 似乎可以通过判断 EntitEditForm 对话框是否为“新增册”状态来甄别这种情况(通过 this.Item.ItemDisplayState 是否为 New 可以判断)



            // 看左侧是否装载过。如果没有装载过则自动装载
            if (_leftLoaded == false)
            {
                // return:
                //      -1  出错
                //      0   放弃装载
                //      1   成功装载
                int nRet = LoadOldChip(pii,
                    "adjust_right,saving",
                    // false, true, 
                    out strError);
                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
$"装载标签原有内容发生错误: {strError}。\r\n\r\n是否继续保存新内容到此标签?",
"EntityEditForm",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        goto ERROR1;
                }
                if (nRet == 0)
                {
                    strError = "已放弃保存 RFID 标签内容";
                    goto ERROR1;
                }
            }

            // 然后保存
            {
                int nRet = SaveNewChip(item, out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // TODO: 改成类似 ShowMessage() 效果
            MessageBox.Show(this, "RFID 标签保存成功");

            // 刷新左侧显示
            {
                Debug.Assert(_tagExisting != null, "");

                Debug.WriteLine("222 " + (_tagExisting.TagInfo != null ? "!=null" : "==null"));

                Debug.Assert(_tagExisting.TagInfo != null, "");
                // 2019/9/30
                Debug.Assert(_tagExisting.AntennaID == _tagExisting.TagInfo.AntennaID, $"1 _tagExisting.AntennaID({_tagExisting.AntennaID}) 应该 == _tagExisting.TagInfo.AntennaID({_tagExisting.TagInfo.AntennaID})");

                // 用保存后的确定了的 UID 重新装载
                int nRet = LoadChipByUID(
                    _tagExisting.ReaderName,
                    _tagExisting.TagInfo.UID,
                    _tagExisting.AntennaID,
    out TagInfo tag_info,
    out strError);
                if (nRet == -1)
                {
                    _leftLoaded = false;
                    strError = "保存 RFID 标签内容已经成功。但刷新左侧显示时候出错: " + strError;
                    goto ERROR1;
                }

                Debug.Assert(tag_info != null, "");

                _tagExisting.TagInfo = tag_info;
                _tagExisting.AntennaID = tag_info.AntennaID;    // 2019/9/30

                Debug.WriteLine("set taginfo");
                var chip = LogicChipItem.FromTagInfo(tag_info);
                this.chipEditor_existing.LogicChipItem = chip;

#if NO
                string new_pii = this.chipEditor_editing.LogicChipItem.FindElement(ElementOID.PII)?.Text;

                // return:
                //      -1  出错
                //      0   放弃装载
                //      1   成功装载
                int nRet = LoadOldChip(new_pii,
                    "auto_close_dialog",
                    // true, false, 
                    out strError);
                if (nRet != 1)
                {
                    // this.chipEditor_existing.LogicChipItem = null;
                    _leftLoaded = false;
                    strError = "保存 RFID 标签内容已经成功。但刷新左侧显示时候出错: " + strError;
                    goto ERROR1;
                }
#endif
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从册记录 XML 中获得册条码号
        public static string GetPII(string strItemXml)
        {
            if (string.IsNullOrEmpty(strItemXml))
                return null;
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strItemXml);
            }
            catch (Exception ex)
            {
                return null;
            }

            return DomUtil.GetElementText(dom.DocumentElement, "barcode");
        }

        // 从现有标签中装载信息到左侧，供对比使用
        private void toolStripButton_loadRfid_Click(object sender, EventArgs e)
        {
            // 如果装入的元素里面有锁定状态的元素，要警告以后，覆盖右侧编辑器中的同名元素(右侧这些元素也要显示为只读状态)
            _leftLoaded = false;
            // string pii = this.chipEditor_editing.LogicChipItem.FindElement(ElementOID.PII).Text;
            string pii = this.Item.ItemDisplayState == ItemDisplayState.New ?
                "" : GetPII(this.Item.OldRecord);   // 从修改前的册记录中获得册条码号
            // return:
            //      -1  出错
            //      0   放弃装载
            //      1   成功装载
            int nRet = LoadOldChip(pii,
                "adjust_right,auto_close_dialog",
                // false, true, 
                out string strError);
            if (nRet != 1)
                goto ERROR1;
            _leftLoaded = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        OneTag _tagExisting = null;

        // 装入以前的标签信息
        // 如果读卡器上有多个标签，则出现对话框让从中选择一个。列表中和右侧 PII 相同的，优先被选定
        // parameters:
        //      strStyle    操作方式
        //                  auto_close_dialog  是否要自动关闭选择对话框。条件是选中了 auto_select_pii 事项
        //                  /*adjust_right    是否自动调整右侧元素。即，把左侧的锁定状态元素覆盖到右侧。调整前要询问。如果不同意调整，可以放弃，然后改为放一个空白标签并装载保存 */
        //                  adjust_right 验证右侧正在编辑的内容是否会和标签上原有的锁定字段发生冲突(从而导致无法写入)
        //                  saving  是否为了保存而装载？如果是，有些提示要改变
        // return:
        //      -1  出错
        //      0   放弃装载
        //      1   成功装载
        int LoadOldChip(
            string auto_select_pii,
            string strStyle,
#if NO
            bool auto_close_dialog,
            bool adjust_right,
#endif
            out string strError)
        {
            strError = "";

            bool auto_close_dialog = StringUtil.IsInList("auto_close_dialog", strStyle);
            bool adjust_right = StringUtil.IsInList("adjust_right", strStyle);
            bool saving = StringUtil.IsInList("saving", strStyle);

            try
            {
            REDO:
                // 出现对话框让选择一个
                // SelectTagDialog dialog = new SelectTagDialog();
                using (RfidToolForm dialog = new RfidToolForm())
                {
                    dialog.Text = "选择 RFID 标签";
                    dialog.OkCancelVisible = true;
                    dialog.LayoutVertical = false;
                    dialog.AutoCloseDialog = auto_close_dialog;
                    dialog.SelectedPII = auto_select_pii;
                    dialog.AutoSelectCondition = "auto_or_blankPII";    // 2019/1/30
                    dialog.ProtocolFilter = InventoryInfo.ISO15693;
                    Program.MainForm.AppInfo.LinkFormState(dialog, "selectTagDialog_formstate");
                    dialog.ShowDialog(this);

                    if (dialog.DialogResult == DialogResult.Cancel)
                    {
                        strError = "放弃装载 RFID 标签内容";
                        return 0;
                    }

                    if (auto_close_dialog == false
                        // && string.IsNullOrEmpty(auto_select_pii) == false
                        && dialog.SelectedPII != auto_select_pii
                        && string.IsNullOrEmpty(dialog.SelectedPII) == false
                        )
                    {
                        string message = $"您所选择的标签其 PII 为 '{dialog.SelectedPII}'，和期待的 '{auto_select_pii}' 不吻合。请小心检查是否正确。\r\n\r\n是否重新选择?\r\n\r\n[是]重新选择 RFID 标签;\r\n[否]将这一种不吻合的 RFID 标签装载进来\r\n[取消]放弃装载";
                        if (saving)
                            message = $"您所选择的标签其 PII 为 '{dialog.SelectedPII}'，和期待的 '{auto_select_pii}' 不吻合。请小心检查是否正确。\r\n\r\n是否重新选择?\r\n\r\n[是]重新选择 RFID 标签;\r\n[否]将信息覆盖保存到这一种不吻合的 RFID 标签中(危险)\r\n[取消]放弃保存";

                        DialogResult temp_result = MessageBox.Show(this,
        message,
        "EntityEditForm",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Yes)
                            goto REDO;
                        if (temp_result == DialogResult.Cancel)
                        {
                            strError = "放弃装载 RFID 标签内容";
                            return 0;
                        }
                        if (saving == false)
                            MessageBox.Show(this, "警告：您刚装入了一个可疑的标签，极有可能不是当前册对应的标签。待会儿保存标签内容的时候，有可能会张冠李戴覆盖了它。保存标签内容前，请务必反复仔细检查");
                    }

                    var tag_info = dialog.SelectedTag.TagInfo;
                    _tagExisting = dialog.SelectedTag;
                    Debug.WriteLine("set _tagExisting");

                    Debug.Assert(_tagExisting != null, "");
                    Debug.Assert(_tagExisting.TagInfo != null, "");

                    var chip = LogicChipItem.FromTagInfo(tag_info);
                    this.chipEditor_existing.LogicChipItem = chip;

                    if (adjust_right)
                    {
                        int nRet = VerifyLock(this.chipEditor_existing.LogicChipItem,
        this.chipEditor_editing.LogicChipItem,
        out strError);
                        if (nRet == -1)
                            return -1;

                        /*
                        // 让右侧编辑器感受到 readonly 和 text 的变化
                        var save = this.chipEditor_editing.LogicChipItem;
                        this.chipEditor_editing.LogicChipItem = null;
                        this.chipEditor_editing.LogicChipItem = save;
                        */
                    }

                    return 1;
                }
            }
            catch (Exception ex)
            {
                this.chipEditor_existing.LogicChipItem = null;
                strError = "出现异常: " + ex.Message;
                return -1;
            }
        }

#if REMOVED
        // 把新旧芯片内容合并。即，新芯片中不应修改旧芯片中已经锁定的元素
        //      old_chip    标签上已经存在的内容
        //      new_chip    正在编辑的内容
        public static int Merge(LogicChipItem old_chip,
            LogicChipItem new_chip,
            out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();
            // 检查一遍
            foreach (Element old_element in old_chip.Elements)
            {
                if (old_element.Locked == false)
                    continue;
                Element new_element = new_chip.FindElement(old_element.OID);
                if (new_element != null)
                {
                    if (new_element.Text != old_element.Text)
                        errors.Add($"当前标签中元素 {old_element.OID} 已经被锁定，无法进行内容合并(从 '{old_element.Text}' 修改为 '{new_element.Text}')。");
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, ";");
                strError += "\r\n\r\n强烈建议从图书上撕掉和废弃此标签，然后重新贴一个空白标签才能进行写入";
                return -1;
            }

            /*
            bool changed = false;
            foreach (Element old_element in old_chip.Elements)
            {
                if (old_element.Locked == false)
                    continue;
                Element new_element = new_chip.FindElement(old_element.OID);
                if (new_element != null)
                {
                    // 修改新元素
                    int index = new_chip.Elements.IndexOf(new_element);
                    Debug.Assert(index != -1);
                    new_chip.Elements.RemoveAt(index);
                    new_chip.Elements.Insert(index, old_element.Clone());
                    changed = true;
                }
            }

            if (changed)
                return 1;   // new_chip 发生改变，被标签中旧内容冲掉部分字段，无法进行修改
            */
            return 0;
        }

#endif

        // 验证，新芯片中无法修改旧芯片中已经锁定的元素
        //      old_chip    标签上已经存在的内容
        //      new_chip    正在编辑的内容
        public static int VerifyLock(LogicChipItem old_chip,
            LogicChipItem new_chip,
            out string strError)
        {
            strError = "";

            List<string> errors = new List<string>();
            // 检查一遍
            foreach (Element old_element in old_chip.Elements)
            {
                if (old_element.Locked == false)
                    continue;
                Element new_element = new_chip.FindElement(old_element.OID);
                if (new_element != null)
                {
                    if (new_element.Text != old_element.Text)
                        errors.Add($"当前标签中元素 {old_element.OID} 已经被锁定，无法进行内容合并(从 '{old_element.Text}' 修改为 '{new_element.Text}')。");
                }
            }

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, ";");
                strError += "\r\n\r\n强烈建议从图书上撕掉和废弃此标签，然后重新贴一个空白标签才能进行写入";
                return -1;
            }

            /*
            bool changed = false;
            foreach (Element old_element in old_chip.Elements)
            {
                if (old_element.Locked == false)
                    continue;
                Element new_element = new_chip.FindElement(old_element.OID);
                if (new_element != null)
                {
                    // 修改新元素
                    int index = new_chip.Elements.IndexOf(new_element);
                    Debug.Assert(index != -1);
                    new_chip.Elements.RemoveAt(index);
                    new_chip.Elements.Insert(index, old_element.Clone());
                    changed = true;
                }
            }

            if (changed)
                return 1;   // new_chip 发生改变，被标签中旧内容冲掉部分字段，无法进行修改
            */
            return 0;
        }

        // parameters:
        //      item    只用于写入统计日志
        int SaveNewChip(BookItem item, out string strError)
        {
            strError = "";

#if SN
            string filename = Path.Combine(Program.MainForm.UserDir, $"daily_counter_{"rfid"}.txt");
            var exceed = DailyCounter.IncDailyCounter(filename,
                // "rfid",
                10);
            if (exceed == true)
            {
                // 序列号中要求包含 function=rfid 参数
                // return:
                //      -1  出错
                //      0   放弃
                //      1   成功
                int nRet = Program.MainForm.VerifySerialCode("rfid", false, out strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = $"写入 RFID 标签功能尚未被许可('rfid'): {strError}";
                    return -1;
                }
            }
#endif

#if OLD_CODE
            RfidChannel channel = StartRfidChannel(
Program.MainForm.RfidCenterUrl,
out strError);
            if (channel == null)
            {
                strError = "StartRfidChannel() error";
                return -1;
            }
#endif


            try
            {
#if NO
                TagInfo new_tag_info = _tagExisting.TagInfo.Clone();
                new_tag_info.Bytes = this.chipEditor_editing.LogicChipItem.GetBytes(
                    (int)(new_tag_info.MaxBlockCount * new_tag_info.BlockSize),
                    (int)new_tag_info.BlockSize,
                    LogicChip.GetBytesStyle.None,
                    out string block_map);
                new_tag_info.LockStatus = block_map;
#endif
                Debug.Assert(_tagExisting != null, "");
                Debug.WriteLine("333 " + (_tagExisting.TagInfo != null ? "!=null" : "==null"));

                Debug.Assert(_tagExisting.TagInfo != null, "");

                /*
                if (this.chipEditor_editing.LogicChipItem == null)
                {
                    strError = "this.chipEditor_editing.LogicChipItem == null";
                    return -1;
                }
                */

                // 2020/10/27
                // 检查 PII 是否为空
                string barcode = this.chipEditor_editing.LogicChipItem.FindElement(ElementOID.PII)?.Text;
                if (string.IsNullOrEmpty(barcode))
                {
                    strError = "PII 不允许为空";
                    return -1;
                }

                TagInfo new_tag_info = LogicChipItem.ToTagInfo(
                    _tagExisting.TagInfo,
                    this.chipEditor_editing.LogicChipItem);
#if OLD_CODE
                NormalResult result = channel.Object.WriteTagInfo(
                    _tagExisting.ReaderName,
                    _tagExisting.TagInfo,
                    new_tag_info);
#else
                Debug.Assert(_tagExisting != null, "");

                Debug.WriteLine("111 " + (_tagExisting.TagInfo != null ? "!=null" : "==null"));

                Debug.Assert(_tagExisting.TagInfo != null, "");
                // 2019/9/30
                Debug.Assert(_tagExisting.AntennaID == _tagExisting.TagInfo.AntennaID, $"2 _tagExisting.AntennaID({_tagExisting.AntennaID}) 应该 == _tagExisting.TagInfo.AntennaID({_tagExisting.TagInfo.AntennaID})");

                NormalResult result = RfidManager.WriteTagInfo(
    _tagExisting.ReaderName,
    _tagExisting.TagInfo,
    new_tag_info);
                RfidTagList.ClearTagTable(_tagExisting.UID);
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                // 写入统计日志
                if (item != null)
                {
                    StatisLog log = new StatisLog
                    {
                        BookItem = item,
                        ReaderName = _tagExisting.ReaderName,
                        NewTagInfo = new_tag_info
                    };
                    AddWritingLog(log);
                }

                return 0;
            }
            catch (Exception ex)
            {
                strError = "SaveNewChip() 出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
#if OLD_CODE
                EndRfidChannel(channel);
#endif
            }
        }

        int LoadChipByUID(
            string reader_name,
            string uid,
            uint antenna_id,
            out TagInfo tag_info,
            out string strError)
        {
            strError = "";
            tag_info = null;

#if OLD_CODE
            RfidChannel channel = StartRfidChannel(
Program.MainForm.RfidCenterUrl,
out strError);
            if (channel == null)
            {
                strError = "StartRfidChannel() error";
                return -1;
            }
#endif
            try
            {
#if OLD_CODE
                var result = channel.Object.GetTagInfo(
                    reader_name,
                    uid);
#else
                var result = RfidManager.GetTagInfo(
                    reader_name,
                    uid,
                    antenna_id);
#endif
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                Debug.Assert(result.TagInfo != null, "");
                tag_info = result.TagInfo;
                return 0;
            }
            catch (Exception ex)
            {
                strError = "GetTagInfo() 出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
#if OLD_CODE
                EndRfidChannel(channel);
#endif
            }
        }

        private void EntityEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // var reason = e.CloseReason;
            if (this.DialogResult == DialogResult.Cancel)
            {
                // 对保护过的号码放弃保护
                if (ProtectedNumbers != null)
                {
                    foreach (var number in ProtectedNumbers)
                    {
                        int nRet = ProtectTailNumber(
    "unmemo",
    number.ArrangeGroupName,
    number.Class,
    number.Number,
    out string strOutputNumber,
    out string strError);
                        if (nRet == -1)
                            MessageBox.Show(this, strError);
                    }
                }
            }
        }


        // parameters:
        //      strAction   protect/unmemo 之一
        int ProtectTailNumber(
            string strAction,
            string strArrangeGroupName,
            string strClass,
            string strTestNumber,
            out string strOutputNumber,
            out string strError)
        {
            strOutputNumber = "";

            if (string.IsNullOrEmpty(strTestNumber) == false
    && strTestNumber.Contains("/") == true)
            {
                strError = $"strTestNumber 参数值中不应包含 '/' ('{strTestNumber}')";
                return -1;
            }

            // EnableControls(false);

            Debug.Assert(strAction == "protect" || strAction == "unmemo", "");

            // 显示到操作历史中
            {
                string oper_name = "保护";
                if (strAction == "unmemo")
                    oper_name = "解除保护";
                string text = $"{oper_name} 种次号 '{strTestNumber}' (类号={strClass}, 排架体系名={strArrangeGroupName})";
                Program.MainForm.OperHistory.AppendHtml("<div class='debug green'>" + HttpUtility.HtmlEncode(text) + "</div>");
            }

            LibraryChannel channel = Program.MainForm.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = new TimeSpan(0, 1, 0);
            try
            {
                long lRet = channel.SetOneClassTailNumber(
                    null,
                    strAction,
                    strArrangeGroupName,
                    strClass,
                    strTestNumber,
                    out strOutputNumber,
                    out strError);
                if (lRet == -1)
                {
                    Program.MainForm.OperHistory.AppendHtml("<div class='debug red'>" + HttpUtility.HtmlEncode($"返回出错:{strError}") + "</div>");
                    return -1;
                }

                Program.MainForm.OperHistory.AppendHtml("<div class='debug yellow'>" + HttpUtility.HtmlEncode($"返回成功:strOutputNumber={strOutputNumber}, lRet={lRet}, strError={strError}") + "</div>");
                return (int)lRet;
            }
            finally
            {
                channel.Timeout = old_timeout;
                Program.MainForm.ReturnChannel(channel);
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer_back);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.splitContainer_back);
                GuiState.SetUiState(controls, value);
            }
        }


#if NO
        // 装入以前的标签信息
        // 如果读卡器上有多个标签，则出现对话框让从中选择一个。列表中和右侧 PII 相同的，优先被选定
        // parameters:
        //      adjust_right    是否自动调整右侧元素。即，把左侧的锁定状态元素覆盖到右侧。调整前要询问。如果不同意调整，可以放弃，然后改为放一个空白标签并装载保存
        int LoadOldChip(bool adjust_right, 
            out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(Program.MainForm.RfidCenterUrl))
            {
                strError = "尚未配置 RFID 中心 URL";
                return -1;
            }
            RfidChannel channel = StartRfidChannel(
                Program.MainForm.RfidCenterUrl,
                out strError);
            if (channel == null)
                return -1;
            try
            {
                ListTagsResult result = channel.Object.ListTags("*");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    return -1;
                }

                // 出现对话框让选择一个
                SelectTagDialog dialog = new SelectTagDialog();
                dialog.Tags = result.Results;
                dialog.ShowDialog(this);
                if (dialog.DialogResult == DialogResult.Cancel)
                    return 0;

                // 装载标签详细信息
                GetTagInfoResult result1 = channel.Object.GetTagInfo(dialog.SelectedTag.ReaderName,
                    dialog.SelectedTag.UID);

                return 1;
            }
            catch(Exception ex)
            {
                strError = "出现异常: " + ex.Message;
                return -1;
            }
            finally
            {
                EndRfidChannel(channel);
            }
        }

#endif
    }

    /// <summary>
    /// 册记录编辑对话框的基础类
    /// </summary>
    public class EntityEditFormBase : ItemEditFormBase<BookItem, BookItemCollection>
    {
    }
}