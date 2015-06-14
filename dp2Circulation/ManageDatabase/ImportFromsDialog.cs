using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;

namespace dp2Circulation
{
    /// <summary>
    /// 用于选择导入若干数据库的From的对话框
    /// </summary>
    internal partial class ImportFromsDialog : Form
    {
        /// <summary>
        /// 系统管理窗
        /// </summary>
        public ManagerForm ManagerForm = null;

        List<string> m_dbnames = new List<string>();

        string m_strFromsXml = "";

        /// <summary>
        /// 返回表示选定的检索途径的 XML
        /// </summary>
        public string SelectedFromsXml = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        public ImportFromsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerform">系统管理窗</param>
        /// <param name="dbnames">数据库名集合</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错  0: 正常</returns>
        public int Initial(ManagerForm managerform,
            List<string> dbnames,
            out string strError)
        {
            strError = "";

            this.ManagerForm = managerform;
            this.m_dbnames = dbnames;

            // 合并后的结果
            XmlDocument dom_total = new XmlDocument();
            dom_total.LoadXml("<root />");

            // 获得全部数据库定义
            for (int i = 0; i < this.m_dbnames.Count; i++)
            {
                string strDbName = this.m_dbnames[i];

                string strOutputInfo = "";
                // 获得普通数据库定义
                int nRet = this.ManagerForm.GetDatabaseInfo(
                    strDbName,
                    out strOutputInfo,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 0)
                {
                    strError = "数据库 '" + strDbName + "' 不存在";
                    return -1;
                }

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strOutputInfo);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("from");
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];
                    string strFromStyle = DomUtil.GetAttr(node, "style");

                    // 跳过没有style属性的<from>
                    if (String.IsNullOrEmpty(strFromStyle) == true)
                        continue;

                    // 跳过“记录索引号”style。因为这个<from>元素下没有<caption>元素，会引起麻烦
                    if (strFromStyle == "recid")
                        continue;

                    XmlNode nodeExist = dom_total.DocumentElement.SelectSingleNode("from[@style='" + strFromStyle + "']");
                    if (nodeExist != null)
                    {
                        // style已经存在，对captions进行合并增补
                        MergeCaptions(nodeExist,
                            node);
                        continue;
                    }

                    // 新增
                    XmlNode new_node = dom_total.CreateElement("from");
                    dom_total.DocumentElement.AppendChild(new_node);
                    DomUtil.SetAttr(new_node, "style", strFromStyle);
                    new_node.InnerXml = node.InnerXml;
                }
            }

            this.m_strFromsXml = dom_total.DocumentElement.InnerXml;

            return 0;
        }

        // 对两个<from>元素下的若干<caption>按照语言代码去重合并
        // 方向是从nodeSource合并到nodeTarget中
        // 对语言代码是否重，有两种判断方式：一种是完全一样才叫重；一种是左边部分一样就叫重。目前采用前者，以求最全面的合并效果
        /// <summary>
        /// 对两个 from 元素下的若干 caption 元素按照语言代码去重合并
        /// 方向是从nodeSource合并到nodeTarget中
        /// 对语言代码是否重，有两种判断方式：一种是完全一样才叫重；一种是左边部分一样就叫重。目前采用前者，以求最全面的合并效果
        /// </summary>
        /// <param name="nodeTarget">目标节点</param>
        /// <param name="nodeSource">源节点</param>
        public static void MergeCaptions(XmlNode nodeTarget,
            XmlNode nodeSource)
        {
            for (int i = 0; i < nodeSource.ChildNodes.Count; i++)
            {
                XmlNode nodeSourceCaption = nodeSource.ChildNodes[i];
                if (nodeSource.NodeType != XmlNodeType.Element)
                    continue;
                if (nodeSource.Name != "caption")
                    continue;

                string strSourceLang = DomUtil.GetAttr(nodeSource, "lang");
                bool bFound = false;
                for (int j = 0; j < nodeTarget.ChildNodes.Count; j++)
                {
                    XmlNode nodeTargetCaption = nodeTarget.ChildNodes[j];
                    if (nodeTargetCaption.NodeType != XmlNodeType.Element)
                        continue;
                    if (nodeTargetCaption.Name != "caption")
                        continue;
                    string strTargetLang = DomUtil.GetAttr(nodeTarget, "lang");
                    if (strSourceLang == strTargetLang)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == true)
                    continue;

                // 增补
                XmlNode new_caption = nodeTarget.OwnerDocument.CreateElement("caption");
                nodeTarget.ParentNode.AppendChild(new_caption);

                DomUtil.SetAttr(new_caption, "lang", strSourceLang);
                new_caption.InnerText = nodeSourceCaption.InnerText;
            }
        }

        private void ImportFromsDialog_Load(object sender, EventArgs e)
        {
            this.fromEditControl1.Xml = this.m_strFromsXml;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 检查当前是否有选择事项？
            if (this.fromEditControl1.SelectedElements.Count == 0)
            {
                MessageBox.Show(this, "尚未选定任何检索途径事项");
                return;
            }

            try
            {
                this.SelectedFromsXml = this.fromEditControl1.SelectedXml;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

        }

        private void button_selectAll_Click(object sender, EventArgs e)
        {
            this.fromEditControl1.SelectAll();
        }

        private void button_unSelectAll_Click(object sender, EventArgs e)
        {
            this.fromEditControl1.ClearAllSelect();

        }

        private void fromEditControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.fromEditControl1.SelectedElements.Count > 0)
                this.button_OK.Enabled = true;
            else
                this.button_OK.Enabled = false;
        }
    }
}