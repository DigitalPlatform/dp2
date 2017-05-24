using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;
using System.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 实体/订购/评注/期查询窗的脚本宿主类
    /// </summary>
    public class ItemHost : StatisHostBase0
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        /// <summary>
        /// 数据库类型。"item" 表示查询实体库；"order" 表示查询订购库；"issue" 表示查询期库；"comment" 表示查询评注库
        /// </summary>
        public string DbType = "item";  // comment order issue 

        /// <summary>
        /// 当前记录路径
        /// </summary>
        public string RecordPath = "";

        /// <summary>
        /// 当前记录的 XMlDocument 对象
        /// </summary>
        public XmlDocument ItemDom = null;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed = false;

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
        public ItemHost()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        internal override string GetOutputFileNamePrefix()
        {
            return Path.Combine(Program.MainForm.DataDir, "~item_statis");
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

        /// <summary>
        /// 创建初始的 .cs 文件
        /// </summary>
        /// <param name="strFileName">文件名</param>
        /// <param name="strDbType">是否包含类型检查的初始代码。如果为 null，表示不包含；其他值表示要等于这个代码值</param>
        /// <param name="strDbTypeCaption">数据库类型名称用于显示的字符串</param>
        public static void CreateStartCsFile(string strFileName,
            string strDbType = null,
            string strDbTypeCaption = null)
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

                sw.WriteLine("using DigitalPlatform.Xml;");
                sw.WriteLine("");

                sw.WriteLine("public class MyItemHost : ItemHost");

                sw.WriteLine("{");

                if (strDbType != null)
                {
                    sw.WriteLine("\tpublic override void OnInitial(object sender, StatisEventArgs e)");
                    sw.WriteLine("\t{");
                    sw.WriteLine("\t\tif (this.DbType != \"" + strDbType + "\")");
                    sw.WriteLine("\t\t{");
                    sw.WriteLine("\t\t\te.Continue = ContinueType.Error;");
                    string strText = "类型为 " + strDbType + " 的查询窗";
                    if (string.IsNullOrEmpty(strDbTypeCaption) == false)
                        strText = strDbTypeCaption + "查询窗";
                    sw.WriteLine("\t\t\te.ParamString = \"本程序只能被 " + strText + " 所调用\";");
                    sw.WriteLine("\t\t\treturn;");
                    sw.WriteLine("\t\t}");
                    sw.WriteLine("\t}");
                }

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
            Program.MainForm.OperHistory.AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }
    }
}
