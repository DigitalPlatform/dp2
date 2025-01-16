using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*<summary>
     * 2.00 Login Response
     * The ACS should send this message in response to the Login message.When this message is used, it will be the first message sent to the SC.
     * 94<ok>
     */
    public class LoginResponse_94 : BaseMessage
    {
        public LoginResponse_94()
        {
            this.CommandIdentifier = "94";

#if REMOVED
            //==前面的定长字段
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Ok, 1));

            // 2021/3/4
            // 标准里面没有的两个字段，方便观察错误信息
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false, true)); //重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false, true)); //重复字段

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
    }
}
