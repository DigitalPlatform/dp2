using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 编辑资源对象的对话框
    /// </summary>
    public partial class ResObjectDlg : Form
    {
        public ResObjectDlg()
        {
            InitializeComponent();
        }

        public string ID
        {
            get
            {
                return this.textBox_serverName.Text;
            }
            set
            {
                this.textBox_serverName.Text = value;
            }
        }

        public string State
        {
            get
            {
                return this.textBox_state.Text;
            }
            set
            {
                this.textBox_state.Text = value;
            }
        }

        public string Mime
        {
            get
            {
                return this.textBox_mime.Text;
            }
            set
            {
                this.textBox_mime.Text = value;
            }
        }

        public string LocalPath
        {
            get
            {
                return this.textBox_localPath.Text;
            }
            set
            {
                this.m_nDisableLocalPathTextChange++;
                this.textBox_localPath.Text = value;
                this.m_nDisableLocalPathTextChange--;
            }
        }

        public string SizeString
        {
            get
            {
                return this.textBox_size.Text;
            }
            set
            {
                this.textBox_size.Text = value;
            }
        }

        public string Timestamp
        {
            get
            {
                return this.textBox_timestamp.Text;
            }
            set
            {
                this.textBox_timestamp.Text = value;
            }
        }

        public string Usage
        {
            get
            {
                return this.textBox_usage.Text;
            }
            set
            {
                this.textBox_usage.Text = value;
            }
        }

        public string Rights
        {
            get
            {
                return this.textBox_rights.Text;
            }
            set
            {
                this.textBox_rights.Text = value;
            }
        }

        bool m_bResChanged = false; // 对象被改变过
        public bool ResChanged
        {
            get
            {
                return this.m_bResChanged;
            }
            set
            {
                this.m_bResChanged = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (textBox_localPath.Text == "")
            {
                MessageBox.Show(this, "尚未指定文件本地路径");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_findLocalPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择文件";
            //dlg.InitialDirectory = "c:\\" ;
            //dlg.FileName = itemSelected.Text ;
            dlg.Filter = "All files (*.*)|*.*";
            dlg.FilterIndex = 2;
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            FileInfo fileInfo = new FileInfo(dlg.FileName);

            this.textBox_localPath.Text = fileInfo.FullName;
            this.textBox_size.Text = Convert.ToString(fileInfo.Length);
            this.textBox_state.Text = "尚未上载";

            textBox_mime.Text = API.MimeTypeFrom(ReadFirst256Bytes(dlg.FileName),
                "");
        }

        public int SetObjectFilePath(string strObjectFilePath,
            out string strError)
        {
            strError = "";

            FileInfo fileInfo = new FileInfo(strObjectFilePath);
            if (fileInfo.Exists == false)
            {
                strError = "文件 '"+strObjectFilePath+"' 尚不存在";
                return -1;
            }

            this.textBox_localPath.Text = fileInfo.FullName;
            this.textBox_size.Text = Convert.ToString(fileInfo.Length);
            this.textBox_state.Text = "尚未上载";

            textBox_mime.Text = API.MimeTypeFrom(ReadFirst256Bytes(strObjectFilePath),
                "");

            return 0;
        }

        // 读取文件前256bytes
        public static byte[] ReadFirst256Bytes(string strFileName)
        {
            using (FileStream fileSource = File.Open(
                strFileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {

                byte[] result = new byte[Math.Min(256, fileSource.Length)];
                fileSource.Read(result, 0, result.Length);

                return result;
            }
        }

        int m_nDisableLocalPathTextChange = 0;

        private void textBox_localPath_TextChanged(object sender, EventArgs e)
        {
            if (m_nDisableLocalPathTextChange > 0)
                return;

            this.m_bResChanged = true;
        }

        /// <summary>
        /// 权限值配置文件全路径
        /// </summary>
        public string RightsCfgFileName
        {
            get;
            set;
        }

        private void button_editRights_Click(object sender, EventArgs e)
        {
            DigitalPlatform.CommonDialog.PropertyDlg dlg = new DigitalPlatform.CommonDialog.PropertyDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Text = "对象的权限";
            dlg.PropertyString = this.textBox_rights.Text;
            dlg.CfgFileName = RightsCfgFileName;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_rights.Text = dlg.PropertyString;
        }
    }
}