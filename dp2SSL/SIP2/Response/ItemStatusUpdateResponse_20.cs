using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Item Status Update Response
    The ACS must send this message in response to the Item Status Update message.
    20<item properties ok><transaction date><item identifier><title identifier><item properties><screen message><print line>
    20	1-char	18-char	AB	AJ	CH	AF	AG
     */
    public class ItemStatusUpdateResponse_20 : BaseMessage
    {
        public ItemStatusUpdateResponse_20()
        {
            this.CommandIdentifier = "20";

#if REMOVED
            //==前面的定长字段
            //<item properties ok><transaction date>
            //1-char	18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_ItemPropertiesOk, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<item identifier><title identifier><item properties><screen message><print line>
            //AB	AJ	CH	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false,true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false,true));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        //1-char, fixed-length required field:  0 or 1.
        public string ItemPropertiesOk_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_ItemPropertiesOk);
            }
            set
            {
                if (value != "0" && value != "1")
                    throw new Exception("item properties ok参数不合法，必须为0 or 1。");

                this.SetFixedFieldValue(SIPConst.F_ItemPropertiesOk, value);
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
