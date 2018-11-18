using DigitalPlatform;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FingerprintCenter
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (StringUtil.IsDevelopMode() == false)
                ClientInfo.PrepareCatchException();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static MainForm _mainForm = null;
        // 这里用 _mainForm 存储窗口对象，不采取 Form.ActiveForm 获取的方式。原因如下
        // http://stackoverflow.com/questions/17117372/form-activeform-occasionally-works
        // Form.ActiveForm occasionally works

        public static MainForm MainForm
        {
            get
            {
                return _mainForm;
            }
        }

    }
}
