using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MessageClient;
using DigitalPlatform.Text;

namespace dp2ManageCenter.Message
{
    public partial class GetFileDialog : Form
    {
        public GetFileDialog()
        {
            InitializeComponent();
        }

        public string MyAccount
        {
            get { return this.comboBox_query_myAccount.Text; }
            set { this.comboBox_query_myAccount.Text = value; }
        }

        public string ShelfAccount
        {
            get { return this.comboBox_query_shelfAccount.Text; }
            set { this.comboBox_query_shelfAccount.Text = value; }
        }

        // 使用过的 ShelfAccount 值列表
        public List<string> UsedShelfAccounts
        {
            get
            {
                return new List<string> (this.comboBox_query_shelfAccount.Items.Cast<string>());
            }
            set
            {
                this.comboBox_query_shelfAccount.Items.Clear();
                if (value != null)
                {
                    foreach (var s in value)
                    {
                        this.comboBox_query_shelfAccount.Items.Add(s);
                    }
                }
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_query_myAccount),
                    new ComboBoxText(this.comboBox_query_shelfAccount),
                    this.textBox_remotePath,
                    this.textBox_localFileName,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    new ComboBoxText(this.comboBox_query_myAccount),
                    new ComboBoxText(this.comboBox_query_shelfAccount),
                    this.textBox_remotePath,
                    this.textBox_localFileName,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_getFileName_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的本地文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = true;
            dlg.FileName = this.textBox_localFileName.Text;
            // dlg.InitialDirectory = Environment.CurrentDirectory;
            dlg.Filter = "All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            this.textBox_localFileName.Text = dlg.FileName;
        }

        void EnableControls(bool enable)
        {
            this.comboBox_query_myAccount.Enabled = enable;
            this.comboBox_query_shelfAccount.Enabled = enable;
            this.textBox_remotePath.Enabled = enable;
            this.textBox_localFileName.Enabled = enable;
            this.button_getFileName.Enabled = enable;
            this.button_get.Enabled = enable;
            this.button_stop.Enabled = !enable;
        }

        CancellationTokenSource _cancelSearch = null;

        private async void button_get_Click(object sender, EventArgs e)
        {
            string strError = "";
            string local_filepath = "";

            _cancelSearch?.Dispose();
            _cancelSearch = new CancellationTokenSource();
            CancellationToken token = _cancelSearch.Token;

            bool compare_md5 = true;

            EnableControls(false);
            try
            {
                var get_result = await ConnectionPool.OpenConnectionAsync(this.MyAccount);
                if (get_result.Value == -1)
                {
                    strError = get_result.ErrorInfo;
                    goto ERROR1;
                }

                SetStatusText($"正在获取文件 {this.textBox_remotePath.Text} --> {this.textBox_localFileName.Text} ...");

                var connection = get_result.Connection;

                string remoteUserName = this.ShelfAccount;
                AddShelfAccountToUsedList(remoteUserName);

                string remote_path = this.textBox_remotePath.Text;
                GetResRequest request = new GetResRequest(Guid.NewGuid().ToString(),
                    null,   // loginInfo,
                    "getRes",
                    remote_path,
                    0,  // start,
                    -1, // length,
                    "data,timestamp"  // style
                    );

                // TODO: 可以先下载到一个临时文件，等下载完成后再改名为目标文件名
                byte[] local_md5 = null;
                using (FileStream stream = File.Open(this.textBox_localFileName.Text, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (stream.Length > 0)
                        stream.SetLength(0);

                    local_filepath = this.textBox_localFileName.Text;

                    var result = await connection.GetResAsyncLite(remoteUserName,
                    request,
                    stream,
                    (long totalLength, long current) =>
                    {
                        this.Invoke((Action)(() =>
                        {
                            if (this.toolStripProgressBar1.Maximum != (int)totalLength)
                            {
                                this.toolStripProgressBar1.Minimum = 0;
                                this.toolStripProgressBar1.Maximum = (int)totalLength;
                            }
                            this.toolStripProgressBar1.Value = (int)current;
                        }));
                    },
                    TimeSpan.FromSeconds(60),
                    token);
                    if (result.TotalLength == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }
                    if (result.TotalLength == 0)
                    {
                        strError = "没有命中";
                        goto ERROR1;
                    }

                    stream.Seek(0, SeekOrigin.Begin);
                    local_md5 = GetFileMd5(stream);
                }

                // 获得 MD5 和本地文件比对
                if (compare_md5)
                {
                    request.Style = "md5";
                    var result = await connection.GetResAsyncLite(remoteUserName,
request,
null,
(long totalLength, long current) =>
{
},
TimeSpan.FromSeconds(60),
token);
                    if (result.TotalLength == -1)
                    {
                        strError = result.ErrorInfo;
                        goto ERROR1;
                    }

                    var server_md5 = ByteArray.GetTimeStampByteArray(result.Timestamp);
                    if (ByteArray.Compare(server_md5, local_md5) != 0)
                    {
                        strError = $"服务器端文件 '{remote_path}' 和刚获取的本地文件 '{local_filepath}' MD5 不匹配。请重新获取";
                        goto ERROR1;
                    }
                }

                SetStatusText("文件获取完成");
                return;
            }
            catch (OperationCanceledException)
            {
                strError = "用户中断";
                goto ERROR1;
            }
            catch (AggregateException ex)
            {
                strError = MessageConnection.GetExceptionText(ex);
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ExceptionUtil.GetDebugText(ex);  // ex.Message;
                goto ERROR1;
            }
            finally
            {
                EnableControls(true);
            }

        ERROR1:
            if (string.IsNullOrEmpty(local_filepath) == false)
                File.Delete(local_filepath);
            SetStatusText(strError.Replace("\r\n", "; "));
            MessageBox.Show(this, strError);
        }

        public static byte[] GetFileMd5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(stream);
            }
        }

        void AddShelfAccountToUsedList(string text)
        {
            if (this.comboBox_query_shelfAccount.Items.Cast<string>().Contains(text))
                return;
            this.comboBox_query_shelfAccount.Items.Add(text);
        }

        void SetStatusText(string text)
        {
            this.Invoke((Action)(() =>
            {
                this.toolStripStatusLabel_message.Text = text;
            }));
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            _cancelSearch?.Cancel();
        }

        private void GetFileDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            {
                var list = StringUtil.MakePathList(this.UsedShelfAccounts, ",");

                ClientInfo.Config.Set("GetFileDialog",
        "usedShelfAccounts",
        list);
            }
        }

        private void GetFileDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancelSearch?.Cancel();
            _cancelSearch?.Dispose();


        }

        void FillMyAccountList()
        {
            if (this.comboBox_query_myAccount.Items.Count == 0)
            {
                var accounts = MessageAccountForm.GetAccounts();
                foreach (var account in accounts)
                {
                    this.comboBox_query_myAccount.Items.Add(account.UserName + "@" + account.ServerUrl);
                }
            }
        }

        private void comboBox_query_myAccount_DropDown(object sender, EventArgs e)
        {
            FillMyAccountList();
        }

        private void GetFileDialog_Load(object sender, EventArgs e)
        {
            var list = ClientInfo.Config.Get("GetFileDialog",
                "usedShelfAccounts",
                "");
            this.UsedShelfAccounts = StringUtil.SplitList(list);
        }
    }
}
