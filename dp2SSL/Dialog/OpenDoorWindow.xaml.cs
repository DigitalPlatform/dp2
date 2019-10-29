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
using System.Xml;

namespace dp2SSL
{
    /// <summary>
    /// OpenDoorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OpenDoorWindow : Window
    {
        public OpenDoorWindow()
        {
            InitializeComponent();

            Loaded += OpenDoorWindow_Loaded;
        }

        private void OpenDoorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeButtons((int)this.Width, (int)this.Height);
        }

        void InitializeButtons(
            int total_width,
            int total_height)
        {
            string cfg_filename = ShelfData.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);

            XmlNodeList shelfs = cfg_dom.DocumentElement.SelectNodes("shelf");
            int shelf_width = total_width / Math.Max(1, shelfs.Count);
            int level_height = 100;
            // 初始化 Definitions
            foreach (XmlElement shelf in shelfs)
            {
                this.grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(shelf_width) });

                XmlNodeList doors = shelf.SelectNodes("door");
                level_height = total_height / Math.Max(1, doors.Count);
                foreach (XmlElement door in doors)
                {
                    this.grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(level_height) });
                }

                break;
            }

            // 填充 Buttons
            int column = 0;
            foreach (XmlElement shelf in shelfs)
            {
                XmlNodeList doors = shelf.SelectNodes("door");
                int row = 0;
                foreach (XmlElement door in doors)
                {
                    string door_name = door.GetAttribute("name");

                    Button button = new Button
                    {
                        Name = $"button_{column}_{row}",
                        // Height = level_height,
                        Content = door_name,
                    };
                    button.SetValue(Grid.RowProperty, row);
                    button.SetValue(Grid.ColumnProperty, column);
                    this.grid.Children.Add(button);
                    row++;
                }

                column++;
            }

            /*
            this.grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            this.grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Button button = new Button
            {
                Name = "button1",
                Height = 100,
                Content = "1",
            };
            button.SetValue(Grid.RowProperty, 0);
            //button.SetValue(Grid.ColumnProperty, 0);
            this.grid.Children.Add(button);
            */
        }
    }
}
