using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform;

namespace TestUHF
{
    public partial class MainForm : Form
    {
        UIntPtr _readerHandle = UIntPtr.Zero;

        public MainForm()
        {
            InitializeComponent();
        }

        private void button_openReader_Click(object sender, EventArgs e)
        {
            string strError = "";

            using (OpenReaderDialog dlg = new OpenReaderDialog())
            {
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.Cancel)
                    return;

                var connectionString = dlg.BuildConnectionString();

                var iret = RFIDLIB.rfidlib_reader.RDR_Open(connectionString, ref this._readerHandle);
                if (iret != 0)
                {
                    strError = "Open Reader Fail";
                    goto ERROR1;
                }
                else
                {
                    this.button_openReader.Enabled = false;
                    this.button_closeReader.Enabled = true;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_closeReader_Click(object sender, EventArgs e)
        {
            string strError = "";

            /*
            if (hTag != (UIntPtr)0)
            {
                MessageBox.Show("disconnect from tag first!");
                return;
            }
            */

            var iret = RFIDLIB.rfidlib_reader.RDR_Close(this._readerHandle);
            if (iret == 0)
            {
                this._readerHandle = (UIntPtr)0;
                this.button_openReader.Enabled = true;
                this.button_closeReader.Enabled = false;
            }
            else
            {
                strError = "Close Reader Failed";
                goto ERROR1;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        public class ParseDataResult : NormalResult
        {
            public string EPC { get; set; }
            // public string UID { get; set; }
            public uint Timestamp { get; set; }
            public uint Frequency { get; set; }
            public byte RSSI { get; set; }
            public byte ReadCount { get; set; }
            public byte[] ReadData { get; set; }
        }

        public static ParseDataResult ParseData(uint metaFlags,
            byte[] tagData,
            uint datlen)
        {

            UInt16 epcBitsLen = 0;
            int idx = 0;
            List<Byte> epc;
            List<Byte> readData;
            int i;
            // String strAntId;
            UInt32 timestamp;
            UInt32 frequency;
            Byte rssi;
            Byte readCnt;

            // strAntId = antID.ToString();

            epc = new List<byte>();
            readData = new List<byte>();
            timestamp = 0;
            frequency = 0;
            rssi = 0;
            readCnt = 0;
            if (metaFlags == 0) metaFlags |= RFIDLIB.rfidlib_def.ISO18000p6C_META_BIT_MASK_EPC;
            if ((metaFlags & RFIDLIB.rfidlib_def.ISO18000p6C_META_BIT_MASK_EPC) > 0)
            {
                if (datlen < 2)
                {
                    //error data size 
                    // return;
                    throw new Exception("error data size");
                }

                epcBitsLen = (UInt16)(tagData[idx] | (tagData[idx + 1] << 8));
                idx += 2;
                int epcBytes = ((epcBitsLen + 7) / 8);
                if ((datlen - idx) < epcBytes)
                {
                    // error data size 
                    // return;
                    throw new Exception("error data size 1");
                }
                for (i = 0; i < epcBytes; i++)
                {
                    epc.Add(tagData[idx + i]);
                }

                idx += epcBytes;
            }
            if ((metaFlags & RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_TIMESTAMP) > 0)
            {
                if ((datlen - idx) < 4)
                {
                    //error data size 
                    // return;
                    throw new Exception("error data size 2");
                }
                timestamp = (UInt32)(tagData[idx] | (tagData[idx + 1] << 8 & 0xff00) | (tagData[idx + 2] << 16 & 0xff0000) | (tagData[idx + 3] << 24 & 0xff000000));
                idx += 4;
            }
            if ((metaFlags & RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_FREQUENCY) > 0)
            {
                if ((datlen - idx) < 4)
                {
                    //error data size 
                    // return;
                    throw new Exception("error data size 3");

                }
                frequency = (UInt32)(tagData[idx] | (tagData[idx + 1] << 8 & 0xff00) | (tagData[idx + 2] << 16 & 0xff0000) | (tagData[idx + 3] << 24 & 0xff000000));
                idx += 4;
            }
            if ((metaFlags & RFIDLIB.rfidlib_def.ISO18000p6C_META_BIT_MASK_RSSI) > 0)
            {
                if ((datlen - idx) < 1)
                {
                    //error data size 
                    // return;
                    throw new Exception("error data size 4");

                }
                rssi = tagData[idx];
                idx += 1;
            }
            if ((metaFlags & RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_READCOUNT) > 0)
            {
                if ((datlen - idx) < 1)
                {
                    //error data size 
                    // return;
                    throw new Exception("error data size 5");

                }
                readCnt = tagData[idx];
                idx += 1;
            }
            if ((metaFlags & RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_TAGDATA) > 0)
            {
                for (i = idx; i < datlen; i++)
                {
                    readData.Add(tagData[i]);
                }
            }

            String strEPC = BitConverter.ToString(epc.ToArray(), 0, epc.Count).Replace("-", string.Empty);
            // String strUid = BitConverter.ToString(readData.ToArray(), 0, readData.Count).Replace("-", string.Empty);

            return new ParseDataResult
            {
                EPC = strEPC,
                // UID = strUid,
                Timestamp = timestamp,
                Frequency = frequency,
                RSSI = rssi,
                ReadCount = readCnt,
                ReadData = readData?.ToArray()
            };
        }


        public class UhfInventoryItem
        {
            public uint aip_id { get; set; }
            public uint tag_id { get; set; }
            // 天线编号
            public uint ant_id { get; set; }

            public uint metaFlags { get; set; }

            public byte[] tagData { get; set; }
            public uint nSize { get; set; }

            public string writeOper { get; set; }

            public string lockOper { get; set; }


        }

        public int tag_inventory(
            PARAMETERS invenParams,
            UIntPtr hreader,
            Byte AIType,
            Byte AntennaSelCount,
            Byte[] AntennaSel,
            out List<UhfInventoryItem> results,
            // delegate_tag_report_handle tagReportHandler,
            ref UInt32 nTagCount)
        {
            results = new List<UhfInventoryItem>();

            int iret;
            UIntPtr InvenParamSpecList = UIntPtr.Zero;
            InvenParamSpecList = RFIDLIB.rfidlib_reader.RDR_CreateInvenParamSpecList();
            if (InvenParamSpecList.ToUInt64() != 0)
            {
                /* set timeout */
                RFIDLIB.rfidlib_reader.RDR_SetInvenStopTrigger(InvenParamSpecList, RFIDLIB.rfidlib_def.INVEN_STOP_TRIGGER_TYPE_TIMEOUT, invenParams.m_timeout, 0);
                /* create ISO18000p6C air protocol inventory parameters */
                UIntPtr AIPIso18000p6c = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_CreateInvenParam(InvenParamSpecList, 0, 0, RFIDLIB.rfidlib_def.ISO18000p6C_S0, RFIDLIB.rfidlib_def.ISO18000p6C_TARGET_A, RFIDLIB.rfidlib_def.ISO18000p6C_Dynamic_Q);
                if (AIPIso18000p6c.ToUInt64() != 0)
                {
                    //set selection parameters
                    if (invenParams.m_sel.m_enable)
                    {
                        Byte[] maskBits = invenParams.m_sel.m_maskBits.ToArray();
                        RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_SetInvenSelectParam(AIPIso18000p6c, invenParams.m_sel.m_target, invenParams.m_sel.m_action, invenParams.m_sel.m_memBank, invenParams.m_sel.m_pointer, maskBits, invenParams.m_sel.m_maskBitsLength, 0);

                    }
                    // set inventory read parameters
                    if (invenParams.m_read.m_enable)
                    {
                        RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_SetInvenReadParam(AIPIso18000p6c, invenParams.m_read.m_memBank, invenParams.m_read.m_wordPtr, (Byte)invenParams.m_read.m_wordCnt);
                    }

                    // Add Embedded commands
                    if (invenParams.m_write.m_enable)
                    {
                        Byte[] writeDatas = invenParams.m_write.m_datas.ToArray();

                        RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_CreateTAWrite(AIPIso18000p6c, invenParams.m_write.m_memBank, invenParams.m_write.m_wordPtr, invenParams.m_write.m_wordCnt, writeDatas, (UInt32)writeDatas.Length);
                    }

                    if (invenParams.m_lock.m_enable)
                    {
                        UInt16 mask, action;
                        mask = action = 0;
                        if (invenParams.m_lock.m_userMemSelected)
                        {
                            mask |= 0x03;
                            action |= (UInt16)(invenParams.m_lock.m_userMem);
                        }
                        if (invenParams.m_lock.m_TIDMemSelected)
                        {
                            mask |= (0x03 << 2);
                            action |= (UInt16)(invenParams.m_lock.m_TIDMem << 2);
                        }
                        if (invenParams.m_lock.m_EPCMemSelected)
                        {
                            mask |= (0x03 << 4);
                            action |= (UInt16)(invenParams.m_lock.m_EPCMem << 4);
                        }
                        if (invenParams.m_lock.m_accessPwdSelected)
                        {
                            mask |= (0x03 << 6);
                            action |= (UInt16)(invenParams.m_lock.m_accessPwd << 6);
                        }
                        if (invenParams.m_lock.m_killPwdSelected)
                        {
                            mask |= (0x03 << 8);
                            action |= (UInt16)(invenParams.m_lock.m_killPwd << 8);
                        }

                        RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_CreateTALock(AIPIso18000p6c, mask, action);
                    }
                    // set meta flags 
                    if (invenParams.m_metaFlags.m_enable)
                    {
                        UInt32 metaFlags = 0;
                        if (invenParams.m_metaFlags.m_EPC)
                        {
                            metaFlags |= RFIDLIB.rfidlib_def.ISO18000p6C_META_BIT_MASK_EPC;
                        }
                        if (invenParams.m_metaFlags.m_timestamp)
                        {
                            metaFlags |= RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_TIMESTAMP;
                        }
                        if (invenParams.m_metaFlags.m_frequency)
                        {
                            metaFlags |= RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_FREQUENCY;
                        }
                        if (invenParams.m_metaFlags.m_RSSI)
                        {
                            metaFlags |= RFIDLIB.rfidlib_def.ISO18000p6C_META_BIT_MASK_RSSI;
                        }
                        if (invenParams.m_metaFlags.m_readCnt)
                        {
                            metaFlags |= RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_READCOUNT;
                        }
                        if (invenParams.m_metaFlags.m_tagData)
                        {
                            metaFlags |= RFIDLIB.rfidlib_def.ISO18000P6C_META_BIT_MASK_TAGDATA;
                        }
                        RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_SetInvenMetaDataFlags(AIPIso18000p6c, metaFlags);
                    }
                    // set access password
                    if (invenParams.m_read.m_enable || invenParams.m_write.m_enable || invenParams.m_lock.m_enable)
                    {
                        RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_SetInvenAccessPassword(AIPIso18000p6c, invenParams.m_accessPwd);
                    }
                }

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
                    Byte[] tagData = new Byte[256];
                    UInt32 nSize = (UInt32)tagData.Length;
                    UInt32 metaFlags = 0;

                    iret = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_ParseTagReport(TagDataReport, ref aip_id, ref tag_id, ref ant_id, ref metaFlags, tagData, ref nSize);
                    if (iret == 0)
                    {
                        String writeOper = "";
                        String lockOper = "";
                        if (invenParams.m_write.m_enable)
                        {
                            iret = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_CheckTAWriteResult(TagDataReport);
                            if (iret != 0)
                            {
                                writeOper = "fail";
                            }
                            else
                            {
                                writeOper = "success";

                            }
                        }
                        if (invenParams.m_lock.m_enable)
                        {
                            iret = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_CheckTALockResult(TagDataReport);
                            if (iret != 0)
                            {
                                lockOper = "fail";
                            }
                            else
                            {
                                lockOper = "success";
                            }
                        }
                        /*
                        object[] pList = { aip_id, tag_id, ant_id, metaFlags, tagData, nSize, writeOper, lockOper };
                        Invoke(tagReportHandler, pList);
                        */
                        results.Add(new UhfInventoryItem
                        {
                            aip_id = aip_id,
                            tag_id = tag_id,
                            ant_id = ant_id,
                            metaFlags = metaFlags,
                            tagData = tagData,
                            nSize = nSize,
                            writeOper = writeOper,
                            lockOper = lockOper
                        });
                    }

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

        private void button_inventory_Click(object sender, EventArgs e)
        {
            Inventory();
        }

        List<string> _epcs = new List<string>();

        void Inventory()
        {
            this.textBox_result.Text = "";

            int iret;
            Byte AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
            /*
            if (onlyNewTag == 1)
            {
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;  //only new tag inventory 

            }
            */

            PARAMETERS parameters = new PARAMETERS();

            {
                parameters.m_metaFlags.m_enable = true; // ckbMetaEnable.Checked;
                parameters.m_metaFlags.m_EPC = true;    // ckbMetaEPC.Checked;
                parameters.m_metaFlags.m_frequency = true;  // ckbMetaFrequency.Checked;
                parameters.m_metaFlags.m_readCnt = false;   // ckbMetaReadCnt.Checked;
                parameters.m_metaFlags.m_RSSI = false;  //  ckbMetaRSS.Checked;
                parameters.m_metaFlags.m_tagData = false;   // ckbMetaTagData.Checked;
                parameters.m_metaFlags.m_timestamp = false; // ckbMetaTimestamp.Checked;
                parameters.m_metaFlags.m_antennaID = false; // ckbMetaAntennaID.Checked;
            }

            {
                parameters.m_metaFlags.m_enable = false; // ckbMetaEnable.Checked;

                parameters.m_read.m_enable = true;
                parameters.m_read.m_memBank = 0x02; // USER Bank
                parameters.m_read.m_wordPtr = 0;
                parameters.m_read.m_wordCnt = 10;
            }


            byte[] AntennaSel = new byte[] { 1 };

            UInt32 nTagCount = 0;
            iret = tag_inventory(parameters,
                this._readerHandle,
                AIType,
                (byte)AntennaSel.Length,
                AntennaSel,
                out List<UhfInventoryItem> results,
                ref nTagCount);
            if (iret == 0)
            {
                // inventory ok

            }
            else
            {
                // inventory error 
                this.textBox_result.Text = "inventory error";
                return;
            }

            /*
            AIType = RFIDLIB.rfidlib_def.AI_TYPE_NEW;
            if (onlyNewTag == 1)
            {
                AIType = RFIDLIB.rfidlib_def.AI_TYPE_CONTINUE;  //only new tag inventory 
            }
            */

            /*
             *  If API RDR_SetCommuImmeTimeout is called when stop, API RDR_ResetCommuImmeTimeout 
             *  must be called too, Otherwise, an error -5 may occurs .
             */
            RFIDLIB.rfidlib_reader.RDR_ResetCommuImmeTimeout(this._readerHandle);

            // 显示结果
            {
                _epcs.Clear();

                StringBuilder text = new StringBuilder();
                int i = 0;
                text.AppendLine($"tag count: {results.Count}");
                foreach (var result in results)
                {

                    ParseDataResult parse_result = ParseData(result.metaFlags,
    result.tagData,
    result.nSize);

                    text.AppendLine($"{(i + 1)}) tag_id={result.tag_id}, aip_id={result.aip_id}, ant_id={result.ant_id}, metaFlags={result.metaFlags}, tagData={ByteArray.GetHexTimeStampString(result.tagData)}");
                    text.AppendLine($"EPC={parse_result.EPC}, {parse_result.ReadCount} Frequency={parse_result.Frequency}, RSSI={parse_result.RSSI}, Timestamp={parse_result.Timestamp}, ReadData={ByteArray.GetHexTimeStampString(parse_result.ReadData)}");

                    _epcs.Add(parse_result.EPC);
                }

                this.textBox_result.Text = text.ToString();
            }
        }

        private void button_readerData_Click(object sender, EventArgs e)
        {
            ReadDataDialog dialog = new ReadDataDialog();
            dialog.Epcs = _epcs;
            dialog.ReaderHandle = _readerHandle;
            dialog.ShowDialog(this);
        }
    }
}
