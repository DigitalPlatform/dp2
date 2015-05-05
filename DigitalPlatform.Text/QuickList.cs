using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.Text
{
    public class QuickList
    {
        string m_strText = "";

        Hashtable m_table = new Hashtable();

        public string Text
        {
            get
            {
                return m_strText;
            }
            set
            {
                this.m_strText = value;

                BuildTable(value);
            }
        }

        // 创建Hashtable
        void BuildTable(string strText)
        {
            this.m_table.Clear();
            string[] parts = strText.Split(new char[] {','});
            foreach (string s in parts)
            {
                string strPart = s.Trim();
                if (string.IsNullOrEmpty(strPart) == true)
                    continue;
                this.m_table[strPart.ToLower()] = true;
            }
        }

        public bool IsInList(string strSub)
        {
            if (string.IsNullOrEmpty(strSub) == true)
                return false;

            strSub = strSub.Trim();
            if (string.IsNullOrEmpty(strSub) == true)
                return false;

            if (this.m_table.Contains(strSub.ToLower()) == true)
                return true;
            return false;
        }
    }
}
