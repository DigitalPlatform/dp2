using DigitalPlatform.RFID;
using DigitalPlatform.Text;
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
            try
            {
                InitializeButtons();
                SetSize(new Size(this.ActualWidth, this.ActualHeight));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化 DoorControl 出现异常:{ex.Message}");
            }
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
                        Content = door_name,
                    };
                    button.SetValue(Grid.RowProperty, row);
                    button.SetValue(Grid.ColumnProperty, column);
                    button.Click += Button_Click;

                    ParseLockString(door.GetAttribute("lock"), out string lockName, out int lockIndex);
                    ParseLockString(door.GetAttribute("antenna"), out string readerName, out int antenna);

                    button.Tag = new Door
                    {
                        Name = button.Name,
                        LockName = lockName,
                        LockIndex = lockIndex,
                        ReaderName = readerName,
                        Antenna = antenna
                    };
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
        public void SetLockState(LockState state)
        {
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

            int i = 0;
            foreach (Button button in buttons)
            {
                Door door = doors[i];
                if (IsEqual(state.Name, door.LockName)
                    && state.Index == door.LockIndex)
                {
                    door.State = state.State;

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (state.State == "open")
                            button.Background = new SolidColorBrush(Colors.Red);
                        else
                            button.Background = new SolidColorBrush(Colors.Green);
                    }));
                }

                i++;
            }
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
    }
}
