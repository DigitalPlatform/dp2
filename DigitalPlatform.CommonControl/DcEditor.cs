using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;

using DigitalPlatform.Xml;

namespace DigitalPlatform.CommonControl
{
    public partial class DcEditor : UserControl
    {
        [Category("New Event")]
        public event EventHandler SelectedIndexChanged = null;

        public XmlNamespaceManager XmlNamespaceManager = null;

        public DcElement LastClickElement = null;   // 最近一次click选择过的Element对象

        XmlDocument DataDom = null; // 数据DOM
        XmlNode DataRoot = null;    // DC容器元素
        List<XmlNode> DcNodes = null;

        int m_nInSuspend = 0;

        int m_nDisableDrawCell = 0;

        public string Lang = "zh";

        public string CfgFilename = "";

        XmlDocument CfgDom = null;

        // 元素名对照值
        public List<CaptionValue> ElementTable = null;

        // 语言对照值
        public List<CaptionValue> LanguageTable = null;


        public List<DcElement> Elements = new List<DcElement>();

        bool m_bChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {

                if (this.m_bChanged != value)
                {
                    this.m_bChanged = value;

                    if (value == false)
                        ResetLineColor();
                }
            }
        }

        public override Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;

                this.tableLayoutPanel_main.ForeColor = value;
            }
        }

        public DcEditor()
        {
            InitializeComponent();
        }

        void ResetLineColor()
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                DcElement element = this.Elements[i];
                element.State = ElementState.Normal;
            }
        }

        void OnSelectedIndexChanged()
        {
            if (this.SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, new EventArgs());
            }
        }

        // 查找符合条件的DcElement对象
        // parameters:
        //      strElementQName 类似"dc:title"的Qualified Dc element name
        //      strTypeValue    type值。如果为""，表示这里必须为空才能匹配；如果为"*", 表示通配
        //      strLangrageValue    language值。如果为""，表示这里必须为空才能匹配；如果为"*", 表示通配
        public List<DcElement> FindElements(string strElementQName,
            string strTypeValue,
            string strLanguageValue)
        {
            List<DcElement> results = new List<DcElement>();

            for (int i = 0; i < this.Elements.Count; i++)
            {
                DcElement element = this.Elements[i];

                // 
                if (String.IsNullOrEmpty(strElementQName) == true
                        || strElementQName == "[无]")
                {
                    if (this.IsBlankString(element.Element) == false)
                        continue;
                }

                if (String.IsNullOrEmpty(strElementQName) == false
                    && strElementQName != "*")
                {
                    if (element.Element != strElementQName)
                        continue;
                }

                // 
                if (String.IsNullOrEmpty(strTypeValue) == true
                        || strTypeValue == "[无]")
                {
                    if (this.IsBlankString(element.SchemeCaption) == false)
                        continue;
                }

                if (String.IsNullOrEmpty(strTypeValue) == false
                    && strTypeValue != "*")
                {
                    if (element.Scheme != strTypeValue)
                        continue;
                }

                if (String.IsNullOrEmpty(strLanguageValue) == true
                    || strLanguageValue == "[无]")
                {
                    if (element.Language != null)
                        continue;
                }

                if (String.IsNullOrEmpty(strLanguageValue) == false
                    && strLanguageValue != "*")
                {
                    // TODO: 最好支持通配符，类似"zh-*"
                    if (element.Language != strLanguageValue)
                        continue;
                }

                results.Add(element);
            }

            return results;
        }

        /*
         * 已经移入DigitalPlatform.ClipboardUtil
        public static string GetClipboardText()
        {
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.UnicodeText) == false)
                return "";
            return (string)ido.GetData(DataFormats.UnicodeText);
        }
         * */

        #region selection functions

        public void SelectAll()
        {
            bool bSelectedChanged = false;

            for (int i = 0; i < this.Elements.Count; i++)
            {
                DcElement cur_element = this.Elements[i];
                if ((cur_element.State & ElementState.Selected) == 0)
                {
                    cur_element.State |= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            if (bSelectedChanged == true)
                this.OnSelectedIndexChanged();
        }

        public List<DcElement> SelectedElements
        {
            get
            {
                List<DcElement> results = new List<DcElement>();

                for (int i = 0; i < this.Elements.Count; i++)
                {
                    DcElement cur_element = this.Elements[i];
                    if ((cur_element.State & ElementState.Selected) != 0)
                        results.Add(cur_element);
                }

                return results;
            }
        }

        public List<int> SelectedIndices
        {
            get
            {
                List<int> results = new List<int>();

                for (int i = 0; i < this.Elements.Count; i++)
                {
                    DcElement cur_element = this.Elements[i];
                    if ((cur_element.State & ElementState.Selected) != 0)
                        results.Add(i);
                }

                return results;
            }
        }

        public void SelectElement(DcElement element,
            bool bClearOld)
        {
            bool bSelectedChanged = false;

            if (bClearOld == true)
            {
                for (int i = 0; i < this.Elements.Count; i++)
                {
                    DcElement cur_element = this.Elements[i];

                    if (cur_element == element)
                        continue;   // 暂时不处理当前行

                    if ((cur_element.State & ElementState.Selected) != 0)
                    {
                        cur_element.State -= ElementState.Selected;
                        bSelectedChanged = true;
                    }
                }
            }

            // 选中当前行
            if ((element.State & ElementState.Selected) == 0)
            {
                element.State |= ElementState.Selected;
                bSelectedChanged = true;
            }

            this.LastClickElement = element;

            if (bClearOld == true)
            {
                // 看看focus是不是已经在这一行上？
                // 如果不在，则要切换过来
                if (element.IsSubControlFocused() == false)
                    element.comboBox_element.Focus();
            }

            if (bSelectedChanged == true)
                OnSelectedIndexChanged();
        }

        public void ToggleSelectElement(DcElement element)
        {
            // 选中当前行
            if ((element.State & ElementState.Selected) == 0)
                element.State |= ElementState.Selected;
            else
                element.State -= ElementState.Selected;

            this.LastClickElement = element;

            this.OnSelectedIndexChanged();
        }

        public void RangeSelectElement(DcElement element)
        {
            bool bSelectedChanged = false;

            DcElement start = this.LastClickElement;

            int nStart = this.Elements.IndexOf(start);
            if (nStart == -1)
                return;

            int nEnd = this.Elements.IndexOf(element);

            if (nStart > nEnd)
            {
                // 交换
                int nTemp = nStart;
                nStart = nEnd;
                nEnd = nTemp;
            }

            for (int i = nStart; i <= nEnd; i++)
            {
                DcElement cur_element = this.Elements[i];

                if ((cur_element.State & ElementState.Selected) == 0)
                {
                    cur_element.State |= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            // 清除其余位置
            for (int i = 0; i < nStart; i++)
            {
                DcElement cur_element = this.Elements[i];

                if ((cur_element.State & ElementState.Selected) != 0)
                {
                    cur_element.State -= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            for (int i = nEnd + 1; i < this.Elements.Count; i++)
            {
                DcElement cur_element = this.Elements[i];

                if ((cur_element.State & ElementState.Selected) != 0)
                {
                    cur_element.State -= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            if (bSelectedChanged == true)
                this.OnSelectedIndexChanged();
        }

        #endregion

        private void DcEditor_Load(object sender, EventArgs e)
        {
            /*
            this.AddLine();
            this.AddLine();
            this.AddLine();
            this.AddLine();
             * */
        }

        private void DcEditor_SizeChanged(object sender, EventArgs e)
        {
            this.DisableUpdate();
            try
            {
                tableLayoutPanel_main.Size = this.Size;
                // 重新调整textbox高度
                SetElementsHeight();
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        [Category("Data")]
        [DescriptionAttribute("Xml")]
        [DefaultValue("")]
        public string Xml
        {
            get
            {
                string strXml = "";
                string strError = "";
                int nRet = this.GetXml(out strXml,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
                return strXml;
            }
            set
            {
                string strError = "";
                int nRet = this.SetXml(value,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }
        }

        public string BlankString
        {
            get
            {
                return "[无]";
            }
        }

        public bool IsBlankString(string strText)
        {
            if (String.IsNullOrEmpty(strText) == true
                || strText == "[无]")
                return true;

            return false;
        }

        public void DisableDrawCell()
        {
            this.m_nDisableDrawCell++;
        }

        public void EnableDrawCell()
        {

            this.m_nDisableDrawCell--;
        }

        // 防止size changing时的大量闪动
        protected override void OnSizeChanged(EventArgs e)
        {
            // MessageBox.Show(this, "begin");
            this.DisableDrawCell();
            try
            {
                base.OnSizeChanged(e);
            }
            finally
            {
                this.EnableDrawCell();
                // MessageBox.Show(this, "end");
            }
        }

        public void DisableUpdate()
        {
            /*
            bool bOldVisible = this.Visible;

            this.Visible = false;

            return bOldVisible;
             * */

            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_main.SuspendLayout();
            }

            this.m_nInSuspend++;
       }

        // parameters:
        public void EnableUpdate()
        {
            this.m_nInSuspend--;

            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_main.ResumeLayout(false);
                this.tableLayoutPanel_main.PerformLayout();
            }
        }

        public void Clear()
        {
            this.DisableUpdate();

            try
            {

                for (int i = 0; i < this.Elements.Count; i++)
                {
                    DcElement element = this.Elements[i];
                    ClearOneElementControls(this.tableLayoutPanel_main,
                        element);
                }

                /*
                Line line = null;
                if (this.Lines.Count > 0)
                    line = this.Lines[0];
                 * */

                this.Elements.Clear();
                this.tableLayoutPanel_main.RowCount = 2;    // 为什么是2？
                for (; ; )
                {
                    if (this.tableLayoutPanel_main.RowStyles.Count <= 2)
                        break;
                    this.tableLayoutPanel_main.RowStyles.RemoveAt(2);
                }

            }
            finally
            {
                this.EnableUpdate();
            }
            /*
            if (line != null)
                ClearOneLineControls(this.tableLayoutPanel_main,
                    line);
             * */
        }

        // 清除一个DcElement对象对应的Control
        public void ClearOneElementControls(
            TableLayoutPanel table,
            DcElement line)
        {
            // color
            Label label = line.label_color;
            table.Controls.Remove(label);

            // element
            ComboBox element_combo = line.comboBox_element;
            table.Controls.Remove(element_combo);

            // scheme
            ComboBox scheme = line.comboBox_scheme;
            table.Controls.Remove(scheme);

            // language
            ComboBox language = line.comboBox_language;
            table.Controls.Remove(language);

            // text
            TextBox text = line.textBox_value;
            table.Controls.Remove(text);
        }

        public int SetXml(string strXml,
            out string strError)
        {
            strError = "";

            // clear lines原有内容
            this.Clear();
            this.DataDom = null;
            this.DataRoot = null;
            this.DcNodes = null;
            this.LastClickElement = null;
            this.XmlNamespaceManager = null;

            if (String.IsNullOrEmpty(strXml) == true)
                return 0;

            this.DataDom = new XmlDocument();
            try
            {
                this.DataDom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装入DOM时发生错误: " + ex.Message;
                return -1;
            }

            return this.SetXml(this.DataDom, out strError);
        }

            // 设置好名字空间环境
        int PrepareNs(out XmlNamespaceManager nsmgr,
            out List<string> prefixs,
            out List<string> uris,
            out string strDcPrefix,
            out string strDcTermsPrefix,
            out string strError)
        {
            strError = "";
            uris = null;
            prefixs = null;
            strDcPrefix = "";
            strDcTermsPrefix = "";

            nsmgr = new XmlNamespaceManager(new NameTable());

            string strUri = "";
            uris = new List<string>();
            prefixs = new List<string>();

            if (this.CfgDom != null)
            {
                XmlNodeList ns_item_nodes = this.CfgDom.DocumentElement.SelectNodes("//namespaces/item");
                for (int i = 0; i < ns_item_nodes.Count; i++)
                {
                    XmlNode item_node = ns_item_nodes[i];

                    string strPrefix = DomUtil.GetAttr(item_node, "prefix");
                    strUri = DomUtil.GetAttr(item_node, "namespaceUri");

                    nsmgr.AddNamespace(strPrefix, strUri);
                    uris.Add(strUri);
                    prefixs.Add(strPrefix);
                }
            }

            // 确保两个基本的namespace
            strUri = "http://purl.org/dc/elements/1.1/";
            strDcPrefix = nsmgr.LookupPrefix(strUri);
            if (strDcPrefix == null)
            {
                strDcPrefix = "dc";
                nsmgr.AddNamespace("dc", strUri);
                uris.Add(strUri);
                prefixs.Add(strDcPrefix);
            }

            strUri = "http://purl.org/dc/terms/";
            strDcTermsPrefix = nsmgr.LookupPrefix(strUri);
            if (strDcTermsPrefix == null)
            {
                strDcTermsPrefix = "dcterms";
                nsmgr.AddNamespace("dcterms", strUri);
                uris.Add(strUri);
                prefixs.Add(strDcTermsPrefix);
            }

            return 0;
        }

        public int SetXml(XmlDocument dom,
            out string strError)
        {
            strError = "";

            if (this.CfgDom == null)
            {
                strError = "尚未LoadCfg()";
                return -1;
            }

 			XmlNamespaceManager nsmgr = null;
            List<string> prefixs = null;
            List<string> uris = null;
            string strDcPrefix = "";
            string strDcTermsPrefix = "";
            int nRet = PrepareNs(out nsmgr,
                out prefixs,
                out uris,
                out strDcPrefix,
                out strDcTermsPrefix,
                out strError);
            if (nRet == -1)
                return -1;


            /*
            // 设置好名字空间环境
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());

            string strUri = "";
            List<string> uris = new List<string>();
            XmlNodeList ns_item_nodes = this.CfgDom.DocumentElement.SelectNodes("//namespaces/item");
            for (int i = 0; i < ns_item_nodes.Count; i++)
            {
                XmlNode item_node = ns_item_nodes[i];

                string strPrefix = DomUtil.GetAttr(item_node, "prefix");
                strUri = DomUtil.GetAttr(item_node, "namespaceUri");

                nsmgr.AddNamespace(strPrefix, strUri);
                uris.Add(strUri);
            }

            // 确保两个基本的namespace
            strUri = "http://purl.org/dc/elements/1.1/";
            string strDcPrefix = nsmgr.LookupPrefix(strUri);
            if (strDcPrefix == null)
            {
                strDcPrefix = "dc";
                nsmgr.AddNamespace("dc", strUri);
                uris.Add(strUri);
            }

            strUri = "http://purl.org/dc/terms/";
            string strDcTermPrefix = nsmgr.LookupPrefix(strUri);
            if (strDcTermPrefix == null)
            {
                strDcTermPrefix = "dcterms";
                nsmgr.AddNamespace("dcterms", strUri);
                uris.Add(strUri);
            }
             * */

            /*
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            nsmgr.AddNamespace("dcterms", "http://purl.org/dc/terms/");
             * */

            // 找到根
            XmlNode root = null;

            // 找到第一个dc或者dcterms前缀的元素
            XmlNode temp_first = dom.DocumentElement.SelectSingleNode("//"+strDcPrefix+":* | //"+strDcTermsPrefix+":*", nsmgr);
            if (temp_first == null)
            {
                strError = "没有发现任何DC元素或者修饰词";
                return 0;
            }

            root = temp_first.ParentNode;
            if (root == null)
            {
                strError = "所找到的第一个DC元素 <"+root.Name+"> 没有父(容器)对象";
                return 0;
            }

            this.DataRoot = root;   // 把容器元素保存起来

            this.DisableUpdate();

            try
            {
                // 编辑器能处理的DC元素列表。根据它可以决定GetXml返还时候要替代的位置
                List<XmlNode> oldnodes = new List<XmlNode>();

                // 遍历所有下级元素
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    XmlNode node = root.ChildNodes[i];
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    // 忽略不是定义名字空间的元素
                    if (uris.IndexOf(node.NamespaceURI) == -1)
                        continue;

                    /*
                    if (node.NamespaceURI != "http://purl.org/dc/elements/1.1/"
                        && node.NamespaceURI != "http://purl.org/dc/terms/")
                        continue;
                     * */

                    oldnodes.Add(node);

                    DcElement line = this.AppendNewElement();
                    string strPrefix = nsmgr.LookupPrefix(node.NamespaceURI);
                    if (String.IsNullOrEmpty(strPrefix) == true)
                        line.Element = node.Name;
                    else
                        line.Element = strPrefix + ":" + node.LocalName; // BUG

                    line.Value = node.InnerText;

                    string strScheme = DomUtil.GetAttr("http://www.w3.org/2001/XMLSchema-instance",
                        node,
                        "type");

                    if (String.IsNullOrEmpty(strScheme) == false)
                        line.Scheme = strScheme;

                    string strLanguage = DomUtil.GetAttr(
                        "http://www.w3.org/XML/1998/namespace",
                        node,
                        "lang");

                    if (String.IsNullOrEmpty(strLanguage) == false)
                    {
                        line.Language = strLanguage;
                    }

                }

                this.DcNodes = oldnodes;

                SetElementsHeight();

                this.XmlNamespaceManager = nsmgr;
            }
            finally
            {
                this.EnableUpdate();
            }
            return 1;
        }

#if NOOOOOOOOOOOOOOOOOO
        // 获得XML字符串
        public int GetXml(out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            if (this.Lines.Count == 0)
                return 0;

            MemoryStream s = new MemoryStream();
            XmlTextWriter w = new XmlTextWriter(s, Encoding.UTF8);

            w.WriteStartElement("", // prefix
                "metadata",
                "http://example.org/myapp/");

            XmlNamespaceManager nsmgr = null;
            List<string> prefixs = null;
            List<string> uris = null;
            string strDcPrefix = "";
            string strDcTermsPrefix = "";
            int nRet = PrepareNs(out nsmgr,
                out prefixs,
                out uris,
                out strDcPrefix,
                out strDcTermsPrefix,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(prefixs.Count == uris.Count, "");


            // 在容器元素内写入名字空间定义
            for (int i = 0; i < prefixs.Count; i++)
            {
                w.WriteAttributeString("xmlns", prefixs[i], null, uris[i]);
            }

            // 为xsi:type准备名字空间
            w.WriteAttributeString("xmlns", "xsi", null,
                "http://www.w3.org/2001/XMLSchema-instance");

            /*
            // 设置好名字空间环境
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());

            List<string> uris = new List<string>();
            XmlNodeList ns_item_nodes = this.CfgDom.DocumentElement.SelectNodes("//namespaces/item");
            for (int i = 0; i < ns_item_nodes.Count; i++)
            {
                XmlNode item_node = ns_item_nodes[i];

                string strPrefix = DomUtil.GetAttr(item_node, "prefix");
                string strUri = DomUtil.GetAttr(item_node, "namespaceUri");

                nsmgr.AddNamespace(strPrefix, strUri);
                uris.Add(strUri);

                w.WriteAttributeString("xmlns", strPrefix, null, strUri);
            }
             * */

            /*
            w.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
            w.WriteAttributeString("xmlns", "dcterms", null, "http://purl.org/dc/terms/");
             * */


            foreach (Line line in this.Lines)
            {
                string strElementName = line.ElementName;
                string strPrefix = line.Prefix;

                if (String.IsNullOrEmpty(strElementName) == true)
                {
                    if (String.IsNullOrEmpty(line.Value) == true)
                        continue;
                    else
                    {
                        strError = "格式错误：内容为 '" +line.Value+ "' 的行没有指定DC元素名";
                        return -1;
                    }
                }

                string strUri = nsmgr.LookupNamespace(strPrefix);

                // dc dcterms others
                w.WriteStartElement(
                    strElementName,
                    strUri);

                string strSchemeName = line.Scheme;

                if (String.IsNullOrEmpty(strSchemeName) == false)
                {
                    w.WriteAttributeString("xsi",
                        "type",
                        "http://www.w3.org/2001/XMLSchema-instance",
                        strSchemeName);
                }

                string strLanguageName = line.Language;
                if (String.IsNullOrEmpty(strLanguageName) == false)
                {
                    w.WriteAttributeString("xml",
                        "lang",
                        null,
                        strLanguageName);
                }
                w.WriteString(line.Value);

                w.WriteEndElement();

            }

            w.WriteEndElement();

            w.Flush();
            s.Flush();

            s.Seek(0, SeekOrigin.Begin);

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(s);
            }
            catch (Exception ex)
            {

                strError = ex.Message;
                return -1;
            }

            strXml = dom.OuterXml;

            s.Close();

            return 0;
        }

#endif 

        public int GetXml(out string strXml,
    out string strError)
        {
            strXml = "";
            strError = "";

            // 2008/3/12
            if (this.CfgDom == null && this.Elements.Count == 0)
            {
                return 0;
            }

            /*
            if (this.Lines.Count == 0)
                return 0;
             * */

            if (this.DataDom == null)
            {
                this.DataDom = new XmlDocument();
                this.DataDom.LoadXml("<metadata />");
                this.DataRoot = this.DataDom.DocumentElement;
                this.DcNodes = null;
            }

            XmlNamespaceManager nsmgr = null;
            List<string> prefixs = null;
            List<string> uris = null;
            string strDcPrefix = "";
            string strDcTermsPrefix = "";
            int nRet = PrepareNs(out nsmgr,
                out prefixs,
                out uris,
                out strDcPrefix,
                out strDcTermsPrefix,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(prefixs.Count == uris.Count, "");


            string strXmlNsUri = nsmgr.LookupNamespace("xmlns");
            // 在容器元素内写入名字空间定义
            for (int i = 0; i < prefixs.Count; i++)
            {
                string strValue = "";
                XmlAttribute attr = this.DataDom.DocumentElement.Attributes[prefixs[i], strXmlNsUri];
                if (attr != null)
                    strValue = attr.Value;
                    
                // 如果不存在相应的属性，就要建立
                if (String.IsNullOrEmpty(strValue) == true)
                {
                    XmlAttribute attr_node = this.DataDom.CreateAttribute("xmlns", prefixs[i], strXmlNsUri);
                    attr_node.Value = uris[i];
                    this.DataDom.DocumentElement.Attributes.Append(attr_node);
                }
                // w.WriteAttributeString("xmlns", prefixs[i], null, uris[i]);
            }

            /*
            // 为xsi:type准备名字空间
            w.WriteAttributeString("xmlns", "xsi", null,
                "http://www.w3.org/2001/XMLSchema-instance");
             * */

            XmlNode refTailNode = null; // 插入参考点，将插入在这个节点以前
            if (this.DataRoot.ChildNodes.Count > 0)
                refTailNode = this.DataRoot.ChildNodes[0];  // 最初定位于容器下第一个元素

            List<XmlNode> new_nodes = new List<XmlNode>();

            for(int i=0;i<this.Elements.Count; i++)
            {
                DcElement line = this.Elements[i];

                string strElementName = line.ElementName;
                string strPrefix = line.Prefix;

                if (String.IsNullOrEmpty(strElementName) == true)
                {
                    if (String.IsNullOrEmpty(line.Value) == true)
                        continue;
                    else
                    {
                        strError = "格式错误：内容为 '" + line.Value + "' 的行没有指定DC元素名";
                        return -1;
                    }
                }

                XmlNode refNode = null;

                if (this.DcNodes != null && this.DcNodes.Count > 0)
                {
                    refNode = this.DcNodes[0];
                    this.DcNodes.RemoveAt(0);
                }

                string strUri = nsmgr.LookupNamespace(strPrefix);

                XmlNode element = this.DataDom.CreateElement(nsmgr.LookupPrefix(strUri),
                    strElementName,
                    strUri);
                if (refNode != null)
                {
                    this.DataRoot.InsertAfter(element, refNode);

                    // 用完参考节点后，把它删除
                    if (refNode.ParentNode != null)
                        refNode.ParentNode.RemoveChild(refNode);

                    // 随时修正尾部参考节点，以备DcNodes数组用完后使用
                    refTailNode = element.NextSibling;
                }
                else
                {
                    if (refTailNode != null)
                        this.DataRoot.InsertBefore(element, refTailNode);
                    else
                        this.DataRoot.AppendChild(element);
                }

                new_nodes.Add(element);

                string strSchemeName = line.Scheme;

                if (String.IsNullOrEmpty(strSchemeName) == false)
                {
                    DomUtil.SetAttr(element,
                        "type",
                        "xsi",  // 2007/12/25
                        "http://www.w3.org/2001/XMLSchema-instance",
                        strSchemeName);
                    /*
                    w.WriteAttributeString("xsi",
                        "type",
                        "http://www.w3.org/2001/XMLSchema-instance",
                        strSchemeName);
                     * */
                }

                string strLanguageName = line.Language;
                if (String.IsNullOrEmpty(strLanguageName) == false)
                {
                    DomUtil.SetAttr(element,
                        "lang",
                        "xml", // 2007/12/25
                        "http://www.w3.org/XML/1998/namespace",
                        strLanguageName);

                    /*
                    w.WriteAttributeString("xml",
                        "lang",
                        null,
                        strLanguageName);
                     * */
                }

                element.InnerText = line.Value;

                /*
                w.WriteString(line.Value);
                w.WriteEndElement();
                 * */
            }

            // 删除DcNodes中残余的节点
            if (this.DcNodes != null && this.DcNodes.Count > 0)
            {
                for (int i = 0; i < this.DcNodes.Count; i++)
                {
                    XmlNode node = this.DcNodes[i];
                    if (node.ParentNode != null)
                        node.ParentNode.RemoveChild(node);
                }
            }

            this.DcNodes = new_nodes;   // 替换为本次新创建的节点

            strXml = this.DataRoot.OuterXml;

            return 0;
        }

        // 获得所选择的部分元素的XML
        public int GetFragmentXml(
            List<DcElement> selected_lines,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            if (selected_lines.Count == 0)
                return 0;

            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            XmlNamespaceManager nsmgr = null;
            List<string> prefixs = null;
            List<string> uris = null;
            string strDcPrefix = "";
            string strDcTermsPrefix = "";
            int nRet = this.PrepareNs(out nsmgr,
                out prefixs,
                out uris,
                out strDcPrefix,
                out strDcTermsPrefix,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(prefixs.Count == uris.Count, "");


            string strXmlNsUri = nsmgr.LookupNamespace("xmlns");
            // 在容器元素内写入名字空间定义
            for (int i = 0; i < prefixs.Count; i++)
            {
                string strValue = "";
                XmlAttribute attr = dom.DocumentElement.Attributes[prefixs[i], strXmlNsUri];
                if (attr != null)
                    strValue = attr.Value;

                // 如果不存在相应的属性，就要建立
                if (String.IsNullOrEmpty(strValue) == true)
                {
                    XmlAttribute attr_node = dom.CreateAttribute("xmlns", prefixs[i], strXmlNsUri);
                    attr_node.Value = uris[i];
                    dom.DocumentElement.Attributes.Append(attr_node);
                }
            }

            for (int i = 0; i < selected_lines.Count; i++)
            {
                DcElement line = selected_lines[i];

                string strElementName = line.ElementName;
                string strPrefix = line.Prefix;

                if (String.IsNullOrEmpty(strElementName) == true)
                {
                    if (String.IsNullOrEmpty(line.Value) == true)
                        continue;
                    else
                    {
                        strError = "格式错误：内容为 '" + line.Value + "' 的行没有指定DC元素名";
                        return -1;
                    }
                }

                string strUri = nsmgr.LookupNamespace(strPrefix);

                XmlNode element = dom.CreateElement(nsmgr.LookupPrefix(strUri),
                    strElementName,
                    strUri);

                dom.DocumentElement.AppendChild(element);

                string strSchemeName = line.Scheme;

                if (String.IsNullOrEmpty(strSchemeName) == false)
                {
                    DomUtil.SetAttr(element,
                        "type",
                        "xsi",
                        "http://www.w3.org/2001/XMLSchema-instance",
                        strSchemeName);
                }

                string strLanguageName = line.Language;
                if (String.IsNullOrEmpty(strLanguageName) == false)
                {
                    DomUtil.SetAttr(element,
                        "lang",
                        "xml",
                        "http://www.w3.org/XML/1998/namespace",
                        strLanguageName);

                }

                element.InnerText = line.Value;

            }

            strXml = dom.DocumentElement.InnerXml;
            return 0;
        }

        // 用片断XML中包含的元素，替换指定的若干行
        // 如果selected_lines.Count == 0，则表示从nInsertPos开始插入
        public int ReplaceElements(
            int nInsertPos,
            List<DcElement> selected_lines,
            string strFragmentXml,
            out string strError)
        {
            strError = "";
            bool bSelectedChanged = false;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strFragmentXml;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            XmlNamespaceManager nsmgr = null;
            List<string> prefixs = null;
            List<string> uris = null;
            string strDcPrefix = "";
            string strDcTermsPrefix = "";
            int nRet = PrepareNs(out nsmgr,
                out prefixs,
                out uris,
                out strDcPrefix,
                out strDcTermsPrefix,
                out strError);
            if (nRet == -1)
                return -1;

            XmlNode root = dom.DocumentElement;

            int index = 0;  // selected_lines下标 选中lines集合中的第几个

            int nTailPos = nInsertPos;   // 所处理的最后一个line对象在所有行中的位置

            this.DisableUpdate();

            try
            {

                // 遍历所有下级元素
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    XmlNode node = root.ChildNodes[i];
                    if (node.NodeType != XmlNodeType.Element)
                        continue;

                    // 忽略不是定义名字空间的元素
                    if (uris.IndexOf(node.NamespaceURI) == -1)
                        continue;

                    DcElement line = null;
                    if (selected_lines != null && index < selected_lines.Count)
                    {
                        line = selected_lines[index];
                        index++;
                    }
                    else
                    {
                        // 在最后位置后面插入
                        line = this.InsertNewElement(nTailPos);
                    }

                    // 选上修改过的line
                    line.State |= ElementState.Selected;
                    bSelectedChanged = true;

                    nTailPos = this.Elements.IndexOf(line) + 1;

                    string strPrefix = nsmgr.LookupPrefix(node.NamespaceURI);
                    if (String.IsNullOrEmpty(strPrefix) == true)
                        line.Element = node.Name;
                    else
                        line.Element = strPrefix + ":" + node.LocalName; // BUG

                    line.Value = node.InnerText;

                    string strScheme = DomUtil.GetAttr("http://www.w3.org/2001/XMLSchema-instance",
                        node,
                        "type");

                    if (String.IsNullOrEmpty(strScheme) == false)
                        line.Scheme = strScheme;

                    string strLanguage = DomUtil.GetAttr(
                        "http://www.w3.org/XML/1998/namespace",
                        node,
                        "lang");

                    if (String.IsNullOrEmpty(strLanguage) == false)
                    {
                        line.Language = strLanguage;
                    }

                    line.SetTextBoxHeight(true);
                }

                // 然后把selected_lines中多余的line删除
                if (selected_lines != null)
                {
                    for (int i = index; i < selected_lines.Count; i++)
                    {
                        this.RemoveElement(selected_lines[i]);
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }

            if (bSelectedChanged == true)
                this.OnSelectedIndexChanged();

            return 0;
        }

        void SetElementsHeight()
        {
            this.DisableUpdate();
            try
            {
                foreach (DcElement line in this.Elements)
                {
                    line.SetTextBoxHeight(true);
                }

            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public DcElement AppendNewElement()
        {
            this.DisableUpdate();   // 防止闪动。彻底解决问题。2009/10/13 

            try
            {
                // int nLastRow = this.tableLayoutPanel_main.RowCount;

                this.tableLayoutPanel_main.RowCount += 1;
                this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());

                DcElement line = new DcElement(this);

                line.AddToTable(this.tableLayoutPanel_main, this.Elements.Count + 1);

                this.Elements.Add(line);

                return line;
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public DcElement InsertNewElement(int index)
        {
            this.DisableUpdate();   // 防止闪动。彻底解决问题。2009/10/13 

            try
            {
                this.tableLayoutPanel_main.RowCount += 1;
                this.tableLayoutPanel_main.RowStyles.Insert(index + 1, new System.Windows.Forms.RowStyle());

                DcElement line = new DcElement(this);

                line.InsertToTable(this.tableLayoutPanel_main, index);

                this.Elements.Insert(index, line);

                line.State = ElementState.New;

                return line;
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public void RemoveElement(int index)
        {
            DcElement line = this.Elements[index];

            line.RemoveFromTable(this.tableLayoutPanel_main, index);

            this.Elements.Remove(line);

            if (this.LastClickElement == line)
                this.LastClickElement = null;

            this.Changed = true;
        }

        public void RemoveElement(DcElement line)
        {
            int index = this.Elements.IndexOf(line);

            if (index == -1)
                return;

            line.RemoveFromTable(this.tableLayoutPanel_main, index);

            this.Elements.Remove(line);

            if (this.LastClickElement == line)
                this.LastClickElement = null;

            this.Changed = true;
        }

        public int LoadCfg(string strCfgFilename,
            out string strError)
        {
            strError = "";
            try
            {
                this.CfgDom = new XmlDocument();
                this.CfgDom.Load(strCfgFilename);
            }
            catch (Exception ex)
            {
                strError = "配置文件 '" + strCfgFilename + "' 装载到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            // 初始化值列表
            this.ElementTable = GetElementCaptionValues(this.Lang);

            this.LanguageTable = GetLanguageCaptionValues(this.Lang);


            /*
            // 刷新已有行的list
            foreach (Line line in this.Lines)
            {
                line.FillLists();
            }*/

            this.CfgFilename = strCfgFilename;

            return 0;
        }

        public int LoadCfgCode(string strCfgCode,
            out string strError)
        {
            strError = "";
            try
            {
                this.CfgDom = new XmlDocument();
                this.CfgDom.LoadXml(strCfgCode);
            }
            catch (Exception ex)
            {
                strError = "配置代码装载到XMLDOM时发生错误: " + ex.Message;
                return -1;
            }

            // 初始化值列表
            this.ElementTable = GetElementCaptionValues(this.Lang);

            this.LanguageTable = GetLanguageCaptionValues(this.Lang);


            /*
            // 刷新已有行的list
            foreach (Line line in this.Lines)
            {
                line.FillLists();
            }*/

            this.CfgFilename = "";

            return 0;
        }

        // 填充元素名列表
        public void FillElementList(ComboBox list,
            List<CaptionValue> elements)
        {
            list.Items.Clear();
            if (elements == null)
                return;

            foreach(CaptionValue pair in elements)
            {
                list.Items.Add(pair.Caption);
            }
        }

        // 填充语言名列表
        public void FillLanguageList(ComboBox list,
            List<CaptionValue> languages)
        {
            list.Items.Clear();
            if (languages == null)
                return;

            list.Items.Add(this.BlankString);

            foreach (CaptionValue pair in languages)
            {
                list.Items.Add(pair.Caption);
            }
        }

        /*
        // 填充修饰词列表
        public void FillRefinementList(ComboBox list,
            List<CaptionValue> refinements)
        {
            list.Items.Clear();
            if (refinements == null)
                return;

            list.Items.Add("[无]"); // TODO: 需要根据各语种有所区别

            foreach (CaptionValue pair in refinements)
            {
                list.Items.Add(pair.Caption);
            }
        }
         * */

        // 填充类型列表
        public void FillTypeList(ComboBox list,
            List<CaptionValue> types)
        {
            list.Items.Clear();
            if (types == null)
                return;

            list.Items.Add(this.BlankString);

            foreach (CaptionValue pair in types)
            {
                list.Items.Add(pair.Caption);
            }
        }

        // 获得特定界面语言下的Element名称列表
        // 找语言的时候，先找精确的，找不到再找模糊的
        public List<CaptionValue> GetElementCaptionValues(string strLang)
        {

            List<CaptionValue> results = new List<CaptionValue>();
            XmlNodeList nodes = this.CfgDom.SelectNodes("//element");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strElementName = DomUtil.GetAttr(node, "name");
                string strCaption = DomUtil.GetCaption(strLang, node);

                bool bRefinement = false;

                if (node.ParentNode.Name == "refinements")
                    bRefinement = true;

                CaptionValue pair = null;

                if (strCaption == null)
                {   // 如果下面根本没有定义<caption>元素，则采用<element>元素的name属性值
                    if (String.IsNullOrEmpty(strElementName) == true)
                        continue;   // 实在没有，只好舍弃
                    pair = new CaptionValue();
                    pair.Caption = "<" + strElementName + ">";
                    if (bRefinement == true)
                        pair.Value = "  " + strCaption;
                    else
                        pair.Value = strCaption;
                    results.Add(pair);
                    continue;
                }

                pair = new CaptionValue();
                if (bRefinement == true)
                    pair.Caption = "  " + strCaption;
                else
                    pair.Caption = strCaption;

                pair.Value = strElementName;
                results.Add(pair);

            }

            return results;
        }

        // 获得特定界面语言下的language名称列表
        // 找语言的时候，先找精确的，找不到再找模糊的
        public List<CaptionValue> GetLanguageCaptionValues(string strLang)
        {

            List<CaptionValue> results = new List<CaptionValue>();
            XmlNodeList nodes = this.CfgDom.SelectNodes("//languages/item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strLanguageName = DomUtil.GetAttr(node, "name");
                string strCaption = DomUtil.GetCaption(strLang, node);

                CaptionValue pair = null;

                if (strCaption == null)
                {   // 如果下面根本没有定义<caption>元素，则采用<element>元素的name属性值
                    if (String.IsNullOrEmpty(strLanguageName) == true)
                        continue;   // 实在没有，只好舍弃
                    pair = new CaptionValue();
                    pair.Caption = "<" + strLanguageName + ">";
                    pair.Value = strCaption;
                    results.Add(pair);
                    continue;
                }

                pair = new CaptionValue();
                pair.Caption = strCaption;

                pair.Value = strLanguageName;
                results.Add(pair);

            }

            return results;
        }

        /*
        // 获得特定界面语言下的、特定Element的refinement列表
        public List<CaptionValue> GetRefinementCaptionValues(string strLang,
            string strElementValue)
        {
            List<CaptionValue> results = new List<CaptionValue>();
            XmlNodeList nodes = this.CfgDom.SelectNodes("//elements/element[@name='"+strElementValue+"']/refinements/refinement");
            for (int i = 0; i < nodes.Count; i++)
            {
                string strRefinementName = DomUtil.GetAttr(nodes[i], "name");
                string strCaption = DomUtil.GetCaption(strLang, nodes[i]);

                CaptionValue pair = null;

                if (strCaption == null)
                {   // 如果下面根本没有定义<caption>元素，则采用<refinement>元素的name属性值
                    if (String.IsNullOrEmpty(strRefinementName) == true)
                        continue;   // 实在没有，只好舍弃
                    pair = new CaptionValue();
                    pair.Caption = "<" + strRefinementName + ">";
                    pair.Value = strCaption;
                    results.Add(pair);
                    continue;
                }

                pair = new CaptionValue();
                pair.Caption = strCaption;
                pair.Value = strRefinementName;
                results.Add(pair);

            }

            return results;
        }
         * */

        // 获得特定界面语言下的、特定Element的、特定refinement的type列表
        public List<CaptionValue> GetTypeCaptionValues(string strLang,
            string strElementValue/*,
            string strRefinementValue*/)
        {
            List<CaptionValue> results = new List<CaptionValue>();
            string strXPath = "";

            strXPath = "//element[@name='" + strElementValue + "']/types/type";

            XmlNodeList nodes = this.CfgDom.SelectNodes(strXPath);
            for (int i = 0; i < nodes.Count; i++)
            {
                string strTypeName = DomUtil.GetAttr(nodes[i], "name");
                string strCaption = DomUtil.GetCaption(strLang, nodes[i]);

                CaptionValue pair = null;

                if (strCaption == null)
                {   // 如果下面根本没有定义<caption>元素，则采用<type>元素的name属性值
                    if (String.IsNullOrEmpty(strTypeName) == true)
                        continue;   // 实在没有，只好舍弃
                    pair = new CaptionValue();
                    pair.Caption = "<" + strTypeName + ">";
                    pair.Value = strCaption;
                    results.Add(pair);
                    continue;
                }

                pair = new CaptionValue();
                pair.Caption = strCaption;
                pair.Value = strTypeName;
                results.Add(pair);
            }

            return results;
        }

        // 根据element value获得Caption
        public string GetElementCaption(string strElementValue)
        {
            if (String.IsNullOrEmpty(strElementValue) == true)
                return null;    // not found

            foreach (CaptionValue item in this.ElementTable)
            {
                if (item.Value == strElementValue)
                    return item.Caption;
            }

            // return null;    // not found
            return "<" + strElementValue + ">";
        }

        // 根据element Caption获得value
        public string GetElementValue(string strElementCaption)
        {
            if (this.IsBlankString(strElementCaption) == true)
                return null;

            // 如果为原始值
            if (strElementCaption.Length > 1
                && strElementCaption[0] == '<')
                return strElementCaption.Substring(1, strElementCaption.Length - 2);

            foreach(CaptionValue item in this.ElementTable)
            {
                if (item.Caption == strElementCaption)
                    return item.Value;
            }

            return null;    // not found
        }

        ///

        // 根据language value获得Caption
        public string GetLanguageCaption(string strLanguageValue)
        {
            if (String.IsNullOrEmpty(strLanguageValue) == true)
                return null;    // not found

            foreach (CaptionValue item in this.LanguageTable)
            {
                if (item.Value == strLanguageValue)
                    return item.Caption;
            }

            // return null;    // not found
            return "<" + strLanguageValue + ">";
        }

        // 根据language Caption获得value
        public string GetLanguageValue(string strLanguageCaption)
        {
            if (this.IsBlankString(strLanguageCaption) == true)
                return null;

            // 如果为原始值
            if (strLanguageCaption.Length > 1
                && strLanguageCaption[0] == '<')
                return strLanguageCaption.Substring(1, strLanguageCaption.Length - 2);

            foreach (CaptionValue item in this.LanguageTable)
            {
                if (item.Caption == strLanguageCaption)
                    return item.Value;
            }

            return null;    // not found
        }

        /*
        // 根据refinement Caption获得value
        // TODO: 可能需要一个版本，把strElementCaption更换为strElementValue。因有许多情况，strElementValue已经获得，可以直接利用提高速度
        public string GetRefinementValue(
            string strElementCaption,
            string strRefinementCaption)
        {
            if (String.IsNullOrEmpty(strRefinementCaption) == true)
                return null;


            // 如果为原始值
            if (strRefinementCaption.Length > 1
                && strRefinementCaption[0] == '<')
                return strRefinementCaption.Substring(1, strRefinementCaption.Length - 2);

            string strElementValue = null;

            strElementValue = GetElementValue(strElementCaption);

            // 获得特定界面语言下的、特定Element的refinement列表
            List<CaptionValue> refinements = GetRefinementCaptionValues(this.Lang,
                strElementValue);

            foreach (CaptionValue item in refinements)
            {
                if (item.Caption == strRefinementCaption)
                    return item.Value;
            }

            return null;    // not found
        }
         * */


        // 根据type Caption获得value
        public string GetTypeValue(
            string strElementCaption,
            // string strRefinementCaption,
            string strTypeCaption)
        {
            if (this.IsBlankString(strTypeCaption) == true)
                return null;


            // 如果为原始值
            if (strTypeCaption.Length > 1
                && strTypeCaption[0] == '<')
                return strTypeCaption.Substring(1, strTypeCaption.Length - 2);

            string strElementValue = GetElementValue(strElementCaption);
            /*
            string strRefinementValue = GetRefinementValue(strElementCaption,
                strRefinementCaption);
             * */

            // 获得特定特定Element的type列表
            List<CaptionValue> types = this.GetTypeCaptionValues(this.Lang,
                strElementValue/*,
                strRefinementValue*/);

            foreach (CaptionValue item in types)
            {
                if (item.Caption == strTypeCaption)
                    return item.Value;
            }

            return null;    // not found
        }

        // 根据type value(还要附加所从属的element value)获得Caption
        public string GetTypeCaption(string strElementValue,
            string strTypeValue)
        {
            if (String.IsNullOrEmpty(strTypeValue) == true)
                return null;    // not found

            // 获得特定特定Element的type列表
            List<CaptionValue> types = this.GetTypeCaptionValues(this.Lang,
                strElementValue);

            foreach (CaptionValue item in types)
            {
                if (item.Value == strTypeValue)
                    return item.Caption;
            }

            return "<" + strTypeValue + ">";
        }

        // 左上角的一个label上右鼠标键popupmenu
        private void label_topleft_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bHasClipboardObject = false;
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.Text) == true)
                bHasClipboardObject = true;

            //
            menuItem = new MenuItem("后插新元素(&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("复制整个记录(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyRecord_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴替换整个记录(&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteRecord_Click);
            if (bHasClipboardObject == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.label_topleft, new Point(e.X, e.Y));
        }

        // 在最后追加一个新行
        public void NewElement()
        {
            DcElement element = this.InsertNewElement(this.Elements.Count);

            // 滚入可见范围？
            element.ScrollIntoView();

            // 选定它
            element.Select(1);
        }

        public void DeleteSelectedElements()
        {
            bool bSelectedChanged = false;

            List<DcElement> selected_lines = this.SelectedElements;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的元素");
                return;
            }
            string strText = "";

            if (selected_lines.Count == 1)
                strText = "确实要删除元素 '" + selected_lines[0].ElementCaption + "'? ";
            else
                strText = "确实要删除所选定的 " + selected_lines.Count.ToString() + " 个元素?";

            DialogResult result = MessageBox.Show(this,
                strText,
                "DcEditor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }


            this.DisableUpdate();
            try
            {
                for (int i = 0; i < selected_lines.Count; i++)
                {
                    this.RemoveElement(selected_lines[i]);
                    bSelectedChanged = true;
                }
            }
            finally
            {
                this.EnableUpdate();
            }

            if (bSelectedChanged == true)
                this.OnSelectedIndexChanged();
        }

        // 在最后追加一个新行
        void menu_appendElement_Click(object sender, EventArgs e)
        {
            // this.InsertNewElement(this.Elements.Count);

            NewElement();
        }

        // 复制整个记录
        // TODO: 复制的内容中，是否不要包含<dprms:file>元素？或者虽然此时包含，但是paste的时候不理会即可
        void menu_copyRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";
            int nRet = this.GetXml(out strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            string strOutXml = "";
            nRet = DomUtil.GetIndentXml(strXml,
                out strOutXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }


            Clipboard.SetDataObject(strOutXml);
        }

        // 粘贴替换整个记录
        // TODO: 注意当剪贴板中为片断XML时间，本函数所起作用为用这些行替换全部行(但不清除任何其他已有内容)
        void menu_pasteRecord_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = ClipboardUtil.GetClipboardText();
            int nRet = this.SetXml(strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }
        }

        private void tableLayoutPanel_main_ForeColorChanged(object sender, 
            EventArgs e)
        {
            foreach (DcElement element in this.Elements)
            {
                element.comboBox_element.ForeColor = this.tableLayoutPanel_main.ForeColor;
                element.comboBox_scheme.ForeColor = this.tableLayoutPanel_main.ForeColor;
                element.comboBox_language.ForeColor = this.tableLayoutPanel_main.ForeColor;
                element.textBox_value.ForeColor = this.tableLayoutPanel_main.ForeColor;
            }
        }

        private void tableLayoutPanel_main_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            if (this.m_nInSuspend > 0)
                return; // 防止闪动

            if (this.m_nDisableDrawCell > 0)
                return;


            // Rectangle rect = Rectangle.Inflate(e.CellBounds, -1, -1);
            Rectangle rect = e.CellBounds;
            Pen pen = new Pen(Color.FromArgb(200, 200, 200));
            e.Graphics.DrawRectangle(pen, rect);
        }

    }

    [Flags]
    public enum ElementState
    {
        Normal = 0x00,  // 普通状态
        Changed = 0x01, // 内容被修改过
        New = 0x02, // 新增的行
        Selected = 0x04,    // 被选择
    }

    public class DcElement
    {
        public DcEditor Container = null;

        // 颜色、popupmenu
        public Label label_color = null;    

        // 元素
        public ComboBox comboBox_element = null;

        /*
        // 修饰词
        public ComboBox comboBox_refinement = null;
         * */

        // 编码方案
        public ComboBox comboBox_scheme = null;

        // 语种
        public ComboBox comboBox_language = null;

        // 内容
        public TextBox textBox_value = null;

        ElementState m_state = ElementState.Normal;

        public ElementState State
        {
            get
            {
                return this.m_state;
            }
            set
            {
                if (this.m_state != value)
                {
                    this.m_state = value;
                    SetLineColor();
                }
            }
        }

        void SetLineColor()
        {
            if ((this.m_state & ElementState.Selected) != 0)
            {
                this.label_color.BackColor = SystemColors.Highlight;
                return;
            }
            if ((this.m_state & ElementState.New) != 0)
            {
                this.label_color.BackColor = Color.Yellow;
                return;
            }
            if ((this.m_state & ElementState.Changed) != 0)
            {
                this.label_color.BackColor = Color.LightGreen;
                return;
            }

            this.label_color.BackColor = SystemColors.Window;
        }


        public DcElement(DcEditor container)
        {
            this.Container = container;

            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            //label_color.MaximumSize = new Size(150, 28);
            label_color.Size = new Size(6, 28);
            //label_color.MinimumSize = new Size(50, 28);

            // element
            comboBox_element = new ComboBox();
            comboBox_element.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_element.FlatStyle = FlatStyle.Flat;
            comboBox_element.Dock = DockStyle.Fill;
            comboBox_element.MaximumSize = new Size(200, 28);
            comboBox_element.Size = new Size(150, 28);
            comboBox_element.MinimumSize = new Size(50, 28);
            comboBox_element.DropDownHeight = 300;
            comboBox_element.DropDownWidth = 300;

            comboBox_element.ForeColor = this.Container.tableLayoutPanel_main.ForeColor;

            /*
            comboBox_element.Items.AddRange(new object[] {
                "title",
                "author",
                "decription",
            });
             * */
            comboBox_element.Text = "";


            /*
            // refinement
            comboBox_refinement = new ComboBox();
            comboBox_refinement.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_refinement.FlatStyle = FlatStyle.Flat;
            comboBox_refinement.DropDownHeight = 300;
            comboBox_refinement.DropDownWidth = 300;
            comboBox_refinement.Dock = DockStyle.Fill;
            comboBox_refinement.MaximumSize = new Size(200, 28);
            comboBox_refinement.Size = new Size(150, 28);
            comboBox_refinement.MinimumSize = new Size(100, 28);
             * */

            // scheme
            comboBox_scheme = new ComboBox();
            comboBox_scheme.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_scheme.FlatStyle = FlatStyle.Flat;
            comboBox_scheme.DropDownHeight = 300;
            comboBox_scheme.DropDownWidth = 300;
            comboBox_scheme.Dock = DockStyle.Fill;
            comboBox_scheme.MaximumSize = new Size(200, 28);
            comboBox_scheme.Size = new Size(150, 28);
            comboBox_scheme.MinimumSize = new Size(50, 28);

            comboBox_scheme.ForeColor = this.Container.tableLayoutPanel_main.ForeColor;


            // language
            comboBox_language = new ComboBox();
            comboBox_language.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_language.FlatStyle = FlatStyle.Flat;
            comboBox_language.DropDownHeight = 300;
            comboBox_language.DropDownWidth = 250;
            comboBox_language.Dock = DockStyle.Fill;
            comboBox_language.MaximumSize = new Size(150, 28);
            comboBox_language.Size = new Size(100, 28);
            comboBox_language.MinimumSize = new Size(50, 28);

            comboBox_language.ForeColor = this.Container.tableLayoutPanel_main.ForeColor;

            // value
            textBox_value = new TextBox();
            textBox_value.BorderStyle = BorderStyle.None;
            textBox_value.Dock = DockStyle.Fill;
            textBox_value.MinimumSize = new Size(100, 28);
            textBox_value.MaxLength = 0;
            textBox_value.Multiline = true;
            textBox_value.Margin = new Padding(6, 3, 6, 0);

            textBox_value.ForeColor = this.Container.tableLayoutPanel_main.ForeColor;
        }

        // 元素标签
        public string ElementCaption
        {
            get
            {
                return this.comboBox_element.Text;
            }
        }

        // 元素名。由prefix和name部分组合而成
        public string Element
        {
            get
            {
                return this.Container.GetElementValue(this.comboBox_element.Text);
            }
            set
            {
                string strValue = this.Container.GetElementCaption(value);
                if (String.IsNullOrEmpty(strValue) == true)
                    this.comboBox_element.Text = "";
                else
                    this.comboBox_element.Text = strValue;

                // this.FillLists();
            }
        }

        // 元素名prefix部分
        public string Prefix
        {
            get
            {
                string strText = this.Container.GetElementValue(this.comboBox_element.Text);

                if (String.IsNullOrEmpty(strText) == true)
                    return "";

                int nRet = strText.IndexOf(":");
                if (nRet == -1)
                    return "";
                return strText.Substring(0, nRet);
            }
        }

        // 纯粹的元素名，不包含prefix
        public string ElementName
        {
            get
            {
                string strText = this.Container.GetElementValue(this.comboBox_element.Text);

                if (String.IsNullOrEmpty(strText) == true)
                    return "";

                int nRet = strText.IndexOf(":");
                if (nRet == -1)
                    return strText;
                return strText.Substring(nRet + 1);
            }
        }

        /*
        public bool IsRefinement
        {
            get
            {
                string strText = this.comboBox_element.Text;
                if (String.IsNullOrEmpty(strText) == true)
                    return false;
                if (strText[0] == ' ')
                    return true;
                return false;
            }
        }
         */

        /*
        // 修饰词标签
        public string RefinementCaption
        {
            get
            {
                return this.comboBox_refinement.Text;
            }
        }

        // 修饰词
        public string Refinement
        {
            get
            {
                return this.Container.GetRefinementValue(this.comboBox_element.Text,
                    this.comboBox_refinement.Text);
            }
        }
         * */

        // 类型标签
        public string SchemeCaption
        {
            get
            {
                return this.comboBox_scheme.Text;
            }
        }

        // 类型
        public string Scheme
        {
            get
            {
                return this.Container.GetTypeValue(this.comboBox_element.Text,
                    // this.comboBox_refinement.Text,
                    this.comboBox_scheme.Text);
            }
            set
            {
                // 2007/12/20
                string strElementValue = this.Container.GetElementValue(this.comboBox_element.Text);

                string strTypeValue = this.Container.GetTypeCaption(strElementValue,
                    value);

                this.comboBox_scheme.Text = strTypeValue;
            }
        }

        public string Language
        {
            get
            {
                return this.Container.GetLanguageValue(this.comboBox_language.Text);
            }
            set
            {
                // 2007/12/20
                string strLangrageValue = this.Container.GetLanguageCaption(value);

                this.comboBox_language.Text = strLangrageValue;
            }
        }

        public string Value
        {
            get
            {
                return this.textBox_value.Text;
            }
            set
            {
                this.textBox_value.Text = value;
                // SetTextBoxHeight();
            }
        }

        public DcElement NextElement
        {
            get
            {
                int index = this.Container.Elements.IndexOf(this);
                if (index == -1)
                    return null;

                if (index + 1 >= this.Container.Elements.Count)
                    return null;

                return this.Container.Elements[index + 1];
            }
        }

        public DcElement PrevElement
        {
            get
            {
                int index = this.Container.Elements.IndexOf(this);
                if (index == -1)
                    return null;

                if (index - 1 < 0)
                    return null;

                return this.Container.Elements[index - 1];
            }
        }

        public void SetTextBoxHeight(bool bResetHeightFirst)
        {
            TextBox textbox = this.textBox_value;

            // 先恢复最小高度
            if (bResetHeightFirst == true)
                textbox.Size = new Size(textbox.Size.Width, 28);

            bool bChangedHeight = false;
            API.SendMessage(textbox.Handle,
                API.EM_LINESCROLL,
                0,
                (int)1000);	// 0x7fffffff
            while (true)
            {
                int nFirstLine = API.GetEditFirstVisibleLine(textbox);
                if (nFirstLine != 0)
                {
                    bChangedHeight = true;
                    textbox.Size = new Size(textbox.Size.Width, textbox.Size.Height + 10);
                }
                else
                    break;
            }
            if (bChangedHeight)
            {
            }
        }

        /*
        static ComboBox CreateComboBox()
        {
            ComboBox c = new ComboBox();
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.FlatStyle = FlatStyle.Flat;
            c.Dock = DockStyle.Fill;
            c.MaximumSize = new Size(150, 28);
            c.Size = new Size(80, 28);
            c.MinimumSize = new Size(50, 28);
            c.Text = "";
        }*/

        public void AddToTable(TableLayoutPanel table,
            int nRow)
        {
            table.Controls.Add(this.label_color, 0, nRow);
            table.Controls.Add(this.comboBox_element, 1, nRow);
            // table.Controls.Add(this.comboBox_refinement, 2, nRow);
            table.Controls.Add(this.comboBox_scheme, 2, nRow);
            table.Controls.Add(this.comboBox_language, 3, nRow);
            table.Controls.Add(this.textBox_value, 4, nRow);

            AddEvents();
        }

        void AddEvents()
        {
            // events

            // label_color
            this.label_color.MouseUp -= new MouseEventHandler(label_color_MouseUp);
            this.label_color.MouseUp += new MouseEventHandler(label_color_MouseUp);

            this.label_color.MouseClick -= new MouseEventHandler(label_color_MouseClick);
            this.label_color.MouseClick += new MouseEventHandler(label_color_MouseClick);

            // element
            this.comboBox_element.TextChanged -= new EventHandler(comboBox_element_TextChanged);
            this.comboBox_element.TextChanged += new EventHandler(comboBox_element_TextChanged);

            this.comboBox_element.DropDown -= new EventHandler(comboBox_element_DropDown);
            this.comboBox_element.DropDown += new EventHandler(comboBox_element_DropDown);

            this.comboBox_element.Enter -= new EventHandler(comboBox_element_Enter);
            this.comboBox_element.Enter += new EventHandler(comboBox_element_Enter);

            // scheme
            this.comboBox_scheme.DropDown -= new EventHandler(comboBox_scheme_DropDown);
            this.comboBox_scheme.DropDown += new EventHandler(comboBox_scheme_DropDown);

            this.comboBox_scheme.TextChanged -= new EventHandler(comboBox_scheme_TextChanged);
            this.comboBox_scheme.TextChanged += new EventHandler(comboBox_scheme_TextChanged);

            this.comboBox_scheme.Enter -= new EventHandler(comboBox_scheme_Enter);
            this.comboBox_scheme.Enter += new EventHandler(comboBox_scheme_Enter);

            // language
            this.comboBox_language.DropDown -= new EventHandler(comboBox_language_DropDown);
            this.comboBox_language.DropDown += new EventHandler(comboBox_language_DropDown);

            this.comboBox_language.TextChanged -= new EventHandler(comboBox_language_TextChanged);
            this.comboBox_language.TextChanged += new EventHandler(comboBox_language_TextChanged);

            this.comboBox_language.Enter -= new EventHandler(comboBox_language_Enter);
            this.comboBox_language.Enter += new EventHandler(comboBox_language_Enter);

            // value
            this.textBox_value.KeyUp -= new KeyEventHandler(textBox_value_KeyUp);
            this.textBox_value.KeyUp += new KeyEventHandler(textBox_value_KeyUp);

            this.textBox_value.TextChanged -= new EventHandler(textBox_value_TextChanged);
            this.textBox_value.TextChanged += new EventHandler(textBox_value_TextChanged);

            this.textBox_value.Enter -= new EventHandler(textBox_value_Enter);
            this.textBox_value.Enter += new EventHandler(textBox_value_Enter);

            // 2011/2/24
            this.textBox_value.MouseWheel -= new MouseEventHandler(textBox_value_MouseWheel);
            this.textBox_value.MouseWheel += new MouseEventHandler(textBox_value_MouseWheel);
        }

        // 将鼠标滚轮转而对tablelayout起作用
        void textBox_value_MouseWheel(object sender, MouseEventArgs e)
        {
            int nValue = this.Container.tableLayoutPanel_main.VerticalScroll.Value;
            nValue -= e.Delta;
            if (nValue > this.Container.tableLayoutPanel_main.VerticalScroll.Maximum)
                nValue = this.Container.tableLayoutPanel_main.VerticalScroll.Maximum;
            if (nValue < this.Container.tableLayoutPanel_main.VerticalScroll.Minimum)
                nValue = this.Container.tableLayoutPanel_main.VerticalScroll.Minimum;

            if (this.Container.tableLayoutPanel_main.VerticalScroll.Value != nValue)
            {
                this.Container.tableLayoutPanel_main.VerticalScroll.Value = nValue;
                this.Container.tableLayoutPanel_main.PerformLayout();
            }
        }



        #region 事件

        void textBox_value_Enter(object sender, EventArgs e)
        {
            this.Container.SelectElement(this, true);
        }

        void comboBox_language_Enter(object sender, EventArgs e)
        {
            this.Container.SelectElement(this, true);
        }

        void comboBox_element_Enter(object sender, EventArgs e)
        {
            this.Container.SelectElement(this, true);
        }

        void comboBox_scheme_Enter(object sender, EventArgs e)
        {
            this.Container.SelectElement(this, true);
        }

        // 单选本元素
        // parameters:
        //      nCol    1 元素名列; 2: 类型列 3: 语言列 4: 内容列
        public void Select(int nCol)
        {

            if (nCol == 1)
            {
                this.comboBox_element.SelectAll();
                this.comboBox_element.Focus();
                return;
            }

            if (nCol == 2)
            {
                this.comboBox_scheme.SelectAll();
                this.comboBox_scheme.Focus();
                return;
            }

            if (nCol == 3)
            {
                this.comboBox_language.SelectAll();
                this.comboBox_language.Focus();
                return;
            }

            if (nCol == 4)
            {
                this.textBox_value.SelectAll();
                this.textBox_value.Focus();
                return;
            }

            this.Container.SelectElement(this, true);

        }

        // 在颜色label上单击鼠标
        void label_color_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // MessageBox.Show(this.Container, "left click");
                if (Control.ModifierKeys == Keys.Control)
                {
                    this.Container.ToggleSelectElement(this);
                }
                else if (Control.ModifierKeys == Keys.Shift)
                    this.Container.RangeSelectElement(this);
                else
                {
                    this.Container.SelectElement(this, true);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 如果当前有多重选择，则不必作什么l
                // 如果当前为单独一个选择或者0个选择，则选择当前对象
                // 这样做的目的是方便操作
                if (this.Container.SelectedIndices.Count < 2)
                {
                    this.Container.SelectElement(this, true);
                }
            }
        }


        void textBox_value_TextChanged(object sender, EventArgs e)
        {
            this.Container.Changed = true;

            if ((this.State & ElementState.New) == 0)
                this.State |= ElementState.Changed;
        }

        void comboBox_language_TextChanged(object sender, EventArgs e)
        {
            // 如果language为非空值，就必须把scheme变为空值
            if (this.Container.IsBlankString(this.comboBox_language.Text) == true)
            {
            }
            else
            {
                this.comboBox_scheme.Text = this.Container.BlankString;
            }

            if ((this.State & ElementState.New) == 0)
                this.State |= ElementState.Changed;

            this.Container.Changed = true;
        }

        void comboBox_scheme_TextChanged(object sender, EventArgs e)
        {
            // 如果scheme为非空值，就必须把language变为空值
            if (this.Container.IsBlankString(this.comboBox_scheme.Text) == true)
            {
            }
            else
            {
                this.comboBox_language.Text = this.Container.BlankString;
            }

            if ((this.State & ElementState.New) == 0)
                this.State |= ElementState.Changed;

            this.Container.Changed = true;
        }



        void comboBox_language_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_language.Items.Count == 0)
                this.FillLanguageList();
        }

        void comboBox_element_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_element.Items.Count == 0)
                this.FillElementList();
        }

        void textBox_value_KeyUp(object sender, KeyEventArgs e)
        {
            this.SetTextBoxHeight(false);
        }

        /*
        void comboBox_refinement_TextChanged(object sender, EventArgs e)
        {
            this.FillTypeList();
        }
         * */

        void comboBox_element_TextChanged(object sender, EventArgs e)
        {

            /*
            // 元素名改变，其编码方式列表也要随之改变？
            this.FillTypeList();
             * */

            // 元素名改变，其编码方式列表也要随之改变。
            // 这里清空，迫使下次dropdown的时候出新的列表
            this.comboBox_scheme.Items.Clear();

            // TODO: 当前comboBox_scheme.Text值也需要校验，看看是否还在合法列表值中。如果不在了，就要清空。
            this.Scheme = "";    // 2008/1/9

            if ((this.State & ElementState.New) == 0)
                this.State |= ElementState.Changed;

            this.Container.Changed = true;
        }

        void comboBox_scheme_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_scheme.Items.Count == 0)
                this.FillTypeList();
        }

        void label_color_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedCount = this.Container.SelectedIndices.Count;
            bool bHasClipboardObject = false;
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.Text) == true)
                bHasClipboardObject = true;

            //
            menuItem = new MenuItem("前插(&I)");
            menuItem.Click += new System.EventHandler(this.menu_insertElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("后插(&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteElements_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("剪切(&T)");
            menuItem.Click += new System.EventHandler(this.menu_cut_Click);
            if (nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copy_Click);
            if (nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴插入[前](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteInsert_Click);
            if (bHasClipboardObject == true
                && nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴插入[后](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteInsertAfter_Click);
            if (bHasClipboardObject == true
                && nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("粘贴替换(&R)");
            menuItem.Click += new System.EventHandler(this.menu_pasteReplace_Click);
            if (bHasClipboardObject == true
                && nSelectedCount > 0)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);



            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.label_color, new Point(e.X, e.Y));
        }

        #endregion

        // 剪切
        void menu_cut_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";

            List<DcElement> selected_lines = this.Container.SelectedElements;


            // 获得所选择的部分元素的XML
            int nRet = this.Container.GetFragmentXml(
                selected_lines,
                out strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.Container, strError);
                return;
            }

            Clipboard.SetDataObject(strXml);


            this.Container.DisableUpdate();
            try
            {
                for (int i = 0; i < selected_lines.Count; i++)
                {
                    this.Container.RemoveElement(selected_lines[i]);
                }
            }
            finally
            {
                this.Container.EnableUpdate();
            }
        }

        // 复制
        void menu_copy_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";
            // 获得所选择的部分元素的XML
            int nRet = this.Container.GetFragmentXml(
                this.Container.SelectedElements,
                out strXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.Container, strError);
                return;
            }

            Clipboard.SetDataObject(strXml);
        }

        // 粘贴插入[前]
        void menu_pasteInsert_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<DcElement> selected_lines = this.Container.SelectedElements;

            int nInsertPos = 0;

            if (selected_lines.Count == 0)
                nInsertPos = 0;
            else
                nInsertPos = this.Container.SelectedIndices[0];

            string strFragmentXml = ClipboardUtil.GetClipboardText();

            int nRet = this.Container.ReplaceElements(
                nInsertPos,
                null,
                strFragmentXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.Container, strError);
                return;
            }

            // 把原来选中的元素变为没有选中状态
            for (int i = 0; i < selected_lines.Count; i++)
            {
                DcElement line = selected_lines[i];
                if ((line.State & ElementState.Selected) != 0)
                    line.State -= ElementState.Selected;
            }

        }

        // 粘贴插入[后]
        void menu_pasteInsertAfter_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<DcElement> selected_lines = this.Container.SelectedElements;

            int nInsertPos = 0;

            if (selected_lines.Count == 0)
                nInsertPos = this.Container.Elements.Count;
            else
                nInsertPos = this.Container.SelectedIndices[0] + 1;

            string strFragmentXml = ClipboardUtil.GetClipboardText();

            int nRet = this.Container.ReplaceElements(
                nInsertPos,
                null,
                strFragmentXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.Container, strError);
                return;
            }

            // 把原来选中的元素变为没有选中状态
            for (int i = 0; i < selected_lines.Count; i++)
            {
                DcElement line = selected_lines[i];
                if ((line.State & ElementState.Selected) != 0)
                    line.State -= ElementState.Selected;
            }

        }


        // 粘贴替换
        void menu_pasteReplace_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strFragmentXml = ClipboardUtil.GetClipboardText();

            int nRet = this.Container.ReplaceElements(
                0,
                this.Container.SelectedElements,
                strFragmentXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this.Container, strError);
                return;
            }
        }


        // 移除本Element
        // parameters:
        //      nRow    从0开始计数
        public void RemoveFromTable(TableLayoutPanel table,
            int nRow)
        {
            this.Container.DisableUpdate();

            try
            {

                // 移除本行相关的控件
                table.Controls.Remove(this.label_color);
                table.Controls.Remove(this.comboBox_element);
                table.Controls.Remove(this.comboBox_scheme);
                table.Controls.Remove(this.comboBox_language);
                table.Controls.Remove(this.textBox_value);

                Debug.Assert(this.Container.Elements.Count ==
                    table.RowCount - 2, "");

                // 然后压缩后方的
                for (int i = (table.RowCount - 2) - 1; i >= nRow + 1; i--)
                {
                    DcElement line = this.Container.Elements[i];

                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, 0, i - 1 + 1);

                    // element
                    ComboBox element = line.comboBox_element;
                    table.Controls.Remove(element);
                    table.Controls.Add(element, 1, i - 1 + 1);

                    /*
                    // refinement
                    ComboBox refinement = line.comboBox_refinement;
                    table.Controls.Remove(refinement);
                    table.Controls.Add(refinement, 2, i - 1 + 1);
                     * */

                    // scheme
                    ComboBox scheme = line.comboBox_scheme;
                    table.Controls.Remove(scheme);
                    table.Controls.Add(scheme, 2, i - 1 + 1);

                    // language
                    ComboBox language = line.comboBox_language;
                    table.Controls.Remove(language);
                    table.Controls.Add(language, 3, i - 1 + 1);

                    // text
                    TextBox text = line.textBox_value;
                    table.Controls.Remove(text);
                    table.Controls.Add(text, 4, i - 1 + 1);

                }

                table.RowCount--;
                table.RowStyles.RemoveAt(nRow);

            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // this.FillLists();
        }


        // 插入本Line到某行。调用前，table.RowCount已经增量
        // parameters:
        //      nRow    从0开始计数
        public void InsertToTable(TableLayoutPanel table,
            int nRow)
        {
            this.Container.DisableUpdate();

            try
            {

                Debug.Assert(table.RowCount == 
                    this.Container.Elements.Count + 3, "");

                // 先移动后方的
                for (int i = (table.RowCount - 1) - 3; i >= nRow; i--)
                {
                    DcElement line = this.Container.Elements[i];

                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, 0, i + 1 + 1);

                    // element
                    ComboBox element = line.comboBox_element;
                    table.Controls.Remove(element);
                    table.Controls.Add(element, 1, i + 1 + 1);

                    /*
                    // refinement
                    ComboBox refinement = line.comboBox_refinement;
                    table.Controls.Remove(refinement);
                    table.Controls.Add(refinement, 2, i + 1 + 1);
                     * */

                    // scheme
                    ComboBox scheme = line.comboBox_scheme;
                    table.Controls.Remove(scheme);
                    table.Controls.Add(scheme, 2, i + 1 + 1);

                    // language
                    ComboBox language = line.comboBox_language;
                    table.Controls.Remove(language);
                    table.Controls.Add(language, 3, i + 1 + 1);

                    // text
                    TextBox text = line.textBox_value;
                    table.Controls.Remove(text);
                    table.Controls.Add(text, 4, i + 1 + 1);

                }

                table.Controls.Add(this.label_color, 0, nRow + 1);
                table.Controls.Add(this.comboBox_element, 1, nRow + 1);
                // table.Controls.Add(this.comboBox_refinement, 2, nRow+1);
                table.Controls.Add(this.comboBox_scheme, 2, nRow + 1);
                table.Controls.Add(this.comboBox_language, 3, nRow + 1);
                table.Controls.Add(this.textBox_value, 4, nRow + 1);

            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // this.FillLists();

            // events
            AddEvents();
        }



        // 填充元素名列表
        public void FillElementList()
        {

            // 填充element列表
            this.Container.FillElementList(this.comboBox_element,
                this.Container.ElementTable);

            // this.FillTypeList();
        }

        public void FillLanguageList()
        {
            this.Container.FillLanguageList(this.comboBox_language,
                this.Container.LanguageTable);
        }

        /*
        // 根据当前的element value，填充refinement列表
        public void FillRefinementList()
        {
            // 先获得当前的element value
            string strElementValue = this.Container.GetElementValue(this.comboBox_element.Text);

            if (strElementValue == null)
                this.comboBox_refinement.Items.Clear();
            else
            {
                List<CaptionValue> refinements = this.Container.GetRefinementCaptionValues(
                    this.Container.Lang,
                    strElementValue);

                this.Container.FillRefinementList(this.comboBox_refinement,
                    refinements);
            }

            FillTypeList();
        }*/


        // 根据当前的element value和refinement value，填充type列表
        public void FillTypeList()
        {
            // 先获得当前的element value
            string strElementValue = this.Container.GetElementValue(
                this.comboBox_element.Text);

            /*
            string strRefinementValue = this.Container.GetRefinementValue(
                this.comboBox_element.Text,
                this.comboBox_refinement.Text);
             * */

            if (strElementValue == null /*
                && strRefinementValue == null*/)
                this.comboBox_scheme.Items.Clear();
            else
            {
                List<CaptionValue> types = this.Container.GetTypeCaptionValues(
                    this.Container.Lang,
                    strElementValue/*,
                    strRefinementValue*/);

                this.Container.FillTypeList(this.comboBox_scheme,
                    types);
            }
        }

        void menu_insertElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Elements.IndexOf(this);

            if (nPos == -1)
                throw new Exception("not found myself");

            this.Container.InsertNewElement(nPos);
        }

        void menu_appendElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Elements.IndexOf(this);
            if (nPos == -1)
            {
                throw new Exception("not found myself");
            }

            this.Container.InsertNewElement(nPos + 1);
        }

        // 全选
        void menu_selectAll_Click(object sender, EventArgs e)
        {
            this.Container.SelectAll();
        }

        // 删除当前元素
        void menu_deleteElements_Click(object sender, EventArgs e)
        {
            this.Container.DeleteSelectedElements();
        }

        // 滚入可见范围
        public void ScrollIntoView()
        {
            this.Container.tableLayoutPanel_main.ScrollControlIntoView(this.comboBox_element);
        }

        // 本元素所从属的控件拥有了焦点了么?
        public bool IsSubControlFocused()
        {
            if (this.comboBox_element.Focused == true)
                return true;

            if (this.comboBox_language.Focused == true)
                return true;

            if (this.comboBox_scheme.Focused == true)
                return true;

            if (this.textBox_value.Focused == true)
                return true;

            return false;
        }
    }

    // 名字 - 值 对
    public class CaptionValue
    {
        public string Caption = "";
        public string Value = "";

    }
}
