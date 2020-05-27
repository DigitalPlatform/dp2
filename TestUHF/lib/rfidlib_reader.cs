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
        public static extern  UInt32  RDR_GetLibVersion(StringBuilder buf ,UInt32 nSize )  ;
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
        public static extern UInt32  COMPort_Enum() ;
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int  COMPort_GetEnumItem(UInt32 idx,StringBuilder nameBuf,UInt32 nSize ) ;
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int  RDR_Open(string connStr ,ref UIntPtr hrOut) ;
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern  int RDR_Close(UIntPtr hr)  ;
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
        public static extern int RDR_TagDisconnect(UIntPtr hr,UIntPtr hTag);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetAcessAntenna(UIntPtr hr ,Byte AntennaID) ;
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_OpenRFTransmitter(UIntPtr hr) ;
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
									 Byte Type ,
									 StringBuilder buffer,
									ref UInt32 nSize) ;

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int  RDR_ExeSpecialControlCmd(UIntPtr hr,string cmd ,string parameters,StringBuilder result,ref UInt32 nSize) ;
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_ConfigBlockWrite(UIntPtr hr,UInt32 cfgno ,byte[] cfgdata,UInt32 nSize,UInt32 mask) ;
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
        public static extern int RDR_SetInvenStopTrigger(UIntPtr hInvenParams, Byte stopTriggerType, UInt32 maxTimeout, UInt32 triggerValue);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Bluetooth_Enum();

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int Bluetooth_GetEnumItem(UInt32 idx, byte infType, StringBuilder nameBuf, ref UInt32 nSize);

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
        public static extern int RDR_GetOutputCount(UIntPtr hr, ref byte nCount);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetOutputName(UIntPtr hr, byte idxOut, StringBuilder bufName, ref UInt32 nSize);
        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_GetReaderLastReturnError(UIntPtr hr);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int RDR_SetInvenStopTrigger(UIntPtr hInvenParams, Byte stopTriggerType, UInt32 maxTimeout, UInt32 triggerValue);

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Bluetooth_Enum();

        [DllImport("rfidlib_reader.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Bluetooth_GetEnumItem(UInt32 idx, byte infType, StringBuilder nameBuf, ref UInt32 nSize);



#endif
    }
}
