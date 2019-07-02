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

        Entity FindEntity(string uid)
        {
            foreach (Entity entity in this)
            {
                if (entity.UID == uid)
                    return entity;
            }
            return null;
        }

        // 修改一个 entity 中和 EAS 有关的内存数据
        public bool SetEasData(string uid, bool enable)
        {
            Entity entity = FindEntity(uid);
            if (entity == null)
                return false;
            return entity.SetEasData(enable);
        }

        public Entity Add(TagAndData data, bool auto_update = true)
        {
            // 查重
            Entity entity = FindEntity(data.OneTag.UID);
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
                TagInfo = data.OneTag.TagInfo
            };
            this.Add(entity);

            SetPII(entity);

            entity.SetError(data.Error);
            return entity;
        }

        // 更新一个事项内容
        // return:
        //      null 没有找到
        //      其他    找到并更新了
        public Entity Update(TagAndData data, bool auto_add = true)
        {
            Entity entity = FindEntity(data.OneTag.UID);
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

            SetPII(entity);
            // 如何触发下一步的获取和显示?

            entity.SetError(data.Error);
            return entity;
        }

        static void SetPII(Entity entity)
        {
            // 刷新 PII
            if (string.IsNullOrEmpty(entity.PII)
                && entity.TagInfo != null)
            {
                string pii = "";

                LogicChip chip = LogicChip.From(entity.TagInfo.Bytes,
    (int)entity.TagInfo.BlockSize,
    "" // tag.TagInfo.LockStatus
    );
                pii = chip.FindElement(ElementOID.PII)?.Text;
                entity.PII = pii;
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

        public bool FillFinished { get; set; }
#if NO
        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged("Error");
            }
        }

        bool _waiting = true;

        public bool Waiting
        {
            get
            {
                return _waiting;
            }
            set
            {
                _waiting = value;
                OnPropertyChanged("Waiting");
            }
        }

#endif


#if NO
        void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

#endif

        // internal string ItemXml { get; set; }


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

            // TODO: 设置借书日期、期限、应还日期等
            string borrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
            string returningDate = DomUtil.GetElementText(dom.DocumentElement, "returningDate");
            string period = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            if (string.IsNullOrEmpty(borrowDate))
                this.BorrowInfo = null;
            else
            {
                borrowDate = ToDate(borrowDate);
                returningDate = ToDate(returningDate);
                this.BorrowInfo = $"借书日期:\t{borrowDate}\n期限:\t\t{period}\n应还日期:\t{returningDate}";
            }
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
