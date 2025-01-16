using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Hold
    This message is used to create, modify, or delete a hold.  The ACS should respond with a Hold Response message.  Either or both of the “item identifier” and “title identifier” fields must be present for the message to be useful.
    15<hold mode><transaction date><expiration date><pickup location><hold type><institution id><patron identifier><patron password><item identifier><title identifier><terminal password><fee acknowledged>
    15	1-char	18-char	BW	BS	BY	AO	AA	AD	AB AJ	AC	BO
     */
    public class Hold_15 : BaseMessage
    {
        public Hold_15()
        {
            this.CommandIdentifier = "15";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<hold mode><transaction date>
            //1-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_HoldMode, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<expiration date><pickup location><hold type><institution id><patron identifier><patron password><item identifier><title identifier><terminal password><fee acknowledged>
            //BW	BS	BY	 ---AO	AA	AD	---AB AJ	AC	BO
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BW_ExpirationDate, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BS_PickupLocation, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BY_HoldType, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AD_PatronPassword, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BO_FeeAcknowledged, false ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        // 1-char, fixed-length required field  '+'/'-'/'*'  Add, delete, change
        public string HoldMode_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_HoldMode);
            }
            set
            {
                if (value != "+" && value != "-" && value!="*")
                    throw new Exception("SC renewal policy_1参数不合法，必须为+ or - or *。");

                this.SetFixedFieldValue(SIPConst.F_HoldMode, value);
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

        //18-char, fixed-length optional field:  YYYYMMDDZZZZHHMMSS
        public string BW_ExpirationDate_18
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BW_ExpirationDate);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BW_ExpirationDate, value);
            }
        }


        //variable-length, optional field
        public string BS_PickupLocation_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BS_PickupLocation);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BS_PickupLocation, value);
            }
        }


        //1-char, optional field
        public string BY_HoldType_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BY_HoldType);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BY_HoldType, value);
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
        public string AB_ItemIdentifier_o
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

        //variable-length optional field
        public string AJ_TitleIdentifier_o
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
