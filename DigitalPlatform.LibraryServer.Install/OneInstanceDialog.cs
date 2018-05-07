using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections;

using DigitalPlatform.Install;
using DigitalPlatform.GUI;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

namespace DigitalPlatform.LibraryServer
{
    public partial class OneInstanceDialog : Form
    {
        public bool IsNew = false;   // 是否为新创建实体
        public DigitalPlatform.LibraryServer.LineInfo LineInfo = null;

        public event VerifyEventHandler VerifyInstanceName = null;
        public event VerifyEventHandler VerifyDataDir = null;
        public event VerifyEventHandler VerifyBindings = null;

        public event LoadXmlFileInfoEventHandler LoadXmlFileInfo = null;    // 临时获取特定的数据目录内的相关信息

        public string LoadedDataDir = "";  // 已经特别装载过的数据目录。防止Leave时重复装载同一个目录
        bool m_bDataDirExist = false;   // 数据目录是否已经存在


        bool m_bDataDirKeyEdited = false;    // 数据目录textbox是否被键盘修改过

        private MessageBalloon m_firstUseBalloon = null;

        const int WM_CHECK_DATADIR = API.WM_USER + 201;

        public OneInstanceDialog()
        {
            InitializeComponent();
        }

        private void OneInstanceDialog_Load(object sender, EventArgs e)
        {
            // Debug.Assert(false, "");

            Refreshdp2KernelDef();
            RefreshSupervisorUserInfo();
            RefreshLibraryName();
            RefreshUpdateCfgsDir();

            if (IsNew == true && String.IsNullOrEmpty(this.textBox_dataDir.Text) == true)
            {
                SetDefaultDataDirValue();   // 让数据目录textbox有缺省值
            }

            if (IsNew == false)
            {
                if (String.IsNullOrEmpty(this.textBox_dataDir.Text) == false
                    && Directory.Exists(this.textBox_dataDir.Text) == true)
                    this.LoadedDataDir = this.textBox_dataDir.Text;
            }

            if (IsNew && String.IsNullOrEmpty(this.textBox_instanceName.Text) == true)
            {
                ShowMessageTip();
            }

            API.PostMessage(this.Handle, WM_CHECK_DATADIR, 0, 0);
        }

        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_CHECK_DATADIR:
                    HideMessageTip();
                    textBox_dataDir_Leave(this, null);  // 促使检查数据目录是否碰巧为已经存在的目录

                    if (IsNew && String.IsNullOrEmpty(this.textBox_instanceName.Text) == true)
                    {
                        ShowMessageTip();
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        void ShowMessageTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.textBox_instanceName;
            m_firstUseBalloon.Title = "创建第一个实例";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "对于第一个实例(或者唯一的实例)来说，可以让实例名保持为空，这样，缺省的数据目录名和协议路径都可以简短一些";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();
        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 检查

            // 数据目录
            if (String.IsNullOrEmpty(this.textBox_dataDir.Text) == true)
            {
                strError = "尚未指定数据目录";
                goto ERROR1;
            }

            // dp2Kernel服务器信息
            if (String.IsNullOrEmpty(this.textBox_dp2KernelDef.Text) == true)
            {
                strError = "尚未指定dp2Kernel服务器信息";
                goto ERROR1;
            }

            // 协议绑定
            if (String.IsNullOrEmpty(this.textBox_bindings.Text) == true)
            {
                strError = "尚未指定协议绑定信息";
                goto ERROR1;
            }

            // 图书馆名
            if (String.IsNullOrEmpty(this.textBox_libraryName.Text) == true)
            {
                strError = "尚未指定图书馆名";
                goto ERROR1;
            }
            if (this.LineInfo == null)
                this.LineInfo = new DigitalPlatform.LibraryServer.LineInfo();
            this.LineInfo.LibraryName = this.textBox_libraryName.Text;

            this.LineInfo.UpdateCfgsDir = this.checkBox_updateCfgsDir.Checked;

            // 检查是否为旧式url地址
            string strKernelUrl = this.LineInfo.KernelUrl;
            if (strKernelUrl.IndexOf(".asmx") != -1)
            {
                strError = "安装程序发现当前使用了旧版本数据库内核的地址 '" + strKernelUrl + "'，需要您将它修改为新版dp2Kernel(内核)的URL地址。";
                goto ERROR1;
            }

            if (this.IsNew == true
                && this.m_bDataDirExist == false)
            {
                // supervisor用户信息
                if (this.LineInfo.SupervisorUserName == null
        || this.LineInfo.SupervisorPassword == null
        || this.LineInfo.SupervisorRights == null)
                {
                    strError = "尚未设定supervisor账户的用户名、密码、权限";
                    goto ERROR1;
                }
            }

            // 如果修改时，需要创建新的数据目录
            if (this.IsNew == false)
            {
                // 探测数据目录，是否已经存在数据，是不是属于升级情形
                // return:
                //      -1  error
                //      0   数据目录不存在
                //      1   数据目录存在，但是xml文件不存在
                //      2   xml文件已经存在
                int nRet = LibraryInstallHelper.DetectDataDir(this.textBox_dataDir.Text,
            out strError);
                if (nRet == -1)
                {
                    strError = "探测数据目录 '" + this.textBox_dataDir.Text + "' 是否存在时，出现错误: " + strError;
                    goto ERROR1;
                }

                if (nRet == 0 || nRet == 1)
                {
                    // supervisor用户信息
                    if (this.LineInfo.SupervisorUserName == null
            || this.LineInfo.SupervisorPassword == null
            || this.LineInfo.SupervisorRights == null)
                    {
                        strError = "尚未设定supervisor账户的用户名、密码、权限";
                        goto ERROR1;
                    }
                }
            }

            // TODO: 如果在编辑状态，需要排除和listview中自己重的情况

            if (this.VerifyInstanceName != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.textBox_instanceName.Text;
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

            if (this.VerifyBindings != null)
            {
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = this.textBox_bindings.Text;
                this.VerifyBindings(this, e1);
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

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // 要把修改还原
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button_editdp2KernelDef_Click(object sender, EventArgs e)
        {
            // string strError = "";

            InstallLibraryParamDlg param_dlg = new InstallLibraryParamDlg();
            GuiUtil.AutoSetDefaultFont(param_dlg);
            param_dlg.StartPosition = FormStartPosition.CenterScreen;
            if (this.LineInfo != null)
            {
                param_dlg.KernelUrl = this.LineInfo.KernelUrl;
                param_dlg.ManageUserName = this.LineInfo.KernelUserName;
                param_dlg.ManagePassword = this.LineInfo.KernelPassword;
            }
            else
            {
                // param_dlg.KernelUrl = this.LineInfo.KernelUrl;
                param_dlg.ManageUserName = "root";  // 不能叫 supervisor ，那样容易和dpLibrary层的supervisor用户名混淆
                // param_dlg.ManagePassword = this.LineInfo.KernelPassword;
            }

            param_dlg.ShowDialog(this);
            if (param_dlg.DialogResult != DialogResult.OK)
                return;

            if (this.LineInfo == null)
                this.LineInfo = new LineInfo();

            this.LineInfo.KernelUrl = param_dlg.KernelUrl;
            this.LineInfo.KernelUserName = param_dlg.ManageUserName;
            this.LineInfo.KernelPassword = param_dlg.ManagePassword;
            Refreshdp2KernelDef();
            return;
            /*
        ERROR1:
            MessageBox.Show(this, strError);
            return;
             * */
        }

        private void button_editSupervisorUserInfo_Click(object sender, EventArgs e)
        {
            CreateSupervisorDlg dlg = new CreateSupervisorDlg();
            GuiUtil.AutoSetDefaultFont(dlg);

            if (this.LineInfo == null
                ||
                (this.LineInfo.SupervisorUserName == null
                && this.LineInfo.SupervisorPassword == null
                && this.LineInfo.SupervisorRights == null)
                )
            {
                dlg.UserName = "supervisor";
                dlg.Password = "";
                // dlg.Rights = "this:management;children_database:management;children_directory:management;children_leaf:management;descendant_directory:management;descendant_record:management;descendant_leaf:management";
            }
            else
            {
                Debug.Assert(this.LineInfo != null, "");
                dlg.UserName = this.LineInfo.SupervisorUserName;
                dlg.Password = this.LineInfo.SupervisorPassword;
                dlg.Rights = this.LineInfo.SupervisorRights;
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (this.LineInfo == null)
            {
                this.LineInfo = new LineInfo();
            }

            this.LineInfo.SupervisorUserName = dlg.UserName;
            this.LineInfo.SupervisorPassword = dlg.Password;
            this.LineInfo.SupervisorRights = dlg.Rights;

            RefreshSupervisorUserInfo();
        }

        void Refreshdp2KernelDef()
        {
            if (this.LineInfo == null)
            {
                this.textBox_dp2KernelDef.Text = "";
                return;
            }

            this.textBox_dp2KernelDef.Text = "dp2Kernel URL = " + this.LineInfo.KernelUrl
                + "; UserName = " + this.LineInfo.KernelUserName
                + "; Password = " + new string('*', this.LineInfo.KernelPassword.Length);
        }

        void RefreshSupervisorUserInfo()
        {
            if (this.LineInfo == null)
            {
                this.textBox_supervisorUserInfo.Text = "";
                return;
            }
            if (this.LineInfo.SupervisorUserName != null)
                this.textBox_supervisorUserInfo.Text = "UserName = " + this.LineInfo.SupervisorUserName;
            else
                this.textBox_supervisorUserInfo.Text = "UserName = <不修改>";

            if (this.LineInfo.SupervisorPassword != null)
                this.textBox_supervisorUserInfo.Text += "; Password = " + new string('*', this.LineInfo.SupervisorPassword.Length);
            else
                this.textBox_supervisorUserInfo.Text += "; Password = <不修改>";

            if (this.LineInfo.SupervisorRights != null)
                this.textBox_supervisorUserInfo.Text += "; Rights = " + this.LineInfo.SupervisorRights;
            else
                this.textBox_supervisorUserInfo.Text += "; Rights = <不修改>";
        }

        void RefreshLibraryName()
        {
            if (this.LineInfo != null)
                this.textBox_libraryName.Text = this.LineInfo.LibraryName;
            else
                this.textBox_libraryName.Text = "";
        }

        void RefreshUpdateCfgsDir()
        {
            if (this.LineInfo != null)
                this.checkBox_updateCfgsDir.Checked = this.LineInfo.UpdateCfgsDir;
            else
                this.checkBox_updateCfgsDir.Checked = false;
        }

        // 检查绑定内容是否适合利用对话框进行编辑
        // return:
        //      -1  出错
        //      0   适合编辑
        //      1   不适合编辑
        int CheckBindingsEditable(out string strError)
        {
            strError = "";
            if (string.IsNullOrEmpty(this.textBox_bindings.Text) == true)
                return 0;
            string[] bindings = this.textBox_bindings.Text.Replace("\r\n", ";").Split(new char[] { ';' });

            int nTcpCount = 0;
            int nHttpCount = 0;
            int nPipeCount = 0;
            for (int i = 0; i < bindings.Length; i++)
            {
                string strOneBinding = bindings[i].Trim();
                if (String.IsNullOrEmpty(strOneBinding) == true)
                    continue;

                Uri one_uri = new Uri(strOneBinding);
                if (one_uri.Scheme.ToLower() == "net.tcp")
                {
                    nTcpCount++;
                }
                else if (one_uri.Scheme.ToLower() == "net.pipe")
                {
                    nPipeCount++;
                }
                else if (one_uri.Scheme.ToLower() == "http")
                {
                    nHttpCount++;
                }
            }

            if (nTcpCount > 1)
            {
                strError = "net.tcp 协议绑定数超过一个";
                return 1;
            }
            if (nPipeCount > 1)
            {
                strError = "net.pipe 协议绑定数超过一个";
                return 1;
            }
            if (nHttpCount > 1)
            {
                strError = "http 协议绑定数超过一个";
                return 1;
            }
            return 0;
        }

        // 准备可选的缺省绑定内容
        int PrepareDefaultBindings(string strTail,
            out string[] default_urls,
            out string strError)
        {
            default_urls = null;
            strError = "";

            // 无法检查
            if (this.VerifyBindings == null)
            {
                default_urls = new string[] {
                    "net.tcp://localhost:8002/dp2library/" + strTail,
                    "net.pipe://localhost/dp2library/" + strTail,
                    "http://localhost:8001/dp2library/" + strTail,
                    "rest.http://localhost:8001/dp2library/" + strTail,
                    "basic.http://localhost:8001/dp2library/" + strTail
                };
                return 0;
            }

            string strTcpUrl = "";
            for (int nPort = 8002; ; nPort++)
            {
                strTcpUrl = "net.tcp://localhost:" + nPort.ToString() + "/dp2library/" + strTail;
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strTcpUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            string strPipeUrl = "";
            for (int nNumber = 0; ; nNumber++)
            {
                strPipeUrl = "net.pipe://localhost/dp2library/" + strTail;
                if (nNumber > 0)
                    strPipeUrl += nNumber.ToString() + "/";

                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strPipeUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            string strHttpUrl = "";
            for (int nPort = 8001; ; nPort++)
            {
                strHttpUrl = "http://localhost:" + nPort.ToString() + "/dp2library/" + strTail;
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strHttpUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            string strRestUrl = "";
            for (int nPort = 8001; ; nPort++)
            {
                strRestUrl = "rest.http://localhost:" + nPort.ToString() + "/dp2library/rest/" + strTail;
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strRestUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            string strBasicUrl = "";
            for (int nPort = 8001; ; nPort++)
            {
                strBasicUrl = "basic.http://localhost:" + nPort.ToString() + "/dp2library/basic/" + strTail;
                VerifyEventArgs e1 = new VerifyEventArgs();
                e1.Value = strBasicUrl;
                this.VerifyBindings(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == true)
                {
                    break;
                }
            }

            default_urls = new string[] {
                    strTcpUrl,
                    strPipeUrl,
                    strHttpUrl,
                    strRestUrl,
                    strBasicUrl,
                };
            return 0;
        }

        private void button_editBindings_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 检查绑定内容是否适合利用对话框进行编辑
            // return:
            //      -1  出错
            //      0   适合编辑
            //      1   不适合编辑
            int nRet = CheckBindingsEditable(out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                strError = "当前绑定内容因为 " + strError + " 而无法被专用对话框进行编辑，但可以直接在文本框中进行修改";
                goto ERROR1;
            }

            // TODO: 要找到一个没有被使用过的tcp端口号
            string strTail = this.InstanceName + (String.IsNullOrEmpty(this.InstanceName) == false ? "/" : "");

            string[] default_urls = null;
            nRet = PrepareDefaultBindings(strTail,
            out default_urls,
            out strError);
            if (nRet == -1)
            {
                strError = "准备缺省班定值时发生错误: " + strError;
                goto ERROR1;
            }
            /*
            string[] default_urls = new string[] {
                    "net.tcp://localhost:8002/dp2kernel/" + strTail,
                    "net.pipe://localhost/dp2kernel/" + strTail,
                    "http://localhost:8001/dp2kernel/" + strTail
                };
             * */

            WcfBindingDlg dlg = new WcfBindingDlg();
            GuiUtil.AutoSetDefaultFont(dlg);
            if (this.IsNew && String.IsNullOrEmpty(this.textBox_bindings.Text) == true)
                dlg.Urls = default_urls;
            else
                dlg.Urls = this.textBox_bindings.Text.Replace("\r\n", ";").Split(new char[] { ';' });
            dlg.DefaultUrls = default_urls;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
            if (dlg.DialogResult != DialogResult.OK)
                return;

            this.textBox_bindings.Text = string.Join("\r\n", dlg.Urls);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
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

        public string Bindings
        {
            get
            {
                return this.textBox_bindings.Text;
            }
            set
            {
                this.textBox_bindings.Text = value;
            }
        }

        public string LibraryName
        {
            get
            {
                return this.textBox_libraryName.Text;
            }
            set
            {
                this.textBox_libraryName.Text = value;
            }
        }

        private void textBox_instanceName_TextChanged(object sender, EventArgs e)
        {
            if (IsNew == true)
            {
                // 数据目录textbox要跟从实体名进行变化
                if (m_bDataDirKeyEdited == false)
                {
                    SetDefaultDataDirValue();
                }
            }
        }

        public void SetDefaultDataDirValue()
        {
            this.textBox_dataDir.Text = "c:\\library_data"
    + (String.IsNullOrEmpty(this.textBox_instanceName.Text) == false ? "_" : "")
    + this.textBox_instanceName.Text;
        }

        private void textBox_dataDir_KeyPress(object sender, KeyPressEventArgs e)
        {
            m_bDataDirKeyEdited = true;
        }

        private void textBox_instanceName_Leave(object sender, EventArgs e)
        {
            HideMessageTip();
        }

        bool IsMouseOnCancelButton()
        {
            return GuiUtil.PtInRect(Control.MousePosition.X,
                Control.MousePosition.Y,
                this.RectangleToScreen(this.button_Cancel.Bounds));
        }

        // 检测目录是否已经存在
        private void textBox_dataDir_Leave(object sender, EventArgs e)
        {
            // 已经准备Cancel，就不检查了
            if (IsMouseOnCancelButton() == true)
                return;

            // 新建时
            if (IsNew == true
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
                        MessageBox.Show(this, e1.ErrorInfo);
                        return;
                    }
                }

                string strXmlFilename = PathUtil.MergePath(this.textBox_dataDir.Text, "library.xml");
                if (File.Exists(strXmlFilename) == true)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
"您指定的数据目录 '" + this.textBox_dataDir.Text + "' 中已经存在 library.xml 文件。\r\n\r\n是否要直接利用其中的配置信息?",
"安装 dp2Library",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复询问
                        return;
                    }

                    if (this.LoadXmlFileInfo != null)
                    {
                        LoadXmlFileInfoEventArgs e1 = new LoadXmlFileInfoEventArgs();
                        e1.DataDir = this.textBox_dataDir.Text;
                        this.LoadXmlFileInfo(this, e1);
                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            MessageBox.Show(this, e1.ErrorInfo);
                            return;
                        }

                        // refresh
                        this.LineInfo = (LineInfo)e1.LineInfo;
                        Refreshdp2KernelDef();
                        RefreshSupervisorUserInfo();
                        RefreshLibraryName();
                        RefreshUpdateCfgsDir();
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                        this.m_bDataDirExist = true;    // 防止OK时不合适的检查警告
                    }
                }

                return;
            }

            // 修改时
            if (IsNew == false
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
                        MessageBox.Show(this, e1.ErrorInfo);
                        return;
                    }
                }

                string strXmlFilename = PathUtil.MergePath(this.textBox_dataDir.Text, "library.xml");
                if (File.Exists(strXmlFilename) == true)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
"您指定的数据目录 '" + this.textBox_dataDir.Text + "' 中已经存在 library.xml 文件。\r\n\r\n是否要直接利用其中的配置信息？\r\n\r\n是：直接利用其中的信息，也即将其中的配置信息装入当前对话框\r\n否：利用这个数据目录，但其中xml文件的相关信息即将被当前对话框中的值覆盖\r\n\r\n(提示：无论您选“是”“否”，原有目录 '" + this.LoadedDataDir + "' 都会被闲置)",
"安装 dp2Library",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复询问
                        return;
                    }

                    if (this.LoadXmlFileInfo != null)
                    {
                        LoadXmlFileInfoEventArgs e1 = new LoadXmlFileInfoEventArgs();
                        e1.DataDir = this.textBox_dataDir.Text;
                        this.LoadXmlFileInfo(this, e1);
                        if (string.IsNullOrEmpty(e1.ErrorInfo) == false)
                        {
                            MessageBox.Show(this, e1.ErrorInfo);
                            return;
                        }

                        // refresh
                        this.LineInfo = (LineInfo)e1.LineInfo;
                        Refreshdp2KernelDef();
                        RefreshSupervisorUserInfo();
                        RefreshLibraryName();
                        RefreshUpdateCfgsDir();
                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                        this.m_bDataDirExist = true;    // 防止OK时不合适的检查警告
                    }
                }
                else if (String.IsNullOrEmpty(this.LoadedDataDir) == false)
                {
                    // 修改目录名

                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
"要将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '" + this.textBox_dataDir.Text + "' 么?\r\n\r\n(如果选择“否”，则安装程序在稍后将新创建一个数据目录，并复制进初始内容)",
"安装 dp2Library",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button1);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        this.m_bDataDirExist = false;
                        return;
                    }

                    try
                    {
                        // TODO: 需要测试当数据目录中内容尺寸太大，而目标盘无妨容纳时的报错情况
                        Directory.Move(this.LoadedDataDir, this.textBox_dataDir.Text);

                        this.LoadedDataDir = this.textBox_dataDir.Text; // 防止重复装载
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "将已经存在的数据目录 '" + this.LoadedDataDir + "' 更名为 '" + this.textBox_dataDir.Text + "' 时发生错误: " + ex.Message);
                    }

                }

                return;
            }

        }

        // 将本地字符串匹配序列号
        public static bool MatchLocalString(string strSerialNumber, string strInstanceName)
        {
            List<string> macs = SerialCodeForm.GetMacAddress();
            foreach (string mac in macs)
            {
                string strLocalString = OneInstanceDialog.GetEnvironmentString(mac,
                    strSerialNumber,
                    strInstanceName);
                string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                    return true;
            }

            // 2014/12/19
            if (DateTime.Now.Month == 12)
            {
                foreach (string mac in macs)
                {
                    string strLocalString = OneInstanceDialog.GetEnvironmentString(mac,
                        strSerialNumber,
                        strInstanceName,
                        true);
                    string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
                    if (strSha1 == SerialCodeForm.GetCheckCode(strSerialNumber))
                        return true;
                }
            }
            return false;
        }

        // 出现对话框重新设置序列号
        // return:
        //      0   Cancel
        //      1   OK
        public static int ResetSerialCode(
            IWin32Window owner,
            string strTitle,
            bool bAllowSetBlank,
            ref string strSerialCode,
            string strOriginCode)
        {
            Hashtable ext_table = StringUtil.ParseParameters(strOriginCode);
            string strMAC = (string)ext_table["mac"];
            if (string.IsNullOrEmpty(strMAC) == true)
                strOriginCode = "!error";
            else
                strOriginCode = Cryptography.Encrypt(strOriginCode,
                CopyrightKey);
            SerialCodeForm dlg = new SerialCodeForm();

            GuiUtil.AutoSetDefaultFont(dlg);

            if (string.IsNullOrEmpty(strTitle) == false)
                dlg.Text = strTitle;

            // dlg.Font = this.Font;
            dlg.DefaultCodes = new List<string>(new string[] { "community|社区版" });
            dlg.SerialCode = strSerialCode;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.OriginCode = strOriginCode;

        REDO:
            dlg.ShowDialog(owner);
            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            strSerialCode = dlg.SerialCode;

            if (string.IsNullOrEmpty(strSerialCode) == true)
            {
                if (bAllowSetBlank == true)
                {
                    DialogResult result = MessageBox.Show(owner,
        "确实要将序列号设置为空?\r\n\r\n(一旦将序列号设置为空，dp2Library 将按照最多 5 个前端方式运行)",
        "dp2Library",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        return 0;
                    }
                }
                else
                {
                    MessageBox.Show(owner, "序列号不允许为空。请重新设置");
                    goto REDO;
                }
            }

            return 1;
        }

        public static string GetEnvironmentString(string strMAC,
            string strSerialCode,
            string strInstanceName,
            bool bNextYear = false)
        {
            Hashtable table = new Hashtable();
            table["mac"] = strMAC;  //  SerialCodeForm.GetMacAddress();
            if (bNextYear == false)
                table["time"] = SerialCodeForm.GetTimeRange();
            else
                table["time"] = SerialCodeForm.GetNextYearTimeRange();

            table["instance"] = strInstanceName;

            table["product"] = "dp2library";

            // string strSerialCode = this.LineInfo.SerialNumber;
            // 将 strSerialCode 中的扩展参数设定到 table 中
            SerialCodeForm.SetExtParams(ref table, strSerialCode);
            return StringUtil.BuildParameterString(table);
        }

        static string CopyrightKey = "dp2library_sn_key";

        int ConfigMq(bool bAdd,
            out string strError)
        {
            strError = "";

            string strDataDir = this.textBox_dataDir.Text;
            string strInstanceName = this.textBox_instanceName.Text;

            if (string.IsNullOrEmpty(strDataDir))
            {
                strError = "尚未指定数据目录";
                return -1;
            }

            string strLibraryXmlFileName = Path.Combine(strDataDir, "library.xml");
            if (File.Exists(strLibraryXmlFileName) == false)
            {
                strError = "配置文件 '" + strLibraryXmlFileName + "' 不存在，无法进行进一步配置";
                return -1;
            }

            return InstallHelper.SetupMessageQueue(
                strLibraryXmlFileName,
                strInstanceName,
                bAdd,
                out strError);
#if NO
            XmlDocument dom = new XmlDocument();
            dom.Load(strLibraryXmlFileName);

            bool bChanged = false;

            // message 元素 defaultQueue 属性
            {
                XmlElement message = dom.DocumentElement.SelectSingleNode("message") as XmlElement;
                if (message == null)
                {
                    message = dom.CreateElement("message");
                    dom.DocumentElement.AppendChild(message);
                    bChanged = true;
                }

                string strOldValue = message.GetAttribute("defaultQueue");
                if (string.IsNullOrEmpty(strOldValue) == true)
                {
                    string strNewValue = ".\\private$\\dp2library";
                    if (string.IsNullOrEmpty(strInstanceName) == false)
                        strNewValue += "_" + strInstanceName;
                    message.SetAttribute("defaultQueue", strNewValue);
                    bChanged = true;
                }
            }

            // arrived 元素的 notifyTypes 属性
            {
                XmlElement arrived = dom.DocumentElement.SelectSingleNode("arrived") as XmlElement;
                if (arrived == null)
                {
                    arrived = dom.CreateElement("arrived");
                    dom.DocumentElement.AppendChild(arrived);
                    bChanged = true;
                }

                string strOldValue = arrived.GetAttribute("notifyTypes");
                if (string.IsNullOrEmpty(strOldValue) == true)
                {
                    string strNewValue = "dpmail,mail,mq";
                    arrived.SetAttribute("notifyTypes", strNewValue);
                    bChanged = true;
                }
                else
                {
                    // 增加 mq
                    string strNewValue = strOldValue;
                    StringUtil.SetInList(ref strNewValue, "mq", true);
                    if (strNewValue != strOldValue)
                    {
                        arrived.SetAttribute("notifyTypes", strNewValue);
                        bChanged = true;
                    }
                }
            }

            // monitors/readersMonitor 元素的 types 元素
            {
                XmlElement readersMonitor = dom.DocumentElement.SelectSingleNode("monitors/readersMonitor") as XmlElement;
                if (readersMonitor != null)
                {
                    string strOldValue = readersMonitor.GetAttribute("types");
                    if (string.IsNullOrEmpty(strOldValue) == true)
                    {
                        string strNewValue = "mq";
                        readersMonitor.SetAttribute("types", strNewValue);
                        bChanged = true;
                    }
                    else
                    {
                        // 增加 mq
                        string strNewValue = strOldValue;
                        StringUtil.SetInList(ref strNewValue, "mq", true);
                        if (strNewValue != strOldValue)
                        {
                            readersMonitor.SetAttribute("types", strNewValue);
                            bChanged = true;
                        }
                    }
                }
            }

            // circulation 元素的 notifyTypes 属性
            {
                XmlElement circulation = dom.DocumentElement.SelectSingleNode("circulation") as XmlElement;
                if (circulation == null)
                {
                    circulation = dom.CreateElement("circulation");
                    dom.DocumentElement.AppendChild(circulation);
                    bChanged = true;
                }

                string strOldValue = circulation.GetAttribute("notifyTypes");
                if (string.IsNullOrEmpty(strOldValue) == true)
                {
                    string strNewValue = "mq";
                    circulation.SetAttribute("notifyTypes", strNewValue);
                    bChanged = true;
                }
                else
                {
                    // 增加 mq
                    string strNewValue = strOldValue;
                    StringUtil.SetInList(ref strNewValue, "mq", true);
                    if (strNewValue != strOldValue)
                    {
                        circulation.SetAttribute("notifyTypes", strNewValue);
                        bChanged = true;
                    }
                }
            }

            if (bChanged == true)
            {
                // 提前备份一个原来文件，避免保存中途出错造成 0 bytes 的文件
                string strBackupFileName = Path.Combine(strDataDir, "library.xml.config.save");
                File.Copy(strLibraryXmlFileName, strBackupFileName, true);

                dom.Save(strLibraryXmlFileName);
                return 1;   // 发生了修改
            }

            return 0;   // 没有发生修改
#endif
        }

        int ConfigMongoDB(bool bAdd,
    out string strError)
        {
            strError = "";

            string strDataDir = this.textBox_dataDir.Text;
            string strInstanceName = this.textBox_instanceName.Text;

            if (string.IsNullOrEmpty(strDataDir))
            {
                strError = "尚未指定数据目录";
                return -1;
            }

            string strLibraryXmlFileName = Path.Combine(strDataDir, "library.xml");
            if (File.Exists(strLibraryXmlFileName) == false)
            {
                strError = "配置文件 '" + strLibraryXmlFileName + "' 不存在，无法进行进一步配置";
                return -1;
            }

            return InstallHelper.SetupMongoDB(
                strLibraryXmlFileName,
                strInstanceName,
                bAdd,
                out strError);
        }

        // 证书
        private void toolStripButton_certificate_Click(object sender, EventArgs e)
        {
            CertificateDialog dlg = new CertificateDialog();
            dlg.Font = this.Font;
            dlg.SN = LineInfo.CertificateSN;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel)
                return;

            LineInfo.CertificateSN = dlg.SN;
        }

        // 序列号
        private void toolStripButton_setSerialNumber_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 2014/11/15
            string strFirstMac = "";
            List<string> macs = SerialCodeForm.GetMacAddress();
            if (macs.Count != 0)
            {
                strFirstMac = macs[0];
            }

            // Debug.Assert(false, "");
            string strSerialCode = this.LineInfo.SerialNumber;
        REDO_INPUT:
            // 出现设置序列号对话框
            nRet = ResetSerialCode(
                this,
                "为实例 '" + this.InstanceName + "' 设置序列号",
                true,
                ref strSerialCode,
                GetEnvironmentString(strFirstMac, strSerialCode, this.InstanceName));
            if (nRet == 0)
            {
                strError = "放弃";
                goto ERROR1;
            }
            if (string.IsNullOrEmpty(strSerialCode) == true
                || strSerialCode == "community"
                || strSerialCode == "*")
            {
                // MessageBox.Show(this, "序列号为空，将按照最多 5 个前端方式运行");
                this.LineInfo.SerialNumber = strSerialCode;
                return;
            }

            //string strLocalString = GetEnvironmentString(strSerialCode, this.InstanceName);
            //string strSha1 = Cryptography.GetSHA1(StringUtil.SortParams(strLocalString) + "_reply");
            if (String.IsNullOrEmpty(strSerialCode) == true
                || MatchLocalString(strSerialCode, this.InstanceName) == false)
            //    if (strSha1 != SerialCodeForm.GetCheckCode(strSerialCode) || String.IsNullOrEmpty(strSerialCode) == true)
            {
                if (String.IsNullOrEmpty(strSerialCode) == false)
                    MessageBox.Show(this, "序列号无效。请重新输入");
                goto REDO_INPUT;
            }

            this.LineInfo.SerialNumber = strSerialCode;
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 为 library.xml 配置 MSMQ 相关参数
        private void ToolStripMenuItem_configMq_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ConfigMq(
                Control.ModifierKeys == Keys.Control ? false : true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 重新启动一次 dp2library? 没有必要，因为整个实例对话框进入以前，dp2library 已经暂停了。对话框退出后会重新启动。

            MessageBox.Show(this, "配置成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 为 library.xml 自动配置 MongoDB 参数
        private void ToolStripMenuItem_configMongoDB_Click(object sender, EventArgs e)
        {
            string strError = "";

            int nRet = ConfigMongoDB(
                Control.ModifierKeys == Keys.Control ? false : true,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 重新启动一次 dp2library? 没有必要，因为整个实例对话框进入以前，dp2library 已经暂停了。对话框退出后会重新启动。

            MessageBox.Show(this, "配置成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 为 library.xml 配置服务器同步参数
        private void ToolStripMenuItem_configServerReplication_Click(object sender, EventArgs e)
        {
            string strError = "";

            string strDataDir = this.textBox_dataDir.Text;
            string strInstanceName = this.textBox_instanceName.Text;

            if (string.IsNullOrEmpty(strDataDir))
            {
                strError = "尚未指定数据目录";
                goto ERROR1;
            }

            string strLibraryXmlFileName = Path.Combine(strDataDir, "library.xml");
            if (File.Exists(strLibraryXmlFileName) == false)
            {
                strError = "配置文件 '" + strLibraryXmlFileName + "' 不存在，无法进行进一步配置";
                goto ERROR1;
            }

            LibraryXmlDialog dlg = new LibraryXmlDialog();
            GuiUtil.SetControlFont(dlg, this.Font);
            dlg.Text = "服务器同步参数";
            dlg.LibraryXmlFileName = strLibraryXmlFileName;
            dlg.ShowDialog(this);

            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                return;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
