using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    public partial class InventoryForm : MyForm
    {
        QuickChargingForm _openMarcFileDialog = null;

        public InventoryForm()
        {
            InitializeComponent();

            _openMarcFileDialog = new QuickChargingForm();
            this.tabPage_scan.Padding = new Padding(4, 4, 4, 4);
            this.tabPage_scan.Controls.Add(_openMarcFileDialog.MainPanel);
            _openMarcFileDialog.MainPanel.Dock = DockStyle.Fill;

        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {

            this._openMarcFileDialog.SupressSizeSetting = true; // 避免保存窗口尺寸
            this._openMarcFileDialog.MainForm = this.MainForm;
            this._openMarcFileDialog.Show();
#if NO
                        // 输入的ISO2709文件名
            this._openMarcFileDialog.FileName = this.MainForm.AppInfo.GetString(
                "iso2709statisform",
                "input_iso2709_filename",
                "");
#endif
        }

        private void InventoryForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void InventoryForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._openMarcFileDialog.Close();
        }

        /// <summary>
        /// 允许或者禁止界面控件。在长操作前，一般需要禁止界面控件；操作完成后再允许
        /// </summary>
        /// <param name="bEnable">是否允许界面控件。true 为允许， false 为禁止</param>
        public override void EnableControls(bool bEnable)
        {
            this._openMarcFileDialog.MainPanel.Enabled = bEnable;

#if NO
            this.button_getProjectName.Enabled = bEnable;

            this.button_next.Enabled = bEnable;

            this.button_projectManage.Enabled = bEnable;
#endif
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
    Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                // MessageBox.Show(this, "test");
                this._openMarcFileDialog.DoEnter();
                return true;
            }

#if NO
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
#endif

            // return false;
            return base.ProcessDialogKey(keyData);
        }

    }
}
