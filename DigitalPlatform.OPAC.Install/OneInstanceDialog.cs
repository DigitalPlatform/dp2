using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.Install;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;

namespace DigitalPlatform.OPAC
{
    public partial class OneInstanceDialog : Form
    {
        FloatingMessageForm _floatingMessage = null;

        public bool CreateMode = false;   // 是否为新创建实体
        public DigitalPlatform.OPAC.LineInfo LineInfo = null;

        // parameters:
        //      strInstanceName 站点名和虚拟目录名的组合。例如 Default Web Site/dp2OPAC
        public event VerifyEventHandler VerifyInstanceName = null;
        public event VerifyEventHandler VerifyDataDir = null;

        public event LoadXmlFileInfoEventHandler LoadXmlFileInfo = null;    // 临时获取特定的数据目录内的相关信息

        // const int WM_CHECK_DATADIR = API.WM_USER + 201;

        public string LoadedDataDir = "";  // 已经特别装载过的数据目录。防止Leave时重复装载同一个目录
        bool m_bDataDirExist = false;   // 数据目录是否已经存在


        public OneInstanceDialog()
        {
            InitializeComponent();
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

        public string DataDir
        {
            get
            {
                return this.textBox_dataDir.Text;
            }
            set
            {
                this.textBox_dataDir.Text = value;
            }
        }

        public string SiteName
        {
            get
            {
                return this.comboBox_site.Text;
            }
            set
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    if (this.comboBox_site.Items.IndexOf(value) == -1)
                        this.comboBox_site.Items.Add(value);
                }
                this.comboBox_site.Text = value;
            }
        }

        private void OneInstanceDialog_Load(object sender, EventArgs e)
        {
            if (this.CreateMode == false)
            {
                this.textBox_instanceName.ReadOnly = true;
                this.comboBox_site.Enabled = false;
            }

            FillSiteList();

            RefreshDp2LibraryInfo();

            // 设置缺省的虚拟目录名
            if (CreateMode == true && String.IsNullOrEmpty(this.textBox_instanceName.Text) == true)
            {
                SetDefaultVirtualDirValue();
            }

            // 设置缺省的数据目录路径
            if (CreateMode == true && String.IsNullOrEmpty(this.textBox_dataDir.Text) == true)
            {
                SetDefaultDataDirValue();
            }

            if (CreateMode == false)
            {
                if (String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
                    && Directory.Exists(this.textBox_dataDir.Text) == true)
                    this.LoadedDataDir = this.textBox_dataDir.Text;
            }

            // API.PostMessage(this.Handle, WM_CHECK_DATADIR, 0, 0);
            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.Show(this);
            }

        }

#if NO
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CHECK_DATADIR:
                    textBox_dataDir_Leave(this, null);  // 促使检查数据目录是否碰巧为已经存在的目录
                    return;
            }
            base.DefWndProc(ref m);
        }
#endif

        void FillSiteList()
        {
            string strError = "";

            // this.comboBox_site.Items.Clear();

            List<string> results = null;
            // 用 appcmd 方式获得 sites 信息
            int nRet = OpacAppInfo.GetSitesByAppCmd(out results,
            out strError);
            if (nRet == -1)
            {
                this.MessageBoxShow(strError);
                return;
            }

            foreach (string name in results)
            {
                if (this.comboBox_site.Items.IndexOf(name) == -1)
                    this.comboBox_site.Items.Add(name);
            }

            if (this.CreateMode == true
                && results.Count > 0
                && string.IsNullOrEmpty(this.comboBox_site.Text) == true)
                this.comboBox_site.Text = results[0];
        }

        bool IsMouseOnCancelButton()
        {
            return GuiUtil.PtInRect(Control.MousePosition.X,
                Control.MousePosition.Y,
                this.RectangleToScreen(this.button_Cancel.Bounds));
        }

        // 数据目录需要更名的情况，是否延迟到 OK 时候执行？如果立即执行，则需要禁止 Cancel 按钮，表示必须 OK 结束对话框
        // 检测目录是否已经存在
        // return:
        //      -1  出错
        //      0   没有处理
        //      1   已经处理
        int RenameDataDir(out string strError)
        {
            strError = "";

#if NO
            // 已经准备Cancel，就不检查了
            if (IsMouseOnCancelButton() == true)
                return;
#endif

            // 新建时
            if (CreateMode == true
                && String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
                && this.LoadedDataDir != this.textBox_dataDir.Text)
            {
                // 注意检查，不能是别的instance的数据目录
                if (this.VerifyDataDir != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = this.textBox_dataDir.Text;
                    this.VerifyDataDir(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        strError = e1.ErrorInfo;
                        return -1;
                    }
                }

                string strXmlFilename = PathUtil.MergePath(this.textBox_dataDir.Text, "opac.xml");
                if (File.Exists(strXmlFilename) == true)
                {
                    DialogResult result = MessageBox.Show(this,
"您指定的数据目录 '" + this.textBox_dataDir.Text + "' 中已经存在 opac.xml 文件。\r\n\r\n是否要直接利用其中的配置信息?",
"安装 dp2OPAC",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复询问
                        return 0;
                    }

                    if (this.LoadXmlFileInfo != null)
                    {
                        LoadXmlFileInfoEventArgs e1 = new LoadXmlFileInfoEventArgs();
                        e1.DataDir = this.textBox_dataDir.Text;
                        this.LoadXmlFileInfo(this, e1);
                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            strError = e1.ErrorInfo;
                            return -1;
                        }

                        // refresh
                        this.LineInfo = (LineInfo)e1.LineInfo;
                        //Refreshdp2KernelDef();
                        //RefreshSupervisorUserInfo();
                        //RefreshLibraryName();
                        //RefreshUpdateCfgsDir();
                        RefreshDp2LibraryInfo();

                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                        this.m_bDataDirExist = true;    // 防止OK时不合适的检查警告
                    }
                }

                return 0;
            }

            // 修改时
            if (CreateMode == false
    && String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
    && this.LoadedDataDir != this.textBox_dataDir.Text)
            {
                // 注意检查，不能是别的instance的数据目录
                if (this.VerifyDataDir != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = this.textBox_dataDir.Text;
                    this.VerifyDataDir(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                    {
                        strError = e1.ErrorInfo;
                        return -1;
                    }
                }

                string strXmlFilename = PathUtil.MergePath(this.textBox_dataDir.Text, "opac.xml");
                if (File.Exists(strXmlFilename) == true)
                {
                    DialogResult result = MessageBox.Show(this,
"您指定的数据目录 '" + this.textBox_dataDir.Text + "' 中已经存在 opac.xml 文件。\r\n\r\n是否要直接利用其中的配置信息？\r\n\r\n是：直接利用其中的信息，也即将其中的配置信息装入当前对话框\r\n否：利用这个数据目录，但其中xml文件的相关信息即将被当前对话框中的值覆盖\r\n\r\n(提示：无论您选“是”“否”，原有目录 '" + this.LoadedDataDir + "' 都会被闲置)",
"安装 dp2OPAC",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复询问
                        return 0;
                    }

                    if (this.LoadXmlFileInfo != null)
                    {
                        LoadXmlFileInfoEventArgs e1 = new LoadXmlFileInfoEventArgs();
                        e1.DataDir = this.textBox_dataDir.Text;
                        this.LoadXmlFileInfo(this, e1);
                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            strError = e1.ErrorInfo;
                            return -1;
                        }

                        // refresh
                        this.LineInfo = (LineInfo)e1.LineInfo;
                        //Refreshdp2KernelDef();
                        //RefreshSupervisorUserInfo();
                        //RefreshLibraryName();
                        //RefreshUpdateCfgsDir();
                        RefreshDp2LibraryInfo();

                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                        this.m_bDataDirExist = true;    // 防止OK时不合适的检查警告
                    }
                }
                else if (String.IsNullOrEmpty(this.LoadedDataDir) == false)
                {
                    // 修改目录名

                    DialogResult result = MessageBox.Show(this,
"要将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '" + this.textBox_dataDir.Text + "' 么?\r\n\r\n(如果选择“否”，则安装程序在稍后将新创建一个数据目录，并复制进初始内容)",
"安装 dp2OPAC",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.m_bDataDirExist = false;
                        return 0;
                    }

                REDO_MOVE:
                    strError = "";
                    try
                    {
                        // TODO: 需要测试当数据目录中内容尺寸太大，而目标盘无妨容纳时的报错情况
                        Directory.Move(this.LoadedDataDir, this.textBox_dataDir.Text);

                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(this, "将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '" + this.textBox_dataDir.Text + "' 时发生错误: " + ex.Message);
                        strError = "将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '" + this.textBox_dataDir.Text + "' 时发生错误: " + ex.Message;
                    }

                    if (string.IsNullOrEmpty(strError) == false)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
strError + "\r\n\r\n建议先停止 IIS，然后重试操作。\r\n\r\n是否重试?",
"安装 dp2OPAC",
MessageBoxButtons.RetryCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_MOVE;
                        return -1;
                    }
                }

                return 0;
            }

            return 0;
        }

#if NO
        public void SetDefaultDataDirValue()
        {
            // TODO: 要加上站点名因素
            if (this.comboBox_site.Text == "Default Web Site"
                && this.textBox_instanceName.Text == "/dp2OPAC")
            { 
                this.textBox_dataDir.Text = "c:\\opac_data";
                return;
            }

            this.textBox_dataDir.Text = "c:\\opac_data"
    + (String.IsNullOrEmpty(this.textBox_instanceName.Text.Replace("/", "")) == false ? "_" : "")
    + this.textBox_instanceName.Text.Replace("/", "");
        }
#endif

        void SetDefaultDataDirValue()
        {
            for (int i = 0; ; i++)
            {
                string strDateDir = "c:\\opac_data";
                if (i > 0)
                    strDateDir = "c:\\opac_data_" + (i + 1).ToString();

                // 已经存在的物理目录不能使用
                if (Directory.Exists(strDateDir) == true)
                    continue;

                // 注意检查，不能是别的instance的数据目录
                if (this.VerifyDataDir != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = strDateDir;
                    this.VerifyDataDir(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                        continue;
                }

                this.textBox_dataDir.Text = strDateDir;
                return;
            }
        }

        void SetDefaultVirtualDirValue()
        {
            string strError = "";
            List<OpacAppInfo> infos = null;
            // 用 appcmd 方式获得 所有虚拟目录的信息 (不仅仅是 dp2OPAC 虚拟目录)
            int nRet = OpacAppInfo.GetAllVirtualInfoByAppCmd(out infos,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            for (int i = 0; ; i++)
            {
                string strVirtualDir = "/dp2OPAC";
                if (i > 0)
                    strVirtualDir = "/dp2OPAC" + (i + 1).ToString();

                // 已经存在的虚拟目录不能使用

                // 查找一个虚拟目录是否存在
                // return:
                //      -1  不存在
                //      其他  数组元素的下标
                nRet = OpacAppInfo.Find(infos,
                    this.comboBox_site.Text,
                    strVirtualDir);
                if (nRet != -1)
                    continue;

                // 注意检查，不能是别的instance的数据目录
                if (this.VerifyDataDir != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = strVirtualDir;
                    this.VerifyDataDir(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                        continue;
                }

                if (this.VerifyInstanceName != null)
                {
                    VerifyEventArgs e1 = new VerifyEventArgs();
                    e1.Value = this.comboBox_site.Text + strVirtualDir;
                    this.VerifyInstanceName(this, e1);
                    if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                        continue;
                }

                this.textBox_instanceName.Text = strVirtualDir;
                return;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void RefreshDp2LibraryInfo()
        {
            if (this.LineInfo == null)
            {
                this.textBox_dp2LibraryDef.Text = "";
                return;
            }
            if (this.LineInfo.LibraryUrl != null)
                this.textBox_dp2LibraryDef.Text = "dp2LibrayURL = " + this.LineInfo.LibraryUrl;
            else
                this.textBox_dp2LibraryDef.Text = "dp2LibrayURL = <不修改>";

            if (this.LineInfo.LibraryUserName != null)
                this.textBox_dp2LibraryDef.Text += ";\r\nUserName = " + this.LineInfo.LibraryUserName;
            else
                this.textBox_dp2LibraryDef.Text += ";\r\nUserName = <不修改>";

            if (this.LineInfo.LibraryPassword != null)
                this.textBox_dp2LibraryDef.Text += ";\r\nPassword = " + new string('*', this.LineInfo.LibraryPassword.Length);
            else
                this.textBox_dp2LibraryDef.Text += ";\r\nPassword = <不修改>";

            if (this.LineInfo.LibraryReportDir != null)
                this.textBox_dp2LibraryDef.Text += ";\r\nLibraryReportDir = " + this.LineInfo.LibraryReportDir;
            else
                this.textBox_dp2LibraryDef.Text += ";\r\nLibraryReportDir = <不修改>";

        }


        private void button_editDp2LibraryDef_Click(object sender, EventArgs e)
        {
            InstallOpacParamDlg param_dlg = new InstallOpacParamDlg();
            GuiUtil.AutoSetDefaultFont(param_dlg);

            if (this.LineInfo == null
    ||
    (this.LineInfo.LibraryUrl == null
    && this.LineInfo.LibraryUserName == null
    && this.LineInfo.LibraryPassword == null)
    )
            {
                param_dlg.ManageUserName = "opac";
                param_dlg.ManagePassword = "";
                // dlg.Rights = "this:management;children_database:management;children_directory:management;children_leaf:management;descendant_directory:management;descendant_record:management;descendant_leaf:management";
            }
            else
            {
                Debug.Assert(this.LineInfo != null, "");
                param_dlg.Dp2LibraryUrl = this.LineInfo.LibraryUrl;
                param_dlg.ManageUserName = this.LineInfo.LibraryUserName;
                param_dlg.ManagePassword = this.LineInfo.LibraryPassword;
                param_dlg.LibraryReportDir = this.LineInfo.LibraryReportDir;
            }

            param_dlg.StartPosition = FormStartPosition.CenterScreen;
            param_dlg.ManageUserName = "opac";
            param_dlg.ShowDialog(this);
            if (param_dlg.DialogResult != System.Windows.Forms.DialogResult.OK)
                return;

            if (this.LineInfo == null)
            {
                this.LineInfo = new LineInfo();
            }

            this.LineInfo.LibraryUrl = param_dlg.Dp2LibraryUrl;
            this.LineInfo.LibraryUserName = param_dlg.ManageUserName;
            this.LineInfo.LibraryPassword = param_dlg.ManagePassword;
            this.LineInfo.LibraryReportDir = param_dlg.LibraryReportDir;

            RefreshDp2LibraryInfo();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 检查

            if (String.IsNullOrEmpty(this.comboBox_site.Text) == true)
            {
                strError = "尚未指定站点名";
                goto ERROR1;
            }

            if (String.IsNullOrEmpty(this.textBox_instanceName.Text) == true)
            {
                strError = "尚未指定虚拟目录名";
                goto ERROR1;
            }

            if (this.textBox_instanceName.Text[0] != '/')
            {
                strError = "虚拟目录名第一字符必须为 '/'";
                goto ERROR1;
            }

            string strVirtualDirRight = this.textBox_instanceName.Text.Substring(1);
            if (strVirtualDirRight.IndexOfAny(new char[] { '/', ' ' }) != -1)
            {
                strError = "虚拟目录名第一字符以外的字符中，不允许包含 '/' 或空格字符";
                goto ERROR1;
            }

            // 数据目录
            if (String.IsNullOrEmpty(this.textBox_dataDir.Text) == true)
            {
                strError = "尚未指定数据目录";
                goto ERROR1;
            }

            // dp2Library 服务器信息
            if (String.IsNullOrEmpty(this.textBox_dp2LibraryDef.Text) == true)
            {
                strError = "尚未指定 dp2Library 服务器信息";
                goto ERROR1;
            }

            if (this.LineInfo == null)
                this.LineInfo = new DigitalPlatform.OPAC.LineInfo();

            // 检查是否为旧式url地址
            string strUrl = this.LineInfo.LibraryUrl;
            if (strUrl.IndexOf(".asmx") != -1)
            {
                strError = "安装程序发现当前使用了旧版本 dp2LibraryWs 的地址 '" + strUrl + "'，需要您将它修改为新版 dp2Library (图书馆应用服务器) 的 URL 地址。";
                goto ERROR1;
            }

            if (this.CreateMode == true
                && this.m_bDataDirExist == false)
            {
                // opac 用户信息
                if (this.LineInfo.LibraryUserName == null
        || this.LineInfo.LibraryPassword == null)
                {
                    strError = "尚未设定 opac 账户的用户名、密码";
                    goto ERROR1;
                }
            }

            // 如果修改时，需要创建新的数据目录
            if (this.CreateMode == false)
            {
                // 探测数据目录，是否已经存在数据，是不是属于升级情形
                // return:
                //      -1  error
                //      0   数据目录不存在
                //      1   数据目录存在，但是xml文件不存在
                //      2   xml文件已经存在
                nRet = InstanceDialog.DetectDataDir(this.textBox_dataDir.Text,
            out strError);
                if (nRet == -1)
                {
                    strError = "探测数据目录 '" + this.textBox_dataDir.Text + "' 是否存在时，出现错误: " + strError;
                    goto ERROR1;
                }

                if (nRet == 0 || nRet == 1)
                {
                    // opac 用户信息
                    if (this.LineInfo.LibraryUserName == null
            || this.LineInfo.LibraryPassword == null)
                    {
                        strError = "尚未设定 opac 账户的用户名、密码";
                        goto ERROR1;
                    }
                }
            }

            // 处理数据目录更名情况
            nRet = RenameDataDir(out strError);
            if (nRet == -1)
                goto ERROR1;

            // TODO: 如果在编辑状态，需要排除和 listview 中自己重的情况

            if (this.VerifyInstanceName != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.comboBox_site.Text + this.textBox_instanceName.Text;
                this.VerifyInstanceName(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = e1.ErrorInfo;
                    goto ERROR1;
                }
            }

            if (this.VerifyDataDir != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.textBox_dataDir.Text;
                this.VerifyDataDir(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = e1.ErrorInfo;
                    goto ERROR1;
                }
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void comboBox_site_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.CreateMode == true)
            {
                SetDefaultVirtualDirValue();
            }
        }

        private void OneInstanceDialog_Move(object sender, EventArgs e)
        {
            if (this._floatingMessage != null)
                this._floatingMessage.OnResizeOrMove();
        }

        private void OneInstanceDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_floatingMessage != null)
                _floatingMessage.Close();
        }
    }
}
