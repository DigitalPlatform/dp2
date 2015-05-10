using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

using DigitalPlatform.Xml;

namespace dp2Manager
{
    public partial class QuickSetDatabaseRightsDlg : Form
    {
        XmlDocument cfgDom = new XmlDocument();

        public string CfgFileName = "";	// 配置文件名

        public QuickRights QuickRights = null;	// 返回选择的权限参数

        public bool AllDatabases = false;	// 返回是否选择了针对全部数据库

        public List<ObjectInfo> AllObjectNames = new List<ObjectInfo>();
        public List<ObjectInfo> SelectedObjectNames = new List<ObjectInfo>();


        public QuickSetDatabaseRightsDlg()
        {
            InitializeComponent();
        }

        private void QuikSetDatabaseRightsDlg_Load(object sender, EventArgs e)
        {
            string strError = "";

            FillDatabaseNameList();

            int nRet = FillList(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            // 选择第一项
            if (this.listView_style.Items.Count >= 1)
                this.listView_style.Items[0].Selected = true;
        }

        private int FillList(out string strError)
        {
            strError = "";
            try
            {
                cfgDom.Load(this.CfgFileName);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            this.listView_style.Items.Clear();
            XmlNodeList nodes = cfgDom.DocumentElement.SelectNodes("style");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strName = DomUtil.GetAttr(node, "name");
                string strComment = DomUtil.GetAttr(node, "comment");

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strComment);

                this.listView_style.Items.Add(item);
            }

            return 0;
        }

        private void FillDatabaseNameList()
        {
            this.listView_objectNames.Items.Clear();

            List<ObjectInfo> Selected = new List<ObjectInfo>();
            Selected.AddRange(this.SelectedObjectNames);

            for (int i = 0; i < this.AllObjectNames.Count; i++)
            {
                ObjectInfo objectinfo = this.AllObjectNames[i];

                ListViewItem item = new ListViewItem(objectinfo.Path, objectinfo.ImageIndex);

                this.listView_objectNames.Items.Add(item);

                for (int j = 0; j < Selected.Count; j++)
                {
                    ObjectInfo selectedobjectinfo = Selected[j];

                    if (objectinfo.Path == selectedobjectinfo.Path)
                    {
                        item.Selected = true;
                        Selected.RemoveAt(j);
                        break;
                    }
                }
            }

            for (int j = 0; j < Selected.Count; j++)
            {
                ObjectInfo selectedobjectinfo = Selected[j];

                ListViewItem item = new ListViewItem(selectedobjectinfo.Path,
                    selectedobjectinfo.ImageIndex);
                this.listView_objectNames.Items.Insert(j, item);
                item.Selected = true;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.listView_style.SelectedItems.Count == 0)
            {
                strError = "尚未选定风格名";
                goto ERROR1;
            }

            if (this.listView_objectNames.SelectedItems.Count == 0)
            {
                strError = "尚未选定要针对的用户名";
                goto ERROR1;
            }

            string strName = this.listView_style.SelectedItems[0].Text;

            int nRet = QuickRights.Build(this.cfgDom,
                strName,
                out this.QuickRights,
                out strError);
            if (nRet == -1)
                goto ERROR1;


            /*
            string strXPath = "//style[@name='" + strName + "']";
            XmlNode parent = this.cfgDom.DocumentElement.SelectSingleNode(strXPath);
            if (parent == null)
            {
                MessageBox.Show(this, "dom出错");
                return;
            }

            this.QuickRights = new QuickRights();

            XmlNodeList nodes = parent.SelectNodes("rights");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                QuickRightsItem item = new QuickRightsItem();
                item.Type = DomUtil.GetAttr(node, "type");
                item.Name = DomUtil.GetAttr(node, "name");
                item.Rights = DomUtil.GetNodeText(node);
                item.Style = QuickRights.GetStyleInt(DomUtil.GetAttr(node, "style"));

                this.QuickRights.Add(item);

            }
             */

            /*
            this.QuickRights.ServerRights = DomUtil.GetElementText(parent, "rights[@name='server']");
            this.QuickRights.DatabaseRights = DomUtil.GetElementText(parent, "rights[@name='database']");
            this.QuickRights.DirectoryRights = DomUtil.GetElementText(parent, "rights[@name='directory']");
            this.QuickRights.FileRights = DomUtil.GetElementText(parent, "rights[@name='file']");
             */


            // 收集已经选择的数据库名
            this.SelectedObjectNames.Clear();
            for (int i = 0; i < this.listView_objectNames.SelectedItems.Count; i++)
            {
                ObjectInfo objectinfo = new ObjectInfo();
                objectinfo.Path = this.listView_objectNames.SelectedItems[i].Text;
                objectinfo.ImageIndex = this.listView_objectNames.SelectedItems[i].ImageIndex;
                this.SelectedObjectNames.Add(objectinfo);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void listView_objectNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 是否全选
            if (this.listView_objectNames.SelectedItems.Count == this.listView_objectNames.Items.Count)
            {
                if (this.radioButton_allObjects.Checked != true)
                {
                    this.radioButton_allObjects.Checked = true;
                }
                return;
            }

            this.radioButton_selectedObjects.Checked = true;
        }

        private void radioButton_allObjects_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_allObjects.Checked == true)
            {
                // 全选
                for (int i = 0; i < this.listView_objectNames.Items.Count; i++)
                {
                    if (this.listView_objectNames.Items[i].Selected != true)
                        this.listView_objectNames.Items[i].Selected = true;
                }
            }
        }

        private void radioButton_selectedObjects_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}