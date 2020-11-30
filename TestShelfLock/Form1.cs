using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;

namespace TestShelfLock
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void MenuItem_openLock_Click(object sender, EventArgs e)
        {
            ShelfLockDriver driver = null;
            try
            {
                using (OpenLockDialog dlg = new OpenLockDialog())
                {
                    dlg.UiState = ClientInfo.Config.Get("openShelfLockDialog",
                        "uiState",
                        null);

                    dlg.OpenLockButton.Click += (s1, e1) =>
                    {
                        int cardNumber = Convert.ToInt32(dlg.CardNumber);
                        int lockNumber = Convert.ToInt32(dlg.LockNumber);

                        if (driver == null)
                        {
                            driver = new ShelfLockDriver();
                            driver.InitializeDriver(
                                new LockProperty
                                {
                                    SerialPort = dlg.ComPort
                                },
                                "");
                        }
                        var ret = driver.OpenShelfLock($"*.{cardNumber}.{lockNumber}", "");

                        if (ret.Value == -1)
                        {
                            // MessageBox.Show(this, ret.ErrorInfo);
                            dlg.ResultString = $"sendMessage error: {ret.ErrorInfo}";
                            return;
                        }
                        dlg.ResultString = ret.Value.ToString();
                    };
                    dlg.ShowDialog(this);
                    ClientInfo.Config.Set("openShelfLockDialog",
    "uiState",
    dlg.UiState);

                }
            }
            finally
            {
                driver?.ReleaseDriver();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var ret = ClientInfo.Initial("TestShelfLock");
            if (ret == false)
            {
                Application.Exit();
                return;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClientInfo.Finish();
        }

        private void MenuItem_getLockState_Click(object sender, EventArgs e)
        {
            using (GetLockStateDialog dlg = new GetLockStateDialog())
            {
                dlg.UiState = ClientInfo.Config.Get("GetLockStateDialog",
                    "uiState",
                    null);
                dlg.ShowDialog(this);
                ClientInfo.Config.Set("GetLockStateDialog",
"uiState",
dlg.UiState);

            }

        }

#if REMOVED


        public class SendMessageResult : NormalResult
        {
            public byte[] RecieveBytes { get; set; }
        }

        // 发送串口指令
        public static Task<SendMessageResult> sendMessage(
            string portName,
            byte[] bytes)
        {
            ComModel _comModel = new ComModel();
            try
            {
                var tcs = new TaskCompletionSource<SendMessageResult>();
                _comModel.comReceiveDataEvent += (sender, e) =>
                {
                    tcs.SetResult(new SendMessageResult { RecieveBytes = e.receivedBytes });
                    _comModel.Close();
                };

                // TODO: 收到错误事件，也要关闭 COM 口

                // 打开串口
                var ret = _comModel.Open(portName,
                    "9600",//baudRate,
                    "8",//dataBits,
                    "One",//stopBits,
                    "None",//parity,
                    "None",//handshake
                    out string error);
                if (ret == false)
                    return Task.FromResult(new SendMessageResult
                    {
                        Value = -1,
                        ErrorInfo = $"Open() serial port error: {error}"
                    });

                // 给串口发送二进制
                bool bRet = _comModel.Send(bytes, out string strError);
                if (bRet == false)
                {
                    return Task.FromResult(new SendMessageResult
                    {
                        Value = -1,
                        ErrorInfo = $"给串口({portName})发送消息失败: {strError}"
                    });
                }

                return tcs.Task;
            }
            catch (Exception ex)
            {
                return Task.FromResult(new SendMessageResult
                {
                    Value = -1,
                    ErrorInfo = ex.Message,
                });
            }
            finally
            {
                // 关闭串口
                // _comModel.Close();
            }
        }

#endif

        /*
        public static Task WhenDocumentCompleted(this WebBrowser browser)
        {
            var tcs = new TaskCompletionSource<bool>();
            browser.DocumentCompleted += (s, args) => tcs.SetResult(true);
            return tcs.Task;
        }
        */
    }
}
