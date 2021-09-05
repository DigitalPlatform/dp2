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
    /// 排架体系 属性页
    /// </summary>
    public partial class ManagerForm
    {
        bool m_bArrangementChanged = false;

        /// <summary>
        /// 排架体系定义是否被修改
        /// </summary>
        public bool ArrangementChanged
        {
            get
            {
                return this.m_bArrangementChanged;
            }
            set
            {
                this.m_bArrangementChanged = value;
                if (value == true)
                    this.toolStripButton_arrangement_save.Enabled = true;
                else
                    this.toolStripButton_arrangement_save.Enabled = false;
            }
        }

        static string MakeArrangementGroupNodeName(string strGroupName,
            string strClassType,
            string strQufenhaoType,
            string strZhongcihaoDbName,
            string strCallNumberStyle)
        {
            string strResult = "排架体系: " + strGroupName + " 类号=" + strClassType + " 区分号=" + strQufenhaoType;

            if (String.IsNullOrEmpty(strZhongcihaoDbName) == false)
                strResult += " 种次号库='" + strZhongcihaoDbName + "'";

            if (string.IsNullOrEmpty(strCallNumberStyle) == false)
                strResult += " 索取号形态='" + strCallNumberStyle + "'";

            return strResult;
        }

        static string MakeArrangementLocationNodeName(string strLocationName)
        {
            /*
            if (String.IsNullOrEmpty(strLocationName) == true)
                return "<空>";

            return strLocationName;
             * */
            return ArrangementLocationDialog.GetDisplayString(strLocationName);
        }

        // 列出排架体系定义
        int ListArrangement(out string strError)
        {
            strError = "";

            if (this.ArrangementChanged == true)
            {
                // 警告尚未保存
                DialogResult result = MessageBox.Show(this,
                    "当前窗口内排架体系定义被修改后尚未保存。若此时刷新窗口内容，现有未保存信息将丢失。\r\n\r\n确实要刷新? ",
                    "ManagerForm",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return 0;
                }
            }

            this.treeView_arrangement.Nodes.Clear();

            string strArrangementXml = "";

            // 获得种次号相关定义
            int nRet = GetArrangementInfo(out strArrangementXml,
                out strError);
            if (nRet == -1)
                return -1;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<callNumber />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strArrangementXml;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            /*
    <callNumber>
        <group name="中文" classType="中图法" qufenhaoType="GCAT" zhongcihaodb="种次号">
            <location name="基藏库" />
            <location name="流通库" />
        </group>
        <group name="英文" classType="科图法" qufenhaoType="zhongcihao" zhongcihaodb="新种次号库">
            <location name="英文基藏库" />
            <location name="英文流通库" />
        </group>
    </callNumber>
 * */
            XmlNodeList group_nodes = dom.DocumentElement.SelectNodes("group");
            for (int i = 0; i < group_nodes.Count; i++)
            {
                XmlNode node = group_nodes[i];

                string strGroupName = DomUtil.GetAttr(node, "name");
                string strClassType = DomUtil.GetAttr(node, "classType");
                string strQufenhaoType = DomUtil.GetAttr(node, "qufenhaoType");
                string strZhongcihaoDbName = DomUtil.GetAttr(node, "zhongcihaodb");
                string strCallNumberStyle = DomUtil.GetAttr(node, "callNumberStyle");

                string strGroupCaption = MakeArrangementGroupNodeName(strGroupName,
                    strClassType,
                    strQufenhaoType,
                    strZhongcihaoDbName,
                    strCallNumberStyle);
                TreeNode group_treenode = new TreeNode(strGroupCaption,
                    TYPE_ARRANGEMENT_GROUP, TYPE_ARRANGEMENT_GROUP);
                group_treenode.Tag = node.OuterXml;

                this.treeView_arrangement.Nodes.Add(group_treenode);

                // 加入location节点
                XmlNodeList location_nodes = node.SelectNodes("location");
                for (int j = 0; j < location_nodes.Count; j++)
                {
                    XmlNode location_node = location_nodes[j];

                    string strLocationName = DomUtil.GetAttr(location_node, "name");

                    string strLocationCaption = MakeArrangementLocationNodeName(strLocationName);

                    TreeNode location_treenode = new TreeNode(strLocationCaption,
                        TYPE_ARRANGEMENT_LOCATION, TYPE_ARRANGEMENT_LOCATION);
                    location_treenode.Tag = location_node.OuterXml;

                    group_treenode.Nodes.Add(location_treenode);
                }
            }

            this.treeView_arrangement.ExpandAll();
            this.ArrangementChanged = false;

            return 1;
        }

        // 获得排架体系相关定义
        int GetArrangementInfo(out string strArrangementXml,
            out string strError)
        {
            strError = "";
            strArrangementXml = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在获取排架体系定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.GetSystemParameter(
                    stop,
                    "circulation",
                    "callNumber",
                    out strArrangementXml,
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

        // 保存排架体系定义
        // parameters:
        //      strArrangementXml   脚本定义XML。注意，没有根元素
        int SetArrangementDef(string strArrangementXml,
            out string strError)
        {
            strError = "";

            EnableControls(false);

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在保存排架体系定义 ...");
            stop.BeginLoop();

            this.Update();
            Program.MainForm.Update();

            try
            {
                long lRet = Channel.SetSystemParameter(
                    stop,
                    "circulation",
                    "callNumber",
                    strArrangementXml,
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
        int SubmitArrangementDef(out string strError)
        {
            strError = "";
            string strArrangementDef = "";
            int nRet = BuildArrangementDef(out strArrangementDef,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = this.SetArrangementDef(strArrangementDef,
                out strError);
            if (nRet == -1)
                return -1;

            Program.MainForm.GetCallNumberInfo();  // 2009/6/5 刷新内存中残留的旧定义信息
            return 0;
        }

        // 构造排架体系定义的XML片段
        // 注意是下级片断定义，没有<callNumber>元素作为根。
        int BuildArrangementDef(out string strArrangementDef,
            out string strError)
        {
            strError = "";
            strArrangementDef = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<callNumber />");

            for (int i = 0; i < this.treeView_arrangement.Nodes.Count; i++)
            {
                TreeNode item = this.treeView_arrangement.Nodes[i];

                if (item.ImageIndex == TYPE_ARRANGEMENT_GROUP)
                {
                    // 取得name/classType/qufenhaoType/zhongcihaodb属性
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
                    string strClassType = DomUtil.GetAttr(temp_dom.DocumentElement,
                        "classType");
                    string strQufenhaoType = DomUtil.GetAttr(temp_dom.DocumentElement,
                        "qufenhaoType");
                    string strZhongcihaoDbName = DomUtil.GetAttr(temp_dom.DocumentElement,
                        "zhongcihaodb");
                    string strCallNumberStyle = DomUtil.GetAttr(temp_dom.DocumentElement,
    "callNumberStyle");

                    XmlNode group_node = dom.CreateElement("group");
                    DomUtil.SetAttr(group_node, "name", strName);
                    DomUtil.SetAttr(group_node, "classType", strClassType);
                    DomUtil.SetAttr(group_node, "qufenhaoType", strQufenhaoType);
                    DomUtil.SetAttr(group_node, "zhongcihaodb", strZhongcihaoDbName);
                    DomUtil.SetAttr(group_node, "callNumberStyle", strCallNumberStyle);

                    dom.DocumentElement.AppendChild(group_node);

                    for (int j = 0; j < item.Nodes.Count; j++)
                    {
                        TreeNode location_treenode = item.Nodes[j];

                        string strXmlFragment = (string)location_treenode.Tag;

                        XmlDocumentFragment fragment = dom.CreateDocumentFragment();
                        try
                        {
                            fragment.InnerXml = strXmlFragment;
                        }
                        catch (Exception ex)
                        {
                            strError = "location fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                            return -1;
                        }

                        group_node.AppendChild(fragment);
                    }
                }
            }

            strArrangementDef = dom.DocumentElement.InnerXml;

            return 0;
        }

        // 获得treeview_arrangement中已经使用过的全部种次号名
        // parameters:
        //      exclude_node    要排除的TreeNode节点。也就是说这个节点用过的种次号库不算在其中
        List<string> GetArrangementAllUsedZhongcihaoDbName(TreeNode exclude_node)
        {
            List<string> existing_dbnames = new List<string>();
            for (int i = 0; i < this.treeView_arrangement.Nodes.Count; i++)
            {
                TreeNode tree_node = this.treeView_arrangement.Nodes[i];
                if (tree_node.ImageIndex != TYPE_ARRANGEMENT_GROUP)
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



        // 获得treeview_arrangement中已经使用过的location名
        // parameters:
        //      exclude_node    要排除的TreeNode节点。也就是说这个节点用过的location不算在其中
        List<string> GetArrangementAllUsedLocationName(TreeNode exclude_node)
        {
            List<string> existing_locationnames = new List<string>();
            for (int i = 0; i < this.treeView_arrangement.Nodes.Count; i++)
            {
                TreeNode tree_node = this.treeView_arrangement.Nodes[i];
                if (tree_node.ImageIndex != TYPE_ARRANGEMENT_GROUP)
                    continue;

                // 进入group节点的下层
                for (int j = 0; j < tree_node.Nodes.Count; j++)
                {
                    TreeNode location_tree_node = tree_node.Nodes[j];

                    if (location_tree_node == exclude_node)
                        continue;

                    string strXml = (string)location_tree_node.Tag;

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

                    string strLocationName = DomUtil.GetAttr(dom.DocumentElement, "name");

                    existing_locationnames.Add(strLocationName);    // 空也是允许的
                }
            }

            return existing_locationnames;
        }


        void menu_arrangement_up_Click(object sender, EventArgs e)
        {
            ArrangementMoveUpDown(true);
        }

        void menu_arrangement_down_Click(object sender, EventArgs e)
        {
            ArrangementMoveUpDown(false);
        }

        void ArrangementMoveUpDown(bool bUp)
        {
            string strError = "";
            // int nRet = 0;

            // 当前已选择的node
            if (this.treeView_arrangement.SelectedNode == null)
            {
                MessageBox.Show("尚未选择要进行上下移动的节点");
                return;
            }

            TreeNodeCollection nodes = null;

            TreeNode parent = treeView_arrangement.SelectedNode.Parent;

            if (parent == null)
                nodes = this.treeView_arrangement.Nodes;
            else
                nodes = parent.Nodes;

            TreeNode node = treeView_arrangement.SelectedNode;

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

            this.treeView_arrangement.SelectedNode = node;


            this.ArrangementChanged = true;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
