using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

using DigitalPlatform.RFID;
using DigitalPlatform.Text;

namespace dp2SSL
{
    public class DoorItem : INotifyPropertyChanged
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

        private string _state;

        // 状态
        public string State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        public string LockName { get; set; }
        public int LockIndex { get; set; }
        public string ReaderName { get; set; }
        public int Antenna { get; set; }

        // public string State { get; set; }


        public static List<DoorItem> BuildItems(XmlDocument cfg_dom)
        {
            List<DoorItem> results = new List<DoorItem>();

            XmlNodeList shelfs = cfg_dom.DocumentElement.SelectNodes("shelf");

            int column = 0;
            foreach (XmlElement shelf in shelfs)
            {
                XmlNodeList doors = shelf.SelectNodes("door");
                int row = 0;
                foreach (XmlElement door in doors)
                {
                    string door_name = door.GetAttribute("name");

                    ParseLockString(door.GetAttribute("lock"), out string lockName, out int lockIndex);
                    ParseLockString(door.GetAttribute("antenna"), out string readerName, out int antenna);

                    DoorItem item = new DoorItem
                    {
                        Name = door_name,
                        LockName = lockName,
                        LockIndex = lockIndex,
                        ReaderName = readerName,
                        Antenna = antenna,
                    };

                    results.Add(item);
                    row++;
                }

                column++;
            }

            return results;
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

        public static List<DoorItem> FindDoors(
            List<DoorItem> _doors,
            string readerName,
            string antenna)
        {
            List<DoorItem> results = new List<DoorItem>();
            foreach (var door in _doors)
            {
                if (door.Antenna.ToString() == antenna
                    && IsEqual(door.ReaderName, readerName))
                    results.Add(door);
            }
            return results;
        }

        public static bool IsEqual(string name1, string name2)
        {
            if (name1 == "*" || name2 == "*")
                return true;
            return name1 == name2;
        }

        public static List<LockCommand> GetLockCommands()
        {
            string cfg_filename = App.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);
            return GetLockCommands(cfg_dom);
        }

        // 注：不用刷新。可以把背景色绑定到状态文字上
        // 刷新门锁(开/关)状态
        public static LockChanged SetLockState(
            List<DoorItem> _doors,
            LockState state)
        {
            List<LockChanged> results = new List<LockChanged>();

            int i = 0;
            foreach (DoorItem door in _doors)
            {
                if (DoorItem.IsEqual(state.Name, door.LockName)
                    && state.Index == door.LockIndex)
                {
                    results.Add(new LockChanged
                    {
                        LockName = door.Name,
                        OldState = door.State,
                        NewState = state.State
                    });

                    door.State = state.State;

                    /*
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (state.State == "open")
                            door.Button.Background = new SolidColorBrush(Colors.Red);
                        else
                            door.Button.Background = new SolidColorBrush(Colors.Green);
                    }));
                    */
                }

                i++;
            }

            if (results.Count == 0)
                return new LockChanged();
            return results[0];
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

        class Three
        {
            public List<Entity> All { get; set; }
            public List<Entity> Removes { get; set; }
            public List<Entity> Adds { get; set; }
        }

        public static Hashtable Build(List<Entity> entities,
            List<DoorItem> items)
        {
            // door --> List<Entity>
            Hashtable table = new Hashtable();
            foreach (Entity entity in entities)
            {
                var doors = DoorItem.FindDoors(items, entity.ReaderName, entity.Antenna);
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

        // 统计各种计数，然后刷新到 DoorItem 中
        public static void DisplayCount(List<Entity> entities,
            List<Entity> adds,
            List<Entity> removes,
            List<DoorItem> _doors)
        {
            var all_table = Build(entities, _doors);
            var add_table = Build(adds, _doors);
            var remove_table = Build(removes, _doors);

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
                    door.Count = count.Count;
                    door.Add = add.Count;
                    door.Remove = remove.Count;
                    /*
                    TextBlock block = (TextBlock)door.Button.GetValue(Button.ContentProperty);
                    SetBlockText(block, null,
                        count.Count.ToString(),
                        add.Count.ToString(),
                        remove.Count.ToString());
                        */
                }));
            }
        }

    }


    public delegate void OpenCountChangedEventHandler(object sender,
OpenCountChangedEventArgs e);

    /// <summary>
    /// 打开门数变化事件的参数
    /// </summary>
    public class OpenCountChangedEventArgs : EventArgs
    {
        public int OldCount { get; set; }
        public int NewCount { get; set; }
    }

}
