using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using DigitalPlatform.rms.Client;

namespace dp2Manager
{

    [Serializable()]
    public class Template
    {
        public DatabaseObject Object = null;
        public List<string[]> LogicNames = null;
        public string Type = "";
        public string SqlDbName = "";
        public string KeysDef = "";
        public string BrowseDef = "";

    }

    [Serializable()]
    public class TemplateCollection
    {
        List<Template> m_list = null;

        public TemplateCollection()
        {
            m_list = new List<Template>();
        }

        public void Add(Template o)
        {
            m_list.Add(o);
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        public Template this[int index]
        {
            get
            {
                return m_list[index];
            }
        
        }

        // 从文件中装载创建一个TemplateCollection对象
        // parameters:
        //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
        //							如果==true，函数直接返回一个新的空TemplateCollection对象
        // Exception:
        //			FileNotFoundException	文件没找到
        //			SerializationException	版本迁移时容易出现
        public static TemplateCollection Load(
            string strFileName,
            bool bIgnorFileNotFound)
        {
            Stream stream = null;
            TemplateCollection templates = null;

            try
            {
                stream = File.Open(strFileName, FileMode.Open);
            }
            catch (FileNotFoundException ex)
            {
                if (bIgnorFileNotFound == false)
                    throw ex;

                templates = new TemplateCollection();
                // templates.m_strFileName = strFileName;

                // 让调主有一个新的空对象可用
                return templates;
            }


            BinaryFormatter formatter = new BinaryFormatter();

            templates = (TemplateCollection)formatter.Deserialize(stream);
            stream.Close();
            // templates.m_strFileName = strFileName;


            return templates;
        }

        // 保存到文件
        // parameters:
        //		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
        public void Save(string strFileName)
        {
            if (String.IsNullOrEmpty(strFileName) == true)
            {
                throw (new Exception("TemplateCollection.Save()没有指定保存文件名"));
            }

            Stream stream = File.Open(strFileName,
                FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, this);
            stream.Close();
        }

    }
}
