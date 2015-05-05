using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// MS SQL Server 管理员账户登录对话框
    /// </summary>
    public partial class SaLoginDialog : Form
    {
        public SaLoginDialog()
        {
            InitializeComponent();
        }

        private void SaLoginDialog_Load(object sender, EventArgs e)
        {
            radioButton_SSPI_CheckedChanged(null, null);
        }

        private void button_detect_Click(object sender, EventArgs e)
        {
            if (this.textBox_sqlServerName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL服务器名，无法检测。");
                return;
            }

            if (this.SSPI == false && this.textBox_sqlUserName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL用户，无法检测。");
                return;
            }

            EnableControls(false);
            string strError = "";
            int nRet = this.detect(this.textBox_sqlServerName.Text,
                this.textBox_sqlUserName.Text,
                this.textBox_sqlPassword.Text,
                this.radioButton_SSPI.Checked,
                out strError);
            EnableControls(true);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                MessageBox.Show(this, "SQL服务器信息正确。");
            }
        }

        public string SqlServerName
        {
            get
            {
                return this.textBox_sqlServerName.Text;
            }
            set
            {
                this.textBox_sqlServerName.Text = value;
            }
        }

        public string SqlUserName
        {
            get
            {
                return this.textBox_sqlUserName.Text;
            }
            set
            {
                this.textBox_sqlUserName.Text = value;
            }
        }

        public string SqlPassword
        {
            get
            {
                return this.textBox_sqlPassword.Text;
            }
            set
            {
                this.textBox_sqlPassword.Text = value;
            }
        }

        public bool SSPI
        {
            get
            {
                return this.radioButton_SSPI.Checked;
            }
            set
            {
                this.radioButton_SSPI.Checked = true;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_sqlServerName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL服务器。");
                return;
            }

            if (this.SSPI == false && this.textBox_sqlUserName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL帐号。");
                return;
            }


            // 检测SQL帐户是否正确
            EnableControls(false);
            string strError = "";
            int nRet = this.detect(this.textBox_sqlServerName.Text,
                this.textBox_sqlUserName.Text,
                this.textBox_sqlPassword.Text,
                radioButton_SSPI.Checked,
                out strError);
            EnableControls(true);
            if (nRet == -1)
            {
                strError = strError + "\r\n" + "请重新指定登录信息。";
                MessageBox.Show(this, strError);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // return:
        //      -1  出错
        //      0   成功
        public int detect(string strSqlServerName,
            string strSqlUserName,
            string strPassword,
            bool bSSPI,
            out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            if (bSSPI == true)
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + strSqlServerName + ";"
                    + "Connect Timeout=30"; // 30秒
            }


            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(strConnection);
            }
            catch (Exception ex)
            {
                strError = "建立连接出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                connection.Open();
            }
            catch (SqlException sqlEx)
            {
                strError = "连接SQL数据库发生出错：" + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接SQL数据库发生出错：" + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            try
            {
                string strCommand = "";
                SqlCommand command = null;
                strCommand = "use master " + "\n";


                command = new SqlCommand(strCommand,
                    connection);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    strError = "执行命令出错：" + ex.Message + " 类型：" + ex.GetType().ToString();
                    return -1;
                }
            }
            finally
            {
                connection.Close();
            }

            return 0;
        }

        public void EnableControls(bool bEnable)
        {
            this.textBox_sqlServerName.Enabled = bEnable;

            if (this.radioButton_SSPI.Checked == true)
            {
                this.textBox_sqlUserName.Enabled = false;
                this.textBox_sqlPassword.Enabled = false;
            }
            else
            {
                this.textBox_sqlUserName.Enabled = bEnable;
                this.textBox_sqlPassword.Enabled = bEnable;
            }

            this.button_detect.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
        }

        private void radioButton_SSPI_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SSPI.Checked == true)
            {
                this.textBox_sqlUserName.Enabled = false;
                this.textBox_sqlPassword.Enabled = false;
            }
            else
            {
                this.textBox_sqlUserName.Enabled = true;
                this.textBox_sqlPassword.Enabled = true;
            }

        }

        private void radioButton_sqlAccount_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SSPI.Checked == true)
            {
                this.textBox_sqlUserName.Enabled = false;
                this.textBox_sqlPassword.Enabled = false;
            }
            else
            {
                this.textBox_sqlUserName.Enabled = true;
                this.textBox_sqlPassword.Enabled = true;
            }
        }
    }
}