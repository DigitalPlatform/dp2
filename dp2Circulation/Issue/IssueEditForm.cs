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

namespace dp2Circulation
{
    /// <summary>
    /// 期记录编辑对话框
    /// </summary>
    public partial class IssueEditForm : IssueEditFormBase
        // ItemEditFormBase<IssueItem, IssueItemCollection>
    {
#if NO
        /// <summary>
        /// 起始事项
        /// </summary>
        public IssueItem StartIssueItem = null;   // 最开始时的对象

        /// <summary>
        /// 当前事项
        /// </summary>
        public IssueItem IssueItem = null;

        /// <summary>
        /// 事项集合
        /// </summary>
        public IssueItemCollection IssueItems = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 期控件
        /// </summary>
        public IssueControl IssueControl = null;
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public IssueEditForm()
        {
            InitializeComponent();

            _editing = this.issueEditControl_editing;
            _existing = this.issueEditControl_existing;

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
        //      issueitems   容器。用于UndoMaskDelete
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="issueitem">要编辑的期事项</param>
        /// <param name="issueitems">事项所在的集合</param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int InitialForEdit(
            IssueItem issueitem,
            IssueItemCollection issueitems,
            out string strError)
        {
            strError = "";

            this.IssueItem = issueitem;
            this.IssueItems = issueitems;

            this.StartIssueItem = issueitem;

            return 0;
        }
#endif

        private void IssueEditForm_Load(object sender, EventArgs e)
        {
#if NO
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            LoadIssueItem(this.IssueItem);
            EnablePrevNextRecordButtons();

            // 参考记录
            if (this.IssueItem != null
                && this.IssueItem.Error != null
                && string.IsNullOrEmpty(this.IssueItem.Error.OldRecord) == false)
            {

                this.splitContainer_main.Panel1Collapsed = false;

                string strError = "";
                int nRet = FillExisting(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);


                this.issueEditControl_existing.SetReadOnly(ReadOnlyStyle.All);

                // 突出差异内容
                this.issueEditControl_editing.HighlightDifferences(this.issueEditControl_existing);

            }
            else
            {
                this.tableLayoutPanel_main.RowStyles[0].Height = 0F;
                this.textBox_message.Visible = false;

                this.label_editing.Visible = false;
                this.splitContainer_main.Panel1Collapsed = true;
                this.issueEditControl_existing.Enabled = false;
            }
#endif
        }

        private void IssueEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            this.issueEditControl_editing.GetValueTable -= new GetValueTableEventHandler(issueEditControl_editing_GetValueTable);
#endif
        }

#if NO
        void LoadIssueItem(IssueItem issueitem)
        {
            if (issueitem != null)
            {
                string strError = "";
                int nRet = FillEditing(issueitem, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "LoadIssueItem() 发生错误: " + strError);
                    return;
                }
            }
            if (issueitem != null
                && issueitem.ItemDisplayState == ItemDisplayState.Deleted)
            {
                // 已经标记删除的事项, 不能进行修改。但是可以观察
                this.issueEditControl_editing.SetReadOnly(ReadOnlyStyle.All);
                this.checkBox_autoSearchDup.Enabled = false;

                this.button_editing_undoMaskDelete.Enabled = true;
                this.button_editing_undoMaskDelete.Visible = true;
            }
            else
            {
                this.issueEditControl_editing.SetReadOnly(ReadOnlyStyle.Librarian);

                this.button_editing_undoMaskDelete.Enabled = false;
                this.button_editing_undoMaskDelete.Visible = false;
            }

            this.issueEditControl_editing.GetValueTable -= new GetValueTableEventHandler(issueEditControl_editing_GetValueTable);
            this.issueEditControl_editing.GetValueTable += new GetValueTableEventHandler(issueEditControl_editing_GetValueTable);

            this.IssueItem = issueitem;

            SetOkButtonState();
        }

        void issueEditControl_editing_GetValueTable(object sender, GetValueTableEventArgs e)
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
            if (this.IssueItem != this.StartIssueItem)
            {
                this.button_OK.Enabled = issueEditControl_editing.Changed;
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
        int FinishOneIssueItem(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (issueEditControl_editing.Changed == false)
                return 0;

            string strPublishTime = this.issueEditControl_editing.PublishTime;

            // TODOL 检查出版时间形式是否合法
            if (String.IsNullOrEmpty(strPublishTime) == true)
            {
                strError = "出版时间不能为空";
                goto ERROR1;
            }

            nRet = Restore(out strError);
            if (nRet == -1)
                goto ERROR1;

            return nRet;
        ERROR1:
            return -1;
        }
#endif
        internal override int FinishVerify(out string strError)
        {
            strError = "";
            int nRet = 0;

            string strPublishTime = this.issueEditControl_editing.PublishTime;

            // TODOL 检查出版时间形式是否合法
            if (String.IsNullOrEmpty(strPublishTime) == true)
            {
                strError = "出版时间不能为空";
                return -1;
            }

            // 2014/10/23
            if (string.IsNullOrEmpty(this.issueEditControl_editing.PublishTime) == false)
            {
                // 检查单个出版日期字符串是否合法
                // return:
                //      -1  出错
                //      0   正确
                nRet = LibraryServerUtil.CheckSinglePublishTime(this.issueEditControl_editing.PublishTime,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = 0;


            nRet = this.FinishOneIssueItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            // TODO: 提交保存后timestamp不匹配时出现的对话框，应当禁止prev/next按钮

            // 针对有报错信息的情况
            if (this.IssueItem != null
                && this.IssueItem.Error != null
                && this.IssueItem.Error.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCodeValue.TimestampMismatch)
            {
                this.IssueItem.OldRecord = this.IssueItem.Error.OldRecord;
                this.IssueItem.Timestamp = this.IssueItem.Error.OldTimestamp;
            }

            this.IssueItem.Error = null; // 结束报错状态

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

#if NO
        // 填充编辑界面数据
        int FillEditing(IssueItem issueitem,
            out string strError)
        {
            strError = "";

            if (issueitem == null)
            {
                strError = "issueitem参数值为空";
                return -1;
            }

            string strXml = "";
            int nRet = issueitem.BuildRecord(out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.issueEditControl_editing.SetData(strXml,
                issueitem.RecPath,
                issueitem.Timestamp,
                out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }

        // 填充参考编辑界面数据
        int FillExisting(out string strError)
        {
            strError = "";

            if (this.IssueItem == null)
            {
                strError = "IssueItem为空";
                return -1;
            }

            if (this.IssueItem.Error == null)
            {
                strError = "IssueItem.Error为空";
                return -1;
            }

            this.textBox_message.Text = this.IssueItem.ErrorInfo;

            int nRet = this.issueEditControl_existing.SetData(this.IssueItem.Error.OldRecord,
                this.IssueItem.Error.OldRecPath, // NewRecPath
                this.IssueItem.Error.OldTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 从界面中更新issueitem中的数据
        // return:
        //      -1  error
        //      0   没有必要更新
        //      1   已经更新
        int Restore(out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (issueEditControl_editing.Changed == false)
                return 0;

            if (this.IssueItem == null)
            {
                strError = "IssueItem为空";
                return -1;
            }


            // TODO: 是否当这个checkbox为false的时候，至少也要检查本种之类的重复情形？
            // 如果这里不检查，可否在提交保存的时候，先查完本种之类的重复，才真正向服务器提交?
            if (this.checkBox_autoSearchDup.Checked == true
                && this.IssueControl != null)
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
                this.IssueItem.RecordDom = this.issueEditControl_editing.DataDom;
            }
            catch (Exception ex)
            {
                strError = "获得数据时出错: " + ex.Message;
                return -1;
            }

            this.IssueItem.Changed = true;
            if (this.IssueItem.ItemDisplayState != ItemDisplayState.New)
            {
                this.IssueItem.ItemDisplayState = ItemDisplayState.Changed;
                // 这意味着Deleted状态也会被修改为Changed
            }

            this.IssueItem.RefreshListView();

            return 1;
        }


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
#endif

        private void button_editing_undoMaskDelete_Click(object sender, EventArgs e)
        {
            if (this.Items != null)
            {
                this.Items.UndoMaskDeleteItem(this.Item);
                this.issueEditControl_editing.SetReadOnly("librarian");
                this.checkBox_autoSearchDup.Enabled = true;
                // this.button_OK.Enabled = entityEditControl_editing.Changed;
            }
        }

#if NO
        void LoadPrevOrNextIssueItem(bool bPrev)
        {
            string strError = "";

            IssueItem new_issueitem = GetPrevOrNextIssueItem(bPrev,
                out strError);
            if (new_issueitem == null)
                goto ERROR1;

            // 保存当前事项
            int nRet = FinishOneIssueItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            LoadIssueItem(new_issueitem);

            // 在listview中滚动到可见范围
            new_issueitem.HilightListViewItem(true);
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
#endif

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

#if NO
        // 根据当前bookitem事项在容器中的位置，设置PrevRecord和NextRecord按钮的Enabled状态
        void EnablePrevNextRecordButtons()
        {
            // 有参考记录的情况
            if (this.IssueItem != null
                && this.IssueItem.Error != null)
            {
                goto DISABLE_TWO_BUTTON;
            }


            if (this.IssueControl == null)
            {
                // 因为没有容器，所以无法prev/next，于是就diable
                goto DISABLE_TWO_BUTTON;
            }

            int nIndex = 0;

            nIndex = this.IssueControl.IndexOfVisibleItems(this.IssueItem);

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

            if (nIndex >= this.IssueControl.CountOfVisibleItems() - 1)
            {
                this.button_editing_nextRecord.Enabled = false;
            }

            return;
        DISABLE_TWO_BUTTON:
            this.button_editing_prevRecord.Enabled = false;
            this.button_editing_nextRecord.Enabled = false;
            return;
        }
#endif

        private void issueEditControl_editing_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            SetOkButtonState();
        }

        private void issueEditControl_editing_ControlKeyDown(object sender, ControlKeyEventArgs e)
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
            IssueItem issueitem = GetPrevOrNextItem(bUp, out strError);
            if (issueitem == null)
                return;
            switch (e.Name)
            {
                case "PublishTime":
                    this.issueEditControl_editing.PublishTime = issueitem.PublishTime;
                    break;
                case "State":
                    this.issueEditControl_editing.State = issueitem.State;
                    break;
                case "Issue":
                    this.issueEditControl_editing.Issue = issueitem.Issue;
                    break;
                case "Zong":
                    this.issueEditControl_editing.Zong = issueitem.Zong;
                    break;
                case "Volume":
                    this.issueEditControl_editing.Volume = issueitem.Volume;
                    break;
                case "OrderInfo":
                    this.issueEditControl_editing.OrderInfo = issueitem.OrderInfo;
                    break;
                case "Comment":
                    this.issueEditControl_editing.Comment = issueitem.Comment;
                    break;
                case "BatchNo":
                    this.issueEditControl_editing.BatchNo = issueitem.BatchNo;
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
        public string BiblioDbName
        {
            get
            {
                return this.issueEditControl_editing.BiblioDbName;
            }
            set
            {
                this.issueEditControl_editing.BiblioDbName = value;
                this.issueEditControl_existing.BiblioDbName = value;
            }
        }

        IssueItem GetPrevOrNextIssueItem(bool bPrev,
            out string strError)
        {
            strError = "";

            if (this.IssueControl == null)
            {
                strError = "没有容器";
                goto ERROR1;
            }

            int nIndex = this.IssueControl.IndexOfVisibleItems(this.IssueItem);
            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                strError = "IssueItem事项居然在容器中没有找到。";
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

            if (nIndex >= this.IssueControl.CountOfVisibleItems())
            {
                strError = "到尾";
                goto ERROR1;
            }

            return this.IssueControl.GetVisibleItemAt(nIndex);
        ERROR1:
            return null;
        }
#endif
    }

    /// <summary>
    /// 期记录编辑对话框的基础类
    /// </summary>
    public class IssueEditFormBase : ItemEditFormBase<IssueItem, IssueItemCollection>
    {
    }
}