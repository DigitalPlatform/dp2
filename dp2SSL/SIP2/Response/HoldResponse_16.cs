using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
 2.00 Hold Response
 The ACS should send this message in response to the Hold message from the SC.
 16<ok><available><transaction date><expiration date><queue position><pickup location><institution id><patron identifier><item identifier><title identifier><screen message><print line>
  16	1-char	1-char	18-char	BW	BR	BS	AO	AA	AB AJ	AF	AG
     * */
    public class HoldResponse_16 : BaseMessage
    {
        public HoldResponse_16()
        {
            this.CommandIdentifier = "16";

#if REMOVED
            //==前面的定长字段
            //<ok><available><transaction date>
            //1-char	1-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Ok, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Available, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<expiration date><queue position><pickup location><institution id><patron identifier><item identifier><title identifier><screen message><print line>
            //BW	BR	BS	AO ---	AA	AB AJ	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BW_ExpirationDate, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BR_QueuePosition, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BS_PickupLocation, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false,true ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false,true ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }
        
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

        //1-char, fixed-length required field:  Y or N.
        public string Available_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Available);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("available参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_Available, value);
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
                return this.GetVariableFieldValue(SIPConst.F_AO_InstitutionId);
            }
            set
            {
                if (value.Length != 18)
                    throw new Exception("expiration date参数长度须为18位。");


                this.SetFixedFieldValue(SIPConst.F_AO_InstitutionId, value);
            }
        }

        //variable-length optional field
        public string BR_QueuePosition_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BR_QueuePosition);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BR_QueuePosition, value);
            }
        }

        //variable-length optional field
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
