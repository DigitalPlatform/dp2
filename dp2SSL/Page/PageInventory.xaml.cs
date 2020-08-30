using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.WPF;
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
                ProcessTag(tag);
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

            // TODO: 对离开的 tag 变化为灰色颜色

            foreach (var tag in e.AddTags)
            {
                ProcessTag(tag);
            }

            foreach (var tag in e.UpdateTags)
            {
                ProcessTag(tag);
            }
        }

        void ProcessTag(TagAndData tag)
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

            if (string.IsNullOrEmpty(entity.PII) == false
                && string.IsNullOrEmpty(info.State))
            {
                info.State = "processing";
                InventoryData.AppendList(entity);
                InventoryData.ActivateInventory();
            }
        }


    }
}
