using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 查重对话框
    /// </summary>
    public class DupDlg : System.Windows.Forms.Form
    {
        OneHit m_hit = null;

        string m_strWeightList = "";  // 原始的weight定义，逗号分割的列表

        /*
        string m_strSearchStyle = "";
		int m_nCurWeight = 0;
		int m_nThreshold = 0;
		string m_strSearchReason = "";	// 检索细节信息
         * */

        Hashtable m_tableItem = new Hashtable();

        SearchPanel SearchPanel = null;

        /// <summary>
        /// 检索结束
        /// </summary>
        public AutoResetEvent EventFinish = new AutoResetEvent(false);

        bool m_bAutoBeginSearch = false;

        /// <summary>
        /// 哪些记录需要装载浏览信息列
        /// </summary>
        public LoadBrowse LoadBrowse = LoadBrowse.All;

        /// <summary>
        /// 打开详细窗
        /// </summary>
        public event OpenDetailEventHandler OpenDetail = null;

        XmlDocument domDupCfg = null;

        string m_strRecord = "";

        private System.Windows.Forms.Button button_findServerUrl;
        private System.Windows.Forms.TextBox textBox_serverUrl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_sum;

        /// <summary>
        /// 用于浏览检索命中记录的ListView
        /// </summary>
        public ListView listView_browse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_recordPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_projectName;
        private System.Windows.Forms.Button button_findProjectName;
        private System.Windows.Forms.ColumnHeader columnHeader_searchComment;
        private System.Windows.Forms.ToolTip toolTip_searchComment;
        private System.Windows.Forms.Label label_dupMessage;
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DupDlg()
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                this.EventFinish.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DupDlg));
            this.button_findServerUrl = new System.Windows.Forms.Button();
            this.textBox_serverUrl = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label_message = new System.Windows.Forms.Label();
            this.button_stop = new System.Windows.Forms.Button();
            this.button_search = new System.Windows.Forms.Button();
            this.listView_browse = new System.Windows.Forms.ListView();
            this.columnHeader_path = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_sum = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_searchComment = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_recordPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_projectName = new System.Windows.Forms.TextBox();
            this.button_findProjectName = new System.Windows.Forms.Button();
            this.toolTip_searchComment = new System.Windows.Forms.ToolTip(this.components);
            this.label_dupMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_findServerUrl
            // 
            this.button_findServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findServerUrl.Location = new System.Drawing.Point(495, 13);
            this.button_findServerUrl.Name = "button_findServerUrl";
            this.button_findServerUrl.Size = new System.Drawing.Size(42, 29);
            this.button_findServerUrl.TabIndex = 14;
            this.button_findServerUrl.Text = "...";
            this.button_findServerUrl.Click += new System.EventHandler(this.button_findServerUrl_Click);
            // 
            // textBox_serverUrl
            // 
            this.textBox_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverUrl.Location = new System.Drawing.Point(152, 15);
            this.textBox_serverUrl.Name = "textBox_serverUrl";
            this.textBox_serverUrl.Size = new System.Drawing.Size(337, 25);
            this.textBox_serverUrl.TabIndex = 13;
            this.textBox_serverUrl.TextChanged += new System.EventHandler(this.textBox_serverUrl_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(99, 15);
            this.label3.TabIndex = 12;
            this.label3.Text = "主服务器(&S):";
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(16, 338);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(521, 30);
            this.label_message.TabIndex = 17;
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(437, 120);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(100, 29);
            this.button_stop.TabIndex = 16;
            this.button_stop.Text = "停止(&S)";
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(329, 120);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(100, 29);
            this.button_search.TabIndex = 15;
            this.button_search.Text = "检索(&S)";
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // listView_browse
            // 
            this.listView_browse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_sum,
            this.columnHeader_searchComment});
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.Location = new System.Drawing.Point(16, 157);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(521, 130);
            this.listView_browse.TabIndex = 18;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_browse_MouseMove);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "记录路径";
            this.columnHeader_path.Width = 93;
            // 
            // columnHeader_sum
            // 
            this.columnHeader_sum.Text = "权值和";
            this.columnHeader_sum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_sum.Width = 70;
            // 
            // columnHeader_searchComment
            // 
            this.columnHeader_searchComment.Text = "检索详情";
            this.columnHeader_searchComment.Width = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 19;
            this.label1.Text = "源记录路径(&P):";
            // 
            // textBox_recordPath
            // 
            this.textBox_recordPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recordPath.Location = new System.Drawing.Point(152, 85);
            this.textBox_recordPath.Name = "textBox_recordPath";
            this.textBox_recordPath.Size = new System.Drawing.Size(337, 25);
            this.textBox_recordPath.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 21;
            this.label2.Text = "查重方案(&P):";
            // 
            // textBox_projectName
            // 
            this.textBox_projectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_projectName.Location = new System.Drawing.Point(152, 50);
            this.textBox_projectName.Name = "textBox_projectName";
            this.textBox_projectName.Size = new System.Drawing.Size(337, 25);
            this.textBox_projectName.TabIndex = 22;
            // 
            // button_findProjectName
            // 
            this.button_findProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findProjectName.Location = new System.Drawing.Point(495, 48);
            this.button_findProjectName.Name = "button_findProjectName";
            this.button_findProjectName.Size = new System.Drawing.Size(42, 29);
            this.button_findProjectName.TabIndex = 23;
            this.button_findProjectName.Text = "...";
            this.button_findProjectName.Click += new System.EventHandler(this.button_findProjectName_Click);
            // 
            // toolTip_searchComment
            // 
            this.toolTip_searchComment.AutomaticDelay = 5000;
            // 
            // label_dupMessage
            // 
            this.label_dupMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label_dupMessage.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_dupMessage.Location = new System.Drawing.Point(16, 297);
            this.label_dupMessage.Name = "label_dupMessage";
            this.label_dupMessage.Size = new System.Drawing.Size(527, 30);
            this.label_dupMessage.TabIndex = 24;
            this.label_dupMessage.Text = "尚未查重...";
            // 
            // DupDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 18);
            this.ClientSize = new System.Drawing.Size(553, 379);
            this.Controls.Add(this.label_dupMessage);
            this.Controls.Add(this.button_findProjectName);
            this.Controls.Add(this.textBox_projectName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_recordPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView_browse);
            this.Controls.Add(this.label_message);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_search);
            this.Controls.Add(this.button_findServerUrl);
            this.Controls.Add(this.textBox_serverUrl);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DupDlg";
            this.ShowInTaskbar = false;
            this.Text = "DupDlg";
            this.Load += new System.EventHandler(this.DupDlg_Load);
            this.Closed += new System.EventHandler(this.DupDlg_Closed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        /// <summary>
        /// 主服务器URL
        /// </summary>
        /// <remarks>用于获取cfgs/dup配置文件的服务器URL</remarks>
        public string ServerUrl
        {
            get
            {
                return textBox_serverUrl.Text;
            }
            set
            {
                domDupCfg = null;
                textBox_serverUrl.Text = value;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="searchpanel">检索面板</param>
        /// <param name="strServerUrl">主服务器URL</param>
        /// <param name="bAutoBeginSearch">当对话框打开后是否自动开始检索</param>
        public void Initial(
            SearchPanel searchpanel,
            string strServerUrl,
            bool bAutoBeginSearch)
        {
            this.SearchPanel = searchpanel;

            this.SearchPanel.InitialStopManager(this.button_stop,
                this.label_message);

            this.ServerUrl = strServerUrl;

            this.m_bAutoBeginSearch = bAutoBeginSearch;
        }

        /// <summary>
        /// 发起查重的记录
        /// </summary>
        public string Record
        {
            get
            {
                return m_strRecord;
            }
            set
            {
                m_strRecord = value;
            }
        }

        /// <summary>
        /// 发起查重的记录路径。id可以为?。主要用来模拟出keys
        /// </summary>
        public string RecordFullPath
        {
            get
            {
                return this.textBox_recordPath.Text;
            }
            set
            {
                this.textBox_recordPath.Text = value;
                this.Text = "查重: " + ResPath.GetReverseRecordPath(value);
            }
        }


        /// <summary>
        /// 发起查重的记录路径的数据库部分
        /// </summary>
        public string OriginDbFullPath
        {
            get
            {
                ResPath respath = new ResPath(this.textBox_recordPath.Text);

                return respath.Url + "?" + ResPath.GetDbName(respath.Path);
            }

        }

        /// <summary>
        /// 查重方案名
        /// </summary>
        public string ProjectName
        {
            get
            {
                return this.textBox_projectName.Text;
            }
            set
            {
                this.textBox_projectName.Text = value;
            }
        }

        /// <summary>
        /// 从主服务器上获取cfgs/dup配置文件
        /// </summary>
        /// <param name="strError">返回的出错信息</param>
        /// <returns>
        /// <value>-1出错</value>
        /// <value>0正常</value>
        /// </returns>
        int GetDupCfgFile(out string strError)
        {
            strError = "";

            if (this.domDupCfg != null)
                return 0;	// 优化

            if (this.textBox_serverUrl.Text == "")
            {
                strError = "尚未指定服务器URL";
                return -1;
            }

            string strCfgFilePath = "cfgs/dup";
            XmlDocument tempdom = null;
            // 获得配置文件
            // return:
            //		-1	error
            //		0	not found
            //		1	found
            int nRet = this.SearchPanel.GetCfgFile(
                this.textBox_serverUrl.Text,
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

            this.domDupCfg = tempdom;

            return 0;
        }

        private void DupDlg_Load(object sender, System.EventArgs e)
        {
            object[] pList = new object[] { null, null };

            if (m_bAutoBeginSearch == true)
            {
                this.BeginInvoke(new Delegate_Search(this.button_search_Click), pList);
            }

            // this.BackgroundImage = new Bitmap("f:\\cs\\dp1batch\\project_icon.bmp" );
            // this.BackgroundImage = GetBackImage();

            // this.listView_browse.BackgroundImage = "f:\\cs\\dp1batch\\project_icon.bmp";

            // this.listView_browse.BackImage = GetBackImage();

        }

        delegate void Delegate_Search(object sender, EventArgs e);

        /// <summary>
        /// 等待检索结束
        /// </summary>
        public void WaitSearchFinish()
        {
            for (; ; )
            {
                Application.DoEvents();
                bool bRet = this.EventFinish.WaitOne(10, true);
                if (bRet == true)
                    break;
            }
        }

        private void DupDlg_Closed(object sender, System.EventArgs e)
        {
            EventFinish.Set();
        }

        private void button_search_Click(object sender, System.EventArgs e)
        {
            string strError = "";

            int nRet = DoSearch(out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
            }

            /*
            EventFinish.Reset();
            try 
            {

                this.listView_browse.Items.Clear();
                this.m_tableItem.Clear();

                if (this.ServerUrl == "")
                {
                    strError = "主服务器URL尚未指定";
                    goto ERROR1;
                }
                if (this.ProjectName == "")
                {
                    strError = "查重方案名尚未指定";
                    goto ERROR1;
                }
                if (this.RecordFullPath == "")
                {
                    strError = "源记录路径尚未指定";
                    goto ERROR1;
                }
                if (this.Record == "")
                {
                    strError = "源记录内容尚未指定";
                    goto ERROR1;
                }

                // 从服务器上获取dup配置文件
                int nRet = GetDupCfgFile(out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 检查project name是否存在
                XmlNode nodeProject = GetProjectNode(this.ProjectName,
                    out strError);
                if (nodeProject == null)
                    goto ERROR1;

                // 分析源记录路径
                ResPath respath = new ResPath(this.RecordFullPath);

                ArrayList aLine = null;	// AccessKeyInfo对象数组
                // 获得keys
                // 模拟创建检索点
                // return:
                //		-1	一般出错
                //		0	正常
                nRet = this.SearchPanel.GetKeys(
                    respath.Url,
                    respath.Path,
                    this.Record,
                    out aLine,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = 	LoopSearch(
                    nodeProject,
                    aLine,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 排序
                this.SearchPanel.BeginLoop("正在排序");
                try 
                {
                    this.listView_browse.ListViewItemSorter = new ListViewItemComparer();
                }
                finally 
                {
                    this.SearchPanel.EndLoop();
                }


                // 获得浏览信息
                this.SearchPanel.BeginLoop("正在获取浏览列信息 ...");
                try 
                {
                    nRet = GetBrowseColumns(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally 
                {
                    this.SearchPanel.EndLoop();
                }


                // MessageBox.Show(this, "OK");	// 汇报查重情况

                return;
            }
            finally 
            {
                EventFinish.Set();
            }
			
		
            ERROR1:
                MessageBox.Show(this, strError);
            */
        }


        /// <summary>
        /// 检索
        /// </summary>
        /// <param name="strError">返回的错误信息</param>
        /// <returns>-1出错;0正常</returns>
        public int DoSearch(out string strError)
        {
            strError = "";

            EventFinish.Reset();
            try
            {

                this.listView_browse.Items.Clear();
                this.m_tableItem.Clear();

                if (this.ServerUrl == "")
                {
                    strError = "主服务器URL尚未指定";
                    goto ERROR1;
                }
                if (this.ProjectName == "")
                {
                    strError = "查重方案名尚未指定";
                    goto ERROR1;
                }
                if (this.RecordFullPath == "")
                {
                    strError = "源记录路径尚未指定";
                    goto ERROR1;
                }
                if (this.Record == "")
                {
                    strError = "源记录内容尚未指定";
                    goto ERROR1;
                }

                // 从服务器上获取dup配置文件
                int nRet = GetDupCfgFile(out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (this.ProjectName == "{default}")
                {
                    ResPath respathtemp = new ResPath(this.RecordFullPath);

                    string strOriginDbFullPath = respathtemp.Url + "?" + ResPath.GetDbName(respathtemp.Path);
                    string strDefaultProjectName = "";
                    nRet = GetDefaultProjectName(strOriginDbFullPath,
                        out strDefaultProjectName,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "查重发起库 '" + strOriginDbFullPath + "' 尚未定义缺省查重方案参数(需在dup配置文件中用<default>元素定义)。\r\n或可用'查重方案'textbox右边的'...'按钮指定好一个实在的查重方案名后，再行查重。";
                        goto ERROR1;
                    }
                    Debug.Assert(nRet == 1, "");
                    this.ProjectName = strDefaultProjectName;
                }

                // 检查project name是否存在
                XmlNode nodeProject = GetProjectNode(this.ProjectName,
                    out strError);
                if (nodeProject == null)
                    goto ERROR1;

                // 分析源记录路径
                ResPath respath = new ResPath(this.RecordFullPath);

                List<AccessKeyInfo> aLine = null;	// AccessKeyInfo对象数组
                // 获得keys
                // 模拟创建检索点
                // return:
                //		-1	一般出错
                //		0	正常
                nRet = this.SearchPanel.GetKeys(
                    respath.Url,
                    respath.Path,
                    this.Record,
                    out aLine,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                nRet = LoopSearch(
                    nodeProject,
                    aLine,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 排序
                this.SearchPanel.BeginLoop("正在排序");
                try
                {
                    this.listView_browse.ListViewItemSorter = new ListViewItemComparer();
                }
                finally
                {
                    this.SearchPanel.EndLoop();
                }

                SetDupState();

                // 获得浏览信息
                this.SearchPanel.BeginLoop("正在获取浏览列信息 ...");
                try
                {
                    nRet = GetBrowseColumns(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                }
                finally
                {
                    this.SearchPanel.EndLoop();
                }
                return 0;
            }
            finally
            {
                EventFinish.Set();
            }

        ERROR1:
            return -1;
        }


        /// <summary>
        /// 获得查重结果：记录全路径的集合
        /// </summary>
        public string[] DupPaths
        {
            get
            {
                int i;
                ArrayList aPath = new ArrayList();
                for (i = 0; i < this.listView_browse.Items.Count; i++)
                {
                    string strText = this.listView_browse.Items[i].SubItems[1].Text;

                    if (strText.Length > 0 && strText[0] == '*')
                    {
                        aPath.Add(ResPath.GetRegularRecordPath(this.listView_browse.Items[i].Text));
                    }
                    else
                        break;
                }

                if (aPath.Count == 0)
                    return new string[0];

                string[] result = new string[aPath.Count];
                for (i = 0; i < aPath.Count; i++)
                {
                    result[i] = (string)aPath[i];
                }

                return result;
            }
        }

        // 设置查重状态
        void SetDupState()
        {
            int nCount = 0;
            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                string strText = this.listView_browse.Items[i].SubItems[1].Text;

                if (strText.Length > 0 && strText[0] == '*')
                    nCount++;
                else
                    break;
            }

            if (nCount > 0)
                this.label_dupMessage.Text = "有 " + Convert.ToString(nCount) + " 条重复记录。";
            else
                this.label_dupMessage.Text = "没有重复记录。";

        }

        // 获得一个发起库对应的缺省查重方案名
        int GetDefaultProjectName(string strFromDbFullPath,
            out string strDefaultProjectName,
            out string strError)
        {
            strDefaultProjectName = "";
            strError = "";

            if (this.domDupCfg == null)
            {
                strError = "配置文件dom尚未初始化";
                return -1;
            }

            ResPath respath = new ResPath(strFromDbFullPath);


            XmlNode node = this.domDupCfg.SelectSingleNode("//default[@origin='" + strFromDbFullPath + "']");
            if (node == null)
            {
                node = this.domDupCfg.SelectSingleNode("//default[@origin='" + respath.Path + "']");
            }

            if (node == null)
                return 0;	// not found

            strDefaultProjectName = DomUtil.GetAttr(node, "project");

            return 1;
        }

        // 循环检索
        int LoopSearch(
            XmlNode nodeProject,
            List<AccessKeyInfo> aLine,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (nodeProject == null)
            {
                strError = "nodeProject参数不能为null";
                return -1;
            }

            Hashtable threshold_table = new Hashtable();    // 数据库名和阈值的对照表
            Hashtable keyscount_table = new Hashtable();    // 发起记录的每个from所包含的key的数目 对照表。hashtable key的形态为strDbName + "|" + strFrom

            XmlNodeList databases = nodeProject.SelectNodes("database");

            // <database>循环
            for (int i = 0; i < databases.Count; i++)
            {
                XmlNode database = databases[i];

                string strName = DomUtil.GetAttr(database, "name");
                if (strName == "")
                    continue;

                string strThreshold = DomUtil.GetAttr(database, "threshold");

                int nThreshold = 0;
                try
                {
                    nThreshold = Convert.ToInt32(strThreshold);
                }
                catch
                {
                    strError = "name为 '" + strName + "' 的<database>元素内threshold属性值 '" + strThreshold + "' 格式不正确，应为纯数字";
                    return -1;
                }

                threshold_table[strName] = nThreshold;

                string strUrl = "";
                string strDbName = "";
                // 分离出URL和库名
                nRet = strName.IndexOf("?");
                if (nRet == -1)
                {
                    strUrl = this.ServerUrl;	// 当前主服务器
                    strDbName = strName;
                }
                else
                {
                    strUrl = strName.Substring(0, nRet);
                    strDbName = strName.Substring(nRet + 1);
                }

                XmlNodeList accesspoints = database.SelectNodes("accessPoint");
                // <accessPoint>循环
                for (int j = 0; j < accesspoints.Count; j++)
                {
                    XmlNode accesspoint = accesspoints[j];

                    string strFrom = DomUtil.GetAttr(accesspoint, "name");

                    // 获得from所对应的key
                    List<string> keys = GetKeysByFrom(aLine,
                        strFrom);
                    if (keys.Count == 0)
                        continue;

                    keyscount_table[strDbName + "|" + strFrom] = keys.Count;

                    string strWeight = DomUtil.GetAttr(accesspoint, "weight");
                    string strSearchStyle = DomUtil.GetAttr(accesspoint, "searchStyle");

                    /*
					int nWeight = 0;
					try 
					{
						nWeight = Convert.ToInt32(strWeight);
					}
					catch
					{
						// 警告定义问题?
					}*/

                    for (int k = 0; k < keys.Count; k++)
                    {
                        string strKey = (string)keys[k];
                        if (strKey == "")
                            continue;

                        // 检索一个from
                        nRet = SearchOneFrom(
                            strUrl,
                            strDbName,
                            strFrom,
                            strKey,
                            strSearchStyle,
                            strWeight,
                            // nThreshold,
                            5000,
                            out strError);
                        if (nRet == -1)
                        {
                            // ??? 警告检索错误?
                        }
                    }

                }

                // 处理完一个数据库了
            }

            // 将listview中每行显示出来
            Color color = Color.FromArgb(255, 255, 200);

            for (int i = 0; i < this.listView_browse.Items.Count; i++)
            {
                ListViewItem item = this.listView_browse.Items[i];
                ItemInfo info = (ItemInfo)item.Tag;
                Debug.Assert(info != null, "");

                // 获得库名
                ResPath respath = new ResPath(ResPath.GetRegularRecordPath(item.Text));
                string strDbName = respath.GetDbName();


                int nWeight = AddWeight(
                    keyscount_table,
                    strDbName,
                    info.Hits);

                // 获得当前库的threshold
                int nThreshold = (int)threshold_table[strDbName];

                string strNumber = nWeight.ToString();
                if (nWeight >= nThreshold)
                {
                    strNumber = "*" + strNumber;
                    item.BackColor = color;
                }

                ListViewUtil.ChangeItemText(item, 1, strNumber);
                ListViewUtil.ChangeItemText(item, 2, BuildComment(info.Hits));
            }

            return 0;
        }

        // 先按照各个渠道累加各自的weight，然后算出总weight
        static int AddWeight(
            Hashtable keyscount_table,
            string strDbName,
            List<OneHit> hits)
        {
            Hashtable weight_table = new Hashtable();

            int nWeight = 0;    // 没有特殊检索风格的权值累计
            // 累加分数
            for (int i = 0; i < hits.Count; i++)
            {
                OneHit hit = hits[i];

                if (StringUtil.IsInList("average", hit.SearchStyle) == true)
                {
                    OneFromWeights one = (OneFromWeights)weight_table[hit.From];
                    if (one == null)
                    {
                        one = new OneFromWeights();
                        one.From = hit.From;

                        weight_table[hit.From] = one;
                    }

                    one.Weights += hit.Weight;
                    one.Hits++;
                }
                else
                    nWeight += hit.Weight;
            }

            // 累加特殊风格的weights
            foreach (string key in weight_table.Keys)
            {
                OneFromWeights one = (OneFromWeights)weight_table[key];
                Debug.Assert(one != null, "");

                // 获得该from的keyscount
                int nKeysCount = (int)keyscount_table[strDbName + "|" + one.From];

                Debug.Assert(nKeysCount != 0, "");    // 防止被0除
                nWeight += one.Weights / nKeysCount;
            }

            return nWeight;
        }

        static string BuildComment(List<OneHit> hits)
        {
            string strResult = "";
            for (int i = 0; i < hits.Count; i++)
            {
                OneHit hit = hits[i];

                if (String.IsNullOrEmpty(strResult) == false)
                    strResult += ";";
                strResult += "key='" + hit.Key + "', from='" + hit.From + "', weight=" + hit.Weight.ToString();
                if (String.IsNullOrEmpty(hit.SearchStyle) == false)
                    strResult += ", searchStyle=" + hit.SearchStyle;
            }

            return strResult;
        }

        // 一个渠道的权值总和，以及事项数
        class OneFromWeights
        {
            public string From = "";    // 检索途径名
            public int Weights = 0; // 总的weight
            public int Hits = 0;    // 命中事项数(次数)
        }

        // 从模拟keys中根据from获得对应的key
        List<string> GetKeysByFrom(List<AccessKeyInfo> aLine,
            string strFromName)
        {
            List<string> aResult = new List<string>();
            for (int i = 0; i < aLine.Count; i++)
            {
                AccessKeyInfo info = aLine[i];
                if (info.FromName == strFromName)
                    aResult.Add(info.Key);
            }

            return aResult;
        }

        // 从字符串中挑选出XML检索式专用的search style
        // 也就是 exact left middle right。如果缺省，认为等于exact
        // 如果有多个可用的值，则第一个起作用
        static string GetFirstQuerySearchStyle(string strText)
        {
            string[] parts = strText.Split(new char[] { ',' });
            for (int i = 0; i < parts.Length; i++)
            {
                string strStyle = parts[i].Trim().ToLower();
                if (strStyle == "exact"
                    || strStyle == "left"
                    || strStyle == "middle"
                    || strStyle == "right")
                    return strStyle;
            }

            return "exact";
        }

        // 针对一个from进行检索
        int SearchOneFrom(
            string strServerUrl,
            string strDbName,
            string strFrom,
            string strKey,
            string strSearchStyle,
            string strWeight,
            // int nThreshold,
            long nMax,
            out string strError)
        {

            this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(BrowseRecordNoColsCallBack);
            this.SearchPanel.BrowseRecord += new BrowseRecordEventHandler(BrowseRecordNoColsCallBack);

            try
            {

                /*
                if (strSearchStyle == "")
                    strSearchStyle = "exact";
                 * */

                /*
                this.m_strSearchStyle = strSearchStyle; // 2009/3/2
				this.m_nCurWeight = nWeight;	// 为事件处理函数所预备
				this.m_nThreshold = nThreshold;
                 * */


                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                string strQueryXml = "<target list='"
                    + StringUtil.GetXmlStringSimple(strDbName + ":" + strFrom)       // 2007/9/14
                    + "'><item><word>"
                    + StringUtil.GetXmlStringSimple(strKey)
                    + "</word><match>" + GetFirstQuerySearchStyle(strSearchStyle) + "</match><relation>=</relation><dataType>string</dataType><maxCount>" + Convert.ToString(nMax) + "</maxCount></item><lang>zh</lang></target>";

                this.SearchPanel.BeginLoop("正在针对库 '" + strDbName + "' 检索 '" + strKey + "'");

                this.m_hit = new OneHit();
                this.m_hit.Key = strKey;
                this.m_hit.From = strFrom;
                this.m_hit.SearchStyle = strSearchStyle;

                this.m_strWeightList = strWeight;
                // this.m_strSearchReason = "key='" + strKey + "', from='" +strFrom + "', weight=" + Convert.ToString(nWeight);

                long lRet = 0;

                try
                {
                    // return:
                    //		-2	用户中断
                    //		-1	一般错误
                    //		0	未命中
                    //		>=1	正常结束，返回命中条数
                    lRet = this.SearchPanel.SearchAndBrowse(
                        strServerUrl,
                        strQueryXml,
                        false,
                        out strError);

                    return (int)lRet;
                }
                finally
                {
                    this.SearchPanel.EndLoop();
                }
            }
            finally
            {
                this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(BrowseRecordNoColsCallBack);
            }
        }


        void BrowseRecordNoColsCallBack(object sender, BrowseRecordEventArgs e)
        {
            string strError = "";

            if (e.FullPath == this.RecordFullPath)
                return;	// 当前记录自己并不要装入浏览窗

            int nRet = FillList(
                e.FullPath,
                this.m_hit,
                this.m_strWeightList,
                /*
                this.m_strSearchStyle,
				this.m_nCurWeight,
				this.m_nThreshold,
				this.m_strSearchReason,
                 * */
                out strError);
            if (nRet == -1)
            {
                e.Cancel = true;
                e.ErrorInfo = strError;
            }
        }

        // 填充列表
        // parameters:
        //      strReason   检索过程注释
        //      hit_param   携带了参数，但是要被复制后，将新对象进入队列
        int FillList(
            string strFullPath,
            OneHit hit_param,
            string strWeightList,
            /*
            string strSearchStyle,
			int nCurWeight,
			int nThreshold,
			string strReason,
             * */
            out string strError)
        {
            strError = "";

            // Color color = Color.FromArgb(255,255,200);

            // string strNumber = "";

            OneHit hit = new OneHit(hit_param);
            /*
            hit.From = hit_param.From;
            hit.Key = hit_param.Key;
            hit.SearchStyle = hit_param.SearchStyle;
            hit.Weight = hit_param.Weight;
             * */

            ItemInfo info = null;

            string strPath = ResPath.GetReverseRecordPath(strFullPath);

            // 根据path寻找已经存在的item
            ListViewItem item = (ListViewItem)m_tableItem[strPath];
            if (item == null)
            {
                item = new ListViewItem(strPath, 0);

                /*
				strNumber = Convert.ToString(nCurWeight);

				if (nCurWeight >= nThreshold)
				{
					strNumber = "*" + strNumber;
					item.BackColor = color;
				}

				item.SubItems.Add(strNumber);
				item.SubItems.Add(strReason);
                 * */

                this.listView_browse.Items.Add(item);
                m_tableItem[strPath] = item;

                info = new ItemInfo();
                item.Tag = info;
                info.Hits.Add(hit);
            }
            else
            {
                /*
				// 把已经存在的weight值加上本次新值
				if (nCurWeight != 0)
				{
					string strExistWeight = item.SubItems[1].Text;

					// 去掉可能存在的引导'*'字符
					if (strExistWeight.Length > 0 && strExistWeight[0] == '*')
						strExistWeight = strExistWeight.Substring(1);

					int nOldValue = 0;
					try 
					{
						nOldValue = Convert.ToInt32(strExistWeight);
					}
					catch 
					{
					}


					int nValue = nOldValue + nCurWeight;

					strNumber = Convert.ToString(nValue);

					if (nValue >= nThreshold)
					{
						strNumber = "*" + strNumber;
						if (nOldValue < nThreshold)
							item.BackColor = color;
					}

					item.SubItems[1].Text = strNumber;
				}

				string strOldReason = item.SubItems[2].Text;

				if (strOldReason != "")
					item.SubItems[2].Text += ";";

				item.SubItems[2].Text += strReason;
                */


                info = (ItemInfo)item.Tag;
                Debug.Assert(info != null, "");

                info.Hits.Add(hit);
            }

            int nHitIndex = GetHitIndex(info.Hits, hit.From);

            Debug.Assert(nHitIndex >= 0, "");

            // 获得具体一次命中的weight值
            // 如果列表中的数字不够nHitIndex那么多个，则取最后一个的值
            // parameters:
            //      strWeightList   原始的weight属性定义，形态为"100,50,20"或者"50"
            //      nHitIndex   当前命中的这一次为总共命中的多少次
            hit.Weight = GetWeight(strWeightList,
                nHitIndex);


            return 0;
        }

        // 获得特定from下，最后一次命中的index
        // return:
        //      -1  not found
        //      其他    found
        static int GetHitIndex(List<OneHit> hits,
            string strFrom)
        {
            int j = -1;
            for (int i = 0; i < hits.Count; i++)
            {
                OneHit hit = hits[i];
                if (hit.From == strFrom)
                    j++;
            }

            return j;
        }

        // 获得具体一次命中的weight值
        // 如果列表中的数字不够nHitIndex那么多个，则取最后一个的值
        // parameters:
        //      strWeightList   原始的weight属性定义，形态为"100,50,20"或者"50"
        //      nHitIndex   当前命中的这一次为总共命中的多少次
        static int GetWeight(string strWeightList,
            int nHitIndex)
        {
            Debug.Assert(nHitIndex >= 0, "");

            string[] parts = strWeightList.Split(new char[] { ',' });
            Debug.Assert(parts.Length >= 1, "");

            string strWeight = "";

            if (parts.Length - 1 < nHitIndex)
                strWeight = parts[parts.Length - 1].Trim();
            else
                strWeight = parts[nHitIndex].Trim();

            try
            {
                return Convert.ToInt32(strWeight);
            }
            catch
            {
                return 0;
            }
        }

        /*
        // parameters:
        //      strReason   检索过程注释
		int FillList(
			string strFullPath,
            string strSearchStyle,
			int nCurWeight,
			int nThreshold,
			string strReason,
			out string strError)
		{
			strError = "";

			Color color = Color.FromArgb(255,255,200);

			string strNumber = "";

			string strPath = ResPath.GetReverseRecordPath(strFullPath);

			// 根据path寻找已经存在的item
			ListViewItem item = (ListViewItem)m_tableItem[strPath];
			if (item == null)
			{
				item = new ListViewItem(strPath, 0);

				strNumber = Convert.ToString(nCurWeight);

				if (nCurWeight >= nThreshold)
				{
					strNumber = "*" + strNumber;
					item.BackColor = color;
				}

				item.SubItems.Add(strNumber);
				item.SubItems.Add(strReason);

				this.listView_browse.Items.Add(item);

				m_tableItem[strPath] = item;
			}
			else 
			{
				// 把已经存在的weight值加上本次新值
				if (nCurWeight != 0)
				{
					string strExistWeight = item.SubItems[1].Text;

					// 去掉可能存在的引导'*'字符
					if (strExistWeight.Length > 0 && strExistWeight[0] == '*')
						strExistWeight = strExistWeight.Substring(1);

					int nOldValue = 0;
					try 
					{
						nOldValue = Convert.ToInt32(strExistWeight);
					}
					catch 
					{
					}


					int nValue = nOldValue + nCurWeight;

					strNumber = Convert.ToString(nValue);

					if (nValue >= nThreshold)
					{
						strNumber = "*" + strNumber;
						if (nOldValue < nThreshold)
							item.BackColor = color;
					}

					item.SubItems[1].Text = strNumber;
				}

				string strOldReason = item.SubItems[2].Text;

				if (strOldReason != "")
					item.SubItems[2].Text += ";";

				item.SubItems[2].Text += strReason;

			}

			return 0;
		}
         * */

        private void button_stop_Click(object sender, System.EventArgs e)
        {
            if (this.SearchPanel != null)
                this.SearchPanel.DoStopClick();

        }

        // 获得<project>配置元素
        XmlNode GetProjectNode(string strProjectName,
            out string strError)
        {
            strError = "";

            if (this.domDupCfg == null)
            {
                strError = "请先调用GetDupCfgFile()获取配置文件";
                return null;
            }

            XmlNode node = this.domDupCfg.DocumentElement.SelectSingleNode("//project[@name='" + strProjectName + "']");
            if (node == null)
                strError = "查重方案 '" + strProjectName + "' 不存在";
            return node;
        }

        private void textBox_serverUrl_TextChanged(object sender, System.EventArgs e)
        {
            if (this.SearchPanel != null)
                this.SearchPanel.ServerUrl = this.textBox_serverUrl.Text;

        }

        private void listView_browse_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ListViewItem selection = this.listView_browse.GetItemAt(e.X, e.Y);

            if (selection != null)
            {
                string strText = "";
                int nRet = ListViewUtil.ColumnHitTest(this.listView_browse,
                    e.X);
                if (nRet == 0)
                    strText = selection.SubItems[0].Text;
                else if (nRet == 1 || nRet == 2)
                    strText = selection.SubItems[0].Text + "\r\n------\r\n" +
                    selection.SubItems[2].Text.Replace(";", ";\r\n");

                this.toolTip_searchComment.SetToolTip(this.listView_browse,
                    strText);
            }
            else
                this.toolTip_searchComment.SetToolTip(this.listView_browse, null);

        }

        int GetBrowseColumns(out string strError)
        {
            strError = "";

            if (this.LoadBrowse == LoadBrowse.None)
                return 0;


            ArrayList aFullPath = new ArrayList();
            int i = 0;
            for (i = 0; i < this.listView_browse.Items.Count; i++)
            {
                string strFullPath = this.listView_browse.Items[i].Text;

                string strNumber = this.listView_browse.Items[i].SubItems[1].Text;

                if (strNumber.Length > 0 && strNumber[0] == '*')
                {
                }
                else
                {
                    if (this.LoadBrowse == LoadBrowse.Dup)
                        continue;
                }

                aFullPath.Add(ResPath.GetRegularRecordPath(strFullPath));
            }

            string[] fullpaths = new string[aFullPath.Count];
            for (i = 0; i < fullpaths.Length; i++)
            {
                fullpaths[i] = (string)aFullPath[i];
            }


            this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(BrowseRecordColsCallBack);
            this.SearchPanel.BrowseRecord += new BrowseRecordEventHandler(BrowseRecordColsCallBack);


            try
            {
                // 获取浏览记录
                // return:
                //		-1	error
                //		0	not found
                //		1	found
                int nRet = this.SearchPanel.GetBrowseRecord(fullpaths,
                    false,
                    "cols",
                    out strError);
                if (nRet == -1)
                    return -1;
            }
            finally
            {
                this.SearchPanel.BrowseRecord -= new BrowseRecordEventHandler(BrowseRecordColsCallBack);
            }

            return 0;
        }


        void BrowseRecordColsCallBack(object sender, BrowseRecordEventArgs e)
        {
            ListViewUtil.EnsureColumns(this.listView_browse,
                3 + e.Cols.Length,
                200);

            ListViewItem item = (ListViewItem)this.m_tableItem[ResPath.GetReverseRecordPath(e.FullPath)];
            if (item == null)
            {
                e.Cancel = true;
                e.ErrorInfo = "路径为 '" + e.FullPath + "' 的事项在listview中不存在...";
                return;
            }


            for (int j = 0; j < e.Cols.Length; j++)
            {
                ListViewUtil.ChangeItemText(item,
                    j + 3,
                    e.Cols[j]);
            }
        }

        private void listView_browse_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.OpenDetail == null)
                return;

            string[] paths = BrowseList.GetSelectedRecordPaths(this.listView_browse, true);

            if (paths.Length == 0)
                return;
            /*
            string [] paths = new string [this.listView_browse.SelectedItems.Count];
            for(int i=0;i<this.listView_browse.SelectedItems.Count;i++)
            {
                string strPath = this.listView_browse.SelectedItems[i].Text;

                // paths[i] = this.textBox_serverUrl.Text + "?" + strPath;
                paths[i] = ResPath.GetRegularRecordPath(strPath);

            }
            */

            OpenDetailEventArgs args = new OpenDetailEventArgs();
            args.Paths = paths;
            args.OpenNew = true;

            this.listView_browse.Enabled = false;
            this.OpenDetail(this, args);
            this.listView_browse.Enabled = true;
        }

        private void button_findServerUrl_Click(object sender, System.EventArgs e)
        {
            OpenResDlg dlg = new OpenResDlg();

            dlg.Text = "请选择主服务器";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
            dlg.ap = this.SearchPanel.ap;
            dlg.ApCfgTitle = "findServerUrl_openresdlg";
            dlg.MultiSelect = false;
            dlg.Path = this.textBox_serverUrl.Text;
            dlg.Initial(this.SearchPanel.Servers,
                this.SearchPanel.Channels);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            textBox_serverUrl.Text = dlg.Path;
        }

        private void button_findProjectName_Click(object sender, System.EventArgs e)
        {
            FindProjectName();
        }

        /// <summary>
        /// 打开"获得查重方案名"对话框,获得查重方案名和主服务器URL
        /// </summary>
        /// <returns>DialogResult.OK对话框由OK按钮关闭;DialogResult.Cancel对话框由Cancel按钮关闭</returns>
        public DialogResult FindProjectName()
        {
            GetDupProjectNameDlg dlg = new GetDupProjectNameDlg();

            dlg.ServerUrl = this.ServerUrl;
            dlg.SearchPanel = this.SearchPanel;
            dlg.DomDupCfg = this.domDupCfg;
            dlg.ProjectName = this.textBox_projectName.Text;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return dlg.DialogResult;

            this.ServerUrl = dlg.ServerUrl;
            this.textBox_projectName.Text = dlg.ProjectName;
            return dlg.DialogResult;
        }


        // Implements the manual sorting of items by columns.
        class ListViewItemComparer : IComparer
        {
            public ListViewItemComparer()
            {
            }

            public int Compare(object x, object y)
            {
                /*
				string strNumber1 = ((ListViewItem)x).SubItems[1].Text;
				string strNumber2 = ((ListViewItem)y).SubItems[1].Text;
                 * */

                // 2009/6/30 changed
                // sorter具备后，可能ListView.Items.Add()插入只有一列的新行，就会引起排序动作，而此时相关行的[1]列并不具备。
                string strNumber1 = ListViewUtil.GetItemText(((ListViewItem)x), 1);
                string strNumber2 = ListViewUtil.GetItemText(((ListViewItem)y), 1);


                // 规整一下
                if (strNumber1.Length > 0)
                {
                    if (strNumber1[0] == '*')
                    {
                        strNumber1 = strNumber1.Remove(0, 1);
                    }
                }

                if (strNumber2.Length > 0)
                {
                    if (strNumber2[0] == '*')
                    {
                        strNumber2 = strNumber2.Remove(0, 1);
                    }
                }

                int nNumber1 = 0;
                int nNumber2 = 0;

                try
                {
                    nNumber1 = Convert.ToInt32(strNumber1);
                }
                catch
                {
                }

                try
                {
                    nNumber2 = Convert.ToInt32(strNumber2);
                }
                catch
                {
                }

                return -1 * (nNumber1 - nNumber2);
            }
        }

    }

    /// <summary>
    /// 浏览框中哪些行需要装载浏览信息列
    /// </summary>
    public enum LoadBrowse
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,

        /// <summary>
        /// 超过阈值的行
        /// </summary>
        Dup = 1,

        /// <summary>
        /// 全部都不
        /// </summary>
        None = 2,
    }

    // 一次命中的信息
    class OneHit
    {
        public string Key = ""; // 检索词
        public string From = "";    // 检索途径
        public int Weight = 0;  // 命中的分数值
        public string SearchStyle = ""; // 检索风格

        public OneHit()
        {
        }

        public OneHit(OneHit hit_param)
        {
            this.From = hit_param.From;
            this.Key = hit_param.Key;
            this.SearchStyle = hit_param.SearchStyle;
            this.Weight = hit_param.Weight;
        }
    }

    class ItemInfo
    {
        public List<OneHit> Hits = new List<OneHit>();
    }
}
