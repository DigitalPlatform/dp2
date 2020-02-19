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
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            this.numericUpDown_backupChannelMax.Value = ClientInfo.Config.GetInt(
    "config",
    "backupChannelMax",
    5);
            this.numericUpDown_operlogChannelMax.Value = ClientInfo.Config.GetInt(
                "config",
                "operlogChannelMax",
                5);
        }

        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void ConfigForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            ClientInfo.Config.SetInt("config",
                "backupChannelMax",
                (int)this.numericUpDown_backupChannelMax.Value);
            ClientInfo.Config.SetInt("config",
                "operlogChannelMax",
                (int)this.numericUpDown_operlogChannelMax.Value);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
