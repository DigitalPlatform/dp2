using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 框架窗口的脚本宿主类
    /// </summary>
    public class MainFormHost : StatisHostBase0
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

#if NO
        public string ProjectDir = "";  // 方案源文件所在目录
        public string InstanceDir = ""; // 当前实例独占的目录。用于存储临时文件
        public List<string> OutputFileNames = new List<string>(); // 存放输出的html文件
        int m_nFileNameSeed = 1;

        private bool disposed = false;
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainFormHost()
        {
            //
            // TODO: Add constructor logic here
            //
        }

#if NO
        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method 
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
                ~MainFormHost()
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
#endif
        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.MainForm.DataDir, "~mainform_statis");
        }

        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void Main(object sender, EventArgs e)
        {

        }

#if NO
        // 获得一个新的输出文件名
        public string NewOutputFileName()
        {
            string strFileNamePrefix = this.MainForm.DataDir + "\\~item_statis";

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
}
