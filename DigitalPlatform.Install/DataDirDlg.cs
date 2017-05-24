using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform.IO;

namespace DigitalPlatform.Install
{
    public partial class DataDirDlg : Form
    {
        public string MessageBoxTitle = "setup";

        public DataDirDlg()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.DataDir == "")
            {
                MessageBox.Show(this, "尚未指定数据目录。");
                return;
            }

        REDO:
            if (Directory.Exists(this.DataDir) == false)
            {
                string strText = "数据目录 '" + this.DataDir + "' 不存在。\r\n\r\n是否创建此目录?";
                DialogResult result = MessageBox.Show(this,
                    strText,
                    this.MessageBoxTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result == DialogResult.No)
                {
                    MessageBox.Show(this, "请手动创建数据目录 " + this.DataDir);
                    return;
                }

                PathUtil.TryCreateDir(this.DataDir);

                if (Directory.Exists(this.DataDir) == false)
                {
                    MessageBox.Show(this, "数据目录 " + this.DataDir + "创建失败。");
                    return;
                }
                goto REDO;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
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

        // 注释
        public string Comment
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }
    }
}