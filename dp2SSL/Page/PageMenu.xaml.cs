using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// PageMenu.xaml 的交互逻辑
    /// </summary>
    public partial class PageMenu : Page
    {
        public static PageMenu MenuPage = null;

        public PageMenu()
        {
            InitializeComponent();

            this.ShowsNavigationUI = false;

            this.Loaded += PageMenu_Loaded;
            this.Unloaded += PageMenu_Unloaded;
            this.DataContext = App.CurrentApp;

            InitWallpaper();

            MenuPage = this;

            SetIdleEvents();
        }

        public void SetIdleEvents()
        {
            InputManager.Current.PreProcessInput -= OnActivity;
            if (_activityTimer != null)
                _activityTimer.Tick -= OnInactivity;

            var seconds = App.AutoBackMainMenuSeconds;
            if (seconds > 0)
            {
                // https://stackoverflow.com/questions/4963135/wpf-inactivity-and-activity
                InputManager.Current.PreProcessInput += OnActivity;
                _activityTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds), IsEnabled = true };
                _activityTimer.Tick += OnInactivity;
            }
        }

        ~PageMenu()
        {
            DeleteTempFiles();
        }

        bool _initialized = false;

        private void PageMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // 只初始化一次。
            // 但初始化必须放在 _loaded 事件里面，因为只有这里才能得到 Application.Current.MainWindow。构造函数那里时机偏早了，MainWindow 还没有来得及创建
            if (_initialized == false)
            {
                Initial();
                _initialized = true;

                // 初始化智能书柜
                // 过程中需要检查门锁是否关上，如果没有关上要警告，只有关上了才能进入正常的菜单画面
                if (App.Function == "智能书柜")
                    NavigatePageShelf("initial");
            }

            // 如果有读者卡，要延时提醒不要忘了拿走读者卡
            if (TagList.Patrons?.Count > 0)
            {
                PageBorrow.BeginNotifyTask();
            }

            App.TagChanged += CurrentApp_TagChanged;

            // testing
            // ShowUpdated();

            if (App.Function == "智能书柜")
                this.SetSystemName("智能书柜");
            else if (App.Function == "自助借还")
                this.SetSystemName("自助借还系统");
            else
                this.SetSystemName("盘点系统");

            SetCompanyName();
        }

        void SetCompanyName()
        {
            string filename = System.IO.Path.Combine(WpfClientInfo.UserDir, "oem");
            if (File.Exists(filename))
            {
                App.Invoke(new Action(() =>
                {
                    companyName.Foreground = Brushes.Transparent;
                }));
            }
        }

        private void CurrentApp_TagChanged(object sender, TagChangedEventArgs e)
        {
            // 如果有读者卡，要延时提醒不要忘了拿走读者卡
            if (/*PageBorrow.isPatronChanged(e) &&*/ TagList.Patrons?.Count > 0)
            {
                PageBorrow.BeginNotifyTask();
            }
        }

        private void PageMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            App.TagChanged -= CurrentApp_TagChanged;
        }

        public void UpdateMenu()
        {
            if (string.IsNullOrEmpty(App.FaceUrl))
                this.registerFace.Visibility = Visibility.Collapsed;
            else
                this.registerFace.Visibility = Visibility.Visible;

            if (App.Function == "智能书柜")
            {
                this.shelf.Visibility = Visibility.Visible;
                this.borrowButton.Visibility = Visibility.Collapsed;
                this.returnButton.Visibility = Visibility.Collapsed;
                this.renewButton.Visibility = Visibility.Collapsed;
                this.inventory.Visibility = Visibility.Collapsed;

                this.bindPatronCard.Visibility = Visibility.Visible;
            }
            else if (App.Function == "自助借还")
            {
                this.shelf.Visibility = Visibility.Collapsed;

                this.borrowButton.Visibility = Visibility.Visible;
                this.returnButton.Visibility = Visibility.Visible;
                this.renewButton.Visibility = Visibility.Visible;
                this.inventory.Visibility = Visibility.Collapsed;

                this.bindPatronCard.Visibility = Visibility.Visible;
            }
            else
            {
                this.inventory.Visibility = Visibility.Visible;

                this.shelf.Visibility = Visibility.Collapsed;
                this.borrowButton.Visibility = Visibility.Collapsed;
                this.returnButton.Visibility = Visibility.Collapsed;
                this.renewButton.Visibility = Visibility.Collapsed;

                this.registerFace.Visibility = Visibility.Collapsed;
                this.bindPatronCard.Visibility = Visibility.Collapsed;
            }
        }

        void Initial()
        {
            Window window = Application.Current.MainWindow;

            window.Left = 0;
            window.Top = 0;
            if (// StringUtil.IsDevelopMode() == false &&
                App.FullScreen == true)
            {
                // 最大化
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.CanResize;
                window.WindowState = WindowState.Maximized;
                //window.Width = SystemParameters.VirtualScreenWidth;
                //window.Height = SystemParameters.VirtualScreenHeight;
            }

            this.message.Text = $"dp2SSL 版本号:\r\n{WpfClientInfo.ClientVersion}";

            /*
            if (string.IsNullOrEmpty(App.CurrentApp.Error))
            {
                this.error.Visibility = Visibility.Collapsed;
            }
            */

            /*
            ColorAnimation colorChangeAnimation1 = new ColorAnimation
            {
                From = ((SolidColorBrush)this.borrowButton.Background).Color,
                To = Colors.Black,
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true
            };

            PropertyPath colorTargetPath = new PropertyPath("(Button.Background).(SolidColorBrush.Color)");
            Storyboard CellBackgroundChangeStory = new Storyboard();
            Storyboard.SetTarget(colorChangeAnimation1, this.borrowButton);
            Storyboard.SetTargetProperty(colorChangeAnimation1, colorTargetPath);
            CellBackgroundChangeStory.Children.Add(colorChangeAnimation1);

            CellBackgroundChangeStory.RepeatBehavior = RepeatBehavior.Forever;
            CellBackgroundChangeStory.Begin();
*/

            // var task = SetWallPaper();

            UpdateMenu();
        }

        #region Wallpaper & tempo files

        List<string> _temp_filenames = new List<string>();

        void InitWallpaper()
        {
            string filename = System.IO.Path.Combine(WpfClientInfo.UserDir,
                "daily_wallpaper");
            if (File.Exists(filename) == false)
            {
                filename = System.IO.Path.Combine(WpfClientInfo.UserDir,
                    "wallpaper");
                if (File.Exists(filename) == false)
                    return;
            }

            // 复制到一个临时文件
            string temp_filename = System.IO.Path.Combine(WpfClientInfo.UserTempDir, "~" + Guid.NewGuid().ToString());
            File.Copy(filename, temp_filename, true);
            _temp_filenames.Add(temp_filename);

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filename, UriKind.Absolute);
            bitmap.EndInit();

            var brush = new ImageBrush(bitmap);
            brush.Stretch = Stretch.UniformToFill;
            this.Background = brush;
        }

        void DeleteTempFiles()
        {
            foreach (string filename in _temp_filenames)
            {
                try
                {
                    File.Delete(filename);
                }
                catch
                {

                }
            }
        }

        #endregion

#if NO
        private void RfidManager_ListTags(object sender, ListTagsEventArgs e)
        {
            string text = e.Result?.Results?.Count.ToString();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (this.number.Text != text)
                    this.number.Text = text;
            }));
        }
#endif
        public static PageBorrow PageBorrow
        {
            get
            {
                if (_pageBorrow == null)
                    _pageBorrow = new PageBorrow();

                return _pageBorrow;
            }
        }

        static PageSetting _pageSetting = null;

        void NavigatePageSetting()
        {
            if (_pageSetting == null)
                _pageSetting = new PageSetting();

            this.NavigationService.Navigate(_pageSetting);
        }

        static PageBorrow _pageBorrow = null;

        void NavigatePageBorrow(string buttons)
        {
            if (_pageBorrow == null)
                _pageBorrow = new PageBorrow();

            _pageBorrow.ActionButtons = buttons;
            this.NavigationService.Navigate(_pageBorrow);
        }

        public static PageShelf PageShelf
        {
            get
            {
                return _pageShelf;
            }
        }
        static PageShelf _pageShelf = null;

        void NavigatePageShelf(string mode)
        {
            if (_pageShelf == null)
            {
                _pageShelf = new PageShelf(mode);

                // 2020/9/17
                var pos = WpfClientInfo.Config.Get("pageShelf", "splitterPosition", null);
                if (string.IsNullOrEmpty(pos) == false)
                {
                    try
                    {
                        _pageShelf.SplitterPosition = pos;
                    }
                    catch (Exception ex)
                    {
                        // 2021/1/15
                        WpfClientInfo.WriteErrorLog($"settings.xml 中 pageShelf/splitterPosition 值 '{pos}' 格式不正确({ExceptionUtil.GetDebugText(ex)})。已被忽略");
                    }
                }
            }
            else
                _pageShelf.Mode = mode;

            this.NavigationService.Navigate(_pageShelf);
        }

        public static PageInventory PageInventory
        {
            get
            {
                return _pageInventory;
            }
        }
        static PageInventory _pageInventory = null;

        void NavigatePageInventory()
        {
            if (_pageInventory == null)
                _pageInventory = new PageInventory();

            this.NavigationService.Navigate(_pageInventory);
        }

        private void Button_Borrow_Click(object sender, RoutedEventArgs e)
        {
            NavigatePageBorrow("borrow");

#if NO
            Window mainWindow = Application.Current.MainWindow;
            var page = new PageBorrow();
            // page.Background = Brushes.Red;
            mainWindow.Content = page;
#endif
            // this.NavigationService.Navigate(new PageBorrow("borrow"));
        }

        private void Config_Click(object sender, RoutedEventArgs e)
        {
            //Window cfg_window = new ConfigWindow();
            //cfg_window.ShowDialog();

            /*
            // 测试用
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }
            */

            NavigatePageSetting();
            //this.NavigationService.Navigate(new PageSetting());
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            NavigatePageBorrow("return");

            // this.NavigationService.Navigate(new PageBorrow("return"));
        }

        private void RenewBotton_Click(object sender, RoutedEventArgs e)
        {
            NavigatePageBorrow("renew");

            // this.NavigationService.Navigate(new PageBorrow("renew"));
        }

        private void Error_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new PageError());
        }

        private void Message_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                Clipboard.SetDataObject(this.message.Text, true);

            /*
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            // 测试功能
                            MessageDocument doc = new MessageDocument();
                            DateTime now = DateTime.Now;
                            doc.Add(new Operator { PatronName = "姓名" }, now, "borrow", "succeed", "", "", new Entity { Title = "书名1" });
                            doc.Add(new Operator { PatronName = "姓名" }, now, "borrow", "succeed", "", "", new Entity { Title = "书名2" });
                            doc.Add(new Operator { PatronName = "姓名" }, now, "return", "warning", "这是警告信息", "", new Entity { Title = "书名3" });
                            doc.Add(new Operator { PatronName = "姓名" }, now, "return", "error", "还书出错", "errorCode", new Entity { Title = "书名4" });

                            ProgressWindow progress = null;

                            App.Invoke(new Action(() =>
                            {

                                progress = new ProgressWindow();
                                // progress.MessageText = "正在处理，请稍候 ...";
                                progress.MessageDocument = doc.BuildDocument(
                                    MessageDocument.BaseFontSize,
                                    "",
                                    out string speak);
                                progress.Owner = Application.Current.MainWindow;
                                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                //progress.Closed += Progress_Closed;
                                App.SetSize(progress, "wide");

                                //progress.Width = Math.Min(700, this.ActualWidth);
                                //progress.Height = Math.Min(500, this.ActualHeight);
                                progress.Show();
                                //AddLayer();
                            }));
                        }
            */
        }

        private void RegisterFace_Click(object sender, RoutedEventArgs e)
        {
            NavigatePageBorrow("registerFace,deleteFace");

            // this.NavigationService.Navigate(new PageBorrow("registerFace,deleteFace"));
        }

        private void Shelf_Click(object sender, RoutedEventArgs e)
        {
            // this.NavigationService.Navigate(new PageShelf());
            NavigatePageShelf("");  // 普通使用
        }

        private void bindPatronCard_Click(object sender, RoutedEventArgs e)
        {
            NavigatePageBorrow("bindPatronCard,releasePatronCard");
        }

        public void ShowUpdated()
        {
            App.Invoke(new Action(() =>
            {
                this.message.Background = Brushes.DarkOrange;
            }));
        }

        public void SetLibraryName(string libraryName)
        {
            App.Invoke(new Action(() =>
            {
                this.libraryName.Text = libraryName;
            }));
        }

        public void SetSystemName(string systemName)
        {
            App.Invoke(new Action(() =>
            {
                this.systemName.Text = systemName;
            }));
        }

        // 盘点
        private void inventory_Click(object sender, RoutedEventArgs e)
        {
            NavigatePageInventory();
        }

        #region 不活跃探测

        private DispatcherTimer _activityTimer;
        private Point _inactiveMousePosition = new Point(0, 0);

        void OnInactivity(object sender, EventArgs e)
        {
            // remember mouse position
            _inactiveMousePosition = Mouse.GetPosition(this);

            /*
            // set UI on inactivity
            rectangle.Visibility = Visibility.Hidden;
            */
            // MessageBox.Show("Idle!");

            var nav = (NavigationWindow)App.Current.MainWindow;
            if ((nav.Content == PageMenu.PageBorrow && PageMenu.PageBorrow.IsEmpty())
                || (nav.Content == PageMenu.PageShelf && PageMenu.PageShelf.IsPatronEmpty() == true && ShelfData.OpeningDoorCount == 0 && DoorStateTask.CopyList().Count == 0)
                || (nav.Content == PageMenu._pageSetting && PageMenu._pageSetting.GetDialogCount() == 0)
                || nav.Content.GetType() == typeof(PageError))
            {
                nav.Navigate(PageMenu.MenuPage);
            }
        }

        // 返回到菜单页面
        public static void RetunMenuPage()
        {
            var nav = (NavigationWindow)App.Current.MainWindow;
            nav.Navigate(PageMenu.MenuPage);
        }

        void OnActivity(object sender, PreProcessInputEventArgs e)
        {
            InputEventArgs inputEventArgs = e.StagingItem.Input;

            if (inputEventArgs is MouseEventArgs || inputEventArgs is KeyboardEventArgs)
            {
                if (e.StagingItem.Input is MouseEventArgs)
                {
                    MouseEventArgs mouseEventArgs = (MouseEventArgs)e.StagingItem.Input;

                    // no button is pressed and the position is still the same as the application became inactive
                    if (mouseEventArgs.LeftButton == MouseButtonState.Released &&
                        mouseEventArgs.RightButton == MouseButtonState.Released &&
                        mouseEventArgs.MiddleButton == MouseButtonState.Released &&
                        mouseEventArgs.XButton1 == MouseButtonState.Released &&
                        mouseEventArgs.XButton2 == MouseButtonState.Released &&
                        _inactiveMousePosition == mouseEventArgs.GetPosition(this))
                        return;
                }

                /*
                // set UI on activity
                rectangle.Visibility = Visibility.Visible;
                */

                _activityTimer.Stop();
                _activityTimer.Start();
            }
        }

        public void ResetActivityTimer()
        {
            _activityTimer.Stop();
            _activityTimer.Start();
        }

        #endregion

#if NO
        // https://blog.csdn.net/m0_37682004/article/details/82314055
        Task SetWallPaper()
        {
            return Task.Run(() =>
            {
                WebClient client = new WebClient();
                byte[] bytes = client.DownloadData("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN");
                dynamic obj = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes));
                string url = obj.images[0].url;
                url = $"https://cn.bing.com{url}";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(url, UriKind.Absolute);
                    bitmap.EndInit();
                    // backImage.ImageSource = bitmap;

                    this.Background = new ImageBrush(bitmap);

                    Thread.Sleep(1000);
                    this.mask.Background = new SolidColorBrush(Colors.Transparent);
                }));
            });
        }
#endif

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
