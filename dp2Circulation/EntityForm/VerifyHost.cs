using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Script;
using DigitalPlatform.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dp2Circulation
{
    /// <summary>
    /// 校验数据的宿主类
    /// </summary>
    public class VerifyHost : IDisposable
    {
        public static MainForm MainForm
        {
            get
            {
                return Program.MainForm;
            }
        }

        /// <summary>
        /// 种册窗
        /// </summary>
        public EntityForm DetailForm = null;

        /// <summary>
        /// 结果字符串
        /// </summary>
        // public string ResultString = "";

        public VerifyResult VerifyResult { get; set; }

        // [in,out] 通用参数。依具体应用而定
        public string Parameter { get; set; }

        // [in,out] 通用字典表。用于往返传递各种信息
        public Hashtable Table { get; set; }

        /// <summary>
        /// 脚本编译后的 Assembly
        /// </summary>
        public Assembly Assembly = null;

        public void Dispose()
        {
            // 2017/4/23
            if (this.DetailForm != null)
                this.DetailForm = null;
        }

        public void ClearParameter()
        {
            this.VerifyResult = null;
            this.Parameter = null;
            this.Table = null;
        }

        /// <summary>
        /// 调用一个功能函数
        /// </summary>
        /// <param name="strFuncName">功能名称</param>
        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // 调用成员函数
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod
                ,
                null,
                this,
                null);
        }

#if REMOVED
        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void Main(object sender, HostEventArgs e)
        {
            // 要触发转换 MARC 功能，需要在 e.e.ScriptEntry 中放入 "#convertMarc"，
            // 并在 e.e.Parameter 中放入动作名称

            {
                var marc = this.DetailForm.GetMarc();
                var action = e.e.Parameter as string;
                var result = this.Verify(action, marc);
                if (result.Value == -1)
                {
                    this.ResultString = result.ErrorInfo;
                    return;
                }

                this.ResultString = result.Result;
                // this.DetailForm.SetMarc(result.Result);
            }
        }
#endif

        // 2024/5/9
        // 获得动作列表
        // parameters:
        //      operation   目前为 "verify" 或 "convert"
        public virtual List<string> GetRules(string marc)
        {
            return new List<string>();
        }

        // 2024/5/14
        // 进行校验
        public virtual VerifyResult Verify(string rule, string marc)
        {
            return new VerifyResult
            {
                Value = -1,
                ErrorInfo = "尚未实现"
            };
        }

        public static string ChangeString(string text,
            int start,
            string new_value)
        {
            if (text.Length < start)
                return text + (new string(' ', start - text.Length)) + new_value;
            var left = text.Substring(0, start);
            if (text.Length < start + new_value.Length)
                return left + new_value;
            var right = text.Substring(start + new_value.Length);
            return left + new_value + right;
        }

        #region 实用函数

        // 是否为"n版"的形态
        public static bool IsNumberPlusVersion(string text)
        {
            var ret = PriceUtil.ParsePriceUnit(text,
                out string prefix,
                out string value,
                out string postfix,
                out string error);
            if (ret == -1)
                return false;
            if (string.IsNullOrEmpty(prefix) == true
                && StringUtil.IsPureNumber(value)
                && postfix == "版")
                return true;
            return false;
        }

        public static string ReplaceNumberPlusVersion(string content)
        {
            int ret = PriceUtil.ParsePriceUnit(content,
out string prefix,
out string value,
out string postfix,
out string error);
            if (ret == -1)
                return content;
            if (prefix == "第"
                && StringUtil.IsPureNumber(value)
                && postfix == "版")
            {
                return value + postfix;
            }

            return content;
        }

        // 根据丛书名查找记录中的 4xx 字段
        // return:
        //      找到的 4xx 字段对象集合
        public static List<MarcField> Find4xx(
            MarcRecord record,
            string field_name,
            string title)
        {
            List<MarcField> results = new List<MarcField>();
            var subfields = record.select($"field[@name='{field_name}']/field[@name='200']/subfield[@name='a']");
            foreach (MarcSubfield subfield in subfields)
            {
                if (subfield.Content == title)
                    results.Add(subfield.Parent.Parent as MarcField);
            }

            return results;
        }

        // 把所有 $a 子字段内容连接起来成为一个字符串。中间间隔空格
        public static string JoinSubfield_a(List<MarcField> fields)
        {
            StringBuilder text = new StringBuilder();
            var nodes = new MarcNodeList();
            foreach (var field in fields)
            {
                nodes.add(field);
            }
            var subfields = nodes.select("*/subfield[@name='a']");
            foreach (MarcSubfield subfield in subfields)
            {
                if (string.IsNullOrEmpty(subfield.Content))
                    continue;
                if (text.Length > 0)
                    text.Append(" ");
                text.Append(subfield.Content);
            }
            return text.ToString();
        }

        public static List<string> FindResponseTypes(MarcRecord record)
        {
            List<string> results = new List<string>();
            // 获得 200$f$g 中的编著方式
            var subfields_fg = record.select("field[@name='200']/subfield[@name='f' or @name='g']");
            foreach (MarcSubfield subfield in subfields_fg)
            {
                if (subfield.Content.Contains("[等]"))
                {
                    int index = subfield.Content.IndexOf("[等]");
                    if (index != -1)
                    {
                        var text = subfield.Content.Substring(index + "[等]".Length);
                        if (string.IsNullOrWhiteSpace(text) == false)
                            results.Add(text);
                    }
                }
            }

            // 细节调整
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].EndsWith("著"))
                {
                    results[i] = results[i] + "者";
                    continue;
                }

                if (results[i].Length == 1)
                    results[i] = results[i] + "者";
            }

            return results;
        }

        public static List<string> GetContents(MarcNodeList list)
        {
            return list.List.Select(o => o.Content).ToList();
        }

        public static List<string> GetNames(MarcNodeList list)
        {
            return list.List.Select(o => o.Name).ToList();
        }

        // 删除一个字符串末尾的(半角)逗号
        public static string RemoveTailComma(string text)
        {
            if (string.IsNullOrEmpty(text)
                || text.EndsWith(",") == false)
                return text;
            return text.Substring(0, text.Length - 1);
        }

        // 探测当前是否已经存在 $A(或者 $9)。
        // 如果已经存在 $9，会自动改为 $A，然后再统计并返回 $A 是否存在的 bool
        // return:
        //      false   不存在
        //      true    已经存在
        public static bool HasPinyinA(MarcRecord record,
            string field_name)
        {
            // 把已经存在的 $9 变为 $A
            record.select($"field[@name='{field_name}']/subfield[@name='9']").Name = "A";
            return record.select($"field[@name='{field_name}']/subfield[@name='A']").count > 0;
        }

        // return
        //      -1: 出错;
        //      0: 用户希望中断;
        //      1: 正常;
        //      2: 结果字符串中有没有找到拼音的汉字
        public static int GetPinyin(string strHanzi,
            PinyinStyle style,
            out string strPinyin,
            out string strError)
        {
            return Program.MainForm.GetPinyin(
                Program.MainForm,
                strHanzi,
                style,  // PinyinStyle.None,
                true,
                out strPinyin,
                out strError);
        }

        public static string SafeSubstring(string text, int start, int length)
        {
            if (text == null)
                return null;
            try
            {
                return text.Substring(start, length);
            }
            catch
            {
                return null;
            }
        }



        // 从 MARC 记录中删除所有类似 $A $F 这样的拼音子字段
        public static void RemoveUpperLetterPinyinSubfields(MarcRecord record)
        {
            foreach (MarcSubfield subfield in record.select("//subfield"))
            {
                if (char.IsUpper(subfield.Name[0]))
                    subfield.detach();
            }
        }

        // 检查英文标点符号的正确性
        public static string CheckEnglishPointing(string text)
        {
            // 逗号后有一个空格“,#” ，
            // "/"前后都有空格“#/#” ，
            // 分号前后都有空格“#;#”
            for (int i = 0; i < text.Length; i++)
            {
                char left = (char)0;
                if (i > 0)
                    left = text[i - 1];
                char current = text[i];
                char right = (char)0;
                if (i < text.Length - 1)
                    right = text[i + 1];

                if (current == ',' && left == ' ')
                    return "逗号','左边不应该有空格";
                if (current == ',' && right != ' ' && right != (char)0)
                    return "逗号','右边应该有一个空格";

                if (current == '/' && left != ' ' && left != (char)0)
                    return "斜杠'/'左边应该有一个空格";
                if (current == '/' && right != ' ' && right != (char)0)
                    return "斜杠'/'右边应该有一个空格";

                if (current == ';' && left != ' ' && left != (char)0)
                    return "分号';'左边应该有一个空格";
                if (current == ';' && right != ' ' && right != (char)0)
                    return "分号';'右边应该有一个空格";

                if (current == '.' && left == ' ')
                    return "点'.'左边不应该有空格";
                if (current == '.' && right != ' ' && right != (char)0)
                    return "点'.'右边应该有一个空格";
            }

            return null;
        }

        public static int CountChar(string text, char ch)
        {
            return text.Where(o => o == ch).Count();
        }

        public static string VerifyUnimarc005(string content)
        {
            if (content == null || content.Length != 16)
                return $"内容应为 16 字符。但现在为 {content?.Length} 字符";
            if (DateTime.TryParseExact(content,
                "yyyyMMddHHmmss.f",
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out DateTime result) == false)
                return $"时间字符串 '{content}' 格式不正确";
            return null;
        }

        public static MarcNode FirstOrDefault(MarcNodeList list)
        {
            return list.List.FirstOrDefault();
        }

#if REMOVED
        string[] sample_cfg_lines = new string[] {
"高中生-->高中",
"初中生-->初中",
"小学生-->小学",
"高职高专-->高职",
"职业教育-->职业教育",
"中等专业/中等职业-->中职",
"技术学校-->技校",
"漫画、连环画(休闲)-->漫画 连环画",
"漫画、连环画(娱乐)-->漫画 连环画",
"成人教育/成人高考-->成人教育",
"高等教育自学考试-->高自考",
"老年人-->老年",
"教材-->教材",
"幼儿/儿童/少儿-->儿童",
"青少年-->青少年",
};

#endif

        #endregion
    }

    // .Value:
    //      -1  执行过程出错(也就是说校验没有完成)
    //      0   校验正确
    //      1   校验产生警告
    //      2   校验产生错误(也可能同时包含警告)
    // 注: 如果执行出错，.Value 为 -1。此时出错信息放到 .ErrorInfo 中。执行成功时，.Value 为非 -1 的值，此时 .ErrorInfo 中也可以有内容，表示过程信息
    // 如果执行成功，产生的结果内容放到 .Result 中。.Result 的数据格式放到 .Format 中
    //  .TargetNewMarc 在 .Value 返回 -1 的时候没有值。其它情况都可能有值
    public class VerifyResult : NormalResult
    {
        // [out] 被改变后的 MARC 机内格式记录
        public string ChangedMarc { get; set; }

        // [out] Result 的格式
        public string Format { get; set; }

        // [out] 返回校验结果集合
        public List<VerifyError> Errors { get; set; }

        public void AddError(string text)
        {
            if (Errors == null)
                Errors = new List<VerifyError>();
            VerifyError.AddError(Errors, text);
        }

        public void AddWarning(string text)
        {
            if (Errors == null)
                Errors = new List<VerifyError>();
            VerifyError.AddWarning(Errors, text);
        }

        public void AddInfo(string text)
        {
            if (Errors == null)
                Errors = new List<VerifyError>();
            VerifyError.AddInfo(Errors, text);
        }
    }


    public class VerifyBase
    {
        public MarcRecord Record { get; set; }

        List<VerifyError> _errors = new List<VerifyError>();

        public List<VerifyError> Errors
        {
            get
            {
                return new List<VerifyError>(_errors);
            }
        }

        public void AddError(string text)
        {
            VerifyError.AddError(_errors, text);
        }

        public void AddWarning(string text)
        {
            VerifyError.AddWarning(_errors, text);
        }

        public void AddInfo(string text)
        {
            VerifyError.AddInfo(_errors, text);
        }

        public void AddSucceed(string text)
        {
            VerifyError.AddSucceed(_errors, text);
        }

        public void AddTestingErrors()
        {
            AddInfo("这是 info 行");
            AddWarning("这是 warning 行");
            AddError("这是 error 行");
            AddSucceed("这是 succeed 行");
        }

        public void AutoSetIndicator(string field_name,
            string new_indicator)
        {
            foreach (MarcField field in this.Record.select("field[@name='461']"))
            {
                if (field.Indicator != new_indicator)
                {
                    AddInfo($"{field_name} 字段的指示符已经从 '{DisplayIndicator(field.Indicator)}' 自动修改为 '{DisplayIndicator(new_indicator)}'");
                    field.Indicator = new_indicator;
                }
            }
        }

        // 校验字段的必备性，指示符值，字段中子字段的必备性
        // 注: 单字符参数的含义如下:
        // r 必备
        // o 可选
        // n 可重复
        // 1 不可重复
        // parameters:
        //      condition   校验要求。
        //                  field:xxxx 字段要求
        //                  subfield:axxxx|bxxxx|cxxxx 子字段要求
        //                  indicator:xx|xx|xx 指示符值要求。注意空格用下划线替代
        public void VerifyField(
            string field_name,
            string condition)
        {
            var fields = this.Record.select($"field[@name='{field_name}']");

            var field_properties = StringUtil.GetParameterByPrefix(condition, "field");
            if (field_properties != null)
            {
                // r 必备
                // o 可选
                // n 可重复
                // 1 不可重复

                foreach (char value in field_properties)
                {
                    if (value == 'r')
                    {
                        if (fields.count == 0)
                            AddError($"缺乏必备的 {field_name} 字段");
                    }
                    if (value == '1')
                    {
                        if (fields.count > 1)
                            AddError($"{field_name} 字段多于 1 个");
                    }
                    if (value == 'n')
                    {

                    }
                }
            }


            var subfield_properties = StringUtil.GetParameterByPrefix(condition, "subfield");
            if (subfield_properties != null)
            {
                foreach (MarcField field in fields)
                {
                    // subfield:a|b|c 参数中的名字部分集合
                    var names = GetSubfieldNames(subfield_properties);

                    // 看看 field 中的各个子字段，名字是否超过 names 范围
                    foreach (MarcSubfield subfield in field.select("subfield"))
                    {
                        if (names.IndexOf(subfield.Name) == -1)
                            AddError($"{field.Name} 字段 ${subfield.Name} 超出定义范围");
                    }

                    foreach (var name in names)
                    {
                        var parameter = GetSufieldParameter(subfield_properties, name);
                        if (parameter != null)
                        {
                            var current_subfields = field.select($"subfield[@name='{name}']");

                            // 2024/8/1
                            // 如果当前子字段名为 A 或者 9，并且发现子字段不存在，则要观察对应的 a 子字段是否没有包含任何汉字。如果是，则当作此子字段“存在”
                            if (current_subfields.count == 0
                                && (char.IsUpper(name[0]) || name == "9"))
                            {
                                string origin_name = "a";
                                if (char.IsUpper(name[0]))
                                    origin_name = name.ToLower();

                                if (is_all_english(field, origin_name) == true)
                                {
                                    // 假装这个子字段存在(一个)
                                    current_subfields = new MarcNodeList(new MarcSubfield(name, ""));
                                }
                            }

                            foreach (char p in parameter)
                            {
                                // r 必备
                                // o 可选
                                // n 可重复
                                // 1 不可重复
                                if (p == 'r')
                                {
                                    if (current_subfields.count == 0)
                                        AddError($"{field_name} 字段内缺乏必备的 ${name} 子字段");
                                }
                                if (p == '1')
                                {
                                    if (current_subfields.count > 1)
                                        AddError($"{field_name}字段内 ${name} 子字段多于 1 个");
                                }
                                if (p == 'n')
                                {

                                }
                            }
                        }
                    }
                }
            }

            var indicator_values = StringUtil.GetParameterByPrefix(condition, "indicator");
            {
                string[] values = null;
                if (indicator_values != null)
                {
                    indicator_values = indicator_values.Replace("_", " ");
                    values = indicator_values.Split('|');
                }
                if (fields.count == 1 && values != null)
                {
                    MarcField field = fields[0] as MarcField;
                    if (Array.IndexOf(values, field.Indicator) == -1)
                        AddError($"{field_name} 字段指示符应为 {IndicatorsToString(values)}。但现在是 '{field.Indicator.Replace(" ", "#")}'");
                }
            }

            // 判断一个子字段名对应的所有子字段内容是否都是非汉字内容
            bool is_all_english(MarcField field, string origin_name)
            {
                var origin_contents = field.select($"subfield[@name='{origin_name}']").Contents;
                if (origin_contents.Count == 0)
                    return false;
                foreach (var content in origin_contents)
                {
                    if (StringUtil.ContainHanzi(content) == true)
                        return false;
                }

                return true;
            }
        }

        // 得到可用于显示的指示符集合字符串
        static string IndicatorsToString(string[] values)
        {
            StringBuilder text = new StringBuilder();
            foreach (var value in values)
            {
                if (text.Length > 0)
                    text.Append("、");
                text.Append($"'{value.Replace(" ", "#")}'");
            }

            return text.ToString();
        }

        // 获得可用于显示的两位指示符字符。即，把空格替换为 '#'
        public static string DisplayIndicator(string text)
        {
            if (text == null)
                return text;
            return text.Replace(" ", "#");
        }

        // 获得 a?|b?|c? 参数的名字部分。即 a,b,c
        static List<string> GetSubfieldNames(string subfield_properties)
        {
            List<string> results = new List<string>();
            string[] values = subfield_properties.Split('|');

            foreach (string s in values)
            {
                if (string.IsNullOrEmpty(s))
                    throw new ArgumentException($"subfield 子参数值 '{subfield_properties}' 不合法。'|' 之间至少要有一个字符");
                string name = s.Substring(0, 1);
                results.Add(name);
            }

            return results;
        }

        // 获得 a?|b?|c? 参数中，指定名字对应的值部分。例如名字 a 对应的就是 ?
        static string GetSufieldParameter(string subfield_properties,
            string subfield_name)
        {
            string[] values = subfield_properties.Split('|');

            foreach (string s in values)
            {
                if (string.IsNullOrEmpty(s))
                    throw new ArgumentException($"subfield 子参数值 '{subfield_properties}' 不合法。'|' 之间至少要有一个字符");
                string name = s.Substring(0, 1);

                if (name == subfield_name)
                {
                    var result = s.Substring(1);
                    // r 必备
                    // o 可选
                    // n 可重复
                    // 1 不可重复
                    if (result.Contains("o") && result.Contains("r"))
                        throw new ArgumentException($"子字段子参数中的 o 和 r 不允许同时出现。('{result}')");
                    if (result.Contains("1") && result.Contains("n"))
                        throw new ArgumentException($"子字段子参数中的 1 和 n 不允许同时出现。('{result}')");
                    return result;
                }
            }

            return null;
        }

        // 校验 423 字段是否和 200$a (第一个以后的)对应
        public void Verify200a423()
        {
            var subfields_a = this.Record.select("field[@name='200']/subfield[@name='a']");
            if (subfields_a.count > 1)
            {
                foreach (MarcSubfield subfield in subfields_a.List.Skip(1))
                {
                    var title = subfield.Content;
                    // 423#0$12001#$a...$1701#0$a...
                    var count = this.Record.select($"field[@name='423' and indicator=' 0']/field[@name='200' and @indicator='1 ']/subfield[@name='a' and @content='{title}']").count;
                    if (count == 0)
                        AddError($"200$a{title} 没有找到对应的 423 字段(423#0$12001#$a...)");
                    else if (count > 1)
                        AddError($"200$a{title} 找到对应的 423 字段(423#0$12001#$a...)数量多于一个 ({count})");
                }
            }
        }

        /*
有$d必须要有$z。
有$d必须有对应的500$a字段或510$a字段。
200$d$z生成510字段的规则为：
5101# $a(获取200字段$d的内容)$z(获取200字段$z的内容)。
* */
        // 校验 200$d$z 和 500/510 字段之间的对应关系
        public void Verify200d5xx()
        {
            var subfields_d = this.Record.select("field[@name='200']/subfield[@name='d']");
            var subfields_z = this.Record.select("field[@name='200']/subfield[@name='z']");
            if (subfields_d.count != subfields_z.count)
                AddError($"200$d 子字段的数量 {subfields_d.count} 和 $z 的数量 {subfields_z.count} 不一致");
            else
            {
                for (int i = 0; i < subfields_d.count; i++)
                {
                    var subfield_d = subfields_d[i] as MarcSubfield;
                    var subfield_z = subfields_z[i] as MarcSubfield;
                    var s_d = subfield_d.Content;
                    if (string.IsNullOrEmpty(s_d))
                    {
                        AddError($"200$d 内容不允许为空");
                        continue;
                    }
                    else if (s_d.StartsWith("="))
                    {
                        AddError($"200$d 内容中不允许以 '=' 开头 ('{s_d}')");
                        continue;
                    }
                    var s_z = subfield_z.Content;
                    if (string.IsNullOrEmpty(s_z))
                    {
                        AddError($"200$z 内容不允许为空");
                        continue;
                    }
                    if (Find510(s_d, s_z) == false
                        && Find500(s_d) == false)
                        AddError($"200$d{s_d}$z{s_z} 组合没有找到匹配的 510 或 500 字段");
                }
            }

            bool Find510(string d, string z)
            {
                var fields = this.Record.select($"field[@name='510']");
                foreach (MarcField field in fields)
                {
                    if (field.select("subfield[@name='a']").FirstContent == d
                        && field.select("subfield[@name='z']").FirstContent == z)
                        return true;
                }
                return false;
            }

            bool Find500(string d)
            {
                return this.Record.select($"field[@name='510']/subfield[@name='a' and @content='{d}']").count > 0;
            }
        }

        // 找到左边的兄弟。但找的过程要越过大写字母名字的子字段或$9
        public static MarcSubfield GetPrevSibling(MarcSubfield current)
        {
            MarcSubfield result = current.PrevSibling as MarcSubfield;
            while (result != null)
            {
                if (char.IsUpper(result.Name[0]) == true
                    || result.Name[0] == '9')
                    result = result.PrevSibling as MarcSubfield;
                else
                    return result;
            }
            return result;
        }

    }



    /// <summary>
    /// 用于数据校验的 FilterDocument 派生类(MARC 过滤器文档类)
    /// </summary>
    public class VerifyFilterDocument : FilterDocument
    {
        /// <summary>
        /// 宿主对象
        /// </summary>
        public VerifyHost FilterHost = null;

        // 2024/5/14
        public string Action { get; set; }
    }

    public static class MarcRecordUtility
    {
        // 根据前缀字符串获得匹配的子风格
        public static List<string> GetStyleByPrefix(string list,
            string prefix)
        {
            List<string> results = new List<string>();
            if (string.IsNullOrEmpty(list))
                return results;
            var segments = list.Split(',');
            foreach (var s in segments)
            {
                var text = s.Trim();
                if (text.StartsWith(prefix))
                    results.Add(text);
            }

            return results;
        }

        // 从字符串中删除所有匹配前缀的子风格
        public static string RemoveStyleByPrefix(string list,
    string prefix)
        {
            if (string.IsNullOrEmpty(list))
                return list;
            List<string> results = new List<string>();
            var segments = list.Split(',');
            foreach (var s in segments)
            {
                var text = s.Trim();
                if (text.StartsWith(prefix) == false)
                    results.Add(text);
            }

            return StringUtil.MakePathList(results, ",");
        }

        // 在一个字符串中设置阶段 style。所谓阶段 style 就是一种以 _ 开头的 style
        public static string SetStageStyle(string list,
            string stage)
        {
            if (stage.StartsWith("_") == false)
                throw new ArgumentException($"stage 参数值应当以字符 '_' 开头");

            var value = RemoveStyleByPrefix(list, "_");
            StringUtil.SetInList(ref value, stage, true);
            return value;
        }

        // 根据定位字符串，获得 MarcRecord 中的具体 MarcNode 对象
        // parameters:
        //      locationString  定位路径。例如 "200/a"，或者 "300:1/a:5"
        public static MarcNode LocateMarcNode(MarcRecord record,
            string locationString)
        {
            var current = record as MarcNode;
            var levels = locationString.Split('/');
            int i = 0;
            foreach (var level in levels)
            {
                var parts = StringUtil.ParseTwoPart(level, ":");
                var name = parts[0];
                var count_string = parts[1];

                // 第一级出现 ###，当作选中 record 对象
                if (name == "###" || name == "hdr")
                {
                    if (i == 0)
                        return current;
                    else
                        throw new Exception($"除了第一级以外的其它级不允许出现 '{name}'");
                }

                var hits = current.select($"*[@name='{name}']");
                if (hits.count == 0)
                    throw new Exception($"级别 '{level}' 没有命中任何节点");
                if (string.IsNullOrEmpty(count_string) == false)
                {
                    if (Int32.TryParse(count_string, out int count) == false)
                        throw new ArgumentException($"级别 '{level}' 中重复次数部分格式不合法");
                    if (hits.count <= count)
                        throw new Exception($"级别 '{level}' 命中的同名节点数量少于 {count}");
                    current = hits[count];
                }
                else
                {
                    if (hits.count == 1)
                        current = hits[0];
                    else
                    {
                        // 暂时不要求多于一个同名节点时级别中具有数字下标
                        current = hits[0];
                        // throw new Exception($"级别 '{level}' 命中了多于一个同名节点 ({hits.count}) 但并没有提供区分数部分");
                    }
                }

                i++;
            }

            return current;
        }

        // return:
        //      null    无法获得定位字符串
        //      ""      如果用于定位，可能会定位到记录上，或者定位到头标区
        //      其它  正常的值
        public static string GetLocationString(
            MarcNode node)
        {
            if (node == null)
                return null;
            List<MarcNode> levels = new List<MarcNode>();
            // 根节点
            MarcNode root = node;
            while (root.Parent != null)
            {
                levels.Insert(0, root);
                root = root.Parent;
            }

            List<string> paths = new List<string>();
            foreach (MarcNode level in levels)
            {
                var count = GetDupCount(level);
                if (count == 0)
                    paths.Add(level.Name);
                else
                    paths.Add(level.Name + ":" + count.ToString());
            }

            return StringUtil.MakePathList(paths, "/");
        }

        // 获得节点的同名偏移数字
        static int GetDupCount(MarcNode node)
        {
            int count = 0;
            var name = node.Name;
            node = node.PrevSibling;
            while (node != null)
            {
                if (node.Name == name)
                    count++;
                node = node.PrevSibling;
            }

            return count;
        }

        public static bool Add500(MarcRecord record,
string rule,
ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_e = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_e = LocateMarcNode(record,
                locationString) as MarcSubfield;

            bool subfield_name_match = false;
            if (subfield_e != null && subfield_e.Name == "d")
                subfield_name_match = true;

            // 判断 subfield 是否为 200$d
            if (subfield_e != null
                && subfield_e.Parent?.Name == "200"
                && subfield_name_match)
            {

            }
            else
                subfield_e = null;

            if (subfield_e == null)
                return false;

            MarcField new_field = new MarcField("500", "10", $"‡a{subfield_e.Content.TrimStart('=',' ')}‡mChinese".Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);
            {
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            return old_marc != record.Text;
        }


        /*
## 200$d-->510

当200字段出现"$d= xxx"，在200字段$d内容上按Ctrl+a自动生成510字段。
510字段的指示符为10，格式恒定为“$axxx$mChinese” xxx是200$d中=号后的内容。

         * */
        public static bool Add510(MarcRecord record,
    string rule,
    ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_e = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_e = LocateMarcNode(record,
                locationString) as MarcSubfield;

            bool subfield_name_match = false;
            if (subfield_e != null && subfield_e.Name == "d")
                subfield_name_match = true;

            // 判断 subfield 是否为 200$d
            if (subfield_e != null
                && subfield_e.Parent?.Name == "200"
                && subfield_name_match)
            {

            }
            else
                subfield_e = null;

            if (subfield_e == null)
                return false;

            MarcField new_field = new MarcField("510", "1 ", $"‡a{subfield_e.Content.TrimStart('=', ' ')}‡z".Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);
            {
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            return old_marc != record.Text;
        }


        // 根据 200$e 或 200$i 生成 517$a
        // 区分 CALIS 和 NLC 编目规则。其中 NLC 没有 $i
        public static bool Add517(MarcRecord record,
            string rule,
            ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_e = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_e = LocateMarcNode(record,
                locationString) as MarcSubfield;

            bool subfield_name_match = false;
            if (subfield_e != null)
            {
                if (rule == "CALIS" && (subfield_e.Name == "e" || subfield_e.Name == "i"))
                    subfield_name_match = true;
                else if (subfield_e.Name == "e")
                    subfield_name_match = true;
            }

            // 判断 subfield 是否为 200$e
            if (subfield_e != null
                && subfield_e.Parent?.Name == "200"
                && subfield_name_match)
            {

            }
            else
                subfield_e = null;

            if (subfield_e == null)
                return false;

            MarcField new_field = new MarcField("517", "1 ", MarcQuery.SUBFLD + "a" + subfield_e.Content);
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);
            {
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            return old_marc != record.Text;
        }

        // 若200出现$c或者第2个$a时，光标在$c或第2个$a后，按Ctrl+a自动生成423字段,
        // 423#0$12001#$a...$1701#0$a...的内容
        public static bool Add423From200(MarcRecord record,
            string rule,
            ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_c = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_c = LocateMarcNode(record,
                locationString) as MarcSubfield;

            // 判断subfield_e 是否为 200$e
            if (subfield_c != null
                && (subfield_c.Name == "a" || subfield_c.Name == "c")
                && subfield_c.Parent?.Name == "200")
            {

            }
            else
                subfield_c = null;

            if (subfield_c == null)
                return false;

            // 寻找其后的，下一个 $c 或者 $c 之前的第一个遇到的 $f 或者 $g
            var subfield_g = GetNextAuthor(subfield_c);

            string content = "";
            if (subfield_g != null)
                content = $"‡12001 ‡a{subfield_c.Content}‡1701 0‡a{subfield_g.Content}";
            else
                content = $"‡12001 ‡a{subfield_c.Content}";

            MarcField new_field = new MarcField("423", " 0", content.Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);

            // 返回 423$a 的定位
            if (new_field != null)
            {
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            else
                locationString = "";

            return old_marc != record.Text;
        }

        public static bool Add423From311(MarcRecord record,
    string rule,
    ref string locationString)
        {
            string old_marc = record.Text;

            MarcNode node = null;
            if (string.IsNullOrEmpty(locationString) == false)
                node = LocateMarcNode(record, locationString);

            if (node == null)
                return false;

            MarcField field = null;

            // 判断subfield 是否为 311 下面的任意子字段
            if (node.NodeType == DigitalPlatform.Marc.NodeType.Subfield
                && node.Parent?.Name == "311")
            {
                field = node.Parent as MarcField;
            }
            else if (node.NodeType == DigitalPlatform.Marc.NodeType.Field
                && node.Name == "311")
            {
                field = node as MarcField;
            }
            else
                return false;

            // 311   $a本书与: 中华古今注 / (五代) 马缟撰. 封氏闻见记 / (唐) 封演撰. 资暇集 / (唐) 李匡乂撰. 刊误 / (唐) 李培撰. 苏氏演义 / (唐) 苏鹗撰. 兼明书 / (五代) 丘光庭撰 合订一册。
            // 423 0$12001$a中华古今注$1701 0$a马缟,$f五代
            MarcField new_field = null;
            foreach (MarcSubfield subfield in field.select("subfield[@name='a']"))
            {
                var results = Split311a(subfield.Content);
                foreach (var result in results)
                {
                    // 将 result.Author 再次切割为 著者姓名和朝代
                    SplitNameAndDynasty(result.Author,
    out string name,
    out string dynasty);

                    var content = $"‡12001‡a{result.Title}‡1701 0‡a{name},‡f{dynasty}";
                    if (string.IsNullOrEmpty(result.Author))
                        content = $"‡12001‡a{result.Title}";

                    new_field = new MarcField("423", " 0", content.Replace("‡", MarcQuery.SUBFLD));
                    record.ChildNodes.insertSequence(
    new_field,
    InsertSequenceStyle.PreferTail);

                }
            }

            // 返回 423$a 的定位
            if (new_field != null)
            {
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            else
                locationString = "";
            return old_marc != record.Text;
        }

        class TitleAndAuthor
        {
            public string Title { get; set; }
            public string Author { get; set; }
        }

        // 切割 311$a 内容
        // 311   $a本书与: 中华古今注 / (五代) 马缟撰. 封氏闻见记 / (唐) 封演撰. 资暇集 / (唐) 李匡乂撰. 刊误 / (唐) 李培撰. 苏氏演义 / (唐) 苏鹗撰. 兼明书 / (五代) 丘光庭撰 合订一册。
        static List<TitleAndAuthor> Split311a(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<TitleAndAuthor>();
            // 去掉头尾
            if (text.StartsWith("本书与"))
                text = text.Substring(3).TrimStart(':', '：');
            text = text.TrimEnd('.', '。');
            if (text.EndsWith("合订一册"))
                text = text.Substring(0, text.Length - 4).TrimEnd(' ');

            var results = new List<TitleAndAuthor>();
            var segments = text.Split('.', ';', '；');
            foreach (var segment in segments)
            {
                var parts = StringUtil.ParseTwoPart(segment, "/");
                var left = parts[0].Trim();
                var right = parts[1].Trim();
                results.Add(new TitleAndAuthor
                {
                    Title = left,
                    Author = right
                });
            }

            return results;
        }

        static void SplitNameAndDynasty(string text,
            out string name,
            out string dynasty)
        {
            name = "";
            dynasty = "";
            if (string.IsNullOrEmpty(text))
                return;
            int index = text.IndexOf(")");
            if (index == -1)
            {
                name = text;
                return;
            }
            dynasty = text.Substring(0, index).TrimStart('(').Trim();
            name = text.Substring(index + 1).Trim();

            if (name.EndsWith("编撰")
    || name.EndsWith("编著")
    || name.EndsWith("编写"))
                name = name.Substring(0, name.Length - 2);
            // 撰
            else if (name.EndsWith("撰")
                || name.EndsWith("编")
                || name.EndsWith("著"))
                name = name.Substring(0, name.Length - 1);
            return;
        }

        // parameters:
        //      locationString  [in] 调用前插入符所在的子字段定位
        //                      [out] 调用后插入符应当去到的子字段定位。如果为空，表示无需改变当前插入符位置
        public static bool Add304(MarcRecord record,
    string rule,
    ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_f = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_f = LocateMarcNode(record,
                locationString) as MarcSubfield;

            // 判断subfield_f 是否为 200$f$g
            if (subfield_f != null
                && subfield_f.Parent?.Name == "200"
                && (subfield_f.Name == "f" || subfield_f.Name == "g")
                && subfield_f.Content.Contains("等"))
            {

            }
            else
                subfield_f = null;

            if (subfield_f == null)
                return false;

            string content = "‡a???还有:";
            if (rule == "CALIS")
                content = "‡a题名页题其余责任者：";

            var new_field = new MarcField("304", "  ", content.Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);

            // 返回 304$a 的定位
            var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
            locationString = MarcRecordUtility.GetLocationString(
                subfield_a);
            return old_marc != record.Text;
        }

        // 通过215$e 生成 307 字段，307$a内容为`???附书：书1册 (38页 ; 26cm)`。
        // NLC 和 CALIS 规则相同。
        // parameters:
        //      locationString  [in] 调用前插入符所在的子字段定位
        //                      [out] 调用后插入符应当去到的子字段定位。如果为空，表示无需改变当前插入符位置
        public static bool Add307(MarcRecord record,
    string rule,
    ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_e = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_e = LocateMarcNode(record,
                locationString) as MarcSubfield;

            // 判断subfield_e 是否为 215$e
            if (subfield_e != null
                && subfield_e.Parent?.Name == "215"
                && subfield_e.Name == "e")
            {

            }
            else
                subfield_e = null;

            if (subfield_e == null)
                return false;

            string content = $"‡a???附书：{subfield_e.Content}";

            var new_field_name = "307";
            var new_field = new MarcField(new_field_name, "  ", content.Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);

            // 返回 307$a 的定位
            var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
            locationString = MarcRecordUtility.GetLocationString(
                subfield_a);
            return old_marc != record.Text;
        }

        // parameters:
        //      locationString  [in] 调用前插入符所在的子字段定位
        //                      [out] 调用后插入符应当去到的子字段定位。如果为空，表示无需改变当前插入符位置
        public static bool Add410(MarcRecord record,
    string rule,
    ref string locationString)
        {
            string old_marc = record.Text;

            MarcNode node = null;
            if (string.IsNullOrEmpty(locationString) == false)
                node = LocateMarcNode(record, locationString);

            if (node == null)
                return false;

            MarcField field = null;

            // 判断subfield 是否为 225 下面的任意子字段
            if (node.NodeType == DigitalPlatform.Marc.NodeType.Subfield
                && node.Parent?.Name == "225")
            {
                field = node.Parent as MarcField;
            }
            else if (node.NodeType == DigitalPlatform.Marc.NodeType.Field
                && node.Name == "225")
            {
                field = node as MarcField;
            }
            else
                return false;

            if (rule == "CALIS")
                return CalisAdd410(field,
rule,
ref locationString);
            else
                return NlcAdd46x(field,
rule,
ref locationString);
        }

        static bool CalisAdd410(MarcField field,
string rule,
ref string locationString)
        {
            MarcRecord record = field.Parent as MarcRecord;

            string old_marc = record.Text;

            // 1）若225指示符为"0#"或者"2#"时，才生成410，其它指示符不生成。
            if (field.Indicator == "0 " || field.Indicator == "2 ")
            {

            }
            else
                return false;

            var ref_field = field.clone();
            ref_field.select("subfield[@name='A' or @name='9' or @name='f']").detach();

            var content = $"‡12001 {ref_field.Content}";

            // 2）410指定符为固定值“#0" ，将225字段所有子字段内容（除拼音$A和$f外）都到410。
            var new_field_name = "410";
            var new_field = new MarcField(new_field_name, " 0", content.Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);

            // 返回 410 的定位
            locationString = MarcRecordUtility.GetLocationString(
                new_field);
            return old_marc != record.Text;
        }

        static bool NlcAdd46x(MarcField field,
string rule,
ref string locationString)
        {
            MarcRecord record = field.Parent as MarcRecord;

            string old_marc = record.Text;

            // 1）225字段指示符是"0#"或者"2#"时，才生成46X字段，其它的指示符都不生成。
            if (field.Indicator == "0 " || field.Indicator == "2 ")
            {

            }
            else
                return false;

            var ref_field = field.clone();

            // 2）无$i，生成461，461的内容只要225$a的内容，不要其它。
            // 3）有$i，生成462，462的内容为225全部内容，不要拼音子字段和$f和$e。（例子todo)
            // 4）461与461指示符，固定为"#0"
            var new_field_name = "461";
            if (field.select("subfield[@name='i']").count > 0)
            {
                new_field_name = "462";
                ref_field.select("subfield[@name='A' or @name='9' or @name='f' or @name='e']").detach();
            }
            else
                ref_field.select("subfield[@name!='a']").detach();

            var content = $"‡12001 {ref_field.Content}";

            var new_field = new MarcField(new_field_name, " 0", content.Replace("‡", MarcQuery.SUBFLD));
            record.ChildNodes.insertSequence(
                new_field,
                InsertSequenceStyle.PreferTail);

            // 返回 46x 的定位
            locationString = MarcRecordUtility.GetLocationString(
                new_field);
            return old_marc != record.Text;
        }


        // parameters:
        //      locationString  [in] 调用前插入符所在的子字段定位
        //                      [out] 调用后插入符应当去到的子字段定位。如果为空，表示无需改变当前插入符位置
        public static bool Add7xx(MarcRecord record,
    string rule,
    ref string locationString)
        {
            string old_marc = record.Text;

            MarcField field_7xx = null;

            MarcSubfield subfield_f = null;
            if (string.IsNullOrEmpty(locationString) == false)
            {
                var node = LocateMarcNode(record,
                locationString);
                if (node is MarcSubfield)
                    subfield_f = node as MarcSubfield;
                else if (node is MarcField)
                    field_7xx = node as MarcField;
            }

            // 判断subfield_f 是否为 200$f$g
            if (subfield_f != null
                && subfield_f.Parent?.Name == "200"
                && (subfield_f.Name == "f" || subfield_f.Name == "g"))
            {

            }
            // 2024/8/12
            // 是否为 701$a 或者 702$a
            else if (subfield_f != null
                && (subfield_f.Parent?.Name == "701" || subfield_f.Parent?.Name == "702")
                /*&& subfield_f.Name == "a"*/)
            {
                field_7xx = subfield_f.Parent as MarcField;
            }
            else if (field_7xx != null
                && (field_7xx.Name == "701" || field_7xx.Name == "702"))
            {

            }
            else
                subfield_f = null;

            if (field_7xx != null)
            {
                // 转为找到第一个 200$f
                if (field_7xx?.Name == "701")
                {
                    subfield_f = VerifyHost.FirstOrDefault(record.select("field[@name='200']/subfield[@name='f']")) as MarcSubfield;
                }
                // 转为找到第一个 200$g
                else if (field_7xx?.Name == "702")
                {
                    subfield_f = VerifyHost.FirstOrDefault(record.select("field[@name='200']/subfield[@name='g']")) as MarcSubfield;
                }
                else
                    subfield_f = null;
            }

            if (subfield_f == null)
                return false;

            // TODO: 按照 subfield.Content 内容切割为多个著者名字符串

            if (field_7xx == null)
            {
                string content = $"‡a{subfield_f.Content}";

                var new_field_name = "701";
                if (subfield_f.Name == "g")
                    new_field_name = "702";
                var new_field = new MarcField(new_field_name, " 0", content.Replace("‡", MarcQuery.SUBFLD));
                record.ChildNodes.insertSequence(
                    new_field,
                    InsertSequenceStyle.PreferTail);

                // 返回 7xx$a 的定位
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            else
            {
                var subfield_a = VerifyHost.FirstOrDefault(field_7xx.select("subfield[@name='a']")) as MarcSubfield;
                if (subfield_a == null)
                {
                    subfield_a = new MarcSubfield("a", subfield_f.Content);
                    field_7xx.ChildNodes.insertSequence(subfield_a);
                }
                else
                    subfield_a.Content = subfield_f.Content;
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }
            return old_marc != record.Text;
        }

        // 根据插入符位置把 7xx$a 切割为两部分，生成两个 7xx 字段，并删除原字段
        // parameters:
        //      locationString  [in] 调用前插入符所在的子字段定位
        //                      [out] 调用后插入符应当去到的子字段定位。如果为空，表示无需改变当前插入符位置
        public static bool Split7xx(MarcRecord record,
    string rule,
    ref string locationString,
    int caret_offs_in_end_level)
        {
            string old_marc = record.Text;

            MarcField field_7xx = null;

            MarcSubfield subfield_f = null;
            if (string.IsNullOrEmpty(locationString) == false)
            {
                var node = LocateMarcNode(record,
                locationString);
                if (node is MarcSubfield)
                    subfield_f = node as MarcSubfield;
            }

            // 是否为 701$a 或者 702$a
            if (subfield_f != null
                && (subfield_f.Parent?.Name == "701" || subfield_f.Parent?.Name == "702")
                && subfield_f.Name == "a")
            {
                field_7xx = subfield_f.Parent as MarcField;
            }
            else
                subfield_f = null;

            if (subfield_f == null)
                return false;

            if (caret_offs_in_end_level < 0
                || caret_offs_in_end_level >= subfield_f.Text.Length)
            {
                // 偏移量不对
                return false;
            }

            var offs_in_content = caret_offs_in_end_level - 2;
            if (offs_in_content < 0)
                return false;   // 插入符不在子字段内容上
            var left = subfield_f.Content.Substring(0, offs_in_content);
            var right = subfield_f.Content.Substring(offs_in_content);
            if (string.IsNullOrEmpty(left)
                || string.IsNullOrEmpty(right))
                return false;   // left right 中有一个为空

            // 修改 $a
            field_7xx.select("subfield[@name='a']")[0].Content = left;

            {
                var new_field = field_7xx.clone();
                var index = field_7xx.Parent.ChildNodes.indexOf(field_7xx);
                field_7xx.Parent.ChildNodes.insert(index + 1, new_field);
                /*
                record.ChildNodes.insertSequence(
                    new_field,
                    InsertSequenceStyle.PreferTail);
                */

                // 修改 $a
                new_field.select("subfield[@name='a']")[0].Content = right;

                // 返回 7xx$a 的定位
                var subfield_a = VerifyHost.FirstOrDefault(new_field.select("subfield[@name='a']"));
                locationString = MarcRecordUtility.GetLocationString(
                    subfield_a);
            }

            return old_marc != record.Text;
        }

        public static bool Copy333ato960e(MarcRecord record,
string rule,
ref string locationString)
        {
            string old_marc = record.Text;

            MarcSubfield subfield_e = null;
            if (string.IsNullOrEmpty(locationString) == false)
                subfield_e = LocateMarcNode(record,
                locationString) as MarcSubfield;

            MarcSubfield source = null;
            MarcSubfield target = null;
            if (subfield_e != null
                && subfield_e.Parent?.Name == "333"
                && subfield_e.Name == "a")
            {
                source = subfield_e;
            }
            else if (subfield_e != null
                && subfield_e.Parent?.Name == "960"
                && subfield_e.Name == "e")
            {
                target = subfield_e;
            }
            else
                return false;

            string source_content = "";
            if (source != null)
                source_content = source.Content;
            else
            {
                source_content = record.select("field[@name='333']/subfield[@name='a']").FirstContent;
            }

            if (string.IsNullOrEmpty(source_content))
                return false;

            if (target != null)
                target.Content = source_content;
            else
                record.setFirstSubfield("960", "e", source_content);

            locationString = null;
            return old_marc != record.Text;
        }

        // parameters:
        //      style   force13 force10 auto 之一
        public static bool HyphenISBN(MarcRecord record,
string rule,
string style,   // bool force13,
ref string locationString)
        {
            if (style != "force13"
                && style != "force10"
                && style != "auto")
                throw new ArgumentException($"style 参数值 '{style}' 不合法。应为 force13 force10 auto 之一");

            if (style == "auto")
                throw new ArgumentException("暂未实现");

            string old_marc = record.Text;

            MarcNode node = null;
            if (string.IsNullOrEmpty(locationString) == false)
            {
                node = LocateMarcNode(record,
                locationString);
            }

            if (node == null)
                node = VerifyHost.FirstOrDefault(record.select("field[@name='010']/subfield[@name='a']"));

            if (node == null)
                return false;

            MarcSubfield subfield = null;
            if (node is MarcSubfield)
                subfield = node as MarcSubfield;
            else if (node is MarcField)
                subfield = VerifyHost.FirstOrDefault(node.select("subfield[@name='a']")) as MarcSubfield;
            else
                subfield = VerifyHost.FirstOrDefault(record.select("field[@name='010']/subfield[@name='a']")) as MarcSubfield;

            if (subfield == null) 
                return false;

            if (subfield.Parent?.Name != "010"
                || subfield.Name != "a")
                return false;

            string strISBN = subfield.Content.Trim();
            if (string.IsNullOrEmpty(strISBN))
                return false;

            int nRet = Program.MainForm.LoadIsbnSplitter(
                true, 
                out string strError);
            if (nRet == -1)
                goto ERROR1;

            nRet = Program.MainForm.IsbnSplitter.IsbnInsertHyphen(
                strISBN,
                style + ",strict",
                out string strResult,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
            {
                DialogResult result = MessageBox.Show(
                    Program.MainForm,
                    "原ISBN '" + strISBN + "'加工成 '" + strResult + "' 后发现校验位有变化。\r\n\r\n是否接受修改?",
                    "规整ISBN",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return false;
            }

            subfield.Content = strResult;
            locationString = null;
            return old_marc != record.Text;
        ERROR1:
            MessageBox.Show(Program.MainForm, strError);
            return false;
        }


        static MarcSubfield GetNextAuthor(MarcSubfield current)
        {
            MarcSubfield result = current.NextSibling as MarcSubfield;
            while (result != null)
            {
                // 遇到第一个 $f $g 就返回
                if (result.Name == "f" || result.Name == "g")
                    return result;

                // 遇到 $a $c 就停止
                if (result.Name == "a" || result.Name == "c")
                    return null;

                result = result.NextSibling as MarcSubfield;
            }
            return result;
        }


    }

    [TestClass]
    public class UnitTestVerifyHost
    {
        [TestMethod]
        public void test_locateMarcNode_01()
        {
            string worksheet = @"123456789012345678901234
001_____
100  $a1111
200  $a2222".Replace('$', 'ǂ');
            string locationString = "100/a";
            MarcRecord record = MarcRecord.FromWorksheet(worksheet);
            var ret = MarcRecordUtility.LocateMarcNode(record, locationString);
            Assert.AreEqual("a", ret.Name);
            Assert.AreEqual("1111", ret.Content);
            Assert.AreEqual("100", ret.Parent.Name);
        }

        [TestMethod]
        public void test_locateMarcNode_02()
        {
            string worksheet = @"123456789012345678901234
001_____
100  $a1111
100  $a2222$a3333
200  $a4444".Replace('$', 'ǂ');
            string locationString = "100:1/a:1";
            MarcRecord record = MarcRecord.FromWorksheet(worksheet);
            var ret = MarcRecordUtility.LocateMarcNode(record, locationString);
            Assert.AreEqual("a", ret.Name);
            Assert.AreEqual("3333", ret.Content);
            Assert.AreEqual("100", ret.Parent.Name);
        }

        [TestMethod]
        public void test_getLocationString_01()
        {
            string worksheet = @"123456789012345678901234
001_____
100  $a1111
100  $a2222$a3333
200  $a4444".Replace('$', 'ǂ');
            string correct = "100/a";
            MarcRecord record = MarcRecord.FromWorksheet(worksheet);
            var node = record.select("field[@name='100']/subfield[@name='a']").List.FirstOrDefault();
            var locationString = MarcRecordUtility.GetLocationString(node);
            Assert.AreEqual(correct, locationString);
        }

        [TestMethod]
        public void test_getLocationString_02()
        {
            string worksheet = @"123456789012345678901234
001_____
100  $a1111
100  $a2222$a3333
200  $a4444".Replace('$', 'ǂ');
            string correct = "100:1/a:1";
            MarcRecord record = MarcRecord.FromWorksheet(worksheet);
            var node = record.select("field[@name='100'][2]/subfield[@name='a'][2]").List.FirstOrDefault();
            var locationString = MarcRecordUtility.GetLocationString(node);
            Assert.AreEqual(correct, locationString);
        }

        [TestMethod]
        public void test_getLocationString_03()
        {
            string worksheet = @"123456789012345678901234
001_____
100  $a1111
100  $a2222$a3333
200  $a4444".Replace('$', 'ǂ');
            string correct = "001";
            MarcRecord record = MarcRecord.FromWorksheet(worksheet);
            var node = record.select("field[@name='001']").List.FirstOrDefault();
            var locationString = MarcRecordUtility.GetLocationString(node);
            Assert.AreEqual(correct, locationString);
        }

        [TestMethod]
        public void test_getLocationString_04()
        {
            string worksheet = @"123456789012345678901234
001_____
100  $a1111
100  $a2222$a3333
200  $a4444".Replace('$', 'ǂ');
            string correct = "";
            MarcRecord record = MarcRecord.FromWorksheet(worksheet);
            var node = record as MarcNode;
            var locationString = MarcRecordUtility.GetLocationString(node);
            Assert.AreEqual(correct, locationString);
        }
    }
}
