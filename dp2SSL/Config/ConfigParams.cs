using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections;

using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

using DigitalPlatform.Core;
using DigitalPlatform.Text;

namespace dp2SSL
{
    /// <summary>
    /// 这是提供给 PropertySheet 进行编辑时使用的代表配置参数的内存模型
    /// </summary>
    public class ConfigParams : DataErrorInfoImpl, INotifyPropertyChanged
    {
        ConfigSetting _config = null;

        public ConfigParams(ConfigSetting config)
        {
            _config = config;
        }

        #region 记忆修改过的参数名

        // 修改了的参数名
        // name --> true
        Hashtable _changedParams = new Hashtable();

        bool IsParamChanged(string name)
        {
            if (_changedParams.ContainsKey(name))
                return true;
            return false;
        }

        #endregion

        #region INotifyPropertyChanged 接口实现

        internal void OnPropertyChanged(string name)
        {
            // 记载下来
            _changedParams[name] = true;

            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        // 从 XML 文件中装载配置参数到内存
        public void LoadData()
        {
            SipServerUrl = _config.Get("global", "sipServerUrl", "");
            SipUserName = _config.Get("global", "sipUserName", "");
            SipPassword = App.DecryptPasssword(_config.Get("global", "sipPassword", ""));
            SipEncoding = _config.Get("global", "sipEncoding", "utf-8");
            SipInstitution = _config.Get("global", "sipInstitution", "");

            Dp2ServerUrl = _config.Get("global", "dp2ServerUrl", "");
            Dp2UserName = _config.Get("global", "dp2UserName", "");
            Dp2Password = App.DecryptPasssword(_config.Get("global", "dp2Password", ""));

            RfidURL = _config.Get("global", "rfidUrl", "");
            FingerprintURL = _config.Get("global", "fingerprintUrl", "");
            FaceURL = _config.Get("global", "faceUrl", "");

            FullScreen = _config.GetInt("global", "fullScreen", 1) == 1 ? true : false;
            AutoTrigger = _config.GetBoolean("ssl_operation", "auto_trigger", false);
            PatronInfoLasting = _config.GetBoolean("ssl_operation", "patron_info_lasting", false);
            AutoBackMenuPage = _config.GetBoolean("ssl_operation", "auto_back_menu_page", false);
            ProcessMonitor = _config.GetBoolean("global", "process_monitor", true);
            ReplicateEntities = _config.GetBoolean("shelf", "replicateEntities", false);
            Function = _config.Get("global", "function", "自助借还");
            PatronBarcodeStyle = _config.Get("global", "patron_barcode_style", "禁用");
            WorkerBarcodeStyle = _config.Get("global", "worker_barcode_style", "禁用");
            PosPrintStyle = _config.Get("global", "pos_print_style", "不打印");
            CacheWorkerPasswordLength = _config.Get("global", "memory_worker_password", "无");
            AutoBackMainMenuSeconds = _config.GetInt("global", "autoback_mainmenu_seconds", -1);
            AutoShutdownParam = ShutdownTask.GetPerdayTask();
            AutoUpdateWallpaper = _config.GetBoolean("global", "auto_update_wallpaper", false);

            MessageServerUrl = _config.Get("global", "messageServerUrl", "");
            MessageUserName = _config.Get("global", "messageUserName", "");
            MessagePassword = App.DecryptPasssword(_config.Get("global", "messagePassword", ""));

            _changedParams.Clear();
        }

        // 将内存中(修改了的)配置参数保存回到 XML 文件
        public string SaveData()
        {
            List<string> errors = new List<string>();

            _config.Set("global", "sipServerUrl", SipServerUrl);
            _config.Set("global", "sipUserName", SipUserName);
            _config.Set("global", "sipPassword", App.EncryptPassword(SipPassword));
            _config.Set("global", "sipEncoding", SipEncoding);
            _config.Set("global", "sipInstitution", SipInstitution);

            {
                _config.Set("global", "dp2ServerUrl", Dp2ServerUrl);
                _config.Set("global", "dp2UserName", Dp2UserName);
                _config.Set("global", "dp2Password", App.EncryptPassword(Dp2Password));

                if (IsParamChanged("Dp2ServerUrl")
                    || IsParamChanged("Dp2UserName")
                    || IsParamChanged("Dp2Password"))
                    App.CurrentApp.ClearChannelPool();
            }

            _config.Set("global", "rfidUrl", RfidURL);
            _config.Set("global", "fingerprintUrl", FingerprintURL);
            _config.Set("global", "faceUrl", FaceURL);
            _config.SetInt("global", "fullScreen", FullScreen == true ? 1 : 0);
            _config.SetBoolean("ssl_operation", "auto_trigger", AutoTrigger);
            _config.SetBoolean("ssl_operation", "patron_info_lasting", PatronInfoLasting);
            _config.SetBoolean("ssl_operation", "auto_back_menu_page", AutoBackMenuPage);
            _config.SetBoolean("global", "process_monitor", ProcessMonitor);
            _config.SetBoolean("shelf", "replicateEntities", ReplicateEntities);
            _config.Set("global", "function", Function);
            _config.Set("global", "patron_barcode_style", PatronBarcodeStyle);
            _config.Set("global", "worker_barcode_style", WorkerBarcodeStyle);
            _config.Set("global", "pos_print_style", PosPrintStyle);
            _config.Set("global", "memory_worker_password", CacheWorkerPasswordLength);
            _config.SetInt("global", "autoback_mainmenu_seconds", AutoBackMainMenuSeconds);

            if (IsParamChanged("AutoShutdownParam"))
            {
                var result = ShutdownTask.ChangePerdayTask(AutoShutdownParam);
                if (result.Value == -1)
                    errors.Add(result.ErrorInfo);
            }

            _config.SetBoolean("global", "auto_update_wallpaper", AutoUpdateWallpaper);

            {
                _config.Set("global", "messageServerUrl", MessageServerUrl);
                _config.Set("global", "messageUserName", MessageUserName);
                _config.Set("global", "messagePassword", App.EncryptPassword(MessagePassword));

                if (IsParamChanged("MessageServerUrl")
                    || IsParamChanged("MessageUserName")
                    || IsParamChanged("MessagePassword"))
                {
                    // 2021/11/10 增加
                    _ = App.StartMessageSendingAsync("配置界面修改了消息发送相关参数");
                }
            }

            // 有错误
            if (errors.Count > 0)
            {
                return (StringUtil.MakePathList(errors, "; "));
            }

            return null;
        }

        public string Validate()
        {
            List<string> errors = new List<string>();
            /*
            List<string> names = new List<string>(_changedParams.Keys.Cast<string>());
            if (names.IndexOf("AutoBackMainMenuSeconds") == -1)
                names.Add("AutoBackMainMenuSeconds");
            */
            foreach (string name in _changedParams.Keys)
            {
                string error = ValidateProperty(name);  // "AutoShutdownParam"
                if (error != null)
                    errors.Add(error);
            }

            if (errors.Count == 0)
                return null;

            return StringUtil.MakePathList(errors, "\r\n");
        }

        string ValidateProperty(string name)
        {
            IDataErrorInfo errorInfo = (IDataErrorInfo)this;
            string error = errorInfo[name];
            if (error == null)
                return null;

            var value = GetProperty(this, name);
            var display_attr = typeof(ConfigParams)
  .GetProperty(name)
  .GetCustomAttributes(false)
  .OfType<DisplayAttribute>().ToList().FirstOrDefault();
            return $"{display_attr.Name} 值 '{value}' 不合法: {error}";
        }

        static string GetProperty(Object obj, string name)
        {
            obj.GetType().GetField(name);   // ?
            var prop = obj.GetType().GetProperty(name);
            return (string)prop.GetValue(obj);
        }

        #region SIP2 服务器

        [Display(
            Order = 1,
            Name = "地址和端口号",
            Description = "SIP2 服务器的地址和端口号"
            )]
        [Category("SIP2 服务器")]
        public string SipServerUrl
        {
            get => _sipServerUrl;
            set
            {
                if (_sipServerUrl != value)
                {
                    _sipServerUrl = value;
                    OnPropertyChanged("SipServerUrl");
                }
            }
        }
        private string _sipServerUrl;

        [Display(
    Order = 2,
    Name = "用户名",
    Description = "SIP2 服务器的用户名"
    )]
        [Category("SIP2 服务器")]
        public string SipUserName
        {
            get => _sipUserName;
            set
            {
                if (_sipUserName != value)
                {
                    _sipUserName = value;
                    OnPropertyChanged("SipUserName");
                }
            }
        }
        private string _sipUserName;

        [Display(
    Order = 3,
    Name = "密码",
    Description = "SIP2 服务器的密码"
    )]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Category("SIP2 服务器")]
        public string SipPassword
        {
            get => _sipPassword;
            set
            {
                if (_sipPassword != value)
                {
                    _sipPassword = value;
                    OnPropertyChanged("SipPassword");
                }
            }
        }
        private string _sipPassword;

        [Display(
Order = 4,
Name = "编码方式",
Description = "SIP2 通讯所用的字符集编码方式"
)]
        [ItemsSource(typeof(EncodingItemsSource))]
        [Category("SIP2 服务器")]
        public string SipEncoding
        {
            get => _sipEncoding;
            set
            {
                if (_sipEncoding != value)
                {
                    _sipEncoding = value;
                    OnPropertyChanged("SipEncoding");
                }
            }
        }
        private string _sipEncoding;

        // 2021/1/31
        [Display(
Order = 5,
Name = "机构代码",
Description = "用于 SIP2 服务器的 RFID 标签机构代码"
)]
        [Category("SIP2 服务器")]
        public string SipInstitution
        {
            get => _sipInstitution;
            set
            {
                if (_sipInstitution != value)
                {
                    _sipInstitution = value;
                    OnPropertyChanged("SipInstitution");
                }
            }
        }
        private string _sipInstitution;

        #endregion

        #region dp2 服务器

        [Display(
            Order = 1,
            Name = "URL 地址",
            Description = "dp2library 服务器的 URL 地址"
            )]
        [Category("dp2 服务器")]
        public string Dp2ServerUrl
        {
            get => _dp2ServerUrl;
            set
            {
                if (_dp2ServerUrl != value)
                {
                    _dp2ServerUrl = value;
                    OnPropertyChanged("Dp2ServerUrl");
                }
            }
        }
        private string _dp2ServerUrl;

        [Display(
    Order = 2,
    Name = "用户名",
    Description = "dp2library 服务器的用户名"
    )]
        [Category("dp2 服务器")]
        public string Dp2UserName
        {
            get => _dp2UserName;
            set
            {
                if (_dp2UserName != value)
                {
                    _dp2UserName = value;
                    OnPropertyChanged("Dp2UserName");
                }
            }
        }
        private string _dp2UserName;

        [Display(
    Order = 3,
    Name = "密码",
    Description = "dp2library 服务器的密码"
    )]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Category("dp2 服务器")]
        public string Dp2Password
        {
            get => _dp2Password;
            set
            {
                if (_dp2Password != value)
                {
                    _dp2Password = value;
                    OnPropertyChanged("Dp2Password");
                }
            }
        }
        private string _dp2Password;

        #endregion

        // 默认值 ipc://RfidChannel/RfidServer
        [Display(
Order = 4,
Name = "RFID 接口 URL",
Description = "RFID 接口 URL 地址"
)]
        [Category("RFID 接口")]
        public string RfidURL
        {
            get => _rfidURL;
            set
            {
                if (_rfidURL != value)
                {
                    _rfidURL = value;
                    OnPropertyChanged("RfidURL");
                }
            }
        }
        private string _rfidURL;

        // 默认值 ipc://FingerprintChannel/FingerprintServer
        [Display(
Order = 5,
Name = "指纹接口 URL",
Description = "指纹接口 URL 地址"
)]
        [Category("指纹接口")]
        public string FingerprintURL
        {
            get => _fingerprintURL;
            set
            {
                if (_fingerprintURL != value)
                {
                    _fingerprintURL = value;
                    OnPropertyChanged("FingerprintURL");
                }
            }
        }
        private string _fingerprintURL;

        // 默认值 ipc://FaceChannel/FaceServer
        [Display(
Order = 6,
Name = "人脸接口 URL",
Description = "人脸接口 URL 地址"
)]
        [Category("人脸接口")]
        public string FaceURL
        {
            get => _faceURL;
            set
            {
                if (_faceURL != value)
                {
                    _faceURL = value;
                    OnPropertyChanged("FaceURL");
                }
            }
        }
        private string _faceURL;

        // 默认值 true
        [Display(
Order = 7,
Name = "启动时全屏",
Description = "程序启动时候是否自动全屏"
)]
        [Category("启动")]
        public bool FullScreen
        {
            get => _fullScreen;
            set
            {
                if (_fullScreen != value)
                {
                    _fullScreen = value;
                    OnPropertyChanged("FullScreen");
                }
            }
        }
        private bool _fullScreen;

        // 默认值 false
        [Display(
Order = 7,
Name = "借还按钮自动触发",
Description = "借书和还书操作是否自动触发操作按钮"
)]
        [Category("自助借还操作风格")]
        public bool AutoTrigger
        {
            get => _autoTrigger;
            set
            {
                if (_autoTrigger != value)
                {
                    _autoTrigger = value;
                    OnPropertyChanged("AutoTrigger");
                }
            }
        }
        private bool _autoTrigger;

        // 默认值 false
        [Display(
Order = 7,
Name = "身份读卡器竖放",    // 拿走不敏感。读者信息显示持久
Description = "RFID读者卡读卡器是否竖向放置"
)]
        [Category("自助借还操作风格")]
        public bool PatronInfoLasting
        {
            get => _patronInfoLasting;
            set
            {
                if (_patronInfoLasting != value)
                {
                    _patronInfoLasting = value;
                    OnPropertyChanged("PatronInfoLasting");
                }
            }
        }
        private bool _patronInfoLasting;

        // 默认值 false
        [Display(
Order = 8,
Name = "立即自动返回菜单页面",
Description = "借书还书操作完成后是否立即自动返回菜单页面"
)]
        [Category("自助借还操作风格")]
        public bool AutoBackMenuPage
        {
            get => _autoBackMenuPage;
            set
            {
                if (_autoBackMenuPage != value)
                {
                    _autoBackMenuPage = value;
                    OnPropertyChanged("AutoBackMenuPage");
                }
            }
        }
        private bool _autoBackMenuPage;

        /*
        // 默认值 false
        [Display(
Order = 7,
Name = "读者信息延时清除",
Description = "是否自动延时清除读者信息"
)]
        [Category("自助借还操作风格")]
        public bool PatronInfoDelayClear
        {
            get
            {
                return _config.GetBoolean("ssl_operation", "patron_info_delay_clear", false);
            }
            set
            {
                _config.SetBoolean("ssl_operation", "patron_info_delay_clear", value);
            }
        }
        */
        /*
        // 默认值 false
        [Display(
Order = 7,
Name = "启用读者证条码扫入",
Description = "是否允许自助借还时扫入读者证条码"
)]
        [Category("自助借还操作风格")]
        public bool EanblePatronBarcode
        {
            get
            {
                return _config.GetBoolean("ssl_operation", "enable_patron_barcode", false);
            }
            set
            {
                _config.SetBoolean("ssl_operation", "enable_patron_barcode", value);
            }
        }
        */

        // 默认值 true
        [Display(
Order = 8,
Name = "监控相关进程",
Description = "自动监控和重启 人脸中心 RFID中心 指纹中心等模块"
)]
        [Category("维护")]
        public bool ProcessMonitor
        {
            get => _processMonitor;
            set
            {
                if (_processMonitor != value)
                {
                    _processMonitor = value;
                    OnPropertyChanged("ProcessMonitor");
                }
            }
        }
        private bool _processMonitor;

        // 默认值 false
        [Display(
Order = 9,
Name = "同步册记录",
Description = "(智能书柜)自动同步全部册记录和书目摘要到本地"
)]
        [Category("维护")]
        public bool ReplicateEntities
        {
            get => _replicateEntities;
            set
            {
                if (_replicateEntities != value)
                {
                    _replicateEntities = value;
                    OnPropertyChanged("ReplicateEntities");
                }
            }
        }
        private bool _replicateEntities;

        /*
        // 默认值 空
        [Display(
Order = 9,
Name = "馆藏地",
Description = "智能书架内的图书的专属馆藏地"
)]
        [Category("智能书架")]
        public string ShelfLocation
        {
            get
            {
                return _config.Get("shelf", "location", "");
            }
            set
            {
                _config.Set("shelf", "location", value);
            }
        }
        */

        // https://github.com/xceedsoftware/wpftoolkit/issues/1269
        // 默认值 空
        [Display(
Order = 10,
Name = "功能类型",
Description = "dp2SSL 的功能类型"
)]
        [ItemsSource(typeof(FunctionItemsSource))]
        [Category("全局")]
        public string Function
        {
            get => _function;
            set
            {
                if (_function != value)
                {
                    _function = value;
                    OnPropertyChanged("Function");
                }
            }
        }
        private string _function;

        // 默认值 空
        [Display(
Order = 10,
Name = "读者证条码输入方式",
Description = "读者证条码的输入方式"
)]
        [ItemsSource(typeof(PatronBarcodeStyleSource))]
        [Category("全局")]
        public string PatronBarcodeStyle
        {
            get => _patronBarcodeStyle;
            set
            {
                if (_patronBarcodeStyle != value)
                {
                    _patronBarcodeStyle = value;
                    OnPropertyChanged("PatronBarcodeStyle");
                }
            }
        }
        private string _patronBarcodeStyle;

        // 默认值 空
        [Display(
Order = 11,
Name = "工作人员条码输入方式",
Description = "工作人员条码的输入方式"
)]
        [ItemsSource(typeof(PatronBarcodeStyleSource))]
        [Category("全局")]
        public string WorkerBarcodeStyle
        {
            get => _workerBarcodeStyle;
            set
            {
                if (_workerBarcodeStyle != value)
                {
                    _workerBarcodeStyle = value;
                    OnPropertyChanged("WorkerBarcodeStyle");
                }
            }
        }
        private string _workerBarcodeStyle;

        // 默认值 空
        [Display(
Order = 12,
Name = "凭条打印方式",
Description = "凭条(小票)打印方式"
)]
        [ItemsSource(typeof(PosPrintStyleSource))]
        [Category("全局")]
        public string PosPrintStyle
        {
            get => _posPrintStyle;
            set
            {
                if (_posPrintStyle != value)
                {
                    _posPrintStyle = value;
                    OnPropertyChanged("PosPrintStyle");
                }
            }
        }
        private string _posPrintStyle;

        // 默认值 false
        [Display(
Order = 13,
Name = "工作人员刷卡免密码时长",
Description = "工作人员刷卡成功登录后，多少时间内再刷卡不用输入密码"
)]
        [ItemsSource(typeof(CachePasswordLengthSource))]
        [Category("全局")]
        public string CacheWorkerPasswordLength
        {
            get => _cacheWorkerPasswordLength;
            set
            {
                if (_cacheWorkerPasswordLength != value)
                {
                    _cacheWorkerPasswordLength = value;
                    OnPropertyChanged("CacheWorkerPasswordLength");
                }
            }
        }
        private string _cacheWorkerPasswordLength;

        // 默认值 -1。-1 表示永远不返回
        [Display(
Order = 14,
Name = "休眠返回主菜单秒数",
Description = "当没有操作多少秒以后，自动返回主菜单页面"
)]
        [Category("全局")]
        [Range(-1, 36000)]
        public int AutoBackMainMenuSeconds
        {
            get => _autoBackMainMenuSeconds;
            set
            {
                if (_autoBackMainMenuSeconds != value)
                {
                    _autoBackMainMenuSeconds = value;
                    OnPropertyChanged("AutoBackMainMenuSeconds");
                }
            }
        }
        private int _autoBackMainMenuSeconds;

        // 2021/11/7
        // 默认值 null。null 表示不关机
        [Display(
Order = 15,
Name = "每日自动关机",
Description = "每日自动执行关机的时间定义。例如 17:30,18:30"
)]
        [Category("全局")]
        [CustomValidation(typeof(ShutdownParamValidator), "Validate")]  // https://stackoverflow.com/questions/4396205/implementing-validations-in-wpf-propertygrid
        public string AutoShutdownParam
        {
            get => _autoShutdownParam;
            set
            {
                if (_autoShutdownParam != value)
                {
                    _autoShutdownParam = value;
                    OnPropertyChanged("AutoShutdownParam");
                }
            }
        }
        private string _autoShutdownParam;


        // 2021/11/17
        // 默认值 true
        [Display(
Order = 16,
Name = "每日自动更新壁纸",
Description = "是否每日自动更新一次壁纸"
)]
        [Category("全局")]
        public bool AutoUpdateWallpaper
        {
            get => _autoUpdateWallpaper;
            set
            {
                if (_autoUpdateWallpaper != value)
                {
                    _autoUpdateWallpaper = value;
                    OnPropertyChanged("AutoUpdateWallpaper");
                }
            }
        }
        private bool _autoUpdateWallpaper;


        /*
        // 默认值 空
        [Display(
Order = 10,
Name = "14443A卡号预处理",
Description = "14443A卡号预处理"
)]
        [ItemsSource(typeof(CardNumberConvertItemsSource))]
        [Category("全局")]
        public string CardNumberConvert
        {
            get
            {
                return _config.Get("global", "card_number_convert_method", "十六进制");
            }
            set
            {
                _config.Set("global", "card_number_convert_method", value);
            }
        }
        */

        /*
        // 默认值 false
        [Display(
Order = 1,
Name = "动态反馈图书变动数",
Description = "是否动态反馈图书变动数"
)]
        [Category("智能书柜操作风格")]
        public bool DetectBookChange
        {
            get
            {
                return _config.GetBoolean("shelf_operation", "detect_book_change", false);
            }
            set
            {
                _config.SetBoolean("shelf_operation", "detect_book_change", value);
            }
        }
        */

        #region 消息服务器相关参数

        [Display(
    Order = 21,
    Name = "URL 地址",
    Description = "消息服务器的 URL 地址"
    )]
        [Category("消息服务器")]
        public string MessageServerUrl
        {
            get => _messageServerUrl;
            set
            {
                if (_messageServerUrl != value)
                {
                    _messageServerUrl = value;
                    OnPropertyChanged("MessageServerUrl");
                }
            }
        }
        private string _messageServerUrl;

        [Display(
    Order = 22,
    Name = "用户名",
    Description = "消息服务器的用户名"
    )]
        [Category("消息服务器")]
        public string MessageUserName
        {
            get => _messageUserName;
            set
            {
                if (_messageUserName != value)
                {
                    _messageUserName = value;
                    OnPropertyChanged("MessageUserName");
                }
            }
        }
        private string _messageUserName;

        [Display(
    Order = 23,
    Name = "密码",
    Description = "消息服务器的密码"
    )]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Category("消息服务器")]
        public string MessagePassword
        {
            get => _messagePassword;
            set
            {
                if (_messagePassword != value)
                {
                    _messagePassword = value;
                    OnPropertyChanged("MessagePassword");
                }
            }
        }
        private string _messagePassword;

        /*
        [Display(
Order = 24,
Name = "组名",
Description = "用于交换消息的组的名字"
)]
        [Category("消息服务器")]
        public string MessageGroupName
        {
            get
            {
                return _config.Get("global", "messageGroupName", "");
            }
            set
            {
                _config.Set("global", "messageGroupName", value);
                App.CurrentApp.ClearChannelPool();
            }
        }
        */

        #endregion
    }

    // https://stackoverflow.com/questions/4396205/implementing-validations-in-wpf-propertygrid
    public class DataErrorInfoImpl : IDataErrorInfo
    {
        string IDataErrorInfo.Error
        {
            get
            {
                return string.Empty;
            }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var pi = GetType().GetProperty(columnName);
                var value = pi.GetValue(this, null);

                var context = new ValidationContext(this, null, null) { MemberName = columnName };
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (!Validator.TryValidateProperty(value, context, validationResults))
                {
                    var sb = new StringBuilder();
                    foreach (var vr in validationResults)
                    {
                        sb.AppendLine(vr.ErrorMessage);
                    }
                    return sb.ToString().Trim();
                }
                return null;
            }
        }
    }
}
