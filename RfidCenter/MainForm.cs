using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Web;
using System.IO;

using RfidDrivers.First;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.GUI;
using DigitalPlatform.CommonControl;
using DigitalPlatform.RFID.UI;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using System.Speech.Synthesis;

namespace RfidCenter
{
    public partial class MainForm : Form
    {
        Driver1 _driver = new Driver1();

        FloatingMessageForm _floatingMessage = null;

        public MainForm()
        {
            ClientInfo.ProgramName = "rfidcenter";
            ClientInfo.MainForm = this;
            Program.Rfid = _driver;

            InitializeComponent();

            this.tabControl_main.TabPages.Remove(this.tabPage_cfg);
            this.tabPage_cfg.Dispose();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.AutoHide = false;
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }

            UsbNotification.RegisterUsbDeviceNotification(this.Handle);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (DetectVirus.Detect360() || DetectVirus.DetectGuanjia())
            {
                MessageBox.Show(this, "rfidcenter 被木马软件干扰，无法启动");
                Application.Exit();
                return;
            }

            ClientInfo.Initial("rfidcenter");

            ClearHtml();

            // 显示版本号
            this.OutputHistory($"版本号: {ClientInfo.ClientVersion}");

            if (StartRemotingServer() == false)
                return;

            // "ipc://RfidChannel/RfidServer"
            // 通道打开成功后，窗口应该显示成一种特定的状态
            int nRet = StartChannel(
                "ipc://RfidChannel/RfidServer",
                out string strError);
            if (nRet == -1)
            {
                this.ShowMessage(strError, "red", true);
                return;
            }

            Task.Run(() =>
            {
                InitializeDriver();
            });

            if (StringUtil.IsDevelopMode() == false)
            {
                MenuItem_testing.Visible = false;
                this.toolStripButton_autoInventory.Visible = false;
            }

            // 后台自动检查更新
            Task.Run(() =>
            {
                NormalResult result = ClientInfo.InstallUpdateSync();
                if (result.Value == -1)
                    OutputHistory("自动更新出错: " + result.ErrorInfo, 2);
                else if (result.Value == 1)
                    OutputHistory(result.ErrorInfo, 1);
                else if (string.IsNullOrEmpty(result.ErrorInfo) == false)
                    OutputHistory(result.ErrorInfo, 0);
            });

            this.Speak("RFID 中心启动");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_rfidObj?.BeginCapture(false);

            UsbNotification.UnregisterUsbDeviceNotification();

            _cancelInventory?.Cancel();

            {
                if (this.checkBox_cfg_savePasswordLong.Checked == false)
                    this.textBox_cfg_password.Text = "";
                ClientInfo.Config.Set("global", "ui_state", this.UiState);
                ClientInfo.Config.Set("global", "replication_start", this.textBox_replicationStart.Text);
                ClientInfo.Finish();
            }

            EndChannel();
            EndRemotingServer();

            _driver.ReleaseDriver();
        }

        void UpdateDeviceList(List<Reader> readers)
        {
            if (readers == null)
                return;

            this.Invoke((Action)(() =>
            {
                this.comboBox_deviceList.Items.Clear();
                foreach (Reader reader in readers)
                {
                    this.comboBox_deviceList.Items.Add(reader.Name);
                }
            }));
        }

        void InitializeDriver()
        {
            try
            {
                InitializeDriverResult result = _driver.InitializeDriver("");
                // 列出所有可用设备名称
                UpdateDeviceList(result.Readers);

                if (result.Value == -1)
                    this.ShowMessage(result.ErrorInfo, "red", true);
                else
                {
                    // 一开始就启动捕捉状态
                    m_rfidObj?.BeginCapture(true);
                }

                this.Invoke((Action)(() =>
                {
                    this.UiState = ClientInfo.Config.Get("global", "ui_state", ""); // Properties.Settings.Default.ui_state;
                }));
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        public void ShowMessageBox(string strText)
        {
            if (this.IsHandleCreated)
                this.Invoke((Action)(() =>
                {
                    try
                    {
                        MessageBox.Show(this, strText);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }));
        }


        private void MenuItem_openReader_Click(object sender, EventArgs e)
        {
            //NormalResult result = _driver.OpenReader();
            //MessageBox.Show(this, result.ToString());
        }

        private void MenuItem_closeReader_Click(object sender, EventArgs e)
        {
            //NormalResult result = _driver.CloseReader();
            //MessageBox.Show(this, result.ToString());
        }

        string GetCurrentReaderName()
        {
            return (string)this.Invoke((Func<string>)(() =>
            {
                return this.comboBox_deviceList.Text;
            }));
        }

        InventoryInfo _inventory_info = null;

        private void MenuItem_inventory_Click(object sender, EventArgs e)
        {
            InventoryResult result = _driver.Inventory(GetCurrentReaderName(), "only_new");
            MessageBox.Show(this, result.ToString());
            if (result.Results != null && result.Results.Count > 0)
                _inventory_info = result.Results[0];
            else
                _inventory_info = null;
        }

        private void MenuItem_getTagInfo_Click(object sender, EventArgs e)
        {
            // byte[] uid = new byte[8];
            GetTagInfoResult result = _driver.GetTagInfo(GetCurrentReaderName(), _inventory_info);
            MessageBox.Show(this, result.ToString());
        }

        private void MenuItem_readBlocks_Click(object sender, EventArgs e)
        {

        }

        private void ToolStripMenuItem_testWriteContentToNewChip_Click(object sender, EventArgs e)
        {
#if NO
            // 准备好一个芯片内容
            byte[] data = Element.FromHexString(
    @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00"
);

            // 测试 BlockRange.GetBlockRanges()
            List<BlockRange> ranges = BlockRange.GetBlockRanges(
                data,
                "ll....lll",
                4);
            Debug.Assert(ranges[0].BlockCount == 2);
            Debug.Assert(ranges[0].Locked == true);
            Debug.Assert(ranges[0].Bytes.SequenceEqual(
                Element.FromHexString(
                @"91 00 05 1c
                be 99 1a 14"
                )
            ));

            Debug.Assert(ranges[1].BlockCount == 4);
            Debug.Assert(ranges[1].Locked == false);
            Debug.Assert(ranges[1].Bytes.SequenceEqual(
                Element.FromHexString(
                @"02 01 d0 14
                02 04 b3 46
                07 44 1c b6
                e2 e3 35 d6"
                )
            ));
            Debug.Assert(ranges[2].BlockCount == 3);
            Debug.Assert(ranges[2].Locked == true);
            Debug.Assert(ranges[2].Bytes.SequenceEqual(
                Element.FromHexString(
                @"83 02 07 ac
                c0 9e ba a0
                6f 6b 00 00"
                )
            ));
#endif
            GetTagInfoResult result = _driver.GetTagInfo(GetCurrentReaderName(), null);
            MessageBox.Show(this, "初始芯片内容: " + result.ToString());

            TagInfo new_chip = result.TagInfo.Clone();
            new_chip.Bytes = Element.FromHexString(
    @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00");
            new_chip.LockStatus = "ww....www";
            NormalResult write_result = _driver.WriteTagInfo(GetCurrentReaderName(), result.TagInfo, new_chip);
            MessageBox.Show(this, write_result.ToString());
        }

        private void ToolStripMenuItem_testLockBlocks_Click(object sender, EventArgs e)
        {
            GetTagInfoResult result = _driver.GetTagInfo(GetCurrentReaderName(), null);
            MessageBox.Show(this, "初始芯片内容: " + result.ToString());
            if (result.Value == -1)
                return;

            TagInfo new_chip = result.TagInfo.Clone();
            new_chip.Bytes = Element.FromHexString(
    @"91 00 05 1c
be 99 1a 14
02 01 d0 14
02 04 b3 46
07 44 1c b6
e2 e3 35 d6
83 02 07 ac
c0 9e ba a0
6f 6b 00 00");
            new_chip.LockStatus = "ll....www";
            NormalResult write_result = _driver.WriteTagInfo(GetCurrentReaderName(), result.TagInfo, new_chip);
            MessageBox.Show(this, write_result.ToString());
        }

        private void toolStripButton_autoInventory_CheckStateChanged(object sender, EventArgs e)
        {
            StartInventory(toolStripButton_autoInventory.Checked);
        }

        // 启动或者停止自动盘点
        void StartInventory(bool start)
        {
            string reader_name = GetCurrentReaderName();
            if (string.IsNullOrEmpty(reader_name))
            {
                MessageBox.Show(this, "尚未选定当前读卡器");
                this.tabControl_main.SelectedTab = this.tabPage_cfg;
                this.comboBox_deviceList.Focus();
                return;
            }

            if (start)
                Task.Run(() => { DoInventory(); });
            else
                _cancelInventory.Cancel();
        }

        CancellationTokenSource _cancelInventory = new CancellationTokenSource();

        void DoInventory()
        {
            _cancelInventory = new CancellationTokenSource();
            bool bFirst = true;
            try
            {
                while (_cancelInventory.IsCancellationRequested == false)
                {
                    Task.Delay(500, _cancelInventory.Token).Wait();

                    InventoryResult result = null;

                    result = _driver.Inventory(GetCurrentReaderName(), bFirst ? "" : "only_new");
                    bFirst = false;
                    if (result.Value == -1)
                    {
                        // 显示报错信息
                        this.ShowMessage($"{result.ErrorInfo},error_code={result.ErrorCode}", "red", true);
                        // 让按钮释放
                        this.Invoke((Action)(() =>
                        {
                            this.toolStripButton_autoInventory.Checked = false;
                        }));
                        break;
                    }

                    this.Invoke((Action)(() =>
                    {
                        FillIventoryInfo(result.Results);
                    }));
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {

            }
        }

        ListViewItem FindItem(string uid)
        {
            foreach (ListViewItem item in this.listView_chips.Items)
            {
                ItemInfo tag_info = (ItemInfo)item.Tag;
                if (tag_info.OldInfo.UID == uid)
                    return item;
            }

            return null;
        }

        void FillIventoryInfo(List<InventoryInfo> infos)
        {
            foreach (InventoryInfo info in infos)
            {
                string uid = info.UID;
                ListViewItem item = FindItem(uid);
                if (item == null)
                {
                    item = new ListViewItem(uid);
                    // ListViewUtil.ChangeItemText(item, 1, pii);
                    this.listView_chips.Items.Add(item);

                    Task.Run(() => { LoadChipData(uid, item); });
                }
            }
        }

        static void SetItemColor(ListViewItem item, string state)
        {
            if (state == "normal")
            {
                item.BackColor = SystemColors.Control;
                item.ForeColor = SystemColors.ControlText;
                return;
            }

            if (state == "changed")
            {
                item.BackColor = SystemColors.Info;
                item.ForeColor = SystemColors.InfoText;
                return;
            }

            if (state == "error")
            {
                item.BackColor = Color.DarkRed;
                item.ForeColor = Color.White;
                return;
            }
        }

        // 装载芯片数据
        bool LoadChipData(string uid, ListViewItem item)
        {
            InventoryInfo info = new InventoryInfo
            {
                UID = uid,
            };
            GetTagInfoResult result = _driver.GetTagInfo(GetCurrentReaderName(), info);
            if (result.Value == -1)
            {
                this.Invoke((Action)(() =>
                {
                    SetItemColor(item, "error");
                    ListViewUtil.ChangeItemText(item, 1, $"error:{result.ErrorInfo},error_code:{result.ErrorCode}");
                }));
                return false;
            }

            // 刷新 item 行
            ItemInfo tag_info = new ItemInfo
            {
                OldInfo = result.TagInfo,
                LogicChip = LogicChipItem.FromTagInfo(result.TagInfo)
            };
            item.Tag = tag_info;

            tag_info.LogicChip.DSFID = tag_info.OldInfo.DSFID;
            tag_info.LogicChip.AFI = tag_info.OldInfo.AFI;
            tag_info.LogicChip.EAS = tag_info.OldInfo.EAS;

            tag_info.LogicChip.PropertyChanged += LogicChip_PropertyChanged;

            string pii = tag_info.LogicChip.FindElement(ElementOID.PII)?.Text;
            this.Invoke((Action)(() =>
            {
                ListViewUtil.ChangeItemText(item, 1, pii);
                ListViewUtil.ChangeItemText(item, 0, uid);
                SetItemColor(item, "normal");
            }));

            // 如果当前打开的 ChipDialog 正好是这个 UID，则更新它
            this.Invoke((Action)(() =>
            {
                SetChipDialogContent(item, uid);
            }));
            return true;
        }

        private void LogicChip_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateChanged(sender as LogicChipItem);
        }

        void UpdateChanged(LogicChipItem chip)
        {
            this.Invoke((Action)(() =>
            {
                foreach (ListViewItem item in this.listView_chips.Items)
                {
                    ItemInfo tag_info = (ItemInfo)item.Tag;
                    if (tag_info.LogicChip == chip)
                    {
                        // 更新 column 0
                        string uid = ListViewUtil.GetItemText(item, 0);
                        if (uid.StartsWith("*"))
                            uid = uid.Substring(1);
                        if (tag_info.LogicChip.Changed)
                            uid = "*" + uid;
                        ListViewUtil.ChangeItemText(item, 0, uid);

                        if (tag_info.LogicChip.Changed)
                            item.BackColor = SystemColors.Info;
                        else
                            item.BackColor = SystemColors.Control;

                        // 更新 column 1 
                        string pii = tag_info.LogicChip.FindElement(ElementOID.PII)?.Text;
                        ListViewUtil.ChangeItemText(item, 1, pii);
                        return;
                    }
                }
            }));
        }

        ItemInfo FindItemInfo(LogicChipItem chip, out ListViewItem output_item)
        {
            ListViewItem temp = null;
            var result = (ItemInfo)this.Invoke((Func<ItemInfo>)(() =>
            {
                foreach (ListViewItem item in this.listView_chips.Items)
                {
                    ItemInfo tag_info = (ItemInfo)item.Tag;
                    if (tag_info.LogicChip == chip)
                    {
                        temp = item;
                        return tag_info;
                    }
                }
                return null;
            }));
            output_item = temp;
            return result;
        }


        class ItemInfo
        {
            public TagInfo OldInfo { get; set; }
            public LogicChipItem LogicChip { get; set; }
            public bool Changed { get; set; }
        }

        public void ShowMessage(string strMessage,
string strColor = "",
bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        public void ClearMessage()
        {
            this.ShowMessage("");
        }

        ChipDialog _chipDialog = null;

        private void listView_chips_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem item = this.listView_chips.FocusedItem;
            if (item == null && this.listView_chips.SelectedItems.Count > 0)
                item = this.listView_chips.SelectedItems[0];

            {
                if (_chipDialog == null)
                {
                    _chipDialog = new ChipDialog();
                    _chipDialog.FormClosed += _chipDialog_FormClosed;
                    _chipDialog.SaveTriggerd += _chipDialog_SaveTriggerd;
                    _chipDialog.RefreshTriggerd += _chipDialog_RefreshTriggerd;
                }

                if (_chipDialog.Visible == false)
                    _chipDialog.Show(this);
            }

            SetChipDialogContent(item, null);
        }

        private void _chipDialog_RefreshTriggerd(object sender, EventArgs e)
        {
            ChipDialog dialog = (ChipDialog)sender;
            Reload(dialog, true);

#if NO
            LogicChipItem chip = dialog.LogicChipItem;
            ItemInfo item_info = FindItemInfo(chip, out ListViewItem item);
            if (item_info == null)
            {
                this.ShowMessage("cant find item_info", "red", true);
                _chipDialog.ShowMessage("cant find item_info", "red", true);
                return;
            }

            string uid = Element.GetHexString(item_info.OldInfo.uid);
            if (LoadChipData(uid, item) == true)
            {
                this.ShowMessage("重新装载成功", "green", true);
                _chipDialog.ShowMessage("重新装载成功", "green", true);
            }
            else
            {
                this.ShowMessage("重新装载失败", "red", true);
                _chipDialog.ShowMessage("重新装载失败", "red", true);
            }
#endif
        }

        void Reload(ChipDialog dialog, bool show_message)
        {
            string strError = "";

            string reader_name = GetCurrentReaderName();
            if (string.IsNullOrEmpty(reader_name))
            {
                strError = "尚未选定当前读卡器";
                goto ERROR1;
            }

            LogicChipItem chip = dialog.LogicChipItem;
            ItemInfo item_info = FindItemInfo(chip, out ListViewItem item);
            if (item_info == null)
            {
                if (show_message)
                {
                    strError = "cant find item_info";
                    goto ERROR1;
                }
                return;
            }

            string uid = item_info.OldInfo.UID;
            if (LoadChipData(uid, item) == true)
            {
                if (show_message)
                {
                    this.ShowMessage("重新装载成功", "green", true);
                    _chipDialog.ShowMessage("重新装载成功", "green", true);
                }
            }
            else
            {
                if (show_message)
                {
                    strError = "重新装载失败";
                    goto ERROR1;
                }
            }
            return;
            ERROR1:
            this.ShowMessage(strError, "red", true);
            _chipDialog.ShowMessage(strError, "red", true);
        }


        // TODO: 如何显示保存成功？注意 ChipDialog 可能会遮挡后面显示的文字
        private void _chipDialog_SaveTriggerd(object sender, EventArgs e)
        {
            string strError = "";

            string reader_name = GetCurrentReaderName();
            if (string.IsNullOrEmpty(reader_name))
            {
                strError = "尚未选定当前读卡器";
                goto ERROR1;
            }

            ChipDialog dialog = (ChipDialog)sender;
            LogicChipItem chip = dialog.LogicChipItem;
            ItemInfo item_info = FindItemInfo(chip, out ListViewItem item);
            if (item_info == null)
            {
                strError = "cant find item_info";
                goto ERROR1;
            }
            TagInfo new_tag_info = BuildNewTagInfo(
                item_info.OldInfo,
                chip);

            NormalResult result = _driver.WriteTagInfo(GetCurrentReaderName(), item_info.OldInfo, new_tag_info);
            if (result.Value == 0)
            {
                this.ShowMessage("保存成功", "green", true);
                _chipDialog.ShowMessage("保存成功", "green", true);

                Reload(dialog, false);
            }
            else
            {
                strError = $"保存失败:{result.ErrorInfo}";
                goto ERROR1;
            }

            return;
            ERROR1:
            this.ShowMessage(strError, "red", true);
            _chipDialog.ShowMessage(strError, "red", true);
        }

        static TagInfo BuildNewTagInfo(TagInfo old_tag_info,
            LogicChipItem chip)
        {
            TagInfo new_tag_info = old_tag_info.Clone();
            new_tag_info.Bytes = chip.GetBytes(
                (int)new_tag_info.MaxBlockCount * (int)new_tag_info.BlockSize,
                (int)new_tag_info.BlockSize,
                LogicChip.GetBytesStyle.None,
                out string block_map);
            new_tag_info.LockStatus = block_map;

            new_tag_info.DSFID = chip.DSFID;
            new_tag_info.AFI = chip.AFI;
            new_tag_info.EAS = chip.EAS;
            return new_tag_info;
        }

        private void _chipDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            _chipDialog = null;
        }

        // parameters:
        //      uid 如果为空，则表示无条件刷新当前 ChidDialog 内容。如果为其他，则表示当 ChipDialog 的 UID 和 uid 匹配时才进行刷新
        void SetChipDialogContent(ListViewItem item, string uid)
        {
            if (_chipDialog == null)
                return;

            if (string.IsNullOrEmpty(uid) == false)
            {
                if (item != null)
                {
                    ItemInfo info = (ItemInfo)item.Tag;
                    if (_chipDialog != null)
                    {
                        _chipDialog.UID = info.OldInfo.UID;
                        _chipDialog.LogicChipItem = info.LogicChip;
                    }
                }
                else
                {
                    if (_chipDialog != null)
                    {
                        _chipDialog.UID = "";
                        _chipDialog.LogicChipItem = null;
                    }
                }
                return;
            }

            // if (_chipDialog.UID == uid)
            {
                if (item != null)
                {
                    ItemInfo info = (ItemInfo)item.Tag;
                    if (_chipDialog != null)
                    {
                        _chipDialog.UID = info.OldInfo.UID;
                        _chipDialog.LogicChipItem = info.LogicChip;
                    }
                }
                else
                {
                    if (_chipDialog != null)
                    {
                        _chipDialog.UID = "";
                        _chipDialog.LogicChipItem = null;
                    }
                }
            }
        }

        private void listView_chips_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem item = null;
            if (this.listView_chips.SelectedItems.Count > 0)
                item = this.listView_chips.SelectedItems[0];

            SetChipDialogContent(item, null);
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.textBox_cfg_dp2LibraryServerUrl,
                    this.textBox_cfg_userName,
                    this.textBox_cfg_password,
                    this.textBox_cfg_location,
                    new ControlWrapper(this.checkBox_speak, true),
                    new ControlWrapper(this.checkBox_beep, true),
                    new ControlWrapper(this.checkBox_cfg_savePasswordLong, true),
                    this.comboBox_deviceList,
                    this.textBox_cfg_shreshold
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.textBox_cfg_dp2LibraryServerUrl,
                    this.textBox_cfg_userName,
                    this.textBox_cfg_password,
                    this.textBox_cfg_location,
                    new ControlWrapper(this.checkBox_speak, true),
                    new ControlWrapper(this.checkBox_beep, true),
                    new ControlWrapper(this.checkBox_cfg_savePasswordLong, true),
                    this.comboBox_deviceList,
                    this.textBox_cfg_shreshold
                };
                GuiState.SetUiState(controls, value);
            }
        }

        #region remoting server

#if HTTP_CHANNEL
        HttpChannel m_serverChannel = null;
#else
        IpcServerChannel m_serverChannel = null;
#endif

        bool StartRemotingServer()
        {
            try
            {
#if HTTP_CHANNEL
            m_serverChannel = new HttpChannel();
#else
                // TODO: 重复启动 .exe 这里会抛出异常，要进行警告处理
                m_serverChannel = new IpcServerChannel(
                    "RfidChannel");
#endif

                //Register the server channel.
                ChannelServices.RegisterChannel(m_serverChannel, false);

                RemotingConfiguration.ApplicationName = "RfidServer";

                //Register this service type.
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(RfidServer),
                    "RfidServer",
                    WellKnownObjectMode.Singleton);
                return true;
            }
            catch (RemotingException ex)
            {
                this.ShowMessage(ex.Message);
                return false;
            }
        }

        void EndRemotingServer()
        {
            if (m_serverChannel != null)
            {
                ChannelServices.UnregisterChannel(m_serverChannel);
                m_serverChannel = null;
            }
        }

        #endregion

        #region ipc channel

        IpcClientChannel m_rfidChannel = new IpcClientChannel();
        IRfid m_rfidObj = null;

        // "ipc://RfidChannel/RfidServer"
        // 通道打开成功后，窗口应该显示成一种特定的状态
        int StartChannel(
            string strUrl,
            out string strError)
        {
            strError = "";

            //Register the channel with ChannelServices.
            ChannelServices.RegisterChannel(m_rfidChannel, false);

            try
            {
                m_rfidObj = (IRfid)Activator.GetObject(typeof(IRfid),
                    strUrl);
                if (m_rfidObj == null)
                {
                    strError = "could not locate Rfid Server";
                    return -1;
                }
            }
            finally
            {

            }

            return 0;
        }

        void EndChannel()
        {
            if (this.m_rfidObj != null)
            {
                ChannelServices.UnregisterChannel(m_rfidChannel);
                this.m_rfidObj = null;
            }
        }

        #endregion

        private void ToolStripMenuItem_testRfidChannel_Click(object sender, EventArgs e)
        {
            ListReadersResult result = m_rfidObj.ListReaders();
            MessageBox.Show(this, result.ToString());
        }

        #region 浏览器控件

        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(ClientInfo.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";
            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    this.webBrowser1.Navigate("about:blank");
                    doc = this.webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);

        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);
            // Global.ScrollToEnd(this.WebBrowser);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        public static void WriteHtml(WebBrowser webBrowser,
string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");

                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
            REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        public static void SetHtmlString(WebBrowser webBrowser,
    string strHtml,
    string strDataDir,
    string strTempFileType)
        {
            // StopWebBrowser(webBrowser);

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", PathUtil.MergePath(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".html");
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            Navigate(webBrowser, strTempFilename);  // 2015/7/28
        }

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            AppendHtml(strHtml);
        }

        public void OutputHistory(string strText, int nWarningLevel = 0)
        {
            OutputText(DateTime.Now.ToShortTimeString() + " " + strText, nWarningLevel);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

        #endregion

        private void MenuItem_openSendKey_Click(object sender, EventArgs e)
        {
            m_rfidObj.EnableSendKey(true);
        }

        private void MenuItem_closeSendKey_Click(object sender, EventArgs e)
        {
            m_rfidObj.EnableSendKey(false);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == UsbNotification.WmDevicechange)
            {
                switch ((int)m.WParam)
                {
                    case UsbNotification.DbtDeviceremovecomplete:
                        //MessageBox.Show(this, "removed"); 
                        BeginRefreshReaders();
                        break;
                    case UsbNotification.DbtDevicearrival:
                        //MessageBox.Show(this, "added");
                        BeginRefreshReaders();
                        break;
                }
            }
        }

        System.Threading.Timer _refreshTimer = null;
        private static readonly Object _syncRoot_refresh = new Object(); // 2017/5/18

#if NO
        void BeginRefreshReaders()
        {
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                lock (_syncRoot_refresh)
                {
                    _driver.RefreshAllReaders();
                    m_rfidObj?.BeginCapture(false);
                    m_rfidObj?.BeginCapture(true);
                    UpdateDeviceList(_driver.Readers);
                }
            });
        }
#endif

        // 2 秒内多次到来的请求，会被合并为一次执行
        void BeginRefreshReaders()
        {
            lock (_syncRoot_refresh)
            {
                if (_refreshTimer == null)
                {
                    _refreshTimer = new System.Threading.Timer(
            new System.Threading.TimerCallback(refreshTimerCallback),
            null,
            TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
                }
            }
        }

        void refreshTimerCallback(object o)
        {
            // 迫使重新启动
            {
                _driver.RefreshAllReaders();
                m_rfidObj?.BeginCapture(false);
                m_rfidObj?.BeginCapture(true);
                UpdateDeviceList(_driver.Readers);
            }

            lock (_syncRoot_refresh)
            {
                if (_refreshTimer != null)
                {
                    _refreshTimer.Dispose();
                    _refreshTimer = null;
                }
            }
        }

        private void ToolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MenuItem_loadFactoryDefault_Click(object sender, EventArgs e)
        {
            {
                DialogResult result = MessageBox.Show(this,
    "确实要将全部读卡器恢复为厂家出厂状态?",
    "MainForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
            }

            {
                NormalResult result = _driver.LoadFactoryDefault("*");
                if (result.Value == -1)
                    MessageBox.Show(this, result.ErrorInfo);
                else
                    MessageBox.Show(this, "OK");
            }
        }

        private void MenuItem_testSetConfig_Click(object sender, EventArgs e)
        {
            NormalResult result = _driver.SetConfig("*", "beep:-");
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);
            else
                MessageBox.Show(this, "OK");
        }

        // 将读卡器恢复为出厂状态，和数字平台预设的状态(不鸣叫，被动模式)
        private void MenuItem_resetReaderToDigitalPlatformState_Click(object sender, EventArgs e)
        {
            string strError = "";
            {
                DialogResult result = MessageBox.Show(this,
    "确实要将全部读卡器恢复为初始状态?",
    "MainForm",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;
            }

            {
                NormalResult result = _driver.LoadFactoryDefault("*");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }

            {
                NormalResult result = _driver.SetConfig("*", "beep:-,mode:host");
                if (result.Value == -1)
                {
                    strError = result.ErrorInfo;
                    goto ERROR1;
                }
            }

            MessageBox.Show(this, "OK");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        private void MenuItem_readConfig_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strCfgNo = InputDlg.GetInput(this, "", "cfg_no", "0");
            if (strCfgNo == null)
                return;
            if (uint.TryParse(strCfgNo, out uint cfg_no) == false)
            {
                strError = $"cfg_no '{strCfgNo}' 不合法";
                goto ERROR1;
            }
            ReadConfigResult result = _driver.ReadConfig("*", cfg_no);
            MessageDlg.Show(this, $"cfg_no:{result.CfgNo}\r\nbytes:\r\n{Element.GetHexString(result.Bytes, "4")}", "config info");
            return;
            ERROR1:
            MessageBox.Show(this, strError);
        }

        SpeechSynthesizer m_speech = new SpeechSynthesizer();
        string m_strSpeakContent = "";

        public void Speak(string strText, bool bError = false)
        {
#if NO
            string color = "gray";
            if (bError)
                color = "darkred";

            DisplayText(strText, "white", color);
#endif

            if (this.m_speech == null)
                return;

            if (this.SpeakOn == false)
                return;

            this.m_strSpeakContent = strText;
            this.Invoke((Action)(() =>
            {
                this.m_speech.SpeakAsyncCancelAll();
                this.m_speech.SpeakAsync(strText);
            }));
        }

        public bool SpeakOn
        {
            get
            {
                return true;    // for testing
#if NO
                return (bool)this.Invoke(new Func<bool>(() =>
                {
                    return this.checkBox_speak.Checked;
                }));
#endif
            }
        }
    }
}
