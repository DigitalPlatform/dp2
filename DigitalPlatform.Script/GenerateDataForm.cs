using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Script
{
    public partial class GenerateDataForm : Form
    {
        const int WM_SET_FOCUS = API.WM_USER + 209;

        public bool CloseWhenComplete = false;

        public object sender = null;
        public GenerateDataEventArgs e = null;

        public event TriggerActionEventHandler TriggerAction = null;
        public event RefreshMenuEventHandler SetMenu = null;

        /// <summary>
        /// 停靠
        /// </summary>
        public event DoDockEventHandler DoDockEvent = null;

        public event EventHandler MyFormClosed = null;

        public bool Docked = false;
        // public MainForm MainForm = null;

        public int SelectedIndex = -1;  // 所选择的事项在Actions数组中的下标。注意，不是表格行的行号，因为重新排序后可能顺序就对不上了
        public ScriptAction SelectedAction = null;

        ScriptActionCollection m_actions = null;

        public ScriptActionCollection Actions
        {
            get
            {
                return this.m_actions;
            }
            set
            {
                this.m_actions = value;

                if (value != null)
                {
                    FillList();
                }
                else
                {
                    this.ActionTable.Rows.Clear();
                }

                ActionTable_SelectionChanged(null, null);
            }
        }

        public TableLayoutPanel Table
        {
            get
            {
                return this.tableLayoutPanel_main;
            }
#if NO
            // 需要重新挂接this.ActionTable的事件，太复杂了
            set
            {
                // 删除 TableLayoutPanel 类型的
                foreach(Control control in this.Controls)
                {
                    if (control is TableLayoutPanel)
                    {
                        this.Controls.Remove(control);
                        break;
                    }
                }

                Control table = FindChildControl(this,
                    typeof(TableLayoutPanel));
                Control dptable = FindChildControl(table,
                    typeof(DpTable));
                if (table != null)
                    this.Controls.Remove(table);
                if (dptable != null)
                    this.Controls.Remove(dptable);

                if (value != null)
                {
                    this.Controls.Add(value);
                }

                this.tableLayoutPanel_main = value;
                this.ActionTable = (DpTable)FindChildControl(value,
                    typeof(DpTable));


            }
#endif
        }

        static Control FindChildControl(Control container,
            Type type)
        {
            if (container == null)
                return null;
            foreach(Control control in container.Controls)
            {
                if (control.GetType().Equals(type) == true)
                    return control;
            }

            return null;
        }

        public Font GetDefaultFont()
        {
            FontFamily family = null;
            try
            {
                family = new FontFamily("微软雅黑");
            }
            catch
            {
                return null;
            }
            float height = (float)9.0;
            if (this.Font != null)
                height = this.Font.SizeInPoints;
            return new Font(family, height, GraphicsUnit.Point);
        }

        public GenerateDataForm()
        {
            InitializeComponent();
        }

        private void GenerateDataForm_Load(object sender, EventArgs e)
        {
            if (this.Owner != null)
                this.Font = this.Owner.Font;
            else
            {
                Font default_font = GetDefaultFont();
                if (default_font != null)
                    this.Font = default_font;
            }

            // FillList();
            API.PostMessage(this.Handle, WM_SET_FOCUS, 0, 0);
        }

        void FillList()
        {
            this.ActionTable.Rows.Clear();
            if (m_actions == null)
                return;

            DpRow first_item = null;

            this.ActionTable.BeginUpdate();
            for (int i = 0; i < m_actions.Count; i++)
            {
                ScriptAction action = (ScriptAction)m_actions[i];

                DpRow item = new DpRow();
                if (action.Name == "-")
                    item.Style = DpRowStyle.Seperator;
                else
                {
                    DpCell cell = new DpCell(action.Name);
                    cell.Font = new Font(this.ActionTable.Font.FontFamily, 10, FontStyle.Bold, GraphicsUnit.Point);
                    item.Add(cell);

                    // 快捷键
                    cell = new DpCell();
                    if (action.ShortcutKey != (char)0)
                    {
                        cell.Text = new string(action.ShortcutKey, 1);
                        cell.Text = cell.Text.ToUpper();
                    }
                    item.Add(cell);

                    // 说明
                    item.Add(new DpCell(action.Comment));

                    // 入口函数
                    cell = new DpCell(action.ScriptEntry);
                    cell.ForeColor = SystemColors.GrayText;
                    cell.Font = new Font(this.ActionTable.Font.FontFamily, 8, GraphicsUnit.Point);
                    item.Add(cell);
                }

                if (action.Active == true)
                {
                    item.Selected = true;

                    // 2009/2/24 
                    if (first_item == null)
                        first_item = item;
                }

                item.Tag = action;
                this.ActionTable.Rows.Add(item);
            }

            if (first_item != null)
            {
                this.ActionTable.FocusedItem = first_item;
                first_item.EnsureVisible();
            }
            this.ActionTable.EndUpdate();
        }

        // TODO: 可否通过一个事件，在事件中让 EntityForm 通过 FloatingMessage 显示报错信息
        // 或者用 WebBrowser 控件显示报错信息
        public void DisplayError(string strError)
        {
            this.ActionTable.Rows.Clear();
            this.ActionTable.MaxTextHeight = 500;

            DpRow item = new DpRow();

            DpCell cell = new DpCell("");
            item.Add(cell);

            // 快捷键
            cell = new DpCell();
            item.Add(cell);

            // 说明
            cell = new DpCell(strError);
            // cell.Font = new Font(this.ActionTable.Font.FontFamily, this.ActionTable.Font.SizeInPoints * 2, FontStyle.Bold, GraphicsUnit.Point);
            item.Add(cell);

            item.Tag = new ScriptAction();
            this.ActionTable.Rows.Add(item);
        }

        int _processing = 0;    // 是否正在处理中

        void EnableControls(bool bEnable)
        {
            // this.ActionTable.Enabled = bEnable;
            if (bEnable == true)
                this.ActionTable.BackColor = SystemColors.Window;
            else
                this.ActionTable.BackColor = SystemColors.Control;

            int nCount = this.ActionTable.SelectedRows.Count;
            if (nCount == 0)
            {
                this.button_excute.Enabled = false;
            }
            else
            {
                this.button_excute.Enabled = bEnable;
            }

            this.checkBox_autoRun.Enabled = bEnable;
        }

        void BeginProcess()
        {
            EnableControls(false);
            this._processing++;
            Application.DoEvents();
        }

        void EndProcess()
        {
            this._processing--;
            EnableControls(true);
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            DoDock(true);
        }

        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏

            /*
            this.MainForm.CurrentGenerateDataControl = this.Table;
            if (bShowFixedPanel == true
                && this.MainForm.PanelFixedVisible == false)
                this.MainForm.PanelFixedVisible = true;

            this.Docked = true;
            this.Visible = false;
             * */
            if (this.DoDockEvent != null)
            {
                DoDockEventArgs e = new DoDockEventArgs();
                e.ShowFixedPanel = bShowFixedPanel;
                this.DoDockEvent(this, e);
            }
        }

        #region 防止控件泄露

        // 不会被自动 Dispose 的 子 Control，放在这里托管，避免内存泄漏
        List<Control> _freeControls = new List<Control>();

        public void AddFreeControl(Control control)
        {
            ControlExtention.AddFreeControl(_freeControls, control);
        }

        public void RemoveFreeControl(Control control)
        {
            ControlExtention.RemoveFreeControl(_freeControls, control);
        }

        public void DisposeFreeControls()
        {

            ControlExtention.DisposeFreeControls(_freeControls);
        }

        #endregion

        public int Count
        {
            get
            {
                return this.ActionTable.Rows.Count;
            }
        }

        public void Clear()
        {
            this.ActionTable.Rows.Clear();
        }

        public void TriggerMyFormClose()
        {
            if (this.MyFormClosed != null)
            {
                this.MyFormClosed(this, new EventArgs());
            }
        }

        public new void Close()
        {
            base.Close();
            TriggerMyFormClose();
            this.Table.Visible = false;   // 2015/8/17
        }

        private void ActionTable_DoubleClick(object sender, EventArgs e)
        {
            if (this._processing > 0)
            {
                MessageBox.Show(this, "正在处理中，请稍后再重新启动执行");
                return;
            }
            if (this.ActionTable.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "尚未选择事项...");
                return;
            }

            this.SelectedAction = (ScriptAction)this.ActionTable.SelectedRows[0].Tag;
            if (this.SelectedAction == null)
                return; // 一般是因为双击了错误信息行

            Debug.Assert(this.SelectedAction != null, "");

            this.SelectedIndex = this.Actions.IndexOf(this.SelectedAction);

            if (this.CloseWhenComplete == true)
                this.Close();

            if (this.SelectedAction != null
                && this.TriggerAction != null)
            {
                BeginProcess();
                try
                {
                    TriggerActionArgs e1 = new TriggerActionArgs();
                    e1.EntryName = this.SelectedAction.ScriptEntry;
                    e1.sender = this.sender;
                    e1.e = this.e;
                    this.TriggerAction(this, e1);
                }
                finally
                {
                    EndProcess();
                }
            }
        }

        public bool AutoRun
        {
            get
            {
                return this.checkBox_autoRun.Checked;
            }
            set
            {
                this.checkBox_autoRun.Checked = value;
            }
        }

        public bool TryAutoRun()
        {
            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                // 旁路
                return false;
            }
            
            // 自动执行
            if (this.checkBox_autoRun.Checked == true
                && this.ActionTable.SelectedRows.Count == 1)
            {
                ActionTable_DoubleClick(this, null);
                return true;
            }

            return false;
        }

        public void RefreshState()
        {
            if (this.SetMenu == null)
                return;

            RefreshMenuEventArgs e = new RefreshMenuEventArgs();
            e.Actions = this.Actions;
            e.sender = this.sender;
            e.e = this.e;

            this.SetMenu(this, e);

            DpRow first_selected_row = null;
            DpRow last_selected_row = null;

            for (int i=0;i<this.ActionTable.Rows.Count; i++)
            {
                DpRow row = this.ActionTable.Rows[i];

                if (row.Style == DpRowStyle.Seperator)
                    continue;

                ScriptAction action = (ScriptAction)row.Tag;
                if (action == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (this.Actions == null || this.Actions.IndexOf(action) == -1)
                {
                    row.Selected = false;
                    continue;
                }

                if (row.Count == 0)
                    continue;

                Debug.Assert(row.Count >= 4, "");

                // 刷新一行
                row[0].Text = action.Name;
                string strText = "";
                if (action.ShortcutKey != (char)0)
                {
                    strText = new string(action.ShortcutKey, 1);
                    strText = strText.ToUpper();
                }
                row[1].Text = strText;
                row[2].Text = action.Comment;
                row[3].Text = action.ScriptEntry;

                row.Selected = action.Active;

                if (first_selected_row == null
                    && row.Selected == true)
                    first_selected_row = row;
                if (row.Selected == true)
                    last_selected_row = row;
            }

            if (first_selected_row != null)
                first_selected_row.EnsureVisible();
            if (last_selected_row != null
                && last_selected_row != first_selected_row)
                last_selected_row.EnsureVisible();
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                this.ActionTable_DoubleClick(this, null);
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void ActionTable_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*
            if (e.KeyChar == (char)Keys.Enter)
            {
                this.ActionTable_DoubleClick(this, null);
            }
             * */
        }

        private void ActionTable_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.Assert(this.Actions != null, "");

            char key = (char)e.KeyValue;
            if (key == (char)Keys.Enter)
            {
                this.ActionTable_DoubleClick(this, null);
                return;
            }
            else if (key == (char)Keys.Escape)
            {
                this.Close();
                return;
            }
            else if (char.IsLetter(key) == true)
            {
                foreach (ScriptAction action in this.Actions)
                {
                    if (Char.ToUpper(key) == Char.ToUpper(action.ShortcutKey))
                    {
                        this.SelectedIndex = this.Actions.IndexOf(action);
                        this.SelectedAction = action;

                        if (this.CloseWhenComplete == true)
                            this.Close();

                        if (this.SelectedAction != null
                            && this.TriggerAction != null)
                        {
                            if (this._processing > 0)
                            {
                                MessageBox.Show(this, "正在处理中，请稍后再重新启动执行");
                                return;
                            }

                            BeginProcess();
                            try
                            {
                                TriggerActionArgs e1 = new TriggerActionArgs();
                                e1.EntryName = this.SelectedAction.ScriptEntry;
                                e1.sender = this.sender;
                                e1.e = this.e;
                                this.TriggerAction(this, e1);
                            }
                            finally
                            {
                                EndProcess();
                            }
                        }
                        return;
                    }
                }

                // Console.Beep();
            }

        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SET_FOCUS:
                    this.ActionTable.Focus();
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void ActionTable_SelectionChanged(object sender, EventArgs e)
        {
            int nCount = this.ActionTable.SelectedRows.Count;
            if (nCount == 0)
            {
                this.button_excute.Text = "执行";
                this.button_excute.Enabled = false;
            }
            else
            {
                if (nCount > 1)
                    this.button_excute.Text = "执行("+nCount.ToString()+"项)";
                else
                    this.button_excute.Text = "执行";

                this.button_excute.Enabled = true;
            }
        }

        private void button_excute_Click(object sender, EventArgs e)
        {
            if (Actions == null || this.TriggerAction == null)
                return;

            if (this._processing > 0)
            {
                MessageBox.Show(this, "正在处理中，请稍后再重新启动执行");
                return;
            }

            if (this.ActionTable.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要执行的事项...");
                return;
            }

            if (this.CloseWhenComplete == true)
                this.Close();

            this.ActionTable.Focus();   // 如果没有这个语句, 功能执行完后会把书目查询窗给翻起来

            BeginProcess();
            try
            {

                List<DpRow> selections = new List<DpRow>();
                selections.AddRange(this.ActionTable.SelectedRows);

                foreach (DpRow row in selections)
                {
                    ScriptAction action = (ScriptAction)row.Tag;
                    Debug.Assert(action != null, "");

                    if (action != null
                        && this.TriggerAction != null)
                    {
                        TriggerActionArgs e1 = new TriggerActionArgs();
                        e1.EntryName = action.ScriptEntry;
                        e1.sender = this.sender;
                        e1.e = this.e;
                        this.TriggerAction(this, e1);
                    }
                }
            }
            finally
            {
                EndProcess();
            }
        }
    }

    public delegate void TriggerActionEventHandler(object sender,
TriggerActionArgs e);

    public class TriggerActionArgs : EventArgs
    {
        public string EntryName = "";
        public object sender = null;
        public GenerateDataEventArgs e = null;
    }

    //
    public delegate void RefreshMenuEventHandler(object sender,
RefreshMenuEventArgs e);

    public class RefreshMenuEventArgs : EventArgs
    {
        public List<ScriptAction> Actions = null;
        public object sender = null;
        public GenerateDataEventArgs e = null;
    }

    //
    public class SetMenuEventArgs : EventArgs
    {
        public ScriptAction Action = null;
        public object sender = null;
        public GenerateDataEventArgs e = null;
    }
}
