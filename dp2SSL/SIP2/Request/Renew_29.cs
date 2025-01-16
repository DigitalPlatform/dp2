using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{

    /*
   2.00 Renew
   This message is used to renew an item.  The ACS should respond with a Renew Response message. Either or both of the “item identifier” and “title identifier” fields must be present for the message to be useful.
   29<third party allowed><no block><transaction date><nb due date><institution id><patron identifier><patron password><item identifier><title identifier><terminal password><item properties><fee acknowledged>
   29	1-char	1-char	18-char	18-char	AO	AA	AD	AB AJ	AC	CH	BO
     */
    public class Renew_29 : BaseMessage
    {
        public Renew_29()
        {
            this.CommandIdentifier = "29";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<third party allowed><no block><transaction date><nb due date>
            //1-char	1-char	18-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_ThirdPartyAllowed, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_NoBlock, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_NbDueDate, 18));

            //==后面变长字段 
            //<institution id><patron identifier><patron password><item identifier><title identifier><terminal password><item properties><fee acknowledged>
            //AO	AA	AD	AB AJ	AC	CH	BO
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BO_FeeAcknowledged, false ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();

            ThirdPartyAllowed_1 = "N"; //默认设为N,不允许第三方续借
            NoBlock_1 = "N";
            NbDueDate_18 = "".PadLeft(18, ' '); //默认为18个空格
            BO_FeeAcknowledged_1_o = "N";
        }

        //1-char, fixed-length required field:  Y or N.
        public string ThirdPartyAllowed_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_ThirdPartyAllowed);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("third party allowed参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_ThirdPartyAllowed, value);
            }
        }

        //1-char, fixed-length required field:  Y or N.
        public string NoBlock_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_NoBlock);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("no block参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_NoBlock, value);
            }
        }

        //18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS
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

        //18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS
        public string NbDueDate_18
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_NbDueDate);
            }
            set
            {
                if (value.Length != 18)
                    throw new Exception("Nb due date参数长度须为18位。");

                this.SetFixedFieldValue(SIPConst.F_NbDueDate, value);
            }
        }

        //variable-length required field
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

        //variable-length required field
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


        //variable-length optional field
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

        //variable-length optional field
        public string AB_ItemIdentifier_o
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


        //variable-length optional field
        public string AJ_TitleIdentifier_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AJ_TitleIdentifier);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AJ_TitleIdentifier, value);
            }
        }


        //variable-length optional field
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

        //variable-length optional field
        public string CH_ItemProperties_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CH_ItemProperties);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CH_ItemProperties, value);
            }
        }

        //1-char, optional field: Y or N.
        public string BO_FeeAcknowledged_1_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BO_FeeAcknowledged);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BO_FeeAcknowledged, value);
            }
        }

        /*
        // 构造函数
        public Renew_29()
        { }

        public Renew_29(string p_thirdPartyAllowed_1
            , string p_noBlock_1
            , string p_transactionDate_18

            , string p_nbDueDate_18
            , string p_institutionId_AO_r
            , string p_patronIdentifier_AA_r

            , string p_patronPassword_AD_o
            , string p_itemIdentifier_AB_o
            , string p_titleIdentifier_AJ_o

            , string p_terminalPassword_AC_o
            , string p_itemProperties_CH_o
            , string p_feeAcknowledged_BO_1_o
            )
        {
            if (p_thirdPartyAllowed_1.Length != 1)
                throw new Exception("p_thirdPartyAllowed_1字段长度必须是1位");
            this.ThirdPartyAllowed_1 = p_thirdPartyAllowed_1;

            if (p_noBlock_1.Length != 1)
                throw new Exception("noBlock_1字段长度必须是3位");
            this.NoBlock_1 = p_noBlock_1;

            if (p_transactionDate_18.Length != 18)
                throw new Exception("transactionDate_18字段长度必须是4位");
            this.TransactionDate_18 = p_transactionDate_18;



            if (p_nbDueDate_18.Length != 18)
                throw new Exception("nbDueDate_18字段长度必须是4位");
            this.NbDueDate_18 = p_nbDueDate_18;

            if (p_institutionId_AO_r == null)
                throw new Exception("institutionId_AO_r不能为null");
            this.InstitutionId_AO_r = p_institutionId_AO_r;

            if (p_patronIdentifier_AA_r == null)
                throw new Exception("patronIdentifier_AA_r不能为null");
            this.PatronIdentifier_AA_r = p_patronIdentifier_AA_r;


            this.PatronPassword_AD_o = p_patronPassword_AD_o;
            this.ItemIdentifier_AB_o = p_itemIdentifier_AB_o;
            this.TitleIdentifier_AJ_o = p_titleIdentifier_AJ_o;

            this.TerminalPassword_AC_o = p_terminalPassword_AC_o;
            this.ItemProperties_CH_o = p_itemProperties_CH_o;
            this.FeeAcknowledged_BO_1_o = p_feeAcknowledged_BO_1_o;
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
                if (String.IsNullOrEmpty(this.ThirdPartyAllowed_1)==true)
                {
                    this.ThirdPartyAllowed_1 = rest.Substring(0, 1);
                    rest = rest.Substring(1);
                    continue;
                }
                if (String.IsNullOrEmpty(this.NoBlock_1)==true)
                {
                    this.NoBlock_1 = rest.Substring(0, 1);
                    rest = rest.Substring(1);
                    continue;
                }
                if (String.IsNullOrEmpty(this.TransactionDate_18)==true)
                {
                    this.TransactionDate_18 = rest.Substring(0, 18);
                    rest = rest.Substring(18);
                    continue;
                }
                if (String.IsNullOrEmpty(this.NbDueDate_18 )==true)
                {
                    this.NbDueDate_18 = rest.Substring(0, 18);
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
                    //error = "发现不足2位的字段:" + part;
                    //goto ERROR1;
                }
                //AO	AA AD	AB AJ	AC	CH	BO
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

                else if (fieldId == "AD")
                {
                    this.PatronPassword_AD_o = value;
                }
                else if (fieldId == "AB")
                {
                    this.ItemIdentifier_AB_o = value;
                }
                else if (fieldId == "AJ")
                {
                    this.TitleIdentifier_AJ_o = value;
                }
                //===
                else if (fieldId == "AC")
                {
                    this.TerminalPassword_AC_o = value;
                }
                else if (fieldId == "CH")
                {
                    this.ItemProperties_CH_o = value;
                }
                else if (fieldId == "BO")
                {
                    this.FeeAcknowledged_BO_1_o = value;
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

            //1-char	1-char	18-char	18-char
            if (this.ThirdPartyAllowed_1 == "")
            {
                error = "thirdPartyAllowed_1字段未赋值";
                goto ERROR1;
            }
            if (this.NoBlock_1 == "")
            {
                error = "noBlock_1字段未赋值";
                goto ERROR1;
            }

            if (this.TransactionDate_18 == "")
            {
                error = "transactionDate_18字段未赋值";
                goto ERROR1;
            }

            if (this.NbDueDate_18 == "")
            {
                error = "nbDueDate_18字段未赋值";
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
            string text = "29";

            //1-char	1-char	18-char	18-char
            text += this.ThirdPartyAllowed_1;
            text += this.NoBlock_1;
            text += this.TransactionDate_18;
            text += this.NbDueDate_18;

            //AO	AA AD	AB AJ	AC	CH	BO
            if (this.InstitutionId_AO_r != null)
                text += "AO" + this.InstitutionId_AO_r + "|";

            if (this.PatronIdentifier_AA_r != null)
                text += "AA" + this.PatronIdentifier_AA_r + "|";

            if (this.PatronPassword_AD_o != null)
                text += "AD" + this.PatronPassword_AD_o + "|";
            if (this.ItemIdentifier_AB_o != null)
                text += "AB" + this.ItemIdentifier_AB_o + "|";
            if (this.TitleIdentifier_AJ_o != null)
                text += "AJ" + this.TitleIdentifier_AJ_o + "|";


            if (this.TerminalPassword_AC_o != null)
                text += "AC" + this.TerminalPassword_AC_o + "|";
            if (this.ItemProperties_CH_o != null)
                text += "CH" + this.ItemProperties_CH_o + "|";
            if (this.FeeAcknowledged_BO_1_o != null)
                text += "BO" + this.FeeAcknowledged_BO_1_o + "|";



            return text;
        }
         */
    }
}
