using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Xml;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 显示并选择连接着的数据库
    /// 一个书目库和一个实体库，连接起来，共同描述了种、册信息
    /// </summary>
    public partial class GetLinkDbDlg : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public SearchPanel SearchPanel = null;

        string m_strItemDbName = "";
        string m_strBiblioDbName = "";

        XmlDocument dom = null; // global配置文件dom

        /// <summary>
        /// 构造函数
        /// </summary>
        public GetLinkDbDlg()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 服务器URL
        /// </summary>
        public string ServerUrl
        {
            get
            {
                return this.textBox_serverUrl.Text;
            }
            set
            {
                this.textBox_serverUrl.Text = value;
                dom = null; // 清除缓存的配置文件dom
            }
        }

        /// <summary>
        /// 书目库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return this.m_strBiblioDbName;
            }
            set
            {
                this.m_strBiblioDbName = value;
            }
        }

        /// <summary>
        /// 实体库名
        /// </summary>
        public string ItemDbName
        {
            get
            {
                return this.m_strItemDbName;
            }
            set
            {
                this.m_strItemDbName = value;
            }
        }

        private void GetLinkDbDlg_Load(object sender, EventArgs e)
        {

            if (this.textBox_serverUrl.Text != "")
            {
                string strError = "";
                int nRet = this.GetGlobalCfgFile(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                FillList();
            }

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView_dbs.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择书目库和实体库");
                return;
            }

            this.BiblioDbName = this.listView_dbs.SelectedItems[0].Text;
            this.ItemDbName = this.listView_dbs.SelectedItems[0].SubItems[1].Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_findServer_Click(object sender, EventArgs e)
        {
            // 选择目标服务器
            OpenResDlg dlg = new OpenResDlg();

            dlg.Text = "请选择服务器";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
            dlg.ap = this.SearchPanel.ap;
            dlg.ApCfgTitle = "getlinkdbdlg_findserver";
            dlg.Path = this.textBox_serverUrl.Text;
            dlg.Initial(this.SearchPanel.Servers,
                this.SearchPanel.Channels);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.ServerUrl = dlg.Path;

            //

            string strError = "";
            int nRet = this.GetGlobalCfgFile(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            FillList();

        }

        // 获得cfgs/global配置文件
        int GetGlobalCfgFile(out string strError)
        {
            strError = "";

            if (this.dom != null)
                return 0;	// 优化

            if (this.textBox_serverUrl.Text == "")
            {
                strError = "尚未指定服务器URL";
                return -1;
            }

            string strCfgFilePath = "cfgs/global";
            XmlDocument tempdom = null;
            // 获得配置文件
            // return:
            //		-1	error
            //		0	not found
            //		1	found
            int nRet = this.SearchPanel.GetCfgFile(
                this.textBox_serverUrl.Text,
                strCfgFilePath,
                out tempdom,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "配置文件 '" + strCfgFilePath + "' 没有找到...";
                return -1;
            }

            this.dom = tempdom;

            return 0;
        }

        void FillList()
        {
            this.listView_dbs.Items.Clear();

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dblink");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strBiblioDbName = DomUtil.GetAttr(node, "bibliodb");
                string strItemDbName = DomUtil.GetAttr(node, "itemdb");
                string strComment = DomUtil.GetAttr(node, "comment");

                ListViewItem item = new ListViewItem(strBiblioDbName);
                item.SubItems.Add(strItemDbName);
                item.SubItems.Add(strComment);

                this.listView_dbs.Items.Add(item);

                if (this.BiblioDbName == strBiblioDbName)
                    item.Selected = true;
            }
        }

        private void listView_dbs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_dbs.SelectedItems.Count == 0)
                this.button_OK.Enabled = false;
            else
                this.button_OK.Enabled = true;
        }

        private void listView_dbs_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(null, null);
        }

    }
}