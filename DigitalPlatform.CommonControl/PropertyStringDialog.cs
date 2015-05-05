using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CommonControl
{
    // 服务于CheckedCombobox
    public partial class PropertyStringDialog : Form
    {
        public CheckedComboBox CheckedComboBox = null;

        const int WM_CLOSE = API.WM_USER + 201;


        public bool HideCloseButton 
        {
            get
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                    return true;
                return false;
            }
            set
            {
                if (value == true)
                {
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.listView1.BorderStyle = BorderStyle.FixedSingle;   // Form没有边框了，就靠ListView的边框了
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
                    this.listView1.BorderStyle = BorderStyle.None;
                }
            }
        }

        // 是否仅仅在结果中返回第一个\t左边的内容。否则返回全部内容
        public bool ReturnFirstPart = true;

        int m_nIndex = 0;

        // public Point StartLocation = new Point(0, 0);

        public string PropertyString = "";

        // 全部可用值的列表
        public List<string> Items = new List<string>();

        public PropertyStringDialog()
        {
            InitializeComponent();

            this.listView1.LostFocus += new EventHandler(listView1_LostFocus);
        }

        void listView1_LostFocus(object sender, EventArgs e)
        {
            this.PropertyString = GetValue();
            this.Close();
            Debug.WriteLine("LostFocus");
        }

        private void PropertyStringDialog_Load(object sender, EventArgs e)
        {
            // this.Location = this.StartLocation;
            BuildList();
            SetValue(this.PropertyString);
            SetColumnWidth();
        }

        private void PropertyStringDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.PropertyString = GetValue();
        }

        // 根据列的多少，智能设置列宽度
        void SetColumnWidth()
        {
            if (this.listView1.View == View.Details)
            {
                int nMaxColumns = 0;
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    ListViewItem item = this.listView1.Items[i];
                    if (item.SubItems.Count > nMaxColumns)
                        nMaxColumns = item.SubItems.Count;
                }

                if (nMaxColumns == 1)
                {
                    this.columnHeader_name.Width = this.listView1.Width - SystemInformation.Border3DSize.Height * 4 - SystemInformation.VerticalScrollBarWidth;
                    this.columnHeader_comment.Width = 0;
                }
            }
        }

        // 根据ValueTable先建立未勾选的全部列表事项
        void BuildList()
        {
            this.listView1.Items.Clear();

            if (this.Items == null)
                return;

            for (int i = 0; i < this.Items.Count; i++)
            {
                string strLine = this.Items[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                ListViewItem item = new ListViewItem();

                // 根据\t分为若干部分
                string[] columns = strLine.Split(new char[] { '\t' });
                for (int j = 0; j < columns.Length; j++)
                {
                    ListViewUtil.ChangeItemText(item, j, columns[j].Trim());
                }

                this.listView1.Items.Add(item);
            }
        }

        // 根据逗号分割的值列表先建立未勾选的全部列表事项
        void BuildList(string strValue)
        {
            this.listView1.Items.Clear();

            string[] parts = strValue.Split(new char[] { ',' });

            for (int i = 0; i < parts.Length; i++)
            {
                string strLine = parts[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                ListViewItem item = new ListViewItem();

                // 根据\t分为若干部分
                string[] columns = strLine.Split(new char[] { '\t' });
                for (int j = 0; j < columns.Length; j++)
                {
                    ListViewUtil.ChangeItemText(item, j, columns[j].Trim());
                }

                this.listView1.Items.Add(item);
            }
        }

        // 构造勾选事项字符串
        string GetValue()
        {
            List<NumberedString> results = new List<NumberedString>();
            string strResult = "";
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                ListViewItem item = this.listView1.Items[i];
                if (item.Checked == false)
                    continue;

                ItemInfo iteminfo = null;
                if (item.Tag != null)
                    iteminfo = (ItemInfo)item.Tag;
                else
                {
                    iteminfo = new ItemInfo();
                    iteminfo.Index = 99999;
                }

                NumberedString one = new NumberedString();
                one.Index = iteminfo.Index;
                if (this.ReturnFirstPart == false)
                    one.Text = ListViewUtil.GetLineText(item);
                else
                    one.Text = ListViewUtil.GetItemText(item, 0);
                results.Add(one);
            }

            // 排序
            results.Sort(new NumberdStringCompare());

            for (int i = 0; i < results.Count; i++)
            {
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ",";
                strResult += results[i].Text;
            }

            return strResult;
        }

        // 勾选值事项。本函数须要在BuildList()后调用
        // 本函数能自动添加strValue中具备、但是ListView中尚未存在的事项行
        void SetValue(string strValue)
        {
            // 清除先前的全部checked状态
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                ListViewItem item = this.listView1.Items[i];
                if (item.Checked == true)
                    item.Checked = false;
                item.Tag = null;    // 清除原来残留的序号
            }

            this.m_nIndex = 0;  // 序号

            string[] parts = strValue.Split(new char[] {','});

            for (int i = 0; i < parts.Length; i++)
            {
                string strLine = parts[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;

                // 根据\t分为若干部分
                string[] columns = strLine.Split(new char [] {'\t'});
                if (columns.Length == 0)
                    continue;

                // 只用第一部分来进行匹配勾选
                string strFirstColumn = columns[0].Trim();

                ListViewItem item = ListViewUtil.FindItem(
                    this.listView1,
                    strFirstColumn, 0);

                // 如果事项居然不存在
                if (item == null)
                {
                    item = new ListViewItem();

                    for (int j = 0; j < columns.Length; j++)
                    {
                        ListViewUtil.ChangeItemText(item, j, columns[j].Trim());
                    }
                    this.listView1.Items.Add(item);
                }

                item.Checked = true;

                // 保留原始序号
                ItemInfo iteminfo = new ItemInfo();
                iteminfo.Index = this.m_nIndex++;
                iteminfo.OldItem = true;
                item.Tag = iteminfo; 
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            ListViewItem item = e.Item;

            ItemInfo iteminfo = (ItemInfo)item.Tag;
            if (iteminfo == null)
            {
                iteminfo = new ItemInfo();
                iteminfo.OldItem = false;
                iteminfo.Index = this.m_nIndex++;
                item.Tag = iteminfo;
            }
            else
            {
                if (iteminfo.Index == -1)
                    iteminfo.Index = this.m_nIndex++;
            }

            SetItemColor(item);

            if (this.CheckedComboBox != null)
                this.CheckedComboBox.OnItemChecked(e);
        }

        // 上下文菜单
        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("出现窗口标题条(&T)");
            menuItem.Click += new System.EventHandler(this.menu_displayTitleBar_Click);
            if (this.FormBorderStyle == FormBorderStyle.None)
                menuItem.Checked = false;
            else
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("出现栏目标题(&C)");
            menuItem.Click += new System.EventHandler(this.menu_displayColumTitle_Click);
            if (this.listView1.HeaderStyle == ColumnHeaderStyle.None)
                menuItem.Checked = false;
            else
                menuItem.Checked = true;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("全选(&S)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("全不选(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // TODO: 上移、下移、按照显示顺序输出、按照原始顺序显示...

            /*
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除(&D)");
            menuItem.Tag = this.listView_in;
            menuItem.Click += new System.EventHandler(this.menu_deleteSelected_Click);
            if (this.listView_in.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
             * */

            contextMenu.Show(this.listView1, new Point(e.X, e.Y));
        }

        void menu_displayColumTitle_Click(object sender, EventArgs e)
        {
            if (this.listView1.HeaderStyle == ColumnHeaderStyle.None)
                this.listView1.HeaderStyle = ColumnHeaderStyle.Clickable;
            else
                this.listView1.HeaderStyle = ColumnHeaderStyle.None;
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                ListViewItem item = this.listView1.Items[i];
                if (item.Checked == false)
                    item.Checked = true;
            }
        }

        void menu_displayTitleBar_Click(object sender, EventArgs e)
        {
            if (this.HideCloseButton == true)
                this.HideCloseButton = false;
            else
                this.HideCloseButton = true;
        }

        void menu_clearAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                ListViewItem item = this.listView1.Items[i];
                if (item.Checked == true)
                    item.Checked = false;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            this.PropertyString = GetValue();
            this.Close();
        }

        void SetItemColor(ListViewItem item)
        {
            if (item.Checked == true)
            {
                item.BackColor = SystemColors.Info;
                item.ForeColor = SystemColors.InfoText;
                item.Font = new Font(item.Font, FontStyle.Bold);
            }
            else
            {
                item.BackColor = SystemColors.Window;
                item.ForeColor = SystemColors.WindowText;
                item.Font = new Font(item.Font, FontStyle.Regular);
            }
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CLOSE:
                    this.Close();
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            /*
            if (e.X < 20)
                return;
             * */

            Point p = new Point(e.X, e.Y);
            ListViewHitTestInfo info = this.listView1.HitTest(p);
            if (info.Item == null)
                return;


            if (e.X < info.Item.Bounds.Height+2)
                return;

            if (info.SubItem == info.Item.SubItems[0])
            {
                // check此项，清除其它项
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    ListViewItem item = this.listView1.Items[i];
                    if (item == info.Item)
                    {
                        if (item.Checked == false)
                            item.Checked = true;
                    }
                    else
                    {
                        if (item.Checked == true)
                            item.Checked = false;
                    }
                }

                this.PropertyString = GetValue();
                API.PostMessage(this.Handle, WM_CLOSE, 0, 0);   // 2009/11/2 changed 如果这里直接用this.Close()，会导致无法捕捉的DivideByZeroException
                return;
            }

        }

        private void listView1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
                this.Close();
        }
    }

    class ItemInfo
    {
        public int Index = -1;  // -1表示尚未初始化
        public bool OldItem = false;    // ==true表示为原先就有的事项。新勾选的事项为false
    }

    // 带有序号的字符串
    public class NumberedString
    {
        public int Index = 0;
        public string Text = "";
    }

    public class NumberdStringCompare : IComparer<NumberedString>
    {

        int IComparer<NumberedString>.Compare(NumberedString x, NumberedString y)
        {
            return x.Index - y.Index;
        }

    }

    /*
    public delegate void OpenDetailEventHandler(object sender,
OpenDetailEventArgs e);

    /// <summary>
    /// 打开详细窗事件的参数
    /// </summary>
    public class OpenDetailEventArgs : EventArgs
    {
        /// <summary>
        /// 记录全路径集合。
        /// </summary>
        public string[] Paths = null;

        /// <summary>
        /// 是否开为新窗口
        /// </summary>
        public bool OpenNew = false;
    }
    */
}