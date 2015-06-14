using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;

namespace dp2Circulation
{
    /// <summary>
    /// 管理期信息的对话框。即将废止
    /// </summary>
    internal partial class IssueManageForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        public event GetOrderInfoEventHandler GetOrderInfo = null;

        /// <summary>
        /// 获得册信息
        /// </summary>
        public event GetItemInfoEventHandler GetItemInfo = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        // public event GenerateEntityEventHandler GenerateEntity = null;

        public List<string> DeletingIds
        {
            get
            {
                return this.issueManageControl1.DeletingIds;
            }
        }

        public IssueManageForm()
        {
            InitializeComponent();
        }

        private void IssueManageForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            // 把事件挂到控件上面
            this.issueManageControl1.GetOrderInfo -= new GetOrderInfoEventHandler(issueManageControl1_GetOrderInfo);
            this.issueManageControl1.GetOrderInfo += new GetOrderInfoEventHandler(issueManageControl1_GetOrderInfo);

            /*
            this.issueManageControl1.GetItemInfo -= new GetItemInfoEventHandler(issueManageControl1_GetItemInfo);
            this.issueManageControl1.GetItemInfo += new GetItemInfoEventHandler(issueManageControl1_GetItemInfo);
            */

            this.issueManageControl1.GetValueTable -= new DigitalPlatform.GetValueTableEventHandler(issueManageControl1_GetValueTable);
            this.issueManageControl1.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(issueManageControl1_GetValueTable);
            /*
            this.issueManageControl1.GenerateEntity -= new GenerateEntityEventHandler(issueManageControl1_GenerateEntity);
            this.issueManageControl1.GenerateEntity += new GenerateEntityEventHandler(issueManageControl1_GenerateEntity);
             * */

            this.issueManageControl1.Sort();    // 排序 2009/2/8
        }

        void issueManageControl1_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }

        /*
        void issueManageControl1_GenerateEntity(object sender, GenerateEntityEventArgs e)
        {
            if (this.GenerateEntity != null)
                this.GenerateEntity(sender, e);
        }
         * */

        void issueManageControl1_GetOrderInfo(object sender, GetOrderInfoEventArgs e)
        {
            if (this.GetOrderInfo != null)
                this.GetOrderInfo(sender, e);
        }

        void issueManageControl1_GetItemInfo(object sender, GetItemInfoEventArgs e)
        {
            if (this.GetItemInfo != null)
                this.GetItemInfo(sender, e);
        }

        private void IssueManageForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void IssueManageForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            // 注意控件里面最后一次收尾update
            this.issueManageControl1.UpdateTreeNodeInfo();

            if (this.Changed == true)
            {
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.issueManageControl1.Changed;
            }
            set
            {
                this.issueManageControl1.Changed = value;
            }
        }

        internal List<IssueManageItem> Items
        {
            get
            {
                return this.issueManageControl1.Items;
            }
        }

        internal IssueManageItem AppendNewItem(string strXml,
    out string strError)
        {
            return this.issueManageControl1.AppendNewItem(strXml, out strError);
        }

        // 获取值列表时作为线索的数据库名
        public string BiblioDbName
        {
            get
            {
                return this.issueManageControl1.BiblioDbName;
            }
            set
            {
                this.issueManageControl1.BiblioDbName = value;
            }
        }
    }
}