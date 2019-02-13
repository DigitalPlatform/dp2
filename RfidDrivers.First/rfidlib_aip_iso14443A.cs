using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace RFIDLIB
{
    class rfidlib_aip_iso14443A
    {
#if UNICODE
        /**********************************************Use Unicode Character Set********************************************/
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ISO14443A_GetLibVersion(StringBuilder buf, UInt32 nSize);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO14443A_ParseTagDataReport(UIntPtr hTagReport,
                                          ref UInt32 aip_id,
                                         ref UInt32 tag_id,
                                          ref UInt32 ant_id,
                                          Byte[] uid,
                                          ref Byte uidlen);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO14443A_CreateInvenParam(UIntPtr hInvenParamSpecList,
                                                            Byte AntennaID
                                                            );
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Connect(UIntPtr hr, Byte tagType, Byte[] uid, ref UIntPtr ht);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Authenticate(UIntPtr hr, UIntPtr ht, Byte blkAddr, Byte keyType, Byte[] key);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_ReadBlock(UIntPtr hr, UIntPtr ht, Byte blkAddr, Byte[] blkData, UInt32 nSize);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_WriteBlock(UIntPtr hr, UIntPtr ht, Byte blkAddr, Byte[] blkData);


        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_FormatValueBlock(UIntPtr hr, UIntPtr ht, Byte blkAddr, UInt32 initValue);


        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Increment(UIntPtr hr, UIntPtr ht, Byte blkAddr, UInt32 val);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Decrement(UIntPtr hr, UIntPtr ht, Byte blkAddr, UInt32 val);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Transfer(UIntPtr hr, UIntPtr ht, Byte blkAddr);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Restore(UIntPtr hr, UIntPtr ht, Byte blkAddr);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_CreateAccessCondition(byte blk0AccType, byte blk1AccType, byte blk2AccType, byte trailerAccType,
            byte[] formattedAccCondi);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern byte MFCL_Sector2Block(byte sector);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern byte MFCL_Block2Sector(byte block);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern byte MFCL_IsTailerBlock(byte blkAddr);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_ParseAccessCondi(byte[] formattedAccCondi, ref byte blk0AccType, ref byte blk1AccType, ref byte blk2AccType, ref byte trailerAccType);


        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_Connect(UIntPtr hr, Byte[] uid, ref UIntPtr ht);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_LockPage(UIntPtr hr,
                                     UIntPtr ht,
                                     UInt32 pageType);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_LockBlock(UIntPtr hr,
                                      UIntPtr ht,
                                      UInt32 blockType);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_ReadMultiplePages(UIntPtr hr,
                                              UIntPtr ht,
                                              UInt32 pageStart,
                                              UInt32 pageNum,
                                              Byte[] databuf,
                                             ref UInt32 nSize);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_WriteMultiplePages(UIntPtr hr,
                                               UIntPtr ht,
                                               UInt32 pageStart,
                                               UInt32 pageNum,
                                               Byte[] databuf,
                                               UInt32 bytesToWrite);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_Authenticate(UIntPtr hr,
                                         UIntPtr ht,
                                         Byte[] key /* 16bytes */);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_UpdatePassword(UIntPtr hr,
                                            UIntPtr ht,
                                            Byte[] key);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_UpdateAuthConfig(UIntPtr hr,
                                             UIntPtr ht,
                                             Byte auth0,
                                             Byte auth1);

#else
        /**************************************************Use Multi-Byte Character Set**********************************************/
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ISO14443A_GetLibVersion(StringBuilder buf, UInt32 nSize);
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO14443A_ParseTagDataReport(UIntPtr hTagReport,
										  ref UInt32 aip_id,
										 ref UInt32 tag_id,
										  ref UInt32 ant_id,
										  Byte[]  uid,
										  ref Byte uidlen)   ;
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO14443A_CreateInvenParam(UIntPtr hInvenParamSpecList,
															Byte AntennaID
															)   ;
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Connect(UIntPtr hr, Byte tagType, Byte[] uid, ref UIntPtr ht);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Authenticate(UIntPtr hr, UIntPtr ht, Byte blkAddr, Byte keyType, Byte[] key);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_ReadBlock(UIntPtr hr, UIntPtr ht, Byte blkAddr, Byte[] blkData, UInt32 nSize);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_WriteBlock(UIntPtr hr, UIntPtr ht, Byte blkAddr, Byte[] blkData);


        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_FormatValueBlock(UIntPtr hr, UIntPtr ht, Byte blkAddr, UInt32 initValue);


        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Increment(UIntPtr hr, UIntPtr ht, Byte blkAddr, UInt32 val);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Decrement(UIntPtr hr, UIntPtr ht, Byte blkAddr, UInt32 val);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Transfer(UIntPtr hr, UIntPtr ht, Byte blkAddr);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_Restore(UIntPtr hr, UIntPtr ht, Byte blkAddr);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_CreateAccessCondition(byte blk0AccType, byte blk1AccType, byte blk2AccType, byte trailerAccType,
            byte[] formattedAccCondi);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern byte MFCL_Sector2Block(byte sector);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern byte MFCL_Block2Sector(byte block);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern byte MFCL_IsTailerBlock(byte blkAddr);

        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCL_ParseAccessCondi(byte[] formattedAccCondi, ref byte blk0AccType, ref byte blk1AccType, ref byte blk2AccType, ref byte trailerAccType);



        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_Connect(UIntPtr hr,Byte[] uid,ref UIntPtr ht) ;
         [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_LockPage(UIntPtr hr ,
									  UIntPtr ht,
									  UInt32 pageType)  ; 
         [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
         public static extern int ULTRALIGHT_LockBlock(UIntPtr hr ,
									   UIntPtr ht,
									   UInt32 blockType) ;
         [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_ReadMultiplePages(UIntPtr hr ,
											   UIntPtr ht,
											   UInt32 pageStart,
											   UInt32 pageNum ,
											   Byte[] databuf,
											  ref  UInt32 nSize ) ;
         [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
          public static extern int ULTRALIGHT_WriteMultiplePages(UIntPtr hr ,
												UIntPtr ht, 
												UInt32  pageStart,
												UInt32 pageNum ,
												Byte[] databuf,
												UInt32 bytesToWrite) ;
         [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_Authenticate(UIntPtr hr ,
										  UIntPtr ht, 
										  Byte[] key /* 16bytes */) ;
        [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_UpdatePassword(UIntPtr hr ,
											UIntPtr ht, 
											Byte[] key) ;
         [DllImport("rfidlib_aip_iso14443A.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ULTRALIGHT_UpdateAuthConfig(UIntPtr hr,
                                              UIntPtr ht,
											  Byte auth0,
                                              Byte auth1);
#endif
    }
}

