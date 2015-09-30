using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;

using DigitalPlatform.Xml;

namespace DigitalPlatform.rms.Client
{
	public enum AskMode
	{
		None = 0,
		AskNullOrigin = 1,
		AskNotMatchOrigin = 2,
	}
	/// <summary>
	/// Summary description for DbNameMapItemDlg.
	/// </summary>
	public class DbNameMapItemDlg : System.Windows.Forms.Form
	{
        public ApplicationInfo AppInfo = null;
        public ServerCollection Servers = null;
        public RmsChannelCollection Channels = null;


		public AskMode AskMode = AskMode.None;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_origin;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox_target;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboBox_writeMode;
		private System.Windows.Forms.Button button_OK;
		private System.Windows.Forms.Button button_Cancel;
		private System.Windows.Forms.Button button_findOrigin;
		private System.Windows.Forms.Button button_findTarget;
		private System.Windows.Forms.Button button_directAllServerMap;
		private System.Windows.Forms.TextBox textBox_comment;
		private System.Windows.Forms.Button button_explain;
        private Button button_directSingleServerMap;
        private Button button_directSingleDbMap;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DbNameMapItemDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbNameMapItemDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_origin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_target = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_writeMode = new System.Windows.Forms.ComboBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_findOrigin = new System.Windows.Forms.Button();
            this.button_findTarget = new System.Windows.Forms.Button();
            this.button_directAllServerMap = new System.Windows.Forms.Button();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.button_explain = new System.Windows.Forms.Button();
            this.button_directSingleServerMap = new System.Windows.Forms.Button();
            this.button_directSingleDbMap = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "源(&O):";
            // 
            // textBox_origin
            // 
            this.textBox_origin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_origin.Location = new System.Drawing.Point(12, 88);
            this.textBox_origin.Name = "textBox_origin";
            this.textBox_origin.Size = new System.Drawing.Size(415, 21);
            this.textBox_origin.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 129);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "目标(&T):";
            // 
            // textBox_target
            // 
            this.textBox_target.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_target.Location = new System.Drawing.Point(12, 144);
            this.textBox_target.Name = "textBox_target";
            this.textBox_target.Size = new System.Drawing.Size(415, 21);
            this.textBox_target.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 195);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "写入方式(&M):";
            // 
            // comboBox_writeMode
            // 
            this.comboBox_writeMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBox_writeMode.Items.AddRange(new object[] {
            "overwrite -- 覆盖",
            "append -- 追加",
            "skip -- 跳过"});
            this.comboBox_writeMode.Location = new System.Drawing.Point(120, 192);
            this.comboBox_writeMode.Name = "comboBox_writeMode";
            this.comboBox_writeMode.Size = new System.Drawing.Size(152, 20);
            this.comboBox_writeMode.TabIndex = 7;
            this.comboBox_writeMode.Text = "overwrite -- 覆盖";
            this.comboBox_writeMode.TextChanged += new System.EventHandler(this.comboBox_writeMode_TextChanged);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(307, 277);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(388, 277);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_findOrigin
            // 
            this.button_findOrigin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findOrigin.Location = new System.Drawing.Point(431, 86);
            this.button_findOrigin.Name = "button_findOrigin";
            this.button_findOrigin.Size = new System.Drawing.Size(32, 23);
            this.button_findOrigin.TabIndex = 2;
            this.button_findOrigin.Text = "...";
            this.button_findOrigin.Click += new System.EventHandler(this.button_findOrigin_Click);
            // 
            // button_findTarget
            // 
            this.button_findTarget.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findTarget.Location = new System.Drawing.Point(431, 142);
            this.button_findTarget.Name = "button_findTarget";
            this.button_findTarget.Size = new System.Drawing.Size(32, 23);
            this.button_findTarget.TabIndex = 5;
            this.button_findTarget.Text = "...";
            this.button_findTarget.Click += new System.EventHandler(this.button_findTarget_Click);
            // 
            // button_directAllServerMap
            // 
            this.button_directAllServerMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_directAllServerMap.Location = new System.Drawing.Point(12, 232);
            this.button_directAllServerMap.Name = "button_directAllServerMap";
            this.button_directAllServerMap.Size = new System.Drawing.Size(136, 23);
            this.button_directAllServerMap.TabIndex = 10;
            this.button_directAllServerMap.Text = "全部服务器对应(&A)";
            this.button_directAllServerMap.Click += new System.EventHandler(this.button_directAllServerMap_Click);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(12, 12);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(451, 44);
            this.textBox_comment.TabIndex = 11;
            // 
            // button_explain
            // 
            this.button_explain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_explain.Location = new System.Drawing.Point(12, 277);
            this.button_explain.Name = "button_explain";
            this.button_explain.Size = new System.Drawing.Size(75, 23);
            this.button_explain.TabIndex = 12;
            this.button_explain.Text = "解释(&E)";
            this.button_explain.Click += new System.EventHandler(this.button_explain_Click);
            // 
            // button_directSingleServerMap
            // 
            this.button_directSingleServerMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_directSingleServerMap.Enabled = false;
            this.button_directSingleServerMap.Location = new System.Drawing.Point(154, 232);
            this.button_directSingleServerMap.Name = "button_directSingleServerMap";
            this.button_directSingleServerMap.Size = new System.Drawing.Size(131, 23);
            this.button_directSingleServerMap.TabIndex = 13;
            this.button_directSingleServerMap.Text = "当前服务器内对应(&S)";
            this.button_directSingleServerMap.Click += new System.EventHandler(this.button_directSingleServerMap_Click);
            // 
            // button_directSingleDbMap
            // 
            this.button_directSingleDbMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_directSingleDbMap.Location = new System.Drawing.Point(291, 232);
            this.button_directSingleDbMap.Name = "button_directSingleDbMap";
            this.button_directSingleDbMap.Size = new System.Drawing.Size(102, 23);
            this.button_directSingleDbMap.TabIndex = 14;
            this.button_directSingleDbMap.Text = "原库对应(&D)";
            this.button_directSingleDbMap.Click += new System.EventHandler(this.button_directSingleDbMap_Click);
            // 
            // DbNameMapItemDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(475, 312);
            this.Controls.Add(this.button_directSingleDbMap);
            this.Controls.Add(this.button_directSingleServerMap);
            this.Controls.Add(this.button_explain);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.button_directAllServerMap);
            this.Controls.Add(this.button_findTarget);
            this.Controls.Add(this.button_findOrigin);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.comboBox_writeMode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_target);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_origin);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DbNameMapItemDlg";
            this.ShowInTaskbar = false;
            this.Text = "库名映射事项";
            this.Load += new System.EventHandler(this.DbNameMapItemDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void button_OK_Click(object sender, System.EventArgs e)
		{
			if (this.textBox_origin.Text == "")
			{
				MessageBox.Show(this, "尚未指定源");
				return;
			}

			if (this.textBox_target.Text == ""
				&& this.WriteMode != "skip")
			{
				MessageBox.Show(this, "尚未指定目标");
				return;
			}

			if (this.comboBox_writeMode.Text == "")
			{
				MessageBox.Show(this, "尚未指定写入方式");
				return;
			}
		
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button_Cancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void button_findOrigin_Click(object sender, System.EventArgs e)
		{
			OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.Text = "请选择源数据库";
			dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
			dlg.ap = this.AppInfo;
			dlg.ApCfgTitle = "dbnamemapitemdlg_origin";
			dlg.MultiSelect = false;
			dlg.Path = this.textBox_origin.Text;
			dlg.Initial( this.Servers,
				this.Channels);	
			// dlg.StartPositon = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			textBox_origin.Text = dlg.Path;
		}

		private void button_findTarget_Click(object sender, System.EventArgs e)
		{
			OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

			dlg.Text = "请选择目标数据库";
			dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
			dlg.ap = this.AppInfo;
			dlg.ApCfgTitle = "dbnamemapitemdlg_origin";
			dlg.MultiSelect = false;

            // 如果目标textbox有内容，优先用它决定缺省选定的数据库节点。否则用源textbox的内容
            if (string.IsNullOrEmpty(this.textBox_target.Text) == false)
			    dlg.Path = this.textBox_target.Text;
            else if (string.IsNullOrEmpty(this.textBox_origin.Text) == false)
                dlg.Path = this.textBox_origin.Text;

			dlg.Initial( this.Servers,
				this.Channels);	
			// dlg.StartPositon = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);   // 对于数据库节点，不要主动展开其下一级

			if (dlg.DialogResult != DialogResult.OK)
				return;

			textBox_target.Text = dlg.Path;		
		}

		// 询问空源路径如何处理
		// return:
        //      -1  出错
		//		0	cancel全部处理
		//		1	已经选择处理办法
		public static int AskNullOriginBox(
			IWin32Window owner,
			ApplicationInfo ap,
                ServerCollection Servers,
                RmsChannelCollection Channels,
            string strComment,
            string strSelectedLongPath,
			DbNameMap map)
		{
			DbNameMapItemDlg dlg = new DbNameMapItemDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Servers = Servers;
            dlg.Channels = Channels;
            dlg.Comment = strComment;
			dlg.AskMode = AskMode.AskNullOrigin;
			dlg.Origin = "{null}";
            dlg.Target = strSelectedLongPath;
			dlg.WriteMode = "append";

			dlg.Text = "请指定映射关系";

			if (ap != null)
				ap.LinkFormState(dlg, "AskNotMatchOriginBox_state");
			dlg.ShowDialog(owner);
			if (ap != null)
				ap.UnlinkFormState(dlg);


			if (dlg.DialogResult != DialogResult.OK)
				return 0;	// cancel

            string strError = "";
            if (map.NewItem(dlg.Origin, dlg.Target, dlg.WriteMode,
                0,// 插入最前面
                out strError) == null)
            {
                MessageBox.Show(owner, strError);
                return -1;
            }

			return 1;
		}

        // 询问无法匹配的源路径如何处理
        // return:
        //      -1  出错
        //		0	cancel全部处理
        //		1	已经选择处理办法
        public static int AskNotMatchOriginBox(
            IWin32Window owner,
            ApplicationInfo ap,
            ServerCollection Servers,
            RmsChannelCollection Channels,
            string strComment,
            string strSelectedLongPath,
            string strOrigin,
            DbNameMap map)
        {
            DbNameMapItemDlg dlg = new DbNameMapItemDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Servers = Servers;
            dlg.Channels = Channels;
            dlg.Comment = strComment;
            dlg.AskMode = AskMode.AskNotMatchOrigin;
            dlg.Origin = strOrigin;
            dlg.Target = strSelectedLongPath;
            dlg.WriteMode = "append";

            dlg.Text = "请指定映射关系";

            if (ap != null)
                ap.LinkFormState(dlg, "AskNotMatchOriginBox_state");
            dlg.ShowDialog(owner);
            if (ap != null)
                ap.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;	// cancel

            string strError = "";
            if (map.NewItem(dlg.Origin, dlg.Target,
                dlg.WriteMode,
                0, // 插入最前面
                out strError) == null)
            {
                MessageBox.Show(owner, strError);
                return -1;
            }

            return 1;
        }

        // 全部服务器直接对应
		private void button_directAllServerMap_Click(object sender, System.EventArgs e)
		{
			this.textBox_origin.Text = "*";
			this.textBox_origin.Enabled = true;
			this.button_findOrigin.Enabled = true;

			this.textBox_target.Text = "*";
			this.textBox_target.Enabled = true;
			this.button_findTarget.Enabled = true;

		}

        // 当前服务器内直接对应
        private void button_directSingleServerMap_Click(object sender, EventArgs e)
        {
            if (this.textBox_origin.Text.IndexOf("?") == -1)
            {
                MessageBox.Show("无法设置对应");
                return;
            }

            ResPath respath = new ResPath(this.textBox_origin.Text);

            this.textBox_origin.Text = respath.Url + "?*";
            this.textBox_origin.Enabled = true;
            this.button_findOrigin.Enabled = true;

            this.textBox_target.Text = respath.Url + "?*";
            this.textBox_target.Enabled = true;
            this.button_findTarget.Enabled = true;
        }

        // 原库对应
        private void button_directSingleDbMap_Click(object sender, EventArgs e)
        {
            if (this.textBox_origin.Text.IndexOf("?") == -1)
            {
                MessageBox.Show("无法设置对应");
                return;
            }

            this.textBox_target.Text = this.textBox_origin.Text;
            this.textBox_target.Enabled = true;
            this.button_findTarget.Enabled = true;
        }

		private void DbNameMapItemDlg_Load(object sender, System.EventArgs e)
		{
			if (this.AskMode == AskMode.AskNullOrigin)
			{
				this.textBox_origin.Enabled = false;
				this.button_findOrigin.Enabled = false;
				this.button_directAllServerMap.Enabled = false;

				this.comboBox_writeMode.Items.Clear();
				this.comboBox_writeMode.Items.Add("append -- 追加");
				this.comboBox_writeMode.Items.Add("skip -- 跳过");
			}
			else if (this.AskMode == AskMode.AskNotMatchOrigin)
			{
				this.textBox_origin.Enabled = false;
				this.button_findOrigin.Enabled = false;

				this.button_directAllServerMap.Enabled = true;
			}
		}

		private void button_explain_Click(object sender, System.EventArgs e)
		{
			string strText = "";

			if (this.textBox_origin.Text == "{null}")
			{
				if (this.WriteMode == "skip")
					strText = "将源数据文件中没有来源库信息的记录，全部跳过。";
				else
					strText = "将源数据文件中没有来源库信息的记录, 以 " +this.WriteModeCaption + "方式写入目标 数据库 "+this.textBox_target.Text+" 中。";
				goto DONE;
			}

			if (this.textBox_origin.Text == "*" 
				&& this.textBox_target.Text == "*")
			{
				if (this.WriteMode == "skip")
					strText = "将源数据文件中有来源数据库信息的记录, 全部跳过。";
				else
                    strText = "将源数据文件中有来源数据库信息的记录, 以 " + this.WriteModeCaption + " 方式写入服务器端同样数据库中。";
				goto DONE;
			}

			if (this.textBox_origin.Text == "*" 
				&& this.textBox_target.Text != "*")
			{
				if (this.WriteMode == "skip")
					strText = "将源数据文件中有来源数据库信息的记录, 全部跳过。";
				else
                    strText = "将源数据文件中有来源数据库信息的记录, 不管其来源数据库名是什么, 都以 " + this.WriteModeCaption + " 方式写入服务器端数据库 " + this.textBox_target.Text + " 中。";
				goto DONE;
			}

			if (this.textBox_origin.Text != "*" 
				&& this.textBox_target.Text == "*")
			{
				if (this.WriteMode == "skip")
					strText = "将源数据文件中的记录, 如果其随文件记载的来源数据库名是 "+this.textBox_origin.Text+" ，则跳过。";
				else
                    strText = "将源数据文件中的记录, 如果其随文件记载的来源数据库名是 " + this.textBox_origin.Text + " ，则以 " + this.WriteModeCaption + " 方式写入服务器端同名数据库中。";
				goto DONE;
			}
	
			if (this.WriteMode == "skip")
				strText = "将源数据文件中的记录, 如果其随文件记载的来源数据库名是 "+this.textBox_origin.Text+" ，则跳过。";
			else
                strText = "将源数据文件中的记录, 如果其随文件记载的来源数据库名是 " + this.textBox_origin.Text + " ，则以 " + this.WriteModeCaption + " 方式写入服务器端数据库 " + this.textBox_target.Text + " 中。";

			DONE:
			MessageBox.Show(this, strText);
		}

		private void comboBox_writeMode_TextChanged(object sender, System.EventArgs e)
		{
			if (this.WriteMode == "skip")
			{
				this.textBox_target.Text = "";
				this.textBox_target.Enabled = false;
			}
			else 
			{
				this.textBox_target.Enabled = true;
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

		public string Origin
		{
			get
			{
				return this.textBox_origin.Text;
			}
			set 
			{
				this.textBox_origin.Text = value;
			}
		}

		public string Target
		{
			get 
			{
				return this.textBox_target.Text;
			}
			set 
			{
				this.textBox_target.Text = value;
			}
		}

        public string WriteModeCaption
        {
            get
            {
                string strText = this.WriteMode;
                if (strText == "append")
                    return "追加";
                if (strText == "overwrite")
                    return "覆盖";
                if (strText == "skip")
                    return "跳过";

                return strText;
            }
        }

		public string WriteMode
		{
			get 
			{
                
				return StringUtil.GetLeft(this.comboBox_writeMode.Text);
			}
			set 
			{
                string strValue = StringUtil.GetLeft(value);
                foreach (string s in this.comboBox_writeMode.Items)
                {
                    string strLeft = StringUtil.GetLeft(s);
                    if (strLeft == strValue)
                    {
                        this.comboBox_writeMode.Text = s;
                        return;
                    }
                }


				this.comboBox_writeMode.Text = value;
			}
		}




	}
}
