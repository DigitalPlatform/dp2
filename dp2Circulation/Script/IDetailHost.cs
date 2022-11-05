using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Script;

namespace dp2Circulation
{
    /// <summary>
    /// DetailHost 接口。用于种册窗的 Host 类
    /// </summary>
    public interface IDetailHost : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        Assembly Assembly {get;set;}
        /// <summary>
        /// 关联的 Form
        /// </summary>
        Form Form { get; set; }

        IBiblioItemsWindow DetailWindow
        {
            get;
            set;
        }

        ScriptActionCollection ScriptActions { get; set; }

        void Invoke(string strFuncName,
    object sender,
            // GenerateDataEventArgs e
            EventArgs e);

        void CreateMenu(object sender, GenerateDataEventArgs e);

#if NO
        void BeforeSaveRecord(object sender,
    BeforeSaveRecordEventArgs e);

        void AfterCreateItems(object sender,
            AfterCreateItemsArgs e);
#endif
    }

    // 抽象的种册窗 App-DOM 结构接口
    public interface IBiblioItemsWindow
    {
#if NO
        MainForm MainForm
        {
            get;
            set;
        }
#endif

        string GetMarc();

        void SetMarc(string strMARC);

        string BiblioRecPath
        {
            get;
        }

        string MarcSyntax
        {
            get;
        }

#if SUPPORT_OLD_STOP
        Stop Progress
        {
            get;
        }
#endif

        /// <summary>
        /// 宿主窗口
        /// </summary>
        Form Form
        {
            get;
        }
    }
}
