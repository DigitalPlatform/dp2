using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace   dp2SSL.SIP2
{
    /*
     Fee Paid
     This message can be used to notify the ACS that a fee has been collected from the patron. The ACS should record this information in their database and respond with a Fee Paid Response message.
     37<transaction date><fee type><payment type><currency type><fee amount><institution id><patron identifier><terminal password><patron password><fee identifier><transaction id>
     37	18-char	2-char	2-char	3-char	BV	AO	AA	AC	AD  CG	BK
     */
    public class FeePaid_37 : BaseMessage
    {
        public FeePaid_37()
        {
            this.CommandIdentifier = "37";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<transaction date><fee type><payment type><currency type>
            //18-char	2-char	2-char	3-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_BT_FeeType, 2));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_PaymentType, 2));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_BH_CurrencyType, 3));

            //==后面变长字段
            //<fee amount><institution id><patron identifier><terminal password><patron password><fee identifier><transaction id>
            //BV	AO	AA	AC	AD  CG	BK
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BV_FeeAmount, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CG_FeeIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BK_TransactionId, false ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
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

        // 2-char, fixed-length required field (01 thru 99). identifies a fee type to apply  the payment to.
        public string FeeType_2
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_BT_FeeType);
            }
            set
            {
                if (value.Length != 2)
                    throw new Exception("fee type参数长度须为2位。");

                this.SetFixedFieldValue(SIPConst.F_BT_FeeType, value);
            }
        }

        // 2-char, fixed-length required field (00 thru 99)
        public string PaymentType_2
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_PaymentType);
            }
            set
            {
                if (value.Length != 2)
                    throw new Exception("payment type参数长度须为2位。");

                this.SetFixedFieldValue(SIPConst.F_PaymentType, value);
            }
        }

        // 3-char, fixed-length required field
        public string CurrencyType_3
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_BH_CurrencyType);
            }
            set
            {
                if (value.Length != 3)
                    throw new Exception("currency type参数长度须为3位。");

                this.SetFixedFieldValue(SIPConst.F_BH_CurrencyType, value);
            }
        }

        // variable-length required field; the amount paid.
        public string BV_FeeAmount_r
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

        // variable-length optional field; identifies a specific fee to apply the payment to.
        public string CG_FeeIdentifier_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CG_FeeIdentifier);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CG_FeeIdentifier, value);
            }
        }

        // variable-length optional field; a transaction id assigned by the payment device.
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


    }
}
