using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Web;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.CirculationClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 查重 属性页
    /// </summary>
    public partial class ManagerForm
    {
        bool m_bDupChanged = false;

        XmlDocument m_dup_dom = null;

        /// <summary>
        /// 查重定义是否被修改
        /// </summary>
        public bool DupChanged
        {
            get
            {
                return this.m_bDupChanged;
            }
            set
            {
                this.m_bDupChanged = value;
                if (value == true)
                    this.toolStripButton_dup_save.Enabled = true;
                else
                    this.toolStripButton_dup_save.Enabled = false;
            }
        }

        // 列出排架体系定义
        int ListDup(out string strError)
        {
            strError = "";

            if (this.DupChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内查重定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            this.listView_dup_projects.Items.Clear();
            this.listView_dup_defaults.Items.Clear();


            string strDupXml = "";

            // 获得种次号相关定义
            int nRet = GetDupInfo(out strDupXml,
                out strError);
            if (nRet == -1)
                return -1;

            this.m_dup_dom = new XmlDocument();
            this.m_dup_dom.LoadXml("<dup />");

            XmlDocumentFragment fragment = this.m_dup_dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strDupXml;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            this.m_dup_dom.DocumentElement.AppendChild(fragment);

            /*
 <dup>
        <project name="采购查重" comment="示例方案">
            <database name="测试书目库" threshold="60">
                <accessPoint name="著者" weight="50" searchStyle="" />
                <accessPoint name="题名" weight="70" searchStyle="" />
                <accessPoint name="索书类号" weight="10" searchStyle="" />
            </database>
            <database name="编目库" threshold="60">
                <accessPoint name="著者" weight="50" searchStyle="" />
                <accessPoint name="题名" weight="70" searchStyle="" />
                <accessPoint name="索书类号" weight="10" searchStyle="" />
            </database>
        </project>
        <project name="编目查重" comment="这是编目查重示例方案">
            <database name="中文图书" threshold="100">
                <accessPoint name="责任者" weight="50" searchStyle="" />
                <accessPoint name="ISBN" weight="80" searchStyle="" />
                <accessPoint name="题名" weight="20" searchStyle="" />
            </database>
            <database name="图书测试" threshold="100">
                <accessPoint name="责任者" weight="50" searchStyle="" />
                <accessPoint name="ISBN" weight="80" searchStyle="" />
                <accessPoint name="题名" weight="20" searchStyle="" />
            </database>
        </project>
        <default origin="中文图书" project="编目查重" />
        <default origin="图书测试" project="编目查重" />
    </dup>
             * * */
            FillProjectNameList(this.m_dup_dom);
            FillDefaultList(this.m_dup_dom);

            this.DupChanged = false;

            return 1;
        }


        void FillProjectNameList(XmlDocument dom)
        {
            this.listView_dup_projects.Items.Clear();

            if (dom == null)
                return;

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//project");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strComment = DomUtil.GetAttr(node, "comment");

                ListViewItem item = new ListViewItem(strName, 0);
                item.SubItems.Add(strComment);
                this.listView_dup_projects.Items.Add(item);
            }
        }


        void FillDefaultList(XmlDocument dom)
        {
            this.listView_dup_defaults.Items.Clear();

            // 获得全部<sourceDatabase>元素name属性中的发起路径
            List<string> startpaths = new List<string>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//default"); // "//defaultProject/sourceDatabase"
            for (int i = 0; i < nodes.Count; i++)
            {
                string strStartPath = DomUtil.GetAttr(nodes[i], "origin");
                if (String.IsNullOrEmpty(strStartPath) == true)
                    continue;
                startpaths.Add(strStartPath);
            }


            // 先按照查重方案定义中用到过的发起路径(数据库)列出多行
            List<string> database_names = GetAllDatabaseNames(dom);
            for (int i = 0; i < database_names.Count; i++)
            {
                string strDatabaseName = database_names[i];

                string strDefaultProject = "";
                // XmlNode nodeDefault = dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + strDatabaseName + "']");
                XmlNode nodeDefault = dom.DocumentElement.SelectSingleNode("//default[@origin='" + strDatabaseName + "']");
                if (nodeDefault != null)
                    strDefaultProject = DomUtil.GetAttr(nodeDefault, "project");

                ListViewItem item = new ListViewItem(strDatabaseName, 0);
                item.SubItems.Add(strDefaultProject);
                this.listView_dup_defaults.Items.Add(item);
                item.Tag = 1;   // 表示为实在

                // 从startpaths中移走已经用过的startpath
                startpaths.Remove(strDatabaseName);
            }

            // 再按照查重方案定义中没有用到过的发起路径列出多行
            for (int i = 0; i < startpaths.Count; i++)
            {
                string strDatabaseName = startpaths[i];

                string strDefaultProject = "";
                // XmlNode nodeDefault = dom.DocumentElement.SelectSingleNode("//defaultProject/sourceDatabase[@name='" + strDatabaseName + "']");
                XmlNode nodeDefault = dom.DocumentElement.SelectSingleNode("//default[@origin='" + strDatabaseName + "']");
                if (nodeDefault != null)
                    strDefaultProject = DomUtil.GetAttr(nodeDefault, "project");

                ListViewItem item = new ListViewItem(strDatabaseName, 0);
                item.SubItems.Add(strDefaultProject);
                this.listView_dup_defaults.Items.Add(item);
                item.Tag = null;    // 表示为发虚

                item.ForeColor = SystemColors.GrayText; // 颜色发虚，表示这个数据库名没有在查重方案定义中出现过
            }
        }

        // 获得全部的数据库名
        static List<string> GetAllDatabaseNames(XmlDocument dom)
        {
            List<string> results = new List<string>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//database");
            for (int i = 0; i < nodes.Count; i++)
            {
                results.Add(DomUtil.GetAttr(nodes[i], "name"));
            }

            results.Sort();

            // 去重
            StringUtil.RemoveDup(ref results);

            return results;
        }

        // 获得查重定义
        int GetDupInfo(out string strDupXml,
            out string strError)
        {
            strError = "";
            strDupXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取查重定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "dup",
                    out strDupXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 保存查重定义
        // parameters:
        //      strDupXml   脚本定义XML。注意，没有根元素
        int SetDupDef(string strDupXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存查重定义 ...");
            stop.BeginLoop();

            this.Update();
            this.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "dup",
                    strDupXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 提交排架体系定义修改
        int SubmitDupDef(out string strError)
        {
            strError = "";
            string strDupDef = "";
            int nRet = BuildDupDef(out strDupDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.SetDupDef(strDupDef,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 构造排架体系定义的XML片段
        // 注意是下级片断定义，没有<dup>元素作为根。
        int BuildDupDef(out string strDupDef,
            out string strError)
        {
            strError = "";
            strDupDef = "";

            strDupDef = this.m_dup_dom.DocumentElement.InnerXml;
            return 0;
        }

        // 在方案名列表中，选定一个特定的名字的行
        void SelectProjectItem(string strProjectName)
        {
            for (int i = 0; i < this.listView_dup_projects.Items.Count; i++)
            {
                ListViewItem item = this.listView_dup_projects.Items[i];
                if (item.Text == strProjectName)
                    item.Selected = true;
                else
                    item.Selected = false;
            }
        }

        // 获得全部书目库名的列表
        List<string> GetAllBiblioDbNames()
        {
            List<string> results = new List<string>();

            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
                return results;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception /*ex*/)
            {
                // strError = "XML装入DOM时出错: " + ex.Message;
                // return -1;
                Debug.Assert(false, "");
                return results;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if ("biblio" == strType)
                    results.Add(strName);
            }

            return results;
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
            for (int i = 0; i < listView_dup_defaults.Items.Count; i++)
            {
                ListViewItem item = this.listView_dup_defaults.Items[i];

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
            XmlNodeList nodes = this.m_dup_dom.DocumentElement.SelectNodes(
                "//default[@project='" + strOldProjectName + "']");
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
                    DomUtil.SetAttr(node, "project", strNewProjectName);
                }
                bChanged = true;
            }

            if (bChanged == true)
                this.DupChanged = true;
        }
    }
}
