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
            SetBackgroundImage();
        }

        // 计算背景图象 Uniform 方式显示的 x y 偏移
        Point GetOffset(Size size, out Size picture_size)
        {
            double x = 0;
            double y = 0;

            picture_size = size;

            if (_canvas_height == 0 || _canvas_width == 0)
                return new Point(0, 0);

            // 按照横向放下，计算图片高度
            double ratio = size.Width / _canvas_width;
            double test_height = _canvas_height * ratio;
            if (test_height > size.Height)
            {
                // 竖向放不下。改为按照竖向放下，来计算图片宽度
                ratio = size.Height / _canvas_height;
                double test_width = _canvas_width * ratio;
                x = (size.Width - test_width) / 2;
                Debug.Assert(x >= 0, "");
                picture_size.Width = test_width;
            }
            else
            {
                y = (size.Height - test_height) / 2;
                Debug.Assert(y >= 0, "");
                picture_size.Height = test_height;
            }

            return new Point(x, y);
        }

        void SetSize(Size size)
        {
            // 计算居中的 x y 偏移
            Point offset = GetOffset(size, out Size picture_size);

            // 遍历 Grid 对象
            foreach (Grid grid in this.canvas.Children)
            {
                if (grid == null)
                    continue;
                GroupPosition gp = grid.Tag as GroupPosition;
                if (gp == null)
                    continue;

                grid.SetValue(Canvas.LeftProperty, offset.X + (picture_size.Width * gp.Left));
                grid.SetValue(Canvas.TopProperty, offset.Y + (picture_size.Height * gp.Top));
                grid.Width = picture_size.Width * gp.Width;
                grid.Height = picture_size.Height * gp.Height;
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

        class GroupPosition
        {
            public double Left { get; set; }    // 相对于图片宽度的百分比
            public double Top { get; set; }     // 相对于图片高度的百分比
            public double Width { get; set; }   // 相对于图片宽度的百分比
            public double Height { get; set; }  // 相对于图片宽度的百分比
        }

        // 根据一个 group 元素定义，创建一个 Grid 控件
        Grid CreateGroupControls(XmlElement group, List<DoorItem> items, ref int index)
        {
            Grid grid = new Grid();

            //grid.SetValue(Canvas.LeftProperty, 0);
            //grid.SetValue(Canvas.TopProperty, 0);

            int row = 0;
            XmlNodeList doors = group.SelectNodes("door");
            foreach (XmlElement door in doors)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength((double)1, GridUnitType.Star) });

                string door_name = door.GetAttribute("name");

                Button button = new Button
                {
                    // Name = $"button_{column}_{row}",
                };

                var template = this.Resources["ButtonTemplate"];

                // button.Children.Add(block);
                button.SetValue(Button.TemplateProperty, template);
                // button.SetValue(Grid.prop.ContentProperty, block);
                button.SetValue(Grid.RowProperty, row);
                button.SetValue(Grid.ColumnProperty, 0);
                button.Click += Button_Click;

                button.DataContext = items[index++];

                grid.Children.Add(button);
                row++;
            }

            return grid;
        }

        static double GetDouble(XmlElement element, string attr, double default_value)
        {
            if (element.HasAttribute(attr) == false)
                return default_value;
            string s = element.GetAttribute(attr);
            if (double.TryParse(s, out double value) == false)
                throw new Exception($"属性值 {attr} 定义错误，应为数字({element.OuterXml})");
            return value;
        }

        static string[] all_attrs = new string[] { "left", "top", "width", "height" };
        static string[] two_attrs = new string[] { "width", "height" };

        static void CheckAttributes(XmlElement element, string [] attrs)
        {
            int count = 0;
            foreach (string attr in attrs)
            {
                if (element.HasAttribute(attr) == true)
                    count++;
            }
            if (count == 0)
                return;
            if (count != attrs.Length)
                throw new Exception($"元素 {element.OuterXml} 中属性 {string.Join(",", attrs)} 必须同时都定义，或者都不定义");
        }

        double _canvas_width = 0;
        double _canvas_height = 0;

        public void InitializeButtons(XmlDocument cfg_dom,
            List<DoorItem> items)
        {
            XmlElement root = cfg_dom.DocumentElement;
            CheckAttributes(root, two_attrs);

            double canvas_width = GetDouble(root, "width", 0);
            double canvas_height = GetDouble(root, "height", 0);

            XmlNodeList shelfs = cfg_dom.DocumentElement.SelectNodes("shelf");

            bool undefined = false;
            int index = 0;
            foreach (XmlElement shelf in shelfs)
            {
                CheckAttributes(shelf, all_attrs);

                GroupPosition gp = new GroupPosition();
                gp.Left = GetDouble(shelf, "left", -1);
                gp.Top = GetDouble(shelf, "top", -1);
                gp.Width = GetDouble(shelf, "width", -1);
                gp.Height = GetDouble(shelf, "height", -1);

                if (gp.Left == -1 || gp.Top == -1 || gp.Width == -1 || gp.Height == -1)
                    undefined = true;

                Grid grid = CreateGroupControls(shelf, items, ref index);
                grid.Tag = gp;

                this.canvas.Children.Add(grid);
            }

            // 对 -1 进行调整
            if (undefined)
            {
                double x = 0;
                foreach (Grid grid in this.canvas.Children)
                {
                    GroupPosition gp = grid.Tag as GroupPosition;
                    if (gp.Left == -1)
                    {
                        gp.Left = x;
                        gp.Top = 0;
                        gp.Width = 10;
                        gp.Height = 20;

                        x += 10;
                    }
                }
                canvas_width = x;
                canvas_height = 20;
            }

            // 变换为比率
            foreach (Grid grid in this.canvas.Children)
            {
                GroupPosition gp = grid.Tag as GroupPosition;
                gp.Left /= canvas_width;
                gp.Top /= canvas_height;
                gp.Width /= canvas_width;
                gp.Height /= canvas_height;
            }

            // 记忆
            _canvas_width = canvas_width;
            _canvas_height = canvas_height;

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

        void SetBackgroundImage()
        {
            // shelf.xml 中 root 元素的 backImageFile 属性
            string backImageFile = ShelfData.ShelfCfgDom.DocumentElement.GetAttribute("backImageFile");
            if (string.IsNullOrEmpty(backImageFile))
                return;

            string filename = System.IO.Path.Combine(WpfClientInfo.UserDir, backImageFile);

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filename, UriKind.Absolute);
                bitmap.EndInit();

                var brush = new ImageBrush(bitmap);
                brush.Stretch = Stretch.Uniform;
                this.Background = brush;
            }
            catch(Exception ex)
            {
                // TODO: 用一个报错文字图片设定为背景?
            }
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
