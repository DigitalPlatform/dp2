using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 宏值编辑对话框
    /// </summary>
    public partial class MacroTableDialog : Form
    {
        public string XmlFileName = "";

        public MacroTableDialog()
        {
            InitializeComponent();
        }

        private void MacroTableDialog_Load(object sender, EventArgs e)
        {
            LoadXmlFile();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (SaveXmlFile() == true)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

        }

        bool SaveXmlFile()
        {
            if (string.IsNullOrEmpty(this.XmlFileName) == true)
                return false;

            string strError = "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<root />");

            int i = 0;
            foreach(DataGridViewRow row in this.dataGridView1.Rows)
            {
                if (row.IsNewRow == true)
                    continue;

                string strName = row.Cells[0].Value as string;
                string strValue = row.Cells[1].Value as string;

                if (string.IsNullOrEmpty(strName) == true)
                {
                    strError = "第" + (i+1).ToString() + "行：宏名不能为空";
                    goto ERROR1;
                }
                if (string.IsNullOrEmpty(strValue) == true)
                {
                    strError = "第" + (i + 1).ToString() + "行：宏值不能为空";
                    goto ERROR1;
                }

                XmlElement item = dom.CreateElement("item");
                dom.DocumentElement.AppendChild(item);

                item.SetAttribute("name", strName);
                item.InnerText = strValue;

                i++;
            }

            dom.Save(this.XmlFileName);
            return true;
        ERROR1:
            MessageBox.Show(this, strError);
            return false;
        }

        void LoadXmlFile()
        {
            if (string.IsNullOrEmpty(this.XmlFileName) == true)
                return;

            string strError = "";
            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(this.XmlFileName);
            }
            catch (FileNotFoundException)
            {
                dom.LoadXml("<root />");
            }
            catch (Exception ex)
            {
                strError = "文件 '"+this.XmlFileName+"' 装入 XMLDOM 时出错: " + ex.Message;
                goto ERROR1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
            foreach (XmlElement item in nodes)
            {
                string strName = item.GetAttribute("name");
                string strValue = item.InnerText.Trim();

                int index = this.dataGridView1.Rows.Add();
                DataGridViewRow row = this.dataGridView1.Rows[index];

                row.Cells[0].Value = strName;
                row.Cells[1].Value = strValue;

#if NO
                DataGridViewRow row = new DataGridViewRow();

                var nameCell = new DataGridViewComboBoxCell();
                // nameCell.Items.Add(strName);
                nameCell.Value = strName;

                var incStyleCell = new DataGridViewComboBoxCell();
                // incStyleCell.Items.Add("test");
                incStyleCell.Value = "test";

                var valueCell = new DataGridViewTextBoxCell();
                valueCell.Value = strValue;

                row.Cells.Add(nameCell);
                row.Cells.Add(incStyleCell);
                row.Cells.Add(valueCell);

                this.dataGridView1.Rows.Add(row);
#endif

            }

            return;
        ERROR1:
            this.MessageBoxShow(strError);
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = false;
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            int nCount = this.dataGridView1.SelectedRows.Count;
            if (nCount == 0 && this.dataGridView1.CurrentRow != null)
                nCount = 1;
            menuItem = new MenuItem("删除 [" + nCount.ToString() + "] (&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteSelectedRow_Click);
            if (nCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.dataGridView1, new Point(e.X, e.Y));	
        }

        void menu_deleteSelectedRow_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            foreach(DataGridViewRow row in this.dataGridView1.SelectedRows)
            {
                rows.Add(row);
            }

            if (rows.Count == 0 && this.dataGridView1.CurrentRow != null)
                rows.Add(this.dataGridView1.CurrentRow);

            foreach (DataGridViewRow row in rows)
            {
                if (row.IsNewRow == false)
                    this.dataGridView1.Rows.Remove(row);
            }
        }

#if NO
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is ComboBox)
            {
                ComboBox combobox = e.Control as ComboBox;
                combobox.DropDownStyle = ComboBoxStyle.DropDown;
            }
        }
#endif
    }
}
