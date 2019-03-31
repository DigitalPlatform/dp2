using DigitalPlatform.Text;
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
    /// PageError.xaml 的交互逻辑
    /// </summary>
    public partial class PageError : Page
    {
        public PageError()
        {
            InitializeComponent();

            this.error.Text = StringUtil.MakePathList(App.CurrentApp.Errors, "\r\n");
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageMenu());
        }
    }
}
