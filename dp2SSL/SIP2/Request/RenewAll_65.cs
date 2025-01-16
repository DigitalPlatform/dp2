using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Renew All
    This message is used to renew all items that the patron has checked out.  The ACS should respond with a Renew All Response message.
    65<transaction date><institution id><patron identifier><patron password><terminal password><fee acknowledged>
    65	18-char	AO	AA	AD	AC	BO
     */
    public class RenewAll_65 : BaseMessage
    {
        public RenewAll_65()
        {
            this.CommandIdentifier = "65";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<transaction date>
            //18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><patron identifier><patron password><terminal password><fee acknowledged>
            //	AO	AA	AD	AC	BO
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword,false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false)) ;
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BO_FeeAcknowledged, false)) ;

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
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

        //1-char, optional field: Y or N.
        public string BO_FeeAcknowledged_o
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

    }
}
