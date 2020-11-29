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

namespace TestShelfLock
{
    public partial class OpenLockDialog : Form
    {
        public OpenLockDialog()
        {
            InitializeComponent();
        }

        public string ComPort
        {
            get
            {
                return this.comboBox_comPort.Text;
            }
        }

        public string CardNumber
        {
            get
            {
                return this.comboBox_cardNo.Text;
            }
        }

        public string LockNumber
        {
            get
            {
                return this.comboBox_lockNo.Text;
            }
        }

        public string ResultString
        {
            get
            {
                return this.textBox_result.Text;
            }
            set
            {
                this.textBox_result.Text = value;
            }
        }

        public Button OpenLockButton
        {
            get
            {
                return this.button_openLock;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(new ComboBoxText(this.comboBox_comPort));
                controls.Add(new ComboBoxText(this.comboBox_cardNo));
                controls.Add(new ComboBoxText(this.comboBox_lockNo));
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(new ComboBoxText(this.comboBox_comPort));
                controls.Add(new ComboBoxText(this.comboBox_cardNo));
                controls.Add(new ComboBoxText(this.comboBox_lockNo));
                GuiState.SetUiState(controls, value);
            }
        }

    }
}
