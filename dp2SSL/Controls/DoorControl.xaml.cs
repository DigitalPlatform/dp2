using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// DoorControl.xaml 的交互逻辑
    /// </summary>
    public partial class DoorControl : UserControl
    {
        // 开门事件
        public event OpenDoorEventHandler OpenDoor = null;

        // List<Door> _doors = new List<Door>();

        public DoorControl()
        {
            InitializeComponent();

            /*
            try
            {
                InitializeButtons();

                SetSize(new Size(this.ActualWidth - (this.Padding.Left + this.Padding.Right), 
                    this.ActualHeight - (this.Padding.Top + this.Padding.Bottom)));
            }
            catch (Exception ex)
            {
                App.CurrentApp.SetError("cfg", $"初始化 DoorControl 出现异常:{ex.Message}");
            }
            */

            Loaded += DoorControl_Loaded;
            SizeChanged += DoorControl_SizeChanged;
            // LayoutUpdated += DoorControl_LayoutUpdated;
        }

        private void DoorControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size size = new Size(e.NewSize.Width - (this.Padding.Left + this.Padding.Right),
                e.NewSize.Height - (this.Padding.Top + this.Padding.Bottom));
            SetSize(size);
        }

        private void DoorControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        void SetSize(Size size)
        {
            double row_height = size.Height / Math.Max(1, this.grid.RowDefinitions.Count);
            foreach (var row in this.grid.RowDefinitions)
            {
                row.Height = new GridLength(row_height);
            }
            double column_width = size.Width / Math.Max(1, this.grid.ColumnDefinitions.Count);
            foreach (var column in this.grid.ColumnDefinitions)
            {
                column.Width = new GridLength(column_width);
            }
        }


        /*
        List<Door> FindDoors(string readerName, string antenna)
        {
            List<Door> results = new List<Door>();
            foreach (var door in _doors)
            {
                if (door.Antenna.ToString() == antenna
                    && IsEqual(door.ReaderName, readerName))
                    results.Add(door);
            }
            return results;
        }
        */

        public void InitializeButtons(XmlDocument cfg_dom,
            List<DoorItem> items)
        {
            /*
            string cfg_filename = App.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);
            */

            XmlNodeList shelfs = cfg_dom.DocumentElement.SelectNodes("shelf");

            this.grid.ColumnDefinitions.Clear();
            this.grid.RowDefinitions.Clear();

            // 获得一个 shelf 元素下 door 数量的最多那个
            int max_doors = 0;
            foreach (XmlElement shelf in shelfs)
            {
                int current = shelf.SelectNodes("door").Count;
                if (current > max_doors)
                    max_doors = current;
            }

            // int shelf_width = total_width / Math.Max(1, shelfs.Count);
            // int level_height = 100;
            bool rowDefinitionCreated = false;
            // 初始化 Definitions
            foreach (XmlElement shelf in shelfs)
            {
                this.grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                // TODO: 这里要用具有最多 door 元素的 shelf 元素来获得数字
                if (rowDefinitionCreated == false)
                {
                    /*
                    XmlNodeList doors = shelf.SelectNodes("door");
                    // level_height = total_height / Math.Max(1, doors.Count);
                    foreach (XmlElement door in doors)
                    {
                        this.grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
                    }
                    */
                    for (int i = 0; i < max_doors; i++)
                    {
                        this.grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
                    }

                    rowDefinitionCreated = true;
                }
            }

            // 填充 Buttons
            int index = 0;
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
                        // Content = door_name,
                    };

                    /*
                    var block = BuildTextBlock();
                    SetBlockText(block, door_name, "0", null, null);
                    */

                    var template = this.Resources["ButtonTemplate"];

                    // button.Children.Add(block);
                    button.SetValue(Button.TemplateProperty, template);
                    // button.SetValue(Grid.prop.ContentProperty, block);
                    button.SetValue(Grid.RowProperty, row);
                    button.SetValue(Grid.ColumnProperty, column);
                    button.Click += Button_Click;

                    /*
                    ParseLockString(door.GetAttribute("lock"), out string lockName, out int lockIndex);
                    ParseLockString(door.GetAttribute("antenna"), out string readerName, out int antenna);

                    var tag = new Door
                    {
                        Name = door_name,
                        LockName = lockName,
                        LockIndex = lockIndex,
                        ReaderName = readerName,
                        Antenna = antenna,
                        Button = button
                    };
                    */

                    button.DataContext = items[index++];

                    /*
                    _doors.Add(tag);

                    button.Tag = tag;
                    */
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

            InitialSize();
        }

        public void InitialSize()
        {
            if (this.Visibility != Visibility.Visible)
                return;

            Debug.Assert(this.Visibility == Visibility.Visible, "");

            double w1 = this.ActualWidth;
            double w2 = this.Width;
            SetSize(new Size(this.ActualWidth - (this.Padding.Left + this.Padding.Right),
this.ActualHeight - (this.Padding.Top + this.Padding.Bottom)));
        }

        /*
        class TextPart
        {
            public Run Name { get; set; }
            public Run Count { get; set; }
            public Run Add { get; set; }
            public Run Remove { get; set; }
        }

        static TextBlock BuildTextBlock()
        {
            TextBlock block = new TextBlock();
            Run name = new Run();
            block.Inlines.Add(name);

            block.Inlines.Add("\r\n");

            Run count = new Run();
            count.FontSize = 20;
            block.Inlines.Add(count);

            Run add = new Run();
            add.FontSize = 30;
            block.Inlines.Add(add);

            Run remove = new Run();
            remove.FontSize = 30;
            block.Inlines.Add(remove);

            block.Tag = new TextPart
            {
                Name = name,
                Count = count,
                Add = add,
                Remove = remove
            };
            return block;
        }

        static void SetBlockText(TextBlock block,
            string name,
            string count,
            string add,
            string remove)
        {
            Debug.Assert(block != null, "");

            TextPart part = block.Tag as TextPart;
            if (name != null)
                part.Name.Text = name;
            if (count != null)
                part.Count.Text = count;
            if (add != null)
            {
                if (add == "0")
                    part.Add.Text = "";
                else
                    part.Add.Text = "+" + add;
            }
            if (remove != null)
            {
                if (remove == "0")
                    part.Remove.Text = "";
                else
                    part.Remove.Text = "-" + remove;
            }
        }
        */




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            // MessageBox.Show(button.Name);
            OpenDoor?.Invoke(sender, new OpenDoorEventArgs { Door = button.DataContext as DoorItem });
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock textblock = (TextBlock)sender;
            DoorItem door = textblock.DataContext as DoorItem;

            OpenDoor?.Invoke(sender, new OpenDoorEventArgs
            {
                ButtonName = textblock.Name,
                Door = textblock.DataContext as DoorItem
            });

            e.Handled = true;
            /*
            Button button = textblock.TemplatedParent as Button;
            DoorItem door = button.DataContext as DoorItem;
            */
        }

        private void All_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            DoorItem door = button.DataContext as DoorItem;

            OpenDoor?.Invoke(sender, new OpenDoorEventArgs
            {
                ButtonName = "count",
                Door = button.DataContext as DoorItem
            });

            e.Handled = true;
        }
    }

    public delegate void OpenDoorEventHandler(object sender,
OpenDoorEventArgs e);

    /// <summary>
    /// 开门事件的参数
    /// </summary>
    public class OpenDoorEventArgs : EventArgs
    {
        /*
        public string Name { get; set; }
        public string Lock { get; set; }    // 门锁。形如 *:0
        public string Antenna { get; set; } // 读卡器天线。形如 RD242:0

        public string State { get; set; }
        */
        public DoorItem Door { get; set; }

        // 触发位置。空/count/add/remove/error
        public string ButtonName { get; set; }
    }

#if NO
    public class Door : INotifyPropertyChanged
    {
        internal void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    // Debug.WriteLine($"PII='{value}'");

                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private int _count;

        // 现有册数
        public int Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    // Debug.WriteLine($"PII='{value}'");

                    _count = value;
                    OnPropertyChanged("Count");
                }
            }
        }

        private int _add;

        // 增加的册数
        public int Add
        {
            get => _add;
            set
            {
                if (_add != value)
                {
                    _add = value;
                    OnPropertyChanged("Add");
                }
            }
        }

        private int _remove;

        // 减少的册数
        public int Remove
        {
            get => _remove;
            set
            {
                if (_remove != value)
                {
                    _remove = value;
                    OnPropertyChanged("Remove");
                }
            }
        }

        public string LockName { get; set; }
        public int LockIndex { get; set; }
        public string ReaderName { get; set; }
        public int Antenna { get; set; }

        public string State { get; set; }

        public Button Button { get; set; }
    }
#endif
    public class LockChanged
    {
        public string LockName { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
    }
}
