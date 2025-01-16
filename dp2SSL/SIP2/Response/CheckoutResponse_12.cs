using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    Checkout Response
    This message must be sent by the ACS in response to a Checkout message from the SC.
    12<ok><renewal ok><magnetic media><desensitize><transaction date><institution id><patron identifier><item identifier><title identifier><due date><fee type><security inhibit><currency type><fee amount><media type><item properties><transaction id><screen message><print line>
    12	1-char	1-char	1-char	1-char	18-char	AO	AA	AB	AJ	AH BT	CI	BH	BV	CK	CH	BK	AF	AG
     */
    public class CheckoutResponse_12 : BaseMessage
    {
        public CheckoutResponse_12()
        {
            this.CommandIdentifier = "12";

#if REMOVED
            //==前面的定长字段
            //<ok><renewal ok><magnetic media><desensitize><transaction date>
            //1-char	1-char	1-char	1-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Ok, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_RenewalOk, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_MagneticMedia, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Desensitize, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><patron identifier><item identifier><title identifier><due date><fee type><security inhibit><currency type><fee amount><media type><item properties><transaction id><screen message><print line>
            //	AO	AA	AB	---AJ	    AH  BT	---CI	BH	BV	---CK	CH	BK	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, false)); //true
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AH_DueDate, false)); //true
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BT_FeeType, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CI_SecurityInhibit, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BH_CurrencyType, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BV_FeeAmount, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CK_MediaType, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BK_TransactionId, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false,true ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false,true));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        //OK should be set to 1 if the ACS checked out the item to the patron. should be set to 0 if the ACS did not check out the item to the patron.
        //1-char, fixed-length required field:  0 or 1.
        public string Ok_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Ok);
            }
            set
            {
                if (value != "0" && value != "1")
                    throw new Exception("ok参数不合法，必须为0 or 1。");

                this.SetFixedFieldValue(SIPConst.F_Ok, value);
            }
        }

        //Renewal OK should be set to Y if the patron requesting to check out the item already has the item checked out should be set to N if the item is not already checked out to the requesting patron. 
        //1-char, fixed-length required field:  Y or N.
        public string RenewalOk_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_RenewalOk);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("renewal ok参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_RenewalOk, value);
            }
        }

        //1-char, fixed-length required field:  Y or N or U.
        public string MagneticMedia_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_MagneticMedia);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("magnetic media参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_MagneticMedia, value);
            }
        }

        // Desensitize should be set to Y if the SC should desensitize the article. should be set to N if the SC should not desensitize the article (for example, a closed reserve book, or the checkout was refused).
        //1-char, fixed-length required field:  Y or N or U.
        public string Desensitize_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Desensitize);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("desensitize参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_Desensitize, value);
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

        //variable-length required field
        public string AJ_TitleIdentifier_r
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


        //variable-length required field
        public string AH_DueDate_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AH_DueDate);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AH_DueDate, value);
            }
        }


        //2-char, fixed-length optional field (01 thru 99).  The type of fee associated with checking out this item.
        public string BT_FeeType_2_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BT_FeeType);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BT_FeeType, value);
            }
        }

        //1-char, fixed-length optional field:  Y or N.
        public string CI_SecurityInhibit_1_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CI_SecurityInhibit);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CI_SecurityInhibit, value);
            }
        }

        //3-char fixed-length optional field
        public string BH_CurrencyType_3_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BH_CurrencyType);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BH_CurrencyType, value);
            }
        }

        //Fee Amount should be set to the value of the fee associated with checking out the item should be set to 0 if there is no fee associated with checking out the item.
        //variable-length optional field.  The amount of the fee associated with checking out this item.
        public string BV_FeeAmount_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BV_FeeAmount);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BV_FeeAmount, value);
            }
        }

        //3-char, fixed-length optional field
        public string CK_MediaType_3_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CK_MediaType);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CK_MediaType, value);
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

        //variable-length optional field.  May be assigned by the ACS when checking out the item involves a fee.
        public string BK_TransactionId_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BK_TransactionId);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BK_TransactionId, value);
            }
        }

        // 2020/8/14 AF,AG是可重复字段，该成员统一放在BaseMessage里
        /*
        //variable-length optional field
        public string AF_ScreenMessage_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AF_ScreenMessage);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AF_ScreenMessage, value);
            }
        }

        //variable-length optional field
        public string AG_PrintLine_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AG_PrintLine);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AG_PrintLine, value);
            }
        }
        */
    }
}
