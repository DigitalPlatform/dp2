using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
2.00 End Patron Session
This message will be sent when a patron has completed all of their transactions.  The ACS may, upon receipt of this command, close any open files or deallocate data structures pertaining to that patron. The ACS should respond with an End Session Response message.
35<transaction date><institution id><patron identifier><terminal password><patron password>
35	18-char	AO	AA	AC	AD
  */
    public class EndPatronSession_35 : BaseMessage
    {
        public EndPatronSession_35()
        {
            this.CommandIdentifier = "35";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<transaction date>
            //18-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><patron identifier><terminal password><patron password>
            //AO	AA	AC	AD
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, false));

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

        // variable-length optional field
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
        
    }
}
