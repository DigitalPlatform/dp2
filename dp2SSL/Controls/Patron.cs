using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.LibraryServer;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace dp2SSL
{
    public class RfidItem : INotifyPropertyChanged
    {
        public object Tag { get; set; }

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

        private string _oi;

        public string OI
        {
            get => _oi;
            set
            {
                if (_oi != value)
                {
                    _oi = value;
                    OnPropertyChanged("OI");
                }
            }
        }

        private string _aoi;

        public string AOI
        {
            get => _aoi;
            set
            {
                if (_aoi != value)
                {
                    _aoi = value;
                    OnPropertyChanged("AOI");
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

        // 2020/7/14
        // 和 Error 配套，表示错误码
        public string ErrorCode { get; set; }

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

        private string _readerName;

        // RFID 读卡器名字
        public string ReaderName
        {
            get => _readerName;
            set
            {
                if (_readerName != value)
                {
                    _readerName = value;
                    OnPropertyChanged("ReaderName");
                }
            }
        }

        private string _antenna;

        // RFID 天线编号
        public string Antenna
        {
            get => _antenna;
            set
            {
                if (_antenna != value)
                {
                    // Debug.WriteLine($"PII='{value}'");

                    _antenna = value;
                    OnPropertyChanged("Antenna");
                }
            }
        }

        private string _protocol;

        public string Protocol
        {
            get => _protocol;
            set
            {
                if (_protocol != value)
                {
                    _protocol = value;
                    OnPropertyChanged("Protocol");
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

        // 2020/4/9
        // 追加一个错误信息
        public void AppendError(string error,
            string color = "red",
            string errorCode = null)
        {
            if (string.IsNullOrEmpty(error))
            {
                this.Waiting = false;
                return;
            }
            if (string.IsNullOrEmpty(this.Error) == false)
                this.Error += ";";
            this.Error += error;

            if (errorCode != null)
            {
                if (string.IsNullOrEmpty(this.ErrorCode) == false)
                    this.ErrorCode += ",";
                this.ErrorCode += errorCode;
            }

            this.ErrorColor = color;
            this.Waiting = false;
        }

        public RfidItem Clone()
        {
            Patron dup = new Patron();
            CopyTo(dup);
            return dup;
        }

        public void CopyTo(RfidItem dup)
        {
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
            dup.Protocol = this.Protocol;
            dup.Tag = this.Tag;
        }

        // parameters:
        //      strict  是否为严格方式。严格方式下，如果 OI 和 AOI 为空，会返回 ".xxxx" 形态
        public string GetOiPii(bool strict = false)
        {
            if (strict == true)
            {
                if (string.IsNullOrEmpty(this.OI) == false)
                    return this.OI + "." + this.PII;
                else if (string.IsNullOrEmpty(this.AOI) == false)
                    return this.AOI + "." + this.PII;
                return "." + this.PII;
            }
            // 包含 OI 的 PII
            string pii = this.PII;
            if (string.IsNullOrEmpty(this.OI) == false)
                pii = this.OI + "." + this.PII;
            else if (string.IsNullOrEmpty(this.AOI) == false)
                pii = this.AOI + "." + this.PII;

            return pii;
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

        #region 和借阅权限、借阅情况有关的统计数字

        int _maxBorrowItems;

        // 最大可借册数
        public int MaxBorrowItems
        {
            get
            {
                return _maxBorrowItems;
            }
            set
            {
                if (_maxBorrowItems != value)
                {
                    _maxBorrowItems = value;
                    OnPropertyChanged("MaxBorrowItems");
                }
            }
        }

        int _canBorrowItems;

        // 当前还可借册数
        public int CanBorrowItems
        {
            get
            {
                return _canBorrowItems;
            }
            set
            {
                if (_canBorrowItems != value)
                {
                    _canBorrowItems = value;
                    OnPropertyChanged("CanBorrowItems");
                }
            }
        }


        int _overdueCount;

        // (待处理的)违约数量
        public int OverdueCount
        {
            get
            {
                return _overdueCount;
            }
            set
            {
                if (_overdueCount != value)
                {
                    _overdueCount = value;
                    OnPropertyChanged("OverdueCount");
                }
            }
        }

        int _overdueBorrowCount;

        // 在借册中的已超期册数
        public int OverdueBorrowCount
        {
            get
            {
                return _overdueBorrowCount;
            }
            set
            {
                if (_overdueBorrowCount != value)
                {
                    _overdueBorrowCount = value;
                    OnPropertyChanged("OverdueBorrowCount");
                }
            }
        }

        int _arrivedCount;

        // 已经到书的预约请求数
        public int ArrivedCount
        {
            get
            {
                return _arrivedCount;
            }
            set
            {
                if (_arrivedCount != value)
                {
                    _arrivedCount = value;
                    OnPropertyChanged("ArrivedCount");
                }
            }
        }

        int _borrowingCount;

        // 在借册数
        public int BorrowingCount
        {
            get
            {
                return _borrowingCount;
            }
            set
            {
                if (_borrowingCount != value)
                {
                    _borrowingCount = value;
                    OnPropertyChanged("BorrowingCount");
                }
            }
        }

        #endregion

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

        public new Patron Clone()
        {
            Patron dup = new Patron();
            this.CopyTo(dup);
            return dup;
        }

        public void CopyTo(Patron dup)
        {
            if (this == dup)
                throw new ArgumentException("dup 不应该和 this 相同");

            base.CopyTo(dup);
            dup.Source = this.Source;
            dup.NotEmpty = this.NotEmpty;
            dup.PatronName = this.PatronName;
            dup.Barcode = this.Barcode;
            dup.Department = this.Department;
            dup.MaxBorrowItems = this.MaxBorrowItems;
            dup.CanBorrowItems = this.CanBorrowItems;
            dup.OverdueCount = this.OverdueCount;
            dup.OverdueBorrowCount = this.OverdueBorrowCount;
            dup.ArrivedCount = this.ArrivedCount;
            dup.BorrowingCount = this.BorrowingCount;
            dup.PhotoPath = this.PhotoPath;

            dup.Xml = this.Xml;
            dup.Timestamp = this.Timestamp;
            dup.RecPath = this.RecPath;
        }

        // 刷新读者记录中的 可借总册数 和 当前还可借 参数数量
        public static void RefreshMaxBorrowable(XmlDocument patron_dom)
        {
            string libraryCode = DomUtil.GetElementText(patron_dom.DocumentElement, "libraryCode");
            string readerType = DomUtil.GetElementText(patron_dom.DocumentElement, "readerType");
            var total_max_result = ShelfData.GetTotalMax(libraryCode,
readerType);

            // 全部<borrow>元素
            XmlNodeList borrow_nodes = patron_dom.DocumentElement.SelectNodes("borrows/borrow");

            int nFreeBorrowCount = Math.Max(0, total_max_result.Max - borrow_nodes.Count);

            SetParamValue(patron_dom.DocumentElement, "当前还可借", nFreeBorrowCount.ToString());
            SetParamValue(patron_dom.DocumentElement, "可借总册数", total_max_result.Max.ToString());
        }

#if NO
        static XmlNode EnsureCreateNode(XmlElement root,
            string path,
            string value)
        {
            var list = StringUtil.SplitList(path, '/');
            XmlElement current = root;
            foreach (string level in list)
            {
                if (level[0] == '@')
                {
                    string name = level.Substring(1);
                    if (value != null)
                        current.SetAttribute(name, value);
                    else
                    {
                        current.SetAttribute(name, null);
                    }
                    return current.SelectSingleNode(level);
                }
                var new_element = current.OwnerDocument.CreateElement(level);
                current.AppendChild(new_element);
                current = new_element;
            }

            if (value != null)
                current.InnerText = value;
            return current;
        }
#endif
        // 模拟设置读者 XML 记录中的可借册数有关参数
        public static void SetParamValue(XmlElement root, string name, string value)
        {
            var info = root.SelectSingleNode("info") as XmlElement;
            if (info == null)
            {
                info = root.AppendChild(root.OwnerDocument.CreateElement("info")) as XmlElement;
            }

            var item = info.SelectSingleNode($"item[@name='{name}']") as XmlElement;
            if (item == null)
            {
                item = info.AppendChild(root.OwnerDocument.CreateElement("item")) as XmlElement;
                item.SetAttribute("name", name);
            }

            item.SetAttribute("value", value);
        }

        public void SetPatronXml(string recpath, string xml, byte[] timestamp)
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
            
            // 2020/9/8
            // 暂时无法区分 OI 和 AOI，都放到 OI 里面
            this.OI = DomUtil.GetElementText(dom.DocumentElement, "oi");

            this.PatronName = DomUtil.GetElementText(dom.DocumentElement, "name");
            this.Department = DomUtil.GetElementText(dom.DocumentElement, "department");

            if (App.Protocol == "dp2library")
            {
                // 获得头像路径
                this.PhotoPath = GetCardPhotoPath(dom,
                    new List<string> { "face", "cardphoto" },
                    recpath);
            }

            if (App.Protocol == "sip")
            {
                this.PhotoPath = this.Barcode;
            }

            // 2019/11/28
            /*
<info>
<item name="可借总册数" value="15" />
<item name="日历名">
    <value>基本日历</value>
</item>
<item name="当前还可借" value="15"/>
</info>
             * */
            {

                if (dom.DocumentElement.SelectSingleNode("info/item[@name='可借总册数']") is XmlElement item)
                {
                    DomUtil.GetIntegerParam(item, "value", 0, out int value, out string strError);
                    this.MaxBorrowItems = value;
                }
                else
                    this.MaxBorrowItems = 0;
            }

            {
                if (dom.DocumentElement.SelectSingleNode("info/item[@name='当前还可借']") is XmlElement item)
                {
                    DomUtil.GetIntegerParam(item, "value", 0, out int value, out string strError);
                    this.CanBorrowItems = value;
                }
                else
                    this.CanBorrowItems = 0;
            }

            // 违约/交费信息
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");
                this.OverdueCount = nodes.Count;
            }

            // 在借图书中的已经超期的册数
            {
                // 检查当前是否有潜在的超期册
                // return:
                //      -1  error
                //      0   没有超期册
                //      1   有超期册
                int nRet = LibraryServerUtil.CheckOverdue(
                    dom,
                    out List<string> overdue_infos,
                    out string strError);
                if (nRet == -1)
                    this.OverdueBorrowCount = 0;
                else
                    this.OverdueBorrowCount = overdue_infos.Count;
            }

            // 在借册数
            {
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
                this.BorrowingCount = nodes.Count;
            }
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

        public class FillResult : NormalResult
        {
            // [out] 当 ErrorCode 为 "bookTag" 时，这里返回图书的 PII
            public string PII { get; set; }
            // [out] 当 ErrorCode 为 "bookTag" 时，这里返回图书的 OI 或者 AOI
            public string OI { get; set; }
            // [out] 当 ErrorCode 为 "bookTag" 时，这里返回图书的 AOI
            public string AOI { get; set; }
        }

        // 刷新信息
        // Exception:
        //      可能会抛出异常 ArgumentException TagDataException
        // result.Value:
        //      -1  出错
        //      0   未进行刷新
        //      1   成功进行了刷新
        public FillResult Fill(OneTag tag)
        {
            string pii = "";
            string oi = "";
            string aoi = "";

            // 2020/6/2
            this.Protocol = tag.Protocol;

            if (tag.TagInfo == null && tag.Protocol == InventoryInfo.ISO15693)
            {
                // throw new Exception("Fill() taginfo == null");
                return new FillResult { Value = 0 };
            }

            if (tag.TagInfo != null && tag.Protocol == InventoryInfo.ISO15693)
            {
                // Exception:
                //      可能会抛出异常 ArgumentException TagDataException
                LogicChip chip = LogicChip.From(tag.TagInfo.Bytes,
    (int)tag.TagInfo.BlockSize,
    "" // tag.TagInfo.LockStatus
    );
                pii = chip.FindElement(ElementOID.PII)?.Text;

                oi = chip.FindElement(ElementOID.OI)?.Text;
                aoi = chip.FindElement(ElementOID.AOI)?.Text;

                string typeOfUsage = chip.FindElement(ElementOID.TypeOfUsage)?.Text;
                if (typeOfUsage != null && typeOfUsage.StartsWith("8"))
                {

                }
                else
                {
                    return new FillResult
                    {
                        Value = -1,
                        ErrorInfo = "这是一张图书标签",
                        ErrorCode = "bookTag",
                        PII = pii,
                        OI = oi,
                        AOI = aoi,
                    };
                }
            }

            if (this.UID == tag.UID && this.PII == pii)
                return new FillResult { Value = 1 }; // 优化

            this.Clear();

            this.UID = tag.UID;
            this.PII = pii;
            this.OI = oi;
            this.AOI = aoi;
            return new FillResult { Value = 1 };
        }

        public void Clear()
        {
            this.PatronName = null;
            this.Barcode = null;
            this.Department = null;
            this.UID = null;
            this.PII = null;
            this.PhotoPath = "";

            this.MaxBorrowItems = 0;
            this.CanBorrowItems = 0;
            this.OverdueCount = 0;
            this.OverdueBorrowCount = 0;
            this.ArrivedCount = 0;
            this.BorrowingCount = 0;
            this.Source = null;
            this.Xml = "";
            this.Timestamp = null;

            this.RecPath = "";

            // 2019/12/10
            this.Error = null;

            // 2020/9/8
            this.Protocol = null;

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

        // 用于精确定位的名字
        public string NameSummary
        {
            get
            {
                return $"{PatronName} 证条码号:{Barcode}";
            }
        }

    }
}
