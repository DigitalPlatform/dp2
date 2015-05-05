using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace DigitalPlatform.dp2.Statis
{

    /// <summary>
    /// 带有名字的文件的集合
    /// 便于用名字来管理文件对象
    /// </summary>
    public class NamedStringFileCollection : List<NamedStringFile>
    {
        public NamedStringFileCollection()
        {
        }

        public void AddLine(
            string strName,
            string strLine)
        {
            NamedStringFile file = GetStringFile(strName);

            // Debug.Assert(false, "");
            LineItem lineitem = new LineItem();
            lineitem.FileLine = new FileLine(strLine);
            file.StringFile.Add(lineitem);
        }

        // 获得一个适当的表格。如果没有找到，会自动创建
        public NamedStringFile GetStringFile(string strName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                NamedStringFile file = this[i];

                if (file.Name == strName)
                    return file;
            }

            // 没有找到。创建一个新的表
            NamedStringFile newFile = new NamedStringFile();
            newFile.Name = strName;
            newFile.StringFile = new StringFile();
            newFile.StringFile.Open(false);

            this.Add(newFile);
            return newFile;
        }

    }

    // 带有名字的表格
    public class NamedStringFile
    {
        public string Name = "";   // 名字
        public StringFile StringFile = null;
    }

}
