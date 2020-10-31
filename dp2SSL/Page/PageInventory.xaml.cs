using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Collections;

using static dp2SSL.InventoryData;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    // https://stackoverflow.com/questions/16477552/remove-highlight-effect-from-listviewitem
    /// <summary>
    /// PageInventory.xaml 的交互逻辑
    /// </summary>
    public partial class PageInventory : MyPage, INotifyPropertyChanged
    {
        EntityCollection _entities = new EntityCollection();

        public PageInventory()
        {
            InitializeComponent();

            this.DataContext = this;

            this.list.ItemsSource = _entities;
            /*
            _entities.Add(new Entity { UID = "111", PII="PII", Title="title1" });
            _entities.Add(new Entity { UID = "222", PII = "PII", Title = "title2" });
            */
            this.Loaded += PageInventory_Loaded;
            this.Unloaded += PageInventory_Unloaded;

            /*
            _ = Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        _tagChangedCount = 0;
                        await InitialEntitiesAsync();
                        if (_tagChangedCount == 0)
                            break;  // 只有当初始化过程中没有被 TagChanged 事件打扰过，才算初始化成功了。否则就要重新初始化
                    }
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"InitialEntitiesAsync() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
            });
            */
        }

        private void PageInventory_Unloaded(object sender, RoutedEventArgs e)
        {
            App.IsPageInventoryActive = false;
            App.NewTagChanged -= CurrentApp_NewTagChanged;
        }

        private void PageInventory_Loaded(object sender, RoutedEventArgs e)
        {
            App.NewTagChanged += CurrentApp_NewTagChanged;
            App.IsPageInventoryActive = true;

            RefreshActionModeMenu();

            _ = Task.Run(() =>
            {
                CancellationTokenSource cancel = new CancellationTokenSource();

                ProgressWindow progress = null;
                App.Invoke(new Action(() =>
                {
                    progress = new ProgressWindow();
                    progress.TitleText = "dp2SSL -- 盘点";
                    progress.MessageText = "正在获取 UID 对照信息，请稍候 ...";
                    progress.Owner = Application.Current.MainWindow;
                    progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    progress.Closed += (s, e1) =>
                    {
                        cancel.Cancel();
                    };
                    progress.okButton.Visibility = Visibility.Collapsed;
                    // progress.okButton.Content = "停止";
                    App.SetSize(progress, "middle");
                    progress.BackColor = "green";
                    progress.Show();
                }));

                try
                {
                    Hashtable uid_table = new Hashtable();
                    var result = InventoryData.DownloadUidTable(
            null,
            uid_table,
            (text) =>
            {
                App.Invoke(new Action(() =>
                {
                    progress.MessageText = text;
                }));
            },
            cancel.Token);
                    InventoryData.SetUidTable(uid_table);
                }
                catch (Exception ex)
                {
                    WpfClientInfo.WriteErrorLog($"准备册记录过程出现异常: {ExceptionUtil.GetDebugText(ex)}");
                }
                finally
                {
                    App.Invoke(new Action(() =>
                    {
                        progress.Close();
                    }));
                }
            });

        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private void goHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(PageMenu.MenuPage);
        }

#if NO
        int _tagChangedCount = 0;

        // 首次初始化 Entity 列表
        async Task<NormalResult> InitialEntitiesAsync()
        {
            App.Invoke(new Action(() =>
            {
                _entities.Clear();  // 2019/9/4
            }));

            foreach (var tag in NewTagList.Tags)
            {
                ProcessTag(null, tag);
            }

            return new NormalResult();
        }
#endif

        // 新版本的事件
#pragma warning disable VSTHRD100 // 避免使用 Async Void 方法
        private async void CurrentApp_NewTagChanged(object sender, NewTagChangedEventArgs e)
#pragma warning restore VSTHRD100 // 避免使用 Async Void 方法
        {
            /*
            {
                await ShelfData.ChangeEntitiesAsync((BaseChannel<IRfid>)sender,
                    sep_result,
                    () =>
                    {
                        // 如果图书数量有变动，要自动清除挡在前面的残留的对话框
                        CloseDialogs();
                    });
            }
            */

            var channel = (BaseChannel<IRfid>)sender;
            // TODO: 对离开的 tag 变化为灰色颜色

            bool speaked = false;
            // 2020/10/10
            // TODO: 发出什么响声表示?
            // 把以前遗留的出错 entity 尝试重新 GetTagInfo()
            foreach (var entity in InventoryData.ErrorEntities)
            {
                FillEntity(channel, entity, (e1) =>
                {
                    // 说过一次便不再说
                    if (speaked == true)
                        return false;
                    speaked = SpeakLocation(e1);
                    return speaked;
                });
            }

            // 筛选出需要 GetTagInfo() 的那些标签
            FilterTags(channel, e.AddTags);

#if NO
            SoundMaker.InitialSequence(e.AddTags.Count);

            foreach (var tag in e.AddTags)
            {
                SoundMaker.NextSound();
                ProcessTag(channel, tag);
            }

            SoundMaker.StopCurrent();
#endif

            /*
            foreach (var tag in e.UpdateTags)
            {
                ProcessTag(channel, tag);
            }
            */
        }

        // 筛选出需要 GetTagInfo() 的那些标签
        void FilterTags(BaseChannel<IRfid> channel, List<TagAndData> tags)
        {
            List<Entity> entities1 = new List<Entity>();
            List<Entity> all = new List<Entity>();

            foreach (var tag in tags)
            {
                var entity = InventoryData.AddEntity(tag, out bool isNewly);
                var info = entity.Tag as ProcessInfo;
                if (info == null)
                {
                    info = new ProcessInfo();
                    entity.Tag = info;
                }

                if (isNewly)
                {
                    App.Invoke(new Action(() =>
                    {
                        _entities.Add(entity);
                    }));
                }

                if (string.IsNullOrEmpty(entity.PII))
                    entities1.Add(entity);

                all.Add(entity);
            }

            // 准备音阶
            SoundMaker.InitialSequence(entities1.Count);

            bool speaked = false;
            // 集中 GetTagInfo()
            foreach (var entity in entities1)
            {
                SoundMaker.NextSound();

                FillEntity(channel, entity,
                    (e) =>
                    {
                        // 说过一次便不再说
                        if (speaked == true)
                            return false;
                        speaked = SpeakLocation(e);
                        return speaked;
                    });
            }

            // 停止音阶响声
            SoundMaker.StopCurrent();

            // 获取题名等
            foreach (var entity in all)
            {
                var info = entity.Tag as ProcessInfo;

                if (string.IsNullOrEmpty(entity.PII) == false
&& string.IsNullOrEmpty(info.State))
                {
                    info.State = "processing";  // 正在获取册信息
                    InventoryData.AppendList(entity);
                    InventoryData.ActivateInventory();
                }
            }
        }

#if NO
        void ProcessTag(BaseChannel<IRfid> channel, TagAndData tag)
        {
            var entity = InventoryData.AddEntity(tag, out bool isNewly);

            var info = entity.Tag as ProcessInfo;
            if (info == null)
            {
                info = new ProcessInfo();
                entity.Tag = info;
            }

            if (isNewly)
            {
                App.Invoke(new Action(() =>
                {
                    _entities.Add(entity);
                }));
            }

            FillEntity(channel, entity);
        }
#endif
        delegate bool delegate_speakLocation(Entity entity);

        void FillEntity(BaseChannel<IRfid> channel,
            Entity entity,
            delegate_speakLocation func_speakLocation)
        {
            var info = entity.Tag as ProcessInfo;

            // 2020/10/7
            // 尝试获取 PII
            if (string.IsNullOrEmpty(entity.PII))
            {
                if (InventoryData.UidExsits(entity.UID, out string pii))
                {
                    entity.PII = pii;
                }
                else
                {
                    string error = null;
                    if (channel.Started == false)
                        error = "RFID 通道尚未启动";
                    else
                    {
                        var get_result = channel.Object.GetTagInfo(entity.ReaderName, entity.UID, Convert.ToUInt32(entity.Antenna), "quick");

                        /*
                        // testing
                        get_result.Value = -1;
                        get_result.ErrorInfo = "GetTagInfo() error error 1234 error 1345";
                        */

                        if (get_result.Value == -1)
                        {
                            SoundMaker.ErrorSound();

                            // 朗读出错 entity 数量
                            var count = InventoryData.AddErrorEntity(entity, out bool changed);
                            if (changed == true)
                                App.CurrentApp.SpeakSequence(count.ToString());

                            info.State = "errorGetTagInfo";
                            info.ErrorCount++;
                            error = get_result.ErrorInfo;

                            // TODO: 当有一行以上 GetTagInfo() 出错时，要不断发出响声警告。
                            entity.Error = error;

                            if (info.ErrorCount > 5)
                            {
                                if (func_speakLocation?.Invoke(entity) == true)
                                    info.ErrorCount = 0;
                            }
                        }
                        else
                        {
                            entity.Error = null;

                            // 把 PII 显示出来
                            InventoryData.UpdateEntity(entity, get_result.TagInfo);
                            info.State = "";

                            // 朗读出错 entity 数量
                            var count = InventoryData.RemoveErrorEntity(entity, out bool changed);
                            if (changed == true)
                                App.CurrentApp.SpeakSequence(count.ToString());

                            if (StringUtil.IsInList("blankTag", entity.ErrorCode))
                                App.CurrentApp.SpeakSequence(entity.Error);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(entity.PII) == false
                && string.IsNullOrEmpty(info.State))
            {
                info.State = "processing";  // 正在获取册信息
                InventoryData.AppendList(entity);
                InventoryData.ActivateInventory();
            }
        }

        string FindTitle(int index)
        {
            int prev_index = index;
            int next_index = index;

            while (true)
            {
                prev_index--;
                next_index++;
                if (IsOut(prev_index))
                    break;
                Entity prev = _entities[prev_index];
                if (string.IsNullOrEmpty(prev.Title) == false)
                    return prev.Title;
                if (IsOut(next_index))
                    break;
                Entity next = _entities[next_index];
                if (string.IsNullOrEmpty(next.Title) == false)
                    return next.Title;
            }

            return null;

            bool IsOut(int i)
            {
                if (i < 0 || i > _entities.Count - 1)
                    return true;
                return false;
            }
        }

        // 语音播报提醒 entity 的位置
        bool SpeakLocation(Entity entity)
        {
            int index = _entities.IndexOf(entity);
            if (index == -1)
                return false;

            string title = FindTitle(index);
            if (title == null)
                return false;

            App.CurrentApp.Speak($"在 {CutTitle(title)} 附近");
            return true;
        }


#if NO
        // 语音播报提醒 entity 的位置
        void SpeakLocation(Entity entity)
        {
            int index = _entities.IndexOf(entity);
            if (index == -1)
                return;
            Entity prev = null;
            if (index > 0)
                prev = _entities[index-1];

            string prev_title = CutTitle(prev?.Title);

            Entity next = null;
            if (index < _entities.Count - 1)
                next = _entities[index++];

            string next_title = CutTitle(next?.Title);

            if (prev != null && next != null)
                App.CurrentApp.Speak($"在 {prev_title} 和 {next_title} 之间");
            else if (prev == null && next != null)
                App.CurrentApp.Speak($"在 {next_title} 之前");
            else if (prev != null && next == null)
                App.CurrentApp.Speak($"在 {prev_title} 之后");
        }

#endif

        static string CutTitle(string title)
        {
            if (title == null)
                return null;

            int index = title.IndexOf("/");
            if (index != -1)
                title = title.Substring(0, index).Trim();

            if (title.Length > 20)
                return title.Substring(0, 20);

            return title;
        }

        private void clearList_Click(object sender, RoutedEventArgs e)
        {
            App.Invoke(new Action(() =>
            {
                _entities.Clear();
            }));
            InventoryData.Clear();
        }

        private void beginSound_Click(object sender, RoutedEventArgs e)
        {
            SoundMaker.Start();
        }

        private void stopSound_Click(object sender, RoutedEventArgs e)
        {
            SoundMaker.Stop();
        }

        private void addSound_Click(object sender, RoutedEventArgs e)
        {
            SoundMaker.AddSound();
        }

        // 动作模式
        /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
         * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
         * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
         * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
         * */
        static string _actionMode = "setUID";    // 空/setUID/setCurrentLocation/setLocation/verifyEAS 中之一或者组合

        public static string ActionMode
        {
            get
            {
                return _actionMode;
            }
        }

        void UpdateActionMode()
        {
            var value = _actionMode;
            StringUtil.SetInList(ref value, "setUID", this.actionSetUID.IsChecked);
            StringUtil.SetInList(ref value, "setCurrentLocation", this.actionSetCurrentLocation.IsChecked);
            StringUtil.SetInList(ref value, "setLocation", this.actionSetLocation.IsChecked);
            StringUtil.SetInList(ref value, "verifyEAS", this.actionVerifyEas.IsChecked);
            _actionMode = value;
        }

        void RefreshActionModeMenu()
        {
            var value = _actionMode;
            this.actionSetUID.IsChecked = StringUtil.IsInList("setUID", value);
            this.actionSetCurrentLocation.IsChecked = StringUtil.IsInList("setCurrentLocation", value);
            this.actionSetLocation.IsChecked = StringUtil.IsInList("setLocation", value);
            this.actionVerifyEas.IsChecked = StringUtil.IsInList("verifyEAS", value);
        }

        private void actionMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            menuItem.IsChecked = !menuItem.IsChecked;
            UpdateActionMode();
        }
    }
}
