using DigitalPlatform.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
                return _config.Get("global", "dp2Password", "");
            }
            set
            {
                _config.Set("global", "dp2Password", value);
            }
        }


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
    }
}
