using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.IO;

namespace DigitalPlatform.Install
{
    public partial class SetupMongoDbDialog : Form
    {
        public SetupMongoDbDialog()
        {
            InitializeComponent();
        }

        private void SetupMongoDbDialog_Load(object sender, EventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_dataDir.Text))
            {
                strError = "尚未指定 MongoDB 数据目录";
                goto ERROR1;
            }

            // return:
            //      false   已经存在
            //      true    刚刚新创建
            // exception:
            //      可能会抛出异常 System.IO.DirectoryNotFoundException (未能找到路径“...”的一部分)
            PathUtil.TryCreateDir(this.textBox_dataDir.Text);

            if (string.IsNullOrEmpty(this.textBox_binDir.Text))
            {
                strError = "尚未指定 mongod.exe 所在";
                goto ERROR1;
            }

            string strExePath = Path.Combine(this.textBox_binDir.Text, "mongod.exe");
            if (File.Exists(strExePath) == false)
            {
                strError = "您指定的目录 '"+this.textBox_binDir.Text+"' 中并未包含 mongod.exe。请重新指定";
                goto ERROR1;
            }

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

        private void button_findDataDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定 MongoDB 数据目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = true;
            dir_dlg.SelectedPath = this.textBox_dataDir.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_dataDir.Text = dir_dlg.SelectedPath;
        }

        public string DataDir
        {
            get
            {
                return this.textBox_dataDir.Text;
            }
            set
            {
                this.textBox_dataDir.Text = value;
            }
        }

        public string BinDir
        {
            get
            {
                return this.textBox_binDir.Text;
            }
            set
            {
                this.textBox_binDir.Text = value;
            }
        }

        private void button_findBinDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir_dlg = new FolderBrowserDialog();

            dir_dlg.Description = "请指定 mongod.exe 所在目录:";
            dir_dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dir_dlg.ShowNewFolderButton = false;
            dir_dlg.SelectedPath = this.textBox_binDir.Text;

            if (dir_dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_binDir.Text = dir_dlg.SelectedPath;
        }
    }
}
