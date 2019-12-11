using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using DigitalPlatform.Core;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace dp2SSL
{
    public class ConfigParams
    {
        ConfigSetting _config = null;

        public ConfigParams(ConfigSetting config)
        {
            _config = config;
        }

        [Display(
            Order = 1,
            Name = "URL 地址",
            Description = "dp2library 服务器的 URL 地址"
            )]
        [Category("dp2 服务器")]
        public string Dp2ServerUrl
        {
            get
            {
                return _config.Get("global", "dp2ServerUrl", "");
            }
            set
            {
                _config.Set("global", "dp2ServerUrl", value);
                App.CurrentApp.ClearChannelPool();
            }
        }

        [Display(
    Order = 2,
    Name = "用户名",
    Description = "dp2library 服务器的用户名"
    )]
        [Category("dp2 服务器")]
        public string Dp2UserName
        {
            get
            {
                return _config.Get("global", "dp2UserName", "");
            }
            set
            {
                _config.Set("global", "dp2UserName", value);
                App.CurrentApp.ClearChannelPool();
            }
        }

        [Display(
    Order = 3,
    Name = "密码",
    Description = "dp2library 服务器的密码"
    )]
        [Editor(typeof(PasswordEditor), typeof(PasswordEditor))]
        [Category("dp2 服务器")]
        public string Dp2Password
        {
            get
            {
                return App.DecryptPasssword(_config.Get("global", "dp2Password", ""));
            }
            set
            {
                _config.Set("global", "dp2Password", App.EncryptPassword(value));
                App.CurrentApp.ClearChannelPool();
            }
        }

        // 默认值 ipc://RfidChannel/RfidServer
        [Display(
Order = 4,
Name = "RFID 接口 URL",
Description = "RFID 接口 URL 地址"
)]
        [Category("RFID 接口")]
        public string RfidURL
        {
            get
            {
                return _config.Get("global", "rfidUrl", "");
            }
            set
            {
                _config.Set("global", "rfidUrl", value);
            }
        }

        // 默认值 ipc://FingerprintChannel/FingerprintServer
        [Display(
Order = 5,
Name = "指纹接口 URL",
Description = "指纹接口 URL 地址"
)]
        [Category("指纹接口")]
        public string FingerprintURL
        {
            get
            {
                return _config.Get("global", "fingerprintUrl", "");
            }
            set
            {
                _config.Set("global", "fingerprintUrl", value);
            }
        }

        // 默认值 ipc://FaceChannel/FaceServer
        [Display(
Order = 6,
Name = "人脸接口 URL",
Description = "人脸接口 URL 地址"
)]
        [Category("人脸接口")]
        public string FaceURL
        {
            get
            {
                return _config.Get("global", "faceUrl", "");
            }
            set
            {
                _config.Set("global", "faceUrl", value);
            }
        }

        // 默认值 true
        [Display(
Order = 7,
Name = "启动时全屏",
Description = "程序启动时候是否自动全屏"
)]
        [Category("启动")]
        public bool FullScreen
        {
            get
            {
                return _config.GetInt("global", "fullScreen", 1) == 1 ? true : false;
            }
            set
            {
                _config.SetInt("global", "fullScreen", value == true ? 1 : 0);
            }
        }

        // 默认值 false
        [Display(
Order = 7,
Name = "借还按钮自动触发",
Description = "借书和还书操作是否自动触发操作按钮"
)]
        [Category("自助借还操作风格")]
        public bool AutoTrigger
        {
            get
            {
                return _config.GetBoolean("ssl_operation", "auto_trigger", false);
            }
            set
            {
                _config.SetBoolean("ssl_operation", "auto_trigger", value);
            }
        }

        // 默认值 false
        [Display(
Order = 7,
Name = "读者信息显示持久",
Description = "读者卡拿走后是否保持读者信息不清除"
)]
        [Category("自助借还操作风格")]
        public bool PatronInfoLasting
        {
            get
            {
                return _config.GetBoolean("ssl_operation", "patron_info_lasting", false);
            }
            set
            {
                _config.SetBoolean("ssl_operation", "patron_info_lasting", value);
            }
        }

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

        // 默认值 true
        [Display(
Order = 8,
Name = "监控相关进程",
Description = "自动监控和重启 人脸中心 RFID中心 指纹中心等模块"
)]
        [Category("维护")]
        public bool ProcessMonitor
        {
            get
            {
                return _config.GetBoolean("global", "process_monitor", true);
            }
            set
            {
                _config.SetBoolean("global", "process_monitor", value);
            }
        }

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
            get
            {
                return _config.Get("global", "function", "自助借还");
            }
            set
            {
                _config.Set("global", "function", value);
            }
        }
    }


}
