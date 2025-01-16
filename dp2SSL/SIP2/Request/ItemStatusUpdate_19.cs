using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
     2.00 Item Status Update
     This message can be used to send item information to the ACS, without having to do a Checkout or Checkin operation.  The item properties could be stored on the ACS’s database.  The ACS should respond with an Item Status Update Response message.
     19<transaction date><institution id><item identifier><terminal password><item properties>
     19	18-char	AO	AB	AC	CH
     */
    public class ItemStatusUpdate_19 : BaseMessage
    {
        public ItemStatusUpdate_19()
        {
            this.CommandIdentifier = "19";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<transaction date>
            //18-char	
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段
            //<institution id><item identifier><terminal password><item properties>
            //	AO	AB	AC	CH
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AO_InstitutionId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AB_ItemIdentifier, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AC_TerminalPassword, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CH_ItemProperties, true));


            // 2020/12/9 dp2扩展字段
            //永久馆藏地
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AQ_PermanentLocation, false));
            //当前馆藏地
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AP_CurrentLocation, false));
            //永久架位号
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_KQ_PermanentShelfNo, false));
            //当前架位号
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_KP_CurrentShelfNo, false));
            //册状态,可选	0丢失 1编目 2在馆 ，dp2扩展字段。
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_HS_HoldingState, false));
#endif

        }


        // 18-char, fixed-length required field:  YYYYMMDDZZZZHHMMSS
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

        //  variable-length required field
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

        //  variable-length optional field
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

        //  variable-length required field
        public string CH_ItemProperties_r
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

        // 2020/12/9，dp2扩展字段
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

        // 2020/12/9，dp2扩展字段
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


        // 2020/12/9，dp2扩展字段
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

        // 2020/12/9，dp2扩展字段
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

        // 2020/12/9，dp2扩展字段
        public string HS_HoldingState_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_HS_HoldingState);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_HS_HoldingState, value);
            }
        }

    }
}
