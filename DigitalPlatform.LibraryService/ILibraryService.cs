using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Net.Security;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Message;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace dp2Library
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService1”。
    [ServiceContract(
        Name = "dp2library",
        Namespace = "http://dp2003.com/dp2library/",
        SessionMode = SessionMode.Required/*,
        ProtectionLevel = ProtectionLevel.None*/)]
    // [ServiceContract(Session = true)]
    public interface ILibraryService
    {
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetVersion(out string uid);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Login(
            string strUserName,
            string strPassword,
            string strParameters,
            out string strOutputUserName,
            out string strRights,
            out string strLibraryCode);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Logout();

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetLang(string strLang,
            out string strOldLang);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void Stop();

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult VerifyReaderPassword(string strReaderBarcode,
            string strReaderPassword);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ChangeReaderPassword(string strReaderBarcode,
            string strReaderOldPassword,
            string strReaderNewPassword);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetReaderInfo(string strBarcode,
            string strResultTypeList,
            out string[] results,
            out string strRecPath,
            out byte[] baTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult MoveReaderInfo(
            string strSourceRecPath,
            ref string strTargetRecPath,
            out byte[] target_timestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult DevolveReaderInfo(
            string strSourceReaderBarcode,
            string strTargetReaderBarcode);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchReader(
            string strReaderDbNames,
            string strQueryWord,
            int nPerMax,
            string strFrom,
            string strMatchStyle,
            string strLang,
            string strResultSetName,
            string strOutputStyle);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetFriends(
    string strAction,
    string strReaderBarcode,
    string strComment,
    string strStyle);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchOneDb(
            string strQueryWord,
            string strDbName,
            string strFrom,
            string strMatchStyle,
            string strLang,
            long lMaxCount,
            string strResultSetName,
            string strOutputStyle);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Search(
            string strQueryXml,
            string strResultSetName,
            string strOutputStyle);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetSearchResult(
            string strResultSetName,
            long lStart,
            long lCount,
            string strBrowseInfoStyle,
            string strLang,
            out Record[] searchresults);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetRecord(
            string strPath,
            out byte[] timestamp,
            out string strXml);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetBrowseRecords(
            string[] paths,
            string strBrowseInfoStyle,
            out Record[] searchresults);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ListBiblioDbFroms(
            string strDbType,
            string strLang,
            out BiblioDbFromInfo[] infos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetBiblioInfo(
            string strAction,
            string strBiblioRecPath,
            string strBiblioType,
            string strBiblio,
            byte[] baTimestamp,
            string strComment,
            out string strOutputBiblioRecPath,
            out byte[] baOutputTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetBiblioInfo(
            string strBiblioRecPath,
            string strBiblioXml,
            string strBiblioType,
            out string strBiblio);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetBiblioInfos(
            string strBiblioRecPath,
            string strBiblioXml,    // 2013/3/6
            string[] formats,
            out string[] results,
            out byte[] baTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchItemDup(string strBarcode,
                   int nMax,
                   out string[] paths);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetBiblioSummary(
                    string strItemBarcode,
                    string strConfirmItemRecPath,
                    string strBiblioRecPathExclude,
                    out string strBiblioRecPath,
                    out string strSummary);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Reservation(
                    string strFunction,
                    string strReaderBarcode,
                    string strItemBarcodeList);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Amerce(
                    string strFunction,
                    string strReaderBarcode,
                    AmerceItem[] amerce_items,
                    out AmerceItem[] failed_items,  // 2011/6/27
                    out string strReaderXml);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetIssues(
                    string strBiblioRecPath,
                    long lStart,
                    long lCount,
                    string strStyle,
                    string strLang,
                    out EntityInfo[] issueinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetIssues(
                    string strBiblioRecPath,
                    EntityInfo[] issueinfos,
                    out EntityInfo[] errorinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchIssueDup(string strPublishTime,
                    string strBiblioRecPath,
                    int nMax,
                    out string[] paths);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetEntities(
                   string strBiblioRecPath,
                   long lStart,
                   long lCount,
                   string strStyle,
                   string strLang,
                   out EntityInfo[] entityinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetEntities(
                    string strBiblioRecPath,
                    EntityInfo[] entityinfos,
                    out EntityInfo[] errorinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetOrders(
                    string strBiblioRecPath,
                    long lStart,
                    long lCount,
                    string strStyle,
                    string strLang,
                    out EntityInfo[] orderinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetOrders(
                   string strBiblioRecPath,
                   EntityInfo[] orderinfos,
                   out EntityInfo[] errorinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchOrderDup(string strIndex,
                   string strBiblioRecPath,
                   int nMax,
                   out string[] paths);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetClock(string strTime);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetClock(out string strTime);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ResetPassword(string strParameters,
            string strMessageTemplate);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetValueTable(
                    string strTableName,
                    string strDbName,
                    out string[] values);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetOperLogs(
            string strFileName,
            long lIndex,
            long lHint,
            int nCount,
            string strStyle,
            string strFilter,
            out OperLogInfo[] records);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetCalendar(
                   string strAction,
                   string strName,
                   int nStart,
                   int nCount,
                   out CalenderInfo[] contents);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetCalendar(
                    string strAction,
                    CalenderInfo info);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult BatchTask(
                   string strName,
                   string strAction,
                   BatchTaskInfo info,
                   out BatchTaskInfo resultInfo);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ClearAllDbs();

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ManageDatabase(string strAction,
                    string strDatabaseName,
                    string strDatabaseInfo,
                    out string strOutputInfo);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetUser(
                    string strAction,
                    string strName,
                    int nStart,
                    int nCount,
                    out UserInfo[] contents);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetUser(
                   string strAction,
                   UserInfo info);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetChannelInfo(
                    string strQuery,
                    string strStyle,
                    int nStart,
                    int nCount,
                    out ChannelInfo[] contents);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ManageChannel(
    string strAction,
    string strStyle,
    ChannelInfo[] requests,
    out ChannelInfo[] results);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ChangeUserPassword(
                    string strUserName,
                    string strOldPassword,
                    string strNewPassword);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult VerifyBarcode(
            string strLibraryCode,
            string strBarcode);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetSystemParameter(
                    string strCategory,
                    string strName,
                    out string strValue);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetSystemParameter(
                    string strCategory,
                    string strName,
                    string strValue);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult UrgentRecover(
                   string strXML);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult PassGate(
                    string strReaderBarcode,
                    string strGateName,
                    string strResultTypeList,
                    out string[] results);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Foregift(
                    string strAction,
                    string strReaderBarcode,
                    out string strOutputReaderXml,
                    out string strOutputID);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Hire(
                    string strAction,
                    string strReaderBarcode,
                    out string strOutputReaderXml,
                    out string strOutputID);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult Settlement(
                    string strAction,
                    string[] ids);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchOneClassCallNumber(
                    string strArrangeGroupName,
                    string strClass,
                    string strResultSetName,
                    out string strQueryXml);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetCallNumberSearchResult(
                    string strArrangeGroupName,
                    string strResultSetName,
                    long lStart,
                    long lCount,
                    string strBrowseInfoStyle,
                    string strLang,
                    out CallNumberSearchResult[] searchresults);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetOneClassTailNumber(
                   string strArrangeGroupName,
                   string strClass,
                   out string strTailNumber);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetOneClassTailNumber(
                   string strAction,
                   string strArrangeGroupName,
                   string strClass,
                   string strTestNumber,
                   out string strOutputNumber);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchUsedZhongcihao(
                   string strZhongcihaoGroupName,
                   string strClass,
                   string strResultSetName,
                   out string strQueryXml);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetZhongcihaoSearchResult(
                   string strZhongcihaoGroupName,
                   string strResultSetName,
                   long lStart,
                   long lCount,
                   string strBrowseInfoStyle,
                   string strLang,
                   out ZhongcihaoSearchResult[] searchresults);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetZhongcihaoTailNumber(
                    string strZhongcihaoGroupName,
                    string strClass,
                    out string strTailNumber);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetZhongcihaoTailNumber(
                   string strAction,
                   string strZhongcihaoGroupName,
                   string strClass,
                   string strTestNumber,
                   out string strOutputNumber);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchDup(
                   string strOriginBiblioRecPath,
                   string strOriginBiblioRecXml,
                   string strProjectName,
                   string strStyle,
                   out string strUsedProjectName);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetDupSearchResult(
                    long lStart,
                    long lCount,
                    string strBrowseInfoStyle,
                    out DupSearchResult[] searchresults);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ListDupProjectInfos(
                    string strOriginBiblioDbName,
                    out DupProjectInfo[] results);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetUtilInfo(
                   string strAction,
                   string strDbName,
                   string strFrom,
                   string strKey,
                   string strValueAttrName,
                   out string strValue);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetUtilInfo(
                    string strAction,
                    string strDbName,
                    string strFrom,
                    string strRootElementName,
                    string strKeyAttrName,
                    string strValueAttrName,
                    string strKey,
                    string strValue);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetRes(string strResPath,
                    long nStart,
                    int nLength,
                    string strStyle,
                    out byte[] baContent,
                    out string strMetadata,
                    out string strOutputResPath,
                    out byte[] baOutputTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetComments(
                   string strBiblioRecPath,
                   long lStart,
                   long lCount,
                   string strStyle,
                   string strLang,
                   out EntityInfo[] commentinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetComments(
                    string strBiblioRecPath,
                    EntityInfo[] commentinfos,
                    out EntityInfo[] errorinfos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SearchCommentDup(string strIndex,
                    string strBiblioRecPath,
                    int nMax,
                    out string[] paths);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
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

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetMessage(
            string[] message_ids,
            MessageLevel messagelevel,
            out List<MessageData> messages);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ListMessage(
            string strStyle,
            string strResultsetName,
            string strBoxType,
            MessageLevel messagelevel,
            int nStart,
            int nCount,
            out int nTotalCount,
            out List<MessageData> messages);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult SetMessage(string strAction,
            string strStyle,
            List<MessageData> messages,
            out List<MessageData> output_messages);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetStatisInfo(string strDateRangeString,
            string strStyle,
            out RangeStatisInfo info,
            out string strXml);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ExistStatisInfo(string strDateRangeString,
            out List<DateExist> dates);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult GetFile(
    string strCategory,
    string strFileName,
    long lStart,
    long lLength,
    out byte[] baContent,
    out string strFileTime);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult ListFile(
            string strAction,
            string strCategory,
            string strFileName,
            long lStart,
            long lLength,
            out List<FileItemInfo> infos);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        LibraryServerResult HitCounter(string strAction,
            string strName,
            string strClientAddress,
            out long Value);

    }
}
