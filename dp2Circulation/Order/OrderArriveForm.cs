using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 订购验收对话框
    /// </summary>
    internal partial class OrderArriveForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        // 事项数组
        public List<DigitalPlatform.CommonControl.Item> Items
        {
            get
            {
                return this.orderDesignControl1.Items;
            }
        }

        public OrderArriveForm()
        {
            InitializeComponent();
        }

        private void OrderArriveForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }
            this.orderDesignControl1.GetValueTable -= new DigitalPlatform.GetValueTableEventHandler(orderCrossControl1_GetValueTable);
            this.orderDesignControl1.GetValueTable += new DigitalPlatform.GetValueTableEventHandler(orderCrossControl1_GetValueTable);

            // 如果窗口打开的时候，发现一个事项也没有，就需要加入一个空白事项，以便用户在此基础上进行编辑
            if (this.orderDesignControl1.Items.Count == 0)
            {
                this.orderDesignControl1.RemoveMultipleZeroCopyItem();
            }
        }

        void orderCrossControl1_GetValueTable(object sender, DigitalPlatform.GetValueTableEventArgs e)
        {
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

        // 验收目标记录路径
        public string TargetRecPath
        {
            get
            {
                return this.orderDesignControl1.TargetRecPath;
            }
            set
            {
                this.orderDesignControl1.TargetRecPath = value;
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
    }
}