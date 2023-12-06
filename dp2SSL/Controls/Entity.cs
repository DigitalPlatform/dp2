using System;
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
using DigitalPlatform;
using DigitalPlatform.Core;

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

        static bool IsEqual(string s1, string s2)
        {
            if (s1 == s2)
                return true;
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                return true;
            return false;
        }

        // 2019/8/6
        public Entity FindEntityByPII(string pii, string oi, string aoi)
        {
            foreach (Entity entity in this)
            {
                if (entity.PII == pii
                    && IsEqual(entity.OI, oi)
                    && IsEqual(entity.AOI, aoi))
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

        // 根据已知的 PII 在集合中添加一个 Entity 元素
        public Entity Add(string pii, string oi, string aoi, bool auto_update = true)
        {
            // 查重
            Entity entity = FindEntityByPII(pii, oi, aoi);
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
                OI = oi,
                AOI = aoi,
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

            var result = GetPII(data.OneTag.TagInfo);
            if (string.IsNullOrEmpty(result.PII))
                return null;

            var entity = FindEntityByPII(result.PII, result.OI, result.AOI);
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

            var result = GetPII(data.OneTag.TagInfo);
            if (string.IsNullOrEmpty(result.PII))
                return null;

            var entity = FindEntityByPII(result.PII, result.OI, result.AOI);
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
                TagInfo = data.OneTag.TagInfo,
                Protocol = data.OneTag.Protocol,    // 2023/12/4
            };
            this.Add(entity);

            // Exception:
            //      可能会抛出异常 ArgumentException
            SetPII(entity);

            if (string.IsNullOrEmpty(data.Error) == false)
                entity.AppendError(data.Error);
            return entity;
        }

        // 2023/12/1
        // 更新一个事项内容
        // 注: 本函数适用于 UID 发生改变的情况
        // return:
        //      null 没有找到(注: 没有更新也返回 null)
        //      其他    找到并更新了
        public Entity Update(TagAndData old_data,
            TagAndData new_data,
            bool auto_add = true)
        {
            // 检查 old_data 和 new_data 的 UID 是否一样
            if (old_data.OneTag.UID == new_data.OneTag.UID)
                return Update(new_data, auto_add);

            Entity old_entity = FindEntityByUID(old_data.OneTag.UID);
            Entity new_entity = FindEntityByUID(new_data.OneTag.UID);
            if (old_entity == null && new_entity == null)
            {
                if (auto_add)
                    return Add(new_data, false);
                return null;
            }

            // new_data 的 UID 已经存在了，但 old_data 的 UID 不存在
            if (old_entity == null && new_entity != null)
            {
                return Update(new_data, auto_add);
            }

            // old_data 和 new_data 的 UID 都已经存在了
            if (old_entity != null && new_entity != null)
            {
                // 删除 old_entity
                this.Remove(old_entity);
                return Update(new_data, auto_add);
            }

            // 一般情况
            Debug.Assert(old_entity != null && new_entity == null);

            {
                // 删除 old_entity
                this.Remove(old_entity);
                var result = Update(new_data, auto_add);
                // 把 old_entity 的 Error 复制给 new_entity
                result.Error = old_entity.Error;
                result.ErrorColor = old_entity.ErrorColor;
                return result;
            }
        }


        // 更新一个事项内容
        // 注: 本函数适用于 UID 没有改变的情况
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

            // entity.BuildError("data", data.Error, null);
            entity.AppendError(data.Error);
            return entity;
        }

        public class GetPIIResult : NormalResult
        {
            public string PII { get; set; }
            public string OI { get; set; }
            public string AOI { get; set; }
        }

        public static GetPIIResult GetPII(TagInfo tagInfo)
        {
            LogicChip chip = null;

            if (tagInfo.Protocol == InventoryInfo.ISO15693)
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                chip = LogicChip.From(tagInfo.Bytes,
    (int)tagInfo.BlockSize,
    "" // tagInfo.LockStatus
    );
            }
            else if (tagInfo.Protocol == InventoryInfo.ISO18000P6C)
            {
                // 2023/11/3
                // 注1: taginfo.EAS 在调用后可能被修改
                // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                var chip_info = RfidTagList.GetUhfChipInfo(tagInfo);
                chip = chip_info.Chip;
            }
            else
            {
                // 无法识别的 RFID 标签格式
                // TODO: 抛出异常？
            }

            return new GetPIIResult
            {
                PII = chip?.FindElement(ElementOID.PII)?.Text,
                OI = chip?.FindElement(ElementOID.OI)?.Text,
                AOI = chip?.FindElement(ElementOID.AOI)?.Text,
            };
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
                    LogicChip chip = null;
                    // 2023/11/3
                    if (entity.TagInfo.Protocol == InventoryInfo.ISO15693)
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        chip = LogicChip.From(entity.TagInfo.Bytes,
            (int)entity.TagInfo.BlockSize,
            "" // tag.TagInfo.LockStatus
            );
                        pii = chip.FindElement(ElementOID.PII)?.Text;
                    }
                    else if (entity.TagInfo.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // 注1: taginfo.EAS 在调用后可能被修改
                        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                        var chip_info = RfidTagList.GetUhfChipInfo(entity.TagInfo);
                        pii = chip_info.PII;
                        chip = chip_info.Chip;
                    }
                    else
                    {
                        // 2023/11/3
                        entity.SetError($"无法识别的 RFID 标签协议 '{entity.TagInfo.Protocol}'");
                        return;
                    }

                    entity.PII = pii;
                    // 2021/4/2
                    entity.OI = chip?.FindElement(ElementOID.OI)?.Text;
                    entity.AOI = chip?.FindElement(ElementOID.AOI)?.Text;
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
                entity.OI = null;
                entity.AOI = null;
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
                string oi = "";
                string aoi = "";
                if (tag.TagInfo != null)
                {
                    LogicChip chip = null;
                    if (tag.Protocol == InventoryInfo.ISO15693)
                    {
                        // Exception:
                        //      可能会抛出异常 ArgumentException TagDataException
                        chip = LogicChip.From(tag.TagInfo.Bytes,
            (int)tag.TagInfo.BlockSize,
            "" // tag.TagInfo.LockStatus
            );
                    }
                    else if (tag.Protocol == InventoryInfo.ISO18000P6C)
                    {
                        // 2023/11/3
                        // 注1: taginfo.EAS 在调用后可能被修改
                        // 注2: 本函数不再抛出异常。会在 ErrorInfo 中报错
                        var chip_info = RfidTagList.GetUhfChipInfo(tag.TagInfo);
                        chip = chip_info.Chip;
                    }
                    else
                    {
                        // 无法识别的 RFID 标签协议
                        // TODO: 抛出异常？
                    }

                    pii = chip?.FindElement(ElementOID.PII)?.Text;

                    // 2021/4/2
                    oi = chip?.FindElement(ElementOID.OI)?.Text;
                    aoi = chip?.FindElement(ElementOID.AOI)?.Text;
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
                    OI = oi,
                    AOI = aoi,
                    Container = this,
                };
                this.Add(entity);
                changed = true;

                if (new_entities != null)
                    new_entities.Add(entity);
            }

            return changed;
        }

        // 2020/11/5
        // 移动到列表末尾
        public void MoveToTail(Entity entity)
        {
            if (this.Remove(entity) == true)
            {
                this.Add(entity);
            }
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
            dup.AccessNo = this.AccessNo;

            dup.PII = this.PII;
            dup.OI = this.OI;
            dup.AOI = this.AOI;
            dup.UID = this.UID;
            dup.Error = this.Error;
            dup.ErrorCode = this.ErrorCode;
            dup.ErrorColor = this.ErrorColor;
            dup.Waiting = this.Waiting;
            dup.ReaderName = this.ReaderName;
            dup.Antenna = this.Antenna;
            dup.Tag = this.Tag;
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

        private string _accessNo;

        // 索取号
        public string AccessNo
        {
            get => _accessNo;
            set
            {
                if (_accessNo != value)
                {
                    _accessNo = value;
                    OnPropertyChanged("AccessNo");
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

        // TODO: 检查所有引用的位置，增加处理 result.Value == -1 的代码逻辑
        public NormalResult SetData(string item_recpath,
            string xml,
            DateTime now)
        {
            List<string> errors = new List<string>();

            this.ItemRecPath = item_recpath;

            // FontAwesome.WPF.FontAwesomeIcon.HandGrabOutline
            if (string.IsNullOrEmpty(xml))
            {
                this.State = null;
                return new NormalResult();
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            string borrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");
            if (string.IsNullOrEmpty(borrower))
                this.State = "onshelf";
            else
                this.State = "borrowed";

            // 2021/1/25
            // XML 册记录中的原始 state 值
            {
                string state = DomUtil.GetElementText(dom.DocumentElement, "state");
                if (string.IsNullOrEmpty(state) == false)
                    this.State += "," + state;
            }

            // 设置借书日期、期限、应还日期等
            string location = DomUtil.GetElementText(dom.DocumentElement, "location");
            location = StringUtil.GetPureLocation(location);
            this.Location = location;

            // 2019/11/24
            string currentLocation = DomUtil.GetElementText(dom.DocumentElement, "currentLocation");
            this.CurrentLocation = currentLocation;

            string shelfNo = DomUtil.GetElementText(dom.DocumentElement, "shelfNo");
            this.ShelfNo = shelfNo;

            // 2020/8/20
            this.AccessNo = DomUtil.GetElementText(dom.DocumentElement, "accessNo");

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
                // DateTime time = DateTimeUtil.FromRfc1123DateTimeString(returningDate);
                if (TryParseRfc1123(returningDate, out DateTime time) == false)
                    errors.Add($"returningDate:'{returningDate}' 时间字符串不合法");
                else
                {
                    TimeSpan delta = /*DateTime.Now*/now - time.ToLocalTime();
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
            }

            if (isOverdue)
                this.State += ",overdue";

            string overflow = DomUtil.GetElementText(dom.DocumentElement, "overflow");
            if (string.IsNullOrEmpty(overflow) == false)
                this.State += ",overflow";

            if (errors.Count > 0)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = StringUtil.MakePathList(errors, "; ")
                };

            return new NormalResult();
        }

        // 2021/8/31
        public static bool TryParseRfc1123(string rfc1123, out DateTime time)
        {
            try
            {
                time = DateTimeUtil.FromRfc1123DateTimeString(rfc1123);
                return true;
            }
            catch
            {
                time = DateTime.MinValue;
                return false;
            }
        }

        public static bool IsOverdue(
            DateTime now,
            string borrowDate,
            string returningDate,
            string period)
        {
            string strBorrowInfo = "";

            List<string> errors = new List<string>();
            /*
            string borrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
            string returningDate = DomUtil.GetElementText(dom.DocumentElement, "returningDate");
            string period = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            */
            if (string.IsNullOrEmpty(borrowDate))
                strBorrowInfo = null;
            else
            {
                strBorrowInfo = $"借书日期:\t{ToDate(borrowDate)}\n期限:\t\t{period}\n应还日期:\t{ToDate(returningDate)}";
            }

            // 2019/11/9
            // 判断是否超期
            bool isOverdue = false;
            if (string.IsNullOrEmpty(returningDate) == false)
            {
                // DateTime time = DateTimeUtil.FromRfc1123DateTimeString(returningDate);
                if (TryParseRfc1123(returningDate, out DateTime time) == false)
                    errors.Add($"returningDate:'{returningDate}' 时间字符串不合法");
                else
                {
                    TimeSpan delta = /*DateTime.Now*/now - time.ToLocalTime();
                    if (period.IndexOf("hour") != -1)
                    {
                        // TODO: 如果没有册条码号则用 refID 代替
                        if (delta.Hours > 0)
                            isOverdue = true;
                    }
                    else
                    {
                        if (delta.Days > 0)
                            isOverdue = true;
                    }
                }
            }

            return isOverdue;
        }

        public static string ToDate(string strTime)
        {
            // throw new Exception("test");
            try
            {
                if (string.IsNullOrEmpty(strTime))
                    return "";
                // TODO: 还可以优化为 (今天) 之类的简略说法
                return DateTimeUtil.FromRfc1123DateTimeString(strTime).ToLocalTime().ToString("d");
            }
            catch (Exception ex)
            {
                // 2021/8/31
                return $"error:RFC1123 时间字符串 '{strTime}' 不合法: {ex.Message}";
            }
        }

        // 修改内存中和 EAS 有关的状态
        public bool SetEasData(bool enable)
        {
            if (this.TagInfo == null)
                return false;
            RfidTagList.SetTagInfoEAS(this.TagInfo, enable);
            return true;
        }

        public bool GetEas()
        {
            if (this.TagInfo == null)
                return false;

            if (this.TagInfo.Protocol == InventoryInfo.ISO18000P6C)
                return RfidTagList.GetUhfEas(this.UID, out _);
            return this.TagInfo.EAS;
        }

        #region 分类错误处理机制

        List<ErrorItem> _errorItems = null;

        // 构造错误信息
        public string BuildError(string type,
            string error,
            string code)
        {
            var text = ErrorItem.BuildError(ref _errorItems,
                type, error, code);
            this.Error = text;
            return text;
        }

        // 清除所有错误信息
        public void ClearAllError()
        {
            _errorItems = null;
        }

        public string GetError(string typeList)
        {
            if (_errorItems == null)
                return null;
            return ErrorItem.ToString(typeList, _errorItems);
        }

        #endregion
    }


    public class ErrorItem
    {
        public string Type { get; set; }
        public string Error { get; set; }
        public string Code { get; set; }

        // 在既有的错误集合中添加一个新的错误，并整理输出为纯文本
        public static string BuildError(
            ref List<ErrorItem> errors,
            string type,
            string error,
            string code)
        {
            SetError(ref errors, type, error, code);

            return ToString(errors);
        }

        // 设置错误信息
        static void SetError(ref List<ErrorItem> errors,
            string type,
            string error,
            string code)
        {
            // 正好，不用添加了
            if (error == null && errors == null)
                return;
            if (errors == null)
                errors = new List<ErrorItem>();

            Debug.Assert(errors != null);

            var item = errors.Find(o => o.Type == type);
            // 正好不用添加了
            if (item == null && error == null)
                return;
            if (item == null)   // 没有事项就添加
                errors.Add(new ErrorItem
                {
                    Type = type,
                    Error = error,
                    Code = code,
                });
            else if (error == null) // 去掉事项
            {
                Debug.Assert(item != null);
                errors.Remove(item);
            }
            else // 修改现有事项
            {
                Debug.Assert(item != null);
                item.Error = error;
                item.Code = code;
            }
        }

        // 构造为方便显示的纯文本
        public static string ToString(List<ErrorItem> errors,
            string style = "")
        {
            if (errors == null || errors.Count == 0)
                return null;
            var include_type = StringUtil.IsInList("includeType", style);
            List<string> lines = new List<string>();
            foreach (var item in errors)
            {
                if (include_type)
                    lines.Add($"{item.Type}:{item.Error}");
                else
                    lines.Add(item.Error);
            }
            return StringUtil.MakePathList(lines, ";");
        }

        // 获得特定类型的错误信息(显示形态)
        public static string ToString(
            string typeList,
            List<ErrorItem> errors,
            string style = "")
        {
            if (errors == null || errors.Count == 0)
                return null;
            var include_type = StringUtil.IsInList("includeType", style);
            List<string> lines = new List<string>();
            foreach (var item in errors)
            {
                if (StringUtil.IsInList(item.Type, typeList) == false)
                    continue;

                if (include_type)
                    lines.Add($"{item.Type}:{item.Error}");
                else
                    lines.Add(item.Error);
            }
            return StringUtil.MakePathList(lines, ";");
        }
    }
}
