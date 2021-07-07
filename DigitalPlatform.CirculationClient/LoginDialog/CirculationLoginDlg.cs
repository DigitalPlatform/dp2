using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

using Newtonsoft.Json;

using DigitalPlatform.GUI;
using DigitalPlatform.CommonControl;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// dp2library 前端登录的对话框
    /// </summary>
    public class CirculationLoginDlg : System.Windows.Forms.Form
    {
        public System.Windows.Forms.CheckBox checkBox_savePasswordShort;
        public System.Windows.Forms.TextBox textBox_password;
        public System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_userName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        public System.Windows.Forms.TextBox textBox_comment;
        public TextBox textBox_location;
        private Label label4;
        private CheckBox checkBox_isReader;
        private CheckBox checkBox_savePasswordLong;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        const int WM_MOVE_FOCUS = API.WM_USER + 201;

        private MessageBalloon m_firstUseBalloon = null;

        public bool AutoShowShortSavePasswordTip = false;
        private ToolStrip toolStrip_server;
        private ToolStripButton toolStripButton_server_setXeServer;
        private ToolStripButton toolStripButton_server_setHongnibaServer;
        private ToolStripSeparator toolStripSeparator1;

        public bool SetDefaultMode = false;
        private ComboBox comboBox_serverAddr;
        private ToolStripButton toolStripButton_deleteFromList;
        public TextBox textBox_phoneNumber;
        private Label label_phoneNumber;
        public TextBox textBox_tempCode;
        private Label label_tempCode; // 是否为 设置缺省帐户 状态？ 第一次进入程序时候是这个状态，其他登录失败后重新输入以便登录的时候不是这个状态
        private ToolStripButton toolStripButton_pasteFromJSONClipboard;
        private ToolStripButton toolStripButton_changePassword;
        public bool SupervisorMode = false; // 是否为 supervisor 模式。也就是管理员模式。在这个模式下， 无法修改 URL ，无法选择读者类型，不出现 红泥巴数字平台服务器按钮

        public CirculationLoginDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // http://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c
            this.toolStrip_server.Renderer = new TransparentToolStripRenderer(this.toolStrip_server);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CirculationLoginDlg));
            this.checkBox_savePasswordShort = new System.Windows.Forms.CheckBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label_userName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.textBox_location = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox_isReader = new System.Windows.Forms.CheckBox();
            this.checkBox_savePasswordLong = new System.Windows.Forms.CheckBox();
            this.toolStrip_server = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_server_setXeServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_server_setHongnibaServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_deleteFromList = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_pasteFromJSONClipboard = new System.Windows.Forms.ToolStripButton();
            this.comboBox_serverAddr = new System.Windows.Forms.ComboBox();
            this.textBox_phoneNumber = new System.Windows.Forms.TextBox();
            this.label_phoneNumber = new System.Windows.Forms.Label();
            this.textBox_tempCode = new System.Windows.Forms.TextBox();
            this.label_tempCode = new System.Windows.Forms.Label();
            this.toolStripButton_changePassword = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_server.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_savePasswordShort
            // 
            this.checkBox_savePasswordShort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_savePasswordShort.AutoSize = true;
            this.checkBox_savePasswordShort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_savePasswordShort.Location = new System.Drawing.Point(220, 427);
            this.checkBox_savePasswordShort.Margin = new System.Windows.Forms.Padding(0);
            this.checkBox_savePasswordShort.Name = "checkBox_savePasswordShort";
            this.checkBox_savePasswordShort.Size = new System.Drawing.Size(193, 25);
            this.checkBox_savePasswordShort.TabIndex = 9;
            this.checkBox_savePasswordShort.Text = "短期保持密码(&S)";
            this.checkBox_savePasswordShort.CheckedChanged += new System.EventHandler(this.checkBox_savePasswordShort_CheckedChanged);
            this.checkBox_savePasswordShort.Click += new System.EventHandler(this.controls_Click);
            // 
            // textBox_password
            // 
            this.textBox_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_password.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_password.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_password.Location = new System.Drawing.Point(220, 387);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(284, 31);
            this.textBox_password.TabIndex = 8;
            this.textBox_password.Click += new System.EventHandler(this.controls_Click);
            this.textBox_password.TextChanged += new System.EventHandler(this.textBox_password_TextChanged);
            // 
            // textBox_userName
            // 
            this.textBox_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_userName.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_userName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_userName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_userName.Location = new System.Drawing.Point(220, 313);
            this.textBox_userName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(284, 31);
            this.textBox_userName.TabIndex = 5;
            this.textBox_userName.Click += new System.EventHandler(this.controls_Click);
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cancel.Location = new System.Drawing.Point(638, 595);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(143, 42);
            this.button_cancel.TabIndex = 18;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 390);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 21);
            this.label3.TabIndex = 7;
            this.label3.Text = "密码(&P):";
            // 
            // label_userName
            // 
            this.label_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_userName.AutoSize = true;
            this.label_userName.Location = new System.Drawing.Point(18, 317);
            this.label_userName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label_userName.Name = "label_userName";
            this.label_userName.Size = new System.Drawing.Size(117, 21);
            this.label_userName.TabIndex = 4;
            this.label_userName.Text = "用户名(&U):";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 187);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "图书馆应用服务器地址(&H):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Location = new System.Drawing.Point(638, 542);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(143, 42);
            this.button_OK.TabIndex = 17;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBox_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_comment.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_comment.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox_comment.Location = new System.Drawing.Point(22, 21);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(759, 161);
            this.textBox_comment.TabIndex = 0;
            this.textBox_comment.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_location
            // 
            this.textBox_location.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_location.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_location.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_location.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_location.Location = new System.Drawing.Point(220, 464);
            this.textBox_location.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_location.Name = "textBox_location";
            this.textBox_location.Size = new System.Drawing.Size(284, 31);
            this.textBox_location.TabIndex = 11;
            this.textBox_location.Click += new System.EventHandler(this.controls_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 467);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "工作台号(&W):";
            // 
            // checkBox_isReader
            // 
            this.checkBox_isReader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_isReader.AutoSize = true;
            this.checkBox_isReader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_isReader.Location = new System.Drawing.Point(220, 353);
            this.checkBox_isReader.Margin = new System.Windows.Forms.Padding(0);
            this.checkBox_isReader.Name = "checkBox_isReader";
            this.checkBox_isReader.Size = new System.Drawing.Size(109, 25);
            this.checkBox_isReader.TabIndex = 6;
            this.checkBox_isReader.Text = "读者(&R)";
            this.checkBox_isReader.UseVisualStyleBackColor = true;
            this.checkBox_isReader.CheckedChanged += new System.EventHandler(this.checkBox_isReader_CheckedChanged);
            this.checkBox_isReader.Click += new System.EventHandler(this.controls_Click);
            // 
            // checkBox_savePasswordLong
            // 
            this.checkBox_savePasswordLong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_savePasswordLong.AutoSize = true;
            this.checkBox_savePasswordLong.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_savePasswordLong.Location = new System.Drawing.Point(24, 514);
            this.checkBox_savePasswordLong.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.checkBox_savePasswordLong.Name = "checkBox_savePasswordLong";
            this.checkBox_savePasswordLong.Size = new System.Drawing.Size(193, 25);
            this.checkBox_savePasswordLong.TabIndex = 12;
            this.checkBox_savePasswordLong.Text = "长期保持密码(&L)";
            this.checkBox_savePasswordLong.UseVisualStyleBackColor = true;
            this.checkBox_savePasswordLong.CheckedChanged += new System.EventHandler(this.checkBox_savePasswordLong_CheckedChanged);
            this.checkBox_savePasswordLong.Click += new System.EventHandler(this.controls_Click);
            // 
            // toolStrip_server
            // 
            this.toolStrip_server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_server.AutoSize = false;
            this.toolStrip_server.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_server.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_server.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip_server.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_server_setXeServer,
            this.toolStripSeparator1,
            this.toolStripButton_server_setHongnibaServer,
            this.toolStripButton_deleteFromList,
            this.toolStripButton_pasteFromJSONClipboard,
            this.toolStripButton_changePassword});
            this.toolStrip_server.Location = new System.Drawing.Point(22, 248);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip_server.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_server.Size = new System.Drawing.Size(759, 44);
            this.toolStrip_server.TabIndex = 3;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_server_setXeServer
            // 
            this.toolStripButton_server_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setXeServer.Name = "toolStripButton_server_setXeServer";
            this.toolStripButton_server_setXeServer.Size = new System.Drawing.Size(142, 38);
            this.toolStripButton_server_setXeServer.Text = "单机版服务器";
            this.toolStripButton_server_setXeServer.ToolTipText = "设为单机版服务器";
            this.toolStripButton_server_setXeServer.Click += new System.EventHandler(this.toolStripButton_server_setXeServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripButton_server_setHongnibaServer
            // 
            this.toolStripButton_server_setHongnibaServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setHongnibaServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setHongnibaServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setHongnibaServer.Name = "toolStripButton_server_setHongnibaServer";
            this.toolStripButton_server_setHongnibaServer.Size = new System.Drawing.Size(231, 38);
            this.toolStripButton_server_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_server_setHongnibaServer_Click);
            // 
            // toolStripButton_deleteFromList
            // 
            this.toolStripButton_deleteFromList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_deleteFromList.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_deleteFromList.Image")));
            this.toolStripButton_deleteFromList.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_deleteFromList.Name = "toolStripButton_deleteFromList";
            this.toolStripButton_deleteFromList.Size = new System.Drawing.Size(40, 38);
            this.toolStripButton_deleteFromList.Text = "从列表中删除此项";
            this.toolStripButton_deleteFromList.Click += new System.EventHandler(this.toolStripButton_deleteFromList_Click);
            // 
            // toolStripButton_pasteFromJSONClipboard
            // 
            this.toolStripButton_pasteFromJSONClipboard.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_pasteFromJSONClipboard.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_pasteFromJSONClipboard.Image")));
            this.toolStripButton_pasteFromJSONClipboard.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_pasteFromJSONClipboard.Name = "toolStripButton_pasteFromJSONClipboard";
            this.toolStripButton_pasteFromJSONClipboard.Size = new System.Drawing.Size(121, 38);
            this.toolStripButton_pasteFromJSONClipboard.Text = "粘贴服务器";
            this.toolStripButton_pasteFromJSONClipboard.ToolTipText = "从 Windows 剪贴板中粘贴服务器参数";
            this.toolStripButton_pasteFromJSONClipboard.Click += new System.EventHandler(this.toolStripButton_pasteFromJSONClipboard_Click);
            // 
            // comboBox_serverAddr
            // 
            this.comboBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_serverAddr.BackColor = System.Drawing.SystemColors.ControlLight;
            this.comboBox_serverAddr.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.comboBox_serverAddr.DropDownHeight = 260;
            this.comboBox_serverAddr.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_serverAddr.ForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBox_serverAddr.FormattingEnabled = true;
            this.comboBox_serverAddr.IntegralHeight = false;
            this.comboBox_serverAddr.Location = new System.Drawing.Point(22, 214);
            this.comboBox_serverAddr.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_serverAddr.Name = "comboBox_serverAddr";
            this.comboBox_serverAddr.Size = new System.Drawing.Size(756, 32);
            this.comboBox_serverAddr.TabIndex = 2;
            this.comboBox_serverAddr.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBox_serverAddr_DrawItem);
            this.comboBox_serverAddr.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.comboBox_serverAddr_MeasureItem);
            this.comboBox_serverAddr.SelectedIndexChanged += new System.EventHandler(this.comboBox_serverAddr_SelectedIndexChanged);
            this.comboBox_serverAddr.Click += new System.EventHandler(this.controls_Click);
            // 
            // textBox_phoneNumber
            // 
            this.textBox_phoneNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_phoneNumber.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_phoneNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_phoneNumber.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_phoneNumber.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_phoneNumber.Location = new System.Drawing.Point(220, 550);
            this.textBox_phoneNumber.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_phoneNumber.Name = "textBox_phoneNumber";
            this.textBox_phoneNumber.Size = new System.Drawing.Size(284, 31);
            this.textBox_phoneNumber.TabIndex = 14;
            // 
            // label_phoneNumber
            // 
            this.label_phoneNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_phoneNumber.AutoSize = true;
            this.label_phoneNumber.Location = new System.Drawing.Point(18, 553);
            this.label_phoneNumber.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label_phoneNumber.Name = "label_phoneNumber";
            this.label_phoneNumber.Size = new System.Drawing.Size(117, 21);
            this.label_phoneNumber.TabIndex = 13;
            this.label_phoneNumber.Text = "手机号(&P):";
            // 
            // textBox_tempCode
            // 
            this.textBox_tempCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_tempCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_tempCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_tempCode.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_tempCode.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_tempCode.Location = new System.Drawing.Point(220, 590);
            this.textBox_tempCode.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_tempCode.Name = "textBox_tempCode";
            this.textBox_tempCode.Size = new System.Drawing.Size(284, 31);
            this.textBox_tempCode.TabIndex = 16;
            this.textBox_tempCode.Visible = false;
            // 
            // label_tempCode
            // 
            this.label_tempCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_tempCode.AutoSize = true;
            this.label_tempCode.Location = new System.Drawing.Point(18, 593);
            this.label_tempCode.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label_tempCode.Name = "label_tempCode";
            this.label_tempCode.Size = new System.Drawing.Size(117, 21);
            this.label_tempCode.TabIndex = 15;
            this.label_tempCode.Text = "验证码(&S):";
            this.label_tempCode.Visible = false;
            // 
            // toolStripButton_changePassword
            // 
            this.toolStripButton_changePassword.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_changePassword.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_changePassword.Image")));
            this.toolStripButton_changePassword.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_changePassword.Name = "toolStripButton_changePassword";
            this.toolStripButton_changePassword.Size = new System.Drawing.Size(79, 38);
            this.toolStripButton_changePassword.Text = "改密码";
            this.toolStripButton_changePassword.Click += new System.EventHandler(this.toolStripButton_changePassword_Click);
            // 
            // CirculationLoginDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(803, 658);
            this.Controls.Add(this.textBox_tempCode);
            this.Controls.Add(this.label_tempCode);
            this.Controls.Add(this.textBox_phoneNumber);
            this.Controls.Add(this.label_phoneNumber);
            this.Controls.Add(this.comboBox_serverAddr);
            this.Controls.Add(this.checkBox_savePasswordLong);
            this.Controls.Add(this.checkBox_isReader);
            this.Controls.Add(this.textBox_location);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.checkBox_savePasswordShort);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label_userName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.toolStrip_server);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "CirculationLoginDlg";
            this.ShowInTaskbar = false;
            this.Text = "登录";
            this.Load += new System.EventHandler(this.LoginDlg_Load);
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        bool _serverAddrEnabled = true;
        public bool ServerAddrEnabled
        {
            get
            {
                return _serverAddrEnabled;
            }
            set
            {
                _serverAddrEnabled = value;
                this.comboBox_serverAddr.Enabled = value;
            }
        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            string strError = "";

            if (comboBox_serverAddr.Text == ""
                // && textBox_serverAddr.Enabled == true
                && this.ServerAddrEnabled == true)
            {
                strError = "尚未输入服务器地址";
                goto ERROR1;
            }
            if (string.IsNullOrEmpty(textBox_userName.Text))
            {
                strError = "尚未输入用户名";
                goto ERROR1;
            }

            // 2017/4/11
            if (this.textBox_tempCode.Visible == false)
                this.textBox_tempCode.Text = "";

            if (this.RetryLogin && string.IsNullOrEmpty(this.textBox_phoneNumber.Text) == false)
            {
#if NO
                InputTempPasswordDialog dlg = new InputTempPasswordDialog();
                GuiUtil.AutoSetDefaultFont(dlg);
                // dlg.Font = this.Font;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    MessageBox.Show(this, "放弃以手机验证码方式登录。(若要用其他方式登录，请清除“手机短信验证”复选框再点“登录”按钮)");
                    return;
                }
                this._tempCode = dlg.TempPassword;
#endif
                if (string.IsNullOrEmpty(this.TempCode))
                {
                    strError = "请输入您收到的手机短信中的验证码";
                    goto ERROR1;
                }
            }

            this.SavePannel();
            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoginDlg_Load(object sender, System.EventArgs e)
        {
            if (textBox_comment.Text == "")
                textBox_comment.Visible = false;

            if (this.SupervisorMode == true)
            {
                this.checkBox_isReader.Visible = false;
                this.toolStrip_server.Visible = false;
                this.comboBox_serverAddr.Enabled = false;   // readonly
            }

            // 打开 修改密码 对话框
            if (this.PasswordExpired)
            {
                Task.Run(()=> {
                    this.Invoke((Action)(() =>
                    {
                        ChangePasswordDialog dlg = new ChangePasswordDialog();
                        dlg.Font = this.Font;
                        dlg.ServerUrl = this.ServerUrl;
                        dlg.UserName = this.UserName;
                        dlg.OldPassword = this.Password;
                        dlg.IsReader = this.IsReader;
                        dlg.StartPosition = FormStartPosition.CenterParent;
                        dlg.ShowDialog(this);
                        if (dlg.DialogResult == DialogResult.OK)
                            this.textBox_password.Text = dlg.NewPassword;
                    }));
                });
            }

            API.PostMessage(this.Handle, WM_MOVE_FOCUS, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_MOVE_FOCUS:

                    // 2008/7/1
                    if (this.textBox_userName.Text == "")
                        this.textBox_userName.Focus();
                    else if (this.textBox_password.Text == "")
                        this.textBox_password.Focus();
                    else
                        this.button_OK.Focus();

                    if (AutoShowShortSavePasswordTip == true)
                        ShowShortSavePasswordTip();

                    SetFocus();
                    return;
            }
            base.DefWndProc(ref m);
        }

        public string UserName
        {
            get
            {
                return this.textBox_userName.Text;
            }
            set
            {
                this.textBox_userName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_password.Text;
            }
            set
            {
                this.textBox_password.Text = value;
            }
        }

        public bool PasswordEnabled
        {
            get
            {
                return this.textBox_password.Enabled;
            }
            set
            {
                this.textBox_password.Enabled = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.comboBox_serverAddr.Text;
            }
            set
            {
                this.comboBox_serverAddr.Text = value;
            }
        }

        public bool SavePasswordShort
        {
            get
            {
                return this.checkBox_savePasswordShort.Checked;
            }
            set
            {
                this.checkBox_savePasswordShort.Checked = value;
            }
        }

        public bool SavePasswordShortEnabled
        {
            get
            {
                return this.checkBox_savePasswordShort.Enabled;
            }
            set
            {
                this.checkBox_savePasswordShort.Enabled = value;
            }
        }

        public bool SavePasswordLong
        {
            get
            {
                return this.checkBox_savePasswordLong.Checked;
            }
            set
            {
                this.checkBox_savePasswordLong.Checked = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        public string OperLocation
        {
            get
            {
                return this.textBox_location.Text;
            }
            set
            {
                this.textBox_location.Text = value;
            }
        }

        public bool IsReader
        {
            get
            {
                return this.checkBox_isReader.Checked;
            }
            set
            {
                this.checkBox_isReader.Checked = value;
            }
        }

        public bool IsReaderEnabled
        {
            get
            {
                return this.checkBox_isReader.Enabled;
            }
            set
            {
                this.checkBox_isReader.Enabled = value;
            }
        }

        private void checkBox_savePasswordLong_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_savePasswordLong.Checked == true)
                this.checkBox_savePasswordShort.Checked = true;
        }

        public void ShowShortSavePasswordTip()
        {
            m_firstUseBalloon = new MessageBalloon();
            m_firstUseBalloon.Parent = this.checkBox_savePasswordShort;
            m_firstUseBalloon.Title = "小技巧";
            m_firstUseBalloon.TitleIcon = TooltipIcon.Info;
            m_firstUseBalloon.Text = "勾选 “短期保存密码” 项，可以让内务前端在运行期间记住您用过的密码，不再反复出现本对话框";

            m_firstUseBalloon.Align = BalloonAlignment.BottomRight;
            m_firstUseBalloon.CenterStem = false;
            m_firstUseBalloon.UseAbsolutePositioning = false;
            m_firstUseBalloon.Show();

            this.checkBox_savePasswordShort.BackColor = SystemColors.Highlight;
            this.checkBox_savePasswordShort.ForeColor = SystemColors.HighlightText;
            this.checkBox_savePasswordShort.Padding = new Padding(6);

        }

        void HideMessageTip()
        {
            if (m_firstUseBalloon == null)
                return;

            m_firstUseBalloon.Dispose();
            m_firstUseBalloon = null;

            this.checkBox_savePasswordShort.BackColor = SystemColors.Control;
            this.checkBox_savePasswordShort.ForeColor = SystemColors.ControlText;
            this.checkBox_savePasswordShort.Padding = new Padding(0);
        }

        private void controls_Click(object sender, EventArgs e)
        {
            HideMessageTip();
        }

        /// <summary>
        /// 处理对话框键
        /// </summary>
        /// <param name="keyData">System.Windows.Forms.Keys 值之一，它表示要处理的键。</param>
        /// <returns>如果控件处理并使用击键，则为 true；否则为 false，以允许进一步处理</returns>
        protected override bool ProcessDialogKey(
Keys keyData)
        {
            /*
            if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return true;
            }
             * */

            HideMessageTip();

            return base.ProcessDialogKey(keyData);
        }

        private void checkBox_savePasswordShort_CheckedChanged(object sender, EventArgs e)
        {
            // 在设置缺省帐户的情况下，如果去掉短期保持密码的选择，必须同时清除已经输入的密码字符，这样可以提醒用户，这样(unchecked)情况下，只能用空密码继续本对话框后面的操作
            if (this.SetDefaultMode == true)
            {
                if (this.checkBox_savePasswordShort.Checked == false)
                {
                    this.textBox_password.Text = "";
                }
            }
        }

        private void textBox_password_TextChanged(object sender, EventArgs e)
        {
            // 在设置缺省帐户的情况下，只要输入了密码字符，就表示要短期维持密码。否则容易造成后面自动用空密码试探登录(并可能会登录成功)，容易造成误会
            if (this.SetDefaultMode == true)
            {
                if (string.IsNullOrEmpty(this.textBox_password.Text) == false)
                    this.checkBox_savePasswordShort.Checked = true;
            }
        }

        private void toolStripButton_server_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.comboBox_serverAddr.Text != ServerDlg.HnbUrl)
            {
                // this.textBox_serverName.Text = "红泥巴.数字平台服务器";
                this.comboBox_serverAddr.Text = ServerDlg.HnbUrl;

                this.textBox_userName.Text = "";
                this.textBox_password.Text = "";
            }
        }

        public static string dp2LibraryXEServerUrl = "net.pipe://localhost/dp2library/xe";

        private void toolStripButton_server_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.comboBox_serverAddr.Text != dp2LibraryXEServerUrl)
            {
                // this.textBox_serverName.Text = "单机版服务器";
                this.comboBox_serverAddr.Text = dp2LibraryXEServerUrl;

                this.textBox_userName.Text = "supervisor";
                this.textBox_password.Text = "";
            }
        }

        private void checkBox_isReader_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_isReader.Checked == false)
            {
                this.label_userName.Text = "用户名(&U):";
                this.BackColor = SystemColors.ControlDark;
                this.ForeColor = SystemColors.ControlText;
                this.toolStrip_server.BackColor = SystemColors.ControlDark;
            }
            else
            {
                this.label_userName.Text = "读者证条码号(&B):";
                this.BackColor = Color.DarkGreen;
                this.ForeColor = Color.White;
                this.toolStrip_server.BackColor = this.BackColor;
            }
        }

        private void comboBox_serverAddr_Click(object sender, EventArgs e)
        {

        }

        // 为 urlList 添加服务器名
        public static bool SetServerName(ref string value,
            string strUrl,
            string strServerName,
            bool bOverwrite = false)
        {
            List<OneUrl> urlList = JsonConvert.DeserializeObject<List<OneUrl>>(value);
            if (urlList == null)
                urlList = new List<OneUrl>();

            bool bChanged = false;
            foreach (OneUrl url in urlList)
            {
                if (url.Url == strUrl)
                {
                    if (string.IsNullOrEmpty(url.Name) == true || bOverwrite == true)
                    {
                        url.Name = strServerName;
                        bChanged = true;
                    }
                }
            }

            if (bChanged == true)
                value = JsonConvert.SerializeObject(urlList);

            return bChanged;
        }

        List<OneUrl> _urlList = new List<OneUrl>();

        public string UsedList
        {
            get
            {
                return JsonConvert.SerializeObject(_urlList);
            }
            set
            {
                this._urlList = JsonConvert.DeserializeObject<List<OneUrl>>(value);
                if (this._urlList == null)
                    this._urlList = new List<OneUrl>();

                FillList(this._urlList);
            }
        }

        // 填充组合框。组合框内显示 URL 列表
        void FillList(List<OneUrl> urlList)
        {
            this.comboBox_serverAddr.Items.Clear();
            foreach (OneUrl url in urlList)
            {
                this.comboBox_serverAddr.Items.Add(url.Url);    // url.Url
            }
        }

        // 将一个 URL 相关的数据设定到面板
        void SetPanel(string strURL)
        {
            // 清除面板
            this.ClearPanel();

            OneUrl url = FindUrl(strURL);
            if (url == null)
                return;

            this.comboBox_serverAddr.Text = url.Url;
            this.UiState = url.UiState;
        }

        // 将当前面板数据保存起来
        void SavePannel()
        {
            if (string.IsNullOrEmpty(this.comboBox_serverAddr.Text) == true)
                return;

            OneUrl url = FindUrl(this.comboBox_serverAddr.Text);
            if (url == null)
            {
                url = new OneUrl();
                url.Url = this.comboBox_serverAddr.Text;
                this._urlList.Add(url);
            }

            url.UiState = this.UiState;

            // TODO: 是否更新组合框列表显示?
        }

        void ClearPanel()
        {
            this.comboBox_serverAddr.Text = "";
            this.textBox_userName.Text = "";
            this.textBox_password.Text = "";
            this.checkBox_isReader.Checked = false;
            this.checkBox_savePasswordLong.Checked = false;
            this.checkBox_savePasswordShort.Checked = false;
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_userName);
                controls.Add(this.checkBox_isReader);
                SavePassword save = new SavePassword(this.textBox_password, this.checkBox_savePasswordLong);
                controls.Add(save);
                controls.Add(this.checkBox_savePasswordShort);
                controls.Add(this.textBox_phoneNumber);
                controls.Add(this.textBox_tempCode);    // 2017/4/11
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>();
                controls.Add(this.textBox_userName);
                controls.Add(this.checkBox_isReader);
                SavePassword save = new SavePassword(this.textBox_password, this.checkBox_savePasswordLong);
                controls.Add(save);
                controls.Add(this.checkBox_savePasswordShort);
                controls.Add(this.textBox_phoneNumber);
                controls.Add(this.textBox_tempCode);    // 2017/4/11
                GuiState.SetUiState(controls, value);
            }
        }

        OneUrl FindUrl(string strURL)
        {
            foreach (OneUrl url in this._urlList)
            {
                if (url.Url == strURL)
                    return url;
            }

            return null;
        }

        private void comboBox_serverAddr_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPanel(this.comboBox_serverAddr.Text);
        }

        private void comboBox_serverAddr_DrawItem(object sender, DrawItemEventArgs e)
        {
            using (Brush brushBack = new SolidBrush(e.BackColor))
            {
                e.Graphics.FillRectangle(brushBack, e.Bounds);
            }

            string strText = (string)this.comboBox_serverAddr.Items[e.Index];
            string strName = "";
            {
                OneUrl url = FindUrl(strText);
                if (url != null)
                    strName = url.Name;
            }

            int height = this.comboBox_serverAddr.Font.Height;
            // 绘制 URL 行
            using (Brush brush = new SolidBrush(ControlPaint.Light(e.ForeColor)))
            {
                Rectangle rect = new Rectangle(e.Bounds.X,
                    e.Bounds.Y,
                    e.Bounds.Width,
                    height);
                e.Graphics.DrawString(strText,
                    this.comboBox_serverAddr.Font,
                    brush,
                    rect);
            }

            if (string.IsNullOrEmpty(strName) == false)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // 绘制 名字 行
                using (Brush brush = new SolidBrush(e.ForeColor))
                using (Font font = new Font(this.comboBox_serverAddr.Font.FontFamily, this.comboBox_serverAddr.Font.Size * 1.5f, this.comboBox_serverAddr.Font.Unit))
                {
                    Rectangle rect = new Rectangle(e.Bounds.X,
                        e.Bounds.Y + height,
                        e.Bounds.Width,
                        height * 2);
                    e.Graphics.DrawString(strName,
                        font,
                        brush,
                        rect);
                }
            }

#if NO
            using(Pen pen = new Pen(ControlPaint.Dark(e.BackColor)))
            {
                Point pt1 = new Point(e.Bounds.X, e.Bounds.Y + e.Bounds.Height - 1);
                Point pt2 = new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y + e.Bounds.Height - 1);
                e.Graphics.DrawLine(pen, pt1, pt2);
            }
#endif
        }

        private void comboBox_serverAddr_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            string strText = (string)this.comboBox_serverAddr.Items[e.Index];
            OneUrl url = FindUrl(strText);

            if (url == null || string.IsNullOrEmpty(url.Name) == true)
                e.ItemHeight = this.comboBox_serverAddr.Font.Height;
            else
                e.ItemHeight = this.comboBox_serverAddr.Font.Height * 3;
        }

        private void toolStripButton_deleteFromList_Click(object sender, EventArgs e)
        {
            OneUrl url = this.FindUrl(this.comboBox_serverAddr.Text);
            if (url != null)
            {
                this._urlList.Remove(url);
                FillList(this._urlList);
            }

            if (this.comboBox_serverAddr.Items.Count == 0)
                this.comboBox_serverAddr.Text = "";
            else
                this.comboBox_serverAddr.Text = this.comboBox_serverAddr.Items[0] as string;
        }

        // 用于接收验证短信的手机号码
        public string PhoneNumber
        {
            get
            {
                return this.textBox_phoneNumber.Text;
            }
            set
            {
                this.textBox_phoneNumber.Text = value;
            }
        }

#if NO
        // 是否以验证码方式登录
        public bool SmsPassword
        {
            get
            {
                return this.checkBox_smsPassword.Checked;
            }
            set
            {
                this.checkBox_smsPassword.Checked = value;
            }
        }
#endif

        bool _retryLogin = false;
        public bool RetryLogin
        {
            get
            {
                return _retryLogin;
            }
            set
            {
                _retryLogin = value;
                TempCodeVisible = value;
            }
        }

        // 密码已经失效，需要在对话框打开时先打开 ChangePasswordDialog 以便进行密码修改操作
        public bool PasswordExpired { get; set; }

        // string _tempCode = "";

        // 验证码
        public string TempCode
        {
            get
            {
                return this.textBox_tempCode.Text;
            }
            set
            {
                this.textBox_tempCode.Text = value;
            }
        }

        bool _tempCodeVisible = false;
        public bool TempCodeVisible
        {
            get
            {
                return this._tempCodeVisible;
            }
            set
            {
                this._tempCodeVisible = value;
                this.label_tempCode.Visible = value;
                this.textBox_tempCode.Visible = value;
            }
        }

        bool _phoneNumberActivated = false;
        public void ActivatePhoneNumber()
        {
            int nOldWidth = this.textBox_phoneNumber.Width;
            int nOldHeight = this.textBox_phoneNumber.Height;

            // 将字体放大一倍
            this.textBox_phoneNumber.Font = new Font(this.Font.FontFamily, 
                this.Font.Size * 2);
            this.label_phoneNumber.Font = this.textBox_phoneNumber.Font;

            int nHeightDelta = this.textBox_phoneNumber.Height - nOldHeight;

            // 保持原有的宽度
            this.textBox_phoneNumber.Width = nOldWidth;

            _phoneNumberActivated = true;
#if NO
            // 向上移动一点
            this.textBox_phoneNumber.Location = new Point(this.textBox_phoneNumber.Location.X, this.textBox_phoneNumber.Location.Y - nHeightDelta);
            this.label_phoneNumber.Location = new Point(this.label_phoneNumber.Location.X, this.label_phoneNumber.Location.Y - nHeightDelta);
#endif
        }

        bool _tempCodeActivated = false;
        public void ActivateTempCode()
        {
            int nOldWidth = this.textBox_tempCode.Width;
            int nOldHeight = this.textBox_tempCode.Height;

            // 将字体放大一倍
            this.textBox_tempCode.Font = new Font(this.Font.FontFamily,
                this.Font.Size * 2);
            this.label_tempCode.Font = this.textBox_tempCode.Font;

            int nHeightDelta = this.textBox_tempCode.Height - nOldHeight;

            // 保持原有的宽度
            this.textBox_tempCode.Width = nOldWidth;

            _tempCodeActivated = true;
        }

        void SetFocus()
        {
            if (_phoneNumberActivated)
                this.textBox_phoneNumber.Focus();
            else if (_tempCodeActivated)
                this.textBox_tempCode.Focus();
        }

        private void toolStripButton_pasteFromJSONClipboard_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (Clipboard.ContainsText() == false)
            {
                strError = "Windows 剪贴板中没有文本";
                goto ERROR1;
            }

            try
            {
                string value = Clipboard.GetText();
                // Newtonsoft.Json.JsonReaderException
                var servers = JsonConvert.DeserializeObject<List<CopyServer>>(value);
                foreach (var source in servers)
                {
                    this.IsReader = false;
                    this.ServerUrl = source.Url;
                    this.UserName = source.UserName;
                    this.SavePasswordShort = source.SavePassword;
                    this.SavePasswordLong = this.SavePasswordShort;
                    this.Password = source.GetPassword();
                    break;
                }

                return;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                strError = "剪贴板中的内容不是特定格式";
                goto ERROR1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void toolStripButton_changePassword_Click(object sender, EventArgs e)
        {
            ChangePasswordDialog dlg = new ChangePasswordDialog();
            dlg.Font = this.Font;
            dlg.ServerUrl = this.ServerUrl;
            dlg.UserName = this.UserName;
            dlg.OldPassword = this.Password;
            dlg.IsReader = this.IsReader;
            dlg.StartPosition = FormStartPosition.CenterParent;
            dlg.ShowDialog(this);
            if (dlg.DialogResult == DialogResult.OK)
                this.textBox_password.Text = dlg.NewPassword;
        }
    }

    // 一个 URL 事项
    class OneUrl
    {
        public string Url = "";

        // 登录对话框对应于此 URL 时的状态，包含用户名和密码等
        public string UiState = "";

        // 图书馆名字 2016/10/30
        public string Name = "";
    }
}
