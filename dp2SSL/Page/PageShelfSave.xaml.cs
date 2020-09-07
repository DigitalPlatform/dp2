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

namespace dp2SSL
{
    /// <summary>
    /// PageShelf.xaml 的交互逻辑
    /// </summary>
    public partial class PageShelfSave : MyPage, INotifyPropertyChanged
    {
        /*
        LayoutAdorner _adorner = null;
        AdornerLayer _layer = null;
        */

        EntityCollection _entities = new EntityCollection();
        Patron _patron = new Patron();

        List<Entity> _adds = new List<Entity>();
        List<Entity> _removes = new List<Entity>();

        public PageShelfSave()
        {
            InitializeComponent();

            _patronErrorTable = new ErrorTable((e) =>
            {
                _patron.Error = e;
            });

            Loaded += PageShelf_Loaded;
            Unloaded += PageShelf_Unloaded;

            this.DataContext = this;

            this.booksControl.SetSource(_entities);
            this.patronControl.DataContext = _patron;

            this._patron.PropertyChanged += _patron_PropertyChanged;

            this.doorControl.OpenDoor += DoorControl_OpenDoor;

            App.CurrentApp.PropertyChanged += CurrentApp_PropertyChanged;



            // this.error.Text = "test";
        }

        private void DoorControl_OpenDoor(object sender, OpenDoorEventArgs e)
        {
            // MessageBox.Show(e.Name);
            var result = RfidManager.OpenShelfLock(e.Door.LockPath);
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

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void PageShelf_Loaded(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            FingerprintManager.SetError += FingerprintManager_SetError;
            FingerprintManager.Touched += FingerprintManager_Touched;

            App.TagChanged += CurrentApp_TagChanged;

            RfidManager.ListLocks += RfidManager_ListLocks;

            RfidManager.ClearCache();
            // 注：将来也许可以通过(RFID 以外的)其他方式输入图书号码
            if (string.IsNullOrEmpty(RfidManager.Url))
                this.SetGlobalError("rfid", "尚未配置 RFID 中心 URL");

            /*
            _layer = AdornerLayer.GetAdornerLayer(this.mainGrid);
            _adorner = new LayoutAdorner(this);
            */
            InitializeLayer(this.mainGrid);

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

            // RfidManager.LockCommands = DoorControl.GetLockCommands();

            await FillAsync(new CancellationToken());
        }

        private void RfidManager_ListLocks(object sender, ListLocksEventArgs e)
        {
            if (e.Result.Value == -1)
                return;

            foreach(var state in e.Result.States)
            {
                // SetLockState(state);
            }
        }

        /*
        void SetLockState(LockState state)
        {
            this.doorControl.SetLockState(state);
        }
        */

        private void PageShelf_Unloaded(object sender, RoutedEventArgs e)
        {
            RfidManager.SetError -= RfidManager_SetError;

            App.TagChanged -= CurrentApp_TagChanged;

            FingerprintManager.Touched -= FingerprintManager_Touched;
            FingerprintManager.SetError -= FingerprintManager_SetError;

            RfidManager.ListLocks -= RfidManager_ListLocks;
        }

        // 从指纹阅读器获取消息(第一阶段)
#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void FingerprintManager_Touched(object sender, TouchedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            SetPatronInfo(e.Result);

            await FillPatronDetailAsync();

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


        // 首次填充图书列表
        async Task<NormalResult> FillAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                FillLocationBooks(_entities,
        "?", // App.ShelfLocation,
        token);
            });

            List<Entity> update_entities = new List<Entity>();
            update_entities.AddRange(_entities);

            // TODO: 首次从累积的列表里面初始化
            update_entities.AddRange(InitialEntities());

            var task = RefreshPatronsAsync();

            return await UpdateAsync(null, update_entities, token);
        }

        async Task<NormalResult> UpdateAsync(
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
                        await FillBookFieldsAsync(channel, update_entities, token);
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
            App.SetError(type, error);
        }

        // 第二阶段：填充图书信息的 PII 和 Title 字段
        async Task FillBookFieldsAsync(BaseChannel<IRfid> channel,
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
                        GetEntityDataResult result = await GetEntityDataAsync(entity.GetOiPii(), "");
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
                SetGlobalError("current", ex.Message);
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
                    App.Invoke(new Action(() =>
                    {
                        entities.Add(pii, "", "");
                    }));
                }
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
            }
        }

#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void CurrentApp_TagChanged(object sender, TagChangedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            await ChangeEntitiesAsync((BaseChannel<IRfid>)sender, e);
        }

        // 跟随事件动态更新列表
        // Add: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        // Remove: 检查列表中是否存在这个 PII，如果存在，则修改状态为 不在架
        //      如果不存在这个 PII，则不做任何动作
        // Update: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        async Task ChangeEntitiesAsync(BaseChannel<IRfid> channel,
            TagChangedEventArgs e)
        {
            // 读者。不再精细的进行增删改跟踪操作，而是笼统地看 TagList.Patrons 集合即可
            var task = RefreshPatronsAsync();

            // bool changed = false;
            List<Entity> update_entities = new List<Entity>();
            App.Invoke(new Action(() =>
            {
                if (e.AddBooks != null)
                    foreach (var tag in e.AddBooks)
                    {
                        var entity = _entities.OnShelf(tag);
                        if (entity != null)
                            update_entities.Add(entity);
                    }
                if (e.RemoveBooks != null)
                    foreach (var tag in e.RemoveBooks)
                    {
                        var entity = _entities.OffShelf(tag);
                        if (entity != null)
                            update_entities.Add(entity);
                        // changed = true;
                    }
                if (e.UpdateBooks != null)
                    foreach (var tag in e.UpdateBooks)
                    {
                        var entity = _entities.OnShelf(tag);
                        if (entity != null)
                            update_entities.Add(entity);
                    }
            }));

            if (update_entities.Count > 0)
            {
                // await FillBookFields(channel, update_entities);

                await UpdateAsync(channel, update_entities, new CancellationToken());

                // Trigger(update_entities);

            }
            /*
            else if (changed)
            {
                // 修改 borrowable
                booksControl.SetBorrowable();
            }*/

            /*
            if (update_entities.Count > 0)
                changed = true;
                */

            /*
            {
                if (_entities.Count == 0
        && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
        && _patron.IsFingerprintSource)
                    PatronClear();

                // 2019/7/1
                // 当读卡器上的图书全部拿走时候，自动关闭残留的模式对话框
                if (_entities.Count == 0
    && changed == true  // 限定为，当数量减少到 0 这一次，才进行清除
    )
                    CloseDialogs();

                // 当列表为空的时候，主动清空一次 tag 缓存。这样读者可以用拿走全部标签一次的方法来迫使清除缓存(比如中途利用内务修改过 RFID 标签的 EAS)
                // TODO: 不过此举对反复拿放图书的响应速度有一定影响。后面也可以考虑继续改进(比如设立一个专门的清除缓存按钮，这里就不要清除缓存了)
                if (_entities.Count == 0 && TagList.TagTableCount > 0)
                    TagList.ClearTagTable(null);
            }
            */
        }

        List<Entity> InitialEntities()
        {
            var books = TagList.Books;
            if (books.Count == 0)
                return new List<Entity>();

            List<Entity> update_entities = new List<Entity>();
            App.Invoke(new Action(() =>
            {
                foreach (var tag in books)
                {
                    var entity = _entities.OnShelf(tag);
                    if (entity != null)
                        update_entities.Add(entity);
                }
            }));

            return update_entities;
        }

        async Task RefreshPatronsAsync()
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
                        // result.Value:
                        //      -1  出错
                        //      0   未进行刷新
                        //      1   成功进行了刷新
                        var fill_result = _patron.Fill(patrons[0].OneTag);
                        if (fill_result.Value == 0)
                            return;

                        SetPatronError("rfid_multi", "");   // 2019/5/22

                        // 2019/5/29
                        await FillPatronDetailAsync();
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
        async Task<NormalResult> FillPatronDetailAsync(bool force = false)
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

            if (string.IsNullOrEmpty(_patron.State) == true)
                OpenDoor();

            if (force)
                _patron.PhotoPath = "";
            // string old_photopath = _patron.PhotoPath;
            App.Invoke(new Action(() =>
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
                        await FillBookFieldsAsync(channel, entities, new CancellationToken());
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

            if (this.patronControl.BorrowedEntities.Count > 0)
            {
                if (!Application.Current.Dispatcher.CheckAccess())
                    App.Invoke(new Action(() =>
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
                _ = Task.Run(() =>
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
                    App.Invoke(new Action(() =>
                    {
                        patronControl.Visibility = Visibility.Visible;
                        _visiblityChanged = true;
                    }));
                // 如果读者卡又被拿走了，则要恢复 patronControl 的隐藏状态
                else if (string.IsNullOrEmpty(_patron.UID) == true && string.IsNullOrEmpty(_patron.Barcode) == true
    && this.patronControl.Visibility == Visibility.Visible
    && _visiblityChanged)
                    App.Invoke(new Action(() =>
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

            App.Invoke(new Action(() =>
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
                App.Invoke(new Action(() =>
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

        /*
        void AddLayer()
        {
            _layer.Add(_adorner);
        }

        void RemoveLayer()
        {
            _layer.Remove(_adorner);
        }
        */

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageMenu.MenuPage);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenDoor();
        }
    }
}
