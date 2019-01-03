using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DigitalPlatform.RFID;

namespace RfidDrivers.First
{
    public class Driver1 : IRfidDriver
    {
        public UIntPtr hreader;

        public void InitializeDriver()
        {
            GetDriversInfo();
        }

        public void ReleaseDriver()
        {

        }

        public List<CReaderDriverInf> readerDriverInfoList = new List<CReaderDriverInf>();


        private void GetDriversInfo()
        {
            /* 
             *  Call required, when application load ,this API just only need to load once
             *  Load all reader driver dll from drivers directory, like "rfidlib_ANRD201.dll"  
             */
            RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers");

            /*
             * Not call required,it can be Omitted in your own appliation
             * enum and show loaded reader driver 
             */
            UInt32 nCount;
            nCount = RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverCount();
            uint i;
            for (i = 0; i < nCount; i++)
            {
                UInt32 nSize;
                CReaderDriverInf driver = new CReaderDriverInf();
                StringBuilder strCatalog = new StringBuilder();
                strCatalog.Append('\0', 64);

                nSize = (UInt32)strCatalog.Capacity;
                RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_CATALOG, strCatalog, ref nSize);
                driver.m_catalog = strCatalog.ToString();
                if (driver.m_catalog == RFIDLIB.rfidlib_def.RDRDVR_TYPE_READER) // Only reader we need
                {
                    StringBuilder strName = new StringBuilder();
                    strName.Append('\0', 64);
                    nSize = (UInt32)strName.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_NAME, strName, ref nSize);
                    driver.m_name = strName.ToString();

                    StringBuilder strProductType = new StringBuilder();
                    strProductType.Append('\0', 64);
                    nSize = (UInt32)strProductType.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_ID, strProductType, ref nSize);
                    driver.m_productType = strProductType.ToString();

                    StringBuilder strCommSupported = new StringBuilder();
                    strCommSupported.Append('\0', 64);
                    nSize = (UInt32)strCommSupported.Capacity;
                    RFIDLIB.rfidlib_reader.RDR_GetLoadedReaderDriverOpt(i, RFIDLIB.rfidlib_def.LOADED_RDRDVR_OPT_COMMTYPESUPPORTED, strCommSupported, ref nSize);
                    driver.m_commTypeSupported = (UInt32)int.Parse(strCommSupported.ToString());

                    readerDriverInfoList.Add(driver);
                }

            }


        }

        // parameters:
        //      comm_type   COM/USB/NET/BLUETOOTH 之一
        string BuildConnectionString(string readerDriverName,
            string comm_type)
        {
            if (string.IsNullOrEmpty(readerDriverName))
                readerDriverName = readerDriverInfoList[0].m_name;

            if (string.IsNullOrEmpty(comm_type))
                comm_type = "USB";

            return RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + comm_type + ";" +
                          "AddrMode=0";// ;SerNum=
#if NO
            int commTypeIdx = comboBox10.SelectedIndex;
            string connstr = "";
            // Build serial communication connection string
            if (commTypeIdx == 0)
            {
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + comm_type + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMNAME + "=" + comboBox1.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMBARUD + "=" + comboBox14.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMFRAME + "=" + comboBox15.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_BUSADDR + "=" + "255";
            }
            // Build USBHID communication connection string
            else if (commTypeIdx == 1)
            {
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + comm_type + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + usbOpenType.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + comboBox9.Text;
            }
            // Build network communication connection string
            else if (commTypeIdx == 2)
            {
                string ipAddr;
                UInt16 port;
                ipAddr = textBox5.Text;
                port = (UInt16)int.Parse(textBox6.Text);
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_NET + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_REMOTEIP + "=" + ipAddr + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_REMOTEPORT + "=" + port.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_LOCALIP + "=" + "";
            }
            // Build blueTooth communication connection string
            else if (commTypeIdx == 3)
            {
                if (txbBluetoothSN.Text == "")
                {
                    MessageBox.Show("The address of the bluetooth can not be null!");
                    return;
                }
                connstr = RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + readerDriverName + ";" +
                         RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_BLUETOOTH + ";" +
                         RFIDLIB.rfidlib_def.CONNSTR_NAME_BLUETOOTH_SN + "=" + txbBluetoothSN.Text;
            }
#endif
        }

        public void OpenReader()
        {
            var iret = RFIDLIB.rfidlib_reader.RDR_Open(
                BuildConnectionString("", ""),
                ref hreader);
            if (iret != 0)
                throw new Exception($"OpenReader error, return: {iret}");
        }

        public void CloseReader()
        {
            var iret = RFIDLIB.rfidlib_reader.RDR_Close(hreader);
            if (iret == 0)
            {
                // 成功
                hreader = (UIntPtr)0;
            }
            else
                throw new Exception($"CloseReader error, return: {iret}");
        }

        public void Inventory()
        {
            UInt32 nTagCount = 0;
            int ret = tag_inventory(
                   RFIDLIB.rfidlib_def.AI_TYPE_NEW,
                   1,
                   new Byte[] { 1 },
                     ref nTagCount);

        }

        public void ConnectTag()
        {

        }

        public void DisconnectTag()
        {

        }

        // parameters:
        //      AIType  RFIDLIB.rfidlib_def.AI_TYPE_NEW / RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE
        //      AntinnaSel  从 1 开始？
        public int tag_inventory(
                           Byte AIType,
                             Byte AntennaSelCount,
                             Byte[] AntennaSel,
                             //bool enable15693,
                             //bool enable14443A,
                             //bool enable18000p3m3,
                             // Byte afiVal,
                             // delegate_tag_report_handle tagReportHandler,
                             ref UInt32 nTagCount)
        {

            Byte enableAFI = 0;
            int iret;
            UIntPtr InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
            if (InvenParamSpecList.ToUInt64() != 0)
            {
                RFIDLIB.rfidlib_aip_iso15693.ISO15693_CreateInvenParam(
                    InvenParamSpecList,
                    0,
                    enableAFI,
                    0x00,   // AFI, 打算要匹配的 AFI byte 值
                    0);
            }
            nTagCount = 0;
            LABEL_TAG_INVENTORY:
            iret = RFIDLIB.rfidlib_reader.RDR_TagInventory(hreader, AIType, AntennaSelCount, AntennaSel, InvenParamSpecList);
            if (iret == 0 || iret == -21)
            {
                nTagCount += RFIDLIB.rfidlib_reader.RDR_GetTagDataReportCount(hreader);
                UIntPtr TagDataReport;
                TagDataReport = (UIntPtr)0;
                TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_FIRST); //first
                while (TagDataReport.ToUInt64() > 0)
                {
                    UInt32 aip_id = 0;
                    UInt32 tag_id = 0;
                    UInt32 ant_id = 0;
                    Byte dsfid = 0;
                    Byte uidlen = 0;
                    Byte[] uid = new Byte[16];

                    /* Parse iso15693 tag report */
                    {
                        iret = RFIDLIB.rfidlib_aip_iso15693.ISO15693_ParseTagDataReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref dsfid, uid);
                        if (iret == 0)
                        {
                            uidlen = 8;
                            object[] pList = { aip_id, tag_id, ant_id, uid, (int)uidlen };
                            //// Invoke(tagReportHandler, pList);
                            //tagReportHandler(hreader, aip_id, tag_id, ant_id, uid ,8);
                        }
                    }

                    /* Get Next report from buffer */
                    TagDataReport = RFIDLIB.rfidlib_reader.RDR_GetTagDataReport(hreader, RFIDLIB.rfidlib_def.RFID_SEEK_NEXT); //next
                }
                if (iret == -21) // stop trigger occur,need to inventory left tags
                {
                    AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;//use only-new-tag inventory 
                    goto LABEL_TAG_INVENTORY;
                }
                iret = 0;
            }
            if (InvenParamSpecList.ToUInt64() != 0)
                RFIDLIB.rfidlib_reader.DNODE_Destroy(InvenParamSpecList);
            return iret;
        }
    }

    public class CReaderDriverInf
    {
        public string m_catalog;
        public string m_name;
        public string m_productType;
        public UInt32 m_commTypeSupported;
    }
}
