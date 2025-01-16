using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
     2.00 End Session Response
     The ACS must send this message in response to the End Patron Session message.
     36<end session>< transaction date >< institution id >< patron identifier ><screen message><print line>
     36	1-char	18-char	AO	AA	AF	AG
     */
    public class EndSessionResponse_36 : BaseMessage
    {
        public EndSessionResponse_36()
        {
            this.CommandIdentifier = "36";

#if REMOVED
            //==前面的定长字段
            //<end session>< transaction date >
            //1-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_EndSession, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //< institution id >< patron identifier ><screen message><print line>
            //AO	AA	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false,true ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false,true ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        //1-char, fixed-length required field:  Y or N.
        public string EndSession_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_EndSession);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("end session参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_EndSession, value);
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

        //variable-length required field.
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
