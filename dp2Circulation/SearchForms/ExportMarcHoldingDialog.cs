using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 指定如何在 MARC 记录中包含册记录信息的对话框
    /// </summary>
    public partial class ExportMarcHoldingDialog : Form
    {
        public ExportMarcHoldingDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public bool Create905
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_905.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_905.Checked = value;
                });
            }
        }

        public bool RemoveOld905
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_removeOld905.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_removeOld905.Checked = value;
                });
            }
        }

        // 如何创建 905 字段?
        public string Style905
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.comboBox_905_style.Text;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.comboBox_905_style.Text = value;
                });
            }
        }

        public bool Create906
        {
            get
            {
                return this.TryGet(() =>
                {
                    return this.checkBox_906.Checked;
                });
            }
            set
            {
                this.TryInvoke(() =>
                {
                    this.checkBox_906.Checked = value;
                });
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkBox_905);
                controls.Add(this.comboBox_905_style);
                controls.Add(this.checkBox_removeOld905);
                controls.Add(this.checkBox_906);
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.checkBox_905);
                controls.Add(this.comboBox_905_style);
                controls.Add(this.checkBox_removeOld905);
                controls.Add(this.checkBox_906);
                GuiState.SetUiState(controls, value);
            }
        }

        private void checkBox_905_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_905.Checked)
            {
                this.comboBox_905_style.Visible = true;
                this.checkBox_removeOld905.Visible = true;
            }
            else
            {
                this.comboBox_905_style.Visible = false;
                this.checkBox_removeOld905.Visible = false;
            }
        }

    }
}
