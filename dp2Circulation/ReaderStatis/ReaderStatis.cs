using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Web;

using DigitalPlatform.Xml;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.Script;

namespace dp2Circulation
{
    /// <summary>
    /// ReaderStatisForm (读者统计窗) 统计方案的宿主类
    /// </summary>
    public class ReaderStatis : StatisHostBase
    {
        /// <summary>
        /// 本对象所关联的 ReaderStatisForm (读者统计窗)
        /// </summary>
        public ReaderStatisForm ReaderStatisForm = null;	// 引用

        /// <summary>
        /// 从服务器端如何获取读者XML的格式？ 如果为 "advancexml"，则包含丰富的业务信息，但运行速度会稍慢。缺省为 "xml"
        /// </summary>
        public string XmlFormat = "xml";    // 从服务器端获取读者XML的格式。如果为advancexml，则包含丰富的业务信息，但运行速度会稍慢

#if NO
        private bool disposed = false;

                public WebBrowser Console = null;

                public string ProjectDir = "";  // 方案源文件所在目录
        public string InstanceDir = ""; // 当前实例独占的目录。用于存储临时文件

        public List<string> OutputFileNames = new List<string>(); // 存放输出的html文件

        int m_nFileNameSeed = 1;

#endif

        /// <summary>
        /// 办证日期范围。用于筛选参与同级的读者记录
        /// </summary>
        public string CreateTimeRange = "";

        /// <summary>
        /// (读者证)失效日期范围。用于筛选参与同级的读者记录
        /// </summary>
        public string ExpireTimeRange = "";

        /// <summary>
        /// 办证日期范围之开始时间
        /// </summary>
        public DateTime CreateStartDate = new DateTime(0);
        /// <summary>
        /// 办证日期范围之结束时间
        /// </summary>
        public DateTime CreateEndDate = new DateTime(0);

        /// <summary>
        /// 失效日期范围之开始时间
        /// </summary>
        public DateTime ExpireStartDate = new DateTime(0);
        /// <summary>
        /// 失效日期范围之结束时间
        /// </summary>
        public DateTime ExpireEndDate = new DateTime(0);

        /// <summary>
        /// 单位名列表。用于筛选参与同级的读者记录
        /// </summary>
        public string DepartmentNames = "";

        /// <summary>
        /// 读者类型列表。用于筛选参与同级的读者记录
        /// </summary>
        public string ReaderTypes = "";

        /// <summary>
        /// 当前读者记录路径
        /// </summary>
        public string CurrentRecPath = "";    // 当前读者记录路径

        /// <summary>
        /// 当前读者记录在整批中的下标
        /// </summary>
        public long CurrentRecordIndex = -1; // 当前读者记录在整批中的偏移量

        /// <summary>
        /// 当前读者记录的 XmlDocument 对象
        /// </summary>
        public XmlDocument ReaderDom = null;    // Xml装入XmlDocument

        string m_strXml = "";    // 读者记录体
        /// <summary>
        /// 当前正在处理的读者 XML 记录，字符串类型
        /// </summary>
        public string Xml
        {
            get
            {
                return this.m_strXml;
            }
            set
            {
                this.m_strXml = value;
            }
        }

        /// <summary>
        /// 当前读者记录的时间戳
        /// </summary>
        public byte[] Timestamp = null; // 当前读者记录的时间戳

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReaderStatis()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.ReaderStatisForm.MainForm.DataDir, "~reader_statis");
        }

#if NO
        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ReaderStatis()      
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }


        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // 删除所有输出文件
                if (this.OutputFileNames != null)
                {
                    Global.DeleteFiles(this.OutputFileNames);
                    this.OutputFileNames = null;
                }

                /*
                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;
                 * */
                try // 2008/6/26
                {
                    this.FreeResources();
                }
                catch
                {
                }

            }
            disposed = true;
        }

        public virtual void FreeResources()
        {

        }

        // 初始化
        public virtual void OnInitial(object sender, StatisEventArgs e)
        {

        }

        // 开始
        public virtual void OnBegin(object sender, StatisEventArgs e)
        {

        }

        // 每一记录处理
        public virtual void OnRecord(object sender, StatisEventArgs e)
        {

        }

        // 结束
        public virtual void OnEnd(object sender, StatisEventArgs e)
        {

        }

        // 打印输出
        public virtual void OnPrint(object sender, StatisEventArgs e)
        {

        }

        public void ClearConsoleForPureTextOutputing()
        {
            Global.ClearForPureTextOutputing(this.Console);
        }

        public void WriteToConsole(string strText)
        {
            Global.WriteHtml(this.Console, strText);
        }

        public void WriteTextToConsole(string strText)
        {
            Global.WriteHtml(this.Console, HttpUtility.HtmlEncode(strText));
        }

        // 获得一个新的输出文件名
        public string NewOutputFileName()
        {
            string strFileNamePrefix = this.ReaderStatisForm.MainForm.DataDir + "\\~reader_statis";

            string strFileName = strFileNamePrefix + "_" + this.m_nFileNameSeed.ToString() + ".html";

            this.m_nFileNameSeed++;

            this.OutputFileNames.Add(strFileName);

            return strFileName;
        }

        // 将字符串内容写入文本文件
        public void WriteToOutputFile(string strFileName,
            string strText,
            Encoding encoding)
        {
            StreamWriter sw = new StreamWriter(strFileName,
                false,	// append
                encoding);
            sw.Write(strText);
            sw.Close();
        }

        // 删除一个输出文件
        public void DeleteOutputFile(string strFileName)
        {
            int nIndex = this.OutputFileNames.IndexOf(strFileName);
            if (nIndex != -1)
                this.OutputFileNames.RemoveAt(nIndex);

            try
            {
                File.Delete(strFileName);
            }
            catch
            {
            }
        }
#endif
    }

    // 
    /// <summary>
    /// “带有名字表格”的集合
    /// </summary>
    public class NamedStatisTableCollection : List<NamedStatisTable>
    {
        int m_nColumnsHint = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nColumnHint">栏目数暗示</param>
        public NamedStatisTableCollection(int nColumnHint)
        {
            this.m_nColumnsHint = nColumnHint;
        }

        /// <summary>
        /// 增量一个单元的整数值
        /// </summary>
        /// <param name="strTableName">表格名字</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="createValue">创建值</param>
        /// <param name="incValue">增量值</param>
        public void IncValue(
            string strTableName,
            string strEntry,
            int nColumn,
            Int64 createValue,
            Int64 incValue)
        {
            NamedStatisTable table = GetTable(strTableName, this.m_nColumnsHint);
            table.Table.IncValue(strEntry,
                nColumn, createValue, incValue);
        }

        /// <summary>
        /// 增量一个单元的字符串值
        /// </summary>
        /// <param name="strTableName">表格名字</param>
        /// <param name="strEntry">事项名</param>
        /// <param name="nColumn">列号</param>
        /// <param name="createValue">创建值</param>
        /// <param name="incValue">增量值</param>
        public void IncValue(
            string strTableName,
            string strEntry,
            int nColumn,
            string createValue,
            string incValue)
        {
            NamedStatisTable table = GetTable(strTableName, this.m_nColumnsHint);
            table.Table.IncValue(strEntry,
                nColumn, createValue, incValue);
        }

        // 获得一个适当的表格。如果没有找到，会自动创建
        /// <summary>
        /// 获得一个适当的表格。如果当前不存在，会自动创建
        /// </summary>
        /// <param name="strTableName">表格名字</param>
        /// <param name="nColumnsHint">栏目数暗示</param>
        /// <returns>NamedStatisTable 类型的表格对象</returns>
        public NamedStatisTable GetTable(string strTableName,
            int nColumnsHint)
        {
            for (int i = 0; i < this.Count; i++)
            {
                NamedStatisTable table = this[i];


                if (table.Name == strTableName)
                    return table;
            }

            // 没有找到。创建一个新的表
            NamedStatisTable newTable = new NamedStatisTable();
            newTable.Name = strTableName;
            newTable.Table = new Table(nColumnsHint);

            this.Add(newTable);
            return newTable;
        }

    }

    // 
    /// <summary>
    /// 带有名字的表格
    /// </summary>
    public class NamedStatisTable
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name = "";   // 名字

        /// <summary>
        /// 表格
        /// </summary>
        public Table Table = null;
    }

}
