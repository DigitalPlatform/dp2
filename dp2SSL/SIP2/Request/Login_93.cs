using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dp2SSL.SIP2
{
    /*
    Login
    This message can be used to login to an ACS server program.  
    The ACS should respond with the Login Response message.  
    Whether to use this message or to use some other mechanism to login to the ACS is configurable on the SC.  When this message is used, it will be the first message sent to the ACS.
    93<UID algorithm><PWD algorithm><login user id><login password><location code>
    93	1-char	1-char	CN	CO	CP
     */
    public class Login_93 : BaseMessage
    {
        // 构造函数
        public Login_93()
        {
            this.CommandIdentifier = "93";

            this.SetDefaultValue();
#if REMOVED
            //==前面的定长字段
            //<UID algorithm><PWD algorithm>
            //1-char	1-char
            FixedLengthFields.Add(new FixedLengthField(SIPConst.F_UIDAlgorithm, 1));
            FixedLengthFields.Add(new FixedLengthField(SIPConst.F_PWDAlgorithm, 1));

            // <login user id><login password><location code>
            // CN   	CO	CP
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CN_LoginUserId, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CO_LoginPassword, true));
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_CP_LocationCode, false));

            // 校验码相关，todo
            this.VariableLengthFields.Add(new VariableLengthField(SIPConst.F_AY_SequenceNumber,false));
#endif
        }

        public override void SetDefaultValue()
        {
            base.SetDefaultValue();

            // https://github.com/DigitalPlatform/dp2/issues/756#issuecomment-733002892
            UIDAlgorithm_1 = "0"; // 0 表示不加密
            PWDAlgorithm_1 = "0";// 0 表示不加密
        }

       
        //2.00 UID algorithm 1-char, fixed-length required field; the algorithm used to encrypt the user id.
        public string UIDAlgorithm_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_UIDAlgorithm);
            }
            set
            {
                if (value.Length != 1)
                    throw new Exception("UID algorithm参数长度须为1位。");

                this.SetFixedFieldValue(SIPConst.F_UIDAlgorithm, value);
            }
        }

        //2.00 PWD algorithm 1-char, fixed-length required field; the algorithm used to encrypt the password.
        public string PWDAlgorithm_1
        {
            get
            {
                return this.GetFixedFieldValue(SIPConst.F_PWDAlgorithm);
            }
            set
            {
                if (value.Length != 1)
                    throw new Exception("PWD algorithm参数长度须为1位。");

                this.SetFixedFieldValue(SIPConst.F_PWDAlgorithm, value);
            }
        }

        //2.00 login user id CN variable-length required field
        public string CN_LoginUserId_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CN_LoginUserId);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CN_LoginUserId, value);
            }
        }
        //2.00 login password CO variable-length required field
        public string CO_LoginPassword_r
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CO_LoginPassword);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CO_LoginPassword, value);
            }
        }
        //2.00 location code CP variable-length optional field; the SC location.
        public string CP_LocationCode_o
        {
            get
            {
                return this.GetVariableFieldValue(SIPConst.F_CP_LocationCode);
            }
            set
            {
                this.SetVariableFieldValue(SIPConst.F_CP_LocationCode, value);
            }
        }
         

        /*
        public Login_93(string p_UIDAlgorithm_1
            , string p_PWDAlgorithm_1
            , string p_loginUserId_CN_r
            , string p_loginPassword_CO_r
            , string p_locationCode_CP_o)
        {
            if (p_UIDAlgorithm_1.Length != 1)
                throw new Exception("UIDAlgorithm长度必须是1位");
            this.UIDAlgorithm_1 = p_UIDAlgorithm_1;

            if (p_PWDAlgorithm_1.Length != 1)
                throw new Exception("PWDAlgorithm长度必须是1位");
            this.PWDAlgorithm_1 = p_PWDAlgorithm_1;

            if (p_loginUserId_CN_r == null)
                throw new Exception("loginUserId_CN不能为null");
            this.LoginUserId_CN_r = p_loginUserId_CN_r;

            if (p_loginPassword_CO_r == null)
                throw new Exception("loginPassword_CO不能为null");
            this.LoginPassword_CO_r = p_loginPassword_CO_r;

            this.LocationCode_CP_o = p_locationCode_CP_o;
        }
         */

        /*
        // 解析字符串命令为对象
        public override bool parse(string text, out string error)
        {
            error = "";

            if (text == null || text.Length < 2)
            {
                error = "命令字符串为null或长度小于2位";
                return false;
            }
            string cmdIdentifiers = text.Substring(0, 2);
            text = text.Substring(2);

            //处理定长字段
            string rest = text;
            while (rest.Length > 0)
            {
                if (String.IsNullOrEmpty(this.UIDAlgorithm_1) == true)
                {
                    this.UIDAlgorithm_1 = rest.Substring(0, 1);
                    rest = rest.Substring(1);
                    continue;
                }

                if (String.IsNullOrEmpty(this.PWDAlgorithm_1) == true)
                {
                    this.PWDAlgorithm_1 = rest.Substring(0, 1);
                    rest = rest.Substring(1);
                    continue;
                }
                break;
            }


            //处理变长字段
            string[] parts = rest.Split(new char[] { '|' });
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                if (part.Length < 2)
                {
                    continue;
                    //error = "发现不足2位的字段:" + part;
                    //goto ERROR1;
                }

                string fieldId = part.Substring(0, 2);
                string value = part.Substring(2);
                if (fieldId == "CN")
                {
                    this.LoginUserId_CN_r = value;
                }
                else if (fieldId == "CO")
                {
                    this.LoginPassword_CO_r = value;
                }
                else if (fieldId == "CP")
                {
                    this.LocationCode_CP_o = value;
                }
                //else if (fieldId == "AY")
                //{
                //    this._AYAZ = part;
                //}
                else
                {
                    error = "不支持的字段:" + part;
                    return false;
                }
            }

            // 校验;
            bool ret = this.Verify(out error);
            if (ret == false)
                return false;


            return true;

        }
         */
        
        /*
        // 校验对象的各参数是否合法
        public override bool Verify(out string error)
        {
            error = "";
            if (this.UIDAlgorithm_1 == "")
            {
                error = "UIDAlgorithm字段未赋值";
                goto ERROR1;
            }
            if (this.PWDAlgorithm_1 == "")
            {
                error = "PWDAlgorithm字段未赋值";
                goto ERROR1;
            }

            if (this.LoginUserId_CN_r == null)
            {
                error = "缺必备字段CN";
                goto ERROR1;
            }
            if (this.LoginPassword_CO_r == null)
            {
                error = "缺必备字段CO";
                goto ERROR1;
            }
            return true;

        ERROR1:

            return false;
        }

        // 将对象转换字符串命令
        public override string ToText()
        {
            string text = "93";

            text += this.UIDAlgorithm_1;
            text += this.PWDAlgorithm_1;

            if (this.LoginUserId_CN_r != null)
                text += "CN" + this.LoginUserId_CN_r + "|";

            if (this.LoginPassword_CO_r != null)
                text += "CO" + this.LoginPassword_CO_r + "|";

            if (this.LocationCode_CP_o != null)
                text += "CP" + this.LocationCode_CP_o + "|";


            return text;
        }
        */
    }
}
