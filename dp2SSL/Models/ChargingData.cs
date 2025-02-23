using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using DigitalPlatform.WPF;
using Jint;
using Jint.Native;

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

        /*
<charging>
	<messageIO>
		<script lang="javascript">

		</script>
	</messageIO>
</charging>
        * */
        // 2025/2/22
        // 从 charging.xml 配置文件中获得 消息 IO 的脚本代码
        public static string GetMessageIoScript(string lang = "javascript")
        {
            if (ChargingCfgDom == null)
                return null;
            var script_node = ChargingCfgDom.DocumentElement.SelectSingleNode($"messageIO/script[@lang='{lang}']") as XmlElement;
            if (script_node == null)
                return null;
            return script_node.InnerText;
        }

        public static string GetMessageIoLogging()
        {
            if (ChargingCfgDom == null)
                return null;
            return ChargingCfgDom.DocumentElement.SelectSingleNode($"messageIO/@logging")?.Value;
        }

        #region 消息 IO 脚本执行

        // parameters:
        //      type    消息的类型。"request" "response" 之一
        public static void LoggingMessage(string prefix, string type, string message)
        {
            var logging = GetMessageIoLogging();
            if (logging == "on" || logging == "yes")
            {
                WpfClientInfo.WriteDebugLog($"{prefix} {type} message: '{message}'");
            }
        }

        // parameters:
        //      type    消息的类型。"request" "response" 之一
        public static string TriggerScript(string type,
            ref string message,
            ScriptContext context)
        {
            var lang = "javascript";
            string script_code = GetMessageIoScript(lang);
            if (string.IsNullOrEmpty(script_code))
                return null;

            try
            {
                Engine engine = new Engine(cfg => cfg.AllowClr(typeof(App).Assembly));

                SetValue(engine,
                    "type",
                    type);
                SetValue(engine,
                    "message",
                    message);
                SetValue(engine,
                    "context",
                    context);

                engine.Execute("var DigitalPlatform = importNamespace('dp2SSL');\r\n"
                    + script_code) // execute a statement
                    ?.GetCompletionValue() // get the latest statement completion value
                    ?.ToObject()?.ToString() // converts the value to .NET
                    ;
                message = GetString(engine, "message", "");
                var error = GetString(engine, "error", null);

                // 将变换后的 SIP2 消息记入日志
                ChargingData.LoggingMessage($"经 {lang} 脚本变换{(string.IsNullOrEmpty(error) ? "" : " (error="+error)}",
                    "response",
                    message);
                return error;
            }
            catch (Exception ex)
            {
                // TODO: 截取脚本代码的前面若干行出现在报错信息中
                return $"执行消息 {type} 脚本时出现异常: {ex.Message}";
            }
        }

        static void SetValue(Engine engine, string name, object o)
        {
            if (o == null)
                engine.SetValue(name, JsValue.Null);
            else
                engine.SetValue(name, o);
        }

        static string GetString(Engine engine, string name, string default_value)
        {
            var result_obj = engine.GetValue(name);
            string value = result_obj.IsUndefined() ? default_value : result_obj.ToObject().ToString();
            if (value == null)
                value = "";
            return value;
        }

        #endregion
    }
}
