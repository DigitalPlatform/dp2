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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

using DigitalPlatform.WPF;

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

        static double _length = 0.8;    // 动画持续时间(秒)
        static double _top = 0.1;   // 达到最亮的时间(秒)
        static double _delay = 0.02; // 下一个门启动动画的延迟时间(秒)

        public void AnimateDoors()
        {
            double start = 0;
            App.Invoke(new Action(() =>
            {
                // 遍历 Grid 对象
                foreach (Button button in this.canvas.Children)
                {
                    if (button == null)
                        continue;

                    {
                        if (button == null)
                            continue;
                        var door = button.DataContext as DoorItem;
                        if (door.State == "open")
                            continue;

                        var borders = GetChildOfType<Border>(button);
                        var border = borders[borders.Count - 1];

                        // https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/how-to-animate-color-by-using-key-frames
                        ColorAnimationUsingKeyFrames colorAnimation
        = new ColorAnimationUsingKeyFrames();
                        colorAnimation.Duration = TimeSpan.FromSeconds(start + _length);

                        Color oldColor = Colors.Black;
                        if (door.CloseBrush is SolidColorBrush)
                            oldColor = DoorItem.FromColor((door.CloseBrush as SolidColorBrush).Color, 255);  // door.CloseBrush;

                        Color brightColor = Colors.White;
                        if (door.OpenBrush is SolidColorBrush)
                            brightColor = DoorItem.FromColor((door.OpenBrush as SolidColorBrush).Color, 255);  // door.CloseBrush;

                        colorAnimation.KeyFrames.Add(
               new LinearColorKeyFrame(
                   brightColor,  // Colors.DarkOrange, // Target value (KeyValue)
                   KeyTime.FromTimeSpan(TimeSpan.FromSeconds(start + _top))) // KeyTime
               );

                        colorAnimation.KeyFrames.Add(
new LinearColorKeyFrame(
oldColor, // Target value (KeyValue)
KeyTime.FromTimeSpan(TimeSpan.FromSeconds(start + _length))) // KeyTime
);
                        //Binding save = BindingOperations.GetBinding(border, Border.BackgroundProperty);

                        // 2020/7/12
                        // SolidColorBrush temp = border.Background as SolidColorBrush;
                        //if (border.Background != null)
                        //    border.Background = border.Background.CloneCurrentValue();  // new SolidColorBrush(temp != null ? temp.Color : Colors.Black);
                        border.Background = new SolidColorBrush(oldColor);

                        colorAnimation.FillBehavior = FillBehavior.Stop;
                        colorAnimation.Completed += (s, e) =>
                        {
                            border.Background = null;
                            // border.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
                            // border.Background = new SolidColorBrush(Colors.Red);
                            // BindingOperations.SetBinding(border, Border.BackgroundProperty, save);
                        };
                        border.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                        start += _delay;
                    }
                }
            }));
        }

        public static List<T> GetChildOfType<T>(DependencyObject depObj)
    where T : DependencyObject
        {
            if (depObj == null) return null;

            List<T> results = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    results.Add(child as T);
                if (child != null)
                    results.AddRange(GetChildOfType<T>(child));
                /*
        var result = (child as T) ?? GetChildOfType<T>(child);
        if (result != null) return result;
                */
            }
            // return null;
            return results;
        }

        void SetSize(Size size)
        {
            // 计算居中的 x y 偏移
            Point offset = GetOffset(size, out Size picture_size);

            // 遍历 ContentControl 对象
            foreach (Button button in this.canvas.Children)
            {
                if (button == null)
                    continue;
                GroupPosition gp = button.Tag as GroupPosition;
                if (gp == null)
                    continue;

                button.SetValue(Canvas.LeftProperty, offset.X + (picture_size.Width * gp.Left));
                button.SetValue(Canvas.TopProperty, offset.Y + (picture_size.Height * gp.Top));
                button.Width = picture_size.Width * gp.Width;
                button.Height = picture_size.Height * gp.Height;
                var door_item = button.DataContext as DoorItem;
                if (door_item != null)
                {
                    door_item.Padding = Multiple(gp.Padding, picture_size.Width, picture_size.Height);
                    door_item.Margin = Multiple(gp.Margin, picture_size.Width, picture_size.Height);
                    door_item.BorderThickness = Multiple(gp.BorderThickness, picture_size.Width, picture_size.Height);
                    door_item.CornerRadius = Multiple(gp.CornerRadius, picture_size.Width, picture_size.Height);
                }
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

            public Thickness Padding { get; set; }  // 内边距
            public Thickness Margin { get; set; }  // 外边距

            public Thickness BorderThickness { get; set; }  // 边框线条宽度
            public CornerRadius CornerRadius { get; set; }  // 边框圆角

            public Brush BorderBrush { get; set; }  // 边框颜色
            public Brush OpenBrush { get; set; }
            public Brush CloseBrush { get; set; }
            public Brush Foreground { get; set; }   // 前景色
            public Brush ErrorForeground { get; set; }  // 错误事项的前景色
            // public DoorItem DoorItem { get; set; }
        }

        // 根据一个 door 元素定义，创建一个 Button 控件
        Button CreateDoorControl(XmlElement door,
            DoorItem door_item)
        {
            string door_name = door.GetAttribute("name");

            Button button = new Button
            {
                // Name = $"button_{column}_{row}",
            };

            var template = this.Resources["ButtonTemplate"];

            // button.Children.Add(block);
            button.SetValue(Button.TemplateProperty, template);

            // button.SetValue(Grid.RowProperty, row);
            button.SetValue(Grid.ColumnProperty, 0);
            button.Click += Button_Click;

            button.DataContext = door_item;

            // this.canvas.Children.Add(button);
            return button;
        }

        // 根据一个 group 元素定义，创建一个 Grid 控件
        Grid CreateGroupControls(XmlElement group,
            List<DoorItem> door_items,
            ref int index)
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

                if (door_items.Count - 1 < index)
                {
                    // throw new Exception($"门 items 个数({door_items.Count})不足");
                    button.DataContext = null;
                }
                else
                    button.DataContext = door_items[index++];

                grid.Children.Add(button);
                row++;
            }

            return grid;
        }

        // 获得 shelf.xml 中 root 元素的 backImageFileOpen 属性定义的文件的路径
        static string GetBackImageFileOpenPath()
        {
            string backImageFile = ShelfData.ShelfCfgDom?.DocumentElement?.GetAttribute("backImageFile");
            if (string.IsNullOrEmpty(backImageFile))
                return null;

            return System.IO.Path.Combine(WpfClientInfo.UserDir, backImageFile);
        }

        public static Brush GetPanelBackground()
        {
            var root = ShelfData.ShelfCfgDom?.DocumentElement;
            if (root.HasAttribute("background"))
                return GetBrush(root,
                    "background",
                    null,   // new SolidColorBrush(Colors.Transparent),
                    null);
            // 尝试寻找下级元素
            var element = root.SelectSingleNode("background");
            if (element == null)
                return null;

            return XamlReader.Parse(element.InnerXml) as Brush;
        }

        // 根据图像文件名，获得当前元素相关的 ImageSource
        delegate ImageSource Delegate_GetPartImage(string filename);


        static System.Windows.Media.Brush GetBrush(XmlElement element,
            string attr,
            System.Windows.Media.Brush default_value,
            Delegate_GetPartImage getPartImage = null)
        {
            if (element.HasAttribute(attr) == false)
                return default_value;

            string s = element.GetAttribute(attr);

            if (s != null && s.StartsWith("image:"))
            {
                string filename = s.Substring("image:".Length);
                string filepath = System.IO.Path.Combine(WpfClientInfo.UserDir, filename);
                var brush = new ImageBrush(GetBitmapImage(filepath));
                brush.Stretch = Stretch.Fill;
                return brush;
            }

            if (s != null && s.StartsWith("imageMap:"))
            {
                string filename = s.Substring("imageMap:".Length);
                var source = getPartImage(filename);

                var brush = new ImageBrush(source);
                brush.Stretch = Stretch.Fill;
                return brush;
            }

            try
            {
                BrushConverter convertor = new BrushConverter();
                System.Windows.Media.Brush value = (System.Windows.Media.Brush)convertor.ConvertFromString(s);
                return value;
            }
            catch (Exception ex)
            {
                throw new Exception($"属性值 {attr} 定义错误({ex.Message}) ({element.OuterXml})");
            }
        }

        static Thickness GetThickness(XmlElement element,
            string attr,
            Thickness default_value)
        {
            ThicknessConverter convertor = new ThicknessConverter();

            if (element.HasAttribute(attr) == false)
                return default_value;
            string s = element.GetAttribute(attr);

            try
            {
                Thickness value = (Thickness)convertor.ConvertFromString(s);
                return value;
            }
            catch (Exception ex)
            {
                throw new Exception($"属性值 {attr} 定义错误({ex.Message})，应为数字，或 n,n,n,n 形态({element.OuterXml})");
            }
        }

        static CornerRadius GetCornerRadius(XmlElement element,
    string attr,
    CornerRadius default_value)
        {
            CornerRadiusConverter convertor = new CornerRadiusConverter();

            if (element.HasAttribute(attr) == false)
                return default_value;
            string s = element.GetAttribute(attr);

            try
            {
                CornerRadius value = (CornerRadius)convertor.ConvertFromString(s);
                return value;
            }
            catch (Exception ex)
            {
                throw new Exception($"属性值 {attr} 定义错误({ex.Message})，应为数字，或 n,n,n,n 形态({element.OuterXml})");
            }
        }


        static CornerRadius Multiple(CornerRadius radius, double width, double height)
        {
            // double topLeft, double topRight, double bottomRight, double bottomLeft
            return new CornerRadius(radius.TopLeft * width,
                radius.TopRight * width,
                radius.BottomRight * width,
                radius.BottomLeft * width);
        }

        static CornerRadius Divide(CornerRadius radius, double width, double height)
        {
            return new CornerRadius(radius.TopLeft / width,
    radius.TopRight / width,
    radius.BottomRight / width,
    radius.BottomLeft / width);
        }

        static Thickness Multiple(Thickness thickness, double width, double height)
        {
            return new Thickness(thickness.Left * width,
                thickness.Top * height,
                thickness.Right * width,
                thickness.Bottom * height);
        }

        static Thickness Divide(Thickness thickness, double width, double height)
        {
            return new Thickness(thickness.Left / width,
                thickness.Top / height,
                thickness.Right / width,
                thickness.Bottom / height);
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

        static void CheckAttributes(XmlElement element, string[] attrs)
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
    List<DoorItem> door_items)
        {
            // 2019/12/22
            this.canvas.Children.Clear();

            // testing 
            // door_items = new List<DoorItem>();

            XmlElement root = cfg_dom.DocumentElement;
            CheckAttributes(root, two_attrs);

            double canvas_width = GetDouble(root, "width", 0);
            double canvas_height = GetDouble(root, "height", 0);

            var doors = cfg_dom.DocumentElement.SelectNodes("//door");

            bool undefined = false;
            int index = 0;
            foreach (XmlElement door in doors)
            {
                CheckAttributes(door, all_attrs);

                GroupPosition gp = GetPosition(door);

                if (gp.Left == -1 || gp.Top == -1 || gp.Width == -1 || gp.Height == -1)
                    undefined = true;

                DoorItem door_item = null;
                if (index < door_items.Count)
                    door_item = door_items[index++];

                var button = CreateDoorControl(door, door_item);
                button.Tag = gp;

                if (door_item != null)
                {
                    door_item.BorderBrush = gp.BorderBrush;
                    // if (gp.OpenBrush is SolidColorBrush)
                    door_item.OpenBrush = gp.OpenBrush; // (gp.OpenBrush as SolidColorBrush).Color;
                                                        // if (gp.CloseBrush is SolidColorBrush)
                    door_item.CloseBrush = gp.CloseBrush;   // (gp.CloseBrush as SolidColorBrush).Color;
                    door_item.BorderThickness = gp.BorderThickness;
                    door_item.CornerRadius = gp.CornerRadius;
                    door_item.Foreground = gp.Foreground;
                    door_item.ErrorForeground = gp.ErrorForeground;

                }

                this.canvas.Children.Add(button);
            }

            // 对 -1 进行调整
            if (undefined)
            {
                double x = 0;
                double y = 0;
                foreach (Button button in this.canvas.Children)
                {
                    GroupPosition gp = button.Tag as GroupPosition;
                    if (gp.Left == -1)
                    {
                        gp.Left = 0;
                        gp.Top = y;
                        gp.Width = 20;
                        gp.Height = 10;

                        y += 10;
                    }
                }
                canvas_width = 20;
                canvas_height = y;
            }

            // 变换为比率
            foreach (Button button in this.canvas.Children)
            {
                GroupPosition gp = button.Tag as GroupPosition;
                gp.Left /= canvas_width;
                gp.Top /= canvas_height;
                gp.Width /= canvas_width;
                gp.Height /= canvas_height;
                gp.Padding = Divide(gp.Padding, canvas_width, canvas_height);
                gp.Margin = Divide(gp.Margin, canvas_width, canvas_height);
                gp.BorderThickness = Divide(gp.BorderThickness, canvas_width, canvas_height);
                gp.CornerRadius = Divide(gp.CornerRadius, canvas_width, canvas_height);
            }

            // 记忆
            _canvas_width = canvas_width;
            _canvas_height = canvas_height;

            InitialSize();

            SetBackgroundImage();
        }

        static BitmapImage GetBitmapImage(string filename)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filename, UriKind.Absolute);
                bitmap.EndInit();

                return bitmap;
                /*
                var brush = new ImageBrush(bitmap);
                brush.Stretch = Stretch.Uniform;
                this.Background = brush;
                */
            }
            catch (Exception ex)
            {
                // TODO: 用一个报错文字图片设定为背景?
                return null;
            }
        }

        static Int32Rect GetImageRect(GroupPosition gp)
        {
            return new Int32Rect((int)(gp.Left + gp.BorderThickness.Left + gp.Margin.Left),
                        (int)(gp.Top + gp.BorderThickness.Top + gp.Margin.Top),
                        (int)(gp.Width - gp.BorderThickness.Left - gp.BorderThickness.Right - gp.Margin.Left - gp.Margin.Right),
                        (int)(gp.Height - gp.BorderThickness.Top - gp.BorderThickness.Bottom - gp.Margin.Top - gp.Margin.Bottom));
        }

        // 获得一个 door 元素的位置参数。如果 door 元素缺乏参数，则自动找外围的 group 元素中的参数
        GroupPosition GetPosition(XmlElement door)
        {
            GroupPosition gp = new GroupPosition();
            gp.Left = GetDouble(door, "left", -1);
            gp.Top = GetDouble(door, "top", -1);
            gp.Width = GetDouble(door, "width", -1);
            gp.Height = GetDouble(door, "height", -1);
            gp.Padding = GetThickness(door, "padding", new Thickness(8));
            gp.Margin = GetThickness(door, "margin", new Thickness(0));
            gp.BorderBrush = GetBrush(door, "borderBrush", new System.Windows.Media.SolidColorBrush(Colors.DarkGray));
            gp.BorderThickness = GetThickness(door, "borderThickness", new Thickness(1));
            gp.CornerRadius = GetCornerRadius(door, "cornerRadius", new CornerRadius(0));
            gp.OpenBrush = GetBrush(door,
                "openBrush",
                new System.Windows.Media.SolidColorBrush(DoorItem.DefaultOpenColor),
                (filename) =>
                {
                    string filepath = System.IO.Path.Combine(WpfClientInfo.UserDir, filename);
                    var source = new BitmapImage(new Uri(filepath));
                    // 转换为物理的 pixel 坐标
                    return CutBitmap(source, GetImageRect(gp));
                });
            gp.CloseBrush = GetBrush(door,
                "closeBrush",
                new System.Windows.Media.SolidColorBrush(DoorItem.DefaultCloseColor),
                (filename) =>
                {
                    string filepath = System.IO.Path.Combine(WpfClientInfo.UserDir, filename);
                    var source = new BitmapImage(new Uri(filepath));
                    // 转换为物理的 pixel 坐标
                    return CutBitmap(source, GetImageRect(gp));
                });
            gp.Foreground = GetBrush(door, "foreground", new System.Windows.Media.SolidColorBrush(DoorItem.DefaultForegroundColor));
            gp.ErrorForeground = GetBrush(door, "errorForeground", new System.Windows.Media.SolidColorBrush(DoorItem.DefaultErrorForegroundColor));

            /*
            {
                string testfilename = System.IO.Path.Combine(WpfClientInfo.UserDir, "daily_wallpaper");
                var brush = new ImageBrush(GetBitmapImage(testfilename));
                brush.Stretch = Stretch.Uniform;
                gp.CloseBrush = brush;
            }
            */

            if (gp.Left == -1 || gp.Top == -1 || gp.Width == -1 || gp.Height == -1)
            {
                // 找外围的 group 元素
                XmlElement group = FindGroup(door);
                if (group == null)
                {
                    return gp;
                    // throw new Exception("door 元素没有定义位置参数，也没有从属于任何 group 元素");
                }
                gp.Left = GetDouble(group, "left", -1);
                gp.Top = GetDouble(group, "top", -1);
                gp.Width = GetDouble(group, "width", -1);
                gp.Height = GetDouble(group, "height", -1);
                gp.Padding = GetThickness(group, "padding", new Thickness(8));
                gp.Margin = GetThickness(group, "margin", new Thickness(0));
                gp.BorderBrush = GetBrush(group, "borderBrush", new System.Windows.Media.SolidColorBrush(Colors.DarkGray));
                gp.BorderThickness = GetThickness(group, "borderThickness", new Thickness(1));
                gp.CornerRadius = GetCornerRadius(group, "cornerRadius", new CornerRadius(0));
                gp.OpenBrush = GetBrush(group,
                    "openBrush",
                    new System.Windows.Media.SolidColorBrush(DoorItem.DefaultOpenColor),
                    (filename) =>
                    {
                        string filepath = System.IO.Path.Combine(WpfClientInfo.UserDir, filename);
                        var source = new BitmapImage(new Uri(filepath));
                        // 转换为物理的 pixel 坐标
                        return CutBitmap(source, GetImageRect(gp));
                    });
                gp.CloseBrush = GetBrush(group,
                    "closeBrush",
                    new System.Windows.Media.SolidColorBrush(DoorItem.DefaultCloseColor),
                    (filename) =>
                    {
                        string filepath = System.IO.Path.Combine(WpfClientInfo.UserDir, filename);
                        var source = new BitmapImage(new Uri(filepath));
                        // 转换为物理的 pixel 坐标
                        return CutBitmap(source, GetImageRect(gp));
                    });
                gp.Foreground = GetBrush(group, "foreground", new System.Windows.Media.SolidColorBrush(DoorItem.DefaultForegroundColor));
                gp.ErrorForeground = GetBrush(group, "errorForeground", new System.Windows.Media.SolidColorBrush(DoorItem.DefaultErrorForegroundColor));

                // 看看 door 是 group 的第几个子元素
                var child_nodes = group.SelectNodes("door");
                int index = IndexOf(child_nodes, door);
                if (index == -1)
                    throw new Exception("door 元素在 group 下级没有找到");
                double per_height = gp.Height / child_nodes.Count;
                gp.Top = gp.Top + (index * per_height);
                gp.Height = per_height;
            }

            return gp;
        }

        static int IndexOf(XmlNodeList nodes, XmlElement door)
        {
            int index = 0;
            foreach (var node in nodes)
            {
                if (node == door)
                    return index;
                index++;
            }

            return -1;
        }

        static XmlElement FindGroup(XmlElement door)
        {
            // 找外围的 group 元素
            XmlNode parent = door.ParentNode;
            while (parent != null)
            {
                if (parent.NodeType == XmlNodeType.Element
                    && parent.Name == "group")
                    return parent as XmlElement;
                parent = parent.ParentNode;
            }

            return null;
        }

#if OLD
        public void InitializeButtons(XmlDocument cfg_dom,
            List<DoorItem> door_items)
        {
            // 2019/12/22
            this.canvas.Children.Clear();

            // testing 
            // door_items = new List<DoorItem>();

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

                Grid grid = CreateGroupControls(shelf, door_items, ref index);
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

            SetBackgroundImage();
        }
#endif

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

        /*
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
        */

        void SetBackgroundImage()
        {
            // TODO: ShelfCfgDom 在从自助借还模式切换为智能书柜模式时会成为 null

            // 优化
            if (this.Background != null)
                return;

            // shelf.xml 中 root 元素的 backImageFile 属性
            string backImageFile = ShelfData.ShelfCfgDom?.DocumentElement?.GetAttribute("backImageFile");
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
            catch (Exception ex)
            {
                // TODO: 用一个报错文字图片设定为背景?
            }
        }

        static BitmapSource CutBitmap(BitmapSource source, Int32Rect rect)
        {

            //计算Stride
            var source_stride = source.Format.BitsPerPixel * source.PixelWidth / 8;
            var target_stride = source.Format.BitsPerPixel * rect.Width / 8;
            //声明字节数组
            byte[] data = new byte[rect.Height * target_stride];

            source.CopyPixels(rect, data, target_stride, 0);

            return BitmapSource.Create(rect.Width,
                rect.Height,
                source.DpiX,
                source.DpiY,
                source.Format,  // PixelFormats.Bgr32,
                null,
                data,
                target_stride);
            /*
            // Create WriteableBitmap to copy the pixel data to.      
            WriteableBitmap target = new WriteableBitmap(
              rect.Width,
              rect.Height,
              source.DpiX,
              source.DpiY,
              source.Format, null);

            // Write the pixel data to the WriteableBitmap.
            target.WritePixels(
              new Int32Rect(0, 0, rect.Width, rect.Height),
              data, target_stride, 0);
            return target;
            */
        }

        private void All_Click(object sender, MouseButtonEventArgs e)
        {
            TextBlock button = (TextBlock)sender;
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
        public DoorItem Door { get; set; }
        public string LockName { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
    }
}
