using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;

using dp2SSL.Models;
using static dp2SSL.LibraryChannelUtil;
using dp2SSL.Dialog;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Interfaces;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryServer;

namespace dp2SSL
{
    /// <summary>
    /// PageShelf.xaml 的交互逻辑
    /// </summary>
    public partial class PageShelf : Page, INotifyPropertyChanged
    {
        LayoutAdorner _adorner = null;
        AdornerLayer _layer = null;

        // EntityCollection _entities = new EntityCollection();
        Patron _patron = new Patron();

        public string Mode { get; set; }    // 运行模式。空/initial

        public PageShelf()
        {
            InitializeComponent();

            _patronErrorTable = new ErrorTable((e) =>
            {
                _patron.Error = e;
            });

            Loaded += PageShelf_Loaded;
            Unloaded += PageShelf_Unloaded;

            this.DataContext = this;

            // this.booksControl.SetSource(_entities);
            this.patronControl.DataContext = _patron;
            this.patronControl.InputFace += PatronControl_InputFace;

            this._patron.PropertyChanged += _patron_PropertyChanged;

            this.doorControl.OpenDoor += DoorControl_OpenDoor;

            App.CurrentApp.PropertyChanged += CurrentApp_PropertyChanged;



            // this.error.Text = "test";
        }

        // parameters:
        //      mode    空字符串或者“initial”
        public PageShelf(string mode) : this()
        {
            this.Mode = mode;
        }

        private async void PageShelf_Loaded(object sender, RoutedEventArgs e)
        {
            // _firstInitial = false;

            FingerprintManager.SetError += FingerprintManager_SetError;
            FingerprintManager.Touched += FingerprintManager_Touched;

            App.CurrentApp.TagChanged += CurrentApp_TagChanged;

            // RfidManager.ListLocks += RfidManager_ListLocks;
            ShelfData.OpenCountChanged += CurrentApp_OpenCountChanged;
            //ShelfData.BookChanged += ShelfData_BookChanged;

            RfidManager.ClearCache();
            // 注：将来也许可以通过(RFID 以外的)其他方式输入图书号码
            if (string.IsNullOrEmpty(RfidManager.Url))
                this.SetGlobalError("rfid", "尚未配置 RFID 中心 URL");

            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);

            {
                List<string> style = new List<string>();
                if (string.IsNullOrEmpty(App.RfidUrl) == false)
                    style.Add("rfid");
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    style.Add("fingerprint");
                if (string.IsNullOrEmpty(App.FaceUrl) == false)
                    style.Add("face");
                this.patronControl.SetStartMessage(StringUtil.MakePathList(style));
            }

            /*
            try
            {
                RfidManager.LockCommands = DoorControl.GetLockCommands();
            }
            catch (Exception ex)
            {
                this.SetGlobalError("cfg", $"获得门锁命令时出错:{ex.Message}");
            }
            */

            // 要在初始化以前设定好
            // RfidManager.AntennaList = GetAntennaList();

            // _patronReaderName = GetPatronReaderName();

            if (Mode == "initial" || ShelfData.FirstInitialized == false)
            {
                // TODO: 可否放到 App 的初始化阶段? 这样好处是菜单画面就可以看到有关数量显示了
                await InitialShelfEntities();

                // 迫使图书盘点暂停(如果门是全部关闭的话)
                // SetOpenCount(_openCount);

            }
        }

        static string GetPartialName(string buttonName)
        {
            if (buttonName == "count")
                return "全部图书";
            if (buttonName == "add")
                return "新放入";
            if (buttonName == "remove")
                return "新取出";
            if (buttonName == "errorCount")
                return "状态错误的图书";
            return buttonName;
        }

        private void ShowBookInfo(object sender, OpenDoorEventArgs e)
        {
            // 书柜外的读卡器触发观察图书信息对话框
            // if (e.Door.Type == "free" && e.Adds != null && e.Adds.Count > 0)
            {
                BookInfoWindow bookInfoWindow = null;

                EntityCollection collection = null;
                if (e.ButtonName == "count")
                    collection = e.Door.AllEntities;
                else if (e.ButtonName == "add")
                    collection = e.Door.AddEntities;
                else if (e.ButtonName == "remove")
                    collection = e.Door.RemoveEntities;
                else if (e.ButtonName == "errorCount")
                    collection = e.Door.ErrorEntities;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    bookInfoWindow = new BookInfoWindow();
                    bookInfoWindow.TitleText = $"{e.Door.Name} {GetPartialName(e.ButtonName)}";
                    bookInfoWindow.Owner = Application.Current.MainWindow;
                    bookInfoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    bookInfoWindow.Width = Math.Min(1000, this.ActualWidth);
                    bookInfoWindow.Height = Math.Min(700, this.ActualHeight);
                    bookInfoWindow.Closed += BookInfoWindow_Closed;
                    bookInfoWindow.SetBooks(collection);
                    bookInfoWindow.Show();
                    AddLayer();
                }));
            }
        }

        private void BookInfoWindow_Closed(object sender, EventArgs e)
        {
            RemoveLayer();
        }

        // 当前读者卡状态是否 OK?
        bool IsPatronOK(string action, out string message)
        {
            message = "";

            // 如果 UID 为空，而 Barcode 有内容，也是 OK 的。这是指纹的场景
            if (string.IsNullOrEmpty(_patron.UID) == true
                && string.IsNullOrEmpty(_patron.Barcode) == false)
                return true;

            // UID 和 Barcode 都不为空。这是 15693 和 14443 读者卡的场景
            if (string.IsNullOrEmpty(_patron.UID) == false
    && string.IsNullOrEmpty(_patron.Barcode) == false)
                return true;

            string debug_info = $"uid:[{_patron.UID}],barcode:[{_patron.Barcode}]";
            if (action == "open")
            {
                // 提示信息要考虑到应用了指纹的情况
                if (string.IsNullOrEmpty(App.FingerprintUrl) == false)
                    message = $"请先刷读者卡，或扫入一次指纹，然后再开门\r\n({debug_info})";
                else
                    message = $"请先刷读者卡，然后再开门\r\n({debug_info})";
            }
            else
            {
                // 调试用
                message = $"读卡器上的当前读者卡状态不正确。无法进行 {action} 操作\r\n({debug_info})";
            }
            return false;
        }

        void DisplayError(ref ProgressWindow progress,
    string message,
    string color = "red")
        {
            if (progress == null)
                return;
            MemoryDialog(progress);
            var temp = progress;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                temp.MessageText = message;
                temp.BackColor = color;
                temp = null;
            }));
            progress = null;
        }

        void DisplayMessage(ProgressWindow progress,
            string message,
            string color = "")
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progress.MessageText = message;
                if (string.IsNullOrEmpty(color) == false)
                    progress.BackColor = color;
            }));
        }

        List<Window> _dialogs = new List<Window>();

        void CloseDialogs()
        {
            // 确保 page 关闭时对话框能自动关闭
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var window in _dialogs)
                {
                    window.Close();
                }
            }));
        }

        void MemoryDialog(Window dialog)
        {
            _dialogs.Add(dialog);
        }

        void ErrorBox(string message,
            string color = "red",
            string style = "")
        {
            ProgressWindow progress = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.MessageText = "正在处理，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progress.Closed += Progress_Closed;
                if (StringUtil.IsInList("button_ok", style))
                    progress.okButton.Content = "确定";
                progress.Show();
                AddLayer();
            }));


            if (StringUtil.IsInList("auto_close", style))
            {
                DisplayMessage(progress, message, color);

                Task.Run(() =>
                {
                    // TODO: 显示倒计时计数？
                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                });
            }
            else
                DisplayError(ref progress, message, color);
        }

        private void DoorControl_OpenDoor(object sender, OpenDoorEventArgs e)
        {
            // 观察图书详情
            if (string.IsNullOrEmpty(e.ButtonName) == false)
            {
                ShowBookInfo(sender, e);
                return;
            }

            // 没有门锁的门
            if (string.IsNullOrEmpty(e.Door.LockName))
            {
                ErrorBox("没有门锁");
                return;
            }

            // 检查门锁是否已经是打开状态?
            if (e.Door.State == "open")
            {
                App.CurrentApp.Speak("已经打开");
                ErrorBox("已经打开", "yellow", "auto_close,button_ok");
                return;
            }

            // 以前积累的 _adds 和 _removes 要先处理，处理完再开门

            // 先检查当前是否具备读者身份？
            // 检查读者卡状态是否 OK
            if (IsPatronOK("open", out string check_message) == false)
            {
                if (string.IsNullOrEmpty(check_message))
                    check_message = $"(读卡器上的)当前读者卡状态不正确。无法进行开门操作";

                /*
                ProgressWindow progress = null;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.MessageText = "正在处理，请稍候 ...";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += Progress_Closed;
                    progress.Show();
                    AddLayer();
                }));

                DisplayError(ref progress, check_message);
                */
                ErrorBox(check_message);
                return;
            }

            // 检查读者记录状态
            XmlDocument readerdom = new XmlDocument();
            readerdom.LoadXml(_patron.Xml);
            // return:
            //      -1  检查过程出错
            //      0   状态不正常
            //      1   状态正常
            int nRet = LibraryServerUtil.CheckPatronState(readerdom,
                out string strError);
            if (nRet != 1)
            {
                ErrorBox(check_message);
                return;
            }

            // MessageBox.Show(e.Name);
            var result = RfidManager.OpenShelfLock(e.Door.LockName, e.Door.LockIndex);
            if (result.Value == -1)
                MessageBox.Show(result.ErrorInfo);
        }

        private void CurrentApp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Error")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public string Error
        {
            get
            {
                return App.CurrentApp.Error;
            }
        }


        private void CurrentApp_OpenCountChanged(object sender, OpenCountChangedEventArgs e)
        {
            // 如果从有门打开的状态变为全部门都关闭的状态，要尝试提交一次出纳请求
            if (e.OldCount > 0 && e.NewCount == 0)
            {
                SubmitCheckInOut();
                PatronClear(false);  // 确保在没有可提交内容的情况下也自动清除读者信息
            }
        }

#if NO
        int _openCount = 0; // 当前处于打开状态的门的个数

        private void RfidManager_ListLocks(object sender, ListLocksEventArgs e)
        {
            if (e.Result.Value == -1)
                return;

            // bool triggerAllClosed = false;
            {
                int count = 0;
                foreach (var state in e.Result.States)
                {
                    if (state.State == "open")
                        count++;
                    var result = DoorItem.SetLockState(_doors, state);
                    if (result.LockName != null && result.OldState != null && result.NewState != null)
                    {
                        if (result.NewState != result.OldState)
                        {
                            if (result.NewState == "open")
                                App.CurrentApp.Speak($"{result.LockName} 打开");
                            else
                                App.CurrentApp.Speak($"{result.LockName} 关闭");
                        }
                    }
                }

                //if (_openCount > 0 && count == 0)
                //    triggerAllClosed = true;

                SetOpenCount(count);
            }
        }

        // 设置打开门数量
        void SetOpenCount(int count)
        {
            int oldCount = _openCount;

            _openCount = count;

            // 打开门的数量发生变化
            if (oldCount != _openCount)
            {
                /*
                if (_openCount == 0)
                {
                    // 关闭图书读卡器(只使用读者证读卡器)
                    if (string.IsNullOrEmpty(_patronReaderName) == false
                        && RfidManager.ReaderNameList != _patronReaderName)
                    {
                        RfidManager.ReaderNameList = _patronReaderName;
                        RfidManager.ClearCache();
                    }
                }
                else
                {
                    // 打开图书读卡器(同时也使用读者证读卡器)
                    if (RfidManager.ReaderNameList != "*")
                    {
                        RfidManager.ReaderNameList = "*";
                        RfidManager.ClearCache();
                    }
                }*/
                if (oldCount > 0 && count == 0)
                {
                    // TODO: 如果从有门打开的状态变为全部门都关闭的状态，要尝试提交一次出纳请求
                    // if (triggerAllClosed)
                    {
                        SubmitCheckInOut();
                        PatronClear(false);  // 确保在没有可提交内容的情况下也自动清除读者信息
                    }
                }

            }
        }
#endif

        /*
        LockChanged SetLockState(LockState state)
        {
            return this.doorControl.SetLockState(state);
        }
        */

        private void PageShelf_Unloaded(object sender, RoutedEventArgs e)
        {
            // 提交尚未提交的取出和放入
            PatronClear(true);

            RfidManager.SetError -= RfidManager_SetError;

            App.CurrentApp.TagChanged -= CurrentApp_TagChanged;
            //ShelfData.BookChanged -= ShelfData_BookChanged;

            FingerprintManager.Touched -= FingerprintManager_Touched;
            FingerprintManager.SetError -= FingerprintManager_SetError;

            // RfidManager.ListLocks -= RfidManager_ListLocks;
            ShelfData.OpenCountChanged -= CurrentApp_OpenCountChanged;

            // 确保 page 关闭时对话框能自动关闭
            CloseDialogs();
            PatronClear(true);
        }

        // 从指纹阅读器获取消息(第一阶段)
        private async void FingerprintManager_Touched(object sender, TouchedEventArgs e)
        {
            SetPatronInfo(e.Result);

            await FillPatronDetail();

#if NO
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _patron.IsFingerprintSource = true;
                _patron.Barcode = "test1234";
            }));
#endif
        }

        // 从指纹阅读器获取消息(第一阶段)
        void SetPatronInfo(GetMessageResult result)
        {
            if (result.Value == -1)
            {
                SetPatronError("fingerprint", $"指纹中心出错: {result.ErrorInfo}, 错误码: {result.ErrorCode}");
                if (_patron.IsFingerprintSource)
                    PatronClear(true);    // 只有当面板上的读者信息来源是指纹仪时，才清除面板上的读者信息
                return;
            }
            else
            {
                // 清除以前残留的报错信息
                SetPatronError("fingerprint", "");
            }

            if (result.Message == null)
                return;

            PatronClear(true);
            _patron.IsFingerprintSource = true;
            _patron.PII = result.Message;
        }


        private void FingerprintManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetGlobalError("fingerprint", e.Error);
        }

        private void RfidManager_SetError(object sender, SetErrorEventArgs e)
        {
            SetGlobalError("rfid", e.Error);
            /*
            if (e.Error == null)
            {
                // 恢复正常
            }
            else
            {
                // 进入错误状态
                if (_rfidState != "error")
                {
                    await ClearBooksAndPatron(null);
                }

                _rfidState = "error";
            }
            */
        }

#if NO
        async Task<NormalResult> Update(
            BaseChannel<IRfid> channel_param,
            List<Entity> update_entities,
            CancellationToken token)
        {
            if (update_entities.Count > 0)
            {
                try
                {
                    BaseChannel<IRfid> channel = channel_param;
                    if (channel == null)
                        channel = RfidManager.GetChannel();
                    try
                    {
                        await FillBookFields(channel, update_entities, token);
                    }
                    finally
                    {
                        if (channel_param == null)
                            RfidManager.ReturnChannel(channel);
                    }
                }
                catch (Exception ex)
                {
                    string error = $"填充图书信息时出现异常: {ex.Message}";
                    SetGlobalError("rfid", error);
                    return new NormalResult { Value = -1, ErrorInfo = error };
                }

                // 自动检查 EAS 状态
                // CheckEAS(update_entities);
            }
            return new NormalResult();
        }

#endif

        // 设置全局区域错误字符串
        void SetGlobalError(string type, string error)
        {
            /*
            if (error != null && error.StartsWith("未"))
                throw new Exception("test");
                */
            App.CurrentApp?.SetError(type, error);
        }

        // 第二阶段：填充图书信息的 PII 和 Title 字段
        async Task FillBookFields(BaseChannel<IRfid> channel,
            List<Entity> entities,
            CancellationToken token)
        {
            try
            {
                foreach (Entity entity in entities)
                {
                    if (token.IsCancellationRequested)
                        return;

                    if (entity.FillFinished == true)
                        continue;

                    // 获得 PII
                    // 注：如果 PII 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        if (entity.TagInfo == null)
                            continue;

                        Debug.Assert(entity.TagInfo != null);

                        LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
(int)entity.TagInfo.BlockSize,
"" // tag.TagInfo.LockStatus
);
                        string pii = chip.FindElement(ElementOID.PII)?.Text;
                        entity.PII = PageBorrow.GetCaption(pii);
                    }

                    // 获得 Title
                    // 注：如果 Title 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.Title)
                        && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                    {
                        GetEntityDataResult result = await
                            Task<GetEntityDataResult>.Run(() =>
                            {
                                return GetEntityData(entity.PII);
                            });

                        if (result.Value == -1)
                        {
                            entity.SetError(result.ErrorInfo);
                            continue;
                        }
                        entity.Title = PageBorrow.GetCaption(result.Title);
                        entity.SetData(result.ItemRecPath, result.ItemXml);
                    }

                    entity.SetError(null);
                    entity.FillFinished = true;
                }

                booksControl.SetBorrowable();
            }
            catch (Exception ex)
            {
                SetGlobalError("current", $"FillBookFields exception: {ex.Message}");
            }
        }

        // 初始化时列出当前馆藏地应有的全部图书
        // 本函数中，只给 Entity 对象里面设置好了 PII，其他成员尚未设置
        static void FillLocationBooks(EntityCollection entities,
            string location,
            CancellationToken token)
        {
            var channel = App.CurrentApp.GetChannel();
            try
            {
                long lRet = channel.SearchItem(null,
                    "<全部>",
                    location,
                    5000,
                    "馆藏地点",
                    "exact",
                    "zh",
                    "shelfResultset",
                    "",
                    "",
                    out string strError);
                if (lRet == -1)
                    throw new ChannelException(channel.ErrorCode, strError);

                string strStyle = "id,cols,format:@coldef:*/barcode|*/borrower";

                ResultSetLoader loader = new ResultSetLoader(channel,
                    null,
                    "shelfResultset",
                    strStyle,
                    "zh");
                foreach (DigitalPlatform.LibraryClient.localhost.Record record in loader)
                {
                    token.ThrowIfCancellationRequested();
                    string pii = record.Cols[0];
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        entities.Add(pii);
                    }));
                }
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }

        private async void CurrentApp_TagChanged(object sender, TagChangedEventArgs e)
        {
            // 读者。不再精细的进行增删改跟踪操作，而是笼统地看 TagList.Patrons 集合即可
            var task = RefreshPatrons();

            await ShelfData.ChangeEntities((BaseChannel<IRfid>)sender, e);

            // "initial" 模式下，立即合并到 _all。等关门时候一并提交请求
            // TODO: 不过似乎此时有语音提示放入、取出，似乎更显得实用一些？
            if (this.Mode == "initial")
            {
                List<Entity> adds = new List<Entity>(ShelfData.Adds);
                foreach (var entity in adds)
                {
                    ShelfData.Add(ShelfData.All, entity);

                    ShelfData.Remove(ShelfData.Adds, entity);
                    ShelfData.Remove(ShelfData.Removes, entity);
                }

                List<Entity> removes = new List<Entity>(ShelfData.Removes);
                foreach (var entity in removes)
                {
                    ShelfData.Remove(ShelfData.All, entity);

                    ShelfData.Remove(ShelfData.Adds, entity);
                    ShelfData.Remove(ShelfData.Removes, entity);
                }

                ShelfData.RefreshCount();
            }
        }

        bool _initialCancelled = false;

        // 初始化开始前，要先把 RfidManager.ReaderNameList 设置为 "*"
        // 初始化完成前，先不要允许(开关门变化导致)修改 RfidManager.ReaderNameList
        async Task InitialShelfEntities()
        {
            if (ShelfData.FirstInitialized)
                return;

            this.doorControl.Visibility = Visibility.Collapsed;
            _initialCancelled = false;

            ProgressWindow progress = null;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.TitleText = "初始化智能书柜";
                progress.MessageText = "正在初始化图书信息，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progress.Closed += Progress_Cancelled;
                progress.Width = Math.Min(700, this.ActualWidth);
                progress.Height = Math.Min(500, this.ActualHeight);
                progress.okButton.Content = "取消";
                progress.Show();
                AddLayer();
            }));
            this.doorControl.Visibility = Visibility.Hidden;

            try
            {
                await ShelfData.InitialShelfEntities(
                    (s) =>
                    {
                        DisplayMessage(progress, s, "green");
                    },
                    () =>
                    {
                        return _initialCancelled;
                    });

                if (_initialCancelled)
                    return;

#if NO
                // TODO: 出现“正在初始化”的对话框。另外需要注意如果 DataReady 信号永远来不了怎么办
                await Task.Run(() =>
                {
                    TagList.DataReady = false;
                    // TODO: 是否一开始主动把 RfidManager ReaderNameList 设置为 "*"?
                    while (true)
                    {
                        if (TagList.DataReady == true)
                            return true;
                        Thread.Sleep(100);
                    }
                });

                _all.Clear();
                var books = TagList.Books;
                foreach (var tag in books)
                {
                    _all.Add(NewEntity(tag));
                }

                // DoorItem.DisplayCount(_all, _adds, _removes, App.CurrentApp.Doors);
                ShelfData.RefreshCount();
#endif
                // 把门显示出来。因为此时需要看到是否关门的状态
                this.doorControl.Visibility = Visibility.Visible;
                this.doorControl.InitializeButtons(ShelfData.ShelfCfgDom, ShelfData.Doors);

                // 检查门是否为关闭状态？
                // 注意 RfidManager 中门锁启动需要一定时间。状态可能是：尚未初始化/有门开着/门都关了
                await Task.Run(() =>
                {
                    while (ShelfData.OpeningDoorCount > 0)
                    {
                        if (_initialCancelled)
                            break;
                        DisplayMessage(progress, "请关闭全部柜门，以完成初始化", "yellow");
                        Thread.Sleep(1000);
                    }
                });

                if (_initialCancelled)
                    return;

                // 此时门是关闭状态。让读卡器切换到节省盘点状态
                ShelfData.RefreshReaderNameList();

                // TODO: 如何显示还书操作中的报错信息? 看了报错以后点继续?
                // result.Value
                //      -1  出错
                //      0   没有必要处理
                //      1   已经处理
                var result = TryReturn(progress, ShelfData.All);
                if (result.MessageDocument.ErrorCount > 0)
                {
                    string speak = "";
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            progress.BackColor = "yellow";
                            progress.MessageDocument = result.MessageDocument.BuildDocument("初始化", 18, out speak);
                            if (result.MessageDocument.ErrorCount > 0)
                                progress = null;
                        }));
                    }
                    if (string.IsNullOrEmpty(speak) == false)
                        App.CurrentApp.Speak(speak);
                }

                if (_initialCancelled)
                    return;

                /*
                if (_initialCancelled == false)
                {
                    this.doorControl.Visibility = Visibility.Visible;
                }
                */
            }
            finally
            {
                // _firstInitial = true;   // 第一次初始化已经完成
                RemoveLayer();

                if (_initialCancelled == false)
                {
                    // PageMenu.MenuPage.shelf.Visibility = Visibility.Visible;

                    if (progress != null)
                    {
                        progress.Closed -= Progress_Cancelled;
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (progress != null)
                                progress.Close();
                        }));
                    }

                    SetGlobalError("initial", null);
                    this.Mode = ""; // 从初始化模式转为普通模式
                }
                else
                {
                    ShelfData.FirstInitialized = false;

                    // PageMenu.MenuPage.shelf.Visibility = Visibility.Collapsed;

                    // TODO: 页面中央大字显示“书柜初始化失败”。重新进入页面时候应该自动重试初始化
                    SetGlobalError("initial", "智能书柜初始化失败。请检查读卡器和门锁参数配置，重新进行初始化 ...");
                    /*
                    ProgressWindow error = null;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        error = new ProgressWindow();
                        error.Owner = Application.Current.MainWindow;
                        error.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        error.Closed += Error_Closed;
                        error.Show();
                        AddLayer();
                    }));
                    DisplayError(ref error, "智能书柜初始化失败。请检查读卡器和门锁参数配置，重新进行初始化 ...");
                    */
                }
            }

            // TODO: 初始化中断后，是否允许切换到菜单和设置画面？(只是不让进入书架画面)
        }

        private void Error_Closed(object sender, EventArgs e)
        {
            RemoveLayer();
        }

        // 初始化被中途取消
        private void Progress_Cancelled(object sender, EventArgs e)
        {
            _initialCancelled = true;
        }

        // 刷新读者信息
        // TODO: 当读者信息更替时，要检查前一个读者是否有 _adds 和 _removes 队列需要提交，先提交，再刷成后一个读者信息
        async Task RefreshPatrons()
        {
            //_lock_refreshPatrons.EnterWriteLock();
            try
            {
                var patrons = TagList.Patrons;
                if (patrons.Count == 1)
                    _patron.IsRfidSource = true;

                if (_patron.IsFingerprintSource)
                {
                    // 指纹仪来源
                }
                else
                {
                    // RFID 来源
                    if (patrons.Count == 1)
                    {
                        if (_patron.Fill(patrons[0].OneTag) == false)
                            return;

                        SetPatronError("rfid_multi", "");   // 2019/5/22

                        // 2019/5/29
                        await FillPatronDetail();
                    }
                    else
                    {
                        // 拿走 RFID 读者卡时，不要清除读者信息。也就是说和指纹做法一样

                        // PatronClear(false); // 不需要 submit


                        SetPatronError("getreaderinfo", "");
                        if (patrons.Count > 1)
                        {
                            // 读卡器上放了多张读者卡
                            SetPatronError("rfid_multi", $"读卡器上放了多张读者卡({patrons.Count})。请拿走多余的");
                        }
                        else
                            SetPatronError("rfid_multi", "");   // 2019/5/20
                    }
                }
            }
            finally
            {
                //_lock_refreshPatrons.ExitWriteLock();
            }
        }

        // 填充读者信息的其他字段(第二阶段)
        async Task<NormalResult> FillPatronDetail(bool force = false)
        {
            // 已经填充过了
            if (_patron.PatronName != null
                && force == false)
                return new NormalResult();

            string pii = _patron.PII;
            if (string.IsNullOrEmpty(pii))
                pii = _patron.UID;

            if (string.IsNullOrEmpty(pii))
                return new NormalResult();

            // return.Value:
            //      -1  出错
            //      0   读者记录没有找到
            //      1   成功
            GetReaderInfoResult result = await
                Task<GetReaderInfoResult>.Run(() =>
                {
                    return GetReaderInfo(pii);
                });

            if (result.Value != 1)
            {
                string error = $"读者 '{pii}': {result.ErrorInfo}";
                SetPatronError("getreaderinfo", error);
                return new NormalResult { Value = -1, ErrorInfo = error };
            }

            SetPatronError("getreaderinfo", "");

            //if (string.IsNullOrEmpty(_patron.State) == true)
            //    OpenDoor();

            // TODO: 出现一个半透明(倒计时)提示对话框，提示可以开门了。如果书柜只有一个门，则直接打开这个门？

            if (force)
                _patron.PhotoPath = "";
            // string old_photopath = _patron.PhotoPath;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _patron.SetPatronXml(result.RecPath, result.ReaderXml, result.Timestamp);
                this.patronControl.SetBorrowed(result.ReaderXml);
            }));

            // 显示在借图书列表
            List<Entity> entities = new List<Entity>();
            foreach (Entity entity in this.patronControl.BorrowedEntities)
            {
                entities.Add(entity);
            }
            if (entities.Count > 0)
            {
                try
                {
                    BaseChannel<IRfid> channel = RfidManager.GetChannel();
                    try
                    {
                        await FillBookFields(channel, entities, new CancellationToken());
                    }
                    finally
                    {
                        RfidManager.ReturnChannel(channel);
                    }
                }
                catch (Exception ex)
                {
                    string error = $"填充读者信息时出现异常: {ex.Message}";
                    SetGlobalError("rfid", error);
                    return new NormalResult { Value = -1, ErrorInfo = error };
                }
            }
#if NO
            // 装载图象
            if (old_photopath != _patron.PhotoPath)
            {
                Task.Run(()=> {
                    LoadPhoto(_patron.PhotoPath);
                });
            }
#endif
            return new NormalResult();
        }

        public void Submit(bool silently = false)
        {
            if (ShelfData.Adds.Count > 0 || ShelfData.Removes.Count > 0)
                SubmitCheckInOut(false, silently);
        }
        // parameters:
        //      submitBefore    是否自动提交前面残留的 _adds 和 _removes ?
        public void PatronClear(bool submitBefore)
        {
            // 预先提交一次
            if (submitBefore)
            {
                if (ShelfData.Adds.Count > 0 || ShelfData.Removes.Count > 0)
                    SubmitCheckInOut(false);
            }

            _patron.Clear();

            if (this.patronControl.BorrowedEntities.Count == 0)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        this.patronControl.BorrowedEntities.Clear();
                    }));
                else
                    this.patronControl.BorrowedEntities.Clear();
            }
        }

        #region patron 分类报错机制

        // 错误类别 --> 错误字符串
        // 错误类别有：rfid fingerprint getreaderinfo
        ErrorTable _patronErrorTable = null;

        // 设置读者区域错误字符串
        void SetPatronError(string type, string error)
        {
            _patronErrorTable.SetError(type, error);
        }

        #endregion

        bool _visiblityChanged = false;

        private void _patron_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoPath")
            {
                Task.Run(() =>
                {
                    try
                    {
                        this.patronControl.LoadPhoto(_patron.PhotoPath);
                    }
                    catch (Exception ex)
                    {
                        SetGlobalError("patron", ex.Message);
                    }
                });
            }

            if (e.PropertyName == "UID"
                || e.PropertyName == "Barcode")
            {
                // 如果 patronControl 本来是隐藏状态，但读卡器上放上了读者卡，这时候要把 patronControl 恢复显示
                if ((string.IsNullOrEmpty(_patron.UID) == false || string.IsNullOrEmpty(_patron.Barcode) == false)
                    && this.patronControl.Visibility != Visibility.Visible)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Visible;
                        _visiblityChanged = true;
                    }));
                // 如果读者卡又被拿走了，则要恢复 patronControl 的隐藏状态
                else if (string.IsNullOrEmpty(_patron.UID) == true && string.IsNullOrEmpty(_patron.Barcode) == true
    && this.patronControl.Visibility == Visibility.Visible
    && _visiblityChanged)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Collapsed;
                    }));
            }
        }

        /*
        // 开门
        NormalResult OpenDoor()
        {
            // 打开对话框，询问门号
            OpenDoorWindow progress = null;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progress = new OpenDoorWindow();
                // progress.MessageText = "正在处理，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progress.Closed += Progress_Closed;
                progress.Show();
                AddLayer();
            }));

            try
            {
                progress = null;

                return new NormalResult();
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));
            }
        }
        */

        private void Progress_Closed(object sender, EventArgs e)
        {
            RemoveLayer();
        }

        void AddLayer()
        {
            _layer.Add(_adorner);
        }

        void RemoveLayer()
        {
            _layer.Remove(_adorner);
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            // 检查全部门是否关闭

            if (ShelfData.OpeningDoorCount > 0)
            {
                ErrorBox("请先关闭全部柜门，才能返回主菜单页面", "yellow", "button_ok");
                return;
            }

            /*
            await Task.Run(() =>
            {
                while (ShelfData.OpeningDoorCount > 0)
                {
                    if (_initialCancelled)
                        break;
                    DisplayMessage(progress, "请先关闭全部柜门，以返回菜单页面", "yellow");
                    Thread.Sleep(1000);
                }
            });
            */

            this.NavigationService.Navigate(PageMenu.MenuPage);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // OpenDoor();
        }

        // TODO: 报错信息尝试用 FlowDocument 改造
        // 尝试进行一次还书操作
        // result.Value
        //      -1  出错
        //      0   没有必要处理
        //      1   已经处理
        SubmitResult TryReturn(ProgressWindow progress,
            List<Entity> entities)
        {
            List<ActionInfo> actions = new List<ActionInfo>();
            foreach (var entity in entities)
            {
                actions.Add(new ActionInfo
                {
                    Entity = entity,
                    Action = "return"
                });
                actions.Add(new ActionInfo
                {
                    Entity = entity,
                    Action = "transfer",
                    CurrentShelfNo = ShelfData.GetShelfNo(entity),
                });
            }

            if (actions.Count == 0)
                return new SubmitResult();  // 没有必要处理

            return ShelfData.SubmitCheckInOut(
                (min, max, value) =>
                {
                    if (progress != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (min == -1 && max == -1 && value == -1)
                                progress.ProgressBar.Visibility = Visibility.Collapsed;
                            if (min != -1)
                                progress.ProgressBar.Minimum = min;
                            if (max != -1)
                                progress.ProgressBar.Maximum = max;
                            if (value != -1)
                                progress.ProgressBar.Value = value;
                        }));
                    }
                },
                "", // _patron.Barcode,
                "", // _patron.PatronName,
                actions,
                false);
        }

        // 关门，或者更换读者的时候，向服务器提交出纳请求
        // parameters:
        //      clearPatron 操作完成后是否自动清除右侧的读者信息
        void SubmitCheckInOut(bool clearPatron = true, bool silence = false)
        {
            List<ActionInfo> actions = new List<ActionInfo>();
            foreach (var entity in ShelfData.Adds)
            {
                if (ShelfData.BelongToNormal(entity) == false)
                    continue;
                actions.Add(new ActionInfo
                {
                    Entity = entity,
                    Action = "return"
                });
                actions.Add(new ActionInfo
                {
                    Entity = entity,
                    Action = "transfer",
                    CurrentShelfNo = ShelfData.GetShelfNo(entity),
                });
            }
            foreach (var entity in ShelfData.Removes)
            {
                if (ShelfData.BelongToNormal(entity) == false)
                    continue;
                actions.Add(new ActionInfo { Entity = entity, Action = "borrow" });
            }

            if (actions.Count == 0)
                return;  // 没有必要处理

            ProgressWindow progress = null;
            string patron_name = "";
            patron_name = _patron.PatronName;

            if (silence == false)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.MessageText = "正在处理，请稍候 ...";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += Progress_Closed;
                    progress.Width = Math.Min(700, this.ActualWidth);
                    progress.Height = Math.Min(500, this.ActualHeight);
                    progress.Show();
                    AddLayer();
                }));
            }

            bool patron_filled = false;

            // 检查读者卡状态是否 OK
            if (IsPatronOK("open", out string check_message) == false)
            {
                /*
                if (string.IsNullOrEmpty(check_message))
                    check_message = $"读卡器上的当前读者卡状态不正确。无法进行 checkin/out 操作";

                DisplayError(ref progress, check_message);
                return;
                */
            }
            else
                patron_filled = true;

            try
            {
                var result = ShelfData.SubmitCheckInOut(
                    (min, max, value) =>
                    {
                        if (progress != null)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (min == -1 && max == -1 && value == -1)
                                    progress.ProgressBar.Visibility = Visibility.Collapsed;
                                if (min != -1)
                                    progress.ProgressBar.Minimum = min;
                                if (max != -1)
                                    progress.ProgressBar.Maximum = max;
                                if (value != -1)
                                    progress.ProgressBar.Value = value;
                            }));
                        }
                    },
                    _patron.Barcode,
                    _patron.PatronName,
                    actions,
                    patron_filled);
                if (result.Value == -1)
                {
                    DisplayError(ref progress, result.ErrorInfo);
                    return;
                }

                string speak = "";
                if (progress != null && result.Value == 1)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.MessageDocument = result.MessageDocument.BuildDocument(patron_name, 18, out speak);
                        progress = null;
                    }));
                }

                if (string.IsNullOrEmpty(speak) == false)
                    App.CurrentApp.Speak(speak);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));
                if (clearPatron)
                    PatronClear(false);
            }
        }

        static string MakeList(List<string> list)
        {
            StringBuilder text = new StringBuilder();
            int i = 1;
            foreach (string s in list)
            {
                text.Append($"{i++}) {s}\r\n");
            }

            return text.ToString();
        }

        // 延时自动清除读者信息
        // 当在规定的时间内没有打开柜门，则自动清除读者信息。若打开了则不会清除
        async Task DelayClearPatron(CancellationToken token)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), token);
            PatronClear(true);
        }

        #region 人脸识别功能

        bool _stopVideo = false;

        private async void PatronControl_InputFace(object sender, EventArgs e)
        {
            RecognitionFaceResult result = null;

            VideoWindow videoRecognition = null;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                videoRecognition = new VideoWindow
                {
                    TitleText = "识别人脸 ...",
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                videoRecognition.Closed += VideoRecognition_Closed;
                videoRecognition.Show();
            }));
            _stopVideo = false;
            var task = Task.Run(() =>
            {
                DisplayVideo(videoRecognition);
            });
            try
            {
                result = await RecognitionFace("");
                if (result.Value == -1)
                {
                    if (result.ErrorCode != "cancelled")
                        SetGlobalError("face", result.ErrorInfo);
                    DisplayError(ref videoRecognition, result.ErrorInfo);
                    return;
                }

                SetGlobalError("face", null);
            }
            finally
            {
                if (videoRecognition != null)
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        videoRecognition.Close();
                    }));
            }

            GetMessageResult message = new GetMessageResult
            {
                Value = 1,
                Message = result.Patron,
            };
            SetPatronInfo(message);
            SetQuality("");
            await FillPatronDetail();
        }

        void DisplayError(ref VideoWindow videoRegister,
        string message,
        string color = "red")
        {
            MemoryDialog(videoRegister);
            var temp = videoRegister;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                temp.MessageText = message;
                temp.BackColor = color;
                temp.okButton.Content = "返回";
                temp = null;
            }));
            videoRegister = null;
        }


        void SetQuality(string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Quality.Text = text;
            }));
        }

        void DisplayVideo(VideoWindow window)
        {
            while (_stopVideo == false)
            {
                var result = FaceManager.GetImage("");
                if (result.ImageData == null)
                {
                    Thread.Sleep(500);
                    continue;
                }
                MemoryStream stream = new MemoryStream(result.ImageData);
                try
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        window.SetPhoto(stream);
                    }));
                    stream = null;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
        }

        private void VideoRecognition_Closed(object sender, EventArgs e)
        {
            FaceManager.CancelRecognitionFace();
            _stopVideo = true;
            RemoveLayer();
        }

        void EnableControls(bool enable)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //this.borrowButton.IsEnabled = enable;
                //this.returnButton.IsEnabled = enable;
                this.goHome.IsEnabled = enable;
                this.patronControl.inputFace.IsEnabled = enable;
            }));
        }

        async Task<RecognitionFaceResult> RecognitionFace(string style)
        {
            EnableControls(false);
            try
            {
                return await Task.Run<RecognitionFaceResult>(() =>
                {
                    // 2019/9/6 增加
                    var result = FaceManager.GetState("camera");
                    if (result.Value == -1)
                        return new RecognitionFaceResult
                        {
                            Value = -1,
                            ErrorInfo = result.ErrorInfo,
                            ErrorCode = result.ErrorCode
                        };
                    return FaceManager.RecognitionFace("");
                });
            }
            finally
            {
                EnableControls(true);
            }
        }

        #endregion
    }
}
