using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;

namespace dp2Manager
{
	/// <summary>
	/// Summary description for DatabaseDlg.
	/// </summary>
	public class DatabaseDlg : System.Windows.Forms.Form
	{
        public bool BatchMode = false;
        public DatabaseObject RefObject = null;

        // 临时变量
        string m_strTempUserName = null;

		public MainForm MainForm = null;

		bool m_bChanged = false;

		string OldDbName = "";
        
		public string Lang = "zh";

		public bool Changed
		{
			get 
			{
				if (m_bChanged == true)
					return true;
				if (this.treeView_objects.Log.Count != 0)
					return true;
				if (this.treeView_objects.Nodes.Count >= 1)
				{
					if (HasChangedRights(this.treeView_objects.Nodes[0]) == true)
						return true;
				}

				return false;
			}

			set 
			{
				m_bChanged = value;
			}
		}

		public bool IsCreate = false;

		public string ServerUrl = "";
		public string DbName = "";
		public string RefDbName = "";

		ArrayList m_aUserName = null;

		string m_strCurDatabaseObject = "";	// listview对应的当前已选择对象
		// Hashtable m_rightsChanged = new Hashtable();

		
		Hashtable m_tableUserRec = new Hashtable();	// 用户记录的缓存。元素为UserRec对象

		private System.Windows.Forms.TabControl tabControl_main;
		private System.Windows.Forms.TabPage tabPage_name;
		private System.Windows.Forms.TabPage tabPage_keysDef;
		private System.Windows.Forms.TabPage tabPage_browseDef;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader columnHeader_lang;
		private System.Windows.Forms.ColumnHeader columnHeader_value;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView listView_logicName;
		private System.Windows.Forms.TextBox textBox_sqlDbName;
		private System.Windows.Forms.TextBox textBox_sqlConnectionString;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox_keysDef;
		private System.Windows.Forms.TextBox textBox_browseDef;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBox_databaseType;
		private System.Windows.Forms.Button button_create;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_save;
		private System.Windows.Forms.Button button_delete;
		private System.Windows.Forms.TabPage tabPage_objects;
		private DatabaseObjectTree treeView_objects;
		private System.Windows.Forms.Panel panel_objectMain;
		private System.Windows.Forms.Splitter splitter_objectMain;
		private System.Windows.Forms.ListView listView_usersRights;
		private System.Windows.Forms.ColumnHeader columnHeader_userName;
        private System.Windows.Forms.ColumnHeader columnHeader_rights;
        private Button button_formatKeysXml;
        private Button button_formatBrowseXml;
        private IContainer components;

		public DatabaseDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseDlg));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_name = new System.Windows.Forms.TabPage();
            this.textBox_databaseType = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_sqlConnectionString = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_sqlDbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listView_logicName = new System.Windows.Forms.ListView();
            this.columnHeader_lang = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_value = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_keysDef = new System.Windows.Forms.TabPage();
            this.button_formatKeysXml = new System.Windows.Forms.Button();
            this.textBox_keysDef = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_browseDef = new System.Windows.Forms.TabPage();
            this.button_formatBrowseXml = new System.Windows.Forms.Button();
            this.textBox_browseDef = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage_objects = new System.Windows.Forms.TabPage();
            this.panel_objectMain = new System.Windows.Forms.Panel();
            this.listView_usersRights = new System.Windows.Forms.ListView();
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_rights = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitter_objectMain = new System.Windows.Forms.Splitter();
            this.button_create = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_save = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.treeView_objects = new DigitalPlatform.rms.Client.DatabaseObjectTree();
            this.tabControl_main.SuspendLayout();
            this.tabPage_name.SuspendLayout();
            this.tabPage_keysDef.SuspendLayout();
            this.tabPage_browseDef.SuspendLayout();
            this.tabPage_objects.SuspendLayout();
            this.panel_objectMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_name);
            this.tabControl_main.Controls.Add(this.tabPage_keysDef);
            this.tabControl_main.Controls.Add(this.tabPage_browseDef);
            this.tabControl_main.Controls.Add(this.tabPage_objects);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(546, 346);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_name
            // 
            this.tabPage_name.Controls.Add(this.textBox_databaseType);
            this.tabPage_name.Controls.Add(this.label6);
            this.tabPage_name.Controls.Add(this.textBox_sqlConnectionString);
            this.tabPage_name.Controls.Add(this.label3);
            this.tabPage_name.Controls.Add(this.textBox_sqlDbName);
            this.tabPage_name.Controls.Add(this.label2);
            this.tabPage_name.Controls.Add(this.label1);
            this.tabPage_name.Controls.Add(this.listView_logicName);
            this.tabPage_name.Location = new System.Drawing.Point(4, 22);
            this.tabPage_name.Name = "tabPage_name";
            this.tabPage_name.Size = new System.Drawing.Size(538, 320);
            this.tabPage_name.TabIndex = 0;
            this.tabPage_name.Text = "数据库名";
            this.tabPage_name.UseVisualStyleBackColor = true;
            // 
            // textBox_databaseType
            // 
            this.textBox_databaseType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_databaseType.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_databaseType.Location = new System.Drawing.Point(144, 230);
            this.textBox_databaseType.Name = "textBox_databaseType";
            this.textBox_databaseType.Size = new System.Drawing.Size(267, 21);
            this.textBox_databaseType.TabIndex = 7;
            this.textBox_databaseType.TextChanged += new System.EventHandler(this.textBox_databaseType_TextChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 233);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "数据库类型(&T):";
            // 
            // textBox_sqlConnectionString
            // 
            this.textBox_sqlConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlConnectionString.Enabled = false;
            this.textBox_sqlConnectionString.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sqlConnectionString.Location = new System.Drawing.Point(144, 262);
            this.textBox_sqlConnectionString.Name = "textBox_sqlConnectionString";
            this.textBox_sqlConnectionString.Size = new System.Drawing.Size(370, 21);
            this.textBox_sqlConnectionString.TabIndex = 5;
            this.textBox_sqlConnectionString.TextChanged += new System.EventHandler(this.textBox_sqlConnectionString_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 265);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "SQL Server连接(&C):";
            // 
            // textBox_sqlDbName
            // 
            this.textBox_sqlDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlDbName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sqlDbName.Location = new System.Drawing.Point(144, 286);
            this.textBox_sqlDbName.Name = "textBox_sqlDbName";
            this.textBox_sqlDbName.Size = new System.Drawing.Size(267, 21);
            this.textBox_sqlDbName.TabIndex = 3;
            this.textBox_sqlDbName.TextChanged += new System.EventHandler(this.textBox_sqlDbName_TextChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 289);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "SQL库名(&S):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "逻辑库名(&L):";
            // 
            // listView_logicName
            // 
            this.listView_logicName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_logicName.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_lang,
            this.columnHeader_value});
            this.listView_logicName.FullRowSelect = true;
            this.listView_logicName.HideSelection = false;
            this.listView_logicName.Location = new System.Drawing.Point(8, 29);
            this.listView_logicName.Name = "listView_logicName";
            this.listView_logicName.Size = new System.Drawing.Size(520, 197);
            this.listView_logicName.TabIndex = 0;
            this.listView_logicName.UseCompatibleStateImageBehavior = false;
            this.listView_logicName.View = System.Windows.Forms.View.Details;
            this.listView_logicName.SelectedIndexChanged += new System.EventHandler(this.listView_logicName_SelectedIndexChanged);
            this.listView_logicName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_logicName_MouseUp);
            // 
            // columnHeader_lang
            // 
            this.columnHeader_lang.Text = "语言代码";
            this.columnHeader_lang.Width = 78;
            // 
            // columnHeader_value
            // 
            this.columnHeader_value.Text = "值";
            this.columnHeader_value.Width = 174;
            // 
            // tabPage_keysDef
            // 
            this.tabPage_keysDef.Controls.Add(this.button_formatKeysXml);
            this.tabPage_keysDef.Controls.Add(this.textBox_keysDef);
            this.tabPage_keysDef.Controls.Add(this.label4);
            this.tabPage_keysDef.Location = new System.Drawing.Point(4, 22);
            this.tabPage_keysDef.Name = "tabPage_keysDef";
            this.tabPage_keysDef.Size = new System.Drawing.Size(538, 320);
            this.tabPage_keysDef.TabIndex = 1;
            this.tabPage_keysDef.Text = "检索点定义 -- keys";
            this.tabPage_keysDef.UseVisualStyleBackColor = true;
            // 
            // button_formatKeysXml
            // 
            this.button_formatKeysXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_formatKeysXml.Location = new System.Drawing.Point(8, 294);
            this.button_formatKeysXml.Name = "button_formatKeysXml";
            this.button_formatKeysXml.Size = new System.Drawing.Size(121, 23);
            this.button_formatKeysXml.TabIndex = 2;
            this.button_formatKeysXml.Text = "整理XML(&F)";
            this.button_formatKeysXml.UseVisualStyleBackColor = true;
            this.button_formatKeysXml.Click += new System.EventHandler(this.button_formatKeysXml_Click);
            // 
            // textBox_keysDef
            // 
            this.textBox_keysDef.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_keysDef.HideSelection = false;
            this.textBox_keysDef.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_keysDef.Location = new System.Drawing.Point(8, 32);
            this.textBox_keysDef.Multiline = true;
            this.textBox_keysDef.Name = "textBox_keysDef";
            this.textBox_keysDef.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_keysDef.Size = new System.Drawing.Size(527, 258);
            this.textBox_keysDef.TabIndex = 1;
            this.textBox_keysDef.TextChanged += new System.EventHandler(this.textBox_keysDef_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "检索点配置文件(keys)内容:";
            // 
            // tabPage_browseDef
            // 
            this.tabPage_browseDef.Controls.Add(this.button_formatBrowseXml);
            this.tabPage_browseDef.Controls.Add(this.textBox_browseDef);
            this.tabPage_browseDef.Controls.Add(this.label5);
            this.tabPage_browseDef.Location = new System.Drawing.Point(4, 22);
            this.tabPage_browseDef.Name = "tabPage_browseDef";
            this.tabPage_browseDef.Size = new System.Drawing.Size(538, 320);
            this.tabPage_browseDef.TabIndex = 2;
            this.tabPage_browseDef.Text = "浏览格式定义 -- browse";
            this.tabPage_browseDef.UseVisualStyleBackColor = true;
            // 
            // button_formatBrowseXml
            // 
            this.button_formatBrowseXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_formatBrowseXml.Location = new System.Drawing.Point(10, 294);
            this.button_formatBrowseXml.Name = "button_formatBrowseXml";
            this.button_formatBrowseXml.Size = new System.Drawing.Size(121, 23);
            this.button_formatBrowseXml.TabIndex = 4;
            this.button_formatBrowseXml.Text = "整理XML(&F)";
            this.button_formatBrowseXml.UseVisualStyleBackColor = true;
            this.button_formatBrowseXml.Click += new System.EventHandler(this.button_formatBrowseXml_Click);
            // 
            // textBox_browseDef
            // 
            this.textBox_browseDef.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_browseDef.HideSelection = false;
            this.textBox_browseDef.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_browseDef.Location = new System.Drawing.Point(10, 32);
            this.textBox_browseDef.Multiline = true;
            this.textBox_browseDef.Name = "textBox_browseDef";
            this.textBox_browseDef.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_browseDef.Size = new System.Drawing.Size(525, 258);
            this.textBox_browseDef.TabIndex = 3;
            this.textBox_browseDef.TextChanged += new System.EventHandler(this.textBox_browseDef_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(179, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "浏览格式配置文件(browse)内容:";
            // 
            // tabPage_objects
            // 
            this.tabPage_objects.Controls.Add(this.panel_objectMain);
            this.tabPage_objects.Location = new System.Drawing.Point(4, 22);
            this.tabPage_objects.Name = "tabPage_objects";
            this.tabPage_objects.Size = new System.Drawing.Size(538, 320);
            this.tabPage_objects.TabIndex = 3;
            this.tabPage_objects.Text = "对象";
            this.tabPage_objects.UseVisualStyleBackColor = true;
            // 
            // panel_objectMain
            // 
            this.panel_objectMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel_objectMain.Controls.Add(this.listView_usersRights);
            this.panel_objectMain.Controls.Add(this.splitter_objectMain);
            this.panel_objectMain.Controls.Add(this.treeView_objects);
            this.panel_objectMain.Location = new System.Drawing.Point(8, 8);
            this.panel_objectMain.Name = "panel_objectMain";
            this.panel_objectMain.Size = new System.Drawing.Size(622, 286);
            this.panel_objectMain.TabIndex = 1;
            // 
            // listView_usersRights
            // 
            this.listView_usersRights.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_userName,
            this.columnHeader_rights});
            this.listView_usersRights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_usersRights.FullRowSelect = true;
            this.listView_usersRights.HideSelection = false;
            this.listView_usersRights.Location = new System.Drawing.Point(224, 0);
            this.listView_usersRights.Name = "listView_usersRights";
            this.listView_usersRights.Size = new System.Drawing.Size(398, 286);
            this.listView_usersRights.TabIndex = 2;
            this.listView_usersRights.UseCompatibleStateImageBehavior = false;
            this.listView_usersRights.View = System.Windows.Forms.View.Details;
            this.listView_usersRights.DoubleClick += new System.EventHandler(this.listView_usersRights_DoubleClick);
            this.listView_usersRights.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_usersRights_MouseUp);
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 97;
            // 
            // columnHeader_rights
            // 
            this.columnHeader_rights.Text = "权限";
            this.columnHeader_rights.Width = 300;
            // 
            // splitter_objectMain
            // 
            this.splitter_objectMain.Location = new System.Drawing.Point(216, 0);
            this.splitter_objectMain.Name = "splitter_objectMain";
            this.splitter_objectMain.Size = new System.Drawing.Size(8, 286);
            this.splitter_objectMain.TabIndex = 1;
            this.splitter_objectMain.TabStop = false;
            // 
            // button_create
            // 
            this.button_create.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_create.Location = new System.Drawing.Point(12, 364);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(75, 24);
            this.button_create.TabIndex = 1;
            this.button_create.Text = "创建(&C)";
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(484, 364);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(74, 24);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "退出";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_save
            // 
            this.button_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_save.Location = new System.Drawing.Point(403, 364);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(75, 24);
            this.button_save.TabIndex = 3;
            this.button_save.Text = "保存(&S)";
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_delete.Location = new System.Drawing.Point(93, 364);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(76, 24);
            this.button_delete.TabIndex = 4;
            this.button_delete.Text = "删除(&D)";
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // treeView_objects
            // 
            this.treeView_objects.DbName = "";
            this.treeView_objects.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView_objects.HideSelection = false;
            this.treeView_objects.ImageIndex = 0;
            this.treeView_objects.Location = new System.Drawing.Point(0, 0);
            this.treeView_objects.Name = "treeView_objects";
            this.treeView_objects.SelectedImageIndex = 0;
            this.treeView_objects.Size = new System.Drawing.Size(216, 286);
            this.treeView_objects.TabIndex = 0;
            this.treeView_objects.OnSetMenu += new DigitalPlatform.GUI.GuiAppendMenuEventHandle(this.treeView_objects_OnSetMenu);
            this.treeView_objects.OnObjectDeleted += new DigitalPlatform.rms.Client.OnObjectDeletedEventHandle(this.treeView_objects_OnObjectDeleted);
            this.treeView_objects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_objects_AfterSelect);
            // 
            // DatabaseDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(570, 400);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.button_save);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DatabaseDlg";
            this.ShowInTaskbar = false;
            this.Text = "数据库管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.DatabaseDlg_Closing);
            this.Closed += new System.EventHandler(this.DatabaseDlg_Closed);
            this.Load += new System.EventHandler(this.DatabaseDlg_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_name.ResumeLayout(false);
            this.tabPage_name.PerformLayout();
            this.tabPage_keysDef.ResumeLayout(false);
            this.tabPage_keysDef.PerformLayout();
            this.tabPage_browseDef.ResumeLayout(false);
            this.tabPage_browseDef.PerformLayout();
            this.tabPage_objects.ResumeLayout(false);
            this.panel_objectMain.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		public void Initial(string strServerUrl,
			string strDbName)
		{
			this.ServerUrl = strServerUrl;
			this.DbName = strDbName;

			if (m_aUserName == null)
			{
				string strError = "";
				int nRet = GetAllUserName(out m_aUserName,
					out strError);
				if (nRet == -1)
				{
					MessageBox.Show(this, strError);
					return;
				}

				nRet = this.InitialUserRecordCache(out strError);
				if (nRet == -1)
				{
					MessageBox.Show(this, strError);
					return;
				}



			}
		}


		void SetButtonStates()
		{
			if (IsCreate == true)
			{
				this.button_create.Enabled = true;
				this.button_save.Enabled = false;
				this.button_delete.Enabled = false;
			}
			else 
			{
				this.button_create.Enabled = false;
				this.button_save.Enabled = true;
				this.button_delete.Enabled = true;
			}

		}

		private void DatabaseDlg_Load(object sender, System.EventArgs e)
		{

			SetButtonStates();

			string strError = "";
            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
			if (channel == null)
			{
				strError = "Channels.GetChannel 异常";
				goto ERROR1;
			}

			List<string[]> logicNames = null;
			string strType = "";
			string strSqlDbName = "";
			string strKeysDef = "";
			string strBrowseDef = "";

			string strDbName = this.DbName;

			if (strDbName == "")
				strDbName = this.RefDbName;

			long nRet = 0;

            if (strDbName != "")
            {
                nRet = channel.DoGetDBInfo(
                    strDbName,
                    "all",
                    out logicNames,
                    out strType,
                    out strSqlDbName,
                    out strKeysDef,
                    out strBrowseDef,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获取数据库 '"+strDbName+"' 的配置信息时发生错误: " + strError;
                    goto ERROR1;
                }

                if (this.DbName != "")
                    FillLogicNames(logicNames);

                if (this.DbName != "")
                    this.textBox_sqlDbName.Text = strSqlDbName;

                this.textBox_databaseType.Text = strType;

                string strOutXml = "";
                nRet = DomUtil.GetIndentXml(strKeysDef,
                    out strOutXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.textBox_keysDef.Text = strOutXml;
                nRet = DomUtil.GetIndentXml(strBrowseDef,
                    out strOutXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                this.textBox_browseDef.Text = strOutXml;

                if (this.IsCreate == false)
                {
                    this.treeView_objects.Initial(this.MainForm.Servers,
                        this.MainForm.Channels,
                        this.MainForm.stopManager,
                        this.ServerUrl,
                        strDbName);

                    if (this.listView_logicName.Items.Count >= 1)
                        this.OldDbName = this.listView_logicName.Items[0].SubItems[1].Text;
                    else
                        this.OldDbName = "";


                }
                else
                {
                    if (this.RefDbName != "")
                    {
                        this.treeView_objects.Initial(this.MainForm.Servers,
                            this.MainForm.Channels,
                            this.MainForm.stopManager,
                            this.ServerUrl,
                            this.RefDbName);
                        this.treeView_objects.DbName = "?";	// 避免误操作殃及所参考的库

                    }
                    else
                    {
                    }


                }

                if (this.treeView_objects.Nodes.Count >= 1)
                {
                    nRet = this.InitialObjRights(this.treeView_objects.Nodes[0],
                        out strError);
                    if (nRet == -1)
                    {
                        goto ERROR1;
                    }
                }


                this.Changed = false;
            }
            else
            {
                this.treeView_objects.Initial(this.MainForm.Servers,
                    this.MainForm.Channels,
                    this.MainForm.stopManager,
                    this.ServerUrl,
                    "");
                this.treeView_objects.DbName = "?";	// 避免误操作殃及所参考的库

                // 装入参考对象树
                if (this.RefObject != null)
                    this.treeView_objects.SetRootObject(this.RefObject);

                AfterLogicNameChanged();
            }
								 
			return;
		ERROR1:
		MessageBox.Show(strError);
		return;
		}


        public string SqlDbName
        {
            get
            {
                return this.textBox_sqlDbName.Text;
            }
            set
            {
                this.textBox_sqlDbName.Text = value;
            }
        }

        public string DatabaseType
        {
            get
            {
                return this.textBox_databaseType.Text;
            }
            set
            {
                this.textBox_databaseType.Text = value;
            }
        }

        public string KeysDef
        {
            get
            {
                return this.textBox_keysDef.Text;
            }
            set
            {
                this.textBox_keysDef.Text = value;
            }
        }

        public string BrowseDef
        {
            get
            {
                return this.textBox_browseDef.Text;
            }
            set
            {
                this.textBox_browseDef.Text = value;
            }
        }

        public List<string[]> LogicNames
        {
            get
            {
                List<string[]> names = null;
                string strError = "";
                int nRet = this.BuildLogicNames(true,
                    out names, out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
                return names;
            }
            set
            {
                FillLogicNames(value);
                AfterLogicNameChanged();

            }
        }

		private void DatabaseDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.Changed == true)
			{
				DialogResult result = MessageBox.Show(this,
					"当前对话框有修改内容尚未保存。确实要关闭对话框? (此时关闭所有修改内容将丢失)",
					"dp2manager",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, 
					MessageBoxDefaultButton.Button2);
				if (result != DialogResult.Yes)
				{
					e.Cancel = true;
					return;
				}
			}
		
		}

		private void DatabaseDlg_Closed(object sender, System.EventArgs e)
		{
		
		}

		void FillLogicNames(List<string[]> logicNames)
		{
			this.m_bChanged = true;

			this.listView_logicName.Items.Clear();

			for(int i=0;i<logicNames.Count;i++)
			{
				string [] cols = (string [])logicNames[i];

				ListViewItem item = new ListViewItem(cols[1], 0);
				item.SubItems.Add(cols[0]);
				listView_logicName.Items.Add(item);
			}
		}

		private void listView_logicName_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			bool bSelected = false;

			if (this.listView_logicName.SelectedItems.Count != 0)
				bSelected = true;


			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			menuItem = new MenuItem("修改(&M)");
			menuItem.Click += new System.EventHandler(this.menu_editLogicName_Click);
			menuItem.Enabled = bSelected;
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("快速修改(&Q)");
			menuItem.Click += new System.EventHandler(this.menu_globalEditLogicName_Click);
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("新增(&N)");
			menuItem.Click += new System.EventHandler(this.menu_newLogicName_Click);
			contextMenu.MenuItems.Add(menuItem);

			menuItem = new MenuItem("-");
			contextMenu.MenuItems.Add(menuItem);


			menuItem = new MenuItem("删除(&D)");
			menuItem.Click += new System.EventHandler(this.menu_deleteLogicName_Click);
			menuItem.Enabled = bSelected;
			contextMenu.MenuItems.Add(menuItem);

			contextMenu.Show(this.listView_logicName, new Point(e.X, e.Y) );		

		}

		// 逻辑库名list内容修改后，应作的联动操作
		void AfterLogicNameChanged()
		{
			string strDbName = "";
			if (this.listView_logicName.Items.Count == 0)
				strDbName = "?";
			else 
			{
				// 选第一行的名字
				strDbName = this.listView_logicName.Items[0].SubItems[1].Text;
			}


			string strOldDbName = this.treeView_objects.DbName;

			if (this.treeView_objects.DbName == strDbName)
				return;	// 实际上没有发生修改

			CollectChangedRights();
			this.m_strCurDatabaseObject = "";

			this.treeView_objects.DbName = strDbName;

			// 修改用户记录中的对应数据库权限定义节点的name参数值
			PutChangedDatabaseNameToUserRec(
                strOldDbName,
                strDbName);

			/* // 新方法后, 不用这样复杂的步骤了
			ArrayList aPath = new ArrayList();

			// 修改hashtable中全部名字事项
			foreach(string strDatabaseObject in m_rightsChanged.Keys)
			{
				aPath.Add(strDatabaseObject);
			}

			foreach(string strDatabaseObject in aPath)
			{
				Hashtable table = (Hashtable)m_rightsChanged[strDatabaseObject];

				if (strDatabaseObject == "")
					continue;

				// 先移出
				m_rightsChanged.Remove(strDatabaseObject);

				string strNewPath = strDatabaseObject;

				string strFirstPart = StringUtil.GetFirstPartPath(ref strNewPath);
				if (strNewPath != "")
					strNewPath = strDbName + "/" + strNewPath;
				else
					strNewPath = strDbName;

				// 以新名字加入
				m_rightsChanged[strNewPath] = table;
			}
			*/

		}
		
		// 快速修改所有逻辑库名
		private void menu_globalEditLogicName_Click(object sender, System.EventArgs e)
		{
			GlobalEditLogicNamesDlg dlg = new GlobalEditLogicNamesDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			List<string[]> names = null;
			string strError = "";
			int nRet = this.BuildLogicNames(false,
				out names, out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

			dlg.LogicNames = names;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			this.FillLogicNames(dlg.LogicNames);

			AfterLogicNameChanged();
		}


		// 编辑逻辑名
		private void menu_editLogicName_Click(object sender, System.EventArgs e)
		{
			if (this.listView_logicName.SelectedItems.Count == 0)
			{
				MessageBox.Show("尚未选择要编辑的事项...");
				return;
			}

			OneLogicNameDlg dlg = new OneLogicNameDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            dlg.textBox_lang.Text = this.listView_logicName.SelectedItems[0].Text;
			dlg.textBox_value.Text = this.listView_logicName.SelectedItems[0].SubItems[1].Text;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			this.listView_logicName.SelectedItems[0].Text = dlg.textBox_lang.Text;
			this.listView_logicName.SelectedItems[0].SubItems[1].Text = dlg.textBox_value.Text;

			this.Changed = true;

			AfterLogicNameChanged();
		}

		// 新增逻辑名
		private void menu_newLogicName_Click(object sender, System.EventArgs e)
		{
			OneLogicNameDlg dlg = new OneLogicNameDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			ListViewItem item = new ListViewItem(dlg.textBox_lang.Text, 0);
			item.SubItems.Add(dlg.textBox_value.Text);
			listView_logicName.Items.Add(item);

			this.Changed = true;

			AfterLogicNameChanged();
		}

		// 删除逻辑名
		private void menu_deleteLogicName_Click(object sender, System.EventArgs e)
		{
			if (this.listView_logicName.SelectedItems.Count == 0)
			{
				MessageBox.Show("尚未选择要删除的事项...");
				return;
			}

			// 删除listview中事项
			for(int i=listView_logicName.SelectedIndices.Count-1;i>=0;i--)
			{
				listView_logicName.Items.RemoveAt(listView_logicName.SelectedIndices[i]);
			}

			this.Changed = true;

			AfterLogicNameChanged();
		}

		// 检查创建参数
		int CheckCreateDbParams(out string strError)
		{
			strError = "";

			// 逻辑库名列表是否为空?
			if (this.listView_logicName.Items.Count == 0)
			{
				strError = "逻辑库名尚未设置...";
				return -1;
			}

			// 是否至少定义了zh和en两种语言的逻辑库名?
			bool bHasZh = false;
			bool bHasEn = false;
			for(int i=0;i<this.listView_logicName.Items.Count;i++)
			{
				string strLang = listView_logicName.Items[i].Text;

				if (strLang.Length < 2) 
				{
					strError = "逻辑库名中语言代码 '" + strLang + "' 不正确，因为2字符以上";
					return -1;
				}

				strLang = strLang.Substring(0, 2);
				if (strLang == "zh")
					bHasZh = true;
				if (strLang == "en")
					bHasEn = true;

			}

			if (bHasZh == false)
			{
				strError = "逻辑库名中必须至少包含一个语言代码为zh的名字事项";
				return -1;
			}

			if (bHasEn == false)
			{
				strError = "逻辑库名中必须至少包含一个语言代码为en的名字事项";
				return -1;
			}

			// 检查检索点定义

			string strOutXml = "";
			int nRet = DomUtil.GetIndentXml(this.textBox_keysDef.Text,
				out strOutXml,
				out strError);
			if (nRet == -1)
			{
				strError = "检索点定义内容格式有错: " + strError;
				return -1;
			}

			// 检查浏览格式定义

			nRet = DomUtil.GetIndentXml(this.textBox_browseDef.Text,
				out strOutXml,
				out strError);
			if (nRet == -1)
			{
				strError = "浏览格式定义内容格式有错: " + strError;
				return -1;
			}

			return 0;
		}

		int BuildLogicNames(
			bool bCheck,
			out List<string[]> result,
			out string strError)
		{
			strError = "";
			result = new List<string[]>();
			for(int i=0;i<this.listView_logicName.Items.Count;i++)
			{
				if (bCheck == true 
					&& this.listView_logicName.Items[i].Text == "")
				{
					strError = "逻辑库名列表中行 " + Convert.ToString(i+1) + "语言代码不应为空...";
					return -1;
				}
				if (bCheck == true
					&& this.listView_logicName.Items[i].SubItems.Count < 2)
				{
					strError = "逻辑库名列表中行 " + Convert.ToString(i+1) + "值不应为空...";
					return -1;
				}
				if (bCheck == true
					&& this.listView_logicName.Items[i].SubItems[1].Text == "")
				{
					strError = "逻辑库名列表中行 " + Convert.ToString(i+1) + "值不应为空...";
					return -1;
				}
				string [] cols = new string[2];
				if (this.listView_logicName.Items[i].SubItems.Count == 2)
					cols[0] = this.listView_logicName.Items[i].SubItems[1].Text;
				else
					cols[0] = "";

				cols[1] = this.listView_logicName.Items[i].Text;
				result.Add(cols);
			}

			return 0;
		}

		private void button_create_Click(object sender, System.EventArgs e)
		{
            this.Enabled = false;
            try
            {
                string strError = "";

                long lRet = CheckCreateDbParams(out strError);
                if (lRet == -1)
                    goto ERROR1;

                RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
                if (channel == null)
                {
                    strError = "Channels.GetChannel 异常";
                    goto ERROR1;
                }

                List<string[]> logicNames = new List<string[]>();

                lRet = BuildLogicNames(true,	// 带有检查功能
                    out logicNames,
                    out strError);
                if (lRet == -1)
                    goto ERROR1;

                lRet = channel.DoCreateDB(logicNames,
                    this.textBox_databaseType.Text,
                    this.textBox_sqlDbName.Text,
                    this.textBox_keysDef.Text,
                    this.textBox_browseDef.Text,
                    out strError);
                if (lRet == -1)
                {
                    strError = "创建数据库出错: " + strError;
                    goto ERROR1;
                }

                // 清除原有日志
                this.treeView_objects.ClearLog();

                // 将树上全部对象以特定类型加入日志中
                this.treeView_objects.PutAllObjToLog(ObjEventOper.New,
                    this.treeView_objects.Root);

                int nRet = this.treeView_objects.SubmitLog(out strError);
                if (nRet == -1)
                    goto ERROR1;
                /*
                // 创建下属对象
                int nRet = this.treeView_objects.BuildRealObjects(
                    this.listView_logicName.Items[0].SubItems[1].Text,
                    this.treeView_objects.Root,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                */


                /// 为账户库添加权限
                nRet = this.SaveChangedUserRec(out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                this.DbName = this.listView_logicName.Items[0].SubItems[1].Text;

                // 初始化数据库
                nRet = InitalDB(this.DbName,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, "数据库 '" + this.DbName + "' 初始化失败：" + strError + "\r\n\r\n(尽管数据库初始化失败，但已创建成功。请排除故障后重新初始化)");
                }

                MessageBox.Show(this, "数据库创建成功");

                // 重新获取刚创建的信息

                this.IsCreate = false;
                DatabaseDlg_Load(null, null);

                this.MainForm.menuItem_refresh_Click(null, null);	// 刷新显示


                /*
                // 为所有帐户增加相应的权限
                ChangeUsersRightsDlg dlg = new ChangeUsersRightsDlg();

                dlg.Initial(this.MainForm.Servers,
                    this.MainForm.Channels,
                    this.MainForm.stopManager,
                    this.ServerUrl,
                    this.DbName);
                */

                //this.DialogResult = DialogResult.Yes;
                //this.Close();

                if (BatchMode == true)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }

                return;
            ERROR1:
                MessageBox.Show(this, strError);
                return;
            }
            finally
            {
                this.Enabled = true;
            }
		}

		int InitalDB(string strDbName,
			out string strError)
		{
			strError = "";

            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);

			Debug.Assert(channel != null, "Channels.GetChannel() 异常");

			long lRet = channel.DoInitialDB(strDbName, out strError);

			if (lRet == -1) 
				return -1;

			return 0;
		}

        /*
        // 2008/11/14
        int RefreshDB(string strDbName,
    out string strError)
        {
            strError = "";

            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            long lRet = channel.DoRefreshDB(strDbName, out strError);

            if (lRet == -1)
                return -1;

            return 0;
        }*/

		private void button_save_Click(object sender, System.EventArgs e)
		{
			string strError = "";

			strError = "";

            List<string[]> logicNames = new List<string[]>();

			long lRet = BuildLogicNames(true,	// 带有检查功能
				out logicNames,
				out strError);
			if (lRet == -1)
				goto ERROR1;


            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
			if (channel == null)
			{
				strError = "Channels.GetChannel 异常";
				goto ERROR1;
			}

			lRet = channel.DoSetDBInfo(
				this.OldDbName,
				logicNames,
				this.textBox_databaseType.Text,
				this.textBox_sqlDbName.Text,
				this.textBox_keysDef.Text,
				this.textBox_browseDef.Text,
				out strError);
			if (lRet == -1)
				goto ERROR1;

			if (this.OldDbName != this.listView_logicName.Items[0].SubItems[1].Text)
			{
				// 发生过数据库改名
			}

			int nRet = this.treeView_objects.SubmitLog(out strError);
			if (nRet == -1)
			{
				goto ERROR1;
			}

			nRet = this.SaveChangedUserRec(out strError);
			if (nRet == -1)
			{
				goto ERROR1;
			}

			this.m_bChanged = false;

			this.MainForm.menuItem_refresh_Click(null, null);	// 刷新显示

			this.DialogResult = DialogResult.OK;
			this.Close();
			return;
			ERROR1:
				MessageBox.Show(this, strError);
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void button_delete_Click(object sender, System.EventArgs e)
		{
			string strError = "";

			string strDbName = this.listView_logicName.Items[0].SubItems[1].Text;

			//
			DialogResult result = MessageBox.Show(this,
				"确实要删除位于 " +this.ServerUrl + "\r\n的数据库 '" + strDbName + "' ?\r\n\r\n***警告：数据库一旦删除，就无法恢复。",
				"dp2manager",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, 
				MessageBoxDefaultButton.Button2);
			if (result != DialogResult.Yes)
				return;

			int nRet = DeleteDb(strDbName, out strError);
			if (nRet == -1)
				goto ERROR1;

			// 从账户库中删除对应的权限?
			nRet = RemoveObjectFromCache(strDbName,
				out strError);
			if (nRet == -1)
				goto ERROR1;
			nRet = SaveUserRecs(out strError);
			if (nRet == -1)
				goto ERROR1;


			MessageBox.Show(this, "数据库删除成功。");

			// 修改状态，为立即重新创建做好准备
			this.IsCreate = true;
			this.DbName = "";
			this.RefDbName = "";

			// 重新设置按钮状态
			SetButtonStates();


			this.MainForm.menuItem_refresh_Click(null, null);	// 刷新显示

			return;
			ERROR1:
				MessageBox.Show(this, strError);
			return;

		}

		public int DeleteDb(string strDbName,
			out string strError)
		{
			strError = "";
            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
			if (channel == null)
			{
				strError = "Channels.GetChannel 异常";
				return -1;
			}

			long lRet = channel.DoDeleteDB(strDbName, out strError);

			if (lRet == -1)
				return -1;

			return 0;
		}

		// 选择了左边的树上的对象
		private void treeView_objects_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{

			Debug.Assert(m_aUserName != null, "");

			FillUsersRights(m_aUserName,
				TreeViewUtil.GetPath(this.treeView_objects.SelectedNode, '/'));
		}

		// 填充listview
		public int GetAllUserName(
			out ArrayList aUserName,
			out string strError)
		{
			strError = "";
			aUserName = new ArrayList();

			string strQueryXml = "<target list='" + Defs.DefaultUserDb.Name
                + ":" + "__id'><item><word>"
				+ "" + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>chi</lang></target>";

            RmsChannel channel = this.MainForm.Channels.GetChannel(this.ServerUrl);
			if (channel == null)
			{
				strError = "Channels.GetChannel 异常";
				return -1;
			}

            long nRet = channel.DoSearch(strQueryXml,
                "default",
                out strError);
			if (nRet == -1) 
			{
				strError = "检索帐户库时出错: " + strError;
				return -1;
			}

			if (nRet == 0)
				return 0;	// not found

			long lTotalCount = nRet;	// 总命中数
			long lThisCount = lTotalCount;
			long lStart = 0;

			for(;;)
			{

				ArrayList aLine = null;
				nRet = channel.DoGetSearchFullResult(
                    "default",
					lStart,
					lThisCount,
					this.Lang,
					null,	// stop,
					out aLine,
					out strError);
				if (nRet == -1) 
				{
					strError = "检索注册用户库获取检索结果时出错: " + strError;
					return -1;
				}

				for(int i=0;i<aLine.Count;i++)
				{
					string[] acol = (string[])aLine[i];
					if (acol.Length < 1)
						continue;
					if (acol.Length < 2)
					{
						// 列中没有用户名, 用获取记录来补救?
					}

					aUserName.Add(acol[1]);
				}

				if (lStart + aLine.Count >= lTotalCount)
					break;

				lStart += aLine.Count;
				lThisCount -= aLine.Count;
			}

			return 0;
		}

		// 把权限修改信息放入树对象所附的信息对象中
		void SetRights(string strDatabaseObject,
			string strUserName,
			string strRights)
		{

			TreeNode node = TreeViewUtil.GetTreeNode(this.treeView_objects,
				strDatabaseObject);
			if (node == null)
			{
				Debug.Assert(false, "");
				return;
			}

			ObjRight objright = (ObjRight)node.Tag;
			if (objright == null)
			{
				objright = new ObjRight();
				node.Tag = objright;
			}

			objright.SetRights(strUserName,
				strRights);
		}


		// 对于当前右侧listview, 收集修改过的权限值
		int CollectChangedRights()
		{
			if (m_strCurDatabaseObject == "")
				return 0;

			TreeNode node = TreeViewUtil.GetTreeNode(this.treeView_objects,
				m_strCurDatabaseObject);
			if (node == null)
				return -1;

			ObjRight objright = (ObjRight)node.Tag;
			if (objright == null)
			{
				objright = new ObjRight();
				node.Tag = objright;
			}


			// 观察listview中是否有修改过的事项
			for(int i=0;i<this.listView_usersRights.Items.Count;i++)
			{
				string strUserName = this.listView_usersRights.Items[i].Text;
				string strRights = this.listView_usersRights.Items[i].SubItems[1].Text;

				// 找到旧rights
				string strOldRights = objright.GetRights(strUserName);

				if (strOldRights != strRights)
					objright.SetRights(strUserName, strRights);
			}

			return 0;
		}

		// 填充右侧listview
		// parameters:
		//		strDatabaseObject	数据库对象路径
		public int FillUsersRights(ArrayList aUserName,
			string strDatabaseObject)
		{
			if (m_strCurDatabaseObject == strDatabaseObject)
				return 0;	// 没有必要做填充

			// 收集已经修改的事情
			CollectChangedRights();


			listView_usersRights.Items.Clear();

			TreeNode node = this.treeView_objects.SelectedNode;
			if (node == null)
				return -1;

			ObjRight objright = (ObjRight)node.Tag;
			if (objright == null)
				return -1;

			for(int i=0;i<objright.RightLines.Count;i++)
			{
				RightLine line = (RightLine)objright.RightLines[i];

				ListViewItem item = new ListViewItem(line.UserName, 0);
				this.listView_usersRights.Items.Add(item);

				item.SubItems.Add(line.Rights);
				if (line.Changed == true)
					item.ForeColor = Color.Red;
			}

			m_strCurDatabaseObject = strDatabaseObject;
			return 0;
		}

		// 初始化,装入全部用户的xml记录
		int InitialUserRecordCache(out string strError)
		{
			strError = "";

			Debug.Assert(this.ServerUrl != "", "");

			for(int i=0;i<this.m_aUserName.Count;i++)
			{
				string strUserName = (string)this.m_aUserName[i];

				Debug.Assert(strUserName != "" && strUserName != null, "");

				// 先从cache中找
				UserRec rec = (UserRec)this.m_tableUserRec[strUserName];
				if (rec == null)
				{

					string strXml = "";
					string strUserRecPath = "";
					byte [] baTimeStamp = null;
					// 获得帐户记录
					int nRet = MainForm.GetUserRecord(
						this.ServerUrl,
						strUserName,
                        out strUserRecPath,
						out strXml,
						out baTimeStamp,
						out strError);
					if (nRet == -1)
					{
						strError = "获取用户 '" + strUserName + "' 的帐户记录时出错 : " + strError;
						return -1;
					}

					rec = new UserRec();
					rec.Xml = strXml;
                    rec.RecPath = strUserRecPath;
					rec.TimeStamp = baTimeStamp;

					this.m_tableUserRec[strUserName] = rec;
				}

			}

			return 0;
		}

		// 获得一个账户记录中，针对特定数据库对象的权限
		int GetObjectRights(
			string strServerUrl,
			string strUserName,
			string strDatabaseObject,
			out string strRights,
			out string strError)
		{
			strRights = "";
			strError = "";

			// 先从cache中找
			UserRec rec = (UserRec)this.m_tableUserRec[strUserName];
			if (rec == null)
			{
				// 如果初始化过,这不应该发生
				Debug.Assert(false, "");
				strError = "用户 '" + strUserName + "' 的UserRec对象在cache中没有找到...";
				return -1;


			}

			XmlDocument dom = new XmlDocument();
			try
			{
				dom.LoadXml(rec.Xml);
			}
			catch (Exception ex)
			{
				strError = "用户"+strUserName+"记录XML装载到dom时出错: " + ex.Message;
				return -1;
			}

			string strPath = GetXPath(strDatabaseObject);

			XmlNode node = dom.DocumentElement.SelectSingleNode(strPath);
			if (node != null) 
			{
				strRights = DomUtil.GetAttr(node, "rights");
				return 1;
			}

			return 0;
		}

        string GetXPath(string strDatabaseObject)
        {
            return GetXPath(strDatabaseObject, -1);
        }

		// 获得定位数据库对象的xpath
        // parameters:
        //      nLeafType   最末尾一级的对象类型，-1表示不清楚对象类型
		string GetXPath(string strDatabaseObject,
            int nLeafType)
		{
			string[] aName = strDatabaseObject.Split(new Char [] {'/'});
			string strPath = "server";  // rightsItem

			for(int i=0;i<aName.Length;i++)
			{
				if (strPath != "")
					strPath += "/";
                if (i == aName.Length - 1 && nLeafType != -1)
                {
                    string strElementName = "*";
                    if (nLeafType == ResTree.RESTYPE_FILE)
                        strElementName = "file";
                    else if (nLeafType == ResTree.RESTYPE_FOLDER)
                        strElementName = "dir";
                    else if (nLeafType == ResTree.RESTYPE_DB)
                        strElementName = "database";
                    else if (nLeafType == ResTree.RESTYPE_SERVER)
                        strElementName = "server";


                    strPath += strElementName + "[@name='" + aName[i] + "']";
                }
                else
                    strPath += "*[@name='" + aName[i] + "']";
			}

			return strPath;
		}

		private void listView_usersRights_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Right)
				return;

			ContextMenu contextMenu = new ContextMenu();
			MenuItem menuItem = null;

			menuItem = new MenuItem("权限(&R)");
			menuItem.Click += new System.EventHandler(this.menu_editRights_Click);
			if (this.listView_usersRights.SelectedItems.Count == 0)
				menuItem.Enabled = false;
			contextMenu.MenuItems.Add(menuItem);

			contextMenu.Show(this.listView_usersRights, new Point(e.X, e.Y) );		
	
		}

		// 编辑权限
		private void menu_editRights_Click(object sender, System.EventArgs e)
		{
			if (listView_usersRights.SelectedItems.Count == 0)
			{
				MessageBox.Show("尚未选择要编辑的事项...");
				return;
			}

            DigitalPlatform.CommonDialog.CategoryPropertyDlg dlg = new DigitalPlatform.CommonDialog.CategoryPropertyDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			string strFirstRights = listView_usersRights.SelectedItems[0].SubItems[1].Text;

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.Text = "用户 '" + listView_usersRights.SelectedItems[0].Text +"' 针对对象 '"+ this.treeView_objects.SelectedNode.Text +"' 的权限";
			dlg.PropertyString = strFirstRights;
			dlg.CfgFileName = "userrightsdef.xml";
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			for(int i=0;i<listView_usersRights.SelectedItems.Count;i++)
			{
				// 令显示正确
				listView_usersRights.SelectedItems[i].SubItems[1].Text = dlg.PropertyString;

				// 令内存正确
				listView_usersRights.SelectedItems[i].Tag = true;	// 表示曾经修改过

				listView_usersRights.SelectedItems[i].ForeColor = Color.Red;	// ControlPaint.LightLight(nodeNew.ForeColor);

				// nodeinfo.TreeNode.ForeColor = this.SelectedItems[i].ForeColor;
			}
		}


		// 从所有缓存的用户记录中删除特定对象的权限节点
		int RemoveObjectFromCache(string strDatabaseObject,
			out string strError)
		{
			strError = "";
			/*
			Hashtable table = (Hashtable)m_rightsChanged[strDatabaseObject];
			if (table == null)
				return 0;	// 没有找到
			*/

			foreach(string strUserName in this.m_tableUserRec.Keys)
			{
				// string strRights = (string)table[strUserName];

				// 把权限兑现到xml字符串中

				UserRec rec = (UserRec)this.m_tableUserRec[strUserName];
				XmlDocument dom = new XmlDocument();
				dom.LoadXml(rec.Xml);

				string xpath = GetXPath(strDatabaseObject);
				XmlNode node = dom.DocumentElement.SelectSingleNode(xpath);
				if (node != null)
				{
					//DomUtil.SetAttr(node, "rights", "");
					node.ParentNode.RemoveChild(node);	 // 删除这个权限定义节点
					rec.Changed = true;
					rec.Xml = dom.OuterXml;
				}
			}

			return 1;
		}

		// 看看有没有修改过、未保存的权限信息
		bool HasChangedRights(TreeNode parent)
		{
			ObjRight objright = (ObjRight)parent.Tag;
			if (objright == null)
				return false;

            string strDatabaseObject = TreeViewUtil.GetPath(parent, '/');

			int i;

			for(i=0;i<objright.RightLines.Count;i++)
			{
				RightLine line = (RightLine)objright.RightLines[i];
				if (line.Changed == true)
					return true;
			}

			// 递归
			for(i=0;i<parent.Nodes.Count;i++)
			{
				TreeNode child = parent.Nodes[i];

				bool bRet = HasChangedRights(child);
				if (bRet == true)
					return true;
			}

			return false;
		}

		int PutChangedDatabaseNameToUserRec(
			string strOldDbName,
			string strNewDbName)
		{

			string strDatabaseObject = strOldDbName;

			int i;

			for(i=0;i<this.m_aUserName.Count;i++)
			{
				string strUserName = (string)this.m_aUserName[i];

				UserRec rec = (UserRec)this.m_tableUserRec[strUserName];
				XmlDocument dom = new XmlDocument();
				dom.LoadXml(rec.Xml);

				string xpath = GetXPath(strDatabaseObject,
                    ResTree.RESTYPE_DB);
				XmlNode node = dom.DocumentElement.SelectSingleNode(xpath);
				if (node == null)
					continue;

				DomUtil.SetAttr(node, "name", strNewDbName);
				rec.Changed = true;
				rec.Xml = dom.OuterXml;

			}

			return 0;
		}

		// 把对象节点中所附加的权限信息收集到用户记录集合中
		int PutChangedRightsToUserRec(TreeNode parent,
			bool bClearChanged)
		{
			ObjRight objright = (ObjRight)parent.Tag;
			if (objright == null)
				return 0;

            string strDatabaseObject = TreeViewUtil.GetPath(parent, '/');

			int i;

			for(i=0;i<objright.RightLines.Count;i++)
			{
				RightLine line = (RightLine)objright.RightLines[i];
				if (line.Changed == false)
					continue;

				string strRights = line.Rights;

				UserRec rec = (UserRec)this.m_tableUserRec[line.UserName];
				XmlDocument dom = new XmlDocument();
				dom.LoadXml(rec.Xml);

				string xpath = GetXPath(strDatabaseObject);
				XmlNode node = dom.DocumentElement.SelectSingleNode(xpath);
				if (strRights != "" && strRights != null)
				{
					// 创建节点
					node = CreateRightsNode(dom,
						strDatabaseObject,
						GetObjectType(strDatabaseObject));
					rec.Changed = true;
				}

				if (node != null)
				{
					DomUtil.SetAttr(node, "rights", strRights);
					rec.Changed = true;
				}

				rec.Xml = dom.OuterXml;

				if (bClearChanged == true)
					line.Changed = false;
			}

			// 递归
			for(i=0;i<parent.Nodes.Count;i++)
			{
				TreeNode child = parent.Nodes[i];

				int nRet = PutChangedRightsToUserRec(child, bClearChanged);
				if (nRet == -1)
					return -1;
			}

			return 0;
		}

		// 把hashtable中记录的权限修改汇总到用户记录cache中,并保存
		int SaveChangedUserRec(out string strError)
		{
            strError = "";

            if (this.treeView_objects.Nodes.Count == 0)
                return 0;

			// 收集已经修改的事情
			CollectChangedRights();

			m_strCurDatabaseObject = "";



			// 把权限兑现到xml字符串中
			int nRet = PutChangedRightsToUserRec(this.treeView_objects.Nodes[0],
				true);	// 执行后自动清除了所有修改标记
			if (nRet == -1)
			{
				Debug.Assert(false, "");
				return -1;
			}

			nRet = SaveUserRecs(out strError);
			if (nRet == -1)
				return -1;
		

			return 0;
		}

		// 将用户记录cache加以保存
		int SaveUserRecs(out string strError)
		{
			strError = "";

            RmsChannel channel = MainForm.Channels.GetChannel(this.ServerUrl);
			if (channel == null)
			{
				strError = "Channels.GetChannel 异常";
				return -1;
			}

            if (this.m_tableUserRec.Count == 0)
                return 0;   // 2006/7/4 add

			Debug.Assert(this.m_tableUserRec.Count != 0, "必须先初始化uerrec缓冲");

			// 保存记录
			foreach(string strUserName in this.m_tableUserRec.Keys)
			{
				UserRec rec = (UserRec)this.m_tableUserRec[strUserName];

				if (rec.Changed == false)
					continue;


				// 保存
				string strXml = rec.Xml;
				string strOutputPath = "";
				byte [] baOutputTimeStamp;

                long lRet = channel.DoSaveTextRes(/*Defs.DefaultUserDb.Name+ "/" + */
                    rec.RecPath,
					strXml,
					false,	// bInlucdePreamble
					"",	// style
					rec.TimeStamp,	// baTimeStamp,
					out baOutputTimeStamp,
					out strOutputPath,
					out strError);
				if (lRet == -1)
				{
					if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
						rec.TimeStamp = baOutputTimeStamp;	// 为以后的再次保存提供方便

					return-1;
				}
				rec.TimeStamp = baOutputTimeStamp;	// 为以后的再次保存提供方便

				rec.Changed = false;	// 清除修改标记
			}

			return 0;
		}

		int GetObjectType(string strPath)
		{
			// 从路径得到树对象的imageindex
			TreeNode node = TreeViewUtil.GetTreeNode(this.treeView_objects, 
				strPath);
			if (node == null)
				return -1;
			return node.ImageIndex;
		}

		// 逐级创建，直到创建好所需的权限节点
		XmlNode CreateRightsNode(XmlDocument dom,
			string strDatabaseObject,
			int nType)
		{
			// <server> 节点
			XmlNode nodeRoot = dom.DocumentElement.SelectSingleNode("server");  // rightsItem
			if (nodeRoot == null)
			{
				nodeRoot = dom.CreateElement("server"); // rightsItem
				nodeRoot = dom.DocumentElement.AppendChild(nodeRoot);
			}

			string[] aName = strDatabaseObject.Split(new Char [] {'/'});

			XmlNode parent = nodeRoot;
			for(int i=0;i<aName.Length;i++)
			{
                string strCurName = aName[i];
				string strElementName = "";

				if (i==0)	// 创建数据库节点
				{
					strElementName = "database";
				}
				else if (i != aName.Length - 1)	// 中间
				{
					strElementName = "dir";
				}
				else 
				{
					if (nType == ResTree.RESTYPE_FILE)
						strElementName = "file";
					else
					{
						Debug.Assert(nType == ResTree.RESTYPE_FOLDER, "意外的(不是folder的)节点类型");
						strElementName = "dir";
					}
				}

				string strXPath = strElementName + "[@name='" +strCurName+ "']";

				XmlNode nodeNew = parent.SelectSingleNode(strXPath);

				if (nodeNew == null)
				{
					nodeNew = dom.CreateElement(strElementName);
					nodeNew = parent.AppendChild(nodeNew);
					DomUtil.SetAttr(nodeNew, "name", strCurName);
					DomUtil.SetAttr(nodeNew, "rights", "");
				}

				parent = nodeNew;
			}

			return parent;
		}

		private void treeView_objects_OnSetMenu(object sender,
            DigitalPlatform.GUI.GuiAppendMenuEventArgs e)
		{
			Debug.Assert(e.ContextMenu != null, "e不能为null");

			MenuItem menuItem = new MenuItem("-");
			e.ContextMenu.MenuItems.Add(menuItem);

			TreeNode node = this.treeView_objects.SelectedNode;
			string strText = "权限(&R)";

			if (node == null || node.ImageIndex == ResTree.RESTYPE_DB)
				strText = "权限[数据库整体](&R)";
			else
				strText = "权限[对象'"+node.Text+"'](&R)";

			menuItem = new MenuItem(strText);
			menuItem.Click += new System.EventHandler(this.menu_quickSetRights_Click);

			e.ContextMenu.MenuItems.Add(menuItem);
		}

		void menu_quickSetRights_Click(object sender, EventArgs e)
		{
			/*
			CollectChangedRights();

			TreeNode oldSelected = this.treeView_objects.SelectedNode;
			this.treeView_objects.SelectedNode = null;
			*/
			CollectChangedRights();

			TreeNode node = this.treeView_objects.SelectedNode;
			if (node == null)
				node = this.treeView_objects.Nodes[0];

			QuickSetRightsDlg dlg = new QuickSetRightsDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.CfgFileName = "quickrights.xml";
			dlg.AllUserNames = new ArrayList();
			dlg.AllUserNames.AddRange(m_aUserName);

			dlg.SelectedUserNames = new ArrayList();
				for(int i=0;i<listView_usersRights.SelectedItems.Count;i++)
				{
					dlg.SelectedUserNames.Add(this.listView_usersRights.SelectedItems[i].Text);
				}

			this.MainForm.AppInfo.LinkFormState(dlg, "QuickSetRightsDlg_state");
			dlg.ShowDialog(this);
			this.MainForm.AppInfo.UnlinkFormState(dlg);

			if (dlg.DialogResult != DialogResult.OK)
				return;


			// 根 this.treeView_objects.Nodes[0]
			ModiRights(node,
				dlg.SelectedUserNames,
				dlg.QuickRights);

			/*
			this.treeView_objects.SelectedNode = oldSelected;
			*/

			m_strCurDatabaseObject = "";
			FillUsersRights(m_aUserName,
                TreeViewUtil.GetPath(this.treeView_objects.SelectedNode, '/'));

		}

  

		void ModiRights(TreeNode parent,
			ArrayList aUserName,
			QuickRights quickrights)
		{
			for(int i=0;i<aUserName.Count;i++)
			{
				string strUserName = (string)aUserName[i];

                this.m_strTempUserName = strUserName;

                List<TreeNode> nodes = new List<TreeNode>();
                nodes.Add(parent);

                quickrights.GetNodeStyle += new GetNodeStyleEventHandle(quickrights_GetNodeStyle);
                quickrights.SetRights += new SetRightsEventHandle(quickrights_SetRights);
                try
                {
                    quickrights.ModiRights(nodes, this.treeView_objects.Nodes[0]);
                }
                finally
                {
                    quickrights.GetNodeStyle -= new GetNodeStyleEventHandle(quickrights_GetNodeStyle);
                    quickrights.SetRights -= new SetRightsEventHandle(quickrights_SetRights);
                }

                /*
                for (int j = 0; j < quickrights.Count; j++)
                {
                    QuickRightsItem item = quickrights[j];

                    bool bRet = QuickRights.MatchType(parent.ImageIndex,
                        item.Type);
                    if (bRet == false)
                        continue;

                    if (item.Style != 0)
                    {
                        if (item.Style != ResRightTree.GetNodeStyle(parent))
                            continue;
                    }

                    SetRights(TreeViewUtil.GetPath(parent),
                        strUserName,
                        item.Rights);
                }
                 */

			}

            /*
			for(int i=0;i<parent.Nodes.Count;i++)
			{
				TreeNode node = parent.Nodes[i];
				ModiRights(node,
					aUserName,
                    quickrights);
			}
             */
		}

        void quickrights_GetNodeStyle(object sender, GetNodeStyleEventArgs e)
        {
            /*
            if (e.Node.ImageIndex == ResTree.RESTYPE_DB)
                e.Style = this.treeView_objects.DbStyle;
            else
                e.Style = 0;
             */
            e.Style = this.treeView_objects.GetNodeStyle(e.Node);
        }

        void quickrights_SetRights(object sender, SetRightsEventArgs e)
        {
            SetRights(TreeViewUtil.GetPath(e.Node, '/'),
                m_strTempUserName,
                e.Rights);
            
        }

        /*
        void quickrights_GetTreeNodeByPath(object sender, GetTreeNodeByPathEventArgs e)
        {
            e.Node = TreeViewUtil.GetTreeNode(this.treeView_objects.Nodes[0], e.Path);
        }
         */

	
		// treeview中一个对象已经被删除, 这里处理后续事情
		private void treeView_objects_OnObjectDeleted(object sender, DigitalPlatform.rms.Client.OnObjectDeletedEventArgs e)
		{
			string strError = "";
			int nRet = RemoveObjectFromCache(e.ObjectPath,
				out strError);
			if (nRet == -1)
			{
				MessageBox.Show(this, strError);
				return;
			}

			// this.m_rightsChanged.Remove(e.ObjectPath);
		}

		// 初始化全部对象节点的权限信息
		// 本函数应当在InitialUserRecordCache()后调用, 因为要用到用户记录信息
		int InitialObjRights(TreeNode parent,
			out string strError)
		{
			strError = "";
			Debug.Assert(parent != null, "");

			int i;
			int nRet;

			ObjRight objright = new ObjRight();

            string strDatabaseObject = TreeViewUtil.GetPath(parent, '/');

			for(i=0;i<this.m_aUserName.Count;i++)
			{
				string strUserName = (string)this.m_aUserName[i];

				Debug.Assert(strUserName != "", "用户名不能为空");

				string strRights = "";
				nRet = GetObjectRights(
					this.ServerUrl,
					strUserName,
					strDatabaseObject,
					out strRights,
					out strError);
				if (nRet == -1)
					return -1;

				RightLine line = new RightLine();
				line.UserName = strUserName;
				line.Rights = strRights;
				objright.RightLines.Add(line);
			}


			parent.Tag = objright;

			// 递归

			for(i=0;i<parent.Nodes.Count;i++)
			{
				TreeNode child = parent.Nodes[i];

				nRet = InitialObjRights(child,
					out strError);
				if (nRet == -1)
					return -1;

			}

			return 0;
		}

		// 在右侧listview上双击。修改权限。
		private void listView_usersRights_DoubleClick(object sender, System.EventArgs e)
		{
			menu_editRights_Click(null, null);
		}

		private void textBox_databaseType_TextChanged(object sender, System.EventArgs e)
		{
			this.m_bChanged = true;
		}

		private void textBox_sqlConnectionString_TextChanged(object sender, System.EventArgs e)
		{
			this.m_bChanged = true;

		}

		private void textBox_sqlDbName_TextChanged(object sender, System.EventArgs e)
		{
			this.m_bChanged = true;

		}

		private void textBox_browseDef_TextChanged(object sender, System.EventArgs e)
		{
			this.m_bChanged = true;
		}

		private void textBox_keysDef_TextChanged(object sender, System.EventArgs e)
		{
			this.m_bChanged = true;
		}



		// 缓存用户记录
		public class UserRec
		{
			public string Xml = "";
			public byte [] TimeStamp = null;
			public string RecPath = "";
			public bool Changed = false;

		}

		// 一个对象节点所附加的权限信息
		public class ObjRight
		{
			public ArrayList RightLines = new ArrayList();

			public string GetRights(string strUserName)
			{
				for(int i=0;i<RightLines.Count;i++)
				{
					RightLine line = (RightLine)RightLines[i];
					if (line.UserName == strUserName)
						return line.Rights;
				}

				return null;	// not found
			}

			public void SetRights(string strUserName,
				string strRights)
			{
				RightLine line = null;

				for(int i=0;i<RightLines.Count;i++)
				{
					line = (RightLine)RightLines[i];
					if (line.UserName == strUserName)
						goto FOUND;
				}

					line = new RightLine();
					line.UserName = strUserName;
					this.RightLines.Add(line);


				FOUND:

				line.Rights = strRights;
				line.Changed = true;
			}
		}

		// 一行信息: 包括用户名和权限字符串
		public class RightLine
		{
			public string UserName = "";
			public string Rights = null;
			public bool Changed = false;
		}

        private void button_formatKeysXml_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strOutXml = "";
            int nRet = DomUtil.GetIndentXml(this.textBox_keysDef.Text,
                out strOutXml,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                this.textBox_keysDef.Text = strOutXml;
        }

        private void button_formatBrowseXml_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strOutXml = "";
            int nRet = DomUtil.GetIndentXml(this.textBox_browseDef.Text,
                out strOutXml,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);
            else
                this.textBox_browseDef.Text = strOutXml;

        }

        private void listView_logicName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

	}
}
