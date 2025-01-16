using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2SSL.SIP2
{
    public class SIPConst
    {

        #region 字段名
        // 1-char, fixed-length field:  Y or N. 
        //A Y indicates that the SC is allowed by the ACS to process patron renewal requests as a policy.  
        //This field was called “renewal ok” in Version 1.00 of the protocol.
        public const string F_ACSRenewalPolicy = "ACS renewal policy";
        //alert
        public const string F_Alert = "alert";
        //available
        public const string F_Available = "available";
        //card retained
        public const string F_CardRetained = "card retained";
        //charged items count 
        public const string F_ChargedItemsCount = "charged items count";
        //checkin ok 
        public const string F_CheckinOk = "checkin ok";
        //checkout ok
        public const string F_CheckoutOk = "checkout ok";
        //circulation status
        public const string F_CirculationStatus = "circulation status";
        //datetime sync
        public const string F_DatetimeSync = "datetime sync";
        //desensitize 
        public const string F_Desensitize = "desensitize";
        //end session
        public const string F_EndSession = "end session";
        //fine items count
        public const string F_FineItemsCount = "fine items count";
        //hold items count
        public const string F_HoldItemsCount = "hold items count";
        //hold mode
        public const string F_HoldMode = "hold mode";
        //item properties ok
        public const string F_ItemPropertiesOk = "item properties ok";
        //language
        public const string F_Language = "language";
        //magnetic media
        public const string F_MagneticMedia = "magnetic media";
        //max print width 3-char, fixed-length field
        public const string F_MaxPrintWidth = "max print width";
        //nb due date
        public const string F_NbDueDate = "nb due date";
        //1-char, fixed-length
        public const string F_NoBlock = "no block";
        //off-line ok
        public const string F_OfflineOk = "off-line ok";
        //ok
        public const string F_Ok = "ok";
        //on-line status
        public const string F_OnlineStatus = "on-line status";
        //overdue items count
        public const string F_OverdueItemsCount = "overdue items count";
        //patron status
        public const string F_PatronStatus = "patron status";
        //payment accepted
        public const string F_PaymentAccepted = "payment accepted";
        //payment type
        public const string F_PaymentType = "payment type";
        //protocol version 4-char, fixed-length field:  x.xx. 
        public const string F_ProtocolVersion = "protocol version";
        //PWD algorithm
        public const string F_PWDAlgorithm = "PWD algorithm";
        //recall items count
        public const string F_RecallItemsCount = "recall items count";
        //renewal ok
        public const string F_RenewalOk = "renewal ok";
        //renewed count    
        public const string F_RenewedCount = "renewed count";
        //resensitize
        public const string F_Resensitize = "resensitize";
        //retries allowed
        public const string F_RetriesAllowed = "retries allowed";
        //return date 18-char
        public const string F_ReturnDate = "return date";
        //SC renewal policy
        public const string F_SCRenewalPolicy = "SC renewal policy";
        //security marker
        public const string F_SecurityMarker = "security marker";

        //1-char, fixed-length field: 0 or 1 or 2; the status of the SC unit. Value Definition    
        //0 SC unit is OK    
        //1 SC printer is out of paper    
        //2 SC is about to shut down
        public const string F_StatusCode = "status code";

        //status update ok
        public const string F_StatusUpdateOk = "status update ok";
        //summary
        public const string F_Summary = "summary";
        //third party allowed
        public const string F_ThirdPartyAllowed = "third party allowed";
        //timeout period
        public const string F_TimeoutPeriod = "timeout period";
        //transaction date 18-char
        public const string F_TransactionDate = "transaction date";
        //UID algorithm
        public const string F_UIDAlgorithm = "UID algorithm";
        //unavailable holds count
        public const string F_UnavailableHoldsCount = "unavailable holds count";
        //unrenewed count
        public const string F_UnrenewedCount = "unrenewed count";

        //==变长字段===

        //patron identifier AA
        public const string F_AA_PatronIdentifier = "AA";
        //item identifier AB
        public const string F_AB_ItemIdentifier = "AB";
        //terminal password AC
        public const string F_AC_TerminalPassword = "AC";
        //patron password AD
        public const string F_AD_PatronPassword = "AD";
        //personal name AE
        public const string F_AE_PersonalName = "AE";
        //screen message AF
        public const string F_AF_ScreenMessage = "AF";
        //print line AG
        public const string F_AG_PrintLine = "AG";
        //due date AH
        public const string F_AH_DueDate = "AH";
        //title identifier AJ
        public const string F_AJ_TitleIdentifier = "AJ";
        //blocked card msg AL
        public const string F_AL_BlockedCardMsg = "AL";
        //library name AM
        public const string F_AM_LibraryName = "AM";
        //terminal location AN
        public const string F_AN_TerminalLocation = "AN";
        //institution id AO
        public const string F_AO_InstitutionId = "AO";
        //current location AP
        public const string F_AP_CurrentLocation = "AP";
        //permanent location AQ
        public const string F_AQ_PermanentLocation = "AQ";
        //hold items AS
        public const string F_AS_HoldItems = "AS";
        //overdue items AT
        public const string F_AT_OverdueItems = "AT";
        //charged items AU
        public const string F_AU_ChargedItems = "AU";
        //fine items AV
        public const string F_AV_FineItems = "AV";
        //sequence number AY
        public const string F_AY_SequenceNumber = "AY";
        //checksum AZ
        public const string F_AZ_Checksum = "AZ";

        //===B==

        //home address BD
        public const string F_BD_HomeAddress = "BD";
        //e-mail address BE
        public const string F_BE_EmailAddress = "BE";
        //home phone number BF
        public const string F_BF_HomePhoneNumbers = "BF";
        //owner BG
        public const string F_BG_Owner = "BG";
        //currency type BH
        public const string F_BH_CurrencyType = "BH";
        //cancel BI
        public const string F_BI_Cancel = "BI";
        //transaction id BK
        public const string F_BK_TransactionId = "BK";
        //valid patron BL
        public const string F_BL_ValidPatron = "BL";
        //renewed items BM
        public const string F_BM_RenewedItems = "BM";
        //unrenewed items BN
        public const string F_BN_UnrenewedItems = "BN";
        //fee acknowledged BO
        public const string F_BO_FeeAcknowledged = "BO";
        //start item BP
        public const string F_BP_StartItem = "BP";
        //end item BQ
        public const string F_BQ_EndItem = "BQ";
        //queue position BR
        public const string F_BR_QueuePosition = "BR";
        //pickup location BS
        public const string F_BS_PickupLocation = "BS";
        //fee type BT
        public const string F_BT_FeeType = "BT";
        //recall items BU
        public const string F_BU_RecallItems = "BU";
        //fee amount BV
        public const string F_BV_FeeAmount = "BV";
        //expiration date BW
        public const string F_BW_ExpirationDate = "BW";
        //supported messages BX
        public const string F_BX_SupportedMessages = "BX";
        //hold type BY
        public const string F_BY_HoldType = "BY";
        //hold items limit BZ
        public const string F_BZ_HoldItemsLimit = "BZ";

        //===C===
        //overdue items limit CA
        public const string F_CA_OverdueItemsLimit = "CA";
        //charged items limit CB
        public const string F_CB_ChargedItemsLimit = "CB";
        //fee limit CC
        public const string F_CC_FeeLimit = "CC";
        //unavailable hold items CD
        public const string F_CD_UnavailableHoldItems = "CD";
        //hold queue length CF
        public const string F_CF_HoldQueueLength = "CF";
        //fee identifier CG
        public const string F_CG_FeeIdentifier = "CG";
        //item properties CH
        public const string F_CH_ItemProperties = "CH";
        //security inhibit CI
        public const string F_CI_SecurityInhibit = "CI";
        //recall date CJ
        public const string F_CJ_RecallDate = "CJ";
        //media type CK
        public const string F_CK_MediaType = "CK";
        //sort bin CL
        public const string F_CL_SortBin = "CL";
        //hold pickup date CM
        public const string F_CM_HoldPickupDate = "CM";
        //login user id CN
        public const string F_CN_LoginUserId = "CN";
        //login password CO
        public const string F_CO_LoginPassword = "CO";
        //location code CP
        public const string F_CP_LocationCode = "CP";
        //valid patron password CQ
        public const string F_CQ_ValidPatronPassword = "CQ";

        //===K===

        // call no	KC	可选	索取号，dp2扩展字段。
        public const string F_KC_CallNo = "KC";

        // permanent shelf no	KQ	可选	永久架位号，dp2扩展字段。
        public const string F_KQ_PermanentShelfNo = "KQ";

        // current shelf no	KP	可选 	当前架位号，dp2扩展字段。
        public const string F_KP_CurrentShelfNo = "KP";

        //holding state	HS	可选	0丢失 1编目 2在馆 ，dp2扩展字段。
        public const string F_HS_HoldingState = "HS";


        //===Z===

        //search word，检索词，dp2扩展字段。
        public const string F_ZW_SearchWord = "ZW";

        //start item，开始序号，dp2扩展字段。
        //public const string F_BP_StartItem = "BP"; //前面已定义

        //max count，最大数量，dp2扩展字段。
        public const string F_ZC_MaxCount = "ZC";

        //format，数据格式，dp2扩展字段。
        public const string F_ZF_format = "ZF";

        //total count，总数量，dp2扩展字段。
        public const string F_ZT_TotalCount = "ZT";

        //return count，总数量，dp2扩展字段。
        public const string F_ZR_ReturnCount = "ZR";

        //channel value，通道内容，dp2扩展字段。
        public const string F_ZV_Value = "ZV";

        #endregion

        // 缓冲区长度
        public const int COMM_BUFF_LEN = 1024;


        // 变长字段分隔符
        public const string FIELD_TERMINATOR = "|";

        //消息结束符
        public const string MESSAGE_TERMINATOR = "\n";




        //public const char Terminator = (char)13;
        // language
        public const string LANGUAGE_UNKNOWN = "000";
        public const string LANGUAGE_ENGLISH = "001";
        public const string LANGUAGE_CHINESE = "019";

        //media type
        public const string MEDIA_TYPE_OTHER = "000";
        public const string MEDIA_TYPE_BOOK = "001";
        public const string MEDIA_TYPE_ZINE = "002";
        public const string MEDIA_TYPE_BOUND_JOURNAL = "003";
        public const string MEDIA_TYPE_AUDIO_TAPE = "004";
        public const string MEDIA_TYPE_VIDEO_TAPE = "005";
        public const string MEDIA_TYPE_CD = "006";
        public const string MEDIA_TYPE_DISKETTE = "007";
        public const string MEDIA_TYPE_BOOK_WITH_DISKETTE = "008";
        public const string MEDIA_TYPE_BOOK_WITH_CD = "009";
        public const string MEDIA_TYPE_BOOK_WITH_AUDIO_TAPE = "010";

        //payment type
        public const string PAYMENT_TYPE_CASH = "00";
        public const string PAYMENT_TYPE_VISA = "01";
        public const string PAYMENT_TYPE_CREDIT_CARD = "02";

        // currency type
        public const string CURRENCY_TYPE_USD = "USD";
        public const string CURRENCY_TYPE_CAD = "CAD";
        public const string CURRENCY_TYPE_GBP = "GBP";
        public const string CURRENCY_TYPE_FRF = "FRF";
        public const string CURRENCY_TYPE_DEM = "DEM";
        public const string CURRENCY_TYPE_ITL = "ITL";
        public const string CURRENCY_TYPE_ESP = "ESP";
        public const string CURRENCY_TYPE_JPY = "JPY";

        // 无错误
        public const int MSIP_NO_ERROR = 0;

        // ACS字符集
        public const int ACS_CHAR_SET = 850;

        // AO字段值
        public const string AO_Value = "dp2Library";
    }

}
