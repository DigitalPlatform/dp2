using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace RFIDLIB
{
    class rfidlib_aip_iso15693
    {
#if UNICODE
        /**********************************************Use Unicode Character Set********************************************/
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ISO15693_GetLibVersion(StringBuilder buf, UInt32 nSize);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO15693_CreateInvenParam(UIntPtr hInvenParamSpecList, Byte AntennaID, Byte en_afi, Byte afi, Byte slot_type);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_ParseTagDataReport(UIntPtr hTagReport, ref UInt32 aip_id, ref UInt32 tag_id, ref UInt32 ant_id, ref Byte dsfid, Byte[] uid);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_Connect(UIntPtr hr, UInt32 tagType, Byte address_mode, Byte[] uid, ref UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_Reset(UIntPtr hr,
                                    UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_ReadSingleBlock(
                                    UIntPtr hr,
                                    UIntPtr ht,
                                    Byte readSecSta,
                                  UInt32 blkAddr,
                        Byte[] bufBlockDat,
                        UInt32 nSize,
                        ref UInt32 bytesBlkDatRead);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteSingleBlock(
                                    UIntPtr hr,
                                    UIntPtr ht,
                                   UInt32 blkAddr,
                                Byte[] newBlkData,
                                UInt32 bytesToWrite);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockMultipleBlocks(UIntPtr hr,
                                    UIntPtr ht, /* Tag handle connected */
                                     UInt32 blkAddr,
                                     UInt32 numOfBlks);


        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_ReadMultiBlocks(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte readSecSta,
                                     UInt32 blkAddr,
                            UInt32 numOfBlksToRead,
                            ref UInt32 numOfBlksRead,
                            Byte[] bufBlocks,
                            UInt32 nSize,
                            ref UInt32 bytesBlkDatRead);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteMultipleBlocks(UIntPtr hr,
                                    UIntPtr ht,
                                     UInt32 blkAddr,
                            UInt32 numOfBlks,
                            Byte[] newBlksData,
                            UInt32 bytesToWrite);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteAFI(UIntPtr hr,
                                    UIntPtr ht,
                                Byte afi);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockAFI(UIntPtr hr,
                                    UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteDSFID(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte dsfid);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockDSFID(UIntPtr hr,
                                    UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_GetSystemInfo(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte[] uid,
                                            ref Byte dsfid,
                                            ref Byte afi,
                                            ref UInt32 blkSize,
                                            ref UInt32 numOfBloks,
                                            ref Byte icRef);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_GetBlockSecStatus(UIntPtr hr,
                                    UIntPtr ht,
                                        UInt32 blkAddr,
                                        UInt32 numOfBlks,
                                        Byte[] bufBlkSecs,
                                        UInt32 nSize,
                                        ref UInt32 bytesSecRead);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_EableEAS(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_DisableEAS(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_LockEAS(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_EASCheck(UIntPtr hr, UIntPtr ht, ref Byte EASStatus);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_WritePassword(UIntPtr hr, UIntPtr ht, Byte pwdNo, UInt32 pwd);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_LockPassword(UIntPtr hr, UIntPtr ht, Byte pwdNo);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_PasswordProtect(UIntPtr hr, UIntPtr ht, Byte bandType);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_GetRandomAndSetPassword(UIntPtr hr, UIntPtr ht, Byte pwdNo, UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_WriteEASID(UIntPtr hr, UIntPtr ht, UInt16 EASID);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_GetNxpSysInfo(UIntPtr hr,
                                            UIntPtr ht,
                                            ref Byte PPPointer,
                                            ref Byte PPConditions,
                                            ref Byte lockBits,
                                            ref UInt32 featureFlags);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_ReadSignature(UIntPtr hr,
                                            UIntPtr ht,
                                            Byte[] signature/* out ,32 bytes */);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_Enable64BitPwd(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_ProtectPage(UIntPtr hr,
                                          UIntPtr ht,
                                          Byte PPPointer,
                                          Byte protSta);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_LockPageProtection(UIntPtr hr,
                                                 UIntPtr ht,
                                                 Byte pageAddr);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_Destroy(UIntPtr hr,
                                      UIntPtr ht,
                                      UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_EnblePrivacy(UIntPtr hr,
                                           UIntPtr ht,
                                           UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_PresetCounter(UIntPtr hr,
                                            UIntPtr ht,
                                            UInt16 initCnt,
                                            Byte enReadPwdProtect);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_IncrementCounter(UIntPtr hr,
                                               UIntPtr ht);


        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int TIHFIPLUS_Write2Blocks(UIntPtr hr,
                            UIntPtr ht,
                                    UInt32 blkAddr,
                                    Byte[] newTwoBlksData,
                                    UInt32 bytesToWrite
                                    );
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int TIHFIPLUS_Lock2Blocks(UIntPtr hr,
                            UIntPtr ht,
                                    UInt32 blkAddr);



        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CIT83128_GetSecPara(UIntPtr hr,
                                                    UIntPtr ht,
                                                        Byte readSecByte,
                                                          Byte[] secData);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CIT83128_ActAu(UIntPtr hr,
                                                    UIntPtr ht,
                                                       UInt32 rr /* in */,
                                                     Byte[] token /* out */);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int CIT83128_InitMem(UIntPtr hr,
                                                            UIntPtr ht,
                                                             Byte loseEffect, /*in */
                                   Byte blkAddr, /*in */
                                        Byte blkNum,/* in */
                                   Byte[] blkData /*in,4bytes */);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_WriteSectorPassword(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte pwdnum,
                                    UInt32 new_pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_PresentSectorPassword(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte pwdnum,
                                    UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_LockSector(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte sector_num,
                                    Byte access_type,
                                    Byte pwd_num_protect);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_ReadCFG(UIntPtr hr,
                                    UIntPtr ht,
                                    ref Byte cfgby);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_WriteEHCfg(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte EnergyHarvesting,
                                    Byte EHMode);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_WriteDOCfg(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte cfg_do /* 0: RF busy mode(RF WIP/BUSY pin for RF busy) , 1: RF Write in progress( RF WIP/BUSY pin for) */
                                    );
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_SetRstEHEn(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte enable
                                        );
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_CheckEHEn(UIntPtr hr,
                                    UIntPtr ht,
                                    ref Byte ctrlreg);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STLRI2k_Kill(UIntPtr hr, /*in */
                                UIntPtr ht, /*in */
                                    Byte access, /*value is 0x00 */
                                    UInt32 code);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STLRI2k_WriteKill(UIntPtr hr, /*in */
                                    UIntPtr ht, /*in */
                                    Byte access, /*value is 0x00 */
                                    UInt32 code);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int STLRI2k_LockKill(UIntPtr hr, /*in */
                                    UIntPtr ht, /*in */
                                    Byte access, /*value is 0x00 */
                                    Byte protectSta);

#else
        /**************************************************Use Multi-Byte Character Set**********************************************/
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ISO15693_GetLibVersion(StringBuilder buf, UInt32 nSize);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr ISO15693_CreateInvenParam(UIntPtr hInvenParamSpecList, Byte AntennaID, Byte en_afi, Byte afi, Byte slot_type);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_ParseTagDataReport(UIntPtr hTagReport, ref UInt32 aip_id, ref UInt32 tag_id, ref UInt32 ant_id, ref Byte dsfid, Byte[] uid);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_Connect(UIntPtr hr, UInt32 tagType, Byte address_mode, Byte[] uid, ref UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_Reset(UIntPtr hr ,
									UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_ReadSingleBlock(
                                    UIntPtr hr,
                                    UIntPtr ht,
                                    Byte readSecSta,
                                  UInt32 blkAddr,
                        Byte[] bufBlockDat,
                        UInt32 nSize,
                        ref UInt32 bytesBlkDatRead);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteSingleBlock(
                                    UIntPtr hr,
                                    UIntPtr ht,
                                   UInt32 blkAddr,
                                Byte[] newBlkData,
                                UInt32 bytesToWrite);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockBlock(
                                     UIntPtr hr,
                                    UIntPtr ht,
                                    UInt32 blkAddr);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockMultipleBlocks(UIntPtr hr,
                                    UIntPtr ht, /* Tag handle connected */
                                     UInt32 blkAddr,
                                     UInt32 numOfBlks);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_ReadMultiBlocks(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte readSecSta,
                                     UInt32 blkAddr,
                            UInt32 numOfBlksToRead,
                            ref UInt32 numOfBlksRead,
                            Byte[] bufBlocks,
                            UInt32 nSize,
                            ref UInt32 bytesBlkDatRead);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteMultipleBlocks(UIntPtr hr,
                                   UIntPtr ht,
                                    UInt32 blkAddr,
                           UInt32 numOfBlks,
                           Byte[] newBlksData,
                           UInt32 bytesToWrite);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteAFI(UIntPtr hr,
                                    UIntPtr ht,
                                Byte afi);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockAFI(UIntPtr hr,
                                    UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_WriteDSFID(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte dsfid);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_LockDSFID(UIntPtr hr,
                                    UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_GetSystemInfo(UIntPtr hr,
                                    UIntPtr ht,
                                    Byte[] uid,
                                            ref Byte dsfid,
                                            ref Byte afi,
                                            ref UInt32 blkSize,
                                            ref UInt32 numOfBloks,
                                            ref Byte icRef);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int ISO15693_GetBlockSecStatus(UIntPtr hr,
                                    UIntPtr ht,
                                        UInt32 blkAddr,
                                        UInt32 numOfBlks,
                                        Byte[] bufBlkSecs,
                                        UInt32 nSize,
                                        ref UInt32 bytesSecRead);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_EableEAS(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_DisableEAS(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_LockEAS(UIntPtr hr, UIntPtr ht);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_EASCheck(UIntPtr hr, UIntPtr ht, ref  Byte EASStatus);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_WritePassword(UIntPtr hr, UIntPtr ht, Byte pwdNo, UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_LockPassword(UIntPtr hr, UIntPtr ht, Byte pwdNo);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_PasswordProtect(UIntPtr hr, UIntPtr ht, Byte bandType);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_GetRandomAndSetPassword(UIntPtr hr, UIntPtr ht, Byte pwdNo, UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int  NXPICODESLI_WriteEASID(UIntPtr hr ,UIntPtr ht,UInt16 EASID ) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_GetNxpSysInfo(UIntPtr hr ,
											UIntPtr ht,
											ref Byte PPPointer ,
											ref Byte PPConditions ,
											ref Byte lockBits,
											ref UInt32 featureFlags) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_ReadSignature(UIntPtr hr ,
											UIntPtr ht,
											Byte[] signature/* out ,32 bytes */) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_Enable64BitPwd(UIntPtr hr ,UIntPtr ht) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_ProtectPage(UIntPtr hr ,
										  UIntPtr ht,
										  Byte PPPointer,
										  Byte protSta) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_LockPageProtection(UIntPtr hr ,
												 UIntPtr ht,
												 Byte pageAddr) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_Destroy(UIntPtr hr ,
									  UIntPtr ht,
									  UInt32 pwd) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_EnblePrivacy(UIntPtr hr ,
										   UIntPtr ht,
										   UInt32 pwd) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_PresetCounter(UIntPtr hr ,
											UIntPtr ht,
											UInt16 initCnt,
											Byte enReadPwdProtect) ;
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int NXPICODESLI_IncrementCounter(UIntPtr hr,
                                               UIntPtr ht);


        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int TIHFIPLUS_Write2Blocks(UIntPtr hr,
                            UIntPtr ht,
                                    UInt32 blkAddr,
                                    Byte[] newTwoBlksData,
                                    UInt32 bytesToWrite
                                    );
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int TIHFIPLUS_Lock2Blocks(UIntPtr hr,
                            UIntPtr ht,
                                    UInt32 blkAddr);

      


        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int CIT83128_GetSecPara(UIntPtr hr,
                                                    UIntPtr ht,
                                                        Byte readSecByte,
                                                          Byte[] secData);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int CIT83128_ActAu(UIntPtr hr,
                                                    UIntPtr ht,
                                                       UInt32 rr /* in */,
                                                     Byte[] token /* out */);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int CIT83128_InitMem(UIntPtr hr,
                                                            UIntPtr ht,
                                                             Byte loseEffect, /*in */
								   Byte blkAddr, /*in */
                                   Byte blkNum ,/* in */
								   Byte[] blkData /*in,4bytes * blkNum */);


        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_WriteSectorPassword(UIntPtr hr,
									UIntPtr ht,
									Byte pwdnum,
									UInt32 new_pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int  STM24LR_PresentSectorPassword(UIntPtr hr,
									UIntPtr ht,
									Byte pwdnum,
									UInt32 pwd);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_LockSector(UIntPtr hr,
                                    UIntPtr ht,
									Byte sector_num,
                                    Byte access_type,
                                    Byte pwd_num_protect);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_ReadCFG(UIntPtr hr,
                                    UIntPtr ht,
									ref Byte cfgby);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_WriteEHCfg(UIntPtr hr,
                                    UIntPtr ht,
									Byte EnergyHarvesting,
                                    Byte EHMode);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_WriteDOCfg(UIntPtr hr,
                                    UIntPtr ht,
									Byte cfg_do /* 0: RF busy mode(RF WIP/BUSY pin for RF busy) , 1: RF Write in progress( RF WIP/BUSY pin for) */
									);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_SetRstEHEn(UIntPtr hr,
                                    UIntPtr ht,
									Byte enable
										);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STM24LR_CheckEHEn(UIntPtr hr,
                                    UIntPtr ht,
									ref Byte ctrlreg);

        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STLRI2k_Kill(UIntPtr hr, /*in */
								UIntPtr ht, /*in */
									Byte access, /*value is 0x00 */
									UInt32 code);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STLRI2k_WriteKill(UIntPtr hr, /*in */
									UIntPtr ht, /*in */
									Byte access, /*value is 0x00 */
									UInt32 code);
        [DllImport("rfidlib_aip_iso15693.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int STLRI2k_LockKill(UIntPtr hr, /*in */
                                    UIntPtr ht, /*in */
                                    Byte access, /*value is 0x00 */
                                    Byte protectSta);
#endif
    }
}

