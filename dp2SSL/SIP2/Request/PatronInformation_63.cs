using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Patron Information
    This message is a superset of the Patron Status Request message.  It should be used to request patron information.  The ACS should respond with the Patron Information Response message.
    63<language><transaction date><summary><institution id><patron identifier><terminal password><patron password><start item><end item>
   63	3-char	18-char	10-char	AO	AA	AC	AD	BP  BQ
     * */
    public class PatronInformation_63 : BaseMessage
    {

        public PatronInformation_63()
        {
            this.CommandIdentifier = "63";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Language, 3));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Summary, 10));

            //==后面变长字段 AO	AA	AC	AD	BP  BQ
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BP_StartItem, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BQ_EndItem, false ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();

            Language_3 = "019"; //Chinese 019
            Summary_10 = "  Y       ";
        }
        

        //3-char, fixed-length required field
        public string Language_3
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Language);
            }
            set
            {
                if (value.Length != 3)
                    throw new Exception("language参数长度须为3位。");

                this.SetFixedFieldValue(SIPConst.F_Language, value);
            }
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

        //10-char, fixed-length required field
        public string Summary_10
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Summary);
            }
            set
            {
                if (value.Length != 10)
                    throw new Exception("summary参数长度须为10位。");

                this.SetFixedFieldValue(SIPConst.F_Summary, value);
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

        // variable-length required field
        public string AA_PatronIdentifier_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AA_PatronIdentifier);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AA_PatronIdentifier, value);
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

        // variable-length optional field
        public string AD_PatronPassword_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AD_PatronPassword);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AD_PatronPassword, value);
            }
        }

        // variable-length optional field
        public string BP_StartItem_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BP_StartItem);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BP_StartItem, value);
            }
        }

        //variable-length optional field
        public string BQ_EndItem_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BQ_EndItem);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BQ_EndItem, value);
            }
        }

        /*
        // 构造函数
        public PatronInformation_63()
        { }

        public PatronInformation_63(string p_language_3
            , string p_transactionDate_18
            , string p_summary_10

            , string p_institutionId_AO_r
            , string p_patronIdentifier_AA_r
            , string p_terminalPassword_AC_o

            , string p_patronPassword_AD_o
            , string p_startItem_BP_o
            , string p_endItem_BQ_o
            )
        {
            if (p_language_3.Length != 3)
                throw new Exception("p_language_3字段长度必须是3位");
            this.Language_3 = p_language_3;

            if (p_transactionDate_18.Length != 18)
                throw new Exception("p_transactionDate_18字段长度必须是4位");
            this.TransactionDate_18 = p_transactionDate_18;

            if (p_summary_10.Length != 10)
                throw new Exception("p_summary_10字段长度必须是4位");
            this.Summary_10 = p_summary_10;

            //===
            if (p_institutionId_AO_r == null)
                throw new Exception("p_institutionId_AO_r不能为null");
            this.InstitutionId_AO_r = p_institutionId_AO_r;

            if (p_patronIdentifier_AA_r == null)
                throw new Exception("p_patronIdentifier_AA_r不能为null");
            this.PatronIdentifier_AA_r = p_patronIdentifier_AA_r;

            this.TerminalPassword_AC_o = p_terminalPassword_AC_o;

            //===
            this.PatronPassword_AD_o = p_patronPassword_AD_o;
            this.StartItem_BP_o = p_startItem_BP_o;
            this.EndItem_BQ_o = p_endItem_BQ_o;
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
                if (String.IsNullOrEmpty(this.Language_3)==true)
                {
                    this.Language_3 = rest.Substring(0, 3);
                    rest = rest.Substring(3);
                    continue;
                }
                if (String.IsNullOrEmpty(this.TransactionDate_18)==true)
                {
                    this.TransactionDate_18 = rest.Substring(0, 18);
                    rest = rest.Substring(18);
                    continue;
                }
                if (String.IsNullOrEmpty(this.Summary_10)==true)
                {
                    this.Summary_10 = rest.Substring(0, 10);
                    rest = rest.Substring(10);
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
                //AO	AA	AC	AD	BP  BQ
                string fieldId = part.Substring(0, 2);
                string value = part.Substring(2);
                if (fieldId == "AO")
                {
                    this.InstitutionId_AO_r = value;
                }
                else if (fieldId == "AA")
                {
                    this.PatronIdentifier_AA_r = value;
                }
                else if (fieldId == "AC")
                {
                    this.TerminalPassword_AC_o = value;
                }
                else if (fieldId == "AD")
                {
                    this.PatronPassword_AD_o = value;
                }
                else if (fieldId == "BP")
                {
                    this.StartItem_BP_o = value;
                }
                else if (fieldId == "BQ")
                {
                    this.EndItem_BQ_o = value;
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

            //3-char	18-char	10-char
            if (this.Language_3 == "")
            {
                error = "language_3字段未赋值";
                goto ERROR1;
            }

            if (this.TransactionDate_18 == "")
            {
                error = "transactionDate_18字段未赋值";
                goto ERROR1;
            }

            if (this.Summary_10 == "")
            {
                error = "summary_10字段未赋值";
                goto ERROR1;
            }
            //AO	AA
            if (this.InstitutionId_AO_r == null)
            {
                error = "缺必备字段AO";
                goto ERROR1;
            }
            if (this.PatronIdentifier_AA_r == null)
            {
                error = "缺必备字段AA";
                goto ERROR1;
            }


            return true;
        ERROR1:

            return false;
        }

        // 将对象转换字符串命令
        public override string ToText()
        {
            string text = "63";

            //3-char	18-char	10-char
            text += this.Language_3;
            text += this.TransactionDate_18;
            text += this.Summary_10;

            //AO	AA	AC	AD	BP  BQ
            if (this.InstitutionId_AO_r != null)
                text += "AO" + this.InstitutionId_AO_r + "|";

            if (this.PatronIdentifier_AA_r != null)
                text += "AA" + this.PatronIdentifier_AA_r + "|";

            if (this.TerminalPassword_AC_o != null)
                text += "AC" + this.TerminalPassword_AC_o + "|";

            if (this.PatronPassword_AD_o != null)
                text += "AD" + this.PatronPassword_AD_o + "|";

            if (this.StartItem_BP_o != null)
                text += "BP" + this.StartItem_BP_o + "|";

            if (this.EndItem_BQ_o != null)
                text += "BQ" + this.EndItem_BQ_o + "|";

            return text;
        }
         */
    }
}
