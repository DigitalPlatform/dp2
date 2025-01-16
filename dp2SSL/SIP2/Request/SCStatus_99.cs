using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    SC Status
    The SC status message sends SC status to the ACS.  It requires an ACS Status Response message reply from the ACS. This message will be the first message sent by the SC to the ACS once a connection has been established (exception: the Login Message may be sent first to login to an ACS server program). The ACS will respond with a message that establishes some of the rules to be followed by the SC and establishes some parameters needed for further communication.
    99<status code><max print width><protocol version>
    99	1-char	3-char	4-char
     */
    public class SCStatus_99 : BaseMessage
    {
        // 构造函数
        public SCStatus_99()
        {
            this.CommandIdentifier = "99";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<status code><max print width><protocol version>
            //1-char	3-char	4-char
            FixedLengthFields.Add(new FixedLengthField(SIPConst.F_StatusCode,1));// 1-char, fixed-length required field: 0 or 1 or 2
            FixedLengthFields.Add(new FixedLengthField(SIPConst.F_MaxPrintWidth, 3));// 3-char, fixed-length required field
            FixedLengthFields.Add(new FixedLengthField(SIPConst.F_ProtocolVersion, 4));// 4-char, fixed-length required field:  x.xx

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        //status code 1-char, fixed-length required field: 0 or 1 or 2
        public string StatusCode_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_StatusCode);
            }
            set
            {
                if (value != "0" && value != "1" && value != "2")
                    throw new Exception("status code参数不合法，必须为0 or 1 or 2。");

                this.SetFixedFieldValue(SIPConst.F_StatusCode, value);
            }
        }

        //max print width 3-char, fixed-length required field
        public string MaxPrintWidth_3
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_MaxPrintWidth);
            }
            set
            {
                if (value.Length != 3)
                    throw new Exception("max print width参数长度须为3位。");

                this.SetFixedFieldValue(SIPConst.F_MaxPrintWidth, value);
            }
        }

        //protocol version 4-char, fixed-length required field:  x.xx
        public string ProtocolVersion_4
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_ProtocolVersion);
            }
            set
            {
                if (value.Length != 4)
                    throw new Exception("protocol version参数长度须为4位。");

                this.SetFixedFieldValue(SIPConst.F_ProtocolVersion, value);
            }
        }

    }
}
