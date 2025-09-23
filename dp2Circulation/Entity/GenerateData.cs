using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.Core;

namespace dp2Circulation
{
    /// <summary>
    /// 提供自动创建数据功能的类
    /// </summary>
    public class GenerateData : IDisposable
    {
        public Type DetailHostType = null;

        // 内核数据库下 cfgs 子目录中，脚本文件名
        // 纯文件名
        public string ScriptFileName = "dp2circulation_marc_autogen.cs";

        public event EventHandler SynchronizeMarcEvent = null;

        public MyForm _myForm = null;

        // public MainForm MainForm = null;

        public IBiblioItemsWindow DetailWindow = null;

        // 拥有
        GenerateDataForm m_genDataViewer = null;

        public GenerateData(MyForm myform,
            IBiblioItemsWindow detailWindow)
        {
            this._myForm = myform;
            // this.MainForm = myform.MainForm;
            this.DetailWindow = detailWindow;
        }

        public void Dispose()
        {
            if (m_detailHostObj != null)
                m_detailHostObj.Dispose();

            // 2017/4/23
            this.Close();
        }

        public void Close()
        {
            CloseGenDataViewer();
        }

        int _inCloseGenDataViewer = 0;  // 防止重入

        void CloseGenDataViewer()
        {
            if (_inCloseGenDataViewer > 0)
                return;

            _inCloseGenDataViewer++;
            try
            {
                if (this.m_genDataViewer != null)
                {
                    // 2015/11/7
                    // 注意解除 dock 时建立的关系。便于后面 Dispose()
                    if (Program.MainForm.CurrentGenerateDataControl == m_genDataViewer.Table)
                    {
                        Program.MainForm.CurrentGenerateDataControl = null;
                    }

                    this.m_genDataViewer.Close();
                    m_genDataViewer.DoDockEvent -= new DoDockEventHandler(m_genDataViewer_DoDockEvent);
                    m_genDataViewer.SetMenu -= new RefreshMenuEventHandler(m_genDataViewer_SetMenu);
                    m_genDataViewer.TriggerAction -= new TriggerActionEventHandler(m_genDataViewer_TriggerAction);
                    m_genDataViewer.MyFormClosed -= new EventHandler(m_genDataViewer_MyFormClosed);
                    m_genDataViewer.FormClosed -= new FormClosedEventHandler(m_genDataViewer_FormClosed);
                    this.m_genDataViewer = null;
                }
            }
            finally
            {
                _inCloseGenDataViewer--;
            }
        }

        public void ClearViewer()
        {
            if (this.m_genDataViewer != null)
            {
                //this._myForm.TryInvoke(() =>
                //{
                this.m_genDataViewer.Clear();
                //});
            }
        }

        public void RefreshViewerState()
        {
            if (this.m_genDataViewer != null)
            {
                //this._myForm.TryInvoke(() =>
                //{
                this.m_genDataViewer.RefreshState();
                //});
            }
        }

        #region 最新的创建数据脚本功能

        Assembly m_autogenDataAssembly = null;
        string m_strAutogenDataCfgFilename = "";    // 自动创建数据的.cs文件路径，全路径，包括库名部分
        object m_autogenSender = null;

        // 拥有
        IDetailHost m_detailHostObj = null;
        public IDetailHost DetailHostObj
        {
            get
            {
                return this.m_detailHostObj;
            }
        }

        // 是否为新的风格
        bool AutoGenNewStyle
        {
            get
            {
                if (this.m_detailHostObj == null)
                    return false;

                if (this.m_detailHostObj.GetType().GetMethod("CreateMenu") != null)
                    return true;
                return false;
            }
        }

        // 从本地获得两个配置文件的内容
        // return:
        //      -1  出错
        //      0   正常返回
        //      1   配置文件没有找到，但 host 已经初始化
        int GetCodeFromLocal(
            string strAutogenDataCfgFilename,
            out string strCode,
            out string strRef,
            out string strError)
        {
            strError = "";
            strCode = "";
            strRef = "";

            if (File.Exists(strAutogenDataCfgFilename) == false)
            {
                IDetailHost host = null;

                if (this.DetailHostType != null)
                    host = (IDetailHost)DetailHostType.InvokeMember(null,
                        BindingFlags.DeclaredOnly |
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                        null);
                else
                    host = new DetailHost();

                host.Assembly = null;
                host.Form = this._myForm;
                host.DetailWindow = this._myForm as IBiblioItemsWindow;
                this.m_detailHostObj = host;

                m_strAutogenDataCfgFilename = strAutogenDataCfgFilename;

                this.m_autogenDataAssembly = Assembly.GetAssembly(this.GetType());  // 充数
                return 1;
            }

            // 能自动识别文件内容的编码方式的读入文本文件内容模块
            // parameters:
            //      lMaxLength  装入的最大长度。如果超过，则超过的部分不装入。如果为-1，表示不限制装入长度
            // return:
            //      -1  出错 strError中有返回值
            //      0   文件不存在 strError中有返回值
            //      1   文件存在
            //      2   读入的内容不是全部
            int nRet = FileUtil.ReadTextFileContent(strAutogenDataCfgFilename,
                -1,
            out strCode,
            out Encoding encoding,
            out strError);
            if (nRet != 1)
                return -1;

            string strCfgFilePath = strAutogenDataCfgFilename + ".ref"; // strBiblioDbName + "/cfgs/" + this.ScriptFileName + ".ref";
            if (File.Exists(strCfgFilePath) == true)
            {
                nRet = FileUtil.ReadTextFileContent(strCfgFilePath,
        -1,
    out strRef,
    out encoding,
    out strError);
                if (nRet != 1)
                    return -1;
            }

            return 0;
        }

        // 从服务器获得两个配置文件的内容
        // return:
        //      -1  出错
        //      0   正常返回
        //      1   配置文件没有找到，但 host 已经初始化
        int GetCode(
            string strAutogenDataCfgFilename,
            out string strCode,
            out string strRef,
            out string strError)
        {
            strError = "";
            strCode = "";
            strRef = "";

            byte[] baCfgOutputTimestamp = null;
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            int nRet = this._myForm.GetCfgFileContent(strAutogenDataCfgFilename,
                out strCode,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                IDetailHost host = null;

                if (this.DetailHostType != null)
                    host = (IDetailHost)DetailHostType.InvokeMember(null,
                        BindingFlags.DeclaredOnly |
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                        null);
                else
                    host = new DetailHost();

                host.Assembly = null;
                host.Form = this._myForm;
                host.DetailWindow = this._myForm as IBiblioItemsWindow;
                this.m_detailHostObj = host;

                m_strAutogenDataCfgFilename = strAutogenDataCfgFilename;

                this.m_autogenDataAssembly = Assembly.GetAssembly(this.GetType());  // 充数

                return 1;
            }

            string strCfgFilePath = strAutogenDataCfgFilename + ".ref"; // strBiblioDbName + "/cfgs/" + this.ScriptFileName + ".ref";
            nRet = this._myForm.GetCfgFileContent(strCfgFilePath,
                out strRef,
                out baCfgOutputTimestamp,
                out strError);
            if (nRet == -1 /*|| nRet == 0*/)    // .ref 文件可以没有
                return -1;

            return 0;
        }

        // 初始化 dp2circulation_marc_autogen.cs 的 Assembly，并new DetailHost对象
        // return:
        //      -1  error
        //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
        //      1   重新(或者首次)初始化了Assembly
        public int InitialAutogenAssembly(
            string strBiblioRecPath,
            out string strError)
        {
            strError = "";
            int nRet = 0;

#if NO
            // 2014/7/14
            if (string.IsNullOrEmpty(strBiblioRecPath) == true)
                strBiblioRecPath = this.BiblioRecPath;
#endif
            string strAutogenDataCfgFilename = "";

            string strFormat = "";
            if (StringUtil.HasHead(strBiblioRecPath, "format:") == true)
            {
                strFormat = strBiblioRecPath.Substring("format:".Length);
                strAutogenDataCfgFilename = Path.Combine(Program.MainForm.DataDir, strFormat + "_cfgs/" + this.ScriptFileName);
            }
            else
            {
                // 库名部分路径
                string strBiblioDbName = Global.GetDbName(strBiblioRecPath);

                if (string.IsNullOrEmpty(strBiblioDbName) == true)
                    return 0;

                strAutogenDataCfgFilename = strBiblioDbName + "/cfgs/" + this.ScriptFileName;
            }

            bool bAssemblyReloaded = false;

            // 如果必要，重新准备Assembly
            if (m_autogenDataAssembly == null
                || m_strAutogenDataCfgFilename != strAutogenDataCfgFilename)
            {
                this.m_autogenDataAssembly = Program.MainForm.AssemblyCache.FindObject(strAutogenDataCfgFilename);
                this.m_detailHostObj = null;

                // 如果Cache中没有现成的Assembly
                if (this.m_autogenDataAssembly == null)
                {
                    string strCode = "";
                    string strRef = "";

#if NO
                    byte[] baCfgOutputTimestamp = null;
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = this._myForm.GetCfgFileContent(strAutogenDataCfgFilename,
                        out strCode,
                        out baCfgOutputTimestamp,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        IDetailHost host = null;

                        if (this.DetailHostType != null)
                            host = (IDetailHost)DetailHostType.InvokeMember(null,
                                BindingFlags.DeclaredOnly |
                                BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                                null);
                        else
                            host = new DetailHost();

                        host.Assembly = null;
                        host.Form = this._myForm;
                        host.DetailWindow = this._myForm as IBiblioItemsWindow;
                        this.m_detailHostObj = host;

                        m_strAutogenDataCfgFilename = strAutogenDataCfgFilename;

                        this.m_autogenDataAssembly = Assembly.GetAssembly(this.GetType());  // 充数

                        return 1;
                    }

                    string strCfgFilePath = strAutogenDataCfgFilename + ".ref"; // strBiblioDbName + "/cfgs/" + this.ScriptFileName + ".ref";
                    nRet = this._myForm.GetCfgFileContent(strCfgFilePath,
                        out strRef,
                        out baCfgOutputTimestamp,
                        out strError);
                    if (nRet == -1 /*|| nRet == 0*/)    // .ref 文件可以没有
                        goto ERROR1;
#endif
                    if (string.IsNullOrEmpty(strFormat) == false)
                    {
                        nRet = GetCodeFromLocal(
                            strAutogenDataCfgFilename,
                            out strCode,
                            out strRef,
                            out strError);
                    }
                    else
                    {
                        // 从服务器获得两个配置文件的内容
                        nRet = GetCode(
                            strAutogenDataCfgFilename,
                            out strCode,
                            out strRef,
                            out strError);
                    }
                    if (nRet == -1)
                    {
                        strError = $"获得配置文件 '{strAutogenDataCfgFilename}' 时出错: {strError}";
                        goto ERROR1;
                    }
                    if (nRet == 1)
                        return 1;

                    try
                    {
                        // 准备Assembly
                        nRet = GetCsScriptAssembly(
                            strCode,
                            strRef,
                            out Assembly assembly,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "编译脚本文件 '" + strAutogenDataCfgFilename + "' 时出错：" + strError;
                            goto ERROR1;
                        }
                        // 记忆到缓存
                        Program.MainForm.AssemblyCache.SetObject(strAutogenDataCfgFilename, assembly);

                        this.m_autogenDataAssembly = assembly;

                        bAssemblyReloaded = true;
                    }
                    catch (Exception ex)
                    {
                        strError = "准备脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                        goto ERROR1;
                    }
                }

                bAssemblyReloaded = true;

                m_strAutogenDataCfgFilename = strAutogenDataCfgFilename;

                // 至此，Assembly已经纯备好了
                Debug.Assert(this.m_autogenDataAssembly != null, "");
            }

            Debug.Assert(this.m_autogenDataAssembly != null, "");

            // 准备 host 对象
            if (this.m_detailHostObj == null
                || bAssemblyReloaded == true)
            {
                try
                {
                    IDetailHost host = null;
                    nRet = NewHostObject(
                        out host,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "准备脚本文件 '" + m_strAutogenDataCfgFilename + "' 时出错：" + strError;
                        goto ERROR1;
                    }

                    this.m_detailHostObj = host;
                }
                catch (Exception ex)
                {
                    strError = "准备脚本代码过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }
            }

            Debug.Assert(this.m_detailHostObj != null, "");

            if (bAssemblyReloaded == true)
                return 1;
            return 0;
        ERROR1:
            return -1;
        }

        int m_nInFillMenu = 0;

        // 自动加工数据
        // parameters:
        //      sender    从何处启动? MarcEditor EntityEditForm BindingForm
        //      e.ScriptEntry   如果为 "!createMenu" 表示重新初始化菜单事项
        //                      如果为 "!getActiveMenu" 表示查询获得当前 active 状态的菜单事项
        public void AutoGenerate(object sender,
            GenerateDataEventArgs e,
            string strBiblioRecPath,
            bool bOnlyFillMenu = false)
        {
            int nRet = 0;
            string strError = "";
            bool bAssemblyReloaded = false;

            // 防止重入
            if (bOnlyFillMenu == true && this.m_nInFillMenu > 0)
                return;

            this.m_nInFillMenu++;
            try
            {
                // 初始化 dp2circulation_marc_autogen.cs 的 Assembly，并new DetailHost对象
                // return:
                //      -1  error
                //      0   没有重新初始化Assembly，而是直接用以前Cache的Assembly
                //      1   重新(或者首次)初始化了Assembly
                nRet = InitialAutogenAssembly(strBiblioRecPath, // null,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    if (this.m_detailHostObj == null)
                    {
                        if (string.IsNullOrEmpty(strBiblioRecPath))
                            e.ErrorInfo = $"数据库名为空，无法初始化 Assembly";
                        return; // 库名不具备，无法初始化
                    }
                }
                if (nRet == 1)
                    bAssemblyReloaded = true;

                Debug.Assert(this.m_detailHostObj != null, "");

                if (this.AutoGenNewStyle == true)
                {
                    bool bDisplayWindow = Program.MainForm.PanelFixedVisible == false ? true : false;
                    if (bDisplayWindow == true)
                    {
                        if (String.IsNullOrEmpty(e.ScriptEntry) != true
                            && e.ScriptEntry != "Main")
                            bDisplayWindow = false;
                    }

                    if (sender is EntityEditForm
                        && (String.IsNullOrEmpty(e.ScriptEntry) == true
                            || e.ScriptEntry == "Main"))
                    {
                        bDisplayWindow = true;
                    }
                    else if (sender is BindingForm
    && (String.IsNullOrEmpty(e.ScriptEntry) == true
        || e.ScriptEntry == "Main"))
                    {
                        bDisplayWindow = true;
                    }

                    DisplayAutoGenMenuWindow(this._myForm, bDisplayWindow);   // 可能会改变 .ActionTable以及 .Count
                    if (bOnlyFillMenu == false)
                    {
                        if (Program.MainForm.PanelFixedVisible == true
                            && e.Parameter == null) // 2015/6/5
                            Program.MainForm.ActivateGenerateDataPage();
                    }

                    if (this.m_genDataViewer != null)
                    {
                        this.m_genDataViewer.sender = sender;
                        this.m_genDataViewer.e = e;
                    }

                    // 清除残留菜单事项
                    if (m_autogenSender != sender
                        || bAssemblyReloaded == true)
                    {
                        if (this.m_genDataViewer != null
                            && this.m_genDataViewer.Count > 0)
                            this.m_genDataViewer.Clear();
                    }
                }
                else // 旧的风格
                {
#if NO
                    if (this.m_genDataViewer != null)
                    {
                        this.m_genDataViewer.Close();
                        this.m_genDataViewer = null;
                    }
#endif
                    this._myForm.TryInvoke(() =>
                    {
                        CloseGenDataViewer();
                    });

                    if (this._myForm.Focused == true
                        // || this.m_marcEditor.Focused TODO: 这里要研究一下如何实现
                        )
                        Program.MainForm.CurrentGenerateDataControl = null;

                    // 如果意图仅仅为填充菜单
                    if (bOnlyFillMenu == true)
                        return;
                }

                try
                {
                    // 旧的风格
                    if (this.AutoGenNewStyle == false)
                    {
                        this.m_detailHostObj.Invoke(String.IsNullOrEmpty(e.ScriptEntry) == true ? "Main" : e.ScriptEntry,
    sender,
    e);
                        // this.SetSaveAllButtonState(true); TODO: 应该没有必要。MARC 编辑器内容修改自然会引起保存按钮状态变化
                        return;
                    }

                    // 初始化菜单
                    try
                    {
                        if (this.m_genDataViewer != null)
                        {
                            this._myForm.TryInvoke(() =>
                            {
                                // 迫使重新创建菜单
                                if (e.ScriptEntry == "!createMenu")
                                    this.m_genDataViewer.Clear();

                                // 出现菜单界面
                                if (this.m_genDataViewer.Count == 0)
                                {
                                    dynamic o = this.m_detailHostObj;
                                    o.CreateMenu(sender, e);

                                    this.m_genDataViewer.Actions = this.m_detailHostObj.ScriptActions;
                                }

                                // 根据当前插入符位置刷新加亮事项
                                this.m_genDataViewer.RefreshState();
                            });

                            if (e.ScriptEntry == "!createMenu")
                                return;
                        }

                        // 执行 ScriptEntry 入口函数名
                        if (String.IsNullOrEmpty(e.ScriptEntry) == false
                            && e.ScriptEntry.StartsWith("!") == false)
                        {
                            this.m_detailHostObj.Invoke(e.ScriptEntry,
                                sender,
                                e);
                            return; // 2024/7/10
                        }
                        else
                        {
                            if (Program.MainForm.PanelFixedVisible == true
                                && bOnlyFillMenu == false
                                && Program.MainForm.CurrentGenerateDataControl != null
                                && e.ScriptEntry != "!getActiveMenu")
                            {
                                TableLayoutPanel table = (TableLayoutPanel)Program.MainForm.CurrentGenerateDataControl;
                                for (int i = 0; i < table.Controls.Count; i++)
                                {
                                    Control control = table.Controls[i];
                                    if (control is DpTable)
                                    {
                                        control.Focus();
                                        break;
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        /*
                        // 被迫改用旧的风格
                        this.m_detailHostObj.Invoke(String.IsNullOrEmpty(e.ScriptEntry) == true ? "Main" : e.ScriptEntry,
        sender,
        e);
                        this.SetSaveAllButtonState(true);
                        return;
                         * */
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    strError = "执行脚本文件 '" + m_strAutogenDataCfgFilename + "' 过程中发生异常: \r\n" + ExceptionUtil.GetDebugText(ex);
                    goto ERROR1;
                }

                this.m_autogenSender = sender;  // 记忆最近一次的调用发起者

                // 2024/7/9
                // 获得当前可用的菜单
                if (e.ScriptEntry == "!getActiveMenu"
                    /*&& this.m_genDataViewer != null*/)
                {
                    if (this.m_genDataViewer == null)
                    {
                        e.ErrorInfo = "m_genDataViewer == null";
                        return;
                    }
                    var actions = this.m_genDataViewer.GetSelectedActions();
                    e.Parameter = GenerateDataForm.ActionsToXml(actions);
                    return;
                }

                if (bOnlyFillMenu == false
                    && this.m_genDataViewer != null)
                    this.m_genDataViewer.TryAutoRun();
                return;
            ERROR1:
                // TODO: 报错是否要直接显示在 dpTable 中?
                // MessageBox.Show(this._myForm, strError);
                DisplayAutoGenMenuWindow(this._myForm, false);
                if (this.m_genDataViewer != null)
                    this.m_genDataViewer.DisplayError(strError);
            }
            finally
            {
                this.m_nInFillMenu--;
            }
        }

        void DisplayAutoGenMenuWindow(Control control,
            bool bOpenWindow)
        {
            control.TryInvoke(() =>
            {

                // 优化，避免无谓地进行服务器调用
                if (bOpenWindow == false)
                {
                    if (Program.MainForm.PanelFixedVisible == false
                        && (m_genDataViewer == null || m_genDataViewer.Visible == false))
                        return;
                }


                if (this.m_genDataViewer == null
                    || (bOpenWindow == true && this.m_genDataViewer != null && this.m_genDataViewer.Visible == false))
                {
                    m_genDataViewer = new GenerateDataForm();

                    m_genDataViewer.AutoRun = Program.MainForm.AppInfo.GetBoolean("detailform", "gen_auto_run", false);
                    // MainForm.SetControlFont(m_genDataViewer, this.Font, false);

                    {   // 恢复列宽度

                        string strWidths = Program.MainForm.AppInfo.GetString(
                                       "gen_data_dlg",
                                        "column_width",
                                       "");
                        if (String.IsNullOrEmpty(strWidths) == false)
                        {
                            DpTable.SetColumnHeaderWidth(m_genDataViewer.ActionTable,
                                strWidths,
                                false);
                        }
                    }

                    // m_genDataViewer.MainForm = Program.MainForm;  // 必须是第一句
                    m_genDataViewer.Text = "创建数据";

                    m_genDataViewer.DoDockEvent -= new DoDockEventHandler(m_genDataViewer_DoDockEvent);
                    m_genDataViewer.DoDockEvent += new DoDockEventHandler(m_genDataViewer_DoDockEvent);

                    m_genDataViewer.SetMenu -= new RefreshMenuEventHandler(m_genDataViewer_SetMenu);
                    m_genDataViewer.SetMenu += new RefreshMenuEventHandler(m_genDataViewer_SetMenu);

                    m_genDataViewer.TriggerAction -= new TriggerActionEventHandler(m_genDataViewer_TriggerAction);
                    m_genDataViewer.TriggerAction += new TriggerActionEventHandler(m_genDataViewer_TriggerAction);

                    m_genDataViewer.MyFormClosed -= new EventHandler(m_genDataViewer_MyFormClosed);
                    m_genDataViewer.MyFormClosed += new EventHandler(m_genDataViewer_MyFormClosed);

                    m_genDataViewer.FormClosed -= new FormClosedEventHandler(m_genDataViewer_FormClosed);
                    m_genDataViewer.FormClosed += new FormClosedEventHandler(m_genDataViewer_FormClosed);
                }

                if (bOpenWindow == true)
                {
                    if (m_genDataViewer.Visible == false)
                    {
                        Program.MainForm.AppInfo.LinkFormState(m_genDataViewer, "autogen_viewer_state");
                        m_genDataViewer.Show(this._myForm);
                        m_genDataViewer.Activate();

                        Program.MainForm.CurrentGenerateDataControl = null;
                    }
                    else
                    {
                        if (m_genDataViewer.WindowState == FormWindowState.Minimized)
                            m_genDataViewer.WindowState = FormWindowState.Normal;
                        m_genDataViewer.Activate();
                    }
                }
                else
                {
                    if (m_genDataViewer.Visible == true)
                    {

                    }
                    else
                    {
                        if (Program.MainForm.CurrentGenerateDataControl != m_genDataViewer.Table)
                            m_genDataViewer.DoDock(false); // 不会自动显示FixedPanel
                    }
                }

                if (this.m_genDataViewer != null)
                    this.m_genDataViewer.CloseWhenComplete = bOpenWindow;
            });
        }

        void m_genDataViewer_DoDockEvent(object sender, DoDockEventArgs e)
        {
            m_genDataViewer.TryInvoke(() =>
            {
                if (Program.MainForm.CurrentGenerateDataControl != m_genDataViewer.Table)
                {
                    Program.MainForm.CurrentGenerateDataControl = m_genDataViewer.Table;
                    // 防止内存泄漏
                    m_genDataViewer.AddFreeControl(m_genDataViewer.Table);
                }

                if (e.ShowFixedPanel == true
                    && Program.MainForm.PanelFixedVisible == false)
                    Program.MainForm.PanelFixedVisible = true;

                /*
                Program.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", m_genDataViewer.AutoRun);

                {	// 保存列宽度
                    string strWidths = DpTable.GetColumnWidthListString(m_genDataViewer.ActionTable);
                    Program.MainForm.AppInfo.SetString(
                        "gen_data_dlg",
                        "column_width",
                        strWidths);
                }
                 * */

                m_genDataViewer.Docked = true;
                m_genDataViewer.Visible = false;
            });
        }

        void m_genDataViewer_SetMenu(object sender, RefreshMenuEventArgs e)
        {
            if (e.Actions == null || this.m_detailHostObj == null)
                return;

            Type classType = m_detailHostObj.GetType();

#if REMOVED
            // 2024/7/10
            {
                string strFuncName = "beginSetMenu";

                DigitalPlatform.Script.SetMenuEventArgs e1 = new DigitalPlatform.Script.SetMenuEventArgs();
                e1.Action = new ScriptAction();
                e1.sender = e.sender;
                e1.e = e.e;

                ScriptUtil.InvokeMember(classType,
                    strFuncName,
                    this.m_detailHostObj,
                    new object[] { sender, e1 });

                // 此时 m_detailHostObj.ScriptActions 可能被改变
                if (e1.Result == "actions_changed")
                    this.m_genDataViewer.Actions = this.m_detailHostObj.ScriptActions;
            }
#endif

            foreach (ScriptAction action in e.Actions)
            {
                string strFuncName = action.ScriptEntry + "_setMenu";
                if (string.IsNullOrEmpty(strFuncName) == true)
                    continue;

                DigitalPlatform.Script.SetMenuEventArgs e1 = new DigitalPlatform.Script.SetMenuEventArgs();
                e1.Action = action;
                e1.sender = e.sender;
                e1.e = e.e;

                ScriptUtil.InvokeMember(classType,
                    strFuncName,
                    this.m_detailHostObj,
                    new object[] { sender, e1 });
#if NO
                classType = m_detailHostObj.GetType();
                while (classType != null)
                {
                    try
                    {
                        // 有两个参数的成员函数
                        // 用 GetMember 先探索看看函数是否存在
                        MemberInfo [] infos = classType.GetMember(strFuncName,
                            BindingFlags.DeclaredOnly |
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod);
                        if (infos == null || infos.Length == 0)
                        {
                            classType = classType.BaseType;
                            if (classType == null)
                                break;
                            continue;
                        }

                        classType.InvokeMember(strFuncName,
                            BindingFlags.DeclaredOnly |
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod
                            ,
                            null,
                            this.m_detailHostObj,
                            new object[] { sender, e1 });
                        break;
                    }
                    catch (System.MissingMethodException/*ex*/)
                    {
                        classType = classType.BaseType;
                        if (classType == null)
                            break;
                    }
                }

#endif
            }

#if REMOVED
            // 2024/7/10
            {
                string strFuncName = "endSetMenu";

                DigitalPlatform.Script.SetMenuEventArgs e1 = new DigitalPlatform.Script.SetMenuEventArgs();
                e1.Action = new ScriptAction();
                e1.sender = e.sender;
                e1.e = e.e;

                ScriptUtil.InvokeMember(classType,
                    strFuncName,
                    this.m_detailHostObj,
                    new object[] { sender, e1 });
            }
#endif
        }

        void m_genDataViewer_TriggerAction(object sender, TriggerActionArgs e)
        {
            if (this.m_detailHostObj != null)
            {
                if (this._myForm != null && this._myForm.IsDisposed == true)
                {
                    if (this.m_genDataViewer != null)
                    {
                        this.m_genDataViewer.Clear();
#if NO
                        this.m_genDataViewer.Close();
                        this.m_genDataViewer = null;
#endif
                        CloseGenDataViewer();
                        return;
                    }
                }
                if (String.IsNullOrEmpty(e.EntryName) == false)
                {
                    this.SynchronizeMarc();

                    this.m_detailHostObj.Invoke(e.EntryName,
                        e.sender,
                        e.e);

                    // if (this.tabControl_biblioInfo.SelectedTab == this.tabPage_template) TODO: 这里是不是没有必要优化了
                    this.SynchronizeMarc();
                }

                if (this.m_genDataViewer != null)
                    this.m_genDataViewer.RefreshState();
            }
        }

        void SynchronizeMarc()
        {
            if (this.SynchronizeMarcEvent != null)
                this.SynchronizeMarcEvent(this, new EventArgs());
        }

        void m_genDataViewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (m_genDataViewer != null)
            {
                Program.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", m_genDataViewer.AutoRun);

                {	// 保存列宽度
                    string strWidths = DpTable.GetColumnWidthListString(m_genDataViewer.ActionTable);
                    Program.MainForm.AppInfo.SetString(
                        "gen_data_dlg",
                        "column_width",
                        strWidths);
                }

                Program.MainForm.AppInfo.UnlinkFormState(m_genDataViewer);
#if NO
                this.m_genDataViewer = null;
#endif
                CloseGenDataViewer();
            }
        }

        void m_genDataViewer_MyFormClosed(object sender, EventArgs e)
        {
            if (m_genDataViewer != null)
            {
                Program.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", m_genDataViewer.AutoRun);

                {	// 保存列宽度
                    string strWidths = DpTable.GetColumnWidthListString(m_genDataViewer.ActionTable);
                    Program.MainForm.AppInfo.SetString(
                        "gen_data_dlg",
                        "column_width",
                        strWidths);
                }

                Program.MainForm.AppInfo.UnlinkFormState(m_genDataViewer);
                // this.m_genDataViewer = null;
                CloseGenDataViewer();
            }
        }

        int NewHostObject(
            out IDetailHost hostObj,
            out string strError)
        {
            strError = "";
            hostObj = null;

            Type entryClassType = ScriptManager.GetDerivedClassType(
    this.m_autogenDataAssembly,
    "dp2Circulation.IDetailHost");
            if (entryClassType == null)
            {
                // 寻找 Host 派生类Type
                entryClassType = ScriptManager.GetDerivedClassType(
                    this.m_autogenDataAssembly,
                    "dp2Circulation.Host");
                if (entryClassType != null)
                {
                    strError = "您的脚本代码是从 dp2Circulation.Host 类继承的，这种方式目前已不再支持，需要修改(升级)为从 dp2Circulation.DetailHost 继承";
                    return -1;
                }

                strError = "dp2Circulation.IDetailHost 的派生类都没有找到";
                return -1;
            }

            // new一个DetailHost派生对象
            hostObj = (IDetailHost)entryClassType.InvokeMember(null,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
                null);

            if (hostObj == null)
            {
                strError = "new DetailHost 的派生类对象时失败";
                return -1;
            }

            // 为DetailHost派生类设置参数
            //hostObj.DetailForm = this._myForm as EntityForm;
            hostObj.Form = this._myForm;
            hostObj.Assembly = this.m_autogenDataAssembly;
            hostObj.DetailWindow = this.DetailWindow;

            return 0;
        }

        int GetCsScriptAssembly(
            string strCode,
            string strRef,
            out Assembly assembly,
            out string strError)
        {
            strError = "";
            assembly = null;

            // 2018/8/18
            // 为了兼容以前代码，对 using 部分进行修改
            strCode = ScriptManager.ModifyCode(strCode);


            string[] saRef = null;
            int nRet;

            nRet = ScriptManager.GetRefsFromXml(strRef,
                out saRef,
                out strError);
            if (nRet == -1)
                return -1;

            ScriptManager.RemoveRefsBinDirMacro(ref saRef);

            string[] saAddRef = {
                                    // 2011/3/4 增加
                                    "system.dll",
                                    "system.xml.dll",
                                    "system.windows.forms.dll",
                                    "system.drawing.dll",
                                    "System.Runtime.Serialization.dll",

                                    "netstandard.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.xmleditor.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marceditor.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcfixedfieldcontrol.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.gcatclient.dll",
									Environment.CurrentDirectory + "\\digitalplatform.script.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.commoncontrol.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.circulationclient.dll",
                                    Environment.CurrentDirectory + "\\digitalplatform.libraryclient.dll",

                                    Environment.CurrentDirectory + "\\digitalplatform.amazoninterface.dll",
									//Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2circulation.exe"
                                };

            if (saAddRef != null)
            {
                string[] saTemp = new string[saRef.Length + saAddRef.Length];
                Array.Copy(saRef, 0, saTemp, 0, saRef.Length);
                Array.Copy(saAddRef, 0, saTemp, saRef.Length, saAddRef.Length);
                saRef = saTemp;
            }

            string strErrorInfo = "";
            string strWarningInfo = "";
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saRef,
                null,   // strLibPaths,
                out assembly,
                out strErrorInfo,
                out strWarningInfo);
            if (nRet == -1)
            {
                strError = "脚本编译发现错误或警告:\r\n" + strErrorInfo;
                return -1;
            }
            /*
            if (m_scriptDomain != null)
                m_scriptDomain.Load(assembly.GetName());
             * */

            return 0;
        }


        #endregion

    }
}
