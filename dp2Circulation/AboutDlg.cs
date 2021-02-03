using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Deployment.Application;

using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2Circulation
{
    /// <summary>
    /// 版权 对话框
    /// </summary>
    internal class AboutDlg : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label_copyright;
        private System.Windows.Forms.TextBox textBox_environment;
        private System.Windows.Forms.Button button_health;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AboutDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            // this.Opacity = 0.75;

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.label_copyright = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_environment = new System.Windows.Forms.TextBox();
            this.button_health = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(18, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(718, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2 内务/流通 dp2Circulation V3";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_copyright
            // 
            this.label_copyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_copyright.Location = new System.Drawing.Point(18, 86);
            this.label_copyright.Name = "label_copyright";
            this.label_copyright.Size = new System.Drawing.Size(718, 72);
            this.label_copyright.TabIndex = 1;
            this.label_copyright.Text = "(C) 版权所有 2006-2015 数字平台(北京)软件有限责任公司\r\nDigital Platform (Beijing) Software Corp. Lt" +
    "d.";
            this.label_copyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.linkLabel1.Location = new System.Drawing.Point(18, 158);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(714, 61);
            this.linkLabel1.TabIndex = 2;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "https://github.com/DigitalPlatform/dp2";
            this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_OK.AutoSize = true;
            this.button_OK.Location = new System.Drawing.Point(288, 494);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(176, 53);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_environment
            // 
            this.textBox_environment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_environment.BackColor = System.Drawing.Color.MidnightBlue;
            this.textBox_environment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_environment.ForeColor = System.Drawing.Color.Gainsboro;
            this.textBox_environment.Location = new System.Drawing.Point(18, 225);
            this.textBox_environment.MaxLength = 0;
            this.textBox_environment.Multiline = true;
            this.textBox_environment.Name = "textBox_environment";
            this.textBox_environment.ReadOnly = true;
            this.textBox_environment.Size = new System.Drawing.Size(718, 252);
            this.textBox_environment.TabIndex = 6;
            this.textBox_environment.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button_health
            // 
            this.button_health.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_health.AutoSize = true;
            this.button_health.Location = new System.Drawing.Point(556, 494);
            this.button_health.Name = "button_health";
            this.button_health.Size = new System.Drawing.Size(176, 53);
            this.button_health.TabIndex = 7;
            this.button_health.Text = "健康状态";
            this.button_health.Click += new System.EventHandler(this.button_health_Click);
            // 
            // AboutDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(11, 24);
            this.BackColor = System.Drawing.Color.DarkSlateGray;
            this.ClientSize = new System.Drawing.Size(754, 561);
            this.Controls.Add(this.button_health);
            this.Controls.Add(this.textBox_environment);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label_copyright);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.Gainsboro;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDlg";
            this.ShowInTaskbar = false;
            this.Text = "关于 About";
            this.Load += new System.EventHandler(this.CopyrightDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(// "iexplore",
                linkLabel1.Text);
        }

        private void CopyrightDlg_Load(object sender, System.EventArgs e)
        {
            label_copyright.Text = "(C) 版权所有 2006-2015 数字平台(北京)软件有限责任公司\r\n2015 年以 Apache License Version 2.0 方式开源";

            Assembly myAssembly = Assembly.GetAssembly(this.GetType());
            AssemblyName name = myAssembly.GetName();

            textBox_environment.Text = "版本和环境:"
                + "\r\n本软件: " + name.Name + " " + name.Version.ToString()    // .FullName
                + "\r\n当前连接的 dp2Library (位于 " + Program.MainForm.LibraryServerUrl + "): " + Program.MainForm.ServerVersion.ToString() + " UID:" + Program.MainForm.ServerUID + " 失效期:" + Program.MainForm.ExpireDate
                + $"\r\n当前登录账户:{Program.MainForm.GetCurrentUserName()} token:{Program.MainForm.GetCurrentAccountToken()}"
                + "\r\n本机 .NET Framework 版本: " + myAssembly.ImageRuntimeVersion
                + "\r\n\r\n本机 MAC 地址: " + StringUtil.MakePathList(SerialCodeForm.GetMacAddress())
                // + "\r\n是否安装 KB2468871: " + Global.IsKbInstalled("KB2468871")
                + "\r\n是否 ClickOnce 安装: " + ApplicationDeployment.IsNetworkDeployed;
        }

        private void button_health_Click(object sender, System.EventArgs e)
        {
            Task.Run(() =>
            {
                Program.MainForm.Invoke((Action)(() =>
                {
                    var form = Program.MainForm.EnsureChildForm<UtilityForm>();
                    form.Health();
                }));
            });

            this.Close();
        }
    }
}
