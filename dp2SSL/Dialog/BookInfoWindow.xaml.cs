using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace dp2SSL
{
    /// <summary>
    /// BookInfoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BookInfoWindow : Window
    {
        CancellationTokenSource _cancel = new CancellationTokenSource();

        public BookInfoWindow()
        {
            InitializeComponent();

            Unloaded += BookInfoWindow_Unloaded;
        }

        ~BookInfoWindow()
        {
            _cancel.Dispose();
        }

        private void BookInfoWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancel.Cancel();
        }

        /*
        public void SetBooks(List<Entity> entities)
        {
            EntityCollection collection = new EntityCollection();
            foreach(var entity in entities)
            {
                // TODO: 是否克隆 Entity?
                entity.Container = collection;
                collection.Add(entity);
            }
            books.SetSource(collection);
        }
        */

        public void SetBooks(EntityCollection collection)
        {
            books.SetSource(collection);

            // 后台处理刷新
            var task = ShelfData.FillBookFields(collection, _cancel.Token, false);
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



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
