using DigitalPlatform;
using DigitalPlatform.RFID;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

            foreach(var tag in e.AddTags)
            {
                var entity = InventoryData.AddEntity(tag, out bool isNewly);
                if (isNewly)
                {
                    App.Invoke(new Action(() =>
                    {
                        _entities.Add(entity);
                    }));
                }
            }

            foreach (var tag in e.UpdateTags)
            {
                var entity = InventoryData.AddEntity(tag, out bool isNewly);
                if (isNewly)
                {
                    App.Invoke(new Action(() =>
                    {
                        _entities.Add(entity);
                    }));
                }
            }
        }


    }
}
