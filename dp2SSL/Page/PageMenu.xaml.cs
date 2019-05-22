using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DigitalPlatform.Text;

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

            this.ShowsNavigationUI = false;

            this.Loaded += PageMenu_Loaded;
            this.DataContext = App.CurrentApp;
        }

        private void PageMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Application.Current.MainWindow;

            window.Left = 0;
            window.Top = 0;
            if (StringUtil.IsDevelopMode() == false)
            {
                // 最大化
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.CanResize;
                window.WindowState = WindowState.Maximized;
                //window.Width = SystemParameters.VirtualScreenWidth;
                //window.Height = SystemParameters.VirtualScreenHeight;
            }

            this.message.Text = $"dp2SSL 版本号:\r\n{WpfClientInfo.ClientVersion}";
            if (string.IsNullOrEmpty(App.CurrentApp.Error))
            {
                this.error.Visibility = Visibility.Collapsed;
            }
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
            //Window cfg_window = new ConfigWindow();
            //cfg_window.ShowDialog();

            // 测试用
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }
            this.NavigationService.Navigate(new PageSetting());
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageBorrow("return"));
        }

        private void RenewBotton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageBorrow("renew"));
        }

        private void Error_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageError());
        }

        private void Message_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetDataObject(this.message.Text, true);
        }

#if REMOVED
        #region 探测平板模式

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);
        // System metric constant for Windows XP Tablet PC Edition
        private const int SM_TABLETPC = 86;
        private const int SM_CONVERTIBLESLATEMODE = 0x2003;
        private const int SM_SYSTEMDOCKED = 0x2004;

        // https://stackoverflow.com/questions/5795010/detecting-tablet-pc
        protected bool IsRunningOnTablet()
        {
            int value = GetSystemMetrics(SM_TABLETPC);
            return (value != 0);
        }

        private static Boolean QueryTabletMode()
        {
            int state = GetSystemMetrics(SM_CONVERTIBLESLATEMODE);
            return (state == 0);    // && isTabletPC;
        }

        private static Boolean QueryDocked()
        {
            int state = GetSystemMetrics(SM_SYSTEMDOCKED);
            return (state != 0);
        }

        #endregion

#endif
    }
}
