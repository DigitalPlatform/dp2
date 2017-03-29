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
                return MainForm.StatisAssemblyVersion;
            }
            set
            {
                MainForm.StatisAssemblyVersion = value;
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

                this.m_strInstanceDir = PathUtil.MergePath(this.MainForm.DataDir, "~bin_" + Guid.NewGuid().ToString());
                PathUtil.CreateDirIfNeed(this.m_strInstanceDir);

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

            if (this.MainForm != null)
            {
                ScriptManager.applicationInfo = this.MainForm.AppInfo;

                /*
                ScriptManager.CfgFilePath =
                    this.MainForm.DataDir + "\\biblio_statis_projects.xml";
                 * */
                ScriptManager.DataDir = this.MainForm.UserDir;

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
