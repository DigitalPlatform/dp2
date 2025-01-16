using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
     Checkin Response
     This message must be sent by the ACS in response to a SC Checkin message.
     10<ok><resensitize><magnetic media><alert><transaction date><institution id><item identifier><permanent location><title identifier><sort bin><patron identifier><media type><item properties><screen message><print line>
     10	1-char	1-char	1-char 1-char	18-char	AO	AB	AQ	AJ	CL AA	CK	CH	AF	AG
    */
    public class CheckinResponse_10 : BaseMessage
    {
        public CheckinResponse_10()
        {
            this.CommandIdentifier = "10";

#if REMOVED
            //==前面的定长字段 
            //<ok><resensitize><magnetic media><alert><transaction date><institution id><item identifier><permanent location><title identifier><sort bin><patron identifier><media type><item properties><screen message><print line>
            //1-char	1-char	1-char	1-char 18-char
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Ok, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Resensitize, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_MagneticMedia, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_Alert, 1));
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><item identifier><permanent location><title identifier><sort bin><patron identifier><media type><item properties><screen message><print line>
            //AO	AB	AQ	---AJ CL AA	---	CK CH	AF	AG
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AQ_PermanentLocation, true));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AJ_TitleIdentifier, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CL_SortBin, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AA_PatronIdentifier, false ));

            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CK_MediaType, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AF_ScreenMessage, false, true)); //重复字段
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AG_PrintLine, false, true)); //重复字段

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
#endif
        }
        
        //OK should be set to 1 if the ACS checked in the item. should be set to 0 if the ACS did not check in the item.
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

        //Resensitize should be set to Y if the SC should resensitize the article. should be set to N if the SC should not resensitize the article (for example, a closed reserve book, or the checkin was refused).
        //1-char, fixed-length required field:  Y or N.
        public string Resensitize_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Resensitize);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("resensitize参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_Resensitize, value);
            }
        }

        //1-char, fixed-length required field:  Y or N or U.
        public string MagneticMedia_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_MagneticMedia);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("magnetic media参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_MagneticMedia, value);
            }
        }

        //1-char, fixed-length required field:  Y or N.
        public string Alert_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_Alert);
            }
            set
            {
                if (value != "Y" && value != "N")
                    throw new Exception("alert参数不合法，必须为Y/N。");

                this.SetFixedFieldValue(SIPConst.F_Alert, value);
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
        public string AQ_PermanentLocation_r
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
        public string CL_SortBin_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CL_SortBin);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CL_SortBin, value);
            }
        }

        //variable-length optional field.  ID of the patron who had the item checked out.
        public string AA_PatronIdentifier_o
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

        //3-char, fixed-length optional field
        public string CK_MediaType_e
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
