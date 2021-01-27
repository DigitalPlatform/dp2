using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;

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

        public BarcodeValidator()
        {
            _dom.LoadXml("<empty />");
        }

        // 2021/1/27
        public bool IsEmpty()
        {
            if (_dom == null || _dom.DocumentElement == null)
                return true;
            if (_dom.DocumentElement.Name == "empty")
                return true;
            return false;
        }

        // 选择 根下 的 validator 元素，符合 location 的那些
        // location 字符串的格式如下：location1,location2,location3
        //      可能会包含 馆代码；具体的馆藏地字符串(例如 '西城分馆/阅览室')；或者模式匹配的馆藏地字符串(例如 '西城分馆/社科*')
        List<XmlElement> GetValidators(string location)
        {
            var error = VerifyOne(location);
            if (string.IsNullOrEmpty(error) == false)
                throw new ArgumentException(error);
            XmlNodeList nodes = _dom.DocumentElement.SelectNodes("validator");
            if (nodes.Count == 0)
                return new List<XmlElement>();
            foreach (XmlElement validator in nodes)
            {
                string current = validator.GetAttribute("location");
                if (Match(location, current))
                    return new List<XmlElement> { validator };
            }
            return new List<XmlElement>();
        }

        static string VerifyOne(string one)
        {
            if (one.IndexOfAny(new char[] { ',', '*', '?' }) != -1)
                return $"字符串 '{one}' 中不应包含 ,*? 这些字符";
            return null;
        }

        static bool Match(string one, string pattern)
        {
            if (one == pattern)
                return true;
            string[] list = pattern.Split(new char[] { ',' });
            foreach (string p in list)
            {
                if (one == p)
                    return true;

                if (Regex.IsMatch(one, WildCardToRegular(p)))
                    return true;
            }
            return false;
        }

        // https://stackoverflow.com/questions/30299671/matching-strings-with-wildcard
        // If you want to implement both "*" and "?"
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        // 判断一个馆藏地是否需要进行条码变换
        // exception:
        //      可能会抛出异常
        public bool NeedTransform(string location)
        {
            // XmlNodeList nodes = _dom.DocumentElement.SelectNodes($"validator[@location='{location}']");
            var nodes = GetValidators(location);
            if (nodes.Count == 0)
                return false;
            if (nodes.Count > 1)
                throw new Exception($"馆藏地 '{location}' 定义验证规则太多 ({nodes.Count})");

            XmlElement validator = nodes[0] as XmlElement;

            // 2019/7/30
            if (validator.GetAttributeNode("suppress") != null)
                return false;

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
    string barcode)
        {
            // XmlNodeList nodes = _dom.DocumentElement.SelectNodes($"validator[@location='{location}']");
            var nodes = GetValidators(location);
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

            // ValidateResult result = null;

            XmlElement validator = nodes[0] as XmlElement;

            if (validator.GetAttributeNode("suppress") != null)
            {
                string comment = validator.GetAttribute("suppress");
                if (string.IsNullOrEmpty(comment))
                    comment = $"馆藏地 '{location}' 不打算定义验证规则";
                return new ValidateResult
                {
                    OK = false,
                    ErrorInfo = comment,
                    ErrorCode = "suppressed"    // 不打算定义验证规则
                };
            }

#if NO
            // validator 元素下的 transform 元素
            XmlElement transform = validator.SelectSingleNode("transform") as XmlElement;
            string transform_script = transform?.InnerText.Trim();
#endif

            XmlNodeList patron_or_entitys = validator.SelectNodes("patron | entity");
            foreach (XmlElement patron_or_entity in patron_or_entitys)
            {
                var ret = ProcessEntry(patron_or_entity,
                    barcode);
                if (ret == true)
                {
                    return new ValidateResult
                    {
                        OK = true,
                        Type = patron_or_entity.Name,
                    };
                }
            }

            return new ValidateResult
            {
                OK = false,
                ErrorInfo = $"号码 '{barcode}' (馆藏地属于 '{location}')既不是合法的册条码号，也不是合法的证条码号",
                ErrorCode = "notMatch"
            };
        }

        // 2021/1/18
        /*
           <validator>
               <patron>
                   <CMIS />
                   <range value='P000001-P999999' />
               </patron>
               <entity>
                   <range value='0000001-9999999'></range>
               </entity>
               <shelf>
                   <range value='0101-0909'></range>
               </shelf>
           </validator>
        * */
        // 单独验证一种类型的号码
        // type 为 "patron" "entity" "shelf" 之一
        public ValidateResult ValidateByType(
            string type,
            string barcode)
        {
            // 无论如何，先检查是否为空
            if (string.IsNullOrEmpty(barcode))
                return new ValidateResult
                {
                    OK = false,
                    ErrorInfo = "条码号不应为空",
                    ErrorCode = "emptyBarcode"
                };

            XmlElement validator = _dom.DocumentElement;
            /*
            if (validator.GetAttributeNode("suppress") != null)
            {
                string comment = validator.GetAttribute("suppress");
                if (string.IsNullOrEmpty(comment))
                    comment = $"馆藏地 '{location}' 不打算定义验证规则";
                return new ValidateResult
                {
                    OK = false,
                    ErrorInfo = comment,
                    ErrorCode = "suppressed"    // 不打算定义验证规则
                };
            }
            */

            XmlNodeList patron_or_entitys = validator.SelectNodes(type);
            foreach (XmlElement patron_or_entity in patron_or_entitys)
            {
                var ret = ProcessEntry(patron_or_entity,
                    barcode);
                if (ret == true)
                {
                    return new ValidateResult
                    {
                        OK = true,
                        Type = patron_or_entity.Name,
                    };
                }
            }

            return new ValidateResult
            {
                OK = false,
                ErrorInfo = $"号码 '{barcode}' 不是合法的 {type} 类型号码",
                ErrorCode = "notMatch"
            };
        }

        public TransformResult Transform(string location,
    string barcode)
        {
            // XmlNodeList nodes = _dom.DocumentElement.SelectNodes($"validator[@location='{location}']");
            var nodes = GetValidators(location);
            if (nodes.Count == 0)
                return new TransformResult
                {
                    OK = false,
                    ErrorInfo = $"馆藏地 '{location}' 没有定义验证规则",
                    ErrorCode = "locationDefNotFound"
                };
            if (nodes.Count > 1)
                return new TransformResult
                {
                    OK = false,
                    ErrorInfo = $"馆藏地 '{location}' 定义验证规则太多 ({nodes.Count})",
                    ErrorCode = "locationDefMoreThanOne"
                };

            TransformResult result = null;
            string current_script = ""; // 每个 range 元素里面的 scirpt 脚本

            XmlElement validator = nodes[0] as XmlElement;

            if (validator.GetAttributeNode("suppress") != null)
            {
                string comment = validator.GetAttribute("suppress");
                if (string.IsNullOrEmpty(comment))
                    comment = $"馆藏地 '{location}' 不打算定义验证规则";
                return new TransformResult
                {
                    OK = false,
                    ErrorInfo = comment,
                    ErrorCode = "suppressed"    // 不打算定义验证规则
                };
            }

            // validator 元素下的 transform 元素
            XmlElement transform = validator.SelectSingleNode("transform") as XmlElement;
            string transform_script = transform?.InnerText.Trim();

            // 校验所匹配上的类型
            string verify_type = null;

            XmlNodeList patron_or_entitys = validator.SelectNodes("patron | entity");
            foreach (XmlElement patron_or_entity in patron_or_entitys)
            {
                var ret = ProcessEntry(patron_or_entity,
                    barcode,
                    out current_script);
                if (ret == true)
                {
                    result = new TransformResult
                    {
                        OK = true,
                        Type = patron_or_entity.Name,
                    };
                    verify_type = patron_or_entity.Name;
                    break;
                }

                // 按照 verify 算法来再匹配一次。
                // patron 元素里面，只要命中过一个 range 元素，哪怕是没有 transform 属性的 range 元素，
                // 后面也就不再尝试匹配 entity 元素了
                // entity 元素亦然。
                ret = ProcessEntry(patron_or_entity,
    barcode);
                if (ret == true)
                {
                    verify_type = patron_or_entity.Name;
                    // transform = null;   // 迫使后面也不使用 transform 元素
                    break;
                }
            }

            // 没有匹配上。不需要变换
            if (result == null && transform == null)
                result = new TransformResult
                {
                    OK = true,
                    Type = verify_type,
                    TransformedBarcode = null,
                    Transformed = false,
                    ErrorInfo = $"号码 '{barcode}' (馆藏地属于 '{location}') 没有发生变换",
                    ErrorCode = "notMatch"
                };

            // 最后进行变换
            {
                // 优先用局部的 script
                if (string.IsNullOrEmpty(current_script) == false)
                    transform_script = current_script;
                if (string.IsNullOrEmpty(transform_script) == false)
                {
                    // 询问是否需要变换？(但本次并不做变换)
                    if (barcode == "?transform")
                    {
                        return new TransformResult
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
                        if (result == null)
                            result = new TransformResult();
                        result.Type = verify_type;
                        result.TransformedBarcode = transform_result;
                        result.Transformed = true;
                        result.OK = true;
                    }
                    catch (TransformException ex)
                    {
                        return new TransformResult
                        {
                            OK = false,
                            ErrorInfo = $"对条码号 '{barcode}' (属于馆藏地 '{location}') 进行变换时出错: {ex.Message}",
                            ErrorCode = "scriptError"
                        };
                    }
                    catch (Exception ex)
                    {
                        return new TransformResult
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

#if NO
        public ValidateResult Validate(string location,
            string barcode/*,
            bool do_transform = true*/)
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
#endif

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

        // 验证条码(注意，不做变换)
        // return:
        //      false   barcode 不在定义的范围内
        //      true    barcode 在定义的范围内
        public static bool ProcessEntry(XmlElement container,
            string barcode)
        {
            XmlNodeList nodes = container.SelectNodes("*");
            foreach (XmlElement range in nodes)
            {
                var attr = range.GetAttributeNode("transform");
                if (attr != null)
                    continue;   // 跳过 具有 transform 属性的元素
                if (range.Name == "range")
                {
                    string value = range.GetAttribute("value");
                    // 2020/7/24
                    string pattern = range.GetAttribute("pattern");
                    if (string.IsNullOrEmpty(pattern))
                    {
                        if (IsInRangeDefault(barcode, value))
                            return true;
                    }
                    else
                    {
                        if (StringUtil.RegexCompare(pattern, barcode) == true)
                        {
                            if (IsInRange(barcode, value))
                                return true;
                        }
                    }
                }

                if (range.Name.ToLower() == "cmis")
                {
                    if (StringUtil.IsValidCMIS(barcode))
                        return true;
                }

                if (range.Name.ToLower() == "guid")
                {
                    if (IsValidGUID(barcode))
                        return true;
                }

                if (range.Name.ToLower() == "mpn")
                {
                    if (IsMobilePhone(barcode))
                        return true;
                }
            }

            return false;
        }

        // 2020/3/13
        // https://stackoverflow.com/questions/11040707/c-sharp-regex-for-guid
        static bool IsValidGUID(string text)
        {
            return Guid.TryParse(text, out _);
        }

        // 判断是否手机号
        // https://www.jianshu.com/p/37cb110604fb
        public static bool IsMobilePhone(string input)
        {
            Regex regex = new Regex("^1[34578]\\d{9}$");
            return regex.IsMatch(input);
        }

        // 获得 transform 属性
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
                var attr = range.GetAttributeNode("transform");
                if (attr == null)
                    continue;
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

                if (range.Name.ToLower() == "guid")
                {
                    if (IsValidGUID(barcode))
                        return true;
                }

                if (range.Name.ToLower() == "mpn")
                {
                    if (IsMobilePhone(barcode))
                        return true;
                }
            }

            transform_script = "";
            return false;
        }

        // 一般性的范围比较。注：01-99，0A 也是在范围里面的哟
        static bool IsInRange(string text, string range)
        {
            var parts = StringUtil.ParseTwoPart(range, "-");
            return StringUtil.Between(text, parts[0], parts[1]);
        }

        // 默认的范围比较。注：要进行逐字符比较，这样 01-99，0A 就不在范围内了
        static bool IsInRangeDefault(string text, string range)
        {
            var parts = StringUtil.ParseTwoPart(range, "-");
            var ret = VerifyChars(parts[0], parts[1], text);
            if (ret == false)
                return false;
            return StringUtil.Between(text, parts[0], parts[1]);
        }

        // 验证 text 内的每一个字符的取值范围是否在期望以内
        // 算法是把 start 和 end 拆为 length 那么多对 char 范围，然后加以比较
        public static bool VerifyChars(string start, string end, string text)
        {
            if (start.Length != end.Length)
                throw new ArgumentException("start 和 end 应当字符数相同");
            if (text == null)
                throw new ArgumentException("text 参数值不能为 null");

            if (text.Length != start.Length)
                return false;

            for (int i = 0; i < start.Length; i++)
            {
                char min = start[i];
                char max = end[i];

                // 2020/7/25
                // 如果范围的两端都是数字字符，则默认范围是 '0'-'9'
                if (char.IsDigit(min) && char.IsDigit(max))
                {
                    min = '0';
                    max = '9';
                }

                char current = text[i];
                if (current < min || current > max)
                    return false;
            }

            return true;
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
    }

    public class TransformResult : ValidateResult
    {
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
     *              <GUID />
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
