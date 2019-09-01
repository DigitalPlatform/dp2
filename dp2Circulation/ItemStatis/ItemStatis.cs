using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Web;

using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.dp2.Statis;
using DigitalPlatform.CommonControl;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

// 2013/3/26 添加 XML 注释

namespace dp2Circulation
{
    /// <summary>
    /// ItemStatisForm (册统计窗) 统计方案的宿主类
    /// </summary>
    public class ItemStatis : StatisHostBase
    {
        // private bool disposed = false;

        /// <summary>
        /// 馆藏地点列表
        /// </summary>
        public string LocationNames = "";

        /// <summary>
        /// 当前册记录的时间戳
        /// </summary>
        public byte[] Timestamp = null; // 当前册记录的时间戳 2009/9/26

        // public WebBrowser Console = null;

        /// <summary>
        /// 本对象所关联的 ItemStatisForm (册统计窗)
        /// </summary>
        public ItemStatisForm ItemStatisForm = null;	// 引用

        /// <summary>
        /// 当前册记录路径
        /// </summary>
        public string CurrentRecPath = "";    // 当前册记录路径
        /// <summary>
        /// 当前册记录在整批中的下标。从 0 开始计数。如果为 -1，表示尚未开始处理
        /// </summary>
        public long CurrentRecordIndex = -1; // 当前册记录在整批中的偏移量

        /// <summary>
        /// 当前册记录所从属的书目记录路径
        /// </summary>
        public string CurrentBiblioRecPath = "";    // 当前书目记录路径（指册记录从属的书目记录）

#if NO
        public string ProjectDir = "";  // 方案源文件所在目录
        public string InstanceDir = ""; // 当前实例独占的目录。用于存储临时文件

        public List<string> OutputFileNames = new List<string>(); // 存放输出的html文件

        int m_nFileNameSeed = 1;
#endif
        /// <summary>
        /// 当前正在处理的册 XML 记录，XmlDocument 类型
        /// </summary>
        public XmlDocument ItemDom = null;    // Xml装入XmlDocument

        string m_strXml = "";    // 册记录体
        /// <summary>
        /// 当前正在处理的册 XML 记录，字符串类型
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

        PromptManager _prompt = new PromptManager(2);

        internal string m_strBiblioXml = "";

        /// <summary>
        /// 当前正在处理的册记录所从属的书目 XML 记录，字符串类型
        /// </summary>
        public string BiblioXml
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strBiblioXml) == false)
                    return this.m_strBiblioXml;

                if (string.IsNullOrEmpty(this.CurrentBiblioRecPath) == true)
                    throw new Exception("CurrentBiblioRecPath为空，无法取得书目记录");

                REDO:
                int nRet = this.ItemStatisForm.GetBiblioInfo(this.CurrentBiblioRecPath,
                    "xml",
                    out string strBiblioXml,
                    out string strError);
                if (nRet == -1)
                {
                    // 2019/8/31 增加重试机制
                    MessagePromptEventArgs e = new MessagePromptEventArgs
                    {
                        MessageText = $"获得书目记录 '{this.CurrentBiblioRecPath}' 时出错：{strError}\r\n\r\n是否重试?\r\n\r\n(重试) 重试操作; (中断) 中断处理",
                        IncludeOperText = true,
                        ButtonCaptions = new string[] { "重试", "中断" },
                        Actions = "yes,cancel"
                    };
                    _prompt.Prompt(this.ItemStatisForm, e);
                    if (e.ResultAction == "cancel")
                        throw new Exception("获得书目记录时出错: " + strError);
                    else if (e.ResultAction == "yes")
                        goto REDO;
                    else
                    {
                        // 返回空字符串表示想跳过
                        strBiblioXml = null;
                    }

                    // throw new Exception("获得书目记录时出错: " + strError);
                }

                this.m_strBiblioXml = strBiblioXml;
                return this.m_strBiblioXml;
            }
        }

        internal XmlDocument m_biblioDom = null;

        /// <summary>
        /// 当前正在处理的册记录所从属的书目 XML 记录，XmlDocument 类型
        /// </summary>
        public XmlDocument BiblioDom
        {
            get
            {
                if (this.m_biblioDom != null)
                    return this.m_biblioDom;

                this.m_biblioDom = new XmlDocument();
                try
                {
                    this.m_biblioDom.LoadXml(this.BiblioXml);
                }
                catch (Exception ex)
                {
                    this.m_biblioDom = null;
                    throw ex;
                }

                return this.m_biblioDom;
            }
        }

        internal string m_strMarcRecord = "";
        internal string m_strMarcSyntax = "";
        /// <summary>
        /// 当前正在处理的册记录所从属的书目记录， MARC 字符串
        /// </summary>
        public string MarcRecord
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strMarcRecord) == false)
                    return this.m_strMarcRecord;

                // 将XML书目记录转换为MARC格式
                string strOutMarcSyntax = "";
                string strMarc = "";
                string strError = "";

                // 将MARCXML格式的xml记录转换为marc机内格式字符串
                // parameters:
                //		bWarning	== true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
                //		strMarcSyntax	指示marc语法,如果==""，则自动识别
                //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
                int nRet = MarcUtil.Xml2Marc(this.BiblioXml,
                    true,   // 2013/1/12 修改为true
                    "", // strMarcSyntax
                    out strOutMarcSyntax,
                    out strMarc,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                this.m_strMarcSyntax = strOutMarcSyntax;
                this.m_strMarcRecord = strMarc;
                return this.m_strMarcRecord;
            }
        }

        /// <summary>
        /// 当前正在处理的册记录所从属的书目记录的 MARC 格式。usmarc / unimarc
        /// </summary>
        public string MarcSyntax
        {
            get
            {
                // 促使MARC格式被获得
                if (string.IsNullOrEmpty(this.m_strMarcSyntax) == true)
                {
                    string strTemp = this.MarcRecord;
                }

                return this.m_strMarcSyntax;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ItemStatis()
        {
            //
            // TODO: Add constructor logic here
            //
        }

#if NO
        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~ItemStatis()
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
            // Therefore, you should call GC.SuppressFinalize to
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

                try // 2008/11/28
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
            string strFileNamePrefix = this.ItemStatisForm.MainForm.DataDir + "\\~item_statis";

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
        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.ItemStatisForm.MainForm.DataDir, "~item_statis");
        }

        /// <summary>
        /// 针对当前册记录所从属的书目记录执行 MARC 过滤器
        /// </summary>
        public void DoMarcFilter()
        {
            string strError = "";
            string strMarcRecord = this.MarcRecord;
            if (string.IsNullOrEmpty(strMarcRecord) == false)
            {
                this.ItemStatisForm.DoMarcFilter(
                    (int)this.CurrentRecordIndex,
                    strMarcRecord,
                    this.MarcSyntax,
                    out strError);
            }
        }
    }

}

