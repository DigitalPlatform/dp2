using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using System.Xml;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.LibraryClient;

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
            _splitContainer_main = this.splitContainer_main;
            _tableLayoutPanel_main = this.tableLayoutPanel_main;
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
        }

        void entityEditControl_editing_LocationStringChanged(object sender, TextChangeEventArgs e)
        {
            string strError = "";

            if (this.entityEditControl_editing.Initializing == false
                && string.IsNullOrEmpty(this.entityEditControl_editing.AccessNo) == false)
            {
                // MessageBox.Show(this, "修改 old '"+e.OldText+"' new '"+e.NewText+"'" );

                ArrangementInfo old_info = null;
                string strOldName = "[not found]";
                // 获得关于一个特定馆藏地点的索取号配置信息
                // <returns>-1: 出错; 0: 没有找到; 1: 找到</returns>
                int nRet = this.MainForm.GetArrangementInfo(e.OldText,
                    out old_info,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                    strOldName = old_info.ArrangeGroupName;

                ArrangementInfo new_info = null;
                string strNewName = "[not found]";
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = this.MainForm.GetArrangementInfo(e.NewText,
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
                    strLibraryCode,
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
                    strError = "价格字符串格式不合法: " +strError;
                    goto ERROR1;
                }
            }

            string strIssueDbName = "";

            if (string.IsNullOrEmpty(this.BiblioDbName) == false)
                strIssueDbName = this.MainForm.GetIssueDbName(this.BiblioDbName);

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
            // this.MainForm.stopManager.Active(this.stop);

            this.MainForm.MenuItem_recoverUrgentLog.Enabled = false;
            this.MainForm.MenuItem_font.Enabled = false;
            this.MainForm.MenuItem_restoreDefaultFont.Enabled = false;
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
                    Debug.Assert(false, "未知的栏目名称 '" +e.Name+ "'");
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
            EntityFormOptionDlg dlg = new EntityFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.MainForm = this.MainForm;
            dlg.DisplayStyle = "normal_entity";
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
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
    }

    /// <summary>
    /// 册记录编辑对话框的基础类
    /// </summary>
    public class EntityEditFormBase : ItemEditFormBase<BookItem, BookItemCollection>
    {
    }
}