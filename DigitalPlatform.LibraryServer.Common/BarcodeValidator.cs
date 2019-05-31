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

        public ValidateResult Validate(string location, string barcode)
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

            XmlElement validator = nodes[0] as XmlElement;

            XmlNodeList patron_or_entitys = validator.SelectNodes("patron | entity");
            foreach (XmlElement patron_or_entity in patron_or_entitys)
            {
                var ret = ProcessEntry(patron_or_entity, barcode, out string script);
                if (ret == true)
                {
                    if (string.IsNullOrEmpty(script) == false)
                    {
                        // 进行条码变换
                        try
                        {
                            var result = Transform(barcode,
            location,
            script);
                            return new ValidateResult
                            {
                                OK = true,
                                Type = patron_or_entity.Name,
                                Transformed = result
                            };
                        }
                        catch(Exception ex)
                        {
                            return new ValidateResult
                            {
                                OK = false,
                                ErrorInfo = $"javascript 脚本 {script} 执行时出现异常: {ex.Message}",
                                ErrorCode = "scriptError"
                            };
                        }
                    }
                    return new ValidateResult
                    {
                        OK = true,
                        Type = patron_or_entity.Name,
                        Transformed = barcode
                    };
                }
            }

            return new ValidateResult
            {
                OK = false,
                ErrorInfo = $"号码 '{barcode}' 既不是合法的册条码号，也不是合法的证条码号",
                ErrorCode = "notMatch"
            };
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
                throw new Exception(message);

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
        public string Transformed { get; set; }
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
     *      </validator>
     * </collection>
     * */
}
