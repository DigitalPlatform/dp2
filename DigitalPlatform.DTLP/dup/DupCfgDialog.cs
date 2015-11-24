using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;

namespace DigitalPlatform.DTLP
{
    /// <summary>
    /// 查重配置对话框
    /// </summary>
    public partial class DupCfgDialog : Form
    {
        public DtlpChannelArray DtlpChannels = null;
        public DtlpChannel DtlpChannel = null;

        public string CfgFilename = ""; // XML配置文件名

        XmlDocument dom = null; // 配置文件DOM

        bool m_bChanged = false;

        public DupCfgDialog()
        {
            InitializeComponent();
        }

        private void DupCfgDialog_Load(object sender, EventArgs e)
        {
            FillProjectNameList();

            FillDefaultList();
        }

        private void DupCfgDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Changed == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前有修改的内容尚未保存。若此时关闭窗口，这些修改将丢失。\r\n\r\n确实要关闭窗口? ",
                    "DupCfgDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

        }

        private void DupCfgDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.Changed == true
                && this.dom != null
                && String.IsNullOrEmpty(this.CfgFilename) == false)
            {
                this.dom.Save(this.CfgFilename);
                this.Changed = false;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_newProject_Click(object sender, EventArgs e)
        {
            // 复制出一个新的DOM
            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(this.dom.OuterXml);

            ProjectDialog dlg = new ProjectDialog();

            dlg.CreateMode = true;
            dlg.DupCfgDialog = this;
            dlg.ProjectName = "新的查重方案";
            dlg.ProjectComment = "";
            dlg.dom = new_dom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.dom = new_dom;

            // 刷新列表
            FillProjectNameList();

            // 选定刚插入的事项
            SelectProjectItem(dlg.ProjectName);

            this.Changed = true;

            FillDefaultList();  // 库名的集合可能发生改变
        }

        // 在方案名列表中，选定一个特定的名字的行
        void SelectProjectItem(string strProjectName)
        {
            for (int i = 0; i < this.listView_projects.Items.Count; i++)
            {
                ListViewItem item = this.listView_projects.Items[i];
                if (item.Text == strProjectName)
                    item.Selected = true;
                else
                    item.Selected = false;
            }
        }

        // 修改查重方案
        private void button_modifyProject_Click(object sender, EventArgs e)
        {
            if (this.listView_projects.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的查重方案事项");
                return;
            }

            ListViewItem item = this.listView_projects.SelectedItems[0];

            string strProjectName = item.Text;
            string strProjectComment = ListViewUtil.GetItemText(item, 1);

            // 复制出一个新的DOM
            XmlDocument new_dom = new XmlDocument();
            new_dom.LoadXml(this.dom.OuterXml);

            ProjectDialog dlg = new ProjectDialog();

            dlg.CreateMode = false;
            dlg.DupCfgDialog = this;
            dlg.ProjectName = strProjectName;
            dlg.ProjectComment = strProjectComment;
            dlg.dom = new_dom;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.dom = new_dom;

            item.Text = dlg.ProjectName;
            ListViewUtil.ChangeItemText(item,
                1, dlg.ProjectComment);

            this.Changed = true;

            FillDefaultList(); // 库名的集合可能发生改变

            if (strProjectName != dlg.ProjectName)
            {
                // 方案名发生改变后，兑现到下方的缺省关系列表中
                ChangeDefaultProjectName(strProjectName,
                    dlg.ProjectName);
            }
        }

        // 删除查重方案
        private void button_deleteProject_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_projects.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的查重方案事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除所选定的 " + this.listView_projects.SelectedIndices.Count.ToString() + " 个查重方案?",
                "DupCfgDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            for (int i = this.listView_projects.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int index = this.listView_projects.SelectedIndices[i];

                ListViewItem item = this.listView_projects.Items[index];

                string strProjectName = item.Text;

                XmlNode nodeProject = this.dom.DocumentElement.SelectSingleNode("//project[@name='" + strProjectName + "']");
                if (nodeProject == null)
                {
                    strError = "不存在name属性值为 '" + strProjectName + "' 的<project>元素";
                    goto ERROR1;
                }

                nodeProject.ParentNode.RemoveChild(nodeProject);

                this.listView_projects.Items.RemoveAt(index);

                // 方案名删除，兑现到下方的缺省关系列表中，也删除相应的列
                ChangeDefaultProjectName(strProjectName,
                        null);
            }

            this.Changed = true;

            FillDefaultList(); // 库名的集合可能发生改变

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 初始化
        public int Initial(string strCfgFilename,
            out string strError)
        {
            strError = "";

            this.dom = new XmlDocument();
            try
            {
                this.dom.Load(strCfgFilename);
            }
            catch (FileNotFoundException /*ex*/)
            {
                this.dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                strError = "装载配置文件 '" 
                    +strCfgFilename
                    +"' 到XMLDOM时发生错误: "
                    +ex.Message;
                return -1;
            }

            this.CfgFilename = strCfgFilename;

            return 0;
        }

        void FillProjectNameList()
        {
            this.listView_projects.Items.Clear();

            if (this.dom == null)
                return;

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//project");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strComment = DomUtil.GetAttr(node, "comment");

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strComment);
                this.listView_projects.Items.Add(item);
            }
        }

        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        private void listView_projects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_projects.SelectedItems.Count == 0)
            {
                this.button_deleteProject.Enabled = false;
                this.button_modifyProject.Enabled = false;
            }
            else
            {
                this.button_deleteProject.Enabled = true;
                this.button_modifyProject.Enabled = true;
            }
        }

        // 双击，等同于编辑修改
        private void listView_projects_DoubleClick(object sender, EventArgs e)
        {
            button_modifyProject_Click(this, e);
        }

        // 修改数据库和缺省查重方案对应关系
        private void button_modifyDefaut_Click(object sender, EventArgs e)
        {
            if (this.listView_defaults.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要修改的缺省关系事项");
                return;
            }

            ListViewItem item = this.listView_defaults.SelectedItems[0];

            DefaultProjectDialog dlg = new DefaultProjectDialog();
            dlg.dom = this.dom;
            dlg.DatabaseName = item.Text;
            dlg.DefaultProjectName = ListViewUtil.GetItemText(item, 1);

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 兑现到DOM中
            XmlNode nodeDefault = this.dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + item.Text + "']");
            if (nodeDefault == null)
            {
                nodeDefault = this.dom.CreateElement("sourceDatabase");

                XmlNode nodeRoot = this.dom.DocumentElement.SelectSingleNode("//defaultProject");
                if (nodeRoot == null)
                {
                    nodeRoot = this.dom.CreateElement("defaultProject");
                    this.dom.DocumentElement.AppendChild(nodeRoot);
                }

                nodeRoot.AppendChild(nodeDefault);
            }

            DomUtil.SetAttr(nodeDefault, "name", item.Text);
            DomUtil.SetAttr(nodeDefault, "defaultProject", dlg.DefaultProjectName);


            // 兑现到listview中
            Debug.Assert(dlg.DatabaseName == item.Text, "");
            ListViewUtil.ChangeItemText(item, 1, dlg.DefaultProjectName);

            this.Changed = true;
        }

        private void button_newDefault_Click(object sender, EventArgs e)
        {
            string strError = "";

            DefaultProjectDialog dlg = new DefaultProjectDialog();

            dlg.Text = "新增缺省关系事项";
            dlg.DupCfgDialog = this;
            dlg.dom = this.dom;
            dlg.DatabaseName = "";  // 让填入内容
            dlg.DefaultProjectName = "";

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 兑现到DOM中
            XmlNode nodeDefault = this.dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + dlg.DatabaseName + "']");
            if (nodeDefault != null)
            {
                // 查重
                strError = "发起路径为 '" + dlg.DatabaseName + "' 的缺省关系事项已经存在，不能再次新增。可编辑已经存在的该事项。";
                goto ERROR1;
            }

            {
                nodeDefault = this.dom.CreateElement("sourceDatabase");

                XmlNode nodeRoot = this.dom.DocumentElement.SelectSingleNode("//defaultProject");
                if (nodeRoot == null)
                {
                    nodeRoot = this.dom.CreateElement("defaultProject");
                    this.dom.DocumentElement.AppendChild(nodeRoot);
                }

                nodeRoot.AppendChild(nodeDefault);
            }
            DomUtil.SetAttr(nodeDefault, "name", dlg.DatabaseName);
            DomUtil.SetAttr(nodeDefault, "defaultProject", dlg.DefaultProjectName);

            // 兑现到listview中
            ListViewItem item = new ListViewItem(dlg.DatabaseName, 0);
            item.SubItems.Add(dlg.DefaultProjectName);
            this.listView_defaults.Items.Add(item);

            // 看看数据库名字是否在已经用到的数据库名集合中？如果是，为实在颜色；如果不是，为发虚颜色
            List<string> database_names = GetAllDatabaseNames();
            if (database_names.IndexOf(dlg.DatabaseName) == -1)
            {
                item.ForeColor = SystemColors.GrayText;
                item.Tag = null;
            }
            else
            {
                item.Tag = 1;
            }

            this.Changed = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_deleteDefault_Click(object sender, EventArgs e)
        {
            // string strError = "";

            if (this.listView_defaults.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要删除的缺省关系事项");
                return;
            }

            ListViewItem item = this.listView_defaults.SelectedItems[0];
            string strText = item.Text + " -- " + ListViewUtil.GetItemText(item, 1);
            if (item.Tag == null)
            {
                DialogResult result = MessageBox.Show(this,
                    "确实要删除缺省关系事项 "+strText+" ?",
                    "DupCfgDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;

                // 发虚的事项可以删除
                this.listView_defaults.Items.Remove(item);
            }
            else
            {
                DialogResult result = MessageBox.Show(this,
                    "确实要清除缺省关系事项 " + strText + " ?",
                    "DupCfgDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;

                // 实在的事项，只能抹除方案名栏内容
                ListViewUtil.ChangeItemText(item, 1, "");
            }

            // 兑现到DOM中
            XmlNode nodeDefault = this.dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + item.Text + "']");
            /*
            if (nodeDefault == null)
            {
                strError = "发起路径为 '" + item.Text + "' 的缺省关系事项居然在DOM中不存在";
                goto ERROR1;
            }
             * */
            // 2009/3/13 changed
            if (nodeDefault != null)
            {
                nodeDefault.ParentNode.RemoveChild(nodeDefault);
                this.Changed = true;
            }
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
             * */
        }

        private void listView_defaults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_defaults.SelectedItems.Count == 0)
            {
                this.button_modifyDefaut.Enabled = false;
                this.button_deleteDefault.Enabled = false;
            }
            else
            {
                this.button_modifyDefaut.Enabled = true;
                this.button_deleteDefault.Enabled = true;
            }
        }


        void FillDefaultList()
        {
            this.listView_defaults.Items.Clear();

            // 获得全部<sourceDatabase>元素name属性中的发起路径
            List<string> startpaths = new List<string>();
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//defaultProject/sourceDatabase");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strStartPath = DomUtil.GetAttr(nodes[i], "name");
                if (String.IsNullOrEmpty(strStartPath) == true)
                    continue;
                startpaths.Add(strStartPath);
            }


            // 先按照查重方案定义中用到过的发起路径(数据库)列出多行
            List<string> database_names = GetAllDatabaseNames();
            for (int i = 0; i < database_names.Count; i++)
            {
                string strDatabaseName = database_names[i];

                string strDefaultProject = "";
                XmlNode nodeDefault = this.dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + strDatabaseName + "']");
                if (nodeDefault != null)
                    strDefaultProject = DomUtil.GetAttr(nodeDefault, "defaultProject");

                ListViewItem item = new ListViewItem(strDatabaseName, 0);
                item.SubItems.Add(strDefaultProject);
                this.listView_defaults.Items.Add(item);
                item.Tag = 1;   // 表示为实在

                // 从startpaths中移走已经用过的startpath
                startpaths.Remove(strDatabaseName);
            }

            // 再按照查重方案定义中没有用到过的发起路径列出多行
            for (int i = 0; i < startpaths.Count; i++)
            {
                string strDatabaseName = startpaths[i];

                string strDefaultProject = "";
                XmlNode nodeDefault = this.dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + strDatabaseName + "']");
                if (nodeDefault != null)
                    strDefaultProject = DomUtil.GetAttr(nodeDefault, "defaultProject");

                ListViewItem item = new ListViewItem(strDatabaseName, 0);
                item.SubItems.Add(strDefaultProject);
                this.listView_defaults.Items.Add(item);
                item.Tag = null;    // 表示为发虚

                item.ForeColor = SystemColors.GrayText; // 颜色发虚，表示这个数据库名没有在查重方案定义中出现过
            }
        }

        // 获得全部的数据库名
        List<string> GetAllDatabaseNames()
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//database");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(DomUtil.GetAttr(nodes[i], "name"));
            }

            results.Sort();

            // 去重
            Global.RemoveDup(ref results);

            return results;
        }

        // 修改缺省关系
        private void listView_defaults_DoubleClick(object sender, EventArgs e)
        {
            button_modifyDefaut_Click(this, e);
        }

        // 方案名发生改变后，兑现到下方的缺省关系列表中
        void ChangeDefaultProjectName(string strOldProjectName,
            string strNewProjectName)
        {
            if (strOldProjectName == strNewProjectName)
            {
                Debug.Assert(false, "");
                return;
            }

            bool bChanged = false;
            int nCount = 0;
            for (int i = 0; i < listView_defaults.Items.Count; i++)
            {
                ListViewItem item = this.listView_defaults.Items[i];

                // 兑现视觉修改
                string strProjectName = ListViewUtil.GetItemText(item, 1);
                if (strProjectName == strOldProjectName)
                {
                    ListViewUtil.ChangeItemText(item, 1, strNewProjectName);
                    bChanged = true;
                    nCount++;
                }

            }
            // 兑现DOM修改
            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("//defaultProject/sourceDatabase[@defaultProject='"+strOldProjectName+"']");
            Debug.Assert(nCount == nodes.Count, "两边数目应该相等");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                if (String.IsNullOrEmpty(strNewProjectName) == true)
                {
                    // 删除
                    node.ParentNode.RemoveChild(node);
                }
                else
                {
                    DomUtil.SetAttr(node, "defaultProject", strNewProjectName);
                }
                bChanged = true;
            }

            if (bChanged == true)
                this.Changed = true;
        }

        #region 从dt1000 gcs.ini升级查重配置

        // 将dt1000 gcs.ini文件中的查重配置升级到dup.xml文件中
        /*
[/我的电脑/图书编目]
key1=010$a,50,
key2=200$a,50,
key3=905$d,20,
key4=906$a,50,
key5=986$a,50,
HoldValue=130
targetDB1=/我的电脑/图书编目,
targetDB2=/我的电脑/图书总库,
         * */
        public static int UpgradeGcsIniDupCfg(string strGcsIniFilename,
            string strDupXmlFilename,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            // 获得全部section值，
            List<string> sections = null;

            int nRet = GetIniSections(strGcsIniFilename,
                out sections,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return -1;

            // 然后看每个section里面有没有一个名为"key1"的entry，
            // 如果有，就是我们关心的配置section
            List<string> dbpaths = new List<string>();
            for (int i = 0; i < sections.Count; i++)
            {
                string strSection = sections[i];

                StringBuilder s = new StringBuilder(255, 255);

                nRet = API.GetPrivateProfileString(strSection,
                    "key1",
                    "!!!null",
                    s,
                    255,
                    strGcsIniFilename);
                string strValue = s.ToString();
                if (nRet <= 0
                    || strValue == "!!!null")
                {
                }
                else
                {
                    if (IsValidDbPath(strSection) == true)
                        dbpaths.Add(strSection);
                }
            }

            // 方案名和发起数据库的关系数组
            List<DefaultProjectRelation> relations = new List<DefaultProjectRelation>();

            string strToday = DateTime.Now.ToShortDateString();

            // 做每个数据库
            for (int i = 0; i < dbpaths.Count; i++)
            {
                string strDbPath = dbpaths[i];

                if (String.IsNullOrEmpty(strDbPath) == true)
                    continue;

                string strThreshold = "";

                StringBuilder s = new StringBuilder(255, 255);
                nRet = API.GetPrivateProfileString(strDbPath,
                    "HoldValue",
                    "!!!null",
                    s,
                    255,
                    strGcsIniFilename);
                string strValue = s.ToString();
                if (strValue != "!!!null")
                {
                    strThreshold = strValue;
                }

                if (String.IsNullOrEmpty(strThreshold) == true)
                    continue;

                // 增加一个<project>元素
                XmlNode nodeProject = dom.CreateElement("project");
                dom.DocumentElement.AppendChild(nodeProject);
                string strProjectName = "查重方案" + (i + 1).ToString();    //"查重方案" + (i + 1).ToString() + " " + NewStylePath(strDbPath)
                DomUtil.SetAttr(nodeProject, "name", strProjectName);
                DomUtil.SetAttr(nodeProject, "comment", "发起库为 " + NewStylePath(strDbPath) + "，" + strToday + " 从dt1000升级上来");

                DefaultProjectRelation relation = new DefaultProjectRelation();
                relation.StartDbPath = NewStylePath(strDbPath);
                relation.ProjectName = strProjectName;
                relations.Add(relation);

                // 增加若干<database>元素
                for (int j = 0; ; j++)
                {
                    string strEntry = "targetDB" + (j + 1).ToString();

                    StringBuilder s1 = new StringBuilder(255, 255);
                    nRet = API.GetPrivateProfileString(strDbPath,
                        strEntry,
                        "!!!null",
                        s1,
                        255,
                        strGcsIniFilename);
                    string strLine = s1.ToString();
                    if (nRet <= 0
                        || strLine == "!!!null")
                        break;

                    string strDatabaseName = "";
                    string strType = "";

                    string[] parts = strLine.Split(new char[] { ',' });
                    if (parts.Length > 0)
                        strDatabaseName = parts[0].Trim();

                    if (parts.Length > 1)
                        strType = parts[1].Trim();

                    XmlNode nodeDatabase = dom.CreateElement("database");
                    nodeProject.AppendChild(nodeDatabase);

                    DomUtil.SetAttr(nodeDatabase, "name", NewStylePath(strDatabaseName));
                    DomUtil.SetAttr(nodeDatabase, "threshold", strThreshold);

                    ////
                    // 增加若干<accessPoint>元素
                    for (int k = 0; ; k++)
                    {
                        string strEntry2 = "key" + (k + 1).ToString();

                        StringBuilder s2 = new StringBuilder(255, 255);
                        nRet = API.GetPrivateProfileString(strDbPath,
                            strEntry2,
                            "!!!null",
                            s2,
                            255,
                            strGcsIniFilename);
                        string strLine1 = s2.ToString();
                        if (nRet <= 0
                            || strLine1 == "!!!null")
                            break;

                        string strFromName = "";
                        string strWeight = "";
                        string strSearchStyle = "";

                        string[] parts_of_line = strLine1.Split(new char[] { ',' });
                        if (parts_of_line.Length > 0)
                            strFromName = parts_of_line[0].Trim();

                        if (parts.Length > 1)
                            strWeight = parts_of_line[1].Trim();

                        if (parts_of_line.Length > 2)
                            strSearchStyle = parts_of_line[2].Trim();

                        if (strSearchStyle == "q")
                            strSearchStyle = "Left";
                        else if (strSearchStyle == "")
                            strSearchStyle = "Exact";

                        XmlNode nodeAccessPoint = dom.CreateElement("accessPoint");
                        nodeDatabase.AppendChild(nodeAccessPoint);

                        DomUtil.SetAttr(nodeAccessPoint, "name", strFromName);
                        DomUtil.SetAttr(nodeAccessPoint, "weight", strWeight);
                        DomUtil.SetAttr(nodeAccessPoint, "searchStyle", strSearchStyle);
                    } // end of k


                } // end of j

            } // end of i

            // 缺省查重方案定义
            // 在根下增加一个<defaultProject>容器元素
            XmlNode nodeContainer = dom.CreateElement("defaultProject");
            dom.DocumentElement.AppendChild(nodeContainer);

            for (int i = 0; i < relations.Count; i++)
            {
                DefaultProjectRelation relation = relations[i];

                XmlNode nodeSourceDatabase = dom.CreateElement("sourceDatabase");
                nodeContainer.AppendChild(nodeSourceDatabase);

                DomUtil.SetAttr(nodeSourceDatabase, "name", relation.StartDbPath);
                DomUtil.SetAttr(nodeSourceDatabase, "defaultProject", relation.ProjectName);
            }


            dom.Save(strDupXmlFilename);

            return 0;
        }

        // 将旧风格的路径变为新风格的路径。去掉第一个字符的'/'
        public static string NewStylePath(string strDbPath)
        {
            if (String.IsNullOrEmpty(strDbPath) == true)
                return strDbPath;

            // 去掉第一个'/'字符
            if (strDbPath[0] == '/')
                return strDbPath.Substring(1);

            return strDbPath;
        }

        // 看看是不是合法的数据库路径？
        // 如果第一级为“我的电脑”，则不是合法的路径(这显然是home模式下的路径，升级后已不再支持)
        static bool IsValidDbPath(string strDbPath)
        {
            strDbPath = NewStylePath(strDbPath);

            string[] parts = strDbPath.Split(new char[] { '/' });

            string strServerName = "";
            string strDbName = "";

            if (parts.Length > 0)
                strServerName = parts[0].Trim();

            if (parts.Length > 1)
                strDbName = parts[1].Trim();

            if (strServerName == "我的电脑")
                return false;

            return true;
        }



        // 获得一个.ini文件中的所有section值
        // return:
        //      -1  error
        //      0   文件不存在
        //      1   文件存在
        public static int GetIniSections(string strIniFilename,
            out List<string> sections,
            out string strError)
        {
            strError = "";
            sections = new List<string>();


            try
            {
                using (StreamReader sr = new StreamReader(strIniFilename, Encoding.GetEncoding(936)))
                {
                    for (int i = 0; ; i++)
                    {
                        string strLine = sr.ReadLine();
                        if (strLine == null)
                            break;
                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '[' && strLine[strLine.Length - 1] == ']')
                        {
                            strLine = strLine.Substring(1, strLine.Length - 2); // 去掉外围的[]
                            sections.Add(strLine);
                        }
                    }
                }
                return 1;
            }
            catch (FileNotFoundException)
            {
                strError = "文件 " + strIniFilename + " 不存在";
                return 0;
            }
            catch (Exception ex)
            {
                strError = "装载文件 " + strIniFilename + " 时发生错误: " + ex.Message;
                return -1;
            }
        }

        private void button_upgradeFromGcsIni_Click(object sender, EventArgs e)
        {
            // 询问gcs.ini原始文件全路径
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要参考的gcs.ini文件";
            dlg.FileName = "";
            dlg.Filter = "gcs.ini file (gcs.ini)|gcs.ini|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string strOutputFilename = this.CfgFilename + ".tmp";

            string strError = "";
            int nRet = UpgradeGcsIniDupCfg(dlg.FileName,
                strOutputFilename,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            // 警告覆盖
            DialogResult result = MessageBox.Show(this,
                "是否要用从文件 " + dlg.FileName + " 中获取的配置内容覆盖当前窗口中的现有配置内容?\r\n\r\n(Yes)是 -- 覆盖；(No)否 -- 不覆盖，仅在notepad中观察获取的内容；(Cancel)放弃",
                "DupCfgDialog",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Cancel)
            {
                return;
            }

            if (result == DialogResult.No)
            {
                System.Diagnostics.Process.Start("notepad", strOutputFilename);
                return;
            }

            // 确保内存修改兑现到配置文件
            if (this.Changed == true
                && this.dom != null
                && String.IsNullOrEmpty(this.CfgFilename) == false)
            {
                this.dom.Save(this.CfgFilename);
                this.Changed = false;
            }

            // 保存一个备份文件
            try
            {
                File.Copy(this.CfgFilename, this.CfgFilename + ".bak", true);
            }
            catch
            {
            }

            File.Copy(strOutputFilename, this.CfgFilename, true);

            nRet = Initial(this.CfgFilename,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 切换到方案 page
            this.tabControl_main.SelectedTab = this.tabPage_projects;

            FillProjectNameList();
            FillDefaultList();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        #endregion

        private void button_viewDupXml_Click(object sender, EventArgs e)
        {
            if (File.Exists(this.CfgFilename) == true)
                System.Diagnostics.Process.Start("notepad", this.CfgFilename);
            else 
                MessageBox.Show(this, "配置文件 " + this.CfgFilename + " 尚不存在...");

        }


    }

    class DefaultProjectRelation
    {
        public string ProjectName = "";
        public string StartDbPath = "";
    }
}