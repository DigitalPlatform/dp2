using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestUHF
{
    public partial class ReadDataDialog : Form
    {
        List<string> _epcs = new List<string>();

        public List<string> Epcs
        {
            get
            {
                return _epcs;
            }
            set
            {
                _epcs.Clear();
                if (value != null)
                    _epcs.AddRange(value);

                this.combobox_epcList.Items.Clear();
                this.combobox_epcList.Items.AddRange(_epcs.ToArray());
            }
        }

        public UIntPtr ReaderHandle { get; set; }

        public ReadDataDialog()
        {
            InitializeComponent();
        }

        private void ReadDataDialog_Load(object sender, EventArgs e)
        {
            int i;
            for (i = 0; i <= 32; i++)
            {
                this.combobox_startWord.Items.Add(i.ToString());
            }

            for (i = 0; i < 32; i++)
            {
                this.combobox_wordCount.Items.Add(i.ToString());
            }
            /*
            combobox_startWord.SelectedIndex = 2;
            combobox_wordCount.SelectedIndex = 1;
            */
            combobox_startWord.SelectedIndex = 0;
            combobox_wordCount.SelectedIndex = 0;

            this.combobox_epcList.SelectedIndex = 0;
            this.combobox_memoryBank.SelectedIndex = 1;
        }

        private void button_read_Click(object sender, EventArgs e)
        {
            string strError = "";

            UIntPtr ht = UIntPtr.Zero;
            try
            {
                int WordCnt;
                int WordPointer;
                int iret;
                Byte memBank;
                Byte[] epc = null;
                Byte[] readData = new Byte[256];
                Byte[] accessPwdBytes = null;
                UInt32 m_accessPwd = 0;
                UInt32 nSize = (UInt32)readData.Length;

                if (this.combobox_epcList.Text == "")
                {
                    strError = "尚未指定要读取的标签的 EPC";
                    goto ERROR1;
                }

                epc = ByteArray.GetTimeStampByteArray(this.combobox_epcList.Text);
                // epc = StringToByteArrayFastest(cbbEPCs.Text);
                if (epc == null)
                {
                    strError = "GetTimeStampByteArray() error";
                    goto ERROR1;
                }

                /*
                if (textBox1.Text.Length != 8)
                {
                    MessageBox.Show("Wrong access password");
                    goto Finish;
                }


                accessPwdBytes = StringToByteArrayFastest(textBox1.Text);
                m_accessPwd = (UInt32)(accessPwdBytes[0] | (accessPwdBytes[1] << 8 & 0xff00) | (accessPwdBytes[2] << 16 & 0xff0000) | (accessPwdBytes[3] << 24 & 0xff000000));
                */

                WordCnt = Convert.ToInt32(this.combobox_wordCount.Text);
                WordPointer = Convert.ToInt32(this.combobox_startWord.Text);

                switch (this.combobox_memoryBank.SelectedIndex)
                {
                    case 0:
                        memBank = (Byte)RFIDLIB.rfidlib_def.ISO18000p6C_MEM_BANK_RFU;
                        break;
                    case 1:
                        memBank = (Byte)RFIDLIB.rfidlib_def.ISO18000p6C_MEM_BANK_EPC;
                        break;
                    case 2:
                        memBank = (Byte)RFIDLIB.rfidlib_def.ISO18000p6C_MEM_BANK_TID;
                        break;
                    case 3:
                        memBank = (Byte)RFIDLIB.rfidlib_def.ISO18000p6C_MEM_BANK_USER;
                        break;
                    default:
                        memBank = (Byte)RFIDLIB.rfidlib_def.ISO18000p6C_MEM_BANK_EPC;
                        break;
                }

                /*
                AntennaSel = GetSelectAntennas();
                if (AntennaSel == null)
                {
                    AntennaSelCount = 0;
                }
                else
                {
                    AntennaSelCount = (Byte)AntennaSel.Length;
                }
                */

                byte[] AntennaSel = new byte[] { 1 };   // 号码到底从 0 开始还是从 1 开始，要查配置文件
                byte AntennaSelCount = 1;

                iret = RFIDLIB.rfidlib_reader.RDR_SetMultiAccessAntennas(this.ReaderHandle, 
                    AntennaSel, 
                    AntennaSelCount);
                if (iret != 0)
                {
                    strError = "RDR_SetMultiAccessAntennas() error";
                    goto ERROR1;
                }

                iret = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_Connect(this.ReaderHandle,
                    0, 
                    epc,
                    (Byte)epc.Length,
                    m_accessPwd, 
                    ref ht);
                if (iret != 0)
                {
                    strError = "ISO18000p6C_Connect() error";
                    goto ERROR1;
                }

                iret = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_Read(this.ReaderHandle,
                    ht, 
                    memBank, 
                    (UInt32)WordPointer,
                    (UInt32)WordCnt,
                    readData,
                    ref nSize);
                //iret = RFIDLIB.rfidlib_aip_iso18000p6C.ISO18000p6C_Write(hreader, ht, memBank, (UInt32)WordPointer, (UInt32)WordCnt, writeData, (UInt32)writeData.Length);

                if (iret != 0)
                {
                    strError = "ISO18000p6C_Read() error";
                    goto ERROR1;
                }

                this.textbox_data.Text = BitConverter.ToString(readData, 0, (int)nSize).Replace("-", string.Empty);
                return;
            }
            finally
            {
                if (ht != UIntPtr.Zero)
                {
                    RFIDLIB.rfidlib_reader.RDR_TagDisconnect(this.ReaderHandle, ht);
                    ht = UIntPtr.Zero;
                }
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
