using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace RFIDLIB
{
    class rfidlib_aip_iso18000p6C
    {
#if UNICODE
        /**********************************************Use Unicode Character Set********************************************/
        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ISO18000p6C_GetLibVersion(StringBuilder buf, UInt32 nSize);


        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr  ISO18000p6C_CreateInvenParam(UIntPtr hInvenParamSpecList,
														Byte AntennaID /* By default set to 0,apply to all antenna */,
														Byte Sel,
														Byte Session,
														Byte Target,
														Byte Q) ;

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int   ISO18000p6C_SetInvenSelectParam(UIntPtr hIso18000p6CInvenParam ,
											        Byte target ,
													 Byte action ,
													  Byte memBank ,
													   UInt32  dwPointer,
														Byte[] maskBits,
														  Byte maskBitLen,
														  Byte truncate) ;

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int  ISO18000p6C_SetInvenMetaDataFlags(UIntPtr hIso18000p6CInvenParam ,UInt32 flags) ;
        

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_SetInvenReadParam(UIntPtr hIso18000p6CInvenParam, Byte MemBank, UInt32 WordPtr, Byte WordCount);


        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int  ISO18000p6C_ParseTagReport(UIntPtr hTagReport,
											ref UInt32 aip_id,
										  ref UInt32 tag_id,
										  ref UInt32 ant_id,
										  ref UInt32 metaFlags,
										  Byte[] tagdata,
										  ref UInt32  tdLen /* IN:max size of the tagdata buffer ,OUT:bytes written into tagdata buffer */) ;



        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr  ISO18000p6C_CreateTAWrite(UIntPtr hIso18000p6CInvenParam,
													 Byte memBank,
													 UInt32 wordPtr ,
													 UInt32 wordCnt ,
													 Byte[] pdatas,
													 UInt32 nSize) ;

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_CheckTAWriteResult(UIntPtr hTagReport);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_SetInvenAccessPassword(UIntPtr hIso18000p6CInvenParam, UInt32 pwd);


        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO18000p6C_CreateTALock(UIntPtr hIso18000p6CInvenParam,
                                                    UInt16 mask,
                                                    UInt16 action);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_CheckTALockResult(UIntPtr hTagReport);
#else
        /**************************************************Use Multi-Byte Character Set**********************************************/
        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ISO18000p6C_GetLibVersion(StringBuilder buf, UInt32 nSize);


        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO18000p6C_CreateInvenParam(UIntPtr hInvenParamSpecList,
                                                        Byte AntennaID /* By default set to 0,apply to all antenna */,
                                                        Byte Sel,
                                                        Byte Session,
                                                        Byte Target,
                                                        Byte Q);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_SetInvenSelectParam(UIntPtr hIso18000p6CInvenParam,
                                                    Byte target,
                                                     Byte action,
                                                      Byte memBank,
                                                       UInt32 dwPointer,
                                                        Byte[] maskBits,
                                                          Byte maskBitLen,
                                                          Byte truncate);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_SetInvenMetaDataFlags(UIntPtr hIso18000p6CInvenParam, UInt32 flags);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_SetInvenReadParam(UIntPtr hIso18000p6CInvenParam, Byte MemBank, UInt32 WordPtr, Byte WordCount);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_ParseTagReport(UIntPtr hTagReport,
                                            ref UInt32 aip_id,
                                          ref UInt32 tag_id,
                                          ref UInt32 ant_id,
                                          ref UInt32 metaFlags,
                                          Byte[] tagdata,
                                          ref UInt32 tdLen /* IN:max size of the tagdata buffer ,OUT:bytes written into tagdata buffer */);



        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO18000p6C_CreateTAWrite(UIntPtr hIso18000p6CInvenParam,
                                                     Byte memBank,
                                                     UInt32 wordPtr,
                                                     UInt32 wordCnt,
                                                     Byte[] pdatas,
                                                     UInt32 nSize);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_CheckTAWriteResult(UIntPtr hTagReport);



        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_SetInvenAccessPassword(UIntPtr hIso18000p6CInvenParam, UInt32 pwd);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO18000p6C_CreateTALock(UIntPtr hIso18000p6CInvenParam,
                                                    UInt16 mask,
                                                    UInt16 action);

        [DllImport("rfidlib_aip_iso18000p6C.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO18000p6C_CheckTALockResult(UIntPtr hTagReport);
#endif
    }
}
