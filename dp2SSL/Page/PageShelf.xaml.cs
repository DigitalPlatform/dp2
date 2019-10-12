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

using dp2SSL.Models;
using static dp2SSL.LibraryChannelUtil;

using DigitalPlatform;
using DigitalPlatform.Core;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Interfaces;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;

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

        List<Entity> _all = new List<Entity>();
        List<Entity> _adds = new List<Entity>();
        List<Entity> _removes = new List<Entity>();

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

            this._patron.PropertyChanged += _patron_PropertyChanged;

            this.doorControl.OpenDoor += DoorControl_OpenDoor;

            App.CurrentApp.PropertyChanged += CurrentApp_PropertyChanged;



            // this.error.Text = "test";
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

        private void DoorControl_OpenDoor(object sender, OpenDoorEventArgs e)
        {
            // TODO: 以前积累的 _adds 和 _removes 要先处理，处理完再开门


            // 先检查当前是否具备读者身份？
            // 检查读者卡状态是否 OK
            if (IsPatronOK("open", out string check_message) == false)
            {
                if (string.IsNullOrEmpty(check_message))
                    check_message = $"(读卡器上的)当前读者卡状态不正确。无法进行开门操作";

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
                /*
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.MessageText = ;
                    progress.BackColor = "red";
                    progress = null;
                }));
                */
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

        private void PageShelf_Loaded(object sender, RoutedEventArgs e)
        {
            FingerprintManager.SetError += FingerprintManager_SetError;
            FingerprintManager.Touched += FingerprintManager_Touched;

            App.CurrentApp.TagChanged += CurrentApp_TagChanged;

            RfidManager.ListLocks += RfidManager_ListLocks;

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

            RfidManager.LockCommands = DoorControl.GetLockCommands();

            InitialEntities();
        }

        int _openCount = 0; // 当前处于打开状态的门的个数

        private void RfidManager_ListLocks(object sender, ListLocksEventArgs e)
        {
            if (e.Result.Value == -1)
                return;

            bool triggerAllClosed = false;
            {
                int count = 0;
                foreach (var state in e.Result.States)
                {
                    if (state.State == "open")
                        count++;
                    SetLockState(state);
                }

                if (_openCount > 0 && count == 0)
                    triggerAllClosed = true;

                _openCount = count;
            }

            // TODO: 如果从有门打开的状态变为全部门都关闭的状态，要尝试提交一次出纳请求
            if (triggerAllClosed)
                SubmitCheckInOut();
        }

        void SetLockState(LockState state)
        {
            this.doorControl.SetLockState(state);
        }

        private void PageShelf_Unloaded(object sender, RoutedEventArgs e)
        {
            RfidManager.SetError -= RfidManager_SetError;

            App.CurrentApp.TagChanged -= CurrentApp_TagChanged;

            FingerprintManager.Touched -= FingerprintManager_Touched;
            FingerprintManager.SetError -= FingerprintManager_SetError;

            RfidManager.ListLocks -= RfidManager_ListLocks;

            // 确保 page 关闭时对话框能自动关闭
            CloseDialogs();
            PatronClear();
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
                    PatronClear();    // 只有当面板上的读者信息来源是指纹仪时，才清除面板上的读者信息
                return;
            }
            else
            {
                // 清除以前残留的报错信息
                SetPatronError("fingerprint", "");
            }

            if (result.Message == null)
                return;

            PatronClear();
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

        // 设置全局区域错误字符串
        void SetGlobalError(string type, string error)
        {
            if (error != null && error.StartsWith("未"))
                throw new Exception("test");
            App.CurrentApp.SetError(type, error);
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
            await ChangeEntities((BaseChannel<IRfid>)sender, e);
        }

        static Entity NewEntity(TagAndData tag)
        {
            var result = new Entity
            {
                UID = tag.OneTag.UID,
                ReaderName = tag.OneTag.ReaderName,
                Antenna = tag.OneTag.AntennaID.ToString(),
                TagInfo = tag.OneTag.TagInfo,
            };

            EntityCollection.SetPII(result);
            return result;
        }

        List<Entity> Find(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            return results;
        }

        bool Add(List<Entity> entities, Entity entity)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == entity.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
                return false;
            entities.Add(entity);
            return true;
        }

        bool Remove(List<Entity> entities, Entity entity)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == entity.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
            {
                foreach(var o in results)
                {
                    entities.Remove(o);
                }
                return true;
            }
            return false;
        }

        bool Add(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            if (results.Count == 0)
            {
                entities.Add(NewEntity(tag));
                return true;
            }
            return false;
        }

        bool Remove(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
            {
                foreach (var o in results)
                {
                    entities.Remove(o);
                }
                return true;
            }
            return false;
        }

        // 跟随事件动态更新列表
        // Add: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        // Remove: 检查列表中是否存在这个 PII，如果存在，则修改状态为 不在架
        //      如果不存在这个 PII，则不做任何动作
        // Update: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        async Task ChangeEntities(BaseChannel<IRfid> channel,
            TagChangedEventArgs e)
        {
            // 读者。不再精细的进行增删改跟踪操作，而是笼统地看 TagList.Patrons 集合即可
            var task = RefreshPatrons();

            // 开门状态下，动态信息暂时不要合并
            bool changed = false;

            List<TagAndData> tags = new List<TagAndData>();
            if (e.AddBooks != null)
                tags.AddRange(e.AddBooks);
            if (e.UpdateBooks != null)
                tags.AddRange(e.UpdateBooks);

            // 新添加标签(或者更新标签信息)
            foreach (var tag in tags)
            {
                // 没有 TagInfo 信息的先跳过
                if (tag.OneTag.TagInfo == null)
                    continue;

                // 看看 _all 里面有没有
                var results = Find(_all, tag);
                if (results.Count == 0)
                {
                    if (Add(_adds, tag) == true)
                        changed = true;
                    if (Remove(_removes, tag) == true)
                        changed = true;
                }
                else
                {
                    // 要把 _adds 和 _removes 里面都去掉
                    if (Remove(_adds, tag) == true)
                        changed = true;
                    if (Remove(_removes, tag) == true)
                        changed = true;
                }
            }

            // 拿走标签
            foreach (var tag in e.RemoveBooks)
            {
                if (tag.OneTag.TagInfo == null)
                    continue;

                // 看看 _all 里面有没有
                var results = Find(_all, tag);
                if (results.Count > 0)
                {
                    if (Remove(_adds, tag) == true)
                        changed = true;
                    if (Add(_removes, tag) == true)
                        changed = true;
                }
                else
                {
                    // _all 里面没有，很奇怪。但，
                    // 要把 _adds 和 _removes 里面都去掉
                    if (Remove(_adds, tag) == true)
                        changed = true;
                    if (Remove(_removes, tag) == true)
                        changed = true;
                }
            }

            if (changed == true)
            {
                this.doorControl.DisplayCount(_all, _adds, _removes);
            }
        }

        void InitialEntities()
        {
            _all.Clear();
            var books = TagList.Books;
            foreach (var tag in books)
            {
                _all.Add(NewEntity(tag));
            }

            this.doorControl.DisplayCount(_all, _adds, _removes);
        }

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
                        PatronClear();
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

        void PatronClear()
        {
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
            this.NavigationService.Navigate(new PageMenu());
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            // OpenDoor();
        }

        class ActionInfo
        {
            public Entity Entity { get; set; }
            public string Action { get; set; }  // borrow/return
        }

        // 关门，或者更换读者的时候，向服务器提交出纳请求
        void SubmitCheckInOut()
        {
            // TODO: 如果当前没有读者身份，则当作初始化处理，将书柜内的全部图书做还书尝试；被拿走的图书记入本地日志(所谓无主操作)
            // TODO: 注意还书，也就是往书柜里面放入图书，是不需要具体读者身份就可以提交的

            List<ActionInfo> actions = new List<ActionInfo>();
            foreach (var entity in _adds)
            {
                actions.Add(new ActionInfo { Entity = entity, Action = "return" });
            }
            foreach (var entity in _removes)
            {
                actions.Add(new ActionInfo { Entity = entity, Action = "borrow" });
            }

            if (actions.Count == 0)
                return;

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

            // 检查读者卡状态是否 OK
            if (IsPatronOK("open", out string check_message) == false)
            {
                if (string.IsNullOrEmpty(check_message))
                    check_message = $"读卡器上的当前读者卡状态不正确。无法进行 checkin/out 操作";

                DisplayError(ref progress, check_message);
                return;
            }

            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                // ClearEntitiesError();

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.ProgressBar.Value = 0;
                    progress.ProgressBar.Minimum = 0;
                    progress.ProgressBar.Maximum = actions.Count;
                }));

                // TODO: 准备工作：把涉及到的 Entity 对象的字段填充完整
                // 检查 PII 是否都具备了

                int skip_count = 0;
                int success_count = 0;
                List<string> errors = new List<string>();
                foreach (ActionInfo info in actions)
                {
                    string action = info.Action;
                    Entity entity = info.Entity;

                    string action_name = "借书";
                    if (action == "return")
                        action_name = "还书";
                    else if (action == "renew")
                        action_name = "续借";

                    /*
                    // 借书操作必须要有读者卡。(还书和续借，可要可不要)
                    if (action == "borrow")
                    {
                        if (string.IsNullOrEmpty(_patron.Barcode))
                        {
                            DisplayError(ref progress, $"请先在读卡器上放好读者卡，再进行{action_name}");
                            return;
                        }
                    }
                    */

                    long lRet = 0;
                    string strError = "";
                    string[] item_records = null;
                    string[] biblio_records = null;

                    if (action == "borrow" || action == "renew")
                    {
                        /*
                        if (action == "borrow" && entity.State == "borrowed")
                        {
                            entity.SetError($"本册是外借状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }
                        if (action == "renew" && entity.State == "onshelf")
                        {
                            entity.SetError($"本册是在馆状态。{action_name}操作被忽略 (只有处于外借状态的册才能进行续借)", "yellow");
                            skip_count++;
                            continue;
                        }
                        */
                        // TODO: 智能书柜要求强制借书。如果册操作前处在被其他读者借阅状态，要自动先还书再进行借书

                        entity.Waiting = true;
                        lRet = channel.Borrow(null,
                            action == "renew",
                            _patron.Barcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            null,
                            "item,reader,biblio", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "summary",
                            out biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out BorrowInfo borrow_info,
                            out strError);
                    }
                    else if (action == "return")
                    {
                        /*
                        if (entity.State == "onshelf")
                        {
                            entity.SetError($"本册是在馆状态。{action_name}操作被忽略", "yellow");
                            skip_count++;
                            continue;
                        }
                        */

                        /*
                        // TODO: 增加检查 EAS 现有状态功能，如果已经是 true 则不用修改，后面 API 遇到出错后也不要回滚 EAS
                        // return 操作，提前修改 EAS
                        // 注: 提前修改 EAS 的好处是比较安全。相比 API 执行完以后再修改 EAS，提前修改 EAS 成功后，无论后面发生什么，读者都无法拿着这本书走出门禁
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, action == "return");
                            if (result.Value == -1)
                            {
                                entity.SetError($"{action_name}时修改 EAS 动作失败: {result.ErrorInfo}", "red");
                                errors.Add($"册 '{entity.PII}' {action_name}时修改 EAS 动作失败: {result.ErrorInfo}");
                                continue;
                            }
                        }
                        */
                        // 智能书柜不使用 EAS 状态。可以考虑统一修改为 EAS Off 状态？

                        entity.Waiting = true;
                        lRet = channel.Return(null,
                            "return",
                            _patron.Barcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            "item,reader,biblio", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "summary",
                            out biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out ReturnInfo return_info,
                            out strError);
                    }

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Value++;
                    }));

                    if (biblio_records != null && biblio_records.Length > 0)
                        entity.Title = biblio_records[0];

                    string title = entity.PII;
                    if (string.IsNullOrEmpty(entity.Title) == false)
                        title += " (" + entity.Title + ")";

                    if (lRet == -1)
                    {
                        /*
                        // return 操作如果 API 失败，则要改回原来的 EAS 状态
                        if (action == "return")
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, false);
                            if (result.Value == -1)
                                strError += $"\r\n并且复原 EAS 状态的动作也失败了: {result.ErrorInfo}";
                        }
                        */

                        entity.SetError($"{action_name}操作失败: {strError}", "red");
                        // TODO: 这里最好用 title
                        errors.Add($"册 '{title}': {strError}");
                        continue;
                    }

                    // TODO: 把 _adds 和 _removes 归入 _all
                    // 是否一边处理一边动态修改 _all?
                    if (action == "return")
                        Add(_all, entity);
                    else
                        Remove(_all, entity);

                    Remove(_adds, entity);
                    Remove(_removes, entity);

                    /*
                    // borrow 操作，API 之后才修改 EAS
                    // 注: 如果 API 成功但修改 EAS 动作失败(可能由于读者从读卡器上过早拿走图书导致)，读者会无法把本册图书拿出门禁。遇到此种情况，读者回来补充修改 EAS 一次即可
                    if (action == "borrow")
                    {
                        var result = SetEAS(entity.UID, entity.Antenna, action == "return");
                        if (result.Value == -1)
                        {
                            entity.SetError($"虽然{action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}", "yellow");
                            errors.Add($"册 '{entity.PII}' {action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}");
                        }
                    }
                    */

                    // 刷新显示
                    {
                        if (item_records?.Length > 0)
                            entity.SetData(entity.ItemRecPath, item_records[0]);

                        if (entity.Error != null)
                            continue;

                        string message = $"{action_name}成功";
                        if (lRet == 1 && string.IsNullOrEmpty(strError) == false)
                            message = strError;
                        entity.SetError(message,
                            lRet == 1 ? "yellow" : "green");
                        success_count++;
                        // 刷新显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    progress.ProgressBar.Visibility = Visibility.Collapsed;
                    // progress.ProgressBar.Value = progress.ProgressBar.Maximum;
                }));

                // 修改 borrowable
                // booksControl.SetBorrowable();

                if (errors.Count > 0)
                {
                    string error = StringUtil.MakePathList(errors, "\r\n");
                    string message = $"操作出错 {errors.Count} 个";
                    if (success_count > 0)
                        message += $"，成功 {success_count} 个";
                    if (skip_count > 0)
                        message += $" (另有 {skip_count} 个被忽略)";

                    DisplayError(ref progress, message);
                    App.CurrentApp.Speak(message);

                    return; // new NormalResult { Value = -1, ErrorInfo = StringUtil.MakePathList(errors, "; ") };
                }
                else
                {
                    // 成功
                    string backColor = "green";
                    string message = $"操作成功 {success_count} 笔";
                    string speak = $"出纳完成";

                    if (skip_count > 0)
                        message += $" (另有 {skip_count} 笔被忽略)";
                    if (skip_count > 0 && success_count == 0)
                    {
                        backColor = "yellow";
                        message = $"全部 {skip_count} 笔出纳操作被忽略";
                        speak = $"出纳失败";
                    }
                    if (skip_count == 0 && success_count == 0)
                    {
                        backColor = "yellow";
                        message = $"请先把图书放到读卡器上，再进行 出纳 操作";
                        speak = $"出纳失败";
                    }

                    DisplayError(ref progress, message, backColor);

                    // 重新装载读者信息和显示
                    // var task = FillPatronDetail(true);
                    this.doorControl.DisplayCount(_all, _adds, _removes);

                    App.CurrentApp.Speak(speak);
                }

                return; // new NormalResult { Value = success_count };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));
            }
        }

    }
}
