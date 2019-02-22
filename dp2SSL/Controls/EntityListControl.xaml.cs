using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace dp2SSL
{
    /// <summary>
    /// 显示册信息的控件
    /// EntityListControl.xaml 的交互逻辑
    /// </summary>
    public partial class EntityListControl : UserControl, INotifyPropertyChanged
    {
        // public string RfidUrl { get; set; }

        EntityCollection _entities = null;

        public EntityListControl()
        {
            InitializeComponent();

            ((INotifyCollectionChanged)listView.Items).CollectionChanged += ListView_CollectionChanged;

            // RfidUrl = WpfClientInfo.Config.Get("global", "rfidUrl", "");
        }

        public void SetSource(EntityCollection entities)
        {
            _entities = entities;
            this.listView.ItemsSource = entities;
        }

        string _borrowable = null;

        public string Borrowable
        {
            get
            {
                return _borrowable;
            }
            set
            {
                if (_borrowable != value)
                {
                    _borrowable = value;
                    OnPropertyChanged("Borrowable");
                }
            }
        }

#if NO
        public int ItemsCount
        {
            get
            {
                return listView.Items.Count;
            }
        }
#endif

#if NO
        public static readonly DependencyProperty CompanyNameProperty =
  DependencyProperty.Register("ItemsCount", typeof(int), typeof(EntityListControl), new UIPropertyMetadata(100));

        public int ItemsCount
        {
            get { return (int)this.GetValue(CompanyNameProperty); }
            set { this.SetValue(CompanyNameProperty, value); }
        }
#endif
        int _itemCount = 0;

        public int ItemCount
        {
            get { return _itemCount; }
            set
            {
                if (_itemCount != value)
                {
                    _itemCount = value;
                    OnPropertyChanged("ItemCount");
                }
            }
        }

        private void ListView_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            ItemCount = listView.Items.Count;
        }

        string _renewable = null;

        public string Renewable
        {
            get
            {
                return _renewable;
            }
            set
            {
                if (_renewable != value)
                {
                    _renewable = value;
                    OnPropertyChanged("Renewable");
                }
            }
        }

        string _returnable = null;

        public string Returnable
        {
            get
            {
                return _returnable;
            }
            set
            {
                if (_returnable != value)
                {
                    _returnable = value;
                    OnPropertyChanged("Returnable");
                }
            }
        }

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

#if NO
        static App App
        {
            get
            {
                return ((App)Application.Current);
            }
        }
#endif

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
#if NO
            Task.Run(() =>
            {
                Refresh();
            });
#endif
        }


        public void SetBorrowable()
        {
            int borrowable_count = 0;
            int returnable_count = 0;
            int renewable_count = 0;
            foreach (Entity entity in _entities)
            {
                if (entity.State == "onshelf")
                    borrowable_count++;
                if (entity.State == "borrowed")
                    returnable_count++;
                if (entity.State == "borrowed")
                    renewable_count++;
            }

            this.Borrowable = borrowable_count.ToString();
            this.Returnable = returnable_count.ToString();
            this.Renewable = renewable_count.ToString();
        }

    }

}
