using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Xceed.Wpf.Toolkit.PropertyGrid;

using DigitalPlatform;
using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        ConfigParams _configParams = null;

        public ConfigWindow()
        {
            InitializeComponent();

            _configParams = new ConfigParams(WpfClientInfo.Config);
            _configParams.LoadData();
            _propertyGrid.SelectedObject = _configParams;
        }

        public void Initial()
        {
        }

        private void UrlDefault_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PropertyItem item = (PropertyItem)button.DataContext;
            if (item.PropertyName == "RfidURL")
            {
                item.Value = "ipc://RfidChannel/RfidServer";
            }
            else if (item.PropertyName == "FingerprintURL")
            {
                item.Value = "ipc://FingerprintChannel/FingerprintServer";
            }
            else if (item.PropertyName == "FaceURL")
            {
                item.Value = "ipc://FaceChannel/FaceServer";
            }
            else
                throw new Exception($"未知的属性名{item.PropertyName}");
        }

        private void UrlClear_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PropertyItem item = (PropertyItem)button.DataContext;
            item.Value = "";
        }

        // “确定”
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            string error = _configParams.Validate();
            if (string.IsNullOrEmpty(error) == false)
            {
                MessageBox.Show(error);
                return;
            }
            _configParams.SaveData();
            this.Close();
        }

        private void openKeyboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("osk");
            }
            catch (Exception ex)
            {
                WpfClientInfo.WriteErrorLog($"打开触摸键盘时出现异常: {ExceptionUtil.GetDebugText(ex)}");
            }
        }

        // “取消”
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void urlDefault2_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            PropertyItem item = (PropertyItem)button.DataContext;
            if (item.PropertyName == "FingerprintURL")
            {
                item.Value = "ipc://PalmChannel/PalmServer";
            }
            else
                throw new Exception($"未知的属性名{item.PropertyName}");
        }
    }
}
