using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace dp2SSL.SIP2
{
    // 一个字段的合法性规则
    public class FieldRule
    {
        public string ID { get; set; }  // ID 为 "##" 表示这是一个固定长度、无名字段
        public string[] Alias { get; set; }

        bool _required = false;
        // 必备/可选
        public bool IsRequired
        {
            get { return _required; }
            set { _required = value; }
        }

        bool _repeatable = true;
        // 是否可以重复
        public bool IsRepeatable
        {
            get { return _repeatable; }
            set { _repeatable = value; }
        }

        int _fixFieldLength = -1;
        public int FixFieldLength
        {
            get { return _fixFieldLength; }
        }

        // 是否为定长字段
        public bool IsFixLength
        {
            get
            {
                return _fixFieldLength > 0;
            }
        }

        public FieldRule(string name)
        {
            ParseNameString(name,
            out string two_chars,
            out string[] alias);

            this.ID = two_chars;
            this.Alias = alias;

            if (this.ID == "##")
                SetFixLengthType();
        }

        // r 必备
        // o 可选
        // n 可重复
        // 1 不可重复
        // parameters:
        //      name    字段名字。这是一个 _ 或者 | 间隔的多个名字的字符串。第一个名字只能是 2 字符，从第二个开始是别名
        //      def     定义。例如 "fix:10,var:r1"
        public FieldRule(string name,
            string def)
        {
            ParseNameString(name,
            out string two_chars,
            out string[] alias);

            this.ID = two_chars;
            this.Alias = alias;

            var fix_def = StringUtil.GetParameterByPrefix(def, "fix");
            var var_def = StringUtil.GetParameterByPrefix(def, "var");

            if (string.IsNullOrEmpty(fix_def) == false)
            {
                if (Int32.TryParse(fix_def, out int value) == false)
                    throw new Exception($"'fix:{fix_def}' 定义格式不合法。应为一个整数数字");
                this._fixFieldLength = Convert.ToInt32(fix_def);
            }

            if (string.IsNullOrEmpty(var_def) == false)
            {
                foreach (var ch in var_def)
                {
                    // TODO: 检查互相矛盾的定义，比如 r,o n,1
                    // TODO: 检查 ID == "##" 和 o 的矛盾，和 n 的矛盾
                    switch (ch)
                    {
                        case 'r':
                            this._required = true;
                            break;
                        case 'o':
                            this._required = false;
                            break;
                        case 'n':
                            this._repeatable = true;
                            break;
                        case '1':
                            this._repeatable = false;
                            break;
                        default:
                            throw new Exception($"'var:{var_def}' 定义中出现了无法识别的字符 '{ch}'");
                    }
                }
            }

            if (this.ID == "##")
                SetFixLengthType();

        }

        void SetFixLengthType()
        {
            this._repeatable = false;
            this._required = true;
        }

        // 匹配别名。只要匹配上一个就算数
        public bool MatchAlias(string[] names)
        {
            if (this.Alias == null
                || this.Alias.Length == 0)
                return false;
            if (Array.IndexOf(names, this.Alias) != -1)
                return true;
            return false;
        }

        public static void ParseNameString(string name,
            out string two_chars_id,
            out string[] alias)
        {
            var names = name.Split(new char[] { '|', '_', ',' });
            if (names.Length == 0)
                throw new Exception($"name 参数值 '{name}' 不合法。至少应该包含一个名字");
            two_chars_id = names[0];

            if (string.IsNullOrEmpty(two_chars_id)
|| two_chars_id.Length != 2)
                throw new Exception($"参数 '{name}' 不合法。第一个名字必须为 2 字符");

            if (names.Length > 1)
                alias = names.ToList().GetRange(1, names.Length - 1).ToArray();
            else
                alias = null;
        }
    }

    // 一个消息的合法性规则
    public class MessageRule
    {
        public string Name { get; set; }

        List<FieldRule> _fieldRules = new List<FieldRule>();

        public IEnumerable<FieldRule> FieldRules
        {
            get
            {
                return _fieldRules;
            }
        }

        // parameters:
        //      defs        字段定义集合。例如 "91 fix:10,var:r1","92 "
        //      ref_rule    用于参考的现有字段定义
        public MessageRule(string name,
            string[] defs = null,
            MessageRule ref_rule = null)
        {
            if (string.IsNullOrEmpty(name)
|| name.Length != 2)
                throw new Exception($"消息名 '{name}' 不合法。应为 2 字符");

            this.Name = name;

            if (defs != null)
            {
                foreach (var def in defs)
                {
                    if (string.IsNullOrEmpty(def))
                        continue;
                    if (def.Length < 2)
                        throw new Exception($"字段定义 '{def}' 不合法。长度不足 2");

                    var parts = StringUtil.ParseTwoPart(def, " ");
                    var field_name = parts[0];

                    var field_def = parts[1].Trim();
                    if (string.IsNullOrEmpty(field_def))
                    {
                        var temp = ref_rule.FindFieldRule(field_name);
                        if (temp != null)
                        {
                            _fieldRules.Add(temp);
                            continue;
                        }
                    }

                    _fieldRules.Add(new FieldRule(field_name, field_def));
                }
            }
        }

        // 获得一个字段的合法性规则
        // parameters:
        //      name    要匹配的字段名字。这是一个 _ 或者 | 间隔的多个名字的字符串。第一个名字只能是 2 字符，从第二个开始是别名
        //      return_null 当找不到定义的时候，是否返回 null
        public FieldRule FindFieldRule(string name,
            FieldRule default_rule = null)
        {
            var names = name.Split(new char[] { '|', '_', ',' });

            var first = _fieldRules.Where(o => Array.IndexOf(names, o.ID) != -1 || o.MatchAlias(names)).FirstOrDefault();
            if (first != null)
                return first;
            return default_rule;
        }
    }
}
