using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RfidTool
{
    public partial class MainForm : Form
    {
        ScanDialog _scanDialog = new ScanDialog();

        public MainForm()
        {
            InitializeComponent();

            this.MenuItem_exit.Click += MenuItem_exit_Click;
            this.MenuItem_writeBookTags.Click += MenuItem_writeBookTags_Click;

            _scanDialog.FormClosing += _scanDialog_FormClosing;
        }

        private void _scanDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dialog = sender as Form;

            // 将关闭改为隐藏
            dialog.Visible = false;
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        // 开始(扫描并)写入图书标签
        private void MenuItem_writeBookTags_Click(object sender, System.EventArgs e)
        {
            // 把扫描对话框打开
            if (_scanDialog.Visible == false)
                _scanDialog.Show(this);

        }

        // 退出
        private void MenuItem_exit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Enabled = false;
            _ = Task.Run(() =>
            {
                DataModel.InitialDriver();
                this.Invoke((Action)(() =>
                {
                    this.Enabled = true;
                }));
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DataModel.ReleaseDriver();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
