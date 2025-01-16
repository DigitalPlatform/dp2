using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
     Patron Information Response
     The ACS must send this message in response to the Patron Information message.
     64<patron status><language><transaction date><hold items count><overdue items count><charged items count><fine items count><recall items count><unavailable holds count>
     64	14-char	3-char	18-char	4-char	4-char  4-char	4-char
     <institution id><patron identifier><personal name><hold items limit><overdue items limit>
     AO	AA	AE	    BZ	CA
     <charged items limit><valid patron><valid patron password><currency type><fee amount><fee limit>
     CB	BL	CQ	BH	BV	CC
     <hold items><overdue items><charged items><fine items><recall items><unavailable hold items>
     AS	AT	AU	AV	BU	CD
     <home address><e-mail address><home phone number><screen message><print line>
     BD	BE  BF	AF	AG
    */
    public class PatronInformationResponse_64 : BaseMessage
    {
        public PatronInformationResponse_64()
        {
            this.CommandIdentifier = "64";

#if REMOVED
            //==前面的定长字段
            //<patron status><language><transaction date><hold items count><overdue items count><charged items count><fine items count><recall items count><unavailable holds count>
            //14-char	3-char	18-char	---4-char	4-char  4-char	---4-char  4-char	4-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_PatronStatus, 14));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Language, 3));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_HoldItemsCount, 4));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_OverdueItemsCount, 4));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_ChargedItemsCount, 4));

            //<fine items count><recall items count><unavailable holds count>
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_FineItemsCount, 4));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_RecallItemsCount, 4));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_UnavailableHoldsCount, 4));

            //==后面变长字段
             //<institution id><patron identifier><personal name><hold items limit><overdue items limit>
             //AO	AA	AE	    BZ	CA
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AE_PersonalName, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BZ_HoldItemsLimit, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CA_OverdueItemsLimit, false ));

            //<charged items limit><valid patron><valid patron password><currency type><fee amount><fee limit>
            //CB	BL	CQ	BH	BV	CC
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CB_ChargedItemsLimit, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BL_ValidPatron, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CQ_ValidPatronPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BH_CurrencyType, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BV_FeeAmount, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CC_FeeLimit, false ));

             //<hold items><overdue items><charged items><fine items><recall items><unavailable hold items>
             //AS	AT	AU	AV	BU	CD
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AS_HoldItems, false,true ));//重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AT_OverdueItems, false, true));//重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AU_ChargedItems, false, true));//重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AV_FineItems, false, true));//重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BU_RecallItems, false, true));//重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CD_UnavailableHoldItems, false, true));//重复字段


            //<home address><e-mail address><home phone number><screen message><print line>
            //BD	BE  BF	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BD_HomeAddress, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BE_EmailAddress, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BF_HomePhoneNumbers, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false, true)); //重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false ,true));//重复字段

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }
        
        //14-char, fixed-length required field
        public string PatronStatus_14
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_PatronStatus);
            }
            set
            {
                if (value.Length != 14)
                    throw new Exception("patron status参数长度须为14位。");

                this.SetFixedFieldValue(SIPConst.F_PatronStatus, value);
            }
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

        //4-char, fixed-length required field
        public string HoldItemsCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_HoldItemsCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("hold items count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_HoldItemsCount, value);
            }
        }

        //4-char, fixed-length required field
        public string OverdueItemsCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_OverdueItemsCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("overdue items count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_OverdueItemsCount, value);
            }
        }


        //4-char, fixed-length required field
        public string ChargedItemsCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_ChargedItemsCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("charged items count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_ChargedItemsCount, value);
            }
        }

        //4-char, fixed-length required field
        public string FineItemsCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_FineItemsCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("fine items count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_FineItemsCount, value);
            }
        }

        //4-char, fixed-length required field
        public string RecallItemsCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_RecallItemsCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("recall items count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_RecallItemsCount, value);
            }
        }

        //4-char, fixed-length required field
        public string UnavailableHoldsCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_UnavailableHoldsCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("unavailable holds count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_UnavailableHoldsCount, value);
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
        public string AE_PersonalName_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AE_PersonalName);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AE_PersonalName, value);
            }
        }

        //4-char, fixed-length optional field
        public string BZ_HoldItemsLimit_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BZ_HoldItemsLimit);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BZ_HoldItemsLimit, value);
            }
        }

        //4-char, fixed-length optional field
        public string CA_OverdueItemsLimit_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CA_OverdueItemsLimit);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CA_OverdueItemsLimit, value);
            }
        }

        //4-char, fixed-length optional field
        public string CB_ChargedItemsLimit_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CB_ChargedItemsLimit);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CB_ChargedItemsLimit, value);
            }
        }

        //1-char, optional field:  Y or N
        public string BL_ValidPatron_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BL_ValidPatron);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BL_ValidPatron, value);
            }
        }

        //1-char, optional field: Y or N
        public string CQ_ValidPatronPassword_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CQ_ValidPatronPassword);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CQ_ValidPatronPassword, value);
            }
        }

        //3-char fixed-length optional field
        public string BH_CurrencyType_3
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

        //variable-length optional field.  The amount of fees owed by this patron.
        public string BV_feeAmount_o
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

        //variable-length optional field.  The fee limit amount.
        public string CC_FeeLimit_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CC_FeeLimit);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CC_FeeLimit, value);
            }
        }


        //item: zero or more instances of one of the following, based on “summary” field of the Patron Information message:
        //variable-length optional field  (this field should be sent for each hold item).
        public List<VariableLengthField> AS_HoldItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_AS_HoldItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_AS_HoldItems, value);
            }
        }

        //variable-length optional field  (this field should be sent for each overdue item).
        public List<VariableLengthField> AT_OverdueItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_AT_OverdueItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_AT_OverdueItems, value);
            }
        }

        //variable-length optional field  (this field should be sent for each charged item).
        public List<VariableLengthField> AU_ChargedItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_AU_ChargedItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_AU_ChargedItems, value);
            }
        }

        //variable-length optional field  (this field should be sent for each fine item).
        public List<VariableLengthField> AV_FineItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_AV_FineItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_AV_FineItems, value);
            }
        }

        //variable-length optional field  (this field should be sent for each recall item). 
        public List<VariableLengthField> BU_RecallItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_BU_RecallItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_BU_RecallItems, value);
            }
        }

        //variable-length optional field  (this field should be sent for each unavailable hold item).
        public List<VariableLengthField> CD_UnavailableHoldItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_CD_UnavailableHoldItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_CD_UnavailableHoldItems, value);
            }
        }

        //variable-length optional field
        public string BD_HomeAddress_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BD_HomeAddress);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BD_HomeAddress, value);
            }
        }

        //variable-length optional field
        public string BE_EmailAddress_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BE_EmailAddress);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BE_EmailAddress, value);
            }
        }

        //variable-length optional field
        public string BF_HomePhoneNumber_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BF_HomePhoneNumbers);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BF_HomePhoneNumbers, value);
            }
        }


    }
}
