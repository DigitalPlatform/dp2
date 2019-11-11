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
using System.Windows.Documents;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
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
                        if (result.NewState != result.OldState
                            && string.IsNullOrEmpty(result.OldState) == false)
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

        public static string GetShelfNo(Entity entity)
        {
            var doors = DoorItem.FindDoors(_doors, entity.ReaderName, entity.Antenna);
            if (doors.Count == 0)
                return "";
            return doors[0].ShelfNo;
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
                if (string.IsNullOrEmpty(lockDef))
                    continue;
                DoorItem.ParseLockString(lockDef, out string lockName, out int lockIndex);
                if (string.IsNullOrEmpty(lockName))
                    continue;

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



        // public delegate void Delegate_showDialog();

        // -1 -1 n only change progress value
        // -1 -1 -1 hide progress bar
        public delegate void Delegate_setProgress(double min, double max, double value);

        // result.Value
        //      -1  出错(要用对话框显示结果)
        //      0   没有必要处理
        //      1   已经完成处理(要用对话框显示结果)
        public static SubmitResult SubmitCheckInOut(
            // Delegate_showDialog func_showDialog,
            Delegate_setProgress func_setProgress,
            // ProgressWindow progress,
            string patronBarcode,
            string patron_name,
            List<ActionInfo> actions,
            bool patron_filled)
        {
            // TODO: 如果当前没有读者身份，则当作初始化处理，将书柜内的全部图书做还书尝试；被拿走的图书记入本地日志(所谓无主操作)
            // TODO: 注意还书，也就是往书柜里面放入图书，是不需要具体读者身份就可以提交的

            // TODO: 属于 free 类型的门里面的图书不要参与处理

            // ProgressWindow progress = null;
            //string patron_name = "";
            //patron_name = _patron.PatronName;

            // 先尽量执行还书请求，再报错说无法进行借书操作(记入错误日志)
            MessageDocument doc = new MessageDocument();

            LibraryChannel channel = App.CurrentApp.GetChannel();
            try
            {
                // ClearEntitiesError();

                /*
                if (progress != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Value = 0;
                        progress.ProgressBar.Minimum = 0;
                        progress.ProgressBar.Maximum = actions.Count;
                    }));
                }
                */
                int index = 0;
                func_setProgress?.Invoke(0, actions.Count, index);

                // TODO: 准备工作：把涉及到的 Entity 对象的字段填充完整
                // 检查 PII 是否都具备了

                int success_count = 0;
                List<string> errors = new List<string>();
                List<string> borrows = new List<string>();
                List<string> returns = new List<string>();
                //List<string> warnings = new List<string>();
                foreach (ActionInfo info in actions)
                {
                    string action = info.Action;
                    Entity entity = info.Entity;

                    string action_name = "借书";
                    if (action == "return")
                        action_name = "还书";
                    else if (action == "renew")
                        action_name = "续借";
                    else if (action == "transfer")
                        action_name = "转移";

                    // 借书操作必须要有读者卡
                    if (action == "borrow")
                    {
                        if (patron_filled == false)
                        {
                            // 界面警告
                            errors.Add($"册 '{entity.PII}' 无法进行借书请求");
                            // 写入错误日志
                            WpfClientInfo.WriteInfoLog($"册 '{entity.PII}' 无法进行借书请求");
                            continue;
                        }

                        if (string.IsNullOrEmpty(patronBarcode))
                        {
                            return new SubmitResult
                            {
                                Value = -1,
                                ErrorInfo = $"请先在读卡器上放好读者卡，再进行{action_name}"
                            };
                        }
                    }

                    long lRet = 0;
                    string strError = "";
                    string[] item_records = null;
                    string[] biblio_records = null;
                    BorrowInfo borrow_info = null;

                    if (action == "borrow" || action == "renew")
                    {
                        // TODO: 智能书柜要求强制借书。如果册操作前处在被其他读者借阅状态，要自动先还书再进行借书

                        entity.Waiting = true;
                        lRet = channel.Borrow(null,
                            action == "renew",
                            patronBarcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            null,
                            "item,reader,biblio,overflowable", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "summary",
                            out biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out borrow_info,
                            out strError);
                    }
                    else if (action == "return")
                    {
                        /*
                        // TODO: 增加检查 EAS 现有状态功能，如果已经是 true 则不用修改，后面 API 遇到出错后也不要回滚 EAS
                        // return 操作，提前修改 EAS
                        // 注: 提前修改 EAS 的好处是比较安全。相比 API 执行完以后再修改 EAS，提前修改 EAS 成功后，无论后面发生什么，读者都无法拿着这本书走出门禁
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, action == "return");
                            if (result.Value == -1)
                            {
                                entity.SetError($"{action_name}时修改 EAS 动作失败: {result.ErrorInfo}", "red");
                                errors.Add($"册 '{entity.PII}' {action_name}时修改 EAS 动作失败: {result.ErrorInfo}");
                                continue;
                            }
                        }
                        */
                        // 智能书柜不使用 EAS 状态。可以考虑统一修改为 EAS Off 状态？

                        entity.Waiting = true;
                        lRet = channel.Return(null,
                            "return",
                            "", // _patron.Barcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            "item,reader,biblio", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "summary",
                            out biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out ReturnInfo return_info,
                            out strError);
                    }
                    else if (action == "transfer")
                    {
                        // currentLocation 元素内容。格式为 馆藏地:架号
                        // 注意馆藏地和架号字符串里面不应包含逗号和冒号
                        string currentLocation = info.CurrentShelfNo;
                        entity.Waiting = true;
                        lRet = channel.Return(null,
                            "transfer",
                            "", // _patron.Barcode,
                            entity.PII,
                            entity.ItemRecPath,
                            false,
                            $"item,biblio,currentLocation:{StringUtil.EscapeString(currentLocation, ":,")}", // style,
                            "xml", // item_format_list
                            out item_records,
                            "xml",
                            out string[] reader_records,
                            "",
                            out biblio_records,
                            out string[] dup_path,
                            out string output_reader_barcode,
                            out ReturnInfo return_info,
                            out strError);
                    }

                    /*
                    // testing
                    lRet = -1;
                    strError = "testing";
                    channel.ErrorCode = ErrorCode.AccessDenied;
                    */

                    /*
                    if (progress != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            progress.ProgressBar.Value++;
                        }));
                    }
                    */
                    func_setProgress?.Invoke(-1, -1, ++index);

                    if (biblio_records != null && biblio_records.Length > 0)
                        entity.Title = biblio_records[0];

                    string title = entity.PII;
                    if (string.IsNullOrEmpty(entity.Title) == false)
                        title += " (" + entity.Title + ")";

                    if (action == "borrow" || action == "return")
                    {
                        // 把 _adds 和 _removes 归入 _all
                        // 一边处理一边动态修改 _all?
                        if (action == "return")
                            ShelfData.Add(ShelfData.All, entity);
                        else
                            ShelfData.Remove(ShelfData.All, entity);

                        ShelfData.Remove(ShelfData.Adds, entity);
                        ShelfData.Remove(ShelfData.Removes, entity);
                    }

                    string resultType = "succeed";
                    if (lRet == -1)
                        resultType = "error";
                    else if (lRet == 1)
                        resultType = "information";
                    MessageItem messageItem = new MessageItem
                    {
                        Operation = action,
                        ResultType = resultType,
                        ErrorCode = channel.ErrorCode.ToString(),
                        ErrorInfo = strError,
                        Entity = entity,
                    };
                    doc.Add(messageItem);

                    // 微调
                    if (lRet == 0 && action == "return")
                        messageItem.ErrorInfo = "";

                    if (lRet == -1)
                    {
                        /*
                        // return 操作如果 API 失败，则要改回原来的 EAS 状态
                        if (action == "return")
                        {
                            var result = SetEAS(entity.UID, entity.Antenna, false);
                            if (result.Value == -1)
                                strError += $"\r\n并且复原 EAS 状态的动作也失败了: {result.ErrorInfo}";
                        }
                        */

                        if (action == "return")
                        {
                            if (channel.ErrorCode == ErrorCode.NotBorrowed)
                            {
                                // TODO: 这里不知是普通状态还是 warning 合适。warning 是否比较强烈了
                                messageItem.ResultType = "warning";
                                // messageItem.ErrorCode = channel.ErrorCode.ToString();
                                // 界面警告
                                //warnings.Add($"册 '{title}' (尝试还书时发现未曾被借出过): {strError}");
                                // 写入错误日志
                                WpfClientInfo.WriteInfoLog($"读者 {patron_name} {patronBarcode} 尝试还回册 '{title}' 时: {strError}");
                                continue;
                            }
                        }


                        if (action == "transfer")
                        {
                            if (channel.ErrorCode == ErrorCode.NotChanged)
                            {
                                // 不出现在结果中
                                doc.Remove(messageItem);

                                // 改为警告
                                messageItem.ResultType = "warning";
                                // messageItem.ErrorCode = channel.ErrorCode.ToString();
                                // 界面警告
                                //warnings.Add($"册 '{title}' (尝试转移时发现没有发生修改): {strError}");
                                // 写入错误日志
                                WpfClientInfo.WriteInfoLog($"转移册 '{title}' 时: {strError}");
                                continue;
                            }
                        }

                        entity.SetError($"{action_name}操作失败: {strError}", "red");
                        // TODO: 这里最好用 title
                        errors.Add($"册 '{title}': {strError}");
                        continue;
                    }

                    if (action == "borrow")
                    {
                        if (borrow_info.Overflows != null && borrow_info.Overflows.Length > 0)
                        {
                            // 界面警告
                            // TODO: 可以考虑归入 overflows 单独语音警告处理。语音要简洁。详细原因可出现在文字警告中
                            // warnings.Add($"册 '{title}' (借书操作发生溢出，请于当日内还书): {string.Join("; ", borrow_info.Overflows)}");

                            // TODO: 详细原因文字可否用稍弱的字体效果来显示？
                            messageItem.ErrorInfo = $"借书操作超越许可，请将本册放回书柜。详细原因： {string.Join("; ", borrow_info.Overflows)}";
                            messageItem.ResultType = "warning";
                            messageItem.ErrorCode = "overflow";
                            // 写入错误日志
                            WpfClientInfo.WriteInfoLog($"读者 {patron_name} {patronBarcode} 借阅 '{title}' 时发生超越许可: {strError}");
                        }
                    }

                    if (action == "borrow")
                        borrows.Add(title);
                    if (action == "return")
                        returns.Add(title);

                    /*
                    // borrow 操作，API 之后才修改 EAS
                    // 注: 如果 API 成功但修改 EAS 动作失败(可能由于读者从读卡器上过早拿走图书导致)，读者会无法把本册图书拿出门禁。遇到此种情况，读者回来补充修改 EAS 一次即可
                    if (action == "borrow")
                    {
                        var result = SetEAS(entity.UID, entity.Antenna, action == "return");
                        if (result.Value == -1)
                        {
                            entity.SetError($"虽然{action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}", "yellow");
                            errors.Add($"册 '{entity.PII}' {action_name}操作成功，但修改 EAS 动作失败: {result.ErrorInfo}");
                        }
                    }
                    */

                    // 刷新显示
                    {
                        if (item_records?.Length > 0)
                            entity.SetData(entity.ItemRecPath, item_records[0]);

                        if (entity.Error != null)
                            continue;

                        string message = $"{action_name}成功";
                        if (lRet == 1 && string.IsNullOrEmpty(strError) == false)
                            message = strError;
                        entity.SetError(message,
                            lRet == 1 ? "yellow" : "green");
                        success_count++;
                        // 刷新显示。特别是一些关于借阅日期，借期，应还日期的内容
                    }
                }

                /*
                if (progress != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        progress.ProgressBar.Visibility = Visibility.Collapsed;
                        // progress.ProgressBar.Value = progress.ProgressBar.Maximum;
                    }));
                }*/
                func_setProgress?.Invoke(-1, -1, -1);   // hide progress bar

                string speak = "";
                {
                    /*
                    if (progress != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            // DisplayError(ref progress, message, backColor);
                            progress.MessageDocument = doc.BuildDocument(patron_name, 18, out speak);
                            progress = null;
                        }));
                    }
                    */

                    // 重新装载读者信息和显示
                    // DoorItem.DisplayCount(_all, _adds, _removes, App.CurrentApp.Doors);
                    ShelfData.RefreshCount();

                    // App.CurrentApp.Speak(speak);
                }

                return new SubmitResult
                {
                    Value = 1,
                    MessageDocument = doc,
                    // SpeakContent = speak
                }; // new NormalResult { Value = success_count };
            }
            finally
            {
                App.CurrentApp.ReturnChannel(channel);
                /*
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (progress != null)
                        progress.Close();
                }));
                */
            }
        }

    }

    public class ActionInfo
    {
        public Entity Entity { get; set; }
        public string Action { get; set; }  // borrow/return/transfer
        public string CurrentShelfNo { get; set; }  // 当前架号。transfer 动作会用到
    }

    public class SubmitResult : NormalResult
    {
        public MessageDocument MessageDocument { get; set; }
        // public string SpeakContent { get; set; }
    }
}
