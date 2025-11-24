using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

namespace DigitalPlatform.DTLP
{
    /// <summary>
    /// 编辑一个查重方案的对话框
    /// </summary>
    public partial class ProjectDialog : Form
    {
        public bool CreateMode = false; // 是否在创建模式？==false，表示在修改模式
        public DupCfgDialog DupCfgDialog = null;

        public XmlDocument dom = null;

        XmlNode m_nodeProject = null;

        bool m_bChanged = false;

        // 总体来说，内容是否发生了改变？
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;

                if (value == true)
                    this.button_OK.Enabled = true;
                else
                    this.button_OK.Enabled = false;
            }
        }

        public string ProjectName
        {
            get
            {
                return this.textBox_projectName.Text;
            }
            set
            {
                this.textBox_projectName.Text = value;
            }
        }

        public string ProjectComment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        string m_strCurDatabaseName = "";

        bool m_bAccessPointsChanged = false;

        // 检索点listview中的内容是否发生了改变？
        public bool AccessPointsChanged
        {
            get
            {
                return this.m_bAccessPointsChanged;
            }
            set
            {
                this.m_bAccessPointsChanged = value;

                if (value == true)
                    this.Changed = true;
            }
        }

        public ProjectDialog()
        {
            InitializeComponent();
        }

        private void ProjectDialog_Load(object sender, EventArgs e)
        {
            if (this.CreateMode == false)
            {
                // 先获得<project元素>
                if (String.IsNullOrEmpty(this.ProjectName) == false
                    && this.dom != null)
                {
                    this.m_nodeProject = this.dom.DocumentElement.SelectSingleNode("//project[@name='" + this.ProjectName + "']");
                    if (this.m_nodeProject == null)
                    {
                        this.MessageBoxShow("DOM中并不存在name属性值为 '" + this.ProjectName + "' 的<project>元素");
                    }
                }

                FillDatabaseList();
            }
            else
            {
                // 创建<project元素>
                if (this.dom != null)
                {
                    this.m_nodeProject = this.dom.CreateElement("project");
                    this.dom.DocumentElement.AppendChild(this.m_nodeProject);
                    DomUtil.SetAttr(this.m_nodeProject, "name", this.ProjectName);
                    DomUtil.SetAttr(this.m_nodeProject, "comment", this.ProjectComment);
                }
            }

            this.Changed = false;
        }

        private void ProjectDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ProjectDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.AccessPointsChanged == true)
            {
                PutAccessPoints();
            }

            if (this.Changed == false)
            {
                Debug.Assert(false, "当OK按钮可以按下的时候, this.Changed不可能为false");
            }

            // 查重，看projectname是否和其他<project>元素的name属性值相同
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project[@name='" + this.textBox_projectName.Text + "']");
            int nCount = 0;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strName = DomUtil.GetAttr(node, "name");

                if (node == this.m_nodeProject)
                    continue;

                nCount++;
            }

            if (nCount > 0)
            {
                MessageBox.Show(this, "发现当前方案名 '" +this.textBox_projectName+ "' 和其他 "+nCount.ToString()+" 个<project>元素具有相同的name属性值。必须修改当前方案的名字，避免和它(们)相重。");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        ListViewItem FindDatabaseItem(string strName)
        {
            for (int i = 0; i < this.listView_databases.Items.Count; i++)
            {
                if (strName == this.listView_databases.Items[i].Text)
                    return this.listView_databases.Items[i];
            }

            return null;
        }

        private void button_newDatabase_Click(object sender, EventArgs e)
        {
            TargetDatabaseDialog dlg = new TargetDatabaseDialog();

            dlg.DupCfgDialog = this.DupCfgDialog;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strError = "";

            // 查重
            ListViewItem exist = FindDatabaseItem(dlg.DatabaseName);
            if (exist != null)
            {
                exist.Selected = true;
                exist.EnsureVisible();

                strError = "已经存在名为 '" + dlg.DatabaseName + "' 的目标库事项。放弃新创建事项。";
                goto ERROR1;
            }

            Debug.Assert(this.m_nodeProject != null, "");

            // 找到<project>元素
            XmlNode nodeProject = this.m_nodeProject;
            if (nodeProject == null)
            {
                Debug.Assert(false, "");
                strError = "m_nodeProject成员为空";
                goto ERROR1;
            }

            // 创建新的<database>元素
            XmlNode nodeDatabase = this.dom.CreateElement("database");
            nodeProject.AppendChild(nodeDatabase);

            DomUtil.SetAttr(nodeDatabase, "name", dlg.DatabaseName);
            DomUtil.SetAttr(nodeDatabase, "threshold", dlg.Threshold);

            // 兑现对ListViewItem的增补
            ListViewItem item = new ListViewItem(dlg.DatabaseName, 0);
            item.SubItems.Add(dlg.Threshold);
            this.listView_databases.Items.Add(item);

            item.Selected = true;   // 选中刚插入的listviewitem对象

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        private void listView_databases_DoubleClick(object sender, EventArgs e)
        {
            button_modifyDatabase_Click(this, e);
        }

        private void button_modifyDatabase_Click(object sender, EventArgs e)
        {
            if (this.listView_databases.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的目标数据库事项");
                return;
            }

            ListViewItem item = this.listView_databases.SelectedItems[0];

            TargetDatabaseDialog dlg = new TargetDatabaseDialog();

            dlg.DupCfgDialog = this.DupCfgDialog;
            dlg.DatabaseName = item.Text;
            dlg.Threshold = ListViewUtil.GetItemText(item, 1);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strError = "";

            // 查重
            ListViewItem exist = FindDatabaseItem(dlg.DatabaseName);
            if (exist != null && exist != item)
            {
                exist.Selected = true;
                exist.EnsureVisible();

                strError = "已经存在名为 '" + dlg.DatabaseName + "' 的目标库事项。放弃刚才对事项的修改。";
                goto ERROR1;
            }

            Debug.Assert(this.m_nodeProject != null, "");

            // 找到相应的<database>元素
            XmlNode nodeDatabase = this.m_nodeProject.SelectSingleNode("database[@name='" + item.Text + "']");
            if (nodeDatabase == null)
            {
                strError = "名为 '" + item.Text + "' 的<database>元素并不存在";
                goto ERROR1;
            }

            // 兑现对DOM的修改
            DomUtil.SetAttr(nodeDatabase, "name", dlg.DatabaseName);
            DomUtil.SetAttr(nodeDatabase, "threshold", dlg.Threshold);

            // 兑现对ListViewItem的修改
            item.Text = dlg.DatabaseName;
            ListViewUtil.ChangeItemText(item, 1, dlg.Threshold);

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_deleteDatabase_Click(object sender, EventArgs e)
        {
            if (this.listView_databases.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的目标数据库事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除所选定的 " + this.listView_databases.SelectedIndices.Count.ToString() + " 个目标数据库事项?",
                "ProjectDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            string strError = "";

            for (int i = this.listView_databases.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int index = this.listView_databases.SelectedIndices[i];
                ListViewItem item = this.listView_databases.Items[index];

                Debug.Assert(this.m_nodeProject != null, "");

                // 找到相应的<database>元素
                XmlNode nodeDatabase = this.m_nodeProject.SelectSingleNode("database[@name='" + item.Text + "']");
                if (nodeDatabase == null)
                {
                    strError = "名为 '" + item.Text + "' 的<database>元素并不存在";
                    goto ERROR1;
                }
                // 删除XML节点
                nodeDatabase.ParentNode.RemoveChild(nodeDatabase);

                // 兑现对ListViewItem的修改
                this.listView_databases.Items.RemoveAt(index);
            }

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 新建一个检索点事项
        private void button_newAccessPoint_Click(object sender, EventArgs e)
        {
            AccessPointDialog dlg = new AccessPointDialog();

            // TODO: 是否需要把当前已经选择的对象当作参考对象?

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewItem new_item = new ListViewItem(dlg.FromName, 0);
            new_item.SubItems.Add(dlg.Weight);
            new_item.SubItems.Add(dlg.SearchStyle);
            this.listView_accessPoints.Items.Add(new_item);

            // TODO: 是否将来需要做成插入在当前已选择的事项前面

            this.AccessPointsChanged = true;
        }

        // 修改一个检索点事项
        private void button_modifyAccessPoint_Click(object sender, EventArgs e)
        {
            if (this.listView_accessPoints.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的检索点事项");
                return;
            }

            ListViewItem item = this.listView_accessPoints.SelectedItems[0];

            AccessPointDialog dlg = new AccessPointDialog();

            dlg.FromName = item.Text;
            dlg.Weight = ListViewUtil.GetItemText(item, 1);
            dlg.SearchStyle = ListViewUtil.GetItemText(item, 2);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            item.Text = dlg.FromName;
            ListViewUtil.ChangeItemText(item, 1, dlg.Weight);
            ListViewUtil.ChangeItemText(item, 2, dlg.SearchStyle);

            this.AccessPointsChanged = true;
        }

        // 删除一个检索点事项
        private void button_deleteAccessPoint_Click(object sender, EventArgs e)
        {
            if (this.listView_accessPoints.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的检索点事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除所选定的 " + this.listView_accessPoints.SelectedIndices.Count.ToString() + " 个检索点事项?",
                    "ProjectDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            for (int i = this.listView_accessPoints.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int index = this.listView_accessPoints.SelectedIndices[i];

                this.listView_accessPoints.Items.RemoveAt(index);
            }

            this.AccessPointsChanged = true;
        }

        // 填充(目标)数据库列表
        // throw:
        //      Exception
        void FillDatabaseList()
        {
            this.listView_databases.Items.Clear();

            if (this.dom == null)
                return;

            if (String.IsNullOrEmpty(this.ProjectName) == true)
                return;

            Debug.Assert(this.m_nodeProject != null, "");

            XmlNodeList database_nodes = this.m_nodeProject.SelectNodes("database");
            for (int i = 0; i < database_nodes.Count; i++)
            {
                XmlNode node = database_nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strThreshold = DomUtil.GetAttr(node, "threshold");

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strThreshold);

                this.listView_databases.Items.Add(item);
            }
        }

        private void listView_databases_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_databases.SelectedItems.Count == 0)
            {
                this.button_deleteDatabase.Enabled = false;
                this.button_modifyDatabase.Enabled = false;

                // 在上面listview没有选择项的时候，下面也无法用
                this.button_newAccessPoint.Enabled = false;
                this.button_modifyAccessPoint.Enabled = false;
                this.button_deleteAccessPoint.Enabled = false;
            }
            else
            {
                this.button_deleteDatabase.Enabled = true;
                this.button_modifyDatabase.Enabled = true;

                this.button_newAccessPoint.Enabled = true;

                // 其余两个按钮，不去管
            }

            FillAccessPointList();
        }

        // 根据当前选中的数据库名，填充检索点列表
        void FillAccessPointList()
        {
            // 检查先前是否有修改？
            if (this.AccessPointsChanged == true)
            {
                PutAccessPoints();
            }

            this.listView_accessPoints.Items.Clear();

            if (this.listView_databases.SelectedItems.Count == 0)
                return;

            string strDatabaseName = this.listView_databases.SelectedItems[0].Text;

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project[@name='" + this.ProjectName + "']/database[@name='" + strDatabaseName + "']/accessPoint");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strWeight = DomUtil.GetAttr(node, "weight");
                string strSearchStyle = DomUtil.GetAttr(node, "searchStyle");

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strWeight);
                item.SubItems.Add(strSearchStyle);

                this.listView_accessPoints.Items.Add(item);
            }

            this.m_strCurDatabaseName = strDatabaseName;

            listView_accessPoints_SelectedIndexChanged(this, null);
        }

        // 将检索点信息的修改，兑现到相应的<database>元素内
        void PutAccessPoints()
        {
            if (this.AccessPointsChanged == false)
                return;

            if (this.m_strCurDatabaseName == "")
            {
                Debug.Assert(false, "");
                return;
            }

            // 找到相应的<database>元素
            XmlNode nodeDatabase = this.dom.DocumentElement.SelectSingleNode("//project[@name='" + this.ProjectName + "']/database[@name='" + this.m_strCurDatabaseName + "']");
            if (nodeDatabase == null)
            {
                Debug.Assert(false, "名为 '" + this.m_strCurDatabaseName + "' 的<database>元素并不存在");
                return;
            }

            // 删除原有的下属元素
            while (nodeDatabase.ChildNodes.Count != 0)
            {
                nodeDatabase.RemoveChild(nodeDatabase.ChildNodes[0]);
            }

            // 将listview中的元素加入
            for (int i = 0; i < this.listView_accessPoints.Items.Count; i++)
            {
                ListViewItem item = this.listView_accessPoints.Items[i];

                string strName = item.Text;
                string strWeight = ListViewUtil.GetItemText(item, 1);
                string strSearchStyle = ListViewUtil.GetItemText(item, 2);

                XmlNode new_node = this.dom.CreateElement("accessPoint");
                nodeDatabase.AppendChild(new_node);
                DomUtil.SetAttr(new_node, "name", strName);
                DomUtil.SetAttr(new_node, "weight", strWeight);
                DomUtil.SetAttr(new_node, "searchStyle", strSearchStyle);
            }

            this.AccessPointsChanged = false;
        }

        private void listView_accessPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_accessPoints.SelectedItems.Count == 0)
            {
                this.button_deleteAccessPoint.Enabled = false;
                this.button_modifyAccessPoint.Enabled = false;
            }
            else
            {
                this.button_deleteAccessPoint.Enabled = true;
                this.button_modifyAccessPoint.Enabled = true;
            }
        }

        private void listView_accessPoints_DoubleClick(object sender, EventArgs e)
        {
            button_modifyAccessPoint_Click(this, e);
        }

        private void textBox_projectName_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nodeProject != null)
                DomUtil.SetAttr(this.m_nodeProject, "name", this.textBox_projectName.Text);

            this.Changed = true;
        }

        private void textBox_comment_TextChanged(object sender, EventArgs e)
        {
            if (this.m_nodeProject != null)
                DomUtil.SetAttr(this.m_nodeProject, "comment", this.textBox_comment.Text);

            this.Changed = true;

        }


    }
}