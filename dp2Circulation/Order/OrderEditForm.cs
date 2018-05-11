using System;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using System.Xml;
using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 订购记录编辑对话框
    /// </summary>
    public partial class OrderEditForm : OrderEditFormBase
    // ItemEditFormBase<OrderItem, OrderItemCollection>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OrderEditForm()
        {
            InitializeComponent();

            _editing = this.orderEditControl_editing;
            _existing = this.orderEditControl_existing;

            _label_editing = this.label_editing;
            _button_editing_undoMaskDelete = this.button_editing_undoMaskDelete;
            _button_editing_nextRecord = this.button_editing_nextRecord;
            _button_editing_prevRecord = this.button_editing_prevRecord;

            _checkBox_autoSearchDup = this.checkBox_autoSearchDup;

            _button_OK = this.button_OK;
            _button_Cancel = this.button_Cancel;

            _textBox_message = this.textBox_message;
            _splitContainer_main = this.splitContainer_main;
            _tableLayoutPanel_main = this.tableLayoutPanel_main;
        }


        private void OrderEditForm_Load(object sender, EventArgs e)
        {
            this.orderEditControl_editing.EditDistributeButton.Click += EditDistributeButton_Click;
        }

        void EditDistributeButton_Click(object sender, EventArgs e)
        {
            bool bControl = Control.ModifierKeys == Keys.Control;

            string strCopy = this.orderEditControl_editing.Copy;

            string strNewCopy = "";
            string strOldCopy = "";
            OrderDesignControl.ParseOldNewValue(strCopy,
                out strOldCopy,
                out strNewCopy);
            int copy = -1;
            Int32.TryParse(OrderDesignControl.GetCopyFromCopyString(strOldCopy), out copy);

            string strDistribute = this.orderEditControl_editing.Distribute;
            DistributeDialog dlg = new DistributeDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.DistributeString = strDistribute;
            if (bControl == false)
                dlg.Count = copy;
            dlg.GetValueTable += dlg_GetValueTable;
            Program.MainForm.AppInfo.LinkFormState(dlg, "DistributeDialog_state");
            dlg.ShowDialog(this);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
            this.orderEditControl_editing.Distribute = dlg.DistributeString;
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            string strError = "";
            string[] values = null;
            int nRet = Program.MainForm.GetValueTable(e.TableName,
                e.DbName,
                out values,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            e.values = values;
        }

        private void OrderEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            this.orderEditControl_editing.GetValueTable -= new GetValueTableEventHandler(orderEditControl_editing_GetValueTable);
             * */
        }

        OrderControl OrderControl
        {
            get
            {
                return (OrderControl)this.ItemControl;
            }
        }

        public ItemEditControlBase OrderEditControl
        {
            get
            {
                return this.orderEditControl_editing;
            }
        }

        public string DisplayMode
        {
            get
            {
                return this.orderEditControl_editing.DisplayMode;
            }
            set
            {
                this.orderEditControl_editing.DisplayMode = value;
                if (value == "simplebook" || value == "simpleseries")
                {
                    this.checkBox_autoSearchDup.Visible = false;
                    this.button_editing_nextRecord.Visible = false;
                    this.button_editing_prevRecord.Visible = false;
                }
                else
                {
                    this.checkBox_autoSearchDup.Visible = true;
                    this.button_editing_nextRecord.Visible = true;
                    this.button_editing_prevRecord.Visible = true;
                }
            }
        }

        internal override int RestoreVerify(out string strError)
        {
            strError = "";
            int nRet = 0;

            // TODO: 是否当这个checkbox为false的时候，至少也要检查本种之类的重复情形？
            // 如果这里不检查，可否在提交保存的时候，先查完本种之类的重复，才真正向服务器提交?
            if (this.checkBox_autoSearchDup.Checked == true
                && this.OrderControl != null
                && String.IsNullOrEmpty(this.orderEditControl_editing.Distribute) == false)
            {
                // Debug.Assert(false, "");
                // distribute 中的 refid 查重
                // return:
                //      -1  出错
                //      0   不重复
                //      1   重复
                nRet = this.OrderControl.CheckDistributeDup(
                    this.orderEditControl_editing.Distribute,
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

        /// <summary>
        /// 结束前的校验
        /// </summary>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 出错; 0: 没有错误</returns>
        internal override int FinishVerify(out string strError)
        {
#if NO
            strError = "";
            int nRet = 0;

            string strRange = this.orderEditControl_editing.Range;
            string strOrderTime = this.orderEditControl_editing.OrderTime;

            if (string.IsNullOrEmpty(strRange) == false)
            {
                // 检查出版时间范围字符串是否合法
                // 如果使用单个出版时间来调用本函数，也是可以的
                // return:
                //      -1  出错
                //      0   正确
                nRet = LibraryServerUtil.CheckPublishTimeRange(strRange,
                    out strError);
                if (nRet == -1)
                {
                    goto ERROR1;
                }
            }

            if (string.IsNullOrEmpty(strOrderTime) == false)
            {
                try
                {
                    DateTime time = DateTimeUtil.FromRfc1123DateTimeString(strOrderTime);
                    if (time.Year == 1753)
                    {
                        strError = "订购时间字符串 '" + strOrderTime + "' 这是一个不太可能的时间";
                        goto ERROR1;
                    }
                }
                catch (Exception ex)
                {
                    strError = "订购时间字符串 '" + strOrderTime + "' 格式错误: " + ex.Message;
                    goto ERROR1;
                }
            }

            // TODO: 验证馆藏分配字符串

            return 0;
        ERROR1:
            return -1;
#endif
            // 检查各个字段内容是否正确
            // return:
            //      -1  有错
            //      0   正确
            return this.orderEditControl_editing.VerifyFields(out strError);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = 0;


            nRet = this.FinishOneOrderItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            // TODO: 提交保存后timestamp不匹配时出现的对话框，应当禁止prev/next按钮

            // 针对有报错信息的情况
            if (this.OrderItem != null
                && this.OrderItem.Error != null
                && this.OrderItem.Error.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.TimestampMismatch)
            {
                this.OrderItem.OldRecord = this.OrderItem.Error.OldRecord;
                this.OrderItem.Timestamp = this.OrderItem.Error.OldTimestamp;
            }

            this.OrderItem.Error = null; // 结束报错状态

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
#endif
            OnButton_OK_Click(sender, e);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_existing_undoMaskDelete_Click(object sender, EventArgs e)
        {
        }

        private void button_editing_undoMaskDelete_Click(object sender, EventArgs e)
        {
            if (this.Items != null)
            {
                this.Items.UndoMaskDeleteItem(this.Item);
                this.orderEditControl_editing.SetReadOnly("librarian");
                this.checkBox_autoSearchDup.Enabled = true;
                // this.button_OK.Enabled = entityEditControl_editing.Changed;
            }

        }

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

        private void orderEditControl_editing_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            SetOkButtonState();
        }

        private void orderEditControl_editing_ControlKeyDown(object sender, ControlKeyEventArgs e)
        {
            bool bUp = false;
            if (e.e.KeyCode == Keys.OemOpenBrackets && e.e.Control == true)
            {
                bUp = true; // 从上面拷贝
            }
            else if (e.e.KeyCode == Keys.OemCloseBrackets && e.e.Control == true)
            {
                bUp = false;    // 从下面拷贝
            }
            else
                return;

            string strError = "";
            OrderItem orderitem = GetPrevOrNextItem(bUp, out strError);
            if (orderitem == null)
                return;
            switch (e.Name)
            {
                case "Index":
                    this.orderEditControl_editing.Index = orderitem.Index;
                    break;
                case "State":
                    this.orderEditControl_editing.State = orderitem.State;
                    break;
                case "Seller":
                    this.orderEditControl_editing.Seller = orderitem.Seller;
                    break;
                case "Range":
                    this.orderEditControl_editing.Range = orderitem.Range;
                    break;
                case "Copy":
                    this.orderEditControl_editing.Copy = orderitem.Copy;
                    break;
                case "Price":
                    this.orderEditControl_editing.Price = orderitem.Price;
                    break;
                case "TotalPrice":
                    this.orderEditControl_editing.TotalPrice = orderitem.TotalPrice;
                    break;
                case "OrderTime":
                    this.orderEditControl_editing.OrderTime = orderitem.OrderTime;
                    break;
                case "OrderID":
                    this.orderEditControl_editing.OrderID = orderitem.OrderID;
                    break;
                case "Distribute":
                    // TODO: 复制的时候是否要自动去掉 refid 部分?
                    this.orderEditControl_editing.Distribute = orderitem.Distribute;
                    break;
                case "Comment":
                    this.orderEditControl_editing.Comment = orderitem.Comment;
                    break;
                case "BatchNo":
                    this.orderEditControl_editing.BatchNo = orderitem.BatchNo;
                    break;
                case "RecPath":
                    //this.entityEditControl_editing.RecPath = bookitem.RecPath;
                    break;
                default:
                    Debug.Assert(false, "未知的栏目名称 '" + e.Name + "'");
                    return;
            }
        }

        public static string GetXml(ItemEditControlBase control)
        {
            string strError = "";
            string strXml = "";
            int nRet = control.GetData(false, out strXml, out strError);
            if (nRet == -1)
                throw new Exception(strError);
            return strXml;
        }

        public static void SetXml(ItemEditControlBase control,
            string strXml,
            string strPublicationType)
        {
            string strError = "";

            // 去掉记录里面的 issueCount 和 range 元素
            if (string.IsNullOrEmpty(strXml) == false
                && strPublicationType == "book")
            {
                XmlDocument dom = new XmlDocument();
                DomUtil.SafeLoadXml(dom, strXml);
                DomUtil.DeleteElement(dom.DocumentElement, "range");
                DomUtil.DeleteElement(dom.DocumentElement, "issueCount");
                strXml = dom.DocumentElement.OuterXml;
            }

            int nRet = control.SetData(strXml, "", null, out strError);
            if (nRet == -1)
                throw new Exception(strError);
        }

        // return:
        //      null    strBiblioRecPath 不是书目库名
        public static string GetPublicationType(string strBiblioRecPath)
        {
            string strBiblioDbName = Global.GetDbName(strBiblioRecPath);
            BiblioDbProperty prop = Program.MainForm.GetBiblioDbProperty(strBiblioDbName);
            if (prop == null)
                return null;

            if (string.IsNullOrEmpty(prop.IssueDbName) == true)
                return "book";

            return "series";
        }

    }

    /// <summary>
    /// 订购记录编辑对话框的基础类
    /// </summary>
    public class OrderEditFormBase : ItemEditFormBase<OrderItem, OrderItemCollection>
    {
    }
}