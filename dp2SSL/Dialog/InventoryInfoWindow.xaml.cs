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

namespace dp2SSL
{
    /// <summary>
    /// InventoryInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InventoryInfoWindow : Window
    {
        public InventoryInfoWindow()
        {
            InitializeComponent();

            this.Closing += InventoryInfoWindow_Closing;
        }

        private void InventoryInfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public string TotalCount
        {
            get
            {
                return totalCount.Text;
            }
            set
            {
                totalCount.Text = value;
            }
        }

        public string SucceedCount
        {
            get
            {
                return succeedCount.Text;
            }
            set
            {
                succeedCount.Text = value;
            }
        }

        public string ErrorCount
        {
            get
            {
                return this.errorCount.Text;
            }
            set
            {
                this.errorCount.Text = value;
            }
        }

        public string ShelfCount
        {
            get
            {
                return this.shelfCount.Text;
            }
            set
            {
                this.shelfCount.Text = value;
            }
        }
    }
}
