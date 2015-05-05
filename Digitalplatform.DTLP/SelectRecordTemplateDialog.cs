using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.Marc;

namespace DigitalPlatform.DTLP
{
    public partial class SelectRecordTemplateDialog : Form
    {
        bool m_bChanged = false;
        public bool LoadMode = true;    // 是否为装载状态？==false表示保存、修改状态

        public string SelectedRecordMarc = "";  // 装载模式下：选中的记录的MARC机内格式字符串；修改模式下：对话框打开前就要设置，提供新的MARC记录内容

        public string Content = ""; // 配置文件内容

        public SelectRecordTemplateDialog()
        {
            InitializeComponent();
        }

        private void SelectRecordTemplateDialog_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.Content) == false)
                FillList(this.LoadMode); // 修改模式时，不进行自动选定，避免用户无意识覆盖了第一项
            

            if (this.LoadMode == true)
            {
                this.textBox_name.ReadOnly = true;
                this.listView1.MultiSelect = false;
                this.Text = "请选择新记录模板";
            }
            else
            {
                this.textBox_name.ReadOnly = false;
                this.listView1.MultiSelect = true;
                if (this.Text == "请选择新记录模板")
                    this.Text = "请指定要修改的记录模板";
            }
        }

        private void SelectRecordTemplateDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.LoadMode == false)
            {
                if (this.DialogResult != DialogResult.OK
                    && m_bChanged == true)
                {
                    DialogResult result = MessageBox.Show(this,
                        "确实要放弃先前所做的全部修改么?\r\n\r\n(是)放弃修改 (否)不关闭窗口\r\n\r\n(注: 模板名为空的情况下仍可以按\"确定\"按钮保存所做的修改。)",
                        "SelectRecordTemplateDialog",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.LoadMode == true)
            {
                if (this.listView1.SelectedItems.Count == 0)
                {
                    MessageBox.Show(this, "尚未选定要装载的模板事项");
                    return;
                }

                ListViewItem item = this.listView1.SelectedItems[0];
                this.SelectedRecordMarc = GetMarc((string)item.Tag);
            }
            else
            {
                // 观察textbox_name中的名字。
                // 如果和已存在的某个事项同名，表明要用提供的MARC记录替换模板记录
                // 而如果不和任何已存在的事项名相同，表明要在末尾增添一个新的模板事项
                // return:
                //      0   not changed
                //      1   changed
                ChangeContent();

                if (this.Changed == true)
                {
                    // 合成content
                    this.Content = "";
                    for (int i = 0; i < this.listView1.Items.Count; i++)
                    {
                        ListViewItem item = this.listView1.Items[i];
                        string strName = item.Text;
                        string strComment = ListViewUtil.GetItemText(item, 1);

                        if (String.IsNullOrEmpty(strComment) == false)
                            strName += "|" + strComment;

                        this.Content += strName + "\r\n";
                        this.Content += (string)item.Tag;
                    }

                    // 对话框返回后，this.Content中有更新过的整个模板文件内容
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // return:
        //      0   not changed
        //      1   changed
        int ChangeContent()
        {
            if (String.IsNullOrEmpty(this.SelectedRecordMarc) == true)
                return 0;

            if (String.IsNullOrEmpty(this.textBox_name.Text) == true)
                return 0;

            // 观察textbox_name中的名字。
            // 如果和已存在的某个事项同名，表明要用提供的MARC记录替换模板记录
            // 而如果不和任何已存在的事项名相同，表明要在末尾增添一个新的模板事项
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                ListViewItem item = this.listView1.Items[i];

                if (this.textBox_name.Text == item.Text)
                {
                    item.Tag = GetWorksheet(this.SelectedRecordMarc);
                    this.Changed = true;
                    return 1;
                }
            }

            // 没有找到，则在末尾新增加一项
            ListViewItem newitem = new ListViewItem(this.textBox_name.Text, 0);
            newitem.Tag = GetWorksheet(this.SelectedRecordMarc);
            this.listView1.Items.Add(newitem);

            this.Changed = true;
            return 1;
        }

        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        void FillList(bool bAutoSelect)
        {
            listView1.Items.Clear();
            // listView1_SelectedIndexChanged(null, null);

            List<TemplateItem> items = GetItems();


            for (int i = 0; i < items.Count; i++)
            {
                TemplateItem template_item = items[i];

                // 分离name和comment'
                string strName = "";
                string strComment = "";
                int nRet = template_item.Title.IndexOf("|");
                if (nRet != -1)
                {
                    strName = template_item.Title.Substring(0, nRet);
                    strComment = template_item.Title.Substring(nRet + 1);
                }
                else
                {
                    strName = template_item.Title;
                }


                ListViewItem listview_item = new ListViewItem(strName, 0);

                listview_item.SubItems.Add(strComment);
                listview_item.Tag = template_item.Content;

                this.listView1.Items.Add(listview_item);
            }

            // 选择第一项
            if (bAutoSelect == true)
            {
                if (listView1.Items.Count != 0)
                    listView1.Items[0].Selected = true;
            }

        }

        // 分析出全部事项
        List<TemplateItem> GetItems()
        {
            List<TemplateItem> items = new List<TemplateItem>();
            int nRet = 0;

            if (String.IsNullOrEmpty(this.Content) == true)
                return items;

            int nOffs = 0;
            string strLine = "";
            bool bEnd = false;
            TemplateItem item = null;
            for (int i=0; ;i++)
            {
                if (nOffs >= this.Content.Length)
                    break;

                nRet = this.Content.IndexOf("\r\n", nOffs);
                if (nRet == -1)
                {
                    strLine = this.Content.Substring(nOffs);
                    nOffs += strLine.Length;
                }
                else
                {
                    strLine = this.Content.Substring(nOffs, nRet - nOffs);
                    nOffs = nRet + 2;
                }

                if (i == 0)
                {
                    // 开始第一个
                    item = new TemplateItem();
                    item.Title = strLine;
                    // items.Add(strLine);
                }
                else if (bEnd == true)
                {
                    // 前一个进入列表
                    items.Add(item);

                    // 开始新的一个
                    item = new TemplateItem();
                    item.Title = strLine;
                    // items.Add(strLine);
                    bEnd = false;
                }
                else
                {
                    item.Content += strLine + "\r\n";
                }

                if (strLine == "***")
                    bEnd = true;

            }

            if (item != null)
                items.Add(item);

            return items;
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bSelected = listView1.SelectedItems.Count > 0;

            //
            menuItem = new MenuItem("查看(&C)");
            menuItem.Click += new System.EventHandler(this.menu_viewContent);
            if (bSelected == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("修改(&M)");
            menuItem.Click += new System.EventHandler(this.menu_Modify);
            if (bSelected == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteRecord);
            if (bSelected == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(listView1, new Point(e.X, e.Y));	
        }

        void menu_viewContent(object sender, System.EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择要查看的模板记录事项...");
                return;
            }

            string strText = "";
            for (int i = 0; i < this.listView1.SelectedItems.Count; i++)
            {
                ListViewItem item = this.listView1.SelectedItems[i];
                string strTitle = item.Text;
                string strContent = (string)item.Tag;

                strText += "[" + strTitle + "\r\n";
                strText += strContent + "]";
                strText += "\r\n";
            }

            MessageBox.Show(this, strText);
        }


        // 修改名字和注释
        void menu_Modify(object sender, System.EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择拟修改的模板记录事项...");
                return;
            }
            TemplateRecordDialog dlg = new TemplateRecordDialog();

            // string strOldName = ListViewUtil.GetItemText(listView1.SelectedItems[0], 0);

            dlg.TemplateName = ListViewUtil.GetItemText(listView1.SelectedItems[0], 0);
            dlg.TemplateComment = ListViewUtil.GetItemText(listView1.SelectedItems[0], 1);

            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            ListViewUtil.ChangeItemText(this.listView1.SelectedItems[0], 0, dlg.TemplateName);
            ListViewUtil.ChangeItemText(this.listView1.SelectedItems[0], 1, dlg.TemplateComment);

            this.Changed = true;
        }

        void menu_deleteRecord(object sender, System.EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选择拟删除的模板记录事项...");
                return;
            }

            //string strError = "";
            //int nRet = 0;

            DialogResult result = MessageBox.Show(this,
                "确实要删除所选择的 " + this.listView1.SelectedItems.Count.ToString() + " 个模板记录?",
                "SelectRecordTemplateDialog",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;


            for (int i = this.listView1.SelectedIndices.Count - 1; i >= 0 ; i--)
            {
                int index = this.listView1.SelectedIndices[i];

                this.listView1.Items.RemoveAt(index);
            }

            this.Changed = true;

        }

        // 把机内格式转化为工作单格式
        // 不包含标题
        static string GetWorksheet(string strMarc)
        {
            if (String.IsNullOrEmpty(strMarc) == true)
                return "012345678901234567890123\r\n***\r\n";
            if (strMarc.Length < 24)
                strMarc = strMarc.PadRight(24, ' ');

            string strRecord = strMarc;
            
            // 为头标区末尾增加一个字段结束符
            if (strRecord.Length > 24)
                strRecord = strRecord.Insert(24, new string(MarcUtil.FLDEND, 1));

            // 如果倒数第一个字符不是FLDEND，则插入一个
            if (strRecord[strRecord.Length - 1] != MarcUtil.FLDEND)
            {
                strRecord += new string(MarcUtil.FLDEND, 1);
            }

            // 将字段结束符替换为回车换行
            strRecord = strRecord.Replace(new string(MarcUtil.FLDEND, 1), "\r\n");

            // 将子字段符号替换为'@'
            strRecord = strRecord.Replace(new string(MarcUtil.SUBFLD, 1), "@");


            strRecord += "***\r\n";

            return strRecord;
        }

        // 把工作单格式的内容转化为机内格式
        static string GetMarc(string strContent)
        {
            string strRecord = strContent.Replace("\r\n***\r\n",
                new string(MarcUtil.FLDEND, 1));

            strRecord = strRecord.Replace("\r\n", new string(MarcUtil.FLDEND, 1));

            if (strRecord.Length >= 25)
            {
            // 搜寻第一个字段结束符。可能不在24位置
            int nRet = strRecord.IndexOf((char)MarcUtil.FLDEND);
            if (nRet != -1)
            {
                strRecord = strRecord.Remove(nRet, 1);

                if (nRet > 24)
                    strRecord = strRecord.Remove(24, nRet - 24);
                else if (nRet < 24)
                    strRecord = strRecord.Insert(nRet , new string(' ', 24 - nRet));

                if (strRecord[23] == '\\')
                    strRecord = strRecord.Remove(23, 1).Insert(23, " ");
            }

                // strRecord = strRecord.Remove(24, 1);	// 删除头标区后第一个FLDEND
            }

            // 如果倒数第一个字符不是FLDEND，则插入一个
            if (strRecord[strRecord.Length - 1] != MarcUtil.FLDEND)
            {
                strRecord += new string(MarcUtil.FLDEND, 1);
            }

            return strRecord.Replace('@', MarcUtil.SUBFLD);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            this.button_OK_Click(null, null);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
                this.textBox_name.Text = "";
            else
                this.textBox_name.Text = this.listView1.SelectedItems[0].Text;
        }



    }

    // 一个模板事项
    class TemplateItem
    {
        public string Title = "";   // 标题
        public string Content = ""; // 内容。原始的工作单格式
    }
}