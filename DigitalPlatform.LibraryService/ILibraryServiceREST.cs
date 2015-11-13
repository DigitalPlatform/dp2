using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using System.Net.Security;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace dp2Library
{
    [ServiceContract(
Name = "dp2libraryREST",
Namespace = "http://dp2003.com/dp2library/rest",
SessionMode = SessionMode.NotAllowed)]
    public interface ILibraryServiceREST
    {
        [OperationContract]
        /*
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
         * */
        LibraryServerResult GetVersion(out string uid);

        [OperationContract]
        /*
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "login?username={strUserName}&password={strPassword}&parameters={strParameters}")]
         * */
        LibraryServerResult Login(
            string strUserName,
            string strPassword,
            string strParameters,
            out string strOutputUserName,
            out string strRights,
            out string strLibraryCode);

        [OperationContract]
        /*
        [WebInvoke(Method = "GET",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
         * */
        LibraryServerResult Logout();

        [OperationContract]
        LibraryServerResult SetLang(string strLang,
            out string strOldLang);

        [OperationContract(IsOneWay = true)]
        void Stop();

        [OperationContract]
        LibraryServerResult VerifyReaderPassword(
            string strReaderBarcode,
            string strReaderPassword);

        [OperationContract]
        LibraryServerResult ChangeReaderPassword(
            string strReaderBarcode,
            string strReaderOldPassword,
            string strReaderNewPassword);

        [OperationContract]
        LibraryServerResult GetReaderInfo(
            string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp);

        [OperationContract]
        LibraryServerResult SetReaderInfo(
            string strAction,
            string strRecPath,
            string strNewXml,
            string strOldXml,
            byte[] baOldTimestamp,
            out string strExistingXml,
            out string strSavedXml,
            out string strSavedRecPath,
            out byte[] baNewTimestamp,
            out DigitalPlatform.rms.Client.rmsws_localhost.ErrorCodeValue kernel_errorcode);

        [OperationContract]
        LibraryServerResult MoveReaderInfo(
            string strSourceRecPath,
            ref string strTargetRecPath,
            out byte[] target_timestamp);

        [OperationContract]
        LibraryServerResult DevolveReaderInfo(
            string strSourceReaderBarcode,
            string strTargetReaderBarcode);

        [OperationContract]
        LibraryServerResult SearchReader(
            string strReaderDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strOutputStyle);

        [OperationContract]
        LibraryServerResult SetFriends(
    string strAction,
    string strReaderBarcode,
    string strComment,
    string strStyle);

        [OperationContract]
        LibraryServerResult SearchOneDb(
            string strQueryWord,
            string strDbName,
            string strFrom,
            string strMatchStyle,
            string strLang,
            long lMaxCount,
            string strResultSetName,
            string strOutputStyle);

        [OperationContract]
        LibraryServerResult Search(
            string strQueryXml,
            string strResultSetName,
            string strOutputStyle);

        [OperationContract]
        LibraryServerResult GetSearchResult(
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out Record[] searchresults);

        [OperationContract]
        LibraryServerResult GetRecord(
            string strPath,
            out byte[] timestamp,
            out string strXml);

        [OperationContract]
        LibraryServerResult GetBrowseRecords(
            string[] paths,
            string strBrowseInfoStyle,
            out Record[] searchresults);

        [OperationContract]
        LibraryServerResult ListBiblioDbFroms(
            string strDbType,
            string strLang,
            out BiblioDbFromInfo[] infos);

        [OperationContract]
        LibraryServerResult SearchBiblio(
            string strBiblioDbNames,
            string strQueryWord,
            int nPerMax,
            string strFromStyle,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle,
            out string strQueryXml);

        [OperationContract]
        LibraryServerResult SetBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp);

        [OperationContract]
        LibraryServerResult CopyBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strNewBiblioRecPath,
            string strNewBiblio,
            string strMergeStyle,
            out string strOutputBiblio,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp);

        [OperationContract]
        LibraryServerResult GetBiblioInfo(
            string strBiblioRecPath,
            string strBiblioXml,
            string strBiblioType,
            out string strBiblio);

        [OperationContract]
        LibraryServerResult GetBiblioInfos(
            string strBiblioRecPath,
            string strBiblioXml,    // 2013/3/6
            string[] formats,
            out string[] results,
            out byte[] baTimestamp);

        [OperationContract]
        LibraryServerResult SearchItem(
            string strItemDbName,   // 2007/9/25
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strSearchStyle,
            string strOutputStyle);

        [OperationContract]
        LibraryServerResult GetItemInfo(
            string strItemDbType,
            string strBarcode,
            string strItemXml,
            string strResultType,
            out string strResult,
            out string strItemRecPath,
            out byte[] item_timestamp,
            string strBiblioType,
            out string strBiblio,
            out string strBiblioRecPath);


        // *** 此API已经废止 ***
        [OperationContract]
        LibraryServerResult SearchItemDup(string strBarcode,
                   int nMax,
                   out string[] paths);

        [OperationContract]
        LibraryServerResult GetBiblioSummary(
                    string strItemBarcode,
                    string strConfirmItemRecPath,
                    string strBiblioRecPathExclude,
                    out string strBiblioRecPath,
                    out string strSummary);

        [OperationContract]
        LibraryServerResult Borrow(
                    bool bRenew,
                    string strReaderBarcode,
                    string strItemBarcode,
                    string strConfirmItemRecPath,
                    bool bForce,
                    string[] saBorrowedItemBarcode,
                    string strStyle,
                    string strItemFormatList,
                    out string[] item_records,
                    string strReaderFormatList,
                    out string[] reader_records,
                    string strBiblioFormatList,
                    out string[] biblio_records,
                    out BorrowInfo borrow_info,
                    out string[] aDupPath,
                    out string strOutputReaderBarcode);


        [OperationContract]
        LibraryServerResult Return(
                    string strAction,
                    string strReaderBarcode,
                    string strItemBarcode,
                    string strComfirmItemRecPath,
                    bool bForce,
                    string strStyle,
                    string strItemFormatList,
                    out string[] item_records,
                    string strReaderFormatList,
                    out string[] reader_records,
                    string strBiblioFormatList,
                    out string[] biblio_records,
                    out string[] aDupPath,
                    out string strOutputReaderBarcode,
                    out ReturnInfo return_info);

        [OperationContract]
        LibraryServerResult Reservation(
                    string strFunction,
                    string strReaderBarcode,
                    string strItemBarcodeList);

        [OperationContract]
        LibraryServerResult Amerce(
                    string strFunction,
                    string strReaderBarcode,
                    AmerceItem[] amerce_items,
                    out AmerceItem[] failed_items,  // 2011/6/27
                    out string strReaderXml);

        [OperationContract]
        LibraryServerResult GetIssues(
                    string strBiblioRecPath,
                    long lStart,
                    long lCount,
                    string strStyle,
                    string strLang,
                    out EntityInfo[] issueinfos);

        [OperationContract]
        LibraryServerResult SetIssues(
                    string strBiblioRecPath,
                    EntityInfo[] issueinfos,
                    out EntityInfo[] errorinfos);

        [OperationContract]
        LibraryServerResult GetIssueInfo(
                    string strRefID,
            // string strBiblioRecPath,
            string strItemXml,
                    string strResultType,
                    out string strResult,
                    out string strIssueRecPath,
                    out byte[] issue_timestamp,
                    string strBiblioType,
                    out string strBiblio,
                    out string strOutputBiblioRecPath);

        // *** 此API已经废止 ***
        [OperationContract]
        LibraryServerResult SearchIssueDup(string strPublishTime,
                    string strBiblioRecPath,
                    int nMax,
                    out string[] paths);

        [OperationContract]
        LibraryServerResult SearchIssue(
                    string strIssueDbName,
                    string strQueryWord,
                    int nPerMax,
                    string strFrom,
                    string strMatchStyle,
                    string strLang,
                    string strResultSetName,
                    string strSearchStyle,
                    string strOutputStyle);

        [OperationContract]
        LibraryServerResult GetEntities(
                   string strBiblioRecPath,
                   long lStart,
                   long lCount,
                   string strStyle,
                   string strLang,
                   out EntityInfo[] entityinfos);

        [OperationContract]
        LibraryServerResult SetEntities(
                    string strBiblioRecPath,
                    EntityInfo[] entityinfos,
                    out EntityInfo[] errorinfos);

        [OperationContract]
        LibraryServerResult GetOrders(
                    string strBiblioRecPath,
                    long lStart,
                    long lCount,
                    string strStyle,
                    string strLang,
                    out EntityInfo[] orderinfos);

        [OperationContract]
        LibraryServerResult SetOrders(
                   string strBiblioRecPath,
                   EntityInfo[] orderinfos,
                   out EntityInfo[] errorinfos);

        [OperationContract]
        LibraryServerResult GetOrderInfo(
                    string strRefID,
            // string strBiblioRecPath,
            string strItemXml,
                    string strResultType,
                    out string strResult,
                    out string strOrderRecPath,
                    out byte[] order_timestamp,
                    string strBiblioType,
                    out string strBiblio,
                    out string strOutputBiblioRecPath);

        // *** 此API已经废止 ***
        [OperationContract]
        LibraryServerResult SearchOrderDup(string strIndex,
                   string strBiblioRecPath,
                   int nMax,
                   out string[] paths);

        [OperationContract]
        LibraryServerResult SearchOrder(
                    string strOrderDbName,
                    string strQueryWord,
                    int nPerMax,
                    string strFrom,
                    string strMatchStyle,
                    string strLang,
                    string strResultSetName,
                    string strSearchStyle,
                    string strOutputStyle);

        [OperationContract]
        LibraryServerResult SetClock(string strTime);

        [OperationContract]
        LibraryServerResult GetClock(out string strTime);

        [OperationContract]
        LibraryServerResult ResetPassword(string strParameters,
            string strMessageTemplate);

        [OperationContract]
        LibraryServerResult GetValueTable(
                    string strTableName,
                    string strDbName,
                    out string[] values);

        [OperationContract]
        LibraryServerResult GetOperLogs(
            string strFileName,
            long lIndex,
            long lHint,
            int nCount,
            string strStyle,
            string strFilter,
            out OperLogInfo[] records);

        [OperationContract]
        LibraryServerResult GetOperLog(
                    string strFileName,
                    long lIndex,
                    long lHint,
                    string strStyle,
                    string strFilter,
                    out string strXml,
                    out long lHintNext,
                    long lAttachmentFragmentStart,
                    int nAttachmentFragmentLength,
                    out byte[] attachment_data,
                    out long lAttachmentTotalLength);

        [OperationContract]
        LibraryServerResult GetCalendar(
                   string strAction,
                   string strName,
                   int nStart,
                   int nCount,
                   out CalenderInfo[] contents);

        [OperationContract]
        LibraryServerResult SetCalendar(
                    string strAction,
                    CalenderInfo info);

        [OperationContract]
        LibraryServerResult BatchTask(
                   string strName,
                   string strAction,
                   BatchTaskInfo info,
                   out BatchTaskInfo resultInfo);

        [OperationContract]
        LibraryServerResult ClearAllDbs();

        [OperationContract]
        LibraryServerResult ManageDatabase(string strAction,
                    string strDatabaseName,
                    string strDatabaseInfo,
                    out string strOutputInfo);

        [OperationContract]
        LibraryServerResult GetUser(
                    string strAction,
                    string strName,
                    int nStart,
                    int nCount,
                    out UserInfo[] contents);

        [OperationContract]
        LibraryServerResult SetUser(
                   string strAction,
                   UserInfo info);

        [OperationContract]
        LibraryServerResult GetChannelInfo(
                    string strQuery,
                    string strStyle,
                    int nStart,
                    int nCount,
                    out ChannelInfo[] contents);

        [OperationContract]
        LibraryServerResult ManageChannel(
    string strAction,
    string strStyle,
    ChannelInfo[] requests,
    out ChannelInfo[] results);

        [OperationContract]
        LibraryServerResult ChangeUserPassword(
                    string strUserName,
                    string strOldPassword,
                    string strNewPassword);

        [OperationContract]
        LibraryServerResult VerifyBarcode(
            string strLibraryCode,
            string strBarcode);

        [OperationContract]
        LibraryServerResult GetSystemParameter(
                    string strCategory,
                    string strName,
                    out string strValue);

        [OperationContract]
        LibraryServerResult SetSystemParameter(
                    string strCategory,
                    string strName,
                    string strValue);

        [OperationContract]
        LibraryServerResult UrgentRecover(
                   string strXML);

        [OperationContract]
        LibraryServerResult RepairBorrowInfo(
                   string strAction,
                   string strReaderBarcode,
                   string strItemBarcode,
                   string strConfirmItemRecPath,
                   int nStart,
                   int nCount,
                   out int nProcessedBorrowItems,
                   out int nTotalBorrowItems,
                   out string strOutputReaderBarcode,
                   out string[] aDupPath);

        [OperationContract]
        LibraryServerResult PassGate(
                    string strReaderBarcode,
                    string strGateName,
                    string strResultTypeList,
                    out string[] results);

        [OperationContract]
        LibraryServerResult Foregift(
                    string strAction,
                    string strReaderBarcode,
                    out string strOutputReaderXml,
                    out string strOutputID);

        [OperationContract]
        LibraryServerResult Hire(
                    string strAction,
                    string strReaderBarcode,
                    out string strOutputReaderXml,
                    out string strOutputID);

        [OperationContract]
        LibraryServerResult Settlement(
                    string strAction,
                    string[] ids);

        [OperationContract]
        LibraryServerResult SearchOneClassCallNumber(
                    string strArrangeGroupName,
                    string strClass,
                    string strResultSetName,
                    out string strQueryXml);

        [OperationContract]
        LibraryServerResult GetCallNumberSearchResult(
                    string strArrangeGroupName,
                    string strResultSetName,
                    long lStart,
                    long lCount,
                    string strBrowseInfoStyle,
                    string strLang,
                    out CallNumberSearchResult[] searchresults);

        [OperationContract]
        LibraryServerResult GetOneClassTailNumber(
                   string strArrangeGroupName,
                   string strClass,
                   out string strTailNumber);

        [OperationContract]
        LibraryServerResult SetOneClassTailNumber(
                   string strAction,
                   string strArrangeGroupName,
                   string strClass,
                   string strTestNumber,
                   out string strOutputNumber);

        [OperationContract]
        LibraryServerResult SearchUsedZhongcihao(
                   string strZhongcihaoGroupName,
                   string strClass,
                   string strResultSetName,
                   out string strQueryXml);

        [OperationContract]
        LibraryServerResult GetZhongcihaoSearchResult(
                   string strZhongcihaoGroupName,
                   string strResultSetName,
                   long lStart,
                   long lCount,
                   string strBrowseInfoStyle,
                   string strLang,
                   out ZhongcihaoSearchResult[] searchresults);

        [OperationContract]
        LibraryServerResult GetZhongcihaoTailNumber(
                    string strZhongcihaoGroupName,
                    string strClass,
                    out string strTailNumber);

        [OperationContract]
        LibraryServerResult SetZhongcihaoTailNumber(
                   string strAction,
                   string strZhongcihaoGroupName,
                   string strClass,
                   string strTestNumber,
                   out string strOutputNumber);

        [OperationContract]
        LibraryServerResult SearchDup(
                   string strOriginBiblioRecPath,
                   string strOriginBiblioRecXml,
                   string strProjectName,
                   string strStyle,
                   out string strUsedProjectName);

        [OperationContract]
        LibraryServerResult GetDupSearchResult(
                    long lStart,
                    long lCount,
                    string strBrowseInfoStyle,
                    out DupSearchResult[] searchresults);

        [OperationContract]
        LibraryServerResult ListDupProjectInfos(
                    string strOriginBiblioDbName,
                    out DupProjectInfo[] results);

        [OperationContract]
        LibraryServerResult GetUtilInfo(
                   string strAction,
                   string strDbName,
                   string strFrom,
                   string strKey,
                   string strValueAttrName,
                   out string strValue);

        [OperationContract]
        LibraryServerResult SetUtilInfo(
                    string strAction,
                    string strDbName,
                    string strFrom,
                    string strRootElementName,
                    string strKeyAttrName,
                    string strValueAttrName,
                    string strKey,
                    string strValue);

        [OperationContract]
        LibraryServerResult GetRes(string strResPath,
                    long nStart,
                    int nLength,
                    string strStyle,
                    out byte[] baContent,
                    out string strMetadata,
                    out string strOutputResPath,
                    out byte[] baOutputTimestamp);

        [OperationContract]
        LibraryServerResult WriteRes(
                   string strResPath,
                   string strRanges,
                   long lTotalLength,
                   byte[] baContent,
                   string strMetadata,
                   string strStyle,
                   byte[] baInputTimestamp,
                   out string strOutputResPath,
                   out byte[] baOutputTimestamp);

        [OperationContract]
        LibraryServerResult GetComments(
                   string strBiblioRecPath,
                   long lStart,
                   long lCount,
                   string strStyle,
                   string strLang,
                   out EntityInfo[] commentinfos);

        [OperationContract]
        LibraryServerResult SetComments(
                    string strBiblioRecPath,
                    EntityInfo[] commentinfos,
                    out EntityInfo[] errorinfos);

        [OperationContract]
        LibraryServerResult GetCommentInfo(
                    string strRefID,
            // string strBiblioRecPath,
            string strItemXml,
                    string strResultType,
                    out string strResult,
                    out string strCommentRecPath,
                    out byte[] comment_timestamp,
                    string strBiblioType,
                    out string strBiblio,
                    out string strOutputBiblioRecPath);

        // *** 此API已经废止 ***
        [OperationContract]
        LibraryServerResult SearchCommentDup(string strIndex,
                    string strBiblioRecPath,
                    int nMax,
                    out string[] paths);

        [OperationContract]
        LibraryServerResult SearchComment(
                    string strCommentDbName,
                    string strQueryWord,
                    int nPerMax,
                    string strFrom,
                    string strMatchStyle,
                    string strLang,
                    string strResultSetName,
                    string strSearchStyle,
                    string strOutputStyle);

        [OperationContract]
        LibraryServerResult GetMessage(
    string[] message_ids,
    MessageLevel messagelevel,
    out List<MessageData> messages);

        [OperationContract]
        LibraryServerResult ListMessage(
    string strStyle,
    string strResultsetName,
    string strBoxType,
    MessageLevel messagelevel,
    int nStart,
    int nCount,
    out int nTotalCount,
    out List<MessageData> messages);


        [OperationContract]
        LibraryServerResult SetMessage(string strAction,
    string strStyle,
    List<MessageData> messages,
    out List<MessageData> output_messages);

        [OperationContract]
        LibraryServerResult GetStatisInfo(string strDateRangeString,
            string strStyle,
            out RangeStatisInfo info,
            out string strXml);

        [OperationContract]
        LibraryServerResult ExistStatisInfo(string strDateRangeString,
            out List<DateExist> dates);

        [OperationContract]
        LibraryServerResult GetFile(
    string strCategory,
    string strFileName,
    long lStart,
    long lLength,
    out byte[] baContent,
    out string strFileTime);

        [OperationContract]
        LibraryServerResult ListFile(
            string strAction,
            string strCategory,
            string strFileName,
            long lStart,
            long lLength,
            out List<FileItemInfo> infos);

        [OperationContract]
        LibraryServerResult HitCounter(string strAction,
            string strName);

    }
}
