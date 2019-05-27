using DigitalPlatform.Text;
using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
            foreach(XmlElement patron_or_entity in patron_or_entitys)
            {
                var ret = ProcessEntry(patron_or_entity, barcode, out string script);
                if (ret == true)
                {
                    if (string.IsNullOrEmpty(script) == false)
                    {
                        // 进行条码变换
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
                    return new ValidateResult
                    {
                        OK = true,
                        Type = patron_or_entity.Name,
                        Transformed = barcode
                    };
                }
            }

            return new ValidateResult { OK = false };
        }

        static string Transform(string barcode,
            string location,
            string script)
        {
            Engine engine = new Engine(cfg => cfg.AllowClr(typeof(StringUtil).Assembly));
            SetValue(engine, "barcode", barcode);
            SetValue(engine, "location", location);

            engine.Execute("var DigitalPlatform = importNamespace('DigitalPlatform');\r\n"
    + script) // execute a statement
    ?.GetCompletionValue() // get the latest statement completion value
    ?.ToObject()?.ToString() // converts the value to .NET
    ;
            string result = GetString(engine, "result", "yes");
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
            if (value == null)
                value = "";
            return value;
        }

        // return:
        //      false   barcode 不在定义的范围内
        //      true    barcode 在定义的范围内
        bool ProcessEntry(XmlElement container,
            string barcode,
            out string script)
        {
            script = "";

            XmlNodeList nodes = container.SelectNodes("*");
            foreach (XmlElement validator in nodes)
            {
                if (validator.Name == "range")
                {
                    string value = validator.GetAttribute("value");
                    if (IsInRange(barcode, value))
                        return true;
                }

                if (validator.Name.ToLower() == "cmis")
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
