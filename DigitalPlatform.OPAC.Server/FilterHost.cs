using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using DigitalPlatform.MarcDom;

namespace DigitalPlatform.OPAC.Server
{
    public class FilterHost
    {
        public OpacApplication App = null;
        public string RecPath = "";
        public string ResultString = "";
        public KeyValueCollection ResultParams = null;   // 2012/11/30
    }

    public class LoanFilterDocument : FilterDocument
    {
        public FilterHost FilterHost = null;
    }

    public class KeyValue
    {
        public string Key = "";
        public string Value = "";
    }

    public class KeyValueCollection : List<KeyValue>
    {
        public KeyValue Add(string strKey, string strValue)
        {
            KeyValue item = new KeyValue();
            item.Key = strKey;
            item.Value = strValue;
            this.Add(item);
            return item;
        }

        public KeyValue Insert(int index, string strKey, string strValue)
        {
            KeyValue item = new KeyValue();
            item.Key = strKey;
            item.Value = strValue;
            this.Insert(index, item);
            return item;
        }

        // 删除第一个匹配的项
        public KeyValue RemoveFirst(string strKey)
        {
            KeyValue found_item = null;
            foreach (KeyValue item in this)
            {
                if (item.Key == strKey)
                {
                    found_item = item;
                    break;
                }
            }

            if (found_item != null)
            {
                this.Remove(found_item);
            }

            return found_item;
        }

        // 删除全部key匹配的项
        // 返回已经删除的项
        public KeyValueCollection RemoveAll(string strKey)
        {
            KeyValueCollection items = new KeyValueCollection();
            foreach (KeyValue item in this)
            {
                if (item.Key == strKey)
                {
                    items.Add(item);
                }
            }

            if (items.Count > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    this.Remove(items[i]);
                    i--;
                }
            }

            return items;
        }

        // 查找特定的项
        public KeyValueCollection Find(string strKey)
        {
            KeyValueCollection items = new KeyValueCollection();
            foreach (KeyValue item in this)
            {
                if (item.Key == strKey)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        public KeyValueCollection this[string strKey]
        {
            get
            {
                return this.Find(strKey);
            }
        }

        // 获得第一个元素的Key
        public string Key
        {
            get
            {
                if (this.Count == 0)
                    return "";
                return this[0].Key;
            }
        }

        // 获得第一个元素的Value
        public string Value
        {
            get
            {
                if (this.Count == 0)
                    return "";
                return this[0].Value;
            }
        }
    }

    public class KeyValueComparer : IComparer<KeyValue>
    {
        int IComparer<KeyValue>.Compare(KeyValue x, KeyValue y)
        {
            int nRet = String.Compare(x.Key, y.Key);
            if (nRet != 0)
                return nRet;

            return String.Compare(x.Value, y.Value);
        }

    }
}
