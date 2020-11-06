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

using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// BeginInventoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BeginInventoryWindow : Window
    {
        public BeginInventoryWindow()
        {
            InitializeComponent();
        }

        public string TitleText
        {
            get
            {
                return this.title.Text;
            }
            set
            {
                this.title.Text = value;
            }
        }

        public string Text
        {
            get
            {
                return this.text.Text;
            }
            set
            {
                this.text.Text = value;
            }
        }

        public string BatchNo
        {
            get
            {
                return this.batchNo.Text;
            }
            set
            {
                this.batchNo.Text = value;
            }
        }

        public string Location
        {
            get
            {
                return this.location.Text;
            }
            set
            {
                this.location.Text = value;
            }
        }

        // 慢速模式。故意令 UID --> PII 对照表为空，迫使盘点过程从 RFID 标签获取 PII
        public bool SlowMode
        {
            get
            {
                return (bool)this.slowMode.IsChecked;
            }
            set
            {
                this.slowMode.IsChecked = value;
            }
        }

        // 动作模式
        /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
         * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
         * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
         * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
         * */

        public string ActionMode
        {
            get
            {
                List<string> values = new List<string>();

                if (actionSetUID.IsChecked == true)
                    values.Add("setUID");
                if (actionSetCurrentLocation.IsChecked == true)
                    values.Add("setCurrentLocation");
                if (actionSetLocation.IsChecked == true)
                    values.Add("setLocation");
                if (actionVerifyEas.IsChecked == true)
                    values.Add("verifyEAS");

                return StringUtil.MakePathList(values);
            }
            set
            {
                actionSetUID.IsChecked = (StringUtil.IsInList("setUID", value));
                actionSetCurrentLocation.IsChecked = (StringUtil.IsInList("setCurrentLocation", value));
                actionSetLocation.IsChecked = (StringUtil.IsInList("setLocation", value));
                actionVerifyEas.IsChecked = (StringUtil.IsInList("verifyEAS", value));
            }
        }

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.location.Text))
            {
                MessageBox.Show("请选择当前馆藏地");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void checkbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox.IsChecked == true)
            {
                checkbox.FontWeight = FontWeights.Bold;
            }
            else
            {
                checkbox.FontWeight = FontWeights.Normal;
            }
        }
    }
}
