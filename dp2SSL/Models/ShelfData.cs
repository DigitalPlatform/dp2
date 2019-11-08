using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using static dp2SSL.LibraryChannelUtil;

namespace dp2SSL
{
    /// <summary>
    /// 智能书架要用到的数据
    /// </summary>
    public static class ShelfData
    {
        public static event OpenCountChangedEventHandler OpenCountChanged;

        /*
        public static event BookChangedEventHandler BookChanged;

        public static void TriggerBookChanged(BookChangedEventArgs e)
        {
            BookChanged?.Invoke(null, e);
        }
        */

        // 读者证读卡器名字。在 shelf.xml 中配置
        static string _patronReaderName = "";

        // 当前处于打开状态的门的个数
        public static int OpeningDoorCount
        {
            get
            {
                return _openingDoorCount;
            }
        }

        static int _openingDoorCount = -1; // 当前处于打开状态的门的个数。-1 表示个数尚未初始化


        #region


        public static void RfidManager_ListLocks(object sender, ListLocksEventArgs e)
        {
            if (e.Result.Value == -1)
                return;

            bool triggerAllClosed = false;
            {
                int count = 0;
                foreach (var state in e.Result.States)
                {
                    if (state.State == "open")
                        count++;

                    var result = DoorItem.SetLockState(ShelfData.Doors, state);
                    if (result.LockName != null && result.OldState != null && result.NewState != null)
                    {
                        if (result.NewState != result.OldState)
                        {
                            if (result.NewState == "open")
                                App.CurrentApp.Speak($"{result.LockName} 打开");
                            else
                                App.CurrentApp.Speak($"{result.LockName} 关闭");
                        }
                    }
                }

                if (_openingDoorCount > 0 && count == 0)
                    triggerAllClosed = true;

                SetOpenCount(count);
            }

            /*
            // TODO: 如果从有门打开的状态变为全部门都关闭的状态，要尝试提交一次出纳请求
            if (triggerAllClosed)
            {
                SubmitCheckInOut();
                PatronClear(false);  // 确保在没有可提交内容的情况下也自动清除读者信息
            }
            */
        }

        // 设置打开门数量
        static void SetOpenCount(int count)
        {
            int oldCount = _openingDoorCount;

            _openingDoorCount = count;

            // 打开门的数量发生变化
            if (oldCount != _openingDoorCount)
            {
                OpenCountChanged?.Invoke(null, new OpenCountChangedEventArgs
                {
                    OldCount = oldCount,
                    NewCount = count
                });

                // 
                RefreshReaderNameList();
            }
        }

        public static void RefreshReaderNameList()
        {
            if (_openingDoorCount == 0)
            {
                // 关闭图书读卡器(只使用读者证读卡器)
                if (string.IsNullOrEmpty(_patronReaderName) == false
                    && RfidManager.ReaderNameList != _patronReaderName)
                {
                    RfidManager.ReaderNameList = _patronReaderName;
                    RfidManager.ClearCache();
                }
            }
            else
            {
                // 打开图书读卡器(同时也使用读者证读卡器)
                if (RfidManager.ReaderNameList != "*")
                {
                    RfidManager.ReaderNameList = "*";
                    RfidManager.ClearCache();
                }
            }
        }

        // exception:
        //      可能会抛出异常
        public static void InitialShelf()
        {
            ShelfData.InitialDoors();

            // 要在初始化以前设定好
            RfidManager.AntennaList = GetAntennaList();
            RfidManager.LockCommands = ShelfData.GetLockCommands();
            _patronReaderName = GetPatronReaderName();
        }

        // 从 shelf.xml 配置文件中获得读者证读卡器名
        public static string GetPatronReaderName()
        {
            if (ShelfCfgDom == null)
                return "";

            XmlElement patron = ShelfCfgDom.DocumentElement.SelectSingleNode("patron") as XmlElement;
            if (patron == null)
                return "";

            return patron.GetAttribute("readerName");


            /*
            string cfg_filename = ShelfData.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.Load(cfg_filename);

                XmlElement patron = cfg_dom.DocumentElement.SelectSingleNode("patron") as XmlElement;
                if (patron == null)
                    return "";

                return patron.GetAttribute("readerName");
            }
            catch (FileNotFoundException)
            {
                return "";
            }
            catch (Exception ex)
            {
                this.SetError("cfg", $"装载配置文件 shelf.xml 时出现异常: {ex.Message}");
                return "";
            }
            */
        }

        // 从 shelf.xml 配置文件中归纳出所有的天线编号
        public static string GetAntennaList()
        {
            if (ShelfCfgDom == null)
                return "";

            List<string> antenna_list = new List<string>();

            XmlNodeList doors = ShelfCfgDom.DocumentElement.SelectNodes("shelf/door");
            foreach (XmlElement door in doors)
            {
                DoorItem.ParseLockString(door.GetAttribute("antenna"),
                    out string readerName,
                    out int antenna);
                antenna_list.Add(antenna.ToString());
            }

            StringUtil.RemoveDup(ref antenna_list, false);
            return StringUtil.MakePathList(antenna_list, "|");

            /*
            List<string> antenna_list = new List<string>();
            string cfg_filename = ShelfData.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.Load(cfg_filename);

                XmlNodeList doors = cfg_dom.DocumentElement.SelectNodes("shelf/door");
                foreach (XmlElement door in doors)
                {
                    DoorItem.ParseLockString(door.GetAttribute("antenna"),
                        out string readerName,
                        out int antenna);
                    antenna_list.Add(antenna.ToString());
                }

                StringUtil.RemoveDup(ref antenna_list, false);
                return StringUtil.MakePathList(antenna_list, "|");
            }
            catch (FileNotFoundException)
            {
                return "";
            }
            catch (Exception ex)
            {
                this.SetError("cfg", $"装载配置文件 shelf.xml 时出现异常: {ex.Message}");
                return "";
            }
            */
        }

        #endregion

        static XmlDocument _shelfCfgDom = null;

        public static XmlDocument ShelfCfgDom
        {
            get
            {
                return _shelfCfgDom;
            }
        }

        public static string ShelfFilePath
        {
            get
            {
                string cfg_filename = System.IO.Path.Combine(WpfClientInfo.UserDir, "shelf.xml");
                return cfg_filename;
            }
        }

        static List<DoorItem> _doors = new List<DoorItem>();
        public static List<DoorItem> Doors
        {
            get
            {
                return _doors;
            }
        }

        static List<Entity> _all = new List<Entity>();
        static List<Entity> _adds = new List<Entity>();
        static List<Entity> _removes = new List<Entity>();

        public static List<Entity> All
        {
            get
            {
                return _all;
            }
        }

        public static List<Entity> Adds
        {
            get
            {
                return _adds;
            }
        }

        public static List<Entity> Removes
        {
            get
            {
                return _removes;
            }
        }

        public static void InitialDoors()
        {
            {
                string cfg_filename = ShelfFilePath;
                XmlDocument cfg_dom = new XmlDocument();
                cfg_dom.Load(cfg_filename);

                _shelfCfgDom = cfg_dom;
            }

            _doors = DoorItem.BuildItems(_shelfCfgDom);
        }

        static bool _firstInitial = false;

        public static bool FirstInitialized
        {
            get
            {
                return _firstInitial;
            }
            set
            {
                _firstInitial = value;
            }
        }

        public delegate void Delegate_displayText(string text);
        public delegate bool Delegate_cancelled();


        // 首次初始化智能书柜所需的标签相关数据结构
        // 初始化开始前，要先把 RfidManager.ReaderNameList 设置为 "*"
        // 初始化完成前，先不要允许(开关门变化导致)修改 RfidManager.ReaderNameList
        public static async Task InitialShelfEntities(
            Delegate_displayText func_display,
            Delegate_cancelled func_cancelled)
        {
            /*
            ProgressWindow progress = null;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progress = new ProgressWindow();
                progress.MessageText = "正在初始化图书信息，请稍候 ...";
                progress.Owner = Application.Current.MainWindow;
                progress.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                progress.Closed += Progress_Closed;
                //progress.Width = 700;
                //progress.Height = 500;
                progress.Show();
                AddLayer();
            }));
            this.doorControl.Visibility = Visibility.Hidden;
            */

            try
            {
                // TODO: 出现“正在初始化”的对话框。另外需要注意如果 DataReady 信号永远来不了怎么办
                func_display("等待读卡器就绪 ...");
                bool ret = await Task.Run(() =>
                {
                    while (true)
                    {
                        if (TagList.DataReady == true)
                            return true;
                        if (func_cancelled() == true)
                            return false;
                        Thread.Sleep(100);
                    }
                });

                if (ret == false)
                    return;

                // 使用全部读卡器、全部天线进行初始化。即便门是全部关闭的(注：一般情况下，当门关闭的时候图书读卡器是暂停盘点的)
                func_display("启用全部读卡器 ...");
                ret = await Task.Run(() =>
                {
                    // 使用全部读卡器，全部天线
                    RfidManager.Pause = true;
                    RfidManager.ReaderNameList = "*";
                    RfidManager.AntennaList = GetAntennaList();
                    TagList.DataReady = false;
                    RfidManager.Pause = false;
                    RfidManager.ClearCache();   // 迫使立即重新请求 Inventory
                    while (true)
                    {
                        if (TagList.DataReady == true)
                            return true;
                        if (func_cancelled() == true)
                            return false;
                        Thread.Sleep(100);
                    }
                });

                if (ret == false)
                    return;

                // TODO: 显示“等待锁控就绪 ...”
                func_display("等待锁控就绪 ...");
                ret = await Task.Run(() =>
                {
                    while (true)
                    {
                        if (OpeningDoorCount != -1)
                            return true;
                        if (func_cancelled() == true)
                            return false;
                        Thread.Sleep(100);
                    }
                });

                if (ret == false)
                    return;

                _all.Clear();
                var books = TagList.Books;
                foreach (var tag in books)
                {
                    _all.Add(NewEntity(tag));
                }

                // DoorItem.DisplayCount(_all, _adds, _removes, App.CurrentApp.Doors);
                RefreshCount();

                // TryReturn(progress, _all);
                _firstInitial = true;   // 第一次初始化已经完成

                var task = Task.Run(async () =>
                {
                    await FillBookFields(_all);
                    await FillBookFields(_adds);
                    await FillBookFields(_removes);
                });
            }
            finally
            {

                /*
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));

                this.doorControl.Visibility = Visibility.Visible;
                */
            }
        }

        static Entity NewEntity(TagAndData tag)
        {
            var result = new Entity
            {
                UID = tag.OneTag.UID,
                ReaderName = tag.OneTag.ReaderName,
                Antenna = tag.OneTag.AntennaID.ToString(),
                TagInfo = tag.OneTag.TagInfo,
            };

            EntityCollection.SetPII(result);
            return result;
        }

        // 检查一本图书是否处在普通(非 free) 类型的门内
        public static bool BelongToNormal(Entity entity)
        {
            var doors = DoorItem.FindDoors(_doors, entity.ReaderName, entity.Antenna);
            int count = 0;
            foreach (DoorItem door in doors)
            {
                if (door.Type == "free")
                    return false;
                count++;
            }
            return count > 0;
        }

        // 刷新门内图书数字显示
        public static void RefreshCount()
        {
            List<Entity> errors = GetErrors(_all, _adds, _removes);
            DoorItem.DisplayCount(_all, _adds, _removes, errors, Doors);
        }

        public static List<Entity> GetErrors(List<Entity> all,
            List<Entity> adds,
            List<Entity> removes)
        {
            List<Entity> errors = new List<Entity>();
            List<Entity> list = new List<Entity>(all);
            list.AddRange(adds);
            list.AddRange(removes);
            foreach (var entity in list)
            {
                if (entity.Error != null && entity.ErrorColor == "red")
                {
                    if (errors.IndexOf(entity) == -1)
                        Add(errors, entity);
                }
            }

            return errors;
        }

        public static List<LockCommand> GetLockCommands()
        {
            /*
            string cfg_filename = App.ShelfFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);
            */
            return GetLockCommands(ShelfCfgDom);
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
                DoorItem.ParseLockString(lockDef, out string lockName, out int lockIndex);
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

        static List<Entity> Find(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            return results;
        }

        internal static bool Add(List<Entity> entities, Entity entity)
        {
            Debug.Assert(entity != null, "");
            Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == entity.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
                return false;
            entities.Add(entity);
            return true;
        }

        internal static bool Remove(List<Entity> entities, Entity entity)
        {
            Debug.Assert(entity != null, "");
            Debug.Assert(string.IsNullOrEmpty(entity.UID) == false, "");

            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == entity.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
            {
                foreach (var o in results)
                {
                    entities.Remove(o);
                }
                return true;
            }
            return false;
        }

        static bool Add(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            if (results.Count == 0)
            {
                entities.Add(NewEntity(tag));
                return true;
            }
            return false;
        }

        static bool Remove(List<Entity> entities, TagAndData tag)
        {
            List<Entity> results = new List<Entity>();
            entities.ForEach((o) =>
            {
                if (o.UID == tag.OneTag.UID)
                    results.Add(o);
            });
            if (results.Count > 0)
            {
                foreach (var o in results)
                {
                    entities.Remove(o);
                }
                return true;
            }
            return false;
        }

        // 更新 Entity 信息
        static void Update(List<Entity> entities, TagAndData tag)
        {
            foreach (var entity in entities)
            {
                if (entity.UID == tag.OneTag.UID)
                {
                    entity.ReaderName = tag.OneTag.ReaderName;
                    entity.Antenna = tag.OneTag.AntennaID.ToString();
                }
            }
        }

        static SpeakList _speakList = new SpeakList();


        // 跟随事件动态更新列表
        // Add: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        // Remove: 检查列表中是否存在这个 PII，如果存在，则修改状态为 不在架
        //      如果不存在这个 PII，则不做任何动作
        // Update: 检查列表中是否存在这个 PII，如果存在，则修改状态为 在架，并设置 UID 成员
        //      如果不存在，则为列表添加一个新元素，修改状态为在架，并设置 UID 和 PII 成员
        public static async Task ChangeEntities(BaseChannel<IRfid> channel,
            TagChangedEventArgs e)
        {
            if (ShelfData.FirstInitialized == false)
                return;

            // 开门状态下，动态信息暂时不要合并
            bool changed = false;

            List<TagAndData> tags = new List<TagAndData>();
            if (e.AddBooks != null)
                tags.AddRange(e.AddBooks);
            if (e.UpdateBooks != null)
                tags.AddRange(e.UpdateBooks);

            List<string> add_uids = new List<string>();
            // 新添加标签(或者更新标签信息)
            foreach (var tag in tags)
            {
                // 没有 TagInfo 信息的先跳过
                if (tag.OneTag.TagInfo == null)
                    continue;

                add_uids.Add(tag.OneTag.UID);

                // 看看 _all 里面有没有
                var results = Find(_all, tag);
                if (results.Count == 0)
                {
                    if (Add(_adds, tag) == true)
                    {
                        changed = true;
                    }
                    if (Remove(_removes, tag) == true)
                        changed = true;
                }
                else
                {
                    // 更新 _all 里面的信息
                    Update(_all, tag);

                    // 要把 _adds 和 _removes 里面都去掉
                    if (Remove(_adds, tag) == true)
                        changed = true;
                    if (Remove(_removes, tag) == true)
                        changed = true;
                }
            }

            // 拿走标签
            int removeBooksCount = 0;
            foreach (var tag in e.RemoveBooks)
            {
                if (tag.OneTag.TagInfo == null)
                    continue;

                if (tag.Type == "patron")
                    continue;

                // 看看 _all 里面有没有
                var results = Find(_all, tag);
                if (results.Count > 0)
                {
                    if (Remove(_adds, tag) == true)
                        changed = true;
                    if (Add(_removes, tag) == true)
                    {
                        changed = true;
                    }
                }
                else
                {
                    // _all 里面没有，很奇怪。但，
                    // 要把 _adds 和 _removes 里面都去掉
                    if (Remove(_adds, tag) == true)
                        changed = true;
                    if (Remove(_removes, tag) == true)
                        changed = true;
                }

                removeBooksCount++;
            }

            StringUtil.RemoveDup(ref add_uids, false);
            int add_count = add_uids.Count;
            int remove_count = 0;
            if (e.RemoveBooks != null)
                remove_count = removeBooksCount; // 注： e.RemoveBooks.Count 是不准确的，有时候会把 ISO15693 的读者卡判断时作为 remove 信号

            if (remove_count > 0)
            {
                // App.CurrentApp.SpeakSequence($"取出 {remove_count} 本");
                _speakList.Speak("取出 {0} 本",
                    remove_count,
                    (s) =>
                    {
                        App.CurrentApp.SpeakSequence(s);
                    });
            }
            if (add_count > 0)
            {
                // App.CurrentApp.SpeakSequence($"放入 {add_count} 本");
                _speakList.Speak("放入 {0} 本",
    add_count,
    (s) =>
    {
        App.CurrentApp.SpeakSequence(s);
    });
            }

            // TODO: 把 add remove error 动作分散到每个门，然后再触发 ShelfData.BookChanged 事件

            if (changed == true)
            {
                // DoorItem.DisplayCount(_all, _adds, _removes, ShelfData.Doors);
                ShelfData.RefreshCount();
            }

            var task = Task.Run(async () =>
            {
                await FillBookFields(_all);
                await FillBookFields(_adds);
                await FillBookFields(_removes);
            });
        }

        public static async Task FillBookFields(// BaseChannel<IRfid> channel,
    IList<Entity> entities)
        {
            try
            {
                int error_count = 0;
                foreach (Entity entity in entities)
                {
                    /*
                    if (_cancel == null
                        || _cancel.IsCancellationRequested)
                        return;
                        */
                    if (entity.FillFinished == true)
                        continue;

                    //if (string.IsNullOrEmpty(entity.Error) == false)
                    //    continue;

                    // 获得 PII
                    // 注：如果 PII 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.PII))
                    {
                        if (entity.TagInfo == null)
                            continue;

                        Debug.Assert(entity.TagInfo != null);

                        LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
(int)entity.TagInfo.BlockSize,
"" // tag.TagInfo.LockStatus
);
                        string pii = chip.FindElement(ElementOID.PII)?.Text;
                        if (string.IsNullOrEmpty(pii))
                        {
                            // 报错
                            App.CurrentApp.SpeakSequence($"警告：发现 PII 字段为空的标签");
                            entity.SetError($"PII 字段为空");
                            entity.FillFinished = true;
                            error_count++;
                            continue;
                        }

                        entity.PII = PageBorrow.GetCaption(pii);
                    }

                    // 获得 Title
                    // 注：如果 Title 为空，文字中要填入 "(空)"
                    if (string.IsNullOrEmpty(entity.Title)
                        && string.IsNullOrEmpty(entity.PII) == false && entity.PII != "(空)")
                    {
                        GetEntityDataResult result = await
                            Task<GetEntityDataResult>.Run(() =>
                            {
                                return GetEntityData(entity.PII);
                            });
                        if (result.Value == -1 || result.Value == 0)
                        {
                            // TODO: 条码号没有找到的错误码要单独记下来
                            // 报错
                            string error = $"警告：PII 为 {entity.PII} 的标签出错: {result.ErrorInfo}";
                            if (result.ErrorCode == "NotFound")
                                error = $"警告：PII 为 {entity.PII} 的图书没有找到记录";

                            App.CurrentApp.SpeakSequence(error);
                            entity.SetError(result.ErrorInfo);
                            entity.FillFinished = true;
                            error_count++;
                            continue;
                        }
                        entity.Title = PageBorrow.GetCaption(result.Title);
                        entity.SetData(result.ItemRecPath, result.ItemXml);
                    }

                    entity.SetError(null);
                    entity.FillFinished = true;
                }

                ShelfData.RefreshCount();
            }
            catch (Exception ex)
            {
                //LibraryChannelManager.Log?.Error($"FillBookFields() 发生异常: {ExceptionUtil.GetExceptionText(ex)}");   // 2019/9/19
                //SetGlobalError("current", $"FillBookFields() 发生异常(已写入错误日志): {ex.Message}"); // 2019/9/11 增加 FillBookFields() exception:
            }
        }

    }
}
