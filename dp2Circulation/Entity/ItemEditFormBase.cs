using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 册/订购/期/评注 记录编辑对话框的基础类
    /// </summary>
    public class ItemEditFormBase<T, TC> : Form
        where T : BookItemBase, new()
        where TC : BookItemCollectionBase, new()
    {
        /// <summary>
        /// 是否允许“确定”按钮处于 Enabled 状态。如果为 false，表示要根据控件中内容是否修改来决定按钮的 Enabled 状态
        /// </summary>
        public bool EnableButtonOK = false;
        /// <summary>
        /// 起始事项
        /// </summary>
        public T StartItem = null;   // 最开始时的对象

        /// <summary>
        /// 当前事项
        /// </summary>
        public T Item = null;

        /// <summary>
        /// 事项集合
        /// </summary>
        public TC Items = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 订购控件
        /// </summary>
        public ItemControlBase<T, TC> ItemControl = null;

        // 为编辑目的的初始化
        // parameters:
        //      bookitems   容器。用于UndoMaskDelete
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="item">要编辑的订购事项</param>
        /// <param name="items">事项所在的集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int InitialForEdit(
            T item,
            TC items,
            out string strError)
        {
            strError = "";

            this.Item = item;
            this.Items = items;

            this.StartItem = item;

            return 0;
        }

        /// <summary>
        /// “确定”按钮
        /// </summary>
        public Button ButtonOK
        {
            get
            {
                return _button_OK;
            }
        }

        internal ItemEditControlBase _editing = null;
        internal ItemEditControlBase _existing = null;

        internal Label _label_editing = null;
        internal Button _button_editing_undoMaskDelete = null;
        internal object _button_editing_nextRecord = null;  // 2015/10/14 以前为 Button
        internal object _button_editing_prevRecord = null;

        internal CheckBox _checkBox_autoSearchDup = null;

        internal Button _button_OK = null;
        internal Button _button_Cancel = null;

        internal TextBox _textBox_message = null;
        internal SplitContainer _splitContainer_main = null;
        internal TableLayoutPanel _tableLayoutPanel_main = null;

        /// <summary>
        /// 是否要自动进行查重
        /// </summary>
        public bool AutoSearchDup
        {
            get
            {
                if (this._checkBox_autoSearchDup != null)
                    return this._checkBox_autoSearchDup.Checked;
                return
                    false;
            }
            set
            {
                if (this._checkBox_autoSearchDup != null)
                    this._checkBox_autoSearchDup.Checked = value;
            }
        }

        void LoadItem(T item)
        {
            if (item != null)
            {
                string strError = "";
                int nRet = FillEditing(item, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "LoadItem() 发生错误: " + strError);
                    return;
                }
            }

            if (this._editing != null)
            {
                if (item != null
                    && item.ItemDisplayState == ItemDisplayState.Deleted)
                {
                    // 已经标记删除的事项, 不能进行修改。但是可以观察
                    this._editing.SetReadOnly("all");
                    this._checkBox_autoSearchDup.Enabled = false;

                    this._button_editing_undoMaskDelete.Enabled = true;
                    this._button_editing_undoMaskDelete.Visible = true;
                }
                else
                {
                    this._editing.SetReadOnly("librarian");

                    this._button_editing_undoMaskDelete.Enabled = false;
                    this._button_editing_undoMaskDelete.Visible = false;
                }

                this._editing.GetValueTable -= new GetValueTableEventHandler(itemEditControl_editing_GetValueTable);
                this._editing.GetValueTable += new GetValueTableEventHandler(itemEditControl_editing_GetValueTable);
            }
            this.Item = item;

            SetOkButtonState();
        }

        void itemEditControl_editing_GetValueTable(object sender, GetValueTableEventArgs e)
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

        internal void SetOkButtonState()
        {
            if (this.EnableButtonOK == true && this._button_OK != null)
            {
                this._button_OK.Enabled = true;
                return;
            }

            if (this.EnableButtonOK == false && this._button_OK != null)
            {
                if (this.Item != this.StartItem)
                {
                    this._button_OK.Enabled = _editing.Changed;
                }
                else
                {
                    this._button_OK.Enabled = true;
                }
            }
        }



        // 结束一个事项的编辑
        // return:
        //      -1  出错
        //      0   没有必要做restore
        //      1   做了restore
        internal int FinishOneItem(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (_editing.Changed == false)
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
            nRet = FinishVerify(out strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = Restore(out strError);
            if (nRet == -1)
                goto ERROR1;

            return nRet;
        ERROR1:
            return -1;
        }

        // 填充编辑界面数据
        int FillEditing(T item,
            out string strError)
        {
            strError = "";

            if (item == null)
            {
                strError = "item 参数值为空";
                return -1;
            }

            string strXml = "";
            int nRet = item.BuildRecord(
                false,  // 不必验证 parent 成员
                out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this._editing.SetData(strXml,
                item.RecPath,
                item.Timestamp,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 填充参考编辑界面数据
        int FillExisting(out string strError)
        {
            strError = "";

            if (this.Item == null)
            {
                strError = "Item 为空";
                return -1;
            }

            if (this.Item.Error == null)
            {
                strError = "Item.Error为空";
                return -1;
            }

            this._textBox_message.Text = this.Item.ErrorInfo;

            int nRet = this._existing.SetData(this.Item.Error.OldRecord,
                this.Item.Error.OldRecPath, // NewRecPath
                this.Item.Error.OldTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 从界面中更新 item 中的数据
        // return:
        //      -1  error
        //      0   没有必要更新
        //      1   已经更新
        int Restore(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (_editing.Changed == false)
                return 0;

            if (this.Item == null)
            {
                strError = "Item为空";
                return -1;
            }

            nRet = RestoreVerify(out strError);
            if (nRet == -1)
                return -1;

            // 获得编辑后的数据
            try
            {

                this.Item.RecordDom = this._editing.DataDom;
            }
            catch (Exception ex)
            {
                strError = "获得数据时出错: " + ex.Message;
                return -1;
            }

            this.Item.Changed = true;
            if (this.Item.ItemDisplayState != ItemDisplayState.New)
            {
                this.Item.ItemDisplayState = ItemDisplayState.Changed;
                // 这意味着Deleted状态也会被修改为Changed
            }

            this.Item.RefreshListView();
            return 1;
        }

        internal virtual int RestoreVerify(out string strError)
        {
            strError = "";
            return 0;
        }

        /// <summary>
        /// 结束前的校验
        /// </summary>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1: 出错; 0: 没有错误</returns>
        internal virtual int FinishVerify(out string strError)
        {
            strError = "";
            return 1;
        }

        internal void LoadPrevOrNextItem(bool bPrev)
        {
            string strError = "";

            T new_item = GetPrevOrNextItem(bPrev,
                out strError);
            if (new_item == null)
                goto ERROR1;

            // 保存当前事项
            int nRet = FinishOneItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            LoadItem(new_item);

            // 在listview中滚动到可见范围
            new_item.HilightListViewItem(true);
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
            this._button_Cancel.Enabled = bEnable;

            if (bEnable == true)
                SetOkButtonState();
            else
                this._button_OK.Enabled = false;


            if (bEnable == false)
            {
                // this._button_editing_nextRecord.Enabled = bEnable;
                // this._button_editing_prevRecord.Enabled = bEnable;
                OnButtonEnabledChanged(this._button_editing_nextRecord, bEnable);
                OnButtonEnabledChanged(this._button_editing_prevRecord, bEnable);
            }
            else
                this.EnablePrevNextRecordButtons();
        }

        // 如果 button 对象不是 已知 类型，则派生类需要重载这个函数，实现按钮 Enabled 状态的变化
        public virtual void OnButtonEnabledChanged(object button, bool bEnabled)
        {
            if (button is Button)
                ((Button)button).Enabled = bEnabled;
            else if (button is ToolStripButton)
                ((ToolStripButton)button).Enabled = bEnabled;
            else
                throw new Exception("需要重载函数 OnButtonEnabledChanged()，处理类型 " + button.GetType().ToString());
        }

        // 根据当前bookitem事项在容器中的位置，设置PrevRecord和NextRecord按钮的Enabled状态
        internal void EnablePrevNextRecordButtons()
        {
            // 有参考记录的情况
            if (this.Item != null
                && this.Item.Error != null)
            {
                goto DISABLE_TWO_BUTTON;
            }


            if (this.ItemControl == null)
            {
                // 因为没有容器，所以无法prev/next，于是就diable
                goto DISABLE_TWO_BUTTON;
            }

            int nIndex = 0;

            nIndex = this.ItemControl.IndexOfVisibleItems(this.Item);

            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                // Debug.Assert(false, "BookItem事项居然在容器中没有找到。");
                goto DISABLE_TWO_BUTTON;
            }

            //this._button_editing_prevRecord.Enabled = true;
            //this._button_editing_nextRecord.Enabled = true;
            OnButtonEnabledChanged(this._button_editing_prevRecord, true);
            OnButtonEnabledChanged(this._button_editing_nextRecord, true);

            if (nIndex == 0)
            {
                // this._button_editing_prevRecord.Enabled = false;
                OnButtonEnabledChanged(this._button_editing_prevRecord, false);
            }

            if (nIndex >= this.ItemControl.CountOfVisibleItems() - 1)
            {
                //this._button_editing_nextRecord.Enabled = false;
                OnButtonEnabledChanged(this._button_editing_nextRecord, false);
            }

            return;
        DISABLE_TWO_BUTTON:
            //this._button_editing_prevRecord.Enabled = false;
            //this._button_editing_nextRecord.Enabled = false;
            OnButtonEnabledChanged(this._button_editing_prevRecord, false);
            OnButtonEnabledChanged(this._button_editing_nextRecord, false);
            return;
        }

        internal T GetPrevOrNextItem(bool bPrev,
    out string strError)
        {
            strError = "";

            if (this.ItemControl == null)
            {
                strError = "没有容器";
                goto ERROR1;
            }

            int nIndex = this.ItemControl.IndexOfVisibleItems(this.Item);
            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                strError = "Item 事项居然在容器中没有找到。";
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

            if (nIndex >= this.ItemControl.CountOfVisibleItems())
            {
                strError = "到尾";
                goto ERROR1;
            }

            return this.ItemControl.GetVisibleItemAt(nIndex);
        ERROR1:
            return null;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ItemEditFormBase()
        {
            InitializeComponent();

        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ItemEditFormBase
            // 
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.Name = "ItemEditFormBase";
            this.ResumeLayout(false);

        }

        // 摘要:
        //     引发 System.Windows.Forms.Form.Load 事件。
        //
        // 参数:
        //   e:
        //     一个包含事件数据的 System.EventArgs。
        /// <summary>
        /// 引发 System.Windows.Forms.Form.Load 事件。
        /// </summary>
        /// <param name="e">一个包含事件数据的 System.EventArgs</param>
        protected override void OnLoad(EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
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
            base.OnLoad(e);
        }

        // 摘要:
        //     引发 System.Windows.Forms.Form.FormClosed 事件。
        //
        // 参数:
        //   e:
        //     一个 System.Windows.Forms.FormClosedEventArgs，其中包含事件数据。
        /// <summary>
        /// 引发 System.Windows.Forms.Form.FormClosed 事件。
        /// </summary>
        /// <param name="e">一个 System.Windows.Forms.FormClosedEventArgs，其中包含事件数据</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            this._editing.GetValueTable -= new GetValueTableEventHandler(itemEditControl_editing_GetValueTable);
        }

        // 获取值列表时作为线索的数据库名
        /// <summary>
        /// 书目库名。
        /// 获取值列表时作为线索的数据库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                if (this._editing != null)
                    return this._editing.BiblioDbName;

                return "";
            }
            set
            {
                if (this._editing != null)
                {
                    this._editing.BiblioDbName = value;
                    this._existing.BiblioDbName = value;
                }
            }
        }

        internal void OnButton_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            this.Enabled = false;

            nRet = this.FinishOneItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            // TODO: 提交保存后timestamp不匹配时出现的对话框，应当禁止prev/next按钮

            // 针对有报错信息的情况
            if (this.Item != null
                && this.Item.Error != null
                && this.Item.Error.ErrorCode == DigitalPlatform.LibraryClient.localhost.ErrorCodeValue.TimestampMismatch)
            {
                this.Item.OldRecord = this.Item.Error.OldRecord;
                this.Item.Timestamp = this.Item.Error.OldTimestamp;
            }

            this.Item.Error = null; // 结束报错状态

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            this.Enabled = true;
        }

    }
}
