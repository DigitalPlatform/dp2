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
                    this.checkBox_action_forceLog,
                    new ComboBoxText(this.comboBox_action_location),
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
                    this.checkBox_action_forceLog,
                    new ComboBoxText(this.comboBox_action_location),
                    this.checkBox_action_slowMode,
                };
                GuiState.SetUiState(controls, value);
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            var control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            if (control == false
                && (this.checkBox_action_setCurrentLocation.Checked
                || this.checkBox_action_setLocation.Checked))
            {
                if (string.IsNullOrEmpty(this.comboBox_action_location.Text))
                {
                    strError = "当更新当前和永久位置的时候，馆藏地不允许为空";
                    goto ERROR1;
                }
            }

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

            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"BeginModifyDialog_Load() 装载数据出错: {ex.Message}");
                this.Close();
            }
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
                // get_result = InventoryData.sip_GetLocationListFromLocal();
                get_result = new GetLocationListResult
                {
                    List =
                    DataModel.GetSipLocationList()
                };
            }
            else
            {
                if (string.IsNullOrEmpty(DataModel.dp2libraryServerUrl))
                    throw new Exception("尚未配置 dp2library 服务器 URL");

                get_result = LibraryChannelUtil.GetLocationList();
            }

            if (get_result.Value == -1)
                throw new Exception($"获得馆藏地列表时出错: {get_result.ErrorInfo}");
            
            var old_value = this.comboBox_action_location.Text;
            this.comboBox_action_location.DataSource = get_result.List;
            this.comboBox_action_location.Text = old_value;

            string batchNo = "inventory_" + DateTime.Now.ToShortDateString();
            this.textBox_action_batchNo.Text = batchNo;

        }

        // 动作模式
        /* setUID               设置 UID --> PII 对照关系。即，写入册记录的 UID 字段
         * setCurrentLocation   设置册记录的 currentLocation 字段内容为当前层架标编号
         * setLocation          设置册记录的 location 字段为当前阅览室/书库位置。即调拨图书
         * verifyEAS            校验 RFID 标签的 EAS 状态是否正确。过程中需要检查册记录的外借状态
         * forceLog             transfer 请求时，即便没有对册记录发生实质性修改，也会被 dp2library 记入操作日志
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
                if (this.checkBox_action_forceLog.Checked == true)
                    values.Add("forceLog");

                return StringUtil.MakePathList(values);
            }
            set
            {
                this.checkBox_action_setUID.Checked = (StringUtil.IsInList("setUID", value));
                this.checkBox_action_setCurrentLocation.Checked = (StringUtil.IsInList("setCurrentLocation", value));
                this.checkBox_action_setLocation.Checked = (StringUtil.IsInList("setLocation", value));
                this.checkBox_action_verifyEas.Checked = (StringUtil.IsInList("verifyEAS", value));
                this.checkBox_action_forceLog.Checked = (StringUtil.IsInList("forceLog", value));
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

        private void checkBoxes_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox control = sender as CheckBox;
            if (control.Checked)
            {
                control.BackColor = Color.LightGreen;
            }
            else
            {
                control.BackColor = Color.Transparent;
            }

            // 只有当 设置当前位置 和 设置永久位置 启用的时候，forceLog 才会显示
            SetForceLogVisible();
        }

        void SetForceLogVisible()
        {
            if (this.checkBox_action_setCurrentLocation.Checked
                || this.checkBox_action_setLocation.Checked)
                this.checkBox_action_forceLog.Visible = true;
            else
                this.checkBox_action_forceLog.Visible = false;
        }
    }
}
