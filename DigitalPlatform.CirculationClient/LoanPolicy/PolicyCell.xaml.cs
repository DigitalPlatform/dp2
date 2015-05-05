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
    /// 普通权限单元
    /// PolicyCell.xaml 的交互逻辑
    /// </summary>
    public partial class PolicyCell : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // public event TextChangedEventHandler TextChanged = null;

        public PolicyCell()
        {
            InitializeComponent();
            this.DataContext = this;
        }

#if NO
                public static string[] two_d_paramnames = new string[] { 
                    "可借册数",
                    "借期" ,
                    "超期违约金因子",
                    "丢失违约金因子",
            };
#endif

        public void SetValue(string strName, string strValue)
        {
            if (strName == "可借册数")
            {
                MaxBorrowItems = strValue;
                // this.textBox_maxBorrowItems.Text = strValue;
            }
            else if (strName == "借期")
            {
                BorrowPeriod = strValue;
                // this.textBox_borrowPeriod.Text = strValue;
            }
            else if (strName == "超期违约金因子")
            {
                OverdueRatio = strValue;
                // this.textBox_overdueRatio.Text = strValue;
            }
            else if (strName == "丢失违约金因子")
            {
                LostRatio = strValue;
                // this.textBox_lostRatio.Text = strValue;
            }
            else
                throw new Exception("无法识别的 strName 值 '" + strName + "'");
        }

        public string GetValue(string strName)
        {
            if (strName == "可借册数")
            {
                return MaxBorrowItems;
                // return this.textBox_maxBorrowItems.Text;
            }
            else if (strName == "借期")
            {
                return BorrowPeriod;
                // return this.textBox_borrowPeriod.Text;
            }
            else if (strName == "超期违约金因子")
            {
                return OverdueRatio;
                // return this.textBox_overdueRatio.Text;
            }
            else if (strName == "丢失违约金因子")
            {
                return LostRatio;
                // return this.textBox_lostRatio.Text;
            }
            else
                throw new Exception("无法识别的 strName 值 '" + strName + "'");
        }

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

        string _borrowPeriod = "";

        public string BorrowPeriod
        {
            get
            {
                return _borrowPeriod;
            }
            set
            {
                _borrowPeriod = value;
                OnPropertyChanged("BorrowPeriod");
            }
        }

        string _overdueRatio = "";

        public string OverdueRatio
        {
            get
            {
                return _overdueRatio;
            }
            set
            {
                _overdueRatio = value;
                OnPropertyChanged("OverdueRatio");
            }
        }

        string _lostRatio = "";

        public string LostRatio
        {
            get
            {
                return _lostRatio;
            }
            set
            {
                _lostRatio = value;
                OnPropertyChanged("LostRatio");
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

#if NO
        private void textBox_maxBorrowItems_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }

        private void textBox_borrowPeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }

        private void textBox_overdueRatio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }

        private void textBox_lostRatio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
            {
                this.TextChanged(sender, e);
            }
        }
#endif

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
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
