using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{

    // 通道信息响应消息
    public class ChannelInformationResponse_42 : BaseMessage
    {
        public ChannelInformationResponse_42()
        {
            this.CommandIdentifier = "42";

#if REMOVED
            //==前面的定长字段
            //42 <status><transaction date>
            //1 - char      18 - char 
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Ok, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));


            //==后面变长字段
            //<total count><return count><channel value><screen message><print line>
            //ZT    ZR  ZV      AF AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_ZT_TotalCount,true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_ZR_ReturnCount, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_ZV_Value, true));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage,false, true)); 
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false ,true));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }
        
        //1-char, fixed-length required field
        public string Status_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Ok);
            }
            set
            {
                if (value.Length != 1)
                    throw new Exception("status参数长度须为1位。");

                this.SetFixedFieldValue(SIPConst.F_Ok, value);
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

        public string ZT_TotalCount_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_ZT_TotalCount);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_ZT_TotalCount, value);
            }
        }

        public string ZR_ReturnCount_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_ZR_ReturnCount);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_ZR_ReturnCount, value);
            }
        }

        public string ZV_Value_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_ZV_Value);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_ZV_Value, value);
            }
        }


    }
}
