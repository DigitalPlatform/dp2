using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
 Patron Status Request
 This message is used by the SC to request patron information from the ACS. 
 The ACS must respond to this command with a Patron Status Response message.
 23<language><transaction date><institution id><patron identifier><terminal password><patron password>
 23	3-char	18-char	AO	AA	AC	AD
     * */
    public class PatronStatusRequest_23 : BaseMessage
    {
        public PatronStatusRequest_23()
        {
            this.CommandIdentifier = "23";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<language><transaction date>
            //3-char	18-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Language, 3));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><patron identifier><terminal password><patron password>
            //AO	AA	AC	AD
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, true));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        // 3-char, fixed-length required field，Chinese 019
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

        // 18-char, fixed-length required field，YYYYMMDDZZZZHHMMSS
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

        // variable-length required field
        public string AA_PatronIentifier_r
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

        // variable-length required field
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

        // variable-length required field
        public string AD_PatronPassword_r
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
         
    }
}
