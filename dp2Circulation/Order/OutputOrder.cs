using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Web;

using DigitalPlatform.Xml;
using DigitalPlatform.dp2.Statis;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;

namespace dp2Circulation
{
    /// <summary>
    /// 输出订单的宿主类
    /// 用于 PrintOrderForm (打印订单窗)
    /// </summary>
    public class OutputOrder : StatisHostBase0
    {
        /// <summary>
        /// 本对象所关联的 PrintOrderForm (打印订单窗)
        /// </summary>
        public PrintOrderForm PrintOrderForm = null;	// [in]打印订单窗

#if NO
        private bool disposed = false;
        /// <summary>
        /// 统计方案存储目录
        /// </summary>
        public string ProjectDir = "";  // [in]当前订单输出方案的所在目录

#endif

        /// <summary>
        /// 公共数据目录
        /// </summary>
        public string DataDir = ""; // [in]内务前端的数据目录


        /// <summary>
        /// 渠道名
        /// </summary>
        public string Seller = "";  // [in]渠道名

        /// <summary>
        /// 订单 XML 文件名
        /// </summary>
        public string XmlFilename = ""; // [in]内置的XML格式订单文件，已经创建好了。文件名全路径

        /// <summary>
        /// 订单输出目录
        /// </summary>
        public string OutputDir = "";   // [in]订单输出目录

        /// <summary>
        /// 当前宿主中已经被选定的出版物类型。值为“图书”“连续出版物”之一
        /// </summary>
        public string PubType = ""; // [in]当前宿主中已经被选定的出版物类型。值为“图书”“连续出版物”之一

        /// <summary>
        /// 构造函数
        /// </summary>
        public OutputOrder()
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
        ~OutputOrder()
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

                /*
                // 删除所有输出文件
                if (this.OutputFileNames != null)
                {
                    Global.DeleteFiles(this.OutputFileNames);
                    this.OutputFileNames = null;
                }*/

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

        // 释放资源
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void FreeResources()
        {
        }

#endif

        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(this.PrintOrderForm.MainForm.DataDir, "~outputorder_statis");
        }

	    // 初始化
        // return:
        //      false   初始化失败。错误信息在strError中
        //      true    初始化成功
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="strError">错误信息</param>
        /// <returns>是否初始化成功</returns>
        public virtual bool Initial(out string strError)
        {
            strError = "";
            return true;
        }

        // 入口函数
        /// <summary>
        /// 入口函数。重载此方法以实现脚本功能
        /// </summary>
        public virtual void Output()
        {

        }
    }
}

