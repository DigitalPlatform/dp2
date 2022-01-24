using System;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

namespace dp2Circulation
{
    internal partial class BindingOptionDialog : Form
    {
        bool m_bCellContentsChanged = false;
        bool m_bGroupContentsChanged = false;

        public IApplicationInfo AppInfo = null;

        public string[] DefaultTextLineNames = null;
        public string[] DefaultGroupTextLineNames = null;

        public BindingOptionDialog()
        {
            InitializeComponent();
        }

        private void BindingOptionDialog_Load(object sender, EventArgs e)
        {
            Debug.Assert(this.AppInfo != null, "");

#if NO
            // 验收批次号
            this.textBox_general_acceptBatchNo.Text = this.AppInfo.GetString(
   "binding_form",
   "accept_batchno",
   "");
#endif

            // 批次号需要从默认值模板里面获得
            this.textBox_general_batchNo.Text = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
                "batchNo");

            // 编辑区布局方式
            this.comboBox_ui_splitterDirection.Text = this.AppInfo.GetString(
                "binding_form",
                "splitter_direction",
                "水平");

            // 显示订购信息坐标值
            this.checkBox_ui_displayOrderInfoXY.Checked = this.AppInfo.GetBoolean(
                "binding_form",
                "display_orderinfoxy",
                false);

            // 显示分馆外订购组
            this.checkBox_ui_displayLockedOrderGroup.Checked = this.AppInfo.GetBoolean(
                "binding_form",
                "display_lockedOrderGroup",
                true);

            // 册格子内容行
            {
                string strLinesCfg = this.AppInfo.GetString(
        "binding_form",
        "cell_lines_cfg",
        "");
                if (string.IsNullOrEmpty(strLinesCfg) == true
                    && this.DefaultTextLineNames != null)
                {
                    strLinesCfg = string.Join(",", this.DefaultTextLineNames);
                }

                FillCellContentsList(strLinesCfg);
            }

            // 组格子内容行
            {
                string strLinesCfg = this.AppInfo.GetString(
    "binding_form",
    "group_lines_cfg",
    "");
                if (string.IsNullOrEmpty(strLinesCfg) == true
                    && this.DefaultGroupTextLineNames != null)
                {
                    strLinesCfg = string.Join(",", this.DefaultGroupTextLineNames);
                }

                FillGroupContentsList(strLinesCfg);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            Debug.Assert(this.AppInfo != null, "");

#if NO
            this.AppInfo.SetString(
               "binding_form",
               "accept_batchno",
               this.textBox_general_acceptBatchNo.Text);
#endif
            // 批次号需要保存到默认值模板
            EntityFormOptionDlg.SetFieldValue("quickRegister_default",
"batchNo",
this.textBox_general_batchNo.Text);

            this.AppInfo.SetString(
                "binding_form",
                "splitter_direction",
                this.comboBox_ui_splitterDirection.Text);

            // 显示订购信息坐标值
            this.AppInfo.SetBoolean(
                "binding_form",
                "display_orderinfoxy",
                this.checkBox_ui_displayOrderInfoXY.Checked);

            // 显示分馆外订购信息
            this.AppInfo.SetBoolean(
                "binding_form",
                "display_lockedOrderGroup",
                this.checkBox_ui_displayLockedOrderGroup.Checked);

            // 册格子内容行
            if (this.m_bCellContentsChanged == true)
            {
                string strLinesCfg = GetCellContentList();
                string strDefault = string.Join(",", this.DefaultTextLineNames);
                if (strLinesCfg == strDefault)
                    strLinesCfg = "";

                this.AppInfo.SetString(
         "binding_form",
         "cell_lines_cfg",
         strLinesCfg);
            }

            // 组格子内容行
            if (this.m_bGroupContentsChanged == true)
            {
                string strLinesCfg = GetGroupContentList();
                string strDefault = string.Join(",", this.DefaultGroupTextLineNames);
                if (strLinesCfg == strDefault)
                    strLinesCfg = "";

                this.AppInfo.SetString(
         "binding_form",
         "group_lines_cfg",
         strLinesCfg);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        void FillCellContentsList(string strCfgText)
        {
            this.listView_cellContents_lines.Items.Clear();

            string[] parts = strCfgText.Split(new char[] {','});

            for (int i = 0; i < parts.Length / 2; i++)
            {
                string strName = parts[i * 2];
                string strCaption = parts[i * 2 + 1];

                ListViewItem item = new ListViewItem();
                item.Text = strName;
                ListViewUtil.ChangeItemText(item, 1, strCaption);

                this.listView_cellContents_lines.Items.Add(item);
            }
        }

        void FillGroupContentsList(string strCfgText)
        {
            this.listView_groupContents_lines.Items.Clear();

            string[] parts = strCfgText.Split(new char[] { ',' });

            for (int i = 0; i < parts.Length / 2; i++)
            {
                string strName = parts[i * 2];
                string strCaption = parts[i * 2 + 1];

                ListViewItem item = new ListViewItem();
                item.Text = strName;
                ListViewUtil.ChangeItemText(item, 1, strCaption);

                this.listView_groupContents_lines.Items.Add(item);
            }
        }

        string GetCellContentList()
        {
            string strResult = "";
            for (int i = 0; i < this.listView_cellContents_lines.Items.Count; i++)
            {
                ListViewItem item = this.listView_cellContents_lines.Items[i];

                if (i > 0)
                    strResult += ",";

                strResult += item.Text + ",";
                strResult += ListViewUtil.GetItemText(item, 1);
            }

            return strResult;
        }

        string GetGroupContentList()
        {
            string strResult = "";
            for (int i = 0; i < this.listView_groupContents_lines.Items.Count; i++)
            {
                ListViewItem item = this.listView_groupContents_lines.Items[i];

                if (i > 0)
                    strResult += ",";

                strResult += item.Text + ",";
                strResult += ListViewUtil.GetItemText(item, 1);
            }

            return strResult;
        }

        private void button_cellContents_new_Click(object sender, EventArgs e)
        {
            CellLineDialog dlg = new CellLineDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 查重?
            // 名称查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_cellContents_lines, dlg.FieldName, 0);
            if (dup != null)
            {
                // 让操作者能看见已经存在的行
                ListViewUtil.SelectLine(dup, true);
                dup.EnsureVisible();

                DialogResult result = MessageBox.Show(this,
                    "当前已经存在名为 '" + dlg.FieldName + "' 的内容行。继续新增?",
                    "BindingOptionDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            ListViewItem item = new ListViewItem();
            item.Text = dlg.FieldName;
            ListViewUtil.ChangeItemText(item, 1, dlg.Caption);

            this.listView_cellContents_lines.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();

            listView_cellContents_lines_SelectedIndexChanged(sender, null);

            this.m_bCellContentsChanged = true;
        }

        private void listView_cellContents_lines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_cellContents_lines.SelectedIndices.Count == 0)
            {
                // 没有选择事项
                this.button_cellContents_delete.Enabled = false;
                this.button_cellContents_modify.Enabled = false;
                this.button_cellContents_moveDown.Enabled = false;
                this.button_cellContents_moveUp.Enabled = false;
                this.button_cellContents_new.Enabled = true;
            }
            else
            {
                // 有选择事项
                this.button_cellContents_delete.Enabled = true;
                this.button_cellContents_modify.Enabled = true;
                if (this.listView_cellContents_lines.SelectedIndices[0] >= this.listView_cellContents_lines.Items.Count - 1)
                    this.button_cellContents_moveDown.Enabled = false;
                else
                    this.button_cellContents_moveDown.Enabled = true;

                if (this.listView_cellContents_lines.SelectedIndices[0] == 0)
                    this.button_cellContents_moveUp.Enabled = false;
                else
                    this.button_cellContents_moveUp.Enabled = true;

                this.button_cellContents_new.Enabled = true;

            }
        }

        private void button_cellContents_delete_Click(object sender, EventArgs e)
        {
            if (this.listView_cellContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除选定的 " + this.listView_cellContents_lines.SelectedItems.Count.ToString() + " 个事项? ",
                "BindingOptionDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;


            while (this.listView_cellContents_lines.SelectedItems.Count > 0)
            {
                this.listView_cellContents_lines.Items.Remove(this.listView_cellContents_lines.SelectedItems[0]);
            }

            // 删除事项后，当前已选择事项的上下移动的可能性会有所改变
            listView_cellContents_lines_SelectedIndexChanged(sender, null);

            this.m_bCellContentsChanged = true;
        }

        private void button_cellContents_modify_Click(object sender, EventArgs e)
        {
            if (this.listView_cellContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要修改的事项");
                return;
            }

            CellLineDialog dlg = new CellLineDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.FieldName = this.listView_cellContents_lines.SelectedItems[0].Text;
            dlg.Caption = this.listView_cellContents_lines.SelectedItems[0].SubItems[1].Text;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewItem item = this.listView_cellContents_lines.SelectedItems[0];
            item.Text = dlg.FieldName;
            ListViewUtil.ChangeItemText(item, 1, dlg.Caption);

            this.m_bCellContentsChanged = true;
        }

        private void button_cellContents_moveUp_Click(object sender, EventArgs e)
        {
            if (this.listView_cellContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移动的事项");
                return;
            }

            int nIndex = this.listView_cellContents_lines.SelectedIndices[0];

            if (nIndex == 0)
            {
                MessageBox.Show(this, "已在顶部");
                return;
            }

            ListViewItem item = this.listView_cellContents_lines.SelectedItems[0];

            this.listView_cellContents_lines.Items.Remove(item);
            this.listView_cellContents_lines.Items.Insert(nIndex - 1, item);

            this.m_bCellContentsChanged = true;
        }

        private void button_cellContents_moveDown_Click(object sender, EventArgs e)
        {
            if (this.listView_cellContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移动的事项");
                return;
            }

            int nIndex = this.listView_cellContents_lines.SelectedIndices[0];

            if (nIndex >= this.listView_cellContents_lines.Items.Count - 1)
            {
                MessageBox.Show(this, "已在底部");
                return;
            }

            ListViewItem item = this.listView_cellContents_lines.SelectedItems[0];

            this.listView_cellContents_lines.Items.Remove(item);
            this.listView_cellContents_lines.Items.Insert(nIndex + 1, item);

            this.m_bCellContentsChanged = true;
        }

        private void listView_groupContents_lines_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_groupContents_lines.SelectedIndices.Count == 0)
            {
                // 没有选择事项
                this.button_groupContents_delete.Enabled = false;
                this.button_groupContents_modify.Enabled = false;
                this.button_groupContents_moveDown.Enabled = false;
                this.button_groupContents_moveUp.Enabled = false;
                this.button_groupContents_new.Enabled = true;
            }
            else
            {
                // 有选择事项
                this.button_groupContents_delete.Enabled = true;
                this.button_groupContents_modify.Enabled = true;
                if (this.listView_groupContents_lines.SelectedIndices[0] >= this.listView_cellContents_lines.Items.Count - 1)
                    this.button_groupContents_moveDown.Enabled = false;
                else
                    this.button_groupContents_moveDown.Enabled = true;

                if (this.listView_groupContents_lines.SelectedIndices[0] == 0)
                    this.button_groupContents_moveUp.Enabled = false;
                else
                    this.button_groupContents_moveUp.Enabled = true;

                this.button_groupContents_new.Enabled = true;
            }
        }

        private void button_groupContents_moveUp_Click(object sender, EventArgs e)
        {
            if (this.listView_groupContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移动的事项");
                return;
            }

            int nIndex = this.listView_groupContents_lines.SelectedIndices[0];

            if (nIndex == 0)
            {
                MessageBox.Show(this, "已在顶部");
                return;
            }

            ListViewItem item = this.listView_groupContents_lines.SelectedItems[0];

            this.listView_groupContents_lines.Items.Remove(item);
            this.listView_groupContents_lines.Items.Insert(nIndex - 1, item);

            this.m_bGroupContentsChanged = true;
        }

        private void button_groupContents_moveDown_Click(object sender, EventArgs e)
        {
            if (this.listView_groupContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移动的事项");
                return;
            }

            int nIndex = this.listView_groupContents_lines.SelectedIndices[0];

            if (nIndex >= this.listView_groupContents_lines.Items.Count - 1)
            {
                MessageBox.Show(this, "已在底部");
                return;
            }

            ListViewItem item = this.listView_groupContents_lines.SelectedItems[0];

            this.listView_groupContents_lines.Items.Remove(item);
            this.listView_groupContents_lines.Items.Insert(nIndex + 1, item);

            this.m_bGroupContentsChanged = true;
        }

        private void button_groupContents_new_Click(object sender, EventArgs e)
        {
            CellLineDialog dlg = new CellLineDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.FillGroupFieldNameTable();
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 查重?
            // 名称查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_groupContents_lines, dlg.FieldName, 0);
            if (dup != null)
            {
                // 让操作者能看见已经存在的行
                ListViewUtil.SelectLine(dup, true);
                dup.EnsureVisible();

                DialogResult result = MessageBox.Show(this,
                    "当前已经存在名为 '" + dlg.FieldName + "' 的内容行。继续新增?",
                    "BindingOptionDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }



            ListViewItem item = new ListViewItem();
            item.Text = dlg.FieldName;
            ListViewUtil.ChangeItemText(item, 1, dlg.Caption);

            this.listView_groupContents_lines.Items.Add(item);
            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();

            listView_groupContents_lines_SelectedIndexChanged(sender, null);

            this.m_bGroupContentsChanged = true;
        }

        private void button_groupContents_modify_Click(object sender, EventArgs e)
        {
            if (this.listView_groupContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要修改的事项");
                return;
            }

            CellLineDialog dlg = new CellLineDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.FillGroupFieldNameTable();
            dlg.FieldName = this.listView_groupContents_lines.SelectedItems[0].Text;
            dlg.Caption = this.listView_groupContents_lines.SelectedItems[0].SubItems[1].Text;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewItem item = this.listView_groupContents_lines.SelectedItems[0];
            item.Text = dlg.FieldName;
            ListViewUtil.ChangeItemText(item, 1, dlg.Caption);

            this.m_bGroupContentsChanged = true;
        }

        private void button_groupContents_delete_Click(object sender, EventArgs e)
        {
            if (this.listView_groupContents_lines.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除选定的 " + this.listView_groupContents_lines.SelectedItems.Count.ToString() + " 个事项? ",
                "BindingOptionDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;


            while (this.listView_groupContents_lines.SelectedItems.Count > 0)
            {
                this.listView_groupContents_lines.Items.Remove(this.listView_groupContents_lines.SelectedItems[0]);
            }

            // 删除事项后，当前已选择事项的上下移动的可能性会有所改变
            listView_groupContents_lines_SelectedIndexChanged(sender, null);

            this.m_bGroupContentsChanged = true;
        }

        private void button_defaultEntityFields_Click(object sender, EventArgs e)
        {
            EntityFormOptionDlg dlg = new EntityFormOptionDlg();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.DisplayStyle = "quick_entity";
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == DialogResult.Cancel)
                return;

            // 批次号可能被对话框修改，需要刷新
            this.textBox_general_batchNo.Text = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
                "batchNo");
        }

        private void textBox_general_acceptBatchNo_Leave(object sender, EventArgs e)
        {
            EntityFormOptionDlg.SetFieldValue("quickRegister_default",
                "batchNo",
                this.textBox_general_batchNo.Text);
        }
    }
}