using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Net.Security;

using DigitalPlatform.rms;

#if NO
namespace dp2Kernel
{
    [ServiceContract(
        Name = "KernelService",
        Namespace = "http://dp2003.com/dp2kernel/",
        SessionMode = SessionMode.Required/*,
        ProtectionLevel = ProtectionLevel.None*/)]
    public interface IKernelService
    {
        /*
        [OperationContract]
        int DoTest(string strText);
         * */

        // 2012/1/5
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetVersion();

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result Login(string strUserName,
            string strPassword);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        Result Logout();

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result Search(string strQuery,
            string strResultSetName,
            string strOutputStyle);

        // 2012/1/5
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result SearchEx(string strQuery,
    string strResultSetName,
    string strSearchStyle,
    long lRecordCount,
    string strLang,
    string strRecordStyle,
    out Record[] records);

        [OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = false)]
        void Stop();


        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result ChangePassword(string strNewPassword);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result ChangeOtherPassword(
            string strUserName,
            string strNewPassword);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result Dir(string strResPath,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out ResInfoItem[] items);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result InitializeDb(string strDbName);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result RefreshDb(
            string strAction,
            string strDbName,
            bool bClearAllKeyTables);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetRecords(
            string strResultSetName,
            long lStart,
            long lLength,
            string strLang,
            string strStyle,
            out Record[] records);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetBrowse(
            string[] paths,
            string strStyle,
            out Record[] records);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetRichRecords(
            string strResultSetName,
            string strRanges,
            string strLang,
            string strStyle,
            out RichRecord[] richRecords);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetRes(string strResPath,
            long nStart,
            int nLength,
            string strStyle,
            out byte[] baContent,
            // out string strAttachmentID,
            out string strMetadata,
            out string strOutputResPath,
            out byte[] baOutputTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result WriteRes(string strResPath,
            string strRanges,
            long lTotalLength,
            byte[] baContent,
            // string strAttachmentID,
            string strMetadata,
            string strStyle,
            byte[] baInputTimestamp,
            out string strOutputResPath,
            out byte[] baOutputTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result WriteRecords(
            RecordBody[] inputs,
            string strStyle,
            out RecordBody[] results);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result DeleteRes(string strResPath,
            byte[] baInputTimestamp,
            string strStyle,
            out byte[] baOutputTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result RebuildResKeys(string strResPath,
            string strStyle,
            out string strOutputResPath);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result SetDbInfo(string strDbName,
            LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysText,
            string strBrowseText);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetDbInfo(string strDbName,
            string strStyle,
            out LogicNameItem[] logicNames,
            out string strType,
            out string strSqlDbName,
            out string strKeysText,
            out string strBrowseText);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result CreateDb(LogicNameItem[] logicNames,
            string strType,
            string strSqlDbName,
            string strKeysDefault,
            string strBrowseDefault);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result DeleteDb(string strDbName);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result CreateKeys(string strXml,
            string strRecPath,
            int lStart,
            int lLength,
            string strLang,
            out KeyInfo[] keys);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result CopyRecord(string strOriginRecordPath,
            string strTargetRecordPath,
            bool bDeleteOriginRecord,
            out string strOutputRecordPath,
            out byte[] baOutputRecordTimestamp);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result BatchTask(
            string strName,
            string strAction,
            TaskInfo info,
            out TaskInfo [] results);

        [OperationContract(IsInitiating = true, IsTerminating = false)]
        Result GetProperty(out string strProperty);

    }
}

#endif