using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.Script
{
    public partial class SelectInstallProjectsDialog : Form
    {
        public string Category = "";    // 允许的特别目录

        // [in]只显示哪些宿主的统计方案
        public List<string> FilterHosts = new List<string>();

        // [in]事项目录文件
        public string XmlFilename = "";

        // [in]已经安装的方案URL数组
        public List<string> InstalledUrls = new List<string>();

        // [out]用户选定的要安装的事项
        public List<ProjectItem> SelectedProjects = new List<ProjectItem>();

        public SelectInstallProjectsDialog()
        {
            InitializeComponent();
        }

        private void SelectInstallProjectsDialog_Load(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = FillList(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        private void SelectInstallProjectsDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        public static string GetHanziHostName(string strHost)
        {
            if (strHost == "OperLogStatisForm")
                return "日志统计窗";
            if (strHost == "ReaderStatisForm")
                return "读者统计窗";
            if (strHost == "ItemStatisForm")
                return "册统计窗";
            if (strHost == "OrderStatisForm")
                return "订购统计窗";
            if (strHost == "IssueStatisForm")
                return "期统计窗";
            if (strHost == "CommentStatisForm")
                return "评注统计窗";

            if (strHost == "BiblioStatisForm")
                return "书目统计窗";
            if (strHost == "XmlStatisForm")
                return "XML统计窗";
            if (strHost == "PrintOrderForm")
                return "打印订单窗";
            if (strHost == "Iso2709StatisForm")
                return "ISO2709统计窗";
            if (strHost == "Iso2709StatisForm")
                return "ISO2709统计窗";
            if (strHost == "OperHistory")
                return "操作历史";
            if (strHost == "MainForm")
                return "框架窗口";

            return strHost;
        }

        int FillList(out string strError)
        {
            strError = "";

            this.listView1.Items.Clear();

            if (string.IsNullOrEmpty(this.XmlFilename) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(this.XmlFilename);
            }
            catch (Exception ex)
            {
                strError = "装入文件 " + this.XmlFilename + " 到XMLDOM过程中出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("project");

            foreach (XmlNode node in nodes)
            {
                string strName = DomUtil.GetAttr(node, "name");
                string strHost = DomUtil.GetAttr(node, "host");
                string strUrl = DomUtil.GetAttr(node, "url");
                string strLocalFile = DomUtil.GetAttr(node, "localFile");   // 2013/5/11
                string strCategory = DomUtil.GetAttr(node, "category");

                // 用宿主名字过滤
                if (this.FilterHosts.Count > 0)
                {
                    if (this.FilterHosts.IndexOf(strHost) == -1)
                        continue;
                }

                int nImageIndex = 0;

                // 用目录过滤
                if (string.IsNullOrEmpty(strCategory) == false)
                {
                    if (MatchCategory(strCategory, this.Category) == false)
                        continue;
                    // 特殊颜色
                    nImageIndex = 1;
                }

                ListViewItem item = new ListViewItem();
                item.ImageIndex = nImageIndex;
                ListViewUtil.ChangeItemText(item, 0, strName);

                if (this.InstalledUrls.IndexOf(strUrl) != -1)
                    ListViewUtil.ChangeItemText(item, 1, "已安装");

                ListViewUtil.ChangeItemText(item, 2, GetHanziHostName(strHost));
                ListViewUtil.ChangeItemText(item, 3, strUrl);

                this.listView1.Items.Add(item);

                ProjectItem project = new ProjectItem();
                project.Name = strName;
                project.Host = strHost;
                project.Url = strUrl;
                project.FilePath = strLocalFile;
                item.Tag = project;
            }

            SetItemColor();

            return 0;
        }

        static bool MatchCategory(string s1, string s2)
        {
            string[] parts1 = s1.Split(new char[] { ',' });
            string[] parts2 = s2.Split(new char[] { ',' });

            foreach (string part1 in parts1)
            {
                string strPart1 = part1.Trim();
                if (string.IsNullOrEmpty(strPart1) == true)
                    continue;
                foreach (string part2 in parts2)
                {
                    string strPart2 = part2.Trim();
                    if (string.IsNullOrEmpty(strPart2) == true)
                        continue;

                    if (strPart1 == strPart2)
                        return true;
                }
            }
            return false;
        }

        void SetItemColor()
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                string strState = ListViewUtil.GetItemText(item, 1);
                if (strState == "已安装")
                {
                    item.ForeColor = SystemColors.GrayText;
                    item.ImageIndex = -1;
                }
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.listView1.CheckedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未(打勾)选择要安装的方案");
                return;
            }

            this.SelectedProjects.Clear();

            for (int i = 0; i < this.listView1.CheckedItems.Count; i++)
            {
                ListViewItem item = this.listView1.CheckedItems[i];

                ProjectItem project = (ProjectItem)item.Tag;
                Debug.Assert(project != null, "");

                this.SelectedProjects.Add(project);
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_selectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listView1.Items)
            {
                string strState = ListViewUtil.GetItemText(item, 1);
                if (strState == "已安装")
                    continue;
                item.Checked = true;
            }
        }

        private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // 禁止勾选已安装的事项
            ListViewItem item = this.listView1.Items[e.Index];
            string strState = ListViewUtil.GetItemText(item, 1);
            if (strState == "已安装")
            {
                if (e.NewValue == CheckState.Checked)
                    e.NewValue = CheckState.Unchecked;
            }

        }
    }

    public class ProjectItem
    {
        // 方案名
        public string Name = "";
        // 宿主名
        public string Host = "";
        // 下载源URL
        public string Url = "";

        // 2013/5/11
        // 本地 projpack 文件路径
        public string FilePath = "";
    }
}
