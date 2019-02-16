using DigitalPlatform.RFID;
using DigitalPlatform.Xml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dp2SSL
{
    public class RfidItem : INotifyPropertyChanged
    {
        private string _pii;

        public string PII
        {
            get => _pii;
            set
            {
                _pii = value;
                OnPropertyChanged("PII");
            }
        }

        private string _uid;
        private string _error;  // = "test error line asdlja sdkf; ;jasldf jasdjkf aasdfasdf";
        bool _waiting = true;

        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged("UID");
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                OnPropertyChanged("Error");
            }
        }

        string _errorColor = "red";

        public string ErrorColor
        {
            get
            {
                return _errorColor;
            }
            set
            {
                _errorColor = value;
                OnPropertyChanged("ErrorColor");
            }
        }

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

        string _state = null;

        // 事项状态。borrowed/onshelf/空
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }


        internal void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 设置错误信息并结束等待状态
        public void SetError(string error, string color = "red")
        {
            this.Error = error;
            this.ErrorColor = color;
            this.Waiting = false;
        }

    }

    // 读者信息
    public class Patron : RfidItem
    {
        string _patronName;

        public string PatronName
        {
            get
            {
                return _patronName;
            }
            set
            {
                _patronName = value;
                OnPropertyChanged("PatronName");
            }
        }

        string _barcode;

        public string Barcode
        {
            get
            {
                return _barcode;
            }
            set
            {
                _barcode = value;
                OnPropertyChanged("Barcode");
            }
        }

        string _department;

        public string Department
        {
            get
            {
                return _department;
            }
            set
            {
                _department = value;
                OnPropertyChanged("Department");
            }
        }

        public void SetPatronXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                this.Error = "xml is null";
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            this.Barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

            this.PatronName = DomUtil.GetElementText(dom.DocumentElement, "name");
            this.Department = DomUtil.GetElementText(dom.DocumentElement, "department");
        }

        // 刷新信息
        public void Fill(OneTag tag)
        {
            string pii = "";
            if (tag.TagInfo != null && tag.Protocol == InventoryInfo.ISO15693)
            {
                LogicChip chip = LogicChip.From(tag.TagInfo.Bytes,
    (int)tag.TagInfo.BlockSize,
    "" // tag.TagInfo.LockStatus
    );
                pii = chip.FindElement(ElementOID.PII)?.Text;
            }

            this.UID = tag.UID;
            this.PII = pii;
        }

        public void Clear()
        {
            this.PatronName = null;
            this.Barcode = null;
            this.Department = null;
            this.UID = null;
            this.PII = null;
        }
    }
}
