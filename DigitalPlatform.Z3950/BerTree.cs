using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DigitalPlatform.Z3950
{
    public class INIT_REQUEST
    {
    	public string m_strReferenceId = "";
        public string m_strOptions = "";

        public long m_lPreferredMessageSize = 0;
	    public long m_lExceptionalRecordSize = 0;

    	public int	m_nAuthenticationMethod = 0;	// 0: open 1:idPass
	    public string m_strGroupID = "";
	    public string m_strID = "";

        public string m_strPassword = "";
        public string m_strImplementationId = "";
	    public string m_strImplementationName = "";
	    public string m_strImplementationVersion = "";

        public CharsetNeogatiation m_charNego = null;

    }

    public class INIT_RESPONSE
    {
        public string m_strReferenceId = "";
	    public string m_strOptions = "";
	    public long m_lPreferredMessageSize = 0;
	    public long m_lExceptionalRecordSize = 0;
        public long m_nResult = 0;

        public string m_strImplementationId = "";
        public string m_strImplementationName = "";
        public string m_strImplementationVersion = "";

        public long m_lErrorCode = 0;
        public string m_strErrorMessage = "";

        public CharsetNeogatiation m_charNego = null;
    }

    public class CLOSE_REQUEST
    {
        public string m_strReferenceId = "";
        public long m_nCloseReason = 0;
        public string m_strDiagnosticInformation = "";
        public string m_strResourceReportFormat = "";   // ResourceReportId    ::=    OBJECT IDENTIFIER
        public string m_strResourceReport = ""; //  ResourceReport     ::=   EXTERNAL
        public string m_strOtherInfo = "";  // 

        /*
        -- OtherInformation
        OtherInformation   ::= [201] IMPLICIT SEQUENCE OF SEQUENCE{
            category            [1]   IMPLICIT InfoCategory OPTIONAL, 
            information        CHOICE{
                characterInfo        [2]  IMPLICIT InternationalString,
                binaryInfo        [3]  IMPLICIT OCTET STRING,
                externallyDefinedInfo    [4]  IMPLICIT EXTERNAL,
                oid          [5]  IMPLICIT OBJECT IDENTIFIER}}
--
        InfoCategory ::= SEQUENCE{
            categoryTypeId  [1]   IMPLICIT OBJECT IDENTIFIER OPTIONAL,
            categoryValue  [2]   IMPLICIT INTEGER}

         * */


    }

    public class SEARCH_REQUEST
    {
        public string m_strReferenceId = "";
        public long m_lSmallSetUpperBound = 0;
	    public long m_lLargeSetLowerBound = 0;
	    public long m_lMediumSetPresentNumber = 0;
	    public int  m_nReplaceIndicator = 0;
        public string m_strResultSetName = "";
        public string [] m_dbnames = null;
        public string m_strSmallSetElementSetNames = "";
	    public string m_strMediumSetElementSetNames = "";
	    public string m_strPreferredRecordSyntax = "";
	    public string m_strQuery = "";
	    public int  m_nQuery_type = 0;
        public Encoding m_queryTermEncoding = Encoding.GetEncoding(936);
    }

    public class SEARCH_RESPONSE
    {
        public string m_strReferenceId = "";
	    public long m_lResultCount = 0;
	    public long m_lNumberOfRecordsReturned = 0;
	    public long m_lNextResultSetPosition = 0;
	    public int  m_nSearchStatus = 0;
	    public long m_lResultSetStatus = 0;
	    public int  m_nPresentStatus = 0;

	    // public long m_lErrorCode = 0;
	    // public string m_strErrorMessage = "";

        /*
        public long  m_nDiagCondition = 0;
	    public string m_strDiagSetID = "";
	    public string m_strAddInfo = "";
         * */
        // 一个或者多个non surrogate diagnostic record
        public DiagRecords m_diagRecords = null;
    }

    /// <summary>
    /// 诊断记录DiagFormat的数组
    /// </summary>
    public class DiagRecords : List<DiagFormat>
    {

        public string GetMessage()
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                DiagFormat diag = this[i];
                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += "\r\n";
                strResult += (this.Count > 1 ? (i+1).ToString() + ") " : "")
                    + diag.GetMessage();
            }

            return strResult;
        }
    }

    /// <summary>
    /// 诊断记录
    /// </summary>
    public class DiagFormat
    {
        public long m_nDiagCondition = 0;
        public string m_strDiagSetID = "";
        public string m_strAddInfo = "";    // 

        public void Clear()
        {
            this.m_nDiagCondition = 0;
            this.m_strDiagSetID = "";
            this.m_strAddInfo = ""; 
        }

        // 以字符串显示内部信息
        public string GetMessage()
        {
            return "addinfo=\"" + this.m_strAddInfo + "\"; diagSetId=\"" + this.m_strDiagSetID + "\"; condition=" + this.m_nDiagCondition + "";
        }

        // 估算数据所占的包尺寸
        public int GetPackageSize()
        {
            int nSize = 0;

            if (String.IsNullOrEmpty(this.m_strDiagSetID) == false)
            {
                // TODO: 修改为估算OID编码后的尺寸
                nSize += Encoding.UTF8.GetByteCount(this.m_strDiagSetID);
            }

            nSize += 4; // integer

            if (String.IsNullOrEmpty(this.m_strAddInfo) == false)
            {
                nSize += Encoding.UTF8.GetByteCount(this.m_strAddInfo);
            }

            return nSize;
        }

        public void BuildBer(BerNode nodeDiagRoot)
        {
            nodeDiagRoot.NewChildOIDsNode(6,
                BerNode.ASN1_UNIVERSAL,
                this.m_strDiagSetID);   // "1.2.840.10003.4.1"

            nodeDiagRoot.NewChildIntegerNode(2,
                BerNode.ASN1_UNIVERSAL,
                BitConverter.GetBytes((long)this.m_nDiagCondition));

            if (String.IsNullOrEmpty(this.m_strAddInfo) == false)
            {
                // V2必须用VisibleString，而V3既可以用VisibleString，也可以用GeneralString(称为InternationalString，具体编码受字符集协商的全局影响)
                // public const char ASN1_VISIBLESTRING    = (char)26;
                // public const char ASN1_GENERALSTRING    = (char)27;
                nodeDiagRoot.NewChildCharNode(BerNode.ASN1_VISIBLESTRING,   // 26 V2要求
                    BerNode.ASN1_UNIVERSAL,
                    Encoding.UTF8.GetBytes(this.m_strAddInfo));
            }
        }

        // 解码：将BerTree中的信息传递到本类结构中
        public void Decode(BerNode nodeDiagRoot,
            Encoding encodingOfInternationalString,
            bool bSetDebugInfo)
        {
            this.Clear();

            for (int i = 0; i < nodeDiagRoot.ChildrenCollection.Count; i++)
            {
                BerNode node = nodeDiagRoot.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case 6:
                        this.m_strDiagSetID = node.GetOIDsNodeData();
                        if (bSetDebugInfo == true)
                            node.m_strDebugInfo += "OID [" + this.m_strDiagSetID + "]";
                        break;
                    case 2:  /* integer */
                        this.m_nDiagCondition = node.GetIntegerNodeData();
                        if (bSetDebugInfo == true)
                            node.m_strDebugInfo += "integer [" + this.m_nDiagCondition.ToString() + "]";
                        break;
                    case BerNode.ASN1_VISIBLESTRING:  // 26 VisibleString if Z39.50 V2 in force
                        this.m_strAddInfo = node.GetCharNodeData();
                        if (bSetDebugInfo == true)
                            node.m_strDebugInfo += "VisibleString(26) [" + this.m_strAddInfo + "]";
                        break;
                    case BerNode.ASN1_GENERALSTRING:  // 27 GeneralString if Z39.50 V3 in force
                        {
                            // 缺省为UTF-8
                            if (encodingOfInternationalString == null)
                                encodingOfInternationalString = Encoding.UTF8;

                            this.m_strAddInfo = node.GetCharNodeData(encodingOfInternationalString);
                            if (bSetDebugInfo == true)
                                node.m_strDebugInfo += "GeneralString(27) encoding=" + encodingOfInternationalString.EncodingName + "  [" + this.m_strAddInfo + "]";
                        }
                        break;
                }
            }
        }
    }

    public class PRESENT_REQUEST
    {
	    public string m_strReferenceId = "";
	    public string m_strResultSetName = "";
        public long m_lResultSetStartPoint = 0;
	    public long m_lNumberOfRecordsRequested = 0;
        public string m_strElementSetNames = "";
	    public string m_strPreferredRecordSyntax = "";
    }


    public class BerTree
    {
        public BerNode m_RootNode = new BerNode();

        #region 常量

        public const string OPAC_SYNTAX = "1.2.840.10003.5.102";
        public const string OCLC_BER_SYNTAX = "1.2.840.10003.5.1000.17.1";
        public const string MARC_SYNTAX = "1.2.840.10003.5.10";
        public const string SIMPLETEXT_SYNTAX = "1.2.840.10003.5.101";
        public const string OCLC_USERINFORMATION_1 = "1.2.840.10003.10.1000.17.1";
        public const string OCLC_EXTENDED_SERVICE_PRICE = "1.2.840.10003.9.1000.17.1";
        public const string OCLC_EXTENDED_SERVICE_ORDER_SUPPLIER_INFO = "1.2.840.10003.9.1000.17.2";
        public const string EXTENDED_SERVICE_ORDER = "1.2.840.10003.9.4";


        public const int GIIR_REFERENCEID	= 0x0001;
        public const int GIIR_OPTIONS		= 0x0002;
        public const int GIIR_PREFERREDMESSAGESIZE	= 0x0004;
        public const int GIIR_MAXIMUMRECORDSIZE		= 0x0008;
        public const int GIIR_RESULT = 0x0010;


        /* init parms */
        public const UInt16 z3950_groupId                       = 0;
        public const UInt16 z3950_userId                        = 1;
        public const UInt16 z3950_password                      = 2;
        public const UInt16 z3950_ReferenceId                   = 2;
        public const UInt16 z3950_newPassword                   = 3;
        public const UInt16 z3950_ProtocolVersion               = 3;
        public const UInt16 z3950_Options                       = 4;
        public const UInt16 z3950_PreferredMessageSize          = 5;
        public const UInt16 z3950_ExceptionalRecordSize         = 6;
        public const UInt16 z3950_idAuthentication              = 7;
        public const UInt16 z3950_UserInformationField         = 11;
        public const UInt16 z3950_result                       = 12;
        public const UInt16 z3950_ImplementationId             =110;
        public const UInt16 z3950_ImplementationName           =111;
        public const UInt16 z3950_ImplementationVersion        =112;
        public const UInt16 z3950_InitFailureReason             = 3;
        public const UInt16 z3950_InitFailureCode               = 1;
        public const UInt16 z3950_InitFailureMsg                = 2;

        public const UInt16 z3950_OtherInformationField = 201;
        

        /* pdu names */
        public const UInt16 z3950_initRequest      = 20;
        public const UInt16 z3950_initResponse = 21;
        public const UInt16 z3950_searchRequest = 22;
        public const UInt16 z3950_searchResponse = 23;
        public const UInt16 z3950_presentRequest = 24;
        public const UInt16 z3950_presentResponse              = 25;
        public const UInt16 z3950_deleteResultSetRequest       = 26;
        public const UInt16 z3950_deleteResultSetResponse      = 27;
        public const UInt16 z3950_accessControlRequest         = 28;
        public const UInt16 z3950_accessControlResponse        = 29;
        public const UInt16 z3950_resourceControlRequest       = 30;
        public const UInt16 z3950_resourceControlResponse      = 31;
        public const UInt16 z3950_triggerResourceControlRequest= 32;
        public const UInt16 z3950_resourceReportRequest        = 33;
        public const UInt16 z3950_resourceReportResponse       = 34;
        public const UInt16 z3950_scanRequest                  = 35;
        public const UInt16 z3950_scanResponse                 = 36;
        public const UInt16 z3950_extendedservicesRequest      = 46;
        public const UInt16 z3950_extendedservicesResponse     = 47;
        public const UInt16 z3950_close                         = 48;

        /* 检索与表示 */
        public const UInt16 z3950_databaseRecord               =  1;
        public const UInt16 z3950_surrogateDiagnostic          =  2;
        public const UInt16 z3950_smallSetUpperBound           = 13;
        public const UInt16 z3950_largeSetLowerBound           = 14;
        public const UInt16 z3950_mediumSetPresentNumber       = 15;
        public const UInt16 z3950_replaceIndicator             = 16;
        public const UInt16 z3950_resultSetName                = 17;
        public const UInt16 z3950_databaseNames                = 18;
        public const UInt16 z3950_ElementSetNames              = 19;
        public const UInt16 z3950_query                        = 21;
        public const UInt16 z3950_searchStatus                 = 22;
        public const UInt16 z3950_resultCount                  = 23;
        public const UInt16 z3950_NumberOfRecordsReturned      = 24;
        public const UInt16 z3950_NextResultSetPosition        = 25;
        public const UInt16 z3950_resultSetStatus              = 26;
        public const UInt16 z3950_presentStatus                = 27;
        public const UInt16 z3950_dataBaseOrSurDiagnostics     = 28;
        public const UInt16 z3950_numberOfRecordsRequested     = 29;
        public const UInt16 z3950_resultSetStartPoint          = 30;
        public const UInt16 z3950_ResultSetId                  = 31;
        public const UInt16 z3950_AttributeList                = 44;
        public const UInt16 z3950_Term                         = 45;
        public const UInt16 z3950_Operator                     = 46;
        public const UInt16 z3950_smallSetElementSetNames      =100;
        public const UInt16 z3950_mediumSetElementSetNames     =101;
        public const UInt16 z3950_AttributesPlusTerm           =102;
        public const UInt16 z3950_PreferredRecordSyntax        =104;
        public const UInt16 z3950_DatabaseName                 =105;
        public const UInt16 z3950_AttributeType                =120;
        public const UInt16 z3950_AttributeValue = 121;

        /* query types */
        public const UInt16 z3950_type_0                       =  0;
        public const UInt16 z3950_type_1                       =  1;
        public const UInt16 z3950_type_2                       =  2;
        public const UInt16 z3950_type_100                     =100;
        public const UInt16 z3950_type_101 = 101;


        /* DIAG.2 Diagnostic Format Diag-1 */
        public const UInt16 z3950_nonSurrogateDiagnostic       =130;
        public const UInt16 z3950_MultipleNonSurrogates        =205;
        public const UInt16 z3950_DiagnosticFormatcondition     = 1;
        public const UInt16 z3950_DiagnosticFormatunspecified   = 1;
        public const UInt16 z3950_DiagnosticFormatspecified     = 2;
        public const UInt16 z3950_DiagnosticFormataddMsg        = 1;

        #endregion

        public BerNode GetAPDuRoot()
        {
            Debug.Assert(this.m_RootNode.ChildrenCollection.Count == 1);
            return this.m_RootNode.ChildrenCollection[0];
        }

        // 获得bitstring的一位
        public static bool GetBit(string strBitString,
            int nIndex)
        {
            if (String.IsNullOrEmpty(strBitString) == true)
                return false;

            if (nIndex >= strBitString.Length)
                return false;

            char ch = strBitString[nIndex];
            if (ch == 'y' || ch == 'Y'
                || ch == '1'
                || ch == 't' || ch == 'T')
                return true;

            return false;
        }

        // 修改bitstring的一位
        public static void SetBit(ref string strBitString,
            int nIndex,
            bool bOn)
        {
            if (strBitString.Length < nIndex + 1)
                strBitString = strBitString.PadRight(nIndex + 1, 'n');

            strBitString = strBitString.Remove(nIndex, 1);
            strBitString = strBitString.Insert(nIndex, bOn == true ? "y" : "n");
        }

        //	 build a z39.50 Init request   
        public int InitRequest(INIT_REQUEST struInit_request,
            Encoding encoding,
            out byte[] baPackage)
        {
            baPackage = null;

            BerNode root = null;
            BerNode subroot = null;
            string strTemp = "";
            string strData = "";
            // int nRet;

            string strProtocol_version = "yyn";  //  "yyynnnnn";  // "yy" versions 1 and 2 
            string strOptions_supported = "yynnnnnnnnnnnnnnnn";   //  "yynnnny"; /* search and present only */
                                        //"yyynynnyynynnnyn"
                                        // 012345678901234567
            if (struInit_request.m_charNego != null)
            {
                SetBit(ref strOptions_supported,
                    17,
                    true);
            }


            /* option
             * 
                     search                 (0), 
                     present                (1), 
                     delSet                 (2),
                     resourceReport         (3),
                     triggerResourceCtrl    (4),
                     resourceCtrl           (5), 
                     accessCtrl             (6),
                     scan                   (7),
                     sort                   (8), 
                     --                     (9) (reserved)
                     extendedServices       (10),
                     level-1Segmentation    (11),
                     level-2Segmentation    (12),
                     concurrentOperations   (13),
                     namedResultSets        (14)
                        15 Encapsulation  Z39.50-1995 Amendment 3: Z39.50 Encapsulation 
                        16 resultCount parameter in Sort Response  See Note 8 Z39.50-1995 Amendment 1: Add resultCount parameter to Sort Response  
                        17 Negotiation Model  See Note 9 Model for Z39.50 Negotiation During Initialization  
                        18 Duplicate Detection See Note 1  Z39.50 Duplicate Detection Service  
                        19 Query type 104 
             * }
            */

            root = m_RootNode.NewChildConstructedNode(z3950_initRequest,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(struInit_request.m_strReferenceId) == false)
            {
                root.NewChildCharNode(z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struInit_request.m_strReferenceId));
                // BitConverter.GetBytes((long)struInit_request.m_lReferenceId));
            }

            root.NewChildBitstringNode(z3950_ProtocolVersion,
                BerNode.ASN1_CONTEXT,
                strProtocol_version);

            if (String.IsNullOrEmpty(struInit_request.m_strOptions) == false)
            {
                root.NewChildBitstringNode(z3950_Options,
                    BerNode.ASN1_CONTEXT,
                    struInit_request.m_strOptions);
            }
            else
            {
                // 缺省的
                root.NewChildBitstringNode(z3950_Options,
                    BerNode.ASN1_CONTEXT,
                    strOptions_supported);
            }

            root.NewChildIntegerNode(z3950_PreferredMessageSize,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struInit_request.m_lPreferredMessageSize));

            root.NewChildIntegerNode(z3950_ExceptionalRecordSize,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struInit_request.m_lExceptionalRecordSize));

            /*
            root.NewChildintegerNode(z3950_ExceptionalRecordSize,
                BerNode.ASN1_CONTEXT,
                (CHAR*)&struInit_request.m_lMaximumRecordSize,
                sizeof(struInit_request.m_lMaximumRecordSize));
            */


            if (String.IsNullOrEmpty(struInit_request.m_strID) == false)
            {
                /*
        --   IdAuthentication [7] CHOICE{
        --      open    VisibleString,
        --      idPass  SEQUENCE {
        --                 groupId   [0]   IMPLICIT InternationalString
        OPTIONAL,
        --                 userId    [1]   IMPLICIT InternationalString
        OPTIONAL,
        --                 password  [2]   IMPLICIT InternationalString
        OPTIONAL },
        */

                subroot = root.NewChildConstructedNode(
                    z3950_idAuthentication,
                    BerNode.ASN1_CONTEXT);
                Debug.Assert(subroot != null, "");

                if (struInit_request.m_nAuthenticationMethod == 0)
                { // open
                    // strTemp.Format("%s/", struInit_request.m_strID);
                    strData += struInit_request.m_strID + "/";

                    if (String.IsNullOrEmpty(struInit_request.m_strPassword) == false)
                        strTemp = struInit_request.m_strPassword;
                    else
                        strTemp = "";
                    strData += strTemp;
                    subroot.NewChildCharNode(
                        BerNode.ASN1_VISIBLESTRING,
                        BerNode.ASN1_UNIVERSAL,
                        encoding.GetBytes(strData));
                }
                else if (struInit_request.m_nAuthenticationMethod == 1)
                { // idPass
                    subroot = subroot.NewChildConstructedNode(
                        BerNode.ASN1_SEQUENCE,
                        BerNode.ASN1_UNIVERSAL);
                    Debug.Assert(subroot != null, "");

                    subroot.NewChildCharNode(0,
                        BerNode.ASN1_CONTEXT,
                        encoding.GetBytes(struInit_request.m_strGroupID));

                    subroot.NewChildCharNode(1,
                        BerNode.ASN1_CONTEXT,
                        encoding.GetBytes(struInit_request.m_strID));
                    subroot.NewChildCharNode(2,
                        BerNode.ASN1_CONTEXT,
                        encoding.GetBytes(struInit_request.m_strPassword));
                }
                else
                {
                    Debug.Assert(false, "");
                }
            }


            root.NewChildCharNode(z3950_ImplementationId,
                BerNode.ASN1_CONTEXT,
                encoding.GetBytes(struInit_request.m_strImplementationId));

            root.NewChildCharNode(z3950_ImplementationName,
                BerNode.ASN1_CONTEXT,
                encoding.GetBytes(struInit_request.m_strImplementationName));

            root.NewChildCharNode(z3950_ImplementationVersion,
                BerNode.ASN1_CONTEXT,
                encoding.GetBytes(struInit_request.m_strImplementationVersion));

            // char neg - 4
            /*
otherInfo {
   {
      externallyDefinedInfo {
         OID: 1 2 840 10003 15 3
         externallyDefinedInfo choice
         {
            proposal {
              proposedCharSets {
                iso10646 {
                  encodingLevel OID: 1 0 10646 1 0 8
                }
              }
            }
         }
       }
   }
}
             * */

            if (struInit_request.m_charNego != null)
            {
                struInit_request.m_charNego.EncodeProposal(root);

                /*
                // SEQUENCE
                subroot = root.NewChildConstructedNode(
                    z3950_OtherInformationField,    // 201
                    BerNode.ASN1_CONTEXT);


                // OF SEQUENCE
                subroot = subroot.NewChildConstructedNode(
                    BerNode.ASN1_SEQUENCE,    // 16
                    BerNode.ASN1_CONTEXT);

                // externallyDefineInfo(of OtherInformation)
                subroot = subroot.NewChildConstructedNode(
                    4,
                    BerNode.ASN1_CONTEXT);

                // oid
                subroot.NewChildOIDsNode(
                    6,   // UNI_OBJECTIDENTIFIER,
                    BerNode.ASN1_UNIVERSAL,
                    "1.2.840.10003.15.3");

                // http://www.loc.gov/z3950/agency/defns/charneg-3.html
                // http://www.loc.gov/z3950/agency/defns/charneg-4.html

                // originProposal SEQUENCE
                subroot = subroot.NewChildConstructedNode(
                    1,
                    BerNode.ASN1_CONTEXT);

                // proposedCharSets SEQUENCE OF CHOICE
                subroot = subroot.NewChildConstructedNode(
                    1,
                    BerNode.ASN1_CONTEXT);

                // iso10646 SEQUENCE
                subroot = subroot.NewChildConstructedNode(
                    2,  // iso10646
                    BerNode.ASN1_CONTEXT);

                // encodingLevel
                BerNode node = subroot.NewChildOIDsNode(
                    2,  // encodingLevel
                    BerNode.ASN1_CONTEXT,
                    "1.0.10646.1.0.8");
                //
                //
                //          -- oid of form 1.0.10646.1.0.form
                //    -- where value of 'form' is 2, 4, 5, or 8 
                //    -- for ucs-2, ucs-4, utf-16, utf-8
                //
                 * */
            }

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

        // 观察Initial请求包
        public static int GetInfo_InitRequest(
    BerNode root,
    out string strDebugInfo,
    out string strError)
        {
            strError = "";
            strDebugInfo = "";

            Debug.Assert(root != null, "");

            if (root.m_uTag != z3950_initRequest)
            {
                strError = "root tag is not z3950_initRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strDebugInfo += "ReferenceID='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ProtocolVersion:
                        strDebugInfo += "ProtocolVersion='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case z3950_Options:
                        strDebugInfo += "Options='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case z3950_PreferredMessageSize:
                        strDebugInfo += "PreferredMessageSize='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_ExceptionalRecordSize:
                        strDebugInfo += "ExceptionalRecordSize='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_idAuthentication:
                        strDebugInfo += "idAuthentication struct occur\r\n";
                        break;
                    case z3950_ImplementationId:
                        strDebugInfo += "ImplementationId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ImplementationName:
                        strDebugInfo += "ImplementationName='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ImplementationVersion:
                        strDebugInfo += "ImplementationVersion='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }

            return 0;
        }


        //	 build a z39.50 Search Request 
        public int SearchRequest(SEARCH_REQUEST struSearch_request,
            out byte[] baPackage)
        {
            baPackage = null;

            BerNode root = null;
            BerNode subroot = null;
            // int i,nMax;
            int nRet;

            root = this.m_RootNode.NewChildConstructedNode(z3950_searchRequest,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(struSearch_request.m_strReferenceId) == false)
            {
                root.NewChildCharNode(z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struSearch_request.m_strReferenceId));
                // BitConverter.GetBytes((long)struSearch_request.m_lReferenceId));
                // ???到底应该是什么类型?
            }

            root.NewChildIntegerNode(z3950_smallSetUpperBound,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struSearch_request.m_lSmallSetUpperBound));

            root.NewChildIntegerNode(z3950_largeSetLowerBound,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struSearch_request.m_lLargeSetLowerBound));

            root.NewChildIntegerNode(z3950_mediumSetPresentNumber,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struSearch_request.m_lMediumSetPresentNumber));

            root.NewChildIntegerNode(z3950_replaceIndicator,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((int)struSearch_request.m_nReplaceIndicator));

            root.NewChildCharNode(z3950_resultSetName,
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(struSearch_request.m_strResultSetName));

            /*
            g_ptrResultName.Add((void *)struSearch_request.m_pszResultSetName);
                            //  此处的ptrResultName是一个全局变量，
                            //  用于表示不同的查询集，
                            //  而查询结果集将会在构造逆波兰树时使用
            */
            subroot = root.NewChildConstructedNode(z3950_databaseNames,
                BerNode.ASN1_CONTEXT);

            for (int i = 0; i < struSearch_request.m_dbnames.Length; i++)
            {
                subroot.NewChildCharNode(z3950_DatabaseName,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struSearch_request.m_dbnames[i]));
            }

            if (String.IsNullOrEmpty(struSearch_request.m_strSmallSetElementSetNames) == false)
            {
                subroot = root.NewChildConstructedNode(
                    z3950_smallSetElementSetNames,
                    BerNode.ASN1_CONTEXT);
                subroot.NewChildCharNode(0,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struSearch_request.m_strSmallSetElementSetNames));
            }

            if (String.IsNullOrEmpty(struSearch_request.m_strMediumSetElementSetNames) == false)
            {
                subroot = root.NewChildConstructedNode(
                    z3950_mediumSetElementSetNames,
                    BerNode.ASN1_CONTEXT);
                subroot.NewChildCharNode(0,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struSearch_request.m_strMediumSetElementSetNames));
            }

            if (String.IsNullOrEmpty(struSearch_request.m_strPreferredRecordSyntax) == false)
            {
                root.NewChildOIDsNode(z3950_PreferredRecordSyntax,
                    BerNode.ASN1_CONTEXT,
                    struSearch_request.m_strPreferredRecordSyntax);
            }
            /*
        else
        {
            root.NewChildOIDsNode(z3950_PreferredRecordSyntax,
                BerNode.ASN1_CONTEXT,
                MARC_SYNTAX);
        }
             * */

            subroot = root.NewChildConstructedNode(z3950_query,
                BerNode.ASN1_CONTEXT);
            if (struSearch_request.m_nQuery_type == 1
                || struSearch_request.m_nQuery_type == 100)
            {
                nRet = make_type_1(struSearch_request.m_strQuery,
                    struSearch_request.m_queryTermEncoding,
                    subroot);
                if (nRet == -1)
                {
                    Debug.Assert(false, "");
                    return -1;
                }
            }

            if (struSearch_request.m_nQuery_type == 101)
            {
                nRet = make_type_101(struSearch_request.m_strQuery, subroot);
                if (nRet == -1)
                {
                    Debug.Assert(false, "");
                    return -1;
                }
            }
            if (struSearch_request.m_nQuery_type == 0)
            {
                subroot.NewChildCharNode(z3950_type_0,
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(struSearch_request.m_strQuery));
            }

            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

        // 观察Search请求包
        public static int GetInfo_SearchRequest(
        BerNode root,
        out string strDebugInfo,
        out string strError)
        {
            strError = "";
            strDebugInfo = "";

            Debug.Assert(root != null, "");

            if (root.m_uTag != z3950_searchRequest)
            {
                strError = "root tag is not z3950_searchRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strDebugInfo += "ReferenceID='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_smallSetUpperBound:
                        strDebugInfo += "smallSetUpperBound='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_largeSetLowerBound:
                        strDebugInfo += "largeSetLowerBound='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_mediumSetPresentNumber:
                        strDebugInfo += "mediumSetPresentNumber='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_replaceIndicator:
                        strDebugInfo += "replaceIndicator='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_resultSetName:
                        strDebugInfo += "resultSetName='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_databaseNames:
                        strDebugInfo += "databaseNames=" + GetStringArray(node, z3950_DatabaseName) + "\r\n";
                        break;
                    case z3950_smallSetElementSetNames:
                        strDebugInfo += "smallSetElementSetNames=" + GetStringArray(node, 0) + "\r\n";
                        break;
                    case z3950_mediumSetElementSetNames:
                        strDebugInfo += "mediumSetElementSetNames=" + GetStringArray(node, 0) + "\r\n";
                        break;
                    case z3950_ResultSetId:
                        strDebugInfo += "ResultSetId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_resultSetStartPoint:
                        strDebugInfo += "resultSetStartPoint='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_numberOfRecordsRequested:
                        strDebugInfo += "numberOfRecordsRequested='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_PreferredRecordSyntax:
                        strDebugInfo += "PreferredRecordSyntax='" + node.GetOIDsNodeData() + "'\r\n";
                        break;
                    case z3950_query:
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }

            return 0;
        }

        static string GetStringArray(BerNode construct,
            ushort uTag)
        {
            string strDebugInfo = "";
            for (int j = 0; j < construct.ChildrenCollection.Count; j++)
            {
                BerNode child = construct.ChildrenCollection[j];
                if  (child.m_uTag == uTag)
                {
                    strDebugInfo += "'" + child.GetCharNodeData() + "', ";
                }
                else {
                    strDebugInfo += "undifine tag '" + child.m_uTag.ToString() + "', ";
                }
            }

            return strDebugInfo;
        }

        // return:
        //		-1	error
        //		0	succeed
        static int make_type_1(string strQuery,
            Encoding queryTermEncoding,
            BerNode subroot)
        {
            BerNode param = null;
            PolandNode poland = new PolandNode(strQuery);
            int i;

            param = subroot.NewChildConstructedNode(z3950_type_1,
                BerNode.ASN1_CONTEXT);
            param.NewChildOIDsNode(6,
                BerNode.ASN1_UNIVERSAL,
                "1.2.840.10003.3.1");

            poland.m_queryTermEncoding = queryTermEncoding;
            poland.ChangeOrgToRPN();

            i = poland.m_Subroot.ChildrenCollection.Count - 1;
            Debug.Assert(i >= 0, "");
            param.AddSubtree(poland.m_Subroot.ChildrenCollection[i]);
            Debug.Assert(poland.m_Subroot.ChildrenCollection[i] != null, "");

            poland.m_Subroot.ChildrenCollection.RemoveAt(i);

            return 0;
        }


        // return:
        //		-1	error
        //		0	succeed
        int make_type_101(string strQuery,
            BerNode subroot)
        {
            BerNode param = null;

            PolandNode poland = new PolandNode(strQuery);

            param = subroot.NewChildConstructedNode(z3950_type_101,
                BerNode.ASN1_CONTEXT);
            param.NewChildOIDsNode(6,
                BerNode.ASN1_UNIVERSAL,
                "1.2.840.10003.3.1");
            poland.ChangeOrgToRPN();
            /*
            int i;
            i = poland.m_Subroot.ChildrenCollection.Count - 1;
            Debug.Assert(i>=0, "");
             * */
            param.AddSubtree(poland.m_Subroot.ChildrenCollection[0]);
            poland.m_Subroot.ChildrenCollection.RemoveAt(0);

            return 0;
        }


        // 2007/7/18
        //	 build a z39.50 Init response
        public int BuildInitResponse(INIT_RESPONSE struInit_response,
            out byte[] baPackage)
        {
            baPackage = null;

            BerNode root = null;

            root = m_RootNode.NewChildConstructedNode(z3950_initResponse,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(struInit_response.m_strReferenceId) == false)
            {
                root.NewChildCharNode(z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struInit_response.m_strReferenceId));
            }

            root.NewChildBitstringNode(z3950_ProtocolVersion,   // 3
                BerNode.ASN1_CONTEXT,
                "yy");

            /* option
 * 
         search                 (0), 
         present                (1), 
         delSet                 (2),
         resourceReport         (3),
         triggerResourceCtrl    (4),
         resourceCtrl           (5), 
         accessCtrl             (6),
         scan                   (7),
         sort                   (8), 
         --                     (9) (reserved)
         extendedServices       (10),
         level-1Segmentation    (11),
         level-2Segmentation    (12),
         concurrentOperations   (13),
         namedResultSets        (14)
            15 Encapsulation  Z39.50-1995 Amendment 3: Z39.50 Encapsulation 
            16 resultCount parameter in Sort Response  See Note 8 Z39.50-1995 Amendment 1: Add resultCount parameter to Sort Response  
            17 Negotiation Model  See Note 9 Model for Z39.50 Negotiation During Initialization  
            18 Duplicate Detection See Note 1  Z39.50 Duplicate Detection Service  
            19 Query type 104 
 * }
*/
            root.NewChildBitstringNode(z3950_Options,   // 4
                BerNode.ASN1_CONTEXT,
                struInit_response.m_strOptions);    // "110000000000001"


            root.NewChildIntegerNode(z3950_PreferredMessageSize,    // 5
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struInit_response.m_lPreferredMessageSize));

            root.NewChildIntegerNode(z3950_ExceptionalRecordSize,   // 6
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struInit_response.m_lExceptionalRecordSize));

            root.NewChildCharNode(z3950_ImplementationId,   // 110
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(struInit_response.m_strImplementationId));

            root.NewChildCharNode(z3950_ImplementationName, // 111
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(struInit_response.m_strImplementationName));

            root.NewChildCharNode(z3950_ImplementationVersion,  // 112
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(struInit_response.m_strImplementationVersion));  // "3"

            // bool
            root.NewChildIntegerNode(z3950_result,  // 12
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes((long)struInit_response.m_nResult));  

            baPackage = null;
            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

#if NNOOOOOOOOOOOOOO
        //	 build a z39.50 Init response   
        public int InitResponse(INIT_RESPONSE struInit_response,
            out byte [] baPackage)
{
	BerNode root = null;
	int nTotlen;
	int nDelta=0;
	int i,nMax;

	root = this.m_RootNode.NewChildConstructedNode(0,
		ASN1_CONTEXT);

	root.BuildTreeNode(baPackage,
        ref nDelta,
        out nTotlen);

    if(pRoot->m_uTag!=z3950_initResponse)
        return NULL;
	nMax = pRoot->m_ChildArray.GetSize();

    for(i=0;i<nMax;i++)
	{
		
		switch(pRoot->m_ChildArray[i]->m_uTag)
		{
		case z3950_ReferenceId:
			pRoot->m_ChildArray[i]->GetCharNodeData(
				struInit_response.m_strReferenceId);
			break;
		case z3950_Options:
			pRoot->m_ChildArray[i]->GetBitstringNodeData(
				struInit_response.m_strOptions);
			break;
		case z3950_PreferredMessageSize:
				pRoot->m_ChildArray[i]->GetIntegerNodeData(
				struInit_response.m_lPreferredMessageSize);
			break;
		case z3950_ExceptionalRecordSize:
			pRoot->m_ChildArray[i]->GetIntegerNodeData(
				struInit_response.m_lExceptionalRecordSize);
			break;
		case z3950_result:
			pRoot->m_ChildArray[i]->GetIntegerNodeData(
				(long &)struInit_response.m_nResult);
			break;
		default:
			break;
		}
	}
	
    return 0;
}
#endif


        //  build a present request
        public int PresentRequest(PRESENT_REQUEST struPresent_request,
            out byte[] baPackage)
        {
            baPackage = null;

            BerNode root = null;
            BerNode subroot = null;
            // int nRet;

            root = this.m_RootNode.NewChildConstructedNode(z3950_presentRequest,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(struPresent_request.m_strReferenceId) == false)
            {
                root.NewChildCharNode(z3950_ReferenceId,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struPresent_request.m_strReferenceId));
            }

            root.NewChildCharNode(z3950_ResultSetId,
                BerNode.ASN1_CONTEXT,
                Encoding.UTF8.GetBytes(struPresent_request.m_strResultSetName));

            root.NewChildIntegerNode(z3950_resultSetStartPoint,
                BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes(struPresent_request.m_lResultSetStartPoint));

            root.NewChildIntegerNode(z3950_numberOfRecordsRequested,
                 BerNode.ASN1_CONTEXT,
                BitConverter.GetBytes(struPresent_request.m_lNumberOfRecordsRequested));

            if (String.IsNullOrEmpty(struPresent_request.m_strElementSetNames) == false)
            {
                subroot = root.NewChildConstructedNode(
                    z3950_ElementSetNames,
                    BerNode.ASN1_CONTEXT);
                subroot.NewChildCharNode(0,
                    BerNode.ASN1_CONTEXT,
                    Encoding.UTF8.GetBytes(struPresent_request.m_strElementSetNames));
            }

            if (String.IsNullOrEmpty(struPresent_request.m_strPreferredRecordSyntax) == false)
            {
                root.NewChildOIDsNode(z3950_PreferredRecordSyntax,
                    BerNode.ASN1_CONTEXT,
                    struPresent_request.m_strPreferredRecordSyntax);
            }
                /*
            else
            {
                root.NewChildOIDsNode(z3950_PreferredRecordSyntax,
                   BerNode.ASN1_CONTEXT,
                   MARC_SYNTAX);
            }
                 * */

            root.EncodeBERPackage(ref baPackage);

            return 0;
        }

        // 观察Present请求包
        public static int GetInfo_PresentRequest(
        BerNode root,
        out string strDebugInfo,
        out string strError)
        {
            strError = "";
            strDebugInfo = "";

            Debug.Assert(root != null, "");

            if (root.m_uTag != z3950_presentRequest)
            {
                strError = "root tag is not z3950_presentRequest";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strDebugInfo += "ReferenceID='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ResultSetId:
                        strDebugInfo += "ResultSetId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_resultSetStartPoint:
                        strDebugInfo += "resultSetStartPoint='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_numberOfRecordsRequested:
                        strDebugInfo += "numberOfRecordsRequested='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_ElementSetNames:
                        {
                            strDebugInfo += "ElementSetNames: ";
                            for (int j = 0; j < node.ChildrenCollection.Count; j++)
                            {
                                BerNode child = node.ChildrenCollection[j];
                                switch (child.m_uTag)
                                {
                                    case 0:
                                        strDebugInfo += "'" + child.GetCharNodeData() + "', ";
                                        break;
                                    default:
                                        strDebugInfo += "undifine tag '" + child.m_uTag.ToString() + "', ";
                                        break;
                                }
                            }
                            strDebugInfo += "\r\n";
                        }
                        break;
                    case z3950_PreferredRecordSyntax:
                        strDebugInfo += "PreferredRecordSyntax='" + node.GetOIDsNodeData() + "'\r\n";
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }

            return 0;
        }

        // parameters:
        //		pBERTree	已经初始化好的，包含initial响应包信息的BERTree
        //		nMask		INIT_RESPONSE结构成员掩码
        //		pInitStruct	INIT_RESPONSE结构指针。该结构用来返回信息
        // return:
        //		-1	error
        //		0	succeed
        public static int GetInfo_InitResponse(
            BerNode root,
            ref INIT_RESPONSE InitStruct,
            out string strError)
        {
            strError = "";

            Debug.Assert(root != null, "");
            Debug.Assert(InitStruct != null, "");

            /*
            Debug.Assert(tree.m_RootNode.ChildrenCollection.Count == 1);
            root = tree.m_RootNode.ChildrenCollection[0];
             * */

            if (root.m_uTag != z3950_initResponse)
            {
                strError = "root tag is not z3950_initResponse";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                            InitStruct.m_strReferenceId = node.GetCharNodeData();
                        break;
                    case z3950_Options:
                            InitStruct.m_strOptions = node.GetBitstringNodeData();
                        break;
                    case z3950_PreferredMessageSize:
                            InitStruct.m_lPreferredMessageSize = node.GetIntegerNodeData();
                        break;
                    case z3950_ExceptionalRecordSize:
                            InitStruct.m_lExceptionalRecordSize = node.GetIntegerNodeData();
                        break;
                    case z3950_result:
                            InitStruct.m_nResult = node.GetIntegerNodeData();
                        break;
                    case z3950_ImplementationId:
                        InitStruct.m_strImplementationId = node.GetCharNodeData();
                        break;
                    case z3950_ImplementationName:
                        InitStruct.m_strImplementationName = node.GetCharNodeData();
                        break;
                    case z3950_ImplementationVersion:
                        InitStruct.m_strImplementationVersion = node.GetCharNodeData();
                        break;
                    case z3950_UserInformationField:
                        {
                            /*
                            int nRet = ParseUserInformationField(
                                node,
                                ref InitStruct,
                                out strError);
                             * */

                            // 2007/7/25
                            External external = new External();
                            BerNode nodeAny = null;
                            external.Decode(node, out nodeAny);
                            if (external.m_strDirectRefenerce == "1.2.840.10003.10.1000.17.1"
                                && nodeAny != null)
                            {
                                // 解析[ANY]


                                OclcUserInfo oclc = new OclcUserInfo();
                                oclc.Decode(nodeAny);

                                InitStruct.m_lErrorCode = oclc.code;
                                if (String.IsNullOrEmpty(oclc.text) == false)
                                    InitStruct.m_strErrorMessage = oclc.text;
                                /*
                                if (String.IsNullOrEmpty(oclc.Message) == false)
                                {
                                    InitStruct.m_strErrorMessage += oclc.Message;
                                }*/
                            }
                            else
                            {
                                InitStruct.m_lErrorCode = external.m_lIndirectReference;
                                if (external.m_octectAligned != null)
                                    InitStruct.m_strErrorMessage = Encoding.UTF8.GetString(external.m_octectAligned);
                            }
                        }
                        break;
                    case z3950_OtherInformationField:
                        {
                            InitStruct.m_charNego = new CharsetNeogatiation();
                            InitStruct.m_charNego.DecodeResponse(node);
                        }
                        break;

                    default:
                        break;
                }
            }

            /*

            if (InitStruct.m_strOptions.Length > 17)
            {
                if (InitStruct.m_strOptions[17] == 'y')
                {
                    Debug.Assert(false, "willing nego");
                }
            }*/

            return 0;
        }

        static int ParseUserInformationField(
            BerNode root,
            ref INIT_RESPONSE InitStruct,
            out string strError)
        {
            strError = "";
            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode sub = root.ChildrenCollection[i];
                if (sub.m_uTag != 8)
                    continue;
                // OID 
                if (sub.ChildrenCollection.Count >= 2)
                {
                    BerNode oid = sub.ChildrenCollection[0];
                    string strOID = oid.GetOIDsNodeData();
                    if (strOID == "1.2.840.10003.10.1000.17.1")
                    {
                        // OCLC
                        return ParseOCLcUserInformation(sub.ChildrenCollection[1],
                            ref InitStruct,
                            out strError);
                    }
                    if (strOID == "1.2.840.10003.10.3")
                    {
                        // RLG
                        // UserInfo-1
                    }
                }
            }

            return 0;
        }

        static int ParseOCLcUserInformation(
    BerNode root,
    ref INIT_RESPONSE InitStruct,
    out string strError)
        {
            strError = "";
            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode sub = root.ChildrenCollection[i];

                if (sub.m_uTag == 16)
                {
                    for (int j = 0; j < sub.ChildrenCollection.Count; j++)
                    {
                        BerNode node = sub.ChildrenCollection[j];
                        if (node.m_uTag == 3)
                            InitStruct.m_lErrorCode = node.GetIntegerNodeData();
                        if (node.m_uTag == 2)
                            InitStruct.m_strErrorMessage = node.GetCharNodeData();
                    }
                }
            }
            return 0;
        }

        public static int GetInfo_InitResponse(
            BerNode root,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            Debug.Assert(root != null, "");

            if (root.m_uTag != z3950_initResponse)
            {
                strError = "root tag is not z3950_initResponse";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strDebugInfo += "ReferenceId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ProtocolVersion:
                        strDebugInfo += "ProtocolVersion='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case z3950_Options:
                        strDebugInfo += "Options='" + node.GetBitstringNodeData() + "'\r\n";
                        break;
                    case z3950_PreferredMessageSize:
                        strDebugInfo += "PreferredMessageSize='" + node.GetIntegerNodeData().ToString() + "'\r\n";
                        break;
                    case z3950_ExceptionalRecordSize:
                        strDebugInfo += "ExceptionalRecordSize='" + node.GetIntegerNodeData().ToString() + "'\r\n";
                        break;
                    case z3950_result:
                        strDebugInfo += "result='" + node.GetIntegerNodeData().ToString() + "'\r\n";
                        break;
                    case z3950_ImplementationId:
                        strDebugInfo += "ImplementationId='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ImplementationName:
                        strDebugInfo += "ImplementationName='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_ImplementationVersion:
                        strDebugInfo += "ImplementationVersion='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }

            return 0;
        }

        public static int GetInfo_closeRequest(
    BerNode root,
    ref CLOSE_REQUEST CloseStruct,
    out string strError)
        {
            strError = "";

            Debug.Assert(root != null, "");
            Debug.Assert(CloseStruct != null, "");

            if (root.m_uTag != z3950_close)
            {
                strError = "root tag is not z3950_close";
                return -1;
            }

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];
                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        CloseStruct.m_strReferenceId = node.GetCharNodeData();
                        break;
                    case 211:   // CloseReason
                        CloseStruct.m_nCloseReason = node.GetIntegerNodeData();
                        break;
                    case 3:    // diagnosticInformation
                        CloseStruct.m_strDiagnosticInformation = node.GetCharNodeData();
                        break;
                    case 4: // resourceReportFormat
                        CloseStruct.m_strResourceReportFormat = node.GetOIDsNodeData();
                        break;
                    case 5: // resourceReport
                        CloseStruct.m_strResourceReport = node.GetCharNodeData();
                        break;
                    case 201:   // 
                        // 暂时未处理
                        break;
                    default:
                        break;
                }
            }

            return 0;
        }

        /*
	long m_lReferenceId;
	long m_lResultCount;
	long m_lNumberOfRecordsReturned;
	long m_lNextResultSetPosition;
	int  m_nSearchStatus;
	long m_lResultSetStatus;
	int  m_nPresentStatus;
	long m_lErrorCode;
	CString m_strErrorMessage;
*/
        //????
        //	填充SearchResponse结构信息
        public static int GetInfo_SearchResponse(BerNode root,
                                   ref SEARCH_RESPONSE SearchStruct,
                                   bool bDebug,
                                   out string strError)
        {
            strError = "";

            string strTemp;
            long lTemp;

            Debug.Assert(root != null, "");
            Debug.Assert(SearchStruct != null, "");

            /*
            Debug.Assert(tree.m_RootNode.ChildrenCollection.Count == 1);
            root = tree.m_RootNode.ChildrenCollection[0];
             * */

            if (root.m_uTag != z3950_searchResponse)
            {
                strError = "root结点tag类型应当为z3950_searchResponse, 但是为 " +
                    root.m_uTag.ToString();
                return -1;
            }

            SearchStruct.m_diagRecords = new DiagRecords();    // 初始化诊断记录结构


            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strTemp = node.GetCharNodeData();
                        SearchStruct.m_strReferenceId = strTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "ReferenceId OCTET STRING %s" + strTemp;
                        }
                        break;
                    case z3950_resultCount:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lResultCount = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "resultCount INTEGER " + lTemp.ToString();
                        }
                        break;
                    case z3950_NumberOfRecordsReturned:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lNumberOfRecordsReturned = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "numberOfRecordsReturned INTEGER "
                            + lTemp.ToString();
                        }

                        break;
                    case z3950_NextResultSetPosition:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lNextResultSetPosition = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "nextResultSetPosition INTEGER " + lTemp.ToString();
                        }
                        break;
                    case z3950_searchStatus:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_nSearchStatus = (int)lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "searchStatus BOOLEAN " + GetTrueOrFalse(lTemp);
                        }
                        break;
                    case z3950_resultSetStatus:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lResultSetStatus = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "resultSetStatus INTEGER " + lTemp.ToString() + " " + GetResultSetPresent(lTemp);
                        }
                        break;
                    case z3950_presentStatus:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_nPresentStatus = (int)lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "PresentStatus INTEGER " + lTemp.ToString()
                                + " " + GetPresentStatus(lTemp);
                        }
                        break;
                    case z3950_nonSurrogateDiagnostic:
                        {
                            // 新增一个诊断记录结构
                            DiagFormat diag = new DiagFormat();
                            SearchStruct.m_diagRecords.Add(diag);
                            diag.Decode(node, 
                                null,   // default encoding
                                true);  // create debuginfo

                            // TODO: 如何为node的下级节点增加m_strDebugInfo信息？需要改造Decode()函数

                            /*
                            int j;
                            BerNode subroot = null;
                            BerNode obj = null;
                            subroot = node;
                            for (j = 0; j < subroot.ChildrenCollection.Count; j++)
                            {

                                obj = subroot.ChildrenCollection[j];
                                // TRACE("\ntag[%d]", pObject->m_uTag);
                                switch (obj.m_uTag)
                                {
                                    case 6: // diagnosticSetId, type OID
                                        diag.m_strDiagSetID = obj.GetOIDsNodeData();
                                        break;
                                    case 2:  // condition, type integer
                                        diag.m_nDiagCondition = obj.GetIntegerNodeData();  // m_lErrorCode
                                        if (bDebug == true)
                                        {
                                            obj.m_strDebugInfo = "integer [" + diag.m_nDiagCondition.ToString() + "]";
                                        }
                                        break;
                                    case 26:  // addinfo, visiblestring 
                                        strTemp = obj.GetCharNodeData();
                                        diag.m_strAddInfo = strTemp;    // m_strErrorMessage
                                        if (bDebug == true)
                                        {
                                            obj.m_strDebugInfo = "visiblestring [" + strTemp + "]";
                                        }
                                        break;
                                }
                            }
                             * */
                        }
                        break;
                    default:
                        break;
                }
            }
            return 0;
        }

        // 调试版本
        public static int GetInfo_SearchResponse(BerNode root,
            out string strDebugInfo,
            out string strError)
        {
            strError = "";
            strDebugInfo = "";

            /*
            string strTemp;
            long lTemp;
             * */

            Debug.Assert(root != null, "");

            if (root.m_uTag != z3950_searchResponse)
            {
                strError = "root结点tag类型应当为z3950_searchResponse, 但是为 " +
                    root.m_uTag.ToString();
                return -1;
            }


            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strDebugInfo += "ReferenceID='" + node.GetCharNodeData() + "'\r\n";
                        break;
                    case z3950_resultCount:
                        strDebugInfo += "resultCount='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_NumberOfRecordsReturned:
                        strDebugInfo += "NumberOfRecordsReturned='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_NextResultSetPosition:
                        strDebugInfo += "NextResultSetPosition='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_searchStatus:
                        strDebugInfo += "searchStatus='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_resultSetStatus:
                        strDebugInfo += "resultSetStatus='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_presentStatus:
                        strDebugInfo += "PresentStatus='" + node.GetIntegerNodeData() + "'\r\n";
                        break;
                    case z3950_nonSurrogateDiagnostic:
                        {
                            strDebugInfo += "nonSurrogateDiagnostic: ";
                            for (int j = 0; j < node.ChildrenCollection.Count; j++)
                            {
                                BerNode obj = node.ChildrenCollection[j];
                                switch (obj.m_uTag)
                                {
                                    case 6:
                                        strDebugInfo += "tag 6, ";
                                        break;
                                    case 2:  /* integer */
                                        strDebugInfo += "errorcode=" + obj.GetIntegerNodeData() + ", ";
                                        break;
                                    case 26:  /* visiblestring */
                                        strDebugInfo += "errorstring=" + obj.GetCharNodeData() + ", ";
                                        break;
                                }
                            }
                            strDebugInfo += "\r\n";
                        }
                        break;
                    default:
                        strDebugInfo += "Undefined tag = [" + node.m_uTag.ToString() + "]\r\n";
                        break;
                }
            }
            return 0;
        }

        public static int GetInfo_PresentResponse(
            BerNode root,
            ref SEARCH_RESPONSE SearchStruct,
            out RecordCollection records,
            bool bDebug,
            out string strError)
        {
            records = null;
            strError = "";

            // int i, nMax;
            string strTemp;
            long lTemp;
            int nRet;

            if (root.m_uTag != z3950_presentResponse)
            {
                strError = "root结点tag类型应当为z3950_presentResponse, 但是为 "
                    + root.m_uTag.ToString();
                return -1;
            }

            SearchStruct.m_diagRecords = new DiagRecords();    // 初始化诊断记录结构

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode node = root.ChildrenCollection[i];

                switch (node.m_uTag)
                {
                    case z3950_ReferenceId:
                        strTemp = node.GetCharNodeData();
                        SearchStruct.m_strReferenceId = strTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "ReferenceId OCTET STRING " + strTemp;
                        }
                        break;
                    case z3950_resultCount:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lResultCount = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "resultCount INTEGER " + lTemp.ToString();
                        }
                        break;
                    case z3950_NumberOfRecordsReturned:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lNumberOfRecordsReturned = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "numberOfRecordsReturned INTEGER "
                                + lTemp.ToString();
                        }
                        break;
                    case z3950_NextResultSetPosition:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lNextResultSetPosition = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "nextResultSetPosition INTEGER "
                                + lTemp.ToString();
                        }
                        break;
                    case z3950_searchStatus:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_nSearchStatus = (int)lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "searchStatus BOOLEAN " + GetTrueOrFalse(lTemp);
                        }
                        break;
                    case z3950_resultSetStatus:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_lResultSetStatus = lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "resultSetStatus INTEGER " + lTemp.ToString() + " " + GetResultSetPresent(lTemp);
                        }
                        break;
                    case z3950_presentStatus:
                        lTemp = node.GetIntegerNodeData();
                        SearchStruct.m_nPresentStatus = (int)lTemp;
                        if (bDebug == true)
                        {
                            node.m_strDebugInfo = "PresentStatus INTEGER " + lTemp.ToString() + " " + GetPresentStatus(lTemp);
                        }
                        break;
                    case z3950_nonSurrogateDiagnostic:  // 130
                        {

                            // 新增一个诊断记录结构
                            DiagFormat diag = new DiagFormat();
                            SearchStruct.m_diagRecords.Add(diag);
                            diag.Decode(node,
                                null,   // default encoding
                                true);  // create debuginfo

                            // TODO: 如何为node的下级节点增加m_strDebugInfo信息？需要改造Decode()函数

                            /*
                            BerNode subroot = null;
                            BerNode obj = null;
                            subroot = node;
                            for (int j = 0; j < subroot.ChildrenCollection.Count; j++)
                            {
                                obj = subroot.ChildrenCollection[j];
                                switch (obj.m_uTag)
                                {
                                    case 6:
                                        diag.m_strDiagSetID = obj.GetOIDsNodeData();
                                        break;
                                    case 2:  // integer
                                        lTemp = obj.GetIntegerNodeData();
                                        diag.m_nDiagCondition = lTemp;
                                        if (bDebug == true)
                                        {
                                            obj.m_strDebugInfo = "integer [" + lTemp.ToString() + "]";
                                        }
                                        break;
                                    case 26:  // visiblestring 
                                        strTemp = obj.GetCharNodeData();
                                        diag.m_strAddInfo = strTemp;
                                        if (bDebug == true)
                                        {
                                            obj.m_strDebugInfo = "visiblestring [" + strTemp + "]";
                                        }
                                        break;
                                }
                            }
                             * */
                        }
                        break;
                    case 28:
                        nRet = GetRecords(node,
                            out records,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 1)
                        {	// diag rec s
                            /*
                            ASSERT(psaRecords->GetSize() >= 3);
                            pSearchStruct->m_nDiagCondition = atoi(psaRecords->GetAt(0));
                            pSearchStruct->m_strDiagSetID = psaRecords->GetAt(1);
                            pSearchStruct->m_strAddInfo = psaRecords->GetAt(2);
                            */

                        }
                        break;
                    default:
                        break;
                }
            }
            return 0;
        }

        static int GetRecords(BerNode subroot,
            out RecordCollection records,
            out string strError)
        {
            records = null;
            strError = "";

            Debug.Assert(subroot != null, "");
            Debug.Assert(subroot.m_uTag == 28, "");

            // int i, nMax;
            BerNode obj = null;
            int nRet;
            // string strRecord;
            byte[] baRecord = null;
            string strMarcSyntaxOID;
            string strDatabaseName;
            int nDiagCondition;
            string strDiagSetID;
            string strAddInfo;

            string strLastDBName = "";
            string strLastMarcSyntaxOID = "";

            records = new RecordCollection();

            for (int i = 0; i < subroot.ChildrenCollection.Count; i++)
            {
                obj = subroot.ChildrenCollection[i];

                if (obj.m_uTag != 16)
                    continue;

                nRet = GetOneRecord(obj,
                    out strDatabaseName,
                    out strMarcSyntaxOID,
                    out baRecord,   // strRecord,
                    out nDiagCondition,
                    out strDiagSetID,
                    out strAddInfo,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    // diag rec
                    Record record = new Record();
                    record.m_nDiagCondition = nDiagCondition;
                    record.m_strDiagSetID = strDiagSetID;
                    record.m_strAddInfo = strAddInfo;
                    records.Add(record);
                }
                else
                {
                    Debug.Assert(nRet == 0, "");
                    Record record = new Record();
                    record.m_baRecord = baRecord;
                    record.m_strDBName = strDatabaseName;
                    record.m_strSyntaxOID = strMarcSyntaxOID;
                    records.Add(record);

                    // 如果一个记录的数据库名为空，则取前一个数据库名
                    if (String.IsNullOrEmpty(record.m_strDBName) == true)
                        record.m_strDBName = strLastDBName;
                    else
                        strLastDBName = record.m_strDBName;

                    // 如果一个记录的MARC OID为空，则取前一个OID
                    if (String.IsNullOrEmpty(record.m_strSyntaxOID) == true)
                        record.m_strSyntaxOID = strLastMarcSyntaxOID;
                    else
                        strLastMarcSyntaxOID = record.m_strDBName;
                }

            }

            return 0;
        }

        // parameters:
        //		strDatabaseName	[out]数据库名
        //		strMARCFormatOID	[out]MARC格式OID
        //		strRecord		[out]MARC记录体
        //		以上3个参数用于返回正常记录
        //		nDiagCondition		[out]诊断码
        //		strDiagSetID	[out]诊断集合ID
        //		strAddInfo		[out]附加信息
        //		以上3个参数用于返回诊断记录
        // return:
        //		-1	error
        //		0	normal	正常获得记录
        //		1	diag rec	诊断记录
        //						在这种情况下，strRecord为addinfo VisibleString
        //					strDatabaseName为condition INTEGER
        //					strMARCFormatOID为diagnosticSetId OBJECT IDENTIFIER,
        static int GetOneRecord(BerNode subroot,
                         out string strDatabaseName,
                         out string strMARCFormatOID,
                         out byte[] baRecord,
                         out int nDiagCondition,
                         out string strDiagSetID,
                         out string strAddInfo,
                         out string strError)
        {

            strDatabaseName = "";
            strMARCFormatOID = "";
            baRecord = null;
            nDiagCondition = 0;
            strDiagSetID = "";
            strAddInfo = "";
            strError = "";

            Debug.Assert(subroot != null, "");
            Debug.Assert(subroot.m_uTag == 16, "");

            // int i, nMax;
            BerNode obj = null;
            // int j, nMax1;
            // string strTemp;
            int nRet;

            for (int i = 0; i < subroot.ChildrenCollection.Count; i++)
            {
                obj = subroot.ChildrenCollection[i];

                if (obj.m_uTag == 0)
                {	// database name
                    strDatabaseName = obj.GetCharNodeData();
                }

                if (obj.m_uTag == 1)
                {	// choice retrievalRecord
                    BerNode subobj = null;

                    if (obj.ChildrenCollection.Count == 0)
                    {
                        strError = "choice's has no children";
                        return -1;
                    }

                    // 1
                    subobj = obj.ChildrenCollection[0];

                    if (subobj.m_uTag == 2)
                    {	// diagRec
                        if (subobj.ChildrenCollection.Count == 0)
                        {
                            strError = "choice's children has no children 1";
                            return -1;
                        }

                        subobj = subobj.ChildrenCollection[0];
                        nRet = GetOneDiagRec(subobj,
                            out nDiagCondition,
                            out strDiagSetID,
                            out strAddInfo,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        return 1;
                    }

                    if (subobj.m_uTag != 1)
                    {
                        strError = "choice's children tag must be 1, but is [" + subobj.m_uTag.ToString() + "]";
                        return -1;
                    }

                    if (subobj.ChildrenCollection.Count == 0)
                    {
                        strError = "choice's children has no children 2";
                        return -1;
                    }

                    // 8
                    subobj = subobj.ChildrenCollection[0];

                    if (subobj.m_uTag != 8)
                    {
                        strError = "tag must be 8, but is [" + subobj.m_uTag.ToString() + "]";
                        return -1;
                    }

                    if (subobj.ChildrenCollection.Count == 0)
                    {
                        strError = "8 has no children";
                        return -1;
                    }

                    // 6, 1
                    for (int j = 0; j < subobj.ChildrenCollection.Count; j++)
                    {
                        BerNode curobj = subobj.ChildrenCollection[j];

                        if (curobj.m_uTag == 6)
                        {
                            strMARCFormatOID = curobj.GetOIDsNodeData();
                        }

                        if (curobj.m_uTag == 1)
                        {
                            baRecord = curobj.m_baData;
                        }

                        // [ANY]
                        if (curobj.m_uTag == 0)
                        {
                            baRecord = GetChildGeneralStringData(curobj);
                        }
                    }

                }
            }


            return 0;
        }

        // 获得子节点中UNIVERSAL GeneralString [27]的内容
        static byte[] GetChildGeneralStringData(BerNode nodeParent)
        {
            for (int i = 0; i < nodeParent.ChildrenCollection.Count; i++)
            {
                BerNode node = nodeParent.ChildrenCollection[i];
                if (node.m_uTag == 27
                    && node.m_cClass == 0)  // ASN1_GENERALSTRING
                {
                    return node.m_baData;
                }
            }
            return null;
        }

        // 获得子节点中UNIVERSAL GeneralString [27]的内容
        static string GetChildGeneralString(BerNode nodeParent)
        {
            for (int i = 0; i < nodeParent.ChildrenCollection.Count; i++)
            {
                BerNode node = nodeParent.ChildrenCollection[i];
                if (node.m_uTag == 27
                    && node.m_cClass == 0)  // ASN1_GENERALSTRING
                {
                    return node.GetCharNodeData();
                }
            }

            return null;
        }

        static int GetOneDiagRec(BerNode subroot,
                          out int nDiagCondition,
                          out string strDiagSetID,
                          out string strAddInfo,
                          out string strError)
        {
            nDiagCondition = 0;
            strDiagSetID = "";
            strAddInfo = "";
            strError = "";

            Debug.Assert(subroot != null, "");
            Debug.Assert(subroot.m_uTag == 16, "");

            int i, nMax;
            BerNode obj = null;

            nMax = subroot.ChildrenCollection.Count;

            strDiagSetID = "";
            nDiagCondition = 0;
            strAddInfo = "";

            for (i = 0; i < nMax; i++)
            {

                obj = subroot.ChildrenCollection[i];

                if (obj.m_uTag == 6)
                {	// diag set id
                    strDiagSetID = obj.GetOIDsNodeData();
                }
                if (obj.m_uTag == 2)
                {	// condition
                    nDiagCondition = (int)obj.GetIntegerNodeData();
                }
                if (obj.m_uTag == 26)
                {	// add info
                    strAddInfo = obj.GetCharNodeData();
                }

            }

            return 0;
        }

        public static string GetPresentStatus(long lTemp)
        {
            switch (lTemp)
            {
                case 0:
                    return "success";
                case 1:
                    return "partial-1";
                case 2:
                    return "partial-2";
                case 3:
                    return "partial-3";
                case 4:
                    return "partial-4";
                case 5:
                    return "failure";
            }
            return "unknown " + lTemp.ToString();
        }

        public static string GetTrueOrFalse(long lTemp)
        {
            if (lTemp == 0)
                return "FALSE";
            else
                return "TRUE";
        }


        public static string GetResultSetPresent(long lTemp)
        {
            switch (lTemp)
            {
                case 1:
                    return "subset";
                case 2:
                    return "interim";
                case 3:
                    return "none";
            }
            return "unknown " + lTemp.ToString();
        }

    }

        /*
EXTERNAL ::= [UNIVERSAL 8] IMPLICIT SEQUENCE
    {direct-reference      OBJECT IDENTIFIER OPTIONAL,
     indirect-reference    INTEGER           OPTIONAL,
     data-value-descriptor ObjectDescriptor  OPTIONAL,
     encoding              CHOICE
        {single-ASN1-type  [0] ANY,
         octet-aligned     [1] IMPLICIT OCTET STRING,
         arbitrary         [2] IMPLICIT BIT STRING}}
     * */
    /*
[UNIVERSAL 8] IMPLICIT SEQUENCE {
    direct-reference      OBJECT IDENTIFIER OPTIONAL,
    indirect-reference    INTEGER OPTIONAL,
    data-value-descriptor ObjectDescriptor OPTIONAL,
    encoding              CHOICE {
      single-ASN1-type   [0] ABSTRACT_SYNTAX.&Type,
      octet-aligned      [1] IMPLICIT OCTET STRING,
      arbitrary          [2] IMPLICIT BIT STRING 
      }
    }

     * */
    public class External
    {
        public string m_strDirectRefenerce = "";    // OID

        public bool m_bHasIndirectReference = false;
        public long m_lIndirectReference = 0;  // integer

        public byte[] m_octectAligned = null;

        public string m_strArbitrary = ""; // bit string

        public void Clear()
        {
            m_strDirectRefenerce = "";
            m_bHasIndirectReference = false;
            m_lIndirectReference = 0;
            m_octectAligned = null;
            m_strArbitrary = "";
        }

        // 估算数据所占的包尺寸
        public int GetPackageSize()
        {
            int nSize = 0;

            if (String.IsNullOrEmpty(this.m_strDirectRefenerce) == false)
            {
                // TODO: 修改为估算OID编码后的尺寸
                nSize += Encoding.UTF8.GetByteCount(this.m_strDirectRefenerce);
            }

            if (this.m_octectAligned != null)
            {
                nSize += m_octectAligned.Length;
            }

            return nSize;
        }

        public void Build(BerNode nodeRoot)
        {
            BerNode nodeExternal = nodeRoot.NewChildConstructedNode(
                8,  // UNI_EXTERNAL
                BerNode.ASN1_UNIVERSAL);

            if (String.IsNullOrEmpty(this.m_strDirectRefenerce) == false)
            {
                nodeExternal.NewChildOIDsNode(6,   // UNI_OBJECTIDENTIFIER,
                    BerNode.ASN1_UNIVERSAL,
                    this.m_strDirectRefenerce);
            }

            if (this.m_bHasIndirectReference == true)
            {
                nodeExternal.NewChildIntegerNode(BerNode.ASN1_INTEGER,
                    BerNode.ASN1_UNIVERSAL,
                    this.m_lIndirectReference);
            }

            if (this.m_octectAligned != null
                && String.IsNullOrEmpty(this.m_strArbitrary) == false)
            {
                throw new Exception("m_octectAligned和m_strArbitrary二者只允许其中一个有非空值");
            }

            if (this.m_octectAligned != null)
            {
                // 1 条 MARC 记录
                nodeExternal.NewChildCharNode(1,
                    BerNode.ASN1_CONTEXT,
                    this.m_octectAligned);
            }

            if (String.IsNullOrEmpty(this.m_strArbitrary) == false)
            {
                nodeExternal.NewChildBitstringNode(2,
                    BerNode.ASN1_CONTEXT,
                    this.m_strArbitrary);
            }
        }

        // 从Ber数中取出数据填充本类的各成员
        public void Decode(BerNode nodeRoot,
            out BerNode nodeAny)
        {
            nodeAny = null;

            // 注意操作前需要清空本类全部成员值。否则以前遗留的值会造成误会。
            this.Clear();

            for (int i = 0; i < nodeRoot.ChildrenCollection.Count; i++)
            {
                BerNode sub = nodeRoot.ChildrenCollection[i];
                if (sub.m_uTag != 8)
                    continue;

                for (int j = 0; j < sub.ChildrenCollection.Count; j++)
                {
                    BerNode node = sub.ChildrenCollection[j];

                    if (node.m_cClass == BerNode.ASN1_UNIVERSAL)
                    {
                        switch (node.m_uTag)
                        {
                            case BerNode.ASN1_OBJECTIDENTIFIER: // 6
                                this.m_strDirectRefenerce = node.GetOIDsNodeData();
                                break;
                            case BerNode.ASN1_INTEGER:  // 2
                                this.m_lIndirectReference = node.GetIntegerNodeData();
                                this.m_bHasIndirectReference = true;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (node.m_uTag)
                        {
                            case 0: // [ANY]
                                nodeAny = node;
                                break;
                            case 1: // octets
                                this.m_octectAligned = node.GetOctetsData();
                                break;
                            case 2: // bit string
                                this.m_strArbitrary = node.GetBitstringNodeData();
                                break;
                            default:
                                break;
                        }
                    }
                }

            }

        }

    }


    /*
http://www.oclc.org/asiapacific/zhcn/support/documentation/z3950/config_guide/
userInformationField - On the Init Response message, we return the OCLC_Information Record in the userInformation field. The OID for this is 1.2.840.10003.10.1000.17.1 
OCLC-UserInformation ::= SEQUENCE {
    motd [1] IMPLICIT VisibleString,
    dblist SEQUENCE OF DBName,
    failReason [3] IMPLICIT SEQUENCE {
        diagnosticSetId OBJECT IDENTIFIER OPTIONAL,
        code [1] IMPLICIT INTEGER,
        text [2] IMPLICIT VisibleString OPTIONAL
    } OPTIONAL
}

DBName ::= [2] IMPLICIT VisibleString
     
~~~
上述格式是OCLC网站给出的。但是通过访问下列Z39.50服务器，
     zgate-test.oclc.org:7210
发现上述格式定义和实际有差别。利用google查到有这样一种修正的定义：
http://www.koders.com/noncode/fidC38762FD7B4578DBB1F848120AFD463385B75425.aspx
UserInfoFormat-oclcUserInformation
{Z39-50-userInfoFormat OCLCUserInformation (7)} DEFINITIONS ::=
BEGIN

-- $Id: oclcui.asn,v 1.1 2003/10/27 12:21:33 adam Exp $
--
-- This format is returned from the server at
--	fsz3950test.oclc.org:210
-- I found the definition at
--	http://www.oclc.org/firstsearch/documentation/z3950/config_guide.htm
--
-- I have added OPTIONAL modifiers to the `dblist' and and `code'
-- elements because they appear to be admitted from the APDU returned
-- as an Init diagnostic from fsz3950test.oclc.org:210.  Adam further
-- removed the SEQUENCE structure, changed failReason to a BOOLEAN and
-- deleted diagnosticSetId altogether, to make the ASN.1 conform to
-- what's actually returned on the wire.  Finally, I removed the
-- OPTIONAL on failReason on the advice of OCLC's Keith Neibarger
-- <neibarge@oclc.org> (although he'd also advised me, wrongly, that I
-- could remove the OPTIONAL on dblist).

OCLC-UserInformation ::= SEQUENCE {
    motd        [1] IMPLICIT VisibleString OPTIONAL,
    dblist      SEQUENCE OF DBName OPTIONAL,
    failReason  [3] IMPLICIT BOOLEAN OPTIONAL,
    code        [1] IMPLICIT INTEGER OPTIONAL,
    text        [2] IMPLICIT VisibleString OPTIONAL
}

DBName ::= [2] IMPLICIT VisibleString

END
~~~
这里有一些早期的email通信谈到过这个格式
http://www.lists.ufl.edu/cgi-bin/wa?A2=ind9506&L=z3950iw&D=0&X=5BD6A0503A7D1E026F&Y=xietao%40datatrans.com.cn&P=5282
     
     */
    public class OclcUserInfo
    {
        public string motd = "";
        public List<string> dbnames = null;

        // failReason
        public string DiagnosticSetId = ""; // OID
        public long code = 0;
        public string text = "";

        // nodeRoot是[ANY]
        public void Decode(BerNode nodeRoot)
        {
            this.dbnames = new List<string>();

            for (int i = 0; i < nodeRoot.ChildrenCollection.Count; i++)
            {
                BerNode temp = nodeRoot.ChildrenCollection[i];

                if (!
                    (temp.m_uTag == BerNode.ASN1_SEQUENCE
                    && temp.m_cClass == BerNode.ASN1_UNIVERSAL)
                    )
                    continue;

                for(int j=0;j<temp.ChildrenCollection.Count;j++)
                {
                    BerNode node = temp.ChildrenCollection[j];

                    switch (node.m_uTag)
                    {
                        case 1: // 如果为VisibleString
                            this.motd = node.GetCharNodeData();
                            break;
                            /*
                        case 1: // 如果为整数
                            this.code = node.GetIntegerNodeData();
                            break;
                             * */
                        case 2: // 修正后的text
                            this.text = node.GetCharNodeData();
                            break;
                        case 3: // failReason
                            this.code = node.GetIntegerNodeData();
                            break;
                        case BerNode.ASN1_SEQUENCE:
                            for (int k = 0; k < node.ChildrenCollection.Count; k++)
                            {
                                BerNode nodeDbName = node.ChildrenCollection[i];
                                if (nodeDbName.m_uTag == 2)
                                {
                                    this.dbnames.Add(nodeDbName.GetCharNodeData());
                                }
                            }
                            break;
                            /*
                        case 3:
                            for (int k = 0; k < node.ChildrenCollection.Count; k++)
                            {
                                BerNode sub = node.ChildrenCollection[k];
                                switch (sub.m_uTag)
                                {
                                    case BerNode.ASN1_OBJECTIDENTIFIER:
                                        this.DiagnosticSetId = sub.GetOIDsNodeData();
                                        break;
                                    case 1:
                                        this.code = sub.GetIntegerNodeData();
                                        break;
                                    case 2:
                                        this.text = sub.GetCharNodeData();
                                        break;
                                }
                            }
                            break;
                             * */
                    } // end of -- switch (node.m_uTag)


                }   // end of -- for j

            }   // end of -- for i

        }

    }

    // 字符集协商
    public class CharsetNeogatiation
    {
        public string CharNegoOID = "1.2.840.10003.15.3";
        public string EncodingLevelOID = "";
        public long RecordsInSelectedCharsets = 0;   // 0 false 1 true -1 null(未知)

        public const string Nego3OID = "1.2.840.10003.15.3";    // nego-3
        public const string Nego4OID = "1.2.840.10003.15.4";    // nego-4

        public const string Utf8OID = "1.0.10646.1.0.8";   // utf-8

        public void Clear()
        {
            this.CharNegoOID = "";
            this.EncodingLevelOID = "";
            this.RecordsInSelectedCharsets = -1;    // 未知状态
        }

        // 编码请求
        // parameters:
        //      root    InitRequest root
        public void EncodeProposal(BerNode root)
        {
            if (this.EncodingLevelOID != CharsetNeogatiation.Utf8OID)
            {
                throw new Exception("目前尚不支持请求utf-8以外的编码方式");
                return;
            }

            // SEQUENCE
            BerNode subroot = root.NewChildConstructedNode(
                BerTree.z3950_OtherInformationField,    // 201
                BerNode.ASN1_CONTEXT);

            // OF SEQUENCE
            subroot = subroot.NewChildConstructedNode(
                BerNode.ASN1_SEQUENCE,    // 16
                BerNode.ASN1_UNIVERSAL);

            // externallyDefineInfo(of OtherInformation)
            subroot = subroot.NewChildConstructedNode(
                4,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(this.CharNegoOID) == true)
                throw new Exception("CharNegoOID 不能为空");


            // oid
            subroot.NewChildOIDsNode(
                6,   // UNI_OBJECTIDENTIFIER,
                BerNode.ASN1_UNIVERSAL,
                this.CharNegoOID);  // "1.2.840.10003.15.3" nego-3

            // http://www.loc.gov/z3950/agency/defns/charneg-3.html
            // http://www.loc.gov/z3950/agency/defns/charneg-4.html

            // originProposal SEQUENCE
            subroot = subroot.NewChildConstructedNode(
                1,
                BerNode.ASN1_CONTEXT);

            // proposedCharSets SEQUENCE OF CHOICE
            BerNode proposedCharSets = subroot.NewChildConstructedNode(
                1,
                BerNode.ASN1_CONTEXT);

            if (this.EncodingLevelOID == CharsetNeogatiation.Utf8OID)
            {

                // iso10646 SEQUENCE
                BerNode temp_sub = proposedCharSets.NewChildConstructedNode(
                    2,  // iso10646
                    BerNode.ASN1_CONTEXT);

                Debug.Assert(String.IsNullOrEmpty(this.EncodingLevelOID) == false,
                    "EncodingLevelOID 不能为空");

                if (String.IsNullOrEmpty(this.EncodingLevelOID) == true)
                    throw new Exception("EncodingLevelOID 不能为空");


                // encodingLevel
                temp_sub.NewChildOIDsNode(
                    2,  // encodingLevel
                    BerNode.ASN1_CONTEXT,
                    this.EncodingLevelOID); // "1.0.10646.1.0.8" utf-8
                //
                //
                //          -- oid of form 1.0.10646.1.0.form
                //    -- where value of 'form' is 2, 4, 5, or 8 
                //    -- for ucs-2, ucs-4, utf-16, utf-8
                //
            }
            else
            {
                throw new Exception("目前尚不支持请求utf-8以外的编码方式");
            }

            if (this.RecordsInSelectedCharsets == -1)
                throw new Exception("RecordsInSelectedCharsets在编码前必须为0/1之一，不能为-1");

            // recordsInSelectedCharSets
            if (this.RecordsInSelectedCharsets == 1)
            {
                subroot.NewChildIntegerNode(3,
                    BerNode.ASN1_CONTEXT,
                    this.RecordsInSelectedCharsets);
            }

        }

        // 解码请求
        // parameters:
        //      root    OtherInformationField root
        public void DecodeProposal(BerNode root)
        {
            this.Clear();

            BerNode nodeOriginProposal = null;

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode sequence = root.ChildrenCollection[i];

                if (!
                    (sequence.m_uTag == 16
                    && sequence.m_cClass == BerNode.ASN1_UNIVERSAL)
                    )
                    continue;

                // externallyDefineInfo
                for (int j = 0; j < sequence.ChildrenCollection.Count; j++)
                {
                    BerNode externally = sequence.ChildrenCollection[j];

                    if (externally.m_uTag != 4)
                        continue;

                    // oid and originProposal
                    for (int k = 0; k < externally.ChildrenCollection.Count; k++)
                    {
                        BerNode node = externally.ChildrenCollection[k];

                        if (node.m_uTag == 6)
                        {
                            string strOID = node.GetOIDsNodeData();
                            if (!
                                (strOID == "1.2.840.10003.15.3"
                                || strOID == "1.2.840.10003.15.4")
                                )
                                break;
                            this.CharNegoOID = strOID;
                        }

                        if (node.m_uTag == 1)
                        {
                            nodeOriginProposal = node;
                            goto FOUND_ORIGIN_PROPOSAL;
                        }
                    }
                }

            }

            return;

        FOUND_ORIGIN_PROPOSAL:
            Debug.Assert(nodeOriginProposal != null, "");

            for (int i = 0; i < nodeOriginProposal.ChildrenCollection.Count; i++)
            {
                BerNode node = nodeOriginProposal.ChildrenCollection[i];

                // recordsInSelectedCharsets
                if (node.m_uTag == 3)
                {
                    this.RecordsInSelectedCharsets = node.GetIntegerNodeData();
                    Debug.Assert(this.RecordsInSelectedCharsets != -1, "");
                    Debug.Assert(this.RecordsInSelectedCharsets == 1
                    || this.RecordsInSelectedCharsets == 2, "");
                }

                // proposalCharSets
                if (node.m_uTag == 1)
                {
                    for (int j = 0; j < node.ChildrenCollection.Count; j++)
                    {
                        BerNode sub = node.ChildrenCollection[j];

                        if (sub.m_uTag == 2)   // iso10646
                        {
                            for (int k = 0; k < sub.ChildrenCollection.Count; k++)
                            {
                                BerNode temp = sub.ChildrenCollection[k];

                                if (temp.m_uTag == 2)   // encoding level
                                {
                                    this.EncodingLevelOID = temp.GetOIDsNodeData();
                                }
                            }
                        }
                    }
                } // end of if // proposalCharSets
            }

        }


        // 编码响应
        // parameters:
        //      root    InitResponse root
        public void EncodeResponse(BerNode root)
        {
            if (this.EncodingLevelOID != CharsetNeogatiation.Utf8OID)
            {
                // 对于非UTF-8编码方式，不答复，就表示不同意。维持缺省的编码方式
                return;
            }

            // SEQUENCE
            BerNode subroot = root.NewChildConstructedNode(
                BerTree.z3950_OtherInformationField,    // 201
                BerNode.ASN1_CONTEXT);

            // OF SEQUENCE
            subroot = subroot.NewChildConstructedNode(
                BerNode.ASN1_SEQUENCE,    // 16
                BerNode.ASN1_UNIVERSAL);

            // externallyDefineInfo(of OtherInformation)
            subroot = subroot.NewChildConstructedNode(
                4,
                BerNode.ASN1_CONTEXT);

            if (String.IsNullOrEmpty(this.CharNegoOID) == true)
                throw new Exception("CharNegoOID 不能为空");


            // oid
            subroot.NewChildOIDsNode(
                6,   // UNI_OBJECTIDENTIFIER,
                BerNode.ASN1_UNIVERSAL,
                this.CharNegoOID);  // "1.2.840.10003.15.3" nego-3

            // http://www.loc.gov/z3950/agency/defns/charneg-3.html
            // http://www.loc.gov/z3950/agency/defns/charneg-4.html

            // targetResponse SEQUENCE
            subroot = subroot.NewChildConstructedNode(
                2,
                BerNode.ASN1_CONTEXT);

            // selectedCharSets CHOICE
            BerNode selectedCharSets = subroot.NewChildConstructedNode(
                1,
                BerNode.ASN1_CONTEXT);

            if (this.EncodingLevelOID == CharsetNeogatiation.Utf8OID)
            {

                // iso10646 SEQUENCE
                BerNode temp_sub = selectedCharSets.NewChildConstructedNode(
                    2,  // iso10646
                    BerNode.ASN1_CONTEXT);

                Debug.Assert(String.IsNullOrEmpty(this.EncodingLevelOID) == false,
                    "EncodingLevelOID 不能为空");

                if (String.IsNullOrEmpty(this.EncodingLevelOID) == true)
                    throw new Exception("EncodingLevelOID 不能为空");

                // encodingLevel
                temp_sub.NewChildOIDsNode(
                    2,  // encodingLevel
                    BerNode.ASN1_CONTEXT,
                    this.EncodingLevelOID); // "1.0.10646.1.0.8" utf-8
                //
                //
                //          -- oid of form 1.0.10646.1.0.form
                //    -- where value of 'form' is 2, 4, 5, or 8 
                //    -- for ucs-2, ucs-4, utf-16, utf-8
                //
            } // end if utf-8
            else
            {
                throw new Exception("目前尚不支持返回utf-8以外的编码方式");
            }

            // -1表示不响应，1表示响应true, 0表示响应false
            // 按照Z39.50定义，要看Proposal中有没有这个部件。如果没有，就不要在响应中包含对等部件。
            if (this.RecordsInSelectedCharsets != -1)
            {
                // recordsInSelectedCharSets
                subroot.NewChildIntegerNode(3,
                        BerNode.ASN1_CONTEXT,
                        this.RecordsInSelectedCharsets);
            }
        }


        // 解码响应
        // parameters:
        //      root    OtherInformationField root
        public void DecodeResponse(BerNode root)
        {
            this.Clear();

            BerNode nodeTargetResponse = null;

            for (int i = 0; i < root.ChildrenCollection.Count; i++)
            {
                BerNode sequence = root.ChildrenCollection[i];

                if (!
                    (sequence.m_uTag == 16
                    && sequence.m_cClass == BerNode.ASN1_UNIVERSAL)
                    )
                    continue;

                // externallyDefineInfo
                for (int j = 0; j < sequence.ChildrenCollection.Count; j++)
                {
                    BerNode externally = sequence.ChildrenCollection[j];

                    if (externally.m_uTag != 4)
                        continue;

                    // oid and originProposal
                    for (int k = 0; k < externally.ChildrenCollection.Count; k++)
                    {
                        BerNode node = externally.ChildrenCollection[k];

                        if (node.m_uTag == 6)
                        {
                            string strOID = node.GetOIDsNodeData();
                            if (!
                                (strOID == "1.2.840.10003.15.3"
                                || strOID == "1.2.840.10003.15.4")
                                )
                                break;
                            this.CharNegoOID = strOID;
                        }

                        if (node.m_uTag == 2)
                        {
                            nodeTargetResponse = node;
                            goto FOUND_TARGET_RESPONSE;
                        }
                    }
                }

            }

            return;

        FOUND_TARGET_RESPONSE:
            Debug.Assert(nodeTargetResponse != null, "");

            for (int i = 0; i < nodeTargetResponse.ChildrenCollection.Count; i++)
            {
                BerNode node = nodeTargetResponse.ChildrenCollection[i];

                // recordsInSelectedCharsets
                if (node.m_uTag == 3)
                {
                    this.RecordsInSelectedCharsets = node.GetIntegerNodeData();
                    Debug.Assert(this.RecordsInSelectedCharsets != -1, "");
                    Debug.Assert(this.RecordsInSelectedCharsets == 1
                    || this.RecordsInSelectedCharsets == 2, "");
                }

                // selectedCharSets
                if (node.m_uTag == 1)
                {
                    for (int j = 0; j < node.ChildrenCollection.Count; j++)
                    {
                        BerNode sub = node.ChildrenCollection[j];


                        if (sub.m_uTag == 2)   // iso10646
                        {
                            for (int k = 0; k < sub.ChildrenCollection.Count; k++)
                            {
                                BerNode temp = sub.ChildrenCollection[k];

                                if (temp.m_uTag == 2)   // encoding level
                                {
                                    this.EncodingLevelOID = temp.GetOIDsNodeData();
                                }
                            }
                        }
                    } // end of for
                } // end of if selectedCharSets
            }

        }

    }

}
