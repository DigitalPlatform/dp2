using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.rms.Client.rmsws_localhost;

namespace DigitalPlatform.LibraryServer
{
    public partial class InstallLibraryParamDlg : Form
    {
        string ManagerUserName = "root";
        string ManagerPassword = "";
        bool SavePassword = true;

        // public string RootDir = "";

        // bool bBbsUserNameTouched = false;
        // int nNest = 0;  // 是否在修改文字的嵌套事件中


        public InstallLibraryParamDlg()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            this.Update();

            try
            {
                string strError = "";

                if (this.KernelUrl == "")
                {
                    MessageBox.Show(this, "尚未输入dp2Kernel服务器的URL");
                    return;
                }

                // 验证asmx是否存在？

                if (this.ManageUserName == "")
                {
                    MessageBox.Show(this, "尚未指定代理帐户用户名。");
                    return;
                }

                if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
                {
                    strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                    MessageBox.Show(this, strError);
                    return;
                }



                // 验证代理帐户用户是否已经可用？
                // return:
                //       -1  出错
                //      0   不存在
                //      1   存在, 且密码一致
                //      2   存在, 但密码不一致
                int nRet = DetectManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "验证代理帐户时发现问题: " + strError + "\r\n\r\n请确保代理帐户已经正确创建");
                    return;
                }

                if (nRet == 2)
                {
                    string strText = "代理帐户已经存在, 但是其密码和当前面板中拟设置的密码不一致。\r\n\r\n是否要重设其密码?\r\n\r\n(Yes 重设密码并继续安装；No 不重设密码并继续安装；Cancel 返回设置面板)";
                    DialogResult result = MessageBox.Show(this,
                        strText,
                        "setup_dp2library",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        nRet = ResetManageUserPassword(out strError);
                        if (nRet == -1)
                        {
                            MessageBox.Show(this, "重设代理帐户密码时出错: " + strError + "\r\n\r\n请确保代理帐户已经正确创建");
                            return;
                        }
                    }

                    if (result == DialogResult.Cancel)
                        return; // 返回面板
                }

                // root用户不存在
                else if (nRet == 0)
                {
                    // 自动创立?
                    string strText = "代理帐户 '" + this.textBox_manageUserName.Text + "' 尚未创建, 是否创建之?\r\n\r\n(OK 创建；Cancel 不创建，返回设置面板)";
                    DialogResult result = MessageBox.Show(this,
                        strText,
                        "setup_dp2library",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        return;

                    nRet = CreateManageUser(out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(this, strError);
                        return;
                    }
                }


            }
            finally
            {
                EnableControls(true);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string KernelUrl
        {
            get
            {
                return this.textBox_kernelUrl.Text;
            }
            set
            {
                this.textBox_kernelUrl.Text = value;
            }
        }

        public string ManageUserName
        {
            get
            {
                return this.textBox_manageUserName.Text;
            }
            set
            {
                this.textBox_manageUserName.Text = value;
            }
        }

        public string ManagePassword
        {
            get
            {
                return this.textBox_managePassword.Text;
            }
            set
            {
                this.textBox_managePassword.Text = value;
                this.textBox_confirmManagePassword.Text = value;
            }
        }

        // 检测管理用户是否已经存在?
        // return:
        //       -1  出错
        //      0   不存在
        //      1   存在, 且密码一致
        //      2   存在, 但密码不一致
        int DetectManageUser(out string strError)
        {
            strError = "";
            if (this.textBox_kernelUrl.Text == "")
            {
                strError = "尚未指定dp2Kernel服务器URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (RmsChannelCollection channels = new RmsChannelCollection())
            {
                RmsChannel channel = channels.GetChannel(this.textBox_kernelUrl.Text);
                if (channel == null)
                {
                    strError = "channel == null";
                    return -1;
                }

                // Debug.Assert(false, "");

                int nRet = channel.Login(this.textBox_manageUserName.Text,
                    this.textBox_managePassword.Text,
                    out strError);
                if (nRet == -1)
                {
                    strError = "以用户名 '" + this.textBox_manageUserName.Text + "' 和密码登录失败: " + strError;
                    return -1;
                }

                channel.DoLogout(out strError);

                if (nRet == 0)
                {
                    channels.AskAccountInfo -= new AskAccountInfoEventHandle(channels_AskAccountInfo);
                    channels.AskAccountInfo += new AskAccountInfoEventHandle(channels_AskAccountInfo);

                    nRet = channel.UiLogin("为确认代理帐户是否存在, 请用root用户身份登录。",
                        "",
                        LoginStyle.None,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                    {
                        strError = "以root用户身份登录失败: " + strError + "\r\n\r\n因此无法确定代理帐户是否存在";
                        return -1;
                    }

                    string strRecPath = "";
                    string strXml = "";
                    byte[] baTimeStamp = null;

                    // 获得用户记录
                    // return:
                    //      -1  error
                    //      0   not found
                    //      >=1   检索命中的条数
                    nRet = GetUserRecord(
                        channel,
                        this.textBox_manageUserName.Text,
                        out strRecPath,
                        out strXml,
                        out baTimeStamp,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "获取用户 '" + this.textBox_manageUserName.Text + "' 信息时发生错误: " + strError + "\r\n\r\n因此无法确定代理帐户是否存在。";
                        return -1;
                    }

                    if (nRet == 1)
                    {
                        strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 已经存在, 但其密码和当前面板拟设置的密码不一致。";
                        return 2;
                    }
                    if (nRet >= 1)
                    {
                        strError = "以 '" + this.textBox_manageUserName.Text + "' 为用户名 的用户记录存在多条，这是一个严重错误，请利用root身份启用dp2manager尽快修正此错误。";
                        return -1;
                    }

                    strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 不存在。";
                    return 0;
                }

                strError = "代理帐户 '" + this.textBox_manageUserName.Text + "' 代理帐户经检验存在。";
                return 1;
            }
        }

        // 获得用户记录
        // return:
        //      -1  error
        //      0   not found
        //      >=1   检索命中的条数
        public static int GetUserRecord(
            RmsChannel channel,
            string strUserName,
            out string strRecPath,
            out string strXml,
            out byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            strXml = "";
            strRecPath = "";
            baTimeStamp = null;

            if (strUserName == "")
            {
                strError = "用户名为空";
                return -1;
            }

            string strQueryXml = "<target list='" + Defs.DefaultUserDb.Name
                + ":" + Defs.DefaultUserDb.SearchPath.UserName + "'><item><word>"
                + strUserName + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>chi</lang></target>";

            long nRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOutputStyle
                out strError);
            if (nRet == -1)
            {
                strError = "检索帐户库时出错: " + strError;
                return -1;
            }

            if (nRet == 0)
                return 0;	// not found

            long nSearchCount = nRet;

            nRet = channel.DoGetSearchResult(
                "default",
                1,
                "zh",
                null,	// stop,
                out List<string> aPath,
                out strError);
            if (nRet == -1)
            {
                strError = "检索注册用户库获取检索结果时出错: " + strError;
                return -1;
            }
            if (aPath.Count == 0)
            {
                strError = "检索注册用户库获取的检索结果为空";
                return -1;
            }

            // strRecID = ResPath.GetRecordId((string)aPath[0]);
            strRecPath = (string)aPath[0];

            string strStyle = "content,data,timestamp,withresmetadata";
            string strMetaData;
            string strOutputPath;

            nRet = channel.GetRes((string)aPath[0],
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获取注册用户库记录体时出错: " + strError;
                return -1;
            }

            return (int)nSearchCount;
        }

        // 创建代理帐户
        int CreateManageUser(out string strError)
        {
            strError = "";
            if (this.textBox_kernelUrl.Text == "")
            {
                strError = "尚未指定dp2Kernel服务器URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (RmsChannelCollection channels = new RmsChannelCollection())
            {
                channels.AskAccountInfo -= new AskAccountInfoEventHandle(channels_AskAccountInfo);
                channels.AskAccountInfo += new AskAccountInfoEventHandle(channels_AskAccountInfo);

                RmsChannel channel = channels.GetChannel(this.textBox_kernelUrl.Text);
                if (channel == null)
                {
                    strError = "channel == null";
                    return -1;
                }

                int nRet = channel.UiLogin("请用root用户身份登录，以便创建代理帐户。",
                    "",
                    LoginStyle.None,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以root用户身份登录失败: " + strError;
                    return -1;
                }

                // 获得用户库名


                string strRecPath = "";
                string strXml = "";
                byte[] baTimeStamp = null;

                // 查重，看这个用户名是否已经存在
                // 获得用户记录
                // return:
                //      -1  error
                //      0   not found
                //      >=1   检索命中的条数
                nRet = GetUserRecord(
                    channel,
                    this.textBox_manageUserName.Text,
                    out strRecPath,
                    out strXml,
                    out baTimeStamp,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取用户 '" + this.textBox_manageUserName.Text + "' 信息时发生错误: " + strError;
                    return -1;
                }

                if (nRet == 1)
                {
                    strError = "用户 '" + this.textBox_manageUserName.Text + "' 已经存在。";
                    return -1;
                }
                if (nRet >= 1)
                {
                    strError = "以 '" + this.textBox_manageUserName.Text + "' 为用户名 的用户记录存在多条，这是一个严重错误，请利用root身份启用dp2manager尽快修正此错误。";
                    return -1;
                }

                // 构造一条用户记录写入
                nRet = BuildUserRecord(out strXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "构造用户记录时发生错误: " + strError;
                    return -1;
                }

                string strOutputPath = "";
                byte[] baOutputTimeStamp;

                if (strRecPath == "")
                    strRecPath = Defs.DefaultUserDb.Name + "/" + "?";

                long lRet = channel.DoSaveTextRes(
                    strRecPath,
                    strXml,
                    false,	// bInlucdePreamble
                    "",	// style
                    baTimeStamp,	// baTimeStamp,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存用户记录时发生错误: " + strError;
                    return -1;
                }

                channel.DoLogout(out strError);
                return 0;
            }
        }

        // 重设置代理帐户密码
        int ResetManageUserPassword(out string strError)
        {
            strError = "";
            if (this.textBox_kernelUrl.Text == "")
            {
                strError = "尚未指定dp2Kernel服务器URL";
                return -1;
            }

            if (this.textBox_manageUserName.Text == "")
            {
                strError = "尚未指定代理帐户的用户名";
                return -1;
            }

            if (this.textBox_managePassword.Text != this.textBox_confirmManagePassword.Text)
            {
                strError = "代理帐户 密码 和 再次输入密码 不一致。请重新输入。";
                return -1;
            }

            using (RmsChannelCollection channels = new RmsChannelCollection())
            {
                channels.AskAccountInfo -= new AskAccountInfoEventHandle(channels_AskAccountInfo);
                channels.AskAccountInfo += new AskAccountInfoEventHandle(channels_AskAccountInfo);

                RmsChannel channel = channels.GetChannel(this.textBox_kernelUrl.Text);
                if (channel == null)
                {
                    strError = "channel == null";
                    return -1;
                }

                int nRet = channel.UiLogin("请用root用户身份登录，以便重设代理帐户密码。",
                    "",
                    LoginStyle.None,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    strError = "以root用户身份登录失败: " + strError;
                    return -1;
                }

                // 获得用户库名
                string strRecPath = "";
                string strXml = "";
                byte[] baTimeStamp = null;

                // 查重，看这个用户名是否已经存在
                // 获得用户记录
                // return:
                //      -1  error
                //      0   not found
                //      >=1   检索命中的条数
                nRet = GetUserRecord(
                    channel,
                    this.textBox_manageUserName.Text,
                    out strRecPath,
                    out strXml,
                    out baTimeStamp,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取用户 '" + this.textBox_manageUserName.Text + "' 信息时发生错误: " + strError;
                    return -1;
                }

                if (nRet == 0)
                {
                    strError = "用户 '" + this.textBox_manageUserName.Text + "' 尚不存在，因此无法重设其密码。请直接创建。";
                    return -1;
                }

                if (nRet > 1)
                {
                    strError = "以 '" + this.textBox_manageUserName.Text + "' 为用户名 的用户记录存在多条，这是一个严重错误，请利用root身份启用dp2manager尽快修正此错误。";
                    return -1;
                }

                // 修改密码
                nRet = ResetUserRecordPassword(ref strXml,
                    out strError);
                if (nRet == -1)
                {
                    strError = "构造用户记录时发生错误: " + strError;
                    return -1;
                }

                string strOutputPath = "";
                byte[] baOutputTimeStamp;

                if (strRecPath == "")
                {
                    Debug.Assert(false, "不可能出现的情况。");
                    strRecPath = Defs.DefaultUserDb.Name + "/" + "?";
                }

                long lRet = channel.DoSaveTextRes(
                    strRecPath,
                    strXml,
                    false,	// bInlucdePreamble
                    "",	// style
                    baTimeStamp,	// baTimeStamp,
                    out baOutputTimeStamp,
                    out strOutputPath,
                    out strError);
                if (lRet == -1)
                {
                    strError = "保存用户记录时发生错误: " + strError;
                    return -1;
                }

                channel.DoLogout(out strError);
                return 0;
            }
        }

        int ResetUserRecordPassword(ref string strXml,
    out string strError)
        {
            strError = "";

            XmlDocument UserRecDom = new XmlDocument();
            try
            {
                UserRecDom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "用户记录XML装载到DOM时出错: " + ex.Message;
                return -1;
            }


            // 密码
            DomUtil.SetElementText(UserRecDom.DocumentElement,
               "password",
                Cryptography.GetSHA1(this.textBox_managePassword.Text));

            /*
            XmlNode nodeServer = UserRecDom.DocumentElement.SelectSingleNode("server");
            if (nodeServer == null)
            {
                Debug.Assert(false, "不可能的情况");
                return -1;
            }

            DomUtil.SetAttr(nodeServer, "rights", "children_database:create,list");
             */

            strXml = UserRecDom.OuterXml;

            return 0;
        }

        void channels_AskAccountInfo(object sender, AskAccountInfoEventArgs e)
        {
            e.Owner = this;

            LoginDlg dlg = new LoginDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.textBox_serverAddr.Text = this.textBox_kernelUrl.Text;
            dlg.textBox_serverAddr.ReadOnly = true;
            dlg.textBox_comment.Text = e.Comment;
            dlg.textBox_userName.Text = this.ManagerUserName;
            dlg.textBox_password.Text = this.ManagerPassword;
            dlg.checkBox_savePassword.Checked = this.SavePassword;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
            {
                e.Result = 0;
                return;
            }

            this.ManagerPassword = dlg.textBox_userName.Text;

            if (dlg.checkBox_savePassword.Checked == true)
                this.ManagerPassword = dlg.textBox_password.Text;
            else
                this.ManagerPassword = "";

            e.UserName = dlg.textBox_userName.Text;
            e.Password = dlg.textBox_password.Text;

            e.Result = 1;
        }

        int BuildUserRecord(out string strXml,
    out string strError)
        {
            strXml = "";
            strError = "";

            XmlDocument UserRecDom = new XmlDocument();
            UserRecDom.LoadXml("<record><name /><password /><server /></record>");


            // 设置用户名
            DomUtil.SetElementText(UserRecDom.DocumentElement,
                "name",
                this.textBox_manageUserName.Text);


            // 密码
            DomUtil.SetElementText(UserRecDom.DocumentElement,
               "password",
                Cryptography.GetSHA1(this.textBox_managePassword.Text));

            XmlNode nodeServer = UserRecDom.DocumentElement.SelectSingleNode("server");
            if (nodeServer == null)
            {
                Debug.Assert(false, "不可能的情况");
                return -1;
            }

            DomUtil.SetAttr(nodeServer, "rights", "children_database:create,list");

            strXml = UserRecDom.OuterXml;

            return 0;
        }

        void EnableControls(bool bEnable)
        {
            this.textBox_confirmManagePassword.Enabled = bEnable;
            this.textBox_managePassword.Enabled = bEnable;
            this.textBox_manageUserName.Enabled = bEnable;
            this.textBox_kernelUrl.Enabled = bEnable;

            this.button_OK.Enabled = bEnable;
            this.button_Cancel.Enabled = bEnable;
            this.button_createManageUser.Enabled = bEnable;
            this.button_detectManageUser.Enabled = bEnable;
            this.button_resetManageUserPassword.Enabled = bEnable;

        }

        // 检测管理帐户是否存在，登录是否正确
        private void button_detectManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);
            try
            {
                // Debug.Assert(false, "");

                string strError = "";
                // 检测管理用户是否已经存在?
                // return:
                //       -1  出错
                //      0   不存在
                //      1   存在, 且密码一致
                //      2   存在, 但密码不一致
                int nRet = DetectManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, strError);
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

        // 创建一个新的管理帐户。需要用 root 权限登录才能创建
        private void button_createManageUser_Click(object sender, EventArgs e)
        {
            EnableControls(false);

            try
            {
                string strError = "";
                int nRet = CreateManageUser(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, "代理帐户创建成功。");
                }
            }
            finally
            {
                EnableControls(true);
            }

        }

        private void button_resetManageUserPassword_Click(object sender, EventArgs e)
        {
            // 重设置代理帐户密码
            EnableControls(false);
            try
            {
                string strError = "";
                int nRet = ResetManageUserPassword(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                }
                else
                {
                    MessageBox.Show(this, "重设代理帐户密码成功。");
                }
            }
            finally
            {
                EnableControls(true);
            }
        }

    }
}