using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Item Information Response
    The ACS must send this message in response to the Item Information message. 
    18<circulation status><security marker><fee type><transaction date><hold queue length><due date><recall date><hold pickup date><item identifier><title identifier><owner><currency type><fee amount><media type><permanent location><current location><item properties><screen message><print line>
    18	2-char	2-char	2-char	18-char	CF	AF	CJ	CM	AB AJ	BG	BH	BV	CK	AQ	AP	CH	AF	AG
     */
    public class ItemInformationResponse_18 : BaseMessage
    {
        public ItemInformationResponse_18()
        {
            this.CommandIdentifier = "18";

#if REMOVED
            //==前面的定长字段
            //<circulation status><security marker><fee type><transaction date>
            //2-char	2-char	2-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_CirculationStatus, 2));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_SecurityMarker, 2));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_BT_FeeType, 2));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<hold queue length><due date><recall date><hold pickup date>
            //CF	AF	CJ	CM
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CF_HoldQueueLength, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AH_DueDate, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CJ_RecallDate, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CM_HoldPickupDate, false));

            //<item identifier><title identifier><owner><currency type>
            //AB AJ	BG	BH	
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BG_Owner, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BH_CurrencyType, false));

            //<fee amount><media type><permanent location><current location>
            //BV	CK	AQ	AP	
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BV_FeeAmount, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CK_MediaType, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AQ_PermanentLocation, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AP_CurrentLocation, false));

            //<item properties><screen message><print line>
            //CH	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false, true));

            // 2020/12/8
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_KC_CallNo, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_KP_CurrentShelfNo, false));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_KQ_PermanentShelfNo, false));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        //2-char, fixed-length required field (00 thru 99)
        public string CirculationStatus_2
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_CirculationStatus);
            }
            set
            {
                if (value.Length != 2)
                    throw new Exception("circulation status参数长度须为2位。");

                this.SetFixedFieldValue(SIPConst.F_CirculationStatus, value);
            }
        }

        //2-char, fixed-length required field (00 thru 99)
        public string SecurityMarker_2
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_SecurityMarker);
            }
            set
            {
                if (value.Length != 2)
                    throw new Exception("security marker参数长度须为2位。");

                this.SetFixedFieldValue(SIPConst.F_SecurityMarker, value);
            }
        }

        //2-char, fixed-length required field (01 thru 99).  The type of fee associated with checking out this item.
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

        //variable-length optional field
        public string CF_HoldQueueLength_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CF_HoldQueueLength);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CF_HoldQueueLength, value);
            }
        }

        //variable-length optional field.
        public string AH_DueDate_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AH_DueDate);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AH_DueDate, value);
            }
        }

        //18-char, fixed-length optional field:  YYYYMMDDZZZZHHMMSS
        public string CJ_RecallDate_18
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CJ_RecallDate);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CJ_RecallDate, value);
            }
        }


        //18-char, fixed-length optional field:  YYYYMMDDZZZZHHMMSS
        public string CM_HoldPickupDate_18
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CM_HoldPickupDate);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CM_HoldPickupDate, value);
            }
        }


        //variable-length required field
        public string AB_ItemIdentifier_r
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

        //variable-length required field
        public string AJ_TitleIdentifier_r
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
        public string BG_Owner_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BG_Owner);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BG_Owner, value);
            }
        }

        //3 char, fixed-length optional field
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


        //variable-length optional field.  The amount of the fee associated with this item.
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


        //3-char, fixed-length optional field
        public string CK_MediaType_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CK_MediaType);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CK_MediaType, value);
            }
        }

        //variable-length optional field
        public string AQ_PermanentLocation_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AQ_PermanentLocation);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AQ_PermanentLocation, value);
            }
        }

        //variable-length optional field
        public string AP_CurrentLocation_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AP_CurrentLocation);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AP_CurrentLocation, value);
            }
        }

        //variable-length optional field
        public string CH_ItemProperties_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CH_ItemProperties);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CH_ItemProperties, value);
            }
        }

        // 2020/12/8
        public string KC_CallNo_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_KC_CallNo);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_KC_CallNo, value);
            }
        }

        // 2020/12/8
        // variable-length optional field
        public string KQ_PermanentShelfNo_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_KQ_PermanentShelfNo);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_KQ_PermanentShelfNo, value);
            }
        }

        // 2020/12/8
        public string KP_CurrentShelfNo_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_KP_CurrentShelfNo);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_KP_CurrentShelfNo, value);
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
