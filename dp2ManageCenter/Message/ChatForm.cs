using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CirculationClient;

namespace dp2ManageCenter.Message
{
    /// <summary>
    /// 聊天窗口
    /// </summary>
    public partial class ChatForm : Form
    {
        string _userNameAndUrl = "";

        public string UserNameAndUrl
        {
            get
            {
                return _userNameAndUrl;
            }
            set
            {
                _userNameAndUrl = value;
                this.Text = $"聊天 {_userNameAndUrl}";
            }
        }

        public ChatForm()
        {
            InitializeComponent();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        void LoadSettings()
        {
            this.UserNameAndUrl = ClientInfo.Config.Get("chat", "userNameAndUrl", "");
        }

        void SaveSettings()
        {
            ClientInfo.Config.Set("chat", "userNameAndUrl", this.UserNameAndUrl);
        }

        private void toolStripButton_selectAccount_Click(object sender, EventArgs e)
        {
            using (MessageAccountForm dlg = new MessageAccountForm())
            {
                dlg.Mode = "select";
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;
                var account = dlg.SelectedAccount;
                this.UserNameAndUrl = account.UserName + "@" + account.ServerUrl;
            }
        }
    }
}
