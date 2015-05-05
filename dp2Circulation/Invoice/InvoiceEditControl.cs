using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.Xml;

namespace dp2Circulation.Invoice
{
    /// <summary>
    /// 发票记录编辑控件
    /// </summary>
    internal partial class InvoiceEditControl : UserControl
    {
        XmlDocument RecordDom = null;

        bool m_bChanged = false;

        bool m_bInInitial = true;   // 是否正在初始化过程之中

        Color ColorChanged = Color.Yellow; // 表示内容改变过的颜色
        Color ColorDifference = Color.Blue; // 表示内容有差异的颜色

        public InvoiceEditControl()
        {
            InitializeComponent();
        }

        #region 数据成员

        public string OldRecord = "";
        public byte[] Timestamp = null;

        // 发票号
        public string No
        {
            get
            {
                return this.textBox_no.Text;
            }
            set
            {
                this.textBox_no.Text = value;
            }
        }

        // 状态
        public string State
        {
            get
            {
                return this.comboBox_state.Text;
            }
            set
            {
                this.comboBox_state.Text = value;
            }
        }

        // ??
        public string CatalogNo
        {
            get
            {
                return this.textBox_catalogNo.Text;
            }
            set
            {
                this.textBox_catalogNo.Text = value;
            }
        }

        // 书商
        public string Seller
        {
            get
            {
                return this.comboBox_seller.Text;
            }
            set
            {
                this.comboBox_seller.Text = value;
            }
        }

        // 经费来源
        public string Source
        {
            get
            {
                return this.comboBox_source.Text;
            }
            set
            {
                this.comboBox_source.Text = value;
            }
        }

        // ??
        public string Range
        {
            get
            {
                return this.textBox_range.Text;
            }
            set
            {
                this.textBox_range.Text = value;
            }
        }

        // ??
        public string IssueCount
        {
            get
            {
                return this.textBox_issueCount.Text;
            }
            set
            {
                this.textBox_issueCount.Text = value;
            }
        }

        // 种数
        public string BiblioCount
        {
            get
            {
                return this.textBox_biblioCount.Text;
            }
            set
            {
                this.textBox_biblioCount.Text = value;
            }
        }

        // 册数
        public string ItemCount
        {
            get
            {
                return this.textBox_itemCount.Text;
            }
            set
            {
                this.textBox_itemCount.Text = value;
            }
        }

        // 总金额
        public string TotalPrice
        {
            get
            {
                return this.textBox_totalPrice.Text;
            }
            set
            {
                this.textBox_totalPrice.Text = value;
            }
        }

        // 报销时间
        public string OrderTime
        {
            get
            {
                return this.textBox_orderTime.Text;
            }
            set
            {
                this.textBox_orderTime.Text = value;
            }
        }

        // ??
        public string OrderID
        {
            get
            {
                return this.textBox_orderID.Text;
            }
            set
            {
                this.textBox_orderID.Text = value;
            }
        }

        // ??
        public string Distribute
        {
            get
            {
                return this.textBox_distribute.Text;
            }
            set
            {
                this.textBox_distribute.Text = value;
            }
        }

        // ??
        public string Class
        {
            get
            {
                return this.comboBox_class.Text;
            }
            set
            {
                this.comboBox_class.Text = value;
            }
        }

        // 注释
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

        // 验收批次号
        public string BatchNo
        {
            get
            {
                return this.textBox_batchNo.Text;
            }
            set
            {
                this.textBox_batchNo.Text = value;
            }
        }

        // 书商地址
        public string SellerAddress
        {
            get
            {
                return this.textBox_sellerAddress.Text;
            }
            set
            {
                this.textBox_sellerAddress.Text = value;
            }
        }

        public string RecPath
        {
            get
            {
                return this.textBox_recPath.Text;
            }
            set
            {
                this.textBox_recPath.Text = value;
            }
        }

        public string RefID
        {
            get
            {
                return this.textBox_refID.Text;
            }
            set
            {
                this.textBox_refID.Text = value;
            }
        }

        // 2010/4/8
        public string Operations
        {
            get
            {
                return this.textBox_operations.Text;
            }
            set
            {
                this.textBox_operations.Text = value;
            }
        }

        #endregion

    }
}
