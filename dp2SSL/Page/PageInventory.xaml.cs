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
using static dp2SSL.InventoryData;

using DigitalPlatform;
using DigitalPlatform.RFID;

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

            SoundMaker.InitialSequence(e.AddTags.Count);

            foreach (var tag in e.AddTags)
            {
                SoundMaker.NextSound();
                ProcessTag(channel, tag);
            }

            SoundMaker.StopCurrent();

            /*
            foreach (var tag in e.UpdateTags)
            {
                ProcessTag(channel, tag);
            }
            */
        }

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
                            error = get_result.ErrorInfo;

                            // TODO: 当有一行以上 GetTagInfo() 出错时，要不断发出响声警告。
                        
                        }
                        else
                        {
                            // 把 PII 显示出来
                            tag.OneTag.TagInfo = get_result.TagInfo;
                            InventoryData.AddEntity(tag, out isNewly);
                            info.State = "";

                            // 朗读出错 entity 数量
                            var count = InventoryData.RemoveErrorEntity(entity, out bool changed);
                            if (changed == true)
                                App.CurrentApp.SpeakSequence(count.ToString());
                        }
                    }

                    entity.Error = error;
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
    }
}
