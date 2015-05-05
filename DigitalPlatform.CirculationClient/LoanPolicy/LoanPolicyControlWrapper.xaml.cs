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
    /// LoanPolicyControlWrapper.xaml 的交互逻辑
    /// </summary>
    public partial class LoanPolicyControlWrapper : UserControl, INotifyPropertyChanged
    {
        // public event TextChangedEventHandler TextChanged = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public LoanPolicyControl LoanPolicyControl
        {
            get
            {
                return this.loanPolicyControl1;
            }
        }
        public LoanPolicyControlWrapper()
        {
            InitializeComponent();
        }

        private void loanPolicyControl1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(sender, e);
        }

#if NO
        private void loanPolicyControl1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.TextChanged != null)
                this.TextChanged(sender, e);
        }
#endif
    }
}
