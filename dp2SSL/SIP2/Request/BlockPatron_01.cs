using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
 This message requests that the patron card be blocked by the ACS.  This is, for example, sent when the patron is detected tampering with the SC or when a patron forgets to take their card.  The ACS should invalidate the patron’s card and respond with a Patron Status Response message.  The ACS could also notify the library staff that the card has been blocked.
 01<card retained><transaction date><institution id><blocked card msg><patron identifier><terminal password>
 01	1-char	18-char	AO	AL  AA	AC
     */
    public class BlockPatron_01 : BaseMessage
    {
        static string[] _defs = new string[] {
                "##_CardRetained fix:1",
                "##_TransactionDate fix:18",
                "AO_InstitutionId var:r1",
                "AL_BlockedCardMsg var:r1",
                "AA_PatronIdentifier var:r1",
                "AC_TerminalPassword var:r1"
            };
        public override MessageRule GetMessageRule()
        {
            return new MessageRule("01", _defs);
        }

        public BlockPatron_01()
        {
            /*
            //==前面的定长字段
            //<card retained><transaction date>
            //1-char	18-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_CardRetained, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><blocked card msg><patron identifier><terminal password>
            //AO	AL  AA	AC
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AL_BlockedCardMsg, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, true));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
            */
            SetDefaultValue();
        }
        
        // 1-char, fixed-length required field:  Y or N.
        public string CardRetained_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_NoBlock);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("card retained参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_NoBlock, value);
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


        // variable-length required field
        public string AL_BlockedCardMsg_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AL_BlockedCardMsg);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AL_BlockedCardMsg, value);
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



    }
}
