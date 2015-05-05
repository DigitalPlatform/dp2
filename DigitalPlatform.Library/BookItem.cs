using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.Xml;
using System.Drawing;

using DigitalPlatform.Xml;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// 册信息
    /// </summary>
    public class BookItem
    {
        /// <summary>
        ///  册条码号
        /// </summary>
        public string Barcode = ""; 

        /// <summary>
        /// 册状态
        /// </summary>
        public string State = "";   

        /// <summary>
        /// 从属的书目记录id
        /// </summary>
        public string Parent = ""; 

        /// <summary>
        /// 馆藏地点
        /// </summary>
        public string Location = "";  

        /// <summary>
        /// 册价格
        /// </summary>
        public string Price = ""; 
        /// <summary>
        /// 图书类型
        /// </summary>
        public string BookType = "";  
        /// <summary>
        /// 注释
        /// </summary>
        public string Comment = ""; 
        /// <summary>
        /// 借书人证条码号
        /// </summary>
        public string Borrower = "";  
        /// <summary>
        /// 借书的日期
        /// </summary>
        public string BorrowDate = ""; 
        /// <summary>
        /// 借阅期限
        /// </summary>
        public string BorrowPeriod = ""; 

        /// <summary>
        ///  册记录路径
        /// </summary>
        public string RecPath = "";

        /// <summary>
        /// 是否被修改
        /// </summary>
        bool m_bChanged = false;

        /// <summary>
        /// 记录的dom
        /// </summary>
        public XmlDocument RecordDom = null;  

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        ListViewItem ListViewItem = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BookItem()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strRecPath"></param>
        /// <param name="dom"></param>
        public BookItem(string strRecPath, XmlDocument dom)
        {
            this.RecPath = strRecPath;
            this.RecordDom = dom;

            this.Initial();
        }


        /// <summary>
        /// 根据dom初始化各个成员
        /// </summary>
        /// <returns></returns>
        public int Initial()
        {
            if (this.RecordDom == null)
                return 0;

            this.Barcode = DomUtil.GetElementText(this.RecordDom.DocumentElement, "barcode");
            this.State = DomUtil.GetElementText(this.RecordDom.DocumentElement, "state");
            this.Location = DomUtil.GetElementText(this.RecordDom.DocumentElement, "location");
            this.Price = DomUtil.GetElementText(this.RecordDom.DocumentElement, "price");
            this.BookType = DomUtil.GetElementText(this.RecordDom.DocumentElement, "bookType");

            this.Comment = DomUtil.GetElementText(this.RecordDom.DocumentElement, "comment");
            this.Borrower = DomUtil.GetElementText(this.RecordDom.DocumentElement, "borrower");
            this.BorrowDate = DomUtil.GetElementText(this.RecordDom.DocumentElement, "borrowDate");
            this.BorrowPeriod = DomUtil.GetElementText(this.RecordDom.DocumentElement, "borrowPeriod");

            this.Parent = DomUtil.GetElementText(this.RecordDom.DocumentElement, "parent");
            m_bChanged = false;
            return 0;
        }

        /// <summary>
        /// 创建好适合于保存的记录信息
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int BuildRecord(
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (this.RecordDom == null)
            {
                this.RecordDom = new XmlDocument();
                this.RecordDom.LoadXml("<root />");
            }

            if (this.Parent == "")
            {
                strError = "Parent成员尚未定义";
                return -1;
            }

            if (this.Barcode == "")
            {
                strError = "Barcode成员尚未定义";
                return -1;
            }

            DomUtil.SetElementText(this.RecordDom.DocumentElement, "parent", this.Parent);

            DomUtil.SetElementText(this.RecordDom.DocumentElement, "barcode", this.Barcode);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "state", this.State);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "location", this.Location);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "price", this.Price);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "bookType", this.BookType);

            DomUtil.SetElementText(this.RecordDom.DocumentElement, "comment", this.Comment);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "borrower", this.Borrower);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "borrowDate", this.BorrowDate);
            DomUtil.SetElementText(this.RecordDom.DocumentElement, "borrowPeriod", this.BorrowPeriod);

            strXml = this.RecordDom.OuterXml;

            return 0;
        }

        /// <summary>
        /// 是否被修改过
        /// </summary>
        public bool Changed
        {
            get
            {
                return m_bChanged;
            }
            set
            {
                m_bChanged = value;
            }
        
        }


        /// <summary>
        /// 将本事项加入到listview中
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ListViewItem AddToListView(ListView list)
        {
            ListViewItem item = new ListViewItem(this.Barcode);

            item.SubItems.Add(this.State);
            item.SubItems.Add(this.Location);
            item.SubItems.Add(this.Price);
            item.SubItems.Add(this.BookType);
            item.SubItems.Add(this.Comment);
            item.SubItems.Add(this.Borrower);
            item.SubItems.Add(this.BorrowDate);
            item.SubItems.Add(this.BorrowPeriod);
            item.SubItems.Add(this.RecPath);

            this.SetItemBackColor(item);

            list.Items.Add(item);

            this.ListViewItem = item;

            return item;
        }

        void SetItemBackColor(ListViewItem item)
        {
            if (this.State == "" && this.m_bChanged == true)
            {
                // 新事项
               item.BackColor = Color.FromArgb(255, 255, 100); // 浅黄色
            }
            else if (this.m_bChanged == true)
            {
                // 修改过的旧事项
                item.BackColor = Color.FromArgb(100, 255, 100); // 浅绿色
            }
            else
            {
                item.BackColor = SystemColors.Window;
            }
        }

        /// <summary>
        /// 刷新事项颜色
        /// </summary>
        public void RefreshItemColor()
        {
            if (this.ListViewItem != null)
                this.SetItemBackColor(this.ListViewItem);
        }

    }

    /// <summary>
    /// 册信息的集合容器
    /// </summary>
    public class BookItemCollection : List<BookItem>
    {

        /// <summary>
        /// 以册条码号定位一个事项
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <returns></returns>
        public BookItem GetItem(string strBarcode)
        {
            for (int i = 0; i < this.Count; i++)
            {
                BookItem item = this[i];
                if (item.Barcode == strBarcode)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// 是否修改过
        /// </summary>
        public bool Changed
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    BookItem item = this[i];
                    if (item.Changed == true)
                        return true;
                }

                return false;
            }

            set
            {
                for (int i = 0; i < this.Count; i++)
                {
                    BookItem item = this[i];
                    if (item.Changed != value)
                        item.Changed = value;
                }
            }
        }
    }
}
