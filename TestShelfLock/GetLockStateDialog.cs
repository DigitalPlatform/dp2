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

using DigitalPlatform.CommonControl;

namespace TestShelfLock
{
    public partial class GetLockStateDialog : Form
    {
        ShelfLockDriver _driver = null;

        public GetLockStateDialog()
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

        public string LockPath
        {
            get
            {
                return this.textBox_lockPath.Text;
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

        public Button GetLockStateButton
        {
            get
            {
                return this.button_getLockState;
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(new ComboBoxText(this.comboBox_comPort));
                controls.Add(this.textBox_lockPath);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(new ComboBoxText(this.comboBox_comPort));
                controls.Add(this.textBox_lockPath);
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_getLockState_Click(object sender, EventArgs e)
        {
            OpenDriver();

            var result = _driver.GetShelfLockState(this.textBox_lockPath.Text);
            this.textBox_result.Text = result.ToString();
        }

        private void GetLockStateDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cancel();
            _driver?.ReleaseDriver();
        }

        void Cancel()
        {
            _cancel?.Cancel();
            _cancel?.Dispose();
            _cancel = null;
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        private async void button_loopQuery_Click(object sender, EventArgs e)
        {
            this.button_stopLoop.Enabled = true;
            this.button_loopQuery.Enabled = false;

            Cancel();
            _cancel = new CancellationTokenSource();
            var token = _cancel.Token;

            OpenDriver();

            await Task.Run(() =>
            {
                int count = 0;
                while (token.IsCancellationRequested == false)
                {
                    var result = _driver.GetShelfLockState(this.textBox_lockPath.Text);

                    this.Invoke((Action)(() =>
                    {
                        this.textBox_result.Text = count.ToString() + "\r\n" + result.ToString();
                    }));

                    if (result.Value == -1)
                        break;
                    count++;
                }
            });
        }

        private void button_stopLoop_Click(object sender, EventArgs e)
        {
            Cancel();
            this.button_stopLoop.Enabled = false;
            this.button_loopQuery.Enabled = true;
        }

        void OpenDriver()
        {
            if (_driver == null)
            {
                _driver = new ShelfLockDriver();
                _driver.InitializeDriver(
                    new LockProperty
                    {
                        SerialPort = this.comboBox_comPort.Text,
                    },
                    "");
            }
        }
    }
}
