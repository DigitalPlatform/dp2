using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace RFIDLIB
{

    class rfidlib_reader
    {

#if UNICODE
        /**********************************************Use Unicode Character Set********************************************/
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetLibVersion(StringBuilder buf, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_LoadReaderDrivers(string drvpath);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetLoadedReaderDriverCount();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetLoadedReaderDriverOpt(UInt32 idx, string option, StringBuilder valueBuffer, ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 HID_Enum(string DevId);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int HID_GetEnumItem(UInt32 indx, byte infType, StringBuilder infBuffer, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 COMPort_Enum();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int COMPort_GetEnumItem(UInt32 idx, StringBuilder nameBuf, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_Open(string connStr, ref UIntPtr hrOut);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_Close(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr RDR_CreateInvenParamSpecList();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_TagInventory(UIntPtr hr, Byte AIType, Byte AntennaCoun, Byte[] AntennaIDs, UIntPtr InvenParamSpecList);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetTagDataReportCount(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr RDR_GetTagDataReport(UIntPtr hr, Byte seek);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_EnableAsyncTagReportOutput(UIntPtr hr, Byte type, UInt32 msg, UIntPtr hwnd, [In] RFIDLIB_EVENT_CALLBACK cb);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_DisableAsyncTagReportOutput(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_TagDisconnect(UIntPtr hr, UIntPtr hTag);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]

        // 2020/12/9
        public static extern int RDR_SetMultiAccessAntennas(UIntPtr hr, Byte[] antennas, Byte nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]

        public static extern int RDR_SetAcessAntenna(UIntPtr hr, Byte AntennaID);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_OpenRFTransmitter(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_CloseRFTransmitter(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetCommuImmeTimeout(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ResetCommuImmeTimeout(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetAntennaInterfaceCount(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetReaderInfor(UIntPtr hr,
                                     Byte Type,
                                     StringBuilder buffer,
                                    ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ExeSpecialControlCmd(UIntPtr hr, string cmd, string parameters, StringBuilder result, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockWrite(UIntPtr hr, UInt32 cfgno, byte[] cfgdata, UInt32 nSize, UInt32 mask);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockRead(UIntPtr hr, UInt32 cfgno, byte[] cfgbuff, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockSave(UIntPtr hr, UInt32 cfgno);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_CreateRS485Node(UIntPtr hr, UInt32 busAddr, ref UIntPtr ohrRS485Node);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetSupportedAirInterfaceProtocol(UIntPtr hr, UInt32 index, ref UInt32 AIPType);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetAirInterfaceProtName(UIntPtr hr, UInt32 AIPType, StringBuilder namebuf, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int DNODE_Destroy(UIntPtr dn);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetOutputCount(UIntPtr hr, ref byte nCount);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetOutputName(UIntPtr hr, byte idxOut, StringBuilder bufName, ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetReaderLastReturnError(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Bluetooth_Enum();

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Bluetooth_GetEnumItem(UInt32 idx, byte infType, StringBuilder nameBuf, ref UInt32 nSize);


        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetStopCommBeforeClose(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetSystemTime(UIntPtr hr, ref UInt32 year, ref Byte month, ref Byte day, ref Byte hour, ref Byte minute, ref Byte second);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetSystemTime(UIntPtr hr, UInt32 year, Byte month, Byte day, Byte hour, Byte minute, Byte second);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_FetchRecords(UIntPtr hr, UInt32 flags);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_ClearRecords(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_FlashEmpty(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ParseTagDataReportRaw(UIntPtr hTagReport, Byte[] rawBuffer, ref UInt32 nSize);


        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr RDR_CreateSetOutputOperations();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_AddOneOutputOperation(UIntPtr hOperas, Byte outNo, Byte outMode, UInt32 outFrequency, UInt32 outActiveDuration, UInt32 outInactiveDuration);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetOutput(UIntPtr hr, UIntPtr outputOpers /* output operations */);


        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_StartReportCollection(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_StopReportCollection(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_StopReportCollectionNoWait(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetEventHandler(UIntPtr hr, byte eventType, byte methType, byte msg, UIntPtr hwnd, RFID_EVENT_CALLBACK_NEW cb, UIntPtr param);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ResetPassingCounter(UIntPtr hr, UInt32 flag);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetPassingCounter(UIntPtr hr, ref UInt32 inFlow, ref UInt32 outFlow);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ReverseInOutDirection(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_Login(UIntPtr hr, Byte[] pwd);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_EnablePasswordLogin(UIntPtr hr, Byte[] pwd, Byte enable);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_UpdateLoginPassword(UIntPtr hr, Byte[] pwd, Byte[] newPwd);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetAIPTypeName(UIntPtr hr, UInt32 AIP_ID, StringBuilder nameBuf, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetTagTypeName(UIntPtr hr, UInt32 AIP_ID, UInt32 TAG_ID, StringBuilder nameBuf, ref UInt32 nSize);

        // 2019/1/24
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_LoadFactoryDefault(UIntPtr hr);

        // 2020/12/9
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetInvenStopTrigger(UIntPtr hInvenParams, Byte stopTriggerType, UInt32 maxTimeout, UInt32 triggerValue);

#else

        /**************************************************Use Multi-Byte Character Set***********************************************/
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetLibVersion(StringBuilder buf, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_LoadReaderDrivers(string drvpath);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetLoadedReaderDriverCount();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetLoadedReaderDriverOpt(UInt32 idx, string option, StringBuilder valueBuffer, ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 HID_Enum(string DevId);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int HID_GetEnumItem(UInt32 indx, byte infType, StringBuilder infBuffer, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 COMPort_Enum();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int COMPort_GetEnumItem(UInt32 idx, StringBuilder nameBuf, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_Open(string connStr, ref UIntPtr hrOut);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_Close(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr RDR_CreateInvenParamSpecList();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_TagInventory(UIntPtr hr, Byte AIType, Byte AntennaCoun, Byte[] AntennaIDs, UIntPtr InvenParamSpecList);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetTagDataReportCount(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr RDR_GetTagDataReport(UIntPtr hr, Byte seek);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_EnableAsyncTagReportOutput(UIntPtr hr, Byte type, UInt32 msg, UIntPtr hwnd, [In] RFIDLIB_EVENT_CALLBACK cb);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_DisableAsyncTagReportOutput(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_TagDisconnect(UIntPtr hr, UIntPtr hTag);

        // 2020/12/9
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetMultiAccessAntennas(UIntPtr hr, Byte[] antennas, Byte nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetAcessAntenna(UIntPtr hr, Byte AntennaID);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_OpenRFTransmitter(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_CloseRFTransmitter(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetCommuImmeTimeout(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ResetCommuImmeTimeout(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 RDR_GetAntennaInterfaceCount(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetReaderInfor(UIntPtr hr,
                                     Byte Type,
                                     StringBuilder buffer,
                                    ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ExeSpecialControlCmd(UIntPtr hr, string cmd, string parameters, StringBuilder result, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockWrite(UIntPtr hr, UInt32 cfgno, byte[] cfgdata, UInt32 nSize, UInt32 mask);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockRead(UIntPtr hr, UInt32 cfgno, byte[] cfgbuff, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockSave(UIntPtr hr, UInt32 cfgno);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_CreateRS485Node(UIntPtr hr, UInt32 busAddr, ref UIntPtr ohrRS485Node);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetSupportedAirInterfaceProtocol(UIntPtr hr, UInt32 index, ref UInt32 AIPType);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetAirInterfaceProtName(UIntPtr hr, UInt32 AIPType, StringBuilder namebuf, UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int DNODE_Destroy(UIntPtr dn);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetReaderLastReturnError(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Bluetooth_Enum();

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Bluetooth_GetEnumItem(UInt32 idx, byte infType, StringBuilder nameBuf, ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetStopCommBeforeClose(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetSystemTime(UIntPtr hr, ref UInt32 year, ref Byte month, ref Byte day, ref Byte hour, ref Byte minute, ref Byte second);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetSystemTime(UIntPtr hr, UInt32 year, Byte month, Byte day, Byte hour, Byte minute, Byte second);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_FetchRecords(UIntPtr hr, UInt32 flags);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_ClearRecords(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_FlashEmpty(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ParseTagDataReportRaw(UIntPtr hTagReport, Byte[] rawBuffer, ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetOutputCount(UIntPtr hr, ref Byte nCount);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetOutputName(UIntPtr hr, Byte idxOut, StringBuilder bufName, ref UInt32 nSize);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr RDR_CreateSetOutputOperations();
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_AddOneOutputOperation(UIntPtr hOperas, Byte outNo, Byte outMode, UInt32 outFrequency, UInt32 outActiveDuration, UInt32 outInactiveDuration);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetOutput(UIntPtr hr, UIntPtr outputOpers /* output operations */);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_StartReportCollection(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_StopReportCollection(UIntPtr hr);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_BuffMode_StopReportCollectionNoWait(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetEventHandler(UIntPtr hr, byte eventType, byte methType, byte msg, UIntPtr hwnd, RFID_EVENT_CALLBACK_NEW cb, UIntPtr param);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ResetPassingCounter(UIntPtr hr, UInt32 flag);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetPassingCounter(UIntPtr hr, ref UInt32 inFlow, ref UInt32 outFlow);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ReverseInOutDirection(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_Login(UIntPtr hr, Byte[] pwd);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_EnablePasswordLogin(UIntPtr hr, Byte[] pwd, Byte enable);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_UpdateLoginPassword(UIntPtr hr, Byte[] pwd, Byte[] newPwd);


        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetAIPTypeName(UIntPtr hr, UInt32 AIP_ID, StringBuilder nameBuf, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetTagTypeName(UIntPtr hr, UInt32 AIP_ID, UInt32 TAG_ID, StringBuilder nameBuf, ref UInt32 nSize);

        // 2019/1/24
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_LoadFactoryDefault(UIntPtr hr);

        // 2020/12/19
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetInvenStopTrigger(UIntPtr hInvenParams, Byte stopTriggerType, UInt32 maxTimeout, UInt32 triggerValue);

#endif
    }
}

