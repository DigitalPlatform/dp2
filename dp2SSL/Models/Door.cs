using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Diagnostics;

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

        private string _type;

        // 门的类型。值可能为 空/free。free 表示这是一个安装在书柜外面的读卡器，它实际上没有门 
        public string Type
        {
            get => _type;
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged("Type");
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

        private int _errorCount;

        // 有出错的册数
        public int ErrorCount
        {
            get => _errorCount;
            set
            {
                if (_errorCount != value)
                {
                    _errorCount = value;
                    OnPropertyChanged("ErrorCount");
                }
            }
        }

        private string _shelfNo;

        // 架号
        public string ShelfNo
        {
            get => _shelfNo;
            set
            {
                if (_shelfNo != value)
                {
                    _shelfNo = value;
                    OnPropertyChanged("ShelfNo");
                }
            }
        }

        private string _state = "";

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

        // 全部图书对象
        EntityCollection _allEntities = new EntityCollection();
        public EntityCollection AllEntities
        {
            get
            {
                return _allEntities;
            }
        }

        // 新添加的图书对象
        EntityCollection _addEntities = new EntityCollection();
        public EntityCollection AddEntities
        {
            get
            {
                return _addEntities;
            }
        }

        // 新添加的图书对象
        EntityCollection _removeEntities = new EntityCollection();
        public EntityCollection RemoveEntities
        {
            get
            {
                return _removeEntities;
            }
        }

        // 错误状态的图书对象
        EntityCollection _errorEntities = new EntityCollection();
        public EntityCollection ErrorEntities
        {
            get
            {
                return _errorEntities;
            }
        }

        public string LockName { get; set; }
        public int LockIndex { get; set; }
        public string ReaderName { get; set; }
        public int Antenna { get; set; }

        // public string State { get; set; }

        // 根据 shelf.xml 配置文件定义，构建 DoorItem 集合
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
                    string door_type = door.GetAttribute("type");
                    string door_shelfNo = door.GetAttribute("shelfNo");

                    ParseLockString(door.GetAttribute("lock"), out string lockName, out int lockIndex);
                    ParseLockString(door.GetAttribute("antenna"), out string readerName, out int antenna);

                    DoorItem item = new DoorItem
                    {
                        Name = door_name,
                        LockName = lockName,
                        LockIndex = lockIndex,
                        ReaderName = readerName,
                        Antenna = antenna,
                        Type = door_type,
                        ShelfNo = door_shelfNo,
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
            if (Int32.TryParse(parts[1], out index) == false)
                index = 0;
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
            List<Entity> errors,
            List<DoorItem> _doors)
        {
            var all_table = Build(entities, _doors);
            var add_table = Build(adds, _doors);
            var remove_table = Build(removes, _doors);
            var error_table = Build(errors, _doors);

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

                List<Entity> error = (List<Entity>)error_table[door];
                if (error == null)
                    error = new List<Entity>();

                /*
                // TODO: 触发 BookChanged 事件?
                ShelfData.TriggerBookChanged(new BookChangedEventArgs
                {
                    Door = door,
                    All = count,
                    Adds = add,
                    Removes = remove,
                    Errors = error
                });
                */


                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    // 更新 entities
                    // TODO: 异步填充
                    if (Refresh(door._allEntities, count) == true
                    || Refresh(door._removeEntities, remove) == true
                    || Refresh(door._addEntities, add) == true
                    || Refresh(door._errorEntities, error) == true)
                    {
                        var task = Task.Run(async () =>
                        {
                            await ShelfData.FillBookFields(door._allEntities);
                            await ShelfData.FillBookFields(door._removeEntities);
                            await ShelfData.FillBookFields(door._addEntities);
                            await ShelfData.FillBookFields(door._errorEntities);
                        });
                    }

                    door.Count = count.Count;
                    door.Add = add.Count;
                    door.Remove = remove.Count;
                    door.ErrorCount = error.Count;
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

        // 根据 items 集合更新 collection 集合内容
        static bool Refresh(EntityCollection collection, List<Entity> items)
        {
            bool changed = false;
            int oldCount = items.Count;
            // 添加 items 中多出来的对象
            foreach (var item in items)
            {
                // TODO: 用 UID 来搜索
                var found = collection.FindEntityByUID(item.UID);
                if (found == null)
                {
                    Entity dup = item.Clone();
                    dup.Container = collection;
                    collection.Add(dup);
                    changed = true;
                }
            }

            List<Entity> removes = new List<Entity>();
            // 删除 collection 中多出来的对象
            foreach (var item in collection)
            {
                var found = items.Find((o) => { return (o.UID == item.UID); });
                if (found == null)
                    removes.Add(item);
            }

            foreach (var item in removes)
            {
                collection.Remove(item);
                changed = true;
            }

            Debug.Assert(oldCount == items.Count, "");
            return changed;
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

    public delegate void BookChangedEventHandler(object sender,
BookChangedEventArgs e);

    /// <summary>
    /// 图书拿放变化事件的参数
    /// </summary>
    public class BookChangedEventArgs : EventArgs
    {
        public DoorItem Door { get; set; }

        public List<Entity> All { get; set; }
        public List<Entity> Adds { get; set; }
        public List<Entity> Removes { get; set; }
        public List<Entity> Errors { get; set; }
    }
}
