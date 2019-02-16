using System;
using System.Collections.Generic;
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
    /// PageMenu.xaml 的交互逻辑
    /// </summary>
    public partial class PageMenu : Page
    {
        public PageMenu()
        {
            InitializeComponent();
        }

        private void Button_Borrow_Click(object sender, RoutedEventArgs e)
        {
#if NO
            Window mainWindow = Application.Current.MainWindow;
            var page = new PageBorrow();
            // page.Background = Brushes.Red;
            mainWindow.Content = page;
#endif
            this.NavigationService.Navigate(new PageBorrow("borrow"));
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            Window cfg_window = new ConfigWindow();
            cfg_window.ShowDialog();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageBorrow("return"));
        }

        private void RenewBotton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageBorrow("renew"));
        }
    }
}
