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

using DigitalPlatform.GUI;
using DigitalPlatform.Core;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform;

namespace RfidTool
{
    public partial class ModifyDialog : Form
    {
        public event WriteCompleteEventHandler WriteComplete = null;

        public ModifyDialog()
        {
            InitializeComponent();

            _errorTable = new ErrorTable((s) =>
            {
                try
                {
                    this.Invoke((Action)(() =>
                    {

                    }));
                }
                catch (ObjectDisposedException)
                {

                }
            });
        }

        private void ModifyDialog_Load(object sender, EventArgs e)
        {

        }

        private void ModifyDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancel?.Cancel();
        }

        private void ModifyDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        ActionInfo _action = null;

        // 开始修改
        private void toolStripButton_begin_Click(object sender, EventArgs e)
        {
            using (BeginModifyDialog dlg = new BeginModifyDialog())
            {
                GuiUtil.SetControlFont(dlg, this.Font);
                ClientInfo.MemoryState(dlg, "beginModifyDialog", "state");

                dlg.UiState = ClientInfo.Config.Get("BeginModifyDialog", "uiState", "");
                dlg.ShowDialog(this);
                ClientInfo.Config.Set("BeginModifyDialog", "uiState", dlg.UiState);

                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                _action = new ActionInfo
                {
                    OI = dlg.OiString,
                    AOI = dlg.AoiString,
                    LinkUID = dlg.LinkUID,
                };
            }

            // TODO: 开始前用一个概要对话框显示确认一下本次批处理要进行的修改操作
            _cancel?.Dispose();
            _cancel = new CancellationTokenSource();
            BeginModify(_cancel.Token);
        }

        private void toolStripButton_stop_Click(object sender, EventArgs e)
        {
            _cancel?.Cancel();
        }

        // 导出 UID --> PII 对照关系
        private void toolStripButton_exportUidPiiMap_Click(object sender, EventArgs e)
        {

        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.listView_tags,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.listView_tags,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        CancellationTokenSource _cancel = new CancellationTokenSource();

        void BeginModify(CancellationToken token)
        {
            this.toolStripButton_begin.Enabled = false;
            this.toolStripButton_stop.Enabled = true;
            _ = Task.Factory.StartNew(
                async () =>
                {
                    try
                    {
                        while (token.IsCancellationRequested == false)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), token);

                            // 语音提示倒计时开始盘点
                            await SpeakCounter(token);

                            string readerNameList = "*";
                            var result = DataModel.ListTags(readerNameList, "");
                            if (result.Value == -1)
                                ShowMessageBox("inventory", result.ErrorInfo);
                            else
                            {
                                ShowMessageBox("inventory", null);
                            }

                            // 语音或音乐提示正在处理，不要移动天线
                            await Task.Delay(TimeSpan.FromSeconds(5), token);
                        }
                    }
                    catch (TaskCanceledException)
                    {

                    }
                    catch (Exception ex)
                    {
                        ClientInfo.WriteErrorLog($"修改循环出现异常: {ExceptionUtil.GetDebugText(ex)}");
                        ShowMessageBox("inventory", $"修改循环因为异常已终止: {ex.Message}");
                    }
                    finally
                    {
                        this.Invoke((Action)(() =>
                        {
                            // 把按钮状态复原到未启动状态
                            this.toolStripButton_begin.Enabled = true;
                            this.toolStripButton_stop.Enabled = false;
                        }));
                    }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

        }

        // 语音提示倒计时
        async Task SpeakCounter(CancellationToken token)
        {
            for (int i = 5; i > 0; i--)
            {
                FormClientInfo.Speak($"{i}", false, true);
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            FormClientInfo.Speak($"开始扫描", false, true);
        }

        #region Error Dialog

        ErrorTable _errorTable = null;

        FloatingErrorDialog _errorDialog = null;

        void CreateErrorDialog()
        {
            if (_errorDialog == null)
            {
                _errorDialog = new FloatingErrorDialog();

                _errorDialog.FormClosing += _errorDialog_FormClosing;

                /*
                GuiUtil.SetControlFont(_errorDialog, this.Font);
                ClientInfo.MemoryState(_errorDialog, "scanDialog", "state");
                _errorDialog.UiState = ClientInfo.Config.Get("scanDialog", "uiState", null);
                */
            }
        }
        private void _errorDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dialog = sender as Form;

            // 将关闭改为隐藏
            dialog.Visible = false;
            if (e.CloseReason == CloseReason.UserClosing)
                e.Cancel = true;
        }

        void ShowMessageBox(string type, string text)
        {
            // 语音提示出错
            if (text != null)
                FormClientInfo.Speak(text, false, false);

            this.Invoke((Action)(() =>
            {
                CreateErrorDialog();
                if (text == null)
                    _errorDialog.Hide();
                else
                {
                    if (_errorDialog.Visible == false)
                    {
                        _errorDialog.Show(this);
                    }
                }

                _errorTable.SetError(type, text);
                _errorDialog.Message = _errorTable.GetError(false);
            }));
        }

        #endregion


    }

    // 修改动作
    class ActionInfo
    {
        public string OI { get; set; }
        public string AOI { get; set; }

        public bool LinkUID { get; set; }
    }
}
