using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 出纳打印管理窗
    /// </summary>
    public partial class ChargingPrintManageForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ChargingPrintManageForm()
        {
            InitializeComponent();
        }

        private void ChargingPrintManageDlg_Load(object sender, EventArgs e)
        {
            FillPrintedList();
            FillUnprintList();
            FillCurrentContent();
        }

        void FillPrintedList()
        {
            this.listView_printed_list.Items.Clear();

            if (this.MainForm.OperHistory.PrintHostObj == null)
                return;

            for (int i = 0; i < this.MainForm.OperHistory.PrintHostObj.PrintedInfos.Count; i++)
            {
                PrintInfo info = this.MainForm.OperHistory.PrintHostObj.PrintedInfos[i];

                ListViewItem item = new ListViewItem();
                item.Text = info.CurrentReaderBarcode;
                item.SubItems.Add(info.CreateTime.ToString());
                item.Tag = info;

                this.listView_printed_list.Items.Add(item);
            }
        }

        void FillUnprintList()
        {
            this.listView_unprint_list.Items.Clear();

            if (this.MainForm.OperHistory.PrintHostObj == null)
                return;

            for (int i = 0; i < this.MainForm.OperHistory.PrintHostObj.UnprintInfos.Count; i++)
            {
                PrintInfo info = this.MainForm.OperHistory.PrintHostObj.UnprintInfos[i];

                ListViewItem item = new ListViewItem();
                item.Text = info.CurrentReaderBarcode;
                item.SubItems.Add(info.CreateTime.ToString());
                item.Tag = info;

                this.listView_unprint_list.Items.Add(item);
            }
        }

        void FillCurrentContent()
        {
            string strFormat = "";
            string strText = "";

            if (this.MainForm.OperHistory.PrintHostObj == null)
                return;

            this.MainForm.OperHistory.GetPrintContent(this.MainForm.OperHistory.PrintHostObj.PrintInfo,
                out strText,
                out strFormat);
            this.textBox_currentContent.Text = strText;
            if (strFormat == "html")
                this.textBox_currentContent.EnableHtml = true;
            else
                this.textBox_currentContent.EnableHtml = false;

        }

        private void listView_printed_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_printed_list.SelectedIndices.Count == 0)
            {
                this.textBox_printed_oneContent.Text = "";
                this.button_printed_print.Enabled = false;
            }
            else
            {
                this.button_printed_print.Enabled = true;

                PrintInfo info = (PrintInfo)listView_printed_list.SelectedItems[0].Tag;

                string strFormat = "";
                string strText = "";

                this.MainForm.OperHistory.GetPrintContent(info,
                     out strText,
                     out strFormat);
                this.textBox_printed_oneContent.Text = strText;
                if (strFormat == "html")
                {
                    this.textBox_printed_oneContent.EnableHtml = true;
                }
                else
                {
                    this.textBox_printed_oneContent.EnableHtml = false;
                }
            }
        }

        private void button_printed_print_Click(object sender, EventArgs e)
        {
            if (this.listView_printed_list.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要打印的事项");
                return;
            }

            foreach (ListViewItem item in this.listView_printed_list.SelectedItems)
            {
                PrintInfo info = (PrintInfo)item.Tag;
                this.MainForm.OperHistory.Print(info);
            }

        }

        private void listView_unprint_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView_unprint_list.SelectedIndices.Count == 0)
            {
                this.textBox_unprint_oneContent.Text = "";
                this.button_unprint_print.Enabled = false;
            }
            else
            {
                this.button_unprint_print.Enabled = true;

                PrintInfo info = (PrintInfo)listView_unprint_list.SelectedItems[0].Tag;

                string strFormat = "";
                string strText = "";

                this.MainForm.OperHistory.GetPrintContent(info,
                    out strText,
                    out strFormat);
                this.textBox_unprint_oneContent.Text = strText;
                if (strFormat == "html")
                {
                    this.textBox_unprint_oneContent.EnableHtml = true;
                }
                else
                {
                    this.textBox_unprint_oneContent.EnableHtml = false;
                }
            }

        }

        private void button_unprint_print_Click(object sender, EventArgs e)
        {
            if (this.listView_unprint_list.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "尚未选定要打印的事项");
                return;
            }

            foreach (ListViewItem item in this.listView_unprint_list.SelectedItems)
            {
                PrintInfo info = (PrintInfo)item.Tag;
                this.MainForm.OperHistory.Print(info);
            }

        }

        private void button_currentContent_print_Click(object sender, EventArgs e)
        {
            this.MainForm.OperHistory.Print();
        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            FillPrintedList();
            FillUnprintList();
            FillCurrentContent();

        }

        private void button_testPrint_Click(object sender, EventArgs e)
        {
            this.MainForm.OperHistory.TestPrint();
        }

        private void button_currentContent_push_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.MainForm.OperHistory.PrintHostObj == null)
            {
                MessageBox.Show(this, "PrintHostObj尚未初始化");
                return;
            }


            int nRet = this.MainForm.OperHistory.PrintHostObj.PushCurrentToQueue(out strError);

            MessageBox.Show(this, strError);

            // 刷新
            button_refresh_Click(null, null);
        }

        // 清除打印机配置
        private void button_clearPrinterPreference_Click(object sender, EventArgs e)
        {
            this.MainForm.OperHistory.ClearPrinterPreference();
        }

    }
}