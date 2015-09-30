using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;


using DigitalPlatform.Xml;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 编辑一组 caption 元素的界面控件
    /// </summary>
    public partial class CaptionEditControl : UserControl
    {
        internal const int BASE_ROW_COUNT = 2;

        internal bool m_bFocused = false;

        bool m_bHideSelection = true;


        // 初始的语言代码表
        internal string[] language_codes = new string[] {
            "zh-CN",
            "en",
        };

        public event EventHandler ElementCountChanged = null;

        public event EventHandler SelectedIndexChanged = null;

        public CaptionElement LastClickElement = null;   // 最近一次click选择过的Element对象

        int m_nInSuspend = 0;

        public List<CaptionElement> Elements = new List<CaptionElement>();

        bool m_bChanged = false;

        public CaptionEditControl()
        {
            InitializeComponent();
        }

        // 是否有标题行?
        internal bool m_bHasTitleLine = true;

        [DefaultValue(true)]
        public bool HasTitleLine
        {
            get
            {
                return this.m_bHasTitleLine;
            }
            set
            {
                this.m_bHasTitleLine = value;

                this.label_topleft.Visible = this.m_bHasTitleLine;
                this.label_language.Visible = this.m_bHasTitleLine;
                this.label_text.Visible = this.m_bHasTitleLine;
            }
        }

        // 返回结果:
        //     如果容器允许自动滚动，则为 true；否则为 false。默认值为 false。
        [DefaultValue(false)]
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                base.AutoScroll = value;
                this.tableLayoutPanel_main.AutoScroll = value;
            }
        }

        public TableLayoutPanelCellBorderStyle CellBorderStyle
        {
            get
            {
                return this.tableLayoutPanel_main.CellBorderStyle;
            }
            set
            {
                this.tableLayoutPanel_main.CellBorderStyle = value;
            }
        }

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

        // 填充语言名列表
        public void FillLanguageList(ComboBox list,
            string[] languages)
        {
            list.Items.Clear();
            if (languages == null
                || languages.Length == 0)
                return;

            foreach (string value in languages)
            {
                list.Items.Add(value);
            }
        }

        void ResetLineColor()
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                CaptionElement element = this.Elements[i];
                element.State = ElementState.Normal;
            }
        }

        void RefreshLineColor()
        {
            for (int i = 0; i < this.Elements.Count; i++)
            {
                CaptionElement item = this.Elements[i];
                item.SetLineColor();
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("HideSelection")]
        [DefaultValue(true)]
        public bool HideSelection
        {
            get
            {
                return this.m_bHideSelection;
            }
            set
            {
                if (this.m_bHideSelection != value)
                {
                    this.m_bHideSelection = value;
                    this.RefreshLineColor(); // 迫使颜色改变
                }
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

        // 检查当前内容形式上是否合法
        // return:
        //      -1  检查过程本身出错
        //      0   格式有错误
        //      1   格式没有错误
        public int Verify(out string strError)
        {
            strError = "";

            if (this.Elements.Count == 0)
            {
                strError = "连一行内容都没有";
                return 0;
            }


            List<string> langs = new List<string>();

            for (int i = 0; i < this.Elements.Count; i++)
            {
                CaptionElement element = this.Elements[i];
                string strLanguage = element.Language;
                string strValue = element.Value;

                if (String.IsNullOrEmpty(strLanguage) == true
                    && String.IsNullOrEmpty(strValue) == true)
                {
                    strError = "第 " + (i+1).ToString() + " 行为空行，如果无用，应予以删除";
                    return 0;
                }

                if (String.IsNullOrEmpty(strLanguage) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的语言代码尚未指定";
                    return 0;
                }

                if (String.IsNullOrEmpty(strValue) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的文字值尚未指定";
                    return 0;
                }

                int index = langs.IndexOf(strLanguage);
                if (index != -1)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的语言代码 '" + strLanguage + "' 和 第 " + (index+1).ToString() + " 行的重复了";
                    return 0;
                }

                langs.Add(strLanguage);
            }

            return 1;
        }

        int GetXml(out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            for (int i = 0; i < this.Elements.Count; i++)
            {
                CaptionElement element = this.Elements[i];
                string strLanguage = element.Language;
                string strValue = element.Value;

                if (String.IsNullOrEmpty(strLanguage) == true
                    && String.IsNullOrEmpty(strValue) == true)
                    continue;

                if (String.IsNullOrEmpty(strLanguage) == true)
                {
                    strError = "第 " + (i+1).ToString() + " 行的语言代码尚未指定";
                    return -1;
                }

                if (String.IsNullOrEmpty(strValue) == true)
                {
                    strError = "第 " + (i + 1).ToString() + " 行的文字值尚未指定";
                    return -1;
                }

                XmlNode node = dom.CreateElement("caption");
                dom.DocumentElement.AppendChild(node);

                DomUtil.SetAttr(node, "lang", strLanguage);
                node.InnerText = strValue;
            }

            strXml = dom.DocumentElement.InnerXml;

            return 0;
        }

        int SetXml(string strXml,
            out string strError)
        {
            strError = "";

            // clear lines原有内容
            this.Clear();
            this.LastClickElement = null;

            if (String.IsNullOrEmpty(strXml) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            XmlDocumentFragment fragment = dom.CreateDocumentFragment();
            try
            {
                fragment.InnerXml = strXml;
            }
            catch (Exception ex)
            {
                strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("caption");

            this.DisableUpdate();

            try
            {

                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];

                    CaptionElement element = this.AppendNewElement();

                    element.Language = DomUtil.GetAttr(node, "lang");
                    element.Value = node.InnerText;
                }


                this.Changed = false;
            }
            finally
            {
                this.EnableUpdate();
            }

            return 0;
        }

        public CaptionElement AppendNewElement()
        {
            this.tableLayoutPanel_main.RowCount += 1;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());

            CaptionElement line = new CaptionElement(this);

            line.AddToTable(this.tableLayoutPanel_main, this.Elements.Count + 1);

            this.Elements.Add(line);
            if (this.ElementCountChanged != null)
                this.ElementCountChanged(this, new EventArgs());

            return line;
        }

        public CaptionElement InsertNewElement(int index)
        {
            this.tableLayoutPanel_main.RowCount += 1;
            this.tableLayoutPanel_main.RowStyles.Insert(index + 1, new System.Windows.Forms.RowStyle());

            CaptionElement line = new CaptionElement(this);

            line.InsertToTable(this.tableLayoutPanel_main, index);

            this.Elements.Insert(index, line);
            if (this.ElementCountChanged != null)
                this.ElementCountChanged(this, new EventArgs());

            line.State = ElementState.New;

            SelectElement(line, true);  // 2008/6/10

            return line;
        }

        public void RemoveElement(int index)
        {
            CaptionElement line = this.Elements[index];

            line.RemoveFromTable(this.tableLayoutPanel_main, index);

            this.Elements.Remove(line);
            if (this.ElementCountChanged != null)
                this.ElementCountChanged(this, new EventArgs());

            if (this.LastClickElement == line)
                this.LastClickElement = null;

            this.Changed = true;
        }

        public void RemoveElement(CaptionElement line)
        {
            int index = this.Elements.IndexOf(line);

            if (index == -1)
                return;

            line.RemoveFromTable(this.tableLayoutPanel_main, index);

            this.Elements.Remove(line);
            if (this.ElementCountChanged != null)
                this.ElementCountChanged(this, new EventArgs());

            if (this.LastClickElement == line)
                this.LastClickElement = null;

            this.Changed = true;
        }

        public void DisableUpdate()
        {
            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_main.SuspendLayout();
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
                this.tableLayoutPanel_main.ResumeLayout(false);
                this.tableLayoutPanel_main.PerformLayout();
            }
        }

        void ClearElements()
        {
            if (this.Elements != null)
            {
                foreach (CaptionElement caption in this.Elements)
                {
                    if (caption != null)
                        caption.Dispose();
                }

                this.Elements.Clear();
            }
        }

        public void Clear()
        {
            this.DisableUpdate();

            try
            {

                for (int i = 0; i < this.Elements.Count; i++)
                {
                    CaptionElement element = this.Elements[i];
                    ClearOneElementControls(this.tableLayoutPanel_main,
                        element);
                }

                // this.Elements.Clear();
                this.ClearElements();

                this.tableLayoutPanel_main.RowCount = BASE_ROW_COUNT;    // 为什么是1？
                for (; ; )
                {
                    if (this.tableLayoutPanel_main.RowStyles.Count <= BASE_ROW_COUNT)
                        break;
                    this.tableLayoutPanel_main.RowStyles.RemoveAt(BASE_ROW_COUNT);
                }

            }
            finally
            {
                this.EnableUpdate();
            }

            if (this.ElementCountChanged != null)
                this.ElementCountChanged(this, new EventArgs());
        }

        // 清除一个CaptionElement对象对应的Control
        public void ClearOneElementControls(
            TableLayoutPanel table,
            CaptionElement line)
        {
            // color
            Label label = line.label_color;
            table.Controls.Remove(label);

            // language
            ComboBox language = line.comboBox_language;
            table.Controls.Remove(language);

            // text
            TextBox text = line.textBox_value;
            table.Controls.Remove(text);
        }

        public void SelectAll()
        {
            bool bSelectedChanged = false;

            for (int i = 0; i < this.Elements.Count; i++)
            {
                CaptionElement cur_element = this.Elements[i];
                if ((cur_element.State & ElementState.Selected) == 0)
                {
                    cur_element.State |= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            if (bSelectedChanged == true)
                this.OnSelectedIndexChanged();
        }

        public List<CaptionElement> SelectedElements
        {
            get
            {
                List<CaptionElement> results = new List<CaptionElement>();

                for (int i = 0; i < this.Elements.Count; i++)
                {
                    CaptionElement cur_element = this.Elements[i];
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
                    CaptionElement cur_element = this.Elements[i];
                    if ((cur_element.State & ElementState.Selected) != 0)
                        results.Add(i);
                }

                return results;
            }
        }

        public void SelectElement(CaptionElement element,
            bool bClearOld)
        {
            bool bSelectedChanged = false;

            if (bClearOld == true)
            {
                for (int i = 0; i < this.Elements.Count; i++)
                {
                    CaptionElement cur_element = this.Elements[i];

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
                    element.comboBox_language.Focus();
            }

            if (bSelectedChanged == true)
                OnSelectedIndexChanged();
        }

        public void ToggleSelectElement(CaptionElement element)
        {
            // 选中当前行
            if ((element.State & ElementState.Selected) == 0)
                element.State |= ElementState.Selected;
            else
                element.State -= ElementState.Selected;

            this.LastClickElement = element;

            this.OnSelectedIndexChanged();
        }

        void OnSelectedIndexChanged()
        {
            if (this.SelectedIndexChanged != null)
            {
                this.SelectedIndexChanged(this, new EventArgs());
            }
        }

        public void RangeSelectElement(CaptionElement element)
        {
            bool bSelectedChanged = false;

            CaptionElement start = this.LastClickElement;

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
                CaptionElement cur_element = this.Elements[i];

                if ((cur_element.State & ElementState.Selected) == 0)
                {
                    cur_element.State |= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            // 清除其余位置
            for (int i = 0; i < nStart; i++)
            {
                CaptionElement cur_element = this.Elements[i];

                if ((cur_element.State & ElementState.Selected) != 0)
                {
                    cur_element.State -= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            for (int i = nEnd + 1; i < this.Elements.Count; i++)
            {
                CaptionElement cur_element = this.Elements[i];

                if ((cur_element.State & ElementState.Selected) != 0)
                {
                    cur_element.State -= ElementState.Selected;
                    bSelectedChanged = true;
                }
            }

            if (bSelectedChanged == true)
                this.OnSelectedIndexChanged();
        }

        public void DeleteSelectedElements()
        {
            bool bSelectedChanged = false;

            List<CaptionElement> selected_lines = this.SelectedElements;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的行");
                return;
            }
            string strText = "";

            if (selected_lines.Count == 1)
                strText = "确实要删除行 '" + selected_lines[0].Language + "'? ";
            else
                strText = "确实要删除所选定的 " + selected_lines.Count.ToString() + " 个行?";

            DialogResult result = MessageBox.Show(this,
                strText,
                "CaptionEditControl",
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

        // 获得所选择的部分元素的XML
        public int GetFragmentXml(
            List<CaptionElement> selected_lines,
            out string strXml,
            out string strError)
        {
            strXml = "";
            strError = "";

            if (selected_lines.Count == 0)
                return 0;

            XmlDocument dom = new XmlDocument();

            dom.LoadXml("<root />");

            for (int i = 0; i < selected_lines.Count; i++)
            {
                CaptionElement line = selected_lines[i];

                string strLanguage = line.Language;

                if (String.IsNullOrEmpty(strLanguage) == true)
                {
                    if (String.IsNullOrEmpty(line.Value) == true)
                        continue;
                    else
                    {
                        strError = "格式错误：内容为 '" + line.Value + "' 的行没有指定语言代码";
                        return -1;
                    }
                }

                XmlNode element = dom.CreateElement("caption");
                dom.DocumentElement.AppendChild(element);

                DomUtil.SetAttr(element, "lang", strLanguage);
                element.InnerText = line.Value;

            }

            strXml = dom.DocumentElement.InnerXml;
            return 0;
        }

        // 用片断XML中包含的元素，替换指定的若干行
        // 如果selected_lines.Count == 0，则表示从nInsertPos开始插入
        public int ReplaceElements(
            int nInsertPos,
            List<CaptionElement> selected_lines,
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
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            dom.DocumentElement.AppendChild(fragment);


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
                    if (node.Name != "caption")
                        continue;

                    CaptionElement line = null;
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

                    line.Language = DomUtil.GetAttr(node, "lang");

                    line.Value = node.InnerText;
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

        private void CaptionEditControl_Enter(object sender, EventArgs e)
        {
            this.m_bFocused = true;
            this.RefreshLineColor();

        }

        private void CaptionEditControl_Leave(object sender, EventArgs e)
        {
            this.m_bFocused = false;
            this.RefreshLineColor();
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
            menuItem = new MenuItem("后插新行(&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("复制所有行(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyRecord_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴替换所有行(&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteRecord_Click);
            if (bHasClipboardObject == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.label_topleft, new Point(e.X, e.Y));

        }

        // 在最后追加一个新行
        void menu_appendElement_Click(object sender, EventArgs e)
        {
            NewElement();
        }

        // 在最后追加一个新行
        public void NewElement()
        {
            CaptionElement element = this.InsertNewElement(this.Elements.Count);

            // 滚入可见范围？
            element.ScrollIntoView();

            // 选定它
            element.Select(1);
        }

        // 复制整个记录
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

            /*
            string strOutXml = "";
            nRet = DomUtil.GetIndentXml(strXml,
                out strOutXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }*/

            Clipboard.SetDataObject(strXml);
        }

        // 粘贴替换整个记录
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
    }


    public class CaptionElement : IDisposable
    {
        public CaptionEditControl Container = null;

        // 颜色、popupmenu
        public Label label_color = null;

        // 语言
        public ComboBox comboBox_language = null;

        // 文字
        public TextBox textBox_value = null;

        void DisposeChildControls()
        {
            label_color.Dispose();
            comboBox_language.Dispose();
            textBox_value.Dispose();
            Container = null;
        }

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

        #region 释放资源

        ~CaptionElement()
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
                    AddEvents(false);
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

        internal void SetLineColor()
        {
            if ((this.m_state & ElementState.Selected) != 0)
            {
                // 没有焦点，又需要隐藏selection情形
                if (this.Container.HideSelection == true
                    && this.Container.m_bFocused == false)
                {
                    // 继续向后走，显示其他颜色
                }
                else
                {
                    this.label_color.BackColor = SystemColors.Highlight;
                    return;
                }
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

        public CaptionElement(CaptionEditControl container)
        {
            this.Container = container;

            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            label_color.Size = new Size(6, 28);

            // language
            comboBox_language = new ComboBox();
            comboBox_language.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_language.FlatStyle = FlatStyle.Flat;
            comboBox_language.Dock = DockStyle.Fill;
            comboBox_language.MaximumSize = new Size(150, 28);
            comboBox_language.Size = new Size(100, 28);
            comboBox_language.MinimumSize = new Size(50, 28);
            comboBox_language.DropDownHeight = 300;
            comboBox_language.DropDownWidth = 150;

            comboBox_language.ForeColor = this.Container.tableLayoutPanel_main.ForeColor;

            comboBox_language.Text = "";

            // value
            textBox_value = new TextBox();
            textBox_value.BorderStyle = BorderStyle.None;
            textBox_value.Dock = DockStyle.Fill;
            textBox_value.MinimumSize = new Size(100, 26);  // 26才能避免覆盖表格线
            textBox_value.Margin = new Padding(6, 3, 6, 1);

            textBox_value.ForeColor = this.Container.tableLayoutPanel_main.ForeColor;
        }

        public string Language
        {
            get
            {
                return this.comboBox_language.Text;
            }
            set
            {
                this.comboBox_language.Text = value;
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
            }
        }

        public void AddToTable(TableLayoutPanel table,
            int nRow)
        {
            table.Controls.Add(this.label_color, 0, nRow);
            table.Controls.Add(this.comboBox_language, 1, nRow);
            table.Controls.Add(this.textBox_value, 2, nRow);

            AddEvents(true);
        }

        void AddEvents(bool bAdd)
        {
            // events

            if (bAdd)
            {
                // label_color
                this.label_color.MouseUp += new MouseEventHandler(label_color_MouseUp);

                this.label_color.MouseClick += new MouseEventHandler(label_color_MouseClick);

                // language
                this.comboBox_language.DropDown += new EventHandler(comboBox_language_DropDown);

                this.comboBox_language.TextChanged += new EventHandler(comboBox_language_TextChanged);

                this.comboBox_language.Enter += new EventHandler(comboBox_language_Enter);

                // value
                this.textBox_value.KeyUp += new KeyEventHandler(textBox_value_KeyUp);

                this.textBox_value.TextChanged += new EventHandler(textBox_value_TextChanged);

                this.textBox_value.Enter += new EventHandler(textBox_value_Enter);
            }
            else
            {
                this.label_color.MouseUp -= new MouseEventHandler(label_color_MouseUp);
                this.label_color.MouseClick -= new MouseEventHandler(label_color_MouseClick);
                this.comboBox_language.DropDown -= new EventHandler(comboBox_language_DropDown);
                this.comboBox_language.TextChanged -= new EventHandler(comboBox_language_TextChanged);
                this.comboBox_language.Enter -= new EventHandler(comboBox_language_Enter);
                this.textBox_value.KeyUp -= new KeyEventHandler(textBox_value_KeyUp);
                this.textBox_value.TextChanged -= new EventHandler(textBox_value_TextChanged);
                this.textBox_value.Enter -= new EventHandler(textBox_value_Enter);
            }
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

        // 剪切
        void menu_cut_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strXml = "";

            List<CaptionElement> selected_lines = this.Container.SelectedElements;


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

            List<CaptionElement> selected_lines = this.Container.SelectedElements;

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
                CaptionElement line = selected_lines[i];
                if ((line.State & ElementState.Selected) != 0)
                    line.State -= ElementState.Selected;
            }

        }

        // 粘贴插入[后]
        void menu_pasteInsertAfter_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<CaptionElement> selected_lines = this.Container.SelectedElements;

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
                CaptionElement line = selected_lines[i];
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

        void comboBox_language_DropDown(object sender, EventArgs e)
        {
            if (this.comboBox_language.Items.Count == 0)
                this.FillLanguageList();
        }

        void comboBox_language_TextChanged(object sender, EventArgs e)
        {
            if ((this.State & ElementState.New) == 0)
                this.State |= ElementState.Changed;

            this.Container.Changed = true;
        }

        void textBox_value_Enter(object sender, EventArgs e)
        {
            this.Container.SelectElement(this, true);
        }

        void comboBox_language_Enter(object sender, EventArgs e)
        {
            this.Container.SelectElement(this, true);
        }

        void textBox_value_KeyUp(object sender, KeyEventArgs e)
        {
        }

        void textBox_value_TextChanged(object sender, EventArgs e)
        {
            this.Container.Changed = true;

            if ((this.State & ElementState.New) == 0)
                this.State |= ElementState.Changed;
        }

        public void FillLanguageList()
        {
            this.Container.FillLanguageList(this.comboBox_language,
                this.Container.language_codes);
        }

        // 本元素所从属的控件拥有了焦点了么?
        public bool IsSubControlFocused()
        {
            if (this.comboBox_language.Focused == true)
                return true;

            if (this.textBox_value.Focused == true)
                return true;

            return false;
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
                    this.Container.Elements.Count + (CaptionEditControl.BASE_ROW_COUNT + 1), "");

                // 先移动后方的
                for (int i = (table.RowCount - 1) - (CaptionEditControl.BASE_ROW_COUNT + 1); i >= nRow; i--)
                {
                    CaptionElement line = this.Container.Elements[i];

                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, 0, i + 1 + 1);

                    // language
                    ComboBox language = line.comboBox_language;
                    table.Controls.Remove(language);
                    table.Controls.Add(language, 1, i + 1 + 1);

                    // text
                    TextBox text = line.textBox_value;
                    table.Controls.Remove(text);
                    table.Controls.Add(text, 2, i + 1 + 1);

                }

                table.Controls.Add(this.label_color, 0, nRow + 1);
                table.Controls.Add(this.comboBox_language, 1, nRow + 1);
                table.Controls.Add(this.textBox_value, 2, nRow + 1);

            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // events
            AddEvents(true);
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
                table.Controls.Remove(this.comboBox_language);
                table.Controls.Remove(this.textBox_value);

                Debug.Assert(this.Container.Elements.Count
                    == table.RowCount - CaptionEditControl.BASE_ROW_COUNT, "");

                // 然后压缩后方的
                for (int i = (table.RowCount - CaptionEditControl.BASE_ROW_COUNT) - 1; i >= nRow + 1; i--)
                {
                    CaptionElement line = this.Container.Elements[i];

                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, 0, i - 1 + 1);

                    // language
                    ComboBox language = line.comboBox_language;
                    table.Controls.Remove(language);
                    table.Controls.Add(language, 1, i - 1 + 1);

                    // text
                    TextBox text = line.textBox_value;
                    table.Controls.Remove(text);
                    table.Controls.Add(text, 2, i - 1 + 1);
                }

                table.RowCount--;
                table.RowStyles.RemoveAt(nRow);

                this.AddEvents(false);
            }
            finally
            {
                this.Container.EnableUpdate();
            }
        }

        // 滚入可见范围
        public void ScrollIntoView()
        {
            this.Container.tableLayoutPanel_main.ScrollControlIntoView(this.comboBox_language);
        }

        // 单选本元素
        // parameters:
        //      nCol    1 语言代码列; 2: 内容列
        public void Select(int nCol)
        {

            if (nCol == 1)
            {
                this.comboBox_language.SelectAll();
                this.comboBox_language.Focus();
                return;
            }

            if (nCol == 2)
            {
                this.textBox_value.SelectAll();
                this.textBox_value.Focus();
                return;
            }

            this.Container.SelectElement(this, true);
        }
    }

}
