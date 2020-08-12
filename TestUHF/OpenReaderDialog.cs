using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Text;

namespace TestUHF
{
    public partial class OpenReaderDialog : Form
    {
        public List<CReaderDriverInf> readerDriverInfoList = new List<CReaderDriverInf>();

        public OpenReaderDialog()
        {
            InitializeComponent();
        }

        private void OpenReaderDialog_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                FillReaderTypes();

                if (_uiState != null)
                {
                    this.Invoke((Action)(() =>
                    {
                        UiState = _uiState;
                        _uiState = null;
                    }));
                }
            });
        }

        bool _filled = false;

        void FillReaderTypes()
        {
            string path = "\\x86\\Drivers";
            if (IntPtr.Size == 8)
                path = "\\x64\\Drivers";
            int ret = RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers(
                path
                );
            // RFIDLIB.rfidlib_reader.RDR_LoadReaderDrivers("\\Drivers");
            /* enum and show loaded reader driver */
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

            this.Invoke((Action)(() =>
            {
                foreach (var driver in readerDriverInfoList)
                {
                    this.comboBox_readerType.Items.Add(driver.m_name);
                }

                if (this.comboBox_readerType.Items.Count > 0)
                    this.comboBox_readerType.SelectedIndex = 0;
            }));

            _filled = true;
        }

        List<string> ListUsbSerialNumber(string readerType)
        {
            var driver = readerDriverInfoList.Find(o => o.m_name == readerType);
            if (driver == null)
                return new List<string>();

            if ((driver.m_commTypeSupported & RFIDLIB.rfidlib_def.COMMTYPE_USB_EN) > 0)
            {
                List<string> results = new List<string>();

                UInt32 nCount = RFIDLIB.rfidlib_reader.HID_Enum(driver.m_name);
                int iret;
                int i;
                for (i = 0; i < nCount; i++)
                {
                    StringBuilder sernum = new StringBuilder();
                    sernum.Append('\0', 64);
                    UInt32 nSize;
                    nSize = (UInt32)sernum.Capacity;
                    iret = RFIDLIB.rfidlib_reader.HID_GetEnumItem((UInt32)i, RFIDLIB.rfidlib_def.HID_ENUM_INF_TYPE_SERIALNUM, sernum, ref nSize);
                    if (iret == 0)
                    {
                        results.Add(sernum.ToString());
                    }
                }

                return results;
            }
            return new List<string>();
        }

        private void comboBox_readerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {

                // 更新 USB 设备 SerialNumber 列表
                this.comboBox_usbSerialNumber.Items.Clear();
                this.comboBox_usbSerialNumber.Items.AddRange(ListUsbSerialNumber(this.comboBox_readerType.Text).ToArray());
            }
            finally
            {
                this.Cursor = oldCursor;
            }
        }

        public string BuildConnectionString()
        {
            if (this.tabControl1.SelectedTab == this.tabPage_usb)
            {
                return RFIDLIB.rfidlib_def.CONNSTR_NAME_RDTYPE + "=" + this.comboBox_readerType.Text + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE + "=" + RFIDLIB.rfidlib_def.CONNSTR_NAME_COMMTYPE_USB + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDADDRMODE + "=" + this.comboBox_usbOpenType.SelectedIndex.ToString() + ";" +
                          RFIDLIB.rfidlib_def.CONNSTR_NAME_HIDSERNUM + "=" + this.comboBox_usbSerialNumber.Text;
            }

            return "";
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();

            if (string.IsNullOrEmpty(this.comboBox_readerType.Text))
                errors.Add("尚未选择 Reader Type");

            if (this.tabControl1.SelectedTab == this.tabPage_usb)
            {
                if (string.IsNullOrEmpty(this.comboBox_usbOpenType.Text) == true)
                    errors.Add("尚未选择 USB Open Type");

                if (this.comboBox_usbOpenType.Text == "Serial number"
                    && string.IsNullOrEmpty(this.comboBox_usbSerialNumber.Text) == true)
                    errors.Add("尚未选择 USB Serial Number");

            }

            if (errors.Count > 0)
                goto ERROR1;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, $"{StringUtil.MakePathList(errors, "; ")}");
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        string _uiState = null;

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.comboBox_readerType);
                controls.Add(this.comboBox_usbOpenType);

                return GuiState.GetUiState(controls);
            }
            set
            {
                if (_filled == false)
                {
                    _uiState = value;
                    return;
                }

                List<object> controls = new List<object>();
                controls.Add(this.comboBox_readerType);
                controls.Add(this.comboBox_usbOpenType);

                GuiState.SetUiState(controls, value);
            }
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
