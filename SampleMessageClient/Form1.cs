using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace SampleMessageClient
{
    public partial class Form1 : Form
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public Form1()
        {
            ClientInfo.ProgramName = "dp2inventory";
            FormClientInfo.MainForm = this;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var ret = FormClientInfo.Initial("samplemessageclient", null);
            if (ret == false)
            {
                Application.Exit();
                return;
            }

            LoadSettings();

            DataModel.StartMessageThread(_cancel.Token);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Dispose();

            SaveSettings();
        }

        void LoadSettings()
        {
            // this.UiState = ClientInfo.Config.Get("global", "ui_state", "");

            // 恢复 MainForm 的显示状态
            {
                var state = ClientInfo.Config.Get("mainForm", "state", "");
                if (string.IsNullOrEmpty(state) == false)
                {
                    FormProperty.SetProperty(state, this, ClientInfo.IsMinimizeMode());
                }
            }
        }

        void SaveSettings()
        {
            // 保存 MainForm 的显示状态
            {
                var state = FormProperty.GetProperty(this);
                ClientInfo.Config.Set("mainForm", "state", state);
            }

            // ClientInfo.Config?.Set("global", "ui_state", this.UiState);
            ClientInfo.Finish();
        }

        private void MenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuItem_settings_Click(object sender, EventArgs e)
        {
            using (SettingsDialog dlg = new SettingsDialog())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "settingsDialog", "state");
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {

                }
            }
        }
    }
}
