using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL.SIP2
{
    public class VariableLengthField
    {
        //public string Name { get; set; }
        public string ID { get; set; }
        /*
        public bool IsRequired { get; set; }
        */
        public string Value { get; set; }

        /*
        // 是否是重复字段 2020/8/13加
        public bool IsRepeat { get; set; }
        */
        public FieldRule FieldRule { get; set; }

#if REMOVED
        public VariableLengthField(string id, bool required, bool repeat = false)
        {
            //this.Name = name;
            this.ID = id;
            this.IsRequired = required;

            // 是否是可重复字段 
            this.IsRepeat = repeat;
        }

        public VariableLengthField(string id, bool required, string value, bool repeat = false)
        {
            //this.Name = name;
            this.ID = id;
            this.IsRequired = required;
            this.Value = value;

            this.IsRepeat = repeat;
        }
#endif

        public VariableLengthField(string id)
        {
            this.ID = id;
        }

        public VariableLengthField(FieldRule rule)
        {
            this.ID = rule.ID;
            this.FieldRule = rule;
        }

        public bool IsRequired
        {
            get
            {
                if (this.FieldRule == null)
                    return false;
                return this.FieldRule.IsRequired;
            }

        }

        // 是否是重复字段
        public bool IsRepeat
        {
            get
            {
                if (this.FieldRule == null)
                    return true;
                return this.FieldRule.IsRepeatable;
            }
        }
    }


    public class FixedLengthField
    {
        public string Name { get; set; }
        public int Length { get; set; }

        private string _value = ""; // _value 永远不应该为 null
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == null)
                    throw new Exception("value 值不允许为 null");
                if (value.Length != Length)
                    throw new Exception($"value '{value}' 的字符数与定长字段 {this.Name} 定义的(或者最初创建时的) 字符数 {this.Length} 不符");
                this._value = value;
            }
        }


        public FixedLengthField(string name, int length)
        {
            this.Name = name;
            this.Length = length;
        }

    }

}
