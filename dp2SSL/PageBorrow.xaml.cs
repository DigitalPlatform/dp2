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
    /// <summary>
    /// PageBorrow.xaml 的交互逻辑
    /// </summary>
    public partial class PageBorrow : Page, INotifyPropertyChanged
    {
        public PageBorrow()
        {
            InitializeComponent();

            this.DataContext = this;

            this.entities.PropertyChanged += Entities_PropertyChanged;
        }

        private void Entities_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Borrowable")
                OnPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public string Borrowable
        {
            get
            {
                return entities.Borrowable;
            }
            set
            {
                entities.Borrowable = value;
            }
        }
    }
}
