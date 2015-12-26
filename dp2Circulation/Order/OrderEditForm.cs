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
using DigitalPlatform.LibraryServer;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 订购记录编辑对话框
    /// </summary>
    public partial class OrderEditForm : OrderEditFormBase
    // ItemEditFormBase<OrderItem, OrderItemCollection>
    {
#if NO
        /// <summary>
        /// 起始事项
        /// </summary>
        public OrderItem StartOrderItem = null;   // 最开始时的对象

        /// <summary>
        /// 当前事项
        /// </summary>
        public OrderItem OrderItem = null;

        /// <summary>
        /// 事项集合
        /// </summary>
        public OrderItemCollection OrderItems = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 订购控件
        /// </summary>
        public OrderControl OrderControl = null;
#endif

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

#if NO

        // 为编辑目的的初始化
        // parameters:
        //      bookitems   容器。用于UndoMaskDelete
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="orderitem">要编辑的订购事项</param>
        /// <param name="orderitems">事项所在的集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int InitialForEdit(
            OrderItem orderitem,
            OrderItemCollection orderitems,
            out string strError)
        {
            strError = "";

            this.OrderItem = orderitem;
            this.OrderItems = orderitems;

            this.StartOrderItem = orderitem;

            return 0;
        }

        void LoadOrderItem(OrderItem orderitem)
        {
            if (orderitem != null)
            {
                string strError = "";
                int nRet = FillEditing(orderitem, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "LoadOrderItem() 发生错误: " + strError);
                    return;
                }
            }
            if (orderitem != null
                && orderitem.ItemDisplayState == ItemDisplayState.Deleted)
            {
                // 已经标记删除的事项, 不能进行修改。但是可以观察
                this.orderEditControl_editing.SetReadOnly(ReadOnlyStyle.All);
                this.checkBox_autoSearchDup.Enabled = false;

                this.button_editing_undoMaskDelete.Enabled = true;
                this.button_editing_undoMaskDelete.Visible = true;
            }
            else
            {
                this.orderEditControl_editing.SetReadOnly(ReadOnlyStyle.Librarian);

                this.button_editing_undoMaskDelete.Enabled = false;
                this.button_editing_undoMaskDelete.Visible = false;
            }

            this.orderEditControl_editing.GetValueTable -= new GetValueTableEventHandler(orderEditControl_editing_GetValueTable);
            this.orderEditControl_editing.GetValueTable += new GetValueTableEventHandler(orderEditControl_editing_GetValueTable);

            this.OrderItem = orderitem;

            SetOkButtonState();
        }


        void orderEditControl_editing_GetValueTable(object sender, GetValueTableEventArgs e)
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

        void SetOkButtonState()
        {
            if (this.OrderItem != this.StartOrderItem)
            {
                this.button_OK.Enabled = orderEditControl_editing.Changed;
            }
            else
            {
                this.button_OK.Enabled = true;
            }
        }



        // 结束一个事项的编辑
        // return:
        //      -1  出错
        //      0   没有必要做restore
        //      1   做了restore
        int FinishOneOrderItem(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (orderEditControl_editing.Changed == false)
                return 0;

#if NO
            string strIndex = this.orderEditControl_editing.Index;

            // TODOL 检查编号形式是否合法
            if (String.IsNullOrEmpty(strIndex) == true)
            {
                strError = "编号不能为空";
                goto ERROR1;
            }
#endif

            nRet = Restore(out strError);
            if (nRet == -1)
                goto ERROR1;

            return nRet;
        ERROR1:
            return -1;
        }

        // 填充编辑界面数据
        int FillEditing(OrderItem orderitem,
            out string strError)
        {
            strError = "";

            if (orderitem == null)
            {
                strError = "orderitem参数值为空";
                return -1;
            }

            string strXml = "";
            int nRet = orderitem.BuildRecord(out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.orderEditControl_editing.SetData(strXml,
                orderitem.RecPath,
                orderitem.Timestamp,
                out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }

#endif

#if NO

        // 填充参考编辑界面数据
        int FillExisting(out string strError)
        {
            strError = "";

            if (this.OrderItem == null)
            {
                strError = "OrderItem为空";
                return -1;
            }

            if (this.OrderItem.Error == null)
            {
                strError = "OrderItem.Error为空";
                return -1;
            }

            this.textBox_message.Text = this.OrderItem.ErrorInfo;

            int nRet = this.orderEditControl_existing.SetData(this.OrderItem.Error.OldRecord,
                this.OrderItem.Error.OldRecPath, // NewRecPath
                this.OrderItem.Error.OldTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 从界面中更新orderitem中的数据
        // return:
        //      -1  error
        //      0   没有必要更新
        //      1   已经更新
        int Restore(out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (orderEditControl_editing.Changed == false)
                return 0;

            if (this.OrderItem == null)
            {
                strError = "OrderItem为空";
                return -1;
            }


            // TODO: 是否当这个checkbox为false的时候，至少也要检查本种之类的重复情形？
            // 如果这里不检查，可否在提交保存的时候，先查完本种之类的重复，才真正向服务器提交?
            if (this.checkBox_autoSearchDup.Checked == true
                && this.OrderControl != null)
            {
#if NOOOOOOOOOOOOO
                // Debug.Assert(false, "");
                // 条码查重
                // return:
                //      -1  出错
                //      0   不重复
                //      1   重复
                nRet = this.EntityForm.CheckPublishTimeDup(
                    this.issueEditControl_editing.PublishTime,
                    this.IssueItem,
                    true,   // bCheckCurrentList,
                    true,   // bCheckDb,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    return -1;   // 重复
#endif
            }

            // 获得编辑后的数据
            try
            {

                this.OrderItem.RecordDom = this.orderEditControl_editing.DataDom;
            }
            catch (Exception ex)
            {
                strError = "获得数据时出错: " + ex.Message;
                return -1;
            }

            this.OrderItem.Changed = true;
            if (this.OrderItem.ItemDisplayState != ItemDisplayState.New)
            {
                this.OrderItem.ItemDisplayState = ItemDisplayState.Changed;
                // 这意味着Deleted状态也会被修改为Changed
            }

            this.OrderItem.RefreshListView();
            return 1;
        }


        /// <summary>
        /// 是否自动查重
        /// </summary>
        public bool AutoSearchDup
        {
            get
            {
                return this.checkBox_autoSearchDup.Checked;
            }
            set
            {
                this.checkBox_autoSearchDup.Checked = value;
            }
        }

        void LoadPrevOrNextOrderItem(bool bPrev)
        {
            string strError = "";

            OrderItem new_orderitem = GetPrevOrNextOrderItem(bPrev,
                out strError);
            if (new_orderitem == null)
                goto ERROR1;

            // 保存当前事项
            int nRet = FinishOneOrderItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            LoadOrderItem(new_orderitem);

            // 在listview中滚动到可见范围
            new_orderitem.HilightListViewItem(true);
            this.Text = "册信息";
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public void EnableControls(bool bEnable)
        {
            this.button_Cancel.Enabled = bEnable;

            if (bEnable == true)
                SetOkButtonState();
            else
                this.button_OK.Enabled = false;


            if (bEnable == false)
            {
                this.button_editing_nextRecord.Enabled = bEnable;
                this.button_editing_prevRecord.Enabled = bEnable;
            }
            else
                this.EnablePrevNextRecordButtons();
        }

        // 根据当前bookitem事项在容器中的位置，设置PrevRecord和NextRecord按钮的Enabled状态
        void EnablePrevNextRecordButtons()
        {
            // 有参考记录的情况
            if (this.OrderItem != null
                && this.OrderItem.Error != null)
            {
                goto DISABLE_TWO_BUTTON;
            }


            if (this.OrderControl == null)
            {
                // 因为没有容器，所以无法prev/next，于是就diable
                goto DISABLE_TWO_BUTTON;
            }

            int nIndex = 0;

            nIndex = this.OrderControl.IndexOfVisibleItems(this.OrderItem);

            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                // Debug.Assert(false, "BookItem事项居然在容器中没有找到。");
                goto DISABLE_TWO_BUTTON;
            }

            this.button_editing_prevRecord.Enabled = true;
            this.button_editing_nextRecord.Enabled = true;

            if (nIndex == 0)
            {
                this.button_editing_prevRecord.Enabled = false;
            }

            if (nIndex >= this.OrderControl.CountOfVisibleItems() - 1)
            {
                this.button_editing_nextRecord.Enabled = false;
            }

            return;
        DISABLE_TWO_BUTTON:
            this.button_editing_prevRecord.Enabled = false;
            this.button_editing_nextRecord.Enabled = false;
            return;
        }

        OrderItem GetPrevOrNextOrderItem(bool bPrev,
    out string strError)
        {
            strError = "";

            if (this.OrderControl == null)
            {
                strError = "没有容器";
                goto ERROR1;
            }

            int nIndex = this.OrderControl.IndexOfVisibleItems(this.OrderItem);
            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                strError = "OrderItem事项居然在容器中没有找到。";
                Debug.Assert(false, strError);
                goto ERROR1;
            }

            if (bPrev == true)
                nIndex--;
            else
                nIndex++;

            if (nIndex <= -1)
            {
                strError = "到头";
                goto ERROR1;
            }

            if (nIndex >= this.OrderControl.CountOfVisibleItems())
            {
                strError = "到尾";
                goto ERROR1;
            }

            return this.OrderControl.GetVisibleItemAt(nIndex);
        ERROR1:
            return null;
        }

#endif

        private void OrderEditForm_Load(object sender, EventArgs e)
        {
#if NO
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            LoadOrderItem(this.OrderItem);
            EnablePrevNextRecordButtons();

            // 参考记录
            if (this.OrderItem != null
                && this.OrderItem.Error != null)
            {

                this.splitContainer_main.Panel1Collapsed = false;

                string strError = "";
                int nRet = FillExisting(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);


                this.orderEditControl_existing.SetReadOnly(ReadOnlyStyle.All);

                // 突出差异内容
                this.orderEditControl_editing.HighlightDifferences(this.orderEditControl_existing);

            }
            else
            {
                this.tableLayoutPanel_main.RowStyles[0].Height = 0F;
                this.textBox_message.Visible = false;

                this.label_editing.Visible = false;
                this.splitContainer_main.Panel1Collapsed = true;
                this.orderEditControl_existing.Enabled = false;
            }
#endif
        }

        private void OrderEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            this.orderEditControl_editing.GetValueTable -= new GetValueTableEventHandler(orderEditControl_editing_GetValueTable);
             * */
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

#if NO
        // 获取值列表时作为线索的数据库名
        /// <summary>
        /// 书目库名。
        /// 获取值列表时作为线索的数据库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return this.orderEditControl_editing.BiblioDbName;
            }
            set
            {
                this.orderEditControl_editing.BiblioDbName = value;
                this.orderEditControl_existing.BiblioDbName = value;
            }
        }
#endif
    }

    /// <summary>
    /// 订购记录编辑对话框的基础类
    /// </summary>
    public class OrderEditFormBase : ItemEditFormBase<OrderItem, OrderItemCollection>
    {
    }
}