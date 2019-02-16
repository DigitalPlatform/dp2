using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Xml;

namespace dp2SSL
{
    public class EntityCollection : ObservableCollection<Entity>
    {
        // 第一阶段：填充 UID 和 PII
        public bool Refresh(List<OneTag> tags)
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

                Entity entity = new Entity { UID = tag.UID, PII = pii };
                this.Add(entity);
                changed = true;
            }

            return changed;
        }
    }

    public class Entity : RfidItem
    {
        private string _itemRecPath;

        public string ItemRecPath
        {
            get => _itemRecPath;
            set
            {
                _itemRecPath = value;
                OnPropertyChanged("ItemRecPath");
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
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        private string _borrowInfo;

        public string BorrowInfo
        {
            get => _borrowInfo;
            set
            {
                _borrowInfo = value;
                OnPropertyChanged("BorrowInfo");
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
    }

}
