using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Web;
using System.Reflection;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Script;
using DigitalPlatform.Marc;

using DigitalPlatform.IO;
using DigitalPlatform.Text;

using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 各种批打印的基础类。
    /// 提供了下列设施：
    /// MARC 过滤器、扩展列
    /// 调试信息窗
    /// 从记录路径文件装载
    /// 从册条码号文件装载
    /// 根据批次号装入册记录
    /// 书目摘要装载
    /// </summary>
    public class BatchPrintFormBase : MyForm    // Form, IMdiWindow
    {
        /// <summary>
        /// 扩展列信息
        /// </summary>
        public Hashtable ColumnTable = new Hashtable();

        internal Assembly AssemblyFilter = null;
        internal ColumnFilterDocument MarcFilter = null;

        // 兼容以前的版本
        public int GetMarc(
    string strBiblioRecPath,
    out string strMARC,
    out string strOutMarcSyntax,
    out string strError)
        {
            LibraryChannel channel = this.GetChannel();

            try
            {
                return GetMarc(
            channel,
            strBiblioRecPath,
        out strMARC,
        out strOutMarcSyntax,
        out strError);
            }
            finally
            {
                this.ReturnChannel(channel);
            }
        }

        // 2017/4/8 新版本，具有 channel 参数
        // 获得MARC格式书目记录
        // return:
        //      -1  出错
        //      0   空记录
        //      1   成功
        /// <summary>
        /// 获得书目记录的 MARC (机内)格式内容
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="strBiblioRecPath">书目记录路径</param>
        /// <param name="strMARC">返回 MARC 机内格式字符串</param>
        /// <param name="strOutMarcSyntax">返回记录的 MARC 格式类型</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1:  出错</para>
        /// <para>0:   空记录</para>
        /// <para>1:   成功</para>
        /// </returns>
        public int GetMarc(
            LibraryChannel channel,
            string strBiblioRecPath,
            out string strMARC,
            out string strOutMarcSyntax,
            out string strError)
        {
            strError = "";
            strMARC = "";
            strOutMarcSyntax = "";
            int nRet = 0;

            string[] formats = new string[1];
            formats[0] = "xml";

            string[] results = null;
            byte[] timestamp = null;

            Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");

            long lRet = channel.GetBiblioInfos(
                    null, // stop,
                    strBiblioRecPath,
                    "",
                    formats,
                    out results,
                    out timestamp,
                    out strError);
            if (lRet == -1 || lRet == 0)
            {
                if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                    strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                strError = "获得书目记录时发生错误: " + strError;
                return -1;
            }

            string strXml = results[0];

            if (string.IsNullOrEmpty(strXml) == true)
            {
                strError = "书目记录 '" + strBiblioRecPath + "' 是一条空记录";
                return 0;
            }

            // 转换为MARC格式
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strXml,
                true,   // 2013/1/12 修改为true
                "", // strMarcSyntax
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            return 1;
        }

        // parameters:
        //      strMarcFilterFilePath   脚本文件名，或者脚本内容。如果是脚本内容，第一个字符应该确保为 '<'
        /// <summary>
        /// 准备 MARC 过滤器
        /// </summary>
        /// <param name="strMarcFilterFilePath">MARC过滤器 .fltx 文件名全路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int PrepareMarcFilter(string strMarcFilterFilePath,
            out string strError)
        {
            strError = "";
            ColumnFilterDocument filter = null;

            this.ColumnTable = new Hashtable();
            // parameters:
            //      strFilterFileName   脚本文件名，或者脚本内容。如果是脚本内容，第一个字符应该确保为 '<'
            int nRet = PrepareMarcFilter(strMarcFilterFilePath,
                out filter,
                out strError);
            if (nRet == -1)
                return -1;

            //
            if (filter != null)
                this.AssemblyFilter = filter.Assembly;
            else
                this.AssemblyFilter = null;

            this.MarcFilter = filter;

            return 0;
        }


        // 准备脚本环境
        // parameters:
        //      strFilterFileName   脚本文件名，或者脚本内容。如果是脚本内容，第一个字符应该确保为 '<'
        int PrepareMarcFilter(
            string strFilterFileName,
            out ColumnFilterDocument filter,
            out string strError)
        {
            strError = "";
            filter = null;

            bool bFile = true;
            if (string.IsNullOrEmpty(strFilterFileName) == false
    && strFilterFileName[0] == '<')
                bFile = false;

            if (bFile == true && FileUtil.FileExist(strFilterFileName) == false)
            {
                strError = "文件 '" + strFilterFileName + "' 不存在";
                goto ERROR1;
            }

            string strWarning = "";

            string strLibPaths = "\"" + Program.MainForm.DataDir + "\"";
            Type entryClassType = this.GetType();

            filter = new ColumnFilterDocument();
            filter.Host = new ColumnFilterHost();
            filter.Host.ColumnTable = this.ColumnTable;

            filter.strOtherDef = "dp2Circulation.ColumnFilterHost Host = null;";
            filter.strPreInitial = " ColumnFilterDocument doc = (ColumnFilterDocument)this.Document;\r\n";
            filter.strPreInitial += " Host = doc.Host;\r\n";
            /*
           filter.strOtherDef = entryClassType.FullName + " Host = null;";

           filter.strPreInitial = " ColumnFilterDocument doc = (ColumnFilterDocument)this.Document;\r\n";
           filter.strPreInitial += " Host = ("
               + entryClassType.FullName + ")doc.Host;\r\n";
            * */

            try
            {
                if (bFile == false)
                    filter.LoadContent(strFilterFileName);  // 2013/3/27
                else
                    filter.Load(strFilterFileName);
            }
            catch (Exception ex)
            {
                if (bFile == true)
                    strError = "文件 " + strFilterFileName + " 装载到 MarcFilter 时发生错误: " + ex.Message;
                else
                    strError = "XML代码装载到 MarcFilter 时发生错误: " + ex.Message;
                goto ERROR1;
            }

            string strCode = "";    // c#代码
            int nRet = filter.BuildScriptFile(out strCode,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 一些必要的链接库
            string[] saAddRef1 = {
										 Environment.CurrentDirectory + "\\digitalplatform.core.dll",
                                         Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
                                         Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marcquery.dll",
										 //Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
										 //Environment.CurrentDirectory + "\\digitalplatform.library.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
										 // Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
										 Environment.CurrentDirectory + "\\dp2circulation.exe"
                };

            // fltx文件里显式增补的链接库
            string[] saAdditionalRef = filter.GetRefs();

            // 合并的链接库
            string[] saTotalFilterRef = new string[saAddRef1.Length + saAdditionalRef.Length];
            Array.Copy(saAddRef1, saTotalFilterRef, saAddRef1.Length);
            Array.Copy(saAdditionalRef, 0,
                saTotalFilterRef, saAddRef1.Length,
                saAdditionalRef.Length);

            Assembly assemblyFilter = null;

            // 创建Script的Assembly
            // 本函数内对saRef不再进行宏替换
            nRet = ScriptManager.CreateAssembly_1(strCode,
                saTotalFilterRef,
                strLibPaths,
                out assemblyFilter,
                out strError,
                out strWarning);
            if (nRet == -2)
                goto ERROR1;
            if (nRet == -1)
            {
                if (strWarning == "")
                {
                    goto ERROR1;
                }
                MessageBox.Show(this, strWarning);
            }

            filter.Assembly = assemblyFilter;

            return 0;
        ERROR1:
            return -1;
        }

        /// <summary>
        /// 错误信息窗
        /// </summary>
        HtmlViewerForm m_errorInfoForm = null;

        /// <summary>
        /// 获得错误信息窗
        /// </summary>
        /// <returns>错误信息窗</returns>
        public HtmlViewerForm GetErrorInfoForm()
        {
            if (this.m_errorInfoForm == null
                || this.m_errorInfoForm.IsDisposed == true
                || this.m_errorInfoForm.IsHandleCreated == false)
            {
                this.m_errorInfoForm = new HtmlViewerForm();
                this.m_errorInfoForm.ShowInTaskbar = false;
                this.m_errorInfoForm.Text = "错误信息";
                this.m_errorInfoForm.Show(this);
                this.m_errorInfoForm.WriteHtml("<pre>");  // 准备文本输出
            }

            return this.m_errorInfoForm;
        }

        /// <summary>
        /// 清除错误信息窗内容
        /// </summary>
        public void ClearErrorInfoForm()
        {
            // 清除错误信息窗口中残余的内容
            if (this.m_errorInfoForm != null)
            {
                try
                {
                    this.m_errorInfoForm.HtmlString = "<pre>";
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 关闭错误信息窗
        /// </summary>
        public void CloseErrorInfoForm()
        {
            if (this.m_errorInfoForm != null)
            {
                try
                {
                    this.m_errorInfoForm.Close();
                }
                catch
                {
                }

                this.m_errorInfoForm = null;
            }
        }

        /// <summary>
        /// 清除列表中现存的内容，准备装入新内容
        /// </summary>
        public virtual void ClearBefore()
        {
            this.ClearErrorInfoForm();
            this.m_summaryTable.Clear();
        }

        // 检查路径所从属书目库是否为图书/期刊库？
        // return:
        //      -1  error
        //      0   不符合要求。提示信息在strError中
        //      1   符合要求
        internal virtual int CheckItemRecPath(string strLoadType,
            string strItemRecPath,
            out string strError)
        {
            strError = "尚未重载 CheckItemRecPath() ";
            return -1;
        }

        // 从记录路径文件装载
        // TODO: 改造成可以从非 UI 线程中调用，如果能用 list<string> 指定参数则更好了
        /// <summary>
        /// 从记录路径文件装载
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="strRecPathFilename">记录路径文件名(全路径)</param>
        /// <param name="strPubType">出版物类型</param>
        /// <param name="bFillSummaryColumn">是否填充书目摘要列</param>
        /// <param name="summary_col_names">书目摘要列的名称数组</param>
        /// <param name="bClearBefore">是否要在装载前情况浏览列表</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError 参数返回; 0: 成功</returns>
        public int LoadFromRecPathFile(
            LibraryChannel channel,
            string strRecPathFilename,
            string strPubType,
            bool bFillSummaryColumn,
            string[] summary_col_names,
            bool bClearBefore,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (bClearBefore == true)
                ClearBefore();

            string strTimeMessage = "";

            StreamReader sr = null;
            try
            {
                // 打开文件
                sr = new StreamReader(strRecPathFilename);

                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在初始化浏览器组件 ...");
                stop.BeginLoop();
                this.Update();
                Program.MainForm.Update();

                try
                {
                    // this.m_nGreenItemCount = 0;

                    // 逐行读入文件内容
                    // 测算文件行数
                    int nLineCount = 0;
                    for (; ; )
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            goto ERROR1;
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;   // 跳过空行了

                        // 检查路径所从属书目库是否为图书/期刊库？
                        // return:
                        //      -1  error
                        //      0   不符合要求。提示信息在strError中
                        //      1   符合要求
                        nRet = CheckItemRecPath(strPubType,
                            strLine,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            GetErrorInfoForm().WriteHtml(strError + "\r\n");
                        }

                        nLineCount++;
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    sr.Close();

                    ProgressEstimate estimate = new ProgressEstimate();
                    estimate.SetRange(0, nLineCount);
                    estimate.StartEstimate();

                    List<string> lines = new List<string>();
                    // 正式开始处理
                    sr = new StreamReader(strRecPathFilename);
                    for (int i = 0; ; )
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            goto ERROR1;
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();


                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        stop.SetProgressValue(i);

                        lines.Add(strLine);
                        if (lines.Count >= 100)
                        {
                            if (lines.Count > 0)
                                stop.SetMessage("(" + i.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录。"
                                    + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(i)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));

                            // 处理一小批记录的装入
                            nRet = DoLoadRecords(
                                channel,
                                lines,
                                null,
                                bFillSummaryColumn,
                                summary_col_names,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            lines.Clear();
                        }

                        i++;   // 进度数字不包括空行
                    }

                    // 最后剩下的一批
                    if (lines.Count > 0)
                    {
                        if (lines.Count > 0)
                            stop.SetMessage("(" + nLineCount.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录...");

                        // 处理一小批记录的装入
                        nRet = DoLoadRecords(
                            channel,
                            lines,
                            null,
                            bFillSummaryColumn,
                            summary_col_names,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        lines.Clear();
                    }

                    strTimeMessage = "共装入册记录 " + nLineCount.ToString() + " 条。耗费时间: " + estimate.GetTotalTime().ToString();
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = "BatchPrintFormBase LoadFromRecPathFile() exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            finally
            {
                sr.Close();
            }

            Program.MainForm.StatusBarMessage = strTimeMessage;

            return 0;
        ERROR1:
            return -1;
        }


        // 处理一小批记录的装入
        internal virtual int DoLoadRecords(
            LibraryChannel channel,
            List<string> lines,
            List<ListViewItem> items,
            bool bFillSummaryColumn,
            string[] summary_col_names,
            out string strError)
        {
            strError = "尚未重载 DoLoadRecords() ";
            return -1;
        }

        // 书目记录路径 --> SummaryInfo
        Hashtable m_summaryTable = new Hashtable();
        internal class SummaryInfo
        {
#if NO
            public string Summary = "";
            public string ISBnISSn = "";
            public string TargetRecPath = "";
#endif
            public string[] Values = null;    // 按照 names 顺序排列的值
        }

        internal class RecordInfo
        {
            public DigitalPlatform.LibraryClient.localhost.Record Record = null;    // 册记录
            public XmlDocument Dom = null;  // 册记录XML装入DOM
            public string BiblioRecPath = "";
            public SummaryInfo SummaryInfo = null;  // 摘要信息
        }

        // 2017/4/8 新版本函数，使用了 channel 参数
        internal virtual int GetSummaries(
            LibraryChannel channel,
    bool bFillSummaryColumn,
    string[] summary_col_names,
    List<DigitalPlatform.LibraryClient.localhost.Record> records,
    out List<RecordInfo> infos,
    out string strError)
        {
            strError = "";
            infos = new List<RecordInfo>();

            // 准备DOM和书目摘要
            for (int i = 0; i < records.Count; i++)
            {
                Application.DoEvents();

                if (stop != null && stop.State != 0)
                {
                    strError = "用户中断1";
                    return -1;
                }

                RecordInfo info = new RecordInfo();
                info.Record = records[i];
                infos.Add(info);

                if (info.Record.RecordBody == null)
                {
                    strError = "请升级dp2Kernel到最新版本";
                    return -1;
                }

                if (info.Record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                    continue;

                info.Dom = new XmlDocument();
                try
                {
                    info.Dom.LoadXml(info.Record.RecordBody.Xml);
                }
                catch (Exception ex)
                {
                    strError = "册记录的XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                // 准备书目记录路径
                string strParentID = DomUtil.GetElementText(info.Dom.DocumentElement,
"parent");
                string strBiblioDbName = Program.MainForm.GetBiblioDbNameFromItemDbName(Global.GetDbName(info.Record.Path));
                if (string.IsNullOrEmpty(strBiblioDbName) == true)
                {
                    strError = "根据册记录路径 '" + info.Record.Path + "' 获得书目库名时出错";
                    return -1;
                }
                info.BiblioRecPath = strBiblioDbName + "/" + strParentID;
            }

            // 准备摘要
            if (bFillSummaryColumn == true)
            {
                if (summary_col_names == null || summary_col_names.Length == 0)
                {
                    strError = "当 bFillSummaryColumn 为 true 的时候，summary_col_names 不应为空";
                    return -1;
                }

                // 归并书目记录路径
                List<string> bibliorecpaths = new List<string>();
                foreach (RecordInfo info in infos)
                {
                    bibliorecpaths.Add(info.BiblioRecPath);
                }

                // 去重
                StringUtil.RemoveDupNoSort(ref bibliorecpaths);

                // 看看cache中是否已经存在，如果已经存在则不再从服务器取
                for (int i = 0; i < bibliorecpaths.Count; i++)
                {
                    string strPath = bibliorecpaths[i];
                    SummaryInfo summary = (SummaryInfo)this.m_summaryTable[strPath];
                    if (summary != null)
                    {
                        bibliorecpaths.RemoveAt(i);
                        i--;
                    }
                }

                // 从服务器获取
                if (bibliorecpaths.Count > 0)
                {
                REDO_GETBIBLIOINFO_0:
                    string strCommand = "@path-list:" + StringUtil.MakePathList(bibliorecpaths);

#if NO
                    string[] formats = new string[2];
                    formats[0] = "summary";
                    formats[1] = "@isbnissn";
                    formats[2] = "targetrecpath";
#endif

                    string[] results = null;
                    byte[] timestamp = null;

                    // stop.SetMessage("正在装入书目记录 '" + bibliorecpaths[0] + "' 等的摘要 ...");

                    // TODO: 有没有可能希望取的事项数目一次性取得没有取够?
                REDO_GETBIBLIOINFO:
                    long lRet = channel.GetBiblioInfos(
                        stop,
                        strCommand,
                        "",
                        summary_col_names,
                        out results,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        this.FormCaption,
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETBIBLIOINFO;
                    }
                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                            strError = "书目记录 '" + StringUtil.MakePathList(bibliorecpaths) + "' 不存在";

                        strError = "获得书目摘要时发生错误: " + strError;
                        // 如果results.Length表现正常，其实还可以继续处理
                        if (results != null /* && results.Length == 2 * bibliorecpaths.Count */)
                        {
                        }
                        else
                            return -1;
                    }

                    if (results != null/* && results.Length == 2 * bibliorecpaths.Count*/)
                    {
                        // Debug.Assert(results != null && results.Length == 2 * bibliorecpaths.Count, "results必须包含 " + (2 * bibliorecpaths.Count).ToString() + " 个元素");

                        // 放入缓存
                        for (int i = 0; i < results.Length / summary_col_names.Length; i++)
                        {
                            SummaryInfo summary = new SummaryInfo();

                            summary.Values = new string[summary_col_names.Length];
                            for (int j = 0; j < summary_col_names.Length; j++)
                            {
                                summary.Values[j] = results[i * summary_col_names.Length + j];
                            }

                            this.m_summaryTable[bibliorecpaths[i]] = summary;
                        }
                    }

                    if (results != null && results.Length != summary_col_names.Length * bibliorecpaths.Count)
                    {
                        // 没有取够，需要继续处理
                        bibliorecpaths.RemoveRange(0, results.Length / summary_col_names.Length);
                        goto REDO_GETBIBLIOINFO_0;
                    }
                }

                // 挂接到每个记录附近
                foreach (RecordInfo info in infos)
                {
                    SummaryInfo summary = (SummaryInfo)this.m_summaryTable[info.BiblioRecPath];
                    if (summary == null)
                    {
                        strError = "缓存中找不到书目记录 '" + info.BiblioRecPath + "' 的摘要事项";
                        return -1;
                    }

                    info.SummaryInfo = summary;
                }

                // 避免cache占据的内存太多
                if (this.m_summaryTable.Count > 1000)
                    this.m_summaryTable.Clear();
            }

            return 0;
        }

        // 老版本，没有 channel 参数
        // 准备DOM和书目摘要等
        // parameters:
        //      bFillSummaryColumn  是否要填充书目摘要列。如果为 false，则只设定 info.BiblioRecPath 的值
        internal virtual int GetSummaries(
            bool bFillSummaryColumn,
            string[] summary_col_names,
            List<DigitalPlatform.LibraryClient.localhost.Record> records,
            out List<RecordInfo> infos,
            out string strError)
        {
            return GetSummaries(
                this.Channel,
                bFillSummaryColumn,
                summary_col_names,
                records,
                out infos,
                out strError);
        }

        internal virtual void SetError(ListView list,
            ref ListViewItem item,
            string strBarcodeOrRecPath,
            string strError)
        {
            throw new Exception("尚未重载 SetError() ");
        }

        internal virtual ListViewItem AddToListView(ListView list,
            XmlDocument dom,
            byte[] baTimestamp,
            string strRecPath,
            string strBiblioRecPath,
            string[] summary_col_names,
            SummaryInfo summary)
        {
            throw new Exception("尚未重载 AddToListView() ");
        }

        // 根据册记录 DOM 设置 ListViewItem 除第一列以外的文字
        // parameters:
        //      baTimestamp     册记录的时间戳。如果需要这个信息，则要在 获取册信息阶段注意包含这个要求
        //      bSetBarcodeColumn   是否要设置条码列内容(第一列)
        internal virtual void SetListViewItemText(XmlDocument dom,
            byte[] baTimestamp,
            bool bSetBarcodeColumn,
            string strRecPath,
            string strBiblioRecPath,
            string[] summary_col_names,
            SummaryInfo summary,
            ListViewItem item)
        {
            throw new Exception("尚未重载 SetListViewItemText() ");
        }

        internal virtual int VerifyItem(
    string strPubType,
    string strBarcodeOrRecPath,
    ListViewItem item,
    XmlDocument item_dom,
    out string strError)
        {
            strError = "";
            return 0;
        }

        // 2017/4/8 新版本，有 channel 参数
        // 根据册条码号或者记录路径，装入册记录
        // parameters:
        //      strBarcodeOrRecPath 册条码号或者记录路径。如果内容前缀为"@path:"则表示为路径
        //      strMatchLocation    附加的馆藏地点匹配条件。如果==null，表示没有这个附加条件(注意，""和null含义不同，""表示确实要匹配这个值)
        // return: 
        //      -2  册条码号或者记录路径已经在list中存在了(行没有加入listview中)
        //      -1  出错(注意表示出错的行已经加入listview中了)
        //      0   因为馆藏地点不匹配，没有加入list中
        //      1   成功
        internal virtual int LoadOneItem(
            LibraryChannel channel,
            string strPubType,
            bool bFillSummaryColumn,
            string[] summary_col_names,
            string strBarcodeOrRecPath,
            RecordInfo info,
            ListView list,
            string strMatchLocation,
            out string strOutputItemRecPath,
            ref ListViewItem item,
            out string strError)
        {
            strError = "";
            strOutputItemRecPath = "";
            long lRet = 0;

            // 判断是否有 @path: 前缀，便于后面分支处理
            bool bIsRecPath = StringUtil.HasHead(strBarcodeOrRecPath, "@path:");

            string strItemText = "";
            string strBiblioText = "";

            // string strItemRecPath = "";
            string strBiblioRecPath = "";
            XmlDocument item_dom = null;
#if NO
            string strBiblioSummary = "";
            string strISBnISSN = "";
#endif
            SummaryInfo summary = null;
            byte[] item_timestamp = null;


            if (info == null)
            {

            REDO_GETITEMINFO:
                lRet = channel.GetItemInfo(
                    stop,
                    strBarcodeOrRecPath,
                    "xml",
                    out strItemText,
                    out strOutputItemRecPath,
                    out item_timestamp,
                    "recpath",
                    out strBiblioText,
                    out strBiblioRecPath,
                    out strError);
                if (lRet == -1)
                {
                    DialogResult temp_result = MessageBox.Show(this,
    strError + "\r\n\r\n是否重试?",
    this.FormCaption,
    MessageBoxButtons.RetryCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETITEMINFO;
                }
                if (lRet == -1 || lRet == 0)
                {
                    SetError(list,
                        ref item,
                        strBarcodeOrRecPath,
                        strError);
                    goto ERROR1;
                }

                summary = (SummaryInfo)this.m_summaryTable[strBiblioRecPath];
                if (summary != null)
                {
#if NO
                    strBiblioSummary = summary.Summary;
                    strISBnISSN = summary.ISBnISSn;
#endif
                }

                if (summary == null
                    && bFillSummaryColumn == true)
                {
                    string[] results = null;
                    byte[] timestamp = null;

                    stop.SetMessage("正在装入书目记录 '" + strBiblioRecPath + "' 的摘要 ...");

                    Debug.Assert(String.IsNullOrEmpty(strBiblioRecPath) == false, "strBiblioRecPath值不能为空");
                REDO_GETBIBLIOINFO:
                    lRet = channel.GetBiblioInfos(
                        stop,
                        strBiblioRecPath,
                        "",
                        summary_col_names,
                        out results,
                        out timestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        DialogResult temp_result = MessageBox.Show(this,
        strError + "\r\n\r\n是否重试?",
        this.FormCaption,
        MessageBoxButtons.RetryCancel,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button1);
                        if (temp_result == DialogResult.Retry)
                            goto REDO_GETBIBLIOINFO;
                    }
                    if (lRet == -1 || lRet == 0)
                    {
                        if (lRet == 0 && String.IsNullOrEmpty(strError) == true)
                            strError = "书目记录 '" + strBiblioRecPath + "' 不存在";

                        // strBiblioSummary = "获得书目摘要时发生错误: " + strError;
                        summary = new SummaryInfo();
                        summary.Values = new string[1];
                        summary.Values[0] = "获得书目摘要时发生错误: " + strError;
                    }
                    else
                    {
                        Debug.Assert(results != null && results.Length == summary_col_names.Length, "results必须包含 " + summary_col_names.Length + " 个元素");

#if NO
                        strBiblioSummary = results[0];
                        strISBnISSN = results[1];
#endif

                        // 避免cache占据的内存太多
                        if (this.m_summaryTable.Count > 1000)
                            this.m_summaryTable.Clear();

                        if (summary == null)
                        {
                            summary = new SummaryInfo();
                            summary.Values = new string[summary_col_names.Length];
                            for (int j = 0; j < summary_col_names.Length; j++)
                            {
                                summary.Values[j] = results[j];
                            }
                            this.m_summaryTable[strBiblioRecPath] = summary;
                        }
                    }
                }

                // 剖析一个册的xml记录，取出有关信息放入listview中
                if (item_dom == null)
                {
                    item_dom = new XmlDocument();
                    try
                    {
                        item_dom.LoadXml(strItemText);
                    }
                    catch (Exception ex)
                    {
                        strError = "册记录的XML装入DOM时出错: " + ex.Message;
                        goto ERROR1;
                    }
                }

            }
            else
            {
                // record 不为空调用时，对调用时参数strBarcodeOrRecPath不作要求

                strBarcodeOrRecPath = "@path:" + info.Record.Path;
                bIsRecPath = true;

                if (info.Record.RecordBody.Result.ErrorCode != ErrorCodeValue.NoError)
                {
                    SetError(list,
    ref item,
    strBarcodeOrRecPath,
    info.Record.RecordBody.Result.ErrorString);
                    goto ERROR1;
                }

                strItemText = info.Record.RecordBody.Xml;
                strOutputItemRecPath = info.Record.Path;
                // 2013/4/3
                if (info.Record.RecordBody != null)
                    item_timestamp = info.Record.RecordBody.Timestamp;
                //
                item_dom = info.Dom;
                strBiblioRecPath = info.BiblioRecPath;
                if (info.SummaryInfo != null)
                {
#if NO
                    strBiblioSummary = info.SummaryInfo.Summary;
                    strISBnISSN = info.SummaryInfo.ISBnISSn;
#endif
                    summary = info.SummaryInfo;
                }
            }


            // 附加的馆藏地点匹配
            if (strMatchLocation != null)
            {
                // TODO: #reservation, 情况如何处理?
                string strLocation = DomUtil.GetElementText(item_dom.DocumentElement,
                    "location");

                // 2013/3/26
                if (strLocation == null)
                    strLocation = "";

                if (strMatchLocation != strLocation)
                    return 0;
            }

            if (item == null)
            {
                item = AddToListView(list,
                    item_dom,
                    item_timestamp,
                    strOutputItemRecPath,
                    strBiblioRecPath,
#if NO
                    strBiblioSummary,
                    strISBnISSN,
#endif
 summary_col_names,
                    summary);


                // 将新加入的事项滚入视野
                list.EnsureVisible(list.Items.Count - 1);

#if NO
                // 填充需要从订购库获得的栏目信息
                if (this.checkBox_load_fillOrderInfo.Checked == true)
                    FillOrderColumns(item, strPubType);
#endif
            }
            else
            {
                SetListViewItemText(item_dom,
                    item_timestamp,
                    true,
                    strOutputItemRecPath,
                    strBiblioRecPath,
#if NO
    strBiblioSummary,
    strISBnISSN,
#endif
 summary_col_names,
                    summary,
                    item);
            }

            int nRet = VerifyItem(
                strPubType,
                strBarcodeOrRecPath,
                item,
                item_dom,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return 1;
        ERROR1:
            return -1;
        }

        // 老版本，没有 channel 参数
        internal virtual int LoadOneItem(
    string strPubType,
    bool bFillSummaryColumn,
    string[] summary_col_names,
    string strBarcodeOrRecPath,
    RecordInfo info,
    ListView list,
    string strMatchLocation,
    out string strOutputItemRecPath,
    ref ListViewItem item,
    out string strError)
        {
            return LoadOneItem(
                this.Channel,
                        strPubType,
                        bFillSummaryColumn,
                        summary_col_names,
                        strBarcodeOrRecPath,
                        info,
                        list,
                        strMatchLocation,
                        out strOutputItemRecPath,
                        ref item,
                        out strError);
        }

        #region ConvertBarcodeFile 把册条码号翻译为记录路径

        // 根据册条码号文件得到记录路径文件
        internal virtual int ConvertBarcodeFile(
            LibraryChannel channel,
            string strBarcodeFilename,
            string strRecPathFilename,
            out int nDupCount,
            out string strError)
        {
            nDupCount = 0;
            strError = "";
            int nRet = 0;

            StreamReader sr = null;
            StreamWriter sw = null;

            try
            {
                // 打开文件
                sr = new StreamReader(strBarcodeFilename);

                sw = new StreamWriter(strRecPathFilename);

                EnableControls(false);
                // MainForm.ShowProgress(true);

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在将册条码号转换为记录路径 ...");
                stop.BeginLoop();
                this.Update();
                Program.MainForm.Update();

                try
                {
                    Hashtable barcode_table = new Hashtable();
                    // this.m_nGreenItemCount = 0;

                    // 逐行读入文件内容
                    // 测算文件行数
                    int nLineCount = 0;
                    List<string> lines = new List<string>();
                    for (; ; )
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            goto ERROR1;
                        }

                        string strLine = "";
                        strLine = sr.ReadLine();

                        if (strLine == null)
                            break;

                        strLine = strLine.Trim();
                        if (String.IsNullOrEmpty(strLine) == true)
                            continue;

                        if (strLine[0] == '#')
                            continue;   // 注释行

                        if (barcode_table[strLine] != null)
                        {
                            nDupCount++;
                            continue;
                        }

                        //

                        barcode_table[strLine] = true;
                        lines.Add(strLine);
                        nLineCount++;
                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");
                    }

                    barcode_table.Clear(); // 腾出空间

                    // 设置进度范围
                    stop.SetProgressRange(0, nLineCount);
                    // stop.SetProgressValue(0);

                    // 逐行处理
                    // 文件回头?
                    // sr.BaseStream.Seek(0, SeekOrigin.Begin);

                    sr.Close();
                    sr = null;


                    int i = 0;
                    List<string> temp_lines = new List<string>();
                    foreach (string s in lines)
                    {
                        Application.DoEvents();

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断1";
                            goto ERROR1;
                        }

                        stop.SetProgressValue(i++);

                        string strLine = s;

                        // 2017/5/17
                        // 变换条码号
                        if (Program.MainForm.NeedTranformBarcode(Program.MainForm.FocusLibraryCode) == true)
                        {
                            string strText = strLine;

                            nRet = Program.MainForm.TransformBarcode(
                                Program.MainForm.FocusLibraryCode,
                                ref strText,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            strLine = strText;
                        }

                        // stop.SetMessage("正在装入册条码号 " + strLine + " 对应的记录...");

                        temp_lines.Add(strLine);
                        if (temp_lines.Count >= 100)
                        {
                            // 将册条码号转换为册记录路径
                            List<string> recpaths = null;
                            nRet = ConvertItemBarcodeToRecPath(
                                channel,
                                temp_lines,
                                out recpaths,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            foreach (string recpath in recpaths)
                            {
                                sw.WriteLine(recpath);
                            }
                            temp_lines.Clear();
                        }
                    }

                    // 最后一批
                    if (temp_lines.Count > 0)
                    {
                        // 将册条码号转换为册记录路径
                        List<string> recpaths = null;
                        nRet = ConvertItemBarcodeToRecPath(
                            channel,
                            temp_lines,
                            out recpaths,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        foreach (string recpath in recpaths)
                        {
                            sw.WriteLine(recpath);
                        }
                        temp_lines.Clear();
                    }
                }
                finally
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("装入完成。");
                    stop.HideProgress();

                    EnableControls(true);
                    // MainForm.ShowProgress(false);
                }
            }
            catch (Exception ex)
            {
                strError = "BatchPrintFormBase ConvertBarcodeFile() exception: " + ExceptionUtil.GetAutoText(ex);
                goto ERROR1;
            }
            finally
            {
                if (sr != null)
                    sr.Close();
                if (sw != null)
                    sw.Close();
            }

            return 0;
        ERROR1:
            return -1;
        }

        // parameters:
        //      channel_param   如果为空，则自动获得一个通道完成任务
        internal int ConvertItemBarcodeToRecPath(
            LibraryChannel channel_param,
            List<string> barcodes,
            out List<string> recpaths,
            out string strError)
        {
            strError = "";
            recpaths = null;

            LibraryChannel channel = channel_param;
            if (channel == null)
                channel = this.GetChannel();
            try
            {
            REDO_GETITEMINFO:
                string strBiblio = "";
                long lRet = channel.GetItemInfo(stop,
                    "@barcode-list:" + StringUtil.MakePathList(barcodes),
                    "get-path-list",
                    out string strResult,
                    "", // strBiblioType,
                    out strBiblio,
                    out strError);
                if (lRet == -1)
                    return -1;
                recpaths = StringUtil.SplitList(strResult);

                if (recpaths.Count == 0 && barcodes.Count == 1)
                    recpaths.Add("");
                else
                {
                    Debug.Assert(barcodes.Count == recpaths.Count, "");
                }

                if (this.InvokeRequired == true)
                {
                    for (int i = 0; i < recpaths.Count; i++)
                    {
                        string recpath = recpaths[i];
                        if (string.IsNullOrEmpty(recpath) == true)
                            recpaths[i] = "!条码号 " + barcodes[i] + " 没有找到";
                    }

                    return 0;
                }

                List<string> notfound_barcodes = new List<string>();
                List<string> errors = new List<string>();
                {
                    int i = 0;
                    foreach (string recpath in recpaths)
                    {
                        if (string.IsNullOrEmpty(recpath) == true)
                            notfound_barcodes.Add(barcodes[i]);
                        else if (recpath[0] == '!')
                            errors.Add(recpath.Substring(1));
                        i++;
                    }
                }

                if (errors.Count > 0)
                {
                    strError = "转换册条码号的过程发生错误: " + StringUtil.MakePathList(errors);

                    DialogResult temp_result = MessageBox.Show(this,
                        strError + "\r\n\r\n是否重试?",
                        this.FormCaption,
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Retry)
                        goto REDO_GETITEMINFO;
                    return -1;
                }

                if (notfound_barcodes.Count > 0)
                {
                    if (string.IsNullOrEmpty(strError) == false)
                        strError += ";\r\n";

                    strError += "下列册条码号没有找到: " + StringUtil.MakePathList(notfound_barcodes);
                    DialogResult temp_result = MessageBox.Show(this,
                        strError + "\r\n\r\n是否继续处理?",
                        this.FormCaption,
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (temp_result == DialogResult.Cancel)
                        return -1;
                }

                /*
                if (string.IsNullOrEmpty(strError) == false)
                    return -1;
                 * */
                // 把空字符串和 ！ 打头的都去掉
                for (int i = 0; i < recpaths.Count; i++)
                {
                    string recpath = recpaths[i];
                    if (string.IsNullOrEmpty(recpath) == true)
                    {
                        recpaths.RemoveAt(i);
                        i--;
                    }
                    else if (recpath[0] == '!')
                    {
                        recpaths.RemoveAt(i);
                        i--;
                    }
                }

                return 0;
            }
            finally
            {
                if (channel_param == null)
                    this.ReturnChannel(channel);
            }
        }

        #endregion

        #region SearchBatchNoAndLocation 根据批次号装入册记录

        // 检索 批次号 和 馆藏地点 将命中的记录路径写入文件
        // parameters:
        //      strBatchNo 要限定的批次号。如果为 "" 表示批次号为空，而 null 表示不指定批次号
        //      strLocation 要限定的馆藏地点名称。如果为 "" 表示馆藏地点为空，而 null 表示不指定馆藏地点
        /// <summary>
        /// 检索 批次号 和 馆藏地点 将命中的记录路径写入文件
        /// </summary>
        /// <param name="channel">通讯通道</param>
        /// <param name="strPubType">出版物类型</param>
        /// <param name="strBatchNo">批次号</param>
        /// <param name="strLocation">馆藏地点</param>
        /// <param name="strOutputFilename">输出文件名全路径</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有命中; 其他: 命中的记录数</returns>
        public virtual int SearchBatchNoAndLocation(
            LibraryChannel channel,
            string strPubType,
            string strBatchNo,
            string strLocation,
            string strOutputFilename,
            out string strError)
        {
            strError = "";
            long lRet = 0;

            EnableControls(false);
            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在检索批次号 '" + strBatchNo + "' 和馆藏地点 '" + strLocation + "' ...");
            stop.BeginLoop();

            try
            {
                string strQueryXml = "";

                if (strBatchNo != null
                    && strLocation != null)
                {
                    string strBatchNoQueryXml = "";
                    lRet = channel.SearchItem(
        stop,
         strPubType == "图书" ? "<all book>" : "<all series>",
        strBatchNo,
        -1,
        "批次号",
        "exact",
        this.Lang,
        "null",   // strResultSetName
        "",    // strSearchStyle
        "__buildqueryxml", // strOutputStyle
        out strError);
                    if (lRet == -1)
                        return -1;
                    strBatchNoQueryXml = strError;

                    string strLocationQueryXml = "";
                    lRet = channel.SearchItem(
        stop,
         strPubType == "图书" ? "<all book>" : "<all series>",
        strLocation,
        -1,
        "馆藏地点",
        "exact",
        this.Lang,
        "null",   // strResultSetName
        "",    // strSearchStyle
        "__buildqueryxml", // strOutputStyle
        out strError);
                    if (lRet == -1)
                        return -1;
                    strLocationQueryXml = strError;

                    // 合并成一个检索式
                    strQueryXml = "<group>" + strBatchNoQueryXml + "<operator value='AND'/>" + strLocationQueryXml + "</group>";    // !!!
#if DEBUG
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.LoadXml(strQueryXml);
                    }
                    catch (Exception ex)
                    {
                        strError = "合并检索式进入DOM时出错: " + ex.Message;
                        return -1;
                    }
#endif


                }
                else if (strBatchNo != null)
                {
                    stop.SetMessage("正在检索批次号 '" + strBatchNo + "' ...");

                    lRet = channel.SearchItem(
        stop,
         strPubType == "图书" ? "<all book>" : "<all series>",
        strBatchNo,
        -1,
        "批次号",
        "exact",
        this.Lang,
        "null",   // strResultSetName
        "",    // strSearchStyle
        "__buildqueryxml", // strOutputStyle
        out strError);
                    if (lRet == -1)
                        return -1;
                    strQueryXml = strError;
                }
                else if (strLocation != null)
                {
                    stop.SetMessage("正在检索馆藏地点 '" + strLocation + "' ...");

                    lRet = channel.SearchItem(
    stop,
    strPubType == "图书" ? "<all book>" : "<all series>",
    strLocation,    // strBatchNo, BUG !!!
    -1,
    "馆藏地点",
    "exact",
    this.Lang,
    "null",   // strResultSetName
    "",    // strSearchStyle
    "__buildqueryxml", // strOutputStyle
    out strError);
                    if (lRet == -1)
                        return -1;
                    strQueryXml = strError;
                }
                else
                {
                    Debug.Assert(strBatchNo == null && strLocation == null,
                        "");
                    lRet = channel.SearchItem(
    stop,
    strPubType == "图书" ? "<all book>" : "<all series>",
    "", // strBatchNo,
    -1,
    "__id",
    "left",
    this.Lang,
    "null",   // strResultSetName
    "",    // strSearchStyle
    "__buildqueryxml", // strOutputStyle
    out strError);
                    if (lRet == -1)
                        return -1;
                    strQueryXml = strError;
                }

                long lHitCount = 0;

                using (StreamWriter sw = new StreamWriter(strOutputFilename))
                {
                    lRet = channel.Search(stop,
        strQueryXml,
        "default",
        "id",   // 只要记录路径
        out strError);
                    if (lRet == -1)
                        return -1;
                    if (lRet == 0)
                        return 0;   // 没有命中

                    lHitCount = lRet;

                    stop.SetProgressRange(0, lHitCount);

                    long lStart = 0;
                    long lPerCount = Math.Min(150, lHitCount);
                    DigitalPlatform.LibraryClient.localhost.Record[] searchresults = null;

                    // 装入浏览格式
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop != null && stop.State != 0)
                        {
                            strError = "用户中断";
                        }

                        // stop.SetMessage("正在装入浏览信息 " + (lStart + 1).ToString() + " - " + (lStart + lPerCount).ToString() + " (命中 " + lHitCount.ToString() + " 条记录) ...");

                        lRet = channel.GetSearchResult(
                            stop,
                            "default",   // strResultSetName
                            lStart,
                            lPerCount,
                            "id",
                            this.Lang,
                            out searchresults,
                            out strError);
                        if (lRet == -1)
                            return -1;

                        if (lRet == 0)
                        {
                            strError = "GetSearchResult() error";
                            return -1;
                        }

                        // 处理浏览结果
                        foreach (DigitalPlatform.LibraryClient.localhost.Record record in searchresults)
                        {
                            sw.WriteLine(record.Path);
                        }

                        lStart += searchresults.Length;
                        // lCount -= searchresults.Length;
                        if (lStart >= lHitCount || lPerCount <= 0)
                            break;

                        stop.SetProgressValue(lStart);
                    }
                }

                return (int)lHitCount;
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.HideProgress();

                EnableControls(true);
                // MainForm.ShowProgress(false);
            }
        }

        #endregion

        #region RefreshLines 刷新若干行

        /// <summary>
        /// 刷新若干浏览行
        /// </summary>
        /// <param name="nRecPathColumn">记录路径列的列号</param>
        /// <param name="items">要刷新的行事项数组</param>
        /// <param name="bFillBiblioSummary">是否要填充书目摘要列</param>
        /// <param name="summary_col_names">书目摘要列名数组</param>
        public virtual void RefreshLines(
            int nRecPathColumn,
            List<ListViewItem> items,
            bool bFillBiblioSummary,
            string[] summary_col_names)
        {
            string strError = "";
            string strTimeMessage = "";
            int nRet = 0;

            if (this.InvokeRequired == false)
                EnableControls(false);
            // MainForm.ShowProgress(true);

            LibraryChannel channel = this.GetChannel();

            stop.OnStop += new StopEventHandler(this.DoStop);
            if (this.InvokeRequired == false)
                stop.Initial("正在刷新 ...");
            stop.BeginLoop();

            try
            {
                if (this.InvokeRequired == false)
                    stop.SetProgressRange(0, items.Count);

                ProgressEstimate estimate = new ProgressEstimate();
                estimate.SetRange(0, items.Count);
                estimate.StartEstimate();

                int nLineCount = 0;
                List<string> lines = new List<string>();
                List<ListViewItem> part_items = new List<ListViewItem>();
                for (int i = 0; i < items.Count; i++)
                {
                    Application.DoEvents();

                    if (stop.State != 0)
                    {
                        strError = "用户中断1";
                        goto ERROR1;
                    }

                    ListViewItem item = items[i];

                    if (this.InvokeRequired == false)
                    {
                        stop.SetMessage("正在刷新 " + item.Text + " ...");
                        stop.SetProgressValue(i);
                    }

                    string strRecPath = ListViewUtil.GetItemText(item, nRecPathColumn);

                    if (string.IsNullOrEmpty(strRecPath) == true)
                        continue;

                    lines.Add(strRecPath);
                    part_items.Add(item);
                    if (lines.Count >= 100)
                    {
                        if (this.InvokeRequired == false)
                        {
                            if (lines.Count > 0)
                                stop.SetMessage("(" + i.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录。"
                                    + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(i)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));
                        }

                        // 处理一小批记录的装入
                        nRet = DoLoadRecords(
                            channel,
                            lines,
                            part_items,
                            bFillBiblioSummary,
                            summary_col_names,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        lines.Clear();
                        part_items.Clear();
                    }
                }

                // 最后剩下的一批
                if (lines.Count > 0)
                {
                    if (this.InvokeRequired == false)
                    {
                        if (lines.Count > 0)
                            stop.SetMessage("(" + nLineCount.ToString() + " / " + nLineCount.ToString() + ") 正在装入路径 " + lines[0] + " 等记录...");
                    }

                    // 处理一小批记录的装入
                    nRet = DoLoadRecords(
                        channel,
                        lines,
                        part_items,
                        bFillBiblioSummary,
                        summary_col_names,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    lines.Clear();
                    part_items.Clear();
                }

                strTimeMessage = "共刷新册信息 " + nLineCount.ToString() + " 条。耗费时间: " + estimate.GetTotalTime().ToString();
            }
            finally
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                if (this.InvokeRequired == false)
                {
                    stop.Initial("刷新完成。");
                    stop.HideProgress();
                }

                this.ReturnChannel(channel);

                if (this.InvokeRequired == false)
                    EnableControls(true);
                // MainForm.ShowProgress(false);
            }
            return;
        ERROR1:
            if (this.InvokeRequired == false)
                MessageBox.Show(this, strError);
            else
                stop.SetMessage(strError);
        }

        #endregion
    }
}
