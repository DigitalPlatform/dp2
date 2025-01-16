using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{


    /*
     * ACS Status
     * The ACS must send this message in response to a SC Status message.This message will be the first message sent by the ACS to the SC, since it establishes some of the rules to be followed by the SC and establishes some parameters needed for further communication (exception: the Login Response Message may be sent first to complete login of the SC).
     * 98<on-line status><checkin ok><checkout ok><ACS renewal policy><status update ok><off-line ok><timeout period><retries allowed><date / time sync><protocol version><institution id><library name><supported messages ><terminal location><screen message><print line>
     98	1-char	1-char	1-char	1-char	1-char	1-char	3-char	3-char  18-char	4-char	AO	AM	BX	AN	AF	AG
     */
    public class ACSStatus_98 : BaseMessage
    {

        public ACSStatus_98()
        {
            this.CommandIdentifier = "98";

#if REMOVED
            //==前面的定长字段
            //98<on-line status><checkin ok><checkout ok><ACS renewal policy><status update ok><off-line ok>
            //98	1-char	1-char	1-char	---1-char	1-char	1-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_OnlineStatus, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_CheckinOk, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_CheckoutOk, 1));

            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_ACSRenewalPolicy, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_StatusUpdateOk, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_OfflineOk, 1));

            //<timeout period><retries allowed><date / time sync><protocol version>
            //3-char	3-char  18-char	4-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TimeoutPeriod, 3));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_RetriesAllowed, 3));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_DatetimeSync, 18));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_ProtocolVersion, 4));

            //==后面变长字段
            //<institution id><library name><supported messages ><terminal location><screen message><print line>
            //AO	AM	BX	AN	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AM_LibraryName, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BX_SupportedMessages, false)); //2020/8/14 改为false,发现SIP2 Developers Guide.pdf里的例子没有BX，但在SIP2.0.pdf里指明是必备字段

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AN_TerminalLocation, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false, true)); //重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine,false , true)); //重复字段

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }

        
        // 1-char, fixed-length required field:  Y or N.
        public string OnlineStatus_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_OnlineStatus);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("online status参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_OnlineStatus, value);
            }
        }

        //1-char, fixed-length required field:  Y or N.
        public string CheckinOk_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_CheckinOk);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("checkin ok参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_CheckinOk, value);
            }
        }


        //1-char, fixed-length required field:  Y or N.
        public string CheckoutOk_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_CheckoutOk);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("checkout ok参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_CheckoutOk, value);
            }
        }

        //1-char, fixed-length required field:  Y or N.
        public string ACSRenewalPolicy_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_ACSRenewalPolicy);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("ACS renewal policy参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_ACSRenewalPolicy, value);
            }
        }

        //===

        //1-char, fixed-length required field:  Y or N.
        public string StatusUpdateOk_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_StatusUpdateOk);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("status update ok参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_StatusUpdateOk, value);
            }
        }

        //1-char, fixed-length required field:  Y or N.
        public string OfflineOk_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_OfflineOk);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("offline ok参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_OfflineOk, value);
            }
        }

        //3-char, fixed-length required field
        public string TimeoutPeriod_3
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_TimeoutPeriod);
            }
            set
            {
                if (value.Length != 3)
                    throw new Exception("timeout period参数长度须为3位。");

                this.SetFixedFieldValue(SIPConst.F_TimeoutPeriod, value);
            }
        }


        //3-char, fixed-length required field
        public string RetriesAllowed_3
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_RetriesAllowed);
            }
            set
            {
                if (value.Length != 3)
                    throw new Exception("retries allowed参数长度须为3位。");

                this.SetFixedFieldValue(SIPConst.F_RetriesAllowed, value);
            }
        }

        //===
        //18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS
        public string DatetimeSync_18
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_DatetimeSync);
            }
            set
            {
                if (value.Length != 18)
                    throw new Exception("date/time sync参数长度须为18位。");

                this.SetFixedFieldValue(SIPConst.F_DatetimeSync, value);
            }
        }

        //4-char, fixed-length required field:  x.xx
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

        //variable-length optional field
        public string AM_LibraryName_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AM_LibraryName);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AM_LibraryName, value);
            }
        }

        //variable-length required field
        public string BX_SupportedMessages_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BX_SupportedMessages);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BX_SupportedMessages, value);
            }
        }

        //variable-length optional field
        public string AN_TerminalLocation_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_AN_TerminalLocation);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_AN_TerminalLocation, value);
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
