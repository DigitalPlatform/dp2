
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

namespace dp2LibraryApiTester
{
    /// <summary>
    /// 实用工具函数
    /// </summary>
    public static class Utility
    {
        #region 和 verify 元素有关的函数

        public static void Verify(
            VerifyContext context,
            XmlElement trigger)
        {
            var verify = FindVerifyElement(trigger);
            if (verify == null)
                return;
            var nodes = verify.SelectNodes("*");
            foreach (XmlElement node in nodes)
            {
                if (node.Name == "compareBiblio")
                    CompareBiblio(context, node);
            }
        }

        /*
				<compareBiblio name="去掉 xxx 比较，期待相同"
						assert="equal"
						xml_pair="target_starting,target_read"
						ignore_elements="refID,operations,unimarc:997"/>
				<compareBiblio name="包含 xxx 比较，期待不同"
						assert="not_equal"
						xml_pair="target_starting,target_read"
						ignore_elements="default,refID,operations,unimarc:997,-refID,-997"/>
         * */
        // TODO: 设计一些简写法，比如 <compareBiblio>全部字段兑现</compareBiblio> "200没有兑现" "300没有兑现"
        static void CompareBiblio(VerifyContext context,
            XmlElement compare_element)
        {
            if (context.target_read == null)
                throw new ArgumentException($"调用 CompareBiblio() 之前应当把 context.target_read 准备好");
            // 取出两条 XML 记录
            var xml_pair = compare_element.GetAttribute("xml_pair");
            GetTwoXml(context,
                xml_pair,
                out string xml1,
                out string xml2);
            var ignore_elements = BuildNames(GetAttribute(compare_element, "ignore_elements"));
            var comparerable_xml1 = RemoveElements(xml1, ignore_elements);
            var comparerable_xml2 = RemoveElements(xml2, ignore_elements);

            var assert = compare_element.GetAttribute("assert");
            var requir_equal = assert != "not_equal" && assert != "!";

            if (XmlUtility.AreEqualByC14N(comparerable_xml1, comparerable_xml2) != requir_equal)
                throw new Exception($"验证 {compare_element.OuterXml} 时\r\n两端 {xml_pair} 比较不符合预期的{(requir_equal ? "一致" : "不一致")}\r\n{DomUtil.GetIndentXml(comparerable_xml1)}\r\n -- 和 --\r\n{DomUtil.GetIndentXml(comparerable_xml2)}");

            var name = compare_element.GetAttribute("name");
            if (string.IsNullOrEmpty(name))
                name = compare_element.OuterXml;
            DataModel.SetMessage($"验证 compareBiblio {name} 符合预期");
        }

        public static string GetAttribute(XmlElement element, string attr_name)
        {
            if (element.HasAttribute(attr_name) == false)
                return null;
            return element.GetAttribute(attr_name);
        }

        static void GetTwoXml(VerifyContext context,
            string xml_pair,
            out string xml1,
            out string xml2)
        {
            var parts = StringUtil.ParseTwoPart(xml_pair, ",");

            xml1 = GetXml(parts[0]);
            xml2 = GetXml(parts[1]);

            // target_start
            // target_final
            string GetXml(string name)
            {
                switch (name)
                {
                    case "target_starting":
                        return context.target_starting;
                    case "target_copying":
                        return context.target_copying;
                    case "target_read":
                        return context.target_read;
                    default:
                        throw new ArgumentException($"无法识别的 xml_pair 参数值 '{name}'");
                }
            }
        }

        // 逗号间隔的列表。每个单元可以是一个元素名。或者 MARC 字段名。前面引导减号表示从集合中去掉这个名字。
        // default 表示默认的三个名字。
        // parameters:
        //      ignore_names_value  ignore_elements 属性值。逗号间隔的字符串。
        //                          如果为 null 表示采用默认值。如果为 "" 则表示什么都没有(不忽略任何元素)
        static string[] BuildNames(string ignore_names_value)
        {
            if (ignore_names_value == null)
                ignore_names_value = "default";
            List<string> names = new List<string>();
            foreach (var segment in ignore_names_value.Split(','))
            {
                string action = "+";
                var name = segment;

                if (char.IsLetterOrDigit(name.Substring(0, 1)[0]) == false)
                {
                    action = segment.Substring(0, 1);
                    name = segment.Substring(1);
                }

                var values = GetValues(name);
                if (action == "-")
                {
                    foreach (var value in values)
                    {
                        names.Remove(value);
                    }
                }
                else
                {
                    if (action != "+")
                        throw new ArgumentException($"参数 '{segment}' 中出现了无法识别的动作符号 '{action}'");
                    foreach (var value in values)
                    {
                        if (names.IndexOf(value) == -1)
                            names.Add(value);
                    }
                }
            }

            return names.ToArray();

            string[] GetValues(string name)
            {
                string[] values = null;
                if (name == "default")
                {
                    // default 表示默认的三个名字
                    values = new string[] {
                "refID",
                "operations",
                "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
            };
                }
                else
                    values = new string[] { name };

                return values.Select(o =>
                {
                    // 替换一些 name 的简写形态
                    if (name.Length == 3 && StringUtil.IsNumber(name))
                        return ($"//{{http://dp2003.com/UNIMARC}}:datafield[@tag='{name}']");
                    else
                        return (name);
                }).ToArray();
            }
        }


#if REMOVED
        {
            var ignore_elements = new string[] {
                "refID",
                "operations",
                "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
            };

            string ignore_elements_value = null;
            if (compare_element.HasAttribute("ignore_elements"))
                ignore_elements = compare_element.GetAttribute("ingore_elements")?.Split(',');

            string[] list = methods.Split(',');
            // 分离出 compare_ignore_fields 参数
            var ignore_fields_value = list.Where(o => o.StartsWith("compare_ignore_fields:"))
                .Select(o => o.Substring("compare_ignore_fields:".Length))
                .FirstOrDefault();

            // 分离出 compare_has_fields
            var has_fields_value = list.Where(o => o.StartsWith("compare_has_fields:"))
                .Select(o => o.Substring("compare_has_fields:".Length))
                .FirstOrDefault();

            ignore_elements = AdjustNames(ignore_elements,
                ignore_fields_value.Split('|'));

            foreach (var segment in list)
            {
                // 分离参数部分
                var parts = StringUtil.ParseTwoPart(segment, ":");
                var action = parts[0];
                var parameters = parts[1];

                if (action.StartsWith("compare_"))
                    continue;

                // 目标记录内容没有变化，和原先准备好时的内容一致
                if (action == "=existing_origin")
                {
                    var comparerable_target_existing_xml = Utility.RemoveElements(target_existing_xml, ignore_elements);

                    var verify_ret = Utility.VerifyBiblioRecord(channel,
    target_path,
    null,
    /*
    new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
    },
    */
    ignore_elements,
    comparerable_target_existing_xml,
    null);
                    if (verify_ret.Value == -1)
                        throw new Exception($"verify_target_biblio 属性值 {action} 执行验证目标记录失败: {verify_ret.ErrorInfo}");
                }

                // 目标记录内容和测试意图保存的新内容一致
                if (action == "=newly")
                {
                    var comparerable_target_xml = Utility.RemoveElements(target_newly_xml, ignore_elements);

                    var verify_ret = Utility.VerifyBiblioRecord(channel,
    target_path,
    null,
    ignore_elements,
    comparerable_target_xml,
    null);
                    if (verify_ret.Value == -1)
                        throw new Exception($"verify_target_biblio 属性值 {action} 执行验证目标记录失败: {verify_ret.ErrorInfo}");
                }

                if (action == "timestamp")
                {
                    var verify_ret = Utility.VerifyBiblioTimestamp(channel,
    target_path,
    target_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"verify_target_biblio 属性值 {action} 执行验证目标记录失败: {verify_ret.ErrorInfo}");
                }
            }

        }
#endif

        // 从 trigger 元素开始寻找 verify 元素
        static XmlElement FindVerifyElement(XmlElement trigger)
        {
            // 先检查一下 trigger 对象是否具有 verify_ref 属性
            var verify_ref = GetAttribute(trigger, "verify_ref");
            if (verify_ref != null)
                return FindRefNode(trigger.OwnerDocument.DocumentElement,
                    "verify",
                    verify_ref);

            var node = trigger.SelectSingleNode("verify") as XmlElement;
            if (node == null)
                return null;
            var ref_name = node.GetAttribute("ref");
            if (string.IsNullOrEmpty(ref_name))
                return node;
            return FindRefNode(node.OwnerDocument.DocumentElement,
                "verify",
                ref_name);
        }

        // 找到和 start_element 相同元素名的 name 属性为指定内容的元素
        static XmlElement FindRefNode(
            XmlElement root,
            string element_name,
            string ref_name)
        {
            // var element_name = "verify";
            var result = root.SelectSingleNode($"//{element_name}[@name='{ref_name}']") as XmlElement;
            if (result == null)
                throw new Exception($"未能找到 name 属性值为 '{ref_name}' 的 verify 元素");
            return result;
        }

        // 和校验有关的数据结构
        public class VerifyContext
        {
            // 目标记录路径
            public string TargetPath { get; set; }
            // 目标书目记录 XML，指测试开始时计划写入的内容
            public string target_starting { get; set; }
            // 目标书目记录 XML，指测试 copy 动作时计划写入的内容
            public string target_copying { get; set; }

            // 目标书目记录 XML，指验证的当时，数据库中已经写入的内容(读出来的)
            public string target_read { get; set; }

            // 目标书目记录的时间戳。指测试开始时自动写入时的时间戳
            public byte[] target_old_timestamp { get; set; }

            // 目标书目记录的时间戳，指测试完成时已经覆盖写入的时间戳。
            public byte[] target_new_timestamp { get; set; }

        }

        #endregion

        // 获得 trigger 元素的路径。便于寻找位置
        public static string GetTriggerPath(XmlElement trigger)
        {
            List<string> levels = new List<string>();
            while (trigger != null)
            {
                int number = GetNumber(trigger);
                levels.Insert(0, trigger.Name + (number == 0 ? "" : "(" + (number + 1).ToString() + ")"));
                trigger = trigger.ParentNode as XmlElement;
            }
            return StringUtil.MakePathList(levels, "/");

            // 数一下自己是同名兄弟中的第几个？
            int GetNumber(XmlElement start)
            {
                var name = start.Name;
                var current = start.PreviousSibling;
                int i = 0;
                while (current != null)
                {
                    if (current.Name == name)
                        i++;
                    current = current.PreviousSibling;
                }

                return i;
            }
        }

        //      trigger 包含期望的结果的 XmlElement 对象
        //              	例如 <trigger ret='0' code='AccessDenied' info='*未包含*'/>
        public static void AssertResult(
            XmlElement trigger,
string api_name,
long ret,
//long correct_ret,
ErrorCode code,
//ErrorCode correct_code,
string error,
//string contain_error = null,
bool display = true)
        {
            if (display)
                DataModel.SetMessage($"{api_name}  ret={ret}  code={code} error={error}");

            if (trigger.HasAttribute("info"))
            {
                // info 属性值的用法: 前导一个 ! 表示否定条件。
                // 如果前导 '@' 表示正则表达式。否则就是类似 DOS dir 命令的通配符用法
                var contain_error = trigger.GetAttribute("info");
                if (contain_error.StartsWith("!"))
                {
                    var text = contain_error.Substring(1);
                    if (Match(error, text) == true)
                        throw new ArgumentException($"{api_name} error=****** {error} ******\r\n中包含了不希望出现的内容 '{contain_error}'");
                }
                else if (Match(error, contain_error) == false)
                    throw new ArgumentException($"{api_name} error=****** {error} ******\r\n中未包含期待的内容 '{contain_error}'");
            }
            if (trigger.HasAttribute("ret"))
            {
                if (int.TryParse(trigger.GetAttribute("ret"), out int value) == false)
                    throw new ArgumentException($"trigger 元素中 ret 属性值 '{value}' 不合法。\r\n{trigger.OuterXml}");
                if (ret != value)
                    throw new ArgumentException($"{api_name} ret={ret} 与期待的 {value} 不符");
            }
            if (trigger.HasAttribute("code"))
            {
                if (Enum.TryParse<ErrorCode>(trigger.GetAttribute("code"), out ErrorCode value) == false)
                    throw new ArgumentException($"trigger 元素中 code 属性值 '{value}' 不合法，无法解析为 ErrorCode 枚举。\r\n{trigger.OuterXml}");
                if (code != value)
                    throw new ArgumentException($"{api_name} ErrorCode={code} 与期待的 {value} 不符");
            }
        }

        public static bool Match(string strValue,
            string strMatchCase)
        {
            string strPattern = "";
            // Regular expression
            if (strMatchCase.Length >= 1
                && strMatchCase[0] == '@')
            {
                strPattern = strMatchCase.Substring(1);
            }
            else
                strPattern = WildcardToRegex(strMatchCase);

            return (StringUtil.RegexCompare(strPattern,
                    RegexOptions.None,
                    strValue) == true);

            string WildcardToRegex(string pattern)
            {
                return "^" + Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".")
                + "$";
            }
        }


        public static void AssertResult(
    string api_name,
    long ret,
    long correct_ret,
    ErrorCode code,
    ErrorCode correct_code,
    string error,
    string contain_error = null,
    bool display = true)
        {
            if (display)
                DataModel.SetMessage($"{api_name}  ret={ret}  error={error}  code={code}");
            if (contain_error != null)
            {
                if (contain_error.StartsWith("!"))
                {
                    var text = contain_error.Substring(1);
                    if (error.Contains(text))
                        throw new ArgumentException($"{api_name} error={error} 中包含了不希望出现的内容 '{contain_error}'");
                }
                else if (error.Contains(contain_error) == false)
                    throw new ArgumentException($"{api_name} error={error} 中未包含期待的内容 '{contain_error}'");
            }
            if (ret != correct_ret || code != correct_code)
                throw new ArgumentException($"{api_name} ret={ret} channel.ErrorCode={code} 与期待的 {correct_ret} {correct_code} 不符");
        }


        public static NormalResult VerifyBiblioRecord(string origin_xml,
    string new_xml,
    string[] ignore_new_elements)
        {
            return VerifyBiblioRecord(origin_xml,
            null,
            new_xml,
            ignore_new_elements);
        }


        public static NormalResult VerifyBiblioRecord(string origin_xml,
            string[] ignore_origin_elements,
            string new_xml,
            string[] ignore_new_elements)
        {
            var comparerable = Utility.RemoveElements(new_xml, ignore_new_elements);
            comparerable = DomUtil.GetIndentXml(comparerable);
            if (ignore_origin_elements != null && ignore_origin_elements.Length > 0)
                origin_xml = Utility.RemoveElements(origin_xml, ignore_origin_elements);
            origin_xml = DomUtil.GetIndentXml(origin_xml);

            if (XmlUtility.AreEqualByC14N(comparerable, origin_xml) == false)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = $"从 dp2library 获得的书目记录 XML\r\n{DomUtil.GetIndentXml(new_xml)}\r\n中可比较的部分\r\n{comparerable}\r\n和预期的\r\n{origin_xml}\r\n不一致"
                };
            return new NormalResult();
        }

        public static MarcRecord BuildBiblioRecord(
string strTitle,
string strStyle)
        {
            MarcRecord record = new MarcRecord();
            record.add(new MarcField('$', "200  $a" + strTitle));
            record.add(new MarcField('$', "690  $aI247.5"));
            record.add(new MarcField('$', "701  $a测试著者"));
            return record;
        }

        public static NormalResult VerifyBiblioRecord(LibraryChannel channel,
    string recpath,
    string[] has_elements,    // 返回的 XML 中应当有这些元素
    string[] ignore_elements,   // 和 correct_xml 比较之前，返回的 XML 中应该删除这些元素
    string correct_xml,         // 用于对比的 XML
    byte[] timestamp)
        {
            return VerifyBiblioRecord(channel,
            recpath,
            has_elements,
            ignore_elements,
            correct_xml,
            null,
            timestamp);
        }

        // 验证书目记录 XML
        public static NormalResult VerifyBiblioRecord(LibraryChannel channel0,
            string recpath,
            string[] has_elements,    // 返回的 XML 中应当有这些元素
            string[] ignore_elements,   // 和 correct_xml 比较之前，返回的 XML 中应该删除这些元素
            string correct_xml,         // 用于对比的 XML
            string[] corrent_ignore_elements, // 对比前要先将 correct_xml 中去掉这些元素
            byte[] timestamp = null)
        {
            var channel = DataModel.GetChannel();
            try
            {
                var ret = channel.GetBiblioInfos(null,
                    recpath,
                    "",
                    new string[] { "xml" },
                    out string[] results,
                    out byte[] back_timestamp,
                    out string error);
                if (ret == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"验证书目记录时出错: {error}",
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                if (results == null || results.Length != 1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "results error"
                    };

                if (has_elements != null && has_elements.Length > 0)
                {
                    error = Utility.AssertHasElements(
                    results[0],
                    has_elements);
                    if (error != null)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = $"返回的 XML 记录中: {error}"
                        };
                }

                // TODO: 两侧都去掉 ignore_elements 指定的元素再比较
                var return_xml = results[0];
                var comparerable = Utility.RemoveElements(return_xml, ignore_elements);
                comparerable = DomUtil.GetIndentXml(comparerable);
                if (corrent_ignore_elements != null && corrent_ignore_elements.Length > 0)
                    correct_xml = Utility.RemoveElements(correct_xml, corrent_ignore_elements);
                correct_xml = DomUtil.GetIndentXml(correct_xml);
                if (XmlUtility.AreEqualByC14N(comparerable, correct_xml) == false)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"从 dp2library 获得的书目记录 {recpath} XML\r\n{DomUtil.GetIndentXml(return_xml)}\r\n中可比较的部分\r\n{comparerable}\r\n和预期的\r\n{correct_xml}\r\n不一致"
                    };
                if (timestamp != null
                    && ByteArray.Compare(back_timestamp, timestamp) != 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"从 dp2library 获得的书目记录 {recpath} 时间戳 [{ByteArray.GetHexTimeStampString(back_timestamp)}] 和预期的 [{ByteArray.GetHexTimeStampString(timestamp)}] 不一致"
                    };

                // 检查 dprms:file 元素获取对象并验证内容


                return new NormalResult();
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

        public static string GetBiblioXml(string recpath,
            bool throw_exception = true)
        {
            var channel = DataModel.GetChannel();
            try
            {
                var ret = channel.GetBiblioInfos(null,
                    recpath,
                    "",
                    new string[] { "xml" },
                    out string[] results,
                    out byte[] back_timestamp,
                    out string error);
                if (ret == -1)
                {
                    if (channel.ErrorCode == ErrorCode.NotFound
                        && throw_exception == false)
                        return null;
                    throw new Exception($"获得书目记录时出错: {error}");
                }
                if (results == null || results.Length != 1)
                    throw new Exception($"获得书目记录时出错: results error");

                return results[0];
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

        public static NormalResult VerifyBiblioTimestamp(LibraryChannel channel0,
    string recpath,
    byte[] timestamp)
        {
            var channel = DataModel.GetChannel();
            try
            {
                var ret = channel.GetBiblioInfos(null,
                    recpath,
                    "",
                    new string[] { "xml" },
                    out string[] results,
                    out byte[] back_timestamp,
                    out string error);
                if (ret == -1)
                {
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"验证书目记录时出错: {error}",
                        ErrorCode = channel.ErrorCode.ToString()
                    };
                }

                if (results == null || results.Length != 1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = "results error"
                    };

                if (ByteArray.Compare(back_timestamp, timestamp) != 0)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = $"从 dp2library 获得的书目记录 {recpath} 时间戳 [{ByteArray.GetHexTimeStampString(back_timestamp)}] 和预期的 [{ByteArray.GetHexTimeStampString(timestamp)}] 不一致"
                    };
                return new NormalResult();
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }


        public static NormalResult DeleteMemoryUsers(LibraryChannel channel)
        {

            StringUtil.RemoveDupNoSort(ref _usedUserNames);
            if (_usedUserNames.Count > 0)
            {
                DataModel.SetMessage($"正在删除用户 {StringUtil.MakePathList(_usedUserNames, ",")} ...");
                int nRet = Utility.DeleteUsers(channel,
                    _usedUserNames,
                    out string error);
                if (nRet == -1)
                    return new NormalResult
                    {
                        Value = -1,
                        ErrorInfo = error
                    };
            }

            return new NormalResult();
        }

        public static List<string> _usedUserNames = new List<string>();

        // 获取用于测试的账户信息
        public delegate UserInfo delegate_getUser(LibraryChannel super_channel);

        // 触发实质性测试动作
        public delegate NormalResult delegate_testAction(LibraryChannel test_channel,
            UserInfo user);

        // 清理环境，便于下一轮测试正常进行
        public delegate void delegate_cleanUp(LibraryChannel super_channel);


        public static NormalResult _test(
            delegate_getUser func_getuser,
            delegate_testAction func_test,
            delegate_cleanUp func_cleanUp,
            CancellationToken token)
        {

            LibraryChannel super_channel = DataModel.GetChannel();
            try
            {
                // 准备测试账户
                UserInfo user = null;

                if (func_getuser != null)
                    user = func_getuser?.Invoke(super_channel);
                if (user == null)   // 默认的
                    user = new UserInfo
                    {
                        UserName = "_test_account",
                        Rights = "",
                        Access = "",
                    };

                MemoryUser(user);
                var result = Utility.PrepareAccount(super_channel, user);
                if (result.Value == -1)
                {
                    // return result;
                    throw new Exception($"创建账户 '{user.UserName}' 失败: {result.ErrorInfo}");
                }

                if (func_test != null)
                {
                    LibraryChannel test_channel = DataModel.NewChannel(user.UserName, "");
                    try
                    {
                        // 进行测试操作
                        result = func_test(test_channel, user);
                        if (result.Value == -1)
                            return result;

                        return new NormalResult();
                    }
                    finally
                    {
                        DataModel.DeleteChannel(test_channel);
                    }
                }
                return new NormalResult();
            }
            finally
            {
                func_cleanUp?.Invoke(super_channel);
                DataModel.ReturnChannel(super_channel);
            }
        }

        // 记忆用户信息。最后结尾时用于清除账户
        static void MemoryUser(UserInfo user)
        {
            if (_usedUserNames.Contains(user.UserName) == false)
                _usedUserNames.Add(user.UserName);
        }

        public delegate string[] delegate_select(string[] filenames);

        // 指定一个目录和通配符进行运行
        public static void RunDirectory(
delegate_test1 func_test,
string directory,
string wildcard,
delegate_select func_select,
CancellationToken token)
        {
            List<string> filenames = new List<string>();
            var di = new DirectoryInfo(directory);
            foreach (var fi in di.GetFiles(wildcard))
            {
                filenames.Add(fi.FullName);
            }

            if (func_select != null)
            {
                filenames = func_select(filenames.ToArray())?.ToList();
                if (filenames == null)
                {
                    DataModel.SetMessage("放弃选择", "error");
                    return;
                }
            }

            {
                RunMany(func_test,
    filenames.ToArray(),
    token);
            }
        }

        public static void RunMany(
delegate_test1 func_test,
string[] filenames,
CancellationToken token)
        {
            foreach (var filename in filenames)
            {
                RunMany(func_test,
    filename,
    token);
            }
        }

        public delegate NormalResult delegate_test1(
            XmlElement trigger,
            CancellationToken token);

        // 用 XML 文件驱动
        public static void RunMany(
    delegate_test1 func_test,
    string xml_filename,
    CancellationToken token)
        {
            var results = new List<NormalResult>();
            XmlDocument dom = new XmlDocument();
            dom.Load(xml_filename);
            var triggers = dom.DocumentElement.SelectNodes("//trigger");
            foreach (XmlElement trigger in triggers)
            {
                var name = GetProperty(trigger, "name");
                var style = GetProperty(trigger, "style");

                // 跳过自己和上级具有 enabled='false' 属性的 trigger
                if (HasEnabled(trigger) == false)
                {
                    DataModel.SetMessage($"跳过 {Utility.GetProperty(trigger, "comment")}{trigger.InnerText.Replace("\n", "\r").Replace("\r", " ")} name={name} style={style}", "warning");
                    continue;
                }

                var ret = func_test(trigger, token);
                if (ret.Value == -1)
                    DataModel.SetMessage($"{ret.ErrorInfo}", "error");
                results.Add(ret);
            }
            var all_errors = results.Where(o => o.Value == -1).ToList();
            if (all_errors.Count > 0)
                DataModel.SetMessage($"以上测试共发现 {all_errors.Count} 处错误", "error");
        }

        static bool HasEnabled(XmlElement trigger)
        {
            while (trigger != null)
            {
                var value = trigger.GetAttribute("enable");
                if (DomUtil.IsBooleanTrue(value, true) == false)
                    return false;
                trigger = trigger.ParentNode as XmlElement;
            }
            return true;
        }

        public static string GetProperty(XmlElement trigger,
            string propName)
        {
            if (trigger.HasAttribute(propName))
                return trigger.GetAttribute(propName);
            while (trigger != null)
            {
                if (trigger.Name == "env"
                    && trigger.HasAttribute(propName))
                    return trigger.GetAttribute(propName);
                trigger = trigger.ParentNode as XmlElement;
            }

            return null;
        }

        // 获得书目记录中要构建的组成清单
        // 			<env biblio='file' subrecord='file' />
        public static string GetBiblioCreate(XmlElement trigger)
        {
            return GetProperty(trigger, "biblio");
        }

        public static string GetSubrecordCreate(XmlElement trigger)
        {
            return GetProperty(trigger, "subrecord");
        }

        public delegate NormalResult delegate_test(string name,
    string style,
    CancellationToken token);

        // 旧版本，用 string [] 驱动
        public static void RunMany(
            delegate_test func_test,
            string[] names,
            string[] styles,
            CancellationToken token)
        {
            var all_errors = names.SelectMany(name =>
            {
                token.ThrowIfCancellationRequested();
                var errors = styles.Select(style =>
                {
                    var ret = func_test(name, style, token);
                    if (ret.Value == -1)
                        DataModel.SetMessage($"{ret.ErrorInfo}", "error");
                    return ret;
                }).Where(o => o.Value == -1).ToList();
                return errors;
            }).ToList();
            if (all_errors.Count > 0)
                DataModel.SetMessage($"以上测试共发现 {all_errors.Count} 处错误", "error");
        }


        public static string GetError(EntityInfo[] errorinfos, out ErrorCodeValue error_code)
        {
            error_code = ErrorCodeValue.NoError;

            if (errorinfos != null)
            {
                List<ErrorCodeValue> codes = new List<ErrorCodeValue>();
                List<string> errors = new List<string>();
                foreach (var error in errorinfos)
                {
                    if (error.ErrorCode != ErrorCodeValue.NoError)
                    {
                        errors.Add(error.ErrorInfo);
                        codes.Add(error.ErrorCode);
                    }
                }

                if (codes.Count > 0)
                    error_code = codes[0];

                if (errors.Count > 0)
                    return StringUtil.MakePathList(errors, "; ");
            }

            return null;
        }


        public static void DisplayErrors(List<string> errors)
        {
            DataModel.SetMessage("**********************************", "error");
            foreach (string error in errors)
            {
                DataModel.SetMessage($"!!! {error} !!!");
            }
            DataModel.SetMessage("**********************************", "error");
        }

        public static int DeleteUsers(LibraryChannel channel,
    IEnumerable<string> user_names,
    out string strError)
        {
            strError = "";
            foreach (string user_name in user_names)
            {
                long lRet = channel.SetUser(null,
        "delete",
        new UserInfo
        {
            UserName = user_name,
        },
        out strError);
                if (lRet == -1 && channel.ErrorCode != ErrorCode.NotFound)
                    return -1;
            }
            return 0;
        }

        // 设置条码号校验规则
        public static int SetBarcodeValidation(
            string validation_innerxml,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = DataModel.GetChannel();

            try
            {
                long lRet = channel.SetSystemParameter(null,
    "circulation",
    "barcodeValidation",
    validation_innerxml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

        // 设置借阅权限表
        public static int SetRightsTable(
            string innerxml,
            out string strError)
        {
            strError = "";

            LibraryChannel channel = DataModel.GetChannel();

            try
            {
                long lRet = channel.SetSystemParameter(null,
    "circulation",
    "rightsTable",
    innerxml,
    out strError);
                if (lRet == -1)
                    return -1;

                return 0;
            }
            finally
            {
                DataModel.ReturnChannel(channel);
            }
        }

        // 准备测试用的账户
        public static NormalResult PrepareAccount(LibraryChannel channel,
            UserInfo user)
        {
            // DataModel.SetMessage($"正在删除用户 {user.UserName} ...");
            var nRet = Utility.DeleteUsers(channel,
                new string[] { user.UserName },
                out string strError);
            if (nRet == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            // 创建

            // DataModel.SetMessage($"正在创建用户 {user.UserName} rights={user.Rights} access={user.Access} ...");

            DataModel.SetMessage($"RIGHTS={user.Rights}\r\nACCESS={user.Access}");

            var lRet = channel.SetUser(null,
                "new",
                user,
                out strError);
            if (lRet == -1)
                return new NormalResult
                {
                    Value = -1,
                    ErrorInfo = strError
                };

            return new NormalResult();
        }


        // 从 XML 中移走指定的元素
        public static string RemoveElements(string xml1,
            string[] element_names)
        {
            if (string.IsNullOrEmpty(xml1))
                return xml1;
            if (element_names == null || element_names.Length == 0)
                return xml1;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml1);
            if (dom.DocumentElement == null)
                return xml1;
            foreach (var name in element_names)
            {
                var nodes = SelectNodes(dom, name);
                foreach (XmlElement node in nodes)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
            return dom.DocumentElement.OuterXml;
        }

        // 将形如 "123{456}789" 的内容切割为 "123" "{456}" "789" 若干部分
        public static string[] SplitByBraces(string text,
            bool throw_exception = false)
        {
            if (text == null)
                return new string[0];
            if (text == "")
                return new string[] { "" };
            var results = new List<string>();
            int level = 0;
            StringBuilder part = new StringBuilder();
            int i = 0;
            foreach (char ch in text)
            {
                if (level == 0
                    && ch == '{'
                    && part.Length > 0)
                {
                    // 如果以前有残存的内容，先推出
                    results.Add(part.ToString());
                    part.Clear();
                }
                if (ch == '{')
                    level++;
                if (ch == '}')
                    level--;
                if (throw_exception && level < 0)
                    throw new ArgumentException($"路径字符串 \"{PosMask(text, i)}\"(^ 表示出错位置) 中 }} 比 {{ 多");
                part.Append(ch);
                if (ch == '}')
                {
                    if (level == 0)
                    {
                        results.Add(part.ToString());
                        part.Clear();
                    }
                }

                i++;
            }

            if (throw_exception && level > 0)
                throw new ArgumentException($"路径字符串 \"{text}\" 中 {{ 比 }} 多");

            if (throw_exception && level < 0)
                throw new ArgumentException($"路径字符串 \"{text}\" 中 }} 比 {{ 多");

            if (part.Length > 0)
                results.Add(part.ToString());
            return results.ToArray();

            // 插入错误位置标记
            string PosMask(string s, int index)
            {
                return s.Insert(index, "^");
            }
        }

#if REMOVED
        // 将 111:222:333 切割为 111:222 和 333
        static string[] SplitRightmost(string s, char delimeter = ':')
        {
            int index = s.LastIndexOf(delimeter);
            if (index == -1)
                return new string[] { s };
            return new string[]
            {
                s.Substring(0, index),
                s.Substring(index + 1)
            };
        }
#endif

        // 将 path 中花括号局部 {http:xxx} 替换为 prefix1 返回(算法是识别花括号)。便于后面用 Select()
        public static string ReplaceNamespaceToPrefix(
            XmlNamespaceManager nsmgr,
            string path)
        {
            if (path.Contains('{') == false)
                return path;

            int prefix_seed = 1;
            string NewPrefix()
            {
                return $"prefix{prefix_seed++}";
            }

            StringBuilder text = new StringBuilder();
            var segments = SplitByBraces(path, true);
            foreach (var segment in segments)
            {
                if (segment.First() == '{' && segment.Last() == '}')
                {
                    var content = segment.Substring(1, segment.Length - 2);
                    var uri = content;
                    // 看看 名字空间管理器中是否已经有这个条目了
                    var prefix = nsmgr.LookupPrefix(uri);
                    if (string.IsNullOrEmpty(prefix))
                    {
                        prefix = NewPrefix();
                        // uri 进入名字空间管理器
                        nsmgr.AddNamespace(prefix, uri);
                    }

                    // prefix 输出用于后继 XPATH 选择之用
                    text.Append(prefix);
                }
                else
                    text.Append(segment.ToString());
            }
            return text.ToString();
        }

#if OLD
        // 将 path 中花括号局部 {http:xxx:name} 替换为 prefix1:name 返回(算法是识别最后一个冒号)。便于后面用 Select()
        public static string ReplaceNamespaceToPrefix(
            XmlNamespaceManager nsmgr,
            string path)
        {
            if (path.Contains(':') == false)
                return path;

            int prefix_seed = 1;
            string NewPrefix()
            {
                return $"prefix{prefix_seed++}";
            }

            StringBuilder text = new StringBuilder();
            var segments = SplitByBraces(path, true);
            foreach(var segment in segments)
            {
                if (segment.First() == '{' && segment.Last() == '}')
                {
                    var content = segment.Substring(1, segment.Length - 2);
                    var parts = SplitRightmost(content, ':');

                    var uri = parts[0];
                    var prefix = NewPrefix();
                    // uri 进入名字空间管理器
                    nsmgr.AddNamespace(prefix, uri);
                    // prefix 输出用于后继 XPATH 选择之用
                    text.Append(prefix + ':' + parts[1]);
                }
                else
                    text.Append(segment.ToString());
            }
            return text.ToString();
        }
#endif

        // parameters:
        //      name    为 "operations" 或者 "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
        //              其中 // 表示需要遍历所有位置的此名元素。如果没有 //，表示只在根元素的下级寻找
        public static List<XmlElement> SelectNodes(XmlDocument dom,
            string path)
        {
            var xpath = "";

            var results = new List<XmlElement>();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);
            xpath = ReplaceNamespaceToPrefix(nsmgr, path);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes(xpath, nsmgr);
            return nodes.Cast<XmlElement>().ToList();
            /*
            foreach (XmlElement element in nodes)
            {
                var current = GetNamespaceName(element);
                if (current == name)
                    results.Add(element);
            }

            return results;
            */
            string GetNamespaceName(XmlElement element)
            {
                if (string.IsNullOrEmpty(element.NamespaceURI))
                    return element.LocalName;
                return element.NamespaceURI + ":" + element.LocalName;
            }
        }

        public static string AssertHasElements(
            string xml1,
            string[] element_names)
        {
            if (string.IsNullOrEmpty(xml1))
                return "xml 不应为空";
            if (element_names == null || element_names.Length == 0)
                return null;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml1);
            if (dom.DocumentElement == null)
                return "xml 不应缺乏根元素";
            var errors = new List<string>();
            foreach (var name in element_names)
            {
                var nodes = SelectNodes(dom, name);
                if (nodes.Count == 0)
                    errors.Add($"缺乏必要的元素 {name}");
            }
            if (errors.Count > 0)
                return StringUtil.MakePathList(errors, "; ");
            return null;
        }

        public static string AssertMissingElements(
    string xml1,
    string[] element_names)
        {
            if (string.IsNullOrEmpty(xml1))
                return null;
            if (element_names == null || element_names.Length == 0)
                return null;
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml1);
            if (dom.DocumentElement == null)
                return null;
            var errors = new List<string>();
            foreach (var name in element_names)
            {
                var nodes = SelectNodes(dom, name);
                if (nodes.Count > 0)
                    errors.Add($"出现了不该出现的元素 {name}");
            }
            if (errors.Count > 0)
                return StringUtil.MakePathList(errors, "; ");
            return null;
        }

        public static void ChangeElementText(XmlDocument dom,
            string path,
            string text)
        {
            var nodes = SelectNodes(dom, path);
            if (nodes.Count == 0)
                throw new ArgumentException($"path 为 '{path}' 的元素没有找到，无法进行修改");
            nodes[0].InnerText = text;
        }

        public static string ChangeElementText(string xml,
            string path,
            string text)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(xml);
            ChangeElementText(dom, path, text);
            return dom.DocumentElement?.OuterXml;
        }

        public delegate void delegate_change(MarcRecord record);

        // 修改 MARCXML 中的 MARC 部分
        public static string ChangeMarc(string xml,
            delegate_change func_change)
        {
            var ret = MarcUtil.Xml2Marc(xml,
                true,
                "",
                out string syntax,
                out string marc,
                out string error);
            if (ret == -1)
                throw new Exception($"Xml2Marc error: {error}");

            var record = new MarcRecord(marc);
            func_change(record);

            ret = MarcUtil.Marc2XmlEx(
                record.Text,
                syntax,
                ref xml,
                out error);
            if (ret == -1)
                throw new Exception($"Marc2XmlEx error: {error}");
            return xml;
        }
    }
}
