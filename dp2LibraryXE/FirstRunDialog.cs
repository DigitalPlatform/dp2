using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;
using System.Diagnostics;

namespace dp2LibraryXE
{
    public partial class FirstRunDialog : Form
    {
        public MainForm MainForm = null;

        public FirstRunDialog()
        {
            InitializeComponent();
        }

        private void button_prev_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex > 0)
            {
                this.tabControl_main.SelectedIndex--;
                SetTitle();
                SetButtonState();
            }
        }

        private void button_next_Click(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedIndex < this.tabControl_main.TabPages.Count - 1)
            {
                this.tabControl_main.SelectedIndex++;
                SetTitle();
                SetButtonState();
            }
        }

        void SetTitle()
        {
            this.Text = this.tabControl_main.SelectedTab.Text;
        }

        void SetButtonState()
        {
            if (this.tabControl_main.SelectedIndex == 0)
                this.button_prev.Enabled = false;
            else
                this.button_prev.Enabled = true;

            if (this.tabControl_main.SelectedIndex >= this.tabControl_main.TabPages.Count - 1)
                this.button_next.Enabled = false;
            else
            {
                if (this.tabControl_main.SelectedTab == this.tabPage_license
                    && this.checkBox_license_agree.Checked == false)
                    this.button_next.Enabled = false;
                else
                    this.button_next.Enabled = true;
            }

            if (this.tabControl_main.SelectedIndex == this.tabControl_main.TabPages.Count - 1)
                this.button_finish.Enabled = true;
            else
                this.button_finish.Enabled = false;
        }

        private void button_finish_Click(object sender, EventArgs e)
        {
            string strError = "";


            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void FirstRunDialog_Load(object sender, EventArgs e)
        {
            SetTitle();
            SetButtonState();
            LoadEula();
        }

        void LoadEula()
        {
            string strFileName = Path.Combine(this.MainForm.DataDir, "eula.txt");
            if (File.Exists(strFileName) == false)
                strFileName = Path.Combine(Environment.CurrentDirectory, "eula.txt");

            using (StreamReader sr = new StreamReader(strFileName, true))
            {
                this.textBox_license.Text = sr.ReadToEnd().Replace("\r\n", "\n").Replace("\n", "\r\n");   // 两个 Replace() 会将只有 LF 结尾的行处理为 CR LF
            }
        }

        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_license)
                this.textBox_license.Select(0, 0);
        }

        private void checkBox_license_agree_CheckedChanged(object sender, EventArgs e)
        {
            SetButtonState();
        }

        private void radioButton_licenseMode_testing_CheckedChanged(object sender, EventArgs e)
        {
            OnLicenseTypeChecked();
        }

        private void radioButton_licenseMode_standard_CheckedChanged(object sender, EventArgs e)
        {
            OnLicenseTypeChecked();
        }

        void OnLicenseTypeChecked()
        {
                // 社区
            if (this.radioButton_licenseMode_community.Checked == true)
            {
                this.radioButton_licenseMode_enterprise.Checked = false;
            }
            // 企业
            else if (this.radioButton_licenseMode_enterprise.Checked == true)
            {
                this.radioButton_licenseMode_community.Checked = false;
            }
        }

        /* 和以前兼容的 Mode 含义
test	-- community single
miniTest	-- community mini 这是新增的
standard	-- enterprise single
miniServer	-- enterprise mini
         * */
        public string Mode
        {
            get
            {
                if (this.LicenseType == "community")
                {
                    if (this.IsServer == false)
                        return "test";
                    return "miniTest";
                }
                if (this.IsServer == false)
                    return "standard";
                return "miniServer";
            }
            set
            {
                if (value == "test")
                {
                    this.LicenseType = "community";
                    this.IsServer = false;
                }
                else if (value == "miniTest")
                {
                    this.LicenseType = "community";
                    this.IsServer = true;
                }
                else if (value == "standard")
                {
                    this.LicenseType = "enterprise";
                    this.IsServer = false;
                }
                else if (value == "miniServer")
                {
                    this.LicenseType = "enterprise";
                    this.IsServer = true;
                }
                else
                {
                    throw new Exception("无法识别的 Mode 值 '"+value+"'");
                }
            }
        }

        /// <summary>
        /// 发行版类型
        /// </summary>
        string LicenseType
        {
            get
            {
                if (this.radioButton_licenseMode_community.Checked == true)
                    return "community";
                return "enterprise"; 
            }
            set
            {
                if (value == "community")
                    this.radioButton_licenseMode_community.Checked = true;
                else
                {
                    Debug.Assert(value == "enterprise", "");
                    this.radioButton_licenseMode_enterprise.Checked = true;
                }
            }
        }

        /// <summary>
        /// 是否为小型版
        /// </summary>
        public bool IsServer
        {
            get
            {
                return this.radioButton_mini.Checked;
            }
            set
            {
                this.radioButton_mini.Checked = value;
            }
        }

        private void radioButton_single_CheckedChanged(object sender, EventArgs e)
        {
            OnServerTypeChecked();
        }

        private void radioButton_mini_CheckedChanged(object sender, EventArgs e)
        {
            OnServerTypeChecked();
        }

        void OnServerTypeChecked()
        {
            if (this.radioButton_single.Checked == true)
                this.radioButton_mini.Checked = false;
            else if (this.radioButton_mini.Checked == true)
                this.radioButton_single.Checked = false;
        }
    }
}
