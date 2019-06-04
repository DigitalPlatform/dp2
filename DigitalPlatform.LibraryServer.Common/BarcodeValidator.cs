using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Jint;
using Jint.Native;

using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer.Common
{
    /// <summary>
    /// 条码号校验器
    /// </summary>
    public class BarcodeValidator
    {
        XmlDocument _dom = new XmlDocument();

        public BarcodeValidator(string definition)
        {
            _dom.LoadXml(definition);
        }

        // 判断一个馆藏地是否需要进行条码变换
        // exception:
        //      可能会抛出异常
        public bool NeedValidate(string location)
        {
            XmlNodeList nodes = _dom.DocumentElement.SelectNodes($"validator[@location='{location}']");
            if (nodes.Count == 0)
                return false;
            if (nodes.Count > 1)
                throw new Exception($"馆藏地 '{location}' 定义验证规则太多 ({nodes.Count})");

            XmlElement validator = nodes[0] as XmlElement;

            // validator 元素下的 transform 元素
            if (validator.SelectSingleNode("transform") is XmlElement transform)
                return true;

            XmlNodeList patron_or_entitys = validator.SelectNodes("patron | entity");
            foreach (XmlElement patron_or_entity in patron_or_entitys)
            {
                XmlNodeList attrs = patron_or_entity.SelectNodes("*/@transform");
                if (attrs.Count > 0)
                    return true;
            }

            return false;
        }

        public ValidateResult Validate(string location,
            string barcode,
            bool do_transform = true)
        {
            XmlNodeList nodes = _dom.DocumentElement.SelectNodes($"validator[@location='{location}']");
            if (nodes.Count == 0)
                return new ValidateResult
                {
                    OK = false,
                    ErrorInfo = $"馆藏地 '{location}' 没有定义验证规则",
                    ErrorCode = "locationDefNotFound"
                };
            if (nodes.Count > 1)
                return new ValidateResult
                {
                    OK = false,
                    ErrorInfo = $"馆藏地 '{location}' 定义验证规则太多 ({nodes.Count})",
                    ErrorCode = "locationDefMoreThanOne"
                };

            ValidateResult result = null;
            string current_script = ""; // 每个 range 元素里面的 scirpt 脚本

            XmlElement validator = nodes[0] as XmlElement;

            // validator 元素下的 transform 元素
            XmlElement transform = validator.SelectSingleNode("transform") as XmlElement;
            string transform_script = transform?.InnerText.Trim();

            XmlNodeList patron_or_entitys = validator.SelectNodes("patron | entity");
            foreach (XmlElement patron_or_entity in patron_or_entitys)
            {
                var ret = ProcessEntry(patron_or_entity,
                    barcode,
                    out current_script);
                if (ret == true)
                {
                    result = new ValidateResult
                    {
                        OK = true,
                        Type = patron_or_entity.Name,
                    };
                    break;
                }
            }

            if (result == null)
                result = new ValidateResult
                {
                    OK = false,
                    ErrorInfo = $"号码 '{barcode}' (馆藏地属于 '{location}')既不是合法的册条码号，也不是合法的证条码号",
                    ErrorCode = "notMatch"
                };

            // 最后进行变换
            {
                // 优先用局部的 script
                if (string.IsNullOrEmpty(current_script) == false)
                    transform_script = current_script;
                if (do_transform &&
                    string.IsNullOrEmpty(transform_script) == false)
                {
                    // 询问是否需要变换？(但本次并不做变换)
                    if (barcode == "?transform")
                    {
                        return new ValidateResult
                        {
                            OK = true,
                            TransformedBarcode = barcode,
                            Transformed = true
                        };
                    }

                    // 进行条码变换
                    try
                    {
                        var transform_result = Transform(barcode,
        location,
        transform_script);
                        result.TransformedBarcode = transform_result;
                        result.Transformed = true;
                        result.OK = true;
                    }
                    catch(TransformException ex)
                    {
                        return new ValidateResult
                        {
                            OK = false,
                            ErrorInfo = $"对条码号 '{barcode}' (属于馆藏地 '{location}') 进行变换时出错: {ex.Message}",
                            ErrorCode = "scriptError"
                        };
                    }
                    catch (Exception ex)
                    {
                        return new ValidateResult
                        {
                            OK = false,
                            ErrorInfo = $"javascript 脚本 {transform_script} 执行时出现异常(条码号 '{barcode}' (属于馆藏地 '{location}')): {ex.Message}",
                            ErrorCode = "scriptError"
                        };
                    }
                }
            }

            return result;
        }

        static string Transform(string barcode,
            string location,
            string script)
        {
            Engine engine = new Engine(cfg => cfg.AllowClr(typeof(StringUtil).Assembly));
            SetValue(engine, "barcode", barcode);
            SetValue(engine, "location", location);

            string result = engine.Execute("var DigitalPlatform = importNamespace('DigitalPlatform');\r\n"
    + script) // execute a statement
    ?.GetCompletionValue() // get the latest statement completion value
    ?.ToObject()?.ToString() // converts the value to .NET
    ;
            string var_result = GetString(engine, "result", null);
            if (var_result != null)
                result = var_result;
            string message = GetString(engine, "message", "");
            if (string.IsNullOrEmpty(message) == false)
                throw new TransformException(message);

            return result;
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
            //if (value == null)
            //    value = "";
            return value;
        }

        // return:
        //      false   barcode 不在定义的范围内
        //      true    barcode 在定义的范围内
        bool ProcessEntry(XmlElement container,
            string barcode,
            out string transform_script)
        {
            transform_script = "";

            XmlNodeList nodes = container.SelectNodes("*");
            foreach (XmlElement range in nodes)
            {
                transform_script = range.GetAttribute("transform");
                if (range.Name == "range")
                {
                    string value = range.GetAttribute("value");
                    if (IsInRange(barcode, value))
                        return true;
                }

                if (range.Name.ToLower() == "cmis")
                {
                    if (StringUtil.IsValidCMIS(barcode))
                        return true;
                }
            }

            return false;
        }

        static bool IsInRange(string text, string range)
        {
            var parts = StringUtil.ParseTwoPart(range, "-");
            return StringUtil.Between(text, parts[0], parts[1]);
        }
    }

#if NO
    public class TransformHost
    {
        public string Barcode { get; set; }
        public string Location { get; set; }
    }
#endif

    public class ValidateResult
    {
        // [out]
        public string ErrorInfo { get; set; }
        // [out]
        public string ErrorCode { get; set; }
        // [out]
        public bool OK { get; set; }

        // [out]
        public string Type { get; set; }

        // [out] 变换后的条码号字符串
        public string TransformedBarcode { get; set; }

        // [out] 是否进行过变换(注意即便进行过变换，变换前后的条码号也可能完全相同)
        public bool Transformed { get; set; }
    }

    // 变换时出错。用于脚本代码中用 message 环境变量主动返回出错信息
    public class TransformException : Exception
    {

        public TransformException(string s)
            : base(s)
        {
        }

    }

    /*
     * <collection>
     *      <validator location="海淀分馆" >
     *          <patron>
     *              <CMIS />
     *              <range value="P000001-P999999" transform="..." >
     *              <range>"P000001", "P999999"</range>
     *              <range>P000001-P999999</range>
     *          </patron>
     *          <entity>
     *              <range>20000001-20999999</range>
     *          </entity>
     *          <transform>
     *              script code in here
     *          </transform>
     *      </validator>
     * </collection>
     * */
}
