using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;

namespace dp2Inventory
{
    public partial class BeginInventoryDialog : Form
    {
        public BeginInventoryDialog()
        {
            InitializeComponent();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.checkBox_action_setUID,
                    this.checkBox_action_setCurrentLocation,
                    this.checkBox_action_setLocation,
                    this.checkBox_action_verifyEas,
                    this.comboBox_action_location,
                    this.checkBox_action_slowMode,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.checkBox_action_setUID,
                    this.checkBox_action_setCurrentLocation,
                    this.checkBox_action_setLocation,
                    this.checkBox_action_verifyEas,
                    this.comboBox_action_location,
                    this.checkBox_action_slowMode,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BeginModifyDialog_Load(object sender, EventArgs e)
        {
            // this.textBox_verifyRule.Text = DataModel.PiiVerifyRule;
        }

        public bool ActionSetUID
        {
            get
            {
                return this.checkBox_action_setUID.Checked;
            }
            set
            {
                this.checkBox_action_setUID.Checked = value;
            }
        }
    }
}
