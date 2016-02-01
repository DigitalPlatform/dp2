using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;


namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// Summary description for GetUserNameDlg.
    /// </summary>
    public class GetUserNameDlg : System.Windows.Forms.Form
    {
        public ServerCollection Servers = null;	// 引用
        public RmsChannelCollection Channels = null;

        public DigitalPlatform.StopManager stopManager = null;

        // Channel channel = null;

        public string ServerUrl = "";

        public string Lang = "zh";


        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ColumnHeader columnHeader_userName;
        private System.Windows.Forms.ColumnHeader columnHeader_recPath;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public GetUserNameDlg()
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

                if (this.Channels != null)
                    this.Channels.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GetUserNameDlg));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_recPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_userName,
            this.columnHeader_recPath});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 12);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(384, 196);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader_userName
            // 
            this.columnHeader_userName.Text = "用户名";
            this.columnHeader_userName.Width = 218;
            // 
            // columnHeader_recPath
            // 
            this.columnHeader_recPath.Text = "帐户记录路径";
            this.columnHeader_recPath.Width = 281;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 217);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "用户名(&U):";
            // 
            // textBox_userName
            // 
            this.textBox_userName.Location = new System.Drawing.Point(91, 214);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(224, 21);
            this.textBox_userName.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.Location = new System.Drawing.Point(321, 214);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(321, 241);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // GetUserNameDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(408, 276);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GetUserNameDlg";
            this.ShowInTaskbar = false;
            this.Text = "指定用户名";
            this.Load += new System.EventHandler(this.GetUserNameDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public string SelectedUserName
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

        public string SelectedUserRecPath
        {
            get
            {
                if (this.listView1.SelectedItems.Count == 0)
                    return "";
                return this.listView1.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void GetUserNameDlg_Load(object sender, System.EventArgs e)
        {

        }

        public int Initial(ServerCollection servers,
            RmsChannelCollection channels,
            DigitalPlatform.StopManager stopManager,
            string serverUrl,
            out string strError)
        {
            this.Servers = servers;
            this.Channels = channels;
            this.stopManager = stopManager;
            this.ServerUrl = serverUrl;

            strError = "";
            int nRet = Fill(this.Lang,
                out strError);

            if (nRet == -1)
                return -1;
            return 0;
        }

        // 填充listview
        public int Fill(
            string strLang,
            out string strError)
        {
            listView1.Items.Clear();
            strError = "";

            string strQueryXml = "<target list='"
                + StringUtil.GetXmlStringSimple(Defs.DefaultUserDb.Name)      // 2007/9/14
                + ":" + "__id'><item><word>"
                + "" + "</word><match>left</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>zh</lang></target>";

            RmsChannel channel = Channels.GetChannel(this.ServerUrl);
            if (channel == null)
            {
                strError = "Channels.GetChannel 异常";
                return -1;
            }

            long nRet = channel.DoSearch(strQueryXml,
                "default",
                "", // strOuputStyle
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

            for (; ; )
            {

                ArrayList aLine = null;
                nRet = channel.DoGetSearchFullResult(
                    "default",
                    lStart,
                    lThisCount,
                    strLang,
                    null,	// stop,
                    out aLine,
                    out strError);
                if (nRet == -1)
                {
                    strError = "检索注册用户库获取检索结果时出错: " + strError;
                    return -1;
                }

                for (int i = 0; i < aLine.Count; i++)
                {
                    string[] acol = (string[])aLine[i];
                    if (acol.Length < 1)
                        continue;

                    ListViewItem item = null;

                    if (acol.Length < 2)
                    {
                        // 列中没有用户名, 用获取记录来补救?
                        item = new ListViewItem("", 0);
                    }
                    else
                    {
                        item = new ListViewItem(acol[1], 0);
                    }

                    this.listView1.Items.Add(item);
                    item.SubItems.Add(acol[0]);

                    if (item.Text == this.SelectedUserName)
                        item.Selected = true;
                }


                if (lStart + aLine.Count >= lTotalCount)
                    break;

                lStart += aLine.Count;
                lThisCount -= aLine.Count;


            }



            return 0;
        }

        private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.listView1.SelectedItems.Count == 0)
            {
                textBox_userName.Text = "";
                return;
            }

            textBox_userName.Text = this.listView1.SelectedItems[0].Text;

        }

        private void listView1_DoubleClick(object sender, System.EventArgs e)
        {
            button_OK_Click(null, null);

        }

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            if (textBox_userName.Text == "")
            {
                MessageBox.Show("尚未指定用户名");
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
    }
}
