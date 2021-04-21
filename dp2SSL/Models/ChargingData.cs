using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.WPF;

namespace dp2SSL
{
    /// <summary>
    /// 自助借还功能的数据模型
    /// </summary>
    public static class ChargingData
    {
        static XmlDocument _chargingCfgDom = null;

        public static XmlDocument ChargingCfgDom
        {
            get
            {
                if (_chargingCfgDom == null)
                {
                    try
                    {
                        InitialChargingDom();
                    }
                    catch (FileNotFoundException ex)
                    {
                        _chargingCfgDom = new XmlDocument();
                        _chargingCfgDom.LoadXml("<root />");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"装载配置文件 charging.xml 时出现异常: {ex.Message}", ex);
                    }
                }
                return _chargingCfgDom;
            }
        }

        public static string ChargingFilePath
        {
            get
            {
                string cfg_filename = System.IO.Path.Combine(WpfClientInfo.UserDir, "charging.xml");
                return cfg_filename;
            }
        }

        static void InitialChargingDom()
        {
            string cfg_filename = ChargingFilePath;
            XmlDocument cfg_dom = new XmlDocument();
            cfg_dom.Load(cfg_filename);
            // 验证 root 元素里面的 verify 属性
            var verify_value = cfg_dom.DocumentElement.GetAttribute("verify");
            if (string.IsNullOrEmpty(verify_value) == false
                && verify_value != "数字平台")
                throw new Exception($"配置文件 {cfg_filename} 根元素 verify 属性不正确");
            _chargingCfgDom = cfg_dom;
        }

        // 从 charging.xml 配置文件中获得 图书标签严格要求机构代码 参数
        public static bool GetBookInstitutionStrict()
        {
            if (ChargingCfgDom == null)
                return true;
            var value = ChargingCfgDom.DocumentElement.SelectSingleNode("settings/key[@name='图书标签严格要求机构代码']/@value")?.Value;
            if (string.IsNullOrEmpty(value))
                value = "true";

            return value == "true";
        }

        // 从 charging.xml 配置文件中获得 启用小票打印机即将缺纸警告 参数
        public static bool GetPosPrintPaperWillOut()
        {
            if (ChargingCfgDom == null)
                return true;
            var value = ChargingCfgDom.DocumentElement.SelectSingleNode("settings/key[@name='启用小票打印机即将缺纸警告']/@value")?.Value;
            if (string.IsNullOrEmpty(value))
                value = "true";

            return value == "true";
        }
    }
}
