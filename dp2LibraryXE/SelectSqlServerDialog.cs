using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2LibraryXE
{
    public partial class SelectSqlServerDialog : Form
    {
        public SelectSqlServerDialog()
        {
            InitializeComponent();
        }

        // 
        // 这篇文章是关于 .NET Framework 4 Runtime Update 2 的
        // https://support.microsoft.com/en-us/kb/2544514
        //
        //
        // 这篇文章介绍了如何从注册表检查各种 Update：
        // https://msdn.microsoft.com/en-us/library/hh925567(v=vs.110).aspx
        // HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2544514
        static bool IsKB2544514Installed()
        {
            try
            {
                using (RegistryKey service = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Updates\\Microsoft .NET Framework 4 Extended\\KB2544514"))
                {
                    return (string)service.GetValue("ThisVersionInstalled") == "Y";
                }
            }
            catch
            {
                return false;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (IsKB2544514Installed() == false)
            {
                MessageBox.Show(this, "MS SQL LocalDB 要求必须先安装 .NET Framework 4 Runtime Update 2。请先安装它(KB2544514)");
                Process.Start("IExplore.exe", "https://support.microsoft.com/en-us/kb/2544514");
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void SelectSqlServerDialog_Load(object sender, EventArgs e)
        {
            Refresh();
        }

        void Refresh()
        {
            if (IsLocalDBInstalled() == false)
                this.radioButton_localdb.Enabled = false;
            else
                this.radioButton_localdb.Enabled = true;
        }

        static bool IsLocalDBInstalled()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Microsoft SQL Server Local DB\\Installed Versions");
                if (key.SubKeyCount > 0)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void button_installLocalDB_Click(object sender, EventArgs e)
        {
            // https://www.microsoft.com/en-us/download/details.aspx?id=29062
            // Microsoft® SQL Server® 2012 Express (v11)

            // https://www.microsoft.com/en-us/download/details.aspx?id=42299
            // Microsoft® SQL Server® 2014 Express (v12)

            Process.Start("IExplore.exe", "https://www.microsoft.com/zh-cn/download/details.aspx?id=42299");
        }

        public string SelectedType
        {
            get
            {
                if (this.radioButton_sqlite.Checked == true)
                    return "sqlite";
                return "localdb";
            }
            set
            {
                if (value == "sqlite")
                    this.radioButton_sqlite.Checked = true;
                else
                    this.radioButton_localdb.Checked = true;
            }
        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
