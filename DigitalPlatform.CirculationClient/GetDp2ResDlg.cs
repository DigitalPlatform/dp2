using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.CirculationClient
{
    public partial class GetDp2ResDlg : Form
    {
        const int WM_AUTO_CLOSE = API.WM_USER + 200;
        public bool AutoClose = false;  // 对话框口打开后立即关闭?

        public bool EnableNotAsk = false;

        bool m_bLoaded = false;

#if OLD_CHANNEL
        public LibraryChannelCollection dp2Channels = null;
#endif
        public IChannelManager ChannelManager = null;

        // dp2library服务器数组(缺省用户名/密码等)
        public dp2ServerCollection Servers = null;

        public int[] EnabledIndices = null;

        public GetDp2ResDlg()
        {
            InitializeComponent();
        }

        private void GetDp2ResDlg_Load(object sender, EventArgs e)
        {
            this.dp2ResTree1.Servers = this.Servers;	// 引用
#if OLD_CHANNEL
            this.dp2ResTree1.Channels = this.dp2Channels;
#endif
            this.dp2ResTree1.ChannelManager = this.ChannelManager;

            this.dp2ResTree1.EnabledIndices = this.EnabledIndices;  // new int[] { dp2ResTree.RESTYPE_DB };
            this.dp2ResTree1.Fill(null);

            this.m_bLoaded = true;

            if (String.IsNullOrEmpty(this.textBox_path.Text) == false)
            {
                this.dp2ResTree1.ExpandPath(this.textBox_path.Text);
            }

            if (this.EnableNotAsk == true)
                this.checkBox_notAsk.Enabled = true;

            if (this.AutoClose == true)
                API.PostMessage(this.Handle, WM_AUTO_CLOSE, 0, 0);

        }

        private void GetDp2ResDlg_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_path.Text == "")
            {
                MessageBox.Show(this, "尚未指定对象");
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

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_AUTO_CLOSE:
                    this.button_OK_Click(this, null);
                    return;
            }
            base.DefWndProc(ref m);
        }

        private void dp2ResTree1_AfterSelect(object sender,
            TreeViewEventArgs e)
        {
            if (this.EnabledIndices != null
                && StringUtil.IsInList(this.dp2ResTree1.SelectedNode.ImageIndex,
                this.EnabledIndices) == false)
            {
                this.textBox_path.Text = "";
                return;
            }

            string strServerName = "";
            string strServerUrl = "";
            string strDbName = "";
            string strFrom = "";
            string strFromStyle = "";
            string strError = "";

            int nRet = dp2ResTree.GetNodeInfo(this.dp2ResTree1.SelectedNode,
                out strServerName,
                out strServerUrl,
                out strDbName,
                out strFrom,
                out strFromStyle,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (strServerName != "")
                this.textBox_path.Text = strServerName;

            if (strDbName != "")
                this.textBox_path.Text += "/" + strDbName;

            if (strFrom != "")
                this.textBox_path.Text += "/" + strFrom;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public string Path
        {
            get
            {
                return this.textBox_path.Text;
            }
            set
            {
                if (this.m_bLoaded == true)
                    this.dp2ResTree1.ExpandPath(value);

                this.textBox_path.Text = value;
            }
        }

        public bool NotAsk
        {
            get
            {
                return this.checkBox_notAsk.Checked;
            }
            set
            {
                this.checkBox_notAsk.Checked = value;
            }
        }
    }
}