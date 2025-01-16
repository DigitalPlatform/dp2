using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    2.00 Patron Information
    This message is a superset of the Patron Status Request message.  It should be used to request patron information.  The ACS should respond with the Patron Information Response message.
    63<language><transaction date><summary><institution id><patron identifier><terminal password><patron password><start item><end item>
   63	3-char	18-char	10-char	AO	AA	AC	AD	BP  BQ
     * */
    public class ChannelInformation_41 : BaseMessage
    {

        public ChannelInformation_41()
        {
            this.CommandIdentifier = "41";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            this.FixedLengthFields.Add(new FixedLengthField(SIPConst.F_TransactionDate, 18));

            //==后面变长字段ZW		BP	ZC	ZF
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_ZW_SearchWord, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_BP_StartItem, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_ZC_MaxCount, false ));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_ZF_format, false ));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber, false));
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
        public string ZW_SearchWord_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_ZW_SearchWord);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_ZW_SearchWord, value);
            }
        }

        // variable-length required field
        public string BP_StartItem_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_BP_StartItem);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_BP_StartItem, value);
            }
        }

        // variable-length required field
        public string ZC_MaxCount_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_ZC_MaxCount);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_ZC_MaxCount, value);
            }
        }

        // variable-length required field
        public string ZF_format_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_ZF_format);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_ZF_format, value);
            }
        }

    }
}
