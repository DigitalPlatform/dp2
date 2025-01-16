using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    Checkout
    This message is used by the SC to request to check out an item, and also to cancel a Checkin request that did not successfully complete.  The ACS must respond to this command with a Checkout Response message.
    11<SC renewal policy><no block><transaction date><nb due date><institution id><patron identifier><item identifier><terminal password><patron password><item properties><fee acknowledged><cancel>
11	1-char	1-char	18-char	18-char	 AO	AA	AB	AC      AD	CH	BO	BI
     */
    public class Checkout_11 : BaseMessage
    {
        // 构造函数
        public Checkout_11()
        {
            this.CommandIdentifier = "11";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<SC renewal policy><no block><transaction date><nb due date>
            //1-char	1-char	18-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_SCRenewalPolicy, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_NoBlock, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_NbDueDate, 18));

            //==后面变长字段 
            //<institution id><patron identifier><item identifier><terminal password><patron password><item properties><fee acknowledged><cancel>
            //AO	AA	AB	AC      AD	CH	BO	BI
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, true));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, false));            
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BO_FeeAcknowledged, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BI_Cancel, false));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();

            base.SetDefaultValue();

            SCRenewalPolicy_1 = "Y"; //默认设为Y,允许续借
            NoBlock_1 = "N";
            NbDueDate_18 = "".PadLeft(18, ' '); //默认为18个空格

            AC_TerminalPassword_r = "";
            BO_FeeAcknowledged_1_o = "N";
            BI_Cancel_1_o = "N";
        }


        //Y表示SC已由图书馆工作人员配置可以进行续借，N表示不可以续借。
        //1-char,fixed-length required field:  Y or N.
        public string SCRenewalPolicy_1
        {
            get
            {
                return this.GetFixedFieldValue("SCRenewalPolicy");
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("SC renewal policy参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_SCRenewalPolicy, value);
            }
        }

        //当此字段为Y时，ACS不应阻止此事务，因为当ACS离线时,在SC该书已经被借或还。。
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

        //The date and time that the patron checked out the item at the SC unit.
        //18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS.  
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

        //18-char,fixed-length required field:  YYYYMMDDZZZZHHMMSS
        //当noBlock为N时，该值为空。
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

        //图书馆的机构ID,目前传的dp2Library
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

        //读者证条码号
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

        //册条码号
        //variable-length required field
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

        //This is the password for the SC unit.  目前该字段传空值
        //variable-length required field
        public string AC_TerminalPassword_r
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

        //In current applications, this field is not used. 目前不传这个字段
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

        // 读者密码，目前不传这个字段
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

        //如果该字段值为N，并且操作时发现一个待交费事项，则ACS应在返回消息里告诉SC有一个待交费事项且拒绝借出该书。
        //如果SC与读者经过交互，且读者同意支付费用，则该字段将在第二个Checkout消息上设置为Y，向ACS表明读者已经确认该费用，并且该借书操作不应该被拒绝
        //目前传的N
        //1-char, optional field: Y or N
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


        // 当Checkout此参数传Y时，则取消上一个错误的Checkin
        //当Checkin此参数传Y时，则取消上一个错误的Checkout
        //普通的Checkout与Checkin，此参数都就传N。
        //1-char,optional field: Y or N
        public string BI_Cancel_1_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BI_Cancel);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BI_Cancel, value);
            }
        }


        /*

        public Checkout_11(string p_SCRenewalPolicy_1
            , string p_noBlock_1
            , string p_transactionDate_18

            , string p_nbDueDate_18
            , string p_institutionId_AO_r
            , string p_patronIdentifier_AA_r

            , string p_itemIdentifier_AB_r
            , string p_terminalPassword_AC_r
            , string p_itemProperties_CH_o

            , string p_patronPassword_AD_o
            , string p_feeAcknowledged_BO_1_o
            , string p_cancel_BI_1_o)
        {
            if (p_SCRenewalPolicy_1.Length != 1)
                throw new Exception("SCRenewalPolicy_1字段长度必须是1位");
            this.SCRenewalPolicy_1 = p_SCRenewalPolicy_1;

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



            if (p_itemIdentifier_AB_r == null)
                throw new Exception("itemIdentifier_AB_r不能为null");
            this.ItemIdentifier_AB_r = p_itemIdentifier_AB_r;

            if (p_terminalPassword_AC_r == null)
                throw new Exception("terminalPassword_AC_r不能为null");
            this.TerminalPassword_AC_r = p_terminalPassword_AC_r;

            this.ItemProperties_CH_o = p_itemProperties_CH_o;


            this.PatronPassword_AD_o = p_patronPassword_AD_o;
            this.FeeAcknowledged_BO_1_o = p_feeAcknowledged_BO_1_o;
            this.Cancel_BI_1_o = p_cancel_BI_1_o;
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
                if (String.IsNullOrEmpty(this.SCRenewalPolicy_1)==true)
                {
                    this.SCRenewalPolicy_1 = rest.Substring(0, 1);
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
                if (String.IsNullOrEmpty(this.NbDueDate_18)==true)
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
                //AO	AA	AB	AC  AD	CH	BO	BI
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
                else if (fieldId == "AB")
                {
                    this.ItemIdentifier_AB_r = value;
                }
                else if (fieldId == "AC")
                {
                    this.TerminalPassword_AC_r = value;
                }
                else if (fieldId == "AD")
                {
                    this.PatronPassword_AD_o = value;
                }
                else if (fieldId == "CH")
                {
                    this.ItemProperties_CH_o = value;
                }
                else if (fieldId == "BO")
                {
                    this.FeeAcknowledged_BO_1_o = value;
                }
                else if (fieldId == "BI")
                {
                    this.Cancel_BI_1_o = value;
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
            if (this.SCRenewalPolicy_1 == "")
            {
                error = "SCRenewalPolicy_1字段未赋值";
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
            //AO	AA	AB	AC 
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

            if (this.ItemIdentifier_AB_r == null)
            {
                error = "缺必备字段AB";
                goto ERROR1;
            }

            if (this.TerminalPassword_AC_r == null)
            {
                error = "缺必备字段AC";
                goto ERROR1;
            }


            return true;
        ERROR1:

            return false;
        }

        // 将对象转换字符串命令
        public override string ToText()
        {
            string text = "11";

            //1-char	1-char	18-char	18-char
            text += this.SCRenewalPolicy_1;
            text += this.NoBlock_1;
            text += this.TransactionDate_18;
            text += this.NbDueDate_18;

            //AO	AA	AB	AC  AD	CH	BO	BI
            if (this.InstitutionId_AO_r != null)
                text += "AO" + this.InstitutionId_AO_r + "|";

            if (this.PatronIdentifier_AA_r != null)
                text += "AA" + this.PatronIdentifier_AA_r + "|";

            if (this.ItemIdentifier_AB_r != null)
                text += "AB" + this.ItemIdentifier_AB_r + "|";

            if (this.TerminalPassword_AC_r != null)
                text += "AC" + this.TerminalPassword_AC_r + "|";

            if (this.PatronPassword_AD_o != null)
                text += "AD" + this.PatronPassword_AD_o + "|";

            if (this.ItemProperties_CH_o != null)
                text += "CH" + this.ItemProperties_CH_o + "|";

            if (this.FeeAcknowledged_BO_1_o != null)
                text += "BO" + this.FeeAcknowledged_BO_1_o + "|";

            if (this.Cancel_BI_1_o != null)
                text += "BI" + this.Cancel_BI_1_o + "|";

            return text;
        }
        */
    }
}
