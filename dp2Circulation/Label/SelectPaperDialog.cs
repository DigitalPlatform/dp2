using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;

using DigitalPlatform.GUI;

namespace dp2Circulation
{
    /// <summary>
    /// 针对一个特定的打印机选定纸张
    /// </summary>
    public partial class SelectPaperDialog : Form
    {
        public PrintDocument Document = null;

        public SelectPaperDialog()
        {
            InitializeComponent();
        }

        private void SelectPaperDialog_Load(object sender, EventArgs e)
        {
            if (this.Document != null)
            {
                this.textBox_printerName.Text = this.Document.PrinterSettings.PrinterName;
            }

            FillList();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_paperName.Text) == true)
            {
                strError = "请选定一种纸张类型";
                goto ERROR1;
            }
            SelectPaperSize(this.textBox_paperName.Text);

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        const int COLUMN_PAPERNAME = 0;
        const int COLUMN_KIND = 1;
        const int COLUMN_WIDTH = 2;
        const int COLUMN_HEIGHT = 3;

        void FillList()
        {
            this.listView_papers.Items.Clear();

            if (this.Document == null)
                return;

            foreach (PaperSize ps in this.Document.PrinterSettings.PaperSizes)
            {
                ListViewItem item = new ListViewItem();
                ListViewUtil.ChangeItemText(item, COLUMN_PAPERNAME, ps.PaperName);
                ListViewUtil.ChangeItemText(item, COLUMN_KIND, ps.Kind.ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_WIDTH, ps.Width.ToString());
                ListViewUtil.ChangeItemText(item, COLUMN_HEIGHT, ps.Height.ToString());
                this.listView_papers.Items.Add(item);
            }

            if (this.listView_papers.Items.Count > 0)
            {
                ListViewItem select = ListViewUtil.FindItem(this.listView_papers, this.textBox_paperName.Text, COLUMN_PAPERNAME);
                if (select != null)
                    ListViewUtil.SelectLine(select, true);
                else
                    ListViewUtil.SelectLine(this.listView_papers, 0, true);
            }
        }

        public string SelectedPaperName
        {
            get
            {
                return this.textBox_paperName.Text;
            }
            set
            {
                this.textBox_paperName.Text = value;

                if (this.listView_papers.Items.Count > 0)
                {
                    ListViewUtil.ClearSelection(this.listView_papers);
                    ListViewItem select = ListViewUtil.FindItem(this.listView_papers, value, COLUMN_PAPERNAME);
                    if (select != null)
                    {
                        ListViewUtil.SelectLine(select, true);
                    }
                }
            }
        }

        bool SelectPaperSize(string strPaperName)
        {
            if (this.Document == null)
                return false;

            foreach (PaperSize ps in this.Document.PrinterSettings.PaperSizes)
            {
                if (ps.PaperName.Equals(strPaperName) == true)
                {
                    this.Document.PrinterSettings.DefaultPageSettings.PaperSize = ps;
                    this.Document.DefaultPageSettings.PaperSize = ps;
                    return true;
                }
            }

            return false;
        }

        public string Comment
        {
            get
            {
                return this.label_comment.Text;
            }
            set
            {
                this.label_comment.Text = value;

                if (string.IsNullOrEmpty(this.label_comment.Text) == true)
                    this.label_comment.Visible = false;
                else
                    this.label_comment.Visible = true;
            }
        }

        private void listView_papers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_papers.SelectedItems.Count == 1)
            {
                this.textBox_paperName.Text = ListViewUtil.GetItemText(this.listView_papers.SelectedItems[0], COLUMN_PAPERNAME);
            }
            else
                this.textBox_paperName.Text = "";
        }

        private void listView_papers_DoubleClick(object sender, EventArgs e)
        {
            button_OK_Click(sender, e);
        }
    }
}
