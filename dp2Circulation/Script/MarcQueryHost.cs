using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Web;

using DigitalPlatform.Script;
using DigitalPlatform.Marc;

namespace dp2Circulation
{
    /// <summary>
    /// MarcQuery 宿主类。
    /// 继承了MarcQuery类，具有一些全局的函数
    /// </summary>
    public class MarcQueryHost : MarcQuery
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        /// <summary>
        /// 当前窗口所从属的框架窗口
        /// </summary>
        public virtual MainForm MainForm
        {
            get
            {
                return Program.MainForm;
            }
            set
            {
                // 为了让脚本代码能兼容
            }
        }

        /// <summary>
        /// 当前记录路径
        /// </summary>
        public string RecordPath = "";

        /// <summary>
        /// 当前 MARC 记录的 MarcRecord 对象
        /// </summary>
        public MarcRecord MarcRecord = null;

        /// <summary>
        /// 当前 MARC 记录的 MARC 格式。为 unimarc/usmarc 之一
        /// </summary>
        public string MarcSyntax = "";

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;

        // public string SUBFLD = new string((char)31, 1);

        /// <summary>
        /// 源代码文件名全路径
        /// </summary>
        public string CodeFileName = "";

        /// <summary>
        /// 视觉事项对象。
        /// 一般是一个 ListViewItem 对象，代表当前正在处理的浏览行
        /// </summary>
        public object UiItem = null;

        /// <summary>
        /// 视觉窗体对象。
        /// 一般是一个特定的 Form 派生类对象，代表当前正在处理的 MDI 窗口
        /// </summary>
        public object UiForm = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MarcQueryHost()
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
        /// <summary>
        /// 析构函数
        /// </summary>
        ~MarcQueryHost()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
#endif

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        /// <summary>
        /// Dispose
        /// </summary>
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

        private bool disposed = false;
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

                /*
                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;
                 * */
                if (_freed == false)
                {
                    try
                    {
                        this.FreeResources();
                    }
                    catch
                    {
                    }
                }
            }
            disposed = true;
        }


#if NO
        public virtual void Main()
        {

        }
#endif

        bool _freed = false;
        /// <summary>
        /// 释放资源。在本对象被摧毁前调用
        /// </summary>
        public virtual void FreeResources()
        {
            _freed = true;
        }

        /// <summary>
        /// 初始化。在统计方案执行的第一阶段被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnInitial(object sender, StatisEventArgs e)
        {

        }

        /// <summary>
        /// 开始。在统计方案执行的第二阶段被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnBegin(object sender, StatisEventArgs e)
        {

        }


        // 每一记录，在触发MARCFilter之前
        /// <summary>
        /// 处理一条记录之前。在统计方案执行中，第三阶段，针对每条记录被调用一次，在 OnRecord() 之前触发
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="e">事件参数</param>
        public virtual void PreFilter(object sender, StatisEventArgs e)
        {

        }

        // 每一记录处理
        /// <summary>
        /// 处理一条记录。在统计方案执行中，第三阶段，针对每条记录被调用一次
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnRecord(object sender, StatisEventArgs e)
        {

        }

        /// <summary>
        /// 结束。在统计方案执行的第四阶段被调用
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void OnEnd(object sender, StatisEventArgs e)
        {

        }

        // 
        /// <summary>
        /// 创建初始的 .cs 文件
        /// </summary>
        /// <param name="strFileName">文件名</param>
        public static void CreateStartCsFile(string strFileName)
        {
            using (StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.Windows.Forms;");
                sw.WriteLine("using System.IO;");
                sw.WriteLine("using System.Text;");
                sw.WriteLine("using System.Xml;");
                sw.WriteLine("");

                //sw.WriteLine("using DigitalPlatform.MarcDom;");
                //sw.WriteLine("using DigitalPlatform.Statis;");
                sw.WriteLine("using dp2Circulation;");
                sw.WriteLine("");

                sw.WriteLine("using DigitalPlatform.Marc;");
                sw.WriteLine("using DigitalPlatform.Xml;");
                sw.WriteLine("using DigitalPlatform.Script;");
                sw.WriteLine("");

                sw.WriteLine("public class MyMarcQueryHost : MarcQueryHost");

                sw.WriteLine("{");

                sw.WriteLine("\tpublic override void OnRecord(object sender, StatisEventArgs e)");
                sw.WriteLine("\t{");
                sw.WriteLine("");
                sw.WriteLine("\t\t// TODO: 在这里开始写代码吧");
                sw.WriteLine("");
                sw.WriteLine("\t}");

                sw.WriteLine("}");
            }
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
             Program.MainForm.OperHistory.AppendHtml(strHtml);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            Program.MainForm.OperHistory.AppendHtml("<div class='debug "+strClass+"'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }
    }
}
