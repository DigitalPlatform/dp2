using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Threading;


using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.Library
{

	/// <summary>
	/// 种次号维护对话框
	/// </summary>
	public class ZhongcihaoDlg : System.Windows.Forms.Form
	{
		XmlNode CurrentNode = null;	// 当前数据库定义节点。为回调函数而设置

		SearchPanel SearchPanel = null;

        /// <summary>
        /// 检索结束信号
        /// </summary>
		public AutoResetEvent EventFinish = new AutoResetEvent(false);


		bool m_bAutoBeginSearch = false;

		string m_strMaxNumber = null;
		string m_strTailNumber = null;

        /// <summary>
        /// 打开详细窗
        /// </summary>
		public event OpenDetailEventHandler OpenDetail = null;

		XmlDocument dom = null;

		byte [] TailNumberTimestamp = null;

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox_classNumber;
        /// <summary>
        /// 显示记录和种次号的ListView
        /// </summary>
		public  System.Windows.Forms.ListView listView_number;
		private System.Windows.Forms.ColumnHeader columnHeader_path;
		private System.Windows.Forms.ColumnHeader columnHeader_number;
		private System.Windows.Forms.ColumnHeader columnHeader_title;
		private System.Windows.Forms.ColumnHeader columnHeader_author;
		private System.Windows.Forms.Button button_search;
		private System.Windows.Forms.Button button_stop;
		private System.Windows.Forms.Label label_message;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button_findServerUrl;
		private System.Windows.Forms.TextBox textBox_serverUrl;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox_groupName;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBox_maxNumber;
		private System.Windows.Forms.Button button_copyMaxNumber;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox textBox_tailNumber;
		private System.Windows.Forms.Button button_resetTailNumber;
		private System.Windows.Forms.Button button_pushTailNumber;
		private System.Windows.Forms.Label label_tailNumberTitle;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        /// <summary>
        /// 构造函数
        /// </summary>
		public ZhongcihaoDlg()
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZhongcihaoDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_classNumber = new System.Windows.Forms.TextBox();
            this.listView_number = new System.Windows.Forms.ListView();
            this.columnHeader_path = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_number = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_title = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_author = new System.Windows.Forms.ColumnHeader();
            this.button_search = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.label_message = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_serverUrl = new System.Windows.Forms.TextBox();
            this.button_findServerUrl = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_groupName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_maxNumber = new System.Windows.Forms.TextBox();
            this.button_copyMaxNumber = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label_tailNumberTitle = new System.Windows.Forms.Label();
            this.textBox_tailNumber = new System.Windows.Forms.TextBox();
            this.button_resetTailNumber = new System.Windows.Forms.Button();
            this.button_pushTailNumber = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "类号(&C):";
            // 
            // textBox_classNumber
            // 
            this.textBox_classNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_classNumber.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_classNumber.Location = new System.Drawing.Point(149, 95);
            this.textBox_classNumber.Name = "textBox_classNumber";
            this.textBox_classNumber.Size = new System.Drawing.Size(239, 25);
            this.textBox_classNumber.TabIndex = 1;
            // 
            // listView_number
            // 
            this.listView_number.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_number.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_number,
            this.columnHeader_title,
            this.columnHeader_author});
            this.listView_number.FullRowSelect = true;
            this.listView_number.HideSelection = false;
            this.listView_number.Location = new System.Drawing.Point(16, 130);
            this.listView_number.Name = "listView_number";
            this.listView_number.Size = new System.Drawing.Size(588, 115);
            this.listView_number.TabIndex = 2;
            this.listView_number.UseCompatibleStateImageBehavior = false;
            this.listView_number.View = System.Windows.Forms.View.Details;
            this.listView_number.DoubleClick += new System.EventHandler(this.listView_number_DoubleClick);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "记录路径";
            this.columnHeader_path.Width = 93;
            // 
            // columnHeader_number
            // 
            this.columnHeader_number.Text = "种次号";
            this.columnHeader_number.Width = 115;
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "题名";
            this.columnHeader_title.Width = 219;
            // 
            // columnHeader_author
            // 
            this.columnHeader_author.Text = "著者";
            this.columnHeader_author.Width = 103;
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(396, 93);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(100, 29);
            this.button_search.TabIndex = 3;
            this.button_search.Text = "检索(&S)";
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(504, 93);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(100, 29);
            this.button_stop.TabIndex = 4;
            this.button_stop.Text = "停止(&S)";
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(16, 377);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(588, 29);
            this.label_message.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "服务器(&S):";
            // 
            // textBox_serverUrl
            // 
            this.textBox_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverUrl.Location = new System.Drawing.Point(149, 15);
            this.textBox_serverUrl.Name = "textBox_serverUrl";
            this.textBox_serverUrl.Size = new System.Drawing.Size(407, 25);
            this.textBox_serverUrl.TabIndex = 7;
            this.textBox_serverUrl.TextChanged += new System.EventHandler(this.textBox_serverUrl_TextChanged);
            // 
            // button_findServerUrl
            // 
            this.button_findServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findServerUrl.Location = new System.Drawing.Point(561, 15);
            this.button_findServerUrl.Name = "button_findServerUrl";
            this.button_findServerUrl.Size = new System.Drawing.Size(43, 30);
            this.button_findServerUrl.TabIndex = 8;
            this.button_findServerUrl.Text = "...";
            this.button_findServerUrl.Click += new System.EventHandler(this.button_findServerUrl_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "库群(&G):";
            // 
            // textBox_groupName
            // 
            this.textBox_groupName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_groupName.Location = new System.Drawing.Point(149, 50);
            this.textBox_groupName.Name = "textBox_groupName";
            this.textBox_groupName.Size = new System.Drawing.Size(239, 25);
            this.textBox_groupName.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 260);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(189, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "统计出的种次号最大值(&M):";
            // 
            // textBox_maxNumber
            // 
            this.textBox_maxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_maxNumber.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox_maxNumber.Location = new System.Drawing.Point(245, 253);
            this.textBox_maxNumber.Name = "textBox_maxNumber";
            this.textBox_maxNumber.ReadOnly = true;
            this.textBox_maxNumber.Size = new System.Drawing.Size(225, 25);
            this.textBox_maxNumber.TabIndex = 12;
            // 
            // button_copyMaxNumber
            // 
            this.button_copyMaxNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_copyMaxNumber.Location = new System.Drawing.Point(476, 253);
            this.button_copyMaxNumber.Name = "button_copyMaxNumber";
            this.button_copyMaxNumber.Size = new System.Drawing.Size(128, 30);
            this.button_copyMaxNumber.TabIndex = 13;
            this.button_copyMaxNumber.Text = "复制最大号+1";
            this.button_copyMaxNumber.Click += new System.EventHandler(this.button_copyMaxNumber_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(16, 315);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(588, 10);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            // 
            // label_tailNumberTitle
            // 
            this.label_tailNumberTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_tailNumberTitle.AutoSize = true;
            this.label_tailNumberTitle.Location = new System.Drawing.Point(16, 339);
            this.label_tailNumberTitle.Name = "label_tailNumberTitle";
            this.label_tailNumberTitle.Size = new System.Drawing.Size(151, 15);
            this.label_tailNumberTitle.TabIndex = 15;
            this.label_tailNumberTitle.Text = "种次号库中的尾号(&T)";
            // 
            // textBox_tailNumber
            // 
            this.textBox_tailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tailNumber.Location = new System.Drawing.Point(245, 338);
            this.textBox_tailNumber.Name = "textBox_tailNumber";
            this.textBox_tailNumber.Size = new System.Drawing.Size(225, 25);
            this.textBox_tailNumber.TabIndex = 16;
            // 
            // button_resetTailNumber
            // 
            this.button_resetTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_resetTailNumber.Location = new System.Drawing.Point(478, 335);
            this.button_resetTailNumber.Name = "button_resetTailNumber";
            this.button_resetTailNumber.Size = new System.Drawing.Size(86, 30);
            this.button_resetTailNumber.TabIndex = 17;
            this.button_resetTailNumber.Text = "重设(&R)";
            this.button_resetTailNumber.Click += new System.EventHandler(this.button_resetTailNumber_Click);
            // 
            // button_pushTailNumber
            // 
            this.button_pushTailNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_pushTailNumber.Location = new System.Drawing.Point(246, 288);
            this.button_pushTailNumber.Name = "button_pushTailNumber";
            this.button_pushTailNumber.Size = new System.Drawing.Size(224, 29);
            this.button_pushTailNumber.TabIndex = 18;
            this.button_pushTailNumber.Text = "推动种次号库尾号(&P)";
            this.button_pushTailNumber.Click += new System.EventHandler(this.button_pushTailNumber_Click);
            // 
            // ZhongcihaoDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 18);
            this.ClientSize = new System.Drawing.Size(620, 418);
            this.Controls.Add(this.button_pushTailNumber);
            this.Controls.Add(this.button_resetTailNumber);
            this.Controls.Add(this.textBox_tailNumber);
            this.Controls.Add(this.textBox_maxNumber);
            this.Controls.Add(this.textBox_groupName);
            this.Controls.Add(this.textBox_serverUrl);
            this.Controls.Add(this.textBox_classNumber);
            this.Controls.Add(this.label_tailNumberTitle);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_copyMaxNumber);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button_findServerUrl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_search);
            this.Controls.Add(this.listView_number);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ZhongcihaoDlg";
            this.Text = "维护种次号";
            this.Load += new System.EventHandler(this.ZhongcihaoDlg_Load);
            this.Closed += new System.EventHandler(this.ZhongcihaoDlg_Closed);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        /// <summary>
        /// 组名。若干书目库为共享同一种次号库，需聚集为一个“组”
        /// </summary>
		public string GroupName
		{
			get 
			{
				return this.textBox_groupName.Text;
			}
			set 
			{
				this.textBox_groupName.Text = value;
			}
		}

        /// <summary>
        /// 服务器URL
        /// </summary>
		public string ServerUrl
		{
			get 
			{
				return textBox_serverUrl.Text;
			}
			set
			{
				dom = null;
				textBox_serverUrl.Text = value;
			}
		}

        /// <summary>
        /// 类号
        /// </summary>
		public string ClassNumber
		{
			get 
			{
				return this.textBox_classNumber.Text;
			}
			set 
			{
				this.textBox_classNumber.Text = value;
			}
		}

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="searchpanel">检索面板</param>
        /// <param name="strServerUrl">服务器URL</param>
        /// <param name="strGroupName">组名称</param>
        /// <param name="strClassNumber">分类号</param>
        /// <param name="bAutoBeginSearch">对话框打开后是否自动开始检索</param>
		public void Initial(
			SearchPanel searchpanel,
			/*ServerCollection servers,
			CfgCache cfgcache,
			*/
			string strServerUrl,
			string strGroupName,
			string strClassNumber,
			bool bAutoBeginSearch)
		{
			/*
			this.Servers = servers;
			this.Channels.procAskAccountInfo = 
				new Delegate_AskAccountInfo(this.Servers.AskAccountInfo);

			this.cfgCache = cfgcache;
			*/

			this.SearchPanel = searchpanel;

			this.SearchPanel.InitialStopManager(this.button_stop,
				this.label_message);

			this.ServerUrl = strServerUrl;
			this.GroupName = strGroupName;
			this.ClassNumber = strClassNumber;

			this.m_bAutoBeginSearch = bAutoBeginSearch;
		}

		private void ZhongcihaoDlg_Load(object sender, System.EventArgs e)
		{
			/*
			stop = new DigitalPlatform.GUI.Stop();
		
			stop.Register(this.stopManager);	// 和容器关联
			*/

			object[] pList = new object []  { null, null };

			if (m_bAutoBeginSearch == true) 
			{
				this.BeginInvoke(new Delegate_Search(this.button_search_Click), pList);
			}
		}


		delegate void Delegate_Search(object sender, EventArgs e);

        /// <summary>
        /// 等待检索结束
        /// </summary>
		public void WaitSearchFinish()
		{
			for(;;)
			{
				Application.DoEvents();
				bool bRet = this.EventFinish.WaitOne(10, true);
				if (bRet == true)
					break;
			}
		}

		private void ZhongcihaoDlg_Closed(object sender, System.EventArgs e)
		{
			/*
			if (stop != null) // 脱离关联
			{
				stop.Unregister();	// 和容器关联

				// MainForm.stopManager.Remove(stop);
				stop = null;
			}
			*/

			EventFinish.Set();
		}


        /// <summary>
        /// 检索
        /// </summary>
        /// <param name="strError">返回的错误信息</param>
        /// <returns>-1出错;0正常</returns>
		public int DoSearch(out string strError)
		{
			strError = "";
			int nRet = GetGlobalCfgFile(out strError);
			if (nRet == -1)
				goto ERROR1;

			nRet = FillList(true, out strError);
			if (nRet == -1)
				goto ERROR1;

			nRet = PanelSearchTailNumber(out strError);
			if (nRet == -1)
				goto ERROR1;

			return 0;
			ERROR1:
				return -1;
		}

		// 检索尾号，放入面板中界面元素
		int PanelSearchTailNumber(out string strError)
		{
			strError = "";
			this.textBox_tailNumber.Text = "";

			string strTailNumber = "";
			int nRet = SearchTailNumber(out strTailNumber,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;

			this.textBox_tailNumber.Text = strTailNumber;
			this.label_tailNumberTitle.Text = "库'" +this.ZhongcihaoDbName+ "'中的尾号(&T):";

			return 0;
		}

		private void button_search_Click(object sender, System.EventArgs e)
		{
			string strError = "";

			EventFinish.Reset();
			try 
			{
                this.textBox_tailNumber.Text = "";   // yufang filllist tiqiantuichu, wangji qingchu

				int nRet = GetGlobalCfgFile(out strError);
				if (nRet == -1)
					goto ERROR1;

				nRet = FillList(true, out strError);
				if (nRet == -1)
					goto ERROR1;

				nRet = PanelSearchTailNumber(out strError);
				if (nRet == -1)
					goto ERROR1;

				return;
			}
			finally
			{
				EventFinish.Set();
			}

			ERROR1:
				MessageBox.Show(this, strError);
		}

		private void button_stop_Click(object sender, System.EventArgs e)
		{
			if (this.SearchPanel != null)
				this.SearchPanel.DoStopClick();
		}

		/*
		void DoStop()
		{
			if (this.SearchPanel != null)
				this.SearchPanel.DoStop();
		}
		*/

		int GetGlobalCfgFile(out string strError)
		{
			strError = "";

			if (this.dom != null)
				return 0;	// 优化

			if (this.textBox_serverUrl.Text == "")
			{
				strError = "尚未指定服务器URL";
				return -1;
			}

			string strCfgFilePath = "cfgs/global";
			XmlDocument tempdom = null;
		// 获得配置文件
		// return:
		//		-1	error
		//		0	not found
		//		1	found
			int nRet = this.SearchPanel.GetCfgFile(
				null,
				strCfgFilePath,
				out tempdom,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0) 
			{
				strError = "配置文件 '" + strCfgFilePath + "' 没有找到...";
				return -1;
			}

			this.dom = tempdom;

			return 0;
		}

		// 使相邻重复行变色
		void ColorDup()
		{
			string strPrevNumber = "";
			Color color1 = Color.FromArgb(220,220,220);
			Color color2 = Color.FromArgb(230,230,230);
			Color color = color1;
			int nDupCount = 0;
			for(int i=0;i<this.listView_number.Items.Count;i++)
			{
				string strNumber = this.listView_number.Items[i].SubItems[1].Text;

				if (strNumber == strPrevNumber)
				{
					if ( i>=1 && nDupCount == 0)
						this.listView_number.Items[i-1].BackColor = color;

					this.listView_number.Items[i].BackColor = color;
					nDupCount ++;
				}
				else 
				{
					if (nDupCount >= 1)
					{
						// 换一下颜色
						if (color == color1)
							color = color2;
						else 
							color = color1;
					}

					nDupCount = 0;

					this.listView_number.Items[i].BackColor = SystemColors.Window;

				}


				strPrevNumber = strNumber;
			}

		}

		int FillList(bool bSort,
			out string strError)
		{
			strError = "";
			// int nRet = 0;

			this.listView_number.Items.Clear();
            this.MaxNumber = "";

			if (dom == null)
			{
				strError = "请先调用GetGlobalCfgFile()函数";
				return -1;
			}

			if (this.ClassNumber == "")
			{
				strError = "尚未指定分类号";
				return -1;
			}

			if (this.GroupName == "")
			{
				strError = "尚未指定库群";
				return -1;
			}

			if (this.ServerUrl == "")
			{
				strError = "尚未指定服务器URL";
				return -1;
			}

			XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dbgroup[@name='"+this.GroupName+"']/database");

			for(int i=0;i<nodes.Count;i++) 
			{
				XmlNode node = nodes[i];
				string strDbName = DomUtil.GetAttr(node, "name");
				string strFromName = DomUtil.GetAttr(node, "accessnumber", "leftfrom");

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
				string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFromName)        // 2007/9/14
                    + "'><item><word>"
					+ StringUtil.GetXmlStringSimple(this.ClassNumber)
                    + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

				this.CurrentNode = node;

				this.SearchPanel.BeginLoop("正在针对库 '"+strDbName+"' 检索 '" + this.ClassNumber + "'");

				long lRet = 0;

				this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(BrowseRecordCallBack);
				this.SearchPanel.BrowseRecord += new BrowseRecordEventHandler(BrowseRecordCallBack);

				try 
				{
					// return:
					//		-2	用户中断
					//		-1	一般错误
					//		0	未命中
					//		>=1	正常结束，返回命中条数
					lRet = this.SearchPanel.SearchAndBrowse(
						null,
                        strQueryXml,
                        false,
						out strError);
				}
				finally 
				{
					this.SearchPanel.EndLoop();
				}

		
				// long lRet = channel.DoSearch(strQueryXml, out strError);
				if (lRet == -1)
                {
                    strError = "检索库 " + strDbName + " 时出错: " + strError;
                    DialogResult result = MessageBox.Show(this,
                         strError + "\r\n\r\n是否继续进行操作?",
                         "ZhongcihaoDlg",
                         MessageBoxButtons.YesNo,
                         MessageBoxIcon.Question,
                         MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                        continue;
                    else
                    {
                        strError += "\r\n\r\n操作已被放弃。";
                        return -1;
                    }
                }
				if (lRet == 0) 
					continue;

			}

			if (bSort == true)
			{
				this.listView_number.ListViewItemSorter = new ListViewItemComparer();

				ColorDup();

				if (this.listView_number.Items.Count == 0)
					this.MaxNumber = "";
				else 
					this.MaxNumber = this.listView_number.Items[0].SubItems[1].Text;
			}

			return 0;
		}


		void BrowseRecordCallBack(object sender, BrowseRecordEventArgs e)
		{
			string strError = "";
			int nRet = FillList(
				this.CurrentNode,
				e.FullPath,
				out strError);
			if (nRet == -1) 
			{
				e.Cancel = true;
				e.ErrorInfo = strError;
			}
		}

        /// <summary>
        /// 保存尾号到种次号库
        /// </summary>
        /// <param name="strTailNumber">要保存的尾号</param>
        /// <param name="strError">返回的错误信息</param>
        /// <returns>-1出错;0正常</returns>
		public int SaveTailNumber(
            string strTailNumber,
			out string strError)
		{
			strError = "";
			string strPath = "";
			int nRet = SearchTailNumberPath(
				out strPath,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0) 
			{
				// 新创建记录
				string strZhongcihaoDbName = "";

				try 
				{
					strZhongcihaoDbName = this.ZhongcihaoDbName;
				}
				catch (Exception ex)
				{
                    strError = ExceptionUtil.GetAutoText(ex);
					return -1;
				}

				strPath = strZhongcihaoDbName + "/?";

			}
			else 
			{
				// 覆盖记录
			}

			string strXml = "<r c='"+this.ClassNumber+"' v='"+strTailNumber+"'/>";

			
			byte [] baOutputTimestamp = null;
			// string strOutputPath = "";

			//string strTemp = ByteArray.GetHexTimeStampString(this.TimeStamp);

			REDO:
				// return:
				//		-2	时间戳不匹配
				//		-1	一般出错
				//		0	正常
				nRet = this.SearchPanel.SaveRecord(
                    null,
					strPath,
					strXml,
					this.TailNumberTimestamp,
					false,
					out baOutputTimestamp,
					out strError);
			if (nRet == -1)
				return -1;

			if (nRet == -2)
			{
				DialogResult result = MessageBox.Show(this, 
					"尾号记录时间戳不匹配，说明可能被他人修改过。详细原因: " + strError
					+ "\r\n\r\n是否强行保存记录?",
					this.Text,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button2);
				if (result == DialogResult.Yes)
				{
					this.TailNumberTimestamp = baOutputTimestamp;
					goto REDO;
				}
			}

			this.TailNumberTimestamp = baOutputTimestamp;

			return 0;
		}

        /// <summary>
        /// 增量尾号。
        /// </summary>
        /// <param name="strDefaultNumber">初始值。如果本类(ClassNumber属性中有类号)之种次号条目不存在，则采用本初始值。(此时是否还要增量?)</param>
        /// <param name="strOutputNumber">增量后的尾号</param>
        /// <param name="strError"></param>
        /// <returns></returns>
		public int IncreaseTailNumber(string strDefaultNumber,
			out string strOutputNumber,
			out string strError)
		{
			strError = "";
			strOutputNumber = "";

			string strPath = "";
			int nRet = SearchTailNumberPath(
				out strPath,
				out strError);
			if (nRet == -1)
				return -1;

			string strXml = "";
			bool bNewRecord = false;

			if (nRet == 0) 
			{
				// 新创建记录
				string strZhongcihaoDbName = "";

				try 
				{
					strZhongcihaoDbName = this.ZhongcihaoDbName;
				}
				catch (Exception ex)
				{
                    strError = ExceptionUtil.GetAutoText(ex);
					return -1;
				}


				strPath = strZhongcihaoDbName + "/?";
				strXml = "<r c='"+this.ClassNumber+"' v='"+strDefaultNumber+"'/>";

				bNewRecord = true;

			}
			else 
			{
				string strPartXml = "/xpath/<locate>@v</locate><action>+AddInteger</action>";
				strPath += strPartXml;
				strXml = "1";

				bNewRecord = false;
			}

			
			byte [] baOutputTimestamp = null;
			// string strOutputPath = "";


			// return:
			//		-2	时间戳不匹配
			//		-1	一般出错
			//		0	正常
			nRet = this.SearchPanel.SaveRecord(
                null,
				strPath,
				strXml,
				this.TailNumberTimestamp,
				true,
				out baOutputTimestamp,
				out strError);
			if (nRet < 0)
			{
				return -1;
			}

			this.TailNumberTimestamp = baOutputTimestamp;

			if (bNewRecord == true)
			{
				strOutputNumber = strDefaultNumber;
			}
			else 
			{
				strOutputNumber = strError;
			}


			return 0;
		}


        /// <summary>
        /// 推动尾号
        /// </summary>
        /// <param name="strTestNumber">试探号码。如果本类的尾号小于试探号码，将会被推动。</param>
        /// <param name="strOutputNumber">被推动后的尾号</param>
        /// <param name="strError">返回的错误信息</param>
        /// <returns>-1出错;0正常</returns>
		public int PushTailNumber(string strTestNumber,
			out string strOutputNumber,
			out string strError)
		{
			strError = "";
			strOutputNumber = "";

			string strPath = "";
			int nRet = SearchTailNumberPath(
				out strPath,
				out strError);
			if (nRet == -1)
				return -1;

			string strXml = "";
			bool bNewRecord = false;

			if (nRet == 0) 
			{
				// 新创建记录
				string strZhongcihaoDbName = "";

				try 
				{
					strZhongcihaoDbName = this.ZhongcihaoDbName;
				}
				catch (Exception ex)
				{
                    strError = ExceptionUtil.GetAutoText(ex);
					return -1;
				}


				strPath = strZhongcihaoDbName + "/?";
				strXml = "<r c='"+this.ClassNumber+"' v='"+strTestNumber+"'/>";

				bNewRecord = true;

			}
			else 
			{
				string strPartXml = "/xpath/<locate>@v</locate><action>Push</action>";
				strPath += strPartXml;
				strXml = strTestNumber;

				bNewRecord = false;
			}

			
			byte [] baOutputTimestamp = null;
			// string strOutputPath = "";



			// return:
			//		-2	时间戳不匹配
			//		-1	一般出错
			//		0	正常
			nRet = this.SearchPanel.SaveRecord(
                null,
				strPath,
				strXml,
				this.TailNumberTimestamp,
				true,
				out baOutputTimestamp,
				out strError);
			if (nRet < 0)
			{
				return -1;
			}

			this.TailNumberTimestamp = baOutputTimestamp;

			if (bNewRecord == true)
			{
				strOutputNumber = strTestNumber;
			}
			else 
			{
				strOutputNumber = strError;
			}


			return 0;

		}

        /// <summary>
        /// 种次号库名
        /// </summary>
		public string ZhongcihaoDbName
		{
			get 
			{
				if (this.dom == null)
				{
					throw (new Exception("dom尚未初始化..."));
				}

				string strError = "";

				// 得到种次号库名
				XmlNode node = dom.DocumentElement.SelectSingleNode("//dbgroup[@name='"+this.GroupName+"']");
				if (node == null)
				{
					strError = "global配置文件中name为'"+this.GroupName+"'的<dbgroup>元素没有定义...";
					throw (new Exception(strError));
				}

				string strZhongcihaoDbName = DomUtil.GetAttr(node, "zhongcihaodb");
				if (strZhongcihaoDbName == "")
				{
					strError = "global配置文件中name为'"+this.GroupName+"'的<dbgroup>元素中缺种次号库名定义属性zhongcihaodb";
					throw (new Exception(strError));
				}

				return strZhongcihaoDbName;
			}

		}

        /// <summary>
        /// 检索出尾号记录路径
        /// </summary>
        /// <param name="strPath">返回记录路径</param>
        /// <param name="strError">返回的出错信息</param>
        /// <returns>-1出错;0没有找到;1找到</returns>
		public int SearchTailNumberPath(
			out string strPath,
			out string strError)
		{
			strError = "";
			strPath = "";

			if (dom == null)
			{
				strError = "请先调用GetGlobalCfgFile()函数";
				return -1;
			}

			if (this.ClassNumber == "")
			{
				strError = "尚未指定分类号";
				return -1;
			}

			if (this.GroupName == "")
			{
				strError = "尚未指定库群";
				return -1;
			}

			if (this.ServerUrl == "")
			{
				strError = "尚未指定服务器URL";
				return -1;
			}


			string strZhongcihaoDbName = "";

			try 
			{
				strZhongcihaoDbName = this.ZhongcihaoDbName;
			}
			catch (Exception ex)
			{
                strError = ExceptionUtil.GetAutoText(ex);
				return -1;
			}

            // 2007/4/5 改造 加上了 GetXmlStringSimple()
			string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(strZhongcihaoDbName + ":" + "分类号")       // 2007/9/14
                + "'><item><word>"
				+ StringUtil.GetXmlStringSimple(this.ClassNumber)
                + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

			this.SearchPanel.BeginLoop("正在针对库 '"+strZhongcihaoDbName+"' 检索 '" + this.ClassNumber + "'");

			// 检索一个命中结果
			// return:
			//		-1	一般错误
			//		0	not found
			//		1	found
			//		>1	命中多于一条
			int nRet = this.SearchPanel.SearchOnePath(
                null,
				strQueryXml,
				out strPath,
				out strError);

			this.SearchPanel.EndLoop();

			if (nRet == -1) 
			{
				strError = "检索库 "+strZhongcihaoDbName+" 时出错: " + strError;
				return -1;
			}
			if (nRet == 0) 
			{
				return 0;	// 没有找到
			}

			if (nRet > 1)
			{
				strError = "以分类号'"+this.ClassNumber+"'检索库 "+strZhongcihaoDbName+" 时命中 " + Convert.ToString(nRet) + " 条，无法取得尾号。请修改库 '" +strZhongcihaoDbName + "' 中相应记录，确保同一类目只有一条对应的记录。";
				return -1;
			}

			return 1;
		}

	

        /// <summary>
        ///  检索获得种次号库中对应类目的尾号。此功能比较单纯，所获得的结果并不放入面板界面元素
        /// </summary>
        /// <param name="strTailNumber">返回尾号</param>
        /// <param name="strError">返回错误信息</param>
        /// <returns>-1出错;0没有找到;1找到</returns>
		public int SearchTailNumber(
            out string strTailNumber,
			out string strError)
		{
			strTailNumber = "";
			strError = "";

			string strPath = "";
			int nRet = SearchTailNumberPath(
				out strPath,
				out strError);
			if (nRet == -1)
				return -1;
			if (nRet == 0)
				return 0;


			XmlDocument tempdom = null;
			byte [] baTimeStamp = null;
			// 获取记录
			// return:
			//		-1	error
			//		0	not found
			//		1	found
			nRet = this.SearchPanel.GetRecord(
                null,
                strPath,
				out tempdom,
				out baTimeStamp,
				out strError);
			if (nRet != 1)
				return -1;

			this.TailNumberTimestamp = baTimeStamp;

			strTailNumber = DomUtil.GetAttr(tempdom.DocumentElement, "v");

			m_strTailNumber = strTailNumber;


			return 1;
		}


        /// <summary>
        /// 查找nstable定义节点
        /// </summary>
        /// <param name="nodeDatabase">database元素节点</param>
        /// <param name="nodeNsTable">返回nstable元素节点</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1出错;0没有找到定义节点;1找到</returns>
		public static int LocateNsTableCfgNode(XmlNode nodeDatabase,
			out XmlNode nodeNsTable,
			out string strError)
		{
			strError = "";
			nodeNsTable = null;

			string strNsTableName = DomUtil.GetAttr(nodeDatabase, "nstable");

			// 先从<database>内部找
			string strXPath = "";
			if (strNsTableName == "")
				strXPath = "nstable";
			else
				strXPath = "nstable[@name='"+strNsTableName+"']";

			nodeNsTable = nodeDatabase.SelectSingleNode(strXPath);

			if (nodeNsTable != null)
				return 1;
			
			// 先从<database>外部找
			if (strNsTableName == "")
				strXPath = "//nstable";
			else
				strXPath = "//nstable[@name='"+strNsTableName+"']";

			nodeNsTable = nodeDatabase.SelectSingleNode(strXPath);

			if (nodeNsTable != null)
				return 1;


			return 0;
		}

        /// <summary>
        /// 准备名字空间环境
        /// </summary>
        /// <param name="nodeNsTable">nstable节点</param>
        /// <param name="mngr">返回名字空间管理器对象</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>0</returns>
		public static int PrepareNs(
			XmlNode nodeNsTable,
			out XmlNamespaceManager mngr,
			out string strError)
		{
			strError = "";
			mngr = new XmlNamespaceManager(new NameTable());
			XmlNodeList nodes = nodeNsTable.SelectNodes("item");
			for(int i=0;i<nodes.Count;i++)
			{
				XmlNode node = nodes[i];
				string strPrefix = DomUtil.GetAttr(node, "prefix");
				string strUri = DomUtil.GetAttr(node, "uri");

				mngr.AddNamespace(strPrefix, strUri);
			}

			return 0;
		}


		int FillList(
			XmlNode nodeDatabase,
			string strFullPath,
			out string strError)
		{
			strError = "";


			ResPath respath = new ResPath(strFullPath);

			string strPath = respath.Path;
			


			XmlNode nodeNsTable = null;
			int nRet = LocateNsTableCfgNode(nodeDatabase,
				out nodeNsTable,
				out strError);
			if (nRet == -1)
				return -1;

			ListViewItem item = new ListViewItem(strPath, 0);

			string strNumber = "";
			string strTitle = "";
			string strAuthor = "";


			nRet = GetRecordProperties(
				nodeDatabase,
				nodeNsTable,
				strPath,
				out strNumber,
				out strTitle,
				out strAuthor,
				out strError);
			if (nRet == -1)
				return -1;

			item.SubItems.Add(strNumber);
			item.SubItems.Add(strTitle);
			item.SubItems.Add(strAuthor);


			this.listView_number.Items.Add(item);

			return 0;
		}

		int GetRecordProperties(
			XmlNode nodeDatabase,
			XmlNode nodeNsTable,
			string strPath,
			out string strNumber,
			out string strTitle,
			out string strAuthor,
			out string strError)
		{
			strNumber = "";
			strTitle = "";
			strAuthor = "";
			strError = "";

			XmlDocument tempdom = null;

			byte [] baTimeStamp = null;

			int nRet = this.SearchPanel.GetRecord(
                null,
				strPath,
				out tempdom,
				out baTimeStamp,
				out strError);
			if (nRet != 1)
				return -1;

			XmlNamespaceManager mngr = null;

			// 准备名字空间环境
			nRet = PrepareNs(
				nodeNsTable,
				out mngr,
				out strError);
			if (nRet == -1)
				return -1;


			string strNumberXPath = DomUtil.GetAttr(nodeDatabase, "accessnumber", "rightxpath");
			string strTitleXPath = DomUtil.GetAttr(nodeDatabase, "accessnumber", "titlexpath");
			string strAuthorXPath = DomUtil.GetAttr(nodeDatabase, "accessnumber", "authorxpath");


			try 
			{

				if (strNumberXPath != "")
				{
					XmlNode node = tempdom.DocumentElement.SelectSingleNode(strNumberXPath, mngr);
					if (node != null)
						strNumber = node.Value;
				}

				if (strTitleXPath != "")
				{
					XmlNode node = tempdom.DocumentElement.SelectSingleNode(strTitleXPath, mngr);
					if (node != null)
						strTitle = node.Value;
				}

				if (strAuthorXPath != "")
				{
					XmlNode node = tempdom.DocumentElement.SelectSingleNode(strAuthorXPath, mngr);
					if (node != null)
						strAuthor = node.Value;
				}
			}
			catch (Exception ex)
			{
                strError = ExceptionUtil.GetAutoText(ex);
				return -1;
			}

			return 0;
		}

		private void button_findServerUrl_Click(object sender, System.EventArgs e)
		{
			OpenResDlg dlg = new OpenResDlg();

			dlg.Text = "请选择服务器";
			dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
			/*
			dlg.ap = this.ap;
			dlg.ApCfgTitle = "detailform_openresdlg";
			*/
			dlg.Path = textBox_serverUrl.Text;
			dlg.Initial( this.SearchPanel.Servers,
				this.SearchPanel.Channels);	
			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			this.textBox_serverUrl.Text = dlg.Path;

		}

		private void listView_number_DoubleClick(object sender, System.EventArgs e)
		{
			if (this.OpenDetail == null)
				return;

			if (this.listView_number.SelectedItems.Count == 0)
				return;

			string [] paths = new string [this.listView_number.SelectedItems.Count];
			for(int i=0;i<this.listView_number.SelectedItems.Count;i++)
			{
				string strPath = this.listView_number.SelectedItems[i].Text;

				// paths[i] = this.textBox_serverUrl.Text + "?" + strPath;
				paths[i] = ResPath.GetRegularRecordPath(strPath);

			}

			OpenDetailEventArgs args = new OpenDetailEventArgs();
			args.Paths = paths;
			args.OpenNew = true;
		
			this.listView_number.Enabled = false;
			this.OpenDetail(this, args);
			this.listView_number.Enabled = true;
		}

		private void button_copyMaxNumber_Click(object sender, System.EventArgs e)
		{
			string strResult = "";
			string strError = "";
			int nRet = StringUtil.IncreaseLeadNumber(this.MaxNumber,
				1,
				out strResult,
				out strError);
			if (nRet == -1) 
			{
				MessageBox.Show(this, "为数字 '" +this.MaxNumber+ "' 增量时发生错误: " + strError);
				return;
			}
			Clipboard.SetDataObject(strResult);
		}

		private void button_resetTailNumber_Click(object sender, System.EventArgs e)
		{
			string strError = "";
			int nRet = SaveTailNumber(this.textBox_tailNumber.Text,
                out strError);
			if (nRet == -1)
				MessageBox.Show(this, strError);
			else 
				MessageBox.Show(this, "重设成功");
		}

		private void button_pushTailNumber_Click(object sender, System.EventArgs e)
		{
		
			string strError = "";
			string strOutputNumber = "";
				// 推动尾号
			int nRet = PushTailNumber(this.textBox_maxNumber.Text,
                out strOutputNumber,
				out strError);
			if (nRet == -1)
				MessageBox.Show(this, strError);
			else 
			{
				MessageBox.Show(this, "推动成功");
				this.textBox_tailNumber.Text = strOutputNumber;
			}
		}

		private void textBox_serverUrl_TextChanged(object sender, System.EventArgs e)
		{
            if (this.SearchPanel != null)
				this.SearchPanel.ServerUrl = this.textBox_serverUrl.Text;
		}

        /// <summary>
        /// 最大号
        /// </summary>
		public string MaxNumber
		{
			get 
			{
				if (m_strMaxNumber == null)
				{
					string strError = "";
					int nRet = GetGlobalCfgFile(out strError);
					if (nRet == -1)
						goto ERROR1;

					nRet = FillList(true, out strError);
					if (nRet == -1)
						goto ERROR1;

					return m_strMaxNumber;
				ERROR1:
					throw(new Exception(strError));

				}
				return m_strMaxNumber;

			}
			set 
			{
				this.textBox_maxNumber.Text = value;
				m_strMaxNumber = value;
			}
		}

        /// <summary>
        /// 尾号
        /// </summary>
		public string TailNumber
		{
			get 
			{
				if (m_strTailNumber == null)
				{
					string strError = "";
					int nRet = GetGlobalCfgFile(out strError);
					if (nRet == -1)
						goto ERROR1;

					nRet = PanelSearchTailNumber(out strError);
					if (nRet == -1)
						goto ERROR1;

					return m_strTailNumber;
				ERROR1:
					throw(new Exception(strError));

				}
				return m_strTailNumber;

			}
			set 
			{
				string strError = "";
				int nRet = SaveTailNumber(value,
					out strError);
				if (nRet == -1)
					throw( new Exception(strError));
				else 
					m_strTailNumber = value;	// 刷新记忆
			}
		}

	}

    /// <summary>
    /// 打开详细窗
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
	public delegate void OpenDetailEventHandler(object sender,
	OpenDetailEventArgs e);

    /// <summary>
    /// 打开详细窗事件的参数
    /// </summary>
	public class OpenDetailEventArgs: EventArgs
	{
        /// <summary>
        /// 记录全路径集合。
        /// </summary>
		public string[] Paths = null;

        /// <summary>
        /// 是否开为新窗口
        /// </summary>
		public bool OpenNew = false;
	}


	// Implements the manual sorting of items by columns.
	class ListViewItemComparer : IComparer
	{
		public ListViewItemComparer()
		{
		}

		public int Compare(object x, object y)
		{
            // 种次号字符串需要右对齐 2007/10/12
            string s1 = ((ListViewItem)x).SubItems[1].Text;
            string s2 = ((ListViewItem)y).SubItems[1].Text;

            int nMaxLength = Math.Max(s1.Length, s2.Length);
            s2 = s2.PadLeft(nMaxLength, '0');
            s1 = s1.PadLeft(nMaxLength, '0');

            return -1 * String.Compare(s1, s2);


			// return -1*String.Compare(((ListViewItem)x).SubItems[1].Text, ((ListViewItem)y).SubItems[1].Text);
		}
	}


}
