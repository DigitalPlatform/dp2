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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 读者权限单元
    /// PatronPolicyCell.xaml 的交互逻辑
    /// </summary>
    public partial class PatronPolicyCell : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // public event TextChangedEventHandler TextChanged = null;

        public PatronPolicyCell()
        {
            InitializeComponent();
            this.DataContext = this;
        }

#if NO
        public static string[] reader_d_paramnames = new string[] { 
                    "可借总册数",
                    "可预约册数", 
                    "以停代金因子",
                    "工作日历名",
            };
#endif

        public void SetValue(string strName, string strValue)
        {
            if (strName == "可借总册数")
            {
                MaxBorrowItems = strValue;
                // this.textBox_maxBorrowItems.Text = strValue;
            }
            else if (strName == "可预约册数")
            {
                MaxReserveItems = strValue;
                // this.textBox_maxReserveItems.Text = strValue;
            }
            else if (strName == "以停代金因子")
            {
                StopRatio = strValue;
                // this.textBox_stopRatio.Text = strValue;
            }
            else if (strName == "工作日历名")
            {
                CalendarName = strValue;
                // this.comboBox_calendar.Text = strValue;
            }
            else
                throw new Exception("无法识别的 strName 值 '" + strName + "'");
        }

        public string GetValue(string strName)
        {
            if (strName == "可借总册数")
            {
                return MaxBorrowItems;
                // return this.textBox_maxBorrowItems.Text;
            }
            else if (strName == "可预约册数")
            {
                return MaxReserveItems;
                // return this.textBox_maxReserveItems.Text;
            }
            else if (strName == "以停代金因子")
            {
                return StopRatio;
                // return this.textBox_stopRatio.Text;
            }
            else if (strName == "工作日历名")
            {
                return CalendarName;
                // return this.comboBox_calendar.Text;
            }
            else
                throw new Exception("无法识别的 strName 值 '" + strName + "'");
        }

#if NO
        private void textBox_maxBorrowItems_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }

        private void textBox_maxReserveItems_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }

        private void textBox_stopRatio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }

        private void comboBox_calendar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }
#endif

        string _maxBorrowItems = "";

        public string MaxBorrowItems
        {
            get
            {
                return _maxBorrowItems;
            }
            set
            {
                _maxBorrowItems = value;
                OnPropertyChanged("MaxBorrowItems");
            }
        }


        string _maxReserveItems = "";

        public string MaxReserveItems
        {
            get
            {
                return _maxReserveItems;
            }
            set
            {
                _maxReserveItems = value;
                OnPropertyChanged("MaxReserveItems");
            }
        }

        string _stopRatio = "";

        public string StopRatio
        {
            get
            {
                return _stopRatio;
            }
            set
            {
                _stopRatio = value;
                OnPropertyChanged("StopRatio");
            }
        }

        string _calendarName = "";

        public string CalendarName
        {
            get
            {
                return _calendarName;
            }
            set
            {
                _calendarName = value;
                OnPropertyChanged("CalendarName");
            }
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


        List<string> _carlendarList = new List<string>();

        public List<string> CalendarList
        {
            get
            {
                return this._carlendarList;
            }
            set
            {
                this._carlendarList = value;
                OnPropertyChanged("CalendarList");
            }
        }

        string _commentText = "";

        public string CommentText
        {
            get
            {
                return _commentText;
            }
            set
            {
                _commentText = value;
                OnPropertyChanged("CommentText");
            }
        }

        private void comboBox_calendar_DropDownOpened(object sender, EventArgs e)
        {
#if NO
            if (this.CalendarList.Count > 0)
                return;

            List<string> list = new List<string>();
            list.Add("test1");
            list.Add("test2");

            this.CalendarList = list;
#endif
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            _comment.Visibility = System.Windows.Visibility.Visible;

        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            _comment.Visibility = System.Windows.Visibility.Collapsed;

        }
    }
}
