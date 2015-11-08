using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;

namespace dp2Catalog
{
    public partial class AmazonSearchParametersControl : UserControl
    {
        public string Lang = "zh-CN";

        public XmlDocument CfgDom = null;

        List<Item> Items = new List<Item>();

        string m_strSearchIndex = "All";
        public string SearchIndex
        {
            get
            {
                return this.m_strSearchIndex;
            }
            set
            {
                this.m_strSearchIndex = value;

                // 更新所有表格行，并尽量维持原来的事项值

                string strError = "";
                int nRet = BuildItems(value,
                    out strError);
                if (nRet == -1)
                    throw new ArgumentException(strError);
            }
        }

        public AmazonSearchParametersControl()
        {
            InitializeComponent();
        }

        // 清除一个Item对象对应的Control
        void ClearOneItemControls(
            TableLayoutPanel table,
            Item line)
        {
            table.Controls.Remove(line.label);

            if (line.textBox != null)
                table.Controls.Remove(line.textBox);

            if (line.comboBox != null)
                table.Controls.Remove(line.comboBox);
        }

        public void Clear()
        {
            this.DisableUpdate();

            try
            {

                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item element = this.Items[i];
                    ClearOneItemControls(this.tableLayoutPanel_content,
                        element);
                }

                this.Items.Clear();
                if (this.tableLayoutPanel_content.RowCount > 1)
                {
                    this.tableLayoutPanel_content.RowCount = 1;
                    for (; ; )
                    {
                        if (this.tableLayoutPanel_content.RowStyles.Count <= 1)
                            break;
                        this.tableLayoutPanel_content.RowStyles.RemoveAt(1);
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        // 构造参数表
        public void BuildParameterTable(ref IDictionary<string, string> table)
        {
            if (table == null)
                table = new ParameterTable();

            foreach (Item item in this.Items)
            {
                string strName = item.Name;
                string strValue = item.Value;
                if (string.IsNullOrEmpty(strValue) == false)
                {
                    if (item.ItemType == ItemType.ComboBox)
                    {
                        string strLeft = "";
                        string strRight = "";
                        AmazonQueryControl.ParseLeftRight(strValue,
                            out strLeft,
                            out strRight);
                        if (string.IsNullOrEmpty(strRight) == false)
                            table[strName] = strRight;
                        else
                        {
                            // TODO: 警告？
                        }
                    }
                    else if (item.ItemType == ItemType.TextBox)
                        table[strName] = strValue;
                    else
                        table[strName] = strValue;
                }
            }
        }

        // 从配置文件中获得一个 SearchIndex 名称对应的 Caption
        public string GetSearchIndexCaption(string strName,
            string strLang)
        {
            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return null;

            // 找到 <searchIndex> 元素
            XmlNode node = this.CfgDom.DocumentElement.SelectSingleNode("searchIndexCollection/searchIndex[@name='"+strName+"']");
            if (node == null)
                return null;

            XmlElement element = (XmlElement)node;

            // 获得特定语言的 Caption
            return DomUtil.GetCaption(strLang, element);
        }

        // 从配置文件中获得所有 SearchIndex 名称
        public List<string> GetSearchIndexNames(string strLang)
        {
            List<string> results = new List<string>();
            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return results;

            // 找到 <searchIndex> 元素
            XmlNodeList nodes = this.CfgDom.DocumentElement.SelectNodes("searchIndexCollection/searchIndex");
            foreach (XmlNode node in nodes)
            {
                XmlElement element = (XmlElement)node;
                string strName = element.GetAttribute("name");

                // 获得特定语言的 Caption
                string strCaption = DomUtil.GetCaption(strLang, element);
                if (string.IsNullOrEmpty(strCaption) == false)
                    strName = strCaption + "\t" + strName;
                else
                    strName = "\t" + strName;

                results.Add(strName);
            }

            return results;
        }

        IDictionary<string, string> m_oldValues = new ParameterTable();  // 累积保存以前的值

        // 构造所有事项
        int BuildItems(string strSearchIndex,
            out string strError)
        {
            strError = "";

            // 先保存现有的值内容
            this.BuildParameterTable(ref m_oldValues);

            this.Clear();

            if (this.CfgDom == null || this.CfgDom.DocumentElement == null)
                return 0;

            // 找到 <searchIndex> 元素
            XmlNode nodeSearchIndex = this.CfgDom.DocumentElement.SelectSingleNode("searchIndexCollection/searchIndex[@name='"+strSearchIndex+"']");
            if (nodeSearchIndex == null)
            {
                strError = "配置文件中没有找到 name 属性值为 '"+strSearchIndex+"' 的 searchIndexCollection/searchIndex 元素";
                return -1;
            }

            // 列出它下面的全部 <line> 元素
            XmlNodeList line_nodes = nodeSearchIndex.SelectNodes("line");
            if (line_nodes.Count == 0)
                return 0;

            foreach(XmlNode line_node in line_nodes)
            {
                XmlElement def = GetLineDefElement((XmlElement)line_node);
                if (def == null)
                {
                    strError = "配置文件中，下列 <line> 元素没有找到 name 属性匹配的 定义元素 lineTypes/line : " + line_node.OuterXml;
                    return -1;
                }

                string strLineType = def.GetAttribute("type");
                if (string.IsNullOrEmpty(strLineType) == true)
                    strLineType = "textBox";    // 缺省为 textBox

                ItemType itemType = GetItemType(strLineType);
                if (itemType == ItemType.None)
                {
                    strError = "配置文件中使用了未定义的 (<line> 元素) type 属性值 '"+strLineType+"' ";
                    return -1;
                }
                Item item = AppendNewItem(itemType);
                string strName = def.GetAttribute("name");
                item.Name = strName;

                // 获得特定语言的 Caption
                string strCaption = DomUtil.GetCaption(this.Lang, def);
                if (string.IsNullOrEmpty(strCaption) == false)
                    strName = strCaption;

                item.label.Text = strName;

                // TODO: 设置下拉列表值
                List<ValueItem> values = GetValueItems(def);
                if (values.Count != 0)
                    item.FillList(values);

                // this.Items.Add(item);
            }

            // 恢复值内容
            if (this.m_oldValues.Count > 0 && this.Items.Count > 0)
                RestoreValues(this.m_oldValues);
            return 0;
        }

        // 恢复值内容
        public void RestoreValues(IDictionary<string, string> values)
        {
            foreach (Item item in this.Items)
            {
                if (values.ContainsKey(item.Name) == true)
                    item.Value = values[item.Name];
            }
        }

        static ItemType GetItemType(string strItemType)
        {
            if (string.Compare(strItemType, "textBox", true) == 0)
                return ItemType.TextBox;
            if (string.Compare(strItemType, "comboBox", true) == 0)
                return ItemType.ComboBox;

            return ItemType.None;    // 没有找到
        }

        // 找到一个 <line> 元素对应的 <line> 定义元素
        static XmlElement GetLineDefElement(XmlElement line)
        {
            string strName = line.GetAttribute("name");
            return (XmlElement)line.OwnerDocument.DocumentElement.SelectSingleNode("lineTypes/line[@name='"+strName+"']");
        }

        enum ItemType
        {
            None = 0,
            TextBox = 1,
            ComboBox = 2,
        }

        class Item : IDisposable
        {
            public AmazonSearchParametersControl Container = null;

            public string Name = "";    // 对应于 <line> 元素 name 属性值

            public ItemType ItemType = ItemType.TextBox;

            public Label label = null;

            public TextBox textBox = null;

            public TabComboBox comboBox = null;

            void DisposeChildControls()
            {
                if (label != null)
                {
                    label.Dispose();
                    label = null;
                }
                if (textBox != null)
                {
                    textBox.Dispose();
                    textBox = null;
                }
                if (comboBox != null)
                {
                    comboBox.Dispose();
                    comboBox = null;
                }
                Container = null;
            }

            #region 释放资源

            ~Item()
            {
                Dispose(false);
            }

            private bool disposed = false;
            public void Dispose()
            {
                Dispose(true);
                // Take yourself off the Finalization queue 
                // to prevent finalization code for this object
                // from executing a second time.
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    if (disposing)
                    {
                        // release managed resources if any
                        // AddEvents(false);
                        DisposeChildControls();
                    }

                    // release unmanaged resource

                    // Note that this is not thread safe.
                    // Another thread could start disposing the object
                    // after the managed resources are disposed,
                    // but before the disposed flag is set to true.
                    // If thread safety is necessary, it must be
                    // implemented by the client.
                }
                disposed = true;
            }

            #endregion


            public Item(AmazonSearchParametersControl container,
                ItemType itemType)
            {
                this.Container = container;

                this.ItemType = itemType;

                int nTopBlank = 0;  //  (int)this.Container.Font.GetHeight() + 2;

                label = new Label();
                label.Dock = DockStyle.Fill;
                label.Size = new Size(70, 28);
                label.AutoSize = true;
                label.TextAlign = ContentAlignment.MiddleRight;
                label.Margin = new Padding(1, 0, 1, 0);

                if (this.ItemType == AmazonSearchParametersControl.ItemType.TextBox)
                {
                    this.textBox = new TextBox();
                    textBox.BorderStyle = BorderStyle.None;
                    textBox.Dock = DockStyle.Fill;
                    textBox.MinimumSize = new Size(80, 24);
                    // textBox_price.Multiline = true;
                    textBox.Margin = new Padding(6, nTopBlank + 6, 6, 0);
                    textBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
                }

                if (this.ItemType == AmazonSearchParametersControl.ItemType.ComboBox)
                {
                    comboBox = new TabComboBox();
                    comboBox.RemoveRightPartAtTextBox = false;
                    comboBox.DropDownStyle = ComboBoxStyle.DropDown;
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.Dock = DockStyle.Fill;
                    // comboBox.MaximumSize = new Size(150, 28);
                    comboBox.Size = new Size(100, 28);
                    comboBox.MinimumSize = new Size(50, 28);
                    comboBox.DropDownHeight = 300;
                    comboBox.DropDownWidth = 300;
                    comboBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
                    comboBox.Text = "";
                    comboBox.Margin = new Padding(6 - 4, nTopBlank + 6 - 2, 6, 2);
                }
            }

            public string Value
            {
                get
                {
                    if (this.ItemType == AmazonSearchParametersControl.ItemType.ComboBox)
                        return this.comboBox.Text;
                    else if (this.ItemType == AmazonSearchParametersControl.ItemType.TextBox)
                        return this.textBox.Text;
                    else
                        return null;
                }
                set
                {
                    if (this.ItemType == AmazonSearchParametersControl.ItemType.ComboBox)
                        this.comboBox.Text = value;
                    else if (this.ItemType == AmazonSearchParametersControl.ItemType.TextBox)
                        this.textBox.Text = value;
                }
            }

            internal void FillList(List<ValueItem> values)
            {
                if (values == null || values.Count == 0)
                    return;
                this.comboBox.Items.Clear();
                foreach (ValueItem value in values)
                {
                    this.comboBox.Items.Add(value.Caption + "\t" + value.Name);
                }
            }

            // 将控件加入到tablelayoutpanel中
            internal void AddToTable(TableLayoutPanel table,
                int nRow)
            {
                table.Controls.Add(this.label, 0, nRow);

                if (this.textBox != null)
                    table.Controls.Add(this.textBox, 1, nRow);
                else if (this.comboBox != null)
                    table.Controls.Add(this.comboBox, 1, nRow);

                // AddEvents();
            }
        }

        int m_nInSuspend = 0;

        public void DisableUpdate()
        {
            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.SuspendLayout();
            }

            this.m_nInSuspend++;
        }

        // parameters:
        //      bOldVisible 如果为true, 表示真的要结束
        public void EnableUpdate()
        {
            this.m_nInSuspend--;

            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.ResumeLayout(false);
                this.tableLayoutPanel_content.PerformLayout();
            }
        }

        Item AppendNewItem(ItemType itemType)
        {
            this.DisableUpdate();   // 防止闪动。彻底解决问题。2009/10/13 

            try
            {
                this.tableLayoutPanel_content.RowCount += 1;
                this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle());

                Item item = new Item(this, itemType);

                item.AddToTable(this.tableLayoutPanel_content, this.Items.Count + 1);

                this.Items.Add(item);
                return item;
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        private void tableLayoutPanel_content_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            if (this.m_nInSuspend > 0)
                return; // 防止闪动

            // Rectangle rect = Rectangle.Inflate(e.CellBounds, -1, -1);
            Rectangle rect = e.CellBounds;
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        // 一个列表值事项
        class ValueItem
        {
            public string Name = "";
            public string Caption = "";
        }

        /*
    <line name="Condition"  type="comboBox">
      <item name="value1">
        <caption lang="zh-CN">值1</caption>
      </item>
      <item name="value2">
        <caption lang="zh-CN">值2</caption>
      </item>
    </line>
         * * */
        List<ValueItem> GetValueItems(XmlElement parent)
        {
            List<ValueItem> results = new List<ValueItem>();

            XmlNodeList items = parent.SelectNodes("item");
            foreach (XmlNode item in items)
            {
                var element = item as XmlElement;
                ValueItem value = new ValueItem();
                value.Name = element.GetAttribute("name");
                value.Caption = DomUtil.GetCaption(this.Lang, element);
                results.Add(value);
            }

            return results;
        }
    }
}
