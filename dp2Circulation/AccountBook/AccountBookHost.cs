using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Web;

namespace dp2Circulation
{
    /// <summary>
    /// 打印财产账窗的脚本宿主类
    /// </summary>
    public class AccountBookHost : StatisHostBase
    {
        /// <summary>
        /// 本对象所关联的 AccountBookForm (打印财产账窗)
        /// </summary>
        public AccountBookForm AccountBookForm = null;	// 引用

        /// <summary>
        /// 浏览控件
        /// </summary>
        public ListViewItem ListViewItem = null; 

        /// <summary>
        /// 构造函数
        /// </summary>
        public AccountBookHost()
        {
        }

        // 创建初始的的 .cs 文件
        /// <summary>
        /// 创建初始的的 .cs 文件
        /// </summary>
        /// <param name="strFileName">要创建的 C# 脚本文件名</param>
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
                sw.WriteLine("");

                sw.WriteLine("public class MyAccountBookHost : AccountBookHost");

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
        /// 获得框架窗口
        /// </summary>
        public MainForm MainForm
        {
            get
            {
                return this.AccountBookForm.MainForm;
            }
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            this.MainForm.OperHistory.AppendHtml(strHtml);
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
            this.MainForm.OperHistory.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }
    }
}
