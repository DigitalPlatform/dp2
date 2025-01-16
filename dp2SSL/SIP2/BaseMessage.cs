using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.Text;

namespace dp2SSL.SIP2
{
    public class BaseMessage
    {
        string _id = "";

        // 命令指示符
        public string CommandIdentifier
        {
            get { return _id; }
            set
            {
                _id = value;
                _rule = GetMessageRule();
            }
        }

#if REMOVED
        //The sequence number is a single ASCII digit, '0' to '9'.  
        //When error detection is enabled, the SC will increment the sequence number field for each new message it transmits. 
        //The ACS should verify that the sequence numbers increment as new messages are received from the 3M SelfCheck system.  
        //When error detection is enabled, the ACS response to a message should include a sequence number field also, where the sequence number field’s value matches the sequence number value from the message being responded to.
        private string _sequenceNumber_AY { get; set; }

        //The checksum is four ASCII character digits representing the binary sum of the characters including the first character of the transmission and up to and including the checksum field identifier characters.
        //To calculate the checksum add each character as an unsigned binary number, take the lower 16 bits of the total and perform a 2's complement.  The checksum field is the result represented by four hex digits.
        //To verify the correct checksum on received data, simply add all the hex values including the checksum.  It should equal zero.
        // 4位16进制
        private string _checksum_AZ { get; set; }
#endif

        #region 消息静态定义

        // 所有已知消息定义
        static string _message_defs =
@"---01---
##_CardRetained fix:1
##_TransactionDate fix:18
AO_InstitutionId var:r1
AL_BlockedCardMsg var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:r1
---09---
            //==前面的定长字段
            //09<no block><transaction date><return date>
            //09	1-char	18-char	18-char
##_NoBlock fix:1
##_TransactionDate fix:18
##_ReturnDate fix:18
            //==后面变长字段
            //<current location><institution id><item identifier><terminal password><item properties><cancel>
            //AP	AO	AB	AC	CH	BI
AP_CurrentLocation var:r1
AO_InstitutionId var:r1
AB_ItemIdentifier var:r1
AC_TerminalPassword var:r1
CH_ItemProperties var:o1
BI_Cancel var:o1
---10---
            //==前面的定长字段 
            //<ok><resensitize><magnetic media><alert><transaction date><institution id><item identifier><permanent location><title identifier><sort bin><patron identifier><media type><item properties><screen message><print line>
            //1-char	1-char	1-char	1-char 18-char
##_Ok fix:1
##_Resensitize fix:1
##_MagneticMedia fix:1
##_Alert fix:1
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><item identifier><permanent location><title identifier><sort bin><patron identifier><media type><item properties><screen message><print line>
            //AO	AB	AQ	---AJ CL AA	---	CK CH	AF	AG
AO_InstitutionId var:r1
AB_ItemIdentifier var:r1
AQ_PermanentLocation var:r1
AJ_TitleIdentifier var:o1
CL_SortBin var:o1
AA_PatronIdentifier var:o1
CK_MediaType var:o1
CH_ItemProperties var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on


---11---
            //==前面的定长字段
            //<SC renewal policy><no block><transaction date><nb due date>
            //1-char	1-char	18-char	18-char
##_SCRenewalPolicy fix:1
##_NoBlock fix:1
##_TransactionDate fix:18
##_NbDueDate fix:18

            //==后面变长字段 
            //<institution id><patron identifier><item identifier><terminal password><patron password><item properties><fee acknowledged><cancel>
            //AO	AA	AB	AC      AD	CH	BO	BI
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AB_ItemIdentifier var:r1
AC_TerminalPassword var:r1
AD_PatronPassword var:o1            
CH_ItemProperties var:o1
BO_FeeAcknowledged var:o1
BI_Cancel var:o1
---12---
            //==前面的定长字段
            //<ok><renewal ok><magnetic media><desensitize><transaction date>
            //1-char	1-char	1-char	1-char	18-char
##_Ok fix:1
##_RenewalOk fix:1
##_MagneticMedia fix:1
##_Desensitize fix:1
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><item identifier><title identifier><due date><fee type><security inhibit><currency type><fee amount><media type><item properties><transaction id><screen message><print line>
            //	AO	AA	AB	---AJ	    AH  BT	---CI	BH	BV	---CK	CH	BK	AF	AG
AO_InstitutionId, true));
AA_PatronIdentifier, true));
AB_ItemIdentifier, true));

AJ_TitleIdentifier var:o1
AH_DueDate var:o1
BT_FeeType var:o1

CI_SecurityInhibit var:o1
BH_CurrencyType var:o1
BV_FeeAmount var:o1

CK_MediaType var:o1
CH_ItemProperties var:o1
BK_TransactionId var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on

---15---
            //==前面的定长字段
            //<hold mode><transaction date>
            //1-char	18-char
##_HoldMode fix:1
##_TransactionDate fix:18
            //==后面变长字段
            //<expiration date><pickup location><hold type><institution id><patron identifier><patron password><item identifier><title identifier><terminal password><fee acknowledged>
            //BW	BS	BY	 ---AO	AA	AD	---AB AJ	AC	BO
BW_ExpirationDate var:o1
BS_PickupLocation var:o1
BY_HoldType var:o1
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AD_PatronPassword var:o1
AB_ItemIdentifier var:o1
AJ_TitleIdentifier var:o1
AC_TerminalPassword var:o1
BO_FeeAcknowledged var:o1
---16---
            //==前面的定长字段
            //<ok><available><transaction date>
            //1-char	1-char	18-char
##_Ok fix:1
##_Available fix:1
##_TransactionDate fix:18

            //==后面变长字段
            //<expiration date><queue position><pickup location><institution id><patron identifier><item identifier><title identifier><screen message><print line>
            //BW	BR	BS	AO ---	AA	AB AJ	AF	AG
BW_ExpirationDate var:o1
BR_QueuePosition var:o1
BS_PickupLocation var:o1
AO_InstitutionId var:r1

AA_PatronIdentifier var:r1
AB_ItemIdentifier var:o1
AJ_TitleIdentifier var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on


---17---
            //==前面的定长字段
##_TransactionDate fix:18
            //==后面变长字段
AO_InstitutionId var:r1
AB_ItemIdentifier var:r1
AC_TerminalPassword var:o1
---18---
            //==前面的定长字段
            //<circulation status><security marker><fee type><transaction date>
            //2-char	2-char	2-char	18-char
##_CirculationStatus fix:2
##_SecurityMarker fix:2
##_BT_FeeType fix:2
##_TransactionDate fix:18
            //==后面变长字段
            //<hold queue length><due date><recall date><hold pickup date>
            //CF	AF	CJ	CM
CF_HoldQueueLength var:o1
AH_DueDate var:o1
CJ_RecallDate var:o1
CM_HoldPickupDate var:o1

            //<item identifier><title identifier><owner><currency type>
            //AB AJ	BG	BH	
AB_ItemIdentifier, true));
AJ_TitleIdentifier, true));
BG_Owner var:o1
BH_CurrencyType var:o1

            //<fee amount><media type><permanent location><current location>
            //BV	CK	AQ	AP	
BV_FeeAmount var:o1
CK_MediaType var:o1
AQ_PermanentLocation var:o1
AP_CurrentLocation var:o1

            //<item properties><screen message><print line>
            //CH	AF	AG
CH_ItemProperties var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on

            // 2020/12/8
KC_CallNo var:o1
KP_CurrentShelfNo var:o1
KQ_PermanentShelfNo var:o1

---19---
            //==前面的定长字段
            //<transaction date>
            //18-char	
##_TransactionDate fix:18
            //==后面变长字段
            //<institution id><item identifier><terminal password><item properties>
            //	AO	AB	AC	CH
AO_InstitutionId var:r1
AB_ItemIdentifier var:r1
AC_TerminalPassword var:o1
CH_ItemProperties var:r1
            // 2020/12/9 dp2扩展字段
            //永久馆藏地
AQ_PermanentLocation var:o1
            //当前馆藏地
AP_CurrentLocation var:o1
            //永久架位号
KQ_PermanentShelfNo var:o1
            //当前架位号
KP_CurrentShelfNo var:o1
            //册状态,可选	0丢失 1编目 2在馆 ，dp2扩展字段。
HS_HoldingState var:o1
---20---
            //==前面的定长字段
            //<item properties ok><transaction date>
            //1-char	18-char
##_ItemPropertiesOk fix:1
##_TransactionDate fix:18

            //==后面变长字段
            //<item identifier><title identifier><item properties><screen message><print line>
            //AB	AJ	CH	AF	AG
AB_ItemIdentifier var:r1
AJ_TitleIdentifier var:o1
CH_ItemProperties var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on

---23---
            //==前面的定长字段
            //<language><transaction date>
            //3-char	18-char	
##_Language fix:3
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><terminal password><patron password>
            //AO	AA	AC	AD
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:r1
AD_PatronPassword var:r1
---24---
            //==前面的定长字段
            //<patron status><language><transaction date>
            //14-char	3-char	18-char
##_PatronStatus fix:14
##_Language fix:3
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><personal name><valid patron><valid patron password><currency type><fee amount><screen message><print line>
            //AO	AA	AE	    BL --- CQ  BH	BV	AF	AG
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AE_PersonalName var:r1
BL_ValidPatron var:o1

CQ_ValidPatronPassword var:o1
BH_CurrencyType var:o1
BV_FeeAmount var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on

---25---
            //==前面的定长字段
            //<transaction date>
            // 18-char
##_TransactionDate fix:18
            //==后面变长字段
            //<institution id><patron identifier><terminal password><patron password>
            //AO	AA	AC	AD
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:o1
AD_PatronPassword var:o1
---26---
            //==前面的定长字段
            //<patron status><language><transaction date>
            //14-char	3-char	18-char
##_PatronStatus fix:14
##_Language fix:3
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><personal name><valid patron><valid patron password><screen message><print line>
            //AO	AA	AE  ---	BL  CQ  AF	AG
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AE_PersonalName var:r1

BL_ValidPatron var:o1
CQ_ValidPatronPassword var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on
---29---
            //==前面的定长字段
            //<third party allowed><no block><transaction date><nb due date>
            //1-char	1-char	18-char	18-char
##_ThirdPartyAllowed fix:1
##_NoBlock fix:1
##_TransactionDate fix:18
##_NbDueDate fix:18

            //==后面变长字段 
            //<institution id><patron identifier><patron password><item identifier><title identifier><terminal password><item properties><fee acknowledged>
            //AO	AA	AD	AB AJ	AC	CH	BO
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AD_PatronPassword var:o1
AB_ItemIdentifier var:o1

AJ_TitleIdentifier var:o1
AC_TerminalPassword var:o1
CH_ItemProperties var:o1
BO_FeeAcknowledged var:o1
---30---
            //==前面的定长字段
            //30<ok><renewal ok><magnetic media><desensitize><transaction date>
            //30	1-char	1-char	1-char	1-char	18-char
##_Ok fix:1
##_RenewalOk fix:1
##_MagneticMedia fix:1
##_Desensitize fix:1
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><item identifier><title identifier><due date><fee type>
            //AO	AA	AB	AJ  AH  BT
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AB_ItemIdentifier var:r1
AJ_TitleIdentifier var:r1
AH_DueDate var:r1
BT_FeeType var:o1

            //<security inhibit><currency type><fee amount><media type><item properties><transaction id><screen message><print line>
            //CI	BH	BV	CK ---	CH	BK	AF	AG
CI_SecurityInhibit var:o1
BH_CurrencyType var:o1
BV_FeeAmount var:o1
CK_MediaType var:o1

CH_ItemProperties var:o1
BK_TransactionId var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on



---35---
            //==前面的定长字段
            //<transaction date>
            //18-char	
##_TransactionDate fix:18
            //==后面变长字段
            //<institution id><patron identifier><terminal password><patron password>
            //AO	AA	AC	AD
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:o1
AD_PatronPassword var:o1
---36---
            //==前面的定长字段
            //<end session>< transaction date >
            //1-char	18-char
##_EndSession fix:1
##_TransactionDate fix:18
            //==后面变长字段
            //< institution id >< patron identifier ><screen message><print line>
            //AO	AA	AF	AG
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AF_ScreenMessage var:on
AG_PrintLine var:on

---37---
            //==前面的定长字段
            //<transaction date><fee type><payment type><currency type>
            //18-char	2-char	2-char	3-char
##_TransactionDate fix:18
##_BT_FeeType fix:2
##_PaymentType fix:2
##_BH_CurrencyType fix:3
            //==后面变长字段
            //<fee amount><institution id><patron identifier><terminal password><patron password><fee identifier><transaction id>
            //BV	AO	AA	AC	AD  CG	BK
BV_FeeAmount var:r1
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:o1
AD_PatronPassword var:o1
CG_FeeIdentifier var:o1
BK_TransactionId var:o1
---38---
            //==前面的定长字段
            //<payment accepted><transaction date>
            //	1-char	18-char
##_PaymentAccepted fix:1
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><transaction id><screen message><print line>
            //AO	AA	BK	AF	AG
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
BK_TransactionId var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on


---41---
##_TransactionDate fix:18
ZW_SearchWord var:r1
BP_StartItem var:r1
ZC_MaxCount var:o1
ZF_format var:o1
---42---
            //==前面的定长字段
            //42 <status><transaction date>
            //1 - char      18 - char 
##_Ok fix:1
##_TransactionDate fix:18
            //==后面变长字段
            //<total count><return count><channel value><screen message><print line>
            //ZT    ZR  ZV      AF AG
ZT_TotalCount var:r1
ZR_ReturnCount var:r1
ZV_Value var:r1
AF_ScreenMessage var:on 
AG_PrintLine var:on

---63---
            //==前面的定长字段
##_Language fix:3
##_TransactionDate fix:18
##_Summary fix:10

            //==后面变长字段 AO	AA	AC	AD	BP  BQ
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:o1

AD_PatronPassword var:o1
BP_StartItem var:o1
BQ_EndItem var:o1
---64---
            //==前面的定长字段
            //<patron status><language><transaction date><hold items count><overdue items count><charged items count><fine items count><recall items count><unavailable holds count>
            //14-char	3-char	18-char	---4-char	4-char  4-char	---4-char  4-char	4-char	
##_PatronStatus fix:14
##_Language fix:3
##_TransactionDate fix:18

##_HoldItemsCount fix:4
##_OverdueItemsCount fix:4
##_ChargedItemsCount fix:4

            //<fine items count><recall items count><unavailable holds count>
##_FineItemsCount fix:4
##_RecallItemsCount fix:4
##_UnavailableHoldsCount fix:4

            //==后面变长字段
             //<institution id><patron identifier><personal name><hold items limit><overdue items limit>
             //AO	AA	AE	    BZ	CA
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AE_PersonalName var:r1
BZ_HoldItemsLimit var:o1
CA_OverdueItemsLimit var:o1

            //<charged items limit><valid patron><valid patron password><currency type><fee amount><fee limit>
            //CB	BL	CQ	BH	BV	CC
CB_ChargedItemsLimit var:o1
BL_ValidPatron var:o1
CQ_ValidPatronPassword var:o1
BH_CurrencyType var:o1
BV_FeeAmount var:o1
CC_FeeLimit var:o1

             //<hold items><overdue items><charged items><fine items><recall items><unavailable hold items>
             //AS	AT	AU	AV	BU	CD
AS_HoldItems var:on
AT_OverdueItems var:on
AU_ChargedItems var:on
AV_FineItems var:on
BU_RecallItems var:on
CD_UnavailableHoldItems var:on

            //<home address><e-mail address><home phone number><screen message><print line>
            //BD	BE  BF	AF	AG
BD_HomeAddress var:o1
BE_EmailAddress var:o1
BF_HomePhoneNumbers var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on

---65---
            //==前面的定长字段
            //<transaction date>
            //18-char
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><patron identifier><patron password><terminal password><fee acknowledged>
            //	AO	AA	AD	AC	BO
AO_InstitutionId var:r1
AA_PatronIdentifier var:r1
AD_PatronPassword var:o1
AC_TerminalPassword var:o1
BO_FeeAcknowledged var:o1
---66---
            //==前面的定长字段
            //<ok ><renewed count><unrenewed count><transaction date>
            //1-char	4-char	4-char	18-char	
##_Ok fix:1
##_RenewedCount fix:4
##_UnrenewedCount fix:4
##_TransactionDate fix:18

            //==后面变长字段
            //<institution id><renewed items><unrenewed items><screen message><print line>
            //AO	BM	BN	AF	AG
AO_InstitutionId var:r1
BM_RenewedItems var:on
BN_UnrenewedItems var:on
AF_ScreenMessage var:on
AG_PrintLine var:on

---93---
            //==前面的定长字段
            //<UID algorithm><PWD algorithm>
            //1-char	1-char
##_UIDAlgorithm fix:1
##_PWDAlgorithm fix:1

            // <login user id><login password><location code>
            // CN   	CO	CP
CN_LoginUserId var:r1
CO_LoginPassword var:r1
CP_LocationCode var:o1
---94---
            //==前面的定长字段
##_Ok fix:1
            // 2021/3/4
            // 标准里面没有的两个字段，方便观察错误信息
AF_ScreenMessage var:on
AG_PrintLine var:on

---98---
            //==前面的定长字段
            //98<on-line status><checkin ok><checkout ok><ACS renewal policy><status update ok><off-line ok>
            //98	1-char	1-char	1-char	---1-char	1-char	1-char
##_OnlineStatus fix:1
##_CheckinOk fix:1
##_CheckoutOk fix:1
##_ACSRenewalPolicy fix:1
##_StatusUpdateOk fix:1
##_OfflineOk fix:1
            //<timeout period><retries allowed><date / time sync><protocol version>
            //3-char	3-char  18-char	4-char	
##_TimeoutPeriod fix:3
##_RetriesAllowed fix:3
##_DatetimeSync fix:18
##_ProtocolVersion fix:4

            //==后面变长字段
            //<institution id><library name><supported messages ><terminal location><screen message><print line>
            //AO	AM	BX	AN	AF	AG
AO_InstitutionId var:r1
AM_LibraryName var:o1
//2020/8/14 改为false,发现SIP2 Developers Guide.pdf里的例子没有BX，但在SIP2.0.pdf里指明是必备字段
BX_SupportedMessages var:o1

AN_TerminalLocation var:o1
AF_ScreenMessage var:on
AG_PrintLine var:on

---99---
            //==前面的定长字段
            //<status code><max print width><protocol version>
            //1-char	3-char	4-char
// 1-char, fixed-length required field: 0 or 1 or 2
##_StatusCode fix:1
// 3-char, fixed-length required field
##_MaxPrintWidth fix:3
// 4-char, fixed-length required field:  x.xx
##_ProtocolVersion fix:4
";

        // 公共字段定义
        static string _common_field_defs =
@"##_CardRetained fix:1
##_TransactionDate fix:18
AO_InstitutionId var:r1
AL_BlockedCardMsg var:r1
AA_PatronIdentifier var:r1
AC_TerminalPassword var:r1";

        #endregion

        static List<MessageRule> _global_rules = null;
        static MessageRule _global_common_rule = null;

        MessageRule _rule = null;

        // 清楚当前消息的定义对象 _rule
        public virtual void ClearMessageRule()
        {
            _rule = null;
        }

        // (从全局搜索)获得当前消息的定义对象
        public virtual MessageRule GetMessageRule()
        {
            if (_global_rules == null)
                InitializeRules();

            return _global_rules.Where(o => o.Name == this.CommandIdentifier).FirstOrDefault();
        }

        // 初始化全局定义存储
        void InitializeRules()
        {
            _global_rules = new List<MessageRule>();

            _global_common_rule = new MessageRule("  ", _common_field_defs.Replace("\r\n", "\n").Split('\n'));

            string field_name = "";
            List<string> lines = new List<string>();
            using (StringReader sr = new StringReader(_message_defs))
            {
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.StartsWith("---"))
                    {
                        if (lines.Count > 0)
                        {
                            var rule = new MessageRule(field_name, lines.ToArray(), _global_common_rule);
                            _global_rules.Add(rule);
                            lines.Clear();
                        }
                        field_name = line.Substring(3, 2);
                        continue;
                    }

                    // 跳过注释行
                    if (line.Trim().StartsWith("//"))
                        continue;
                    lines.Add(line);
                }

                if (lines.Count > 0)
                {
                    var rule = new MessageRule(field_name, lines.ToArray());
                    _global_rules.Add(rule);
                    lines.Clear();
                }
            }
        }

        // 定长字段数组
        public List<FixedLengthField> FixedLengthFields = new List<FixedLengthField>();

        // 变长字段数组
        public List<VariableLengthField> VariableLengthFields = new List<VariableLengthField>();

        #region 新函数

        #endregion


        #region 定长字段

        // 获取某个定长字段对象
        protected FixedLengthField GetFixedField(string name)
        {
            name = name.ToLower().Replace(" ", "").Replace("-", "");
            foreach (FixedLengthField field in this.FixedLengthFields)
            {
                if (string.Compare(field.Name, name, true) == 0)
                    return field;
            }
            return null;
        }

        // 获取某个定长字段的值
        // return:
        //      null    指定的定长字段不存在
        //      ""      指定的定长字段存在，值为 ""
        //      其它      定长字段值
        public string GetFixedFieldValue(string name)
        {
            // var field_rule = verify ? this._rule?.FindFieldRule(name) : null;

            FixedLengthField field = this.GetFixedField(name);
            if (field == null)
                return null;

            /*
                throw new Exception($"消息 {this._id} 中不存在定长字段 {name}");   //???要不要统一改为getWarning，暂时先不改。2022/9/13
            */

            return field?.Value;
        }

        // 设置某个定长字段的值
        public void SetFixedFieldValue(string name,
            string value,
            bool verify = true)
        {
            var field_rule = verify ? this._rule?.FindFieldRule(name) : null;

            FixedLengthField field = this.GetFixedField(name);
            if (field == null)
            {
                /*
                if (verify == true)
                    throw new Exception($"消息 {this._id} 中未定义定长字段 {name}，无法设置值"); 
                */

                if (field_rule != null)
                {
                    // int index = this._rule.FieldRules.IndexOf(field_rule);
                    field = new FixedLengthField(field_rule.Alias[0], field_rule.FixFieldLength);
                    // TODO: 插入到正确位置
                    this.FixedLengthFields.Add(field);
                }
                else
                {
                    field = new FixedLengthField(name, value.Length);
                    this.FixedLengthFields.Add(field);
                }
            }

            field.Value = value;
        }

        #endregion

        #region 变长字段

        /*
        // 获取某个定长字段
        protected VariableLengthField GetVariableField(string id)
        {
            VariableLengthField temp = null;
            foreach (VariableLengthField field in this.VariableLengthFields)
            {
                if (field.ID == id)
                {
                    //return field; 
                    temp = field;//2020/8/13 改造，找到最后一个同名字段再返回。
                }
            }

            return temp;
        }
        */

        // 获得指定 ID 的一个或者多个变长字段对象
        List<VariableLengthField> GetVariableFields(string id)
        {
            return this.VariableLengthFields.Where(o => o.ID == id).ToList();
        }

        /*
        protected string GetVariableFieldValue(string id)
        {
            VariableLengthField field = this.GetVariableField(id);
            if (field == null)
            {
                // 20170811 jane todo
                if (id == "AY" || id == "AZ")
                    return "";

                throw new Exception(this.GetWarning(id));//未定义变长字段" + id); 2022/9/13修改
            }

            return field.Value;
        }
        */

        // 注意获得的一个字符串中，可能会有 \r\n
        public string GetVariableFieldValue(string id, string delimeter = "\r\n")
        {
            var values = GetVariableFieldValues(id);
            if (values == null)
                return null;
            return string.Join(delimeter, values);
        }

        // 获得指定 ID 的一个或者多个字段的值
        // 注: 对定义的不可重复或者可重复字段都是适用的。注意真实环境下，可能出现违背定义的情况，比如不允许重复出现的字段发生重复了
        protected string[] GetVariableFieldValues(string id)
        {
            var fields = this.GetVariableFields(id);
            StringBuilder text = new StringBuilder();
            return fields.Select(o => o.Value).ToArray();
        }

#if REMOVED
        // 设置某个变长字段的值
        protected void SetVariableFieldValue(string id, string value)
        {
            VariableLengthField field = this.GetVariableField(id);
            if (value == null && field == null)
                return;
            if (field == null)
            {
                /*
                // 20170811 jane todo
                if (id == "AY" || id == "AZ")
                    return;

                throw new Exception(GetWarning(id));//"未定义变长字段" + id);  2022/9/13
                */
                var field_rule = this._rule.FindFieldRule(id);
                field = new VariableLengthField(field_rule);
                this.VariableLengthFields.Add(field);
            }
            else
            {
                if (value == null)
                {
                    this.VariableLengthFields.Remove(field);
                    return;
                }
            }

            field.Value = value;
#if REMOVED
            // 2020/8/13 如果是重复的字段，先检查字段是否已经赋值，没赋值过则赋值，
            // 如果已有值，则新new的一个字段,插在其后（这样可以保证原始的字段顺序）
            if (field.IsRepeat == false)
            {
                field.Value = value;
            }
            else
            {
                if (string.IsNullOrEmpty(field.Value) == true)
                    field.Value = value;
                else
                {
                    VariableLengthField f = new VariableLengthField(id, field.IsRepeat, field.IsRepeat);
                    f.Value = value;

                    // 插在其后,不要用增加（这样可以保证原始的字段顺序）
                    this.VariableLengthFields.Insert(this.VariableLengthFields.IndexOf(field) + 1, f);
                    //this.VariableLengthFields.Add(f);
                }
            }
#endif
        }

        // 2022/9/13 针对不能识别的字段，内部统一提示的一个接口
        private string GetWarning(string field)
        {
            return "'" + this.CommandIdentifier + "'消息中，出现了不能识别的'" + field + " 字段。";
        }
#endif

        // 注意 value 中可能会有 \r\n
        public void SetVariableFieldValue(string id, string value, string delimeter = "\r\n")
        {
            string[] values = null;
            if (value != null)
                values = value.Replace(delimeter, "\n").Split('\n');
            SetVariableFieldValues(id, values);
        }

        // 设置指定 ID 的一个或者若干个字段的值
        protected void SetVariableFieldValues(string id,
            string[] values,
            bool verify_rule = true)
        {
            var field_rule = verify_rule ? this._rule?.FindFieldRule(id) : null;

            var values_is_null = (values == null || values.Length == 0);
            var fields = this.GetVariableFields(id);
            if (fields.Count == 0)
            {
                if (values_is_null)
                    return; // 正好不需要删除

                if (field_rule != null
                    && values.Length > 1
                    && field_rule.IsRepeatable == false)
                    throw new Exception($"消息 {this._id} 的字段 {id} 按照定义不允许重复，可是现在却试图创建 {values.Length} 个字段");
                foreach (var value in values)
                {
                    var current = new VariableLengthField(id);
                    current.Value = value;
                    this.VariableLengthFields.Add(current);
                }
                return;
            }

            Debug.Assert(fields.Count > 0);

            // 记忆插入点
            int index = this.VariableLengthFields.IndexOf(fields[0]);
            // 先删除
            foreach (var current in fields)
            {
                this.VariableLengthFields.Remove(current);
            }

            // 再创建
            if (values_is_null == false)
            {
                if (field_rule != null
    && values.Length > 1
    && field_rule.IsRepeatable == false)
                    throw new Exception($"消息 {this._id} 的字段 {id} 按照定义不允许重复，可是现在却试图创建 {values.Length} 个字段");

                // var field_rule = this._rule.FindFieldRule(id);
                foreach (var value in values)
                {
                    var current = new VariableLengthField(id);
                    current.Value = value;
                    this.VariableLengthFields.Insert(index++, current);
                }
                return;
            }
        }


        protected List<VariableLengthField> GetVariableFieldList(string id)
        {
            List<VariableLengthField> list = new List<VariableLengthField>();
            foreach (VariableLengthField field in this.VariableLengthFields)
            {
                if (field.ID == id)
                {
                    list.Add(field);
                }
            }
            return list;
        }

        // 替换指定 ID 的若干字段
        protected void SetVariableFieldList(string id, List<VariableLengthField> list)
        {
            if (string.IsNullOrEmpty(id) == true)
                throw new Exception("id参数不能为空");

            //传入数组字段必须与id同名
            foreach (VariableLengthField field in list)
            {
                if (field.ID != id)
                    throw new Exception("数组中有个字段名为" + field.ID + ",与指定的字段名" + id + "不符。");
            }

            // 先删除原来已存在的同名字段
            List<VariableLengthField> oldList = this.GetVariableFieldList(id);
            foreach (VariableLengthField field in oldList)
            {
                this.VariableLengthFields.Remove(field);
            }

            // 再增加新的字段
            foreach (VariableLengthField field in list)
            {
                this.VariableLengthFields.Add(field);
            }

        }

        #endregion

        #region 各命令通用字段

        public List<VariableLengthField> AF_ScreenMessage_List
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_AF_ScreenMessage);
            }
        }

        //variable-length optional field
        public string AF_ScreenMessage_o
        {
            get
            {
                return GetVariableFieldValue("AF");
                /*
                // 2020/8/14，因为AF是重复字段，所以把所有值用逗号组合起来，如果前端想自己拼装，请调AF_ScreenMessage_List
                List<VariableLengthField> list = this.GetVariableFieldList(SIPConst.F_AF_ScreenMessage);
                string text = "";
                foreach (VariableLengthField one in list)
                {
                    if (text != "")
                        text += ",";
                    text += one.Value;
                }
                return text;
                */
            }
            set
            {
                SetVariableFieldValue("AF", value);
                /*
                // 2020/8/16注意赋值的时候，只给一个字段赋值（GetVariableField是找最后一个字段，一般情况下dp2 response命令只有一个AF）。
                VariableLengthField field = this.GetVariableField(SIPConst.F_AF_ScreenMessage);
                if (field == null)
                {
                    field = new VariableLengthField(SIPConst.F_AF_ScreenMessage, false, true);
                    this.VariableLengthFields.Add(field);
                }
                field.Value = value;
                */
            }
        }

        // AG是重复字段
        public List<VariableLengthField> AG_PrintLine
        {
            get
            {
                return this.GetVariableFieldList(SIPConst.F_AG_PrintLine);
            }
        }

        //variable-length optional field
        public string AG_PrintLine_o
        {
            get
            {
                return GetVariableFieldValue("AG");
                /*
                // 2020/8/14，因为AG是重复字段，所以把所有值用逗号组合起来，如果前端想自己拼装，请调 AG_PrintLine
                List<VariableLengthField> list = this.GetVariableFieldList(SIPConst.F_AG_PrintLine);
                string text = "";
                foreach (VariableLengthField one in list)
                {
                    if (text != "")
                        text += ",";
                    text += one.Value;
                }
                return text;
                */
            }
            set
            {
                SetVariableFieldValue("AG", value);
                /*
                // TODO: 用逗号分隔，同时设置多个字段的值

                // 2020/8/16注意赋值的时候，只给一个字段赋值（GetVariableField是找最后一个字段，一般情况下dp2 response命令只有一个AG）。
                VariableLengthField field = this.GetVariableField(SIPConst.F_AG_PrintLine);
                if (field == null)
                {
                    field = new VariableLengthField(SIPConst.F_AG_PrintLine, false, true);
                    this.VariableLengthFields.Add(field);
                }
                field.Value = value;
                */
            }
        }

        #endregion

        // 解析字符串命令为对象
        public virtual int Parse(string text, out string error)
        {
            error = "";

            // 去掉末尾的 \r
            if (text != null
                && string.IsNullOrEmpty(text) == false
                && text[text.Length - 1] == '\r')
                text = text.Substring(0, text.Length - 1);

            if (text == null || text.Length < 2)
            {
                error = "消息字符串为 null 或字符数小于 2";
                return -1;
            }

            this.CommandIdentifier = text.Substring(0, 2);  //命令指示符
            string content = text.Substring(2); //内容

            this.FixedLengthFields.Clear();
            int start = 0;
            // 给定长字段赋值
            if (this._rule != null)
            {
                var rules = this._rule.FieldRules.Where(o => o.ID == "##").ToList();
                foreach (var rule in rules)
                {
                    var field = new FixedLengthField(rule.Alias[0], rule.FixFieldLength);
                    this.FixedLengthFields.Add(field);
                    field.Value = content.Substring(start, field.Length);
                    start += field.Length;
                }
            }
            else
            {
                // 没有任何固定长字段定义的情况下

                // 寻找第一个 |
                string value = "";
                var pos = content.IndexOf('|');
                if (pos == -1)
                {
                    value = content;
                    start = content.Length;
                }
                else
                {
                    value = content.Substring(0, pos);
                    start += value.Length + 1;
                }

                {
                    var field = new FixedLengthField($"fix", value.Length);
                    this.FixedLengthFields.Add(field);
                    field.Value = value;
                    start += field.Length;
                }
            }

            //处理后面的变长字段
            string rest = content.Substring(start);
            if (string.IsNullOrEmpty(rest) == false)
            {
                string[] parts = rest.Split(new char[] { '|' });
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    if (part.Length < 2)
                    {
                        continue;
                    }

                    string fieldId = part.Substring(0, 2);
                    string value = part.Substring(2);
                    var rule = this._rule.FindFieldRule(fieldId, new FieldRule(fieldId));
                    VariableLengthField field = null;
                    if (rule != null)
                        field = new VariableLengthField(rule);  // 有定义的情况
                    else
                        field = new VariableLengthField(fieldId);   // 没有定义的情况

                    field.Value = StringUtil.EscapeString(value, "|");

                    this.VariableLengthFields.Add(field);
                }
            }

            // 校验;
            int ret = this.Verify(out error);
            if (ret == -1)
                return -1;

            return 0;
        }

        // 将对象转换字符串命令
        public virtual string ToText()
        {
            Debug.Assert(String.IsNullOrEmpty(this.CommandIdentifier) == false, "命令指示符未赋值");
            StringBuilder text = new StringBuilder(this.CommandIdentifier);

            if (this.FixedLengthFields != null)
            {
                //foreach (FixedLengthField field in this.FixedLengthFields)
                for (int i = 0; i < this.FixedLengthFields.Count; i++)
                {
                    FixedLengthField field = this.FixedLengthFields[i];
                    if (field.Value == null || field.Value.Length != field.Length)
                        throw new Exception("定长字段[" + field.Name + "]的值为null或者长度不符合定义");
                    text.Append(field.Value);
                }
            }

            if (this.VariableLengthFields != null && this.VariableLengthFields.Count > 0)
            {
                //foreach (VariableLengthField field in this.VariableLengthFields)
                for (int i = 0; i < this.VariableLengthFields.Count; i++)
                {
                    VariableLengthField field = this.VariableLengthFields[i];
                    if (field.Value != null)
                    {
                        text.Append(field.ID + field.Value + SIPConst.FIELD_TERMINATOR);
                    }
                }
            }


            string result = text.ToString();

            // 去掉字符串最后一个|
            if (string.IsNullOrEmpty(result) == false)
            {
                if (result.Substring(result.Length - 1) == SIPConst.FIELD_TERMINATOR)
                    result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        // 校验对象的各参数是否合法
        public virtual int Verify(out string error)
        {
            error = "";
            List<string> errors = new List<string>();

            // 校验定长字段
            foreach (FixedLengthField field in this.FixedLengthFields)
            {
                if (field.Value == null || field.Value.Length != field.Length)
                {
                    errors.Add($"消息 {_id} 中字段 {field.Name} 的值为 null 或者长度不符合要求的长度 {field.Length}");
                }
            }

            foreach (VariableLengthField field in this.VariableLengthFields)
            {
                /*
                if (field.IsRequired == true && field.Value == null)
                {
                    error = $"消息 {_id} 中 字段 {field.ID} 是必备字段，消息中需包含该字段";
                    return -1;
                }
                */
            }

            if (this._rule != null && this._rule.FieldRules != null)
            {
                foreach (var item in this._rule.FieldRules)
                {
                    // 检查必备字段是否具备
                    if (item.IsRequired && item.ID != "##")
                    {
                        var fields = this.GetVariableFieldList(item.ID);
                        if (fields.Count == 0)
                            errors.Add($"消息 {_id} 中缺乏必备字段 {item.ID}");
                    }
                }
            }

            if (errors.Count > 0)
            {
                error = StringUtil.MakePathList(errors, "; ");
                return -1;
            }
            return 0;
        }

        // 根据定义，创建缺省的字段内容
        public virtual void SetDefaultValue()
        {
            this.FixedLengthFields.Clear();
            this.VariableLengthFields.Clear();

            var rule = GetMessageRule();
            if (rule == null)
                return;

            // TODO: 如何表达某个可重复字段在缺省情况下就需要创建多个?
            foreach (var item in rule.FieldRules)
            {
                if (item.ID == "##")  // 表示这是定长、无名字段
                {
                    Debug.Assert(item.IsFixLength == true);
                    Debug.Assert(item.IsRequired == true);
                    Debug.Assert(item.IsRepeatable == false);
                    /*
                    if (item.IsFixLength == false)
                        throw new Exception($"字段 '{item.ID}' 的定义不合法，ID 为 ## 但 .IsFixLength 为 false");
                    if (item.IsRequired == false)
                        throw new Exception($"字段 '{item.ID}' 的定义不合法，.IsRequired 为 false");
                    */
                    // Debug.Assert(item.Alias != null && item.Alias.Length > 0);
                    var field = new FixedLengthField(item.Alias[0], item.FixFieldLength);
                    field.Value = new string(' ', item.FixFieldLength);
                    this.FixedLengthFields.Add(field);
                }
                else
                {
                    if (item.IsRequired)
                        this.VariableLengthFields.Add(new VariableLengthField(item));
                }
            }

        }
    }

}
