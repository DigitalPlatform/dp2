using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 规划订购对话框
    /// 用来分配订购的复本数
    /// </summary>
    internal partial class OrderDesignForm : Form
    {
        public DateTime? FocusedTime = null;

        const int WM_SETCARETPOS = API.WM_USER + 201;

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        /// <summary>
        /// 获得缺省记录
        /// </summary>
        public event GetDefaultRecordEventHandler GetDefaultRecord = null;
        // 2012/10/4
        /// <summary>
        /// 检查馆代码是否在管辖范围内
        /// </summary>
        public event VerifyLibraryCodeEventHandler VerifyLibraryCode = null;

        // 事项数组
        public List<DigitalPlatform.CommonControl.Item> Items
        {
            get
            {
                return this.orderDesignControl1.Items;
            }
        }

        public OrderDesignForm()
        {
            InitializeComponent();
        }

        private void OrderDesignForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
            this.orderDesignControl1.GetValueTable -= new DigitalPlatform.GetValueTableEventHandler(orderCrossControl1_GetValueTable);
            this.orderDesignControl1.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(orderCrossControl1_GetValueTable);

            this.orderDesignControl1.GetDefaultRecord -= new GetDefaultRecordEventHandler(orderCrossControl1_GetDefaultRecord);
            this.orderDesignControl1.GetDefaultRecord += new GetDefaultRecordEventHandler(orderCrossControl1_GetDefaultRecord);

            this.orderDesignControl1.VerifyLibraryCode -= new VerifyLibraryCodeEventHandler(orderDesignControl1_VerifyLibraryCode);
            this.orderDesignControl1.VerifyLibraryCode += new VerifyLibraryCodeEventHandler(orderDesignControl1_VerifyLibraryCode);

            // 如果窗口打开的时候，发现一个事项也没有，就需要加入一个空白事项，以便用户在此基础上进行编辑
            if (this.orderDesignControl1.Items.Count == 0)
            {
                try
                {
                    // TODO: 需要删除缺省就在里面的copy为0的唯一事项，然后增加一个copy为0的事项。新增加的事项会有批次号等信息。
                    this.orderDesignControl1.InsertNewItem(0);  // this.orderDesignControl1.Items.Count
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message);
                }

                this.orderDesignControl1.RemoveMultipleZeroCopyItem();
            }
            if (this.FocusedTime != null)
                API.PostMessage(this.Handle, WM_SETCARETPOS, 0, 0);
        }

        void orderDesignControl1_VerifyLibraryCode(object sender, VerifyLibraryCodeEventArgs e)
        {
            if (this.VerifyLibraryCode != null)
                this.VerifyLibraryCode(sender, e);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_SETCARETPOS:
                    {
                        EnsureCurrentVisible(this.FocusedTime);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }

        // 确保和当前日期有关的事项滚入视野
        public void EnsureCurrentVisible(DateTime? time)
        {
            if (time == null)
                return;

            if (this.orderDesignControl1.Items.Count > 0)
            {
                string strTime = DateTimeUtil.DateTimeToString8((DateTime)time);
                int nCount = 0;
                foreach (DigitalPlatform.CommonControl.Item item in this.orderDesignControl1.Items)
                {
                    if (item.InRange(strTime) == true)
                    {
                        this.orderDesignControl1.EnsureVisible(item);
                        this.orderDesignControl1.SelectItem(item, nCount == 0 ? true : false);
                        nCount++;
                    }
                    // TODO: 如果没有精确匹配的，还可以计算出和当前时间距离最近的
                    // 如果时间范围为空，还可以看订购时间
                }

                if (nCount == 0)
                {
                    DigitalPlatform.CommonControl.Item item = this.orderDesignControl1.Items[this.orderDesignControl1.Items.Count - 1];
                    this.orderDesignControl1.EnsureVisible(item);
                    this.orderDesignControl1.SelectItem(item, true);
                }
            }
        }

        void orderCrossControl1_GetDefaultRecord(object sender, GetDefaultRecordEventArgs e)
        {
            if (this.GetDefaultRecord != null)
                this.GetDefaultRecord(sender, e);
        }

        void orderCrossControl1_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.orderDesignControl1.Changed;
            }
            set
            {
                this.orderDesignControl1.Changed = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 进行检查
            // return:
            //      -1  函数运行出错
            //      0   检查没有发现错误
            //      1   检查发现了错误
            int nRet = this.orderDesignControl1.Check(out strError);
            if (nRet != 0)
            {
                if (nRet == 1)
                {
                    strError = "经检查发现数据不规范问题:\r\n\r\n" + strError;
                }
                goto ERROR1;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 包装已有的函数
        public DigitalPlatform.CommonControl.Item AppendNewItem(string strOrderXml,
            out string strError)
        {
            return this.orderDesignControl1.AppendNewItem(strOrderXml, out strError);
        }

        // 包装已有的函数
        public void ClearAllItems()
        {
            this.orderDesignControl1.Clear();
        }


        public bool SeriesMode
        {
            get
            {
                return this.orderDesignControl1.SeriesMode;
            }
            set
            {
                this.orderDesignControl1.SeriesMode = value;
            }
        }

        // 获取值列表时作为线索的数据库名
        public string BiblioDbName
        {
            get
            {
                return this.orderDesignControl1.BiblioDbName;
            }
            set
            {
                this.orderDesignControl1.BiblioDbName = value;
            }
        }

        public bool CheckDupItem
        {
            get
            {
                return this.orderDesignControl1.CheckDupItem;
            }
            set
            {
                this.orderDesignControl1.CheckDupItem = value;
            }
        }
    }
}