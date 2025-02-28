using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;

using Jint;
using Jint.Native;

using DigitalPlatform.Interfaces;
using DigitalPlatform.WPF;
using DigitalPlatform;
using DigitalPlatform.RFID;
using DigitalPlatform.Script;
using DigitalPlatform.SIP;

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

        // 从 charging.xml 配置文件中获得是否校验 SIP 消息的参数
        // 值应为空，或者 fix,var,requir 的任意组合
        // return:
        //      null    不校验
        //      ""      全功能校验
        //      其它      自定义的组合校验
        public static string GetSipMessageVerifyStyle()
        {
            if (ChargingCfgDom == null)
                return "";
            var value = ChargingCfgDom.DocumentElement.SelectSingleNode("settings/key[@name='SIP消息校验']/@value")?.Value;
            if (value == null)
                return "";
            if (value == "[none]" || value == "[不校验]")
                return null;
            return value;
        }

        // 格式为 00:01:40
        public static string GetSipDetectPeriod()
        {
            if (ChargingCfgDom == null)
                return null;
            return ChargingCfgDom.DocumentElement.SelectSingleNode("settings/key[@name='SIP探测间隔']/@value")?.Value;
        }

#if REMOVED
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
#endif
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
        public static List<ScriptItem> GetMessageIoScripts()
        {
            if (ChargingCfgDom == null)
                return null;
            List<ScriptItem> results = new List<ScriptItem>();
            var script_nodes = ChargingCfgDom.DocumentElement.SelectNodes($"messageIO/script");
            foreach(XmlElement script_node in script_nodes)
            {
                var item = new ScriptItem();
                item.Lang = script_node.GetAttribute("lang");
                item.Code = script_node.InnerText;
                item.FileName = script_node.GetAttribute("fileName");
                results.Add(item);
            }

            return results;
        }

        public class ScriptItem
        {
            // 语言名称
            public string Lang { get; set; }

            // 源代码。目前仅支持 javascript 语言的源代码
            public string Code { get; set; }

            // 物理文件名。目前仅支持 DLL 文件名
            public string FileName { get; set; }
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
            if (_filters == null)
            {
                var ret = LoadSipFilters();
                if (ret.Value == -1)
                    throw new Exception($"初始化 charging.xml 中 SIP 消息过滤规则时失败: {ret.ErrorInfo}");
            }

            // var items = GetMessageIoScripts();
            // 顺次触发调用 script
            foreach(var item in _filters)
            {
                string ext = Path.GetExtension(item.FileName)?.ToLower();
                string error = null;
                if (item.Lang?.ToLower() == "javascript")
                {
                    if (string.IsNullOrEmpty(item.Code) == false)
                    {
                        error = RunJavascript(
        item,
        type,
        ref message,
        context);
                    }
                }
                else if (string.IsNullOrEmpty(item.FileName) == false
                    &&  ext == ".dll")
                {
                    error = RunDll(
item,
type,
ref message,
context);
                }
                else if (IsZhao(item.Lang))
                {
                    error = RunTransformer(
item,
type,
ref message,
context);
                }
                else
                    throw new ArgumentException($"charging.xml 中 messageIO/script/@lang 中出现了无法识别的语言名称 '{item.Lang}'");
                
                if (string.IsNullOrEmpty(error) == false)
                    return error;
            }

            return null;
        }

        static List<FilterItem> _filters = null;

        class FilterItem
        {
            public string FileName { get; set; }
            public ISipMessageFilter Filter { get; set; }

            public string Lang { get; set; }

            public string Code { get; set; }

            public MessageTransformer Transformer { get; set; }
        }

        static bool IsZhao(string name)
        {
            if (name == "ZHAO" || name == "zhao" || name == "SMTS" || name == "smts")
                return true;
            return false;
        }

        // 目前不加入 javascript 语言的事项。只是加入 DLL 或 Transformer 事项
        public static NormalResult LoadSipFilters()
        {
            if (_filters == null)
                _filters = new List<FilterItem>();
            _filters.Clear();

            var items = GetMessageIoScripts();

            foreach (var item in items)
            {
                string ext = Path.GetExtension(item.FileName)?.ToLower();
                if (item.Lang == "javascript")
                {
                    if (string.IsNullOrEmpty(item.FileName) == false)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "script/@lang 属性值为 'javascript' 时，暂不支持 script/@fileName 属性"
                        };
                    _filters.Add(new FilterItem
                    {
                        Lang = item.Lang,
                        Code = item.Code,
                        FileName = item.FileName,
                    });
                }
                else if (string.IsNullOrEmpty(item.FileName) == false
                    && ext == ".dll")
                {
                    if (string.IsNullOrEmpty(item.FileName) == false)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "script/@fileName 属性值为 'xxx.dll' 时，暂不支持 script/@lang 属性"
                        };
                    var assembly = Assembly.LoadFile(item.FileName);
                    if (assembly == null)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"SIP 消息过滤 DLL '{item.FileName}' 加载失败"
                        };
                    }

                    Type type = ScriptManager.GetDerivedClassType(
    assembly,
    "DigitalPlatform.Interfaces.ISipMessageFilter");
                    if (type == null)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"SIP 消息过滤 DLL '{item.FileName}' 中没有找到从 DigitalPlatform.Interfaces.ISipMessageFilter 派生的类。加载失败"
                        };
                    }

                    var filter = (ISipMessageFilter)type.InvokeMember(null,
    BindingFlags.DeclaredOnly |
    BindingFlags.Public | BindingFlags.NonPublic |
    BindingFlags.Instance | BindingFlags.CreateInstance,
    null,
    null,
    null);
                    if (filter == null)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"创建类 {type.Name} 的实例失败({item.FileName})"
                        };
                    }

                    _filters.Add(new FilterItem
                    {
                        Lang = item.Lang,
                        Code = item.Code,
                        FileName = item.FileName,
                        Filter = filter
                    });
                }
                else if (IsZhao(item.Lang))
                {
                    if (string.IsNullOrEmpty(item.FileName) == false)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = "script/@lang 属性值为 'ZHAO' 时，暂不支持 script/@fileName 属性"
                        };

                    var transformer = MessageTransformer.Instance();
                    try
                    {
                        transformer.Initial(item.Code);
                    }
                    catch(Exception ex)
                    {
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"Initialize MessageTransformer 出错: {ex.Message}\r\n(code='{item.Code}')"
                        };
                    }
                    _filters.Add(new FilterItem
                    {
                        Lang = item.Lang,
                        Code = item.Code,
                        Transformer = transformer,
                    });
                }
            }


            return new NormalResult();
        }

        static string RunDll(
    // string fileName,
    FilterItem filter_item,
    string type,
    ref string message,
    ScriptContext context)
        {
            /*
            var obj_item = _filters.Where(o => o.FileName == fileName).FirstOrDefault();
            if (obj_item == null)
                throw new Exception($"在 _filter 集合中没有找到 FileName 为 '{fileName}' 的事项");
            */
            var error = filter_item.Filter.TriggerScript(type,
                ref message,
                context);
            // 将变换后的 SIP2 消息记入日志
            ChargingData.LoggingMessage($"经 DLL {filter_item.FileName} 变换{(string.IsNullOrEmpty(error) ? "" : " (error=" + error)}",
                type,
                message);
            return error;
        }

        static string RunTransformer(
// string script_code,
FilterItem filter_item,
string type,
ref string message,
ScriptContext context)
        {
            /*
            var obj_item = _filters.Where(o => IsZhao(o.Lang) && o.Code == script_code).FirstOrDefault();
            if (obj_item == null)
            {
                // TODO: 取规则文本的前面若干行报错
                throw new Exception($"在 _filter 集合中没有找到 代码 为 '{script_code}' 的事项");
            }
            */
            string error = null;
            try
            {
                filter_item.Transformer.Process(message,
                    out string result);
                message = result;
            }
            catch(Exception ex)
            {
                error = ex.Message;
            }
            // 将变换后的 SIP2 消息记入日志
            // TODO: 取规则文本的前面几行，作为名字出现在 LoogingMessage() 中
            ChargingData.LoggingMessage($"经 ZHAO 规则变换{(string.IsNullOrEmpty(error) ? "" : " (error=" + error)}",
                type,
                message);
            return error;
        }


        static string RunJavascript(
            // string script_code,
            FilterItem filter_item,
            string type,
            ref string message,
            ScriptContext context)
        {
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
                    + filter_item.Code) // execute a statement
                    ?.GetCompletionValue() // get the latest statement completion value
                    ?.ToObject()?.ToString() // converts the value to .NET
                    ;
                message = GetString(engine, "message", "");
                var error = GetString(engine, "error", null);

                // 将变换后的 SIP2 消息记入日志
                // TODO: 取 javascript 代码的前若干行作为名字
                ChargingData.LoggingMessage($"经 javascript 脚本变换{(string.IsNullOrEmpty(error) ? "" : " (error=" + error)}",
                    type,
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
