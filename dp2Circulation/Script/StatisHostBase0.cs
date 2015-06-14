using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 各种类型的统计窗，统计方案的宿主类的基础类
    /// 没有定义过程 virtual 函数
    /// </summary>
    public class StatisHostBase0
    {
        /// <summary>
        /// 统计方案存储目录
        /// </summary>
        public string ProjectDir = "";  // 方案源文件所在目录

        /// <summary>
        /// 当前正在运行的统计方案实例的独占目录。一般用于存储统计过程中的临时文件
        /// </summary>
        public string InstanceDir = ""; // 当前实例独占的目录。用于存储临时文件

        /// <summary>
        /// 调试信息控制台
        /// </summary>
        public WebBrowser Console = null;

        /// <summary>
        /// 输出的 HTML 统计结果文件名集合
        /// </summary>
        public List<string> OutputFileNames = new List<string>(); // 存放输出的html文件

        int m_nFileNameSeed = 1;

        private bool disposed = false;

                // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        /// <summary>
        /// 析构函数
        /// </summary>
        ~StatisHostBase0()      
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

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
                if (_freed == false)
                {
                    try // 2009/10/10
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

        bool _freed = false;
        /// <summary>
        /// 释放资源。在本对象被摧毁前调用
        /// </summary>
        public virtual void FreeResources()
        {
            _freed = true;
        }

        /// <summary>
        /// 清除控制台已有的显示内容，为后面显示文本内容作好准备
        /// </summary>
        public void ClearConsoleForPureTextOutputing()
        {
            Global.ClearForPureTextOutputing(this.Console);
        }

        /// <summary>
        /// 将 HTML 信息输出到控制台，显示出来。
        /// </summary>
        /// <param name="strText">要输出的 HTML 字符串</param>
        public void WriteToConsole(string strText)
        {
            Global.WriteHtml(this.Console, strText);
        }

        /// <summary>
        /// 将文本信息输出到控制台，显示出来
        /// </summary>
        /// <param name="strText">要输出的文本字符串</param>
        public void WriteTextToConsole(string strText)
        {
            Global.WriteHtml(this.Console, HttpUtility.HtmlEncode(strText));
        }

        internal virtual string GetOutputFileNamePrefix()
        {
            throw new Exception("尚未实现");
        }

        /// <summary>
        /// 获得一个新的输出文件名
        /// </summary>
        /// <returns>输出文件名</returns>
        public string NewOutputFileName()
        {
            // string strFileNamePrefix = this.XmlStatisForm.MainForm.DataDir + "\\~xml_statis";
            string strFileNamePrefix = GetOutputFileNamePrefix();

            string strFileName = strFileNamePrefix + "_" + this.m_nFileNameSeed.ToString() + ".html";

            this.m_nFileNameSeed++;

            this.OutputFileNames.Add(strFileName);

            return strFileName;
        }

        /// <summary>
        /// 将字符串内容写入指定的文本文件。如果文件中已经存在内容，则被本次写入的覆盖
        /// </summary>
        /// <param name="strFileName">文本文件名</param>
        /// <param name="strText">要写入文件的字符串</param>
        /// <param name="encoding">编码方式</param>
        public void WriteToOutputFile(string strFileName,
            string strText,
            Encoding encoding)
        {
            using (StreamWriter sw = new StreamWriter(strFileName,
                false,	// append
                encoding))
            {
                sw.Write(strText);
            }
        }

        /// <summary>
        /// 从磁盘上删除一个输出文件，并从 OutputFileNames 集合中移走其文件名
        /// </summary>
        /// <param name="strFileName">文件名</param>
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

    }
}
