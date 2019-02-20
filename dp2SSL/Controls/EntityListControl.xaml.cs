using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
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

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Xml;

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
                _borrowable = value;
                OnPropertyChanged("Borrowable");
            }
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
                _renewable = value;
                OnPropertyChanged("Renewable");
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
                _returnable = value;
                OnPropertyChanged("Returnable");
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
