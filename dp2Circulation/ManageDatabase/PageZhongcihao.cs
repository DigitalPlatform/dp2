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

using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 种次号 属性页
    /// </summary>
    public partial class ManagerForm
    {
        bool m_bZhongcihaoChanged = false;

        /// <summary>
        /// 种次号定义是否被修改
        /// </summary>
        public bool ZhongcihaoChanged
        {
            get
            {
                return this.m_bZhongcihaoChanged;
            }
            set
            {
                this.m_bZhongcihaoChanged = value;
                if (value == true)
                    this.toolStripButton_zhongcihao_save.Enabled = true;
                else
                    this.toolStripButton_zhongcihao_save.Enabled = false;
            }
        }

        static string MakeZhongcihaoGroupNodeName(string strGroupName,
    string strZhongcihaoDbName)
        {
            return "组: " + strGroupName + " 种次号库='" + strZhongcihaoDbName + "'";
        }

        static string MakeZhongcihaoNstableNodeName(string strNsTableName)
        {
            return "名字表: " + strNsTableName;
        }

        static string MakeZhongcihaoDatabaseNodeName(string strBiblioDbName)
        {
            return "书目库: " + strBiblioDbName;
        }

        int ListZhongcihao(out string strError)
        {
            strError = "";

            if (this.ZhongcihaoChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内种次号定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            this.treeView_zhongcihao.Nodes.Clear();


            string strZhongcihaoXml = "";

            // 获得种次号相关定义
            int nRet = GetZhongcihaoInfo(out strZhongcihaoXml,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<zhogncihao />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strZhongcihaoXml;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
    <zhongcihao>
        <nstable name="nstable">
            <item prefix="marc" uri="http://dp2003.com/UNIMARC" />
        </nstable>
        <group name="中文书目" zhongcihaodb="种次号">
            <database name="中文图书" leftfrom="索取类号" 

rightxpath="//marc:record/marc:datafield[@tag='905']/marc:subfield[@code='e']/text()" 

titlexpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='a']/text()" 

authorxpath="//marc:record/marc:datafield[@tag='200']/marc:subfield[@code='f' or @code='g']/text()" 

/>
        </group>
    </zhongcihao>
 * */
            XmlNodeList nstable_nodes = dom.DocumentElement.SelectNodes("nstable");
            for (int i = 0; i < nstable_nodes.Count; i++)
            {
                XmlNode node = nstable_nodes[i];

                string strNstableName = DomUtil.GetAttr(node, "name");

                string strNstableCaption = MakeZhongcihaoNstableNodeName(strNstableName);

                TreeNode nstable_treenode = new TreeNode(strNstableCaption,
                    TYPE_ZHONGCIHAO_NSTABLE, TYPE_ZHONGCIHAO_NSTABLE);
                nstable_treenode.Tag = node.OuterXml;

                this.treeView_zhongcihao.Nodes.Add(nstable_treenode);
            }

            XmlNodeList group_nodes = dom.DocumentElement.SelectNodes("group");
            for (int i = 0; i < group_nodes.Count; i++)
            {
                XmlNode node = group_nodes[i];

                string strGroupName = DomUtil.GetAttr(node, "name");
                string strZhongcihaoDbName = DomUtil.GetAttr(node, "zhongcihaodb");

                string strGroupCaption = MakeZhongcihaoGroupNodeName(strGroupName, strZhongcihaoDbName);
                TreeNode group_treenode = new TreeNode(strGroupCaption,
                    TYPE_ZHONGCIHAO_GROUP, TYPE_ZHONGCIHAO_GROUP);
                group_treenode.Tag = node.OuterXml;

                this.treeView_zhongcihao.Nodes.Add(group_treenode);

                // 加入database节点
                XmlNodeList database_nodes = node.SelectNodes("database");
                for (int j = 0; j < database_nodes.Count; j++)
                {
                    XmlNode database_node = database_nodes[j];

                    string strDatabaseName = DomUtil.GetAttr(database_node, "name");

                    string strDatabaseCaption = MakeZhongcihaoDatabaseNodeName(strDatabaseName);

                    TreeNode database_treenode = new TreeNode(strDatabaseCaption,
                        TYPE_ZHONGCIHAO_DATABASE, TYPE_ZHONGCIHAO_DATABASE);
                    database_treenode.Tag = database_node.OuterXml;

                    group_treenode.Nodes.Add(database_treenode);
                }
            }

            this.treeView_zhongcihao.ExpandAll();
            this.ZhongcihaoChanged = false;

            return 1;
        }

        // 获得种次号相关定义
        int GetZhongcihaoInfo(out string strZhongcihaoXml,
            out string strError)
        {
            strError = "";
            strZhongcihaoXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取种次号定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            var channel = this.GetChannel();

            try
            {
                long lRet = channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "zhongcihao",
                    out strZhongcihaoXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;
                return (int)lRet;
            }
            finally
            {
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }

        // 保存种次号定义
        // parameters:
        //      strZhongcihaoXml   脚本定义XML。注意，没有根元素
        int SetZhongcihaoDef(string strZhongcihaoXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存种次号定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            var channel = this.GetChannel();

            try
            {
                long lRet = channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "zhongcihao",
                    strZhongcihaoXml,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                return (int)lRet;
            }
            finally
            {
                this.ReturnChannel(channel);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                EnableControls(true);
            }

        ERROR1:
            return -1;
        }


        // 看看指定的prefix是否存在
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int ExistingPrefix(string strPrefix,
            out string strError)
        {
            strError = "";

            // 看看当前是否已经有了nstable节点
            TreeNode existing_node = FindExistNstableNode();
            if (existing_node == null)
            {
                strError = "尚未创建名字表节点";
                return -1;
            }

            string strXml = (string)existing_node.Tag;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strCurrentPrefix = DomUtil.GetAttr(node, "prefix");
                if (strPrefix == strCurrentPrefix)
                    return 1;
            }

            return 0;
        }

        // 根据名字空间URI查找对应的prefix
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int FindNamespacePrefix(string strUri,
            out string strPrefix,
            out string strError)
        {
            strPrefix = "";
            strError = "";

            // 看看当前是否已经有了nstable节点
            TreeNode existing_node = FindExistNstableNode();
            if (existing_node == null)
            {
                strError = "尚未创建名字表节点，因此无法获得URI '" + strUri + "' 所对应的prefix";
                return -1;
            }

            string strXml = (string)existing_node.Tag;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strCurrentUri = DomUtil.GetAttr(node, "uri");
                if (strUri.ToLower() == strCurrentUri.ToLower())
                {
                    strPrefix = DomUtil.GetAttr(node, "prefix");
                    return 1;
                }
            }

            return 0;
        }

        // 获得书目库的syntax
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetBiblioSyntax(string strBiblioDbName,
            out string strSyntax,
            out string strError)
        {
            strSyntax = "";
            strError = "";

            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "参数strBiblioDbName的值不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if (strName == strBiblioDbName)
                {
                    if (strType != "biblio")
                    {
                        strError = "数据库 '" + strBiblioDbName + "' 并不是书目库类型，而是 " + strType + " 类型";
                        return -1;
                    }

                    strSyntax = DomUtil.GetAttr(node, "syntax");
                    if (String.IsNullOrEmpty(strSyntax) == true)
                        strSyntax = "unimarc";

                    return 1;
                }
            }

            return 0;
        }

        // 检查指定名字的书目库是否已经创建
        // return:
        //      -2  所指定的书目库名字，实际上是一个已经存在的其他类型的库名
        //      -1  error
        //      0   还没有创建
        //      1   已经创建
        int CheckBiblioDbCreated(string strBiblioDbName,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strBiblioDbName) == true)
            {
                strError = "参数strBiblioDbName的值不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if (strType == "biblio")
                {
                    if (strName == strBiblioDbName)
                        return 1;

                    string strEntityDbName = DomUtil.GetAttr(node, "entityDbName");
                    if (strEntityDbName == strBiblioDbName)
                    {
                        strError = "所拟定的书目库名和当前已经存在的实体库名 '" + strEntityDbName + "' 相重了";
                        return -2;
                    }

                    string strOrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    if (strOrderDbName == strBiblioDbName)
                    {
                        strError = "所拟定的书目库名和当前已经存在的订购库名 '" + strOrderDbName + "' 相重了";
                        return -2;
                    }

                    string strIssueDbName = DomUtil.GetAttr(node, "issueDbName");
                    if (strIssueDbName == strBiblioDbName)
                    {
                        strError = "所拟定的书目库名和当前已经存在的期库名 '" + strIssueDbName + "' 相重了";
                        return -2;
                    }

                }

                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;

                if (strName == strBiblioDbName)
                {
                    strError = "所拟定的书目库名和当前已经存在的" + strTypeName + "库名 '" + strName + "' 相重了";
                    return -2;
                }

            }

            return 0;
        }

        // 获得表示所有书目库的名字和类型的XML代码
        // TODO: 既然列入的都是书目库，类型就是多余的了
        internal string GetAllBiblioDbInfoXml()
        {
            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
                return null;

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
                return "";
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if ("biblio" == strType)
                    continue;

                node.ParentNode.RemoveChild(node);
            }

            return dom.OuterXml;
        }


        // 获得treeview中已经使用过的全部书目库名
        // parameters:
        //      exclude_node    要排除的TreeNode节点。也就是说这个节点用过的书目库名不算在其中
        List<string> Zhongcihao_GetAllUsedBiblioDbName(TreeNode exclude_node)
        {
            List<string> existing_dbnames = new List<string>();
            for (int i = 0; i < this.treeView_zhongcihao.Nodes.Count; i++)
            {
                TreeNode tree_node = this.treeView_zhongcihao.Nodes[i];
                if (tree_node.ImageIndex != TYPE_ZHONGCIHAO_GROUP)
                    continue;

                // 进入group节点的下层
                for (int j = 0; j < tree_node.Nodes.Count; j++)
                {
                    TreeNode database_tree_node = tree_node.Nodes[j];

                    if (database_tree_node == exclude_node)
                        continue;

                    string strXml = (string)database_tree_node.Tag;

                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strXml);
                    }
                    catch (Exception /*ex*/)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    string strDatabaseName = DomUtil.GetAttr(dom.DocumentElement, "name");

                    if (String.IsNullOrEmpty(strDatabaseName) == false)
                        existing_dbnames.Add(strDatabaseName);
                }
            }

            return existing_dbnames;
        }

        TreeNode FindExistNstableNode()
        {
            for (int i = 0; i < this.treeView_zhongcihao.Nodes.Count; i++)
            {
                TreeNode node = this.treeView_zhongcihao.Nodes[i];
                if (node.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE)
                    return node;
            }

            return null;
        }

        // 提交种次号定义修改
        int SubmitZhongcihaoDef(out string strError)
        {
            strError = "";
            string strZhongcihaoDef = "";
            int nRet = BuildZhongcihaoDef(out strZhongcihaoDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.SetZhongcihaoDef(strZhongcihaoDef,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }


        // 构造种次号定义的XML片段
        // 注意是下级片断定义，没有<zhongcihao>元素作为根。
        int BuildZhongcihaoDef(out string strZhongcihaoDef,
            out string strError)
        {
            strError = "";
            strZhongcihaoDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<zhongcihao />");

            for (int i = 0; i < this.treeView_zhongcihao.Nodes.Count; i++)
            {
                TreeNode item = this.treeView_zhongcihao.Nodes[i];

                if (item.ImageIndex == TYPE_ZHONGCIHAO_NSTABLE)
                {
                    string strFragmentXml = (string)item.Tag;
                    XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                    try
                    {
                        fragment.InnerXml = strFragmentXml;
                    }
                    catch (Exception ex)
                    {
                        strError = "nstable fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                        return -1;
                    }

                    dom.DocumentElement.AppendChild(fragment);
                }
                else if (item.ImageIndex == TYPE_ZHONGCIHAO_GROUP)
                {
                    // 取得name和zhongcihaodb两个属性
                    string strXml = (string)item.Tag;

                    XmlDocument temp_dom = new XmlDocument();
                    try
                    {
                        temp_dom.LoadXml(strXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "group节点的XML装入DOM时出错: " + ex.Message;
                        return -1;
                    }

                    string strName = DomUtil.GetAttr(temp_dom.DocumentElement,
                        "name");
                    string strZhongcihaoDbName = DomUtil.GetAttr(temp_dom.DocumentElement,
                        "zhongcihaodb");

                    XmlNode group_node = dom.CreateElement("group");
                    DomUtil.SetAttr(group_node, "name", strName);
                    DomUtil.SetAttr(group_node, "zhongcihaodb", strZhongcihaoDbName);

                    dom.DocumentElement.AppendChild(group_node);

                    for (int j = 0; j < item.Nodes.Count; j++)
                    {
                        TreeNode database_treenode = item.Nodes[j];

                        string strXmlFragment = (string)database_treenode.Tag;

                        XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                        try
                        {
                            fragment.InnerXml = strXmlFragment;
                        }
                        catch (Exception ex)
                        {
                            strError = "database fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                            return -1;
                        }

                        group_node.AppendChild(fragment);
                    }
                }
            }

            strZhongcihaoDef = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 检查指定名字的种次号库是否已经创建
        // return:
        //      -2  所指定的种次号库名字，实际上是一个已经存在的其他类型的库名
        //      -1  error
        //      0   还没有创建
        //      1   已经创建
        int CheckZhongcihaoDbCreated(string strZhongcihaoDbName,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strZhongcihaoDbName) == true)
            {
                strError = "参数strZhongcihaoDbName的值不能为空";
                return -1;
            }

            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
            {
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(this.AllDatabaseInfoXml);
            }
            catch (Exception ex)
            {
                strError = "XML装入DOM时出错: " + ex.Message;
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if ("zhongcihao" == strType)
                {
                    if (strName == strZhongcihaoDbName)
                        return 1;
                }

                if (strType == "biblio")
                {
                    if (strName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的小书目库名 '" + strName + "' 相重了";
                        return -2;
                    }

                    string strEntityDbName = DomUtil.GetAttr(node, "entityDbName");
                    if (strEntityDbName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的实体库名 '" + strEntityDbName + "' 相重了";
                        return -2;
                    }

                    string strOrderDbName = DomUtil.GetAttr(node, "orderDbName");
                    if (strOrderDbName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的订购库名 '" + strOrderDbName + "' 相重了";
                        return -2;
                    }

                    string strIssueDbName = DomUtil.GetAttr(node, "issueDbName");
                    if (strIssueDbName == strZhongcihaoDbName)
                    {
                        strError = "所拟定的种次号库名和当前已经存在的期库名 '" + strIssueDbName + "' 相重了";
                        return -2;
                    }

                }

                string strTypeName = GetTypeName(strType);
                if (strTypeName == null)
                    strTypeName = strType;

                if (strName == strZhongcihaoDbName)
                {
                    strError = "所拟定的种次号库名和当前已经存在的" + strTypeName + "库名 '" + strName + "' 相重了";
                    return -2;
                }

            }

            return 0;
        }

        string GetAllZhongcihaoDbInfoXml()
        {
            if (String.IsNullOrEmpty(this.AllDatabaseInfoXml) == true)
                return null;

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
                return "";
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("database");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strName = DomUtil.GetAttr(node, "name");
                string strType = DomUtil.GetAttr(node, "type");

                if ("zhongcihao" == strType)
                    continue;

                node.ParentNode.RemoveChild(node);
            }

            return dom.OuterXml;
        }

        // 获得treeview_zhongcihao中已经使用过的全部种次号名
        // parameters:
        //      exclude_node    要排除的TreeNode节点。也就是说这个节点用过的种次号库不算在其中
        List<string> GetAllUsedZhongcihaoDbName(TreeNode exclude_node)
        {
            List<string> existing_dbnames = new List<string>();
            for (int i = 0; i < this.treeView_zhongcihao.Nodes.Count; i++)
            {
                TreeNode tree_node = this.treeView_zhongcihao.Nodes[i];
                if (tree_node.ImageIndex != TYPE_ZHONGCIHAO_GROUP)
                    continue;
                if (tree_node == exclude_node)
                    continue;

                string strXml = (string)tree_node.Tag;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception /*ex*/)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                string strZhongcihaoDbName = DomUtil.GetAttr(dom.DocumentElement, "zhongcihaodb");

                if (String.IsNullOrEmpty(strZhongcihaoDbName) == false)
                    existing_dbnames.Add(strZhongcihaoDbName);
            }

            return existing_dbnames;
        }

        void menu_zhongcihao_up_Click(object sender, EventArgs e)
        {
            ZhongcihaoMoveUpDown(true);
        }

        void menu_zhongcihao_down_Click(object sender, EventArgs e)
        {
            ZhongcihaoMoveUpDown(false);
        }

        void ZhongcihaoMoveUpDown(bool bUp)
        {
            string strError = "";
            // int nRet = 0;

            // 当前已选择的node
            if (this.treeView_zhongcihao.SelectedNode == null)
            {
                MessageBox.Show("尚未选择要进行上下移动的节点");
                return;
            }

            TreeNodeCollection nodes = null;

            TreeNode parent = treeView_zhongcihao.SelectedNode.Parent;

            if (parent == null)
                nodes = this.treeView_zhongcihao.Nodes;
            else
                nodes = parent.Nodes;

            TreeNode node = treeView_zhongcihao.SelectedNode;

            int index = nodes.IndexOf(node);

            Debug.Assert(index != -1, "");

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "已经到头";
                    goto ERROR1;
                }

                nodes.Remove(node);
                index--;
                nodes.Insert(index, node);
            }
            if (bUp == false)
            {
                if (index >= nodes.Count - 1)
                {
                    strError = "已经到尾";
                    goto ERROR1;
                }

                nodes.Remove(node);
                index++;
                nodes.Insert(index, node);

            }

            this.treeView_zhongcihao.SelectedNode = node;


            this.ZhongcihaoChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

    }
}
