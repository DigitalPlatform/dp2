using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
 Patron Status Response
 The ACS must send this message in response to a Patron Status Request message as well as in response to a Block Patron message.
 24<patron status><language><transaction date><institution id><patron identifier><personal name><valid patron><valid patron password><currency type><fee amount><screen message><print line>
 24	14-char	3-char	18-char	AO	AA	AE	    BL  CQ  BH	BV	AF	AG
     * */
    public class PatronStatusResponse_24 : BaseMessage
    {
        public PatronStatusResponse_24()
        {
            this.CommandIdentifier = "24";

#if REMOVED
            //==前面的定长字段
            //<patron status><language><transaction date>
            //14-char	3-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_PatronStatus, 14));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Language, 3));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><patron identifier><personal name><valid patron><valid patron password><currency type><fee amount><screen message><print line>
            //AO	AA	AE	    BL --- CQ  BH	BV	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AE_PersonalName, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BL_ValidPatron, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CQ_ValidPatronPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BH_CurrencyType, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BV_FeeAmount, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false,true ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false,true ));

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
        public string AA_PatronIdentifier_AA_r
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

        //1-char, optional field: Y or N
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

        //3-char, fixed-length optional field
        public string BH_CurrencyType_o
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
