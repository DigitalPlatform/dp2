using System;
using System.Collections.Generic;
using System.Text;

namespace RFIDLIB
{
#if REMOVED
    public delegate void RFIDLIB_EVENT_CALLBACK(UIntPtr wparam, UIntPtr lparam);  //stdcall
    class rfidlib_def
    {

        // Air protocol id
        public const int RFID_APL_UNKNOWN_ID = 0;
        public const int RFID_APL_ISO15693_ID = 1;
        public const int RFID_APL_ISO14443A_ID = 2;
        // ISO15693 Tag type id
        public const int RFID_UNKNOWN_PICC_ID = 0;
        public const int RFID_ISO15693_PICC_ICODE_SLI_ID = 1;
        public const int RFID_ISO15693_PICC_TI_HFI_PLUS_ID = 2;
        public const int RFID_ISO15693_PICC_ST_M24LRXX_ID = 3;	    /* ST M24 serial */
        public const int RFID_ISO15693_PICC_FUJ_MB89R118C_ID = 4;
        public const int RFID_ISO15693_PICC_ST_M24LR64_ID = 5;
        public const int RFID_ISO15693_PICC_ST_M24LR16E_ID = 6;
        public const int RFID_ISO15693_PICC_ICODE_SLIX_ID = 7;
        public const int RFID_ISO15693_PICC_TIHFI_STANDARD_ID = 8;
        public const int RFID_ISO15693_PICC_TIHFI_PRO_ID = 9;
        public const int RFID_ISO15693_PICC_ICODE_SLIX2_ID = 10;
        public const int RFID_ISO15693_PICC_CIT83128_ID = 11;
        //ISO14443a tag type id 
        public const int RFID_ISO14443A_PICC_NXP_ULTRALIGHT_ID = 1;
        public const int RFID_ISO14443A_PICC_NXP_MIFARE_S50_ID = 2;
        public const int RFID_ISO14443A_PICC_NXP_MIFARE_S70_ID = 3;


        //Inventory type 
        public const int AI_TYPE_NEW = 1;   // new antenna inventory  (reset RF power)
        public const int AI_TYPE_CONTINUE = 2;  // continue antenna inventory ;


        // Move position 
        public const int RFID_NO_SEEK = 0;			// No seeking 
        public const int RFID_SEEK_FIRST = 1;					// Seek first
        public const int RFID_SEEK_NEXT = 2;				// Seek next 
        public const int RFID_SEEK_LAST = 3;				// Seek last

        /*
        usb enum information
        */
        public const int HID_ENUM_INF_TYPE_SERIALNUM = 1;
        public const int HID_ENUM_INF_TYPE_DRIVERPATH = 2;


        public const int INVEN_STOP_TRIGGER_TYPE_Tms = 0;
        public const int INVEN_STOP_TRIGGER_TYPE_N_attempt = 1;
        public const int INVEN_STOP_TRIGGER_TYPE_N_found = 2;
        public const int INVEN_STOP_TRIGGER_TYPE_TIMEOUT = 3;

        /*
        * Open connection string 
        */
        public const string CONNSTR_NAME_RDTYPE = "RDType";
        public const string CONNSTR_NAME_COMMTYPE = "CommType";

        public const string CONNSTR_NAME_COMMTYPE_COM = "COM";
        public const string CONNSTR_NAME_COMMTYPE_USB = "USB";
        public const string CONNSTR_NAME_COMMTYPE_NET = "NET";

        //HID Param
        public const string CONNSTR_NAME_HIDADDRMODE = "AddrMode";
        public const string CONNSTR_NAME_HIDSERNUM = "SerNum";
        //COM Param
        public const string CONNSTR_NAME_COMNAME = "COMName";
        public const string CONNSTR_NAME_COMBARUD = "BaudRate";
        public const string CONNSTR_NAME_COMFRAME = "Frame";
        public const string CONNSTR_NAME_BUSADDR = "BusAddr";
        //TCP,UDP
        public const string CONNSTR_NAME_REMOTEIP = "RemoteIP";
        public const string CONNSTR_NAME_REMOTEPORT = "RemotePort";
        public const string CONNSTR_NAME_LOCALIP = "LocalIP";



        /*
        * Get loaded reader driver option 
        */
        public const string LOADED_RDRDVR_OPT_CATALOG = "CATALOG";
        public const string LOADED_RDRDVR_OPT_NAME = "NAME";
        public const string LOADED_RDRDVR_OPT_ID = "ID";
        public const string LOADED_RDRDVR_OPT_COMMTYPESUPPORTED = "COMM_TYPE_SUPPORTED";

        /*
        * Reader driver type
        */
        public const string RDRDVR_TYPE_READER = "Reader";// general reader
        public const string RDRDVR_TYPE_MTGATE = "MTGate";// meeting gate
        public const string RDRDVR_TYPE_LSGATE = "LSGate";// Library secure gate


        /* supported product type */

        public const byte COMMTYPE_COM_EN = 0x01;
        public const byte COMMTYPE_USB_EN = 0x02;
        public const byte COMMTYPE_NET_EN = 0x04;

        //iso18000p6C session
        public const byte ISO18000p6C_S0 = 0;
        public const byte ISO18000p6C_S1 = 1;
        public const byte ISO18000p6C_S2 = 2;
        public const byte ISO18000p6C_S3 = 3;

        //iso18000p6c target
        public const byte ISO18000p6C_TARGET_A = 0x00;
        public const byte ISO18000p6C_TARGET_B = 0x01;

        //iso18000p6C Q
        public const byte ISO18000p6C_Dynamic_Q = 0xff;


        public const UInt32 ISO18000p6C_META_BIT_MASK_EPC = 0x01;
        public const UInt32 ISO18000P6C_META_BIT_MASK_TIMESTAMP = 0x02;
        public const UInt32 ISO18000P6C_META_BIT_MASK_FREQUENCY = 0x04;
        public const UInt32 ISO18000p6C_META_BIT_MASK_RSSI = 0x08;
        public const UInt32 ISO18000P6C_META_BIT_MASK_READCOUNT = 0x10;
        public const UInt32 ISO18000P6C_META_BIT_MASK_TAGDATA = 0x20;
    }
#endif

    public delegate void RFIDLIB_EVENT_CALLBACK(UIntPtr wparam, UIntPtr lparam);  //stdcall
    class rfidlib_def
    {

        // Air protocol id
        public const int RFID_APL_UNKNOWN_ID = 0;
        public const int RFID_APL_ISO15693_ID = 1;
        public const int RFID_APL_ISO14443A_ID = 2;
        // ISO15693 Tag type id
        public const int RFID_UNKNOWN_PICC_ID = 0;
        public const int RFID_ISO15693_PICC_ICODE_SLI_ID = 1;
        public const int RFID_ISO15693_PICC_TI_HFI_PLUS_ID = 2;
        public const int RFID_ISO15693_PICC_ST_M24LRXX_ID = 3;	    /* ST M24 serial */
        public const int RFID_ISO15693_PICC_FUJ_MB89R118C_ID = 4;
        public const int RFID_ISO15693_PICC_ST_M24LR64_ID = 5;
        public const int RFID_ISO15693_PICC_ST_M24LR16E_ID = 6;
        public const int RFID_ISO15693_PICC_ICODE_SLIX_ID = 7;
        public const int RFID_ISO15693_PICC_TIHFI_STANDARD_ID = 8;
        public const int RFID_ISO15693_PICC_TIHFI_PRO_ID = 9;
        public const int RFID_ISO15693_PICC_ICODE_SLIX2_ID = 10;
        public const int RFID_ISO15693_PICC_CIT83128_ID = 11;
        //ISO14443a tag type id 
        public const int RFID_ISO14443A_PICC_NXP_ULTRALIGHT_ID = 1;
        public const int RFID_ISO14443A_PICC_NXP_MIFARE_S50_ID = 2;
        public const int RFID_ISO14443A_PICC_NXP_MIFARE_S70_ID = 3;


        //Inventory type 
        public const int AI_TYPE_NEW = 1;   // new antenna inventory  (reset RF power)
        public const int AI_TYPE_CONTINUE = 2;  // continue antenna inventory ;


        // Move position 
        public const int RFID_NO_SEEK = 0;			// No seeking 
        public const int RFID_SEEK_FIRST = 1;					// Seek first
        public const int RFID_SEEK_NEXT = 2;				// Seek next 
        public const int RFID_SEEK_LAST = 3;				// Seek last

        /*
        usb enum information
        */
        public const int HID_ENUM_INF_TYPE_SERIALNUM = 1;
        public const int HID_ENUM_INF_TYPE_DRIVERPATH = 2;


        public const int INVEN_STOP_TRIGGER_TYPE_Tms = 0;
        public const int INVEN_STOP_TRIGGER_TYPE_N_attempt = 1;
        public const int INVEN_STOP_TRIGGER_TYPE_N_found = 2;
        public const int INVEN_STOP_TRIGGER_TYPE_TIMEOUT = 3;

        /*
        * Open connection string 
        */
        public const string CONNSTR_NAME_RDTYPE = "RDType";
        public const string CONNSTR_NAME_COMMTYPE = "CommType";

        public const string CONNSTR_NAME_COMMTYPE_COM = "COM";
        public const string CONNSTR_NAME_COMMTYPE_USB = "USB";
        public const string CONNSTR_NAME_COMMTYPE_NET = "NET";

        //HID Param
        public const string CONNSTR_NAME_HIDADDRMODE = "AddrMode";
        public const string CONNSTR_NAME_HIDSERNUM = "SerNum";
        //COM Param
        public const string CONNSTR_NAME_COMNAME = "COMName";
        public const string CONNSTR_NAME_COMBARUD = "BaudRate";
        public const string CONNSTR_NAME_COMFRAME = "Frame";
        public const string CONNSTR_NAME_BUSADDR = "BusAddr";
        //TCP,UDP
        public const string CONNSTR_NAME_REMOTEIP = "RemoteIP";
        public const string CONNSTR_NAME_REMOTEPORT = "RemotePort";
        public const string CONNSTR_NAME_LOCALIP = "LocalIP";



        /*
        * Get loaded reader driver option 
        */
        public const string LOADED_RDRDVR_OPT_CATALOG = "CATALOG";
        public const string LOADED_RDRDVR_OPT_NAME = "NAME";
        public const string LOADED_RDRDVR_OPT_ID = "ID";
        public const string LOADED_RDRDVR_OPT_COMMTYPESUPPORTED = "COMM_TYPE_SUPPORTED";

        /*
        * Reader driver type
        */
        public const string RDRDVR_TYPE_READER = "Reader";// general reader
        public const string RDRDVR_TYPE_MTGATE = "MTGate";// meeting gate
        public const string RDRDVR_TYPE_LSGATE = "LSGate";// Library secure gate


        /* supported product type */

        public const byte COMMTYPE_COM_EN = 0x01;
        public const byte COMMTYPE_USB_EN = 0x02;
        public const byte COMMTYPE_NET_EN = 0x04;

        //iso18000p6C session
        public const byte ISO18000p6C_S0 = 0;
        public const byte ISO18000p6C_S1 = 1;
        public const byte ISO18000p6C_S2 = 2;
        public const byte ISO18000p6C_S3 = 3;
        //ISO18000p6C Memory bank 
        public const uint ISO18000p6C_MEM_BANK_RFU = 0x00;
        public const uint ISO18000p6C_MEM_BANK_EPC = 0x01;
        public const uint ISO18000p6C_MEM_BANK_TID = 0x02;
        public const uint ISO18000p6C_MEM_BANK_USER = 0x03;
        //iso18000p6c target
        public const byte ISO18000p6C_TARGET_A = 0x00;
        public const byte ISO18000p6C_TARGET_B = 0x01;

        //iso18000p6C Q
        public const byte ISO18000p6C_Dynamic_Q = 0xff;


        public const UInt32 ISO18000p6C_META_BIT_MASK_EPC = 0x01;
        public const UInt32 ISO18000P6C_META_BIT_MASK_TIMESTAMP = 0x02;
        public const UInt32 ISO18000P6C_META_BIT_MASK_FREQUENCY = 0x04;
        public const UInt32 ISO18000p6C_META_BIT_MASK_RSSI = 0x08;
        public const UInt32 ISO18000P6C_META_BIT_MASK_READCOUNT = 0x10;
        public const UInt32 ISO18000P6C_META_BIT_MASK_TAGDATA = 0x20;

        public const uint ISO18000p6C_SELECT_TARGET_INV_S0 = 0x00;
        public const uint ISO18000p6C_SELECT_TARGET_INV_S1 = 0x01;
        public const uint ISO18000p6C_SELECT_TARGET_INV_S2 = 0x02;
        public const uint ISO18000p6C_SELECT_TARGET_INV_S3 = 0x03;
        public const uint ISO18000p6C_SELECT_TARGET_INV_SL = 0x04;
    }

}
