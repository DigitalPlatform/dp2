using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace dp2Catalog
{
    public class VirtualItemCollection : List<VirtualItem>
    {
        List<int> m_selectedIndices = null;

        public void ExpireSelectedIndices()
        {
            this.m_selectedIndices = null;
        }

        //
        // 摘要:
        //     从 System.Collections.Generic.List<T> 中移除所有元素。
        public new void Clear()
        {
            base.Clear();
            this.ExpireSelectedIndices();
        }

        // 摘要:
        //     将对象添加到 System.Collections.Generic.List<T> 的结尾处。
        //
        // 参数:
        //   item:
        //     要添加到 System.Collections.Generic.List<T> 的末尾处的对象。对于引用类型，该值可以为 null。
        public new void Add(VirtualItem item)
        {
            base.Add(item);
            this.ExpireSelectedIndices();
        }
        //
        // 摘要:
        //     将指定集合的元素添加到 System.Collections.Generic.List<T> 的末尾。
        //
        // 参数:
        //   collection:
        //     一个集合，其元素应被添加到 System.Collections.Generic.List<T> 的末尾。集合自身不能为 null，但它可以包含为
        //     null 的元素（如果类型 T 为引用类型）。
        //
        // 异常:
        //   System.ArgumentNullException:
        //     collection 为 null。
        public new void AddRange(IEnumerable<VirtualItem> collection)
        {
            base.AddRange(collection);
            this.ExpireSelectedIndices();
        }


        public List<int> SelectedIndices
        {
            get
            {
                if (m_selectedIndices != null)
                    return m_selectedIndices;

                this.m_selectedIndices = new List<int>();

                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Selected == true)
                        m_selectedIndices.Add(i);
                }
                return m_selectedIndices;
            }
            set
            {
                // 先全部清除
                for (int i = 0; i < this.Count; i++)
                {
                    this[i].Selected = false;
                }
                // 然后设置
                for (int i = 0; i < value.Count; i++)
                {
                    int index = value[i];
                    this[index].Selected = true;
                }

            }
        }


    }

    public class VirtualItem
    {
        public bool Selected = false;

        public List<string> SubItems = null;

        public int ImageIndex = -1;

        public object Tag = null;

        public VirtualItem(string strFirstSubItemText,
            int nImageIndex)
        {
            if (this.SubItems == null)
                this.SubItems = new List<string>();

            if (this.SubItems.Count == 0)
                this.SubItems.Add(strFirstSubItemText);
            else
                this.SubItems[0] = strFirstSubItemText;

            this.ImageIndex = nImageIndex;
        }

        public ListViewItem GetListViewItem(int nColumnCount)
        {
            string strFirstSubItemText = "";
            if (this.SubItems != null && this.SubItems.Count > 0)
                strFirstSubItemText = this.SubItems[0];
            ListViewItem item = new ListViewItem(strFirstSubItemText,
                this.ImageIndex);
            for (int i = 1; i < nColumnCount; i++)
            {
                string strText = "";
                if (i<this.SubItems.Count)
                    strText = this.SubItems[i];
                item.SubItems.Add(strText);
            }

            item.Selected = this.Selected;

            return item;
        }
    }
}
