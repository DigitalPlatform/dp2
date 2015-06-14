using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.CommonControl;

namespace DigitalPlatform.Script
{
    public partial class ScriptActionMenuDlg : Form
    {
        public ScriptActionCollection Actions = null;

        public int SelectedIndex = -1;
        public ScriptAction SelectedAction = null;

        public ScriptActionMenuDlg()
        {
            InitializeComponent();
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

        private void ScriptActionMenuDlg_Load(object sender, EventArgs e)
        {

            if (this.Owner != null)
                this.Font = this.Owner.Font;
            else
            {
                Font default_font = GetDefaultFont();
                if (default_font != null)
                    this.Font = default_font;
            }

            FillList();

            if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                // 旁路
            }
            else
            {
                // 自动执行
                if (this.AutoRun == true)
                {
                    if (this.ActionTable.SelectedRows.Count == 1)
                    {
                        button_OK_Click(null, null);
                        return;
                    }

                }
            }
        }

        void FillList()
        {
            this.ActionTable.Rows.Clear();
            if (Actions == null)
                return;

            DpRow first_item = null;

            for (int i = 0; i < Actions.Count; i++)
            {
                ScriptAction action = (ScriptAction)Actions[i];

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

                this.ActionTable.Rows.Add(item);
            }

            if (first_item != null)
            {
                this.ActionTable.FocusedItem = first_item;
                first_item.EnsureVisible();
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.ActionTable.SelectedRows.Count == 0)
            {
                MessageBox.Show(this, "尚未选择事项...");
                return;
            }

            this.SelectedIndex = this.ActionTable.SelectedRowIndices[0];
            if (Actions != null)
                this.SelectedAction = (ScriptAction)this.Actions[this.SelectedIndex];
            else
                this.SelectedAction = null;


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void dpTable1_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);
        }

        // 是否自动运行
        public bool AutoRun
        {
            get
            {
                return checkBox_autoRun.Checked;
            }
            set
            {
                checkBox_autoRun.Checked = value;
            }
        }

        private void dpTable1_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.Assert(this.Actions != null, "");

            char key = (char)e.KeyValue;
            if (key == (char)Keys.Enter)
            {
                button_OK_Click(null, null);
                return;
            }
            else if (key == (char)Keys.Escape)
            {
                button_Cancel_Click(null, null);
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

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        return;
                    }
                }

                // Console.Beep();
            }
        }
    }
}
