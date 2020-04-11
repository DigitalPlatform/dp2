﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using dp2SSL.Models;

using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace dp2SSL
{
    public class EntityCollection : ObservableCollection<Entity>
    {
        public string Style { get; set; }

        public void Remove(string uid)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (uid == this[i].UID)
                {
                    this.RemoveAt(i);
                    return;
                }
            }
        }

        public Entity FindEntityByUID(string uid)
        {
            foreach (Entity entity in this)
            {
                if (entity.UID == uid)
                    return entity;
            }
            return null;
        }

        // 2019/8/6
        public Entity FindEntityByPII(string pii)
        {
            foreach (Entity entity in this)
            {
                if (entity.PII == pii)
                    return entity;
            }
            return null;
        }

        // 修改一个 entity 中和 EAS 有关的内存数据
        public bool SetEasData(string uid, bool enable)
        {
            Entity entity = FindEntityByUID(uid);
            if (entity == null)
                return false;
            return entity.SetEasData(enable);
        }

        // 根据已知的 PII 在结合中添加一个 Entity 元素
        public Entity Add(string pii, bool auto_update = true)
        {
            // 查重
            Entity entity = FindEntityByPII(pii);
            if (entity != null)
            {
                //if (auto_update)
                //    Update(data);
                return entity;
            }

            entity = new Entity
            {
                Container = this,
                PII = pii,
                OnShelf = false,
                UID = null,
                TagInfo = null
            };
            this.Add(entity);

            entity.FillFinished = false;

            // SetPII(entity);

            entity.SetError(null);
            return entity;
        }

        public Entity OffShelf(TagAndData data)
        {
            if (data.OneTag.TagInfo == null)
                return null;

            string pii = GetPII(data.OneTag.TagInfo);
            if (string.IsNullOrEmpty(pii))
                return null;

            var entity = FindEntityByPII(pii);
            if (entity == null)
                return null;

            entity.OnShelf = false;

            // TODO: 如果本来就不是当前馆藏地的(被误放进来的)，要从列表中删除
            if (entity.BelongToCurrentShelf == false)
                this.Remove(entity);

            return entity;
        }

        // 放到架上
        public Entity OnShelf(TagAndData data)
        {
            if (data.OneTag.TagInfo == null)
                return null;

            string pii = GetPII(data.OneTag.TagInfo);
            if (string.IsNullOrEmpty(pii))
                return null;

            var entity = FindEntityByPII(pii);
            if (entity != null)
            {
                // 馆藏地属于本书架的图书
                entity.BelongToCurrentShelf = true;
            }
            else
            {
                // 这是馆藏地不属于本书架的图书
                entity = Add(data, true);
                entity.BelongToCurrentShelf = false;
            }

            entity.OnShelf = true;
            entity.UID = data.OneTag.UID;
            return entity;
        }

        public Entity Add(TagAndData data, bool auto_update = true)
        {
            // 查重
            Entity entity = FindEntityByUID(data.OneTag.UID);
            if (entity != null)
            {
                if (auto_update)
                    Update(data);
                return entity;
            }

            entity = new Entity
            {
                Container = this,
                UID = data.OneTag.UID,
                Antenna = data.OneTag.AntennaID.ToString(),
                ReaderName = data.OneTag.ReaderName,
                TagInfo = data.OneTag.TagInfo
            };
            this.Add(entity);

            // Exception:
            //      可能会抛出异常 ArgumentException
            SetPII(entity);

            entity.AppendError(data.Error);
            return entity;
        }

        // 更新一个事项内容
        // return:
        //      null 没有找到
        //      其他    找到并更新了
        public Entity Update(TagAndData data, bool auto_add = true)
        {
            Entity entity = FindEntityByUID(data.OneTag.UID);
            if (entity == null)
            {
                if (auto_add)
                    return Add(data, false);
                return null;
            }
            if (data.OneTag != null
                && data.OneTag.TagInfo != null
                && entity.TagInfo == null)
            {
                entity.TagInfo = data.OneTag.TagInfo;
            }
            else if (data.OneTag != null
                && data.OneTag?.TagInfo == null && entity.TagInfo != null)
            {
                entity.TagInfo = null;
                entity.PII = null;
                entity.FillFinished = false;
            }

            // 2019/9/29
            if (data.OneTag != null)
            {
                var id = data.OneTag.AntennaID.ToString();
                if (entity.Antenna != id)
                    entity.Antenna = id;
                var readerName = data.OneTag.ReaderName;
                if (entity.ReaderName != readerName)
                    entity.ReaderName = readerName;
            }

            // Exception:
            //      可能会抛出异常 ArgumentException
            SetPII(entity);
            // 如何触发下一步的获取和显示?

            entity.AppendError(data.Error);
            return entity;
        }

        public static string GetPII(TagInfo tagInfo)
        {
            // Exception:
            //      可能会抛出异常 ArgumentException TagDataException
            LogicChip chip = LogicChip.From(tagInfo.Bytes,
(int)tagInfo.BlockSize,
"" // tagInfo.LockStatus
);
            return chip.FindElement(ElementOID.PII)?.Text;
        }

        // 2020/4/11
        public static void SetPII(Entity entity, string pii)
        {
            entity.PII = pii;
        }

        // 根据 entity 中的 RFID 信息设置 PII
        // 注：如果标签内容解析错误，Entity.Error 中会返回有报错信息
        // Exception:
        //      可能会抛出异常 ArgumentException
        public static void SetPII(Entity entity)
        {
            // 刷新 PII
            if (string.IsNullOrEmpty(entity.PII)
                && entity.TagInfo != null)
            {
                string pii = "";

                try
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
        (int)entity.TagInfo.BlockSize,
        "" // tag.TagInfo.LockStatus
        );
                    pii = chip.FindElement(ElementOID.PII)?.Text;
                    entity.PII = pii;
                }
                catch (TagInfoException ex)
                {
                    // entity.UID 应该有值
                    entity.SetError($"标签内容解析错误: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // entity.UID 应该有值
                    entity.SetError($"标签内容解析错误(1): {ex.Message}");
                }
            }
            else if (string.IsNullOrEmpty(entity.PII) == false
                && entity.TagInfo == null)
            {
                entity.PII = null;  // 2019/7/2
            }
        }

        // 第一阶段：填充 UID 和 PII
        // parameters:
        //      new_entities    返回本次新增的部分 Entity。调用前如果为 null，则表示不希望返回信息
        // return:
        //      是否发生了变动
        public bool Refresh(List<OneTag> tags,
            ref List<Entity> new_entities)
        {
            bool changed = false;
            List<Entity> need_delete = new List<Entity>();
            foreach (Entity entity in this)
            {
                OneTag tag = tags.Find((o) =>
                {
                    if (o.UID == entity.UID)
                        return true;
                    return false;
                });
                if (tag == null)
                {
                    // 多出来的，需要删除
                    need_delete.Add(entity);
                }
                else
                {
                    // 已经存在的
                    tags.Remove(tag);
                }
            }

            // 删除
            foreach (Entity entity in need_delete)
            {
                this.Remove(entity);
                changed = true;
            }

            // 新添加
            foreach (OneTag tag in tags)
            {
                string pii = "";
                if (tag.TagInfo != null)
                {
                    // Exception:
                    //      可能会抛出异常 ArgumentException TagDataException
                    LogicChip chip = LogicChip.From(tag.TagInfo.Bytes,
        (int)tag.TagInfo.BlockSize,
        "" // tag.TagInfo.LockStatus
        );
                    pii = chip.FindElement(ElementOID.PII)?.Text;
                }
                else
                {
                    // 尝试重新获取一次
                }

                Entity entity = new Entity
                {
                    TagInfo = tag.TagInfo,
                    UID = tag.UID,
                    PII = pii,
                    Container = this,
                };
                this.Add(entity);
                changed = true;

                if (new_entities != null)
                    new_entities.Add(entity);
            }

            return changed;
        }
    }

    public class Entity : RfidItem
    {
        /*
ERROR dp2SSL 2019-11-28 14:35:59,100 - InitialShelfEntities() 出现异常: Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
   在 dp2SSL.Entity.Clone()
   在 dp2SSL.DoorItem.Update(EntityCollection collection, List`1 items)
   在 dp2SSL.DoorItem.<>c__DisplayClass79_1.<DisplayCount>b__0()
   在 System.Windows.Threading.Dispatcher.Invoke(Action callback, DispatcherPriority priority, CancellationToken cancellationToken, TimeSpan timeout)
   在 System.Windows.Threading.Dispatcher.Invoke(Action callback)
   在 dp2SSL.DoorItem.DisplayCount(List`1 entities, List`1 adds, List`1 removes, List`1 errors, List`1 _doors)
   在 dp2SSL.ShelfData.RefreshCount()
   在 dp2SSL.ShelfData.<InitialShelfEntities>d__70.MoveNext()
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter.GetResult()
   在 dp2SSL.PageShelf.<InitialShelfEntities>d__45.MoveNext()
--- 引发异常的上一位置中堆栈跟踪的末尾 ---
   在 System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
   在 System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   在 System.Runtime.CompilerServices.TaskAwaiter.GetResult()
   在 dp2SSL.PageShelf.<PageShelf_Loaded>d__9.MoveNext()
   * */
        public new Entity Clone()
        {
            Entity dup = new Entity();
            dup.Container = this.Container;
            if (this.TagInfo != null)
                dup.TagInfo = this.TagInfo.Clone();
            else
                dup.TagInfo = null;
            dup.ItemRecPath = this.ItemRecPath;
            dup.Title = this.Title;
            dup.Location = this.Location;
            dup.BorrowInfo = this.BorrowInfo;
            dup.ShelfState = this.ShelfState;
            dup.OnShelf = this.OnShelf;
            dup.BelongToCurrentShelf = this.BelongToCurrentShelf;
            dup.FillFinished = this.FillFinished;
            dup.State = this.State;
            dup.CurrentLocation = this.CurrentLocation;
            dup.ShelfNo = this.ShelfNo;

            dup.PII = this.PII;
            dup.UID = this.UID;
            dup.Error = this.Error;
            dup.ErrorColor = this.ErrorColor;
            dup.Waiting = this.Waiting;
            dup.ReaderName = this.ReaderName;
            dup.Antenna = this.Antenna;

            return dup;
        }

        public EntityCollection Container { get; set; }

        public TagInfo TagInfo { get; set; }

        private string _itemRecPath;

        public string ItemRecPath
        {
            get => _itemRecPath;
            set
            {
                if (_itemRecPath != value)
                {
                    _itemRecPath = value;
                    OnPropertyChanged("ItemRecPath");
                }
            }
        }

#if NO
        private string _uid;
        private string _error = null;   // "test error line asdljasdkf; ;jasldfjasdjkf aasdfasdf";

        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged("UID");
            }
        }
#endif

        private string _title;

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        private string _location;

        // 原始馆藏地
        // 从册记录 location 元素中取出来的，并去除了 #reserve 部分的字符串
        public string Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged("Location");
                }
            }
        }

        private string _currentLocation = "";

        // 当前位置
        // 从册记录 currentLocation 元素中取出来的字符串
        public string CurrentLocation
        {
            get => _currentLocation;
            set
            {
                if (_currentLocation != value)
                {
                    _currentLocation = value;
                    OnPropertyChanged("CurrentLocation");
                }
            }
        }

        private string _shelfNo = "";

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
        private string _borrowInfo;

        public string BorrowInfo
        {
            get => _borrowInfo;
            set
            {
                if (_borrowInfo != value)
                {
                    _borrowInfo = value;
                    OnPropertyChanged("BorrowInfo");
                }
            }
        }

        string _shelfState = null;

        // 智能书架事项状态。各种值的组合: onshelf/currentshelf
        public string ShelfState
        {
            get
            {
                return _shelfState;
            }
            set
            {
                if (_shelfState != value)
                {
                    _shelfState = value;
                    OnPropertyChanged("ShelfState");
                }
            }
        }

        // 是否在智能书架架上
        public bool OnShelf
        {
            get
            {
                return !StringUtil.IsInList("offshelf", this.ShelfState);
            }
            set
            {
                string s = this.ShelfState;
                if (s != null)
                    StringUtil.SetInList(ref s, "offshelf", !value);
                this.ShelfState = s;
            }
        }

        // (馆藏地)是否属于当前智能书架
        public bool BelongToCurrentShelf
        {
            get
            {
                return StringUtil.IsInList("currentshelf", this.ShelfState);
            }
            set
            {
                string s = this.ShelfState;
                if (s == null)
                    s = "";
                StringUtil.SetInList(ref s, "currentshelf", value);
                this.ShelfState = s;
            }
        }

        public bool FillFinished { get; set; }

        public void SetData(string item_recpath, string xml)
        {
            this.ItemRecPath = item_recpath;

            // FontAwesome.WPF.FontAwesomeIcon.HandGrabOutline
            if (string.IsNullOrEmpty(xml))
            {
                this.State = null;
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            string borrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
            if (string.IsNullOrEmpty(borrower))
                this.State = "onshelf";
            else
                this.State = "borrowed";

            // 设置借书日期、期限、应还日期等
            string location = DomUtil.GetElementText(dom.DocumentElement, "location");
            location = StringUtil.GetPureLocation(location);
            this.Location = location;

            // 2019/11/24
            string currentLocation = DomUtil.GetElementText(dom.DocumentElement, "currentLocation");
            this.CurrentLocation = currentLocation;

            string shelfNo = DomUtil.GetElementText(dom.DocumentElement, "shelfNo");
            this.ShelfNo = shelfNo;

            string borrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
            string returningDate = DomUtil.GetElementText(dom.DocumentElement, "returningDate");
            string period = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            if (string.IsNullOrEmpty(borrowDate))
                this.BorrowInfo = null;
            else
            {
                this.BorrowInfo = $"借书日期:\t{ToDate(borrowDate)}\n期限:\t\t{period}\n应还日期:\t{ToDate(returningDate)}";
            }

            // 2019/11/9
            // 判断是否超期
            bool isOverdue = false;
            if (string.IsNullOrEmpty(returningDate) == false)
            {
                DateTime time = DateTimeUtil.FromRfc1123DateTimeString(returningDate);
                TimeSpan delta = DateTime.Now - time.ToLocalTime();
                if (period.IndexOf("hour") != -1)
                {
                    // TODO: 如果没有册条码号则用 refID 代替
                    if (delta.Hours > 0)
                        isOverdue = true;
                    // overdue_infos.Add($"册 {strItemBarcode} 已超期 {delta.Hours} 小时");
                }
                else
                {
                    if (delta.Days > 0)
                        isOverdue = true;
                    // overdue_infos.Add($"册 {strItemBarcode} 已超期 {delta.Days} 天");
                }
            }

            if (isOverdue)
                this.State += ",overdue";

            string overflow = DomUtil.GetElementText(dom.DocumentElement, "overflow");
            if (string.IsNullOrEmpty(overflow) == false)
                this.State += ",overflow";
        }

        static string ToDate(string strTime)
        {
            if (string.IsNullOrEmpty(strTime))
                return "";
            // TODO: 还可以优化为 (今天) 之类的简略说法
            return DateTimeUtil.FromRfc1123DateTimeString(strTime).ToLocalTime().ToString("d");
        }

        // 修改内存中和 EAS 有关的状态
        public bool SetEasData(bool enable)
        {
            if (this.TagInfo == null)
                return false;
            TagList.SetTagInfoEAS(this.TagInfo, enable);
            return true;
        }
    }

}
