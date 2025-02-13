using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;

using System.Runtime.InteropServices;

namespace dp2rms
{
    /// <summary>
    /// Summary description for SearchForm.
    /// </summary>
    public class SearchForm : System.Windows.Forms.Form
    {
        public Hashtable ParamTable = new Hashtable();

        public string Lang = "zh";

        RmsChannel _channel = null;	// 临时使用的channel对象

        public AutoResetEvent eventClose = new AutoResetEvent(false);

        RmsChannelCollection Channels = new RmsChannelCollection();	// 拥有

        DigitalPlatform.Stop stop = null;

        ArrayList m_aComplexServer = new ArrayList();	// 复杂检索中，服务器地址URL字符串的数组
        private System.Windows.Forms.TabControl tabControl_query;
        private BrowseList listView_browse;
        private System.Windows.Forms.TabPage tabPage_querySimple;
        private System.Windows.Forms.TabPage tabPage_queryAdvance;
        private System.Windows.Forms.TabPage tabPage_queryXml;
        private ResTree treeView_simpleQueryResTree;
        private System.Windows.Forms.TextBox textBox_simpleQueryWord;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_simpleQuerySearch;
        private System.Windows.Forms.Panel panel_simpleQueryOther;
        private System.Windows.Forms.TextBox textBox_simpleQuery_comment;
        private System.Windows.Forms.Button button_simpleQueryProperty;
        private System.Windows.Forms.TextBox textBox_xmlQueryString;
        private System.Windows.Forms.Button button_xmlQuerySearch;
        private System.Windows.Forms.TextBox textBox_complex_word1;
        private System.Windows.Forms.ComboBox comboBox_complex_db1;
        private System.Windows.Forms.ComboBox comboBox_complex_from1;
        private System.Windows.Forms.ComboBox comboBox_complex_from2;
        private System.Windows.Forms.ComboBox comboBox_complex_db2;
        private System.Windows.Forms.TextBox textBox_complex_word2;
        private System.Windows.Forms.ComboBox comboBox_complex_logic2;
        private System.Windows.Forms.ComboBox comboBox_complex_logic3;
        private System.Windows.Forms.ComboBox comboBox_complex_from3;
        private System.Windows.Forms.ComboBox comboBox_complex_db3;
        private System.Windows.Forms.TextBox textBox_complex_word3;
        private System.Windows.Forms.ComboBox comboBox_complex_logic4;
        private System.Windows.Forms.ComboBox comboBox_complex_from4;
        private System.Windows.Forms.ComboBox comboBox_complex_db4;
        private System.Windows.Forms.TextBox textBox_complex_word4;
        private System.Windows.Forms.Button button_complex_server1;
        private System.Windows.Forms.Button button_complex_server2;
        private System.Windows.Forms.Button button_complex_server3;
        private System.Windows.Forms.Button button_complex_server4;
        private System.Windows.Forms.ImageList imageList_complex_serverButton;
        private System.Windows.Forms.Button button_complexQuerySearch;
        private System.Windows.Forms.TextBox textBox_complexQuery_comment;
        private System.Windows.Forms.ToolTip toolTip_serverUrl;
        private Button button_test;
        private SplitContainer splitContainer_simpleQeury;
        private SplitContainer splitContainer_main;
        private SplitContainer splitContainer_xml;
        private TextBox textBox_xmlQuery_comment;
        private System.ComponentModel.IContainer components;

        public SearchForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            ListViewProperty prop = new ListViewProperty();
            this.listView_browse.Tag = prop;
            // 第一列特殊，记录路径
            prop.SetSortStyle(0, ColumnSortStyle.LongRecPath);
            prop.GetColumnTitles -= new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.GetColumnTitles += new GetColumnTitlesEventHandler(prop_GetColumnTitles);
            prop.ParsePath -= new ParsePathEventHandler(prop_ParsePath);
            prop.ParsePath += new ParsePathEventHandler(prop_ParsePath);
        }

        void prop_ParsePath(object sender, ParsePathEventArgs e)
        {
            string strPath = ResPath.GetRegularRecordPath(e.Path);
            ResPath respath = new ResPath(strPath);

            e.DbName = respath.Url + "|" + respath.GetDbName();
        }

        void prop_GetColumnTitles(object sender, GetColumnTitlesEventArgs e)
        {
            if (e.DbName == "<blank>")
            {
                e.ColumnTitles = new ColumnPropertyCollection();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
#if NO
                e.ColumnTitles = new List<string>();
                e.ColumnTitles.Add("检索点");
                e.ColumnTitles.Add("数量");
#endif
                return;
            }

            // e.ColumnTitles = this.MainForm.GetBrowseColumnNames(e.DbName);

            // e.ColumnTitles = new List<string>();
            List<string> titles = this.GetBrowseColumnNames(e.DbName);
            if (titles == null) // 意外的数据库名
                return;
            e.ColumnTitles = new ColumnPropertyCollection();
            // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件
            foreach (string title in titles)
            {
                e.ColumnTitles.Add(title);
            }

            // e.ColumnTitles.AddRange(titles);  // 要复制，不要直接使用，因为后面可能会修改。怕影响到原件

            if (this.m_bFirstColumnIsKey == true)
                e.ColumnTitles.Insert(0, "命中的检索点");
        }

        List<string> GetBrowseColumnNames(string strPrefix)
        {
            string[] parts = strPrefix.Split(new char[] { '|' });
            if (parts.Length < 2)
                return new List<string>();

            return this.treeView_simpleQueryResTree.GetBrowseColumnNames(parts[0], parts[1]);
        }

        void ClearListViewPropertyCache()
        {
            ListViewProperty prop = (ListViewProperty)this.listView_browse.Tag;
            prop.ClearCache();
        }

        void ClearListViewItems()
        {
            this.listView_browse.Items.Clear();

            ListViewUtil.ClearSortColumns(this.listView_browse);

            // 清除所有需要确定的栏标题
            for (int i = 1; i < this.listView_browse.Columns.Count; i++)
            {
                this.listView_browse.Columns[i].Text = i.ToString();
            }
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

                if (this.Channels != null)
                    this.Channels.Dispose();
                this.eventClose.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchForm));
            this.tabControl_query = new System.Windows.Forms.TabControl();
            this.tabPage_querySimple = new System.Windows.Forms.TabPage();
            this.splitContainer_simpleQeury = new System.Windows.Forms.SplitContainer();
            this.treeView_simpleQueryResTree = new DigitalPlatform.rms.Client.ResTree();
            this.panel_simpleQueryOther = new System.Windows.Forms.Panel();
            this.button_test = new System.Windows.Forms.Button();
            this.button_simpleQueryProperty = new System.Windows.Forms.Button();
            this.textBox_simpleQuery_comment = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_simpleQueryWord = new System.Windows.Forms.TextBox();
            this.button_simpleQuerySearch = new System.Windows.Forms.Button();
            this.tabPage_queryAdvance = new System.Windows.Forms.TabPage();
            this.textBox_complexQuery_comment = new System.Windows.Forms.TextBox();
            this.button_complexQuerySearch = new System.Windows.Forms.Button();
            this.button_complex_server4 = new System.Windows.Forms.Button();
            this.imageList_complex_serverButton = new System.Windows.Forms.ImageList(this.components);
            this.button_complex_server3 = new System.Windows.Forms.Button();
            this.button_complex_server2 = new System.Windows.Forms.Button();
            this.button_complex_server1 = new System.Windows.Forms.Button();
            this.comboBox_complex_logic4 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_from4 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_db4 = new System.Windows.Forms.ComboBox();
            this.textBox_complex_word4 = new System.Windows.Forms.TextBox();
            this.comboBox_complex_logic3 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_from3 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_db3 = new System.Windows.Forms.ComboBox();
            this.textBox_complex_word3 = new System.Windows.Forms.TextBox();
            this.comboBox_complex_logic2 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_from2 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_db2 = new System.Windows.Forms.ComboBox();
            this.textBox_complex_word2 = new System.Windows.Forms.TextBox();
            this.comboBox_complex_from1 = new System.Windows.Forms.ComboBox();
            this.comboBox_complex_db1 = new System.Windows.Forms.ComboBox();
            this.textBox_complex_word1 = new System.Windows.Forms.TextBox();
            this.tabPage_queryXml = new System.Windows.Forms.TabPage();
            this.splitContainer_xml = new System.Windows.Forms.SplitContainer();
            this.textBox_xmlQueryString = new System.Windows.Forms.TextBox();
            this.textBox_xmlQuery_comment = new System.Windows.Forms.TextBox();
            this.button_xmlQuerySearch = new System.Windows.Forms.Button();
            this.toolTip_serverUrl = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.listView_browse = new DigitalPlatform.rms.Client.BrowseList();
            this.tabControl_query.SuspendLayout();
            this.tabPage_querySimple.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_simpleQeury)).BeginInit();
            this.splitContainer_simpleQeury.Panel1.SuspendLayout();
            this.splitContainer_simpleQeury.Panel2.SuspendLayout();
            this.splitContainer_simpleQeury.SuspendLayout();
            this.panel_simpleQueryOther.SuspendLayout();
            this.tabPage_queryAdvance.SuspendLayout();
            this.tabPage_queryXml.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_xml)).BeginInit();
            this.splitContainer_xml.Panel1.SuspendLayout();
            this.splitContainer_xml.Panel2.SuspendLayout();
            this.splitContainer_xml.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_query
            // 
            this.tabControl_query.Controls.Add(this.tabPage_querySimple);
            this.tabControl_query.Controls.Add(this.tabPage_queryAdvance);
            this.tabControl_query.Controls.Add(this.tabPage_queryXml);
            this.tabControl_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_query.Location = new System.Drawing.Point(0, 0);
            this.tabControl_query.Name = "tabControl_query";
            this.tabControl_query.SelectedIndex = 0;
            this.tabControl_query.Size = new System.Drawing.Size(671, 314);
            this.tabControl_query.TabIndex = 0;
            this.tabControl_query.SelectedIndexChanged += new System.EventHandler(this.tabControl_query_SelectedIndexChanged);
            // 
            // tabPage_querySimple
            // 
            this.tabPage_querySimple.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_querySimple.Controls.Add(this.splitContainer_simpleQeury);
            this.tabPage_querySimple.Location = new System.Drawing.Point(4, 31);
            this.tabPage_querySimple.Name = "tabPage_querySimple";
            this.tabPage_querySimple.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_querySimple.Size = new System.Drawing.Size(663, 279);
            this.tabPage_querySimple.TabIndex = 0;
            this.tabPage_querySimple.Text = "简单";
            this.tabPage_querySimple.UseVisualStyleBackColor = true;
            // 
            // splitContainer_simpleQeury
            // 
            this.splitContainer_simpleQeury.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_simpleQeury.Location = new System.Drawing.Point(3, 3);
            this.splitContainer_simpleQeury.Name = "splitContainer_simpleQeury";
            // 
            // splitContainer_simpleQeury.Panel1
            // 
            this.splitContainer_simpleQeury.Panel1.Controls.Add(this.treeView_simpleQueryResTree);
            // 
            // splitContainer_simpleQeury.Panel2
            // 
            this.splitContainer_simpleQeury.Panel2.Controls.Add(this.panel_simpleQueryOther);
            this.splitContainer_simpleQeury.Size = new System.Drawing.Size(657, 273);
            this.splitContainer_simpleQeury.SplitterDistance = 263;
            this.splitContainer_simpleQeury.TabIndex = 0;
            // 
            // treeView_simpleQueryResTree
            // 
            this.treeView_simpleQueryResTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_simpleQueryResTree.HideSelection = false;
            this.treeView_simpleQueryResTree.ImageIndex = 0;
            this.treeView_simpleQueryResTree.Location = new System.Drawing.Point(0, 0);
            this.treeView_simpleQueryResTree.Name = "treeView_simpleQueryResTree";
            this.treeView_simpleQueryResTree.SelectedImageIndex = 0;
            this.treeView_simpleQueryResTree.Size = new System.Drawing.Size(263, 273);
            this.treeView_simpleQueryResTree.TabIndex = 0;
            // 
            // panel_simpleQueryOther
            // 
            this.panel_simpleQueryOther.Controls.Add(this.button_test);
            this.panel_simpleQueryOther.Controls.Add(this.button_simpleQueryProperty);
            this.panel_simpleQueryOther.Controls.Add(this.textBox_simpleQuery_comment);
            this.panel_simpleQueryOther.Controls.Add(this.label1);
            this.panel_simpleQueryOther.Controls.Add(this.textBox_simpleQueryWord);
            this.panel_simpleQueryOther.Controls.Add(this.button_simpleQuerySearch);
            this.panel_simpleQueryOther.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_simpleQueryOther.Location = new System.Drawing.Point(0, 0);
            this.panel_simpleQueryOther.Name = "panel_simpleQueryOther";
            this.panel_simpleQueryOther.Size = new System.Drawing.Size(390, 273);
            this.panel_simpleQueryOther.TabIndex = 2;
            // 
            // button_test
            // 
            this.button_test.Location = new System.Drawing.Point(11, 89);
            this.button_test.Name = "button_test";
            this.button_test.Size = new System.Drawing.Size(137, 40);
            this.button_test.TabIndex = 6;
            this.button_test.Text = "Test";
            this.button_test.UseVisualStyleBackColor = true;
            this.button_test.Visible = false;
            this.button_test.Click += new System.EventHandler(this.button_test_Click);
            // 
            // button_simpleQueryProperty
            // 
            this.button_simpleQueryProperty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_simpleQueryProperty.Location = new System.Drawing.Point(280, 233);
            this.button_simpleQueryProperty.Name = "button_simpleQueryProperty";
            this.button_simpleQueryProperty.Size = new System.Drawing.Size(110, 40);
            this.button_simpleQueryProperty.TabIndex = 5;
            this.button_simpleQueryProperty.Text = "属性(&P)";
            this.button_simpleQueryProperty.Click += new System.EventHandler(this.button_simpleQueryProperty_Click);
            // 
            // textBox_simpleQuery_comment
            // 
            this.textBox_simpleQuery_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_simpleQuery_comment.Location = new System.Drawing.Point(15, 151);
            this.textBox_simpleQuery_comment.Multiline = true;
            this.textBox_simpleQuery_comment.Name = "textBox_simpleQuery_comment";
            this.textBox_simpleQuery_comment.ReadOnly = true;
            this.textBox_simpleQuery_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_simpleQuery_comment.Size = new System.Drawing.Size(368, 72);
            this.textBox_simpleQuery_comment.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(11, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 31);
            this.label1.TabIndex = 2;
            this.label1.Text = "检索词(&W):";
            // 
            // textBox_simpleQueryWord
            // 
            this.textBox_simpleQueryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_simpleQueryWord.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_simpleQueryWord.Location = new System.Drawing.Point(11, 43);
            this.textBox_simpleQueryWord.Name = "textBox_simpleQueryWord";
            this.textBox_simpleQueryWord.Size = new System.Drawing.Size(379, 31);
            this.textBox_simpleQueryWord.TabIndex = 1;
            // 
            // button_simpleQuerySearch
            // 
            this.button_simpleQuerySearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_simpleQuerySearch.Location = new System.Drawing.Point(280, 89);
            this.button_simpleQuerySearch.Name = "button_simpleQuerySearch";
            this.button_simpleQuerySearch.Size = new System.Drawing.Size(110, 40);
            this.button_simpleQuerySearch.TabIndex = 3;
            this.button_simpleQuerySearch.Text = "检索(&S)";
            this.button_simpleQuerySearch.Click += new System.EventHandler(this.button_simpleQuerySearch_Click);
            // 
            // tabPage_queryAdvance
            // 
            this.tabPage_queryAdvance.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_queryAdvance.Controls.Add(this.textBox_complexQuery_comment);
            this.tabPage_queryAdvance.Controls.Add(this.button_complexQuerySearch);
            this.tabPage_queryAdvance.Controls.Add(this.button_complex_server4);
            this.tabPage_queryAdvance.Controls.Add(this.button_complex_server3);
            this.tabPage_queryAdvance.Controls.Add(this.button_complex_server2);
            this.tabPage_queryAdvance.Controls.Add(this.button_complex_server1);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_logic4);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_from4);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_db4);
            this.tabPage_queryAdvance.Controls.Add(this.textBox_complex_word4);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_logic3);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_from3);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_db3);
            this.tabPage_queryAdvance.Controls.Add(this.textBox_complex_word3);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_logic2);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_from2);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_db2);
            this.tabPage_queryAdvance.Controls.Add(this.textBox_complex_word2);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_from1);
            this.tabPage_queryAdvance.Controls.Add(this.comboBox_complex_db1);
            this.tabPage_queryAdvance.Controls.Add(this.textBox_complex_word1);
            this.tabPage_queryAdvance.Location = new System.Drawing.Point(4, 31);
            this.tabPage_queryAdvance.Name = "tabPage_queryAdvance";
            this.tabPage_queryAdvance.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_queryAdvance.Size = new System.Drawing.Size(663, 279);
            this.tabPage_queryAdvance.TabIndex = 1;
            this.tabPage_queryAdvance.Text = "高级";
            this.tabPage_queryAdvance.UseVisualStyleBackColor = true;
            // 
            // textBox_complexQuery_comment
            // 
            this.textBox_complexQuery_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_complexQuery_comment.Location = new System.Drawing.Point(24, 238);
            this.textBox_complexQuery_comment.Multiline = true;
            this.textBox_complexQuery_comment.Name = "textBox_complexQuery_comment";
            this.textBox_complexQuery_comment.ReadOnly = true;
            this.textBox_complexQuery_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_complexQuery_comment.Size = new System.Drawing.Size(612, 35);
            this.textBox_complexQuery_comment.TabIndex = 20;
            // 
            // button_complexQuerySearch
            // 
            this.button_complexQuerySearch.Location = new System.Drawing.Point(24, 187);
            this.button_complexQuerySearch.Name = "button_complexQuerySearch";
            this.button_complexQuerySearch.Size = new System.Drawing.Size(146, 39);
            this.button_complexQuerySearch.TabIndex = 19;
            this.button_complexQuerySearch.Text = "检索(&S)";
            this.button_complexQuerySearch.Click += new System.EventHandler(this.button_complexQuerySearch_Click);
            // 
            // button_complex_server4
            // 
            this.button_complex_server4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_complex_server4.ImageIndex = 0;
            this.button_complex_server4.ImageList = this.imageList_complex_serverButton;
            this.button_complex_server4.Location = new System.Drawing.Point(121, 139);
            this.button_complex_server4.Name = "button_complex_server4";
            this.button_complex_server4.Size = new System.Drawing.Size(44, 39);
            this.button_complex_server4.TabIndex = 16;
            this.button_complex_server4.Click += new System.EventHandler(this.button_complex_server4_Click);
            this.button_complex_server4.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_complex_server4_MouseMove);
            // 
            // imageList_complex_serverButton
            // 
            this.imageList_complex_serverButton.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_complex_serverButton.ImageStream")));
            this.imageList_complex_serverButton.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_complex_serverButton.Images.SetKeyName(0, "");
            // 
            // button_complex_server3
            // 
            this.button_complex_server3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_complex_server3.ImageIndex = 0;
            this.button_complex_server3.ImageList = this.imageList_complex_serverButton;
            this.button_complex_server3.Location = new System.Drawing.Point(121, 98);
            this.button_complex_server3.Name = "button_complex_server3";
            this.button_complex_server3.Size = new System.Drawing.Size(44, 39);
            this.button_complex_server3.TabIndex = 11;
            this.button_complex_server3.Click += new System.EventHandler(this.button_complex_server3_Click);
            this.button_complex_server3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_complex_server3_MouseMove);
            // 
            // button_complex_server2
            // 
            this.button_complex_server2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_complex_server2.ImageIndex = 0;
            this.button_complex_server2.ImageList = this.imageList_complex_serverButton;
            this.button_complex_server2.Location = new System.Drawing.Point(121, 57);
            this.button_complex_server2.Name = "button_complex_server2";
            this.button_complex_server2.Size = new System.Drawing.Size(44, 39);
            this.button_complex_server2.TabIndex = 6;
            this.button_complex_server2.Click += new System.EventHandler(this.button_complex_server2_Click);
            this.button_complex_server2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_complex_server2_MouseMove);
            // 
            // button_complex_server1
            // 
            this.button_complex_server1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_complex_server1.ImageIndex = 0;
            this.button_complex_server1.ImageList = this.imageList_complex_serverButton;
            this.button_complex_server1.Location = new System.Drawing.Point(121, 15);
            this.button_complex_server1.Name = "button_complex_server1";
            this.button_complex_server1.Size = new System.Drawing.Size(44, 40);
            this.button_complex_server1.TabIndex = 1;
            this.button_complex_server1.Click += new System.EventHandler(this.button_complex_server1_Click);
            this.button_complex_server1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.button_complex_server1_MouseMove);
            // 
            // comboBox_complex_logic4
            // 
            this.comboBox_complex_logic4.Location = new System.Drawing.Point(24, 142);
            this.comboBox_complex_logic4.Name = "comboBox_complex_logic4";
            this.comboBox_complex_logic4.Size = new System.Drawing.Size(146, 29);
            this.comboBox_complex_logic4.TabIndex = 14;
            // 
            // comboBox_complex_from4
            // 
            this.comboBox_complex_from4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_from4.Location = new System.Drawing.Point(414, 142);
            this.comboBox_complex_from4.Name = "comboBox_complex_from4";
            this.comboBox_complex_from4.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_from4.TabIndex = 18;
            // 
            // comboBox_complex_db4
            // 
            this.comboBox_complex_db4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_db4.Location = new System.Drawing.Point(181, 142);
            this.comboBox_complex_db4.Name = "comboBox_complex_db4";
            this.comboBox_complex_db4.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_db4.TabIndex = 17;
            this.comboBox_complex_db4.SelectedIndexChanged += new System.EventHandler(this.comboBox_complex_db4_SelectedIndexChanged);
            // 
            // textBox_complex_word4
            // 
            this.textBox_complex_word4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_complex_word4.Location = new System.Drawing.Point(182, 142);
            this.textBox_complex_word4.Name = "textBox_complex_word4";
            this.textBox_complex_word4.Size = new System.Drawing.Size(0, 31);
            this.textBox_complex_word4.TabIndex = 15;
            // 
            // comboBox_complex_logic3
            // 
            this.comboBox_complex_logic3.Location = new System.Drawing.Point(24, 101);
            this.comboBox_complex_logic3.Name = "comboBox_complex_logic3";
            this.comboBox_complex_logic3.Size = new System.Drawing.Size(146, 29);
            this.comboBox_complex_logic3.TabIndex = 9;
            // 
            // comboBox_complex_from3
            // 
            this.comboBox_complex_from3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_from3.Location = new System.Drawing.Point(414, 101);
            this.comboBox_complex_from3.Name = "comboBox_complex_from3";
            this.comboBox_complex_from3.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_from3.TabIndex = 13;
            // 
            // comboBox_complex_db3
            // 
            this.comboBox_complex_db3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_db3.Location = new System.Drawing.Point(181, 101);
            this.comboBox_complex_db3.Name = "comboBox_complex_db3";
            this.comboBox_complex_db3.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_db3.TabIndex = 12;
            this.comboBox_complex_db3.SelectedIndexChanged += new System.EventHandler(this.comboBox_complex_db3_SelectedIndexChanged);
            // 
            // textBox_complex_word3
            // 
            this.textBox_complex_word3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_complex_word3.Location = new System.Drawing.Point(182, 101);
            this.textBox_complex_word3.Name = "textBox_complex_word3";
            this.textBox_complex_word3.Size = new System.Drawing.Size(0, 31);
            this.textBox_complex_word3.TabIndex = 10;
            // 
            // comboBox_complex_logic2
            // 
            this.comboBox_complex_logic2.Location = new System.Drawing.Point(24, 60);
            this.comboBox_complex_logic2.Name = "comboBox_complex_logic2";
            this.comboBox_complex_logic2.Size = new System.Drawing.Size(146, 29);
            this.comboBox_complex_logic2.TabIndex = 4;
            // 
            // comboBox_complex_from2
            // 
            this.comboBox_complex_from2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_from2.Location = new System.Drawing.Point(414, 60);
            this.comboBox_complex_from2.Name = "comboBox_complex_from2";
            this.comboBox_complex_from2.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_from2.TabIndex = 8;
            // 
            // comboBox_complex_db2
            // 
            this.comboBox_complex_db2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_db2.Location = new System.Drawing.Point(181, 60);
            this.comboBox_complex_db2.Name = "comboBox_complex_db2";
            this.comboBox_complex_db2.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_db2.TabIndex = 7;
            this.comboBox_complex_db2.SelectedIndexChanged += new System.EventHandler(this.comboBox_complex_db2_SelectedIndexChanged);
            // 
            // textBox_complex_word2
            // 
            this.textBox_complex_word2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_complex_word2.Location = new System.Drawing.Point(182, 60);
            this.textBox_complex_word2.Name = "textBox_complex_word2";
            this.textBox_complex_word2.Size = new System.Drawing.Size(0, 31);
            this.textBox_complex_word2.TabIndex = 5;
            // 
            // comboBox_complex_from1
            // 
            this.comboBox_complex_from1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_from1.Location = new System.Drawing.Point(414, 19);
            this.comboBox_complex_from1.Name = "comboBox_complex_from1";
            this.comboBox_complex_from1.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_from1.TabIndex = 3;
            // 
            // comboBox_complex_db1
            // 
            this.comboBox_complex_db1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_complex_db1.Location = new System.Drawing.Point(181, 19);
            this.comboBox_complex_db1.Name = "comboBox_complex_db1";
            this.comboBox_complex_db1.Size = new System.Drawing.Size(222, 29);
            this.comboBox_complex_db1.TabIndex = 2;
            this.comboBox_complex_db1.SelectedIndexChanged += new System.EventHandler(this.comboBox_complex_db1_SelectedIndexChanged);
            // 
            // textBox_complex_word1
            // 
            this.textBox_complex_word1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_complex_word1.Location = new System.Drawing.Point(182, 19);
            this.textBox_complex_word1.Name = "textBox_complex_word1";
            this.textBox_complex_word1.Size = new System.Drawing.Size(0, 31);
            this.textBox_complex_word1.TabIndex = 0;
            // 
            // tabPage_queryXml
            // 
            this.tabPage_queryXml.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_queryXml.Controls.Add(this.splitContainer_xml);
            this.tabPage_queryXml.Controls.Add(this.button_xmlQuerySearch);
            this.tabPage_queryXml.Location = new System.Drawing.Point(4, 31);
            this.tabPage_queryXml.Name = "tabPage_queryXml";
            this.tabPage_queryXml.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_queryXml.Size = new System.Drawing.Size(663, 279);
            this.tabPage_queryXml.TabIndex = 2;
            this.tabPage_queryXml.Text = "XML";
            this.tabPage_queryXml.UseVisualStyleBackColor = true;
            // 
            // splitContainer_xml
            // 
            this.splitContainer_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_xml.Location = new System.Drawing.Point(6, 6);
            this.splitContainer_xml.Name = "splitContainer_xml";
            this.splitContainer_xml.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_xml.Panel1
            // 
            this.splitContainer_xml.Panel1.Controls.Add(this.textBox_xmlQueryString);
            // 
            // splitContainer_xml.Panel2
            // 
            this.splitContainer_xml.Panel2.Controls.Add(this.textBox_xmlQuery_comment);
            this.splitContainer_xml.Size = new System.Drawing.Size(651, 267);
            this.splitContainer_xml.SplitterDistance = 153;
            this.splitContainer_xml.SplitterWidth = 8;
            this.splitContainer_xml.TabIndex = 5;
            // 
            // textBox_xmlQueryString
            // 
            this.textBox_xmlQueryString.AcceptsReturn = true;
            this.textBox_xmlQueryString.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_xmlQueryString.Location = new System.Drawing.Point(0, 0);
            this.textBox_xmlQueryString.Multiline = true;
            this.textBox_xmlQueryString.Name = "textBox_xmlQueryString";
            this.textBox_xmlQueryString.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_xmlQueryString.Size = new System.Drawing.Size(651, 153);
            this.textBox_xmlQueryString.TabIndex = 0;
            // 
            // textBox_xmlQuery_comment
            // 
            this.textBox_xmlQuery_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_xmlQuery_comment.Location = new System.Drawing.Point(0, 0);
            this.textBox_xmlQuery_comment.Multiline = true;
            this.textBox_xmlQuery_comment.Name = "textBox_xmlQuery_comment";
            this.textBox_xmlQuery_comment.ReadOnly = true;
            this.textBox_xmlQuery_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_xmlQuery_comment.Size = new System.Drawing.Size(651, 106);
            this.textBox_xmlQuery_comment.TabIndex = 21;
            // 
            // button_xmlQuerySearch
            // 
            this.button_xmlQuerySearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_xmlQuerySearch.Location = new System.Drawing.Point(834, 209);
            this.button_xmlQuerySearch.Name = "button_xmlQuerySearch";
            this.button_xmlQuerySearch.Size = new System.Drawing.Size(110, 35);
            this.button_xmlQuerySearch.TabIndex = 4;
            this.button_xmlQuerySearch.Text = "检索(&S)";
            this.button_xmlQuerySearch.Click += new System.EventHandler(this.button_xmlQuerySearch_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_query);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView_browse);
            this.splitContainer_main.Size = new System.Drawing.Size(671, 501);
            this.splitContainer_main.SplitterDistance = 314;
            this.splitContainer_main.TabIndex = 1;
            // 
            // listView_browse
            // 
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.Location = new System.Drawing.Point(0, 0);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(671, 183);
            this.listView_browse.TabIndex = 0;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_browse_ColumnClick);
            this.listView_browse.SelectedIndexChanged += new System.EventHandler(this.listView_browse_SelectedIndexChanged);
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_browse_MouseUp);
            // 
            // SearchForm
            // 
            this.AcceptButton = this.button_simpleQuerySearch;
            this.AutoScaleBaseSize = new System.Drawing.Size(11, 24);
            this.ClientSize = new System.Drawing.Size(671, 501);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SearchForm";
            this.Text = "检索窗";
            this.Activated += new System.EventHandler(this.SearchForm_Activated);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.SearchForm_Closing);
            this.Closed += new System.EventHandler(this.SearchForm_Closed);
            this.Load += new System.EventHandler(this.SearchForm_Load);
            this.tabControl_query.ResumeLayout(false);
            this.tabPage_querySimple.ResumeLayout(false);
            this.splitContainer_simpleQeury.Panel1.ResumeLayout(false);
            this.splitContainer_simpleQeury.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_simpleQeury)).EndInit();
            this.splitContainer_simpleQeury.ResumeLayout(false);
            this.panel_simpleQueryOther.ResumeLayout(false);
            this.panel_simpleQueryOther.PerformLayout();
            this.tabPage_queryAdvance.ResumeLayout(false);
            this.tabPage_queryAdvance.PerformLayout();
            this.tabPage_queryXml.ResumeLayout(false);
            this.splitContainer_xml.Panel1.ResumeLayout(false);
            this.splitContainer_xml.Panel1.PerformLayout();
            this.splitContainer_xml.Panel2.ResumeLayout(false);
            this.splitContainer_xml.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_xml)).EndInit();
            this.splitContainer_xml.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion


        /*
		public new void Show()
		{
			InitialSize();
			base.Show();
		}
		*/

        public void RefreshResTree()
        {
            if (treeView_simpleQueryResTree != null)
                treeView_simpleQueryResTree.Refresh(ResTree.RefreshStyle.All);
        }

        public void InitialSize()
        {
            // 设置窗口尺寸状态
            if (MainForm.AppInfo != null)
            {
                MainForm.AppInfo.LoadMdiChildFormStates(this,
                    "mdi_form_state");

#if NO
                // 获得splitter_main的状态
                this.splitter_main.SplitPosition = MainForm.AppInfo.GetInt(
                    "searchform",
                    "splitter_main_splitposition",
                    240);

                // 获得splitter_simpleQuery的状态
                this.splitter_simpleQuery.SplitPosition = MainForm.AppInfo.GetInt(
                    "searchform",
                    "splitter_simplequery_splitposition",
                    200);
#endif
            }
        }

        private void SearchForm_Load(object sender, System.EventArgs e)
        {
            this.MainForm.AppInfo.LoadMdiLayout += new EventHandler(AppInfo_LoadMdiLayout);
            this.MainForm.AppInfo.SaveMdiLayout += new EventHandler(AppInfo_SaveMdiLayout);

            InitialSize();

            stop = new DigitalPlatform.Stop();

            stop.Register(MainForm.stopManager, true);	// 和容器关联

            this.Channels.AskAccountInfo += new AskAccountInfoEventHandle(MainForm.Servers.OnAskAccountInfo);
            /*
			this.Channels.procAskAccountInfo = 
				 new Delegate_AskAccountInfo(MainForm.Servers.AskAccountInfo);
             */
            string strWidths = this.MainForm.AppInfo.GetString(
"searchform",
"record_list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(this.listView_browse,
                    strWidths,
                    true);
            }

            // 简单检索界面准备工作
            treeView_simpleQueryResTree.AppInfo = MainForm.AppInfo;	// 便于treeview中popup菜单修改配置文件时保存dialog尺寸位置

            treeView_simpleQueryResTree.stopManager = MainForm.stopManager;

            treeView_simpleQueryResTree.Servers = MainForm.Servers;	// 引用

            treeView_simpleQueryResTree.Channels = this.Channels;	// 引用

            treeView_simpleQueryResTree.Fill(null);

            textBox_simpleQueryWord.Text = MainForm.AppInfo.GetString(
                "search_simple_query",
                "word",
                "");

            // 按照上次保存的路径展开resdircontrol树
            string strResDirPath = MainForm.AppInfo.GetString(
                "search_simple_query",
                "resdirpath",
                "");
            if (strResDirPath != null)
            {
                object[] pList = { strResDirPath };

                this.BeginInvoke(new Delegate_ExpandResDir(ExpandResDir),
                    pList);

                // this.ExpandResDir(strResDirPath);
            }

            InitialComplexSearchUI();

        }

        public delegate void Delegate_ExpandResDir(string strResDirPath);

        void ExpandResDir(string strResDirPath)
        {
            //statusBar_main.Text = "正在展开资源目录 " + strResDirPath + ", 请稍候...";
            this.Update();

            ResPath respath = new ResPath(strResDirPath);

            this.EnableControlsInSearching(true);

            // 展开到指定的节点
            treeView_simpleQueryResTree.ExpandPath(respath);

            this.EnableControlsInSearching(false);

            //statusBar_main.Text = "";


        }

        private void SearchForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0 || stop.State == 1)
                {
                    if (this._channel != null)
                    {
                        this._channel.Abort();
                        e.Cancel = true;
                    }
                }
            }
        }

        private void SearchForm_Closed(object sender, System.EventArgs e)
        {
            eventClose.Set();

            if (stop != null) // 脱离关联
            {
                stop.Unregister();	// 和容器关联

                // MainForm.stopManager.Remove(stop);
                stop = null;
            }


            this.Channels.AskAccountInfo -= new AskAccountInfoEventHandle(MainForm.Servers.OnAskAccountInfo);
            this.Channels.Dispose();

            if (MainForm.AppInfo != null)
            {
                MainForm.AppInfo.SaveMdiChildFormStates(this,
                    "mdi_form_state");

#if NO
                // 保存splitter_main的状态
                MainForm.AppInfo.SetInt(
                    "searchform",
                    "splitter_main_splitposition",
                    this.splitter_main.SplitPosition);
                // 保存splitter_simpleQuery的状态
                MainForm.AppInfo.SetInt(
                    "searchform",
                    "splitter_simplequery_splitposition",
                    this.splitter_simpleQuery.SplitPosition);
#endif

                MainForm.AppInfo.SetString(
                    "search_simple_query",
                    "word",
                    textBox_simpleQueryWord.Text);

                // 保存resdircontrol最后的选择

                ResPath respath = new ResPath(treeView_simpleQueryResTree.SelectedNode);
                MainForm.AppInfo.SetString(
                    "search_simple_query",
                    "resdirpath",
                    respath.FullPath);

                string strWidths = ListViewUtil.GetColumnWidthListString(this.listView_browse);
                this.MainForm.AppInfo.SetString(
                    "searchform",
                    "record_list_column_width",
                    strWidths);
            }

            this.MainForm.AppInfo.LoadMdiLayout -= new EventHandler(AppInfo_LoadMdiLayout);
            this.MainForm.AppInfo.SaveMdiLayout -= new EventHandler(AppInfo_SaveMdiLayout);

        }

        public void AppInfo_LoadMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            try
            {
                // 获得splitContainer_main的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_main,
                    "searchform",
                    "splitContainer_main");

                // 获得splitContainer_up的状态
                this.MainForm.LoadSplitterPos(
                    this.splitContainer_simpleQeury,
                    "searchform",
                    "splitContainer_simpleQeury");
            }
            catch
            {
            }
        }

        void AppInfo_SaveMdiLayout(object sender, EventArgs e)
        {
            if (sender != this)
                return;

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {

                // 分割条位置
                // 保存splitContainer_main的状态
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "searchform",
                    "splitContainer_main");
                // 保存splitContainer_up的状态
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_simpleQeury,
                    "searchform",
                    "splitContainer_simpleQeury");
            }
        }


        public MainForm MainForm
        {
            get
            {
                return (MainForm)this.MdiParent;
            }
        }


        /*
        public string GetQueryXml()
        {
            string strQuery = "";
            TabPage page = tabControl1.SelectedTab ;
            if (page != null)
            {
                if (page.Name == "tabPage_xmlQuery") //XML检索式界面，涉及到的服务器直接使用
                {
                    strQuery = textBox_xmlQuery.Text;
                }
                else if (page.Name == "tabPage_simple") //简单界面,也涉及到服务，暂时按不涉及算
                {
                    strQuery = QueryClient.ProcessQuery2Xml(GetQueryExpressionsOfSimpleUI(),
                        this.m_containerForm .GetLanguage ());

                    //测试用
                    //textBox_xmlQuery.Text = strQuery;
                }
                else if (page.Name == "tabPage_complex")  //复杂界面，目录涉及到多个服务器，
                {
                    int nRet = GetQueryXmlOfComplex(out strQuery);
                    //if (nRet == -1)
                    // goto ERROR1;
                    //textBox_xmlQuery.Text = strQuery;
                }
            }
            else
            {
                MessageBox.Show ("环境不正确，不能检索，应改为相关按钮不能用");
            }
            return strQuery;
        }
        */

        private void button_simpleQuerySearch_Click(object sender, System.EventArgs e)
        {
            DoSimpleSearch(false);
        }

        bool m_bFirstColumnIsKey = false;

        public void DoSimpleSearch(bool bOutputKeyID)
        {
            textBox_simpleQuery_comment.Text = "";

            // 第一阶段
            TargetItemCollection targets = treeView_simpleQueryResTree.
                GetSearchTarget();
            Debug.Assert(targets != null, "GetSearchTarget() 异常");

            int i;

            // 第二阶段
            for (i = 0; i < targets.Count; i++)
            {
                TargetItem item = (TargetItem)targets[i];
                item.Words = textBox_simpleQueryWord.Text;
            }
            targets.MakeWordPhrases(
                Convert.ToBoolean(MainForm.AppInfo.GetInt("simple_query_property", "auto_split_words", 1)),
                Convert.ToBoolean(MainForm.AppInfo.GetInt("simple_query_property", "auto_detect_range", 0)),
                Convert.ToBoolean(MainForm.AppInfo.GetInt("simple_query_property", "auto_detect_relation", 0))
                );


            // 参数
            for (i = 0; i < targets.Count; i++)
            {
                TargetItem item = (TargetItem)targets[i];
                item.MaxCount = MainForm.AppInfo.GetInt("simple_query_property", "maxcount", -1);
            }

            // 第三阶段
            targets.MakeXml();

            // 正式检索

            string strError;

            if (bOutputKeyID == true)
                this.m_bFirstColumnIsKey = true;
            else
                this.m_bFirstColumnIsKey = false;
            this.ClearListViewPropertyCache();
            this.ClearListViewItems();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            EnableControlsInSearching(true);
            try
            {

                long lTotalCount = 0;   // 命中记录总数
                for (i = 0; i < targets.Count; i++)
                {

                    if (stop.State >= 1)
                        break;

                    TargetItem item = (TargetItem)targets[i];

                    _channel = Channels.GetChannel(item.Url);
                    Debug.Assert(_channel != null, "Channels.GetChannel 异常");

                    _channel.Idle += Channel_Idle;
                    try
                    {

                        textBox_simpleQuery_comment.Text += "检索式XML:\r\n" + DomUtil.GetIndentXml(item.Xml) + "\r\n";

                        // 2010/5/18
                        string strBrowseStyle = "id,cols";
                        string strOutputStyle = "";
                        if (bOutputKeyID == true)
                        {
                            strOutputStyle = "keyid";
                            strBrowseStyle = "keyid,id,key,cols";
                        }

                        // MessageBox.Show(this, item.Xml);
                        long nRet = _channel.DoSearch(item.Xml,
                            "default",
                            strOutputStyle + ",explain",
                            out string explain,
                            out strError);
                        if (nRet == -1)
                        {
                            textBox_simpleQuery_comment.Text += "出错: " + strError + "\r\n";
                            MessageBox.Show(this, strError);
                            continue;
                        }
                        lTotalCount += nRet;
                        textBox_simpleQuery_comment.Text += $"命中记录数: {nRet}\r\nExplain:\r\n{explain}";

                        if (nRet == 0)
                            continue;

                        // 获取结果集
                        nRet = _channel.DoBrowse(listView_browse,
                            listView_browse.Lang,
                            stop,
                            "default",
                            strBrowseStyle,
                            out strError);
                        if (nRet == -1)
                        {
                            textBox_simpleQuery_comment.Text += "装载浏览信息时出错: " + strError + "\r\n";
                            MessageBox.Show(this, strError);
                            continue;
                        }
                    }
                    finally
                    {
                        _channel.Idle -= Channel_Idle;
                    }
                }

                if (targets.Count > 1)
                {
                    textBox_simpleQuery_comment.Text += "命中总条数: " + Convert.ToString(lTotalCount) + "\r\n";
                }

                if (lTotalCount == 0)
                {
                    MessageBox.Show(this, "未命中");
                }

                _channel = null;
            }
            finally
            {
                EnableControlsInSearching(false);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }
        }

        private void Channel_Idle(object sender, IdleEventArgs e)
        {
            Application.DoEvents();
        }

        void DoStop(object sender, StopEventArgs e)
        {
            if (this._channel != null)
                this._channel.Abort();
        }

        private void SearchForm_Activated(object sender, System.EventArgs e)
        {
            if (stop != null)
                MainForm.stopManager.Active(this.stop);

            MainForm.SetMenuItemState();

            // 借用stop的状态
            if (this.MainForm.toolBarButton_stop.Enabled == true)
            {
                this.MainForm.toolBarButton_search.Enabled = false;
                this.MainForm.ToolStripMenuItem_searchKeyID.Enabled = false;
            }
            else
            {
                this.MainForm.toolBarButton_search.Enabled = true;
                this.MainForm.ToolStripMenuItem_searchKeyID.Enabled = true;
            }
            /*

			MainForm.toolBarButton_save.Enabled = false;

			MainForm.toolBarButton_delete.Enabled = false;

			MainForm.toolBarButton_prev.Enabled = false;
			MainForm.toolBarButton_next.Enabled = false;

			MainForm.MenuItem_viewAccessPoint.Enabled = false;
			MainForm.MenuItem_save.Enabled = false;
			MainForm.MenuItem_saveas.Enabled = false;
			MainForm.MenuItem_saveasToDB.Enabled = false;
			MainForm.MenuItem_saveToTemplate.Enabled = false;
			MainForm.MenuItem_autoGenerate.Enabled = false;
             * */
        }

        void EnableControlsInSearching(bool bSearching)
        {
            this.MainForm.Searching(bSearching);

            if (bSearching == true)
            {
                button_simpleQuerySearch.Enabled = false;
                textBox_simpleQueryWord.Enabled = false;
                treeView_simpleQueryResTree.Enabled = false;
                button_simpleQueryProperty.Enabled = false;

                this.MainForm.toolBarButton_search.Enabled = false;
                this.MainForm.ToolStripMenuItem_searchKeyID.Enabled = false;

                this.button_test.Enabled = false;
            }
            else
            {
                button_simpleQuerySearch.Enabled = true;
                textBox_simpleQueryWord.Enabled = true;
                treeView_simpleQueryResTree.Enabled = true;
                button_simpleQueryProperty.Enabled = true;

                this.MainForm.toolBarButton_search.Enabled = true;
                this.MainForm.ToolStripMenuItem_searchKeyID.Enabled = true;

                this.button_test.Enabled = true;
            }
        }

        private void listView_browse_DoubleClick(object sender, System.EventArgs e)
        {
            if (listView_browse.SelectedItems.Count != 0
                || listView_browse.FocusedItem != null)
            {
                string[] paths = BrowseList.GetSelectedRecordPaths(listView_browse, true);

                DetailForm child = null;

                if (!(Control.ModifierKeys == Keys.Control))
                    child = MainForm.TopDetailForm;

                if (child == null)
                {
                    child = new DetailForm();
                    child.MdiParent = MainForm;
                    child.Show();
                }
                else
                {
                    child.Activate();
                }

                this.listView_browse.Enabled = false;
                child.LoadRecord(paths[0], null);
                this.listView_browse.Enabled = true;

            }
        }

        void LoadRecordsToDetailForm(string[] paths,
            bool bActivateEveryWindow)
        {
            this.listView_browse.Enabled = false;

            for (int i = 0; i < paths.Length; i++)
            {
                DetailForm child = new DetailForm();
                child.MdiParent = MainForm;
                child.Show();

                if (bActivateEveryWindow == true)
                    child.Activate();

                child.LoadRecord(paths[i], null);
            }

            this.listView_browse.Enabled = true;

        }

        public string PropertiesText()
        {
            if (tabControl_query.SelectedTab == this.tabPage_querySimple)
            {
            }

            return "";
        }

        private void button_simpleQueryProperty_Click(object sender, System.EventArgs e)
        {
            SearchPropertyDlg dlg = new SearchPropertyDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.ap = MainForm.AppInfo;
            dlg.CfgTitle = "simple_query_property";
            dlg.ShowDialog(this);

        }

        private void listView_browse_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            bool bSelected = listView_browse.SelectedItems.Count > 0;

            //
            menuItem = new MenuItem("装入新详细窗(&N)");
            menuItem.Click += new System.EventHandler(this.menu_loadToNewDetailWindows);
            if (bSelected == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("装入活动的详细窗(&A)");
            menuItem.Click += new System.EventHandler(this.menu_loadToActiveDetailWindows);
            if (bSelected == false)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            contextMenu.Show(listView_browse, new Point(e.X, e.Y));

        }


        // 装入新详细窗
        void menu_loadToNewDetailWindows(object sender, System.EventArgs e)
        {
            // string[] paths = listView_browse.GetSelectedRecordPaths(false);
            string[] paths = BrowseList.GetSelectedRecordPaths(listView_browse, false);

            if (paths == null || paths.Length == 0)
            {
                MessageBox.Show(this, "尚未选择要装入的记录...");
                return;
            }

            LoadRecordsToDetailForm(paths, true);

        }

        // 装入活动详细窗
        void menu_loadToActiveDetailWindows(object sender, System.EventArgs e)
        {
            // string[] paths = listView_browse.GetSelectedRecordPaths(false);
            string[] paths = BrowseList.GetSelectedRecordPaths(listView_browse, false);

            if (paths == null || paths.Length == 0)
            {
                MessageBox.Show(this, "尚未选择要装入的记录...");
                return;
            }

            DetailForm child = null;

            child = MainForm.TopDetailForm;

            // 第一个装入活动窗口
            if (child != null)
            {
                child.Activate();
                this.listView_browse.Enabled = false;
                child.LoadRecord(paths[0], null);
                this.listView_browse.Enabled = true;
                if (paths.Length <= 1)
                    return;

                string[] temp = new string[paths.Length - 1];
                Array.Copy(paths, 1, temp, 0, paths.Length - 1);
                paths = temp;
            }


            // 余下的装入新开的详细窗
            LoadRecordsToDetailForm(paths, true);
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct CREATESTRUCTW
        {
            public IntPtr lpCreateParams;
            public IntPtr hInstance;
            public IntPtr hMenu;
            public IntPtr hwndParent;
            public int cy;
            public int cx;
            public int y;
            public int x;
            public Int32 style;
            public string lpszName;
            public string lpszClass;
            public UInt32 dwExStyle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MDICREATESTRUCT
        {
            public string szClass;
            public string szTitle;
            public IntPtr hOwner;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public UInt32 style;
            public IntPtr lParam;
        }


        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                /*
            case API.WM_CREATE:
            {
                CREATESTRUCTW lpcs;

                lpcs = (CREATESTRUCTW)Marshal.PtrToStructure(m.LParam,
                    typeof(CREATESTRUCTW));

                MDICREATESTRUCT cs;

                cs = (MDICREATESTRUCT)Marshal.PtrToStructure(lpcs.lpCreateParams,
                    typeof(MDICREATESTRUCT));

                cs.cy = 20;
            }
            break;
            */

                default:
                    break;
            }

            base.DefWndProc(ref m);
        }

        public void DoSearch(bool bOutputKeyID)
        {
            if (this.tabControl_query.SelectedTab == this.tabPage_querySimple)
            {
                DoSimpleSearch(bOutputKeyID);
                return;
            }

            if (this.tabControl_query.SelectedTab == this.tabPage_queryAdvance)
            {
                DoComplexSearch(bOutputKeyID);
                return;
            }

            if (this.tabControl_query.SelectedTab == this.tabPage_queryXml)
            {
                DoXmlSearch(bOutputKeyID);
                return;
            }
        }

        private void button_xmlQuerySearch_Click(object sender, System.EventArgs e)
        {
            DoXmlSearch(false);
        }

        public void DoXmlSearch(bool bOutputKeyID)
        {
            textBox_xmlQuery_comment.Text = "";

            // 第一阶段
            TargetItemCollection targets = treeView_simpleQueryResTree.
                GetSearchTarget();
            Debug.Assert(targets != null, "GetSearchTarget() 异常");

            // 正式检索
            string strError;

            if (bOutputKeyID == true)
                this.m_bFirstColumnIsKey = true;
            else
                this.m_bFirstColumnIsKey = false;
            this.ClearListViewPropertyCache();
            this.ClearListViewItems();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            EnableControlsInSearching(true);

            // 2010/5/18
            string strBrowseStyle = "id,cols";
            string strOutputStyle = "";
            if (bOutputKeyID == true)
            {
                strOutputStyle = "keyid";
                strBrowseStyle = "keyid,id,key,cols";
            }

            long lTotalCount = 0;	// 命中记录总数
            for (int i = 0; i < targets.Count; i++)
            {

                if (stop.State >= 1)
                    break;

                TargetItem item = (TargetItem)targets[i];

                _channel = Channels.GetChannel(item.Url);
                Debug.Assert(_channel != null, "Channels.GetChannel 异常");

                _channel.Idle += Channel_Idle;
                try
                {
                    // MessageBox.Show(this, item.Xml);
                    long nRet = _channel.DoSearch(textBox_xmlQueryString.Text,
                        "default",
                        strOutputStyle + ",explain",
                        out string explain,
                        out strError);
                    if (nRet == -1)
                    {
                        textBox_xmlQuery_comment.Text += "出错: " + strError + "\r\n";
                        MessageBox.Show(this, strError);
                        continue;
                    }
                    lTotalCount += nRet;
                    textBox_xmlQuery_comment.Text += $"命中记录数: {nRet}\r\nExplain:\r\n{explain}";

                    if (nRet == 0)
                        continue;

                    // 获取结果集

                    nRet = _channel.DoBrowse(listView_browse,
                        listView_browse.Lang,
                        stop,
                        "default",
                        strBrowseStyle,
                        out strError);
                    if (nRet == -1)
                    {
                        textBox_xmlQuery_comment.Text += "装载浏览信息时出错: " + strError + "\r\n";
                        MessageBox.Show(this, strError);
                        continue;
                    }
                }
                finally
                {
                    if (_channel != null)
                        _channel.Idle -= Channel_Idle;
                }
            }

            if (targets.Count > 1)
            {
                textBox_xmlQuery_comment.Text += "命中总条数: " + Convert.ToString(lTotalCount) + "\r\n";
            }

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            if (lTotalCount == 0)
            {
                MessageBox.Show(this, "未命中");
            }

            EnableControlsInSearching(false);

            _channel = null;

        }

        // 填充逻辑算子列表
        void FillLogicComboBox(System.Windows.Forms.ComboBox list)
        {
            list.Items.Add("AND 与");
            list.Items.Add("OR  或");
            list.Items.Add("SUB 减");
        }

        // 填充数据库名
        int FillDbNames(string strServerUrl,
            System.Windows.Forms.ComboBox list,
            out string strError)
        {
            Debug.Assert(strServerUrl != "", "strServerUrl参数不能为空");
            strError = "";
            RmsChannel channel = Channels.GetChannel(strServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            list.Enabled = false;

            string[] dbs = null;
            long lRet = channel.DoDir("",
                this.Lang,
                null,   // 不要求返回所有语言的名字
                0,	// 数据库类型
                out dbs,
                out strError);

            list.Enabled = true;

            if (lRet == -1)
                return -1;

            list.Items.Clear();
            list.Items.AddRange(dbs);


            return 0;
        }

        // 填充From名
        int FillFromNames(string strServerUrl,
            string strDbName,
            System.Windows.Forms.ComboBox list,
            out string strError)
        {
            Debug.Assert(strServerUrl != "", "strServerUrl参数不能为空");
            Debug.Assert(strDbName != "", "strDbName参数不能为空");
            strError = "";
            RmsChannel channel = Channels.GetChannel(strServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            string[] froms = null;

            list.Enabled = false;


            long lRet = channel.DoDir(strDbName,
                this.Lang,
                null,   // 不要求返回所有语言的名字
                1,	// From类型
                out froms,
                out strError);

            list.Enabled = true;

            if (lRet == -1)
                return -1;

            list.Items.Clear();
            list.Items.AddRange(froms);


            return 0;
        }

        // 初始化复杂检索的界面
        void InitialComplexSearchUI()
        {
            FillLogicComboBox(comboBox_complex_logic2);
            FillLogicComboBox(comboBox_complex_logic3);
            FillLogicComboBox(comboBox_complex_logic4);

            comboBox_complex_logic2.SelectedIndex = 0;
            comboBox_complex_logic3.SelectedIndex = 0;
            comboBox_complex_logic4.SelectedIndex = 0;


            /*
            // 初始化服务器URL
            ResPath respath = new ResPath(treeView_simpleQueryResTree.SelectedNode);
            m_aComplexServer.Clear();
            m_aComplexServer.Add(respath.Url);
            m_aComplexServer.Add(respath.Url);
            m_aComplexServer.Add(respath.Url);
            m_aComplexServer.Add(respath.Url);
            */

            m_aComplexServer.Clear();
            m_aComplexServer.Add("");
            m_aComplexServer.Add("");
            m_aComplexServer.Add("");
            m_aComplexServer.Add("");



            EnableComplexSearchControls();
        }

        // 如果服务器发生改变，刷新库名、From名
        void AfterServerChanged(int nIndex)
        {
            Cursor oldcurcor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string strUrl = (string)this.m_aComplexServer[nIndex];

                System.Windows.Forms.ComboBox listDB = null;
                if (nIndex == 0)
                    listDB = this.comboBox_complex_db1;
                else if (nIndex == 1)
                    listDB = this.comboBox_complex_db2;
                else if (nIndex == 2)
                    listDB = this.comboBox_complex_db3;
                else if (nIndex == 3)
                    listDB = this.comboBox_complex_db4;


                // 填充库名列表
                string strError = "";
                int nRet = FillDbNames((string)this.m_aComplexServer[nIndex],
                    listDB,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(strError);
                }

                if (listDB.Items.Count > 0)
                    listDB.SelectedIndex = 0;	// 默认选定第一项

                System.Windows.Forms.ComboBox listFrom = null;
                if (nIndex == 0)
                    listFrom = this.comboBox_complex_from1;
                else if (nIndex == 1)
                    listFrom = this.comboBox_complex_from2;
                else if (nIndex == 2)
                    listFrom = this.comboBox_complex_from3;
                else if (nIndex == 3)
                    listFrom = this.comboBox_complex_from4;


                if (listDB.Items.Count > 0)
                {
                    // 填充from列表
                    nRet = FillFromNames((string)this.m_aComplexServer[nIndex],
                        (string)listDB.Items[0],
                        listFrom,
                        out strError);
                    if (nRet == -1)
                    {
                        MessageBox.Show(strError);
                    }

                    if (listFrom.Items.Count > 0)
                        listFrom.SelectedIndex = 0;	// 默认选定第一项

                }

                EnableComplexSearchControls();
            }
            finally
            {
                this.Cursor = oldcurcor;
            }
        }


        // 如果库名发生改变，刷新From名
        void AfterDbChanged(int nIndex)
        {
            string strUrl = (string)this.m_aComplexServer[nIndex];

            System.Windows.Forms.ComboBox listDB = null;
            if (nIndex == 0)
                listDB = this.comboBox_complex_db1;
            else if (nIndex == 1)
                listDB = this.comboBox_complex_db2;
            else if (nIndex == 2)
                listDB = this.comboBox_complex_db3;
            else if (nIndex == 3)
                listDB = this.comboBox_complex_db4;

            System.Windows.Forms.ComboBox listFrom = null;
            if (nIndex == 0)
                listFrom = this.comboBox_complex_from1;
            else if (nIndex == 1)
                listFrom = this.comboBox_complex_from2;
            else if (nIndex == 2)
                listFrom = this.comboBox_complex_from3;
            else if (nIndex == 3)
                listFrom = this.comboBox_complex_from4;


            if (listDB.Items.Count > 0)
            {
                int nSelected = listDB.SelectedIndex;
                if (nSelected == -1)
                {
                    listDB.SelectedIndex = 0;
                    nSelected = 0;
                }

                string strError = "";
                // 填充from列表
                int nRet = FillFromNames((string)this.m_aComplexServer[nIndex],
                    (string)listDB.Items[nSelected],
                    listFrom,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(strError);
                }

                if (listFrom.Items.Count > 0)
                    listFrom.SelectedIndex = 0;	// 默认选定第一项

            }

        }

        // 选定一个服务器
        private void button_complex_server1_Click(object sender, System.EventArgs e)
        {
            string strOldUrl = (string)this.m_aComplexServer[0];

            // 选择目标服务器
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标服务器";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = strOldUrl;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (strOldUrl != dlg.Path)
            {
                this.m_aComplexServer[0] = dlg.Path;

                AfterServerChanged(0);
            }
        }

        private void button_complex_server2_Click(object sender, System.EventArgs e)
        {
            string strOldUrl = (string)this.m_aComplexServer[1];

            // 选择目标服务器
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标服务器";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = strOldUrl;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (strOldUrl != dlg.Path)
            {
                this.m_aComplexServer[1] = dlg.Path;

                AfterServerChanged(1);
            }


        }

        private void button_complex_server3_Click(object sender, System.EventArgs e)
        {
            string strOldUrl = (string)this.m_aComplexServer[2];

            // 选择目标服务器
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标服务器";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = strOldUrl;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (strOldUrl != dlg.Path)
            {
                this.m_aComplexServer[2] = dlg.Path;

                AfterServerChanged(2);
            }

        }

        private void button_complex_server4_Click(object sender, System.EventArgs e)
        {
            string strOldUrl = (string)this.m_aComplexServer[3];

            // 选择目标服务器
            OpenResDlg dlg = new OpenResDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Text = "请选择目标服务器";
            dlg.EnabledIndices = new int[] { ResTree.RESTYPE_SERVER };
            dlg.ap = this.MainForm.AppInfo;
            dlg.ApCfgTitle = "detailform_openresdlg";
            dlg.Path = strOldUrl;
            dlg.Initial(MainForm.Servers,
                this.Channels);
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            if (strOldUrl != dlg.Path)
            {
                this.m_aComplexServer[3] = dlg.Path;

                AfterServerChanged(3);
            }

        }

        private void comboBox_complex_db1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            AfterDbChanged(0);

        }

        private void comboBox_complex_db2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            AfterDbChanged(1);

        }

        private void comboBox_complex_db3_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            AfterDbChanged(2);

        }

        private void comboBox_complex_db4_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            AfterDbChanged(3);

        }


        // 构造dprms内核可以接受的xml检索式
        public int BuildQueryXml(
            out ArrayList aServer,
            out ArrayList aQueryXml,
            out string strError)
        {
            aServer = new ArrayList();
            aQueryXml = new ArrayList();
            strError = "";

            // 收集检索词
            ArrayList aWord = new ArrayList();

            aWord.Add(this.textBox_complex_word1.Text);
            aWord.Add(this.textBox_complex_word2.Text);
            aWord.Add(this.textBox_complex_word3.Text);
            aWord.Add(this.textBox_complex_word4.Text);

            // 收集逻辑算符

            ArrayList aLogic = new ArrayList();

            aLogic.Add(null);
            aLogic.Add(this.comboBox_complex_logic2.SelectedItem);
            aLogic.Add(this.comboBox_complex_logic3.SelectedItem);
            aLogic.Add(this.comboBox_complex_logic4.SelectedItem);

            // 收集目标库

            ArrayList aDB = new ArrayList();
            aDB.Add(this.comboBox_complex_db1.SelectedItem);
            aDB.Add(this.comboBox_complex_db2.SelectedItem);
            aDB.Add(this.comboBox_complex_db3.SelectedItem);
            aDB.Add(this.comboBox_complex_db4.SelectedItem);

            // 收集检索途径

            ArrayList aFrom = new ArrayList();

            aFrom.Add(this.comboBox_complex_from1.SelectedItem);
            aFrom.Add(this.comboBox_complex_from2.SelectedItem);
            aFrom.Add(this.comboBox_complex_from3.SelectedItem);
            aFrom.Add(this.comboBox_complex_from4.SelectedItem);


            int nLineCount = 0;
            string strLastServer = "";
            string strQueryXml = "";

            // 构造检索式
            for (int i = 0; i < 4; i++)
            {

                if ((string)this.m_aComplexServer[i] != strLastServer
                    && strQueryXml != "")
                {
                    aServer.Add(strLastServer);
                    strLastServer = "";
                    aQueryXml.Add("<group>" + strQueryXml + "</group>");
                    strQueryXml = "";
                    nLineCount = 0;
                }

                if (this.m_aComplexServer[i] == null
                    || (string)this.m_aComplexServer[i] == "")
                    continue;

                string strWord = (string)aWord[i];
                string strDbName = (string)aDB[i];

                if (strDbName == null)
                    strDbName = "";

                if (i > 0)
                {
                    if (strWord == "" && strDbName == "")
                        continue;
                }



                string strLogic = (string)aLogic[i];
                if (strLogic != null)
                {
                    int nRet = strLogic.IndexOf(" ", 0);
                    if (nRet != -1)
                        strLogic = strLogic.Substring(0, nRet).Trim();
                }

                string strTargetList = "";
                string strFrom = (string)aFrom[i];
                if (strFrom == null)
                    strFrom = "";
                /*
                if (strDbName == "全部" 
                    || String.Compare(strDbName,"All",true) == 0)
                {
                    int j=0;

                    for(j=0;j<boards.Count;j++)
                    {
                        Board board = (Board)boards[j];
                        if (board.IsRecycleBin == true)
                            continue;
                        if (strTargetList != "")
                            strTargetList += ";";
                        strTargetList += board.DbName + ":" + strFrom;
                    }

                }
                else 
                */
                {
                    strTargetList = strDbName + ":" + strFrom;
                }

                string strMatchStyle = "left";

                if (strWord.Length > 1)
                {
                    if (strWord[strWord.Length - 1] == '|')
                    {
                        strWord = strWord.Substring(0, strWord.Length - 1);
                        strMatchStyle = "exact";
                    }

                    if (strWord[0] == '*')
                    {
                        strWord = strWord.Substring(1);
                        if (strMatchStyle == "exact")	// 如果已经是精确匹配, 也就是右边出现了'|', 则加上左方的'*',就构成了右方一致
                            strMatchStyle = "right";
                        else
                            strMatchStyle = "middle";
                    }

                }

                string strOneDbQuery =
                    "<target list='" + strTargetList + "'><item><word>"	// <order>DESC</order> 在<word>之前，表示倒序
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word><match>" + strMatchStyle + "</match><relation>=</relation><dataType>string</dataType><maxCount>"
                    + Convert.ToString(10000)
                    + "</maxCount></item><lang>chi</lang></target>";

                if (nLineCount > 0)
                    strQueryXml += "<operator value='" + strLogic + "'/>";

                strQueryXml += strOneDbQuery;
                nLineCount++;
                strLastServer = (string)this.m_aComplexServer[i];
            }

            if (strQueryXml != "")
            {
                aServer.Add(strLastServer);
                strLastServer = "";
                aQueryXml.Add("<group>" + strQueryXml + "</group>");
                strQueryXml = "";
                nLineCount = 0;
            }


            return 0;
        }

        // 复杂检索
        private void button_complexQuerySearch_Click(object sender, System.EventArgs e)
        {
            DoComplexSearch(false);
        }

        // 复杂检索
        public void DoComplexSearch(bool bOutputKeyID)
        {
            textBox_complexQuery_comment.Text = "";

            ArrayList aServer = null;
            ArrayList aQueryXml = null;
            string strError = "";

            long nRet = BuildQueryXml(
                out aServer,
                out aQueryXml,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(strError);
                return;
            }

            if (aServer.Count == 0)
            {
                MessageBox.Show("因没有选定任何服务器地址，无法进行检索");
                return;
            }

            // 正式检索

            // string strError;

            if (bOutputKeyID == true)
                this.m_bFirstColumnIsKey = true;
            else
                this.m_bFirstColumnIsKey = false;
            this.ClearListViewPropertyCache();
            this.ClearListViewItems();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索 ...");
            stop.BeginLoop();

            EnableControlsInSearching(true);

            // 2010/5/18
            string strBrowseStyle = "id,cols";
            string strOutputStyle = "";
            if (bOutputKeyID == true)
            {
                strOutputStyle = "keyid";
                strBrowseStyle = "keyid,id,key,cols";
            }

            long lTotalCount = 0;	// 命中记录总数
            int i = 0;
            for (i = 0; i < aServer.Count; i++)
            {

                if (stop.State >= 1)
                    break;

                string strServer = (string)aServer[i];
                string strQueryXml = (string)aQueryXml[i];

                _channel = Channels.GetChannel(strServer);
                Debug.Assert(_channel != null, "Channels.GetChannel 异常");

                _channel.Idle += Channel_Idle;
                try
                {

                    textBox_complexQuery_comment.Text += "目标服务器:\t" + strServer + "\r\n";

                    textBox_complexQuery_comment.Text += "检索式XML:\r\n" + DomUtil.GetIndentXml(strQueryXml) + "\r\n";

                    // MessageBox.Show(this, item.Xml);
                    nRet = _channel.DoSearch(strQueryXml,
                        "default",
                        strOutputStyle + ",explain",
                        out string explain,
                        out strError);
                    if (nRet == -1)
                    {
                        textBox_complexQuery_comment.Text += "出错: " + strError + "\r\n";
                        MessageBox.Show(this, strError);
                        continue;
                    }
                    lTotalCount += nRet;
                    textBox_complexQuery_comment.Text += $"命中记录数: {nRet}\r\nExplain:\r\n{explain}";

                    if (nRet == 0)
                        continue;

                    // 获取结果集

                    nRet = _channel.DoBrowse(listView_browse,
                        listView_browse.Lang,
                        stop,
                        "default",
                        strBrowseStyle,
                        out strError);
                    if (nRet == -1)
                    {
                        textBox_complexQuery_comment.Text += "装载浏览信息时出错: " + strError + "\r\n";
                        MessageBox.Show(this, strError);
                        continue;
                    }
                }
                finally
                {
                    _channel.Idle -= Channel_Idle;
                }
            }

            if (aServer.Count > 1)
            {
                textBox_complexQuery_comment.Text += "命中总条数: " + Convert.ToString(lTotalCount) + "\r\n";
            }

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            if (lTotalCount == 0)
            {
                MessageBox.Show(this, "未命中");
            }

            EnableControlsInSearching(false);

            _channel = null;
        }

        void EnableComplexSearchLine1(bool bEnable)
        {
            this.textBox_complex_word1.Enabled = bEnable;
            this.comboBox_complex_db1.Enabled = bEnable;
            this.comboBox_complex_from1.Enabled = bEnable;
        }

        void EnableComplexSearchLine2(bool bEnable)
        {
            this.comboBox_complex_logic2.Enabled = bEnable;
            this.textBox_complex_word2.Enabled = bEnable;
            this.comboBox_complex_db2.Enabled = bEnable;
            this.comboBox_complex_from2.Enabled = bEnable;
        }

        void EnableComplexSearchLine3(bool bEnable)
        {
            this.comboBox_complex_logic3.Enabled = bEnable;
            this.textBox_complex_word3.Enabled = bEnable;
            this.comboBox_complex_db3.Enabled = bEnable;
            this.comboBox_complex_from3.Enabled = bEnable;
        }

        void EnableComplexSearchLine4(bool bEnable)
        {
            this.comboBox_complex_logic4.Enabled = bEnable;
            this.textBox_complex_word4.Enabled = bEnable;
            this.comboBox_complex_db4.Enabled = bEnable;
            this.comboBox_complex_from4.Enabled = bEnable;
        }


        void EnableComplexSearchControls()
        {
            for (int i = 0; i < this.m_aComplexServer.Count; i++)
            {
                string strServer = (string)this.m_aComplexServer[i];
                if (strServer == null || strServer == "")
                {
                    if (i == 0)
                        EnableComplexSearchLine1(false);
                    else if (i == 1)
                        EnableComplexSearchLine2(false);
                    else if (i == 2)
                        EnableComplexSearchLine3(false);
                    else if (i == 3)
                        EnableComplexSearchLine4(false);
                }
                else
                {
                    if (i == 0)
                        EnableComplexSearchLine1(true);
                    else if (i == 1)
                        EnableComplexSearchLine2(true);
                    else if (i == 2)
                        EnableComplexSearchLine3(true);
                    else if (i == 3)
                        EnableComplexSearchLine4(true);
                }
            }
        }

        private void button_complex_server1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string strServer = (string)this.m_aComplexServer[0];

            toolTip_serverUrl.SetToolTip(this.button_complex_server1,
                strServer);

        }

        private void button_complex_server2_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string strServer = (string)this.m_aComplexServer[1];

            toolTip_serverUrl.SetToolTip(this.button_complex_server2,
                strServer);

        }

        private void button_complex_server3_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string strServer = (string)this.m_aComplexServer[2];

            toolTip_serverUrl.SetToolTip(this.button_complex_server3,
                strServer);

        }

        private void button_complex_server4_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string strServer = (string)this.m_aComplexServer[3];

            toolTip_serverUrl.SetToolTip(this.button_complex_server4,
                strServer);

        }

        // tab页切换的时候, 设置缺省按钮
        private void tabControl_query_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_query.SelectedTab == this.tabPage_querySimple)
                this.AcceptButton = this.button_simpleQuerySearch;
            else if (this.tabControl_query.SelectedTab == this.tabPage_queryAdvance)
                this.AcceptButton = this.button_complexQuerySearch;
            else if (this.tabControl_query.SelectedTab == this.tabPage_queryXml)
                this.AcceptButton = this.button_xmlQuerySearch;
            else
                this.AcceptButton = null;

        }

        // 浏览标题列被点击
        private void listView_browse_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            // 排序
            // int nClickColumn = e.Column;
            // this.listView_browse.ListViewItemSorter = new ListViewBrowseItemComparer(nClickColumn);
            ListViewUtil.OnColumnClick(this.listView_browse, e);

        }

        // Implements the manual sorting of items by columns.
        class ListViewBrowseItemComparer : IComparer
        {
            private int col;
            public ListViewBrowseItemComparer()
            {
                col = 0;
            }
            public ListViewBrowseItemComparer(int column)
            {
                col = column;
            }
            public int Compare(object x, object y)
            {
                return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }
        }

        private void button_test_Click(object sender, EventArgs e)
        {
#if NO
            textBox_simpleQuery_comment.Text = "";

            // 第一阶段
            TargetItemCollection targets = treeView_simpleQueryResTree.
                GetSearchTarget();
            Debug.Assert(targets != null, "GetSearchTarget() 异常");

            int i;

            // 第二阶段
            for (i = 0; i < targets.Count; i++)
            {
                TargetItem item = (TargetItem)targets[i];
                item.Words = textBox_simpleQueryWord.Text;
            }
            targets.MakeWordPhrases(
                Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_split_words", 1)),
                Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_detect_range", 0)),
                Convert.ToBoolean(MainForm.applicationInfo.GetInt("simple_query_property", "auto_detect_relation", 0))
                );


            // 参数
            for (i = 0; i < targets.Count; i++)
            {
                TargetItem item = (TargetItem)targets[i];
                item.MaxCount = MainForm.applicationInfo.GetInt("simple_query_property", "maxcount", -1);
            }

            // 第三阶段
            targets.MakeXml();

            // 正式检索

            string strError;

            if (bOutputKeyID == true)
                this.m_bFirstColumnIsKey = true;
            else
                this.m_bFirstColumnIsKey = false;
            this.ClearListViewPropertyCache();
                            this.ClearListViewItems();

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在Test ...");
            stop.BeginLoop();

            EnableControlsInSearching(true);


            long lTotalCount = 0;	// 命中记录总数
            for (i = 0; i < targets.Count; i++)
            {

                if (stop.State >= 1)
                    break;

                TargetItem item = (TargetItem)targets[i];

                channel = Channels.GetChannel(item.Url);
                Debug.Assert(channel != null, "Channels.GetChannel 异常");

                textBox_simpleQuery_comment.Text += "检索式XML:\r\n" + item.Xml + "\r\n";

                // MessageBox.Show(this, item.Xml);
                long nRet = channel.DoTest(item.Xml);
                if (nRet == -1)
                {
                    textBox_simpleQuery_comment.Text += "Test出错: \r\n";
                    MessageBox.Show(this, textBox_simpleQuery_comment.Text);
                    continue;
                }
                lTotalCount += nRet;
                textBox_simpleQuery_comment.Text += "命中记录数: " + Convert.ToString(nRet) + "\r\n";

            }

            if (targets.Count > 1)
            {
                textBox_simpleQuery_comment.Text += "命中总条数: " + Convert.ToString(lTotalCount) + "\r\n";
            }

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            EnableControlsInSearching(false);

            channel = null;
#endif
        }

        private void listView_browse_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewUtil.OnSelectedIndexChanged(this.listView_browse,
0,
null);
            if (this.listView_browse.SelectedIndices.Count == 0)
                SetStatusMessage("");
            else
            {
                if (this.listView_browse.SelectedIndices.Count == 1)
                {
                    // this.label_message.Text = "第 " + (this._listviewRecords.SelectedIndices[0] + 1).ToString() + " 行";
                    SetStatusMessage("第 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行");
                }
                else
                {
                    SetStatusMessage("从 " + (this.listView_browse.SelectedIndices[0] + 1).ToString() + " 行开始，共选中 " + this.listView_browse.SelectedIndices.Count.ToString() + " 个事项");
                }
            }
        }

        // 在状态行显示文字信息
        void SetStatusMessage(string strMessage)
        {
            this.MainForm.StatusLabel = strMessage;
        }
    }
}
