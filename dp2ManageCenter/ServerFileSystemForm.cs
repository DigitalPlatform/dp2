using DigitalPlatform.CirculationClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2ManageCenter
{
    // 显示和管理 dp2library 服务器端文件系统的窗口
    public partial class ServerFileSystemForm : Form
    {
        string _serverName = null;
        public string ServerName
        {
            get
            {
                return _serverName;
            }
            set
            {
                _serverName = value;
                RefreshTitle();
            }
        }

        public string ServerUrl { get; set; }

        public string SelectedPath { get; set; }

        public ServerFileSystemForm()
        {
            InitializeComponent();
        }

        private void ServerFileSystemForm_Load(object sender, EventArgs e)
        {
            RefreshTitle();
            this.kernelResTree1.Fill();

            if (string.IsNullOrEmpty(this.SelectedPath) == false)
            {
                this.BeginInvoke((Action)(() =>
                {
                    this.kernelResTree1.ExpandPath(this.SelectedPath, out string strError);
                }));
            }
        }

        private void ServerFileSystemForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ServerFileSystemForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void kernelResTree1_GetChannel(object sender, DigitalPlatform.LibraryClient.GetChannelEventArgs e)
        {
            e.Channel = ((ClientInfo.MainForm) as MainForm).GetChannel(this.ServerUrl);
        }

        private void kernelResTree1_ReturnChannel(object sender, DigitalPlatform.LibraryClient.ReturnChannelEventArgs e)
        {
            ((ClientInfo.MainForm) as MainForm).ReturnChannel(e.Channel);
        }

        void RefreshTitle()
        {
            if (this.IsHandleCreated && this.IsDisposed == false)
                this.Invoke((Action)(() =>
                {
                    this.Text = $"服务器 '{_serverName}' 的文件夹";
                }));
        }

        private void toolStripButton_refresh_Click(object sender, EventArgs e)
        {
            this.kernelResTree1.Refresh(this.kernelResTree1.SelectedNode);
        }
    }
}
