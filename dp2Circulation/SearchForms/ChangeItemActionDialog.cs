using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;

namespace dp2Circulation
{
    /// <summary>
    /// 快速修改 册/订购/期看/评注/读者 记录 -- 指定动作参数对话框
    /// </summary>
    internal partial class ChangeItemActionDialog : Form
    {
        public string ElementCaption = "字段";    // 字段/元素/子字段

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DbType = "item";  // item / order / comment / issue / patron

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        // public event GetValueTableEventHandler GetValueTable = null;
        public string RefDbName = "";

        public XmlDocument CfgDom = null;

        // OneActionCfg _stateActionCfg = null;   // 当前 <state> 字段对应的配置参数

        public ChangeItemActionDialog()
        {
            InitializeComponent();
        }

        private void ChangeItemActionDialog_Load(object sender, EventArgs e)
        {
#if NO
            // state
            this.changeStateActionControl1.ActionString = this.MainForm.AppInfo.GetString(
                "change_"+this.DbType+"_param",
                "state",
                "<不改变>");

            this.changeStateActionControl1.AddString = this.MainForm.AppInfo.GetString(
                "change_" + this.DbType + "_param",
                "state_add",
                "");
            this.changeStateActionControl1.RemoveString = this.MainForm.AppInfo.GetString(
                "change_" + this.DbType + "_param",
                "state_remove",
                "");
#endif
            // 设置第一栏的标题
            this.listView_actions.Columns[0].Text = this.ElementCaption + "名";

            // 恢复 listview 中的内容

            // string strCfgFileName = Path.Combine( this.MainForm.UserDir, this.DbType + "_change_actions.xml");
            string strCfgFileName = Path.Combine(this.MainForm.DataDir, "default\\" + this.DbType + "_change_actions.xml");
            string strError = "";
            int nRet = LoadCfgDom(strCfgFileName,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            listView_actions_SelectedIndexChanged(this, new EventArgs());
#if NO
            if (this.CfgDom != null)
            {
                this.comboBox_fieldName.Items.Add("<不使用>");
                OneActionDialog.FillFieldNameList("zh",
                    this.CfgDom,
                    this.GetUsedFieldNames("state"),
                    this.comboBox_fieldName);
                if (string.IsNullOrEmpty(this.comboBox_fieldName.Text) == true
                    && this.comboBox_fieldName.Items.Count > 0)
                    this.comboBox_fieldName.Text = (string)this.comboBox_fieldName.Items[0];
            }
#endif
        }

        int LoadCfgDom(string strFileName,
            out string strError)
        {
            strError = "";

            this.CfgDom = new XmlDocument();
            try
            {
                this.CfgDom.Load(strFileName);
            }
            catch(Exception ex)
            {
                strError = "装载文件 '"+strFileName+"' 时间发生错误: " + ex.Message;
                return -1;
            }

#if NO
            OneActionCfg cfg = null;

            // 根据语言相关的 caption 值或者 <> {} 形态的 element 名， 获取 <action> 配置参数
            // return:
            //      -1  出错
            //      0   没有找到定义
            //      1   找到定义
            int nRet = OneActionDialog.GetActionCfg(
                this.CfgDom,
                "{state}",
                out cfg,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            this._stateActionCfg = cfg;
#endif
            return 0;
        }

#if NO
        private void changeStateActionControl1_AddOrRemoveListDropDown(object sender, EventArgs e)
        {
            var combobox = sender as DigitalPlatform.CommonControl.CheckedComboBox;

            if (combobox.Items.Count > 0)
                return;

            FillStateDropDown(combobox);
        }

        int m_nInDropDown = 0;

        void FillStateDropDown(CheckedComboBox combobox)
        {
            // 防止重入
            if (this.m_nInDropDown > 0)
                return;

            string strTableName = "";
#if NO
            if (_stateActionCfg != null)
                strTableName = _stateActionCfg.List;
#endif

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                if (combobox.Items.Count <= 0
                    && this.GetValueTable != null)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.RefDbName;

                    e1.TableName = strTableName;
#if NO
                    if (this.DbType == "patron")
                        e1.TableName = "readerState";
                    else if (this.DbType == "item")
                        e1.TableName = "state";
                    else
                        e1.TableName = this.DbType + "State";
#endif

                    this.GetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        // combobox.Items.Add("{not found}");
                    }
                }
            }
            finally
            {
                this.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

#endif
        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.Actions.Count == 0)
            {
                strError = "当前尚未指定任何修改动作";
                goto ERROR1;
            }

#if NO
            // state
            this.MainForm.AppInfo.SetString(
                "change_" + this.DbType + "_param",
                "state",
                this.changeStateActionControl1.ActionString);

            this.MainForm.AppInfo.SetString(
                "change_" + this.DbType + "_param",
                "state_add",
                this.changeStateActionControl1.AddString);

            this.MainForm.AppInfo.SetString(
                "change_" + this.DbType + "_param",
                "state_remove",
                this.changeStateActionControl1.RemoveString );
#endif

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

#if NO
        private void changeStateActionControl1_ActionChanged(object sender, EventArgs e)
        {
            if (this.changeStateActionControl1.IsActionChange == false)
                this.label_state.BackColor = this.BackColor;
            else
                this.label_state.BackColor = Color.Green;
        }
#endif

        private void listView_actions_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("修改 (&M)");
            menuItem.Click += new System.EventHandler(this.menu_modify_Click);
            if (this.listView_actions.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新增 (&N)");
            menuItem.Click += new System.EventHandler(this.menu_new_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("全选 (&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除 (&D)");
            menuItem.Click += new System.EventHandler(this.menu_delete_Click);
            if (this.listView_actions.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("宏定义 (&M) ...");
            menuItem.Click += new System.EventHandler(this.menu_macroDef_Click);
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_actions, new Point(e.X, e.Y));	
        }

        // 宏定义
        void menu_macroDef_Click(object sender, EventArgs e)
        {
            MacroTableDialog dlg = new MacroTableDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.XmlFileName = Path.Combine(this.MainForm.UserDir, this.DbType + "_macrotable.xml");

            this.MainForm.AppInfo.LinkFormState(dlg, this.DbType + "_MacroTableDialog_state");
            dlg.ShowDialog(this);
            this.MainForm.AppInfo.UnlinkFormState(dlg);
            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;
        }

        // 获得当前 ListView 中(和外面)已经用过的字段名
        List<string> GetUsedFieldNames(string strStyle = "listview")    // state,listview
        {
            List<string> results = new List<string>();

            if (StringUtil.IsInList("state", strStyle) == true)
            {
                results.Add("{state}");
            }

            if (StringUtil.IsInList("listview", strStyle) == true)
            {
                foreach (ListViewItem item in this.listView_actions.Items)
                {
                    results.Add(ListViewUtil.GetItemText(item, 0));
                }
            }

            return results;
        }

        // 获得当前 ListView 中(和外面)的动作参数
        List<OneAction> GetActions()
        {
            List<OneAction> results = new List<OneAction>();

#if NO
            if (this.changeStateActionControl1.ActionString == "<不改变>")
            {
            }
            else if (this.changeStateActionControl1.ActionString == "<增、减>")
            {
                if (string.IsNullOrEmpty(this.changeStateActionControl1.AddString) == false)
                {
                    OneAction action = new OneAction();
                    action.FieldName = "{state}";
                    action.Action = "add";
                    action.FieldValue = this.changeStateActionControl1.AddString;
                    results.Add(action);
                }
                if (string.IsNullOrEmpty(this.changeStateActionControl1.RemoveString) == false)
                {
                    OneAction action = new OneAction();
                    action.FieldName = "{state}";
                    action.Action = "remove";
                    action.FieldValue = this.changeStateActionControl1.RemoveString;
                    results.Add(action);
                }
            }
            else
            {
                OneAction action = new OneAction();
                action.FieldName = "{state}";
                action.FieldValue = this.changeStateActionControl1.ActionString;
                results.Add(action);
            }
#endif

            foreach (ListViewItem item in this.listView_actions.Items)
            {
#if NO
                OneAction action = new OneAction();
                action.FieldName = ListViewUtil.GetItemText(item, 0);
                action.FieldValue = ListViewUtil.GetItemText(item, 1);
                action.Additional = ListViewUtil.GetItemText(item, 2);

                results.Add(action);
#endif
                string strFieldName = ListViewUtil.GetItemText(item, 0);

                string strFieldValue = ListViewUtil.GetItemText(item, 1);
                string strAdd = ListViewUtil.GetItemText(item, 2);
                string strRemove = ListViewUtil.GetItemText(item, 3);
                string strAdditional = ListViewUtil.GetItemText(item, 4);

                if (strFieldValue == "<不改变>")
                {
                }
                else if (strFieldValue == "<删除>")
                {
                    OneAction action = new OneAction();
                    action.FieldName = strFieldName;
                    action.Action = "delete";
                    action.FieldValue = "";
                    results.Add(action);
                }
                else if (strFieldValue == "<增、减>")
                {
                    if (string.IsNullOrEmpty(strAdd) == false)
                    {
                        OneAction action = new OneAction();
                        action.FieldName = strFieldName;
                        action.Action = "add";
                        action.FieldValue = strAdd;
                        results.Add(action);
                    }
                    if (string.IsNullOrEmpty(strRemove) == false)
                    {
                        OneAction action = new OneAction();
                        action.FieldName = strFieldName;
                        action.Action = "remove";
                        action.FieldValue = strRemove;
                        results.Add(action);
                    }
                }
                else
                {
                    OneAction action = new OneAction();
                    action.FieldName = strFieldName;
                    action.FieldValue = strFieldValue;
                    results.Add(action);
                }
            }

            return results;
        }

        public List<OneAction> Actions
        {
            get
            {
                return this.GetActions();
            }
        }

        void menu_new_Click(object sender, EventArgs e)
        {
            OneActionDialog dlg = new OneActionDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ElementCaption = this.ElementCaption;
            dlg.UserDir = this.MainForm.UserDir;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.CfgDom = this.CfgDom;
            dlg.UsedFieldNames = GetUsedFieldNames();
            dlg.RefDbName = this.RefDbName;
            // dlg.AddOrRemoveListDropDown += dlg_AddOrRemoveListDropDown;
            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;
            int index = -1;

            if (this.listView_actions.SelectedIndices.Count > 0)
                index = this.listView_actions.SelectedIndices[0] + 1;
            else
                index = this.listView_actions.Items.Count;  // 追加

            ListViewItem item = new ListViewItem();
            ListViewUtil.ChangeItemText(item, 0, dlg.FieldName);
            ListViewUtil.ChangeItemText(item, 1, dlg.FieldValue);
            ListViewUtil.ChangeItemText(item, 2, dlg.FieldValueAdd); 
            ListViewUtil.ChangeItemText(item, 3, dlg.FieldValueRemove);
            ListViewUtil.ChangeItemText(item, 4, dlg.Additional);

            this.listView_actions.Items.Insert(index, item);
            ListViewUtil.SelectLine(item, true);
        }

#if NO
        void dlg_AddOrRemoveListDropDown(object sender, EventArgs e)
        {
            var combobox = sender as DigitalPlatform.CommonControl.CheckedComboBox;

            if (combobox.Items.Count > 0)
                return;

            FillStateDropDown(combobox);
        }
#endif

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            ListViewUtil.SelectAllLines(this.listView_actions);
        }

        void menu_modify_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_actions.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要修改的行";
                goto ERROR1;
            }

            ListViewItem item = this.listView_actions.SelectedItems[0];

            List<string> used_fieldnames = GetUsedFieldNames();
            used_fieldnames.Remove(ListViewUtil.GetItemText(item, 0));  // 去掉当前事项已经使用的名字

            OneActionDialog dlg = new OneActionDialog();

            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.ElementCaption = this.ElementCaption;
            dlg.UserDir = this.MainForm.UserDir;
            dlg.DataDir = this.MainForm.DataDir;
            dlg.CfgDom = this.CfgDom;
            dlg.UsedFieldNames = used_fieldnames;
            dlg.FieldName = ListViewUtil.GetItemText(item, 0);
            dlg.FieldValue = ListViewUtil.GetItemText(item, 1);
            dlg.FieldValueAdd = ListViewUtil.GetItemText(item, 2);
            dlg.FieldValueRemove = ListViewUtil.GetItemText(item, 3);

            // dlg.Additional = ListViewUtil.GetItemText(item, 4);

            dlg.RefDbName = this.RefDbName;
            // dlg.AddOrRemoveListDropDown += dlg_AddOrRemoveListDropDown;
            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            ListViewUtil.ChangeItemText(item, 0, dlg.FieldName);
            ListViewUtil.ChangeItemText(item, 1, dlg.FieldValue);
            ListViewUtil.ChangeItemText(item, 2, dlg.FieldValueAdd);
            ListViewUtil.ChangeItemText(item, 3, dlg.FieldValueRemove);
            ListViewUtil.ChangeItemText(item, 4, dlg.Additional);

            ListViewUtil.SelectLine(item, true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
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

        void menu_delete_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_actions.SelectedIndices.Count == 0)
            {
                strError = "尚未选定要删除的行";
                goto ERROR1;
            }

            string strText = "确实要删除所选定的 " + this.listView_actions.SelectedItems.Count.ToString() + " 个行?";

            DialogResult result = MessageBox.Show(this,
    strText,
    "ChangeItemActionDialog",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            ListViewUtil.DeleteSelectedItems(this.listView_actions);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void listView_actions_DoubleClick(object sender, EventArgs e)
        {
            menu_modify_Click(sender, e);
        }

        private void toolStripButton_new_Click(object sender, EventArgs e)
        {
            menu_new_Click(sender, e);
        }

        private void toolStripButton_modify_Click(object sender, EventArgs e)
        {
            menu_modify_Click(sender, e);
        }

        private void toolStripButton_delete_Click(object sender, EventArgs e)
        {
            menu_delete_Click(sender, e);
        }

        private void listView_actions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_actions.SelectedItems.Count > 0)
            {
                this.toolStripButton_delete.Enabled = true;
                this.toolStripButton_modify.Enabled = true;
            }
            else
            {
                this.toolStripButton_delete.Enabled = false;
                this.toolStripButton_modify.Enabled = false;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_actions);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.listView_actions);
                GuiState.SetUiState(controls, value);
            }
        }

    }

    internal class OneAction
    {
        public string FieldName = "";
        public string FieldValue = "";
        public string Action = "";  // 如果为空，表示替换为；如果为 add / remove，表示增减部分
        public string Additional = "";  // 附加的特性参数
    }

    // 一个 <action> 配置事项
    internal class OneActionCfg
    {
        /// <summary>
        /// element 属性值
        /// </summary>
        public string Element = "";

        /// <summary>
        /// type 属性值
        /// </summary>
        public string Type = "";

        /// <summary>
        /// list 属性值。表示从服务器获取 list 时的 name 参数
        /// </summary>
        public string List = "";

        /// <summary>
        /// 附加的信息
        /// </summary>
        public string Additional = "";

        public string Style = "";   // removable

        /// <summary>
        /// 值的长度
        /// </summary>
        public string Length = "";  // 如果为空，表示无所谓。如果为 "1"，表示长度恒定为 1 字符
    }
}
