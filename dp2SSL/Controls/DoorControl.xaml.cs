using System;
using System.Collections;
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

        List<Door> _doors = new List<Door>();

        public DoorControl()
        {
            InitializeComponent();

            try
            {
                InitializeButtons();
                SetSize(new Size(this.ActualWidth, this.ActualHeight));
            }
            catch (Exception ex)
            {
                App.CurrentApp.SetError("cfg", $"初始化 DoorControl 出现异常:{ex.Message}");
            }

            Loaded += DoorControl_Loaded;
            SizeChanged += DoorControl_SizeChanged;
            // LayoutUpdated += DoorControl_LayoutUpdated;
        }

        private void DoorControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetSize(e.NewSize);
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

        class Three
        {
            public List<Entity> All { get; set; }
            public List<Entity> Removes { get; set; }
            public List<Entity> Adds { get; set; }


        }

        public Hashtable Build(List<Entity> entities)
        {
            // door --> List<Entity>
            Hashtable table = new Hashtable();
            foreach (Entity entity in entities)
            {
                var doors = FindDoors(entity.ReaderName, entity.Antenna);
                foreach (var door in doors)
                {
                    List<Entity> list = null;
                    if (table.ContainsKey(door) == false)
                    {
                        list = new List<Entity>();
                        table[door] = list;
                    }
                    else
                        list = (List<Entity>)table[door];
                    list.Add(entity);
                }
            }

            return table;
        }

        public void DisplayCount(List<Entity> entities,
            List<Entity> adds,
            List<Entity> removes)
        {
            var all_table = Build(entities);
            var add_table = Build(adds);
            var remove_table = Build(removes);

            foreach (var door in _doors)
            {
                List<Entity> count = (List<Entity>)all_table[door];
                if (count == null)
                    count = new List<Entity>();

                List<Entity> add = (List<Entity>)add_table[door];
                if (add == null)
                    add = new List<Entity>();

                List<Entity> remove = (List<Entity>)remove_table[door];
                if (remove == null)
                    remove = new List<Entity>();


                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TextBlock block = (TextBlock)door.Button.GetValue(Button.ContentProperty);
                    SetBlockText(block, null,
                        count.Count.ToString(),
                        add.Count.ToString(),
                        remove.Count.ToString());
                }));
            }
        }

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

        void InitializeButtons()
        {
            string cfg_filename = System.IO.Path.Combine(WpfClientInfo.DataDir, "shelf.xml");
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);

            XmlNodeList shelfs = cfg_dom.DocumentElement.SelectNodes("shelf");
            // int shelf_width = total_width / Math.Max(1, shelfs.Count);
            // int level_height = 100;
            bool rowDefinitionCreated = false;
            // 初始化 Definitions
            foreach (XmlElement shelf in shelfs)
            {
                this.grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

                if (rowDefinitionCreated == false)
                {
                    XmlNodeList doors = shelf.SelectNodes("door");
                    // level_height = total_height / Math.Max(1, doors.Count);
                    foreach (XmlElement door in doors)
                    {
                        this.grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
                    }

                    rowDefinitionCreated = true;
                }
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
                        // Content = door_name,
                    };

                    var block = BuildTextBlock();
                    SetBlockText(block, door_name, "0", null, null);

                    button.SetValue(Button.ContentProperty, block);
                    button.SetValue(Grid.RowProperty, row);
                    button.SetValue(Grid.ColumnProperty, column);
                    button.Click += Button_Click;

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

                    _doors.Add(tag);

                    button.Tag = tag;
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

        public static List<LockCommand> GetLockCommands()
        {
            string cfg_filename = System.IO.Path.Combine(WpfClientInfo.DataDir, "shelf.xml");
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);
            return GetLockCommands(cfg_dom);
        }

        // 构造锁命令字符串数组
        public static List<LockCommand> GetLockCommands(XmlDocument cfg_dom)
        {
            // lockName --> List<int>
            Hashtable table = new Hashtable();
            XmlNodeList doors = cfg_dom.DocumentElement.SelectNodes("//door");
            foreach (XmlElement door in doors)
            {
                string lockDef = door.GetAttribute("lock");
                ParseLockString(lockDef, out string lockName, out int lockIndex);
                List<int> array = null;
                if (table.ContainsKey(lockName) == false)
                {
                    array = new List<int>();
                    table[lockName] = array;
                }
                else
                    array = (List<int>)table[lockName];

                array.Add(lockIndex);
            }

            List<LockCommand> results = new List<LockCommand>();
            foreach (string key in table.Keys)
            {
                StringBuilder text = new StringBuilder();
                int i = 0;
                foreach (var v in table[key] as List<int>)
                {
                    if (i > 0)
                        text.Append(",");
                    text.Append(v);
                    i++;
                }
                results.Add(new LockCommand
                {
                    LockName = key,
                    Indices = text.ToString()
                });
            }

            return results;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            // MessageBox.Show(button.Name);
            OpenDoor?.Invoke(sender, new OpenDoorEventArgs { Door = button.Tag as Door });
        }



        // 刷新门锁(开/关)状态
        public LockChanged SetLockState(LockState state)
        {
            /*
            List<Button> buttons = new List<Button>();
            List<Door> doors = new List<Door>();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (Button button in this.grid.Children)
                {
                    if (button == null)
                        continue;

                    buttons.Add(button);
                    doors.Add(button.Tag as Door);
                }
            }));
            */

            List<LockChanged> results = new List<LockChanged>();

            int i = 0;
            foreach (Door door in _doors)
            {
                if (IsEqual(state.Name, door.LockName)
                    && state.Index == door.LockIndex)
                {
                    results.Add(new LockChanged
                    {
                        LockName = door.Name,
                        OldState = door.State,
                        NewState = state.State
                    });

                    door.State = state.State;

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (state.State == "open")
                            door.Button.Background = new SolidColorBrush(Colors.Red);
                        else
                            door.Button.Background = new SolidColorBrush(Colors.Green);
                    }));
                }

                i++;
            }

            if (results.Count == 0)
                return new LockChanged();
            return results[0];
        }

        static bool IsEqual(string name1, string name2)
        {
            if (name1 == "*" || name2 == "*")
                return true;
            return name1 == name2;
        }

        public static void ParseLockString(string text,
            out string lockName,
            out int index)
        {
            lockName = "";
            index = 0;
            var parts = StringUtil.ParseTwoPart(text, ":");
            lockName = parts[0];
            index = Convert.ToInt32(parts[1]);
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
        public Door Door { get; set; }
    }

    public class Door
    {
        public string Name { get; set; }
        public string LockName { get; set; }
        public int LockIndex { get; set; }
        public string ReaderName { get; set; }
        public int Antenna { get; set; }

        public string State { get; set; }
        public string Count { get; set; }   // 现有册数

        public Button Button { get; set; }
    }

    public class LockChanged
    {
        public string LockName { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
    }
}
