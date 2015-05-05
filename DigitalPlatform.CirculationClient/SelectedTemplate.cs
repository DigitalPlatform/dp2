using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalPlatform.CirculationClient
{
    public class SelectedTemplate
    {
        public string DbName = "";
        public bool NotAskDbName = false;
        public string TemplateName = "";
        public bool NotAskTemplateName = false;
    }

    public class SelectedTemplateCollection : List<SelectedTemplate>
    {
        public SelectedTemplate Find(string strDbName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                SelectedTemplate temp = this[i];
                if (temp.DbName == strDbName)
                    return temp;
            }

            return null;
        }

        // 根据数据库名获得模板名
        public string Get(string strDbName)
        {
            SelectedTemplate temp = this.Find(strDbName);
            if (temp == null)
            {
                return null;
            }

            return temp.TemplateName;
        }

        public void Set(string strDbName,
            bool bNotAskDbName,
            string strSelectedTemplateName,
            bool bNotAskTemplateName)
        {
            SelectedTemplate temp = this.Find(strDbName);
            if (temp == null)
            {
                temp = new SelectedTemplate();
                temp.DbName = strDbName;
                temp.NotAskDbName = bNotAskDbName;
                temp.TemplateName = strSelectedTemplateName;
                temp.NotAskTemplateName = bNotAskTemplateName;
                this.Add(temp);
            }
            else
            {
                temp.NotAskDbName = bNotAskDbName;
                temp.TemplateName = strSelectedTemplateName;
                temp.NotAskTemplateName = bNotAskTemplateName;
            }
        }

        // 从字符串创建整个内存对象
        public void Build(string strContent)
        {
            this.Clear();

            string[] sections = strContent.Split(new char[] { ';' });
            for (int i = 0; i < sections.Length; i++)
            {
                string strSection = sections[i].Trim();
                if (String.IsNullOrEmpty(strSection) == true)
                    continue;
                string strDbName = "";
                string strNotAskDbName = "";
                string strTemplateName = "";
                string strNotAskTemplateName = "";

                string[] parts = strSection.Split(new char[] { ',' });

                if (parts.Length > 0)
                    strDbName = parts[0].Trim();

                if (parts.Length > 1)
                    strNotAskDbName = parts[1].Trim();

                if (parts.Length > 2)
                    strTemplateName = parts[2].Trim();

                if (parts.Length > 3)
                    strNotAskTemplateName = parts[3].Trim();

                SelectedTemplate temp = new SelectedTemplate();
                temp.DbName = strDbName;
                temp.NotAskDbName = (strNotAskDbName == "true" ? true : false);
                temp.TemplateName = strTemplateName;
                temp.NotAskTemplateName = (strNotAskTemplateName == "true" ? true : false);

                this.Add(temp);
            }
        }

        // 把内存对象输出到字符串
        public string Export()
        {
            string strContent = "";

            for (int i = 0; i < this.Count; i++)
            {
                SelectedTemplate temp = this[i];

                if (i != 0)
                    strContent += ";";

                strContent += temp.DbName + ",";
                strContent += (temp.NotAskDbName == true ? "true" : "false") + ",";
                strContent += temp.TemplateName + ",";
                strContent += (temp.NotAskTemplateName == true ? "true" : "false");
            }

            return strContent;
        }
    }

}
