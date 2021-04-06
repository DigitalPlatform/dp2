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
using static dp2Inventory.LibraryChannelUtil;

namespace dp2Inventory
{
    public partial class BeginInventoryDialog : Form
    {
        public BeginInventoryDialog()
        {
            InitializeComponent();
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.checkBox_action_setUID,
                    this.checkBox_action_setCurrentLocation,
                    this.checkBox_action_setLocation,
                    this.checkBox_action_verifyEas,
                    this.comboBox_action_location,
                    this.checkBox_action_slowMode,
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.checkBox_action_setUID,
                    this.checkBox_action_setCurrentLocation,
                    this.checkBox_action_setLocation,
                    this.checkBox_action_verifyEas,
                    this.comboBox_action_location,
                    this.checkBox_action_slowMode,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void BeginModifyDialog_Load(object sender, EventArgs e)
        {
            // this.textBox_verifyRule.Text = DataModel.PiiVerifyRule;

            LoadData();
        }

        public bool ActionSetUID
        {
            get
            {
                return this.checkBox_action_setUID.Checked;
            }
            set
            {
                this.checkBox_action_setUID.Checked = value;
            }
        }

        void LoadData()
        {
            // 获得馆藏地列表
            GetLocationListResult get_result = null;
            if (DataModel.Protocol == "sip")
            {
                // SIP2 协议模式下需要在 inventory.xml 中 root/library/@locationList 中配置馆藏地列表
                get_result = InventoryData.sip_GetLocationListFromLocal();
            }
            else
                get_result = LibraryChannelUtil.GetLocationList();
            if (get_result.Value == -1)
                throw new Exception($"获得馆藏地列表时出错: {get_result.ErrorInfo}");

            this.comboBox_action_location.DataSource = get_result.List;

            string batchNo = "inventory_" + DateTime.Now.ToShortDateString();
            this.textBox_action_batchNo.Text = batchNo;

        }

        // 动作模式
        /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
         * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
         * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
         * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
         * */

        public string ActionMode
        {
            get
            {
                List<string> values = new List<string>();

                if (this.checkBox_action_setUID.Checked == true)
                    values.Add("setUID");
                if (this.checkBox_action_setCurrentLocation.Checked == true)
                    values.Add("setCurrentLocation");
                if (this.checkBox_action_setLocation.Checked == true)
                    values.Add("setLocation");
                if (this.checkBox_action_verifyEas.Checked == true)
                    values.Add("verifyEAS");

                return StringUtil.MakePathList(values);
            }
            set
            {
                this.checkBox_action_setUID.Checked = (StringUtil.IsInList("setUID", value));
                this.checkBox_action_setCurrentLocation.Checked = (StringUtil.IsInList("setCurrentLocation", value));
                this.checkBox_action_setLocation.Checked = (StringUtil.IsInList("setLocation", value));
                this.checkBox_action_verifyEas.Checked = (StringUtil.IsInList("verifyEAS", value));
            }
        }

        public string BatchNo
        {
            get
            {
                return this.textBox_action_batchNo.Text;
            }
            set
            {
                this.textBox_action_batchNo.Text = value;
            }
        }

        public string LocationString
        {
            get
            {
                return this.comboBox_action_location.Text;
            }
            set
            {
                this.comboBox_action_location.Text = value;
            }
        }

        public bool SlowMode
        {
            get
            {
                return this.checkBox_action_slowMode.Checked;
            }
            set
            {
                this.checkBox_action_slowMode.Checked = value;
            }
        }
    }
}
