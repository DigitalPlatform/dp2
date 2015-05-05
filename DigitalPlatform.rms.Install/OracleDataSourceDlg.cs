using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;

namespace DigitalPlatform.rms
{
    public partial class OracleDataSourceDlg : Form
    {
        public OracleDataSourceDlg()
        {
            InitializeComponent();
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

        public string KernelLoginName
        {
            get
            {
                return this.textBox_loginName.Text;
            }
            set
            {
                this.textBox_loginName.Text = value;
            }
        }

        public string KernelLoginPassword
        {
            get
            {
                return this.textBox_loginPassword.Text;
            }
            set
            {
                this.textBox_loginPassword.Text = value;
                this.textBox_confirmLoginPassword.Text = value;
            }
        }

        public string InstanceName
        {
            get
            {
                return this.textBox_instanceName.Text;
            }
            set
            {
                this.textBox_instanceName.Text = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.textBox_sqlServerName.Text == "")
            {
                strError = "尚未指定 Oracle SQL 服务器";
                goto ERROR1;
            }

            if (string.Compare(this.textBox_sqlServerName.Text.Trim(), "~sqlite") == 0)
            {
                strError = "Oracle SQL 服务器名不能为 '~sqlite'，因为这个名字保留给了 SQLite 内置数据库类型";
                goto ERROR1;
            }

            if (this.textBox_loginName.Text == "")
            {
                strError = "尚未指定 Oracle 用户名";
                goto ERROR1;
            }

            if (this.textBox_loginPassword.Text != this.textBox_confirmLoginPassword.Text)
            {
                strError = "Oracle帐户的密码和确认密码不一致";
                goto ERROR1;
            }

#if NO
            SaLoginDialog dlg = new SaLoginDialog();
            GuiUtil.AutoSetDefaultFont(dlg);
            dlg.SqlServerName = this.textBox_sqlServerName.Text;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;
#endif

#if NO
            // 创建dp2Kernel登录名

            // 创建一个适合于dpKernel的SQL Server login
            // return:
            //      -1  出错
            //      0   成功
            //      1   原来已经存在，且不允许删除
            nRet = CreateLogin(
                this.SqlServerName,
                dlg.SqlUserName,
                dlg.SqlPassword,
                dlg.SSPI,
                this.textBox_loginName.Text,
                this.textBox_loginPassword.Text,
                out strError);
            if (nRet == -1 || nRet == 1)
                goto ERROR1;
#endif

            this.button_OK.Enabled = false;
            try
            {
                nRet = VerifySqlServer(
                    this.SqlServerName,
                    this.textBox_loginName.Text,
                    this.textBox_loginPassword.Text,
                    false,
                    out strError);
                if (nRet == -1)
                {
                    DialogResult result = MessageBox.Show(this,
    "在检查服务器参数的过程中发生错误: \r\n\r\n"
    + strError
    + "\r\n\r\n是否依然采用这些参数继续完成安装?",
    "OracleDataSourceDlg",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        MessageBox.Show(this, "请修改服务器参数");
                        return;
                    }
                }
            }
            finally
            {
                this.button_OK.Enabled = true;
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

        public int VerifySqlServer(
            string strSqlServerName,
            string strSqlUserName,
            string strSqlUserPassword,
            bool bSSPI,
            out string strError)
        {
            strError = "";

            string strConnection = @"Persist Security Info=False;"
                + "User ID=" + strSqlUserName + ";"    //帐户和密码
                + "Password=" + strSqlUserPassword + ";"
                + "Data Source=" + strSqlServerName + ";"
                + "Connect Timeout=30";

            if (bSSPI == true)
            {
                strConnection = @"Persist Security Info=False;"
                    + "Integrated Security=SSPI; "      //信任连接
                    + "Data Source=" + strSqlServerName + ";"
                    + "Connect Timeout=30"; // 30秒
            }

            OracleConnection connection = null;
            try
            {
                connection = new OracleConnection(strConnection);
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
            catch (OracleException sqlEx)
            {
                strError = "连接 SQL 数据库出错： " + sqlEx.Message + "。";
                int nError = sqlEx.ErrorCode;
                return -1;
            }
            catch (Exception ex)
            {
                strError = "连接 SQL 数据库出错： " + ex.Message + " 类型:" + ex.GetType().ToString();
                return -1;
            }

            return 0;
        }
    }
}
