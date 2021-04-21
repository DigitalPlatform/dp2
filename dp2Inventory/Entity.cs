using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.RFID;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2Inventory
{
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
            // dup.Container = this.Container;
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

        // public EntityCollection Container { get; set; }

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

        public static string ToDate(string strTime)
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

        #region 分类错误处理机制

        List<ErrorItem> _errorItems = null;

        // 错误事项的集合
        public List<ErrorItem> ErrorItems
        {
            get
            {
                if (_errorItems == null)
                    return new List<ErrorItem>();

                return new List<ErrorItem>(_errorItems);
            }
        }

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
            RfidItem dup = new RfidItem();
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

        public string GetOiOrAoi()
        {
            if (string.IsNullOrEmpty(this.OI) == false)
                return this.OI;

            return this.AOI;
        }
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
