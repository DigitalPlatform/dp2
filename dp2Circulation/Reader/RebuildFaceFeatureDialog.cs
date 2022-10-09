using DigitalPlatform.CommonControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2Circulation.Reader
{
    public partial class RebuildFaceFeatureDialog : Form
    {
        public RebuildFaceFeatureDialog()
        {
            InitializeComponent();
        }

        private void RebuildFaceFeatureDialog_Load(object sender, EventArgs e)
        {

        }

        private void RebuildFaceFeatureDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public bool OnlyRebuildExistingFaceElement
        {
            get
            {
                return this.checkBox_onlyRebuildExistingFaceElement.Checked;
            }
            set
            {
                this.checkBox_onlyRebuildExistingFaceElement.Checked = value;
            }
        }

        public bool SearchDup
        {
            get
            {
                return this.checkBox_searchDup.Checked;
            }
            set
            {
                this.checkBox_searchDup.Checked = value;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    new ControlWrapper(this.checkBox_onlyRebuildExistingFaceElement, true),
                    new ControlWrapper(this.checkBox_searchDup, true),
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    new ControlWrapper(this.checkBox_onlyRebuildExistingFaceElement, true),
                    new ControlWrapper(this.checkBox_searchDup, true),
                };
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
