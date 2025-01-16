using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    Item Information
    This message may be used to request item information.  The ACS should respond with the Item Information Response message.
    17<transaction date><institution id>< item identifier ><terminal password>
    17	18-char	AO	AB	AC
    */
    public class ItemInformation_17 : BaseMessage
    {
        // 构造函数
        public ItemInformation_17()
        {
            this.CommandIdentifier = "17";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        // 18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS
        public string TransactionDate_18
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_TransactionDate);
            }
            set
            {
                if (value.Length != 18)
                    throw new Exception("transaction date参数长度须为18位。");

                this.SetFixedFieldValue(SIPConst.F_TransactionDate, value);
            }
        }

        // variable-length required field
        public string AO_InstitutionId_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AO_InstitutionId, value);
            }
        }

        // variable-length required field.
        public string AB_ItemIdentifier_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AB_ItemIdentifier);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AB_ItemIdentifier, value);
            }
        }

        // variable-length optional fiel d
        public string AC_TerminalPassword_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AC_TerminalPassword);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AC_TerminalPassword, value);
            }
        }


        /*
        public ItemInformation_17(string p_transactionDate_18
            , string p_institutionId_AO_r
            , string p_itemIdentifier_AB_r
            , string p_terminalPassword_AC_o
            )
        {
            if (p_transactionDate_18.Length != 18)
                throw new Exception("p_transactionDate_18字段长度必须是4位");
            this.TransactionDate_18 = p_transactionDate_18;

            if (p_institutionId_AO_r == null)
                throw new Exception("p_institutionId_AO_r不能为null");
            this.InstitutionId_AO_r = p_institutionId_AO_r;

            if (p_itemIdentifier_AB_r == null)
                throw new Exception("p_itemIdentifier_AB_r不能为null");
            this.ItemIdentifier_AB_r = p_itemIdentifier_AB_r;

            this.TerminalPassword_AC_o = p_terminalPassword_AC_o;
        }

        // 解析字符串命令为对象
        public override bool parse(string text, out string error)
        {
            error = "";

            if (text == null || text.Length < 2)
            {
                error = "命令字符串为null或长度小于2位";
                return false;
            }
            string cmdIdentifiers = text.Substring(0, 2);
            text = text.Substring(2);

            //处理定长字段
            string rest = text;
            while (rest.Length > 0)
            {
                if (String.IsNullOrEmpty(this.TransactionDate_18)==true)
                {
                    this.TransactionDate_18 = rest.Substring(0, 18);
                    rest = rest.Substring(18);
                    continue;
                }
                break;
            }

            //处理变长字段
            string[] parts = rest.Split(new char[] { '|' });
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                {
                    continue;
                }
                //AO	AB	AC
                string fieldId = part.Substring(0, 2);
                string value = part.Substring(2);
                if (fieldId == "AO")
                {
                    this.InstitutionId_AO_r = value;
                }
                else if (fieldId == "AB")
                {
                    this.ItemIdentifier_AB_r = value;
                }
                else if (fieldId == "AC")
                {
                    this.TerminalPassword_AC_o = value;
                }
                else
                {
                    error = "不支持的字段:" + part;
                    return false;
                }
            }

            // 校验;
            bool ret = this.Verify(out error);
            if (ret == false)
                return false;

            return true;

        }

        // 校验对象的各参数是否合法
        public override bool Verify(out string error)
        {
            error = "";

            //18-char
            if (this.TransactionDate_18 == "")
            {
                error = "transactionDate_18字段未赋值";
                goto ERROR1;
            }

            //AO	AB
            if (this.InstitutionId_AO_r == null)
            {
                error = "缺必备字段AO";
                goto ERROR1;
            }
            if (this.ItemIdentifier_AB_r == null)
            {
                error = "缺必备字段AB";
                goto ERROR1;
            }


            return true;
        ERROR1:

            return false;
        }

        // 将对象转换字符串命令
        public override string ToText()
        {
            string text = "17";

            //18-char
            text += this.TransactionDate_18;

            //AO	AB	AC
            if (this.InstitutionId_AO_r != null)
                text += "AO" + this.InstitutionId_AO_r + "|";

            if (this.ItemIdentifier_AB_r != null)
                text += "AB" + this.ItemIdentifier_AB_r + "|";

            if (this.TerminalPassword_AC_o != null)
                text += "AC" + this.TerminalPassword_AC_o + "|";

            return text;
        }
         */
    }
}
