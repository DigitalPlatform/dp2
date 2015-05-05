using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

// 2013/3/16 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// 打印选项对话框
    /// </summary>
    internal partial class PrintOptionDlg : Form
    {
        /// <summary>
        /// 本窗口从属的框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 打印参数
        /// </summary>
        internal PrintOption PrintOption = new PrintOption();

        /// <summary>
        /// 要在栏目名下拉列表中显示的事项
        /// </summary>
        public string[] ColumnItems = null;

        /// <summary>
        /// 数据目录。用于存储配置的模板文件等
        /// </summary>
        public string DataDir = ""; // 如果此项为空，则无法创建新的模板文件

        int m_nCurrentTemplateIndex = -1;   // 当前文件内容所对应的模板listview事项index
        bool m_bTemplateFileContentChanged = false;

        bool m_bTempaltesChanged = false;   // 模板列表发生了变化，提醒退出的时候需要保存

        List<string> m_newCreateTemplateFiles = new List<string>();

        const int COLUMN_NAME = 0;
        const int COLUMN_CAPTION = 1;
        const int COLUMN_WIDTHCHARS = 2;
        const int COLUMN_MAXCHARS = 3;
        const int COLUMN_EVALUE = 4;



        /// <summary>
        /// 构造函数
        /// </summary>
        public PrintOptionDlg()
        {
            InitializeComponent();
        }

        private void PrintOptionDlg_Load(object sender, EventArgs e)
        {
            this.textBox_pageHeader.Text = PrintOption.PageHeader;
            this.textBox_pageFooter.Text = PrintOption.PageFooter;

            this.textBox_tableTitle.Text = PrintOption.TableTitle;
            this.textBox_linesPerPage.Text = PrintOption.LinesPerPage.ToString();
            // this.textBox_maxSummaryChars.Text = PrintOption.MaxSummaryChars.ToString();

            this.listView_columns.Items.Clear();
            for (int i = 0; i < PrintOption.Columns.Count; i++)
            {
                ListViewItem item = new ListViewItem();

#if NO
                item.Text = PrintOption.Columns[i].Name;
                item.SubItems.Add(PrintOption.Columns[i].Caption);
                item.SubItems.Add(PrintOption.Columns[i].MaxChars.ToString());
#endif
                ListViewUtil.ChangeItemText(item, COLUMN_NAME, PrintOption.Columns[i].Name);
                ListViewUtil.ChangeItemText(item, COLUMN_CAPTION, PrintOption.Columns[i].Caption);
                ListViewUtil.ChangeItemText(item, COLUMN_WIDTHCHARS, PrintOption.Columns[i].WidthChars.ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_MAXCHARS, PrintOption.Columns[i].MaxChars.ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_EVALUE, PrintOption.Columns[i].Evalue);


                this.listView_columns.Items.Add(item);
            }

            LoadTemplates();
        }

        private void PrintOptionDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.m_bTempaltesChanged == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "模板页面有改动尚未保存，确实要放弃这些改动?\r\n(如果想保存这些修改并退出打印选项对话框，要按打印选项对话框下部的“确定”按钮)",
                    "PrintOptionDlg",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                RemoveNewCreatedTemplateFiles();
            }
        }

        private void PrintOptionDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            PrintOption.PageHeader = this.textBox_pageHeader.Text;
            PrintOption.PageFooter = this.textBox_pageFooter.Text;

            PrintOption.TableTitle = this.textBox_tableTitle.Text;

            try
            {
                PrintOption.LinesPerPage = Convert.ToInt32(this.textBox_linesPerPage.Text);
            }
            catch
            {
                MessageBox.Show(this, "每页行数值必须为纯数字");
                return;
            }



            PrintOption.Columns.Clear();
            for (int i = 0; i < this.listView_columns.Items.Count; i++)
            {
                ListViewItem item = this.listView_columns.Items[i];

                Column column = new Column();
                column.Name = ListViewUtil.GetItemText(item, COLUMN_NAME); // item.Text;
                column.Caption = ListViewUtil.GetItemText(item, COLUMN_CAPTION);  // item.SubItems[1].Text;

                try
                {
                    column.WidthChars = Convert.ToInt32(
                        ListViewUtil.GetItemText(item, COLUMN_WIDTHCHARS)
                        // item.SubItems[2].Text
                        );
                }
                catch
                {
                    column.WidthChars = -1;
                }

                try
                {
                    column.MaxChars = Convert.ToInt32(
                        ListViewUtil.GetItemText(item, COLUMN_MAXCHARS)
                        // item.SubItems[2].Text
                        );
                }
                catch
                {
                    column.MaxChars = -1;
                }

                column.Evalue = ListViewUtil.GetItemText(item, COLUMN_EVALUE);

                PrintOption.Columns.Add(column);
            }

            // 兑现最后一次对textbox的修改
            this.RefreshContentToTemplateFile();

            /*
            // 保存模板列表
            if (this.m_bTempaltesChanged == true)
            {
                PrintOption.TemplatePages.Clear();
                for (int i = 0; i < this.listView_templates.Items.Count; i++)
                {
                    ListViewItem item = this.listView_templates.Items[i];

                    TemplatePageParam param = new TemplatePageParam();
                    param.Caption = item.Text;
                    param.FilePath = ListViewUtil.GetItemText(item, 1);

                    PrintOption.TemplatePages.Add(param);
                }

                this.m_bTempaltesChanged = false;
            }

            this.m_newCreateTemplateFiles.Clear();  // 避免后面Closing()处理中不小心删除刚刚创建的文件
             * */
            SaveTemplatesChanges();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        void SaveTemplatesChanges()
        {
            // 保存模板列表
            if (this.m_bTempaltesChanged == true)
            {
                PrintOption.TemplatePages.Clear();
                for (int i = 0; i < this.listView_templates.Items.Count; i++)
                {
                    ListViewItem item = this.listView_templates.Items[i];

                    TemplatePageParam param = new TemplatePageParam();
                    param.Caption = item.Text;
                    param.FilePath = ListViewUtil.GetItemText(item, 1);

                    PrintOption.TemplatePages.Add(param);
                }

                this.m_bTempaltesChanged = false;
            }

            this.m_newCreateTemplateFiles.Clear();  // 避免后面Closing()处理中不小心删除刚刚创建的文件
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            /*
            if (this.m_bTempaltesChanged == true)
            {
                this.RefreshContentToTemplateFile();
            }
            */

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 删除本次刚刚创建的模板文件们
        void RemoveNewCreatedTemplateFiles()
        {
            for (int i = 0; i < this.m_newCreateTemplateFiles.Count; i++)
            {
                try
                {
                    File.Delete(this.m_newCreateTemplateFiles[i]);
                }
                catch
                {
                }
            }

            this.m_newCreateTemplateFiles.Clear();
        }

        // 新增栏目
        private void button_columns_new_Click(object sender, EventArgs e)
        {
            PrintColumnDlg dlg = new PrintColumnDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            if (this.ColumnItems != null)
            {
                dlg.ColumnItems = this.ColumnItems;
            }

            if (this.MainForm != null)
                this.MainForm.AppInfo.LinkFormState(dlg, "printorderdlg_formstate");
            dlg.ShowDialog(this);
            if (this.MainForm != null)
                this.MainForm.AppInfo.UnlinkFormState(dlg);


            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 名称查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_columns, dlg.ColumnName, 0);
            if (dup != null)
            {
                // 让操作者能看见已经存在的行
                ListViewUtil.SelectLine(dup, true);
                dup.EnsureVisible();

                DialogResult result = MessageBox.Show(this,
                    "当前已经存在名为 '"+dlg.ColumnName+"' 的栏目。继续新增?",
                    "PrintOptionDlg",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            ListViewItem item = new ListViewItem();
#if NO
            item.Text = dlg.ColumnName;
            item.SubItems.Add(dlg.ColumnCaption);
            item.SubItems.Add(dlg.MaxChars.ToString());
#endif
            ListViewUtil.ChangeItemText(item, COLUMN_NAME, dlg.ColumnName);
            ListViewUtil.ChangeItemText(item, COLUMN_CAPTION, dlg.ColumnCaption);
            ListViewUtil.ChangeItemText(item, COLUMN_WIDTHCHARS, dlg.WidthChars.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_MAXCHARS, dlg.MaxChars.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_EVALUE, dlg.ColumnEvalue);


            this.listView_columns.Items.Add(item);

            // 让操作者能看见新插入的行
            ListViewUtil.SelectLine(item, true);
            item.EnsureVisible();

            // 新增事项后，当前已选择事项的上下移动的可能性会有所改变
            listView_columns_SelectedIndexChanged(sender, null);
        }

        // 修改栏目
        private void button_columns_modify_Click(object sender, EventArgs e)
        {
            if (this.listView_columns.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要修改的事项");
                return;
            }

            PrintColumnDlg dlg = new PrintColumnDlg();
            MainForm.SetControlFont(dlg, this.Font, false);

            if (this.ColumnItems != null)
            {
                dlg.ColumnItems = this.ColumnItems;
            }

            ListViewItem item = this.listView_columns.SelectedItems[0];

            dlg.ColumnName = ListViewUtil.GetItemText(item, COLUMN_NAME);   // this.listView_columns.SelectedItems[0].Text;
            dlg.ColumnCaption = ListViewUtil.GetItemText(item, COLUMN_CAPTION);  // this.listView_columns.SelectedItems[0].SubItems[1].Text;

            try
            {
                dlg.WidthChars = Convert.ToInt32(
                    ListViewUtil.GetItemText(item, COLUMN_WIDTHCHARS)
                    // this.listView_columns.SelectedItems[0].SubItems[2].Text
                    );
            }
            catch
            {
                dlg.WidthChars = -1;
            }

            try
            {
                dlg.MaxChars = Convert.ToInt32(
                    ListViewUtil.GetItemText(item, COLUMN_MAXCHARS)
                    // this.listView_columns.SelectedItems[0].SubItems[2].Text
                    );
            }
            catch
            {
                dlg.MaxChars = -1;
            }

            dlg.ColumnEvalue = ListViewUtil.GetItemText(item, COLUMN_EVALUE);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // ListViewItem item = this.listView_columns.SelectedItems[0];
#if NO
            item.Text = dlg.ColumnName;
            item.SubItems[1].Text = dlg.ColumnCaption;
            item.SubItems[2].Text = dlg.MaxChars.ToString();
#endif
            ListViewUtil.ChangeItemText(item, COLUMN_NAME, dlg.ColumnName);
            ListViewUtil.ChangeItemText(item, COLUMN_CAPTION, dlg.ColumnCaption);
            ListViewUtil.ChangeItemText(item, COLUMN_WIDTHCHARS, dlg.WidthChars.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_MAXCHARS, dlg.MaxChars.ToString());
            ListViewUtil.ChangeItemText(item, COLUMN_EVALUE, dlg.ColumnEvalue);

        }

        // 删除栏目
        private void button_columns_delete_Click(object sender, EventArgs e)
        {
            if (this.listView_columns.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要删除的事项");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要删除选定的 "+this.listView_columns.SelectedItems.Count.ToString()+" 个事项? ",
                "PrintOptionDlg",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;


            while (this.listView_columns.SelectedItems.Count>0)
            {
                this.listView_columns.Items.Remove(this.listView_columns.SelectedItems[0]);
            }

            // 删除事项后，当前已选择事项的上下移动的可能性会有所改变
            listView_columns_SelectedIndexChanged(sender, null);
        }

        // 向上移动(栏目)
        private void button_columns_moveUp_Click(object sender, EventArgs e)
        {
            if (this.listView_columns.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移动的事项");
                return;
            }

            int nIndex = this.listView_columns.SelectedIndices[0];

            if (nIndex == 0)
            {
                MessageBox.Show(this, "已在顶部");
                return;
            }

            ListViewItem item = this.listView_columns.SelectedItems[0];

            this.listView_columns.Items.Remove(item);
            this.listView_columns.Items.Insert(nIndex - 1, item);
        }

        // 向下移动(栏目)
        private void button_columns_moveDown_Click(object sender, EventArgs e)
        {
            if (this.listView_columns.SelectedItems.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要移动的事项");
                return;
            }

            int nIndex = this.listView_columns.SelectedIndices[0];

            if (nIndex >= this.listView_columns.Items.Count - 1)
            {
                MessageBox.Show(this, "已在底部");
                return;
            }

            ListViewItem item = this.listView_columns.SelectedItems[0];

            this.listView_columns.Items.Remove(item);
            this.listView_columns.Items.Insert(nIndex + 1, item);
        }

        private void listView_columns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_columns.SelectedIndices.Count == 0)
            {
                // 没有选择事项
                this.button_columns_delete.Enabled = false;
                this.button_columns_modify.Enabled = false;
                this.button_columns_moveDown.Enabled = false;
                this.button_columns_moveUp.Enabled = false;
                this.button_columns_new.Enabled = true;
            }
            else
            {
                // 有选择事项
                this.button_columns_delete.Enabled = true;
                this.button_columns_modify.Enabled = true;
                if (this.listView_columns.SelectedIndices[0] >= this.listView_columns.Items.Count - 1)
                    this.button_columns_moveDown.Enabled = false;
                else
                    this.button_columns_moveDown.Enabled = true;

                if (this.listView_columns.SelectedIndices[0] == 0)
                    this.button_columns_moveUp.Enabled = false;
                else
                    this.button_columns_moveUp.Enabled = true;

                this.button_columns_new.Enabled = true;

            }
        }

        private void listView_columns_DoubleClick(object sender, EventArgs e)
        {
            this.button_columns_modify_Click(sender, null);
        }

        void LoadTemplates()
        {
            if (this.m_bTempaltesChanged == true)
            {
                DialogResult result = MessageBox.Show(this,
                    "当前模板列表中有改动尚未保存。如此时强行刷新列表，新增和改动的内容会丢失。\r\n\r\n确实要刷新列表? ",
                    "PrintOptionDlg",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;

                RemoveNewCreatedTemplateFiles();
            }


            this.listView_templates.Items.Clear();
            this.textBox_templates_content.Text = "";
            this.textBox_templates_content.Enabled = false;

            this.m_nCurrentTemplateIndex = -1;
            this.m_bTemplateFileContentChanged = false;

            if (this.PrintOption == null)
                return;
            if (this.PrintOption.TemplatePages == null)
                return;

            for (int i = 0; i < this.PrintOption.TemplatePages.Count; i++)
            {
                TemplatePageParam param = this.PrintOption.TemplatePages[i];

                ListViewItem item = new ListViewItem();
                item.Text = param.Caption;
                item.SubItems.Add(param.FilePath);

                this.listView_templates.Items.Add(item);
            }

            this.m_bTempaltesChanged = false;
        }

        private void listView_templates_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshContentToTemplateFile();

            if (this.listView_templates.SelectedItems.Count == 0)
            {
                this.textBox_templates_content.Text = "";
                this.textBox_templates_content.Enabled = false;
                this.m_nCurrentTemplateIndex = -1;
                this.m_bTemplateFileContentChanged = false;
            }
            else
            {
                string strError = "";

                // 保留位置
                this.m_nCurrentTemplateIndex = this.listView_templates.SelectedIndices[0];

                this.textBox_templates_content.Text = "";
                this.textBox_templates_content.Enabled = true;

                string strFilePath = ListViewUtil.GetItemText(this.listView_templates.SelectedItems[0], 1);

                /*
                if (File.Exists(strFilePath) == false)
                {
                    this.m_bTemplateFileContentChanged = false;
                    return;
                }

                Encoding encoding = FileUtil.DetectTextFileEncoding(strFilePath);

                StreamReader sr = null;

                try
                {
                    // TODO: 这里的自动探索文件编码方式功能不正确，
                    // 需要专门编写一个函数来探测文本文件的编码方式
                    // 目前只能用UTF-8编码方式
                    sr = new StreamReader(strFilePath, encoding);
                    this.textBox_templates_content.Text = sr.ReadToEnd();
                    sr.Close();
                    sr = null;
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    goto ERROR1;
                }
                 * */

                string strContent = "";
                // 能自动识别文件内容的编码方式的读入文本文件内容模块
                // return:
                //      -1  出错
                //      0   文件不存在
                //      1   文件存在
                int nRet = Global.ReadTextFileContent(strFilePath,
                    out strContent,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.textBox_templates_content.Text = strContent;

                this.m_bTemplateFileContentChanged = false;
                return;
            ERROR1:
                this.m_bTemplateFileContentChanged = false;
                MessageBox.Show(this, strError);
            }
        }

        private void listView_templates_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("新增模板(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newTemplatePage_Click);
            if (String.IsNullOrEmpty(this.DataDir) == true)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("用Windows记事本打开模板文件(&O)");
            menuItem.Click += new System.EventHandler(this.menu_openTemplateFileByNotepad_Click);
            if (this.listView_templates.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // -----
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteTemplatePages_Click);
            if (this.listView_templates.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.listView_templates,
                new Point(e.X, e.Y));		
        }

        void menu_openTemplateFileByNotepad_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_templates.SelectedItems.Count == 0)
            {
                strError = "尚未选定要用记事本打开的模板文件事项";
                goto ERROR1;
            }
            foreach (ListViewItem item in this.listView_templates.SelectedItems)
            {
                // ListViewItem item = this.listView_templates.SelectedItems[i];
                string strFilePath = ListViewUtil.GetItemText(item, 1);
                if (String.IsNullOrEmpty(strFilePath) == true)
                    continue;

                System.Diagnostics.Process.Start("notepad.exe", strFilePath);
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 新增模板
        void menu_newTemplatePage_Click(object sender, EventArgs e)
        {
            string strError = "";

            REDO_INPUT:
            string strName = DigitalPlatform.InputDlg.GetInput(
                this,
                "请指定模板名",
                "模板名(&T):",
                "",
            this.Font);
            if (strName == null)
                return;

            if (String.IsNullOrEmpty(strName) == true)
            {
                MessageBox.Show(this, "模板名不能为空");
                goto REDO_INPUT;
            }

            // 查重
            ListViewItem dup = ListViewUtil.FindItem(this.listView_templates, strName, 0);
            if (dup != null)
            {
                strError = "模板名 '" + strName + "' 在列表中已经存在，不能重复加入";
                goto ERROR1;
            }

            string strFilePath = "";
            int nRedoCount = 0;
            string strDir = PathUtil.MergePath(this.DataDir, "print_templates");
            PathUtil.CreateDirIfNeed(strDir);
            for (int i = 0; ; i++)
            {
                strFilePath = PathUtil.MergePath(strDir, "template_" + (i+1).ToString());
                if (File.Exists(strFilePath) == false)
                {
                    // 创建一个0字节的文件
                    try
                    {
                        File.Create(strFilePath).Close();
                    }
                    catch (Exception/* ex*/)
                    {
                        if (nRedoCount > 10)
                        {
                            strError = "创建文件 '" + strFilePath + "' 失败...";
                            goto ERROR1;
                        }
                        nRedoCount++;
                        continue;
                    }
                    break;
                }
            }

            // 清除原来已有的选择
            this.listView_templates.SelectedItems.Clear();

            ListViewItem item = new ListViewItem();
            item.Text = strName;
            item.SubItems.Add(strFilePath);
            this.listView_templates.Items.Add(item);
            item.Selected = true;   // 选上新增的事项
            this.m_bTempaltesChanged = true;

            item.EnsureVisible();   // 滚入视野

            this.m_newCreateTemplateFiles.Add(strFilePath);

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menu_deleteTemplatePages_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (this.listView_templates.SelectedItems.Count == 0)
            {
                strError = "尚未选定要删除的模板事项";
                goto ERROR1;
            }

            // 警告
            DialogResult result = MessageBox.Show(this,
                "确实要删除所选定的 " + this.listView_templates.SelectedItems.Count.ToString() + " 项模板文件?\r\n\r\n(警告: 删除操作一旦进行，就无法用打印选项对话框上的“取消”按钮来取消)",
                "PrintOptionDlg",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            // 兑现最后一次对textbox的修改
            // 以免删除后index发生变化，张冠李戴
            this.RefreshContentToTemplateFile();

            for (int i = this.listView_templates.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int index = this.listView_templates.SelectedIndices[i];
                ListViewItem item = this.listView_templates.Items[index];

                string strFilePath = ListViewUtil.GetItemText(item, 1);

                try
                {
                    File.Delete(strFilePath);
                }
                catch (Exception ex)
                {
                    strError = "删除文件 '" + strFilePath + "' 时发生错误: " + ex.Message;
                    //goto ERROR1;
                    MessageBox.Show(this, strError);
                }

                this.m_newCreateTemplateFiles.Remove(strFilePath);

                this.listView_templates.Items.RemoveAt(index);
                this.m_bTempaltesChanged = true;
            }

            SaveTemplatesChanges(); // 修改无法撤销
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void RefreshContentToTemplateFile()
        {
            if (this.m_bTemplateFileContentChanged == false)
                return;

            if (this.m_nCurrentTemplateIndex == -1)
            {
                Debug.Assert(false, "");
                this.m_bTemplateFileContentChanged = false;
                return;
            }

            string strError = "";

            ListViewItem item = this.listView_templates.Items[this.m_nCurrentTemplateIndex];
            string strFilePath = ListViewUtil.GetItemText(item, 1);

            if (String.IsNullOrEmpty(strFilePath) == true)
                return;

            try
            {
                StreamWriter sw = new StreamWriter(strFilePath, false, Encoding.UTF8);
                sw.Write(this.textBox_templates_content.Text);
                sw.Close();
            }
            catch (Exception ex)
            {
                strError = "写入文件 '" + strFilePath + "' 时发生错误：" + ex.Message;
                goto ERROR1;
            }

            this.m_bTemplateFileContentChanged = false;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void textBox_templates_content_TextChanged(object sender, EventArgs e)
        {
            this.m_bTemplateFileContentChanged = true;
        }
    }

}