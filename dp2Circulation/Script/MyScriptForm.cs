using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.IO;
using DigitalPlatform.Script;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 具有脚本功能的窗口的基础类
    /// dp2Library API 可以用 this.Channel 和 this.Stop 来直接访问
    /// </summary>
    public class MyScriptForm : MyForm
    {
        /// <summary>
        /// Assembly 版本号
        /// </summary>
        public int AssemblyVersion
        {
            get
            {
                if (Program.MainForm == null)
                    return 0;
                return Program.MainForm.StatisAssemblyVersion;
            }
            set
            {
                if (Program.MainForm != null)
                    Program.MainForm.StatisAssemblyVersion = value;
            }
        }

        string m_strInstanceDir = "";
        /// <summary>
        /// 实例目录
        /// </summary>
        public string InstanceDir
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strInstanceDir) == false)
                    return this.m_strInstanceDir;

                // 2018/9/25
                if (string.IsNullOrEmpty(Program.MainForm?.DataDir))
                    return null;

                this.m_strInstanceDir = PathUtil.MergePath(Program.MainForm.DataDir, "~bin_" + Guid.NewGuid().ToString());
                PathUtil.TryCreateDir(this.m_strInstanceDir);

                return this.m_strInstanceDir;
            }
        }

#if NO
        public override void OnMyFormClosed()
        {
            base.OnMyFormClosed();

            // 删除实例目录
            if (string.IsNullOrEmpty(this.m_strInstanceDir) == false)
            {
                try
                {
                    Directory.Delete(this.m_strInstanceDir, true);
                }
                catch
                {
                }
            }
        }
#endif
        /// <summary>
        /// 脚本管理器
        /// </summary>
        public ScriptManager ScriptManager = new ScriptManager();

        /// <summary>
        /// Form 装载事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Program.MainForm != null)
            {
                ScriptManager.applicationInfo = Program.MainForm.AppInfo;

                /*
                ScriptManager.CfgFilePath =
                    Program.MainForm.DataDir + "\\biblio_statis_projects.xml";
                 * */
                ScriptManager.DataDir = Program.MainForm.UserDir;

                ScriptManager.CreateDefaultContent -= new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);
                ScriptManager.CreateDefaultContent += new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);

                if (string.IsNullOrEmpty(ScriptManager.CfgFilePath) == false)
                {
                    try
                    {
                        ScriptManager.Load();
                    }
                    catch (FileNotFoundException)
                    {
                        // 不必报错 2009/2/4
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                    }
                }
                else
                {
                    Debug.Assert(false, "派生类尚未初始化 ScriptManager.CfgFilePath");
                }
            }
        }

        private void scriptManager_CreateDefaultContent(object sender, CreateDefaultContentEventArgs e)
        {
            CreateDefaultContent(e);
        }

        internal virtual void CreateDefaultContent(CreateDefaultContentEventArgs e)
        {

        }

        public virtual void TestCompile(string strProjectName)
        {
            string strError = "";

            if (String.IsNullOrEmpty(strProjectName) == true)
            {
                strError = "尚未指定方案名";
                goto ERROR1;
            }

            string strProjectLocate = "";
            // 获得方案参数
            // strProjectNamePath	方案名，或者路径
            // return:
            //		-1	error
            //		0	not found project
            //		1	found
            int nRet = this.ScriptManager.GetProjectData(
                strProjectName,
                out strProjectLocate);

            if (nRet == 0)
            {
                strError = "方案 " + strProjectName + " 没有找到...";
                goto ERROR1;
            }
            if (nRet == -1)
            {
                strError = "scriptManager.GetProjectData() error ...";
                goto ERROR1;
            }

            string strWarning = "";

            // TODO: 增加一个参数表示这是测试编译
            nRet = RunScript(strProjectName,
                strProjectLocate,
                "test_compile", // strInitialParamString
                out strError,
                out strWarning);
            if (nRet == -1)
                goto ERROR1;

            return;
            ERROR1:
            throw new Exception(strError);
        }

        // 兼容以前用法
        public int RunScript(string strProjectName,
            string strProjectLocate,
            out string strError)
        {
            string strWarning = "";
            return RunScript(strProjectName,
            strProjectLocate,
            "",
            out strError,
            out strWarning);
        }

        public virtual int RunScript(string strProjectName,
            string strProjectLocate,
            string strInitialParamString,
            out string strError,
            out string strWarning)
        {
            strError = "尚未重载 RunScript() 函数";
            strWarning = "";

            return -1;
        }

        /// <summary>
        /// Form 关闭事件
        /// </summary>
        /// <param name="e">事件参数</param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.Close();
                }
                catch
                {
                }
            }

            // 删除实例目录
            if (string.IsNullOrEmpty(this.m_strInstanceDir) == false)
            {
                try
                {
                    Directory.Delete(this.m_strInstanceDir, true);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 停止对象
        /// </summary>
        public Stop Stop
        {
            get
            {
                return this.stop;
            }
        }

        /// <summary>
        /// 是否正在执行运算
        /// </summary>
        public bool Running = false;   // 正在执行运算

        /// <summary>
        /// 错误信息窗
        /// </summary>
        public HtmlViewerForm ErrorInfoForm = null;

        // 获得错误信息窗
        internal HtmlViewerForm GetErrorInfoForm()
        {
            if (this.ErrorInfoForm == null
                || this.ErrorInfoForm.IsDisposed == true
                || this.ErrorInfoForm.IsHandleCreated == false)
            {
                this.ErrorInfoForm = new HtmlViewerForm();
                this.ErrorInfoForm.ShowInTaskbar = false;
                this.ErrorInfoForm.Text = "错误信息";
                this.ErrorInfoForm.Show(this);
                this.ErrorInfoForm.WriteHtml("<pre>");  // 准备文本输出
            }

            return this.ErrorInfoForm;
        }


        // 清除错误信息窗口中残余的内容
        internal void ClearErrorInfoForm()
        {
            if (this.ErrorInfoForm != null)
            {
                try
                {
                    this.ErrorInfoForm.HtmlString = "<pre>";
                }
                catch
                {
                }
            }
        }

        // 附加的 DLL 搜索路径
        internal List<string> _dllPaths = new List<string>();

        internal Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return ScriptManager.ResolveAssembly(args.Name, _dllPaths);
        }
    }
}
