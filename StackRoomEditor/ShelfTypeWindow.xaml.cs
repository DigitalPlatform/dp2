using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace StackRoomEditor
{
    /// <summary>
    /// ShelfTypeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShelfTypeWindow : Window
    {
        public BookShelf BookShelf = null;
        // 书架模板
        ObservableCollection<BookShelf> _shelfModels = new ObservableCollection<BookShelf>();
        public ObservableCollection<BookShelf>  ShelfModels
        {
            get
            {
                return _shelfModels;
            }
        }

        public ShelfTypeWindow(List<BookShelf> shelfModels)
        {
            foreach (BookShelf item in shelfModels)
            {
                this._shelfModels.Add(item);
            }

            InitializeComponent();
        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
                this.button_OK.IsEnabled = true;
            else
                this.button_OK.IsEnabled = false;
        }

        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.BookShelf = (BookShelf)this.listView1.SelectedItems[0];
            this.Close();
        }

        private void button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.BookShelf = (BookShelf)this.listView1.SelectedItems[0];
            this.Close();
        }
    }
}
