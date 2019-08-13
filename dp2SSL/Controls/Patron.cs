using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.RFID;
using DigitalPlatform.Xml;

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
                if (_pii != value)
                {
                    // Debug.WriteLine($"PII='{value}'");

                    _pii = value;
                    OnPropertyChanged("PII");
                }
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
                if (_uid != value)
                {
                    _uid = value;
                    OnPropertyChanged("UID");
                }
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
                if (_errorColor != value)
                {
                    _errorColor = value;
                    OnPropertyChanged("ErrorColor");
                }
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
                if (_waiting != value)
                {
                    _waiting = value;
                    OnPropertyChanged("Waiting");
                }
            }
        }

        string _state = null;

        // 事项状态。borrowed/onshelf/空，或者各种值的组合
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
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
        // 来源。默认从 RFID 读卡器。"fingerprint" 表示从指纹仪而来
        public string Source { get; set; }

        public bool IsFingerprintSource
        {
            get
            {
                return Source == "fingerprint";
            }
            set
            {
                if (value == true)
                    Source = "fingerprint";
                else
                    Source = "";
            }
        }

        public bool IsRfidSource
        {
            get
            {
                return Source == "";
            }
            set
            {
                if (value == true)
                    Source = "";
                else
                    Source = "fingerprint";
            }
        }

        public new string UID
        {
            get
            {
                return base.UID;
            }
            set
            {
                base.UID = value;
                SetNotEmpty();
            }
        }

        public new string PII
        {
            get
            {
                return base.PII;
            }
            set
            {
                base.PII = value;
                SetNotEmpty();
            }
        }

        public new string Error
        {
            get
            {
                return base.Error;
            }
            set
            {
                base.Error = value;
                SetNotEmpty();
            }
        }

        // 是否非空
        bool _notEmpty = false;

        public bool NotEmpty
        {
            get
            {
                return _notEmpty;
            }
            set
            {
                if (_notEmpty != value)
                {
                    _notEmpty = value;
                    OnPropertyChanged("NotEmpty");
                }
            }
        }

        string _patronName;

        public string PatronName
        {
            get
            {
                return _patronName;
            }
            set
            {
                if (_patronName != value)
                {
                    _patronName = value;
                    OnPropertyChanged("PatronName");
                }
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
                if (_barcode != value)
                {
                    _barcode = value;
                    OnPropertyChanged("Barcode");
                    this.SetNotEmpty();
                }
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
                if (_department != value)
                {
                    _department = value;
                    OnPropertyChanged("Department");
                }
            }
        }

        string _photoPath;

        public string PhotoPath
        {
            get
            {
                return _photoPath;
            }
            set
            {
                if (_photoPath != value)
                {
                    _photoPath = value;
                    OnPropertyChanged("PhotoPath");
                }
            }
        }

        string _xml = "";
        public string Xml
        {
            get
            {
                return _xml;
            }
            set
            {
                _xml = value;
            }
        }

        public byte[] Timestamp { get; set; }

        public string RecPath { get; set; }

        public void SetPatronXml(string recpath, string xml, byte [] timestamp)
        {
            if (string.IsNullOrEmpty(xml))
            {
                this.Error = "xml is null";
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);

            RecPath = recpath;
            _xml = xml;
            Timestamp = timestamp;

            this.Barcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");

            this.PatronName = DomUtil.GetElementText(dom.DocumentElement, "name");
            this.Department = DomUtil.GetElementText(dom.DocumentElement, "department");

            // 获得头像路径
            this.PhotoPath = GetCardPhotoPath(dom,
                new List<string> { "face", "cardphoto" },
                recpath);
        }

        // 从读者记录 XML 中获得读者卡片头像的路径。例如 "读者/1/object/0"
        // parameters:
        //      usage_list  用途列表。只要顺次匹配上其中任何一个就算命中
        static string GetCardPhotoPath(XmlDocument readerdom,
            List<string> usage_list,
            string strRecPath)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);

            foreach (string usage in usage_list)
            {
                XmlNodeList nodes = readerdom.DocumentElement.SelectNodes($"//dprms:file[@usage='{usage}']", nsmgr);
                if (nodes.Count == 0)
                    continue;

                string strID = DomUtil.GetAttr(nodes[0], "id");
                if (string.IsNullOrEmpty(strID) == true)
                    continue;

                string strResPath = strRecPath + "/object/" + strID;
                return strResPath.Replace(":", "/");
            }

            return null;
        }

        // 刷新信息
        public void Fill(OneTag tag)
        {
            string pii = "";

            if (tag.TagInfo == null && tag.Protocol == InventoryInfo.ISO15693)
            {
                throw new Exception("Fill() taginfo == null");
            }

            if (tag.TagInfo != null && tag.Protocol == InventoryInfo.ISO15693)
            {
                LogicChip chip = LogicChip.From(tag.TagInfo.Bytes,
    (int)tag.TagInfo.BlockSize,
    "" // tag.TagInfo.LockStatus
    );
                pii = chip.FindElement(ElementOID.PII)?.Text;
            }

            if (this.UID == tag.UID && this.PII == pii)
                return; // 优化

            this.Clear();

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
            this.PhotoPath = "";

            this.RecPath = "";

            this.SetNotEmpty();
        }

        public void SetNotEmpty()
        {
            if (string.IsNullOrEmpty(this.Barcode) == false
                || string.IsNullOrEmpty(this.UID) == false
                || string.IsNullOrEmpty(this.PII) == false
                || string.IsNullOrEmpty(this.Error) == false)
                this.NotEmpty = true;
            else
                this.NotEmpty = false;
        }

    }
}
