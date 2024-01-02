using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
// using System.Windows.Shapes;
using System.Diagnostics;
using System.Xml;
using System.IO;

// using Xceed.Wpf.Toolkit;
using WindowsInput;

using static dp2SSL.LibraryChannelUtil;
using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// SelectPatronWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectPatronWindow : Window
    {
        PatronCollection _patrons = null;
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public string PressedButton { get; set; }

        public SelectPatronWindow()
        {
            InitializeComponent();

            Loaded += SelectPatronWindow_Loaded;
            Unloaded += SelectPatronWindow_Unloaded;

            this.keyboard.KeyPressed += Keyboard_KeyPressed;
        }

        private void Keyboard_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == '\r')
            {
                OnEnter();
                return;
            }

            // this.password.Password = this.keyboard.Text;

            this.password.Focus();

            InputSimulator sim = new InputSimulator();
            if (e.Key == '\b')
                sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.BACK);
            else
                sim.Keyboard.TextEntry(e.Key);
        }

        void OnEnter()
        {
            // LoginButton_Click(this.loginButton, new RoutedEventArgs());
            // 验证密码，然后报错，或者关闭对话框
            string text = this.password.Password;
            if (text.Length < 4)
            {
                MessageBox.Show(this, "输入的密码长度必须大于等于 4 字符");
                return;
            }
            var result = SelectPatronByPassword(text);
            if (result.Value == 1)
            {
                selectButton_Click(this, new RoutedEventArgs());
                return;
            }
            MessageBox.Show(this, result.ErrorInfo);
            return;
        }

        private void SelectPatronWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancel.Cancel();

            ReleaseSource();
        }

        bool _loadFinish = false;

        private void SelectPatronWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.password.Focus();

            this.InitialVisible();

            _ = Task.Factory.StartNew(async () =>
            {
                using (CancellationTokenSource cancel = CancellationTokenSource.CreateLinkedTokenSource(_cancel.Token, App.CancelToken))
                {
                    var result = await FillPatronCollectionDetailAsync(
        _patrons,
        cancel.Token);
                    // 等读者记录装载完成后，再使能密码验证
                    _loadFinish = true;
                }
            },
App.CancelToken,
TaskCreationOptions.LongRunning,
TaskScheduler.Default);
        }

        public void SetSource(PatronCollection patrons)
        {
            _patrons = patrons;
            this.listView.ItemsSource = patrons;
            /*
            foreach (var patron in _patrons)
            {
                patron.PropertyChanged += Patron_PropertyChanged;
            }
            */
        }

#if REMOVED
        private void Patron_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PhotoPath")
            {
                _ = Task.Run(() =>
                {
                    PageBorrow.LoadPhoto(sender as PatronControl, (sender as Patron).PhotoPath);
                });
            }
        }
#endif

        public void ReleaseSource()
        {
            /*
            foreach (var patron in _patrons)
            {
                patron.PropertyChanged -= Patron_PropertyChanged;
            }
            */
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var patron = (Patron)listView.SelectedItem;
            if (patron != null
                && string.IsNullOrEmpty(patron.Barcode) == false)
                this.selectButton.IsEnabled = true;
            else
                this.selectButton.IsEnabled = false;
        }

        public Patron SelectedPatron
        {
            get
            {
                if (this.listView.SelectedItem == null)
                    return null;
                return this.listView.SelectedItem as Patron;
            }
        }


        async Task<NormalResult> FillPatronCollectionDetailAsync(
            PatronCollection patrons,
            CancellationToken token,
            bool force = false)
        {
            var cover_items = new List<Patron>();
            foreach (var patron in patrons)
            {
                if (token.IsCancellationRequested)
                    break;

                patron.Waiting = true;
                try
                {
                    // 已经填充过了
                    if (patron.PatronName != null
                    && force == false)
                        continue;

                    string pii = patron.PII;
                    if (string.IsNullOrEmpty(pii))
                        pii = patron.UID;

                    if (string.IsNullOrEmpty(pii))
                        continue;

                    // TODO: 改造为 await
                    // return.Value:
                    //      -1  出错
                    //      0   读者记录没有找到
                    //      1   成功
                    GetReaderInfoResult result = null;
                    if (App.Protocol == "sip")
                    {
                        string oi = string.IsNullOrEmpty(patron.OI) ? patron.AOI : patron.OI;

                        // 对指纹、掌纹来源的做特殊处理，保证 SIP 请求中含有 AO 字段
                        if (oi == null && patron.IsFingerprintSource)
                            oi = "";

                        // 2021/6/9
                        if (Patron.IsPQR(pii))
                            oi = "";

                        result = await SipChannelUtil.GetReaderInfoAsync(oi, pii);
                    }
                    else
                        result = await
                            Task<GetReaderInfoResult>.Run(() =>
                            {
                                // 2021/4/2 改为用 oi+pii
                                bool strict = !patron.IsFingerprintSource;
                                // 2021/4/15
                                if (ChargingData.GetBookInstitutionStrict() == false)
                                    strict = false;
                                string oi_pii = patron.GetOiPii(strict); // 严格模式，必须有 OI
                                return LibraryChannelUtil.GetReaderInfo(string.IsNullOrEmpty(oi_pii) ? pii : oi_pii);
                            });

                    if (result.Value != 1)
                    {
                        string error = $"读者 '{pii}': {result.ErrorInfo}";
                        patron.SetError(error);
                        continue;
                    }

                    // SetPatronError("getreaderinfo", "");

                    if (force)
                        patron.PhotoPath = "";
                    // string old_photopath = _patron.PhotoPath;
                    App.Invoke(new Action(() =>
                    {
                        try
                        {
                            patron.MaskDefinition = ShelfData.GetPatronMask();
                            patron.SetPatronXml(result.RecPath, result.ReaderXml, result.Timestamp);
                            // this.patronControl.SetBorrowed(result.ReaderXml);
                        }
                        catch (Exception ex)
                        {
                            patron.SetError(ex.Message);
                            //WpfClientInfo.WriteErrorLog($"SetBorrowed() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                            //SetGlobalError("patron", $"SetBorrowed() 出现异常: {ex.Message}");
                        }
                    }));

                    if (string.IsNullOrEmpty(patron.PhotoPath) == false)
                        cover_items.Add(patron);
                }
                finally
                {
                    patron.Waiting = false;
                }
            }

            // 获取封面图像
            if (cover_items.Count > 0)
            {
                // string cacheDir = CoverImagesDirectory;

                foreach (var patron in cover_items)
                {
                    if (token.IsCancellationRequested)
                        break;

                    string fileName = PatronControl.GetCachePhotoPath(patron.PhotoPath);
                    // Path.Combine(cacheDir, PageBorrow.GetImageFilePath(patron.PhotoPath));

                    if (File.Exists(fileName) == false)
                    {
                        var get_result = await LibraryChannelUtil.GetCoverImageAsync(patron.PhotoPath, fileName);
                        if (get_result.Value == 1)
                            patron.PhotoImageLocalPath = fileName;
                        if (get_result.Value == -1 && get_result.ErrorCode == "System.IO.IOException")
                        {
                            // TODO: 执行缓存清理任务
                            // BeginCleanCoverImagesDirectory(DateTime.Now);
                        }
                    }
                    else
                        patron.PhotoImageLocalPath = fileName;
                }
            }

            return new NormalResult();
        }

#if REMOVED
        public class CoverItem
        {
            public string ObjectPath { get; set; }
            public Patron Patron { get; set; }
        }


        public static string CoverImagesDirectory
        {
            get
            {
                string cacheDir = Path.Combine(WpfClientInfo.UserDir, "coverImages");
                PathUtil.CreateDirIfNeed(cacheDir);
                return cacheDir;
            }
        }
#endif

        // 根据输入的密码选择读者记录
        // 自动根据证条码号、生日、身份证号进行匹配
        NormalResult SelectPatronByPassword(string password)
        {
            // 等待装载完成

            // TODO: 显示一个半透明的文字“请等待 ...”
            this.passwordArea.IsEnabled = false;
            try
            {
                var start_time = DateTime.Now;
                while (_loadFinish == false)
                {
                    _cancel.Token.ThrowIfCancellationRequested();
                    Thread.Sleep(100);
                    // 超过十秒还没有等到装载完成，也只好退出等待循环
                    if (DateTime.Now - start_time > TimeSpan.FromSeconds(10))
                        break;
                }
            }
            finally
            {
                this.passwordArea.IsEnabled = true;
            }

            try
            {
                // this.listView.SelectedItem = null;
                var hits = new List<Patron>();
                foreach (var patron in _patrons)
                {
                    if (string.IsNullOrEmpty(patron.Xml))
                        continue;

                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(patron.Xml);

                    string barcode = DomUtil.GetElementText(dom.DocumentElement,
                        "barcode");
                    string dateOfBirth = DomUtil.GetElementText(dom.DocumentElement,
                        "dateOfBirth");
                    try
                    {
                        dateOfBirth = DateTimeUtil.Rfc1123DateTimeStringToLocal(dateOfBirth, "yyyyMMdd");
                    }
                    catch
                    {

                    }
                    string idCardNumber = DomUtil.GetElementText(dom.DocumentElement,
                        "idCardNumber");

                    if (barcode.EndsWith(password)
                        || dateOfBirth.EndsWith(password)
                        || idCardNumber.EndsWith(password))
                    {
                        hits.Add(patron);
                    }
                }

                if (hits.Count == 0)
                    return new NormalResult
                    {
                        Value = 0,
                        ErrorInfo = "没有匹配的读者"
                    };

                if (hits.Count == 1)
                {
                    this.listView.SelectedItem = hits[0];
                    return new NormalResult { Value = 1 };
                }

                Debug.Assert(hits.Count > 1);
                return new NormalResult
                {
                    Value = hits.Count,
                    ErrorInfo = "匹配多个读者。请使用更精确的密码重新匹配"
                };
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"SelectPatronByPassword() 出现异常: {ExceptionUtil.GetDebugText(ex)}");
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"SelectPatronByPassword() 出现异常(已写入错误日志): {ex.Message}"
                };
            }
        }

        private void selectButton_Click(object sender, RoutedEventArgs e)
        {
            PressedButton = "select";

            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            PressedButton = "cancel";

            this.Close();
        }

        private void listView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var patron = SelectedPatron;
            if (patron == null)
                return;
            if (string.IsNullOrEmpty(patron.Barcode))
                return;
            selectButton_Click(sender, e);
        }

        public string TitleText
        {
            get
            {
                return title.Text;
            }
            set
            {
                title.Text = value;
            }
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnEnter();
                e.Handled = true;
                return;
            }

            /*
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                ToggleListViewVisiblity();
            */
        }

        void ToggleListViewVisiblity()
        {
            /*
            if (this.listView.Visibility == Visibility.Collapsed)
                this.listView.Visibility = Visibility.Visible;
            else
                this.listView.Visibility = Visibility.Collapsed;
            */
            SetLeftVisible(this.listView.Visibility == Visibility.Collapsed);
        }

        void TogglePasswordVisiblity()
        {
            /*
            if (this.passwordArea.Visibility == Visibility.Collapsed)
                this.passwordArea.Visibility = Visibility.Visible;
            else
                this.passwordArea.Visibility = Visibility.Collapsed;
            */
            SetRightVisible(this.passwordArea.Visibility == Visibility.Collapsed);
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.passwordArea.Visibility == Visibility.Collapsed)
                    selectButton_Click(this, new RoutedEventArgs());
                else
                    OnEnter();
                return;
            }

            if (e.Key == Key.LeftCtrl)
            {
                ToggleListViewVisiblity();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.RightCtrl)
            {
                TogglePasswordVisiblity();
                e.Handled = true;
                return;
            }

            if (this.password.IsFocused == false)
            {
                this.password.Focus();

                /*
                var e1 = new System.Windows.Input.KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, e.Key) { RoutedEvent = Keyboard.KeyDownEvent };
                bool b = InputManager.Current.ProcessInput(e1);
                */
                InputSimulator sim = new InputSimulator();
                if (e.Key == Key.Back)
                    sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.BACK);
                else
                    sim.Keyboard.TextEntry((char)e.Key);
                e.Handled = true;
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            this.password.Clear();
        }

        public void SetLeftVisible(bool visible)
        {
            // 不允许两边都隐藏
            if (this.passwordArea.Visibility == Visibility.Collapsed
                && visible == false)
                return;

            this.listView.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            // (this.mainGrid.ColumnDefinitions[0] as ColumnDefinitionExtended).Visible = visible;

            OnVisibleChanged();
        }


        public void SetRightVisible(bool visible)
        {
            // 不允许两边都隐藏
            if (this.listView.Visibility == Visibility.Collapsed
                && visible == false)
                return;

            this.passwordArea.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            (this.mainGrid.ColumnDefinitions[1] as ColumnDefinitionExtended).Visible = visible;
            
            OnVisibleChanged();

            if (visible == false)
                this.listView.Focus();
        }

        public void InitialVisible()
        {
            var left_visible = App.FaceInputMultipleHits.Contains("列表选择");
            SetLeftVisible(left_visible);

            var right_visible = App.FaceInputMultipleHits.Contains("密码筛选");
            SetRightVisible(right_visible);

            /*
            if (left_visible == false && right_visible == true)
            {
                this.mainGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            }
            else if (left_visible == true && right_visible == true)
            {
                this.mainGrid.ColumnDefinitions[1].Width = new GridLength(330);
            }
            */
            OnVisibleChanged();
        }

        void OnVisibleChanged()
        {
            var left_visible = App.FaceInputMultipleHits.Contains("列表选择");

            var right_visible = App.FaceInputMultipleHits.Contains("密码筛选");

            if (left_visible == false && right_visible == true)
            {
                this.mainGrid.ColumnDefinitions[1].Width = GridLength.Auto;
            }
            else if (left_visible == true && right_visible == true)
            {
                this.mainGrid.ColumnDefinitions[1].Width = new GridLength(330);
            }
        }
    }

    public class PatronCollection : ObservableCollection<Patron>
    {

    }
}
