using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Renew All Response
    The ACS should send this message in response to a Renew All message from the SC.
    66<ok ><renewed count><unrenewed count><transaction date><institution id><renewed items><unrenewed items><screen message><print line>
    66	1-char	4-char	4-char	18-char	AO	BM	BN	AF	AG
     */
    public class RenewAllResponse_66 : BaseMessage
    {
        public RenewAllResponse_66()
        {
            this.CommandIdentifier = "66";

#if REMOVED
            //==前面的定长字段
            //<ok ><renewed count><unrenewed count><transaction date>
            //1-char	4-char	4-char	18-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Ok, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_RenewedCount, 4));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_UnrenewedCount, 4));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate,18));

            //==后面变长字段
            //<institution id><renewed items><unrenewed items><screen message><print line>
            //AO	BM	BN	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BM_RenewedItems, false,true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BN_UnrenewedItems, false,true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false,true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false,true));

            // 校验码相关
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        //1-char, fixed-length required field:  0 or 1
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

        //4-char fixed-length required field
        public string RenewedCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_RenewedCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("renewed count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_RenewedCount, value);
            }
        }

        //4-char fixed-length required field
        public string UnrenewedCount_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_UnrenewedCount);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("unrenewed count参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_UnrenewedCount, value);
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

        //variable-length optional field  (this field sent for each renewed item)
        public List<VariableLengthField> BM_RenewedItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_BM_RenewedItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_BM_RenewedItems, value);
            }
        }

        //variable-length optional field  (this field sent for each unrenewed item)
        public List<VariableLengthField> BN_UnrenewedItems_o
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_BN_UnrenewedItems);
            }
            set
            {
                this.SetVariableFieldList(SIPConst.F_BN_UnrenewedItems, value);
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
