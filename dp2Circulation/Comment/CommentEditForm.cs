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

namespace dp2Circulation
{
    /// <summary>
    /// 评注记录编辑对话框
    /// </summary>
    public partial class CommentEditForm : CommentEditFormBase
        // ItemEditFormBase<CommentItem, CommentItemCollection>
    {
#if NO
        public CommentItem StartCommentItem = null;   // 最开始时的对象

        public CommentItem CommentItem = null;

        public CommentItemCollection CommentItems = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        public CommentControl CommentControl = null;
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommentEditForm()
        {
            InitializeComponent();

            _editing = this.commentEditControl_editing;
            _existing = this.commentEditControl_existing;

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
        //      commentitems   容器。用于UndoMaskDelete
        public int InitialForEdit(
            CommentItem commentitem,
            CommentItemCollection commentitems,
            out string strError)
        {
            strError = "";

            this.CommentItem = commentitem;
            this.CommentItems = commentitems;

            this.StartCommentItem = commentitem;

            return 0;
        }

        void LoadCommentItem(CommentItem commentitem)
        {
            if (commentitem != null)
            {
                string strError = "";
                int nRet = FillEditing(commentitem, out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "LoadCommentItem() 发生错误: " + strError);
                    return;
                }
            }
            if (commentitem != null
                && commentitem.ItemDisplayState == ItemDisplayState.Deleted)
            {
                // 已经标记删除的事项, 不能进行修改。但是可以观察
                this.commentEditControl_editing.SetReadOnly(ReadOnlyStyle.All);
                this.checkBox_autoSearchDup.Enabled = false;

                this.button_editing_undoMaskDelete.Enabled = true;
                this.button_editing_undoMaskDelete.Visible = true;
            }
            else
            {
                this.commentEditControl_editing.SetReadOnly(ReadOnlyStyle.Librarian);

                this.button_editing_undoMaskDelete.Enabled = false;
                this.button_editing_undoMaskDelete.Visible = false;
            }

            this.commentEditControl_editing.GetValueTable -= new GetValueTableEventHandler(commentEditControl_editing_GetValueTable);
            this.commentEditControl_editing.GetValueTable += new GetValueTableEventHandler(commentEditControl_editing_GetValueTable);

            this.CommentItem = commentitem;

            SetOkButtonState();
        }

        private void commentEditControl_editing_GetValueTable(object sender, GetValueTableEventArgs e)
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
            if (this.CommentItem != this.StartCommentItem)
            {
                this.button_OK.Enabled = commentEditControl_editing.Changed;
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
        int FinishOneCommentItem(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (commentEditControl_editing.Changed == false)
                return 0;

#if NO
            string strIndex = this.commentEditControl_editing.Index;

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
        int FillEditing(CommentItem commentitem,
            out string strError)
        {
            strError = "";

            if (commentitem == null)
            {
                strError = "commentitem参数值为空";
                return -1;
            }

            string strXml = "";
            int nRet = commentitem.BuildRecord(out strXml,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.commentEditControl_editing.SetData(strXml,
                commentitem.RecPath,
                commentitem.Timestamp,
                out strError);
            if (nRet == -1)
                return -1;


            return 0;
        }

        // 填充参考编辑界面数据
        int FillExisting(out string strError)
        {
            strError = "";

            if (this.CommentItem == null)
            {
                strError = "CommentItem为空";
                return -1;
            }

            if (this.CommentItem.Error == null)
            {
                strError = "CommentItem.Error为空";
                return -1;
            }

            this.textBox_message.Text = this.CommentItem.ErrorInfo;

            int nRet = this.commentEditControl_existing.SetData(this.CommentItem.Error.OldRecord,
                this.CommentItem.Error.OldRecPath, // NewRecPath
                this.CommentItem.Error.OldTimestamp,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 从界面中更新Commtentitem中的数据
        // return:
        //      -1  error
        //      0   没有必要更新
        //      1   已经更新
        int Restore(out string strError)
        {
            strError = "";
            // int nRet = 0;

            if (commentEditControl_editing.Changed == false)
                return 0;

            if (this.CommentItem == null)
            {
                strError = "CommentItem为空";
                return -1;
            }


            // TODO: 是否当这个checkbox为false的时候，至少也要检查本种之类的重复情形？
            // 如果这里不检查，可否在提交保存的时候，先查完本种之类的重复，才真正向服务器提交?
            if (this.checkBox_autoSearchDup.Checked == true
                && this.CommentControl != null)
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

                this.CommentItem.RecordDom = this.commentEditControl_editing.DataDom;
            }
            catch (Exception ex)
            {
                strError = "获得数据时出错: " + ex.Message;
                return -1;
            }

            this.CommentItem.Changed = true;
            if (this.CommentItem.ItemDisplayState != ItemDisplayState.New)
            {
                this.CommentItem.ItemDisplayState = ItemDisplayState.Changed;
                // 这意味着Deleted状态也会被修改为Changed
            }

            this.CommentItem.RefreshListView();

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

        void LoadPrevOrNextCommentItem(bool bPrev)
        {
            string strError = "";

            CommentItem new_commentitem = GetPrevOrNextCommentItem(bPrev,
                out strError);
            if (new_commentitem == null)
                goto ERROR1;

            // 保存当前事项
            int nRet = FinishOneCommentItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            LoadCommentItem(new_commentitem);

            // 在listview中滚动到可见范围
            new_commentitem.HilightListViewItem(true);
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
            if (this.CommentItem != null
                && this.CommentItem.Error != null)
            {
                goto DISABLE_TWO_BUTTON;
            }


            if (this.CommentControl == null)
            {
                // 因为没有容器，所以无法prev/next，于是就diable
                goto DISABLE_TWO_BUTTON;
            }

            int nIndex = 0;

            nIndex = this.CommentControl.IndexOfVisibleItems(this.CommentItem);

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

            if (nIndex >= this.CommentControl.CountOfVisibleItems() - 1)
            {
                this.button_editing_nextRecord.Enabled = false;
            }

            return;
        DISABLE_TWO_BUTTON:
            this.button_editing_prevRecord.Enabled = false;
            this.button_editing_nextRecord.Enabled = false;
            return;
        }

        CommentItem GetPrevOrNextCommentItem(bool bPrev,
    out string strError)
        {
            strError = "";

            if (this.CommentControl == null)
            {
                strError = "没有容器";
                goto ERROR1;
            }

            int nIndex = this.CommentControl.IndexOfVisibleItems(this.CommentItem);
            if (nIndex == -1)
            {
                // 居然在容器中没有找到
                strError = "CommentItem事项居然在容器中没有找到。";
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

            if (nIndex >= this.CommentControl.CountOfVisibleItems())
            {
                strError = "到尾";
                goto ERROR1;
            }

            return this.CommentControl.GetVisibleItemAt(nIndex);
        ERROR1:
            return null;
        }
#endif


        private void CommentEditForm_Load(object sender, EventArgs e)
        {
#if NO
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            LoadCommentItem(this.CommentItem);
            EnablePrevNextRecordButtons();

            // 参考记录
            if (this.CommentItem != null
                && this.CommentItem.Error != null)
            {

                this.splitContainer_main.Panel1Collapsed = false;

                string strError = "";
                int nRet = FillExisting(out strError);
                if (nRet == -1)
                    MessageBox.Show(this, strError);


                this.commentEditControl_existing.SetReadOnly(ReadOnlyStyle.All);

                // 突出差异内容
                this.commentEditControl_editing.HighlightDifferences(this.commentEditControl_existing);

            }
            else
            {
                this.tableLayoutPanel_main.RowStyles[0].Height = 0F;
                this.textBox_message.Visible = false;

                this.label_editing.Visible = false;
                this.splitContainer_main.Panel1Collapsed = true;
                this.commentEditControl_existing.Enabled = false;
            }
#endif
        }

        private void CommentEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
#if NO
            this.commentEditControl_editing.GetValueTable -= new GetValueTableEventHandler(commentEditControl_editing_GetValueTable);
        
#endif
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
#if NO
            string strError = "";
            int nRet = 0;


            nRet = this.FinishOneCommentItem(out strError);
            if (nRet == -1)
                goto ERROR1;

            // TODO: 提交保存后timestamp不匹配时出现的对话框，应当禁止prev/next按钮

            // 针对有报错信息的情况
            if (this.CommentItem != null
                && this.CommentItem.Error != null
                && this.CommentItem.Error.ErrorCode == DigitalPlatform.CirculationClient.localhost.ErrorCodeValue.TimestampMismatch)
            {
                this.CommentItem.OldRecord = this.CommentItem.Error.OldRecord;
                this.CommentItem.Timestamp = this.CommentItem.Error.OldTimestamp;
            }

            this.CommentItem.Error = null; // 结束报错状态

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

        private void button_editing_undoMaskDelete_Click(object sender, EventArgs e)
        {
            if (this.Items != null)
            {
                this.Items.UndoMaskDeleteItem(this.Item);
                this.commentEditControl_editing.SetReadOnly("librarian");
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

        private void commentEditControl_editing_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            SetOkButtonState();
        }

        private void commentEditControl_editing_ControlKeyDown(object sender, ControlKeyEventArgs e)
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
            CommentItem commentitem = GetPrevOrNextItem(bUp, out strError);
            if (commentitem == null)
                return;
            switch (e.Name)
            {
                case "Index":
                    this.commentEditControl_editing.Index = commentitem.Index;
                    break;
                case "State":
                    this.commentEditControl_editing.State = commentitem.State;
                    break;
                case "Type":
                    this.commentEditControl_editing.TypeString = commentitem.TypeString;
                    break;
                case "Title":
                    this.commentEditControl_editing.Title = commentitem.Title;
                    break;
                case "Author":
                    this.commentEditControl_editing.Creator = commentitem.Creator;
                    break;
                case "Subject":
                    this.commentEditControl_editing.Subject = commentitem.Subject;
                    break;
                case "Summary":
                    this.commentEditControl_editing.Summary = commentitem.Summary;
                    break;
                case "Content":
                    this.commentEditControl_editing.Content = commentitem.Content;
                    break;
                case "CreateTime":
                    this.commentEditControl_editing.CreateTime = commentitem.CreateTime;
                    break;
                case "LastModified":
                    this.commentEditControl_editing.LastModified = commentitem.LastModified;
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
                return this.commentEditControl_editing.BiblioDbName;
            }
            set
            {
                this.commentEditControl_editing.BiblioDbName = value;
                this.commentEditControl_existing.BiblioDbName = value;
            }
        }
#endif
    }

    /// <summary>
    /// 评注记录编辑对话框的基础类
    /// </summary>
    public class CommentEditFormBase : ItemEditFormBase<CommentItem, CommentItemCollection>
    {
    }
}