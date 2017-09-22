using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Text;
using System.ServiceProcess;

using Microsoft.Win32;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Install;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.rms.Client;
using DigitalPlatform.LibraryServer;

namespace dp2Library
{
    [RunInstaller(true)]
    public partial class Installer1 : Installer
    {
        private System.ServiceProcess.ServiceProcessInstaller ServiceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;

        public Installer1()
        {
            InitializeComponent();

            this.ServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // ServiceProcessInstaller1
            // 
            this.ServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;  // LocalSystem
            this.ServiceProcessInstaller1.Password = null;
            this.ServiceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.DisplayName = "dp2 Library Service";
            this.serviceInstaller1.ServiceName = "dp2LibraryService";
            this.serviceInstaller1.Description = "dp2图书馆应用服务器，数字平台北京软件有限责任公司 http://dp2003.com";
            this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
            /* // dp2Library和dp2Kernel可以不在同一台机器，所以不能设定依赖关系
            this.serviceInstaller1.ServicesDependedOn = new string[] {
                "dp2KernelService"};
             * */
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
				this.ServiceProcessInstaller1,
				this.serviceInstaller1});

            this.serviceInstaller1.Committed += new InstallEventHandler(serviceInstaller1_Committed);
        }

        void serviceInstaller1_Committed(object sender, InstallEventArgs e)
        {
            try
            {
                ServiceController sc = new ServiceController(this.serviceInstaller1.ServiceName);
                sc.Start();
            }
            catch (Exception ex)
            {
                // 报错，但是不停止安装
                MessageBox.Show(ForegroundWindow.Instance,
                    "安装已经完成，但启动 '" + this.serviceInstaller1.ServiceName + "' 失败： " + ex.Message);
            }
        }

        static string UnQuote(string strText)
        {
            return strText.Replace("'", "");
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string strParameter = this.Context.Parameters["rootdir"];
            if (string.IsNullOrEmpty(strParameter) == true)
                return;

#if NO
            string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

            InstanceDialog dlg = new InstanceDialog();
            GuiUtil.AutoSetDefaultFont(dlg);

            dlg.SourceDir = strRootDir;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(ForegroundWindow.Instance);

            if (dlg.DialogResult == DialogResult.Cancel)
                throw new InstallException("用户取消安装。");

            if (dlg.Changed == true)
            {
                // 兑现修改

            }
#endif
        }

        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);

#if NO
            string strParameter = this.Context.Parameters["rootdir"];
            if (string.IsNullOrEmpty(strParameter) == true)
                return;
#endif

            // 创建事件日志目录
            if (!EventLog.SourceExists("dp2Library"))
            {
                EventLog.CreateEventSource("dp2Library", "DigitalPlatform");
            }
            EventLog Log = new EventLog();
            Log.Source = "dp2Library";

            // string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

            Log.WriteEntry("dp2library 安装成功。", EventLogEntryType.Information);
        }

        public override void Rollback(System.Collections.IDictionary savedState)
        {
            base.Rollback(savedState);

#if NO
            int nRet = 0;
            string strError = "";

            string strRootDir = UnQuote(this.Context.Parameters["rootdir"]);

            string strDataDir = (string)savedState["datadir"];
            string strDataDir_newly = (string)savedState["datadir_newly"];

            if (String.IsNullOrEmpty(strDataDir) == false
                && strDataDir_newly == "yes")
            {
            REDO_DELETE_DATADIR:
                // 删除数据目录
                try
                {
                    Directory.Delete(strDataDir, true);
                }
                catch (Exception ex)
                {
                    DialogResult temp_result = MessageBox.Show(ForegroundWindow.Instance,
                        "删除数据目录'" + strDataDir + "'出错：" + ex.Message + "\r\n\r\n是否重试?\r\n\r\n(Retry: 重试; Cancel: 不重试，继续后续卸载过程)",
                        "install dp2library -- 回滚",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);

                    if (temp_result == DialogResult.Retry)
                        goto REDO_DELETE_DATADIR;
                }
            }
#endif
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            // Debug.Assert(false, "");

            base.Uninstall(savedState);

            string strParameter = this.Context.Parameters["rootdir"];
            if (string.IsNullOrEmpty(strParameter) == true)
                return;


#if NO
            String strRootDir = UnQuote(strParameter);

            DialogResult result;

            string strText = "是否完全卸载？\r\n\r\n"
                + "单击'是'，则把全部实例的数据目录删除，所有的库配置信息丢失，所有的实例信息丢失。以后安装时需要重新安装数据目录和数据库。\r\n\r\n"
                + "单击'否'，不删除数据目录，仅卸载执行程序，下次安装时可以继续使用已存在的库配置信息。升级安装前的卸载应选此项。";
            result = MessageBox.Show(ForegroundWindow.Instance,
                strText,
                "卸载 dp2Library",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                InstanceDialog dlg = new InstanceDialog();
                GuiUtil.AutoSetDefaultFont(dlg);
                dlg.Text = "彻底卸载所有实例和数据目录";
                dlg.Comment = "下列实例将被全部卸载。请仔细确认。一旦卸载，全部数据目录和实例信息将被删除，并且无法恢复。";
                dlg.UninstallMode = true;
                dlg.SourceDir = strRootDir;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(ForegroundWindow.Instance);

                if (dlg.DialogResult == DialogResult.Cancel)
                {
                    MessageBox.Show(ForegroundWindow.Instance,
                        "已放弃卸载全部实例和数据目录。仅仅卸载了可执行程序。");
                }
            }
#endif
        }
    }
}