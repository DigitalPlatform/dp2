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
#if NO
            if (this.radioButton_licenseMode_testing.Checked == true)
                this.radioButton_licenseMode_standard.Checked = false;
            else
                this.radioButton_licenseMode_standard.Checked = true;
#endif
            OnChecked();
        }

        private void radioButton_licenseMode_standard_CheckedChanged(object sender, EventArgs e)
        {
#if NO
            if (this.radioButton_licenseMode_standard.Checked == true)
                this.radioButton_licenseMode_testing.Checked = false;
            else
                this.radioButton_licenseMode_testing.Checked = true;
#endif
            OnChecked();
        }

        void OnChecked()
        {
            if (this.radioButton_licenseMode_standard.Checked == true)
            {
                this.radioButton_licenseMode_testing.Checked = false;
                this.radioButton_licenseMode_miniServer.Checked = false;
            }
            else if (this.radioButton_licenseMode_testing.Checked == true)
            {
                this.radioButton_licenseMode_standard.Checked = false;
                this.radioButton_licenseMode_miniServer.Checked = false;
            }
            else if (this.radioButton_licenseMode_miniServer.Checked == true)
            {
                this.radioButton_licenseMode_standard.Checked = false;
                this.radioButton_licenseMode_testing.Checked = false;
            }
        }

        public string Mode
        {
            get
            {
                if (this.radioButton_licenseMode_testing.Checked == true)
                    return "test";
                if (this.radioButton_licenseMode_standard.Checked == true)
                    return "standard";
                return "miniServer";
            }
            set
            {
                if (value == "test")
                    this.radioButton_licenseMode_testing.Checked = true;
                else if (value == "standard")
                    this.radioButton_licenseMode_standard.Checked = true;
                else
                    this.radioButton_licenseMode_miniServer.Checked = true;
            }
        }
    }
}
