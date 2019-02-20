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
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace dp2SSL
{
    /// <summary>
    /// ConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();

            ConfigParams param = new ConfigParams(WpfClientInfo.Config);
            _propertyGrid.SelectedObject = param;
        }

        public void Initial()
        {
        }

        private void UrlDefault_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            i++;

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
            else
                throw new Exception($"未知的属性名{item.PropertyName}");
        }
    }
}
